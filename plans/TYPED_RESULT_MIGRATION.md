# Typed Result migration — explicit expected failures, exceptions for faults

> **Status:** in progress; the Phase 1 producer core foundation is complete and the published-package
> consumer sync remains outstanding.
>
> **Decision:** adopt `CSharpFunctionalExtensions` for `Result<TValue, TError>` and
> `UnitResult<TError>`, use Dunet for project-owned closed error unions on .NET 10, and replace
> those Dunet declarations with native C# unions after the repository moves to stable .NET 11.
> Expected, caller-actionable failures travel as typed Results. Cancellation, programming defects,
> broken invariants, and infrastructure faults remain exceptions.

This is not a library-swap plan. It replaces three contradictory failure models with one rule and
then migrates complete use-case slices so no method mixes expected exceptions, stringly Results,
and typed Results.

## Why do this

The top three reasons are specific to Concertable:

1. **The existing contracts lie about their failures.** `TicketService.PurchaseAsync` returns a
   Result but throws for concert absence and validation, while returning payment failure. Payment
   returns a Result internally, throws an `RpcException` at gRPC, then reconstructs a string failure
   in its client. A caller cannot know which failures are in the signature.
2. **Failure identity is already being destroyed.** `CancelExecutor` and `FinishExecutor` catch
   every exception and return `Result.Fail(ex.Message)`. Domain rejection, a SQL outage, a null bug,
   cancellation, and a Stripe transport fault become the same string. That prevents correct HTTP,
   gRPC, retry, logging, and worker decisions.
3. **Independent deployment makes explicit contracts more valuable, not less.** Payment failures
   cross a published client package and a gRPC wire. Stable typed error codes let Payment evolve
   without leaking HTTP exceptions or treating a domain rejection as gRPC `Internal`.

The goal is not “no exceptions.” The goal is that a method signature truthfully describes all
normal outcomes, while faults still use the runtime’s native exception and observability path.

## The rule developers apply

Use this decision table. It is the non-arbitrary seam.

| Situation | Representation |
|---|---|
| A command/use case can be refused during normal operation and its immediate caller can choose a response | `Result<TValue, TError>` |
| The same, but success has no value | `UnitResult<TError>` |
| A query’s absence is itself ordinary data and the caller naturally branches on it | nullable value or the existing query shape |
| Repository lookup is used by a command where absence means that command failed | repository stays nullable; the application/use-case layer converts once to its typed error |
| Request-shape or aggregate policy validation | FluentValidation, or the existing private `FluentResults.Result` policy validator |
| Cancellation, timeout, unavailable dependency, database/serialization failure, programming bug, or violated internal invariant | exception |
| HTTP, gRPC, controller, worker, or message-handler edge | terminally match a Result or centrally handle an exception |

Additional mechanical rules:

- Once a use-case contract returns a typed Result, **every expected failure on that call path remains
  a typed Result until a terminal adapter**. Intermediate layers may `Bind`, `Map`, or `MapError`;
  they may not unwrap and throw an HTTP exception.
- A layer catches an exception only when it can actually recover, compensate, retry, or add context
  and rethrow. It never catches `Exception` to manufacture a failed Result.
- Domain methods return a typed Result when rejecting the requested transition is a normal domain
  outcome. Domain methods throw when reaching the condition means the caller or persisted state
  violated an invariant that should already have been guaranteed.
- Transport names and status codes do not appear in Domain or Application error cases.
- Results do not cross HTTP, protobuf, integration-event, or persisted-data boundaries. Each
  transport adapts the semantic error contract.
- `OperationCanceledException` is never converted to a failed Result.

This is partial adoption with a principled boundary: a **complete use-case slice**, not a whole
layer and not whichever method a developer happens to be editing.

## Chosen libraries and ownership

Use the current stable packages verified during this investigation:

- `CSharpFunctionalExtensions` `3.7.0` for the outer Result carrier and composition API.
- `Dunet` `1.16.2` for closed error unions and exhaustive matching on .NET 10.
- Keep `FluentResults` only for the three private aggregate policy validators while they benefit
  from collecting multiple messages. It must disappear from service, client, domain-operation,
  controller, and shared exception contracts.

Relevant upstream references:

