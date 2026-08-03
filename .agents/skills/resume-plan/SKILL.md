---
name: resume-plan
description: Recover plan-managed work after chat or session context is lost. Use when Tommy invokes `/resume-plan` or asks where a plan, branch, PR, review, or multi-phase project has reached and wants one exact prompt to continue. Accept an optional referenced `plans/*.md` file; otherwise discover the active plan from the current worktree. Reconcile the plan and its companion `_PROGRESS.md` ledger against git, worktrees, reviews, tests, PRs, and package or merge gates before reporting status.
---

# Resume Plan

Reconstruct the work from durable evidence, correct stale progress documentation, report the current state, and finish with exactly one paste-ready prompt for the next actionable step.

## Locate the work

1. Read the repository `AGENTS.md` and `plans/AGENTS.md` completely.
2. Run `git worktree list --porcelain` before choosing a checkout.
3. If the invocation includes a plan reference such as `/resume-plan @plans/b2b/EXAMPLE.md`, use that plan as the anchor even when the current checkout is not its worktree. Resolve its declared branch or PR, then find the corresponding worktree. Search candidate worktrees when necessary.
4. Without a plan reference, inspect **every** worktree returned by step 2. For each listed absolute path, record its branch and upstream, `git status`, dirty plan and `_PROGRESS.md` files, `origin/main..HEAD` commits, review artifacts, and PR evidence for that branch. Correlate those signals into repository-relative candidate plan paths: direct plan changes; a ledger's Plan, Worktree, Branch, or PR fields; plan-declared branches or PRs; and plan names supported by commits, reviews, or PR metadata. Exclude `AGENTS.md` and `*_PROGRESS.md` from plan candidates. Select a plan only when this repository-wide correlation leaves exactly one candidate. If it leaves multiple plausible candidates, report **every** remaining candidate plan path with its supporting worktree, branch, and PR evidence, then ask for the plan path instead of inventing a status.
5. Use the plan's same-directory companion named `<PLAN_STEM>_PROGRESS.md` when it exists.

## Reconcile durable state

Treat documentation as a lead, not proof. Inspect at least:

- the absolute worktree path, current branch, upstream, `git status`, and `origin/main..HEAD` commits;
- the plan's phase checkboxes and requirements;
- the progress ledger's current-state summary, chronological events, reviews, verification, blockers, gates, and exact next action;
- review artifacts and whether their findings are open, fixed, deferred, or superseded;
- the current PR, checks, merge state, and merge-queue history when a PR exists;
- package publication or platform-sync state when it gates the next phase.

Never claim that code, tests, review, CI, publication, or merge work completed without evidence. Distinguish verified facts from unresolved or stale documentation.

## Handle legacy plans

If the plan has no companion progress ledger, do not interpret that absence as no progress. Reconstruct the current state from the plan plus git, review, test, PR, and package evidence. Create `<PLAN_STEM>_PROGRESS.md` from [the progress template](assets/progress-template.md) with an explicitly labelled reconstructed baseline; do not fabricate unavailable history. From that point onward, record all progress in the ledger.

When a ledger exists but reality has moved beyond it, update its current-state sections and append a reconciliation event describing the evidence. Stage only the ledger and commit the documentation update as a local checkpoint when repository rules allow it. Never push during this skill unless the user explicitly requests a push.

## Report and hand off

Give a compact status containing:

- plan, absolute worktree, branch, and PR;
- completed phases and material code changes;
- working-tree and commit state;
- verification and review state;
- current blockers or external gates;
- the immediate next actionable step.

Then provide exactly one self-contained resume prompt. Its first line must be:

`cd <absolute-worktree-path>`

The prompt must tell the next agent to read `AGENTS.md`, `plans/AGENTS.md`, the plan, and its progress ledger, verify the stated branch and current evidence, and perform the immediate next action. Include prerequisites or gates in that same prompt. Do not offer multiple prompts or ask whether to continue.

If all plan work is genuinely complete and verified, state that it is complete and provide no invented continuation prompt. Apply the plan lifecycle rules for removing the completed plan and progress ledger.
