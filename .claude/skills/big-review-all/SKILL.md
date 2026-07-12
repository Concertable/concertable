---
name: big-review-all
description: Run the ENTIRE staged big-review to completion, unattended — every remaining stage, each in its own fresh subagent context (the automated equivalent of /big-review -> /clear -> /big-review, on a loop). Use when the user wants to "run the whole big review", "do all the stages", "big review everything", "finish the big review", or "big review all" instead of stepping through stages by hand with /big-review. For a single stage (one context, then stop) use /big-review; this skill drives all remaining stages in one go.
---

# big-review-all

This skill runs the **whole** staged big-review in one shot, so the user doesn't have to `/big-review` → `/clear` → `/big-review` by hand. Each stage still gets its own clean context — that's the point — because it runs the `.claude/workflows/big-review-all.js` workflow, which loops one fresh subagent per stage until every coverage-checklist area is `[x]`.

## What to do

Invoke the workflow. This skill IS the explicit opt-in to run it:

```
Workflow({ name: 'big-review-all' })
```

- **No arguments normally.** The workflow's first subagent auto-detects the active tracking file for the current branch (the `reviews/BIG-*Review*.md` with pending `[ ]`/`[~]` areas, preferring the highest wave suffix) and threads it through the loop.
- **Only** if the user names a specific tracking file (e.g. an old wave, or a non-default path), pass it: `Workflow({ name: 'big-review-all', args: 'reviews/BIG-<slug>-Review-Wave2.md' })`.
- The workflow is strictly sequential (stages share one file and leave cross-area notes for later stages) — do not try to parallelise it.

## After it finishes

The workflow returns `{ trackingFile, stagesRun, areasReviewed, complete }`. Report to the user, concisely:

- which tracking file was driven, how many stages ran, and whether the pass is `complete`;
- if `complete`, that the `## Summary` rollup and `Reviewed up to commit:` marker are stamped, and point them at the file to read the assembled findings;
- if NOT complete (e.g. a subagent died, or the safety cap was hit), say so plainly and tell them to re-run `/big-review-all` (it resumes from the checklist) or step the stuck stage manually with `/big-review`.

Do not re-review or second-guess the subagents' findings here — the file is the deliverable; the user reviews it.

## When NOT to use

- The checklist is already fully `[x]` → the pass is done; there's nothing to run. Point them at the file / `incremental-review` for new commits, per the big-review skill's own "When NOT to use".
- Just one stage wanted → `/big-review`.
- A normal-sized branch → `/code-review`.
