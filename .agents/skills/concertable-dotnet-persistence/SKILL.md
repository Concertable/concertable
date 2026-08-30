---
name: concertable-dotnet-persistence
description: Concertable's repository roster — repositories bind to a `Concertable.DataAccess` capability rather than a concrete context type, the shared bases take no `TContext` parameter, and each module keeps a local `Repository<TEntity>` alias binding its concrete context and key type (`int`/`IIdEntity` for most modules, `Guid`/`IGuidEntity` for User and Tenant). Use when adding or reviewing a repository here, or choosing a base or alias.
---

# Persistence — Concertable's repository roster

The generic standard is the `persistence` skill: the capability triple and the base it selects, the module
alias, one repository per entity, `InsertAsync` vs `AddAsync`, schema constants, `IQueryable` never leaking,
`CancellationToken`, paging via `Map`, and exactly one context migrating any given table. This file is
the roster of what those shapes bind to here.

## Repositories bind to a `Concertable.DataAccess` capability, not to a context type

`Concertable.DataAccess.Infrastructure` holds the shared implementations of the capability triple. Each
module's local `Repository<TEntity>` alias binds its own context and key type — `int` + `IIdEntity` for most
modules, `Guid` + `IGuidEntity` for User and Tenant:

```csharp
internal abstract class Repository<TEntity>(TenantDbContext context)
    : Repository<TEntity, Guid>(context)
    where TEntity : class, IGuidEntity;
```

## The four borrowed tables

Every context maps the outbox and inbox with `ExcludeFromMigrations`, from the shared `DbContextBase` —
`Concertable.Messaging` owns their schema, so a service's own migrations must never claim them. Beyond
those, exactly two tables are borrowed across modules today, both by Concert reading another module's
rating projection:

| Borrowed table | Owned and migrated by | Borrowed read-only by |
|---|---|---|
| `messaging.OutboxMessages`, `messaging.InboxMessages` | `Concertable.Messaging` | every `DbContextBase` |
| `artist.ArtistRatingProjections` | Artist (`ArtistEntityConfiguration`) | `Concert.Infrastructure/Data/Configurations/ArtistRatingProjectionConfiguration.cs` |
| `venue.VenueRatingProjections` | Venue | `Concert.Infrastructure/Data/Configurations/VenueRatingProjectionConfiguration.cs` |

The borrowing configuration keys the projection and sets `ValueGeneratedNever`, because the owning module
assigns the id. Nothing else in the repo borrows a table — if you are about to add the fourth, check first
that a query on the owning module's read stance would not do instead.
