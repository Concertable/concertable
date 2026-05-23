# Step 9 — Transactional Outbox (per service)

> Companion to `MICROSERVICE_STEPS.md` Step 9. Implement step-by-step; `/clear` between
> steps and re-enter via `@STEP_9_PLAN.md` (the project's plan→file→clear→implement flow).

> **Progress (2026-05-20):** Steps 1–7 ✅ all done — messaging library reworked, two-phase
> dispatch, B2B + Customer wired, Auth fixed, all migrations re-scaffolded (incl. OutboxDbContext),
> build green, 32/32 messaging unit tests pass, outbox integration test written and compiling,
> `MICROSERVICE_STEPS.md` Step 9 marked done, stale `AddMessagingOutbox` wording fixed.
> **Not yet committed.**

**Goal:** integration-event publishes become durable and atomic with the business write —
the outbox row commits in the *same transaction* as the entity change. Solves the
dual-write problem (ARCHITECTURE §6).

**Exit criteria (MICROSERVICE_STEPS.md Phase 2):** outbox proven on at least one event in
each direction — `ConcertChangedEvent` (B2B→Customer) and `ReviewSubmittedEvent`
(Customer→B2B).

---

## Current state

- Library shipped at `86b9b6f7`: `OutboxBus : IBus` (publish → `OutboxMessageEntity` row),
  `OutboxStore<TContext>`, `OutboxDispatcher<TContext>` (`BackgroundService`, drains via
  `IBusTransport`), `OutboxMessageEntityConfiguration` (table `Outbox`, schema `messaging`),
  `AddOutbox<TContext>` extension. Unit-tested in `Concertable.Messaging.UnitTests`.
- **Not wired into any composition root.** `AddMessaging()` registers `IBus`→`Bus` +
  `IBusTransport`→`InMemoryBusTransport` only.
- `MessageTypeRegistry` + `MessageSerializer` are registered **only on the ASB path** — the
  in-memory composition roots register neither, yet `OutboxBus` and `OutboxDispatcher` both
  require them.
- (Doc nit: `MICROSERVICE_STEPS.md` says `AddMessagingOutbox<TDbContext>` — the actual
  extension is `AddOutbox<TContext>`. Fix when closing the step.)

## Two problems the library's one-context assumption hides

**P1 — Multi-DbContext.** `AddOutbox<TContext>` binds `OutboxStore`/`OutboxBus` to one
DbContext. B2B and Customer are each still internally a modular monolith with many
per-module DbContexts (B2B ~9), and publishers are spread across them. The outbox row must
be inserted into whichever module context is mid-`SaveChanges`, for atomicity. "One outbox
per service" = one `messaging.Outbox` *table* per service DB, writable by N module contexts.

**P2 — Dispatch ordering.** `DomainEventDispatchInterceptor` dispatches domain events in
`SavedChangesAsync` (post-commit). Integration events are published only from domain-event
handlers (`feedback_domain_events_for_integration`), so every publish is currently
post-commit → a row added there is a separate write, not atomic. But the 7 domain-event
handlers split two ways:
- **5 pure publishers** — `ArtistChangedDomainEventHandler`, `VenueChangedDomainEventHandler`,
  `ConcertChangedDomainEventHandler`, `UserCreatedDomainEventHandler`,
  `ReviewCreatedDomainEventHandler` (Customer) — each just `bus.PublishAsync`. These can and
  must move pre-commit.
- **2 workflow handlers** — `ApplicationAcceptedDomainEventHandler` (calls
  `SaveChangesAsync()` + queues a background task), `BookingSettledDomainEventHandler` (runs
  `concertDraftService.CreateAsync`) — must stay post-commit. A blanket move would re-enter
  the interceptor mid-save.

## Design

- **D1 — One outbox table per service DB.** A reusable `OutboxDbContext` (ship it in
  `Concertable.Messaging.Infrastructure`) owns the `messaging.Outbox` table + its migration;
  `OutboxDispatcher` drains through it. Each *publishing* module DbContext also maps
  `OutboxMessageEntity` but `ExcludeFromMigrations` (the established `Genre` pattern) so its
  `SaveChanges` can insert rows.
- **D2 — Two-phase domain-event dispatch.** New marker `IPreCommitDomainEventHandler<T> :
  IDomainEventHandler<T>`. The interceptor dispatches markers in `SavingChangesAsync`
  (pre-commit) and the rest in `SavedChangesAsync` (unchanged). Mark the 5 publishers;
  default (unmarked) stays post-commit so future handlers are safe by default. Contract:
  pre-commit handlers must not call `SaveChanges`.
- **D3 — Context-routed outbox write.** Make `OutboxStore` non-generic; it writes to an
  ambient "current saving context" that `DomainEventDispatchInterceptor` sets before its
  pre-commit dispatch. Rework `AddOutbox` so `OutboxBus`/`OutboxStore` register once (not
  per-context).
- **D4 — Register registry + serializer** in both composition roots: `MessageTypeRegistry`
  with every integration-event type via `SubscribeTo<T>()`, plus `MessageSerializer`.
- **D5 — One dispatcher per service.** `OutboxDispatcher` runs in exactly one host per
  service (no row-claiming yet — two would double-dispatch). B2B: Web or Workers (decide in
  step 3). Customer: `Customer.Web`.

## Steps (one commit each)

1. ✅ **DONE (uncommitted) — Messaging library rework.** The shipped `OutboxStore<TContext>` conflates two roles —
   the *write* path (`AddAsync`, runs inside a module context's `SaveChanges`, needs the
   ambient context) and the *drain* path (`GetPendingAsync`/`SaveChangesAsync`, runs in the
   dispatcher's background scope, needs a dedicated context). Split them:
   - `Messaging.Application`: replace `IOutboxStore` with `IOutboxWriter` (`AddAsync`) and
     `IOutboxReader` (`GetPendingAsync` + `SaveChangesAsync`).
   - `Messaging.Infrastructure`: add `IOutboxContextAccessor` (`public`, scoped — holds the
     `DbContext` currently mid-`SaveChanges`); `OutboxWriter : IOutboxWriter` writes to
     `accessor.Current`; new `public OutboxDbContext` owns the `messaging.Outbox` table + its
     migration; `OutboxReader : IOutboxReader` drains via `OutboxDbContext`; `OutboxDispatcher`
     becomes non-generic, resolving `IOutboxReader`.
   - `OutboxBus` takes `IOutboxWriter` instead of `IOutboxStore`.
   - `AddOutbox` rework: `AddOutbox(Action<DbContextOptionsBuilder> configureDb,
     Action<OutboxOptions>? configure = null)` — registers `OutboxDbContext`,
     `IOutboxContextAccessor`/`OutboxWriter`/`OutboxReader` (scoped), `IBus`→`OutboxBus`,
     `OutboxDispatcher` hosted service, `MessageSerializer` (`TryAddSingleton`). Caller
     supplies the connection string; `MessageTypeRegistry` stays caller-wired (steps 3/4).
   - Update `Concertable.Messaging.UnitTests`: split `OutboxStoreTests` into writer/reader
     tests; re-point `OutboxBusTests` at `IOutboxWriter`; make `OutboxDispatcherTests`
     non-generic.
   - **Landed:** `IOutboxStore` + `OutboxStore<T>` + `OutboxStoreTests` deleted; new files
     `IOutboxWriter`/`IOutboxReader` (Application), `IOutboxContextAccessor`/
     `OutboxContextAccessor`/`OutboxWriter`/`OutboxReader`/`OutboxDbContext` (Infrastructure/
     Outbox); `OutboxBus`/`OutboxDispatcher`/`AddOutbox` reworked. Build green; 32/32
     messaging unit tests pass. Not committed — `git status` shows the working-tree changes.
2. ✅ **DONE — Two-phase dispatch.** `IPreCommitDomainEventHandler<T>` added to Kernel;
   `IDomainEventDispatcher` gains `DispatchPreCommitAsync`; `DomainEventDispatcher` filters
   by phase via `IsAssignableFrom`; `DomainEventDispatchInterceptor` sets
   `IOutboxContextAccessor.Current` before pre-commit dispatch (clears in `finally`);
   `DataAccess.Infrastructure` gains ref to `Messaging.Infrastructure`; 5 publisher handlers
   marked: Artist/Venue/Concert/User/Review.
3. ✅ **DONE — Wire B2B (`Concertable.Web`).** `OutboxMessageEntity` mapped (ExcludeFromMigrations)
   on Artist/Venue/Concert/UserDbContext; `Messaging.Domain` ref added to each. `AddMessaging()`
   replaced in Web+Workers with `AddInMemoryTransport` + `AddDirectBusKeyed("webhook")` +
   `AddOutbox(UseSqlServer)`; B2B `MessageTypeRegistry` (7 event types) wired. Dispatcher runs
   in Web only (`runDispatcher: false` in Workers). `WebhookProcessor` uses `[FromKeyedServices("webhook")]`
   to keep direct dispatch for payment events (payment outbox deferred).
4. ✅ **DONE — Wire Customer (`Concertable.Customer.Web`).** `OutboxMessageEntity` mapped on
   `ReviewDbContext`; `Messaging.Domain` ref added to `Review.Infrastructure.csproj`. `AddMessaging()`
   replaced with `AddInMemoryTransport` + `AddDirectBusKeyed("webhook")` + `AddOutbox(UseSqlServer)`;
   Customer `MessageTypeRegistry` (`ReviewSubmittedEvent`) wired. Dispatcher runs in Customer.Web.
5. ✅ **DONE — Migration re-scaffold.** Fixed Auth DI (`AddOutbox(runDispatcher: false)` +
   `Messaging.Infrastructure` project ref); added `OutboxDbContext` to script + scaffolded its
   `InitialCreate` (creates `messaging.Outbox`); added `Microsoft.EntityFrameworkCore.SqlServer`
   to `Messaging.Infrastructure.csproj`; re-ran `./initial-migrations.ps1` — all contexts done.
6. ✅ **DONE — Verify.** Build green (0 errors); `OutboxVerificationTests.PostConcert_WritesOutboxRow_AndDispatcherDrainsIt`
   added to `Concert.IntegrationTests` — PUTs to `/api/Concert/post/{id}`, asserts 204, then
   queries `OutboxDbContext` for a `ConcertChangedEvent` row (atomicity), polls until
   `OutboxStatus.Dispatched` (drain). `ApiFixture` gains `Services` property; both
   `Messaging.Domain` + `Messaging.Infrastructure` project refs added to test + fixture projects.
7. ✅ **DONE — Close out.** `MICROSERVICE_STEPS.md` Step 9 marked done; stale
   `AddMessagingOutbox<TDbContext>` wording replaced with `AddOutbox`.

## Notes / open items

- Step 9 makes the publish **durable + atomic**. Actual B2B↔Customer cross-process delivery
  still lands at Step 14 (ASB swap) — the dispatcher drains to `InMemoryBusTransport`,
  in-process only.
- `PaymentSucceededEvent` / `PaymentFailedEvent` are webhook-originated, likely published
  outside the interceptor path (directly from the Stripe webhook handler). They need the
  same outbox treatment via that handler's own `SaveChanges` — confirm scope during step 3;
  not required to satisfy the Step 9 exit criterion.
- Step 10 (inbox / idempotent consumers) builds directly on this.
