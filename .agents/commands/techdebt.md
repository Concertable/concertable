---
description: Pick one tech-debt item and take it all the way to a PR, in an isolated worktree
---
Start a self-contained tech-debt session in an **isolated worktree** so nothing touches in-flight branches.

1. **Workspace.** If a `Chore/TechDebt` worktree already exists, use it; otherwise spin one up, branching fresh from `origin/main`:
   ```bash
   git fetch origin --quiet
   git worktree add ../Concertable.worktrees/<Branch> -b <Type>/<Name> origin/main
   ```
   Each item gets its own branch off current `origin/main` — `Refactor/<Name>`, `Fix/<Name>`, or `Chore/<Name>` as fits. (In Claude Code the `worktree` skill does this setup.)
2. **Pick one item.** Survey every `TECH_DEBT.md` in the repo, then choose a **single** item that's high-value and self-contained enough to land cleanly. Say which you picked and why in a couple of lines before diving in.
3. **Investigate.** Read the surrounding code and understand the real root cause before touching anything. If the item as written is stale or wrong, say so.
4. **Fix it properly — the rule is absolute:** always the long-term, scalable solution, never the hacky shortcut, even when it's harder or spans **multiple PRs**. Done right beats done fast. If it genuinely needs splitting, say so in one line and start with the first PR on the current branch.
5. **Verify.** Build the affected projects to zero errors and run the affected unit/integration tests. E2E only if the change is genuinely risky (per `plans/AGENTS.md`); otherwise add the `Skip-E2E: true` git trailer.
6. **Close the loop.** In the same commit that lands the work, delete the resolved entry from its `TECH_DEBT.md`. Then push and open a plain `gh pr create` PR (personal repo — no Azure DevOps, no `AB#`, no assignee).

Don't ask to confirm reversible steps — investigate, fix, build, commit, and push on the branch. Just surface the item you picked before starting, and flag anything irreversible.
