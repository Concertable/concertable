# Review request: dual state tracking in the B2B Concert module

**SUPERSEDED (2026-06-06):** the "keep both axes" verdict below was overruled — both axes plus the stage guard are being replaced by one per-contract FSM. See `plans/CONTRACT_LIFECYCLE_FSM.md` (Stage 0 explored and signed off; final transition tables live there).

## Question

In `api/Concertable.B2B/Modules/Concert/`, the lifecycle entities each track state on two axes:

- `BookingEntity` — `Status` (`BookingStatus`: Pending / AwaitingPayment / Confirmed / Complete / PaymentFailed) **and** `CurrentStage` (`ConcertStage`)
- `ApplicationEntity` — `Status` (`ApplicationStatus`: Pending / Accepted / Rejected / Withdrawn) **and** `CurrentStage` (note: both enums contain an "Accepted")
- `ConcertEntity` — `CurrentStage` plus `DatePosted` acting as a de-facto published flag

`Status` is read by queries, DTOs, and domain-method guards. `CurrentStage` is consumed by the workflow machinery (`WorkflowStateMachine<TEntity>` → `ConcertTransitionValidator`, sequence built per contract type in `ConcertWorkflowBuilder` from step-registration order).

**Is this dual tracking a genuine design problem, or is it sound? Explore the code and give a verdict.** Do not take either position below on trust — verify against the code.

## Competing hypotheses to test

1. **Redundant sources of truth.** The stage axis duplicates protections that already exist elsewhere (domain-method status guards like `Confirm()`/`Complete()`/`AwaitPayment()`, the workflow capability interfaces such as `IVerifies`, webhook inbox dedup in the `*PaymentProcessor` handlers). If every illegal transition is already blocked by those, `CurrentStage` and the validator add only drift risk and could be removed (or one axis derived from the other / unified).
2. **Distinct concerns, both earned.** `Status` models business/payment outcomes (including failure branches like PaymentFailed), `CurrentStage` models position in a per-contract workflow sequence; collapsing them loses the per-contract ordering guarantee or muddies the domain model. The right fix may instead be clearer ownership rules between the axes, or moving more truth INTO the stage axis rather than deleting it.

Also consider any option neither hypothesis covers.

## Where to look

- Entities: `Concertable.B2B.Concert.Domain/Entities/` (`ApplicationEntity`, `BookingEntity`, `ConcertEntity`), `ILifecycleEntity`
- Workflow machinery: `Concertable.B2B.Concert.Infrastructure/Services/Workflow/` (builder, validator, state machine, executors, dispatchers, steps), capability interfaces in `Concertable.B2B.Concert.Application/Workflow/`
- Webhook entry points: `Concertable.B2B.Concert.Infrastructure/Services/Payment/*Processor.cs`
- Who reads which axis: grep `CurrentStage` vs `Status` usages across queries/DTOs/repositories
- Seeding of mid-lifecycle entities: `Concertable.B2B.Seed.Infrastructure/Factories/BookingFactory.cs`, `ApplicationFactory.cs`
- Safety net: `Concertable.B2B.Concert.IntegrationTests` (56 tests, currently green) — including per-contract finish/settle tests under `Concert/`

Note: the working tree (branch `refactor/Microservices`) contains recent uncommitted changes in exactly this area — several bugs were just fixed here. Run `git status` / `git diff` to see them, and weigh that history as evidence for whichever hypothesis it actually supports, not as a presumption.

## Deliverable

1. A clear verdict: is the dual tracking an issue worth refactoring, or not?
2. The reasoning, grounded in specific code references — in particular, enumerate what (if anything) the stage axis blocks that the status guards + capability checks + inbox dedup do not, and vice versa.
3. If it is an issue: the recommended direction (delete an axis, derive, unify, or re-own), with trade-offs, migration risk, and what the existing test suite does/doesn't cover for the change.
4. If it is not an issue: the invariants and ownership rules that make the current design safe, stated explicitly enough to document.

---

## Verdict (2026-06-06)

**The dual tracking is sound. Hypothesis 2 holds: distinct concerns, both earned. No delete/derive/unify refactor is warranted.**

### Neither axis is derivable from the other

The Status/Stage pairs form a genuine 2D space:

- `(Confirmed, Settled)` is the standard-booking resting state while `(Confirmed, Verified)` is the deferred one (`BookingFactory.Confirmed` vs `ConfirmedDeferred`) — same status, different stages.
- `AwaitingPayment` occurs at stage `Accepted` (deferred accept) **and** at stage `Verified` (`DoorSplitFinishStep` marks awaiting-payment when finish initiates settlement) — same status, different stages, and vice versa the same stage carries multiple statuses over its lifetime.

