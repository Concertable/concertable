# Deal lifecycle ownership

> **Next steps live in @plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md → `## Next Steps`.**

## 1. Decision

Model one concrete `DealEntity` as the aggregate that owns the commercial state machine from the
moment an artist applies until the deal is rejected, withdrawn, cancelled, or completed after
settlement.

`ConcertEntity` remains a separate aggregate. It is created only when the deal becomes `Booked`, is
referenced from the deal by `ConcertId`, and owns its independent operational and marketplace life
after creation. It does not own or mirror the deal state.

This is a boundary correction, not a field move:

- the current editable economic offer becomes `DealTerms`;
- applying to an opportunity creates one concrete `Deal` between the artist and venue;
- `Application` and `Booking` remain valid phase vocabulary for UI projections and user copy, but
  neither remains a persisted aggregate;
- the Deal module owns the full commercial vertical slice, including opportunity terms, contract,
  invoicing, lifecycle orchestration, payment outcomes, and settlement rules;
- the Concert module owns the realised concert and exposes only narrow operations/facts through
  `IConcertModule`;
- Payment receives one opaque external reference instead of B2B-specific application and booking
  identifiers.

The target relationship is:

```text
Opportunity 1 ── 1 DealTerms
Opportunity 1 ── * Deal
Deal        1 ── 0..1 Contract
Deal        1 ── 0..1 Invoice
Deal        1 ── 0..1 ConcertId   ──> Concert
```

The identity and state path is:

```text
apply
  └─> Deal(Applied)
        ├─> Rejected
        ├─> Withdrawn
        └─> Accepted ─> payment outcome ─> Booked ─> finish/settlement ─> Complete
                                      └─> payment failure/retry
                         └─> cancellation/refund ─> Cancelled
```

There is one database identity throughout. Payment callbacks, contract and invoice records, concert
creation, cancellation, and settlement all correlate to the same deal.

## 2. Why this is the domain model

### 2.1 `Deal` is true at every state

An opportunity publishes terms. Applying accepts the invitation to negotiate and opens a concrete
commercial deal between one artist and one venue. That deal may fall through, be accepted, become a
booking, produce a concert, be cancelled, or complete after settlement. None of those outcomes makes
it stop being the same deal.

`Application`, `Booking`, and `Concert` each name a phase or a later result. They are useful words at
the product edge, but they cannot honestly name the one aggregate that exists for the full arc.

### 2.2 The current terms object is not a concrete deal

The current `DealEntity` is a tenant-owned, editable TPH value selected by an opportunity. It has no
artist counterparty, no lifecycle state, and no existence independent of that published opportunity.
Its domain role is terms, so its target name is `DealTermsEntity`; the contract surface becomes
`IDealTerms` with `FlatFeeTerms`, `DoorSplitTerms`, `VersusTerms`, and `VenueHireTerms`.

Once Opportunity and DealTerms share the Deal module and context, Opportunity owns its terms directly.
The terms row no longer needs to impersonate a tenant-scoped aggregate merely to cross a module seam.

### 2.3 `BookingEntity` has no surviving aggregate responsibility

The current booking row does three things: creates a second identity at Accept, stores a payment-method
reference for deferred deals, and provides joins from Application to Contract/Invoice/Concert and
Payment. It has no state machine, independent commands, or invariant that is not already an accepted
deal fact.

The target Deal already has an identity before Accept. It can store the required payment authorization,
own its contract and invoice dependants, and reference the resulting concert. Keeping Booking would
therefore preserve a phase-named identity whose only purpose is to route around the missing Deal
aggregate. `BookingService`, the booking repository/DTO hierarchy, and the booking table are removed.

An accepted or booked deal may still be presented to users as a booking. That is a projection, not a
second write model.

### 2.4 The module boundary follows the aggregate

Moving only `DealState` while leaving its executors, workflow definitions, contracts, and settlement
effects in Concert would make Concert an implementation back door into a Deal-owned aggregate. Moving
only workflow interfaces would invert the same dependency in a different layer. Both violate modular
monolith ownership.

The cohesive boundary is:

