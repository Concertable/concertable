# Working in `plans/`

**The plan lifecycle itself is the `plans` skill** — what a roadmap, plan and progress ledger each are,
that git history is the archive, plans outliving their PR worktrees, the two dependency graphs, cross-plan
blockers as two-way handoffs, and the rename grep gate. Read it before working a plan. This file and the
[`agents/`](agents/) playbooks carry only what is true of *this* repo: its folder layout, its hooks, its
scripts, and its skill names.

One convention, three tiers, each with its own playbook: **ROADMAP → PLAN → PROGRESS**.

- **Roadmaps** (`<EPIC>_ROADMAP.md`) — an epic's living tracker; each item spins off its own plan.
  Here → [`agents/ROADMAP.md`](agents/ROADMAP.md).
- **Plans** (`<NAME>_PLAN.md`) — one buildable item, each with a `<NAME>_PROGRESS.md` ledger.
  Here → [`agents/PLAN.md`](agents/PLAN.md).

**Folder = roadmap/plan.** Each epic gets a folder under `plans/`; its roadmap and every plan it spins
off live inside it (`plans/<epic>/<EPIC>_ROADMAP.md`, `plans/<epic>/<NAME>_PLAN.md` + `<NAME>_PROGRESS.md`).
Standing reference/RFC docs keep a bare stem (no suffix). The `plans/` hub (this file) and `agents/`
playbooks stay at the top. A plan's worktree/branch is temporary execution state, normally named
`<Type>/<epic>_<name>` and safe to recreate from `origin/main` after the prior PR's worktree is removed.

## The plan graph is machine-checked

```bash
python .agents/hooks/plan_graph.py --root <absolute-worktree>
```

Run it after creating or changing plan graph metadata. Missing or broken links, malformed blockers,
missing reciprocal owner handoffs, and terminal owners with pending handoffs fail.

## Closing a plan-managed worktree

Every plan-managed PR must merge the current plan and progress ledger, so `main` is always the recovery
anchor. Once the PR merges:

```powershell
./scripts/worktrees.ps1 close -Worktree <path> -PullRequest <n> -PlanManaged
```

Then create a fresh worktree from current `origin/main` if work remains. A superseded no-PR branch uses
`./scripts/worktrees.ps1 retire` with its exact head and a retirement-evidence commit already on `main`.
Never commit observations after a PR's merged head, or retain its worktree as the only recovery copy.

## The handoff pointer is enforced by a hook

An actionable non-terminal ledger owned by the current or explicitly targeted worktree must end its final
response with the two-line plan pointer from [`../PROMPTS.md`](../PROMPTS.md). Reading or editing a foreign
owner's ledger during dependency or roadmap reconciliation does not claim that owner's handoff. The
exceptions — a registered in-flight owner wait, the four-line hard-blocker schema in
[`agents/PLAN.md`](agents/PLAN.md), or a human-gated `Paused:` line — emit no pointer.
`.agents/hooks/plan_handoff_stop.py` blocks one incomplete final response for Claude and Codex and supplies
the exact replacement handoff; its retry guard prevents recursive blocking.

## A red suite routes to this repo's debug skill

A failing test is never reported back and left there — the `failing-tests` skill owns the run → diagnose →
fix → re-run loop. Here it routes by tier:

- unit / integration → **`integration-debug`**
- API E2E → **`e2e-api-debug`**
- UI E2E → **`e2e-ui-debug`** (or **`e2e-debug`** to sweep both layers)

## Merge-queue E2E tier

The full E2E suites (API `Concertable.B2B.E2ETests` + the UI regress) are expensive and Docker-gated.
**The merge queue IS the E2E gate — never run E2E locally ahead of a merge**, and
[`.agents/skills/merge/SKILL.md`](../.agents/skills/merge/SKILL.md) Step 4 is the single source of truth
for which tier runs. A plan phase line or kickoff prompt that says "run the E2E regress" is **not** a
reason to duplicate the queue; it selects the queue's tier. The only local E2E is targeted diagnosis after
a queue failure, through the debug skills above. Gate ownership and this repo's commands:
[`../docs/REMOTE_VALIDATION.md`](../docs/REMOTE_VALIDATION.md).
