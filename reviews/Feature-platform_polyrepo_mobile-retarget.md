# Code review — Feature/platform_polyrepo_mobile-retarget

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `ce49f78864823670e938aa7134999793e235f86e`  _(2026-08-07)_

> Range reviewed: `529dba9dd..ce49f788` (6 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C#
conventions, and test coverage of changed paths.

The diff is entirely frontend + CI + docs (mobile bundler config, `App.tsx` CSS import, the
`@concertable/mobile` brand-asset packaging fix, `carve-fe.mjs`, `test.yml`, plan ledger, tech-debt
doc) — no `api/**`, so the backend lenses (microservice isolation / module boundaries / seeding / C#)
are N/A.

- **Lens A (correctness):** the `require.resolve("@concertable/mobile/…")` calls resolve against the
  tier's `exports` (`./package.json`, `./global.css`, `./*`→`dist`), giving the same path in-monorepo
  (symlink) and carved; the tailwind `content` glob is POSIX-normalized for fast-glob; the moved
  `brand/` makes the tier's existing `../../../assets/brand` require path correct and `files` now ships
  it. Empirically validated by CI run 31188407992, which bundled 4325 modules with the retargeted
  config resolving the tier from the feed.
- **Lens F (test coverage):** the effective test for this change is the `carve-fe` mobile `expo export`
  gate, deliberately deferred to a follow-up PR (publish-first — the carve restores the tier from the
  feed, so the asset fix must republish first). Tracked in the plan ledger + `app/mobile/TECH_DEBT.md`.

One accuracy nit found and fixed in-range (not carried as a finding): the `test.yml` carve-fe comments
claimed "each web + mobile surface" while the matrix is web-only on this PR — reworded to note mobile
joins the matrix in the follow-up.
