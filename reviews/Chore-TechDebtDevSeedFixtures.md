# Code review — Chore/TechDebtDevSeedFixtures

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `67c4819190472c6e9153830c777843e9745b4a25`  _(2026-08-29)_

> Range reviewed: `7629c9ae0..67c481919` (5 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [ ] **NAT1 — native** — no findings. Layer-1 general review (correctness, reuse, simplification,
  efficiency, error handling) over `7629c9ae0..33ed717bb` at medium effort surfaced nothing.
- [x] **CV1 — LOW — test coverage** — `app/shared/src/features/venues/api/venueApi.ts`,
  `app/shared/src/features/artists/api/artistApi.ts`,
  `app/web/b2b/shared/src/features/organizations/api/organizationApi.ts`
  `getMyVenue`/`getMyArtist`/`organizationApi.get` had zero test coverage before this branch — the
  `frontend-testing` skill lists "API modules … with the HTTP client mocked at its module boundary" as
  earning a unit test, and this diff changes exactly the branch (absent-value handling) that was the live
  bug. Fixed: added `getMyVenue`/`getMyArtist` null-vs-present cases to the existing `venueApi.test.ts` /
  `artistApi.test.ts`, and a new `organizationApi.test.ts` covering the 204→null and 200→data branches.

No Lens B/C/D findings — no service-boundary, module-boundary or seeding-standard issues; the Auth
credential addition follows the existing direct-write seeding pattern and the B2B fixture change follows
the "get an authenticated client from the fixture" convention. No Lens E findings beyond CV1 — the diff
matches `csharp-style`/`csharp-naming`/`typescript-style`/`contract-naming`/`http-layer`/
`tiered-shared-code`/`docs-and-debt` as invoked.
