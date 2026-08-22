# Application, Booking, and Concert module ownership progress

- Plan: `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/deal-lifecycle-ownership`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules-phase2`
- Branch: `Refactor/launch_deal-lifecycle-modules-phase2`
- PR: draft whole-refactor PR [#633](https://github.com/Concertable/concertable/pull/633) is published
  through migration-closure work head
  `7eb5e7a88db900621c637c5302abe6232031c157`. Starting remote head for that checkpoint was
  `5b18da56f7444d2d2ee6d94c3a594b20751274ef`; local, remote-tracking, and PR heads were verified equal.
  This ledger commit is the checkpoint-transport leg.
- Dependency/package gates: the Deal dispatch foundation is terminal. PR #678 merged as
  `1e26f824472fb5329e22eaca8ecd53cab49c1e86`; package publication succeeded; platform-sync PR #694
  merged green as `d0b8f616fc95052629fc745d9b24fdcfc05a6167` at `0.1.0-alpha.0.1108`. The
  additive Kernel state-machine API was delivered by producer PR
  [#719](https://github.com/Concertable/concertable/pull/719) from exact source head
  `541ff90f5b3910815c5835c38c03a52dc47434f4`, merged as
  `a1ec9fa2e690aa9c1df5c467cd742481514e28c7` after successful merge-group run `32541628076`.
  Publish-packages run `32543949659` published `Concertable.Kernel` version
  `0.1.0-alpha.0.1133`; generated platform-sync PR
  [#730](https://github.com/Concertable/concertable/pull/730) merged green as
  `067438ccf8442e10aa05fa4b8f40d0b045c0aaf1`. The Phase 5 consumer gate is clear.
- Last reconciled: 2026-08-22 after resolving and validating the dashboard landing from `origin/main`
  at `223e20a75`; `origin/main` subsequently advanced to `26645ecd1` and is the next reconciliation checkpoint

## Current state

Tommy approved the target ownership design on 2026-08-16. The fixed progression is Application →
Booking → Concert for every `DealType`; DealType varies only the local behaviour performed at each
stage. Opportunity remains the upstream one-Deal/many-Applications aggregate.

Application, Booking, and Concert will become independent modules with their own state, transition
model, contextual step contracts, and module-local keyed selection. There is no umbrella process entity,
shared lifecycle state, workflow module, cross-module resolver, or parent state machine. A combined
status exists only as a read projection.

The transition mechanism is now resolved. `Concertable.Kernel` owns one stateless immutable
`IStateMachine<TState, TTrigger>` backed by a frozen transition table. Its `Transition` method returns
`Result<TState, TransitionError<TState, TTrigger>>`: success carries the real next state and rejection
carries the common parameterized machine error. There is no `Try`/`out` API, fabricated default state,
extra outcome generic, mutable configuration, persistence, events, callbacks, DI, or module knowledge.

Application, Booking, and Concert each own their configured machine, state, trigger, and semantic
aggregate operations. Aggregates privately observe the transition Result, mutate themselves only from
its success value, and raise their own domain events. Application and Infrastructure never calculate or
assign the next state. Operations return the common transition error directly when it is their complete
failure contract; otherwise they compose it into a closed operation-owned error union. Shared error/state
inheritance, open error catalogs, and `Result<T, IError>` are rejected.

Kernel references Reunion deliberately because Result is part of this pure domain API. Producer PR #719
delivered the fixed surface and frozen lookup, pins Reunion in the Shared closure, and keeps Kernel's
Reunion reference private in package metadata so no consumer can rely on it transitively. Its focused
Release suite, package metadata gate, exact-head CI, review, merge, publication, and platform sync are
green at published version `0.1.0-alpha.0.1133`. The B2B machine adoption stays on PR #633. Payment
PR #707 may reuse only the immutable edge lookup while retaining its provider-specific validation and
transition semantics. Moving the abstraction into the Reunion repository is rejected; a standalone
state-machine NuGet remains only a possible later extraction after the API is proven in both consumers.

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
use a separate operation-specific executor. Their current keyed selectors remain provisional for this
delivery, but the downstream decision is settled: cancellation becomes a direct refund collaborator;
completion retains a named facade over validated keyed release/payout implementations. The shared
factory/builder must restrict declared families and preserve exact vertical coverage, lifetimes, and
selected-only construction. This follow-up does not block the lifecycle ownership cutover.

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
completion-step selection. The prior Concert.Infrastructure build exposed only the deferred cross-module
`InvoiceRepository` query error. The Invoice repository now resolves application-based reads through
the Concert-owned `ApplicationId` and `BookingId` facts delivered by `ConfirmedBooking`; no Booking
runtime query or new facade dependency is required. Concert.Infrastructure compiles with 0 errors.

The Concert unit-test ownership recovery is complete in this commit. Tests of the deleted umbrella
lifecycle, workflow, cross-stage executors, and Application/Booking-owned collaborators were removed
from the Concert project. Surviving Concert tests now construct the immutable `ConfirmedBooking`
contract directly, validate only Concert-owned state and persistence, and retain the draft-creation
regression against `ConcertService.CreateAsync`. The scoped stale-ownership vocabulary scan is empty,
the project builds with 0 warnings and 0 errors, and the Release suite passes 88/88.

Published work head `d1703c218` clears all four host-facing errors from exact-head CI run
`32192242931`. Application.Api resolves its moved request type, Concert.Api no longer imports the
deleted workflow namespace or injects Booking's internal Contract service, and the existing
`/api/concert/{id}/contract/pdf` route now resolves the document through the forward
`Concert -> Booking.Contracts` facade. The host composes moved modules and their dev seeders through
their API extensions without direct references to their Infrastructure namespaces. The repeated B2B
Web Release build passes with 0 warnings and 0 errors.

The current checkpoint moves the Workers completion-runner fixture from the deleted
`IFinishExecutor`/`FinishAsync`/`GetEndedConfirmedIdsAsync` vocabulary to `ICompleteExecutor`,
`CompleteAsync`, and `GetEndedPendingCompletionIdsAsync`. Opportunity, Application, Booking, and
Concert now register their module-owned dev seeders inside `AddXApi`; the Web host no longer composes
those seeders separately. The focused Workers suite passes 5/5 and the B2B Web Release build passes
with 0 warnings and 0 errors.

Published work head `e4dab271c` clears the two remaining Concert unit-test constructor diagnostics by
supplying the existing `IBookingModule` boundary at both fixture construction sites. The compiler named
the trailing logger parameter only because each positional argument list was one entry short; both
fixtures already supplied `ILogger<ConcertService>`. The focused Release suite passes 88/88.

The prior focused Concert integration request-builder recovery imported `OpportunityRequest` from its
Opportunity-owned Application namespace. Its repeated project build removed both builder diagnostics
and left 26 stale integration-test ownership diagnostics across Application and Concert fixtures; no
other integration fixture was changed in that slice.

The focused `ApplicationCancelApiTests` recovery now reads Application, Booking, and Concert entities
through their owning contexts exposed by the existing fixture reset scope. Its assertions use
`ApplicationState`, `BookingState`, or `ConcertState` for the stage they verify. All four diagnostics
owned by that file are gone, `git diff --check` passes, and the exact remaining integration frontier is
22 errors outside this slice. Published work head `da3d55be6` is verified equal across local, remote,
and draft PR #633.

The focused `ApplicationDoorSplitApiTests` recovery now reads Application rows through
`ApplicationDb`, Booking creation and financial-confirmation failure through `BookingDb`, and
Concert creation only through `ConcertReads`. DoorSplit acceptance asserts the resulting
`DeferredBooking`; both failed-verification paths assert `BookingState.ConfirmationFailed`.
The fixture support project now directly references the Admin Domain and Infrastructure assemblies whose
types it consumes, so a normal dependency build reaches the Concert integration project. The target file
contributes no diagnostic and the exact remaining integration frontier is 21 errors outside this slice.
Published work head `eeee95ac4` is verified equal across local, remote, and draft PR #633.

The focused `ApplicationFinancialOperationApiTests` recovery now consumes `BookingState` at its response
boundary. A pending acceptance operation maps to `AwaitingConfirmation`; a rejected operation maps to
`ConfirmationFailed`. Both diagnostics owned by that file are gone, `git diff --check`
passes, and the exact remaining integration frontier is 19 errors outside this slice. Published work head
`59a5326e1` is verified equal across local, remote, and draft PR #633.

Booking's financial state vocabulary is now `AwaitingConfirmation` and `ConfirmationFailed`; the
redundant `Financial` qualifier was removed from both the internal state and public response status.
Booking unit tests pass 15/15 and the Application API Release build passes with 0 warnings and 0 errors
at published work head `84c57fb4d`.

The focused `ApplicationFlatFeeApiTests` recovery now reads Application and Booking state through their
own module contexts, asserts `BookingState.ConfirmationFailed` for both payment-failure arrival orders,
and leaves Concert assertions on `ConcertReads`. Direct subtype queries use `Set<StandardApplication>()`.
The raw Application and Booking fixture handles are provisionally named `ApplicationDb` and `BookingDb`;
the missing canonical module-owned integration assertion surface is recorded in B2B technical debt for
discussion. The target file contributes no diagnostic and the exact remaining integration frontier is
18 errors outside this slice. Published work head `757703caa` is on the draft PR branch.

Tommy rejected that provisional topology because it contradicts this PR's module-ownership purpose.
The complete Concert integration tree has now been audited by operation and assertion surface. Its
Application, Booking/Contract, Opportunity, Deal, Artist-dashboard, complete-journey, and stale Booking
processor coverage will move to owning module or B2B process projects; mixed files will split by
operation rather than move as indivisible legacy files. Concert retains only its HTTP, creation,
cancellation, completion, settlement, invoice, self-billing, notification, and outbox coverage.

The corrected fixture topology is fixed before further compile recovery: local module fixtures derive
from the shared host harness and resolve only their own production context/read stance; the B2B process
suite uses HTTP or deliberate Contracts boundaries and directly references no module Domain or
Infrastructure assembly. A mechanical project-reference guard enforces that rule. The temporary
`ApplicationDb`/`BookingDb` surface and the corresponding TECH_DEBT entry have been removed rather than
renamed or hidden behind another resolver.

Concert's fixture topology is now corrected. `ConcertApiFixture` is module-local and resolves only
Concert's production write/read stance and Concert-owned operation drivers; Application and Booking
contexts, generic reads, and foreign repository service location are absent. Inherited
`ApiFixture.Services` remains available as host-harness composition infrastructure. The
shared fixture no longer owns a Concert fixture or Concert persistence. Concert assertions use only
Concert entities, while the full VenueHire posting/outbox journey and exact cross-module cancellation
refund assertion live in B2B Process. The Concert and Process integration projects build with 0 errors,
and the focused integration-project boundary guard passes.

The complete integration topology is now closed. Opportunity, Application, Booking, Deal, and B2B
Process integration projects exist in `Concertable.B2B.slnx`; every module fixture is local and resolves
only its owning production context/read stance. The shared fixture project contains only host-neutral
`ApiFixture`, including its public `Services` composition capability. The B2B E2E host no longer reads
Application persistence: cancellation is verified through the Application HTTP contract, and Concert
completion is verified through the Concert HTTP response. Module facades now delegate persistence work to
module-local application services, the Worker host composes every required module, and the mechanical
architecture rules pass in full. No local integration or E2E suite was executed.

Current `origin/main` at `a091f5342` is reconciled by work head `b5c35c095`. Checkpoint `ab9190723`
merged the preceding main head with six textual conflicts resolved without restoring the deleted
Concert-side Booking service, stale Concert-owned Application cancellation tests, or the shared Admin
fixture. The merged Admin and User provisioning coverage now uses each module's local fixture and public
Auth/event boundary; Admin no longer resolves User persistence or constructs a User domain entity. The
short-lived shared-fixture TECH_DEBT item introduced on `main` is removed because its minimal test-identity
concern is resolved by the existing `CreateClient(Guid, string)` boundary. The final upstream delta was a
docs-only polyrepo-plan closeout and merged without conflict as `b5c35c095`.

Exact-head CI run `32542251616` passed the solution build and all other completed shards, then exposed two
closure failures. The Deal unit suite still carried five source-text assertions for deleted Concert
workflow registries and registrations; they now assert that `FrozenDictionary<DealType>` is absent and
self-verify the live Deal and Concert strategy families, passing 54/54 locally. The Admin integration shard
did not reach a test: B2B host startup reported pending model changes for `ConcertDbContext`, so the
repository-wide `InitialCreate` re-scaffold was the next mechanical closure step.

The canonical repository-wide `InitialCreate` re-scaffold is complete at published work head
`7eb5e7a88`. Opportunity, Application, and Booking are now included in `api/initial-migrations.ps1`, each
has a module-local design-time factory, and each owns its first migration and model snapshot. The corrected
Concert migration contains only Concert-owned persistence. The scaffold reported every pre-existing
unaffected context unchanged, and EF reports no pending model changes for Opportunity, Application,
Booking, or Concert. The B2B Release solution builds with 0 errors; architecture tests pass 10/10,
composition tests pass 5/5, Deal unit tests pass 54/54, the plan graph reports 0 errors/warnings, and
`git diff --check` passes. No local integration or E2E runtime suite was executed.

The Deal foundation is now delivered. Its production net10 result is the Deal-owned validated invariant
factory for `IDealMapper` and `IDealUpdater`; the generator/analyzer prototype was intentionally removed.
PR #633 therefore resumes against `DealDto` and the proven module-local factory pattern, not against a
nonexistent generated factory. After the compile-recovery frontier is green, heterogeneous lifecycle
methods use keyed implementations behind dedicated factories and method-header interfaces on net10;
their .NET 11 return boundary becomes a direct native union of those interfaces.

Terminal current main `d0b8f616f` is reconciled into the preserved module carve. All 45 merge conflicts
are resolved without restoring obsolete Concert-owned Application, Booking, Contract, or workflow
artifacts. Application now consumes the landed `DealDto` hierarchy, Opportunity's cross-tenant claim
context uses the renamed `PrivilegedDbContext`, and the B2B host composes Artist, Venue, and Deal dev
seeders only through their API module boundaries. The B2B Web Release build passes with 0 warnings and
0 errors.

## Next Steps

Active slice: checkpoint the validated `223e20a75` dashboard reconciliation, merge the subsequent
`origin/main` head `26645ecd1`, repeat the B2B build and focused topology gates, then remote-validate the
resulting exact PR #633 head. Do not edit aggregate transition tables or state-machine implementation files
owned by the parallel state-machine slice. When CI is green, the topology correction is terminal and the
parallel state-machine slice owns the next implementation change. Do not resume the superseded file-by-file
Concert compile recovery.

## Completed work

- Resolved the dashboard landing from `origin/main` at `223e20a75` without restoring stale Concert-owned
  Application or Opportunity services, repositories, mappers, projections, tests, or persistence. Application
  and Opportunity now own their dashboard endpoints and use Contracts-only cross-module facts; a narrow
  Application-owned metrics provider keeps the production dependency graph acyclic. Booking no longer grants
  Concert test assemblies internal access. Application and Concert integration projects build with 0 warnings
  and 0 errors; the complete B2B Release solution builds with 0 errors; architecture tests pass 12/12,
  composition tests pass 6/6, Opportunity unit tests pass 3/3, all four carved EF contexts report no pending
  model changes, the plan graph reports 0 errors and 0 warnings, and both staged and unstaged diff checks pass.
  No local integration or E2E runtime suite was executed.
- Published migration-closure checkpoint `7eb5e7a88`; local HEAD, the remote branch, and PR #633
  `headRefOid` all equalled `7eb5e7a88db900621c637c5302abe6232031c157`. The canonical full re-scaffold
  added module-owned Opportunity, Application, and Booking migrations plus design-time factories, and
  removed those tables from Concert's migration. All pre-existing unaffected contexts retained their
  migration IDs; all four affected B2B contexts report no pending model changes. The B2B Release solution
  builds with 0 errors; architecture tests pass 10/10, composition tests pass 5/5, Deal unit tests pass
  54/54, the plan graph reports 0 errors/warnings, and `git diff --check` passes.
- Published Deal CI-fix checkpoint `93d9d3ee4`; local HEAD, the remote branch, and PR #633 `headRefOid`
  all equalled `93d9d3ee49177534581c5c2238ecf75099ae363a`. It removes five stale architecture
  assertions for deleted Concert workflow registries and strategy registrations. The guard now requires
  the current Deal and Concert strategy families and asserts that the rejected
  `FrozenDictionary<DealType>` workflow registry shape is absent.
- Reconciled `origin/main` through `a091f5342` into the closed integration topology. Verified work heads
  `ab91907232517f990e341a4ef2c1c228392b265a` and
  `b5c35c09545c08eaad22e8800cb1074ce0c5f27c` were each pushed and proven equal across local,
  remote-tracking, and PR #633 heads. Admin provisioning now crosses into Auth through the
  integration-event and HTTP boundaries while retaining only Admin's real persistence stance; User
  provisioning remains on the User fixture. Deleted Concert-owned and shared multi-owner fixtures remain
  deleted.
- Published integration-topology closure checkpoint `fc678e0ed`; local HEAD, the remote branch, and PR
  #633 `headRefOid` all equalled `fc678e0edaf5970e01ace03b9400c59a23da3c86`. The complete B2B solution
  builds with every module and Process integration project plus the host E2E project; architecture tests
  pass 10/10 and composition tests pass 5/5. The host no longer asserts Application lifecycle through SQL,
  module facades no longer depend directly on persistence components, Worker composition includes the carved
  modules, and direct Reunion package ownership is exact.
- Published remaining-fixture ownership checkpoint `e196fddb1`; local HEAD, the remote branch, and PR
  #633 `headRefOid` all equalled `e196fddb1bf7227ae8eb4496b897bf781b73d187`. Moved User's fixture out
  of the shared harness, added module-local Artist and Venue fixtures, converted their persistence
  assertions to owning read contexts, and moved the complete Admin registration journey to B2B Process.
  Standardized fixture fields on `dbContext`/`readDbContext`, retained the inherited public service provider
  as a host-harness capability, and removed the resolved fixture TECH_DEBT item. Artist, Venue, Application,
  Booking, Admin, Tenant, User, Concert, and Process integration projects compile with 0 errors; the focused
  architecture guard passes 1/1, the plan graph reports 0 errors and 0 warnings, and `git diff --check`
  passes.
- Published Admin/Tenant fixture-ownership checkpoint `f1e7dca08`; local HEAD, the remote branch, and
  PR #633 `headRefOid` all equalled `f1e7dca088d6d60f7a0c6bc27ca181e8667e68da`. Moved both module
  fixtures out of the shared harness, replaced test service location with typed owning-module operation
  drivers, removed the shared fixture project's Admin references and stale IVTs, and added only owning
  project references. Both integration projects build with 0 warnings and 0 errors; the focused
  architecture guard passes 1/1, the plan graph reports 0 errors and 0 warnings, and `git diff --check`
  passes.
- Published Concert fixture-contraction checkpoint `897ef9e89`; local HEAD, the remote branch, and PR
  #633 `headRefOid` all equalled `897ef9e894a46b212e2eeab76eac6f8afc47c860`. Replaced the shared
  multi-context fixture with a module-local Concert fixture, removed Application/Booking persistence and
  repository service location, converted retained assertions to Concert-owned state, and moved the
  posting/outbox journey to B2B Process. Concert and Process build with 0 errors; the focused architecture
  guard passes 1/1, the plan graph reports 0 errors and 0 warnings, and `git diff --check` passes.
- Published cancellation integration ownership checkpoint `5e98d6b31`; local HEAD, the remote branch,
  and PR #633 `headRefOid` all equalled `5e98d6b31554635c0c1e38ce3f161366434c8d0c`.
  Deleted the stale Concert `ApplicationCancelApiTests`, added the accepted-withdraw guard and pending
  cancel-link assertions to Application, and placed the boundary-only Booking cancellation, late-capture,
  notification/outbox, and Booking/Concert Opportunity-reopening journeys in B2B Process. Booking and
  Process built with 0 warnings and 0 errors; Application built with 0 errors and its inherited
  `UserEntity` warning; the architecture guard and plan graph passed. The Process facts deliberately pin
  the missing non-blocking notification/reopening reactions for implementation after the parallel state
  machine checkpoint rather than weakening or replacing them with foreign persistence assertions.
- Published booking-confirmation integration ownership checkpoint `adb000732`; local HEAD, the remote
  branch, and PR #633 `headRefOid` all equalled `adb000732b32e451d851252eff44ef4cb4d2cfe5`.
  Moved the complete Application/Booking/Concert email-delivery journey to B2B Process, moved the
  Concert template rendering and escaping assertion to Concert unit tests, and removed the remaining
  `fixture.Services` dependency from that coverage. The Process project built with 0 warnings and 0
  errors, the focused Concert unit test passed, and the integration-project boundary guard passed.
- Delivered Kernel producer PR [#719](https://github.com/Concertable/concertable/pull/719) from exact
  source head `541ff90f5b3910815c5835c38c03a52dc47434f4`; local, remote-tracking, and PR heads were proven
  equal before merge. Producer commit `33ee4dd1199b7a9ed3f84d1ca0b0eacde3eddc95` adds the immutable
  Result-based state machine and its focused behavioural coverage. Merge-group run `32541628076`
  passed and the PR merged as `a1ec9fa2e690aa9c1df5c467cd742481514e28c7`. Publish-packages run
  `32543949659` published `Concertable.Kernel` `0.1.0-alpha.0.1133`; generated platform-sync PR
  [#730](https://github.com/Concertable/concertable/pull/730) passed run `32545078748` and merged as
  `067438ccf8442e10aa05fa4b8f40d0b045c0aaf1`.
- Published tenant-scoping integration ownership checkpoint `58c72d03b`; local HEAD, the remote branch,
  and PR #633 `headRefOid` all equalled `58c72d03b793a1dca40b76f8030b125bebc4b8fc`. Split the
  six former Concert cases across Application tenant stamping/visibility, Booking's unscoped module read
  stance, Concert's public read, and B2B Process tenant-propagation/organization-action journeys. Removed
  repository service location, foreign generic context reads, and the now-unused lifecycle request builder.
- Published Booking cancellation ownership checkpoint `5b5155926`; local HEAD, the remote branch, and
  PR #633 `headRefOid` all equalled `5b5155926a5136ffee6fe0071eb99f20c2a574dc`. Added the
  missing Booking-owned refund success/rejection processor and seven Booking integration cases covering
  cancellation from awaiting confirmation and confirmation failure, no-held-escrow cancellation, late
  capture compensation, rejected refund, confirmed-state guarding, and artist authorization. Booking and
  Concert now consume only refund operation IDs owned by their lifecycle stage.
- Published Versus integration ownership checkpoint `db13a175c`; local HEAD, the remote branch, and PR
  #633 `headRefOid` all equalled `db13a175c58dcd3ec41be0cd58c1d6daef420e77`. Moved four
  Application-owned checkout, creation, and duplicate-accept cases into Application tests and five
  complete verify-payment journeys into B2B Process tests. The stale Concert lifecycle failure assertion
  is now the current Booking `ConfirmationFailed` boundary plus no-Concert and notification assertions.
- Published VenueHire integration ownership checkpoint `7dbbea977`; local HEAD, the remote branch, and
  PR #633 `headRefOid` all equalled `7dbbea9777e549f6c44b63e655b261aa944c0785`. Moved five
  Application-owned checkout, prepaid-application, and duplicate-accept cases into Application tests and
  four complete acceptance/payment journeys into B2B Process tests. The escrow command's Booking ID is
  derived from the public Application cancel action before confirmation, preserving the exact cross-module
  assertion without Booking persistence access.
- Published DoorSplit integration ownership checkpoint `ccb9ff0a8`; local HEAD, the remote branch, and
  PR #633 `headRefOid` all equalled `ccb9ff0a8143c9dc78f33abf83e07b76f75735ad`. Moved four
  Application-owned checkout, validation, creation, and duplicate-accept cases into Application
  integration tests and six complete deferred-payment journeys into B2B Process integration tests.
  Both payment/Accept arrival orders, confirmation failure, the public Concert identity across webhook
  redelivery, Booking state, Concert creation, and notifications remain asserted through boundaries.
- Published FlatFee integration ownership checkpoint `98ee00aeb`; local HEAD, the remote branch, and
  PR #633 `headRefOid` all equalled `98ee00aeb9a5b012d0e57b81513cdb2d6b143ed4`. Moved the four
  Application-owned checkout, validation, creation, and duplicate-accept cases into Application
  integration tests and the four payment/Accept/Booking/Concert journeys into B2B Process integration
  tests. Every cross-module assertion now uses HTTP or Contracts; the original eight facts remain.
- Published Contract integration ownership checkpoint `f398121c6`; local HEAD, the remote branch, and PR
  #633 `headRefOid` all equalled `f398121c6606b1b12900bcd04ffcc55abcd188f2`. Split all 16
  `ContractApiTests` cases by owner: six Application consent, signature, fingerprint, and
  HATEOAS cases now use only `ApplicationApiFixture`; ten immutable Contract snapshot, signature, PDF, and
  metadata cases now use only `BookingApiFixture`. Opportunity creation/update and Application setup cross
  module boundaries through HTTP, replacing the old `IDealModule` service location and Concert multi-context
  reads without dropping an assertion.
- Published Artist dashboard integration ownership checkpoint `0d926e5e0`; local HEAD, the remote branch,
  and PR #633 `headRefOid` all equalled `0d926e5e00fbbda6ed4fa1546cd100041d1b84eb`. Moved
  `ArtistDashboardCountsTests` into Artist integration tests and replaced direct `IConcertModule` service
  location with before/after assertions through `/api/artist-dashboard/kpis`.
- Published Booking financial integration ownership checkpoint `51dd489d8`; local HEAD, the remote branch,
  and PR #633 `headRefOid` all equalled `51dd489d82834cc7e97b06a1ceda2bc54c0b84e8`. Moved
  `ApplicationFinancialOperationApiTests` into Booking integration tests and replaced the stale
  `EscrowPaymentProcessorTests` with `AcceptanceFinancialOperationOutcomeProcessorTests`. The rewritten
  test dispatches the current capture-success contract twice through host-neutral harness infrastructure,
  asserts Booking confirmation through the Booking read stance, and proves exactly one Booking-owned inbox
  acknowledgment through Booking's real production context. No test receives a service provider.
- Published Application integration ownership checkpoint `9b28ce842`; local HEAD, the remote branch, and
  PR #633 `headRefOid` all equalled `9b28ce842b8143b74c9cbf56e75c866a7a62b69c`. Moved
  `ApplicationApiTests` and `ApplicationWithdrawRejectApiTests` into Application integration tests.
  Both suites use `ApplicationApiFixture` and the module-owned Application read stance; the withdrawal
  scenario observes Opportunity reopening through HTTP with a local boundary projection rather than an
  Opportunity Domain/Infrastructure reference.
- Published Opportunity/Deal integration ownership checkpoint `d5ac1c35e`; local HEAD, the remote branch,
  and PR #633 `headRefOid` all equalled `d5ac1c35e8747097e063c9181f7cb785dc9375f5`. Moved
  `OpportunityApiTests` and its canonical request builder into Opportunity integration tests and
  moved `DealApiTests` into Deal integration tests. Both suites now use their owning fixtures and current
  module contracts; Concert has no Deal or Opportunity test namespace/class. The temporary lifecycle
  request helper left for not-yet-moved journey tests is explicitly Application-local and is removed when
  those consumers move.
- Published integration-test topology checkpoint `36e460bd5`; local HEAD, the remote branch, and PR #633
  `headRefOid` all equalled `36e460bd57920cb9c94abc3e5e512a69fe2067d8`. Application, Booking, Opportunity, Deal, and B2B
  Process integration projects now have local fixtures, collections, tier metadata, guidance siblings,
  solution entries, and required own-module friend access. Application and Booking gained real
  module-owned, read-only production DbContext stances; their fixtures expose only those stances. The B2B
  process project references only the host-neutral fixture/testing harness, and an architecture test now
  rejects direct references from any module integration project to another module's Domain or
  Infrastructure assembly.
- Published integration-test ownership audit range `3204e8abd..f80f8cbb1`; local HEAD, the remote
  branch, and PR #633 `headRefOid` all equalled
  `f80f8cbb1ca8ebe33f9e023ba3b06d20d03d0936` after the work-head push.
- Audited every test and helper in `Concertable.B2B.Concert.IntegrationTests` by actual operation and
  assertion purpose; recorded the per-file split and the module/process target topology in the plan.
- Reconciled the heterogeneous-method landing design: net10 uses keyed DI only inside a dedicated
  marker-returning factory, consumers match honest method-header interfaces and guarded required input,
  and .NET 11 changes that return boundary to a direct native interface union without Dunet wrappers.
- Published `ApplicationDoorSplitApiTests` recovery work head `eeee95ac4`; Application, Booking, and
  Concert assertions use their owning read contexts, financial failure uses `BookingState`, the fixture
  declares its direct Admin references, and the remaining integration frontier is exactly 21 errors.
- Published `ApplicationFinancialOperationApiTests` recovery work head `59a5326e1`; its response model
  uses Booking-owned financial state and the remaining integration frontier is exactly 19 errors.
- Published Booking state-vocabulary simplification work head `84c57fb4d`; the internal and public
  financial states are now `AwaitingConfirmation` and `ConfirmationFailed`.
- Published `ApplicationFlatFeeApiTests` recovery work head `757703caa`; Application and Booking
  assertions use their owning contexts, both financial-failure arrival orders assert Booking state,
  and the remaining integration frontier is exactly 18 errors.
- Published current-main reconciliation range `0511c35ca..fccab851d`; local HEAD, the remote branch,
  and PR #633 `headRefOid` all equalled `fccab851de826ebfcce87265a32f20522ce7289c`, and the branch was
  0 commits behind `origin/main`.
- Published `ApplicationCancelApiTests` recovery range `f64d5fe32..da3d55be6`; its four diagnostics are
  gone, the remaining integration frontier is exactly 22 errors, and local, remote, and PR heads matched.
- Recovered `OpportunityRequestBuilders` onto the Opportunity-owned request namespace in its prior
  checkpoint; both stale builder diagnostics are gone and the remaining integration frontier was 26 errors.
- Recovered both surviving `ConcertService` unit-test fixtures onto the current `IBookingModule`
  constructor boundary; the focused Release suite passes 88/88.
- Recovered the Workers completion-runner fixture onto the current operation-owned executor and moved
  the four lifecycle-module dev-seeder registrations behind `AddOpportunityApi`, `AddApplicationApi`,
  `AddBookingApi`, and `AddConcertApi`.
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
- Published Concert invoice-query compile-recovery range `23be92681..8a44f386a` from starting remote
  head `23be926810fc1698003eaf764a48a57ccd3b49ec`; local HEAD, the remote branch, and draft PR #633
  `headRefOid` all equalled `8a44f386a47c06c02d0ead3b5c3458472d22e7ac` after the work-head push.
- Published Concert unit-test ownership recovery range `34010ca4c..b390da9b3`; local HEAD, the remote
  branch, and draft PR #633 `headRefOid` all equalled
  `b390da9b3fa67731edefcf1e9fbc60c6d251e056` after the work-head push.
- Merged the 93-commit `origin/main` drift without conflicts as
  `ff2e4dc553aad7bd9093e958235fa809efe5c881`, then verified local HEAD, the remote branch, and draft PR
  #633 `headRefOid` matched and the branch was 0 commits behind.
- Cleared the B2B host/API compile frontier by routing the Concert contract-PDF compatibility endpoint
  through `IBookingModule`, removing stale workflow/request imports, and keeping dev-seeder
  registration behind module API composition extensions; published as `d1703c218`.

## Verification

- Exact-head CI run `32542251616` passed the solution build and every completed shard except Deal unit tests
  and the Admin integration shard. The Deal failure reproduces locally and is fixed; its Release suite now
  passes 54/54. Admin failed before test execution because `ConcertDbContext` has pending model changes;
  the prescribed full `InitialCreate` re-scaffold remains before the next exact-head run.
- Current-main reconciliation through work head `b5c35c095`: Admin and User integration projects build
  with 0 errors; architecture
  tests pass 10/10 and composition tests pass 5/5. The full B2B Release solution build succeeds with
  0 errors and two generated UI nullable-annotation warnings. The Concert stale-ownership scan is empty,
  the plan graph reports 0 errors and 0 warnings, and both staged and unstaged `git diff --check` pass.
  No local integration or E2E test was run.
- Integration-topology closure at work head `fc678e0edaf5970e01ace03b9400c59a23da3c86`:
  `dotnet build api/Concertable.B2B/Concertable.B2B.slnx --no-restore -c Release --nologo -v:minimal`
  passed with 0 errors. Every module integration project, B2B Process, remaining Concert integration, and
  the B2B E2E host compiled; output contained two existing Concert nullable warnings and two generated UI
  nullable-annotation warnings.
- Full `Concertable.B2B.ArchitectureTests` passed 10/10, including integration-project reference boundaries,
  module-facade dependency boundaries, and direct Reunion package ownership. Full
  `Concertable.B2B.CompositionTests` passed 5/5 across the B2B executable hosts.
- The stale B2B E2E Application SQL helper and Concert lifecycle imports are absent. Cancellation now polls
  the public Application response, and completion polls the public Concert response. No local integration or
  E2E test was run.
- `python .agents/hooks/plan_graph.py --root <worktree>` reports 0 errors and 0 warnings; `git diff --check`
  passes.
- Kernel producer PR #719 exact head: `Concertable.Kernel.UnitTests` passed 246/246 in Release;
  successful Result, typed rejection, duplicate-edge rejection, mutable-input snapshotting, and
  concurrent reads are covered. Packing `Concertable.Kernel` at the same head produced a `.nuspec`
  with no Reunion dependency, and `git diff --check` passed. Exact-head CI run `32540096727`,
  successful merge-group run `32541628076`, publish-packages run `32543949659`, and platform-sync
  merge-group run `32545078748` are green; the published and synchronized version is
  `0.1.0-alpha.0.1133`.
- Tenant-scoping ownership split: Application, Booking, and Process integration projects build with 0
  warnings and 0 errors; the focused `IntegrationTestBoundaryTests` guard passes. Concert remains at exactly
  13 unrelated known compile errors and its retained tenant test reads only the public Concert endpoint.
- Booking cancellation ownership: Booking integration tests and Concert infrastructure build with 0 warnings
  and 0 errors; the focused `IntegrationTestBoundaryTests` guard passes. The new Booking suite asserts only
  Booking's real read stance after initiating cancellation through the public Booking route.
- Versus ownership split: Application and Process integration projects each build with 0 warnings and
  0 errors; all nine original facts remain and the focused `IntegrationTestBoundaryTests` guard passes.
  Removing the final deal-specific Application file reduces the Concert integration frontier from 14 to
  exactly 13 known compile errors.
- VenueHire ownership split: Application and Process integration projects each build with 0 warnings and
  0 errors; all nine original facts remain and the focused `IntegrationTestBoundaryTests` guard passes.
  Removing the stale VenueHire Application file reduces the Concert integration frontier from 15 to exactly
  14 known compile errors.
- DoorSplit ownership split: Application and Process integration projects each build with 0 warnings and
  0 errors; all ten original facts remain and the focused `IntegrationTestBoundaryTests` guard passes.
  The reduced Concert integration frontier remains exactly 15 known compile errors because the removed
  DoorSplit Application file contributed no diagnostic before the move.
- FlatFee ownership split: Application and Process integration projects each build with 0 warnings and
  0 errors; the focused `IntegrationTestBoundaryTests` guard passes. The reduced Concert integration
  frontier remains exactly 15 known compile errors because the removed FlatFee file contributed no
  diagnostic before the move.
- Contract ownership split: Application and Booking integration projects each build with 0 warnings and 0
  errors; the 6 Application and 10 Booking facts preserve the original 16 cases. The reduced Concert
  integration frontier is exactly 15 known compile errors, down from 17 because the stale Contract file is
  gone.
- Artist dashboard move: the Artist integration project restores and builds with 0 warnings and 0 errors.
- Booking financial-outcome move: the Booking integration project builds with 0 warnings and 0 errors;
  the stale `EscrowPaymentProcessor` test/class vocabulary is absent.
- Application single-owner move: the Application integration project builds with 0 warnings and 0 errors.
  The reduced Concert integration project now has exactly 17 known compile errors, one fewer because the
  stale withdrawal/rejection suite moved and was corrected in its owning project.
- Opportunity and Deal single-owner move: both owning integration projects build with 0 warnings and 0
  errors. The reduced Concert integration project remains at exactly the prior 18 known compile errors,
  with no new diagnostic. The Concert ownership search finds no Deal/Opportunity test namespace or class.
- Integration-test topology scaffold: Application, Booking, Opportunity, Deal, and B2B Process integration
  projects each build with 0 errors; the focused `IntegrationTestBoundaryTests` architecture guard passes
  1/1. Application, Opportunity, Deal, and Process report 0 warnings; Booking's clean incremental
  confirmation reports 0 warnings after its initial dependency-graph build reported one transient warning.
  Docs reachability reports 0 errors, the plan graph reports 0 errors/warnings, and `git diff --check`
  passes.
- Integration-test ownership audit: `python .agents/hooks/plan_graph.py --root .` reports 0 errors and
  0 warnings; `git diff --check` passes.
- Current-main reconciliation: no unmerged paths or conflict markers remain, and `git diff --check`
  passes.
- `dotnet build api/Concertable.B2B/src/Concertable.B2B.Web/Concertable.B2B.Web.csproj
  --configuration Release --no-restore --disable-build-servers --maxcpucount:1
  --consoleLoggerParameters:ErrorsOnly`: 0 warnings and 0 errors after the `DealDto`, privileged-context,
  and host-composition adaptations.
- `dotnet test
  api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.UnitTests/Concertable.B2B.Concert.UnitTests.csproj
  --configuration Release --no-restore --disable-build-servers --maxcpucount:1`: 88/88 passed after
  supplying `IBookingModule` at both `ConcertService` fixture construction sites.
- Current candidate `git diff --check`: passed.
- `dotnet test
  api/Concertable.B2B/tests/Concertable.B2B.Workers.UnitTests/Concertable.B2B.Workers.UnitTests.csproj
  --configuration Release --no-restore --disable-build-servers --maxcpucount:1`: 5/5 passed.
- `dotnet build api/Concertable.B2B/src/Concertable.B2B.Web/Concertable.B2B.Web.csproj
  --configuration Release --no-restore --disable-build-servers --maxcpucount:1`: 0 warnings and
  0 errors after the lifecycle API composition-root change.
- The scoped Workers scan finds no `IFinishExecutor`, `FinishAsync`, `GetEndedConfirmedIdsAsync`, or
  deleted `Application.Workflow.Executors` reference. The B2B Web host has no direct Opportunity,
  Application, Booking, or Concert dev-seeder call. `git diff --check` passes.
- Exact-head CI run `32236953306` at `a7eddeeaa` identifies the next bounded frontier as two stale
  `ConcertService` unit-test constructor calls missing the current logger dependency; its broader
  integration/E2E stale-ownership errors remain for later bounded slices.
- Repeated `dotnet build
  api/Concertable.B2B/src/Concertable.B2B.Web/Concertable.B2B.Web.csproj --configuration Release
  --no-restore --disable-build-servers --maxcpucount:1`: 0 warnings and 0 errors.
- Scoped host/module-API ownership scans find no deleted workflow, shared lifecycle, cross-stage entity,
  moved-module host Infrastructure import, or Concert-API `IContractService` reference.
- Exact-head CI run `32192242931` at `a183b2df2` proves the remaining full-build frontier is confined to
  `Concertable.B2B.Workers.UnitTests/Functions/ConcertFinishedFunctionTests.cs`, which still references
  the deleted `IFinishExecutor` and workflow namespace.
- Work head `d1703c218` `git diff --check`: passed.
- Concert unit-test ownership scan finds no `IConcertWorkflow`, `ConcertWorkflow`,
  `IConcertStateMachineRegistry`, `LifecycleState`, `IFinishStep`, `FinishExecutor`,
  `ConcertDraftService`, `ContractIssuer`, Application/Booking/Opportunity entities or repositories,
  or deleted payment-amount mapper reference in the Concert unit project; its file paths contain no
  deleted `Workflow` or `FinishExecutor` vocabulary.
- `dotnet test api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.UnitTests/Concertable.B2B.Concert.UnitTests.csproj --configuration Release --no-restore --disable-build-servers --maxcpucount:1`:
  88/88 passed with 0 warnings and 0 errors.
- Current candidate `git diff --check`: passed.
- Concert composition-root ownership scan finds neither the Application-owned
  `IApplicationValidator` registration nor the Opportunity-owned `OpportunityDtoValidator` scan under
  Concert. Their owning composition roots retain both registrations, and Concert registers
  `BookingConfirmedDomainEventHandler` through
  `IPreCommitDomainEventHandler<BookingConfirmedDomainEvent>`.
- The scoped Invoice/Booking ownership grep across Concert repositories and repository interfaces
  finds no `context.Bookings`, `BookingEntity`, or Booking Domain/Infrastructure dependency.
- The Concert.Infrastructure single-worker Release build passes with 0 errors and one inherited
  `UserEntity` CS0628 warning. The former `InvoiceRepository` CS1061 diagnostic and all nine
  `CompleteExecutor` diagnostics are cleared.
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
- `git diff --check` passed for the email-composition slice.
- Application counterparty-notification ownership grep: every
  `ApplicationCounterpartyNotifiedDomainEvent` and `ApplicationNotification` production/test reference
  is under Application; Concert has no match.
- Application unit tests passed 11/11 in Release after the ownership move; Application.Infrastructure
  built with 0 warnings and 0 errors using `--no-restore`, disabled build servers, and single-worker
  MSBuild.
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
- Refund recovery slice: `git diff --check` passed, and the scoped
  `RefundEscrowDeferredEvent` scan across Concert plus the B2B host returned no matches.
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
- One configured state machine exists per owning aggregate/module, not per individual enum value or
  `DealType`.
- The common `IStateMachine<TState, TTrigger>` is a stateless Result-based Kernel algorithm boundary,
  not a common lifecycle contract. Configured machines, states, triggers, and tables remain module-local.
- `Transition` returns `Result<TState, TransitionError<TState, TTrigger>>`. Success always carries the
  real next state; rejection carries the common concrete machine error. There is no `Try`/`out` shape,
  default state, exception for an ordinary missing edge, or additional outcome generic.
- `TransitionError<TState, TTrigger>` is a concrete parameterized state-machine failure, not an
  inheritance base. It may be returned directly when complete or composed into a closed operation-owned
  error union when the operation has additional failures.
- Shared `NotFound`/transition-error bases, `IState` markers, state inheritance, open error catalogs, and
  `Result<T, IError>` are rejected because they erase exhaustive operation failure contracts.
- Aggregates alone mutate lifecycle state. Callers select semantic operations and persist their
  outcomes; they never calculate a target state, inject or resolve a machine into an entity, or invoke
  a generic public transition command.
- The state machine is built once from a copied frozen table and stores no current entity state. It has
  no mutation API, callbacks, entry/exit actions, persistence, messaging, retries, or service resolution.
- Opportunity's atomic `Open -> Filled` claim remains a conditional persistence operation. Its remaining
  state changes are audited before deciding whether they form a genuine aggregate machine; symmetry is
  not sufficient.
- The old per-`DealType` Concert machine is not restored. Application, Booking, and Concert legal edges
  are independent of the Deal implementation selected inside an operation.
- Kernel package publication is an external-artifact prerequisite: deliver the shared producer
  checkpoint and published platform version, then consume it from PR #633. This does not split B2B
  ownership implementation into another PR.
- Payment PR #707 is a downstream candidate for the same immutable edge lookup, without moving Payment
  validation, duplicate handling, terminal-state rules, or transition outcomes into Kernel.
- Context supplies names inside a module: `State`, `Trigger`, `StateMachine`, and `ICancelStep` do not
  need Application/Booking/Concert prefixes internally.
- The dispatch investigation is concluded. Honest same-interface families use module-specific invariant
  strategy factories. Heterogeneous lifecycle methods use one marker plus one interface per honest method
  header. On net10, a dedicated factory confines keyed service resolution and returns the selected marker;
  consumers pattern-match the header type. On .NET 11, its return boundary becomes a direct native union
  such as `union Accept(IStandardAccept, IPrepaidAccept)`, with exhaustive type-pattern matching and no
  wrapper records. Deliberate aliases may map multiple Deals to implementations of one header, and each
  header may have multiple keyed implementations. Neither path restores a global workflow.
- `IApplicationDealStrategyFactory<TStrategy>` is not reused for `Accept`: its method headers have
  different invocations and do not form one substitutable strategy family. The dedicated name is
  `IAcceptFactory`, not `IAcceptStepFactory`.
- A prepaid method requires a non-null payment method. The consumer uses
  `when paymentMethodId is not null` to invoke it and returns the typed payment-method-required Result in
  the remaining prepaid arm. It does not use a throwing `RequirePaymentMethod` helper or make the method
  parameter nullable.
- PR #633 owns the best-effort net10 heterogeneous-operation conversion after its compile-recovery
  frontier is green. It builds the module-local marker, method-header interfaces, validated mapping, and
  dedicated factory over the keyed-DI foundation already available on net10; no generated factory
  machinery exists. The downstream Deal plan retains the later .NET 11 native/closed compiler-enforced
  cut-over.
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
  operation-outcome shapes with case-specific data. A heterogeneous method factory may return a direct
  native union of its method-header interfaces. No concrete implementation may implement multiple headers
  in the same union. Neither form creates shared lifecycle ownership; persistence maps each module's
  discriminator explicitly, and the future compile-time dispatch package replaces keyed factory
  internals without changing the call-site abstraction.
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
  resulting closed value shapes and enables module-local dedicated factories to return direct native
  unions of their method-header interfaces without restoring the rejected god-workflow model.
- Completed prerequisite owner: `plans/launch/DEAL_CLOSED_SUM_MODEL_PROGRESS.md`.
  PR #678 and platform-sync PR #694 are terminal on `main`. PR #633 consumes `DealDto` and the proven
  validated module-local factory pattern; no generated Application or operation-factory surface exists.
  After PR #633 delivers, the Deal ledger resumes for the compiler-exhaustive native-union and
  closed-Deal cut-over.
