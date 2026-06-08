# Refactor: kill the Payment seed simulator, reflection-seed pre-sold tickets

**Do this in the current uncommitted changeset — do NOT commit first.** The Payment seed catalog/simulator
is an anti-pattern; committing it would bake it into history.

## Why (the decision, settled)

- **Payment is an agnostic adapter.** `PaymentSucceededEvent.Metadata` is an opaque `IReadOnlyDictionary<string,string>`
  Payment never reads; `type`/`concertId`/`quantity` are set by the initiator and read by the consumers
  (`TicketSaleProcessor`, Customer `TicketPaymentProcessor`). So `Concertable.Payment.Seed.Contracts` owning a
  catalog of **ticket purchases** parks consumer-domain meaning in Payment — wrong. Payment keeps only
  `Payment.Contracts.PaymentSucceededEvent` (the event type).
- **The only thing the Payment simulator produced** is two payment-driven projections that can't be direct-seeded
  under the events-only rule: B2B `ConcertEntity.TicketsSold` and Customer `TicketEntity`. Real Payment never emits
  for seed data (only live Stripe webhooks), and you can't buy a past-dated concert — so this is *inherently
  unreproducible historical state*. The right tool is **reflection-seeding on each consumer's own side**, not a
  Payment-owned simulator.
- **The B2B↔Customer simulator pattern STAYS.** `Concertable.B2B.Seed.Simulator` is untouched: Customer runs without
  B2B's server, so it must replay B2B's events to build projections. That's a peer-data-service decoupling and is
  legitimate. Payment had no equivalent justification (it always runs; it's an adapter).
- **Documented convention exception:** reflection-setting `TicketsSold` / direct-seeding `TicketEntity` violates the
  letter of `SEEDING_CONVENTIONS.md` (their only prod writer is an event handler). Justified ONLY because past-dated
  sales can't be reproduced by the real flow. Document it so nobody re-introduces a simulator to "fix" it.

## Facts gathered (so you don't re-discover)

- Reflection API: `Concertable.Seed.Identity.Extensions.EntityReflectionExtensions` — `New<T>()` + `.With(nameof(Prop), value)`.
- `ConcertEntity.TicketsSold` is `private set`, written only by `IncrementTicketsSold` (called only by `TicketSaleProcessor`).
- B2B concert seed factory: `api/Concertable.B2B/Concertable.B2B.Seed.Infrastructure/Factories/ConcertFactory.cs` —
  `Create(...)` already does `ConcertEntity...With(nameof(ConcertEntity.Id), id)...`. Add a `ticketsSold` param + `.With(nameof(ConcertEntity.TicketsSold), ticketsSold)`.
- Concert specs live in `api/Concertable.B2B/Concertable.B2B.Seed.Contracts/SeedCatalog.Concerts.cs` (`ConcertSeedSpec.Create(id, name, price, ...)`). Add an optional `ticketsSold` to `ConcertSeedSpec.Create` + the spec record, thread it into `ConcertFactory`.
- B2B settlement E2E that needs the seed sales: `api/Concertable.B2B/Tests/E2ETests/.../Payments/ConcertFinishedTests.cs`
  - DoorSplit: asserts `1400` = 1 ticket × £20 × 70%  → the **Past DoorSplit** concert needs `TicketsSold = 1`.
  - Versus: asserts `11400` = £100 + 1 ticket × £20 × 70% → the **Past Versus** concert needs `TicketsSold = 1`.
  - (FlatFee / VenueHire completion tests don't depend on ticket sales.)
  - Map `SeedState.PastDoorSplitBooking` / `PastVersusBooking` → their concert spec entries in `SeedCatalog.Concerts.cs`
    by name and set `ticketsSold: 1` there. NOTE: the old Payment catalog covered concerts 13/12/10 but **omitted the
    Versus concert** — a real gap this refactor closes.
- Customer ticket seeding: `TicketTestSeeder` (`ITestSeeder`, Order 5) inserts `SeedState.Tickets` if empty
  (`api/Concertable.Customer/Modules/Ticket/.../Data/Seeders/TicketTestSeeder.cs`). Registration helper
  `AddCustomerTicketTestSeeder` in that module's `Extensions/ServiceCollectionExtensions.cs`.
- Blast radius: nothing outside the two Payment seed projects consumes `Payment.Seed.Contracts`.

## Steps

### 1. B2B — reflection-seed `TicketsSold`
- `ConcertSeedSpec` (+ `ConcertSeedSpec.Create`): add optional `int ticketsSold = 0`.
- `ConcertFactory.Create`: accept `ticketsSold`, add `.With(nameof(ConcertEntity.TicketsSold), ticketsSold)`.
- `SeedCatalog.Concerts.cs`: set `ticketsSold: 1` on the **Past DoorSplit** and **Past Versus** concert entries.
- Verify `ConcertFinishedTests` door-split (1400) + versus (11400) pass off seed.

### 2. Customer — `TicketDevSeeder` (dev/E2E replacement for the simulator's tickets)
- Add `TicketDevSeeder : IDevSeeder` mirroring `TicketTestSeeder` (insert `SeedState.Tickets` if empty). Match the
  `IDevSeeder` shape used by the existing Customer dev seeders (e.g. `PreferenceDevSeeder`) incl. `Order`.
- Add `AddCustomerTicketDevSeeder` next to `AddCustomerTicketTestSeeder`.
- Register it in `Customer.Web/Program.cs` dev-seeding block (alongside `AddCustomerPreferenceDevSeeder`).
- Customer `SeedState` already builds the 3 tickets from `B2B.Seed.Contracts` concerts — unchanged. (B2B server not required; it's fixture data + the B2B.Seed.Simulator events, both already present.)

### 3. Delete the Payment seed machinery
- Delete dirs: `api/Concertable.Payment/Concertable.Payment.Seed.Contracts/` and `.../Concertable.Payment.Seed.Simulator/`.
- `api/Concertable.AppHost.Shared/Constants.cs`: remove the `PaymentSeedingSimulator` resource-name constant.
- `api/Concertable.AppHost.Shared/DistributedApplicationBuilderExtensions.cs`: remove `AddPaymentSeedingSimulator<T>`.
- `api/Concertable.B2B/Concertable.B2B.AppHost/Program.cs`: remove the `AddPaymentSeedingSimulator(...)` line.
- `api/Concertable.B2B/Concertable.B2B.AppHost/Concertable.B2B.AppHost.csproj`: remove the `Payment.Seed.Simulator` `ProjectReference`.
- `api/Concertable.Customer/Concertable.Customer.AppHost/Program.cs`: remove the `AddPaymentSeedingSimulator(...)` line.
- `api/Concertable.Customer/Concertable.Customer.AppHost/Concertable.Customer.AppHost.csproj`: remove the ref.
- `api/Concertable.slnx` and `api/Concertable.Payment/Concertable.Payment.slnx`: remove `Payment.Seed.Simulator` (+ `Payment.Seed.Contracts`) entries.
- **Do NOT touch** `Concertable.B2B.Seed.Simulator` / `AddB2BSeedingSimulator` — that pattern stays.
- **Do NOT remove** the `reg.SubscribeTo<PaymentSucceededEvent>()` / `PaymentFailedEvent` subscriptions in `B2B.Web`/`Customer.Web` — real Payment still emits these for *live* purchases in E2E (Stripe), and the processors handle them.

### 4. Docs
- `api/docs/SEEDING_CONVENTIONS.md`: add the documented exception (reflection-seed for inherently-unreproducible
  historical state; example = past-dated ticket sales / `TicketsSold`).
- `api/CLAUDE.md`: remove the note I added that says "the `Payment.Seed.Simulator` is required in B2B/Customer dev+E2E" — no longer true.
- `api/ARCHITECTURE.md` "Producer seed libraries point downward only": rewrite so the rule is — peer **data** services
  (B2B↔Customer) that don't run each other's servers use a `*.Seed.Simulator`; an agnostic **adapter** (Payment) does
  NOT own a seed catalog, and consumers reflection-seed any derived state. Drop Payment as the "producer simulator" example.
- `plans/PAYMENT_SEED_CATALOG.md`: mark fully superseded by this refactor.
- `api/Concertable.Payment/TECH_DEBT.md`: mark the HIGH "Payment.Seed.Contracts parks consumer-domain data" item resolved.
- `api/Concertable.B2B/TECH_DEBT.md`: resolve the LOW "Seed `TicketsSold` depends on the Payment seed simulator" item (decided: reflection-seed).

### 5. Build + verify
- `dotnet build api/Concertable.slnx` green (use the PowerShell tool, single-line).
- If feasible, run the B2B settlement E2E (`ConcertFinishedTests`) + a Customer ticket/E2E to confirm seed tickets land.

## Guardrails
- B2B must not reference Customer (the `TicketsSold` set is B2B-side only).
- Payment must end up owning **zero** ticket/concert knowledge — only `Payment.Contracts.PaymentSucceededEvent`.
- Keep `Concertable.B2B.Seed.Simulator` intact.
