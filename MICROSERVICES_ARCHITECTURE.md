# Concertable Microservices Architecture

> **Status:** Direction confirmed, not started. Architectural decisions documented after the 2026-05-18 / 2026-05-19 exploration sessions.
>
> **Goal:** Migrate from the current modular monolith to an event-driven microservices architecture that separates B2B (venue ↔ artist booking + settlement) from Customer (ticket marketplace), with a centralised Payment adapter service.
>
> **Constraint that changed the plan:** This is a learning side project. The Nov 2026 launch in `LAUNCH_PLAN.md` is aspirational, not a hard deadline. Skill development (event-driven architecture, transactional outbox, sagas, OpenTelemetry, Service Bus operations) is an explicit goal alongside any eventual deployment.
>
> **Companion docs:** [LAUNCH_PLAN.md](LAUNCH_PLAN.md), [MARKETPLACE_PLAN.md](MARKETPLACE_PLAN.md), [B2B_LAUNCH_CHECKLIST.md](B2B_LAUNCH_CHECKLIST.md), [ORGANIZATION_REFACTOR_PLAN.md](ORGANIZATION_REFACTOR_PLAN.md).

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

| Service | Type | DB | Hosts |
|---|---|---|---|
| `Concertable.B2B.Api` | Data service | B2B SQL DB | Owns Venue, Artist, Concert (workflow shape), Contract, Booking, Application, Opportunity, Settlement, Organization, Messaging, manager/admin profiles. Manager + venue/artist SPA endpoints. |
| `Concertable.B2B.Workers` | Worker | (shared with B2B.Api) | Background jobs: settlement triggers, lifecycle transitions, payout reconciliation. |
| `Concertable.Customer.Api` | Data service | Customer SQL DB | Owns Tickets (with `AvailableTickets`), Preferences, Reviews, `CustomerProfileEntity`. Holds slim browse-projections of B2B's Venue/Artist/Concert *if not served by Search*. |
| `Concertable.Customer.Workers` | Worker (optional) | (shared with Customer.Api) | Ticket email delivery, refund batching, projection rebuild from event replay. |
| `Concertable.Search.Api` | Read projection service | Search SQL DB | Owns `ArtistSearchModel`, `VenueSearchModel`, `ConcertSearchModel`. Read-only, sync-callable from both B2B and Customer. Serves browse, autocomplete, header, and detail-page reads. Event-fed from canonical owners. |
| `Concertable.Payment` | Adapter service | Payment SQL DB | Owns PayoutAccount (Connect refs), StripeCustomer refs, payment intent ledger, transfer ledger, refund ledger. Sole receiver of Stripe webhooks. |
| `Concertable.Auth` | Adapter service | Duende IS DB | OIDC issuer for all audiences. Owns identity authority (`sub`, password hash, email-verification flow). **Does not emit role claims** — see §4 Identity row. |
| `Concertable.AppHost` | Aspire orchestrator | — | Local dev orchestration. Already in place. |
| `Concertable.Contracts` | NuGet/csproj | — | Shared event contracts and cross-service DTOs. Also home for `ICurrentUser` / authorization-claim helpers consumed per-service. Not a deployable. |
| Azure Service Bus | Managed broker | — | Async event substrate between services. |

**Service count:** 6 deployable services (B2B.Api, B2B.Workers, Customer.Api, Search.Api, Payment, Auth), optional 7th if Customer needs workers. Plus AppHost (dev only), Contracts package, and the managed broker.

## 3. Service categories

A distinction that became load-bearing during the design conversation:

| Category | Sync calls from other services OK? | Examples |
|---|---|---|
| **Adapter service** — wraps an external concern (Stripe, identity provider, email), owns operational state, exposes operations rather than data fetches | **Yes** — same shape as calling Stripe directly | Payment, Auth, Notification (future), Webhook Receiver (future) |
| **Data service** — owns canonical domain data that consumers think of as their own | **No** — distributed-monolith antipattern. Project via events. | B2B (Venue, Artist, Concert workflow), Customer (Tickets, Reviews, CustomerProfile) |
| **Read projection service** — event-fed denormalised store, read-only from outside | Sync reads OK (it's a projection, not an authority) | Search |

This is the rule the rest of the architecture hangs on. **A read projection service that ever owns a write becomes a data service and loses sync-call privilege.** Search stays read-only; updates flow exclusively through canonical owners' events.

## 4. Data ownership

| Concept | Source of truth | Projected to | Sync mechanism |
|---|---|---|---|
| Venue | B2B DB (`VenueEntity`, full ~25 fields with Organization, PayoutAccount, ComplianceContext) | Search DB (`VenueSearchModel` for browse/details) | `VenueChangedEvent` via bus |
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
- **Authentication vs authorization.** Auth (Duende) issues tokens with `sub` + audience only — no role claim. Each service derives the persona's role from its own data: B2B inspects the manager-profile subtype/membership for the `sub`; Customer treats any token from its audience as `Customer`. This separates "who are you" (Auth) from "what can you do here" (per-service authorization). `Concertable.Authorization.Contracts` (`ICurrentUser`, claim helpers) stays as a shared library, consumed per-service.
- **Customer/Search projection split.** Customer DB only needs to project what *Customer's own write paths* require (e.g., `TotalTickets` to compute remaining). Browse/detail reads route through Search. This avoids duplicating projections across Customer and Search.

## 4.5 Entity migration map

Where current monolith code lands post-extraction. Sequence matters — most of this should happen *in-process* before any service split, so the cross-process move is a packaging change rather than a refactor.

### Concert module decomposition

`ConcertEntity` is currently a god-entity: B2B workflow fields *and* customer-display fields on one row.

| Current field on `ConcertEntity` | Post-split owner |
|---|---|
| `BookingId`, `ContractType`, `CurrentStage`, `DatePosted` | B2B (workflow shape, canonical) |
| `TotalTickets` (capacity) | B2B (set by venue when posting) → Search + Customer projections |
| `AvailableTickets` (remaining) | **Customer** — moves off B2B's `ConcertEntity` entirely |
| `Name`, `About`, `Price`, `BannerUrl`, `Avatar`, `Period`, `Location` | B2B (canonical for editing) → Search (browse projection) |
| `ConcertGenres`, `Images` | B2B (canonical) → Search (browse projection) |
| `Tickets` collection | **Customer** — `TicketEntity` moves out of `Concert.Domain` |

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
| `Modules/Authorization/` (`ICurrentUser`, claim extensions) | Stays as cross-cutting library in `Concertable.Contracts` (or its own NuGet), consumed per-service |

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
| `Modules/Notification` | Stays in-process initially; future adapter service (deferred per §11 Q2) |
| `Modules/Payment` | Already shaped as adapter service; extracts straight across |

## 5. Communication patterns

| Channel | Used for | Used between |
|---|---|---|
| **Async events on Azure Service Bus (MassTransit)** | Domain-event-driven projection updates, cross-service workflow coordination | B2B ↔ Customer (only mode); B2B/Customer → Search (projection feed); B2B/Customer ↔ Payment for webhooks |
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
5. MassTransit EntityFrameworkOutbox processor drains Outbox,
   publishes to Service Bus topic "venue-changed"
6. Customer.Workers consumes the message:
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

The same pattern runs Customer → B2B for analytics and settlement math:

```
1. Customer SPA: POST /tickets/purchase → Customer.Api → TicketService.PurchaseAsync
2. Sync call to Payment.Api → CreatePaymentIntent (PCI scope stays in Payment)
3. Customer DB transaction: insert TicketEntity + decrement AvailableTickets on Concert projection
   + insert TicketPurchasedEvent row in Customer's outbox — one transaction
4. MassTransit drains, publishes to Service Bus topic "ticket-purchased"
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

Single Git repo, single `.sln` (or split into multiple `.sln` files for build performance later). Designed so a poly-repo split is a folder move and packaging change, not a rewrite.

```
Concertable/
├── api/
│   ├── Concertable.AppHost/                    (Aspire orchestrator)
│   ├── Concertable.Auth/                       (Duende IS, existing — identity only)
│   ├── Concertable.Contracts/                  (shared NuGet/csproj: event contracts, DTOs, authorization helpers)
│   ├── Concertable.B2B/
│   │   ├── Concertable.B2B.Api/                (HTTP host)
│   │   ├── Concertable.B2B.Workers/            (background jobs)
│   │   └── Modules/                            (Venue, Artist, Concert workflow, Contract, Booking, Application, Opportunity, Organization, Messaging, Membership/ManagerProfiles)
│   ├── Concertable.Customer/
│   │   ├── Concertable.Customer.Api/
│   │   ├── Concertable.Customer.Workers/       (optional)
│   │   └── Modules/                            (Tickets, Preferences, Reviews, CustomerProfile, Projections)
│   ├── Concertable.Search/
│   │   ├── Concertable.Search.Api/             (read-only HTTP host: browse, autocomplete, details)
│   │   ├── Concertable.Search.Workers/         (event consumers populating projections)
│   │   └── Modules/                            (VenueSearchModel, ArtistSearchModel, ConcertSearchModel)
│   └── Concertable.Payment/
│       ├── Concertable.Payment.Api/
│       ├── Concertable.Payment.Workers/        (webhook processing, reconciliation)
│       └── Modules/                            (Stripe integration, ledger)
├── app/
│   ├── web/
│   │   ├── b2b/                                (venue / artist / business SPAs)
│   │   └── customer/
│   └── mobile/
├── Concertable.sln
└── ...
```

**Discipline that keeps poly-repo optionality:**

1. No sync HTTP between B2B and Customer. Async events only.
2. Shared code lives only in `Concertable.Contracts` (or future shared adapter NuGets).
3. Per-service database, per-service migrations.
4. CI architecture test (NetArchTest or similar) fails the build on boundary violations.

If those hold, mono-repo → poly-repo is a folder move + replacing project references with NuGet feeds.

## 9. Learning sequence

The architecture is the *what*; this is the *how to build it as a learning project*. Each step teaches a specific concept. Steps 0–2 happen *in the monolith* before any process split — the lesson "shrink the boundary in-process first" is itself the point.

0. **Decompose `ConcertEntity` in-place.** While still in one process, split B2B workflow fields from customer-display fields per §4.5. Move `TicketEntity` and `ReviewEntity` out of `Concert.Domain` into `Customer.Domain`. Move ticket QR/PDF infrastructure (QRCoder, QuestPDF) with them. Migrate `ConcertController` read endpoints into Search's existing controllers. Result: the monolith already has the *internal* boundary the future split will materialize.
1. **Dismantle the `Modules/User/` TPH.** Replace with flat per-persona profile entities owned by their respective modules; move identity authority into Auth (Duende). Strip role claims from issued tokens; derive role per-service from token audience + profile-table membership.
2. **Clean Search's upstream refs.** Remove `Search.Infrastructure` references to `Artist.Infrastructure` / `Venue.Infrastructure`. Search consumes domain events only.
3. **Extract Customer to its own service.** New solution folder, own `Program.cs`, own DbContext on its own SQL Server (or separate DB on same instance — same lesson). References only `Concertable.Contracts`.
4. **MassTransit on in-memory transport** between B2B and Customer. Skip cloud broker latency while learning publish/subscribe semantics.
5. **Transactional outbox** via MassTransit's `EntityFrameworkOutbox`. Lesson: "publish reliably exactly when the DB transaction commits." (§6 callout.)
6. **Idempotent consumers** with inbox state. Lesson: "events arrive at-least-once, sometimes out of order — handlers must be safe."
7. **Extract Search to its own service.** Same playbook as Customer, but read-only and consumes events from both B2B and Customer.
8. **Switch transport to RabbitMQ** in a Docker container. Operational layer without cloud cost.
9. **Switch transport to Azure Service Bus.** Queues vs topics, subscriptions, dead-letter handling, sessions for ordering.
10. **Extract Payment to its own service** with its own DB and webhook endpoint.
11. **Build one saga** for the concert lifecycle (Posted → Settled). MassTransit's state machine. Lesson: long-running orchestration with persistent state.
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
| Q2 | Where do Notifications live in the longer-term plan? | Probably its own adapter service, event-driven. Defer extraction until concrete need. |
| Q3 | Do we eventually need a dedicated `Concertable.Webhooks.Stripe` service separate from Payment? | Only if Stripe event handling outgrows Payment. Defer. |
| Q4 | Future B2B venue↔artist reviews — same `ReviewEntity` shape as customer reviews, or different? | Open. Default: separate table in B2B (different bounded context, even if conceptually similar). Same conceptual name `Review` is fine across services. |
| Q5 | Where does the existing `Concertable.Authorization` (`ICurrentUser`, claim helpers) live post-split? | Default: shared library in `Concertable.Contracts` (or its own NuGet). Each service references it; no service owns the *authorization decisions* centrally. |

## 12. Decision log

- **2026-05-18** — Direction shift from modular-monolith Citadel to event-driven microservices. Rationale: learning project framing; bounded contexts genuinely differ; deployment isolation matters.
- **2026-05-18** — Citadel pattern (two hosts sharing one DB) considered and rejected for this project. Reason: it served deadline-driven launch goals, not learning goals.
- **2026-05-18** — Mono-repo retained; poly-repo optionality preserved by event-only inter-service communication.
- **2026-05-19** — Payment is its own adapter service, not internal modules per audience. Reason: PCI scope containment + venues/artists are both payers and payees so Connect/Customer refs can't be partitioned by audience.
- **2026-05-19** — Adapter vs data service distinction codified. Adapter services (Payment, Auth) are legitimately shared with sync calls; data services (Venue/Artist/Concert) are not.
- **2026-05-19** — gRPC explicitly out of scope for inter-service communication.
- **2026-05-19** — Customer owns `TicketEntity`, `ReviewEntity`, and `AvailableTickets`. Reason: every field on `TicketEntity` (`sub`, QR, purchase date) is customer-side; the marketplace is what *sells* the ticket. Splitting `AvailableTickets` from Tickets causes a race-condition window; ownership stays together.
- **2026-05-19** — B2B keeps `TotalTickets` as capacity (set when venue posts concert). Customer projects it and tracks its own `AvailableTickets` locally. Settlement math reads `ConcertSalesProjection` populated by inverse-direction `TicketPurchasedEvent`.
- **2026-05-19** — Inverse-direction projections legitimized. Customer → B2B for tickets/reviews/analytics; B2B → Customer for venue/artist/concert metadata. The canonical-owner rule is what's fixed; direction follows ownership.
- **2026-05-19** — Search is its own deployable read projection service (`Concertable.Search.Api`), sync-callable from both B2B and Customer SPAs. Owns `*SearchModel`s + browse/detail/autocomplete endpoints. Read-only — writes flow through canonical owners and reach Search via events. Reason: Search is already shaped as a projection in the monolith; serving both audiences from one projection avoids duplicating browse tables across Customer and B2B.
- **2026-05-19** — `Modules/User/` TPH dismantled. Identity authority lives in Auth (Duende: `sub`, email, password hash, email-verification). Each service owns a flat per-persona profile entity carrying the `sub` (B2B: `VenueManagerEntity`/`ArtistManagerEntity`/`AdminEntity`; Customer: `CustomerProfileEntity`). Reason: customer data should not live in B2B; a TPH spanning two bounded contexts is the monolith leaking across the split.
- **2026-05-19** — Authentication and authorization explicitly separated. Auth issues tokens with `sub` + audience only, no role claim. Each service derives role from token audience (Customer audience ⇒ Customer role) + service-side profile-table membership (B2B looks up `sub` in manager/admin tables to determine persona). Reason: identity ("who are you") and authorization ("what can you do *here*") are different concerns; baking role claims into the identity token couples them and leaks per-service authorization rules into Auth.
- **2026-05-19** — Future B2B venue↔artist reviews (post-gig counterparty feedback) will live as a separate B2B-owned table. Same conceptual name `Review` is fine because the two reviews live in different bounded contexts.

## 13. Reference

- [LAUNCH_PLAN.md](LAUNCH_PLAN.md) — broader launch context (mostly applies to B2B deployment if/when it happens)
- [MARKETPLACE_PLAN.md](MARKETPLACE_PLAN.md) — original marketplace deferral plan; partially superseded by this doc since marketplace would return as a separate microservice, not as a feature flag
- [B2B_LAUNCH_CHECKLIST.md](B2B_LAUNCH_CHECKLIST.md) — legal/business setup checklist
- [ORGANIZATION_REFACTOR_PLAN.md](ORGANIZATION_REFACTOR_PLAN.md) — User/Membership/Tenant separation, still relevant inside B2B
- MassTransit docs — https://masstransit.io
- *Microservices Patterns* (Chris Richardson) — outbox, sagas, CQRS, idempotency
- *Building Microservices* (Sam Newman) — boundaries, distributed-monolith antipatterns
