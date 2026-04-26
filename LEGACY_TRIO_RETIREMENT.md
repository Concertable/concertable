# Legacy Trio Retirement — Vision and Cleanup Plan

`Concertable.Core`, `Concertable.Application`, and `Concertable.Infrastructure` were the legacy
monolith layer. They have been **deleted** (2026-04-26). Each module extraction pulled files out;
the last extraction (Customer) closed the loop.

**Status: ✅ COMPLETE (2026-04-26).** All 9 modules shipped (Search, Identity, Artist, Venue,
Concert, Contract, Payment, Notification, Messaging, Customer). `ApplicationDbContext` deleted.
All 3 legacy trio `.csproj` files deleted. Migrations re-scaffolded cleanly per-module:
Shared → Identity → Artist → Venue → Concert → Contract → Payment → Customer → Messaging
(no AppDb migration — context no longer exists). Build green across Web, Workers, IntegrationTests,
E2ETests, Infrastructure.UnitTests, Core.UnitTests.

**Final per-module pulls in this pass** (deferred from earlier extractions):
- Concert: `IQrCodeService`/`ITicketRepository`/`ITicketValidator`/`TicketMappers`/
  `TicketPurchaseParamsValidator`/`OpportunityApplicationWithStatus` → `Concert.Application` (internal);
  `ConcertValidator`/`OpportunityApplicationValidator`/`TicketValidator`/`PdfService`/`QrCodeService`/
  `ApplicationAcceptHandler`/`QueryableConcertMappers` → `Concert.Infrastructure` (internal);
  `TicketDto`/`TicketConcertDto`/`UserDto` → `Concert.Application/DTOs/TicketDtos.cs` (internal record).
  Concert.Infrastructure gained QRCoder + QuestPDF package refs and Artist.Domain + Venue.Domain
  project refs (for rating-projection types in QueryableConcertMappers).
- Identity: `UserMappers` deleted as dead code (no consumers).
- `IUnitOfWork`/`UnitOfWork` deleted (per `project_generic_uow`); `OpportunityApplicationService`
  now calls `applicationRepository.SaveChangesAsync()` directly with a `DbUpdateException` catch
  (per `feedback_module_service_saves_own_context`).
- `Concertable.Web/Extensions/ServiceCollectionExtensions.cs`: dropped
  `AddDbContext<ApplicationDbContext>`, `AddServiceValidators`, `AddScoped<IPdfService>`,
  `AddScoped<IQrCodeService>`, `AddSingleton<QRCodeGenerator>`, `IUnitOfWork` registration.
  Concert validators + QR/PDF + QRCodeGenerator now live inside `AddConcertModule`.
- `Concertable.Web/DevDbInitializer` + `Tests/.../TestDbInitializer`: shed `ApplicationDbContext`
  injection and `context.Database.MigrateAsync()` (each module seeder calls its own MigrateAsync).
- `Tests/.../ApiFixture`: dropped `public ApplicationDbContext DbContext` (was unused; tests use
  `fixture.ReadDbContext`).
- Project ref fan-out: Concertable.Seeding (gained Shared/Identity/Artist/Venue/Concert/Contract/
  Payment Domain refs + Microsoft.EntityFrameworkCore package ref); Concertable.Data.Application
  (gained Shared/Identity/Artist/Venue/Concert/Payment Domain refs); Identity/Artist/Venue
  Infrastructure (gained Concertable.Shared.Infrastructure ref for Geometry types); Identity.Infrastructure
  (gained BCrypt.Net-Next package ref); Search.Application (gained Identity.Contracts +
  Artist/Venue/Concert.Domain refs); Search.Infrastructure (gained LinqKit.Microsoft.EntityFrameworkCore
  package ref); Tests/Concertable.Core.UnitTests (gained Shared.Domain + Identity Contracts/Domain +
  Concert.Domain + Payment.Domain refs).

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

