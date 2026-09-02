# Application, Booking, and Concert module ownership

> **Next steps live in @plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md → `## Next Steps`.**

## 1. Approved decision

The B2B lifecycle has a fixed, one-way stage order for every `DealType`:

```text
Opportunity ──→ Application ──→ Booking ──→ Concert
                    *              0..1         0..1
```

`DealType` changes the behaviour performed inside a stage. It never changes the order or makes a
stage optional in the domain model. Application, Booking, Contract, and Concert retain their
established identities and cardinalities.

The current Concert module must be decomposed so each persisted stage owns its own state and
behaviour. There is no umbrella lifecycle entity, aggregate, state enum, state machine, workflow
object, resolver, or module spanning the stages.

These are compile-time module boundaries inside the single B2B deployable. Independent deployment is
not a goal or justification. The value is enforcing the one-way ownership rule so a later stage cannot
reach backwards and mutate an earlier aggregate.

This decision was explicitly approved by Tommy on 2026-08-16 after comparing the current model with
the aggregate-collapse, Deal-owned workflow, premature two-way state split, and separate process-root
alternatives.

The remaining implementation from Phase 2 onward is one complete draft PR. The phases below are
in-branch recovery and verification checkpoints, not independently mergeable slices. This B2B-internal
refactor has no published-package, deployment, or production-data dependency requiring a partial state
to land first; PR #633 remains draft until the full definition of done is satisfied.

## 2. Domain meanings that do not change

- **Opportunity** is the venue's advertised opening. It owns one current Deal and may receive many
  Applications. It is upstream of the per-artist progression, not a stage to hide inside Application.
  Applications reference it by ID and are queried by the Application module; Opportunity does not own
  an unbounded `Applications` aggregate collection or cross-module EF navigation.
- **Deal** is the editable economic arrangement selected by `DealType`. The Deal module remains
  independent and never queries or commands Application, Booking, or Concert.
- **Application** is one artist's submission to an Opportunity. Acceptance is its successful terminal
  decision; later financial or operational outcomes do not rewrite that fact.
- **Booking** is the accepted commercial relationship created from one Application. Its Standard and
  Deferred variants retain their payment-timing meaning.
- **Contract** is the immutable signed terms snapshot formed with the Booking at acceptance.
- **Concert** is the realised operational event created from a Booking after financial confirmation.
  Drafting, posting, editing, cancellation, completion, door revenue, and event facts belong here.

```text
Opportunity 1 ── 1 Deal
Opportunity 1 ── * Application
Application 1 ── 0..1 Booking
Booking     1 ── 0..1 Contract
Booking     1 ── 0..1 Concert
```

Invoice, settlement-attempt, refund-attempt, and ticket-transaction records keep their genuine
financial identities. Their final module placement must follow the operation they make durable; they
must not be folded into a replacement end-to-end lifecycle aggregate.

## 3. Target module boundaries

### Deal

Owns Deal entities, values, validation, rendering/mapping, and `DealType`. It exposes immutable deal
facts through `Deal.Contracts`. Owning the selection key does not make Deal the owner of downstream
behaviour selected by that key.

### Opportunity

Owns Opportunity identity, schedule, availability, posting, and the `DealId` association. It reads
Deal through `Deal.Contracts`/`IDealModule`. It does not own an Application, Booking, or Concert state
machine.

### Application

Owns applying, apply-time checkout, pre-accept acceptance-checkout initiation, terms-fingerprint and
artist-signature capture, rejection, withdrawal, and acceptance. It may retain immutable pre-accept
payment evidence that can arrive before a Booking exists; that evidence is not Application lifecycle
state.

Application reaches a terminal state when it is accepted, rejected, or withdrawn:

```text
Applied ──Accept──→ Accepted
Applied ──Reject──→ Rejected
Applied ──Withdraw→ Withdrawn
```

`Application.Contracts` owns the immutable accepted-application handoff consumed by Booking. That
handoff is the provenance Booking requires: a Booking cannot be created from an arbitrary application
identifier or without the accepted Application facts. Pre-accept payment evidence crosses with the
handoff as case-specific immutable data, not as an enum or boolean accompanied by nullable metadata.

Acceptance raises an `ApplicationAcceptedDomainEvent` synchronously before commit. Booking consumes
the immutable handoff and forms Booking and Contract inside the same ambient B2B transaction. This is
a pre-commit domain handoff, not an asynchronous outbox integration handoff: the outbox makes outbound
messages durable but does not make later asynchronous Booking creation atomic with Application.

### Booking

Owns Booking and Contract creation, acceptance-triggered payment processing after Booking creation,
financial confirmation, payment failure/retry, and cancellation/refund before a Concert exists.
Acceptance atomically forms the Booking and Contract; financial confirmation hands authority to
Concert.

Booking creation requires the accepted-application contract. Financial confirmation requires an
explicit successful financial-operation fact correlated to that accepted Application and the expected
operation/provider transaction; an identifier-only `ConfirmAsync(bookingId)` command is invalid. Once
the Booking exists, it owns later financial outcomes and does not reload or accept a live Application
aggregate to confirm itself.

The exact enum names are fixed during the implementation inventory, but the state meaning is:

```text
AwaitingConfirmation
ConfirmationFailed
Confirmed
CancellationPending
CancellationFailed
Cancelled
```

`Confirmed` is a terminal historical Booking fact. A later Concert cancellation does not make the
accepted Booking or signed Contract cease to have existed.

### Concert

Owns the Concert entity and all post-creation operational state: draft/posting, cancellation,
completion, and any recovery state whose success is required to complete those operations. It does
not inspect `Booking.Application.State` or ask Booking/Deal how to interpret Concert state.

