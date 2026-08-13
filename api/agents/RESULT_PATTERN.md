# Result pattern

This is the source of truth for Result, Option, typed application errors, validation results, and
their transport adapters in Concertable. It applies to every backend service. Existing code that
uses another carrier or an older construction style is migration debt, not precedent.

## Packages and ownership

Use the Reunion package family directly in the project that consumes each API:

| Package | Owns |
|---|---|
| `Reunion` | `Result`, `Result<TValue>`, `Result<TValue, TError>`, `UnitResult<TError>`, `Option<T>`, their named cases, composition, collection, and task extensions |
| `Reunion.Errors` | `IError`, `ErrorDefinition`, `ErrorKind`, `ValidationErrors`, `ErrorCodeAttribute`, and definition factories |
| `Reunion.Validation` | `ValidationResult`, its `Valid`/`Invalid` cases, and validation accumulation |
| `Reunion.AspNetCore` | Minimal API and MVC terminal adapters |

Each service owns its exact Reunion versions in its service-local `Directory.Packages.props`. Keep
all Reunion packages in a service on one version; the current baseline is `0.1.0-alpha.2`. Reference
only the packages whose APIs a project uses, and reference them directly rather than relying on a
transitive dependency.

Do not redistribute Reunion through `Concertable.Kernel`, `Concertable.Shared.Api`, or another
Concertable package. The legacy `Concertable.Kernel.Functional` carriers and
`Concertable.Shared.Api.Results` terminals remain only until their owning migrations remove them.
New and changed contracts use Reunion directly.

## Choose the smallest truthful carrier

Choose the return type from the decisions the caller must make:

| Operation outcome | Return type |
|---|---|
| Success value or expected, actionable failure | `Result<TValue, TError>` |
| Completion or expected, actionable failure | `UnitResult<TError>` |
| Present or ordinarily absent as an intentional application outcome | `Option<T>` |
| Expected failure, otherwise an optional value | `Result<Option<T>, TError>` |
| Provider, storage, framework, wire, or short local value that may be null | `T?` |
| Zero or more values | `IReadOnlyList<T>`; no matches is an empty list |
| No actionable alternate outcome | Plain value, `Task`, or another completion type |
| Capability question only | `bool` |

The short rule is: use `T?` for technical nullability that stays in infrastructure or short local
plumbing; use `Option<T>` when `Some(T)` and `None` are the complete, intentional outcomes of an
in-process API. If absence is a named failure, needs an explanation, or must coexist with other
failure cases, use `Result<TValue, TError>` instead.

The layer is a strong heuristic, not the decision by itself. Repository and provider lookups
normally return `T?`. Domain, application, `IXModule`, service, and published C# client query
contracts normally promote ordinary absence to `Option<T>` so callers cannot access `T` without
observing the case. Commands and queries with named rejections use Result. A guaranteed value stays
a plain value, and an optional property on a DTO remains nullable rather than wrapping each field
in Option.

`Option<T>` is a distinct runtime value with a non-null payload; NRT annotations are compiler flow
analysis over the same runtime reference type. On .NET 10, Option forces conditional payload
extraction but cannot prevent a caller from ignoring the returned value or a `TryGetValue` boolean.
On .NET 11, Reunion's native-union asset additionally supports exhaustive `Some<T>`/`None` switch
expressions. Do not claim either carrier provides more safety than it actually does.

An expected failure is part of normal control flow and gives a caller a legitimate branch: not
found, invalid input, conflict, unauthenticated, forbidden, payment required, or another named
domain outcome. Infrastructure faults, violated invariants, programmer errors, and cancellation
remain exceptions.

Use `bool` only for an actual predicate such as `CanAuthenticate`. A command should not return
`bool` when `false` hides several caller actions. Conversely, do not manufacture a Result solely for
uniformity when every expected outcome is intentionally indistinguishable to the caller. Login is
an example: invalid credentials and an unknown account are both ordinary absence because preserving
that equivalence avoids an account-enumeration branch.

