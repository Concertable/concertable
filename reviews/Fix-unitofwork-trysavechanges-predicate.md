# Code review — Fix/unitofwork-trysavechanges-predicate

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `3c701561573d912339c0c337be97e5eb685c0243`  `(2026-08-31)`
**Security-reviewed up to commit:** `3c701561573d912339c0c337be97e5eb685c0243`  `(2026-08-31)`
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

## Review pass — 2026-08-31 — incremental

**Candidate base:** `8a04d4b4ca4dc1c132f12b4f3e9e3c34f8936728`
**Candidate head:** `67d399eaad397fe91c65ae60b6908cf7f743a180`
**Candidate branch:** `Fix/unitofwork-trysavechanges-predicate`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:0a83030184be4641dafcfb311505527bb79e9b6c1789c4d5ab2ae7bef348b3b3` `(7 paths)`
**Work-order path:** `reviews/Fix-unitofwork-trysavechanges-predicate.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

None open. The delta adds one parameterless `TrySaveChangesAsync(CancellationToken)` overload beside the
required-predicate one, delegating to `static _ => true`, plus the matching member on the four other
implementers. `grep -rn ": IUnitOfWork\b\|: IUnitOfWork<"` over `api/` confirms the enumerated set is
complete: `Concertable.Payment.Infrastructure.IUnitOfWork` and
`Concertable.Customer.Ticket.Infrastructure.IUnitOfWork` are bare marker interfaces over
`IUnitOfWork<TContext>` declaring no members, so the base declaration reaches every production
implementation, and the two hand-written test doubles are the only other classes needing the member.

Routed skills re-opened and checked against the changed files (`csharp-style`, `csharp-naming`,
`persistence` both plugins, `dependency-injection`, `module-structure` both plugins, `result-carriers`,
`unit-testing` and `integration-testing` both plugins): the new extension member is declared inside the
existing `extension(DbContext context)` block as `csharp-style` requires, `CancellationToken` is present
on every added async signature, no registration or repository shape changed, and no test-tier rule is
touched. Overload resolution is unambiguous — the predicate overload has no default for `isExpected`, so
a bare `TrySaveChangesAsync()` or `TrySaveChangesAsync(ct)` binds only to the new member.

Security: the frozen delta adds no input path, no query construction, no auth or authz change, no crypto,
and no data exposure; the two changed files under `Concertable.Payment` are a pass-through delegation and
a test double. Assessed by the parent over the frozen delta — the host security-review harness resolved
its diff against the primary checkout rather than this worktree and returned an empty candidate.

Noted, not opened as a finding — `dotnet-standards:result-carriers` bans "a bool or enum that collapses
caller-actionable outcomes", and this overload maps every `DbUpdateException` (concurrency, duplicate key,
FK, check constraint) onto one indistinguishable `false`. It is a tension rather than a violation here:
the `Task<bool>` shape is pre-existing on the predicate overload, and the catch-all has zero production
callers, so there is no caller action being collapsed today. The alternative, if a caller ever appears
and the collapse starts to matter, is to drop the naked overload and expose the catch-all as a named
predicate constant so the breadth stays visible at the call site. The XML summary names
`DbUpdateConcurrencyException` explicitly and steers callers to the predicate overload.

Re-verified on the full local-platform build: `Concertable.DataAccess.UnitTests` 22/22,
`Concertable.Payment.UnitTests` 568/568, `Concertable.Payment.IntegrationTests` 49/49.

## Review pass — 2026-08-31 — incremental (base merge)

**Candidate base:** `67d399eaad397fe91c65ae60b6908cf7f743a180`
**Candidate head:** `3c701561573d912339c0c337be97e5eb685c0243`
**Candidate branch:** `Fix/unitofwork-trysavechanges-predicate`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:71998ecc5a59eb965e70ea59abc25cc7a478457ef9870a7a346083743d62c8f6` `(11 paths)`
**Work-order path:** `reviews/Fix-unitofwork-trysavechanges-predicate.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

None. The range authors no new change: it is the prior pass's review commit plus a merge of `origin/main`,
which the branch was 5 behind after `#893` bumped `ConcertablePlatformVersion` to `0.1.0-alpha.0.1279`.
`git diff-tree --cc` on the merge commit is empty, so every path came through from one side verbatim — no
conflict resolution and no hand edit to attribute to this branch. The incoming paths are the eight
`Directory.Packages.props` pin bumps, `DistributedApplicationBuilderExtensions.cs` and
`ContainerImageResourceTests.cs` from `#892`, and that PR's own review artifact, all reviewed and landed on
their own PRs. `git diff origin/main...HEAD` confirms this branch's own contribution is unchanged at the 16
paths the earlier passes reviewed.

Security: re-stamped rather than re-argued. The only Payment-matching path in this range is
`api/Concertable.Payment/Directory.Packages.props`, a one-line version-pin bump, and this branch's own
security-sensitive surface is byte-identical to what the prior pass cleared.

Re-verified against the merged base on a fresh local platform (`0.1.0-local.1788183172639`):
`Concertable.DataAccess.UnitTests` 22/22, `Concertable.Payment.UnitTests` 568/568,
`Concertable.Payment.IntegrationTests` 49/49.
