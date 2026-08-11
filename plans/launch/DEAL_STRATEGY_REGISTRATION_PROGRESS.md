# Deal-type strategy registration refactor progress

- Plan: `plans/launch/DEAL_STRATEGY_REGISTRATION_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal_strategy_registration`
- Branch: `Refactor/launch_deal_strategy_registration`
- PR: [#451](https://github.com/Concertable/concertable/pull/451) — open; blocked by a merge-group
  runner failure at verified remote head `30c459d712ab0b5b05c801db79664eca6772f9bf`
- Dependency/package gates: no pre-merge package dependency; the generated platform-sync PR must be
  followed to green/merged after this `api/**` PR lands
- Last reconciled: 2026-08-11; current `origin/main` is merged through
  `ddd2ca4ced246a23969965ff2eacd508956f3b0b`; branch is 0 commits behind and 36 ahead

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

Current main at `48580afedca699a596c01db0c432c4ebb0452027` is merged into the branch. Its only post-build
advance was docs/meta guidance, so the API tree is identical to the tree built successfully from
`bc4d9d174`. The automatic merge seam in Concert's `ServiceCollectionExtensions` retains both current
main's `IApplicationExecutor` registration and this branch's `AddConcertDealStrategies` composition.

The reviewed work range `bc05263e7..7cdf680f0` and its single checkpoint-transport commit are pushed.
Local HEAD, the remote-tracking ref, and PR `headRefOid` were verified at
`30c459d712ab0b5b05c801db79664eca6772f9bf`. Replacement build, carve, unit, and integration checks
are terminal green at that exact remote head. There are no skip trailers or labels, so the default
full API + UI merge-queue E2E tier applies. No unrelated dirty or untracked source is present.

Merge-group run `31486088803` completed successfully, including full API and UI E2E. GitHub then
rebuilt the group after an earlier queued PR changed `main`. Replacement run `31486673612` passed API
E2E and reached UI E2E, but its B2B Tenant integration job failed before executing product assertions:
the runner's Docker pull of the SQL Server image from `mcr.microsoft.com` was reset by the network.
All 56 tests failed in fixture startup in 76 ms. This is an external runner/image-pull failure, not a
changed-area test failure, and the failed queue run must not be retried automatically.

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
  C:\Users\TommySeery\AppData\Local\Temp\Concertable\launch-deal-strategy-pr451-current-main`
  completed in 9m24s with 0 errors and 8 existing warnings from `bc4d9d174`. The subsequent main merge
  changed only `.md` and skill metadata, so no API input to that build changed.
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
- Final current-main incremental review covered `bc05263e7..ddd2ca4ce` (232 commits), including the
  net PR diff and automatic Concert registration merge seam; it found no issue and stamped native and
  security watermarks at `ddd2ca4ced246a23969965ff2eacd508956f3b0b`.

## Decisions and constraints

- Full API + UI merge-queue E2E is required because this is a broad booking, payment, workflow, and
  settlement dispatch refactor. Remove any skip labels or trailers; do not duplicate E2E locally.
- Use a short `--artifacts-path` for local SDK work because the deep Windows worktree can exceed path
  limits in ordinary `obj` output.
- After source merge, transfer the plan-only observation tail to a clean
  `Docs/launch_deal_strategy_registration_closeout` worktree, remove this feature worktree and branch,
  then own package publication and generated platform sync to terminal green before docs closeout.

## Next Steps

Blocked: PR #451's current-base merge-group run `31486673612` has a failed Tenant integration job caused by a GitHub runner network reset while pulling SQL Server from MCR.
Unblock action: Once GitHub runner access to `mcr.microsoft.com` is healthy, explicitly re-enqueue PR #451 once and require a fresh current-base merge-group run to pass; do not rerun the failed job.
Resume when: A fresh merge-group run for verified remote head `30c459d7` is admitted, or PR #451 is merged after that run passes; then transfer the plan-only observation tail to the close-out worktree, remove the feature worktree/branch, and own publication and platform sync through terminal green.
