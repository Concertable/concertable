# Microservice Migration Steps

> **Companion to** [MICROSERVICES_ARCHITECTURE.md](MICROSERVICES_ARCHITECTURE.md). That doc is the *what*; this one is *what order, in phases*.
>
> **Status:** Phase 1 COMPLETE. All 6 steps done. Exit criteria met.
>
> **Rule:** Don't open Phase N until Phase N−1 is done. Half-done migrations are worse than no migration.

---

## Phase 0 — Lock the vision (now)

Pre-execution doc work. No code refactor yet.

- [x] Close gaps in `MICROSERVICES_ARCHITECTURE.md` from the 2026-05-19 conversation: reference data (§4.6), Notification (§4.7), `api/Shared/*` collapse (§4.8), service-to-service auth (§5.5), inventory inconsistency in §2 fixed
- [x] Write `MICROSERVICES_NORTH_STAR.md` — short principle-first vision doc
- [x] **`Feature/ManagerFrontPage` parked** at head `23c8fc4c`. Microservices migration takes priority; the dashboard work resumes (or is abandoned) post-Phase 1 when the codebase has the post-decomposition shape. Decision date: 2026-05-19. Rationale: B2B SaaS + Customer marketplace separation needs to happen first; finishing dashboard work on top of the god-`ConcertEntity` is wasted effort because Step 1 of Phase 1 rewrites the data shape underneath it.

**Exit criteria:** all three docs (`NORTH_STAR`, `ARCHITECTURE`, `STEPS`) trusted as canonical. ManagerFrontPage parking documented. Ready to start Phase 1.

---

## Phase 1 — In-monolith decomposition

> All work stays in the modular monolith. Zero deployment changes. Monolith ships throughout. Most of the future cross-process boundary materialises here as an *internal* boundary first.

1. ~~**Decompose `ConcertEntity` in-place.**~~ **DONE `ad6b4c31`.** Split B2B workflow fields from customer-display fields per §4.5. Move `TicketEntity`, `ReviewEntity`, QR/PDF infra (`QRCoder`, `QuestPDF`) out of `Concert.Domain` into new `Customer.Domain`. Move `ConcertController.GetDetailsById`, `GetUpcoming*`, `GetHistory*`, `GetUnposted*`, header `Search` endpoints to Search's controllers. *Biggest refactor on the path (R7).* Ship in small PRs with integration tests covering both shapes during transition. *Note: Search-controller move is deferred to a follow-up; current commit landed the entity decomposition + Customer module moves.*
2. ~~**Collapse `Concertable.Shared.*` to two csprojs.**~~ **DONE `7491498a`.** Per §4.8: `Concertable.Contracts` (wire) + `Concertable.Kernel` (framework). Six csprojs become two; `Concertable.Shared.UnitTests` becomes `Concertable.Kernel.UnitTests`. Cycle break en route: `FakeEmailService` moved Kernel → `User.Infrastructure` (it pulled `IUserModule`); email DI now inside `AddUserModule`. `GenreMappers` moved Contracts → Kernel so Contracts is zero-dep.
3. ~~**Delete `SharedDbContext` + move Genre to `Concertable.Contracts`.**~~ **DONE `2832354b`.** Genre is a `JsonStringEnumConverter`-decorated enum in `Concertable.Contracts` with explicit int values (Rock=1..House=8). SharedDbContext, GenreEntity, GenreRepository, IGenreService, GenreService, GenreDto, GenreMappers, IGenreJoin, GenreJoinExtensions all deleted. EF stores as int, wire sends as string. Frontend: `Genre` TypeScript type is a string union; `genreLabel()` helper for display names. Migration re-scaffold deferred (ICustomerReviewModule DI gap is pre-existing; unblocked when Customer DI is wired in Step 1).
4. ~~**Dismantle `Modules/User/` TPH.**~~ **DONE `cd872fad`.** Flat per-service profile tables: `VenueManagerProfile { Sub, VenueId }`, `ArtistManagerProfile { Sub, ArtistId }`, `AdminProfile { Sub }` in `UserDbContext`; `CustomerProfile { Sub }` in `Customer.Profile` module created via `UserRegisteredEvent` handler. IUserMapper + IUserLoader dispatcher patterns deleted; `UserModule` does inline profile-aware mapping. Seeders insert profile rows explicitly.
5. ~~**Auth becomes identity-only.**~~ **DONE `4aa7e641`.** Delete `RoleEnforcingInteractionResponseGenerator`, `IClientRoleResolver`, `Concertable.User.Contracts.Role` enum. Auth issues tokens with `sub` + audience only. Per-service authorization rejects tokens whose `sub` isn't in the service's profile tables — that replaces the "user must have role X for client Y" Auth-side check. `UserRegisteredEvent` split into 4 typed events; `IUserRegister` dispatcher + per-role impls deleted; `Role` enum promoted to `User.Domain`.
6. ~~**Clean Search's upstream refs.**~~ **DONE `f62bc4fd`.** Remove `Search.Infrastructure` references to `Artist.Infrastructure` / `Venue.Infrastructure`. Replace with direct `Artist.Domain` / `Venue.Domain` refs; inline "artist"/"venue" schema strings in the 3 EF configs. Rating providers are injected via DI — no code-path changes needed.

**Exit criteria:** monolith still ships. Internal boundary matches future split. `Concert.Domain` no longer god-entity. Auth has no role concept. Search has no upstream module refs.

---

## Phase 2 — First extraction: Customer

> First cross-process boundary. Bus introduced. Outbox/inbox shows up.