Concert creation consumes the immutable `ConfirmedBooking` handoff from `Booking.Contracts`; it never
loads a live Booking or Application aggregate. Creation is uniform across `DealType`: every case uses
the same projection lookup, genre intersection, aggregate creation, persistence, notification, and
email path. The immutable terms cases supply different data to `ConcertEntity.CreateDraft`; they do
not select different creation behaviour. `IConcertService.CreateAsync(ConfirmedBooking)` therefore
owns creation.

Cancellation and completion are coordinated by the module-local `IConcertWorkflow`. Each method retains
its own validation, persistence, transaction, IO, and typed failure contract. Deal-varying work remains
behind operation-specific `ICancel` and `IComplete` implementations selected through the homogeneous
`IDealStrategyFactory<T>`.
The pre-commit `BookingConfirmedDomainEventHandler` remains a thin adapter to the service. Creation has
no expected caller-actionable failure after a confirmed Booking: Application already validated genre
eligibility, while a missing or mismatched local projection is an invariant violation. Cancel and
Complete keep their operation-owned typed Results for expected failures.

Settlement and invoice records must be assessed by identity during the carve. They may remain
Concert-owned children where they make a Concert completion operation durable, or move to a
separately justified financial module. They cannot revive a shared Application-to-Concert state.

## 4. State ownership

There is no authoritative state of a hidden end-to-end "thing." Each persisted identity records facts
about itself and stops transitioning when authority moves forward:

```text
Application 42: Accepted
Booking 81:     Confirmed
Concert 103:    Complete
```

The API may derive one current journey view by preferring the latest existing stage:

```text
Concert exists      → Concert status and actions
else Booking exists → Booking status and actions
else                 → Application status and actions
```

That is a read model only. It has no command surface, transition method, repository, or source-of-truth
row. After the B2B runtime moves to .NET 11, native C# unions will represent justified closed internal
values, beginning with the combined read shape
`ApplicationStage | BookingStage | ConcertStage` and module-local state, trigger, or operation-outcome
shapes whose cases carry genuinely different data. Unions do not replace state ownership or dependency
resolution: each module maps its persistence discriminator explicitly and retains transition authority.

## 5. State machines and contextual names

Application, Booking, and Concert each own an explicit immutable state-machine definition. The
shared mechanism is an algorithm, not a shared lifecycle: it knows how to look up one closed
`(state, trigger) -> next state` edge and nothing about B2B stages, persistence, events, operations,
or `DealType`.

`Concertable.Kernel` owns the reusable surface:

```csharp
public interface IStateMachine<TState, TTrigger>
    where TState : notnull
    where TTrigger : notnull
{
    Result<TState, TransitionError<TState, TTrigger>> Transition(
        TState current,
        TTrigger trigger);
}

public sealed record TransitionError<TState, TTrigger>(
    TState Current,
    TTrigger Trigger);
```

Its immutable implementation copies the supplied transitions into one
`FrozenDictionary<(TState, TTrigger), TState>` during construction, rejects duplicate edges, and has
no registration or mutation API after construction. It carries no current entity state. It has no
entry/exit actions, callbacks, persistence, event publication, dependency injection, service
resolution, retries, or asynchronous behaviour. `Transition` always returns a real Result value: an
accepted edge carries the real next `TState`; a missing edge carries the common parameterized
`TransitionError`, never `default(TState)`, an `out` value, or an exception. Duplicate edges and other
invalid machine construction remain programmer/composition exceptions. A module may hold one static,
thread-safe instance; the persisted current state always remains on each entity.

Kernel references Reunion deliberately because Result is part of this pure domain API. Every consuming
service also references Reunion directly at the compatible version; no consumer relies on a transitive
carrier reference. The Kernel producer checkpoint must therefore reconcile the existing package rule,
package metadata, and architecture guard that currently forbid Kernel from exposing Reunion. That is a
deliberate dependency-policy correction, not a reason to move Concertable's state-machine ownership into
the Reunion repository.

Each module owns its state, trigger, immutable edge declaration, and aggregate operations. The common
`TransitionError<TState, TTrigger>` is the complete failure contract of the generic machine and is not
an inheritance base. The types may use the same short names because their module namespace is the
context:

```text
Application.Domain.Lifecycle.State
Application.Domain.Lifecycle.Trigger
Application.Domain.Lifecycle.StateMachine

Booking.Domain.Lifecycle.State
Booking.Domain.Lifecycle.Trigger
Booking.Domain.Lifecycle.StateMachine

Concert.Domain.Lifecycle.State
Concert.Domain.Lifecycle.Trigger
Concert.Domain.Lifecycle.StateMachine
```

These independent definitions use `IStateMachine<TState, TTrigger>` but do not inherit behaviour or
share a configured machine, transition table, state, trigger, error, or selector. A future .NET 11
native union may improve a module's closed state, trigger, or outcome vocabulary without changing the
ownership or the shared lookup boundary.

The aggregate owns enforcement. Callers invoke intent-named operations such as `Accept`, `Confirm`,
`BeginCancellation`, or `CompleteSettlement`; they do not invoke a public generic `Transition` method.
Each aggregate funnels those operations through one private transition helper, observes the returned
Result, mutates its own state only from the success value, then updates operation-specific data and
raises domain events. A rejected Result leaves state, auxiliary facts, and events unchanged.
Application and Infrastructure may choose an operation and persist its outcome, but may never
calculate a target state and hand it to the entity. The entity never resolves the machine from DI,
saves itself, publishes to a bus, or calls another module.

An aggregate or operation returns `TransitionError<TState, TTrigger>` directly when that is its complete
expected failure contract. An operation that has additional failures owns a closed error union with a
nested transition-rejection case and forwards the common transition error's meaning. It does not widen
to `IError`, inherit from a shared error base, or introduce a wrapper when the transition error alone is
sufficient. Shared `NotFound` bases, shared error catalogs, `IState` markers, and state inheritance are
rejected: operation-owned errors and unconstrained module-owned state/trigger types remain exhaustive.

