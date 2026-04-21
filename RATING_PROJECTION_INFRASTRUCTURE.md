# Rating Projection Infrastructure — Implementation Plan

> **Status: Not started (2026-04-21)**
>
> **Blocker for:** `ArtistRepository` independence from `IReadDbContext`. Currently `ArtistRepository`
> injects `IRatingSpecification<ArtistEntity>` and reads `readDb.Reviews` for rating aggregation —
> a cross-module query that violates the module boundary. This plan fixes it properly and establishes
> the pattern that Venue and Concert will replicate exactly.
>
> **Position in overall plan:** Do this between Artist Stage 1 step 8 (`IArtistModule`) and
> continuing Venue extraction. Once this lands, `ArtistRepository` no longer needs `IReadDbContext`
> for ratings, and `IRatingSpecification<ArtistEntity>` is removed from it entirely.

---

## What this solves

```
Before:
  ArtistRepository → IReadDbContext.Reviews (cross-module)
                   → IRatingSpecification<ArtistEntity> (Search module concern)

After:
  ArtistRepository → ArtistDbContext.ArtistRatingProjections (own context only)
```

**Permanent homes for every new moving piece.** Nothing parked in `Concertable.Infrastructure` or
`Concertable.Core` "for now" — each file lives in the project that will still exist after the
monolith is gone:

| Piece | Permanent home | Why |
|---|---|---|
| `IIntegrationEvent*` interfaces | `Concertable.Shared.Domain` | Primitives every module references |
| `InProcessIntegrationEventBus` | `Concertable.Data.Infrastructure` | Shared infra — survives with `ReadDbContext`/`AuditInterceptor` until the monolith boundary itself dissolves. Swaps for `OutboxIntegrationEventBus` later in place. |
| `ReviewSubmittedEvent` | `Concertable.Concert.Contracts` | Cross-module event — Concert owns the fact |
| `ReviewCreatedDomainEvent` | `Concertable.Concert.Domain` | Intra-module domain event raised by `ReviewEntity` |
| `ReviewCreatedDomainEventHandler` | `Concertable.Concert.Infrastructure` | Module's own infrastructure — never goes through `Concertable.Infrastructure` |
| `AddConcertModule()` | `Concertable.Concert.Infrastructure` | DI entry point for Concert module from day one — accretes more registrations as Concert extracts |
| `ArtistRatingProjection` | `Concertable.Artist.Domain` | Artist owns its read projections |
| `ArtistReviewProjectionHandler` | `Concertable.Artist.Infrastructure` | Artist owns the handler that maintains its projection |

`IReadDbContext` + `IRatingSpecification` stay alive for the Search module (header repos), but
`ArtistRepository` stops needing them entirely. Same outcome for `VenueRepository` and
`ConcertRepository` when their turns come.

---

## The pattern (established once, replicated per module)

```
ReviewEntity.Create() raises ReviewCreatedDomainEvent
         │
         ▼  (DomainEventDispatchInterceptor fires after ApplicationDbContext.SaveChangesAsync)
ReviewCreatedDomainEventHandler (Concertable.Infrastructure)
         │
         ├── publishes ReviewSubmittedEvent (IIntegrationEvent) via IIntegrationEventBus
         │
         ▼  (InProcessIntegrationEventBus dispatches to all registered handlers)
         ├── ArtistReviewProjectionHandler (Artist.Infrastructure)
         │       → upserts ArtistRatingProjection in ArtistDbContext
         │
         └── VenueReviewProjectionHandler (Venue.Infrastructure — future)
                 → upserts VenueRatingProjection in VenueDbContext
```

---

## New concepts and where they live

| Type | Project | Notes |
|---|---|---|
| `IIntegrationEvent` | `Concertable.Shared` | Marker interface, parallel to `IDomainEvent` |
| `IIntegrationEventHandler<T>` | `Concertable.Shared` | Parallel to `IDomainEventHandler<T>` |
| `IIntegrationEventBus` | `Concertable.Shared` | Publish-only, in-process for now |
| `InProcessIntegrationEventBus` | `Concertable.Data.Infrastructure` | Resolves handlers from DI, same pattern as `DomainEventDispatcher` |
| `ReviewSubmittedEvent` | `Concertable.Concert.Contracts` *(new stub project)* | Cross-module event; lives in Contracts so Artist/Venue can reference it without Concert internals |
| `ReviewCreatedDomainEvent` | `Concertable.Concert.Domain` *(new stub project)* | Internal domain event raised by `ReviewEntity` — lives with the entity's eventual home |
| `ReviewCreatedDomainEventHandler` | `Concertable.Concert.Infrastructure` *(new stub project)* | Converts domain event → integration event. Permanent home — never lived in `Concertable.Infrastructure` |
| `ArtistRatingProjection` | `Concertable.Artist.Domain` | Projection entity owned by Artist |
| `ArtistReviewProjectionHandler` | `Concertable.Artist.Infrastructure` | Handles `ReviewSubmittedEvent`, upserts projection |

