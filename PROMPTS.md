# Prompts

- Start with `cd <absolute-worktree-path>` and keep it inside the paste-ready prompt.
- For **non-plan** work, make the prompt self-contained for zero context: name the branch or PR,
  relevant working files, and exact next action.
- For **plan-managed** work the prompt is ONLY the pointer — literally:

  ```
  cd <absolute-worktree-path>
  Read @plans/<PLAN>_PLAN.md and @plans/<this-worktree-ledger>_PROGRESS.md and do what its `## Next Steps` says.
  ```

  Nothing plan-specific goes in the prompt — no branch to verify, no checkpoints to keep, no gates, no
  commands, no next action. Every one of those is a specific, and specifics live in the ledger (its
  header fields + `## Next Steps`), never in the prompt, so the prompt can't drift. A generic "read X
  and do what it says" is the only instruction the prompt carries.
- When work remains, end with one prompt that advances it — or, when several independent pieces remain,
  one prompt each so they run in separate contexts.
- When blocked, first check whether the resolving work is already in flight — an open PR mid-merge, or
  another session via the `search`/`recents` skills. If so, report "waiting for X" (name it) and stop;
  never re-poll a blocker into repeated "still blocked" recheck commits. If nobody is on it, hand off one
  prompt per blocker targeting the resolving work, each naming the worktree and the continuation it
  unlocks so the handoff routes back, and push any finished part meanwhile rather than parking it.
- Use the handoff instead of asking whether to continue.
- Before an implementation PR merges, route through `/code-review` or `/big-review`; use
  `/incremental-review` after later code commits.
- When nothing remains, state that the work is complete without a continuation prompt.