Operation-specific prerequisites remain outside the generic lookup. Identity/correlation checks,
required payment evidence, provider references, terms, timestamps, financial metadata, idempotency,
and event payload construction stay in the owning aggregate operation. The machine decides only
whether the requested state edge exists.

Opportunity's `Open -> Filled` claim is decided: it is not a synchronous conditional write participating
in the accept transaction, but an ordinary aggregate-owned transition (`MarkFilled`), triggered
asynchronously by the `ApplicationAcceptedEvent` integration event after the winning accept commits,
mirroring the existing `Reopen` reaction to Booking/Concert cancellation. The concurrency invariant
("only one Accepted Application per Opportunity") is enforced entirely inside the Application module by
a unique filtered index, not by an Opportunity-side conditional write. Opportunity may use the shared
state-machine primitive for `Filled`/`Withdrawn`/`Reopen` like any other aggregate-owned transition.

The old per-`DealType` Concert `LifecycleStateMachine` is not restored. It combined stage ownership
and selected four configured machines by `DealType`; the new design has one configured machine per
owning aggregate/module because `DealType` changes operation behaviour, not legal stage order.

The additive Kernel API is delivered as a small Concertable shared-package producer checkpoint first,
with focused tests and a published platform version compatible with the consuming Reunion baseline.
PR #633 may validate locally with `UseLocalCore=true` or an exact producer artifact, but committed
consumer verification waits for the published Kernel package and reconciled platform pin. After that
release is available, Application, Booking, and Concert adopt it on this same module-refactor PR. This
is the genuine external-artifact exception to the otherwise single-PR refactor; it is not authority to
split the B2B ownership work.

Extracting the proven primitive into an independent state-machine NuGet package remains a possible
future decision after both B2B and Payment exercise the API. It is not part of this plan and creates no
work in the Reunion repository.

Payment PR #707's intent/refund allowed-transition maps are a downstream candidate for the same
primitive. Payment keeps its provider observation validation, terminal-state protection, duplicate
classification, context selection, freshness rules, and transition result types. Reuse is limited to
the immutable allowed-edge lookup and creates no B2B-to-Payment dependency.

## 6. Module workflows and Deal-type behaviour

Application, Booking, and Concert each own one executable, module-local workflow. These workflows group
the named lifecycle operations for one aggregate stage; none spans a module boundary, stores aggregate
state, or acts as a dependency-holder for an end-to-end process.

Each module owns only the Deal-selected contracts that operate on its aggregates. Internal types use
the operation name without `Step` or redundant aggregate prefixes:

| Module | Deal-selected contracts |
|---|---|
| Application | `IApplyStandard`, `IApplyPrepaid`, `IAccept`, `IAcceptPaid` |
| Booking | `IConfirm`, `ICancel` |
| Concert | `ICancel`, `IComplete` |

Deal-varying methods are classified by invocation shape. A genuine same-interface family is selected
through `IDealStrategyFactory<TStrategy>`. A method whose implementations require different parameters,
results, or capabilities gets one interface per honest method header and one operation-owned union over
those interfaces. Each header may have multiple keyed DI implementations. `IDealUnionFactory<TUnion>`
selects the configured implementation and returns the operation union; its consumer matches the method
header, never the four Deal cases:

```csharp
return acceptFactory.Create(deal.DealType) switch
{
    Accept.Standard(var accept) =>
        accept.Accept(application, opportunity, artist, venue, deal, signature, operationId),
    Accept.Paid when string.IsNullOrWhiteSpace(paymentMethodId) =>
        new AcceptApplicationError.PaymentMethodRequired(),
    Accept.Paid(var accept) =>
        accept.Accept(application, opportunity, artist, venue, deal, signature, operationId, paymentMethodId),
    _ =>
        throw new UnreachableException()
};
```

`IAcceptPaid.Accept` requires the payment method supplied at acceptance. `IAccept.Accept` needs no new
acceptance input: `StandardAccept` handles FlatFee, while `PrepaidAccept` validates `PrepaidApplication`
and reads its stored payment method internally. The guarded Paid arm returns the established typed
validation failure. The final net10 arm represents invalid factory composition and is therefore an
unreachable invariant failure, not an expected application outcome.

The Deal dispatch foundation is terminal on `main`. It delivered the Deal module's validated invariant
net10 factory for the honest `IDealMapper` and `IDealUpdater` families; the production generator and
analyzer prototype was deliberately removed. PR #633 must consume that proven pattern without claiming
generated machinery exists or reaching into Deal's internal factory implementation.

Application `IDealTerms` remains a genuine same-interface family. Heterogeneous lifecycle methods instead
use operation-owned Dunet unions on net10 and honest method-header interfaces. `Accept` has
`IAccept` and `IAcceptPaid`; FlatFee and VenueHire map to `IAccept`, while DoorSplit and Versus map to
`IAcceptPaid`. `Apply` has standard and prepaid method headers; FlatFee, DoorSplit, and Versus map to
Standard while VenueHire maps to Prepaid. Complete remains a homogeneous strategy family
unless its invocation contract later proves otherwise. Multiple Deal cases may deliberately select the
same method header, while every Deal case maps to exactly one header in each operation union.

`Concertable.B2B.KeyedStrategies` owns the key-generic `KeyedUnionBuilder<TKey, TUnion>` beside
`KeyedStrategyBuilder<TKey>`. It records each declared union case, the one case selected by every key,
the keyed implementation, the conversion into `TUnion`, and the lifetime. Build rejects incomplete or
duplicate key coverage, undeclared or uninhabited cases, conflicting lifetimes, and an implementation that
implements multiple cases in the same union before mutating the service collection.

