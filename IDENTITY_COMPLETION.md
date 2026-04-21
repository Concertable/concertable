# Identity Module — Completion Gaps

Findings from investigation session. All items below are outstanding before Identity can be
considered properly isolated. Items are ordered by impact on module integrity.

---

## 1. `UserRepository` — Drop `ApplicationDbContext`, Inject `IReadDbContext`

**File:** `api/Modules/Identity/Concertable.Identity.Infrastructure/Repositories/UserRepository.cs`

`UserRepository` currently injects `ApplicationDbContext` transitionally for 4 cross-module queries:

```csharp
public UserRepository(IdentityDbContext context, ApplicationDbContext appContext)
```

The 4 methods using `appContext`:
- `GetByApplicationIdAsync(int applicationId)` — joins `OpportunityApplications → Artist.UserId`
- `GetByConcertIdAsync(int concertId)` — joins `Concerts → Booking.Application.Artist.UserId`
- `GetIdByApplicationIdAsync(int applicationId)` — same, returns `Guid?` only
- `GetIdByConcertIdAsync(int concertId)` — same, returns `Guid?` only

These are cross-module reads: they navigate from Concert/Application entities to get a User FK.
`IdentityDbContext` knows nothing about `OpportunityApplicationEntity` or `ConcertEntity` — it
can't do the join. The right tool is `IReadDbContext`, which spans all modules and has
`IQueryable<OpportunityApplicationEntity>`, `IQueryable<ConcertEntity>`, and can do the join in
one query.

**Proposed replacement:**

```csharp
public UserRepository(IdentityDbContext context, IReadDbContext readDb)

// GetIdByApplicationIdAsync — pure read, no UserEntity needed at all
public Task<Guid?> GetIdByApplicationIdAsync(int applicationId) =>
    readDb.OpportunityApplications
        .Where(a => a.Id == applicationId)
        .Select(a => (Guid?)a.Artist.UserId)
        .FirstOrDefaultAsync();

// GetByApplicationIdAsync — read FK via readDb, then fetch UserEntity from IdentityDbContext
public async Task<UserEntity?> GetByApplicationIdAsync(int applicationId)
{
    var userId = await readDb.OpportunityApplications
        .Where(a => a.Id == applicationId)
        .Select(a => (Guid?)a.Artist.UserId)
        .FirstOrDefaultAsync();
    return userId.HasValue ? await context.Users.FirstOrDefaultAsync(u => u.Id == userId) : null;
}
```

Note: `IReadDbContext` is not injected anywhere else in Identity — this is the one legitimate
cross-module read case in the Identity module, and `IReadDbContext` exists precisely for it.

`Concertable.Identity.Infrastructure.csproj` will need a reference to `Concertable.Data.Application`.

---

## 2. `ArtistManagerRepository` + `VenueManagerRepository` — Move to Identity.Infrastructure

**Files:**
- `api/Concertable.Infrastructure/Repositories/ArtistManagerRepository.cs`
- `api/Concertable.Infrastructure/Repositories/VenueManagerRepository.cs`

These repositories implement `IManagerRepository<T>` which lives in `Identity.Application.Interfaces`.
`ArtistManagerEntity` and `VenueManagerEntity` are Identity-owned types. They belong in
`Identity.Infrastructure`, not `Concertable.Infrastructure`.

Current state — both query `ApplicationDbContext.Users.OfType<T>()` and cross-join to
`ApplicationDbContext.Concerts` / `ApplicationDbContext.OpportunityApplications`:

```csharp
public class ArtistManagerRepository : GuidRepository<ArtistManagerEntity>, IManagerRepository<ArtistManagerEntity>
{
    public ArtistManagerRepository(ApplicationDbContext context) : base(context) { }

    public async Task<ArtistManagerEntity?> GetByConcertIdAsync(int concertId) =>
        context.Users.OfType<ArtistManagerEntity>()
            .Where(u => u.Id == context.Concerts
                .Where(c => c.Id == concertId)
                .Select(c => c.Booking.Application.Artist.UserId)
                .First())
            .FirstOrDefaultAsync();
    ...
}
```

`ApplicationDbContext.Users` only works here because `ApplicationDbContext` still has a `Users`
`DbSet` — a legacy remnant that should be removed once these repos are properly migrated.

**Proposed design after move:**

