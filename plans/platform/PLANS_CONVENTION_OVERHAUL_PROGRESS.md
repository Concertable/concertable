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

**Strategy (per Tommy): FULL migration + consumer handling before merge — do NOT de-scope, do NOT
merge blind.** Treat #346 as a breaking "version" bump of the plans convention: everything on `main`
must be in the new format AND the consumers (the ~15 in-flight worktrees whose ledgers point at moved
paths) must be handled first, exactly like the platform-sync gate handles package consumers. The
de-scope attempt was reversed — typed-result is back in-format (links fixed).

Collision sweep result (moved-plan files each branch edits): only 3 branches collide —
`Refactor/B2BTypedResultMigration` + `Feature/TypedResultMigrationPhase2` + `Docs/TypedResultPhaseOwnership`
on `TYPED_RESULT_MIGRATION.md` (R099, content-edited → real conflicts); `Feature/CommissionBindingDeferredPricing`
on `PLATFORM_COMMISSION.md`/`LAUNCH_CHECKLIST.md` (R100 pure renames → git follows cleanly). The
`ERROR_CASE_NAMES_PROGRESS.md` "deletion" was confirmed staleness.

**Worktree audit — 15/15 DONE.** Hot shared files = `TYPED_RESULT_MIGRATION.md` + `plans/AGENTS.md`
(~7 worktrees touch them). Classifications:
- **LAND-FIRST:** `Refactor/B2BTypedResultMigration` (16 commits, no PR), `Feature/CommissionBindingDeferredPricing`
  (PR #296, 123-file Payment feat), `Feature/PaymentOwnedResultExpansion` (45 commits + 12 uncommitted, no PR),
  `Docs/PlanNextStepsSinglePath` (PR #333).
- **CONFLICT-RISK:** `Feature/TypedResultMigrationPhase2` (PR #282), `Docs/TypedResultPhaseOwnership`,
  `Docs/TypedResultConventions`, `Feature/FrontendBuildSeparation` (unpushed `merge/SKILL.md`),
  `Feature/SelfBillingAgreement` (main checkout — 10 commits real code, no PR; `merge-tree` dry-run: PLAN.md
  auto-merges, but its 2 NEW `plans/b2b/SELF_BILLING_AGREEMENT*.md` files hit a b2b→launch **location**
  conflict + 3 internal refs to repoint. Per §6 this plan should have been created at `plans/launch/` — it wasn't).
- **REPOINT-AFTER:** `Docs/NaturalErrorCaseNames` (PR #343 merged). **CLEAN-FOLLOW:** `Chore/TechDebt` (#339),
  `Docs/TypedErrorRepresentation` (already merged — prune), `Plan/RepositoryPerMicroserviceMigration` (188 behind),
  `Refactor/DealStrategyCompletenessGuard` (1 unpushed test, no plan collision — own PR on its own schedule).
- **DEAD/PRUNE:** `Refactor/DerivedErrorDefinitions` (orphan root commit, feature already merged as PR #344).

**BLOCKED ON TOMMY'S DECISION — two forks presented, awaiting his pick:**
(1) *drain-then-bump* = land the 5 LAND-FIRST branches (he merges #296/#282/#333, OKs pushing the 2 no-PR
WIP branches), then #346 last — cleanest, most coordination, #346 re-merges after each; or
(2) *bump-in-a-window* = merge #346 in a quiet moment, CONFLICT-RISK/REPOINT-AFTER worktrees fix their
(mostly 1-line) ledger headers on next sync. Recommendation: fork 2 for all EXCEPT pushing/PR-ing the two
no-PR code branches (`B2BTypedResultMigration`, `PaymentOwnedResultExpansion`) which should land regardless.

Next action when he answers: execute the chosen fork. Independently still to do before #346 merges:
backfill `_PROGRESS` ledgers for the 11 `_PLAN` files lacking one, verify skills/agent files, re-run the
grep+link gate. Do NOT merge #346 or any other PR / commit others' WIP without his explicit go-ahead.

On merge: delete this plan + ledger in close-out; REPOINT-AFTER worktrees fix their ledger `- Plan:`
line on next sync; rename worktrees to `<Type>/<epic>_<name>` (only after merge).

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
