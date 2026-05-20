# Kernel Split Plan v2

> **Status (2026-05-20):** Phases **A + C landed `952b75fb`**, **Phase B landed next commit** on `Refactor/Microservices`. Build green. DataAccess + Seeding + Shared.Blob + Shared.Email all extracted. Phases D / E / F / G / H / I still pending.

Supersedes `KERNEL_SPLIT_PLAN.md`. v1 treated this as "extract DataAccess and leave the rest of Kernel alone for a later pass." That approach broke down once we realised:

- Bulk-sedding `Concertable.Application.Interfaces` → `Concertable.DataAccess` accidentally rewrote Kernel files that stayed in Kernel (`IImageService`, `IEmailService`, `IGeocodingService`, `IUriService`, `IPdfService` now declare themselves in the `Concertable.DataAccess` namespace while physically living in Kernel.dll — cross-assembly namespace pollution).
- `IModuleSeeder` / `IDevSeeder` / `ITestSeeder` aren't data-access concerns. Putting them in `DataAccess.Application` dragged `BlobDevSeeder` along, which is even worse — blob seeding isn't data access either.
- "Adapter" terminology was rejected. The adapter interfaces become `Concertable.Shared.{Email,Blob,Geocoding,Imaging,Pdf}`, each scoped lib (NOT one dumpster `Concertable.Shared`).
- Per the modular-monolith rules, each shared lib gets clean-arch layering (`X.Application` interface csproj + `X.Infrastructure` impl csproj) — always, even when contents are thin.

v2 plans the full split in one coherent pass.

---

## Target end state (full lib inventory)

| Lib | Purpose | Csproj shape |
|---|---|---|
| `Concertable.Kernel` | Domain primitives — entity markers, domain events, value types, exceptions | Single (no clean-arch split) |
| `Concertable.Contracts` | Cross-service wire types | Single |
| `Concertable.Messaging` | Bus + outbox (already extracted ✅) | Already 4-csproj (Contracts/Domain/Application/Infrastructure) |
| `Concertable.Messaging.AzureServiceBus` | ASB transport (parallel agent) | Single |
| `Concertable.DataAccess` | Repository/UoW abstractions + EF base + interceptors + specs | 2-csproj (Application + Infrastructure) |
| `Concertable.Seeding` | Seeding contracts (`IModuleSeeder`/`IDevSeeder`/`ITestSeeder`) + Fakers/Factories/SeedData | Single (already exists, expand) |
| `Concertable.BackgroundTasks` | `IBackgroundTaskQueue`/`IBackgroundTaskRunner` + `QueueHostedService` | Single (3 files, no split warranted) |
| `Concertable.AspNetCore` | Middleware, problem details, global exception handler, IUriService impl | 2-csproj |
| `Concertable.Observability` | OTel setup, structured logging | 2-csproj, materialises at Phase 4 Step 17 |
| `Concertable.Testing` | Testcontainers helpers + xUnit collection fixtures + ApiFixture | Single (rename of `Concertable.IntegrationTests.Common` / `Concertable.Tests.Common`) |
| `Concertable.Shared.Email` | Email adapter | 2-csproj |
| `Concertable.Shared.Blob` | Blob storage adapter (includes `BlobDevSeeder`) | 2-csproj |
| `Concertable.Shared.Geocoding` | Geocoding adapter | 2-csproj |
| `Concertable.Shared.Imaging` | Image processing adapter | 2-csproj |
| `Concertable.Shared.Pdf` | PDF rendering adapter | 2-csproj |

`Shared.X` naming: explicitly NOT a single shared lib — each is its own scoped package with one purpose. Avoids the "one big Shared dumpster" antipattern.

---

## Execution order

Each phase is one commit. Order matters — earlier phases unblock later ones.

