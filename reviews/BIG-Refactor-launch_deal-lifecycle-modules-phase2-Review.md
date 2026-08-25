# Big review — Refactor/launch_deal-lifecycle-modules-phase2

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Plan anchored to commit:** `c50469d483f697890dc9b4f3d2b3013ee1b8c1c9`  _(2026-08-23)_
**Security-reviewed up to commit:** `c50469d483f697890dc9b4f3d2b3013ee1b8c1c9`  _(2026-08-23)_
**Reviewed up to commit:** `c50469d483f697890dc9b4f3d2b3013ee1b8c1c9`  _(2026-08-23)_
**Post-anchor incremental drift reviewed through:** `6ba7a13c5336e66346ec3e9313d13be44bca6b1b`  _(2026-08-23)_
Net diff reviewed: `fb561acee..c50469d48`. Move-only files skipped.
Each staged area stayed fixed to the anchor; the separate post-anchor reconciliation now covers published HEAD `6ba7a13c5`.
Status legend: `[ ]` not yet reviewed · `[x]` reviewed (date) · `[~]` in progress (incomplete — re-review).

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
the final section. The PR is not merge-ready while the open high findings and exact-head CI failures
remain.

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
- [ ] **NAT17 — HIGH — correctness / transactionality** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/ConcertService.cs:91`
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
- [ ] **MB6 — HIGH — module boundary / test topology** — `api/Concertable.B2B/src/Modules/Booking/Tests/Concertable.B2B.Booking.IntegrationTests/ContractApiTests.cs:27`
  The contract suite initiates Opportunity creation and Application acceptance, then verifies Booking's private Booking/Contract persistence at `GetContractAsync` (`:302`). That is a complete cross-module journey inside a module-owned integration project, contrary to the corrected topology. Either initiate Booking through its public Application-contract fact and keep Booking-owned persistence assertions, or move the full journey to `Concertable.B2B.Process.IntegrationTests` and assert the Contract through Booking's public boundary.
- [x] **NAT20 — MEDIUM — test coverage** — `api/Concertable.B2B/src/Modules/Venue/Tests/Concertable.B2B.Venue.IntegrationTests/TenantScopingTests.cs:43`
  `GetAllByTenantId_ReturnsOnlyThatTenantsVenues` no longer invokes `IVenueRepository.GetAllByTenantIdAsync`; it duplicates the tenant predicate directly over `fixture.Venues`, so the named repository behaviour can regress while this test remains green. Resolve the Venue-owned repository in the module fixture/scope and exercise the real operation, or rename and re-home the test if it is intended to cover only the read-context stance.
- [ ] **CV9 — MEDIUM — test-tier conventions** — `api/Concertable.B2B/src/Modules/Opportunity/Tests/Concertable.B2B.Opportunity.UnitTests/Services/OpportunityDashboardServiceTests.cs:23`
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
- [ ] **CV10 — MEDIUM — test-tier conventions** — `api/Concertable.B2B/tests/Concertable.B2B.Workers.UnitTests/Functions/ConcertFinishedFunctionTests.cs:14`
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
