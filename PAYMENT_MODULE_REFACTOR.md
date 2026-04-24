# Payment Module Extraction — Implementation Plan

> **STATUS: PAUSED (2026-04-24)** — Step 0 (Discovery) complete, Appendix A filled.
> Payment extraction is blocked pending **Contract module extraction** (see
> `CONTRACT_MODULE_REFACTOR.md`, in progress). Reason: `IContractStrategy` + its
> factory/resolver are a cross-module concern, not a Concert-owned or Payment-owned one.
> If Payment extracts first, it either inherits Concert's anti-pattern (referencing
> Concert.Application for the marker) or duplicates the resolver. Contract gets extracted
> first; Payment resumes with a clean boundary.
>
> **Updates from Contract extraction in flight (2026-04-24, second pass — refines decisions
> below):**
> - **`IContractStrategy` marker now lives in `Concertable.Contract.Abstractions`** (public,
>   empty). `ITicketPaymentStrategy` + `IStripeValidationStrategy` (both Payment-owned) extend
>   this marker — compile-time restriction "this strategy family is keyed by `ContractType`."
>   Marker is NOT consumed by `IContractModule` itself — it's purely a typing constraint on
>   strategy interfaces. Earlier §A.7 option (c) ("delete the marker") REJECTED; option (b)
>   wins (marker promoted to a cross-module location).
> - **No generic `ResolveFor…Async<T>` on `IContractModule`.** Earlier draft of this plan
>   referenced `IContractModule.ResolveFor*Async<IStripeValidationStrategy>` and similar —
>   that "Shape B" design was rejected during Contract extraction. `IContractModule` exposes
>   exactly one method: `GetByOpportunityAsync(int opportunityId) → IContract?`. Strategy
>   resolution is the consumer's concern, not Contract's.
> - **Payment writes its own per-strategy-family factories.** Three 5-line internal classes,
>   each wrapping `IServiceProvider.GetRequiredKeyedService<TStrategy>(ContractType)`:
>   - `ITicketPaymentStrategyFactory` (Payment.Application internal) +
>     `TicketPaymentStrategyFactory` (Payment.Infrastructure internal)
>   - `IStripeValidationStrategyFactory` (Payment.Application internal) +
>     `StripeValidationStrategyFactory` (Payment.Infrastructure internal)
>   - These replace the legacy `ContractStrategyFactory<T>` / `ContractStrategyResolver<T>`
>     that are **deleted** during Contract Step 4 — do NOT move them to Payment.
> - **Concert writes the same shape for `IConcertWorkflowStrategy`** during Contract Step 9
>   (already locked there). Payment mirrors the pattern; nothing in Concert affects Payment.
> - **`IStripeValidationFactory`** (the legacy class in `Concertable.Infrastructure/Validators/`)
>   is the same redundancy noted in the original lock — deleted during Payment Step 7.
>   Consumers inject `IStripeValidationStrategyFactory` (the new per-Payment factory above)
>   and call `factory.Create(contract.ContractType).ValidateAsync()`.
> - **Dispatcher pattern for Payment**: `TicketPaymentDispatcher` rewrites to:
>   1. fetch opportunity id from the relevant Concert repo (the same `GetOpportunityIdAsync`
>      methods Contract Step 9 added to Concert),
>   2. call `IContractModule.GetByOpportunityAsync(opportunityId)` to get the `IContract`,
>   3. call `factory.Create(contract.ContractType).PayAsync(concertId, quantity, ...)`,
>   passing the `IContract` through to the strategy as a method parameter (so the strategy
>   doesn't refetch). Payment-owned strategies cast the `IContract` to their expected subtype
>   at the top of each method (safe by construction — keyed registration + `ContractType`
>   switch guarantee the match), per Contract §3.1.
> - **`TicketService` split — purchase orchestration folds into `CustomerPaymentService`, not
>   a new `TicketPurchaseService` class.** After stripping `ITicketValidator` + direct
>   `IConcertRepository.GetByIdAsync` (both replaced by `IConcertTicketPaymentModule.GetTicketEligibilityAsync`),
>   the purchase orchestrator is ~12 lines: auth check → eligibility facade → payment dispatcher
>   → response build. Not enough to justify its own class. The placeholder name "TicketPurchaseService"
>   in earlier plan revisions is dropped — at Step 7, add a `PurchaseTicketsAsync` method to
>   the existing `CustomerPaymentService` and delete legacy `TicketService` (its `CompleteAsync`
>   half goes to Concert via `IConcertTicketPaymentModule.IssueTicketsAsync`; `GetUserUpcoming`
>   / `GetUserHistoryAsync` placement TBD at Step 7 — likely Concert since they read
>   `TicketEntity` which Concert owns, but borderline).
> - **Stripe webhook endpoint location**: Contract plan banner notes the *endpoint* (signature
>   validation, event extraction) may live in Payment when Payment extracts — that's a Payment
>   plan question. **Decision: yes, Payment owns the webhook endpoint** (`WebhookController`
>   was already in Step 9's move list). The `SettlementWebhookHandler` body (line A.6) calls
>   `IConcertLifecycleModule.SettleAsync(bookingId, ct)` — Concert owns the booking state
>   transition, Payment owns the webhook entry point.
>
> **Decisions locked in this session (2026-04-24, original):**
> - Workers registers `IManagerPaymentService` with `"offSession"` keyed only (not both, not non-keyed).
> - Workers registers `IPaymentService` with `"offSession"` keyed only (same reason).
> - `IStripeValidationFactory` is redundant — will be deleted during Payment Step 7; consumers use the new internal `IStripeValidationStrategyFactory` (per-module factory pattern, see banner update above — REPLACES earlier "use `IContractModule.ResolveFor*Async<...>`" plan).
> - Webhook handlers (`TicketWebhookHandler`, `SettlementWebhookHandler`, `WebhookService`, `WebhookProcessor`, `WebhookQueue`, `WebhookStrategyFactory`) move to `Payment.Infrastructure` in Step 7.
> - `BackgroundTaskQueue` / `BackgroundTaskRunner` / `QueueHostedService` stay Web-only (no Workers registration) — documented, not a defect.
>
> **REOPENED 2026-04-24: `TicketService` placement.** Previously locked to move to
> `Payment.Infrastructure`. Reversed — `TicketService` **stays in Concert.Infrastructure**.
> Reasoning: creating `TicketEntity` records + calling `concertEntity.SellTickets()` is
> concert aggregate lifecycle, not payment work. Payment owns money movement (Stripe
> `PaymentIntent`, `TicketTransactionEntity`, webhook dispatch). The two are split:
> - **Concert.Infrastructure** keeps `TicketService`. `CompleteAsync` body (create
>   `TicketEntity` rows, `concert.SellTickets(quantity)`, save via `ConcertDbContext`)
>   runs inside Concert. Exposed cross-module via `IConcertTicketPaymentModule.IssueTicketsAsync`.
> - **Payment.Infrastructure** gets whatever orchestrator wraps the pre-payment flow
>   (today part of `TicketService.PurchaseAsync` — the `ITicketPaymentStrategy` selection +
>   Stripe intent creation + transaction logging). Likely name: `TicketPurchaseService`
>   or merge into existing `CustomerPaymentService` — decide at Step 7 when the code is
>   on screen.
> - **`TicketWebhookHandler`** (Payment) calls `IConcertTicketPaymentModule.IssueTicketsAsync`
>   to run Concert's `TicketService.CompleteAsync` body post-settle. Facade method already
>   planned.
>
> Ancillary benefit: avoids the Concert Step 16 silent-save-to-wrong-context trap
> (`TicketService` writing via `ConcertDbContext` in the Payment module would be the same
> bug pattern `feedback_module_service_saves_own_context.md` warns against).
>
> Successor to `CONCERT_MODULE_REFACTOR.md` (Stage 1 complete 2026-04-23). Payment is the
> first module where **eventual consistency matters enough to justify outbox infrastructure** —
> per MM_NORTH_STAR §Event Infrastructure, integration events + outbox should be introduced
> before or during this extraction.
>
> Memory: `project_modular_monolith.md` + `project_future_modules.md` tracks stage progress.

---

## Starting state (discovery, 2026-04-24)

`api/Modules/Payment/` already exists with one project: `Concertable.Payment.Contracts`,
containing only `PaymentResponse.cs` (base Stripe response record). Everything else lives
in legacy layers.

### What moves to Payment

**Entities (currently `Concertable.Core/Entities/`):**
- `TransactionEntity` (abstract TPT base) + `TicketTransactionEntity` + `SettlementTransactionEntity`
  (note: `SettlementTransactionEntity` lives in a file named `ConcertTransactionEntity.cs` — filename mismatch, rename on move)
- `StripeEventEntity` (Stripe webhook idempotency log)
- Enums: `TransactionStatus`, `TransactionType`, `PayoutAccountStatus`, `WebhookType`
- `TicketPurchaseParams` (from `Concertable.Core/Parameters/`)

**Application interfaces (currently `Concertable.Application/Interfaces/`):**
- Payment: `IPaymentService`, `ICustomerPaymentService`, `IManagerPaymentService`, `ITransactionService`
- Stripe: `IStripeAccountService`, `IStripeValidator`, `IStripeValidationFactory`, `IStripeValidationStrategy`
- Transaction: `ITransactionRepository`, `ITransactionMapper`, `ITransactionMapperFactory`
- Ticket: `ITicketService`, `ITicketRepository`, `ITicketValidator`
- Webhook (from `Concertable.Infrastructure/Interfaces/`): `IWebhookService`, `IWebhookProcessor`,
  `IWebhookQueue`, `IWebhookStrategy`, `IWebhookStrategyFactory`, `ITicketWebhookStrategy`,
  `ISettlementWebhookStrategy`, `IStripePaymentClient`

**DTOs / Requests / Responses:**
- `PaymentDtos.cs` (`TicketTransactionDto`, `SettlementTransactionDto`, `PaymentMethodDto`, `PurchaseCompleteDto`)
- `PaymentRequests.cs`, `PaymentResponses.cs`
- `TicketTransactionMapper`, `ConcertTransactionMapper` (payment-scoped mappers)
- `TicketMappers`, `TicketValidators`
- `TicketDto`, `TicketConcertDto` (currently in `SharedDtos.cs`)

**Infrastructure services (currently `Concertable.Infrastructure/Services/Payment/`):**
- `PaymentService`, `StripePaymentClient`, `StripeAccountService`, `TransactionService`
- `TicketPaymentDispatcher`, `CustomerPaymentService`, `ManagerPaymentService`
- `ArtistTicketPaymentService`, `VenueTicketPaymentService`
- `OnSessionPaymentService`, `OffSessionPaymentService`
- `FakePaymentService`, `FakeStripeAccountService`, `FakeWebhookService`
- `TicketService`

**Infrastructure repos:**
- `StripeEventRepository`, `TransactionRepository`
- `TicketRepository` (currently legacy — deferred from Concert extraction)

**Infrastructure validators / factories:**
- `StripeValidator`, `StripeValidationFactory`, `StripeAccountValidator`, `StripeCustomerValidator`
- `WebhookStrategyFactory`, `ContractStrategyFactory`, `ContractStrategyResolver`

**Mappers:**
- `PaymentIntentMappers`

**Infrastructure background / queue:**
- `BackgroundTaskQueue`, `BackgroundTaskRunner`, `QueueHostedService`, `IBackgroundTaskQueue`,
  `IBackgroundTaskRunner` — currently used only by `WebhookQueue`. Move to Payment; if a second
  consumer appears, revisit whether this belongs in `Shared.Infrastructure`.

**EF configurations (move out of `Data.Infrastructure/Data/Configurations/`):**
- `TransactionConfigurations.cs` (owns `Transactions`, `TicketTransactions`, `SettlementTransactions`)

**Controllers (to `Payment.Api`):**
- `PaymentController`, `StripeAccountController`, `TransactionController`, `WebhookController`, `TicketController`

### What stays / goes elsewhere

- `TicketEntity` — stays in `Concert.Domain`. It is a Concert aggregate member (a seat at a
  show); the financial transaction wrapping it (`TicketTransactionEntity`) is what Payment owns.
  `Payment.Infrastructure` interacts with tickets via `IConcertModule` / `IConcertTicketPaymentModule`.
- `PreferenceEntity`, `GenrePreferenceEntity` — **not Payment**. Goes to Customer module later.
- `MessageEntity` — not Payment. Goes to Messaging module.
- `AuthSettings`, `BlobStorageSettings`, `StripeSettings`, `UrlSettings` — Settings stay in
  `Concertable.Infrastructure` for now; `StripeSettings` moves with the module.
- `ConcertValidator` — lives in `Concertable.Infrastructure/Validators/`. Validates concert
  eligibility before payment. Moves to Payment (it is a payment pre-condition check).
- `TicketValidator`, `OpportunityApplicationValidator` — move to Payment and Concert
  respectively.

---

## Key design decisions

### 1. TicketEntity AND TicketService both stay in Concert

**REVISED 2026-04-24** — earlier draft of this plan moved `TicketService` to Payment with
the reasoning "TicketService handles ticket purchasing and fulfillment — a payment
concern." Reversed. Creating `TicketEntity` records and calling `concertEntity.SellTickets()`
is concert aggregate lifecycle — Concert's job unambiguously. Payment's job is money
movement (Stripe, transactions).

**Split:**

- **Concert.Infrastructure** owns `TicketService`. `CompleteAsync` body (build
  `TicketEntity` rows, `concert.SellTickets(quantity)`, save via `ConcertDbContext`)
  runs here. `TicketRepository` (Concert.Infrastructure, writes `TicketEntity` to
  `ConcertDbContext`) stays.
- **Concert.Contracts** gains `IConcertTicketPaymentModule`:
  - `IssueTicketsAsync(concertId, userId, quantity)` → wraps `TicketService.CompleteAsync`
    body. Called from Payment-side webhook handler after Stripe settles.
  - `GetTicketEligibilityAsync(concertId, userId)` → wraps `ITicketValidator.CanPurchaseTicketAsync`
    + concert price lookup. Called from Payment orchestrator before `PaymentIntent` creation.
- **Payment.Infrastructure** owns:
  - Orchestrator wrapping today's `TicketService.PurchaseAsync` body (minus the ticket-issue
    piece) — calls `IConcertTicketPaymentModule.GetTicketEligibilityAsync`, resolves
    `ITicketPaymentStrategy` via the Payment-internal `ITicketPaymentStrategyFactory` (after
    fetching `IContract` via `IContractModule.GetByOpportunityAsync(opportunityId)` — Concert
    repo gives the `opportunityId` from `concertId`), creates Stripe `PaymentIntent`, logs
    `TicketTransactionEntity`. Name TBD at Step 7. (Earlier draft referenced
    `IContractModule.ResolveForConcertAsync<...>` — that API does not exist; replaced by the
    per-module factory + `GetByOpportunityAsync` pair, see banner update.)
  - `TransactionRepository` writing `TicketTransactionEntity` / `SettlementTransactionEntity`
    to `PaymentDbContext`.
  - `TicketWebhookHandler` — on Stripe `payment_intent.succeeded`, logs transaction, then
    calls `IConcertTicketPaymentModule.IssueTicketsAsync` to run Concert's issue logic.

Net result: neither module crosses the other's aggregate boundary. Concert owns Tickets +
Concerts; Payment owns Transactions + Stripe state.

### 2. PayoutAccount projection (replaces deprecated cross-module lookup)

Today `ArtistManagerRepository.GetByConcertIdAsync` (now deleted) + direct `IReadDbContext`
reads retrieved `StripeAccountId` from Identity. North-star fix: Payment owns a
`PayoutAccountProjection` table (`UserId`, `StripeAccountId`, `StripeCustomerId`, `Status`)
populated from Identity integration events (`ManagerStripeAccountLinked`,
`CustomerStripeAccountLinked`, `ManagerStripeAccountVerified`). Payment workflows read
locally — no Identity call at payment time.

Identity must emit these events (see Identity TODO below).

### 3. IConcertModule facades crystallise here

Per `project_concert_multi_facade.md`, Concert will ship multiple facades. During Payment
extraction the following methods become concrete on Concert.Contracts:

**`IConcertLifecycleModule`**:
- `SettleAsync(int bookingId)` — Payment webhook handler calls after Stripe settles; backed
  internally by `SettlementDispatcher`
- `FinishAsync(int concertId)` — Workers' `ConcertFinishedFunction` calls; backed by
  `FinishedDispatcher`

**`IConcertTicketPaymentModule`:**
- `GetTicketEligibilityAsync(int concertId, Guid userId)` — wraps `ITicketValidator.CanPurchaseTicketAsync`
  + `concertRepository.GetByIdAsync` (price). Currently these are called together in
  `TicketService.PurchaseAsync` lines 47–53. Returns `TicketEligibility` (valid + price +
  available quantity) in `Concert.Contracts`.
- `IssueTicketsAsync(int concertId, Guid userId, int quantity)` → `IReadOnlyList<Guid> ticketIds`
  — wraps the body of `TicketService.CompleteAsync` (lines 78–98): creates `TicketEntity`
  records, calls `concertEntity.SellTickets(quantity)`, saves, returns ticket IDs. Currently
  called by `TicketWebhookHandler.HandleAsync`.
- `RefundTicketAsync(Guid ticketId)` — **backlog, not yet implemented**. Interface stub added
  now so the ownership is clear.

These replace the current direct Concert service injections inside `TicketService` and
`SettlementWebhookHandler`.

### 4. Workers DI divergence — reconcile here

Workers currently registers `IManagerPaymentService` **non-keyed** (line 63) while Web uses a
keyed onSession/offSession pair. This host discrepancy must be reconciled during Payment
extraction — not left as a silent difference. Likely outcome: Workers adopts the same keyed
registration; or if Workers only needs a default payment service, document why explicitly.

### 5. Transaction TPT — PaymentDbContext owns all three tables

`TransactionEntity` uses TPT (`UseTptMappingStrategy`). `PaymentDbContext` owns `Transactions`,
`TicketTransactions`, and `SettlementTransactions`. The `TransactionConfigurations.cs` file
moves out of `Data.Infrastructure` and into `Payment.Infrastructure/Data/Configurations/`.

---

## Identity TODO (prerequisite or parallel)

Payment needs `PayoutAccountProjection` populated from events that Identity doesn't currently
raise. Before or during Payment Step 6 (integration events), add to `IdentityModule`:

- `ManagerStripeAccountLinked(Guid userId, string stripeAccountId)` domain event +
  integration event
- `ManagerStripeAccountVerified(Guid userId)` integration event
- `CustomerStripeAccountLinked(Guid userId, string stripeCustomerId)` integration event

These can be added to Identity in a thin, focused pass before Payment Step 6.

---

## Stage 1 — Implementation steps

### Step 0 — Discovery (before any code changes)
- Audit all inbound consumers of payment services in Web + Workers (grep `IPaymentService`,
  `IManagerPaymentService`, `ICustomerPaymentService`, `ITicketService`, `IWebhookService`,
  `IStripeAccountService`, `ITransactionService`).
- Capture full DI snapshot: every payment/ticket/webhook/stripe registration in
  `Web/Extensions/ServiceCollectionExtensions.cs` + `Workers/ServiceCollectionExtensions.cs`.
- Record keyed registrations explicitly (ContractType, onSession/offSession, real/fake toggle).
- Identify the Workers non-keyed `IManagerPaymentService` discrepancy and agree resolution.
- Output: Appendix A in this document.

### Step 1 — Scaffold Payment projects
Scaffold under `api/Modules/Payment/` (Contracts already exists):
- `Concertable.Payment.Domain` — entities, enums, domain events
- `Concertable.Payment.Application` — interfaces, DTOs, requests, validators, mappers
- `Concertable.Payment.Infrastructure` — EF repos, services, `AddPaymentModule()`
- `Concertable.Payment.Api` — controllers, `AddPaymentApi()`

Add all four to `Concertable.sln` under `Modules/Payment` folder (see sln duplicate-folder
gotcha in `feedback_sln_solution_folder_duplicate.md`).

### Step 2 — Move entities + enums to Payment.Domain
Move from `Concertable.Core/Entities/` + `/Enums/`:
- `TransactionEntity`, `TicketTransactionEntity`, `SettlementTransactionEntity`
- `StripeEventEntity`
- `TransactionStatus`, `TransactionType`, `PayoutAccountStatus`, `WebhookType`
- `TicketPurchaseParams`

Add `Concertable.Core → Payment.Domain` project ref. Propagate
`global using Concertable.Payment.Domain` to Core, Application, Infrastructure,
Data.Infrastructure, Web, Workers + their test projects.

**New:** Add `PayoutAccountProjection` entity to `Payment.Domain` now
(`UserId`, `StripeAccountId`, `StripeCustomerId`, `Status : PayoutAccountStatus`).

### Step 3 — Move Application layer to Payment.Application
Move (all `internal`, namespace `Concertable.Payment.Application.*`):
- All payment/stripe/transaction/ticket/webhook interfaces
- `PaymentDtos.cs`, `PaymentRequests.cs`, `PaymentResponses.cs`
- `TicketDto`, `TicketConcertDto` (out of legacy `SharedDtos.cs`)
- `TicketMappers`, `TicketValidators`, `PaymentIntentMappers`, `TicketTransactionMapper`,
  `ConcertTransactionMapper`, `TransactionMapper`
- `ITransaction` interface + polymorphic DTO types

`Payment.Application/AssemblyInfo.cs`: grant `InternalsVisibleTo` to Payment.Infrastructure,
Payment.Api, Concertable.Infrastructure (TEMPORARY until Step 7), Concertable.Workers
(TEMPORARY until Step 13), Concertable.Web (TEMPORARY until Step 9),
Concertable.Web.IntegrationTests.

Flip all legacy payment impl classes `public` → `internal` in `Concertable.Infrastructure`
(same accessibility cascade pattern as Concert Step 5 — see
`feedback_module_impl_visibility_cascade.md`).

### Step 4 — Crystallise IConcertModule facades in Concert.Contracts
Add to `Concertable.Concert.Contracts`:

```csharp
public interface IConcertLifecycleModule
{
    Task SettleAsync(int bookingId, CancellationToken ct = default);
    Task FinishAsync(int concertId, CancellationToken ct = default);
}

public interface IConcertTicketPaymentModule
{
    // Wraps TicketValidator.CanPurchaseTicketAsync + concertRepository.GetByIdAsync (price)
    Task<TicketEligibility> GetTicketEligibilityAsync(int concertId, Guid userId, CancellationToken ct = default);
    // Wraps TicketService.CompleteAsync body: creates TicketEntity records, SellTickets, Save
    Task<IReadOnlyList<Guid>> IssueTicketsAsync(int concertId, Guid userId, int quantity, CancellationToken ct = default);
    // Backlog — not yet implemented; stub added to lock in ownership
    Task RefundTicketAsync(Guid ticketId, CancellationToken ct = default);
}
```

Implement in `Concert.Infrastructure`. Register in `AddConcertModule()`. The DTOs
(`TicketEligibility`) live in `Concert.Contracts`.

This step may surface compilation errors in `TicketService` + workflow services — good,
those are the callers that need rewiring in Step 7.

### Step 5 — Create PaymentDbContext
`internal class PaymentDbContext : DbContextBase` in `Payment.Infrastructure/Data/`.

DbSets:
- `Transactions`, `TicketTransactions`, `SettlementTransactions` (TPT)
- `StripeEvents`
- `PayoutAccountProjections`

Configs (all `internal`, in `Payment.Infrastructure/Data/Configurations/`):
- `TransactionEntityConfiguration` (moved from `Data.Infrastructure/Data/Configurations/TransactionConfigurations.cs`)
- `StripeEventEntityConfiguration`
- `PayoutAccountProjectionConfiguration`

`Payment.Infrastructure.csproj` gets EF Core SqlServer + Data.Infrastructure project ref.

### Step 6 — Integration events: PayoutAccountProjection
**Prerequisite:** Identity emits `ManagerStripeAccountLinked`, `ManagerStripeAccountVerified`,
`CustomerStripeAccountLinked` (see Identity TODO above).

Add to `Payment.Infrastructure/Handlers/`:
```csharp
internal class PayoutAccountProjectionHandler :
    IIntegrationEventHandler<ManagerStripeAccountLinkedEvent>,
    IIntegrationEventHandler<ManagerStripeAccountVerifiedEvent>,
    IIntegrationEventHandler<CustomerStripeAccountLinkedEvent>
```

Upsert-by-userId, idempotent. Register in `AddPaymentModule()`.

This step retires the remaining cross-module Stripe account lookups in the workflow services.

### Step 7 — Move Infrastructure layer to Payment.Infrastructure
Move all services, repositories, factories, validators from `Concertable.Infrastructure/Services/Payment/`
and `Concertable.Infrastructure/Repositories/` (payment-scoped) and `Concertable.Infrastructure/Validators/`
(stripe/ticket validators) and `Concertable.Infrastructure/Factories/`:

- All payment services + fakes
- `StripeEventRepository`, `TransactionRepository`
- Payment-owned `TicketRepository` (manages `TicketTransactionEntity` in `PaymentDbContext` —
  distinct from `Concert.Infrastructure`'s `TicketRepository` which manages `TicketEntity`)
- `StripeValidator`, `StripeValidationFactory`, `StripeAccountValidator`, `StripeCustomerValidator`
- `ConcertValidator`, `TicketValidator` (payment pre-condition validators)
- `WebhookStrategyFactory` (Payment-owned)
- ~~`ContractStrategyFactory`, `ContractStrategyResolver`~~ — these were **deleted in Contract
  extraction Step 4** and are NOT moved to Payment. Replaced by Payment-internal
  `ITicketPaymentStrategyFactory` + `IStripeValidationStrategyFactory` (each a 5-line
  `IServiceProvider.GetRequiredKeyedService<TStrategy>(ContractType)` wrapper) authored in
  this step. See banner update for full pattern.
- `BackgroundTaskQueue`, `BackgroundTaskRunner`, `QueueHostedService` (from legacy
  `Concertable.Infrastructure/Background/` + `Services/`)

**Webhook handlers currently in Concert.Infrastructure** — during Concert extraction,
`WebhookService`, `WebhookProcessor`, `WebhookQueue`, `TicketWebhookHandler`, and
`SettlementWebhookHandler` were placed in `Concert.Infrastructure/Services/Webhook/`.
They are Stripe payment concerns, not Concert concerns; move them to `Payment.Infrastructure`
in this step. They call Concert via `IConcertLifecycleModule` / `IConcertTicketPaymentModule`
facades for any concert/ticket side effects.

Rewrite callers of deleted cross-module lookups to use `PayoutAccountProjection` (now
populated from Step 6 events). `TicketService` rewired to use `IConcertTicketPaymentModule`
instead of direct Concert service injection.

Reconcile Workers DI discrepancy (keyed vs non-keyed `IManagerPaymentService`).

`Payment.Infrastructure.csproj` refs:
- Payment.Application, Payment.Contracts, Concert.Contracts, **Contract.Abstractions** (for
  `IContractModule`, `IContract`, `IContractStrategy` marker, `ContractType`, `PaymentMethod`),
  Identity.Contracts (for events)
- Stripe.net NuGet package

### Step 8 — Scaffold IPaymentModule facade
`Payment.Contracts/IPaymentModule.cs` — start minimal; methods crystallise once callers
are identified. Likely candidates:
- `GetTransactionsForUserAsync(Guid userId, PageParams)` — cross-module transaction history

`Payment.Infrastructure/PaymentModule.cs` — `internal sealed class PaymentModule : IPaymentModule`.
`AddPaymentModule()` gains `services.AddScoped<IPaymentModule, PaymentModule>()`.

### Step 9 — Move controllers to Payment.Api
Move from `Concertable.Web/Controllers/`:
- `PaymentController`, `StripeAccountController`, `TransactionController`,
  `WebhookController`, `TicketController`

All `internal`. `Payment.Api` gets `InternalControllerFeatureProvider` (same pattern as
Venue/Concert). `AddPaymentApi()` calls `AddPaymentModule()` + `AddApplicationPart(...)`.

`Concertable.Web` drops direct refs to payment controllers; gains `Payment.Api` project ref.

### Step 10 — PaymentDevSeeder + PaymentTestSeeder
`internal class PaymentDevSeeder : IDevSeeder` and `PaymentTestSeeder : ITestSeeder`, both
`Order=4` (after Concert at 3). Inject `PaymentDbContext`.

Seed: `PayoutAccountProjection` rows for seeded managers (so payment workflows have Stripe
account data without needing Identity events to have fired). Test seeder seeds predictable
transaction + stripe event fixtures for integration tests.

### Step 11 — Remove Payment DbSets from ApplicationDbContext
Remove `DbSet<TransactionEntity>`, `DbSet<TicketTransactionEntity>`,
`DbSet<SettlementTransactionEntity>`, `DbSet<StripeEventEntity>` from `ApplicationDbContext`.
Add `ExcludeFromMigrations` for all four. Rescaffold `ApplicationDbContext` `InitialCreate`.

### Step 12 — Migration scaffolding
Delete all `Migrations/` folders. Rescaffold `InitialCreate` in dependency order:
`Shared → Identity → Artist → Venue → Concert → Payment → AppDb`

Payment migration: check for cross-context FK issues. `TransactionEntity` has no FK into
Genres (no cross-context issue). `TicketTransactions` has `TicketId` FK into Concert's
`Tickets` table — both contexts run before AppDb, but Payment runs after Concert, so this
FK is safe (Tickets exists when Payment migrates).

### Step 13 — Wire AddPaymentApi() in Web + Workers
- `Program.cs`: `services.AddPaymentApi(builder.Configuration)`
- `Workers/ServiceCollectionExtensions.cs`: `services.AddPaymentModule(configuration)`
  (Workers doesn't need the Api layer)
- Remove TEMPORARY `InternalsVisibleTo` grants from Payment.Application/AssemblyInfo.cs as
  callers migrate to proper module boundaries.

### Step 14 — Full test suite
Run Core + Infra + Workers unit tests + full integration suite. Expected issues:
- Stripe webhook integration tests may need `FakeWebhookService` wired correctly
- `PayoutAccountProjection` seeding must match what payment workflow tests expect
- Workers keyed/non-keyed `IManagerPaymentService` discrepancy surfaces here if not
  caught in Step 7

---

## Appendix A — DI snapshot (discovery output, 2026-04-24)

### A.1 Web host — `api/Concertable.Web/Extensions/ServiceCollectionExtensions.cs`

**Stripe SDK singletons (real-only, skipped when fake-mode):**
- `Stripe.AccountService` (Singleton)
- `Stripe.AccountLinkService` (Singleton)
- `Stripe.CustomerService` (Singleton)
- `Stripe.PaymentMethodService` (Singleton)
- `Stripe.SetupIntentService` (Singleton)
- `IStripePaymentClient → StripePaymentClient` (Singleton, real-only)

**Real/Fake toggle (branch on `UseRealStripe` config):**
| Interface | Real | Fake | Keyed |
|-----------|------|------|-------|
| `IStripeAccountService` | `StripeAccountService` | `FakeStripeAccountService` | N |
| `IPaymentService` | `OnSessionPaymentService` | `FakePaymentService` | Y, `"onSession"` |
| `IPaymentService` | `OffSessionPaymentService` | `FakePaymentService` | Y, `"offSession"` |
| `IWebhookService` | `WebhookService` | `FakeWebhookService` | N |

**Stripe validation (keyed by `ContractType`):**
- `StripeAccountValidator`, `StripeCustomerValidator` (Scoped, concrete)
- `IStripeValidator → StripeValidator` (Scoped)
- `IStripeValidationFactory → StripeValidationFactory` (Scoped)
- `IStripeValidationStrategy` keyed:
  - `ContractType.VenueHire → StripeAccountValidator`
  - `ContractType.FlatFee → StripeCustomerValidator`
  - `ContractType.DoorSplit → StripeCustomerValidator`
  - `ContractType.Versus → StripeCustomerValidator`

**Manager payment (keyed):**
- `IManagerPaymentService → ManagerPaymentService` via factory helper `AddKeyedManagerPaymentService(key)`:
  - keyed `"onSession"`
  - keyed `"offSession"`

**Customer + ticket + transaction:**
- `ICustomerPaymentService → CustomerPaymentService` (Scoped)
- `ITicketService → TicketService` (Scoped)
- `ITransactionService → TransactionService` (Scoped)
- `ITicketValidator → TicketValidator` (Scoped)

**Ticket payment strategies (keyed by `ContractType`):**
- `ITicketPaymentStrategy`:
  - `ContractType.FlatFee → VenueTicketPaymentService`
  - `ContractType.DoorSplit → VenueTicketPaymentService`
  - `ContractType.Versus → VenueTicketPaymentService`
  - `ContractType.VenueHire → ArtistTicketPaymentService`

**Webhook plumbing:**
- Registered in Web: `IWebhookStrategyFactory → WebhookStrategyFactory` (Scoped)
- Registered in **Concert.Infrastructure** `AddConcertModule()`:
  - `IWebhookProcessor → WebhookProcessor`
  - `IWebhookQueue → WebhookQueue`
  - `IWebhookStrategy` keyed: `WebhookType.Concert → TicketWebhookHandler`, `WebhookType.Settlement → SettlementWebhookHandler`

**Background queue (Web-only):**
- `IBackgroundTaskQueue → BackgroundTaskQueue` (Singleton)
- `IBackgroundTaskRunner → BackgroundTaskRunner` (Singleton)
- `services.AddHostedService<QueueHostedService>()` ← Web-only

### A.2 Workers host — `api/Concertable.Workers/ServiceCollectionExtensions.cs`

- `IPaymentService → PaymentService` (Scoped, **non-keyed**) — differs from Web's keyed pair
- `IStripeAccountService → StripeAccountService` (Scoped) — real only, no fake toggle
- `IManagerPaymentService → ManagerPaymentService` (Scoped, **non-keyed**) — **discrepancy vs Web's keyed `"onSession"`/`"offSession"`**
- **No** `IBackgroundTaskQueue` / `IBackgroundTaskRunner` / `QueueHostedService` registration
- **No** fake-mode toggle (real Stripe only in Workers)

### A.3 Cross-host discrepancies (resolve in Step 7)

1. **Keyed `IManagerPaymentService`** — Web keys by session type; Workers registers plain. Workers currently only consumes `IManagerPaymentService` from `ConcertFinishedFunction`-path code which runs off-session. Resolution: Workers registers `offSession` key only (or both keys pointing to `OffSessionPaymentService`). Document why if Workers keeps non-keyed.
2. **Keyed `IPaymentService`** — Same split. Workers' non-keyed base `PaymentService` never goes through On/OffSession wrappers. Align with #1.
3. **`QueueHostedService` missing in Workers** — if Workers ever processes Stripe webhooks (it doesn't today; Web owns `WebhookController`), the queue wouldn't drain. Keep Web-only for now; note explicitly.
4. **Fake-mode toggle missing in Workers** — integration tests run against Web, not Workers, so this is fine. Document that Workers is real-Stripe-only.

### A.4 Inbound consumers (grep sweep, 60 files total)

**Consumers that survive extraction (stay on Payment interfaces via `InternalsVisibleTo` until own module extracts):**
- `PaymentController`, `StripeAccountController`, `TransactionController`, `WebhookController`, `TicketController` — move to `Payment.Api` in Step 9
- `UpfrontConcertService` (Concert), `DeferredConcertService` (Concert) inject keyed `IManagerPaymentService` — stay; they call into Payment via `IManagerPaymentService`
- `VenueTicketPaymentService`, `ArtistTicketPaymentService` inject `ICustomerPaymentService` — move with Payment
- `StripeAccountValidator` injects `IStripeAccountService` — moves with Payment

**Consumers that rewire during Step 7:**
- `TicketService` → uses `IConcertTicketPaymentModule` (replaces direct Concert injection)
- `SettlementWebhookHandler` → uses `PayoutAccountProjection` + `IConcertLifecycleModule.SettleAsync`
- `ArtistTicketPaymentService` / `VenueTicketPaymentService` → `IConcertLifecycleModule` for booking lookups

**Test-only consumers (keep `InternalsVisibleTo("Concertable.Web.IntegrationTests")`):**
- `MockWebhookService`, `ApiFixture` wiring

### A.5 Webhook handler file locations (confirmed)

- `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Webhook/TicketWebhookHandler.cs`
- `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Webhook/SettlementWebhookHandler.cs`
- `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Webhook/WebhookService.cs`
- `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Webhook/WebhookProcessor.cs`
- `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Webhook/WebhookQueue.cs`

All five move to `Payment.Infrastructure/Services/Webhook/` in Step 7. `WebhookStrategyFactory` (legacy `Concertable.Infrastructure/Factories/`) moves with them.

### A.6 `SettlementWebhookHandler` body (verbatim, for Step 7 rewiring reference)

```csharp
internal class SettlementWebhookHandler : ISettlementWebhookStrategy
{
    private readonly ITransactionService transactionService;
    private readonly ISettlementDispatcher settlementDispatcher;

    public async Task HandleAsync(PaymentIntent intent, CancellationToken cancellationToken)
    {
        await transactionService.CompleteAsync(intent.Id);
        await settlementDispatcher.SettleAsync(int.Parse(intent.Metadata["bookingId"]));
    }
}
```

Step 7 rewrites: `ISettlementDispatcher` → `IConcertLifecycleModule.SettleAsync(bookingId, ct)`. `ITransactionService` stays (co-located in Payment).

### A.7 `ITicketPaymentStrategy` + `IContractStrategy` (for Step 7 relocation)

**RESOLVED 2026-04-24 (during Contract extraction):** option (b) wins — marker promoted to a
cross-module location, but **`Concertable.Contract.Abstractions`** rather than
`Concert.Contracts`. Reasoning: Contract is its own bounded module post-extraction;
`IContractStrategy`'s semantic — "this strategy family is keyed by `ContractType`" — sits in
the same module as the `ContractType` enum it pairs with.

By Step 7 of this Payment plan, the world looks like:

```csharp
// Concertable.Contract.Abstractions/IContractStrategy.cs (PUBLIC marker)
public interface IContractStrategy { }

// Concertable.Payment.Application/Interfaces/ITicketPaymentStrategy.cs (internal)
internal interface ITicketPaymentStrategy : IContractStrategy
{
    Task<Result<PaymentResponse>> PayAsync(int concertId, int quantity, string? paymentMethodId, decimal price, IContract contract);
    //                                                                                                          ^^^^^^^^^^^^^^^^^^
    // Strategy receives the IContract from the dispatcher (per Contract §3.1) — casts to
    // expected subtype DTO at the top of the method body. No second contract lookup.
}

// Concertable.Payment.Application/Interfaces/IStripeValidationStrategy.cs (internal)
internal interface IStripeValidationStrategy : IContractStrategy
{
    Task<bool> ValidateAsync();
}
```

Plus the two Payment-internal factories (5 lines each, Infrastructure):

```csharp
internal interface ITicketPaymentStrategyFactory
{
    ITicketPaymentStrategy Create(ContractType type);
}
internal sealed class TicketPaymentStrategyFactory(IServiceProvider sp) : ITicketPaymentStrategyFactory
{
    public ITicketPaymentStrategy Create(ContractType type)
        => sp.GetRequiredKeyedService<ITicketPaymentStrategy>(type);
}
// IStripeValidationStrategyFactory mirrors the same shape.
```

These replace the legacy generic `IContractStrategyFactory<T>` / `IContractStrategyResolver<T>`
(deleted in Contract extraction Step 4 — do not move to Payment). The `ITicketPaymentStrategy`
method gains an `IContract contract` parameter passed by the dispatcher (per Contract §3.1
locked pattern) so the strategy doesn't need its own contract lookup.

`IStripeValidationStrategy` is currently `public` and does NOT inherit `IContractStrategy`
(see Contract Appendix A surprise #6). On the move to `Payment.Application`, flip to `internal`
and add `: IContractStrategy` for symmetry — it's keyed by `ContractType` just like
`ITicketPaymentStrategy`, the marker is the right type-system signal.

(Earlier "(c)" recommendation — delete the marker — REJECTED. The marker is genuinely useful
as a compile-time hint on factory signatures and prevents accidentally registering a non-
ContractType-keyed strategy. The fix to "marker is Concert-owned" is to move it, not delete it.)

### A.8 Extraction blockers / open decisions

- **Workers DI reconciliation (#A.3.1–2):** proposal = register keyed `offSession` in Workers (same helper), drop non-keyed. Confirm with user at Step 7.
- ~~**`IContractStrategy` marker fate (#A.7):** lean toward delete. Revisit at Step 7.~~ — RESOLVED 2026-04-24: marker moves to `Contract.Abstractions` (public). See updated §A.7.
- **`QueueHostedService` stays Web-only:** documented, not a blocker.

---

## Appendix B — Known cross-module couplings at extraction start

| Caller | Dependency | Resolution |
|--------|-----------|-----------|
| `TicketService` | Direct Concert service injection | → `IConcertTicketPaymentModule` (Step 4) |
| `TicketPaymentDispatcher` (today) / orchestrator (post-Step 7) | Legacy `IContractStrategyResolver<ITicketPaymentStrategy>` | → Payment-internal `ITicketPaymentStrategyFactory` + `IContractModule.GetByOpportunityAsync` (Step 7); legacy resolver is deleted by Contract Step 4 |
| `StripeValidator` (legacy `IStripeValidationFactory`) | Legacy keyed-DI lookup via redundant factory | → Payment-internal `IStripeValidationStrategyFactory` + `IContractModule.GetByOpportunityAsync` (Step 7) |
| `SettlementWebhookHandler` | `IManagerPaymentService` cross-context lookup | → `PayoutAccountProjection` (Step 6) |
| `SettlementWebhookHandler` | `ISettlementDispatcher` (Concert-internal) | → `IConcertLifecycleModule.SettleAsync` (Step 4) |
| `TicketWebhookHandler` | Direct `TicketService.CompleteAsync` call | → `IConcertTicketPaymentModule.IssueTicketsAsync` (Step 4) |
| `ArtistTicketPaymentService` | `IConcertBookingRepository` | → `IConcertLifecycleModule` (Step 4) |
| `VenueTicketPaymentService` | `IConcertBookingRepository` | → `IConcertLifecycleModule` (Step 4) |
| Payment workflows | Direct `IReadDbContext` Identity lookups | → `PayoutAccountProjection` (Step 6) |
| Payment workflows | Direct subtype reads on `*ContractEntity` | → `IContractModule.GetByOpportunityAsync(opportunityId) → IContract` (post-Contract-extraction; strategies cast to expected subtype DTO at the top of each method) |
