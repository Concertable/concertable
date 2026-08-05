# Money value type — progress

- Plan: `plans/launch/MONEY_VALUE_TYPE_PLAN.md`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Refactor/launch_money-value-type`
- Branch: `Refactor/launch_money-value-type` (off `origin/main` @ `55c807784`)
- PR: `not opened`
- Dependency/package gates: Phase 5 is a breaking published-contract change (`Concertable.Payment.Client`) → publish-first; consumers migrate in the `chore/platform-sync-*` PR. Coordinate the B2B settlement/VAT seam with `Feature/launch_tenant-config-surface` (whichever lands second rebases).
- Last reconciled: 2026-08-05, off `origin/main` worktree evidence (grep gate + file reads).

## Current state

Fresh worktree off `origin/main`. Phases 1 & 2 already merged (see reconstructed baseline below). Phase 3 (platform fee) is out of scope here — owned by `PLATFORM_COMMISSION_PLAN.md` / PR #296. Remaining: Phase 4 (Customer, done in-tree) and Phase 5 (Contract signature swap — publisher side done in-tree, consumer side deferred to the sync PR).

**PR1 edits applied in-tree (uncommitted, pending build/test):**
- Phase 4: `TicketService.cs:130` → `Money.Gbp(concert.Price * quantity).ToMinorUnits()` (+ using).
- Phase 5 publisher/expand side (all inside the published `Concertable.Payment.Client` source, so B2B/Customer still compile against the OLD pinned package):
  - `IManagerPaymentClient.PayAsync`/`CreateHoldSessionAsync`, `ICustomerPaymentClient.PayAsync`, `IEscrowClient.DepositAsync`/`CaptureAsync`: `decimal amount` → `Money amount`.
  - Adapters `ManagerPaymentClient`/`CustomerPaymentClient`/`EscrowClient`: dropped `var money = Money.Gbp(amount)`, now `amount.ToProtoMoney()` (param is `Money`). `ToProtoMoney(this Money)` already existed in `PaymentMappers`.
  - Deleted orphaned public `Concertable.Payment.Client/EscrowDto.cs` (dead — only the internal `Application.DTOs.EscrowDto` is used by `IEscrowService`; the public one carried a money-typed `decimal Amount` and had zero consumers).
  - `StripeFixture.cs` (Payment E2E helper): 2 × `(long)(amount*100)` → `Money.Gbp(amount).ToMinorUnits()` (+ using).

**Why the consumer side can't be in PR1:** B2B/Customer reference `Concertable.Payment.Client` as a PINNED published package. Changing the client interface signatures republishes only on merge; until the pin bumps, consumers see the old `decimal` signatures. Migrating `deal.Fee`→`Money` or the B2B call sites now would force throwaway `.Amount` smears at the still-`decimal` boundary — exactly the anti-pattern the plan warns against. So all consumer work lands in the auto-opened `chore/platform-sync-*` PR after PR1 publishes.

**Sync-PR (consumer/contract) migration plan — exact surface (from Explore map):**
- B2B client call sites → pass `Money`: `PayoutFinishStep.cs:45` (PayAsync), `HoldCheckoutStep.cs:38` (CreateHoldSession), `CaptureEscrowAcceptStep.cs:46` (Capture), `DepositEscrowAcceptStep.cs:47` (Deposit).
- `ISettlementAmountResolver.ResolveGrossAsync` → `Task<Money>` + 3 impls (`FlatFee`/`VenueHire`/`RevenueShare`; wrap the revenue-share decimal with `Money.Gbp(...)` at the resolver boundary — the door-% arithmetic stays decimal, it's percentages).
- `deal.Fee`/`HireFee` → `Money`: `FlatFeeDeal`/`VenueHireDeal` (Deal.Contracts), `FlatFeeDealEntity`/`VenueHireDealEntity` (Deal.Domain), EF `ComplexProperty` mapping (Amount+Currency cols, mirroring EscrowEntity), **re-scaffold via `./initial-migrations.ps1`**, `FlatFeeDealMapper`/`VenueHireDealMapper`, `FlatPayment(Money)` (Checkout.cs), read sites (`SetupCheckoutStep`, `FlatFeePaymentAmountMapper`, `VenueHirePaymentAmountMapper`).
- `InvoiceIssuer.cs:40`: `gross` is `Money` → VAT boundary `GetVatCalculationAsync(decimal)` — pass `gross.Amount` unless the tenant-config branch migrates VAT to `Money` (COORDINATE at this seam).
- Test doubles → `Money`: `MockEscrowClient`, `MockEscrowClientFail`, `MockManagerPaymentClient`+`IMockManagerPaymentClient` (drop `(long)(amount*100)`, capture-tuple `decimal`→`Money`), `MockCustomerPaymentClient`; Moq setups/verifies (`HoldCheckoutStepTests`, `DepositEscrowAcceptStepTests`, etc.) `It.IsAny<decimal>()`→`It.IsAny<Money>()`.
- Customer `TicketService.cs:65` PayAsync call → pass `Money.Gbp(concert.Price * quantity)`.
- Grep gate: `rg -n "\(long\)\(.*\* 100|/ ?100m?\b" api` returns zero outside `Money` + the percentage allowlist; no money-typed `decimal` on a resolver/client signature.

Reconciliation findings vs the plan's Phase 4/5 text:
- **`Concertable.Payment.Seed/E2EStripeAccountClient` is ALREADY on `Money`** — `CreateHoldSessionAsync(string, Money amount, …)` uses `amount.ToMinorUnits()`. It moved with Phase 2's Payment-internal adoption (it implements the Payment-internal `IStripeAccountClient`). So **Phase 4 reduces to the single Customer site** `Customer.Ticket.Infrastructure/TicketService.cs:130`.
- **Grep-gate false positives to allowlist** (percentage math, not minor-unit conversion): `ArtistDoorPercent / 100` in `VersusCalculator.cs:10`, `VersusDealEntity.cs:40`, `DoorSplitDealEntity.cs:30`, `DoorSplitCalculator.cs:10`; and the `UkVatCalculatorTests` `/ 100` inline-data comment. These are `percent → fraction`, unrelated to `Money`.
- Genuine remaining pounds↔pence casts: `TicketService.cs:130` (Phase 4); test doubles `StripeFixture.cs:57,67`, `MockManagerPaymentClient.cs:26,81`, `MockEscrowClient.cs:38` (ride Phase 5 as they implement the client interfaces / simulate the wire).

## Next Steps

1. **Finish PR1 (this branch — Phase 4 + Phase 5 publisher side):** confirm `dotnet build api/Concertable.slnx` 0 errors (must show Payment.Client compiling with `Money` while B2B/Customer still build against the old package — the split), run affected tests via `integration-debug` (Customer Ticket + Payment unit), commit, open the PR. This PR is a **package cut-over** (breaking published `Concertable.Payment.Client` signatures) → the merge-queue E2E can't pass here (packaged consumers still on old wire) → ship `skip-e2e`, same rationale as Phase 2's #207.
2. **Own the `chore/platform-sync-*` PR after merge** → execute the "Sync-PR (consumer/contract) migration plan" in Current state (B2B call sites + `ISettlementAmountResolver` + `deal.Fee`/`HireFee` + re-scaffold + mocks + Customer PayAsync). Build B2B+Customer 0 errors, grep gate zero, push. That green sync PR is the plan's terminal gate.

## Completed work

- **Phase 1 — `Money` + `Currency` in Kernel (reconstructed baseline).** `Concertable.Kernel.ValueObjects.Money` (`readonly record struct Money(decimal Amount, Currency Currency)`) + `Currency` enum present at `api/Concertable.Shared/src/Concertable.Kernel/ValueObjects/Money.cs`: `ToMinorUnits()` (`AwayFromZero`), `FromMinorUnits`, `Zero`, `Gbp(decimal)`, `+`/`-` with same-currency `DomainException` guard. Published (Kernel pin ≥ `0.1.0-alpha.0.662`); on `origin/main` today. PRs #205 (`Money.Gbp`)/#207.
- **Phase 2 — Payment adopts `Money` + gRPC nested-`Money` wire (reconstructed baseline).** Payment internals + the gRPC wire carry `Money`; the 6 (+fake) truncating casts in `Payment/src` are gone; `EscrowEntity.Amount` is an EF `ComplexProperty` (`Amount decimal(18,2)` + `Currency int`). Published C# client interfaces kept `decimal` params (adapters wrap `Money.Gbp(x).ToProtoMoney()`). Sync #208 → pin `0.1.0-alpha.0.664`; validated by local B2B API-E2E 10/10 on `main`. PR #207.

## Verification

- 2026-08-05: `Money.cs` + `Money.Gbp` present on `origin/main` worktree (P1 confirmed). Grep gate run (see Current state). Customer Ticket Infrastructure `.csproj` references `Concertable.Kernel` package (Money resolvable for Phase 4).

## Reviews

<none yet>

## Decisions, discoveries, blockers, and deviations

- **Deviation from plan Phase 4 scope:** `E2EStripeAccountClient` already migrated in Phase 2; Phase 4 is Customer-only (`TicketService`).
- **Grep-gate allowlist:** door-percent `/ 100` and VAT test comment are percentage math, deliberate survivors (documented in Current state).
- **Coordination:** `Feature/launch_tenant-config-surface` may edit the same B2B settlement/VAT code (`ISettlementAmountResolver`, VAT calc) — whichever lands second rebases.

## Event log

### 2026-08-05 — resume + baseline

- Action: Created worktree `Refactor/launch_money-value-type` off `origin/main` (`55c807784`); reconciled plan vs code; created this reconstructed-baseline ledger.
- Evidence: `git worktree add`; `Money.cs` read; grep gate; `E2EStripeAccountClient.cs` already on `Money`.
- Outcome: Phase 4 scoped to `TicketService.cs:130`; Phase 5 contract surface mapping in progress.
- Follow-up: execute Phase 4, then Phase 5.

## Resume prompt

```
cd C:/Users/TommySeery/source/repos/Concertable.worktrees/Refactor/launch_money-value-type
Read @plans/launch/MONEY_VALUE_TYPE_PLAN.md and @plans/launch/MONEY_VALUE_TYPE_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