A lookup returning HTTP 404 does not automatically require a Result. `GetDetailsByIdAsync` may
return `Option<T>` when found and absent are the whole application contract and the HTTP terminal
owns the `None`-to-404 policy. Use a `NotFound` error case when the missing resource is one of several
operation failures, carries useful detail, or must remain distinguishable outside that terminal.

`Result<TValue>` and non-generic `Result` carry string errors. Keep them to genuinely private,
low-level flows where a string is the complete local contract. Module, application, service, and
published client operations with expected failures use an operation-owned `TError`.

## Boundary rules

Result and Option are in-process vocabulary. They may appear in domain, application, module, and
published C# client signatures. They never appear in:

- HTTP request or response DTOs;
- protobuf messages;
- integration events or messages;
- persistence entities, columns, or repository query contracts;
- configuration or serialized cache contracts.

Each edge maps the carrier to its owned wire or storage contract. Cross-service consumers reference
published client or Contracts packages, never another service's runtime project.

Repository single-item lookups return nullable values such as `Task<TEntity?>`, matching the
provider's missing-row contract. Convert nullable values to `Option<T>` when an ordinary
present-or-absent result crosses a domain, application, module, service, or client boundary. Do not
push `Option<T>` into EF or repository contracts, and do not wrap a nullable value merely to unwrap
it again in the same local flow.

```csharp
public interface IVenueReadRepository
{
    Task<VenueDetails?> GetDetailsByIdAsync(int venueId);
}

public interface IVenueService
{
    Task<Option<VenueDetails>> GetDetailsByIdAsync(int venueId);
}

public async Task<Option<VenueDetails>> GetDetailsByIdAsync(int venueId) =>
    await repository.GetDetailsByIdAsync(venueId);
```

The implicit conversion in the service implementation translates the provider's nullable row into
the application's explicit `Some(VenueDetails) | None` outcome. A command that requires the venue
instead returns an operation-owned Result such as
`Task<Result<VenueDetails, UpdateVenueError>>`, with `UpdateVenueError.VenueNotFound` as a named
failure.

## Construct Results and Options

Reunion alpha.2 supports target-typed raw payload conversions. Use them when the declared return or
assignment type makes the branch unambiguous:

```csharp
Result<User, LookupUserError> found = user;
Result<User, LookupUserError> failed = new LookupUserError.NotFound();

UnitResult<CreateUserError> completed = new Success();
UnitResult<CreateUserError> rejected = new CreateUserError.EmailInUse();

Option<User> present = user;
Option<User> absent = null;
```

The usual method form is therefore direct:

```csharp
public Option<User> Find(Guid id)
{
    User? user = users.SingleOrDefault(x => x.Id == id);
    return user;
}

public UnitResult<ChangePasswordError> ChangePassword(string password)
{
    if (!IsAllowed(password))
        return new ChangePasswordError.InvalidPassword();

    SetPassword(password);
    return new Success();
}
```

For a target-typed `Option<T>`, prefer `return null;` for `None` and return a nullable value directly
when it already expresses present-or-absent. Do not write `new None()` or `Option.None<T>()` when the
target type already supplies the conversion. Use `ToOption()` when an explicit conversion inside a
larger expression improves the composition or when there is no target-typed conversion site. These
conversions create an Option; there is deliberately no implicit conversion from Option back to
`T?`.

Use named cases when the value and error payload types overlap, when a broad source type would hide
the intended branch, or when the branch itself is the point of the expression:

```csharp
Result<string, string> success = new Success<string>(value);
Result<string, string> failure = new Failure<string>(error);
```

Named cases preserve their branch. An interface-typed or boxed value follows its declared raw
payload conversion; Reunion does not inspect its runtime type to guess a branch. If C# reports
ambiguous operators, make the branch explicit with a named case.

Factories are the universal fallback when there is no target type, type inference is unclear, or a
factory is materially easier to read:

```csharp
var success = Result.Success<object, object>(value);
var failure = Result.Failure<object, object>(error);
```

