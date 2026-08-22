# Code review — Feature/navbar-shell-shell

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `3d1fbfb59` _(2026-08-22)_

> Range reviewed: `a48adb54c..3d1fbfb59` (2 files).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No findings. This branch extracts exactly the `Navbar.tsx`/`spinner.tsx` diff already reviewed as
`NAT4`/`BUG1` (and every prior admin-console finding touching these files) in
`Feature/launch_admin-console`'s own review — see its `reviews/Feature-launch_admin-console.md`,
incremental section dated 2026-08-22. Split into its own branch/PR only because `carve-fe (web/admin)`
builds against the *published* `@concertable/web` package rather than local source, so this shared-tier
change has to land and publish before the admin-console PR's `web/admin` carve check resolves the new
`profileSlot`/`showSearch`/`showMailbox`/`Spinner` exports. Verified: `build:web-packages`,
`build:customer`, `build:venue`, `build:artist`, `build:business` all green; `lint:boundaries` clean.
`tiered-shared-code`: `Navbar`'s new props are the slot pattern (a shared component declares the hole,
the owning app fills it), not a role check inside shared code.
