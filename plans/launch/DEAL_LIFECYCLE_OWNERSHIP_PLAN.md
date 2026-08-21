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
AwaitingFinancialConfirmation
FinancialConfirmationFailed
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

Cancellation and completion each use their own operation-specific executor. The lifecycle delivery may
retain its minimum provisional keyed seams, but the downstream Deal dispatch plan classifies them:
cancellation becomes a direct refund collaborator because every Deal case is identical, while completion
retains validated keyed release/payout implementations because it is one substantial homogeneous
operation with materially different effect graphs. There is no multi-operation `IConcertExecutor`:
established executors own one named lifecycle operation, and combining both operations would create a
dependency bag rather than a cohesive facade.
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

Each module owns its own transition vocabulary and implementation. The types may use the same short
names because their module namespace is the context:

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

They do not implement a common lifecycle interface or inherit from an umbrella state machine. A
module may use an explicit transition table, aggregate methods, or a .NET 11 native union when that
best expresses its closed local states, triggers, or outcomes. Similar syntax is not a reason to force
identical structure.

A tiny generic transition primitive may be extracted only after real duplication is demonstrated. It
must contain no B2B state, trigger, `DealType`, module reference, transition table, or ownership rule.

## 6. Deal-type behaviour and step resolution

Delete the runtime `IConcertWorkflow` dependency-holder. No request needs every lifecycle operation at
once, and no executable workflow spans the module boundary.

Each module owns only the step families that operate on its aggregates. Internal types use contextual
names rather than repeating the aggregate name:

| Module | Local step contracts |
|---|---|
| Application | `IApplyStep`, `IApplyCheckoutStep`, `IAcceptStep`, `IAcceptCheckoutStep` |
| Booking | `IConfirmStep`, `ICancelStep` |
| Concert | `ICancelStep`, `ICompleteStep`, and local settlement-recovery steps where required |

Deal-varying operations are classified by invocation shape. A genuine same-interface family is selected
through the module's invariant Deal strategy factory. An operation whose implementations require
different parameters, results, or capabilities gets a dedicated operation factory returning a closed
operation union. Its consumer matches the operation kind, never the four Deal cases:

```csharp
await acceptFactory.Create(deal).Match(
    captureEscrow => captureEscrow.Value.AcceptAsync(application, cancellationToken),
    paid => paid.Value.AcceptAsync(
        application,
        RequirePaymentMethod(paymentMethodId),
        cancellationToken),
    depositEscrow => depositEscrow.Value.AcceptAsync(application, cancellationToken));
```

The Deal dispatch foundation is terminal on `main`. It delivered the Deal module's validated invariant
net10 factory for the honest `IDealMapper` and `IDealUpdater` families; the production generator and
analyzer prototype was deliberately removed. PR #633 must consume that proven pattern without claiming
generated machinery exists or reaching into Deal's internal factory implementation.

Application `IDealTerms` remains a genuine same-interface family. Application Infrastructure owns its
equivalent invariant factory and complete `DealType` registration catalog. Heterogeneous lifecycle
operations instead use dedicated non-generic factories returning module-local Dunet unions over their
concrete implementations, with typed constructor injection and one Deal-to-operation mapping. Multiple
Deal cases may deliberately map to one operation case.

Because this union names concrete DI implementations, it and its factory remain in the owning module's
Infrastructure assembly beside the effectful consumer. It must not make Application reference
Infrastructure or turn a cross-module fact into a service carrier.

The .NET 11 follow-up preserves the factory and call-site semantics and replaces each Dunet wrapper with
a native union against `closed Deal`. The compiler then enforces the operation-union match and closed
Deal switch. Neither design contains an
`IWorkflowStepResolver`, `IStepResolver<TStep>`, `IKeyedServiceProvider`, global workflow bundle, or
four-Deal executor switch.

`IApplicationDealStrategyFactory<TStrategy>` remains separate and applies only to genuine Application
strategy families such as `IDealTerms`. It is not reused as `IApplicationDealStrategyFactory<Accept>`:
the union cases do not share one substitutable invocation, so `Accept` is not a strategy family.

Cancel and Complete use separate executors because each is one named Concert lifecycle operation with
its own validation, persistence, transaction, IO, and typed failure contract. Uniform creation remains
on `IConcertService`; it neither selects a keyed step nor belongs in either executor. Expected failures
use typed Results; no design may convert them into explicit exceptions merely to cross an internal
boundary.

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
Within B2B, the Application, Opportunity, and Booking module DbContexts join one ambient SQL
transaction. `ApplicationAcceptedDomainEvent` is dispatched synchronously pre-commit so Booking and
Contract formation either commits with Application acceptance or all participating writes roll back.
That coordinator is an application-boundary operation with no persisted identity or state; it is not
an umbrella aggregate.

The transaction must stage all resulting outbox work before commit. A failure creating Booking or
Contract leaves Application `Applied`.

Acceptance must first claim the Opportunity atomically with an `Open` to `Filled` conditional write.
A zero-row claim is an expected typed conflict Result. The claim, Application acceptance, sibling
rejection, Booking and Contract creation, and staged outbox writes share the transaction. Concurrent
Applications for one Opportunity must therefore produce exactly one Accepted Application, Booking,
and Contract; all siblings become Rejected. No `AcceptedPendingBooking` state or reconciliation process
is introduced.

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
  the shared `LifecycleState`, its transition table, executor filenames, source tokens, or other
  implementation structure scheduled for deletion.
