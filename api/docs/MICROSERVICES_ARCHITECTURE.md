# Concertable Microservices Architecture

> **Status (2026-05-23):** Phases 1–7 ✅ landed — B2B / Customer / Search / Payment extracted, ASB wired, Outbox/Inbox, projections for Venue/Artist/Concert/Ratings. Outstanding: inverse-direction events (`TicketPurchasedEvent`, `ConcertSalesProjection`), Customer `ConcertEntity` behavioural shape, B2B `Modules/User` TPH unwind, B2B Workers ASB transport. See per-service `TECH_DEBT.md` files.
>
> **Goal:** Migrate from the current modular monolith to an event-driven microservices architecture that separates B2B (venue ↔ artist booking + settlement) from Customer (ticket marketplace), with a centralised Payment adapter service.
>
> **Constraint that changed the plan:** This is a learning side project. The Nov 2026 launch in `LAUNCH_PLAN.md` is aspirational, not a hard deadline. Skill development (event-driven architecture, transactional outbox, sagas, OpenTelemetry, Service Bus operations) is an explicit goal alongside any eventual deployment.
>
> **Companion docs:** [LAUNCH_PLAN.md](/plans/LAUNCH_PLAN.md), [MARKETPLACE_PLAN.md](/plans/MARKETPLACE_PLAN.md), [B2B_LAUNCH_CHECKLIST.md](/plans/B2B_LAUNCH_CHECKLIST.md), [USER_MODEL_PLAN.md](/plans/USER_MODEL_PLAN.md).

---

## 1. Motivation

The current codebase is a .NET modular monolith with strong module boundaries (`IXModule` facades, per-module `DbContext`, in-process domain events). It deploys as one API host (`Concertable.Web`) + Workers + Auth (Duende IS).

Two observations drove the direction change:

1. **B2B and Customer are genuinely different bounded contexts**, not just different audience UIs over the same data:
   - **Customer-side Concert** = "an event I can buy tickets for: when, where, how much, seats remaining."
   - **B2B-side Concert** = "a contracted booking with a workflow state machine (Posted → Applied → Accepted → Verified → Finished → Settled), contract terms, settlement obligations, compliance snapshot."
   - Customer doesn't care about transition validators; B2B doesn't care about ticket buyer identity for most of its work.

2. **Deployment isolation, blast radius, and legal scope all favour separation.** Customer marketplace carries direct consumer liability (CMA, consumer rights, refunds) that B2B does not. If only B2B is deployed (current plan), Customer code shouldn't be in the production binary.

The modular monolith already enforces the *internal* boundary correctly. The microservices step pushes the boundary to the *deployment and data* layers.

## 2. Service inventory

| Logical service | Category | Composition (deployable hosts) | DB | Owns |
|---|---|---|---|---|
| **B2B** | Data | `Concertable.B2B.Web` + `Concertable.B2B.Workers` | B2B SQL | Venue, Artist, Concert (workflow shape), Contract, Booking, Application, Opportunity, Settlement, Tenant, Messaging, manager/admin profiles. Manager + venue/artist SPA endpoints. Workers run settlement triggers, lifecycle transitions, payout reconciliation. |
| **Customer** | Data | `Concertable.Customer.Web` | Customer SQL | Tickets (with `AvailableTickets`), Preferences, Reviews, `UserEntity` (customer profile). Single deployable — service is write-light; ASB consumers run in-process. No Workers host needed. |
| **Search** | Read projection | `Concertable.Search.Web` + `Concertable.Search.Workers` | Search SQL | `ArtistSearchModel`, `VenueSearchModel`, `ConcertSearchModel`. Browse/autocomplete/header/detail reads. Web host read-only, sync-callable from B2B and Customer SPAs. Workers consume events from B2B and Customer to populate projections. |
| **Payment** | Adapter | `Concertable.Payment.Web` + `Concertable.Payment.Workers` | Payment SQL | PayoutAccount, StripeCustomer refs, payment intent / transfer / refund ledger. Sole receiver of Stripe webhooks. Workers process webhooks and reconciliation. |
| **Auth** | Adapter | `Concertable.Auth` (Duende IS, single host) | Duende DB | OIDC issuer. Identity authority (`sub`, password hash, email-verification). No role claims. Also issues service tokens for service-to-service auth (§5.5). |
**Non-deployables:**

| | | |
|---|---|---|
| `Concertable.AppHost` | Aspire orchestrator | Local dev only. |
| `Concertable.Contracts` | csproj (future NuGet if poly-repo) | The *wire*: event contracts, cross-service DTOs, static lookups (Genre). |
| `Concertable.Kernel` | csproj (future NuGet if poly-repo) | The *framework*: `BaseEntity`, `Period`, `IUnitOfWork<TContext>`, `DbContextBase`, validation helpers. `IEmailSender` abstraction. `ICurrentUser` + claim helpers (`Identity/`). |
| `Concertable.Email` | csproj shared library | `IEmailSender` concrete impls (SendGrid, SMTP, Fake). Both B2B and Customer import this library directly — no network hop. See §4.7. |
| Azure Service Bus | Managed broker | Async event substrate. |

**Service count:** 5 logical services. 9 deployable hosts at end-state (Auth + B2B.Web + B2B.Workers + Customer.Web + Search.Web + Search.Workers + Payment.Web + Payment.Workers + Aspire AppHost dev only). Email/Notification is a shared library (`Concertable.Email`), not a service.

## 3. Service categories

A distinction that became load-bearing during the design conversation:

| Category | Sync calls from other services OK? | Examples |
|---|---|---|
| **Adapter service** — wraps an external concern (Stripe, identity provider), owns operational state, exposes operations rather than data fetches | **Yes** — same shape as calling the external thing directly | Payment, Auth |
| **Data service** — owns canonical domain data that consumers think of as their own | **No** — distributed-monolith antipattern. Project via events. | B2B (Venue, Artist, Concert workflow), Customer (Tickets, Reviews, CustomerProfile) |
| **Read projection service** — event-fed denormalised store, read-only from outside | Sync reads OK (it's a projection, not an authority) | Search |

This is the rule the rest of the architecture hangs on. **A read projection service that ever owns a write becomes a data service and loses sync-call privilege.** Search stays read-only; updates flow exclusively through canonical owners' events.

## 4. Data ownership

| Concept | Source of truth | Projected to | Sync mechanism |
|---|---|---|---|
| Venue | B2B DB (`VenueEntity`, full ~25 fields with Tenant, PayoutAccount, ComplianceContext) | Search DB (`VenueSearchModel` for browse/details) | `VenueChangedEvent` via bus |
| Artist | B2B DB (`ArtistEntity`, full) | Search DB (`ArtistSearchModel`) | `ArtistChangedEvent` via bus |
| Concert (workflow shape) | B2B DB (`ConcertEntity`, lean — `BookingId`, `ContractType`, `CurrentStage`, `DatePosted`, `TotalTickets` capacity only) | Search DB (`ConcertSearchModel` — buyable/browse view: name, period, price, banner, avatar, genres, images, location, rating) | `ConcertChangedEvent` via bus |
| `TotalTickets` (capacity) | B2B DB (set by venue when posting) | Search DB (display); Customer DB (so Customer can compute remaining) | Carried on `ConcertChangedEvent` |
| `AvailableTickets` (remaining) | Customer DB — decremented atomically with `TicketEntity` insert | Search DB (refresh for "X tickets left" UX) | `TicketPurchasedEvent` / `TicketRefundedEvent` via bus |
| Booking, Contract, Application, Opportunity, Settlement | B2B DB | Not projected | — |
| Ticket | Customer DB (`TicketEntity` — id, sub, concertId, QR, purchaseDate) | B2B DB (`ConcertSalesProjection`: concertId, soldCount, grossRevenue) for dashboards + settlement math | `TicketPurchasedEvent`, `TicketRefundedEvent` via bus |
| Customer→concert Review | Customer DB (`ReviewEntity`) | B2B DB (`ConcertRatingProjection`: artist/venue rating aggregates) — feeds Search rating display via re-publication | `ReviewSubmittedEvent`, `ReviewUpdatedEvent` via bus |
| Venue↔artist Review *(future)* | B2B DB — separate table from customer reviews | Not projected to Customer | — |
| Customer profile, Preferences | Customer DB (`CustomerProfileEntity`) | Not projected to B2B | — |
| Manager/Admin profile | B2B DB (`VenueManagerEntity`, `ArtistManagerEntity`, `AdminEntity` — flat tables post-TPH unwind, each carrying Auth `sub`) | Not projected to Customer | — |
| **Identity** (`sub`, email, password hash, email-verification state) | Auth DB (Duende IS) | Each service stores `sub` on its own profile row | Sync token validation only; no role claim in token |
| `PayoutAccountEntity` (Stripe Connect refs for venues + artists) | Payment DB | Neither — accessed via Payment sync API | Sync call |
| `StripeCustomerEntity` (Stripe Customer refs for venues + artists + customers) | Payment DB | Neither — accessed via Payment sync API | Sync call |
| Payment intent / transfer / refund ledger | Payment DB | Status events published to bus | `PaymentSucceededEvent`, `TransferCompletedEvent`, etc. |

**Notes:**

- `PayoutAccountEntity` and `StripeCustomerEntity` are **not** partitioned between B2B and Customer. Venues and artists are both payers (FlatFee, VenueHire) and payees (DoorSplit, ticket revenue) depending on contract type, so they need both Connect accounts and Stripe Customer refs. Both live in the Payment service.
- **Inverse-direction projections.** Most events flow B2B → Search/Customer for browse. `TicketPurchasedEvent`, `TicketRefundedEvent`, `ReviewSubmittedEvent` flow Customer → B2B for analytics and settlement math. The bus is bidirectional; the canonical-owner rule is what's fixed.
- **Authentication vs authorization.** Auth (Duende) issues tokens with `sub` + audience only — no role claim. Each service derives the persona's role from its own data: B2B inspects the manager-profile subtype/membership for the `sub`; Customer treats any token from its audience as `Customer`. This separates "who are you" (Auth) from "what can you do here" (per-service authorization). `ICurrentUser` and the claim helpers live in `Concertable.Kernel` (`Identity/`), consumed per-service.
- **Customer/Search projection split.** Customer DB only needs to project what *Customer's own write paths* require (e.g., `TotalTickets` to compute remaining). Browse/detail reads route through Search. This avoids duplicating projections across Customer and Search.

## 4.5 Entity migration map

Where current monolith code lands post-extraction. Sequence matters — most of this should happen *in-process* before any service split, so the cross-process move is a packaging change rather than a refactor.

### Concert module decomposition

`ConcertEntity` is currently a god-entity: B2B workflow fields *and* customer-display fields on one row.

| Current field on `ConcertEntity` | Post-split owner |
|---|---|
| `BookingId`, `ContractType`, `CurrentStage`, `DatePosted` | B2B (workflow shape, canonical) |
| `TotalTickets` (capacity) | B2B (set by venue when posting) → Search + Customer projections |
| `AvailableTickets` (remaining) | **Customer** — moves off B2B's `ConcertEntity` entirely (see "Customer's ConcertEntity" below) |
| `Name`, `About`, `Price`, `BannerUrl`, `Avatar`, `Period`, `Location` | B2B (canonical for editing) → Search (browse projection) |
| `ConcertGenres`, `Images` | B2B (canonical) → Search (browse projection) |
| `Tickets` collection | **Customer** — `TicketEntity` moves out of `Concert.Domain` |

### Customer's `ConcertEntity` (new — Customer's bounded-context view)

Customer gets its own `ConcertEntity` (same name as B2B's, different shape) — **not** a projection table named `ConcertTicketAvailability` or similar. Within Customer's bounded context, this row *is* a Concert: it has behavior (`DecrementAvailability(int)`), invariants (`AvailableTickets >= 0`, `AvailableTickets <= TotalTickets`), and a lifecycle.

```csharp
// Customer.Domain.Entities.ConcertEntity
public class ConcertEntity : BaseEntity<int> {
    public int TotalTickets { get; private set; }       // sourced from ConcertChangedEvent
    public int AvailableTickets { get; private set; }    // Customer owns; decremented on purchase

    public void DecrementAvailability(int qty) { ... }
    public void RestoreAvailability(int qty) { ... }     // on refund
}
```

**Status (2026-05-23):** Currently implemented as `ConcertReadModel` (read-only projection, no `AvailableTickets`). Behavioural shape with `DecrementAvailability` and ticket-capacity ownership is pending — see `api/Concertable.Customer/TECH_DEBT.md`.

**Naming rationale:** B2B's `ConcertEntity` and Customer's `ConcertEntity` share the same `Id` (so cross-context events correlate) but have entirely different shapes because each bounded context cares about different things — textbook §1 ("Customer's Concert ≠ B2B's Concert"). Naming Customer's row as a "projection" or "availability" would falsely imply it's data-only; in fact it owns business rules around ticket allocation. Different ubiquitous language per context is the DDD-correct outcome.

**Namespace disambiguation at the code level:** `B2B.Concert.Domain.Entities.ConcertEntity` vs `Customer.Domain.Entities.ConcertEntity`. They never appear in the same file. Cross-service events use `Concertable.Contracts.ConcertId` for correlation, not either entity type.

### Whole-entity moves out of `Concert.Domain`

| Today in | Moves to |
|---|---|
| `Concert.Domain/Entities/TicketEntity.cs` | `Customer.Domain/Entities/TicketEntity.cs` |
| `Concert.Domain/Entities/ReviewEntity.cs` | `Customer.Domain/Entities/ReviewEntity.cs` |
| `Concert.Api/Controllers/TicketController.cs` | `Customer.Api/Controllers/TicketController.cs` |
| `Concert.Api/Controllers/ConcertReviewsController.cs` | `Customer.Api/Controllers/ConcertReviewsController.cs` |
| `Concert.Application/Interfaces/Reviews/*`, ticket services | `Customer.Application/...` |
| `Concert.Infrastructure` packages: QRCoder, QuestPDF (ticket QR/PDF generation) | `Customer.Infrastructure` |

### Read-projection moves

| Today in | Moves to |
|---|---|
| `Concert.Domain/ReadModels/ArtistReadModel.cs` | `Search.Domain` (consolidated with existing `ArtistSearchModel`) |
| `Concert.Domain/ReadModels/VenueReadModel.cs` | `Search.Domain` (consolidated with existing `VenueSearchModel`) |
| `Concert.Domain/ReadModels/ConcertRatingProjection.cs` | Split: B2B keeps for dashboards; Search re-publishes rating for browse |
| `ConcertController.GetDetailsById`, `GetUpcomingByVenueId`, `GetUpcomingByArtistId`, `GetHistory*`, `GetUnposted*`, `Search` header endpoints | **`Search.Api`** — Search owns *all* read-projection details endpoints |
| `ConcertController.Update`, `Post` (write paths, `[VenueManager]`-gated) | Stay in B2B (`Concert.Api`) |

### User module dismantling

`Modules/User/` (current TPH: `VenueManagerEntity` / `ArtistManagerEntity` / `CustomerEntity` / `AdminEntity` extending `UserEntity`) gets fully unwound:

| Today | Post-split |
|---|---|
| `UserEntity` base (email, password hash, email-verification) | **Auth** — Duende IS owns the identity record |
| `VenueManagerEntity`, `ArtistManagerEntity`, `AdminEntity` | **B2B** — flat tables (no TPH), each carrying Auth `sub` + B2B-specific fields (`VenueId`, `ArtistId`, etc.) |
| `CustomerEntity` | **Customer** — becomes `CustomerProfileEntity` (no TPH), carries Auth `sub` + customer-side fields (location, avatar, preferences) |
| `Role` enum on `UserEntity` | **Deleted** — role inferred from token audience + service-side profile-table membership |
| `Modules/Authorization/` (`ICurrentUser`, claim extensions) | Stays as cross-cutting framework code in `Concertable.Kernel` (`Identity/`), consumed per-service |

### Search module extraction

`Modules/Search/` becomes its own service largely as-is. Two cleanups required first:

- `Search.Infrastructure` currently references `Artist.Infrastructure` and `Venue.Infrastructure` (per `project_search_rating_projection_ownership`). Post-extraction, Search consumes events only — no upstream module-infrastructure refs.
- Existing `ArtistReadModel` / `VenueReadModel` in `Concert.Domain` consolidate into Search's `*SearchModel`s; the dual-model split is a monolith-era convenience, not a microservices boundary.

### Modules that stay where they are (named for completeness)

| Module | Lands in |
|---|---|
| `Modules/Artist`, `Modules/Venue` | B2B (canonical owners) |
| `Modules/Contract` | B2B |
| `Modules/Messaging` (venue↔artist messaging) | B2B |
| `Modules/Notification` | Becomes `Concertable.Email` shared library (see §4.7). Not a service. |
| `Modules/Payment` | Already shaped as adapter service; extracts straight across |

## 4.6 Reference data ownership

Reference data is split by characteristic:

- **Enum-like, static, never admin-CRUD'd** — lives as constants in `Concertable.Contracts`. No DB row in any service. **Genre is this case**: ~20–50 values, set when artist/concert is created, never edited at runtime. The current `SharedDbContext.Genres` table is **deleted**; Genre becomes a `record GenreId(int Value)` or `enum Genre` in `Concertable.Contracts`. Search projects genre names denormalized onto `*SearchModel`s — consumers never join on GenreId.
- **Admin-editable reference data** (none in Concertable today) — owned by exactly one canonical service. Name denormalized onto outbound events. Treat like Venue.
- **Per-service status enums** (`OpportunityStage`, `ContractType`) — stay local to the owning service. No cross-service sharing needed.

Rule: ask "does this reference data need an admin UI?" If no, it's a contract, not a service. Resist the temptation to make a `Concertable.ReferenceData` service.

## 4.7 Cross-cutting infrastructure: Email + Notification as shared libraries

**Decision (2026-05-19):** Email and Notification are *shared libraries*, not services. Both B2B and Customer import the same library and invoke it in-process. No network hop; no 6th logical service.

**Why a library beats a service here:**

- Email sending is synchronous fire-and-forget — the caller doesn't need a round-trip to its own service boundary to do it.
- Both B2B (booking accepted, settlement) and Customer (ticket purchased, verification) need to send emails. A library that both import is simpler than a bus event → Notification service → SMTP.
- Vendor swap is still an internal-only change — the abstraction lives in the library, not the consumer.
- Failure isolation is achievable at the library level with retry/queue within the library itself; a separate process doesn't add useful isolation for this use case.

**The abstraction:** `IEmailSender` (mirrors `Microsoft.AspNetCore.Identity`'s naming convention) lives in `Concertable.Kernel`. Concrete implementations (`SendGridEmailSender`, `SmtpEmailSender`, `FakeEmailSender`) live in a `Concertable.Email` csproj that each service references. Each service DI-wires the concrete implementation it wants.

**Two flavors still exist — handled within each service:**

- **Identity email** (verification, password reset) — `User.Infrastructure` (B2B) calls `IEmailSender` directly after user creation / token issuance. Customer service does the same via `Customer.Profile`.
- **Domain email** (booking accepted, ticket purchased) — the relevant module calls `IEmailSender` in its own event handler or service method.

**Notification module:** the existing `Modules/Notification` module becomes the `Concertable.Email` library. `INotificationModule` consumers switch to calling `IEmailSender` directly (one less indirection). No extraction to a separate process planned.

**Status (2026-05-23):** `Concertable.Shared.Email` exists and is wired by both B2B and Customer. B2B's `Modules/Notification/` still ships — it hosts the SignalR `NotificationHub`. Phase 8 Step 24 will separate SignalR from email; the email-only remnant deletes after that.

**Future cross-cutting adapters** (file storage, image CDN, geocoding) follow the same library-first principle — only extract to a service if measurable operational pressure demands it.

## 4.8 `api/Shared/*` — decision reversed (2026-05-23)

**Original plan** called for collapsing 6+ shared csprojs into two (`Concertable.Contracts` + `Concertable.Kernel`). **This decision is reversed.**

`Concertable.Contracts` and `Concertable.Kernel` stay as the wire and framework packages respectively. The per-concern shared libraries remain as separate csprojs because each owns a distinct cross-cutting concern with its own Application/Infrastructure split — collapsing them into Kernel would blur that separation without benefit.

**Current shared csprojs under `api/Shared/` and `api/`:**

| Csproj | Purpose |
|---|---|
| `Concertable.Contracts` | *The wire* — event contracts, cross-service DTOs, `ICurrentUser`, Genre lookup |
| `Concertable.Kernel` | *The framework* — `BaseEntity`, `Period`, `IUnitOfWork<TContext>`, `DbContextBase`, validation helpers |
| `Concertable.Shared.Api` | ASP.NET Core / HTTP cross-cutting helpers |
| `Concertable.Shared.Blob` | `IBlobService` abstraction + Azure Blob impl |
| `Concertable.Shared.Email` | `IEmailSender` abstraction + SendGrid/SMTP/Fake impls |
| `Concertable.Shared.Geocoding` | Geocoding abstraction + provider impl |
| `Concertable.Shared.Imaging` | Image processing abstraction + impl |
| `Concertable.Shared.Pdf` | PDF generation abstraction + QuestPDF impl |

The two-package wire/framework rule still applies for `Concertable.Contracts` vs `Concertable.Kernel` themselves — breaking changes in Contracts = protocol changes; in Kernel = compile failures.

## 5. Communication patterns

| Channel | Used for | Used between |
|---|---|---|
| **Async events on a broker (bus client TBD)** | Domain-event-driven projection updates, cross-service workflow coordination | B2B ↔ Customer (only mode); B2B/Customer → Search (projection feed); B2B/Customer ↔ Payment for webhooks |
| **Sync HTTP to adapter services** | Payment operations (create intent, transfer, refund); Auth token validation | B2B/Customer/Search → Payment; all → Auth |
| **Sync HTTP to read projection service** | Browse, autocomplete, header, detail-page reads | B2B SPAs → Search.Api; Customer SPA → Search.Api |
| **Sync HTTP from clients to services** | SPA / mobile → service | Customer SPA → Customer.Api; B2B SPAs → B2B.Api; both → Search.Api |
| **External SDK / HTTP** | Third-party integration | Payment → Stripe, services → geocoding, services → image storage |

**Disallowed:**

- **Sync HTTP between B2B.Api and Customer.Api** — would create distributed-monolith coupling. Async events only.
- **Sync HTTP from Search.Api outbound to B2B or Customer** — Search is read-only and event-fed. If Search ever needed to "fill in a missing field" by sync-calling a data service, the projection is wrong and the fix is to expand the event, not introduce coupling.
- **gRPC between services** — same antipattern risk as sync HTTP, with worse-fitting tooling.
- **Cross-service joins, shared schemas, shared DBs** — each service has its own database.
- **Project references between B2B modules and Customer modules** — only `Concertable.Contracts` (and the authorization-helpers library) may be referenced by both. Enforce via CI architecture tests.

## 5.5 Service-to-service authentication

**Decision: OAuth2 `client_credentials` via Duende.** Each service is registered as a Client in Auth with its own `client_id` + secret. When B2B needs to call Payment, B2B requests a service token from Auth (via the existing OIDC issuer), then presents it as `Authorization: Bearer <token>`. Payment validates audience + scope using the same JWT middleware it uses for user tokens.

This works because Auth is already there for user tokens — Duende IS supports `client_credentials` grant natively. Adding service-to-service auth is "register one more Client per service" rather than a new authentication subsystem.

**Alternatives considered:**

| Option | Verdict | Reason |
|---|---|---|
| client_credentials via Duende | **Chosen** | Duende already there; same JWT plumbing as user auth; standard pattern; trivial to add |
| mTLS | Rejected for now | Stronger identity but cert lifecycle is operationally heavy. Revisit if compliance ever demands it. |
| API keys | Rejected | No proper claims model; rotation/revocation brittle; no audience scoping |
| VNet only (no wire auth) | Rejected as primary | "Soft chewy center" — one network breach compromises everything. Used as defense-in-depth on top, not instead of |

**Production posture:** client_credentials on the wire **+** private network (VNet/VPC) as defense-in-depth. Internal services aren't publicly addressable except via API gateway, *and* every internal call carries a service token.

**Local dev:** Aspire AppHost wires the same client_credentials flow against the local Duende instance. No "skip auth in dev" mode — keeps the path tested.

**Note on roles:** Service tokens carry **scope** (e.g., `payment:write`, `auth:read`), not roles. User tokens carry **`sub` + audience**, also no role claim. Roles are derived per-service from token audience + service-side profile-table membership (§4 Identity row). Auth never emits a role claim into either type of token.

## 6. Event-driven CQRS pattern

The data-service side of the architecture is cross-service CQRS: write model on B2B, read model on Customer, bus keeps them aligned.

**Flow on a Venue update:**

```
1. Venue manager edits venue in B2B SPA
2. POST /venues/{id} hits B2B.Api → VenueController → VenueService.UpdateAsync
3. VenueEntity.Update(...) raises VenueChangedDomainEvent (in-process)
4. SaveChanges interceptor reads pending domain events,
   writes VenueChangedEvent row into B2B's Outbox table,
   commits both in one DB transaction
5. Outbox processor (bus client TBD — MassTransit's EntityFrameworkOutbox
   or a hand-rolled drain over the SDK) drains Outbox,
   publishes to broker topic "venue-changed"
6. Customer.Web (in-process consumer) consumes the message:
   - Inbox table check (idempotency)
   - VenueProjectionHandler updates Customer DB's slim Venue projection
   - Records message ID in Inbox in same transaction
7. Next Customer SPA detail page load reads the updated projection
```

End-to-end latency: typically a few hundred milliseconds. Eventual consistency window must be reflected in UX where it matters.

> **Why the outbox earns its keep — the dual-write problem.**
>
> Without it: `await SaveChangesAsync()` commits the DB row, then `await bus.PublishAsync(evt)` fails (process crash, broker outage). The DB has the change; nobody downstream ever hears about it. Or the inverse — publish succeeds, save fails — and you've broadcast a ghost event. Either way, B2B and Customer drift silently.
>
> The outbox makes step 4 *one* DB transaction: domain row + outbox row commit together. The sweeper (step 5) drains the outbox after the fact, retrying until it gets through. Inbox idempotency on the consumer (step 6) handles the at-least-once redelivery this guarantees. Combined: exactly-once *effects* from at-least-once *delivery* — the only honest semantic distributed systems will give you.

### Inverse-direction flow: ticket purchase

> **Not yet implemented (2026-05-23).** Customer's `TicketEntity.Create` does not raise `TicketPurchasedDomainEvent`; no `TicketPurchasedEvent` integration event exists; B2B has no `ConcertSalesProjection`. Tracked in `api/Concertable.Customer/TECH_DEBT.md` and `api/Concertable.B2B/TECH_DEBT.md`.

The same pattern runs Customer → B2B for analytics and settlement math (target state):

```
1. Customer SPA: POST /tickets/purchase → Customer.Api → TicketService.PurchaseAsync
2. Sync call to Payment.Api → CreatePaymentIntent (PCI scope stays in Payment)
3. Customer DB transaction: insert TicketEntity + decrement AvailableTickets on ConcertEntity
   + insert TicketPurchasedEvent row in Customer's outbox — one transaction
4. Outbox processor drains, publishes to broker topic "ticket-purchased"
5. B2B.Workers consumes: inbox check → update ConcertSalesProjection (soldCount, grossRevenue)
6. Search.Workers consumes the same event: refresh Search's ConcertSearchModel "tickets left" display
7. Venue/Artist dashboards (B2B) read ConcertSalesProjection; settlement math reads it too
```

Same plumbing, opposite direction. There is no "B2B-first" rule in the architecture — only "canonical owner publishes; consumers project."

## 7. Payment service architecture

Payment is the most distinctive piece of the design — sole reason it's worth pulling out as a separate adapter service:

- **PCI scope containment** — only Payment holds Stripe API keys. Everything else is out of PCI scope. Big audit win.
- **Single Stripe integration** — one place handles webhooks, idempotency, retry semantics, 3DS, refund routing, dispute handling.
- **Operational ledger** — single DB owns "what payments happened, in what state, confirmed when."
- **Processor abstraction** — if a second processor (Adyen, GoCardless) is ever added, the abstraction lives in one place.
- **Sole webhook receiver** — Stripe sends all webhooks to Payment, which validates signature and publishes domain-meaningful events to the bus.

**Payment API (sync, called by B2B and Customer):**

- `ProvisionConnectAccountAsync` — seller onboarding (B2B)
- `ProvisionStripeCustomerAsync` — anyone who needs to pay
- `CreatePaymentIntentAsync` — B2B settlement OR Customer ticket purchase
- `TransferToConnectAsync` — B2B settlement payouts
- `RefundAsync` — B2B cancellation OR Customer refund
- `GetPaymentStatusAsync` — consumer checks state

**Payment publishes to bus:**

- `PaymentSucceededEvent` — Customer subscribes for ticket confirmation; B2B subscribes for booking settlement
- `PaymentFailedEvent`
- `ConnectAccountUpdatedEvent` — B2B subscribes for DAC7/compliance status changes
- `TransferCompletedEvent` — B2B subscribes for payout ledger

## 8. Repository structure

Single Git repo. Solutions are per-service `.slnx` files in each service root (`Concertable.B2B.slnx`, `Concertable.Customer.slnx`, `Concertable.Search.slnx`, `Concertable.Payment.slnx`) plus the umbrella `api/Concertable.slnx`; the legacy single `Concertable.sln` is deleted. Designed so a poly-repo split is a folder move and packaging change, not a rewrite.

```
Concertable/
├── api/
│   ├── Concertable.slnx                        (umbrella solution)
│   ├── Concertable.AppHost/                    (umbrella Aspire orchestrator — local dev only)
│   ├── Concertable.AppHost.Shared/             (shared host topology/reference helpers)
│   ├── Concertable.Auth/                       (Duende IS — identity only)
│   ├── Concertable.Auth.Contracts/
│   ├── Concertable.B2B/
│   │   ├── Concertable.B2B.slnx
│   │   ├── Concertable.B2B.AppHost/            (standalone host, + AppHost.Extensions)
│   │   ├── Concertable.B2B.Web/                (HTTP host)
│   │   ├── Concertable.B2B.Workers/            (background jobs)
│   │   ├── Modules/                            (Artist, Concert, Contract, Conversations, Tenant, User, Venue)
│   │   ├── Seed/                               (Seed.Contracts, Seed.Infrastructure, Seed.Simulator)
│   │   └── Tests/
│   ├── Concertable.Customer/                   (same shape — Web only, consumers in-process; Modules: Concert, Preference, Review, Ticket, User)
│   ├── Concertable.Search/                     (Web + Workers, Seed/, Tests/)
│   ├── Concertable.Payment/                    (Web + Workers + Client gRPC adapters, Seed/, Tests/)
│   ├── Concertable.Messaging/                  (outbox/inbox/bus + ASB transport, + Tests/)
│   ├── Concertable.DataAccess/
│   ├── Concertable.ServiceDefaults/
│   └── Shared/                                 (Concertable.Kernel, Concertable.Contracts, Concertable.Shared.* libs, Seed/, Tests/)
├── app/
│   ├── web/                                    (customer SPA, b2b/{shared,venue,artist,business} SPAs, shared)
│   ├── customer/shared                         (npm @customer/shared — customer web+mobile core)
│   └── mobile/
└── ...
```

**Discipline that keeps poly-repo optionality:**

1. No sync HTTP between B2B and Customer. Async events only.
2. Shared code lives only in `Concertable.Contracts` (or future shared adapter NuGets).
3. Per-service database, per-service migrations.
4. CI architecture test (NetArchTest or similar) fails the build on boundary violations.

If those hold, mono-repo → poly-repo is a folder move + replacing project references with NuGet feeds.

## 8.5 Modular monolith inside each microservice

The microservices split **does not retire the modular monolith pattern** — it shrinks the pattern's scope to the inside of each service. The current discipline (`IXModule` facades, per-module `XDbContext` with its own schema, in-process domain events between modules via `IEventRaiser`, module-owned `IEntityTypeConfiguration<T>`, NetArchTest boundary enforcement, `MODULAR_MONOLITH_RULES.md`) **carries forward verbatim into each microservice's internal structure**.

**Inside `Concertable.B2B`:**

```
B2B/
├── Concertable.B2B.Web/                (HTTP host — composition root)
├── Concertable.B2B.Workers/            (background jobs host)
└── Modules/
    ├── Venue/        Api + Application + Domain + Infrastructure + Contracts
    ├── Artist/       same shape
    ├── Concert/      same shape (Booking/Application/Opportunity live inside it)
    ├── Contract/
    ├── Conversations/
    ├── Tenant/
    └── User/
```

Same shape inside `Concertable.Customer` (modules: Concert, Preference, Review, Ticket, User), `Concertable.Search` (VenueSearchModel, ArtistSearchModel, ConcertSearchModel as projection modules), and `Concertable.Payment` (Stripe integration, ledger).

**What stays:**

- `IXModule` facade per module (cross-module access goes through it; no direct entity reach-in)
- Per-module `XDbContext` with its own schema, owning only its tables (all DbContexts now point at the same B2B DB, but tables stay schema-isolated)
- In-process domain events between modules (`IEventRaiser` + `XChangedDomainEvent`) — cheap, synchronous, no bus involvement for intra-service flows
- Module-owned `IEntityTypeConfiguration<T>` in `Module.Infrastructure/Data/Configurations/`
- Per-module `IDevSeeder` / `ITestSeeder` at `Order=N`
- `InternalsVisibleTo` chains keeping `Module.Application` interfaces `internal`
- Module per-context migrations (one history table per service's DB; modules within the service still have their own migrations)
- NetArchTest rules enforcing cross-module discipline

**What changes:**

- Cross-service module access (e.g., Customer's Ticket module → B2B's Concert module) is **no longer in-process** — it goes via the bus (events for projection updates) or sync HTTP to adapter services. Within a single service, cross-module access remains in-process.
- The "Concertable.Workers" composition host splits into per-service Workers hosts (`B2B.Workers`, `Customer.Workers`, etc.). Each Workers host loads only its own service's modules.
- `Concertable.AppHost` orchestrates N service hosts in dev, where today it orchestrates one Web + one Workers.

**Why this earns its keep:**

- If a sub-module within a service later needs its own deployable (e.g., `Contract` grows to warrant a `Concertable.Contract.Api`), extraction is a packaging change because the internal boundary already exists.
- In-process domain events between modules avoid the latency and operational complexity of the bus for flows that don't actually cross a service boundary.
- The same patterns devs (or future-you) already know, scoped smaller.
- `MODULAR_MONOLITH_RULES.md` doesn't get deleted — it gets cited *per service*.

**What this is not:** an excuse to keep B2B and Customer modules together. The microservices split between bounded contexts (§1) is not negotiable. Inside a bounded context, the modular monolith pattern still applies.

## 9. Learning sequence

The architecture is the *what*; this is the *how to build it as a learning project*. Each step teaches a specific concept. Steps 0–2 happen *in the monolith* before any process split — the lesson "shrink the boundary in-process first" is itself the point.

0. **Decompose `ConcertEntity` in-place.** While still in one process, split B2B workflow fields from customer-display fields per §4.5. Move `TicketEntity` and `ReviewEntity` out of `Concert.Domain` into `Customer.Domain`. Move ticket QR/PDF infrastructure (QRCoder, QuestPDF) with them. Migrate `ConcertController` read endpoints into Search's existing controllers. Result: the monolith already has the *internal* boundary the future split will materialize.
1. **Dismantle the `Modules/User/` TPH.** Replace with flat per-persona profile entities owned by their respective modules; move identity authority into Auth (Duende). Strip role claims from issued tokens; derive role per-service from token audience + profile-table membership.
2. **Clean Search's upstream refs.** Remove `Search.Infrastructure` references to `Artist.Infrastructure` / `Venue.Infrastructure`. Search consumes domain events only.
3. **Extract Customer to its own service.** New solution folder, own `Program.cs`, own DbContext on its own SQL Server (or separate DB on same instance — same lesson). References only `Concertable.Contracts`.
4. **Bus on in-memory transport** between B2B and Customer. Skip cloud broker latency while learning publish/subscribe semantics. Bus client choice (MassTransit vs Azure Service Bus SDK vs other) **open** — decide at the start of this step.
5. **Transactional outbox.** Lesson: "publish reliably exactly when the DB transaction commits." (§6 callout.) Mechanism depends on Step 4 bus choice — `EntityFrameworkOutbox` if MassTransit, hand-rolled outbox table + dispatcher if SDK.
6. **Idempotent consumers** with inbox state. Lesson: "events arrive at-least-once, sometimes out of order — handlers must be safe."
7. **Extract Search to its own service.** Same playbook as Customer, but read-only and consumes events from both B2B and Customer.
8. **Switch transport to RabbitMQ** in a Docker container. Operational layer without cloud cost.
9. **Switch transport to Azure Service Bus.** Queues vs topics, subscriptions, dead-letter handling, sessions for ordering.
10. **Extract Payment to its own service** with its own DB and webhook endpoint.
11. **Build one saga** for the concert lifecycle (Posted → Settled). Lesson: long-running orchestration with persistent state. Implementation depends on Step 4 bus choice (MassTransit state machine or hand-rolled).
12. **OpenTelemetry distributed tracing** across services. Watch a flow end-to-end.
13. **Hard event-schema migration.** Change an event's shape with consumers running both old and new versions.

Roughly a year of evenings-and-weekends if taken seriously. Valuable on a CV at senior-backend level.

## 10. Non-goals and rejected patterns

- **No gRPC between services.** Synchronous coupling regardless of wire format.
- **No shared "domain logic" service that B2B and Customer both call sync for venue/concert/artist data.** That is the distributed-monolith antipattern. Project via events instead.
- **No shared database.** Each service has its own. Citadel (shared DB, multiple hosts) was considered and rejected because it doesn't serve the learning goals.
- **No premature webhook fan-out service.** Single Webhook Receiver service is an option *later* if Stripe event routing gets complex. Start with each service exposing its own webhook endpoint, or Payment receiving all webhooks.
- **No native mobile apps in launch scope** (already in LAUNCH_PLAN.md).

## 11. Risks and open questions

| # | Risk / question | Notes |
|---|---|---|
| R1 | Eventual-consistency UX is harder than it looks. Customer briefly sees stale Concert data after B2B updates. | Mitigation: design UX with refresh affordances; surface "live" indicators only where consistency matters. |
| R2 | Event schema versioning over time. Once published, events can't change shape easily. | Mitigation: deliberate event design; version events explicitly; learn this via final step of the sequence. |
| R3 | Solo dev maintenance overhead of N services. | Mitigation: mono-repo + Aspire AppHost keeps local-dev friction low. Production deploy complexity is real if it ever ships. |
| R4 | Stripe webhook routing — does Payment handle all of them, or per-service endpoints? | Open. Default: Payment handles all (PCI scope reasons), fans out via bus. Revisit if it gets noisy. |
| R5 | Ticket purchase is the highest-write event. Per-event projection scales fine for normal load but a flash sale will pin the inbox handler on the B2B + Search consumers. | Mitigation: defer until measured. Options when it bites: batched/windowed projection, dedicated consumer scaling, summary events instead of per-ticket. |
| R6 | `Modules/User/` TPH unwind (§4.5) touches every B2B controller that reads `ICurrentUser` for role/persona checks. Sequence carefully so the monolith builds at each step. | Mitigation: do the role-claim removal in Auth first, derive role from token audience + DB lookup per controller, *then* delete the TPH. |
| R7 | `ConcertEntity` decomposition (Step 0) is the biggest in-monolith refactor on the path. Touches Concert + Customer + Search projections + every read path. | Mitigation: explicit migration step (§4.5) sequences the field moves; ship in small PRs; integration tests cover both old and new shapes during transition. |
| ~~Q1~~ R8 *(was Q1)* | **Resolved 2026-05-19** — Search is its own service, sync-callable from both B2B and Customer SPAs. Single projection serves both audiences; no duplicate browse-projection tables in B2B and Customer. Updates stay with canonical owners. | — |
| ~~Q2~~ R9 *(was Q2)* | **Re-resolved 2026-05-19** — Email/Notification is a *shared library* (`Concertable.Email`), not a service. Both B2B and Customer import it in-process. `IEmailSender` is the cross-service abstraction in `Concertable.Kernel`. See §4.7. | — |
| Q3 | Do we eventually need a dedicated `Concertable.Webhooks.Stripe` service separate from Payment? | Only if Stripe event handling outgrows Payment. Defer. |
| Q4 | Future B2B venue↔artist reviews — same `ReviewEntity` shape as customer reviews, or different? | Open. Default: separate table in B2B (different bounded context, even if conceptually similar). Same conceptual name `Review` is fine across services. |
| ~~Q5~~ R10 *(was Q5)* | **Resolved 2026-05-19** — `ICurrentUser` and claim helpers stay a shared library; no central authorization service; each service makes its own authz decisions against tokens it validates. *(As-built they landed in `Concertable.Kernel/Identity/` — the framework package — not Contracts.)* | — |
| Q6 | Service-to-service auth in `client_credentials` model: how are scopes structured? Per-callee service (`payment:write`, `auth:read`) or finer (`payment:intent:create`)? | Open. Default: start coarse (one scope per callee service), refine only if a service legitimately needs to distinguish caller intents. |
| Q7 | Event schema versioning concrete mechanism. `V1`/`V2` type names? CloudEvents headers? Upcaster pattern? | Open. Decide at Step 13 of §9 with concrete migration in hand. |
| Q8 | DB-per-service cutover when extracting Customer (Step 3 §9). One logical DB → N physical DBs operationally — backup/restore, logical export, tolerated downtime? | Open. Likely "fresh DB + replay events from B2B outbox to populate Customer's projections" since the boundary is fresh; but spike it before committing. |

## 12. Decision log

**Foundational decisions (2026-05-18/19)** — all baked into §§2–8 above:
- Event-driven microservices over Citadel (shared DB). Reason: learning goals + B2B/Customer are genuinely separate bounded contexts.
- Adapter vs data service rule: Payment and Auth accept sync calls; B2B and Customer do not (§3).
- No gRPC. No shared DB. No sync between B2B and Customer.
- Auth issues `sub` + audience only — no role claims. Role derived per-service from profile-table membership (§5.5).
- Service-to-service: `client_credentials` via Duende. Rejected mTLS and API keys.
- Customer owns `TicketEntity`, `ReviewEntity`, `AvailableTickets`. B2B owns `TotalTickets` capacity.
- Search is a sync-callable read projection service, not a data service.
- Email/Notification is a shared library (`Concertable.Shared.Email`), not a service (§4.7).
- Modular monolith pattern stays inside each microservice (§8.5).
- Genre and other static reference data lives as enum in `Concertable.Contracts`, not a DB table (§4.6).

**2026-05-23** — Doc-vs-reality reconciliation after Phases 1–7 landed:
- `Concertable.Customer.Workers` dropped from §2 — Customer is write-light; consumers run in-process in `Concertable.Customer.Web`. No Workers host needed.
- §4.8 csproj-collapse **reversed** — per-concern shared csprojs kept as-is alongside `Contracts` and `Kernel`.
- `CustomerProfileEntity` is named `UserEntity` in practice.
- B2B Workers uses `AddInMemoryTransport` (bug — see `api/Concertable.B2B/TECH_DEBT.md`).
- Inverse-direction event flow (§6) not yet implemented (see `TECH_DEBT.md` files).
- Per-service `ARCHITECTURE.md` and `TECH_DEBT.md` added at `api/Concertable.B2B/` and `api/Concertable.Customer/`.

**2026-06-05** — Doc-vs-reality reconciliation (big-review NOTE4). `api/ARCHITECTURE.md` is the canonical current-state doc; this doc records the design and its decision history.
- Solutions are per-service `.slnx` + the umbrella `api/Concertable.slnx`; the single `Concertable.sln` is deleted (§8 updated).
- Service HTTP hosts are named `Concertable.X.Web`, not `Concertable.X.Api` — `.Api` is reserved for module layers inside a service (§2/§8/§8.5 updated).
- `ICurrentUser` + claim helpers landed in `Concertable.Kernel/Identity/` (the framework package), not `Concertable.Contracts` and not a `Concertable.Authorization.Contracts` project — no such project exists (§4/§4.5/R10 updated).
- §8 repo tree redrawn as-built: standalone per-service AppHosts, `Seed/` + `Tests/` inside each service, `api/Shared/` for Kernel/Contracts/cross-cutting libs, `Concertable.Auth.Contracts`.

## 13. Reference

- [LAUNCH_PLAN.md](/plans/LAUNCH_PLAN.md) — broader launch context (mostly applies to B2B deployment if/when it happens)
- [MARKETPLACE_PLAN.md](/plans/MARKETPLACE_PLAN.md) — original marketplace deferral plan; partially superseded by this doc since marketplace would return as a separate microservice, not as a feature flag
- [B2B_LAUNCH_CHECKLIST.md](/plans/B2B_LAUNCH_CHECKLIST.md) — legal/business setup checklist
- [USER_MODEL_PLAN.md](/plans/USER_MODEL_PLAN.md) — multi-user tenants, roles, permissions, the auth sweep (B2B-internal; the tenancy foundation has shipped)
- MassTransit docs — https://masstransit.io
- *Microservices Patterns* (Chris Richardson) — outbox, sagas, CQRS, idempotency
- *Building Microservices* (Sam Newman) — boundaries, distributed-monolith antipatterns