---

## Step-by-step

### Step 1 — Integration event interfaces in `Concertable.Shared`

Three new interfaces, parallel to the existing domain event set:

```csharp
// Concertable.Shared.Domain/IIntegrationEvent.cs
public interface IIntegrationEvent { }

// Concertable.Shared.Domain/IIntegrationEventHandler.cs
public interface IIntegrationEventHandler<TEvent> where TEvent : IIntegrationEvent
{
    Task HandleAsync(TEvent @event, CancellationToken ct = default);
}

// Concertable.Shared.Domain/IIntegrationEventBus.cs
public interface IIntegrationEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IIntegrationEvent;
}
```

No new project. Three new files in `Concertable.Shared.Domain`.

---

### Step 2 — In-process bus in `Concertable.Data.Infrastructure`

```csharp
// Concertable.Data.Infrastructure/Events/InProcessIntegrationEventBus.cs
public class InProcessIntegrationEventBus(IServiceProvider sp) : IIntegrationEventBus
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IIntegrationEvent
    {
        var handlers = sp.GetServices<IIntegrationEventHandler<TEvent>>();
        foreach (var handler in handlers)
            await handler.HandleAsync(@event, ct);
    }
}
```

Register in `Concertable.Data.Infrastructure/Extensions/ServiceCollectionExtensions.cs`:

```csharp
services.AddScoped<IIntegrationEventBus, InProcessIntegrationEventBus>();
```

This registration is shared infrastructure — lives alongside `DomainEventDispatcher` registration.

> **Future swap point.** When you add an outbox, replace `InProcessIntegrationEventBus` with
> `OutboxIntegrationEventBus`. Every handler, every event, every module stays unchanged.

---

### Step 3 — Stub Concert module projects (Contracts + Domain + Infrastructure)

We need permanent homes for the cross-module event *and* the handler that publishes it. Rather
than park anything in `Concertable.Infrastructure` (which will eventually be deleted), stand up
the three Concert projects we'll need anyway. They start as minimal stubs.

```
api/Modules/Concert/
  Concertable.Concert.Contracts/          ← ReviewSubmittedEvent (cross-module)
  Concertable.Concert.Domain/             ← ReviewCreatedDomainEvent (intra-module)
  Concertable.Concert.Infrastructure/     ← ReviewCreatedDomainEventHandler
```

**`Concertable.Concert.Contracts.csproj`** — references only `Concertable.Shared.Domain`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\Concertable.Shared.Domain\Concertable.Shared.Domain.csproj" />
  </ItemGroup>
</Project>
```

**`Concertable.Concert.Domain.csproj`** — references `Concert.Contracts` + `Shared.Domain`:

```xml
<ItemGroup>
  <ProjectReference Include="..\Concertable.Concert.Contracts\Concertable.Concert.Contracts.csproj" />
  <ProjectReference Include="..\..\..\Concertable.Shared.Domain\Concertable.Shared.Domain.csproj" />
</ItemGroup>
```

**`Concertable.Concert.Infrastructure.csproj`** — references `Concert.Contracts`, `Concert.Domain`,
and `Concertable.Data.Infrastructure` (for DI + interceptor plumbing later). No `Concert.Application`
yet — we don't need it for this slice.

The event:

```csharp
// Concertable.Concert.Contracts/Events/ReviewSubmittedEvent.cs
namespace Concertable.Concert.Contracts.Events;

public record ReviewSubmittedEvent(
    int ArtistId,
    int VenueId,
    int ConcertId,
    double Stars) : IIntegrationEvent;
```

`ArtistId` and `VenueId` are both carried on the event so Artist and Venue handlers each get what
they need from a single publish. `ConcertId` is there for when Concert gets its own projection.

Add all three projects to the solution file.

---

### Step 4 — `ReviewCreatedDomainEvent` + wire `ReviewEntity` as `IEventRaiser`

`ReviewEntity` currently raises no domain events. Add the event — lives in `Concert.Domain` (its
permanent home, next to `ReviewEntity` once Concert extraction moves the entity over).

```csharp
// Concertable.Concert.Domain/Events/ReviewCreatedDomainEvent.cs
namespace Concertable.Concert.Domain.Events;

