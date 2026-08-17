# Code review — Refactor/RepositoryPermissionContraction

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `21cd0aba85ccac98924415b0e54f5645bbc829e8`  _(2026-08-17)_
**Security-reviewed up to commit:** `21cd0aba85ccac98924415b0e54f5645bbc829e8`  _(2026-08-17)_

> Range reviewed: `92ea04166..21cd0aba8` (3 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked correctness, security-sensitive Payment changes, microservice isolation,
module boundaries, seeding, C# conventions, and test coverage of changed paths.

## Incremental review — 2026-08-17

- [x] **CV1 — MEDIUM — convention accuracy** — `api/agents/CODE_CONVENTIONS.md:56`
  Update the repository convention section in this contraction: it still instructs module aliases to
  inherit the deleted `WriteRepository<TEntity, TContext>` and `Repository<TEntity, TContext, TKey>`
  arities and tells specialized repositories to use the removed inherited concrete `context`. Phase 3
  explicitly requires the final DataAccess/B2B hierarchy guidance to land with the contraction.
