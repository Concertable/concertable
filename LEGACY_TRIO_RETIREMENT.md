# Legacy Trio Retirement — Vision and Cleanup Plan

`Concertable.Core`, `Concertable.Application`, and `Concertable.Infrastructure` are the legacy
monolith layer. Each module extraction pulls files out; when the last domain-specific file
leaves a project, the project is deleted. This document tracks what remains after each planned
extraction and where it goes.

**Last refreshed: 2026-04-26** — corrected against current ground truth after Messaging shipped.
Module status: Search ✅, Identity ✅, Artist ✅, Venue ✅, Concert ✅, Contract ✅, Payment ✅,
Notification ✅, Messaging ✅. Remaining: Shared.Infrastructure cleanup + Preferences extraction.

---

## End-state project layout

```
api/
  Modules/
    Identity/       ✅
    Artist/         ✅
    Venue/          ✅
    Concert/        ✅
    Contract/       ✅
    Payment/        ✅
    Search/         ✅
    Notification/   ✅
    Messaging/      ✅
    Preferences/    (planned — gating task; collapses AppDbContext + AppDbConfigurationProvider + DevDbInitializer Preferences seeding)
  Shared/
    Concertable.Shared.Domain/         (exists — `IRepository<T>` joins existing IBaseRepository/IGuidRepository/IIdRepository)
    Concertable.Shared.Contracts/      (exists — grows with Email/Blob/Geocoding/etc. interfaces)
    Concertable.Shared.Infrastructure/ (exists — grows with Email/Blob/PDF/Geocoding/etc. impls)
    Concertable.Shared.Validation/     (exists)
  Data/
    Concertable.Data.Application/      (IReadDbContext — retires when modules stop using it)
    Concertable.Data.Infrastructure/   (ApplicationDbContext lives here today; retires when Preferences extracts)
  Seeding/
    Concertable.Seeding/               (IModuleSeeder/IDevSeeder/ITestSeeder/IDbInitializer move here)
  Concertable.Web     (composition host — permanent; GlobalExceptionHandler moves here)
  Concertable.Workers (background host — permanent)
```

`Concertable.Core`, `Concertable.Application`, and `Concertable.Infrastructure` do not appear
in this layout — they are retired, not renamed.

---

## Current legacy footprint (verified 2026-04-26)

### `Concertable.Core` — 7 source files

| File | Destination | Notes |
|------|-------------|-------|
| `Entities/PreferenceEntity.cs` + `GenrePreferenceEntity.cs` | `Preferences.Domain/` | Blocks AppDbContext retirement |
| `Enums/HeaderType.cs` | `Search.Contracts/` | Only Search consumes it |
| `Enums/ImageType.cs` | **DELETE** | Empty enum, no consumers |
| `Enums/ManagerType.cs` | **DELETE** | No consumers |
| `Enums/ZoneType.cs` | **DELETE** | No consumers (re-add in `Concert.Domain` when ticket zones land) |
| `Parameters/PageParams.cs` | `Shared.Contracts/` | Sits next to existing `IPageParams`; 38 files reference it |
| `Projections/RatingAggregate.cs` | `Shared.Domain/` | Primitive `(int, double)` record; consumed by Artist/Venue/Concert review services. Final home decided during the rating-pipeline rewrite (`project_search_rating_projection_ownership`); Shared.Domain is the safe interim |

### `Concertable.Application`

**Deletions (dead code):**
| File | Reason |
|------|--------|
| `Interfaces/ILatLong.cs` | Declared, never used |
| `Interfaces/IUnitOfWork.cs` | Retiring per `project_generic_uow` |

**`Validators/Parameters/SearchParamsValidator.cs` — restore, don't delete.** File is empty in
legacy; move skeleton to `Search.Application/Validators/` and **implement it** to validate
`SearchParams` (page bounds via PageParamsValidator inheritance, sort field allowlist,
lat/long range, radius bounds). Wire into Search FluentValidation registration.

