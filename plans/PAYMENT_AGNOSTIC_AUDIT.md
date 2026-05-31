# Payment ↔ B2B Coupling Audit

Investigation of whether `Concertable.Payment` violates its "agnostic foundational adapter"
premise by depending **up** into B2B. Branch `Refactor/Microservices`, uncommitted tree.
Scope: `api/` only (worktrees ignored). No code changed.

**Verdict up front:** Confirmed. Payment compile-depends on B2B in three places, all
low-value or dead, plus a hidden transitive coupling through a shared lib, plus a reverse
leak (B2B reading Payment's tables). The only Payment→B2B dependency tied to live behaviour
is the `ConcertChangedEvent` subscription used for payee routing — and even that is avoidable.

---

## 1. Complete cross-boundary inventory

### A. Payment → B2B — compile-time (illegitimate / backwards arrows)

| # | Ref | Project | Concrete usage | Status |
|---|-----|---------|----------------|--------|
| 1 | `B2B.Contract.Contracts` | Payment.Application | `ContractType` enum (global using) — only in `IStripeValidationFactory.Create(ContractType)` signature | **Dead** |
| 2 | `B2B.Contract.Contracts` | Payment.Infrastructure | `ContractType` — keys 4 `IStripeValidationStrategy` DI registrations (`ServiceCollectionExtensions.cs:104-107`) + `StripeValidationFactory.Create` | **Dead** |
| 3 | `B2B.Concert.Contracts` | Payment.Infrastructure | `ConcertChangedEvent` in `ConcertPayeeProjectionHandler.cs`, its subscription (`ServiceCollectionExtensions.cs:118`), and `Payment.Workers/Program.cs:8` | **Live** (payee projection) |
| 4 | `B2B.User.Contracts` (global using, *no csproj ref*) | Payment.Application + Payment.Infrastructure | `using Concertable.B2B.User.Contracts;` in `TransactionService.cs:3` + `EscrowServiceTests.cs:5` + both GlobalUsings. Its only type, `ManagerDto`, is **never referenced** | **Dead** |

On #1/#2: the `IStripeValidationFactory.Create(ContractType)` factory and the keyed
`StripeValidationStrategy` graph it feeds are **not invoked anywhere** — no constructor injects
`IStripeValidator`/`IStripeValidationFactory` in Payment or B2B. The `AssemblyInfo.cs`
`InternalsVisibleTo("Concertable.B2B.Concert.Infrastructure")` with the comment *"TEMPORARY
until eligibility routes through a Payment.Contracts facade"* is **stale** — B2B.Concert no
longer references those types in any source file. So the sole reason `B2B.Contract.Contracts`
is referenced (the `ContractType` enum) props up dead code.

### B. Payment → B2B — transitive, hidden (shared-lib re-monolithing)

`Concertable.DataAccess.Application` is a **shared** infra lib (generic `IRepository<T>`,
`IUnitOfWork`, specifications) referenced by **every** service (Auth, B2B, Customer, Search,
Payment). Its `.csproj` + `GlobalUsings.cs` carry `ProjectReference`/`global using` to
`B2B.User/Artist/Venue/Concert.Domain` **and `Payment.Domain`**. **No file in the project uses
any of them** — every type is generic over Kernel interfaces (`IIdEntity`, `IHasDateRange`).
These are stale monolith-era references that silently make Payment (and Customer, Search, Auth)
compile against B2B's *internal domain entities*. Pure dead weight; removable for free.

### C. B2B / Customer → Payment — legitimate

- `B2B.Concert.Infrastructure` → `Payment.Client` + `Payment.Contracts` ✓
- `B2B.Concert.Application` → `Payment.Client` ✓
- `Customer.Ticket.Application` → `Payment.Client` ✓
- `Customer.Ticket.Infrastructure` → `Payment.Contracts` ✓

`PaymentSucceededEvent`/`PaymentFailedEvent` now live in `Payment.Contracts.Events` (not
`Payment.Domain`). Consumers reference the Contracts project. This is the correct
`consumers → Payment` direction.

### D. B2B → Payment — illegitimate (reverse leak)

`B2B.DataAccess/IReadDbContext.cs` + `ReadDbContext.cs` expose **Payment.Domain** entities as
`IQueryable`: `Transactions`, `TicketTransactions`, `SettlementTransactions`, `StripeEvents`,
`PayoutAccounts`, `Escrows`. B2B reads Payment's tables from a single DbContext bound to the
`B2BDb` connection. The compile path works only via the leak in (B): `B2B.DataAccess →
DataAccess.Infrastructure → DataAccess.Application → Payment.Domain`.

- **No B2B production code queries these members.** Live usage is test-only: escrow assertions
  in `ApplicationVenueHireApiTests.cs:138` / `ApplicationFlatFeeApiTests.cs:108`, and
  `MockEscrowClient.cs` writing `dbContext.Escrows`.
- Post-extraction these tables live in **PaymentDb**, not B2BDb — so these `ReadDbContext`
  members point at a connection where the tables no longer exist. Dead + wrong, not just leaky.

### E. Payment → B2B — runtime event subscription (decoupled in principle)

Payment.Workers subscribes (ASB):
- `ConcertChangedEvent` (B2B.Concert) → `ConcertPayeeProjectionHandler` — **this is the live coupling**.
- `CredentialRegisteredEvent` (Auth.Contracts) → payout provisioning — legitimate (Auth is upstream of everyone).
- Its own `PaymentSucceededEvent` / `PaymentFailedEvent`.

Subscribing to an upstream event is fine for an adapter; **compile-depending on the producer's
typed contract to do so is the violation.**

---

## 2. Root need behind each Payment → B2B usage

- **#3 ConcertChangedEvent → ConcertPayee projection.** Payment must resolve
  `concertId → payeeUserId` for customer ticket charges where the caller passes only
  `concert_id` (`CustomerPayRequest`/`CreatePaymentSessionRequest` in `payment.proto`).
  `CustomerPaymentService.cs:40,63` calls `concertPayeeRepository.GetPayeeUserIdAsync(concertId)`.
  Payment stores only `ConcertPayeeEntity(int ConcertId, Guid PayeeUserId)` — an opaque-keyed
  routing fact. **It consumes 2 of the 17 fields on `ConcertChangedEvent`.** Root need = "who
  receives money for concert N", nothing B2B-shaped.
- **#1/#2 ContractType.** Root need today = **none** (dead wiring). If eligibility validation is
  revived, the need is "which Stripe validation flavour" — a Payment-side concern, not B2B's
  contract taxonomy.
- **#4 B2B.User.Contracts / ManagerDto.** Root need = **none**.
- **(D) reverse leak.** Root need = integration-test observability of escrow state after a hire
  flow — a *test* need, not a production read.

---

## 3. Verdict — agnostic-violating vs acceptable

Payment **is** agnostic-violating at compile time. Distinguishing the cases:

- **#1/#2/#4 + shared-lib leak (B): unambiguous violations, zero value.** Dead code and stale
  references. No defensible reason; remove outright.
- **#3 `B2B.Concert.Contracts`: technically within the *letter* of ARCHITECTURE.md** (it allows
  cross-service refs to `*.Contracts`, and this is a Contracts project) **but violates the
  *spirit*.** Payment is the foundational adapter at the bottom of the stack; pointing its
  compile graph **up** at a specific consumer's contract means Payment can't build/ship without
  B2B's package and its redeploys couple to B2B contract churn. The arrow is backwards
  regardless of layer.
