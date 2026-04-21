# Artist Module Extraction — Implementation Plan

> **State of the world (2026-04-21)** — Identity is done. Search is done. Artist is next.
> This file supersedes the previous stale plan (pre-Identity, pre-denormalization).
>
> **Update (2026-04-21, post-design-review):** Original plan §11.a (keep `ArtistEntity.ReviewIdSelector`
> and reference `Concertable.Core` from `Artist.Domain`) conflicts with §4/§8 (keep
> `OpportunityApplicationEntity.Artist` nav in `Concertable.Core`). The two together create a project
> reference cycle. Decision: kill `IReviewable<T>` (the `static abstract` entity member driving the
> cycle) and replace the generic `RatingSpecification<T>` / `ReviewSpecification<T>` /
> `IReviewRepository<T>` triple with per-module concrete classes (Stage 0). Then do the Artist
> extraction as Stage 1.
>
> **Update (2026-04-21, post-pivot):** Earlier iterations of Stage 0 proposed deleting
> `IRatingSpecification<T>` along with the other generics and inlining its body at call sites. That
> thrashed the code (duplicated `.GroupBy/.Select` across ~15 sites) for no architectural gain. Final
> shape:
> - **Keep `IRatingSpecification<T>`** — move to `Concertable.Search.Application/Interfaces/`, drop
>   only the `where T : IReviewable<T>` constraint (that was the cycle driver), keep a light
>   `where T : class, IIdEntity` constraint for intent (matches the other Search interfaces). `T`
>   is a DI tag — the interface methods don't consume it. 3 concrete impls
>   (`ArtistRatingSpecification`, `VenueRatingSpecification`, `ConcertRatingSpecification`) live in
>   `Search.Infrastructure`.
> - **Delete `IReviewSpecification<T>` entirely** — Search has no consumer; its only user was the
>   generic `ReviewRepository<T>` wrapper, which is also being deleted. Reviews are per-module reads
>   (filter `Reviews` by `artistId`/`venueId`/`concertId`) — the 3 concrete review repos inline the
>   1-line `.Where(...)` themselves.

---

## Staged approach

- **Stage 0 — Kill `IReviewable<T>` + the generic review/rating/wrapper triple.** Prerequisite to
  Artist. No module boundaries move; replace the cross-module entity coupling with 3 concrete
  rating specs in Search + 3 inlined review repos per module.
- **Stage 1 — Artist module extraction.** As originally designed (§2-§10 below), now free of the
  cycle. `Concertable.Core` takes a project reference to `Concertable.Artist.Domain` for the
  `OpportunityApplicationEntity.Artist` nav; `Artist.Domain` does *not* need to reference
  `Concertable.Core`.

---

## Stage 0 — Kill IReviewable + concretize reviews/ratings

### Why each piece goes / stays

| Piece | Status | Rationale |
|---|---|---|
| `IReviewable<T>` (`static abstract ReviewIdSelector`) | **delete** | Source of the cycle. Entity implementation body traverses `Ticket → Concert → Booking → Application`, pulling Concert-module types into whatever project owns the entity. |
| Generic `RatingSpecification<T>` + `ReviewSpecification<T>` | **delete** | Only exist to consume `IReviewable<T>`. Replace with 3 concrete rating specs; review has no generic impl. |
| Generic `IReviewRepository<T>` + `ReviewRepository<T>` | **delete** | Thin wrapper the 3 concrete review repos delegate through. Absorbed directly. |
| `IRatingSpecification<T>` | **keep**, relocate, drop constraint | 15+ consumers across Search headers, entity repos, rating repos, and a unit test. Phantom `T` keeps DI disambiguation. Lives alongside the rest of Search's interfaces. |
| `IReviewSpecification<T>` | **delete** | No Search consumer. Sole user was the generic `ReviewRepository<T>` wrapper, also being deleted. Reviews are per-module reads. |

### Files to delete

- `api/Concertable.Core/Interfaces/IReviewable.cs`
- `api/Concertable.Application/Interfaces/Search/IReviewSpecification.cs`
- `api/Concertable.Application/Interfaces/Search/IRatingSpecification.cs` *(old location — moved, see below)*
- `api/Concertable.Application/Interfaces/IReviewRepository.cs` (generic)
- `api/Concertable.Infrastructure/Repositories/Review/ReviewRepository.cs` (generic)
- `api/Modules/Search/Concertable.Search.Infrastructure/Specifications/RatingSpecification.cs` (generic)
- `api/Modules/Search/Concertable.Search.Infrastructure/Specifications/ReviewSpecification.cs` (generic)
- `api/Concertable.Infrastructure/Mappers/ReviewSelectors.cs` *(transient file from the first-pass inlining attempt — not needed with the final shape; delete if it exists)*

### Entity cleanup — strip `IReviewable<TSelf>` + `ReviewIdSelector`

- `api/Concertable.Core/Entities/ArtistEntity.cs` — drop `IReviewable<ArtistEntity>` + selector.
- `api/Concertable.Core/Entities/VenueEntity.cs` — drop `IReviewable<VenueEntity>` + selector.
- `api/Concertable.Core/Entities/ConcertEntity.cs` — drop `IReviewable<ConcertEntity>` + selector.

### Interface relocation

Move `IRatingSpecification<T>` from `Concertable.Application/Interfaces/Search/` to
`Concertable.Search.Application/Interfaces/`. Drop **only** the `where T : IReviewable<T>`
constraint (the cycle driver); keep a light `where T : class, IIdEntity` to document intent and
match the style of other Search interfaces.

This matches `IGeometrySpecification<T>`, `ISearchSpecification<T>`, `ISortSpecification<T>`, etc.,
which are already in `Concertable.Search.Application/Interfaces/`. `IRatingSpecification<T>` +
(now-deleted) `IReviewSpecification<T>` were the last stragglers.

```csharp
// api/Modules/Search/Concertable.Search.Application/Interfaces/IRatingSpecification.cs
public interface IRatingSpecification<T> where T : class, IIdEntity
{
    IQueryable<RatingAggregate> ApplyAggregate(IQueryable<ReviewEntity> reviews);
    IQueryable<double?> ApplyAverage(IQueryable<ReviewEntity> reviews, int id);
}
```

> `T` is purely a DI disambiguation tag — neither method signature consumes it. The constraint is
> documentation, not load-bearing. Unlike `SearchSpecification<TEntity> where TEntity : IEntity, IHasName`,
> where `TEntity` is actually used (`e.Name.Contains(...)`), `IRatingSpecification<T>`'s body
> wouldn't break with any `T`. We keep the weak constraint for consistency only.

### Concrete rating specs — 3 new files in `Search.Infrastructure`

Each hard-codes its own selector inline. No abstractions, no shared helpers.

```csharp
// api/Modules/Search/Concertable.Search.Infrastructure/Specifications/ArtistRatingSpecification.cs
internal class ArtistRatingSpecification : IRatingSpecification<ArtistEntity>
{
    public IQueryable<RatingAggregate> ApplyAggregate(IQueryable<ReviewEntity> reviews) =>
        reviews
            .GroupBy(r => r.Ticket.Concert.Booking.Application.ArtistId)
            .Select(g => new RatingAggregate
            {
                EntityId = g.Key,
                AverageRating = Math.Round(g.Average(r => (double?)r.Stars) ?? 0.0, 1)
            });

    public IQueryable<double?> ApplyAverage(IQueryable<ReviewEntity> reviews, int id) =>
        reviews
            .Where(r => r.Ticket.Concert.Booking.Application.ArtistId == id)
            .GroupBy(_ => 1)
            .Select(g => g.Average(r => (double?)r.Stars));
}
```

Mirror for `VenueRatingSpecification` (`Application.Opportunity.VenueId`) and
`ConcertRatingSpecification` (`Ticket.ConcertId`). These files move with Venue / Concert extractions
later; for Stage 0 they all live in `Search.Infrastructure/Specifications/`.

### Project-reference adjustment

`api/Concertable.Infrastructure/Concertable.Infrastructure.csproj` must reference
`Concertable.Search.Application` so Artist/Venue/Concert/Rating repos can inject
`IRatingSpecification<T>` from its new home.

