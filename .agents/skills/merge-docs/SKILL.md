---
name: merge-docs
description: Land a docs/meta-only change as its own fast PR, bypassing the merge queue and its ~30-40min E2E gate via the sanctioned admin merge. Use when Tommy says "merge docs", "docs pr", "merge-docs", or wants markdown/agents/plans/skill changes shipped without the full queue. Hard-refuses if the diff touches runtime/product/package/CI-test-selection code — route those to /merge.
---

# merge-docs

A doc/meta-only change has **zero runtime blast radius**, so the full merge-queue E2E gate (~30-40 min)
is pure waste on it. This skill lands such a change through a small admin-merged PR — the `--admin`
bypass that the `/merge` skill reserves for doc-/config-only PRs, made into its own one-command flow.

**Docs-only is a hard precondition, not a hint.** If the diff touches anything with runtime, package,
schema, deployment, or test-selection consequence, STOP and use `/merge` — the queue must gate it.

- **In scope (meta-only):** `**/*.md`, `.agents/**`, `.claude/**`, `.codex/**`, `plans/**`, `docs/**`,
  `AGENTS.md`, `CLAUDE.md`, `README*`, `PROMPTS.md`. Nothing else.
- **Out of scope → route to `/merge`:** any `api/**` or `app/**` runtime/source, `package.json` /
  lockfiles / workspace config, `*.csproj` / CPM, `.github/workflows/**` (CI/test-selection logic),
  migrations, deployment artifacts. When unsure, it is **not** docs-only.

Plain `git`/`gh` only (personal repo — never the work PR/ADO skills).

## Steps

0. **Docs review first.** A docs/meta PR still gates on a review — just `/docs-review`, not
   `/review` (it has no runtime to code-review). Confirm a clean docs-review of this branch before
   the admin-merge below; if none exists or findings are open, stop and hand off a `/docs-review`
   prompt naming this worktree and branch. The `--admin` bypass skips the queue, so this is the only
   gate the change gets — don't skip it.

1. **Branch off `origin/main`, never local main.** If not already on a `Docs/<Name>` branch cut from
   `origin/main`, create one in its own worktree so a dirty main checkout is never disturbed:
   ```
   git fetch origin --quiet
   git worktree add <path> -b Docs/<Name> origin/main
   ```
2. **Prove the diff is meta-only** before anything else — this is the gate:
   ```
   git fetch origin --quiet
   git diff --name-only origin/main...HEAD
   ```
   Every path must match the in-scope list. **Any out-of-scope path → STOP and hand off to `/merge`.**
3. **Commit** if uncommitted (mandated `Co-Authored-By:` trailer), then `git push -u origin HEAD`.
4. **Open the PR** (plain `gh`, no work-item/assignee): `gh pr create --fill` (or a short title/body).
   Add `skip-e2e` so a queue fallback still skips E2E: `gh pr edit <n> --add-label skip-e2e`.
5. **Admin-merge — bypass the queue, no E2E** (docs-only is exactly what `--admin` is for):
   ```
   gh pr merge <n> --merge --admin      # NO --delete-branch: the queue rejects that flag
   gh pr view <n> --json state,mergeCommit
   ```
   - If `--admin` is refused (e.g. HTTP 401 on the local token), fall back to the queue **with the skip
     label**: `gh pr merge <n> --merge --auto`, then poll for `MERGED`. Never force past a red check.
6. **Return to clean main** (in the main checkout) and clean up:
   ```powershell
   git checkout main && git pull --ff-only origin main
   ./scripts/worktrees.ps1 close -Worktree <path> -PullRequest <n>
   ```
7. **No platform-sync** — a docs-only diff touches no `api/**`, so nothing republishes. Confirm and stop:
   `gh pr diff <n> --name-only | grep -q '^api/' && echo unexpected || echo "no sync (docs-only)"`.

One short report: PR number + merge commit, that it bypassed E2E (docs-only), and that main is clean.
If the diff wasn't docs-only, report that you stopped and routed to `/merge`.
