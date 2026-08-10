---
name: resume-plan
description: Resume plan-managed work from its ledger. Use when Tommy invokes `/resume-plan` or wants to pick a plan back up after a clear or handoff. Take an optional reference to a `_PROGRESS.md` ledger, a plan `.md`, or a worktree; otherwise use the current worktree's ledger. `cd` to the resolved worktree, read AGENTS.md, plans/AGENTS.md, plans/agents/PLAN.md, the plan and its ledger, then do what the ledger's `## Next Steps` says.
---

# Resume Plan

A plan keeps one `_PROGRESS.md` ledger **per worktree** working it, whose `## Next Steps` is the single
resolved next action kept current at every checkpoint (see `plans/AGENTS.md`). The ledger is 1:1 with a
worktree; a plan may have several. Resuming is landing in the right worktree, reading its ledger, and
executing that action — not choosing among paths or reconstructing it.

## Steps

1. **Resolve the ledger and its worktree.** `/resume-plan` takes a `_PROGRESS.md` ledger, a plan `.md`,
   or a worktree:
   - **a ledger** (`/resume-plan @plans/<X>_PROGRESS.md`) → read its `Worktree` header.
   - **a plan** (`/resume-plan @plans/<X>_PLAN.md`) → find every `plans/**/*_PROGRESS.md` whose `- Plan:`
     header names that plan. One → use it. Several (a plan worked in parallel worktrees) → list each
     with its worktree/branch and a one-line `## Next Steps` gist and ask which to resume — **unless the
     invocation also named a worktree**, then pick that one directly.
   - **nothing** → use the current worktree's ledger.

   Then `cd` to the resolved worktree before anything else — a fresh session may open elsewhere.
2. **Read in full:** `AGENTS.md`, `plans/AGENTS.md`, `plans/agents/PLAN.md`, the plan, and the resolved
   ledger.
3. **Confirm the ledger still holds** before acting: check its header branch/PR/gates against actual
   `git`/PR state and confirm `## Next Steps` still names one resolved action under `AGENTS.md`. Reconcile
   stale state or unresolved alternatives from current evidence and standing instructions, then update
   the ledger's current-state, `## Next Steps`, and event log before acting. For every package, PR,
   publication, or sync dependency, classify implementation and delivery separately. Do not preserve a
   stale hard blocker when source or an exact producer artifact now permits safe local preparation.
4. **Do what `## Next Steps` says,** honoring its prerequisites and gates.
5. **When `## Next Steps` is a hard stop:**
   - Do any safe, authorized work in the current scope that can remove it. Also check whether the gate
     blocks only delivery: if local implementation, tests, or review can proceed against an exact
     producer artifact, reconcile the ledger to actionable delivery-gated work and continue normally.
   - If it cannot move, make `## Next Steps` start with the exact single-line `Blocked:`,
     `Unblock action:`, and `Resume when:` fields from `plans/agents/PLAN.md`. If the same fields and
     evidence were already recorded, do not create another no-change blocker checkpoint.
   - **Existing owner:** establish the two-ledger return path before stopping. Report all three lines verbatim
     and name the owner, but emit no prompt; the owner surfaces this plan when the gate opens.
   - **Separate resolver needed:** emit a paste-ready prompt for that resolving task, naming the
     blocked ledger and return condition. Never emit the blocked plan's own resume pointer.
   - **User or external action needed:** tell the user the exact action and objective verification
     condition directly. Never emit the blocked plan's own resume pointer.

   A blocked plan's ordinary pointer is actively misleading: replaying it can only repeat the same
   audit. It becomes valid again only after evidence opens the gate and the ledger is reconciled to an
   actionable `## Next Steps`.

For the checkpoint procedure repository workflows apply when they advance plan work, see
[the plan-progress checkpoint](references/plan-progress-checkpoint.md).

If a plan genuinely has no ledger it predates the convention: create one from
[the progress template](assets/progress-template.md) as a labelled reconstructed baseline, then resume.
