# Application, Booking, and Concert module ownership progress

- Plan: `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/deal-lifecycle-ownership`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules-phase2`
- Branch: `Refactor/launch_deal-lifecycle-modules-phase2`
- PR: [#633](https://github.com/Concertable/concertable/pull/633), out of draft and merging through the
  merge queue. The candidate carries the completed state-machine cutover, the IR1–IR31 fixes, and the
  Payment v1 consumer cut-over merged up to `main` at `abd3466e3`.
- Dependency/package gates: Deal producer PR #678 and platform sync #694 are terminal at
  `Concertable.Platform 0.1.0-alpha.0.1108`. Kernel producer PR #719 published
  `Concertable.Kernel 0.1.0-alpha.0.1133`, and platform sync PR #730 produced the B2B platform pin
  `0.1.0-alpha.0.1158`. B2B consumes the Kernel state machine directly and every consumer directly pins
  `Reunion 0.1.0-alpha.8` rather than relying on Kernel's transitive reference. No producer gate remains.
- Last reconciled: 2026-09-06 from local Git, GitHub PR #633, the active review work order, and focused
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
method-header case per DealType. No family needs that escalation today: with Payment owning payment-method
commitments, apply and accept take the same arguments for every deal type, so Application's `IApplyStep` and
`IMintCommitment` are same-interface families. `DealUnionBuilder` stays for the first family that genuinely
fractures on caller input.

## Payment v1 consumer cut-over

B2B now consumes only Payment's consumer-agnostic v1 surface (producer PR
[#933](https://github.com/Concertable/concertable/pull/933)). No provider identifier crosses the boundary
and none is persisted: `PaymentOperationReference` is minted once in
`Concertable.B2B.Infrastructure/Payments/PaymentOperationReferences`, frozen onto `ContractEntity`, and
read back by Booking and Concert. `BookingEntity.FinancialOperationReferenceId`,
`ConcertEntity.FinancialOperationReferenceId`, `VerifyPaymentEntity.ProviderTransactionId` and
`SettlementConfirmation.ManagerPaid(transactionId)` are gone; the Payment-owned `Guid OperationId` is the
only operation identity B2B stores. The legacy `IManagerPaymentOperationsClient` /
`IManagerPaymentReportingClient` / `CheckoutSession` / `FindHeldIntentAsync` surface is replaced by
`IPaymentSessionOperationsClient`, `ISettlementOperationsClient` and `IPaymentReportingClient`, and the
`*ByReference` escrow commands collapse back into `CaptureEscrowCommand` / `DepositEscrowCommand` /
`RefundEscrowCommand` carrying the reference. B2B's `Checkout.Session` is now a B2B-owned
`CheckoutSession`, so the SPA contract is unchanged while the Payment type no longer reaches the HTTP edge.

Concert's ticket-sold counter no longer sniffs `PaymentSucceededEvent` metadata for
`type=ticket`/`concertId`/`quantity` — those keys are deleted in v1. `TicketSaleProcessor` subscribes to
Customer's already-published `TicketPurchasedEvent` instead, which is where a ticket sale is actually
owned; the `ConcertSalesProjection` end state in `api/Concertable.B2B/TECH_DEBT.md` stays open.

### Published-package evidence

The producer is delivered and this consumer is validated against the real feed, not a local artifact.

- Producer PRs: [#933](https://github.com/Concertable/concertable/pull/933) merged as
  `3f7fd95cc0fc4a6fbd2168f035cce3088fc3d22f`, and the JSON-transport fix
  [#937](https://github.com/Concertable/concertable/pull/937) (reviewed head
  `4346770d56cbbc81bcc774bde92e3c40bc89ad88`) merged as
  `ea33c48e66ec814d2341b35b72be73a04f8cc347`, which is the source commit the packages were built from.
- Published Payment version: **`0.1.0-alpha.0.1322`** — `Concertable.Payment.Client`,
  `Concertable.Payment.Contracts`, `Concertable.Payment.Hosting`, `Concertable.Payment.TestKit`.
- Package workflow (green, reported "Your package was pushed" for every Payment package, fresh-consumer
  feed restore passed): <https://github.com/Concertable/concertable/actions/runs/33978948615>.
  Corrected image workflow (green): <https://github.com/Concertable/concertable/actions/runs/33978948571>.
  #933's own package and image runs were cancelled before their push steps, so no broken artifact escaped.
- SHA-256 of the packages this branch actually restored from the feed:
  `Concertable.Payment.Contracts` `4cf37682e1e3e6d7fd7e175c591cf8b920b08cecb4da9136100fcca566551539`;
  `Concertable.Payment.Client` `68b7e9aec20c60bc7824fa1af49c8f487c3658b347dbce36e57f2cc0993cef5a`.
  `Payment.Hosting` and `Payment.TestKit` are not restored here: `PlatformSourcePackages.targets` swaps
  them to `ProjectReference` for `*.AppHost` and `/tests/` projects, so only the two `src` packages come
  from the feed in this solution.
- The `[JsonConstructor]` blocker recorded below is **resolved by #937**, which added the attribute and a
  serialize/deserialize regression test. It is history now, not an open gate.

**The platform pin deliberately stays at `0.1.0-alpha.0.1329` while Payment moves to `0.1.0-alpha.0.1322`.**
The alpha heights are not monotonic in time — `1329` was published 2026-09-02 and `1322` on 2026-09-05 — and
`Concertable.Payment.Contracts 1322` declares `Concertable.Kernel >= 0.1.0-alpha.0.1329`. Moving
`ConcertablePlatformVersion` to `1322` therefore downgrades Kernel beneath what Payment itself requires and
fails restore with `NU1605` (warning-as-error). Only the four Payment packages carry the explicit `1322`
pin; every other platform package stays on `$(ConcertablePlatformVersion)`. B2B is the only service bumped —
Customer's pin rides its own migration.

### Preparation evidence, retained as history

Before publication this consumer was proven against an exact artifact packed from #933's head
`ec11b801fb314929552f4907ddf81361ea05d4ab` (reviewed watermark
`6018baa840aac6ae0c493b14fcdcb77a3ab13774`) as version `0.1.0-local.ec11b801f`, consumed through
working-tree-only edits to `api/Concertable.B2B/nuget.config` and `api/Concertable.B2B/Directory.Packages.props`
plus a working-tree-only overlay of #933's `api/Concertable.Payment` source. Every one of those inputs was
reverted before the push; no machine-specific feed, temporary path or disposable version pin was ever
committed. That artifact is superseded by the published packages above and is recorded only so the earlier
green runs in this ledger can be interpreted.

### Pre-existing branch red the cut-over uncovered

`Concertable.B2B.Application.Infrastructure` did not compile before this work, so every project downstream
of it — the Application, Booking, Concert, Dashboard and Lifecycle integration suites and the Concert unit
suite — had never been built or run on this branch. Compiling them surfaced four defects that predate the
Payment cut-over and are fixed here:

- `BookingFactory` left the contract on `BookingEntity.Contract`, so the seeder's booking save dragged the
  contract into the same `IDENTITY_INSERT` window and every B2B fixture failed at seed. The seed aggregate
  now clears the navigation.
- `ConfirmedBookings` hard-coded tenant ids that four Concert unit tests mocked with fresh GUIDs, and two
  door-revenue tests dated `now` before the fixture's 2035 concert. Both now read the fixture's constants.
- `ApplicationEntity.Accept` raised only `ApplicationAcceptedDomainEvent`, so a verification recorded
  *before* the acceptance was never replayed against the booking the acceptance creates and the booking sat
  in `AwaitingConfirmation` forever. `Accept` now re-raises the recorded verification after the acceptance
  event, which is the durable replayable join NAT11 asked for.
- `ConcertEntity`'s settlement payment reference was never mapped, so the column did not exist and the
  reference read back empty on every settlement. `ConcertEntityConfiguration` now maps it as a complex
  property beside the financial failure, and the Concert migration is re-scaffolded.
- `DoorRevenueOutstandingSpecification` downcast to the **abstract** `DoorRevenueConcert`, which EF cannot
  translate at all, so `/api/venue-dashboard/kpis` returned 500 and the completion sweep's query threw. It
  now casts to the concrete `DoorSplitConcert` / `VersusConcert` leaves, which EF translates to the
  discriminator, in the one place the predicate is defined.
- `ConcertApiFixture.FailSettlementPersistenceAsync` armed its CHECK constraint on the provider-reference
  column this change removes; the state half alone already admits the reservation and rejects the
  completion, which is what the constraint is for.
- Four Application/Concert tests asserted the retired contract (a `pi_` client-secret prefix, apply
  succeeding with no committed method, accept requiring one). They now assert the v1 behaviour: apply
  without a commitment is `402`, accept before verification is `204` and waits.
- `MockPaymentTransport` could only settle a *pending* command, so a second webhook for a flow whose only
  Payment operation moves over the bus threw instead of redelivering. Each outcome now carries an envelope
  id stable per operation, and after the wait window finds nothing pending the settled command's outcome is
  repeated — which is what the bus does and what the inbox dedupes.
- `scripts/integration.ps1` carries a hand-written roster that never gained the six integration projects
  this branch adds (Application, Booking, Dashboard, Deal, Opportunity, Lifecycle), so the local entrypoint
  could not see or run the suites covering this change. CI discovers by `find` and always ran them; the
  roster now matches. `Admin` and `E2EAdmin` were missing before this branch and are left alone.

The venue dashboard's revenue chart changed meaning, not just names: v1 has no ticket-scoped reporting
query, so what was `charts/ticket-revenue` now reports every payment where the tenant is payee. It is
renamed end to end — `charts/payment-revenue`, `GetPaymentRevenueAsync`, `useVenuePaymentRevenueQuery`,
card title "Revenue" — rather than left saying something the number no longer means.

`Modules/Deal/ARCHITECTURE.md` §2.7 and two tech-debt entries described the retired surface
(`IManagerPaymentClient`, `FindHeldIntentAsync`, the `*Step` names, and a "resolves when `ManagerPayment`
gains a `CancelHeldIntent` RPC" that v1 makes unreachable). All three now describe the v1 shape.

### Invariant sweep and its deliberate survivors

`PaymentMethodId` / `paymentMethodId` / `payment_method_id` / `PaymentIntentId` / `paymentIntentId` /
`payment_intent_id` / `SetupIntentId` / `ChargeId` / `TransferId` / `RefundId`, case-insensitively over
`api/Concertable.B2B`, `app/web/b2b` and `app/web/shared`, is zero except:

- `app/web/shared/.../checkout/StripePaymentForm.tsx` and `.../payments/NewCardSection.tsx` — the browser's
  own Stripe adapter reading `intent.payment_method`, still offered to callers through `onSuccess` /
  `onConfirmed`. B2B stopped consuming it; narrowing the shared tier is publish-first and is recorded in
  `api/Concertable.B2B/TECH_DEBT.md`.
- `Concertable.B2B.E2ETests` / `.Ui` — provider ids **read back from Payment's own database** through the
  Payment TestKit and asserted against real Stripe objects. That is what the E2E tier is for; nothing B2B
  sends carries them, and the addressing is now `PaymentOperationReference`-shaped via
  `Concertable.B2B.E2ETests/PaymentOperationsDb`.
- Old generated migration snapshots are gone: Application, Booking and Concert were re-scaffolded, so no
  `PaymentMethodId` / `SettlementPaymentMethodId` column survives anywhere in B2B's model.

`api/Concertable.Customer` and `app/customer/shared` still consume the removed contract and are **not**
touched here; that consumer is `plans/launch/CUSTOMER_PAYMENT_REFERENCE_PROGRESS.md`'s.

### Producer defect found while validating — resolved by #937

`PaymentOperationReference` does not survive a JSON round-trip: it is a `readonly record struct` whose
parameterized constructor carries no `[JsonConstructor]`, so `System.Text.Json` binds the implicit
parameterless constructor and every value is lost. Serializing produces
`{"OperationType":"escrow","ClientReference":"booking:48"}`; deserializing yields `('', '')`. That silently
empties the reference on every escrow command and event crossing the outbox, and B2B's Booking integration
suite fails on exactly that. Verified with a standalone probe against the packed v1 assembly, and confirmed
fixed by adding `[JsonConstructor]`, applied only to the local overlay at the time to prove the consumer.
PR #937 landed exactly that attribute plus a serialize/deserialize regression test, and the packages this
branch now restores are built from its merge commit, so the defect cannot reach B2B.

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

Both consumers of Payment v1 are now delivered. Customer's migration merged to `main` (#939 published the
`@concertable/customer` package, #938 landed the service and standalone carves), so the two compile errors
that kept `main` red alongside B2B's are gone from `main` itself. `origin/main` at `abd3466e3` is merged
into this branch, and the six B2B references this PR owns — `ArtistDashboardService` /
`VenueDashboardService` (`IManagerPaymentReportingClient`), `Concert.Application/Responses/Checkout.cs`
(`CheckoutSession`), and `FinishConcertError.cs` (`ManagerPaymentError`) — are resolved against the reviewed
v1 consumer shapes. No Customer or Payment source was modified to get there.

The merged tree therefore builds green as a whole for the first time since #933: `api/Concertable.slnx`
compiles with 0 errors. **PR #633 no longer needs an admin merge and must not get one** — it goes through
the merge queue so the E2E tier validates it against current `main`.

After terminal merge, the causally triggered consequences are the package publish for B2B's published
contracts and the generated `ConcertablePlatformVersion` sync PR, which finally retires the split Payment
pin recorded above. Only when those are terminal is this ledger's lifecycle complete and the plan, ledger
and review artifact deleted.

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
- Replaced operation-specific executors with one executable module-local workflow per Application, Booking,
  and Concert. Deal-varying lifecycle leaves remain discrete `*Step` contracts selected through
  `IDealStrategyFactory<TStrategy>` so implementations can be shared and recombined by `DealType`.

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
- Step naming slice: Application, Booking, and Concert Infrastructure builds completed with 0 warnings and
  0 errors; Application 20/20, Booking 9/9, Concert 105/105, Deal 47/47, and B2B Architecture 29/29 passed.
  The repository-wide old-name and old-filename sweep returned zero matches.
- A local Concert integration diagnostic reached 38 passing B2B cases before it was stopped after five
  failures in unchanged HTTP-status and concurrency tests generated nearly 50 MB of captured seed logs. The
  moved Cancel/Complete bodies match `12273b558`; this run is not recorded as a green integration gate.
- Local E2E deliberately not run. Standalone carve, complete integration matrices, and exact-head CI remain
  owned by PR CI and the merge queue; PR/remote head equality is asserted at the pushed head below.

### Merged-tree verification (`origin/main` `abd3466e3` merged in)

- `./scripts/local-platform.ps1 build api/Concertable.slnx --configuration Release`: **Build succeeded, 0
  errors**, against a freshly packed local platform. This is the compile floor `main` is currently red on;
  the merged tree clears it. Four residual warnings are all pre-existing and outside this change: two
  `MSB3277` EF version conflicts in `Concertable.Auth.ArchitectureTests` and two `CS8632` in generated
  MSBuild temp sources for `Concertable.B2B.E2ETests.Ui`. The two warnings this branch did own — a duplicate
  `using` in `B2BHostGraphTests` and an unguarded nullable read in `OpportunityApiTests` — are fixed here.
- Unit tier, all green, 0 failures: B2B DataAccess 4, Admin 33, Application 20, Artist 12, Booking 9,
  Concert 105, Conversations 46, Dashboard.Opportunity 7, Deal 47, Tenant 178, User 1, Venue 12,
  KeyedStrategies 19; Kernel 303, Concertable.DataAccess 31, AppHost.Shared 13.
- B2B Architecture: 32/32.
- Frontend compile floor: `npm run test:boundaries` 8/8 and `npm run lint:boundaries` clean across all seven
  cruised graphs; `npm run build:web-packages`, `build:venue` and `build:artist` all succeeded.
- Provider-identifier invariant sweep re-run over the merged tree: zero hits in `api/Concertable.B2B/src`.
  The only survivors are the ones already documented above — `Concertable.B2B.E2ETests` /
  `.Ui` reading provider ids back from Payment's own database, `api/Concertable.B2B/TECH_DEBT.md`, and the
  browser's own Stripe adapter in `app/web/shared`. The retired client and error identifiers
  (`IManagerPayment*Client`, `ManagerPaymentError`, `FindHeldIntentAsync`) return zero hits across `api/`
  and `app/` except Payment's own deliberately baseline-pinned `PublishedContractFixture`, which is in no
  solution and consumes `0.1.0-alpha.0.1254` on purpose.
- `git diff --check`: clean.

### First-ever CI test matrix, and the two defects it revealed

Every previous CI run on this branch died at `build`, which gates the whole test matrix — so no unit, architecture
or integration job had ever executed on it. The merge makes `api/Concertable.slnx` compile, the matrix ran, and two
real defects surfaced. Both are this branch's own and both are fixed here (review findings IR32 and IR33).

- **Opportunity lost every genre on persistence.** The module carve regressed `OpportunityEntity.Genres` from
  `EfSet<Genre>` to a `HashSet<Genre>` behind a computed `List<Genre> PersistedGenres` shim — the exact shape
  `b610d9eeb` had already rejected as unusable under EF 10. Artist and Concert kept `EfSet<Genre>`; Opportunity alone
  did not. The HTTP response looked right because it maps the in-memory aggregate, but the JSON column never
  round-tripped, so every Opportunity re-materialised with no genres at all. Restored to `EfSet<Genre>` with
  `builder.PrimitiveCollection(o => o.Genres)`, the shadow property and its single-use query helper deleted, and the
  migration re-scaffolded (same `Genres nvarchar(max)` column; only its ordinal position moves).
- **The provider-contract inventory no longer described the tree.** Four committed entry points had been retired by
  Customer's migration on `main` and two live ones were unclassified — Customer's new `paymentSessions.CreateAsync`
  and this branch's `VerifyPaymentFailedProcessor` status read. Neither side could have caught it alone: CI scopes the
  test matrix to the changed service, so Customer's PRs never ran Payment's unit tier. Reconciled, with the orphaned
  `frontend-ticket-web-correlation` decision removed.

A third, confirmed defect is **not** fixed here and is recorded as HIGH tech debt in `api/Concertable.B2B/TECH_DEBT.md`
plus review finding IR34: the venue dashboard's revenue KPI and chart are structurally zero, because
`GetPaymentRevenueAsync` sums a table whose only writer is keyed on a `type` value nothing in the system emits. The
fix is Payment-side or belongs to the open `ConcertSalesProjection`; both are outside this PR. IR35 records a
separate Customer/Payment idempotency defect found while reviewing the merged tree.

## Reviews

- Work order: `reviews/BIG-Refactor-launch_deal-lifecycle-modules-phase2-Review.md`. Fixed-anchor review
  `fb561acee..c50469d48`, security-reviewed through `c50469d48`; incremental through `b61fc7feb`.
- IR7-IR8 are resolved; IR9-IR10 remain active. IR2/IR3/IR4 (`d1c5d252b`/`05a685317`/`090308c04`), IR5
  (`c61566685`), and the current IR6 topology checkpoint landed after `b61fc7feb`; a fresh incremental review
  over those fix commits is the remaining review gate. Keep the artifact until PR #633 merges, then delete it.
- Independent review 2026-09-05 over `39fbbc0..db5d4be8c` added IR21–IR27; all are closed on the branch,
  including the Payment metadata fix (IR23) in Payment's source here. IR18 and IR20 are closed too; IR19 is
  `wontfix` with its debt entry in `Modules/Concert/TECH_DEBT.md`. No `[ ]` finding remains. CI on this
  branch stays red only on Customer's two legacy compile errors and the jobs downstream of them; the merge is
  Tommy's admin merge.
- Incremental and security closeout began over `db5d4be8c..3f89818c7` / `39fbbc012..3f89818c7` and followed
  the concurrently added local-platform pin commit through remediation head `17ad067e1`; it added IR28–IR31.
  The branch now proves failed verification ownership through B2B's persisted venue and Payment's operation
  owner, retries provider-unavailable ownership checks, stamps the public `operationId` metadata key, and
  covers both terminal outcomes after a deferred refund. Narrative comments found in the reviewed range were
  removed. All four findings are closed; the fresh remediation incremental is clean.

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
