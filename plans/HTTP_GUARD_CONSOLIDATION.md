# HTTP-guard consolidation — kill the copied `?? throw new NotFoundException(...)` boilerplate

**Own branch off `master`: `Refactor/HttpGuardConsolidation`** — codebase-wide cleanup, not part of any
feature. Delivery is inherently **≥2 merges** because `Concertable.Kernel` is a published package: merge
the helper into Kernel first (republishes on merge to `master`), then migrate consuming-service call
sites against the published version. Shipping helper + first call sites in one PR is exactly what broke
CI last time.

> **Status: researched & designed, not started.** This plan is the output of the investigation
> (`HTTP_GUARD_HELPERS_INVESTIGATION.md`, now retired). The design below is verified — every overload
> was compiled and run against .NET 10 to prove overload resolution, self-naming, and inline chaining
> (see "Design verification" for what was actually tested). No production code written yet.

---

## Deliverable 1 — Step-1 verdict (cited state of the art)

**Recommendation: a ~20-line in-house extension helper in `Concertable.Kernel`, for the `?? throw`
(Shape-A) case only. Adopt no library. Do not introduce a Result type for 404s. Leave every
`if (cond) throw` (Shape-B) guard alone.**

### Is centralising these guards idiomatic?

Partly. The **guard-clause-that-returns-its-input** pattern is thoroughly idiomatic — it is exactly
what `Ardalis.GuardClauses` is built around (`_name = Guard.Against.NullOrWhiteSpace(name);` returns
the validated value so it chains) ([ardalis/GuardClauses README](https://github.com/ardalis/GuardClauses),
[discussion #158 on why guards return the input](https://github.com/ardalis/GuardClauses/discussions/158)).
But centralising the **predicate** guard (`if (cond) throw`) is *not* a widely-reached-for pattern — most
teams write the `if` inline and consider it fine, because a hand-rolled `if (x) throw new FooException("msg")`
is already maximally clear and an `Ensure(cond, () => ...)` wrapper adds a closure and an inverted-
condition reading cost for no real gain. So the honest split: consolidate Shape A, leave Shape B.

### Why not the named libraries / Result pattern

| Option | Verdict for THIS codebase | Why |
|---|---|---|
| **`Ardalis.GuardClauses`** (`Guard.Against.NotFound`) | **Reject** | It ships its *own* `Ardalis.GuardClauses.NotFoundException`, a different type from `Concertable.Kernel.Exceptions.NotFoundException`. Every service's `GlobalExceptionHandler` maps only `HttpException` → ProblemDetails; Ardalis's exception would fall through to the **500** branch, not 404. To fix that you'd write custom `IGuardClause` extensions that throw *our* exceptions — i.e. the same in-house code, plus a dependency, plus a `Guard.Against.` prefix that reads worse than `.OrNotFound()`. ([README](https://github.com/ardalis/GuardClauses)) |
| **`ErrorOr` / expand `FluentResults` to 404s** | **Reject (out of scope)** | The discriminated-result school ([ErrorOr](https://github.com/amantinband/error-or), [Result pattern in ASP.NET Core](https://www.red-gate.com/simple-talk/development/dotnet-development/the-result-pattern-in-asp-net-core-minimal-apis/)) is a legitimate alternative to throwing for expected 404s — but this codebase has already committed to the exception + `GlobalExceptionHandler` → ProblemDetails model everywhere. `FluentResults` 4.0.0 *is* already used, but for a **different** job: service methods return `Result<T>` for validation failures and callers do `if (result.IsFailed) throw new BadRequestException(result.Errors)`. Rebuilding the 404 path on `Result<T>` is a cross-cutting architecture change the investigation explicitly rules out of scope. |
| **`CommunityToolkit.Diagnostics` `Guard`, `Dawn.Guard`, `Ensure.That`** | **Reject** | Throw the `ArgumentException` family, aimed at hot-path argument validation — not consumer-facing HTTP `detail` strings, and again the wrong exception type for `GlobalExceptionHandler`. |
| **`TypedResults` / `Results<Ok<T>, NotFound>` + endpoint filters** | **Reject (out of scope)** | Minimal-API idiom ([MS Learn: error handling](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api)); this codebase is controller-based with a central exception handler. Same "don't re-plumb the whole error model" reasoning. |
| **In-house extension (~20 LOC in Kernel)** | **Adopt** | Uses the existing sealed `NotFoundException`, so `GlobalExceptionHandler` maps it for free. No new dependency. Additive to the Kernel package. Handles the value-type sites (`Guid?`/`int?`/`DateRange?`) that a `where T : class` helper can't touch — the exact sites that *had* to hand-roll. |

### Exceptions-for-flow-control consensus

Real and worth respecting — many argue a not-found isn't "exceptional" and belongs in a `Result<T>`.
But the codebase has already chosen; a recommendation to rip out `HttpException`/`GlobalExceptionHandler`
is out of scope. The in-house helper is a **DRY refactor within** the chosen model, not a re-litigation
of it.

---

## Deliverable 2 — Ranked design

### Which families to consolidate (from the grep, `api/` excluding Kernel)

| Exception (→ status) | total | `?? throw` (A) | `if…throw` (B) | Recommendation |
|---|---|---|---|---|
| `NotFoundException` (404) | 79 | 75 | 4 | **Consolidate Shape A** (the win). Shape-B 4 → existing `NotFoundException.ThrowIfNull`. |
| `ForbiddenException` (403) | 14 | 8 | 6 | **Optional** `.OrForbidden(msg)` for the 8 (mostly `TenantId ?? throw`, a `Guid?` value type). Leave B. |
| `InternalServerException` (500) | 4 | 4 | 0 | **Optional** `.OrInternalServerError(msg)`. Marginal — bespoke messages, no template. |
| `BadRequestException` (400) | 36 | 5 | 31 | **Leave.** Overwhelmingly Shape B + the `if (result.IsFailed) throw new BadRequestException(result.Errors)` idiom, already consolidated. |
| `ConflictException` (409) | 3 | 0 | 3 | **Leave.** All Shape B. |
| `UnauthorizedException` (401) | 1 | 0 | 1 | **Leave.** |
| `PaymentRequiredException` (402) | 0 | — | — | n/a |
| `DomainException` (not `HttpException`) | 24 | 0 | 24 | **Out of scope.** Not an `HttpException`; domain-invariant; already has its own `ThrowIfNull`/`ThrowIfNullOrWhiteSpace`. |

**Shape A and Shape B do NOT share one mechanism.** Shape A is "unwrap a nullable or 404" — a value
transform that returns the non-null value; it fits an extension perfectly. Shape B is a boolean assertion
with no value to return; the idiomatic form is the `if` already written, and a `Ensure(cond, …)` wrapper
is a readability regression. So: **one mechanism (fluent `.Or*` extensions) for Shape A; nothing new for
Shape B.**

### The naming problem — self-naming entities (satisfies hard constraints #1–#3)

Constraint #1 (no redundant magic string at the call site) collides with #2 (no `typeof(T).Name` surgery)
and #3 (never leak the `Entity` suffix). There is **no** existing display-name metadata on entities
(markers are `IEntity`/`IEntity<TKey>`/`IIdEntity`/`IGuidEntity`, `Id`-only). The clean resolution is a
**C# 11 `static abstract` interface member** — the entity names itself *once*, compile-time, no reflection,
no string surgery, no suffix leak:

```csharp
namespace Concertable.Kernel;

public interface INamedEntity : IEntity
{
    static abstract string EntityName { get; }   // e.g. => "Booking agreement"
}
```

A `.OrNotFound()` overload constrained to `where T : class, INamedEntity` reads `T.EntityName` directly.
The magic string moves from **75 call sites to one declaration per entity** — and disappears from the
call site entirely (`await repo.GetByApplicationIdAsync(id).OrNotFound()`).

### The overload set (BEST design)

```csharp
namespace Concertable.Kernel.Exceptions;

/// Inline "must exist or it's a 404" guards — the expression-returning companion to the
/// statement-form NotFoundException.ThrowIfNull (which stays for the [NotNull] flow-analysis case).
public static class NotFoundExtensions
{
    // Self-naming — INamedEntity carries its own display name; ZERO string at the call site.
    public static async Task<T> OrNotFound<T>(this Task<T?> task) where T : class, INamedEntity
        => await task ?? throw new NotFoundException($"{T.EntityName} not found");

    public static T OrNotFound<T>(this T? value) where T : class, INamedEntity
        => value ?? throw new NotFoundException($"{T.EntityName} not found");

    // Explicit label — "{entity} not found". DTOs/projections (no INamedEntity) and id-bearing messages.
    public static async Task<T> OrNotFound<T>(this Task<T?> task, string entity) where T : class
        => await task ?? throw new NotFoundException($"{entity} not found");

    // Value types — the sites a `where T : class` helper could never touch. Label required
    // (there is no meaningful name to infer from a Guid/int/DateRange).
    public static async Task<T> OrNotFound<T>(this Task<T?> task, string entity) where T : struct
        => await task ?? throw new NotFoundException($"{entity} not found");

    public static T OrNotFound<T>(this T? value, string entity) where T : struct
        => value ?? throw new NotFoundException($"{entity} not found");
}
```

**Why there is no `OrNotFound<T>(this T? value, string entity) where T : class` (sync ref + label).**
It is provably impossible to have alongside the async label overload: `Task<Foo?>` is *itself* a reference
type, so a `where T : class` sync overload binds `T = Task<Foo?>` and collides with the `Task<T?>` async
overload — `error CS0121: The call is ambiguous`. Verified by compiling it. The **self-naming** sync
overload escapes this because `Task<T>` does **not** implement `INamedEntity`, so it can't bind. The few
sync-ref-with-bespoke-message sites (e.g. `held?.Id ?? throw new NotFoundException("No held payment
intent found for application {id}")`) therefore **stay hand-rolled** — which is fine, they carry bespoke
non-`"{X} not found"` wording anyway (constraint #7).

### Constraint scorecard

| # | Constraint | Met by |
|---|---|---|
| 1 | No magic string at the common call site | `OrNotFound()` self-naming overload — string lives once on the entity |
| 2 | No type-name string surgery | `static abstract EntityName` — no `typeof`/`EndsWith` anywhere |
| 3 | Never leak `Entity` suffix | `EntityName` is authored (`"Booking agreement"`), never derived |
| 4 | Composes with `await`, no `(await …)` parens | Extensions over `Task<T?>` (`await task ??` binds tighter than `??`) |
| 5 | Returns non-null value, chains inline | All overloads return `T` |
| 6 | Nullable value types | `where T : struct` async + sync overloads |
| 7 | Preserves id-bearing / bespoke messages | Label overload takes interpolation (`$"Contract {id}"`); bespoke non-template sites stay hand-rolled |
| 8 | Lives in Kernel, agnostic, cheap | `Concertable.Kernel.Exceptions`; no reflection on any path |
| 9 | Idiomatic & boring | `.OrNotFound()` reads instantly |

### FALLBACK design (if `static abstract` is judged too heavy a per-entity touch)

Drop `INamedEntity`; keep only the **label** and **value-type** overloads. Every ref-type site passes
`.OrNotFound("Booking agreement")`. This *fails hard constraint #1* (the redundant string is back), but
it's a 3-overload, zero-new-interface change and still unblocks all the value-type sites and kills the
message *drift*. Recommended only if implementing `INamedEntity` across ~40 entities proves not worth it.
Also possible as a stepping stone: ship the label+value-type overloads first (merge sequence below), add
`INamedEntity` self-naming later as a pure addition.

### Relationship to the existing `ThrowIfNull`

Keep `NotFoundException.ThrowIfNull` (and `ForbiddenException.ThrowIfNull`). They are the **statement**
form: `[NotNull]`-annotated so the compiler treats the argument as non-null *afterwards* — valuable for
`ThrowIfNull(x); useNonNull(x);`. `OrNotFound` is the **expression** form that returns the value inline.
Complementary, not redundant. Do not unify.

---

## Deliverable 3 — Final code

Two files, both additive to Kernel.

**`api/Concertable.Shared/src/Concertable.Kernel/INamedEntity.cs`**
```csharp
namespace Concertable.Kernel;

/// <summary>An entity that carries its own human-readable name for use in "not found" messages,
/// so <c>OrNotFound()</c> needs no label at the call site and no type-name reflection.</summary>
public interface INamedEntity : IEntity
{
    static abstract string EntityName { get; }
}
```

**`api/Concertable.Shared/src/Concertable.Kernel/Exceptions/NotFoundExtensions.cs`**
```csharp
namespace Concertable.Kernel.Exceptions;

/// <summary>Inline "must exist or it's a 404" guards — the expression-returning companion to the
/// statement-form <see cref="NotFoundException.ThrowIfNull"/>. Returns the non-null value so it chains.</summary>
public static class NotFoundExtensions
{
    public static async Task<T> OrNotFound<T>(this Task<T?> task) where T : class, INamedEntity
        => await task ?? throw new NotFoundException($"{T.EntityName} not found");

    public static T OrNotFound<T>(this T? value) where T : class, INamedEntity
        => value ?? throw new NotFoundException($"{T.EntityName} not found");

    public static async Task<T> OrNotFound<T>(this Task<T?> task, string entity) where T : class
        => await task ?? throw new NotFoundException($"{entity} not found");

    public static async Task<T> OrNotFound<T>(this Task<T?> task, string entity) where T : struct
        => await task ?? throw new NotFoundException($"{entity} not found");

    public static T OrNotFound<T>(this T? value, string entity) where T : struct
        => value ?? throw new NotFoundException($"{entity} not found");
}
```

**Optional (Forbidden/InternalServer Shape-A, lower value — no template, just removes `throw new`):**
```csharp
// same NotFoundExtensions file or a sibling GuardExtensions
public static async Task<T> OrForbidden<T>(this Task<T?> task, string message) where T : class
    => await task ?? throw new ForbiddenException(message);
public static T OrForbidden<T>(this T? value, string message) where T : struct
    => value ?? throw new ForbiddenException(message);   // e.g. context.TenantId.OrForbidden("No active tenant …")
```

### Design verification (actually compiled & run on .NET 10)

- All five `OrNotFound` overloads coexist with **no ambiguity**; the compiler picks self-naming for
  `INamedEntity` types with no arg, label for a bare string, and the struct overloads for `Guid?`/`int?`/
  `DateRange?`.
- Confirmed the **negative** result: adding a `where T : class` sync-label overload produces
  `CS0121: ambiguous` against the async `Task<T?>` overload (Task is a reference type). This is why that
  overload is deliberately absent.
- Confirmed `T.EntityName` (static abstract) is readable inside the constrained extension and produces
  `"Booking agreement not found"` from `BookingAgreementEntity.EntityName => "Booking agreement"`.
- Confirmed inline chaining with no wrapper parens: `(await repo.GetX().OrNotFound()).ToDto()`.

---

## Deliverable 4 — Before / after (real sites, with resulting HTTP `detail`)

**(a) Plain Shape-A 404** — `Concert.Infrastructure/Services/BookingAgreementService.cs:24`
```csharp
// before
var agreement = await repository.GetByApplicationIdAsync(applicationId)
    ?? throw new NotFoundException("Booking agreement not found");
// after  (BookingAgreementEntity : INamedEntity { static string EntityName => "Booking agreement"; })
var agreement = await repository.GetByApplicationIdAsync(applicationId).OrNotFound();
```
→ HTTP 404 `detail`: **`Booking agreement not found`** (unchanged)

**(b) Id-bearing 404** — `Payment.Infrastructure/EscrowService.cs:119`
```csharp
// before
?? throw new NotFoundException($"Escrow {escrowId} not found");
// after (bespoke id in the label param — no self-naming)
.OrNotFound($"Escrow {escrowId}");
```
→ HTTP 404 `detail`: **`Escrow 42 not found`** (unchanged)

**(c) `.ToDto()` chain + value type** — `Concert…/Workflow/Executors/ApplyExecutor.cs:61`
```csharp
// before  (GetTenantIdByIdAsync returns Task<Guid?> — no where T:class helper could touch this)
application.VenueTenantId = await opportunityRepository.GetTenantIdByIdAsync(opportunityId)
    ?? throw new NotFoundException("Concert Opportunity not found");
// after
application.VenueTenantId = await opportunityRepository.GetTenantIdByIdAsync(opportunityId)
    .OrNotFound("Concert Opportunity");
```
→ HTTP 404 `detail`: **`Concert Opportunity not found`** (unchanged). Chaining case, e.g.
`(await repo.GetVenueAsync(id).OrNotFound()).ToDto()` compiles with no wrapper parens.

**(d) Shape-B guard — LEFT ALONE** — `ApplyExecutor.cs:85`
```csharp
// stays exactly as is — already idiomatic, nothing a helper improves
throw new BadRequestException("You cannot apply to the same concert opportunity twice");
```
→ HTTP 400 `detail`: **`You cannot apply to the same concert opportunity twice`** (untouched)

---

## Deliverable 5 — Migration + merge plan (expand/contract, ≥2 merges)

`Concertable.Kernel` is a published NuGet package (`IsPackable=true`); consumers compile against the
*published* package, not the source beside them. New members are additive but invisible downstream until
Kernel republishes on merge to `master`. So:

### Merge 1 — Kernel only (additive; republishes)
Add `INamedEntity.cs` + `NotFoundExtensions.cs` (+ optional `OrForbidden`). No call sites touched.
**Gate:** `dotnet build api/Concertable.slnx` clean + Kernel unit tests. No behaviour change anywhere.
**This must merge before any call site is migrated** — collapsing this into merge 2 is exactly what
broke CI last time.

### Merges 2..N — per service, against the now-published Kernel (parallelisable across services)
Each is one PR (or several commits, one per module) that: (i) adds `: INamedEntity` + `EntityName` to the
entities it touches, (ii) migrates that module's `?? throw new NotFoundException(...)` sites. Rough
distribution of the 75 Shape-A sites:

- **B2B Concert** (~45, the bulk): `Services/` (`ApplicationService`, `BookingService`, `ConcertService`,
  `BookingAgreementService` ×3, `ContractAccessor`, `ConcertDraftService`, `OpportunityService`,
  `BookingAgreementBuilder`, `ApplicationNotifier`) + `Workflow/Executors` + `Workflow/Steps` (Apply,
  Cancel, Verify, Settlement, Finish, HoldCheckout, SetupCheckout, VerifyCheckout, the escrow steps).
  Includes most value-type sites (`GetTenantIdByIdAsync`/`GetContractIdByIdAsync`/`GetPeriodByIdAsync`).
- **B2B other** (~10): Venue (×4), Artist (×3), Tenant, Contract, Conversations.
- **Customer** (~6): `TicketService`, `TicketValidator`, `QrCodeService`, `PreferenceService`, `ConcertReviewService`.
- **Payment** (~10): `PaymentManager`, `ManagerPaymentService`, `CustomerPaymentService`, `EscrowService`, `StripeHoldClient`.

**Sites that intentionally keep a bespoke `?? throw` (non-`"{X} not found"` wording — do NOT reword to fit):**
`StripeHoldClient` `"No held payment intent found for application {id}"`, `ContractAccessor` `"No contract
with id {contractId}"`, any `"Cannot find ticket"` / `"No concert found for Application ID {id}"`. These
are sync-ref or bespoke-message anyway, and constraint #7 protects their wording.

**Nothing can be collapsed below 2 merges.** Merges 2..N are independent of each other (different services)
and can land in parallel once merge 1 republishes.

**Per-phase gate:** build + the affected module's unit/integration tests via `integration-debug`. This is
behaviour-preserving (same exception type, same 404, same message text), so **skip E2E** for the migration
merges (per `plans/CLAUDE.md`'s massive/risky bar).

### Out of scope / separate (do NOT bundle)
`ApplyExecutor` fetches the same `opportunityId` three times (`GetTenantIdByIdAsync` + `GetPeriodByIdAsync`
+ the contract resolve), two throwing identical `"Concert Opportunity not found"`. Collapsing those into a
single fetch is a **redundant-DB-round-trip** fix, not a guard-shape fix — it overlaps with the separate
`ApplicationSigning.SignTerms` encapsulation idea. Track it there; don't double-refactor the method inside
this consolidation.

## Done when
No `?? throw new NotFoundException(...)` remains outside `NotFoundExtensions` and the enumerated bespoke
sites (spot-check with a repo-wide grep). Delete this plan in the commit that finishes the last migration.
