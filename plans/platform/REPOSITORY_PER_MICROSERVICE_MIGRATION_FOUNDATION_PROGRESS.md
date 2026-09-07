# Repository-per-microservice foundation progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-RepoSplit-M2-Owner-Operations`
- Branch: `Refactor/RepoSplit-M2-Owner-Operations`
- PR: [#947](https://github.com/Concertable/concertable/pull/947), draft; independent sibling restacked onto
  landed `origin/main` `516f4cc25936289744babef3f98b1a297035fbb6`
- Dependency/package gates: PR #633 and the Docker-backed integration proof are cleared. M2 still requires a
  refreshed exact-head review and PR CI; private-repository merge-queue rulesets remain unavailable on the
  current GitHub entitlement.
- Last reconciled: 2026-09-07 against landed `origin/main`
  `516f4cc25936289744babef3f98b1a297035fbb6` and rebased M2 candidate
  `c74ca5f3ad0c194a583bd3f0ad303d7d2c5bf7c9` immediately before this checkpoint.

## Current state

Checkpoint 6A is terminal and checkpoint 6B preparation is active. Existing private `auth`, `b2b`,
`customer`, `payment`, `search`, `infra`, and `config` repositories retain their identities. The remaining
repository boundaries are `platform-dotnet`, `platform-frontend`, and `system`; no repository creation is
part of these preparation packets.

M1 is published as four draft stacked PRs #942-#945. M2 is independent sibling draft PR #947. Its ten staged
patches were replayed without conflict from the pre-landing base onto exact landed `origin/main`
`516f4cc25936289744babef3f98b1a297035fbb6`; `git range-diff` reports all ten patches unchanged. All 24 EF
contexts are assigned to owner-local manifests, including Opportunity, Application, and Booking under B2B.
The root migration and local-development scripts are compatibility delegators only. Offline ownership,
rollback, path, bootstrap, evaluated-reference, Docker-health, and full fresh-container integration gates
pass. The previous review watermarks are not ancestors of the rebased candidate, so the mandatory refreshed
exact-head review and PR CI remain before M2 can be represented as merge-ready. M3 is active independently
and does not depend on M1 or M2.

## Next Steps

- Refresh the full general and security review against the exact rebased candidate, resolve any findings, and
  preserve the review work order locally.
- Force-with-lease update draft PR #947 only after the reviewed candidate is stable, then require exact-head
  PR CI before representing M2 as merge-ready.
- Keep M1, M2, and M3 as separate packets and preserve producer/package and final service cutover ordering.

## Completed work

- Checkpoint 6A closed through `.github` PRs #1 and #2; all eleven reusable workflows passed from the public
  fixture before shared policy was applied and read back.
- Corrective commits `82bf5dbbb` and `bb59d9ba3` established the retained target identities and the fixed
  `platform-dotnet`, `platform-frontend`, and `system` topology.
- M1 is represented by draft PRs #942-#945 and creates no repository.
- M2's ten staged commits above the former #633 snapshot retain the owner delegator, assign the three new B2B
  contexts introduced by #633, and carry the completed review repairs. They were replayed without conflict
  onto landed `origin/main` `516f4cc2593`; all ten entries in `git range-diff` are patch-identical.
- The full M2 review found four publication defects: an implicit PowerShell runtime assumption, missing CI
  invocation, missing reparse-point coverage, and monorepo-only scripts leaking into the System extraction.
  All four are repaired; full and incremental review resolved every finding. The approved substantive and
  publication watermark is `97b447bcf`, and `ae43e13cc` carries the review record only.
- M3 is independent sibling draft PR #948; it is not a dependency of M2.

## Verification

- `scripts/test-owner-operations.ps1`: pass for 24 contexts, six isolated migration carves, five isolated
  bootstraps, root dry runs, System rejection, idempotency, rollback, ID stability, environment restoration,
  and path containment.
- `scripts/test-system-bootstrap.ps1`: pass; real MSBuild evaluation rejects runtime references introduced
  by props, targets, and explicit imports while ignoring an inactive reference.
- `eng/repository-split/validate_map.py --owner-operations-only`: pass; monorepo compatibility commands
  dissolve and only System-owned operation commands survive the System extraction map.
- Mandatory CI: `workflow-tests` runs both PowerShell gates; `split-inventory` enforces the focused extraction
  ownership contract. The edited workflow parses successfully as YAML.
- `git diff --check`: pass after the #633 inventory reconciliation.
- `scripts/docker-health.ps1`: pass; the fresh-container host-to-container `DATA` round trip is stable.
- `scripts/integration.ps1 run`: pass; all 22 integration-test projects completed successfully. The first run
  from the long worktree path hit Windows native-DLL path limit `0x800700CE`; rerunning the unchanged owning
  worktree through a temporary short drive alias passed, and the alias was removed after the run.

## Reviews

The original M2 review found three safety defects, all repaired by `d670e3383`; its incremental review found
no regression. The rebased frozen-head review at `a2115afc5` requested four further changes, all repaired by
`7a561adbe`. The final full review of that fixing head requested the platform-tool destination and checkpoint
corrections, both fixed by `d98622e69`. The incremental review over that commit found no further defects and
advanced both the general and security watermarks through `97b447bcf`; `ae43e13cc` adds the review record
only. Those watermarks are not ancestors of the landed-main restack. Draft PR #947 therefore requires a new
full general and security review of the exact rebased candidate before its force-with-lease update.

## Decisions, discoveries, blockers, and deviations

- B2B owns 11 of the current 24 EF contexts; Messaging remains the platform owner of Inbox and Outbox.
- `api/initial-migrations.ps1` remains a thin compatibility route. Owner-local manifests and runners are the
  independently extracted interface.
- M1, M2, and M3 may be prepared in parallel. M2 and M3 are sibling candidates based on #633; neither is
  silently folded into M1 or into the other packet.
- No Docker reset, WSL shutdown, repository creation, history import, visibility change, package publication,
  or service cutover was performed.
