# Review request: the contract lifecycle state machine — find the best C# expression, and rule on where the state lives

**RESOLVED 2026-06-06 — historical record.** Both questions answered; see "Expression revision 2 + step/state divorce" in `plans/CONTRACT_LIFECYCLE_FSM.md`. Q1: the family-shape expression (item 5 below) was replaced by fixed-fragment builder verbs that declare their rows directly (`Add(from, on, to)` with duplicate detection) into the same frozen table. The Stateless library was fully trialled during this revision (both as a boot-time authoring layer and as idiomatic per-transition bound instances) and rejected by the owner both times — the "no external FSM package (ceremony)" rejection below ultimately STANDS: the machines are prebuilt at startup, and the package's runtime idiom (per-transition instances) and its authoring-extraction cost both failed that constraint. Bonus from the revision: `ApplicationEntity` now guards its own invariant via double dispatch (`Transition(trigger, machine)` replaces the dumb `TransitionTo` setter). Q2: state stays on `ApplicationEntity` (keep as-is — the 1:1:1 chain makes the application the engagement root; extraction deferred until it earns the join).

## Question

The B2B Concert module's contract lifecycle was just refactored (see `plans/CONTRACT_LIFECYCLE_FSM.md`) from dual state tracking onto one per-contract FSM. The mechanism works (full build green; 16 table-derived unit tests; 56/56 Concert integration tests), but two design disputes ran through the whole implementation and neither has a satisfying answer yet:

1. **What is the best possible C# expression of this state machine?** Five expressions were tried or designed during the refactor and each was rejected or felt wrong (catalogued below). Design the one that's actually right — or argue convincingly that the current one is it.
2. **Should `LifecycleState` live as a column on `ApplicationEntity`?** It currently does ("application as engagement root"), and it works, but "the application is `Complete`" reads wrong and the owner is not convinced. Argue it properly: keep, rename the concept, or extract.

Do not take the current implementation on trust, and do not relitigate the parts that are settled (listed under Constraints). Explore the code first.

## Current implementation

- `Concert.Domain/Lifecycle/` — `LifecycleState` (9 states: `Applied, Rejected, Withdrawn, Accepted, PaymentFailed, Booked, AwaitingSettlement, SettlementFailed, Complete`), `Trigger` (10), and `ContractStateMachine`: pure mechanism — ctor takes `Dictionary<(LifecycleState, Trigger), LifecycleState>`, exposes `Transitions` (FrozenDictionary) and `Next(current, trigger)` which throws `ConflictException` (409) on no-row. No statics, no `ContractType` knowledge.
- `ConcertWorkflowBuilder` (`Concert.Infrastructure/Services/Workflow/`) — the existing per-contract fluent chain (`WithApply/WithCheckout/WithAccept/WithVerify/WithSettle/WithFinish/WithWorkflow`) now ALSO derives the transition table: each `With*` captures its step's `static LifecycleState IConcertStep.State` declaration, and `Build()` selects one of two named family shapes — `EscrowLifecycle(...)` (no verify step: FlatFee/VenueHire) or `DeferredLifecycle(...)` (verify step present: DoorSplit/Versus) — parameterized by the captured states. The chains in `ServiceCollectionExtensions.AddConcertWorkflows` are therefore the complete per-contract declaration (steps + machine), and the four tables exist nowhere as standalone data.
- `ConcertStateMachineRegistry` — registration-built `FrozenDictionary<ContractType, ContractStateMachine>` behind `IConcertStateMachineRegistry.Get(type)`, registered as a plain singleton instance (mirrors `ConcertWorkflowCapabilityRegistry`). No keyed services — the machines are dependency-free instances finished at registration, so keyed resolution would be pure indirection.
- `LifecycleTransitioner` — load application → `registry.Get(app.ContractType).Next(app.State, trigger)` → run effect → `app.TransitionTo(next)` → save. Executors fire triggers through it; steps are the effects; processors map webhook `Metadata["type"]` → trigger at the edge and let `ConflictException` propagate (bus redelivery is the ordering protection).
- State: `ApplicationEntity.State` — `internal LifecycleState State { get; private set; }` + `internal TransitionTo(next)` (transitioner is the sole writer) + `public void Accept(BookingEntity booking)` kept as the domain verb.
- Tests: `ContractStateMachineTests` resolves the registry from the real `AddConcertWorkflows()` registration and derives every expectation from `machine.Transitions` (no literal rows). Declared rows advance; undeclared cartesian pairs throw; reachability from `Applied`.

## Expressions already tried — do NOT resubmit these as-is

