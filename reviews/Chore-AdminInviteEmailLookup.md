# Code review — Chore/AdminInviteEmailLookup

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `c4e0abfd3e951f1251da0adfc715b51f0e9dedac`  _(2026-08-22)_

> Range reviewed: `26645ecd1..925938841` (7 commits), re-stamped after merging current `main` (platform-sync
> to 0.1.0-alpha.0.1149 + unrelated Composition→Architecture test-tier rename) — no new findings, both
> touched suites (Admin.UnitTests 33/33, DataAccess.UnitTests 12/12) still green post-merge.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — LOW — native** — `api/Concertable.B2B/src/Modules/User/Concertable.B2B.User.Infrastructure/Repositories/UserRepository.cs:21`
  `GetIdByEmailAsync` used an exact DB-level `==` match, silently relying on DB collation instead of the
  code-guaranteed case-insensitive compare `InviteAsync`'s old in-memory `OrdinalIgnoreCase` gave. Fixed:
  query now does `u.Email.ToLower() == email.ToLower()`, and pinned with an integration test asserting a
  differently-cased lookup still resolves against the real SQL Server container.
- [x] **NAT2 — LOW — native** — `api/Concertable.DataAccess/Tests/Concertable.DataAccess.UnitTests/Concertable.DataAccess.UnitTests.csproj:13`
  Direct `Microsoft.EntityFrameworkCore.InMemory` `PackageReference` was redundant once the project also
  referenced `Concertable.Testing.Unit` (which already carries that package non-privately). Removed; build
  and tests still green via the transitive reference.
- [x] **CV1 — LOW — Lens F (test coverage)** — `api/Concertable.B2B/src/Modules/User/Concertable.B2B.User.Infrastructure/Services/UserService.cs`
  The new `GetIdByEmailAsync` chain (`UserModule` → `UserService` → `UserRepository`) had no test exercising
  the real wiring — `AdminServiceTests` only mocks `IUserModule` at the facade boundary. Added
  `GetIdByEmailAsync_ReturnsId_WhenEmailDiffersOnlyByCase` / `_ReturnsNone_WhenNoUserHasThatEmail` to
  `Concertable.B2B.User.IntegrationTests/UserApiTests.cs`, resolving `IUserModule` from the fixture's DI
  scope per the file's existing precedent.

No other findings survived Step 4's confidence bar. Lenses checked: A (correctness), B (service isolation —
n/a, no cross-service change), C (module boundaries — `IUserModule.GetIdByEmailAsync` forwards directly to
`IUserService`, no repository/mapping duplicated in the facade), D (seeding — n/a, no seeder touched), E
(language/framework conventions — csharp-style, csharp-naming, persistence, multitenancy,
dependency-injection, module-structure, result-carriers, unit-testing, integration-testing, packages, all
invoked and checked), F (test coverage — see CV1 above; the DataAccess `RepositoryTests` rewrite and the
new `Concertable.Testing.Unit` extension are themselves covered by the existing/rewritten
`RepositoryTests.cs` suite, 12/12 passing).