- **(D) reverse leak: unambiguous violation** of both the microservice premise ("services never
  depend on each other's runtime code") and the modular rule ("each context reads only its own
  DbContext").
- **(E) runtime subscription: acceptable pattern, wrong contract.** An adapter learning routing
  facts via an event bus is correct — *if* it subscribes through a generic or Payment-owned
  contract, not B2B's typed event.

---

## 4. Target design (recommended)

**Payment owns its inbound contract; opaque facts flow in from callers/producers.**

**(A) Payee routing — primary: pass it in the gRPC request (A1).**
Add `payee_id` to `CustomerPayRequest` + `CreatePaymentSessionRequest`, exactly as
`ManagerPayRequest` already carries it. Callers (B2B/Customer) resolve the payee from the concert
(they already own that data) and pass it. Then Payment needs **no** ConcertPayee projection,
**no** handler/repo/entity/migration, and **no** `B2B.Concert.Contracts` reference.
`CustomerPaymentService` already stamps `toUserId` into Stripe metadata, so downstream
webhook/settlement paths keep the payee without re-resolving.

**Fallback (A2)** if a webhook/settlement path ever needs `concertId → payee` with no caller in
the loop: keep the projection but feed it from a **Payment-owned** generic event
(`Payment.Contracts.Events.PayeeRoutingChangedEvent(int ConcertId, Guid PayeeUserId)`, or a
shared `Concertable.Contracts` event) that B2B publishes/maps to. Drop the `B2B.Concert.Contracts`
ref either way.

**(B) ContractType.** Delete the dead validation wiring + the `B2B.Contract.Contracts` ref. If
validation is revived, route it through a `Payment.Contracts` facade taking a **Payment-owned**
enum (e.g. `PaymentValidationKind { ConnectedAccount, Customer }`); map B2B's `ContractType` →
that kind **on the B2B side**.

**(C) Reverse leak.** Remove Payment.Domain members from `IReadDbContext`/`ReadDbContext` and the
`Payment.Domain` ref from `DataAccess.Application`. Re-point the test-only escrow assertions and
`MockEscrowClient` at the Payment Client/DTOs (or a Payment-side test store) instead of B2B's
DbContext.

**(D) Shared lib.** Strip the 6 stale domain `ProjectReference`s + GlobalUsings from
`DataAccess.Application`.

**Tradeoffs.** A1 deletes a whole projection/handler/repo/entity/migration (less Payment code)
but makes callers responsible for supplying the payee every call. A2 keeps Payment authoritative
and resilient to caller mistakes at the cost of one generic event + the projection. Recommend
**A1** — it matches the "Payment stores opaque ids and is told what to do" model and the
metadata already carries the payee downstream; fall to A2 only if a no-caller path surfaces.
Owning a Payment-side validation enum duplicates 4 values — that is the correct, healthy cost of
the boundary.

---

## 5. Migration steps (ordered)

1. **Free deletions (no behaviour change):**
   - Remove `B2B.Contract.Contracts` from Payment.Application + Payment.Infrastructure csproj;
     drop `ContractType` GlobalUsings; delete the dead `IStripeValidator` /
     `IStripeValidationFactory` / `StripeValidator` / `StripeValidationFactory` /
     `IStripeValidationStrategy` graph + the 4 keyed registrations.
   - Remove `B2B.User.Contracts` GlobalUsings + the `using` in `TransactionService.cs` /
     `EscrowServiceTests.cs`.
   - Remove the stale `InternalsVisibleTo("Concertable.B2B.Concert.Infrastructure")` + TEMPORARY
     comment from `Payment.Application/AssemblyInfo.cs`.
2. **Clean shared lib:** remove the 6 domain `ProjectReference`s + GlobalUsings from
   `DataAccess.Application`; rebuild all services to confirm nothing used them.
3. **Re-route payee (A1):** add `payee_id` to `CustomerPayRequest` + `CreatePaymentSessionRequest`
   in `payment.proto`; update B2B/Customer callers to pass it; change `CustomerPaymentService` to
   read it from the request; delete `ConcertPayeeProjectionHandler`, `ConcertPayeeRepository`,
   `ConcertPayeeEntity` + EF config, the subscription, and the `B2B.Concert.Contracts` ref from
   Payment.Infrastructure + Payment.Workers. (Or A2 if a no-caller path exists.)
4. **Reverse leak:** drop Payment.Domain members from `IReadDbContext`/`ReadDbContext`; re-point
   B2B escrow test assertions + `MockEscrowClient` to Payment Client/DTOs.
5. **Re-scaffold migrations once** via `./initial-migrations.ps1` (per CLAUDE.md) after the
   entity/projection removals.

---

## 6. Interaction with PAYMENT_SEED_CATALOG work (paused, pending this)

- The plan's stated **prerequisite blocker** ("consumers reference `Payment.Domain` for events")
  is **already resolved** on this branch: the events live in `Payment.Contracts.Events`, and
  B2B.Concert + Customer.Ticket reference `Payment.Contracts`. That blocker is cleared.
- **New interaction this audit surfaces:** the Payment **Seed Simulator** is meant to ship
  Payment's cross-boundary events *without* Payment's full runtime, proving Payment deploys
  independently. As long as Payment.Infrastructure compile-depends on `B2B.Concert.Contracts`
  (#3) and `B2B.Contract.Contracts` (#1/#2), the simulator's dependency closure drags in B2B
  contracts — undercutting that proof. **Land steps 1–3 before/with the simulator** so the
  closure is Payment + Shared + Auth.Contracts only.
- The reverse leak (B) means B2B.Web/Workers + the B2B integration fixture still transitively
  reference Payment.Domain. Not a hard blocker for the catalog, but it's the same violation class
  the catalog work is retiring — fold step 4 into the same pass.
- The plan's separate B2B seeding FK blocker (`Opportunities → concert.VenueReadModels`) is
  unrelated to Payment coupling and out of scope here.
