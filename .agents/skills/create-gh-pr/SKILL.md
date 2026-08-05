---
name: create-gh-pr
---

# create-gh-pr

Open the PR for the current branch and hand off. One job: get committed work onto a GitHub PR with a
title and body drafted from the branch, then stop. It does **not** enqueue, label the E2E tier, or wait
for a merge — that is the `merge` skill.

This skill is **Concertable-specific and personal-repo-only**. It is the deliberately-slim sibling of

## Repo facts (what makes this the slim one)

- **GitHub only — no Azure DevOps.** No `AB#`, no work item, no `create-devops-item`, no `az boards`
  transition, no assignee. Plain `gh pr create`. If any instinct says "link the work item", that is the
  wrong (work) skill leaking in — drop it.
- **Claude attribution stays.** Opposite of the work skill: commits here carry the mandated
  `Co-Authored-By: Claude ...` trailer, and the PR body ends with the `🤖 Generated with [Claude Code]`
  footer. Never strip either.
- **The E2E tier and labels are `merge`'s call, not this skill's.** `merge` Step 4 is the single source
  of truth for `skip-e2e` / `full-e2e`, read fresh in the merge group. Do not set E2E labels at
  PR-create time — you'd just have to reconcile them at merge.
- **Docs/plans ride uncommitted; only CODE blocks a PR.** Per the root `AGENTS.md`, uncommitted
  markdown/plans/scratch notes travel with the next commit. Only uncommitted **code** means the PR would
  ship incomplete.

## Steps

### 1. Readiness gate

Run `pr-preflight` (or its checks inline) and stop on any blocker it names — on a real `<Type>/<Name>`
branch (not `main`), local in sync with origin, no red/pending `chore/platform-sync-*` gate, no half-done
published-package cut-over. Fix the blocker with the named skill, then come back. Do not open a PR over a
blocker.

### 2. Uncommitted work

```
git status --porcelain
```

- Only `*.md` / `plans/*` / scratch docs dirty → fine, they ride the next commit. Don't fuss.
- Any uncommitted **code** (`.cs`, `.csproj`, `.props`, `.ts`, `.tsx`, workflow `.yml`, …) → **stop and
  ask** whether to commit it first (offer an inferred message via the `commit` skill). A PR only contains
  committed work; don't auto-commit code silently.

### 3. Push the branch

```
git push -u origin HEAD
```

Only if there's no upstream or the branch is ahead of its remote.

### 4. Draft the title and body from the branch

- **Title**: concise, ≤ 70 chars, states the change — not "fix bug" but the actual fix.
- Read the branch to draft from — `git log --oneline origin/main..HEAD` (drop merge commits) and
  `git diff --stat origin/main...HEAD`.
- **Body** — factual, focused on the change:

```
## Summary
<1–3 sentences: what this does and why>

## What changed
- <area / behaviour bullets, drawn from the commits>

## Test coverage
- <what's verified: build + the affected unit/integration/web builds, and which behaviours are asserted>

## Notes
- <E2E tier if non-default and why; platform-sync if `api/**` changed; anything a reviewer needs>

🤖 Generated with [Claude Code](https://claude.com/claude-code)
```

Drop a section that has nothing to say; keep the footer.

### 5. Create the PR

```
gh pr create --title "<title>" --body "$(cat <<'EOF'
<body>
EOF
)"
```

- Add `--base <branch>` only if targeting something other than `main`; `--draft` only if asked.
- No `--assignee`, no `AB#` — this is the personal repo.

### 6. Report and hand off

Print the PR URL. If a plan owns this work, its next step is landing — say so: **`merge`** takes it from
here (currency update, E2E-tier decision, enqueue, wait for MERGED, then the `chore/platform-sync-*`
consequence if `api/**` changed). This skill stops at the open PR.

## Final summary

Before any report or stop, if this workflow is plan-managed, read and apply
[the shared plan-progress checkpoint](../resume-plan/references/plan-progress-checkpoint.md).

One line: the PR opened (number + URL), and that `merge` is the next step. Plain `git`/`gh` only —
never the work PR/ADO skills.
