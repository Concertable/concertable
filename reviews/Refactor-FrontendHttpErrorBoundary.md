# Code review — Refactor/FrontendHttpErrorBoundary

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `a5571245e20de750ac83064b69cf6aecb0948661`  _(2026-08-08)_

> Range reviewed: `66ef2c7d..d8aa5950` (4 commits), plus the 2026-08-08 pre-merge pass below.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **TEST1 — MEDIUM — test coverage** — `app/shared/package.json:18`
  The interceptor tests are not part of any package lifecycle or CI job, so the new behavior can regress while every gate remains green. Run the shared test suite from the package's `prebuild` lifecycle.
- [x] **TEST2 — MEDIUM — package correctness** — `app/shared/tsconfig.build.json:10`
  The package build emits `client.test.*` into `dist`, so the published tarball includes test code and the default Vitest scan reruns the emitted copy. Exclude test files from declaration/build output and scope the test script to `src`.

## Incremental review — 2026-08-06

No issues found in the interceptor request-config typing change. The remaining commits in the range are
merges from `origin/main` and were not authored by this branch.

## Pre-merge review — 2026-08-08

- [x] **NAT3 — MEDIUM — type-safety** — `app/shared/src/lib/apiClient.ts`
  `notFoundAsNull` was declared via ambient module augmentation on `AxiosRequestConfig`, so it was settable on any `.get()`/`.post()` call on any axios instance, not just through `getOptional` — a type-unsound path where `data` was typed `T` but could resolve `null` at runtime. Replaced the interceptor-flag mechanism with `getOptional` catching its own 404 directly; no global config flag, no module augmentation.
- [x] **NAT4 — MEDIUM — correctness** — `app/shared/src/lib/client.ts`
  The 404-to-null handling was registered only inside `withAuth()`, so a client configured without `.withAuth()` (or an unconfigured `createApiClient()` instance) silently lost `getOptional`'s null-on-404 contract. Fixed by the same change as NAT3 — `getOptional` no longer depends on any interceptor being registered.
- [wontfix] **NAT1 — dead `ApiError` export** / **NAT2 — duplicate `ProblemDetails`** — both already resolved by the next commit in the stack (`refactor: migrate frontend HTTP error consumers`), which wires `ApiError` into the interceptor's reject path and re-exports `ProblemDetails` from `apiError.ts` instead of redeclaring it. Intentional publish-first split, not a gap in this PR.
- [x] **NAT5 — LOW — test coverage** — `app/shared/src/lib/client.test.ts`
  Added a case exercising the plain pass-through (non-404/401) rejection path.

## Incremental review — 2026-08-08

No issues found. The post-watermark branch change was the prior review stamp, and the merge from
`origin/main` resolved without branch-authored runtime changes. Checked correctness, microservice
isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.

## Incremental review — 2026-08-08 (current-main refresh)

No issues found. The additional merge from `origin/main` updated the platform package pin and API
guidance without conflict or branch-authored runtime resolution. Checked correctness, microservice
isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.

## Incremental review — 2026-08-08 (review-tooling refresh)

No issues found. The additional merge from `origin/main` updated only review tooling and resolved
without branch-authored changes. Checked correctness, microservice isolation, module boundaries,
seeding, C# conventions, and test coverage of changed paths.
