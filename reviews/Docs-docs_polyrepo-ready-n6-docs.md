# Docs review — Docs/docs_polyrepo-ready-n6-docs

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `4a6608324cf046629d40f6851f5d8f59ad012ccb`  _(2026-08-22)_

> Range reviewed: `26645ecd..150a6f3f` (N6 delivery) + the ACC1 fix; the ledger commit after it is
> `reviews/`-adjacent plan bookkeeping. Independent docs-review, all six lenses + reachability hook.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **ACC1 — LOW — Lens A (accuracy)** — `plans/docs/POLYREPO_READY_PROGRESS.md`
  The N6 verification parenthetical mis-attributed the reachability warning delta — it claimed the +1 vs N5
  was a historical `docs/OVERVIEW.md` mention, but the hook flags no such warning (it doesn't flag inline-code
  doc mentions). **Fixed**: the statement now reads that all 29 warnings are `plans/` dead-link warnings from
  the ledger's own `@`-import / relative references, none referencing a deleted doc.

## Lenses clean (independent reviewer)

- **A (accuracy):** no surviving dead repo-relative link to the three deleted docs; the two in-repo referrers
  (root `AGENTS.md:3`, `INDEX.md` Product rows) repoint to well-formed `github.com/Concertable/docs/blob/main/`
  URLs. Reachability hook: 0 errors, 29 `plans/` warn-only warnings; no orphan created by the deletions.
- **B (contradiction):** no sibling (`README`, `ARCHITECTURE.md`, roadmap, plan, ledger) still claims product
  docs live in `docs/`; `INDEX.md` reconciles the external Product rows at the point of use.
- **C (right home):** pointers-only edits; no rule relocated or duplicated.
- **D (concision):** the root `AGENTS.md` line-1 edit adds ~6 words conveying the new polyrepo fact once, on a
  descriptive line that carries no rule — below the bar.
- **E (dangling):** no durable doc bakes in a plan filename / Phase-N.
- **F (followable):** plan/ledger Next Steps + the endpoint-at-cut clarification are internally consistent.
