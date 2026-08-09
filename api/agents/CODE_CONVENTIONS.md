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

## Typed operation Results

All new and changed in-process operation contracts follow this decision rule. When a contract needs
to represent an expected failure or ordinary absence, its outcome vocabulary comes from
`Concertable.Kernel.Functional`. Existing unmigrated nullable, boolean, enum, void, or third-party
signatures that encode those semantics are migration debt, not precedent for new work. Choose the
smallest shape that preserves the caller's real decisions:

- Expected, caller-actionable failure with a success payload: `Result<TValue, TError>`.
- Expected, caller-actionable failure without a success payload: `UnitResult<TError>`.
- Ordinary absence with no failure explanation: `Option<T>`.
- A fallible operation whose successful payload is optional: `Result<Option<T>, TError>`.
- Collection query: `IReadOnlyList<T>`, with no matches represented by an empty list.
- No actionable failure or absence: return the plain value, capability boolean, or completion type;
  do not manufacture a Result solely for uniformity.

Use `Result<TValue>` or `Result` only for genuinely internal string-error flows that do or do not
carry a success value; new module, application, service, and client contracts with expected failures
use an operation-owned `TError`. Faults, cancellation, and violated invariants remain exceptions.

Persistence repository single-item lookups return nullable values (`Task<TEntity?>`), matching the
provider's missing-row contract. Module, application, service, and client boundaries convert that
nullable value with `ToOption()` and expose `Option<T>` for ordinary absence. Do not push `Option`
into repository or persistence contracts.

`TError` is an operation-owned `XError` union that implements `IError`. Each expected outcome has a
natural named case, whether it carries a payload or not; callers match domain outcomes rather than
comparing catalog values. Keep the error beside its operation and place it in Application,
`*.Contracts`, or a published client contract according to the widest caller that consumes it.
Shared Kernel owns only `IError`, `ErrorDefinition`, `ErrorKind`, and the generic definition
factories. Never carry Result, Option, Dunet, or service-owned error unions through HTTP DTOs,
protobuf, events, or persistence.

With Dunet, derive `Definition` in one exhaustive switch on the union. The case name is the source of
the default published code and message, so the ordinary declaration repeats neither:

```csharp
[Union(EnableImplicitConversions = false)]
public abstract partial record PaymentError : IError
{
    public ErrorDefinition Definition => this switch
    {
        PayerNotFound => ErrorDefinition.NotFound<PayerNotFound>(),
        Declined => ErrorDefinition.PaymentRequired<Declined>("The payment was declined."),
        ProviderFailure =>
            ErrorDefinition.Invalid<ProviderFailure>("The payment provider rejected the request.")
    };

    public partial record PayerNotFound;

    public partial record Declined;

    public partial record ProviderFailure(string Code);
}
```

Name every static value and union case for the exact domain outcome it represents. Prefer the
natural vocabulary (`ApplicationNotFound`, `ApplicationError`, `PayeeNotFound`,
`RecipientUnavailable`) and use it directly; do not add a `Case` suffix or a wrapper factory that
only renames or constructs the same case. A name and its definition must agree semantically:
`PayerNotFound` uses an `ErrorDefinition.NotFound`, while a broader or different definition requires
an honestly broader or different case name.

The switch is the complete case-to-definition map without positional `Match` lambdas or unused
parameters. A composite case deconstructs and forwards the nested definition:

```csharp
[Union(EnableImplicitConversions = false)]
public abstract partial record EscrowRefundError : IError
{
    public ErrorDefinition Definition => this switch
    {
        EscrowNotFound => ErrorDefinition.NotFound<EscrowNotFound>(),
        PaymentFailure(var error) => error.Definition
    };

    public partial record EscrowNotFound;

    public partial record PaymentFailure(PaymentError Error);
}
```

A validation case carries `ValidationErrors` and uses the same switch; convert its payload only at
the definition boundary:

```csharp
[Union(EnableImplicitConversions = false)]
public abstract partial record CreateDealError : IError
{
    public ErrorDefinition Definition => this switch
    {
        Invalid(var errors) =>
            ErrorDefinition.Validation<Invalid>(errors.ToDictionary())
    };

    public partial record Invalid(ValidationErrors Errors);
}
```

