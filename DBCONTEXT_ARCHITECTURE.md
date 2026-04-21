# DbContext Architecture — Modular Monolith

## The Problem With the Current State

`IdentityDbContext : ApplicationDbContext` is an anti-pattern. `ApplicationDbContext` is slated for
deletion once all modules have their own contexts. Any module that inherits it gains implicit access
to every entity in the solution — defeating the modular boundary — and creates a hard dependency on
something that won't exist.

The same problem applies to any future module that inherits `ApplicationDbContext`.

---

## Target Architecture

```
Concertable.Data.Application        ← IReadDbContext interface
Concertable.Data.Infrastructure     ← DbContextBase, ReadDbContext, interceptors, migrations
Concertable.{Module}.Infrastructure ← XyzDbContext : DbContextBase (module entities only)
```

### Project responsibilities

| Project | Contains |
|---|---|
| `Concertable.Shared` | Pure primitives, interfaces, value objects, shared POCOs (e.g. GenreEntity). No EF dependency. |
| `Concertable.Data.Application` | `IReadDbContext` — interface over the read surface. References Shared + all module Domain projects. |
| `Concertable.Data.Infrastructure` | `DbContextBase`, `ReadDbContext : DbContextBase`, `AuditInterceptor`, `DomainEventDispatcher`, EF config for shared/reference entities, migrations, seeding. |
| `Concertable.{Module}.Infrastructure` | `XyzDbContext : DbContextBase` — declares only module-owned entities. No cross-module DbSets. |

---

## DbContextBase

Lives in `Concertable.Data.Infrastructure`. Behaviour only — no entity knowledge.

```csharp
public abstract class DbContextBase : DbContext
{
    private readonly IDomainEventDispatcher? dispatcher;

    protected DbContextBase(DbContextOptions options, IDomainEventDispatcher? dispatcher = null)
        : base(options)
    {
        this.dispatcher = dispatcher;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var events = ChangeTracker
            .Entries<IEventRaiser>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        foreach (var entry in ChangeTracker.Entries<IEventRaiser>())
            entry.Entity.ClearDomainEvents();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (dispatcher is not null)
            await dispatcher.DispatchAsync(events, cancellationToken);

        return result;
    }
}
```

`AuditInterceptor` registers via `AddDbContext` in each module's `AddXModule()`, injected as an
`IDbContextOptionsExtension` or passed in options builder — same physical interceptor, per-context
registration.

---

## Module DbContext Pattern

```csharp
public class IdentityDbContext : DbContextBase
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, IDomainEventDispatcher? dispatcher = null)
        : base(options, dispatcher) { }

    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    public DbSet<EmailVerificationTokenEntity> EmailVerificationTokens => Set<EmailVerificationTokenEntity>();
    public DbSet<PasswordResetTokenEntity> PasswordResetTokens => Set<PasswordResetTokenEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Only Identity entity config here.
        // Cross-module FKs (e.g. ArtistEntity.UserId) are NOT configured here.
        // The DB enforces the FK constraint. EF does not know about it.
    }
}
```

### Rule: no cross-module DbSets or navigation properties

A module DbContext only declares `DbSet<T>` for entities it owns. It does not configure
relationships to entities in other modules.

---

## Cross-Module FK Rule (critical)

When entity A (owned by Module X) has a FK to entity B (owned by Module Y):

**DO:** Map the FK as a plain property.
```csharp
// ConcertGenreEntity — owns GenreId, does not own GenreEntity
public int GenreId { get; private set; }
// No navigation property. No DbSet<GenreEntity> in ConcertDbContext.
// No HasOne<GenreEntity>() call.
```

**DO NOT:** Call `HasOne<GenreEntity>()` when `GenreEntity` is not registered in the context.
EF will shadow-register it, try to create its table, and add FK constraints — the opposite of
what you want.

**Why this works:** EF Core treats `GenreId` as a plain column. The DB-level FK constraint is
enforced by the schema (created by the context that owns `GenreEntity`, or by a seeding migration).
EF in the module context has no knowledge of the constraint and does not interfere with it.

**Verified:** EF Core docs confirm this is the supported pattern for bounded contexts.
See: *"relationships may need to reference an entity type in the model of a different context...
the foreign key column(s) should be mapped to normal properties."*

---

## ReadDbContext

Lives in `Concertable.Data.Infrastructure`. Inherits `DbContextBase`. Spans all module entities
for read/query purposes.

