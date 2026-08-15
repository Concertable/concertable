# Code review — Feature/launch_dashboard-frontend-package-expand

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `60742981eea63d4741369be4aaad83189f05144c`  _(2026-08-15)_

> Range reviewed: `d64ac4a5b..60742981e` (4 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — MEDIUM — native / test coverage** — `app/web/b2b/shared/src/features/concerts/api/actionLinkApi.ts:5`
  The new action executor and download flow have no tests, and `@concertable/b2b` has no test script. Add focused coverage for API-prefix normalization, method forwarding, blob download, and object-URL cleanup, and run it from the package build.
