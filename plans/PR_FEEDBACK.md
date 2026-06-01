# PR #51 `Refactor/Microservices` — Review Feedback & Fix Tracker

> Multi-agent review of PR #51 (`Refactor/Microservices` → `master`). ~15.8k insertions / 8k
> deletions, 1540 files. This file is the canonical fix checklist — update `Status` as items land.
>
> **Status legend:** `[ ]` todo · `[~]` in progress · `[x]` done · `[verify]` needs confirmation before fixing · `[wontfix]` deliberately skipped (note why).

## Recommended execution order

1. **Verify the 3 inferred CRITICALs** — DI1, DI2, OC2 (Duende hash). Quick to confirm; they gate whether the system runs at all.
2. **CRITICALs** — boundary refs, PCI leak, inbox idempotency, non-atomic ticket purchase, review-handler bus mismatch.
3. **HIGHs** — before merge.
4. **Conventions (CV*)** — follow-up commit, lowest risk.

## Progress — re-audited 2026-06-01

A week of unrelated work (integration suite, E2E) landed after the 2026-05-24 pass and
incidentally fixed several items. Full re-audit of every open item against current code:

**Done (verified fixed in code):** OC2, DI1, DI2, OB1, EH1, EH2, EH3, EH4, DI3, DI4, EH5, SB1,
OB2, OB6, OC1, OC3, OC6, OC7, OC8, OC10 — plus SB3, SB4, SB5 fixed incidentally by the service
re-layout. Notes updated on each below.
**Wontfix (6):** DI5, OB5, DI6, DI7, SB2, OC4 — see notes below.
**Still open after re-audit:**
- *Correctness:* OB3, OB4, OB7, OC5, OC9
- *Boundary:* SB6, SB7, SB8
- *Conventions:* CV1, CV2, CV3, CV4, CV5, CV6

---

## 1. Service boundary violations

- [x] **SB1 — CRITICAL — DONE** — `Concertable.Customer.Web.csproj:27`
  *Done: `INotificationClient` moved from `B2B.Notification.Contracts` to `Concertable.Kernel.Notifications`. `NotificationHub`, `SignalRNotificationClient`, `AddNotificationClient()`, and `Log` moved to new `Concertable.Shared.Notification.Infrastructure` shared project. B2B.Web and Customer.Web both reference the shared project. Customer.Web now maps `NotificationHub` at `/hub/notifications` and calls `AddNotificationClient()`. B2B.Notification.Contracts no longer has any consumers; B2B.Notification.Infrastructure is now empty. All four GlobalUsings.cs files updated to `global using Concertable.Kernel.Notifications`. `Authorization.Infrastructure` reference was a false finding — `ICurrentUser` was already in `Concertable.Kernel` before this review.*

- [wontfix] **SB2 — CRITICAL** — `DataAccess.Infrastructure.csproj:23`
  *False finding. `ICurrentUser`, `CurrentUserExtensions`, and `ClaimsPrincipalExtensions` already live in `Concertable.Kernel.Identity`. `DataAccess.Infrastructure.csproj` references only `Concertable.Kernel` — no B2B `Authorization.Contracts` ref exists.*

- [x] **SB3 — HIGH — DONE (incidental)** — `DataAccess.Application.csproj`
  *Re-audit 2026-06-01: fixed by the service re-layout. `DataAccess.Application.csproj` now references only `Concertable.Kernel`; the 7 `.Domain` refs are gone and `IReadDbContext` lives in B2B's own DataAccess module.*

- [x] **SB4 — HIGH — DONE (incidental)** — `Payment.Application.csproj` + `Payment.Infrastructure.csproj`
  *Re-audit 2026-06-01: neither csproj references `Contract.Contracts` anymore, and neither project uses `ContractType`. Resolved by the re-layout.*

- [x] **SB5 — HIGH — DONE (incidental)** — `Customer.Ticket.Infrastructure.csproj`
  *Re-audit 2026-06-01: no `Concertable.Customer.Contracts` project exists under B2B anymore. `Customer.Ticket.Infrastructure.csproj` references only Customer-owned + shared contracts. Resolved by the re-layout.*

