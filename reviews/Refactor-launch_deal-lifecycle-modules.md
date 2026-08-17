# Code review — Refactor/launch_deal-lifecycle-modules

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `2cc20d1be8bc4e7755d5cc4894f11c159dabd6c7`  _(2026-08-17)_

> Range reviewed: `40cd20957..2cc20d1be` (6 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CV1 — LOW — integration-test convention** — `api/Concertable.B2B/tests/Concertable.B2B.IntegrationTests.Fixtures/ApiFixture.cs:182`
  Dispatch the payment-failure handlers through `IScoped<IEnumerable<IIntegrationEventHandler<PaymentFailedEvent>>>.RunAsync`; `INTEGRATION_CONVENTIONS.md` requires that scope-root abstraction for handler collections instead of a hand-written service scope.

No other issues found. Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.
