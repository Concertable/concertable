# Payment Module Extraction — Implementation Plan

> **STATUS: IN PROGRESS — Pre-Step-1 + Steps 1–6 DONE (2026-04-25). Resume at Step 7.**
> Build green throughout. See "Progress" section below for the close-out summary of each
> completed step. Tests not yet run (Step 13 deliverable).
>
> **Design lock:** Option B — Concert orchestrates ticket purchase, Payment is a stateless
> money-movement service. See `project_payment_module_design.md` for the canonical lock-list.
> Key consequences:
> - `POST /api/tickets/purchase` lives in **Concert.Api**, not Payment.Api.
> - `IPaymentModule` exposes exactly two cross-module verbs: `PurchaseTicketsAsync` +
>   `SettleBookingAsync`. Both take `IContract` as a method parameter (caller owns the read).
> - `ITicketPaymentModule` is callback-only: `IssueTicketsAsync` + `RefundTicketAsync`.
>   Explicitly rejected: `GetContractAsync`, `GetTicketEligibilityAsync` (would make Concert
>   a "data provider" for Payment).
> - Strategies (`ITicketPaymentStrategy`, `IStripeValidationStrategy`) live in
>   `Payment.Application` and are keyed by `ContractType`. Per-method `IContract` parameter
>   on strategy interfaces was REVERTED 2026-04-25 — half the impls ignored it. Strategies
>   that need terms either receive primitives via `IPaymentModule` boundary args or self-
>   fetch via the `IContract` arg passed in.
> - Stripe IDs MOVE OUT of `UserEntity` to a Payment-owned **system-of-record** entity
>   `PayoutAccountEntity` (NOT a projection). Payment is the only writer (Stripe Connect
>   onboarding) and the only reader. No Identity integration events needed.
> - Webhook direction is the exception to "Concert orchestrates": Stripe → Payment (Stripe-
>   shaped HMAC validation belongs to Payment) → callback to Concert via
>   `ITicketPaymentModule.IssueTicketsAsync` and `IConcertWorkflowModule.SettleAsync`.
> - Workers DI lock: `"offSession"` keyed only (Workers' only consumer is
>   `ConcertFinishedFunction`, off-session by definition).
> - Outbox infrastructure DEFERRED — Stage 1 uses `InProcessIntegrationEventBus` (immediate
>   dispatch). Payment events are idempotent (upsert by PK); a missed event self-heals.
>
> **Pre-Step-1 chore (THIS SESSION):** Relocate `TicketService` from legacy
> `Concertable.Infrastructure/Services/TicketService.cs` to `Concert.Infrastructure/Services/`.
> Fixes the silent-save-to-wrong-context bug AND positions Concert as the orchestrator under
> Option B. Discrete: file move + namespace + DI re-wire + integration test pass.
>
> Memory: `project_modular_monolith.md` + `project_future_modules.md` track stage progress;
> `project_payment_module_design.md` is the canonical decision lock.

---

## Progress (2026-04-25)

### ✅ Pre-Step-1 — TicketService relocated to Concert.Infrastructure
`api/Concertable.Infrastructure/Services/TicketService.cs` →
`api/Modules/Concert/Concertable.Concert.Infrastructure/Services/TicketService.cs`. Namespace
flipped, `internal`. DI moved from `Web/Extensions/ServiceCollectionExtensions.cs` →
`AddConcertModule()`. Fixes the silent-save-to-wrong-context bug and positions Concert as
the orchestrator.

### ✅ Step 1 — Payment projects scaffolded
Four new projects under `api/Modules/Payment/` (Contracts already existed):
`Concertable.Payment.Domain`, `.Application`, `.Infrastructure`, `.Api`. All added to
`Concertable.sln` under the `Modules/Payment` solution folder. Stub `AddPaymentModule()` and
`AddPaymentApi()` in place. `Payment.Application/AssemblyInfo.cs` carries permanent IVT
(Infrastructure, Api, IntegrationTests) plus TEMPORARY IVT for legacy hosts (retire at
Steps 7/8/12).

### ✅ Step 2 — Entities + enums moved to Payment.Domain
Moved from `Concertable.Core/Entities/` and `/Enums/`: `TransactionEntity` (TPT base),
`TicketTransactionEntity`, `SettlementTransactionEntity` (renamed from misnamed
`ConcertTransactionEntity.cs`), `StripeEventEntity`, `TransactionStatus`, `TransactionType`,
`PayoutAccountStatus`, `WebhookType`, `TicketPurchaseParams`. Dropped `UserEntity FromUser`/
`ToUser` navs from `TransactionEntity` (cross-module FK = primitive only per CLAUDE.md
rule 4); EF config now declares `HasIndex` on FK columns instead of `HasOne`.

**NEW:** `PayoutAccountEntity` (Payment-owned system of record) — `Id`, `UserId`,
`StripeAccountId?`, `StripeCustomerId?`, `Status : PayoutAccountStatus`. Factory `Create(userId)`
+ `LinkAccount`, `LinkCustomer`, `MarkVerified`. Replaces `UserEntity.StripeAccountId`/
`StripeCustomerId` (drop happens in Step 7).

