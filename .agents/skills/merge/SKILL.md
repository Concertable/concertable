---
name: merge
description: Merge the current branch's PR into main through the merge queue (which runs E2E), wait for it to land, then return to a clean up-to-date main ready for the next task. Use whenever Tommy says "merge", "merge it", "merge this", "merge my branch", "land this PR", or wants the current feature branch shipped and the local repo reset to main. Concertable-specific (knows this repo's merge queue + E2E gate).
---

# merge

One command to land the current branch and reset to a clean `main`: verify the PR's own checks are
green, **enqueue it into the merge queue** (where the E2E suites run and gate the merge), wait for it
to actually land, then switch back to `main`, pull, and delete the merged branch — so there's no
juggling before the next task.

This skill is **Concertable-specific**. It encodes how this repo actually merges (see "Repo facts").

## Repo facts (why this skill exists)

- **`main` is protected by a merge queue** (ruleset `17393335`, `ALLGREEN`). Its required checks are
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

### Transition checkpoints

For plan-managed work, resolve the plan and ledger before step 0 and retain their absolute worktree,
source branch, PR number, and remote `headRefOid` as the delivery identity. Apply
[the shared plan-progress checkpoint](../resume-plan/references/plan-progress-checkpoint.md)
immediately after every material transition below, before the next wait, mutation, checkout, or
early stop. Unchanged polling observations do not need new checkpoints.

Checkpoint review/preflight readiness or blockers; PR discovery; dirty, uncommitted, unpushed,
remote-divergent, or base-stale state; branch update, verification, and compound-push results;
terminal PR checks; the E2E-tier and final labels; queue admission, rejection, ejection, check failure,
timeout, or green-but-unadmitted state; merge completion; main sync and branch cleanup; publication
discovery and terminal state when `api/**` changed; and platform-sync discovery, checks, fixes, pushes,
and merge. Record cancellations, contradictions, and no-op outcomes too. Use the shared push protocol
for an authorized source-head update and its remote-transition protocol for observations after checks
or queueing. Never push a checkpoint-only local tail to a queued, locked, merged, or closed PR.

0. **Review first — code or docs by PR type.** Before querying, pushing or merging a PR, confirm it has
   been reviewed. **Which review depends on the diff:** a **docs/meta-only** PR (every path under
   `**/*.md`, `.agents/**` & `.claude/**` skills, `plans/**`, `docs/**`, `AGENTS.md`, `CLAUDE.md`,
   `PROMPTS.md`, `README*`) requires a clean **`/docs-review`**, not a code review — and such a PR
   normally lands via `/merge-docs`, not this skill. Any runtime/product/package/CI-test-selection path
   makes it a code PR: require **`/code-review`** (`/big-review` when too large for one pass). If commits
   were added after the review, require `/incremental-review`. Do not proceed while review findings
   remain open.

1. **Find the PR for the current branch.**
   ```
   git rev-parse --abbrev-ref HEAD                 # current branch (must not be main)
   gh pr view --json number,state,title,url --jq '{number,state,title,url}'
   ```
   - If on `main`, or there's no PR for the branch, **stop** and say so — there's nothing to merge
     (open one with `create-gh-pr` first).
   - If the PR is already `MERGED`, skip to step 5 (sync main). If `CLOSED`, stop and report.

