# Payment session state implementation plan

Next steps live in [PAYMENT_SESSION_STATE_PROGRESS.md](PAYMENT_SESSION_STATE_PROGRESS.md) → `## Next Steps`.

- Roadmap: [STRIPE_RELIABILITY_ROADMAP.md](STRIPE_RELIABILITY_ROADMAP.md)
- Roadmap item: `payments/payment-session-state`

## Outcome

Add the Payment-owned durable operation and attempt state that reserves a logical client session before
Stripe is called, creates or replays exactly one PaymentIntent or SetupIntent for each attempt, records
normalized provider truth, and lets trusted backend consumers read the smallest provider-neutral snapshot
they need. The implementation builds directly on the provider contract shipped in PR #597 and preserves
Payment's adapter boundary: consumer services own business workflows, and SPAs continue to call their
owning Customer or B2B API rather than Payment.

The implementation is an additive Payment producer change. Existing Customer and B2B RPCs and their
current Stripe flows remain compatible until their later roadmap migrations adopt the new operation
surface.

## Reconciled baseline

- The implementation dependency is terminal: PR #597 merged as
  `bfbfd863c02399bd77b499428465d1fc3585f119`, published Payment Contracts and Client in platform
  `0.1.0-alpha.0.1061`, and platform-sync PR #645 merged as
  `ab6d560c11fbf0b015cce00d8489e5da132acd9f`.
- This plan is reconciled to `origin/main` commit `69df07b8b1ff36e98e82a0c6938b7bb849ee4383`.
  The current service package pin has advanced to `0.1.0-alpha.0.1108`; `.1061` is the shipped
  dependency evidence, not a pin to restore.
- PR #597 added provider-neutral identities, session kinds, normalized states, terminal/retry/failure
  dispositions, `PaymentSessionDescriptor`, `PaymentOperationSnapshot`, the
  `PaymentOperationStateChanged` event, additive protobuf messages, fail-closed Stripe normalizers,
  transition/retry/expiry evaluators, package compatibility tests, and the provider call-site inventory.
  It intentionally added no persistence, runtime RPC, provider-session service, or consumer migration.
- `PaymentDbContext` currently contains transactions, escrow, refunds, ledger, payout accounts,
  Stripe-event receipts, commission state, and `FinancialOperationEntity`. There is no durable
  PaymentIntent/SetupIntent session state. `FinancialOperationEntity` remains the B2B command receipt
  for capture, deposit, and refund and must not be widened into the session aggregate.
- Current client-session creation is spread across `IStripeAccountClient` and `StripeAccountClient`.
  It creates PaymentIntents or SetupIntents before any local reservation, returns legacy
  `CheckoutSession`, and does not supply a stable session-operation idempotency key. Existing methods
  remain a compatibility island while the new additive path is introduced.
- The published `PaymentSessionDescriptor` and `PaymentOperationSnapshot` already contain the intended
  provider-neutral response vocabulary, but no public client interface or protobuf service exposes
  create, retry, or status-read operations yet.
- The checked-in compatibility baseline remains `0.1.0-alpha.0.1009`. It is a lower-bound additive
  guard and must not be regenerated to make candidate changes pass.

## Scope and ownership

### In scope

- A distinct Payment-owned session-operation aggregate and attempt history for PaymentIntent and
  SetupIntent sessions.
- Canonical immutable request fingerprints, race-safe reservation/replay, deterministic Stripe
  idempotency identities, immutable provider binding, and explicit retry revisions.
- A provider-session adapter that creates, retrieves, and where legal cancels PaymentIntents and
  SetupIntents without leaking Stripe types outside Payment infrastructure.
- Persisting normalized provider state through the shipped PR #597 normalizer and transition evaluator.
- An additive backend-only gRPC/Client surface for create, explicit retry, and status read, using the
  already-published descriptor/snapshot/error vocabulary.
- Focused domain, persistence, adapter, gRPC, compatibility, and provider-contract inventory tests,
  including deterministic crash-window and concurrency cases.
- Re-scaffolding Payment's initial migration through `api/initial-migrations.ps1`.
- Payment producer publication and the generated platform-sync lifecycle.

### Out of scope

- Provider reconciliation workers, row claiming/backoff policy, operational recovery endpoints, and
  complete PaymentIntent, SetupIntent, or Refund webhook coverage.
