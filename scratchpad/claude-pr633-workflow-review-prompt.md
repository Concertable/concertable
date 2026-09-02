# Incremental review requested: module-local Deal lifecycle workflows on PR #633

You are reviewing the `Refactor/launch_deal-lifecycle-modules-phase2` branch for PR #633 in this
worktree:

`C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal-lifecycle-modules-phase2`

This is an incremental review. Review only the committed range:

`12273b558..bf4407181`

Do not edit files. Inspect the real diff and surrounding code rather than relying only on this prompt.
Ignore unrelated pre-existing issues outside the range.

## Problem the change is solving

All four Deal types participate in each lifecycle stage, but Deal-varying behavior has two different
invocation shapes.

- Homogeneous operations share one interface and method signature. They use
  `IDealStrategyFactory<TStrategy>`.
- Heterogeneous operations have several honest method-header interfaces. Each DealType maps to exactly
  one interface for that operation, while multiple DealTypes may map to the same interface. They use
  `IDealUnionFactory<TUnion>`, and consumers match by interface capability rather than by DealType.

Application Apply and Accept are heterogeneous. Booking Confirm and Cancel and Concert Cancel and
Complete are currently homogeneous.

The application also needs one cohesive place per module for lifecycle orchestration without bloating
the existing module services or introducing one single-method DI object per operation. The approved
shape is one executable, module-local workflow per aggregate stage:

- `IApplicationWorkflow`: Apply and Accept
- `IBookingWorkflow`: Confirm, Cancel, and correlated financial outcome handling
- `IConcertWorkflow`: Cancel and Complete

These are not a cross-module process object, shared state machine, ambient context, or dependency bag.
Each workflow owns orchestration for one module only. Aggregate state remains on that module's aggregate.

## Intended call paths

```text
HTTP API -> module service -> module workflow
Application accepted domain event -> BookingWorkflow.ConfirmAsync
Concert completion background runner -> scoped ConcertWorkflow.CompleteAsync
```

The module service remains the normal HTTP facade. Domain-event and background entry points may invoke
the workflow directly because they are already internal lifecycle triggers.

## Intended Deal dispatch

Application retains heterogeneous union dispatch:

- Apply maps FlatFee, DoorSplit, and Versus to `IApplyStandard`; VenueHire to `IApplyPrepaid`.
- Accept maps FlatFee and VenueHire to `IAccept`; DoorSplit and Versus to `IAcceptPaid`.
- `PrepaidAccept` is an implementation of the broad `IAccept` interface because it has the same method
  signature as `StandardAccept`.

Booking and Concert retain homogeneous strategy dispatch through operation-named interfaces:

- Booking: `IConfirm`, `ICancel`
- Concert: `ICancel`, `IComplete`

The old `*Executor` and `*Step` wrappers are intentionally removed. Selected interfaces live under the
owning Application project's `Strategies/<Operation>` folder; implementations live under the owning
Infrastructure project's matching folder. Workflow interfaces live in `Interfaces`; implementations live
in `Services`.

## Important Booking ownership change

Before this commit, Deal-specific confirmation implementations called public
`IBookingService.CreateStandardAsync` or `CreateDeferredAsync`. Those methods were not API-level module
operations; they existed so confirmation implementations could call back into the service orchestrating
them.

`BookingWorkflow` now owns uniform Booking and Contract creation privately, then invokes the selected
`IConfirm` only for the Deal-specific financial effect. The two creation methods therefore leave
`IBookingService`. Confirm that this preserves transaction, outbox, persistence-order, ID-generation, and
handoff behavior without introducing a DI cycle.

## Accept data-loading decision

The workflow loads Application, Opportunity, Deal, Artist, and Venue once at the final orchestration site.
It performs shared eligibility and terms-fingerprint checks there, constructs server-owned signature data,
then passes the values every selected Accept implementation actually consumes. There is deliberately no
ambient `IAcceptContext`, accessor, loader, or early context parameter threaded through unrelated callers.

Confirm that the move removed the duplicate Opportunity read without changing validation order, typed
errors, transaction behavior, notifications, or accepted-application construction.

## What to inspect hardest

1. Behavior changes hidden by moving code between services, workflows, and selected implementations.
2. DI cycles, incorrect lifetimes, or scoped dependencies escaping their intended scope.
3. Unit-of-work and outbox nesting/order changes, especially Booking confirmation and cancellation.
4. Booking/Contract creation order and whether concrete `AcceptedApplication` shapes are handled safely.
5. Exact DealType coverage and whether every selected interface still implements `IDealStrategy`.
6. Nullable-input regressions or interfaces weakened to accept data only some implementations need.
7. Module-boundary violations or backwards command/control flow.
8. Whether deleting the old mock-heavy executor tests removed unique behavioral coverage not present at
   integration or public boundaries.
9. Whether the service/workflow entry-point split is consistent across Application, Booking, and Concert.
10. Any stale registration, caller, namespace, or documentation reference left by the rename.

Relevant roots:

- `api/Concertable.B2B/src/Modules/Application`
- `api/Concertable.B2B/src/Modules/Booking`
- `api/Concertable.B2B/src/Modules/Concert`
- `api/Concertable.B2B/src/Modules/Deal`
- `api/Concertable.B2B/src/Concertable.B2B.KeyedStrategies`
- `api/Concertable.B2B/src/Concertable.B2B.Infrastructure`

The owning design documents are:

- `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PLAN.md`
- `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md`
- `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PLAN.md`

## Existing validation evidence

- Application, Booking, and Concert Infrastructure builds: 0 warnings, 0 errors.
- Unit tests: Application 20/20, Booking 8/8, Concert 96/96, Deal 47/47.
- Architecture tests: 21/23; both failures are in unchanged Venue fixture-boundary and Application unit-test
  package-ownership paths.
- A full Concert integration diagnostic was stopped after 38 passes because five unchanged status/race
  tests produced nearly 50 MB of captured logs. The moved Cancel and Complete bodies match the base commit;
  do not assume that statement is correct without checking the diff.

## Required output

Return only actionable findings, ordered by severity. For each finding include:

- severity;
- file and line;
- the concrete correctness or architecture problem;
- why it is caused by this incremental range;
- the smallest durable fix.

If there are no actionable findings, say `No findings.`
