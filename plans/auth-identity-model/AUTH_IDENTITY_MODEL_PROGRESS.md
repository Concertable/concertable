# Auth identity model progress

- Plan: `plans/auth-identity-model/AUTH_IDENTITY_MODEL_PLAN.md`
- Roadmap: `plans/auth-identity-model/AUTH_IDENTITY_MODEL_ROADMAP.md`
- Roadmap item: `auth-identity-model/typed-identity-contract`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-AuthIdentityModel`
- Branch: `Refactor/AuthIdentityModel`
- PR: not yet opened (Phase 1 producer)
- Dependency/package gates: `Concertable.Auth.Contracts` must publish and `platform-sync` bump the pins
  before Phase 2 consumers can reference the typed model. `[Obsolete]` is warn-only (no
  `TreatWarningsAsErrors` in the repo), so the post-publish sync PR goes green unaided — Phase 2 is driven,
  not gate-forced.
- Last reconciled: `2026-09-10` against `origin/main` `f1185d6f1`

## Current state

Phase 1 producer, code complete on `Refactor/AuthIdentityModel`, **not yet committed**.

**Done, uncommitted:**
- Nine typed-model files in `api/Concertable.Auth.Contracts/` — `AuthParty`, `InteractiveClient`,
  `InteractiveClientInfo`, `InteractiveClients`, `AuthScope`, `AuthScopes`, `AuthResource`,
  `AuthResourceInfo`, `AuthResources`.
- `[Obsolete]` on `ClientIds.cs` + `ApiScopeIds.cs`.
- `api/Concertable.Auth.Contracts/tests/Concertable.Auth.Contracts.UnitTests/` — new xunit project
  (55 tests, all green). `ProjectReference` `..\..\Concertable.Auth.Contracts.csproj` (inside the package
  folder → resolves in repo and in the `carve-auth` tree alike). Package csproj gains
  `<Compile Remove="tests/**/*.cs" />`; folder gains `Directory.Build.targets` (imports
  `TestConventions.targets`) and test package versions in `Directory.Packages.props`. Registered in
  `api/Concertable.slnx` and the `carve-auth` project list in `.github/workflows/test.yml`.
- Plan + roadmap + ledger under `plans/auth-identity-model/`.

Test-project placement went through two dead ends: (1) inside `Concertable.Auth.Contracts/tests/` with no
glob exclude → the flat package csproj compiled the test `.cs` into the package; (2) under
`api/Concertable.Auth/tests/` → a `ProjectReference` from there to `api/Concertable.Auth.Contracts/` is a
cross-folder edge whose relative path differs between the repo and the carve tree, and `split-inventory`
flags cross-repo test edges. Final: co-located inside the package's own `tests/` with the glob exclude.

## Next Steps

1. Commit Phase 1 (model + obsolete + test project + plan) on `Refactor/AuthIdentityModel`.
2. Push, open the **draft** producer PR (`open-pr`). It stays draft until reviewed + exact-head CI green;
   merging it publishes `Concertable.Auth.Contracts`.
3. Record a review in `## Reviews` before writing "merge" as a next step.
4. Phase 2 starts from a fresh worktree off the post-publish `origin/main` — see the plan's Phase 2 table.
   Retire the stale `Chore-TechDebt-20260909-234925` worktree (superseded PR #981, closed).

## Completed work

- Phase 1 producer code + tests written; `Concertable.Auth.Contracts` and its unit suite build clean
  (0 warnings), 55/55 tests green. Uncommitted pending the first commit.

## Verification

- `dotnet build api/Concertable.Auth.Contracts/Concertable.Auth.Contracts.csproj` — 0 warnings, 0 errors
  (2026-09-10).
- `dotnet test …/Concertable.Auth.Contracts.UnitTests` — 55 passed, 0 failed (2026-09-10).

## Reviews

- None yet.

## Decisions, discoveries, blockers, and deviations

- Superseded `Chore/TechDebt-20260909-234925` / PR #981 (closed): it wired `Config.cs` + `TestTokenMinter`
  onto the old `ApiScopeIds` names, which this plan replaces. Its worktree
  `C:\Users\tommy\source\repos\Concertable\.worktrees\Chore-TechDebt-20260909-234925` is stale — retire it.
- `Concertable.Auth.Contracts` is consumed only as a feed `PackageReference`, never source-swapped
  (`PlatformSourcePackages.targets` does not list it), so even Auth's own `Config.cs` cannot see the new
  types until the package republishes. This is why Phase 1 is additive-only.
- Service clients (`concertable-b2b` etc.) stay out of the published contract — they do not cross the wire
  and their secrets are Auth's. `ServiceClient` enum lands in `Concertable.Auth` in Phase 2.
- The `api/Concertable.Auth/TECH_DEBT.md` "E2E client identity … magic strings" entry is resolved by this
  plan and deleted in Phase 2 (not before — the drift is only actually closed once consumers migrate).
