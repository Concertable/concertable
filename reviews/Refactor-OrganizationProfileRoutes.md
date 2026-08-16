# Code review — Refactor/OrganizationProfileRoutes

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `d39368b45c121282f3ae1e7f77564bff355ad975`  _(2026-08-16)_

**Security-reviewed up to commit:** `d39368b45c121282f3ae1e7f77564bff355ad975`  _(2026-08-16)_

> Range reviewed: `89361e9..d39368b` (3 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **BUG1 — HIGH — correctness** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Data/Configurations/ArtistReadModelConfiguration.cs:14`
  The Artist and Venue projections still enforce unique `UserId` indexes, so one human cannot create profiles for multiple organizations even though ownership is now tenant-based. Remove both unique user indexes and re-scaffold the Concert migration.

- [x] **SEED1 — HIGH — seeding** — `api/Concertable.B2B/src/Seed/Concertable.B2B.Seed.Contracts/SeedSpecMappers.cs:11`
  `VenueSeedSpec.ToChangedEvent` leaves the new `TenantId` at `Guid.Empty`, so standalone seed events cannot populate tenant-keyed Venue projections. Map the canonical tenant ID into the event and cover it in the seed-contract tests.
  Resolved by mapping `TenantSeedIds.For(spec.UserId)`; the repository has no seed-contract test project, and the B2B build compiles the source contract while Search intentionally remains on the published package until platform sync.

- [x] **CV1 — LOW — convention** — `api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.IntegrationTests/TenantScopingTests.cs:119`
  The renamed organization-scoped endpoint is still described and tested as a current-user resource. Rename the test and its description to `OrganizationConcertRead`.
