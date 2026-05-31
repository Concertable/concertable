# Payment SeedCatalog + Simulator

> **FULLY SUPERSEDED (2026-05-31)** by [`plans/PAYMENT_SEED_REFLECTION_REFACTOR.md`](./PAYMENT_SEED_REFLECTION_REFACTOR.md),
> which was **implemented**: `Payment.Seed.Contracts` and `Payment.Seed.Simulator` were deleted, and the
> seed-only payment-derived state (B2B `TicketsSold`, Customer `Ticket` rows) is now reflection-seeded on
> each consumer's side. Payment owns no seed catalog or simulator. This whole document is historical.

Adopt the B2B seed pattern for Payment, because Payment is a producer of cross-service
integration events (`PaymentSucceededEvent`, `PaymentFailedEvent`) that both B2B and Customer
subscribe to (`B2B.Web/Program.cs:138-139`, `Customer.Web/Program.cs:103-104`).

Mirror of: `Concertable.B2B.Seed.Contracts` (SeedCatalog) + `Concertable.B2B.Seed.Simulator`
(see `api/Concertable.B2B/Concertable.B2B.Seed.Simulator/CLAUDE.md`).

## CORRECTION (2026-05-31) — `Payment.Seed.Contracts` is an anti-pattern; this supersedes the plan below

The premise "adopt the B2B seed pattern for Payment" was **wrong**. Payment is **not** a producer in
the B2B sense, so it must not own a seed *content* catalog. Everything below — and the
"Resolved — producer pattern, Payment stays agnostic" note at the bottom — is **superseded by this**.

**Why it's wrong.** Payment is an agnostic conduit: `PaymentSucceededEvent.Metadata` is an opaque
`IReadOnlyDictionary<string,string>` Payment never reads. The keys `type` / `concertId` / `quantity`
are stuffed in by the *initiator* (customer checkout) and read by the *consumer*
(`TicketSaleProcessor`, `TicketPaymentProcessor`). So a `Payment.Seed.Contracts` that owns a catalog of
**ticket purchases** (`PaymentSeedSpec.Ticket(concertId: 13, …)`) parks consumer-domain meaning in a
service designed to know nothing about tickets. Using literal `13` instead of importing
`B2B.Seed.Contracts` made it *reference*-agnostic but not *content*-agnostic — cosmetic.

**Why B2B's shared seed lib is NOT the same** (the discriminator is *ownership of meaning* + direction,
not "is it shared"): B2B owns concerts/venues/artists, and Customer is genuinely downstream — its read
models are projections of B2B's events. So `Customer → B2B.Seed.Contracts` is the legitimate
consumer→producer arrow, the same direction as production. Payment owns no ticket meaning, so
`Payment.Seed.Contracts` has no such justification.

**Scope of the simulator (narrowed to its real job).** It exists only for the two payment-driven
**projections** that can't be direct-seeded under the events-only rule:
- B2B `ConcertReadModel.TicketsSold` (written only by `TicketSaleProcessor`),
- Customer `TicketEntity` (written only by `TicketPaymentProcessor`).

Booking settlement/escrow/verify states are **B2B-owned write-model** (`BookingEntity.Status`,
direct-seeded via `BookingFactory`), so `PaymentSeedSpec.Settlement` / `Escrow` / `Verify` are dead code.

**Corrective plan.**
1. `Payment.Contracts.PaymentSucceededEvent` stays — the only Payment-owned piece (Payment is the real
   producer of the event *type*).
2. Move seed-payment *authoring* to the consumer/initiator side — the Customer seed owns the
   ticket-purchase meaning and already references `B2B.Seed.Contracts` for concert ids; it builds the
   `PaymentSucceededEvent`s directly (referencing only `Payment.Contracts`).
3. The simulator becomes a **seed-fixture replayer** referencing `Payment.Contracts` (event type) + the
   consumer seed (content) — not "Payment's simulator." Re-home it out of the Payment namespace.
4. Delete `Concertable.Payment.Seed.Contracts` (spec + catalog, including the 3 dead factories).
5. **Unchanged:** dev/E2E still need the replay (real Payment never emits events for seed data); the
   integration tier still direct-inserts via `TicketTestSeeder`; events still flow through the real
   handlers.

Tracked in `api/Concertable.Payment/TECH_DEBT.md`.

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

## Progress (2026-05-31)

All stages built; umbrella `dotnet build` green (0 errors).