```xml
<ProjectReference Include="..\Modules\Search\Concertable.Search.Application\Concertable.Search.Application.csproj" />
```

Direction is fine — Infrastructure-layer consumers depending on module-Application is the normal
flow (same as the existing `Identity.Application` reference).

### Call-site rewrites

**3 entity repos** (`ArtistRepository`, `VenueRepository`, `ConcertRepository`) — no shape change,
just the `using` moves from `Concertable.Application.Interfaces.Search` to
`Concertable.Search.Application.Interfaces`. Injection and `.ApplyAggregate(context.Reviews)` calls
are identical.

**3 Search header repos** (`ArtistHeaderRepository`, `VenueHeaderRepository`, `ConcertHeaderRepository`)
— same. `using` change only.

**3 rating repos** (`ArtistRatingRepository`, `VenueRatingRepository`, `ConcertRatingRepository`) —
same. `using` change only; `.ApplyAverage(context.Reviews, id)` unchanged.

**3 review repos** (`ArtistReviewRepository`, `VenueReviewRepository`, `ConcertReviewRepository`) —
**absorb the generic wrapper body directly.** These classes currently delegate to
`IReviewRepository<T>`. Post-change: inject `ApplicationDbContext`, inline the filter, and inline
`.ToDto().ToPaginationAsync(pageParams)` / `.ToSummaryDto().FirstOrDefaultAsync()`:

```csharp
// Example: ArtistReviewRepository
public Task<IPagination<ReviewDto>> GetAsync(int id, IPageParams pageParams) =>
    context.Reviews
        .Where(r => r.Ticket.Concert.Booking.Application.ArtistId == id)
        .OrderByDescending(r => r.Id)
        .ToDto()
        .ToPaginationAsync(pageParams);

public async Task<ReviewSummaryDto> GetSummaryAsync(int id) =>
    await context.Reviews
        .Where(r => r.Ticket.Concert.Booking.Application.ArtistId == id)
        .ToSummaryDto()
        .FirstOrDefaultAsync()
        ?? new ReviewSummaryDto(0, null);
```

`VenueReviewRepository` uses `Application.Opportunity.VenueId`. `ConcertReviewRepository` uses
`Ticket.ConcertId` and keeps its existing `CanReviewAsync` / `AddAsync` / `SaveChangesAsync` logic.

### DI cleanup

`api/Modules/Search/Concertable.Search.Infrastructure/Extensions/ServiceCollectionExtensions.cs`:
- Delete 3 `IReviewSpecification<T>` registrations.
- Swap 3 `IRatingSpecification<T>, RatingSpecification<T>` registrations for the new concrete types:
  ```csharp
  services.AddSingleton<IRatingSpecification<ArtistEntity>, ArtistRatingSpecification>();
  services.AddSingleton<IRatingSpecification<VenueEntity>, VenueRatingSpecification>();
  services.AddSingleton<IRatingSpecification<ConcertEntity>, ConcertRatingSpecification>();
  ```

`api/Concertable.Web/Extensions/ServiceCollectionExtensions.cs`:
- Delete 3 `IReviewRepository<T>` registrations (lines 243-245).

### Unit-test touch-up

`api/Tests/Concertable.Infrastructure.UnitTests/Repositories/VenueRepositoryTests.cs` mocks
`IRatingSpecification<VenueEntity>`. The mock still works after the interface relocation — only the
`using` needs updating from `Concertable.Application.Interfaces.Search` to
`Concertable.Search.Application.Interfaces`.

### Stage 0 acceptance — ✅ complete (2026-04-21)

- Full `dotnet build` succeeds.
- `dotnet test --filter "FullyQualifiedName~ArtistApiTests"` — 17 pass.
- Full `dotnet test` — matches pre-change baseline:
  - Core.UnitTests: all pass
  - Infrastructure.UnitTests: 74 pass
  - Workers.UnitTests: 3 pass
  - Web.IntegrationTests: 129 pass
  - Web.E2ETests: 4 fail (all `ConcertDraftTests`) — **infrastructure-level, unrelated to this refactor.** Baseline verified by re-running against `git stash`'d HEAD: all 4 fail there too with `StripeCliFixture.InitializeAsync` → `TaskCanceledException`. (The earlier plan wrote "2 failures" from memory; actual baseline is 4.)

---

## Stage 1 — Artist module extraction

Run this only after Stage 0 is green. Previously §1-§12 below, updated for the post-Stage-0 world.

### Stage 1 progress (2026-04-21)

Steps 2–4 of §10 landed. State of the tree:

- ✅ 4 Artist projects scaffolded under `api/Modules/Artist/` (Contracts, Domain, Application,
  Infrastructure) with the project references from §2, added to `Concertable.sln`.
- ✅ `ArtistEntity` + `ArtistGenreEntity` moved to `Concertable.Artist.Domain`. Namespace renamed.
- ✅ `ArtistEntity.Applications` inverse nav dropped. `OpportunityApplicationEntityConfiguration`
  side switched to `.WithMany()`.
- ✅ `ArtistEntity` now implements `IHasLocation` alongside the existing `ILocatable<ArtistEntity>` /
  `LocationExpression` (not deleted — will be phased out during per-entity Location work, per user).
- ✅ **Cycle break:** `GenreEntity` moved from `Concertable.Core/Entities/` to
  `Concertable.Shared.Domain/` (namespace `Concertable.Shared`). Its reverse nav collections
  (`ArtistGenres`, `ConcertGenres`, `OpportunityGenres`) were dropped — grep-verified unused at
  runtime. `ConcertGenreEntityConfiguration` flipped `.WithMany(g => g.ConcertGenres)` to
  `.WithMany()`. This means `Artist.Domain` can own `ArtistGenreEntity.Genre : GenreEntity` (via
  Shared.Domain) without taking a `Concertable.Core` project reference — cycle avoided.
- ✅ `ArtistGenreEntity` composite key + relationships configured fluently in a new
  `ArtistGenreEntityConfiguration` (inside `Concertable.Data.Infrastructure/Data/Configurations/
  ArtistEntityConfiguration.cs`). Entity is free of EF Core attributes, keeping `Artist.Domain`
  dependency-lean (Shared.Domain only, no EF Core package ref).
- ✅ `Concertable.Core` → `Concertable.Artist.Domain` project reference added.
  `OpportunityApplicationEntity.cs` updated with `using Concertable.Artist.Domain;`.
- ✅ Global usings updated (`Concertable.Artist.Domain` added to):
  `Concertable.Core`, `Concertable.Infrastructure`, `Concertable.Web`, `Concertable.Application`,
  `Concertable.Seeding`, `Concertable.Data.Infrastructure`,
  `Concertable.Search.Application`, `Concertable.Search.Infrastructure`. `IReadDbContext.cs` got
  `using Concertable.Artist.Domain;` + `using Concertable.Shared;` for `GenreEntity`.
- ✅ Build green. Test baseline held:
  - `ArtistApiTests`: 17 pass.
  - Core/Infrastructure/Workers unit tests + Web integration tests (129): all pass.
  - E2E `ConcertDraftTests` 4 failures — pre-existing Stripe CLI infrastructure issue, unrelated.

**Unplanned side-quest — `GenreEntity` relocation.** Not in the original plan. Surfaced because
`ArtistGenreEntity.Genre : GenreEntity` made `Artist.Domain → Core` unavoidable while we also
needed `Core → Artist.Domain`. Moving `GenreEntity` to `Shared.Domain` is the canonical fix (it's
reference data every module reads; no module owns writes for it). Captured in CLAUDE.md rule 5.

**New invariant captured (CLAUDE.md):** module `Domain` is the only cross-module escape hatch
we're currently accepting (for entity types referenced by `IReadDbContext` and by EF nav props we
haven't detangled yet). Any other cross-module Application/Infrastructure reference requires an
explicit conversation.

**Remaining Stage 1 steps (from §10): 5–13.** Next up is step 5 — move Application layer
(`IArtistService`, `IArtistRepository`, DTOs, Requests, Validators, Mappers) into
`Concertable.Artist.Application` with correct namespaces from the start.

### Stage 1 progress (2026-04-21, cont.) — shared-type foundations for Application move

Before moving the Application layer, extracted shared dependencies out of legacy
`Concertable.Application` so `Artist.Application` can live without a wrong-direction reference to
it. All done, build green.