**`DTOs/SharedDtos.cs` — split per type:**
| Type | Destination |
|------|-------------|
| `EmailDto`, `AttachmentDto` | `Shared.Contracts/` (email infra contract) ✅ |
| `CoordinatesDto`, `LocationDto` | `Shared.Contracts/` (geocoding contract — `LocationDto` reassigned from `Identity.Contracts/` 2026-04-26: it's a geocoding response type, not an identity type, and Identity already exposes `County`/`Town` on `IUser`) ✅ |
| `UserDto` | `Identity.Contracts/` — verify against existing Identity DTOs first; likely duplicate of the `IUser` hierarchy |
| `TicketConcertDto`, `TicketDto` | `Concert.Application/` |

**Generic repository contracts:**
| File | Destination |
|------|-------------|
| `Interfaces/IRepository.cs` | `Shared.Domain/` (alongside `IBaseRepository`/`IGuidRepository`/`IIdRepository`) |
| `Interfaces/IDapperRepository.cs` | `Shared.Contracts/` |
| `Interfaces/IGenreRepository.cs` | `Shared.Contracts/` (`GenreEntity` already shared) |

**Cross-cutting service interfaces → `Shared.Contracts/`:**
| File |
|------|
| `Interfaces/IEmailService.cs` |
| `Interfaces/IImageService.cs` |
| `Interfaces/Blob/IBlobStorageService.cs` |
| `Interfaces/IPdfService.cs` |
| `Interfaces/IGeocodingService.cs` |
| `Interfaces/Geometry/IGeometryCalculator.cs` + `IGeometryProvider.cs` |
| `Interfaces/IUriService.cs` |
| `Interfaces/IGenreService.cs` |

(`IBackgroundTaskQueue` + `IBackgroundTaskRunner` already in `Shared.Contracts` — skip.)

**Seeding contracts → `Concertable.Seeding/`:**
| File |
|------|
| `Interfaces/IModuleSeeder.cs` (with `IDevSeeder`/`ITestSeeder`) |
| `Interfaces/IDbInitializer.cs` |

**Owning-module pulls:**
| File | Destination |
|------|-------------|
| `Interfaces/IPreferenceRepository.cs` + `IPreferenceService.cs` | `Preferences.Application/` |
| `Interfaces/ITicketRepository.cs` + `ITicketValidator.cs` | `Concert.Application/` |
| `Interfaces/IQrCodeService.cs` | `Concert.Application/` (ticket-specific) |
| `Mappers/TicketMappers.cs` | `Concert.Application/Mappers/` |
| `Validators/TicketValidators.cs` | `Concert.Application/Validators/` |
| `Models/OpportunityApplicationWithStatus.cs` | `Concert.Application/Models/` |
| `Mappers/UserMappers.cs` | `Identity.Application/Mappers/` |
| `DTOs/PreferenceDtos.cs` | `Preferences.Application/` |
| `Mappers/PreferenceMappers.cs` | `Preferences.Application/` |
| `Requests/PreferenceRequests.cs` | `Preferences.Application/` |
| `Validators/PreferenceValidators.cs` | `Preferences.Application/` |

**Shared.Infrastructure pulls:**
| File | Destination |
|------|-------------|
| `Validators/Parameters/PageParamsValidator.cs` | `Shared.Infrastructure/Validators/` |
| `Serializers/TimeOnlyJsonConverter.cs` | `Shared.Infrastructure/Serializers/` |

### `Concertable.Infrastructure`

**Deletions:**
| File | Reason |
|------|--------|
| `Repositories/BaseRepository.cs`, `Repository.cs`, `GuidRepository.cs` | Tied to `ApplicationDbContext`; superseded by `Data.Infrastructure/ModuleRepository.cs` (see Repository consolidation below) |
| `Repositories/UnitOfWork.cs` | Retires with `IUnitOfWork` per `project_generic_uow` |
| `Helpers/PaginationExtensions.cs` | Duplicate of `Shared.Infrastructure/PaginationExtensions.cs` |
| `Data/ApplicationDbContext.cs` | Only owns `Preferences`/`GenrePreferences`; retires when Preferences extracts. Lines 41-87 are pure `ExcludeFromMigrations` carryover |
| `Data/AppDbConfigurationProvider.cs` | Only applies `PreferenceEntityConfiguration` — moves to Preferences module |
| `Migrations/` | Per `project_concert_migration_reset` rule |

**Repository consolidation (Step 5 — replaces "move to Shared.Infrastructure"):**
The legacy `BaseRepository`/`Repository`/`GuidRepository` are AppDbContext-bound and superseded
by `Data.Infrastructure/ModuleRepository.cs` which is generic over `DbContextBase`. Plan:
1. Delete the three legacy classes.
2. Rename the `Data.Infrastructure` types to drop the `Module` prefix:
   - `ModuleRepository<TEntity, TContext>` → `BaseRepository<TEntity, TContext>`
   - `GuidModuleRepository<TEntity, TContext>` → `GuidRepository<TEntity, TContext>`
   - `IdModuleRepository<TEntity, TContext>` → `Repository<TEntity, TContext>`
3. Rename the file `Data.Infrastructure/ModuleRepository.cs` → `BaseRepository.cs`.
4. Audit module repos (Identity/Artist/Venue/Concert/Contract/Payment/Messaging) — every
   `: ModuleRepository<...>` / `GuidModuleRepository<...>` / `IdModuleRepository<...>` updates
   in the same pass.

`DapperRepository` and `GenreRepository` move separately to `Shared.Infrastructure/Repositories/`
— verify their context binding (`GenreRepository` likely binds `SharedDbContext`).

**Composition-host pulls:**
| File | Destination |
|------|-------------|
| `GlobalExceptionHandler.cs` | `Concertable.Web/` |
| `Data/DevDbInitializer.cs` | `Concertable.Seeding/` (Preferences seeding lines 50-71 extract into a `PreferenceDevSeeder`) |
| `Resources/avatar.png` + `banner.png` | `Shared.Infrastructure/Resources/` (used by `FakeImageService`; update embedded resource path to `Concertable.Shared.Infrastructure.Resources.<file>`) |

**Shared.Infrastructure pulls (cross-cutting impls):**
| File | Destination |
|------|-------------|
| `Services/EmailService.cs` + `Email/FakeEmailService.cs` | `Shared.Infrastructure/Services/Email/` |
| `Services/ImageService.cs` + `Blob/FakeImageService.cs` | `Shared.Infrastructure/Services/` |
| `Services/Blob/BlobStorageService.cs` + `FakeBlobStorageService.cs` | `Shared.Infrastructure/Services/Blob/` |
| `Services/PdfService.cs` | `Shared.Infrastructure/Services/` |
| `Services/GeocodingService.cs` + `ApiModels/Google*.cs` | `Shared.Infrastructure/Services/Geocoding/` |
| `Services/Geometry/*.cs` (Calculator + providers + ProviderType) | `Shared.Infrastructure/Services/Geometry/` |
| `Services/UriService.cs` | `Shared.Infrastructure/Services/` |
| `Services/GenreService.cs` | `Shared.Infrastructure/Services/` |
| `Background/BackgroundTaskQueue.cs` + `BackgroundTaskRunner.cs` | `Shared.Infrastructure/Background/` |
| `Services/QueueHostedService.cs` | `Shared.Infrastructure/Background/` |
| `Helpers/GeoApproximatorHelper.cs` + `LocationHelper.cs` | `Shared.Infrastructure/Helpers/` |
| `Expressions/ExpressionExtensions.cs` + `ParameterReplacer.cs` | `Shared.Infrastructure/Expressions/` |
| `Repositories/DapperRepository.cs` | `Shared.Infrastructure/Repositories/` |
| `Repositories/GenreRepository.cs` | `Shared.Infrastructure/Repositories/` (verify `SharedDbContext`-backed) |
| `Extensions/DbUpdateExceptionExtensions.cs` | `Shared.Infrastructure/Extensions/` |
| `Settings/BlobStorageSettings.cs` | `Shared.Infrastructure/Settings/` |
| `Settings/UrlSettings.cs` | `Shared.Infrastructure/Settings/` |

**Owning-module pulls (verified NOT yet there as of 2026-04-26):**
| File | Destination |
|------|-------------|
| `Validators/ConcertValidator.cs` | `Concert.Infrastructure/Validators/` |
| `Validators/OpportunityApplicationValidator.cs` | `Concert.Infrastructure/Validators/` |
| `Validators/TicketValidator.cs` | `Concert.Infrastructure/Validators/` |
| `Mappers/QueryableConcertMappers.cs` | `Concert.Infrastructure/Mappers/` |
| `Mappers/GenreSelectors.cs` | `Concert.Infrastructure/Mappers/` (verify consumers) |
| `Extensions/QueryableConcertExtensions.cs` | `Concert.Infrastructure/Extensions/` (verify still exists) |
| `Handlers/ApplicationAcceptHandler.cs` | `Concert.Infrastructure/Handlers/` |
| `Services/QrCodeService.cs` | `Concert.Infrastructure/Services/` |
| `Mappers/PaymentIntentMappers.cs` | `Payment.Infrastructure/` |
| `Settings/StripeSettings.cs` | `Payment.Infrastructure/` |
| `Settings/AuthSettings.cs` | `Identity.Infrastructure/` |
| `Services/PreferenceService.cs` | `Preferences.Infrastructure/` |
| `Repositories/PreferenceRepository.cs` | `Preferences.Infrastructure/` |

---

## Execution order

Preferences extraction is the **gating task** that collapses AppDbContext, AppDbConfigurationProvider,
and the inline Preferences seeding in `DevDbInitializer` together.

1. **Cheap deletions** — `ILotLong`, `ImageType`, `ManagerType`, `ZoneType`, `Helpers/PaginationExtensions` (duplicate). Reduces churn for the moves below.
2. **Shared.Infrastructure extraction** (Steps below) — most of the cross-cutting volume.
3. **Per-module pulls** during each module's cleanup pass — Concert/Identity/Payment grab their leftover validators/mappers/settings.
4. **`SearchParamsValidator` move + implement** in Search.
5. **Preferences module extraction** — last; kills all three legacy projects together.

---

## Shared.Infrastructure extraction — steps

Standalone effort, runs before Preferences.

### Step 1 — Move interfaces to `Shared.Contracts` ✅
Add `IEmailService`, `IBlobStorageService`, `IImageService`, `IPdfService`, `IGeocodingService`,
`IGeometryCalculator`, `IGeometryProvider`, `IUriService`, `IGenreService`,
`IDapperRepository`, `IGenreRepository`, `EmailDto`/`AttachmentDto`/`CoordinatesDto`/`LocationDto`.
Move `IRepository<T>` to `Shared.Domain`. Skip `IBackgroundTaskQueue`/`IBackgroundTaskRunner` (already there).
Namespaces preserved (`Concertable.Application.Interfaces`, `Concertable.Application.DTOs`) for minimum-churn —
matches the `IBaseRepository`/`IIdRepository`/`IGuidRepository` precedent already sitting in `Shared.Domain`.

### Step 2 — Move Background ✅
`BackgroundTaskQueue`, `BackgroundTaskRunner`, `QueueHostedService` →
`Shared.Infrastructure/Background/` (public, namespace `Concertable.Shared.Infrastructure.Background`).
Queue + Runner registered (`Singleton`) in `AddSharedInfrastructure()`. Hosted-service registration
exposed as separate `AddQueueHostedService()` (Web hosts the drain loop; Workers does not).
Payment's private duplicates (`Payment.Infrastructure/Background/*`) deleted; their global
`IBackgroundTaskQueue`/`IBackgroundTaskRunner` registrations removed from
`AddPaymentInfrastructure()`. `AddPaymentQueueHostedService` collapsed into
`AddQueueHostedService` in Web/Program.cs.

### Step 3 — Move Email + Blob + Geocoding + Image ✅ (PdfService deferred)
Service implementations, fakes, Google API models moved to `Shared.Infrastructure/Services/{Email,Blob,Geocoding}/`.
`BlobStorageSettings` → `Shared.Infrastructure/Settings/`. `Resources/avatar.png` + `banner.png`
moved to `Shared.Infrastructure/Resources/`; embedded resource path updated to
`Concertable.Shared.Infrastructure.Resources.<file>`.

`AddSharedInfrastructure()` now takes `IConfiguration` and centralises:
- `Configure<BlobStorageSettings>` bind
- Conditional fake/real registration for `IBlobStorageService` / `IEmailService` / `IImageService`
  (driven by `ExternalServices:UseRealBlob` / `UseRealEmail` / `UseRealImages`)
- `AddHttpClient("Geocoding", ...)` + `IGeocodingService` registration

Web/Workers updated to pass `IConfiguration`. Web/`AddExternalServices` deleted.

**`UrlSettings` bind deferred to Step 6** (UriService still in legacy until Step 6). **PdfService
deferred** — depends on `IQrCodeService` which is bound for `Concert.Application` per the doc.
Moving PdfService to `Shared.Infrastructure` would create a Shared→Concert direction. PdfService
is also ticket-specific (sole method `GenerateTicketReciptAsync`) — likely belongs in
`Concert.Infrastructure` alongside `QrCodeService`. Final home revisited under owning-module pulls
or a "TicketReceiptService" rename pass.

### Step 4 — Move Geometry + Helpers
`GeometryCalculator`, providers, `GeoApproximatorHelper`, `LocationHelper`.

### Step 5 — Repository consolidation (delete + rename, NOT move)
- DELETE legacy `BaseRepository`/`Repository`/`GuidRepository` (AppDbContext-bound).
- Rename `Data.Infrastructure/ModuleRepository.cs` types: drop `Module` prefix
  (`ModuleRepository` → `BaseRepository`, `GuidModuleRepository` → `GuidRepository`,
  `IdModuleRepository` → `Repository`). Rename file to `BaseRepository.cs`.
- Update every module repo's base-class reference in the same pass.
- Move `DapperRepository` + `GenreRepository` to `Shared.Infrastructure/Repositories/`.

### Step 6 — Move remaining utilities
`ExpressionExtensions`, `ParameterReplacer`, `DbUpdateExceptionExtensions`, `UriService`,
`GenreService`, `TimeOnlyJsonConverter`, `PageParamsValidator`, `BlobStorageSettings`, `UrlSettings`.

### Step 7 — Composition + seeding moves
`GlobalExceptionHandler` → `Concertable.Web`. `DevDbInitializer` + `IDbInitializer` +
`IModuleSeeder`/`IDevSeeder`/`ITestSeeder` → `Concertable.Seeding`.

### Step 8 — Update consumers
Modules still referencing `Concertable.Infrastructure` or `Concertable.Application` for
these utilities switch to `Shared.Infrastructure` / `Shared.Contracts` / `Shared.Domain`.

### Step 9 — Owning-module settings
Move `AuthSettings` to `Identity.Infrastructure`, `StripeSettings` + `PaymentIntentMappers` to
`Payment.Infrastructure`. Verify nothing else still depends on them in legacy.

### Step 10 — Search params validator
Move + implement `SearchParamsValidator` in `Search.Application/Validators/`. Wire registration.

### Step 11 — Core cleanup
Move `HeaderType` → `Search.Contracts`. Move `PageParams` → `Shared.Contracts`. Move
`RatingAggregate` → `Shared.Domain`. Delete `ImageType`/`ManagerType`/`ZoneType`.

---

## Preferences module extraction — collapses the trio

After Shared.Infrastructure is done, Preferences is the last extraction. It pulls:
- `PreferenceEntity` + `GenrePreferenceEntity` (Concertable.Core) → `Preferences.Domain`
- `PreferenceDtos`/`PreferenceRequests`/`PreferenceMappers`/`PreferenceValidators` (Concertable.Application) → `Preferences.Application`
- `IPreferenceRepository`/`IPreferenceService` (Concertable.Application) → `Preferences.Application`
- `PreferenceService` + `PreferenceRepository` (Concertable.Infrastructure) → `Preferences.Infrastructure`
- `PreferenceEntityConfiguration` (Data.Infrastructure) → `Preferences.Infrastructure/Data/Configurations/`
- Inline Preferences seeding (DevDbInitializer 50-71) → `PreferenceDevSeeder`

Once Preferences ships:
- `ApplicationDbContext` deletes (no entities left to own).
- `AppDbConfigurationProvider` deletes (only `PreferenceEntityConfiguration` consumer).
- `Concertable.Core`, `Concertable.Application`, `Concertable.Infrastructure` `.csproj` files removed; references stripped.

---

## Progress

### Cheap deletions
- [x] `ILatLong.cs`
- [x] `Enums/ImageType.cs` / `ManagerType.cs` / `ZoneType.cs`
- [ ] Empty `Validators/Parameters/SearchParamsValidator.cs` (when superseded by Step 10 impl)
- [x] `Helpers/PaginationExtensions.cs` (duplicate) — Search.Infrastructure + Concert.Infrastructure now ProjectReference + global-using `Concertable.Shared.Infrastructure`

### Shared.Infrastructure extraction
- [x] Step 1 — Interfaces to Shared.Contracts / Shared.Domain
- [x] Step 2 — Background
- [x] Step 3 — Email + Blob + Geocoding + Image (incl. Resources/) — PdfService deferred
- [ ] Step 4 — Geometry + Helpers
- [ ] Step 5 — Repository consolidation (delete legacy + rename ModuleRepository types)
- [ ] Step 6 — Remaining utilities
- [ ] Step 7 — Composition (`GlobalExceptionHandler` → Web; `DevDbInitializer` + seeding contracts → Concertable.Seeding)
- [ ] Step 8 — Update consumers
- [ ] Step 9 — Owning-module settings (Auth/Stripe/PaymentIntentMappers)
- [ ] Step 10 — Implement + wire `SearchParamsValidator`
- [ ] Step 11 — Core cleanup (HeaderType / PageParams / RatingAggregate moves; enum deletions)

### Preferences extraction
- [ ] Domain + Application + Infrastructure scaffolded
- [ ] DbContext + InitialCreate migration
- [ ] DevSeeder + TestSeeder
- [ ] AppDbContext / AppDbConfigurationProvider / DevDbInitializer deleted
- [ ] Legacy trio `.csproj` files removed
