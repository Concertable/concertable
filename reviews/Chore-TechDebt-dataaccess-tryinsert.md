# Code review — Chore/TechDebt-dataaccess-tryinsert

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `e5877fcd2cbe64ef974b8bf23610a17478775540`  _(2026-08-22)_

> Range reviewed: `5b6ae85ee..e5877fcd2` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No findings. Checked: Lens A (correctness — `AddAsync`/`SaveChangesAsync` composition, `ct` threading,
`catch when` scoped to duplicate-key only), Lens B (service isolation — N/A, no cross-service touch),
Lens C (module boundaries — N/A), Lens D (seeding — N/A), Lens E (`csharp-style`, `csharp-naming`,
`persistence`, `dotnet-standards:dependency-injection`, `unit-testing` — extension-block shape, naming,
Arrange/Act/Assert test shape all conform), Lens F (test coverage — success path covered; the
duplicate-key catch branch is untested, but that is a pre-existing repo-wide gap: no unit test anywhere
constructs a `SqlException` to trigger it, and the EF InMemory provider used by this test tier cannot
produce one — not a gap this diff introduces or can cleanly close without inventing new test
infrastructure out of scope here). Native review (Layer 1, low effort): no findings, independently
confirmed the same points.
