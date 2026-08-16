# Application, Booking, and Concert module ownership progress

- Plan: `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/deal-lifecycle-ownership`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-launch_deal-lifecycle-decision`
- Branch: `Docs/launch_deal-lifecycle-decision`
- PR: decision checkpoint not yet opened; rejected draft implementation PR [#614](https://github.com/Concertable/concertable/pull/614) remains open at remote head `2208702c903dd26a7f43ff554eb955083317b3cf` and must not be continued
- Dependency/package gates: make this approved decision durable on current `main`, then close and retire the rejected PR/branch before creating the fresh implementation worktree
- Last reconciled: 2026-08-16 against `origin/main` `b633d79aa`, rejected PR #614, and the transferred approved planning diff

## Current state

Tommy approved the target ownership design on 2026-08-16. The fixed progression is Application →
Booking → Concert for every `DealType`; DealType varies only the local behaviour performed at each
stage. Opportunity remains the upstream one-Deal/many-Applications aggregate.

Application, Booking, and Concert will become independent modules with their own state, transition
model, contextual step contracts, and module-local step resolver. There is no umbrella process entity,
shared lifecycle state, workflow module, cross-module resolver, or parent state machine. A combined
status exists only as a read projection.

The approved planning state is isolated on a docs-only worktree from current `origin/main`. The
rejected DealTerms implementation remains confined to PR #614 and its old worktree; none of that code
is an approved implementation base. The old worktree is clean after its planning edits were moved to
this branch through a recoverable stash.

## Next Steps

1. Validate the reconciled plan graph and complete a docs review of this decision checkpoint.
2. Commit, push, and land the docs-only checkpoint so the retirement decision is durable on `origin/main`.
3. Close rejected PR #614 and retire its clean worktree with the landed decision commit as evidence.
4. Create `Refactor/launch_deal-lifecycle-modules` from the resulting current `origin/main`, update this
   ledger to that worktree, and execute Phase 1.

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
- `plan_graph.py` passed with 0 errors and 0 warnings before the planning edits.

## Reviews

No implementation review exists. The rejected PR's prior review is not evidence for this design.

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
