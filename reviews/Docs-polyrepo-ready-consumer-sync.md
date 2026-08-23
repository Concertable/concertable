# Docs review — Docs/polyrepo-ready-consumer-sync

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `aafad60f7a9c2a7b3cb142cf058939fce88f53db`  _(2026-08-23)_

> Range reviewed: `75b564bc9..aafad60f7` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

None. Meta-only diff: a regenerated `.agents/skill-routes.json`, two `auto-memory` skill deletions, and
ledger + roadmap prose. Lenses checked:

- **A (accuracy):** every claim verified against reality — agent-standards #34 and Concertable #764 are
  MERGED; the machine is reprovisioned to `1aefd60`; `gen --kind monorepo --check` is clean against the
  regenerated table; an `*.ArchitectureTests` path resolves `composition-testing` + `module-structure`;
  `--verify-install claude` resolves all 55 routed skills after the deletion. `docs_reachability.py`: 0
  errors. The two cited spent review files both exist in `main`.
- **B (contradiction):** the ledger's "finding A fixed / auto-memory homed" agrees with `POLYREPO_READY_PLAN.md`'s
  N8 note, which points at the ledger for disposition; no sibling now states the opposite. The route SET is
  provably unchanged, so no doc describing routing behaviour is falsified.
- **C (home) / D (concision) / E (dangling) / F (followable):** clean — plan-ledger and roadmap are the right
  homes; they are long-form/tracker docs (not harness-reloaded), so held to clarity not word count; PR/node
  citations belong in a living tracker; Next Steps are concrete and actionable.
