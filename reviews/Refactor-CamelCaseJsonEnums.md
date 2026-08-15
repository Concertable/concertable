# Code review — Refactor/CamelCaseJsonEnums

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `19a20a1db14771a45a4a3f5fd82e7e7d56795b76`  _(2026-08-15)_

**Security-reviewed up to commit:** `19a20a1db14771a45a4a3f5fd82e7e7d56795b76`  _(2026-08-15)_

> Range reviewed: `520761dd..0f971dd5` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CV1 — LOW — C# conventions** — `api/Concertable.Shared/src/Concertable.Shared.Api/Extensions/ControllerBuilderExtensions.cs:12`
  `AddApplicationJson` adds a legacy `this` extension method despite the C# convention requiring new extension members to use a C# 14 `extension()` block; convert the `IMvcBuilder` extensions in this class to one receiver block.

## Incremental review — 2026-08-15

No issues found. Reviewed `0f971dd5..19a20a1d`; the delta is the clean merge of already-reviewed documentation changes from `origin/main`, with no product-code overlap.
