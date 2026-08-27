# Stripe payment reliability roadmap

> **Roadmap** for making every Stripe-backed workflow durable, idempotent, observable, and reusable
> across Customer web, Customer mobile, and both B2B web applications. This is the living epic
> tracker, not an implementation plan. Each buildable item spins off its own `_PLAN.md` and
> `_PROGRESS.md`; the roadmap tier is the `plans` skill.
>
> **Goal:** a browser, mobile process, SignalR connection, webhook, service process, or provider call
> may be delayed, duplicated, reordered, disconnected, or restarted without losing the operation,
> issuing it twice, or leaving the user with a false result.
>
> **Scope:** Payment's Stripe adapter and published contracts; Customer ticket-purchase fulfillment;
> B2B setup, authorization, capture, deposit, and refund workflows; shared React/TanStack Query
> orchestration; Stripe web and mobile adapters; reconciliation, observability, and adversarial tests.

---

## How to continue this roadmap

Select the next unowned, implementable item with:

```text
$continue-roadmap @plans/payments/STRIPE_RELIABILITY_ROADMAP.md
```

The selected item must get a fresh plan and progress ledger in this folder before implementation.
The selector must verify current branches, worktrees, PRs, package publication, platform sync, and the
actual `origin/main` baseline. It must not infer current delivery state from this roadmap alone.

## Status

### Shipped foundations - preserve and build on them

- [x] Payment is an agnostic provider adapter. It accepts opaque consumer correlation and publishes
  provider outcomes; it does not learn Ticket, Concert, Application, or Booking business rules.
- [x] Payment webhook ingress verifies Stripe signatures, records Stripe event IDs, and hands work to
  the durable inbox/outbox pipeline.