Shared B2B Infrastructure owns `DealStrategyBuilder` and `DealUnionBuilder<TUnion>`, which compose those
generic keyed builders with `DealType`, `IDealStrategy`, and the matching Deal factory registration. The
Deal-specific builders derive exhaustive coverage from `Enum.GetValues<DealType>()`; module composition
supplies only the unavoidable DealType-to-implementation assignments and never repeats `RequireAll` or
constructs a generic keyed builder directly.

Deal Contracts owns `IDealUnionFactory<TUnion>` beside `IDealStrategyFactory<TStrategy>`. Their open-generic
runtime implementations live together in shared B2B Infrastructure so Application, Booking, and Concert do
not reference Deal Infrastructure. On net10 the union factory reads the validated `DealType`-to-case
catalog, performs one exact keyed lookup, and applies the configured Dunet conversion. It never probes a
list of candidate services. `IKeyedServiceProvider` must not escape either factory into a workflow or other
consumer. Method-header interfaces remain in the owning module beside the consumer; concrete implementations
and keyed registrations remain in that module's Infrastructure.

The .NET 11 follow-up preserves the method interfaces, implementations, `IDealUnionFactory<TUnion>`, mapping,
and call-site semantics, but replaces the adapter declaration with a direct native type union such as
`union Accept(IAccept, IAcceptPaid)`. The compiler then enforces a direct method-header match with no Dunet
case records, `.Accept` wrappers, or default arm. The upgrade must first prove how the
generic builder constructs `TUnion` now that the current proposal has no generic union-construction
interface; an explicit native conversion delegate is the fallback and does not restore wrapper records.
Neither design contains a resolver, global workflow bundle, candidate-service probing, or four-Deal workflow
switch. No concrete implementation may implement more than one header in the same union because that would
make type-pattern selection ambiguous even though the language permits overlapping cases.

`IDealStrategyFactory<TStrategy>` remains separate and applies only to genuine same-interface families such
as `IDealTerms`. `IDealUnionFactory<TUnion>` applies only where the method headers do not share one
substitutable invocation. Both are factories because they return selected components; neither consumes the
component to produce the final domain answer.

Cancel and Complete are separate methods on `IConcertWorkflow`; each keeps its own validation,
persistence, transaction, IO, and typed failure contract. Uniform creation remains on `IConcertService`
because it performs no Deal selection. Expected failures use typed Results; no design may convert them
into explicit exceptions merely to cross an internal boundary.

Each module declares exact `DealType` coverage vertically at its own composition root. Repeating the
closed key in three independent declarations is correct ownership, not duplication. Adding a new
`DealType` must fail composition/tests in every module whose behaviour requires a deliberate choice.

HATEOAS and dashboard capability checks consume module-local capability metadata or the combined read
projection. They do not instantiate a workflow or reflect over an umbrella capability interface.

## 7. Dependency and communication rules

Runtime code may reference another module only through its Contracts project. The intended dependency
graph is acyclic:

```text
Application.Runtime ──→ Opportunity.Contracts ──→ Deal.Contracts
Booking.Runtime     ──→ Application.Contracts
Concert.Runtime     ──→ Booking.Contracts

Application.Runtime ──→ Deal.Contracts
Booking.Runtime     ──→ Deal.Contracts
Concert.Runtime     ──→ Deal.Contracts
```

The runtime fact flow is always forward:

```text
ApplicationAccepted
        ↓
Booking created / Contract frozen / financial confirmation
        ↓
BookingConfirmed
        ↓
Concert created
```

Rules:

- A module creates only the layers it actually uses. Empty layer projects and no-op composition roots
  are migration scaffolding only and must be populated or removed before delivery.
- Owning a DTO does not make it a Contracts type. Internal service inputs/results stay in the owning
  Application layer; only a shape deliberately consumed across a module boundary belongs in Contracts.
- Purpose-built query shapes mapped by an application service are projections, snapshots, or details.
  `Context` is reserved for ambient request, tenant, transport, or persistence context.
- Deal never references Application, Booking, or Concert runtime/contracts to interpret their state.
- Application never queries or commands Booking or Concert state.
- Booking never queries or commands Concert state.
- Concert never traverses to `Booking.Application.State` or calls upstream services to finish an
  operation.
- A published downstream fact may update an upstream-facing read model or notification, but the
  downstream transition never waits for a reply. Business authority never bounces backwards.
- Opportunity reopening after cancellation must become a non-blocking fact/projection reaction or be
  derived from current stage facts; Concert cancellation must not synchronously command Opportunity.
- A composition/query layer may consume all three Contracts surfaces. It owns no lifecycle state or
  commands.

## 8. Transaction, ordering, and recovery invariants

### Accept

Preserve the current invariant that accepting an Application forms its Booking and Contract atomically.
Within B2B, the Application and Booking module DbContexts join one ambient SQL transaction.
`ApplicationAcceptedDomainEvent` is dispatched synchronously pre-commit so Booking and Contract
formation either commits with Application acceptance or all participating writes roll back. That
coordinator is an application-boundary operation with no persisted identity or state; it is not an
umbrella aggregate. Opportunity's DbContext never joins this transaction: Opportunity is upstream, and
the one-way lifecycle forbids a downstream commit from synchronously enlisting an upstream aggregate,
even to claim it.

The transaction must stage all resulting outbox work before commit. A failure creating Booking or
Contract leaves Application `Applied`.