- Routing existing webhook handlers, eager financial operations, or scheduled sweeps through a common
  reconciliation publisher; that belongs to `payments/provider-reconciliation`.
- Customer `TicketPurchaseAttempt`, inventory/price/fulfillment policy, Customer status endpoints, or
  any Customer consumer adoption.
- Frontend orchestration, TanStack Query state, SignalR invalidation, Stripe web/mobile adapters, and
  web, mobile, or B2B migrations.
- Removal of PR #581's tactical bridge, secret parsing, legacy `CheckoutSession`, latest-ten hold
  lookup, or existing session RPCs. Those remain finite compatibility entries for later roadmap items.
- Refund reconciliation and the corresponding Payment technical-debt removal.

## Detailed design

### Durable aggregate

Add two Payment-owned tables rather than a nullable universal provider-operation table:

1. `PaymentSessionOperationEntity` owns one caller-generated `OperationId` and the immutable logical
   request. It stores the opaque operation type and consumer correlation, provider-neutral session
   kind, opaque payer/payee owner keys, amount/currency when applicable, funds-routing choice,
   Payment-resolved provider customer/account bindings, fingerprint algorithm version and SHA-256
   fingerprint, current revision, creation/cancellation timestamps, and an optimistic concurrency
   token.
2. `PaymentSessionAttemptEntity` owns one Payment-generated `AttemptId`, its `OperationId`, monotonic
   revision, predecessor attempt when revised, derived provider object kind, nullable provider object
   ID while `Creating`, normalized `PaymentOperationState`, last raw provider status, safe failure
   code, restricted provider diagnostic fields, created/last-attempted/last-observed/next-reconcile/
   terminal timestamps, expiry and capture deadline, last provider event evidence, and its own
   optimistic concurrency token.

Database constraints enforce the durable invariants:

- `OperationId` is the operation primary key; `AttemptId` is the attempt primary key.
- `(OperationId, Revision)` is unique and revisions start at one.
- `(ProviderObjectKind, ProviderObjectId)` is unique when the provider ID is present.
- Provider object kind is fixed from `PaymentSessionKind`; provider ID is write-once and may be null
  only while the attempt is `Creating`.
- The operation's current revision advances in the same local transaction that reserves the next
  attempt. The attempt history is retained; a revision never mutates an earlier provider binding.
- Row-version conflicts reload canonical state and re-evaluate rather than blindly retrying a save.

The entities expose behavior for reservation, immutable-match validation, provider binding, normalized
transition application, explicit revision, and terminal protection. Infrastructure maps a persisted
attempt to the shipped `PaymentProviderAttempt` policy record; it does not duplicate the PR #597
transition table in services or repositories.

### Immutable request and fingerprint

Define one versioned canonical session specification. It contains only Payment vocabulary and the exact
inputs required to reproduce a Stripe create call:

- `OperationId`, `PaymentSessionKind`, opaque operation type, and opaque consumer correlation;
- payer owner and optional payee owner;
- integer minor-unit amount and ISO currency when the session moves or authorizes money;
- the provider-neutral funds-routing choice;
- capture and customer-presence modes derived from the session kind;
- Payment-resolved Stripe customer and connected-account identities.

The create boundary validates the session-kind matrix before reserving: Payment and Authorization require
the applicable money/payee inputs; setup and verification reject money-movement inputs; no consumer may
supply a Stripe object ID or idempotency key. Canonical encoding uses invariant field order, explicit null
markers, normalized currency/enums, and a persisted version. Presentation metadata and secrets are not
hashed or persisted. Stripe metadata is rebuilt from the persisted opaque type/correlation plus
Payment-owned operation, attempt, revision, and session-kind keys, so an ambiguous provider retry sends
the same create parameters.

Same `OperationId` plus the same computed fingerprint is replay. Same ID plus a different fingerprint
returns `OperationConflict` before any provider call. A changed amount, currency, owner, routing choice,
or session kind requires a new caller-owned operation ID.

### Reserve, create, bind, and replay

The create path is a two-transaction/provider-call sequence:

1. Resolve Payment-owned provider bindings and compute the canonical fingerprint.
2. Insert the operation and revision-one attempt in `Creating`, then commit before calling Stripe.
   Concurrent inserts rely on the operation primary key; the loser detaches the failed graph, reloads,
   and either replays the matching reservation or returns `OperationConflict`.