public record ReviewCreatedDomainEvent(
    Guid TicketId,
    int ArtistId,
    int VenueId,
    int ConcertId,
    double Stars) : IDomainEvent;
```

Updating `ReviewEntity` to implement `IEventRaiser` requires knowing `ArtistId`, `VenueId`, and
`ConcertId` at creation time. Currently `ReviewEntity.Create(ticketId, stars, details)` doesn't
have these — they live on `Ticket → Concert → Booking → Application`. We have two options:

**Option A — Enrich `ReviewEntity.Create` signature:**
Pass `artistId`, `venueId`, `concertId` into `Create()`. The caller (`ConcertReviewService`) already
fetches the ticket; it can also fetch these IDs. Keeps `ReviewEntity` self-contained.

**Option B — Publish from the service layer directly:**
`ConcertReviewService.CreateAsync` injects `IIntegrationEventBus` and publishes `ReviewSubmittedEvent`
directly after `SaveChangesAsync()`. No domain event, no `IEventRaiser` on `ReviewEntity`. Simpler
but skips the domain event step.

**Decision: Option A.** Keeps the publish point on the entity where it belongs and avoids making
the service responsible for cross-module notification. The additional IDs are already available
in `ConcertReviewService` via the ticket lookup.

`ReviewEntity` (still in `Concertable.Core` for now — moves to `Concert.Domain` during Concert
extraction) gains a project reference to `Concertable.Concert.Domain` so it can raise the event.
Domain → Domain reference is fine; `Concertable.Core` already references `Artist.Domain` and
`Identity.Domain` under the same principle (§2 of `ARTIST_MODULE_REFACTOR.md`).

```csharp
[Table("Reviews")]
public class ReviewEntity : IIdEntity, IEventRaiser
{
    public int Id { get; private set; }
    public Guid TicketId { get; private set; }
    public byte Stars { get; private set; }
    public string? Details { get; private set; }
    public TicketEntity Ticket { get; set; } = null!;

    private readonly EventRaiser _events = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _events.DomainEvents;
    public void ClearDomainEvents() => _events.Clear();

    private ReviewEntity() { }

    public static ReviewEntity Create(Guid ticketId, byte stars, string? details,
        int artistId, int venueId, int concertId)
    {
        ValidateStars(stars);
        var review = new ReviewEntity { TicketId = ticketId, Stars = stars, Details = details };
        review._events.Raise(new ReviewCreatedDomainEvent(ticketId, artistId, venueId, concertId, stars));
        return review;
    }

    private static void ValidateStars(byte stars)
    {
        if (stars is < 1 or > 5)
            throw new DomainException("Stars must be between 1 and 5.");
    }
}
```

Update `ConcertReviewService.CreateAsync` to resolve `artistId`, `venueId`, `concertId` from the
ticket and pass them into `Create(...)`. The ticket query already joins through Concert/Booking —
select these IDs out of that query rather than making extra round trips.

---

### Step 5 — `ReviewCreatedDomainEventHandler` in `Concert.Infrastructure`

This handler sits at the boundary — converts the internal domain event to the cross-module
integration event. Permanent home; never lived in `Concertable.Infrastructure`.

```csharp
// Concertable.Concert.Infrastructure/Events/ReviewCreatedDomainEventHandler.cs
namespace Concertable.Concert.Infrastructure.Events;

internal class ReviewCreatedDomainEventHandler(IIntegrationEventBus bus)
    : IDomainEventHandler<ReviewCreatedDomainEvent>
{
    public Task HandleAsync(ReviewCreatedDomainEvent e, CancellationToken ct = default) =>
        bus.PublishAsync(new ReviewSubmittedEvent(e.ArtistId, e.VenueId, e.ConcertId, e.Stars), ct);
}
```

Stand up `AddConcertModule(IServiceCollection)` in `Concertable.Concert.Infrastructure/Extensions/
ServiceCollectionExtensions.cs` — the permanent DI entry point for the Concert module. Even though
Concert is not extracted yet, this extension owns the registration from day one:

```csharp
public static IServiceCollection AddConcertModule(this IServiceCollection services)
{
    services.AddScoped<IDomainEventHandler<ReviewCreatedDomainEvent>, ReviewCreatedDomainEventHandler>();
    return services;
}
```

Call `services.AddConcertModule()` from `Program.cs` alongside the other `AddXModule()` calls.
When Concert is eventually extracted, `AddConcertModule()` already exists and just accretes more
registrations — no refactor of the wiring surface needed.

---

### Step 6 — `ArtistRatingProjection` entity + EF config

```csharp
// Concertable.Artist.Domain/ArtistRatingProjection.cs
namespace Concertable.Artist.Domain;