Do not add a discard/default arm. Dunet suppresses compiler warning `CS8509` only when the switch
covers every declared variant; an omitted variant keeps the warning and must never suppress it
locally. Use generated full `Match` only when delegates are the natural API for the behavior. When
logic deliberately inspects only some cases, use ordinary C# `is` type patterns.

Every union has an exact definition contract test for every case. Hard-code the expected code,
message, and semantic kind in the test; never calculate expected values with the production helper.
The test's case set must also be explicit so adding a variant requires a deliberate contract update.
Codes are lowercase dot-separated identifiers with an owning operation/module prefix
(`ticket.concert_not_found`); published codes are never renamed or reused for a different meaning.
Messages are caller-safe text, never exception messages, provider detail, SQL, stack traces, or
values whose disclosure has not been reviewed. Validation definitions contain at least one
structured field message.

Every kind has generic factories with and without an explicit message:
`Invalid<TCase>()`, `NotFound<TCase>()`, `Conflict<TCase>()`, `Unauthenticated<TCase>()`,
`Forbidden<TCase>()`, `PaymentRequired<TCase>()`, and `Validation<TCase>(errors)`. The no-message
factory humanizes the case name, so `PayerNotFound` publishes `Payer not found.`. Use the
explicit-message overload when the natural case name is not the complete safe caller-facing text,
as with provider failures or declined payments. Typed error cases never use `[DisplayName]`;
repository entity lookup remains a separate concern, where `OrNotFound<TEntity>()` may use the
entity type's `[DisplayName]`.

The same case name derives its code. The union's first word is the prefix, its remaining words are
context, leading case words that repeat the union are dropped, an optional `Case` suffix is ignored,
and acronyms and digits split on their own, so `EscrowRefundError.EscrowNotFound` publishes
`escrow.refund_not_found`. Deriving codes and default messages is Kernel's job alone; never add a
service-local reflection helper for either.

A published code the naming rule would move — a renamed case, or a prefix that is not the error's own
first word — keeps its code with `[ErrorCode("...")]` on the case. The attribute is not inherited from
the union root and its value is validated like any other code. The per-case contract test is what
makes this safe: a rename that would silently republish a code fails the test, not production.

Do not add an `ErrorCase` base-class hierarchy, `NotFound<T>` case inheritance, `IErrorSet<T>`, or
another marker/catalog abstraction. None adds information beyond the union and `IError`. Do not put
an attribute on every case, discover cases with runtime reflection, or generate code merely to hide a
lookup. A generator is justified only if it removes a demonstrated manual case inventory while
reporting missing, duplicate, or non-reconstructible wire cases at compile time.

### Typed errors at gRPC boundaries

The wire carries an open-ended string code plus its published message and kind; it never carries a
Dunet type. A client maps an application-error `RpcException` to its operation-owned error through a
total `ToXError()` extension. Keep the indexed instances private so callers see neither `Try`,
nullable, nor `Option` parsing:

```csharp
internal static class PaymentErrorMappers
{
    private static readonly FrozenDictionary<string, PaymentError> errors =
        new PaymentError[]
        {
            new PaymentError.PayerNotFound(),
            new PaymentError.Declined()
        }
        .ToFrozenDictionary(error => error.Definition.Code);

    internal static PaymentError ToPaymentError(this RpcException exception)
    {
        var detail = exception.ToOperationErrorDetail();

        if (!errors.TryGetValue(detail.Code, out var error)
            || detail.Message != error.Definition.Message
            || detail.Kind != error.Definition.Kind)
        {
            throw new PaymentContractMismatchException(detail.Code, exception);
        }

        return error;
    }
}
```

The dictionary is the reverse map from an open wire string to a closed case set; arbitrary future
input cannot be exhaustively matched. Do not replace it with `FromCode` switches,
parser-precedence chains, a public `TryToXError`, a nullable `ToXError`, or a catalog that merely moves
the same parsing elsewhere. Duplicate codes must fail a mapper contract test before merge; a future
generator, if justified, must report them at compile time. Do not use runtime assembly discovery.