So "derive one axis from the other" is dead on the evidence, in both directions.

### The axes have disjoint consumers and disjoint jobs

- `Status` carries all read-side and business semantics: every repository filter, DTO, and domain-method guard reads it. Critically, `ConcertRepository.GetEndedConfirmedIdsAsync` (the completion runner's work query) filters on `Application.Status == Accepted && Booking.Status == Confirmed` — the runner's re-entry idempotency is **status**-based, not stage-based. Status is also the only axis that can represent failure branches (`PaymentFailed`, `Rejected`, `Withdrawn`); the strictly linear `ConcertTransitionValidator` (`ti == fi + 1`) cannot.
- `CurrentStage` has exactly **one** decision-reader in the entire `api/` tree: `WorkflowStateMachine.Guard`. It is never a query filter, never in a DTO, never read by a handler. It is purely the workflow front door: a uniform fail-fast `ConflictException` (409) thrown **before** any step side effects run.

### What the stage guard blocks vs. the status backstops

Every duplicate/out-of-order transition does have a *transitive* status backstop — but the backstops are deep, incidental, and fire mid-step:

| Transition | Stage guard | Status backstop if stage guard vanished |
|---|---|---|
| Accept twice | blocks at front door | `ApplicationValidator.CanAccept` + `ApplicationEntity.Accept()` (Pending-only) |
| Verify twice (deferred) | blocks | `BookingEntity.Confirm()` throws — but only inside `ConcertDraftService.CreateAsync`, after re-running genre matching |
| Settle twice (deferred) | blocks | `BookingEntity.Complete()` throws |
| Settle twice (standard) | blocks | **none intentional** — `NoOpSettleStep` mutates nothing; the only stop is `Confirm()` throwing inside the `BookingSettledDomainEventHandler` → draft-creation chain, three layers down |
| Finish twice | blocks | `Complete()` / `AwaitPayment()` throw inside the finish steps, after revenue calculation/logging; plus the status-filtered runner query |

Inbox dedup only covers same-`MessageId` replays; a duplicate webhook with a fresh event id passes it. The stage guard is what turns those into clean 409s instead of `DomainException`s thrown from arbitrary depths after partial side effects. The layering is deliberate: stage guard = orchestration front door, status guards = business backstop, inbox dedup = transport replay.

### Invariants and ownership rules (the documentation)

1. **The relay model.** One conceptual `ConcertStage` timeline per contract type is relayed across three entities; each is born at the stage where it enters the story and traverses only its slice: `ApplicationEntity` at `None` (→ Applied → Accepted), `BookingEntity` at `Accepted` (→ [Verified] → Settled), `ConcertEntity` at `Settled` (→ Finished). The birth-stage property initializers are load-bearing.
2. **Stage ownership.** `CurrentStage` is written only by `WorkflowStateMachine.TransitionAsync` via `AdvanceStage`. Nothing else may call `AdvanceStage`. Stage moves only forward, one step, along the per-contract sequence.
3. **Status ownership.** `Status` is written only by entity domain methods (`Accept/Reject/Withdraw`, `AwaitPayment/Confirm/Complete/FailPayment`), invoked from workflow steps and services. Their guards are the business invariants. Failure branches live here exclusively; a failed booking keeps the stage where it stalled.
4. **Sequence declaration.** The per-contract sequence is the registration order of the transition steps (`WithApply → WithAccept → [WithVerify] → WithSettle → WithFinish`) in `ServiceCollectionExtensions`. `WithCheckout` registers a step but contributes no stage — checkout is not a transition.
5. The working-tree fixes preceding this review support the design rather than indict it: the removed per-entity `AdvanceStage` whitelists were workflow knowledge duplicated into entities (drift risk, now gone), and the seed factories now pair both axes explicitly per lifecycle position.

### Action taken

One genuine fragility was found and fixed: checkout steps used to contribute their interface stage to the sequence (`IAcceptCheckoutStep.Stage => Accepted`), and DoorSplit/Versus relied on that to slot `Accepted` before `Verified` while their chains declared `WithVerify` before `WithAccept`. `WithCheckout` no longer registers a stage, `RegisterStep`'s now-vestigial `Contains` dedup is gone, and the DoorSplit/Versus chains were reordered to declare the real sequence (`WithAccept` before `WithVerify`). Effective sequences are unchanged.