| Phase | Status | Work | Rationale |
|---|---|---|---|
| **A** | ✅ `952b75fb` | Move `IModuleSeeder`/`IDevSeeder`/`ITestSeeder` from `DataAccess.Application` → `Concertable.Seeding`. Defer `BlobDevSeeder`; it lands in Phase C. | Cleans up DataAccess. Build still broken until Phase C — that's fine. |
| **C** | ✅ `952b75fb` | Extract `Concertable.Shared.Blob`. `IBlobStorageService` → `.Application`. `BlobStorageService` + `FakeBlobStorageService` + `BlobStorageSettings` + `SeedImages` (resources) + **`BlobDevSeeder`** → `.Infrastructure`. | BlobDevSeeder lands in its forever home. Phase A finishes building. |
| **B** | ✅ landed | Extract `Concertable.Shared.Email`. `IEmailService` → `.Application`. Real `EmailService` (MailKit) + generic logging `FakeEmailService` → `.Infrastructure` (both `internal sealed`). `EmailDto`/`AttachmentDto` moved out of `Concertable.Contracts` and into `.Infrastructure` as internal. User module's auto-verifying fake renamed to `AutoVerifyingFakeEmailService` and overrides `AddSharedEmail` when `UseRealEmail=false`. Customer.Web duplicate fake + User stub `EmailService` deleted. MailKit/MimeKit dropped from Kernel.csproj. `AddSharedEmail(IConfiguration)` wired into all 4 hosts. | Independent of others. |
| **D** | ⏳ pending | Extract `Concertable.Shared.Geocoding`. `IGeocodingService` → `.Application`. `GeocodingService` + `GoogleGeocodeResponse`/`GoogleGeocodeResult`/`GoogleAddressComponent` → `.Infrastructure`. | Independent. |
| **E** | ⏳ pending | Extract `Concertable.Shared.Imaging`. `IImageService` → `.Application`. `ImageService` → `.Infrastructure`. | Independent. Also drops the temporary Kernel → Shared.Blob.Application ref that ImageService introduced. |
| **F** | ⏳ pending | Extract `Concertable.Shared.Pdf`. `IPdfService` → `.Application`. Locate current impl (likely `Concertable.Concert.Infrastructure` QuestPDF wrapper) → `.Infrastructure`. | Independent. |
| **G** | ⏳ pending | Rename `Concertable.IntegrationTests.Common` (or `Concertable.Tests.Common` — confirm which) → `Concertable.Testing`. Single csproj, no clean-arch split. | Independent, cosmetic. |
| **H** | ⏳ pending (incl. in commit) | Revert bogus namespace rewrites on remaining Kernel files. Fix any stragglers. | After all above moves, audit Kernel for cross-assembly namespace leaks. **Note:** the 5 originally-rewritten files (`IImageService.cs`, `IEmailService.cs`, `IGeocodingService.cs`, `IUriService.cs`, `IPdfService.cs`) were left in their rewritten state for Phase C; B/D/E/F will move them to their proper homes, dissolving the issue. |
| **I** | ⏳ pending | Delete `api/Data/` folder (now empty). Re-scaffold migrations (`./initial-migrations.ps1`). Commit. | Final cleanup. |

Deferred (separate doc):
- `Concertable.BackgroundTasks` extraction (Plan B in v1, still pending).
- `Concertable.AspNetCore` extraction (when next touching middleware).
- `Concertable.Observability` (Phase 4 Step 17).
- `Concertable.DataAccess` → `Concertable.Persistence` rename: **decision locked as DataAccess, no rename**.

---

## Per-lib file moves

### `Concertable.DataAccess.Application` (already done in v1 — preserve)

Stays in `api/Concertable.DataAccess/Concertable.DataAccess.Application/`. Namespace `Concertable.DataAccess`.

Files (from `Concertable.Kernel/` + `Concertable.Data.Application/`):
- `IBaseRepository.cs`, `IRepository.cs`, `IGuidRepository.cs`, `IIdRepository.cs`, `IDapperRepository.cs`
- `IUnitOfWork.cs`, `IUnitOfWorkBehavior.cs`
- `IReadDbContext.cs`
- `IDbInitializer.cs`
- `Specifications/IDateRangeSpecification.cs`, `Specifications/IUpcomingSpecification.cs`
- `Diffing/CollectionSyncer.cs`, `Diffing/ICollectionSyncer.cs`, `Diffing/ISyncRequest.cs`

**Remove**: `IModuleSeeder.cs` (moves to Seeding in Phase A).

Csproj refs: `Concertable.Kernel` + all `Module.Domain` csprojs.

### `Concertable.DataAccess.Infrastructure` (already done in v1 — preserve, with corrections)

Stays in `api/Concertable.DataAccess/Concertable.DataAccess.Infrastructure/`. Namespace `Concertable.DataAccess.Infrastructure`.

