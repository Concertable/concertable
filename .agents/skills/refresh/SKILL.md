---
name: refresh
description: Bring the local checkout fully up to date — fetch + prune, fix a stale origin/HEAD, detect if the current branch's PR already merged (and if so switch back to a clean main), or fast-forward main directly. For a still-open branch, report drift vs origin/main and merge it in when behind and the tree is clean. Use whenever the user wants to "refresh", "sync with main", "pull the latest changes and PRs", "get up to date", or start a session on a clean, current checkout.
---

# refresh

One command to make the local repo match reality: what's on the remote, and whether the branch
you're sitting on is still live or already shipped. `pull` only updates the current branch; `refresh`
also asks "is this branch even still needed?" and "am I looking at a stale default-branch pointer?"

## Steps

1. **Fetch and prune.**
   ```
   git fetch origin --prune --quiet
   ```
   Prune drops remote-tracking refs for branches deleted on the remote (merged PRs) — without it,
   `[gone]` detection in step 3 can't work.

2. **Fix a stale `origin/HEAD`.** The remote's default branch can be renamed (it happened in this repo:
   `master` → `main`) without the local clone noticing.
   ```
   actual=$(gh repo view --json defaultBranchRef -q .defaultBranchRef.name)
   cached=$(git symbolic-ref --short refs/remotes/origin/HEAD 2>/dev/null | sed 's#origin/##')
   [ "$actual" != "$cached" ] && git remote set-head origin -a
   ```

3. **Working tree must be clean before touching branches.** `git status --porcelain`. If dirty, stop and
   report it — don't merge, checkout, or pull over uncommitted work. Offer `commit` or a stash, don't
   just do one.

4. **Read the current branch's state.**
   ```
   git rev-parse --abbrev-ref HEAD
   git status -sb | head -1        # look for "[gone]"
   ```

   - **On `<actual default branch>` already:** just fast-forward it.
     ```
     git pull --ff-only origin <actual default branch>
     ```
     Report commits pulled (or "already up to date") and stop.

   - **On a feature branch — check whether its PR already shipped:**
     ```
     gh pr view --json number,state,url --jq '{number,state,url}' 2>&1
     ```
     - **No PR found**, or PR `MERGED`/`CLOSED`, or `git status -sb` showed `[gone]`: the branch is
       dead — its work already landed (or was abandoned). Switch back to a clean default branch:
       ```
       git checkout <actual default branch> 2>/dev/null || git checkout -b <actual default branch> --track origin/<actual default branch>
       git pull --ff-only origin <actual default branch>
       ```
       Report which PR merged (number + title) so the user knows *why* the branch is gone. Then clean
       up the now-dead branch safely — `-d` only deletes if it's actually merged, so this can't eat
       unmerged work:
       ```
       git branch -d <old-branch>
       git push origin --delete <old-branch> 2>/dev/null || true   # already gone remotely if PR merge deleted it
       ```
       **Exception — persistent branches are never auto-deleted even when "merged"** (e.g.
       `Chore/TechDebt`, reused across passes) — leave those checked out as-is.

     - **PR is `OPEN`:** this branch is live work — don't switch away from it. Report drift against the
       default branch and close it if safe:
       ```
       git rev-list --left-right --count origin/<actual default branch>...HEAD   # "<behind>\t<ahead>"
       ```
       - `behind` is 0: nothing to do, report in sync.
       - `behind` > 0 and the tree is clean (step 3 already confirmed this): merge it in, don't rebase
         (avoids rewriting a pushed/reviewed history):
         ```
         git merge origin/<actual default branch>
         ```
         If it merges clean, note the branch is now current and that a rebuild is worth doing before
         relying on green CI. If it conflicts, stop and surface the conflicting files — don't resolve
         blindly.

5. **Report other local branches worth cleaning up (don't touch them).** Branches whose remote is
   `[gone]` but aren't the current one are just noted, not deleted — deleting a branch you're not
   sitting on wasn't asked for:
   ```
   git for-each-branch 2>/dev/null; git branch -vv | grep ': gone]'
   ```

## Notes

- This never force-pushes, force-deletes, or rewrites a branch's already-pushed history. `merge`
  (never `rebase`) is the only history-changing step, and only on the branch you're already on.
- Deleting the *current* dead branch in step 4 is safe by construction (`git branch -d` refuses unless
  merged); deleting *other* local branches is left to the user to confirm.
- If `gh pr view` errors because there's no PR and no upstream tracking at all, treat it the same as
  "no PR found" — the branch may just be brand new local work; don't misreport it as dead without
  also checking it actually has an `origin/<branch>` to compare against.