- [ ] **SB6 — MEDIUM** — `Concert.Infrastructure/GlobalUsings.cs:10`
  *Re-audit 2026-06-01: narrowed. The csprojs are already clean (only `Payment.Client` + `Payment.Contracts`), and `TransactionTypes`/`PaymentSession` now live in `Payment.Contracts`. Only a stale `global using Concertable.Payment.Domain;` remains on line 10 — no longer needed.*
  **Fix:** Remove the stale global using.

- [ ] **SB7 — MEDIUM** — `Payment.Client.csproj:22`
  gRPC client lib references `Payment.Domain`, leaking domain types (`EscrowStatus`, `PaymentSession`, …) to B2B/Customer consumers.
  **Fix:** Define client-side enums/records in `Payment.Client`; drop the `Payment.Domain` ref.

- [ ] **SB8 — MEDIUM** — `Search.Application.csproj:11` + `Search.Application/GlobalUsings.cs:6` + `Search.Infrastructure/GlobalUsings.cs:11`
  *Re-audit 2026-06-01: dead `B2B.User.Contracts` ref confirmed in `Search.Application.csproj:11` + unused global usings. The `Authorization.Contracts` part of the original finding was wrong — it isn't referenced at all.*
  **Fix:** Remove the dead ref and the unused `Concertable.B2B.User.Contracts` global usings.

## 2. Transactional outbox correctness

- [x] **OB1 — CRITICAL — DONE** — `ConcertReviewProjectionHandler`, `VenueReviewProjectionHandler`, `ArtistReviewProjectionHandler`
  *Done: the 3 handlers set `contextAccessor.Context = context`, then `bus.PublishAsync(...)`, then `SaveChangesAsync` — the outbox row commits atomically with the projection + inbox row in one transaction. Serialization stays abstracted inside `OutboxBus`. Also renamed `IOutboxContextAccessor`→`IDbContextAccessor`, `Current`→`Context`.*

- [x] **OB2 — CRITICAL — DONE** — `Customer.Ticket TicketService.CompleteAsync`
  *Re-audit 2026-06-01: fixed. `CompleteAsync` now opens `unitOfWork.BeginTransactionAsync()`, performs both `concertRepository.SaveChangesAsync()` and `unitOfWork.SaveChangesAsync()` inside it, then `tx.CommitAsync()` with rollback on exception — both saves are atomic.*

- [ ] **OB3 — HIGH** — `Payment WebhookProcessor:53,61+`
  Stripe idempotency record written before event publish, no wrapping transaction → publish failure drops the event silently.
  **Fix:** Idempotency-row insert + outbox-message insert in one `SaveChangesAsync`.

- [ ] **OB4 — HIGH** — `Payment.Web/Program.cs:43` + `WebhookProcessor.cs:17`
  Webhook path uses `AddDirectBusKeyed("webhook")` — non-outbox bus. `PaymentSucceeded/FailedEvent` bypass the outbox, not durable across a crash.
  **Fix:** Route webhook events through the outbox bus.

- [wontfix] **OB5** — `Customer ReviewCreatedDomainEventHandler`
  *False finding (see DI5). Handler is registered under `IDomainEventHandler<T>` (correct — dispatcher requires this). `IPreCommitDomainEventHandler<T>` inherits `IDomainEventHandler<T>`; the dispatcher's `IsAssignableFrom` check gates it to the pre-commit phase. `DomainEventDispatchInterceptor` sets `contextAccessor.Context` before calling `DispatchPreCommitAsync`, so `bus.PublishAsync` stages the outbox row in the review's transaction.*

- [x] **OB6 — HIGH — DONE/wontfix** — `OutboxDispatcher.DrainOnceAsync`
  *Re-audit 2026-06-01: the loop mutates `row.MarkDispatched()`/`row.RecordFailure()` then a single end-of-batch `SaveChangesAsync`. This is acceptable as designed: a redelivered message hits the inbox dedup downstream, so re-publishing a row whose status save failed is idempotent. Not worth the extra round-trips. Closing.*

