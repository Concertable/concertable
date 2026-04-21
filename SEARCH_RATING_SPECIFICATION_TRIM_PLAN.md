# Search `IRatingSpecification` Trim Plan

## Context

`ArtistRatingProjection` now stores `(ArtistId, AverageRating, ReviewCount)` denormalised, updated by
`ArtistReviewProjectionHandler` on every `ReviewSubmittedEvent`. That means **reads no longer need to
aggregate `ReviewEntity` at query time for artists** — `ArtistHeaderRepository` can join the projection
table the same way `ArtistRepository` already does.

Current `IRatingSpecification<T>` surface:

```csharp
public interface IRatingSpecification<T> where T : class, IIdEntity
{
    IQueryable<RatingAggregate> ApplyAggregate(IQueryable<ReviewEntity> reviews);
    IQueryable<double?> ApplyAverage(IQueryable<ReviewEntity> reviews, int id);
}
```

`ApplyAggregate` builds a per-entity average by walking `Review → Ticket → Concert → Booking → Application →
ArtistId | VenueId` (or `ConcertId`). `ApplyAverage` is a single-entity variant used by rating repos. Both
are the legacy row-scan pattern the projection replaces.

## What "trimmed" looks like

Only **Artist** has a projection today. Venue and Concert still scan `ReviewEntity` at read time — those
are the next projection targets (see `RATING_PROJECTION_INFRASTRUCTURE.md` / `MM_NORTH_STAR.md`) but are
**out of scope for this pass**.

So this pass is narrow: **retire `IRatingSpecification<ArtistEntity>` only**, leaving Venue + Concert
implementations intact.

### After this pass

```csharp
// IRatingSpecification.cs — unchanged shape, fewer implementations
public interface IRatingSpecification<T> where T : class, IIdEntity
{
    IQueryable<RatingAggregate> ApplyAggregate(IQueryable<ReviewEntity> reviews);
    IQueryable<double?> ApplyAverage(IQueryable<ReviewEntity> reviews, int id);
}
```

- `VenueRatingSpecification`, `ConcertRatingSpecification` — **keep as-is**.
- `ArtistRatingSpecification` — **delete**.
- `ArtistHeaderRepository` — stop taking `IRatingSpecification<ArtistEntity>`; join `ArtistRatingProjection`
  from the Artist module read side instead (see "Cross-module read wrinkle" below).
- `QueryableArtistHeaderMappers.ToHeaderDtos` — swap `IQueryable<RatingAggregate>` param for
  `IQueryable<ArtistRatingProjection>`, mirror `QueryableArtistMappers.ToSummaryDto` pattern
  (`rating == null ? 0.0 : rating.AverageRating`).
- `ArtistRatingRepository` (legacy, in `Concertable.Infrastructure`) — still uses
  `IRatingSpecification<ArtistEntity>` for its queries. Either:
  - **Option 1 (preferred):** rewrite it to read from `ArtistRatingProjection` directly. Same shape of fix
    as the header repo — trivial since the projection already holds `AverageRating` and `ReviewCount`.
  - **Option 2:** leave as-is, kick the can to the rating-pipeline rewrite. Risk: the legacy repo will
    silently disagree with the projection (projection updates via event, legacy repo recomputes from
    `ReviewEntity`). Low traffic today but a correctness bug waiting to happen.
  - Recommendation: Option 1. The projection is already the source of truth; any consumer that reads
    "artist rating" should hit it.

## Cross-module read wrinkle — where does `ArtistRatingProjection` live for Search?

`ArtistRatingProjection` is owned by `Concertable.Artist.Domain`. Its `DbSet` lives on `ArtistDbContext`
(write side) and — crucially — it also needs to be visible to Search's `IReadDbContext`.

Check: does `ReadDbContext` already expose `IQueryable<ArtistRatingProjection>`?

- If **yes**, `ArtistHeaderRepository` just uses `context.ArtistRatingProjections` and we're done.
- If **no**, we need to add it. `IReadDbContext` already references `Concertable.Artist.Domain` (entity
  escape hatch per CLAUDE.md rule 3), so adding `IQueryable<ArtistRatingProjection>` is consistent with
  how other cross-module entities are exposed. Action: add `IQueryable<ArtistRatingProjection>
  ArtistRatingProjections { get; }` to `IReadDbContext` + wire it in `ReadDbContext`.

**Verify during implementation, not now.**

## Step list (once we're on this task)

1. **Inspect `IReadDbContext`** — confirm/add `ArtistRatingProjections` exposure.
2. **Rewrite `QueryableArtistHeaderMappers.ToHeaderDtos`** to take
   `IQueryable<ArtistRatingProjection>` instead of `IQueryable<RatingAggregate>`. Mirror
   `QueryableArtistMappers.ToSummaryDto` exactly for the null-coalesce.
3. **Rewrite `ArtistHeaderRepository`** — drop `IRatingSpecification<ArtistEntity>`, inject nothing
   extra (the projection DbSet comes off `IReadDbContext` it already has). Both `SearchAsync` and
   `GetByAmountAsync` now pass `context.ArtistRatingProjections` to the mapper.
4. **Rewrite `ArtistRatingRepository`** (legacy) — read `AverageRating` / `ReviewCount` off
   `ArtistRatingProjection` directly. Grep for its callers to confirm method signatures; don't change
   the repo's public shape, just the query implementation.
5. **Delete `ArtistRatingSpecification.cs`**.
6. **Remove DI registration** — the line
   `services.AddSingleton<IRatingSpecification<ArtistEntity>, ArtistRatingSpecification>();` in
   `Concertable.Search.Infrastructure/Extensions/ServiceCollectionExtensions.cs`.
7. **Remove `IRatingSpecification<ArtistEntity>` from `ConcertRepository`** (it's injected there too —
   check whether it's still used after the header repo changes; if yes, leave; if no, trim).
8. **Build** the solution clean. **Run** `Concertable.Infrastructure.UnitTests` +
   `Concertable.Web.IntegrationTests`. Artist header/search endpoints should return the same rating
   values as before the change.

## Explicitly out of scope

- **Venue / Concert projections.** Same refactor will apply when their projections land, but that's
  governed by `RATING_PROJECTION_INFRASTRUCTURE.md`, not this plan.
- **Retiring `RatingAggregate` / `IRatingSpecification<T>` entirely.** They stick around for Venue +
  Concert until those get their projections.
- **Moving `IRatingSpecification` out of `Search.Application`.** Keep its current home; it's Search's
  query-side abstraction for the modules that still need row-scan aggregation.

## After-merge memory update

- Update `project_modular_monolith.md` → strike the Artist rating-spec line item from the blocker list,
  note that `ArtistHeaderRepository` + `ArtistRatingRepository` now read from the projection.
- If `RATING_PROJECTION_INFRASTRUCTURE.md` has a "next projection consumer" checklist, tick the Search
  header path off it.
