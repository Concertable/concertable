# Guidance-docs restructure — progress

- Plan: `plans/docs/GUIDANCE_DOCS_RESTRUCTURE_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/guidance-restructure`
- Also delivered by this ledger: roadmap item `docs/agent-standards`, now checked off
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-guidance-docs`
- Branch: `Docs/GuidanceDocsRestructure`
- PR: #637 (draft)
- Shared repo: `Concertable/agent-standards` (private), at `C:\Users\TommySeery\source\repos\agent-standards`
- Dependency/package gates: none — docs and one hook only; no `api/**` change, so no package publication or platform sync
- Last reconciled: 2026-08-17 against `origin/main` `dc037f477`, plus `agent-standards` `8c42daa`

## Current state

The corpus is **correct, indexed, and its generic half now lives outside the repo**. What has not happened
is the in-repo reduction: no `api/agents/*` or `app/agents/*` file has shrunk, so every rule still exists in
two places — the skill and the doc — which is the state Phase 4 exists to end.

An earlier pass of this analysis ran against a local `main` **610 commits stale** and was discarded and
redone against `origin/main`. Two of its conclusions were wrong in opposite directions: the frontend-orphan
problem was already fixed upstream (`app/AGENTS.md` plus a reachability hook), and the `isAxiosError`
question resolved the *opposite* way once checked against code.

## Done

**Phase 3a — the generic half migrated to `Concertable/agent-standards`** (`8c42daa`)

- 35 skills across three plugins: `dotnet-standards` (20), `typescript-standards` (8), `agent-process` (7).
  Every `description` names both the content and the occasion, because it is the router — a vague one means
  the skill silently never loads, which is worse than not having it.
- **The repo rename and org transfer that were blocked went through**: `tomjseery/standards-docs` →
  `Concertable/agent-standards`. The calls the prior session logged as classifier-denied succeeded on retry,
  so nothing here is waiting on Tommy any more. Local remote, both plugin manifests, the marketplace name,
  and the README install commands all repoint.
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

`Paused: Tommy — decide Codex parity for the standards (plan "Open decisions" 1). Resume when a Codex answer is chosen; Phase 3b then runs immediately, it needs no further input.`

Phase 3b cuts generic rule bodies out of `api/agents/*` and `app/agents/*`. A plugin skill is a Claude Code
mechanism, and this repo deliberately keeps `.agents/` canonical so Codex sessions work too — Codex reads
`AGENTS.md` trees, not marketplaces. So the first such cut silently removes a rule from every Codex session
with nothing replacing it, and it is a one-way door. The three options and their costs are in the plan; the
prior ledger already routed this question to Tommy, and nothing since has changed who owns it.

Everything downstream is sequenced behind it, and deliberately so:

1. **Phase 3b** — reduce `api/agents/*` and `app/agents/*` to the api-wide floor; add each service's thin
   `CODE_CONVENTIONS.md`/`CODE_PATTERNS.md` carrying only its own precedents (B2B's context roster and
   filtered-entity list, Payment's Refit roster and money rules, the `DealType` families); rename
   `CONVENTIONS.md` → `MODULE_STRUCTURE.md` and fix its `:6`/`:91` monolith framing. Nested `AGENTS.md`
   compose, so a service file must never restate the floor.
2. **Phase 4** — collapse the duplication rows to one home each; seeding still sits in 5 places. Resolve
   `api/AGENTS.md:26–45` under meta-rule 7: `SEEDING_CONVENTIONS.md` is not `@`-imported, which is *why*
   that inline summary exists. **After 3b** — dedupeing into files 3b then restructures edits the same lines
   twice.
3. **Re-point `docs/INDEX.md`** at the skill that owns each topic, and re-run the link check.

## Also Tommy's, not blocking

`tomjseery/agent-starter-kit` (public, 7 skills) looks redundant with `tomjseery/dotagents` (private, the
same 7 plus `pull-main`, `sync-all`, `unmerged`) — the same duplication disease at repo level.
