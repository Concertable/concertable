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

## Final enums

```csharp
internal enum LifecycleState
{
    Applied, Rejected, Withdrawn,
    AwaitingVerification, VerificationFailed,   // deferred contracts (DoorSplit/Versus)
    AwaitingEscrow, EscrowFailed,               // escrow contracts (FlatFee/VenueHire)
    Booked,                                     // payment confirmed, draft created — CanPost gate
    AwaitingSettlement, SettlementFailed,       // deferred payout leg
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

## Final per-contract transition tables

FlatFee:

```csharp
[(Applied, Accept)]                          = AwaitingEscrow,   // FlatFeeAcceptStep: standard booking + capture held escrow; accept effects (sibling bulk-reject, notify)
[(Applied, Reject)]                          = Rejected,
[(Applied, Withdraw)]                        = Withdrawn,
[(AwaitingEscrow, EscrowPaymentSucceeded)]   = Booked,           // concert draft + notify (collapses NoOpSettleStep + BookingSettledDomainEventHandler chain)
[(AwaitingEscrow, EscrowPaymentFailed)]      = EscrowFailed,
[(EscrowFailed, EscrowPaymentSucceeded)]     = Booked,           // late-success recovery
[(Booked, Finish)]                           = Complete,         // FlatFeeFinishStep: synchronous escrow release (no webhook back-edge — release is a Transfer)
```

VenueHire — identical table; effects differ (Accept = off-session `DepositAsync` with the card saved at apply; Finish releases the hire fee).

DoorSplit / Versus — identical tables; Finish payout math differs (DoorSplit: `revenue × pct`; Versus: `guarantee + revenue × pct`):

```csharp
[(Applied, Accept)]                                    = AwaitingVerification, // PaidAcceptStep: deferred booking + store paymentMethodId
[(Applied, Reject)]                                    = Rejected,
[(Applied, Withdraw)]                                  = Withdrawn,
[(AwaitingVerification, VerifyPaymentSucceeded)]       = Booked,               // DeferredVerifyStep: concert draft + notify
[(AwaitingVerification, VerifyPaymentFailed)]          = VerificationFailed,   // notify venue manager
[(VerificationFailed, VerifyPaymentSucceeded)]         = Booked,               // retry lands
[(VerificationFailed, VerifyPaymentFailed)]            = VerificationFailed,   // re-notify
[(Booked, Finish)]                                     = AwaitingSettlement,   // Finish step: compute payout, initiate off-session payout
[(AwaitingSettlement, SettlementPaymentSucceeded)]     = Complete,
[(AwaitingSettlement, SettlementPaymentFailed)]        = SettlementFailed,
[(SettlementFailed, SettlementPaymentSucceeded)]       = Complete,             // late-success recovery
```

Notes:
- The tables genuinely differ per contract (FlatFee finish lands directly on `Complete`; deferred contracts get `AwaitingSettlement`) — this is why triggers, not target-states, key the table: shared executors stay contract-agnostic.
- Concert *posting* stays the orthogonal `DatePosted` flag (content publication, not lifecycle); `ConcertValidator.CanPost` gate becomes `state == Booked && DatePosted is null`.

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

**Stage 1 — the machine.** `LifecycleState`, `Trigger`, `ContractStateMachine`, four tables, ctor-injected map. Unit tests on the tables (pure function — table-driven tests, derive expectations from the table per `feedback_derive_test_expectations_not_literals`).

**Stage 2 — write path swap.** `LifecycleState` column on `ApplicationEntity`; a transitioner (load application → `Next(state, trigger)` → run effect → write state → save) replaces `WorkflowStateMachine<TEntity>`; executors fire triggers; payment processors map metadata string → trigger at the edge; reject/withdraw/fail go THROUGH the machine (bulk reject becomes a state write to `Rejected` — still set-based, but legality owned by the table); delete entity guards + `AdvanceStage`. Internal domain-event indirections that exist only to chain transitions (`BookingSettledDomainEvent` → draft creation, `ApplicationAcceptedDomainEvent` → `Application.Accept` + sibling bulk reject) collapse into the owning step.

**Stage 3 — read path.** Repos/validators/mappers to state: `OpportunityRepository` (Pending → `Applied`), `ConcertDashboardRepository`, `ConcertRepository.GetEndedConfirmedIdsAsync` (→ `state == Booked && period ended`), `ConcertValidator.CanPost`, `ApplicationMapper`. Wire shape: keep `ApplicationStatus` on the DTO as a *derived* mapping from `LifecycleState` (no SPA churn now; expose raw state later if wanted).

**Stage 4 — schema + seeds.** Drop dead columns/enums, `./initial-migrations.ps1` re-scaffold (project convention — no additive migrations). Seed factories set single states; delete the Status/Stage pairing.

**Stage 5 — green.** Concert module integration tests (56) updated to assert `LifecycleState`; full B2B integration suite; `e2e-ui-regress` as the merge gate.

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