- ✅ **New `Concertable.Shared.Validation` project.** Owns `ImageValidator` / `BannerImageValidator` /
  `AvatarImageValidator`. FluentValidation + ImageSharp package refs live here; zero internal
  project refs so consumers don't inherit unintended transitive deps. `Concertable.Application`
  references it; `ImageSharp` dropped from `Concertable.Application.csproj`. Added to `Concertable.sln`.
- ✅ **`ImageDto` → `Concertable.Shared.Contracts`** (namespace `Concertable.Shared`). Shared.Contracts
  gained `<FrameworkReference Microsoft.AspNetCore.App>` for `IFormFile`. Legacy `Concertable.Application.DTOs.ImageDto` deleted.
- ✅ **`IIdRepository<T>` in `Shared.Domain`** (parallel to existing `IGuidRepository<T>`), and
  **`IdModuleRepository<TEntity, TContext>` in `Data.Infrastructure/ModuleRepository.cs`** (parallel
  to `GuidModuleRepository`). Legacy `Concertable.Application/Interfaces/IRepository.cs` and
  `Concertable.Infrastructure/Repositories/Repository.cs` **left alone** — 7 non-Artist repos still
  use them until their modules extract.
- ✅ **`IDetails` deleted** (file + `: IDetails` implementations on `ArtistDto`, `VenueDto`,
  `ConcertDto`). It was marker-only polymorphism with zero consumers.
- ✅ **CLAUDE.md**: added rule that modules can ship multiple facades (Identity =
  `IIdentityModule` + `IAuthModule`) — don't force everything through one fat `IXModule`.

### Stage 1 progress (2026-04-21, step 5) — Application layer moved

Step 5 of §10 landed. State:

- ✅ 6 files moved from `Concertable.Application` into `api/Modules/Artist/Concertable.Artist.Application/`
  with `Concertable.Artist.Application.*` namespaces from the start:
  - `Interfaces/IArtistService.cs`, `Interfaces/IArtistRepository.cs`
  - `DTOs/ArtistDtos.cs`, `Requests/ArtistRequests.cs`
  - `Validators/ArtistValidators.cs`, `Mappers/ArtistMappers.cs`
- ✅ `IArtistRepository` now extends `IIdRepository<ArtistEntity>` (Shared.Domain) instead of
  the legacy `IRepository<ArtistEntity>`. Identical surface, clean dependency direction.
- ✅ `Concertable.Artist.Application.csproj` gained refs: `Shared.Contracts` (for `ImageDto`,
  `GenreDto`), `Shared.Validation` (for `Banner`/`AvatarImageValidator`), and
  `<FrameworkReference Microsoft.AspNetCore.App>` (for `IFormFile` in `CreateArtistRequest`).
  `GlobalUsings.cs` added with `Concertable.Shared` + `Concertable.Artist.Domain`.
- ✅ `ArtistMappers` inlines `GenreDto` construction (one line per mapping) instead of calling
  `GenreMappers.ToDto()`. Keeps Artist.Application free of a dep on legacy `Concertable.Application`
  purely for `GenreMappers`.
- ✅ Consumers updated:
  - `Concertable.Web.csproj` + `Concertable.Infrastructure.csproj` + `Concertable.Application.csproj`
    each gained `<ProjectReference Include=".../Concertable.Artist.Application.csproj" />`.
    Concertable.Application → Artist.Application is tolerated as a transitional wrong-direction ref —
    `OpportunityApplicationDto`/`IOpportunityApplicationService` still name `ArtistSummaryDto`/`ArtistDto`;
    that coupling gets removed in Concert extraction (per §4).
  - `ArtistController`, `ArtistResponseMappers`, `OpportunityApplicationController`,
    `OpportunityApplicationService`, `ArtistService`, `ArtistRepository`, `QueryableArtistMappers`,
    `OpportunityApplicationMapper`, `OpportunityApplicationDtos`, `IOpportunityApplicationService`,
    `ArtistApiTests`, `ArtistRequestBuilders`, `ArtistMappers` (test helper) — all `using` updates.
  - `Concertable.Web/Extensions/ServiceCollectionExtensions.cs` — added
    `using Concertable.Artist.Application.Interfaces;` + `using Concertable.Artist.Application.Validators;`
    and `services.AddValidatorsFromAssemblyContaining<CreateArtistRequestValidator>();` so
    FluentValidation picks up validators in the new assembly. (This line moves to `AddArtistModule()`
    at step 9.)
- ✅ Test baseline held:
  - `ArtistApiTests`: 17 pass.
  - Core/Infrastructure/Workers unit tests + Web integration (129): all pass.
  - E2E `ConcertDraftTests` 4 failures — pre-existing Stripe CLI infra issue, unrelated.

**✅ Step 6 done** — `ArtistDbContext` created at `api/Modules/Artist/Concertable.Artist.Infrastructure/Data/ArtistDbContext.cs`. Inherits `DbContextBase`, owns `Artists` + `ArtistGenres` DbSets, applies `ArtistEntityConfiguration` + `ArtistGenreEntityConfiguration` explicitly (both stay in `Concertable.Data.Infrastructure/Data/Configurations/` per §6/§8 Identity precedent). Build green, 0 errors.

**✅ Steps 7+9 done (2026-04-21)** — Infrastructure layer moved + `AddArtistModule()` wired.
- `ArtistService`, `ArtistRepository`, `QueryableArtistMappers` moved from `Concertable.Infrastructure` to `Artist.Infrastructure/Services|Repositories|Mappers`. All three are now `internal`. Old files deleted.
- `ArtistRepository` base switched from `Repository<ArtistEntity>` (ApplicationDbContext) to `IdModuleRepository<ArtistEntity, ArtistDbContext>`. Injects `IReadDbContext readDb`. Read-path DTO/scalar queries (`GetDtoByIdAsync`, `GetDtoByUserIdAsync`, `GetSummaryAsync`, `GetIdByUserIdAsync`) use `readDb.Artists` + `readDb.Reviews` — **must stay on same context to avoid cross-context LINQ join errors** (EF cannot join `IQueryable<T>` from two different DbContext instances). Write-path tracked queries (`GetFullByIdAsync`, `GetByUserIdAsync`) use `context.Artists` for change tracking.
- `ArtistService` drops `IUnitOfWork` dependency; calls `artistRepository.SaveChangesAsync()` directly (saves via `ArtistDbContext`, matching Identity precedent).
- `GlobalUsings.cs` added to `Artist.Infrastructure`.
- `FluentValidation.DependencyInjectionExtensions` (11.11.0) added to `Artist.Infrastructure.csproj`.
- `AddArtistModule(IConfiguration)` extension created in `Artist.Infrastructure/Extensions/ServiceCollectionExtensions.cs`. Registers `ArtistDbContext` with SQL Server + NetTopologySuite + interceptors; registers `IArtistService`, `IArtistRepository`; scans Artist.Application assembly for validators.
- `Concertable.Web.csproj`: swapped `Artist.Application` ref for `Artist.Infrastructure` (transitive coverage). Added `using Concertable.Artist.Infrastructure.Extensions;` + `services.AddArtistModule(builder.Configuration);` to `Program.cs`.
- `Concertable.Web/Extensions/ServiceCollectionExtensions.cs`: removed `IArtistService`, `IArtistRepository`, `CreateArtistRequestValidator` registrations and their dead `using` statements.
- Build: 0 errors (pre-existing `AppHost`/`IntegrationTests` CS2001 editor-config misses unrelated). ArtistApiTests: 17 pass. Full integration suite: 129 pass.

**Amendment to steps 7+9 — integration event pattern (post-7+9, 2026-04-21):**
The intermediate `IReadDbContext` + `IRatingSpecification` approach was superseded before it shipped. Artist now owns its rating data as a first-class projection updated via integration events — no cross-context queries anywhere in the module.