The "only one Accepted Application per Opportunity" invariant is enforced entirely inside the
Application module, with no synchronous cross-module call. A unique filtered index on
`Application(OpportunityId) WHERE State = Accepted` makes two concurrent accepts for the same
Opportunity collide at the database; the loser's `SaveChangesAsync` raises a duplicate-key
`DbUpdateException`, caught and mapped to a typed `AcceptApplicationError.AlreadyAccepted` conflict — an
expected typed conflict Result, not an exception surfaced to the caller. Opportunity's own `Filled`
transition is a downstream reaction, not a claim participating in the accept transaction: the winning
accept publishes `ApplicationAcceptedEvent` (a durable, outbox-staged integration event) after its own
commit, and Opportunity's integration-event handler marks itself `Filled` in its own transaction —
mirroring the already-correct Reopen reaction to Booking/Concert cancellation. Concurrent Applications
for one Opportunity therefore still produce exactly one Accepted Application, Booking, and Contract,
with every sibling Rejected by the winning accept's own `RejectAllExceptAsync`; no `AcceptedPendingBooking`
state or reconciliation process is introduced, and reaching that outcome never requires a backward
synchronous call.

### Payment webhook before Accept

Preserve the durable two-signal join:

- a verification callback may arrive while Application is still `Applied` and before Booking exists;
- Application records that immutable pre-accept payment evidence idempotently as distinct success or
  failure data with every field required for that case;
- Accept creates Booking/Contract from the accepted-application contract and consumes the recorded
  evidence;
- whichever signal arrives second performs the one guarded handoff;
- once Booking exists, later acceptance-payment outcomes and retries are Booking-owned and remain
  correlated to the accepted Application and payment operation;
- duplicate/late callbacks are idempotent and cannot create a second Booking, Contract, or Concert.

Do not solve ordering with retries-as-waiting, cross-module polling, or a global process row.
Do not model the callback as one outcome value plus nullable failure code/message fields. Success and
failure are separate facts with case-specific required data. Name those facts from the concrete payment
operation vocabulary already used by the processors; `ApplicationPaymentVerified` is not an approved
placeholder name.

### Booking confirmation

Financial confirmation and Concert draft creation must converge exactly once. Prefer the same ambient
cross-module transaction while both modules remain inside B2B; otherwise use an outbox/inbox handoff
with deterministic identity and an explicit pending projection. The implementation must prove there
is no lost callback, duplicate Concert, or permanently confirmed Booking without a recoverable Concert
creation path.

The confirmation service/aggregate boundary consumes the explicit successful financial-operation fact,
validates its Application and operation correlation against the Booking created from the accepted-
application handoff, and only then transitions. A failure travels through a separate failure fact and
cannot be supplied to the confirmation method.

### Cancellation and settlement

- Application handles only pre-accept rejection/withdrawal.
- Booking handles cancellation/refund after acceptance and before Concert creation.
- Concert handles cancellation after Concert creation.
- Refund, completion, and settlement operation IDs, failures, retries, and compensations live with the
  aggregate whose command is awaiting that outcome.
- A late capture after cancellation is compensated idempotently without reopening an earlier state.
- FlatFee/VenueHire escrow release and DoorSplit/Versus deferred settlement retain their current money,
  payer/payee, retry, invoice, and completion invariants.

## 9. Implementation phases and single-PR delivery

Phase 1's characterization PR is merged history. Phases 2-6 remain together on draft PR #633 from a
current `origin/main` base. Checkpoint commits and exact-head CI keep the large rewrite recoverable and
green; none of those checkpoints is a merge candidate until all later phases and the definition of done
are complete. Draft PR #614 and its DealTerms implementation are rejected input, not an implementation
base.

Each continuation executes exactly one bounded checklist slice. Before implementation, the progress
ledger must name that slice, its allowed subsystem/path scope, and one focused exit gate. Reaching the
gate ends the continuation: update the plan and ledger, commit and push the recovery checkpoint when
green, then resume the next slice in a fresh context. The instruction to continue across implementable
phases means successive checkpointed continuations, never loading Phases 3-6 into one context.

Do not mechanically preserve a legacy callback merely because it previously existed. Every retained
event handler must produce an owned state change or output, or enforce a specifically documented
invariant that requires consuming the event. If it does none of those, remove the subscription. When
that purpose is uncertain, stop the slice and record the question before editing adjacent lifecycle
code.

### Phase 1 — restore and characterize the real baseline

- [x] Retire the rejected PR/branch through the repository's safe worktree process; do not merge or
  repair its DealTerms code into the new implementation.
- [x] Pin observable acceptance, payment, cancellation, settlement, Contract, Invoice, and
  Concert-creation outcomes at module or API boundaries before moving ownership. Do not add tests for
  the shared `LifecycleState`, its transition table, coordinator filenames, source tokens, or other
  implementation structure scheduled for deletion.
- [x] Record the current coordinators, processors, callbacks, worker, and API/HATEOAS consumers as
  migration inventory in the progress ledger rather than freezing those owners as test expectations.

Gate: the new branch is behaviourally identical to `origin/main`, Deal vocabulary is intact, durable
behaviour is executable as tests, and no new test depends on the legacy shared lifecycle abstraction.

### Phase 2 — establish the in-branch cutover seam

- [x] Establish the Opportunity, Application, and Booking project/Contracts seam needed for the
  migration; runtime layers and composition roots remain incomplete until the ownership moves below.
- [x] Replace cross-stage EF navigation in services, specifications, workers, and mappers with owned
  IDs, module contracts, or query projections.
- [x] Define forward handoff records carrying immutable accepted/confirmed facts and deterministic IDs.
- [x] Preserve current API routes and wire vocabulary during the internal cutover.
- [x] Add architecture rules against the real module assemblies as they are scaffolded, failing direct
  runtime/entity references while allowing Contracts dependencies.

