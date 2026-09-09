# Platform release trains

Next steps live in
[`PLATFORM_RELEASE_TRAINS_PROGRESS.md`](PLATFORM_RELEASE_TRAINS_PROGRESS.md) -> `## Next Steps`.

## Objective

Stop the monorepo paying a per-merge version tax for a pin nothing reads, and adopt the per-producer
release-train shape the cut already specifies while the trains are still cheap to change.

`REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` defines the end state — independent trains, distinct
consumer properties, Renovate producer-pull, expand/migrate/prove/contract for breaking shapes. It
assumes those trains exist at cut time. They do not: today all 55 `IsPackable` projects share one
MinVer height and one `<ConcertablePlatformVersion>` pin. This plan builds the trains before the cut,
so the cut moves them rather than inventing them under nine-repo conditions.

## Why the current pin is not load-bearing, and what that costs

Established by reading every `.NET` job in `.github/workflows/test.yml`:

- Every one of them — `build`, all five `carve-*`, `container-images`, `unit-tests`,
  `architecture-tests`, `integration-tests`, both E2E jobs — runs through `scripts/local-platform.ps1`,
  which passes `-p:ConcertablePlatformVersion=<version packed from the commit's own source>`.
  **No PR job ever restores the committed pin.** The sync PR therefore cannot go red for the reason
  `platform-sync.yml` documents, and `platform-sync-alert.yml` guards a state the pipeline cannot
  produce.
- The single genuine consumer is `publish-images.yml`, which runs a plain `dotnet publish` against the
  feed at the committed pin. So on the main commit where a platform change lands, every deployable
  image is built against the **previous** platform. `test.yml`'s `container-images` job already names
  this: it images "against that source and not the stale feed".
- A sync merge touches `api/*/Directory.Packages.props`, which is an `api/**` push, which republishes at
  a new MinVer height that the cascade guard then suppresses. The pin is structurally always behind the
  feed; currency is unreachable by construction.
- Lockstep MinVer means a Payment-only commit republishes `Concertable.Kernel` at a new version with
  identical content, and the sync PR then moves every service so it can consume a package that did not
  change.

Measured cost on `main`: **423 pin-bump commits and 221 sync merges out of 5704 commits**, each a full
merge-queue cycle with its own build and carve jobs.

## Phases

### Phase 1 — main's images build from the commit's own source

`publish-images.yml` prepares and publishes through `scripts/local-platform.ps1`, exactly as
`test.yml`'s `container-images` gate already does, instead of restoring `Concertable.*` from the feed at
the committed pin. This removes the last real consumer of the pin and closes the stale-image window in
the same change.

**Consumption contract** — unchanged, deliberately. GHCR image, commit SHA as the only tag, digest
reported by the existing step; AppHosts and the fleet manifest keep pinning digests. Only the bytes
change: platform assemblies now match the SHA being imaged.

**Verification gate** — `ci-complete` green; `workflow-tests` green. After merge, the
`Concertable.DataAccess.Infrastructure.dll` `ProductVersion` inside one published image equals that
run's local platform version (the assertion shape `Assert-DataAccessAssembly` already applies to test
projects).

### Phase 2 — retire platform-sync; the pin becomes a slow floor

Delete `platform-sync.yml`, `platform-sync-alert.yml`, `.github/scripts/bump-platform-version.sh`,
`.github/scripts/platform-sync-pr-action.mjs` and its test, and the `platform-sync-broken` label; retire
`PLATFORM_SYNC_TOKEN`. Replace with one scheduled weekly refresh that opens at most one non-blocking PR.

The pin survives because two things still read it: a developer's default `dotnet restore` with no local
platform prepared, and the post-cut `ConcertableDotNetPlatformVersion` seed. Neither needs it current
per-merge; both need it resolvable.

**Ordering** — strictly after Phase 1. Until images stop reading the pin, letting it drift ships stale
images.

**Verification gate** — `workflow-tests` green with the deleted test removed from
`node --test .github/scripts/*.test.mjs`; a clean `dotnet restore api/Concertable.slnx` with no local
platform prepared resolves every `Concertable.*` at the pinned version from the feed.

### Phase 3 — per-train versioning and affected-only publication

Split the 55 `IsPackable` projects into the trains the cut specifies, give each its own MinVer tag
prefix and consumer property, and make `publish-packages.yml` push only the trains whose source changed
in the pushed range.

**Consumption contract** — each service's `Directory.Packages.props` replaces the single
`<ConcertablePlatformVersion>` with one property per consumed train
(`ConcertableDotNetPlatformVersion`, `ConcertableB2BContractsVersion`, `ConcertablePaymentVersion`, …),
using the exact names the cut plan already names, so the cut inherits these properties rather than
introducing them. `scripts/local-platform.ps1` overrides every train property, not one.

**Depends on** `PUBLISHED_SURFACE_ADMISSION_PLAN.md` Phase 1 for train membership: which package sits in
which train is that plan's verdict, not this one's.

**Verification gate** — `verify-restore` restores the full published closure with mixed train versions;
all `carve-*` jobs green; a Payment-only commit publishes only the Payment train, proved by diffing the
feed version index before and after.

## Out of scope

The size of the published surface. Which of the 55 packages should exist at all is
`PUBLISHED_SURFACE_ADMISSION_PLAN.md`; this plan versions and publishes whatever that plan admits.