Files (from `Concertable.Kernel/` + `Concertable.Data.Infrastructure/`):
- `BaseRepository.cs`, `DbContextBase.cs`, `Schema.cs`, `UnitOfWork.cs`, `UnitOfWorkBehavior.cs`
- `Data/AuditInterceptor.cs`, `Data/DomainEventDispatchInterceptor.cs`
- `Data/IEntityTypeConfigurationProvider.cs`, `Data/IRatingProjectionConfigurationProvider.cs`
- `Data/ReadDbContext.cs`
- `Expressions/ExpressionExtensions.cs`, `Expressions/ParameterReplacer.cs`
- `Extensions/DbUpdateExceptionExtensions.cs`, `Extensions/ServiceCollectionExtensions.cs` (`AddReadDbContext()`)
- `Repositories/DapperRepository.cs`
- `Specifications/DateRangeSpecification.cs`, `Specifications/UpcomingSpecification.cs`
- `PaginationExtensions.cs`

**Remove**: `Data/Seeders/BlobDevSeeder.cs` (moves to `Concertable.Shared.Blob.Infrastructure` in Phase C).

Csproj refs: `Concertable.DataAccess.Application` + `Concertable.Kernel` + EF/Dapper packages. Drop `Concertable.Seeding` ref (no longer needed once BlobDevSeeder leaves).

### `Concertable.Seeding` (Phase A — expand existing)

Stays at `api/Seeding/Concertable.Seeding/`. Namespace `Concertable.Seeding`.

**Add** (moved from `DataAccess.Application/IModuleSeeder.cs`):
- `IModuleSeeder.cs` containing `IModuleSeeder`, `IDevSeeder`, `ITestSeeder`. Namespace `Concertable.Seeding`.

No csproj changes needed (Seeding already refs Kernel + module Domains).

### `Concertable.Shared.Blob` (Phase C)

New libs at `api/Shared/Concertable.Shared.Blob/`:
- `Concertable.Shared.Blob.Application/` — namespace `Concertable.Shared.Blob`
- `Concertable.Shared.Blob.Infrastructure/` — namespace `Concertable.Shared.Blob.Infrastructure`

**Application** (from `Concertable.Kernel/Blob/`):
- `IBlobStorageService.cs` → namespace `Concertable.Shared.Blob`

**Infrastructure** (from `Concertable.Kernel/Services/Blob/` + `Concertable.Kernel/Resources/` + `Concertable.Kernel/Settings/` + `Concertable.DataAccess.Infrastructure/Data/Seeders/`):
- `BlobStorageService.cs`, `FakeBlobStorageService.cs` → namespace `Concertable.Shared.Blob.Infrastructure`
- `BlobStorageSettings.cs` → namespace `Concertable.Shared.Blob.Infrastructure`
- `SeedImages.cs` + `Resources/avatar.png` + `Resources/banner.png` (EmbeddedResource entries) → namespace `Concertable.Shared.Blob.Infrastructure`
- **`BlobDevSeeder.cs`** → namespace `Concertable.Shared.Blob.Infrastructure`
- `Extensions/ServiceCollectionExtensions.cs` — exposes `AddSharedBlob(IConfiguration)` (binds settings, registers BlobServiceClient or fake based on config flag, registers `IBlobStorageService`) and `AddBlobDevSeeder()` (opt-in; registers `BlobDevSeeder` as `IDevSeeder`).

Csproj refs:
- `Application` → `Concertable.Kernel` (zero deps otherwise)
- `Infrastructure` → `Application` + `Concertable.Kernel` + `Concertable.Seeding` (for `IDevSeeder`) + `Azure.Storage.Blobs` package + `Microsoft.Extensions.*` packages

### `Concertable.Shared.Email` (Phase B)

`api/Shared/Concertable.Shared.Email/{Application,Infrastructure}`.

**Application** (from `Concertable.Kernel/IEmailService.cs`):
- `IEmailService.cs` → namespace `Concertable.Shared.Email`

**Infrastructure** (from `Concertable.Kernel/Services/Email/`):
- `EmailService.cs` (real MailKit impl) → namespace `Concertable.Shared.Email.Infrastructure`
- Plus `FakeEmailService.cs` if/where it lives — check `Concertable.User.Infrastructure` and `Concertable.Customer.Web` (both have a `FakeEmailService.cs`). Consolidate into Shared.Email.Infrastructure as `FakeEmailService`. Existing duplicates: delete.
- `Extensions/ServiceCollectionExtensions.cs` exposing `AddSharedEmail(IConfiguration)` (chooses real vs fake based on config flag).

