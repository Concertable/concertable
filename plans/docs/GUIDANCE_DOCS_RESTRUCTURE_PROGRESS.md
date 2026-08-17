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
yet *reorganized* — no file has moved, and the `portable/` vs `local/` split does not exist yet.

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
  what a machine already enforces and whether it fails a build; and eight rules for adding to the
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

Phase 3 — split and move — needs one decision first: whether `api/agents/CONVENTIONS.md` becomes
`MODULE_STRUCTURE.md`. It currently collides with `CODE_CONVENTIONS.md`, reads as that file's superset
when it is actually narrower, and its `:6`/`:91` "modules in the monolith" framing contradicts
`api/ARCHITECTURE.md:8`.

Then, in order:

1. `git mv` `api/agents/*` and `app/agents/*` into `conventions/portable/` and `conventions/local/` per
   the plan's target tree, splitting `RESULT_PATTERN.md` (620), both `CODE_CONVENTIONS.md`, and both
   `CODE_PATTERNS.md`. Carry text verbatim so the diff reads as a move.
2. Write the two `conventions/README.md` files from the meta-rules already in `docs/INDEX.md`.
3. Phase 4 — collapse each duplication row to one home. Biggest: seeding from 5 locations to
   `portable/SEEDING.md` + `local/SEED_INVENTORY.md`. Resolve `api/AGENTS.md:26–45` under the
   import-or-pointer rule: `SEEDING_CONVENTIONS.md` is not `@`-imported, which is *why* that summary
   exists — pick one, not both.
4. Re-point `docs/INDEX.md` at the new paths and re-run the link check.

Strict portable/local separation was chosen: portable files carry no Concertable identifier, so the
later extraction is a `git mv` rather than a rewrite. Precedents move to the local sibling.

Not in scope for Phase 3: auto-load thinning, the analyzer push-down plus `EnforceCodeStyleInBuild`
(several `severity = error` style rules are currently IDE-only), and the extraction itself.