Do not mechanically expand clear target-typed returns into factories. Do not add local wrapper
factories around Reunion.

Payload-bearing `Some<T>`, `Success<T>`, and `Failure<TError>` cases reject `null`.
`Failure<string>` also rejects empty or whitespace errors. Convert nullable values to Option rather
than forcing null into a payload case. Non-generic `Result` and `ValidationResult` use their named
cases or factories because they have no raw payload conversion that can identify the branch.

`default(Option<T>)` is `None`. Every default Result shape is an invalid, uninitialized union state;
observing or composing it throws `InvalidOperationException`. Never manufacture, return, or treat a
default Result as success or failure.

## Observe and compose

Read payloads through `Match`, `TryGetValue`, or `TryGetError`. There is deliberately no throwing
`Value`, `Error`, or `Unwrap` accessor.

- Use `Match` at a terminal or when both branches produce one expression.
- Use `TryGetValue` or `TryGetError` for a simple guard clause or early return.
- Use `IsSuccess`, `IsFailure`, `IsSome`, or `IsNone` only when the branch matters but its payload
  does not.

Compose operations before their terminal:

- `Map` transforms a success value.
- `Bind` chains a same-error operation and short-circuits failures.
- `MapError` translates an error at an ownership boundary.
- `Ensure` turns a failed success predicate into an owned error.
- `Tap` and `TapError` perform branch-specific side effects without changing the value.
- `OrFailure` turns an Option into a Result when absence becomes a named failure.
- `OrElse`, `ValueOr`, and `ValueOrElse` supply Option fallbacks.
- `Recover` and `RecoverWith` are for an intentional recovery policy, not for hiding faults.

Prefer the Reunion operation that states the caller's policy instead of open-coding or locally
renaming it:

```csharp
Result<VenueDetails, GetVenueError> venue = await venueService
    .GetDetailsByIdAsync(id)
    .OrFailure(new GetVenueError.VenueNotFound());

Task<string> ResolveRedirectAsync(Task<Option<string>> redirect) =>
    redirect.ValueOr("/");
```

`OrFailure` promotes ordinary absence to a named failure at the boundary where it becomes one.
`ValueOr` and `ValueOrElse` make a caller-owned fallback explicit. The task extensions apply these
operations directly to `Task<Option<T>>`; use `MatchAsync`, `MapAsync`, `BindAsync`,
`OrFailureAsync`, `OrElseAsync`, or `ValueOrElseAsync` when the supplied callback is asynchronous.

For collections, `Sequence` converts many same-error Results into one Result, `Traverse` and
`TraverseAsync` map then sequence, and `Combine` collapses unit Results. These operations are
fail-fast; they are not substitutes for validation accumulation.

Use the Reunion task extensions for asynchronous composition and their `Async` variants when the
delegate is asynchronous. Do not insert unnecessary `await` statements merely to unpack and
reconstruct the same carrier. Result and Option also support minimal LINQ query syntax backed by
their fail-fast `Map` and `Bind`; use it only when it makes the chain clearer.

The null-coalescing operator works only on nullable operands and cannot be overloaded, so
`option ?? fallback` does not compile and an implicit conversion cannot make it compile. If a
framework boundary genuinely requires a nullable reference, convert explicitly at that edge:

```csharp
string? redirect = option.Match<string?>(static value => value, static () => null);
```

Prefer keeping a framework-provided nullable value nullable when wrapping and immediately
unwrapping it would add no application outcome. Do not add Concertable-local `ToNullable`,
`GetValueOrDefault`, or fallback helpers; they obscure whether the correct contract was nullable,
Option, or Result.

Ordinary composition is fail-fast. Only validation flows explicitly designed to collect
independent field errors accumulate failures.

Combinators do not catch exceptions. Cancellation, dependency failures, and faults pass through the
exception path unless an infrastructure adapter explicitly normalizes a known dependency condition.

## .NET 11 native unions

