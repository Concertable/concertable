# Code review — Feature/platform_polyrepo_carved-css

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `646efa37771f7784ab4835c4a463c7b2b97eec63`  _(2026-08-06)_

> Range reviewed: `30c74a844..646efa377` (1 commit), against `origin/main`. Local `main` is stale
> (`66ef2c7d3`), so a `merge-base main HEAD` range spuriously includes #402's already-merged
> `Directory.Packages.props` platform-sync — excluded here. Net branch change vs `origin/main`:
> `app/web/shared/src/index.css` (+4) and this plan's ledger.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.

The only code change is two additive Tailwind `@source` globs in `@concertable/web`'s `index.css`
(`../dist/**/*.js`, `../../b2b/dist/**/*.js`). Backend lenses (B–E) are N/A (frontend CSS build config).
Correctness is proven by a locally-reproduced carved-layout `vite build`: the tier canary classes go
absent→present for the customer (`@concertable/web`) and venue (`@concertable/web` + `@concertable/b2b`)
surfaces, and all four in-monorepo web builds stay green (the added globs are inert/dup in-monorepo).
Test coverage (Lens F): the change alters CSS class emission, not a unit-testable runtime branch; the
`carve-fe-web` CI job plus the carved-CSS proof are the structural coverage — nothing concrete to add.