Only cases reconstructible from the operation-error envelope belong in this instance dictionary. A
payload-carrying case must have an explicit structured wire detail and a mapper that reconstructs the
payload; if the transport does not carry that data, the case remains in-process. Never discard a
payload merely to force it through code-only lookup.

Catch caller cancellation before application errors. The reusable gRPC-only predicate belongs to
`Concertable.Grpc`, not Kernel or a service; generic operation-detail extraction belongs there once
the envelope is shared. `Concertable.Grpc` remains independently packable and service-agnostic; the
concrete error map and mismatch exception stay in the owning client so every service/package closure
builds without a sibling runtime reference:

```csharp
catch (RpcException ex) when (ex.IsClientCancellation(ct))
{
    throw new OperationCanceledException(
        "The payment operation was cancelled.",
        ex,
        ct);
}
catch (RpcException ex) when (ex.HasOperationErrorDetail())
{
    return Result<PaymentOutcome, PaymentError>.Failure(ex.ToPaymentError());
}
```

Unrelated network, availability, and protocol failures remain their original `RpcException`. An
application-error detail with an unknown code, or a known code whose message or kind changed, throws
the operation's contract-mismatch exception with the original `RpcException` as its inner exception.
It never becomes a domain `Unknown` case: adding one would weaken exhaustive domain handling and let
an old client pretend it understands a new server outcome. Publish and deploy the updated contract
and clients before a server starts emitting a new code.

Compose owned Results and Options with `Bind`, `Map`, `MapError`, `Ensure`, `Tap`, `OrFailure`, and
the Kernel Task extensions until a terminal adapter. Ordinary composition is fail-fast; only
validation flows explicitly designed to collect errors accumulate them and map that collection once
into their owning operation error. Consume payloads through composition, `Match`, or `TryGetValue` /
`TryGetError`; the owned types expose no throwing `Value`, `Error`, or `Unwrap` accessor.

`default(Option<T>)` is `None`; every default Result shape is an invalid, uninitialized value whose
state, observation, and composition members throw `InvalidOperationException`. Never manufacture,
return, or treat a default Result as success or failure. `Some(null)`, `Success(null)`, and
`Failure(null)` are invalid even if nullable warnings are disabled; use `ToOption()` at nullable
boundaries. Result and Option construction stays behind explicit factoriesâ€”no public case
constructors or implicit conversions.

Never introduce another Result/Option carrier or use CSharpFunctionalExtensions, FluentResults,
OneOf, ErrorOr, LanguageExt, or Dunet to implement the Kernel functional types. Do not add implicit
conversions, catch exceptions in combinators, turn failures into HTTP exceptions, or carry functional
types across transport or persistence boundaries.

Dunet appears only in error-union declarations, their exhaustive case handling, genuinely useful
full `Match` calls, and package configuration. Do not use generated `Unwrap` or case-specific
`MatchX` APIs without a concrete need. Keep `IError`, definitions, Result extensions, transports,
persistence, messages, and wire formats independent of Dunet.

Controllers terminate through `Concertable.Shared.Api.Results`. Result failures and exceptions both
write through `IProblemDetailsService`, so registered writers, content negotiation, request
instance, `traceId`, and `ProblemDetailsOptions.CustomizeProblemDetails` apply consistently.

Infrastructure adapters may normalize a provider-specific unavailability or deadline fault into
`DependencyUnavailableException` or `DependencyTimeoutException`, preserving the original as the
inner exception. Shared.Api maps only those explicit types to safe 503/504 ProblemDetails; broad
`HttpRequestException`, `RpcException`, `TimeoutException`, database exceptions, and unknown faults
remain safe 500s. Cancellation is never normalized or handled as a response.

At gRPC and worker terminals, match typed failures according to the operation policy and leave
dependency exceptions on the exception path for retry/dead-letter behavior. FluentResults remains
only as a temporary private aggregate-validation detail in unmigrated code and never implements or
mixes with the owned functional foundation.

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

## Geometry — use IGeometryProvider

Inject `[FromKeyedServices(GeometryProviderType.Geographic)] IGeometryProvider geometryProvider` for WGS84 point creation. Never instantiate `GeometryFactory` or `new Point(...)` directly.

```csharp
var location = geometryProvider.CreatePoint(e.Latitude, e.Longitude);
```
