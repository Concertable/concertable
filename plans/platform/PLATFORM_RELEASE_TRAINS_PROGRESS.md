# Platform release trains progress

- Plan: `plans/platform/PLATFORM_RELEASE_TRAINS_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/release-trains`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-DotNetPlatformPublisherCutover`
- Branch: `Refactor/DotNetPlatformPublisherCutover`
- PR: none yet
- Dependency/package gates: Phase 3 is blocked on `PUBLISHED_SURFACE_ADMISSION_PLAN.md` Phase 1 for
  train membership. Phases 1 and 2 have no dependency.
- Last reconciled: 2026-09-11 against `origin/main` `f48aec45680760aebb45aaf655341072dc769c86`.

## Current state

Phase 1 is delivered. Phase 2 is implemented locally: the monorepo publisher filters its packed batch from
the generated ownership inventory, the six bespoke platform-sync artifacts are retired, and Renovate owns a
grouped weekly non-automerge refresh of the shared platform pin. The pin remains at its last resolvable
`0.1.0-alpha.0.1370` slow floor until the service-owned package trains no longer share that property.

## Completed milestones

- Phase 1: PR `#1002` merged as `f48aec45680760aebb45aaf655341072dc769c86`; merge-group CI passed.
- Post-merge image publication run `34549092195` published all nine deployables. The extracted B2B image
  `Concertable.DataAccess.Infrastructure.dll` has ProductVersion
  `0.1.0-local.1789088643628+f48aec45680760aebb45aaf655341072dc769c86`, matching the run's prepared
  local-platform version.

## Latest verification

- Phase 1 exact-head CI run `34545641396`: `ci-complete`, `workflow-tests`, build, and container images green.
- Phase 1 merge-group run `34547024584`: full API/UI E2E and aggregate CI green.
- Phase 2 workflow-policy suite: 41/41 service/E2E scope checks passed; package publication policy passed.
- `python eng/repository-split/inventory.py --check`: passed; 23 retained, 34 platform, and 1 system package.
- `dotnet restore api/Concertable.slnx`: passed from configured feeds with no local platform prepared at the
  retained `0.1.0-alpha.0.1370` slow-floor pin.
- `git diff --check`: passed.

## Reviews

Phase 2 review pending after the branch is reconciled with current `origin/main`.

## Decisions and discoveries

- The diagnosis — PR CI never restores the committed pin — is owned by `api/TECH_DEBT.md`, not by this
  plan. Any future reasoning that treats a green sync PR as evidence a consumer compiles against the
  published pin is wrong.
- Two trains already exist on `main`: `ConcertableDotNetPlatformVersion` (9 pins) and
  `ConcertablePaymentVersion` (4 pins, conditioned to fall back to the platform version under
  `UseLocalPlatformPackages`). Phase 3 extends that precedent; it does not invent the split. The
  property was renamed from `ConcertablePlatformVersion` — do not reintroduce the old name.
- `publish-images.yml` is the only workflow that restores at the committed pin, which is why Phase 1
  must precede Phase 2.
- The pin cannot be made current: the sync merge is itself an `api/**` push whose republish the cascade
  guard suppresses. Do not attempt to close that gap; remove the dependency instead.
- `PIPELINE_REDESIGN_PLAN.md` N3 records the same per-merge tax and gave `platform-sync.yml` the
  verdict "keep, simplify". That verdict now points here; the merge-queue half of that plan is
  untouched.
- `PLATFORM_SYNC_TOKEN` is left in place by Phase 2. The migration plan owns secret teardown at
  cutover, and retiring it early would contradict that.
- `ConcertableDotNetPlatformVersion` still versions retained service packages as well as platform packages.
  Advancing it to `0.2.0-alpha.0.4` made the feed-only restore fail on service packages whose latest train is
  `0.1.x`; Phase 2 therefore preserves the resolvable slow floor. A platform consumer cannot move to `0.2.x`
  until those service-owned references use their own train properties.
- This plan was first authored against a checkout 868 commits behind `main`, which made its counts and
  the single-pin premise wrong. Re-measure against `origin/main` before quoting any figure from it.

## Next Steps

1. Merge current `origin/main`, then review the Phase 2 publisher-cutover candidate.
2. Push the PR and require exact-head `ci-complete`, `workflow-tests`, and the feed-only restore green.
3. Merge Phase 2 and delete the `platform-sync-broken` GitHub label.
4. Reconcile the package-train dependency before moving the PostgreSQL consumer to platform `0.2.x`.
