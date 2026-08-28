# Percentage platform commission and pricing transparency progress

- Plan: `plans/launch/PLATFORM_COMMISSION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/platform-commission`
- Worktree: `C:/Users/tommy/source/repos/Concertable/.worktrees/Feature-launch_platform-commission-phase2`
  (created 2026-08-28 from `origin/main` `10a1aa0bb` for Phase 2). Phases 1 and 1b are terminal on `origin/main`.
- Branch: `Feature/launch_platform-commission-phase2` (Phase 2 active; Phase 1b delivered on
  `Feature/PaymentOwnedResultExpansion` via PR #392).
- PR: [#392 — refactor(payment): own typed operation results](https://github.com/Concertable/concertable/pull/392) (MERGED 2026-08-07) — absorbed and superseded [#296](https://github.com/Concertable/concertable/pull/296).
- Dependency/package gates: Phase 1b's breaking Payment package published and its generated platform
  sync migrated the B2B and Customer consumers. `ConcertablePlatformVersion` has since advanced to
  `0.1.0-alpha.0.1235`. No gate outstanding for Phase 2 entry.
- Last reconciled: 2026-08-28 against `origin/main` `3b7e3e56d`, PR #392, and the Payment proto on main.

## Current state

**Phases 1 and 1b are terminal.** Phase 1b landed on 2026-08-07 via PR #392, not PR #296: the
`Feature/PaymentOwnedResultExpansion` branch merged the `Feature/CommissionBindingDeferredPricing`
work into itself (merge `8e7003de0`) together with the error-convention alignment
(`aa394dd5e refactor(payment): align errors with current convention`), and PR #392 merged the
combined result to `main`. GitHub auto-closed #296 as merged once its commits reached `main`.

On current `origin/main` the Payment proto (`api/Concertable.Payment/src/Concertable.Payment.Client/Protos/payment.proto`)
confirms the Phase 1b shape:

- `ConfirmReviewedGross` is a distinct RPC and `CreateOrBindCommission` carries `reviewed_gross` —
  the sole reviewed-amount commitment boundary.
- `CalculateBoundCommission`, `BoundCommissionManagerPay`, `CreateBoundCommissionHoldSession`,
  `BoundCommissionDeposit`, `BoundCommissionCapture` and the refund requests each
  `reserved "expected_commission_minor", "expected_payer_total_minor"` (and the bind/calc requests
  also reserve `"gross_minor"`) — no post-binding call accepts a caller-supplied commission or total.

**Phase 2 is in progress** on `Feature/launch_platform-commission-phase2`. Step 1 of §10 Phase 2 is
committed: `ISettlementGrossCalculator` — four pure, deal-type-keyed final-gross formulae
(`Concert.Application/Interfaces/ISettlementGrossCalculator.cs` + four leaves in
`Concert.Infrastructure/Services/Settlement/`). The impure `ISettlementAmountResolver` now loads the
eligible takings and delegates the formula to the pure calculator, so `RevenueShareSettlementAmount`
serves both DoorSplit and Guarantee Plus and the redundant `DoorSplitSettlementAmount` /
`VersusSettlementAmount` leaves are gone. Revenue-share multiplication rounds once, half-up, at the
minor unit. 252 Concert unit tests green (17 new in `SettlementGrossCalculatorTests`).

Steps 2–9 remain. No review recorded yet.

## Next Steps

**Continue Phase 2 from step 2 of `PLATFORM_COMMISSION_PLAN.md` §10.**

**Delivery-gate status: none. Phase 2 is directly implementable.** The entire producer surface it
consumes is already published in `Concertable.Payment.Client` at the pinned platform version
(`0.1.0-alpha.0.1235`, `api/Concertable.B2B/Directory.Packages.props`): `ICommissionPricingClient`
(`PreviewAsync` / `CreateOrBindAsync` / `ConfirmReviewedGrossAsync` / `CalculateBoundAsync`),
`IManagerPaymentOperationsClient.PayBoundCommissionAsync` + `CreateBoundCommissionHoldSessionAsync`,
`IEscrowOperationsClient.DepositBoundCommissionAsync` / `CaptureBoundCommissionAsync` /
`RefundBoundCommissionByBookingIdAsync`. B2B production code today calls the legacy non-bound
variants (`PayAsync`, `DepositAsync`, `CaptureAsync`, `CreateHoldSessionAsync`, `RefundByBookingIdAsync`)
and Payment applies the temporary £10 internally — those call sites are what Phase 2 swaps, adding the
binding step at each payer commitment point. §10's "do not compile against unpublished Payment source"
warning does not apply here: the surface is published.

1. ~~Branch a fresh worktree from the current remote default (`main`).~~ Done 2026-08-28.
2. ~~§10 Phase 2 step 1 — four keyed pure gross strategies + exhaustive formula/rounding tests.~~
   Done 2026-08-28 (commit on `Feature/launch_platform-commission-phase2`).
3. **Next: §10 Phase 2 step 2** — persist only `CommissionBindingId` on the application/booking path
   that owns the payer commitment (§3.3); add the frozen `FinalSettlementGrossMinor` snapshot for the
   deferred deals (DoorSplit / Guarantee Plus) per §4.1. Re-scaffold the Concert model via
   `./initial-migrations.ps1` from `api/` (never an additive migration).
4. Then steps 3–6: bind the rate at each payer commitment point and route all four payment journeys
   through the binding-aware Payment methods; exact/deferred pricing DTOs + final takings
   review/attestation + fail-closed error mapping; payer and artist disclosures in the manager SPAs;
   re-scaffold the Concert model.
5. Steps 7–8: local build + focused unit tests, then push the checkpoint for full CI (merge queue
   stays the E2E gate); update this plan and the launch trackers in the implementation commit.
6. Step 9 hard stop: merge and own publish/platform-sync before Phase 3 removes the legacy Payment APIs.

Do not touch Phase 3 (removing the temporary £10 seam) until Phase 2 and its platform sync are green.

## Resume prompt

```
cd C:/Users/tommy/source/repos/Concertable/.worktrees/Feature-launch_platform-commission-phase2
Read @plans/launch/PLATFORM_COMMISSION_PLAN.md and @plans/launch/PLATFORM_COMMISSION_PROGRESS.md and do what its `## Next Steps` says.
```

## Completed work

- **Phase 2 step 1** (2026-08-28, `Feature/launch_platform-commission-phase2`) — `ISettlementGrossCalculator`,
  four pure deal-type-keyed final-gross formulae: FlatFee/VenueHire return the agreed fixed term;
  DoorSplit returns `artistPercent × eligibleTakings`; Guarantee Plus (`Versus`) returns
  `guarantee + artistPercent × eligibleTakings`. Revenue-share term rounds once, half-up, at the minor
  unit. The impure `ISettlementAmountResolver` keeps ownership of loading the takings and delegates the
  formula, so there is one formula home (`RevenueShareSettlementAmount` now serves both revenue-share
  deal types; `DoorSplitSettlementAmount` / `VersusSettlementAmount` deleted). 252 Concert unit tests
  green; 17 new in `SettlementGrossCalculatorTests`.
- **Phase 1** — percentage configuration, immutable SQL configuration history, bindings by
  configuration ID, additive preview/bind/bound-calculation contracts, distinct binding-aware
  money-movement RPCs, transaction tax facts, multi-refund persistence, proportional refund logic,
  migrations. Merged via PR #209 (`Feature/PlatformCommission`).
- **Phase 1b** — reviewed-gross confirmation confined to the `CreateOrBind`/`ConfirmReviewedGross`
  boundary; caller-supplied commission and payer total removed from every later bound calculation
  and money-movement API (Payment derives both from the immutable binding + caller-owned gross);
  review findings OWN1, CV1, BUG1, CV2, TEST1, TEST2, BUG2 resolved. Implementation commits
  `f93aa0c6b`, `e1f4de726`, `e73b30bb4`, `f693c955d` on `Feature/CommissionBindingDeferredPricing`,
  reconciled and error-convention-aligned on `Feature/PaymentOwnedResultExpansion`, merged to `main`
  by PR #392 on 2026-08-07. Breaking Payment package published; generated platform sync migrated the
  B2B and Customer consumers.

## Verification

Phase 2 step 1 (2026-08-28, `Feature/launch_platform-commission-phase2`):

- `dotnet build src/Modules/Concert/Tests/Concertable.B2B.Concert.UnitTests` (from `api/Concertable.B2B`): 0 errors.
- `dotnet test Concertable.B2B.Concert.UnitTests`: 252 passed, 0 failed (was 235; +17 `SettlementGrossCalculatorTests`).
- Full B2B build + carve + integration + merge-queue E2E: deferred to the Phase 2 checkpoint push (step 7).

Phase 1b, from PR #392's merge candidate:

- `dotnet build api/Concertable.slnx --configuration Release --no-restore`: 0 errors.
- Payment unit tests: 219 passed.
- Shared API unit + architecture tests: 52 passed.
- Payment integration tests: 8 passed (SQL Testcontainers).
- Payment standalone package-only carve: 9 deployable-closure projects, 0 errors.
- Full merge-queue E2E ran as the gate (historical `Skip-E2E: true` trailers overridden with
  `full-e2e` before enqueue).

2026-08-28 reconciliation: `origin/main` proto inspection confirms the Phase 1b RPC shape is live;
platform lockstep version `0.1.0-alpha.0.1235` confirms B2B/Customer consume the published surface.

## Reviews

- 2026-08-28 docs reconciliation: `reviews/Docs-platform-commission-1b-reconcile.md` —
  complete, approved, no findings.
- Review artifact: `reviews/Feature-CommissionBindingDeferredPricing.md`.
- OWN1, CV1, BUG1, CV2, TEST1, TEST2, BUG2 — all fixed and closed; no finding reopened after the
  current-main reconciliation, the error-convention alignment on `Feature/PaymentOwnedResultExpansion`,
  or PR #392's own review.
- PR #392 carried its own review and the merge-queue E2E gate.

## Decisions, discoveries, blockers, and deviations

- Phase 1b delivered under PR #392, not #296. The `Feature/CommissionBindingDeferredPricing` branch
  was folded into `Feature/PaymentOwnedResultExpansion` (the typed-result migration) because the two
  touched the same Payment client/error surface and shipping them separately would have forced a
  second breaking package cut-over. #296 is closed-as-merged; #392 is the record.
- The 2026-08-04 "HOLD at the Kernel-convention dependency" is resolved: the natural-case-name error
  convention landed on `main` (`c0b5802b2 feat(kernel): derive published error codes from case names`,
  `eb87a6225 docs(api): codify typed error union conventions`) and the Payment errors were realigned
  in `aa394dd5e` before PR #392 merged.
- Phase 1b changed RPCs that no consumer had adopted yet (Phase 2 is the first consumer), so the
  "consumer migration" in PR #392's platform sync was the typed-result client interface change, not
  the commission binding surface.

## Event log

### 2026-08-28 — Phase 2 step 1: pure keyed settlement-gross calculators

- Action: Added `ISettlementGrossCalculator` (Concert.Application) + four keyed leaves
  (`{FlatFee,VenueHire,DoorSplit,Versus}SettlementGrossCalculator`, revenue-share leaves on a shared
  `RevenueShareSettlementGrossCalculator` base) in `Concert.Infrastructure/Services/Settlement/`.
  Refactored the impure `ISettlementAmountResolver`: `FlatFeeSettlementAmount` /
  `VenueHireSettlementAmount` / `RevenueShareSettlementAmount` now delegate the formula to the pure
  calculator; `RevenueShareSettlementAmount` (formerly abstract) serves both DoorSplit and Versus keys;
  `DoorSplitSettlementAmount` / `VersusSettlementAmount` deleted. Registered the new family with
  `RequireAll<ISettlementGrossCalculator>()`.
- Evidence: `SettlementGrossCalculatorTests` (17 cases: exact, whole-takings, zero-share,
  half-minor-unit round-up, fractional-percentage round-once, additive-not-max for Guarantee Plus).
  Concert unit tests 252 passed. `ConcertDealStrategyFactoryTests` updated for the new family and the
  DoorSplit/Versus → `RevenueShareSettlementAmount` repoint.
- Decision: kept `ISettlementAmountResolver` as the impure orchestration seam (it already encodes
  "load takings or not" per deal type) rather than collapsing it; the pure calculator owns the formula
  so there is no duplicate. Step 2/4 will split "eligible takings" into Concertable sales + declared
  external takings behind that same seam.
- Outcome: step 1 committed. Next: step 2 (persist `CommissionBindingId` + frozen gross snapshot).

### 2026-08-28 — Phase 2 started: worktree created, producer surface verified

- Action: Opened `Feature/launch_platform-commission-phase2` worktree from `origin/main` `10a1aa0bb`.
  Confirmed no open red platform-sync PR. Restored B2B against pin `0.1.0-alpha.0.1235` and reflected
  the published `Concertable.Payment.Client` binding-aware surface.
- Evidence: `ICommissionPricingClient` = `PreviewAsync` / `CreateOrBindAsync(externalReference,
  payerReference, currency, reviewedCommissionConfigurationId, stripePaymentIntentId,
  stripeSetupIntentId)` / `ConfirmReviewedGrossAsync(bindingId, externalReference, payerReference,
  reviewedGross)` / `CalculateBoundAsync(bindingId, externalReference, payerReference, gross,
  stripePaymentIntentId, stripeSetupIntentId)`. `IManagerPaymentOperationsClient.PayBoundCommissionAsync`
  + `CreateBoundCommissionHoldSessionAsync`; `IEscrowOperationsClient.DepositBoundCommissionAsync` /
  `CaptureBoundCommissionAsync` / `RefundBoundCommissionByBookingIdAsync`. No bound variant of
  `ReleaseByBookingIdAsync` — release transfers the stored payee gross (plan §6.2), so no rebind needed.
- Discovery: B2B already has an impure keyed settlement family — `ISettlementAmountResolver`
  (`Concert.Infrastructure/Services/Settlement/`, keyed by `IConcertDealStrategyFactory`), with
  `FlatFeeSettlementAmount` / `VenueHireSettlementAmount` / `DoorSplitSettlementAmount` /
  `VersusSettlementAmount`, the last two on the revenue-loading `RevenueShareSettlementAmount` base.
  Phase 2 step 1 separates the pure formula (`gross = f(deal, eligibleTakings)`) from the takings IO.
- Outcome: Worktree live, ledger reconciled. Next: implement step 1.

### 2026-08-28 — reconciled: Phase 1b is terminal, delivered via PR #392

- Action: Resumed `launch/platform-commission` Phase 1b against a 3-week-stale ledger. Reconciled
  against `origin/main`, PR #296, PR #392, worktrees, the platform version, and the Payment proto.
- Evidence: PR #296 state MERGED (auto-closed); its commits reached `main` via
  `8e7003de0 Merge branch 'Feature/CommissionBindingDeferredPricing' into Feature/PaymentOwnedResultExpansion`
  then `b66325acd Merge pull request #392`. PR #392 MERGED 2026-08-07, body: "Payment now owns
  reviewed-gross confirmation and derives commission and payer totals before bound money movement …
  breaking published-package cutover … generated platform-sync PR must migrate B2B and Customer
  consumers." `payment.proto` on `main` reserves `expected_commission_minor`/`expected_payer_total_minor`
  on all bound calc + money-movement requests; `ConfirmReviewedGross` present.
  `ConcertablePlatformVersion` = `0.1.0-alpha.0.1235`. No active commission worktree.
- Outcome: Phase 1b hard stop checked off in the plan. Ledger and `LAUNCH_ROADMAP.md` item updated —
  Phase 2 is now the active work. No code changed; docs-only reconciliation.
- Follow-up: Phase 2 per `## Next Steps` — new worktree from `main`. Session stops here per the
  resume instruction (do not touch Phase 2 or Phase 3).

### 2026-08-04 — Kernel-convention dependency redefined mid-flight; held at the gate

- Superseded by the 2026-08-28 entry: the convention landed and Phase 1b shipped under PR #392.
- Original: on 2026-08-04 Tommy rejected the `…Case`-suffix + rename-only-factory error-union design
  for natural case names (`ApplicationNotFound`/`PayeeNotFound`/`RecipientUnavailable`) keeping the
  centralized exhaustive `Definition` match with honest case↔Definition agreement. Phase 1b held at
  that gate until the convention merged.
