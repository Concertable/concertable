---
name: resume-plan
description: Resume plan-managed work from its ledger. Use when Tommy invokes `/resume-plan` or wants to pick a plan back up after a clear or handoff. Take an optional reference — a `_PROGRESS.md` ledger, a plan `.md`, and/or a worktree (by absolute path, partial path, or branch), in any combination; otherwise use the current worktree's ledger. `cd` to the resolved worktree, read AGENTS.md, plans/AGENTS.md, the plan and its ledger, then do what the ledger's `## Next Steps` says.
---

# Resume Plan

A plan keeps one `_PROGRESS.md` ledger **per worktree** working it, whose `## Next Steps` is the
authoritative next action kept current at every checkpoint (see `plans/AGENTS.md`). The ledger is 1:1
with a worktree; a plan may have several. Resuming is landing in the right worktree, reading its ledger,
and doing what it says — not reconstructing it.

## Steps

1. **Resolve the target, then `cd` to its worktree.** `/resume-plan` accepts — in any combination — a
   `_PROGRESS.md` ledger, a plan `.md`, and/or a **worktree**. A worktree may be named any way: an
   absolute path, a partial/relative path (e.g. `Concertable.worktrees/Feature/X`), or just its branch
   (`Feature/X`); resolve it against `git worktree list --porcelain`, matching the branch or a path suffix.
   - **A worktree is named** (alone, or alongside a plan/ledger) → it is the target. Its ledger is the
     `_PROGRESS.md` there whose `- Worktree:` header points at it; if a plan or ledger was also named, use
     that to pick when the worktree holds more than one. Resolve immediately — **do not ask**.
   - **A ledger, no worktree** (`@plans/<X>_PROGRESS.md`) → read its `- Worktree:` header for the target.
   - **A plan, no worktree** (`@plans/<X>.md`) → find every `plans/**/*_PROGRESS.md` whose `- Plan:` header
     names that plan. One → use it. Several (parallel worktrees) → list each with its worktree/branch and a
     one-line `## Next Steps` gist and ask which to resume.
   - **Nothing** → use the current worktree's ledger.

   `cd` to the resolved worktree before anything else — a fresh session may open elsewhere.
2. **Read in full:** `AGENTS.md`, `plans/AGENTS.md`, the plan, and the resolved ledger.
3. **Confirm the ledger still holds** before acting: check its header branch/PR/gates against actual
   `git`/PR state, and if a remote transition (queued PR merged, package published, platform-sync) landed
   since the last checkpoint, update the ledger's current-state, `## Next Steps`, and event log first.
4. **Do what `## Next Steps` says,** honoring its prerequisites and gates.

For the checkpoint procedure repository workflows apply when they advance plan work, see
[the plan-progress checkpoint](references/plan-progress-checkpoint.md).

If a plan genuinely has no ledger it predates the convention: create one from
[the progress template](assets/progress-template.md) as a labelled reconstructed baseline, then resume.
