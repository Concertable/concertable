# Code review — Refactor/launch_deal-lifecycle-modules-phase2

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `4285ff3bbcfe88317e75c149c3fb360e87b33b8e`  _(2026-09-06)_
**Security-reviewed up to commit:** `4285ff3bbcfe88317e75c149c3fb360e87b33b8e`  _(2026-09-06)_
**Judgment:** `approved`

## Legacy review history
## Coverage

- [x] Lifecycle contracts and domain foundation — 56 files — reviewed 2026-08-23 — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Contracts/` `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Domain/` `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Contracts/` `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Domain/` `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Contracts/` `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Domain/` `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Domain/` `api/Concertable.B2B/src/Modules/Artist/Concertable.B2B.Artist.Contracts/` `api/Concertable.B2B/src/Modules/Venue/Concertable.B2B.Venue.Contracts/` `api/Concertable.B2B/src/Modules/User/Concertable.B2B.User.Domain/` `api/Concertable.B2B/src/Modules/Deal/Concertable.B2B.Deal.Api/` `api/Concertable.Shared/tests/Concertable.Testing/`
- [x] Application and Opportunity implementations — 140 files — reviewed 2026-08-23 — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Application/` `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Infrastructure/` `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Api/` `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Application/` `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Infrastructure/` `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Api/`
- [x] Booking and supporting module implementations — 86 files — reviewed 2026-08-23 — `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Application/` `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Infrastructure/` `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Api/` `api/Concertable.B2B/src/Modules/Artist/Concertable.B2B.Artist.Application/` `api/Concertable.B2B/src/Modules/Artist/Concertable.B2B.Artist.Infrastructure/` `api/Concertable.B2B/src/Modules/Artist/Concertable.B2B.Artist.Api/` `api/Concertable.B2B/src/Modules/Venue/Concertable.B2B.Venue.Application/` `api/Concertable.B2B/src/Modules/Venue/Concertable.B2B.Venue.Infrastructure/` `api/Concertable.B2B/src/Modules/Venue/Concertable.B2B.Venue.Api/` `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure/` `api/Concertable.B2B/src/Modules/User/Concertable.B2B.User.Infrastructure/` `api/Concertable.B2B/src/Modules/Admin/Concertable.B2B.Admin.Infrastructure/` `api/Concertable.B2B/src/Seed/` `api/Concertable.B2B/src/Concertable.B2B.Web/` `api/Concertable.B2B/src/Concertable.B2B.Workers/`
- [x] Concert application and API — 111 files — reviewed 2026-08-23 — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Application/` `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Api/`
- [x] Concert infrastructure — 103 files — reviewed 2026-08-23 — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/`
- [x] Module-owned tests — 152 files — reviewed 2026-08-23 — `api/Concertable.B2B/src/Modules/Application/Tests/` `api/Concertable.B2B/src/Modules/Booking/Tests/` `api/Concertable.B2B/src/Modules/Opportunity/Tests/` `api/Concertable.B2B/src/Modules/Concert/Tests/` `api/Concertable.B2B/src/Modules/Artist/Tests/` `api/Concertable.B2B/src/Modules/Venue/Tests/` `api/Concertable.B2B/src/Modules/Deal/Tests/` `api/Concertable.B2B/src/Modules/Tenant/Tests/` `api/Concertable.B2B/src/Modules/User/Tests/` `api/Concertable.B2B/src/Modules/Admin/Tests/`
- [x] Host tests, topology, migrations, and plans — 48 files — reviewed 2026-08-23 — `api/Concertable.B2B/tests/` `api/Concertable.B2B/Concertable.B2B.slnx` `api/Concertable.B2B/Directory.Packages.props` `api/Concertable.slnx` `api/initial-migrations.ps1` `api/Concertable.Payment/provider-contract-inventory.json` `api/Concertable.Customer/TECH_DEBT.md` `plans/` `reviews/Refactor-launch_deal-lifecycle-modules-phase2.md`

## Review summary

All seven fixed-anchor areas and the required security layer are complete. At published head
`6ba7a13c5`, 24 findings remain open: 14 high, eight medium, and two low. Post-anchor work resolved 21
anchored findings; those findings are checked below and the incremental reconciliation is recorded in
the final section.

**Status 2026-08-25:** every finding above and every incremental finding (IR1–IR5) is now resolved on the
branch; no `[ ]` finding remains. The IR5 state-machine cutover is the last to close. This file stays as the
local gate's evidence through merge and is deleted immediately after PR #633 merges. Final closure — Shared/
Kernel and affected B2B build/carve, architecture/package guards, plan graph, `git diff --check`, exact-head
CI, and PR/remote head equality — is being run separately and is not asserted here.

## Cross-area notes

- ~~Booking and supporting module implementations: verify the combined dashboard metric is composed outside Application from Booking-owned status, without introducing an Application-to-Booking runtime dependency.~~ Checked at the anchor: that composition is absent and the resulting incorrect metric remains tracked by NAT4.
- ~~Concert infrastructure: remove the downstream implementation of `IApplicationAvailabilityProjection`; Application eligibility must consume an Application-owned projection updated by downstream facts, not query Concert synchronously.~~ Confirmed at the anchor: `ConcertAvailability` implements the Application contract over Concert's read context and is registered from Concert; MB2 owns the fix.
- ~~Concert infrastructure: verify Booking confirmation and Concert creation enlist in the same ambient transaction, and that Concert notifications/outbox effects cannot escape a failed Booking confirmation.~~ Confirmed at the anchor: Concert saves independently and sends direct notifications before the Booking save completes. NAT10 owns the cross-context transaction fix and NAT17 owns the escaping notification.
- ~~Concert infrastructure: make cancellation from `SettlementFailed` return `CancelConcertError.InvalidState` rather than passing through to `ConcertEntity.BeginCancellation` and throwing.~~ Confirmed and tracked by NAT15.
- ~~Module-owned tests: add regressions for multi-row DTO/response mapping, confirmed-booking dashboard counts, applying to filled opportunities, removing referenced opportunities, missing VenueHire payment methods, failed-accept auxiliary state, exact Application and Booking Deal strategy coverage, cancellation with no escrow before and after Concert creation, a rejection arriving during cancellation, cancellation from `SettlementFailed`, declaring door revenue during cancellation, post-cancellation notification rollback, truly concurrent Accept/payment arrival, cross-context rollback during confirmation, and retained DoorSplit/Versus Invoice creation.~~ At the anchor these gaps remain absent or only partially characterized and are owned by the existing production findings NAT3–NAT17 and MB1–MB5. The post-anchor reconciliation closes the repaired items and leaves the remaining gaps open below.

## Findings

## Lifecycle contracts and domain foundation — reviewed 2026-08-23

- [x] **NAT1 — HIGH — correctness** — `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Domain/Entities/BookingEntity.cs:121`
  A refund rejection leaves the booking in `CancellationFailed`, but `BeginCancellation` rejects that state, so `BookingService.CancelAsync` retries crash instead of issuing another refund; allow `CancellationFailed`, assign a fresh `CancellationOperationId` for the retry, and cover the rejected-refund retry path.
- [x] **NAT2 — HIGH — correctness** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Domain/Entities/ConcertEntity.cs:156`
  A Concert cancellation retry reuses the rejected operation ID, causing Payment's terminal-operation replay to return the same rejection forever; assign a fresh operation ID when beginning cancellation from `CancellationFailed` and cover the rejected-refund retry path.

The parallel module-local state-machine slice was not present at the anchored commit. Recheck both findings
against that incoming delta before fixing or closing them.

No security issues were found in this area.

## Application and Opportunity implementations — reviewed 2026-08-23

- [x] **NAT3 — HIGH — correctness** — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Application/Mappers/ApplicationMapper.cs:66`
  List mapping starts per-item module reads with `Task.WhenAll`, so multiple items concurrently operate on the same scoped Artist, Opportunity, Venue, Deal, or Booking EF contexts and can throw EF's second-operation exception; replace the fan-outs in this mapper, `ApplicationResponseMapper`, `OpportunityMapper`, and `OpportunityDashboardService` with batch Contracts operations/read shapes before mapping.
- [x] **NAT4 — HIGH — correctness / module boundary** — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Infrastructure/Services/ApplicationDashboardService.cs:43`
  `AcceptedAwaitingCheckout` counts every upcoming checkout-capable Application forever because Application now stops at `Accepted` while Booking alone owns confirmation; move this combined metric to a B2B host/query composition that reads Application and Booking through their Contracts and counts only Booking's awaiting/failure states, without adding an Application-to-Booking dependency.
- [x] **NAT5 — HIGH — correctness** — `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Infrastructure/Services/OpportunityHandoffService.cs:22`
  The handoff returns Filled opportunities with no availability signal and Application treats every returned detail as applyable, so a direct POST can create and notify an Application after the Opportunity has been claimed; add an Open-only Contracts operation (while retaining the general details read for rendering) and require it in apply eligibility and creation.
- [x] **NAT6 — HIGH — correctness / data integrity** — `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Infrastructure/Services/OpportunityService.cs:123`
  Omitting an open Opportunity from the desired list lets `CollectionSyncer` physically delete it, but the carved Application table intentionally has only a scalar `OpportunityId`, so the delete succeeds and leaves orphan Applications that fail mapping; model removal as an Opportunity-owned closed/withdrawn state and retain the row instead of introducing a backward Application query.
- [x] **NAT7 — MEDIUM — correctness** — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Infrastructure/Services/ApplicationService.cs:196`
  A VenueHire apply request with the optional `PaymentMethodId` omitted or blank throws `InvalidOperationException` and returns 500; return an operation-owned typed payment-method/unsupported-deal failure as the pre-carve path did.
- [x] **MB1 — HIGH — module boundary / plan conformance** — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Application/Interfaces/IDealTermsRenderer.cs:14`
  Application still exposes the plan-rejected generic `IStepResolver<TStep>`, registers unvalidated keyed lookups, and forces heterogeneous Accept variants behind one optional-parameter `IAcceptStep`; replace terms with the validated module-local Deal strategy factory and Accept with the honest standard/prepaid method-header interfaces plus dedicated factory required by the ownership plan, including exact `DealType` composition coverage.
- [x] **MB2 — HIGH — module boundary** — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Infrastructure/Validators/ApplicationValidator.cs:10`
  Application eligibility synchronously calls `IApplicationAvailabilityProjection`, whose implementation is Concert's real read context, so the carve retains the forbidden upstream Application-to-Concert runtime query behind a renamed interface; make the projection Application-owned and update it from downstream facts, then remove Concert's implementation.
- [x] **MB3 — HIGH — module boundary** — `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Infrastructure/Services/OpportunityDashboardService.cs:14`
  Opportunity directly consumes Application dashboard metrics, reversing the plan's Application-to-Opportunity dependency and creating a runtime module cycle; move the combined Opportunity/Application dashboard and match-exclusion query to the explicit B2B host/query composition layer and keep Opportunity's own facade stage-local.
- [x] **BUG1 — MEDIUM — correctness** — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Infrastructure/Services/ApplicationService.cs:320`
  `BeginAcceptance` mutates `AcceptanceOperationId` before the keyed step and atomic claim can return typed failures, while `UnitOfWorkBehavior` saves unconditionally even when the Result is failure, so a rejected accept persists auxiliary transition state; validate the selected method and claim first, then mint and persist the operation id only on the success path.
- [x] **SEED1 — MEDIUM — seeding** — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Infrastructure/Data/Seeders/SeededApplicationSigner.cs:24`
  The seeder mutates the singleton `SeedState` Applications and stamps them with the current clock, violating the seeding rule that seed state is constructor-built and deterministic while seeders only persist; construct fully signed deterministic seed Applications through the owning seed factory/catalog and remove the mutation pass.
- [x] **CV1 — LOW — C# conventions** — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Application/Mappers/ApplicationMappers.cs:8`
  New extension containers use legacy `this` receiver parameters in `ApplicationMappers`, both module Infrastructure `ServiceCollectionExtensions`, and `QueryableOpportunityExtensions`; migrate each complete changed container to C# 14 `extension()` blocks as required by the routed C# style standard.

No additional security review was required for this stage because the shared security marker already covers the exact plan anchor.

## Booking and supporting module implementations — reviewed 2026-08-23

- [x] **NAT8 — HIGH — API compatibility / correctness** — `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Api/Controllers/BookingController.cs:11`
  Cancellation moved to `POST /api/booking/{bookingId}/cancel`, but the shipped B2B client still posts an Application id to `/application/{applicationId}/cancel` (`app/web/b2b/shared/src/features/concerts/api/applicationApi.ts:95`), so venue cancellation now returns 404; migrate the client to the Booking action link/id in the same change or retain a compatible Application-edge adapter until consumers are cut over.
- [x] **NAT9 — HIGH — correctness / convergence** — `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Infrastructure/Services/BookingService.cs:89`
  Cancelling every awaiting or failed Booking sends `RefundEscrowCommand`, including DoorSplit/Versus bookings and rejected FlatFee/VenueHire bookings with no escrow; Payment responds with `RefundEscrowDeferredEvent`, which B2B has no handler for, leaving the Booking permanently `CancellationPending`. A financial rejection racing after cancellation also reaches `RecordFailedAsync` and throws because the domain only permits rejection from confirmation states. Implement the Booking-owned, Deal/financial-state-specific cancel steps required by the plan: cancel no-escrow cases immediately and let a late rejection complete cancellation, with real deferred/rejection arrival regressions rather than an artificial refund-success event.
- [x] **NAT10 — HIGH — correctness / transactionality** — `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Infrastructure/Events/AcceptanceFinancialOperationOutcomeProcessor.cs:123`
  The external financial-success path uses only the outbox behavior, not Booking's ambient `IUnitOfWorkBehavior`; `RecordSucceededAsync` therefore dispatches `BookingConfirmedDomainEvent` and Concert saves its new row before Booking's implicit save transaction starts. Once the pre-commit handler registration is active, a later Booking save failure can leave a Concert for an unconfirmed Booking. Wrap inbox handling, Booking confirmation, and its pre-commit cross-context work in the Booking unit of work and add a rollback regression that proves neither context commits independently.
- [x] **NAT11 — HIGH — correctness / concurrency** — `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Infrastructure/Events/VerifyPaymentSucceededHandler.cs:20`
  The two-signal join handles either sequential arrival order but loses a genuinely concurrent payment arrival: payment can read before Accept commits, find no Booking and return, while Accept already captured a snapshot with no verification, leaving the new Booking awaiting forever. Serialize the Application acceptance/payment evidence transition or introduce a durable replayable join so the second committer always observes and advances the other signal, then cover the overlapping-transaction interleaving.
- [x] **MB4 — HIGH — module boundary / plan conformance** — `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Application/Interfaces/IConfirmStep.cs:13`
  Booking retains the plan-rejected generic `IStepResolver<TStep>` and resolves raw keyed registrations through `IKeyedServiceProvider`, with no composition check for exact `DealType` coverage; replace it with the validated module-local Booking strategy factory/builder and honest `IConfirmStep`/`ICancelStep` families required by the ownership plan.
