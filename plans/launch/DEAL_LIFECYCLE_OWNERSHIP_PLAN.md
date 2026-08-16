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

This decision was explicitly approved by Tommy on 2026-08-16 after comparing the current model with
the aggregate-collapse, Deal-owned workflow, premature two-way state split, and separate process-root
alternatives.

## 2. Domain meanings that do not change

- **Opportunity** is the venue's advertised opening. It owns one current Deal and may receive many
  Applications. It is upstream of the per-artist progression, not a stage to hide inside Application.
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

### Booking

Owns Booking and Contract creation, acceptance-triggered payment processing after Booking creation,
financial confirmation, payment failure/retry, and cancellation/refund before a Concert exists.
Acceptance atomically forms the Booking and Contract; financial confirmation hands authority to
Concert.

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
resolution: each module maps its persistence discriminator explicitly and retains transition authority,
while `IStepResolver<TStep>` continues to resolve runtime services.

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

The module-local resolver is `IStepResolver<TStep>`. Its implementation is the only code in that
module allowed to perform keyed DI lookup. A caller requests one operation-specific dependency:

```csharp
var step = resolver.Resolve<ICompleteStep>(dealType);
await step.ExecuteAsync(concert, cancellationToken);
```

The generic keyed-registration mechanism may be mechanically similar in each module, but
registrations, coverage declarations, step contracts, implementations, and resolver instances are
module-local. There is no shared `IWorkflowStepResolver`, cross-module registry, or registration block.

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
Within B2B, `IUnitOfWorkBehavior<T>` may coordinate the participating module DbContexts under one
ambient transaction. That coordinator is an application-boundary operation with no persisted identity
or state; it is not an umbrella aggregate.

The transaction must stage all resulting outbox work before commit. A failure creating Booking or
Contract leaves Application `Applied`.

### Payment webhook before Accept

Preserve the durable two-signal join:

- a verification callback may arrive while Application is still `Applied` and before Booking exists;
- Application records that immutable pre-accept payment evidence idempotently;
- Accept creates Booking/Contract and consumes the recorded evidence;
- whichever signal arrives second performs the one guarded handoff;
- once Booking exists, later acceptance-payment outcomes and retries are Booking-owned;
- duplicate/late callbacks are idempotent and cannot create a second Booking, Contract, or Concert.

Do not solve ordering with retries-as-waiting, cross-module polling, or a global process row.

### Booking confirmation

Financial confirmation and Concert draft creation must converge exactly once. Prefer the same ambient
cross-module transaction while both modules remain inside B2B; otherwise use an outbox/inbox handoff
with deterministic identity and an explicit pending projection. The implementation must prove there
is no lost callback, duplicate Concert, or permanently confirmed Booking without a recoverable Concert
creation path.

### Cancellation and settlement

- Application handles only pre-accept rejection/withdrawal.
- Booking handles cancellation/refund after acceptance and before Concert creation.
- Concert handles cancellation after Concert creation.
- Refund, completion, and settlement operation IDs, failures, retries, and compensations live with the
  aggregate whose command is awaiting that outcome.
- A late capture after cancellation is compensated idempotently without reopening an earlier state.
- FlatFee/VenueHire escrow release and DoorSplit/Versus deferred settlement retain their current money,
  payer/payee, retry, invoice, and completion invariants.

## 9. Delivery phases

Implementation starts from a fresh worktree based on current `origin/main`. Draft PR #614 and its
DealTerms implementation are rejected input, not an implementation base.

### Phase 1 — restore and characterize the real baseline

- [x] Retire the rejected PR/branch through the repository's safe worktree process; do not merge or
  repair its DealTerms code into the new implementation.
- [ ] Pin observable acceptance, payment, cancellation, settlement, Contract, Invoice, and
  Concert-creation outcomes at module or API boundaries before moving ownership. Do not add tests for
  the shared `LifecycleState`, its transition table, executor filenames, source tokens, or other
  implementation structure scheduled for deletion.
- [x] Record the current executors, processors, callbacks, worker, and API/HATEOAS consumers as
  migration inventory in the progress ledger rather than freezing those owners as test expectations.
- [x] Add architecture tests that fail direct runtime/entity references across the target modules.

Gate: the new branch is behaviourally identical to `origin/main`, Deal vocabulary is intact, durable
behaviour is executable as tests, and no new test depends on the legacy shared lifecycle abstraction.

### Phase 2 — establish module contracts and remove cross-stage entity navigation

- [ ] Scaffold Opportunity, Application, and Booking module project families following existing B2B
  conventions; Concert keeps only its eventual owned surface.
- [ ] Replace cross-stage EF navigation in services, specifications, workers, and mappers with owned
  IDs, module contracts, or query projections.
- [ ] Define forward handoff records carrying immutable accepted/confirmed facts and deterministic IDs.
- [ ] Preserve current API routes and wire vocabulary during the internal cutover.

Gate: the dependency graph is acyclic and Contracts-only while behaviour and public responses remain
unchanged.

### Phase 3 — split Application and Booking ownership atomically

- [ ] Move Application persistence, services, repository, API mapping, actions, and local lifecycle
  state to Application.
- [ ] Move Booking, Contract, acceptance payment/recovery, and pre-Concert cancellation to Booking.
- [ ] Replace the combined `LifecycleState` with independent Application and Booking state.
- [ ] Preserve the Accept transaction, immutable Contract snapshot, operation IDs, early-verification
  join, late-callback compensation, retry, and idempotency invariants.
- [ ] Re-home Standard/Prepaid Application and Standard/Deferred Booking without nullable flattening.

Gate: Application is terminal after its decision, Booking owns every post-accept/pre-Concert
transition, and all accept/payment arrival orders pass focused integration coverage.

### Phase 4 — give Concert independent operational ownership

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
- [ ] Add local `State`, `Trigger`, `StateMachine`, `IStepResolver<TStep>`, and contextual step contracts
  only where each module needs them.
- [ ] Register exact per-`DealType` step coverage independently in Application, Booking, and Concert.
- [ ] Update `api/agents/CODE_PATTERNS.md` and module guidance so shared keyed infrastructure cannot be
  mistaken for shared workflow ownership.

Gate: each command resolves one local step; no service can resolve another module's steps or request a
whole workflow.

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
- Contextual local names (`State`, `Trigger`, `StateMachine`, `IStepResolver<TStep>`, `ICancelStep`) are
  used without redundant aggregate prefixes inside their module.
- Every `DealType` has exact, independently validated coverage for the local operations it requires.
- Accept and Booking-confirmation boundaries are atomic or durably convergent as specified; every
  callback order is idempotent.
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
- one shared resolver, registry, workflow definition, state enum, or state machine for all modules;
- unions over DI service implementations rather than closed values;
- any Rust lifecycle, settlement, or Deal decision engine;
- backwards synchronous calls or a command cycle hidden behind facades, DTOs, events, or Contracts.
