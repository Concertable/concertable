# Code review — Feature/PaymentLedger

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `e66b91356ae51f86d7b903731aad3a05ad3972a2`  _(2026-07-29)_

> Range reviewed: `73a5fd8a..d02c825c` (6 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [ ] **VERIFY1 — BLOCKED — final verification** — `api/Concertable.slnx`
  BUG1, BUG2, BUG3, and CV1 were fixed in commits `d4c8ad7c`, `74082d4c`, `2f28b17b`, and `1e97eeee`; Payment unit tests pass 75/75. The required final full-solution build reached `Concertable.Customer.E2ETests.Mobile` and failed only because C: ran out of disk while copying Aspire resource assemblies. Free sufficient disk space, rerun `dotnet build api/Concertable.slnx --no-restore`, and delete this untracked review file if it passes.

## Incremental review — 2026-07-29

> Range reviewed: `d02c825c..e66b9135` (5 commits).

- [x] **BUG4 — HIGH — correctness** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/LedgerService.cs:50`
  Restore safe concurrent account creation. Two postings can both observe a missing account and stage the same `(Type, OwnerId, Currency)` row; the loser now receives an unhandled unique-index `DbUpdateException`. `IUnitOfWork.ExecuteAsync` does not classify duplicate-key violations as transient or retry them, so replacing the reconcile loop with transaction rollback regresses synchronous escrow calls after Stripe has already moved money. Handle the collision by reconciling to the winning account or retrying the complete posting with a fresh change tracker, and cover concurrent first use with a real-database test.
  Fixed by reconciling staged entries to the winning accounts at the Payment UoW save boundary and retrying inside the same transaction. Added a two-context SQL concurrency test.

- [x] **BUG5 — HIGH — correctness** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/EscrowService.cs:238`
  Post the amount actually refunded after release. `RefundAsync` accepts a partial `amount`, but this branch always reverses the escrow's full gross and platform fee, so a £10 refund of a released £50 escrow writes a £50 ledger reversal. Derive the payable/revenue reversal from `refundAmount` and add partial-refund tests for released escrow with zero and non-zero fees.
  Fixed by reversing the payee transfer up to the original transferred gross and reversing platform revenue only for the remainder. Stripe now receives the payee reversal separately from the total customer refund.

- [x] **TEST1 — MEDIUM — test coverage** — `api/Concertable.Payment/tests/Concertable.Payment.UnitTests/Infrastructure/FakeUnitOfWork.cs:12`
  Add a real-`PaymentDbContext` transaction test for the UoW migration. `FakeUnitOfWork.ExecuteAsync` only invokes the delegate and never saves or rolls back, so tests such as `CompleteAsync_LedgerStagingFails_RetryCommitsStateAndPostingTogether` do not verify their stated atomicity claim. Assert that the operational status and ledger rows commit together and both remain unchanged when staging or saving fails.
  Fixed with SQL-backed UoW tests covering joint commit, staging failure, and save failure through fresh verification contexts.
