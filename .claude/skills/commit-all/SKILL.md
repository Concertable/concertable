---
name: commit-all
description: Commit the ENTIRE working tree in a single commit — no survey, no slicing, no exclusions. Use whenever the user wants everything committed at once: "commit all", "commit everything", "/commit-all", "one commit", "just commit it all", "stage everything and commit". For curated, sliced-by-workstream commits use the `commit` skill instead. Generic — applies to any repo.
---

# commit-all

Stage everything and make one commit. This is the deliberate opposite of the `commit` skill's survey-and-slice flow — the user has opted out of curation. Do NOT survey, do NOT slice, do NOT hold anything back. One commit, whole tree, done.

## The flow

```
git branch --show-current          # must NOT be the default branch
git add -A
git commit -m "<one-line summary of the whole change>"
git log --oneline -1               # confirm it landed
git status --short                 # must be clean afterwards
```

That's the entire skill. No `git status` survey first, no per-file dump, no per-workstream messages — that ceremony is exactly what the user is opting out of by reaching for this instead of `commit`.

## Non-negotiables (these still apply)

- **Never commit on the default branch** (main/master). If you're on it, branch first — `<Type>/<Name>` with a capitalized prefix (`Feature/`, `Refactor/`, `Bug/`, `Fix/`) per the repo convention — then `add -A` + commit.
- **Never `--no-verify`** and never bypass commit signing. If a pre-commit hook fails, fix the cause and retry — don't skip it.
- **No AI-attribution trailer** — never add `Co-Authored-By: Claude`, `Generated with Claude Code`, or similar.
- **Push is out of scope** — this skill only commits. Use `commit-push` or `push` to push.

## The one judgment call

`git add -A` is all-in by design. Only pause to flag — in one line, don't silently drop — if it would obviously sweep in something that must never be in history:
- secrets / credentials / a `.env` with live values,
- large build output or binaries,
- machine-local agent/editor state (e.g. tracked `.claude/worktrees/` gitlink churn).

If you spot one, say so and let the user decide. Otherwise trust the instruction and commit everything. When unsure, commit — they asked for all.

## Message

One imperative line summarizing the overall change, matched to the repo's `git log --oneline` style. If the tree genuinely spans unrelated things, a short summary line plus a 2–4 bullet body listing them is fine — but it's still ONE commit. Derive it from the actual diff, not from memory of the session.

## Report

Hash + subject + file count, and confirm `git status` is clean — or name the single thing you flagged and why it was held back.
