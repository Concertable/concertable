# HTTP-guard consolidation — kill the copied `?? throw new NotFoundException(...)` boilerplate

**Own branch off `master`: `Refactor/HttpGuardConsolidation`** — codebase-wide cleanup, not part of any
feature. Delivery is inherently **≥2 merges** because `Concertable.Kernel` is a published package: merge
the helper into Kernel first (republishes on merge to `master`), then migrate consuming-service call
sites against the published version. Shipping helper + first call sites in one PR is exactly what broke
CI last time.

> **Status: designed, ready to implement. Merge 1 not yet started.**

---

## Verdict (settled)

**A fluent guard for the `?? throw` (Shape-A) case, living in `Concertable.Kernel`. Adopt no library. Do
not introduce a Result type for 404s. Leave every `if (cond) throw` (Shape-B) guard alone.**

Two forms of the guard, and why the split is fundamental:

- **Entity fetch → self-naming, ZERO string at the call site.** The entity already knows its own name
  (`INamedEntity.DisplayName`). Typing `"Venue"` when you fetched a `VenueEntity` is the redundancy being
  removed.
- **Value-type / DTO / projection fetch → explicit label, string required and irreducible.**
  `GetTenantIdByIdAsync → Guid?`, `GetDetailsByIdAsync → VenueDetails?`, `GetArtistPayeeAsync →
  PayeeSummary?`. The name (`"Concert Opportunity"`, `"Venue"`) exists **nowhere** in the return type —
  the call site is the only source. No mechanism can remove it; `typeof(T).Name` can't help (it isn't
  even an entity), and the display names differ from type names anyway (`OpportunityEntity` → "Concert
  Opportunity", `BookingAgreementEntity` → "Booking agreement" *with a space*).

So the label/struct overloads are **not** redundant once entities carry `DisplayName` — roughly half the
Shape-A sites fetch a `Guid?`/`int?`/DTO, which categorically cannot implement the interface. Those are
the exact sites that *had* to hand-roll (constraint #6).

### Why not the named libraries / Result pattern

| Option | Verdict | Why |
|---|---|---|
| **`Ardalis.GuardClauses`** (`Guard.Against.NotFound`) | **Reject** | Ships its *own* `Ardalis.GuardClauses.NotFoundException`, a different type from `Concertable.Kernel.Exceptions.NotFoundException`. Every service's `GlobalExceptionHandler` maps only our `HttpException` → ProblemDetails; Ardalis's would fall through to **500**, not 404. Fixing that means writing custom `IGuardClause` extensions that throw *our* exceptions — the same in-house code, plus a dependency, plus a `Guard.Against.` prefix that reads worse than `.OrNotFound()`. |
| **`ErrorOr` / expand `FluentResults` to 404s** | **Reject (out of scope)** | This codebase committed to the exception + `GlobalExceptionHandler` → ProblemDetails model everywhere. `FluentResults` 4.0 *is* used, but for a different job (service validation `Result<T>` → caller throws `BadRequestException`). Rebuilding the 404 path on `Result<T>` is a cross-cutting rework, out of scope. |
| **`CommunityToolkit.Diagnostics`, `Dawn.Guard`, `Ensure.That`** | **Reject** | Throw the `ArgumentException` family — wrong exception type for `GlobalExceptionHandler`, aimed at hot-path arg validation, not consumer-facing HTTP `detail`. |
| **`TypedResults` / endpoint filters** | **Reject (out of scope)** | Minimal-API idiom; this codebase is controller-based with a central exception handler. |
| **In-house guard in Kernel** | **Adopt** | Uses the existing sealed `NotFoundException`, so `GlobalExceptionHandler` maps it for free. No new dependency. Additive to the Kernel package. Handles the value-type sites a `where T : class` helper can't touch. |

---

## Design

### Scope — which families

| Exception (→ status) | total | `?? throw` (A) | `if…throw` (B) | Action |
|---|---|---|---|---|
| `NotFoundException` (404) | 79 | 75 | 4 | **Consolidate Shape A** (the win). Shape-B 4 → existing `NotFoundException.ThrowIfNull`. |
| `ForbiddenException` (403) | 14 | 8 | 6 | **Optional** `.OrForbidden(msg)` for the 8 (mostly `TenantId ?? throw`, a `Guid?`). Leave B. |
| `InternalServerException` (500) | 4 | 4 | 0 | **Optional** `.OrInternalServerError(msg)`. Marginal — bespoke messages, no template. |
| `BadRequestException` (400) | 36 | 5 | 31 | **Leave.** Overwhelmingly Shape B + the `if (result.IsFailed) throw` idiom, already consolidated. |
| `ConflictException` (409) | 3 | 0 | 3 | **Leave.** All Shape B. |
| `UnauthorizedException` (401) | 1 | 0 | 1 | **Leave.** |
| `DomainException` (not `HttpException`) | 24 | 0 | 24 | **Out of scope.** Not an `HttpException`; domain-invariant; has its own `ThrowIfNull`. |

**Shape A and Shape B do NOT share one mechanism.** Shape A unwraps a nullable or 404s — a value transform
returning the non-null value; it fits a fluent guard perfectly. Shape B is a boolean assertion with no
value to return; the idiomatic form is the `if` already written, and an `Ensure(cond, …)` wrapper is a
readability regression. One mechanism for Shape A; nothing new for Shape B.

### Self-naming: `DisplayName` on an opt-in `INamedEntity` marker (NOT on `IEntity`)

```csharp
namespace Concertable.Kernel;

public interface INamedEntity : IEntity
{
    static abstract string DisplayName { get; }   // authored, e.g. => "Booking agreement"
}
```

A C# 11 `static abstract` member: an entity that opts in names itself once, compile-time, no reflection, no
type-name surgery, no `Entity`-suffix leak. **Static** because the name is read off the type `T` when the
value is `null` (a missing fetch throwing a 404) — there is no instance to read a normal property from. The
self-naming `OrNotFound` overload is constrained `where T : class, INamedEntity`; entities opt in during the
call-site migration (Merges 2..N), against the republished Kernel.

#### Why NOT `static abstract DisplayName` on `IEntity` itself (REJECTED — proven un-mergeable)

Putting the member on the base `IEntity` is the tempting "uniform standard" — and it is **impossible to
land in this repo**. Proven by two red CI runs (`TypeLoadException: get_DisplayName not implemented`, first
on `TenantEntity`, then `ConcertEntity` even after adding `DisplayName` to all 37 entities). Root cause:

- `static abstract` interface members are wired to their implementation **at compile time**, against the
  exact interface version the implementer compiled against.
- The **core libs** (`DataAccess.Infrastructure`, `Messaging.Domain`) reference Kernel by **source**
  `ProjectReference`, so every integration test loads the **new** Kernel (assembly identity `0.0.0.0`).
- The **service module entities** reference Kernel by **published package** (`ConcertablePlatformVersion`),
  so they compile against the **old** `IEntity` — no `get_DisplayName` mapping is recorded in their
  metadata, whatever their source says.
- Runtime: new Kernel demands the mapping, package-compiled entities don't have it → `TypeLoadException`.

It is a **deadlock**, not a sequencing problem: entities can only record the mapping by compiling against a
Kernel *package* that has the member, which cannot exist until this change publishes, which cannot happen
while CI is red — and the core libs always load source Kernel against package-compiled entities, so no
single PR and no ≥2-merge split escapes it. `INamedEntity` sidesteps it entirely: `IEntity` gains nothing,
so no existing entity's type-load ever changes and nothing throws. **Do not re-attempt `IEntity.DisplayName`
without first changing how the core libs reference Kernel — a separate, much larger architectural change.**

### The guard — an extension class co-located in `NotFoundException.cs`

`OrNotFound` is the **expression** form (returns the value so it chains inline); `NotFoundException.ThrowIfNull`
is the **statement** form (`void`, `[NotNull]` flow-analysis). The expression form **must** be an extension
method — postfix `value.OrNotFound()` on `Task<T>`/`T?`/`Guid?` (types we don't own) is the only C# shape
that satisfies constraints #4 (no `(await …)` wrapper parens) and #5 (chains inline), and an extension
method must live in a `static` class, so it **cannot** go on the non-static `NotFoundException` class.
It lives as a second top-level `static class` in the **same file**, `NotFoundException.cs`.

```csharp
public static class NotFoundExtensions
{
    // Self-naming — entity fetches carry their own display name; ZERO string at the call site.
    public static async Task<T> OrNotFound<T>(this Task<T?> task) where T : class, IEntity
        => await task ?? throw new NotFoundException($"{T.DisplayName} not found");

    // Explicit label — DTOs/projections + id-bearing/contextual messages (name is irreducible here).
    public static async Task<T> OrNotFound<T>(this Task<T?> task, string entity) where T : class
        => await task ?? throw new NotFoundException($"{entity} not found");

    // Value types — the sites a `where T : class` helper can't touch (Guid?/int? id projections).
    public static async Task<T> OrNotFound<T>(this Task<T?> task, string entity) where T : struct
        => await task ?? throw new NotFoundException($"{entity} not found");
}
```

**Every overload is over `Task<T?>` — no sync `this T?` overloads.** Verified against all 75 sites: every
migratable one is `await repo.GetX() ?? throw`. The only two sync sites (`StripeHoldClient` `held?.Id`,
`ManagerPaymentService` `account?.StripeCustomerId`) carry bespoke wording and stay hand-rolled anyway
(constraint #7), so sync overloads would be dead code — omitted. (This also sidesteps the `CS0121`
ambiguity a sync `where T : class` label overload would hit against the async `Task<T?>` overload, since
`Task<Foo?>` is itself a reference type.) If a sync site ever appears, add the overload then.

### The one rule that keeps HTTP `detail` unchanged

**Self-name (zero-arg) ONLY where the current message equals `"{DisplayName} not found"` verbatim.**
Otherwise use the label overload. The same entity often has contextual variants — `VenueEntity` self-names
to `"Venue not found"`, but `OpportunityService` throws `"Venue not found for current user"` off the same
type. Self-naming that site would silently change the `detail`. Rule: canonical bare message → zero-arg;
every contextual / id-bearing / bespoke variant → `.OrNotFound("…")` label. Migration is behaviour-
preserving: same exception, same status, same `detail` string.

### Constraint scorecard

| # | Constraint | Met by |
|---|---|---|
| 1 | No magic string at the common call site | zero-arg self-naming overload — string lives once on the entity |
| 2 | No type-name string surgery | `static abstract DisplayName` — no `typeof`/`EndsWith` anywhere |
| 3 | Never leak `Entity` suffix | `DisplayName` is authored (`"Booking agreement"`), never derived |
| 4 | Composes with `await`, no wrapper parens | extensions over `Task<T?>` (`await task ??` binds tighter than `??`) |
| 5 | Returns non-null value, chains inline | all overloads return `T` |
| 6 | Nullable value types | `where T : struct` async + sync overloads |
| 7 | Preserves id-bearing / bespoke messages | label overload takes interpolation; bespoke non-template sites stay hand-rolled |
| 8 | Lives in Kernel, agnostic, cheap | `Concertable.Kernel.Exceptions`; no reflection on any path |
| 9 | Idiomatic & boring | `.OrNotFound()` reads instantly; extension-guard-returns-value is the standard .NET fluent idiom |

### Relationship to the existing `ThrowIfNull`

Keep `NotFoundException.ThrowIfNull` / `ForbiddenException.ThrowIfNull`. They're the **statement** form:
`[NotNull]`-annotated so the compiler treats the argument as non-null afterwards. `OrNotFound` is the
**expression** form that returns the value inline. Complementary, not redundant. Do not unify.

### Optional (Forbidden/InternalServer Shape-A, lower value — no template, just removes `throw new`)

```csharp
// same NotFoundException.cs sibling static class, or ForbiddenException.cs
public static async Task<T> OrForbidden<T>(this Task<T?> task, string message) where T : class
    => await task ?? throw new ForbiddenException(message);
public static T OrForbidden<T>(this T? value, string message) where T : struct
    => value ?? throw new ForbiddenException(message);   // e.g. context.TenantId.OrForbidden("No active tenant …")
```

---

## Before / after (real sites, with resulting HTTP `detail`)

**(a) Entity fetch, canonical message → self-naming** — `BookingAgreementService.cs:24`
```csharp
// before
var agreement = await repository.GetByApplicationIdAsync(applicationId)
    ?? throw new NotFoundException("Booking agreement not found");
// after  (BookingAgreementEntity : INamedEntity { static string DisplayName => "Booking agreement"; })
var agreement = await repository.GetByApplicationIdAsync(applicationId).OrNotFound();
```
→ 404 `detail`: **`Booking agreement not found`** (unchanged)

**(b) Value type → label required** — `ApplyExecutor.cs:61` (`GetTenantIdByIdAsync` returns `Task<Guid?>`)
```csharp
// before
application.VenueTenantId = await opportunityRepository.GetTenantIdByIdAsync(opportunityId)
    ?? throw new NotFoundException("Concert Opportunity not found");
// after
application.VenueTenantId = await opportunityRepository.GetTenantIdByIdAsync(opportunityId)
    .OrNotFound("Concert Opportunity");
```
→ 404 `detail`: **`Concert Opportunity not found`** (unchanged)

**(c) DTO fetch → label required** — `VenueService.cs:45` (`GetDetailsByIdAsync` returns `VenueDetails?`)
```csharp
// before
return await publicRepository.GetDetailsByIdAsync(id) ?? throw new NotFoundException("Venue not found");
// after (VenueDetails is a read DTO, not IEntity → label)
return await publicRepository.GetDetailsByIdAsync(id).OrNotFound("Venue");
```
→ 404 `detail`: **`Venue not found`** (unchanged)

**(d) Contextual variant off a self-naming entity → STAYS on label** — `OpportunityService.cs:43`
```csharp
// message ≠ "{DisplayName} not found", so NOT zero-arg
... .OrNotFound("Venue for current user");
```
→ 404 `detail`: **`Venue not found for current user`** (unchanged)

**(e) Bespoke id message → stays hand-rolled** — `StripeHoldClient.cs:29`
```csharp
held?.Id ?? throw new NotFoundException($"No held payment intent found for application {applicationId}");
```

**(f) Shape-B guard — LEFT ALONE** — `ApplyExecutor.cs:85`
```csharp
throw new BadRequestException("You cannot apply to the same concert opportunity twice");
```

---

## Migration + merge plan (expand/contract, ≥2 merges)

`Concertable.Kernel` is a published NuGet package (`IsPackable=true`); consumers compile against the
*published* package, not the source beside them. `INamedEntity` is purely additive — it changes nothing on
`IEntity`, so it is invisible and harmless downstream until each service bumps Kernel and opts an entity in.

### ☐ Merge 1 — Kernel only (additive; republishes)
Add `INamedEntity.cs` (the opt-in marker) + the `NotFoundExtensions` static class inside
`NotFoundException.cs` (+ optional `OrForbidden`). **No entities touched, no call sites touched.**
**Gate:** `dotnet build api/Concertable.slnx` clean + Kernel unit tests. No behaviour change anywhere.
**This must merge and republish before any call site or entity is migrated.**

### ☐ Merges 2..N — per service, against the now-published Kernel (parallelisable)
Each PR bumps `ConcertablePlatformVersion` to the republished Kernel, then for that service: (i) adds
`: INamedEntity` + `DisplayName` to the entities fetched at canonical Shape-A sites, (ii) migrates that
module's `?? throw new NotFoundException(...)` sites per the verbatim-match rule (zero-arg where the
message is canonical, label otherwise). Only entities that actually self-name need the marker — the rest
stay untouched. Rough distribution of the 75 Shape-A sites:

- **B2B Concert** (~45): `Services/` (`ApplicationService`, `BookingService`, `ConcertService`,
  `BookingAgreementService`, `ContractAccessor`, `ConcertDraftService`, `OpportunityService`,
  `BookingAgreementBuilder`, `ApplicationNotifier`) + `Workflow/Executors` + `Workflow/Steps`. Includes
  most value-type sites (`GetTenantIdByIdAsync`/`GetVenueTenantIdAsync`/`GetPeriodByIdAsync`).
- **B2B other** (~10): Venue, Artist, Tenant, Contract, Conversations.
- **Customer** (~6): `TicketService`, `TicketValidator`, `QrCodeService`, `PreferenceService`, `ConcertReviewService`.
- **Payment** (~10): `PaymentManager`, `ManagerPaymentService`, `CustomerPaymentService`, `EscrowService`,
  `StripeHoldClient` — **almost all bespoke id-bearing messages → label overload or stay hand-rolled**.

**Sites that intentionally keep a bespoke `?? throw`** (non-`"{X} not found"` wording — do NOT reword):
`StripeHoldClient` `"No held payment intent found for application {id}"`, `ContractAccessor` `"No contract
with id {contractId}"`, `"Cannot find ticket"`, `"No concert found for Application ID {id}"`. Constraint #7
protects their wording.

**Per-phase gate:** build + the affected module's unit/integration tests via `integration-debug`.
Behaviour-preserving (same exception, same 404, same message text) → **skip E2E**.

### Out of scope / separate (do NOT bundle)
`ApplyExecutor` fetches the same `opportunityId` three times, two throwing identical `"Concert Opportunity
not found"`. Collapsing those is a redundant-DB-round-trip fix, not a guard-shape fix — track it with the
`ApplicationSigning.SignTerms` encapsulation idea; don't double-refactor here.

## Done when
No `?? throw new NotFoundException(...)` remains outside `NotFoundException.cs` and the enumerated bespoke
sites (spot-check with a repo-wide grep). Delete this plan in the commit that finishes the last migration.
