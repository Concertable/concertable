# Repository interface redesign — read/write split + generic key

## Goal

Reshape the shared repository abstractions into a clean lattice with a **generic key**, so:

- `IBaseRepository` stays the **keyless write base** (composite-key home), and the keyed
  `IRepository` **extends it** — the inheritance the project wanted.
- A read-only contract (`IReadRepository`) exists for the Customer read-models.
- A single generic `TKey` absorbs the int/Guid duplication, so **`IGuidRepository` /
  `GuidRepository` disappear** (no parallel Guid hierarchy).
- The three hand-rolled Customer read repositories (`VenueReadRepository`,
  `ConcertReadRepository`, `ArtistReadRepository`) sit on a **shared `ReadRepository<T>` base**
  instead of re-declaring ctor + context + `GetByIdAsync` each.

This is a pure refactor — no behavioural change. No migrations (model unchanged).

## The lattice

Three orthogonal capabilities — keyless writes, keyed reads, and their union:

```
IBaseRepository<T>            GetAll + Add/AddRange/Update/Remove/Save   (keyless — composite-key OK)
IReadRepository<T,TKey>       GetById + GetAll + Exists                  (keyed, read-only)
IRepository<T,TKey> : IBaseRepository<T>, IReadRepository<T,TKey>        (keyed, read+write)
```

- **`IRepository<T,TKey> : IBaseRepository<T>`** — IRepository extends IBaseRepository, as wanted.
- **Composite-key entity** (no single `Id`, e.g. a join table you mutate as a set) →
  `IBaseRepository<T>`: `class` constraint only, no `IEntity<TKey>`, no `GetById`.
  *(Note: `ArtistGenreEntity` and similar are navigations today — loaded via `Include`, never
  through a repository — so nothing needs this yet. `IBaseRepository` is kept as the correct,
  zero-cost home for the day one does.)*
- `GetAllAsync` is declared in both `IBaseRepository` and `IReadRepository<T,TKey>` — deliberate
  (each usable standalone; identical signature merges cleanly in `IRepository`).

## Open decisions (confirm before implementing)

1. **Int default alias, no Guid alias.** Common case stays `IRepository<VenueEntity>`; the 2 Guid
   repos spell out `IRepository<UserEntity, Guid>`. Same for reads
   (`IReadRepository<VenueEntity>`). **Recommendation: int alias only.**
2. **Keep `IBaseRepository`** as the keyless write base (revised — was "delete"; composite-key
   support + the `IRepository : IBaseRepository` structure both depend on it). **Recommendation:
   keep.**
3. **Read repos gain `GetAllAsync` + `Exists`** by inheriting the shared read interface, even
   though Customer only calls `GetByIdAsync` today. Harmless surface. **Recommendation: accept.**

## Target design

### Kernel — `api/Shared/Concertable.Kernel/`

`IEntity` stays the keyless marker (composite-key rows like `InboxMessageEntity` keep using it
or nothing). Add a keyed sub-interface; the int/Guid markers pin the key:

```csharp
// IEntity.cs (unchanged)
public interface IEntity;

// IEntity{TKey}.cs (new)  -- no variance: covariance does nothing for value-type keys
public interface IEntity<TKey> : IEntity { TKey Id { get; } }

// IIdEntity.cs
public interface IIdEntity : IEntity<int>;

// IGuidEntity.cs
public interface IGuidEntity : IEntity<Guid>;
```

`IIdEntity`/`IGuidEntity` stop declaring `Id` themselves (inherited). Existing
`where T : IIdEntity` / `where T : IEntity` constraints are unaffected (`IEntity<TKey> : IEntity`).

### DataAccess.Application — interfaces

```csharp
// IBaseRepository.cs  -- keyless write base (composite-key home)
public interface IBaseRepository<TEntity> where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<TEntity> AddAsync(TEntity entity);
    Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task SaveChangesAsync();
}
```

```csharp
// IReadRepository.cs  -- keyed read-only
public interface IReadRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    bool Exists(TKey id);
}

public interface IReadRepository<TEntity> : IReadRepository<TEntity, int>
    where TEntity : class, IIdEntity;
```

```csharp
// IRepository.cs  -- keyed read+write
public interface IRepository<TEntity, TKey> : IBaseRepository<TEntity>, IReadRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>;

public interface IRepository<TEntity> : IRepository<TEntity, int>
    where TEntity : class, IIdEntity;
```

**Delete:** `IIdRepository.cs`, `IGuidRepository.cs`. (`IBaseRepository.cs` **kept**;
`IRepository.cs` rewritten — the old dead 1-arg `IRepository` becomes the new int alias.)

### DataAccess.Infrastructure — base classes

