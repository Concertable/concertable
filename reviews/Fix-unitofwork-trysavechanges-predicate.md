# Code review — Fix/unitofwork-trysavechanges-predicate

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `8a04d4b4ca4dc1c132f12b4f3e9e3c34f8936728`  `(2026-08-31)`
**Security-reviewed up to commit:** `8a04d4b4ca4dc1c132f12b4f3e9e3c34f8936728`  `(2026-08-31)`
**Judgment:** `approved`

## Review pass — 2026-08-31 — full

**Candidate base:** `eda7300e9974676c5f99585a26644ac6b2c1074e`
**Candidate head:** `8a04d4b4ca4dc1c132f12b4f3e9e3c34f8936728`
**Candidate branch:** `Fix/unitofwork-trysavechanges-predicate`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:d7b89111a269711aeca0890363ce7e812aa8aa9fc8d8126cfd582589ce477996` `(15 paths)`
**Work-order path:** `reviews/Fix-unitofwork-trysavechanges-predicate.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

### Findings

None. `/security-review` over the full base..head diff (touches `Concertable.Payment`, a
security_paths match): no injection surface in the new `GetOrCreateAsync` upsert (strongly-typed
expression trees, no string interpolation), no auth/authz change, no new sensitive-data exposure; the
validate-before-upsert reordering closes a TOCTOU-shaped gap rather than opening one. Native review (independent `code-reviewer` dispatch over `c5b1934f0..8a04d4b4c`, the newest
commit) found no correctness, duplication, simplification, efficiency, or error-handling defects.
Persistence, module-structure, multitenancy, unit-testing, integration-testing, domain-events and
result-errors routes were loaded and checked against the changed paths; nothing in the diff violates
them (Payment has no tenant stance, so multitenancy is a non-issue here; the `TrySaveChangesAsync`
predicate is per-call-site as `dotnet-standards:result-errors`/DI conventions expect).

Parent synthesis over the full 3-commit range (`11210c787`, `c5b1934f0`, `8a04d4b4c`):
- `TrySaveChangesAsync` now requires an explicit `Func<DbUpdateException, bool>` instead of swallowing
  every `DbUpdateException`; every remaining caller (this branch and the `launch_deal-lifecycle-modules-phase2`
  consumer branch) passes a specific, correct predicate — no caller relies on a "catch everything" default.
- `DomainEventDispatchInterceptor.SaveChangesFailedAsync` now pops `pendingEventsStack` on failure,
  balancing the push in `SavingChangesAsync`; covered by a new dedicated test file.
- The ledger-account race reconciliation (`UnitOfWork.SaveChangesWithAccountReconciliationAsync` /
  `ReconcileConcurrentAccountsAsync`) is deleted in favor of `LedgerAccountRepository` resolving accounts
  through `DbSetExtensions.GetOrCreateAsync`, an atomic SQL UPSERT.
- That upsert executes immediately, outside `SaveChanges`, so it cannot be rolled back by a later
  in-memory failure. The final commit closes the resulting gap: `LedgerTransactionEntity.Post`'s balance
  rules are extracted into `ValidatePosting` and called from `LedgerService.StageAsync` before any account
  is resolved, so a malformed posting is rejected before the eager upsert side effect. Verified by
  `UnitOfWorkTransactionTests.SaveChangesAsync_WhenLedgerStagingFails_LeavesOperationalStateAndLedgerRowsUnchanged`.
- `PaymentSessionReconciliationService.SaveAsync`'s `TrySaveChangesAsync` predicate is corrected from
  `IsDuplicateKey()` to `exception is DbUpdateConcurrencyException` — concurrent reconciliation of the
  same attempt row (a `RowVersion`-tracked update, not an insert) raises a concurrency conflict, not a
  duplicate key.

Verified locally against the full local-platform build (`scripts/local-platform.ps1`, matching CI's
`local-platform-pack` mechanism): `Concertable.DataAccess.UnitTests` 22/22,
`Concertable.Payment.UnitTests` 568/568, `Concertable.Payment.IntegrationTests` 49/49 — including the
four tests that failed on the prior pushed head (`UnitOfWorkTransactionTests.SaveChangesAsync_WhenLedgerStagingFails_LeavesOperationalStateAndLedgerRowsUnchanged`,
`PaymentSessionServiceTests.CreateOrReplayAsync_ConcurrentSameRequest_ConvergesOnOneObject`,
`PaymentSessionServiceTests.RetryAsync_ConcurrentDuplicateRetries_ConvergeAfterCancellationRace`,
`PaymentSessionServiceTests.ReconcileAsync_ConcurrentObservation_ConvergesOnOneAppliedTransition`), now
all passing.
