---
name: push
description: Push the current branch to its remote with git push. Use whenever the user wants to push their commits, "git push", "push my changes", "push to remote", or "push this branch". If the push fails, diagnose and resolve the cause, then push again. Generic — applies to any repo.
---

# push

Push the current branch to its remote. The happy path is one command; the job is to make the push actually land and to tell the user plainly if anything went wrong.

## Steps

1. **Push the current branch.**

   ```
   git push
   ```

   - If the branch has no upstream yet, push and set it:
     ```
     git push -u origin <current-branch>
     ```

2. **If it succeeds**, report it in one line — branch, remote, and how many commits moved (e.g. `Pushed 3 commits to origin/master`). Done.

3. **If it fails**, read the error and fix the actual cause, then push again. Common cases:
   - **Rejected, remote has new commits** (`fetch first` / non-fast-forward): `git pull --rebase`, resolve any conflicts, then push. If a rebase isn't safe or conflicts are messy, stop and tell the user rather than force-pushing.
   - **No upstream configured**: re-run with `git push -u origin <current-branch>`.
   - **No remote at all**: tell the user — don't invent one.
   - **Auth / permission failure**: report it; the user needs to fix credentials. Don't retry in a loop.
   - **Pre-push hook failed**: fix the underlying cause — never `--no-verify` unless the user explicitly asks.

4. **Never force-push** (`--force` / `--force-with-lease`) unless the user explicitly asks for it.

## Final summary

After the push lands (or if you genuinely can't make it land), tell the user in plain terms:
- **If it went straight through:** one line, nothing more.
- **If you had to fix something:** what the problem was (e.g. "remote was 2 commits ahead, rebased and pushed"), what you did about it, and the final state.
- **If you couldn't push:** what's blocking it and what the user needs to do next.

Keep it terminal: push, fix if needed, summarize, stop. No preamble.
