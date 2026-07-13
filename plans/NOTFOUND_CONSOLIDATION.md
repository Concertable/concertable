# NotFound consolidation — route every `?? throw new NotFoundException(...)` through the canonical helper

**Not for the current PR** (`Feature/BookingAgreement`). Own branch: `Refactor/NotFoundConsolidation`,
off `master` — this is codebase-wide cleanup, not part of the booking-agreement feature.

## Problem

`Concertable.Kernel.Exceptions.NotFoundExtensions` already defines the canonical pattern:

```csharp
public static async Task<T> OrNotFound<T>(this Task<T?> task, string? entity = null) where T : class ...
public static T OrNotFound<T>(this T? value, string? entity = null) where T : class ...
```

But ~70 sites across B2B / Customer / Payment hand-roll `?? throw new NotFoundException("X not found")`
instead of using it. Two root causes:

1. **The helper only covers reference types (`where T : class`).** Every repo method that returns a
   nullable *value type* — `Task<Guid?>` (`GetTenantIdByIdAsync`, `GetVenueTenantIdAsync`,
   `GetOwnerByIdAsync`, `GetVenueManagerIdAsync`), `Task<int?>` (`GetIdForCurrentTenantAsync`,
   `GetContractIdByIdAsync`, `GetIdByApplicationIdAsync`, …), `Task<DateRange?>`
   (`GetPeriodByIdAsync`) — *cannot* call `OrNotFound` at all. These sites hand-roll out of necessity.
2. **Ref-type sites that simply were never migrated** — services/controllers that predate the helper
   or copied a sibling.

Costs: duplicated wording that drifts ("Concert not found" vs "Concert Opportunity not found"), no
single place to change NotFound behaviour, and — in `ApplyExecutor` — the same opportunity fetched
two-to-three times, each with its own redundant throw.

## Goal

Every "value must exist or it's a 404" becomes a single `.OrNotFound(...)` call. Zero bespoke
`?? throw new NotFoundException(...)` left except where the message genuinely doesn't fit the
`"{entity} not found"` shape (those get a dedicated raw-message overload, still through the helper).

## Non-goals / constraints

- **Preserve every message that carries context.** `$"Contract {contractId} not found"` must stay
  `"Contract 5 not found"`, not degrade to `"Contract not found"`. The `entity` param already allows
  this: `.OrNotFound($"Contract {contractId}")`.
- **Don't distort intentional wording to fit a template.** Sites like `"No held payment intent found
  for application {id}"`, `"Cannot find ticket"`, `"No concert found for Application ID {id}"` don't
  match `"{entity} not found"` — they use the raw-message overload (Phase 1), not a reworded label.
- **No public-contract breakage.** `NotFoundExtensions` lives in `Concertable.Kernel`, consumed by
  every service. Phase 1 is purely *additive* (new overloads) — safe. No existing signature changes.
- This is a message/DRY refactor with **no behaviour change** (same exception type, same HTTP 404,
  same or identical-in-intent messages) — except Phase 4, which removes redundant DB round-trips.

## Phases

Each phase is independently shippable and ends green. Gate for every phase: `dotnet build
api/Concertable.slnx` clean + the affected modules' unit/integration tests via `integration-debug`.
This is behaviour-preserving, so **skip the E2E suites** for Phases 1–3; consider E2E for Phase 4
only (it touches the Apply flow) per `plans/CLAUDE.md`'s massive/risky bar.

### Phase 1 — Extend the canonical helper (additive, no call sites touched)

In `NotFoundExtensions`, add:

- **Value-type overloads** — the missing half:
  ```csharp
  public static async Task<T> OrNotFound<T>(this Task<T?> task, string entity) where T : struct
      => await task ?? throw new NotFoundException($"{entity} not found");
  public static T OrNotFound<T>(this T? value, string entity) where T : struct
      => value ?? throw new NotFoundException($"{entity} not found");
  ```
  `entity` is **required** here — there's no meaningful `DisplayName<Guid>()`, so the caller must
  name what was missing (`GetTenantIdByIdAsync(id).OrNotFound("Concert Opportunity")`).
- **Raw-message overload** for wording that isn't `"{X} not found"` shaped — pick a distinct name so
  it doesn't collide with the label overloads (e.g. `OrNotFoundMessage(this T? value, string message)`
  / the `Task<T?>` twin), throwing the message verbatim.

Verify the `class` vs `struct` overloads don't create ambiguous-call errors at existing call sites
(they can't both apply to one `T`, but confirm the build is clean before migrating anything).

**Gate:** build + Kernel unit tests. No behaviour change anywhere yet.

### Phase 2 — Migrate reference-type sites, module by module

Replace `await repo.GetXAsync(...) ?? throw new NotFoundException("X not found")` with
`.OrNotFound("X")` (or the raw-message overload for non-template wording). One commit per module/area
to keep diffs reviewable. Known clusters:

- B2B Venue (`VenueService` ×4), Artist (`ArtistService` ×3), Tenant, Contract, Conversations.
- B2B Concert: `ApplicationService`, `BookingService`, `ConcertService`, `ContractAccessor`,
  `ConcertDraftService`, `OpportunityService`, `BookingAgreementBuilder`, `ApplicationNotifier`,
  `OpportunityMapper`, and the Workflow `Steps/` + `Executors/` (`Accept`, `Cancel`, `Finish`,
  `Settlement`, `Verify`, `SetupCheckout`, `HoldCheckout`, `VerifyCheckout`, the escrow steps, …).
- Customer: `TicketService`, `TicketValidator`, `QrCodeService`, `PreferenceService`,
  `ConcertReviewService`.
- Payment: `PaymentManager`, `ManagerPaymentService`, `CustomerPaymentService`, `EscrowService`,
  `StripeHoldClient`.

For each: keep the existing message intent exactly (preserve interpolated ids via the label param;
use the raw-message overload where wording doesn't fit the template).

**Gate per commit:** build + that module's unit/integration tests.

### Phase 3 — Migrate value-type sites (unblocked by Phase 1)

Now the `Guid?` / `int?` / `DateRange?` sites can use the helper. Same module-by-module approach.
This is the set that was *impossible* before Phase 1.

**Gate per commit:** build + affected module tests.

### Phase 4 — Collapse `ApplyExecutor`'s redundant opportunity lookups

`ApplyExecutor.ExecuteAsync` currently hits the same `opportunityId` three times:
`contractResolver.ResolveByOpportunityIdAsync`, `opportunityRepository.GetTenantIdByIdAsync`, and
`opportunityRepository.GetPeriodByIdAsync` — the last two throwing identical
`"Concert Opportunity not found"`. Fetch the opportunity's tenant + period (+ contract) **once** and
derive from it, so there's a single existence check and a single NotFound. Prefer one repo method
returning the pieces the executor needs over three keyed round-trips.

> Overlaps with the separate signature/fingerprint encapsulation idea (`ApplicationSigning.SignTerms`)
> discussed for `ApplyExecutor` — if that lands first, fold this dedupe into it; otherwise do it here.
> Don't do both blindly and double-refactor the same method.

**Gate:** build + Concert module unit/integration tests. This one touches the live Apply flow —
run the UI E2E via `e2e-ui-debug` if the change feels behaviourally risky (per `plans/CLAUDE.md`).

## Done when

No `?? throw new NotFoundException(...)` remains outside `NotFoundExtensions` itself (spot-check with
a repo-wide grep), and `ApplyExecutor` fetches its opportunity once. Delete this plan in the commit
that finishes the last phase.
