# Docs review — Docs/polyrepo_agents-floor-sequencing

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `9506b03a160c` _(2026-08-20)_

> Range reviewed: `origin/main...HEAD` (the branch's own change vs current `origin/main` — three plan/roadmap files).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Scope

`plans/platform/POLYREPO_ROADMAP.md`, `plans/docs/POLYREPO_READY_PLAN.md`,
`plans/docs/POLYREPO_READY_PROGRESS.md`. Meta-only; base is `origin/main` (local `main` is stale, so
`merge-base main HEAD` overstates the range).

## Findings

No issues found. Checked accuracy vs reality (both new links — `plans/launch/LAUNCH_ROADMAP.md`,
`plans/docs/POLYREPO_READY_PLAN.md` — resolve; `skill_router.py --skills-for` runs as cited; `dotnet-standards`,
`~/.agents/skills`, `.agents/skill-routes.json` and the `api/AGENTS.md` 78-line-pointer fact all match the
repo), cross-doc contradiction (the N7 split is reconciled with the ledger's Next Step 5), doc home &
convention, harness-reloaded concision (all three files are plan/roadmap docs, not every-prompt loads),
dangling references, and followable instruction (acceptance criterion 2 states its pass conditions).
