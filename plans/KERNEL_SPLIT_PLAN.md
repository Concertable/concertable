# Kernel Split Plan

`Concertable.Kernel` is currently a Frankenstein csproj — domain primitives + app-layer interfaces + infra impls + adapters + utility code all jammed together. File namespaces still say `Concertable.Application.*` and `Concertable.Shared.Infrastructure.*` from the pre-collapse layout (Phase 1 Step 2, commit `7491498a`). Path: rename without restructuring = lipstick. Real fix: split into focused libs aligned with the microservices direction, each consumed only by services that need it.

This doc plans that split. Executes incrementally — one lib per commit/PR.

---

## Target end state

| Lib | Holds | Notes |
|---|---|---|
| `Concertable.Kernel` (slim) | Domain primitives only: `IEntity`, `IGuidEntity`, `IIdEntity`, `IDomainEvent`, `IDomainEventDispatcher`, `IDomainEventHandler`, `IEventRaiser`, `EventRaiser`, `IAuditable`, `Address`, `DateRange`, `DomainException`, `ErrorExtensions`, `IAddress`/`IHasDateRange`/`IHasLocation`/`IHasName`/`IHasRating`/`IRating`, `Exceptions/*` (HttpException family), `Enums/ErrorType` | The true foundation. Every other lib refs this. |
| `Concertable.Messaging` ✅ | `IBus`, `IBusTransport`, `IIntegrationEvent`/`Command`, handlers, `MessageEnvelope`, in-memory transport, outbox (Step 9) | Done. Lives at `api/Concertable.Messaging/`. |
| `Concertable.Messaging.AzureServiceBus` | ASB transport adapter (`IBusTransport` impl) | Parallel-agent work — see [AZURE_SERVICE_BUS_PLAN.md](AZURE_SERVICE_BUS_PLAN.md). |
| **`Concertable.DataAccess` (next)** | Repository/UoW abstractions + EF base impls + interceptors + ReadDbContext + DbContextBase + specs + expression helpers | Absorbs `Concertable.Data.Application` + `Concertable.Data.Infrastructure` + relevant Kernel interfaces. See [Plan A](#plan-a--concertabledataaccess) below. |
| `Concertable.BackgroundTasks` | `IBackgroundTaskQueue`/impl, `IBackgroundTaskRunner`/impl, `QueueHostedService` | See [Plan B](#plan-b--concertablebackgroundtasks). |
| `Concertable.Adapters.Email` / `.Blob` / `.Geocoding` / `.Image` / `.Pdf` | External-service adapter impls + interfaces | Per-adapter csprojs so a service only pulls what it needs. Could collapse to one `Concertable.Adapters` if 5 csprojs feels heavy. |
| `Concertable.AspNetCore` | Middleware, problem details, GlobalExceptionHandler, auth handlers | Name picked to avoid collision with `Concertable.Web` (the host). |
| `Concertable.Testing` | Test fixtures, Testcontainers helpers, xUnit collections | Rename + expand `Concertable.IntegrationTests.Common`. |
| `Concertable.Observability` | OTel setup, structured logging | Empty for now — materializes at Phase 4 Step 17. |

Order of extraction: DataAccess → BackgroundTasks → Adapters → AspNetCore → Testing → Observability → Kernel slimming. DataAccess first because it's the highest-traffic abstraction (every module's repos hang off it).

---

## Plan A — `Concertable.DataAccess`

### Layout (per MODULAR_MONOLITH_RULES.md)

```
api/Concertable.DataAccess/
  Concertable.DataAccess.Application/      ← interfaces (no EF dep)
  Concertable.DataAccess.Infrastructure/   ← impls (EF dep)
```

Two csprojs, mirrors existing `Concertable.Data.Application` / `Concertable.Data.Infrastructure` split. Lets pure-domain csprojs reference `.Application` without pulling EF transitively.

### File moves

**Into `Concertable.DataAccess.Application/` (namespace `Concertable.DataAccess`):**

From `api/Shared/Concertable.Kernel/`:
- `IBaseRepository.cs`
- `IRepository.cs`
- `IGuidRepository.cs`
- `IIdRepository.cs`
- `IDapperRepository.cs`
- `IUnitOfWork.cs`
- `IUnitOfWorkBehavior.cs`
- `IDbInitializer.cs`
- `IModuleSeeder.cs`
- `Specifications/IDateRangeSpecification.cs`
- `Specifications/IUpcomingSpecification.cs`

From `api/Data/Concertable.Data.Application/`:
- `IReadDbContext.cs`

**Into `Concertable.DataAccess.Infrastructure/` (namespace `Concertable.DataAccess.Infrastructure`):**

From `api/Shared/Concertable.Kernel/`:
- `Expressions/ExpressionExtensions.cs`
- `Expressions/ParameterReplacer.cs`
- `Extensions/DbUpdateExceptionExtensions.cs`
- `Repositories/DapperRepository.cs`
- `Specifications/DateRangeSpecification.cs`
- `Specifications/UpcomingSpecification.cs`
- `PaginationExtensions.cs`

From `api/Data/Concertable.Data.Infrastructure/`:
- `BaseRepository.cs`
- `DbContextBase.cs`
- `Schema.cs`
- `UnitOfWork.cs`
- `UnitOfWorkBehavior.cs`
- `Data/AuditInterceptor.cs`
- `Data/DomainEventDispatchInterceptor.cs`
- `Data/IEntityTypeConfigurationProvider.cs`
- `Data/IRatingProjectionConfigurationProvider.cs`
- `Data/ReadDbContext.cs`
- `Data/Configurations/UserHierarchyConfigurations.cs` ← *check — may not belong in shared lib, possibly module-specific*
- `Extensions/ServiceCollectionExtensions.cs` (carries `AddReadDbContext()` etc.)

### Namespace mapping (consumer-side rewrites)

Apply in order (longest patterns first to avoid prefix collisions):

```
Concertable.Application.Interfaces.Specifications  →  Concertable.DataAccess.Specifications
Concertable.Application.Interfaces                 →  Concertable.DataAccess
Concertable.Shared.Infrastructure.Specifications   →  Concertable.DataAccess.Infrastructure.Specifications
Concertable.Shared.Infrastructure.Expressions      →  Concertable.DataAccess.Infrastructure.Expressions
Concertable.Shared.Infrastructure.Repositories     →  Concertable.DataAccess.Infrastructure.Repositories
Concertable.Shared.Infrastructure.Extensions       →  Concertable.DataAccess.Infrastructure.Extensions (only DbUpdateExceptionExtensions; the AddSharedInfrastructure ext stays in Kernel for now)
Concertable.Data.Application                       →  Concertable.DataAccess
Concertable.Data.Infrastructure.Data               →  Concertable.DataAccess.Infrastructure
Concertable.Data.Infrastructure.Extensions         →  Concertable.DataAccess.Infrastructure.Extensions
Concertable.Data.Infrastructure                    →  Concertable.DataAccess.Infrastructure
```

**Inside Kernel files NOT being moved**, keep their existing stale namespaces for now (e.g. `Concertable.Shared.Infrastructure.Services.*`). Final Kernel slimming pass cleans those up.

### Csproj reference graph after extraction

```
Concertable.DataAccess.Application
  └─ Concertable.Kernel (for IEntity, IDomainEvent, IAuditable markers used in IRepository signatures)

Concertable.DataAccess.Infrastructure
  └─ Concertable.DataAccess.Application
  └─ Concertable.Kernel
  └─ Microsoft.EntityFrameworkCore
  └─ Microsoft.EntityFrameworkCore.SqlServer
  └─ Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite (if DbContextBase uses Point)
  └─ Dapper (for DapperRepository)
```

### Consumers needing ref updates

Every csproj currently referencing `Concertable.Data.Application` OR `Concertable.Data.Infrastructure` swaps to the new lib. Bulk grep:

```
grep -rl "Concertable.Data.Application\|Concertable.Data.Infrastructure" --include="*.csproj" api/
```

Every csproj with `using Concertable.Application.Interfaces` / `using Concertable.Shared.Infrastructure.*` (the moved types only) adds the right ref.

### Execution order

1. Create `api/Concertable.DataAccess/Concertable.DataAccess.Application/` + `Concertable.DataAccess.Infrastructure/` csprojs.
2. `git mv` each file from old location to new (preserves history).
3. Sed-rewrite namespaces inside moved files: `namespace Concertable.Application.* → namespace Concertable.DataAccess.*` etc.
4. Sed-rewrite `using` statements across ALL consumer .cs files in the repo.
5. Update consumer csprojs: drop `Concertable.Data.Application` / `Concertable.Data.Infrastructure` refs; add `Concertable.DataAccess.Application` (and `.Infrastructure` where impls are touched).
6. Add 2 new csprojs to `Concertable.sln`.
7. Build green. Re-scaffold migrations (`./initial-migrations.ps1`) since DbContextBase namespace changed.
8. Delete now-empty `api/Data/` folder.
9. Commit.

---

## Plan B — `Concertable.BackgroundTasks`

### Layout

Single flat csproj (3 files — too small to warrant Application/Infrastructure split).

```
api/Concertable.BackgroundTasks/
  Concertable.BackgroundTasks.csproj
  IBackgroundTaskQueue.cs        (currently has 1 file holding interface + impl mixed — split if needed)
  BackgroundTaskQueue.cs
  IBackgroundTaskRunner.cs
  BackgroundTaskRunner.cs
  QueueHostedService.cs
  Extensions/ServiceCollectionExtensions.cs   ← `AddBackgroundTasks()` extension (renames + relocates current AddQueueHostedService)
```

Namespace: `Concertable.BackgroundTasks`.

### File moves

From `api/Shared/Concertable.Kernel/Background/`:
- `BackgroundTaskQueue.cs`
- `BackgroundTaskRunner.cs`
- `QueueHostedService.cs`

Currently `IBackgroundTaskQueue` + `IBackgroundTaskRunner` interfaces likely declared inside the impl files or in `Concertable.Application.Interfaces` namespace at Kernel root. Verify and consolidate during move.

### Csproj refs

```
Concertable.BackgroundTasks
  └─ Microsoft.Extensions.Hosting.Abstractions (for IHostedService)
  └─ Microsoft.Extensions.DependencyInjection.Abstractions
```

No Kernel dep needed (pure utility).

### Namespace rewrites

```
Concertable.Shared.Infrastructure.Background  →  Concertable.BackgroundTasks
```

Plus update `AddQueueHostedService` → `AddBackgroundTasks` at the 1-2 call sites in `Concertable.Web/Program.cs`.

### Execution order

Same shape as Plan A but smaller. One commit.

---

## Future libs (sketches only)

### `Concertable.Adapters.{Email,Blob,Geocoding,Image,Pdf}`

Each pulls one external service. Per-adapter csproj keeps consumer surface tight (Customer service that doesn't need PDF doesn't transitively pull QuestPDF).

Open Q: collapse into one `Concertable.Adapters` if 5 csprojs feels heavy. Decide when extracting.

### `Concertable.AspNetCore`

Holds GlobalExceptionHandler + problem details + (future) auth handler primitives. Currently these live in `Concertable.Web/`. Name avoids collision with the existing `Concertable.Web` host csproj.

### `Concertable.Testing`

Rename `Concertable.IntegrationTests.Common` → `Concertable.Testing` + scope expansion: Testcontainers helpers, xUnit collection fixtures, mocking primitives.

### `Concertable.Observability`

Empty until Phase 4 Step 17 lands OTel. Sketched here so the future lib has a home.

### Final Kernel slim

After everything above moves out, Kernel keeps ONLY the genuine foundation:
- Entity/event markers: `IEntity`, `IGuidEntity`, `IIdEntity`, `IDomainEvent`, `IDomainEventDispatcher`, `IDomainEventHandler`, `IEventRaiser`, `EventRaiser`, `IAuditable`
- Value types: `Address`, `DateRange`, `IAddress`, `IHasDateRange`, `IHasLocation`, `IHasName`, `IHasRating`, `IRating`
- Errors: `DomainException`, `ErrorExtensions`, `Exceptions/*` (HttpException family), `Enums/ErrorType`

~20 files. Namespace flattens to `Concertable.Kernel` (no more `Concertable.Shared.*`). Done.

---

## Resume-after-compact checklist

When picking this up post-compact:

1. Read this doc.
2. Confirm `Concertable.Messaging` extraction committed (head `517201db`, branch `Refactor/Microservices`).
3. Start Plan A — DataAccess extraction.
4. After DataAccess lands: Plan B — BackgroundTasks.
5. Then Step 9 (outbox) — fills `Concertable.Messaging.Domain` + `.Application`. Independent of further Kernel cleanup.