`Concertable.Core → Payment.Domain` project ref + `global using Concertable.Payment.Domain`
across 13 consumer projects. Mass-rewrote Payment-entity FQN strings in legacy migration
snapshots.

### ✅ Step 3 — Application layer moved to Payment.Application
Created the full `Concertable.Payment.Application.*` namespace under `internal`:
- **Interfaces** (`Interfaces/`): `IPaymentService`, `ICustomerPaymentService`,
  `IManagerPaymentService`, `IStripeAccountService` (TEMPORARY ref to Identity.Domain for
  `UserEntity`/`ManagerEntity` params, retires Step 7), `IStripeValidator`,
  `IStripeValidationFactory`, `IStripeValidationStrategy` (now extends `IContractStrategy` ✓),
  `ITicketPaymentStrategy` (relocated from Concert.Application), `ITicketPaymentStrategyFactory`,
  `ITransactionService`, `ITransactionRepository : IIdRepository<TransactionEntity>`,
  `ITransaction`, `ITransactionMapper`, `IStripeEventRepository`.
- **Webhook subnamespace** (`Interfaces/Webhook/`): `IWebhookService`, `IWebhookProcessor`,
  `IWebhookQueue`, `IWebhookStrategy`, `IWebhookStrategyFactory`, `ITicketWebhookStrategy`,
  `ISettlementWebhookStrategy`, `IStripePaymentClient`.
- **DTOs**: `PaymentDtos.cs` (`PaymentMethodDto`, `PaymentDto`, `TicketTransactionDto`,
  `SettlementTransactionDto`, `PurchaseCompleteDto`).
- **Requests/Responses**: `TransactionRequest`, `TicketPaymentResponse`.
- **Mappers**: `TransactionMapper`, `TicketTransactionMapper`, `SettlementTransactionMapper`.
- **Validators**: `TransactionRequestValidator`.

`ITicketService` moved to `Concert.Application/Interfaces/` (Concert orchestrates per Option B).
Sed mass-rewrite: `Concertable.Application.Interfaces.Payment` → `Concertable.Payment.Application.Interfaces`,
`Concertable.Infrastructure.Interfaces` → `Concertable.Payment.Application.Interfaces.Webhook`.

Flipped 19 legacy impls public→internal in `Concertable.Infrastructure/Services/Payment/`,
`/Validators/`, `/Repositories/`, `/Factories/`. Deleted duplicate `ITicketPaymentStrategyFactory`
interface from legacy Factories — kept impl, now wires to Payment.Application's interface.

**Partial Step 8 pulled forward** (controllers couldn't compile in Web with internal-typed
deps): `PaymentController`, `TransactionController`, `WebhookController`, `StripeAccountController`
→ `Payment.Api/Controllers/` (internal); `TicketController` → `Concert.Api/Controllers/`
(internal, per Option B). `Payment.Api/Extensions/InternalControllerFeatureProvider.cs` +
`AddPaymentApi(IConfiguration)` set up. Web `Program.cs` calls `AddPaymentApi`;
`Concertable.Web.csproj` references `Payment.Api`.

Test mocks (`IMockStripePaymentClient`, `MockStripePaymentClient(Fail)`, `MockWebhookService`,
`MockStripeClient(Fail)`) flipped internal; `ApiFixture.StripePaymentClient` flipped to
internal property.

IVT entries on `Payment.Application/AssemblyInfo.cs`: Concert.Infrastructure (webhook
handlers carryover), Concert.Application (ITicketService), Concert.Api (TicketController),
Identity.Infrastructure (StripeAccountService injection), Web.E2ETests.

### ✅ Step 4 — Concert callback facades crystallised
Added to `Concert.Contracts`:
- `IConcertWorkflowModule` (renamed from initial `IConcertLifecycleModule` — matches
  `IConcertWorkflowStrategy` family): `SettleAsync`, `FinishAsync`. Impl
  `ConcertWorkflowModule` delegates to `ISettlementDispatcher`/`ICompletionDispatcher`.
- `ITicketPaymentModule` (renamed from `IConcertTicketPaymentModule` — `Concert` prefix
  dropped; Concert ownership is implicit by namespace): `IssueTicketsAsync`,
  `RefundTicketAsync`. Impl `TicketPaymentModule` delegates `IssueTicketsAsync` to
  `ITicketService.CompleteAsync` (looking up customer email via `IAuthModule`);
  `RefundTicketAsync` is `NotImplementedException` placeholder (backlog, ownership locked).

Both registered in `AddConcertModule()`. Webhook handler retargeting deferred to Step 7.

### ✅ Step 5 — PaymentDbContext
`Schema.Name = "payment"`. `PaymentDbContext` inherits `DbContextBase`, applies
`PaymentConfigurationProvider` (mirror of `ContractConfigurationProvider`). DbSets:
`Transactions`, `TicketTransactions`, `SettlementTransactions` (TPT), `StripeEvents`,
`PayoutAccounts`.

