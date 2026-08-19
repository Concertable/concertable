# Application, Booking, and Concert module ownership progress

- Plan: `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/deal-lifecycle-ownership`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules-phase2`
- Branch: `Refactor/launch_deal-lifecycle-modules-phase2`
- PR: draft whole-refactor PR [#633](https://github.com/Concertable/concertable/pull/633) remains
  published at `0511c35cadca144c7614b27596575eb66692bc62`. Local merge checkpoint
  `922dcf321` and planning checkpoint `0598ccc70` reconcile current `origin/main` plus the settled
  dispatch design. The Workers, Concert unit, Opportunity request-builder, and
  `ApplicationCancelApiTests` frontiers remain cleared.
- Dependency/package gates: blocked on terminal Phase 0-1 delivery of
  `plans/launch/DEAL_CLOSED_SUM_MODEL_PROGRESS.md`. Phase 1 delivery remains terminal; final `api/**`
  package publication and platform sync remain part of PR #633's later delivery lifecycle.
- Last reconciled: 2026-08-19 after suspending PR #633 behind the independently deliverable Deal
  dispatch foundation while preserving its `ApplicationDoorSplitApiTests` continuation

## Current state

Tommy approved the target ownership design on 2026-08-16. The fixed progression is Application →
Booking → Concert for every `DealType`; DealType varies only the local behaviour performed at each
stage. Opportunity remains the upstream one-Deal/many-Applications aggregate.

Application, Booking, and Concert are being separated into independent modules with their own state,
transition model, contextual operation contracts, and provisional module-local selection. There is no
umbrella process entity, shared lifecycle state, workflow module, cross-module resolver, or parent state
machine. A combined status exists only as a read projection.

Phase 1 characterization is complete. Existing integration coverage already pins both
payment/Accept arrival orders, payment failures, pre- and post-Concert cancellation, late-capture
compensation, immutable Contract snapshots, Invoice creation, and duplicate settlement success. The
two missing cases are now covered: payment-webhook redelivery asserts exactly one persisted Concert,
and a failed settlement followed by a successful retry reaches the existing completion outcome. No
new test asserts the legacy shared lifecycle topology, transition table, source layout, or filenames.

Phase 1 characterization merged through PR #625, its package publication succeeded, and platform-sync
PR #630 merged green. PR #633 owns the complete remaining decomposition through the plan's definition
of done. Phases 2-6 are implementation/recovery checkpoints on that one draft PR, not separate merge
candidates.

The published PR #633 frontier has cleared the Opportunity request-builder, Concert unit, Workers unit,
B2B Web, and `ApplicationCancelApiTests` ownership recoveries. The remaining integration compile
frontier continues one fixture at a time from the exact action below. The keyed-selector concern is a
recorded non-blocking follow-up owned by the Deal plan and must not be expanded inside these recovery
slices.

Rejected PR #614 is closed, and its DealTerms branch and worktree were retired with exact-head checks.
The fresh implementation branch contains only current-main Deal vocabulary; none of the rejected
runtime change was carried forward.

Tommy selected the Deal dispatch foundation as the next delivery owner before further lifecycle recovery.
The local PR #633 worktree is preserved at its current committed checkpoint and remains 0 commits behind
the reconciled `origin/main`; its next integration slice is unchanged but suspended. The foundation must
land first so this PR consumes generated net10 factories directly rather than adding temporary keyed or
handwritten dispatch that would be replaced during .NET 11 preparation.

## Next Steps

Blocked: PR #633 is suspended until the Deal dispatch foundation is delivered on `main`.
Blocked by: `plans/launch/DEAL_CLOSED_SUM_MODEL_PROGRESS.md` Phases 0-1.
Unblock action: The Deal owner must deliver the generator/analyzer plus Deal-owned mapper/updater factory foundation through review, CI, merge, package publication, and platform sync, then update this ledger to resume the preserved `ApplicationDoorSplitApiTests` module-owned read/state compile-recovery slice from the current 22-error frontier.
Resume when: Current `main` contains the delivered Deal foundation, its ledger records the Phase 1 delivery lifecycle terminal green, and this branch has merged that exact `origin/main` state.

## Completed work

- Phase 1 PR #625 merged as `4efa1740e0e74601361e4c6595cc1d9d94e1b1bb`; its package
  publication succeeded and platform-sync PR #630 merged green.
- Opened draft whole-refactor PR #633 for Phases 2-6. Its published head
  `0511c35cadca144c7614b27596575eb66692bc62` includes the Opportunity request-builder, Concert unit,
  Workers unit, B2B Web, and `ApplicationCancelApiTests` recovery checkpoints.
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

- Live GitHub state on 2026-08-19: PR #625 is merged; PR #633 is an open draft at
  `0511c35cadca144c7614b27596575eb66692bc62`.
- The published PR #633 ledger records the next isolated `ApplicationDoorSplitApiTests` recovery slice
  from a 22-error Concert integration compile frontier.
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
- Context supplies names inside a module: `State`, `Trigger`, `StateMachine`, and `ICancelStep` do not
  need Application/Booking/Concert prefixes internally.
- The dispatch investigation is concluded. Honest same-interface families use generated module-specific
  invariant strategy factories. Heterogeneous lifecycle operations use dedicated non-generic factories
  returning Dunet implementation unions on net10 and native implementation unions on .NET 11. The
  consumer matches acceptance/confirmation/completion kind, not four Deal cases; deliberate aliases let
  multiple Deals share one implementation. Neither path uses `IKeyedServiceProvider` or a global workflow.
- `IApplicationDealStrategyFactory<TStrategy>` is not reused for `Accept`: an implementation union whose
  cases have different invocations is not a strategy family. The dedicated name is `IAcceptFactory`, not
  `IAcceptStepFactory`.
- PR #633 owns the best-effort net10 heterogeneous-operation conversion after its compile-recovery
  frontier is green, but only after the Deal foundation lands. It consumes the generated
  common-interface and dedicated-operation factory machinery directly. The downstream Deal plan retains
  the later .NET 11 native/closed compiler-enforced cut-over.
- Generic transition plumbing may be shared only when it has no domain knowledge. Strategy
  registrations, transition tables, capabilities, and selector instances remain module-local.
- Application records pre-accept payment evidence only because the callback can arrive before Booking
  exists. The evidence is not a continuation of Application lifecycle state.
- The fixed progression is an invariant to enforce, not an extension point. A `DealType` cannot skip,
  reorder, or merge Application, Booking, and Concert.
- .NET 11 native unions are the selected mechanism for justified closed internal values after the
  module split, including the combined journey projection and module-local state, trigger, or
  operation-outcome shapes with case-specific data. They do not contain DI services, create shared
  lifecycle ownership; persistence maps each module's discriminator explicitly. They do not preserve
  Deal-keyed service resolution.
- Rust is not an implementation option for this lifecycle, Deal behaviour, or settlement work. The
  obsolete Rust engine plan was deleted rather than retained as a paused alternative.
- Opportunity is not hidden inside Application. Its physical extraction is part of the module carve.
- Invoice/settlement records require evidence-based final placement during the Concert carve, but they
  cannot justify a shared lifecycle owner.
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
  resulting closed value shapes and enables module-local dedicated factories to return native
  implementation unions without restoring the rejected god-workflow model.
- Blocking prerequisite owner: `plans/launch/DEAL_CLOSED_SUM_MODEL_PROGRESS.md`.
  Gate: its Phase 0 generator proof and Phase 1 Deal-owned mapper/updater foundation must be terminal on
  `main` before this PR resumes and consumes the generated Application and operation-factory surfaces.
  After PR #633 delivers, the same Deal ledger resumes for the compiler-exhaustive native-union and
  closed-Deal cut-over.
