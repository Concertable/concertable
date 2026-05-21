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

## Progress — updated 2026-05-21

**Done (4/30):** OC2, DI1, DI2, OB1 — see each item's note below.
**Verified:** `Concertable.B2B.Web` + `Concertable.Messaging.UnitTests` build clean (0 errors).
**Next:** EH1 / EH2.
**Whole-solution build is NOT green — pre-existing, unrelated to these fixes:**
- `Concertable.Organization.UnitTests`: `OrganizationEntity does not exist` compile error (pre-existing on the branch; Organization module untouched by this review).
- `Concertable.E2ETests.dll`: file-lock from a stray `testhost` process — environmental, not a code error.

---

## 1. Service boundary violations

- [ ] **SB1 — CRITICAL** — `Concertable.Customer.Web.csproj:33-34`
  Customer.Web references B2B's `Authorization.Infrastructure` **and** `Notification.Infrastructure` (impl assemblies, not Contracts); calls `AddNotificationModule()` which registers B2B's `SignalRNotificationModule`/`IHubContext<NotificationHub>`.
  **Fix:** Extract what Customer needs behind shared contracts or Customer-owned wrappers; drop the two `.Infrastructure` project refs.

- [ ] **SB2 — CRITICAL** — `DataAccess.Infrastructure.csproj:23`
  Shared infra lib references B2B's `Authorization.Contracts` (for `ICurrentUser` in `AuditInterceptor`). Every non-B2B service transitively depends on a B2B module.
  **Fix:** Move `ICurrentUser` (+ `CurrentUserExtensions`, `ClaimsPrincipalExtensions`) into `Concertable.Kernel` or `Concertable.Contracts`; drop the project ref.

- [ ] **SB3 — HIGH** — `DataAccess.Application.csproj:10-17`
  Shared `DataAccess.Application` references 7 service-specific `.Domain` assemblies (User/Artist/Venue/Concert/Contract/Payment/Customer) because `IReadDbContext` (a B2B concern) lives in a shared project.
  **Fix:** Move `IReadDbContext` into B2B; strip the 7 domain project refs.

- [ ] **SB4 — HIGH** — `Payment.Application.csproj:13` + `Payment.Infrastructure.csproj:33`
  Payment references B2B's `Contract.Contracts` (for `ContractType`).
  **Fix:** Promote `ContractType` to `Concertable.Contracts`; drop the B2B ref from both csprojs.

- [ ] **SB5 — HIGH** — `Customer.Ticket.Infrastructure.csproj:3619`
  Customer references `Concertable.Customer.Contracts` which lives inside `Concertable.B2B/Modules/Customer/` and exposes `ICustomerModule`.
  **Fix:** Remove the cross-service reference; whatever Customer needs is either Customer-owned or a shared contract.

- [ ] **SB6 — MEDIUM** — `Concert.Application.csproj:20` / `Concert.Infrastructure.csproj:30` + GlobalUsings
  B2B's Concert module references `Payment.Domain` for `TransactionTypes`/`PaymentSession`/event types.
  **Fix:** Move those types into `Payment.Client` (or a Payment contracts lib); B2B references `Payment.Client` only.

- [ ] **SB7 — MEDIUM** — `Payment.Client.csproj:22`
  gRPC client lib references `Payment.Domain`, leaking domain types (`EscrowStatus`, `PaymentSession`, …) to B2B/Customer consumers.
  **Fix:** Define client-side enums/records in `Payment.Client`; drop the `Payment.Domain` ref.

- [ ] **SB8 — MEDIUM** — `Search.Application.csproj:11` + `Search.Application/GlobalUsings.cs:5` + `Search.Infrastructure/GlobalUsings.cs:8-9`
  Dead `User.Contracts` project ref + unused `global using`s (incl. `Authorization.Contracts` not even referenced).
  **Fix:** Remove the dead ref and global usings.

## 2. Transactional outbox correctness

- [x] **OB1 — CRITICAL — DONE** — `ConcertReviewProjectionHandler`, `VenueReviewProjectionHandler`, `ArtistReviewProjectionHandler`
  *Done: the 3 handlers set `contextAccessor.Context = context`, then `bus.PublishAsync(...)`, then `SaveChangesAsync` — the outbox row commits atomically with the projection + inbox row in one transaction. Serialization stays abstracted inside `OutboxBus`. Also renamed `IOutboxContextAccessor`→`IDbContextAccessor`, `Current`→`Context`.*

