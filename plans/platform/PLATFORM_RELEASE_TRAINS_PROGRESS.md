# Platform release trains progress

- Plan: `plans/platform/PLATFORM_RELEASE_TRAINS_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/release-trains`
- Worktree: none yet
- Branch: none yet
- PR: none yet
- Dependency/package gates: Phase 3 is blocked on `PUBLISHED_SURFACE_ADMISSION_PLAN.md` Phase 1 for
  train membership. Phases 1 and 2 have no dependency.
- Last reconciled: 2026-09-09 against `origin/main` `081e149a2e0a4544e1c781adc5e322a6ee0e4f85`, merged
  into this branch.

## Current state

Authored, not started. No phase implemented, no worktree, no branch.

The evidence the plan rests on was gathered by reading the live workflows at the SHA above and is
recorded in the plan rather than here, because it is design input rather than recovery state.

## Completed milestones

None.

## Latest verification

None. Nothing implemented.

## Reviews

None recorded — nothing implemented to review.

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

Implement Phase 1 in a fresh worktree off the current remote default:

1. `./scripts/worktrees.ps1` a worktree for branch `Chore/PlatformImagesFromSource`.
2. In `.github/workflows/publish-images.yml`, replace the `Build and push the image` step's plain
   `dotnet publish` with a `scripts/local-platform.ps1 prepare` step followed by
   `scripts/local-platform.ps1 publish "<project>" --configuration Release /t:PublishContainer
   -p:ContainerImageTag=${{ github.sha }}`, mirroring `test.yml`'s `container-images` job. The matrix
   shares one workspace per job, so `prepare` runs once per matrix leg.
3. Keep the `discover` job, the `<ContainerRepository>` derivation, the GHCR login, the SHA-only tag and
   the digest report exactly as they are.
4. Gate: `ci-complete` and `workflow-tests` green on the PR; after merge, confirm the
   `Concertable.DataAccess.Infrastructure.dll` `ProductVersion` inside one published image equals that
   run's local platform version.
