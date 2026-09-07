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
- Last reconciled: 2026-09-07 — design revised against the workflow-divergence capability model after
  the first pass was judged against current code rather than the decided target model. Baseline
  `origin/main` at `516f4cc25`; no implementation started.

## Current state

**Design only. No code written, no branch, no worktree.**

The revised design turns on one reading of the current code: `ApplicationCheckoutExtensions`'
`RequiresApplyCheckout() => dealType == DealType.VenueHire` is not a statement about venue hire. Read
against `ApplicationCheckoutService`, it is `Payer == Artist`, hardcoded to the single entry route in
which the artist moves first. The feared entry-route x deal-type matrix is that one coincidence.

Removing it gives a single rule — the payer must be present when their instrument is collected, so a
payer who consents last authorises on-session (`CaptureEscrow`) and a payer who consents first leaves a
method for an off-session move (`DepositEscrow`). That reproduces both shipped journeys and covers both
new cells with the operations that already exist. `CaptureEscrowCommand` and `DepositEscrowCommand` take
plain `PayerId`/`PayeeId` tenant ids, so Payment needs no change.

Entry route therefore enters as one boolean into the `Expected()` derivation, not as a second
registration dimension. No new `FinancialOperation` member, no new confirm or cancel leaf, no union.

## Completed milestones

- **Design pass v1 (2026-09-07)** — reconciled the proposal against the merged PR #633 tree. Superseded:
  it kept `IApply` keyed by `DealType`, which the standing rule lists under **Not a home**.
- **Design pass v2 (2026-09-07)** — rewritten against the workflow-divergence capability model. Adds the
  payer-presence rule, the collapse of the cross matrix, the one-aggregate argument from the filtered
  unique index, and phases that carry migration steps 2-3 as their foundation.

## Latest verification

None — nothing executable has been produced.

## Reviews

None recorded. A review is required before this plan's delivery PR may merge.

## Decisions, discoveries and blockers

- **Discovery — the cross matrix is one hardcoded coincidence.** `RequiresApplyCheckout()` reads as a
  deal-type rule and is actually `Payer == Artist`. Entry route and deal type never multiplied; one
  constant route made them look fused.
- **Discovery — the binding constraint is payer presence, not deal shape.** `AuthorizeAsync` and
  `SetupPaymentMethodAsync` both return a `ClientSecret` the payer's browser confirms, so the payer must
  be at the keyboard. Whether they consent first or last is what selects capture-vs-deposit, and it
  explains today's two shapes rather than encoding them.
- **Discovery — Payment is party-agnostic.** Both escrow commands take plain `PayerId`/`PayeeId` tenant
  ids, so flat fee funded off-session by the venue and hire fee captured on-session from the artist are
  already-served commands. Zero Payment changes, no producer PR, no platform sync.
- **Decision — one aggregate, not two.** `Applications(OpportunityId) WHERE State = Accepted` is a
  filtered unique index and is the only thing making an exclusive slot safe under concurrent acceptance.
  Two tables cannot share it, so a separate invitation entity would turn a database decision into an
  application-level lock.
- **Decision — the offer endpoint takes an existing `opportunityId`.** Application only reads
  Opportunity, matching the `Booking -> Application.Contracts` edge. No `OpportunityWorkflow`: the
  standing rule's **Not a home** list rules out anything spanning stages.
- **Decision — no union.** Countered terms are figures of the deal, so they are a data arm; the standing
  rule keeps `IDealMapper`/`IDealUpdater` on `DealType`. The escalation tier stays empty until a shared
  action needs different client-supplied *behaviour* parameters.
- **Correction carried from v1 — a counteroffer may not change `DealType`.** Terms are negotiable, the
  financial arrangement is not.
- **Sequencing — this work carries migration steps 2-3** (declare `DealProfile`; re-key Application off
  `DealType`) as Phases 1-2. Shipping offers on `DealType` keying would add a behaviour family the
  standing rule forbids and fail its acceptance test 1.
- **Sequencing — follows `Refactor/launch_operation-claims-and-attempts`** for `OperationClaim`. Not a
  design blocker; it decides the base commit.

## Next Steps

Design is complete and awaiting the owner's decision on scope before any branch is created.

1. Confirm with the owner that Phases 1-2 (declare `DealProfile`, re-key Application off `DealType`)
   belong in this PR rather than shipping ahead of it as their own slice. The plan assumes they are in.
2. Check whether `Refactor/launch_operation-claims-and-attempts` has opened a PR and merged; PR #633 has
   merged so that stack is unblocked and should retarget to the default branch. It decides the base
   commit.
3. On the owner's go-ahead, create `Feature/launch_booking-entry-direct-offers` from the resolved base
   and implement Phase 1: `Payer`, `FundsTiming`, `SettlementBasis`, `DealProfile`, abstract
   `DealTerms.Profile`, and `ContractEntity.ExpectedFinancialOperation` computed from the profile.
4. Keep the commitment-token assertion in `ContractFactory<TTerms>` passing — it is the check that proves
   the profile derivation agrees with the reference vocabulary.
