# Repository-per-microservice migration — Stage 4 fleet E2E progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Plan-RepoSplit-Stage4-Fleet-E2E`
- Branch: `Plan/RepoSplit-Stage4-Fleet-E2E`
- PR: not opened
- Dependency/package gates: service TestKit and fleet source-provider boundary landed in PR #882
- Last reconciled: **2026-08-31** from `origin/main` `67a7c7d85`, PR #882 merge `b1a754ef9`, and the current branch inventory candidate

## Current state

This ledger exclusively owns Stage 4: removing remaining cross-service E2E implementation composition from
service repositories and moving system composition into fleet. PR #882 established B2B, Customer, and
Payment TestKits, E2E admin hosts, the fleet source provider, and the carve-E2E mechanism. It did not finish
the remaining E2E boundary migration.

The isolated Stage 4 worktree owns this ledger. The regenerated inventory contains eight temporary source-mode
E2E cross-target edges, all owned by the non-packable, carve-excluded fleet source provider: two each for B2B,
Customer, Payment, and Search. There are zero blocking E2E edges and zero blocking test edges.

The selected slice moves Search Web and Workers project metadata out of the carve-retained Search helper and
behind `IFleetProjectProvider`. The two previously implicit hard-coded Search implementation paths are now
honest inventory edges in the fleet source provider, while package mode builds without Search service source.

No agent following this ledger may edit rt3 AppHosts, customer-next, Auth-next, or the umbrella migration
ledger. It may read those ledgers to respect dependencies but must record only Stage 4 state here.

## Next Steps

Run the final read-only PR preflight, push the exact reviewed candidate, and deliver this slice's own PR.
After that slice merges, the next smallest independently shippable fleet slice
is the Payment pair: replace the fleet source provider's Payment Web and Workers implementation references
with service-owned package/image composition, reducing the eight remaining temporary source-mode edges to six
without moving service behavior or duplicating TestKit contracts.

## Completed work

- PR #882 merged as `b1a754ef9`, establishing service-owned TestKits, E2E admin hosts, fleet composition contracts, and `carve-e2e.ps1`.
- The fleet source and package-mode provider seam is present on current main.
- Created the owning isolated worktree and branch from `origin/main` `cf0da4c9b`.
- Merged current `origin/main` `67a7c7d85` without conflicts before final validation; the upstream delta is disjoint from this Search fleet slice.
- Regenerated and classified the split inventory: eight temporary fleet source-provider E2E edges, zero blocking E2E edges, and zero blocking test edges.
- Moved Search Web and Workers project metadata behind `IFleetProjectProvider`; the carve-retained Search helper no longer knows repository-relative service project paths.
- Added the two Search implementation references only to the non-packable, carve-excluded fleet source provider and regenerated `inventory.json`.
- Full review identified and resolved two findings: migrate the touched Search extension container to C# 14 syntax and add explicit metadata-routing coverage.
- Added a source-free Search helper model test plus source-provider path tests; both remain in their owning Search/fleet targets and introduce no blocking test edge.

## Verification

Current candidate:

- `python eng/repository-split/inventory.py --check` — passed; inventory current, with no test-tier cross-repository `ProjectReference`.
- `dotnet build api/tests/Concertable.Fleet.E2E.Source/Concertable.Fleet.E2E.Source.csproj --nologo` — passed with zero errors and two pre-existing sealed-constructor warnings.
- `scripts/local-platform.ps1 prepare` — generated 55 exact-head local packages at version `0.1.0-local.1788187646321`.
- `pwsh -NoProfile -File eng/repository-split/carve-e2e.ps1 ...` — generated the package-only carve.
- Package-only `dotnet build api/tests/Concertable.Fleet.E2E.slnx --no-restore -p:UseLocalPlatformPackages=true -p:UseFleetSourceProjects=false ...` — passed with zero errors; existing MessagePack vulnerability and generated nullable warnings remain.
- Package-only Payment E2E helper unit tests — passed 6/6.
- Search E2E helper metadata-routing unit tests — passed 1/1 in source mode and 1/1 in the exact-head package-only carve.
- Fleet source-provider Search path unit tests — passed 2/2 against the exact-head local package feed.
- The successful carve contains no B2B, Customer, Payment, or Search service `src` directory and no `Concertable.Fleet.E2E.Source` directory.
- Local browser/service E2E was not run; repository policy reserves it for merge-queue diagnosis.

## Reviews

PR #882's review is recorded in `reviews/Plan-RepoSplit-Stage4-E2E-TestKit.md`. This candidate's full review is
recorded in `reviews/Plan-RepoSplit-Stage4-Fleet-E2E.md`: the full pass at `83c97871a` raised two findings,
both are resolved, and the incremental pass through `80f6da637` is approved with no new findings.

## Decisions, discoveries, blockers, and deviations

- `Concertable.Testing.E2E` remains service-agnostic; fleet-specific types belong to fleet-owned projects.
- Service TestKits expose service-owned reset/seed/client contracts; fleet owns cross-service orchestration.
- Stage 4 runs independently of rt3 package publication and the private service extraction proofs.
- The original six source-mode inventory edges were already approved temporary fleet-provider edges. Search
  Web and Workers added two hidden source paths in a carve-retained service helper; this slice makes those two
  dependencies explicit and confines all eight to the carve-excluded fleet source provider.
- The package-only carve initially exceeded Windows' path-length limit beneath the nested worktree. Repeating
  the identical carve beneath the repository's short ignored `artifacts` path produced a clean build.
