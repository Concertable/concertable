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

Phase 2 is implemented in this worktree and focused-green; not yet reviewed or PR'd. Session webhook events are now wake-up evidence: `WebhookProcessor` verifies/deduplicates the Stripe event, then `PaymentSessionResourceReconciler` resolves the tracked attempt by provider object id, retrieves the current PaymentIntent/SetupIntent, and delegates to the Phase 1 reconciliation service with `Source.Webhook` and immutable event evidence. `PaymentSessionAttemptEntity` raises a `PaymentOperationStateChangedDomainEvent` only when the committed observable projection (state/failure/capture) changes; the pre-commit handler publishes `PaymentOperationStateChanged` through the durable outbox, so redelivery, reordering, post-eager delivery, and stale payloads cannot publish a second semantic outcome. Eager and webhook paths now converge on the same publish. Legacy `PaymentSucceededEvent`/`PaymentFailedEvent` financial handlers are preserved. No published `Concertable.*` contract changed; no model change, so no migration re-scaffold.

## Next Steps

Commit the Phase 2 slice, run the code review, address findings, and open the Payment producer PR. Phase 3 (stale-session/pending-refund sweep worker and Refund webhook routing) and Phase 4 (delivery) remain.

## Completed work

- Authored the implementation plan and ledger, reconciled the Stripe reliability roadmap, and landed the reviewed planning baseline in PR #816.
- Shipped Phase 1 eager session reconciliation and concurrency-safe canonical outcome reporting in PR #831 (`51d8ba3c5dbb9469f49f15bae48f5e6c2881fcb6`).
- Published Payment `0.1.0-alpha.0.1242` and merged its generated platform sync in PR #846 (`caa13a0a05aa3d101b884f93eca05aaa5d7ad37a`).

## Verification

- Merge-group run `33184255084` passed the required build, carve, unit, architecture, integration, API E2E, and UI E2E gates for PR #831.
- Publish run `33187299741` succeeded from the Phase 1 landing commit; generated sync PR #846 passed its complete package-only gate and merged.

## Reviews

Phase 1 full and incremental review is complete with finding `PAY-REC-001` resolved and no open findings through `557c6d113d9a7e2554bc56f9c1e32598797d860d`. Canonical artifact: `reviews/Feature-payments_provider-reconciliation-phase1.md`.

## Decisions, discoveries, blockers, and deviations

- Session operations and refunds require separate durable models; neither is widened into a nullable universal Stripe table.
- Webhook payloads are wake-up evidence, not provider truth; every durable state transition retrieves and normalizes the current provider object.
- The existing `PaymentOperationStateChanged` event is the only planned session outcome carrier. Consumer workflow interpretation stays in Customer and B2B.
- Publish-once cannot key off the evaluator's `Applied` disposition: `StripeSessionClient` stamps `ObservedAt = now` at retrieval, so a re-retrieved unchanged object still evaluates as `Applied` (same state, newer observation). The entity therefore raises only when the consumer-observable projection (state, failure code, capture-before) actually changes, ignoring `ObservedAt`.