- [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions)
- [Dunet shared properties and exhaustive unions](https://github.com/domn1995/dunet)
- [C# 15 union types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union)

Do not introduce:

- a hand-written Result carrier;
- `ErrorOr`, `OneOf`, Ardalis Result, or a second operation-result abstraction;
- `CSharpFunctionalExtensions.HttpResults` as an application dependency. It still requires an exact
  mapper for each custom error type, whereas Concertable needs one shared semantic-category mapper
  across MVC and its own gRPC contract. Its extra source generators do not remove Concertable’s
  transport decisions;
- string-based `Result<T>` or `Result.Fail("...")` for application/service outcomes;
- a Dunet union around success and failure. Dunet owns `TError`; CSharpFunctionalExtensions owns the
  outer binary Result.

### Naming

The union type uses an `Error` suffix because it is the `TError` contract:

```csharp
Task<Result<TicketPayment, PurchaseError>> PurchaseAsync(...);
```

Cases do not repeat the suffix:

```csharp
[Union]
internal partial record PurchaseError
{
    internal partial record ConcertNotFound(int ConcertId);
    internal partial record Validation(IReadOnlyList<string> Messages);
    internal partial record PaymentRejected(string Code, string Message);
}
```

Use `UnitResult<CancelConcertError>` for a typed command with no success payload. The earlier rejected
`UnitResult<ValidationErrors>` shape is still rejected: validators return their normal aggregate
validation result; the application use case maps that aggregate into a `Validation` case only when
constructing its public operation failure.

## Shared platform foundation

Concertable’s shared code should remove transport repetition without becoming a catalog of every
service’s business errors.

### `Concertable.Kernel` owns the common semantic contract

Add:

```csharp
public enum ErrorKind
{
    Invalid,
    NotFound,
    Conflict,
    Unauthenticated,
    Forbidden,
    PaymentRequired
}

public record ErrorDescriptor(
    string Code,
    string Message,
    ErrorKind Kind);

public sealed record ValidationErrorDescriptor(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]> Errors)
    : ErrorDescriptor(Code, Message, ErrorKind.Invalid);

public interface IError
{
    ErrorDescriptor Descriptor { get; }
}
```

The exact names can be adjusted during Phase 1, but the shape is fixed:

- `Code` is stable and machine-readable, for example `ticket.concert_not_found`.
- `Message` is safe to return to a caller.
- `Kind` is transport-neutral semantic classification.
- structured validation failures use `ValidationErrorDescriptor`; ordinary descriptors do not carry
  a validation member.
- descriptor construction rejects blank/malformed codes, blank safe messages, unknown kinds, and
  empty validation collections as programming defects;
- codes use at least two lowercase dot-separated segments, with the owning operation/module as the
  prefix, and are never renamed or reused after publication;
- safe messages are authored explicitly for callers and never copied from an exception, provider,
  SQL response, stack trace, or unreviewed external value.

Each service or module owns its error unions. It exposes one exhaustive `Descriptor` match on the
union. That is the single unavoidable place where business cases acquire stable codes and public
messages. Adding a new union case then fails the build until its descriptor is supplied.

```csharp
[Union]
internal partial record PurchaseError : IError
{
    partial record ConcertNotFound(int ConcertId);
    partial record Validation(IReadOnlyList<string> Messages);
    partial record PaymentRejected(string PaymentCode, string PaymentMessage);

    public ErrorDescriptor Descriptor => Match<ErrorDescriptor>(
        notFound => new ErrorDescriptor(
            "ticket.concert_not_found",
            $"Concert {notFound.ConcertId} was not found.",
            ErrorKind.NotFound),
        validation => new ValidationErrorDescriptor(
            "ticket.purchase_invalid",
            "The ticket purchase is invalid.",
            new Dictionary<string, string[]> { ["purchase"] = validation.Messages.ToArray() }),
        paymentRejected => new ErrorDescriptor(
            paymentRejected.PaymentCode,
            paymentRejected.PaymentMessage,
            ErrorKind.PaymentRequired));
}
```

The union match answers “what does this business case mean?” once. The shared adapters answer “how
does that semantic kind appear in HTTP or gRPC?” once. No controller, service, or client repeats the
status mapping.

Do **not** put `PurchaseError`, `ConcertWorkflowError`, `PaymentError`, generic entity-not-found
cases, or a global mega-union in Kernel. Those are the union of service concerns, not the shared
intersection.

### `Concertable.Shared.Api` owns HTTP adaptation

Move the common web behavior here:

- one generic Result-to-MVC `ToActionResult` seam that owns the CFE `Match`, plus convenience
  helpers for `Ok`, `Created`, and `NoContent`;
- one `IError` to RFC `ProblemDetails` mapper;
- one shared `IExceptionHandler` for faults and the few framework/authentication exceptions that
  legitimately reach the edge.

The semantic status mapping exists once:

| `ErrorKind` | HTTP |
|---|---:|
| `Invalid` | 400 |
| `NotFound` | 404 |
| `Conflict` | 409 |
| `Unauthenticated` | 401 |
| `Forbidden` | 403 |
| `PaymentRequired` | 402 |

`ProblemDetails.Extensions["code"]` carries the stable code and
`ProblemDetails.Extensions["errors"]` carries structured validation errors.

Expected 4xx Results are not logged as unhandled exceptions. Unexpected exceptions are logged once
and return a generic production-safe 500. Development may include diagnostic detail. The handler
must not return raw exception messages in production.

Result failures and exceptions execute through the same `IProblemDetailsService` path. That path
sets the request path as `instance`, adds `traceId` from the current Activity or HTTP trace
identifier, supplies the exception only for exception-originated problems, and then delegates
serialization, content negotiation, registered writers, and
`ProblemDetailsOptions.CustomizeProblemDetails` to ASP.NET Core. No MVC `ObjectResult` serializer
bypasses those hooks.

Known infrastructure status mapping uses explicit shared exception types rather than guessing from
provider exceptions:

| Exception | HTTP | Safe code |
|---|---:|---|
| `DependencyUnavailableException` | 503 | `dependency.unavailable` |
| `DependencyTimeoutException` | 504 | `dependency.timeout` |

An infrastructure adapter may normalize a provider-specific availability/deadline exception into
one of those types and retain the original as `InnerException`. Raw `HttpRequestException`,
`RpcException`, `TimeoutException`, SQL/provider exceptions, defects, invariants, and unknown faults
remain generic safe 500s. `OperationCanceledException` always passes through. This keeps
infrastructure failures on the exception/observability path without over-classifying broad runtime
types or converting them into Results.

The four duplicated handlers in B2B, Customer, Payment, and Search are replaced by this one shared
implementation. During migration it may retain compatibility mappings for the legacy
`DomainException`/`HttpException` hierarchy, but those branches are deleted with the hierarchy in
the final phase. Its permanent role is the production-safe terminal 500 boundary for unexpected
faults; cancellation passes through and is never logged or converted to ProblemDetails.

### Placement of operation errors

| Error is visible to | Owner |
|---|---|
| One implementation method only | Infrastructure, next to the implementation |
| A module Application interface | that module’s Application project |
| A public cross-module facade | that module’s `*.Contracts` project |
| A published cross-service client | owning service’s `*.Contracts` project |
| All services and transports | only `IError`, `ErrorDescriptor`, `ValidationErrorDescriptor`, and `ErrorKind` in Kernel |

Dunet is referenced only by projects declaring unions. CSharpFunctionalExtensions is referenced by
projects declaring or consuming operation Result signatures. Because every service has an
independent `Directory.Packages.props`, the version pin must appear in each service that uses it;
centralizing that pin above the service roots would break the standalone carve and is not an
acceptable “deduplication.”

## Transport ownership

### HTTP

Controllers terminally adapt their Application result:

```csharp
var result = await ticketService.PurchaseAsync(parameters, ct);
return result.ToOkActionResult();
```

The Result helper uses CFE `Match` to choose the caller-supplied success result or
`IError.ToProblemActionResult`. That error extension uses `IError.Descriptor`, the single frozen
`ErrorKind`-to-status table, and ASP.NET Core reason phrases to create an
`ApplicationErrorResult`. The custom MVC result and the shared exception handler both delegate to
the same `IProblemDetailsService` writer policy described above. Controllers do not switch on error
cases or status codes, receive `ControllerBase` as an adapter argument, or expose `ProblemDetails`;
error unions contain no status codes or MVC types.

### gRPC

Payment owns both sides of its gRPC boundary:

1. Payment Application returns typed CFE Results.
2. The Payment gRPC service matches a failed Result and sends a non-OK gRPC status plus structured
   error detail containing the stable code, semantic kind, safe message, and optional metadata.
3. The published Payment client adapter catches only those known application-error statuses,
   decodes the structured detail, and reconstructs the public Payment error union.
4. `Unavailable` and `DeadlineExceeded` remain exceptions and may be normalized to
   `DependencyUnavailableException`/`DependencyTimeoutException` for transport-independent HTTP and
   worker policy. Cancellation, malformed responses, `Internal`, and other unrecognized faults stay
   their original exceptions.

Use a protobuf error-detail message in `payment.proto` (or the standard Google rich-error model if
the implementation spike confirms the package fits the carve). Preserve the existing status detail
during the expansion so old clients continue to receive their current message while new clients use
the structured code.

The gRPC status mapping is semantic:

| `ErrorKind` | gRPC |
|---|---|
| `Invalid` | `InvalidArgument` |
| `NotFound` | `NotFound` |
| `Conflict` | `FailedPrecondition` |
| `Unauthenticated` | `Unauthenticated` |
| `Forbidden` | `PermissionDenied` |
| `PaymentRequired` | `FailedPrecondition` |

`GrpcExceptionInterceptor` no longer knows about `HttpException`. Expected errors have already been
mapped by the service method. The interceptor passes through existing `RpcException`, lets
cancellation propagate, logs true faults, and returns a production-safe `Internal` without leaking
`ex.Message`.

This fixes the current `EscrowEntity` problem: a normal escrow transition rejection becomes a typed
Payment failure and a known non-500 gRPC status. A genuine broken invariant remains an exception and
correctly crosses as `Internal`.

### Integration events and workers

Results are not serialized into events. An event handler or scheduled worker is a terminal caller
and must make an explicit policy decision:

- idempotent/already-completed result: acknowledge;
- expected business deferral: log at the appropriate level and continue;
- `DependencyUnavailableException`, `DependencyTimeoutException`, and other transient dependency or
  consistency faults: throw for retry;
- poison input: use the messaging system’s existing dead-letter policy.

Do not turn an exception into a string failure merely so a loop can continue. If a batch must isolate
items, catch and log the exception per item while preserving the exception path for retry/health
semantics defined by that worker.

## Current blast radius

The inventory was taken from `api/` on 29 July 2026.

### Result/package footprint

- 44 production C# files directly import FluentResults:
  - B2B Concert: 17
  - Customer Ticket: 4
  - Payment: 21
  - Shared Kernel: 2
- 8 test/mocking files directly import FluentResults.
- 13 projects reference the package.
- 4 independent `Directory.Packages.props` files pin FluentResults.
- There are about 90 explicit Result signature/member references and 40 production
  `IsFailed`/`IsSuccess` branch points.

### Legacy exception footprint

There are 157 matched application call sites that either throw an HTTP-oriented exception or invoke
`.OrNotFound()`:

| Area | Matched sites |
|---|---:|
| B2B Concert | 92 |
| B2B Tenant | 18 |
| Payment | 18 |
| B2B Venue | 7 |
| B2B Artist | 6 |
| Customer Ticket | 6 |
| B2B User | 2 |
| B2B Deal | 2 |
| Customer Preference | 2 |
| Customer Review | 2 |
| B2B Conversations | 1 |
| Customer User | 1 |

There are 56 `.OrNotFound()` calls alone, including 39 in B2B Concert. Repositories are not all
converted to Results; these calls are classified at their owning command/query boundary.

DomainException appears in shared value objects and guards, B2B Artist/Venue/Tenant/Deal/Concert,
Customer Concert/Review, Payment Escrow, and Messaging Outbox. These sites need classification, not
a mechanical replacement.

### Cross-service published surface

`Concertable.Payment.Client` publicly exposes FluentResults through:

- `ICustomerPaymentClient`
- `IEscrowClient`
- `IManagerPaymentClient`

B2B and Customer consume that package at the platform pin, and their integration fixtures mock the
same interfaces. Changing these signatures is a breaking published-package cutover even though the
protobuf request/response success messages remain compatible.

### Areas with little or no Result work

- Search has no application Result surface found; it only consumes the duplicated exception handler.
- Auth uses its authentication/protocol framework and has no matching Result/HttpException migration
  surface. Do not force framework authentication exchanges into this application Result model.
- Messaging only needs its DomainException invariant classification.

## Virality, quantified

Result is deliberately “viral” across the part of a call graph that promises recoverable outcomes.
The plan limits that colour to use-case slices and uses one operation error type through the slice.

### Customer ticket purchase

Today, five contracts in the path already carry FluentResults:

1. `ITicketService`
2. `ICustomerPaymentClient`
3. Payment `ICustomerPaymentService`
4. `IPaymentManager`
5. the Stripe payment client

The refactor does not newly colour five layers; it makes their existing colour truthful. The changes
are:

- `TicketService` converts the nullable concert once to `PurchaseError.ConcertNotFound`.
- Its private policy validator remains an aggregate validation Result and is mapped once to
  `PurchaseError.Validation`.
- Payment failure is `MapError`-translated once from the public Payment error union to
  `PurchaseError.PaymentRejected`.
- The gRPC server and client adapter each perform one terminal transport conversion.
- Repository and pure mapper signatures remain unchanged.

An expected domain mutation at the leaf adds one typed Result only where that mutation can normally
be rejected. Infrastructure faults skip the Result track as exceptions.

### B2B concert cancellation

Today only `ICancellationDispatcher` and `ICancelExecutor` return Result. The public module immediately
throws on failure, while the executor catches every exception and flattens it.

The final cancellation slice adds typed Result contracts at three meaningful seams:

1. `IConcertWorkflowModule.CancelAsync`
2. `ILifecycleTransitioner`
3. the per-deal `ICancelStep`

The controller, module, dispatcher, executor, transitioner, and keyed per-deal step then share one
`CancelConcertError` vocabulary. The lifecycle transitioner should be generic over the operation
error so it can compose a lifecycle rejection without forcing one giant Concert error union:

```csharp
Task<Result<ApplicationEntity, TError>> TransitionAsync<TError>(
    int applicationId,
    Trigger trigger,
    Func<LifecycleError, TError> mapLifecycleError,
    Func<ApplicationEntity, Task<UnitResult<TError>>>? effect = null);
```

The exact API may be simplified during implementation, but it must preserve these properties:

- invalid lifecycle transitions are values, not `ConflictException`;
- an effect can short-circuit without throwing;
- unexpected exceptions and cancellation are untouched;
- each operation retains its exact error union;
- keyed strategy resolvers and per-deal polymorphism remain intact.

Three additional coloured contracts across a six-layer state machine is a real cost. It is also the
compiler-visible representation of three existing failure exits. The rejected alternative is less
code only because those exits are hidden in exceptions or erased into strings.

## The three collision sites after the migration

### `TicketService.PurchaseAsync`

It returns `Result<TicketPayment, PurchaseError>`. Concert absence, business-policy validation, and a
known payment rejection all return cases of `PurchaseError`. It throws none of the legacy
`HttpException` types. Database, gRPC availability, cancellation, malformed Payment responses, and
bugs still throw.

`CompleteAsync` is classified separately. It is called from a `PaymentSucceededEvent`, not an
interactive purchase caller. If the referenced concert cannot exist, that is a consistency/retry
fault, not `PurchaseError.ConcertNotFound`; it should throw and let the message policy retry or
dead-letter. Sibling methods do not share a Result merely because they live on one service.

### `CancelExecutor.ExecuteAsync`

It returns `UnitResult<CancelConcertError>`, contains no catch-all, and composes typed repository,
lifecycle, per-deal, and payment failures. `CancelConcertError` reaches the controller or background
terminal unchanged. Faults remain exceptions.

### `GrpcExceptionInterceptor`

It handles exceptions only. Payment gRPC service methods map typed application failures before the
interceptor. `HttpException` disappears from the interceptor, and `DomainException` cannot
accidentally become a generic 500 for an expected transition because expected domain transitions no
longer throw it.

## DomainException and its guards

Do not keep the current ambiguous meaning where `DomainException` sometimes means invalid user input,
sometimes a normal state rejection, and sometimes a programming invariant, while the HTTP handler
maps all of them to 400.

Classify each site:

- **Normal state rejection:** return a typed domain Result. Initial targets are
  `EscrowEntity.Release/Refund/MarkDisputed`, invitation accept/revoke/expire, ticket availability,
  and lifecycle transitions.
- **Externally supplied value can be invalid:** validate at the Application boundary and use a typed
  factory/mutation Result where Domain is the authoritative validator.
- **Programmer precondition:** use BCL guards
  `ArgumentNullException.ThrowIfNull` and `ArgumentException.ThrowIfNullOrWhiteSpace`.
- **Impossible internal invariant:** throw a renamed `DomainInvariantException` (or
  `InvalidOperationException` where no domain-specific diagnostic value exists). It maps to 500, not
  400.

Delete `DomainException.ThrowIfNull` and `ThrowIfNullOrWhiteSpace`; .NET already owns those
precondition guards. Rename or replace the remaining exception only after every site is classified,
so no expected 400 silently becomes a 500.

Examples:

- Money currency mismatch is an invariant exception.
- Outbox “record failure after dispatched” is an invariant exception.
- “Only held escrow can be released” is a normal operation rejection and becomes a typed Result.
- Invalid invitation state/expiry is a normal invitation-command rejection and becomes a typed
  Result.
- Invalid ticket quantity supplied by an API is validated before mutation; a direct invalid internal
  call is a programming defect.

## Migration phases

Each phase is a hard stop after its commit and verification. Every phase builds
`api/Concertable.slnx` with zero errors and runs affected unit and integration suites through the
repository’s test skills. Full E2E is reserved for the cross-service cutover and final behaviorally
risky gates as required by `plans/AGENTS.md`.

### Phase 1 — additive shared foundation and handler consolidation

Progress:

- [x] Producer core foundation: Kernel contracts, CFE adapters, test-only Dunet integration proof,
  shared fault handler, architecture enforcement, unit tests, and conventions.
- [ ] Publish the Shared packages, then replace the four service-local handlers and run the
  cross-service ProblemDetails smoke gate in the generated platform-sync PR.

Scope:

- add `ErrorKind`, `ErrorDescriptor`, `ValidationErrorDescriptor`, and `IError` to Kernel;
- add CSharpFunctionalExtensions `3.7.0` to Shared package management and
  `Concertable.Shared.Api`;
- pin Dunet `1.16.2` in Shared package management for the test project that proves the complete
  union-to-descriptor-to-CFE-to-HTTP path; Shared production projects do not reference Dunet;
- add generic MVC Result adapters and their unit tests;
- move the common `IExceptionHandler` into `Concertable.Shared.Api`;
- replace the four service-local handler registrations/files with the shared handler;
- preserve legacy `DomainException`/`HttpException` mappings temporarily so this phase is
  behavior-preserving;
- update `api/agents/CODE_CONVENTIONS.md` with the Result rules, naming, transport rule, and
  no-catch-to-failure rule;
- add architecture/grep tests that prohibit HTTP exception types in newly migrated typed-result
  slices where practical;
- enforce descriptor invariants and the Shared production boundary against Dunet/business unions;
- route Result and exception ProblemDetails through one ASP.NET Core writer/customization policy;
- define explicit dependency-unavailable/deadline exceptions and safe HTTP 503/504 mappings.

The foundation architecture tests intentionally enforce only rules that can be strict now: Shared
production declares no business union or Dunet dependency, and a typed-result source file cannot use
the legacy HTTP exception vocabulary. Broader banned-API/analyzer enforcement remains in Phase 8,
after legacy call sites are gone; adding it now would require a large transitional allowlist that
would weaken the final gate.

Package gate:

- this changes published Shared packages, so merge, wait for package publication, and follow the
  platform-sync PR to green before Phase 2.

Verification:

- Kernel and Shared.Api unit tests;
- B2B, Customer, Payment, and Search integration smoke coverage for existing ProblemDetails;
- whole solution build.

### Phase 2 — Customer Ticket vertical slice

Scope:

- add CSharpFunctionalExtensions and Dunet pins to Customer package management;
- define `PurchaseError` and `CheckoutError` in Ticket Application;
- migrate `ITicketService.PurchaseAsync` and `CheckoutAsync` to typed CFE Results;
- keep `ITicketValidator`’s aggregate FluentResults contract private and map it once;
- replace `.OrNotFound()` with explicit nullable-to-error conversion in the use case;
- adapt the current legacy Payment string failure once to a typed Ticket payment-rejection case;
- replace manual `IsFailed` controller branches with shared HTTP adapters;
- reclassify `CompleteAsync` and `TicketPaymentProcessor` as an event-processing path, preserving
  retryable faults as exceptions;
- update mocks and Ticket unit/integration tests to assert exact error cases and ProblemDetails
  codes.

Clean answers produced:

- `PurchaseAsync` has no expected throw path.
- `CompleteAsync` does not pretend event consistency failure is an interactive business result.
- Ticket controllers contain no hand-written error/status switch.

Verification:

- Customer Ticket unit tests;
- Customer integration tests covering purchase/checkout validation and not-found responses;
- whole solution build.

### Phase 3 — B2B Concert validation and lifecycle core

Scope:

- add CSharpFunctionalExtensions and Dunet pins to B2B package management;
- define operation-specific errors for apply, accept, reject, withdraw, draft creation, and lifecycle
  transition;
- change `LifecycleStateMachine.Next` from `ConflictException` to a typed lifecycle result;
- make `ILifecycleTransitioner` compose an operation error without catch/rethrow;
- migrate the dispatcher/executor/capability interfaces for these operations as complete slices;
- keep keyed deal-strategy resolution; do not replace it with `DealType` switches;
- convert private FluentResults policy-validator failures once into the owning operation’s
  `Validation` case;
- replace relevant `BadRequestException`, `ConflictException`, `ForbiddenException`, and
  `.OrNotFound()` calls;
- update controllers, module facades, workers, mocks, and tests.

Verification:

- Concert lifecycle state-machine and workflow unit tests;
- B2B Concert application/deal-type integration tests;
- whole solution build.

### Phase 4 — B2B Concert cancel, finish, and payment workflow slices

Scope:

- migrate `IConcertWorkflowModule`, cancellation/completion dispatchers, executors, and per-deal
  cancel/finish steps to typed Results;
- remove catch-all conversion from `CancelExecutor` and `FinishExecutor`;
- map current legacy Payment-client failures into `CancelConcertError`,
  `FinishConcertError`, `AcceptConcertError`, or the exact owning operation at the Payment boundary;
- migrate escrow/capture/refund/release/payout steps without throwing
  `BadRequestException(result.Errors)`;
- make `ConcertCompletionRunner` explicitly distinguish business deferral/refusal from unexpected
  faults;
- update HTTP and worker terminal behavior and all deal-type tests.

Clean answer produced:

- no exception is flattened to a Result string anywhere in the Concert workflow;
- no typed failure is unwrapped into an HTTP exception in `ConcertWorkflowModule`.

Verification:

- Concert workflow unit tests for every keyed deal strategy;
- B2B Concert integration tests for cancel, finish, escrow, refund, release, and settlement;
- B2B worker unit tests;
- whole solution build.

### Phase 5 — Payment typed Results and gRPC/package cutover

Scope:

- add CSharpFunctionalExtensions and Dunet pins to Payment package management;
- define public operation-specific Payment error unions in `Concertable.Payment.Contracts`;
- migrate Payment Application interfaces, services, `IPaymentManager`, and Stripe adapters to
  `Result<TValue, TError>`/`UnitResult<TError>`;
- replace nullable release/refund command outcomes with `UnitResult<TError>` or explicit success
  unions while preserving benign no-op behavior;
- return typed transition failures from `EscrowEntity`;
- classify Stripe exceptions: caller-actionable decline/rejection becomes a typed failure; network,
  authentication, rate-limit/server, cancellation, and unknown faults remain exceptions;
- remove every `catch (Exception) => Result.Fail(...)` path;
- add structured gRPC error detail and server/client mappings;
- reduce `GrpcExceptionInterceptor` to true fault handling;
- change the three published Payment client interfaces from FluentResults to typed CFE Results;
- update Payment unit tests to assert exact cases, stable codes, gRPC statuses, structured
  round-trips, and fault passthrough.

Published-package cutover:

1. The Payment producer change lands while existing deployed/packaged clients remain wire-compatible.
   Rich error detail is additive and the old status detail remains populated.
2. `publish-packages` publishes the new `Payment.Contracts` and `Payment.Client`.
3. The resulting `chore/platform-sync-*` PR is part of this phase, not an afterthought. Update B2B,
   Customer, their mocks, and tests from the old Payment result members to the new typed errors.
   Their owning typed use-case errors already exist from Phases 2 and 4, so this is a direct
   `MapError`, not a new compatibility wrapper.
4. Build `api/Concertable.slnx`, run Payment unit tests, B2B/Customer integration, and the API E2E
   payment/ticket/escrow/settlement paths before the sync PR is considered green.

The phase is not complete until the platform-sync PR is merged and no service remains pinned to the
old public Result signature.

### Phase 6 — remaining B2B application failures

Scope:

- migrate Tenant invitation/membership/tenant commands;
- migrate Venue and Artist create/update/ownership commands;
- migrate remaining Deal, Conversations, and User expected failures;
- classify “not found after save” as an invariant/infrastructure exception, not a `NotFound` Result;
- convert ownership/permission refusals to typed `Forbidden` cases;
- convert domain creation/transition rejections to typed domain Results where normal;
- preserve nullable query contracts where absence is ordinary data.

This phase removes the non-Concert B2B legacy footprint: Artist (6 matched sites), Tenant (18), Venue
(7), Deal (2), User (2), and Conversations (1).

Verification:

- affected module unit tests;
- all affected B2B module integration tests;
- architecture tests and whole solution build.

### Phase 7 — remaining Customer, shared value objects, and Messaging

Scope:

- migrate Customer Review and Preference command failures;
- classify Customer User absence/authentication behavior;
- convert Customer Concert ticket-availability mutations to typed Results where caller-actionable;
- audit Review creation and shared `DateRange`, `Address`, and `EmailAddress` construction paths so
  expected invalid input is validated before domain construction;
- classify Money and Messaging Outbox conditions as normal rejection versus invariant exception;
- replace all custom null/whitespace guards with BCL guards;
- rename the remaining true invariant exception to `DomainInvariantException`.

Verification:

- affected Customer, Kernel, and Messaging unit tests;
- Customer integration tests;
- whole solution build.

### Phase 8 — contract cleanup and enforcement

Scope:

- delete `HttpException` and its seven subclasses;
- delete `.OrNotFound()` and all exception constructors/extensions tied to FluentResults;
- delete `ErrorExtensions` once no controller consumes FluentResults errors;
- remove FluentResults from Payment, service/client Result contracts, Kernel, and stale project
  references;
- retain FluentResults only in the three private aggregate policy validators if it still materially
  improves them; otherwise replace those final validators with their existing FluentValidation
  result shape and remove FluentResults completely;
- remove compatibility branches for `HttpException` and old `DomainException` from the shared
  handler;
- add banned-API/analyzer or architecture-test enforcement for:
  - HTTP exception types outside the terminal web adapter;
  - stringly application Results;
  - catch-all exception-to-Result conversion;
  - direct CFE/Dunet types in HTTP DTOs, protobuf messages, integration events, and persistence;
  - a discard arm in matches over project error unions unless null/default genuinely requires it;
- update all conventions and remove completed compatibility notes.

Definition-of-done grep gates:

```text
rg -n "using FluentResults" api
```

Returns only the explicitly retained validator whitelist, or zero.

```text
rg -n "HttpException|BadRequestException|NotFoundException|ConflictException|ForbiddenException|PaymentRequiredException|InternalServerException|\\.OrNotFound\\(" api
```

Returns zero production usages and zero deleted definitions.

```text
rg -n "Result\\.Fail\\(|Result\\.Failure\\(" api
```

Returns no stringly application/service failures and no exception-message conversion.

```text
rg -n "catch \\(Exception.*Result|Result.*ex\\.Message" api
```

Returns zero.

Verification:

- whole solution build;
- all unit and integration suites;
- API E2E for ticket purchase, every B2B deal payment workflow, escrow release/refund, and
  settlement;
- final architecture test suite.

## First three implementation files

Phase 1 starts with these files because every later error contract depends on them:

1. `api/Concertable.Shared/src/Concertable.Kernel/Errors/Error.cs` — introduce the shared
   semantic interface, descriptor, and category without any service-specific cases.
2. `api/Concertable.Shared/src/Concertable.Shared.Api/Results/ResultHttpExtensions.cs` — prove one
   generic typed Result can map to consistent ProblemDetails without per-controller switches.
3. `api/Concertable.Shared/src/Concertable.Shared.Api/Exceptions/GlobalExceptionHandler.cs` —
   consolidate the four handlers and establish that only faults reach exception handling in the
   final model.

Package-management and project-reference edits accompany these files in the same phase; the list is
the conceptual implementation order, not a claim that those three files alone compile.

## Tests that must be added, not merely rewritten

- Every error union has a test that each case exposes the intended stable descriptor.
- Kernel descriptor tests reject malformed codes, unsafe empty messages, unknown kinds, and empty
  validation failures.
- Shared HTTP mapping has one parameterized test per `ErrorKind`, plus structured validation and
  production-safe 500 tests. Result and exception execution tests prove the shared writer,
  customization, instance, trace identifier, and exception context policy.
- Explicit dependency exceptions map to safe 503/504 responses, while an unclassified timeout stays
  a safe 500.
- Ticket purchase proves not-found, aggregate validation, and Payment rejection are typed failures;
  gRPC unavailable remains an exception.
- Cancel workflow proves repository absence, invalid lifecycle transition, unsupported deal
  capability, and Payment refusal stay distinguishable through every dispatcher/executor layer.
- Cancel/finish fault tests prove SQL/unknown exceptions and cancellation are not converted to
  Results.
- Payment gRPC tests round-trip every public Payment error code through server status detail and the
  published client adapter.
- Payment gRPC tests prove `Internal`, `Unavailable`, `DeadlineExceeded`, cancellation, malformed
  details, and unknown codes remain exceptions.
- Stripe tests separate card/business rejection from network/authentication/server faults.
- Background worker tests assert acknowledge/defer/retry behavior for typed failures versus
  exceptions.
- Architecture tests enforce that service-specific unions remain in their owner and cross-service
  references remain `*.Contracts`/published-client only.

## Risks and controls

| Risk | Control |
|---|---|
| A global error union grows into an unmaintainable pseudo-exception hierarchy | Error unions are operation-owned; Kernel contains only descriptor semantics. |
| Result colouring creates boilerplate through the Concert state machine | One operation error type flows through the slice; CFE `Bind`/`MapError` compose it; no per-layer wrapper errors. |
| Errors gain HTTP concerns to avoid mapping code | Error cases expose semantic descriptors only; Shared.Api owns HTTP and Payment owns gRPC. |
| General Stripe/SQL/gRPC faults are mislabeled as expected payment failures | Catch only documented caller-actionable conditions; rethrow cancellation and all unknown/transient faults. |
| The Payment client signature breaks independently deployed consumers | Treat Phase 5 as a producer publish plus mandatory platform-sync consumer cutover; preserve wire compatibility throughout. |
| Dunet becomes permanent accidental infrastructure | Error unions are project-owned, Dunet is declaration-only, and the .NET 11 replacement is isolated to those declarations plus published-package sync. |
| Keeping FluentResults for validators causes ambiguous `Result` imports | FluentResults is allowed only in validator files/interfaces; operation files use CFE. Avoid files importing both. |
| DomainException cleanup changes 400s into 500s accidentally | Classify every site before changing the handler; expected invalid input gains a typed Result and tests first. |
| Controllers or workers silently ignore a new error case | Dunet/native-union exhaustive matches; no discard arm for error cases. |

## .NET 11 follow-up

Do not target a preview runtime for this refactor. Native unions are available in .NET 11 previews,
but the repository should switch only after stable .NET 11/C# 15 and its toolchain are adopted.

At that point:

1. Replace each Dunet `[Union] partial record XError` declaration with a native `union XError(...)`.
2. Keep the case records, stable descriptors, CFE `Result<TValue, TError>` signatures, HTTP/gRPC
   adapters, tests, and use-case composition.
3. Remove Dunet package references and generator output.
4. Run a published-package cutover for any union exposed by `*.Contracts`; the source-level generic
   signature may look unchanged, but the binary type shape changed.
5. Reassess the outer carrier independently. .NET 11 native unions do not currently imply a BCL
   Result type. Keep CSharpFunctionalExtensions unless a demonstrably better native-union-aware
   carrier exists and provides enough benefit to justify a separate migration.

Implementing now makes the .NET 11 change easier: the expensive work is identifying expected
failures, assigning ownership and stable codes, fixing transports, and making callers compose them.
Waiting would combine that architectural migration with a runtime/language upgrade. With this plan,
the later work is predominantly declaration and package-cutover work.