- [ ] **OB7 — MEDIUM** — `AzureServiceBusReceiver.cs:68-93`
  Deserialization failure (unknown type / malformed JSON) is **abandoned**, not dead-lettered → poison message loops forever if subscription max-delivery-count isn't set.
  **Fix:** Distinguish unretryable deserialization failures and dead-letter them.

## 3. Event handler correctness

- [x] **EH1 — CRITICAL — DONE** — B2B `VerifyPaymentProcessor`, `EscrowPaymentProcessor`, `SettlementPaymentProcessor`, `BookingPaymentFailedProcessor`, `VerifyPaymentFailedProcessor`
  *Done: each handler injects `ConcertDbContext` and checks `(MessageId, nameof(Handler))` against the inbox, returning on a hit. The 4 DB-writing handlers stage the inbox row on `ConcertDbContext` before delegating — the facade's single `SaveChangesAsync` (same scoped context as the repositories) commits the inbox row in the same transaction as the workflow write. `VerifyPaymentFailedProcessor` is notification-only with no DB write to ride: it sends the notification, then writes the inbox row + `SaveChanges` (send-then-record, so a crash between the two never loses the notification — at worst a duplicate on redelivery).*

- [x] **EH2 — CRITICAL — DONE** — Customer `TicketPaymentProcessor` + `TicketPaymentFailedProcessor`
  *Done: `TicketDbContext` now maps `InboxMessageEntity` (`messaging.Inbox`, ExcludeFromMigrations) — mirrors `Customer.Concert.ConcertDbContext`. `TicketPaymentProcessor` checks the inbox, stages the dedup row on `TicketDbContext`; `TicketService.CompleteAsync`'s `ticketRepository.SaveChangesAsync()` commits it atomically with the ticket insert. NOTE: the concert-availability decrement is a separate `concertRepository.SaveChangesAsync()` — that cross-context split is OB2, still open; EH2 only adds the dedup. `TicketPaymentFailedProcessor` is notification-only: send-then-record.*

- [x] **EH3 — HIGH — DONE** — `DomainEventDispatchInterceptor.cs:11`
  *Done: replaced `_pendingEvents` field with `Stack<List<IDomainEvent>> pendingEventsStack`; `SavingChangesAsync` pushes, `SavedChangesAsync` pops. Re-entrant inner `SaveChanges` pushes its own list on top; outer call pops its original list untouched.*

- [x] **EH4 — HIGH — DONE** — all inbox handlers
  *Done: added `catch (DbUpdateException ex) when (ex.IsDuplicateKey())` with `LogDebug` around the save-triggering call in all 7 handlers. DB-writing handlers wrap the module/service call; notification-only handlers wrap `context.SaveChangesAsync`. `AnyAsync` pre-check kept as fast-path.*

- [x] **EH5 — MEDIUM — DONE** — Payment `TicketTransactionHandler`, `VerifyTransactionHandler` (via `TransactionService.LogAsync`)
  *Done: `LogAsync` now calls `GetByPaymentIntentIdAsync(dto.PaymentIntentId)` first and returns early if a row exists, preventing duplicate transaction rows on `PaymentSucceededEvent` redelivery. Escrow/Settlement handlers already guarded separately.*

## 4. DI registration completeness

- [x] **DI1 — CRITICAL — DONE** — `B2B.Web/Program.cs:109-119` + `AppHost/DistributedApplicationBuilderExtensions.cs:32-35`
  *Done: B2B.Web subscribes ReviewSubmitted/Artist/VenueChanged; AppHost adds `concertable-b2b` subs on those 3 topics. NOTE: B2B's 3 ReviewSubmitted handlers will throw until OB1 is fixed.*
  `AddAzureServiceBusTransport` subscribes only `PaymentSucceeded/FailedEvent`. Missing `ReviewSubmittedEvent`, `ArtistChangedEvent`, `VenueChangedEvent` → review/rating projections, artist/venue read-models, `ArtistManagerProfile.ArtistId`/`VenueManagerProfile.VenueId` sync never run. Registered handlers = dead code.
  **Fix:** Add the 3 `SubscribeTo<>()` calls + matching B2B ASB subscriptions in AppHost.