Reunion's `net11.0` asset exposes Result and Option as compiler-recognized custom unions. Prefer an
exhaustive switch when both cases drive materially different terminal behavior:

```csharp
return venue switch
{
    Some<VenueDetails>(var details) => Render(details),
    None => NotFound()
};
```

Native union matching improves observation ergonomics; it does not change carrier selection.
Repositories, EF, serialization, and nullable framework APIs still use `T?`, and `??` still does
not apply to Option.

## Own typed application errors by operation

Every `TError` is a closed, operation-owned `XError` union implementing `IError`. Give every
expected outcome a natural named case, including cases without payloads. Callers match those cases;
they do not compare catalog values or parse messages.

Place an error beside its operation according to its widest in-process caller:

- Domain when the entity or aggregate operation owns it;
- Application when callers stay inside the module;
- `*.Contracts` for cross-module callers;
- a published client contract for cross-service client callers.

Do not widen an error union with outcomes that the operation cannot produce. Do not create a shared
error catalog, an `ErrorCase` hierarchy, `NotFound<T>` inheritance, `IErrorSet<T>`, marker interfaces,
or wrapper factories that merely rename a case.

Concertable uses Dunet to declare closed error unions. Disable Dunet's implicit case conversions so
the operation result's branch conversions remain deliberate:

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
    public partial record ProviderFailure(string ProviderCode);
}
```

Derive `Definition` in one exhaustive switch. Do not add a discard/default arm. Dunet suppresses
`CS8509` only when every declared case is covered; an added case must force a deliberate definition
and test update. Use the generated full `Match` only when delegates are the natural API. Use ordinary
C# `is` patterns when logic intentionally inspects only selected cases.

A composite error case forwards the nested definition:

```csharp
[Union(EnableImplicitConversions = false)]
public abstract partial record RefundError : IError
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

Use Dunet only for error-union declarations, exhaustive case handling, and genuinely useful full
matches. Do not use it to implement the Result or Option carrier. Do not expose generated `Unwrap`
or case-specific `MatchX` APIs without a concrete need. Result, validation, errors, transports,
persistence, and wire formats remain independent of Dunet.

## Error definitions and published contracts

Use the direct `Reunion.Errors.ErrorDefinition` generic factories:

- `Invalid<TCase>()`;
- `NotFound<TCase>()`;
- `Conflict<TCase>()`;
- `Unauthenticated<TCase>()`;
- `Forbidden<TCase>()`;
- `PaymentRequired<TCase>()`;
- `Validation<TCase>(errors)`.

Nest each case directly inside its owning `IError` union. The generic factories derive the owner
from the case's immediate declaring type. If a genuinely free-standing error value cannot encode an
owner, use the explicit code-and-message factory overload; do not invent a synthetic owner or local
derivation helper.

Each non-validation factory also has an explicit-message overload. The no-message factory humanizes
the case name, so `PayerNotFound` becomes `Payer not found.`. Supply an explicit safe message when
the case name is not the complete caller-facing text. Never publish exception messages, provider
detail, SQL, stack traces, secrets, or unreviewed identifiers.

Typed error cases never use `[DisplayName]`. Repository entity lookup is a separate concern, where
an entity-oriented helper such as `OrNotFound<TEntity>()` may use the entity type's display name.

The owning error and case names derive the lowercase dot-separated code. Repeated leading owner
words and a trailing `Case` are ignored, while acronyms and digits split naturally. For example,
`EscrowRefundError.EscrowNotFound` publishes `escrow.refund_not_found`.

Use `[ErrorCode("...")]` only to preserve an already-published code when a rename or exceptional
prefix would otherwise change it. The attribute belongs on the case, is not inherited from the
union, and is never decoration added to every case. Never add service-local reflection or code
generation merely to reproduce Reunion's code/message derivation.

Names and definitions must agree. A `PayerNotFound` case uses `NotFound`; an authenticated caller
without permission uses `Forbidden`; missing or invalid identity uses `Unauthenticated`. If the
semantic kind changes, rename the case honestly.

