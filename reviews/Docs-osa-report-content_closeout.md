# Docs review — Docs/osa-report-content_closeout

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `PENDING`  _(2026-08-16)_

> Range reviewed: `35b114d4a..HEAD` (3 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **ACC1 — MEDIUM — accuracy vs reality** — `.agents/skills/docs-review/SKILL.md:31`
  The exemption's test command was written `--diff-filter=acmrt`. Lowercase filters **exclude** those
  types, so it printed exactly the deletions it was meant to ignore — a reader following it would see
  output and conclude the branch was *not* a pure close-out, inverting the gate. Verified empirically
  against this branch. **Fixed** to uppercase `ACMRT`, which selects surviving adds/edits/renames and
  correctly prints nothing for a deletions-only diff.

- [x] **CON1 — MEDIUM — contradiction with sibling docs** — `.agents/skills/merge/SKILL.md:61`
  The exemption was added to `merge`'s Step 6 close-out line but not its Step 0, which still stated flatly
  that a docs/meta-only PR "requires a clean `/docs-review`". Two rules in one skill disagreeing is the
  defect this lens exists for, and Step 0 is the one an agent reads first. **Fixed:** Step 0 now names
  the exemption and points at `docs-review` for the condition.

### Checked and clean

- **Right home (Lens C):** the rule is stated once, in `docs-review`, which owns it; `merge-docs` Step 0
  and `merge` Steps 0/6 point at it rather than restating the condition, so the copies cannot drift.
- **Concision (Lens D):** all three files are harness-reloaded. The rule costs six lines in its owner and
  one clause in each pointer; no example block, no restatement.
- **Dangling references (Lens E):** the exemption names no plan, phase or ticket — it is expressed as a
  property of the diff, so it outlives every artifact it will be applied to.
- **Followable (Lens F):** the gate states its own pass condition as a runnable command, and names the
  exact circumstances that revoke it (a roadmap tick, a `TECH_DEBT` entry, a convention edit).
- **Deletions:** `OSA_REPORT_CONTENT_PLAN.md` and `_PROGRESS.md` are the spent plan pair for work merged
  as `fc3c876f5`; the roadmap already carries what outlives them, so nothing referenced them. Both hook
  guards (`plan_graph`, `docs_reachability`) pass with them gone.
