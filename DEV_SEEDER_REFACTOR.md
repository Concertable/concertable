# Dev Seeder → SeedData Refactor

## Problem

Dev seeders use **hardcoded literal IDs** to wire up cross-module FKs (e.g.
`OpportunityFactory.Create(..., contractId: 50)`). This works only if the
target table assigns IDs in array-insert order — but with **TPT inheritance**
(Contracts + 4 subtype tables) EF batches inserts grouped by entity type,
so Contracts.Id 50 is **not** the 50th array element. The result: opportunities
point at the wrong contracts, ticket-payee/settlement-amount logic computes off
the wrong contract type, and a unique index on `Opportunities.ContractId`
explodes when literal IDs collide with `seed.X.Id`-derived ones.

## The pattern (what Test seeders already do)

`ContractTestSeeder` doesn't depend on which DB ID EF assigns. It writes the
entity reference into `SeedData` and downstream consumers reference it by
*entity*, not by integer:

```csharp
// ContractTestSeeder
seed.DoorSplitAppContract = DoorSplitContractEntity.Create(70m, PaymentMethod.Cash);
context.Contracts.AddRange(seed.DoorSplitAppContract, ...);
await context.SaveChangesAsync(ct);

// (Concert)TestSeeder — would consume via:
OpportunityFactory.Create(venueId, period, contractId: seed.DoorSplitAppContract.Id);
```

After SaveChanges, EF backfills `seed.DoorSplitAppContract.Id` with the real DB
ID (whatever it is) and consumers get the right value automatically.

## What the dev seeders do today

- `ContractDevSeeder` creates 59 contracts as an anonymous array, then
  `AddRange` + `SaveChanges`. **Most contracts are not named on `SeedData`**.
- `ConcertDevSeeder` references contracts via literal IDs:
  `contractId: 50`, `contractId: 51`, etc. These literals assume insert-order =
  ID-order, which is **wrong** for TPT.
- Other dev seeders (`UserDevSeeder`, `VenueDevSeeder`, `PaymentDevSeeder`,
  `MessagingDevSeeder`) likely have the same shape — populate cross-module
  references via literals rather than via `SeedData`.

## Fix shape (apply per dev seeder)

For every entity that downstream dev seeders need to reference:

1. Add a property on `SeedData` (already exists for many — `FlatFeeAppContract`,
   `DoorSplitAppContract`, `PostedFlatFeeAppContract`, etc. for contracts).
   Add new properties for any seeded entity referenced by a *different* dev
   seeder.
2. In the dev seeder, assign the entity into `seed.X = ...` *before* AddRange.
3. In the consuming dev seeder, replace the literal int with `seed.X.Id`.

For contracts specifically — the named `seed.X` properties on `SeedData`
already cover the contracts the failing E2E tests care about
(`ConfirmedAppContract`, `AwaitingPaymentAppContract`, `DoorSplitAppContract`,
`VersusAppContract`, `PostedFlatFeeAppContract`, `PostedDoorSplitAppContract`,
`PostedVersusAppContract`, `PostedVenueHireAppContract`, `FlatFeeAppContract`,
`VenueHireAppContract`). Tom has already named them in `ContractDevSeeder`.

## Why my recent edit failed (and was reverted)

I half-applied the pattern: replaced 10 literal `contractId: N` references in
`ConcertDevSeeder` with `seed.X.Id` *but left the other 49 as literals*. With
TPT, the named `seed.X.Id` evaluated to type-grouped IDs (e.g. `Id=17`) that
**collided with literal `contractId: 17` elsewhere** → unique-index violation
on `Opportunities.ContractId` → API failed to start → all 12 tests timed out
on /health.

`ConcertDevSeeder` is now reverted to all-literal references. The fix has to
be applied **consistently** — either all 59 references go through `seed.X.Id`
(which means naming all 59 contracts in SeedData), or `ContractDevSeeder` must
guarantee array-order IDs (per-entity SaveChanges loop).

## Recommended path

**Option A — name everything (matches test seeder pattern, scales to other
seeders):**

- Promote *all* 59 contracts to named `SeedData` properties (or a single
  `IReadOnlyList<ContractEntity> Contracts`).
- Update `ConcertDevSeeder`'s 59 `contractId: N` references to
  `seed.Contracts[N-1].Id` (or `seed.SpecificContract.Id`).
- Apply the same pattern wherever a dev seeder cross-references another module's
  entity by integer.

**Option B — force array-order IDs in `ContractDevSeeder` only:**

```csharp
foreach (var c in contracts)
{
    context.Contracts.Add(c);
    await context.SaveChangesAsync(ct);
}
```

Per-entity SaveChanges sidesteps EF's TPT type-grouping. Downstream literal
references stay valid. Smaller change but doesn't fix the underlying smell —
dev seeders still depend on array-order = ID-order, which any future TPH/TPT
mapping change can break again.

Option A is the right long-term fix and matches what test seeders already do.

## Open issues this still doesn't address

These are *separate* bugs surfaced by the E2E run, not seeder issues:

1. **TicketPurchase VenueHire wrong destination** — even with correct
   contracts, `ArtistTicketPayee.Resolve` returns `concert.Booking.Application
   .Artist.UserId` but ends up routing to VenueManager1's stripe account.
   Likely `ArtistReadModel` projection wiring or the artist→user mapping. Check
   after seeders are fixed.
2. **ConcertDraft FlatFee/VenueHire timeout polling /e2e/payment-intent** —
   `SettlementTransactions` table never gets a row for upfront-flow accept
   payments. `SettlementTransactionHandler` was rewritten to call `LogAsync`
   instead of `CompleteAsync`, but a SQL deadlock (`Process ID 75 deadlocked …
   chosen as deadlock victim`) keeps killing the LogAsync inside the webhook
   handler. The deadlock trace is in the prior api[0] logs.

## Loggers in place (keep)

Production-toned `ILogger<T>` was added at the boundaries that matter for this
flow. Don't strip these:

- `DoorSplitConcertWorkflow` / `VersusConcertWorkflow` — log share calc
- `DeferredConcertService` / `UpfrontConcertService` — log payer/payee/amount
  before Stripe
- `TicketService.PurchaseAsync` — log resolved payee + contract type
- `ManagerPaymentModule` / `CustomerPaymentModule` — log charge boundary
- `PaymentService` — log Stripe response (intent id, amount, status)
- `SettlementExecutor` — log booking + contract type
- `SettlementPaymentService` — log settlement webhook receipt
- `SettlementTransactionHandler` — log transaction record write
