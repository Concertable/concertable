# Code Conventions

Injected collaborators default to interface-typed dependencies and interface-to-implementation DI registrations; use a concrete type only when an interface adds literally no value or actively makes an established pattern worse.

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

**Constructor assignments are always `this.`-qualified** — fields *and* public auto-properties. When a
member is a surfaced public auto-property (see the dependency-holder pattern in
[`CODE_PATTERNS.md`](./CODE_PATTERNS.md)), still write `this.Property = param`, not a bare
`Property = param`. Uniform `this.` at every assignment site reads consistently and makes the
member-vs-param split obvious at a glance.

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

**Name a repository method for the query, a service method for the intent.** A repository
finder says literally what it fetches and by what key — `GetByTenantIdAsync`,
`GetUnreadCountByTenantIdAsync` — so the data access is obvious at the call site. The
use-case name (`GetInboxAsync`, `GetInboxSummaryAsync`) belongs on the *service* that calls
it. Don't push an intent name (`GetInbox`) down onto the repository.

## Table + schema names — the module `Schema.cs` constants

Each persistence module owns a `Schema.cs` (`internal static class Schema`) holding its DB schema name and
its table names as `const string`s — `Schema.Name` (e.g. `"concert"`) and `Schema.Tables.X` (e.g.
`Schema.Tables.Invoices`). EF configs reference these — `builder.ToTable(Schema.Tables.Invoices, Schema.Name)` —
never a bare string literal, so a renamed table changes one constant, not N scattered strings.

Column names need no equivalent: EF names each column after its property, so a config sets one only for a
deliberate rename (`HasColumnName("Period_Start")`) — and those few stay inline literals, not a constants class.

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

## Empty blocks — compact braces

Write deliberately empty blocks with `{ }` on the same line:

```csharp
catch (OperationCanceledException) { }
```

## Optional parameters — don't add one that callers must skip with a named argument

An optional parameter earns its place only when call sites actually pass it *positionally* and naturally. The moment varying one argument forces a call site to name-skip past another —
`ApplyAsync(opportunityId, signatoryName: name)` to hop over an unwanted `paymentMethodId` — the optional has stopped paying for itself: the signature grew, the call got noisier, and nothing reads more clearly. Prefer, in order: vary the value **inline** at the one call site that needs it (especially in tests — a self-contained Arrange beats threading a knob through a shared helper), or add a small **focused overload/helper** for that shape. Only keep the extra optional when several call sites genuinely pass it the ordinary way.

```csharp
// WRONG — the named arg exists only to skip paymentMethodId; the knob serves one caller
private Task<int> ApplyAsync(int opportunityId, string? paymentMethodId = null, string signatoryName = "Test Signatory") ...
var id = await ApplyAsync(opportunityId, signatoryName: "Aretha Artist");

// CORRECT — the one caller that needs a distinct name just does the apply inline
await artistClient.PostAsync($"/api/Application/{opportunityId}", new { eSignature = new { signatoryName = "Aretha Artist" } });
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

The repo-wide policy (default to zero, ≤2 lines, *why* lives in the commit message, and the disqualifiers — restating docs, citing transient artifacts, narrating the *what*) is in the root [`AGENTS.md`](../../AGENTS.md). The C#-mechanical part: a WHY-comment is one line where it can be → `//`; the rare genuinely-multi-line one → a single `/* */` block, never stacked `//` lines.

**Placement:** put the `//` on its own line directly above the statement, or inline after it with a single space — never pad with spaces to align comments into a column.

## Doc comments — XML `<summary>`, not `//`

Use these **sparingly** — don't pollute the codebase with summaries on self-explanatory types and members. Add one only where a developer (or an AI) reading the code later would genuinely benefit: real ambiguity, a non-obvious constraint, a safety/ordering subtlety, an API contract. A summary that just restates the name earns its deletion. The audience is whoever maintains the code next — write it for them.

**Don't document both an interface and its implementation.** The contract lives on the interface — that's the one place a summary belongs. The implementing class repeats nothing; leave it bare unless the *implementation itself* has a non-obvious quirk the interface can't speak to (a specific algorithm, a workaround). Two summaries saying the same thing is just drift waiting to happen.

