# Percentage platform commission and pricing transparency progress

- Plan: `plans/launch/PLATFORM_COMMISSION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/platform-commission`
- Worktree: none active. Phases 1 and 1b are terminal on `origin/main`. Phase 2 starts a fresh
  worktree from the current remote default.
- Branch: none active (Phase 1b delivered on `Feature/PaymentOwnedResultExpansion` via PR #392).
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

There is no uncommitted work and no active worktree for this plan.

## Next Steps

**Start Phase 2 — B2B gross ownership and percentage cut-over.** This session deliberately stops
here (Phase 1b was the gate; it is already through). Phase 2 is a separate delivery slice:

1. Branch a fresh worktree from the current remote default (`main`).
2. Follow `PLATFORM_COMMISSION_PLAN.md` §10 "Phase 2" steps 1–9: the four keyed pure gross
   strategies + exhaustive formula/rounding tests; persist only `CommissionBindingId` + the frozen
   final-gross snapshot for deferred deals; bind the rate at each payer commitment point and route
   all four payment journeys through the binding-aware Payment methods; exact/deferred pricing DTOs
   + final takings review/attestation + fail-closed error mapping; payer and artist disclosures in
   the manager SPAs; re-scaffold the Concert model; local build + focused unit tests, then push the
   checkpoint for full CI (merge queue stays the E2E gate).
3. Phase 2's own hard stop: merge and own publish/platform-sync before Phase 3 removes the legacy
   Payment APIs.

Do not touch Phase 3 (removing the temporary £10 seam) until Phase 2 and its platform sync are green.

## Resume prompt

```
Read @plans/launch/PLATFORM_COMMISSION_PLAN.md and @plans/launch/PLATFORM_COMMISSION_PROGRESS.md
and do what its `## Next Steps` says (Phase 2 — start a fresh worktree from main).
```

## Completed work

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