| Deal module | Concert module |
|---|---|
| Opportunity and DealTerms | Concert and concert images |
| concrete Deal and its state | draft/post/update/public concert behaviour |
| Contract and Invoice | ticket/door-revenue operational facts |
| self-billing agreement and settlement gates | concert projections and published concert events |
| apply/accept/reject/withdraw/cancel/finish executors | narrow `IConcertModule` commands and facts |
| deal-type workflow, payment steps, payment outcome handlers | no Deal entity, state, workflow, or strategy implementation |

The dependency is one-way: Deal calls `IConcertModule`. Concert does not reference Deal runtime or
Contracts. `CreateDraftAsync` returns a Concert id, which Deal stores as a nullable primitive with a
filtered unique index and no cross-module SQL foreign key. Completion discovery and settlement facts
are narrow Concert facade queries; Deal performs the transition and effects. Draft creation passes
operational snapshot facts such as whether door revenue is required, not `DealType`, so Concert does
not need Deal vocabulary to enforce its own commands.

Book and cancel transitions that write both contexts run through the existing cross-module
`IUnitOfWorkBehavior`, so the Concert mutation and Deal state change commit atomically in B2B's one
database. External Payment calls remain outside that database transaction and retain the merged
operation-id, inbox/outbox, retry, and compensation guarantees.

## 3. Target domain types and names

### 3.1 Aggregate and terms

| Concern | Target name | Location |
|---|---|---|
| concrete state owner | `DealEntity` | `Deal.Domain/Entities/DealEntity.cs` |
| editable opportunity offer | `DealTermsEntity` | `Deal.Domain/Entities/DealTermsEntity.cs` |
| public terms shape | `IDealTerms` + typed terms records | `Deal.Contracts/Terms/` |
| state vocabulary | `DealState` | `Deal.Domain/StateMachine/DealState.cs` |
| transition vocabulary | `DealTrigger` | `Deal.Domain/StateMachine/DealTrigger.cs` |
| pure transition graph | `StateMachine` | `Deal.Domain/StateMachine/StateMachine.cs` |

`DealState` and `DealTrigger` keep the noun because enum values and diagnostics frequently appear
outside the defining namespace. `StateMachine` stays short because its namespace and signatures make
the subject unambiguous; `LifecycleStateMachine` repeats what a state machine already is.

### 3.2 Workflow and registration

Use module-local names rather than carrying `Concert` or repeating `Deal` on every internal DI type:

| Current role | Target name |
|---|---|
| `IConcertWorkflow` | `IWorkflow` |
| `FlatFeeWorkflow`, etc. | unchanged |
| `ConcertWorkflowBuilder` | `WorkflowBuilder` |
| `IConcertWorkflowFactory` / implementation | `IWorkflowFactory` / `WorkflowFactory` |
| both state-machine and capability registries | `IWorkflowRegistry` / `WorkflowRegistry` |
| `ConcertWorkflowRegistration` | `WorkflowDefinition` |
| `ConcertDealStrategyBuilder` plus current Deal builder | one `StrategyBuilder` |
| both generic strategy factories | one `IStrategyFactory<T>` / `StrategyFactory<T>` |

`WorkflowDefinition` contains the `DealType`, `StateMachine`, workflow CLR type, and registered step
types. `WorkflowRegistry` is the single immutable per-`DealType` registry and answers both
`Get(type).StateMachine` and capability questions. Do not retain a separate
`DealStateMachineRegistry` and `DealWorkflowCapabilityRegistry`; those would be two long indexes over
the same definitions.

Executors and steps keep their direct, operation-shaped names (`ApplyExecutor`, `AcceptExecutor`,
`PayoutFinishStep`). They move to Deal but do not gain a redundant `Deal` prefix.

### 3.3 Phase vocabulary at the edge

- API resource identity becomes `/api/deals/{dealId}` and response/request types use `Deal`.
- Opportunity payloads expose `terms`, not a second object called `deal`.
- application lists and cards may retain `Application` in user-facing projection names when they only
  show deals in the applied phase.
- booking copy may describe accepted/booked deals, but there is no `BookingEntity`, `BookingService`,
  or `BookingId` in B2B.
- concert endpoints that need commercial artifacts link to `/api/deals/{dealId}/contract` and
  `/api/deals/{dealId}/invoice`; they do not traverse a booking join.

## 4. Aggregate invariants

`DealState` is the persisted union of states used by every deal type. Enum membership never grants a
transition. The per-type graph is the authority.

