# Venue Module Extraction — Implementation Plan

Successor to `ARTIST_MODULE_REFACTOR.md`. Venue is structurally a near-twin of Artist: same
`UserId` FK shape, same denormalization of profile fields, same inbound coupling from Concert,
same Search projection consumer. This plan reuses the Artist playbook and calls out only the
places where Venue diverges.

**Read this with `ARTIST_MODULE_REFACTOR.md` open.** Anywhere this doc says "as Artist §X", the
Artist plan is canonical — don't re-derive. Memory: `project_modular_monolith.md` tracks stage
progress.

---

## Starting state (discovery, 2026-04-22)

`Concertable.Core / Application / Infrastructure / Web` still own all Venue code. No Stage 0
cleanup is needed — the shared `IReviewable<TSelf>` abstraction was already killed during Artist
Stage 0, and `VenueEntity` is a clean POCO today. Straight to Stage 1.

### What already matches the Artist starting line

- `VenueEntity` does **not** implement `IReviewable<VenueEntity>` — already a POCO
  (`api/Concertable.Core/Entities/VenueEntity.cs`).
- `VenueEntity.Email` and `VenueEntity.Avatar` are denormalized copies from `UserEntity` — matches
  the Artist snapshot pattern, no User nav needed.
- `VenueEntity.UserId` is `Guid` with no FK constraint in migrations (same shape as Artist —
  primitive-only cross-module ref, enforced by event sync / app logic, not EF).