- [x] **MB5 — MEDIUM — persistence stance** — `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Infrastructure/Data/BookingDbContext.cs:9`
  Booking's two-party `IVenueArtistTenantScoped` rows are hosted by the single-owner `TenantScopedDbContext` capability and manually apply venue/artist filters; inherit `VenueArtistTenantScopedDbContext` so the context advertises and receives the correct persistence capability instead of relying on an equivalent-looking implementation behind the wrong stance.
- [x] **SEED2 — HIGH — seeding / behavioural correctness** — `api/Concertable.B2B/src/Seed/Concertable.B2B.Seed.Infrastructure/Factories/BookingFactory.cs:8`
  The carved Booking seed factory populates only `Id` (and sometimes `PaymentMethodId`), while `SeedState` later patches only operation/application/opportunity/artist/deal/tenants; all 47 seeded Bookings retain default `AwaitingConfirmation`, zero venue/dates/terms/financial operation, and the Booking seeder never inserts their Contract snapshots even though many Applications and Concerts are seeded as booked/posted/finished. Build complete deterministic Booking and Contract aggregates from the canonical accepted handoffs in `SeedState`, and remove the seeder-time `LinkBookingsToPersistedApplications` mutation.
- [x] **CV2 — LOW — C# conventions** — `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Application/Mappers/BookingMappers.cs:8`
  New or edited extension containers still use legacy `this` receiver parameters in `BookingMappers`, `ContractMappers`, Artist/Venue API `ServiceCollectionExtensions`, and B2B Workers `ServiceCollectionExtensions`; migrate each complete changed container to C# 14 `extension()` blocks as required by the routed C# standard.
- [x] **CV3 — LOW — C# conventions** — `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Api/Controllers/BookingController.cs:8`
  The new controller captures its service through a primary constructor instead of the required explicit `private readonly` field and constructor assignment; use the repository's collaborator form.

The anchor's missing-dispatch defect—pre-commit handlers registered only under `IPreCommitDomainEventHandler<T>` while the dispatcher resolves `IDomainEventHandler<T>`—was confirmed and corrected by post-anchor commit `3b6b689c7`; the incremental review found no regression in that repair.

No additional security issues were found in this area.

## Concert application and API — reviewed 2026-08-23

- [x] **NAT12 — HIGH — correctness** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Api/Extensions/ServiceCollectionExtensions.cs:14`
  The carve removes Concert.Api's only FluentValidation assembly scan when `ApplyRequestValidator` moves, while retaining the Concert update/door-revenue validators and adding the self-billing agreement/signature validators. None of those validators are registered, so invalid Concert requests bypass HTTP validation, an empty legal signature can be persisted, and a missing signature can reach the service as null and return 500 instead of the existing 400 contract; register the complete Concert.Api validator assembly from a surviving Concert validator type with internal types included.
- [x] **CV4 — LOW — typed-result conventions** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Application/Errors/CancelConcertError.cs:25`
  `EscrowRefundFailure` remains in the operation-owned error union even though the new `ICancelStep` returns only `Task` and the asynchronous refund outcome cannot produce that case; remove the unreachable case and its definition-contract expectation so the failure set remains exactly the outcomes `CancelAsync` can return.
- [x] **CV5 — LOW — C# conventions** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Application/Mappers/ConcertMappers.cs:10`
  The changed `ConcertMappers` and Concert.Api `ServiceCollectionExtensions` containers still use legacy `this` receiver parameters; migrate each complete changed container to C# 14 `extension()` blocks as required by the routed C# style standard.
- [x] **CV6 — LOW — naming / simplification** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Application/Interfaces/IConcertRepository.cs:12`
  `GetByIdForLifecycleAsync` names a repository query for its caller's use case and duplicates the inherited `GetByIdAsync` with the same unadorned id predicate; remove the redundant method and use `GetByIdAsync`, keeping repository names literal to their query as required by the routed naming standard.

No additional security review was required for this stage because the shared security marker already covers the exact plan anchor.

## Concert infrastructure — reviewed 2026-08-23

- [x] **NAT13 — HIGH — correctness / financial integrity** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/Executors/CompleteExecutor.cs:84`
  Invoice issuance is now conditional on the step leaving Concert immediately `Complete`; DoorSplit and Versus instead leave it `AwaitingSettlement`, and the settlement-success processor never issues an Invoice, so both existing invoice cases lose their legally material snapshot. Issue the Invoice after every successful completion step as the pre-carve flow did, while retaining the existing deferred-before-payment guards and BookingId uniqueness.
- [x] **NAT14 — HIGH — correctness / convergence** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs:150`
  DoorSplit and Versus Concert cancellation use the same `RefundEscrowCancelStep` as escrow-backed deals even though those Bookings hold no escrow, and Concert no longer handles `RefundEscrowDeferredEvent`; the real Payment outcome therefore leaves the Concert permanently `CancellationPending` while the current test manufactures a refund-success outcome. Register an immediate-cancel step for the no-escrow Deal cases and retain refund completion for FlatFee/VenueHire, then drive the real deferred/no-escrow outcome in integration coverage.
- [x] **NAT15 — MEDIUM — correctness / typed failure** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/Executors/CancelExecutor.cs:39`
  `SettlementFailed` is omitted from the executor's invalid-state guard, so cancellation passes into `BeginCancellation`, throws, and returns 500 instead of the operation-owned `CancelConcertError.InvalidState`; reject `SettlementFailed` before selecting the cancel step and cover the HTTP result.
- [x] **NAT16 — MEDIUM — correctness** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/ConcertService.cs:224`
  Door revenue is rejected only after settlement states, so a direct request can still mutate a Concert in `CancellationPending`, `CancellationFailed`, or `Cancelled`; require the Concert to remain `Draft` or `Posted` before calling `DeclareDoorRevenue` and return the existing stable operation failure for every terminal/cancellation state.
- [x] **NAT17 — HIGH — correctness / transactionality** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/ConcertService.cs:91`
  Concert creation sends both SignalR notifications directly from the pre-commit Booking handler after Concert's nested save, so a later email/outbox or Booking save failure can roll back both database contexts while users have already received a Concert id that does not exist. Stage the notification through an outbox-backed message and deliver it only after the shared confirmation transaction commits; keep email staging in that same transaction and prove a forced rollback emits neither notification nor email.
- [x] **SEED3 — MEDIUM — seeding** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Data/Seeders/ConcertDevSeeder.cs:41`
  Both Concert seeders call `SeedState.LinkConcertsToPersistedBookings()` to reflection-mutate the singleton Concert aggregates immediately before persistence, violating the seeding rule that seed state is constructor-built and seeders only persist it. Construct the final ApplicationId/BookingId relationship in the deterministic seed factory/catalog and delete the mutation method and both seeder calls.
- [x] **CV7 — LOW — C# conventions** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs:48`
  The changed Infrastructure `ServiceCollectionExtensions` and all three changed `Queryable*Mappers` containers retain legacy `this` receiver methods; migrate each complete container to C# 14 `extension()` blocks as required by the routed C# style standard.
- [x] **CV8 — LOW — C# conventions** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/ConcertSteps.cs:8`
  The three new Concert step implementations capture collaborators through primary constructors, and the edited completion runner retains the same captured-dependency form; replace each with explicit `private readonly` fields and `this.`-qualified constructor assignments as required by the routed C# style standard.

The Concert-owned half of the combined dashboard confirms NAT4/MB3: `ConcertDashboardService` still composes Application and Opportunity metrics inside a lifecycle module rather than the explicit B2B query composition. The downstream `IApplicationAvailabilityProjection` implementation confirms MB2. Those existing findings own the fixes and are not duplicated here.

The Booking-confirmation handler registration defect at the anchor is corrected by post-anchor commit `3b6b689c7`; no other post-anchor Concert Infrastructure drift was included in this stage.

No additional security review was required for this stage because the shared security marker already covers the exact plan anchor.

## Module-owned tests — reviewed 2026-08-23

- [x] **NAT18 — HIGH — correctness / test validity** — `api/Concertable.B2B/src/Modules/Booking/Tests/Concertable.B2B.Booking.IntegrationTests/ApplicationFinancialOperationApiTests.cs:29`
  The test was moved into Booking unchanged after the carve deleted the only production `GET /api/application/{id}/financial-operation` endpoint. Its pre-operation case now passes accidentally on an unhandled-route 404 and its pending/rejected case must receive 404 instead of the asserted 200, so this suite no longer protects the financial-failure API contract. Exercise a Booking-owned public API/Contracts response and assert the public `BookingStatus` mapping and failure facts, or restore an explicit compatibility endpoint if that route remains shipped.
- [x] **NAT19 — HIGH — correctness / financial assertion** — `api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.IntegrationTests/Concert/ConcertCancelApiTests.cs:49`
  The FlatFee and VenueHire cancellation tests replace exact `booking.Id == refund.BookingId` checks with `BookingId > 0`, so they pass when Concert refunds an unrelated Booking. Compare the command to the owning Concert row's persisted `BookingId` (already available through `fixture.Concerts`) or move the cross-module journey to Process tests and resolve the expected id through Booking's public boundary; retain exact equality in both cases.
- [x] **MB6 — HIGH — module boundary / test topology** — `api/Concertable.B2B/src/Modules/Booking/Tests/Concertable.B2B.Booking.IntegrationTests/ContractApiTests.cs:27`
  The contract suite initiates Opportunity creation and Application acceptance, then verifies Booking's private Booking/Contract persistence at `GetContractAsync` (`:302`). That is a complete cross-module journey inside a module-owned integration project, contrary to the corrected topology. Either initiate Booking through its public Application-contract fact and keep Booking-owned persistence assertions, or move the full journey to `Concertable.B2B.Process.IntegrationTests` and assert the Contract through Booking's public boundary.
- [x] **NAT20 — MEDIUM — test coverage** — `api/Concertable.B2B/src/Modules/Venue/Tests/Concertable.B2B.Venue.IntegrationTests/TenantScopingTests.cs:43`
  `GetAllByTenantId_ReturnsOnlyThatTenantsVenues` no longer invokes `IVenueRepository.GetAllByTenantIdAsync`; it duplicates the tenant predicate directly over `fixture.Venues`, so the named repository behaviour can regress while this test remains green. Resolve the Venue-owned repository in the module fixture/scope and exercise the real operation, or rename and re-home the test if it is intended to cover only the read-context stance.
- [x] **CV9 — MEDIUM — test-tier conventions** — `api/Concertable.B2B/src/Modules/Opportunity/Tests/Concertable.B2B.Opportunity.UnitTests/Services/OpportunityDashboardServiceTests.cs:23`
  The carved UnitTests projects retain application-service and handler orchestration tests built from many mocked runtime collaborators: this file has seven mocks and a per-test `CreateService`, while `ApplicationCounterpartyNotifiedDomainEventHandlerTests`, `BookingServiceTests`, and `VerifyPaymentConvergenceTests` similarly mock module facades, repositories, buses, or handlers. The routed unit-test standard reserves UnitTests for pure domain/application logic and makes real-host integration the default for collaborator orchestration; move those behaviours to the owning module IntegrationTests through HTTP, Contracts, or event boundaries and retain only pure state/value tests in UnitTests.

At the anchor, Booking cancellation tests manufacture refund-success outcomes for no-escrow and
failed-confirmation cases, and the Artist dashboard test stops before Booking confirmation. Post-anchor
commit `6ba7a13c5` resolves the Booking convergence defect; NAT4 remains open.

No additional security review was required for this stage because the shared security marker already covers the exact plan anchor.

## Host tests, topology, migrations, and plans — reviewed 2026-08-23

- [x] **NAT21 — HIGH — correctness / cross-module convergence** — `api/Concertable.B2B/tests/Concertable.B2B.Process.IntegrationTests/CancellationJourneyTests.cs:22`
  The new process suite requires Booking cancellation to notify the artist and reopen the Opportunity (`:43-48`), and Concert cancellation to reopen it (`:99`), but `BookingEntity.Cancel` only changes local state (`BookingEntity.cs:139`) and raises no public fact. Concert does publish `ConcertCancelledEvent`, but the branch has no consumer for it. Both journeys therefore stop at their owning aggregate instead of converging the Application notification and Opportunity projection. Publish immutable cancellation facts from Booking and Concert and handle them in the owning Application/Opportunity modules through Contracts events; retain these boundary-based process assertions.
- [x] **SEED4 — MEDIUM — seeding / test validity** — `api/Concertable.B2B/src/Seed/Concertable.B2B.Seed.Infrastructure/SeedState.cs:300`
  `FreshVenueHireOpportunity` is assigned by positional index `opps[62]`, but that entry is the expired `(1, -40)` specification. Exact-head CI consequently fails `GetActiveByVenueId_ShouldReturnSeededOpportunity` at `OpportunityApiTests.cs:199` because an active query correctly omits it. Construct or select the named seed from stable semantic inputs as an upcoming VenueHire Opportunity instead of relying on the list position.
- [x] **CV10 — MEDIUM — test-tier conventions** — `api/Concertable.B2B/tests/Concertable.B2B.Workers.UnitTests/Functions/ConcertFinishedFunctionTests.cs:14`
  `ConcertCompletionRunnerTests` mocks the repository, scoped executor, executor, and logger and verifies collaborator calls, while `ConcertFinishedFunctionTests` is another mock-delegation test. The routed unit-test standard excludes runtime collaborator orchestration from UnitTests. Cover the timer/runner wiring through the real B2B worker host and persistence boundary, retaining only pure completion logic in a UnitTests project.
- [x] **CV11 — MEDIUM — solution metadata / validation closure** — `api/Concertable.slnx:64`
  The umbrella solution says it loads every project, but its Application, Booking, Opportunity, and Deal test folders omit their new IntegrationTests projects, and `/B2B/Tests/` at `:297` omits `Concertable.B2B.Process.IntegrationTests`. The service solution includes all five, so the two solution inventories disagree and an umbrella build or IDE load silently skips the new topology. Add the five project entries to `api/Concertable.slnx`.
- [x] **CV12 — LOW — integration-test fixture conventions** — `api/Concertable.B2B/tests/Concertable.B2B.IntegrationTests.Fixtures/ApiFixture.cs:244`
  Both single-context outbox lookup overloads create scopes manually (`:246` and `:256`) even though each resolves only `OutboxDbContext`. The routed integration-test standard reserves manual scopes for several distinct scoped services in one lifetime and uses `IScoped<T>.RunAsync` for one context. Keep `Services` available, but route these two helpers through the scoped abstraction.
- [x] **CV13 — LOW — integration-test seed expectations** — `api/Concertable.B2B/tests/Concertable.B2B.Process.IntegrationTests/BookingConfirmationEmailJourneyTests.cs:9`
  The process test invents the seeded legal address as a private string literal rather than deriving its expectation from `fixture.SeedState`, so changing the canonical seed can break the test for the wrong reason. Include the legal address in `SeedTenantSnapshot` and assert the snapshot value.

The module-specific fixture topology, shared host-neutral fixture, architecture guard, B2B service
solution, migration script, package inventory, test-tier traits, collection metadata, deleted temporary
TECH_DEBT item, and AGENTS/CLAUDE sibling pairs otherwise match the requested ownership boundaries. No
module integration project directly references another module's Domain or Infrastructure assembly, and
the process project asserts module effects only through HTTP/Contracts boundaries.

