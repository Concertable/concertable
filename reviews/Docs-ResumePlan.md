# Code review — Docs/ResumePlan

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `449ed22084e30ac22adec0f86984cb3d40235791`  _(2026-08-03)_

> Range reviewed: `0d72f0ed..6d339e5b` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **WF1 — HIGH — correctness** — `plans/AGENTS.md:143`
  Lifecycle step 4 deletes the plan and ledger in the commit that completes the last locally verified phase, but the new ledger contract at lines 97–108 also requires recording later PR checks, merges, publications, and platform syncs as they happen. Those events necessarily occur after the completing commit, so `/resume-plan` loses its plan/ledger anchor during the delivery window and the promised exhaustive history cannot be written. Keep both artifacts until every delivery/package gate is terminal, then apply the close-out lifecycle, or define another durable post-phase ledger that the recovery skill discovers.

- [x] **WF2 — MEDIUM — correctness** — `.agents/skills/resume-plan/SKILL.md:15`
  No-reference discovery enumerates all worktrees in step 2 but then inspects only the current worktree. A context-lost session reopened in the main checkout cannot discover a uniquely active plan in one of the repository's many feature worktrees; it falls back to the large set of plan files on `main` and asks for a path even when branch/PR/worktree evidence would identify the work. Inspect every listed worktree's branch, dirty plan/ledger files, branch commits, review artifacts, and PR, then select a unique candidate or report the remaining candidates with their paths.

- [x] **WF3 — MEDIUM — correctness** — `plans/AGENTS.md:105`
  The new rule says every implementation, review, verification, commit, PR, merge, publication, and sync workflow owns the ledger update, but the workflow skills' terminal steps are not wired to discover or update a plan ledger. For example, `code-review` ends after writing its review file, `address-review` only inspects the plan and still says the worktree path is optional, and `merge` ends after its PR/platform-sync summary. Update the affected skills (or add one shared mandatory close-out procedure) to resolve the active plan and ledger, checkpoint the event and finding dispositions, and produce the new exact plan-plus-ledger handoff before reporting.
  Fixed by the shared plan-progress checkpoint and mandatory terminal hooks across plan-aware implementation, review, verification, commit, PR, merge, publication, and platform-sync workflows.

## Incremental review — 2026-08-03

No new issues found in `6d339e5b..c8dbc6b`. The range fixes WF1 and WF2; WF3 remains open.

## Incremental review — 2026-08-03 (`c8dbc6b..449ed220`)

- [x] **WF4 — HIGH — correctness** — `.agents/skills/push/SKILL.md:38`
  A successful push now runs the checkpoint afterward, and a remote event must create an immediate local checkpoint commit, but the push workflow never pushes that new commit. It can therefore report success while `HEAD` is ahead of the PR head, so the ledger evidence is absent from the PR and the merge precondition immediately fails. Make the checkpoint commit the final pushed head, verify `origin/<branch>` equals `HEAD`, and avoid recursively logging that final synchronization push.
  Fixed by preparing one compound push checkpoint before the remote mutation, including that commit in the authorized push, and verifying remote-tracking and PR-head equality without recursively checkpointing the synchronization leg.

- [x] **WF5 — MEDIUM — correctness** — `.agents/skills/merge/SKILL.md:194`
  The merge hook runs only immediately before the final report. Queue admission, check failure, merge completion, publication, and platform-sync are long-lived material transitions, so a lost context before the final summary still loses the exact live gate the ledger is meant to preserve. Checkpoint each material transition when it occurs, including before early stops, then reconcile once more before the final report.
  Fixed by immediate merge-transition checkpoints plus a remote-transition protocol that keeps observation commits durable without mutating a queued or merged PR head.
