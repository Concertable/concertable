# Plans convention overhaul — enforce ROADMAP → PLAN → PROGRESS everywhere

Make the whole `plans/` tree obey one convention, and codify it in every doc/skill that governs plans
so **every future plan follows it automatically**. Today the tree is inconsistent: only 2 of ~20 plans
carry the `_PLAN` suffix the playbook already mandates, one roadmap-scale doc isn't named `_ROADMAP`, a
ledger stem drifts, ~15 plan/reference docs sit loose at `plans/` root, and there are 8 dangling
cross-links. That inconsistency is the confusion risk — the doc says one thing, the files do another.
This PR removes it in one pass: reorganise + rename every file, fix every link, and update the
governing docs/skills together so they match reality.

Ledger: [`PLANS_CONVENTION_OVERHAUL_PROGRESS.md`](./PLANS_CONVENTION_OVERHAUL_PROGRESS.md).

---

## 1. The convention (final)

Three tiers, top to bottom:

- **ROADMAP** — `<EPIC>_ROADMAP.md`. Living epic tracker (✅/🔴/🟠/🟡). No ledger. Never deleted. Spins
  off plans; keeps the tick when a plan ships.
- **PLAN** — `<NAME>_PLAN.md`. Multi-phase working doc for one buildable item, spun off from a roadmap
  item. Deleted when its lifecycle is terminal.
- **PROGRESS** — `<NAME>_PROGRESS.md`. The plan's operational ledger, 1:1 with its worktree. Same
  `<NAME>` stem as its `_PLAN`.

**Folder = roadmap/plan.** Each epic gets a folder under `plans/`; its roadmap and every plan it spins
off live inside it:

```
plans/
  <epic>/
    <EPIC>_ROADMAP.md          # the roadmap (one per folder)
    <NAME>_PLAN.md             # a plan spun off from a roadmap item
    <NAME>_PROGRESS.md         # that plan's ledger
    <REF>.md                   # standing reference/RFC docs (bare stem — NOT plans)
  agents/                      # the playbooks (PLAN.md, ROADMAP.md) — unchanged
  AGENTS.md / CLAUDE.md        # the plans/ hub — stays at plans/ root
```

**Worktree/branch name = `<Type>/<epic>_<name>`**, matching the `_PROGRESS` stem, so branch, worktree
folder, plan, and ledger all carry the same identity (`Feature/Launch_SelfBillingAgreement` ↔
`plans/launch/SELF_BILLING_AGREEMENT_PROGRESS.md`). The ledger's `- Plan:`/`- Worktree:`/`- Branch:`
header records the binding; `/resume-plan` and `/continue-roadmap` resolve off it.

**Load-bearing tokens that do NOT change:** the `_PROGRESS.md` suffix and the `plans/` root — the stop
hook, `resume-plan`, the checkpoint, `continue-roadmap`, and CI globs key off both. `_PLAN`/`_ROADMAP`
suffixes and folder moves layer on top safely.

## 2. Folder taxonomy (this repo)

| Folder | Epic / area | Roadmap |
|---|---|---|
| `plans/launch/` | B2B launch epic (was `plans/b2b/`) | `LAUNCH_ROADMAP.md` |
| `plans/typed-result/` | Typed-result migration epic | `TYPED_RESULT_MIGRATION_ROADMAP.md` |
| `plans/marketplace/` | Customer marketplace (future, was `plans/customer/`) | none yet (future `MARKETPLACE_ROADMAP.md`) |
| `plans/platform/` | Infra / deployment / architecture / testing / tooling — standing plans + reference docs not tied to a product epic | none (area folder, not an epic) |
| `plans/agents/` | Playbooks (`PLAN.md`, `ROADMAP.md`) — unchanged | — |

## 3. Rename map (every hit)

### → `plans/launch/` (from `plans/b2b/` + launch-related root files)
| Current | New | Class |
|---|---|---|
| `b2b/LAUNCH_ROADMAP.md` | `launch/LAUNCH_ROADMAP.md` | ROADMAP |
| `b2b/LAUNCH_CHECKLIST.md` | `launch/LAUNCH_CHECKLIST.md` | reference (bare) |
| `b2b/PLATFORM_COMMISSION.md` | `launch/PLATFORM_COMMISSION_PLAN.md` | PLAN |
| `b2b/PLATFORM_FEE_STORAGE_INVESTIGATION.md` | `launch/PLATFORM_FEE_STORAGE_INVESTIGATION.md` | reference (bare) |
| `b2b/DEAL_RENAME.md` | `launch/DEAL_RENAME_PLAN.md` | PLAN |
| `b2b/DEAL_STRATEGY_REGISTRATION.md` | `launch/DEAL_STRATEGY_REGISTRATION_PLAN.md` | PLAN |
| `MONEY_VALUE_TYPE.md` | `launch/MONEY_VALUE_TYPE_PLAN.md` | PLAN (enables commission) |
| `MANAGER_FRONT_PAGE_PLAN.md` | `launch/MANAGER_FRONT_PAGE_PLAN.md` | PLAN (already suffixed) |
| `MANAGER_FRONT_PAGE_FEEDBACK.md` | `launch/MANAGER_FRONT_PAGE_PROGRESS.md` | PROGRESS (de-facto ledger) |

