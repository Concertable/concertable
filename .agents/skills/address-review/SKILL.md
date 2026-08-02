---
name: address-review
description: Automatically address the OPEN findings in a code-review file, one by one, each in its own fresh agent context - fix clear defects, defer judgment calls for a human, commit each fix separately, and delete the review if everything was fixed cleanly. Use when the user wants to "address the review", "action the findings", "fix the review comments", "work through the review", or hands over a reviews/*.md to be actioned. For producing a review use `code-review`, `big-review`, or `big-review-all`; this skill consumes one.
---

# address-review

The fix-side counterpart to `big-review-all`. It works through the open findings in a review file so you do not have to fix each `- [ ]` by hand. Each finding is handled in **its own fresh agent context** so each fix stays focused.

Aligns with `reviews/AGENTS.md`: being handed a review file means address its findings, and a review with nothing left to act on gets deleted.

## What to do

Use Codex multi-agent tooling when available. Spawn exactly one fresh agent context for the next open finding, wait for it to finish, inspect the review file and git state, then continue with the next open finding. Keep the sequence strictly serial because each agent edits, verifies, and commits.

If multi-agent tooling is unavailable, work the findings yourself one at a time with the same policy below.

- **Pass the review-file path when the user named one** or when the branch has more than one review with open findings, which is common with a `BIG-*` plus a normal review.
- **No args** means auto-detect the current branch's review file with open findings, preferring the highest wave or most recent file.

## Behaviour

- **Sequential, one finding per agent context.** Never parallelise fixes.
- **Fix clear defects; defer judgment calls.** Fix only unambiguous defects: correctness, isolation/boundary, seeding, convention nits with a stated fix. Anything framed as a tradeoff, author's call, subjective point, or not high-confidence is marked `- [-] DEFERRED` with the decision needed. Code stays untouched for deferred findings.
- **One commit per finding, never pushed.** Use pathspec-scoped staging so unrelated working-tree changes are not swept in. Verification per fix is build plus nearest unit/integration tests, not E2E. A final step runs a full solution build.
- **Review the fix commits before deleting the watermark.** When code changed, run `incremental-review`
  over the commits added since the recorded review SHA while the review file still exists. Any new
  findings re-enter this same serial fix loop; rebuild and repeat until the incremental pass is clean.
- **Delete only if all fixed cleanly.** When every finding was fixed, nothing was deferred, and the final build is green, `git rm` the review file in a final commit per `reviews/AGENTS.md`. If anything was deferred or the build failed, keep the file with just the outstanding items.

## After it finishes

Resolve the next workflow state before reporting. Never stop at the generic “review the diffs and push
when happy” handoff.

1. Determine whether the review came from an implementation plan and inspect that plan's current
   state (including whether its completing commit deleted it).
2. If an incomplete phase remains, follow `plans/AGENTS.md`: give the one exact resume prompt for that
   phase, including the worktree path when applicable. Do not suggest opening a PR.
3. If the plan is complete/deleted, or there was no plan, run the read-only `pr-preflight` skill.
   Report its verdict and the exact next action: push then plain `gh pr create` for a new personal-repo
   PR, push to update an existing PR, or the named blocker and its fix.

Then report concisely: which findings were fixed, which were deferred and what decision each needs,
whether the build passed, whether the review file was deleted or kept, and the per-finding unpushed
commit SHAs. Do not re-litigate the agents' fixes.

## When NOT to use

- Producing a review -> `code-review`, `big-review`, or `big-review-all`.
- A `BIG-*` review whose Coverage checklist still has `[ ]` or `[~]` stages -> finish the review first with `big-review-all`; its findings are not complete yet.
- The user wants to hand-fix or discuss findings rather than auto-apply -> work the file directly, no multi-agent loop.
