# Organiser direct offers progress

- Plan: `plans/launch/BOOKING_ENTRY_DIRECT_OFFERS_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/booking-entry-direct-offers`
- Worktree: none yet — design authored in the normal checkout on `Docs/launch_seal-and-postgres-plans`
- Branch: none yet; implementation opens `Feature/launch_booking-entry-direct-offers` from `origin/main`
- PR: none yet
- Dependency/package gates: **none.** `Concertable.B2B.Deal.Contracts` is `IsPackable` but every consumer
  inside `api/Concertable.B2B/` takes it by `ProjectReference` and nothing outside B2B references it, so
  promoting `FinancialOperation`/`Payer`/`FundsTiming`/`SettlementBasis` into it is **not** a published
  contract change: no producer PR, no `chore/platform-sync-*`. Payment needs no change at all.
- Last reconciled: 2026-09-07 — design pass v3, read directly from `origin/main` at `516f4cc25`.

## Current state

**Design only. No code written, no branch, no worktree.** The plan is complete and internally decided;
the modelling question that stalled v2 is answered.

The recommended model is **(d)**: `ApplicationEntity` becomes the negotiation thread and a new
append-only `ProposalEntity` carries the versions. It nets **−2** nullable fields against today, keeps
both existing unique indexes, needs no new lifecycle state, and satisfies the product doc's versioned
proposals. Plan §5.3 scores it against (a), (b) and (c).

## Completed milestones

- **Design pass v1 (2026-09-07)** — superseded: kept `IApply` keyed by `DealType`, which the standing
  rule lists under **Not a home**.
- **Design pass v2 (2026-09-07)** — superseded: right on the financial reading, wrong on the modelling
  and authored against a stale tree.
- **Design pass v3 (2026-09-07)** — commit `55c95691b` struck v2's §6 back to an open question; this pass
  then answered it. Financial reading re-verified line by line against `origin/main`.

## Latest verification

None executable. The design's factual claims were verified by direct read of `origin/main`, not of the
working checkout — see the first discovery below.

## Reviews

None recorded. A review is required before this plan's delivery PR may merge.

## Decisions, discoveries and blockers

- **Discovery — v2 was authored against the wrong tree.** It cites baseline `516f4cc25` but the checkout
  it was written in is 592 commits behind and predates PR #633, which split `Concert` into `Application`,
  `Booking`, `Opportunity` and `Dashboard` (401 files, −15296/+4087 in that module alone). Every path in
  v2 was stale. **Any future pass must read `origin/main` explicitly** — `git show origin/main:<path>` —
  not the working checkout.
- **Decision — model by role, not by party. This is the whole answer to the nullable objection.** The
  Application persists exactly one signature: the author of the standing proposal. The venue's is built
  at accept time in `AcceptCoreAsync` and persisted on the *Contract*, never on the Application.
  `ArtistESignature` is a role field wearing a party's name — the same error as `RequiresApplyCheckout()`.
  Adding a venue signature beside the artist's models by party and yields v2's five nullables; modelling
  by role and moving both fields onto `ProposalEntity` removes two `null!` members instead.
- **Decision — shape (d), Application-as-thread plus Proposal versions.** (a) is right about the aggregate
  but has nowhere to record a counteroffer, which the product doc requires as versioned proposals. (b) is
  rejected: it discards `(OpportunityId, ArtistId)` unique — the guard that catches an organiser offering
  to an artist who already applied — and pushes nullables onto `BookingEntity`. (c) is conceptually the
  best home for exclusivity but needs a synchronous cross-module write across two `DbContext`s, and
  Opportunity only learns of acceptance asynchronously via `MarkFilled()`.
- **Discovery — no new lifecycle state is needed.** v2 added `Offered` beside `Applied`. Whose turn it is
  is `Proposals[^1].ProposedBy`. The state machine keeps its four transitions and gains one self-loop,
  `Applied --Counter--> Applied`. Adding `Offered` would encode the initiator into the state and then
  multiply with every counter.