Csproj refs: Application → `Concertable.Kernel`; Infrastructure → Application + `MailKit` + `MimeKit` + `Microsoft.Extensions.*`.

### `Concertable.Shared.Geocoding` (Phase D)

`api/Shared/Concertable.Shared.Geocoding/{Application,Infrastructure}`.

**Application**:
- `IGeocodingService.cs` (from `Concertable.Kernel/IGeocodingService.cs`) → namespace `Concertable.Shared.Geocoding`

**Infrastructure** (from `Concertable.Kernel/Services/Geocoding/`):
- `GeocodingService.cs`, `GoogleAddressComponent.cs`, `GoogleGeocodeResponse.cs`, `GoogleGeocodeResult.cs` → namespace `Concertable.Shared.Geocoding.Infrastructure`
- `Extensions/ServiceCollectionExtensions.cs` — `AddSharedGeocoding(IConfiguration)` (configures `HttpClient("Geocoding")` against Google Maps API + registers service).

**Also moves** (related to geocoding — verify usage):
- `Concertable.Kernel/Geometry/IGeometryCalculator.cs`, `IGeometryProvider.cs` — currently in `Concertable.Application.Interfaces.Geometry` namespace. Decision: do these belong with Geocoding? They're separate (geometry calc is local math, geocoding is external HTTP). Recommend a separate `Concertable.Shared.Geometry` or keep in Kernel.
- Geometry impls (`GeographicGeometryProvider`, `GeometryCalculator`, `MetricGeometryProvider`, `GeometryProviderType`) similarly TBD.

→ **Open question for Phase D**: separate `Concertable.Shared.Geometry` lib or keep in Kernel? Default to keep in Kernel unless Geometry usage spans services with no Kernel ref.

### `Concertable.Shared.Imaging` (Phase E)

`api/Shared/Concertable.Shared.Imaging/{Application,Infrastructure}`.

**Application**:
- `IImageService.cs` (from `Concertable.Kernel/IImageService.cs`) → namespace `Concertable.Shared.Imaging`

**Infrastructure** (from `Concertable.Kernel/Services/`):
- `ImageService.cs` → namespace `Concertable.Shared.Imaging.Infrastructure`
- `Extensions/ServiceCollectionExtensions.cs` — `AddSharedImaging()`.

Csproj refs: Infrastructure → Application + `SixLabors.ImageSharp`.

### `Concertable.Shared.Pdf` (Phase F)

`api/Shared/Concertable.Shared.Pdf/{Application,Infrastructure}`.

**Application**:
- `IPdfService.cs` (from `Concertable.Kernel/IPdfService.cs`) → namespace `Concertable.Shared.Pdf`

**Infrastructure**:
- Locate current PDF impl (most likely in `Concertable.Concert.Infrastructure` — QuestPDF ticket renderer). Move to `Concertable.Shared.Pdf.Infrastructure` if it's a generic adapter. If it's ticket-specific (knows about `TicketEntity`), keep it in Concert and have `IPdfService` be a tiny abstraction over QuestPDF basics (`RenderAsync(IDocument) → byte[]`).
- `Extensions/ServiceCollectionExtensions.cs` — `AddSharedPdf()`.

→ **Open question for Phase F**: is `IPdfService` a generic PDF renderer or a ticket-specific renderer? If generic, full extraction. If ticket-specific, the interface itself should move to Concert and Shared.Pdf becomes empty (skip the phase).

### `Concertable.Testing` (Phase G)

Single csproj rename (no clean-arch split — user explicit). Source folder: check which of `api/Tests/Concertable.Tests.Common/` or `api/Tests/Concertable.IntegrationTests.Common/` is the live one (both currently exist — likely one is stale).

```
api/Tests/Concertable.Testing/
  Concertable.Testing.csproj   (renamed from existing)
  …existing files unchanged besides namespace
```

Namespace rewrite: `Concertable.IntegrationTests.Common` (or `Concertable.Tests.Common`) → `Concertable.Testing`.

---

## Namespace mapping (master table)

