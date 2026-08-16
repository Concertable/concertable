---
name: merge
description: Merge the current branch's PR into main through the merge queue (which applies the selected E2E tier), wait for it to land, then return to a clean up-to-date main ready for the next task. Use whenever Tommy says "merge", "merge it", "merge this", "merge my branch", "land this PR", or wants the current feature branch shipped and the local repo reset to main. Concertable-specific (knows this repo's merge queue + E2E gate).
---

# merge

One command to land the current branch and reset to a clean `main`: verify the PR's own checks are
green, **select the E2E tier and enqueue it into the merge queue**, wait for it
to actually land, then switch back to `main`, pull, and delete the merged branch — so there's no
juggling before the next task.

This skill is **Concertable-specific**. It encodes how this repo actually merges (see "Repo facts").

## Repo facts (why this skill exists)

- **`main` is protected by a merge queue** (ruleset `17393335`, `ALLGREEN`). Its single required check
  is `ci-complete`, which aggregates the selected E2E tier and the carve jobs against current `main`.
  Required E2E runs on the merge group and blocks a red merge; deliberately skipped E2E reports no
  work without bypassing the queue.
- **`e2e-api-tests` / `e2e-ui-tests` are merge-queue-only** (`if: github.event_name == 'merge_group'`).
  On the PR itself they show **`skipping`** — expected, not a failure. They run **after** you enqueue,
  inside the merge group when Step 4 requires them. So a green PR is *not* proof required E2E passed;
  only the queue proves that.
- **The default merge path is the queue:** `gh pr merge <n> --merge --auto`. `allow_auto_merge` is **on**,
  so this enqueues the PR; the queue builds the merge group, runs the selected E2E tier + carves, and
  merges only if ALLGREEN. `--auto` returns immediately — the merge lands later, so you must **poll for
  `MERGED`**, not assume it merged. A full-E2E merge normally takes ~30-40 min.
- **`--admin` is an escape hatch, NOT the default.** Admins have `bypass_mode: always`, so
  `gh pr merge <n> --merge --admin` force-merges immediately and **bypasses the queue — meaning E2E does
  NOT run.** Only use it when the user *explicitly* asks to skip the queue (e.g. a doc-/config-/comment-
  only PR with zero runtime impact, or the queue itself is wedged). Never reach for `--admin` just
  because the queue is slow. Skipping E2E does not mean skipping the queue: apply Step 4's label and
  enqueue normally so the hard floor runs against current `main`.
- **`--delete-branch` is rejected while the merge queue is enabled** (`Cannot use --delete-branch when
  merge queue enabled`) — delete the branch separately, after it has merged.

## Steps

### Transition checkpoints

For plan-managed work, resolve the plan and ledger before step 0 and retain their absolute worktree,
source branch, PR number, and remote `headRefOid` as the delivery identity. Apply
[the shared plan-progress checkpoint](../resume-plan/references/plan-progress-checkpoint.md)
immediately after every material transition below, before the next wait, mutation, checkout, or
early stop. Unchanged polling observations do not need new checkpoints.
The final pushed PR head carries the recovery ledger. After merge, remove that PR worktree and
reconcile later outcomes from a fresh continuation or close-out worktree based on `origin/main`.

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
   `**/*.md`, `.agents/**`, `.claude/**`, `.codex/**`, `plans/**`, `docs/**`, `AGENTS.md`, `CLAUDE.md`,
   `PROMPTS.md`, `README*`) requires a clean **`/docs-review`**, not a code review — and such a PR
   normally lands via `/merge-docs`, not this skill. Any runtime/product/package/CI-test-selection path
   makes it a code PR: require **`/review`** (`/big-review` when too large for one pass). If commits
   were added after the review, require `/incremental-review`. Do not proceed while review findings
   remain open.

   **The gate is hook-enforced, and the hook is where the `!` escape comes in.**
   `.claude/hooks/merge-review-gate.py` (PreToolUse) blocks the *agent's* `gh pr merge` until
   `reviews/<branch>.md` is current + clean. It resolves *this session's* checkout branch, so a worktree
   PR merged from a main-rooted session trips it looking for `reviews/main.md`. When the merge is
   authorized — review clean, or Tommy chose to bypass — **don't ask how to proceed: hand him the command
   to run himself, `! gh pr merge <n> --merge --auto`** — the `!` prefix runs it in his session, outside
   the PreToolUse hook.

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
     surfaces as a queue kick-out. Update, run the smallest affected local build, push, and require
     exact-head PR CI before continuing.

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