### → `plans/typed-result/`
| Current | New | Class |
|---|---|---|
| `TYPED_RESULT_MIGRATION.md` | `typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md` | ROADMAP (promoted — see §6) |
| `TYPED_RESULT_MIGRATION_CONVENTIONS_PROGRESS.md` | `typed-result/TYPED_RESULT_MIGRATION_PROGRESS.md` | PROGRESS (stem realigned) |

### → `plans/marketplace/` (from `plans/customer/`)
| Current | New | Class |
|---|---|---|
| `customer/MARKETPLACE_PLAN.md` | `marketplace/MARKETPLACE_PLAN.md` | PLAN (already suffixed) |

### → `plans/platform/`
| Current | New | Class |
|---|---|---|
| `CONFIG_AND_DEPLOYMENT.md` | `platform/CONFIG_AND_DEPLOYMENT_PLAN.md` | PLAN |
| `CONFIG_STRATEGY.md` | `platform/CONFIG_STRATEGY.md` | reference (bare) |
| `DEPLOYMENT.md` | `platform/DEPLOYMENT.md` | reference/runbook (bare) |
| `DOMAINS_AND_DNS.md` | `platform/DOMAINS_AND_DNS.md` | reference (bare) |
| `PIPELINE_REDESIGN.md` | `platform/PIPELINE_REDESIGN_PLAN.md` | PLAN |
| `POLYREPO.md` | `platform/POLYREPO.md` | reference (bare) |
| `POLYREPO_FULLSTACK.md` | `platform/POLYREPO_FULLSTACK_PLAN.md` | PLAN |
| `POLYREPO_FULLSTACK_PROGRESS.md` | `platform/POLYREPO_FULLSTACK_PROGRESS.md` | PROGRESS |
| `MICROSERVICE_STEPS.md` | `platform/MICROSERVICE_STEPS_PLAN.md` | PLAN |
| `MICROSERVICE_STEPS_CONT.md` | `platform/MICROSERVICE_STEPS_CONT_PLAN.md` | PLAN |
| `RUST_DEAL_MICROSERVICE.md` | `platform/RUST_DEAL_MICROSERVICE_PLAN.md` | PLAN |
| `SHARED_TEST_LIBS_PACKAGING.md` | `platform/SHARED_TEST_LIBS_PACKAGING.md` | reference/RFC (bare) |
| `E2E_FAST_FORWARD_REFACTOR.md` | `platform/E2E_FAST_FORWARD_REFACTOR_PLAN.md` | PLAN |
| `E2E_HARNESS_RENAME.md` | `platform/E2E_HARNESS_RENAME_PLAN.md` | PLAN |
| `SPLIT_TIME_E2E_STRATEGY.md` | `platform/SPLIT_TIME_E2E_STRATEGY.md` | reference (bare) |
| `VERIFY_ACCEPT_CONVERGENCE.md` | `platform/VERIFY_ACCEPT_CONVERGENCE_PLAN.md` | PLAN |
| `WORKFLOW_STEP_NAMING.md` | `platform/WORKFLOW_STEP_NAMING_PLAN.md` | PLAN |
| `EMPTY_STRING_ELIMINATION.md` | `platform/EMPTY_STRING_ELIMINATION_PLAN.md` | PLAN |
| `TECHNICAL_DEBT.md` | `platform/TECHNICAL_DEBT.md` | reference (bare, likely stale) |
| _(new)_ | `platform/PLANS_CONVENTION_OVERHAUL_PLAN.md` (this) + `_PROGRESS.md` | PLAN + PROGRESS |

### Unchanged (plans/ hub + playbooks)
`plans/AGENTS.md`, `plans/CLAUDE.md`, `plans/agents/PLAN.md`, `plans/agents/ROADMAP.md` — stay in place;
content edited (§5), not moved.

## 4. Link fixes (every reference the moves break)

**Inside `plans/` (relative + `@plans/` links, and every ledger `- Plan:` header):** all cross-links in
the deployment cluster (`CONFIG_AND_DEPLOYMENT`↔`DEPLOYMENT`↔`DOMAINS_AND_DNS`↔`CONFIG_STRATEGY`), the
e2e/microservice/polyrepo/money/commission clusters, `LAUNCH_ROADMAP` → its plans, each `_PROGRESS`
`- Plan:` header, and `SELF_BILLING_AGREEMENT_PLAN` → its progress/roadmap. Fixed by a path-rewrite pass
+ grep gate (§7).

