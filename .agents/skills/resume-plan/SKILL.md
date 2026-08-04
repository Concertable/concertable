---
name: resume-plan
description: Resume plan-managed work from its ledger. Use when Tommy invokes `/resume-plan` or wants to pick a plan back up after a clear or handoff. Take an optional `plans/*.md` reference; otherwise use the current worktree's plan. `cd` to the plan's worktree, read AGENTS.md, plans/AGENTS.md, the plan and its `_PROGRESS.md` ledger, then do what the ledger's `## Next Steps` says.
---

# Resume Plan

Every plan keeps a companion `<PLAN_STEM>_PROGRESS.md` ledger whose `## Next Steps` is the authoritative
next action, kept current at every checkpoint (see `plans/AGENTS.md`). Resuming is landing in the right
worktree, reading the ledger, and doing what it says — not reconstructing it.

## Steps

1. **Land in the worktree.** With a plan reference (`/resume-plan @plans/.../PLAN.md`), resolve its
   worktree from `git worktree list` and `cd` there even when the session opened elsewhere; otherwise use
   the current worktree.
2. **Read in full:** `AGENTS.md`, `plans/AGENTS.md`, the plan, and its `<PLAN_STEM>_PROGRESS.md`.
3. **Confirm the ledger still holds** before acting: check its header branch/PR/gates against actual
   `git`/PR state, and if a remote transition (queued PR merged, package published, platform-sync) landed
   since the last checkpoint, update the ledger's current-state, `## Next Steps`, and event log first.
4. **Do what `## Next Steps` says,** honoring its prerequisites and gates.

For the checkpoint procedure repository workflows apply when they advance plan work, see
[the plan-progress checkpoint](references/plan-progress-checkpoint.md).

If a plan genuinely has no ledger it predates the convention: create one from
[the progress template](assets/progress-template.md) as a labelled reconstructed baseline, then resume.