4. **Select the E2E tier mechanically, then enqueue into the merge queue.**
   - **This skill is the single source of truth for the E2E tier. E2E is required if and only if the
     diff can break behaviour the hard floor cannot observe. Run full E2E when any positive trigger is
     present:**
     - a user-facing browser/UI flow;
     - an HTTP/API or cross-service (`*.Contracts` / gRPC) contract;
     - a published-package public shape consumers bind to; or
     - auth/routing behaviour observable only end-to-end.
   - **If any positive trigger is present, E2E cannot be skipped.** Remove `skip-e2e` and
     `skip-e2e-ui`, then add `full-e2e` so it overrides every historical opt-out trailer or label.
   - **If no positive trigger is present, add `skip-e2e` and remove `full-e2e` / `skip-e2e-ui`.** This
     is the default for everything outside the list; do not run E2E “to be safe.” In particular,
     internal refactors, call-site relocations, delegation/“wiring” through a new internal collaborator,
     and DI re-registrations skip E2E when the diff changes no HTTP/wire contract, crosses no
     service/package boundary, and integration tests covering the touched path prove identical behaviour.
   - **“Wiring” is not an independent E2E trigger.** Here it means a runtime value or public/cross-service
     contract that consumers bind to and is covered by a positive trigger above. Moving an internal DI
     registration or call site is not wiring in this sense when integration tests boot the real DI + HTTP
     path and prove the behaviour.
   - **The hard floor never changes:** every code PR runs build + carve + unit + integration, and every PR
     still enters the queue on current `main`. Never use `skip-tests` in this skill.
   - Labels are read fresh from the PR in the merge group. Normalize them exactly as above before
     enqueueing; do not preserve a stale label that contradicts the mechanical decision.
   - **The `Skip-E2E: true` git trailer works too but is fragile here — don't rely on it.** Git parses
     only the *last* paragraph of a commit message as trailers, and every commit in this repo carries a
     mandated `Co-Authored-By:` trailer; if a blank line separates `Skip-E2E: true` from `Co-Authored-By:`
     they become two paragraphs and git no longer sees `Skip-E2E`, so **the queue silently runs E2E
     anyway** (observed on pr-262: `skipping` on the PR — because E2E never runs on PRs — but the full
     UI suite ran in the merge_group and flaked). `skipping` on the PR is **not** proof the skip took;
     only the label (or a correctly-blocked trailer) skips it *in the queue*. Prefer the label.
     `full-e2e` overrides both whenever a positive trigger requires the full suite.
   ```
   gh pr merge <n> --merge --auto
   ```
   - Verify and checkpoint actual queue admission for the recorded PR and remote `headRefOid`. If
     admission fails or the head changed, reconcile the outcome before the next source update; do not
     push an observation-only commit or silently enqueue a different head.
   - **No `--delete-branch`** (the queue rejects it).
   - `--auto` only *enqueues*. Now **wait for it to actually land** — the queue runs carves plus the
     selected E2E tier on the merge group and merges only if green. A positive-trigger PR runs
     `e2e-api-tests` + `e2e-ui-tests`; allow ~30-40 min. A `skip-e2e` PR keeps the hard floor and the
     current-main queue merge but both E2E jobs no-op.
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
   - Reconcile closed-without-merge, failed checks, sustained green-but-unadmitted, and timeout states
     before the next source update. On merge, retain the result as evidence for the fresh worktree.

5. **Return to a clean, up-to-date main — and remove the merged feature worktree immediately.**
   ```
   git checkout main
   git pull --ff-only origin main
   ```
   From another checkout, use the repository command for every worktree-developed branch:
   ```powershell
   ./scripts/worktrees.ps1 close -Worktree <path> -PullRequest <n> [-PlanManaged]
   ```
   Add `-PlanManaged` when a plan owns the work. The command refuses dirty or detached worktrees,
   post-PR commits, PR/head mismatches, missing merged ledgers, case-colliding refs, and persistent
   branches. It handles junctions, Windows long paths, Git administration, and branch deletion.

   A branch developed in the main checkout has no worktree to close; apply the same evidence manually.
   - **Exception — persistent branches, NEVER auto-removed even when merged:** `Chore/TechDebt` (reused
     every debt pass — keep its branch and worktree).
   - If plan work remains, create its next PR-scoped worktree from `origin/main` and resume the same
     ledger. If only remote gates remain, use a fresh `Docs/<epic>_<name>_closeout` worktree.

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
     feed. Check out the sync branch, apply the fix, run the smallest builds covering every reported
     consumer, then push; auto-merge lands it once exact-head CI greens. (This is the sync PR
     body's own instruction — the skill just guarantees someone actually does it instead of leaving it
     red.) The build job may report only the first broken file; use the replacement PR CI build to
     discover any additional consumers rather than starting a full local solution build.
     Checkpoint the red checks and broken consumers before editing, then the fix, targeted builds,
     sync-branch push, replacement checks, and merge as each occurs. Work on the sync branch in its own
     checkout; never push the source plan's recovery commits to either PR.
   - **Close plan-managed delivery from the fresh docs worktree.** After publication and platform sync are
     terminal, commit the final ledger checkpoint. In the following commit delete the plan and ledger
     together and tick the owning roadmap item. Run `/docs-review` (skip it when the branch is a pure
     close-out — net diff deletions only), land the net meta-only branch through
     `/merge-docs`, which removes the close-out worktree through the repository command.

## Final summary

Before any report or stop, including a failed check or delivery gate, if this workflow is
plan-managed, read and apply
[the shared plan-progress checkpoint](../resume-plan/references/plan-progress-checkpoint.md) once more
as a final reconciliation hook. Verify the durable ledger agrees with the worktree, source PR head,
queue/merge state, publication, and platform sync. This does not replace the immediate checkpoints.

One short report: the PR that merged (number + merge commit), whether full E2E ran because a positive
trigger was present or was skipped by label because none was present, that `main` is synced, and that
the branch — **and its worktree, if the work was
done in one** — is cleaned up. For plan-managed work, also confirm the close-out docs PR landed and
its worktree was removed. Then the
platform-sync outcome: **no sync (nothing published), sync merged green (new version), or sync went
red and you migrated its consumers** (which files, now green) — never "merged, and left a red sync PR
behind." If you stopped early (failed check, red E2E in the queue, unpushed work), say exactly what's
blocking and what's needed.

Keep it terminal: verify PR green → enqueue → wait for MERGED → remove the PR worktree → sync main
→ **watch the platform-sync PR to green/merged (or migrate its consumers
if it's red)** → land plan close-out through `/merge-docs` → remove the close-out worktree → summarize
→ stop. No preamble. Plain `git`/`gh` only (personal repo — never the work PR/ADO skills).
