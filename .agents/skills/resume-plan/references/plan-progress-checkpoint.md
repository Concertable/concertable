# Plan-progress checkpoint

Apply this procedure before a repository workflow reports its result.

## Resolve the plan

1. Prefer an explicit `plans/*.md` input supplied to the workflow.
2. Otherwise inspect the current worktree's absolute root, branch, upstream, status, commits, review
   artifacts, and PR. Correlate those facts with candidate plans and `_PROGRESS.md` metadata under
   `plans/`. Plan candidates are `*_PLAN.md`; exclude `AGENTS.md`, `*_PROGRESS.md`, `*_ROADMAP.md`, and
   bare-stem reference docs.
3. Continue only when the evidence identifies exactly one plan. If no plan or multiple plans remain,
   do not create, choose, update, or emit a handoff for one; finish the workflow normally.
4. Use the same-directory `<NAME>_PROGRESS.md` (the plan's `<NAME>_PLAN.md` stem). If it is a legacy plan without a ledger, create
   the ledger from `../assets/progress-template.md` with an explicitly labelled reconstructed baseline
   containing only facts supported by repository evidence.

## Record the transition

Read the plan and ledger, then reconcile them with the workflow's actual evidence. Update every
current-summary section affected by the event:

- worktree, branch, PR, and dependency or package gates;
- current state and partial or uncommitted work that must be preserved;
- `## Next Steps` — the single resolved next action as self-contained steps; when no action can
  proceed, the exact `Blocked:`, `Blocked by:`, `Unblock action:`, and `Resume when:` fields from
  `plans/agents/PLAN.md`;
- compact completed milestones with commit or PR evidence;
- the latest verification commands and outcomes still valid for the current code state;
- current review state and every finding that remains open or needs follow-up;
- decisions, discoveries, blockers, and deviations that still affect execution.

Do not append a permanent dated event entry. If the transition cannot yet be represented safely in the
stable sections, put it briefly in optional `## Recent transitions`. At every checkpoint, fold prior
entries into the stable snapshot and delete superseded chronology. When touching a legacy ledger with
an append-only `## Event log`, compact it under this rule in the same checkpoint; retain only facts whose
removal could change the next agent's action or cause a costly failed approach to be repeated.

For review work, keep the review type and range, artifact, and every finding that remains open or needs
follow-up. Once all findings are resolved and the follow-up review is clean, collapse them to the clean
reviewed state and the fixing commits still material to the branch; the review artifact and git retain
the detailed history. Never claim a transition that the workflow did not verify.

Use workflow-specific evidence: implementation paths and partial state; review range, artifact,
watermark and finding dispositions; verification command, tested commit/working tree, counts and
result; commit subject and SHA (or `this commit` inside the commit that carries the entry); pushed
remote/range and resulting PR head; PR number/URL/head/checks; queue result and merge SHA; published
package/version/run; and platform-sync PR/version/check/merge state. Record failed, blocked, cancelled,
or no-op outcomes only while they affect the current state, next action, or a durable decision.

## Preserve and checkpoint

Keep the plan and ledger while any required review/fix, verification, PR/check/merge, publication,
dependency, or platform-sync gate is non-terminal. After the final gate, record its outcome and make
that ledger state durable before deleting the plan and ledger together in the following close-out
change. The plan and ledger survive the source PR merge on `main`.

Include the ledger update in the work commit when its evidence is already known. If the event became
known after the final pushed PR head or occurred remotely, do not add a local observation commit to
the source branch. Reconcile it from a fresh continuation or close-out worktree based on `origin/main`.

### Push protocol

A plan-managed push has two legs. Resolve the plan and record the starting remote head, actual work
head, local range, branch, and PR when one exists. First push the actual work head without creating a
new push checkpoint. Fetch the branch and require its remote-tracking ref and any PR `headRefOid` to
equal that recorded work head. A successful command without those comparisons is not verified.

Only after the work head is verified, update the ledger with the evidenced pushed range, resulting
work and PR heads, outcome, and exact post-push next action. Stage only the plan and ledger and create
one checkpoint commit. Push that commit as a checkpoint-transport leg, then fetch and require local
`HEAD`, the remote-tracking ref, and any PR `headRefOid` to equal the checkpoint commit. Transport is
part of the same push event: it never invokes this procedure recursively, adds another transition,
or creates another checkpoint commit. The ledger records the verified work push and resulting next
action; it does not fabricate advance evidence that its own transport succeeded.

If the work-head leg fails or cannot be verified after diagnosis, record the failed, rejected, or
unknown outcome and exact known heads in one local failure checkpoint; do not push merely to publish
that failure record. If the work head was verified but checkpoint transport fails, keep the checkpoint
local. Amend that same commit with accurate transport failure, divergence, and prerequisite evidence
only when refreshed remote and PR refs prove it did not land. If its remote status is unknown, do not
rewrite a commit that may have landed: preserve its truthful work-head result, leave the corrective
ledger update in the working tree, and report every known or unknown head. Never claim final success
unless final local, remote-tracking, and PR equality is verified.

### Remote-transition protocol

Resolve the plan once and retain its source worktree, branch, PR number, and remote `headRefOid`.
The final pushed PR head carries the ledger's current state and exact next remote gate. GitHub keeps
queue, check, and merge evidence durable; observe it without committing after the source PR head.

If the queue ejects the PR, reconcile the failure with its required fix and publish both through the
normal push protocol. Never push an observation-only commit to re-trigger a queue.

As soon as the source PR is `MERGED`, switch to another checkout and run:

```powershell
./scripts/worktrees.ps1 close -Worktree <source-path> -PullRequest <n> -PlanManaged
```

The command proves the PR/head relationship, merged containment, cleanliness, and ledger presence at
both the PR head and `origin/main`. If work remains, create a fresh continuation or docs close-out
worktree from current `origin/main` and reconcile remote outcomes there. There is no tail to transfer.

If the queue ejects the PR and a code fix is required, checkpoint the failure first. Once the PR is
confirmed open and unlocked, commit the fix with the ledger update and use the compound push
protocol above to publish the complete new head. A platform-sync fix belongs to the sync PR's own
branch or worktree: record its discovery in the continuation or close-out ledger, never in the merged
source branch.

The final report still requires a full reconciliation checkpoint. That hook verifies and records the
end state; it does not replace any transition checkpoint that should already exist.

## Report and hand off

Report the workflow result only after the checkpoint is durable, having written the immediate next
action into the ledger's `## Next Steps` section so it is the durable source of truth. If actionable
plan-managed work remains, hand off only the current or explicitly targeted logical workstream.
Do not claim a dependency ledger merely because it was read or received a cross-workstream return-link
edit. Use the cd-first pointer for a live worktree or the worktree-create opener from `PROMPTS.md` after
merged cleanup. The prompt remains ONLY
the pointer — nothing plan-specific. Delivery-gated local preparation is actionable; an
implementation-blocked ledger gets no pointer. Literally:

```
Why: `<PLAN>_PROGRESS.md` owns unfinished work from this turn: <short next-action reason>
Only run this continuation if no agent or session already owns this ledger.
```

```
<cd existing-worktree OR /worktree create Type/epic_name>
Read @plans/<PLAN>_PLAN.md and @plans/<PLAN>_PROGRESS.md and do what its `## Next Steps` says.
```

No branch to verify, checkpoints, gates, commands, or steps in the prompt — every such specific lives in
the ledger, never restated, so the prompt can't drift. If the lifecycle is terminal, follow the close-out
rule and do not invent a continuation prompt. If the plan is hard-blocked, do not emit this pointer:
report the ledger's four blocker lines verbatim, then emit a resolver dispatch
prompt only when a separate unowned task can open the gate.