No security issues were found in this area.

## Post-anchor incremental review — reviewed 2026-08-23

Commits `c50469d48..6ba7a13c5` were reviewed for drift against the completed fixed-anchor findings.
They resolve NAT3, NAT5-NAT9, MB5, NAT12-NAT16, NAT19, and CV1-CV8; the corresponding checkboxes above
are closed. The handler-registration repair in `3b6b689c7`, batch and lifecycle boundary repairs in
`70f470299`, Concert financial fixes in `69130696f`, and Booking cancellation convergence in
`6ba7a13c5` do not introduce another finding beyond NAT21 and SEED4.

Exact published-head CI run `32652056483` is red. Build, every service carve, B2B architecture tests,
and workflow tests passed. The Opportunity unit shard fails because the mock-heavy test covered by CV9
does not set up the new batch profile read; the Opportunity integration shard fails on the expired
`FreshVenueHireOpportunity` covered by SEED4. Fail-fast cancellation prevented the remaining
integration matrix, including Process tests, from providing behavioural evidence. No local integration
or E2E suite was run during review. Concurrent uncommitted state-machine work after `6ba7a13c5` was not
included in the incremental range.

## Incremental review — 2026-08-25

Range `c50469d483f697890dc9b4f3d2b3013ee1b8c1c9..b61fc7feb2033047e69fd44896646ec85b6e4262`
was reviewed through the native general and required security layers. Five findings were raised; all five are
now resolved. IR2 (`d1c5d252b`), IR3 (`05a685317`), and IR4 (`090308c04`) landed after the reviewed range;
IR5's state-machine cutover is included in the current candidate. A fresh incremental review over those fix
commits is part of final closure and is not asserted here.

- [x] **IR1 — HIGH — messaging correctness** — `api/Concertable.B2B/src/Concertable.B2B.Web/B2BWebHostExtensions.cs:177`
  `ConcertService` durably stages `NotifyConcertDraftCreatedCommand` and its handler is registered in DI,
  but the production message registry does not handle the command, so Azure Service Bus creates no receiver
  for it and the post-commit notification is never delivered. Register the command in the production topology
  and mechanically cover the registry.
- [x] **IR2 — HIGH — messaging correctness / idempotency** — `api/Concertable.B2B/src/Concertable.B2B.Web/B2BWebHostExtensions.cs:174`
  `BookingCancelledEvent`, `ConcertCancelledEvent`, and `ConcertCreatedEvent` are locally dispatched when
  published and are also subscribed back to B2B through the broker. The broker delivery has a different
  message id, so inbox deduplication cannot prevent the local handlers running twice. Remove the three
  self-subscriptions while retaining their publish registrations and local handlers, and cover the topology.
- [x] **IR3 — HIGH — tenant isolation / correctness** — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Infrastructure/Services/ApplicationAvailabilityProjection.cs:8`
  Acceptance checks Concert availability through tenant-filtered `ApplicationDbContext`. An accepting venue
  therefore cannot see the target artist's conflicting Concert at another venue and can double-book the artist.
  Query the Application-owned projection through `IApplicationReadDbContext` and cover the cross-venue case.
- [x] **IR4 — HIGH — financial integrity / concurrency** — `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Infrastructure/Services/BookingService.cs:121`
  Cancellation and both financial-outcome paths read Booking without an update lock or concurrency retry.
  Concurrent operations can both observe `AwaitingConfirmation`, then overwrite one another so a confirmed
  Booking is refunded with its Concert intact or a cancellation is overwritten without compensation. Serialize
  all three transition reads and prove both commit orders with deterministic overlap tests.
- [x] **IR5 — HIGH — plan conformance / lifecycle correctness** — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Domain/Entities/ApplicationEntity.cs:96`
  The ledger claims PR #633 adopted the published Kernel state machine, but Application, Booking, and Concert
  still enforce transitions with hand-written guards and direct state assignments, and no B2B source consumes
  `IStateMachine`. Implement the documented module-local `Lifecycle.State`, `Trigger`, and `StateMachine`
  definitions, funnel aggregate mutation through the private Result-based transition helper, add exhaustive
  edge and no-mutation tests, and add the mechanical assignment guard.
  Resolved: Application, Booking, and Concert each own `Domain/Lifecycle/{State,Trigger,StateMachine}.cs` with
  a module-local `internal sealed class StateMachine : IStateMachine<State, Trigger>` backed by the published
  Kernel `StateMachine<State, Trigger>` frozen table. Each aggregate funnels every mutation through a private
  `Transition(Trigger)` helper that assigns `State` only from the success value; operation errors carry
  `InvalidTransition(TransitionError<State, Trigger>)`. `StateMachineTests` enumerate every state/trigger pair,
  `*EntityLifecycleTests` prove a rejected transition leaves state, auxiliary facts, and events unchanged, and
  `LifecycleStateOwnershipTests` mechanically fails any `State` assignment outside the private transition path.
  The old combined `LifecycleState`, per-`DealType` `LifecycleStateMachine`, `IConcertStateMachineRegistry`, and
  `ILifecycleTransitioner` no longer exist in source.

- [x] **IR6 — HIGH — messaging provisioning/correctness** — `api/Concertable.B2B/src/Concertable.B2B.Hosting/B2BTopology.cs:17`
  The runtime registry publishes `BookingCancelledEvent`, `ConcertCancelledEvent`, and `ConcertCreatedEvent`
  and handles `NotifyConcertDraftCreatedCommand`, but the Aspire topology provisions none of those topics and
  no notification-command queue. Add the missing entities and enforce registry/provisioning parity in the
  shared topology tests.
  Resolved: `AddB2BTopology` now provisions all three event topics and both B2B command queues, with the
  Booking contracts reference owned by the composition project. `ServiceTopologyTests` enforces the complete
  topic and queue inventory.

- [x] **IR7 — HIGH — Booking financial concurrency** — `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Infrastructure/Events/VerifyPaymentSucceededHandler.cs:21`
  Verify-payment handlers track a Booking before the later update-lock query, allowing EF to return stale
  state and recreate the cancellation/confirmation race. Resolve only the Booking id before the locked
  transition and add handler-level overlap coverage.
  - Resolved by projecting only the Booking id before entering the locked transition and compiling a
    deterministic cancellation/payment-confirmation overlap test through the real pre-commit handler.

- [x] **IR8 — HIGH — Application lifecycle concurrency** — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Infrastructure/Services/ApplicationService.cs:284`
  Accept pre-tracks the Application before its lock, while Withdraw and Reject use unlocked lifecycle reads.
  Acquire the lifecycle lock before validation for all three transitions and prove deterministic overlap
  convergence.
  - Resolved by loading the Application through one update-lock repository boundary before acceptance
    validation and by wrapping Withdraw and Reject in the same serialized unit-of-work path. Deterministic
    Accept/Withdraw and Accept/Reject queue-order tests compile against the real HTTP operations.

- [x] **IR9 — HIGH — Concert financial concurrency/idempotency** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/Executors/CompleteExecutor.cs:49`
  Concert Cancel and Complete use unlocked reads, and settlement performs external money movement without a
  durable operation identity before saving local state. Serialize the lifecycle operations, persist a stable
  settlement identity before the provider call, use it as the provider idempotency key, and prove overlap and
  post-provider retry convergence.

- [x] **IR10 — MEDIUM — messaging subscription parity** — `api/Concertable.B2B/src/Concertable.B2B.Hosting/B2BTopology.cs:41`
  B2B provisions a `RefundEscrowDeferredEvent` subscription but registers no runtime receiver or handler.
  Remove the orphan subscription when deferred refund is intentionally a no-op, or implement the convergence
  handler and enforce runtime/provisioning parity.
## Review pass — 2026-08-27 — incremental