Every error union has an exact definition contract test for every case. Hard-code the expected
code, message, semantic kind, and validation fields. Keep the case inventory explicit; never derive
test expectations with the production helper or runtime reflection. Published codes are never
renamed or reused for a different meaning.

## Structured validation

Validators that produce field errors return `ValidationResult` from `Reunion.Validation`:

```text
ValidationResult = Valid | Invalid(ValidationErrors)
```

`ValidationResult` is distinct from `UnitResult<TError>`. Its invalid payload is always immutable,
non-empty `ValidationErrors`, and its `Combine` operation accumulates independent failures while
preserving field keys and message order. Do not flatten structured field errors into one string.

```csharp
ValidationResult validation = new[]
{
    ValidateName(request.Name),
    ValidateEmail(request.Email)
}.Combine();
```

Validation does not replace the operation's domain error. Map it once at the owning operation
boundary:

```csharp
if (validation.TryGetFailure(
    errors => new CreateUserError.Invalid(errors),
    out var failure))
{
    return failure;
}
```

The union's validation case preserves the payload in its definition:

```csharp
[Union(EnableImplicitConversions = false)]
public abstract partial record CreateUserError : IError
{
    public ErrorDefinition Definition => this switch
    {
        Invalid(var errors) => ErrorDefinition.Validation<Invalid>(errors)
    };

    public partial record Invalid(ValidationErrors Errors);
}
```

Use `Combine` only for independent validations where reporting all field failures is useful.
Business operations, dependency calls, and state transitions remain fail-fast. `ValidationResult`
converts explicitly to the Result family; raw `ValidationErrors` do not implicitly choose a branch.

## HTTP terminals

HTTP projects use `Reunion.AspNetCore` directly. Import exactly one adapter namespace per source
file:

Use `Reunion.AspNetCore.HttpResults` for `TypedResults` and `Results<T1, T2>`. Use
`Reunion.AspNetCore.Mvc` for `ActionResult<T>` and `ActionResult`.

Never import both; identical terminal method names are intentionally ambiguous between programming
models.

Map only at the controller or endpoint boundary:

- `ToOkOrProblem` for value Results;
- `ToNoContentOrProblem` for unit Results;
- `ToCreatedOrProblem` when the normal success is Created;
- `ToActionResult` for custom MVC success mapping;
- `ToResults` for custom typed HTTP-result success mapping;
- `ToOkOr` for Options whose absence maps to a caller-supplied HTTP result;
- `ToOkOrNotFound` and `ToOkOrNoContent` for Options where HTTP owns that absence policy.

Use the projected `ToOkOr` overload when the application value must become a dedicated HTTP
response, and pass controller result methods directly when no extra state is needed:

```csharp
return user.ToOkOr(Unauthorized);
return artist.ToOkOr(value => value.ToDetailsResponse(), NotFound);
```

For `TError : IError`, omit the problem mapper. Reunion maps `Invalid`/`NotFound`/`Conflict`/
`Unauthenticated`/`Forbidden`/`PaymentRequired` to 400/404/409/401/403/402, includes the stable code,
and preserves `ValidationError` as field-indexed `ValidationProblemDetails`. String-error Results
always need an explicit safe problem mapper with an explicit status.

Both HTTP programming models participate in `IProblemDetailsService`, so registered writers,
content negotiation, request instance, trace identifiers, and configured customization remain
consistent. Do not throw an HTTP exception to transport an expected Result failure.

## gRPC boundaries

The wire carries an open string code, published message, and semantic kind; it never carries a
Dunet type or Result. A client maps an application-error `RpcException` to its operation-owned error
with a total `ToXError()` extension.

Keep a private `FrozenDictionary<string, XError>` of reconstructible case instances. Validate code,
message, and kind. Unknown codes and known codes whose message or kind changed throw an operation-
specific contract-mismatch exception containing the original `RpcException`; they never become a
domain `Unknown` case.

