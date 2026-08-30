# Repository-per-microservice migration progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable/.worktrees/Plan-RepoSplit-Stage3-ImageMode`
- Branch: `Plan/RepoSplit-Stage3-ImageMode`
- PR: not opened
- Dependency/package gates: none active. Stage 3 rt1 publishes container images, not packages, so it
  needs no publish-before-consume round-trip and opens no `platform-sync` PR beyond the automatic one
  every `main` push already produces.
- Last reconciled: **2026-08-30** — stage 3 rt1 (the container-image bridge) implemented on this
  branch; ledger compacted to the `plan-checkpoint` budget in the same commit.

## Current state

Approved 2026-08-26 and in execution. **The plan document's 17 checkpoints are superseded by the nine
stages below, and its inventory figures are a 2026-08-02 snapshot this ledger overrides.**

Stages 1 and 2 are closed. Stage 3 is in flight.

1. ~~Swap the 41 test-tier `ProjectReference`s to `PackageReference`.~~ **Done** (PR #798).
2. ~~Make `*.Hosting` packable and publish; swap the last 4.~~ **Done** (PRs #805, #809 + rt3).
   Test-tier cross-repository `ProjectReference`s: **45 → 0**.
3. **AppHost image mode — in flight.** Convert the **44 `apphost` cross-target edges** so a service's
   AppHost stops compiling sibling source. **It owns the `*.ArchitectureTests`-in-the-carve-jobs gate.**
   Round-trips in `## Next Steps`.
4. **Move cross-service E2E to `fleet`** — 21 E2E cross-repository edges, no TestKit exists yet. The
   largest stage; gates `fleet`, customer and b2b, but not auth or payment.
5. **Extract `payment`** — mechanism proven end to end: a real `git-filter-repo` run produced a coherent
   standalone repo whose entire `src/` runtime compiled clean against the live feed.
6. **Extract `auth`** — needs Duende persisted grants moved from `B2BDb` to `AuthDb` first.
7. **Extract `search`, then `customer`, then `b2b`** — b2b last, widest contract and seed fan-out.
8. **Extract `platform-dotnet`, `platform-web`, `fleet`.**
9. **Archive the monorepo.** Checked read-only 2026-08-27 and **not executable**: no extraction target
   repo exists, every service is still monorepo source, and the monorepo still publishes.

**Sequencing correction — these stages are NOT a strict chain.** `blockingRuntimeEdges` is **1**
repo-wide (`Auth.Contracts → Messaging.Contracts`, a one-line swap on Auth extraction) and all five
carve gates are green, so **nothing blocks extracting a service today**. Stages 3 and 4 run **in
parallel with** the extractions, not in front of them. Do not re-estimate archival on paper again —
report the measured rate after one real extraction.

### Stage 3's measured shape

The 44 edges are two distinct problems, not one:

| Class | Count | Fix |
|---|---:|---|
| AppHost → `*.Hosting` / `AppHost.Shared` / `Frontend.Hosting` libraries | 26 | `PackageReference`, via the stage-1 source-swap mechanism extended to the AppHost tier. Needs `Search.Hosting` and `Frontend.Hosting` made packable first; neither is today. |
| AppHost → foreign **deployable** (`AddProject` on a sibling service) | 18 | `AddContainer` on a pinned image digest. |

Only **4** distinct images are needed by the standalone hosts (Auth, Payment.Web, Payment.Workers,
B2B.Seed.Simulator). The other five exist for the umbrella host, which becomes `fleet` at stage 8.

## Next Steps

**Review, then open and land the PR for stage 3 rt1 (the container-image bridge) on this branch.**

Stage 3's remaining round-trips, in order — publish-before-consume forces the split:

- **rt2 — the hosting seam.** `AppHost.Shared` gains the container-resource primitive; each `*.Hosting`
  gains an image-mode overload beside its existing `AddX<TProject>`; `Search.Hosting` and
  `Frontend.Hosting` become packable. Publishes packages; nothing consumes them yet.
- **rt3 — flip the hosts.** AppHost csprojs consume `*.Hosting` as packages (extend
  `api/PlatformSourcePackages.targets` from the test tier to the AppHost tier — `EnforceServiceBoundary`'s
  exemption set is already exactly `.AppHost` + tests, so the condition is the one to mirror); foreign
  deployables become `AddContainer` on pinned digests. This is what closes the 44 edges.
- **rt4 — the gate.** Add `*.ArchitectureTests` to the five carve jobs, plus a standalone-host boot smoke
  in image mode. Stage 3's closeout evidence.

## Completed work

| Item | Evidence |
|---|---|
| Plan authored; design-review blockers resolved | `91e92c445`, `3f8cb3494` |
| Checkpoint 0 — inventory generator, ownership map, extraction map + coverage validator | `f38825fba`, `fddd3d8ef`, `4fa96f8ed` |
| Per-service CI test scoping | `b0bbbdb06` |
| Stage 1 — test-tier package boundary (41 refs across 31 projects) | PR #798 |
| Stage 2 — `*.Hosting` packable + published; last 4 refs swapped; mirror workflows deleted | PRs #805, #809 |
| Stage 3 rt1 — container-image bridge | this commit |

## Verification

**Stage 3 rt1 (the current candidate).**

- All **9** opted-in deployables evaluate their container identity correctly —
  `dotnet msbuild -getProperty:` reports `ghcr.io/concertable/<name>` and
  `EnableSdkContainerSupport=true` on each, with the Functions base image on `B2B.Workers` alone.
- `python eng/repository-split/inventory.py --check` green — no `ProjectReference` moved.
- Both workflows parse; `test.yml` is 21 jobs. The repo's own workflow suites pass:
  `test_service_scope.py` 17/17, `node --test .github/scripts/*.test.mjs` 4/4.
- **Not run locally: the container builds themselves.** Proving they build is the whole purpose of the
  new `container-images` CI job, and heavy builds have not fitted on this dev machine since stage 1.

**Still-valid earlier gates.** All five carve jobs and `split-inventory` are green on `main`; stage 1's
mechanism was proven by three `git archive` carves building at 0 errors against the live feed.

## Reviews

`reviews/Plan-RepoSplit-Stage3-ImageMode.md` — full pass complete at `cf4107eae`, judgment **approved**,
both findings resolved, none open. **Caveat recorded in the artifact:** it was a self-review with no
independent lens, and both defects were caught by CI and by reading vendor targets rather than by the read.

The plan's own design review is recorded only as "blockers resolved" in `3f8cb3494`; treat its specific
findings as unknown beyond that.

## Decisions, discoveries, blockers, and deviations

- **The test tier declares packages and swaps back to source, rather than swapping outright.**
  `api/PlatformSourcePackages.targets` maps each platform package to its in-repo source; a carve has
  neither the file nor the source, so the declaration stands **with no edit**. This avoids a
  publish/restore round-trip on every shared-test-library change until stage 9. rt3 extends this same
  mechanism to the AppHost tier rather than inventing a second one.
- **Prove a "gate X becomes possible after stage Y" claim by running the gate at Y's closeout before
  writing it as the next step.** The `*.ArchitectureTests` carve gate was asserted to be unblocked by
  packable `*.Hosting`, never run, and cost a wasted phase. It actually fails `MSB3202` because a
  service `AppHost` `AddProject`s a sibling **deployable** — so it belongs to stage 3, not stage 2.
- **Image tags are the commit SHA only.** AppHosts and the fleet manifest pin **digests**, so one
  immutable tag suffices and no mutable tag exists to be pinned by mistake. A SemVer tag is deliberately
  absent: deriving one would mean recomputing MinVer (which has produced phantom versions here — see
  `platform-sync.yml`) or reading the NuGet feed, whose train belongs to the platform packages and not
  to these images. Each service gets a real release train when it is extracted to its own repository.
- **GHCR image visibility is undecided and deliberately not set.** Images are created with GHCR's
  default. `DEPLOYMENT.md` wants them public so ACA and local AppHosts need no pull credential, but
  making a package public is outward-facing and effectively irreversible — **Tommy's call**, and only
  actually needed once something outside CI pulls them (rt3 at the earliest).
- **`Concertable.Testing.E2E` has never been published** (0 feed versions), because it references *back
  into* `Concertable.Payment.E2ETests.{Web,Workers}` — a dependency cycle across a future repo boundary.
  Inverting that direction is a **stage 4 prerequisite**.
- **`api/TestConventions.targets` does not survive a carve**, so an extracted service silently loses the
  test-tier naming gate. It must ship from `platform-dotnet` as a `buildTransitive` targets package.
  `scripts/local-platform.ps1` likewise needs a per-repo redesign on extraction.
- **The carve/inventory scaffolding is disposable, not portable.** `carve-*`, `split-inventory` and
  `PlatformSourcePackages.targets` only *simulate* the end state; once a service owns its repo its
  ordinary CI answers the same question. Deleted at stage 9, not migrated.
- **`platform-dotnet`'s carve is multi-folder** (`Concertable.Shared` + `AppHost.Shared` + `Messaging`),
  so no `git archive <one prefix>` gate can express it. Stage 8 inherits this.
- **`git archive HEAD:<prefix>` archives HEAD, not the working tree.** Commit before carving, or the
  gate silently measures the wrong tree.
- **`git-filter-repo` is not installed as a git subcommand** here and must be invoked as its module
  script; a fresh clone used for extraction needs `core.longpaths=true` (the monorepo already sets it,
  so the failure appears only in a fresh clone).
- **Auth's Duende persisted grants live in `B2BDb`** and must move to `AuthDb` before Auth extraction.
- **B2B Workers ships as a container on native Azure Functions on Container Apps**, not Functions
  Consumption, which cannot run a custom container at all. **Write none of that contract by hand.**
  `Microsoft.Azure.Functions.Worker.Sdk`'s `AssignFunctionsBaseImage` target already sets the base image
  (derived from the TFM, so it tracks a framework bump), `/home/site/wwwroot`, `linux-x64`, both
  `AzureWebJobs*` env vars, and the real entrypoint `/opt/startup/start_nonappservice.sh`. Hand-written
  overrides are redundant, and `ContainerAppCommandInstruction=None` actively fails the build with
  `CONTAINER2026` by conflicting with the `ContainerAppCommand` that target sets. The project needs only
  `EnableSdkContainerSupport` and `ContainerRepository`.
- **The six generated mirrors no longer exist** (verified twice, 2026-08-27), so stage 9's mirror-rename
  sub-task is **void**, not pending, and the canonical names are already free.
- **Lockstep `ConcertablePlatformVersion` + the platform-sync PR are retired** at the split, in favour of
  independently versioned producer trains plus Renovate; breaking contracts use
  expand/publish/migrate/contract, never a repo-wide forced bump.
