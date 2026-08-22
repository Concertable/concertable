# Docs review — Docs/docs_polyrepo-ready-n7a-plans-agents

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `9e739a54f3020671acbe83691b4f6dc861b2f0f6`  _(2026-08-22)_

> Range reviewed: `1452b5b8..9e739a54` (1 commit; the off-by-one line-count nit was folded into HEAD).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

**No findings.** All six lenses were checked against the full range and nothing cleared the ~80/100
confidence bar.

- **Lens A (accuracy / dead refs)** — clean. Every skill name the thinned `plans/AGENTS.md` points at
  resolves (`plans`, `handoff`, `plan-checkpoint`, `open-worktree`, `migrations`, `merge`, `failing-tests`,
  `packages`). Every path resolves: `../docs/REMOTE_VALIDATION.md`, `.agents/hooks/plan_graph.py`,
  `.agents/hooks/plan_handoff_stop.py`, `scripts/worktrees.ps1`, `plans/launch/LAUNCH_ROADMAP.md`,
  `./initial-migrations.ps1` (from `api/`). Suite names accurate — `Concertable.B2B.E2ETests` and the UI
  regress (`Concertable.B2B.E2ETests.Ui`) both exist. `docs_reachability.py` (AGENTS.md touched): **0
  errors** (29 pre-existing warn-only `plans/` link warnings, none introduced by this diff).
- **Lens B (contradiction)** — clean. The thinned file agrees with root `AGENTS.md`'s thin-pointer
  philosophy and with `PLANS.md`; it introduces no rule that contradicts a sibling doc.
- **Lens C (one-rule-one-home / no-rule-lost)** — clean. Each platform rule removed from the pre-thin file
  genuinely survives at its destination: plan-managed-PR-includes-plan+ledger + recovery anchor →
  `PLANS.md`; worktree recreate-from-default → `PLANS.md`; branch naming `<Type>/<Name>` → `WORKTREE.md` /
  `BRANCHING.md`; close/retire → `WORKTREE.md`; ledger template + checkpoint → `plan/CHECKPOINT.md`; red
  suite → `FAILING_TESTS.md`; published-contract-needs-own-plan → `PLANS.md` + `packages`; E2E queue tier →
  `merge` + `REMOTE_VALIDATION.md`. No rule left duplicated across both the hub and its owning skill.
- **Lens D (concision, harness-reloaded)** — clean. `plans/AGENTS.md` is an area AGENTS.md; the thin
  **removes** words (68 → 31 lines) and every surviving line carries a this-repo value or a pointer. No
  added narration or restatement.
- **Lens E (dangling / transient refs)** — clean. The durable `plans/AGENTS.md` cites no specific plan
  filename, "Phase N", or ticket; its only reference is the durable `LAUNCH_ROADMAP.md`.
- **Lens F (followable)** — clean. Every rule states its action plus owning pointer; no ambiguous
  referent or conflicting must/may.