2. **Make sure the branch is pushed, current with its remote, AND not stale vs `main`.**
   - If `git status` shows uncommitted changes, or the local branch is ahead of its remote, **stop** and
     tell the user to commit/push first (or do it with the `commit` / `push` skills if they ask). Don't
     merge a PR that's missing local work.
     A local tail created by the shared remote-transition protocol is the sole exception: verify every
     commit after the PR `headRefOid` changes only the active plan and ledger, preserve it, and continue
     against the recorded remote PR head. Any other ahead or dirty state remains a blocker.
   - **`git status -sb` is NOT a main check.** Its `[ahead N, behind M]` compares to the branch's *own*
     remote only — a branch can read "in sync with origin/<branch>" while being dozens of commits behind
     `main`. Check drift vs main explicitly:
     ```
     git fetch origin --quiet
     git rev-list --left-right --count origin/main...HEAD   # -> "<behind-main>\t<ahead>"
     ```
   - **If `behind-main` > 0, update the branch before enqueueing** — merge main in, don't rebase (the
     PR is up for review; no force-push): `git merge origin/main`, resolve conflicts (the
     `<ConcertablePlatformVersion>` / version file is the usual one), then `dotnet build
     api/Concertable.slnx` to **0 errors** and `git push`. **This is non-negotiable when a
     `chore/platform-sync-*` has merged to main since the branch's base** (`git log --oneline
     origin/main ^HEAD | grep platform-sync`): the branch is pinned to an *older* platform version, so
     it builds/tests against a stale platform — the queue rebuilds on main but a real pin/shape drift
     surfaces as a queue kick-out (or worse, a green merge that's actually stale). Update, rebuild, push,
     then continue.

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
   - Checkpoint the complete terminal check set against the exact remote `headRefOid` before routing a
     failure or enqueueing. A green observation checkpoint is local-only: verify the PR head still
     equals the checked OID and enqueue that remote head, not the newer local checkpoint commit.

4. **Enqueue into the merge queue (the default — this is what runs E2E).**
   - **This skill is the single source of truth for the E2E tier. Full E2E is the default.**
   - Add `skip-e2e` only when the PR is **both small and demonstrably low-blast-radius**. Every one of
     these must be true:
     - The diff and affected area are small and isolated.
     - It touches no package/service boundary, shared infrastructure, build/publish/deployment pipeline,
       CI workflow, or multiple application surfaces.
     - It changes no user-facing/runtime flow covered by E2E.
     - Unit/integration tests fully cover the affected behaviour.
   - **Zero intended behaviour change is not sufficient.** Package renames, lockfile/workspace changes,
     shared-library moves, broad refactors, and build/publish separation still have a broad blast radius
     and must run full E2E. When in doubt, do not skip.
   - Before enqueueing, normalize the labels to the decision: remove stale `skip-e2e` /
     `skip-e2e-ui` labels when the PR does not qualify; add the appropriate label only when all criteria
     hold. If a PR must run full E2E but an earlier commit carries a true `Skip-E2E` /
     `Skip-E2E-UI` trailer, add `full-e2e`; it is the authoritative positive override and wins over
     every historical opt-out. Remove `full-e2e` when deliberately selecting a skip tier. Labels are
     read fresh from the PR in the merge group. `skip-tests` remains reserved for a genuinely trivial
     mechanical change; build + carve never skip.
   - **The `Skip-E2E: true` git trailer works too but is fragile here — don't rely on it.** Git parses
     only the *last* paragraph of a commit message as trailers, and every commit in this repo carries a
     mandated `Co-Authored-By:` trailer; if a blank line separates `Skip-E2E: true` from `Co-Authored-By:`
     they become two paragraphs and git no longer sees `Skip-E2E`, so **the queue silently runs E2E
     anyway** (observed on pr-262: `skipping` on the PR — because E2E never runs on PRs — but the full
     UI suite ran in the merge_group and flaked). `skipping` on the PR is **not** proof the skip took;
     only the label (or a correctly-blocked trailer) skips it *in the queue*. Prefer the label.
     `full-e2e` overrides both when the current merge decision requires the full suite.
   ```
   gh pr merge <n> --merge --auto
   ```
   - Verify and checkpoint actual queue admission for the recorded PR and remote `headRefOid`. If
     admission fails or the head changed, checkpoint the outcome and stop; do not push the local
     observation tail or silently enqueue a different head.
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
     Checkpoint the failing merge-group run, jobs, ejected state, and follow-up before debugging. Only
     after the PR is confirmed open and unlocked may a compound fix push create a new remote head.
   - **`--admin` override (only when the user explicitly asked to skip the queue):**
     `gh pr merge <n> --merge --admin` merges immediately with **no E2E**. Verify with
     `gh pr view <n> --json state,mergeCommit`.
   - Checkpoint closed-without-merge, failed checks, sustained green-but-unadmitted, and timeout states
     before stopping. On merge, immediately checkpoint the merge commit, method, E2E outcome, and
     source PR head before switching worktrees or syncing main.

5. **Return to a clean, up-to-date main — and remove the merged branch's worktree.**
   ```
   git checkout main
   git pull --ff-only origin main
   ```
   Then tear down the merged branch. **A worktree-developed branch is still checked out in its worktree,
   so the worktree must go FIRST** — `git branch -d` refuses to delete a branch checked out elsewhere. As
   soon as the PR is `MERGED`, remove the worktree it was developed in (`<path>` from `git worktree list`).
   The branch delete is gated on the worktree actually going: if it's dirty, keep BOTH so uncommitted work
   is never orphaned. (Branch developed in the main checkout, no worktree? Skip the `if` and just run the
   `git branch -d` + `git push origin --delete` lines.)
   ```
   if [ -z "$(git -C <path> status --porcelain | grep -vE '/(bin|obj|node_modules)/')" ]; then
     git worktree remove --force <path>       # safe: tracked tree clean, HEAD is the merged head
     git branch -d <merged-branch>            # now free to delete; safe: -d only deletes if merged
     git push origin --delete <merged-branch> # remote cleanup (the queue blocked gh's --delete-branch)
   else
     echo "worktree <path> has uncommitted source — worktree AND branch LEFT in place, handle manually"
   fi
   ```
   - If `git branch -d` refuses ("not fully merged") — usually because the merge was a squash/merge-commit
     and the local tip differs — confirm the PR really is `MERGED`, then it's safe to `git branch -D`.
     Don't force-delete an unmerged branch.
   - **The worktree teardown is now automatic (it used to be banned).** The incident that once destroyed
     an in-progress worktree was an *unguarded* teardown; the clean-check above is the guard. Remove ONLY
     when that porcelain check is empty — no uncommitted or untracked *source* (`bin/`/`obj/`/
     `node_modules/` don't count). Any real change → leave the worktree and report it; unpushed work is
     never destroyed. Leaving merged worktrees in place is exactly what piled up and filled the disk.
   - `--force` is required — it discards only the build output the guard already cleared; plain
     `git worktree remove` fails on a built tree with "Directory not empty".
   - Windows: a live `dotnet`/Playwright process can lock `bin/`/`node_modules` ("Device or resource
     busy"); `--force` still unregisters the worktree — report any leftover directory for manual deletion.
   - **Exception — persistent branches, NEVER auto-removed even when merged:** `Chore/TechDebt` (reused
     every debt pass — keep its branch and worktree).

6. **Watch the platform-sync consequence — a merge that touched a published package triggers it, and
   nothing else watches it.**
   Merging a change to a **published package** makes `Publish packages` republish, then `platform-sync`
   opens a `chore/platform-sync-*` PR that bumps every service's `<ConcertablePlatformVersion>` to the
   new version. That PR **auto-merges if green**, but goes **red** when the change broke a consumer's
   compile against the new shape. **A red sync PR that nobody watches strands every service on a broken
   pin and detonates on the next unrelated build** — so following it to a terminal state is part of
   this skill, not optional.
   - **Will a sync fire?** `Publish packages` runs on any merge that touches `api/**` (MinVer bumps
     the platform version every time), so **almost every code merge opens a sync PR** — most are green
     and auto-merge in minutes; only a breaking package change goes red. If the merge was
     docs/CI/app-only (nothing under `api/**`), no sync fires — you're done.
     ```
     gh pr diff <n> --name-only | grep -q '^api/' && echo "sync will fire" || echo "no sync"
     ```
   - **If `api/**` changed:** find the `Publish packages` run caused by the merge commit. Checkpoint its
     run ID/URL when discovered and its terminal conclusion and published version. A failed, cancelled,
     missing, or timed-out publication is a checkpointed early stop; do not wait for a sync PR that
     cannot open. If no `api/**` path changed, checkpoint the evidenced no-publication/no-sync outcome.
   - **If publication succeeded:** the sync PR opens within a few minutes. Checkpoint its number, URL,
     branch, version, and initial state when discovered, then poll ITS checks. A pin
     bump is package-only, so E2E no-ops — the gate is `build` + `unit` + `integration`, usually a few
     minutes (prefer the `Monitor` tool over busy-waiting):
     ```
     # there is only ever ONE open sync PR
     while true; do sp=$(gh pr list --state open --json number,headRefName \
         --jq '.[] | select(.headRefName|startswith("chore/platform-sync-")) | .number' | head -1);
       [ -n "$sp" ] && { echo "sync PR #$sp"; break; }; sleep 30; done
     while true; do out=$(gh pr checks "$sp" 2>&1);
       echo "$out" | awk -F'\t' '$2=="pending"' | grep -q . || { echo "TERMINAL"; break; }; sleep 30; done
     ```
   - **Green** → checkpoint the terminal checks, then confirm auto-merge and immediately checkpoint
     the sync merge commit and new version.
   - **Red** → **do not walk away.** This is a breaking platform change surfacing at exactly the
     consumers that must migrate. Read the failing `build` log (`gh run view --job <id> --log`), find
     the broken consumer(s), and **migrate them IN the sync PR** — now legal, the version is on the
     feed. Check out the sync branch, apply the fix, build `api/Concertable.slnx` against the new pins
     to confirm **0 errors**, then push; auto-merge lands it once CI greens. (This is the sync PR
     body's own instruction — the skill just guarantees someone actually does it instead of leaving it
     red.) The build job may report only the first broken file; **build the whole `.slnx` locally**, a
     namespace/shape move usually stranded several consumers, not one.
     Checkpoint the red checks and broken consumers before editing, then the fix, full build,
     sync-branch push, replacement checks, and merge as each occurs. Work on the sync branch in its own
     checkout; never push the source plan's recovery commits to either PR.

## Final summary

Before any report or stop, including a failed check or delivery gate, if this workflow is
plan-managed, read and apply
[the shared plan-progress checkpoint](../resume-plan/references/plan-progress-checkpoint.md) once more
as a final reconciliation hook. Verify the durable ledger agrees with the worktree, source PR head,
queue/merge state, publication, and platform sync. This does not replace the immediate checkpoints.

One short report: the PR that merged (number + merge commit), whether E2E ran (queue) or was skipped
(`--admin`, and why), that `main` is synced, and that the branch — **and its worktree, if the work was
done in one** — is cleaned up. Then the
platform-sync outcome: **no sync (nothing published), sync merged green (new version), or sync went
red and you migrated its consumers** (which files, now green) — never "merged, and left a red sync PR
behind." If you stopped early (failed check, red E2E in the queue, unpushed work), say exactly what's
blocking and what's needed.

Keep it terminal: verify PR green → enqueue → wait for MERGED → sync main → **watch the
platform-sync PR to green/merged (or migrate its consumers if it's red)** → summarize → stop. No
preamble. Plain `git`/`gh` only (personal repo — never the work PR/ADO skills).
