# Code review — Refactor/launch_deal-lifecycle-modules

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `1457a2508db5b69d5a0fa7f05eea78ba412edd76`  _(2026-08-17)_

> Range reviewed: `40cd20957..2cc20d1be` (6 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CV1 — LOW — integration-test convention** — `api/Concertable.B2B/tests/Concertable.B2B.IntegrationTests.Fixtures/ApiFixture.cs:182`
  Dispatch the payment-failure handlers through `IScoped<IEnumerable<IIntegrationEventHandler<PaymentFailedEvent>>>.RunAsync`; `INTEGRATION_CONVENTIONS.md` requires that scope-root abstraction for handler collections instead of a hand-written service scope.

No other issues found. Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.

## Incremental review — 2026-08-17

Range reviewed: `2cc20d1be..1457a2508` (2 commits).

- [x] **NAT1 — LOW — native** — `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md:113`
  Removed a stale statement that the integration matrix was pending after the same ledger recorded exact-head CI green.

No other issues found. Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.