Internal configs in `Payment.Infrastructure/Data/Configurations/` (all in `payment` schema):
- `TransactionEntityConfiguration` (TPT base + Ticket + Settlement subtype configs)
- `StripeEventEntityConfiguration`
- `PayoutAccountEntityConfiguration` (unique `UserId`, lookup indexes on Stripe Ids)

`AddPaymentModule()` registers `PaymentDbContext` (UseSqlServer + AuditInterceptor +
DomainEventDispatchInterceptor) and `PaymentConfigurationProvider` (singleton + as
`IEntityTypeConfigurationProvider`).

Removed legacy: deleted `Data.Infrastructure/Data/Configurations/TransactionConfigurations.cs`
and stripped `StripeEventEntityConfiguration` from `MiscEntityConfigurations.cs`. Removed
4 lines from `AppDbConfigurationProvider`. Added 5 `ExcludeFromMigrations` entries in
`ApplicationDbContext.OnModelCreating` for the Payment-owned tables. `IReadDbContext` +
`ReadDbContext` gained `IQueryable<PayoutAccountEntity> PayoutAccounts`.

### ✅ Step 6 — IPaymentModule shape locked
`Concertable.Payment.Contracts/IPaymentModule.cs`:

```csharp
public interface IPaymentModule
{
    Task<Result<PaymentResponse>> PurchaseTicketsAsync(int concertId, int quantity,
        string? paymentMethodId, decimal price, Guid customerUserId, IContract contract,
        CancellationToken ct = default);

    Task<Result<PaymentResponse>> SettleBookingAsync(int bookingId, Guid payerUserId,
        Guid payeeUserId, decimal amount, IContract contract, CancellationToken ct = default);
}
```

Caller passes `IContract` as method parameter — Payment never reads from Concert/Contract.
Microservice-ready (this becomes the Refit/HTTP contract if Payment ever extracts to its
own process).

`Payment.Contracts.csproj` gained `Contract.Abstractions` ref + `FluentResults` package.
`Concert.Contracts.csproj` gained `→ Payment.Contracts` ProjectReference (one-way, no cycle).
Implementation lands in Step 7.

### 🔜 Step 7 — Move Infrastructure layer (NEXT)
Big step. Highlights:
- Move all payment services + fakes + repositories + validators + factories from
  `Concertable.Infrastructure/Services/Payment/`, `/Validators/`, `/Repositories/`,
  `/Factories/`, `/Background/` to `Payment.Infrastructure/`.
- Move webhook handlers (`WebhookService`, `WebhookProcessor`, `WebhookQueue`,
  `TicketWebhookHandler`, `SettlementWebhookHandler`) from
  `Concert.Infrastructure/Services/Webhook/` to `Payment.Infrastructure/Services/Webhook/`.
  Retarget to `IConcertWorkflowModule.SettleAsync` / `ITicketPaymentModule.IssueTicketsAsync`.
- Implement `PaymentModule : IPaymentModule` in Payment.Infrastructure (delegates to
  `ITicketPaymentStrategyFactory.Create(contract.ContractType).PayAsync(...)`).
- Add `PayoutAccountRepository`. Drop `UserEntity.StripeAccountId`/`StripeCustomerId`. Rewrite
  `IStripeAccountService.AddCustomerAsync`/`AddConnectAccountAsync` to write
  `PayoutAccountEntity` (kills the TEMPORARY Identity.Domain ref + Identity.Infrastructure IVT).
- Rewrite `TicketService.PurchaseAsync` (in Concert.Infrastructure post Pre-Step-1) to call
  `IPaymentModule.PurchaseTicketsAsync` instead of `ITicketPaymentDispatcher` + direct concert
  price lookup.
- Reconcile Workers DI: `"offSession"` keyed only.

---

## Starting state

`api/Modules/Payment/` exists with `Concertable.Payment.Contracts` (only `PaymentResponse.cs`
today). Everything else lives in legacy `Concertable.Core` / `Concertable.Application` /
`Concertable.Infrastructure`.

Contract extraction is closed. `IContractModule` (Contract.Abstractions) is the public read
surface; `IContractLookup` (Concert.Application internal, request-scoped memoizer) is the
Concert-side dedup point. Both already in place.

### What moves to Payment

**Entities (currently `Concertable.Core/Entities/`):**
- `TransactionEntity` (abstract TPT base) + `TicketTransactionEntity` + `SettlementTransactionEntity`
  (note: `SettlementTransactionEntity` is in a file misnamed `ConcertTransactionEntity.cs` —
  rename on move)
- `StripeEventEntity` (Stripe webhook idempotency log)
- Enums: `TransactionStatus`, `TransactionType`, `PayoutAccountStatus`, `WebhookType`
- `TicketPurchaseParams` (from `Concertable.Core/Parameters/`)

**NEW entity (Payment.Domain):**
- `PayoutAccountEntity` — Payment's system of record for Stripe Connect data. Schema:
  `Id`, `UserId` (FK-by-primitive into Identity, no nav), `StripeAccountId?`,
  `StripeCustomerId?`, `Status : PayoutAccountStatus`. Implements `IIdEntity` + `IEventRaiser`.
  Replaces `UserEntity.StripeAccountId` + `UserEntity.StripeCustomerId` (DROPPED from Identity).

