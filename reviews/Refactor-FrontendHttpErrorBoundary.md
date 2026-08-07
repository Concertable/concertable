# Code review — Refactor/FrontendHttpErrorBoundary

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `339dcafa88db831dff41343959e781b13b28fb06`  _(2026-08-06)_

> Range reviewed: `66ef2c7d..d8aa5950` (4 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **TEST1 — MEDIUM — test coverage** — `app/shared/package.json:18`
  The interceptor tests are not part of any package lifecycle or CI job, so the new behavior can regress while every gate remains green. Run the shared test suite from the package's `prebuild` lifecycle.
- [x] **TEST2 — MEDIUM — package correctness** — `app/shared/tsconfig.build.json:10`
  The package build emits `client.test.*` into `dist`, so the published tarball includes test code and the default Vitest scan reruns the emitted copy. Exclude test files from declaration/build output and scope the test script to `src`.

## Incremental review — 2026-08-06

No issues found in the interceptor request-config typing change. The remaining commits in the range are
merges from `origin/main` and were not authored by this branch.