Sed-rewrite consumer `using` statements + inline type references repo-wide. Apply **longest pattern first** to avoid prefix collisions.

| Old | New |
|---|---|
| `Concertable.Application.Interfaces.Specifications` | `Concertable.DataAccess.Specifications` |
| `Concertable.Application.Interfaces.Blob` | `Concertable.Shared.Blob` |
| `Concertable.Application.Interfaces.Geometry` | `Concertable.Kernel.Geometry` *(or `Concertable.Shared.Geometry` if extracted)* |
| `Concertable.Application.Interfaces` | `Concertable.DataAccess` *(repos/UoW) — but **NOT** for `IImageService`/`IEmailService`/`IGeocodingService`/`IUriService`/`IPdfService`; those go to their respective Shared.X namespaces* |
| `Concertable.Application.Diffing` | `Concertable.DataAccess.Diffing` |
| `Concertable.Application.Serializers` | *(stays in Kernel; namespace unchanged or renamed `Concertable.Kernel.Serializers`)* |
| `Concertable.Application.Mappers` | *(stays in Kernel)* |
| `Concertable.Application.Validators.Parameters` | *(stays in Kernel)* |
| `Concertable.Application.DTOs` | *(stays in Kernel)* |
| `Concertable.Data.Application` | `Concertable.DataAccess` |
| `Concertable.Data.Infrastructure.Data` | `Concertable.DataAccess.Infrastructure` |
| `Concertable.Data.Infrastructure.Extensions` | `Concertable.DataAccess.Infrastructure.Extensions` |
| `Concertable.Data.Infrastructure` | `Concertable.DataAccess.Infrastructure` |
| `Concertable.Shared.Infrastructure.Specifications` | `Concertable.DataAccess.Infrastructure.Specifications` |
| `Concertable.Shared.Infrastructure.Expressions` | `Concertable.DataAccess.Infrastructure.Expressions` |
| `Concertable.Shared.Infrastructure.Repositories` | `Concertable.DataAccess.Infrastructure.Repositories` |
| `Concertable.Shared.Infrastructure.Background` | `Concertable.BackgroundTasks` *(when Plan B lands; until then stays)* |
| `Concertable.Shared.Infrastructure.Data.Seeders` | `Concertable.Shared.Blob.Infrastructure` *(for BlobDevSeeder only)* |
| `Concertable.Shared.Infrastructure.Events` | *(stays in Kernel — `DomainEventDispatcher`)* |
| `Concertable.Shared.Infrastructure.Resources` | `Concertable.Shared.Blob.Infrastructure` |
| `Concertable.Shared.Infrastructure.Services.Blob` | `Concertable.Shared.Blob.Infrastructure` |
| `Concertable.Shared.Infrastructure.Services.Email` | `Concertable.Shared.Email.Infrastructure` |
| `Concertable.Shared.Infrastructure.Services.Geocoding` | `Concertable.Shared.Geocoding.Infrastructure` |
| `Concertable.Shared.Infrastructure.Services.Geometry` | `Concertable.Kernel.Geometry` *(or Shared.Geometry)* |
| `Concertable.Shared.Infrastructure.Services` *(plain — ImageService/UriService)* | Split: ImageService → `Concertable.Shared.Imaging.Infrastructure`; UriService → `Concertable.AspNetCore` *(future)* — keep in Kernel for now |
| `Concertable.Shared.Infrastructure.Settings` | Split: `BlobStorageSettings` → `Concertable.Shared.Blob.Infrastructure`; `UrlSettings` → `Concertable.AspNetCore` *(future)* |
| `Concertable.Shared.Infrastructure.Extensions` | *(stays in Kernel — `AddSharedInfrastructure` god-method, broken up phase-by-phase as adapters extract)* |
| `Concertable.Shared.Infrastructure` *(plain — `PaginationExtensions`)* | `Concertable.DataAccess.Infrastructure` |
| `Concertable.IntegrationTests.Common` *(or `Concertable.Tests.Common`)* | `Concertable.Testing` |

### Hard rules for bulk sed