public class ArtistRatingProjection
{
    public int ArtistId { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
```

EF config stays in `Concertable.Data.Infrastructure/Data/Configurations/ArtistEntityConfiguration.cs`
(same file as `ArtistEntityConfiguration`, or a new `ArtistRatingProjectionConfiguration` alongside
it — either is fine):

```csharp
public class ArtistRatingProjectionConfiguration : IEntityTypeConfiguration<ArtistRatingProjection>
{
    public void Configure(EntityTypeBuilder<ArtistRatingProjection> b)
    {
        b.ToTable("ArtistRatingProjections");
        b.HasKey(p => p.ArtistId);
    }
}
```

`ArtistDbContext` adds the DbSet and applies the config:

```csharp
public DbSet<ArtistRatingProjection> ArtistRatingProjections => Set<ArtistRatingProjection>();

// in OnModelCreating:
b.ApplyConfiguration(new ArtistRatingProjectionConfiguration());
```

Add EF migration for the new table.

---

### Step 7 — `ArtistReviewProjectionHandler` in `Artist.Infrastructure`

```csharp
// Concertable.Artist.Infrastructure/Handlers/ArtistReviewProjectionHandler.cs
internal class ArtistReviewProjectionHandler(ArtistDbContext context)
    : IIntegrationEventHandler<ReviewSubmittedEvent>
{
    public async Task HandleAsync(ReviewSubmittedEvent e, CancellationToken ct = default)
    {
        var projection = await context.ArtistRatingProjections
            .FirstOrDefaultAsync(p => p.ArtistId == e.ArtistId, ct);

        if (projection is null)
        {
            context.ArtistRatingProjections.Add(new ArtistRatingProjection
            {
                ArtistId = e.ArtistId,
                AverageRating = e.Stars,
                ReviewCount = 1
            });
        }
        else
        {
            var total = projection.AverageRating * projection.ReviewCount + e.Stars;
            projection.ReviewCount++;
            projection.AverageRating = Math.Round(total / projection.ReviewCount, 1);
        }

        await context.SaveChangesAsync(ct);
    }
}
```

Register in `AddArtistModule()`:

```csharp
services.AddScoped<IIntegrationEventHandler<ReviewSubmittedEvent>, ArtistReviewProjectionHandler>();
```

`Concertable.Artist.Infrastructure.csproj` gains a project reference to `Concertable.Concert.Contracts`.

---

### Step 8 — Update `ArtistRepository` + `QueryableArtistMappers`

`ArtistRepository` drops its `IReadDbContext` injection for ratings and its `IRatingSpecification`
injection entirely. The read-path DTO queries now join against the projection.

```csharp
// ArtistRepository — updated constructor
public ArtistRepository(ArtistDbContext context, IReadDbContext readDb) : base(context)
{
    this.readDb = readDb;
}
// Note: readDb is still needed for GetIdByUserIdAsync (reads readDb.Artists scalar).
// If that moves to context.Artists, readDb can be dropped entirely.
```

`QueryableArtistMappers` updated — accepts `IQueryable<ArtistRatingProjection>` instead of
`IQueryable<RatingAggregate>`:

```csharp
public static IQueryable<ArtistSummaryDto> ToSummaryDto(
    this IQueryable<ArtistEntity> query,
    IQueryable<ArtistRatingProjection> ratings) =>
    from a in query
    join r in ratings on a.Id equals r.ArtistId into rg
    from rating in rg.DefaultIfEmpty()
    select new ArtistSummaryDto
    {
        Id = a.Id,
        Name = a.Name,
        Avatar = a.Avatar,
        Rating = (double?)rating.AverageRating ?? 0.0,
        Genres = ...
    };
```

`GetSummaryAsync` and `GetDtoByIdAsync` in `ArtistRepository` now pass
`context.ArtistRatingProjections` instead of `ratingSpecification.ApplyAggregate(readDb.Reviews)`.

Both queries stay on `ArtistDbContext` — no cross-context join. LinqKit's `AsExpandable()` is no
longer needed in these mappers (it was required to splice in the `IRatingSpecification` expression;
with a plain join against a same-context DbSet it's unnecessary).

---

### Step 9 — Backfill migration

The `ArtistRatingProjections` table will be empty after the EF migration runs against an existing
database. Add a raw SQL statement in the migration's `Up()` to seed from existing reviews:

```csharp
migrationBuilder.Sql(@"
    INSERT INTO ArtistRatingProjections (ArtistId, AverageRating, ReviewCount)
    SELECT
        oa.ArtistId,
        ROUND(AVG(CAST(r.Stars AS FLOAT)), 1),
        COUNT(*)
    FROM Reviews r
    JOIN Tickets t ON r.TicketId = t.Id
    JOIN Concerts c ON t.ConcertId = c.Id
    JOIN ConcertBookings cb ON c.BookingId = cb.Id
    JOIN OpportunityApplications oa ON cb.ApplicationId = oa.Id
    GROUP BY oa.ArtistId
");
```

This runs once. After that, events maintain it.

---

### Step 10 — Cleanup

- Remove `IRatingSpecification<ArtistEntity>` from `ArtistRepository` constructor and DI registration.
- Verify `ArtistRepository` no longer calls `readDb.Reviews` anywhere.
- `IReadDbContext` injection on `ArtistRepository` can be removed entirely if `GetIdByUserIdAsync`
  is also moved to read from `context.Artists` (it already can — `ArtistDbContext` owns `Artists`).
  Do that here and drop `IReadDbContext` from `ArtistRepository` completely.
- `QueryableArtistMappers` no longer imports `LinqKit` (remove the package ref from
  `Artist.Infrastructure.csproj` if it's the only consumer).

---

## Venue and Concert — explicitly deferred

**Don't add `VenueRatingProjection` or `ConcertRatingProjection` as part of this work.**

The Artist implementation *is* the standard. The pattern is two files per module: projection entity
in `X.Domain` + handler in `X.Infrastructure`. To add Venue/Concert projections now would require
scaffolding `Venue.Domain` + `VenueDbContext` / `Concert.Domain` + `ConcertDbContext` — which is
most of those modules' extractions, done out of order just to hang a projection on them.

**What happens in the meantime:**

- `VenueRepository` and `ConcertRepository` keep using `IRatingSpecification<T>` + `readDb.Reviews`
  as they do today. That's exactly what `IReadDbContext` exists for — the transitional window
  (MM_NORTH_STAR §Corollary 1).
- `ReviewSubmittedEvent` already carries `VenueId` and `ConcertId`, so when the consumer modules
  land no event-schema change is needed. Publishing is already correct.

**When Venue extraction happens:** as part of that plan, add `VenueRatingProjection` +
`VenueReviewProjectionHandler` + DbSet + config + backfill. Same recipe as Artist here. Two files
+ a DI registration + a migration. No new infrastructure needed.

**When Concert extraction happens:** same again for `ConcertRatingProjection`. At that point
`ReviewCreatedDomainEvent` and `ReviewCreatedDomainEventHandler` — which already live in
`Concert.Domain` / `Concert.Infrastructure` stubs from Step 3 — get joined by the rest of Concert.

**Action item:** ensure `VENUE_MODULE_REFACTOR.md` and the eventual `CONCERT_MODULE_REFACTOR.md`
(when authored) include a checklist item for standing up the projection + handler + backfill. That
is where the standard gets enforced for those modules — not in code that doesn't exist yet.

---

## Acceptance criteria

- `ArtistRepository` constructor has no `IRatingSpecification<ArtistEntity>` parameter.
- `ArtistRepository` does not call `readDb.Reviews` anywhere.
- `ArtistRepository` constructor has no `IReadDbContext` parameter (once `GetIdByUserIdAsync` moves).
- `dotnet test --filter "FullyQualifiedName~ArtistApiTests"` — 17 pass (rating values correct).
- Full suite — matches baseline (129 integration + pre-existing 4 E2E failures).
- A new review submitted via `ConcertReviewService.CreateAsync` results in the `ArtistRatingProjections`
  row being created/updated within the same request.

---

## Reference

- `ARTIST_MODULE_REFACTOR.md` — this plan unblocks step 8 completion + full IReadDbContext removal.
- `MM_NORTH_STAR.md` §Corollary 1 — `IReadDbContext` is a transitional shim; this removes one consumer.
- `MM_NORTH_STAR.md` §Corollary 5 — each module owns its own read projections.
- `Concertable.Data.Infrastructure/Events/DomainEventDispatcher.cs` — pattern for `InProcessIntegrationEventBus`.
- `Concertable.Data.Infrastructure/Data/DomainEventDispatchInterceptor.cs` — how domain events reach handlers.
