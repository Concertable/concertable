# Organiser direct offers progress

- Plan: `plans/launch/BOOKING_ENTRY_DIRECT_OFFERS_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/booking-entry-direct-offers`
- Worktree: none yet — design authored in the normal checkout on `Docs/launch_seal-and-postgres-plans`
- Branch: none yet; implementation opens `Feature/launch_booking-entry-direct-offers` from the current
  remote default
- PR: none yet
- Dependency/package gates: **no package gate** — the plan's §11 finding is that no published
  `Concertable.*` contract changes, so no producer PR and no `chore/platform-sync-*`. **Sequencing
  dependency:** the design consumes `OperationClaim` from
  `plans/launch/IDEMPOTENT_EXECUTION_PLAN.md`, whose branch
  `Refactor/launch_operation-claims-and-attempts` is implemented but unmerged. That type lives in
  `Concertable.B2B.DataAccess.Application` by `ProjectReference`, so this work either stacks on that
  branch or starts after it merges.
- Last reconciled: 2026-09-07 — design pass complete against the merged PR #633 baseline
  (`origin/main` at `516f4cc25`); no implementation started.

## Current state

**Design only. No code written, no branch, no worktree.**

The design resolves every question the assignment posed and is recorded in the plan. The load-bearing
decisions, in the order they constrain implementation:

1. `Applied` and `Offered` are the same pending proposal seen from either side, so the second direction
   costs one state and one trigger rather than a parallel pipeline.
2. The offer is an Application row, not an Opportunity concern. Opportunity gains one column
   (`OpportunityAdmission`) and no strategy.
3. No `OpportunityWorkflow`: `ApplicationWorkflow` already depends on `IOpportunityModule` and the
   reverse edge would close a module cycle, while buying no atomicity across three `DbContext`s.
4. Separate endpoints (authorisation, rate limiting and request shape are static per action) over one
   shared creation path.
5. The only capability change is narrowing `IApplyStep` to an artist-side commitment precondition
   invoked at the consent moment rather than at creation.
6. No profile-typed generics and no new union case.
7. The Booking handoff is unchanged, which is what keeps this to one PR.

## Completed milestones

- **Design pass (2026-09-07)** — proposal reconciled against the merged PR #633 tree; plan authored with
  phases, acceptance checks and an explicit out-of-scope list. No commit yet.

## Latest verification

None — nothing executable has been produced.

## Reviews

None recorded. A review is required before this plan's delivery PR may merge.

## Decisions, discoveries and blockers

- **Discovery — venue-hire breaks the naive offer.** `VenueHireApplyStep` validates the *artist* tenant's
  payment method before an application may exist. On a direct offer the artist has done nothing at
  creation time, so that check at that place rejects every venue-hire offer. This is why the precondition
  binds to the artist-side consent moment instead of to creation, and it is the single finding that
  changed the design.
- **Discovery — the union machinery has no production consumer** on the merged baseline; `IDealUnionFactory`
  appears only in its own definition, its registration extension and `DealUnionBuilderTests`. This problem
  does not give it one, so none is added.
- **Decision — a counteroffer may not change `DealType`.** Terms are negotiable, the financial
  arrangement is not. This is what preserves every commitment reference, confirm step, contract factory
  and settlement path unchanged, and it is a product constraint to state rather than hide.
- **Decision — the domain says "organiser" while the organiser resolves to the opportunity's owning venue
  tenant.** Promoter tenants and multiple business profiles are out of scope; naming it now means the
  later work changes a resolver, not every call site.
- **Sequencing — this work follows `Refactor/launch_operation-claims-and-attempts`.** It consumes
  `OperationClaim` for retry-safe offer creation. Not a blocker on design or on local implementation, but
  it decides the base commit.

## Next Steps

Decide the base for implementation, then start Phase 1.

1. Check whether `Refactor/launch_operation-claims-and-attempts` has opened a PR and merged
   (`gh pr list --state all --search "operation-claims"`). PR #633 has merged, so that stack is
   unblocked and should retarget to the default branch.
2. If it has merged, create `Feature/launch_booking-entry-direct-offers` from the current remote default.
   If it has not, stack on `Refactor/launch_operation-claims-and-attempts` and record that base commit
   here.
3. Implement Phase 1 of the plan: `Offered` state, `Counter` trigger, `Initiator`, per-side signature
   slots carrying their signed terms fingerprint, `ProposedDealId`, and `OpportunityAdmission`.
4. Re-scaffold initial migrations via `./initial-migrations.ps1` from `api/` — never an additive
   migration.
5. Run the Application and Opportunity unit suites and confirm `has-pending-model-changes` is clean on
   both contexts before committing.