1. **Always anchor with terminating `;` or `.<next-segment>`** to avoid partial matches.
2. **Run sed on consumer files only**, never blanket the whole repo — Kernel-resident files keep their existing namespaces until their phase moves them.
3. **After each phase**, grep for the old namespace inside Kernel to catch any files accidentally rewritten. Pattern: `grep -n "^namespace Concertable\.<NewNamespace>" api/Shared/Concertable.Kernel/`.
4. **Inside files being moved**, rewrite their declared `namespace` first; rewrite their `using` statements second; rewrite consumers' `using` last.

---

## Phase A — relocate `IModuleSeeder` to Seeding (already in flight)

Already partly done in the in-flight tree. To finish:

1. `git mv api/Concertable.DataAccess/Concertable.DataAccess.Application/IModuleSeeder.cs api/Seeding/Concertable.Seeding/IModuleSeeder.cs` ✅ done
2. Inside that file: `namespace Concertable.DataAccess;` → `namespace Concertable.Seeding;` ✅ done
3. Consumer rewrite: every file with `using Concertable.DataAccess;` whose ONLY DataAccess use is `IDevSeeder`/`ITestSeeder`/`IModuleSeeder` should swap to `using Concertable.Seeding;`. Files with both (e.g. a seeder using `IRepository` + `IDevSeeder`) keep both usings.
4. Affected files: every Module.Infrastructure `XDevSeeder.cs` / `XTestSeeder.cs`. Most ALREADY have `using Concertable.Seeding;` for `SeedData`/`Factories` — they're fine if `Concertable.DataAccess` is also present (until step 3 prunes).

Quickest path: just add `using Concertable.Seeding;` to every `XDevSeeder.cs`/`XTestSeeder.cs` that has `using Concertable.DataAccess;`. Don't bother removing DataAccess from those files in this phase.

---

## In-flight working-tree state (as of pause)

> **2026-05-20 update:** Phases A + C landed as commit `952b75fb` on `Refactor/Microservices` ("Refactor: extract DataAccess + Seeding + Shared.Blob from Kernel") — a bundled 181-file diff covering the v1 DataAccess scaffold + moves, Phase A `IModuleSeeder` relocation, and Phase C `Concertable.Shared.Blob` extraction. Build green on `Concertable.sln`. Working tree clean (except unrelated `.claude/worktrees/` submodule from the parallel ASB agent).
>
> The pre-commit notes below are kept as historical context for the in-flight state that preceded the commit.

762 modified/moved files. Current branch: `Refactor/Microservices`, last commit `93c08e4b`. Build is broken.

Done correctly:
- Scaffolded `Concertable.DataAccess.{Application,Infrastructure}` csprojs.
- Moved interface/impl files via `git mv` (history preserved).
- Removed old `Concertable.Data.{Application,Infrastructure}` from `Concertable.sln`; added new DataAccess csprojs.
- Bulk-sed rewrote consumer usings for the DataAccess-bound namespaces.
- Csproj `<ProjectReference>` paths rewritten via Python script (perl/sed both failed on Windows backslashes).
- Re-scaffolded ALL 13 module + AppDb + Auth + Customer migrations via `./initial-migrations.ps1` (visible in `git status` as modified Migrations/ folders).
- Moved `Diffing/*` from Kernel → `DataAccess.Application/Diffing/` with namespace `Concertable.DataAccess.Diffing`.
- `IModuleSeeder` moved to Seeding (Phase A step 1+2).
- `BlobDevSeeder` moved to `Concertable.Web/BlobDevSeeder.cs` — **incorrect, needs revert**. Phase C puts it in `Concertable.Shared.Blob.Infrastructure`.
- `UserHierarchyConfigurations.cs` (`UserEntityConfiguration` class) moved out of `Data.Infrastructure/Data/Configurations/` → `Concertable.User.Infrastructure/Data/Configurations/UserEntityConfiguration.cs` (module-specific config in module home — correct).

Done incorrectly — needs revert in next session:
- **5 Kernel files** had their `namespace Concertable.Application.Interfaces` accidentally rewritten to `namespace Concertable.DataAccess` by the bulk sed. Files:
  - `api/Shared/Concertable.Kernel/IImageService.cs`
  - `api/Shared/Concertable.Kernel/IEmailService.cs`
  - `api/Shared/Concertable.Kernel/IGeocodingService.cs`
  - `api/Shared/Concertable.Kernel/IUriService.cs`
  - `api/Shared/Concertable.Kernel/IPdfService.cs`
  
  These are cross-assembly namespace pollution. Revert their namespace declarations to their pre-sed state (`Concertable.Application.Interfaces`). They'll get their proper final namespace when their respective Phase B/C/D/E/F moves them out.