Checkpoint gate: the dependency graph is acyclic and Contracts-only while behaviour and public
responses remain unchanged. Empty runtime layers, no-op `Add*Module` methods, and the legacy shared
`LifecycleState` are explicitly non-deliverable transient state on the draft branch.

#### Integration-test ownership topology correction

The compile-recovery work exposed a test-boundary defect that must be corrected before further
file-by-file recovery. `Concertable.B2B.Concert.IntegrationTests` currently acts as a service-wide
test bucket and its fixture temporarily resolves Application, Booking, and Concert persistence. That
shape contradicts the module carve and is not an acceptable delivery state.

The audit classifies every current test/helper by the operation and assertion surface it actually
owns:

| Current source | Target ownership and purpose |
|---|---|
| `ApplicationApiTests` | Application API and eligibility behaviour; Application integration tests. |
| `ApplicationWithdrawRejectApiTests` | Application terminal decisions and notifications; Application integration tests. |
| `ApplicationFinancialOperationApiTests` | Booking-owned acceptance financial-operation state exposed by the compatibility route; Booking integration tests. |
| `ApplicationCancelApiTests` | Split by command owner: pending withdrawal/guards to Application, pre-Concert cancellation/refund to Booking, post-creation cancellation to Concert. Opportunity reopening is verified through its HTTP boundary from the initiating module. |
| `ApplicationDoorSplitApiTests`, `ApplicationFlatFeeApiTests`, `ApplicationVenueHireApiTests`, `ApplicationVersusApiTests` | Split Application-only checkout/apply/accept validation from the complete payment/Accept/Booking/Concert journey. Application cases move to Application integration tests; complete journeys move to the B2B journey integration suite. |
| `ContractApiTests` | Split Application-owned consent/signature/fingerprint cases from Booking-owned immutable Contract formation, metadata, PDF, and snapshot cases. |
| `BookingConfirmationEmailTests` | Concert-creation notification/outbox behaviour stays with Concert; the pure renderer case belongs in Concert unit tests. |
| `ConcertApiTests`, `ConcertCancelApiTests`, `ConcertDoorRevenueApiTests`, `ConcertDoorSplitApiTests`, `ConcertFlatFeeApiTests`, `ConcertInvoiceApiTests`, `ConcertPayoutComplianceGateApiTests`, `ConcertSelfBillingGateApiTests`, `ConcertVenueHireApiTests`, `ConcertVersusApiTests`, `OutboxVerificationTests`, `SelfBillingAgreementApiTests`, `SelfBillingAgreementGateApiTests` | Genuinely Concert-owned HTTP, completion, cancellation, settlement, invoice, self-billing, notification, and outbox behaviour; remain in Concert integration tests. |
| `ConcertRequestBuilders` | Concert HTTP request construction; remains in Concert integration tests. |
| `ConcertWorkflowExtensions` | Concert-only setup/operation helper, but must be replaced by fixture/API helpers that do not expose `IServiceProvider` or locate repositories. |
| `ArtistDashboardCountsTests` | Artist dashboard composition; move to the existing Artist integration project and assert through the Artist HTTP API. |
| `DealApiTests` | Deal HTTP behaviour; Deal integration tests. |
| `OpportunityApiTests`, `OpportunityRequestBuilders` | Opportunity HTTP behaviour and requests; Opportunity integration tests. |
| `EscrowPaymentProcessorTests` | Stale Concert processor vocabulary for Booking-owned acceptance financial outcomes; rewrite against the Booking processor in Booking integration tests. |
| `TenantScopingTests` | Split Application visibility/snapshot, Booking persistence stance, Concert public/party reads, and the complete cross-stage tenant-snapshot journey by those owners; the journey case belongs in the B2B process suite. |
| `AssemblyInfo`, `GlobalUsings`, `IntegrationCollection`, project metadata, `AGENTS.md`, `CLAUDE.md` | Recreate per owning integration project; no shared Concert namespace or fixture survives in another module's suite. |

The target topology is:

- Opportunity, Application, Booking, Deal, and Concert each own a `*.IntegrationTests` project. Each
  project's fixture derives from the shared host harness and may expose only that module's real
  production `DbContext` or read stance.
- `ConcertApiFixture` resolves only Concert persistence. Application and Booking fixtures resolve
  only their own contexts and never expose one another's or Concert's context.
- `Concertable.B2B.Journey.IntegrationTests` owns only complete multi-module journeys. It has no direct
  Domain or Infrastructure project reference and observes stages through HTTP or deliberate Contracts
  surfaces.
- The shared `ApiFixture` remains host-neutral infrastructure. It does not become a generic context,
  repository, or service locator for module assertions.
- Architecture/convention coverage reads every module integration-test project reference and fails a
  direct reference to another module's Domain or Infrastructure assembly. A module may friend only its
  own integration-test assembly.
- The temporary integration-fixture TECH_DEBT entry is deleted only after the topology, fixtures,
  references, namespaces, and behavioural coverage are all corrected.

Implementation checkpoints, in order:

1. scaffold the missing module/process projects, local fixtures, collections, metadata, solution
   entries, friend declarations, and the mechanical project-reference guard;
2. move the single-owner suites and helpers, then split mixed Application/Booking/Concert tests by
   operation ownership;
3. re-express complete journeys and cross-module assertions through public boundaries, remove the
   temporary multi-context fixture surface and stale Concert-owned namespaces/references, and delete
   the resolved debt entry;
4. validate each affected project, the architecture guard, the remaining Concert suite, B2B build
   closure, plan graph, and diff hygiene before resuming lifecycle implementation.

### Phase 3 — split Application and Booking ownership atomically

- [x] Move Application persistence, services, repository, API mapping, actions, and local lifecycle
  state to Application.
