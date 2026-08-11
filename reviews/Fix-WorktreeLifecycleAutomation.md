# Code review — Fix/WorktreeLifecycleAutomation

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `40b9c50b3b4ba7948a3a98b0d97e5961362a4549`  _(2026-08-11)_

> Range reviewed: `ed76ef7..40b9c50` (3 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — HIGH — native correctness** — `scripts/worktrees.ps1:250`
  Removed the repository-wide `git worktree prune` from targeted cleanup so closing one worktree
  cannot unregister unrelated prunable or persistent worktrees.