1. **Static tables + `All` map inside `ContractStateMachine`** — rejected: naked data dumped into a place of pure functionality; machine not agnostic of what it's validating.
2. **Separate static holder class (`ContractStateMachines`)** — rejected: a second floating class when the design's point was "two enums + one class"; disconnected from DI.
3. **Per-row `.WithTransition(from, trigger, to)` calls in the chains** — rejected: disjointed and ugly; flattens the obvious leg patterns (application phase / payment-leg-with-retry / finish hop) into an undifferentiated row dump; rows can't legally be mixed arbitrarily, and the expression should say so.
4. **Builder derivation via imperative if/else dictionary assembly** — rejected: "holy hardcoding"; two branches poking 13 indexer assignments reads as slop even when the derivation idea is right.
5. **(Current) builder derivation via two named family-shape methods** — accepted as the best so far, but with a known cost: the tables are no longer readable as standalone data anywhere, the two family shapes live as private methods on a builder in Infrastructure, and the escrow/deferred family split is inferred from `WithVerify` presence (implicit structural switch). It works; it does not feel like "the PERFECT way to express a state machine in C#", which is the bar.

Also rejected during design (don't bring back): the Stateless library or any external FSM package (ceremony), per-contract machine classes, keyed-service resolution for the machines (`IKeyedServiceProvider` — the key is runtime data and the instances are dependency-free; the registry already solves it), per-trigger step maps inside dispatchers, service locators.

## Constraints (settled — these survive any redesign)

- Two enums + one pure machine class; legality owned by the table; `ConflictException` on no-row; triggers are the input alphabet (shared executors stay contract-agnostic).
- The final enum and both family tables exactly as implemented (see `plans/CONTRACT_LIFECYCLE_FSM.md` — `Accepted` is a real state; `PaymentFailed` merges the accept-leg failures; `SettlementFailed` separate; `(Applied, VerifyPaymentFailed) → Applied` pre-accept row; verify-succeeded-pre-accept deliberately has NO row).
- The surrounding architecture is untouchable: per-contract workflows, keyed workflow factory, capability interfaces, paid/simple step splits, dispatchers → executors → steps, the builder fluent chains, `IConcertStep`'s static `State` declarations.
- "Builder order is the declaration" — no parallel declaration that can lie about the chain.
- Config belongs in the DI setup; pure types stay pure; no naked data inside functional classes.
- `Next(state, trigger)` is the future wire contract for the Rust decision-engine extraction (`RUST_CONTRACT_MICROSERVICE.md`) — whatever expression you choose must keep the table serializable/extractable.
- Table-driven tests must keep deriving expectations from the real registered declaration, never literals.

## The state-location question

Current: `LifecycleState` is one column on `ApplicationEntity`, because the application is the only row alive for the whole engagement (`Applied`/`Rejected`/`Withdrawn` precede any booking), and the chain is strictly 1:1:1 (`Booking.ApplicationId`, `Concert.BookingId`). Booking and Concert carry no state.

The owner's discomfort: "why is all the state just dumped into ApplicationEntity" — an *application* being `Complete` (a concert happened, payouts settled) stretches the entity's name past its meaning. Options to argue, with migration cost and read-path impact:

1. **Keep as-is** — application IS the engagement root; renaming discomfort is not a design flaw.
2. **Rename the concept** — the row is an engagement/contract-instance that begins life as an application; rename entity/table accordingly (what does that do to the apply/accept vocabulary, the unique constraint, the SPA wire shape?).
3. **Extract an engagement/lifecycle row** — 1:1 with application today; aligns with the four dormant `Concert*Event`s in `Concert.Contracts` that already carry a `LifecycleId`, and with the Rust extraction boundary. Costs a join on every lifecycle read and a second write per transition. When (if ever) does this earn its keep?

Note the readers before answering: `GetEndedConfirmedIdsAsync`, `ConcertValidator.CanPost`, `OpportunityRepository` accepted-side filters, `ConcertDashboardRepository` counts, derived wire `ApplicationStatus` in `ApplicationMapper`.

## Where to look

- `api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Domain/Lifecycle/` — enums + machine
- `api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/Workflow/` — builder (family shapes), registry, transitioner, factory, executors, steps, dispatchers
- `api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs` — `AddConcertWorkflows` chains
- `api/Concertable.B2B/Modules/Concert/Concertable.B2B.Concert.Domain/Entities/` — `ApplicationEntity` (State/TransitionTo/Accept), `BookingEntity`, `ConcertEntity`
- `api/Concertable.B2B/Modules/Concert/Tests/Concertable.B2B.Concert.UnitTests/Lifecycle/ContractStateMachineTests.cs`
- `plans/CONTRACT_LIFECYCLE_FSM.md` (the refactor record), `plans/DUAL_STATE_TRACKING_REVIEW.md` (the superseded review), `api/RUST_CONTRACT_MICROSERVICE.md` (extraction target)

## Deliverable

1. **The state machine expression**: a concrete design (real code, not sketches) for the best C# expression of this machine under the constraints — or a defended verdict that the current builder-derived family shapes are already the right answer. If you break a constraint, justify it explicitly; "cleaner in the abstract" doesn't count, it must be better *for this codebase's conventions*.
2. **The state-location verdict**: keep / rename / extract, with the reasoning grounded in the actual readers and the Rust extraction plan, plus migration cost (this codebase re-scaffolds `InitialCreate` rather than adding migrations — `./initial-migrations.ps1`).
3. **Test impact**: what the table-driven tests look like under your design, preserving derive-from-the-real-declaration.