**Candidate base:** `b61fc7feb2033047e69fd44896646ec85b6e4262`
**Candidate head:** `3f6d85aaa53914a9de56ecb05245ccb1e1f1507e`
**Candidate branch:** `Refactor/launch_deal-lifecycle-modules-phase2`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:82383470ebc2daeadfe475b9d356db3486f16aff138dcd47cd975016f0b0484c` `(133 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-review-633-3f6d85aaa`
**Candidate bundle identity:** `sha256:1ad0047989583ca3a775ad3096a964bf1d7d6e02ced6df1c39c7cccb6aa5dfc0`
**Work-order path:** `reviews/Refactor-launch_deal-lifecycle-modules-phase2.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No new findings.

## Review pass — 2026-08-29 — test-coverage closeout

**Candidate base:** `04b01f9c90514cacc0b7a362880e0316ef85762e`
**Candidate head:** `04b01f9c90514cacc0b7a362880e0316ef85762e`
**Candidate branch:** `Refactor/launch_deal-lifecycle-modules-phase2`
**Candidate scope:** `the six test-coverage findings below (OPP2, DASH2, CAPP2, CTEST1, CTEST2, FOUND3)`
**Work-order path:** `reviews/Refactor-launch_deal-lifecycle-modules-phase2.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

Direct remediation pass (test-coverage findings supplied outside this file), not a dispatched native/lens
review — no candidate bundle materialized.

### Findings

- [x] **OPP2 — MEDIUM — test coverage** — `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Infrastructure/Events/OpportunityCancellationIntegrationEventHandler.cs`
  `OpportunityCancellationIntegrationEventHandler` had zero tests for either `IIntegrationEventHandler<BookingCancelledEvent>` or `IIntegrationEventHandler<ConcertCancelledEvent>`.
  Resolved: `OpportunityCancellationIntegrationEventHandlerTests.cs` (new, `Concertable.B2B.Opportunity.IntegrationTests`) publishes a `BookingCancelledEvent` against a Filled seeded opportunity and asserts it reopens, and separately asserts a replayed `ConcertCancelledEvent` with the same `MessageId` is a no-op (opportunity stays `Filled`).

- [x] **DASH2 — MEDIUM — test coverage** — `api/Concertable.B2B/src/Modules/Dashboard/Opportunity/Concertable.B2B.Dashboard.Opportunity.Infrastructure/OpportunityDashboardService.cs:47`
  `OpportunityDashboardService.GetOpenAsync`'s `OpportunityDashboardError.MissingVenue` branch was untested.
  Resolved: `OpportunityDashboardServiceTests.GetCurrentForVenue_MissingVenue_ReturnsTypedProblem` (new, `Concertable.B2B.Dashboard.Opportunity.UnitTests`, with a new `InternalsVisibleTo` from `Concertable.B2B.Dashboard.Opportunity.Infrastructure`) drives the service directly with `ITenantContext.TenantId` unset. Note: unlike the Artist path (`GetRecommendedAsync`, which explicitly re-checks `IArtistModule.GetCurrentProfileAsync()`), `GetOpenAsync` only checks `TenantId` nullity — it never re-checks the caller's own Venue profile. A `VenueManagerNoVenue`-style HTTP caller (bare Venue-typed tenant, no `VenueEntity` row) still resolves a non-null `TenantId` and would get a 200 with an empty list rather than `MissingVenue`. That parity gap is a production question, not a test gap — flagging it rather than fixing it here.

- [x] **CAPP2 — MEDIUM — test coverage** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/ConcertService.cs` and `Services/Executors/CancelExecutor.cs`
  Concert's `TrySaveChangesAsync` → `*Error.Superseded` contract was untested for Cancel, Post, Update, and DeclareDoorRevenue.
  Resolved: `ConcertWorkflowTests.CancelAsync_SaveRaceLost_ReturnsSuperseded` plus `ConcertServiceTests.{UpdateAsync,PostAsync,DeclareDoorRevenueAsync}_SaveRaceLost_ReturnsSuperseded` (all `Concertable.B2B.Concert.UnitTests`), each forcing `TrySaveChangesAsync` to return false and asserting the matching `Superseded` case.

- [x] **CTEST1 — MEDIUM — test coverage** — `api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.UnitTests/Executors/CancelExecutorTests.cs`
  Only covered the cancellation-token rethrow path.
  Resolved: added concert-not-found, rejected-transition, and successful-path cases, mirroring `CompleteExecutorTests.cs`'s coverage of the sibling executor.

- [x] **CTEST2 — MEDIUM — test coverage** — `api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.UnitTests/Services/ConcertServiceCreateTests.cs`
  Never tested the duplicate-Concert guard in `ConcertService.CreateAsync`.
  Resolved: `CreateAsync_ExistingConcertForBooking_DoesNotAddOrSave` stubs an existing `ConcertEntity` for the booking and asserts `CreateAsync` neither adds nor saves a duplicate.

- [x] **FOUND3 — MEDIUM — test coverage** — `api/Concertable.DataAccess/Concertable.DataAccess.Infrastructure/Data/DomainEventDispatchInterceptor.cs:53`
  The `SaveChangesFailedAsync` stack-balance fix (`pendingEventsStack.TryPop(out _)`) had no test.
  Resolved: `DomainEventDispatchInterceptorTests` (new, `Concertable.DataAccess.UnitTests`, SQLite in-memory — no Docker) forces a real `SaveChangesAsync` failure (a unique-constraint `DbUpdateException`, not a concurrency conflict — see note), then a successful retry, and asserts via reflection that `pendingEventsStack.Count` returns to 0 after the failure and again after the retry, and that only the retry's domain events are dispatched. Verified as a genuine regression test by reverting the fix locally and confirming the test fails (stack count 1, not 0).
  Note: empirically confirmed (EF Core 10, both the InMemory and SQLite providers) that `DbUpdateConcurrencyException` never invokes `SaveChangesFailedAsync` — only a plain `DbUpdateException` does. `ConcurrencyConflictInterceptor`, named in the original finding as the tool to "force exactly this," cannot exercise this code path; production's own concurrency retries go through `DbContextExtensions.TrySaveChangesAsync`'s direct try/catch, which never touches this interceptor's stack at all. The test therefore forces the failure the fix actually guards against instead.

**Unrelated pre-existing defects surfaced while landing this pass** (not fixed here, out of this branch's stated scope):
- Every B2B integration test failed at host startup on `IDENTITY_INSERT` conflicts for `Applications`, `Bookings`, and `Concerts` (deterministic seed ids stamped via reflection onto SQL Server identity columns with no `SET IDENTITY_INSERT` toggle). Fixed in `ApplicationTestSeeder`, `BookingTestSeeder`, and `ConcertTestSeeder` — each now opens the connection, toggles `IDENTITY_INSERT` around the seed `SaveChangesAsync`, and closes it. Pre-existing, unrelated to the RequestContext/PaymentVerification work; this was blocking the entire integration suite, not just the two findings above that needed it.
- `Concertable.B2B.Deal.UnitTests.Strategies.DealStrategyArchitectureTests` (`KeyedProviderAllowlist_StillUsesKeyedServiceProvider`, `StrategyFactoryAllowlist_StillOwnsKeyedServiceLookup`) reference `ConcertDealStrategyFactory.cs`, renamed to `ConcertDealStrategyBuilder.cs` in commit `2d14e08db` — predates this session's work, unrelated to the six findings above.
- `OpportunityDashboardApiTests.GetCurrentForVenue_MapsApplicationCountAndDeadline` fails once the integration suite can actually run: the response never contains `fixture.SeedState.ActiveVenueHireOpportunity`. Likely adjacent to the already-tracked SEED4 (`FreshVenueHireOpportunity` picked by list position).
- `VenueDashboardApiTests.GetKpis_ReturnsCurrentVenueMetrics` fails once the integration suite can actually run: `VenueDashboardService.GetAsync` throws "A second operation was started on this context instance before a previous operation completed" — concurrent awaits sharing one `DbContext`.
- Now that the whole suite can boot, `Concertable.B2B.Application.IntegrationTests` shows a stable 6/71 failing (`ApplicationApiTests`, mostly Forbidden/eligibility and Accept-race cases). `Concertable.B2B.Booking.IntegrationTests` shows a stable ~12/21 failing, concentrated almost entirely in `BookingCancellationApiTests` — every cancellation-race, refund, and retry scenario. Not triaged individually; flagged for follow-up rather than investigated further in this pass. The concentration in `BookingCancellationApiTests` matches this file's own NAT8/NAT9/IR4/IR7 history of repeated cancellation-flow defects, so this is plausibly one shared root cause rather than 12 independent bugs — but that is a hypothesis, not a verified finding.

## Review pass — 2026-08-31 — save-failure classification

**Candidate base:** `3f6d85aaa53914a9de56ecb05245ccb1e1f1507e`
**Candidate head:** `e396b21645b92afb2ec050144f49f0004dd4d592`
**Candidate branch:** `Refactor/launch_deal-lifecycle-modules-phase2`
**Candidate scope:** branch-authored source only — 297 non-test `.cs` files of the 462-path delta; the commits merged in from `origin/main` (including #891) are out of scope
**Work-order path:** `reviews/Refactor-launch_deal-lifecycle-modules-phase2.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

Triggered by main's #891 landing `TrySaveChangesAsync(Func<DbUpdateException,bool>, ct)` and requiring an
explicit predicate. Three layers ran: native/general, a module-boundary lens, and a changed-behaviour
test-impact lens. Remediation landed in `44b8be344`, `b2fb2d71d`, `b398f3e46` and `08e55af43`, so the
top-level watermark is deliberately **not** advanced — those commits are post-anchor and owe a fresh
incremental pass.

### Findings — resolved in this pass

- [x] **SAVE1 — HIGH — correctness** — `.../Application.Infrastructure/Services/ApplicationService.cs`
  Withdraw, Reject and Cancel each passed `exception.IsDuplicateKey()`, but `Applications` carries a
  rowversion and marks `State` a concurrency token, and none of those paths inserts a row. No write they
  make can raise a duplicate key, so their `Superseded` branch was unreachable and a genuine concurrent
  transition escaped as a 500. Now expect `DbUpdateConcurrencyException`.
- [x] **SAVE2 — HIGH — correctness** — `.../Application.Infrastructure/Services/ApplicationWorkflow.cs:228`
  Accept expects two failures — the filtered unique index on `OpportunityId WHERE State = Accepted` when a
  competing accept wins, and the rowversion check when a competing withdraw/reject/cancel wins. Only the
  first was classified; the second reached the `InvalidOperationException` thrown when no accepted
  application exists. Added `AcceptApplicationError.Superseded`, matching the sibling unions.
- [x] **NAT1 — HIGH — correctness / cross-module transactionality** — `UnitOfWorkBehavior.cs:12-16`
  The root defect, and the reason SAVE1/SAVE2 could not be fixed at the save. `ExecuteAsync` calls
  `scope.Complete()` unconditionally once the delegate returns, so a failure classified *inside* the
  ambient scope still commits it. For Accept the pre-commit `ApplicationAcceptedDomainEventHandler` runs
  `BookingWorkflow.ConfirmAsync` first, which inserts and saves `Bookings` + `Contracts` and stages the
  escrow command — so a lost race committed a Booking and Contract for an unaccepted application and
  dispatched real money movement. `Bookings.ApplicationId` being unique then made every retry fail on that
  duplicate key, which the same predicate accepted, leaving the application permanently unacceptable.
  Resolved by `IUnitOfWorkBehavior.TryExecuteAsync`, which classifies at the transaction boundary: an
  expected `DbUpdateException` disposes the scope without `Complete`, rolling back every enlisted context,
  and only then runs the classification callback.
- [x] **NAT2 — MEDIUM — error handling** — the predicates were not scoped to the owning aggregate, so a
  foreign module's concurrency or duplicate-key failure would be reported as this operation's conflict.
  Each module now owns a `DbUpdateExceptionExtensions` that requires its own entity in `exception.Entries`.
- [x] **NAT3 / NAT4 — MEDIUM — cross-module transactionality** — `BookingWorkflow.cs:73` and
  `ConcertWorkflow.cs:53` had the same escape, with in-process pre-commit fan-out writing
  `ApplicationDbContext` inside the same scope. Both moved onto `TryExecuteAsync`.
- [x] **NAT5 — LOW — error handling** — `Concert/.../Payment/FinancialOperationOutcomeProcessor.cs:44`
  The `RefundEscrowRejected` guard covered only `CancellationFailed`, so a rejection arriving after the
  concert was already `Cancelled` threw and dead-lettered. Booking's sibling guards both; Concert now does.
- [x] **NAT6 — LOW — dead code** — `AcceptApplicationError.UnsupportedDeal` was never constructed anywhere
  in the repo and the `Accept` union covers every `DealType`. Deleted with its `Definition` arm.
- [x] **MB1 — HIGH — multitenancy** — `VenueArtistTenantInterceptor` was registered only on Concert's and
  Conversations' contexts, though `ApplicationDbContext` and `BookingDbContext` host only two-party rows.
  The write guard — both tenant ids stamped on insert, pair immutable after creation — was absent for
  Applications, Bookings and Contracts, and `ContractEntity` has no constructor guard either. Registered on
  both; verified the integration host still boots and seeds.
- [x] **DUP1 — MEDIUM — simplification** — `TenantScopedDbContext` and `VenueArtistTenantScopedDbContext`
  were identical apart from doc comments; the stance is carried by which helper `ApplyTenantFilters` calls.
  Deleted the duplicate and moved Booking/Concert/Conversations onto the survivor. The **repository** pair
  is a real distinction and stays.
- [x] **CARVE1 — HIGH — build** — `E2EAdminExtensions.cs` still named `Concert.Domain.Entities` for
  `ApplicationEntity`/`BookingEntity` and queried `concert.*` for tables now in `application.*`,
  `booking.*` and `opportunity.*`. Failed the solution build with CS0234.
- [x] **CARVE2 — HIGH — build** — `ConcertFor` exists only on the server-side seed state, not the TestKit
  wire contract, and `FreshVenueHireOpportunity` had been renamed `ActiveVenueHireOpportunity` everywhere
  except TestKit. Both E2E projects failed to compile.

### Findings — open

- [x] **OPEN1 — HIGH — API semantics** — the suites contradict each other on an already-accepted
  application: `ApplicationFlatFeeApiTests` and `ApplicationVenueHireApiTests` assert
  `Accept_ShouldReturn400_WhenAlreadyAccepted`, while `ApplicationDoorSplitApiTests` and
  `ApplicationVersusApiTests` assert `Accept_ShouldReturn409_WhenAlreadyAccepted`. The cause is ordering in
  `AcceptCoreAsync`: the eligibility gate (which requires an *open* opportunity) runs before
  `ValidateAccept()`, so a second accept reports "this concert opportunity is no longer open" (400) rather
  than the lifecycle conflict (409). Reordering was tried and reverted — it merely swaps which pair fails
  and additionally broke `ApplicationCancelApiTests.Cancel_ShouldMarkCancelledAndNotifyArtist`. Needs a
  product decision on which status is correct, then one consistent set of assertions.
- [x] **OPEN2 — MEDIUM — test validity** — `Accept_WhenTwoApplicationsRaceForOneOpportunity` runs the
  competing venue's full accept request from inside the first request's armed save interception, so the
  second request's opportunity read blocks on the first's uncommitted write and times out after 30s, giving
  a 500. Verified byte-identical before and after this pass. The overlap must be forced on a separate
  connection or suppressed transaction, as the simpler conflict tests do, rather than by nesting a live HTTP
  request inside an open transaction.
- [x] **OPEN3 — MEDIUM — pre-existing failures** — measured with and without this pass's changes and found
  identical, so none of these is caused by it: `Concertable.B2B.Booking.IntegrationTests` 11/21 failing
  (concentrated in `BookingCancellationApiTests`), `Concertable.B2B.Concert.IntegrationTests` 19/69 failing
  (settlement, payout-compliance and self-billing gates). `Concertable.B2B.Application.IntegrationTests`
  improved from 10/72 to 4/72; the remaining four are OPEN1 (two of them) and OPEN2, plus
  `Accept_WhenPaymentVerificationWinsTheRace_StillConfirmsTheBooking` returning 404.
- [x] **OPEN4 — MEDIUM — test coverage** — no `ApplicationServiceTests` or `ApplicationWorkflowTests` class
  exists, so Withdraw/Reject/Cancel/Accept conflict classification has no unit coverage, and
  `Concertable.B2B.Application.UnitTests` has no `Moq` reference. Every existing save-failure test passes
  `It.IsAny<Func<DbUpdateException, bool>>()`, so *which* failure each site treats as expected is untested —
  reverting a predicate leaves the suite green. The `*.superseded` error codes are asserted nowhere except
  `concert.update.superseded`. `ConcertWorkflowTests`'s new conflict cases drive the behaviour double rather
  than the real predicate, because fabricating a `DbUpdateException` with populated `Entries` needs a live
  EF context; the predicates themselves are therefore only covered by the integration race tests.
- [x] **OPEN5 — LOW — work-order accuracy** — this file's 2026-08-29 pass credits
  `CancelExecutorTests.CancelAsync_SaveRaceLost_ReturnsSuperseded`, but that class no longer exists; the
  surviving assertion is in `Concert.UnitTests/Services/ConcertWorkflowTests.cs`. Repoint the note.

### Considered and rejected

- Concert injecting `IBookingModule` for `GetContractPdfByBookingIdAsync` was raised as a backward runtime
  call against the Concert module's "reaches back into none of the others" rule. Not a violation: it
  resolves through Booking's Contracts facade to the frozen `ContractEntity` snapshot, never a live
  `BookingEntity`, and cross-module traffic through a module facade is exactly what `module-structure`
  permits. Recorded rather than changed.
- Renaming `IVenueArtistTenantScoped` to a generic `ISharedTenantScoped` so it could move to Kernel. Its
  members are `VenueTenantId`/`ArtistTenantId`, and the repository and filter built on it name the same
  sides, so a generic type name over domain-specific members is a worse mismatch and still could not move.
  Revisit only when a second service needs two-party rows.

## Review pass — 2026-08-31 — Concert integration triage

**Candidate:** `a2624a19d..434682d51` on `Refactor/launch_deal-lifecycle-modules-phase2`
**Scope:** the pre-existing `Concertable.B2B.Concert.IntegrationTests` failures, the last red shard on #633
**Pass judgment:** `changes-requested`

Triage of the 19/69 failures recorded as OPEN3. Fourteen are resolved; five remain and reduce to two
questions, both needing a product decision rather than a code fix.

### Resolved

- [x] **CI1 — MEDIUM — stale assertion** — ten tests asserted `ConcertState.Draft` for a seeded concert.
  `ConcertFactory` posts a seeded concert through the real `ConcertEntity.Post` transition whenever its
  spec carries a `DatePosted`, and 46 of the 47 catalog concerts do — only concert 1 opts out. The seed
  can no longer produce `Draft`, and every one of these was an unchanged-state assertion spelled as a
  literal. `ConcertCancelApiTests:235` correctly keeps `Draft`: it creates its concert live.
- [x] **CI2 — MEDIUM — validation placement** — a negative door revenue is rejected by FluentValidation
  auto-validation before the action runs, so the operation-owned problem with a stable code that
  `Declare_ShouldReturnStableProblem_WhenRevenueIsNegative` asserted is an outcome that path cannot
  produce. The range rule now carries the message the test wanted, and the test asserts the
  `ValidationProblemDetails` the pipeline returns. `DeclareDoorRevenueError.Negative` stays reachable
  from the completion runner and the E2E admin surface, neither of which passes through MVC validation.
- [x] **CI3 — LOW — stale assertion** — door revenue after cancellation returns 409, not 400. NAT16
  deliberately routes every terminal and cancellation state through the existing `AlreadySettled`
  failure, which is a `Conflict`.
- [x] **CI4 — HIGH — correctness / concurrency** — settlement had no save-failure classification at all.
  It runs through `IUnitOfWorkBoundary`, which never gained the `TryExecuteAsync` the lifecycle
  operations did, so a concurrency loss on the reservation escaped as a raw
  `DbUpdateConcurrencyException` and surfaced as a 500. The boundary now carries the same shape,
  disposing the failed context — rolling its transaction back — before the classification re-runs the
  reservation against committed truth, so whatever won the race decides the outcome.

### Open

- [x] **CI5 — HIGH — settlement semantics** — the suites contradict each other on when a revenue-share
  concert is settled, exactly as OPEN1 does on 400-vs-409. `ConcertVersusApiTests` expects
  `AwaitingSettlement` after finish and comments that "completion happens on the webhook";
  `ConcertDoorSplitApiTests` expects `Complete` for the identical operation — same deal family, same
  `PayoutCompleteStep` strategy — and passes. Production completes eagerly: `PayoutCompleteStep` returns
  `SettlementConfirmation.ManagerPaid` inline even when the payment requires action, and
  `SettlementService.CompleteAsync` then runs `CompleteSettlement`, so the concert is marked settled
  before the payout confirms. Deciding this is a financial question, not a test fix. Four failures hang
  on it: `ConcertVersusApiTests.Finish_ShouldChargeGuaranteePlusDoorShareOffSession_AfterDoorRevenueDeclared`,
  `TenantVerificationGateApiTests.Finish_Settles_WhenBothTenantsVerified`,
  `ConcertPayoutComplianceGateApiTests.Finish_RevenueShare_Settles_WhenPayeeArtistTaxComplianceComplete`
  and `ConcertCancelApiTests.Cancel_WhenSettlementReservationWinsTheRace_ReturnsConflictAndLeavesSettlementInProgress`,
  whose armed callback asserts the settlement succeeds and whose tail asserts `AwaitingSettlement`.
  Note `Finish_RevenueShare_Settles` additionally reads its concert by `PastDoorSplitApp.Id` — an
  application id used as a concert id — so it asserts against the wrong row regardless.
- [x] **CI6 — MEDIUM — test harness** — `Cancel_WhenAnotherCancellationWinsTheRace_SucceedsWithoutASecondRefund`
  expects exactly one `RefundEscrowCommand`; the transport holds none, only the setup's
  `CaptureEscrowCommand` and an email, so the winning cancel's refund never reaches it. Same family as
  OPEN2: the armed-conflict harness runs a full competing HTTP request from inside the first request's
  save interception. Verified pre-existing — this test was in the original 19 both with and without the
  `TryExecuteAsync` work.

## Review pass — 2026-09-01 — remediation incremental

**Candidate base:** `e396b21645b92afb2ec050144f49f0004dd4d592`
**Candidate head:** `14abccc691ac3c9f3d46f2536ea65ace5229ae55`
**Candidate branch:** `Refactor/launch_deal-lifecycle-modules-phase2`
**Candidate scope:** `all` — 52 paths, 13 non-merge commits
**Candidate path-set digest:** `sha256:b87be622a0be43a9…`
**Work-order path:** `reviews/Refactor-launch_deal-lifecycle-modules-phase2.md`
**Work-order mode:** `append`
**Pass judgment:** `approved-with-remediation`

Covers the remediation for the two prior passes: the `TryExecuteAsync` transaction boundary on both the
unit-of-work behaviour and the boundary, the mapper disambiguation, the E2E seed-contract carve, the
duplicate context-base deletion, and the Concert integration assertion corrections. The prior pass reviewed
`3f6d85aaa..e396b2164`; this pass closes the gap from that head, so completing it advances the single
top-level watermark to this head.

### Findings

- [x] **RP1 — HIGH — correctness** — `.../Application.Infrastructure/Extensions/DbUpdateExceptionExtensions.cs`
  The predicates were scoped to the entity TYPE, not the row, and acceptance saves more than one row of that
  type: after the target application commits, `AcceptCoreAsync` calls `RejectAllExceptAsync`, which loads the
  opportunity's other `Applied` applications and issues its own `SaveChangesAsync`
  (`ApplicationRepository.cs:121`). `State` is a concurrency token, so a competing withdraw or reject of a
  SIBLING application raised a `DbUpdateConcurrencyException` whose `Entries` held that sibling — which the
  type-scoped predicate accepted. The healthy accept, plus the Booking, Contract and staged escrow command
  its pre-commit handler had written, were rolled back and the caller was told `Superseded`: a false
  "someone else won", with no retry. Every predicate now takes the id it guards.
- [x] **RP2 — HIGH — correctness** — `.../ConcertPayoutComplianceGateApiTests.cs:60`
  Self-inflicted earlier in this same range. `726a27002` changed the read to `ConcertAsync(concert.Id)` on the
  claim that an application id was being used as a concert id — but that file's `ConcertAsync(int
  applicationId)` filters on `ApplicationId`, while the identically named helper in
  `TenantVerificationGateApiTests` filters on `Id`. The original was correct. Reverted.
- [x] **RP3 — MEDIUM — correctness** — `SettlementService.ClassifyReservationConflictAsync`
  The retry ran through the unclassified `ExecuteAsync`, so a second concurrency loss escaped as the same raw
  `DbUpdateConcurrencyException` the classification was added to remove. Now bounded.
- [x] **RP4 — LOW — consistency** — `ConcertCancelApiTests.Cancel_WhenSettlementReservationWinsTheRace`
  The last assertion still expecting `AwaitingSettlement` on the eager-completion path this range codified.
- [x] **RP5 — HIGH — test coverage** — `TryExecuteAsync` had no test on either interface despite being the
  load-bearing mechanism for six call sites. `UnitOfWorkBehaviorTests` pins the ordering by asserting a null
  `Transaction.Current` inside the classification callback; `FactoryUnitOfWorkTests` pins the boundary
  variant against the real SQLite harness — the context is disposed when classification runs, and an
  expected failure leaves none of the operation's writes behind. 31 passing, up from 22.
- [x] **RP6 — MEDIUM — test validity** — `MockPaymentTransport.SingleCommand` reads synchronously, but a
  command arrives by outbox dispatch after the staging request returns. Three reads in
  `BookingCancellationApiTests` raced the dispatcher; `SingleCommandAsync` is the waiting counterpart.

### Considered and rejected

- A lens reported the settlement retry hands the race loser the winner's live reservation, risking duplicate
  money movement. The native layer read both complete strategies and found they key on
  `settlement.OperationId`, so a resumed reservation is idempotent at the provider — and the test exercising
  the path, `Finish_WhenAnotherFinishWinsTheRace_ReleasesEscrowAndIssuesInvoiceOnce`, passes. Kept the
  resume and recorded the disagreement rather than acting on the weaker side.
- Two findings attributed to this range touch no line it changed (`git diff` checked):
  `ConcertDoorSplitApiTests.Finish_ShouldIgnoreDuplicateSettlementWebhookEvent` asserts a state that already
  holds before its Act, and `ConcertDoorRevenueApiTests.Declare_ShouldReturn409_AfterConcertHasSettled` never
  checks the door revenue stayed put. Both are earlier weak assertions, carried below.
- **Adding a tracker reset to `IUnitOfWork` was attempted and abandoned.** It exposes EF tracking mechanics
  on an application contract, and `TrySaveChangesAsync` already owns that clear. Reworking the Booking
  convergence to compose `TrySaveChangesAsync` instead then broke a passing test
  (`Cancel_ShouldRecordCancellationFailure_WhenRefundIsRejected`) and was reverted: that clear discards the
  WHOLE tracker, so used as a mid-flow save inside a broader unit of work it throws away the outbox rows the
  pre-commit handlers staged in the same context. Any future fix here must not reset shared tracking.

### Open, carried forward

- [x] **RP7 — HIGH — Booking cancellation convergence** — 9 of 21 failing in
  `Concertable.B2B.Booking.IntegrationTests`, in tests this branch authored (the project does not exist on
  main). Two hypotheses were tested and disproved: cross-class pollution (class-alone still fails 9 of 16)
  and missing concurrency classification on `RecordSucceededAsync`/`RecordFailedAsync` (the attempt broke a
  green test, see above). These are individual defects, not one shared cause. Two of the nine
  (`Cancel_WhenAnotherCancellationWinsTheRace`, 30s) are lock timeouts rather than assertion failures.
- [x] **RP8 — MEDIUM — remaining Concert and Application failures** — 2 and 2, also in tests this branch
  authored. `Cancel_WhenAnotherCancellationWinsTheRace` (no refund command reaches the transport),
  `Cancel_WhenSettlementReservationWinsTheRace` (its armed callback's own `Assert.True` fails),
  `Accept_WhenTwoApplicationsRaceForOneOpportunity` (30s lock timeout),
  `Accept_WhenPaymentVerificationWinsTheRace` (404).
- [x] **RP9 — MEDIUM — test coverage** — the module predicates are unproven for the positive case, because a
  `DbUpdateException` with populated `Entries` needs a live provider, and
  `IsApplicationAcceptanceConflict`'s duplicate-key arm needs real SQL Server (`SqlException` is sealed).
  `application.accept.duplicate` is asserted nowhere. `ConcertWorkflowTests`'s
  `ClassifiesSaveFailureAsConflict` flag short-circuits the real predicate with `||`.
- [x] **RP10 — LOW — weak assertions** — the two earlier weaknesses named above want a witness that actually
  moves (a payment/invoice count, and the persisted door revenue).

## Review pass - 2026-09-01 - transaction-root ownership

**Candidate base:** `35f9ca43a`
**Candidate head:** `83ebbc394`
**Candidate branch:** `Refactor/launch_deal-lifecycle-modules-phase2`
**Pass judgment:** `approved`

Every suite the range touches is green: `Concertable.DataAccess.UnitTests` 31/31,
`Concertable.B2B.Booking.IntegrationTests` 21/21, `Concertable.B2B.Concert.IntegrationTests` 69/69,
`Concertable.B2B.Application.IntegrationTests` 72/72, `Concertable.B2B.Process.IntegrationTests` 41/41,
`Concertable.B2B.ArchitectureTests` 27/27, the three module unit suites 134/134, and
`./scripts/local-platform.ps1 build api/Concertable.slnx` clean.

### Findings

- [x] **TR1 - HIGH - correctness** - `TryExecuteAsync` rolled back its own scope before classifying. A
  nested scope shares the caller's transaction, so that rollback doomed it and `onExpectedFailure` threw
  `TransactionAbortedException` instead of reading committed truth - the failure behind every
  financial-outcome convergence attempt in this range. Recovery now belongs to whoever owns the
  transaction, and a nested loss propagates to the root that can actually roll back and rerun. That is why
  the convergence moved out of `BookingWorkflow` - always nested under a message handler - into
  `AcceptanceFinancialOperationOutcomeProcessor`, which owns the root and rolls the whole message back,
  inbox row included, before reprocessing it in a fresh scope.
- [x] **TR2 - HIGH - correctness** - a payment verification landing mid-accept wrote only to
  `application.VerifyPayments`, so it never conflicted with the accept: the accept committed a booking that
  contradicted a succeeded verification, and because the verification's pre-commit handler had run before
  that booking existed, nothing would ever confirm it. Recording a verification now touches the application
  row, bringing it inside the application's concurrency token, and `AcceptAsync` reruns once in a fresh
  scope against the recorded outcome. `RecordPaymentVerification` reports whether it recorded, so a
  redelivery does not bump the version for nothing.
- [x] **TR3 - MEDIUM - API semantics** - an accept losing to a rival acceptance reported
  `application.eligibility.opportunity_not_found` (404) from the eligibility gate: a lifecycle conflict
  answered as a missing resource belonging to someone else. It now reports `application.accept.duplicate`,
  which had no coverage anywhere before this pass.
- [x] **TR4 - HIGH - test harness** - the armed conflict fired inside the operation's `SaveChangesAsync`,
  by which point its pre-commit handlers held row locks the competing change waited on until the 30s
  command timeout - a deadlock no scheduler could produce, because the operation was itself blocked on the
  competing change. That was OPEN2 and CI6. The window is now the read that fetches the row's concurrency
  tokens - a key or single-column projection does not open it - and the competing change runs with
  execution-context flow suppressed so it inherits neither the operation's transaction nor its
  `HttpContext`; without the latter its fresh scope was neither host nor tenant-resolved, so every
  tenant-filtered read returned nothing. `ForcedConflicts` is still counted only when a pending update of
  the losing entity is actually staged.
- [x] **TR5 - MEDIUM - test validity** - four defects that made suites lie: pre-commit dispatch resolved
  `IPreCommitDomainEventHandler<T>` directly, which registers nothing - handlers register against
  `IDomainEventHandler<T>` and the phase is a marker - so the helper ran no handlers at all; the
  update-failing CHECK constraint named `Id`, which no update writes, and SQL Server skips constraints
  whose columns an UPDATE leaves alone, so it never fired; `PaymentTransport.Commands` carries emails as
  well as money, so `Assert.Empty` on it raced an unrelated dispatch; and "reject the latest financial
  operation" reached the acceptance capture still pending from setup rather than the refund under test.
- [x] **TR6 - HIGH - regression** - moving payment verification into the Application module left the venue
  manager with no signal that their card verification failed. `IConcertNotifier.VerifyPaymentFailedAsync`
  and its implementation survived the carve with no caller. The notification follows the code that moved:
  `IApplicationNotifier` owns it, `VerifyPaymentFailedProcessor` sends it from the payment metadata the
  checkout stamps, and Concert's unreachable pair is deleted.

### Considered and rejected

- **Keeping the convergence in `BookingWorkflow` behind the new root guard.** It reads as one policy in one
  place, but a pre-commit handler is nested by definition and a message handler owns its own scope, so the
  guard would leave it dead in production and live only under a fixture that dispatches handlers directly.
  The convergence sits where the transaction actually begins instead.
- **Reordering the conflict interceptor ahead of the domain-event dispatch interceptor** would have kept the
  exact save-time window, but `AddInterceptors` only appends and a fixture cannot prepend - and
  `Accept_WhenTwoApplicationsRaceForOneOpportunity` showed the deeper point: with a real window the loser
  reads the loss on its eligibility gate before it ever writes, so no rowversion conflict arises there at
  all. That test now asserts the duplicate conflict it actually produces.
- **Asserting the stuck booking** in `Accept_WhenPaymentVerificationWinsTheRace_StillConfirmsTheBooking`
  rather than fixing TR2. The test named the right outcome; the product was wrong.

### Open, carried forward

- [x] **TR7 - MEDIUM - security review** - done, see the security pass below.
- [x] **TR8 - LOW - coverage** - the contract snapshots now assert that acceptance closes the deal to
  edits. The stronger statement - cancel the booking, which reopens the opportunity, then edit the deal and
  find the contract unchanged - exercises immutability through a path the product still allows, and is not
  covered. Recorded in the Booking module's tech debt rather than rushed: the reopen arrives through several
  asynchronous hops and would be the suite's most timing-sensitive assertion.

## Security pass - 2026-09-01

**Span:** `3f6d85aaa..3c474d8be` - 916 files, +26221/-8329 under `api/`
**Pass judgment:** `no vulnerabilities found`

The span is effectively the whole module carve, so the pass was scoped to the surfaces a carve can actually
break: who may call an endpoint, whose rows a query returns, what crosses a trust boundary, and what reaches
a log or a config file.

### Checked

- **Authorization coverage on every controller the span touched.** Zero unguarded mutating endpoints. The
  guard vocabulary is `HasPermissionAttribute`, `RequiredTenantTypeAttribute`, `AdminAttribute`,
  `CustomerAttribute` and the framework's `Authorize`/`AllowAnonymous`; the tenant-verification
  approve/reject pair is `[Admin]`, and the concert contract and invoice PDF downloads are
  `[RequiredTenantType(TenantType.Venue)]` on top of the tenant query filter. Six unguarded reads remain and
  are deliberate public marketplace surface: a venue's public profile (explicitly
  `[EnableRateLimiting(RateLimitPolicies.PublicRead)]`), its `ownership` flag (which answers only about the
  caller), and the artist and venue public review list and summary. None of the six was modified in the span.
- **Tenant scoping.** No `IgnoreQueryFilters` introduced anywhere in the span. Filter registration is now
  guarded by a test rather than by review: `TenantWriteGuardTests` source-scans every module whose DbContext
  calls `ApplyVenueArtist<>` and asserts its composition root registers `VenueArtistTenantInterceptor`, with
  a second test guarding the guard against the module list going stale.
- **Trust boundary on the payment webhook.** `VerifyPaymentFailedProcessor` sends the restored notification
  to the user id in `PaymentMetadataKeys.VenueManagerId`. That value is stamped server-side by
  `ApplicationCheckoutService` from `venue.UserId` when the payment intent is created, never supplied by a
  client, and it returns through a signature-verified provider webhook, so the recipient cannot be steered
  by a caller. Same trust chain the pre-carve Concert-side processor used.
- **Raw SQL.** Every `ExecuteSqlRaw`/`FromSqlRaw`/`SqlQueryRaw` occurrence added in the span is in a test
  fixture or seeder; production code adds none. The one this range authored, the booking update-failing CHECK
  constraint, is a fixed literal.
- **Secrets and sensitive logging.** No credential, key or connection string added outside `*.example`
  files, and nothing added that logs a password, token, key or signature.

### Defence in depth, not a finding

- The verify-payment-failed notification derives its recipient from echoed payment metadata rather than from
  the application row. The trust chain above makes that safe today, but resolving the venue manager from the
  application would remove the dependency on metadata round-tripping through the provider entirely. Recorded
  because the pre-carve code had the same shape and it is worth closing deliberately rather than inheriting.

## Review pass — 2026-09-02 — incremental

**Candidate base:** `83ebbc3942bd9ea5c9c27ad0143975612c947c18`
**Candidate head:** `b121ed028179583fc7599b965caba7a0683c8913`
**Candidate branch:** `Refactor/launch_deal-lifecycle-modules-phase2`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:2e030581a13b2909aa70002370d6b177f297f31c2e578abcf07d8f091f98e55c` `(91 paths)`
**Candidate bundle:** `C:/Users/TOMMYS~1/AppData/Local/Temp/claude/C--Users-TommySeery-source-repos-Concertable/62ef6cf9-f6fe-43dd-a8f6-24f2683961a7/scratchpad/rb-633`
**Candidate bundle identity:** `sha256:980c696fd3136b87b975bec1243844cb0f83a1f9c739e850b4f4deaf120f5ad7`
**Work-order path:** `reviews/Refactor-launch_deal-lifecycle-modules-phase2.md`
**Work-order mode:** `append`
**Pass judgment:** `approved-with-remediation`

Layers: native/general; a conventions lens against `STYLE.md`/`NAMING.md`; a changed-behaviour
test-impact lens. Parent re-verified every retained claim and rejected two.

**Verified clean by the native layer, recorded so it is not re-derived:** the `744fe0bc9` rename sweep
is mechanically identifier-only — 389 hunks token-structure-identical after stripping `this.`, 31
consistent 1:1 renames, zero `X = X;` self-assignments in `api/`, zero `this.X = Y;` name mismatches,
and every non-constructor shadow site either `static` or still `this.`-qualified. No second
`ForService`-shaped bug exists. `b121ed028`'s helper sinking is pure motion: the whitespace-normalised
sorted line multiset is byte-identical before and after for all ten files, and the parent independently
confirmed the member-signature sets are identical (21/21, 33/33, 9/9) for the three largest.
`WithService` cannot leak a scoped name, and `RunAsEmulator` still sees every topic.

### Findings

- [x] **TR9 — LOW — reuse** — `api/Concertable.AppHost.Shared/AsbServiceTopology.cs:15`
  The scoped builder exposed `Publish<TEvent>()` and `Queue<TCommand>()` with no test and no caller
  anywhere in the tree.
  **Disposition:** `Publish` deleted — the five topologies declare publications on the base
  `AsbTopology` before scoping, so a scoped `Publish` has no prospective caller. `Queue` kept and
  covered by `WithService_Queue_NamesTheQueueForThatService`, because the topology migration will call
  it (`AuthTopology` and `CustomerTopology` both queue). `AppHost.Shared.UnitTests` 12/12.

- [wontfix] **TR10 — HIGH — correctness** — `app/web/b2b/venue/src/features/concerts/components/ApplicationCard.tsx:60`
  Deny is gated on `actions.reject`, but the venue endpoint serializes `decline` — the href behind it
  is `/api/application/{id}/reject`, which is what made the mismatch look correct. The button has never
  rendered on the venue applications list. This delta preserves it deliberately: `ApplicationActions`
  carries a `@deprecated reject` purely so the consumer keeps type-checking.
  **Disposition:** cannot be fixed on this branch, and that is CI-proven rather than asserted.
  `carve-fe` builds each app's committed source against the `@concertable/*` tiers **as published to
  the feed** — its own header says `"*"` "links the workspace copy in-monorepo but is unresolvable from
  the feed" — and the published `web-b2b` still types `ApplicationActions` with `reject` only. Flipping
  line 60 failed `carve-fe (web/b2b/venue)` at `53eae7d84` with
  `TS2339: Property 'decline' does not exist on type 'ApplicationActions'`. Transferred to
  `app/web/TECH_DEBT.md` with the objective condition: `web-b2b` republished carrying `decline`, line 60
  flipped, `reject` deleted, `carve-fe (web/b2b/venue)` green.

- [x] **TR11 — LOW — docs-and-debt** — `TECH_DEBT.md:21`
  The entry asserted `dotnet_style_qualification_for_field = true:error` "has been removed from
  `STYLE.md`'s table". It has not: all seven cached `dotnet-standards` copies still carry that row and
  the "every constructor assignment is `this.`-qualified — fields *and* public auto-properties" prose
  with a worked example. The consequence is not cosmetic — the conventions lens on this very pass read
  the standard and reported the sweep as a 50-site violation.
  **Disposition:** reworded to state the standard and the codebase currently disagree, with the added
  resolve condition that `STYLE.md` in `dotagents` states the disambiguation-only rule and is published.
  The sweep itself is not a violation of the intended convention and no code changed.

**Rejected — the conventions lens's headline finding.** It reported the `this.` sweep as violating
`STYLE.md` at ~50 sites, including `DealStrategyBuilder.cs:17` as "internally inconsistent" for keeping
`this.services = services;` while dropping `this.` on the next line. Under the intended
disambiguation-only convention that line is correct: `services` is shadowed by the parameter, `builder`
is not. The lens judged against the standard as published, which TR11 records; the sweep is not a defect.

**Rejected — the native layer's premise on NAT2.** It argued the publish-first rescope was unnecessary
because `app/package.json` links every web package as an npm workspace with `"@concertable/web": "*"`,
so no publish step gates the frontend. That is true of the in-monorepo build and irrelevant to the gate
that actually failed: `carve-fe` exists precisely to catch what is "masked in-monorepo by workspace
hoisting", rewrites `"*"` to `"alpha"`, and installs from the feed only. It failed at `53eae7d84` on
three separate module/type errors. NAT2's secondary observation stands and is not retained as a
separate finding: the shared abstraction currently has no consumer while both widgets keep their own
copies, which is the temporary, intended cost of the split and is owned by the consumer cut-over.

**Not retained:** the new shared `applicationActionLabels` / `ActionLinkButtons` having no tests and no
callers, and nothing exercising `decline` or the `reject`/`decline` dual shape — both are inherent to
the publish-first split and owned by the cut-over PR that TR10's debt entry gates.

## Review pass — 2026-09-05 — Payment v1 consumer cut-over

**Candidate base:** `39fbbc0126d6ddd8d40426e25663bea29cd7f1a5`
**Candidate head:** `62b9fde66500c9830e20ed47a2f5633d2c5ffa80`
**Candidate branch:** `Refactor/launch_deal-lifecycle-modules-phase2`
**Candidate scope:** `all`
**Work-order path:** `reviews/Refactor-launch_deal-lifecycle-modules-phase2.md`
**Work-order mode:** `append`
**Pass judgment:** `approved-with-remediation`

Three read-only lenses over the cut-over — the Payment boundary, persistence/migrations, and test
doubles plus the frontend edge. They ran against the worktree at the candidate head rather than a
materialized bundle, so no bundle identity is recorded; every retained claim below was re-verified by
the parent against the file it names, and against #933's own source in its worktree where the claim was
about producer behaviour.

### Findings

- [x] **IR11 — HIGH — correctness** — `Concertable.B2B.Hosting/B2BTopology.cs:44`
  Payment v1 publishes `RefundEscrowDeferredEvent` when a refund finds nothing to refund
  (`PaymentTopology.cs:19`, raised at `FinancialOperationHandler.cs:213`). B2B neither subscribed to it
  nor handled it, so cancelling a booking whose escrow never captured left the aggregate in
  `CancellationPending` permanently, and `BookingWorkflow.CancelAsync` then returned success on every
  retry without doing anything.
  **Disposition:** handled on both cancel paths. `CancellationFinancialOperationOutcomeProcessor` and
  Concert's `FinancialOperationOutcomeProcessor` take the deferred arm through the same `Cancel` action
  as the succeeded arm — the money never moved, so the cancellation is complete — registered in both
  modules and subscribed in `B2BTopology` and `B2BWebHostExtensions`. **Revised by IR21:** the Booking arm
  now only records the inbox row; Concert's still cancels.

- [x] **IR12 — HIGH — correctness** — `VerifyPaymentProcessor.cs:33`, `VerifyPaymentFailedProcessor.cs:39`,
  `AcceptanceFinancialOperationOutcomeProcessor.cs:45,61,77,94`
  Both families read the encoded id out of the reference before the inbox guard, and threw on a
  reference whose shape they did not own. `MethodVerificationType` is Payment's own `verify` constant
  rather than a B2B-owned token, so any other consumer's verification passed the type guard and then
  threw in `ReadApplicationId`; the acceptance handlers had no type guard at all. A message that throws
  ahead of its inbox row is never marked processed, so the transport redelivers it forever, head-of-line
  blocking a subscription B2B shares with the verify outcomes.
  **Disposition:** both now guard on type and shape and return on a miss (`TryReadApplicationId`, and a
  `TryReadBooking` that requires `EscrowType`). A reference this service did not mint is another
  consumer's message, not a malformed one.

- [x] **IR13 — MEDIUM — test-coverage** — `FlatFeeLifecycleTests.cs:80`, `VersusLifecycleTests.cs:73`
  `Accept_ShouldIgnoreDuplicateWebhookEvent` did not redeliver anything for the checkout-bearing flows.
  The simulator preferred the directly-opened operation, so the second call announced an `escrow-hold`
  `PaymentSucceededEvent` that neither registered handler consumes; the inbox guard could have been
  deleted and the test would still have passed.
  **Disposition:** the simulator now redelivers a settled acceptance outcome ahead of re-announcing a
  session, so the second call is a genuine duplicate of the outcome the flow actually settled on.

- [x] **IR14 — MEDIUM — test-coverage** — `BookingCancellationApiTests.cs:58`, `ConcertCancelApiTests.cs:108`
  Both asserted `Assert.Empty(fixture.EscrowClient.Holds)`. Escrow deposits and captures move as bus
  commands under v1, and the only surviving `IEscrowOperationsClient` caller is `ReleaseEscrowCompleteStep`,
  so `Holds` is unconditionally empty and the assertion could never fail.
  **Disposition:** replaced with `Assert.Empty(fixture.PaymentTransport.FinancialCommands)`, which is
  where a wrongly-staged capture, deposit or refund would actually appear.

- [x] **IR15 — LOW — test-coverage** — `FlatFeeLifecycleTests.cs:143`
  The test expected `InvalidOperationException` from `FailingEmailRenderer`, but the transport throws
  the same type on its own wait timeout, so a flow that never staged the capture at all would have
  satisfied the assertion and the four follow-ups.
  **Disposition:** asserts the renderer's message.

- [x] **IR16 — LOW — efficiency** — `Mocks/MockPaymentTransport.cs:245`
  Redelivery consulted its settled-command fallback only after the full five-second polling deadline,
  on top of the simulator's two-second gate.
  **Disposition:** the fallback is checked before the loop when redelivering, and the gate is now
  acceptance-scoped, so it no longer waits on a pending refund it would not have acted on.

- [x] **IR17 — LOW — correctness** — `MockWebhookSimulatorFail.cs:46`
  The failure path minted a fresh envelope id where the success path uses one stable per operation, so
  calling it twice applied the same failure twice and defeated the invariant this change introduced.
  **Disposition:** both paths now use `PaymentOperationEnvelopes.StableId`.

### Considered and rejected

- **A repeated accept-checkout creates a second authorization hold.** It does not. `CreateAsync` mints a
  fresh operation id per call, but Payment reserves on the reference:
  `PaymentSessionEntityConfigurations.cs:55-56` declares `HasIndex(OperationType, ClientReference)`
  `.IsUnique()`, so the insert hits a duplicate key, `PaymentSessionOperationRepository.cs:67-78`
  re-fetches by reference and re-fingerprints the specification with the existing operation id, and an
  otherwise-identical request comes back `Replayed`. It returns `Conflict` only when the request
  genuinely differs, which is correct. The lens that raised this read the legacy Payment source left in
  this worktree after the overlay was reverted, not v1.

- **The abstract-TPH downcast in `QueryableConcertMappers.ToDetails` is untranslatable.** The failure
  that fixed `DoorRevenueOutstandingSpecification` needed the second ingredient this call site lacks:
  composition into a correlated subquery. `ToDetails` is consumed only at top level, where EF lowers the
  cast to a discriminator `IN`, and `GET /api/concert/{id}` and `/api/concert/application/{id}` are
  covered by Concert 69/69 and by every Lifecycle test that fetches the concert after acceptance. The
  file is also untouched by this change.

### Open, carried forward

- [x] **IR18 — MEDIUM — test-coverage** — `TestClientOptions.cs:24`
  `UseFailingPayment()` has had no caller since `c55c99718`, so `MockEscrowClientFail` is dead and
  `ReleaseEscrowCompleteStep`'s `FinishConcertError.EscrowReleaseFailure` branch has no coverage. Predates
  this change; the branch is reachable and should get a test, or the option should go.
  **Disposition:** the dead option and `MockEscrowClientFail` are deleted. The uncovered branch is recorded
  in `Modules/Concert/TECH_DEBT.md` with the test that resolves it.

- [wontfix] **IR19 — LOW — correctness** — `SettlementPaymentProcessor.cs:41`, `SettlementPaymentFailedProcessor.cs:35`
  Both run the settlement mutation in its own transaction before opening a second one to check and write
  the inbox row, so the envelope-id inbox provides no idempotency on that path; convergence rests on
  `SettlementService`'s state check and `InvoiceIssuer`'s existence check. The shape predates this
  change and the two refund processors do it correctly.
  **Disposition:** deferred; the fix is a transaction-shape change in the settlement path. Recorded in
  `Modules/Concert/TECH_DEBT.md` with the redelivery test that resolves it.

- [x] **IR20 — LOW — test-coverage** — `BookingCancellationApiTests.cs:56,72`, `ConcertCancelApiTests.cs:107`
  Negative assertions about staged commands read the transport synchronously, but a command arrives by
  outbox dispatch after the request returns, so a regression that wrongly stages one is still in flight
  at the moment of the read. Pre-existing pattern; needs the waiting counterpart.
  **Disposition:** `MockPaymentTransport.SettledFinancialCommandsAsync` watches the transport for a bounded
  window and returns the moment a financial command lands, so the three negative assertions read a snapshot
  a late outbox dispatch would have reached.

## Review pass — 2026-09-05 — independent review of the v1 cut-over

**Candidate base:** `39fbbc0126d6ddd8d40426e25663bea29cd7f1a5`
**Candidate head:** `db5d4be8cbd65195e0922cf3f648377f9fb31afb`
**Candidate branch:** `Refactor/launch_deal-lifecycle-modules-phase2`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:32a67f886d591016075700e861a6af9b5ad01fd9d03941bbcd7de0c7dac42b64` `(505 paths)`
**Candidate bundle:** none materialized; read from the worktree at the frozen head, plus the staged 1322
pin and `PaymentTopology.cs` reconciliation. `8fed74604`, the step rename, landed after the read and was
not re-reviewed.
**Work-order path:** `reviews/Refactor-launch_deal-lifecycle-modules-phase2.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

Two lenses, correctness and security, over the consumer cut-over, with the producer read at `origin/main`
`ea33c48e6`, the commit the 1322 packages were built from. Nothing was executed during the review.

### Findings

- [x] **IR21 — HIGH — correctness** — `CancellationFinancialOperationOutcomeProcessor.cs:35`,
  `BookingWorkflow.cs:174`, `BookingStateMachine.cs:9`
  IR11's Booking arm cancelled on `RefundEscrowDeferredEvent`. Cancellation is allowed from
  `AwaitingConfirmation`, so the refund reaches Payment before the acceptance capture or deposit; Payment
  finds no escrow and defers, B2B moves the booking to `Cancelled`, and when the capture then lands
  `RecordSucceededCoreAsync` returns on the terminal state. The money is captured into escrow and nothing
  refunds it; Payment's refund operation stays `Pending` with no retry. IR11's premise was wrong: every
  acceptance command yields a terminal outcome through the outbox, and both outcome arms already converge a
  `CancellationPending` booking.
  **Disposition:** the Booking deferred arm records the inbox row and leaves the booking in
  `CancellationPending`; the acceptance outcome arms finish it. Concert's arm stays, since a concert exists
  only after the escrow landed. `MockPaymentTransport.DeferLatestAsync` emits the deferred event without
  settling the command, covered by `Cancel_ShouldWaitForTheCapture_WhenTheRefundIsDeferred` and
  `Cancel_ShouldMarkCancelled_WhenTheRefundIsDeferred`.

- [x] **IR22 — MEDIUM — security** — `VerifyPaymentProcessor.cs:33`
  A `("verify", "app:N")` outcome was trusted on its reference alone, and for DoorSplit and Versus that
  verification alone confirms the booking. Payment accepts that reference from any consumer with any
  `PayerOwnerId`, and the event carries no payer identity, so another consumer setting up a method under it
  would confirm a B2B booking without the venue registering a card.
  **Disposition:** the processor resolves the application's venue tenant and validates the method with
  Payment against that payer before recording; a miss records the inbox row and skips with a warning.
  `MockPaymentSessionClient` binds each reference to its payer the way Payment's resolver does, covered by
  `Verification_FromAPayerOtherThanTheVenue_IsNotRecorded`. The failure arm stays unbound until the
  producer stamps the payer owner (IR23).

- [x] **IR23 — HIGH — correctness, Payment-owned** — `SetupIntentWebhookHandler.cs`,
  `PaymentSessionProviderRequest.cs:38` at `origin/main`
  The setup-intent webhook reads `operationType` and `clientReference` with the throwing accessor;
  session-created intents carry only `type` and `correlation`. Every B2B verification passes the
  `type == verify` guard and throws inside the outbox unit of work, so `PaymentSucceededEvent` is never
  published and DoorSplit and Versus bookings strand in `AwaitingConfirmation`. Not this diff's code. A
  producer PR must stamp `PaymentMetadataKeys.OperationType`, `ClientReference` and `PayerOwnerId` in
  `PaymentSessionProviderRequest.Create`, with a test that a session-shaped `setup_intent.succeeded`
  publishes; then publish and bump the four Payment pins here.
  **Disposition:** fixed on this branch, in Payment's source, so it merges with PR #633.
  `PaymentSessionProviderRequest.Create` stamps `operationType`, `clientReference`, `payerOwnerId` and,
  when present, `payeeOwnerId` beside the existing keys; `type` is untouched because it routes Payment's
  own transaction handlers. `FakeStripeSessionClient` keeps the request metadata per provider object, and
  `PaymentSessionWebhookReconciliationTests` now drives `setup_intent.succeeded` for a
  `PaymentMethodVerification` session with exactly that metadata and asserts one `PaymentSucceededEvent`,
  plus a `PaymentMethodSetup` session that publishes nothing. The B2B pins move when the package publishes
  from `main`.

- [x] **IR24 — MEDIUM — correctness** — `SettlementPaymentProcessor.cs:39`,
  `SettlementPaymentFailedProcessor.cs:33`, `VenueDashboardService.cs:119`
  Both settlement handlers guard only on operation type, then parse with the throwing readers and throw
  on a missing concert, all before the inbox row. `SettlementType` equals Payment's
  `TransactionTypes.Settlement`, so another consumer's settlement passes the guard and is redelivered until
  dead-lettered, blocking the shared subscription. The IR12 class, unfixed for the settlement pair; the
  dashboard's throwing read turns the same reference into a 500.
  **Disposition:** both processors guard on type, `Reference.TryGetConcertId` and
  `Metadata.TryGetOperationId` and return on a miss; a B2B-shaped reference naming no concert records the
  inbox row and logs `SettlementOutcomeForUnknownConcert`. The readers are extension members on
  `PaymentOperationReference` and on the metadata dictionary; the throwing `Read*` accessors are gone. The
  dashboard filters with the same reader. Covered by
  `SettlementOutcome_ForAReferenceThisServiceDidNotMint_IsSkipped` and
  `SettlementOutcome_WithoutAnOperationId_IsSkipped`.

- [x] **IR25 — LOW — correctness** — `AcceptanceFinancialOperationOutcomeProcessor.cs:150`,
  `BookingWorkflow.cs:218`
  `Validate` throws inside the inbox transaction on an operation-id or expected-operation mismatch.
  Unreachable for B2B-minted references; a foreign `("escrow", "booking:N")` event dead-letters and blocks
  the subscription. Skip with a warning after the inbox row instead.
  **Disposition:** `BookingWorkflow` keeps the checks as `Matches` and skips with
  `FinancialOutcomeSkipped` when the evidence does not match the booking on record, or the booking does not
  exist, leaving the inbox row committed. Covered by `CaptureSuccess_ForAnotherOperation_IsRecordedAndSkipped`.

- [x] **IR26 — HIGH — delivery** — `Directory.Packages.props`, `PaymentTopology.cs`,
  `Concertable.B2B.E2ETests.csproj`
  CI run 33980757957 on `db5d4be8c` fails `local-platform-pack` on `ConfirmedBooking.cs(18,5) CS0246`,
  because the committed props still pin Payment to platform 1329, and on `PaymentTopology.cs(10,52)
  CS0029`, because the committed file is main's shape. Both fixes are staged, not committed.
  `split-inventory` fails on the `ProjectReference` this range added from the E2E project to
  `Concertable.B2B.Infrastructure`. None of the three is a Customer legacy reference.
  **Disposition:** the pin and the topology reconciliation landed in `915e8d596`, and `local-platform-pack`
  went green on that head. The E2E project no longer references any B2B source project: `TestKit` carries
  `ConcertState`, `ApplicationStatus` and `PaymentOperationReferences` for the E2E tier, each guarded by
  `TestKitMirrorTests` against the type it mirrors, and `inventory.json` is regenerated with zero blocking
  E2E edges. The provider-contract inventory was also stale for this branch's call sites and is corrected.

- [x] **IR27 — LOW — test-coverage** — `PaymentOperationEnvelopes.cs:13`
  The simulated `PaymentSucceededEvent` metadata carries `operationType` and `clientReference`, which
  Payment stamps on settlement and escrow intents but not on session-created ones. The suites therefore
  prove the verify flow against a shape the producer does not produce, which is how IR23 stayed invisible.
  Align the double with the producer once IR23 lands.
  **Disposition:** closed by IR23 landing here: the producer now stamps the keys the double assumed, and
  Payment's own webhook test proves the publish against the real session metadata.

### Considered and not overturned

- The repeat accept-checkout double-hold rejection stands: `PaymentSessionService.CreateAsync` reserves on
  the reference and returns `OperationConflict` only on a genuine mismatch.
- The `ToDetails` TPH downcast was not re-examined; the file is untouched.

## Review pass — 2026-09-05 — incremental and security closeout

**Candidate base:** `db5d4be8cbd65195e0922cf3f648377f9fb31afb`
**Candidate head:** `3f89818c7c91b5cf9d658fbe7e8460163de06d78`
**Candidate branch:** `Refactor/launch_deal-lifecycle-modules-phase2`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:e00d0b402f72c19fbf50f7f137e385a58077ebc8fff58742368f1b1eb31e4926` `(89 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-review-633-3f89818c-incremental-17de4143`
**Candidate bundle identity:** `sha256:10d0d7b0c6c6221a69524d1d9b8016bae26cf962d1272fc0a6d5b35c4f6b553c`
**Security candidate base:** `39fbbc0126d6ddd8d40426e25663bea29cd7f1a5`
**Security candidate path-set:** `sha256:5b8af2f01c3b29144ae1c6fcae4821ff76c9f5868394560a328d21dc4b7811ed` `(530 paths)`
**Security candidate bundle identity:** `sha256:2e7aaf663a08038b667c98ed88fc96d3bdea259aaee296402c5208e20b90c90b`
**Work-order path:** `reviews/Refactor-launch_deal-lifecycle-modules-phase2.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

- [x] **IR28 — HIGH — security** — `VerifyPaymentFailedProcessor.cs:54-98`
  accepted a syntactically valid `verify/app:{id}` failure without proving that the Payment operation belonged
  to the application's venue. A foreign payer's event could therefore record a failure and notify users for
  another venue's application. The handler now derives the venue from B2B's persisted application, validates
  `operationId` plus payer ownership in Payment, inboxes unknown/foreign operations without domain mutation,
  and leaves provider-unavailable checks unconsumed for redelivery. Application integration tests cover a
  foreign payer and outage-then-retry. **Disposition:** closed in `d6d6ebf69`.
- [x] **IR29 — MEDIUM — correctness** — `PaymentSessionProviderRequest.cs:46`
  stamped the operation id as private provider metadata key `operation_id`, while B2B's v1 event consumer reads
  public `PaymentMetadataKeys.OperationId` (`operationId`). Production verification failures consequently
  failed the shape guard that the mocks passed. The producer now uses the public key and its webhook integration
  test asserts the exact metadata value. **Disposition:** closed in `d6d6ebf69`.
- [x] **IR30 — LOW — test coverage** — `BookingCancellationApiTests.cs:126`
  covered deferred-refund followed by capture success but not capture rejection, the ordering most likely to
  strand a booking in `CancellationPending`. The new integration case defers the refund, rejects the in-flight
  capture, and proves terminal `Cancelled` with no Concert. **Disposition:** closed in `d6d6ebf69`.
- [x] **IR31 — LOW — maintainability** — changed Booking, Concert, integration-fixture, and package-pin files
  added narrative comments explaining the reviewed design. Those comments were removed; the incremental source
  diff now adds no comments. **Disposition:** closed in `d6d6ebf69` and `17ad067e1`.

### Adversarial money-path adjudication

1. **Deferred refund versus in-flight capture.** The deferred event records only its inbox row
   (`CancellationFinancialOperationOutcomeProcessor.cs:33-37`) and leaves the Booking in
   `CancellationPending`. If capture succeeds, `BookingWorkflow.cs:173-179` reissues the same cancellation
   operation id/reference/reason; Payment's pending operation fingerprint matches, the now-present escrow is
   refunded, and the refund success moves the Booking to `Cancelled`. If capture fails,
   `BookingWorkflow.cs:207-212` applies `Cancel`, also ending `Cancelled`; this is proven at
   `BookingCancellationApiTests.cs:126-140`. Capture-first produces the same two terminal paths after the
   cancellation transaction commits. A redelivered deferred envelope is inbox-skipped; a later success has a
   distinct event-type-stable message id and is not suppressed. No interleaving leaves captured money on a
   cancelled Booking or leaves the Booking pending.
2. **Inbox while waiting.** The deferred arm goes through `TryRecordInboxAsync`; every owned cancellation
   outcome does the same before its effect (`CancellationFinancialOperationOutcomeProcessor.cs:68-84`). The
   acceptance handler writes inbox plus transition in one transaction, and its fresh-scope concurrency retry
   rolls both back before retrying. Foreign reference shapes intentionally return before inbox because they
   belong to another consumer and perform no effect.
3. **Venue binding.** Success validation resolves `VenueTenantId` from B2B's application and Payment resolves
   the whole reference against that owner. Failure validation now resolves the same B2B-owned venue and asks
   Payment for the operation by `operationId` and owner (`VerifyPaymentFailedProcessor.cs:58-74`). Nothing on
   the inbound event supplies the authoritative tenant. Venue A's operation cannot advance or fail venue B's
   application.
4. **Stamped references.** FlatFee opens `EscrowHold(app:{id})`; Booking freezes it as the authorization and
   later emits booking-scoped `Escrow(booking:{id})` for `CaptureEscrowCommand`
   (`FlatFeeConfirmStep.cs:38`). VenueHire freezes `MethodSetup(opportunity:{id}:artist:{tenant})` as the payment
   method and emits the same booking-scoped escrow identity for `DepositEscrowCommand`
   (`VenueHireConfirmStep.cs:36`). Payment resolves either frozen dependency by its whole reference and compares
   `PayerOwnerKey` server-side (`PaymentOperationResolver.cs:100-117`). The two-reference shape is deliberate:
   the new operation is Booking-owned while the underlying authorization/method retains its original identity.
5. **Double effect.** Capture, deposit, and refund enter `FinancialOperationHandler.PrepareAsync`, whose stable
   operation id/fingerprint rejects changed facts and whose terminal replay republishes without calling the
   provider. Refund reservations resume by the same operation id; completed refunds replay. Release atomically
   reserves `EscrowEntity.ReleaseOperationId` and returns the existing transfer on replay. Settlement loads its
   unique operation id and compares its fingerprint before charging. Provider calls also receive operation-based
   idempotency keys. B2B inboxes independently prevent duplicate outcome transitions; none of these guarantees
   relies on a mock.

### Security closeout

- Tenant-scoped HTTP paths still derive authority from the active tenant/membership boundary. The internal
  cross-tenant reads above recover B2B-owned facts by aggregate id and fail closed; no endpoint or projection
  gained caller-selected tenant authority.
- Whole `PaymentOperationReference` values remain intact across contracts. Type guards precede every encoded-id
  parse, foreign shapes are skipped without throwing, and Payment revalidates payer ownership before resolving
  authorization or payment-method dependencies.
- No provider identifier entered B2B state, contracts, logs, DTOs, or frontend. The reviewed logging contains
  references and failure classification only, with no new personal-data disclosure.
- IR19's settlement-completion-before-inbox ordering remains bounded by Settlement's operation/state idempotency,
  retains its existing `wontfix` debt owner, and was not widened by this range.

### Validation

- `./scripts/integration.ps1 Application` — 73/73 passed.
- `./scripts/integration.ps1 Booking` — 24/24 passed.
- Focused Payment webhook metadata test — 1/1 passed.
- Final focused Application ownership/retry tests — 3/3 passed.
- The clean B2B solution build reached only the two explicitly out-of-scope Customer legacy contract errors
  (`CheckoutSession`; sealed `PaymentOutcome`); all B2B projects compiled. No E2E was run.

## Review pass — 2026-09-05 — remediation incremental

**Candidate base:** `3f89818c7c91b5cf9d658fbe7e8460163de06d78`
**Candidate head:** `17ad067e154616d853a59879d8d9dc8dd4f7faa2`
**Candidate branch:** `Refactor/launch_deal-lifecycle-modules-phase2`
**Candidate scope:** `all`
**Work-order path:** `reviews/Refactor-launch_deal-lifecycle-modules-phase2.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No new findings. The branch moved after the original bundle was frozen: concurrent commit `026180690` added a
local-platform-only Payment version override. Its default remains published version `0.1.0-alpha.0.1322`; only
`UseLocalPlatformPackages=true` follows the locally packed platform version, as exercised by the green module
integration runs. The remediation diff closes IR28–IR31 and introduces no compatibility surface, provider
identifier, loose payment reference, Customer change, or added source comment. All findings have terminal
dispositions and both review watermarks cover the final fixing head.

## Review pass — 2026-09-06 — origin/main merge and first-ever green CI matrix

**Candidate base:** `17ad067e154616d853a59879d8d9dc8dd4f7faa2`
**Candidate head:** `ed4ff99c8a604a93dfaf2022551284a6541348a3`
**Candidate branch:** `Refactor/launch_deal-lifecycle-modules-phase2`
**Candidate scope:** `all`
**Work-order path:** `reviews/Refactor-launch_deal-lifecycle-modules-phase2.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

The range merges `origin/main` `abd3466e3` (Customer's delivered payment-reference migration) into the branch.
The merge is mechanically clean: `git diff-tree --cc` over the merge commit contains no source file at all, only
`plans/launch/LAUNCH_ROADMAP.md`, where both sides' edits survive. Every Customer-owned path in the merged tree is
byte-identical to `main` except two files this branch already owned inside the previous watermark —
`Concertable.Customer.Hosting/CustomerTopology.cs`, forced by this branch's own `AsbTopology.WithService` scoping
API, and `api/Concertable.Customer/TECH_DEBT.md`. No Customer or Payment behaviour was changed to resolve the merge.

With the merge in place `api/Concertable.slnx` compiles for the first time since #933, which meant CI ran this
branch's test matrix for the first time ever — every previous run on this branch died at `build`, gating the whole
matrix off. Two real defects surfaced immediately. Both are this branch's and both are fixed here.

### Findings

- [x] **IR32 — HIGH — correctness** — `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Domain/Entities/OpportunityEntity.cs:11`
  The module carve regressed `Genres` from `EfSet<Genre>` to a `HashSet<Genre>` field behind a computed
  `List<Genre> PersistedGenres` shim — exactly the shape commit `b610d9eeb` had already rejected and documented as
  unusable ("EF 10's materializer/comparer/JSON-reader hard-cast to `IList<T>`"). Artist and Concert kept `EfSet<Genre>`;
  Opportunity alone did not. The consequence is not a throw but silent data loss: the HTTP response carries the right
  genres because it is mapped from the in-memory aggregate, while the JSON column never round-trips, so **every
  Opportunity re-materialises with zero genres**. That empties genre-based opportunity matching
  (`GetMatchCandidatesAsync` treats an empty set as "matches everything") and every genre a venue ever set on an
  opportunity. Reproduced locally and in CI by the branch's own
  `OpportunityApiTests.Create_DuplicateGenres_PersistDistinct_AndReMaterialiseFromJsonColumn`, which failed on the
  *persisted* assertion while the response assertion passed. Fixed by restoring `EfSet<Genre>` and mapping it the way
  Artist and Concert do (`builder.PrimitiveCollection(o => o.Genres)`); the string-named shadow property and the
  `QueryablePrimitiveCollectionExtensions` helper that existed only to reach it are deleted, and the genre-overlap
  predicate is inlined at its single call site. The Opportunity migration is re-scaffolded: same `Genres`
  `nvarchar(max)` column, only its ordinal position in `CreateTable` moves. Opportunity integration 14/14.

- [x] **IR33 — HIGH — correctness** — `api/Concertable.Payment/provider-contract-inventory.json`
  `ProviderContractInventoryTests` failed five ways. Four committed entry points no longer exist (Customer's two
  `customerPaymentClient` calls and the two frontend `client-secret-id-split` parsers, all retired by main's
  migration) and two live entry points were unclassified: Customer's new `paymentSessions.CreateAsync` and **this
  branch's** `VerifyPaymentFailedProcessor.HandleAsync` to `paymentSessions.GetStatusAsync`. Neither side's own CI could
  catch it — the classifier scopes the test matrix to the changed service, so Customer's PRs never ran Payment's unit
  tier and this branch never got past `build`. The inventory is reconciled to the merged tree: the four dead entries
  removed, the two live ones classified (`customer-ticket-payment` and `b2b-save-or-verify-method`), and the now
  orphaned `frontend-ticket-web-correlation` decision deleted so `Decisions_AreUniqueAndReferenced` still holds.
  `customer-ticket-payment`'s `identity`/`compatibility` prose still described the retired Stripe-intent-id
  correlation and is corrected to the delivered operation-reference shape. Payment unit tier 551/551.

- [wontfix] **IR34 — HIGH — correctness** — `api/Concertable.B2B/src/Modules/Dashboard/Venue/Concertable.B2B.Dashboard.Venue.Infrastructure/VenueDashboardService.cs:61`
  The venue revenue KPI and its six-month chart are structurally, permanently zero. `GetPaymentRevenueAsync` sums
  `PaymentTransactionEntity`, whose only writer is registered under `TransactionTypes.Payment` (`"payment"`), and
  nothing in the system emits that key — `PaymentSessionProviderRequest` stamps `type` with the operation's own
  `OperationType`, and the only payment-kind operation is Customer's `"ticket-purchase"`. Verified end to end.
  This is not a renaming artefact: the pre-v1 `GetTicketRevenueAsync` summed the same table against the
  `TransactionTypes.Ticket` key that v1 deleted, and v1 offers B2B no ticket-scoped replacement. The fix is either
  Payment-side (key the recorder on the operation kind, and stamp `AmountMinor`, which the recorder reads and nothing
  writes) or B2B-side via the already-open `ConcertSalesProjection`. Both are out of this PR's scope — Payment source
  is explicitly excluded and `ConcertSalesProjection` is its own piece of work — so this lands as a HIGH entry in
  `api/Concertable.B2B/TECH_DEBT.md` ("Venue dashboard revenue reads a table nothing writes") rather than a
  silent regression.

- [wontfix] **IR35 — HIGH — correctness** — `api/Concertable.Customer/src/Modules/Ticket/Concertable.Customer.Ticket.Application/Payments/TicketPaymentOperationReferences.cs:27`
  Customer's purchase reference is `buyer:{id}:concert:{id}:quantity:{n}` — a repeatable user action with no
  per-attempt discriminator — while Payment enforces `(OperationType, ClientReference)` as a unique idempotency key and
  normalizes the caller's fresh `OperationId` out of the replay fingerprint. A buyer's *second* single-ticket purchase
  for the same concert composes a byte-identical reference and every other fingerprint term matches, so Payment
  replays the first, already-succeeded intent: the API returns a valid-looking checkout, the buyer is never charged,
  and no second ticket is minted. This is entirely Customer and Payment code, already merged to `main`, and explicitly
  outside this PR. Recorded here because the branch's own reference scheme was reviewed against the same contract and
  is *not* affected — B2B keys every reference on an entity id that exists once per intended operation, so its replay
  is deliberate. Transferred to `api/Concertable.Customer/TECH_DEBT.md` (HIGH, "A repeat ticket purchase replays
  the first payment instead of charging again").

- [wontfix] **IR36 — LOW — package coherence** — `api/Concertable.Shared/Directory.Packages.props:63`
  `Concertable.Payment.Hosting` is pinned at `$(ConcertablePlatformVersion)` (1329) while B2B and Customer pin every
  Payment package at 1322 through the split `ConcertablePaymentVersion`. Inert today because its only consumer is a
  `tests/`-path project, which `PlatformSourcePackages.targets` swaps to a project reference. It becomes a restore
  failure the moment a carve or a non-test consumer resolves it from the feed. Not changed here: confirming whether
  `Payment.Hosting` exists on the feed at 1329 needs feed access this session does not have, and blind-editing a pin
  during delivery is the wrong trade. Transferred to `api/Concertable.Shared/TECH_DEBT.md`, whose resolution condition is to give Shared the same
  `ConcertablePaymentVersion` block after confirming the published Payment heights against the feed.

- [wontfix] **IR37 — MEDIUM — correctness** — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Infrastructure/Services/ApplicationCheckoutService.cs:111`
  The accept checkout passes `Guid.CreateVersion7()` as the FlatFee authorization's `OperationId`, minting a fresh id
  on every GET of the checkout page, where every other operation-id site in B2B is `??=`-stable. It does not
  double-charge today only because Payment's duplicate-key fallback re-resolves by reference. Correctness rests on
  Payment's fallback rather than on the reference B2B already owns. Transferred to `api/Concertable.B2B/TECH_DEBT.md` ("Accept checkout mints a throwaway authorization operation
  id") rather than a late edit to the accept money path.

- [x] **IR38 — HIGH — correctness** — `api/Concertable.Shared/tests/Concertable.Shared.Api.UnitTests/TypedResultArchitectureTests.cs:335` and `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Contracts/Concertable.B2B.Application.Contracts.csproj:14`
  The next CI run, with the matrix finally reaching the Shared tier for the first time, failed three more
  self-verifying architecture assertions. Two were the transitional typed-result allowlist naming
  `Concertable.Payment.Infrastructure/CustomerPaymentService.cs` and `ManagerPaymentService.cs`, both deleted by
  Payment v1 on `main`; the guard `Single()`s on the path and threw `Sequence contains no matching element`. With both
  slices gone the transitional exemption has actually completed, so the allowlist, the theory guarding it, and the
  `IsTransitionalTypedResultSlice` exclusion are all removed — `TypedResultSlices_DoNotUseHttpExceptions` now scans
  every production file with no exemption and still passes. The third was
  `DunetReferences_BelongToProjectsDeclaringUnions`: `Concertable.B2B.Application.Contracts` still carried a `Dunet`
  package reference after this branch's A2 boundary hardening deleted the union it was there for. Reference removed;
  the project declares and uses no union. Shared.Api unit tier 83/83.

- [x] **IR39 — HIGH — correctness (test isolation)** — `api/Concertable.B2B/tests/Concertable.B2B.IntegrationTests.Fixtures/ApiFixture.cs:165`
  `Concertable.B2B.Lifecycle.IntegrationTests` failed on CI at a head whose only delta from a fully green run
  was four markdown files, so the suite is non-deterministic. `ResetAsync` Respawns the database between
  tests while `OutboxDispatcher` — a `BackgroundService` polling every second — may already hold a leased
  batch in memory: `GetPendingAsync` marks rows `Dispatching` and returns them, and delivery happens after.
  Wiping the table does not recall that batch, so the previous test's messages are delivered *into the next
  test*. That is how `Accept_ShouldNotConfirmBooking_WhenWebhookFails` saw a concert-draft notification: its
  three product assertions all passed (booking `ConfirmationFailed`, concert `404`), and the run log shows a
  `booking-confirmed.v1` dispatch inside a test that confirms no booking — a message that can only have come
  from its predecessor. That dispatch threw `KeyNotFoundException` because the in-memory transport's map had
  already been reset, and it appears zero times in the green run. Locally the suite failed 1 run in 2 before
  the fix, on a *different* test (`BookingCancellation_RetryUsesNewOperationAndCompletes`) — a different
  victim each time is the signature of bleed rather than one bad test, and at that rate it would have jammed
  the merge queue repeatedly. `ResetAsync` now waits for zero `Pending`/`Dispatching` rows before Respawn:
  because a batch is marked `Dispatching` in the database before any of it is delivered and `Dispatched`
  after, an empty set is proof no batch is in the dispatcher's hands. Bounded at 30s and it throws with the
  stuck count rather than proceeding, so an undrainable row is a loud failure instead of a silent flake. No
  sleep, no production change, no test behaviour changed. `runDispatcher: false` was rejected as the lever:
  it also strips the dispatch resolver, so tests could not dispatch at all. Customer, Payment and Auth
  register the same dispatcher and their fixtures carry the same latent hole; they are green and out of this
  PR, recorded in `api/Concertable.B2B/TECH_DEBT.md`.

### Validation

Full `api/Concertable.slnx` build 0 errors. **Every unit and architecture suite in the repository — all 42 projects,
~1,895 tests — run locally with 0 failures**, rather than discovering them one fail-fast CI round-trip at a time.
CI run `34031270386` at head `418a1568b` proves the whole integration tier green: **all 27 integration jobs
succeeded** (14 B2B, 8 Customer, 2 Payment, Search, DataAccess, Auth), with `Shared.Api.UnitTests` its only
failure — the one IR38 fixes. Frontend `test:boundaries` 8/8,
`lint:boundaries` clean, `build:web-packages`/`build:venue`/`build:artist` all succeeded. `git diff --check` clean.

## Review pass — 2026-09-06 — request-scoped tenant carrier

**Candidate base:** `0cf710c7e05855a7ed906ee0c9f2c7f7811d4c3b`
**Candidate head:** `4285ff3bbcfe88317e75c149c3fb360e87b33b8e`
**Candidate branch:** `Refactor/launch_deal-lifecycle-modules-phase2`
**Candidate scope:** `all`
**Work-order path:** `reviews/Refactor-launch_deal-lifecycle-modules-phase2.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No new findings. The range moves the resolved tenant off `TenantContext`'s scoped fields onto the request, so
every dependency-injection scope opened inside a request reads the same resolution instead of a blank one.
That closes the defect `AcceptOnceAsync`'s re-resolve was compensating for, and the re-resolve, its
`ITenantResolver` dependency and its explanatory comment are deleted with it.

Three things were checked rather than assumed, because the new accessor's setter throws when there is no
request and a reachable throw would be a live fault on every non-HTTP path:

- `ITenantContextAccessor.Resolution` is assigned in exactly two places, both inside
  `TenantContext.ResolveAsync`, which returns early when `accessor.Resolution is not null || IsHost`. With
  `IsHost` true precisely when there is no `HttpContext`, the throwing setter is unreachable off the request
  path; a worker, the outbox dispatcher and an event handler all return before it.
- `AcceptOnceAsync` has one caller — the retry at `ApplicationWorkflow.cs:192`, which opens a fresh
  dependency-injection scope inside the same request, not a new request. `IHttpContextAccessor` flows into
  that scope, so dropping the re-resolve leaves the retry reading the tenant the middleware resolved.
- `ITenantResolver.ResolveAsync` has two remaining callers, `TenantResolutionMiddleware` and
  `PermissionAuthorizationHandler`, both on the HTTP path.

Registering the accessor as a singleton over `IHttpContextAccessor` is the right lifetime: the state lives in
`HttpContext.Items`, so the accessor itself holds none. The commit message records that an `AsyncLocal`
holder was tried first and rejected because a value assigned inside an async method is invisible to its
caller — the resolution happens one frame below the middleware, which is exactly where that carrier loses it.

`IsHost => httpContextAccessor.HttpContext is null` keeps a non-HTTP caller bypassing every tenant row
filter. That is unchanged pre-existing behaviour, deliberate and documented on the member (an anonymous HTTP
request keeps `IsHost` false and so fails closed), and not introduced by this range. Making the host stance
an explicit flag rather than an inference from a missing `HttpContext` is a separate change and is not
required for this pass.

### Validation

Exact-head CI owns this range. The pass records the reachability analysis above; it asserts no local run.
