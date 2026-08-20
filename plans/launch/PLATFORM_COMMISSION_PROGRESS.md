# Percentage platform commission and pricing transparency progress

- Plan: `plans/launch/PLATFORM_COMMISSION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/platform-commission`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_platform-commission-phase2`
- Branch: `Feature/launch_platform-commission-phase2` (off `origin/main`)
- PR: none yet — Phase 2 implementation branch, not yet pushed
- Dependency/package gates: **cleared.** Phase 1 and Phase 1b are merged (PR #296 merged 2026-08-07); no open or red `chore/platform-sync-*` PR. The expanded, binding-owned Payment runtime is live on `origin/main`, so Phase 2 may consume it.
- Last reconciled: 2026-08-20 against `origin/main` and GitHub (PR #296 MERGED, no open platform-sync PR)

## Current state

Phase 1b's superseded "HOLD at the Kernel-convention dependency" is obsolete: that gate concerned PR #296,
which merged 2026-08-07. This worktree is a fresh checkout of `origin/main`, 0 behind, and owns **Phase 2 —
B2B gross ownership and percentage cut-over**.

Phase 2 step 1 (the four keyed **pure** gross strategies) is implemented and green locally. The existing
Concert-module gross logic (`ISettlementAmountResolver` + four impure `*SettlementAmount` strategies, whose
revenue-share variants injected `IConcertRepository`) has been refactored into a pure, deal-type-keyed
`ISettlementGrossCalculator` family that owns only the arithmetic; the async `ISettlementAmountResolver`
facade now loads eligible takings once and delegates the formula, so consumers (`PayoutFinishStep`,
`InvoiceIssuer`) are unchanged. No parallel gross-calculation family was introduced.

## Next Steps

Continue Phase 2 in `plans/launch/PLATFORM_COMMISSION_PLAN.md` §10, on this branch:

2. Persist only `CommissionBindingId` on the application/booking path; add the frozen final-gross snapshot
   (`FinalSettlementGrossMinor` + eligible-takings inputs) for the deferred (DoorSplit / Guarantee Plus) deals.
3. Bind the rate at each payer commitment point and route all four payment journeys through the new
   binding-aware Payment methods (`ICommissionPricingClient` + binding-aware hold/capture/deposit/direct-pay),
   passing the binding ID and gross, never a rate or caller-supplied commission.
4. Add exact and deferred pricing DTOs, final takings review/attestation, and fail-closed error mapping.
5. Implement payer and artist disclosures in the manager SPAs.
6. Re-scaffold the Concert model where persistence changed.
7. Build the affected B2B/Payment projects and manager SPAs locally; run focused unit tests. Push the
   checkpoint for authoritative full build/carve/unit/integration CI. The merge queue remains the E2E gate.
8. Update this ledger and the launch trackers in the implementation commit(s).
9. **Hard stop:** merge and own publish/platform-sync before Phase 3 removes the legacy £10 Payment APIs.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_platform-commission-phase2
Read @plans/launch/PLATFORM_COMMISSION_PLAN.md and @plans/launch/PLATFORM_COMMISSION_PROGRESS.md and do what its `## Next Steps` says.
```

## Completed work

- **Phase 1 + 1b (Payment):** percentage configuration, immutable SQL history, Payment-issued bindings,
  additive preview/bind/bound-calculation contracts, binding-aware money-movement RPCs, transaction tax
  facts, multi-refund persistence, proportional refund logic. Merged via PR #296 (2026-08-07), published,
  platform-synced, deployed.
- **Phase 2 step 1 (B2B, this branch):** pure keyed `ISettlementGrossCalculator` family
  (`FlatFeeGrossCalculator`, `VenueHireGrossCalculator`, `DoorSplitGrossCalculator`, `VersusGrossCalculator`
  over a shared `RevenueShareGrossCalculator` base + `SettlementGross` minor-unit rounding), replacing the
  five impure `*SettlementAmount` strategy files; `SettlementAmountResolver` reduced to a key-agnostic async
  facade; DI + `DealStrategyArchitectureTests` + `ConcertDealStrategyFactoryTests` moved to the new family;
  exhaustive formula/rounding tests in `SettlementGrossCalculatorTests`.

## Verification

- `dotnet test` Concert.UnitTests (Settlement + Strategies filter): 69 passed, 0 failed.
- `dotnet test` Deal.UnitTests (`DealStrategyArchitectureTests`): 18 passed, 0 failed.
- Full Concert.UnitTests suite and the authoritative solution build / carve remain for the Phase 2
  push checkpoint (step 7).

## Event log

### 2026-08-20 — stale HOLD reconciled; Phase 2 started; step 1 landed

- Action: Created worktree/branch `Feature/launch_platform-commission-phase2` off `origin/main`. Found the
  ledger's `## Next Steps` HOLD (Kernel error-convention dependency) fully stale. Verified against GitHub:
  PR #296 MERGED 2026-08-07, no open/red platform-sync PR — the Phase 1b gate is cleared. Implemented Phase 2
  step 1: introduced the pure keyed `ISettlementGrossCalculator` family and refactored the impure
  `ISettlementAmountResolver` set into a single key-agnostic facade + pure calculators (no duplicate family).
- Evidence: branch 0 behind `origin/main`; deleted `FlatFeeSettlementAmount`/`VenueHireSettlementAmount`/
  `DoorSplitSettlementAmount`/`VersusSettlementAmount`/`RevenueShareSettlementAmount`; Concert.UnitTests
  Settlement+Strategies 69/69; Deal.UnitTests arch guard 18/18.
- Outcome: Step 1 complete and green locally. Formula single-sourced and now purely unit-testable.
- Follow-up: Phase 2 steps 2–9 per `## Next Steps`.
