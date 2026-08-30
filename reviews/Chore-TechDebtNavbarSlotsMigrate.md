# Code review — Chore/TechDebtNavbarSlotsMigrate

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `c6ceda30fd9a429785a7b39c4c5b4bfdbc624ed5`  _(2026-08-30)_

> Range reviewed: `7e2c3498c..c6ceda30f` (1 commit). PR2 of the two-PR publish-first cutover;
> PR1 (`Chore/TechDebtNavbarSlots`, #863) merged and published `@concertable/web@0.1.0-alpha.0.5534`
> carrying the additive `endSlot`/`messagingSlot` API this PR now consumes.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No findings above the confidence bar. Lenses checked:

- **A (correctness)** — none. Traced all five call sites: admin (no search/mailbox, matching its
  prior explicit `showSearch={false} showMailbox={false}`), venue/artist (`messagingSlot={<Mailbox
  />}` into `AppLayout`, which renders the same `NavbarSearch`/`/find`-link markup unconditionally —
  matching what they always got from `Navbar`'s old defaults — plus `Mailbox` gated on the same
  loaded-`user` check as before), customer (untouched, passes no `messagingSlot`, so it gets search
  but never mounts `Mailbox` — the actual bug fix). No stale `showSearch`/`showMailbox` references
  remain anywhere in `app/`; `Mailbox` confirmed exported from `@concertable/web/features/messaging`.
- **B (service isolation)** — N/A, frontend-only.
- **C (module boundaries)** — N/A.
- **D (seeding)** — N/A.
- **E (language/framework conventions)** — `app-tiers`, `tiered-shared-code`, `typescript-style`,
  `routing`, `docs-and-debt` invoked. `tiered-shared-code`'s slot rule: `messagingSlot` is now
  actually injected only by the two apps whose backend serves messaging (venue, artist) — the app,
  not shared code, makes the identity-conditional decision, matching the rule directly. `docs-and-debt`:
  both closed `TECH_DEBT.md` entries are fully deleted (not left as a stub), matching the "delete once
  fixed" rule; `review-lifecycle`: the spent PR1 review file is deleted in this same commit (merged,
  no open findings) rather than left to rot.
- **F (test coverage)** — none. No test files cover `Navbar`/`AppLayout` before or after (this repo
  deliberately has no component-rendering test setup per `react-standards:frontend-testing`'s
  `TESTING.md`); the diff relocates existing JSX with no new branching logic worth a unit test.

All five web builds (`customer`, `venue`, `artist`, `business`, `admin`) pass unchanged;
`@concertable/web`'s 31/31 unit tests pass; `lint:boundaries` clean.
