---
name: merge
description: Merge the current branch's PR into master, wait for its checks, then return to a clean up-to-date master ready for the next task. Use whenever Tommy says "merge", "merge it", "merge this", "merge my branch", "land this PR", or wants the current feature branch shipped and the local repo reset to master. Concertable-specific (knows this repo's merge queue + admin-bypass merge path).
---

# merge

One command to land the current branch and reset to a clean `master`: confirm the PR's checks are green, merge it, then switch back to `master`, pull, and delete the merged branch — so there's no juggling before the next task.

This skill is **Concertable-specific**. It encodes how this repo actually merges (see "Repo facts" below) so you don't rediscover it each time.

## Repo facts (why this skill exists)

- **`master` is protected by a merge queue** (ruleset `17393335`, `ALLGREEN`). A plain `gh pr merge --merge` does **not** work: gh tries to enqueue via auto-merge, and the repo has `allow_auto_merge` **off**, so it fails with `Auto merge is not allowed for this repository`.
- **Admins have `bypass_mode: always`** in that ruleset, so the working merge path is an **admin merge**: `gh pr merge <n> --merge --admin`. That is the sanctioned path here, not a policy hack.
- **`--delete-branch` is rejected while the merge queue is enabled** (`Cannot use --delete-branch when merge queue enabled`) — delete the branch separately, after.
- **`e2e-api-tests` / `e2e-ui-tests` are merge-queue-only** (`if: github.event_name == 'merge_group'`). On the PR itself they show **`skipping`** — that is expected, not a failure. The PR-level gate you wait on is `build` + the five `carve-*` jobs + `unit-tests` + `integration-tests`.
- **An admin merge bypasses the queue, so the e2e suites do NOT run on merge.** That's fine for zero-/low-behaviour-change work (most of what this skill is for). If the change is **massive or behaviourally risky** (payments, settlement, the event/projection chain, registration/login, broadly cross-cutting), run E2E **before** invoking this skill — via the `e2e-ui-regress` / `e2e-debug` skills — and only merge once it's green. This skill does not run E2E itself.

## Steps

1. **Find the PR for the current branch.**
   ```
   git rev-parse --abbrev-ref HEAD                 # current branch (must not be master)
   gh pr view --json number,state,title,url --jq '{number,state,title,url}'
   ```
   - If on `master`, or there's no PR for the branch, **stop** and say so — there's nothing to merge.
   - If the PR is already `MERGED`, skip to step 5 (sync master). If `CLOSED`, stop and report.

2. **Make sure the branch is actually pushed and current.**
   - If `git status` shows uncommitted changes, or the local branch is ahead of its remote, **stop** and tell the user to commit/push first (or do it with the `commit` / `push` skills if they ask). Don't merge a PR that's missing local work.

3. **Wait for the PR checks to reach a terminal state, then verify green.**
   - Poll `gh pr checks <n>` until **no** check is `pending`. Prefer the `Monitor` tool with an until-loop so you're notified instead of busy-waiting, e.g.:
     ```
     while true; do out=$(gh pr checks <n> 2>&1);
       pend=$(echo "$out" | awk -F'\t' '$2=="pending"' | wc -l);
       fail=$(echo "$out" | awk -F'\t' '$2=="fail"'    | wc -l);
       if [ "$fail" -gt 0 ]; then echo "FAILED"; echo "$out" | awk -F'\t' '$2=="fail"{print $1}'; break; fi;
       if [ "$pend" -eq 0 ]; then echo "ALL-TERMINAL"; break; fi;
       sleep 20; done
     ```
   - **Treat `skipping` as pass** (the e2e merge-queue jobs). The real pass set is `build`, `carve-auth/payment/search/b2b/customer`, `unit-tests`, `integration-tests`.
   - **If any check failed:** do **not** merge. Report which job failed and route to the matching debug skill (`integration-debug` for unit/integration, `e2e-api-debug` / `e2e-ui-debug` for E2E, or read the failing job's log for `build`/`carve-*`). Drive it green, push, and re-run this skill.

4. **Merge (admin bypass — the working path here).**
   ```
   gh pr merge <n> --merge --admin
   ```
   - **No `--delete-branch`** (the queue rejects it).
   - **Verify** it landed: `gh pr view <n> --json state,mergeCommit --jq '{state,mergeCommit:.mergeCommit.oid}'` should show `MERGED`.

5. **Return to a clean, up-to-date master.**
   ```
   git checkout master
   git pull --ff-only origin master
   git branch -d <merged-branch>            # local cleanup (safe: only deletes if merged)
   git push origin --delete <merged-branch> # remote cleanup (the queue blocked gh's --delete-branch)
   ```
   - If `git branch -d` refuses ("not fully merged") — usually because the merge was a squash/merge-commit and the local tip differs — confirm the PR really is `MERGED`, then it's safe to `git branch -D`. Don't force-delete an unmerged branch.

## Final summary

One short report: the PR that merged (number + merge commit), that `master` is synced, and that the branch is cleaned up — i.e. **ready for the next task**. If you stopped early (failed check, unpushed work, blocked merge), say exactly what's blocking and what's needed.

Keep it terminal: verify green → merge → sync master → summarize → stop. No preamble. Plain `git`/`gh` only (personal repo — never the work PR/ADO skills).
