# Lifecycle seal enforcement progress

- Plan: `plans/launch/LIFECYCLE_SEAL_ENFORCEMENT_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/lifecycle-seal-enforcement`
- Worktree: not created
- Branch: not created
- PR: not opened
- Dependency/package gates: Phases 1-2 are producer changes to the published `Concertable.Kernel` and
  `Concertable.DataAccess.Infrastructure` packages and must publish plus platform-sync before Phase 3
  consumes them. Phase 3 additionally requires the per-module state machines to exist on `main`; they
  are currently owned by `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md` and live only on
  `Refactor/launch_deal-lifecycle-modules-phase2`.
- Last reconciled: `2026-09-05` against `origin/main`

## Current state

Design approved by Tommy on 2026-09-05 after establishing that the existing snapshot records freeze only
the facts carried across a module boundary and place no constraint on the source row.

No implementation has started. Phase 1 is implementable today: `Concertable.Kernel.StateMachine` and
`IStateMachine` already exist on `main` at
`api/Concertable.Shared/src/Concertable.Kernel/StateMachines/`, and the terminality primitive needs
nothing from the in-flight module carve.

Phase 3 is sequenced behind the module carve. The per-module state machines
(`Application.Domain.Lifecycle.ApplicationStateMachine` and its Booking and Concert siblings) do **not**
exist on `main` — verified with `git ls-tree -r --name-only main`. They exist only on the carve branch,
which is itself suspended behind the Deal dispatch foundation. Phases 1-2 do not wait for any of that.

## Next Steps

Create a fresh worktree from current `origin/main` and implement Phase 1 in `Concertable.Kernel`:

1. Add a `FrozenSet<TState>` of transition sources to the `StateMachine<TState, TTrigger>` constructor,
   built from the same `transitions` sequence that already populates the `FrozenDictionary`.
2. Add `bool IsTerminal(TState state)` to `IStateMachine<TState, TTrigger>` and implement it on
   `StateMachine` as `!sources.Contains(state)`.
3. Extend `Concertable.Kernel.UnitTests` with the four cases in the plan's Phase 1 gate: a terminal
   state, a non-terminal state, a self-loop source (non-terminal), and a target-only state (terminal).
4. Continue into Phase 2 in the same worktree — `ISealable` in Kernel, `SealingInterceptor` and
   `SealedEntityException` in `Concertable.DataAccess.Infrastructure` beside `AuditInterceptor` — since
   both phases publish together.

Do not begin Phase 3 in this worktree; it consumes the published packages and the carved modules.

## Completed work

- Established that `ApplicationAcceptanceSnapshot`, `ContractSnapshot`, and `ConfirmedBooking` constrain
  the carrier only, and that no layer prevents an update to a superseded row.
- Computed the terminal-state sets from the three live transition tables and confirmed they coincide
  exactly with the stage handoffs, so one rule covers both readings of the invariant.
- Obtained Tommy's decision for a single stored `IsSealed` boolean over a derived flag plus a nullable
  `SealedAt` timestamp.
- Scoped Opportunity out with its prerequisite named rather than forcing it into the same mechanism.

## Verification

- `git ls-tree -r --name-only main` confirms `Concertable.Kernel` `StateMachine`/`IStateMachine` are on
  `main` and the per-module lifecycle state machines are not.
- `IAuditable` is implemented only by `EscrowEntity` and `TransactionEntity`; none of the three
  lifecycle entities are auditable, so no existing column records sealing time.
- `ExecuteUpdateAsync` call sites that will interact with Phase 4:
  `Concertable.Payment` `EscrowRepository`/`TransactionRepository`/`CommissionBindingRepository`,
  `ConcertApiFixture.cs:107`/`112`, `TenantVerificationGateApiTests.cs:31`.
- No `HasTrigger` call exists anywhere in `api/`, so Phase 4's policy approach introduces no EF Core
  trigger-batching penalty.

## Reviews

- None recorded; no implementation commit exists yet.

## Decisions, discoveries, blockers, and deviations

- Terminality is derived from the transition table, never declared beside it. A state is terminal iff it
  never appears as a source.
- One stored fact only. A derived `IsSealed` alongside a nullable `SealedAt` was rejected because the two
  diverge silently once a terminal state gains an outgoing edge.
- The interceptor must read `entry.OriginalValues`, not `entry.Entity`. The sealing save itself carries
  `IsSealed = true` on the entity while the database still holds `false`.
- The Phase 4 proof test asserts on the row, not the exception type, because SQL Server raises a
  block-predicate error where Postgres filters the row out and surfaces a concurrency exception.
- `BookingState.Confirmed` already has no outgoing transition; post-confirmation cancellation is modelled
  on Concert. The modules already express the invariant this plan enforces.
- Building this on SQL Server rather than waiting for Postgres was chosen deliberately: SQL Server's
  block predicate is the stronger of the two for this specific guarantee, and only the per-table SQL
  helpers are provider-specific.
- `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md` is owned by the
  `Refactor/launch_deal-lifecycle-modules-phase2` worktree and was deliberately not edited from the
  normal checkout. Its owner should add a `## Downstream handoffs` entry naming this ledger at its next
  material checkpoint, so that whoever lands the module carve knows Phase 3 here unlocks.
