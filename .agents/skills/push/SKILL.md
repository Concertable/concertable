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

2. **Push the actual work head first.** Record `work_head=HEAD`, then push that exact head.

   ```
   git push
   ```

   - If the branch has no upstream yet, push and set it:
     ```
     git push -u origin <current-branch>
     ```

3. **Verify the work head landed.** Fetch the branch, then require its remote-tracking ref to equal
   `work_head`. If an open PR exists, also require the PR's `headRefOid` to equal `work_head`. A
   command returning success is not enough. Do not create a successful push checkpoint unless these
   comparisons pass.

4. **For a plan-managed push, checkpoint the verified result and transport it.** Follow the shared
   checkpoint's push protocol. Update the ledger with the starting remote head, pushed range, verified
   work and PR heads, outcome, and exact post-push next action. Stage only the plan and ledger and
   create one checkpoint commit. Push that commit as the checkpoint-transport leg, fetch again, and
   require local `HEAD`, the remote-tracking ref, and any PR `headRefOid` to all equal the checkpoint
   commit. This transport does not invoke the checkpoint procedure, add another transition, or
   create another checkpoint commit.

5. **If the push or verification fails**, read the error and fix the actual cause, then retry the
   same terminal push and verification. Common cases:
   - **Rejected, remote has new commits** (`fetch first` / non-fast-forward): `git pull --rebase`, resolve any conflicts, then push. If a rebase isn't safe or conflicts are messy, stop and tell the user rather than force-pushing.
   - **No upstream configured**: re-run with `git push -u origin <current-branch>`.
   - **No remote at all**: tell the user — don't invent one.
   - **Auth / permission failure**: report it; the user needs to fix credentials. Don't retry in a loop.
   - **Pre-push hook failed**: fix the underlying cause — never `--no-verify` unless the user explicitly asks.

   If the work-head push remains blocked or cannot be verified, do not report success. For
   plan-managed work, record the failed, rejected, or unknown outcome and exact known local, remote,
   and PR heads in one local failure checkpoint; do not push merely to publish that record.

   If the work head was verified but checkpoint transport fails, preserve the checkpoint and resolve
   the exact local, remote, and PR heads. Amend that same checkpoint with the failure, divergence, and
   new prerequisite only when refreshed refs prove the checkpoint commit never reached the remote or
   PR. If it may have landed, do not rewrite it: keep its truthful work-head evidence, leave the
   failure correction in the ledger working tree, and report the unknown or divergent heads. Never
   create a chain of checkpoint-transport commits or claim final synchronization without equality.

6. **Never force-push** (`--force` / `--force-with-lease`) unless the user explicitly asks for it.

## Final summary

After the push lands (or if you genuinely can't make it land), tell the user in plain terms:
- **If it went straight through:** one line, nothing more.
- **If you had to fix something:** what the problem was (e.g. "remote was 2 commits ahead, rebased and pushed"), what you did about it, and the final state.
- **If you couldn't push:** what's blocking it and what the user needs to do next.

Keep it terminal: push, fix if needed, summarize, stop. No preamble.
