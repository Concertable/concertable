# Legacy Trio Retirement — Vision and Cleanup Plan

`Concertable.Core`, `Concertable.Application`, and `Concertable.Infrastructure` are the legacy
monolith layer. Each module extraction pulls files out; when the last domain-specific file
leaves a project, the project is deleted. This document tracks what remains after each planned
extraction and where it goes.

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
    Notification/   (planned — see NOTIFICATION_MODULE_REFACTOR.md)
    Messaging/      (planned — see MESSAGING_MODULE_REFACTOR.md)
    Preferences/    (future — home not yet decided per CLAUDE.md)
  Shared/
    Concertable.Shared.Domain/         (exists)
    Concertable.Shared.Contracts/      (exists — grows with shared interfaces below)
    Concertable.Shared.Infrastructure/ (exists — grows with Email/Blob/PDF/Geocoding/etc.)
    Concertable.Shared.Validation/     (exists)
  Data/
    Concertable.Data.Application/      (IReadDbContext — retires when modules stop using it)
    Concertable.Data.Infrastructure/   (ApplicationDbContext — retires when all tables extracted)
  Concertable.Web     (composition host — permanent)
  Concertable.Workers (background host — permanent)
```

`Concertable.Core`, `Concertable.Application`, and `Concertable.Infrastructure` do not appear
in this layout — they are retired, not renamed.

---

## Remaining files after Notification + Messaging extraction

### `Concertable.Infrastructure` → `Shared.Infrastructure`

Cross-cutting utilities with no domain owner. Moved as a standalone step (see Steps below).

| File | Destination |
|------|-------------|
| `Services/EmailService.cs` + `FakeEmailService.cs` | `Shared.Infrastructure/Services/Email/` |
| `Services/ImageService.cs` | `Shared.Infrastructure/Services/` |
| `Services/Blob/BlobStorageService.cs` + `FakeBlobStorageService.cs` | `Shared.Infrastructure/Services/Blob/` |
| `Services/PdfService.cs` | `Shared.Infrastructure/Services/` |
| `Services/GeocodingService.cs` + `ApiModels/Google*.cs` | `Shared.Infrastructure/Services/Geocoding/` |
| `Services/GeometryCalculator.cs` + `Geometry/` providers | `Shared.Infrastructure/Services/Geometry/` |
| `Services/UriService.cs` | `Shared.Infrastructure/Services/` |
| `Services/GenreService.cs` | `Shared.Infrastructure/Services/` |
| `Background/BackgroundTaskQueue.cs` + `BackgroundTaskRunner.cs` | `Shared.Infrastructure/Background/` |
| `Services/QueueHostedService.cs` | `Shared.Infrastructure/Background/` |
| `Helpers/GeoApproximatorHelper.cs` + `LocationHelper.cs` | `Shared.Infrastructure/Helpers/` |
| `Helpers/PaginationExtensions.cs` | **DELETE** — duplicate of `Shared.Infrastructure/Extensions/PaginationExtensions.cs` |
| `Expressions/ExpressionExtensions.cs` + `ParameterReplacer.cs` | `Shared.Infrastructure/Expressions/` |
| `Repositories/BaseRepository.cs`, `Repository.cs`, `GuidRepository.cs` | `Shared.Infrastructure/Repositories/` |
| `Repositories/DapperRepository.cs` | `Shared.Infrastructure/Repositories/` |
| `Repositories/GenreRepository.cs` | `Shared.Infrastructure/Repositories/` (SharedDbContext-backed) |
| `Extensions/DbUpdateExceptionExtensions.cs` | `Shared.Infrastructure/Extensions/` |
| `Settings/BlobStorageSettings.cs` | `Shared.Infrastructure/Settings/` |
| `Settings/UrlSettings.cs` | `Shared.Infrastructure/Settings/` |

### `Concertable.Infrastructure` → owning modules (during those modules' cleanup)

| File | Destination | When |
|------|-------------|------|
| `Validators/ConcertValidator.cs` | `Concert.Infrastructure/Validators/` | Concert cleanup |
| `Validators/OpportunityApplicationValidator.cs` | `Concert.Infrastructure/Validators/` | Concert cleanup |
| `Validators/TicketValidator.cs` | `Concert.Infrastructure/Validators/` | Concert cleanup |
| `Mappers/QueryableConcertMappers.cs` | `Concert.Infrastructure/Mappers/` | Concert cleanup |
| `Mappers/GenreSelectors.cs` | `Concert.Infrastructure/Mappers/` (verify consumers) | Concert cleanup |
| `Extensions/QueryableConcertExtensions.cs` | `Concert.Infrastructure/Extensions/` | Concert cleanup |
| `Handlers/ApplicationAcceptHandler.cs` | `Concert.Infrastructure/Handlers/` | Concert cleanup |
| `Services/QrCodeService.cs` | `Concert.Infrastructure/Services/` (ticket-specific with DB) | Concert cleanup |
| `Mappers/PaymentIntentMappers.cs` | `Payment.Infrastructure/` (check if already there) | Payment cleanup |
| `Settings/StripeSettings.cs` | `Payment.Infrastructure/` (check if already there) | Payment cleanup |
| `Settings/AuthSettings.cs` | `Identity.Infrastructure/` (check if already there) | Identity cleanup |
| `Services/PreferenceService.cs` | `Preferences.Infrastructure/` | Preferences module |
| `Repositories/PreferenceRepository.cs` | `Preferences.Infrastructure/` | Preferences module |
| `Repositories/UnitOfWork.cs` | Retires with `IUnitOfWork<TContext>` per `project_generic_uow.md` | — |

---

### `Concertable.Application` → `Shared.Contracts` (interfaces)

| File | Destination |
|------|-------------|
| `Interfaces/IEmailService.cs` | `Shared.Contracts/` |
| `Interfaces/IImageService.cs` | `Shared.Contracts/` |
| `Interfaces/Blob/IBlobStorageService.cs` | `Shared.Contracts/` |
| `Interfaces/IPdfService.cs` | `Shared.Contracts/` |
| `Interfaces/IQrCodeService.cs` | `Shared.Contracts/` or `Concert.Application` (check consumers) |
| `Interfaces/IGeocodingService.cs` | `Shared.Contracts/` |
| `Interfaces/Geometry/IGeometryCalculator.cs` + `IGeometryProvider.cs` | `Shared.Contracts/` |
| `Interfaces/IUriService.cs` | `Shared.Contracts/` |
| `Interfaces/IGenreService.cs` | `Shared.Contracts/` |

(Note: `IBackgroundTaskQueue` + `IBackgroundTaskRunner` already in `Shared.Contracts` — skip.)

### `Concertable.Application` → `Shared.Infrastructure`

| File | Destination |
|------|-------------|
| `Validators/Parameters/PageParamsValidator.cs` | `Shared.Infrastructure/Validators/` |
| `Serializers/TimeOnlyJsonConverter.cs` | `Shared.Infrastructure/Serializers/` |

### `Concertable.Application` → owning modules

| File | Destination | When |
|------|-------------|------|
| `Mappers/TicketMappers.cs` | `Concert.Application/Mappers/` | Concert cleanup |
| `Validators/TicketValidators.cs` | `Concert.Application/Validators/` | Concert cleanup |
| `Models/OpportunityApplicationWithStatus.cs` | `Concert.Application/Models/` | Concert cleanup |
| `Mappers/UserMappers.cs` | `Identity.Application/Mappers/` | Identity cleanup |
| `DTOs/PreferenceDto.cs` | `Preferences.Application/` | Preferences module |
| `Mappers/PreferenceMappers.cs` | `Preferences.Application/` | Preferences module |
| `Requests/PreferenceRequests.cs` | `Preferences.Application/` | Preferences module |
| `Validators/PreferenceValidators.cs` | `Preferences.Application/` | Preferences module |

---

### `Concertable.Core` — remaining entities

| Entity / Enum | Destination | When |
|---------------|-------------|------|
| `MessageEntity`, `MessageAction` | `Messaging.Domain/` | Messaging extraction |
| `PreferenceEntity` + enums | `Preferences.Domain/` | Preferences module |
| Concert/Opportunity/Ticket entities | `Concert.Domain/` | Check — may already be there |
| `StripeEventEntity`, `TransactionEntity` | `Payment.Domain/` | Check — may already be there |

---

## Shared.Infrastructure extraction — steps

This is a standalone step, independent of any single module extraction. Recommended after
Payment is complete and before Messaging begins (cleans up base classes modules depend on).

### Step 1 — Move interfaces to `Shared.Contracts`
Add `IEmailService`, `IBlobStorageService`, `IImageService`, `IPdfService`, `IGeocodingService`,
`IGeometryCalculator`, `IGeometryProvider`, `IUriService`, `IGenreService`.
(Skip `IBackgroundTaskQueue` + `IBackgroundTaskRunner` — already there.)

### Step 2 — Move Background
`BackgroundTaskQueue`, `BackgroundTaskRunner`, `QueueHostedService` →
`Shared.Infrastructure/Background/`. Register in `AddSharedInfrastructure()`.

### Step 3 — Move Email + Blob + PDF + Geocoding
Service implementations, Google API models, fakes. Bind settings in `AddSharedInfrastructure()`.
Fakes registered conditionally for dev/test environments.

### Step 4 — Move Geometry + Helpers
`GeometryCalculator`, providers, `GeoApproximatorHelper`, `LocationHelper`.

### Step 5 — Move shared repositories
`BaseRepository`, `Repository`, `GuidRepository`, `DapperRepository`, `GenreRepository`.
These are base classes used by module repos — update module Infrastructure project refs from
`Concertable.Infrastructure` → `Shared.Infrastructure`.

### Step 6 — Move remaining utilities
`ExpressionExtensions`, `ParameterReplacer`, `DbUpdateExceptionExtensions`, `UriService`,
`GenreService`, `TimeOnlyJsonConverter`, `PageParamsValidator`, settings.
**DELETE** `PaginationExtensions` (already in Shared.Infrastructure).

### Step 7 — Update all consumers
Modules still referencing `Concertable.Infrastructure` or `Concertable.Application` for
these utilities switch to `Shared.Infrastructure` / `Shared.Contracts`.

### Step 8 — Verify owning-module settings
Confirm `AuthSettings` is in `Identity.Infrastructure`, `StripeSettings` in `Payment.Infrastructure`.
Move any that are still in the legacy Infrastructure.

---

## Progress — Shared.Infrastructure extraction

- [ ] Step 1 — Move interfaces to `Shared.Contracts`
- [ ] Step 2 — Move Background
- [ ] Step 3 — Move Email + Blob + PDF + Geocoding
- [ ] Step 4 — Move Geometry + Helpers
- [ ] Step 5 — Move shared repositories
- [ ] Step 6 — Move remaining utilities; delete `PaginationExtensions` duplicate
- [ ] Step 7 — Update all consumers
- [ ] Step 8 — Verify owning-module settings
