# Code review — Feature/TypedResultMigration

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `44c222faf993acd227e6d7f43d3bf7f7e72195ec`  _(2026-07-30)_

> Range reviewed: `9cd71868..44c222fa` (5 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

### Test coverage

- [x] **TEST1 — HIGH — test coverage** — `api/Concertable.slnx:222`
  Add `api/Concertable.Shared/tests/Concertable.Shared.Api.UnitTests/Concertable.Shared.Api.UnitTests.csproj` to the `unit-tests` matrix in `.github/workflows/test.yml`. The workflow enumerates test projects explicitly, and PR #248's green checks contain no Shared.Api test job, so none of the new Result, ProblemDetails, exception-handler, or architecture tests execute in CI.

- [x] **TEST2 — MEDIUM — test coverage** — `api/Concertable.Shared/src/Concertable.Shared.Api/Exceptions/GlobalExceptionHandler.cs:39`
  Add handler tests for the explicit `UnauthorizedAccessException` → 401 and `DomainException` → 400 compatibility branches, asserting status, title, and safe detail. This phase promises to preserve both legacy mappings, but the new handler tests cover cancellation, validation `BadRequestException`, generic `HttpException`, and 500 behavior without exercising either dedicated branch.
