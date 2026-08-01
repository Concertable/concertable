# Concertable-owned Result and Option migration

> **Status:** Phase 1's revised no-value Result design was completed and verified on 2026-08-01.
> Non-generic `Result`, `Result<TError>`, and accumulating `ValidationErrors` now replace `Unit` and
> every `Result<Unit,TError>` API from the initial implementation.
> Phase 2 remains blocked until the revised branch merges, the Kernel package publishes, and its
> generated platform-sync PR lands green.
>
> **Decision:** Concertable owns status-only `Result`, no-value `Result<TError>`, value-bearing
> `Result<TValue, TError>`, and `Option<T>` in `Concertable.Kernel`. They are stable domain vocabulary,
> not adapters over CSharpFunctionalExtensions, FluentResults, OneOf, Dunet, or a future runtime type.

This is an execution plan for unfinished work. Git history is the archive for the superseded CFE
design.

## Outcome

The backend will use the following vocabulary consistently:

| Situation | Contract |
| --- | --- |
| A value may legitimately be absent and absence has no explanation yet | `Option<T>` |
| An expected operation can succeed or fail for a typed, actionable reason | `Result<TValue, TError>` |
| The same operation has no success payload and has a typed failure | `Result<TError>` |
| Only success/failure status matters and neither case has a payload | `Result` |
| Independent validation rules may all fail | `Result<ValidationErrors>` |
| Both legitimate absence and another expected failure are possible | `Result<Option<T>, TError>` |
| A query returns zero or more values | an empty `IReadOnlyList<T>`, never `Option<IReadOnlyList<T>>` |
| Infrastructure failure, cancellation, or programmer defect | exception/cancelled task |

`Result` and `Option` are in-process .NET contracts. They are not serialized as HTTP, gRPC, database,
or integration-event payloads. Every transport retains an owned wire contract and maps at its service
edge.

## Current reality

The snapshot used for this replan is `origin/main` at `f5d50eb2` on 2026-08-01. The local `main`
checkout is deliberately left untouched apart from this plan; it is behind `origin/main` and also
contains unrelated user changes. None of the concurrent worktrees were modified.

### Landed work that remains valid

The original Phase 1 established useful, library-independent behavior:

- `ErrorKind` provides the transport-neutral categories `Invalid`, `NotFound`, `Conflict`,
  `Unauthenticated`, `Forbidden`, and `PaymentRequired`;
- `ErrorDescriptor` and `ValidationErrorDescriptor` validate stable codes, safe messages, kinds, and
  structured validation errors;
- `IError` gives the shared HTTP layer one error contract to consume;
- `Concertable.Shared.Api` maps those kinds centrally to ProblemDetails and ValidationProblemDetails;
- the global exception path is reserved for genuine faults and maps dependency failures consistently;
- Kernel and Shared.Api tests cover descriptor invariants and each HTTP mapping;
- architecture tests protect the exception handler and shared adapter boundary.

Those semantics stay. The unfinished rename from `Descriptor` to `Definition` is retained because the
type describes an error case rather than an occurrence. The CFE `ResultHttpExtensions`, CFE package
reference, CFE `UnitResult` assumptions, and Dunet test-only API rules are not valid foundation work
and are replaced in Phase 1.

### Current usage inventory

On current `origin/main`:

- FluentResults is referenced by 13 projects, pinned in four service package files, and imported by
  48 production C# files;
- production code has approximately 89 `Result.Fail` calls, 62 `Result.Ok` calls, and 60 `IsFailed`
  checks;
- CSharpFunctionalExtensions is used in production only by the shared HTTP Result adapter;
- Dunet is used only by a Shared.Api test union on `main`;
- no owned or third-party `Option<T>`/`Maybe<T>` is in production;
- current Result code is predominantly branch-and-inspect code, not functional composition;
- published Payment client signatures expose FluentResults, including
  `Result<Transfer?>` and `Result<Refund?>`;
- shared `IReadRepository<TEntity,TKey>.GetByIdAsync` and many service/module lookup contracts expose
  nullable values, while collection contracts often expose `IEnumerable<T>`.

The migration therefore is not just a package substitution. It establishes the composition model,
converts absence into vocabulary that cannot be dereferenced accidentally, and moves operation
meaning into typed errors.

### In-flight branches and PRs

At the snapshot time:

- PR [#284](https://github.com/Concertable/concertable/pull/284),
  `Feature/TypedResultKernelApi`, is open, clean/mergeable, green, and not set to auto-merge. It renames
  ErrorDescriptor to ErrorDefinition, adds error factories, adds CFE to Kernel, and adds a CFE
  `Maybe.OrFailure` extension. Its error-definition work is reusable; its CFE/Dunet architecture is
  not.
- PR [#282](https://github.com/Concertable/concertable/pull/282),
  `Feature/TypedResultMigrationPhase2`, is open, clean/mergeable, green, and not set to auto-merge. It
  contains a valuable Customer Ticket vertical slice and tests, but its public results and optional
  conversion are CFE-based and its progress edits describe the superseded design.
- the `Feature/ResultFoundationComposition` worktree is behind main and has uncommitted CFE
  `MaybeResultExtensions` work. It is an experiment under the rejected design and must remain
  untouched; do not copy, discard, or overwrite it during replanning or Phase 1.
- PR [#286](https://github.com/Concertable/concertable/pull/286), the current platform sync to
  `0.1.0-alpha.0.737`, is red because published Payment client methods are not implemented by B2B
  integration mocks. That live package break must be cleared before starting the new implementation
  sequence.

Recommendation: revise #284 in place into Phase 1 because it is the right isolated foundation branch
and has no dependent merged consumer. Preserve the valid ErrorDefinition work, remove the CFE work,
and make the whole diff subject to fresh review. Hold #282 until Phases 1-2 have published and synced,
then revise it in place as Phase 3; preserve its business classification and tests, but replace every
CFE carrier/conversion and update the plan diff. It does not need to be closed unless rebasing shows
that the retained business diff is no longer reviewable. Do not merge either PR in its current form.

Execution update on 2026-08-01: PR #284 merged concurrently as `14cf7f94` before Phase 1 began, and
its generated platform-sync PR #288 subsequently landed green. Phase 1 was therefore reimplemented
from that current `origin/main` on `Refactor/OwnedResultFoundation`, retaining the valid
ErrorDefinition work and replacing the merged CFE/Maybe foundation. PR #282 and the dirty
`Feature/ResultFoundationComposition` experiment remain untouched.

## Research verdict: owning the types is justified here

Owning foundational types is usually a maintenance cost with little payoff. Concertable is the case
where it is justified:

1. The types will occur in public signatures across every backend service and multiple published
   packages. A third-party carrier becomes part of Concertable's binary and source contract, not an
   internal implementation detail.
2. Concertable needs generic operation-specific errors, a separate optional-value abstraction,
   centralized HTTP translation, package-safe evolution, and strict exception/cancellation behavior.
   No surveyed library matches all of those decisions without imposing its own error model or larger
   programming framework.
3. The required algebra is compact and stable: two cases, construction, observation, composition,
   async lifting, and a few collection operations. It is feasible to own if it is treated as a real
   library with law, invariant, null, default-state, exception, and cancellation tests.
4. Native C# unions can later improve case declarations and compiler pattern matching, but they do
   not remove the need for the `Result`/`Option` operations or Concertable's public semantics.

This is not permission to grow a general functional-programming framework. Add operations only when
they preserve the algebra and have real Concertable use cases.

### Behavioral references, not runtime dependencies

| Reference | What to borrow | Why it is not the public foundation |
| --- | --- | --- |
| [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions) | lazy short-circuit behavior, Result/Maybe combinator edge cases, async test cases | its types would remain in every public signature; `UnitResult` creates a second carrier and a duplicated overload surface |
| [LanguageExt](https://github.com/louthy/language-ext) | mature Option/Either/Validation semantics, laws, null discipline, transformations | it is a broad functional ecosystem rather than a small shared vocabulary and would make its abstractions part of every service contract |
| [OneOf](https://github.com/mcintyre321/OneOf) | fixed alternatives and `Match` requiring one delegate per case | it is a general arity-based union, not a Result/Option algebra; it lacks the required composition contract and leaks case order/types into signatures |
| [ErrorOr](https://github.com/error-or/error-or) | fluent success/failure operations, Ensure/recovery and async API examples | `ErrorOr<T>` owns a list-based error model instead of accepting Concertable's operation-specific `TError` |
| [Ardalis.Result](https://github.com/ardalis/Result) | API-edge mapping and validation examples | its `ResultStatus` vocabulary is oriented toward transport outcomes rather than typed domain errors |
| [FluentResults](https://github.com/altmann/FluentResults) | accumulated validation behavior and migration test cases | its untyped collections of reasons/messages are the ambiguity this migration removes |
| [Optional](https://github.com/nlkl/Optional) and Rust's [Option](https://doc.rust-lang.org/std/option/enum.Option.html) / [Result](https://doc.rust-lang.org/std/result/index.html) | focused Option behavior, `Map`/`Bind`, lazy recovery, fail-fast sequence/traverse | adopting another carrier still creates the same published-package dependency; Rust semantics must be translated honestly to C# defaults and nullability |
| Dunet | temporary source generation for operation error cases | it generates case hierarchy and Match APIs, not the owned Result/Option algebra; generated APIs must not become consumer conventions |

Nick Chapsas's [“The New Option and Result Types of C#”](https://www.youtube.com/watch?v=aksjZkCbIWA)
is useful context for expressing Result and Option as unions. It is not evidence that .NET will ship a
BCL Result abstraction. Current Microsoft documentation says C# 15 unions are a .NET 11 preview
feature, and some proposal features are still unimplemented. The generated union form is currently a
struct backed by `object?`, boxes value cases, gives `default` a null value, and adds neither record
equality nor Result composition. See the official [C# 15 overview](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-15),
[union reference](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/union),
and [language proposal](https://github.com/dotnet/csharplang/blob/main/proposals/unions.md).

## What “monadic” means here

For practical Concertable code, a monad is not magic and does not mean “catch exceptions.” It means:

- construction lifts a value into a context: `Some(value)` or `Success(value)`;
- `Bind` sequences a function that already returns the same context without nesting it;
- `Bind` short-circuits `None` or `Failure`;
- construction and Bind obey left identity, right identity, and associativity;
- `Map` supplies the related functor behavior and obeys identity and composition.

Those laws make pipelines safe to refactor. Kernel tests must exercise them with representative value
and error types. Delegates that throw still throw; tasks that fault or cancel stay faulted or cancelled.
`Try`/exception-capturing effects are explicitly outside this foundation.

LINQ `Select`/`SelectMany` aliases are not in the initial contract. No current call site uses query
syntax, while `Bind` and `Map` provide the actual composition. They can be added compatibly when a real
slice proves the readability benefit.

## Final public API

The types live in `Concertable.Kernel.Functional` and are published by `Concertable.Kernel`:

```csharp
public readonly struct Result : IEquatable<Result>;

public readonly struct Option<T> : IEquatable<Option<T>>
    where T : notnull;

public readonly struct Result<TError> : IEquatable<Result<TError>>
    where TError : notnull;

public readonly struct Result<TValue, TError> : IEquatable<Result<TValue, TError>>
    where TValue : notnull
    where TError : notnull;

```

They are closed two-case tagged values. The public contract is methods and semantics, not storage
fields, generated case classes, or a third-party interface.

### No-success-value representation

Use `Result<TError>` when failure carries a typed reason, and non-generic `Result` when neither case
carries a payload.

| Candidate | Decision |
| --- | --- |
| `Result<Unit, TError>` | rejected: mathematically regular but exposes an implementation token throughout normal application signatures |
| `UnitResult<TError>` | rejected name: communicates the missing payload but gives application code another carrier term |
| `Result<TError>` | selected for no-value operations with a typed failure; its parameter is always `TError` |
| `Result` | selected for the uncommon status-only case where neither success nor failure needs a payload |

The arity is the contract: zero parameters means status only; one means typed failure only; two mean
success value plus typed failure. Do not add a one-parameter value-bearing Result with a universal or
untyped error. Remove public `Unit` once every Phase 1 API and test has moved to the Result arity family.

The existing non-generic static `Result` factory class becomes the non-generic readonly struct. It
continues to host generic `Success` and `Failure` factories alongside its own parameterless factories.
All three arities use explicit named factories; no implicit conversions are added.

Status-only `Result` is available for the uncommon internal case where failure is intentionally just
a control-flow state. It has no HTTP adapter and must not replace `Result<TError>` at a boundary where
the caller needs an actionable reason. Capability queries may still use `bool` when that is their
complete contract.

### Construction and conversions

Expose named factories:

- `Option.Some<T>(T value)` and `Option.None<T>()`;
- `Option.FromNullable<T>(T? value)` overloads for nullable references and nullable value types;
- `Result.Success()` and `Result.Failure()` for status-only outcomes;
- `Result<TError>.Success()` and `Result<TError>.Failure(TError error)`;
- `Result<TValue,TError>.Success(TValue value)` and
  `Result<TValue,TError>.Failure(TError error)`;
- generic factories hosted by non-generic `Result`: `Success<TError>()`,
  `Failure<TError>(TError error)`, `Success<TValue,TError>(TValue value)`, and
  `Failure<TValue,TError>(TError error)`.

Do not add implicit conversions in the initial contract. They hide whether a returned value is the
success or error case, become ambiguous when `TValue` and `TError` are the same type, and make null
construction less visible. Explicit factories cost a few characters and are much easier to search and
review. Adding a carefully justified conversion later is source compatible; removing a leaked
conversion is not.

Do not expose public constructors, mutable setters, public storage/case fields, `Unwrap`, or throwing
`Value`/`Error` properties.

Those are the hand-written Phase 1 rules. Phase 9 intentionally adds the released compiler-generated
case/pattern surface while retaining the factory/combinator API for source-compatible consumers.

### Null semantics

- `Some(null)`, `Success(null)`, and `Failure(null)` are forbidden.
- `where T : notnull` communicates that contract; every factory also performs a runtime null guard
  because nullable warnings can be disabled or suppressed by a consumer.
- `Option<T?>` and `Result<TValue?,TError>` are not valid domain contracts.
- nullable reference/value inputs are converted with `Option.FromNullable`/`ToOption` at the boundary.
- a successful operation with an optional payload is `Result<Option<T>,TError>`.
- wire/ORM APIs that inherently produce null remain nullable through persistence repositories; module,
  application, and client adapters convert before exposing the value to their callers.

### Observation and safe access

`Option<T>` exposes:

- `IsSome`, `IsNone`;
- `Match<TResult>(Func<T,TResult> some, Func<TResult> none)` and an action overload;
- flow-annotated `TryGetValue(out T value)`;
- `ValueOr(T fallback)` and lazy `ValueOrElse(Func<T> fallback)`.

`Result<TValue,TError>` exposes:

- `IsSuccess`, `IsFailure`;
- `Match<TResult>(Func<TValue,TResult> success, Func<TError,TResult> failure)` and an action overload;
- flow-annotated `TryGetValue(out TValue value)` and `TryGetError(out TError error)`.

`Result<TError>` exposes the same two states, Match/action Match without a success value, and
`TryGetError`. Non-generic `Result` exposes the two states and Match/action Match with no payload on
either branch. The method names and selected-branch semantics stay consistent across all arities.

There is deliberately no `Value` or `Error` property that can throw when the wrong case is accessed.
To consume a payload, code must match, compose, or explicitly test a `TryGet` result. C# cannot force a
caller to use any return value at all, so do not claim stronger compiler enforcement than this API
actually provides.

### Default values and representation

Use readonly structs with a private byte tag and payload fields:

- `default(Option<T>)` is `None`; this is useful and matches normal optional-value behavior;
- `default(Result)`, `default(Result<TError>)`, and `default(Result<TValue,TError>)` are invalid,
  uninitialized states, not success or failure;
- every Result state/observation/composition member throws `InvalidOperationException` for an
  uninitialized Result before invoking a delegate;
- `ToString`, equality, and hashing remain total for an uninitialized Result so debugging and
  collections do not throw;
- tests cover arrays, fields, generic defaults, and async paths so an uninitialized Result cannot
  silently become success or failure.

A class does not eliminate invalid defaults—it replaces `default(Result<...>)` with a null reference—and
would add ubiquitous allocation. A tagged struct also most closely matches today's and the proposed
native union representation. The invalid tag is intentional and explicit.

### Equality and formatting

- equality and hashing include the case tag and use `EqualityComparer<T>.Default` for the payload;
- `Success(x)` is never equal to `Failure(x)`, even when value and error types are the same;
- all `None` values of the same closed generic type are equal;
- operators `==` and `!=` agree with `Equals`;
- debugging strings are `Some(value)`, `None`, payload-free `Success`/`Failure`,
  `Success(value)`, `Failure(error)`, and `Uninitialized`;
- formatting is diagnostic only and is never a serialization or error-code contract.

### Synchronous composition

The first public release includes this coherent surface:

| Type | Operations | Required behavior |
| --- | --- | --- |
| `Result<TValue,TError>` | `Map`, `Bind`, `MapError`, `Ensure` | selected branch only; Bind may continue into value-bearing or no-value Result |
| `Result<TError>` | `Bind`, `MapError` | invoke a parameterless continuation on success; continue into any Result arity |
| `Result` | `Bind`, typed `MapError` | invoke a parameterless continuation on success; lazily attach an error when required |
| every Result arity | `Tap`, failure/error tap, recovery, `Match` | preserve the selected case and invoke only its delegate |
| Option | `Map`, `Bind` | selected branch only; never create nested Option through Bind |
| Option | `Match`, `OrElse` | explicit consumption and lazy fallback |
| Option | `OrFailure` | convert `Some` to success and `None` to a lazily created typed failure |

`MapError` is the normal way to lift a lower-level error into an owning operation's error union before
`Bind`. `Recover` is for a genuine successful fallback; it must not be used to erase a failure just to
keep a pipeline moving.

All delegate arguments are null-guarded. A delegate is invoked at most once and only on its selected
case. Exceptions escape unchanged.

### Task-based composition

Provide `ResultTaskExtensions` and `OptionTaskExtensions` so a `Task<Result<...>>` or
`Task<Option<...>>` can be composed without an `await` between every step:

- task-source overloads for synchronous `Match`, `Map`, `Bind`, `MapError`, `Ensure`, `Tap`,
  `TapError`, `Recover`, `RecoverWith`, `OrElse`, and `OrFailure` as applicable;
- `MatchAsync`, `MapAsync`, `BindAsync`, `EnsureAsync`, `TapAsync`, `TapErrorAsync`,
  `RecoverWithAsync`, and Option equivalents for delegates that return `Task`;
- overloads on an already-materialized Result/Option and on a task source where real call sites need
  both, without generating every theoretical sync/async permutation;
- `Task`, not `ValueTask`, in the initial API because current Concertable contracts are task-based.

The extensions never catch. A cancelled/faulted source task stays cancelled/faulted; a delegate's
exception or cancellation propagates; and a delegate on the unselected branch is never invoked.
Cancellation tokens belong to the underlying async operation and are passed by the caller. Pure
combinators do not invent cancellation or override a short-circuited failure/None.

### Collection composition

Add only the collection operations supported by current validators and workflows:

- `Sequence` converts ordered `IEnumerable<Result<T,E>>` to
  `Result<IReadOnlyList<T>,E>` and stops at the first failure;
- `Traverse` maps and sequences in input order, stopping at the first failure;
- `TraverseAsync` runs sequentially in input order, accepts a cancellation token, and stops at the
  first failure or cancellation;
- `Combine` combines `Result<E>` values and returns the first failure.

Do not add parallel traversal until a call site defines ordering, concurrency, and cancellation
semantics. Do not add an Option sequence/traverse surface initially: Concertable collection queries
use empty read-only lists, and no current optional collection pipeline needs it.

Validation accumulation is not Result's fail-fast `Sequence` or `Combine`. Validators collect every
rule violation into one non-empty `ValidationErrors` payload and return
`Result<ValidationErrors>.Failure(errors)`; a valid input returns `Result<ValidationErrors>.Success()`.

`ValidationErrors` is an immutable snapshot of keyed messages. It rejects null/blank keys,
null/blank messages, empty collections, null arrays, and mutable-array aliasing. Multiple messages for
the same key are preserved in rule order. It does not implement `IError`: the owning operation maps it
with `MapError` into an operation-specific error that owns the stable public code, safe message, and
`ErrorKind`.

Repository inventory found three categories that must not be mechanically unified:

- FluentValidation `AbstractValidator<TRequest>` classes validate request shape at the API boundary
  and remain on FluentValidation;
- DI policy validators such as Customer `TicketValidator` and B2B `ApplicationValidator` /
  `ConcertValidator` return `Result<ValidationErrors>`;
- capability queries such as Customer `ReviewValidator` may keep returning `bool` when the caller
  only asks whether an action is available and no explanation is part of the contract.

Overloads that currently mix lookups/authorization with policy validation must be separated during
their vertical slice: absence, forbidden access, and other operation outcomes belong in the owning
typed Result; only accumulated rule violations belong in `ValidationErrors`. Async validators await
their dependencies and then build the Result; dependency faults and cancellation propagate unchanged.

## Repository, module, and application boundaries

### Lookups

Persistence repository methods retain the provider-native nullable contract for a missing row:

```csharp
Task<Concert?> FindByIdAsync(int concertId, CancellationToken ct = default);
```

Module, application, service, and client methods whose only ordinary alternative is “not present”
return `Task<Option<T>>`. This includes the Customer Concert module example:

```csharp
Task<Option<ConcertDto>> GetByIdAsync(int concertId, CancellationToken ct = default);
```

That contract does **not** return Result merely because a caller may later send HTTP 404. The lookup
owner knows presence; the use case owns whether absence means not found, a benign no-op, or another
operation-specific failure:

```csharp
return await concertModule.GetByIdAsync(concertId, ct)
    .OrFailure(() => PurchaseError.ConcertNotFound(concertId));
```

Use `Result<T,E>` at lookup level only when the lookup itself has another expected, typed failure that
the caller can act on. Use `Result<Option<T>,E>` when absence is still legitimate alongside that
failure. Database/network faults and cancellation are not an `E`; they propagate as exceptions.

Final shared repository conventions:

- single-item repository lookups return `Task<TEntity?>`;
- list/query methods return `Task<IReadOnlyList<TEntity>>` and use an empty list;
- scalar repository projections that may be absent retain their nullable provider representation;
- modules and application-facing services convert nullable repository results with `ToOption()`;
- no application, module Contracts, or service/client interface exposes `T?` merely to mean absence.

Repository contracts are not remodeled as part of this migration. Each vertical slice converts
nullable repository results at its module or application boundary.

### Use cases and validators

- application services, module operations, commands, and queries return operation-specific typed
  Result when they have expected refusal/failure outcomes;
- pure reads with no expected outcome beyond absence may return Option directly;
- validators used by application workflows return `Result<ValidationErrors>` when they accumulate
  policy violations;
- FluentValidation remains appropriate for request-shape validation at API boundaries and is not
  mechanically wrapped in Result;
- background/event handlers return Result only when their caller has meaningful typed policy handling;
  retryable delivery, database, transport, and invariant failures remain exceptions.

### Error ownership

Use operation-specific errors by default: `PurchaseTicketError`, `CancelConcertError`,
`ReleaseEscrowError`. Reuse an error type only when the same owner, meaning, payload, and response
policy are genuinely shared. Do not create a service-wide mega-union merely to reduce declarations.

The shared error roles are:

- `IError` has one member: `ErrorDefinition Definition { get; }`;
- `ErrorDefinition` contains a stable code, safe message, and `ErrorKind` and validates all three;
- `ValidationErrorDefinition` additionally contains a non-empty field/message map;
- `ErrorKind` is a coarse, transport-neutral policy category; it is not an HTTP status enum;
- each operation error case owns its dynamic data and returns its definition;
- error codes are public API contracts; exception messages and third-party strings are not.

Keep named `ErrorDefinition.Invalid/NotFound/...` factories where they improve construction.
`ErrorDefinition.NotFound<T>(code)` may derive the standard message from an explicit `[DisplayName]`;
types without that metadata use the explicit-message overload, and CLR type names are never a
fallback. The operation still owns its stable code.

### HTTP and other transports

Controllers consume the shared `Concertable.Shared.Api` adapters:

- `Result<T,TError> where TError : IError` maps success through a caller-provided/default success
  result and failure through one centralized Definition-to-ProblemDetails mapper;
- `Result<TError> where TError : IError` maps success without inventing a value body;
- the status-only non-generic Result has no HTTP adapter because a public failure must have a reason;
- the error mapper is generic over `TError : IError`, avoiding conversion of a future struct union to
  an interface receiver;
- the frozen mapping owns the HTTP status for every ErrorKind and titles come from the centralized
  `HttpStatusCode.ToReasonPhrase()` helper; do not override them in controllers;
- ordinary definitions create `ProblemDetails`; validation definitions create the concrete
  `ValidationProblemDetails` type and preserve its keyed errors;
- both paths retain the shared ProblemDetails service customization, `application/problem+json`,
  request instance, trace id, safe detail, stable code, and fallback JSON writer behavior;
- controllers do not switch on operation error cases or duplicate status mapping;
- Option is converted to an owning typed Result before the controller boundary;
- uninitialized Results and null delegates/definitions fail explicitly rather than generating an HTTP
  response;
- exception middleware remains the sole terminal for infrastructure failures, cancellation policy,
  and programmer defects; Result adapters never catch or route exceptions through typed failures.

gRPC, HTTP clients, and integration-event adapters use explicit wire error codes/details and rebuild
the receiving side's typed error. Kernel Result/Option and Dunet/native union runtime layouts never go
on the wire.

## Typed error unions: separate from Result/Option

Dunet currently contributes only source-generated case records, implicit case conversions, and Match
helpers. It does not provide Result/Option, and it is not required by Kernel.

Retain Dunet temporarily in the application/contract projects that need closed operation errors,
under these rules:

- Result/Option never reference or wrap Dunet;
- operation code constructs errors through owned named factories;
- `IError.Definition` is the stable consumer-facing behavior;
- generated `Unwrap`, case-specific Match helpers, async Match helpers, and implicit conversions are
  not application conventions;
- generated full `Match` may be used inside the union declaration or an owner-local mapper, where a
  new case changes the generated method signature and exposes missed handling at compile time;
- consumers map through owned error factories/Definition and do not publish generated Match APIs as
  a required cross-package programming model;
- no global warning suppression, global warning-as-error change, or claim about ordinary C# switch
  exhaustiveness is introduced to compensate for Dunet.

`IError` is permanent native-union infrastructure, not temporary scaffolding. Native unions close the
cases of one operation; they do not give unrelated operation unions a shared member that generic HTTP
translation can call. Every operation error union implements `IError` and computes its Definition with
an owner-local exhaustive switch:

```csharp
public union PurchaseError(
    ConcertNotFound,
    PurchaseInvalid,
    PaymentRejected) : IError
{
    public ErrorDefinition Definition => this switch
    {
        ConcertNotFound => ErrorDefinition.NotFound(...),
        PurchaseInvalid invalid => ErrorDefinition.Validation(..., invalid.Errors),
        PaymentRejected rejected => ErrorDefinition.PaymentRequired(...),
    };
}
```

`IError` lets Shared.Api retain one generic `where TError : IError` terminal for all independently
owned unions. Removing it would require controller-local switches or one cross-service mega-union,
both of which are rejected. Keep `ErrorKind` as the small payload-free transport-policy enum and keep
ErrorDefinition/ValidationErrorDefinition as validated mapping values; operation alternatives are the
native unions.

Make the Definition-to-ProblemDetails helper generic (`ToProblemActionResult<TError>` with
`where TError : IError`) so a value-type native union uses a constrained interface call rather than
being converted to an `IError` receiver and boxed. The frozen status mapping, centralized reason
phrases, and validation specialization remain unchanged.

### Mandatory native-union destination

The hand-written structs are advance implementations of the native unions, not permanent competing
abstractions. As soon as Concertable adopts the released .NET 11 SDK and stable C# union feature,
change every owned discriminated type to a `union` declaration in the same upgrade workstream:

- non-generic `Result`, `Result<TError>`, and `Result<TValue,TError>`;
- `Option<T>`;
- operation-specific error unions then generated by Dunet or hand-written as closed cases.

The intended stable shapes are success/failure case wrappers for Result and a Some case plus native
null/default handling for Option. The exact declarations must be verified against the released syntax,
but the target is equivalent to:

```csharp
public partial union Result(ResultSuccess, ResultFailure);
public partial union Result<TError>(ResultSuccess, ResultFailure<TError>);
public partial union Result<TValue, TError>(ResultSuccess<TValue>, ResultFailure<TError>);
public partial union Option<T>(Some<T>);
```

Distinct Result case wrappers are required: using `TValue` and `TError` directly would be ambiguous
for valid types such as `Result<string,string>`. `default(Option<T>)` continues to mean None through
the union's null/default case; `default(Result...)` continues to be uninitialized and every operational
member rejects it. Case constructors retain the no-null rules.

Factories, `Match`, `Map`, `Bind`, task/collection composition, equality, hashing, formatting, and HTTP
adapters are the stable application contract. Keep those methods when the declarations become unions,
so existing consumers do not change. The native switch-pattern surface is added on cutover day and may
be adopted incrementally; it does not force service call-site rewrites.

As of the Microsoft documentation updated 2026-07-27, C# 15 unions remain a .NET 11 preview and some
proposal features are still unimplemented. The current generated form is a struct backed by `object?`;
it boxes value cases, exposes `Value`, adds implicit case conversions, and supplies neither Result
composition nor record equality. Native union is therefore not currently a performance optimization
over the hand-written byte-tagged structs. Do not move production to the preview SDK merely to use the
keyword. Use the stable hand-written API now, then accept and test the released union representation as
the required language cutover.

The cutover is intentionally localized: rewrite Kernel declarations/storage and direct invariant tests;
retain task extensions, collection extensions, Shared.Api adapters, validator contracts, and service
call sites. Because these types never cross wire or persistence boundaries, there is no data migration.
Changing a published struct's binary shape still requires a Kernel package publication and a full
platform sync/recompile. Benchmark the released representation to record its cost, but performance does
not turn the required keyword migration back into an optional decision.

## Ordered implementation phases

Every listed implementation phase/subphase is one independently reviewable PR unless its package
publication creates a generated platform-sync PR, which is part of that phase's ownership. Before
each phase, refresh `origin/main`, open PRs, and platform-sync state. A completed and verified phase is
a hard stop under `plans/AGENTS.md`.

### Phase 1 — owned Kernel functional foundation and shared adapters — complete

The initial implementation on `Refactor/OwnedResultFoundation` passed 181/181 Kernel tests, 40/40
Shared.Api tests, and the Release solution build with zero errors on 2026-08-01. Before delivery, the
no-value design was rejected. Revise the same unmerged branch to add non-generic `Result` and
`Result<TError>`, add immutable `ValidationErrors`, remove `Unit` and every `Result<Unit,TError>` API,
then repeat the complete Phase 1 test/build gate. Phase 2 must not begin before the revised package is
merged, published, and platform-synced green.

Completed on 2026-08-01: the revised implementation passed 215/215 Kernel tests, 45/45 Shared.Api
tests, and `dotnet build api/Concertable.slnx --configuration Release` with zero errors. Local E2E was
not run because this remains isolated, behavior-preserving foundation work.

**Dependency:** continue in the clean
`C:\Users\TommySeery\source\repos\Concertable.worktrees\OwnedResultFoundation` worktree on
`Refactor/OwnedResultFoundation`. Recheck current `origin/main` and confirm no open red platform-sync
PR before implementation; do not touch PR #282 or the dirty ResultFoundationComposition experiment.

**Scope and expected projects/files:**

- `api/Concertable.Shared/src/Concertable.Kernel/Functional/`: provide `Option<T>`, all three Result
  arities, factories, synchronous combinators, task extensions, nullable conversions, and collection
  extensions; remove `Unit`;
- `api/Concertable.Shared/src/Concertable.Kernel/Errors/Error.cs`: complete
  ErrorDescriptor/ValidationErrorDescriptor -> ErrorDefinition/ValidationErrorDefinition and
  `IError.Definition`; retain validation invariants and the explicit `[DisplayName]`-based generic
  NotFound factory;
- `api/Concertable.Shared/src/Concertable.Kernel/Concertable.Kernel.csproj`: no CFE, FluentResults,
  Dunet, or OneOf dependency may support the new types;
- `api/Concertable.Shared/src/Concertable.Shared.Api/Results/ResultHttpExtensions.cs` and error/exception
  mapping: consume the owned value-bearing and no-value Result arities; make the internal error mapper
  generic over `TError : IError` so future struct unions use a constrained call rather than boxing;
  store the frozen status mapping and derive titles with `HttpStatusCode.ToReasonPhrase()`; create concrete `ProblemDetails` and
  `ValidationProblemDetails` terminals while preserving the existing serialized policy;
- `api/Concertable.Shared/Directory.Packages.props` and affected csproj files: remove the CFE runtime
  dependency and remove Dunet from the test-only foundation if no production union needs it yet;
- `api/Concertable.Shared/tests/Concertable.Kernel.UnitTests`: add focused tests for every API member;
- `api/Concertable.Shared/tests/Concertable.Shared.Api.UnitTests`: update mapping and architecture tests;
- architecture docs and this plan: document the owned boundary and mark only this phase complete.

**Kernel test matrix:**

- every factory, case property, Match/action Match, TryGet, and fallback;
- null delegates/payloads and nullable reference/value conversions;
- Option default-as-None and every Result operation on an uninitialized default;
- equality, hash code, operators, and formatting for every case/default of all Result arities and
  Option;
- selected-branch invocation count, laziness, short-circuiting, and thrown delegate propagation for
  every combinator;
- Result and Option functor identity/composition and monad left/right identity/associativity;
- async success/failure/None, faulted source, cancelled source, thrown delegate, faulted/cancelled
  delegate task, and unselected async delegate;
- Sequence/Traverse order, empty input, first-failure short circuit, and traversal cancellation;
- every factory, case, combinator, task form, default, equality, formatting, laziness, and propagation
  rule for non-generic `Result` and `Result<TError>`;
- Bind interoperability from status-only to typed-error/value Results, from typed-error to
  value-bearing Results, and from value-bearing to no-value Results;
- `ValidationErrors` non-empty/key/message invariants, accumulation order, duplicate keys, defensive
  copying, equality, hashing, and mapping into operation-specific errors;
- Shared.Api architecture/reflection coverage proving the common terminal remains generic over
  `TError : IError` and does not accept an `IError` receiver;
- every ErrorKind's exact status and title, complete enum coverage, concrete ProblemDetails type,
  validation error dictionary, code/detail/instance/trace/content type, ProblemDetails customization,
  fallback writer, null definition/delegate, uninitialized Result, and exception/cancellation boundary.

**Verification:**

- Kernel and Shared.Api unit tests;
- `dotnet build api/Concertable.slnx --configuration Release` with zero errors;
- no local E2E: this is isolated, behavior-preserving foundation work with exhaustive unit coverage;
  label the PR `skip-e2e`;
- after merge, own package publication and the generated platform-sync PR through green/merged before
  Phase 2. Confirm all standalone service carves still consume packages rather than cross-service
  project references.

### Phase 2 — Payment owned-result expansion

**Dependency:** Phase 1 package published and platform sync merged. This phase must not begin on a
red platform pin.

**Scope and expected projects/files:**

- `api/Concertable.Payment/src/Concertable.Payment.Application`: define operation-specific errors and
  migrate application services/manager interfaces to owned Result/Option;
- `api/Concertable.Payment/src/Concertable.Payment.Domain`: return typed transition refusals while
  invariant defects remain exceptions;
- `api/Concertable.Payment/src/Concertable.Payment.Infrastructure`: classify Stripe caller-actionable
  decline/rejection as typed failure; preserve network, authentication, rate-limit/server,
  cancellation, and unknown faults as exceptions; remove catch-all Result conversion;
- `api/Concertable.Payment/src/Concertable.Payment.Client` and `Protos/payment.proto`: add cleanly named,
  additive typed client operations and structured wire error details. Keep old FluentResults members
  as explicit compatibility adapters over the new behavior; do not add `V2` names as the final API;
- retain wire compatibility for deployed clients and continue populating legacy status detail during
  the expansion window;
- replace nullable release/refund success payloads with the domain-selected
  `Result<Option<Transfer>,E>`/`Result<Option<Refund>,E>` or `Result<E>` form. Confirm the benign
  no-op semantics before selecting one;
- update Payment unit/integration tests and B2B/Customer client mocks for the additive surface;
- publish Payment Contracts/Client and own platform sync through green.

**Verification:**

- Payment unit tests for every case, Stripe classification, gRPC status/detail round trip, legacy
  adapter parity, exception propagation, and cancellation;
- Payment integration tests plus B2B/Customer integration fixtures against the new client surface;
- full Release solution build and every standalone carve;
- API E2E is justified because payment/gRPC behavior and compatibility are high risk; let the merge
  queue run the API payment/escrow paths. UI E2E is not independently justified by the transport
  expansion.

### Phase 3 — revise PR #282: Customer Ticket vertical slice

**Dependency:** additive Payment typed client package from Phase 2 is synced.

**Scope and expected projects/files:**

- rebase/rebuild `Feature/TypedResultMigrationPhase2` from current main without copying its CFE package
  additions;
- Customer Concert `IConcertModule.GetByIdAsync` and implementation return `Task<Option<ConcertDto>>`;
- Ticket repositories retain nullable single-item lookup results; module lookups convert them to
  Option, and collection queries use read-only lists;
- `PurchaseError` and `CheckoutError` remain operation-specific but implement the finalized
  `IError.Definition` contract and obey the Dunet containment rules;
- `ITicketService.PurchaseAsync`/`CheckoutAsync` use owned Result and compose with `OrFailure`, Bind,
  MapError, and the new Payment typed client;
- `ITicketValidator` policy methods return `Result<ValidationErrors>` and preserve accumulation of all
  current validation messages; lookup-owning overloads move not-found into the use-case Result;
- controllers use shared HTTP adapters; event completion/retryable faults remain exceptions;
- remove FluentResults and CFE from the migrated Ticket/Concert contracts where no remaining local
  use requires them;
- preserve and update #282's exact-case, cancellation, infrastructure-fault, and ProblemDetails tests.

**Verification:**

- Customer Ticket and Concert unit tests;
- Customer integration tests for found/missing Option, validation, purchase/checkout, typed Payment
  mapping, stable ProblemDetails codes, exceptions, and cancellation;
- full Release solution build;
- API E2E ticket purchase/payment is justified for this behavior-changing vertical slice. UI E2E is
  required only if visible response behavior or the UI contract changes.

### Phase 4 — B2B Concert validation and lifecycle core

**Dependency:** Phases 1-2 synced; may follow Phase 3 to keep migration learning linear.

**Scope and expected projects/files:**

- `Concertable.B2B.Concert.Application/Domain/Infrastructure/Api/Contracts`: keep repository
  single-item lookups nullable, convert module/application lookups to Option, and migrate operation
  methods to owned Result;
- define errors for apply, accept/reject policy, withdraw, draft creation, and lifecycle transition;
- change `LifecycleStateMachine.Next` and `ILifecycleTransitioner` to typed composition without
  catch/rethrow;
- migrate dispatcher/executor/capability interfaces as complete vertical slices;
- convert private FluentResults policy validators to `Result<ValidationErrors>`, separating lookup
  and authorization outcomes from accumulated policy violations;
- preserve keyed deal-strategy resolution; do not introduce DealType switches or service location;
- update controllers, module facades, workers, mocks, and all deal-type tests.

**Verification:**

- Concert lifecycle/state-machine/workflow unit tests;
- B2B Concert integration tests for each migrated operation and every keyed deal type;
- B2B architecture tests and full Release solution build;
- API E2E is justified because lifecycle behavior spans modules and deal strategies; UI E2E only for
  changed user-visible contracts.

### Phase 5 — B2B Concert payment/cancel/finish workflows

**Dependency:** Phase 4 plus the Phase 2 typed Payment surface.

**Scope and expected projects/files:**

- migrate `IConcertWorkflowModule`, cancellation/completion dispatchers, executors, and every keyed
  cancel/finish/accept/payment step to owned typed Results;
- compose Payment failures with `MapError`; no string/message bridge and no
  `BadRequestException(result.Errors)`;
- remove catch-all conversions in Cancel/Finish executors;
- make `ConcertCompletionRunner` distinguish expected deferral/refusal from retryable faults;
- update HTTP/worker terminal handling and every strategy-specific mock/test;
- remove FluentResults from Concert projects once their last local use is gone.

**Verification:**

- Concert workflow tests for every keyed strategy;
- B2B Concert and Payment integration coverage for cancel, finish, capture, escrow, refund, release,
  payout, and settlement;
- B2B worker tests and full Release solution build;
- API E2E for all payment workflow variants is required; UI E2E only for changed UI contracts.

### Phase 6A — B2B Tenant outcomes and lookups

**Dependency:** Phase 5.

**Scope and expected projects/files:**

- `api/Concertable.B2B/src/Modules/Tenant/{Application,Contracts,Domain,Infrastructure,Api}`:
  invitation, membership, tenant, tax-compliance, and current-tenant operations;
- retain nullable Tenant repository single-item contracts, convert module/application lookups to
  Option, and use read-only lists for collections;
- make expected not-found, conflict, invalid, and forbidden outcomes operation-specific Results;
- classify “not found immediately after this operation saved it” as an invariant/fault, not a normal
  NotFound Result;
- keep framework authorization and infrastructure/cancellation paths exceptional.

**Verification:**

- `Concertable.B2B.Tenant.UnitTests` and `Concertable.B2B.Tenant.IntegrationTests`;
- B2B architecture tests and full Release solution build;
- API E2E is justified for invitation/membership authorization flows; UI E2E only if their response
  contract changes.

### Phase 6B — B2B Venue and Artist outcomes and lookups

**Dependency:** Phase 6A.

**Scope and expected projects/files:**

- `api/Concertable.B2B/src/Modules/Venue/{Application,Contracts,Domain,Infrastructure,Api}` and the
  parallel Artist projects;
- migrate create/update/ownership operations to operation-specific Results;
- convert current-tenant IDs/details and public single-item queries from nullable values to Option;
- keep public list/search queries as empty `IReadOnlyList<T>` values;
- preserve existing keyed strategy and ownership boundaries.

**Verification:**

- Venue unit/integration tests and Artist unit/integration tests;
- B2B architecture tests and full Release solution build;
- API E2E for create/update/ownership behavior is justified; UI E2E only for a changed UI contract.

### Phase 6C — B2B Deal, User, and Conversations outcomes

**Dependency:** Phase 6B.

**Scope and expected projects/files:**

- `api/Concertable.B2B/src/Modules/Deal`, `Modules/User`, and `Modules/Conversations` across their
  Application/Contracts/Domain/Infrastructure/Api projects;
- migrate nullable Deal/User module lookups to Option and multi-ID queries to read-only lists;
- replace remaining expected exceptions with operation-specific Results;
- keep message delivery, authentication middleware, infrastructure failure, cancellation, and
  programmer defects exceptional;
- remove any module-local legacy Result dependency when its last use is gone.

**Verification:**

- Deal and Conversations unit tests plus Deal/User/Conversations integration tests;
- B2B architecture tests and full Release solution build;
- API E2E only for behavior-changing deal/user/conversation flows; a mechanical lookup-contract PR
  with complete integration coverage uses `skip-e2e`.

### Phase 7A — Customer Review, Preference, and User outcomes

**Dependency:** Phase 6 establishes the final patterns; this remains Customer-owned and introduces no
cross-service project reference.

**Scope and expected projects/files:**

- `api/Concertable.Customer/src/Modules/Review`, `Modules/Preference`, and `Modules/User` across
  Application/Contracts/Domain/Infrastructure/Api projects;
- migrate Review/Preference command refusals to typed Results; retain nullable repository lookups and
  convert absence to Option at module/application boundaries;
- classify `GetMe`/authenticated-user absence explicitly as expected NotFound/Unauthenticated or an
  invariant fault according to the existing endpoint contract;
- convert multi-user queries to empty read-only lists and keep infrastructure/cancellation exceptional.

**Verification:**

- Review and User unit/integration tests plus Preference coverage added for each changed contract;
- Customer architecture tests where present and full Release solution build;
- API E2E for changed Review/Preference/User behavior; UI E2E only for changed UI responses.

### Phase 7B — Customer Venue, Artist, and remaining Concert/Ticket contracts

**Dependency:** Phase 7A and the Phase 3 Ticket pattern.

**Scope and expected projects/files:**

- `api/Concertable.Customer/src/Modules/Venue`, `Modules/Artist`, and remaining
  `Modules/Concert`/`Modules/Ticket` Application/Contracts/Domain/Infrastructure/Api projects;
- convert all remaining module/application single-item reads to Option and collection reads to
  read-only lists while repository lookups remain nullable;
- migrate caller-actionable ticket availability and concert mutation outcomes to typed Results;
- keep cross-service data flow through existing Contracts/events and synchronous Payment adapter
  boundaries from `api/ARCHITECTURE.md`.

**Verification:**

- Customer Concert/Ticket/Artist/Venue unit and integration tests as affected;
- full Release solution build and Customer standalone carve;
- API E2E for ticket/concert behavior changes; UI E2E only for a changed visible contract.

### Phase 7C — Kernel value objects, Messaging, and background paths

**Dependency:** Phase 7B.

**Scope and expected projects/files:**

- `api/Concertable.Shared/src/Concertable.Kernel/ValueObjects` and their call sites: use typed creation
  only where invalid input is an expected caller outcome; do not mechanically wrap constructors;
- `api/Concertable.Messaging/{Application,Domain,Infrastructure,AzureServiceBus}`: use Result only for
  caller-actionable policy refusal; delivery, outbox persistence, and transport failures remain
  exceptions for retry/dead-letter behavior;
- B2B/Customer worker and event-handler paths: distinguish expected deferral from retryable faults;
- replace custom null/whitespace guard helpers with BCL guards where the old Result dependency is the
  only reason they remain, and rename the surviving invariant exception consistently.

**Verification:**

- Kernel and both Messaging unit suites, plus affected B2B/Customer worker tests;
- full Release solution build and all affected standalone carves;
- API/UI E2E is not justified for isolated value-object refactors; run API E2E only if a background
  cross-service behavior changes, otherwise label `skip-e2e`.

### Phase 8 — published contract cleanup and enforcement

**Dependency:** every consumer uses the additive Payment owned contracts and the owned Kernel
functional types.

**Scope and expected projects/files:**

- remove legacy Payment FluentResults interfaces/methods, wire compatibility fields that have passed
  the agreed compatibility window, and their adapters; publish and own platform sync;
- remove remaining FluentResults/CSharpFunctionalExtensions package pins/references/usings and Kernel
  `ErrorExtensions`/legacy exception helpers once no longer used;
- retain Dunet only for actual operation union declarations; remove its test-only/shared placement and
  enforce the containment rules;
- add architecture checks for no third-party Result/Maybe/Option in public signatures, no nullable
  module/application/service/client single-item lookup contracts, no Option-wrapped collections, no
  Result on wire DTOs, and no controller-local typed-error-to-status switches;
- run a final inventory for `FluentResults`, `CSharpFunctionalExtensions`, `Maybe`, third-party
  `Result`, nullable non-persistence lookup signatures, `IError`, ErrorDefinition, and generated
  Dunet APIs;
- retain this plan until the mandatory native-union cutover is complete.

**Verification:**

- all Kernel/shared/service unit and integration tests;
- full Release solution build and all standalone carves after each published cleanup/sync;
- full API E2E is justified for the final cross-service contract removal; run UI E2E only if API
  behavior/shape changed rather than solely internal type signatures.

### Phase 9 — .NET 11 native-union cutover

**Dependency:** .NET 11 and C# unions are released and Concertable is ready to move its backend SDK
and target framework together. Begin this phase immediately when that production upgrade is available;
do not leave the owned discriminated types on hand-written tags after the platform supports the stable
union declarations.

**Scope and expected projects/files:**

- upgrade the backend SDK/toolchain and target frameworks to the released .NET 11/C# version through
  the repository's normal platform-upgrade workstream;
- change every owned `Result`, `Option`, and operation-error discriminated declaration to `union`;
- replace Dunet-generated operation errors with native unions and remove Dunet after the last use;
- retain `IError` as the common generic capability implemented by each operation union, with each
  union's `Definition` implemented through a compiler-checked exhaustive case switch;
- use distinct guarded success/failure/some case types so equal `TValue`/`TError` types remain valid and
  null payloads remain impossible;
- preserve factories, combinators, task/collection extensions, HTTP adapters, error definitions,
  equality, hashing, formatting, and exception/cancellation semantics;
- preserve default Option as semantic None and default Result as explicitly uninitialized in every
  operational member, accounting for the released union's native null/default pattern;
- add native exhaustive-pattern tests for every union and a compile-time fixture that fails when a new
  case is not handled;
- benchmark the released native representation against the final hand-written tagged structs and
  record the result; optimize case representation without reverting the required union declarations;
- publish the Kernel and affected contract packages, own the generated platform-sync PR through green,
  rebuild all consumers, and delete this plan in the final verified cutover commit.

**Verification:**

- all Kernel/shared/service unit and integration tests;
- native pattern, null/default, same-type case, equality, hashing, formatting, exception, cancellation,
  serialization-boundary, and binary/package-consumer tests;
- full Release solution build and every standalone carve;
- full API E2E through the merge queue because the SDK upgrade and package-wide binary cutover are
  cross-cutting; run UI E2E if the repository-wide .NET 11 upgrade changes hosted runtime behavior.

## Package and compatibility rules

For every published Kernel, Payment, or Contracts change:

1. expand additively when old and new consumers must coexist;
2. merge/publish from the owning project;
3. wait for the generated platform-sync PR;
4. migrate every consumer and its mocks/tests on the new pin;
5. verify the whole solution and standalone carves;
6. remove the compatibility path only after repository-wide proof that no consumer remains;
7. publish/sync the cleanup and do not leave a red platform PR behind.

Never solve package ordering with a cross-service ProjectReference or `UseLocalCore` in committed
code. `UseLocalCore=true` remains a local diagnostic option only.

## Risks and decision gates

| Risk | Mitigation / gate |
| --- | --- |
| An owned core becomes an under-tested private library | Phase 1 cannot publish without the complete invariant, law, branch, null, default, async, exception, and cancellation matrix |
| The API grows into an unmaintainable overload catalogue | require a real Concertable call site and consistent algebra before adding an overload; add operations compatibly |
| Result structs silently treat default as a valid case | reserve tag zero, fail every operational access, and test default through arrays/fields/tasks |
| Option becomes nullable with new syntax | forbid null payloads at generic and runtime boundaries and expose no throwing Value property |
| Error unions leak Dunet and block native migration | keep generated matching owner-local; public composition depends on Result/IError/factories, not generator APIs |
| Native-union expectations move during previews | keep production on net10/C#14 now; make factories/combinators the stable consumer API, then execute mandatory Phase 9 from the released specification |
| Published package changes strand services | use expand/publish/sync/consumer/cleanup and treat every generated sync as part of its owning phase |
| Typed Result swallows operational failures | no catch in combinators; explicit tests prove infrastructure faults and cancellation propagate |

No design decision blocks Phase 1. Tommy's input is genuinely required later for the existing Payment
`ReleaseByBookingIdAsync`/`RefundByBookingIdAsync` null-success semantics: confirm whether “nothing was
released/refunded” is a benign `None`, a successful no-value/idempotent no-op, or an operation failure.
That decision changes the Phase 2 public contract and must be made from product/idempotency semantics,
not inferred from the present `Result<T?>` shape.

## Resume prompt — review Phase 1 revision

```text
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\OwnedResultFoundation

Run /code-review for Refactor/OwnedResultFoundation's complete Phase 1 diff against origin/main.
Read plans/TYPED_RESULT_MIGRATION.md and the required AGENTS/architecture files first. Review the
owned Result/Option foundation, ValidationErrors, Shared.Api terminals, native-union compatibility,
and expanded tests. Do not begin Phase 2; it remains blocked until this branch merges, Kernel
publishes, and the generated platform-sync PR lands green.
```
