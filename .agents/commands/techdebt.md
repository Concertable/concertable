---
description: Pick one tech-debt item and take it all the way to a PR, in an isolated worktree
---

# /techdebt

Start a self-contained tech-debt session in an isolated worktree so nothing touches in-flight branches.

## Tech-debt worktrees

Keep all tech-debt sessions under `../Concertable.worktrees/TechDebt`.
Use one folder per run so you can identify debt worktrees at a glance.

1. Workspace.
   - One-at-a-time mode (legacy):
     ```bash
     git fetch origin --quiet
     git worktree add ../Concertable.worktrees/TechDebt/legacy -b Chore/TechDebt/legacy origin/main
     ```
   - Parallel mode (recommended):
     Choose one unique slug per item, e.g. `api-logging`.
     ```bash
     # item_slug should be short and unique for this item
     slug="techdebt-${item_slug}-$(date -u +%Y%m%d-%H%M%S)"
     git worktree add ../Concertable.worktrees/TechDebt/$slug -b Chore/TechDebt/$slug origin/main
     ```
   In parallel mode, each run owns one item and one branch.
2. Pick one item. Survey every `TECH_DEBT.md` in the repo, then choose a single, high-value item. It can span multiple PRs (step 4). Say which you picked and why in a couple of lines before diving in.
3. Investigate. Read the surrounding code and understand the real root cause before touching anything. If the item as written is stale or wrong, say so.
4. Fix it properly. The rule is absolute: always use the long-term, scalable solution, never the hacky shortcut, even when it's harder or spans multiple PRs. If it genuinely needs splitting, say so in one line and start with the first PR on the current branch.
5. Verify. Build the affected projects to zero errors and run the affected unit/integration tests. Do not run local E2E for PR-bound work; the merge queue owns that gate under `plans/AGENTS.md`.
6. Close the loop. In the same commit that lands the work, delete the resolved entry from its `TECH_DEBT.md` -- or, for a multi-PR cut-over, the final PR deletes it while earlier PRs record progress in the entry. Then push and open a plain `gh pr create` PR (personal repo -- no Azure DevOps, no `AB#`, no assignee). Do not set an E2E label while opening the PR; the merge skill's Step 4 selects and normalizes the tier mechanically at merge time.

Don't ask to confirm reversible steps -- investigate, fix, build, commit, and push on the branch. Just surface the item you picked before starting, and flag anything irreversible.
