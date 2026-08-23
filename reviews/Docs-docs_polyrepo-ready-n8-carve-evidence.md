# Docs review — Docs/docs_polyrepo-ready-n8-carve-evidence

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `c629aee9592e35d8b283d298731756d0c9634248`  _(2026-08-23)_

> Range reviewed: `fb561acee..d6a7983d3` (1 commit).

## Incremental review — 2026-08-23

Merged `origin/main` for currency (17 commits behind: PR #761 architecture-tests-rename checkpoint, #759
portable-hooks fix, platform-sync #758, and their dependents — all pre-existing, none authored on this
branch). Merge was clean, no conflicts. `git diff origin/main...HEAD --stat` after the merge shows only the
three already-reviewed ledger files plus this review file — the merge added no new branch-authored content,
so this is a clean re-stamp with no new findings.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No findings. Checked all six lenses:

- **Lens A (accuracy)** — Cross-checked the ledger's empirical claims against the live repo rather than
  taking them on faith: `.agents/skill-routes.json` does carry a `\.ArchitectureTests[^/]*/.*\.cs$` row
  mapped to `composition-testing` + `dotnet-standards:module-structure` (+ `dotnet:module-structure`,
  truncated in the grep) exactly as described; `agent-standards/.agents/gen_skill_routes.py` does still
  emit `\.CompositionTests[^/]*/.*\.cs$` with no `\.ArchitectureTests` row — the generator-drift finding
  (A) is real and accurately stated. PR references (#752, #760, agent-standards #20) and SHAs (`ab1755d`
  current agent-standards main, `13fcef1c0` last reprovision — matches the currently installed plugin
  cache path) all verified against GitHub/local state. `docs_reachability.py --root <worktree>`: 0 errors,
  27 warnings, all pre-existing `plans/` dead-link warnings on lines this diff does not touch (confirmed
  the `@plans/docs/POLYREPO_READY_PROGRESS.md` pointer line in `POLYREPO_READY_PLAN.md` is unchanged since
  `fb561acee`). `plan_graph.py`: 0 errors, 0 warnings.
- **Lens B (contradiction)** — The rewritten ledger's "Current state", "Next Steps", "Completed work",
  "Decisions" sections all agree with each other and with the updated `POLYREPO_READY_PLAN.md` N8 section
  and `DOCS_ROADMAP.md` row on the same facts (N7a merged, N8 evidence recorded, generator-drift open).
  No stale claim survives elsewhere in the diff.
- **Lens C (one-rule-one-home)** — No rule duplicated; the compacted ledger states the N8 evidence once
  and points the roadmap row and plan section at it rather than restating.
- **Lens D (concision)** — Neither file is harness-reloaded (not `AGENTS.md`/`CLAUDE.md`/`SKILL.md`); held
  to clarity, not word count, per the standard. The rewrite nets shorter (463 → fewer lines) without losing
  a still-true fact.
- **Lens E (dangling references)** — This ledger is itself the transient/durable split point by design
  (a `PROGRESS.md`), so its own "Phase N"/PR-number citations are expected, not a defect.
- **Lens F (followable instructions)** — `## Next Steps` items are concrete and each names what is owed
  to whom (Tommy's go-ahead on bundling, the auto-memory decision) rather than an ambiguous "your call".
  The resume prompt matches the current state exactly (N7b gated, N8 recorded, generator drift + auto-memory
  pending decisions).

**Scope guard:** diff touches `plans/docs/DOCS_ROADMAP.md`, `plans/docs/POLYREPO_READY_PLAN.md`,
`plans/docs/POLYREPO_READY_PROGRESS.md` only — meta-only, no runtime/product/package/CI path. Not a pure
close-out (surviving content change) — full `DOCS.md` review applied.
