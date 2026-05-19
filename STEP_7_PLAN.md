# Phase 2 Step 7 — Customer extraction plan

> **Companion to** [MICROSERVICE_STEPS.md](MICROSERVICE_STEPS.md). Step 7 = first cross-process boundary.
>
> **Status:** Not started. Plan drafted 2026-05-19 after Phase 1 wrap.

---

## Sequencing decision

MICROSERVICE_STEPS.md orders Step 7 ("extract to own host + own DB") **before** Step 8 ("MassTransit on in-memory transport"). That creates a gap: the moment Customer leaves the monolith process, its existing in-proc event handlers have nothing to subscribe to:

- `ConcertProjectionHandler` listens to `ConcertChangedEvent`
- `CustomerProfileCreationHandler` listens to `CustomerRegisteredEvent`
- `ReviewCreatedDomainEventHandler` raises `ReviewSubmittedEvent`
- `TicketPaymentProcessor` consumes `PaymentSucceededEvent` via the keyed-dispatcher trick

**Decision:** Step 7 = Customer code lives entirely in Customer modules and communicates with the rest of the system via the existing `IIntegrationEvent` / `IIntegrationEventHandler` contracts. The contracts stay; only the transport changes later (current in-proc dispatch → MT in-mem at Step 8 → RabbitMQ/ASB at Step 13). Step 7 is the **code organisation**, not the transport swap. Outbox/bus deferred.

---

## Sub-steps (execute in this order)

### 7b — Expand `ConcertChangedEvent` payload *(load-bearing; do first)* ✅ DONE

**File:** `api/Modules/Concert/Concertable.Concert.Contracts/Events/ConcertChangedEvent.cs`