**Outside `plans/`:**
- Root `AGENTS.md` — "Plans (`plans/*.md`)" section + `@plans/<PLAN>.md` / `<PLAN_STEM>_PROGRESS.md` refs.
- `PROMPTS.md` — the plan handoff template (`@plans/<PLAN>.md` + `<ledger>_PROGRESS.md`).
- `.github/workflows/test.yml` (L~893) and `enable-auto-merge.yml` (L~10) — `plans/PIPELINE_REDESIGN.md`
  → `plans/platform/PIPELINE_REDESIGN_PLAN.md`. (`^plans/` path-trigger regexes are unaffected — still
  under `plans/`.)
- `.agents/skills/package-cutover/SKILL.md` refs `plans/agents/PLAN.md` + `plans/AGENTS.md` — **unchanged
  paths**, no edit.
- `.claude/skills/*` stubs — pointers only, no edit.

## 5. Doc/skill convention edits (make the rules match)

- `plans/AGENTS.md` — describe the 3-tier hierarchy + folder=roadmap/plan + worktree naming.
- `plans/agents/PLAN.md` — `<NAME>_PLAN.md` naming, folder placement, ledger `<NAME>_PROGRESS.md`,
  worktree/branch = `<Type>/<epic>_<name>`; keep the backward-compat clause for legacy ledger-less plans.
- `plans/agents/ROADMAP.md` — folder-per-epic, `<EPIC>_ROADMAP.md`, example paths.
- `.agents/skills/resume-plan/SKILL.md` + `references/plan-progress-checkpoint.md` +
  `assets/progress-template.md` — `_PLAN` resolution, same-stem ledger derivation, worktree/branch
  naming fields.
- `.agents/skills/continue-roadmap/SKILL.md` — mint `plans/<epic>/<NAME>_PLAN.md` + `_PROGRESS.md` and a
  `<Type>/<epic>_<name>` branch when spinning a plan off a roadmap item.

## 6. Out of scope / flagged (not silently downgraded)

- **`TYPED_RESULT_MIGRATION` promoted to a roadmap** per direction, but it currently reads as a phased
  *plan* with a ledger. This PR renames only (→ `_ROADMAP` + realigned `_PROGRESS`); splitting the epic
  tracker from the active-plan content is left as a flagged follow-up. Multiple typed-result worktrees
  are in flight, so their local ledgers' `- Plan:` paths will need repointing when they next sync.
- **`SELF_BILLING_AGREEMENT` is not on `main`** — that plan only lived on the scrapped branch (never
  merged), so it is not part of this PR. It will be (re)created under this convention as
  `plans/launch/SELF_BILLING_AGREEMENT_PLAN.md` + `_PROGRESS.md` wherever that work continues.
- **No deletions.** Stale-looking plans (`MANAGER_FRONT_PAGE*`, `TECHNICAL_DEBT`, near-terminal
  `DEAL_RENAME`) are renamed, not removed — lifecycle close-out is separate judgement.
- **No ledger backfill.** The ~15 plans with no `_PROGRESS` stay ledger-less; PLAN.md's backward-compat
  clause governs them.
- **8 pre-existing dangling links** (`PLATFORM_PACKAGE_SYNC`, `PIPELINE_PROBLEMS`, `PDF_RENDERER_RENAME`,
  `RUST_CONTRACT_MICROSERVICE`, `USER_MODEL_PLAN`, `STEP_7_PLAN`, `POLYREPO_COMPLETION`,
  `CONTRACT_LIFECYCLE_FSM`) are noted, not invented; the `platform-sync.yml` → `PLATFORM_PACKAGE_SYNC.md`
  dangler is left as-is.
- **User-global `worktree` skill** (`~/.agents/skills/worktree/SKILL.md`) and the **local stop hook**
  (`.claude/hooks/handoff-stop-check.py`) are outside this repo/PR. The hook needs no change (robust to
  `_PLAN`/`_ROADMAP`/folder moves; keys off `plans/**/*_PROGRESS.md` + `## Next Steps`). The worktree
  skill's `<epic>_<name>` naming rule is applied as a separate local edit.

## 7. Phases + verification

1. **Reorg** — create folders; `git mv` every file per §3 (preserves history).
2. **Links** — rewrite all references per §4; edit convention docs/skills per §5.
3. **Gate:**
   - `grep -rniE` over the repo for every old path/stem → zero hits outside this plan's own rename table
     and the explicitly-listed pre-existing danglers.
   - No new dangling `plans/...` or `@plans/...` link (every referenced file resolves).
   - `git mv` used throughout (history preserved); `git status` shows renames, not delete+add.
   - Docs-only/skills-only change — no build. `skip-e2e` eligible (no code/package/runtime surface).
4. **PR** — one PR, all of it.

## 8. Completion criteria

Every plan is `<NAME>_PLAN.md`, every roadmap `<EPIC>_ROADMAP.md`, every ledger `<NAME>_PROGRESS.md`,
all under a roadmap/area folder with nothing loose at `plans/` root; all links resolve; the governing
docs/skills describe exactly this; and the worktree/branch naming rule is in place. This plan +
ledger are deleted in the close-out once the PR merges.