Enforce all of the following:

1. `DealEntity.DealType` is immutable and captured from the opportunity terms at apply.
2. `DealEntity.State` has a private setter and is changed only by the aggregate transition method.
3. `StateMachine` carries its `DealType`; a deal rejects a machine for any other type.
4. `StateMachine` exposes no state assignment API, only `Next(current, trigger)`.
5. `WorkflowBuilder` validates duplicate edges, an `Applied` root, reachable configured states,
   declared terminal states, and exact `DealType` coverage before DI is built.
6. `WorkflowRegistry` is the only per-type lookup. Executors, services, mappers, and steps never perform
   keyed service location or branch on `DealType`.
7. Every command and payment outcome passes through the Deal transitioner; no repository, mapper,
   seeder, bulk update, or event handler writes `State` directly.
8. Exact topology tests pin every `(from, trigger,to)` edge for every existing `DealType`. Adding an enum
   state does nothing until a workflow explicitly uses it; adding a deal type fails composition until
   its complete strategy and workflow definition exists.
9. The accept/webhook race may retain a durable `VerificationOutcome` fact on Deal so either arrival
   order can converge. It is not a second lifecycle state or transition authority; the workflow
   consumes it only when `DealState` is ready to advance.

This makes impossible-in-that-deal states representable for persistence but unreachable through the
domain API. A stronger compile-time typestate model remains a possible decision-engine implementation,
but B2B still has one persisted system of record and one aggregate invariant.

## 5. Published boundaries

### 5.1 Payment

Removing Booking must not teach Payment about Deal. `DealId` is B2B vocabulary and does not belong in
the agnostic adapter contract.

Generalise Payment's current `ApplicationId`/`BookingId` correlation to one required string
`ExternalReference`, reusing the vocabulary already present on commission bindings. B2B sends
`deal:{id}` for every checkout, verification, escrow, settlement, refund, ledger, and financial-operation
flow after apply. VenueHire's setup checkout occurs before apply and therefore uses
`opportunity:{id}`; it prepares a payment method but does not create or transition the Deal. `OperationId`
remains the idempotent command identity; the external reference identifies the caller's commercial
subject.

This is a published-package cut-over:

- add reference-native v2 client/protobuf/message surfaces and persist/index the external reference;
- publish and platform-sync before B2B consumes them;
- cut B2B to the new surface in the aggregate boundary PR;
- remove the legacy application/booking-specific surface only after every consumer is proven absent;
- re-scaffold Payment and B2B initial migrations at their respective model changes.

Do not pass a Deal id through a parameter still called `bookingId`, and do not add a permanent adapter
that translates Deal back into Application/Booking terminology.

### 5.2 B2B frontend package and HTTP wire

Venue and Artist consume the published `@concertable/b2b` package in their standalone carves. The
Opportunity `deal` → `terms`, Application → Deal, and exported `Deal`-as-terms → `DealTerms` changes
therefore cannot be an atomic source rename.

Expand first: add the `terms` wire member while retaining `deal`, publish additive `DealTerms` exports
and Deal resource client/types, then deploy the additive backend and publish the package. Phase 3
switches Venue and Artist to those published surfaces. Phase 4 removes the old `deal` member,
Application resource clients, and `Deal`-as-terms exports after repository and deployed-consumer
searches prove they are unused. Compatibility exists only at these transport/package edges during the
cut-over; it never enters the target domain model.

## 6. Delivery graph

```text
Phase 1 ─ terms vocabulary + topology baseline ──────────────┐
                                                             ├─> Phase 3 ─ B2B vertical cut-over
Phase 2 ─ published Payment + frontend boundary expansions ──┘                    │
                                                                               └─> Phase 4 ─ Payment cleanup
                                                                                            │
                                                                                            └─> Phase 5 ─ closeout
```

Phases 1 and 2 may proceed independently. Phase 3 requires both. Do not split Phase 3 into a
Deal-state PR and a later workflow move: that would deliberately create the rejected two-module
ownership seam.

### Phase 1 — terms vocabulary and executable baseline

- [ ] Pin the exact current transition topology for all four deal types, including payment failure,
  retry, late webhook, cancellation pending/failure, and settlement recovery paths.
- [ ] Rename the current editable offer model from Deal to DealTerms across Domain, Contracts,
  Application, Infrastructure, seed data, and tests.
