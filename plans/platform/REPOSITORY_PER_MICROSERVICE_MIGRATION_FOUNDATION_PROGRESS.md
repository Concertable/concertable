# Repository-per-microservice foundation progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-RepoSplit-M2-Owner-Operations`
- Branch: `Refactor/RepoSplit-M2-Owner-Operations`
- PR: [#947](https://github.com/Concertable/concertable/pull/947), ready; independent sibling being merged
  with current `origin/main` `12efedd68da08d92b08990a30e76dab5546b5ed4` after M3 landed through #948.
- Dependency/package gates: PR #633 and the Docker-backed integration proof are cleared. M3 is landed and is
  not a dependency of M2. M2 requires focused validation, current-head review, and exact-head PR CI after
  this final base reconciliation before merge-queue delivery.
- Last reconciled: 2026-09-07 against current `origin/main`
  `12efedd68da08d92b08990a30e76dab5546b5ed4` and the prior exact-head-green M2 candidate
  `4c7109ab5c21e617c97a51572edd87eddb63ce88` immediately before this merge.

## Current state

Checkpoint 6A is terminal and checkpoint 6B preparation is active. Existing private `auth`, `b2b`,
`customer`, `payment`, `search`, `infra`, and `config` repositories retain their identities. The remaining
selected repositories are `platform-dotnet`, `platform-frontend`, and separate `system`; none is created by
this packet. General shared frontend code covers web and mobile while those remain package tiers, not
repository boundaries.

M1 is published as four draft stacked PRs #942-#945. M2 is ready as independent sibling PR #947. Its ten
staged patches were replayed unchanged onto landed #633 main, then merged cleanly with the B2B producer
landing. Exact-head CI run `34166864272` is green at `4c7109ab5`, but M3 landed while that run executed, so
this final current-main reconciliation must be validated, reviewed, and pushed before enqueue. All 24 EF
contexts are assigned to owner-local manifests, including Opportunity, Application, and Booking under B2B.
The root migration and local-development scripts are compatibility delegators only.

M3 landed independently through PR #948 at `12efedd68`. It extracts the product-neutral
`@concertable/build-config` package, makes product workspaces own their package lists, and uses the shared
Metro resolver for both mobile applications without encoding product ownership into the platform tier.

## Next Steps

- Resolve the current-main merge by retaining both M2 owner-operation assignments and landed M3 build-config
  ownership, then run focused owner, inventory, frontend-boundary, package, plan-graph, and whitespace gates.
- Incrementally review the exact merge head, push one stable candidate, require exact-head PR CI, and deliver
  PR #947 through the merge queue.
- Keep M1, M2, and M3 separate. Do not create repositories, import history, publish packages, or perform a
  service cutover from this preparation branch.

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
  publication watermark is `97b447bcf`, and `ae43e13cc` carries the review record only. The landed-main
  restack fixed the Unix path issue at `73ef5db31`; exact-head CI run `34165097661` passed.
- M3 landed independently through PR #948 at `12efedd68`, assigning `app/build-config` to
  `platform-frontend` and preserving product-owned frontend workspace manifests.

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
- Exact-head PR CI run `34166864272` passed at `4c7109ab5` before M3 landed; the current-main merge requires
  one final focused validation and exact-head CI cycle.

## Reviews

The original M2 review found three safety defects, all repaired by `d670e3383`; its incremental review found
no regression. The rebased frozen-head review at `a2115afc5` requested four further changes, all repaired by
`7a561adbe`. The final full review of that fixing head requested the platform-tool destination and checkpoint
corrections, both fixed by `d98622e69`. The incremental review over that commit found no further defects and
advanced both the general and security watermarks through `97b447bcf`; `ae43e13cc` adds the review record
only. The full landed-main review and CI repair pass advanced the current M2 review through `73ef5db31`; the
clean B2B base merge was approved through `4c7109ab5`. This current-main/M3 merge now requires only an
incremental review of its conflict resolutions before the next push.

## Decisions, discoveries, blockers, and deviations

- B2B owns 11 of the current 24 EF contexts; Messaging remains the platform owner of Inbox and Outbox.
- `api/initial-migrations.ps1` remains a thin compatibility route. Owner-local manifests and runners are the
  independently extracted interface.
- M1, M2, and M3 were prepared independently. M3 is now landed; M2 retains its own owner-operation packet
  and incorporates M3 only as current base state.
- `platform-frontend` owns general shared web/mobile packages and build tooling; product repositories own
  their own workspace manifests.
- No Docker reset, WSL shutdown, repository creation, history import, visibility change, package publication,
  or service cutover was performed.
