# Technical Debt

All `// BROKEN`, `// TODO`, and `// TEMPORARY` markers found in the codebase as of 2026-05-23.
Work through these in order — functional bugs first, then missing features, then architectural cleanup.

---

## 1. Functional Bugs (BROKEN) — runtime behaviour is wrong

~~### B1 — Capacity validation disabled~~
~~### B2 — Total revenue always returns zero~~
~~### B3 — Artist review eligibility always returns false~~
~~### B4 — Venue review eligibility always returns false~~

**B1–B4 resolved.** `ConcertEntity.TicketsSold` counter maintained via `TicketSaleProcessor` (subscribes to `PaymentSucceededEvent` type=Ticket). Validator and revenue method both use it. Review eligibility uses `TicketEntity.HasReview` (set via `CustomerReviewSubmittedEvent`) with single `AnyAsync` queries — structurally identical to the pre-extraction monolith. `ReviewSubmittedEvent` ownership also corrected: moved to `Customer.Review.Contracts` as `CustomerReviewSubmittedEvent` with `TicketId` included.

---

## 2. Missing Features (TODO) — hardcoded placeholders

### F1 — Artist dashboard KPIs are stubs
**File:** `Concertable.B2B/Modules/Artist/Concertable.Artist.Infrastructure/Services/ArtistDashboardService.cs` (lines ~23, ~31)
**Method:** `GetKpisAsync()`
**Problem:** `mtdPayoutsTask` call to `paymentModule.GetArtistPayoutsMtdAsync` is commented out. `AcceptedAwaitingCheckout` is hardcoded `0`.
**Fix required:** Wire `IPaymentModule.GetArtistPayoutsMtdAsync`. Implement `AcceptedAwaitingCheckout` via `IConcertWorkflowCapabilityRegistry` / `IAcceptsCheckout` (or equivalent post-extraction contract).

---

### F2 — Venue dashboard revenue is a stub
**File:** `Concertable.B2B/Modules/Venue/Concertable.Venue.Infrastructure/Services/VenueDashboardService.cs` (line ~23)
**Method:** `GetKpisAsync()`
**Problem:** `mtdRevenueTask` call to `paymentModule.GetVenueTicketRevenueMtdAsync` is commented out.
**Fix required:** Same as F1 — wire the Payment module call once the Payment service exposes that endpoint.

---

## 3. Architectural Cleanup (TEMPORARY) — IVT / legacy coupling

These do not affect runtime correctness but should be retired as modules stabilise.

### A1 — B2B.Workers IVT for internal logger types
**File:** `Concertable.B2B/Concertable.B2B.Workers/AssemblyInfo.cs` (line 4)
**Problem:** Castle Core IVT granted so `Concertable.Workers.UnitTests` can mock loggers typed against internal Worker functions (`ConcertFinishedFunction` etc.).
**Retire when:** Worker tests move into per-module test projects.

### A2 — Concert.Application IVT to Concertable.Infrastructure
**File:** `Concertable.B2B/Modules/Concert/Concertable.Concert.Application/AssemblyInfo.cs` (line 10)
**Problem:** Legacy `Concertable.Infrastructure` still hosts Payment + Ticket services that inject `Concert.Application` internals.
**Retire when:** Those services are fully extracted into `Concertable.Payment.Infrastructure`.

### A3 — Concert.Application IVT to B2B.Workers
**File:** `Concertable.B2B/Modules/Concert/Concertable.Concert.Application/AssemblyInfo.cs` (line 14)
**Problem:** `Concertable.B2B.Workers` (`ConcertFinishedFunction`) injects `IConcertRepository` + `ICompletionDispatcher`.
**Retire when:** `ConcertFinishedFunction` moves into `Concert.Api` or a Concert-owned worker.

### A4 — Concert.Application IVT to B2B.Web
**File:** `Concertable.B2B/Modules/Concert/Concertable.Concert.Application/AssemblyInfo.cs` (line 17)
**Problem:** `Concertable.B2B.Web` E2E endpoint extensions inject `ICompletionDispatcher`; `ServiceCollectionExtensions` keyed-registers `ITicketPaymentStrategy` impls.
**Retire when:** Both move into `Concert.Api` / `Payment.Infrastructure`.

### A5 — Concert.Infrastructure IVT to B2B.Web for WebhookService
**File:** `Concertable.B2B/Modules/Concert/Concertable.Concert.Infrastructure/AssemblyInfo.cs` (line 8)
**Problem:** `B2B.Web` injects internal `WebhookService`.
**Retire when:** Webhook routing moves into `Concert.Api` or a Payment-owned host.

### A6 — Payment.Application IVT to B2B.Web + B2B.Workers
**File:** `Concertable.Payment/Concertable.Payment.Application/AssemblyInfo.cs` (line 5)
**Problem:** Legacy hosts consume Payment internals. `Concertable.Infrastructure` entry already retired at Step 10.
**Retire when:** Remaining Steps 7/8/12 retire those hosts.

### A7 — Payment.Application IVT to Concert.Infrastructure (Stripe eligibility)
**File:** `Concertable.Payment/Concertable.Payment.Application/AssemblyInfo.cs` (line 15)
**Problem:** `Concert.Infrastructure` uses `IStripeValidator` + `IStripeValidationFactory` for pre-create/pre-apply eligibility checks.
**Retire when:** Stripe eligibility routes through a `Payment.Contracts` facade.
