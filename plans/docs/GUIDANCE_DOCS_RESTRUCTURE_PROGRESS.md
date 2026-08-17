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

Design corrected mid-turn on two counts from Tommy, both recorded in the plan:

- **Scope is a fourth axis, and it is the one that governs bloat.** Folders can't express it; only the
  `@`-import edge can. Verified: ~200 lines of always-loaded content is single-service — tenancy
  composition (62, B2B), keyed `DealType` strategies (90, B2B), gRPC boundaries (34, Payment), proto
  naming (~12, Payment, and exactly one `.proto` exists). Auth, Search and Customer pay for all of it
  every prompt. The model is granular topic files composed per consumer, generalizing the 42
  test-project stubs that already `@`-import a single `TESTING_*` file.
- **"portable" was a redundant label, and the shared repo lands early, not last.** A generic doc at the
  `api/` layer is portable by construction, so the `portable/`/`local/` folder split is gone. More
  importantly the monorepo is temporary: conventions kept inside it make every carve-out an import
  rewrite, so `conventions/` is built at repo root now — the future submodule mount point — and the swap
  is `git rm -r --cached` + `git submodule add` with zero import churn.

Next, in order:

1. **`docs/conventions-repo`** — create the shared `conventions` repo (`dotnet/`, `typescript/`,
   `process/`) and mount it at repo root. **Blocking prerequisite:** `.github/workflows/*` checks out
   without submodules today, so `actions/checkout` needs `submodules: true` in the same change or every
   `@conventions/...` import resolves to nothing and the reachability gate goes red.
2. **`docs/guidance-restructure`** — split `api/agents/*` and `app/agents/*` into one topic per file
   under `conventions/`, carrying text verbatim. Pull the four scoped topics out to `PROTO.md`,
   `MULTITENANCY.md`, `KEYED_STRATEGIES.md`. Rewrite each consumer's `AGENTS.md` to import only what it
   can act on; move Concertable precedents (context roster, filtered-entity list, `DealType` families,
   Refit client roster) into the consumer's own `agents/` file.
3. Collapse the duplication rows to one home each. Biggest: seeding from 5 locations. Resolve
   `api/AGENTS.md:26-45` under the import-or-summarize rule — `SEEDING_CONVENTIONS.md` is not
   `@`-imported, which is *why* that inline summary exists. Pick one.
4. Re-point `docs/INDEX.md` at the new paths and re-run the link check.

Still needs a decision before step 2: whether `api/agents/CONVENTIONS.md` becomes `MODULE_STRUCTURE.md`.
It collides with `CODE_CONVENTIONS.md`, reads as that file's superset when it is narrower, and its
`:6`/`:91` "modules in the monolith" framing contradicts `api/ARCHITECTURE.md:8`.

Not in scope: auto-load thinning beyond what scoping achieves, and the analyzer push-down including
`EnforceCodeStyleInBuild` (several `severity = error` style rules currently fail no build).
