# Code review — Chore/RemoteValidationWorkflow

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `9663a793151fb31f987b907c80495b67e3dbbdd0`  _(2026-08-13)_

**Security-reviewed up to commit:** `9663a793151fb31f987b907c80495b67e3dbbdd0`  _(2026-08-13)_

> Range reviewed: `3a5df8b18..9663a7931` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — MEDIUM — native** — `.agents/skills/address-review/SKILL.md:30`
  The deletion gate requires exact-head CI to be green before committing the review-file deletion,
  but that closeout commit immediately creates a new unvalidated head. Gate the deletion on the latest
  code checkpoint, then push and verify the review-only closeout head.
