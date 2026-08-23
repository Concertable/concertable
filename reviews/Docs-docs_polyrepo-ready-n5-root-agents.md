# Docs review — Docs/docs_polyrepo-ready-n5-root-agents

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `bd092a9940d0a20b435661368761bcc046d78875`  _(2026-08-22)_

> Range reviewed: `e6e967f2..595136d8` (thin + CON1 fix); the ledger commit after it is `reviews/`-adjacent
> plan bookkeeping. Independent docs-review (all six lenses + a no-rule-lost audit of the destination docs).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CON1 — HIGH — Lens B (contradiction)** — `docs/INDEX.md:30,32,37,41,43,54,56`
  The thin deleted seven root `AGENTS.md` sections that `docs/INDEX.md` (the topic→owner map) still pointed
  at, and the thinned root delegates topic lookup to `INDEX.md` — so a reader following the root's own
  instruction was routed to sections that no longer exist. **Fixed** in `595136d8`: six rows repointed to
  their new owners — long-term/questions/autonomy → skill `floor`; worktree-identity + durable-guidance →
  skills `open-worktree`/`git-branching` (identity-gate keep still in root); ready-for-review + merge
  invariants + platform-sync → skill `merging`; worktree cleanup → skill `open-worktree` + `plans/AGENTS.md`
  + `scripts/worktrees.ps1`. The seventh row (doc-locality + `CLAUDE.md`-siblings) was dropped as redundant
  with the existing `docs-and-debt` row and the reachability-hook row.

## Lenses clean (independent reviewer)

- **A (accuracy):** every link, both scripts, `.agents/skill-routes.json`, and every cited skill resolve;
  `docs_reachability.py` 0 errors.
- **Rule-loss audit:** every deleted section survives at its claimed destination doc; the retained root
  "service ownership" keep is correctly placed (a monorepo concern; `open-worktree` does not restate it, so
  no duplication).
- **C (right home):** hub rules reduced to pointers, no leftover duplicate, no new guidance bolted on.
- **D (concision):** root 149 → 23 lines; every surviving line carries a rule or pointer.
- **E (dangling):** the durable root cites no plan filename or Phase-N; only pre-existing `plans/` warnings.
- **F (followable):** clean.
- **Plan/progress docs:** clean on A/E/F.
