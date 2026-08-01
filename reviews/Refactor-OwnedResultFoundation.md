# Code review — Refactor/OwnedResultFoundation

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `aef9314af995ef05e5d8949970af908871da19cd`  _(2026-08-01)_

> Range reviewed: `1d7b3596..aef9314a` (4 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **BUG1 — MEDIUM — Correctness** — `api/Concertable.Shared/src/Concertable.Shared.Api/Results/ErrorHttpExtensions.cs:11`
  Freeze each `ErrorKind`'s HTTP status and title together instead of deriving the title with `HttpStatusCode.ToReasonPhrase()`. The Phase 1 contract says "the frozen mapping explicitly owns both HTTP status and title" so public ProblemDetails titles do not change with framework behavior.

- [ ] **TEST1 — MEDIUM — Test coverage** — `api/Concertable.Shared/tests/Concertable.Shared.Api.UnitTests/TypedResultArchitectureTests.cs:224`
  Extend `TypedResultPattern` to recognize the new one-arity `Result<TError>` as well as `Result<TValue,TError>`. The typed-result rule says failures must not be turned into HTTP exceptions, but after replacing the old `UnitResult<TError>` alternative this guard no longer inspects no-value result slices at all.
