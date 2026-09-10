# Docs review — Docs/monitor-token-efficient-waits

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `82be2c487b985de50d8a35d8bc488124dca05ccf`  _(2026-08-22)_

> Range reviewed: `7399ada42..82be2c487` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

Checked the single changed file:

- **`AGENTS.md`** — the deleted bullet duplicated and contradicted the canonical long-wait procedure
  owned by the `merge`/`merging` skills. Concertable retains only its repository-specific merge
  invariants, while the permanent agent-standards source now prefers a bounded Monitor/listener with
  exact identity and authoritative terminal confirmation (Concertable/agent-standards PR #24, merged
  as `2241c948f58b0574810f9b02fd72553d75538668`). No replacement prose belongs here under the
  one-rule-one-home policy.

`python .agents/hooks/docs_reachability.py --root .` completed with 0 errors (31 existing plan-link
warnings). `git diff --check` passed. Repository search found no remaining authored copy of the deleted
Monitor ban.

No issues found. Checked accuracy, cross-document contradiction, owning-document locality,
reachability, concision, and followability.
