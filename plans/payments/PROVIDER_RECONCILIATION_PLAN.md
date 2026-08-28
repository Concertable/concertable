# Provider reconciliation implementation plan

Next steps live in `PROVIDER_RECONCILIATION_PROGRESS.md` under `## Next Steps`.

## Outcome

Make Payment's durable provider projections authoritative when Stripe events are delayed, duplicated, reordered, or absent. Payment will retrieve the current Stripe object and apply one normalized transition path for session operations and refunds, regardless of whether the trigger is an eager call, a webhook, or a scheduled sweep. Semantic state-change events will be emitted once from the durable transition; Customer and B2B remain responsible for interpreting that outcome in their own workflow state.

## Reconciled baseline

- The prerequisite is terminal: [PR #721](https://github.com/Concertable/concertable/pull/721) introduced durable PaymentIntent and SetupIntent session operations, and [PR #794](https://github.com/Concertable/concertable/pull/794) synced the published Payment packages.
- This plan is based on `origin/main` `fe0f9dac14c73027f0c67feb35a932b685530580`.
- `PaymentSessionService` already retrieves a bound provider object for create/replay, refresh, and retry, maps it through `StripeProviderObservationMappers` and `StripeOperationTransitionEvaluator`, and persists `PaymentSessionAttemptEntity` state. That path is request-scoped and does not publish an ordering-safe outcome or claim work for a background sweep.
- `WebhookProcessor` deduplicates Stripe event IDs in `StripeEventEntity`, but existing handlers still dispatch from webhook payload delivery rather than treating the event as a prompt to retrieve current provider truth.
- `PaymentRefundEntity` has independent pending, completed, and failed accounting semantics. It must remain a refund-specific aggregate; it must not be folded into the session-operation tables or `FinancialOperationEntity`.
- The published `PaymentOperationStateChanged` contract is available for session projections. This plan must preserve its compatibility and publish it only from a committed, state-changing transition. New public contracts require their own producer/package delivery gate.

## Scope and ownership

### In scope

- One internal Payment reconciliation service that accepts a persisted provider-resource identity and an observed Stripe snapshot, applies the existing normalized transition rules, persists the result under optimistic concurrency, and makes duplicate or stale observations harmless.
- Retrieval of the current PaymentIntent, SetupIntent, and Refund objects before durable state is advanced; Stripe SDK types remain inside Payment infrastructure.
- Webhook routing that uses event IDs only for deduplication and audit evidence, then invokes the same reconciliation service as eager execution.
- Durable due-work claiming, bounded retry/backoff, stale-operation detection, and observability for nonterminal session attempts and pending refunds.
- Refund observation and recovery that preserve escrow and settlement reservation/accounting invariants, with deterministic tests for ambiguous provider success and missed webhooks.
- Idempotent publication of the existing Payment session state-change event from the committed transition/outbox boundary.
- Focused domain, integration, architecture, and provider-adapter tests; Payment migration re-scaffolding if the persistent model changes.

### Out of scope

- Customer `TicketPurchaseAttempt`, ticket fulfillment, inventory policy, or Customer HTTP status endpoints.
- B2B workflow reads, UI migrations, or direct consumer-specific Stripe behaviour.
- Frontend orchestration, SignalR invalidation, Stripe web/mobile adapters, or removal of the current compatibility islands.
- A shared nullable provider-operation table, consumer-domain fields in Payment, or a Customer-to-B2B runtime dependency.
- General retry inflation, webhook-order assumptions, or treating a Stripe event payload as the final source of truth.

## Design constraints

- Payment is the Stripe adapter. Customer and B2B may synchronously call Payment through its published client surface, but neither receives Stripe provider state as business success and neither is introduced as a runtime dependency of the other.
- A webhook is a wake-up signal. The reconciliation service retrieves the current provider object, normalizes that snapshot, and applies the same reducer used by eager create, refresh, and retry paths.
- Every durable mutation is monotonic and concurrency-safe. A duplicate snapshot is a no-op; a stale or illegal regression is recorded as safe diagnostic/reconcile evidence without overwriting the canonical state.
- The outbox event is emitted only when a committed canonical session transition changes the observable state. Redelivery, retries, competing workers, and repeated retrievals cannot publish duplicate semantic outcomes.
- Refunds retain their own amount reservations, posting rules, and terminal accounting transitions. Provider observation records enough evidence to resume a pending refund without reissuing it.
- Nonterminal work has a due timestamp and bounded attempt policy. Worker claims are persisted and lease-safe, so multiple Payment replicas do not process the same record concurrently.

## Delivery shape

The implementation is a Payment producer change. It may add private persistence and runtime composition, but it must not make consumers compile against source-only changes. A changed published `Concertable.*` contract is out of scope for this plan: author a dedicated producer plan, publish it, and merge its generated platform-sync chain before this plan consumes that terminal baseline.

## Phases

### Phase 1 - Centralize durable provider synchronization (implemented locally)

- Extract the session transition-and-persist portion of `PaymentSessionService` into a Payment-internal reconciliation service with an explicit source (`eager`, `webhook`, or `sweep`) and provider event evidence.
- Keep `IStripeSessionClient` as the PaymentIntent/SetupIntent provider boundary. Its retrieval result supplies the normalized snapshot and restricted diagnostics; Stripe SDK types do not leave Infrastructure.
- Route create/replay, refresh, and retry through the reconciliation service without changing their public gRPC/Client responses.
- Persist a retryable reconciliation requirement when provider retrieval fails or a transition cannot safely apply, rather than returning a false terminal result.
- Add exhaustive unit coverage for duplicate, stale, illegal, terminal-protected, and unknown-status observations; add integration coverage for concurrent reconciliation of one attempt.

**Consumption contract:** internal callers provide one persisted session attempt plus provider observation evidence and receive the canonical persisted snapshot or a retryable Payment error. No consumer service receives a Stripe type or a webhook payload.

**Green gate:** the affected Payment projects build; focused provider-contract, session unit, and integration tests prove eager callers converge through one durable path; any model change has a correctly re-scaffolded Payment initial migration.

### Phase 2 - Reconcile webhook work and publish outcomes once

- Replace payload-authoritative webhook handling with resource routing: verify and deduplicate the Stripe event, retrieve the current PaymentIntent or SetupIntent, then call the Phase 1 service with immutable event evidence.
- Complete supported event coverage and make unsupported object/event combinations auditable no-ops rather than implicit success paths.
- Publish `PaymentOperationStateChanged` through the existing durable outbox only for a committed observable transition; retain event ID/type/time for support correlation without coupling consumers to Stripe vocabulary.
- Preserve legacy webhook/financial handlers while routing their relevant session-operation observations through the shared path. Do not alter Customer or B2B workflow ownership.
- Add deterministic integration tests for duplicate delivery, reordered events, event delivery after an eager transition, and an event payload that is stale relative to the retrieved object.

**Consumption contract:** consumers receive at-least-once, provider-neutral `PaymentOperationStateChanged` messages only after the Payment projection commits. Consumers continue to deduplicate and advance their own workflow state atomically.

**Green gate:** webhook and outbox integration tests show one canonical projection and one semantic outcome for duplicate/reordered triggers; Payment architecture and published-contract compatibility guards remain green.

### Phase 3 - Reconcile stale sessions and pending refunds

- Add persisted due-work selection and lease/claim behaviour for nonterminal session attempts and pending refunds, with bounded retries, next-due calculation, last observation, and operator-safe failure diagnostics.
- Add a Payment-hosted reconciliation worker that claims due records, retrieves current Stripe state, and delegates to the same session/refund reconciliation services. It must be safe under multiple replicas and restart after a lease expires.
- Route supported `refund.created`, `refund.updated`, and `refund.failed` webhook events through current-object Refund retrieval and the same refund reconciliation service; the webhook payload remains evidence rather than state truth.
- Extend the refund-specific persistence and service boundary only as necessary to retain Stripe refund identity, current provider status, observation evidence, and recovery outcome without weakening escrow/settlement reservations or duplicate-refund protection.
- Reconcile ambiguous refund creation before deciding whether to complete or release a reservation; never issue a second refund merely because the original response was lost.
- Emit structured metrics/logs for claimed, completed, deferred, terminal-failed, and overdue work, with correlation by Payment operation/refund identity and no client-secret or raw-provider diagnostic leakage.
- Re-scaffold the Payment initial migration if the new durable fields or indexes change the model.

**Consumption contract:** the worker produces the same committed Payment projection and semantic outcome as a request or webhook trigger. It has no HTTP endpoint and exposes no consumer-domain recovery command.

**Green gate:** focused domain/integration tests prove duplicate claims, lease expiry, restart, absent webhook, ambiguous provider success, pending-refund repair, duplicate/reordered/stale Refund webhooks across created, updated, and failed outcomes, and no duplicate Stripe action; the smallest affected Payment host/worker projects build cleanly.

### Phase 4 - Verify delivery and unblock dependent work

- Run the repository-required generators, provider-inventory/architecture checks, affected builds, and focused Payment tests for the completed phases; remote exact-head CI remains authoritative for the full matrix.
- Obtain a clean code review, then deliver the Payment producer through the normal PR lifecycle.
- If the work exposes a need to change a published `Concertable.*` contract, stop that contract work and author its dedicated producer plan. This implementation may consume only the separate plan's terminal published and synced baseline.
- Reconcile the plan ledger and roadmap only after the producer and any required package/sync gates are terminal. This clears only the provider-reconciliation prerequisite; B2B remains blocked on frontend orchestration and active B2B consumer gates.

**Green gate:** delayed, duplicated, reordered, and absent webhook scenarios converge without a duplicate provider action or stranded local projection; review and exact-head CI are green; all required publication/sync gates are terminal.

## Completion conditions

- Eager calls, webhook handlers, and scheduled sweeps use the same current-object reconciliation path.
- Provider event ordering and duplication cannot regress durable state or publish a second semantic outcome.
- Nonterminal PaymentIntent, SetupIntent, and pending Refund work is claimed, retried, and observable after process restart.
- Refund recovery preserves the existing financial reservation and posting invariants.
- Payment remains consumer-domain agnostic, and downstream consumers stay delivery-gated until a real published baseline exists.