```csharp
private static readonly FrozenDictionary<string, PaymentError> errors =
    new PaymentError[]
    {
        new PaymentError.PayerNotFound(),
        new PaymentError.Declined()
    }
    .ToFrozenDictionary(error => error.Definition.Code);
```

Only reconstructible cases belong in the dictionary. A payload-bearing case needs structured wire
detail and an explicit mapper that rebuilds the payload. If the transport does not carry that data,
the case remains in-process. Never discard a payload to force it through code-only lookup.

Catch caller cancellation before application errors and rethrow `OperationCanceledException` with
the caller token. The reusable gRPC cancellation predicate and operation-detail extraction belong
in `Concertable.Grpc`; the concrete case map and mismatch exception stay in the owning client.
Unrelated availability, network, and protocol failures remain their original `RpcException`.

Duplicate codes must fail a mapper contract test. Do not expose public `TryToXError`, nullable
parsers, parser-precedence chains, or runtime assembly discovery. Publish and deploy updated
contracts and clients before a server emits a new code.

## Exceptions, cancellation, workers, and other terminals

An infrastructure adapter may normalize a known provider unavailability or deadline fault into
`DependencyUnavailableException` or `DependencyTimeoutException`, preserving the provider
exception as the inner exception. HTTP maps only those explicit types to safe 503/504 responses.
Broad `HttpRequestException`, `RpcException`, `TimeoutException`, database exceptions, and unknown
faults remain safe 500s.

Cancellation is never normalized into an error Result or handled as an HTTP response. Propagate the
caller token and preserve cancellation semantics.

At worker and gRPC server terminals, match expected typed failures according to the operation's
policy. Leave dependency exceptions on the exception path so retry and dead-letter behavior remains
effective. Do not catch broad exceptions and translate them to a generic domain error.

## Payload and DTO naming

`Result<TValue, TError>` is already the in-process operation wrapper. Its payload is not named
`XResponse` merely because the operation returns it.

- Service and client DTOs use the domain shape name, such as `Transfer`, `Refund`,
  `EscrowDeposit`, or `PaymentOutcome`.
- `Response` is reserved for an HTTP wire model in `Module.Api/Responses` when that wire genuinely
  differs from the application DTO.
- Proto messages retain normal RPC `*Response` names; mappers convert them to suffix-free C# DTOs.

Resolve collisions with aliases rather than wire-flavoured service names.

## Testing and enforcement

Changes to a Result-based operation cover, in proportion to the operation:

- every success, failure, `Some`, and `None` branch;
- the exact definition contract for every error case;
- validation accumulation and field preservation;
- privacy-equivalent branches such as unknown user and bad credential;
- exception and cancellation propagation;
- HTTP status, code, ProblemDetails customization, and validation fields at the terminal;
- gRPC reverse-map completeness, duplicate codes, mismatches, and cancellation precedence;
- architecture rules that prevent legacy or alternate carriers from returning;
- exact package versions and no mixed Reunion graph;
- standalone service-carve restore and build for changed package closures.

Build and test the service against its standalone package closure, not only the monorepo source
graph. A published contract change follows the repository's publish-and-sync cut-over process.

## Do not introduce

- another Result, Option, validation, or application-error carrier;
- `CSharpFunctionalExtensions`, `FluentResults`, `OneOf`, `ErrorOr`, or `LanguageExt` in migrated
  operation contracts;
- legacy `Concertable.Kernel.Functional` types or `Concertable.Shared.Api.Results` terminals in new
  or changed code;
- Result or Option in a wire, event, persistence, or serialized DTO;
- nullable repository contracts changed to Option;
- booleans or enums that collapse caller-actionable outcomes;
- throwing payload accessors or default Results;
- implicit Dunet case conversions;
- broad exception-to-domain-error conversion;
- shared error catalogs, reflection-discovered cases, or wrapper factories;
- `new None()` or `Option.None<T>()` where a target-typed `null` is the clear Option result;
- factory-heavy construction where a raw target-typed payload is unambiguous.
