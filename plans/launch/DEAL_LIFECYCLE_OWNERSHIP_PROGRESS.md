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
- Last reconciled: 2026-08-29 from local Git, GitHub PR #633, the active review work order, and focused
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

Deal-varying dispatch now has one shared Deal-specific composition layer. `DealStrategyBuilder` composes
the generic keyed-strategy builder and automatically requires full `DealType` coverage for every registered
same-interface family. `DealUnionBuilder<TUnion>` composes the generic keyed-union builder and enforces one
method-header case per DealType. Application Apply and Accept use operation-owned Dunet unions; Accept maps
FlatFee and VenueHire to `IAccept`, DoorSplit and Versus to `IAcceptPaid`, and the consumer matches those
interfaces rather than DealType. The .NET 11 plan records the direct native `union Accept(IAccept,
IAcceptPaid)` replacement, which removes only the Dunet wrappers.

The final security review added IR7-IR10. IR7 is closed: verify-payment handlers now resolve only the
Booking id before entering the repository's serialized financial transition, and deterministic overlap
coverage compiles through the real handler. IR8 is also closed: Accept, Withdraw, and Reject acquire the
same aggregate update lock before lifecycle validation, with deterministic queue-order coverage. IR9-IR10
remain active. The earlier review work order had
every fixed-anchor finding and every incremental finding (IR1–IR6) closed on the branch.
`ConcertAvailabilityEntity` naming/layer
placement remains recorded Application technical debt in
`api/Concertable.B2B/src/Modules/Application/TECH_DEBT.md`, deliberately outside this PR's scope.

## Next Steps

Finish the MM_BOUNDARY_HARDENING pass (see "Boundary hardening" below): land A3's architecture rules once
the Kernel package build blocker clears, get A4's concurrency proof back, finish the Part B sweep write-up,
then decide the Conversations cross-context follow-up (log tech debt or fix now). After that, commit the
module-workflow checkpoint and run the requested Claude CLI incremental review over `12273b558..HEAD`,
addressing every valid finding. Then close IR9-IR10 and run the canonical fresh incremental review over the
fix commits from the recorded watermark, `python .agents/hooks/plan_graph.py --root <worktree>`, and
`git diff --check`; do not run local E2E. Then push one stable candidate, prove local, remote-tracking, and
PR heads match, mark PR #633 ready, and follow it through merge-queue E2E, merge, publication, and platform
sync to terminal. Delete this ledger, the plan, and the review artifact in the closeout once the lifecycle
is terminal.

## Boundary hardening (MM_BOUNDARY_HARDENING_PROMPT.md)

An external audit found the module boundary enforced encapsulation but not direction. Working this to
close before PR #633 leaves draft.

- **A1 (fixed)** — `IOpportunityModule.FillAsync`/`TryFillAsync`/`FillOpportunityError` deleted;
  `IOpportunityModule` is now query-only. The "one Accepted Application per Opportunity" invariant moved
  to a unique filtered index (`Application(OpportunityId) WHERE State = Accepted`); the loser's
  `SaveChangesAsync` duplicate-key conflict maps to `AcceptApplicationError.AlreadyAccepted`
  (`application.accept.duplicate`, replacing the deleted `OpportunityUnavailable`). Opportunity's `Filled`
  is now an ordinary aggregate transition (`MarkFilled`, guarded like `Reopen`) reached asynchronously via
  the new `ApplicationAcceptedEvent` integration event (published by
  `Application.Infrastructure.Events.ApplicationAcceptedDomainEventHandler`, consumed by
  `Opportunity.Infrastructure.Events.ApplicationAcceptedIntegrationEventHandler`), mirroring the existing
  `Reopen` reaction to Booking/Concert cancellation. The plan's Section 8 self-contradiction (synchronous
  claim vs. no-backward-synchronous-calls) is resolved in the plan text itself.
- **A2 (fixed)** — `PaymentVerificationRecordedDomainEvent`/`...Handler` deleted (Application commanding
  Booking directly); `IBookingModule.RecordPaymentVerificationAsync` and its `BookingPaymentVerification`
  family deleted from Booking.Contracts. `ApplicationEntity.RecordPaymentVerification` now raises the
  already-contracted `VerifyPaymentSucceeded`/`VerifyPaymentFailed` directly, which the previously-dead
  `VerifyPaymentSucceededHandler`/`VerifyPaymentFailedHandler` in Booking now actually receive (they were
  registered but the events were never raised before this fix).
