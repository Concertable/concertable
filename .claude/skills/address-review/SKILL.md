---
name: address-review
description: Automatically address the OPEN findings in a code-review file, one by one, each in its own fresh subagent context — fix clear defects (one commit per finding, no push), defer judgment calls for a human, then delete the review if everything was fixed cleanly. Use when the user wants to "address the review", "action the findings", "fix the review comments", "work through the review", or hands over a reviews/*.md to be actioned. For producing a review use /code-review or /big-review; this skill CONSUMES one.
---

# address-review

The fix-side counterpart to `/big-review-all`. It works through the open findings in a review
file so you don't have to fix each `- [ ]` by hand. Each finding is handled in **its own fresh
subagent context** (a big review's findings span many files — one clean context per finding
keeps each fix focused and avoids one bloated context), by running the
`.claude/workflows/address-review-all.js` workflow.

Aligns with `reviews/CLAUDE.md`: being handed a review file *means address its findings*, and a
review with nothing left to act on gets deleted.

## What to do

Invoke the workflow. This skill IS the explicit opt-in to run it:

```
Workflow({ name: 'address-review-all' })
```

- **Pass the review-file path when the user named one** (or when the branch has more than one
  review with open findings, which is common — a `BIG-*` plus a normal one):
  `Workflow({ name: 'address-review-all', args: 'reviews/BIG-<slug>-Review.md' })`.
- **No args** ⇒ the first subagent auto-detects the current branch's review file with open
  findings (prefers the highest wave / most recent) and threads it through the loop.

## Behaviour (fixed policy — don't reinvent it per run)

- **Sequential, one finding per subagent.** Never parallelise — each subagent edits code,
  verifies, and commits, so fixes must not overlap.
- **Fix clear defects; DEFER judgment calls.** A subagent fixes only unambiguous defects
  (correctness, isolation/boundary, seeding, convention nits with a stated fix). Anything framed
  as a tradeoff / "author's call" / subjective, or not high-confidence, is marked `- [-]`
  **DEFERRED** with the decision needed — code untouched. It never silently dismisses a finding.
- **One commit per finding, never pushed.** Pathspec-scoped (it will not sweep unrelated
  working-tree changes). Verification per fix is build + nearest unit/integration tests — **not
  E2E**. A final step runs a full solution build.
- **Delete only if all fixed cleanly.** When every finding was FIXED (nothing deferred) and the
  final build is green, the review file is `git rm`'d in a final commit (per `reviews/CLAUDE.md`).
  If anything was deferred or the build failed, the file stays with just the outstanding items.

## After it finishes

The workflow returns `{ reviewFile, fixed, deferred, buildPassed, reviewDeleted, notes }`. Report
concisely: which findings were fixed, which were **deferred and what decision each needs** (this
is the part the user must act on), whether the build passed, and whether the review file was
deleted or kept. The per-finding commits are unpushed — remind the user to review the diffs and
push when happy. Don't re-litigate the subagents' fixes here.

## When NOT to use

- Producing a review → `/code-review`, `/big-review`, `/big-review-all`.
- A `BIG-*` review whose Coverage checklist still has `[ ]`/`[~]` stages → finish the review
  first (`/big-review-all`); its findings aren't complete yet.
- The user wants to hand-fix / discuss findings rather than auto-apply → just work the file
  directly, no workflow.