`DapperRepository` → `Shared.Infrastructure/Repositories/`. `GenreRepository` → **`Data.Infrastructure/Repositories/`**
(deviation from doc: it binds `SharedDbContext` which lives in `Data.Infrastructure`, and `SharedDbContext`
is `internal` — moving GenreRepository to Shared.Infrastructure would force inverting the
Data → Shared layer relationship and re-publicising the context. DI registration moved into
`AddSharedDbContext()` so Web no longer needs visibility into the internal type).
`PreferenceRepository` updated to extend the new two-arg `Repository<PreferenceEntity, ApplicationDbContext>`.

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

### Step 4 — Move Geometry + delete unused Helpers ✅
`GeometryCalculator`, `GeographicGeometryProvider`, `MetricGeometryProvider`, `GeometryProviderType`
moved to `Shared.Infrastructure/Services/Geometry/` (namespace `Concertable.Shared.Infrastructure.Services.Geometry`).
20 consumer using statements updated via sed pass.

`GeoApproximatorHelper` + `LocationHelper` **deleted, not moved** — both were dead code (no callers
across the codebase; `IGeometryProvider.CreatePoint` already covers `LocationHelper.CreatePoint`'s
job; `GeoApproximatorHelper.GetBoundingBox`/`IsWithinBoundingBox` had no consumers). Stale
`using Concertable.Infrastructure.Helpers;` lines stripped from 7 module repos in the same sed pass.

`Concertable.Shared.Infrastructure` ProjectReference added to `Concertable.Infrastructure.csproj`
so consumers of legacy infra (Identity/Venue/Artist Infrastructure) gain transitive visibility into
Shared.Infrastructure types during the migration window.

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

## Customer module extraction — collapses the trio

After Shared.Infrastructure is done, the Customer module is the last extraction (decided 2026-04-26: not "Preferences" — Customer is the destination for any future customer-aggregate state too).

**Module layout shipped:** 5 projects under `api/Modules/Customer/`:
- `Concertable.Customer.Contracts/` — `ICustomerModule` facade (consolidates `GetCustomerAsync` + `GetUserIdsByLocationAndGenresAsync`)
- `Concertable.Customer.Domain/` — `PreferenceEntity` + `GenrePreferenceEntity` (User nav dropped — UserId only since Identity is cross-module)
- `Concertable.Customer.Application/` — internal: DTOs, Requests, Mappers, Validators, IPreferenceRepository, IPreferenceService. `AssemblyInfo.cs` exposes IVT to Customer.Infrastructure / Customer.Api / Web.IntegrationTests / DynamicProxyGenAssembly2
- `Concertable.Customer.Infrastructure/` — `CustomerDbContext` (schema `customer`) + `CustomerConfigurationProvider` + EF configs (Preference + GenrePreference) + `PreferenceRepository : Repository<PreferenceEntity, CustomerDbContext>` + `PreferenceService` + `CustomerModule` facade impl + `CustomerDevSeeder`/`CustomerTestSeeder` (Order=7) + `AddCustomerModule()` DI extension
- `Concertable.Customer.Api/` — `PreferenceController` (`internal` + `InternalControllerFeatureProvider` to match Messaging precedent) + `AddCustomerApi()`

**Cross-module facade consolidation (2026-04-26):** Identity's old `ICustomerModule` (a user-subtype lookup, not a real facade) deleted. Both methods now live on `Concertable.Customer.Contracts.ICustomerModule`:
- `GetCustomerAsync(Guid userId)` — implementation calls `IIdentityModule.GetUserByIdAsync` then pattern-matches `as CustomerDto`. Was a misnamed Identity concern; Customer module is the real owner.
- `GetUserIdsByLocationAndGenresAsync(double lat, double lng, IEnumerable<int> genreIds)` — implementation calls `PreferenceService.GetByMatchingGenresAsync` then `IIdentityModule.GetUsersByIdsAsync` (new — batched user lookup added to IIdentityModule for this purpose) then in-memory radius filter via `IGeometryCalculator`. **No `IReadDbContext` use** (user vetoed — IReadDbContext is hacky workaround that will be removed).

