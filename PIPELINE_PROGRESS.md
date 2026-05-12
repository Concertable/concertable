# Concert Pipeline Refactor — Progress

Branch: `Refactor/ConcertPipeline`

Reference: `CONCERT_WORKFLOW_PATTERN.md` (design doc) and `~/.claude/plans/joyful-jumping-fairy.md` (implementation plan).

## Locked decisions

- Composable step pipeline (each stage variant is its own class)
- `ConcertLifecycleEntity` as workflow correlation spine
- Per-pipeline self-contained state machines, sequence-based (`ConcertStage[]`)
- Four integration events on stage advance, sourced via a single `StageAdvancedDomainEvent` + fan-out handler
- Big-bang refactor on one branch
- `LifecycleId` on `ApplicationEntity` only (Booking + Concert navigate via chain)
- `ConcertEntity.Id` stays IDENTITY (no `ValueGeneratedNever` trick)
- Lifecycle resolved by natural key `(opportunityId, artistId)` — no FE contract change for VenueHire's two-phase flow

## Completed

### Gate 1 — Domain primitives
- `Concert.Domain/Enums/ConcertStage.cs` — `None, Applied, CheckedOut, Verified, Accepted, Settled, Finished`
- `Concert.Domain/Entities/ConcertLifecycleEntity.cs` — `IIdEntity + IEventRaiser`, `Create(opportunityId, artistId)`, `AdvanceTo(stage)`
- `Concert.Domain/Events/StageAdvancedDomainEvent.cs`
- `Concert.Contracts/Events/Concert{ApplicationCreated,ApplicationAccepted,Settled,Finished}Event.cs`

### Gate 2 — Step interfaces (Concert.Application/Workflow/Steps/)
- `IConcertStep` — marker with `static abstract ConcertStage Stage { get; }`
- 9 step interfaces, each providing the target stage via `static ConcertStage IConcertStep.Stage => ConcertStage.X;`
- Signatures: `ISimpleApplyStep`, `IPaidApplyStep`, `IAcceptCheckoutStep`, `IApplyCheckoutStep`, `ISimpleAcceptStep`, `IPaidAcceptStep`, `IVerifyStep`, `ISettleStep`, `IFinishStep`

### Gate 3 — State machines
- `Concert.Application/Workflow/IConcertStateMachine.cs` — `GuardAsync<TStep>`, `AdvanceAsync<TStep>` (constrained to `IConcertStep`)
- `Concert.Application/Workflow/IConcertStateMachineFactory.cs`
- `Concert.Application/Workflow/IConcertPipelineFactory.cs` — `Create<TStep>(ContractType)`
- `Concert.Application/Workflow/IConcertPipelineRegistry.cs` — `Has<TStep>(ContractType)`
- `Concert.Application/Interfaces/IConcertLifecycleRepository.cs`
- `Concert.Infrastructure/Services/Workflow/StateMachines/ConcertStateMachine.cs` — abstract base, derives From from `Sequence[index(TStep.Stage) - 1]`
- 3 impls: `HeldStateMachine`, `DeferredStateMachine`, `ApplyCommittedStateMachine` (each declares its `ConcertStage[] Sequence`)
- `Shared.Domain/Exceptions/ConflictException.cs`

### Gate 4 — Lifecycle data model
- `ApplicationEntity.LifecycleId` + `SetLifecycleId(int)` method
- `ConcertLifecycleEntityConfiguration` — table `ConcertLifecycles`, indexed on `(OpportunityId, ArtistId)`
- `ApplicationEntityConfiguration` updated — unique index on `LifecycleId`
- `ConcertDbContext` gains `DbSet<ConcertLifecycleEntity> ConcertLifecycles`
- `ConcertConfigurationProvider` registers `ConcertLifecycleEntityConfiguration`
- `ConcertLifecycleRepository` — implements lookup methods `GetIdByOpportunityIdAndArtistId/ByApplicationId/ByBookingId/ByConcertIdAsync`

### Gate 5 — Step implementations (18 classes in `Concert.Infrastructure/Services/Workflow/Steps/`)
- Apply (2): `SimpleApplyStep` (shared by FlatFee/DoorSplit/Versus), `VenueHirePaidApplyStep`
- Checkout (4): `FlatFee/DoorSplit/Versus AcceptCheckoutStep`, `VenueHireApplyCheckoutStep`
- Accept (4): `FlatFeeAcceptStep`, `DoorSplit/Versus AcceptStep` (paid), `VenueHireAcceptStep`
- Verify (1): `DeferredVerifyStep` (shared)
- Settle (3): `HeldSettleStep`, `DeferredSettleStep` (shared), `ApplyCommittedSettleStep`
- Finish (4): one per contract

### Gate 6 — Pipeline plumbing (`Concert.Infrastructure/Services/Workflow/`)
- `ConcertPipelineRegistry` — set-based, `Has<TStep>` uses `HashSet.Contains(typeof(TStep))` (no reflection)
- `ConcertPipelineFactory` — typed keyed-DI lookup
- `ConcertStateMachineFactory` — keyed-DI lookup
- `ConcertPipelineBuilder` — fluent `.WithSimpleApply<T>()`, `.WithStateMachine<T>()`, etc.
- `Concert.Infrastructure/Extensions/PipelineRegistrationExtensions.cs` — `services.AddConcertPipeline(ContractType, configure)`
- `ConcertPipelineExecutor` — single class implements all six dispatcher interfaces (`IApplyDispatcher`, `ICheckoutDispatcher`, `IAcceptanceDispatcher`, `ISettlementDispatcher`, `ICompletionDispatcher`, `IVerifyDispatcher`). Owns lifecycle get-or-create, guard/advance, and application save.

