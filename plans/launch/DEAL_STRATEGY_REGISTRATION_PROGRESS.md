# Deal-type strategy registration refactor progress

- Plan: `plans/launch/DEAL_STRATEGY_REGISTRATION_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal_strategy_registration`
- Branch: `Refactor/launch_deal_strategy_registration`
- PR: [#451](https://github.com/Concertable/concertable/pull/451) — open; replacement work head
  `76e17b0206b77742fd5d89961bccb1be9b429387` is pushed and verified; checkpoint transport is in
  progress
- Dependency/package gates: no pre-merge package dependency; the generated platform-sync PR must be
  followed to green/merged after this `api/**` PR lands
- Last reconciled: 2026-08-11; current `origin/main`
  `f63e9a1eb` is merged as `662378e69`; branch is 0 commits behind and 48 ahead

## Current state

All five implementation phases are complete. Concert now declares terms, payee direction, payment
projection, settlement calculation, workflows, lifecycle state machines, capabilities, and steps
vertically per `DealType`. Deal owns a separate module-local factory and vertical registration for its
mapper and updater families. Both builders validate exact coverage and lifetime consistency before
emitting keyed registrations, and operation-specific facades remain the business-facing API.

The implementation commits are `506bc35e4` (terms/factory), `4a741fa50` (payee/payment), `0a8320289`
(settlement), `02730b0da` (workflow composition), and `4d4f44e0a` (Deal registration and architecture
guard). The existing review artifact is `reviews/Refactor-launch_deal_strategy_registration.md`; all
recorded reviews are clean with no open finding.

Current main at `f63e9a1eb` is merged into the branch. The reconciled base includes platform version
`0.1.0-alpha.0.922`, Concert cancellation routing, and the worktree-lifecycle automation without
overlapping either strategy factory interface or the module-local registration composition.
Covariance is removed from both module-local interfaces and their documented examples; repository
search finds no remaining variant factory declaration or covariance-dependent assignment.

After the first compound push, `main` advanced with a reviewed worktree-cleanup-only change. The
replacement work-head leg pushed `529a47958..76e17b020`; local HEAD, the remote-tracking ref, and PR
`headRefOid` were verified equal at `76e17b0206b77742fd5d89961bccb1be9b429387`, with 0 commits
behind current `origin/main` and no unrelated dirty or untracked source.

Merge-group run `31486088803` completed successfully, including API and UI E2E. GitHub then
rebuilt the group after an earlier queued PR changed `main`. Replacement run `31486673612` passed API
E2E and reached UI E2E, but its B2B Tenant integration job failed before executing product assertions:
the runner's Docker pull of the SQL Server image from `mcr.microsoft.com` was reset by the network.
All 56 tests failed in fixture startup in 76 ms. This is an external runner/image-pull failure, not a
changed-area test failure. Tommy has now explicitly authorized one fresh merge attempt after removing
unused covariance from both module-local strategy factory interfaces.

## Completed milestones

- Design and all five implementation phases are complete in the commits listed above.
- Deal and Concert unit coverage pins factory resolution, scoped/singleton lifetimes, exact strategy
  coverage, workflow composition, payee direction, settlement values, rendering, serialization, and
  architecture allowlists.
- Complete implementation review covered `43fe1caf4..fb34f37b1`; later incremental reviews through
  remote head `bc05263e7` found no issue.
- PR #451 is open against `main`; replacement build, carve, unit, and integration checks passed at
  `30c459d7`. Full API and UI E2E are intentionally reserved for the merge queue.
- Merge-group run `31486088803` passed its complete matrix, API E2E, and UI E2E against the first queue
  base. Current-base replacement run `31486673612` passed API E2E but hit an MCR connection reset
  while the Tenant integration fixture pulled SQL Server.
- Pre-existing platform-sync PR #488 for `0.1.0-alpha.0.917` passed its checks and merged as
  `130211aa90ae031a31e8b827e2567c3667fbc2b8` before the branch was reconciled.

## Verification

- `dotnet build api/Concertable.slnx --artifacts-path
  C:\Users\TommySeery\AppData\Local\Temp\Concertable\launch-deal-strategy-pr451-invariant` completed
  in 18m27s with 0 errors and 8 existing warnings against current main.
- `Concertable.B2B.Concert.UnitTests` passed 132/132 and `Concertable.B2B.Deal.UnitTests` passed 41/41;
  `scripts/integration.ps1 concert` passed both projects: B2B Concert 144/144 and Customer Concert
  11/11.
- `git diff --check origin/main..HEAD` is clean.
- The net PR diff has no cross-service runtime reference, no keyed-service lookup outside the two
  module-local factories, no new deal-type business branch, and no undeclared strategy family.
- The only security-sensitive net path is deletion of unused public marker
  `Concertable.B2B.Deal.Contracts.IDealStrategy`; repository search finds no consumer or remaining
  reference.
- Failed job `93764269468` reports `DockerApiException` from `SqlFixture.InitializeAsync`: the MCR
  manifest request was reset by peer; 0 of 56 Tenant integration tests reached execution.

## Review state

- Artifact: `reviews/Refactor-launch_deal_strategy_registration.md`.
- Native, security, correctness, microservice-isolation, module-boundary, seeding, C# convention,
  keyed-strategy, and changed-path coverage reviews have no open finding.
- Incremental reviews cover `ddd2ca4ce..662378e69`, including every current-main merge, the invariant
  factory correction, and security-sensitive workflow/browser/tooling paths; they found no issue and
  advance both review watermarks to `662378e69`.

## Decisions and constraints

- Re-select the merge-queue E2E tier mechanically after the final diff is reviewed; do not duplicate
  queue E2E locally.
- Use a short `--artifacts-path` for local SDK work because the deep Windows worktree can exceed path
  limits in ordinary `obj` output.
- After source merge, close this plan-managed feature worktree with `scripts/worktrees.ps1`, create a
  clean `Docs/launch_deal_strategy_registration_closeout` worktree from current `origin/main`, and own
  package publication and generated platform sync to terminal green before docs closeout.

## Next Steps

1. Commit and push this single checkpoint-transport update, then verify local HEAD, the remote-tracking
   ref, and PR `headRefOid` are identical.
2. Wait for terminal green PR checks, select the E2E tier mechanically, and enqueue that verified
   current remote head once.
