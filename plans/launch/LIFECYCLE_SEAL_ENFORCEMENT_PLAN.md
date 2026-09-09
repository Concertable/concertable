# Lifecycle seal enforcement

> **Next steps live in @plans/launch/LIFECYCLE_SEAL_ENFORCEMENT_PROGRESS.md → `## Next Steps`.**

## 1. Approved decision

A lifecycle row becomes immutable **in the database** once its owning state machine has no outgoing
transition from its current state. Superseded stages stop being editable rows and become the
historical facts the next stage was built from.

Enforcement is layered, and each layer catches what the one above it cannot:

```text
Transition sets IsSealed        the single writer of the fact
SealingInterceptor              rejects every tracked SaveChanges write to a sealed row
database write-block            rejects ExecuteUpdate, raw SQL, and anything outside the ORM
SetAfterSaveBehavior(Throw)     per-column edits that are illegal at any state
```

## 2. Why the existing snapshots are not this

`ApplicationAcceptanceSnapshot`, `ContractSnapshot`, and `ConfirmedBooking` are frozen records that
carry facts **forward across a module boundary**. They constrain the carrier. They place no constraint
on the source row: after `Accept`, the `Applications` row is still writable through a tracked entity,
through `ExecuteUpdateAsync`, or through any SQL client.

`DEAL_LIFECYCLE_OWNERSHIP_PLAN.md` already states the invariant in prose — "Acceptance is its
successful terminal decision; later financial or operational outcomes do not rewrite that fact", and
"each persisted identity records facts about itself and stops transitioning when authority moves
forward". Nothing enforces it below the transition table. This plan supplies the enforcement; it does
not restate or reopen that ownership design.

## 3. The rule — terminality is derived, never declared twice

A state is terminal **iff it never appears as a transition source**. That is already fully determined
by each module's transition table, so it is computed from the table rather than asserted beside it.
Adding an edge out of a state automatically un-seals it; no parallel list can drift.

Computed from the current tables:

| Module | Terminal states |
|---|---|
| Application | `Accepted`, `Rejected`, `Withdrawn`, `Cancelled` |
| Booking | `Confirmed`, `Cancelled` |
| Concert | `Complete`, `Cancelled` |

These coincide exactly with the stage handoffs: `Accepted` is Application→Booking and `Confirmed` is
Booking→Concert. "Seal at handoff" and "seal at terminal state" select the same rows, so only one rule
is needed. `BookingState.Confirmed` already has no outgoing edge — post-confirmation cancellation is
modelled on Concert — so the design this plan enforces is the one the modules already express.

## 4. One stored fact

```csharp
public interface ISealable
{
    bool IsSealed { get; }
}
```

Written at exactly one site, inside each module's `Transition`, from `IsTerminal(next)`.

**Rejected: a nullable `SealedAt` timestamp beside a computed `IsSealed`.** Two representations of one
fact. They diverge in the window between the transition and the stamping save, and diverge permanently
if a terminal state later gains an outgoing edge — silently, because nothing compares them. If sealing
time is wanted, adopt `IAuditable` (already implemented by `EscrowEntity` and `TransactionEntity`, and
by none of the three lifecycle entities) rather than a bespoke column. `Transition` holds no
`TimeProvider`, and threading one through every transition to obtain a timestamp is not worth its cost.

## 5. The interceptor reads original values, not the entity

The save that *seals* a row is itself a `Modified` entry whose entity already carries `IsSealed = true`
while the database still holds `false`. Only the database's view may block, so the guard reads
`entry.OriginalValues`. Reading `entry.Entity` would reject the sealing write itself.

## 6. Opportunity is deliberately out of scope

`OpportunityEntity` cannot join this scheme yet, and forcing it in would be dishonest:

- it has an `OpportunityState` enum but **no state machine** — `MarkFilled` / `Withdraw` / `Reopen`
  guard themselves with inline `if`s;
- `Reopen` moves `Filled → Open`, so `Filled` is not terminal; only `Withdrawn` is;
- `TenantId` and `VenueId` are public setters and `Update()` mutates at any state.

Sealing buys nothing until the Opportunity has a real transition table and private setters. That is
its own item, not a phase here. Sealing an Opportunity on first acceptance is separately rejected in
§9: one Opportunity has many Applications, and freezing it would block the venue from editing an
opening whose other applications are still pending. `TermsFingerprint` already invalidates a stale
acceptance.

## 7. Phases

### Phase 1 — Kernel terminality primitive (producer)

- [ ] Build a `FrozenSet<TState>` of transition sources in the `StateMachine<TState, TTrigger>`
  constructor alongside the existing `FrozenDictionary`.
- [ ] Add `IsTerminal` to `IStateMachine<TState, TTrigger>` and implement it on `StateMachine`.

Consumption contract: `bool IsTerminal(TState state)` on `IStateMachine<TState, TTrigger>`, returning
`true` iff no `(state, *)` key exists in the transition table. Purely additive; no existing member
changes shape. `ConfiguredStateMachine` inherits it unchanged.

