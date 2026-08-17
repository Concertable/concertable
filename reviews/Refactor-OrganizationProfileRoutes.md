# Code review — Refactor/OrganizationProfileRoutes

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `649dfb2c6b62028aee73645cfc4ba540e791ef56`  _(2026-08-17)_

**Security-reviewed up to commit:** `649dfb2c6b62028aee73645cfc4ba540e791ef56`  _(2026-08-17)_

> Range reviewed: `89361e9..649dfb2` (including current main).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **BUG1 — HIGH — correctness** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Data/Configurations/ArtistReadModelConfiguration.cs:14`
  The Artist and Venue projections still enforce unique `UserId` indexes, so one human cannot create profiles for multiple organizations even though ownership is now tenant-based. Remove both unique user indexes and re-scaffold the Concert migration.

- [x] **SEED1 — HIGH — seeding** — `api/Concertable.B2B/src/Seed/Concertable.B2B.Seed.Contracts/SeedSpecMappers.cs:11`
  `VenueSeedSpec.ToChangedEvent` leaves the new `TenantId` at `Guid.Empty`, so standalone seed events cannot populate tenant-keyed Venue projections. Map the canonical tenant ID into the event and cover it in the seed-contract tests.
  Resolved by mapping `TenantSeedIds.For(spec.UserId)`; the repository has no seed-contract test project, and the B2B build compiles the source contract while Search intentionally remains on the published package until platform sync.

- [x] **CV1 — LOW — convention** — `api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.IntegrationTests/TenantScopingTests.cs:119`
  The renamed organization-scoped endpoint is still described and tested as a current-user resource. Rename the test and its description to `OrganizationConcertRead`.

## Incremental review — current-main merge

No new findings. The merge preserves main's permission-specific read repositories, removes both active-tenant profile ID resolver APIs, and keeps tenant resolution in the Artist/Venue services and Concert-owned projections. B2B Web builds with 0 warnings and 0 errors; Artist, Venue, and Concert unit suites pass 18/18, 19/19, and 229/229.

## Incremental review — package cutover compatibility

No new findings. The temporary legacy HTTP aliases preserve published clients while canonical organization routes and active-tenant application-service semantics remain authoritative. The Venue integration-test project builds with 0 errors. The three affected frontend carves produced no compiler error but exceeded the 15-minute local command limit; exact-head CI remains the package-isolated verification gate.