- **A3 (blocked on an unrelated build break)** — added to `ModuleBoundaryTests.cs`: a cycle rule
  (`Slices().Matching("Concertable.B2B.(*).").Should().BeFreeOfCycles()`), a lifecycle-direction rule
  (no later stage's namespace calls a non-`Get`-prefixed member of an earlier stage's `I*Module`), and a
  facade query-only rule (every member of `IOpportunityModule`/`IApplicationModule`/`IBookingModule`/
  `IConcertModule` must start with `Get` — true for all four after A1/A2). `LifecycleStateOwnershipTests`'s
  bulk-state-write scan now includes Opportunity (the one violation it would have caught was A1's
  `TryFillAsync`, already deleted). **Cannot yet build-verify**: `Concertable.B2B.Infrastructure` fails
  with `CS0246: IClientContext could not be found` — commit `880cef5ff` moved `IClientContext` into the
  `Concertable.Kernel` package but no locally-cached Kernel package version (checked through
  `0.1.0-alpha.0.1252`) actually contains it. Pre-existing, unrelated to this hardening pass; user is
  looking into the Kernel publish. Once it clears: verify the cycle-rule slice pattern is meaningful, and
  prove both new rules fail-before/pass-after per this PR's own verification convention.
- **A4 (in progress)** — concurrency test for two applications racing to accept on one Opportunity,
  dispatched to a background agent; not yet returned (also blocked on the same Kernel build issue for its
  own verification).
- **Part B sweep** — 14 `I*Module` contracts across B2B (+3 Customer) enumerated; after A1/A2, all four
  lifecycle contracts are query-only, and only 3 of 14 total carry any command member
  (`IAdminModule.EnsureCurrentUserAdminGrantedIfEligibleAsync`, `IConversationsModule.SendAsync`/
  `SendAndNotifyAsync`, `IDealModule.CreateAsync`/`UpdateAsync`/`Validate` — none is a downstream-to-upstream
  lifecycle call; `IDealModule`'s dead `DeleteAsync` was found and deleted). Every registered
  `IDomainEventHandler<T>`/`IIntegrationEventHandler<T>` in B2B has a confirmed live raise/publish site (no
  dead handlers remain; A2 removed the one that was dead). `ApplicationEntity.BeginAcceptance()` (the
  no-arg overload) is production-dead, test-only — minor, not fixed yet. Cross-context transaction
  enlistment: Application's accept transaction enlists Booking's DbContext twice more (Contract/Booking
  formation via `ApplicationAcceptedDomainEvent`, and payment verification via
  `VerifyPaymentSucceeded`/`Failed`) plus Conversations' via `ApplicationNotifier` — all three forward and
  deliberate; the Conversations one is flagged as a plausible future async-conversion candidate, not fixed
  here (see Decisions below). Booking -> Concert confirmation is *not* a cross-context enlistment — it's
  the durable async `BookingConfirmedEvent` path, correcting an earlier wrong claim in this ledger. No
  contract-leaking-internals found across the 14 `I*Module`s. `PayoutAccountEntity.MarkVerified()` (Payment
  service) found production-dead, logged as tech debt there rather than fixed blind. Plan DoD checkboxes
  reconciled for Phases 3/6 against actual code state; the plan's Section 8 contradiction resolved. Not yet
  done: scaffolding-debt project sweep (item 8) beyond a light spot-check, and a full pass over Customer/
  Payment/Auth's own domain-event rosters for item 2 (B2B's is complete).

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
- IR6 completed the production message topology by provisioning the three lifecycle topics and the durable
  Concert-notification command queue in the Aspire composition layer.
- Replaced the four copied Deal strategy builders with the shared generic keyed builder plus the
  Deal-specific `DealStrategyBuilder`; added the generic keyed-union catalog, `DealUnionBuilder<TUnion>`,
  and `IDealUnionFactory<TUnion>`; and moved Application Apply/Accept dispatch out of DealType switches.
- Replaced operation-specific executors and `*Step` contracts with one executable module-local workflow
  per Application, Booking, and Concert. Application retains heterogeneous Apply/Accept union dispatch;
  Booking and Concert retain homogeneous Deal strategy dispatch behind operation-named interfaces.

## Verification

- Kernel: 246/246. Application: 18/18. Booking: 13/13. Concert: 91/91. B2B Architecture: 22/22 (includes the
  exhaustive per-module state/trigger tests, the aggregate no-mutation tests, and the
  `LifecycleStateOwnershipTests` assignment guard).
- B2B Web build: 0 warnings / 0 errors.
- B2B's published package closure built in Release with `UseLocalCore=false` and
  `EnforceServiceBoundary=true`: 0 warnings / 0 errors. Direct Kernel/Reunion ownership and the shared
  `0.1.0-alpha.8` Reunion pin were mechanically confirmed.
