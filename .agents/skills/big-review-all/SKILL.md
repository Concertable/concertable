---
name: big-review-all
description: Run the ENTIRE staged big-review to completion, unattended - every remaining stage, each in its own fresh agent context. Use when the user wants to "run the whole big review", "do all the stages", "big review everything", "finish the big review", or "big review all" instead of stepping through stages by hand with the `big-review` skill. For a single stage, use `big-review`; this skill drives all remaining stages in one go.
---

# big-review-all

This skill runs the **whole** staged big-review in one shot. Each stage still gets its own clean context; that is the point.

## What to do

Use Codex multi-agent tooling when available. Spawn exactly one fresh agent context for the next pending review stage, wait for it to finish, inspect the tracking file, then continue with the next pending stage. Keep the sequence strictly serial because stages share one tracking file and later stages may consume cross-area notes from earlier ones.

If multi-agent tooling is unavailable, run the `big-review` skill yourself one stage at a time, preserving the same checkpoint discipline between stages.

- **No arguments normally.** Auto-detect the active tracking file for the current branch: the `reviews/BIG-*Review*.md` file with pending `[ ]` or `[~]` areas, preferring the highest wave suffix.
- **Only** if the user names a specific tracking file, use that path, for example `reviews/BIG-<slug>-Review-Wave2.md`.
- Never parallelise stages.

## After it finishes

Report concisely:

- which tracking file was driven, how many stages ran, and whether the pass is `complete`;
- if `complete`, that the `## Summary` rollup and `Reviewed up to commit:` marker are stamped, and point them at the file to read the assembled findings;
- if not complete, say so plainly and tell them to re-run `big-review-all` or step the stuck stage manually with `big-review`.

Do not re-review or second-guess the agents' findings here. The file is the deliverable; the user reviews it.

## When NOT to use

- The checklist is already fully `[x]` -> the pass is done; there is nothing to run. Point them at the file or `incremental-review` for new commits, per the `big-review` skill's own "When NOT to use".
- Just one stage wanted -> `big-review`.
- A normal-sized branch -> `code-review`.