- **Stage 1 ✅** `Concertable.Payment.Contracts` created. `PaymentSucceededEvent`/`PaymentFailedEvent`
  → `Concertable.Payment.Contracts.Events` (namespace matches `Events/` folder, per sibling
  `*.Contracts` convention). `TransactionTypes`/`PaymentSession` → `Concertable.Payment.Contracts`
  (root). All consumers repointed. `B2B.Concert.Application` used no Payment.Domain types → Domain ref
  dropped entirely. `Payment.Domain` no longer needs `Messaging.Contracts` (events used it) → removed.
- **Stage 2 ✅** `Concertable.Payment.Seed.Contracts` is **pure vocabulary**: `PaymentSeedSpec`
  (`Ticket`/`Settlement`/`Escrow`/`Verify` factories) + `SeedSpecMappers.ToPaymentSucceededEvent()`.
  **Refs `Payment.Contracts` only — knows nothing about B2B.** (Payment is a foundational adapter; a
  Payment library depending on B2B inverts the graph. The concrete cross-service payment scenarios live
  in the simulator, the composition host — see Stage 3.)
- **Resolved open Q1 — seed payment set:** Only the **3 Customer-1 ticket purchases** are populated
  (`Upcoming FlatFee` / `Past DoorSplit` / `Past FlatFee`). These are the genuinely event-derived state:
  B2B `TicketsSold` is only set by `TicketSaleProcessor`, Customer tickets by `TicketPaymentProcessor`.
  Booking settlement/escrow/verify states are **direct-seeded** in `SeedState`, so replaying them would
  re-drive `SettleAsync`/`VerifyAsync` against settled rows — left out; factories exist for tests.
- **Resolved open Q2 — AppHosts:** Both `B2B.AppHost` and `Customer.AppHost` run **real Payment**
  (`AddPaymentWeb`+`AddPaymentWorkers`); the umbrella does too. So the B2B-simulator rationale (consumer
  without producer) does **not** apply. But real Payment only publishes on a real Stripe webhook, never
  for seed bookings/tickets — so there is **no double-publish**. The Payment simulator is registered in
  **both standalone AppHosts** (`AddPaymentSeedingSimulator`) to drive seed ticket events through the
  real handler path. **Not** in the umbrella (StripeCli is wired there — real test payments produce them).
  This is a deliberate divergence from the plan's "only hosts without Payment" wording; see the simulator
  `CLAUDE.md` for the reasoning.
- **Stage 3/4 — REVERSED after the agnostic audit.** I had built `Concertable.Payment.Seed.Simulator`
  + `AddPaymentSeedingSimulator` wiring, but that simulator referenced `B2B.Seed.Contracts` — i.e. a
  Payment-namespaced project pointing **up** at B2B, the exact arrow `PAYMENT_AGNOSTIC_AUDIT.md` had us
  delete from production. So it's been **removed** (project, extension, `PaymentSeedingSimulator`
  constant, both AppHost registrations + csproj refs, both `.slnx` entries). Umbrella build green.
- **Stage 5 ✅** `PaymentTestSeeder` stays payout-only. B2B/Customer integration tests keep their
  existing `MockWebhookSimulator`-driven scenarios. `TECH_DEBT.md` clean.

### Resolved — producer pattern, Payment stays agnostic   ⚠️ SUPERSEDED — see CORRECTION at top

Settled after the agnostic audit. Payment is the **producer**; it owns its seed library + simulator,
both B2B-free (consumer → producer, never reverse):

- **`Payment.Seed.Contracts`** — `SeedCatalog.Payments` (the 3 Customer-1 ticket payments) +
  `PaymentSeedSpec` + `ToPaymentSucceededEvent()`. Refs `Payment.Contracts` + shared `Seed.Identity`
  only. Concert ids are **literal ints** (13/12/10) — Payment treats `concertId` as an opaque foreign
  int, so it declares them by fixture convention rather than referencing `B2B.Seed.Contracts`.
- **`Payment.Seed.Simulator`** — refs **only** `Payment.Seed.Contracts` (+ messaging/ServiceDefaults).
  Publishes `SeedCatalog.Payments` as `PaymentSucceededEvent`s on startup, then exits. No B2B anywhere.
- **Wired** into `B2B.AppHost` + `Customer.AppHost` via `AddPaymentSeedingSimulator`; **not** the umbrella
  (StripeCli drives real payments there). Needed even though real Payment runs in every host — real
  Payment only emits on a Stripe webhook, never for seed data, so no double-publish.
- **Pattern documented** in `ARCHITECTURE.md` ("Producer seed libraries point downward only") +
  pointer in `SEEDING_CONVENTIONS.md`, so the consumer→producer direction isn't re-litigated.

Umbrella build green.
