# Application, Booking, and Concert module ownership progress

- Plan: `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/deal-lifecycle-ownership`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules`
- Branch: `Refactor/launch_deal-lifecycle-modules`
- PR: draft implementation PR [#625](https://github.com/Concertable/concertable/pull/625) at verified work head `3d0fc5a823cad198f8de878aecef5928036f6c5f`; docs-only decision PR #622 merged as `5c33f849444dda60ece44070353716c08819b2d8`; rejected PR #614 is closed and retired
- Dependency/package gates: Phase 1 is implemented. Phase 2 has no external package blocker; its module extraction must preserve current HTTP and package surfaces.
- Last reconciled: 2026-08-17 after current `origin/main` `d5669a836` was merged, the B2B carve rebuilt, and draft PR #625 was verified at work head `3d0fc5a823cad198f8de878aecef5928036f6c5f`

## Current state

Tommy approved the target ownership design on 2026-08-16. The fixed progression is Application →
Booking → Concert for every `DealType`; DealType varies only the local behaviour performed at each
stage. Opportunity remains the upstream one-Deal/many-Applications aggregate.

Application, Booking, and Concert will become independent modules with their own state, transition
model, contextual step contracts, and module-local step resolver. There is no umbrella process entity,
shared lifecycle state, workflow module, cross-module resolver, or parent state machine. A combined
status exists only as a read projection.

Phase 1 is implemented. Exact lifecycle graphs, enum values, executors, payment processors, callbacks,
worker entry points, API/HATEOAS consumers, operation correlation, cancellation and settlement recovery,
Invoice linkage, and Booking-to-Concert creation are executable characterization tests. The reserved
Opportunity, Application, Booking, and Concert namespaces reject direct cross-module runtime/entity
dependencies while permitting Contracts-only collaboration.

Rejected PR #614 is closed, and its DealTerms branch and worktree were retired with exact-head checks.
The fresh implementation branch contains only current-main Deal vocabulary; none of the rejected
runtime change was carried forward.

## Next Steps

1. Push this ledger checkpoint and verify local HEAD, the remote branch, and draft PR #625 are identical.
2. Route that exact committed range through code review and address every high-confidence finding.
3. Record the review and exact-head remote-check evidence in this ledger.
4. Begin Phase 2 by scaffolding the Opportunity, Application, and Booking module project families and
   replacing cross-stage entity navigation with Contracts, owned IDs, and query projections.

## Completed work

- Reconstructed `origin/main` and the rejected aggregate-collapse, premature state-split, and
  Deal-owned workflow attempts.
- Established that the combined `ApplicationEntity.State` is an ownership defect rather than evidence
  for a replacement process aggregate.
- Confirmed from current executors/callbacks that commands consume one lifecycle operation at a time;
  the `IConcertWorkflow` dependency-holder leaks unrelated dependencies.
- Obtained Tommy's explicit decision for independent Application, Booking, and Concert ownership,
  module-local state machines/resolvers, contextual names, and no umbrella parent.
- Replaced the undecided plan with executable phases covering module extraction, state ownership,
  transaction/convergence invariants, local step resolution, projections, and delivery.
- Reconciled the approved decision onto current main, fixed all three docs-review findings, and pushed
  reviewed work head `d06422710a5789cc40ab8817f8ee860f80220eda`; the remote-tracking ref matched exactly.
- Published ledger checkpoint `486ad455bdf2ef4a95034a5401fda0a030f9f7c6`, opened docs-only PR #622,
  and confirmed its PR head and `skip-e2e` label.
- Merged docs decision PR #622 as `5c33f849444dda60ece44070353716c08819b2d8`, closed rejected PR #614,
  and retired its clean worktree/local branch at exact head `ec1dcac897ce5075db83247d05ff694a912f9c43`.
- Ported the useful exact lifecycle topology characterization onto current Deal vocabulary, pinning
  both 19-edge graphs and the FlatFee/VenueHire and DoorSplit/Versus topology pairings.
- Added an executable baseline inventory for every current lifecycle state, trigger, executor, payment
  processor, callback and correlation path, worker, API/HATEOAS consumer, cancellation/settlement
  recovery path, Invoice relation, and guarded Booking-to-Concert creation path.
- Added a reserved lifecycle-module architecture rule that allows Contracts dependencies but rejects
  direct Domain, Application, Infrastructure, and Api references between Opportunity, Application,
  Booking, Deal, and Concert.
- Checkpointed the complete locally verified Phase 1 implementation in this commit.
- Published initial work commit `7898bf8bb83f3dff61686044cd49023ed0afb9fc`, merged current
  `origin/main` as `3d0fc5a823cad198f8de878aecef5928036f6c5f`, then pushed range
  `7898bf8bb..3d0fc5a82` from starting remote head `7898bf8bb`.
- Opened draft PR #625 and verified local HEAD, the remote branch, and PR `headRefOid` all equalled
  `3d0fc5a823cad198f8de878aecef5928036f6c5f` before this ledger checkpoint.

## Verification

- `origin/main` uses one broad `LifecycleState` on Application while Booking and Concert have no
  lifecycle state of their own.
- Public Application mapping already collapses post-accept states back to Accepted, proving those later
  states are not meaningful Application status.
- Concert completion currently reaches backwards through `Concert.Booking.Application.State`, the
  dependency leak the target design removes.
- Accept currently forms Application acceptance, Booking, and Contract under one B2B transaction; the
  plan preserves that invariant across module DbContexts.
- Verify-before-Accept convergence already persists the early payment fact before advancing; the plan
  preserves the join without treating it as one end-to-end state.
- Concert unit suite: 255/255 passed in Release, including the new topology and ownership inventory.
- Targeted B2B module-boundary architecture suite: 7/7 passed in Release.
- Complete architecture suite reached the unrelated current-main Reunion package-ownership guard;
  Conversations projects retain direct package references their source no longer consumes. The
  lifecycle module-boundary tests themselves are green.
- `dotnet build api/Concertable.B2B/src/Concertable.B2B.Web/Concertable.B2B.Web.csproj --configuration Release --no-restore`:
  0 errors and the pre-existing `UserEntity` CS0628 warning after merging the current platform pin.
- Rejected DealTerms implementation vocabulary scan: no matches.
- `python .agents/hooks/plan_graph.py --root .`: 0 errors and 0 warnings.
- `git diff --check`: passed.
- Pre-checkpoint publication: local HEAD, `origin/Refactor/launch_deal-lifecycle-modules`, and draft PR
  #625 `headRefOid` all equalled `3d0fc5a823cad198f8de878aecef5928036f6c5f`.

## Reviews

- Docs review of `89361e99e..d06422710` found three issues: the checkout boundary was ambiguous, the
  typed-result ledger retained a transferred return path, and graph evidence was stale. All were fixed
  in `0bd1d2094`; follow-up review through `d06422710` found no further issues.
- No implementation review exists yet. The rejected PR's prior review is not evidence for this design.

## Decisions, discoveries, blockers, and deviations

- One state machine exists per owning aggregate/module, not per individual enum value.
- Local state machines may use different structures; no common lifecycle interface is required.
- Context supplies names inside a module: `State`, `Trigger`, `StateMachine`, `IStepResolver<TStep>`,
  and `ICancelStep` do not need Application/Booking/Concert prefixes internally.
- Generic keyed-DI or transition plumbing may be shared only when it has no domain knowledge. Strategy
  registrations, transition tables, capabilities, and resolver instances remain module-local.
- Application records pre-accept payment evidence only because the callback can arrive before Booking
  exists. The evidence is not a continuation of Application lifecycle state.
- The fixed progression is an invariant to enforce, not an extension point. A `DealType` cannot skip,
  reorder, or merge Application, Booking, and Concert.
- .NET 11 native unions are the selected mechanism for justified closed internal values after the
  module split, including the combined journey projection and module-local state, trigger, or
  operation-outcome shapes with case-specific data. They do not contain DI services, create shared
  lifecycle ownership, or replace local step resolvers; persistence maps each module's discriminator
  explicitly.
- Rust is not an implementation option for this lifecycle, Deal behaviour, or settlement work. The
  obsolete Rust engine plan was deleted rather than retained as a paused alternative.
- Opportunity is not hidden inside Application. Its physical extraction is part of the module carve.
- Invoice/settlement records require evidence-based final placement during the Concert carve, but they
  cannot justify a shared lifecycle owner.

## Downstream handoffs

- Waiting plan: `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PROGRESS.md`.
  Gate: this lifecycle implementation must land before the .NET 11 plan applies native unions to the
  resulting closed value shapes; it must not union concrete DI step implementations from the rejected
  god-workflow model.
