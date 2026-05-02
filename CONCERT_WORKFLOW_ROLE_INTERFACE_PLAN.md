# Concert Workflow — Role-Interface Composition Plan

**Status:** **In progress on `Refactor/ConcertWorkflowCoupling` branch (started 2026-05-02).**

### Progress checkpoint (last updated 2026-05-02)

Read this section first when resuming the work — the original design content (further down) reflects pre-implementation intent and uses earlier names. The progress checkpoint is the source of truth.

#### Branch + commits

Branch: `Refactor/ConcertWorkflowCoupling`. Most-recent first:

| Commit | What |
|---|---|
| `9e095715` | **#11 landed (TPH refactor)**: `ApplicationEntity` abstract → `StandardApplication` (FlatFee/DoorSplit/Versus, no PM) + `PrepaidApplication` (VenueHire, non-nullable PM). `BookingEntity` abstract → `StandardBooking` (FlatFee/VenueHire) + `DeferredBooking` (DoorSplit/Versus, non-nullable PM). `IBookingService` split into `CreateStandardAsync(int)` + `CreateDeferredAsync(int, string)`. `VenueHireConcertWorkflow.AcceptAsync` casts to `PrepaidApplication`; `DeferredConcertService.FinishedAsync` casts to `DeferredBooking`. EF `HasDiscriminator<string>` on both hierarchies. Factories + dev/test seeders gain `CreatePrepaid` / `ConfirmedDeferred` / `AcceptedPrepaid`. Migrations re-scaffolded across all contexts (#15 done as part of #11). Concert build green, unit tests pass (8/8). Drive-by: integration tests had stale `/accept/{id}` URLs and missing PM bodies — fixed both; 6 remaining `Assert.Single(DraftCreated)` failures are pre-existing semantic mismatch (`ConcertDraftService` notifies both artist + venue but tests expect single — for #16). |
| `4641c201` | **#10 landed**: HATEOAS Checkout link on `ApplicationResponse`. `Actions = { Accept, Checkout? }`. Checkout is nullable, gated by `ConcertWorkflowCapabilityRegistry.Has<ICheckoutable>(ct)` — VenueHire emits null, others emit `{ href, method }`. Singleton `IApplicationResponseMapper` owns the registry; controller injects mapper, knows nothing about registry. Apply intentionally omitted from `OpportunityResponse` — zero variance, would be ceremony. Kept the boundary tight: HATEOAS reserved for capability-gated transitions, not "what's the URL of the obvious next button". |
| `efd19d99` | **#22 landed (customer-side parallel cleanup)**: `CustomerPaymentModule.PayAsync` fallback removed; `IStripeAccountService.TryGetPaymentMethodAsync` + `FakeStripeAccountService.TryGetPaymentMethodAsync` deleted; `TicketPurchaseParams.PaymentMethodId` and `ICustomerPaymentModule.PayAsync` PM tightened to non-nullable; validator now requires PM. Frontend `ticketApi.TicketPurchaseRequest.paymentMethodId` tightened to required `string`; `useTicketCheckout` state tightened to `string \| undefined` and the `purchase()` action guards before sending. |
| `557d713e` | **#23 landed**: Drop stored-payment fallback. Frontend `ImmediatePaymentSection.selectSaved()` + `DeferredPaymentSection` effect now pass `savedCard.id` (not `null`). `applicationApi.acceptApplication`, `useAcceptApplicationMutation`, `PaymentMethodSection.onChange`, `StripePaymentForm.onSuccess` all tightened to `string` (no `null`). Backend: `IConcertPaymentFlow.ResolvePaymentMethodAsync` deleted entirely (it was identity passthrough on OnSession + customer-existence-check on OffSession; the existence check moved into `OffSessionConcertPaymentFlow.PayAsync` as a precondition). `IUpfrontConcertService.InitiateAsync` + `IDeferredConcertService.InitiateAsync` PM tightened to non-nullable. `IManagerPaymentModule.TryGetPaymentMethodIdAsync` deleted (no callers post-cleanup). Drive-by: fixed frontend's `/application/accept/${id}` URL to match controller's `{id}/accept`. |
| `a13beac9` | **Unify apply + accept endpoints**; controller branches on optional PM in body. Reverts the earlier endpoint split. Wire format: `POST /api/Application/{oppId}` (apply, optional `ApplyRequest` body), `POST /api/Application/{appId}/checkout`, `POST /api/Application/{appId}/accept` (accept, optional `AcceptRequest` body). Service stays typed. |
| `da21a10e` | First progress checkpoint added to plan doc. |
| `9e0c590d` | Split `ApplicationService.ApplyAsync` + `AcceptAsync` into typed overloads; `IAcceptanceDispatcher.AcceptAsync` split into two non-nullable methods. |
| `b4f58f06` | `IApplyResolver` (resolves `ISimpleApply` / `IApplyWithPaymentMethod` by opportunity contract type). Added `IOpportunityRepository.GetContractIdByIdAsync` + `IContractLoader.LoadByOpportunityIdAsync`. |
| `a775bf58` | `IStandardConcertWorkflow` + `IPrepaidConcertWorkflow` bundle interfaces (pure marker composition; cosmetic shorthand for the current durable capability bundles). |
| `9d2d2fc5` | Redefined `IConcertWorkflow` as marker umbrella. Strategies rewritten as capability composers. `AcceptanceDispatcher` capability-cast. `IApplyWithPaymentMethod` → marker (no method). VenueHire injects `IApplicationRepository` to read its stored PM at accept time. |
| `e0e3753e` | `IConcertWorkflowStep` root marker + `ConcertWorkflowCapabilityRegistry` (singleton, type-only `IsAssignableTo` lookup, no workflow instantiation). |
| `fa8a3277` | Family markers `IApplyable` / `IAcceptable` / `ICheckoutable` + rename `IAcceptCheckout` → `ICheckout`. |
| `5b10ec27` | `Executor` → `Dispatcher` rename (`AcceptanceExecutor` / `CompletionExecutor` / `SettlementExecutor` → `AcceptanceDispatcher` / `CompletionDispatcher` / `SettlementDispatcher`). |
| `e21130c7` | `ApplicationEntity.PaymentMethodId` + two `Create` overloads (with/without PM). |

