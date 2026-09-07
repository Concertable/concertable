# Organiser direct offers progress

- Plan: `plans/launch/BOOKING_ENTRY_DIRECT_OFFERS_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/booking-entry-direct-offers`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/launch_booking-entry-direct-offers`
- Branch: `Docs/launch_booking-entry-direct-offers`, off `origin/main` at `15ce7946f`. Planning-only, so it
  rides a docs branch; implementation opens `Feature/launch_booking-entry-direct-offers` from the default
  branch once this lands.
- PR: none yet
- Dependency/package gates: **none.** `Concertable.B2B.Deal.Contracts` is `IsPackable` but every consumer
  inside `api/Concertable.B2B/` takes it by `ProjectReference` and nothing outside B2B references it, so
  promoting `FinancialOperation`/`Payer`/`FundsTiming`/`SettlementBasis` into it is **not** a published
  contract change: no producer PR, no `chore/platform-sync-*`. Payment needs no change at all.
- Last reconciled: 2026-09-07 — design pass v3, read directly from `origin/main` at `516f4cc25`.

## Current state

**BLOCKED. Design only, nothing built, and the entry-stage design is disproven.** Plan §0 is the entry
point; do not read §4, §6, §9 or §11 as targets.

Two findings ended this pass. Entry commitment and binding operation are **two** axes, not one —
DoorSplit-offer pairs `MethodVerification` at entry with `VerifyPayment` at binding, while DoorSplit-apply
pairs `None` with the same `VerifyPayment`, so entry is not derivable from binding. And building the two
enum axes that follow would build exactly what the configurable-deal refactor deletes. The owner has
reversed the ordering: configuration first, `DealType` removed entirely, direct offers as its first
consumer.

## Completed milestones

- **Design pass v1–v2 (2026-09-07)** — superseded; v2 was additionally authored against a tree 592
  commits stale.
- **Design pass v3 (2026-09-07)** — re-derived against `origin/main`, answered the modelling question
  with the proposal-thread shape, specified the API surface.
- **v3 partially disproven, same session** — the single-derived-key claim fails on DoorSplit-offer;
  sequencing reversed to configurable-deals-first. Recorded in plan §0.

## Latest verification

None executable. Factual claims verified by direct read of `origin/main`; the disproof in §0.1 is a
cell-by-cell check of the entry/binding pairing, not a test run.

## Reviews

None recorded.

## Decisions, discoveries and blockers

- **Discovery — entry and binding are two axes.** Plan §0.1. The counterexample is DoorSplit-offer: the
  venue is present at the offer and gone by acceptance, so its card must be verified at entry, while the
  binding operation stays `VerifyPayment` exactly as in the apply route. `VerifyPayment` therefore pairs
  with both `None` and `MethodVerification`. Registration grows to roughly 3 + 3, not down to 3.
- **Decision — configurable deals first, `DealType` deleted, offers as its first consumer.** Reverses the
  founder ordering in the entry/union refresh. An interim enum axis would be built to be deleted.
- **Blocker — `KeyedStrategyBuilder<TKey> where TKey : struct, Enum`** cannot express a capability
  registry identity plus version. Configuration-backed selection needs that generic widened; it is
  load-bearing for the replacement plan, not a detail.
- **Owner constraint — Route B skips the opportunity entirely.** Two separate endpoints (`ApplyAsync`,
  `OfferAsync`) sharing `AcceptAsync`; the offer invites a named artist with no opportunity created.
  This rejects plan §5.1's hidden-opportunity approach.
- **Open question that follows, unanswered — what does Booking hang off?** `Application.OpportunityId`
  is non-null and carries both unique indexes. Plan §0.5 lays out the three exits (nullable FK, hidden
  opportunity, extract the slot) and argues the scope objection that killed slot-extraction is much
  weaker inside the configurable refactor. **This is the first question the replacement design must
  answer.**
- **Still standing from v3, as input** — the payer-axis reading (§3, credited to
  `DIVERGENCE_BLIND_DESIGN.md` §1, not this plan's finding); the binding-operation table (§4); the
  proposal-thread model and its scoring of four shapes (§5), which is orthogonal to the disproof; the
  dispatch-site inventory (§7); the routes and action-link table (§9.1–9.3).
- **Payment still needs no change.** `CaptureEscrowCommand`/`DepositEscrowCommand` take plain
  `PayerId`/`PayeeId`. Unaffected by the reversal.

## Next Steps

Blocked: no configurable-deal plan exists, and this work must land as its consumer rather than ahead of it
Blocked by: no owning ledger — the configurable-deal refactor is unplanned; `CONFIGURABLE_DEAL_WORKFLOWS.md` in `Concertable/docs` owns the product scope
Unblock action: author a configurable-deal plan that settles (a) widening `KeyedStrategyBuilder`'s key from `struct, Enum` to a registry identity plus version, (b) whether the bookable slot is extracted from Opportunity, and (c) per-stage capability selection covering entry commitment and binding operation as separate stages — owner is dispatching this to a fresh context
Resume when: that plan exists with (a), (b) and (c) decided, and names direct offers as its consumer

Nothing in this plan may be implemented before then. Phase 1's `DealProfile` is the only piece the
replacement still wants, and only in persisted rather than compile-time form.
