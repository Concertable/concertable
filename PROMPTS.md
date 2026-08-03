# Prompts

- Start with `cd <absolute-worktree-path>` and keep it inside the paste-ready prompt.
- Make the prompt self-contained for zero context: name the branch or PR, relevant working files, and
  exact next action.
- When work remains, end with one prompt that advances it.
- When blocked, target the resolving work and include the original worktree and continuation it unlocks
  so the next handoff routes back.
- Use the handoff instead of asking whether to continue.
- Before an implementation PR merges, route through `/code-review` or `/big-review`; use
  `/incremental-review` after later code commits.
- When nothing remains, state that the work is complete without a continuation prompt.
