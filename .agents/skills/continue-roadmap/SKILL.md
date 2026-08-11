---
name: continue-roadmap
description: Pick the next implementable feature off an epic's roadmap and hand it off to be planned. Use when Tommy invokes `/continue-roadmap` or asks what's next on the launch/epic. Accept an optional `*_ROADMAP.md` reference and optional natural-language preference; without one, list candidates that are ready or delivery-gated but locally implementable. Read AGENTS.md, plans/AGENTS.md and the roadmap, verify real git/PR/worktree and dependency state, and emit a handoff prompt that writes the chosen feature plan. This creates a new plan; it does not resume one — that's `/resume-plan`.
---

# Continue Roadmap

A roadmap (`*_ROADMAP.md`) is an epic's living progress tracker: no `_PROGRESS.md`, never deleted,
checkboxes *are* its progress (see `plans/agents/ROADMAP.md`). Each buildable item spins off its own
feature plan. This skill picks the next such item and hands it off to be planned in a fresh context — it
does **not** design or write the plan here, and does **not** resume an existing plan (that's
`/resume-plan`).

## Steps

1. **Resolve the roadmap and optional preference.** Usage is
   `/continue-roadmap [@plans/<X>_ROADMAP.md] [preferred item in natural language]`.
   - A roadmap reference selects that roadmap; without one, use `plans/launch/LAUNCH_ROADMAP.md`.
   - Text after the roadmap reference is a natural-language selection hint. Match it by meaning; do not
     require an exact roadmap title.
   - A preference preselects an item only if the reality check below finds it ready. It never bypasses
     ownership or dependency checks.
2. **Read in full:** `AGENTS.md`, `plans/AGENTS.md`, and the roadmap.
3. **Classify every outstanding item against reality — never off the roadmap text alone.** For each
   unchecked / 🔴 / 🟠 / 🟡 item and each "verify before trusting" line, check `git worktree list`, local
   branches, and `gh pr list` for a branch/PR/worktree already working it, and read its named dependency
   docs for an unlanded gate. Sort each into:
   - **in-flight** — a branch/PR/worktree already owns it (name it; resume its ledger instead of creating
     another owner);
   - **implementation-blocked** — required source/API/design or a trustworthy exact artifact is
     unavailable (name the blocker and owner);
   - **delivery-gated but implementable** — safe local implementation can proceed now, with later
     published-baseline validation before merge;
   - **ready and unowned** — no implementation or delivery gate prevents normal work.
   Enumerate the whole outstanding set before concluding an item is ready; a name scan can't see an
   in-flight worktree, so verify against `git`/PR state, not the roadmap's own status marks.
   Match existing ledgers by their explicit `Roadmap:` and `Roadmap item:` headers before using names
   or branch heuristics. Select a stable `<epic>/<slug>` key for an unkeyed chosen item and include it
   in the handoff; the planning context adds it to the checklist line and ledger before validation.
4. **Resolve the choice.**
   - With no preference, present every ready and delivery-gated-but-implementable candidate with a
     recommendation and stop for Tommy to pick. Use one line each: the item, its size/blast radius, and
     its implementation and delivery state.
   - When the preferred item is ready or delivery-gated but implementable, identify the match and treat
     it as Tommy's pick; continue directly to the handoff.
   - When the preferred item is in flight, implementation-blocked, or not found, explain that status,
     present the implementable alternatives, and stop for Tommy to pick.
5. **Emit the handoff prompt** for the chosen item — the deliverable. It is self-contained text Tommy
   pastes into a fresh context to WRITE the feature plan, and must:
   - name the roadmap line and exact source docs to read (the roadmap, relevant
     `LEGAL_REQUIREMENTS.md` / architecture / already-shipped feature), plus separate implementation
     and delivery dependencies;
   - instruct: branch `Feature/<epic>_<name>` off `origin/main` (the epic is the roadmap's folder), then
     write `plans/<epic>/<NAME>_PLAN.md` **and** its `<NAME>_PROGRESS.md` ledger from the progress
     template, following `plans/agents/PLAN.md`; the ledger must record the roadmap path and exact item
     key, then pass `python .agents/hooks/plan_graph.py --root <absolute-worktree>`;
   - state the outcome, constraints and what's out of scope — but leave the design to the plan;
   - tick the roadmap line when the feature ships (do not delete the roadmap).
   Do not open the branch or write the plan here.