## Remaining

### Gate 7 — DI wiring + domain-event handlers + service refactor
- `Concert.Infrastructure/Extensions/ServiceCollectionExtensions.cs`:
  - Remove `workflowTypes` + `AddConcertWorkflow<T>` helper block (lines ~94–100, 158–167)
  - Replace with four `services.AddConcertPipeline(ContractType.X, p => p.With…)` blocks
  - Register `IConcertLifecycleRepository`, `IConcertPipelineFactory`, `IConcertStateMachineFactory`
  - Register `ConcertPipelineExecutor` once as scoped concrete + six interface bindings to that same scoped instance
  - Register `StageAdvancedDomainEventHandler`
- `Concert.Infrastructure/Events/StageAdvancedDomainEventHandler.cs` — new. `IDomainEventHandler<StageAdvancedDomainEvent>`. Switches on `event.To`, looks up the relevant entity by `lifecycleId`, publishes the matching integration event (`ConcertApplicationCreatedEvent` / `Accepted` / `Settled` / `Finished`).
- `Concert.Infrastructure/Services/ApplicationService.cs` — caller side. Validation still here; messaging/email now run *after* save (executor handles save). `ApplyAsync(opportunityId)` → executor; `AcceptAsync` etc. unchanged from caller's POV (still calls `acceptanceDispatcher.AcceptAsync(...)`, which is now `ConcertPipelineExecutor` under the hood).

### Gate 8 — Atomic delete
**Interfaces** (Concert.Application/Interfaces/):
- `IConcertWorkflowStep`, `IConcertWorkflow`, `IHeldConcertWorkflow`, `IDeferredConcertWorkflow`, `IApplyCommittedConcertWorkflow`
- `IApplyable`, `IAcceptable`, `ICheckoutable`, `ISettleable`, `IFinishable`, `IVerifiable`
- `ISimpleApply`, `IPaidApply`, `ISimpleAccept`, `IPaidAccept`, `IAcceptCheckout`, `IApplyCheckout`
- `IConcertWorkflowFactory`

**Classes** (Concert.Infrastructure/Services/):
- `Workflow/FlatFeeConcertWorkflow`, `DoorSplitConcertWorkflow`, `VersusConcertWorkflow`, `VenueHireConcertWorkflow`
- `Workflow/ConcertWorkflowFactory`
- `Apply/ApplyDispatcher`, `Checkout/CheckoutDispatcher`, `Acceptance/AcceptanceDispatcher`, `Settlement/SettlementDispatcher`, `Completion/CompletionDispatcher`, `Verify/VerifyDispatcher`

**Class** (Concert.Application/Workflow/):
- `ConcertWorkflowCapabilityRegistry`

**Tests** (Concert.UnitTests/Services/Workflow/):
- `FlatFeeConcertWorkflowCompleteTests`, `DoorSplitConcertWorkflowCompleteTests`, `VersusConcertWorkflowCompleteTests`, `VenueHireConcertWorkflowCompleteTests`

### Gate 9 — Migration re-scaffold
- Run `./initial-migrations.ps1` from `api/`
- Confirm `concert.ConcertLifecycles` table created
- Confirm `concert.Applications.LifecycleId` column + unique index

### Gate 10 — Tests
- Update `Apply/AcceptanceDispatcherTests`, `Settlement/SettlementDispatcherTests`, `Completion/CompletionDispatcherTests` — mock `IConcertPipelineFactory` + `IConcertStateMachine`; SUT becomes `ConcertPipelineExecutor`
- Delete 4 `*WorkflowCompleteTests.cs`
- Add `ConcertLifecycleEntityTests`
- Add `Workflow/StateMachines/{Held,Deferred,ApplyCommitted}StateMachineTests`
- Add per-step unit tests (trivial for shared steps; richer for per-contract checkout/accept/finish)
- Add `CheckoutDispatcherTests`, `VerifyDispatcherTests` (filling existing coverage gap)
- Add `StageAdvancedDomainEventHandlerTests`
- Integration tests (`Application{FlatFee,DoorSplit,Versus,VenueHire}ApiTests`, `Ticket{...}ApiTests`) — should pass unchanged once executor is wired

---

## Side note: registration shape

The new design requires registering each step **twice in source** — once when you write the step class (`internal class FlatFeeAcceptStep : ISimpleAcceptStep`) and once in the pipeline builder (`.WithSimpleAccept<FlatFeeAcceptStep>()`). Two places. Forget the second and the step is silently absent at runtime.

The old approach was nicer in this specific way: the class's interface list **was** the registration (`FlatFeeConcertWorkflow : IHeldConcertWorkflow, ISimpleApply, …`), and `ConcertWorkflowCapabilityRegistry.Has<TStep>` discovered capabilities via `IsAssignableTo`. Single source of truth — the class declaration. The cost was runtime reflection per `Has<TStep>` call and the variance of "one class implements many roles."

We lost that pattern not by choice but by structure: now that step impls are shared across contracts (`SimpleApplyStep` by 3, `DeferredSettleStep`/`DeferredVerifyStep` by 2), the class can't be a single contract's capability list anymore — it serves multiple. So the pipeline builder has to be the source of truth for "what does FlatFee support."

**If the duplication ever causes a bug**, the hybrid escape hatch is attribute-based discovery — `[Pipeline(ContractType.FlatFee, ContractType.DoorSplit)]` on the step class, scanner at startup auto-registers into the keyed-DI + registry. Single source of truth (the class), reflection moves to startup-only. Worth considering when pipelines grow or when forgotten registrations actually bite. Until then, the explicit pipeline block is at least readable as documentation ("here's the entire FlatFee pipeline in 6 lines").
