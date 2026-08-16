# Provider contract baseline progress

- Plan: `plans/payments/PROVIDER_CONTRACT_BASELINE_PLAN.md`
- Roadmap: `plans/payments/STRIPE_RELIABILITY_ROADMAP.md`
- Roadmap item: `payments/provider-contract-baseline`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\payments_provider-contract-baseline`
- Branch: `Feature/payments_provider-contract-baseline`
- PR: not opened
- Dependency/package gates: implementation is ready; PR #552 remains the external B2B consumer owner and is a delivery-only gate while the baseline stays additive; published platform baseline is `0.1.0-alpha.0.1009`; no open platform-sync PR was present at reconciliation
- Last reconciled: 2026-08-16 against `origin/main` `836a15a56257a0e35ca5ef5674b39e38eb6767ac`, GitHub PR state, the source roadmap, current repository entry points, and official Stripe documentation

## Current state

Planning is complete in this worktree and no production code has been changed. The source roadmap has
been copied unchanged into the worktree, and the implementation plan records the complete current
entry-point inventory, provider-product matrix, operation/attempt model, normalized transition
vocabulary, additive package boundary, external ownership, phased verification, and delivery DAG.
The reviewed work head `0986af4a2b99203a2671cac51d41715d230cdf90` is verified on
`origin/Feature/payments_provider-contract-baseline`.

The implementation work is ready to begin with Phase 1. PR #552 must not be duplicated; its exact head
was `002c45f5fdb83362fff419448dd1c1a8832fd2a3` at reconciliation, including its additive
`RefundReasonCodes` contract. The historical
`Refactor/GroupStripeWebhookHandling` branch is superseded evidence only.

## Next Steps

1. Open the reviewed meta-only GitHub PR from the verified remote head.
2. Apply the `skip-e2e` label and admin-merge the PR through the sanctioned docs path.
3. Confirm the PR is merged, close this plan-managed worktree, and create a fresh implementation
   worktree from current `origin/main`.
4. In that fresh worktree, restore this ledger's next action to Phase 1 — durable decision artifact
   and exhaustive inventory — before beginning implementation.

## Completed work

- Created the clean worktree from current `origin/main` on
  `Feature/payments_provider-contract-baseline`.
- Inspected all requested repository guidance, legal/architecture constraints, Payment/Customer/B2B
  backend entry points, customer/B2B web entry points, customer mobile flow, PRs #544/#581/#552, and
  the historical webhook branch.
- Researched current primary Stripe guidance and verified the installed Stripe.net `47.3.0` source
  pins API version `2025-01-27.acacia`.
- Wrote the implementation plan and copied the source roadmap unchanged into this worktree.
- Extended the plan-graph validator and its focused test to recognize a roadmap status-table row as
  the same stable checklist marker as a CommonMark task-list row, preserving the supplied roadmap
  byte-for-byte (`e9898bda8f431d50e14ee1aed74266d043664caa`).
- Normalized docs-reachability diagnostic paths to repository-style forward slashes so its hook tests
  are portable on Windows.
- Pushed reviewed work head `0986af4a2b99203a2671cac51d41715d230cdf90` and verified the
  remote-tracking ref matches.

## Verification

- Worktree branch was created at and reconciled to `origin/main`
  `836a15a56257a0e35ca5ef5674b39e38eb6767ac` with zero commits behind.
- Source and copied roadmap SHA-256 matched at
  `4181DB21EEF72F29EC4C61536858FE7F5B8ED659ED991C8076C9EB4DE8B2CDB0`.
- GitHub evidence: PR #544 merged at `d6619a85667617fb29b7cbb8ce005b779b39346d`;
  PR #581 merged at `c75890243c44435d707eacf7e51377e4631bcf22`; PR #552 was open,
  mergeable, and externally owned at the exact head recorded above; no open platform-sync PR was found.
- `python .agents/hooks/tests/test_plan_graph.py`: 19 tests passed.
- `python -m unittest discover -s .agents/hooks/tests -p 'test_*.py'`: 62 tests passed.
- `python .agents/hooks/plan_graph.py --root C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\payments_provider-contract-baseline`:
  0 errors, 0 warnings.
- `git diff --check`: passed for the tracked validator changes; the new plan and ledger also passed
  no-index whitespace checks. The unchanged source roadmap retains its existing final blank line.
- Push verification: local work head and
  `origin/Feature/payments_provider-contract-baseline` both resolved to
  `0986af4a2b99203a2671cac51d41715d230cdf90`.

## Reviews

Docs review of `836a15a56257a0e35ca5ef5674b39e38eb6767ac..26ac5b8f33db9f006bffe3380b7d832b4c060c22`
found no issues across accuracy, contradiction, document ownership, concision, dangling references,
and followable instructions. Review artifact:
`reviews/Feature-payments_provider-contract-baseline.md`. No implementation review exists.

## Decisions, discoveries, blockers, and deviations

- Current flows stay on PaymentIntents for money movement and SetupIntents for save/verify; Checkout
  Sessions are not selected for any current flow.
- The future public model separates caller-owned `OperationId` from Payment-owned `AttemptId`.
- Existing capture/deposit/refund saga contracts remain authoritative; no universal financial-operation
  abstraction will replace them.
- Full webhook handling, reconciliation, persistence, frontend migration, and removal of the tactical
  3DS bridge remain with later work.
- The live webhook endpoint API version cannot be inferred from source and is explicit Phase 1 evidence,
  not an assumed fact or a planning blocker.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\payments_provider-contract-baseline
Read @plans/payments/PROVIDER_CONTRACT_BASELINE_PLAN.md and @plans/payments/PROVIDER_CONTRACT_BASELINE_PROGRESS.md and do what its `## Next Steps` says.
```
