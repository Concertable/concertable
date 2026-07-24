# Money value type — a Kernel `Money(decimal, Currency)`, publish-first

> **Why:** money is raw `decimal` in the domain/service layer and hand-cast to `long` pence at **8+ sites**
> with an independent **truncating** `(long)(x*100)` at each — no owner for the conversion, the rounding
> rule, or the `gross/fee/total` arithmetic. Decision + rationale (hand-roll, not NodaMoney; carry
> `Currency` from day one; GBP ×100 for now) live in
> [`api/Concertable.Shared/TECH_DEBT.md`](../api/Concertable.Shared/TECH_DEBT.md) → *"No money type"*.
> This plan is the execution. It **enables** [`b2b/PLATFORM_COMMISSION.md`](./b2b/PLATFORM_COMMISSION.md):
> the platform fee's `gross + fee` is built on `Money`, not a fourth `(long)(x*100)` site.

## The type (Kernel, additive)

```csharp
public enum Currency { Gbp = 826 }   // ISO-4217 numeric; only GBP for now, shaped for the ISO table later

public readonly record struct Money(decimal Amount, Currency Currency)
{
    // GBP ×100 today; a per-currency exponent (JPY ×1, BHD ×1000) is a later concern — see TECH_DEBT.
    public long ToMinorUnits() => (long)Math.Round(Amount * 100m, 0, MidpointRounding.AwayFromZero);
    public static Money FromMinorUnits(long minor, Currency ccy) => new(minor / 100m, ccy);
    public static Money Zero(Currency ccy) => new(0m, ccy);

    public static Money operator +(Money a, Money b) => Same(a, b) with { Amount = a.Amount + b.Amount };
    public static Money operator -(Money a, Money b) => Same(a, b) with { Amount = a.Amount - b.Amount };
    // same-currency guard THROWS on mismatch — never silently coerce (TECH_DEBT: don't default away a failure)
}
```

- **`decimal`-backed, never `double`.** Immutable, value-equal (record struct).
- **One rounding rule, owned here:** round to the minor unit *before* summing; `AwayFromZero`, matching
  Stripe. The naked truncating `(long)` casts are deleted, not moved.
- **Currency carried but not yet branched on:** GBP ×100 hardcoded; the *shape* (schema, wire, type) is
  future-proofed, the per-currency machinery is not built (revisit NodaMoney when multi-region is real).

## Boundaries this crosses (why it's multi-PR)

`Money` lives in the **published `Concertable.Kernel` package**; every service compiles against the package,
not the source (the carve). Adding the type is **additive** (safe, one publish). Changing a **published
signature** to use it — the `Concertable.Payment.Client` interfaces B2B consumes — is **breaking** and needs
its own publish-first cut-over. So the migration is sequenced by those boundaries, not by convenience.

**Inventory (from TECH_DEBT):** service-layer/client `decimal` signatures — `ISettlementAmountResolver`,
`deal.Fee`, the payment clients — plus the pounds↔pence sites: `Concertable.Payment.Infrastructure` ×6
(`EscrowService`, `ManagerPaymentService`, `StripePaymentIntentClient`, `StripeTransferClient`,
`StripeAccountClient`, `PaymentManager`; + the `FakeStripePaymentIntentClient` double),
`Concertable.Payment.Seed/E2EStripeAccountClient`, `Customer.Ticket.Infrastructure/TicketService`.

## Phases

Every phase: `dotnet build api/Concertable.slnx` (0 errors) + affected unit + integration via
`integration-debug`. Model-touching phases end with `./initial-migrations.ps1` from `api/`. Publish-gated
phases follow the merge → `publish-packages` → `chore/platform-sync-*` to green discipline (root `CLAUDE.md`).

### Phase 1 — `Money` + `Currency` in Kernel (additive; publish-gated) ✅ DONE

Added `Concertable.Kernel.ValueObjects.Money` (`readonly record struct`) + `Currency` enum with unit tests:
minor-unit rounding at the boundary (`£12.345 → 1235`), `FromMinorUnits` round-trip, `+`/`-` arithmetic,
**currency-mismatch throws `DomainException`**, `Zero`. Nothing consumes it yet — the pure additive expand.
Gate green: `dotnet build api/Concertable.slnx` 0 errors + Kernel unit tests pass.

**Gate:** build + `Concertable.Kernel` unit tests. `[skip-e2e]` (no behaviour). **Then:** merge → publish →
follow `chore/platform-sync-*` to green so the new pin is on every service before Phase 2.

