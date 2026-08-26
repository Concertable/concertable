# Docs review — Docs/FrontendDocParity

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `c09c4c14fd5cce0cc731e889636aafb4b3d5b037`  _(2026-08-15)_

> Range reviewed: `520761dd..c09c4c14` (3 commits, diffed against `origin/main`'s merge-base — local
> `main` is stale and would have pulled in unrelated already-merged backend commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **ACC1 — LOW — accuracy** — `app/mobile/shared/AGENTS.md:17`
  Link to `CODE_PATTERNS.md` was `../agents/CODE_PATTERNS.md`, which resolves to the nonexistent
  `app/mobile/agents/CODE_PATTERNS.md` (one `../` short — the real file is `app/agents/CODE_PATTERNS.md`,
  two levels up from `app/mobile/shared/`). Fixed to `../../agents/CODE_PATTERNS.md`.

- [x] **ACC2 — LOW — accuracy** — `ARCHITECTURE.md:11`
  The Mobile bullet had no link at all (`React Native apps.`, full stop), unlike its Web sibling on the
  line above. Now points at `app/mobile/AGENTS.md`, the doc this branch adds.

- [x] **ACC3 — MED — accuracy** — `.agents/skills/e2e-api-debug/SKILL.md:194`,
  `.agents/skills/e2e-ui-debug/SKILL.md:222`
  Both pointed at `api/docs/DEBUGGING_CONVENTIONS.md`, which does not exist — dead since commit
  `3858dc24d` moved normative agent docs from `api/docs/` to `api/agents/`; the two skill links were
  never updated. Caught by `.agents/hooks/docs_reachability.py` (new in this branch) reporting the real
  `api/agents/DEBUGGING_CONVENTIONS.md` as unreachable. Fixed both to `api/agents/DEBUGGING_CONVENTIONS.md`.

- [x] **CONC1 — LOW — concision** — `.agents/skills/docs-review/SKILL.md` (Lens A addition)
  The new mechanical-checker paragraph carried a trailing narration sentence ("this is exactly what
  caused the frontend ... orphan") that added no constraint, inconsistent with the terse
  instruction-only style of the surrounding Lens A bullets — a doc reloaded every prompt. Trimmed to the
  trigger condition, the command, and what to do with its output.

No other issues found. Checked accuracy vs reality (every relative link and `@`-import in the diff
resolved programmatically), cross-doc contradiction (the re-landed frontend chain doesn't conflict with
any sibling tier doc), doc home & convention (the `app/shared/TECH_DEBT.md` debt item lives at the tier
that would host its fix, not merely where all four call sites happen to be), harness-reloaded concision,
dangling/transient references, and followable instructions. `.agents/hooks/docs_reachability.py` runs
clean (0 errors) against the full repo, and its 8 unit tests pass.

## Incremental review — 2026-08-15

New root `TECH_DEBT.md` plus a header narrowing on `api/TECH_DEBT.md` (it had over-claimed "repo-wide
build/CI config" — the same one-concern-two-homes mistake this branch fixed elsewhere). Checked: every
relative link in both files resolves (verified programmatically); the logged `run_code` gate claim was
confirmed against the real job list of PR #579's `merge_group` run (`build`/`carve-*`/unit/integration
all ran; only `fe-boundaries`/`carve-fe` were skipped) — not a guess; no contradiction with
`app/web/TECH_DEBT.md` or `app/shared/TECH_DEBT.md`'s scope (both stay tier-specific).

No findings.
