# Working in `plans/`

**The plan method is the `plans` skill and the handoff prompt's shape is the `handoff` skill** — what a
roadmap, plan and ledger each are, the phases and their verification gate, the required ledger headers and
sections, the lifecycle from writing a plan to closing it out, cross-plan blockers as two-way handoffs, the
four-line blocker schema, and the rename grep gate. Read them before working a plan. This file carries only
what is true of *this* repo: its folder layout, its hooks, its scripts, and its skill names.

**Folder = roadmap/plan.** Each epic gets a folder under `plans/`; its roadmap and every plan it spins off
live inside it (`plans/<epic>/<EPIC>_ROADMAP.md`, `plans/<epic>/<NAME>_PLAN.md` + `<NAME>_PROGRESS.md`).
Standing reference/RFC docs keep a bare stem (no suffix). A plan's worktree/branch is temporary execution
state, normally named `<Type>/<epic>_<name>` and safe to recreate from `origin/main` after the prior PR's
worktree is removed. New ledgers start from
[`resume-plan/assets/progress-template.md`](../.agents/skills/resume-plan/assets/progress-template.md);
the mandatory update procedure is
[`resume-plan/references/plan-progress-checkpoint.md`](../.agents/skills/resume-plan/references/plan-progress-checkpoint.md).

`plans/launch/LAUNCH_ROADMAP.md` is the driving roadmap for the current effort — most work traces back to one
of its items, so it is usually the one a landed change has to tick.

## The plan hooks are machine gates

```bash
python .agents/hooks/plan_graph.py --root <absolute-worktree>
```

Run it after creating or changing plan graph metadata. Missing or broken links, malformed blockers, missing
reciprocal owner handoffs, and terminal owners with pending handoffs fail.
`.agents/hooks/plan_handoff_stop.py` blocks one incomplete final response for Claude and Codex and supplies
the exact replacement handoff; its retry guard prevents recursive blocking.

## Closing a plan-managed worktree

Every plan-managed PR must merge the current plan and progress ledger, so `main` is always the recovery
anchor. Once the PR merges:

```powershell
./scripts/worktrees.ps1 close -Worktree <path> -PullRequest <n> -PlanManaged
```

Then create a fresh worktree from current `origin/main` if work remains. A superseded no-PR branch uses
`./scripts/worktrees.ps1 retire` with its exact head and a retirement-evidence commit already on `main`. A
close-out that only records terminal evidence goes on `Docs/<epic>_<name>_closeout` and lands through
`/merge-docs`.

## The repo's plan skills

`/resume-plan` resumes a ledger; `/continue-roadmap` creates the next roadmap item's plan. Review routes
`/review` or `/big-review`, then `/incremental-review` after later code commits; a docs/meta-only PR routes
`/docs-review` and `/merge-docs`. Each skill owns its own resolution rules — read it, don't infer them here.

## A red suite routes to a debug skill

A failing test is never reported back and left there — the `failing-tests` skill owns the run → diagnose →
fix → re-run loop, and its own tier table names the skill for each red suite.

## What this repo adds to a phase's verification gate

- A phase that changes the model ends with `./initial-migrations.ps1` from `api/` (re-scaffold, never
  additive migrations). A plan must not restate the local build/integration gate — inherit
  [`../docs/REMOTE_VALIDATION.md`](../docs/REMOTE_VALIDATION.md).
- **Merge-queue E2E tier.** The full E2E suites (API `Concertable.B2B.E2ETests` + the UI regress) are
  expensive and Docker-gated. **The merge queue IS the E2E gate — never run E2E locally ahead of a merge**,
  and the `merge` skill's Step 4 is the single source of truth for which tier runs. A plan phase line or
  kickoff prompt saying "run the E2E regress" **selects the queue's tier**; it is not a reason to duplicate the run. The only local E2E is targeted diagnosis after a
  queue failure, through that tier's debug skill.
- A refactor that changes a **published** `Concertable.*` contract is a breaking package change and cannot
  land in one PR — B2B and Customer compile against the published packages, not the source beside them (the
  carve: [`../api/ARCHITECTURE.md`](../api/ARCHITECTURE.md)). Capture it in its own plan; the expand/contract
  shape is the `plans` skill.