- `VenueRatingSpecification` already lives in `Search.Infrastructure` (was moved during Artist
  Stage 0's cross-module interface sweep) — no relocation needed.
- `VenueRepository` injects `IRatingSpecification<VenueEntity>` — keeps that injection post-
  extraction exactly as `ArtistRepository` does (see Artist §11c).

### What's Venue-specific

- **No genres on `VenueEntity`.** Unlike Artist, Venue has no `VenueGenreEntity` join table. Genres
  attach to `OpportunityEntity` (via `OpportunityGenreEntity`), which stays with Concert. This
  **eliminates** the cross-context `FK_*_Genres_GenreId` hand-edit that Artist §Step 10 warned
  about — Venue's migration will have zero FKs into `Shared.Domain`. The gotcha pattern from
  `feedback_cross_context_fk.md` does not apply to Venue.
- **No Avatar collection — single banner + avatar strings.** Venue has `VenueImageEntity` for a
  gallery, scoped to Venue. Moves with Venue to `Venue.Domain`.
- **Inbound nav from Concert is direct, not via join.** `OpportunityEntity.VenueId` (int) +
  `OpportunityEntity.Venue` nav (line 10–12 of `OpportunityEntity.cs`) — a direct 1:N parent
  reference, where Artist's inbound link was `OpportunityApplicationEntity.Artist`. The defer-to-
  Concert rule is the same; the nav chain Concert uses is longer
  (`concert.Booking.Application.Opportunity.Venue`).
- **`VenueLocationSyncHandler` already exists** as an integration-event handler listening for
  `UserLocationUpdatedEvent` (`api/Concertable.Infrastructure/Events/VenueLocationSyncHandler.cs`).
  Artist has the same pattern at `api/Modules/Artist/Concertable.Artist.Infrastructure/Handlers/
  ArtistLocationSyncHandler.cs`. Move the Venue handler to `Venue.Infrastructure/Handlers/` —
  straight copy of the Artist precedent.

### What is *not* Venue and must NOT move

- `VenueHireContractEntity` (`api/Concertable.Core/Entities/Contracts/VenueHireContractEntity.cs`)
  — inherits `ContractEntity`, lives in Concert's orbit. Name is misleading: it's "the contract
  type issued when a venue is hired for an event," not a Venue-owned aggregate. Stays in Core /
  moves with Concert during Concert extraction. Its mapper
  (`VenueHireContractMapper.cs`) and tests also stay.
- `VenueTicketPaymentService` (`api/Concertable.Infrastructure/Services/Payment/`) — payment
  workflow for venue-hosted ticket sales. Moves with Payment extraction.
- `VenueHireConcertWorkflow` (`api/Concertable.Infrastructure/Services/Application/`) — Concert-
  side orchestration that happens to involve venue hire contracts. Moves with Concert.
- `VenueManagerRegister`, `VenueManagerLoader`, `VenueManagerRepository`, `VenueManagerMapper`
  (`api/Modules/Identity/...`) — Identity-owned user-flow code for the VenueManager role. Already
  extracted with Identity. Don't touch.

---

## Stage 1 — Venue module extraction

### 1. Target architecture

```
api/Modules/Venue/
  Concertable.Venue.Contracts/       ← IVenueModule, cross-module DTOs, events
  Concertable.Venue.Domain/          ← VenueEntity, VenueImageEntity, VenueRatingProjection
  Concertable.Venue.Application/     ← IVenueService, IVenueRepository, DTOs, Requests, Validators, Mappers
  Concertable.Venue.Infrastructure/  ← VenueService, VenueRepository, VenueDbContext, handlers, AddVenueModule()
  Concertable.Venue.Api/             ← VenueController (internal) + InternalControllerFeatureProvider
```

Project references (same shape as Artist):
- `Venue.Domain` → `Concertable.Shared.Domain` only.
- `Venue.Contracts` → `Venue.Domain`, `Concertable.Shared.Contracts`.
- `Venue.Application` → `Venue.Contracts`, `Venue.Domain`, `Identity.Contracts`,
  `Concertable.Shared.*`, `Concertable.Data.Application` (for `IReadDbContext`).
- `Venue.Infrastructure` → `Venue.Application`, `Concertable.Data.Infrastructure` (for
  `DbContextBase` + entity configs), `Concertable.Shared.Infrastructure`.
- `Venue.Api` → `Venue.Application`, `Venue.Infrastructure` (same-module, for DI registration in
  controller tests if needed — or just `Venue.Application` for the controllers themselves).
- `Concertable.Core` → `Concertable.Venue.Domain` (the entity-type escape hatch per CLAUDE.md
  rule 3 — `OpportunityEntity.Venue` nav needs the type to compile).
- `Concertable.Web` → `Venue.Api` + `Venue.Infrastructure` (ApplicationPart + DI).

### 2. Files to move

#### Entities → `Venue.Domain/`
- `api/Concertable.Core/Entities/VenueEntity.cs` → `Venue.Domain/VenueEntity.cs`
- `api/Concertable.Core/Entities/VenueImageEntity.cs` → `Venue.Domain/VenueImageEntity.cs`
- **New:** `Venue.Domain/VenueRatingProjection.cs` — mirror of `ArtistRatingProjection` (id,
  AverageRating, ReviewCount). Initially empty projection table; populated by
  `VenueReviewProjectionHandler` following Artist's pattern. Keeping this in scope for Stage 1
  matches the Artist precedent (`ArtistReviewProjectionHandler` landed during Artist Stage 1 DI
  wiring).
- **Drop:** `VenueEntity.Opportunities` collection. Only referenced as inverse nav in
  `OpportunityEntityConfiguration.Configure` — flip `.WithMany(v => v.Opportunities)` to
  `.WithMany()`. Same move as Artist §11b did with `Applications`.

Namespace: `Concertable.Venue.Domain` from the start — do **not** leave `Concertable.Core.Entities`
on moved files (see Identity lesson in `IDENTITY_COMPLETION.md §4`).

#### Application → `Venue.Application/`
- `api/Concertable.Application/Interfaces/IVenueRepository.cs`
- `api/Concertable.Application/Interfaces/IVenueService.cs`
- `api/Concertable.Application/DTOs/VenueDtos.cs` (`VenueDto`, `VenueSummaryDto`)
- `api/Concertable.Application/Requests/VenueRequests.cs` (`CreateVenueRequest`,
  `UpdateVenueRequest`)
- `api/Concertable.Application/Validators/VenueValidators.cs`
- `api/Concertable.Application/Mappers/VenueMappers.cs`

Namespace: `Concertable.Venue.Application.*`. Everything `internal`; `AssemblyInfo.cs` with
`InternalsVisibleTo` entries for `Venue.Infrastructure` and `Venue.Api` (precedent:
`api/Modules/Identity/Concertable.Identity.Infrastructure/AssemblyInfo.cs` lives in the owning
project — for Venue, put the file in `Venue.Application/AssemblyInfo.cs` since the interfaces
being exposed are Application's).

**Keep `IVenueReviewRepository` where it is for now** (in legacy
`Concertable.Application/Interfaces/`) — Venue reviews are tied to Concert/Ticket pipeline, same
as Artist reviews. Moves with Concert extraction. Artist §12 handles its analog the same way.

#### Infrastructure → `Venue.Infrastructure/`
- `api/Concertable.Infrastructure/Services/VenueService.cs`
- `api/Concertable.Infrastructure/Repositories/VenueRepository.cs`
- `api/Concertable.Infrastructure/Mappers/QueryableVenueMappers.cs`
- `api/Concertable.Infrastructure/Events/VenueLocationSyncHandler.cs` →
  `Venue.Infrastructure/Handlers/VenueLocationSyncHandler.cs`
- **New:** `Venue.Infrastructure/Handlers/VenueReviewProjectionHandler.cs` —
  mirror of `ArtistReviewProjectionHandler`. Listens for `ReviewSubmittedEvent`, upserts
  `VenueRatingProjection` row.
- **New:** `Venue.Infrastructure/Data/VenueDbContext.cs` — inherits `DbContextBase`, owns
  `Venues`, `VenueImages`, `VenueRatingProjections`. Does NOT own `Opportunities`.
- **New:** `Venue.Infrastructure/Data/Configurations/VenueRatingProjectionConfiguration.cs`.
- **New:** `Venue.Infrastructure/Extensions/ServiceCollectionExtensions.cs` — `AddVenueModule()`
  DI extension. Mirrors `AddArtistModule()` exactly: registers `VenueDbContext`, `IVenueService`,
  `IVenueRepository`, `IVenueModule`, integration-event handlers.

`VenueEntityConfiguration` and `VenueImageEntityConfiguration` (EF configs) **stay** in
`Concertable.Data.Infrastructure/Data/Configurations/` per CLAUDE.md database rules. `VenueDbContext`
applies them explicitly:
```csharp
modelBuilder.ApplyConfiguration(new VenueEntityConfiguration());
modelBuilder.ApplyConfiguration(new VenueImageEntityConfiguration());
modelBuilder.ApplyConfiguration(new VenueRatingProjectionConfiguration());
```

`VenueRepository` switches its base class to `IntModuleRepository<VenueEntity, VenueDbContext>`
(int-keyed — Venue uses `int Id`, not `Guid` like Artist, so use the int variant). Rating
aggregation swaps from `context.Reviews` to `readDb.Reviews` via injected `IReadDbContext`, same
as Artist §Step 7.

**`VenueRatingRepository`** (`api/Concertable.Infrastructure/Repositories/Rating/VenueRatingRepository.cs`)
stays put — Concert extraction concern, same as `ArtistRatingRepository` per Artist §12.

#### Controller → `Venue.Api/`
- `api/Concertable.Web/Controllers/VenueController.cs` → `Venue.Api/Controllers/VenueController.cs`
- Make the controller `internal`; add `InternalControllerFeatureProvider` in `Venue.Api` (same
  precedent set by Artist/Identity/Search Api migration — 2026-04-22 per memory).
- Routes stay at `/api/venue/...` — all 6 endpoints preserved verbatim:
  - `GET /api/venue/{id}`
  - `GET /api/venue/user`
  - `POST /api/venue` (multipart)
  - `PUT /api/venue/{id}` (multipart)
  - `PATCH /api/venue/{id}/approve`
  - `GET /api/venue/is-owner/{id}`

Controller injects `IVenueService` (made visible via `InternalsVisibleTo`), not `IVenueModule`.

#### Seeding
- `api/Concertable.Seeding/Fakers/VenueFaker.cs` — stays in `Concertable.Seeding` (matches
  Artist — Fakers is cross-module seed-data, not a module-owned concern).
- `DevDbInitializer` + `TestDbInitializer` need `VenueDevSeeder` + `VenueTestSeeder` in
  `Venue.Infrastructure` **before** `DbSet<VenueEntity>` can be removed from `ApplicationDbContext`.
  This is the same precondition Artist §Step 10 surfaced: the per-module seeder pattern (from
  `seeding-refactor.md`) is a blocker for ApplicationDbContext cleanup, not a follow-up. Pull
  these into Stage 1 scope from day one — don't repeat the Artist ordering mistake that had to
  be corrected mid-flight.

### 3. Cross-module coupling

#### Fix now (Venue outbound)
- `VenueService` currently injects `IManagerModule` (Identity Contracts) to fetch the owning user.
  Already the correct shape — leaves as-is.
- `VenueService` uses `IImageService`, `IGeocodingService`, `IGeometryProvider`, `IUnitOfWork` —
  all Shared.Infrastructure-level services. Continue injecting; no change.

#### Defer to Concert extraction (Venue inbound — leave alone)
- `OpportunityEntity.VenueId` + `.Venue` nav — stays. `Concertable.Core → Venue.Domain` project
  reference keeps the type in scope (CLAUDE.md rule 3).
- `ConcertEntity.LocationExpression => c => c.Booking.Application.Opportunity.Venue.Location` —
  stays.
- `ConcertMappers` venue projections (maps `VenueEntity` → `ConcertVenueDto`) — stays.
- `ConcertRepository` injecting `IRatingSpecification<VenueEntity>` — stays.
- `OpportunityApplicationRepository.GetArtistAndVenueByIdAsync` returning `(ArtistEntity,
  VenueEntity)?` — stays.
- `OpportunityRepository` `.Include(x => x.Venue)` chains — stay.

All of the above get rewritten to `IVenueModule.GetSummaryAsync(...)` calls (or local projection
reads in the north-star end state) when **Concert** is extracted. Per CLAUDE.md rule 6: inbound
coupling is the caller's problem.

#### Search — already correct
Search has its own `VenueHeaderRepository`, `VenueRatingSpecification`, `VenueSearchSpecification`
in `Search.Infrastructure`, querying via `IReadDbContext`. No change needed. When
`VenueRatingProjection` lands, Search's rating aggregation can eventually switch off the legacy
`VenueRatingSpecification` — but that's the rating-pipeline rewrite (MM_NORTH_STAR §5), not this
extraction.

### 4. `IVenueModule` — Contracts surface

Minimal cross-module lookups only (per `feedback_module_facade_surface.md` and CLAUDE.md naming
rules). Mirror of `IArtistModule`:

```csharp
public interface IVenueModule
{
    Task<VenueSummaryDto?> GetSummaryAsync(int venueId, CancellationToken ct = default);
    Task<int?> GetVenueIdByUserIdAsync(Guid userId, CancellationToken ct = default);
}
```

`CreateAsync`/`UpdateAsync`/`ApproveAsync` do **not** go on `IVenueModule` — they're controller-
facing only, exposed via `IVenueService` internal to `Venue.Application`. Another module has no
business creating a venue.

`VenueSummaryDto` moves to `Venue.Contracts` since it's the return type of `IVenueModule`.
`VenueDto` stays in `Venue.Application` unless another module ends up needing it — cross-reference
CLAUDE.md's DTO visibility rule.

### 5. `VenueDbContext`

- Inherits `DbContextBase`.
- `DbSet<VenueEntity> Venues`
- `DbSet<VenueImageEntity> VenueImages`
- `DbSet<VenueRatingProjection> VenueRatingProjections`
- **No** `DbSet<OpportunityEntity>` — that belongs to Concert's future context (today still in
  `ApplicationDbContext`).
- Cross-module FKs (`UserId`) remain plain `Guid` primitives with no nav, no constraint (matches
  Artist + CLAUDE.md database rules).
- Migration history table: `__EFMigrationsHistory_Venue` (parallels `__EFMigrationsHistory_Artist`
  / `__EFMigrationsHistory_Identity`).

### 6. `AddVenueModule()` DI wiring

```csharp
public static IServiceCollection AddVenueModule(this IServiceCollection services, IConfiguration cfg)
{
    services.AddDbContext<VenueDbContext>(opts =>
        opts.UseSqlServer(cfg.GetConnectionString("DefaultConnection"),
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory_Venue")
                      .UseNetTopologySuite()));
    services.AddScoped<IVenueService, VenueService>();
    services.AddScoped<IVenueRepository, VenueRepository>();
    services.AddScoped<IVenueModule, VenueModule>();
    services.AddScoped<IIntegrationEventHandler<UserLocationUpdatedEvent>, VenueLocationSyncHandler>();
    services.AddScoped<IIntegrationEventHandler<ReviewSubmittedEvent>, VenueReviewProjectionHandler>();
    services.AddScoped<IVenueDevSeeder, VenueDevSeeder>();
    services.AddScoped<IVenueTestSeeder, VenueTestSeeder>();
    return services;
}
```

Remove from `Concertable.Web/Extensions/ServiceCollectionExtensions.cs`:
- `IVenueService`, `IVenueRepository` registrations.
- Keep `services.AddKeyedScoped<IRatingRepository, VenueRatingRepository>(HeaderType.Venue)` — it
  still lives in legacy Infrastructure (see Artist §12 precedent).

### 7. `ApplicationDbContext` cleanup + migration scaffolding

Remove from `ApplicationDbContext`:
- `DbSet<VenueEntity> Venues`
- `DbSet<VenueImageEntity> VenueImages`
- Any `modelBuilder.ApplyConfiguration(new VenueEntityConfiguration())` / `VenueImage`.
- Keep `ExcludeFromMigrations` for `VenueEntity` until the corresponding
  `ApplicationDbContext` migration drops the table cleanly (same sequence Artist followed at
  step 10).

Migration sequence:
1. Scaffold `VenueDbContext` `InitialCreate` migration — creates `Venues`, `VenueImages`,
   `VenueRatingProjections`.
2. Scaffold `ApplicationDbContext` migration that **drops** `Venues` + `VenueImages` tables (EF
   generates this automatically when the DbSets are removed from the context).
3. **Run them in the right order.** `ApplicationDbContext` migration runs first (drops tables),
   then `VenueDbContext` migration (creates them fresh). Same runtime order Artist used — Web's
   migration orchestration applies ApplicationDbContext migrations before per-module ones.

**Cross-context FK gotcha — does NOT apply to Venue.** Venue has no FKs into `Shared.Domain`
tables (no genres, no shared lookups). The `feedback_cross_context_fk.md` hand-edit Artist needed
for `FK_ArtistGenres_Genres_GenreId` has no equivalent here. EF will generate no outbound FKs;
nothing to strip.

**Inbound FK: `FK_Opportunities_Venues_VenueId`.** This FK lives in `ApplicationDbContext`'s
schema (Opportunities still belongs to `ApplicationDbContext`). When the `ApplicationDbContext`
migration drops the `Venues` table, EF will generate a `DropForeignKey` for this constraint
automatically. The reference integrity then falls back to app logic until Concert extracts and
`VenueId` becomes a plain primitive. This is the expected transitional state and matches what
happened with `OpportunityApplication.ArtistId` during Artist extraction.

### 8. Integration tests

- `VenueApiTests.cs` + helpers move to a folder under
  `api/Tests/Concertable.Web.IntegrationTests/Controllers/Venue/` — keep the existing location
  (Artist didn't relocate its tests either).
- `TestDbInitializer` needs updating to seed via `IVenueTestSeeder` instead of direct
  `context.Venues.Add(...)`.
- Full integration suite must pass. Venue has 6 endpoints + related Ticket / OpportunityApplication
  tests that touch Venue data indirectly.

### 9. Implementation order

1. **Scaffold 5 project files** under `api/Modules/Venue/`; add to `Concertable.sln`; wire project
   references per §1.
2. **Move entities** → `Venue.Domain/`. Update namespaces. Drop `VenueEntity.Opportunities`
   collection; flip `OpportunityEntityConfiguration` side to `.WithMany()`.
3. **Add `VenueRatingProjection`** to `Venue.Domain/` + its EF configuration in
   `Venue.Infrastructure/Data/Configurations/`.
4. **Add `Concertable.Core → Concertable.Venue.Domain` project reference.** Update
   `OpportunityEntity.Venue` nav's `using` to `Concertable.Venue.Domain`.
5. **Move Application layer** → `Venue.Application/` with correct namespaces and `internal`
   visibility from the start. Add `AssemblyInfo.cs` with `InternalsVisibleTo` for sibling Api +
   Infrastructure.
6. **Create `VenueDbContext`** in `Venue.Infrastructure/Data/`. Apply configurations explicitly.
7. **Move Infrastructure layer** → `Venue.Infrastructure/`: `VenueService`, `VenueRepository`
   (switch base to `IntModuleRepository<VenueEntity, VenueDbContext>`, swap rating source to
   `readDb.Reviews`), `QueryableVenueMappers`, `VenueLocationSyncHandler`.
8. **Add `VenueReviewProjectionHandler`** (new — mirrors `ArtistReviewProjectionHandler`).
9. **Create `IVenueModule` + `VenueModule`** in `Venue.Contracts` / `Venue.Infrastructure`.
10. **Create `Venue.Api`** with `VenueController` (internal) + `InternalControllerFeatureProvider`.
11. **Add `VenueDevSeeder` + `VenueTestSeeder`** (precondition for step 13). Wire into
    `DevDbInitializer` and `TestDbInitializer` via the `IDevSeeder`/`ITestSeeder` pattern.
12. **`AddVenueModule()`** DI extension. Remove old registrations from Web/Infrastructure.
13. **`ApplicationDbContext` cleanup** — remove `DbSet<VenueEntity>`, `DbSet<VenueImageEntity>`,
    their configs. Scaffold the paired drop migration.
14. **Scaffold `VenueDbContext` `InitialCreate` migration.** Inspect for outbound FKs — should be
    none; confirm before applying.
15. **Global usings** — add `Concertable.Venue.Contracts`, `Concertable.Venue.Domain` to
    `Concertable.Infrastructure/GlobalUsings.cs` + `Concertable.Web/GlobalUsings.cs` where Venue
    types were previously pulled via `Concertable.Application.*` / `Concertable.Core.Entities`.
16. **Re-run tests** — `VenueApiTests` + full suite.
17. **Fix regressions.** Likely ambiguous `VenueDto` references; leaked `using
    Concertable.Application.Interfaces` still pointing at old `IVenueService` location.

### 10. Known friction points

#### a. `VenueEntity.Opportunities` collection
Drop it. Inverse nav only — same dynamic as `ArtistEntity.Applications` in Artist §11b. Flip
`.WithMany(v => v.Opportunities)` to `.WithMany()` in `OpportunityEntityConfiguration`.

#### b. `VenueRepository` keeps injecting `IRatingSpecification<VenueEntity>`
Same as Artist §11c. Interface lives in `Search.Application.Interfaces`; concrete impl
`VenueRatingSpecification` in `Search.Infrastructure`. Stage 1 only changes the queried source
from `context.Reviews` to `readDb.Reviews`. Long-term replacement is the rating-pipeline rewrite
(MM_NORTH_STAR §5) — `VenueRatingProjection` is the foothold.

#### c. `IVenueService` consumers outside Venue
Grep result (pending re-verification at implementation time):
- `VenueController` → moves to `Venue.Api` and injects `IVenueService` directly.
- Any `OpportunityService` / `VenueHireConcertWorkflow` / similar injections → leave as-is;
  rewritten during Concert extraction (Artist §11d precedent).
- `Concertable.Web/Extensions/ServiceCollectionExtensions.cs` — strip Venue registrations.

#### d. `VenueManager*` in Identity — already handled
Identity owns the VenueManager user flow via role scoping on `UserEntity`. `VenueService.CreateAsync`
calls `managerModule.GetManagerAsync(userId)` — already the correct cross-module contract
invocation. No change.

#### e. Seeding is a Stage 1 precondition, not a follow-up
`VenueDevSeeder` + `VenueTestSeeder` must land **before** step 13 (ApplicationDbContext cleanup).
Artist discovered this mid-refactor and had to pull the work into scope. Do it upfront here.

#### f. Multipart form binding for Create/Update
`VenueController` POST/PUT endpoints are `[FromForm]` multipart with image upload. The existing
`IImageService` injection in `VenueService` handles this. When moving the controller to
`Venue.Api`, keep the exact request-binding annotations — integration tests will catch the
slightest drift.

### 11. Explicitly out of scope

- Rewriting Concert's `.Include(x => x.Venue)` chains or `ConcertMappers.ToConcertVenueDto` →
  **Concert extraction**.
- Removing `OpportunityApplicationRepository.GetArtistAndVenueByIdAsync` → **Concert extraction**.
- Moving `VenueHireContractEntity`, `VenueHireContractMapper`, `VenueTicketPaymentService`,
  `VenueHireConcertWorkflow` → **Concert / Payment extractions**.
- `VenueReviewRepository` / `VenueRatingRepository` relocation → **Concert extraction** (Review
  pipeline is tied to Tickets/Concerts).
- Flipping `VenueEntity` / `VenueImageEntity` to `internal` → post-Concert extraction, when
  nothing outside Venue references them.
- Moving `VenueRatingSpecification`, `VenueSearchSpecification`, `VenueHeaderRepository` out of
  `Search.Infrastructure` → rating-pipeline rewrite per MM_NORTH_STAR §5, not this extraction.
- Integration-event outbox / durable delivery infrastructure → deferred to post-Venue, before
  Payment extraction (per MM_NORTH_STAR bridge §2).

---

## Up next: Concert

After Venue lands, Concert is the big one — most cross-module coupling (inbound to both Artist
and Venue, outbound to Payment), most nav chains to rewrite, first module where `IArtistModule` /
`IVenueModule` facade calls replace `.Include(...)` chains at scale. `CONCERT_MODULE_REFACTOR_DRAFT.md`
already exists — treat it as a starting point, not a finished plan.

---

## Reference

- `ARTIST_MODULE_REFACTOR.md` — the canonical pattern this plan is a delta against.
- `IDENTITY_MODULE_REFACTOR.md` + `IDENTITY_COMPLETION.md` — precedents for the 4-layer shape and
  the namespace lesson.
- `CLAUDE.md` — non-negotiable module / DbContext rules.
- `MM_NORTH_STAR.md` — §Corollary 1/2/5 + bridge §1.
- Memory: `project_modular_monolith.md`, `feedback_module_facade_surface.md`,
  `feedback_cross_context_fk.md` (note: does not apply to Venue per §7 above).
