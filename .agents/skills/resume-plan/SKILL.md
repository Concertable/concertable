---
name: resume-plan
description: Resume plan-managed work from its ledger. Use when Tommy invokes `/resume-plan` or wants to pick a plan back up after a clear or handoff. Take an optional reference to a `_PROGRESS.md` ledger, a plan `.md`, or a worktree; otherwise use the current worktree's ledger. Resolve or recreate the ledger's PR-scoped worktree from current main, read the governing docs, then do what `## Next Steps` says.
---

# Resume Plan

A plan keeps one `_PROGRESS.md` ledger per logical workstream, whose `## Next Steps` is the single
resolved next action kept current at every checkpoint. Delivery happens through replaceable
PR-scoped worktrees; a plan may have several independent workstream ledgers.

## Steps

1. **Resolve the ledger and its current delivery worktree.** `/resume-plan` takes a `_PROGRESS.md` ledger, a plan `.md`,
   or a worktree:
   - **a ledger** (`/resume-plan @plans/<X>_PROGRESS.md`) → read its `Worktree` header.
   - **a plan** (`/resume-plan @plans/<X>_PLAN.md`) → find every `plans/**/*_PROGRESS.md` whose `- Plan:`
     header names that plan. One → use it. Several (independent workstreams) → list each
     with its worktree/branch and a one-line `## Next Steps` gist and ask which to resume — **unless the
     invocation also named a worktree**, then pick that one directly.
   - **nothing** → use the current worktree's ledger.

   Reconcile the recorded worktree, branch, and PR with Git and GitHub. If the worktree exists, `cd`
   there. If it was removed after a merged PR, fetch and create the next branch/worktree from current
   `origin/main`, resume the same ledger, and update its header in the next work commit. If an open
   branch/PR exists without a worktree, restore that exact branch instead. Stop on dirty state or a
   branch/worktree collision.
2. **Read in full:** `AGENTS.md`, `plans/AGENTS.md`, the `plans` skill, the plan, and the resolved
   ledger.
   After applying the root current-main sync gate, run
   `python .agents/hooks/plan_graph.py --root <absolute-worktree>` before trusting the graph; fix any
   reported graph error before resuming implementation.
3. **Confirm the ledger still holds** before acting: check its header branch/PR/gates against actual
   `git`/PR state and confirm `## Next Steps` still names one resolved action under `AGENTS.md`. Reconcile
   stale state or unresolved alternatives from current evidence and standing instructions, then update
   and compact the ledger's current state and `## Next Steps` before acting. Fold any still-material
   recent transition into the stable snapshot and remove superseded chronology. For every package, PR,
   publication, or sync dependency, classify implementation and delivery separately. Do not preserve a
   stale hard blocker when source or an exact producer artifact now permits safe local preparation.
4. **Do what `## Next Steps` says,** honoring its prerequisites and gates.
5. **When `## Next Steps` is a hard stop:**
   - Do any safe, authorized work in the current scope that can remove it. Also check whether the gate
     blocks only delivery: if local implementation, tests, or review can proceed against an exact
     producer artifact, reconcile the ledger to actionable delivery-gated work and continue normally.
   - If it cannot move, make `## Next Steps` start with the exact single-line `Blocked:`, `Blocked by:`,
     `Unblock action:`, and `Resume when:` fields from the `plans` skill. If the same fields and
     evidence were already recorded, do not create another no-change blocker checkpoint.
   - **Existing owner:** establish the reciprocal return path in every plan-owner ledger named by
     `Blocked by:` before stopping. Report all four lines verbatim and name the owner, but emit no
     prompt; the owner surfaces this plan when the gate opens.
   - **Separate resolver needed:** emit a paste-ready prompt for that resolving task, naming the
     blocked ledger and return condition. Never emit the blocked plan's own resume pointer.
   - **User or external action needed:** when only a human decision remains, mark `## Next Steps` with a
     single `Paused: <who> — <action>` line (lighter than the four-line schema), tell the user the exact
     action and objective verification condition directly, and never emit the plan's own resume pointer.

   A blocked plan's ordinary pointer is actively misleading: replaying it can only repeat the same
   audit. It becomes valid again only after evidence opens the gate and the ledger is reconciled to an
   actionable `## Next Steps`.

For the checkpoint procedure repository workflows apply when they advance plan work, see
[the plan-progress checkpoint](references/plan-progress-checkpoint.md).

If a plan genuinely has no ledger it predates the convention: create one from
[the progress template](assets/progress-template.md) as a labelled reconstructed baseline, then resume.
