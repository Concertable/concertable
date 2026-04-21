# Concertable — Architecture Guide

## Modular Monolith: Database Layer Rules

This project is being refactored into a modular monolith. The database architecture rules below are **non-negotiable** — deviating from them defeats the purpose of the refactor.

### Project Responsibilities

| Project | Contains | EF dep? |
|---|---|---|
| `Concertable.Shared` | Pure primitives — `IDomainEventDispatcher`, `IEventRaiser`, value objects, marker interfaces | No |
| `Concertable.Data.Application` | `IReadDbContext` with typed `IQueryable<XEntity>` properties | No (IQueryable is System.Linq) |
| `Concertable.Data.Infrastructure` | `DbContextBase`, `ReadDbContext`, `AuditInterceptor`, `DomainEventDispatcher`, migrations, seeding | Yes |
| `Concertable.{Module}.Infrastructure` | `XyzDbContext : DbContextBase` — only that module's owned entities | Yes |

`Concertable.Shared.Infrastructure` does not exist. `Concertable.Infrastructure` is the legacy project being gutted into `Concertable.Data.*`.

### DbContextBase

Lives in `Concertable.Data.Infrastructure`. Abstract, no DbSets — purely behavioural. Handles domain event dispatch and audit on every `SaveChangesAsync`. All module write contexts inherit from this, never from `DbContext` directly.

```csharp
public abstract class DbContextBase(DbContextOptions options, IDomainEventDispatcher? dispatcher = null)
    : DbContext(options)
{
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var events = ChangeTracker.Entries<IEventRaiser>()
            .SelectMany(e => e.Entity.DomainEvents).ToList();
        foreach (var entry in ChangeTracker.Entries<IEventRaiser>())
            entry.Entity.ClearDomainEvents();
        var result = await base.SaveChangesAsync(ct);
        if (dispatcher is not null)
            await dispatcher.DispatchAsync(events, ct);
        return result;
    }
}
```

### Module Write Contexts

Each module's write context inherits `DbContextBase` and declares **only the entities that module owns**.

```csharp
public class IdentityDbContext(DbContextOptions<IdentityDbContext> options, IDomainEventDispatcher? dispatcher = null)
    : DbContextBase(options, dispatcher)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();
    // Only Identity-owned entities
}
```

### Rules for Module Write Contexts

1. **Always inherit `DbContextBase`, never `ApplicationDbContext` or `DbContext` directly.**

2. **Only register entities this module owns.** No other module's entities, no shared join tables for reference data.

3. **Cross-module FK references are plain primitives.** A `Guid UserId` or `int GenreId` is fine. Do not add a navigation property or `DbSet<>` for an entity another module owns. The DB enforces the FK; EF in this context doesn't need to know the target entity exists.

4. **Never add new DbSets to `ApplicationDbContext`.** It is legacy and shrinks as modules are extracted.

### ReadDbContext and IReadDbContext

`IReadDbContext` lives in `Concertable.Data.Application` — it exposes typed `IQueryable<XEntity>` properties for every entity across all modules. No EF dependency (IQueryable is System.Linq), but does reference all module Domain projects.

`ReadDbContext` lives in `Concertable.Data.Infrastructure`, implements `IReadDbContext`, is strictly no-tracking, and throws on `SaveChanges`. It is the only context that spans all modules and is used for all cross-module reads.

### Shared / Reference Data (e.g. GenreEntity)

No module owns `GenreEntity` in a write sense. The rule is:

- Store the FK as a plain primitive (`int GenreId`) in join entities like `ConcertGenreEntity`.
- Do not configure `.HasOne<GenreEntity>()` in any module write context.
- Query genres via `IReadDbContext`.
- Eventually `GenreEntity` belongs in a `ReferenceData` module — for now it lives where it is.

### Module Extraction Order

Identity → Artist → Venue → Concert → Payment

Each extracted module must have its own `XyzDbContext : DbContextBase` before moving to the next.
