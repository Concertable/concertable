---
name: pr-preflight
description: Pre-flight readiness gate — a READ-ONLY check of whether the current branch is CLEAR to open (or enqueue) a PR, run before you do. Verifies you're on a proper `<Type>/<Name>` branch (not main), your local is in sync with origin (not behind, not ahead-unpushed, not tracking a `[gone]` remote), the branch isn't badly stale vs main, all CODE is committed (docs/plans may ride uncommitted), no red/pending platform-sync gate is blocking merges, and no half-done published-package cut-over is left out of sync — then reports GREEN (clear, with the next command) or names exactly what's blocking and the fix. Use whenever Tommy says "can I PR this", "am I clear to PR", "ready to PR/merge?", "pr preflight", "check before I PR", "is this branch clean to ship", or before running `merge` or `gh pr create`. Concertable-specific (knows this repo's merge queue + platform-sync gate + branch conventions). Changes NO state — plain read-only `git`/`gh`.
---

# pr-preflight

One read-only pass that answers a single question: **is this branch clear to open/enqueue a PR right
now?** It runs the preconditions that, when missed, get a PR rejected, ejected, or built on stale
state — and reports a plain **GREEN (go)** or a list of blockers with the exact fix for each.

This skill is **Concertable-specific**. It encodes the sync/merge preconditions this repo actually
enforces (see "Repo facts").

## Read-only — this skill NEVER changes state

It runs only inspection commands (`git status`, `git rev-list`, `git fetch`, `gh pr view/list`). It
does **not** commit, push, checkout, pull, `sync`, rebase, or `gh pr merge` — **fixing** a blocker is
a separate, user-invoked step (`commit`, `push`, `sync`, `merge`, or finishing the cut-over). The
one write it may do is a `git fetch` to refresh remote-tracking refs so the sync check is accurate; it
never changes the working tree, index, or any branch.

## Repo facts (why these checks exist)

- **`main` is protected by a merge queue** that rebuilds each entry on current main — so a branch
  that's merely *behind main* is still mergeable (the queue rebases it). What actually bites is your
  **local being out of sync with its own remote**: behind `origin/<branch>` (you'd push over newer
  work or merge a stale PR) or tracking a **`[gone]`** remote (the branch already merged + was
  deleted — you're on a dead branch). This is the exact "sync thing" that blocks a clean PR.
- **platform-sync is a first-class merge gate.** An auto-created `chore/platform-sync-<version>` PR
  bumps `ConcertablePlatformVersion`; while one is **red or pending**, merges are gated. Opening a PR
  behind a red sync gate just parks it.
- **Never leave a published-package refactor half-done.** A namespace/type move in a packable
  `Concertable.*` (Kernel, Contracts, Messaging.Domain, Payment.*, …) is an expand→republish→sync
  cut-over; the definition-of-done is the **old-namespace grep returning 0** across the repo. A PR
  opened mid-cut-over is out of sync by construction.
- **Docs/plans ride uncommitted; only CODE blocks.** Per the root `AGENTS.md`, uncommitted
  markdown/plans/scratch notes are fine — they travel with the next commit. Only uncommitted **code**
  (`.cs`, `.csproj`, `.props`, `.targets`, `.ts`, `.tsx`, workflow `.yml`, …) means the PR would ship
  incomplete / be reviewed against stale committed history.

## Checks

1. **On a real feature branch.**
   ```
   git rev-parse --abbrev-ref HEAD
   ```
   - **main** → BLOCKER (nothing to PR; branch first).
   - Not `<Type>/<Name>` with a **capitalized** prefix (`Feature/`, `Fix/`, `Bug/`, `Refactor/`, …) →
     warn (see `AGENTS.md` "Git branch"; never a lowercase `feature/…`).

2. **Local in sync with origin (the sync gate).**
   ```
   git fetch -q origin
   git status -sb | head -1
   ```
   - `...origin/<branch> [gone]` → BLOCKER: remote deleted (branch already merged). `sync` to clean
     main; don't PR a dead branch.
   - `[behind N]` → BLOCKER: local is stale. `sync` or `pull` first.
   - `[ahead N]` → note: N unpushed commits — `push` them (a PR needs them on origin). Not a blocker
     for `gh pr create` (it pushes), but is one for `merge`.

3. **Staleness vs main (soft).**
   ```
   git rev-list --left-right --count origin/main...HEAD
   ```
   - `A  B` = A commits on main you don't have, B commits of yours. Large A → note the branch is
     well behind main; the queue rebuilds so it's not fatal, but a merge of main avoids surprises.

4. **All CODE committed (docs may ride).**
   ```
   git status --porcelain
   ```
   - Any uncommitted **code** path → BLOCKER: `commit` first (review runs on committed history).
   - Only `*.md` / `plans/*` / scratch docs dirty → OK; say "docs ride along" and move on (don't fuss).

5. **Existing PR for this branch.**
   ```
   gh pr view --json number,state,url --jq '{number,state,url}'
   ```
   - Already `OPEN` → you don't open a new one; the next step is `push` (if ahead) then `merge`.
   - `MERGED`/`CLOSED` → report it; likely `sync` and start fresh.

6. **platform-sync gate not red.**
   ```
   gh pr list --state open --search "head:chore/platform-sync" --json number,title,url
   ```
   - If one exists, check its status (`gh pr checks <n>`). **Red/pending** → BLOCKER-ish: merges are
     gated until it goes green; surface it so it's triaged (see `AGENTS.md` merge-monitor section).

7. **No half-done published-package cut-over.** If this branch moved/renamed a type in a packable
   `Concertable.*`, run the rename definition-of-done grep for the OLD identity:
   ```
   grep -rniE "<old.namespace.or.type>" -- . ':!*/obj/*' ':!*/bin/*'
   ```
   - Non-zero outside a written allowlist → BLOCKER: the cut-over is out of sync (`plans/AGENTS.md`
     "Never leave the codebase out of sync"). Skip this check entirely if the branch touched no
     packable contract.

8. **(Optional, slow) Local build green.** The merge queue is the real build/test/E2E gate, but a
   local build catches an obvious break before you spend a ~30-40 min queue cycle:
   ```
   dotnet build api/Concertable.slnx
   ```
   Run it for full confidence on a non-trivial change; skip it for a small/doc change and let the queue
   be the gate.

## Verdict

- **GREEN — clear to PR.** State it plainly and give the next command: `push` (if ahead) → `gh pr create`
  for a new PR, or `merge` if a PR already exists.
- **Not clear.** List each blocker on its own line with the exact fix skill (`sync`, `commit`,
  `push`, fix/wait platform-sync, finish the cut-over) — most-blocking first. Don't fix anything here;
  report and stop.

## Final summary

Before any report or stop, if this workflow is plan-managed, read and apply
[the shared plan-progress checkpoint](../resume-plan/references/plan-progress-checkpoint.md). The
ledger checkpoint is the only permitted write in this otherwise read-only workflow.

One short go/no-go: GREEN + the next command, or the named blockers + their fixes. Read-only
`git`/`gh` only (personal repo — never the work PR/ADO skills). No preamble; inspect → verdict → stop.