- [x] Record the current executors, processors, callbacks, worker, and API/HATEOAS consumers as
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

### Phase 3 — split Application and Booking ownership atomically

- [ ] Move Application persistence, services, repository, API mapping, actions, and local lifecycle
  state to Application.
- [ ] Move Booking, Contract, acceptance payment/recovery, and pre-Concert cancellation to Booking.
- [ ] Replace the combined `LifecycleState` with independent Application and Booking state.
- [ ] Preserve the Accept transaction, immutable Contract snapshot, operation IDs, early-verification
  join, late-callback compensation, retry, and idempotency invariants.
- [ ] Make Opportunity acceptance an atomic `Open` to `Filled` claim and prove concurrent acceptance
  yields exactly one Accepted Application, Booking, and Contract while rejecting every sibling.
- [x] Require accepted-application provenance for Booking creation and explicit correlated financial
  success/failure facts for later outcomes; remove identifier-only confirmation and nullable outcome
  payloads.
- [ ] Re-home Standard/Prepaid Application and Standard/Deferred Booking without nullable flattening.

Gate: Application is terminal after its decision, Booking owns every post-accept/pre-Concert
transition, and all accept/payment arrival orders pass focused integration coverage.

### Phase 4 — give Concert independent operational ownership

- [x] Before changing the Concert application boundary, independently research the candidate
  `IConcertExecutor`/`ConcertExecutor` and uniform `CreateAsync(ConfirmedBooking)` placement against the
  final dependency graph, keyed-step conventions, typed-Result semantics, and comparable repository
  code. Record the decision in this plan and ledger before implementation.
- [ ] Create Concert only from a financially confirmed Booking handoff.
- [ ] Move draft/posting, post-creation cancellation, completion, settlement recovery, and relevant
  financial operation facts onto Concert or justified Concert-owned children.
- [ ] Remove every Concert query or command that interprets `Application.State` or loads upstream
  entities to determine a Concert transition.
- [ ] Decide Invoice/settlement/ticket-transaction placement from their identity and transaction
  evidence; create a separate financial module only if it owns an independent lifecycle.

Gate: Concert can validate and complete every operation from its own state plus immutable handoff facts.

### Phase 5 — replace the god workflow with local steps

- [ ] Delete `IConcertWorkflow`, concrete `*Workflow` dependency-holders, the workflow factory,
  cross-stage builder, state-machine registry, and reflection capability registry.
- [ ] Add local `State`, `Trigger`, `StateMachine`, the minimum provisional keyed-selection seams,
  named operation facades, and contextual step contracts only where each module needs them.
- [ ] Register exact per-`DealType` step coverage independently in Application, Booking, and Concert.
- [ ] Update module guidance for lifecycle ownership without ratifying the provisional selector
  mechanism; the separate dispatch investigation owns any general `api/agents/CODE_PATTERNS.md`
  replacement.
- [ ] After the compile-recovery frontier is green, apply the landed validated invariant-factory pattern
  to Application `IDealTerms`. Application owns its marker, factory, catalog, registrations, and exact
  `DealType` coverage; it does not reference Deal Infrastructure internals.
- [ ] Give each genuinely heterogeneous operation one dedicated net10 factory returning a Dunet union
  over its concrete implementations. Keep mapping once at the module composition boundary, allow
  deliberate many-Deal-to-one-operation aliases, match by operation case, and remove keyed service-
  provider lookup from consumers.
- [ ] Do not convert honest same-interface families to operation unions or erase heterogeneous
  invocations behind a manufactured common interface.

Gate: each command invokes one module-owned operation; no service can resolve another module's
operations or request a whole workflow.

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
- The runtime dependency graph is Contracts-only and acyclic, with no backwards command/control flow.
- There is no shared workflow module, cross-module step registry, umbrella state machine, or dependency-
  holder exposing all steps.
- Contextual local names (`State`, `Trigger`, `StateMachine`, `ICancelStep`) are used without redundant
  aggregate prefixes inside their module.
- Heterogeneous Deal-varying operations resolve once through module-local dedicated factories and match
  by operation case; no executor repeats a four-Deal switch or resolves keyed services.
- Every current Deal case has exact, independently tested net10 factory coverage, with an explicit
  fallback for open `IDeal` and no false claim of native exhaustiveness.
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
- a BookingWorkflow, ConcertWorkflow, or shared Workflow module spanning multiple aggregates;
- treating `ConcertExecutor` as a replacement umbrella workflow or a dependency bag unrelated to
  executing Concert-owned step families;
- combining Cancel and Complete behind one multi-operation `IConcertExecutor` rather than preserving
  one cohesive executor per named lifecycle operation;
- one shared resolver, registry, workflow definition, state enum, or state machine for all modules;
- identifier-only Booking confirmation or confirmation that reloads a live Application aggregate;
- payment outcome contracts that combine success/failure with nullable case-specific fields;
- a global or cross-module union over DI services, or any union that performs service location; a
  module-local dedicated factory returning its closed heterogeneous operation implementations is allowed;
- any Rust lifecycle, settlement, or Deal decision engine;
- backwards synchronous calls or a command cycle hidden behind facades, DTOs, events, or Contracts.
