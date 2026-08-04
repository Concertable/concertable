# Plans convention overhaul — progress

- Plan: `plans/platform/PLANS_CONVENTION_OVERHAUL_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Docs\PlansConventionOverhaul`
- Branch: `Docs/PlansConventionOverhaul`
- PR: #346 (open, `skip-e2e` labelled)
- Dependency/package gates: none (docs + skills markdown only; no `api/**`, no build, no platform-sync).

## Current state

Reorg + link-fix + convention-text edits **complete** and committed. 31 files relocated via `git mv`
(history preserved) into `launch/ typed-result/ marketplace/ platform/`; all links rewritten by a
path-aware fixer + verified (no new danglers); convention prose updated in `plans/AGENTS.md`,
`agents/PLAN.md`, `agents/ROADMAP.md`, `resume-plan` SKILL + checkpoint + template, `continue-roadmap`
SKILL, root `AGENTS.md`, `PROMPTS.md`, and the user-global `worktree` skill (outside this repo/PR).

## Next Steps

**DO NOT MERGE #346 YET — unresolved cross-worktree blast radius.** This PR renames/moves ~30 plan
files (see the rename table). There are **15 other live worktrees** (`git worktree list`), many on
typed-result / docs-error / plan work that edits these exact files. Merging risks: (a) every in-flight
ledger's `- Plan:` path string goes stale after it syncs `main`; (b) rename-vs-edit merge conflicts on
branches that edited a moved plan. Also this PR's diff vs `origin/main` currently shows it **deleting**
`plans/TYPED_RESULT_MIGRATION_ERROR_CASE_NAMES_PROGRESS.md` (owned by `Docs/NaturalErrorCaseNames`) —
almost certainly just staleness (branch fell behind `main` again), but MUST be confirmed, not assumed.

Concrete next actions, in order:
1. In this worktree: `git merge origin/main` (behind again), rebuild nothing (docs only), and confirm
   the `D …ERROR_CASE_NAMES_PROGRESS.md` disappears — i.e. it was staleness, not a real deletion. If it
   still shows deleted, that file must be restored before any merge.
2. Run the per-branch collision sweep: for each of the 15 worktrees,
   `git diff --name-only origin/main...<branch> -- plans/` intersected with this PR's renamed/deleted
   source paths. Zero-collision branches only need a one-line ledger `- Plan:` repoint on their next
   sync; colliding branches (edited a moved plan) need a decision.
3. Take the result to Tommy with a recommendation: **merge-now-and-repoint** (if collisions are only
   stale path strings), **hold** (if several branches are mid-edit on moved plans), or **de-scope** the
   actively-edited plan files from this PR's move. Do not self-merge.

On eventual merge: delete this plan + ledger in close-out; rename/resync sibling worktrees to
`<Type>/<epic>_<name>` (only after merge — renaming first creates a fresh mismatch).

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

### 2026-08-04 — Lifecycle close-out: deleted 3 terminal docs

- Action: ran a codebase-verified terminal-status triage over all 25 non-roadmap plan/reference docs
  (per Tommy folding stale-plan deletion into this PR).
- Result: deleted 3 (`DEAL_RENAME_PLAN`, `WORKFLOW_STEP_NAMING_PLAN`, `PLATFORM_FEE_STORAGE_INVESTIGATION`)
  — all shipped/concluded, zero live inbound links. Trimmed one stale sibling-ref sentence in
  `E2E_HARNESS_RENAME_PLAN.md`.
- Kept despite reading terminal: `MICROSERVICE_STEPS_PLAN` (canonical migration-order ref for live
  `api/docs` + active CONT plan — deleting would dangle 6 links), `POLYREPO` (live-mirror runbook).
- Everything else has real outstanding work (triage confirmed against code). Gate re-run: no refs to
  deleted docs, no new danglers.

### 2026-08-04 — Ran the §7 grep gate for real; fixed one miss

- Action: executed the full §7 sweep (never actually recorded before). Repo-wide grep for stale
  `plans/b2b|customer` folders, old root stems, `@plans/` old paths + a link-resolution pass over every
  `plans/**/*.md` target.
- Findings: one genuine miss — `api/docs/MICROSERVICES_ARCHITECTURE.md:9` had 3/4 links updated but left
  `USER_MODEL_PLAN.md` on the dead `/plans/b2b/` folder. Fixed → `/plans/launch/` (target itself is a
  listed pre-existing dangler).
- Allowlisted survivors (recorded in plan §6): `reviews/*.md` historical watermarks (archival, keep old
  paths), in-flight `TYPED_RESULT_MIGRATION_DERIVED_CODES_PROGRESS.md` (owned by another worktree).
- Outcome: gate green — zero stale refs outside the rename table + explicit allowlist; all live
  `plans/` links resolve.

### 2026-08-04 — Merged main, opened PR #346

- Action: merged `origin/main` (was 7 behind). One conflict in
  `typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md` — main added derived-code + Phase 2 ledger pointers
  to the same line this branch rewrote for the rename; resolved by keeping both pointers.
- Decision: new loose `plans/TYPED_RESULT_MIGRATION_DERIVED_CODES_PROGRESS.md` (landed on main) left at
  root — it's an in-flight ledger for the `Refactor/DerivedErrorDefinitions` worktree; moving it now
  creates the fresh mismatch §6 warns against. Repointed on its own sync.
- Action: pushed (current with main, 0 behind); opened PR #346 + `skip-e2e` label.
- Outcome: PR open, awaiting merge go-ahead.