Replace `BaseRepository.cs` (rename file → `Repository.cs`). `GuidRepository<T,TContext>` is
gone; the generic key absorbs it. A new read-only `ReadRepository<T,TContext,TKey>` serves the
read-models. Methods are `virtual` so read repos needing eager-loads can override `GetByIdAsync`.

```csharp
// keyless write base — unchanged from today except it's now the shared write impl
public abstract class BaseRepository<TEntity, TContext>(TContext context)
    : IBaseRepository<TEntity>
    where TEntity : class
    where TContext : DbContextBase
{
    protected readonly TContext context = context;

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync() =>
        await context.Set<TEntity>().ToListAsync();

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        await context.Set<TEntity>().AddAsync(entity);
        return entity;
    }

    public async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await context.Set<TEntity>().AddRangeAsync(entities);
        return entities;
    }

    public void Update(TEntity entity) => context.Set<TEntity>().Update(entity);
    public void Remove(TEntity entity) => context.Set<TEntity>().Remove(entity);
    public Task SaveChangesAsync() => context.SaveChangesAsync();
}

// keyed read-only — for read-models
public abstract class ReadRepository<TEntity, TContext, TKey>(TContext context)
    : IReadRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TContext : DbContextBase
{
    protected readonly TContext context = context;

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync() =>
        await context.Set<TEntity>().ToListAsync();

    public virtual Task<TEntity?> GetByIdAsync(TKey id) =>
        context.Set<TEntity>().FindAsync(id).AsTask();

    public virtual bool Exists(TKey id) =>
        context.Set<TEntity>().Any(e => e.Id!.Equals(id));
}

// keyed read+write — the everyday base
public abstract class Repository<TEntity, TContext, TKey>(TContext context)
    : BaseRepository<TEntity, TContext>(context), IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TContext : DbContextBase
{
    public virtual Task<TEntity?> GetByIdAsync(TKey id) =>
        context.Set<TEntity>().FindAsync(id).AsTask();

    public virtual bool Exists(TKey id) =>
        context.Set<TEntity>().Any(e => e.Id!.Equals(id));
}
```

> **Small, intentional impl duplication.** C# single-inheritance means `Repository` (extends the
> write base) re-implements the two trivial keyed-read one-liners that `ReadRepository` also has;
> and `GetAll` exists in both write/read bases. The alternative (sharing keyed-read impl) would
> instead duplicate the five write methods. Duplicating two one-liners is the smaller cost.

> **`Exists` translation gotcha.** `e.Id == id` won't compile on an unconstrained `TKey`.
> `e.Id!.Equals(id)` is preferred; if EF Core fails to translate it, fall back to
> `context.Set<TEntity>().Find(id) is not null` (key lookup, no predicate — always works, but
> loads + tracks the row). Decide during implementation via the integration suite.

### Per-context base aliases — each module's `Repositories/Repository.cs`

Today each write module declares three abstract aliases pinning `TContext`
(`BaseRepository<T>`, `Repository<T>`, `GuidRepository<T>`). Replace the Guid alias with a read
alias; keep the keyless `BaseRepository<T>` (composite-key home) and the int `Repository<T>`:

```csharp
internal abstract class BaseRepository<TEntity>(XDbContext context)
    : BaseRepository<TEntity, XDbContext>(context)
    where TEntity : class;

internal abstract class ReadRepository<TEntity>(XDbContext context)
    : ReadRepository<TEntity, XDbContext, int>(context)
    where TEntity : class, IIdEntity;

internal abstract class Repository<TEntity>(XDbContext context)
    : Repository<TEntity, XDbContext, int>(context)
    where TEntity : class, IIdEntity;
```

Guid concrete repos extend the generic 3-arg base directly (only the context is spelled out):

```csharp
internal class TicketRepository : Repository<TicketEntity, TicketDbContext, Guid>, ITicketRepository
```

