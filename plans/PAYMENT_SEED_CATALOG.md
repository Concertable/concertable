# Payment SeedCatalog + Simulator

Adopt the B2B seed pattern for Payment, because Payment is a producer of cross-service
integration events (`PaymentSucceededEvent`, `PaymentFailedEvent`) that both B2B and Customer
subscribe to (`B2B.Web/Program.cs:138-139`, `Customer.Web/Program.cs:103-104`).

Mirror of: `Concertable.B2B.Seed.Contracts` (SeedCatalog) + `Concertable.B2B.Seed.Simulator`
(see `api/Concertable.B2B/Concertable.B2B.Seed.Simulator/CLAUDE.md`).

## Prerequisite finding

`PaymentSucceededEvent` / `PaymentFailedEvent` live in `Concertable.Payment.Domain.Events`.
Consumers reference `Concertable.Payment.Domain` directly:
- `Concertable.B2B.Concert.Infrastructure.csproj`
- `Concertable.B2B.Concert.Application.csproj`
- (Customer equivalents — confirm during Stage 1)

There is no `Concertable.Payment.Contracts`. Cross-service code compiling against Payment.Domain
is the same boundary violation we just removed from the seeder. Must be fixed first.

## Distinction (per B2B simulator CLAUDE.md)

- The **SeedCatalog (Contracts)** holds only data for events that CROSS boundaries →
  for Payment that's the `PaymentSucceeded/Failed` payloads.
- **Payout accounts** are Payment-INTERNAL, provisioned in dev by `ManagerRegisteredHandler`
  reacting to Auth's `CredentialRegisteredEvent`. They are NOT cross-boundary catalog data —
  they're the Payment analogue of B2B's Bookings/Applications. They stay in a Payment-internal
  seeder (integration-test direct insert keyed off shared `SeedUsers`; dev = event-driven).
  This is the `PaymentTestSeeder` we already moved onto `SeedUsers`.

## Already done this session (uncommitted) — do NOT redo

- `B2B.Web/Program.cs` + `Customer.Web/Program.cs`: `AddPaymentClient(...)` now guarded behind
  `if (!builder.Environment.IsEnvironment("Testing"))`.
