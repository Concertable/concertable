# Code Conventions

## Private fields — no underscore prefix

Use `this.field` disambiguation in constructors instead of `_field` prefixes.

```csharp
// CORRECT
private readonly SearchDbContext context;

public MyService(SearchDbContext context)
{
    this.context = context;
}

// WRONG
private readonly SearchDbContext _context;

public MyService(SearchDbContext context)
{
    _context = context;
}
```

## No primary constructors for services

Services, repositories, handlers, and validators use an explicit constructor with `private readonly` fields assigned via `this.field = param`. No primary constructor shorthand.

## Repositories — inherit the module `Repository<T>` base

Every module owns a `Repositories/Repository.cs` that binds the shared
`Concertable.DataAccess.Infrastructure` bases to the module's `DbContext` and key type
(`int` + `IIdEntity` for most modules, `Guid` + `IGuidEntity` for User/Tenant):

```csharp
internal abstract class BaseRepository<TEntity>(TenantDbContext context)
    : BaseRepository<TEntity, TenantDbContext>(context)
    where TEntity : class;

internal abstract class Repository<TEntity>(TenantDbContext context)
    : Repository<TEntity, TenantDbContext, Guid>(context)
    where TEntity : class, IGuidEntity;
```

A concrete repository inherits that base and implements the module's `IXRepository`,
which extends `IRepository<XEntity, TKey>` (or `IRepository<XEntity>` for `int` keys) and
needs **no members of its own** unless the module has extra queries.
`GetAll`/`GetById`/`Exists`/`Add`/`Update`/`Remove`/`SaveChanges` all come from the base —
**never re-declare them** (not even a `CancellationToken` overload of `GetById`). Add only
the *extra* finders the base can't express (e.g. `GetByUserIdAsync`), querying through the
inherited `context` field.

```csharp
internal interface ITenantRepository : IRepository<TenantEntity, Guid>;

internal sealed class TenantRepository : Repository<TenantEntity>, ITenantRepository
{
    public TenantRepository(TenantDbContext context) : base(context) { }
    // extra finders only (e.g. GetByUserIdAsync) — query via the inherited `context`
}
```

The injected `DbContext` field is always named `context` (never `dbContext`) — see the
field-naming rule above. Don't hand-roll a bare `IXRepository` that re-implements CRUD;
inherit the base.

## Single-statement branches — no braces

```csharp
// CORRECT
if (condition)
    return;

// WRONG
if (condition)
{
    return;
}
```

## No comments on WHAT the code does

Only add a comment when the WHY is non-obvious (hidden constraint, subtle invariant, workaround for a specific bug). Never narrate what the code does — well-named identifiers already do that.

## Mappers — `XMappers` extension methods

Type-to-type mapping (e.g. gRPC proto ⇄ domain/contract types) lives in a static `XMappers` class as extension methods named `ToTarget()`, not as private `MapX` helpers on the consumer.

```csharp
internal static class EscrowMappers
{
    public static EscrowResponse ToEscrowResponse(this Proto.EscrowResponse r) => ...;
    public static EscrowStatus ToEscrowStatus(this Proto.EscrowStatusType s) => ...;
}
```

## Logging — source-generated `Log.cs`

No inline `logger.LogInformation/LogWarning/LogError(...)`. Each project owns one `Log.cs` (`internal static partial class Log`) with a `[LoggerMessage]` method per message; call `logger.PublishedVenueEvents(count)`. Source-gen gates on `IsEnabled(level)` so switched-off levels cost nothing.

```csharp
[LoggerMessage(Level = LogLevel.Information, Message = "Published {Count} venue events")]
internal static partial void PublishedVenueEvents(this ILogger logger, int count);
```

## Geometry — use IGeometryProvider

Inject `[FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider` for WGS84 point creation. Never instantiate `GeometryFactory` or `new Point(...)` directly.

```csharp
var location = geometryProvider.CreatePoint(e.Latitude, e.Longitude);
```
