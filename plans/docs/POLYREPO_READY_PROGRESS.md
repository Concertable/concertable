# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: none yet — create one from current `origin/main`
- Branch: none yet
- PR: none yet

## Current state

**Nothing implemented. The plan is written, its decisions are taken, and every phase is unblocked.**
This ledger exists so the work can start from a fresh worktree without re-deriving anything.

The predecessor, `docs/guidance-restructure`, shipped and was closed out on 2026-08-19 (#637 merged as
`b61feed88`, sync #661 green, plan and ledger deleted in #666). It left the corpus split by portability
but shaped for a monorepo, which is what this item finishes.

**Already true, and load-bearing for this work:**

- Five plugins are installed at `--scope user` and all **67** routers open a byte-identical copy of their
  own repo's doc, so moving a doc between repos is a publish-and-reinstall, not a copy.
- The route table gates **100%** of tracked source files via two area floors and four layer routes;
  `matching_routes` yields every match, so a floor and a specific row both fire.
- Six process docs already live in `agent-standards/standards/process/`, which is where Phase 1's content
  joins them.

**Measured baseline for Phase 1**, so progress is checkable rather than asserted:

| File | Lines | Lines mentioning this repo |
|---|---|---|
| `plans/agents/PLAN.md` | 183 | 8 (4%) |
| `PROMPTS.md` | 50 | 1 (2%) |
| `plans/agents/ROADMAP.md` | 26 | 0 (0%) |
| `plans/AGENTS.md` | 53 | 7 (13%) — **stays** |

## Next Steps

1. **Phase 1 — move the 259 lines.** Create a worktree from current `origin/main`, and a matching branch
   in `Concertable/agent-standards`; the two land together because the in-repo shim must not point at a
   doc that has not shipped yet. Fold `plans/agents/PLAN.md` and `ROADMAP.md` into
   `standards/process/PLANS.md`, move `PROMPTS.md` to a new `standards/process/HANDOFF.md`, add its
   router, widen the `plans` skill description, and reduce the in-repo files to the layout/scripts/skill
   names named in the plan's Phase 1.

   **Watch for:** the Stop hook and `plan_graph.py` both read the handoff pointer's shape, and
   `PROMPTS.md` is currently its only definition — so move the doc and re-point those two together, or
   every plan handoff in the repo starts failing its own gate.

2. **Phase 2 — re-anchor `^api/`, `^app/`, `^plans/`.** Verify by replaying every tracked path through
   the table twice: once against the monorepo tree, once against a tree with the prefix stripped, and
   require 100% both times.

3. **Phases 3 and 4** as the plan states. Phase 4 is the only one that produces evidence rather than
   edits; do not call the item done without it.

**Open question for Tommy, not blocking:** each carved service repo will need its own thin
`plans/AGENTS.md` and hook wiring naming its own script paths — roughly 50 lines × 8 repos of genuinely
local content. Vendoring already handles the hooks. Whether that thin file is **generated** from a
template at carve time or hand-kept per repo is a real choice, and Phase 4 is where it gets tested either
way.