- `ServiceTopologyTests`: 7/7 passed with the lifecycle topic and command-queue inventory.
- Current Deal/workflow slice: KeyedStrategies 19/19, Deal 47/47, Application 20/20, Booking 8/8, and
  Concert 96/96. Application, Booking, and Concert Infrastructure builds completed with 0 warnings and
  0 errors. The full B2B solution build completed with 0 errors; its two warnings came from generated
  temporary UI E2E sources. Architecture composition validation passed outside the sandbox, leaving 21/23
  green; the two remaining failures are in unchanged Reunion package-ownership and Venue fixture-boundary
  paths, not this dispatch diff.
- A local Concert integration diagnostic reached 38 passing B2B cases before it was stopped after five
  failures in unchanged HTTP-status and concurrency tests generated nearly 50 MB of captured seed logs. The
  moved Cancel/Complete bodies match `12273b558`; this run is not recorded as a green integration gate.
- Local E2E deliberately not run. Standalone carve, complete integration matrices, and exact-head CI remain
  owned by draft-PR CI; PR/remote head equality remains part of final delivery.

## Reviews

- Work order: `reviews/BIG-Refactor-launch_deal-lifecycle-modules-phase2-Review.md`. Fixed-anchor review
  `fb561acee..c50469d48`, security-reviewed through `c50469d48`; incremental through `b61fc7feb`.
- IR7-IR8 are resolved; IR9-IR10 remain active. IR2/IR3/IR4 (`d1c5d252b`/`05a685317`/`090308c04`), IR5
  (`c61566685`), and the current IR6 topology checkpoint landed after `b61fc7feb`; a fresh incremental review
  over those fix commits is the remaining review gate. Keep the artifact until PR #633 merges, then delete it.

## Decisions, discoveries, blockers, and deviations

- The refactor remains one complete draft PR. Its phases are recovery checkpoints, not independently
  mergeable partial architectures.
- Application acceptance synchronously forms Booking/Contract pre-commit, and the same accept
  transaction also synchronously records `VerifyPaymentSucceeded`/`VerifyPaymentFailed` into Booking's
  financial state via `VerifyPaymentSucceededHandler`/`VerifyPaymentFailedHandler`, and synchronously
  sends the counterparty conversation message via `IConversationsModule.SendAsync`/`SendAndNotifyAsync`
  (`ApplicationNotifier`) -- all three are cross-context enlistments, forward (Application -> Booking,
  Application -> Conversations), all deliberate. Booking's financial confirmation reaching Concert is NOT
  a synchronous pre-commit enlistment -- correcting an earlier wrong claim here -- it is the durable async
  `BookingConfirmedEvent` -> `BookingConfirmedIntegrationEventHandler` integration-event path in Concert's
  own transaction, the same pattern Opportunity's `Filled` reaction now also uses. The Conversations call
  is a plausible future candidate to convert to the same async pattern (Application already has an
  event-driven counterparty-notification path for email via `ApplicationCounterpartyNotifiedDomainEvent`;
  the in-app conversation message uses a different, synchronous mechanism for the same moment) -- not
  fixed here, flagged for a future consistency pass. Outbound notification/email effects must remain
  durable and transactionally staged, never escape before commit.
- A module integration project owns only its resource/API and local persistence assertions. Full journeys
  belong in B2B Process tests and cross boundaries through HTTP or Contracts.
- The shared host integration fixture directly reuses the one B2B `SeedState`; namespace separation is
  sufficient. Do not introduce snapshot, source, mirror, adapter, or copied seed-state taxonomies.
- Seed consumers may read foreign seeded entities only for stable identities/expected immutable seed data;
  they may not invoke foreign domain behaviour or query foreign module persistence.
- Runtime orchestration belongs in integration tests. Unit tests retain pure state, value, transition,
  calculation, and other deterministic logic.
- Generic keyed builders remain business-agnostic. Shared B2B Infrastructure composes them with DealType,
  `IDealStrategy`, exhaustive Deal coverage, and factory registration; module Infrastructure owns only its
  DealType-to-implementation assignments.
- A module workflow groups the named lifecycle operations for one aggregate stage. API entry points begin
  at the module service, while domain-event and background entry points may invoke the workflow directly;
  no workflow spans modules or owns aggregate state.
- `ConcertAvailabilityEntity` naming/layer placement is accepted only as recorded Application technical
  debt for this PR; do not expand the current review fix into that refactor.
- No local E2E. Exact-head PR/merge-queue CI owns the full E2E tier.

## Downstream handoffs

- `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PROGRESS.md` resumes after this lifecycle refactor lands; it may
  replace justified closed internal values/factory return boundaries with native .NET 11 unions without
  restoring shared lifecycle ownership.
- `plans/launch/DEAL_CLOSED_SUM_MODEL_PROGRESS.md` resumes after PR #633 delivers for its compiler-exhaustive
  native-union/closed-Deal cut-over.