- `ArtistRatingProjection` domain entity added to `Artist.Domain` (`ArtistId` PK, `AverageRating`, `ReviewCount`).
- `ArtistRatingProjectionConfiguration` added to `Concertable.Data.Infrastructure/Data/Configurations/ArtistEntityConfiguration.cs` (same file, per §8 precedent). Maps to `"ArtistRatingProjections"` table, `ArtistId` PK with `ValueGeneratedNever`.
- `ArtistDbContext` gained `DbSet<ArtistRatingProjection> ArtistRatingProjections` + `ApplyConfiguration(new ArtistRatingProjectionConfiguration())`.
- `ArtistReviewProjectionHandler` (in `Artist.Infrastructure/Handlers/`) — `IIntegrationEventHandler<ReviewSubmittedEvent>` from `Concertable.Concert.Contracts.Events`. Upserts `ArtistRatingProjection` in `ArtistDbContext` (incremental average, `ReviewCount` used for Welford update).
- `ArtistLocationSyncHandler` (in `Artist.Infrastructure/Handlers/`) — `IIntegrationEventHandler<UserLocationUpdatedEvent>` from `Concertable.Identity.Contracts.Events`. Syncs `Location`/`Address` on `ArtistEntity` in `ArtistDbContext`.
- Both handlers registered in `AddArtistModule()` as scoped `IIntegrationEventHandler<T>`.
- `Artist.Infrastructure.csproj` gained `Concertable.Concert.Contracts` project reference.
- `QueryableArtistMappers` rewritten: takes `IQueryable<ArtistRatingProjection>` instead of `IQueryable<RatingAggregate>`; no LinqKit dependency; genres inlined as `a.ArtistGenres.Select(ag => new GenreDto(ag.Genre.Id, ag.Genre.Name))` (EF navigates directly).
- `ArtistRepository` rewritten as primary constructor (just `ArtistDbContext`). Zero `IReadDbContext`, zero `IRatingSpecification`. Read-path queries use `.AsNoTracking()` on `context.Artists` + `context.ArtistRatingProjections`. Write-path tracked queries (`GetFullByIdAsync`, `GetByUserIdAsync`) use plain `context.Artists`.
- `Data.Application` + `Search.Application` project references in `Artist.Infrastructure.csproj` are now orphaned — pending cleanup (no code uses them).
- **Broader context:** `Concertable.Shared.Infrastructure` project created for integration event bus infrastructure. `Concertable.Concert.Infrastructure` (with `AddConcertModule()`) and `Concertable.Concert.Contracts` also created around this work.

**✅ Step 8 done (2026-04-21) — Module.Api pilot landed.** ArtistApiTests: 17 pass. Full integration suite: 129 pass. `IArtistService` + `IArtistRepository` are `internal`; the only foreign callers (`OpportunityApplicationController`, `OpportunityApplicationService`) now use `IArtistModule` + `ICurrentUser` and the full-DTO over-fetch is gone. One implementation note: ASP.NET Core's default `ControllerFeatureProvider` only discovers `public` controllers, so `ArtistController` is `internal` and `Artist.Api` registers a minimal `InternalControllerFeatureProvider` via `ConfigureApplicationPartManager` inside `AddArtistApi()`. This is the pattern future Module.Api migrations (Identity, Search, Venue, Concert) should follow.

**Step 8 — re-corrected plan (2026-04-21, post-design-debate):**

Two false starts have been superseded. For the record:

- **Attempt 1 (minimal cross-module shim):** `IArtistModule` with just `GetSummaryAsync` +
  `GetIdByUserIdAsync`, `IArtistService` stays `public` in Application, `Concertable.Web`
  references `Artist.Application` directly. Killed by CLAUDE.md rule 1 (Web is a foreign caller;
  foreign callers must go through Contracts).
- **Attempt 2 (full facade + delegator):** `IArtistModule` exposes every controller method,
  `IArtistService` flips `internal`, `ArtistModule` is a thin delegator. Killed on design
  grounds — conflates HTTP-facing surface with the module-to-module contract, adds a
  zero-value delegator layer, pollutes the cross-module contract with CRUD methods no other
  module ever calls (`CreateAsync`, `UpdateAsync`, …).

**Final direction: `Module.Api` as a 5th project per module.** Controllers move **inside** the
module, next to the service they use. `Concertable.Web` becomes pure composition host. `IXModule`
in Contracts stays minimal cross-module (the correct MM stable contract — matches the shape you'd
extract to Refit/HTTP). `IXService` stays `internal` to `Module.Application` with
`InternalsVisibleTo("Concertable.<Module>.Api")` so the sibling Api project can inject it.

**This step pilots the pattern on Artist only.** Identity (`AuthController`, `UserController`,
`StripeAccountController`) and Search keep their pre-pilot shape (controllers in
`Concertable.Web/Controllers/`). If the Artist migration goes cleanly, migrate Identity + Search
later. CLAUDE.md has been updated to document the target shape + the pilot note.

**Target reference graph for Artist:**

```
Concertable.Artist.Contracts     — IArtistModule (minimal), cross-module DTOs, integration events
Concertable.Artist.Domain        — ArtistEntity, ArtistGenreEntity, ArtistRatingProjection
Concertable.Artist.Application   — IArtistService (internal), IArtistRepository (internal), DTOs,
                                   Requests, Validators, Mappers. [InternalsVisibleTo:
                                   Infrastructure, Api]
Concertable.Artist.Infrastructure — ArtistService, ArtistRepository, QueryableArtistMappers,
                                   Handlers, ArtistModule (the IArtistModule impl), ArtistDbContext,
                                   AddArtistModule() DI extension.
Concertable.Artist.Api           — ArtistController, ArtistResponseMappers, ArtistResponses,
                                   AddArtistApi() extension that calls AddArtistModule() +
                                   .AddApplicationPart(typeof(ArtistController).Assembly).

Concertable.Web → Concertable.Artist.Api (only; transitively reaches Infrastructure + below)
```

**Scope of step 8 (execution order):**

1. **Revert the two failed attempts' artifacts** (currently sitting in the tree):
   - Delete `api/Modules/Artist/Concertable.Artist.Contracts/IArtistModule.cs` (the 2-method
     version) — will be recreated with the final minimal shape.
   - Delete `api/Modules/Artist/Concertable.Artist.Contracts/ArtistSummaryDto.cs` (standalone
     `(int, string, string?, string?, Address?)` record) — the existing Application
     `ArtistSummaryDto` (Id/Name/Avatar/Rating/Genres) is the one with real consumers; reuse it.
   - Delete `api/Modules/Artist/Concertable.Artist.Infrastructure/ArtistModule.cs` (wired to the
     2-method interface).
   - In `Artist.Infrastructure/Extensions/ServiceCollectionExtensions.cs`, remove the
     `services.AddScoped<IArtistModule, ArtistModule>()` line + its `using` — will be re-added
     after the new `ArtistModule` exists.

2. **Scaffold `Concertable.Artist.Api` project.** New folder `api/Modules/Artist/Concertable.Artist.Api/`
   with `Concertable.Artist.Api.csproj`:
   - `TargetFramework net10.0`, `ImplicitUsings enable`, `Nullable enable`.
   - `<FrameworkReference Include="Microsoft.AspNetCore.App" />` (MVC/routing).
   - Project refs: `Artist.Contracts`, `Artist.Application`, `Artist.Infrastructure` (for the DI
     extension it wraps).
   - Add to `Concertable.sln`.

3. **Move controller + response types into `Artist.Api`:**
   - `Concertable.Web/Controllers/ArtistController.cs` → `Artist.Api/Controllers/ArtistController.cs`
     (namespace `Concertable.Artist.Api.Controllers`).
   - `Concertable.Web/Mappers/ArtistResponseMappers.cs` → `Artist.Api/Mappers/`.
   - `Concertable.Web/Responses/ArtistResponses.cs` → `Artist.Api/Responses/`.
     (Grep-verify no cross-controller consumer before moving the responses.)
   - Controller keeps injecting `IArtistService` — same-module, made visible via
     `InternalsVisibleTo` in step 4.

4. **Add `InternalsVisibleTo`** in `Artist.Application/AssemblyInfo.cs`:
   ```csharp
   [assembly: InternalsVisibleTo("Concertable.Artist.Infrastructure")]
   [assembly: InternalsVisibleTo("Concertable.Artist.Api")]
   ```
   Mirrors Identity's `api/Modules/Identity/Concertable.Identity.Infrastructure/AssemblyInfo.cs`.
   Flip BOTH `IArtistService` and `IArtistRepository` from `public` → `internal`. Sibling
   projects (Infrastructure, Api) see them via `InternalsVisibleTo`; foreign callers do NOT —
   that's enforced by step 9's swap to `IArtistModule`.

