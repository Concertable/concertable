# Docs review — Docs/docs_guidance-restructure-closeout

Work order for PR [#666](https://github.com/Concertable/concertable/pull/666) — the guidance-docs
restructure close-out. Findings are `- [ ]` until fixed, `- [x]` when addressed on the branch.

**Reviewed up to commit:** `PLACEHOLDER`  _(2026-08-19)_

> Range: `origin/main...HEAD` — 4 files, 6 insertions, 2,218 deletions.

**Not a pure close-out, so this review applies.** `git diff --diff-filter=ACMRT --name-only
origin/main...HEAD` prints two files, not nothing: `plans/docs/DOCS_ROADMAP.md` and
`plans/platform/POLYREPO_ROADMAP.md` both carry surviving edits. Per `docs-review`'s own rule, the
moment a roadmap tick survives it is an ordinary docs PR — so the two deletions are exempt and the
surviving change is reviewed below.

## Findings

- [x] **DOC1 — MEDIUM — Lens A (stale fact)** — `plans/docs/DOCS_ROADMAP.md:39`
  The new `docs/guidance-autoload` row claimed the auto-loaded floor went "from 1,429" and root
  `AGENTS.md` "from 298". Measured against `dc037f477` — the baseline this roadmap's own "Why this epic
  exists" section names — the real figures are **1,428** (`api/AGENTS.md` 97 + `CODE_CONVENTIONS.md` 418
  + `CODE_PATTERNS.md` 293 + `RESULT_PATTERN.md` 620) and **300**. Both numbers were inherited from the
  progress ledger, which this PR deletes, so the roadmap is about to become their only home and they
  needed to be right rather than approximately right. Corrected, and the baseline commit is now named
  in the row so the next reader can re-derive them.
  Worth recording: at the branch's actual base (`29e7a1ad1`) the same four files totalled **1,512** — the
  corpus grew during the work, so "1,428" is the roadmap-baseline figure, not the high-water mark.

- [x] **DOC2 — MEDIUM — Lens A (dead reference)** — `plans/platform/POLYREPO_ROADMAP.md:134`
  Cited `plans/docs/GUIDANCE_DOCS_RESTRUCTURE_PLAN.md` "Phase 5b" — a file this same PR deletes, which
  is precisely the transient-artifact citation the corpus disqualifies. Replaced with the outcome it was
  pointing at, each part verified against `origin/main`: `api/agents/` and `app/agents/` are gone, the
  generic half is in `tomjseery/dotagents` + `tomjseery/react-agents` and this system's roster in
  `Concertable/agent-standards`, `api/AGENTS.md` is 78 lines with no `@`-imports, and `docs/INDEX.md`
  maps topic to owner.

## Verified, no finding

- **Lens A, reachability** — `docs_reachability.py` 0 errors, `plan_graph.py` 0 errors after the deletion.
  No surviving doc references either deleted file (`grep -rn GUIDANCE_DOCS_RESTRUCTURE` outside
  `reviews/` is empty).
- **Lens B, contradiction** — nothing else in the corpus describes `docs/guidance-autoload` or
  `docs/guidance-restructure` as outstanding, so ticking them contradicts no sibling.
- **The roadmap survives correctly.** `docs/analyzer-pushdown` is genuinely undone —
  `EnforceCodeStyleInBuild` is set in no `.props`/`.targets`/`.csproj` under `api/` — so the epic has not
  shipped and its tracker must not be deleted.
- **Lens C/D** — both edits are in the topic's own playbook, and neither file is harness-reloaded every
  prompt, so the added measurement earns its length.

## Observation — pre-existing, out of scope, and not this worktree's to fix

Two **other** plans still cite the docs `#637` deleted, so they dangle now:

- `plans/frontend/DOMAIN_COMPANION_MAPPING_PLAN.md:168` — `app/agents/CODE_PATTERNS.md`,
  `app/agents/CODE_CONVENTIONS.md`
- `plans/launch/ADMIN_CONSOLE_PLAN.md:148` — `api/agents/CODE_PATTERNS.md`

`docs_reachability.py` only warns inside `plans/`, so neither fails a gate. Both are live plans with
their own worktrees, and working markdown rides its owning branch — editing them from here would cross
the worktree-identity gate for a one-line fix. Left to their owners, recorded so it is not discovered
twice.