3. Create the derived PaymentIntent or SetupIntent with the deterministic key
   `payment-session:{OperationId}:{AttemptId}:{Revision}:create` and only persisted request inputs.
4. Bind the returned provider ID exactly once, normalize the returned provider snapshot through the
   PR #597 evaluators, persist the transition under optimistic concurrency, and commit.
5. Create any short-lived Stripe CustomerSession needed for the client response after durable provider
   binding. Return `PaymentSessionDescriptor`; never store or log either secret.

A replay of a bound attempt retrieves the same provider object, creates a fresh CustomerSession where
required, and returns a descriptor with explicit identity and kind. A replay of an unbound `Creating`
attempt repeats step 3 with the same key. Stripe's idempotency result therefore repairs the ambiguous
success window and binds the original object rather than creating another one.

The focused fake provider records objects by idempotency key and exposes one-shot fault points. Tests must
prove convergence after failure before reservation commit, after reservation commit, after provider
acceptance but before binding, and after binding but before response. Parallel same-request calls converge
to one operation/attempt/provider object; conflicting calls never reach the provider.

### Provider refresh and normalized status

Introduce an internal `IStripeSessionClient`/provider-neutral result seam for PaymentIntent and SetupIntent
create, retrieve, and legal cancellation. Real and fake implementations return the raw status, observed
time, capture deadline, safe failure classification, provider request diagnostics, and response-only
secrets needed by the application service. Stripe SDK types remain inside infrastructure.

The status path loads the current bound attempt, retrieves current provider truth, normalizes it, applies
the single PR #597 transition evaluator, and persists an applied transition before returning the snapshot.
Duplicate observations are no-ops. Stale, ambiguous, illegal, unknown-version, or unknown-status evidence
does not mutate state and returns a safe `ProviderUnavailable`/reconcile disposition while retaining
restricted diagnostics. An unbound reservation returns durable `Creating`; create replay, not a read RPC,
is the mutation that resumes provider creation.

This phase establishes the reusable refresh service but does not connect it to webhook handlers or add a
scheduled worker. It also does not publish new semantic outcomes; `payments/provider-reconciliation` owns
ordering-safe outcome publication across eager, webhook, and sweep triggers.

### Explicit revisions

An explicit retry request identifies the operation and expected current attempt/revision; it never parses
or submits a client secret. The service reloads the stored immutable request and uses
`PaymentOperationRetryEvaluator`:

- transport retry, timeout recovery, status read, and webhook redelivery remain on the current attempt;
- only an explicit retry of a policy-eligible terminal failure or provider-confirmed expiry reserves the
  next attempt;
- the operation row advances once under optimistic concurrency, creates a Payment-generated `AttemptId`,
  records the predecessor, and commits the new `Creating` attempt before its provider call;
- a duplicate retry against the same predecessor replays the already-created successor, while a stale
  retry against any other revision conflicts safely;
- a still-existing predecessor provider object is canceled only after retrieval confirms cancellation is
  legal. Cancellation failure leaves the operation on the predecessor and creates no successor.

Every revision receives its own create idempotency key and provider object. Earlier attempt bindings remain
immutable and queryable internally; the published status returns only the current snapshot.

### Additive producer surface

Add one backend-only protobuf service and one `Concertable.Payment.Client` interface for:

- create/replay from a caller-owned operation specification, returning the existing
  `PaymentSessionDescriptor`;
- explicit retry from an expected attempt/revision, returning the descriptor for the canonical current
  attempt;
- status read by operation identity and opaque owner scope, returning the existing
  `PaymentOperationSnapshot`.

Use the existing protobuf descriptor/snapshot messages and `PaymentOperationError` mapping. New request
messages and service methods use new field numbers; existing RPCs, messages, constructors, enum numeric
values, message URNs, and package APIs remain untouched. The status response contains no provider object
ID, metadata, owner, raw provider status, restricted diagnostic, or secret. All three methods require the
existing service-token policy; owner scope is checked against the persisted immutable binding.

Update the provider-contract inventory for every new Stripe create/retrieve/cancel call and backend client
call. Keep the frozen package baseline unchanged, extend candidate contract/protobuf/mapping tests, and
prove the published assemblies remain free of Stripe and consumer runtime references.

## Delivery and lifecycle

All implementation phases land in one Payment producer PR. No Customer or B2B consumer change belongs in
that PR or may land against source-only types. After the producer PR merges:

1. verify the publish workflow produced the new `Concertable.Payment.Contracts` and
   `Concertable.Payment.Client` version;
2. follow the generated `chore/platform-sync-*` PR until its exact package pin is green and merged;
3. only then unblock `payments/customer-ticket-attempt` or any B2B consumer adoption;
4. in a fresh close-out worktree, record terminal publication/sync evidence, tick only the
   `payments/payment-session-state` roadmap row, keep the roadmap itself, and remove this plan and ledger
   under the repository's plan close-out procedure.

The roadmap item remains unchecked throughout implementation, review, producer merge, package publication,
and a pending or failed platform-sync PR.

## Phases

### Phase 1 - Persist the operation and attempt aggregate ✅ DONE (2026-08-20)

- Add the aggregate entities, fingerprint specification/canonicalizer, repository contracts and EF
  implementations, configurations, schema constants, and `PaymentDbContext` sets.
- Implement race-safe initial reservation/replay/conflict and explicit next-attempt reservation as domain
  and repository behavior, without invoking Stripe or exposing a runtime API.
- Re-scaffold the repository's initial migrations by running `./initial-migrations.ps1` from `api/` and
  inspect the generated Payment schema for the required keys, filtered indexes, foreign key, and row
  versions.
- Add focused domain and Payment integration tests for validation, fingerprint stability/versioning,
  duplicate reservation, fingerprint conflict, revision monotonicity, provider-binding uniqueness, and
  optimistic concurrency.

**Green gate:** the migration script succeeds; the Payment migration diff is the expected re-scaffold;
focused domain/integration tests and the smallest affected Payment builds pass; provider-contract and
published compatibility tests remain green.

### Phase 2 - Implement durable provider execution and status refresh ✅ DONE (2026-08-21)

- Add the provider-neutral session adapter, deterministic idempotency-key builder, PaymentIntent and
  SetupIntent option mapping, provider retrieval/cancellation, and real/fake implementations.
- Implement reserve-before-create, bind-and-normalize, bound/unbound replay, response-only secret handling,
  current-provider refresh, and explicit retry/revision orchestration.
- Reuse the shipped normalizers/evaluators and persist their normalized projection; do not create a second
  transition policy or add workers/webhook migrations.
- Add deterministic fault injection and focused tests for every crash window, simultaneous replay/conflict,
  provider-object uniqueness, secret non-persistence, safe unknown-status handling, legal cancellation,
  and one-provider-object convergence.

**Green gate:** focused provider/domain/integration tests pass, the provider inventory is current, and the
smallest affected Payment Web/Infrastructure/UnitTests projects build with zero errors.

### Phase 3 - Publish the backend session-operation API ✅ DONE (2026-08-21)

- Add the request contracts, protobuf service/methods, server implementation, Client interface/adapter,
  mapping and DI/routing needed for create/replay, explicit retry, and status read.
- Enforce service-token authentication, opaque owner scoping, exhaustive error mapping, additive protobuf
  numbering, and the minimal secret-free status shape.
- Add focused gRPC integration tests and extend contract, mapper, frozen-package compatibility, public API,
  message-URN, protobuf descriptor, and provider inventory coverage. Keep every legacy RPC operational.
- Reconcile current `origin/main`, then run the required local generator/invariant gates, smallest affected
  builds, and focused tests under `docs/REMOTE_VALIDATION.md`.

**Green gate:** all focused Payment tests and compatibility guards pass, the branch is green against current
`origin/main`, and the candidate is ready for the repository's review workflow. Full solution and complete
integration matrices remain exact-head draft-PR CI gates; E2E is selected only by the merge queue.

### Phase 4 - Deliver, publish, sync, and close out

- Run `/review`, resolve every finding, open/update the Payment producer PR, and require exact-head CI green.
- Merge only through `/merge`; verify package publication and own the generated platform-sync PR through a
  green merge, fixing any package-consumer break in that sync PR.
- Do not land Customer/B2B consumers before the published/synced version is available.
- Reconcile the ledger from fresh `origin/main`, tick the roadmap item only after the producer,
  publication, and sync gates are terminal, then remove the plan and ledger in the following docs close-out
  change. Never delete `STRIPE_RELIABILITY_ROADMAP.md`.

**Green gate:** producer PR merged, packages published, generated platform-sync PR green and merged, the
roadmap row is checked with durable evidence, and the plan artifacts are closed under the documented
lifecycle.
