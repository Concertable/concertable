# Code review - Feature/PaymentOwnedResultExpansion

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed - don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `df76074c46fb7e55570049abad377323bac3114b`  _(2026-08-07)_

> Range reviewed: `3e3bcce89..a4ae0081e` (63 commits).
> Status legend: `[ ]` todo - `[~]` in progress - `[x]` done - `[wontfix]` (note why).

## Findings

- [x] **BUG1 - HIGH - correctness** - `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/EscrowService.cs:114`
  Bound deposit, capture, and settlement retries return an existing row before revalidating the supplied payer identity, intent, and reviewed gross. Move `CalculateBoundAsync` ahead of each idempotent-return branch and add retry regressions so a mismatched retry cannot report success.

  Fixed in the review-fix commit: all three retry paths authorize first, and focused regressions prove mismatched gross cannot reach Stripe or return the existing operation.

- [x] **BUG2 - MEDIUM - correctness** - `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/EscrowService.cs:388`
  An already-refunded booking returns `Option.Some` even though the established idempotent contract defines an already-completed operation as the successful no-op `Option.None`. Return `None` and update the regression.

  Fixed in the review-fix commit: the already-refunded branch now returns `Option.None` and its regression asserts the no-op contract.

- [x] **BUG3 - MEDIUM - correctness** - `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/ManagerPaymentService.cs:272`
  Settlement refunds expose `EscrowRefundError`, publishing `escrow.refund_*` codes for a non-escrow operation. Give the internal settlement-refund operation its own error contract and `settlement.refund_*` definitions.

  Fixed in the review-fix commit: the internal operation now returns `SettlementRefundError` with exact `settlement.refund_*` definition coverage.

- [x] **CI1 - HIGH - test coverage** - `api/Concertable.Shared/tests/Concertable.Shared.Api.UnitTests/TypedResultArchitectureTests.cs:23`
  The branch's architecture guard is stale against the completed Payment slice and the adopted explicit Dunet-case convention: its self-verifying allowlist already contains a clean file, while two guards require the superseded generated-`Match`/factory shape. Remove the stale entry and align the guards with the current convention so the architecture suite passes.

  Fixed in the review-fix commit: the stale slice entry and constructor ban are removed, and the definition guard accepts the repository's legacy generated-`Match` and current abstract-member shapes. The architecture suite passes 16/16.

- [x] **TEST1 - MEDIUM - test coverage** - `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Repositories/CommissionBindingRepository.cs:36`
  The first-write-wins reviewed-gross update is only represented by a mocked boolean in unit tests. Add a SQL integration regression proving one value wins and a different subsequent value cannot overwrite it.

  Fixed in the review-fix commit: a SQL integration regression confirms the first persisted gross remains and a different second value is rejected.

- [x] **CV1 - LOW - C# conventions** - `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/CommissionService.cs:101`
  Replace the new `is { }` capture with the repository-standard explicit null check required by `api/agents/CODE_CONVENTIONS.md`.

  Fixed in the review-fix commit with an explicit nullable `Money` check.

## Incremental review - 2026-08-05

> Range reviewed: `a4ae0081e..d0fe18afe` (1 commit).

No issues found. Checked the six finding fixes for correctness, microservice isolation, module
boundaries, seeding, C# conventions, and test coverage of changed paths.

The docs-only delivery follow-up through `ee8dbdd57` was also checked; no issues found.

## Incremental review - 2026-08-07

> Range reviewed: `ee8dbdd57..842b9c332` (96 commits).

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C#
conventions, and test coverage of changed paths. The range includes the current-main merges; the
branch-owned Payment error-convention and conflict-resolution changes were reviewed in full.

## Incremental review - 2026-08-07 (final current-main merge)

> Range reviewed: `842b9c332..df76074c4` (6 commits).

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C#
conventions, and test coverage of changed paths. The only non-ledger changes are the already-landed
plan-handoff session-scoping fix and its focused regressions.
