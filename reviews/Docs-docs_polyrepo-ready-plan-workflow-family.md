# Docs review — Docs/docs_polyrepo-ready-plan-workflow-family

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `2b92ca18dae558b04632546b50e6e2b3dc226b4f`  _(2026-08-21)_

> Range reviewed: `c39077f1a..2b92ca18d` (consumer, 1 commit) and `9437795..260f7f68` (producer,
> `Concertable/agent-standards#10`, 1 commit). One review file spans both halves of N1 family 5, since the
> move is a single change split producer/consumer. Run from the moved procedure
> `standards/process/review/DOCS.md`, because the session's active plugin snapshot lacks `docs-review`.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No findings. All six lenses checked clean across both halves, plus two targeted structural checks:

- **A — Accuracy vs reality:** every skill citation resolves (`plan-checkpoint`, `resume-plan`,
  `continue-roadmap`, `update-roadmap`, `techdebt`, `open-worktree`, `open-pr`, `docs-and-debt`,
  `docs-review`, `merge-docs`, `plans`, `handoff` all ship from `plugins/agent-process/skills/`); the five
  producer routers point at existing standard files; `RESUME.md` → `CHECKPOINT.md` links resolve;
  `plan_graph.py` and `scripts/worktrees.ps1` are the valid vendored constants. Consumer sweep: no durable
  doc references any deleted path; both command dirs are gone. The one `<url>` reachability error is
  pre-existing in `PLANS.md`, not this diff.
- **B — Contradiction:** the deferral chain resolves — `plans/AGENTS.md` now names the `plan-checkpoint`
  standard as the ledger-template and checkpoint owner, and both `package-cutover` and the four already-moved
  families ("the checkpoint procedure the repository's plan floor names") bottom out there.
- **C — Right home:** `TECHDEBT.md` states the operational step and explicitly defers the scalable-fix and
  delete-the-entry rules to `docs-and-debt` — a pointer, not a second copy.
- **D — Concision of reloaded docs:** both `.agents/README.md` and `plans/AGENTS.md` edits are net
  reductions; no bloat.
- **E — Dangling/transient references:** the new standards use only generic placeholders; no plan filename,
  "Phase N", or ticket baked in.
- **F — Followable instruction:** every gate in `CHECKPOINT.md` and every step in the plan procedures and
  `TECHDEBT.md` carries an explicit pass/verify condition.
- **Structural:** `CHECKPOINT.md`'s folded ledger template uses a `~~~markdown` outer fence, so its inner
  ```` ``` ```` resume-prompt block cannot terminate it; and its `#the-progress-ledger-template` anchor
  matches the heading.

Ordering caveat (expected, not a defect): the consumer's plugin-delivered references go live only once the
producer PR merges to `agent-standards` main — standard producer-first sequencing.

An independent reviewer (fresh context, all six lenses + the two structural checks) reached the same
conclusion.
