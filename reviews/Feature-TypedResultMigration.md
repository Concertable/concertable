# Code review — Feature/TypedResultMigration

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `147d1a7020c187a7ad16111eec61ae7999833838`  _(2026-07-30)_

> Range reviewed: `44c222fa..147d1a70` (5 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

## Incremental review — 2026-07-30

- [x] **BUG1 — MEDIUM — correctness** — `api/Concertable.Shared/src/Concertable.Shared.Api/Http/ApplicationProblemDetails.cs:44`
  Replace the unconditional `IProblemDetailsService.WriteAsync` call with `TryWriteAsync` plus a non-throwing fallback that preserves and serializes the selected ProblemDetails response, and cover both Result and exception execution with an unsupported `Accept` header. The new Result execution path now throws `InvalidOperationException` when no registered writer accepts the requested media type (for example `Accept: application/xml`), whereas the previous MVC `ObjectResult` still returned the intended error response.