Gate: `Concertable.Kernel.UnitTests` cover a terminal state, a non-terminal state, a self-loop source
(non-terminal), and a state that appears only as a target (terminal). `Concertable.Kernel` packs.

### Phase 2 — DataAccess sealing seam (producer, same publication)

- [ ] Add `ISealable` to `Concertable.Kernel` beside `IAuditable`.
- [ ] Add `SealingInterceptor` to `Concertable.DataAccess.Infrastructure` beside `AuditInterceptor`,
  and `SealedEntityException` carrying the entity type and key.

Consumption contract: a `SaveChangesInterceptor` registered **before** `AuditInterceptor`, which throws
`SealedEntityException` for any `Modified` or `Deleted` entry of an `ISealable` whose
`OriginalValues[nameof(ISealable.IsSealed)]` is `true`. It stamps nothing and needs no `TimeProvider`.

Gate: `Concertable.DataAccess.UnitTests` prove the save that seals a row is allowed, a later update to
a sealed row is rejected, a delete of a sealed row is rejected, an unsealed row is unaffected, and a
non-`ISealable` entity is untouched. Both packages publish; the generated platform sync merges before
Phase 3 begins.

### Phase 3 — B2B adoption (consumer; sequenced behind the module carve)

- [ ] `ApplicationEntity`, `BookingEntity`, and `ConcertEntity` implement `ISealable` with a private
  setter, and each module's `Transition` sets `IsSealed = stateMachine.IsTerminal(next)`.
- [ ] Apply `SetAfterSaveBehavior(PropertySaveBehavior.Throw)` to the columns that are illegal to
  change at any state: `OpportunityId`, `AcceptanceOperationId`, `TermsFingerprint`, `DealType`.
- [ ] Re-scaffold initial migrations from `api/` via `./initial-migrations.ps1` — never an additive
  migration — and backfill `IsSealed` for rows already in a terminal state.
- [ ] Register `SealingInterceptor` on the Application, Booking, and Concert contexts.

Gate: one integration test per module — accept then attempt an update, confirm then attempt an update,
complete then attempt an update — each asserting `SealedEntityException` and an unchanged row.

### Phase 4 — database write-block

- [ ] Add a SQL Server security policy per sealed table: a schema-bound inline TVF plus `BEFORE UPDATE`
  and `BEFORE DELETE` block predicates on `IsSealed = 0`, applied through `migrationBuilder.Sql` from
  one helper per table so the eventual Postgres rewrite is a known, enumerated list.
- [ ] Exempt the integration-reset principal so Respawn can still clear sealed rows, and confirm the
  seeders' `SET IDENTITY_INSERT` paths are unaffected.
- [ ] Fix the fixtures this breaks: `ConcertApiFixture.cs:107`/`112` and
  `TenantVerificationGateApiTests.cs:31` mutate concerts through `ExecuteUpdateAsync`.

Consumption contract: no application code calls this layer. It is a backstop for writes that never
reach the change tracker, and its observable contract is that the row does not change.

Gate: an integration test that seals a row, issues a raw `ExecuteUpdateAsync` against it, and asserts
the row is unchanged. **Assert on the row, not on the exception type** — SQL Server raises a
block-predicate error while Postgres filters the row out and surfaces a concurrency exception, so only
a row-level assertion survives the provider move.

### Phase 5 — the drift test

- [ ] For each sealable table, assert no row has `IsSealed = false` while its `State` is terminal
  according to the live state machine.

This is the pairing that keeps a single stored boolean honest, replacing the rejected second property.
It catches a missed backfill and any row whose state moved outside the change tracker.

Gate: the test runs in the owning module's integration suite and fails loudly on a seeded violation.

## 8. Definition of done

- Terminality is read from the transition table in every layer; no hand-maintained list of terminal
  states exists in C#, in SQL, or in a migration.
- Exactly one stored fact records sealing, written at exactly one site.
- A tracked write to a sealed row fails with `SealedEntityException` before reaching the database.
- A write that bypasses the change tracker leaves the row unchanged.
- The Postgres-specific surface of this work is confined to the per-table SQL helpers from Phase 4.
- Opportunity is documented as out of scope with its prerequisite named, not silently skipped.

## 9. Rejected directions

- a nullable `SealedAt` beside a derived `IsSealed`, or any second representation of the same fact;
- a hand-maintained list or attribute of terminal states parallel to the transition table;
- expressing the database predicate as `State NOT IN (...)`, which copies the transition table into a
  migration that never re-runs and lets C# and SQL disagree with SQL winning;
- temporal tables, system-versioning, or `rowversion` as immutability — they are history and
  concurrency respectively, and leave the row writable;
- an entity-level `Freeze()` called by application services, an enforcement every caller can forget;
- table-wide `REVOKE UPDATE`, which cannot be row-conditional;
- a `CHECK` constraint, which cannot see the pre-update row;
- sealing the Opportunity when its first Application is accepted;
- deferring the whole plan until the Postgres migration, which would leave every row written in the
  meantime unsealed and add a backfill that is not otherwise needed.
