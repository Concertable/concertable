# Guidance-docs restructure — progress

- Plan: `plans/docs/GUIDANCE_DOCS_RESTRUCTURE_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/guidance-restructure`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-guidance-docs`
- Branch: `Docs/GuidanceDocsRestructure`
- PR: #637 (draft)
- Dependency/package gates: none — docs and one hook only; no `api/**` change, so no package publication or platform sync
- Last reconciled: 2026-08-17 against `origin/main` `dc037f477`, the repository tree, `.editorconfig`, the architecture tests, and the shipped frontend client/guard code

## Current state

Phases 1 and 2 are complete, committed, and pushed. The corpus is now *correct and indexed*; it is not
yet *reorganized* — no file has moved, and no topic file or per-consumer import exists yet.

An earlier pass of this analysis was run against a local `main` **610 commits stale** and had to be
discarded and redone against `origin/main`. Two of its conclusions were wrong in opposite directions:
the frontend-orphan problem was already fixed upstream (`app/AGENTS.md` plus a reachability hook), and
the `isAxiosError` question resolved the *opposite* way once checked against code. Every line reference
in the plan is now verified against `dc037f477`.

## Done

**Phase 2 — correctness** (`e5df43bd4`)

- Ten contradictions between loaded docs reconciled; six were settled by code or config rather than
  opinion. The sharpest: `DEBUGGING_CONVENTIONS.md` instructed an inline `logger.Log*` call that
  `CA1848 = error` rejects at build, in the doc the e2e debug skills read first.
- Root-relative `./e2e.ps1` / `./docker-health.ps1` corrected in root `AGENTS.md` (4) and the four e2e
  skills (39). Both scripts live only under `scripts/`.
- `Notification` no longer documented as an adapter service a data service may `WaitFor` — no such
  service exists, only `Concertable.Shared.Notification`.
- `Monitor` and branch-currency each now have one answer. The `Monitor` rule stayed in root rather than
  moving into the `merge` skill: root is always loaded and carries the rationale, so moving it would
  have traded correctness for token savings.
- Deleted `MM_NORTH_STAR.md` (423) and `MICROSERVICES_NORTH_STAR.md` (83). Authority was already
  settled at `MICROSERVICES_ARCHITECTURE.md:509`. Checking before inlining `MM_NORTH_STAR`'s
  corollaries caught one it had propagated into a linked, near-auto-loaded doc: `CONVENTIONS.md` taught
  that shared reference data FKs into `SharedDbContext`, but neither that context nor `GenreEntity`
  exists — `Genre` is a Contracts enum.
- Twelve dangling or misdirected references fixed, including `review/SKILL.md`, which still aimed
  Lens C at the renamed `MODULAR_MONOLITH_RULES.md` — collateral that had silently broken the lens.
- Five rotted citations stripped from `app/agents/CODE_PATTERNS.md`, and the axios instance names
  corrected to `apiClient`/`paymentClient`/`searchClient`.

**Phase 1 — index, meta-rules, and the machine check**

- `docs/INDEX.md`: topic → owning doc across process, architecture, backend and frontend; a table of
  what a machine already enforces and whether it fails a build; and ten rules for adding to the
  corpus. Linked from root `AGENTS.md` "Per-area guidance". All 44 links verified to resolve.
- `docs_reachability.py` extended — brought forward from "deferred" because without a machine check the
  dangling references fixed in Phase 2 just accumulate again. It now errors when a **guidance** doc
  links a non-existent file or uses a root-absolute `/api/...` path, and warns for `plans/`/`reviews/`,
  which are working docs that get deleted. Six tests added; suite green at 72.

Notable while building it: the check first reported 45 errors, which would have failed the gate for
everyone on pre-existing plan churn — hence the guidance/working-doc split. It also produced one false
positive (a shell regex `[/\\](bin|obj)[/\\]` matches the markdown link pattern), so the dead-link pass
skips fenced blocks. Reachability still scans them.

## Next Steps

The distribution mechanism is settled and built: **`tomjseery/standards-docs`** (private), a Claude Code
`/plugin marketplace add tomjseery/standards-docs`; refresh with `/plugin marketplace update`. That is the
"pulled locally and kept synced" requirement satisfied natively — no submodule, no `submodules: true` in
CI, no sync script, and no tech debt. It also supersedes the submodule design recorded earlier in the
plan: a carved-out service installs the same plugin and rewrites no paths.

A `SKILL.md` is a convention doc plus a `description` front-matter that routes it, so migration is
relocation, not rewriting. Landed so far: the marketplace scaffold, the `dotnet-standards` plugin, and the
`proto` skill — chosen because it is the clearest instance of the defect (one `.proto` in the repo, in
Payment, with its rules sitting in docs every `api/**` prompt loads).

Next, in order:

1. **`docs/standards-repo`** — migrate the rest of `dotnet-standards`: `csharp-style`, `csharp-naming`,
   `comments`, `dependency-injection`, `logging`, `validation`, `persistence`, `result-carriers`,
   `result-errors`, `result-terminals`, `http-api`, `module-structure`, `microservice-boundaries`,
   `seeding`, `unit-testing`, `integration-testing`, `e2e-scenarios`, plus the two remaining scoped
   topics `multitenancy` and `keyed-strategies`. Then `typescript-standards` and `agent-process`.
   Genericize every example — the `proto` skill needed a second pass because payment-domain names had
   leaked in, and a standard naming concrete types can't be reused.
2. **`docs/guidance-restructure`** — reduce `api/agents/*` and `app/agents/*` to the in-repo hard floor,
   and give each service a thin `CODE_CONVENTIONS.md`/`CODE_PATTERNS.md` holding only its own precedents
   (B2B's context roster and filtered-entity list, Payment's Refit roster and money rules, the `DealType`
   families). Nested `AGENTS.md` compose, so a service file must never restate the api-wide floor.
3. Collapse the duplication rows to one home each — seeding still sits in 5 places. Resolve
   `api/AGENTS.md:26-45` under the import-or-summarize rule: `SEEDING_CONVENTIONS.md` is not `@`-imported,
   which is *why* that inline summary exists.
4. Re-point `docs/INDEX.md` at the skills that own each topic and re-run the link check.

**Decide before step 2:** whether `api/agents/CONVENTIONS.md` becomes `MODULE_STRUCTURE.md`. It collides
with `CODE_CONVENTIONS.md`, reads as its superset when it is narrower, and its `:6`/`:91` "modules in the
monolith" framing contradicts `api/ARCHITECTURE.md:8`.

**Two things surfaced that are Tommy's to decide, not mine:**

- `tomjseery/agent-starter-kit` (public, 7 skills) looks redundant with `tomjseery/dotagents` (private,
  the same 7 plus `pull-main`, `sync-all`, `unmerged`) — the same duplication disease at repo level.
- Plugin skills are Claude-Code-specific, while this repo deliberately keeps `.agents/` canonical so
  Codex works too. Codex parity for the standards is unresolved; the content is plain markdown in a git
  repo, so a Codex-side pointer is possible but not yet designed.

**Correction to an earlier finding in this ledger:** the report that
`~/.claude/skills/{worktree,commit-push,…}` point at non-existent `.agents/skills/` directories was
misleading. Those stubs resolve — the canonical files live in `~/.agents/skills/`, synced from
`dotagents`. They are absent from *Concertable*, which is correct by design.

Not in scope: auto-load thinning beyond what scoping achieves, and the analyzer push-down including
`EnforceCodeStyleInBuild`.
