# Guidance-docs restructure — progress

- Plan: `plans/docs/GUIDANCE_DOCS_RESTRUCTURE_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/guidance-restructure`
- Also delivered by this ledger: roadmap item `docs/agent-standards`, now checked off
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-guidance-docs`
- Branch: `Docs/GuidanceDocsRestructure`
- PR: #637 (draft)
- Shared repos: `Concertable/agent-standards` (7 process skills) and `tomjseery/dotagents` (28 generic, synced to `~/.agents/skills/`), cloned at `C:\Users\TommySeery\source\repos\{agent-standards,dotagents}`
- Dependency/package gates: none — docs and one hook only; no `api/**` change, so no package publication or platform sync
- Last reconciled: 2026-08-17 against `origin/main` `dc037f477`, plus `agent-standards` `8c42daa`

## Current state

The corpus is **correct, indexed, and its generic half now lives outside the repo as `.agents`-canonical
skills**. What has not happened is the in-repo reduction: no `api/agents/*` or `app/agents/*` file has shrunk,
so every generic rule currently exists in two places — the skill and the doc — which is exactly what Phase 3b
ends. Until it runs, the doc is the one being read on every prompt, so the skills are additive weight, not yet
a saving.

An earlier pass of this analysis ran against a local `main` **610 commits stale** and was discarded and
redone against `origin/main`. Two of its conclusions were wrong in opposite directions: the frontend-orphan
problem was already fixed upstream (`app/AGENTS.md` plus a reachability hook), and the `isAxiosError`
question resolved the *opposite* way once checked against code.

## Done

**Phase 3a — the corpus moved out as `.agents`-canonical skills** (`agent-standards` `ffe5721`,
`dotagents` `00e02f9`)

- 35 skills, **split by whether the rule names this product** — the correction that matters here. 28 generic
  ones (C# style/naming, comments, DI, logging, validation, the three Result skills, persistence,
  multitenancy, keyed strategies, module structure, service boundaries, proto, HTTP contracts, seeding, the
  three test tiers, and the 8 TypeScript/React ones) name no project, so they live in `dotagents` and sync to
  `~/.agents/skills/` for every repo. The 7 process ones are Concertable-shaped — the merge queue and its
  platform-sync PR, the draft-PR/queue tiering, ROADMAP→PLAN→PROGRESS and the worktree scripts — so they stay
  in the org repo where carve-outs inherit them.
- **`.agents/skills/` is canonical in both**, with `.claude/skills/` and `plugins/*/skills/` as generated
  layout literally instead of taking the idea; that is fixed, and nothing is Claude-only.
- **The repo rename and org transfer that the prior ledger recorded as blocked went through**:
  `tomjseery/standards-docs` → `Concertable/agent-standards`. Those calls succeeded on retry.
- **The stub generator mirrors each canonical `description` and now fails rather than emit an unroutable
  stub.** That guard caught a live defect: a bare colon-space truncates an unquoted YAML scalar, so
  `module-structure` and `typescript-style` had no description to route on and would never have loaded, with
  nothing visibly wrong. Side benefit — the pre-existing command skills' stubs now carry real descriptions
  instead of boilerplate.
- Deployed: `~/.agents/skills/` holds 35 canonical skills and `~/.claude/skills/` their stubs, so the
  standards are live in every session now, not only committed.
- Two topics were deliberately **not** migrated, from applying the tier table rather than the topic list:
  "questions come before actions" and "act on reversible work" are always-applicable rules whose violation
  is silent, so a load-on-demand skill is the wrong tier — the task would have to summon them, and by then
  the miss has happened. They stay global, as does the comment *policy*; only the C# mechanics became a skill.
- Two naming departures from the plan's sketch, both recorded there: branch currency folded into `merging`
  (it is a pre-step of enabling auto-merge, not a topic), and no `reviews` skill — `review/SKILL.md` is
  lenses pointed at this repo's own docs, local by construction.
- Migration surfaced a duplicate inside the *shared* repo itself: `proto`'s `XMappers` and payload-naming
  sections restated what `csharp-naming` now owns. Collapsed to pointers — the rule the restructure exists
  to enforce applies to its own output.

**Phase 2 — correctness** (`e5df43bd4`)

- Ten contradictions between loaded docs reconciled; six settled by code or config rather than opinion. The
  sharpest: `DEBUGGING_CONVENTIONS.md` instructed an inline `logger.Log*` call that `CA1848 = error` rejects
  at build, in the doc the e2e debug skills read first.
- Root-relative `./e2e.ps1` / `./docker-health.ps1` corrected in root `AGENTS.md` (4) and the four e2e
  skills (39). Both scripts live only under `scripts/`.
- `Notification` no longer documented as an adapter service a data service may `WaitFor` — no such service
  exists, only `Concertable.Shared.Notification`.
- `Monitor` and branch-currency each now have one answer. The `Monitor` rule stayed in root rather than
  moving into the `merge` skill: root is always loaded and carries the rationale.
- Deleted `MM_NORTH_STAR.md` (423) and `MICROSERVICES_NORTH_STAR.md` (83). Checking before inlining
  `MM_NORTH_STAR`'s corollaries caught one it had propagated into a linked doc: `CONVENTIONS.md` taught that
  shared reference data FKs into `SharedDbContext`, but neither that context nor `GenreEntity` exists.
- Twelve dangling or misdirected references fixed, including `review/SKILL.md`, which still aimed Lens C at
  the renamed `MODULAR_MONOLITH_RULES.md` — collateral that had silently broken the lens.
- Five rotted citations stripped from `app/agents/CODE_PATTERNS.md`; axios instance names corrected.

**Phase 1 — index, meta-rules, and the machine check**

- `docs/INDEX.md`: topic → owning doc; a table of what a machine already enforces and whether it fails a
  build; ten rules for adding to the corpus. All 44 links verified.
- `docs_reachability.py` extended to error on a guidance doc linking a non-existent file or using a
  root-absolute `/api/...` path, warning for `plans/`/`reviews/`. Six tests added; suite green at 72.

## Next Steps

**Phase 3b — reduce the in-repo corpus to the hard floor.** Nothing gates it: the Codex-parity question the
prior ledger raised was never a real blocker, because `.agents/skills/` is agent-agnostic and `~/.agents/skills/`
is exactly what a Codex session reads. Cutting a generic body now removes it from neither tool.

Today `api/agents/*` + `app/agents/*` is **2,681 lines, of which 2,073 auto-load** (1,321 on every `api/**`
prompt via `api/AGENTS.md:3`, 752 on every frontend prompt via `app/AGENTS.md`). Target ≈330 in-repo. Per file,
what leaves and what stays:

| File | Now | Stays | What stays is |
|---|---|---|---|
| `api/agents/RESULT_PATTERN.md` | 614 | ~20 | Reunion version baseline, "don't redistribute through Kernel", the `Kernel.Functional` legacy debt |
| `api/agents/CODE_CONVENTIONS.md` | 414 | ~30 | B2B active-tenant naming, `IGeometryProvider`, the local context roster |
| `app/agents/CODE_CONVENTIONS.md` | 391 | ~70 combined | the 4 axios clients + `qs` serializer, tier map, `User`/`B2bIdentity`, `SharedPermissions`, `FormData` field names |
| `app/agents/CODE_PATTERNS.md` | 361 | ↑ | ↑ |
| `api/agents/CODE_PATTERNS.md` | 293 | ~70 | the real context roster, filtered/unfiltered entity list, `DealType` families + workflow steps, Refit inventory + `ITokenApi` captive-singleton caveat |
| `api/agents/INTEGRATION_CONVENTIONS.md` | 150 | ~40 | the fixture table, `Concertable.Testing.Integration` members, mock simulators, run commands |
| `api/agents/CONVENTIONS.md` | 142 | ~30 | renamed `MODULE_STRUCTURE.md`: project naming, tenant→`organization` routes, `Genre` as enum; `:6`/`:91` framing fixed |
| `api/agents/SEEDING_CONVENTIONS.md` | 113 | ~45 | the seed inventory — actual forbidden tables, simulator specifics, `TicketsSold` |
| `api/agents/MICROSERVICE_COMMUNICATION.md` | 81 | ~20 | per-service surface table, folded into `api/ARCHITECTURE.md` |
| `api/agents/UNIT_CONVENTIONS.md` | 80 | — | deleted |
| `api/agents/E2E_CONVENTIONS.md` | 37 | ~8 | baseline path + `SeedState` shape, folded into the suite's own doc |
| `api/agents/DEBUGGING_CONVENTIONS.md` | 5 | — | deleted; its separate existence is what let it contradict `CA1848` |

Each surviving file names the skills that own its generic half. Nested `AGENTS.md` compose, so a per-service
file must never restate the api-wide floor.

Then, in order:

1. **Phase 4** — collapse the duplication rows to one home each; seeding still sits in 5 places. Resolve
   `api/AGENTS.md:26–45` under meta-rule 7: `SEEDING_CONVENTIONS.md` is not `@`-imported, which is *why* that
   inline summary exists. **After 3b** — dedupeing into files 3b then restructures edits the same lines twice.
2. **Re-point `docs/INDEX.md`** at the skill that owns each topic, and re-run the link check.
3. **Make the 7 process skills concrete.** They were written generic for a shared repo; now that they live in
   the org repo they should name the real commands and paths — this repo's own delivery skills,
   `scripts/worktrees.ps1`, `scripts/e2e.ps1`, the platform-sync workflow — instead of describing them
   abstractly.

## Also Tommy's, not blocking

`tomjseery/agent-starter-kit` (public, 7 skills) looks redundant with `tomjseery/dotagents` (private, the
same 7 plus `pull-main`, `sync-all`, `unmerged`) — the same duplication disease at repo level.
