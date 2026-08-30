# Code review — Chore/TechDebtNavbarSlots

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `de9ba8d04`  _(2026-08-30)_

> Range reviewed: `c4451509f..de9ba8d04` (4 commits — supersedes the prior scope; see note below).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

> **Scope note:** the branch was rescoped mid-review after `carve-fe` proved the original single-PR
> design (Navbar/AppLayout decomposition + consumer migration in one PR) cannot pass CI — `carve-fe`
> always restores `@concertable/*` from the published feed's `alpha` tag, never workspace source, so a
> consumer route file can never reference a shared-package prop in the same PR that adds it. The final
> diff is now PR1 of a two-PR publish-first cutover: a purely additive `Navbar`/`AppLayout` API change,
> with the consumer migration and cleanup deferred to PR2. This review judges the final diff only.

## Findings

No findings above the confidence bar. Lenses checked:

- **A (correctness)** — none. `endSlot`/`messagingSlot` are optional and unused by every current
  caller; `onHeightChange` changing from required to optional is a backward-compatible narrowing: every
  existing caller (which all pass it) stays valid. Verified `git diff origin/main..HEAD -- app/web/shared/src/components/Navbar.tsx app/web/shared/src/components/AppLayout.tsx`
  is a pure superset of origin/main's behavior — confirmed by all five web builds passing unchanged
  (`npm -w @concertable/web-{customer,venue,artist,business,admin} run build`) and `@concertable/web`'s
  own 31/31 unit tests passing.
- **B (service isolation)** — N/A, frontend-only, no backend crossing.
- **C (module boundaries)** — N/A.
- **D (seeding)** — N/A.
- **E (language/framework conventions)** — `app-tiers`, `tiered-shared-code`, `typescript-style`,
  `docs-and-debt` invoked. `tiered-shared-code`'s slot rule is satisfied: `endSlot`/`messagingSlot` are
  declared but injected by no caller yet — consistent with "shared code declares a slot, the app injects
  the variation," since nothing here forces a caller to decide anything. `docs-and-debt`: both
  `TECH_DEBT.md` progress notes name the concrete PR2 follow-up and the reason for the split, matching
  the repo's own precedent for a publish-first cutover recorded mid-flight (e.g. the `AsbTopology`
  progress note in `api/Concertable.Messaging/TECH_DEBT.md`).
- **F (test coverage)** — none. Pure additive prop/type surface with no branching logic; per
  `react-standards:frontend-testing`'s `TESTING.md`, this repo deliberately has no component-rendering
  test setup, and a rendering test is explicitly not the fix for UI composition.

`fe-boundaries` (`test:boundaries` + `lint:boundaries`) run clean locally; the three `carve-fe` failures
from the original (superseded) diff are resolved by the rescope — no consumer route file changed in the
final diff, so `carve-fe` never exercises `@concertable/web`'s new (unpublished) shape.
