# Plan-progress checkpoint

Apply this procedure before a repository workflow reports its result.

## Resolve the plan

1. Prefer an explicit `plans/*.md` input supplied to the workflow.
2. Otherwise inspect the current worktree's absolute root, branch, upstream, status, commits, review
   artifacts, and PR. Correlate those facts with candidate plans and `_PROGRESS.md` metadata under
   `plans/`. Exclude `AGENTS.md` and `*_PROGRESS.md` from plan candidates.
3. Continue only when the evidence identifies exactly one plan. If no plan or multiple plans remain,
   do not create, choose, update, or emit a handoff for one; finish the workflow normally.
4. Use the same-directory `<PLAN_STEM>_PROGRESS.md`. If it is a legacy plan without a ledger, create
   the ledger from `../assets/progress-template.md` with an explicitly labelled reconstructed baseline
   containing only facts supported by repository evidence.

## Record the transition

Read the plan and ledger, then reconcile them with the workflow's actual evidence. Update every
current-summary section affected by the event:

- worktree, branch, PR, and dependency or package gates;
- current state and partial or uncommitted work that must be preserved;
- exact next action, including the prerequisite that blocks it when applicable;
- completed work with commit or PR evidence;
- verification commands and outcomes, tied to the code state they verified;
- decisions, discoveries, blockers, and deviations.

Append a dated event-log entry with the action, evidence, outcome, and follow-up. For review work,
also record the review type and range, artifact, every finding ID and disposition (`open`, `fixed`,
`deferred`, or `superseded`), and the fixing commit or deferral evidence. Never claim a transition
that the workflow did not verify.

Use workflow-specific evidence: implementation paths and partial state; review range, artifact,
watermark and finding dispositions; verification command, tested commit/working tree, counts and
result; commit subject and SHA (or `this commit` inside the commit that carries the entry); pushed
remote/range and resulting PR head; PR number/URL/head/checks; queue result and merge SHA; published
package/version/run; and platform-sync PR/version/check/merge state. Record failed, blocked, cancelled,
and no-op outcomes too, with the prerequisite or reason.

## Preserve and checkpoint

Keep the plan and ledger while any required review/fix, verification, PR/check/merge, publication,
dependency, or platform-sync gate is non-terminal. After the final gate, record its outcome and make
that ledger state durable before deleting the plan and ledger together in the following close-out
change. A final local phase with no later gate may close them in its completing commit.

Include the ledger update in the work commit when its evidence is already known. If the event became
known only after that commit or occurred remotely, stage only the plan/ledger changes and create an
immediate local checkpoint commit when repository rules permit. Never push merely because this
procedure created a checkpoint; pushing remains governed by the invoking workflow and user request.

## Report and hand off

Report the workflow result only after the checkpoint is durable. If plan-managed work remains, end
with exactly one self-contained prompt and no competing next-action prompt. Its first line must be:

`cd <absolute-worktree-path>`

Name both the plan and ledger, require reading `AGENTS.md`, `plans/AGENTS.md`, the plan, and ledger,
state the verified branch/PR, and direct the immediate next action with its prerequisite or gate. If
the lifecycle is terminal, follow the close-out rule and do not invent a continuation prompt.
