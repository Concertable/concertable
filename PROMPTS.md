# Prompts

- **Hard final-response gate for plan-managed work:** if this turn reads or edits a `_PROGRESS.md`
  ledger and its `## Next Steps` remains non-terminal, the response is incomplete until it ends with
  the exact two-line pointer below. “Implementation complete” is not “nothing remains” while review,
  PR, merge, publication, dependency, or platform-sync gates remain. A summary or paraphrased next
  action does not substitute for the pointer. The registered in-flight owner wait described below is
  the only exception. Trusted repository Stop hooks enforce this invariant.

- Start with the worktree opener — `cd <absolute-worktree-path>`, or `/worktree create <Type>/<Name>`
  when the worktree doesn't exist yet — and keep it inside the paste-ready prompt.
- For **non-plan** work, make the prompt self-contained for zero context: name the branch or PR,
  relevant working files, and exact next action.
- For **plan-managed** work the prompt is ONLY the pointer — an opener line then the read line:

  ```
  <opener>
  Read @plans/<PLAN>_PLAN.md and @plans/<this-worktree-ledger>_PROGRESS.md and do what its `## Next Steps` says.
  ```

  The `<opener>` is `/worktree create <Type>/<epic>_<name>` when the plan's worktree doesn't exist yet — a
  freshly-written plan, or after a clear with no live worktree — so implementation runs in an isolated
  worktree, never the main checkout; it's `cd <absolute-worktree-path>` once that worktree exists. Nothing
  else plan-specific goes in the prompt — no branch to verify, checkpoints, gates, commands, or next action;
  every such specific lives in the ledger (its header + `## Next Steps`), so the prompt can't drift.
- When work remains, end with one prompt that advances it — or, when several independent pieces remain,
  one prompt each so they run in separate contexts.
- When blocked, first check whether the resolving work is already in flight — an open PR mid-merge, or
  another session via the `search`/`recents` skills. If so, register this plan in the owner's
  `## Downstream handoffs`, report "waiting for X" (name it), and stop without emitting this plan's
  resume prompt; the owner surfaces it when the gate opens. Never re-poll a blocker into repeated
  "still blocked" recheck commits. If nobody is on it, hand off one prompt per blocker targeting the
  resolving work, each naming the worktree and the continuation it unlocks so the handoff routes back,
  and push any finished part meanwhile rather than parking it.
- Use the handoff instead of asking whether to continue.
- Before an implementation PR merges, route through `/code-review` or `/big-review`; use
  `/incremental-review` after later code commits.
- When nothing remains, state that the work is complete without a continuation prompt.
