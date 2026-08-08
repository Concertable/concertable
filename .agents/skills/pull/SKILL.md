---
name: pull
description: Pull the latest changes for the current branch, and if the pull fails, diagnose the cause, fix it, and briefly explain what was wrong. Use whenever the user wants to "pull", "pull latest", "get the latest changes", or recover from a failed pull.
---

# pull

Pull latest changes. If it fails, figure out why, fix it, and explain the cause in a sentence or two.

## Steps

1. Run `git pull`.
2. If it succeeds, report the result in one line (e.g. files changed / already up to date) and stop.
3. If it fails, read the error and fix the actual cause. Common ones:
   - **Local uncommitted changes blocking the merge** → offer to stash (`git stash`), pull, then `git stash pop`; resolve any pop conflicts.
   - **Merge conflicts** → resolve them in the conflicting files, then complete the merge.
   - **Diverged branches / non-fast-forward** → rebase or merge as appropriate for the situation.
   - **No upstream tracking branch** → set it (`git branch --set-upstream-to=origin/<branch>`) or pull with an explicit remote/branch.
   - **Detached HEAD / wrong branch** → surface it; don't guess which branch they meant.
4. After fixing, re-run the pull to confirm it lands clean.
5. Explain in 1–2 sentences what was causing the failure and what you did.

## Notes

- Keep it lightweight: pull, fix only if needed, explain briefly. No preamble.
- Don't do anything destructive (hard reset, force-anything, discarding local work) without asking first.
