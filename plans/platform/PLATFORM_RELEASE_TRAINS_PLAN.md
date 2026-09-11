# Platform release trains

Next steps live in
[`PLATFORM_RELEASE_TRAINS_PROGRESS.md`](PLATFORM_RELEASE_TRAINS_PROGRESS.md) -> `## Next Steps`.

## Objective

Stop the monorepo paying a per-merge version tax for a pin nothing reads, and finish the per-producer
release-train split the cut depends on while the trains are still cheap to change.

`REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` defines the end state: independent trains, distinct
consumer properties, Renovate producer-pull, and a staged expand/contract sequence for breaking shapes.
Two trains exist today — `ConcertableDotNetPlatformVersion` and `ConcertablePaymentVersion` — while the
remaining 58 packable projects still ride one MinVer height under the platform property. This plan
finishes that split before the cut, so the cut moves trains rather than inventing them under nine-repo
conditions.

## The problem this plan fixes

`api/TECH_DEBT.md` owns the diagnosis — PR CI proves the platform builds from source and never that a
service builds against the versions it pins, because every .NET job routes through
`scripts/local-platform.ps1`, which overrides the pin with a version packed from the commit's own
source. Read it there; it is not restated here.

What this plan adds is the consequence for delivery:

- The one workflow that does restore at the committed pin is `publish-images.yml`, which runs a plain
  `dotnet publish` against the feed. So on the main commit where a platform change lands, every
  deployable image is built against the **previous** platform. `test.yml`'s `container-images` job
  already names the asymmetry: it images "against that source and not the stale feed".
- A sync merge touches `api/*/Directory.Packages.props`, which is an `api/**` push, which republishes at
  a new MinVer height that the cascade guard then suppresses. The pin is structurally always behind the
  feed; currency is unreachable by construction, so chasing it per-merge cannot succeed.
- One MinVer height for the platform train means a commit touching one service republishes packages
  whose content did not change, and the sync PR then moves every service to consume them.

Measured on `origin/main` at `081e149a2`: **455 pin-bump commits and 238 sync merges out of 6572
commits**, each a full merge-queue cycle with its own build and carve jobs.

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

Delete `platform-sync.yml`, `platform-sync-alert.yml`, `.github/scripts/bump-platform-version.sh` and
its test, `.github/scripts/platform-sync-pr-action.mjs` and its test, and the `platform-sync-broken`
label. Replace with one scheduled weekly refresh that opens at most one non-blocking PR.
`PLATFORM_SYNC_TOKEN` becomes unused here but is not removed: the migration plan owns secret teardown at
cutover.

The pin survives because two things still read it: a developer's default `dotnet restore` with no local
platform prepared, and the `ConcertableDotNetPlatformVersion` each service repo inherits at the cut.
Neither needs it current per-merge; both need it resolvable.

**Ordering** — strictly after Phase 1. Until images stop reading the pin, letting it drift ships stale
images.

**Verification gate** — `workflow-tests` green with the deleted tests removed from
`node --test .github/scripts/*.test.mjs`; a clean `dotnet restore api/Concertable.slnx` with no local
platform prepared resolves every `Concertable.*` at its pinned version from the feed.

### Phase 3 — finish the train split and publish only what changed

Extend the two existing trains to cover every `platform` and `service-owned` row in the binding admission
table, including the System train, give each admitted train its own MinVer tag prefix, and make
`publish-packages.yml` push only the admitted trains whose source changed in the pushed range. `vendor` rows
leave the published surface and receive no train property.
`ConcertablePaymentVersion` is the worked precedent: a conditioned property that falls back to the
platform version under `UseLocalPlatformPackages`, so the source inner loop keeps working while the
published pins diverge.

**Consumption contract** — each service's `Directory.Packages.props` carries one property per consumed
train, using the names the cut plan already fixes (`ConcertableDotNetPlatformVersion`,
`ConcertableB2BContractsVersion`, `ConcertablePaymentVersion`, …), so the cut inherits them.
`scripts/local-platform.ps1` overrides every train property, not just the platform one.

**Depends on** `PUBLISHED_SURFACE_ADMISSION_PLAN.md` Phase 1 for train membership: which package sits in
which train is that plan's verdict, not this one's.

**Verification gate** — the admitted package set matches the binding table exactly, no `vendor` row is
packed or pushed, and `verify-restore` restores the full published closure with mixed train versions;
all `carve-*` jobs green; `package_publication_policy.py` still passes on a partial batch; a
Payment-only commit publishes only the Payment train, proved by diffing the feed version index before
and after.

## Out of scope

The size of the published surface. Which of the 58 packages should exist at all is
`PUBLISHED_SURFACE_ADMISSION_PLAN.md`; this plan versions and publishes whatever that plan admits.
