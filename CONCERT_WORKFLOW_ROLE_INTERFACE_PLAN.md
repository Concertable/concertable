# Concert Workflow — Role-Interface Composition Plan

**Status:** Deferred. Captured 2026-05-02. Current intermediate state lives at commit `1a590507` (`refactor/checkout-capability-split` branch — `CheckoutAsync` lifted off `IConcertWorkflow` into `IAcceptCheckout` capability).

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
