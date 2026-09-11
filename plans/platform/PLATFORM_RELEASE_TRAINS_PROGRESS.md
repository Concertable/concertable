# Platform release trains progress

- Plan: `plans/platform/PLATFORM_RELEASE_TRAINS_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/release-trains`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Chore-PlatformImagesFromSource`
- Branch: `Chore/PlatformImagesFromSource`
- PR: none yet
- Dependency/package gates: Phase 3 is blocked on `PUBLISHED_SURFACE_ADMISSION_PLAN.md` Phase 1 for
  train membership. Phases 1 and 2 have no dependency.
- Last reconciled: 2026-09-11 against `origin/main` `1f8f1d59a8dc9e0c94e745c81eb41a4c409725ef`.

## Current state

Phase 1 is implemented on the branch above. Every image matrix leg now prepares the local platform and
publishes through `scripts/local-platform.ps1`, matching the existing `container-images` CI path. The
focused workflow-policy test is wired into the repository's workflow-test aggregator.

## Completed milestones

None delivered yet.

## Latest verification

- `python .github/workflows/tests/test_service_scope.py`: 41/41 service/E2E scope checks passed; package
  publication policy passed; image publication policy passed 6/6.
- `git diff --check`: passed.

## Reviews

- Independent merge review of `fe4b7919e`: one finding — invoke the non-executable PowerShell script
  explicitly through `pwsh` on Ubuntu runners.
- Incremental independent review of `f22f784e9`: clean; both workflow calls and their policy assertions
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
- This plan was first authored against a checkout 868 commits behind `main`, which made its counts and
  the single-pin premise wrong. Re-measure against `origin/main` before quoting any figure from it.

## Next Steps

1. Push the reviewed Phase 1 candidate and require exact-head `ci-complete` and `workflow-tests` green.
2. Merge and confirm one published image contains the local-platform DataAccess assembly version from its
   exact publication run.
3. Resume Phase 2: retire platform sync and hand the slow floor to Renovate.