The earlier rename of Identity's interface to `ICustomerLookup` was reverted: "Lookup" in this codebase means a request-scoped memoizing cache (cf. `IContractLookup` per `feedback_request_scoped_lookup`), not a generic user lookup. Wrong suffix → consolidate onto Customer.Contracts.ICustomerModule instead.

**Concert/Payment consumer rewrites:**
- `Concert.Infrastructure/Services/ConcertService.PostAsync` — was `await preferenceService.GetAsync()` then in-memory iteration with `IGeometryCalculator`. Now one call: `await customerModule.GetUserIdsByLocationAndGenresAsync(...)`. `IPreferenceService` + `IGeometryCalculator` deps removed from ConcertService.
- `Payment.Api/Controllers/StripeAccountController` + `Payment.Infrastructure/CustomerPaymentModule` — switched from `Identity.Contracts.ICustomerModule` to `Customer.Contracts.ICustomerModule`. Payment.Api + Payment.Infrastructure csprojs gained `Customer.Contracts` ProjectReference.
- Concert.Infrastructure csproj gained `Customer.Contracts` ProjectReference.

**Legacy file deletions (consequences of Customer extraction):**
- `Concertable.Core/Entities/PreferenceEntity.cs` + `GenrePreferenceEntity.cs`
- `Concertable.Application/Interfaces/{IPreferenceRepository,IPreferenceService}.cs`
- `Concertable.Application/DTOs/PreferenceDtos.cs` / `Mappers/PreferenceMappers.cs` / `Requests/PreferenceRequests.cs` / `Validators/PreferenceValidators.cs`
- `Concertable.Infrastructure/Repositories/PreferenceRepository.cs` / `Services/PreferenceService.cs` / `Data/AppDbConfigurationProvider.cs`
- `Concertable.Web/Controllers/PreferenceController.cs`
- `Data/Concertable.Data.Infrastructure/Data/Configurations/MiscEntityConfigurations.cs` (only held `PreferenceEntityConfiguration`)
- `Identity.Contracts/ICustomerModule.cs` + `Identity.Infrastructure/CustomerModule.cs` (consolidated onto Customer)
- `Seeding/Concertable.Seeding/Factories/PreferenceFactory.cs` (use `PreferenceEntity.Create(...)` directly)

**Other surfaces touched:**
- `ApplicationDbContext` — Preferences/GenrePreferences DbSets dropped; `AppDbConfigurationProvider().Configure(...)` line dropped; now only ExcludeFromMigrations carryover (deletes in next stage).
- `IReadDbContext` — `Preferences`/`GenrePreferences` IQueryables removed (no consumers — Concert was the only one and it no longer needs them).
- `DevDbInitializer` (Web) — `AppDbContext.Migrate()` call dropped; inline Preferences seeding (L50–71) gone (moved to `CustomerDevSeeder`); `Concertable.Core.Entities` import removed. Stays as the per-module `IDevSeeder` orchestrator.
- `Concertable.Web/Extensions/ServiceCollectionExtensions.cs` — `IPreferenceService`/`IPreferenceRepository` registrations dropped from `AddServices`/`AddRepositories`.
- `Concertable.Seeding.csproj` — `Concertable.Core` ProjectReference removed (no consumers after PreferenceFactory deletion).
- `Web/Program.cs` — `services.AddCustomerApi(builder.Configuration)` + `services.AddCustomerDevSeeder()`.
- `Concertable.Web.csproj` — added Customer.Api + Customer.Infrastructure ProjectReferences.
- `ApiFixture` (IntegrationTests) — `services.AddCustomerTestSeeder()`.
- `IIdentityModule` gained `GetUsersByIdsAsync(IEnumerable<Guid>)` — batched lookup. `IUserRepository` + `UserRepository` gained `GetByIdsAsync(IEnumerable<Guid>)`.
- Test files updated: `AuthApiTests` (`Concertable.Application.Requests` → `Concertable.Identity.Contracts`), `ConcertRequestBuilders` (→ `Concertable.Concert.Application.Requests`), `OpportunityRequestBuilders` (→ same).