**Application interfaces (currently `Concertable.Application/Interfaces/`):**
- Payment: `IPaymentService`, `ICustomerPaymentService`, `IManagerPaymentService`, `ITransactionService`
- Stripe: `IStripeAccountService`, `IStripeValidator`, `IStripeValidationFactory`,
  `IStripeValidationStrategy`
- Transaction: `ITransactionRepository`, `ITransactionMapper`, `ITransactionMapperFactory`
- Ticket payment: `ITicketPaymentStrategy` (currently misplaced in `Concert.Application/Interfaces/`
  — Contract-extraction-era artifact; relocate here)
- Webhook (from `Concertable.Infrastructure/Interfaces/` and
  `Concert.Infrastructure/Services/Webhook/`): `IWebhookService`, `IWebhookProcessor`,
  `IWebhookQueue`, `IWebhookStrategy`, `IWebhookStrategyFactory`, `ITicketWebhookStrategy`,
  `ISettlementWebhookStrategy`, `IStripePaymentClient`

**DTOs / Requests / Responses:**
- `PaymentDtos.cs` (`TicketTransactionDto`, `SettlementTransactionDto`, `PaymentMethodDto`,
  `PurchaseCompleteDto`)
- `PaymentRequests.cs`, `PaymentResponses.cs`
- `TicketTransactionMapper`, `ConcertTransactionMapper`, `PaymentIntentMappers`

**Infrastructure services (currently `Concertable.Infrastructure/Services/Payment/`):**
- `PaymentService`, `StripePaymentClient`, `StripeAccountService`, `TransactionService`
- `TicketPaymentDispatcher`, `CustomerPaymentService`, `ManagerPaymentService`
- `ArtistTicketPaymentService`, `VenueTicketPaymentService`
- `OnSessionPaymentService`, `OffSessionPaymentService`
- `FakePaymentService`, `FakeStripeAccountService`, `FakeWebhookService`

