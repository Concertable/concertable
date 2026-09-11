# Platform release trains progress

- Plan: `plans/platform/PLATFORM_RELEASE_TRAINS_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/release-trains`
- Worktree: none; Phase 2 worktree closed after merge.
- Branch: none active.
- PR: `#1003` — https://github.com/Concertable/concertable/pull/1003 — merged.
- Dependency/package gates: Phase 3 train membership is now defined by the completed local Phase 1 table in
  `PUBLISHED_SURFACE_ADMISSION_PLAN.md`; it becomes unblocked when that docs PR lands. Phases 1 and 2 have no dependency.
- Last reconciled: 2026-09-11 after Phase 2 merged as
  `d01a2a4857c10be8e1e9ca27f3b2e18543d2c38e`.

## Current state

Phases 1 and 2 are delivered. The monorepo publisher filters its packed batch from
the generated ownership inventory, the six bespoke platform-sync artifacts are retired, and Renovate owns a
grouped weekly non-automerge refresh of the shared platform pin. Retained package dependency metadata is
validated against that pin before publication. The pin remains at its last resolvable
`0.1.0-alpha.0.1370` slow floor until the service-owned package trains no longer share that property.

## Completed milestones

- Phase 1: PR `#1002` merged as `f48aec45680760aebb45aaf655341072dc769c86`; merge-group CI passed.
- Phase 2: PR `#1003` merged as `d01a2a4857c10be8e1e9ca27f3b2e18543d2c38e`; exact-head and merge-group CI passed.
- Phase 2 publication run `34580651732` published the retained service batch and passed its isolated
  feed-restore verification.
- Post-merge image publication run `34549092195` published all nine deployables. The extracted B2B image
  `Concertable.DataAccess.Infrastructure.dll` has ProductVersion
  `0.1.0-local.1789088643628+f48aec45680760aebb45aaf655341072dc769c86`, matching the run's prepared
  local-platform version.

## Latest verification

- Phase 1 exact-head CI run `34545641396`: `ci-complete`, `workflow-tests`, build, and container images green.
- Phase 1 merge-group run `34547024584`: full API/UI E2E and aggregate CI green.
- Phase 2 workflow-policy suite: 41/41 service/E2E scope checks passed; package publication policy passed.
- Phase 2 exact solution pack produced 58 packages; the ownership filter retained 23 service packages and
  removed 35 platform/system packages. All 39 retained service-to-platform nuspec dependencies target
  `0.1.0-alpha.0.1370`.
- A fresh isolated-cache consumer restored the 23 candidate service packages plus all 34 published platform
  packages without a dependency downgrade.
- `python eng/repository-split/inventory.py --check`: passed; 23 retained, 34 platform, and 1 system package.
- `dotnet restore api/Concertable.slnx`: passed from configured feeds with no local platform prepared at the
  retained `0.1.0-alpha.0.1370` slow-floor pin.
- Phase 1 image publication policy passed 6/6.
- `git diff --check`: passed.

## Reviews

Phase 2 canonical review is approved through `3686811e6e2eb0afd30d486a444d40e9ae4a23ca`:
`reviews/Refactor-DotNetPlatformPublisherCutover.md`. Native, test-impact and security incremental lenses are
clean after resolving the ownership-trigger, packed-dependency proof and case-insensitive NuGet-ID findings.
- Phase 1 independent merge review of `fe4b7919e`: one finding — invoke the non-executable PowerShell script
  explicitly through `pwsh` on Ubuntu runners.
- Phase 1 incremental independent review of `f22f784e9`: clean; both workflow calls and their policy assertions
  now require the explicit PowerShell host.

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
- The Phase 1 admission table assigns every current packable project to a platform/service train or an
  explicit unpublished verdict. Phase 3 must consume that table rather than today's publisher inventory.
- This plan was first authored against a checkout 868 commits behind `main`, which made its counts and
  the single-pin premise wrong. Re-measure against `origin/main` before quoting any figure from it.

## Next Steps

1. Land the published-surface admission table.
2. Split service and platform consumer properties from the binding table before
   moving the PostgreSQL consumer to platform `0.2.x`.