- **Verified — the payer-presence rule, mechanically.** `VenueHireConfirmStep` passes
  `PaymentSession.OffSession` with payer `ArtistTenantId`; `FlatFeeConfirmStep` passes payer
  `VenueTenantId` against a held authorization. Payer-first → `DepositEscrow` off-session; payer-last →
  `CaptureEscrow` on-session. Both new offer cells reuse existing operations; no new leaf.
- **Verified — the payer axis already exists.** `IDealPayeeResolver` is two leaves
  (`VenuePaysArtist`, `ArtistPaysVenue`) across four `DealType` keys. `ICommitmentReferenceStep`'s three
  leaves are `FinancialOperation` under other names.
- **Verified — Payment is party-agnostic.** `CaptureEscrowCommand`/`DepositEscrowCommand` take plain
  `Guid PayerId`/`PayeeId`. Zero Payment changes.
- **Discovery — the confirm steps hardcode payer and payee.** This is the one genuine code change the
  offer route forces on Booking: each leaf must read the arm's declared `Payer` instead of a literal
  tenant id. Three leaves stay three leaves.
- **Discovery — three dispatch sites beyond the briefed list.** `ApplicationTermsFingerprint:14`
  (`deal switch` with a throwing `_`; belongs on `DealTerms` beside `Render()`), and the three live
  consumers of the checkout extensions: `ApplicationMappers:28`, `:44` and `OpportunityMapper:80`.
  `SeedState:541` is seed-fixture selection, deliberately left alone.
- **Decision — no keyed union, for a better reason than v2 gave.** Apply and Offer are different *acts*
  (different authority, actor, inputs, preconditions), not two arms of one shared action.
  `KeyedUnionBuilder` keeps its zero production consumers deliberately. No generic-by-profile-type either
  — the product doc rules out `IApply<Artist>` versus `IApply<Venue>` explicitly.
- **Decision — no `OpportunityWorkflow`, on module-boundary grounds.** v2 reached this from the standing
  rule alone and silently contradicted the product doc, which permits creating the private opportunity and
  the initial proposal together. The real reason: separate `DbContext`s and a read-only
  `IOpportunityModule`. The composition is the client's — two calls.
- **Known gap, deliberate.** A retried opportunity-create can leave an orphan `ByInvitationOnly`
  opportunity. It is invisible to discovery and carries no proposals. The proper fix is an idempotency key
  on opportunity creation; named in plan §8 and out of scope for this PR. The *offer* itself is protected
  by a hard unique constraint.
- **Sequencing — `Refactor/launch_operation-claims-and-attempts` is a rebase collision, not a
  dependency.** Correcting v2's ledger: this design consumes **nothing** from `OperationClaim`, so it is
  not blocked and bases cleanly on `origin/main`. But that branch (worktree `513298a1a`, no PR, unmerged)
  edits `ApplicationEntity.cs`, `ApplicationEntityConfiguration.cs` and `ApplicationWorkflow.cs` — the
  three files Phase 3 rewrites hardest. Whichever lands second rebases. It is smaller and already
  implemented, so landing it first is the cheaper order.

## Next Steps

Design is complete. Nothing is blocked; the next action is a scope decision that is the owner's, not the
agent's.

1. Confirm the PR shape. The plan puts all five phases in **one PR**: Phases 1–2 are the `DealType`
   re-keying (behaviour-identical, one route), Phases 3–5 add the second route. Splitting 1–2 out as a
   preparatory PR is viable and would halve the review surface; the plan assumes they are in.
2. Decide the merge order against `Refactor/launch_operation-claims-and-attempts`. It has no PR. Landing
   it first is recommended — see the sequencing note above.
3. On the go-ahead, create `Feature/launch_booking-entry-direct-offers` from `origin/main` and implement
   Phase 1: `Payer`, `FundsTiming`, `SettlementBasis`, `DealProfile`, abstract `DealTerms.Profile`,
   promote `FinancialOperation` into `Concertable.B2B.Deal.Contracts.Enums`, and derive
   `ContractEntity.ExpectedFinancialOperation` from the profile.
4. Keep the commitment-token assertion in `ContractFactory<TTerms>` passing — it is the check that proves
   the profile derivation agrees with the reference vocabulary.
5. Read `origin/main` explicitly when implementing. This checkout is 592 commits behind and its Concert
   module predates the #633 split.