```csharp
// Identity.Infrastructure/Repositories/ArtistManagerRepository.cs
internal class ArtistManagerRepository : IManagerRepository<ArtistManagerEntity>
{
    private readonly IdentityDbContext context;
    private readonly IReadDbContext readDb;

    // GetByConcertIdAsync: resolve FK via IReadDbContext, then load entity from IdentityDbContext
    public async Task<ArtistManagerEntity?> GetByConcertIdAsync(int concertId)
    {
        var userId = await readDb.Concerts
            .Where(c => c.Id == concertId)
            .Select(c => (Guid?)c.Booking.Application.Artist.UserId)
            .FirstOrDefaultAsync();
        return userId.HasValue
            ? await context.Users.OfType<ArtistManagerEntity>().FirstOrDefaultAsync(u => u.Id == userId)
            : null;
    }
    ...
}
```

Same pattern for `VenueManagerRepository`.

**Registration:** `AddIdentityModule()` in `ServiceCollectionExtensions` already registers
`IManagerRepository<T>` — move the registration there from `Concertable.Web` and `Concertable.Workers`
`ServiceCollectionExtensions`.

**Callers of `IManagerRepository<T>`:**
- `DoorSplitConcertWorkflow`, `VersusConcertWorkflow` — `GetByConcertIdAsync`
- `FlatFeeConcertWorkflow`, `VenueHireConcertWorkflow` — `GetByApplicationIdAsync`
- `ArtistTicketPaymentService`, `VenueTicketPaymentService` — `GetByConcertIdAsync`

All callers are in `Concertable.Infrastructure` and already reference `Identity.Application` —
the interface move is transparent to them.

---

## 3. New Module Base Repositories — Replace the Monolith Pattern

**Background:**

The monolith base classes (`BaseRepository<T>`, `GuidRepository<T>`, `Repository<T>`) are all
hardcoded to `ApplicationDbContext`. They die when `ApplicationDbContext` is removed. The monolith
repositories (`ArtistRepository`, `VenueRepository`, etc.) stay on those bases until their own
modules are extracted — **do not touch them**. But every new module repository going forward needs
a replacement base that works against a typed `DbContextBase` subclass.

**What to build (in `Concertable.Data.Infrastructure`):**

```csharp
// Replaces BaseRepository<T> — implements IBaseRepository<T>
public abstract class ModuleRepository<TEntity, TContext>(TContext context)
    : IBaseRepository<TEntity>
    where TEntity : class
    where TContext : DbContextBase
{
    protected readonly TContext context;

    public void Add(TEntity entity) => context.Set<TEntity>().Add(entity);
    public void Update(TEntity entity) => context.Set<TEntity>().Update(entity);
    public void Remove(TEntity entity) => context.Set<TEntity>().Remove(entity);
    public Task SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}

// Replaces GuidRepository<T> — implements IGuidRepository<T>
public abstract class GuidModuleRepository<TEntity, TContext>(TContext context)
    : ModuleRepository<TEntity, TContext>(context), IGuidRepository<TEntity>
    where TEntity : class
    where TContext : DbContextBase
{
    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        context.Set<TEntity>().FindAsync([id], ct).AsTask();
}
```

Each module's concrete repository inherits the appropriate base with its own typed context:

```csharp
// Identity.Infrastructure
internal class ArtistManagerRepository(IdentityDbContext context, IReadDbContext readDb)
    : GuidModuleRepository<ArtistManagerEntity, IdentityDbContext>(context),
      IManagerRepository<ArtistManagerEntity>
{
    // Cross-module queries use readDb; entity loads use context via base
}
```

**Why `TContext : DbContextBase` not `DbContextBase` directly:**

The typed parameter preserves access to concrete DbSet properties on the specific module context
(e.g., `context.Users` not just `context.Set<UserEntity>()`). CRUD operations fall through to
`context.Set<T>()` in the base, so concrete DbSet access in subclasses is just a convenience.

**Placement:** Both classes go in `Concertable.Data.Infrastructure`, same project as `DbContextBase`.
Every module Infrastructure project already references `Concertable.Data.Infrastructure` for
`DbContextBase` — no new dependencies needed.

**Interfaces stay in `Concertable.Shared`:** `IBaseRepository<T>` and `IGuidRepository<T>` are
already there and remain unchanged. The new bases implement them.

**Note to implementer:** if you find a cleaner design (e.g., not using a second type parameter,
using an accessor delegate, or a different hierarchy), present it — the goal is clean module
repositories without `ApplicationDbContext` coupling, not strict adherence to the shape above.

**Immediate action for Identity:** `ArtistManagerRepository` and `VenueManagerRepository` (§2)
should use `GuidModuleRepository<T, IdentityDbContext>` as their base. They have cross-module
reads via `IReadDbContext` — the base handles CRUD, the subclass adds the cross-module methods.