When you *do* document a type or member, write it as an XML doc comment (`/// <summary>…</summary>`), not a `//` line comment. Reserve `//` for short inline notes *inside* method bodies. Cross-reference with `<see cref="…"/>` / `<see langword="null"/>` instead of bare prose, and use `<c>Name</c>` for a type the declaring assembly can't reference (avoids an unresolved-cref warning).

**Lead with what the thing *is*, in plain words** — "A snapshot of the deal, frozen at Accept." beats terse jargon like "Columns are copies, never references to the live deal". Name the kind-of-thing ("a snapshot of X", "a cache of X", "a guard that…"), then only the constraint that matters. Don't over-explain — a good "X of Y" opener usually carries it.

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

## Type-name suffixes — `Service` means "orchestrates a repository", not "a class I inject"

`Service` is the suffix that rots first: it gets used for anything injectable, and once a pure
value-producer is also called `Service`, the genuinely useful smell — *a service calling another
service* — stops being visible, because every collaborator looks the same at the injection site.

Pick the suffix from the type's **shape**, not from "it's registered in DI". Almost everything here is
DI'd; that fact carries no naming information.

| Suffix | The shape it claims | Precedent |
|---|---|---|
| `Service` | Orchestrates domain logic **over a repository**. Stateful collaborator, owns a unit of work. | `IVenueService`, `IInvitationService` |
| `Repository` | Domain-entity persistence via the module `DbContext`. | `ITenantRepository` |
| `Store` | Bytes/blobs in and out of a backing store, no domain logic. | `IUserStore` (ASP.NET Identity) |
| `Client` | A remote or third-party API. | `HttpClient`, `BlobServiceClient` |
| `Factory` | Creates **instances/components**, usually of a type family. | `IHttpClientFactory`, `ILoggerFactory` |
| `Generator` | Produces a **value/artifact** from inputs. | `LinkGenerator`, `RandomNumberGenerator` |
| `Builder` | **Mutable, stepwise** accumulation, terminated by `Build()`/a final property. | `StringBuilder`, `UriBuilder`, `WebApplicationBuilder` |
| `Provider` | Supplies a value or pluggable strategy, often one of several. | `IServiceProvider`, `IFileProvider`, `TimeProvider` |
| `Accessor` | Exposes an ambient/current value. | `IHttpContextAccessor` |
| `Handler` | Reacts to a message or event. | `IIntegrationEventHandler<T>` |
| `Helper` / `Utility` | **`static class` of pure functions.** No DI, no state, no config. | `WebUtility`, `HttpUtility` |

Two rules that follow from the table:

- **`Helper`/`Utility` is reserved for `static`.** It is not the escape hatch for "injected but not
  really a service" — an injected, config-bound collaborator gets a shape noun (`Generator`, `Factory`,
  `Store`). Note .NET is itself inconsistent here (`IUrlHelper` is injected), which is exactly why we
  pin the stricter meaning rather than inherit the ambiguity.
- **`Builder` vs `Generator` vs `Factory` is decided by mechanics, not vibes** — mutable-then-finalize is
  a `Builder`; one-shot value from inputs is a `Generator`; one-shot *component* is a `Factory`.

For types whose whole job is a single operation, [`CODE_PATTERNS.md`](./CODE_PATTERNS.md) already governs
the name — the agent-noun of that one method (`Mapper.Map`, `Resolver.Resolve`, `Calculator.Calculate`,
`Renderer.Render`, `Serializer.Serialize`). This table is the same rule widened to collaborator shapes.

Known violations awaiting a batched rename sweep are listed in [`../TECH_DEBT.md`](../TECH_DEBT.md);
don't add new ones.

## Result pattern

Result, Option, typed-error, validation, construction, composition, testing, and transport-terminal
conventions live in [RESULT_PATTERN.md](./RESULT_PATTERN.md). It is the sole source of truth; do not
add Result-pattern rules here.

## DTO naming — `Response` is HTTP-only; typed `Result` is the service wrapper; C# DTOs carry no suffix

The `Response` suffix is reserved for the **HTTP-API wire layer** (`Module.Api/Responses/`, see the
"DTOs vs Responses" section in [`../AGENTS.md`](../AGENTS.md)). It does **not** belong on the C#
service/client DTOs that adapters (gRPC clients, service interfaces) pass around:

