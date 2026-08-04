# Plans convention overhaul — progress

- Plan: `plans/platform/PLANS_CONVENTION_OVERHAUL_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Docs\PlansConventionOverhaul`
- Branch: `Docs/PlansConventionOverhaul`
- PR: not opened
- Dependency/package gates: none (docs + skills markdown only; no `api/**`, no build, no platform-sync).

## Current state

Reorg + link-fix + convention-text edits **complete** and committed. 31 files relocated via `git mv`
(history preserved) into `launch/ typed-result/ marketplace/ platform/`; all links rewritten by a
path-aware fixer + verified (no new danglers); convention prose updated in `plans/AGENTS.md`,
`agents/PLAN.md`, `agents/ROADMAP.md`, `resume-plan` SKILL + checkpoint + template, `continue-roadmap`
SKILL, root `AGENTS.md`, `PROMPTS.md`, and the user-global `worktree` skill (outside this repo/PR).

## Next Steps

Open the PR (docs/skills only → `skip-e2e` eligible). Separately, once merged, existing sibling
worktrees can be renamed to `<Type>/<epic>_<name>` and resynced — NOT before merge (their branches
still carry the old plan names until they pull this in; renaming first creates a fresh mismatch).

## Completed work

- Convention decided (ROADMAP → PLAN → PROGRESS; folder=roadmap/plan; worktree `<Type>/<epic>_<name>`).
- Full `plans/` tree mapped + classified; skill/doc reference sweep done.
- Overhaul plan + ledger authored.

## Verification

Docs/skills-only; gate = the §7 grep sweep (zero stale paths/stems outside the rename table + listed
pre-existing danglers) + every `plans/`/`@plans/` link resolves + `git mv` renames (history preserved).
`skip-e2e` eligible.

## Decisions, discoveries, blockers, and deviations

- Full reorg chosen over convention-only: leaving files inconsistent with the updated doc is the exact
  confusion risk to avoid.
- `TYPED_RESULT_MIGRATION` → `_ROADMAP` by direction, though structured as a plan; rename-only this PR,
  content split flagged (plan §6). Many typed-result worktrees in flight.
- Load-bearing `_PROGRESS.md` suffix + `plans/` root preserved (stop hook / resume-plan / CI globs).
- No deletions, no ledger backfill, pre-existing danglers left (plan §6).
- `worktree` skill + stop hook are outside the repo PR (plan §6).

## Event log

### 2026-08-04 — Plan authored

- Action: created `Docs/PlansConventionOverhaul` worktree off `origin/main`; wrote the overhaul plan +
  this ledger in `plans/platform/`.
- Outcome: hit list enumerated; ready to execute the moves.
- Follow-up: run plan §7.