- [ ] Rename `OpportunityEntity.DealId` to `DealTermsId`; preserve behaviour and the current module
  seam in this phase.
- [ ] Keep the existing HTTP `deal` member and published frontend `Deal`-as-terms export at the boundary
  until Phase 2 expands their replacements; internal C# names use DealTerms immediately.
- [ ] Keep the two module-local strategy builders until ownership moves; do not introduce a shared
  registry as an intermediate abstraction.
- [ ] Update Deal/Concert architecture guidance so no new code uses the old ambiguous term while the
  later phases are in flight.
- [ ] Re-scaffold B2B initial migrations and run the focused Deal/Concert unit and integration gates.

### Phase 2 — published boundary expansions

- [ ] Map the Payment Contracts/Client producer and consumer topology before changing the published
  surface; publish the additive versions through the normal package pipeline.
- [ ] Add reference-native Payment client/protobuf commands and v2 integration messages without
  removing the legacy surface.
- [ ] Persist one external reference through escrow, verification, settlement transactions, financial
  operations, ledgers, reporting, Stripe metadata, and returned outcomes.
- [ ] Prove reference and operation idempotency with unit, SQL integration, and client transport tests.
- [ ] Merge, publish, platform-sync, and verify B2B can restore the additive surface before Phase 3.
- [ ] Add the Opportunity `terms` wire member beside `deal`; deploy that additive B2B backend shape.
- [ ] Add and publish `@concertable/b2b` DealTerms exports plus Deal resource API/types while retaining
  the old Application and Deal-as-terms exports. Verify Venue and Artist standalone carves restore the
  published expansion before changing either consumer.

### Phase 3 — Deal aggregate and module-boundary cut-over

- [ ] Move Opportunity and DealTerms into one Deal context and make terms an Opportunity-owned
  one-to-one relationship.
- [ ] Replace `ApplicationEntity` with concrete `DealEntity`, preserving the one-per-artist/opportunity
  invariant and carrying tenant ids, artist/opportunity identity, immutable `DealType`, state,
  signatures/fingerprint, payment authorization, operation ids, failures, and nullable `ConcertId`.
- [ ] Replace `PaymentVerification`/`BookingAdvancer` phase naming with one durable
  `VerificationOutcome` fact and payment-outcome coordinator on Deal; preserve both webhook-before-
  accept and accept-before-webhook convergence without introducing a second state machine.
- [ ] Move Contract, Invoice, invoice sequence, self-billing agreement/gate, their repositories,
  services, renderers, and endpoints into Deal.
- [ ] Move the complete workflow vertical slice into Deal: executors, steps, checkout dispatch,
  transitioner, payment outcome handlers, payee/amount/terms/settlement strategies, and DI registration.
- [ ] Merge the existing Deal and Concert per-type registrations into one validated vertical
  `StrategyBuilder`; introduce `WorkflowDefinition` and the single `WorkflowRegistry`.
- [ ] Implement `DealState`, `DealTrigger`, and `StateMachine`; route every mutation through the
  Deal aggregate guard and port the exact topology tests.
- [ ] Extend `IConcertModule` with narrow draft creation, cancellation, completion-candidate, and
  settlement-fact operations. Return DTOs/scalars only; never expose `ConcertEntity`.
- [ ] Use `IUnitOfWorkBehavior` for Booked/cancel transitions that write Deal and Concert contexts;
  validate the transition before the effect and commit both local writes atomically.
- [ ] Store returned `ConcertId` on Deal. Remove Booking navigation from Concert and create the draft
  entirely from a snapshot command so Concert remains independently queryable.
- [ ] Remove `BookingEntity`, its TPH variants, service, repository, DTOs, DbSet/configuration, and all
  Application/Booking joins. Point Contract and Invoice directly at Deal.
- [ ] Cut every B2B Payment call and outcome handler to `deal:{id}` on the reference-native surface.
- [ ] Rename the internal/API/frontend resource identity from Application to Deal while retaining
  phase-specific user copy where it is genuinely an application or booking view.
- [ ] Switch Venue and Artist to the published DealTerms/Deal package surface and Opportunity `terms`
  wire member; keep the transport compatibility members only until Phase 4.