```csharp
public class ReadDbContext : DbContextBase
{
    // All module entities as DbSets — read side only
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<ArtistEntity> Artists => Set<ArtistEntity>();
    public DbSet<VenueEntity> Venues => Set<VenueEntity>();
    public DbSet<ConcertEntity> Concerts => Set<ConcertEntity>();
    // ... etc
}
```

`ReadDbContext` is registered as the implementation of `IReadDbContext`. Cross-module queries
(e.g. "given an applicationId, find the artist's UserId") go here — not in module DbContexts.

`IReadDbContext` lives in `Concertable.Data.Application` so module Application layers can
reference it without a hard dependency on EF or the concrete `ReadDbContext`.

---

## GenreEntity

GenreEntity is reference/lookup data — no module truly owns it in a write sense.

- **Write contexts:** `ConcertGenreEntity.GenreId` is a plain `int`. No `DbSet<GenreEntity>` in
  `ConcertDbContext`. DB enforces the FK.
- **Read side:** `ReadDbContext` has `DbSet<GenreEntity>` — queries go through there.
- **Long term:** A thin `ReferenceData` module could own `GenreEntity` if write behaviour is
  ever needed (e.g. admin CRUD). For now it lives in `Concertable.Core` and is exposed via
  `ReadDbContext`.

---

## Migration Strategy

One physical DB. Migrations are split per module context eventually.

**Current (transitional):**
- `ApplicationDbContext` still owns all migrations and the full schema.
- Module DbContexts (`IdentityDbContext` etc.) operate against tables already created by
  `ApplicationDbContext` migrations. They do not run their own migrations yet.
- This is intentional — safe to cut the inheritance without touching the schema.

**Per-module migration (target — done during each module extraction):**
1. Remove the module's entity configs from `ApplicationDbContext`.
2. Add an initial migration on the module's DbContext using `--context XyzDbContext`.
3. Mark it as applied: `dotnet ef database update --context XyzDbContext` against the existing DB
   (tables already exist — migration is a no-op for schema, just records history).
4. Future schema changes for that module go through the module's own migration context.

`ReadDbContext` is read-only — never runs migrations.

---

## Dependency Graph

```
Concertable.Shared
  ↑
Concertable.{Module}.Domain         (entities, domain events — no EF)
  ↑
Concertable.Data.Application        (IReadDbContext — refs all module domains)
  ↑
Concertable.Data.Infrastructure     (DbContextBase, ReadDbContext, interceptors)
  ↑
Concertable.{Module}.Infrastructure (XyzDbContext : DbContextBase)
```

Module Infrastructure → Data.Infrastructure for `DbContextBase`.
Module Application → Data.Application for `IReadDbContext`.
No circular dependencies.

---

## What Needs To Change (Implementation Order)

### Phase 1 — Create Concertable.Data projects
- [ ] Create `Concertable.Data.Application` csproj — add `IReadDbContext`
- [ ] Create `Concertable.Data.Infrastructure` csproj — move `DbContextBase` (extracted from
      `ApplicationDbContext.SaveChangesAsync`), `AuditInterceptor`, `DomainEventDispatcher`
- [ ] Move `ReadDbContext` from `Concertable.Infrastructure` to `Concertable.Data.Infrastructure`

### Phase 2 — Fix IdentityDbContext (proof of concept)
- [ ] `IdentityDbContext : DbContextBase` (stop inheriting `ApplicationDbContext`)
- [ ] Configure only Identity entities in `OnModelCreating`
- [ ] Remove `GetByApplicationIdAsync` / `GetByConcertIdAsync` from `IUserRepository` —
      these were cross-module queries only possible via inherited DbSets
- [ ] Verify build + 236 tests pass

### Phase 3 — Apply to each module during extraction
Each module follows the same pattern as Phase 2 when it is extracted.

### Phase 4 — Delete ApplicationDbContext
Once all modules have their own DbContext and all entities have been extracted, `ApplicationDbContext`
and `Concertable.Infrastructure` are deleted.

---

## Open Questions

- `AuditInterceptor` registration: per-context via options builder, or injected once at host level?
  Leaning toward per-context (each module opts in via `AddXModule()`).
- `DomainEventDispatcher` currently in `Concertable.Infrastructure` — move to
  `Concertable.Data.Infrastructure` as part of Phase 1.
- Cross-module event handlers (e.g. `UserEmailChanged` → sync Artist copy) — handlers live in the
  consuming module's Infrastructure, but the event shape lives in `Concertable.Shared`.