#### Pending tasks (in suggested execution order)

| # | Task | Notes |
|---|---|---|
| ~~23~~ | ~~Drop stored-payment fallback~~ — **DONE.** See commit table. Resolved more aggressively than originally scoped: the entire `IConcertPaymentFlow.ResolvePaymentMethodAsync` method was deleted (it had no remaining work after the fallback came out — OnSession was identity, OffSession was identity + `HasStripeCustomerAsync` check). The check moved to `OffSessionConcertPaymentFlow.PayAsync` (encapsulates off-session preconditions). `IManagerPaymentModule.TryGetPaymentMethodIdAsync` was also deleted — turned out to have no callers post-cleanup (frontend uses `GET /api/StripeAccount/payment-method` for saved-card rendering, not this contract method). |
| ~~22~~ | ~~Customer-side fallback parallel cleanup~~ — **DONE.** See commit table. |
| ~~10~~ | ~~HATEOAS action links~~ — **DONE.** Final shape narrower than originally planned: only `Checkout?` on `ApplicationResponse` (the one capability-gated transition). Apply/Accept omitted — apply has zero variance (one URL for everyone) and accept's variance is the PM body shape, which the FE already knows from `contract.type`. `requiresPayment` flag dropped entirely (post-#23 the wire-format presence/absence of `paymentMethodId` is the signal). |
| ~~11~~ | ~~Application + Booking TPH split~~ — **DONE.** See `9e095715` in commit table. PM column on each base disappeared; subtype owns it. Migrations re-scaffolded as part of this commit (so #15 also done). |
| 13 | Slim `IFinishOutcome` → `Task` | Migrate `E2EEndpointExtensions.cs` and `DevController.Complete` consumers off the `result.Value is DeferredFinishOutcome deferred` payload first. They currently read `deferred.Payment.TransactionId`. Move those test/dev reads to a separate query path (e.g. `GET /e2e/payment-intent/{applicationId}` already exists). |
| 14 | DI registration audit | **Effectively done already.** Strategies registered once under `IConcertWorkflow` keyed by `ContractType`; dispatchers/resolvers cast to specific capability via `is`. Verify no leftover registrations expected the old shape. |
| ~~15~~ | ~~Run `./initial-migrations.ps1`~~ — **DONE** as part of #11. |
| 16 | Test sweep | 6 remaining integration test failures: `Assert.Single(NotificationService.DraftCreated)` mismatches across `ApplicationFlatFeeApiTests`, `ApplicationDoorSplitApiTests`, `ApplicationVersusApiTests`, `ApplicationVenueHireApiTests` — `ConcertDraftService.CreateAsync` notifies BOTH artist + venue (2 entries per draft) but tests expect 1. Either change asserts to `Assert.Equal(2, …)` or split notifier behavior. Pre-existing, not from #11. |
| 17 | Frontend: HATEOAS-driven apply/accept | Read `actions` from response. Drop any `contract.type` branching for choosing endpoints. UI components for the apply form decide whether to render PM input based on the HATEOAS hint (or contract type — frontend can know contract type for rendering choices, just not for URL routing). |

#### Closed/superseded tasks

- **#3-6, #12** — strategy rewrites + `IConcertWorkflow` redefinition all landed in `9d2d2fc5`.
- **#8** — `ApplicationService.ApplyAsync` / `AcceptAsync` typed overloads in `9e0c590d`.
- **#9** — re-scoped from "split URLs" to "unify URLs". Final shape in `a13beac9`.
- **#18** — Executor → Dispatcher rename in `5b10ec27`.
- **#19** — `IApplyResolver` in `b4f58f06`.
- **#20** — family markers + `ICheckout` in `fa8a3277`.
- **#21** — `IConcertWorkflowStep` + `ConcertWorkflowCapabilityRegistry` in `e0e3753e`.

### Late addition (2026-05-02): #11 re-scoped to TPH refactor

Original #11 was "remove `BookingEntity.PaymentMethodId`, consolidate onto `Application.PaymentMethodId`." Started implementing it; user pushed back asking why payment storage feels out of place on Application. After lengthy design discussion the answer became clear: **half the data naturally belongs to the Booking aggregate, not the Application.**

Lifecycle test:
- VenueHire's PM exists from apply → accept → charged. Application aggregate's lifetime.
- DoorSplit/Versus's PM exists from accept → finish → charged. Booking aggregate's lifetime.

Putting both on Application is convenience storage that misaligns aggregate ownership. The principled refactor is TPH on **both** entities, with each PM column on the subtype that uses it.

Considered alternatives:
1. ~~Side entity `ApplicationPaymentAuthorization` (Role: Applicant/Acceptor)~~ — over-engineered; same data scattered when TPH cleanly splits it.
2. ~~TPH on Application alone~~ — handles VenueHire (`PrepaidApplication`) but leaves DoorSplit/Versus PM stuck on a nullable Application column. Half-fix.
3. ~~3 booking subtypes (one per "contract family")~~ — `StandardBooking` and `PrepaidBooking` (for VenueHire) are structurally identical (no fields); collapsing them to 2 subtypes is correct because TPH should reflect *structural variance* (does this booking carry settlement state) not contract heritage.
4. **TPH on both Application AND Booking, 2 subtypes each** ← chosen.

Final shape captured in `project_application_booking_tph.md` memory.

### Late addition (2026-05-02): drop stored-payment fallback

Discovered while resuming after the unified-URL revert: the existing `OnSession`/`OffSession` payment flows have a `?? TryGetPaymentMethodIdAsync(payerId)` fallback that turned a "saved card" UX choice into a null-PM-on-the-wire signal. Frontend `ImmediatePaymentSection.tsx` actively uses this — `selectSaved()` calls `onChange(null)` to mean "use my saved card." That null travels through to `ResolvePaymentMethodAsync` which falls back to the stored Stripe customer PM.

This created a **3rd semantic case** (no PM / explicit PM / stored fallback) that didn't fit the typed `AcceptAsync(int, string)` signature. Two options were considered:

1. Add a third method `AcceptWithStoredPaymentAsync` everywhere (3 service methods + 3 dispatcher methods + controller pattern-match)
2. Remove the fallback entirely; frontend always passes explicit PM

User picked (2). The fallback is solving a problem the frontend doesn't have — it already holds the saved card's PM ID for rendering. Passing it explicitly is one extra string in the request body and removes a whole class of nullable indirection from the backend.

This is now task #23 in the pending list. Until it lands, the saved-card UX on the current branch will throw `"This contract requires a payment method at accept"` because my unified controller routes `{ paymentMethodId: null }` to the by-confirmation path. **Don't release this branch without #23 also landed**.

### Key design decisions made along the way (reasoning preserved)

#### Bundle interfaces (`IStandardConcertWorkflow` / `IPrepaidConcertWorkflow`)

Initially I argued against these as "premature abstraction" and "re-introducing the smell." That was wrong. The original smell was a **shape-forcing parent with concrete nullable methods** (old `IConcertWorkflow.InitiateAsync(int, string?)` that VenueHire ignored). Pure marker composition (no methods) can't shape-force — strategies that don't fit simply don't declare it. User correctly pushed back; bundles are kept as cosmetic shorthand for the two current durable bundles. They're optional opt-in; a future contract with a different mix can declare individual capabilities directly.

#### `ISimpleApply` and `IApplyWithPaymentMethod` are markers (no methods)

Apply has **low variance** — three contracts have nothing to do at apply, one stores a PM. The PM storage is a single-line entity-mutation that fits naturally in `ApplicationService.ApplyAsync(int, string)` via the `ApplicationEntity.Create(artistId, oppId, pmId)` overload. No need for a strategy hook. The strategy interfaces are pure markers used by `IApplyResolver` as permission gates (`is ISimpleApply` / `is IApplyWithPaymentMethod`).

This is **different from** `IAcceptWithPaymentMethod` / `IAcceptByConfirmation` which DO have methods because Accept variance is real (different downstream services, different payment flows).

#### Dispatcher vs Resolver pattern

Two roles, two names, both internal:

- **Dispatcher** — does the work. Loads contract → resolves workflow → calls the capability method → returns the result. Caller treats the dispatcher's method as the operation. `AcceptanceDispatcher`, `CompletionDispatcher`, `SettlementDispatcher`.
- **Resolver** — returns the typed capability or throws. Caller does the work using the resolved capability. `IApplyResolver`. Used when the orchestration lives in the service (not the strategy).

For Apply, orchestration is in `ApplicationService` (validation, save, messaging, email — uniform across all contracts). Service uses `IApplyResolver` as a permission gate; strategy doesn't run any apply method. That's why Resolver fits.

For Accept/Settle/Finish/Checkout, the strategy IS where the work lives (different payment services, different revenue calcs). Dispatcher fits.

The naming distinction is intentional. Don't merge them.

#### `ConcertWorkflowCapabilityRegistry` — type-metadata only

Lives in `Concert.Application/Workflow/`. Singleton. Built at DI registration time from a `Dictionary<ContractType, Type>` populated alongside `AddKeyedScoped<IConcertWorkflow, T>(ct)`. Exposes `Has<TStep>(ct) where TStep : IConcertWorkflowStep` via `Type.IsAssignableTo(typeof(TStep))`.

**Critical: never instantiates a workflow.** Strategy constructors take heavy dependencies (`IPayerLookup`, `IConcertPaymentFlow`, etc.). For HATEOAS link generation on list endpoints we don't want to construct N workflows just to read flags. Pure type metadata answers "does FlatFeeConcertWorkflow implement ICheckoutable?" without ever calling `new FlatFeeConcertWorkflow(...)`.

#### Payment method storage — single nullable column on `ApplicationEntity`

Considered: side table `ApplicationPaymentMethods` with role enum (Applicant/Acceptor). Rejected because per-application the role is mutually exclusive (one party authorizes per contract, not both — `BookingEntity.PaymentMethodId` was only ever used by DoorSplit/Versus, never both with VenueHire's apply-time PM). Single nullable column on `ApplicationEntity` is the simplest cohesive home.

`BookingEntity.PaymentMethodId` will be deleted in #11 — its reads/writes move to `Application.PaymentMethodId`. (Booking only exists post-accept; Application exists from apply onward — so VenueHire can populate the column at apply-time.)

#### HTTP layer: unified URLs (post-revert in `a13beac9`)

The "no nullable-and-ignored params" rule applies to the **service layer**, not the HTTP boundary. The controller is inherently a translation point between untyped JSON and typed code; a single nullable field with one `is { } pmId` branch at the controller is a controlled bridge, not the smell.

```csharp
public async Task<IActionResult> Apply(int opportunityId, [FromBody] ApplyRequest? request = null)
{
    var application = request?.PaymentMethodId is { } pmId
        ? await applicationService.ApplyAsync(opportunityId, pmId)
        : await applicationService.ApplyAsync(opportunityId);
    return CreatedAtAction(nameof(GetById), new { id = application.Id }, application.ToResponse());
}
```

Service overloads stay typed. The branch is at the controller bridge only. The resolver in the typed service path validates capability — VenueHire-without-PM throws; FlatFee-with-PM throws.

For HATEOAS, the response embeds a single action URL plus a hint (e.g. `actions.apply.requiresPayment: true`) rather than varying the URL.

### Final hierarchy as landed

```
IConcertWorkflowStep (root marker)
├── IApplyable        ── ISimpleApply (marker), IApplyWithPaymentMethod (marker)
├── IAcceptable       ── IAcceptWithPaymentMethod (AcceptAsync(int, string)), IAcceptByConfirmation (AcceptAsync(int))
├── ICheckoutable     ── ICheckout (CheckoutAsync(int))
├── ISettleable       (SettleAsync(int))
└── IFinishable       (FinishAsync(int) → Task<IFinishOutcome>; slim to Task pending #13)

IConcertWorkflow : IApplyable, IAcceptable, ISettleable, IFinishable     (umbrella, marker only)

IStandardConcertWorkflow : IConcertWorkflow, ISimpleApply, ICheckout, IAcceptWithPaymentMethod
IPrepaidConcertWorkflow  : IConcertWorkflow, IApplyWithPaymentMethod, IAcceptByConfirmation

FlatFee/DoorSplit/Versus : IStandardConcertWorkflow
VenueHire                : IPrepaidConcertWorkflow
```

### Key files and locations

```
Concert.Application/Interfaces/
├── IConcertWorkflow.cs                       umbrella (marker only)
├── IConcertWorkflowStep.cs                   root marker
├── IApplyable.cs / IAcceptable.cs / ICheckoutable.cs    family markers
├── ISettleable.cs / IFinishable.cs           family markers WITH methods
├── ISimpleApply.cs / IApplyWithPaymentMethod.cs   apply variants (markers)
├── ICheckout.cs                              checkout (was IAcceptCheckout)
├── IAcceptWithPaymentMethod.cs / IAcceptByConfirmation.cs   accept variants
├── IStandardConcertWorkflow.cs / IPrepaidConcertWorkflow.cs   bundle composers
├── IApplyResolver.cs                         resolves apply capability
├── IAcceptanceDispatcher.cs                  dispatches accept (split into 2 methods)
├── ISettlementDispatcher.cs                  (was ISettlementExecutor)
├── ICompletionDispatcher.cs                  (was ICompletionExecutor)
└── IConcertWorkflowFactory.cs                returns IConcertWorkflow umbrella by ContractType

Concert.Application/Workflow/
└── ConcertWorkflowCapabilityRegistry.cs      type-only capability lookup

Concert.Infrastructure/Services/
├── Apply/
│   └── ApplyResolver.cs                      IApplyResolver impl
├── Acceptance/
│   └── AcceptanceDispatcher.cs               (was AcceptanceExecutor)
├── Completion/
│   └── CompletionDispatcher.cs               (was CompletionExecutor)
├── Settlement/
│   └── SettlementDispatcher.cs               (was SettlementExecutor)
├── Workflow/
│   ├── FlatFeeConcertWorkflow.cs             : IStandardConcertWorkflow
│   ├── DoorSplitConcertWorkflow.cs           : IStandardConcertWorkflow
│   ├── VersusConcertWorkflow.cs              : IStandardConcertWorkflow
│   ├── VenueHireConcertWorkflow.cs           : IPrepaidConcertWorkflow
│   ├── ConcertWorkflowFactory.cs
│   ├── UpfrontConcertService.cs              (downstream service, unchanged)
│   ├── DeferredConcertService.cs             (downstream service, has BookingEntity.PaymentMethodId reads — touched by #11)
│   ├── OnSessionConcertPaymentFlow.cs
│   └── OffSessionConcertPaymentFlow.cs
├── ApplicationService.cs                     ApplyAsync x2 + AcceptAsync x2 typed overloads
└── BookingService.cs                         StorePaymentMethod still wired — touched by #11

Concert.Domain/Entities/
├── ApplicationEntity.cs                      has PaymentMethodId column + two Create overloads
└── BookingEntity.cs                          has PaymentMethodId column to be deleted (#11)

Concert.Api/Controllers/
└── ApplicationController.cs                  unified apply + accept endpoints, controller-side null branch

Concert.Api/Requests/
└── ApplicationRequests.cs                    ApplyRequest(string? PaymentMethodId) + AcceptRequest(string? PaymentMethodId)

Concert.Infrastructure/Extensions/
└── ServiceCollectionExtensions.cs            DI registration; AddConcertWorkflow<T>(ct, dict) populates registry
```

### Gotchas to remember when resuming

1. **Build verification** — every commit so far has been verified with `dotnet build` (background) before committing. Keep that pattern.
2. **VenueHire `AcceptAsync`** loads `application.PaymentMethodId` from `IApplicationRepository.GetByIdAsync(applicationId)` (since the artist's PM was stored at apply-time). Throws `BadRequestException("VenueHire application has no payment method stored")` if null.
3. **`AcceptanceDispatcher`** kept the Dispatcher pattern (does the work + returns outcome) — Accept variance is real. **`IApplyResolver`** uses Resolver pattern (returns capability) — Apply orchestration lives in the service.
4. ~~**Saved-card UX is BROKEN on this branch until #23 lands.**~~ — **#23 has landed**, saved-card path now works (frontend passes explicit `savedCard.id`, backend's `IConcertPaymentFlow.ResolvePaymentMethodAsync` deleted along with its fallback). For history: the unified controller would have routed `{ paymentMethodId: null }` to the by-confirmation path and thrown.
5. **`IFinishOutcome` slim (#13)** is blocked on migrating `E2EEndpointExtensions.cs:42-52` (reads `deferred.Payment.TransactionId`) and `DevController.Complete` (returns `result.Value`). Both need to query payment intent separately first.
6. **Migration re-scaffold (#15)** must happen at the very end before integration tests run, after all entity changes settle.
7. **Per-CLAUDE.md** — never additive migrations; always re-scaffold `InitialCreate`.
8. **Per-memory** — never add `Co-Authored-By: Claude` trailer; never use unnecessary braces on single-statement `if/else`; prefer `is not null` over `is { }` capture when the captured value is just reused.

---

(Original plan content below remains as the original design intent; sections may refer to earlier names like `IPreAcceptCheckout`, `IAcceptCheckout`, `ISettle`/`IFinish`, etc. The progress checkpoint above is the source of truth for what's actually landed.)

---

This document captures the **WHY** of the eventual redesign and the high-level shape. Implementation details are deliberately left for whoever picks this up — fill in the gaps when the work actually happens.

---

## Why we're doing this

The current `IConcertWorkflow` strategy interface forces all four contract types (FlatFee, VenueHire, DoorSplit, Versus) into a single shape across all lifecycle methods. This works for three of them. It does not work for VenueHire, because VenueHire genuinely has **different signatures** at certain lifecycle steps:

- **Pre-accept checkout:** doesn't exist for VenueHire (the artist authorised at apply-time; the venue just confirms). Already addressed in `1a590507` via the `IAcceptCheckout` capability split.
- **Accept-time payment method:** FlatFee/DoorSplit/Versus take a `paymentMethodId` from the acceptor's just-completed checkout. VenueHire takes none from the caller — it loads the artist's stored PM from the application row. The current interface keeps `string? paymentMethodId` on `InitiateAsync` and VenueHire ignores it. **That is the smell driving this redesign.**
- **Apply-time payment method:** VenueHire requires the artist to authorise a card at apply-time; the others don't. There is no per-contract apply step today; it's a single `Apply` shape that doesn't carry payment intent.

The lateral move of pushing the smell up into the controller (nullable body field on the Accept request) only relocates it. The fundamental issue is that we have **shape variance across contracts at certain lifecycle steps**, and shape variance belongs in the type system, not as nullable-and-ignored params.

---

## The core distinction this plan rests on

**Shape variation vs behaviour variation.**

| | Shape variation | Behaviour variation |
|---|---|---|
| What varies | Method signature differs across impls | Signature uniform; what the method does inside differs |
| Example | VenueHire's Accept takes no PM; FlatFee's does | FlatFee's Settle is a state-transition no-op; DoorSplit's moves money |
| Where it lives | **Role interfaces** — type system encodes which strategies have which capability | **Method body** — single interface, polymorphic implementation |

Under this distinction:

- `Settle` and `Finish` are **universal** lifecycle hooks. Every contract goes through them. The fact that some are no-ops (state transition + audit event only, no money movement) does NOT justify removing the method. Lifecycle hooks are state-transition anchors, not behaviour wrappers — removing the method removes the transition point, forcing every consumer (UI, audit log, projection, downstream worker) to know which contracts emit which events. That's the spaghetti the role-interface design prevents.
- `Apply`, `Checkout`, and `Accept` have **shape variance** — different contracts genuinely take different inputs. Encode in role interfaces; the type system enforces which strategies have which capability.

---

## Proposed shape

### Lifecycle steps

```
Apply → Checkout → Accept → Settle → Finish
```

### Interface hierarchy: umbrella + family markers + capabilities

Three layers, designed for declarative intent and loose coupling:

```
                 IConcertWorkflow                ← umbrella (required minimum)
                       │
       ┌───────────┬───┴───────┬─────────────┐
       ▼           ▼           ▼             ▼
  IApplyable  IAcceptable  ISettleable  IFinishable    ← required family markers
                                  │            │           (Settleable + Finishable
                                  ▼            ▼            carry the actual method)
                              SettleAsync  FinishAsync

  ICheckoutable                                          ← OPTIONAL family marker
                                                            (NOT in IConcertWorkflow —
                                                             VenueHire skips checkout)

Concrete capabilities (extensions on top):
  Apply:    ISimpleApply              : IApplyable      (marker)
            IApplyWithPaymentMethod   : IApplyable      (OnAppliedAsync)
  Checkout: ICheckout                 : ICheckoutable   (CheckoutAsync)
  Accept:   IAcceptWithPaymentMethod  : IAcceptable     (AcceptAsync(appId, pmId))
            IAcceptByConfirmation     : IAcceptable     (AcceptAsync(appId))
```

Why a hierarchy at all: the markers don't add behaviour, but they make each interface's role declarative — `IApplyable` says "this is something to do with apply." Strategies declare the umbrella + the specific capabilities they fulfil; the type system enforces "every workflow has settle + finish + at least an apply variant + at least an accept variant." Checkout is opt-in.

**Apply step** — variance: does the artist supply a payment method at apply-time?
```csharp
public interface IApplyable { }
public interface ISimpleApply : IApplyable { }                   // marker — no method
public interface IApplyWithPaymentMethod : IApplyable
{
    Task OnAppliedAsync(int applicationId, string paymentMethodId);
}
```

**Checkout step** — variance: does it exist? Independent capability, not all contracts have one.
```csharp
public interface ICheckoutable { }
public interface ICheckout : ICheckoutable
{
    Task<AcceptCheckout> CheckoutAsync(int applicationId);
}
```

**Accept step** — variance: does the venue supply a payment method at accept-time?
```csharp
public interface IAcceptable { }
public interface IAcceptWithPaymentMethod : IAcceptable
{
    Task<IAcceptOutcome> AcceptAsync(int applicationId, string paymentMethodId);
}
public interface IAcceptByConfirmation : IAcceptable
{
    Task<IAcceptOutcome> AcceptAsync(int applicationId);
}
```

**Settle step** — universal lifecycle hook, behaviour varies in method body.
```csharp
public interface ISettleable
{
    Task SettleAsync(int bookingId);
}
```

**Finish step** — universal lifecycle hook, behaviour varies in method body. Worker-triggered, returns `Task` (not `Task<IFinishOutcome>`) — no caller consumes the outcome payload; failure is signalled by exception.
```csharp
public interface IFinishable
{
    Task FinishAsync(int concertId);
}
```

**Workflow umbrella** — every concert workflow must be each of these at minimum:
```csharp
public interface IConcertWorkflow : IApplyable, IAcceptable, ISettleable, IFinishable { }
```

### Strategies as capability composition

Each strategy declares the umbrella plus the specific capabilities it fulfils:

```csharp
public sealed class FlatFeeConcertWorkflow
    : IConcertWorkflow, ISimpleApply, ICheckout, IAcceptWithPaymentMethod { }

public sealed class DoorSplitConcertWorkflow
    : IConcertWorkflow, ISimpleApply, ICheckout, IAcceptWithPaymentMethod { }

public sealed class VersusConcertWorkflow
    : IConcertWorkflow, ISimpleApply, ICheckout, IAcceptWithPaymentMethod { }

public sealed class VenueHireConcertWorkflow
    : IConcertWorkflow, IApplyWithPaymentMethod, IAcceptByConfirmation { }
```

VenueHire deliberately does **not** implement `ISimpleApply` or `ICheckout`. The type system enforces that callers cannot ask VenueHire for a checkout or a payment-method-free apply.

### Dispatch is capability-based, not contract-type-based

Strategies registered keyed by `ContractType` in DI under the `IConcertWorkflow` umbrella — **one registration per strategy**:

```csharp
services.AddKeyedScoped<IConcertWorkflow, FlatFeeConcertWorkflow>(ContractType.FlatFee);
// ...same for DoorSplit, Versus, VenueHire
```

Resolvers/dispatchers fetch the umbrella, then cast to the specific capability:

```csharp
public async Task<ISimpleApply> ResolveSimpleAsync(int opportunityId)
{
    var contract = await contractLoader.LoadByOpportunityIdAsync(opportunityId);
    var workflow = workflowFactory.Create(contract.ContractType);  // returns IConcertWorkflow
    return workflow is ISimpleApply simple
        ? simple
        : throw new BadRequestException("This contract requires a payment method at apply");
}
```

`SettleAsync` and `FinishAsync` need no cast — they're on the umbrella's required `ISettleable` / `IFinishable` parents.

### HTTP layer mirrors capability split

Each capability gets its own endpoint with its own request DTO carrying exactly its params. **No nullable-and-ignored fields anywhere.** The frontend is kept contract-type-blind via HATEOAS — the application/opportunity response embeds the URL of the action that applies to it:

```jsonc
{
  "id": 66,
  "contract": { "type": "VenueHire", ... },
  "actions": {
    "apply":  { "href": "/applications/with-payment", "method": "POST" },
    "accept": { "href": "/applications/66/accept-confirmation", "method": "POST" }
  }
}
```

Frontend POSTs to `application.actions.accept.href`; never reads `contract.type` to choose an endpoint.

The dispatch pattern repeats at three layers, all resolved at **registration time**, never call time:
- Backend — DI keyed resolution by `ContractType`.
- Frontend — component map (registration table) by `ContractType`.
- Wire — embedded action URLs in resource responses.

---

## Schema implication: where VenueHire's apply-time PM lives

Three options, ordered by cleanliness:

1. **Side table** `ApplicationPaymentAuthorization(applicationId, paymentMethodId)` — one row per VenueHire application, zero for others. Cleanest data model; one extra join. Generalises naturally if a second contract type ever needs apply-time PM.
2. **TPH application subtypes** — `VenueHireApplication : Application { string ArtistPaymentMethodId }`. Type system enforces it; more EF mapping ceremony.
3. **Nullable column** `Application.ArtistPaymentMethodId` — pragmatic, one column unused for three of four contract types. Smell tolerated.

**Recommendation:** side table. The data only exists for one variant; modelling as an optional row matches the domain better than a sometimes-null column.

---

## Settle/Finish trigger model

Worker-only, **not** mixed (webhook-for-some, worker-for-others). Mixed reintroduces contract-type knowledge into the trigger layer, which is exactly the leak this design avoids.

- **Worker triggers settle for every contract** regardless of whether money actually moves. FlatFee/VenueHire's `Settle` is a no-op-with-state-change; DoorSplit/Versus's does the off-session charge.
- **Stripe webhook** becomes "tell the worker the settlement charge succeeded" rather than "be the trigger for settlement."
- Same for `Finish` — worker-driven on concert completion.

---

## Open questions / gaps to fill in when this work happens

- **Does `ISimpleApply` need to return `Application` directly, or should both `Apply` interfaces return a richer `ApplyResult` carrying validation outcomes?** Current `IApplicationService.ApplyAsync` returns `ApplicationDto` — confirm shape pre-redesign.
- **Off-session payment flow.** `UpfrontConcertService` currently injects `[FromKeyedServices(PaymentSession.OnSession)] IConcertPaymentFlow` hard-coded. VenueHire's Accept needs off-session. Either parameterise the keyed flow into `UpfrontConcertService.InitiateAsync`, OR the VenueHireStrategy bypasses `UpfrontConcertService` and uses its own off-session path. Decide before VenueHire's `Accept` is implemented.
- **3DS handling.** Off-session charges can return `requires_action` for 3DS. `IAcceptOutcome` likely needs a `RequiresPayerAction` variant so the artist can come back and complete on-session. Audit `IAcceptOutcome` impls when this work starts; add the variant if missing.
- **Decline UX.** When VenueHire's off-session charge declines (insufficient funds, expired card), what does the venue see, and what state does the application go to? "Couldn't charge artist; application kept open; artist notified" is the working assumption — confirm with product before building.
- **Capability registration in DI.** Each strategy implements multiple role interfaces. The keyed registration needs to expose all of them under the same key so `GetRequiredKeyedService<ISettle>(contractType)` and `GetRequiredKeyedService<IPreAcceptCheckout>(contractType)` both resolve to the same strategy instance. Confirm `IServiceCollection.AddKeyedSingleton`/`AddKeyedScoped` semantics support this; if not, register a factory.
- **HATEOAS implementation.** Does the existing `ApplicationResponse` mapper in `Concert.Api/Mappers/` have a place to add `actions`? Or does this need a sibling `ApplicationLinks` type? Decide once existing response shape is reviewed.
- **Migration mechanics.** Every controller, executor, test, and DI registration touches this. Bundle with another rewrite (Duende migration, payment-flow rewrite, or whichever next contract type forces a refactor anyway).
- **Naming.** `IPreAcceptCheckout` vs `IAcceptCheckout` (current name on master) — pick one, rename everywhere. `IAcceptByConfirmation` vs `IConfirmationOnlyAccept` — pick one. Names should land before the work begins to avoid cosmetic churn.
- **`IFinishOutcome` slimming.** Current `IFinishAsync` returns `Task<IFinishOutcome>` carrying a `PaymentResponse` for deferred paths. Production consumers (`ConcertFinishedFunction` Azure worker, `ConcertWorkflowModule` cross-module facade) discard the payload and only check `result.IsFailed`. **However**, `E2EEndpointExtensions.cs` and `DevController.Complete` DO read the payload — E2E pulls `deferred.Payment.TransactionId` to assert on settlement. So removing the payload requires moving those test/dev consumers to a different read path (e.g., query `PaymentResponse` by booking id from the E2E endpoint, or assert on a domain event). Once those are migrated, the role-interface signature can land as `Task Finish(int concertId)` cleanly. `Settle` already returns `Task` in the worker path; verify `ISettlementExecutor` doesn't surface a payload before mirroring the same shape.

---

## Migration trigger

**Do not migrate now.** Cost-benefit on a working codebase is bad — touches every controller, executor, test, and DI registration. The current strategy + `IAcceptCheckout` split (`1a590507`) is a sane intermediate; the one tolerated smell is `paymentMethodId` ignored by VenueHire on `InitiateAsync`.

**Pick this up when one of these is true:**

1. **A third or fourth weird-shaped contract type is being added.** Adding subscription residencies or ticket-share splits with bespoke signatures will make the current design tip over; bundle the redesign with that work.
2. **A bigger rewrite is happening anyway.** Duende migration, payment-flow rewrite, anything that already touches the workflow layer — fold this in.
3. **VenueHire's `paymentMethodId` smell becomes worse than the migration.** Quantify in the moment.

Until then, treat this document as the captured design intent and the existing `IConcertWorkflow` + `IAcceptCheckout` split as the live state.

---

## Pattern name (for whoever picks this up)

This is **role-interface composition** — strategy + Interface Segregation Principle + open/closed, fully decomposed. Sometimes called "trait-based design" by Rust/Scala folks. The generalisable lesson:

> Design around the variance you have, not the uniformity you wish you had.

Nullable-and-ignored params, optional methods on a base interface, and "this impl ignores X" comments are signals that the abstraction is forcing variance into a uniform shape it doesn't have. Either narrow the interface (capability split) or change the pattern (role-interface composition, sum types, commands).
