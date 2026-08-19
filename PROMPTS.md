# Prompts

The rule that an actionable non-terminal plan must end its handoff with the repository's continuation
pointer is the `plans` skill. **This file is that pointer's exact shape** — nothing else defines it.

- **Hard final-response gate for plan-managed work:** if a `_PROGRESS.md` ledger with actionable
  non-terminal `## Next Steps` is owned by the current or explicitly targeted worktree, the response is
  incomplete until it ends with the pointer below. Merely naming a dependency ledger for reading, or editing
  its copy from another plan's worktree to register a return handoff, does not claim it. A ledger whose
  declared worktree already exists elsewhere is left to that owner unless the turn explicitly targets that
  worktree. A summary or paraphrased next action is not the handoff. `.agents/hooks/plan_handoff_stop.py`
  rejects the first incomplete final response and supplies the exact handoff for one bounded repair attempt;
  it never recursively blocks the repair response.

- Start with the worktree opener — `cd <absolute-worktree-path>`, or `/worktree create <Type>/<Name>` when
  the worktree doesn't exist yet — and keep it inside the paste-ready prompt.
- For **non-plan** work, make the prompt self-contained for zero context: name the branch or PR, relevant
  working files, and exact next action.
- For **plan-managed** work, introduce each handoff with one `Why:` line derived from the first paragraph of
  the ledger's current `## Next Steps`. This makes the purpose explicit without duplicating the operational
  procedure. Then end with the paste-ready prompt, which is ONLY the pointer — an opener line then the read
  line:

  ```text
  Why: `<LEDGER>_PROGRESS.md` owns unfinished work from this turn: <short next-action reason>
  ```

  ```
  <opener>
  Read @plans/<PLAN>_PLAN.md and @plans/<this-worktree-ledger>_PROGRESS.md and do what its `## Next Steps` says.
  ```

  Nothing follows the pointer. The final-response shape is a result summary, the `Why:` line, then the
  two-line pointer as the final content.

  The `<opener>` is `/worktree create <Type>/<epic>_<name>` when the plan's worktree doesn't exist yet — a
  freshly-written plan, after a clear with no live worktree, or normal continuation after a prior PR's
  worktree was removed — so implementation runs in an isolated worktree, never the main checkout; it's
  `cd <absolute-worktree-path>` once that worktree exists. Nothing else plan-specific goes in the prompt — no
  branch to verify, checkpoints, gates, commands, or next action; every such specific lives in the ledger (its
  header + `## Next Steps`), so the prompt can't drift.
- When work remains, end with one prompt that advances the ledger owned by this turn. Emit several only when
  this turn explicitly owns several independently executable worktrees. Do not emit pointers for other live
  worktrees merely because their ledgers were dependency inputs or received return-handoff edits. A delivery
  gate does not suppress an implementation pointer when the owned ledger has safe local work; only an
  implementation blocker does.
- A blocked plan never emits its own continuation pointer. First do any safe, authorized work that can remove
  the blocker in this session. If the gate still cannot move, record the four-line blocker schema from
  [`plans/agents/PLAN.md`](plans/agents/PLAN.md) at the start of `## Next Steps`, report those lines
  verbatim, and route the unblock action — a registered downstream handoff with no prompt, a dispatch prompt
  for the resolver, or a `Paused:` line for a human decision. Never re-poll an unchanged blocker into
  repeated "still blocked" commits. The Stop hook requires a blocked plan's four lines verbatim, rejects its
  continuation pointer, and stays silent for a paused plan.
- Use the handoff instead of asking whether to continue.
- Before an implementation PR merges, route through `/review` or `/big-review`; use `/incremental-review`
  after later code commits.
- When nothing remains, state that the work is complete without a continuation prompt.
