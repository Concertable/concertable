---
name: continue-roadmap
description: Pick the next feature off an epic's roadmap and hand it off to be planned. Use when Tommy invokes `/continue-roadmap` or asks what's next on the launch/epic. Take an optional reference to a `*_ROADMAP.md`; otherwise use `plans/b2b/LAUNCH_ROADMAP.md`. Read AGENTS.md, plans/AGENTS.md and the roadmap, classify every outstanding item against real git/PR/worktree state, let Tommy pick, then emit a handoff prompt that a fresh context uses to WRITE that item's feature plan. This creates a new plan; it does not resume one — that's `/resume-plan`.
---

# Continue Roadmap

A roadmap (`*_ROADMAP.md`) is an epic's living progress tracker: no `_PROGRESS.md`, never deleted,
checkboxes *are* its progress (see `plans/agents/ROADMAP.md`). Each buildable item spins off its own
feature plan. This skill picks the next such item and hands it off to be planned in a fresh context — it
does **not** design or write the plan here, and does **not** resume an existing plan (that's
`/resume-plan`).

## Steps

1. **Resolve the roadmap.** `/continue-roadmap @plans/<X>_ROADMAP.md` → that file. Nothing → default to
   `plans/b2b/LAUNCH_ROADMAP.md`.
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
4. **Present the ready candidates with a recommendation and let Tommy pick.** One line each: the item, its
   size/blast radius, and why it's ready. The pick is his.
5. **Emit the handoff prompt** for the chosen item — the deliverable. It is self-contained text Tommy
   pastes into a fresh context to WRITE the feature plan, and must:
   - name the roadmap line and the exact source docs to read (the roadmap, the relevant
     `LEGAL_REQUIREMENTS.md` / architecture / already-shipped feature), and any dependency gate;
   - instruct: branch `Feature/<Name>` off `origin/main`, then write `plans/<area>/<STEM>.md` **and** its
     `<STEM>_PROGRESS.md` ledger from the progress template, following `plans/agents/PLAN.md`;
   - state the outcome, constraints and what's out of scope — but leave the design to the plan;
   - tick the roadmap line when the feature ships (do not delete the roadmap).
   Do not open the branch or write the plan here.
