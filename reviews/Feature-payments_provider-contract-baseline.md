# Code review — Feature/payments_provider-contract-baseline

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `7c1253f6946ac195d809b0bb2d9cd91c2fd16266`  _(2026-08-17)_
**Security-reviewed up to commit:** `7c1253f6946ac195d809b0bb2d9cd91c2fd16266`  _(2026-08-17)_

> Range reviewed: `200e49f3..7c1253f6` (4 branch commits; incoming `origin/main` changes excluded except merge resolution).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CV1 — LOW — C# conventions** — `api/agents/CODE_CONVENTIONS.md:310`
  Corrected the new extension-placement rule so related receiver extensions stay together without
  incorrectly forbidding an operation mapper from grouping several related wire receiver types.
