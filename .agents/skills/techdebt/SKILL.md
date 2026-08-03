---
name: techdebt
description: Pick ONE tech-debt item and take it all the way to a PR, in an isolated worktree so nothing touches the in-flight branch. Surveys every TECH_DEBT.md in the repo, picks a single high-value self-contained item (stating which and why first), fixes it the proper long-term way (never the hacky shortcut, even across multiple PRs), verifies (build affected projects to 0 errors + affected unit/integration tests; E2E only if genuinely risky), then in the SAME commit deletes the resolved entry from its TECH_DEBT.md and opens a plain PR. Use whenever Tommy says "techdebt", "/techdebt", "do some tech debt", "pick a tech-debt item", "pay down tech debt", or "clear a TECH_DEBT entry". Concertable-specific (knows this repo's TECH_DEBT.md layout, worktree + branch conventions, platform-sync/E2E gates, and personal-repo PR flow — no Azure DevOps, no AB#).
---

# techdebt

Run a self-contained tech-debt session in an **isolated worktree** so nothing touches the in-flight
branch. One item, done properly, all the way to a PR.

Decide and act on the reversible steps (investigate, fix, build, commit, push) without asking — only
surface the item picked up front, and flag anything irreversible.

## 1. Workspace — isolated worktree

If a `Chore/TechDebt` worktree already exists, use it. Otherwise spin one up, branching **fresh from
`origin/main`** (never local `main`, which drifts):

```bash
git fetch origin --quiet
git worktree add ../Concertable.worktrees/<Branch> -b <Type>/<Name> origin/main
```

Each item gets its own branch off current `origin/main`, named for what it is — `Refactor/<Name>`,
`Fix/<Name>`, or `Chore/<Name>`. (In Claude Code the `worktree` skill does this setup; other agents
run the `git worktree` command above.)

## 2. Pick ONE item

Survey **every** `TECH_DEBT.md` in the repo, then choose a **single** item that's high-value and
self-contained enough to land cleanly. State which one was picked and why in a couple of lines before
diving in.

## 3. Investigate

Read the surrounding code and understand the real root cause before touching anything. If the item as
written is stale or wrong, say so rather than fixing a non-problem.

## 4. Fix it properly — the rule is absolute

Always the long-term, scalable solution, never the hacky shortcut — even when it's harder or spans
**multiple PRs**. Done right beats done fast. If it genuinely needs splitting, say so in one line and
start with the first PR on the current branch (don't fragment it across speculative future branches).

## 5. Verify

Build the affected projects to **zero errors** and run the affected unit/integration tests. Run E2E
only if the change is genuinely risky (per `plans/AGENTS.md`); otherwise add the `Skip-E2E: true`
git trailer to the commit so the merge queue skips it too.

## 6. Close the loop

In the **same commit** that lands the work, delete the resolved entry from its `TECH_DEBT.md` (per the
`AGENTS.md` rule that resolved debt isn't archived). Then push and open a plain PR:

```bash
gh pr create
```

Personal repo — no Azure DevOps, no `AB#`, no assignee.