**Once final stage ships:**
- `ApplicationDbContext.cs` deletes (no entities left to own).
- `Concertable.Core`, `Concertable.Application`, `Concertable.Infrastructure` `.csproj` files removed; references stripped from all consumers.
- `DevDbInitializer` (in `Concertable.Web`) **stays** — still the per-module `IDevSeeder` orchestrator; just no longer touches AppDbContext.

---

## Progress

### Cheap deletions
- [x] `ILatLong.cs`
- [x] `Enums/ImageType.cs` / `ManagerType.cs` / `ZoneType.cs`
- [x] Empty `Validators/Parameters/SearchParamsValidator.cs` (when superseded by Step 10 impl)
- [x] `Helpers/PaginationExtensions.cs` (duplicate) — Search.Infrastructure + Concert.Infrastructure now ProjectReference + global-using `Concertable.Shared.Infrastructure`

### Shared.Infrastructure extraction
- [x] Step 1 — Interfaces to Shared.Contracts / Shared.Domain
- [x] Step 2 — Background
- [x] Step 3 — Email + Blob + Geocoding + Image (incl. Resources/) — PdfService deferred
- [x] Step 4 — Geometry moved; Helpers deleted (dead code)
- [x] Step 5 — Repository consolidation (delete legacy + rename ModuleRepository types)
- [x] Step 6 — Remaining utilities
- [x] Step 7 — Composition (`GlobalExceptionHandler` + `DevDbInitializer` → Web; seeding contracts → Concertable.Seeding)
- [x] Step 8 — Update consumers
- [x] Step 9 — Owning-module settings (Auth/Stripe/PaymentIntentMappers)
- [x] Step 10 — Implement + wire `SearchParamsValidator`
- [x] Step 11 — Core cleanup (HeaderType / PageParams / RatingAggregate moves; enum deletions)

### Customer module extraction
- [x] 5 projects scaffolded under `api/Modules/Customer/` + added to .sln (auto-nested under Modules folder)
- [x] PreferenceEntity + GenrePreferenceEntity → Customer.Domain (User nav dropped)
- [x] Application layer moved as `internal` + `AssemblyInfo` IVT
- [x] CustomerDbContext + CustomerConfigurationProvider + EF configs + PreferenceRepository + PreferenceService
- [x] PreferencesController moved to Customer.Api (`internal` + InternalControllerFeatureProvider)
- [x] CustomerDevSeeder + CustomerTestSeeder (Order=7) — inline Preferences seeding from DevDbInitializer extracted
- [x] `ICustomerModule` facade in Customer.Contracts (consolidates `GetCustomerAsync` + `GetUserIdsByLocationAndGenresAsync`)
- [x] Identity's old `ICustomerModule` deleted; `IIdentityModule.GetUsersByIdsAsync` added for batched user lookup
- [x] Concert + Payment cross-module references rewired through Customer.Contracts
- [x] AppDbConfigurationProvider deleted, ApplicationDbContext.Preferences/GenrePreferences DbSets dropped, IReadDbContext.Preferences/GenrePreferences dropped
- [x] DevDbInitializer (Web) shed AppDb migrate + inline Preferences seeding (stays as orchestrator)
- [x] Build green

### Final trio retirement (2026-04-26)
- [x] Per-module pulls completed: Concert validators/services/mappers/handler/QrCode/Pdf/Tickets+UserDto,
      Identity UserMappers (dead code), IUnitOfWork retired
- [x] OpportunityApplicationService switched off IUnitOfWork → repo.SaveChangesAsync + DbUpdateException catch
- [x] ApplicationDbContext deleted (all entities now ExcludeFromMigrations and module-owned)
- [x] Legacy 3 `.csproj` files deleted (`Concertable.Core`, `Concertable.Application`, `Concertable.Infrastructure`)
- [x] All consumer ProjectReferences stripped + downstream package + project refs added
- [x] Migrations re-scaffolded: 9 module InitialCreate migrations, no AppDb migration
- [x] Build green: Web, Workers, IntegrationTests, E2ETests, Infrastructure.UnitTests, Core.UnitTests
