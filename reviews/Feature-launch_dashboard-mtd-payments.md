# Code review — Feature/launch_dashboard-mtd-payments

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `c044ee247fb85b339f5a2d59180dfe0aafe5b0ba`  _(2026-08-13)_

**Security-reviewed up to commit:** `c044ee247fb85b339f5a2d59180dfe0aafe5b0ba`  _(2026-08-13)_

> Range reviewed: `2e6e0cc78..c044ee247` (4 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **SEC1 — MEDIUM — native/security** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Grpc/ManagerPaymentRequestMappers.cs:109`
  Convert malformed protobuf timestamps to `InvalidArgument`; `Timestamp.ToDateTime()` otherwise throws an unhandled `InvalidOperationException` at the gRPC boundary.

## Incremental review — 2026-08-13

No issues found in the fixing commit. Checked correctness, security, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.
