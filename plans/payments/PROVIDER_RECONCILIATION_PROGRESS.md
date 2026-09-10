# Provider reconciliation progress

- Plan: `plans/payments/PROVIDER_RECONCILIATION_PLAN.md`
- Roadmap: `plans/payments/STRIPE_RELIABILITY_ROADMAP.md`
- Roadmap item: `payments/provider-reconciliation`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-payments_provider-reconciliation-phase2`
- Branch: `Feature/payments_provider-reconciliation-phase2`
- PR: not opened; Phase 1 [PR #831](https://github.com/Concertable/concertable/pull/831) is merged
- Dependency/package gates: Phase 1 is terminal through PR #831, published Payment baseline `0.1.0-alpha.0.1242`, and generated platform-sync [PR #846](https://github.com/Concertable/concertable/pull/846). A changed published `Concertable.*` contract still requires a dedicated producer plan and terminal publication/sync chain.
- Last reconciled: `2026-08-28` after PR #831 merged as `51d8ba3c5dbb9469f49f15bae48f5e6c2881fcb6`, package publication succeeded, and PR #846 merged the generated platform sync.

## Current state

Phase 2 is implemented and focused-green (merged to `origin/main` at platform `0.1.0-alpha.0.1248`); not yet reviewed as a whole or PR'd. Three commits on top of the sync:

1. Webhook reconciliation + publish-once (`f23b1708c`): `WebhookProcessor` verifies/deduplicates the Stripe event, `PaymentSessionResourceReconciler` resolves the tracked attempt by provider object id, retrieves the current PaymentIntent/SetupIntent, and delegates to the reconciliation service with `Source.Webhook` and immutable event evidence. `PaymentSessionAttemptEntity` raises `PaymentOperationStateChangedDomainEvent` only on a committed observable change; the pre-commit handler publishes `PaymentOperationStateChanged` through the durable outbox, so redelivery/reordering/post-eager/stale-payload cannot publish a second outcome. Eager and webhook converge on the same publish. Legacy financial handlers preserved.
2. State-machine + DDD reshape (`6996b3f72`, `b84deeedf`): the session transition graph is the canonical kernel `StateMachine`, edges as data; `PaymentSessionStateMachine.Evaluate(current, observation)` owns the rules (legality/terminal/capture/transition-building); the reconciliation service is the thin transitioner (normalize → freshness → `Evaluate` → apply → save/defer). The bespoke `PaymentOperationTransitionEvaluator`/`StripeOperationTransitionEvaluator`, the validation extensions, and the `Duplicate` disposition are deleted. Retry/expiry evaluators + their `PaymentProviderAttempt` view untouched.
3. Machine inheritance (`071811d95`): now that #851 published the inheritable kernel base (consumed via platform `0.1.0-alpha.0.1248`), `PaymentSessionStateMachine`/`PaymentRefundStateMachine` derive from `StateMachine<,>` instead of wrapping it.

No published `Concertable.*` contract changed; no persistent-model change, so no migration re-scaffold. Local: 564 unit + 9 architecture + all session/webhook/persistence integration tests green (the full 48-test integration run is Docker-resource-flaky locally; remote CI owns the full matrix).

## Next Steps

Reviewed and green. Open the Payment producer PR and take it through the merge queue (remote exact-head CI owns the full E2E/carve matrix). Phase 3 (stale-session/pending-refund sweep worker and Refund webhook routing) and Phase 4 (delivery) remain.

## Completed work

- Authored the implementation plan and ledger, reconciled the Stripe reliability roadmap, and landed the reviewed planning baseline in PR #816.
- Shipped Phase 1 eager session reconciliation and concurrency-safe canonical outcome reporting in PR #831 (`51d8ba3c5dbb9469f49f15bae48f5e6c2881fcb6`).
- Published Payment `0.1.0-alpha.0.1242` and merged its generated platform sync in PR #846 (`caa13a0a05aa3d101b884f93eca05aaa5d7ad37a`).

## Verification

- Merge-group run `33184255084` passed the required build, carve, unit, architecture, integration, API E2E, and UI E2E gates for PR #831.
- Publish run `33187299741` succeeded from the Phase 1 landing commit; generated sync PR #846 passed its complete package-only gate and merged.

## Reviews

Phase 2 review is complete with no open findings through `363c84c8280e170ff5f8eadedafbc92c42676a30` (two passes: webhook slice, and state-machine + DDD reshape). Findings `PAY-REC2-001..007` all resolved or dispositioned; `PAY-REC2-001` (published `PaymentOperationStateChanged.ExpiresAt` always null) is tracked as a pre-existing published-contract field owed to a dedicated producer plan. Canonical artifact: `reviews/Feature-payments_provider-reconciliation-phase2.md`. (Phase 1: `reviews/Feature-payments_provider-reconciliation-phase1.md`.)

## Decisions, discoveries, blockers, and deviations

- Session operations and refunds require separate durable models; neither is widened into a nullable universal Stripe table.
- Webhook payloads are wake-up evidence, not provider truth; every durable state transition retrieves and normalizes the current provider object.
- The existing `PaymentOperationStateChanged` event is the only planned session outcome carrier. Consumer workflow interpretation stays in Customer and B2B.
- Publish-once cannot key off the evaluator's `Applied` disposition: `StripeSessionClient` stamps `ObservedAt = now` at retrieval, so a re-retrieved unchanged object still evaluates as `Applied` (same state, newer observation). The entity therefore raises only when the consumer-observable projection (state, failure code, capture-before) actually changes, ignoring `ObservedAt`.
