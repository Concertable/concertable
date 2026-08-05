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

1. **PR1 committed (`6daede3ff`) — build + unit green.** Push `Refactor/launch_money-value-type` and open the PR with `skip-e2e` (breaking published `Concertable.Payment.Client` cut-over: merge-queue E2E can't pass until the pin bumps — same rationale as Phase 2's #207; unit + integration + build still run and gate it). Personal repo → plain `gh pr create` (no AB#).
2. **Merge via `/merge`** (currency check → auto-merge → confirm loop). On merge, `Payment.Client` republishes → `chore/platform-sync-*` sync PR opens and goes **RED** (consumers still call the old decimal shape — expected).
3. **Own the sync PR to green** → execute the "Sync-PR (consumer/contract) migration plan" in Current state (B2B call sites + `ISettlementAmountResolver` + `deal.Fee`/`HireFee` + `./initial-migrations.ps1` re-scaffold + mocks + Customer PayAsync). Build B2B+Customer 0 errors, grep gate zero, push. **COORDINATE the VAT/settlement seam with `Feature/launch_tenant-config-surface`.** That green sync PR is the plan's terminal gate → then close out the plan + ledger.

## Completed work

- **Phase 1 — `Money` + `Currency` in Kernel (reconstructed baseline).** `Concertable.Kernel.ValueObjects.Money` (`readonly record struct Money(decimal Amount, Currency Currency)`) + `Currency` enum present at `api/Concertable.Shared/src/Concertable.Kernel/ValueObjects/Money.cs`: `ToMinorUnits()` (`AwayFromZero`), `FromMinorUnits`, `Zero`, `Gbp(decimal)`, `+`/`-` with same-currency `DomainException` guard. Published (Kernel pin ≥ `0.1.0-alpha.0.662`); on `origin/main` today. PRs #205 (`Money.Gbp`)/#207.
- **Phase 2 — Payment adopts `Money` + gRPC nested-`Money` wire (reconstructed baseline).** Payment internals + the gRPC wire carry `Money`; the 6 (+fake) truncating casts in `Payment/src` are gone; `EscrowEntity.Amount` is an EF `ComplexProperty` (`Amount decimal(18,2)` + `Currency int`). Published C# client interfaces kept `decimal` params (adapters wrap `Money.Gbp(x).ToProtoMoney()`). Sync #208 → pin `0.1.0-alpha.0.664`; validated by local B2B API-E2E 10/10 on `main`. PR #207.

## Verification

- 2026-08-05: `Money.cs` + `Money.Gbp` present on `origin/main` worktree (P1 confirmed). Grep gate run (see Current state). Customer Ticket Infrastructure `.csproj` references `Concertable.Kernel` package (Money resolvable for Phase 4).
- 2026-08-05 (PR1, commit `6daede3ff`):
  - **Build** — full `api/Concertable.slnx` green earlier for Phase 4 (exit 0). For the Phase 5 publisher change, targeted builds (full slnx exceeds the 10-min foreground cap and background builds get killed at turn boundaries here): `Concertable.Payment.Client` 0 err, `Concertable.Payment.E2ETests.Helpers` 0 err (publisher side compiles with `Money`), **`Concertable.B2B.Concert.Infrastructure` 0 err + `Concertable.Customer.Ticket.Infrastructure` 0 err (consumers still compile against the OLD pinned package → expand/contract split confirmed)**. B2B/Customer source is untouched by Phase 5, so their package-boundary compile is unchanged from the green Phase-4 full build.
  - **Unit** — `Concertable.Payment.UnitTests` 138/138, `Concertable.Customer.Ticket.UnitTests` 18/18.
  - **Integration** — BLOCKED locally: `Microsoft.Data.SqlClient.SNI.dll` `DllNotFoundException` `0x800700CE` (Windows MAX_PATH exceeded by the SNI native probe path in this deep worktree). Environmental, affects all integration tests here regardless of the change; the merge queue runs integration on normal CI paths as the authoritative gate. Change is a type refactor + GBP rounding no-op, fully covered by build + unit locally.

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

### 2026-08-05 — PR1 committed (Phase 4 + Phase 5 publisher)

- Action: Applied Phase 4 (Customer `TicketService`) + Phase 5 publisher side (`Payment.Client` 5 interface methods `decimal`→`Money` + 3 adapters, deleted orphaned `Client/EscrowDto.cs`, `StripeFixture` 2 casts→`Money`). Committed.
- Evidence: commit `6daede3ff`; targeted builds all 0 err (publisher + both consumers on old package); unit Payment 138/138 + Customer Ticket 18/18; integration blocked locally by worktree MAX_PATH (env, not code — see Verification).
- Outcome: expand/contract split proven; publisher side ready to publish.
- Follow-up: push, open PR (`skip-e2e`), merge, then own the sync PR consumer migration.

## Resume prompt

```
cd C:/Users/TommySeery/source/repos/Concertable.worktrees/Refactor/launch_money-value-type
Read @plans/launch/MONEY_VALUE_TYPE_PLAN.md and @plans/launch/MONEY_VALUE_TYPE_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
