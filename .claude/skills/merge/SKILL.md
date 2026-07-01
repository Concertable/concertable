---
name: merge
description: Merge the current branch's PR into master through the merge queue (which runs E2E), wait for it to land, then return to a clean up-to-date master ready for the next task. Use whenever Tommy says "merge", "merge it", "merge this", "merge my branch", "land this PR", or wants the current feature branch shipped and the local repo reset to master. Concertable-specific (knows this repo's merge queue + E2E gate).
---

# merge

One command to land the current branch and reset to a clean `master`: verify the PR's own checks are
green, **enqueue it into the merge queue** (where the E2E suites run and gate the merge), wait for it
to actually land, then switch back to `master`, pull, and delete the merged branch — so there's no
juggling before the next task.

This skill is **Concertable-specific**. It encodes how this repo actually merges (see "Repo facts").

## Repo facts (why this skill exists)

- **`master` is protected by a merge queue** (ruleset `17393335`, `ALLGREEN`). Its required checks are
  `e2e-api-tests`, `e2e-ui-tests`, and the five `carve-*` jobs — i.e. **the queue is the E2E gate.**
  The whole point of merging through the queue is that E2E runs on the merge group and blocks a red merge.
- **`e2e-api-tests` / `e2e-ui-tests` are merge-queue-only** (`if: github.event_name == 'merge_group'`).
  On the PR itself they show **`skipping`** — expected, not a failure. They run **after** you enqueue,
  inside the merge group. So a green PR is *not* proof E2E passed; only the queue proves that.
- **The default merge path is the queue:** `gh pr merge <n> --merge --auto`. `allow_auto_merge` is **on**,
  so this enqueues the PR; the queue builds the merge group, runs E2E + carves, and merges only if
  ALLGREEN. `--auto` returns immediately — the merge lands later (allow ~30-40 min: a 5-min batching
  wait + the E2E runtime), so you must **poll for `MERGED`**, not assume it merged.
- **`--admin` is an escape hatch, NOT the default.** Admins have `bypass_mode: always`, so
  `gh pr merge <n> --merge --admin` force-merges immediately and **bypasses the queue — meaning E2E does
  NOT run.** Only use it when the user *explicitly* asks to skip the queue (e.g. a doc-/config-/comment-
  only PR with zero runtime impact, or the queue itself is wedged). Never reach for `--admin` just
  because the queue is slow. If you're unsure whether a change is trivial enough to skip E2E, it isn't —
  use the queue.
- **`--delete-branch` is rejected while the merge queue is enabled** (`Cannot use --delete-branch when
  merge queue enabled`) — delete the branch separately, after it has merged.

## Steps

1. **Find the PR for the current branch.**
   ```
   git rev-parse --abbrev-ref HEAD                 # current branch (must not be master)
   gh pr view --json number,state,title,url --jq '{number,state,title,url}'
   ```
   - If on `master`, or there's no PR for the branch, **stop** and say so — there's nothing to merge.
   - If the PR is already `MERGED`, skip to step 5 (sync master). If `CLOSED`, stop and report.

2. **Make sure the branch is actually pushed and current.**
   - If `git status` shows uncommitted changes, or the local branch is ahead of its remote, **stop** and
     tell the user to commit/push first (or do it with the `commit` / `push` skills if they ask). Don't
     merge a PR that's missing local work.

3. **Wait for the PR's own checks to reach a terminal state, then verify green.**
   - Poll `gh pr checks <n>` until **no** check is `pending`. Prefer the `Monitor` tool with an
     until-loop so you're notified instead of busy-waiting, e.g.:
     ```
     while true; do out=$(gh pr checks <n> 2>&1);
       pend=$(echo "$out" | awk -F'\t' '$2=="pending"' | wc -l);
       fail=$(echo "$out" | awk -F'\t' '$2=="fail"'    | wc -l);
       if [ "$fail" -gt 0 ]; then echo "FAILED"; echo "$out" | awk -F'\t' '$2=="fail"{print $1}'; break; fi;
       if [ "$pend" -eq 0 ]; then echo "ALL-TERMINAL"; break; fi;
       sleep 20; done
     ```
   - **Treat `skipping` as expected** for `e2e-api-tests` / `e2e-ui-tests` (they run in the queue, not on
     the PR). The PR-level pass set is `build`, `carve-*`, `unit-tests`, `integration-tests`.
   - **If any check failed:** do **not** merge. Report which job failed and route to the matching debug
     skill (`integration-debug` for unit/integration, `e2e-api-debug` / `e2e-ui-debug` for E2E, or read
     the failing job's log for `build`/`carve-*`). Drive it green, push, and re-run this skill.

4. **Enqueue into the merge queue (the default — this is what runs E2E).**
   ```
   gh pr merge <n> --merge --auto
   ```
   - **No `--delete-branch`** (the queue rejects it).
   - `--auto` only *enqueues*. Now **wait for it to actually land** — the queue runs `e2e-api-tests` +
     `e2e-ui-tests` + carves on the merge group and merges only if green. Poll patiently (E2E is slow;
     allow ~30-40 min):
     ```
     while true; do st=$(gh pr view <n> --json state --jq .state 2>&1);
       echo "$st"; [ "$st" = "MERGED" ] && break; [ "$st" = "CLOSED" ] && { echo "CLOSED-unmerged"; break; };
       sleep 60; done
     ```
   - **If the queue kicks the PR out (E2E went red in the merge group):** the PR returns to `OPEN` and a
     merge-queue check fails. Treat it exactly like a red suite — enter `e2e-api-debug` / `e2e-ui-debug`,
     fix the real bug, push, and re-run this skill. Do **not** fall back to `--admin` to force it past a
     red E2E — that defeats the entire gate.
   - **`--admin` override (only when the user explicitly asked to skip the queue):**
     `gh pr merge <n> --merge --admin` merges immediately with **no E2E**. Verify with
     `gh pr view <n> --json state,mergeCommit`.

5. **Return to a clean, up-to-date master.**
   ```
   git checkout master
   git pull --ff-only origin master
   git branch -d <merged-branch>            # local cleanup (safe: only deletes if merged)
   git push origin --delete <merged-branch> # remote cleanup (the queue blocked gh's --delete-branch)
   ```
   - If `git branch -d` refuses ("not fully merged") — usually because the merge was a squash/merge-commit
     and the local tip differs — confirm the PR really is `MERGED`, then it's safe to `git branch -D`.
     Don't force-delete an unmerged branch.

## Final summary

One short report: the PR that merged (number + merge commit), whether E2E ran (queue) or was skipped
(`--admin`, and why), that `master` is synced, and that the branch is cleaned up — i.e. **ready for the
next task**. If you stopped early (failed check, red E2E in the queue, unpushed work), say exactly
what's blocking and what's needed.

Keep it terminal: verify PR green → enqueue → wait for MERGED → sync master → summarize → stop. No
preamble. Plain `git`/`gh` only (personal repo — never the work PR/ADO skills).
