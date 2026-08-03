---
name: push
description: Push the current branch to its remote with git push. Use whenever the user wants to push their commits, "git push", "push my changes", "push to remote", or "push this branch". If the push fails, diagnose and resolve the cause, then push again. Generic — applies to any repo.
---

# push

Push the current branch to its remote. The happy path is one command; the job is to make the push actually land and to tell the user plainly if anything went wrong.

## Steps

1. **Resolve the push state before mutating the remote.** Record the current branch, upstream, remote
   tip, commits to send, and any open PR/head. Resolve whether the work is plan-managed using
   [the shared plan-progress checkpoint](../resume-plan/references/plan-progress-checkpoint.md).

2. **For a plan-managed push, prepare the checkpoint first.** Follow the checkpoint's push protocol:
   record the compound push transition without claiming success in advance, stage only the plan and
   ledger, and create its local checkpoint commit. That commit, identified in the ledger as `this
   commit`, must be included in the push below. Do not push the work first and create the checkpoint
   afterward.

3. **Push the current branch.**

   ```
   git push
   ```

   - If the branch has no upstream yet, push and set it:
     ```
     git push -u origin <current-branch>
     ```

4. **Verify the resulting remote head.** Fetch the branch, then require its remote-tracking ref to
   equal local `HEAD`. If an open PR exists, also require the PR's `headRefOid` to equal `HEAD`. A
   command returning success is not enough. For a plan-managed push, this is the terminal
   synchronization leg of the already-recorded compound event: do not invoke the checkpoint again,
   append another push event, or create another success commit.

5. **If the push or verification fails**, read the error and fix the actual cause, then retry the
   same terminal push and verification. Common cases:
   - **Rejected, remote has new commits** (`fetch first` / non-fast-forward): `git pull --rebase`, resolve any conflicts, then push. If a rebase isn't safe or conflicts are messy, stop and tell the user rather than force-pushing.
   - **No upstream configured**: re-run with `git push -u origin <current-branch>`.
   - **No remote at all**: tell the user — don't invent one.
   - **Auth / permission failure**: report it; the user needs to fix credentials. Don't retry in a loop.
   - **Pre-push hook failed**: fix the underlying cause — never `--no-verify` unless the user explicitly asks.

   If it remains blocked or the remote result cannot be verified, do not report success. For
   plan-managed work, update the existing ledger event with the failed, rejected, or unknown outcome
   and the exact local/remote heads, then create a local failure checkpoint without pushing merely to
   publish that record. Report the divergence and blocker accurately.

6. **Never force-push** (`--force` / `--force-with-lease`) unless the user explicitly asks for it.

## Final summary

After the push lands (or if you genuinely can't make it land), tell the user in plain terms:
- **If it went straight through:** one line, nothing more.
- **If you had to fix something:** what the problem was (e.g. "remote was 2 commits ahead, rebased and pushed"), what you did about it, and the final state.
- **If you couldn't push:** what's blocking it and what the user needs to do next.

Keep it terminal: push, fix if needed, summarize, stop. No preamble.