5. **Define minimal `IArtistModule` in Contracts.** Three cross-module methods — each a thing
   another *module* actually needs today, no speculation:
   ```csharp
   // Concertable.Artist.Contracts/IArtistModule.cs
   public interface IArtistModule
   {
       Task<int?> GetIdByUserIdAsync(Guid userId);
       Task<ArtistSummaryDto?> GetSummaryAsync(int artistId);
       Task<IReadOnlySet<int>> GetGenreIdsAsync(int artistId);
   }
   ```
   - `GetIdByUserIdAsync` — Concert's `OpportunityApplicationController.CanApply` +
     `OpportunityApplicationService.GetPendingForArtistAsync` / `GetRecentDeniedForArtistAsync` /
     `ApplyAsync` all need "artist Id for this user" — nullable variant (returns `int?`). Throw
     at call site if required.
   - `GetSummaryAsync` — Concert's `OpportunityApplicationDto.Artist` (display card); currently
     populated inline via `.Include(x => x.Artist)`, but the facade method lands now so the
     Concert extraction has the target already wired.
   - `GetGenreIdsAsync` — Concert's `OpportunityApplicationService.ApplyAsync` genre-overlap
     check. Data-shaped (returns the set). We picked this over a verb-shaped
     `HasAnyGenreAsync(artistId, IEnumerable<int>)` because Concert currently owns the overlap
     rule (`artistGenreIds.Overlaps(opportunityGenreIds)`) and a pure lookup keeps that
     ownership clean. Revisit if the rule ever evolves — easy to swap for the verb form later.
   - Move `ArtistSummaryDto` from `Artist.Application/DTOs/ArtistDtos.cs` to
     `Artist.Contracts/ArtistSummaryDto.cs` (namespace `Concertable.Artist.Contracts`) — it has a
     real cross-module consumer via `OpportunityApplicationDto.Artist` today.
   - **`ArtistDto`, `CreateArtistRequest`, `UpdateArtistRequest` stay in `Artist.Application`** —
     only Artist's own controller consumes them, they're an internal implementation detail.
   - `Artist.Contracts.csproj` gains `Concertable.Shared.Contracts` ref (for `GenreDto` used inside
     `ArtistSummaryDto`).

6. **Implement `ArtistModule` in Infrastructure** (tiny — 3 methods, delegate to
   `IArtistRepository`):
   ```csharp
   // Concertable.Artist.Infrastructure/ArtistModule.cs
   internal class ArtistModule(IArtistRepository repo) : IArtistModule
   {
       public Task<int?> GetIdByUserIdAsync(Guid userId) =>
           repo.GetIdByUserIdAsync(userId);
       public Task<ArtistSummaryDto?> GetSummaryAsync(int artistId) =>
           repo.GetSummaryAsync(artistId);
       public Task<IReadOnlySet<int>> GetGenreIdsAsync(int artistId) =>
           repo.GetGenreIdsAsync(artistId);
   }
   ```
   Add `Task<IReadOnlySet<int>> GetGenreIdsAsync(int artistId)` to `IArtistRepository` +
   `ArtistRepository` — 1-liner projecting `context.Artists.Where(a => a.Id == artistId)
   .SelectMany(a => a.ArtistGenres.Select(ag => ag.GenreId))` into a set (`AsNoTracking`, use
   `ToHashSetAsync` then cast to `IReadOnlySet<int>`; or `ToListAsync` then
   `.ToHashSet()`).
   Register `IArtistModule, ArtistModule` in `AddArtistModule()`.
   `IArtistRepository.GetSummaryAsync` already returns an `ArtistSummaryDto`; after step 5 the
   namespace changes from `Concertable.Artist.Application.DTOs` to
   `Concertable.Artist.Contracts` — update the using in `IArtistRepository`, `ArtistRepository`,
   and `QueryableArtistMappers`.

7. **`AddArtistApi()` extension** in `Artist.Api/Extensions/ServiceCollectionExtensions.cs`:
   ```csharp
   public static IServiceCollection AddArtistApi(this IServiceCollection services, IConfiguration config)
   {
       services.AddArtistModule(config);
       services.AddControllers()
           .AddApplicationPart(typeof(ArtistController).Assembly);
       return services;
   }
   ```
   Or leave `.AddApplicationPart(...)` in `Program.cs` — either works. The point is Web calls one
   extension per module.

8. **Swap `Concertable.Web` wiring:**
   - `Concertable.Web.csproj`: drop `Concertable.Artist.Infrastructure` project ref, add
     `Concertable.Artist.Api` project ref (transitively reaches Infrastructure + below).
   - `Program.cs`: swap `services.AddArtistModule(builder.Configuration)` → `services.AddArtistApi(...)`.
   - `Concertable.Web/Extensions/ServiceCollectionExtensions.cs`: any stale `Concertable.Artist.*`
     usings → remove.
   - `Concertable.Web/GlobalUsings.cs`: remove any Artist internals that leaked in.

9. **Swap `OpportunityApplicationController` + `OpportunityApplicationService` to
   `IArtistModule` + `ICurrentUser`.** Both are legacy Concert-side callers that currently inject
   `IArtistService` and over-fetch the full `ArtistDto` to use 1-2 fields. Kill the over-fetch
   AND remove the last foreign consumer of `IArtistService` in one pass — that's what lets step
   4 flip `IArtistService` to `internal` with no transitional wart.

   **`OpportunityApplicationController`** (in `Concertable.Web/Controllers/`) — inject
   `IArtistModule` + `ICurrentUser`, drop `IArtistService`:
   ```csharp
   // CanApply action — was: artistService.GetDetailsForCurrentUserAsync() then ?.Id
   var artistId = await artistModule.GetIdByUserIdAsync(currentUser.GetId());
   if (artistId is null) return NotFound("Artist not found");
   var result = await applicationValidator.CanApplyAsync(opportunityId, artistId.Value);
   ```

   **`OpportunityApplicationService`** (in `Concertable.Infrastructure/Services/Concert/`) —
   inject `IArtistModule`, keep existing `ICurrentUser`, drop `IArtistService`:
   ```csharp
   // GetPendingForArtistAsync / GetRecentDeniedForArtistAsync — was: artistService.GetIdForCurrentUserAsync()
   var artistId = await artistModule.GetIdByUserIdAsync(currentUser.GetId())
       ?? throw new ForbiddenException("You must have an Artist account");

   // ApplyAsync — was: var artistDto = await artistService.GetDetailsForCurrentUserAsync() ?? throw ...
   //                   var artistGenreIds = artistDto.Genres.Select(g => g.Id).ToHashSet();
   var artistId = await artistModule.GetIdByUserIdAsync(currentUser.GetId())
       ?? throw new ForbiddenException("You must create an Artist account before you apply for a concert opportunity");
   var application = OpportunityApplicationEntity.Create(artistId, opportunityId);
   // ... existing opportunity fetch + validator call unchanged (artistDto.Id → artistId)
   var artistGenreIds = await artistModule.GetGenreIdsAsync(artistId);
   // rest of overlap check unchanged
   ```

   Project-ref adjustments:
   - `Concertable.Infrastructure.csproj` gains `Concertable.Artist.Contracts` ref (if not already
     transitively via Artist.Application — likely has it), drops `Concertable.Artist.Application`
     ref **only if** no other legacy file still uses Artist.Application types. Grep-verify. Most
     likely still needs to stay while `OpportunityApplicationDto` in legacy `Concertable.Application`
     still names `ArtistSummaryDto` from Artist.Application — that untangles during Concert
     extraction. Leave the ref; just remove the `using Concertable.Artist.Application.Interfaces;`
     from `OpportunityApplicationService.cs` / `OpportunityApplicationController.cs`.
   - `Concertable.Web/Concertable.Web.csproj` — same: `OpportunityApplicationController` stops
     referencing `Concertable.Artist.Application.Interfaces`.
   - Both files need `using Concertable.Artist.Contracts;` + `using Concertable.Identity.Contracts;`
     (for `ICurrentUser` — probably already there).

   After this swap, **nothing outside the Artist module references `IArtistService`** — step 4's
   `internal` flip is clean.

