# Repository-per-microservice foundation progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-RepoSplit-M2-Owner-Operations`
- Branch: `Refactor/RepoSplit-M2-Owner-Operations`
- PR: not opened; M2 is an independent sibling candidate based on monorepo PR #633
- Dependency/package gates: M2 delivery waits for PR #633 to land. Its Docker-backed integration proof is
  blocked by the local Docker Desktop backend failing during WSL bootstrap; private-repository merge-queue
  rulesets remain unavailable on the current GitHub entitlement.
- Last reconciled: 2026-09-06 against corrective topology commits `82bf5dbbb` and `bb59d9ba3`, the preserved
  M1-M3 preparation record at `f4709fe4b`, and PR #633 head `ad4ad986f4f61f328ec9aae14a5fec1ccde364db`.

## Current state

Checkpoint 6A is terminal and checkpoint 6B preparation is active. Existing private `auth`, `b2b`,
`customer`, `payment`, `search`, `infra`, and `config` repositories retain their identities. The remaining
repository boundaries are `platform-dotnet`, `platform-frontend`, and `system`; no repository creation is
part of these preparation packets.

M1 is published as four draft stacked PRs #942-#945. M2 is independently restacked onto the exact live #633
head as three commits ending at the current work head: the original owner-operation implementation, its
review hardening, and the #633 inventory reconciliation. All 24 EF contexts are assigned to owner-local
manifests, including #633's Opportunity, Application, and Booking contexts under B2B. The root migration and
local-development scripts are compatibility delegators only. Offline ownership, rollback, path, bootstrap,
and evaluated-reference gates pass. M3 is active in its sibling worktree at `7f834b9d3` and does not depend
on M1 or M2.

## Next Steps

- When Docker Desktop reaches a running backend, require `scripts/docker-health.ps1` to pass, then run
  `scripts/integration.ps1 run` for the fresh-container migration proof.
- Complete the frozen-head review, commit this checkpoint, and open M2 as a draft sibling PR with #633 as
  its explicit base. Do not represent it as merge-ready until the Docker proof is green.
- Keep M1, M2, and M3 as separate packets. After #633 lands, move each packet to the exact landed
  `origin/main`, revalidate its own gates, and preserve producer/package and final service cutover ordering.

## Completed work

- Checkpoint 6A closed through `.github` PRs #1 and #2; all eleven reusable workflows passed from the public
  fixture before shared policy was applied and read back.
- Corrective commits `82bf5dbbb` and `bb59d9ba3` established the retained target identities and the fixed
  `platform-dotnet`, `platform-frontend`, and `system` topology.
- M1 is represented by draft PRs #942-#945 and creates no repository.
- M2 commits `a26a3cc6f` and `d670e3383` were restacked as `99d0f2a33` and `ea17ae511` onto #633. The rebase
  retained the owner delegator and assigned the three new B2B contexts introduced by #633. The reconciliation
  checkpoint is `1182b90d2` before this exact-head metadata refresh.
- M3 commits `9654935ae` and `62772dc20` were restacked as `b12104de7` and `7f834b9d3`; its generic Metro
  resolver preserves #633's Stripe and React Native package visibility without product-specific platform code.

## Verification

- `scripts/test-owner-operations.ps1`: pass for 24 contexts, six isolated migration carves, five isolated
  bootstraps, root dry runs, System rejection, idempotency, rollback, ID stability, environment restoration,
  and path containment.
- `scripts/test-system-bootstrap.ps1`: pass; real MSBuild evaluation rejects runtime references introduced
  by props, targets, and explicit imports while ignoring an inactive reference.
- `git diff --check`: pass after the #633 inventory reconciliation.
- Docker proof: externally blocked. Docker Desktop remains in `state:starting`; WSL bootstrap reports a
  missing virtual disk and `/_ping` returns HTTP 500. Resume only when `scripts/docker-health.ps1` is green.

## Reviews

The original M2 review found three safety defects, all repaired by `d670e3383`; its incremental review found
no regression. The rebased candidate requires a fresh frozen-head review covering the #633 context inventory
and the new 24-context assertion before publication.

## Decisions, discoveries, blockers, and deviations

- B2B owns 11 of the current 24 EF contexts; Messaging remains the platform owner of Inbox and Outbox.
- `api/initial-migrations.ps1` remains a thin compatibility route. Owner-local manifests and runners are the
  independently extracted interface.
- M1, M2, and M3 may be prepared in parallel. M2 and M3 are sibling candidates based on #633; neither is
  silently folded into M1 or into the other packet.
- Docker/WSL host repair is outside this branch. No Docker reset, WSL shutdown, repository creation, history
  import, visibility change, package publication, or service cutover was performed.
