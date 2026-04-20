# Artist Module Extraction — Handoff Context

## Goal
Extract the Artist domain into a `Concertable.Artist` module following the same Modular Monolith pattern used for `Concertable.Search`. This is part of a broader MM migration (Search ✅ → Artist → Venue → Concert → Payment).

## Architecture Pattern (established)
Each module has:
- `Concertable.{Module}.Contracts` — public facade interfaces (`IArtistModule`), public DTOs. Only this project is referenced by other modules.
- `Concertable.{Module}.Application` — interfaces (services, repos), internal use cases
- `Concertable.{Module}.Infrastructure` — implementations (services, repos, EF config)
- Everything marked `internal`; only Contracts + `AddArtistModule()` DI extension are public
- Cross-module communication: reference Contracts only, never internals
- One physical DB, separate schema per module (`artist` schema)
- Per-module write DbContext (internal), inheriting from the global `ApplicationDbContext` until full migration is complete

See `Concertable.Search.*` projects as the reference implementation.

## What Belongs in the Artist Module

### Domain entities (move from `Concertable.Core`)
- `ArtistEntity`
- `ArtistGenreEntity`
- `ArtistManagerEntity` (the manager user role entity)
- `ArtistImageEntity` (if it exists separately)

### Application interfaces (move from `Concertable.Application`)
- `IArtistService`
- `IArtistRepository`
- `IArtistReviewRepository`
- DTOs: `ArtistDto`, `ArtistSummaryDto`
- Requests: `CreateArtistRequest`, `UpdateArtistRequest`
- Validators: `CreateArtistRequestValidator`, `UpdateArtistRequestValidator`
- Mappers: `ArtistMappers.cs`, `ArtistManagerMapper.cs`

### Infrastructure implementations (move from `Concertable.Infrastructure`)
- `ArtistService`
- `ArtistRepository`
- `ArtistReviewRepository` (if exists)
- EF configuration for Artist entities

### Web layer
- `ArtistController` stays in `Concertable.Web` but references `IArtistModule` facade instead of `IArtistService` directly (or keeps `IArtistService` if it's exposed through the Contracts project)

## Identity Module Consideration (affects ArtistManagerEntity)

A `Concertable.Identity` module is planned (see MM plan). The key question for Artist extraction is what happens to `ArtistManagerEntity`.

`ArtistManagerEntity` inherits `ManagerEntity → UserEntity` and lives in the same TPH/TPT hierarchy. Two options:

**Option A — Keep manager entities in Identity (recommended for now)**
Identity owns `UserEntity`, `ManagerEntity`, `ArtistManagerEntity`, `VenueManagerEntity`. Artist module references `ArtistManagerEntity` via `IIdentityModule` or through Identity.Contracts. The EF inheritance stays intact — no migration risk.

**Option B — Break inheritance, move to Artist**
`ArtistManagerEntity` moves to Artist module, drops inheritance, keeps only `UserId (Guid)` as a FK to `UserEntity` in Identity. Cleaner MM boundary but requires an EF migration to restructure the TPH table.

Option A is the safer path during initial extraction. Option B is the eventual target. Proceed with Option A; don't restructure the inheritance chain during this refactor.

**Practical impact for Artist extraction:** `ArtistManagerEntity` stays put for now. Artist module will reference it from wherever it lives (currently `Concertable.Core`, eventually `Concertable.Identity`).

## Known Friction Points (investigate before moving)

### 1. `ArtistEntity.ReviewIdSelector` — crosses into Concert domain
The artist entity has an expression that traverses `Ticket.Concert.Booking.Application.ArtistId` for rating calculations. This creates a direct dependency from Artist domain on Concert/Booking/Ticket structure.
**Fix needed:** Abstract the rating calculation — either pass in the selector from outside, or introduce a rating aggregation service that lives in a shared layer.

### 2. Concert domain reaches into Artist via navigation properties
- `OpportunityApplicationEntity` has a direct FK and navigation to `ArtistEntity`
- `ConcertRepository` explicitly includes `.Artist.ArtistGenres.Genre` and `.Artist.User`
- `IOpportunityApplicationRepository` has `GetArtistAndVenueByIdAsync()` returning raw `ArtistEntity`

**Fix needed:** Once Artist is a module, Concert cannot reference `ArtistEntity` directly. Concert must reference `ArtistId` (int) only and call `IArtistModule.GetSummaryAsync(artistId)` for any artist data it needs. Navigation properties across module boundaries must be removed.

### 3. `OpportunityApplicationService` injects `IArtistService`
Concert/Opportunity application service directly calls Artist service.
**Fix needed:** Replace with `IArtistModule` contract call.

### 4. `OwnershipService` injects both `IArtistService` and `IVenueService`
This shared ownership service crosses multiple domains.
**Fix needed:** Either move ownership checks into each module's own service, or keep `OwnershipService` in `Concertable.Application` referencing the Contracts interfaces of each module.

## Integration Tests (written, ready to use as refactor validation)

Tests are at: `api/Tests/Concertable.Web.IntegrationTests/Controllers/Artist/`
- `ArtistApiTests.cs` — 17 tests covering all 4 endpoints
- `ArtistRequestBuilders.cs` — test request factory helpers
- `ArtistMappers.cs` — form content serialization for requests

`SeedData` has `ArtistManagerNoArtist` — a seeded `ArtistManagerEntity` with no artist, used for Create and ownership tests. `ApiFixture` was extended with a `CreateClient(Guid, Role, Action<TestClientOptions>)` overload.

**Run before refactor to establish baseline:**
```
dotnet test --filter "FullyQualifiedName~ArtistApiTests"
```
All 17 should pass before you start moving files.

## Suggested Extraction Order
1. Create `Concertable.Artist.Contracts`, `Concertable.Artist.Application`, `Concertable.Artist.Infrastructure` projects
2. Move domain entities → `Concertable.Artist.Application` (or a Domain sub-folder)
3. Move interfaces + DTOs + validators → `Concertable.Artist.Application`
4. Move service + repo implementations → `Concertable.Artist.Infrastructure`
5. Create `IArtistModule` facade in Contracts, expose only what other modules need
6. Fix the `ReviewIdSelector` coupling (see friction point 1)
7. Update `OpportunityApplicationService` and `OwnershipService` to use `IArtistModule` (see friction points 3 & 4)
8. Remove `ArtistEntity` navigation properties from Concert-side repositories, replace with ID-only + service calls
9. Register via `services.AddArtistModule()` in `Program.cs`
10. Re-run the 15 Artist integration tests — all should still pass

## Reference
- Search module extraction (complete) is the pattern to follow: `Concertable.Search.Contracts`, `Concertable.Search.Application`, `Concertable.Search.Infrastructure`
- Kamil Grzybek modular monolith series: `modular-monolith-with-ddd` on GitHub