- [ ] **OB2 — CRITICAL** — `Customer.Ticket TicketService.CompleteAsync:96-106`
  Two `SaveChangesAsync` calls across two DbContexts (`ConcertDbContext`, `TicketDbContext`), no shared transaction. Crash between them = availability decremented with no ticket.
  **Fix:** Single DbContext for the purchase write, or saga/compensation. "Insert ticket + decrement + outbox row in one tx" must hold.

- [ ] **OB3 — HIGH** — `Payment WebhookProcessor:53,61+`
  Stripe idempotency record written before event publish, no wrapping transaction → publish failure drops the event silently.
  **Fix:** Idempotency-row insert + outbox-message insert in one `SaveChangesAsync`.

- [ ] **OB4 — HIGH** — `Payment.Web/Program.cs:43` + `WebhookProcessor.cs:17`
  Webhook path uses `AddDirectBusKeyed("webhook")` — non-outbox bus. `PaymentSucceeded/FailedEvent` bypass the outbox, not durable across a crash.
  **Fix:** Route webhook events through the outbox bus.

- [ ] **OB5 — HIGH** — `Customer ReviewCreatedDomainEventHandler`
  Publishes `ReviewSubmittedEvent` directly to `IBus`, bypassing the outbox; review save and publish can diverge.
  **Fix:** Make it a proper `IPreCommitDomainEventHandler` so the publish goes through the outbox in the review's transaction. (See also DI5.)

- [ ] **OB6 — HIGH** — `OutboxDispatcher.DrainOnceAsync:57-86`
  Per-row status mutated in a loop, then a single `SaveChangesAsync` at the end. Failed batch save leaves published rows as `Pending` / stale in-memory status.
  **Fix:** Save per-row (accept extra round-trips) so a failed save doesn't suppress status for successfully-published rows.

- [ ] **OB7 — MEDIUM** — `AzureServiceBusReceiver.cs:68-93`
  Deserialization failure (unknown type / malformed JSON) is **abandoned**, not dead-lettered → poison message loops forever if subscription max-delivery-count isn't set.
  **Fix:** Distinguish unretryable deserialization failures and dead-letter them.

## 3. Event handler correctness

- [ ] **EH1 — CRITICAL** — B2B `VerifyPaymentProcessor`, `EscrowPaymentProcessor`, `SettlementPaymentProcessor`, `BookingPaymentFailedProcessor`, `VerifyPaymentFailedProcessor`
  None check `envelope.MessageId` against the inbox or write a dedup row. ASB at-least-once → double verify / double settle on redelivery.
  **Fix:** Each handler delegates to a module facade (`concertWorkflowModule.VerifyAsync`/`SettleAsync`, `bookingService.FailPaymentAsync`, `concertNotifier`) that runs its own `SaveChanges` — the inbox guard can't trivially share that transaction. None of the 5 handlers inject a DbContext today. Decide approach (inbox check + insert inside the facade's unit of work, vs. a wrapping transaction) before editing.

- [ ] **EH2 — CRITICAL** — Customer `TicketPaymentProcessor` + `TicketPaymentFailedProcessor`
  No inbox guard — redelivered `PaymentSucceededEvent` creates duplicate tickets + double-decrements. `TicketDbContext` doesn't even map `InboxMessageEntity`.
  **Fix:** Map `InboxMessageEntity` (ExcludeFromMigrations) on `TicketDbContext`; add inbox dedup to both handlers.

