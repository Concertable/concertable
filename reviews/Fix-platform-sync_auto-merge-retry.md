# Code review — Fix/platform-sync_auto-merge-retry

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `50b56cdf2128b4be690399b2cba70e5904bde28b`  _(2026-08-17)_
**Security-reviewed up to commit:** `50b56cdf2128b4be690399b2cba70e5904bde28b`  _(2026-08-17)_
_(Touches `.github/workflows/` only because it's a workflow file — no new permissions, no new secret
usage, no change to `$branch`/`$VERSION` resolution or validation; purely wraps the existing, unchanged
CLI call in a retry loop.)_

> Range reviewed: `2cfbce326..50b56cdf2` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

Single change to `.github/workflows/platform-sync.yml`'s "Enable auto-merge" step: wraps the existing
auto-merge CLI call in a 5-attempt retry loop with linear backoff (10/20/30/40/50s), and fails the job
loudly with an error annotation if all five attempts fail. Verified:

- YAML parses (`yaml.safe_load`).
- The loop is a plain `for attempt in 1 2 3 4 5; do ... done` with an `if <call>; then exit 0; fi` body
  — `set -euo pipefail` is already active in this step (unchanged), and the `if` guard means a failed
  attempt doesn't trip `set -e` (standard bash behavior — a command's exit status inside an `if`
  condition never triggers `errexit`), so the loop actually gets to retry instead of dying on the first
  failure. This was the one thing worth double-checking given `set -e` is active in the same script.
- Success path (`exit 0` inside the loop) and failure path (falls through to the `::error::` + `exit 1`
  after the loop) are both reachable and correctly terminate the step either way.
- No change to the call's arguments, target branch resolution, or the surrounding steps — purely wraps
  the existing, already-correct call (no `--squash`/`--merge` flag, matching the in-file comment
  documenting why that flag must never be added).
- Backoff is linear and capped at 5 attempts / ~2.5 minutes total added latency in the worst case — small
  relative to the minutes-long CI run this step gates, and appropriate for absorbing a transient API
  blip without masking a genuinely persistent failure for too long.

No issues found.