- [ ] Split dashboard aggregation by owner: Deal supplies opportunity/deal counts, Concert supplies
  concert counts/facts, and Venue/Artist compose the two facades without cross-module queries.
- [ ] Remove Concert's reference to Deal Contracts and add architecture tests for the final one-way
  Deal → Concert.Contracts dependency.
- [ ] Re-scaffold all B2B initial migrations; update dev/test seeders to create terms, opportunities,
  deals, contracts, invoices, and concerts only through their production ownership paths.
- [ ] Reconcile the legacy Rust decision-engine plan before it is resumed: B2B state remains
  authoritative, but its extraction source becomes Deal `Workflow`/`StateMachine`, not the retired
  Application/Booking/Concert chain.

### Phase 4 — retire legacy published surfaces

- [ ] Execute the breaking Payment surface removal through the repository package-cutover workflow.
- [ ] Prove no source, published consumer, integration handler, Stripe metadata parser, seed fixture,
  or test still uses Payment `ApplicationId`, `BookingId`, or booking-named operations.
- [ ] Remove the legacy client/protobuf/message versions and internal phase-specific properties.
- [ ] Rename remaining storage/reporting concepts to external-reference vocabulary and re-scaffold the
  Payment initial migration.
- [ ] Publish and platform-sync the breaking cleanup; migrate any discovered consumer in the sync PR.
- [ ] Remove the old Opportunity `deal` member, Application resource clients/types, and frontend
  `Deal`-as-terms exports after both manager SPAs and standalone carves use the replacements; publish
  the frontend cleanup and verify no deployed consumer remains.

### Phase 5 — verification and closeout

- [ ] Run focused Deal, Concert, Payment, Venue, and Artist unit/integration suites after each phase;
  use draft-PR CI for complete solution, standalone carve, and full matrix validation.
- [ ] Run the B2B API lifecycle E2E scenarios after Phase 3 and again against the final Payment package:
  all four deal types, accept payment success/failure/retry, cancellation/refund races, concert draft,
  finish, deferred settlement success/failure/recovery, contract, and invoice.
- [ ] Keep focused SQL integration coverage for both accept/verification arrival orders and duplicate
  outcomes so the renamed coordinator preserves the merged convergence guarantees.
- [ ] Update `api/Concertable.B2B/ARCHITECTURE.md`, Deal architecture guidance, Concert guidance,
  `api/docs/MICROSERVICES_ARCHITECTURE.md`, and payment vocabulary to the landed ownership model.
- [ ] Review the net boundary, delete superseded review artifacts, verify every package/platform-sync
  gate green, tick the roadmap item, then delete this plan and ledger in closeout.

## 7. Acceptance criteria

- one `DealEntity.Id` exists from apply through terminal state and is the only commercial lifecycle
  identity in B2B;
- `DealEntity.State` is the only persisted deal state and cannot be mutated with a mismatched
  deal-type machine or an undeclared transition;
- the current economic offer is named DealTerms everywhere it is not a concrete artist–venue deal;
- no `ApplicationEntity`, `BookingEntity`, `BookingService`, `BookingId`, `LifecycleStateMachine`, or
  `ConcertWorkflow*` type remains in B2B runtime code;
- Deal owns every lifecycle implementation; Concert owns no Deal workflow implementation and has no
  runtime/project reference back to Deal;
- Concert is created at Booked, remains a separate aggregate, and is referenced from Deal only by id;
- the single Deal strategy registration has exact coverage for every strategy family and workflow;
- impossible per-type states remain unreachable even though `DealState` is a shared persisted union;
- Payment receives only an opaque external reference and contains no Application, Booking, Deal,
  Opportunity, Venue, Artist, or Concert settlement vocabulary except generic caller metadata used by
  unrelated ticket-payment flows;
- API, SPAs, seeders, tests, architecture docs, Rust-plan assumptions, package publications, platform
  sync, and selected E2E coverage all agree with the final model.

## 8. Explicit non-goals

- changing the four deal types or their commercial formulas;
- changing when checkout, capture, verification, escrow release, payout, or refund occurs;
- moving persisted lifecycle state into the Rust decision engine;
- combining Deal and Concert into one aggregate;
- retaining compatibility aliases after the final consumer cut-over;
- using the refactor to introduce dynamic/user-authored deal types.
