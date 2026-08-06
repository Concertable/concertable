# Code review — Refactor/launch_money-value-type

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `0ba389c06740ad08f99158531879a639d35ebb2f`  _(2026-08-05)_

> Range reviewed: `origin/main..0ba389c06` (Phase 4 + Phase 5 publisher; the merge-in of origin/main carries no reviewable change).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.

Notes considered and dropped (all behavior-preserving, sub-threshold):
- `TicketService.cs:130` `Money.Gbp(x).ToMinorUnits()` vs `(long)(x*100)` — AwayFromZero vs truncation is a no-op for 2dp GBP × integer quantity (the product is always integral); path covered by existing Customer Ticket unit tests.
- Payment.Client adapters — param `decimal`→`Money` + `amount.ToProtoMoney()` emit the identical proto `Money` message; runtime unchanged (consumers still call via the old pinned package). gRPC round-trip covered by integration/E2E.
- `StripeFixture` (E2E helper, not production) — same rounding no-op.
- Deleted orphaned public `Client/EscrowDto` — dead code (zero consumers), no coverage impact.
