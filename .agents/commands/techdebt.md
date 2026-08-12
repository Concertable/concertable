---
description: Pick one tech-debt item and take it all the way to a PR, in an isolated worktree
---

# /techdebt

Start a self-contained tech-debt session in an **isolated worktree** so nothing touches in-flight branches.

1. **Workspace.** The branch and worktree are ALWAYS named exactly `Chore/TechDebt` — never a name derived from the item you pick. If a `Chore/TechDebt` worktree already exists, use it; otherwise create it, branching fresh from `origin/main`:
   ```bash
   git fetch origin --quiet
   git worktree add ../Concertable.worktrees/Chore/TechDebt -b Chore/TechDebt origin/main
   ```
   The item picked in step 2 never changes the branch name. This worktree is **persistent — do NOT delete it after the PR merges**; reuse it for the next item. This is an explicit exception to the usual post-merge worktree teardown.
2. **Pick one item.** Survey every `TECH_DEBT.md` in the repo, then choose a **single**, high-value item — it can span multiple PRs (step 4). Say which you picked and why in a couple of lines before diving in.
3. **Investigate.** Read the surrounding code and understand the real root cause before touching anything. If the item as written is stale or wrong, say so.
4. **Fix it properly — the rule is absolute:** always the long-term, scalable solution, never the hacky shortcut, even when it's harder or spans **multiple PRs**. Done right beats done fast. If it genuinely needs splitting, say so in one line and start with the first PR on the current branch.
5. **Verify.** Build the affected projects to zero errors and run the affected unit/integration tests. Do not run local E2E for PR-bound work; the merge queue owns that gate under `plans/AGENTS.md`.
6. **Close the loop.** In the same commit that lands the work, delete the resolved entry from its `TECH_DEBT.md` — or, for a multi-PR cut-over, the **final** PR deletes it while earlier PRs record progress in the entry. Then push and open a plain `gh pr create` PR (personal repo — no Azure DevOps, no `AB#`, no assignee). Do not set an E2E label while opening the PR; the merge skill's Step 4 selects and normalizes the tier mechanically at merge time.

Don't ask to confirm reversible steps — investigate, fix, build, commit, and push on the branch. Just surface the item you picked before starting, and flag anything irreversible.