- [x] **DI2 — CRITICAL — DONE** — `Customer.Web/Program.cs:53-58`
  *Done: Customer.Web subscribes ConcertChanged + CustomerRegistered, drops the bogus ReviewSubmitted self-subscription; AppHost adds `concertable-customer` subs on those 2 topics.*
  Doesn't subscribe `ConcertChangedEvent` (→ `ConcertProjectionHandler` never runs) or `CustomerRegisteredEvent`. *Does* subscribe `ReviewSubmittedEvent` — its own outbound event (bug).
  **Fix:** Add `ConcertChangedEvent` + `CustomerRegisteredEvent` subscriptions; remove `ReviewSubmittedEvent`.

- [x] **DI3 — HIGH — DONE** — `Search AutocompleteServiceFactory.cs:16` + `Search ServiceCollectionExtensions.cs`
  *Done: changed `AddScoped<IAutocompleteService, AllAutocompleteService>()` to `AddKeyedScoped<IAutocompleteService, AllAutocompleteService>((HeaderType?)null)` so `GetRequiredKeyedService<IAutocompleteService>(null)` resolves.*

- [x] **DI4 — HIGH — DONE** — `Search.Web/Program.cs` + `Search.Workers/Program.cs`
  *Done: added `services.AddSingleton(TimeProvider.System)` in `AddSearchModule` — covers both Search.Web and Search.Workers.*

- [wontfix] **DI5** — `Customer Review ServiceCollectionExtensions.cs:38`
  *False finding. `DomainEventDispatcher.DispatchPhaseAsync` resolves all `IDomainEventHandler<T>` services then filters by `IPreCommitDomainEventHandler<T>.IsAssignableFrom(handler.GetType())` — registration under the base interface is required and correct. Registering under `IPreCommitDomainEventHandler<T>` would cause `GetServices(IDomainEventHandler<T>)` to miss the handler entirely.*

- [wontfix] **DI6** — `B2B.Web/Program.cs:122`
  *False finding. `DevDbInitializer` injects `InboxDbContext` from DI and calls `MigrateAsync` on it at startup — the registration is required for migration execution, not handler injection (handlers use their module contexts). Registration stays.*

- [wontfix] **DI7** — `AppHost/DistributedApplicationBuilderExtensions.cs:108`
  *False finding. B2B.Workers calls `AddInMemoryTransport()` only — no `AddAzureServiceBusTransport`, no `SubscribeTo<>()` calls, outbox dispatcher disabled (`runDispatcher: false`). No ASB reference required.*

## 5. Conventions

- [ ] **CV1** — Primary constructors in newly-touched services/handlers/repos/DbContexts. Hit list: every Customer `*DbContext`, `SearchDbContext`, `PaymentDbContext`; `DomainEventDispatchInterceptor`, `AuditInterceptor`, `BaseRepository`, `UnitOfWork*`, `DomainEventDispatcher`; ~13 B2B handlers/modules/repos. → Explicit ctor + `private readonly` + `this.field = param`.
- [ ] **CV2** — Underscore fields: `ClientCredentialsTokenService` (5 fields), `DomainEventDispatchInterceptor._pendingEvents`, `ReviewEntity._events`. → Drop `_`.
- [ ] **CV3** — `ValueGeneratedNever()` missing on `ArtistReadModel`/`VenueReadModel`/`ConcertReadModel` Id configs and `TicketEntityConfiguration` (migration shows `ValueGeneratedOnAdd`). → Add it.
- [ ] **CV4** — Service-layer Response types: `IConcertService`/`IApplicationService` return `ConcertPostResponse`/`Checkout` from `Application.Responses`; Payment `PaymentResponse`/`EscrowResponse`. → Rename to `Result` / move to `Api/Responses/`.
- [ ] **CV5** — Duplicate `global using` lines in 4+ `GlobalUsings.cs` (Customer Concert/Profile/Review/Ticket Infrastructure, Payment Infrastructure). → De-dup.
- [ ] **CV6** — `public` schema classes that should be `internal`; `[Table("Reviews")]` annotation on `ReviewEntity` domain class. → Tidy.

## 6. Other correctness / PCI (outside the 5, but blocking)

