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

## No `string.Empty` as a "populated later" default — use `null!`

A non-nullable `string` property that something else populates (deserialization DTOs, EF entities, config-bound options, audit interceptors) defaults to `null!`, never `string.Empty`. An empty-string default masks a missing value as a present-but-blank one; `null!` says plainly "something else assigns this before use".

Where an empty string is the genuine runtime value (a fallback in `??` / `GetValueOrDefault`, a log fragment), `string.Empty` is correct — keep it. Never the `""` literal.

```csharp
// CORRECT — populated by the deserializer
public string LongName { get; init; } = null!;

// CORRECT — empty string is the real fallback value
var type = metadata.GetValueOrDefault("type", string.Empty);

// WRONG — placeholder default pretending to be a value
public string LongName { get; init; } = string.Empty;
```

## No primary constructors for captured state

Captured constructor parameters — anything read by a method or property — must be explicit `private readonly` fields assigned via `this.field = param`, never primary-constructor captures. This covers services, repositories, handlers, and validators, and any base class that uses its dependencies (e.g. the `TenantScopedDbContext` / `AdminDbContext` bases, whose `provider` and `defaultSchema` are read in `OnModelCreating`).

A constructor that only forwards its parameters to `base(...)` and captures nothing may use a primary constructor — there is no field to make `readonly`, so the shorthand is the clearest spelling. The pure base-forwarder leaf DB contexts (e.g. `PublicVenueDbContext`, `AdminVenueDbContext`) are the standing example.

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

## Base-class members — call through `base.`

When invoking a member that's inherited from a base class (not declared on the current type), qualify
the call with `base.`. It tells the reader at a glance that the member lives on the base, not in this
class, so they don't hunt for a definition that isn't here.

```csharp
// CORRECT — CurrentTenant is defined on TenantScopedRepository, not this repo
return await base.CurrentTenant.Where(v => v.IsActive).ToListAsync(ct);

// WRONG — reads like a local member
return await CurrentTenant.Where(v => v.IsActive).ToListAsync(ct);
```

## No comments on WHAT the code does

Only add a comment when the WHY is non-obvious (hidden constraint, subtle invariant, workaround for a specific bug). Never narrate what the code does — well-named identifiers already do that.

## Comments — default to none; mechanics here, policy in root `CLAUDE.md`

The repo-wide policy (default to zero, ≤2 lines, *why* lives in the commit message, and the disqualifiers — restating docs, citing transient artifacts, narrating the *what*) is in the root [`CLAUDE.md`](../../CLAUDE.md). The C#-mechanical part: a WHY-comment is one line where it can be → `//`; the rare genuinely-multi-line one → a single `/* */` block, never stacked `//` lines.

## Doc comments — XML `<summary>`, not `//`

Use these **sparingly** — don't pollute the codebase with summaries on self-explanatory types and members. Add one only where a developer (or an AI) reading the code later would genuinely benefit: real ambiguity, a non-obvious constraint, a safety/ordering subtlety, an API contract. A summary that just restates the name earns its deletion. The audience is whoever maintains the code next — write it for them.

**Don't document both an interface and its implementation.** The contract lives on the interface — that's the one place a summary belongs. The implementing class repeats nothing; leave it bare unless the *implementation itself* has a non-obvious quirk the interface can't speak to (a specific algorithm, a workaround). Two summaries saying the same thing is just drift waiting to happen.

When you *do* document a type or member, write it as an XML doc comment (`/// <summary>…</summary>`), not a `//` line comment. Reserve `//` for short inline notes *inside* method bodies. Cross-reference with `<see cref="…"/>` / `<see langword="null"/>` instead of bare prose, and use `<c>Name</c>` for a type the declaring assembly can't reference (avoids an unresolved-cref warning).

```csharp
// CORRECT — documents the member
/// <summary>
/// The owning tenant. Settable so <c>TenantInterceptor</c> can stamp it at SaveChanges; domain
/// code never sets it directly.
/// </summary>
Guid TenantId { get; set; }

// WRONG — docstring-style note as a line comment on a member
// Settable so the interceptor can stamp it
Guid TenantId { get; set; }
```

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
