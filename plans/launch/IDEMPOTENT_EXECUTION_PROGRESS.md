# Operation claims and attempt classification progress

- Plan: `plans/launch/IDEMPOTENT_EXECUTION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/operation-claims-and-attempts`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_operation-claims-and-attempts`
- Branch: `Refactor/launch_operation-claims-and-attempts`, stacked on
  `Refactor/launch_deal-lifecycle-modules-phase2` at `55fd099d9`
- PR: none yet — opens against the #633 branch as its base, and GitHub retargets it to the default
  branch when #633 merges
- Dependency/package gates: **no package gate**. Both phases are B2B-internal by design (plan §4) — the types land
  in `Concertable.B2B.DataAccess.Application`/`.Infrastructure`, which every module consumes by
  `ProjectReference`. No `Concertable.*` published contract changes, so no producer PR and no
  `chore/platform-sync-*`. **Delivery gate:** PR #633 must merge before this stack can.
- Last reconciled: 2026-09-06 — plan authored from the design investigation; stacked worktree created;
  Phase 1 implementation starting.

## Current state

**Phase 1 (the operation claim) is implemented and its gates are green. Phase 2 has not started.**

Five claims across three aggregates now share one composed `OperationClaim`
(`Concertable.B2B.DataAccess.Application`), mapped as an EF owned entity type. The decisive result:
`dotnet ef migrations has-pending-model-changes` reports **"No changes have been made to the model since
the last migration"** for all three of `ApplicationDbContext`, `BookingDbContext` and `ConcertDbContext`.
That is the whole mapping hypothesis validated against the real model rather than a probe — same columns,
same unique filtered indexes, same index names, and Booking's non-nullable `OperationId` preserved via
`OwnsRequiredClaim`'s `IsRequired()`.

Encapsulation is stronger than the shape first drafted: `OperationClaim.Claim` is **`internal`**, with
`InternalsVisibleTo` for the three module Domain assemblies and the DataAccess unit tests, so only an
owning aggregate can take a claim — behind whatever transition gates it — while `OperationId` and
`IsHeldBy` stay public so the mapped path remains queryable. An earlier draft exposed a publicly mutable
claim, which would have let any caller bypass `BeginCancellation`.

The capability interface is `IHasCancellationClaim`, with `WithCancellationClaim(operationId)` binding to
it. Both duplicated correlation queries — Booking's and Concert's `FinancialOperationOutcomeProcessor` —
now go through it.

## Next Steps

**State: implementable, delivery-gated.** The stack's base carries the code, so implementation proceeds
now; only the merge waits on PR #633. Do **not** push to #633 itself — it is out of draft in the merge
queue, and a push there resets its review watermark and re-runs its full E2E tier.

Phase 1 is committed. The current action is **Phase 2 (plan §6)**, which needs no further design:

1. Add `AttemptVerdict<TOutcome>` to `Concertable.B2B.DataAccess.Application` as nested abstract records
   (`Settled` / `Transient` / `Recoverable` / `Unrecoverable`).
2. Add the two `AttemptAsync` extension members to `Concertable.B2B.DataAccess.Infrastructure` — no budget
   parameter on `IUnitOfWorkBehavior<TContext>`, a budget on `IUnitOfWorkBoundary<TContext>`.
3. Add `AcceptApplicationError.Contended(int ApplicationId)`, code `application.accept.contended`.
4. Migrate the six scope-backed classifiers, then `SettlementService.ReserveAsync` at budget 2.
5. Delete `AcceptOnceAsync`, `IScoped<ApplicationWorkflow>` and the concrete
   `AddScoped<ApplicationWorkflow>()` registration.
6. Update `Accept_WhenPaymentVerificationWinsTheRace_StillConfirmsTheBooking` to expect 409
   `application.accept.contended` then a successful re-POST.

Phase 1 and Phase 2 are independent, so a red Phase 2 never invalidates Phase 1.

## Completed work

- 2026-09-06 — design investigation, plan and ledger. No source changed.
- 2026-09-06 — **Phase 1: the operation claim.** `OperationClaim` + `IHasCancellationClaim` +
  `OwnsClaim`/`OwnsRequiredClaim`/`WithCancellationClaim`; `ApplicationEntity`, `BookingEntity` and
  `ConcertEntity` migrated with their three EF configurations; ten consumers migrated
  (`ApplicationWorkflow`, `BookingWorkflow.Matches`, `BookingRepository`, `BookingService` x2, the two
  confirm steps, both outcome processors, `SettlementService`); the `api/Concertable.B2B/TECH_DEBT.md`
  operation-claim entry **deleted**; `OperationClaimTests` added.

## Verification

Phase 1, all local:

- **`has-pending-model-changes`: "No changes" on `ApplicationDbContext`, `BookingDbContext` and
  `ConcertDbContext`.** The authoritative no-migration check, and the one that would have caught a wrong
  column name, a wrong index name or the missing `IsRequired()`.
- Unit suites green: `DataAccess.UnitTests` 14/14 (11 of them the new `OperationClaimTests`),
  `Application.UnitTests` 20/20, `Booking.UnitTests` 9/9, `Concert.UnitTests` 105/105.
- Module builds green with 0 warnings: Application, Booking and Concert Infrastructure, plus
  `Concertable.B2B.Web`.

Integration and E2E tiers belong to exact-head CI (no local E2E). Phase 2's gate is plan §6.

## Reviews

No review yet — no implementation exists to review.

## Decisions, discoveries, blockers, and deviations

- **`Claim()` must resume, not re-mint.** `OperationId ?? Claim(Guid.NewGuid())`. The first draft minted
  a fresh id and then hit the rival check, which throws — that breaks `BeginSettlement`'s documented
  reuse and its unit test `BeginSettlement_WhenPreviousAttemptFailed_ReusesTheOperation`. Found by
  probing, before any production code existed.
- **A latent double-refund bug is fixed as a side effect.** Both `BeginCancellation` methods do
  `= Guid.NewGuid()` unconditionally while `BeginSettlement` does `??=`, and
  `CancellationFailed → BeginCancellation` is a legal edge in both state machines. Payment's
  `EscrowService.RefundByReferenceCoreAsync` resumes a `Pending` refund and replays a `Completed` one
  keyed on that id, with a unique index on `PaymentRefundEntity.OperationId` — it is built for reuse, so
  a fresh id on cancel-retry starts a second refund against the same escrow. Name it in the commit
  message; it is a fix, not a refactor.
- **The retry taxonomy is four-way, and the plumbing owns the budget** — not an argument at seven call
  sites. `IUnitOfWorkBehavior` is scope-backed so it can only ever attempt once; `IUnitOfWorkBoundary` is
  factory-backed so it may declare a budget. This is what prevents re-collapsing "transient" into
  "recoverable", which is what produced the deleted accept rerun.
- **`Transient` ships with no production producer, deliberately.** Deadlocks and timeouts are unhandled
  500s today — `IsXConflict` matches only `DbUpdateConcurrencyException`/duplicate-key and no context
  enables `EnableRetryOnFailure`. Giving it a producer needs the unit of work off the DI scope (upstream
  S2). Unit tests cover it; do not "wire it up" by routing a business race into it.
- **`SettlementService.ReserveAsync` is a seventh copy the original brief missed**, and it nests
  `TryExecuteAsync` inside its own classifier. It is correct for it to retry in place — its caller is a
  settlement trigger with no user to re-ask — so it is the case that proves the budget belongs to the
  boundary. Budget 2 reproduces its behaviour exactly.
- **No frontend change.** The B2B SPA does nothing code-specific with these errors: mutations have no
  retry configured at all, `shouldRetry` never retries 409, nothing branches on an error code, and the
  client's `ProblemDetails` type does not even declare `code`. The error *message* is the whole client
  contract, which is why the recoverable branch needs its own case rather than reusing `Superseded`.
- **Only one integration test pins the behaviour being changed** —
  `Accept_WhenPaymentVerificationWinsTheRace_StillConfirmsTheBooking`. `ArmOnce` arms a single conflict,
  so it becomes a 409 followed by a successful re-POST with `ForcedConflicts` still 1.
