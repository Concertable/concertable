# Code review — Feature/launch_dashboard-frontend-package-expand

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `9f3d47417bc40bc6355564b0225e3be290bdcde6`  _(2026-08-15)_

> Range reviewed: `d64ac4a5b..60742981e` (4 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — MEDIUM — native / test coverage** — `app/web/b2b/shared/src/features/concerts/api/actionLinkApi.ts:5`
  The new action executor and download flow have no tests, and `@concertable/b2b` has no test script. Add focused coverage for API-prefix normalization, method forwarding, blob download, and object-URL cleanup, and run it from the package build.

## Incremental review — 2026-08-15

Range reviewed: `60742981e..9f3d47417` (4 commits). No new findings. The only runtime delta is the current
platform package pin merge from `origin/main`; the reviewed frontend source diff is unchanged.
