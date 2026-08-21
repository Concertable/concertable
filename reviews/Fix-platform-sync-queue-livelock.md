# Code review — Fix/platform-sync-queue-livelock

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `43c29d85c` _(2026-08-21)_
**Security-reviewed up to commit:** `43c29d85c` _(2026-08-21)_

> Range reviewed: `4a478433a..43c29d85c` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No findings — checked correctness, error handling, and security.

**Correctness:** The bug this fixes was confirmed with hard evidence, not inference — `gh run view
32516484915 --log` for the platform-sync run at 2026-08-21T19:03:35Z shows `Closing superseded sync
PR #706` immediately after PR #706 had already passed every required check (including `ci-complete`)
and was sitting in the merge queue at position 2. The fix's `isInMergeQueue` GraphQL query was tested
directly against real PR numbers (`gh api graphql -f query='query($n:Int!){...isInMergeQueue}' -F
n=706` and `n=648`, both correctly returning `false` post-close/post-merge) before being committed,
confirming the `-F` int-variable coercion and field name are correct. YAML validated with
`python -c "import yaml; yaml.safe_load(...)"`.

**Behavior when skipped:** the comment block correctly identifies the consequence of leaving a queued
PR alone (main briefly pins a version slightly behind latest) and why it's safe (the cascade guard
prevents that PR's own merge from re-triggering a sync, and the next non-sync `api/**` merge opens a
fresh sync for whatever's newest by then regardless) — matches the existing cascade-guard mechanism
already in this file, no new invariant introduced.

**Security (this path is always-flagged: `^\.github/workflows/`):** The added `gh api graphql` call is
read-only (fetches `isInMergeQueue`, mutates nothing) and uses the same `GH_TOKEN`
(`PLATFORM_SYNC_TOKEN`) this step already uses for `gh pr list`/`gh pr close` — no new secret, no
privilege change. `$n` is passed as a properly typed GraphQL variable (`-F n="$n"`), not
string-interpolated into the query text, so there is no GraphQL/command injection surface; `$n`
itself originates only from `gh pr list --jq '...| .number'`, i.e. a numeric PR id GitHub itself
returned, never external/untrusted input.
