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
- Phase 2 progress: steps 1–2 (settlement-model foundation) are the draft
  [PR #847](https://github.com/Concertable/concertable/pull/847), review-approved, awaiting merge-queue
  CI. Payment-journey rewiring (step 3+) is a separate PR. Steps 3–9 remain.

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

**Phase 2 is in progress** on `Feature/launch_platform-commission-phase2`. Steps 1–2 of §10 Phase 2
are committed:

- **Step 1** — `ISettlementGrossCalculator`: four pure, deal-type-keyed final-gross formulae
  (`Concert.Application/Interfaces/ISettlementGrossCalculator.cs` + four leaves in
  `Concert.Infrastructure/Services/Settlement/`). The impure `ISettlementAmountResolver` now loads the
  eligible takings and delegates the formula to the pure calculator, so `RevenueShareSettlementAmount`
  serves both DoorSplit and Guarantee Plus and the redundant `DoorSplitSettlementAmount` /
  `VersusSettlementAmount` leaves are gone. Revenue-share multiplication rounds once, half-up, at the
  minor unit.
- **Step 2 — settlement-model foundation** (committed locally, own PR pending):
  - `ApplicationEntity.CommissionBindingId` (`Guid?`) + `BindCommission(Guid)` — idempotent on the
    same binding, rejects a different one or empty (§3.3 "never rebound"); filtered unique index.
  - **Revenue-share settlement extracted to its own aggregate** — `RevenueShareSettlementEntity`
    (table `RevenueShareSettlements`, unique FK to concert, no navigation on `ConcertEntity`). A row
    exists only for a DoorSplit/Guarantee Plus concert that has declared its door take. `DoorRevenue`
    moved OFF `ConcertEntity` (which now has zero settlement fields); `DeclareDoorRevenue` → the
    aggregate's `Declare` / `Redeclare`. `SettlementReview` value object (`Domain/ValueObjects/`,
    `OwnsOne` → `Review_*` columns) for the frozen payer-reviewed gross — one all-or-nothing value,
    cleared by `Redeclare`. `FreezeReviewedGross` modelled, not yet wired (like `BindCommission`).
  - **Manager `settlement` response is a `$type` union** — `ISettlement` = `fixed` | `revenueShare`,
    the latter carrying a nested `ISettlementDeclaration` = `undeclared` | `declared` | `reviewed`.
    Built by a keyed `ISettlementMapper` (`Fixed`/`RevenueShare` leaves, `RequireAll`), reusing the
    existing `IPaymentAmountMapper` for the formula. `MyDetailsResponse` drops `DoorRevenue` for
    `Settlement`; `ManagerConcertDetails` splits owner fields off `ConcertDetails`; one-round-trip
    query projection (`ManagerConcertDetailsProjection` + `QueryableSettlementMappers.ToManagerDetails`).
    Frontend: dead `MyConcert.doorRevenue` field removed (nothing rendered it).
  - Concert model re-scaffolded (`20260828154959_InitialCreate`). 266 Concert unit tests green.

This pulled step 4's read-model shape forward. Call-site wiring (bind at commitment, freeze on
review, worker reads the frozen gross) is step 3.

Steps 3–9 remain. Interactive review with Tommy through the session; a formal review artifact is the
next step before the PR merges.

## Next Steps

**Push the step-2 branch and open its PR, then continue with step 3.**

1. ~~Record a formal review of step 2.~~ Done — `reviews/Feature-launch_platform-commission-phase2.md`,
   approved; TEST1 + DOC1 closed.
