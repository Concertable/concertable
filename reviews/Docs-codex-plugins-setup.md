# Code review — Docs/codex-plugins-setup

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `e1488c5cabba137e64850ff6a20b5ffd4ce4d1e9`  _(2026-08-21)_

> Range reviewed: `8bfc169e..e1488c5c` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — HIGH — error handling** — `scripts/setup-codex-plugins.ps1:43,53` (original)
  `$ErrorActionPreference = 'Stop'` does not catch a failed `codex plugin marketplace add` /
  `codex plugin add` external-command call — neither PowerShell 5.1 nor pwsh's default
  `$PSNativeCommandUseErrorActionPreference` treats a non-zero native exit code as terminating — so a
  network blip, expired auth, or rate limit would silently continue and the script would still print
  "Codex plugin setup complete." **Fixed:** check `$LASTEXITCODE -ne 0` after each `codex` call and
  `exit 1`, matching `scripts/docker-health.ps1`'s existing convention (`docker ps`/`docker pull`/
  `docker run` are all guarded the same way). Re-tested end to end after the fix — the idempotent
  skip path (everything already installed) still runs clean.

No other issues. AGENTS.md's new paragraph accurately describes the real constraint (verified against
the actual `openai/codex#18115` issue text, not blog claims — Codex's plugin config is genuinely
user-scoped, no repo-committable equivalent exists today), correctly distinguishes it from Claude
Code's already-solved case, and points at a script that now exists and runs correctly (manually
verified on this machine: fresh-install path and already-installed skip path both produce correct,
idempotent output). No contradiction with sibling docs, no dangling reference, no restated rule.
