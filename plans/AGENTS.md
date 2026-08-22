# Working in `plans/`

**The plan method is the `plans` skill and the handoff prompt's shape is the `handoff` skill** — the
roadmap/plan/ledger convention, phases and their verification gate, the ledger's required sections, the
lifecycle from writing a plan to closing it out, cross-plan blockers, and the rename grep gate all live
there; the checkpoint procedure and ledger template are the `plan-checkpoint` skill. Read them before
working a plan. This file carries only what is true of *this* repo: its plan layout, hook and script
paths, and its suite names.

## This repo's values

- **Layout** — epics live under `plans/<epic>/` (`<EPIC>_ROADMAP.md`, `<NAME>_PLAN.md` +
  `<NAME>_PROGRESS.md`); standing reference/RFC docs keep a bare stem. `plans/launch/LAUNCH_ROADMAP.md`
  is the driving roadmap most work traces back to.
- **Hooks** — `python .agents/hooks/plan_graph.py --root <absolute-worktree>` after changing plan graph
  metadata; `.agents/hooks/plan_handoff_stop.py` is the handoff Stop hook. The gates they enforce are the
  `plans` and `handoff` skills.
- **Closing a worktree** — `./scripts/worktrees.ps1 close -Worktree <path> -PullRequest <n> -PlanManaged`
  once the PR merges (`retire` for a superseded no-PR branch); the procedure is the `open-worktree` skill,
  and a terminal-only close-out rides `Docs/<epic>_<name>_closeout` through `/merge-docs`.

## This repo's phase verification additions

- A model-changing phase re-scaffolds via `./initial-migrations.ps1` from `api/` (the `migrations` skill),
  never an additive migration. Inherit the build/integration gate from
  [`../docs/REMOTE_VALIDATION.md`](../docs/REMOTE_VALIDATION.md) — don't restate it.
- **Merge-queue E2E tier** — the full suites (`Concertable.B2B.E2ETests` + the UI regress) are the merge
  queue's gate, selected by the `merge` skill's Step 4; never run them locally ahead of a merge. A red
  suite routes to the `failing-tests` skill's tier table — the only local E2E.
- A change to a **published** `Concertable.*` contract is a breaking package change (the `packages` skill)
  and needs its own plan; it cannot land in one PR.