10. **Artist.Infrastructure.csproj orphaned-ref cleanup** (carry-over):
    - Drop `Concertable.Data.Application` + `Concertable.Search.Application` refs. Unused since
      the Integration Event pattern landed.

11. **Build + test gates:**
    - `dotnet build` clean.
    - `ArtistApiTests`: 17 pass (same tests, unchanged — the controller lives in a different
      assembly but responds to the same HTTP routes).
    - Full integration suite: 129 pass.
    - Pre-existing 4× `ConcertDraftTests` Stripe-CLI failures — remain as baseline.

**What this step does NOT do:**
- Does NOT migrate Identity or Search controllers into `Identity.Api` / `Search.Api` — pilot on
  Artist first. Those migrations are captured as a follow-up once Artist validates.
- Does NOT remove `.Include(x => x.Artist)` chains in Concert-side repos (rule 6 — Concert's job).
- Does NOT flip `ArtistEntity`/`ArtistGenreEntity` to `internal` (still needed by Concert's EF
  navs + Identity's `ArtistManagerRepository` via `IReadDbContext`).
- Does NOT remove `Concertable.Application → Artist.Application` project ref (legacy
  `OpportunityApplicationDto` still names Application-level `ArtistSummaryDto`; gets untangled in
  Concert extraction when the DTO moves out of legacy Application).

### 1. Why Artist is straightforward now

Most of the Identity work paid forward:

- `ArtistManagerEntity` already lives in `Concertable.Identity.Domain/UserEntity.cs:66` — Artist doesn't own the user hierarchy.
- `ArtistEntity` is already denormalized — `Location`, `Address`, `Avatar`, `Email` live on the entity itself (`api/Concertable.Core/Entities/ArtistEntity.cs:16-19`). No `User` nav property remains.
- `ICurrentUser`, `Role`, `ICurrentUserResolver`, `IManagerModule` all live in `Identity.Contracts` / `Identity.Application` — Artist injects them cleanly.
- `GuidModuleRepository<TEntity, TContext>` base exists in `Concertable.Data.Infrastructure/ModuleRepository.cs` — the replacement for `Repository<ApplicationDbContext>`.
- `IReadDbContext` already exposes `IQueryable<ArtistEntity>` and `IQueryable<ArtistGenreEntity>` (`Concertable.Data.Application/IReadDbContext.cs:13-14`).
- `DbContextBase` handles audit + domain event dispatch — `ArtistDbContext` inherits from it, never from `ApplicationDbContext`.
- **Post-Stage-0:** `ArtistEntity` no longer implements `IReviewable` — it's a clean POCO. `Artist.Domain` doesn't need `Concertable.Core`. Rating aggregation is owned by `ArtistRatingSpecification` in `Search.Infrastructure`.

The remaining friction points are coupling from **Concert into Artist** (nav properties,
`IArtistService` injection in `OpportunityApplicationService`, `.Include(x => x.Artist)` chains).
**These are deferred to Concert extraction** — Artist's job is only to become a clean outbound
module. Inbound coupling gets cleaned up when the caller's module is extracted.

---

### 2. Target architecture

```
api/Modules/Artist/
  Concertable.Artist.Contracts/       ← public; IArtistModule facade, Artist summary DTOs for cross-module
  Concertable.Artist.Domain/          ← internal; ArtistEntity, ArtistGenreEntity, domain events
  Concertable.Artist.Application/     ← internal; IArtistService, IArtistRepository, DTOs, Requests, Validators, Mappers
  Concertable.Artist.Infrastructure/  ← internal; ArtistService, ArtistRepository, ArtistDbContext, EF config, AddArtistModule()
```

Same 4-project shape as Identity. `Domain` was added mid-Identity-refactor because entities don't
belong in Application — follow that correction from the start here.

**Project references (post-Stage-0):**

| Project | References |
|---|---|
| `Artist.Contracts` | `Concertable.Shared.Domain` |
| `Artist.Domain` | `Artist.Contracts`, `Concertable.Shared.Domain` *(no `Concertable.Core` dependency — Stage 0 removed it)* |
| `Artist.Application` | `Artist.Contracts`, `Artist.Domain`, `Identity.Contracts` (for `ICurrentUser`), `Shared.Contracts` (for `ImageDto`, `GenreDto`), `Shared.Validation` (for `Banner`/`AvatarImageValidator`), FluentValidation |
| `Artist.Infrastructure` | `Artist.Contracts`, `Artist.Application`, `Identity.Contracts`, `Identity.Application`, `Data.Application`, `Data.Infrastructure`, `Concertable.Infrastructure` (transitional — for `IImageService`, `IGeocodingService`, `IGeometryProvider`, `IUnitOfWork`) |

**Reverse reference:** `Concertable.Core` gains a project reference to `Concertable.Artist.Domain`,
because `OpportunityApplicationEntity.Artist` (nav in Core) now targets the relocated
`ArtistEntity`. This mirrors how `Concertable.Core` already references `Concertable.Identity.Domain`
for `UserEntity` navs on `MessageEntity` / `TransactionEntity`.

---

### 3. Files to move

#### Entities → `Artist.Domain/`

| From | To |
|---|---|
| `Concertable.Core/Entities/ArtistEntity.cs` | `Artist.Domain/ArtistEntity.cs` |
| `Concertable.Core/Entities/ArtistGenreEntity.cs` | `Artist.Domain/ArtistGenreEntity.cs` |

> Entities stay `public` — `OpportunityApplicationEntity.Artist` nav (Concert side) still exists
> until Concert extraction; `ArtistManagerRepository` in Identity.Infrastructure reads `Artist.UserId`
> via `IReadDbContext`. Flip to `internal` only after Concert is extracted.

#### Application interfaces + DTOs + Requests + Validators + Mappers → `Artist.Application/`

| From | To |
|---|---|
| `Concertable.Application/Interfaces/IArtistService.cs` | `Artist.Application/Interfaces/` |
| `Concertable.Application/Interfaces/IArtistRepository.cs` | `Artist.Application/Interfaces/` |
| `Concertable.Application/DTOs/ArtistDtos.cs` (ArtistDto + ArtistSummaryDto) | `Artist.Application/DTOs/` |
| `Concertable.Application/Requests/ArtistRequests.cs` | `Artist.Application/Requests/` |
| `Concertable.Application/Validators/ArtistValidators.cs` | `Artist.Application/Validators/` |
| `Concertable.Application/Mappers/ArtistMappers.cs` (entity→DTO) | `Artist.Application/Mappers/` |

**Namespace lesson from Identity:** rename the namespaces as you move, not later. Target namespaces
are `Concertable.Artist.Application.Interfaces`, `Concertable.Artist.Application.DTOs`, etc. Do
**not** leave them as `Concertable.Application.*` — Identity did that as a shortcut and it became
§4 of `IDENTITY_COMPLETION.md`.

#### Infrastructure → `Artist.Infrastructure/`

| From | To |
|---|---|
| `Concertable.Infrastructure/Services/ArtistService.cs` | `Artist.Infrastructure/Services/` |
| `Concertable.Infrastructure/Repositories/ArtistRepository.cs` | `Artist.Infrastructure/Repositories/` |
| `Concertable.Infrastructure/Mappers/QueryableArtistMappers.cs` | `Artist.Infrastructure/Mappers/` |

**`ArtistEntityConfiguration.cs` stays in `Concertable.Data.Infrastructure/Data/Configurations/`.**
This matches Identity's precedent — `UserEntityConfiguration` et al. never moved into
`Identity.Infrastructure`; they stay colocated with the other configs and `IdentityDbContext`
imports them explicitly. Keeping the same pattern means `ReadDbContext`'s
`ApplyConfigurationsFromAssembly(typeof(ReadDbContext).Assembly)` keeps finding all configs without
needing per-module assembly scans. Migrating configs per-module is a cleanup pass for later, not
this refactor.

#### Stays in `Concertable.Web`

- `ArtistController.cs` — references `IArtistService` from `Artist.Application`.
- `ArtistResponseMappers.cs` — HTTP response mappers stay with the controller.
- `ArtistResponses.cs` — same.

#### Stays in `Concertable.Infrastructure` (transitional)

- `ArtistReviewRepository.cs` and `ArtistRatingRepository.cs` — both query the `Review`/`Rating`
  pipeline which is tied to Tickets/Concerts. Moving these before Concert is extracted trades one
  cross-module leak for another. Defer.
- `Concertable.Seeding/Fakers/ArtistFaker.cs` — seeding lives in its own seam; will move during the
  seeding-refactor work already planned.

> `ArtistRatingSpecification` lives in `Search.Infrastructure` — that's its permanent-until-rewrite
> home. The rating pipeline rewrite (MM_NORTH_STAR §5, per-module review tables fed by events)
> replaces these specs entirely, so moving them per-module is not on the path.

---

### 4. Cross-module coupling — what to fix, what to defer

#### Fix now (Artist outbound)

1. **`ArtistRepository` base class.** Currently `Repository<ArtistEntity>` with `ApplicationDbContext`.
   After extraction: `GuidModuleRepository<ArtistEntity, ArtistDbContext>`. Constructor becomes
   `(ArtistDbContext context, IReadDbContext readDb, IRatingSpecification<ArtistEntity> ratingSpec)`
   — `readDb` is needed because rating aggregation queries `Reviews`, which `ArtistDbContext`
   doesn't own.
2. **`ArtistRepository` rating aggregation.** Stage 0 kept the `IRatingSpecification<T>` injection;
   Stage 1 just swaps the queried source from `context.Reviews` to `readDb.Reviews` when
   `.ApplyAggregate(...)` is called.
3. **`ArtistService` dependencies — already clean.** Constructor already injects `ICurrentUser` +
   `IManagerModule` from Identity.Contracts. No change beyond project reference updates.

#### Defer to Concert extraction (Artist inbound)

These **stay as-is**. They work today because Artist entities still live in the same physical DB
and EF can still resolve nav properties inside `ApplicationDbContext`. Each one is a leak, but
fixing it now means fixing the Concert side from Artist's module, which is backwards.

1. **`OpportunityApplicationService.cs:24`** injects `IArtistService`. When Concert is extracted
   it'll become `IArtistModule.GetSummaryAsync(artistId)` or similar.
2. **`OpportunityApplicationRepository.GetArtistAndVenueByIdAsync`** — uses `.Include(ca => ca.Artist).ThenInclude(a => a.ArtistGenres)...`.
3. **`ConcertRepository`** — `.Include(...).Artist.ArtistGenres.Genre` chains.
4. **`ConcertBookingRepository` / `TicketRepository`** — same include pattern.

(The old item 5 — `ArtistEntity.ReviewIdSelector` — was killed in Stage 0.)

---

### 5. `IArtistModule` contract — what goes in Contracts

`Artist.Contracts` exposes only what *other modules* call. Two known callers after Identity:

- **Identity's `ArtistManagerRepository`** reads `readDb.Artists.Where(a => a.UserId == ...).Select(a => a.Id)` — this is an `IReadDbContext` read, not a contract call. Nothing needed.
- **Concert/Opportunity** currently uses `IArtistService`. When Concert is extracted it will switch to `IArtistModule`. **Define the facade now** so the Concert extraction has a target.

Proposed starting surface — add methods only as callers appear, don't over-design:

```csharp
// Concertable.Artist.Contracts
public interface IArtistModule
{
    Task<ArtistSummaryDto?> GetSummaryAsync(int artistId);
    Task<int?> GetIdByUserIdAsync(Guid userId);
}

public record ArtistSummaryDto(int Id, string Name, string? Avatar, string? Email, AddressDto? Address);
```

`AddressDto` / `GenreDto` references: if Contracts needs value-object shapes, either put them in
`Artist.Contracts` or reference `Concertable.Shared.Domain` (Address is already a shared VO).

**Naming collision risk:** The Application-level `ArtistDto` (full detail with genres, rating,
banner) is different from the Contracts-level `ArtistSummaryDto` (small, for cross-module). Identity
hit name collisions between `Application.DTOs.UserDto` and `Contracts.UserDto`; the Identity plan
(step 9) dropped the Contracts-side `UserDto` entirely because no one called it. Keep
`ArtistSummaryDto` in Contracts tight; keep full `ArtistDto` in `Artist.Application`. No re-export.

**IArtistService in Contracts? — No.** The controller is the only in-process caller and it lives
in the same deployable. Put `IArtistService` in `Artist.Application` and let `Concertable.Web`
reference `Artist.Application` directly. Identity's `IAuthService` / `IUserService` follow the same
pattern.

---

### 6. `ArtistDbContext`

```csharp
// Artist.Infrastructure/Data/ArtistDbContext.cs
internal class ArtistDbContext(DbContextOptions<ArtistDbContext> options)
    : DbContextBase(options)
{
    public DbSet<ArtistEntity> Artists => Set<ArtistEntity>();
    public DbSet<ArtistGenreEntity> ArtistGenres => Set<ArtistGenreEntity>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.ApplyConfiguration(new ArtistEntityConfiguration());
        b.ApplyConfiguration(new ArtistGenreEntityConfiguration()); // if a dedicated config exists; otherwise configure inline
    }
}
```

**Rules (from CLAUDE.md):**
- Inherit `DbContextBase`, not `ApplicationDbContext`.
- Only Artist-owned entities. `GenreEntity` is shared reference data — `ArtistGenreEntity.GenreId` is a plain `int` FK here; no `DbSet<GenreEntity>` or `.HasOne<GenreEntity>()` in this context.
- `OpportunityApplicationEntity` reverse nav on `ArtistEntity.Applications` → **drop the collection from `ArtistEntity`**. Grep confirmed nothing reads it at runtime; only `OpportunityApplicationEntityConfiguration` uses it as the inverse nav — change that to `.WithMany()` (no inverse).

#### Configuration placement

Identity did **not** move its config files — `UserEntityConfiguration` et al. stayed in
`Concertable.Data.Infrastructure/Data/Configurations/`, and `IdentityDbContext` imports + applies
them explicitly. Match that precedent:

- `ArtistEntityConfiguration.cs` **stays put** in `Concertable.Data.Infrastructure/Data/Configurations/`.
- `ArtistDbContext.OnModelCreating` does `modelBuilder.ApplyConfiguration(new ArtistEntityConfiguration())`
  with `using Concertable.Data.Infrastructure.Data.Configurations;`.
- `ReadDbContext` and `ApplicationDbContext` keep their existing `ApplyConfigurationsFromAssembly(typeof(ReadDbContext).Assembly)`
  call — the config is still in that assembly, so it's still picked up automatically.

**Why not move the config into `Artist.Infrastructure`?** Once you move it, `ReadDbContext` loses
automatic pickup (its assembly scan only covers `Concertable.Data.Infrastructure`). You'd need to
append `ApplyConfigurationsFromAssembly(typeof(ArtistDbContext).Assembly)` to `ReadDbContext` — and
another line for every subsequent module. Identity dodged that whole problem by leaving configs in
place. Do the same.

---

### 7. DI wiring — `AddArtistModule()`

Mirror `AddIdentityModule()`:

```csharp
// Artist.Infrastructure/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddArtistModule(this IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<ArtistDbContext>((sp, opt) =>
        opt.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOpt => sqlOpt.UseNetTopologySuite())
            .AddInterceptors(
                sp.GetRequiredService<AuditInterceptor>(),
                sp.GetRequiredService<DomainEventDispatchInterceptor>()));

    services.AddScoped<IArtistService, ArtistService>();
    services.AddScoped<IArtistRepository, ArtistRepository>();
    services.AddScoped<IArtistModule, ArtistModule>();

    services.AddValidatorsFromAssemblyContaining<CreateArtistRequestValidator>();

    return services;
}
```

Called from `Concertable.Web/Extensions/ServiceCollectionExtensions.cs` alongside
`AddIdentityModule()` and `AddSearchModule()`. Remove the current registrations of `IArtistService`
/ `IArtistRepository` / Artist validators from `Concertable.Web` and `Concertable.Infrastructure`
DI extensions — `AddArtistModule()` owns them.

---

### 8. `ApplicationDbContext` cleanup

After this refactor, `ApplicationDbContext` should no longer own Artist entities:

- Remove `DbSet<ArtistEntity> Artists` and `DbSet<ArtistGenreEntity> ArtistGenres` from `ApplicationDbContext`.
- Cross-module reads on Artist go through `IReadDbContext`.
- `ApplicationDbContext` still contains Concert-side entities that **nav-reference** `ArtistEntity`
  (e.g. `OpportunityApplicationEntity.Artist`). The nav stays and EF can still resolve it because
  Artist is still a `public` POCO and lives in the same DB. The Concert-side EF config keeps
  `.HasOne(a => a.Artist)...`. This works.

**On `ArtistEntityConfiguration`:** stays in `Concertable.Data.Infrastructure/Data/Configurations/`
(matches Identity's precedent — `UserEntityConfiguration` never moved into `Identity.Infrastructure`
either). The configuration (owned-type `Address`, `Location` as `geography`, etc.) is still needed:

- `ArtistDbContext` imports it (`using Concertable.Data.Infrastructure.Data.Configurations;`) and
  applies it explicitly via `modelBuilder.ApplyConfiguration(new ArtistEntityConfiguration())`.
- `ReadDbContext` continues to scan its own assembly — config is still in that assembly, so no
  changes to `ReadDbContext` needed.
- `ApplicationDbContext` needs the config as long as it still has `DbSet<ArtistEntity>`; after
  step 10 of §10 removes the DbSet it doesn't need it either. No scan changes required here either.

---

### 9. Integration tests

`api/Tests/Concertable.Web.IntegrationTests/Controllers/Artist/ArtistApiTests.cs` — **17 tests**
(actual count; earlier plan said 19). These are the refactor harness. Run before Stage 0, between
Stage 0 and Stage 1, and after each Stage 1 step.

```
dotnet test --filter "FullyQualifiedName~ArtistApiTests"
```

Also run the full suite after major steps to catch regressions from seed-data/DbContext changes:

```
dotnet test
```

Baseline going in: whatever count is green now (17 Artist + 2 `ConcertDraftTests` E2E pre-existing failures).
Target: same count after Stage 0, same count after Stage 1.

---

### 10. Implementation order (Stage 1)

1. Confirm Stage 0 green — `ArtistApiTests` all pass, no new failures in full suite.
2. **Scaffold projects** — create 4 `.csproj` files under `api/Modules/Artist/`, add to solution, wire project references per §2.
3. **Move entities** → `Artist.Domain/`. Update namespaces to `Concertable.Artist.Domain`. Drop `ArtistEntity.Applications`; flip the EF config side to `.WithMany()`.
4. **Add `Concertable.Core → Concertable.Artist.Domain` project reference.** Update `OpportunityApplicationEntity.Artist` nav's `using`.
5. **Move Application layer** → `Artist.Application/` with correct `Concertable.Artist.Application.*` namespaces from the start.
6. **Create `ArtistDbContext`** in `Artist.Infrastructure/Data/`. `ArtistEntityConfiguration` stays in `Concertable.Data.Infrastructure/Data/Configurations/`; `ArtistDbContext` imports + applies it explicitly via `modelBuilder.ApplyConfiguration(new ArtistEntityConfiguration())`.
7. **Move Infrastructure layer** → `Artist.Infrastructure/`. `ArtistService` (no logic change), `ArtistRepository` (switch to `GuidModuleRepository<ArtistEntity, ArtistDbContext>` + inject `IReadDbContext`; swap rating aggregation source from `context.Reviews` to `readDb.Reviews`), `QueryableArtistMappers`.
8. **Create `IArtistModule` + `ArtistModule`** in `Artist.Contracts` / `Artist.Infrastructure`. Minimal surface (§5).
9. **`AddArtistModule()`** DI extension. Remove old registrations from Web/Infrastructure.
10. **`ApplicationDbContext` cleanup** — remove `DbSet<ArtistEntity>`, `DbSet<ArtistGenreEntity>`, and their configs.
11. **Global usings** — add `Concertable.Artist.Contracts`, `Concertable.Artist.Domain` to `Concertable.Infrastructure/GlobalUsings.cs` and `Concertable.Web/GlobalUsings.cs` where those projects used to pull Artist types via `Concertable.Application.*` / `Concertable.Core.Entities`.
12. **Re-run tests** — 17 Artist integration + full suite.
13. **Fix any build/test regressions**. Most likely: ambiguous `ArtistDto` references; leaked `using Concertable.Application.Interfaces` still pointing at the old `IArtistService` location.

---

### 11. Known friction points (for the implementer)

#### a. ~~`ArtistEntity.ReviewIdSelector` stays~~ — killed in Stage 0
`ArtistEntity` is now a clean POCO with no `IReviewable` implementation. `Artist.Domain` does not
reference `Concertable.Core`.

#### b. `ArtistEntity.Applications` collection
Drop it. Only used as inverse nav in `OpportunityApplicationEntityConfiguration.Configure` —
change `.WithMany(a => a.Applications)` to `.WithMany()`. Confirmed via grep that no runtime code
reads it.

#### c. `ArtistRepository` keeps injecting `IRatingSpecification<ArtistEntity>`
Interface relocated to `Concertable.Search.Application.Interfaces`; concrete impl is
`ArtistRatingSpecification` in `Search.Infrastructure`. Stage 1 only changes the queried source
from `context.Reviews` to `readDb.Reviews` when `.ApplyAggregate(...)` is called.

#### d. `IArtistService` consumers in Web + Infrastructure
Current consumers (grep result):
- `ArtistController` — just updates its `using` to `Concertable.Artist.Application.Interfaces`.
- `OpportunityApplicationService` — leaves its `IArtistService` injection as-is (deferred to Concert extraction per §4).
- `Concertable.Web/Extensions/ServiceCollectionExtensions.cs` — remove Artist registrations; `AddArtistModule()` takes over.

#### e. ArtistManagerEntity + Artist inheritance — already handled
Identity owns `ArtistManagerEntity`. Artist doesn't touch the user hierarchy. `ArtistService.CreateAsync`
calls `managerModule.GetManagerAsync(...)` to fetch the manager — that's already the correct
cross-module contract call. No change.

#### f. Seeding
`Concertable.Seeding/Fakers/ArtistFaker.cs` stays put. The per-module seeder pattern
(`IDevSeeder`/`ITestSeeder`) from the seeding-refactor memo hasn't been applied to Artist yet.
Don't introduce it here — add `ArtistDevSeeder` / `ArtistTestSeeder` as a separate follow-up
(aligns with the Identity pattern).

---

### 12. Explicitly out of scope

- Breaking `OpportunityApplicationService`'s `IArtistService` injection → **Concert extraction**.
- Removing `.Include(x => x.Artist)...` chains from Concert repos → **Concert extraction**.
- `ArtistReviewRepository` / `ArtistRatingRepository` relocation → **Concert extraction** (they query the Review pipeline tied to Tickets/Concerts).
- Flipping `ArtistEntity` / `ArtistGenreEntity` to `internal` → after Concert extraction, when nothing outside Artist references them.
- Per-module seeders (`ArtistDevSeeder`, `ArtistTestSeeder`) → follow-up after this refactor lands.
- Moving `ArtistRatingSpecification` into `Artist.Infrastructure` (stays in `Search.Infrastructure`; replaced during rating-pipeline rewrite per MM_NORTH_STAR §5).

---

### 13. Up next: Venue

Venue is structurally identical to Artist: same denormalization already done, same `VenueManagerEntity`
in `Identity.Domain`, same inbound coupling from Concert. After Artist lands, Venue is a near-copy.
The plan for Venue writes itself from this file.

## Reference
- `IDENTITY_MODULE_REFACTOR.md` — the pattern.
- `IDENTITY_COMPLETION.md` — specifically §3 (ModuleRepository bases) and §4 (namespace lesson).
- `CLAUDE.md` — the non-negotiable DbContext / module rules.
- `MM_NORTH_STAR.md` — §Corollary 1/2/5 explain why the `IReviewable<T>` abstraction had to die.
- `Concertable.Search.*` + `Concertable.Identity.*` — two reference implementations in-repo.