**Webhook handlers (currently `Concert.Infrastructure/Services/Webhook/` — Concert-extraction
carryover, they're Stripe concerns):**
- `WebhookService`, `WebhookProcessor`, `WebhookQueue`
- `TicketWebhookHandler`, `SettlementWebhookHandler`

**Infrastructure repos:**
- `StripeEventRepository`, `TransactionRepository`

**Infrastructure validators / factories:**
- `StripeValidator`, `StripeValidationFactory`, `StripeAccountValidator`, `StripeCustomerValidator`
- `WebhookStrategyFactory`
- `ITicketPaymentStrategyFactory` + impl (already exists in `Concertable.Infrastructure/Factories/`,
  internal, 5-line `GetRequiredKeyedService` wrapper — moves with Payment)

**Infrastructure background / queue:**
- `BackgroundTaskQueue`, `BackgroundTaskRunner`, `QueueHostedService`, `IBackgroundTaskQueue`,
  `IBackgroundTaskRunner` — currently used only by `WebhookQueue`. Move to Payment; if a
  second consumer appears, revisit `Shared.Infrastructure`.

**EF configurations (currently `Data.Infrastructure/Data/Configurations/`):**
- `TransactionConfigurations.cs` (owns `Transactions`, `TicketTransactions`,
  `SettlementTransactions`)
- `StripeEventEntityConfiguration` (if it exists; otherwise create)

**Controllers (to `Payment.Api`):**
- `PaymentController`, `StripeAccountController`, `TransactionController`, `WebhookController`

**Stays in Concert.Api (Option B):**
- `TicketController` — POST `/api/tickets/purchase` is concert-shaped. Owns the
  orchestration; calls `IPaymentModule.PurchaseTicketsAsync` for money movement.

### What stays / goes elsewhere

- `TicketEntity` — stays in `Concert.Domain`. Concert aggregate member.
- `TicketRepository` (manages `TicketEntity` against `ConcertDbContext`) — stays in
  Concert.Infrastructure.
- `TicketService` — relocates to `Concert.Infrastructure/Services/` in **Pre-Step-1** (this
  session). Under Option B, `TicketService.PurchaseAsync` stays in Concert as the
  orchestrator and calls `IPaymentModule.PurchaseTicketsAsync` instead of the legacy internal
  dispatcher (rewired in Step 7). `CompleteAsync` body becomes
  `ITicketPaymentModule.IssueTicketsAsync`. `GetUserUpcomingAsync` /
  `GetUserHistoryAsync` stay on `TicketService` (Concert-side reads).
- `TicketValidator` — stays in Concert (ticket-side eligibility).
- `ConcertValidator` — stays in Concert. Concert pre-condition; not Payment's job.
- `OpportunityApplicationValidator` — already in Concert.
- `PreferenceEntity`, `GenrePreferenceEntity` — Customer module (later).
- `MessageEntity` — Messaging module (later).
- `AuthSettings`, `BlobStorageSettings`, `UrlSettings` — stay in `Concertable.Infrastructure`.
- `StripeSettings` — moves with Payment.

### Stripe IDs out of Identity

`UserEntity.StripeAccountId` and `UserEntity.StripeCustomerId` get DROPPED. New home:
`PayoutAccountEntity` (Payment.Domain). One row per user that has a Stripe Connect account
(manager) or a Stripe Customer (customer paying). Payment writes during Connect onboarding
(`StripeAccountService`); Payment reads during purchase / settlement workflows.

No Identity integration events. No projection. System of record.

Backfill via `PaymentDevSeeder` / `PaymentTestSeeder` (Step 9) since migrations are
re-scaffolded fresh during the refactor.

---

## Pre-Step-1 — Relocate TicketService to Concert.Infrastructure (THIS SESSION)

`api/Concertable.Infrastructure/Services/TicketService.cs` →
`api/Modules/Concert/Concertable.Concert.Infrastructure/Services/TicketService.cs`.

- Namespace flip: `Concertable.Concert.Infrastructure.Services`. Class stays `internal`.
- File-scoped namespace + remove redundant usings.
- Imports retarget where Concert-extraction-era types now live in
  `Concertable.Concert.Application.Interfaces` (`IConcertRepository`, `ITicketPaymentDispatcher`).
  Payment-owned interfaces (`ITicketRepository` for now, `ITicketValidator`) stay on their
  current legacy namespaces — they get re-homed in Step 3/7 of THIS extraction.
- DI: remove `services.AddScoped<ITicketService, TicketService>()` from
  `Concertable.Web/Extensions/ServiceCollectionExtensions.cs`; add to
  `Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs` `AddConcertModule()`.
- IVT: `ITicketService` interface still lives in legacy `Concertable.Application/Interfaces/`
  for now, visible to Concert.Infrastructure via the existing
  `Concertable.Application → Concertable.Concert.Infrastructure` IVT entry. Verify; add if missing.
- Test: `dotnet build` then run integration ticket suite. The 3
  `Ticket.*Purchase_ShouldCreateTicketAndReduceAvailabilityAfterWebhook` cases tracked
  separately as flaky (`VenueReadModel.UserId` projection issue from Concert close-out) —
  this move should not regress anything else.

---

## Stage 1 — Implementation steps

### Step 0 — Discovery (DONE 2026-04-24)
Appendix A retained at the bottom for reference. Workers DI discrepancy resolution locked
in design memory: register `"offSession"` keyed only.

### Step 1 — Scaffold Payment projects
Scaffold under `api/Modules/Payment/` (Contracts already exists):
- `Concertable.Payment.Domain` — entities, enums, domain events
- `Concertable.Payment.Application` — interfaces, DTOs, requests, validators, mappers, strategies
- `Concertable.Payment.Infrastructure` — EF repos, services, `AddPaymentModule()`
- `Concertable.Payment.Api` — controllers (`PaymentController`, `StripeAccountController`,
  `TransactionController`, `WebhookController`), `AddPaymentApi()`

Add to `Concertable.sln` under `Modules/Payment` solution folder (sln duplicate-folder
gotcha: `feedback_sln_solution_folder_duplicate.md`).

### Step 2 — Move entities + enums to Payment.Domain
Move from `Concertable.Core/Entities/` + `/Enums/`:
- `TransactionEntity`, `TicketTransactionEntity`, `SettlementTransactionEntity`
- `StripeEventEntity`
- `TransactionStatus`, `TransactionType`, `PayoutAccountStatus`, `WebhookType`
- `TicketPurchaseParams`

Add `Concertable.Core → Payment.Domain` project ref. Propagate
`global using Concertable.Payment.Domain` to Core, Application, Infrastructure,
Data.Infrastructure, Web, Workers + their test projects.

**New entity (Payment-owned system of record):**
`PayoutAccountEntity` (`UserId`, `StripeAccountId?`, `StripeCustomerId?`,
`Status : PayoutAccountStatus`). Implements `IIdEntity` + `IEventRaiser`. Factory method
`Create(Guid userId)`; `LinkAccount(string stripeAccountId)`,
`LinkCustomer(string customerId)`, `MarkVerified()`. Domain events optional (Stage 1: omit;
revisit if downstream consumer appears).

**Drop from `UserEntity`:** `StripeAccountId`, `StripeCustomerId`. All current readers move
to `PayoutAccountEntity` via Payment-internal repo (Step 7).

### Step 3 — Move Application layer to Payment.Application
Move (all `internal`, namespace `Concertable.Payment.Application.*`):
- All payment / stripe / transaction / webhook interfaces listed above
- `ITicketPaymentStrategy` (relocated from `Concert.Application/Interfaces/`)
- `IStripeValidationStrategy` (currently `public` and does NOT extend `IContractStrategy`
  marker — flip to `internal` and add `: IContractStrategy` for symmetry with
  `ITicketPaymentStrategy`; both keyed by `ContractType`)
- `PaymentDtos.cs`, `PaymentRequests.cs`, `PaymentResponses.cs`
- `PaymentIntentMappers`, `TicketTransactionMapper`, `ConcertTransactionMapper`,
  `TransactionMapper`
- `ITransaction` interface + polymorphic DTO types

Strategy method signatures: do **NOT** add `IContract contract` per-method param. Strategies
that need contract terms either receive them through `IPaymentModule` boundary parameters
(caller passes), or cast the `IContract` argument forwarded from `IPaymentModule.PurchaseTicketsAsync`.

`Payment.Application/AssemblyInfo.cs`: grant `InternalsVisibleTo` to:
- Payment.Infrastructure (permanent)
- Payment.Api (permanent)
- Concertable.Infrastructure (TEMPORARY until Step 7)
- Concertable.Workers (TEMPORARY until Step 12)
- Concertable.Web (TEMPORARY until Step 8)
- Concertable.Web.IntegrationTests (test-only, permanent)

Flip all legacy payment impl classes `public` → `internal` in `Concertable.Infrastructure`
(accessibility cascade: `feedback_module_impl_visibility_cascade.md`).

### Step 4 — Crystallise Concert facades for Payment callbacks
Add to `Concertable.Concert.Contracts`:

```csharp
public interface IConcertWorkflowModule
{
    Task SettleAsync(int bookingId, CancellationToken ct = default);
    Task FinishAsync(int concertId, CancellationToken ct = default);
}

public interface ITicketPaymentModule
{
    Task<IReadOnlyList<Guid>> IssueTicketsAsync(int concertId, Guid userId, int quantity, CancellationToken ct = default);
    Task RefundTicketAsync(Guid ticketId, CancellationToken ct = default);
}
```

Implement in `Concert.Infrastructure`. Register in `AddConcertModule()`. Both **callback-only**
— Payment webhook handlers call them after Stripe settles. No `GetTicketEligibilityAsync` /
`GetContractAsync` (rejected — would make Concert a data provider for Payment).

`IssueTicketsAsync` body = current `TicketService.CompleteAsync` body (post Pre-Step-1 move).
`RefundTicketAsync` is a stub for now (backlog; lock the ownership).

### Step 5 — Create PaymentDbContext
`internal class PaymentDbContext : DbContextBase` in `Payment.Infrastructure/Data/`.

DbSets:
- `Transactions`, `TicketTransactions`, `SettlementTransactions` (TPT)
- `StripeEvents`
- `PayoutAccounts`

Configs (all `internal`, in `Payment.Infrastructure/Data/Configurations/`):
- `TransactionEntityConfiguration` (moved from `Data.Infrastructure/Data/Configurations/`)
- `StripeEventEntityConfiguration`
- `PayoutAccountEntityConfiguration`

Default schema `"payment"` (matches per-module schema convention; cf. Contract's `"contract"`).

`Payment.Infrastructure.csproj`: EF Core SqlServer + Data.Infrastructure project ref.

### Step 6 — Lock IPaymentModule shape (Contracts)
**REVISED from earlier plan** — no `PayoutAccountProjectionHandler`, no Identity events.
`PayoutAccountEntity` is a Payment-owned system of record; Payment writes directly during
Stripe Connect onboarding. Step 6 work is locking the boundary shape:

```csharp
public interface IPaymentModule
{
    Task<Result<PaymentResponse>> PurchaseTicketsAsync(int concertId, int quantity,
        string? paymentMethodId, decimal price, Guid customerUserId, IContract contract,
        CancellationToken ct = default);
    Task<Result<PaymentResponse>> SettleBookingAsync(int bookingId, Guid payerUserId,
        Guid payeeUserId, decimal amount, IContract contract, CancellationToken ct = default);
}
```

Caller passes `IContract` as a method parameter — Payment never reads from Concert/Contract.
Microservice-ready (this becomes the Refit/HTTP contract if Payment ever extracts to its
own process).

`Payment.Contracts.csproj` gains ref to `Contract.Abstractions` (for `IContract`).

`Concert.Contracts.csproj` gains ref to `Payment.Contracts` (for `IPaymentModule`,
`PaymentResponse`). One-way, no cycle. Same shape as Concert depending on Contract.Abstractions.

### Step 7 — Move Infrastructure layer to Payment.Infrastructure
Move all services, repositories, factories, validators per the lists above:

- All payment services + fakes
- `StripeEventRepository`, `TransactionRepository`, new `PayoutAccountRepository`
- `StripeValidator`, `StripeValidationFactory`, `StripeAccountValidator`,
  `StripeCustomerValidator`
- `WebhookStrategyFactory`
- `ITicketPaymentStrategyFactory` + impl (5-line `GetRequiredKeyedService` wrapper, already
  internal; relocate from `Concertable.Infrastructure/Factories/`)
- `BackgroundTaskQueue`, `BackgroundTaskRunner`, `QueueHostedService`
- Webhook handlers (`WebhookService`, `WebhookProcessor`, `WebhookQueue`,
  `TicketWebhookHandler`, `SettlementWebhookHandler`) from
  `Concert.Infrastructure/Services/Webhook/` to `Payment.Infrastructure/Services/Webhook/`.
  They call Concert via `IConcertWorkflowModule` / `ITicketPaymentModule`.

**`PaymentModule` impl** (`internal sealed class PaymentModule : IPaymentModule`,
`Payment.Infrastructure/PaymentModule.cs`):
- `PurchaseTicketsAsync` → `ITicketPaymentStrategyFactory.Create(contract.ContractType).PayAsync(...)`.
  Strategy method takes the data it needs as primitive args (no `IContract` per-method
  param — reverted lock); if a specific strategy needs contract terms it casts the
  `IContract` arg forwarded by the module.
- `SettleBookingAsync` → existing settlement service path; same shape.

**Stripe IDs cutover:** rewrite all `userEntity.StripeAccountId` / `.StripeCustomerId` reads
to `PayoutAccountRepository.GetByUserIdAsync(userId)`. Remove the two columns from
`UserEntity` and `IdentityDbContext` config. Identity migration re-scaffold absorbs the drop.

**Concert wiring (Option B):** rewrite `TicketService.PurchaseAsync` (now in
Concert.Infrastructure post Pre-Step-1) — replace `ITicketPaymentDispatcher` +
`IConcertRepository.GetByIdAsync(price)` chain with: `contract = await
contractLookup.GetByConcertIdAsync(concertId)` →
`paymentModule.PurchaseTicketsAsync(concertId, quantity, paymentMethodId, price,
currentUser.GetId(), contract, ct)`. Concert keeps the eligibility validation
(`TicketValidator`) as part of orchestration.

**`SettlementWebhookHandler`** body → `await transactionService.CompleteAsync(intent.Id);
await concertLifecycleModule.SettleAsync(int.Parse(intent.Metadata["bookingId"]), ct);`.

**`TicketWebhookHandler`** body → log transaction → `concertTicketPaymentModule.IssueTicketsAsync(...)`.

Reconcile Workers DI: drop non-keyed `IPaymentService` + `IManagerPaymentService`; register
`"offSession"` keyed only.

`Payment.Infrastructure.csproj` refs:
- Payment.Application, Payment.Contracts, Payment.Domain
- Concert.Contracts (for callback facades)
- Contract.Abstractions (for `IContract`, `IContractStrategy` marker, `ContractType`,
  `PaymentMethod`)
- Identity.Contracts (for `ICurrentUser` only — no events)
- Stripe.net NuGet

### Step 8 — Move controllers to Payment.Api
Move from `Concertable.Web/Controllers/`:
- `PaymentController`, `StripeAccountController`, `TransactionController`, `WebhookController`

`TicketController` STAYS in Concert (already there post Concert extraction; Option B owns
ticket purchase orchestration there).

All `internal` — `Payment.Api` gets `InternalControllerFeatureProvider` (Venue/Concert
pattern). `AddPaymentApi()` calls `AddPaymentModule()` + `AddApplicationPart(...)`.

`Concertable.Web` drops direct refs to those four controllers; gains `Payment.Api`
project ref.

### Step 9 — PaymentDevSeeder + PaymentTestSeeder
`internal class PaymentDevSeeder : IDevSeeder` and `PaymentTestSeeder : ITestSeeder`, both
`Order = 5` (after Concert at 4). Inject `PaymentDbContext`.

Seed: `PayoutAccountEntity` rows for seeded managers + customers (so payment workflows have
Stripe data on first run, no Connect onboarding required for tests). Test seeder seeds
predictable transaction + stripe event fixtures for integration tests.

`UserEntity` lost `StripeAccountId` / `StripeCustomerId` in Step 2 — seeders that previously
set those props on user fakers now create matching `PayoutAccountEntity` rows instead.

### Step 10 — Remove Payment DbSets from ApplicationDbContext
Remove `DbSet<TransactionEntity>`, `DbSet<TicketTransactionEntity>`,
`DbSet<SettlementTransactionEntity>`, `DbSet<StripeEventEntity>` from `ApplicationDbContext`.
Add `ExcludeFromMigrations` for all four (until AppDb retires).

### Step 11 — Migration scaffolding
Delete all `Migrations/` folders. Rescaffold `InitialCreate` in dependency order:
`Shared → Identity → Artist → Venue → Concert → Contract → Payment → AppDb`

Payment migration:
- `EnsureSchema("payment")` (matches default context schema)
- TPT for `Transactions` + `TicketTransactions` + `SettlementTransactions`
- `StripeEvents`, `PayoutAccounts`
- Cross-context FK on `TicketTransactions.TicketId → concert.Tickets.Id` is cross-context
  → strip the `table.ForeignKey(...)` declaration per `feedback_cross_context_fk.md`.
  Concert migrates first so the principal exists when AppDb runs, but the SQL FK constraint
  shouldn't be declared in Payment's migration.
- Identity migration loses `StripeAccountId` + `StripeCustomerId` columns (clean re-scaffold).

### Step 12 — Wire AddPaymentApi() in Web + Workers
- `Web/Program.cs`: `services.AddPaymentApi(builder.Configuration)`
- `Workers/ServiceCollectionExtensions.cs`: `services.AddPaymentModule(configuration)`
  (no Api)
- Remove TEMPORARY `InternalsVisibleTo` grants from `Payment.Application/AssemblyInfo.cs`
  as callers migrate to module boundaries (Step 3 grants now retire).

### Step 13 — Full test suite
Run unit + integration suites. Expected issues:
- Stripe webhook integration tests need `FakeWebhookService` wired correctly through Payment DI.
- `PayoutAccountEntity` test seed must match what payment workflow tests expect.
- `DynamicProxyGenAssembly2` IVT for any newly-internal interface that NSubstitute proxies
  (`feedback_castle_proxy_ivt.md`).

---

## Appendix A — DI snapshot (discovery output, 2026-04-24)

### A.1 Web host — `api/Concertable.Web/Extensions/ServiceCollectionExtensions.cs`

**Stripe SDK singletons (real-only, skipped when fake-mode):**
- `Stripe.AccountService`, `Stripe.AccountLinkService`, `Stripe.CustomerService`,
  `Stripe.PaymentMethodService`, `Stripe.SetupIntentService` (all Singleton)
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
- `IManagerPaymentService → ManagerPaymentService` via `AddKeyedManagerPaymentService(key)`:
  keyed `"onSession"`, keyed `"offSession"`

**Customer + ticket + transaction:**
- `ICustomerPaymentService → CustomerPaymentService` (Scoped)
- `ITicketService → TicketService` (Scoped) — moves to Concert in Pre-Step-1
- `ITransactionService → TransactionService` (Scoped)
- `ITicketValidator → TicketValidator` (Scoped) — stays Concert

**Ticket payment strategies (keyed by `ContractType`):**
- `ITicketPaymentStrategy`:
  - `ContractType.FlatFee → VenueTicketPaymentService`
  - `ContractType.DoorSplit → VenueTicketPaymentService`
  - `ContractType.Versus → VenueTicketPaymentService`
  - `ContractType.VenueHire → ArtistTicketPaymentService`

**Webhook plumbing:**
- Web: `IWebhookStrategyFactory → WebhookStrategyFactory` (Scoped)
- Concert.Infrastructure `AddConcertModule()`:
  - `IWebhookProcessor → WebhookProcessor`
  - `IWebhookQueue → WebhookQueue`
  - `IWebhookStrategy` keyed: `WebhookType.Concert → TicketWebhookHandler`,
    `WebhookType.Settlement → SettlementWebhookHandler`

**Background queue (Web-only):**
- `IBackgroundTaskQueue → BackgroundTaskQueue` (Singleton)
- `IBackgroundTaskRunner → BackgroundTaskRunner` (Singleton)
- `services.AddHostedService<QueueHostedService>()`

### A.2 Workers host — `api/Concertable.Workers/ServiceCollectionExtensions.cs`

- `IPaymentService → PaymentService` (Scoped, **non-keyed**) — drift vs Web's keyed pair
- `IStripeAccountService → StripeAccountService` (Scoped, real only)
- `IManagerPaymentService → ManagerPaymentService` (Scoped, **non-keyed**) — drift
- **No** queue/hosted-service registration
- **No** fake-mode toggle (real Stripe only)

### A.3 Cross-host discrepancies (resolved in Step 7)

1. **Keyed `IManagerPaymentService`** — Workers registers `"offSession"` keyed only.
2. **Keyed `IPaymentService`** — same: `"offSession"` keyed only.
3. **`QueueHostedService` missing in Workers** — keep Web-only.
4. **Fake-mode toggle missing in Workers** — fine; integration tests run against Web.

### A.4 Webhook handler file locations (confirmed)

- `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Webhook/TicketWebhookHandler.cs`
- `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Webhook/SettlementWebhookHandler.cs`
- `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Webhook/WebhookService.cs`
- `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Webhook/WebhookProcessor.cs`
- `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Webhook/WebhookQueue.cs`

All five move to `Payment.Infrastructure/Services/Webhook/` in Step 7.

### A.5 `SettlementWebhookHandler` body (verbatim)

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

Step 7 rewrites: `ISettlementDispatcher` → `IConcertWorkflowModule.SettleAsync(bookingId, ct)`.

---

## Appendix B — Known cross-module couplings at extraction start

| Caller | Dependency | Resolution |
|--------|-----------|-----------|
| `TicketService.PurchaseAsync` (Concert post Pre-Step-1) | `ITicketPaymentDispatcher` (legacy) | → `IPaymentModule.PurchaseTicketsAsync(..., contract)` (Step 7) |
| `TicketService.CompleteAsync` (Concert post Pre-Step-1) | called directly by `TicketWebhookHandler` | → exposed via `ITicketPaymentModule.IssueTicketsAsync` (Step 4) |
| `SettlementWebhookHandler` (moves to Payment in Step 7) | `ISettlementDispatcher` (Concert-internal) | → `IConcertWorkflowModule.SettleAsync` (Step 4) |
| `TicketWebhookHandler` (moves to Payment in Step 7) | `TicketService.CompleteAsync` direct call | → `ITicketPaymentModule.IssueTicketsAsync` (Step 4) |
| Payment workflows | `UserEntity.StripeAccountId` / `StripeCustomerId` reads | → `PayoutAccountRepository.GetByUserIdAsync` (Step 7); columns dropped from `UserEntity` |
| Payment workflows | Direct subtype reads on `*ContractEntity` | → `IContract` passed in via `IPaymentModule` boundary (Step 6/7) |