- [x] **OC1 — CRITICAL — DONE** — `AppHost.Shared/DistributedApplicationBuilderExtensions.cs`
  *Re-audit 2026-06-01: fixed. Stripe secrets (`Stripe:SecretKey`, `Stripe:WebhookSecret`, `ExternalServices:UseRealStripe`) are passed only to `AddPaymentWeb`; B2B's `AddApi` no longer receives them.*

- [x] **OC2 — CRITICAL — DONE** — `Auth/Config.cs:109-113`
  *Done: `Sha256` now returns `Convert.ToBase64String(hash)` instead of hex.*
  `ServiceClient` hashes the client secret as **hex**; Duende's `HashedSharedSecretValidator` expects **Base64** (`secret.Sha256()`). Every `client_credentials` token request → 401 → all gRPC Payment calls fail.
  **Fix:** `new Secret(clientSecret.Sha256())` using Duende's extension.

- [x] **OC3 — CRITICAL — DONE** — `Payment.Application/Interfaces/Webhook/IStripeApiClient.cs`
  *Re-audit 2026-06-01: fixed. `IStripeApiClient` is now `internal`; the `B2B.Web`/`Concert.Infrastructure` IVTs are gone from `Payment.Application/AssemblyInfo.cs`. `Stripe.net` remaining in `Payment.Application` is fine now that nothing outside Payment can see the Stripe-typed surface.*

- [wontfix] **OC4 — HIGH** — `B2B.Web/appsettings.Production.json:3`
  *Re-audit 2026-06-01: harmless in practice. Prod reads `B2BDb` from AppHost-injected env vars, not from appsettings, so the stale `DefaultConnection` key is never consulted. Left as-is for now; rename at deploy-hardening time. Not blocking.*

- [ ] **OC5 — HIGH** — `ConcertChangedDomainEventHandler.cs:27-28`
  `e.TotalTickets` passed for both `TotalTickets` and `AvailableTickets` on `ConcertChangedEvent` → Search always shows concerts as fully available.
  **Fix:** Decide ownership of `AvailableTickets` for projection — likely a separate Customer-published event; at minimum stop silently passing `TotalTickets` twice.

- [x] **OC6 — HIGH — DONE** — `Customer ConcertReviewService.CreateAsync`
  *Re-audit 2026-06-01: fixed. `ReviewEntity.Create` now receives `artistId: ticket.ArtistId, venueId: ticket.VenueId` from the purchased ticket.*

- [x] **OC7 — HIGH — DONE** — `Customer TicketService.PurchaseAsync`
  *Re-audit 2026-06-01: fixed (different layer than proposed). `CustomerPaymentService.PayAsync` merges `fromUserId`/`fromUserEmail` into the metadata before creating the intent, so the processor's `meta["fromUserId"]` lookup succeeds.*

- [x] **OC8 — HIGH — DONE** — `Customer.Web/Program.cs`
  *Re-audit 2026-06-01: fixed. Auth `Config.cs` now registers both an `ApiScope` and an `ApiResource` named `concertable.customer.api`, matching Customer.Web's JWT `Audience`.*

- [ ] **OC9 — HIGH** — `ConcertReadModel.cs:9` + `ConcertProjectionHandler.cs`
  `ConcertReadModel.BookingId` never set (`ConcertChangedEvent` carries no `BookingId`) → always 0.
  **Fix:** Remove the unused field, or expand `ConcertChangedEvent` with `BookingId` if downstream needs it.

- [x] **OC10 — LOW — DONE** — old `api/Modules/`
  *Re-audit 2026-06-01: the `api/Modules/` directory no longer exists; modules now live under each service root. Nothing orphaned.*

---

## Notes / open questions

- B2B→Customer in-process `ICustomerModule.GetUserIdsByLocationAndGenresAsync` call (preference module) is **known/accepted/deferred** per `MICROSERVICE_STEPS_CONT.md` Step 22 — not a finding.
- Conflicts between reviewers: the B2B reviewer rated the review-projection-handler ordering "correct"; the messaging reviewer caught the deeper bus-type/context mismatch (OB1). OB1's analysis supersedes — confirm during the fix.
