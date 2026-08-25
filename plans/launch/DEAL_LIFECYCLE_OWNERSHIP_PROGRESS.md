# Application, Booking, and Concert module ownership progress

- Plan: `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/deal-lifecycle-ownership`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules-phase2`
- Branch: `Refactor/launch_deal-lifecycle-modules-phase2`
- PR: draft [#633](https://github.com/Concertable/concertable/pull/633). The candidate recorded in this
  commit carries the completed state-machine cutover and the IR1–IR5 fixes. PR/remote head equality is part
  of the final delivery closure and is not asserted here.
- Dependency/package gates: Deal producer PR #678 and platform sync #694 are terminal at
  `Concertable.Platform 0.1.0-alpha.0.1108`. Kernel producer PR #719 published
  `Concertable.Kernel 0.1.0-alpha.0.1133`, and platform sync PR #730 produced the B2B platform pin
  `0.1.0-alpha.0.1158`. B2B consumes the Kernel state machine directly and every consumer directly pins
  `Reunion 0.1.0-alpha.8` rather than relying on Kernel's transitive reference. No producer gate remains.
- Last reconciled: 2026-08-25 from local Git, GitHub PR #633, the active review work order, and focused
  module lifecycle verification.

## Current state

PR #633 is the one complete B2B modular-monolith refactor. Opportunity, Application, Booking, and
Concert own their full Api/Application/Domain/Infrastructure/test verticals and retain the fixed forward
authority flow `Opportunity -> Application -> Booking -> Concert`. Deal behaviour varies inside each
stage and does not alter that order. The B2B query-composition modules own cross-stage dashboard reads;
the lifecycle modules do not depend backwards for presentation.

The state-machine cutover is complete (review finding IR5). Application, Booking, and Concert each own
`Domain/Lifecycle/{State,Trigger,StateMachine}.cs`: a module-local `internal sealed class StateMachine :
IStateMachine<State, Trigger>` backed by the published Kernel `StateMachine<State, Trigger>` frozen table.
Each aggregate holds one static machine and funnels every mutation through a private `Transition(Trigger)`
helper that assigns `State` only from the success value, then mutates auxiliary data and raises events;
a rejected edge leaves state, auxiliary facts, and events untouched. Operation errors carry
`InvalidTransition(TransitionError<State, Trigger>)`. The old combined `LifecycleState`, per-`DealType`
`LifecycleStateMachine`, `IConcertStateMachineRegistry`, and `ILifecycleTransitioner` are gone from source.

The staged review work order is fully addressed: every fixed-anchor finding and every incremental finding
(IR1–IR5) is closed on the branch. No `[ ]` finding remains. `ConcertAvailabilityEntity` naming/layer
placement remains recorded Application technical debt in
`api/Concertable.B2B/src/Modules/Application/TECH_DEBT.md`, deliberately outside this PR's scope.

## Next Steps

Implementation and review are complete; the branch is delivery-gated on final closure being run separately.
Run the plan-required Shared/Kernel and affected B2B build/carve, architecture/package guards, a fresh
incremental review over the IR1–IR5 fix commits from the recorded watermark, `python .agents/hooks/plan_graph.py
--root <worktree>`, and `git diff --check`; do not run local E2E. Then push one stable candidate, prove local,
remote-tracking, and PR heads match, mark PR #633 ready, and follow it through merge-queue E2E, merge,
publication, and platform sync to terminal. Delete this ledger, the plan, and the review artifact in the
closeout once the lifecycle is terminal.

## Completed work

- Phase 1 characterization shipped through PR #625 and package/platform sync #630.
- The module carve removed cross-stage EF navigations, established Contracts handoffs, split all four
  module verticals, corrected host/module composition and integration-test topology, regenerated the
  canonical initial migrations, and established mechanical module-boundary guards.
- Deal's validated module-local strategy foundation shipped through PR #678 and platform sync #694.
- Kernel's immutable Result-based state-machine producer shipped through PR #719, published
  `Concertable.Kernel 0.1.0-alpha.0.1133`, and reached main through platform sync PR #730 at platform pin
  `0.1.0-alpha.0.1158`.
- PR #633 split all four module verticals, then adopted the module-local Kernel state machines (IR5) and
  closed every fixed-anchor and incremental review finding, including NAT17 (durable post-commit Concert
  notification/email), MB6 (Contract suite re-homed to public boundaries), CV9/CV10 (mock-heavy orchestration
  moved out of UnitTests), IR1/IR2 (production message topology), IR3 (cross-venue availability), and IR4
  (serialized Booking financial transitions).

## Verification

- Kernel: 246/246. Application: 18/18. Booking: 13/13. Concert: 91/91. B2B Architecture: 22/22 (includes the
  exhaustive per-module state/trigger tests, the aggregate no-mutation tests, and the
  `LifecycleStateOwnershipTests` assignment guard).
- B2B Web build: 0 warnings / 0 errors.
- B2B's published package closure built in Release with `UseLocalCore=false` and
  `EnforceServiceBoundary=true`: 0 warnings / 0 errors. Direct Kernel/Reunion ownership and the shared
  `0.1.0-alpha.8` Reunion pin were mechanically confirmed.
- Local E2E deliberately not run. Standalone carve, complete integration matrices, and exact-head CI remain
  owned by draft-PR CI; PR/remote head equality remains part of final delivery.

## Reviews

- Work order: `reviews/BIG-Refactor-launch_deal-lifecycle-modules-phase2-Review.md`. Fixed-anchor review
  `fb561acee..c50469d48`, security-reviewed through `c50469d48`; incremental through `b61fc7feb`.
- All findings resolved; no `[ ]` remains. IR2/IR3/IR4 (`d1c5d252b`/`05a685317`/`090308c04`) and the IR5
  cutover landed after `b61fc7feb`; a fresh incremental review over those fix commits is part of final
  closure. Keep the artifact until PR #633 merges, then delete it.

## Decisions, discoveries, blockers, and deviations

- The refactor remains one complete draft PR. Its phases are recovery checkpoints, not independently
  mergeable partial architectures.
- Application acceptance synchronously forms Booking/Contract pre-commit; Booking financial confirmation
  synchronously forms Concert pre-commit. Outbound notification/email effects must remain durable and
  transactionally staged, never escape before commit.
- A module integration project owns only its resource/API and local persistence assertions. Full journeys
  belong in B2B Process tests and cross boundaries through HTTP or Contracts.
- The shared host integration fixture directly reuses the one B2B `SeedState`; namespace separation is
  sufficient. Do not introduce snapshot, source, mirror, adapter, or copied seed-state taxonomies.
- Seed consumers may read foreign seeded entities only for stable identities/expected immutable seed data;
  they may not invoke foreign domain behaviour or query foreign module persistence.
- Runtime orchestration belongs in integration tests. Unit tests retain pure state, value, transition,
  calculation, and other deterministic logic.
- `ConcertAvailabilityEntity` naming/layer placement is accepted only as recorded Application technical
  debt for this PR; do not expand the current review fix into that refactor.
- No local E2E. Exact-head PR/merge-queue CI owns the full E2E tier.

## Downstream handoffs

- `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PROGRESS.md` resumes after this lifecycle refactor lands; it may
  replace justified closed internal values/factory return boundaries with native .NET 11 unions without
  restoring shared lifecycle ownership.
- `plans/launch/DEAL_CLOSED_SUM_MODEL_PROGRESS.md` resumes after PR #633 delivers for its compiler-exhaustive
  native-union/closed-Deal cut-over.