- **`Result<TValue, TError>`** is already the service-call wrapper — the "did it succeed" envelope.
  Naming the payload `XResponse` on top of `Result<XResponse, XError>` double-encodes "this is a
  reply".
- **Service and client DTOs carry no suffix.** Name them for the shape, Stripe-aligned where the
  concept mirrors Stripe: `Transfer`, `Refund`, `EscrowDeposit`, `PaymentOutcome` — not
  `TransferResponse`/`PaymentResponse`. Accept the Stripe-SDK name collision (`Stripe.Transfer`,
  `Stripe.Refund`) and resolve it with a `using` alias in the few files that need both
  (`using Transfer = Concertable.Payment.Contracts.Transfer;`).
- **Proto message names stay `*Response`.** `EscrowResponse`/`PaymentResponse` in a `.proto` are the
  native gRPC RPC vocabulary — wire-only, generated, and never surfaced as the C# DTO. The client- and
  server-side `XMappers` map proto `*Response` ⇄ the suffix-free C# DTO.

```csharp
// CORRECT — service/client DTO, no suffix; Result<TValue, TError> is the wrapper
Task<Result<EscrowDeposit, DepositError>> DepositAsync(...);
Task<Result<Transfer, ReleaseError>> ReleaseAsync(...);

// WRONG — Response suffix on a non-HTTP DTO, redundant with typed Result
Task<Result<EscrowResponse, DepositError>> DepositAsync(...);
```

## Mappers — `XMappers` extension methods

Type-to-type mapping (e.g. gRPC proto ⇄ domain/contract types) lives in a static `XMappers` class as extension methods named `ToTarget()`, not as private `MapX` helpers on the consumer.

```csharp
internal static class EscrowMappers
{
    public static EscrowDeposit ToEscrowDeposit(this Proto.EscrowResponse r) => ...;
    public static EscrowStatus ToEscrowStatus(this Proto.EscrowStatusType s) => ...;
}
```

## `#region` — sparingly, to group same-shaped members in an aggregating file

The codebase does **not** use `#region` in ordinary classes — it hides code and usually signals a class
that should be split. It earns its place in exactly one shape: a single file that legitimately
**aggregates many same-shaped members**, where grouping by owner/subject is the only practical way to
navigate.

The canonical case is a project's `Log.cs` — one file holding every `[LoggerMessage]` method for the
project — partitioned into `#region`s named for the **class/service that emits them**
(`#region EscrowService`, `#region WebhookProcessor`). Name the region for the thing it groups, never a
generic label. If a class *isn't* an aggregator of one member shape, don't reach for `#region` — split
it instead.

(Test classes have the analogous rule — region by method-under-test — in
[`UNIT_CONVENTIONS.md`](./UNIT_CONVENTIONS.md) / [`INTEGRATION_CONVENTIONS.md`](./INTEGRATION_CONVENTIONS.md).)

## Logging — source-generated `Log.cs`

No inline `logger.LogInformation/LogWarning/LogError(...)`. Each project owns one `Log.cs` (`internal static partial class Log`) with a `[LoggerMessage]` method per message; call `logger.PublishedVenueEvents(count)`. Source-gen gates on `IsEnabled(level)` so switched-off levels cost nothing.

```csharp
[LoggerMessage(Level = LogLevel.Information, Message = "Published {Count} venue events")]
internal static partial void PublishedVenueEvents(this ILogger logger, int count);
```

## Extension members — C# 14 `extension()` blocks, not `this`

New extension members go in `extension(Receiver)` blocks — one `XExtensions` static class per receiver type
(`EnvironmentsExtensions` extends `Environments`; `HostEnvironmentExtensions` extends `IHostEnvironment`). This is
the modern unified form (it also does properties/indexers/static members and groups by receiver). Never add a new
legacy `public static … (this X x)` method; the existing ones await a migration sweep ([`../TECH_DEBT.md`](../TECH_DEBT.md)).

## Geometry — use IGeometryProvider

Inject `[FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider` for WGS84 point creation. Never instantiate `GeometryFactory` or `new Point(...)` directly.

```csharp
var location = geometryProvider.CreatePoint(e.Latitude, e.Longitude);
```