- `B2B` integration fixture `ApiFixture.cs`: added `services.AddSingleton<SeedCatalog>();`
  (B2B's `SeedState` needs it; `TimeProvider` already registered via `AddInfrastructure`).
- `PaymentTestSeeder.cs`: now depends only on `PaymentDbContext`, seeds the 4 manager payout
  accounts keyed off shared `SeedUsers.VenueManagerId(n)`/`ArtistManagerId(n)` (no B2B/Customer
  SeedState). Customer payout account dropped.
- `Concertable.Payment.Infrastructure.csproj`: dropped `B2B.Seed.Infrastructure` +
  `Customer.Seed.Infrastructure` refs; added `Shared/Seed/Concertable.Seed.Identity`.
- `Concertable.Payment/TECH_DEBT.md`: closed the "reaches into B2B/Customer internals" item.

This already removed the seeder boundary violation. The work below is the larger producer-side
pattern the user explicitly wants.

## Separate pre-existing blocker (NOT this plan, but blocks B2B integration green)

B2B integration tests now boot but fail seeding with:
`INSERT ... FK "FK_Opportunities_VenueReadModels_VenueId" ... concert.VenueReadModels is empty`.
B2B seeds `Opportunities` that FK-reference `concert.VenueReadModels` (a read-model projection,
written by `VenueChangedEvent` handlers, never seeded directly). The B2B seeders don't populate
the venue projections first. Distinct from Payment; track/fix separately.

## Solutions to update when adding projects

`.slnx` files: `api/Concertable.Payment/Concertable.Payment.slnx`, `api/Concertable.slnx`
(umbrella). New Payment projects must be added to both. Consumer rewiring may need
`api/Concertable.B2B/Concertable.B2B.slnx` + `api/Concertable.Customer/Concertable.Customer.slnx`.

## Stages

### Stage 1 — Create `Concertable.Payment.Contracts`, relocate events
New project `Concertable.Payment.Contracts` (refs: `Concertable.Messaging.Contracts` only;
`ImplicitUsings` + `Nullable` enabled, net10.0).

Move these 4 types out of `Concertable.Payment.Domain` into `Concertable.Payment.Contracts`
(new namespace `Concertable.Payment.Contracts`):
- `Events/PaymentSucceededEvent.cs` — `record(string TransactionId, IReadOnlyDictionary<string,string> Metadata)`, `[MessageType("concertable.payment.payment-succeeded.v1")]`
- `Events/PaymentFailedEvent.cs` — `record(string TransactionId, string? FailureCode, string? FailureMessage, IReadOnlyDictionary<string,string> Metadata)`, `[MessageType("concertable.payment.payment-failed.v1")]`
- `TransactionTypes.cs` — static class consts Ticket/Settlement/Escrow/Verify (consumers read `metadata["type"]` against these)
- `PaymentSession.cs` — enum OnSession/OffSession (in gRPC facade signatures)

Repoint references from `Concertable.Payment.Domain` → `Concertable.Payment.Contracts`:
- `Concertable.B2B.Concert.Infrastructure.csproj` (handlers: SettlementPaymentProcessor etc.)
- `Concertable.B2B.Concert.Application.csproj`
- `Concertable.Payment.Client.csproj` (PaymentSession in facade signatures)
- `Concertable.Payment.Application.csproj`, `Concertable.Payment.Infrastructure.csproj` (publisher)
- `Concertable.DataAccess.Application.csproj` (references Payment.Domain — check what for)
- Customer: `Customer.Ticket.Infrastructure` uses the events (TicketPaymentProcessor /
  TicketPaymentFailedProcessor) — its csproj does NOT directly ref Payment.Domain, so it gets
  the type transitively; trace and add explicit `Payment.Contracts` ref.
- B2B test fixture `MockWebhookSimulator.cs` (`using Concertable.Payment.Domain.Events`).
Keep Payment.Domain ref where Domain types other than these 4 are still used.
Add `Concertable.Payment.Contracts` to the `.slnx` files. Build green.

### Stage 2 — `Concertable.Payment.Seed.Contracts` (SeedCatalog)
- New project (refs: `Concertable.Payment.Contracts`, `Concertable.Seed.Identity`).
- `SeedCatalog` (class named `SeedCatalog`, namespace `Concertable.Payment.Seed.Contracts`) +
  `PaymentSeedSpec` records + `SeedSpecMappers.ToPaymentSucceededEvent()`.
- Contents = canonical seed payments: derive from which seed bookings/tickets are "paid"
  (study B2B SeedState settled bookings + Customer ticket seeds + the 4 consumer processors:
  Settlement/Escrow/Verify/TicketSale + Customer payment handler). Metadata must carry
  `type` + `bookingId` exactly as the processors expect (see `SettlementPaymentProcessor:28,34`).

### Stage 3 — `Concertable.Payment.Seed.Simulator` (Worker)
- Mirror `B2B.Seed.Simulator`: `SeedEventPublishingService : BackgroundService` publishes every
  catalog payment event on startup, then `lifetime.StopApplication()`.
- Refs: Payment.Seed.Contracts, Messaging, ServiceDefaults. NO Domain/Infrastructure.
- Add a `CLAUDE.md` mirroring B2B's.

### Stage 4 — AppHost wiring
- Register the simulator only in standalone consumer AppHosts that run WITHOUT Payment.
  Determine which: does standalone `B2B.AppHost` run Payment? Customer.AppHost already runs
  Payment (per B2B sim CLAUDE.md line 15) so likely doesn't need it. Confirm before wiring.
- NOT in umbrella `Concertable.AppHost` (real Payment double-publishes).

### Stage 5 — Reconcile seeders + fixtures
- PaymentTestSeeder stays payout-only on `SeedUsers` (already done).
- Anything in B2B/Customer integration tests that needs payment-event-derived state builds it
  from the Payment SeedCatalog (mirroring `VenueProjectionTestSeeder`), OR keeps current direct
  seeding if it's their own internal state. Decide per case.
- Update `TECH_DEBT.md` files.

### Stage 6 — Verify
- Build all. Run B2B + Customer integration suites. Then E2E regress.

## Open questions to resolve while building
1. Exact set of seed payments (Stage 2) — needs study of consumer processors + seed scenarios.
2. Which AppHosts run consumers without Payment (Stage 4).
3. Whether `TransactionTypes`/`PaymentSession` are cross-service (Stage 1 scope).
