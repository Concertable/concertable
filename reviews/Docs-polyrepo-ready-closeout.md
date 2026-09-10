# Docs review — Docs/polyrepo-ready-closeout

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `326570348018ec7195e6b3d63cd9ce70b0ea0e9c`  _(2026-08-24)_

> Range reviewed: `70af43aac..3265703` (1 commit).
> Status legend: `[ ]` todo · `[x]` done · `[wontfix]` (note why).

## Findings

None. Near-pure close-out: deletes the plan + ledger + three spent review records; the only surviving edits
are two roadmap updates. Lenses checked:

- **A (accuracy):** the tick claims match reality — every node merged, N8 #764, the drift fix #34, the
  consumer regen #766, `auto-memory` homed. The one dangling **link** the deletion created
  (`POLYREPO_ROADMAP.md` §6 → the deleted plan) was repointed to `DOCS_ROADMAP.md`; the decision-log prose
  mention updated to past tense. `docs_reachability.py`: 0 errors, no reference to any deleted file.
  `plan_graph.py`: 0/0 (the ticked roadmap item owes no plan).
- **B (contradiction):** consistent — `POLYREPO_ROADMAP` §4c still owns plans-locality, and `DOCS_ROADMAP`
  now hands the residual there rather than restating it.
- **C/D/E/F:** clean — no rule bolted onto a hub, roadmaps are trackers (not harness-reloaded), the residual
  is named by its owning roadmap section rather than a dangling "N7b" pointer, and the handoff is followable.

Backtick prose mentions of the deleted files survive in other branches' review files
(`reviews/Chore-TryInsert.md`, `reviews/Docs-skill-routes-mapper-coverage.md`) — not markdown links, not
durable guidance, and swept by their own branches' lifecycles; out of scope here.
