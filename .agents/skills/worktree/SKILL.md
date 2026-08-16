---
name: worktree
description: Create, inspect, and close one isolated Concertable git worktree for a branch or PR. Use when Tommy says `$worktree`, `/worktree`, "create a worktree", "new worktree", "isolate this branch/PR", "list worktrees", or "remove/close this worktree". Use `scripts/worktrees.ps1` for repository-owned audit, close, and retire operations; use `resume-plan` when resuming plan-managed work from a ledger.
---

# Worktree

Keep every in-flight PR in its own checkout. Treat worktree creation as setup: when the invocation also
contains a task, continue that task in the new worktree during the same session.

## Choose the operation

- Create or restore one branch checkout: follow **Create** below.
- Resume plan-managed work: use `resume-plan`; its ledger owns the branch, PR, and worktree identity.
- Read-only inventory: run `./scripts/worktrees.ps1 audit`.
- Close a merged PR worktree: run `./scripts/worktrees.ps1 close` with its exact path and PR number.
- Retire an explicitly superseded no-PR branch: run `./scripts/worktrees.ps1 retire` with the required
  durable evidence. Never substitute a manual deletion.
- Sweep all dead worktrees: use `prune-worktrees`, not this single-worktree workflow.

## Create

1. Read the repository `AGENTS.md` and apply its worktree identity gate. Do not split code that exists
   only on the current feature branch into a new branch.
2. From any checkout, resolve the common repository root with Git. Inspect the current branch, dirty
   paths, registered worktrees, matching local/remote refs, and open platform-sync PRs. Stop if the task
   belongs to an existing worktree, a case-colliding branch exists, or a red platform-sync gate blocks
   new work.
3. Fetch `origin` with pruning. New branches must use the repository's capitalized `<Type>/<Name>` form
   and start at current `origin/main`, never local `main`. If the exact branch already exists, restore
   that branch instead of creating it again and verify whether it has an open PR.
4. Add the checkout beneath the repository's `.worktrees` directory. Flatten `/` to `-` in the folder
   name so branch hierarchy does not create ambiguous nested worktree roots. Never use `.Codex/worktrees`;
   that location belongs to the harness.

```powershell
$commonDirectory = git rev-parse --path-format=absolute --git-common-dir
$repository = [IO.Path]::GetDirectoryName($commonDirectory.Trim())
$branch = '<Type>/<Name>'
$folder = $branch.Replace('/', '-')
$path = Join-Path $repository ".worktrees/$folder"
git -C $repository fetch origin --prune
git -C $repository worktree add $path -b $branch origin/main
```

For an existing local branch, omit `-b` and `origin/main`:

```powershell
git -C $repository worktree add $path $branch
```

For a remote-only branch, create its matching local tracking ref:

```powershell
git -C $repository worktree add $path -b $branch --track "origin/$branch"
```

5. Verify the resulting path, branch, HEAD, base or existing remote head, and clean status. Use absolute
   worktree paths for all subsequent tool calls in the current session.

Do not copy or junction tracked `.agents` or `.claude` directories: they arrive with the checkout.

## Inspect

Use the repository automation because it classifies PR state, dirty work, detached trees, case
collisions, persistent branches, and orphan folders without deleting anything:

```powershell
./scripts/worktrees.ps1 audit
```

Use `git worktree list --porcelain` only when raw registration data is needed during diagnosis.

## Close a merged PR worktree

Run from a different checkout. The command verifies cleanliness, exact branch/PR/head identity, merge
containment in current `origin/main`, case safety, and folder removal:

```powershell
./scripts/worktrees.ps1 close -Worktree <absolute-path> -PullRequest <number>
```

Add `-PlanManaged` when the worktree belongs to a live plan ledger:

```powershell
./scripts/worktrees.ps1 close -Worktree <absolute-path> -PullRequest <number> -PlanManaged
```

Never force-remove an unmerged or dirty tree. A superseded branch without a PR requires the repository's
`retire` evidence flow documented by `./scripts/worktrees.ps1` and `AGENTS.md`.

## Report

For creation, report the branch, absolute path, HEAD/base, and whether the attached task continued.
For inspection or closure, report the script's classification or verified surviving worktree list.
