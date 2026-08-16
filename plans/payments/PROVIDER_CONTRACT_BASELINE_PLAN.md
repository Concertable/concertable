# Provider contract baseline implementation plan

Next steps live in [PROVIDER_CONTRACT_BASELINE_PROGRESS.md](PROVIDER_CONTRACT_BASELINE_PROGRESS.md) → `## Next Steps`.

## Outcome

Replace implicit Stripe semantics with one checked-in, executable provider contract before any durable
session, reconciliation, or shared-frontend work begins. The shipped baseline will make product
selection, identities, session kinds, normalized states, transition legality, retry/revision/expiry
rules, safe errors, ownership, and package compatibility explicit. Payment remains the only service
that knows Stripe; Customer and B2B retain their own workflow truth and communicate through published
Payment contracts only.

This plan starts from `origin/main` commit `836a15a56257a0e35ca5ef5674b39e38eb6767ac` and the published
platform baseline `0.1.0-alpha.0.1009`.

## Current implementation baseline

### Delivery and ownership constraints

- Merged PR #544 at `d6619a85667617fb29b7cbb8ce005b779b39346d` owns the durable B2B
  `Capture`, `Deposit`, and `Refund` financial-operation journal, request fingerprints, Stripe
  idempotency keys, commands, and outcomes. Its `FinancialOperationEntity` is a command receipt, not
  the future provider-session entity.
- Merged PR #581 at `c75890243c44435d707eacf7e51377e4631bcf22` owns the current Customer
  web/mobile 3DS bridge. Its client-secret parsing, SignalR correlation, and 30-second wait remain an
  explicit compatibility island until the later Customer and frontend migration work.
- Merged PR #552 (`Refactor/B2BTypedResultMigration`, consumer head
  `abb6b0035df2b0ecd32836814d166804cc59aa21`, merge commit
  `33f07c47a497586324edacdcfc10321a9d3f02ee`) owns B2B adoption of the existing
  financial-operation commands and outcomes, including its additive `RefundReasonCodes` contract.
  This work must neither edit its consumer flow nor introduce replacement capture/deposit/refund
  messages or refund-reason constants.
- `origin/Refactor/GroupStripeWebhookHandling` at `f42b7a49e` predates the current generic webhook
  dispatch, inbox/outbox processing, and financial-operation work. It is overlapping historical
  evidence only: do not merge or cherry-pick it. Any useful behavior must be re-derived against
  current `origin/main` and covered by the new transition tests.
- No open platform-sync PR was present when this plan was reconciled. Re-check that live gate before
  creating an implementation branch and again before delivery.

### Installed Stripe baseline and version-sensitive assumptions

- `Stripe.net` is centrally pinned to `47.3.0`. Its official `v47.3.0` source pins outgoing API
  requests to `2025-01-27.acacia`. The repository does not override `Stripe-Version`.
- A webhook event's schema is fixed by the Stripe webhook endpoint/account API version, independently
  of the version used for an API request. That live endpoint version is not represented in the
  repository. Phase 1 must record it as deployment evidence and reconcile it with
  `2025-01-27.acacia`; the baseline must not silently deserialize against an assumed version.
- Status strings and event payloads are versioned input. Normalizers must fail closed on an unknown
  value and tests must name the Stripe.net and API-version baseline they exhaust.
- Stripe's documented webhook guarantees are at-least-once delivery with possible duplicates and no
  ordering guarantee. A transition may be accepted only from current provider truth plus the persisted
  revision, never because a webhook arrived later.

Primary sources used to set this baseline:

- [Checkout Sessions and PaymentIntents comparison](https://docs.stripe.com/payments/checkout-sessions-and-payment-intents-comparison)
- [PaymentIntents lifecycle and one-intent-per-session guidance](https://docs.stripe.com/payments/payment-intents)
- [PaymentIntent status vocabulary](https://docs.stripe.com/api/payment_intents/object)
- [SetupIntents and future-use consent](https://docs.stripe.com/payments/setup-intents)
- [SetupIntent status vocabulary](https://docs.stripe.com/api/setup_intents/object)
- [Manual capture and authorization expiry](https://docs.stripe.com/payments/place-a-hold-on-a-payment-method)
- [Saved and off-session payment methods](https://docs.stripe.com/payments/save-and-reuse-cards-only)
- [Webhook ordering, duplication, and version behavior](https://docs.stripe.com/webhooks)
- [Connect charge-model comparison](https://docs.stripe.com/connect/charges?locale=en-GB)
- [Destination charges and refund reversal](https://docs.stripe.com/connect/destination-charges?locale=en-GB)
- [Separate charges and transfers](https://docs.stripe.com/connect/separate-charges-and-transfers?locale=en-GB&platform=web&ui=elements)
- [Refund lifecycle and events](https://docs.stripe.com/refunds?dashboard-or-api=api)
- [Stripe.net v47.3.0 release](https://github.com/stripe/stripe-dotnet/releases/tag/v47.3.0)

## Exact entry-point inventory

The first phase converts this inspected inventory into a maintained architecture table and proves that
no Stripe call site has been missed.

| Owner | Entry point | Stripe object/action | Current consumer behavior |
| --- | --- | --- | --- |
| Payment account client | `ProvisionCustomerAsync` | Customer create | Payment owns the provider customer ID. |
| Payment account client | `ProvisionConnectAccountAsync`, onboarding link, account status | Express connected account and AccountLink | B2B owns onboarding workflow; Payment owns provider calls. |
| Payment payout client | `CreateSetupIntentAsync`, payment-method lookup | SetupIntent and PaymentMethod list | Saves/selects a payout card; no payment occurs. |
| Payment customer client | `CreatePaymentSessionAsync` | automatic-capture PaymentIntent plus CustomerSession | Customer ticket checkout confirms on web/mobile. |
| Payment manager client | `PayAsync` | server-confirmed PaymentIntent | Current saved-card/on-session payment path. |
| Payment manager client | `CreateSetupSessionAsync` | off-session SetupIntent | Venue Hire application saves the artist payment method. |
| Payment manager client | `CreateVerifySessionAsync` | off-session SetupIntent | Door Split and Versus acceptance verifies/saves the venue payment method. |
| Payment manager client | `CreateHoldSessionAsync`, `GetHoldSessionAsync` | manual-capture PaymentIntent | Flat Fee acceptance authorizes funds in the browser. |
| Payment escrow client | deposit | server-confirmed Connect destination PaymentIntent | Venue Hire acceptance charges the saved method off session. |
| Payment escrow client | capture | PaymentIntent capture | Flat Fee acceptance captures the earlier authorization. |
| Payment escrow client | release | Transfer from source transaction | B2B settlement releases funds; not a client session. |
| Payment escrow client | refund | Refund, optionally transfer reversal | B2B cancellation refunds; PR #544/#552 own the durable command boundary. |
| Payment settlement path | finish charge | server-confirmed Connect PaymentIntent | Door Split/Versus settlement charges the saved venue method off session. |
| Payment webhook ingress | `ProcessStripeWebhook` and typed handlers | PaymentIntent/SetupIntent events | Deduplicates Stripe event IDs and writes outbox messages transactionally; only succeeded/failed subsets are handled today. |
| Customer API | ticket checkout and purchase endpoints | Payment client session or server-confirmed payment | No durable ticket attempt exists yet. |
| Customer web | `useTicketPaymentFlow`, `StripePaymentForm`, `handle3ds` | confirm PaymentIntent/SetupIntent and handle next action | Parses `pi_…_secret_…`, subscribes before confirmation, then treats SignalR as authoritative. |
| Customer mobile | ticket checkout screen and PaymentSheet | confirm PaymentIntent | Same tactical transaction-ID derivation and SignalR wait as web. |
| B2B API | Flat Fee/Venue Hire/Door Split/Versus strategy steps | hold, setup, verify, deposit, capture, refund, release, settlement | Deal strategies own business sequencing; Payment remains deal-type agnostic. |
| B2B web | application/acceptance checkout and payout setup | confirm PaymentIntent or SetupIntent | Shared form currently infers intent type from the client-secret prefix. |

There is no frontend path that may call Stripe's server API directly, and no current B2B mobile
payment flow. The inventory test must scan Payment production source for Stripe service calls and all
frontend source for Stripe confirmation/client-secret parsing so future entry points require an
intentional table update.

## Locked provider-product matrix

Stripe recommends Checkout Sessions for many Elements integrations, but these flows need a stable
Concertable operation identity, consumer-owned workflow persistence, custom Connect settlement,
manual capture, and a common web/mobile orchestration contract. The baseline therefore keeps provider
sessions at the lower-level intent APIs. `CheckoutSession` in the current Payment client is a legacy
name and must not determine the target architecture.

| Flow | Selected product | Checkout Sessions decision | Capture/Connect rule |
| --- | --- | --- | --- |
| Customer ticket checkout | PaymentIntent | Do not adopt: the durable Customer attempt and Payment status endpoint must survive browser/app loss independently of a Stripe Checkout Session. | Automatic capture; direct or destination charge with `on_behalf_of` set to the venue connected account so the venue remains seller/settlement merchant and only the platform fee belongs to Concertable. |
| Customer saved-card payment | PaymentIntent | Do not adopt: this is a server-confirmed reuse of an existing method. | Automatic capture; `off_session` only when the customer is genuinely absent and prior consent is recorded. |
| Flat Fee authorization | PaymentIntent | Do not adopt: manual authorization/capture and B2B acceptance are separate durable operations. | `capture_method=manual`; `requires_capture` normalizes to `Authorized`; retain provider `capture_before`; the artist connected account is the settlement merchant/payee. |
| Venue Hire card save | SetupIntent | Checkout setup mode adds no value to the existing custom application workflow. | `usage=off_session`; record consent for later deposit. |
| Door Split/Versus verification | SetupIntent | Checkout setup mode adds no value to the custom acceptance workflow. | `usage=off_session`; successful setup is not a charge. |
| Venue Hire deposit | PaymentIntent | Do not adopt: it is an off-session server charge in the B2B saga. | Charge on behalf of the venue connected account, retain escrow ownership separately, then transfer at release; use explicit idempotency. |
| Door Split/Versus settlement charge | PaymentIntent | Do not adopt: it is an off-session server charge after consumer calculation. | Charge/transfer direction and connected-account settlement merchant follow the typed deal calculation; only the platform fee belongs to Concertable. |
| Payout card setup | SetupIntent | Checkout setup mode is not required for the embedded saved-method flow. | Save only; never represent setup success as money movement. |
| Capture | PaymentIntent capture API | Not applicable. | Capture only an `Authorized` attempt before `capture_before`; never locate it by a latest-10 search in new code. |
| Refund | Refund API against the PaymentIntent/charge | Not applicable. | Track refund status independently; reverse destination transfer/application fee according to the original Connect charge model. |

Checkout Sessions may be reconsidered only for a new flow whose lifecycle is intentionally owned by
Checkout. Doing so requires a baseline revision covering its separate `open`/`complete`/`expired` and
`paid`/`unpaid` status axes; `complete` alone is not payment success.

## Locked vocabulary and ownership

### Identities and immutable binding

- `OperationId` is a caller-generated UUIDv7 created before the first Payment request and persisted by
  the consumer as its opaque correlation key. It identifies one logical provider operation, not a
  booking, ticket order, Stripe object, HTTP request, or SignalR subscription.
- `AttemptId` is a Payment-generated UUIDv7 for one provider object attempt. An operation has one
  current attempt and may acquire a later attempt only through the explicit retry rule below. Stripe
  IDs remain Payment-private provider references.
- The server computes a versioned SHA-256 request fingerprint from the operation kind, monetary amount
  and currency where applicable, capture mode, presence mode, Payment-owned customer/connected-account
  bindings, and opaque consumer ownership key. Presentation metadata and raw provider IDs supplied by
  consumers are excluded.
- Same `OperationId` plus the same fingerprint is a replay and returns the existing operation/current
  attempt. Same `OperationId` plus a different fingerprint is a safe `OperationConflict`; it never
  mutates the existing attempt. A changed amount, currency, owner, charge model, or session kind needs
  a new operation ID.
- Stripe idempotency keys derive from operation and attempt identity plus the specific provider action;
  they are not accepted from consumers.

### Session kinds

The published session kind is a closed, provider-neutral enum:

- `Payment`: a PaymentIntent intended for automatic capture.
- `Authorization`: a manual-capture PaymentIntent whose successful confirmation becomes `Authorized`.
- `PaymentMethodSetup`: a SetupIntent that saves a method for declared future use.
- `PaymentMethodVerification`: a SetupIntent used to authenticate/verify a method without charging it.

Capture, deposit, settlement charge, transfer, and refund are financial operations, not client
sessions. Existing capture/deposit/refund command names and identities remain authoritative; this item
does not create a universal financial-operation enum or a shared mega-table.

### Normalized states and terminality

The closed normalized state enum is `Creating`, `RequiresPaymentMethod`, `RequiresConfirmation`,
`RequiresAction`, `Processing`, `Authorized`, `Succeeded`, `Canceled`, and `Failed`.

- PaymentIntent statuses map one-for-one except `requires_capture` → `Authorized`. A
  `payment_intent.payment_failed` event whose current object is recoverable remains
  `RequiresPaymentMethod` with a safe failure detail; event type does not override current object
  truth.
- SetupIntent uses the same mappings without `Authorized`. Setup success means reusable method setup,
  not payment success.
- Refund `pending` → `Processing`, `requires_action` → `RequiresAction`, `succeeded` → `Succeeded`,
  `canceled` → `Canceled`, and `failed` → `Failed`.
- `Succeeded`, `Canceled`, and `Failed` are terminal for an attempt. `Authorized` is non-terminal and
  expires at Stripe's provider-reported `capture_before`; expiry transitions the attempt to
  `Canceled` with safe reason `Expired`.
- An operation is terminal after `Succeeded` or an explicit consumer cancellation. A terminal failed
  or expired attempt may be revised only when the consumer explicitly retries the unchanged operation;
  Payment creates the next monotonic revision and a new `AttemptId`. Transport retries and duplicate
  webhooks never create a revision.
- Unknown statuses, illegal regressions, stale revisions, and observations whose provider timestamp is
  insufficient to establish order do not mutate state. They produce diagnostics and schedule current-
  object reconciliation in the later reconciliation item.

### Safe errors

Published failures use a closed code plus a short Concertable-authored message. At minimum the
contract distinguishes `PaymentMethodRequired`, `AuthenticationRequired`, `Declined`, `Expired`,
`Canceled`, `OperationConflict`, `ProviderUnavailable`, and `Unknown`. Stripe exception text, decline
details, request IDs, client secrets, and provider object IDs remain internal diagnostics. Result/
protobuf terminals map every known code exhaustively and fail closed on unknown contract values.

### Truth and consumer ownership

- Payment owns provider object creation, provider IDs/secrets, normalization, attempts, current
  provider state, Connect charge mechanics, capture windows, and idempotent Stripe mutations.
- Customer owns ticket-purchase attempt state, inventory/price validation, fulfillment, and the
  customer-facing durable query. B2B owns deal/booking state and its financial saga.
- Frontends receive an explicit session kind and safe operation snapshot; they may accelerate updates
  with SignalR but must recover through their consumer API. They never infer identity or intent type
  from a client secret.
- Integration events carry opaque operation/attempt identities and Payment vocabulary only. Payment
  packages may not reference Customer or B2B runtime/domain assemblies, identifiers, or workflow enums.

## Smallest additive contract surface

Phase 2 adds definitions only where a later durable session implementation needs a compile-time seam:

1. In the Payment client/protobuf package, add `PaymentSessionKind`, `PaymentOperationState`,
   `PaymentOperationIdentity` (`operation_id`, `attempt_id`, `revision`), `PaymentSessionDescriptor`
   (identity, explicit kind, client secret, optional customer-session secret/customer token),
   `PaymentOperationSnapshot` (identity, state, terminal/retry disposition, expiry/capture deadline,
   safe failure), and the closed safe-failure/retry enums.
2. Do not add a new RPC until the durable session item can implement it end to end. The messages and
   client records are the shared wire vocabulary; existing `CheckoutSessionResponse`,
   `PaymentResponse`, `PaymentSessionType`, and every existing RPC remain byte-for-byte compatible.
3. In Payment integration contracts, add one versioned `PaymentOperationStateChangedV1` event carrying
   operation identity, session kind, normalized state, monotonic revision, terminal/retry disposition,
   safe failure, expiry/capture deadline, and observation time. Do not alter or replace the existing
   `PaymentSucceeded`, `PaymentFailed`, or B2B financial-operation messages.
4. Keep raw Stripe types and status strings inside Payment runtime. Published types use Concertable
   enums, `Guid`, integer minor units, ISO currency where needed, and `DateTimeOffset` only. If PR
   #552 lands first, preserve its provider-valid `RefundReasonCodes` values as an existing
   compatibility contract; do not redefine them as session/error vocabulary.

## Implementation DAG

### Phase checklist

- [x] **Phase 1 — durable decision artifact and exhaustive inventory.** Make every existing Stripe
  entry point and baseline decision durable, then guard the inventory in tests.
- [x] **Phase 2 — additive package and protobuf vocabulary.** Publish the smallest provider-neutral
  session/status/event vocabulary without changing existing consumers.
- [x] **Phase 3 — executable transition specification.** Encode every supported Stripe status and
  legal/illegal normalized transition as pure rules with exhaustive tests.
- [ ] **Phase 4 — compatibility and architecture gates.** Prove additive compatibility with
  `0.1.0-alpha.0.1009`, preserve service/package boundaries, and complete remote validation.

### Phase 1 — durable decision artifact and exhaustive inventory

Status: complete. Live-mode evidence found that the production account has no webhook endpoint, so
fixtures and future endpoint creation are locked to `2025-01-27.acacia`; endpoint creation and signing
secret installation are deployment gates rather than implementation blockers.

- Add `api/Concertable.Payment/PROVIDER_CONTRACT.md` as the durable source of truth containing the
  product matrix, operation/attempt model, state tables, legal transitions, terminality, retry,
  revision, expiry, safe-error rules, Connect posture, ownership, and version assumptions above.
- Generate a checked-in Stripe-entry-point inventory from explicit search roots across Payment,
  Customer, B2B, customer web, B2B web, and customer mobile. Pair it with a unit/architecture test that
  fails when a Stripe service call, confirmation call, or client-secret parser is added without an
  inventory decision. Existing PR #581 bridge files are an explicit finite allowlist, not the target
  pattern.
- Verify and record the live webhook endpoint API version. If it differs from
  `2025-01-27.acacia`, document the exact endpoint version and make normalization fixtures target that
  version; do not upgrade the endpoint as an incidental change.
- Reconcile the artifact against current `origin/main`, merged PR #552's exact contracts, and the
  published `0.1.0-alpha.0.1009` package surface. Do not edit B2B consumer code.

Verification gate:

- Inventory generation/check is repeatable and reports zero unclassified entry points.
- Payment unit tests covering the inventory/architecture rule pass.
- The affected Payment project/test carve builds with zero errors under the remote-validation policy.

### Phase 2 — additive package and protobuf vocabulary

Status: complete. The additive Contracts, Client, and protobuf vocabulary is locally green without
runtime, RPC, persistence, webhook, or consumer wiring.

- Add the client/protobuf records and enums listed under “Smallest additive contract surface”. Assign
  explicit non-zero protobuf enum values and append new field numbers; reserve nothing already shipped.
- Add `PaymentOperationStateChangedV1` in Payment.Contracts with a stable message URN and no consumer-
  domain or Stripe dependency.
- Extend existing error/result mappers with exhaustive handling for the new safe public codes while
  preserving the existing contracts used by `0.1.0-alpha.0.1009` and PR #552.
- Do not wire the new messages into runtime RPC endpoints, persistence, webhooks, consumers, or
  frontends in this phase.

Verification gate:

- Payment Contracts, Protos, Client, runtime, and unit-test projects build with zero errors.
- Contract tests prove stable URNs, enum values, protobuf numbers/types, no provider-type leakage, and
  no Customer/B2B runtime references.
- A candidate package built from the branch passes the committed compatibility baseline described in
  Phase 4.

### Phase 3 — executable transition specification

Status: complete. The pure Domain specification and exhaustive tests cover the pinned provider
vocabularies, every same-revision state pair, identity/freshness rejection, retry/revision, explicit
cancellation, terminal protection, and provider-confirmed authorization expiry without runtime wiring.

- Implement pure, side-effect-free transition specifications for PaymentIntent, SetupIntent, and
  Refund observations. The specification accepts the current persisted revision plus a versioned
  provider observation and returns an allowed transition, a duplicate/no-op, or a typed rejection.
- Encode the complete allowed-edge tables for every normalized state, including authentication loops,
  `requires_capture`, processing, duplicate delivery, stale/out-of-order delivery, explicit
  cancellation, terminal-state protection, revision creation, and capture expiry.
- Add exhaustive theory tests over every state pair and every Stripe status supported by Stripe.net
  `47.3.0`; unknown values must be tested as failures. Add fixtures for duplicate and out-of-order
  webhook sequences without implementing full webhook processing or reconciliation.
- Keep the specification independent of EF, MassTransit, gRPC, Stripe services, timers, and consumer
  domains so the later session entity and workers must call the same rules rather than reimplementing
  them.

Verification gate:

- Exhaustive state-pair, normalizer, terminality, retry, revision, expiry, and safe-error tests pass.
- Mutation/coverage evidence demonstrates that removing an allowed or forbidden edge fails a test.
- The Payment unit-test carve and affected project builds are green.

### Phase 4 — compatibility and architecture gates

Status: implementation complete locally; current-main reconciliation and exact-head draft-PR CI remain.

- Generate committed golden baselines from published `Concertable.Payment.Contracts`, Protos, and
  Client `0.1.0-alpha.0.1009`: public .NET signatures/message URNs and a protobuf descriptor set.
- Add tests that require the candidate surface to be an additive superset: existing public types,
  constructors/properties used by consumers, message URNs, enum numeric values, protobuf field
  numbers/types/cardinality, RPC names, and request/response types may not change or disappear.
- Add assembly/source architecture tests proving Payment contracts/client/protos do not reference
  Stripe.net or Customer/B2B runtime/domain assemblies, and that Payment runtime does not gain a
  consumer-domain dependency.
- Compile the frozen compatibility fixture and verify the merged PR #552 contracts now present on
  current `origin/main`.
- Run plan-graph validation and the repository's focused local gates; keep draft PR #597 current for
  remote build, service carve, unit, integration, and package validation. Do not run local E2E unless
  a failing remote check requires targeted diagnosis.

Verification gate:

- Compatibility and architecture tests pass against `0.1.0-alpha.0.1009` and the candidate packages.
- Focused local build/tests and draft-PR CI are green at the same commit.
- Once the final implementation candidate and exact-head CI are green, mark PR #597 ready for review
  unmistakably in the ledger, include its URL, and make `/review` the ledger's next action. The
  review-ready checkpoint still hands off with the standard continuation pointer; it is not terminal.
- A code review records no open high-confidence findings before merge becomes the ledger's next step.

## Delivery DAG

1. Satisfied: PR #552 merged as `33f07c47a497586324edacdcfc10321a9d3f02ee`; its overlapping
   Payment contract additions are present on this branch through current `origin/main`.
2. Update the implementation branch from current `origin/main`, rerun affected builds/tests, push,
   and let draft-PR CI validate the exact remote head; then review that green candidate. Any later code
   commit requires `/incremental-review` before merge becomes the ledger's next step.
3. Enter the normal code merge queue with the E2E tier selected mechanically from the changed paths.
   No local full E2E run is planned for this contract/architecture item.
4. Follow `publish-packages` and the generated `chore/platform-sync-*` PR through green/merged. Because
   this item changes published Payment packages, the work is not terminal until the new platform pin
   has synchronized every service.
5. Before a production deployment accepts webhooks, create the live endpoint at the actual deployed
   Payment Web URL with API version `2025-01-27.acacia` and the four currently handled event types,
   store its signing secret as `Stripe:WebhookSecret`, and record the returned endpoint evidence.
6. Revalidate the compatibility fixtures and service builds against the published version, tick the
   `payments/provider-contract-baseline` checklist item, then close the plan and ledger through the
   prescribed docs closeout. Keep the roadmap.

## Out of scope

- The durable Payment session entity, repository, and status RPC implementation.
- Reconciliation workers or complete webhook event processing.
- Customer ticket-attempt persistence or durable Customer status endpoints.
- Shared frontend orchestration or SignalR/query recovery changes.
- Customer web/mobile or B2B UI migration.
- Removal or cleanup of PR #581's tactical 3DS bridge.
- B2B adoption work owned by PR #552, or replacement of PR #544's financial-operation saga.
- Provider migration, Checkout Sessions adoption, or incidental Stripe API/webhook endpoint upgrades.

## Definition of done

- Every inspected Stripe entry point is classified by owner, provider product, presence/capture mode,
  Connect model, identity, and current compatibility constraint, with an executable inventory guard.
- The product matrix and all identity/state/transition/retry/revision/expiry/error decisions are durable
  in Payment architecture documentation and executable tests.
- The smallest additive Payment client/protobuf/event vocabulary ships without breaking the published
  `0.1.0-alpha.0.1009` surface or PR #552's consumer ownership.
- Exhaustive transition, contract, architecture, and compatibility tests are green in local focused
  validation and remote CI.
- The implementation PR, package publication, and platform-sync PR are terminal; the checklist item is
  ticked; this plan and ledger are removed while the roadmap remains.
