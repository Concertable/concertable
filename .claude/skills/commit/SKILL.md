---
name: commit
description: Turn the working tree into clean, logical git commits. Use whenever the user asks to commit work — surveys staged/unstaged/untracked state, slices changes into coherent commits with honest messages, excludes junk, and verifies the result. Generic — applies to any repo.
---

# commit

Turn whatever is in the working tree into a readable commit history. The job is curation, not just `git add -A && git commit`.

## Fast path — single commit (skip everything below)

If the user signals they want ONE commit of everything — "commit all", "commit-all", "one commit", "just commit everything", "everything in one", or similar — **do not survey or slice**. It's one button press:

```
git add -A
git commit -m "<one-line summary of the change>"
git log --oneline -1   # confirm
```

- No `git status`/`git diff` survey, no per-file dumps, no slicing analysis — that's exactly the ceremony they're opting out of.
- Still: don't commit on the default branch (branch first), don't `--no-verify`, no AI-attribution trailer.
- If the index is already partially staged, `git add -A` folds it in — that's the intent here.
- Only deviate to ask if `git add -A` would obviously sweep in secrets/huge binaries; otherwise trust the instruction.

The survey-and-slice flow below is for "commit my work" / "tidy this into commits" — NOT for an explicit single-commit request.

## Step 0 — Survey before touching anything

```
git status --short
git diff --cached --stat     # what's already staged
git diff --stat              # unstaged modifications
git ls-files --others --exclude-standard   # untracked
git log --oneline -5         # recent message style
git branch --show-current
```

- **Never commit directly on the default branch** (main/master) — branch first unless the user explicitly says otherwise.
- **A non-empty index is a signal, not an accident.** If the user (or a prior session) deliberately staged a curated set, treat it as its own commit candidate. Do not `git add` over it and fuse it with unrelated work.
- **Check for commit-plan artifacts** before inventing structure: `*COMMIT_PLAN*.md`, notes in CLAUDE.md, TODO/plan files referenced in memory. If one exists, **verify its claims against the actual tree** — plans go stale (dirs it says were moved may have been moved back). Follow what still holds; tell the user what didn't.

## Step 1 — Slice into logical commits

Group by **workstream**, not by directory accident. Typical slices: a feature/refactor, an unrelated workstream that happens to share the tree, bug fixes from a debugging session, tooling/skill changes. One commit per coherent story; a reviewer should be able to read each commit standalone.

- Keep **move-only renames separate from rewrites** when the tree allows it cheaply — fused rename+rewrite commits destroy diff readability. Don't reconstruct history archaeologically if the states no longer exist; note the fusion in the message instead.
- Stage each slice by explicit pathspec (`git add <paths>`), never `git add -A` unless the remaining tree genuinely is one slice.
- A file can carry two workstreams (staged base + unstaged fix on top): committing the index first, then staging the file again, splits them correctly.

## Step 2 — Exclude junk, flag it

Untracked files get reviewed, not blanket-added. Common junk that must NOT be committed: stray lockfiles where no manifest exists, scratch logs, build output, editor/agent state dirs, machine-local locks, temp scripts. Leave them untracked and **tell the user what was excluded and why** — they may overrule.

If a plan/scaffolding file says of itself "don't commit me", honor that.

## Step 3 — Messages

- Subject: imperative-ish, ≤ ~72 chars, says what the commit does. Match the repo's existing style (`git log --oneline`).
- Body: the WHY and the non-obvious consequences, derived from the **actual diff** — read it; don't write messages from memory of the session alone. Numbers and root causes beat adjectives ("READPAST error 650, 1359 occurrences" beats "fixed flaky tests").
- **No AI attribution**: never add `Co-Authored-By: Claude`, `Generated with Claude Code`, or similar trailers.
- Multiline messages via a single-quoted here-string (PowerShell) or `-m` with real newlines (bash) — never `\n` literals.

## Step 4 — Approval gate

Default: show the per-commit plan (slices, files, messages) or the staged diff summary and wait for explicit approval before running `git commit`.

Skip the wait only when the user has **already** explicitly told you to commit in this exchange ("just commit it", "commit all of this") — that instruction is the approval. Re-asking after it is noise.

## Step 5 — Commit and verify

- Commit each slice; capture the hash.
- If a pre-commit hook fails: **fix the cause, never `--no-verify`**, never bypass signing.
- After the last commit: `git status --short` must be clean or every remaining entry deliberately excluded and explained; `git log --oneline -<n>` to confirm the shape.
- Report: hash + subject + file count per commit, plus what was left uncommitted and why.

## Anti-patterns

- One mega-commit of an entire mixed tree when slices are obvious and cheap.
- Inventing slices so fine they require surgical `git add -p` archaeology nobody asked for.
- Committing generated noise (lockfile churn from aborted installs, `.last.log` files) inside a code commit.
- Editing the user's staged index without saying so.
- Messages that describe the session ("fixed the thing we discussed") instead of the change.
