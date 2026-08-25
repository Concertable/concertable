# Application, Booking, and Concert module ownership progress

- Plan: `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/deal-lifecycle-ownership`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules-phase2`
- Branch: `Refactor/launch_deal-lifecycle-modules-phase2`
- PR: draft [#633](https://github.com/Concertable/concertable/pull/633); GitHub head
  `b47cd214bf1da523b0d8988691885d440d2103d8`. Local head `ae38861a0` is three commits ahead before
  the NAT11 regression/direct seed-state fixture checkpoint recorded in this commit.
- Dependency/package gates: Deal producer PR #678 and platform sync #694 are terminal at
  `Concertable.Platform 0.1.0-alpha.0.1108`. Kernel producer PR #719 and platform sync PR #730 are
  terminal; `Concertable.Kernel 0.1.0-alpha.0.1133` is the published state-machine dependency consumed
  by this branch. No producer gate remains.
- Last reconciled: 2026-08-25 from local Git, GitHub PR #633, the active review work order, and focused
  Application integration verification.

## Current state

PR #633 is the one complete B2B modular-monolith refactor. Opportunity, Application, Booking, and
Concert own their full Api/Application/Domain/Infrastructure/test verticals and retain the fixed forward
authority flow `Opportunity -> Application -> Booking -> Concert`. Deal behaviour varies inside each
stage and does not alter that order. The B2B query-composition modules own cross-stage dashboard reads;
the lifecycle modules do not depend backwards for presentation.

The published Kernel checkpoint is complete. Each lifecycle module owns its state, trigger, immutable
transition table, aggregate operations, and typed operation errors while consuming the exact Result-based
Kernel API. Kernel deliberately references Reunion, and every consumer directly pins Reunion rather than
depending on Kernel's private transitive package reference.

The staged review is complete and its work order is being addressed strictly serially. NAT1-NAT16,
NAT18-NAT21, MB1-MB5, SEED1-SEED4, and CV1-CV8/CV11-CV13 are closed. NAT10's cross-context rollback
regression is committed at `ae38861a0`. NAT11's Application-row serialization fix is committed in the
local ahead range; its deterministic overlap regression and supporting fixture/seed correction are green
and included in this commit. The only open findings are NAT17, MB6, CV9, and CV10.

This commit removes the duplicate integration-test seed snapshot hierarchy.
`Concertable.B2B.IntegrationTests.Fixtures.ApiFixture.SeedState` now exposes the single production
`SeedState` directly, and tests use actual seeded entities for stable identities without calling foreign
domain behaviour or querying foreign module contexts. The Application seed includes a legitimate
in-progress Application; the Application API suite uses the correct tenant header for its multi-tenant
manager. `ConcertAvailabilityEntity` remains an Application-owned persistence projection with misleading
Concert vocabulary; its later internal Infrastructure read-model move is recorded in
`api/Concertable.B2B/src/Modules/Application/TECH_DEBT.md` and is intentionally outside this review fix.

Two same-worktree Claude slices are externally owned and must not be duplicated:

- NAT17 only: durable post-commit notification delivery plus the forced-rollback regression. Claude must
  not edit the review or plan ledger and must not commit or push.
- Seed cleanup only: move the already-singular `SeedState` into the agreed `.Seeding` namespace and update
  the owning seed/integration-test convention. Claude must not recreate snapshots or wrappers and must not
  commit or push.

## Next Steps

Finish NAT17 before advancing the serial review work order. Reconcile Claude's NAT17 and seed-namespace
changes against the committed NAT11/direct-SeedState checkpoint, inspect every changed path,
run the smallest owning builds and focused tests, and commit the coherent verified findings without
sweeping unrelated concurrent edits. Then address MB6, CV9, and CV10 one at a time with one verified local
commit per coherent finding. After the final finding, run incremental review from the recorded watermark,
the plan-required Shared/Kernel and affected B2B closure, architecture/package/carve checks, plan graph,
and `git diff --check`; do not run local E2E. Push one stable candidate and prove local, remote-tracking,
and PR heads match.

## Completed work

- Phase 1 characterization shipped through PR #625 and package/platform sync #630.
- The module carve removed cross-stage EF navigations, established Contracts handoffs, split all four
  module verticals, corrected host/module composition and integration-test topology, regenerated the
  canonical initial migrations, and established mechanical module-boundary guards.
- Deal's validated module-local strategy foundation shipped through PR #678 and platform sync #694.
- Kernel's immutable Result-based state-machine producer shipped through PR #719, published
  `Concertable.Kernel 0.1.0-alpha.0.1133`, and reached main through platform sync PR #730.
- PR #633 adopted module-local state machines and closed the review's lifecycle, transaction,
  cancellation, dashboard, seeding, API, persistence, and architecture findings through NAT11.
- NAT10 committed at `ae38861a0`: Booking confirmation, Concert creation, and their pre-commit work roll
  back together when Booking persistence fails.
- NAT11 this commit: concurrent Application acceptance/payment evidence converges by serializing
  the Application row; the deterministic overlap regression passes in the owning Application suite.

## Verification

- `ApplicationApiTests`: 16/16 passed, including the NAT11 overlap regression and in-progress query case.
- `ConcertFacts_UpdateOwnedAvailabilityProjection`: 1/1 passed after the semantic seed selection fix.
- `python .agents/hooks/plan_graph.py --root <worktree>`: 0 errors, 0 warnings before this compaction.
- `git diff --check`: passed for the current uncommitted candidate.
- No local E2E was run. Full exact-head closure and CI remain outstanding until all review findings close.

## Reviews

- Active work order: `reviews/BIG-Refactor-launch_deal-lifecycle-modules-phase2-Review.md`, fixed-anchor
  review `fb561acee..c50469d48`, security-reviewed through `c50469d48`, with post-anchor incremental review
  through `6ba7a13c5`.
- Open findings in strict order: NAT17, MB6, CV9, CV10. NAT17 is externally delegated in this same
  worktree; no later finding may be committed before its result is reconciled and verified.
- The review artifact remains the merge gate. Run incremental review over all fix commits before final
  delivery and keep the artifact until merge.

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