- `BlobDevSeeder.cs` now at `api/Concertable.Web/BlobDevSeeder.cs` — move it to `api/Concertable.Web/Seeders/BlobDevSeeder.cs` *as a stopgap*, OR (cleaner) just leave the broken state and let Phase C move it directly to `Concertable.Shared.Blob.Infrastructure`. Recommend: **leave it where it is until Phase C, don't double-move**.

### Decision for next session: revert vs continue

Two options:

**Option 1 — keep in-flight changes, continue forward.** Phases A→C→B→D→E→F→G→H→I from current state. Fastest path. Risk: 762-file working tree is harder to review; some accidental rewrites may slip through.

**Option 2 — revert to `93c08e4b`, start clean.** `git reset --hard 93c08e4b`. Then execute the full v2 plan from scratch. Cleaner commits per phase. More work but easier review.

**Recommendation**: Option 1 unless review hygiene matters more than time. The in-flight tree is mostly correct; Phase A→C cleanups + the 5-file namespace revert sort the issues.

---

## Open questions to resolve at resume

1. **Geometry types** (Phase D): separate `Concertable.Shared.Geometry` lib, or keep in Kernel? Geometry is local math (point distance, projections) — not really an "adapter" since there's no external service. Probably stays in Kernel.
2. **PDF service** (Phase F): is `IPdfService` generic-renderer or ticket-renderer? If ticket-specific, no extraction needed — interface moves to Concert.
3. **FakeEmailService duplicates** (Phase B): `Concertable.User.Infrastructure/Services/Email/FakeEmailService.cs` AND `Concertable.Customer.Web/Services/FakeEmailService.cs` exist. Consolidate into `Concertable.Shared.Email.Infrastructure` and delete duplicates.
4. **`Concertable.Application.{DTOs,Mappers,Serializers,Validators}`**: Kernel has these stale `Concertable.Application.*` namespaces still. Phase H decides homes (probably stays Kernel for now with renamed namespace `Concertable.Kernel.X`).
5. **`AddSharedInfrastructure` god-method**: currently registers 6 services across what'll become 6 separate libs. Decompose into per-lib `AddSharedX()` calls in composition roots, OR keep a thin meta-extension. Recommend per-lib calls at each composition root — explicit beats magic.

---

## Resume checklist

1. ~~Read this doc end to end.~~ ✅
2. ~~Pick Option 1 (continue) or Option 2 (reset).~~ ✅ Option 1 picked.
3. ~~Execute Phase A finish (Consumer rewrite for `IDevSeeder`/`ITestSeeder`).~~ ✅ `952b75fb`
4. ~~Execute Phase C (`Shared.Blob`) — bring BlobDevSeeder home.~~ ✅ `952b75fb`
5. ~~Build green checkpoint.~~ ✅ 0 errors on Concertable.sln
6. ~~Phase B (`Shared.Email`).~~ ✅ landed. Real MailKit + generic fake both `internal sealed` in `Shared.Email.Infrastructure`; User module keeps `AutoVerifyingFakeEmailService` as the auto-verify override.
7. Phase D (`Shared.Geocoding`) — next. Resolve Geometry open question first.
8. Phase E (`Shared.Imaging`) — also removes the temporary Kernel → Shared.Blob.Application ref.
9. Phase F (`Shared.Pdf`) — resolve generic-vs-ticket open question first.
10. Phase G (`Concertable.Testing` rename) — confirm which Tests.Common is live.
11. Phase H — Kernel namespace audit. Grep for cross-assembly namespace leaks. The 5 originally-rewritten files (`IImageService.cs`, `IEmailService.cs`, `IGeocodingService.cs`, `IUriService.cs`, `IPdfService.cs`) get fixed implicitly via Phases B/D/E/F (they move to the right home with the right namespace) — Phase H is just an audit pass.
12. Phase I — delete `api/Data/`, re-scaffold migrations (`./initial-migrations.ps1`), commit.

Each phase: one commit. Commit message format `Refactor: extract Concertable.Shared.<Lib>` or `Refactor: relocate <X> to <Y>`. No co-authored-by trailers, no Claude generated trailers.