> Base classes keep primary constructors (matches today's `BaseRepository.cs`); concrete repos
> keep explicit ctor + `private readonly` field per `CODE_CONVENTIONS.md`.

### Customer read-model modules — shared read base

`Customer/Modules/{Venue,Concert,Artist}` currently have **no** `Repository.cs`. Add one with
just the read alias:

```csharp
// e.g. Customer.Venue.Infrastructure/Repositories/Repository.cs
internal abstract class ReadRepository<TEntity>(VenueDbContext context)
    : ReadRepository<TEntity, VenueDbContext, int>(context)
    where TEntity : class, IIdEntity;
```

Refactor the three read repos + their interfaces:

```csharp
// IVenueReadRepository.cs
internal interface IVenueReadRepository : IReadRepository<VenueEntity>;

// VenueReadRepository.cs  (plain by-id -> inherit base FindAsync, no override)
internal class VenueReadRepository : ReadRepository<VenueEntity>, IVenueReadRepository
{
    public VenueReadRepository(VenueDbContext context) : base(context) { }
}
```

`Concert` and `Artist` eager-load `Genres`, so they **override** `GetByIdAsync`:

```csharp
internal class ConcertReadRepository : ReadRepository<ConcertEntity>, IConcertReadRepository
{
    public ConcertReadRepository(ConcertDbContext context) : base(context) { }

    public override Task<ConcertEntity?> GetByIdAsync(int id) =>
        context.Concerts.Include(c => c.Genres).FirstOrDefaultAsync(c => c.Id == id);
}
```

**Drop** `IConcertReadRepository.SaveChangesAsync` and its impl — dead (verified: no callers;
the projection handler writes via `DbContext` directly).

## File-by-file change list (live `api/` tree only — ignore `.claude/worktrees/`)

### Shared abstractions
| File | Action |
|---|---|
| `Shared/Concertable.Kernel/IEntity.cs` | unchanged |
| `Shared/Concertable.Kernel/IEntity{TKey}.cs` | **new** |
| `Shared/Concertable.Kernel/IIdEntity.cs` | `: IEntity<int>`, drop `Id` |
| `Shared/Concertable.Kernel/IGuidEntity.cs` | `: IEntity<Guid>`, drop `Id` |
| `DataAccess/...Application/IBaseRepository.cs` | **keep**, unchanged (keyless write base) |
| `DataAccess/...Application/IReadRepository.cs` | **new** (2-arg + int alias) |
| `DataAccess/...Application/IRepository.cs` | rewrite (`: IBaseRepository, IReadRepository` + int alias) |
| `DataAccess/...Application/IIdRepository.cs` | **delete** |
| `DataAccess/...Application/IGuidRepository.cs` | **delete** |
| `DataAccess/...Infrastructure/BaseRepository.cs` | rewrite → `Repository.cs` (`BaseRepository` + `ReadRepository` + `Repository`, drop `GuidRepository`) |

### Interfaces extending the old hierarchy (rename base)
Grep `: IIdRepository<` → `: IRepository<` ; `: IGuidRepository<` → `: IRepository<T, Guid>`.
Known live consumers:
- `IIdRepository<T>` → `IRepository<T>`: `IVenueRepository`, `IContractRepository`,
  `IConcertRepository`, `IBookingRepository`, `IApplicationRepository`, `IOpportunityRepository`,
  `ILifecycleRepository<T>` (B2B); `IArtistRepository` (B2B); `IPreferenceRepository` (Customer);
  `IEscrowRepository` (Payment).
- `IGuidRepository<T>` → `IRepository<T, Guid>`: `IUserRepository` (B2B), `ITicketRepository`
  (Customer). **Re-grep to catch any manager repos on the live branch.**

### Per-context `Repository.cs` (write modules — swap Guid alias → read alias)
B2B: `Venue`, `Contract`, `Concert`, `Artist`, `User`. Customer: `Preference`, `Ticket`.
Payment: root. (Guid-only modules — Ticket, User — drop the int aliases and let the concrete Guid
repo extend the 3-arg base.)

### Customer read modules (add read base + refactor)
`Venue`, `Concert`, `Artist`: new `Repositories/Repository.cs`; refactor `*ReadRepository.cs`
+ `I*ReadRepository.cs`. Concert/Artist override `GetByIdAsync` (Include Genres). Drop
`IConcertReadRepository.SaveChangesAsync`.

## Notes / non-issues
- **DI unchanged** — registrations bind named interfaces (`IVenueReadRepository`, etc.); no open
  generic registration to touch.
- **Composite-key entities** (`InboxMessageEntity`, `ArtistGenreEntity`) stay on `IEntity`/nothing
  and are navigations or infra tables today; `IBaseRepository<T>` is their repo home if ever needed.
- **Search** read models implement `IIdEntity` (preserved); Search uses Specifications, not these
  repos — untouched.

## Execution order
1. Kernel marker change (`IEntity<TKey>`, `IIdEntity`, `IGuidEntity`) — compile Kernel.
2. DataAccess interfaces + base classes.
3. Sweep interface declarations (`IIdRepository`/`IGuidRepository` → new names).
4. Per-context `Repository.cs` files (write modules).
5. Customer read modules (read base + 3 repos).
6. Build solution; fix fallout.
7. Run integration suite (`integration-debug`) — confirms `Exists` translation + read paths.
   No E2E needed (no behavioural/UI change), but a `e2e-ui-regress` is cheap insurance.
