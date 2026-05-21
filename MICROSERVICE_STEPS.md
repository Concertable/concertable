# Microservice Migration Steps

> **Companion to** [MICROSERVICES_ARCHITECTURE.md](MICROSERVICES_ARCHITECTURE.md). That doc is the *what*; this one is *what order, in phases*.
>
> **Status:** Phase 1 COMPLETE ✅. Phase 2 COMPLETE ✅ (Steps 7–11). Phase 3 COMPLETE ✅ (Steps 12–13). Phase 4 Step 14 COMPLETE ✅. Next: Step 15 (Concert lifecycle saga).
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

7. ~~**Extract `Concertable.Customer.Api` + `Concertable.Customer.Workers`** to their own host + own DB.~~ **DONE 2026-05-19** across 4 commits:
   - `8da35e0a` (7a–7e: ConcertChangedEvent expansion, Customer.Ticket off B2B nav chain, IPaymentSucceededProcessor dispatcher retired, Payment/Contract.Contracts/Concert refs trimmed)
   - `ea7ffecd` (7g/7h: Aspire CustomerDb resource + 4 module DbContexts bound to `ConnectionStrings:CustomerDb` + csproj audit)
   - `e5676305` (forwarder retirement: `IConcertModule`'s 4 review-forward methods deleted; consumer-facing list+eligibility endpoints relocated from B2B `Artist/VenueReviewsController` to new controllers under `Customer.Review.Api`. B2B keeps `/summary` only)
   - `8573e472` (Payment + AuthorizationModule decoupled from B2B service-specific facades; Customer.Web composition root wired; all 13 contexts re-scaffold cleanly via `./initial-migrations.ps1`)

   Cross-cutting wins during Step 7: `Concertable.Authorization.Infrastructure` and `Concertable.Payment.Infrastructure` are now clean shared libraries — zero `IUserModule`/`ICustomerModule` injection. Payment owns its own email projection via `PayoutAccountEntity.Email` populated through integration events. Plan + sub-step trace in `STEP_7_PLAN.md`.

   **Open follow-up (Step 8 territory):** Customer.Web has no `IDbInitializer` invocation at startup; no Customer-side dev/test seeders exist yet for Customer.Concert/Ticket/Review/Profile. Pick up alongside the bus + outbox work.
8. ~~**Bus on in-memory transport.**~~ **DONE 2026-05-20.** `IBus` is the publish seam; `IBusTransport` is the swappable delivery mechanism. `InMemoryBusTransport` dispatches to `IIntegrationEventHandler<T>` *within the same process* — it exercises pub/sub semantics without a broker. The B2B↔Customer cross-process hop is not the in-memory transport's job; it lights up when the transport is swapped to Azure Service Bus at Step 14, publishers and handlers unchanged. Skip cloud broker latency while learning publish/subscribe semantics. **Bus choice locked at `517201db` (2026-05-19): ASB SDK + our own `IBusTransport` abstraction; not MassTransit.** Reasoning: MassTransit v9 went partially commercial, this is a learning project, our abstraction shape is already broker-agnostic (proven with `InMemoryBusTransport`). The seam itself (`IBus`, `IBusTransport`, `MessageEnvelope`, `IIntegrationEvent`/`IIntegrationCommand`) shipped at `517201db` with the in-memory transport. The production ASB transport (`Concertable.Messaging.AzureServiceBus`) shipped at `70d05425` (2026-05-20) — sender + receiver implemented against the locked seam, **not wired into any composition root yet**, sits in tree ready for Step 14 cutover.

   **Kernel-split housekeeping (2026-05-20): COMPLETE — `KERNEL_SPLIT_PLAN_V2.md` closed.** A detour off the migration path: the adapter family was extracted from `Concertable.Kernel` across nine commits — `952b75fb` (A: `Concertable.Seeding` IModuleSeeder relocation + C: `Concertable.Shared.Blob` incl. BlobDevSeeder + bundled v1 DataAccess scaffold), `6ba3735e` (B: `Concertable.Shared.Email`), `d7d69ca4` (D: `Concertable.Shared.Geocoding`), `18ca38d4` (E: `Concertable.Shared.Imaging` — temp Kernel→Shared.Blob.Application ref dissolved), `858fcce6` (F: `Concertable.Shared.Pdf` generic + Customer.Ticket ITicketPdfService/ITicketEmailSender composition + `IEmailService`→`IEmailSender` rename), `d4f254eb` (G: test-helper rename — `Tests.Common`→`Concertable.Testing`, `IntegrationTests.Common`→`Concertable.Testing.Integration`), `c40f40d8` (H: `IUriService` cross-assembly namespace leak fixed; surfaced + cleaned a dead `global using` in two Customer projects), `950f9655` (I: deleted the empty `Concertable.Data.{Application,Infrastructure}` stubs — migration re-scaffold skipped, no model change). Build green throughout. Kernel now has zero adapter coupling. This is in-Kernel shape-up, not a Step 8 deliverable — but it happened here because it unblocks per-service host composition roots. Deferred kernel-adjacent extractions (`BackgroundTasks`, `AspNetCore`, `Observability`) are out of scope for that plan and tracked separately.

   **Step 8 ✅ DONE 2026-05-20:** `AddMessaging()` registers `IBus` + `InMemoryBusTransport` in both `Concertable.Web` and `Concertable.Customer.Web` composition roots. Canonical flow: `ConcertChangedEvent` — B2B's `ConcertChangedDomainEventHandler` publishes via `IBus`, Customer's `ConcertProjectionHandler` (`IIntegrationEventHandler<ConcertChangedEvent>`) consumes it. In-memory transport delivers in-process only; the B2B→Customer hop becomes live at the Step 13 ASB transport swap with no handler/publisher changes — the `IBusTransport` seam absorbs it.
9. ~~**Transactional outbox** in each service's own DB.~~ **DONE 2026-05-20.** Library base shipped at `86b9b6f7`; reworked and wired per-service in this phase. `OutboxStore<T>` split into `IOutboxWriter` (ambient pre-commit write) + `IOutboxReader` (drain via dedicated `OutboxDbContext`); `IPreCommitDomainEventHandler<T>` marker drives two-phase dispatch from `DomainEventDispatchInterceptor` — 5 pure-publisher handlers moved pre-commit, 2 workflow handlers stay post-commit. `AddOutbox(configureDb)` wires `OutboxBus`/`OutboxWriter`/`OutboxReader`/`OutboxDispatcher` in both `Concertable.Web` and `Concertable.Customer.Web`; `OutboxDbContext` owns the `messaging.Outbox` migration. Publishing module DbContexts (Artist/Venue/Concert/User on B2B, Review on Customer) map `OutboxMessageEntity` with `ExcludeFromMigrations` for atomic in-transaction inserts. Proven by `OutboxVerificationTests.PostConcert_WritesOutboxRow_AndDispatcherDrainsIt` — asserts atomic `ConcertChangedEvent` row write + `OutboxDispatcher` drain within 5 s. Solves the dual-write problem (§6 callout). Full trace in `STEP_9_PLAN.md`.
10. ~~**Idempotent consumers** with inbox state per service.~~ **DONE 2026-05-20.** Inbox write is atomic with each handler's business write — no separate `InboxDbContext` save. `InboxMessageEntity` uses composite key `(MessageId, ConsumerName)` so each handler gets its own independent dedup row for the same event. `IIntegrationEventHandler<T>.HandleAsync` gains a `MessageEnvelope envelope` parameter; handlers read `envelope.MessageId` for the pre-check `AnyAsync` and the insert. Nine handlers with their own DbContext (projection + sync + profile-creation) do pre-check + inbox insert + business write in a single `SaveChanges`. Eight handlers whose idempotency is guaranteed by the downstream service (Stripe API calls, module workflow facades) just add the `envelope` param with no inbox insert. `InboxDbContext` is retained as migration owner only — consumer DbContexts map `InboxMessageEntity` with `ExcludeFromMigrations` (mirrors the outbox write-path). `DomainEventDispatchInterceptor` re-entrancy bug fixed (save/restore previous `outboxContextAccessor.Current` instead of hard-null). `initial-migrations.ps1` splits Outbox + Inbox into separate `Data/Migrations/Outbox` and `Data/Migrations/Inbox` subdirs to avoid `CS0579` class name collision. `AzureServiceBusReceiver` constructs `MessageEnvelope` from ASB message properties and passes it through to handlers. 32/32 `Concertable.Messaging.UnitTests` pass.
11. ~~**Service-to-service auth** wired for the new Customer → B2B / Payment sync calls (where they exist). `client_credentials` via Duende per §5.5.~~ **DONE 2026-05-20.** No Customer → B2B/Payment HTTP sync calls exist yet — Customer still loads Payment.Infrastructure in-process. Infrastructure only: `ITokenService` (interface) + `ClientCredentialsTokenService` (Singleton, double-checked lock cache, `IHttpClientFactory`) added to `Concertable.Kernel`. `payment:write` ApiScope added to Duende IS; `ServiceClient(clientId, secret, scopes)` factory added to `Config.cs` (SHA-256 hashed secret via `System.Security.Cryptography`). `concertable-b2b` and `concertable-customer` service clients registered in Auth `Program.cs`. AppHost passes `ServiceAuth__ClientId` + `ServiceAuth__ClientSecret` (mapped from per-service keys) to both services and `ServiceAuth__B2BClientSecret` + `ServiceAuth__CustomerClientSecret` to Auth; dev secrets in `appsettings.Development.json`. Both `Concertable.Web` and `Concertable.Customer.Web` call `AddClientCredentials(opts => ...)`. The `ITokenService` is ready to inject wherever Step 14 (Payment extraction) needs to make the first real HTTP call.

**Exit criteria:** Customer runs as its own process. Tickets/reviews/customer profile no longer in B2B's binary. Bus carries projection updates and ticket events. Two DBs (B2B + Customer). Outbox/inbox proven on at least one event in each direction.

---

## Phase 3 — Second extraction: Search

12. ~~**Extract `Concertable.Search.Api` + `Concertable.Search.Workers`** to their own host + own DB.~~ **DONE 2026-05-20.** `Concertable.Search.Web` (ASP.NET Web SDK, query API) + `Concertable.Search.Workers` (Worker SDK, event consumer) extracted as separate Aspire resources. `SearchDb` is a dedicated SQL Server database resource; `AddSearchModule` switched from `DefaultConnection` to `SearchDb`. All 5 EF configs flipped from `[artist]`/`[venue]`/`[concert]` schemas to `[search]` schema — Search now owns its projection tables. `IRatingProjectionConfigurationProvider` eliminated: 3 Search-owned rating projection EF configs (`ArtistRatingProjectionConfiguration`, `VenueRatingProjectionConfiguration`, `ConcertRatingProjectionConfiguration`) replace it. `ConcertChangedEvent` expanded with `Avatar`, `AvailableTickets`, `Latitude`, `Longitude`, `Genres` so Search.Workers has everything it needs to upsert without B2B access. 4 projection handlers added (`ArtistProjectionHandler`, `VenueProjectionHandler`, `ConcertProjectionHandler`, `RatingProjectionHandler`) — all follow inbox dedup + upsert pattern; `RatingProjectionHandler` maintains running average `(oldAvg * count + stars) / (count + 1)`. `SearchDbContext` write-capable (SaveChanges restored; read path stays `.AsNoTracking()` on `ISearchDbContext` properties). `Search.Workers` registers `MessageTypeRegistry` for 4 events + `AddInMemoryTransport` + `AddInbox(SearchDb)`. `Search.Web` removed from `Concertable.Web` composition root; B2B no longer references `Search.Api`. `initial-migrations.ps1` updated to scaffold `SearchDbContext` from `Concertable.Search.Web` as startup project. Cross-process event flow is deferred until Step 13 ASB cutover — Workers uses in-memory transport at this stage.
13. ~~**Switch transport to Azure Service Bus.**~~ **DONE 2026-05-20.** `AddInMemoryTransport()` replaced with `AddAzureServiceBusTransport(...)` in all three hosts: B2B (`Concertable.Web`, publish-only — empty subscriber registry), Customer (`Concertable.Customer.Web`, subscribes `ReviewSubmittedEvent`), Search Workers (`Concertable.Search.Workers`, subscribes 4 events). `AzureServiceBusOptions` switched from `required init` to regular `set` to work with `IOptions<T>` configure pattern. Topic prefix changed from `"event."` to `"event-"` (Aspire ASPIRE006 — dots invalid in resource names). AppHost: `Aspire.Hosting.Azure.ServiceBus` added; `AddServiceBus()` extension configures the 4 subscriber topics with subscriptions and runs the emulator. All three services wire `WithReference(asb).WaitFor(asb)`. 42/42 messaging tests pass.

**Exit criteria:** Search runs as its own process. Three audience-facing services (B2B, Customer, Search). All cross-process event flow live over ASB.

---

## Phase 4 — Production-grade infrastructure

14. ~~**Extract `Concertable.Payment.Api` + `Concertable.Payment.Workers`** to its own host + own DB + own Stripe webhook endpoint. PCI scope shrinks dramatically.~~ **DONE 2026-05-21.** `Payment.Contracts` phased out (types redistributed to `Payment.Domain`/`Payment.Application.DTOs`/`Payment.Client`). `Payment.Client` classlib with gRPC adapters (`IManagerPaymentClient`, `ICustomerPaymentClient`, `IEscrowClient`). `Payment.Web` host (gRPC + HTTP controllers, Kestrel Http1AndHttp2, ServiceToken policy). `Payment.Workers` host (ASB subscriber for 5 events, inbox + outbox). B2B and Customer updated to call `AddPaymentClient()` and inject `IXClient` interfaces. `FakeStripeTransferClient` added (fixes pre-existing DI validation bug in fake path). `PaymentDbContext` migration re-scaffolded against `Payment.Web` startup project. AppHost wired with `AddPaymentWeb`/`AddPaymentWorkers`, `PaymentDb`, 5 new ASB topics. Four audience-facing services (B2B, Customer, Search, Payment). PCI scope contained to `Payment.Web` + `Payment.Workers`.
15. **One saga** for the concert lifecycle (Posted → Settled) — long-running orchestration with persistent state. Hand-rolled state machine + storage (own `IBusTransport` seam; no MassTransit).
16. **OpenTelemetry distributed tracing** across all running services. Watch one ticket-purchase flow end-to-end through B2B + Customer + Payment + Search.

**Exit criteria:** five services running on production-grade infra. PCI scope contained to Payment. Cross-service flows observable.

---

## Phase 5 — Hardening + Notification

17. **Hard event-schema migration.** Change one event's shape with consumers running both old and new versions concurrently. Pick a versioning mechanism (V1/V2 type names? CloudEvents headers? upcaster?) — open per Q7 in ARCHITECTURE.md.
18. **Extract `Concertable.Notification`** when email volume / template management / vendor swap pressure becomes concrete. Auth's direct SMTP/SendGrid call gets replaced by `EmailVerificationRequestedEvent` to bus; Notification subscribes.

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
- **Q7** — event schema versioning concrete mechanism (decide at Step 17)
- **Q8** — DB-per-service cutover operationally (spike at Step 7)

R1 (eventual consistency UX), R5 (flash-sale ticket purchase load) and R6 (TPH unwind sequencing) are the operational risks worth re-reading before starting each phase.