- [x] Move Booking, Contract, acceptance payment/recovery, and pre-Concert cancellation to Booking.
- [x] Replace the combined `LifecycleState` with independent Application and Booking state.
- [ ] Preserve the Accept transaction, immutable Contract snapshot, operation IDs, early-verification
  join, late-callback compensation, retry, and idempotency invariants.
- [ ] Enforce `only one Accepted Application per Opportunity` with a unique filtered index inside
  Application, react to it in Opportunity as an async `Filled` transition, and prove concurrent
  acceptance yields exactly one Accepted Application, Booking, and Contract while rejecting every
  sibling.
- [x] Require accepted-application provenance for Booking creation and explicit correlated financial
  success/failure facts for later outcomes; remove identifier-only confirmation and nullable outcome
  payloads.
- [x] Re-home Standard/Prepaid Application and Standard/Deferred Booking without nullable flattening.

Gate: Application is terminal after its decision, Booking owns every post-accept/pre-Concert
transition, and all accept/payment arrival orders pass focused integration coverage.

### Phase 4 — give Concert independent operational ownership

- [x] Before changing the Concert application boundary, independently research the module workflow and
  uniform `CreateAsync(ConfirmedBooking)` placement against the
  final dependency graph, keyed-strategy conventions, typed-Result semantics, and comparable repository
  code. Record the decision in this plan and ledger before implementation.
- [x] Create Concert only from a financially confirmed Booking handoff.
- [x] Move draft/posting, post-creation cancellation, completion, settlement recovery, and relevant
  financial operation facts onto Concert or justified Concert-owned children.
- [x] Remove every Concert query or command that interprets `Application.State` or loads upstream
  entities to determine a Concert transition.
- [x] Decide Invoice/settlement/ticket-transaction placement from their identity and transaction
  evidence; create a separate financial module only if it owns an independent lifecycle.

Gate: Concert can validate and complete every operation from its own state plus immutable handoff facts.

### Phase 5 — replace the cross-stage workflow with module workflows

- [x] Delete the cross-stage workflow dependency-holders, the workflow factory,
  cross-stage builder, state-machine registry, and reflection capability registry.
- [x] Deliver the additive Kernel `IStateMachine<TState, TTrigger>`,
  `TransitionError<TState, TTrigger>`, and immutable frozen-table implementation through its shared
  package producer checkpoint; reconcile Kernel's direct Reunion dependency rule, reject duplicate
  edges, and prove the input collection cannot mutate the constructed machine.
- [x] After package publication, add independent Application, Booking, and Concert state, trigger, and
  configured-machine definitions. Preserve the approved legal edges and return the common transition
  error for every other state/trigger pair.
- [x] Route every aggregate lifecycle operation through one private transition helper. Keep semantic
  public/internal methods, operation-specific validation, state mutation, auxiliary data mutation, and
  domain-event raising on the aggregate; remove direct target-state assignment from callers.
- [x] Audit Opportunity's Open/Filled/reopen flow. `Filled` is an ordinary aggregate-owned transition
  reached asynchronously via `ApplicationAcceptedEvent`, not a conditional persistence claim; adopt the
  shared state-machine primitive for `Filled`/`Withdrawn`/`Reopen` like any other module.
- [ ] Add local `State`, `Trigger`, `StateMachine`, module workflows, and Deal-selected contracts only
  where each module needs them.
- [ ] Register exact per-`DealType` strategy or union coverage independently in Application, Booking,
  and Concert.
- [ ] Update module guidance for lifecycle ownership without ratifying the provisional selector
  mechanism; the separate dispatch investigation owns any general `api/agents/CODE_PATTERNS.md`
  replacement.
- [x] After the compile-recovery frontier is green, apply the Deal-specific strategy builder and factory
  to Application `IDealTerms`. Application owns its implementations and keyed assignments; Deal Contracts
  owns the marker and factory contract, while shared B2B Infrastructure owns their implementations.
- [x] Add `KeyedUnionBuilder<TKey, TUnion>` beside the shared keyed-strategy builder, compose it through
  `DealUnionBuilder<TUnion>`, and add the Deal-owned `IDealUnionFactory<TUnion>` contract plus its shared B2B
  Infrastructure implementation. Validate exactly one case per DealType, allow many DealTypes to share one
  case, and resolve the selected case with one keyed lookup.
- [x] Move the genuinely heterogeneous Application Apply and Accept operations onto operation-owned net10
  Dunet unions and honest method-header interfaces. Keep the mapping in Application composition, match by
  method-header wrapper in the consumer, and leave homogeneous Complete on `IDealStrategyFactory<TStrategy>`.
- [ ] Do not convert honest same-interface families to operation unions or erase heterogeneous
  invocations behind a manufactured common interface.

State-machine verification includes focused package tests for successful Result values, typed rejected
Results, duplicate-edge rejection, immutable snapshotting, and concurrent reads; exhaustive module
tests that enumerate every state/trigger pair; aggregate tests proving failed transitions leave state,
auxiliary facts, and domain events unchanged; and an architecture/convention guard that fails
lifecycle-state assignment outside the owning aggregate's construction/hydration and private transition
path. Payment PR #707, if still open when the package publishes, replaces only its allowed-edge tables
and reruns its complete provider-transition matrix.

