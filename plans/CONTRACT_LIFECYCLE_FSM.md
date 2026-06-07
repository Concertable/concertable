# Contract lifecycle FSM — unify Status + CurrentStage into one state machine

## Context

The dual-tracking review (`plans/DUAL_STATE_TRACKING_REVIEW.md`) concluded the two axes were individually defensible — but the deeper finding stands: every workflow bug we've hit lives in the seams between three half-mechanisms (entity status guards, the stage guard, inbox dedup), and the real rejection path already bypasses the guards entirely (`ApplicationRepository.RejectAllExceptAsync` is a raw `ExecuteUpdate`). Decision: replace all of it with one mechanism — a proper finite state machine per contract type. This supersedes the review's "keep both axes" verdict; annotate that doc when this lands.

**The goal is the best state machine implementation for THIS problem** — not the design below verbatim. The design captures the agreed shape (pure-data tables, one lookup class, triggers as the input alphabet, zero ceremony), but it was drafted from a partial reading of the workflow surface and may be missing pieces. Stage 0 exists to find them: explore first, and where exploration contradicts the draft (extra states, missing triggers, a flow the table shape can't express cleanly), revise the design to fit the problem — never force the problem into the draft.

## The design (settled in discussion 2026-06-06)

Two enums + one class. Everything else is deletion.

```csharp
internal sealed class ContractStateMachine
{
    private readonly FrozenDictionary<(LifecycleState, Trigger), LifecycleState> transitions;

    public ContractStateMachine(Dictionary<(LifecycleState, Trigger), LifecycleState> transitions)
    {
        this.transitions = transitions.ToFrozenDictionary();
    }

    public LifecycleState Next(LifecycleState current, Trigger trigger)
        => transitions.TryGetValue((current, trigger), out var next)
            ? next
            : throw new ConflictException($"Cannot {trigger} from {current}");
}
```

- **`LifecycleState`** (new enum) — merges `ConcertStage` + `ApplicationStatus` + `BookingStatus`. Members are the *valid* combinations of today's axes, so invalid combos become unrepresentable.
- **`Trigger`** (new enum) — the FSM input alphabet. Collapses today's four trigger encodings: webhook metadata strings (`Metadata["type"]` vs `TransactionTypes.*`), the hardcoded `ConcertStage` targets in the five executors, bare method calls (reject/withdraw), and the anonymous completion timer.
- **Four tables, one per contract** — pure data, all per-contract variance lives here. The map `FrozenDictionary<ContractType, ContractStateMachine>` is built at registration and **ctor-injected** (no keyed services, no locator, no factory).
- Callers fire a trigger; no row in the table = `ConflictException` (409). That single lookup IS the duplicate-webhook/ordering protection.
- Steps stay — they are the transitions' side effects. Executors/processors shrink to: resolve application id → fire trigger (+ effect).

### State ownership (recommended: Application is the engagement root)

`LifecycleState` lives as ONE column on `ApplicationEntity`. The application exists from the first transition (apply), is 1:1 with Booking (`Booking.ApplicationId`) which is 1:1 with Concert, and is the only entity that can hold `Rejected`/`Withdrawn`. Booking and Concert lose `Status`, `CurrentStage`, and all guarded mutators — they become data + effects. No new table needed; the "engagement entity" is the application. (Alternative considered: a separate engagement row — more churn, no added value while the 1:1 chain holds.)

This also IS the Rust extraction boundary later: `Next(state, trigger)` is the stateless decision-engine wire contract (`RUST_CONTRACT_MICROSERVICE.md`).

## Stage 0 findings (✅ explored + signed off 2026-06-06)

The table shape holds — flat `(state, trigger) → state` per contract, no guard conditions needed. Four draft assumptions broke:

1. **VenueHire has no awaiting-payment state at apply.** `VenueHireApplyCheckoutStep` creates a Stripe *setup session* (card save only, `CreateSetupSessionAsync`); the actual charge is an off-session escrow deposit (`EscrowClient.DepositAsync`) inside `VenueHireAcceptStep`. VenueHire uses the FlatFee table shape exactly — no `AwaitingHireFee` state.
2. **Verify failure is not terminal today** — `VerifyPaymentFailedProcessor` only notifies (no state change) and a later verify webhook still lands the booking. `VerificationFailed` becomes a first-class state WITH retry rows, so failure is queryable and retry still works.
3. **Failure dead-ends get recovery rows.** Today `BookingStatus.PaymentFailed` is a stuck terminal (re-accept blocked by validator). `(EscrowFailed, EscrowPaymentSucceeded) → Booked` and `(SettlementFailed, SettlementPaymentSucceeded) → Complete` let a late/retried Stripe success (e.g. delayed 3DS completion) recover the contract.
4. **Withdraw is dead code today** (no controller endpoint, unwired SPA label in `ArtistApplicationsPipelineWidget`) but stays in all four tables — it's a future feature; the table is the full lifecycle declaration.

Confirmed at the edges:

- **No escrow trigger ambiguity**: Payment's `WebhookProcessor` only reacts to `PaymentIntent` events; escrow *release* at Finish is a Stripe Transfer → a second `type=escrow` webhook never fires. `type=escrow` maps 1:1 to the accept-leg capture/deposit.
- **Error semantics**: exact duplicate webhooks are already silently dropped by the atomic inbox write — inbox stays untouched (the `Accept_ShouldIgnoreDuplicateWebhookEvent` tests' "2 notifications" = 1 draft × 2 recipients). FSM no-row `ConflictException` PROPAGATES in bus processors: redelivery covers the real verify-webhook-before-accept race (checkout precedes accept, so the webhook can legitimately arrive first — today that race resolves via `NotFoundException` retry). API-sourced triggers surface it as HTTP 409.
- **`Application.Accept` + sibling bulk reject** run in `ApplicationAcceptedDomainEventHandler` today (chained from `AdvanceStage`, bulk reject on a background task); both collapse into the Accept transition's effect. Bulk reject stays set-based, writing `Rejected` where `state == Applied` — legality owned by the `(Applied, Reject)` row.
- **Apply is creation, not a transition**: the application is created in `Applied` (Standard vs Prepaid variance stays in the apply steps); duplicate-apply protection remains the DB unique constraint.
- The four dormant `Concert*Event` Contracts events (`LifecycleId` fields, no publishers/consumers — TECH_DEBT "Defined-but-not-published events") are out of scope; they presage the Rust wire contract.

## Final enums (REVISED during Stage 2, 2026-06-06 — user rulings)

```csharp
internal enum LifecycleState
{
    Applied,
    Rejected,
    Withdrawn,
    Accepted,               // accept landed; payment leg pending (which leg = contract type)
    PaymentFailed,          // accept-leg payment failed (verify hold / escrow capture) — retryable
    Booked,                 // payment confirmed, draft created — CanPost gate
    AwaitingSettlement,     // deferred payout leg
    SettlementFailed,       // post-Finish payout failed — recovery lands Complete, not Booked
    Complete,
}

internal enum Trigger
{
    Accept, Reject, Withdraw,
    VerifyPaymentSucceeded, VerifyPaymentFailed,
    EscrowPaymentSucceeded, EscrowPaymentFailed,
    SettlementPaymentSucceeded, SettlementPaymentFailed,
    Finish,
}
```

Stage 2 enum rulings (supersede the Stage 0 draft):
- **`Accepted` is a real state** — accept lands there; the booking is created as the accept effect and `ApplicationEntity.Accept(booking)` stays the core domain verb. `AwaitingVerification`/`AwaitingEscrow` are dissolved: which payment leg is pending is already implied by contract type (tables are per-contract).
- **`PaymentFailed` merges `VerificationFailed` + `EscrowFailed`** — same position (accept-leg payment failed), same recovery (`→ Booked`); leg = contract type. `SettlementFailed` stays separate: different position (post-Finish), different recovery (`→ Complete`); merging it would make "verify-failed jumps to Complete without ever being Booked" representable.

## Final per-contract transition tables

FlatFee / VenueHire (escrow contracts — identical table; effects differ: FlatFee Accept captures the held escrow, VenueHire Accept runs the off-session `DepositAsync` with the card saved at apply; Finish releases escrow synchronously — no webhook back-edge, release is a Transfer):

```csharp
[(Applied, Accept)]                       = Accepted,        // accept step: create booking + capture/deposit escrow; sibling bulk-reject
[(Applied, Reject)]                       = Rejected,
[(Applied, Withdraw)]                     = Withdrawn,
[(Accepted, EscrowPaymentSucceeded)]      = Booked,          // DraftSettleStep: concert draft + notify (old BookingSettledDomainEventHandler body)
[(Accepted, EscrowPaymentFailed)]         = PaymentFailed,
[(PaymentFailed, EscrowPaymentSucceeded)] = Booked,          // late-success recovery
[(Booked, Finish)]                        = Complete,
```

DoorSplit / Versus (deferred contracts — identical table; Finish payout math differs: DoorSplit `revenue × pct`, Versus `guarantee + revenue × pct`):

```csharp
[(Applied, Accept)]                                = Accepted,            // PaidAcceptStep: deferred booking + store paymentMethodId
[(Applied, Reject)]                                = Rejected,
[(Applied, Withdraw)]                              = Withdrawn,
[(Applied, VerifyPaymentFailed)]                   = Applied,             // pre-accept decline — notify only; succeeded has NO row (redelivery waits for accept)
[(Accepted, VerifyPaymentSucceeded)]               = Booked,              // DeferredVerifyStep: concert draft + notify
[(Accepted, VerifyPaymentFailed)]                  = PaymentFailed,       // notify venue manager
[(PaymentFailed, VerifyPaymentSucceeded)]          = Booked,              // retry lands
[(PaymentFailed, VerifyPaymentFailed)]             = PaymentFailed,       // re-notify
[(Booked, Finish)]                                 = AwaitingSettlement,  // finish step: compute payout, initiate off-session payout
[(AwaitingSettlement, SettlementPaymentSucceeded)] = Complete,
[(AwaitingSettlement, SettlementPaymentFailed)]    = SettlementFailed,
[(SettlementFailed, SettlementPaymentSucceeded)]   = Complete,            // late-success recovery
```

Notes:
- Triggers, not target-states, key the table: shared executors stay contract-agnostic (FlatFee Finish lands `Complete`; deferred lands `AwaitingSettlement`).
- Concert *posting* stays the orthogonal `DatePosted` flag; `ConcertValidator.CanPost` gate = `state == Booked && DatePosted is null`.
- The "concert must have ended" guard (formerly in `BookingEntity.Complete()`) is temporal, not sequential — it lives in `FinishExecutor` (period check before the transition), mirroring how accept-side business rules live in `ApplicationValidator`.

## Call site → trigger map (complete)

| Trigger | Fired by | Today's path |
|---|---|---|
| `Accept` | `POST /api/Application/{id}/accept` (+ `DevController` `/accept`) | `AcceptanceDispatcher` → `AcceptExecutor` |
| `Reject` | set-based effect inside the Accept transition | `ApplicationAcceptedDomainEventHandler` → `ApplicationRepository.RejectAllExceptAsync` (ExecuteUpdate) |
| `Withdraw` | none yet (future endpoint) | dead `ApplicationEntity.Withdraw()` |
| `VerifyPaymentSucceeded` | `PaymentSucceededEvent` `type=verify` | `VerifyPaymentProcessor` → `VerifyExecutor` |
| `VerifyPaymentFailed` | `PaymentFailedEvent` `type=verify` | `VerifyPaymentFailedProcessor` (notify-only today) |
| `EscrowPaymentSucceeded` | `PaymentSucceededEvent` `type=escrow` | `EscrowPaymentProcessor` → `SettleExecutor` |
| `EscrowPaymentFailed` | `PaymentFailedEvent` `type=escrow` | `BookingPaymentFailedProcessor` → `FailPaymentAsync` |
| `SettlementPaymentSucceeded` | `PaymentSucceededEvent` `type=settlement` | `SettlementPaymentProcessor` → `SettleExecutor` |
| `SettlementPaymentFailed` | `PaymentFailedEvent` `type=settlement` | `BookingPaymentFailedProcessor` → `FailPaymentAsync` |
| `Finish` | hourly `ConcertFinishedFunction` → `ConcertCompletionRunner` (+ `DevController` `/complete`) | `CompletionDispatcher` → `FinishExecutor` |

Processors map metadata `type` → trigger at the edge; `type=ticket` (`TicketSaleProcessor`) stays outside the machine (orthogonal `TicketsSold` increment).

## Reader remap (Stage 3 targets, confirmed complete)

- `ConcertRepository.GetEndedConfirmedIdsAsync` (`Application.Status==Accepted && Booking.Status==Confirmed && started`) → `state == Booked && Period.Start < now` — self-clearing post-Finish (FlatFee/VenueHire → `Complete`, deferred → `AwaitingSettlement` both drop out)
- `ConcertValidator.CanPost` (`Booking.Status==Confirmed`) → `state == Booked && DatePosted is null`
- `OpportunityRepository.GetActiveByVenueIdAsync` ×2 (`!Any(Status==Accepted)`) → no sibling with an accepted-side state
- `ConcertDashboardRepository` venue/artist counts (`Status==Pending`) → `state == Applied`
- `ApplicationValidator.CanAcceptAsync` re-accept guard → subsumed by no `(non-Applied, Accept)` row (409)
- `ApplicationMapper`/`ApplicationResponse`: wire `ApplicationStatus` **derived**: `Applied→Pending`, `Rejected→Rejected`, `Withdrawn→Withdrawn`, else→`Accepted` (no SPA churn)

## What gets deleted

- `ConcertStage`, `ApplicationStatus`, `BookingStatus` enums + the `Status`/`CurrentStage` columns and `AdvanceStage` on all three entities
- All entity status-guard mutators (`Accept/Reject/Withdraw`, `AwaitPayment/Confirm/Complete/FailPayment`) — entities keep data + navs; effects move to steps
- `WorkflowStateMachine<TEntity>`, `ILifecycleEntity`, `ILifecycleRepository<>` (the transitioner works on the application row)
- `IConcertTransitionValidator` + impl + factory (partially collapsed already by the 2026-06-06 builder hardening — fully gone now); the sequence half of `ConcertWorkflowBuilder` (step DI registration stays)
- Capability marker interfaces (`IVerifies` etc.) where "does this contract support X" = "does the table have a row" (checkout capabilities stay — checkout is outside the machine)
- `BookingFactory`'s hand-paired Status+CurrentStage seed states → one `LifecycleState` per factory method

## Stages

**Stage 0 — explore. ✅ DONE 2026-06-06.** Findings, final enums, final tables, call-site map, and reader remap are above. Answers to the questions this stage was opened for: VenueHire = FlatFee shape (setup session at apply, charge at accept); `VersusFinishStep` = DoorSplit + guarantee (no winner-selection state exists yet); `CreateStandardAsync` calls `AwaitPayment`, `CreateDeferredAsync` leaves `Pending` (both die with `BookingStatus`); `Application.Accept` is called from `ApplicationAcceptedDomainEventHandler`; `Withdraw` is dead but kept as a future feature.

**Stage 1 — the machine. ✅ DONE 2026-06-06 (expression revised same day; revised AGAIN later that day — see "Expression revision 2 + step/state divorce" below, which supersedes the family-shape derivation described here).** `LifecycleState`, `Trigger`, `ContractStateMachine` in `Concert.Domain/Lifecycle/`. Final expression after several rejected shapes (full catalogue in `plans/STATE_MACHINE_DESIGN_REVIEW.md`): `ContractStateMachine` is a pure mechanism (ctor takes the table; `Next`; no statics, no `ContractType`); the tables are DERIVED in `ConcertWorkflowBuilder.Build()` from the existing fluent chains — each `With*` captures its step's `static LifecycleState State` declaration, and `Build()` instantiates one of two named family shapes (`EscrowLifecycle` / `DeferredLifecycle`, family = `WithVerify` presence) parameterized by those states. Machines are registered via a registration-built `ConcertStateMachineRegistry` singleton (`IConcertStateMachineRegistry.Get(type)`, mirrors `ConcertWorkflowCapabilityRegistry`; no keyed services). Table-driven unit tests (16) resolve the registry from the real `AddConcertWorkflows()` registration and derive every expectation from `Transitions`.

**Stage 2 — write path swap. ✅ DONE 2026-06-06.** **Machine swap ONLY — the surrounding architecture is preserved verbatim** (user ruling after a wrong wider redesign was reverted): workflows ×4, keyed factory, capability registry, capability interfaces (`IAcceptsPaid`/`IAcceptsSimple`/`IAppliesPaid`/`IAppliesSimple`/`IVerifies`/checkout), split paid/simple step interfaces, dispatchers, executors all stay. What changed:
- `LifecycleTransitioner` replaces `WorkflowStateMachine<TEntity>` (+ `ConcertTransitionValidator`(+Factory), `ILifecycleEntity`, `ILifecycleRepository<>` deleted); executors keep their exact bodies but fire `Trigger.X` through the transitioner instead of `ConcertStage.X` through the stage guard.
- `IConcertStep` keeps the static per-step state declaration, typed `LifecycleState` (uniform defaults on the interfaces: apply→`Applied`, accept→`Accepted`, verify→`Booked`, checkout→`Applied`/`Accepted`; divergent ones per class: `DraftSettleStep`→`Booked`, `NoOpSettleStep`→`Complete`, FlatFee/VenueHire finish→`Complete`, DoorSplit/Versus finish→`AwaitingSettlement`).
- `ApplicationEntity`: internal `State` column (private set, `TransitionTo` written only by the transitioner) + `Accept(booking)` kept as the domain verb (called in the accept effect); `Booking.Confirm(concert)` kept. Status-only guards deleted on all three entities.
- The two chained domain-event handlers collapsed into owning effects: sibling bulk-reject → accept effect (still backgrounded, set-based `State == Applied → Rejected`); draft creation → `DraftSettleStep` (escrow contracts' settle step); `NoOpSettleStep` is now the deferred contracts' settle (settlement success = state write only).
- Processors: identical inbox shape; map `Metadata["type"]` → trigger-named dispatcher/facade methods; `ConflictException` propagates (only inbox dup-key is caught). Facade slims to `VerifySucceededAsync`/`EscrowSucceededAsync`/`SettlementSucceededAsync`/`FinishAsync`; failure processors call dispatchers directly (mirroring how they bypassed the facade before).
- Finish gains the temporal guard in `FinishExecutor` (period-ended check, ex-`BookingEntity.Complete()`).

**Stage 3 — read path. ✅ DONE 2026-06-06.** As planned: `GetEndedConfirmedIdsAsync` → `state == Booked && Period.Start < now`; `CanPost` → `state == Booked` (+ `GetByIdWithBookingAsync` includes Application); opportunity "accepted-side" filters → `state ∉ {Applied, Rejected, Withdrawn}`; dashboard counts → `state == Applied`; `ApplicationValidator` re-accept guard removed (subsumed); wire `ApplicationStatus` derived in `ApplicationMapper` (enum re-homed to `Concert.Application/DTOs`).

**Stage 4 — schema + seeds. ✅ DONE 2026-06-06.** `./initial-migrations.ps1` re-scaffolded (Applications has `State`; no `Status`/`CurrentStage` anywhere in [concert]). `BookingFactory` → data-only (`Standard`/`Deferred`); `ApplicationFactory` → state-named (`Accepted`/`Booked`/`Complete` + prepaid variants), booking attached via `Accept(booking)`.

**Stage 5 — green. ✅ DONE 2026-06-06/07.** Concert module integration tests updated to assert `LifecycleState` (incl. NEW first-class assert: verify-failure lands `PaymentFailed` + notify); E2E `ConcertFinishedTests` poll `Applications.State` via new `ApplicationDb` fixture (IVT to `Concertable.B2B.E2ETests`). Gates run against the final expression-revision-2 tree: B2B integration suite 100/100 (Concert 56/56 incl. processor split), `e2e-ui-regress` 30/30 (B2B 23/23, Customer 7/7). All stages complete; remaining = commit.

## Expression revision 2 + step/state divorce (2026-06-06, supersedes the Stage 1 expression and parts of Stage 2)

The family-shape derivation died on review: shape inference from `WithVerify` presence and the named-argument role-rebinding (`WithSettle` = "booked" in escrow but "complete" in deferred) made the chain vocabulary lie. Replaced wholesale:

- **Stateless evaluated end-to-end and ultimately NOT adopted — the original "ceremony" rejection stands, now with the full ledger.** The package was implemented in two forms during this revision (authoring-layer compiled to the table at boot via `GetInfo()`; full idiomatic per-transition bound instances) and both were rejected by the owner: the boot compile needs extraction contortions (`GetInfo`/probing a throwaway machine), and the idiomatic form constructs a machine per transition, which violates the hard ruling "the machines are prebuilt at startup". Since effects must stay outside the machine anyway (the transitioner's effect-before-assignment contract is incompatible with `OnEntry` running after state mutation), the package's runtime had no job here — so it owns nothing.
- **Final expression: builder verbs declare their rows directly.** Each fixed-fragment verb calls a private `Add(from, on, to)` (`TryAdd` + throw = the duplicate-trigger collision detection Stateless used to provide); `Build()` = `WithWorkflow` guard + `new ContractStateMachine(transitions)`. `ContractStateMachine` stays the original pure Domain mechanism (ctor takes the dictionary; `Transitions` + `Next` + `ConflictException` 409). No reachability check in `Build()` — the `Transitions_ShouldReachEveryDeclaredState_FromApplied` theory owns that invariant against the real registration. Zero package references; tables prebuilt at boot; runtime = one frozen-dictionary lookup.
- **Entity guards its own invariant (double dispatch, owner's design).** `ApplicationEntity.TransitionTo(next)` — a dumb setter any module-internal code could abuse — is replaced by `internal void Transition(Trigger trigger, ContractStateMachine machine) => State = machine.Next(State, trigger);`. The transitioner still validates pre-effect (`machine.Next(app.State, trigger)` peek, so illegal duplicates 409 before non-rollbackable effects like Stripe calls), then hands assignment to the entity: load → validate → effect → `app.Transition(trigger, machine)` → save. `ContractStateMachine` is a Domain type, so this is Domain-consuming-Domain — no layering violation.
- **One executor per payment leg.** `ISettleExecutor`/`SettleExecutor` (which straddled the escrow and settlement legs, forcing the open `ExecuteFailedAsync(bookingId, Trigger)` passthrough) split into `EscrowExecutor` (escrow success → `Book`, escrow failure) and `SettlementExecutor` (effect-less success/failure advances); the matching dispatcher split `IEscrowDispatcher` + slimmed `ISettlementDispatcher` (`SucceededAsync`/`FailedAsync` each). `Trigger` no longer appears on any executor's public surface — each leg executor hardcodes its own triggers, mirroring `VerifyExecutor`.
- **One failure processor per webhook type.** `BookingPaymentFailedProcessor` (straddled `type=Escrow` + `type=Settlement` with a routing ternary) split into `EscrowPaymentFailedProcessor` + `SettlementPaymentFailedProcessor`, mirroring the success side — every payment processor is now the same branchless shape: filter own type → inbox dedup → one dispatcher call.
- **Family shapes deleted; each builder verb owns one fixed transition fragment.** `WithApply` = the `Applied` rows; `WithEscrowPayment()` / `WithVerifiedPayment()` / `WithSettlement()` = parameterless pure-topology verbs (escrow/verify/settlement legs incl. failure + retry rows); `WithFinish<TStep>(to)` = the one genuinely variant edge, explicit in the chain (`Complete` escrow, `AwaitingSettlement` deferred). No shape inference anywhere; `Build()` validates reachability from `Applied`.
- **Step/state divorce.** `static LifecycleState State` deleted from `IConcertStep`, every interface DIM, and every step class — steps are pure effects; topology lives only in the builder fragments.
- **Booked-moment taxonomy.** The old slots were named after webhook executors, not lifecycle moments, hiding that `DraftSettleStep` (escrow) and `DeferredVerifyStep` (deferred) performed the same action — create the draft concert when the booking-securing payment lands. Both replaced by `IBookStep` + one `CreateConcertDraftStep` (`IConcertWorkflow.Settle` → `Book`). `NoOpSettleStep` deleted: deferred settlement success is an effect-less transition (`TransitionAsync` with null effect). `ISettleExecutor.ExecuteAsync(bookingId, trigger)` split into `ExecuteEscrowAsync` (runs `Book`) / `ExecuteSettlementAsync` (no effect). `IVerifies` deleted — the machine's rows already gate verify triggers per contract type (the executor's capability guard ran after `Next` validation and was unreachable).

## Sequencing / branch

REVISED 2026-06-06: implementation happens directly on `refactor/Microservices` (user call — the branch is still red anyway: venue-hire E2E scenario mid-debug, 403 seeder task open, uncommitted work in these files), no separate branch. The builder hardening is partially superseded by Stage 2; fine — it fixed a live fragility either way.

## Open decisions — RESOLVED at Stage 0 review (2026-06-06)

1. State lives on `ApplicationEntity` as engagement root (no separate engagement table).
2. Wire: derive `ApplicationStatus` for the SPA (mapping in the reader remap above); expose raw `LifecycleState` later if wanted.
3. Naming: draft names kept — `Booked`, per-leg failure states (`EscrowFailed`/`SettlementFailed`), `Complete`. Rich FSM: Withdraw rows stay despite being currently untriggerable; failure states get retry/recovery rows.

## Verification

- Stage 1: table unit tests (every declared row advances; every undeclared (state, trigger) throws).
- Stage 2/3: full B2B integration suite — the per-contract Application*/Concert* API tests exercise every accept/verify/settle/finish path end-to-end, including the duplicate-webhook tests (`Accept_ShouldIgnoreDuplicateWebhookEvent`).
- Final: `e2e-ui-regress`.
