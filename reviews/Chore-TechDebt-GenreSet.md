# Code review — Chore/TechDebt-GenreSet

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `b610d9eeb5d92a0a54f0e33b61b40f8fb356dd63`  `(2026-09-02)`
**Judgment:** `approved`

## Review pass — 2026-09-02 — full

**Candidate base:** `156ae28440710e550b4d74ab688ebfce602d493a`
**Candidate head:** `b610d9eeb5d92a0a54f0e33b61b40f8fb356dd63`
**Candidate branch:** `Chore/TechDebt-GenreSet`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:a89756307f0984a4f8cd3b51df168529cf0adf0a19f6ef8a7ee8cbe0ced22691` `(14 paths)`
**Work-order path:** `reviews/Chore-TechDebt-GenreSet.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

This branch was rebuilt on current base after `EfSet<T>` landed in `Concertable.Kernel` (#926, platform
`0.1.0-alpha.0.1310`). It now purely consumes `EfSet<Genre>`; the earlier standalone `GenreSet` history
is gone.

### Findings

No findings.

- **native/general** (correctness, reuse, simplification, efficiency, error handling): no findings.
  Verified `ArtistEntity` raises exactly one `ArtistChangedDomainEvent` per mutation after the
  `Create`/`Update`/`SyncGenres` collapse; `ConcertDraftService`'s `.ToList()` fixes a real
  multiple-enumeration and `.Count > 0`/`== 0` are equivalent to the old `.Any()`; the
  `OpportunityEntity.Create` two-overload split delegates `Create(..., [])` safely and dropping the
  `genres?.ToList() ?? []` null-guard introduces no NRE (`OpportunityRequest.Genres` and the sync DTO
  are non-nullable `IReadOnlyList<Genre> = []`); `o.Genres.ToList()` in the two projections is required
  (target is `List<Genre>`) and EF-translatable; the only `OpportunityFactory` caller uses the surviving
  id overload.
- **persistence / multitenancy**: `PrimitiveCollection` mapping unchanged, storage identical (same JSON
  column, no migration), read stance untouched, no `IgnoreQueryFilters`.
- **domain-events**: single raise site per `ArtistEntity` mutation.
- **microservice-boundaries**: consumes a Kernel primitive; no cross-module query, no new cross-service
  coupling.
- **csharp-style / csharp-naming**: params tightened `IEnumerable` → `IReadOnlyCollection`, two overloads
  over a nullable default, no unnecessary `this.`.
- **unit-testing / integration-testing**: new domain dedup tests on all three entities
  (`ArtistEntityTests.SyncGenres_DuplicateGenre_IsStoredOnce`, `ConcertEntityTests`,
  `OpportunityEntityTests`; `Create`/`Update` transitively covered via `SyncGenres`); new integration
  test proves the JSON-column round-trip. `EfSet<T>` itself is unit-tested in `Concertable.Kernel.UnitTests`.
- **docs-and-debt**: both `TECH_DEBT.md` genre-entry deletions match the resolution (set-shaped type +
  a test proving a duplicate cannot be stored); the Concert cross-service saga entry is correctly retained.

The `EfSet<Genre>` proof-of-concept ran the full B2B integration matrix (Concert 177, Artist 22) before
this rebuild; CI on this head re-runs it.
