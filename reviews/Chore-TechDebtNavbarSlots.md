# Code review — Chore/TechDebtNavbarSlots

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `41e4bf4cbb3a21b50c6932c40b8867f3c8697b9a`  _(2026-08-30)_

> Range reviewed: `c4451509f..41e4bf4cb` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — LOW — native** — `app/web/shared/src/components/AppLayout.tsx:16`
  The `messagingSlot` doc comment cited `app/web/TECH_DEBT.md`'s Mailbox entry for rationale, but this
  same commit deletes that entry (now resolved). Fixed: comment states the rationale inline (customer's
  backend has no `MessageController`) instead of pointing at a doc section that no longer exists.

No other findings above the confidence bar. Lenses checked: A (correctness — none), B (service isolation
— N/A, frontend-only), C (module boundaries — N/A), D (seeding — N/A), E (language/framework conventions
— `app-tiers`, `tiered-shared-code`, `typescript-style`, `routing`, `docs-and-debt` all invoked; the
`Navbar`→`endSlot` decomposition and the venue/artist-only `messagingSlot` injection match
`tiered-shared-code`'s slot rule and the identity rule in `app/web/shared/AGENTS.md` — none violated), F
(test coverage — none: the diff is pure JSX composition wiring with no branching worth a rendering test,
and `react-standards:frontend-testing`'s `TESTING.md` states this repo deliberately has no
component-rendering test setup; UI composition is covered by the existing browser/E2E tier, and no
existing test exercises the changed paths).