- [x] The B2B financial saga producer shipped in PR
  [#544](https://github.com/Concertable/concertable/pull/544): caller-owned operation IDs,
  `FinancialOperationEntity`, request fingerprints, durable pending commands, replay, typed capture,
  deposit, and refund outcomes. This is the command-journal foundation, not work to recreate.
- [x] Web and mobile already use Stripe's supported UI SDKs: Payment Element on web and PaymentSheet
  on mobile. The refactor keeps provider UI behind small adapters instead of replacing it with custom
  card collection.

### Tactical bridge - unblock the current PR, then retire it

- [ ] PR [#581](https://github.com/Concertable/concertable/pull/581) addresses the immediate Customer
  3DS race by subscribing before confirmation and correlating success/failure notifications with the
  Stripe transaction ID. It is an appropriate incident bridge, but not the target architecture:
  SignalR is still authoritative, state is not reload-safe, web and mobile differ, and the client
  learns a correlation ID from a client-secret representation. The closeout item below removes the
  bridge after durable status reads have replaced it.

### Epic items

| Status | Key | Item | Depends on |
|---|---|---|---|
| [x] | `payments/provider-contract-baseline` | Lock Stripe product choices, operation vocabulary, transition tables, package contracts, and executable architecture tests | PR #597 merged; platform `0.1.0-alpha.0.1061`; sync PR #645 merged |
| [x] | `payments/payment-session-state` | Persist and idempotently create/reuse PaymentIntent and SetupIntent sessions; publish an agnostic status-read contract | PR #721 merged; Payment `0.1.0-alpha.0.1195`; sync PR #794 merged |
| [ ] | `payments/provider-reconciliation` | Complete webhook coverage and reconcile stale PaymentIntent, SetupIntent, and Refund state | payment session state; PR #544 foundation |
| [ ] | `payments/customer-ticket-attempt` | Add the Customer-owned durable ticket-purchase attempt and fulfillment status API | published/synced Payment session contracts |
| [ ] | `payments/frontend-orchestration-core` | Add reusable headless TanStack Query payment orchestration and optional invalidation adapters | provider contract baseline; consumer status shape locked |
| [ ] | `payments/customer-web-checkout` | Extract the Stripe web adapter and migrate Customer web to durable ticket-attempt state | customer ticket attempt; frontend core |
| [ ] | `payments/customer-mobile-checkout` | Migrate PaymentSheet to the same durable Customer flow and prove web/mobile parity | customer ticket attempt; frontend core |
| [ ] | `payments/b2b-payment-workflows` | Migrate B2B setup/hold/accept flows to B2B-owned durable workflow reads and shared orchestration | provider reconciliation; frontend core; active B2B consumer gates |
| [ ] | `payments/reliability-closeout` | Add adversarial E2E controls, production diagnostics, remove legacy event/timer flows, and close resolved debt | every migration item |

No implementation item may silently broaden itself across these owners. If a required published
contract changes, its producer item lands and publishes first; the consumer waits for platform sync.

## Why this epic exists

Two failures exposed the same architectural weakness from different directions:

1. **Customer 3DS failure:** Stripe confirmation can return without an immediate browser error and
   finish asynchronously. The UI then waits for a transient SignalR message. If that message arrives
   before subscription, after disconnect, or outside a short browser timeout, the UI cannot discover
   the real result. The failing E2E waited for `payment-error`; the server-side failure existed, but
   no durable Customer state could answer the browser.
2. **B2B refund/settlement timeout:** a multi-step provider operation can commit locally, execute at
   Stripe, and crash or delay before its final local transition. A request retry or short E2E poll is
   not reconciliation. The shipped B2B command journal prevents duplicate command execution, but
   stale provider-resource projections and refund reservations still require an authoritative sweep.

These were timing-dependent passes, not proof the old design was sound. Faster webhooks and intact
SignalR connections made the happy path look synchronous. The permanent fix is durable state that can
be read again, plus idempotent execution and reconciliation at the service that owns each fact.

## External architectural basis

- Stripe's [PaymentIntents lifecycle guidance](https://docs.stripe.com/payments/payment-intents)
  says one PaymentIntent should map to one cart or customer session, should be reused after an
  interrupted attempt, and should be created with an idempotency key. The server must monitor
  webhooks after client confirmation.
- Stripe's [webhook guidance](https://docs.stripe.com/webhooks) states that event ordering is not
  guaranteed and duplicate events can be delivered. Endpoint event IDs must be deduplicated and
  handlers must return success quickly before slow processing.
- Stripe's [refund guidance](https://docs.stripe.com/refunds) exposes created, updated, and failed
  refund events. Refund acceptance is not final settlement; failures require durable recovery and
  operator visibility.
- The [Stripe recommendations reference](https://github.com/t3dotgg/stripe-recommendations) correctly
  identifies the split-brain problem between Stripe and local data and recommends one provider-sync
  operation used by both eager success handling and webhooks. Concertable adopts that invariant, not
  its application-specific subscription/KV implementation.

Stripe currently recommends Checkout Sessions with Payment Element for most new integrations.
Concertable also needs manual capture, Connect destinations, setup/off-session consent, later capture,
and B2B financial sagas. Item 0 records an explicit per-flow decision matrix. No implementation may
perform a fashionable API migration that fragments the common reliability model.

## Decisions locked by this roadmap

### 1. There are three distinct truths

| Owner | Durable fact | Examples | Who reads it |
|---|---|---|---|
| Payment | Provider-resource state | PaymentIntent processing/succeeded/requires-capture; SetupIntent status; Refund pending/succeeded/failed | Payment workers and service clients |
| Consumer service | Business-workflow state | tickets fulfilled; application accepted; booking escrow refunded | Its own API and UI |
| Client | Local interaction state only | form validating, Stripe SDK open, request in flight | Current component tree |

Provider success does not itself mean tickets exist or a booking transition completed. The UI reads
the consumer workflow status. The consumer uses Payment outcomes/status to advance that workflow.
The client never upgrades a provider SDK response or a SignalR event into business success by itself.

### 2. Durable operation identities originate before Stripe

- Every logical operation has a caller-visible UUID created before the first remote call.
- The owning backend binds the UUID to the authenticated owner, workflow inputs, and an immutable
  request fingerprint. Reuse with different inputs is rejected.
- Payment uses that operation ID to select or create the provider resource and to construct Stripe's
  idempotency key. The Stripe object ID is a result, never the primary correlation contract.
- Client secrets are credentials used only by Stripe SDK adapters. No client derives IDs by splitting
  or parsing them, and no log records them.
- Changing order-defining inputs creates a new revision/attempt. A retry of unchanged inputs reuses
  the existing attempt and provider resource.

### 3. Reuse is behavioral, not a mega-abstraction

The system keeps three complementary records:

- `FinancialOperationEntity` remains the idempotent command receipt for B2B capture/deposit/refund
  commands and their typed outcomes.
- PaymentIntent/SetupIntent checkout sessions gain a Payment-owned durable session-operation record
  because those provider resources live across requests, redirects, and retries.
- Existing aggregates such as `PaymentRefundEntity`, `EscrowEntity`, and transaction rows retain their
  financial invariants.

They implement a common internal reconciliation contract - stable operation ID, provider reference,
current provider status, last observation, terminality, and reconcile policy - through composed
services and reducers. They do not become one nullable `StripeOperation` table with every possible
foreign key and status.

### 4. SignalR is an acceleration channel only

SignalR may invalidate a TanStack Query key. It may not be the sole record of success or failure and
may not directly transition a checkout component to a terminal business state. Reload, reconnect,
event-before-subscription, event loss, and duplicate delivery must all converge through the same GET.

### 5. TanStack Query owns server state; mutations represent mutations

- Creating or revising checkout state is a mutation, never a permanently cached `useQuery` POST.
- Status is a query with a stable key factory and explicit terminal/nonterminal classification.
- Nonterminal queries refetch at a bounded interval, immediately after Stripe confirmation, and on
  focus/reconnect. Terminal queries stop polling.
- Network failure is not payment failure. Query retry policy distinguishes transport unavailability
  from a durable domain/provider failure.
- React components render the query state. They do not implement webhook timers or event races.

### 6. Provider snapshots, not webhook arrival order, drive state

The webhook event is a prompt to synchronize its provider object. Where Stripe events can be stale or
partial, the handler retrieves the current object and passes the normalized snapshot through the same
pure reducer used by eager sync and scheduled reconciliation. Event ID deduplication reduces work;
transition rules and current-object retrieval guarantee correctness.

### 7. Recoverable provider states remain recoverable

`payment_intent.payment_failed` commonly returns an intent to `requires_payment_method`. That is not
the same as abandoning the business attempt. The durable model records a safe failure reason and lets
the same attempt accept another method while its immutable order inputs still match. Cancellation,
expiry, business invalidation, or fulfilled success are terminal.

## Target backend architecture

```text
Customer/B2B workflow API
    |
    | consumer attempt ID + immutable request fingerprint
    v
Consumer-owned attempt ------------------------- GET status for UI
    |                                                     ^
    | Payment client command + stable operation ID        |
    v                                                     |
Payment session/financial operation                       |
    | idempotency key                                     |
    v                                                     |
Stripe provider object                                    |
    |                                                     |
    +-- eager sync --+                                    |
    +-- webhook -----+--> normalized snapshot reducer ----+--> integration outcome
    +-- stale sweep -+                                          consumer commits fulfillment
```

### Payment-owned session operation

The exact class name is fixed in the item-0 plan after inspecting the then-current schema, but the
published behavior is fixed here. A record contains:

- `OperationId` and immutable request fingerprint;
- opaque operation kind and consumer correlation, with no consumer-domain vocabulary;
- provider object kind (`PaymentIntent` or `SetupIntent`) and Stripe object ID once bound;
- amount/currency and provider-account/customer identity needed to reject incompatible reuse;
- normalized provider state, capture/setup mode, safe failure code, and restricted diagnostic detail;
- created, last attempted, last provider-observed, next reconcile, and terminal timestamps;
- last processed provider event ID/type/time for support correlation;
- an optimistic concurrency token.

Creation is a local reservation followed by an idempotent Stripe create/bind step. Crash windows are
closed as follows:

| Crash point | Recovery |
|---|---|
| before local reservation commit | caller retries same operation ID |
| after reservation, before Stripe call | pending sweep or caller retry executes it |
| after Stripe accepted, before object ID saved | same Stripe idempotency key returns the same object, then binds it |
| after object binding, before response | caller retry returns the existing session operation |
| after confirmation, before webhook | eager status read or stale sweep retrieves Stripe |
| after webhook, before consumer fulfillment | durable outbox retries the integration outcome |

The session response exposes the operation ID explicitly with the client/customer-session secrets.
It never requires a caller to parse a Stripe secret.

### Normalized provider states

Provider-specific values remain available internally, but published status uses explicit typed cases:

| Normalized state | Meaning | Terminal? |
|---|---|---|
| `Creating` | local reservation exists, provider object not yet bound | no |
| `RequiresPaymentMethod` | user can supply or replace a method | no |
| `RequiresConfirmation` | ready for explicit confirmation | no |
| `RequiresAction` | customer authentication/action required | no |
| `Processing` | provider accepted work but has no final result | no |
| `Authorized` | manual-capture funds are available for capture | no for provider operation; may satisfy an authorization workflow |
| `Succeeded` | provider operation completed successfully | yes |
| `Canceled` | provider object cannot complete | yes |
| `Failed` | provider operation has an unrecoverable failure | yes |

SetupIntent and Refund reducers map only states valid for those resource kinds. Item 0 provides an
exhaustive transition table from the Stripe SDK's exact status vocabulary and fails tests when a new
provider status is not handled.

### Consumer-owned workflow attempts

#### Customer Ticket

`TicketPurchaseAttempt` is created before Payment session creation and owns:

- attempt ID, authenticated Customer user, concert, quantity, and frozen price/currency;
- immutable request fingerprint/revision and Payment operation ID;
- business state and the latest normalized payment summary safe for the client;
- safe failure code/message, retry eligibility, expiry/cancellation reason;
- fulfilled ticket IDs and timestamps.

The intended state machine is:

```text
Preparing -> AwaitingPayment -> PaymentProcessing -> Fulfilling -> Fulfilled
              |       ^                |
              v       |                +-> FulfillmentFailed -> Fulfilling
        PaymentMethodRequired
              |
              +-> Canceled / Expired
```

Customer's PaymentSucceeded handler creates tickets, transitions the attempt, records its inbox
receipt, and enqueues any Customer notification in one database transaction. Duplicate outcomes find
the attempt already advanced. PaymentFailed updates durable attempt state before notifying. A status
GET scoped to the current user is the sole authority for customer web/mobile completion.

Inventory and price must be revalidated at the defined reservation/fulfillment boundary. Item 3 must
make the availability policy explicit rather than assuming that a successful delayed payment can
always mint tickets. If inventory cannot be reserved through payment, the plan must specify the
compensating refund path as a first-class state, not an exception log.

#### B2B

B2B continues to own Application, Booking, Concert, and business authorization. Its durable workflow
read models expose whether setup, hold authorization, acceptance, capture, deposit, or refund has
advanced the business workflow. Payment supplies agnostic operation state and typed integration
outcomes. Existing B2B sagas are extended or composed; they are not bypassed by frontend Stripe state.

## Target frontend architecture

### Package ownership

| Package | Owns | Must not own |
|---|---|---|
| `@concertable/shared` | platform-neutral payment attempt types, query orchestration primitives, status classification, query-key helpers/contracts | Stripe React components, Customer/B2B URLs, global SignalR connection |
| `@concertable/customer` | Ticket attempt API, mutation/query options, Customer wrapper hooks and notification invalidation adapter | browser-only Stripe Elements |
| `@concertable/web` | Stripe.js loader, Elements provider, Payment Element presentation, typed confirm adapter | Ticket/Application/Booking behavior |
| `@concertable/mobile` | PaymentSheet adapter with the same typed confirmation boundary | Customer API ownership duplicated from `@concertable/customer` |
| B2B shared package | B2B workflow API/query options and B2B wrappers | Payment provider internals or Customer endpoints |

The current `StripePaymentForm` moves out of `features/concerts`. `NewCardSection`, ticket checkout,
artist application, and venue acceptance compose payment primitives from the correct owning feature;
they do not make the payment infrastructure pretend every flow is a concert checkout.

### Headless orchestration contract

Item 4 chooses final names, but the public shape must provide these responsibilities:

- a typed operation/attempt ID and normalized consumer workflow status;
- query options supplied by the owning product package;
- a confirmation callback supplied by the platform adapter;
- optional event subscription whose only effect is invalidating/refetching the same query;
- derived UI states such as `collecting`, `confirming`, `processing`, `recoverableFailure`,
  `fulfilled`, and `terminalFailure`;
- explicit actions to confirm, retry with a new method, cancel, and refetch;
- no arbitrary external error string threaded through a generic Stripe form.

The generic hook is intentionally consumer-status-oriented. It does not call a Payment service URL
directly from a SPA and does not assume that Stripe `succeeded` equals business fulfillment.

### Web adapter

The web adapter owns only Stripe Elements concerns:

- lazy Stripe.js loading and the existing browser-storage/consent constraints;
- `elements.submit()` validation;
- `confirmPayment` versus `confirmSetup` selected from an explicit session kind, never secret-prefix
  inspection;
- redirect return-state restoration by operation ID;
- typed results: validation/provider error, action accepted/processing, or provider confirmation;
- Stripe-safe display messages, accessibility, and disabled/submitting presentation.

The owning flow immediately refetches durable status after confirmation. Provider errors that occur
before a server-observable state exists remain adapter results; every asynchronous result comes from
the consumer status query.

### Mobile adapter

The PaymentSheet adapter presents and confirms the same explicit session kind and operation ID. It
does not mount a new event listener after `presentPaymentSheet` returns. App background/foreground,
process restart, deep-link return, and network reconnection resume the attempt query by ID.

### SignalR invalidation

Customer and B2B may each translate existing notification payloads into query invalidations:

```text
notification(attemptId) -> validate identity -> invalidate owning attempt key -> GET canonical state
```

Payloads carry the owning attempt/operation ID. Transaction IDs and client secrets are not frontend
correlation contracts. Missed notifications only make polling slightly slower; they cannot change the
result.

## Webhook, eager sync, and reconciliation design

### One synchronization path per provider resource

Each Stripe resource type has one normalizer and one pure reducer. It is invoked by:

1. the eager post-command path when Stripe returns an object;
2. the webhook worker after signature verification and durable enqueue;
3. the scheduled reconciler for stale nonterminal rows;
4. an authenticated support replay/reconcile action if operational recovery requires one.

PaymentIntent coverage includes creation/confirmation states, processing, payment failure, success,
cancellation, and amount-capturable/authorization changes used by manual capture. SetupIntent coverage
includes setup failure, success, and cancellation. Refund coverage includes created, updated, failed,
and succeeded state as exposed by the installed Stripe API version.

The baseline item must compare event allowlists and SDK versions against current official Stripe
documentation. Unknown event types are recorded and ignored safely; unknown statuses fail loudly in
tests and diagnostics rather than being treated as success.

### Duplicate and out-of-order handling

- Stripe event ID remains the first deduplication key.
- The reducer is idempotent for the same provider snapshot.
- Webhook arrival timestamps never define business order.
- For resource types where an event snapshot may be stale, the handler retrieves the current Stripe
  object before reducing.
- Terminality and allowed transitions are explicit. A stale snapshot cannot regress a newer stored
  observation.
- Publishing an integration outcome is keyed to operation ID plus semantic transition, so duplicate
  provider events cannot produce duplicate consumer side effects.

### Reconciliation

A Payment background worker selects nonterminal records whose `NextReconcileAt` is due. Policies use
bounded exponential backoff with jitter, a maximum stale age, and separate treatment for provider
rate limits versus invalid operations. The worker claims rows safely across multiple service replicas,
retrieves provider state, reduces it, and lets the outbox publish newly observed transitions.

The refund slice explicitly resolves the current `PaymentRefundEntity` debt: old `Pending` reservations
are matched to Stripe through stored provider reference/idempotency identity and completed or failed
without issuing another refund. Once verified, the resolved entry is removed from
[`../../api/Concertable.Payment/TECH_DEBT.md`](../../api/Concertable.Payment/TECH_DEBT.md).

Browser timeouts remain presentation choices such as â€œthis is taking longer than expected.â€ They do
not mark an operation failed, trigger a second charge, or substitute for server reconciliation.

## Security and operational visibility

- Never log or persist a client secret outside the response/session boundary already required by
  Stripe SDK initialization. Scrub it from structured request/response logging.
- Authenticate and authorize every consumer attempt read. A guessed UUID cannot expose price,
  failure, customer, or provider information.
- Persist safe public error codes separately from restricted provider diagnostics. UI copy is mapped
  from the public code; raw decline details stay within Payment observability policy.
- Correlate logs and traces with consumer attempt ID, Payment operation ID, Stripe object ID,
  Stripe event ID/type, and message envelope ID.
- Metrics cover nonterminal age, reconcile attempts/results, duplicate webhook count, unknown status,
  integration-outcome lag, fulfillment lag, and terminal failure by safe code.
- Alerts cover stale operations beyond policy, dead-lettered provider events/outcomes, repeated
  reconciliation failure, refund failure, and payment success without consumer fulfillment.
- Provide a read-only support view/query before any manual mutation endpoint. A reconcile/replay
  command, if needed, is idempotent, audited, and uses the same reducer/outbox path.

## Item specifications

### 0. Provider contract baseline

**Key:** `payments/provider-contract-baseline`

This item prevents architecture-by-implementation. It must:

- inventory every PaymentIntent, SetupIntent, capture, deposit, refund, saved-card, and verification
  entry point across Payment, Customer, B2B, web, and mobile;
- write a decision matrix for PaymentIntents versus Checkout Sessions per flow, including Connect,
  manual capture, saved methods, setup/off-session, mobile parity, and migration cost;
- lock operation/attempt identifiers, session kinds, normalized statuses, public errors, request
  fingerprints, terminality, retry/revision rules, expiry, and consumer ownership;
- define additive Payment client/protobuf and integration-event contracts, including package
  publication and consumer compatibility;
- add executable transition/contract tests that make the decisions compile-time visible;
- reconcile the exact `origin/main` schema with PR #544 and #581 rather than copying stale plan state.

**Exit gate:** no unresolved status, ownership, API-product, retry, or compatibility decision remains
for later implementation items.

### 1. Payment session state

**Key:** `payments/payment-session-state`

- Persist the Payment-owned session-operation reservation before Stripe creation.
- Make create/retry idempotent by operation ID and immutable fingerprint.
- Bind and return the Stripe object/session secrets without secret parsing.
- Reuse one provider object for one unchanged logical attempt; explicitly cancel/expire superseded
  revisions where provider semantics allow it.
- Add Payment's internal/provider status read and the smallest additive published contract consumers
  need. Do not expose Payment directly to SPAs.
- Publish packages, merge, and follow the generated platform-sync PR to green before consumers land.

**Exit gate:** every tested crash point converges to one local operation and one Stripe object.

### 2. Provider reconciliation

**Key:** `payments/provider-reconciliation`

- Implement pure normalizers/reducers and complete provider event coverage.
- Route eager results, webhook work, and scheduled sweeps through the same synchronization services.
- Make semantic outcome publication idempotent and ordering-safe.
- Reconcile pending refunds and other stale provider-resource states.
- Add operational metrics, alerts, and controlled recovery primitives.
- Remove the pending-refund tech-debt entry only after crash/recovery tests prove it resolved.

**Exit gate:** delayed, duplicate, reordered, and absent webhooks all converge without a duplicate
provider action or a permanently stranded local row.

### 3. Customer ticket attempt

**Key:** `payments/customer-ticket-attempt`

- Add `TicketPurchaseAttempt` and its migration in the Customer Ticket module.
- Replace checkout-as-query with idempotent attempt creation/revision semantics.
- Freeze price/order inputs, bind the Payment operation, and define inventory reservation or explicit
  post-payment compensation.
- Update Payment outcome consumers atomically with fulfillment, inbox, outbox, and attempt state.
- Add current-user create/read/cancel/retry endpoints and HATEOAS actions where the module convention
  requires them.
- Publish only Customer-owned HTTP DTOs to customer clients; keep Payment internals behind Customer.

**Exit gate:** after any reload or service restart, Customer can answer whether the attempt needs a
method, is processing, fulfilled tickets, failed terminally, expired, or entered compensation.

### 4. Frontend orchestration core

**Key:** `payments/frontend-orchestration-core`

- Add the platform-neutral attempt state vocabulary and derived-state reducer to
  `@concertable/shared/features/payments`.
- Add headless TanStack Query orchestration accepting product-owned query options and a platform-owned
  confirmation adapter.
- Define query keys, refetch policy, cancellation, transport retry, terminal stop, and focus/reconnect
  behavior.
- Define an optional invalidation-source interface; do not import the global HubConnection.
- Test with fake timers and adapters: confirmation-before-event, event-before-subscription, lost and
  duplicate notifications, reload, reconnect, overlapping attempts, and recoverable method failure.

**Exit gate:** a fake consumer can run the entire state machine without React DOM, Stripe, SignalR, or
a Concert-specific type.

### 5. Customer web checkout

**Key:** `payments/customer-web-checkout`

- Extract `StripePaymentForm` into a payment feature and separate Elements presentation from typed
  confirmation behavior.
- Use explicit PaymentIntent/SetupIntent session kinds instead of client-secret prefix inspection.
- Add Customer ticket attempt mutation/query options and wrapper hooks in `@concertable/customer`.
- Migrate ticket checkout to durable Customer status with SignalR invalidation as acceleration.
- Preserve recoverable card retry in the same attempt and clear superseded quantity revisions.
- Cover redirect restoration, accessibility, safe errors, and existing browser storage constraints.

**Exit gate:** Customer web succeeds and fails correctly with SignalR disabled and across a hard
reload during 3DS processing.

### 6. Customer mobile checkout

**Key:** `payments/customer-mobile-checkout`

- Reuse Customer ticket attempt API/query options and the shared orchestration core.
- Adapt PaymentSheet to the typed confirmation contract.
- Persist/resume the active attempt ID through backgrounding, process restart, and deep-link return.
- Remove the post-confirmation signal-only child flow.
- Add platform contract tests and parity scenarios with web.

**Exit gate:** web and mobile observe the same durable states and neither depends on notification
timing.

### 7. B2B payment workflows

**Key:** `payments/b2b-payment-workflows`

- Inventory and migrate artist application setup, venue acceptance/hold, saved-card setup, capture,
  deposit, cancellation, and refund presentation.
- Compose the shipped Payment financial saga rather than creating direct Stripe bypasses.
- Expose B2B-owned workflow status reads for Application/Booking/Concert operations.
- Reuse shared orchestration and the web Stripe adapter while keeping B2B query keys/API in its own
  shared package.
- Define when `Authorized` is business success for a hold and when later capture/refund remains a
  separate durable operation.
- Respect any still-active B2B typed-result/platform-sync owner; do not duplicate its consumer work.

**Exit gate:** B2B pages can recover every operation after reload and show provider progress only as
part of the owning business workflow.

### 8. Reliability closeout

**Key:** `payments/reliability-closeout`

- Add deterministic fake-provider controls for delayed, duplicate, reordered, failed, and omitted
  webhooks plus crash-window simulation.
- Add focused API/integration tests and the smallest necessary UI E2E scenarios for web/mobile/B2B.
- Verify production dashboards, alerts, secret scrubbing, and support correlation.
- Remove `useCheckoutFlow`, SignalR-authoritative success/failure handlers, checkout POST queries,
  secret-prefix/secret-parsing behavior, arbitrary external form errors, and PR #581's tactical bridge.
- Remove resolved Payment tech-debt entries and update Payment/frontend architecture docs.
- Search all Stripe call sites and prove each remote mutation has a stable idempotency identity and a
  durable recovery owner.

**Exit gate:** the legacy anti-pattern search is empty, adversarial tests are green, and every Stripe
operation appears in the ownership/reconciliation inventory.

## Dependency graphs

### Implementation DAG

```text
provider-contract-baseline
    +--> payment-session-state --> provider-reconciliation --> b2b-payment-workflows
    |             |
    |             +--> customer-ticket-attempt --+--> customer-web-checkout
    |                                              +--> customer-mobile-checkout
    |
    +--> frontend-orchestration-core -----------+--> customer-web-checkout
                                                  +--> customer-mobile-checkout
                                                  +--> b2b-payment-workflows

all migration items --> reliability-closeout
```

Once the baseline locks the consumer status contract, frontend orchestration can proceed in parallel
with Payment persistence. Customer web and mobile can proceed in parallel after the Customer attempt
API exists. B2B remains independent of Customer work, but may proceed only after provider reconciliation and frontend orchestration are terminal and its package/consumer gates are clear.

### Delivery DAG

```text
Payment producer PR
    -> publish packages
    -> generated platform-sync PR green/merged
    -> Customer/B2B consumer PRs
    -> frontend package/application PRs
    -> reliability closeout PR
```

Every code PR follows remote-first validation. Full local E2E is not a checkpoint. Focused local
diagnosis occurs only after a remote failure and only through the matching E2E skill with Docker's
data-round-trip health gate.

## Verification strategy

### Pure domain and contract tests

- exhaustive provider-status normalization and transition tables;
- duplicate snapshot, stale snapshot, illegal regression, and unknown status;
- operation-ID/fingerprint reuse and revision conflict;
- consumer workflow transition and public-error mapping;
- frontend derived-state reducer and query refetch/terminal policies.

### Payment integration tests

- crash before/after local reservation, provider call, provider bind, outbox save, and semantic
  outcome publish;
- Stripe idempotency returns the original object after ambiguous provider success;
- duplicate/out-of-order webhook events;
- webhook absent but reconciliation succeeds;
- provider rate limit/backoff and stale-operation alerting;
- pending refund exists at Stripe versus does not exist at Stripe;
- multiple worker replicas claim due reconciliation safely.

### Consumer integration tests

- success outcome atomically fulfills once;
- duplicate outcome cannot duplicate tickets or B2B transitions;
- failure is durable before notification;
- payment success plus inventory/business failure enters explicit compensation/recovery;
- authorization versus capture semantics remain distinct;
- authenticated attempt ownership and information disclosure.

### Frontend tests

- Stripe immediate validation/provider failure;
- async success and failure with SignalR disabled;
- event before subscription, duplicate event, and wrong-attempt event;
- hard reload during `RequiresAction`, `Processing`, and fulfillment;
- browser reconnect and mobile background/process restart;
- quantity/revision change while an earlier intent exists;
- recoverable new-method retry reuses the intended operation;
- simultaneous tabs converge on canonical state.

### E2E

E2E assertions wait on domain outcomes, not arbitrary webhook timing. Test infrastructure gains
deterministic provider controls rather than longer timeouts. The minimum adversarial scenarios are:

1. Customer 3DS failure delivered after notification loss;
2. Customer 3DS success across reload;
3. webhook delivered before the client begins waiting;
4. duplicated and reordered provider events;
5. Payment crash after provider acceptance before local completion;
6. B2B refund left pending and repaired by reconciliation;
7. mobile resume after PaymentSheet returns while fulfillment is still processing.

## Explicit non-goals

- Payment does not own ticket inventory, application acceptance, booking status, UI copy, or consumer
  navigation.
- SPAs do not call Payment directly or query Stripe as the source of business truth.
- SignalR delivery guarantees are not strengthened into a message bus substitute.
- The epic does not introduce Redux or another client state store for server state already handled by
  TanStack Query.
- It does not wrap every Stripe SDK type in speculative generic abstractions. Shared code exists only
  where PaymentIntent, SetupIntent, Refund, web, and mobile genuinely share behavior.
- It does not increase E2E timeouts, retry a failing merge queue, or label a missing durable state as
  flakiness.
- It does not migrate all flows to Checkout Sessions without the item-0 capability decision.

## Epic definition of done

- [ ] Every Stripe mutation has a stable pre-provider operation ID, immutable fingerprint, provider
  idempotency key, durable local owner, and documented recovery path.
- [ ] Every nonterminal provider resource is synchronized by eager response, webhook, and scheduled
  reconciliation through one reducer.
- [ ] Duplicate/out-of-order events and every named crash window are covered by deterministic tests.
- [ ] Customer and B2B expose authenticated durable workflow status; their UIs never infer business
  completion from Stripe or SignalR alone.
- [ ] Customer web and mobile share one Customer API/query layer and one headless orchestration model.
- [ ] Web and mobile Stripe SDK code is isolated behind typed adapters; product components render
  state and invoke actions.
- [ ] Refund failure/pending recovery is durable and observable; the corresponding tech debt is gone.
- [ ] PR #581's bridge and the old checkout event/timer hooks are removed.
- [ ] No client parses or logs client secrets, no generic component accepts an external business-error
  string, and checkout creation is no longer modeled as a query.
- [ ] Payment remains consumer-domain agnostic and every package cutover/platform-sync gate is green.
- [ ] Production metrics, alerts, traces, and support correlation identify stuck work without database
  guesswork.
- [ ] Focused unit/integration/contract checks and remote CI are green; selected merge-queue E2E passes
  without timeout inflation or retrying a genuine failure.


