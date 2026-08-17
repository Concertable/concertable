# Application, Booking, and Concert module ownership progress

- Plan: `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/deal-lifecycle-ownership`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules-phase2`
- Branch: `Refactor/launch_deal-lifecycle-modules-phase2`
- PR: draft Phase 2 PR [#633](https://github.com/Concertable/concertable/pull/633); scaffold head `66284e37a0c9f54a0bbb890f04180fe22f6902c1` is published and the completed navigation cut is ready for its checkpoint commit; Phase 1 PR #625 merged as `4efa1740e0e74601361e4c6595cc1d9d94e1b1bb`
- Dependency/package gates: Phase 1 delivery is terminal. Package publication run `31986741518` succeeded, and platform-sync PR [#630](https://github.com/Concertable/concertable/pull/630) merged green at platform version `0.1.0-alpha.0.1046`. Phase 2 is active from current `origin/main`.
- Last reconciled: 2026-08-17 after completing the Phase 2 navigation cut and local verification

## Current state

Tommy approved the target ownership design on 2026-08-16. The fixed progression is Application →
Booking → Concert for every `DealType`; DealType varies only the local behaviour performed at each
stage. Opportunity remains the upstream one-Deal/many-Applications aggregate.

Application, Booking, and Concert will become independent modules with their own state, transition
model, contextual step contracts, and module-local step resolver. There is no umbrella process entity,
shared lifecycle state, workflow module, cross-module resolver, or parent state machine. A combined
status exists only as a read projection.

Phase 1 characterization is complete. Existing integration coverage already pins both
payment/Accept arrival orders, payment failures, pre- and post-Concert cancellation, late-capture
compensation, immutable Contract snapshots, Invoice creation, and duplicate settlement success. The
two missing cases are now covered: payment-webhook redelivery asserts exactly one persisted Concert,
and a failed settlement followed by a successful retry reaches the existing completion outcome. No
new test asserts the legacy shared lifecycle topology, transition table, source layout, or filenames.

The speculative future-module architecture guard was removed because it matched no types before the
modules existed. Phase 2 now owns the real compile-time project boundaries and ArchUnitNET rules as
the module assemblies are scaffolded.

Rejected PR #614 is closed, and its DealTerms branch and worktree were retired with exact-head checks.
Phase 1 merged through PR #625, published successfully, and reached a green platform sync. Its merged
worktree and local branch were removed through the plan-managed repository command. Phase 2 starts
from `origin/main` at `92ea04166a10843e4db3978211a874e98b44507a`.

The first Phase 2 checkpoint scaffolds the Application, Booking, and Opportunity Contracts, Domain,
Application, Infrastructure, and Api projects; wires them into both solution manifests and the B2B Web
composition closure; and includes the real module namespaces in the positive-result architecture
rules. Immutable `AcceptedApplication`, `ConfirmedBooking`, and `OpportunityDetails` contract records
establish the forward fact shapes, with operation IDs as replay-stable handoff identities. Persistence,
services, API ownership, and the existing public routes remain in Concert until the next cutover slice.

The completed Phase 2 cut removes the Opportunity-to-Application, Application-to-Booking, and
Booking-to-Concert EF navigations without moving Phase 3 state or persistence ownership early.
Booking and Concert retain immutable accepted/confirmed facts, while queries join explicitly through
owned IDs and contracts. The B2B host calls all three new composition roots, and Concert draft
creation explicitly adds the Concert instead of relying on EF graph tracking.

## Next Steps

1. Commit and push the completed Phase 2 navigation cut to draft PR #633, then run the full branch
   review and address every high-confidence finding.
2. Require exact-head draft-PR CI, including the B2B Concert integration shard that local Docker could
   not start, before treating Phase 2 as delivery-ready. Merge remains an explicit delivery step.

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
- Published initial work commit `7898bf8bb83f3dff61686044cd49023ed0afb9fc`, merged current
  `origin/main` as `3d0fc5a823cad198f8de878aecef5928036f6c5f`, then pushed range
  `7898bf8bb..3d0fc5a82` from starting remote head `7898bf8bb`.
- Opened draft PR #625 and verified local HEAD, the remote branch, and PR `headRefOid` all equalled
  `3d0fc5a823cad198f8de878aecef5928036f6c5f` before this ledger checkpoint.
- Removed the exact shared-state topology and source/file inventory additions after recognizing they
  asserted the implementation scheduled for deletion rather than durable system behaviour. This
  correction is recorded in this commit.
- Published correction `cea004a225d04e1ce92abb0eac1b061220e2bdc2`, verified local HEAD, the
  remote branch, and PR #625 `headRefOid` matched, and corrected the draft PR title and description.
- Audited Phase 1 boundary coverage and added only the missing settlement-recovery flow and direct
  exactly-once Concert assertion; the existing coverage already protects the other required outcomes.
- Published Phase 1 range `40cd20957..96dd65989`, including current `origin/main` merge
  `96dd6598979313c40214cbf78c69facd25e4b2e7`; local HEAD, the remote branch, and PR #625
  `headRefOid` matched exactly before this ledger checkpoint.
- Reviewed range `40cd20957..2cc20d1be`; replaced the fixture's hand-written handler scope with
  `IScoped<...>.RunAsync`, then verified and published the resolved review work head.
- Exact checkpoint `f3ebb0fc966a30efab227d16d191cb8d8dcb07a4` passed draft-PR CI run
  `31983097059`, including build, every service carve, unit matrix, integration matrix, and `ci-complete`.

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
- Corrected Concert unit suite: 229/229 passed in Release with the branch restored to current-main
  lifecycle test coverage and no new assertion over `LifecycleState` or its transition table.
- `dotnet build api/Concertable.B2B/src/Concertable.B2B.Web/Concertable.B2B.Web.csproj --configuration Release --no-restore`:
  0 errors and the pre-existing `UserEntity` CS0628 warning after merging the current platform pin.
- Rejected DealTerms implementation vocabulary scan: no matches.
- `python .agents/hooks/plan_graph.py --root .`: 0 errors and 0 warnings.
- `git diff --check`: passed.
- Pre-checkpoint publication: local HEAD, `origin/Refactor/launch_deal-lifecycle-modules`, and draft PR
  #625 `headRefOid` all equalled `3d0fc5a823cad198f8de878aecef5928036f6c5f`.
- Correction publication: local HEAD, `origin/Refactor/launch_deal-lifecycle-modules`, and draft PR
  #625 `headRefOid` all equalled `cea004a225d04e1ce92abb0eac1b061220e2bdc2`.
- `dotnet build api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.IntegrationTests/Concertable.B2B.Concert.IntegrationTests.csproj --configuration Release`:
  0 errors; the three warnings are pre-existing `UserEntity`/`ConcertApiTests` warnings.
- Current candidate `python .agents/hooks/plan_graph.py --root <worktree>`: 0 errors and 0 warnings;
  `git diff --check`: passed.
- After merging current `origin/main`, the affected integration-test project built with 0 errors and
  two pre-existing nullable warnings.
- Phase 1 work-head publication: local HEAD, `origin/Refactor/launch_deal-lifecycle-modules`, and draft
  PR #625 `headRefOid` all equalled `96dd6598979313c40214cbf78c69facd25e4b2e7`.
- Review-fix publication: local HEAD, `origin/Refactor/launch_deal-lifecycle-modules`, and draft PR #625
  `headRefOid` all equalled `2cc20d1be8bc4e7755d5cc4894f11c159dabd6c7`; the affected integration-test
  project rebuilt with 0 errors after the using-based scoped-dispatch cleanup.
- Exact-head draft-PR CI at checkpoint `f3ebb0fc966a30efab227d16d191cb8d8dcb07a4`: green. The B2B
  Concert integration shard passed in 5m03s, the Concert unit shard passed in 1m14s, and `ci-complete`
  passed.
- Phase 1 PR #625 merged as `4efa1740e0e74601361e4c6595cc1d9d94e1b1bb` with `skip-e2e`: no
  positive E2E trigger was present because the diff added internal characterization coverage without
  changing HTTP, cross-service, published-package, auth, or routing behaviour.
- Package publication run `31986741518` succeeded, and platform-sync PR #630 merged green as
  `b0a3c3b42bf2a50b8518364bcd648e193a1bbd01` at `0.1.0-alpha.0.1046`.
- Published Phase 2 scaffold work head `66284e37a0c9f54a0bbb890f04180fe22f6902c1`; local HEAD and
  `origin/Refactor/launch_deal-lifecycle-modules-phase2` matched exactly before opening draft PR #633.
- Removed every cross-stage Opportunity/Application/Booking/Concert EF navigation and replaced the
  affected service, workflow, specification, mapper, dashboard, and repository traversals with owned
  facts or explicit ID-based query shapes.
- Added immutable accepted/confirmed facts to Booking and Concert, re-scaffolded the Concert initial
  migration through the repository helper, and made seed lifecycle links explicit per `SeedState`.
- Added a focused draft-creation unit test that proves Concert persistence no longer depends on the
  removed Booking navigation, and wired the three new module composition roots into the B2B host.
- Phase 2 scaffold: `dotnet build api/Concertable.B2B/src/Concertable.B2B.Web/Concertable.B2B.Web.csproj
  --configuration Release --no-restore --disable-build-servers` passed with 0 warnings and 0 errors.
- Phase 2 module-boundary scope: `dotnet test
  api/Concertable.B2B/tests/Concertable.B2B.ArchitectureTests/Concertable.B2B.ArchitectureTests.csproj
  --configuration Release --no-restore --disable-build-servers --filter
  "FullyQualifiedName~ModuleBoundaryTests"` passed 6/6.
- Completed Phase 2 Concert unit suite: 230/230 passed after the navigation cut and explicit draft
  persistence test.
- Completed Phase 2 B2B Web build: 0 warnings and 0 errors after migration regeneration, seed-link
  cleanup, and composition-root wiring.
- Concert integration tests compile against the navigation-free fixture surface. The runtime
  integration preflight stopped because elevated `docker ps` timed out; exact-head draft-PR CI owns
  the required SQL/Testcontainers execution.
- `git diff --check` passed, and the entity navigation gate finds only Concert-owned
  `ConcertImageEntity.Concert` and Booking-owned `ContractEntity.Booking` relationships.
- Removed the future-module ArchUnitNET rule before delivery because `WithoutRequiringPositiveResults`
  made it vacuous until the module assemblies exist; Phase 2 now owns meaningful boundary enforcement.
- Published corrected work head `1457a2508db5b69d5a0fa7f05eea78ba412edd76`; local HEAD, the remote
  branch, and PR #625 `headRefOid` matched exactly before this review checkpoint.
- Merged current `origin/main` as `ac7e3799e743657697b73c024b9ec75a7a71760b`; the architecture and
  Concert integration-test projects rebuilt with 0 errors, and local, remote, and PR heads matched.
- Incorporated platform-sync #629 as work head `b88e867ab6f2a52d8fcb838d688957450e361820`; both affected
  projects rebuilt with 0 errors and the branch was 0 commits behind `origin/main` before checkpointing.

## Reviews

- Docs review of `89361e99e..d06422710` found three issues: the checkout boundary was ambiguous, the
  typed-result ledger retained a transferred return path, and graph evidence was stale. All were fixed
  in `0bd1d2094`; follow-up review through `d06422710` found no further issues.
- Review `reviews/Refactor-launch_deal-lifecycle-modules.md` covered `40cd20957..1457a2508` across
  correctness, microservice isolation, module boundaries, seeding, C# conventions, and changed-path
  coverage. Its integration-test convention finding and incremental ledger-consistency finding are
  resolved; no open findings remain.

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
- Local Docker did not answer the integration preflight, so no local Testcontainers run was attempted
  after the timeout. This is an environment gate, not evidence of a product failure; PR CI must supply
  the runtime integration result.
- A characterization test must survive the refactor it protects. Types, transition tables, filenames,
  source tokens, and collaborator ownership explicitly scheduled for removal belong in migration
  inventory, never in new regression assertions.

## Migration inventory

- Application commands currently run through Apply, Accept, Reject, Withdraw, and application-cancel
  executors; Accept owns the transaction/outbox boundary, Contract issuance, and early-payment join.
- Booking progression currently spans verification, escrow, settlement, refund, and financial-operation
  callbacks, with operation IDs on Application and settlement correlation through Booking.
- Concert completion runs from `ConcertFinishedFunction`; Concert API actions cover update, post,
  cancellation, door revenue, Contract reads, and Invoice reads.
- Application and Opportunity HATEOAS currently derive checkout and command links from the Concert
  workflow capability registry.
- Booking-to-Concert creation currently runs through the book step and draft service; Invoice currently
  reaches DealType through Concert to Booking to Application. These are migration sites, not target
  ownership or test expectations.

## Downstream handoffs

- Waiting plan: `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PROGRESS.md`.
  Gate: this lifecycle implementation must land before the .NET 11 plan applies native unions to the
  resulting closed value shapes; it must not union concrete DI step implementations from the rejected
  god-workflow model.