- [ ] **EH3 — HIGH** — `DomainEventDispatchInterceptor.cs:11`
  `_pendingEvents` is an instance field on a scoped interceptor; a re-entrant `SaveChanges` overwrites it (outer call dispatches inner call's events). Latent now, landmine.
  **Fix:** Use a local / `AsyncLocal` stack threaded `SavingChangesAsync` → `SavedChangesAsync`.

- [ ] **EH4 — HIGH** — all inbox handlers
  `AnyAsync` pre-check → insert is a TOCTOU race under concurrent redelivery. Safe-effect, but no `catch (DbUpdateException)` → PK-violation noise + spurious message abandon.
  **Fix:** Treat the composite-PK unique constraint as the gate; catch `DbUpdateException` on the inbox PK and return normally.

- [ ] **EH5 — MEDIUM** — Payment `TicketTransactionHandler`, `VerifyTransactionHandler` (via `TransactionService.LogAsync`)
  `LogAsync` doesn't check for an existing transaction by `PaymentIntentId` → duplicate transaction rows on redelivery. (Escrow/Settlement handlers do guard.)
  **Fix:** `LogAsync` checks `GetByPaymentIntentIdAsync` first, no-ops if present.

## 4. DI registration completeness

- [x] **DI1 — CRITICAL — DONE** — `B2B.Web/Program.cs:109-119` + `AppHost/DistributedApplicationBuilderExtensions.cs:32-35`
  *Done: B2B.Web subscribes ReviewSubmitted/Artist/VenueChanged; AppHost adds `concertable-b2b` subs on those 3 topics. NOTE: B2B's 3 ReviewSubmitted handlers will throw until OB1 is fixed.*
  `AddAzureServiceBusTransport` subscribes only `PaymentSucceeded/FailedEvent`. Missing `ReviewSubmittedEvent`, `ArtistChangedEvent`, `VenueChangedEvent` → review/rating projections, artist/venue read-models, `ArtistManagerProfile.ArtistId`/`VenueManagerProfile.VenueId` sync never run. Registered handlers = dead code.
  **Fix:** Add the 3 `SubscribeTo<>()` calls + matching B2B ASB subscriptions in AppHost.

- [x] **DI2 — CRITICAL — DONE** — `Customer.Web/Program.cs:53-58`
  *Done: Customer.Web subscribes ConcertChanged + CustomerRegistered, drops the bogus ReviewSubmitted self-subscription; AppHost adds `concertable-customer` subs on those 2 topics.*
  Doesn't subscribe `ConcertChangedEvent` (→ `ConcertProjectionHandler` never runs) or `CustomerRegisteredEvent`. *Does* subscribe `ReviewSubmittedEvent` — its own outbound event (bug).
  **Fix:** Add `ConcertChangedEvent` + `CustomerRegisteredEvent` subscriptions; remove `ReviewSubmittedEvent`.

- [ ] **DI3 — HIGH** — `Search AutocompleteServiceFactory.cs:16` + `Search ServiceCollectionExtensions.cs`
  `GetRequiredKeyedService<IAutocompleteService>(null)` when `headerType` unset, but `AllAutocompleteService` is registered **unkeyed**. Throws on every default autocomplete call.
  **Fix:** Register `AllAutocompleteService` as `AddKeyedScoped<IAutocompleteService>((HeaderType?)null)`.

- [ ] **DI4 — HIGH** — `Search.Web/Program.cs` + `Search.Workers/Program.cs`
  `TimeProvider` injected by `ConcertHeaderRepository`/`ConcertSearchSpecification` but never registered.
  **Fix:** `services.AddSingleton(TimeProvider.System)` in `AddSearchModule` (covers both hosts).

- [ ] **DI5 — HIGH** — `Customer Review ServiceCollectionExtensions.cs:38`
  `ReviewCreatedDomainEventHandler` registered as `IDomainEventHandler<T>` but class implements `IPreCommitDomainEventHandler<T>` → dispatched at wrong phase or not at all.
  **Fix:** Register under the interface the class actually implements (and resolve OB5 together).

- [ ] **DI6 — MEDIUM** — `B2B.Web/Program.cs:122`
  `AddInbox(...)` registers an `InboxDbContext` nothing uses at runtime (handlers write inbox rows via their own module contexts).
  **Fix:** Keep `InboxDbContext` for migrations only; drop the runtime registration if it serves nothing. (Confirm migration ownership first.)

- [ ] **DI7 — MEDIUM** — `AppHost/DistributedApplicationBuilderExtensions.cs:108`
  B2B.Workers gets `sql` but no `WithReference(asb)`.
  **Fix:** If B2B.Workers subscribes to any ASB topic, add the `asb` reference; otherwise confirm it genuinely needs none.

## 5. Conventions

- [ ] **CV1** — Primary constructors in newly-touched services/handlers/repos/DbContexts. Hit list: every Customer `*DbContext`, `SearchDbContext`, `PaymentDbContext`; `DomainEventDispatchInterceptor`, `AuditInterceptor`, `BaseRepository`, `UnitOfWork*`, `DomainEventDispatcher`; ~13 B2B handlers/modules/repos. → Explicit ctor + `private readonly` + `this.field = param`.
- [ ] **CV2** — Underscore fields: `ClientCredentialsTokenService` (5 fields), `DomainEventDispatchInterceptor._pendingEvents`, `ReviewEntity._events`. → Drop `_`.
- [ ] **CV3** — `ValueGeneratedNever()` missing on `ArtistReadModel`/`VenueReadModel`/`ConcertReadModel` Id configs and `TicketEntityConfiguration` (migration shows `ValueGeneratedOnAdd`). → Add it.
- [ ] **CV4** — Service-layer Response types: `IConcertService`/`IApplicationService` return `ConcertPostResponse`/`Checkout` from `Application.Responses`; Payment `PaymentResponse`/`EscrowResponse`. → Rename to `Result` / move to `Api/Responses/`.
- [ ] **CV5** — Duplicate `global using` lines in 4+ `GlobalUsings.cs` (Customer Concert/Profile/Review/Ticket Infrastructure, Payment Infrastructure). → De-dup.
- [ ] **CV6** — `public` schema classes that should be `internal`; `[Table("Reviews")]` annotation on `ReviewEntity` domain class. → Tidy.

## 6. Other correctness / PCI (outside the 5, but blocking)

- [ ] **OC1 — CRITICAL** — `AppHost/DistributedApplicationBuilderExtensions.cs:105`
  `Stripe:SecretKey` injected into `B2B.Web`'s environment — PCI scope leak.
  **Fix:** Remove `"Stripe:SecretKey"` from the `AddApi(...)` call.

- [x] **OC2 — CRITICAL — DONE** — `Auth/Config.cs:109-113`
  *Done: `Sha256` now returns `Convert.ToBase64String(hash)` instead of hex.*
  `ServiceClient` hashes the client secret as **hex**; Duende's `HashedSharedSecretValidator` expects **Base64** (`secret.Sha256()`). Every `client_credentials` token request → 401 → all gRPC Payment calls fail.
  **Fix:** `new Secret(clientSecret.Sha256())` using Duende's extension.

- [ ] **OC3 — CRITICAL** — `Payment.Application.csproj:15` + `IStripeApiClient.cs:6` + `Payment.Application/AssemblyInfo.cs:8-9,17`
  `Stripe.net` package + `public IStripeApiClient` exposing raw Stripe types in `Payment.Application`, with IVT to `B2B.Web`/`Concert.Infrastructure` — Stripe types reachable outside Payment.
  **Fix:** Remove `Stripe.net` from `Payment.Application`; make `IStripeApiClient` `internal` and move it + `IPaymentSessionConfigurator` to `Payment.Infrastructure`; introduce plain Application-layer option records; remove the temporary IVTs.

- [ ] **OC4 — HIGH** — `B2B.Web/appsettings.Production.json:3`
  Still has `ConnectionStrings:DefaultConnection`; all code reads `B2BDb` → `UseSqlServer(null)` in prod.
  **Fix:** Rename the key to `B2BDb` (and any Key Vault / App Service connection-string name).

- [ ] **OC5 — HIGH** — `ConcertChangedDomainEventHandler.cs:27-28`
  `e.TotalTickets` passed for both `TotalTickets` and `AvailableTickets` on `ConcertChangedEvent` → Search always shows concerts as fully available.
  **Fix:** Decide ownership of `AvailableTickets` for projection — likely a separate Customer-published event; at minimum stop silently passing `TotalTickets` twice.

- [ ] **OC6 — HIGH** — `Customer ConcertReviewService.CreateAsync:27-33`
  Hardcodes `artistId: 0, venueId: 0` on `ReviewEntity.Create` → review rows unjoinable by artist/venue.
  **Fix:** Source `ArtistId`/`VenueId` from Customer's local `ConcertEntity` (carried via `ConcertChangedEvent`).

- [ ] **OC7 — HIGH** — `Customer TicketService.PurchaseAsync:60-65`
  Metadata dict never sets `fromUserId`/`fromUserEmail`, but `TicketPaymentProcessor` does `meta["fromUserId"]` → `KeyNotFoundException` (and `PaymentFailedProcessor` too).
  **Fix:** Add `fromUserId`/`fromUserEmail` to the metadata in `PurchaseAsync`.

- [ ] **OC8 — HIGH [verify]** — `Customer.Web/Program.cs:79`
  JWT `Audience = "concertable.customer.api"` but Auth registers only `concertable.api` → all Customer tokens rejected.
  **Fix:** Register a `concertable.customer.api` API resource in Auth, or align the audience.

- [ ] **OC9 — HIGH** — `ConcertReadModel.cs:9` + `ConcertProjectionHandler.cs`
  `ConcertReadModel.BookingId` never set (`ConcertChangedEvent` carries no `BookingId`) → always 0.
  **Fix:** Remove the unused field, or expand `ConcertChangedEvent` with `BookingId` if downstream needs it.

- [ ] **OC10 — LOW** — 13 orphaned `.csproj` stubs in old `api/Modules/` after `.cs` deletion.
  **Fix:** Delete the orphaned project files + directories.

---

## Notes / open questions

- B2B→Customer in-process `ICustomerModule.GetUserIdsByLocationAndGenresAsync` call (preference module) is **known/accepted/deferred** per `MICROSERVICE_STEPS_CONT.md` Step 22 — not a finding.
- Conflicts between reviewers: the B2B reviewer rated the review-projection-handler ordering "correct"; the messaging reviewer caught the deeper bus-type/context mismatch (OB1). OB1's analysis supersedes — confirm during the fix.
