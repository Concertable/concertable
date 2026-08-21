# Code review — Fix/platform-sync-queue-livelock

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `PENDING` _(2026-08-21)_
**Security-reviewed up to commit:** `PENDING` _(2026-08-21)_

> Range reviewed: `4a478433a..PENDING`.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **SIMP1 — MEDIUM — simplification** — `.github/workflows/platform-sync.yml`
  First pass used a raw `gh api graphql` call with an inline minified query to read
  `isInMergeQueue` — the only raw GraphQL call in a file that otherwise uses `gh pr` throughout, and
  flagged (correctly) as looking bolted-on rather than native to the file. `mergeStateStatus == CLEAN`
  via the already-idiomatic `gh pr view --json mergeStateStatus` is a strictly simpler, equally
  correct proxy: a `CLEAN` PR (validated, no conflicts) is exactly the state that precedes
  entering/being in the merge queue, so protecting on it covers the same case with no new call shape
  introduced. Fixed.

No further findings — checked correctness, error handling, and security.

**Correctness:** The bug this fixes was confirmed with hard evidence, not inference — `gh run view
32516484915 --log` for the platform-sync run at 2026-08-21T19:03:35Z shows `Closing superseded sync
PR #706` immediately after PR #706 had already passed every required check (including `ci-complete`)
and was sitting in the merge queue at position 2. The `CLEAN` literal was directly observed multiple
times earlier this session via `gh pr view --json mergeStateStatus` (e.g. PR #648 while fully
validated and queued), not assumed. YAML validated with
`python -c "import yaml; yaml.safe_load(...)"`.

**Behavior when skipped:** leaving a `CLEAN` PR alone means main briefly pins a version slightly
behind latest — safe, because the existing cascade guard prevents that PR's own merge from
re-triggering a sync, and the next non-sync `api/**` merge opens a fresh sync for whatever's newest
by then regardless. No new invariant introduced.

**Security (this path is always-flagged: `^\.github/workflows/`):** The added `gh pr view` call is
read-only, uses the same `GH_TOKEN` (`PLATFORM_SYNC_TOKEN`) this step already uses for `gh pr
list`/`gh pr close` — no new secret, no privilege change, no injection surface (PR number is a
plain positional argument, not interpolated into any query text).
