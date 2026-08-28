# Provider reconciliation progress

- Plan: `plans/payments/PROVIDER_RECONCILIATION_PLAN.md`
- Roadmap: `plans/payments/STRIPE_RELIABILITY_ROADMAP.md`
- Roadmap item: `payments/provider-reconciliation`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-payments_provider-reconciliation-phase1`
- Branch: `Feature/payments_provider-reconciliation-phase1`
- PR: [#831](https://github.com/Concertable/concertable/pull/831) (draft)
- Dependency/package gates: Payment session state is terminal through PR #721 and platform-sync PR #794; no implementation blocker exists. A changed published `Concertable.*` contract requires a dedicated producer plan, publication, and generated-sync chain before this plan may consume its terminal baseline.
- Last reconciled: `2026-08-28` after the exact-head Payment integration failure was reproduced and fixed locally; the focused test and all 43 Payment integration tests pass.

## Current state

Phase 1 is implemented on draft PR #831. Exact-head CI at `08582baab75b9753ef5f959aa395ea663e11e698` exposed an aggregate-tracking defect in concurrent retry recovery: detaching only the losing attempt left its parent operation and private attempt collection tracked, so loading the canonical attempt produced two in-memory objects for the current revision. `PaymentSessionAttemptRepository` now detaches the complete owning aggregate before reloading canonical state. The focused failure and the complete Payment integration project are green; incremental review is required before the repaired candidate is pushed.

## Next Steps

Complete incremental review over the Phase 1 CI repair candidate. Resolve any finding, move the review watermark to the resulting exact head, and push that reviewed head. Monitor its exact-head CI; when all required checks are green, record Phase 1 as ready for human review and report the PR ready. Do not merge or begin Phase 2 in this Phase 1 worktree without explicit instruction.

## Completed work

- Selected `payments/provider-reconciliation` as the next ready, unowned Stripe reliability roadmap item after verifying PR #721 and platform sync PR #794 are terminal.
- Authored the implementation plan and this recovery ledger against the current Payment session, webhook, and refund seams.
- Reconciled the roadmap DAG with its explicit B2B dependency table and aligned Refund webhook coverage with the three supported provider events.
- Landed the reviewed plans-only documentation PR #816.
- Centralized eager session reconciliation and concurrency-safe canonical outcome reporting in `eb7f88d96`.
- Added deterministic concurrency and provider-retrieval failure coverage for the Phase 1 persistence contract in this commit.
- Replaced generic `inner` wrapper collaborator names with role-specific `stripeSessionClient` and `paymentSessionAttemptRepository`, reviewed the naming-only delta, pushed exact head `08582baab75b9753ef5f959aa395ea663e11e698`, and opened draft PR #831.
- Fixed concurrent retry recovery so an optimistic-concurrency loser evicts the complete tracked aggregate before loading canonical attempt state.

## Verification

- `dotnet build api/Concertable.Payment/tests/Concertable.Payment.IntegrationTests/Concertable.Payment.IntegrationTests.csproj --no-restore`: passed with 0 warnings after merging current `origin/main`.
- Focused provider-transition unit tests: 50 passed.
- Integration test execution is deferred to exact-head CI because `scripts/docker-health.ps1` proved the local Docker daemon unavailable before test execution.
- `git diff --check`: passed.
- Naming-correction integration-test project build: passed with 0 warnings.
- `RetryAsync_ConcurrentDuplicateRetries_ConvergeAfterCancellationRace`: passed after reproducing the exact CI failure locally.
- Complete Payment integration project: 43 passed, 0 failed, 0 skipped.

## Reviews

Full code review is complete through `4cd3d1d49d995a9d60c60a41c81e7dc1ce6f91e1`; finding `PAY-REC-001` is resolved. Incremental reviews are approved with no findings through `08582baab75b9753ef5f959aa395ea663e11e698`; the CI repair candidate requires incremental review. Canonical artifact: `reviews/Feature-payments_provider-reconciliation-phase1.md`.

## Decisions, discoveries, blockers, and deviations

- Session operations and refunds require separate durable models; neither is widened into a nullable universal Stripe table.
- Webhook payloads are wake-up evidence, not provider truth; every durable state transition retrieves and normalizes the current provider object.
- The existing `PaymentOperationStateChanged` event is the only planned session outcome carrier. Consumer workflow interpretation stays in Customer and B2B.
