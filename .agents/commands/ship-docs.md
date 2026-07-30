---
description: Land an inert (docs-only) change straight to main with NO pipeline — no PR, no merge queue, no checks. Uses admin bypass. Refuses if any changed file could affect a build.
---
Ship a **docs-only** change directly to `main`, skipping the entire pipeline. Only for changes with zero code effect.

**1. Guard — refuse unless every change is inert.** Compare the current branch to `origin/main`. Every changed file MUST match the inert set (markdown anywhere, or under `docs/ plans/ reviews/ .agents/ .claude/`). If ANY other path appears, STOP and use the normal PR flow instead — this command must never push code straight to `main`.

```bash
git fetch origin --quiet
files=$(git diff --name-only origin/main...HEAD)
echo "Changed vs origin/main:"; printf '%s\n' "$files"
INERT='(\.md$|^docs/|^plans/|^reviews/|^\.agents/|^\.claude/)'
if [ -z "$files" ]; then echo "nothing to ship"; exit 0; fi
if printf '%s\n' "$files" | grep -qvE "$INERT"; then
  echo ">>> NON-INERT files present — NOT shipping directly. Use gh pr create:"; printf '%s\n' "$files" | grep -vE "$INERT"; exit 1
fi
```

**2. Push straight to `main`.** You're a ruleset bypass actor, so this ref update lands with no queue and no required checks:

```bash
git push origin HEAD:main
```

If it's rejected as non-fast-forward, `git rebase origin/main` (the delta is still inert → still safe) and push again. Done — it's on `main`, and nothing gated it.

> The post-push `on: push` CI event still fires but is non-gating and no-ops for inert paths (~seconds). To silence even that, add `paths-ignore` for the inert paths to `test.yml`'s `push:` trigger.
