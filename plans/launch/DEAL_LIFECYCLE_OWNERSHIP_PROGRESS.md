# Application, Booking, and Concert module ownership progress

- Plan: `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/deal-lifecycle-ownership`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules-phase2`
- Branch: `Refactor/launch_deal-lifecycle-modules-phase2`
- PR: draft whole-refactor PR [#633](https://github.com/Concertable/concertable/pull/633). Published
  Concert completion compile-recovery work head
  `7b44c105e1f0af9ecc1894e6bc50335a24d156d2` from starting remote head
  `4b5ed8a9aac086e24e0f2c377e8b25f1ec00ae18`;
  local HEAD, the remote branch, and PR `headRefOid` matched exactly after the work-head push.
- Dependency/package gates: none block the remaining B2B-internal implementation. Phase 1 delivery is terminal; final `api/**` delivery will own its routine package publication and platform-sync gate only after the complete refactor merges.
- Last reconciled: 2026-08-18 after clearing the Concert completion executor's Result/lambda
  diagnostics and reducing the compile frontier to `InvoiceRepository`

## Current state

Tommy approved the target ownership design on 2026-08-16. The fixed progression is Application →
Booking → Concert for every `DealType`; DealType varies only the local behaviour performed at each
stage. Opportunity remains the upstream one-Deal/many-Applications aggregate.

Application, Booking, and Concert will become independent modules with their own state, transition
model, contextual step contracts, and module-local keyed selection. There is no umbrella process entity,
shared lifecycle state, workflow module, cross-module resolver, or parent state machine. A combined
status exists only as a read projection.

This is a modular-monolith boundary inside the single B2B deployment, not an independently deployable
service split. Its concrete value is compile-time enforcement of the one-way
Application → Booking → Concert authority flow. Opportunity remains separate and owns its own
Open/Filled state; Applications reference it by ID rather than forming an unbounded aggregate
collection on Opportunity.

PR #633 now owns the complete remaining decomposition through the plan's definition of done. Phases
2-6 are implementation/recovery checkpoints on that one draft PR, not merge candidates. Reviewability,
diff size, and a green intermediate build are not reasons to land a half-owned architecture; a separate
PR is allowed only if a real published-package, deployment, or other external-artifact dependency is
discovered. None is currently present.

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
worktree and local branch were removed through the plan-managed repository command. This branch merged
the current `origin/main` without conflicts as `10140df12`; the merge and verified seeder slice were
published together through work head `ebf259e22` and the branch is 0 commits behind.

The clean branch merged the two newly fetched `origin/main` package-pin commits without conflicts as
`b03309fc5`. It is 0 commits behind, and the merge is published with the Concert creation and
composition-root recovery in `c91ab3886`.

The committed Phase 2 checkpoint removes the Opportunity-to-Application, Application-to-Booking, and
Booking-to-Concert EF navigations and establishes the initial Contracts seams. Published checkpoint
`324186648e0f40b7789b80ca5e3b1ab20dedf8d6` advances the physical carve across 270 paths:

- Opportunity has real Api/Application/Domain/Infrastructure ownership, state, persistence, DI, and
  Venue Contracts composition.
- Application persistence, API, service, local state, keyed acceptance/terms steps, and pre-accept
  verification evidence have moved into Application. Payment callbacks now persist case-specific
  `VerifyPaymentSucceeded` or `VerifyPaymentFailed` facts and dispatch them within the Application
  unit-of-work boundary.
- Booking persistence, API, service, Contract/PDF ownership, local state, keyed confirmation steps,
  and acceptance handler have moved into Booking. Creation requires the immutable
  `AcceptedApplication` handoff, and financial confirmation/failure requires typed evidence correlated
  by Application, expected operation, and provider transaction reference.
- Concert still contains legacy shared lifecycle/workflow/payment/cancellation paths and moved-type
  consumers. The full host/solution is not a valid candidate and has not been verified.

`BookingDto` remains an internal Booking.Application result; deliberate cross-module facts alone belong
in Contracts. Purpose-built mapped read shapes remain projections/snapshots/details, not contexts.

The rejected financial handoff has been removed. There is no `PaymentVerificationOutcome`, combined
nullable outcome event, identifier-only confirmation method, or placeholder
`ApplicationPaymentVerified` vocabulary left in B2B source. Both payment-before-Accept and
Accept-before-payment converge through the same typed evidence boundary, and duplicate delivery is
idempotent.

The broad Phase 3/4 candidate is preserved in published work head `2d14e08db` and checkpoint
`3747fb64f`. It moves Capture/Deposit outcomes and pre-Concert cancellation into Booking, expands the
immutable `ConfirmedBooking` handoff, begins Concert-owned state/steps, and removes the draft/lifecycle
service split. The current candidate adds module-owned Opportunity, Application, Booking, and
Concert seeders over that candidate; the complete PR still has not passed its full focused gate and is
not an approved implementation.

The Concert collaborator boundary is now resolved from the final candidate and repository conventions.
`IConcertService.CreateAsync(ConfirmedBooking)` owns the uniform creation path. Cancel and Complete each
use a separate operation-specific executor backed by module-local keyed selection with exact
`DealType` coverage. There is no umbrella `IConcertExecutor`. The selector mechanism is deliberately
provisional: the existing open-generic factories and `StepResolver<TStep>` lack semantic constraints,
route singleton-only families through scoped keyed-DI lookup, and must be investigated separately
without giving up vertical coverage validation or reintroducing repeated per-facade maps. That
investigation does not block the lifecycle ownership cutover.

The interrupted continuation's 175-path worktree state is now audited. The 89 staged deletions, 7
unstaged deletions, 66 modified paths, and 13 untracked paths form the preserved Phase 3/4 candidate:
Application/Booking ownership, Concert state and operation executors, Opportunity's atomic claim,
host composition, focused tests, and owning tech-debt entries. Within that candidate, the payment
handler rewrites are deliberate bounded implementation: refund success/rejection mutate Concert
cancellation state, and settlement success/failure mutate Concert settlement state inside the owning
inbox/outbox transaction. The only accidental mechanical carry-over found was the deferred-refund
subscription. Its processor interface and method and its module registration were already absent in
the interrupted patch; the remaining B2B host subscription is now removed. No
`RefundEscrowDeferredEvent` reference remains in Concert or the B2B host.

The duplicate Concert `IDealTermsRenderer`, `IDealTermsSerializer`, `IDealTerms`, their implementations,
registrations, and DealTerms-specific unit coverage are removed. The scoped production scan finds no
retained Concert runtime consumer, and Concert.Application now compiles.

The dashboard/projection, module-owned seeder, Application counterparty-notification, and Concert
creation email-composition compile-recovery slices are complete. Opportunity,
Application, Booking, and Concert now seed only their own write tables in order, while the shared
`SeedState` stages generated Application IDs into Booking links and then Concert creation inputs.
Application owns seeded signatures and its internal terms-fingerprint calculator; Concert no longer
registers or tests that collaborator. The three owning Infrastructure projects compile with 0 errors,
and Application now owns its counterparty-notification handler, pre-commit registration, and focused
tests without a post-accept `Cancelled` notification case. Concert creation still consumes only the
immutable `ConfirmedBooking` handoff, and `ConcertService` now depends on the Concert-owned
`IBookingConfirmationEmailSender` application port implemented by Concert Infrastructure. The focused
sender and service tests no longer construct deleted shared-lifecycle or event shapes. No notification,
seeder/fingerprint, dashboard/projection, email-ownership, or stale global-using diagnostic remains in
Concert.

Published checkpoint `c91ab3886` includes the Concert creation email-composition and composition-root
recovery with the current-main merge. Concert no longer registers Application's validator or scans
Opportunity's validators, while `BookingConfirmedDomainEventHandler` is registered through its
pre-commit handler contract. Published completion checkpoint `7b44c105e` supplies the existing
`Result<SettlementOutcome, FinishConcertError>` target to the generic unit-of-work call, clearing all
nine `CompleteExecutor` diagnostics without changing its typed failures, transaction, or module-local
completion-step selection. Concert.Infrastructure now exposes only the deferred cross-module
`InvoiceRepository` query error. The focused Concert unit tests cannot compile until that downstream
production frontier is cleared.

## Next Steps

Fresh-context Concert invoice-query compile-recovery slice only — preserve the checkpointed creation,
composition-root, and completion work and do not continue into migrations, guidance, tests, or another
lifecycle operation:

The keyed-selector design concern is a recorded non-blocking follow-up. Do not refactor, rename, or
generalize selector/factory infrastructure in this slice.

1. Resolve only the `InvoiceRepository.cs(23)` CS1061 diagnostic by replacing its removed
   `ConcertDbContext.Bookings` traversal with the narrow Booking-owned contract or projection required
   by the invoice query. Preserve Invoice ownership and wire behaviour; do not broaden Concert's
   DbContext or add a runtime reference to Booking Infrastructure/Domain.
2. Run a scoped Invoice/Booking ownership grep, `git diff --check`, and the Concert Infrastructure
   Release build with single-worker MSBuild.
3. The slice gate is zero `InvoiceRepository` cross-module-query diagnostics and an exact record of the
   next production compile frontier. Update this ledger with that result and stop the context; do not
   continue into the newly exposed file in the same continuation.

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
- Replaced the rejected flattened verification outcome with immutable success/failure facts, required
  accepted-application provenance, and an explicit Booking financial-evidence boundary.
- Added focused Application and Booking unit projects covering payload validity, provenance,
  correlation rejection, both payment/Accept arrival orders, and duplicate delivery.
- Rewired seed ownership to Booking.Domain and corrected the `ConfirmedBooking` handoff construction;
  the seed project and Application API now compile independently with zero warnings.
- Published implementation checkpoint `324186648e0f40b7789b80ca5e3b1ab20dedf8d6`; local HEAD, the
  remote branch, and draft PR #633 `headRefOid` matched exactly before this ledger commit.
- Published recovery range `b90ecf6db..2d14e08db` from starting remote head
  `b90ecf6dbf5268355381e7f63beacb0a82583f69`; local HEAD, the remote branch, and draft PR
  #633 `headRefOid` all equalled `2d14e08dbcab5d3ff63759781b3573ad17357165`.
- Published current-main and module-owned seeder range `3747fb64f..ebf259e22` from starting remote
  head `3747fb64f90d8189868f37435120daab3bf5ea19`; local HEAD, the remote branch, and draft PR
  #633 `headRefOid` all equalled `ebf259e22e7778b3ffd2944dd83a625724bf811b`.
- Published current-main and Application counterparty-notification range `cb61d8298..af500baa9` from
  starting remote head `cb61d8298567f7d8e07ef028972bd8d4304c4274`. The range merges 15 commits of
  `origin/main` without conflicts as `360da218e`, moves the notification handler, pre-commit
  registration, and focused tests from Concert into Application, and removes the obsolete `Cancelled`
  Application notification copy. Local HEAD, the remote branch, and draft PR #633 `headRefOid` all
  equalled `af500baa92a0d182ff177fa36a4ba061669f3e00` after the work-head push.
- Published current-main, Concert creation email, and composition-root recovery range
  `a86c3e0ea..c91ab3886` from starting remote head
  `a86c3e0ea41bb2a0c258591182cf46c70771d96b`; local HEAD, the remote branch, and draft PR #633
  `headRefOid` all equalled `c91ab3886018f8eb806d276a5b32ef0f4b5c0da2` after the work-head push.
- Published Concert completion compile-recovery range `4b5ed8a9a..7b44c105e` from starting remote
  head `4b5ed8a9aac086e24e0f2c377e8b25f1ec00ae18`; local HEAD, the remote branch, and draft PR #633
  `headRefOid` all equalled `7b44c105e1f0af9ecc1894e6bc50335a24d156d2` after the work-head push.
- Merged the 93-commit `origin/main` drift without conflicts as
  `ff2e4dc553aad7bd9093e958235fa809efe5c881`, then verified local HEAD, the remote branch, and draft PR
  #633 `headRefOid` matched and the branch was 0 commits behind.

## Verification

- Concert composition-root ownership scan finds neither the Application-owned
  `IApplicationValidator` registration nor the Opportunity-owned `OpportunityDtoValidator` scan under
  Concert. Their owning composition roots retain both registrations, and Concert registers
  `BookingConfirmedDomainEventHandler` through
  `IPreCommitDomainEventHandler<BookingConfirmedDomainEvent>`.
- The Concert.Infrastructure single-worker Release build reports 0 warnings and exactly one error:
  CS1061 in `InvoiceRepository` at line 23 for the removed `ConcertDbContext.Bookings` surface. All
  nine `CompleteExecutor` CS8031 diagnostics are cleared.
- Every `ICompleteExecutor`, `CompleteExecutor`, and `ICompleteStep` source reference is Concert-owned;
  the scoped completion scan finds no `IConcertWorkflow`, `ConcertWorkflow`,
  `IConcertStateMachineRegistry`, `LifecycleState`, `IFinishStep`, or `FinishExecutor` match.
- `git diff --check` passed after the completion compile-recovery change.
- Concert creation email ownership grep: every `IBookingConfirmationEmailSender`,
  `BookingConfirmationEmailSender`, and `BookingConfirmationEmailContent` production/test reference is
  under Concert; no Application or Booking runtime dependency was introduced.
- The baseline Concert.Infrastructure Release build reproduced exactly the two recorded
  `BookingConfirmationEmailSender` diagnostics. After the email boundary fix, neither remained; the
  first post-email build exposed the five composition-root diagnostics cleared by the current
  candidate plus the still-current executor and repository frontier.
- The focused sender/service tests were updated to the immutable `ConfirmedBooking` handoff but could
  not execute because the downstream Concert.Infrastructure production errors prevent the unit-test
  project from compiling.
- `git diff --check` passed for the email-composition slice.
- Application counterparty-notification ownership grep: every
  `ApplicationCounterpartyNotifiedDomainEvent` and `ApplicationNotification` production/test reference
  is under Application; Concert has no match.
- Application unit tests passed 11/11 in Release after the ownership move; Application.Infrastructure
  built with 0 warnings and 0 errors using `--no-restore`, disabled build servers, and single-worker
  MSBuild.
- Concert.Infrastructure now stops at exactly 2 errors with 0 warnings, both unresolved
  `BookingConfirmationEmailSender` symbols in `ConcertService`; all three former Application
  counterparty-notification diagnostics are gone.
- `git diff --check` passed for the notification slice.
- Dead Concert terms-rendering slice: the scoped production-reference scan across Concert and the B2B
  host returned no `IDealTermsRenderer`, `IDealTermsSerializer`, `IDealTerms`, renderer/serializer
  implementation, or per-`DealType` terms implementation match; `git diff --check` passed.
- Dashboard/projection rejected-boundary scan across `ConcertDashboardService`,
  `ConcertDashboardRepository`, both dashboard mappers, and Concert.Infrastructure global usings:
  no `IConcertWorkflowCapabilityRegistry`, deleted workflow-executor namespace, `LifecycleState`,
  `ApplicationEntity`, or `OpportunityEntity` match.
- Application.Infrastructure and Opportunity.Infrastructure Release builds with `--no-restore`,
  disabled build servers, and single-worker MSBuild: 0 warnings and 0 errors.
- Module-owned seeder grep: every `Opportunities`, `Applications`, `Bookings`, and `Concerts`
  `AddRange` is now in its owning module seeder; Concert has no cross-module seeder write and no
  `ITermsFingerprintCalculator` or `SeededApplicationSigner` reference.
- Opportunity.Infrastructure, Application.Infrastructure, and Booking.Infrastructure Release builds
  with `--no-restore`, disabled build servers, and single-worker MSBuild passed with 0 errors.
  Application and Booking had 0 warnings; Opportunity reported only the inherited `UserEntity`
  CS0628 warning from a referenced project.
- `dotnet build api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Concertable.B2B.Concert.Infrastructure.csproj --configuration Release --no-restore --disable-build-servers --maxcpucount:1`
  stopped at 5 errors with 0 warnings: three `ApplicationCounterpartyNotifiedDomainEvent` /
  `ApplicationNotification` diagnostics and two `BookingConfirmationEmailSender` diagnostics.
- Refund recovery slice: `git diff --check` passed, and the scoped
  `RefundEscrowDeferredEvent` scan across Concert plus the B2B host returned no matches.
- The published 270-path candidate remains an intentionally non-mergeable carve. Its Application-to-
  Booking seam is verified; the host and architecture suite stop at 37 Concert.Application compile
  errors naming legacy shared-workflow consumers of types already moved to their owning modules.
- 2026-08-17 implementation publication: local HEAD, remote branch, and PR head matched
  `324186648e0f40b7789b80ca5e3b1ab20dedf8d6`; the branch remained 89 commits behind `origin/main`.
- Delivery-model reconciliation: `python .agents/hooks/plan_graph.py --root .` reports 0 errors and 0
  warnings; `git diff --check` passes.
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
- 2026-08-17 focused seam gate: Application unit tests 5/5 and Booking unit tests 12/12 passed in
  Release; Application Infrastructure, Application API, Booking Infrastructure, and seed
  Infrastructure builds passed with 0 warnings and 0 errors.
- Rejected-boundary vocabulary scan across `api/Concertable.B2B/src`: no
  `PaymentVerificationOutcome`, `PaymentVerificationRecordedDomainEvent`, identifier-only
  `ConfirmAsync(bookingId)`, `RecordFinancialFailureAsync`, or `ApplicationPaymentVerified` remains.
- Current candidate `git diff --check` passes and the plan graph reports 0 errors and 0 warnings.
- After the current-main merge, Application unit tests remain 5/5, Booking unit tests remain 12/12,
  and the seed Infrastructure build remains green with 0 warnings and 0 errors.
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
- Review fix `4b1752304598997ee43c7538daf2f8251a21d41d` preserves the Booking subtype's immutable
  door-revenue requirement in the forward handoff and Concert, removing three duplicated `DealType`
  branches from agnostic Concert code. The initial migration was regenerated through the repository
  helper.
- Post-review verification passed: B2B Web build with 0 warnings and 0 errors, Concert unit suite
  230/230, module-boundary tests 6/6, plan graph 0 errors and 0 warnings, and `git diff --check`.
- Integrated current `origin/main` through `b6ba59f3f4eae7be6b15117416fca50ff2b5cbc5`, composing the
  organization-route tenant-ID changes and repository-permission contraction with the navigation-free
  query shapes. The current-main host build passed with 0 errors and one inherited `UserEntity`
  warning; Concert units passed 230/230, module boundaries passed 6/6, and the Concert integration
  project compiled with 0 errors and its two existing nullable warnings.
- Published reviewed Phase 2 range `f563d9b02..9ec087d6f` from starting remote head
  `f563d9b022477cdc2ce28d4e6fc6290995717728`; local HEAD, the remote branch, and PR #633
  `headRefOid` all equalled `9ec087d6f07a6bbb3c248a7aefbcc829f8c906ab`.
- Exact-head CI run `32035899299` failed the solution build at checkpoint
  `4315e80c80b92a82a58bf0e0e9d5acbad6392af6` because two B2B API E2E tests still read the removed
  `BookingEntity.Concert` navigation. Fix `b3b6b071156d1231df10060462fec6c677cc65de` replaces those
  references and the matching UI E2E step with `SeedState.ConcertFor`; no Booking-to-Concert test
  navigation remains.
- Published reviewed CI-fix range `4315e80c8..7fbecfb31`; local HEAD, the remote branch, and PR #633
  `headRefOid` all equalled `7fbecfb3162b3da98b11932f4fe3cee51e165476`.
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
- Review `reviews/Refactor-launch_deal-lifecycle-modules-phase2.md` covered
  current-base range `2cfbce326..3a83e68d3` across correctness, Contracts security, current-main
  integration, microservice isolation, module boundaries, seeding, C# conventions, and changed-path
  coverage. Its duplicated `DealType` rule, project-file whitespace, and missed E2E consumer findings
  are resolved; no open findings remain.
- That review closes only the Phase 2 checkpoint. The complete PR requires a new full review after the
  remaining ownership, state, workflow, projection, and documentation work is implemented.

## Decisions, discoveries, blockers, and deviations

- The remaining refactor is one complete PR. Phases are in-branch checkpoints; only a genuine external
  artifact dependency can justify another merge.
- Opportunity, Application, Booking, and Concert remain separate modules inside one B2B deployable.
  Independent deployment is irrelevant; the boundary exists to enforce one-way state ownership.
- Opportunity does not own an `Applications` collection. Application has an independent identity and
  lifecycle and is queried by `OpportunityId` through its owning module.
- Application acceptance to Booking/Contract formation remains a synchronous pre-commit domain-event
  handoff inside one ambient SQL transaction. An asynchronous outbox handoff is rejected because it
  would permit Accepted-without-Booking and require pending state, reconciliation, and compensation.
- Acceptance must atomically claim Opportunity `Open` to `Filled`; the current non-conditional,
  unused `MarkFilledAsync` path does not prevent two concurrent Applications from winning.
- `ConfirmedBooking` is the only Booking-to-Concert creation input. Concert must not reload live
  upstream aggregates.
- The Concert collaborator boundary is resolved: uniform creation remains on `IConcertService`, while
  Cancel and Complete each have one operation-specific executor backed by the module-local keyed step
  factory. There is no umbrella `IConcertExecutor`, `ConcertCreator`, or separate `ConcertDraftService`.
- `BookingConfirmedDomainEventHandler` remains a thin event adapter. It calls uniform
  `IConcertService.CreateAsync(ConfirmedBooking)`; separate Cancel and
  Complete executors hide their own deal-specific step selection. Creation has no expected
  caller-actionable failure after Application genre validation and Booking confirmation; missing or
  mismatched local projections remain invariant failures.
- HTTP hosts compose only `AddXApi(configuration)` boundaries; each API extension composes its own
  module infrastructure. The analogous Customer host violations are recorded in
  `api/Concertable.Customer/TECH_DEBT.md` for later correction and an architecture guard.
- Pre-accept withdrawal remains Application-owned. Post-accept/pre-Concert cancellation is
  Booking-owned and belongs on `BookingController`; compatibility routing may preserve the existing
  Application cancellation URL without moving state ownership backwards. Post-creation cancellation
  is Concert-owned.
- Concert-owned Artist and Venue read-model repositories read local projections needed for creation;
  they are distinct from the Concert aggregate repository and do not authorize cross-module queries.
- Expected application failures use Reunion Results. Explicit exceptions are reserved for genuine
  infrastructure faults or impossible invariant violations, not ordinary not-found/state/validation
  outcomes.
- Empty layer projects and no-op `Add*Module` methods cannot survive delivery. Add a layer only when the
  module has real content for it.
- DTO placement follows audience: internal service results belong in Application; only deliberate
  cross-module shapes belong in Contracts. Purpose-built mapped read shapes are projections/snapshots,
  not contexts.
- One state machine exists per owning aggregate/module, not per individual enum value.
- Local state machines may use different structures; no common lifecycle interface is required.
- Context supplies names inside a module: `State`, `Trigger`, `StateMachine`, and `ICancelStep` do not
  need Application/Booking/Concert prefixes internally.
- The existing open-generic strategy factories and `StepResolver<TStep>` are not the approved final
  dispatch pattern. They lack strategy-family and caller invariants, add scoped keyed-container lookup
  to singleton-only dispatch, and repeat the same service-locator wrapper across modules. A separate
  non-blocking investigation must preserve vertical exact-coverage validation while designing closed,
  operation-owned, lifetime-aware dispatch with immutable singleton maps and no repeated per-facade
  key declarations. Do not expand this concern inside the lifecycle compile-recovery slices.
- Generic transition plumbing may be shared only when it has no domain knowledge. Strategy
  registrations, transition tables, capabilities, and selector instances remain module-local.
- Application records pre-accept payment evidence only because the callback can arrive before Booking
  exists. The evidence is not a continuation of Application lifecycle state.
- `Application.Contracts` supplies the immutable accepted-application provenance required to create a
  Booking. Booking cannot be created from an arbitrary Application ID.
- Booking confirmation requires an explicit successful financial-operation fact correlated to the
  accepted Application, expected operation, and provider transaction. `ConfirmAsync(bookingId)` is a
  rejected design; confirmation also must not load or accept a live Application aggregate.
- Success and failure are separate facts with case-specific required data. An outcome enum/boolean plus
  nullable failure code/message is rejected. The established payment-operation vocabulary is expressed
  as `VerifyPaymentSucceeded` and `VerifyPaymentFailed`; `ApplicationPaymentVerified` is not used.
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
- Committed Booking-to-Concert creation still carries legacy book-step/draft-service history, while the
  uncommitted candidate removes `ConcertDraftService` and calls uniform creation from
  `BookingConfirmedDomainEventHandler`. Cancel and Complete must be extracted from the candidate's
  dependency-heavy `ConcertService` into their separate operation-specific executors.

## Downstream handoffs

- Waiting plan: `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PROGRESS.md`.
  Gate: this lifecycle implementation must land before the .NET 11 plan applies native unions to the
  resulting closed value shapes; it must not union concrete DI step implementations from the rejected
  god-workflow model.
