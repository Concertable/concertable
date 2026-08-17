# Code Conventions — Concertable's own precedents

The generic C# standard is not here. It lives in load-on-demand skills, which apply to every .NET repo:
`csharp-style` (fields, `this.`, `null!`, primary constructors, braces, optional parameters, `base.`,
`#region`, `extension()`), `csharp-naming` (suffix table, `Projection`, `Response`/`Dto`, `XMappers`,
extensions vs evaluators, frozen tables), `comments`, `dependency-injection`, `logging`, `validation`,
`persistence`, `multitenancy`, `keyed-strategies`, `module-structure`, `http-api`,
`microservice-boundaries`, `proto`, `seeding`, `result-carriers`, `result-errors`, `result-terminals`,
`unit-testing`, `integration-testing`, `e2e-scenarios`.

This file carries only what those skills deliberately omit: the roster of real types in *this* repo.
Per-service precedents live in that service's own docs — B2B's data-access and deal rosters are in
[`../Concertable.B2B/CODE_PATTERNS.md`](../Concertable.B2B/CODE_PATTERNS.md).

## Repositories bind to a `Concertable.DataAccess` capability, not to a context type

The shared `Concertable.DataAccess.Infrastructure` implementations mirror the context and repository
capability hierarchies:

```text
IReadDbContext  -> IReadRepository<TEntity, TKey>  -> ReadRepository<TEntity, TKey>
IWriteDbContext -> IWriteRepository<TEntity>       -> WriteRepository<TEntity>
IDbContext      -> IRepository<TEntity, TKey>      -> Repository<TEntity, TKey>
```

The shared bases take **no concrete `TContext` parameter**; their protected `Context` property exposes
only the matching capability. A module keeps a local `Repository<TEntity>` alias binding its concrete
context and key type — `int` + `IIdEntity` for most modules, `Guid` + `IGuidEntity` for User and Tenant:

```csharp
internal abstract class Repository<TEntity>(TenantDbContext context)
    : Repository<TEntity, Guid>(context)
    where TEntity : class, IGuidEntity;
```

Keep a module-local `ReadRepository<TEntity>` / `WriteRepository<TEntity>` alias only when concrete
repositories actually derive from it. Retain the concrete context in a `private readonly context` field
only when the repository needs typed `DbSet`s or `Entry`/`Database`/`ChangeTracker`/bulk operations.

**Adding one entity with nothing else staged — `InsertAsync`, not `AddAsync` + `SaveChangesAsync`.**
`IWriteRepository<TEntity>` gives both: `AddAsync` stages only, for a unit of work that stages several
writes before one shared save; `InsertAsync` stages *and* saves. Reach for the two-call form only when
something else is already staged in the same method and the save commits all of it together.

## `IPagination<T>.Map` lives in `Concertable.Contracts`

Beside `IPagination<T>` itself, so every layer can reach it — including `*.Api`, which deliberately does
not reference the data-access package.

## Create WGS84 geometry through `IGeometryProvider`

Inject `[FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider` and call
`geometryProvider.CreatePoint(latitude, longitude)`. Never `new GeometryFactory()` or `new Point(...)`.

## Versioned integration events — version the wire identity, not the C# type

Keep the CLR event name free of transport-version suffixes. Put the version in the stable `MessageType`
wire identity: `PaymentOperationStateChanged` with
`concertable.payment.payment-operation-state-changed.v1`, never `PaymentOperationStateChangedV1`.
Application code talks in domain event names; serializers and brokers own wire-version selection.

## Known violations

Type-suffix violations awaiting a batched rename sweep, and the legacy `(this X x)` extension methods
awaiting migration to `extension()` blocks, are listed in [`../TECH_DEBT.md`](../TECH_DEBT.md). Don't add
new ones.
