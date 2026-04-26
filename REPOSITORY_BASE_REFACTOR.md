# Repository Generic Cleanup — Per-Module Base Classes

**Status:** Deferred. Tracked here so it doesn't get lost; not part of the legacy-trio retirement.

## Problem

Concrete module repos currently spell the context generic argument every time:

```csharp
internal class PreferenceRepository : Repository<PreferenceEntity, CustomerDbContext>, IPreferenceRepository { ... }
internal class MessageRepository  : Repository<MessageEntity,    MessagingDbContext>, IMessageRepository  { ... }
internal class ConcertRepository  : Repository<ConcertEntity,    ConcertDbContext>,   IConcertRepository  { ... }
```

The `, XDbContext` repeats on every repo inside a module even though the context is fixed for that module. Visually noisy and reads like a leak of DI plumbing into the type signature.

## Why we can't fix it at the DI / `Program.cs` level

- `TContext` is a **compile-time generic**, not a runtime resolution. DI can pick which `DbContext` to inject into a closed generic, but it can't bind the open generic argument.
- C# has **no default generic type parameters**. `Repository<TEntity, TContext = CustomerDbContext>` is not legal.
- Source generators / Roslyn analyzers could synthesise the bases — overkill for what is fundamentally a one-liner per module.

## Proposed shape

Each module declares **one abstract base per repository flavour** that pre-binds the context. The bases live in `Module.Infrastructure/Repositories/` next to the concrete repos. Concrete repos then extend the module-local base without restating the context.

```csharp
// api/Modules/Customer/Concertable.Customer.Infrastructure/Repositories/CustomerModuleRepository.cs

internal abstract class CustomerModuleBaseRepository<TEntity>(CustomerDbContext context)
    : BaseRepository<TEntity, CustomerDbContext>(context)
    where TEntity : class { }

internal abstract class CustomerModuleRepository<TEntity>(CustomerDbContext context)
    : Repository<TEntity, CustomerDbContext>(context)
    where TEntity : class, IIdEntity { }

internal abstract class CustomerModuleGuidRepository<TEntity>(CustomerDbContext context)
    : GuidRepository<TEntity, CustomerDbContext>(context)
    where TEntity : class, IGuidEntity { }
```

Concrete repos collapse to:

```csharp
internal class PreferenceRepository(CustomerDbContext context)
    : CustomerModuleRepository<PreferenceEntity>(context), IPreferenceRepository { ... }
```

## Naming options

- `CustomerModuleRepository<T>` — explicit, mirrors `IXModule` facade naming.
- `CustomerRepository<T>` — shorter, but collides with concrete repo names in other modules (`ContractRepository`, `ConcertRepository`).
- `ModuleRepository<T>` (per-module file) — concise but loses module identity in stack traces.

Lean toward `XModuleRepository<T>` for symmetry with the existing `IXModule` facade convention. Open question — pick when the work happens.

## Scope

7 modules × 3 base-class flavours = ~21 small files. Each base is empty boilerplate (constructor + `where` constraint). Touch every concrete repo once to switch its base class.

Modules in scope: Identity, Artist, Venue, Concert, Contract, Payment, Messaging, Customer.

## Tradeoffs

**For:**
- Removes the `, XDbContext` repetition from every concrete repo signature.
- Reads as "this is a Customer repo" rather than "this is a Repository parametrised on CustomerDbContext".
- Standard enterprise pattern for this exact problem.

**Against:**
- 21 boilerplate files for a cosmetic win.
- Adds one indirection layer when grepping for repo behaviour (concrete → module base → generic base).
- At the current scale (Customer has 1 repo, Messaging has 1 repo) the saving per module is small. Pays off when a module has 3+ repos (Concert, Identity).

## When to do it

- **Do not** bundle into legacy-trio retirement — it adds churn and conflicts with the migration re-scaffold. Trio retirement should land first so the diff stays focused.
- **Do** schedule as its own focused PR after the trio is gone and the codebase has stabilised. Touches every module's `Repositories/` folder; trivial to review when isolated.
- Reasonable trigger: next time a module gains a 3rd repository, do the refactor for that module first as a proof of value, then propagate.

## Out of scope for this doc

- `DapperRepository` lives in `Shared.Infrastructure` and isn't part of the per-module base hierarchy — leave alone.
- `GenreRepository` is `SharedDbContext`-bound and lives in `Data.Infrastructure` — not a module repo.
- The legacy `IRepository<T>` vs `IIdRepository<T>` duplication in `Shared.Domain` is a separate cleanup; pick one and delete the other when this work happens.