### Phase 2 — Payment adopts `Money` internally + gRPC wire → int64 minor units

Route all Payment money through `Money`, deleting the 6 (+fake) truncating casts. The gRPC wire moves from
**decimal-as-invariant-string → `int64` minor units + `Currency`** (proto regenerates client *and* server
together). **The published C# client interfaces (`IEscrowClient`/`IManagerPaymentClient`) keep their current
`decimal` params in this phase** — the adapter owns the `Money`↔wire mapping — so **B2B needs no source
change** and this phase carries no consumer break. Entities keep storing `long` minor units, but only ever
via `Money.ToMinorUnits()`. EF: map money as minor-unit `long` + `Currency` (value conversion / `OwnsOne`).

**Gate:** build + `Payment` unit + `B2B` integration + **E2E** (money movement changes representation, not
amounts — but it's the payment path, so let the queue run it). Re-scaffold. Publish → sync.

### Phase 3 — Platform fee, built on `Money` *(rides Phase 2; see PLATFORM_COMMISSION.md)*

Execute [`b2b/PLATFORM_COMMISSION.md`](./b2b/PLATFORM_COMMISSION.md) on top of `Money`: `gross + fee` is
`Money.operator+`, the snapshot is `Money.ToMinorUnits()`, no new conversion site. This is where money
actually changes (the fee). Kept as its own plan/PR; **this plan's Phase 2 is its prerequisite** so the fee
never touches raw `decimal` arithmetic.

### Phase 4 — Customer adopts `Money` (`TicketService`, `E2EStripeAccountClient`)

Same internal treatment for the remaining pounds↔pence sites outside Payment. Independent of Phase 3.

**Gate:** build + `Customer` unit + integration + E2E (ticketing path). Re-scaffold if a model changes.

### Phase 5 — Contract: published signatures `decimal` → `Money` (breaking; publish-first)

The final half of expand/contract — remove the last raw-`decimal` money signatures:
- `ISettlementAmountResolver.ResolveGrossAsync` → `Money`, `deal.Fee` → `Money` (B2B-internal, same-PR).
- `Concertable.Payment.Client` interfaces `decimal` → `Money` (**breaking** — B2B migrates in the sync PR).

**Grep gate (definition of done):** `rg -nE "\(long\)\(.*\* 100|/ 100m?\b" api` returns **zero** outside
`Money` itself; no money-typed `decimal` parameter remains on a resolver/client signature. Publish → sync.

## Sequencing decision (the one call to make)

**Recommended: Money first (P1→P2), then the fee rides it (P3).** The whole point of the investigation was
that the fee's arithmetic + conversion belong in `Money`; building the fee on `decimal` first and migrating
later is double work and re-introduces the smear temporarily. The critical path to revenue is
**P1 → P2 → P3**; P4/P5 are non-blocking cleanup that can land whenever.

**Alternative (only if revenue urgency beats tidiness):** ship the fee on `decimal` now (it's 2 dp,
conversion-safe) and treat the whole of this plan as the follow-up that absorbs it — accepting that P2/P5
then rework the fee code. Not recommended, but noted.

## Branch/PR shape

`Money` is a Kernel concern, not the platform-fee feature — so **its own branch** (e.g.
`Refactor/MoneyValueType`), landed publish-first, *before* the `Feature/PlatformCommission` fee PR rebases
onto the new pin. This plan doc can ride the current branch (docs are branch-exempt); execution starts on a
fresh branch off `origin/master`.

## Risks

| # | Risk | Mitigation |
|---|---|---|
| M1 | Money migration merged before its platform-sync pin lands → consumers can't see `Money` | Publish gate is a hard stop between P1 and P2 (and before P3 rebases). Confirm the sync PR merged + pin bumped first. |
| M2 | Rounding change silently alters existing amounts | GBP is 2 dp; every current amount is already exact at ×100, so `AwayFromZero` rounding is a no-op on real data. Assert round-trip in P1 unit tests; E2E on P2/P4 covers the money paths. |
| M3 | gRPC wire change (string→int64) breaks a consumer | The wire is internal to Payment's client+server (both regenerate together); B2B's C# calls are unchanged because the adapter keeps `decimal` until P5. All services rebuild on the P2 sync. |
| M4 | EF value-conversion of `Money` mis-maps or breaks queries | Map to the existing `long` minor-unit column + a `Currency` column; keep the stored representation identical to today (only the C# type changes). Integration tests on read/write paths. |