Delivered (2026-08-25): each module owns `Domain/Lifecycle/{State,Trigger,StateMachine}.cs`. Application
`State` = {Applied, Accepted, Rejected, Withdrawn}, `Trigger` = {Accept, Reject, Withdraw}, three edges.
Booking `State` = {AwaitingConfirmation, ConfirmationFailed, Confirmed, CancellationPending,
CancellationFailed, Cancelled}, `Trigger` = {Confirm, RecordConfirmationFailure, BeginCancellation,
RecordCancellationFailure, Cancel}, ten edges. Concert `State` = {Draft, Posted, CancellationPending,
CancellationFailed, AwaitingSettlement, SettlementFailed, Complete, Cancelled}, `Trigger` = {Post,
BeginCancellation, RecordCancellationFailure, Cancel, BeginSettlement, RecordSettlementFailure,
CompleteSettlement}, sixteen edges. Each `internal sealed class StateMachine : IStateMachine<State,
Trigger>` delegates to the Kernel `StateMachine<State, Trigger>`; each aggregate funnels mutation through a
private `Transition(Trigger)` helper (`State = next` only on success). Operation errors carry
`InvalidTransition(TransitionError<State, Trigger>)`. `StateMachineTests`, `*EntityLifecycleTests`, and
`LifecycleStateOwnershipTests` cover the exhaustive-edge, no-mutation, and assignment-guard requirements.

Gate: each command invokes one module-owned operation; every lifecycle mutation is accepted by that
module's immutable machine and applied by its aggregate; no service calculates a target state, resolves
a machine or another module's operations, or requests a whole workflow.

### Phase 6 — projections, compatibility, and delivery

- [ ] Build the read-only combined journey projection used by APIs, dashboards, notifications, and
  HATEOAS without granting it command authority.
- [ ] Preserve public Application/Booking/Concert vocabulary and migrate frontend consumers without
  exposing internal transition machinery.
- [ ] Re-scaffold initial migrations after the final model move.
- [ ] Update B2B architecture, Deal/Concert guidance, module AGENTS files, diagrams, and the .NET 11
  native-union plan to the implemented boundary.
- [ ] Run focused module/unit/integration verification locally; draft-PR CI owns the full solution,
  carve, and integration matrix. Select the final merge-queue E2E tier under repository policy.
- [ ] Review the complete implementation diff and follow PR, package publication, and platform sync to
  terminal green before closing this plan.

## 10. Definition of done

- Deal, Opportunity, Application, Booking, Contract, and Concert retain their established meanings and
  cardinalities.
- Opportunity, Application, Booking, and Concert have honest module ownership; no module is an umbrella
  named after one downstream entity while owning the entire chain.
- Application, Booking, and Concert own independent state and transitions; no combined lifecycle state
  or separately persisted process root exists.
- The shared Kernel machine is stateless, immutable, domain-neutral, deliberately Result-based, free of
  infrastructure dependencies, and contains no configured B2B or Payment transition.
- Application, Booking, and Concert aggregates expose semantic operations, funnel lifecycle mutation
  through their private transition path, and leave state, auxiliary facts, and events unchanged when an
  edge is rejected.
- The runtime dependency graph is Contracts-only and acyclic, with no backwards command/control flow.
- There is no shared workflow module, cross-module strategy registry, umbrella state machine, or dependency-
  holder exposing the whole lifecycle.
- Contextual local names (`State`, `Trigger`, `StateMachine`, `ICancel`) are used without redundant
  aggregate prefixes inside their module.
- Heterogeneous Deal-varying methods resolve once through `IDealUnionFactory<TUnion>` and match by
  method-header type; each module owns its union and mappings, and no workflow repeats a four-Deal switch
  or resolves keyed services.
- Every current `DealDto` case has exact, independently tested net10 factory coverage, with an explicit
  invariant fallback and no false claim of native exhaustiveness.
- Accept and Booking-confirmation boundaries are atomic or durably convergent as specified; every
  callback order is idempotent.
- Opportunity acceptance uses an atomic claim, so concurrent Applications cannot both become accepted.
- A Booking can only originate from the accepted-application handoff, and confirmation cannot be
  invoked with only a Booking identifier or without matching financial-operation evidence.
- Success and failure use separate, fully populated facts; no outcome enum/boolean is flattened with
  nullable failure metadata.
- Cancellation, late payment, refund, settlement recovery, Contract, Invoice, and Concert-creation
  invariants remain covered.
- APIs/frontends obtain one journey view from a read projection while commands remain module-owned.
- Payment remains unaware of `DealType`, and Deal remains unaware of lifecycle state.

## 11. Rejected directions

- Deal → DealTerms renaming or a new per-artist Deal aggregate;
- deleting or demoting Application or Booking;
- Deal-owned workflow/state, including disguised Concert state passed through Deal Contracts;
- keeping all post-accept state on Application;
- moving all post-accept state onto Booking, including real Concert operations;
- an Engagement/process/lifecycle aggregate or value object spanning the chain;
- a BookingWorkflow, ConcertWorkflow, or shared Workflow object spanning multiple aggregates;
- treating a module workflow as an umbrella over another aggregate or as a passive dependency bag;
- one shared configured resolver, registry, workflow definition, state enum, transition table, or
  lifecycle machine for all modules; the stateless generic Kernel lookup algorithm is deliberately
  shared;
- a machine that stores entity state, resolves services, performs persistence, invokes callbacks, owns
  entry/exit actions, or publishes events;
- Application or Infrastructure calculating a next state and passing it into an aggregate;
- public generic `entity.Transition(...)` command surfaces in place of semantic aggregate operations;
- shared/inherited state markers, transition-error bases, `NotFound` bases, open error catalogs, or
  `Result<T, IError>` used to avoid operation-owned closed failure contracts;
- identifier-only Booking confirmation or confirmation that reloads a live Application aggregate;
- payment outcome contracts that combine success/failure with nullable case-specific fields;
- a global or cross-module union over DI services, or any union that performs service location;
  `IDealUnionFactory<TUnion>` may return a module-owned method-header union, with keyed service location
  confined to the shared net10 factory implementation and replaced by direct interface union cases on
  .NET 11;
- any Rust lifecycle, settlement, or Deal decision engine;
- backwards synchronous calls or a command cycle hidden behind facades, DTOs, events, or Contracts.