**Monolith repos** — `ArtistRepository`, `VenueRepository`, etc. stay on `BaseRepository<T>` /
`GuidRepository<T>` with `ApplicationDbContext` until their modules are extracted. Do not migrate
them. Those base classes are monolith artifacts and will be deleted alongside `ApplicationDbContext`.

---

## 4. Namespace Mismatches — Identity.Application and Identity.Infrastructure

All files physically in `Identity.Application` still declare old monolith namespaces:

| Declared namespace | Should be |
|---|---|
| `Concertable.Application.Interfaces` | `Concertable.Identity.Application.Interfaces` |
| `Concertable.Application.Interfaces.Auth` | `Concertable.Identity.Application.Interfaces.Auth` |
| `Concertable.Application.Mappers` | `Concertable.Identity.Application.Mappers` |
| `Concertable.Application.Requests` | `Concertable.Identity.Application.Requests` |
| `Concertable.Application.Validators` | `Concertable.Identity.Application.Validators` |

All files in `Identity.Infrastructure` also use old namespaces:

| Declared namespace | Should be |
|---|---|
| `Concertable.Infrastructure.Repositories` | `Concertable.Identity.Infrastructure.Repositories` |
| `Concertable.Infrastructure.Services` | `Concertable.Identity.Infrastructure.Services` |
| `Concertable.Infrastructure.Services.Auth` | `Concertable.Identity.Infrastructure.Services.Auth` |
| `Concertable.Infrastructure.Validators` | `Concertable.Identity.Infrastructure.Validators` |

This is the same class of issue as the Domain namespace fix done this session. Callers currently
find these types via `global using Concertable.Application.Interfaces` (and similar), which works
but makes the module boundary invisible.

`IManagerRepository.cs` is the only file that already has the correct namespace
(`Concertable.Identity.Application.Interfaces`) — everything else was migrated with a keep-namespace
shortcut.

**Impact of fixing:** Every `global using Concertable.Application.*` that was added for Identity
types would need to become `global using Concertable.Identity.Application.*`. Many files in
`Concertable.Infrastructure` and `Concertable.Web` explicitly `using Concertable.Application.Interfaces`
or `using Concertable.Infrastructure.Repositories` — those are fine because those namespaces will
still exist for the non-Identity monolith code. They just won't pull in Identity types anymore.

**Scope estimate:** ~40 files across `Identity.Application` and `Identity.Infrastructure` need
namespace updates. The global using files (13 projects) need the new Identity namespaces added.

**Recommendation:** Do this as a dedicated step before Artist extraction. Not blocking for tests
but is a genuine module boundary gap — if someone adds a new class to `Identity.Application` with
`namespace Concertable.Application.Interfaces`, it looks like monolith code not module code.

---

## 5. `IUserService` Cross-Module Methods — Deferred to Concert Extraction

`IUserService` exposes `GetByApplicationIdAsync` and `GetByConcertIdAsync` (returns full
`UserEntity`) and the `GetId*` variants. These are called from nowhere in the current codebase
outside of `UserService` itself. However, having Concert-entity navigation in an Identity
service interface is a leaky boundary. Flagged here; addressed during Concert extraction.

---

## 6. `ApplicationDbContext.Users` — Should Be Removed

Once §1 and §2 are done, `ApplicationDbContext` no longer needs a `Users` `DbSet`. It stays
currently because `ArtistManagerRepository` and `VenueManagerRepository` query it. After those
repos move to Identity.Infrastructure, `DbSet<UserEntity> Users` and the `OnModelCreating`
`UserEntity` config in `ApplicationDbContext` can both be removed. This is a meaningful cleanup —
`ApplicationDbContext` should not know about Identity-owned entities.

---

## Summary — Order of Work

| # | Item | Blocker? |
|---|---|---|
| 1 | `UserRepository`: replace `ApplicationDbContext` with `IReadDbContext` | Yes — Identity leaks into Application |
| 2 | Move `ArtistManagerRepository` + `VenueManagerRepository` to Identity.Infrastructure | Yes — Identity types managed outside Identity |
| 3 | Remove `DbSet<UserEntity>` from `ApplicationDbContext` | After §2 |
| 4 | Namespace rename (Identity.Application + Identity.Infrastructure) | No — cosmetic but important before Artist extraction |
| 5 | `BaseRepository` decision | No — decide during Artist extraction |
| 6 | `IUserService` cross-module methods | No — Concert extraction |