Fields added:
- `string Name` (concert name — needed by Customer.Ticket snapshot)
- `int ArtistId`
- `string ArtistName`
- `int VenueId`
- `string VenueName`
- `Guid PayeeUserId`
- `string ContractType` (string literal — Customer doesn't depend on `Concertable.Contract.Contracts`)

Publisher side: `ConcertChangedDomainEventHandler` injects `IConcertRepository`, calls `GetFullByIdAsync` to pull artist/venue read-model navs + entity's `ContractType` enum, computes `PayeeUserId` inline (`VenueHire → Artist.UserId`, all others → `Venue.UserId`). `IContractLoader` not needed — `ContractType` already lives on `ConcertEntity`.

Consumer side: Customer.Concert's `ConcertEntity` gained snapshot fields + new `Create/Update` signatures; `ConcertProjectionHandler` writes the full snapshot.

### 7a — Lift Customer.Ticket off the B2B nav chain ✅ DONE

**File:** `api/Concertable.Customer/Modules/Ticket/Concertable.Customer.Ticket.Infrastructure/Services/TicketService.cs`

Current pain (lines 71–73, 112, 192–207):
```csharp
var b2bConcert = await b2bConcertRepository.GetFullByIdAsync(...);
var contract = await contractLoader.LoadByConcertIdAsync(...);
var payeeUserId = ticketPayee.Resolve(b2bConcert, contract);
// ...
b2bConcert.Booking.Application.Opportunity.Venue.Name
b2bConcert.Booking.Application.Artist.Name
```

After 7b, all of this is on `customerConcert` (Customer.Concert read model). Drop:
- `IB2BConcertRepository` (alias for `Concertable.Concert.Application.Interfaces.IConcertRepository`) injection
- `IContractLoader` injection
- `ITicketPayee` and the two impls (`ArtistTicketPayee`/`VenueTicketPayee`) — routing decision baked into the event

`BuildTicket` reads venue/artist names from `customerConcert`; payee resolves to `customerConcert.PayeeUserId`.

### 7c — Drop `IPaymentSucceededProcessor` cross-IVT trick ✅ DONE

**File:** `api/Modules/Concert/Concertable.Concert.Application/Interfaces/IPaymentSucceededProcessor.cs` — DELETE.

Rationale: this `internal` interface in B2B Concert has two impls, keyed by `TransactionTypes.{Ticket, Booking}`, dispatched by a B2B Concert handler. Customer.Ticket implements the Ticket variant via `InternalsVisibleTo` (cross-module IVT — the kludge).

Once Customer is its own service, the polymorphic dispatcher dies: each service subscribes to `PaymentSucceededEvent` independently and filters by `metadata.type`. No shared abstraction needed.

- B2B Concert keeps its impl as a plain `IIntegrationEventHandler<PaymentSucceededEvent>` (filters `type == "Booking"`)
- Customer.Ticket flips `TicketPaymentProcessor` to a plain `IIntegrationEventHandler<PaymentSucceededEvent>` (filters `type == "Ticket"`)
- Dispatcher class in B2B Concert deleted

### 7d — Trim Customer.Ticket → Payment refs ✅ DONE

**Files:**
- `api/Concertable.Customer/Modules/Ticket/Concertable.Customer.Ticket.Application/Concertable.Customer.Ticket.Application.csproj`
- `api/Concertable.Customer/Modules/Ticket/Concertable.Customer.Ticket.Infrastructure/Concertable.Customer.Ticket.Infrastructure.csproj`

Drop:
```xml
<ProjectReference Include="..\..\..\..\Modules\Payment\Concertable.Payment.Application\..." />
<ProjectReference Include="..\..\..\..\Modules\Payment\Concertable.Payment.Domain\..." />
```

Keep `Concertable.Payment.Contracts` only. `IPaymentModule` facade already exists. Swap `Payment.Application.DTOs/Responses` and `Payment.Domain` usings for Contracts equivalents — if any type isn't on Contracts yet, promote it there.

### 7e — Drop Customer.Ticket → Contract.Contracts ref ✅ DONE

**File:** `api/Concertable.Customer/Modules/Ticket/Concertable.Customer.Ticket.Application/Concertable.Customer.Ticket.Application.csproj`

Drop:
```xml
<ProjectReference Include="..\..\..\..\Modules\Contract\Concertable.Contract.Contracts\..." />
```

`ContractType` arrives as a string on `ConcertChangedEvent` (7b); Customer never reaches for `IContract`.

### 7f — Acknowledge Customer.Review → Customer.Ticket intra-Customer dep

**File:** `api/Concertable.Customer/Modules/Review/Concertable.Customer.Review.Infrastructure/Repositories/ConcertReviewRepository.cs`

Currently uses `ITicketRepository` to check "did this user own a ticket?". Both modules ship in the same Customer service post-extraction — keep as-is. No change. Noting it here so it's not flagged as a violation in 7h.

### 7g — Customer DB cutover

**Aspire wiring:** add a `Concertable.Customer.Database` resource in `Concertable.AppHost`. Pass connection string to `Concertable.Customer.Web`.

**Per-module DbContexts** (`Customer.Concert/Ticket/Review/Profile.Infrastructure/Data/XDbContext.cs`) bind to `ConnectionStrings:CustomerDb` instead of `DefaultConnection`. Each Customer module's `AddXModule` extension reads the new key.

**Migrations:** per repo convention (`CLAUDE.md` — `./initial-migrations.ps1`), nuke + re-scaffold all `InitialCreate`s. Customer's 4 contexts get their migrations against the new Customer DB; B2B contexts stay on the monolith DB.

**Dev/test data:** drop + reseed via existing `XDevSeeder`/`XTestSeeder` per module. No production data migration (learning project).

### 7h — Final csproj audit

Every `Concertable.Customer.*.csproj` should reference only:
- `Concertable.Contracts`
- `Concertable.Kernel`
- Sibling `Concertable.Customer.*` projects (within the Customer service)
- NuGet packages

Build green. Run integration tests.

---

## Risks / open questions

- **7b is the load-bearing PR.** Schema change to `ConcertChangedEvent` breaks every consumer in one go. Find all with `Grep "IIntegrationEventHandler<ConcertChangedEvent>"`.
- **7g data migration:** existing rows in the monolith DB don't migrate themselves. Plan = clean-slate drop + reseed for the learning project. If real preservation needed, that's a separate side-quest.
- **`PayeeUserId` on the event** assumes the payee never changes for a given concert. Verify against B2B Concert's payee-mutation semantics — if it can change post-publish, Customer's read model must update on the next `ConcertChangedEvent`.
- **No bus until Step 8.** Until then Customer.Web shares the in-proc event bus + AppHost wiring. Step 7 is *code* portability, not deployment.

---

## Out of scope (Step 8+ work)

- MassTransit on in-memory transport
- Transactional outbox (`EntityFrameworkOutbox`)
- Idempotent consumers / inbox state
- Service-to-service auth (`client_credentials` via Duende)
- Physical process split — Customer.Web stops sharing `Concertable.AppHost`'s in-proc bus

These all land bundled in the merged "Step 7+8" PR series.
