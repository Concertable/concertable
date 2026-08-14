# Code review - Refactor/DataAccessRepositoryPermissionHierarchy

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed - don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `94d7664ad2751dd64c35064e067756001b15b6cd`  _(2026-08-14)_

> Range reviewed: `429581025..94d7664ad` (2 commits).
> Status legend: `[ ]` todo - `[~]` in progress - `[x]` done - `[wontfix]` (note why).

## Findings

- [x] **BUG1 - HIGH - correctness** - `api/Concertable.DataAccess/Concertable.DataAccess.Infrastructure/ReadDbContext.cs:7`
  The shared `ReadDbContext` now owns the generic configuration-provider/schema behavior. The redundant Customer `ReadDbContext` and B2B `PublicDbContext` intermediaries were removed, and all six concrete module read contexts derive the shared base directly.