2. ~~Open the PR.~~ Done — draft [PR #847](https://github.com/Concertable/concertable/pull/847)
   (`c68878297` + `bfd511848` + `6ae3c5797`). **Next on it:** let merge-queue CI run — the Concert
   integration suite must confirm the `ToManagerDetails` query + `OwnsOne` projection (worktree can't
   run them: `Microsoft.Data.SqlClient.SNI` / Windows MAX_PATH). When exact-head CI is green, mark
   ready and enqueue (`merge`). Then close this worktree and start step 3 from the updated default.
3. **Step 3** — bind the rate at each payer commitment point (§3.2: FlatFee at Confirm & Pay / hold;
   VenueHire at Authorise & Apply / setup; DoorSplit + Guarantee Plus at booking acceptance / setup)
   and route all four payment journeys through the binding-aware Payment methods (`CreateOrBindAsync`
   + `PayBoundCommissionAsync` / `CreateBoundCommissionHoldSessionAsync` / `DepositBoundCommissionAsync`
   / `CaptureBoundCommissionAsync` / `RefundBoundCommissionByBookingIdAsync`). Call sites are in
   `Concert.Infrastructure/Services/Workflow/Steps/` (`HoldCheckoutStep`, `SetupCheckoutStep`,
   `VerifyCheckoutStep`, `CaptureEscrowAcceptStep`, `PayoutFinishStep`, `ReleaseEscrowFinishStep`),
   all calling the legacy non-bound client variants today. Call `application.BindCommission(...)` at
   the commitment point; wire `RevenueShareSettlementEntity.FreezeReviewedGross` at the payer review;
   `PayoutFinishStep` reads `RevenueShareSettlementEntity.Review.GrossMinor` for deferred deals
   instead of recalculating.
4. Then steps 4–6: exact/deferred pricing DTOs + attestation + fail-closed error mapping (the manager
   settlement-view shape is already done); payer + artist disclosures in the manager SPAs (render the
   new `settlement` union); re-scaffold if the model changes again.
5. Steps 7–8: local build + focused unit tests, push for full CI; update this plan + launch trackers.
6. Step 9 hard stop: merge and own publish/platform-sync before Phase 3 removes the legacy Payment APIs.

**Producer surface (unchanged, for step 3):** the binding-aware Payment methods are all published in
`Concertable.Payment.Client` at the pinned `0.1.0-alpha.0.1235`. §10's "do not compile against
unpublished Payment source" warning does not apply — the surface is published.

Do not touch Phase 3 (removing the temporary £10 seam) until Phase 2 and its platform sync are green.

## Resume prompt

```
cd C:/Users/tommy/source/repos/Concertable/.worktrees/Feature-launch_platform-commission-phase2
Read @plans/launch/PLATFORM_COMMISSION_PLAN.md and @plans/launch/PLATFORM_COMMISSION_PROGRESS.md and do what its `## Next Steps` says.
```

## Completed work

- **Phase 2 step 2 — settlement-model foundation** (2026-08-28, `Feature/launch_platform-commission-phase2`,
  own PR pending) — `ApplicationEntity.CommissionBindingId` + `BindCommission(Guid)` (filtered-unique
  index). Revenue-share settlement extracted to `RevenueShareSettlementEntity` (own aggregate/table,
  row only for a declared revenue-share concert); `DoorRevenue` moved off `ConcertEntity`;
  `SettlementReview` value object (`OwnsOne`) for the frozen payer-reviewed gross. Manager `settlement`
  response is a `$type` union (`fixed` | `revenueShare` → `undeclared` | `declared` | `reviewed`) built
  by a keyed `ISettlementMapper`. One-round-trip query projection. Dead `MyConcert.doorRevenue`
  frontend field removed. Concert model re-scaffolded (`20260828154959_InitialCreate`). 266 Concert
  unit tests green.
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

Phase 2 step 2 (2026-08-28, `Feature/launch_platform-commission-phase2`):

- `dotnet test Concertable.B2B.Concert.UnitTests`: 266 passed, 0 failed.
- `dotnet build` of `Concertable.B2B.Concert.Api`, `Concertable.B2B.Concert.IntegrationTests`,
  `src/Concertable.B2B.AppHost`: 0 errors, 0 warnings.
- `./initial-migrations.ps1` re-scaffold: Concert model changed → `20260828154959_InitialCreate`
  (`Applications.CommissionBindingId` + filtered unique index; new `RevenueShareSettlements` table with
  unique FK to `Concerts` + `Review_GrossMinor` / `Review_ReviewedAtUtc` owned-type columns;
  `Concerts.DoorRevenue` dropped); every other module's migration id unchanged.
- **Not verifiable locally:** the Concert integration suite. `Microsoft.Data.SqlClient.SNI` fails to
  load from this worktree (`0x800700CE` — Windows MAX_PATH on the `.worktrees/Feature-launch_...` path),
  so the `ToManagerDetails` EF query translation and the `OwnsOne` projection are **CI-only**. Same
  root cause as the `MSB3030` copy failures on `Concertable.Shared.Notification.Infrastructure` /
  `Concertable.B2B.Conversations.IntegrationTests` in a full `.slnx` build (reproduced with changes
  stashed — not a regression). Merge-queue E2E is the gate.
- Frontend: `MyConcert.doorRevenue` was a declared-but-unrendered type field; removed. `declareDoorRevenue`
  action link + the declare POST flow unchanged. Full web-build gate deferred to the PR.

Phase 2 step 1 (2026-08-28, `Feature/launch_platform-commission-phase2`):

- `dotnet test Concertable.B2B.Concert.UnitTests`: 252 passed, 0 failed (was 235; +17 `SettlementGrossCalculatorTests`).

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

- **Phase 2 step 2** — `reviews/Feature-launch_platform-commission-phase2.md`, complete, **approved**
  up to `559595388`. Interactive review with Tommy across the session drove the design (nullable on
  `ConcertEntity` → extracted aggregate + `SettlementReview` VO + the `$type` declaration union; 3
  subqueries → 1; dead read-context member; `DeferredBooking` guard). The independent pass added two
  findings, both closed: TEST1 (settlement-mapper state coverage — `SettlementMapperTests`), DOC1
  (`CODE_PATTERNS.md` unfiltered-entity roster). CI-only: the `ToManagerDetails` EF translation and the
  `OwnsOne` projection.
- 2026-08-28 docs reconciliation: `reviews/Docs-platform-commission-1b-reconcile.md` —
  complete, approved, no findings.
- Review artifact: `reviews/Feature-CommissionBindingDeferredPricing.md`.
- OWN1, CV1, BUG1, CV2, TEST1, TEST2, BUG2 — all fixed and closed; no finding reopened after the
  current-main reconciliation, the error-convention alignment on `Feature/PaymentOwnedResultExpansion`,
  or PR #392's own review.
- PR #392 carried its own review and the merge-queue E2E gate.

## Decisions, discoveries, blockers, and deviations

- **Step 2 grew and split off its own PR.** Adding the frozen gross as `long? FinalSettlementGrossMinor`
  on `ConcertEntity` was rejected (Tommy): a deal-type-conditional nullable on the concert aggregate
  that "keeps growing" as the commission model adds fields. Resolution: revenue-share settlement is its
  own aggregate (`RevenueShareSettlementEntity`), `DoorRevenue` moved with it, and the manager
  settlement view became a `$type` union — which is step 4's read-model shape, pulled forward because
  you cannot move `DoorRevenue` off the entity without redoing its read path. Net: step 2 ships as its
  own PR (zero published-contract impact), Phase 2 is now two PRs (foundation, then payment rewiring).
- **The Concert integration suite cannot run in this worktree** — `Microsoft.Data.SqlClient.SNI` DLL
  load fails with `0x800700CE` (Windows MAX_PATH; the `.worktrees/Feature-launch_platform-commission-phase2`
  prefix is too deep). The `ToManagerDetails` EF query and `OwnsOne` projection are verified only by
  merge-queue CI. Not a code problem; do not chase it locally.
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

### 2026-08-28 — Phase 2 step 2: settlement-model foundation (own PR)

- Action: `ApplicationEntity.CommissionBindingId` + `BindCommission(Guid)`. Extracted all revenue-share
  settlement data into `RevenueShareSettlementEntity` — own table, unique FK to concert, no navigation
  on `ConcertEntity`; `DoorRevenue` moved off `ConcertEntity` (now zero settlement fields);
  `DeclareDoorRevenue` → `Declare` / `Redeclare` on the aggregate. `SettlementReview` value object
  (`Domain/ValueObjects/`, `OwnsOne` → `Review_GrossMinor` / `Review_ReviewedAtUtc`) for the frozen
  payer-reviewed gross — set or unset as one value. Manager `settlement` response → `ISettlement`
  `$type` union (`fixed` | `revenueShare`, nested `undeclared` | `declared` | `reviewed`), built by a
  keyed `ISettlementMapper` (`RequireAll`), reusing `IPaymentAmountMapper` for the formula.
  `ManagerConcertDetails` splits owner fields off `ConcertDetails`; one-round-trip
  `ManagerConcertDetailsProjection` + `QueryableSettlementMappers.ToManagerDetails`. Removed the dead
  `MyConcert.doorRevenue` frontend field. Re-scaffolded (`20260828154959_InitialCreate`).
- Decision: see "Decisions" — `long? FinalSettlementGrossMinor` on `ConcertEntity` rejected; extracted
  aggregate instead; step 4's read-model shape pulled forward; ships as its own PR.
- In-session review findings all fixed: 3 subqueries → 1; dead read-context member; `Declare` deferred
  -booking guard; response `CanDeclare` + coupled nullables → declaration union; two `ReviewedGross*`
  columns → `SettlementReview` VO.
- Evidence: 266 Concert unit tests. Concert.Api / Concert.IntegrationTests / B2B.AppHost build clean
  (0 warnings). Concert integration suite is CI-only (SqlClient SNI / MAX_PATH in this worktree).
- Outcome: committed locally on `c68878297`+1 (folded the earlier `fbba220f2` — which had the rejected
  `FinalSettlementGrossMinor` approach — into one clean commit via `git reset --soft`). Next: formal
  review artifact, then PR, then step 3.

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
