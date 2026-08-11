# Prompts

- **Hard final-response gate for plan-managed work:** if a `_PROGRESS.md` ledger with non-terminal
  `## Next Steps` is owned by the current or explicitly targeted worktree, the response is incomplete
  until it ends with the explained, collision-safe pointer below. Merely naming a dependency ledger
  for reading, or editing its copy from another plan's worktree to register a return handoff, does not
  claim it. A ledger whose declared worktree already exists elsewhere is left to that owner unless
  the turn explicitly targets that worktree. “Implementation complete” is not “nothing remains” while review,
  PR, merge, publication, dependency, or platform-sync gates remain. A summary or paraphrased next
  action does not substitute for the pointer. A structured blocked state described below is the
  exception. Trusted repository Stop hooks enforce both sides of this invariant: actionable work
  requires its pointer, while blocked work forbids that pointer.

- Start with the worktree opener — `cd <absolute-worktree-path>`, or `/worktree create <Type>/<Name>`
  when the worktree doesn't exist yet — and keep it inside the paste-ready prompt.
- For **non-plan** work, make the prompt self-contained for zero context: name the branch or PR,
  relevant working files, and exact next action.
- For **plan-managed** work, introduce each handoff with one `Why:` line derived from the first
  paragraph of the ledger's current `## Next Steps`, then state that the continuation must run only if
  no agent or session is already working in the declared worktree. This makes the purpose and collision
  boundary explicit without duplicating the full operational procedure. Then end with the paste-ready
  prompt, which is ONLY the pointer — an opener line then the read line:

  ```text
  Why: `<LEDGER>_PROGRESS.md` owns unfinished work from this turn: <short next-action reason>
  Only run this continuation if no agent or session is already working in `<declared-worktree>`.
  ```

  ```
  <opener>
  Read @plans/<PLAN>_PLAN.md and @plans/<this-worktree-ledger>_PROGRESS.md and do what its `## Next Steps` says.
  ```

  Nothing follows the pointer. The final-response shape is therefore a result summary, the exact
  `Why:` and collision-warning lines, then the exact two-line pointer as the final content.

  The `<opener>` is `/worktree create <Type>/<epic>_<name>` when the plan's worktree doesn't exist yet — a
  freshly-written plan, after a clear with no live worktree, or normal continuation after a prior PR's
  worktree was removed — so implementation runs in an isolated
  worktree, never the main checkout; it's `cd <absolute-worktree-path>` once that worktree exists. Nothing
  else plan-specific goes in the prompt — no branch to verify, checkpoints, gates, commands, or next action;
  every such specific lives in the ledger (its header + `## Next Steps`), so the prompt can't drift.
- When work remains, end with one prompt that advances the ledger owned by this turn. Emit several only
  when this turn explicitly owns several independently executable worktrees. Do not emit pointers for
  other live worktrees merely because their ledgers were dependency inputs or received return-handoff
  edits. A delivery gate does not suppress an implementation pointer when the owned ledger has safe
  local work; only an implementation blocker does.
- A blocked plan never emits its own continuation pointer. First do any safe, authorized work that can
  remove the blocker in the current session. If the gate still cannot move, record the three-line hard
  blocker schema from [`plans/agents/PLAN.md`](plans/agents/PLAN.md) at the start of `## Next Steps` and
  report those lines verbatim to Tommy. Then route the unblock action instead of routing back into the
  blocked plan:
  - resolving work already in flight — register the waiting ledger in the owner's
    `## Downstream handoffs`, name that owner, and emit no prompt; the owner surfaces the waiting
    plan's pointer when the gate opens;
  - separate agent/context required and nobody owns it — emit one paste-ready dispatch prompt for the
    resolving work, including its return path to the waiting ledger;
  - user or external-system action required — give the exact command/action and the observable
    `Resume when` condition, with no prompt.

  Never re-poll an unchanged blocker into repeated "still blocked" commits. The Stop hook rejects the
  blocked plan's pointer and rejects a blocker report that omits any of the three exact lines.
- Use the handoff instead of asking whether to continue.
- Before an implementation PR merges, route through `/review` or `/big-review`; use
  `/incremental-review` after later code commits.
- When nothing remains, state that the work is complete without a continuation prompt.
