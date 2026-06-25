# Code review — Feature/ServiceBuildSeparationPhase2

**Reviewed up to commit:** `84a09c489a1d703d9f4b5d4618a9e1583dd102ff`  _(2026-06-25)_

> Range reviewed: `bf267d08..84a09c48` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, and C# conventions.

The diff is purely packaging metadata — `IsPackable=true` + a one-line `<Description>` on 24
shared-platform csproj, MinVer + package metadata added to four folders' own
`Directory.Build.props` / `Directory.Packages.props`, an expanded `verify-restore` job, and the
plan markdown. **No C# code, no `ProjectReference` changes, no seeders/modules/migrations**, so
Lenses C (module boundaries), D (seeding), and E (C# conventions) don't apply, and Lens B
(microservice isolation) has no surface (no cross-service refs added or removed).

Lens A / B verification performed:
- **No new cross-service or escaping `ProjectReference`** — the commit adds/removes none; it only
  flips `IsPackable` and adds metadata. Microservice isolation is unaffected.
- **BUILD1 closure proven** — `dotnet pack api/Concertable.slnx` emits exactly the 26 intended
  packages at lockstep `0.1.0-alpha.0.527`; auditing every `.nuspec` found no package declaring a
  feed-absent `Concertable.*` dependency (the two non-published `Shared/` libs, `Shared.Api` and
  `Seed.Infrastructure`, are referenced by nobody in the published set).
- **Gate** — `dotnet build api/Concertable.slnx` green (0 errors); shared-platform unit tests green
  (Kernel 14, Messaging 40, Messaging.AzureServiceBus 8).
- **`verify-restore` package list** cross-checked against the 26 packed nupkgs — exact match, no typo,
  none missing/extra.

## Observations (advisory, not blocking)

- [ ] **OBS1 — low — CI maintainability** — `.github/workflows/publish-packages.yml`
  The strengthened `verify-restore` lists all 26 package IDs explicitly, so each future phase that
  opts a new package in must extend this list by hand (a deliberate trade-off — it's a *gate*, so
  drift only weakens coverage, never mis-publishes; publishing itself stays driven solely by
  `IsPackable`). A comment in the step already says to extend it. Phase 2b's Auth carve-check will be
  the stronger, self-covering gate for Auth's closure.
- [ ] **OBS2 — low — package polish** — pack emits "missing a readme" best-practice warnings for the
  new packages. Pre-existing and consistent (Phase-1 `Kernel`/`Contracts` have none either); not a
  regression. Add READMEs later if these packages are ever surfaced externally.
