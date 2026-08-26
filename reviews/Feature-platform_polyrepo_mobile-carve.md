# Code review — Feature/platform_polyrepo_mobile-carve

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `e64245e5192ccbccb30a5fd54d687ca05170c321`  _(2026-08-07)_

> Range reviewed: `59bdd7a8a..e64245e51` (18 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C#
conventions, and test coverage of changed paths.

The runtime diff is limited to the `carve-fe` matrix: both added surface keys already exist in
`carve-fe.mjs`, the workflow self-triggers `run_fe` when `test.yml` changes, and `ci-complete` already
requires the matrix job. PR #416 proved both new entries on the feed-restored path with successful
`tsc --noEmit` and Android `expo export` jobs. The removed tech-debt file contained only the entry whose
explicit resolution gate was those two green jobs. No `api/**` paths changed, so the backend isolation,
module, seeding, and C# lenses are not applicable to this diff.
