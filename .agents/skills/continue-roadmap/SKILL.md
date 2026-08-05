---
name: continue-roadmap
description: Pick the next feature off an epic's roadmap and hand it off to be planned. Use when Tommy invokes `/continue-roadmap` or asks what's next on the launch/epic. Accept an optional `*_ROADMAP.md` reference and an optional natural-language preferred item; without a preference, list the ready candidates for Tommy to choose. Read AGENTS.md, plans/AGENTS.md and the roadmap, classify every outstanding item against real git/PR/worktree state, then emit a handoff prompt that a fresh context uses to WRITE the chosen item's feature plan. This creates a new plan; it does not resume one — that's `/resume-plan`.
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
   - **in-flight** — a branch/PR/worktree already owns it (name it; don't offer it);
   - **blocked** — waits on an unlanded PR, platform sync, or other work (name the blocker);
   - **ready** — unblocked and unowned.
   Enumerate the whole outstanding set before concluding an item is ready; a name scan can't see an
   in-flight worktree, so verify against `git`/PR state, not the roadmap's own status marks.
4. **Resolve the choice.**
   - With no preference, present every ready candidate with a recommendation and stop for Tommy to pick.
     Use one line each: the item, its size/blast radius, and why it's ready.
   - When the preferred item is ready, identify the match and treat it as Tommy's pick; continue directly
     to the handoff.
   - When the preferred item is in flight, blocked, or not found, explain that status, present the ready
     alternatives, and stop for Tommy to pick. Never force an unavailable item.
5. **Emit the handoff prompt** for the chosen item — the deliverable. It is self-contained text Tommy
   pastes into a fresh context to WRITE the feature plan, and must:
   - name the roadmap line and the exact source docs to read (the roadmap, the relevant
     `LEGAL_REQUIREMENTS.md` / architecture / already-shipped feature), and any dependency gate;
   - instruct: branch `Feature/<epic>_<name>` off `origin/main` (the epic is the roadmap's folder), then
     write `plans/<epic>/<NAME>_PLAN.md` **and** its `<NAME>_PROGRESS.md` ledger from the progress
     template, following `plans/agents/PLAN.md`;
   - state the outcome, constraints and what's out of scope — but leave the design to the plan;
   - tick the roadmap line when the feature ships (do not delete the roadmap).
   Do not open the branch or write the plan here.