7. ~~**Extract `Concertable.Customer.Api` + `Concertable.Customer.Workers`** to their own host + own DB.~~ **DONE (code-level) 2026-05-19** — commits `8da35e0a` (7a–7e: ConcertChangedEvent expansion, Customer.Ticket off B2B nav chain, IPaymentSucceededProcessor dispatcher retired, Payment/Contract.Contracts/Concert refs trimmed) + `ea7ffecd` (7g/7h: Aspire CustomerDb resource + 4 module DbContexts bound to `ConnectionStrings:CustomerDb` + csproj audit). Plan + sub-step trace in `STEP_7_PLAN.md`. **Migration re-scaffold + dev seeder wiring deferred** — blocked by pre-existing ICustomerReviewModule DI gap (B2B ConcertModule still forwards review-facade methods); retiring those forwarders unblocks `./initial-migrations.ps1`. Customer.Web has no IDbInitializer yet.
8. **MassTransit on in-memory transport** between B2B and Customer. Skip cloud broker latency while learning publish/subscribe semantics.
9. **Transactional outbox** via MassTransit's `EntityFrameworkOutbox` in each service's own DB. Solves the dual-write problem (§6 callout).
10. **Idempotent consumers** with inbox state per service. Lesson: events arrive at-least-once, sometimes out of order — handlers must be safe.
11. **Service-to-service auth** wired for the new Customer → B2B / Payment sync calls (where they exist). `client_credentials` via Duende per §5.5.

**Exit criteria:** Customer runs as its own process. Tickets/reviews/customer profile no longer in B2B's binary. Bus carries projection updates and ticket events. Two DBs (B2B + Customer). Outbox/inbox proven on at least one event in each direction.

---

## Phase 3 — Second extraction: Search

12. **Extract `Concertable.Search.Api` + `Concertable.Search.Workers`** to their own host + own DB. Read-only, sync-callable from B2B SPAs and Customer SPA. Workers consume events from both B2B and Customer to populate projections.
13. **Switch transport to RabbitMQ** in a Docker container. Operational layer (queues, dead-letter, retry policies) without cloud cost.

**Exit criteria:** Search runs as its own process. Three audience-facing services (B2B, Customer, Search). Two adapter services still in-process (Payment, Auth, Notification).

---

## Phase 4 — Production-grade infrastructure

14. **Switch transport to Azure Service Bus.** Queues vs topics, subscriptions, dead-letter handling, sessions for ordering. Production broker.
15. **Extract `Concertable.Payment.Api` + `Concertable.Payment.Workers`** to its own host + own DB + own Stripe webhook endpoint. PCI scope shrinks dramatically. Stripe webhook URL change in dashboard.
16. **One saga** for the concert lifecycle (Posted → Settled) via MassTransit state machine. Lesson: long-running orchestration with persistent state.
17. **OpenTelemetry distributed tracing** across all running services. Watch one ticket-purchase flow end-to-end through B2B + Customer + Payment + Search.

**Exit criteria:** five services running on production-grade infra. PCI scope contained to Payment. Cross-service flows observable.

---

## Phase 5 — Hardening + Notification

18. **Hard event-schema migration.** Change one event's shape with consumers running both old and new versions concurrently. Pick a versioning mechanism (V1/V2 type names? CloudEvents headers? upcaster?) — open per Q7 in ARCHITECTURE.md.
19. **Extract `Concertable.Notification`** when email volume / template management / vendor swap pressure becomes concrete. Auth's direct SMTP/SendGrid call gets replaced by `EmailVerificationRequestedEvent` to bus; Notification subscribes.

**Exit criteria:** all 6 logical services running independently. Event versioning playbook exercised at least once.

---

## What blocks first launch (B2B SaaS)

**Decision 2026-05-19:** the "ship monolith with Customer muted at the controller level" shortcut is **rejected**. Customer code being in the B2B production binary undermines the entire §1 motivation for separation. B2B SaaS launch waits until Customer is in its own process.

**Practical minimum to launch B2B publicly:**

- **Phase 0** — docs locked ✅
- **Phase 1** — in-monolith decomposition complete (`ConcertEntity` decomposition, Shared collapse, `SharedDbContext` deletion, TPH unwind, Auth becomes identity-only, Search upstream refs cleaned)
- **Phase 2 Step 7** — Customer extracted to its own host + DB

That gets Customer code out of B2B's production binary, which is the §1 motivation. Steps 8–11 (in-memory bus, outbox, inbox, s2s auth) can land *after* first B2B launch since they're internal infrastructure work, not user-facing.

Phases 3–5 (Search/Payment/saga/observability) don't block B2B launch at all — they're learning-driven improvements to a system already running.

---

## Estimated calendar time

Roughly **a year of evenings-and-weekends** end-to-end if taken seriously.

- Phase 1 alone is months. `ConcertEntity` decomposition (step 1) is the biggest in-monolith refactor on the path (R7).
- Phase 2 is the first real distributed-systems learning unit — expect debugging time on outbox/inbox semantics.
- Phases 3–5 are progressively faster as the playbook stabilises.

---

## Open questions that surface during execution

Tracked in `MICROSERVICES_ARCHITECTURE.md` §11. The ones likely to bite during execution:

- **Q6** — service-to-service auth scope granularity (decide as it bites)
- **Q7** — event schema versioning concrete mechanism (decide at Step 18)
- **Q8** — DB-per-service cutover operationally (spike at Step 7)

R1 (eventual consistency UX), R5 (flash-sale ticket purchase load) and R6 (TPH unwind sequencing) are the operational risks worth re-reading before starting each phase.
