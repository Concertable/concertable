# Code review — Fix/merge-gate-resolves-pr

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it.

**Reviewed up to commit:** `bf2bdebe81614a4d5b546747dbccab7151c36ead`  _(2026-08-16)_

> Range reviewed: `origin/main..HEAD`.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **BUG1 — MEDIUM — correctness** — `.claude/hooks/merge-review-gate.py`
  The first cut of this fix made the review-blob fetch fatal, so a **merged** PR (whose branch GitHub
  has already deleted) failed with `git fetch origin <branch>` exit 128 and blocked the merge. Caught by
  the new test. **Fixed:** the fetch is best-effort across `branch` then `head`, and failure falls back
  to the working-tree copy; only an unresolvable *PR* blocks.

### Checked and clean

- **Still fails closed on every path that matters:** missing review file, open `- [ ]` findings, an
  unresolvable PR, and a bare merge on an unreviewed checkout all block. The security-marker layer is
  untouched.
- **The staleness relaxation is narrow.** `review_only` allows the marker to trail HEAD *only* when
  every changed path is under `reviews/`; any source change is still stale, and an unresolvable range
  returns False (fail closed). This closes a genuine impossibility: stamping the marker is itself a
  commit, so a review can never be stamped at the commit containing it.
- **Fallback preserved:** with no PR number in the command the hook resolves the checkout exactly as
  before, so in-worktree usage is unchanged.
- **Tested through the real contract** — the hook is driven via PreToolUse JSON on stdin, not by calling
  internals, so the test would catch a regression in the stdin/exit-code contract too.

### Known and deliberately not fixed here

- The gate matches the literal merge-command string **anywhere** in a Bash command, so it blocks
  commands that merely quote it — it blocked editing this very file and creating this PR until the body
  moved to a file. Narrowing it to an actual invocation is a separate change; doing it in the same PR
  as the resolution fix would conflate two behaviours in a security-adjacent gate.
