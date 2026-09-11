# Auth identity model progress

- Plan: `plans/auth-identity-model/AUTH_IDENTITY_MODEL_PLAN.md`
- Roadmap: `plans/auth-identity-model/AUTH_IDENTITY_MODEL_ROADMAP.md`
- Roadmap item: `auth-identity-model/typed-identity-contract`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-AuthIdentityModel`
- Branch: `Refactor/AuthIdentityModel`
- PR: #986 (draft) — Phase 1 producer. https://github.com/Concertable/concertable/pull/986
- Dependency/package gates: `Concertable.Auth.Contracts` must publish and `platform-sync` bump the pins
  before Phase 2 consumers can reference the typed model. `[Obsolete]` is warn-only (no
  `TreatWarningsAsErrors` in the repo), so the post-publish sync PR goes green unaided — Phase 2 is driven,
  not gate-forced.
- Last reconciled: `2026-09-10` against `origin/main` `ea985630f` (merged in; platform pin now
  `0.1.0-alpha.0.1364`)

## Current state

Phase 1 producer, code complete on `Refactor/AuthIdentityModel`, **not yet committed**.

**Done, uncommitted:**
- Typed-model files in `api/Concertable.Auth.Contracts/` — `AuthParty`, `InteractiveClient`,
  `InteractiveClientInfo` (scalars only), `InteractiveClients` (`Find` → nullable, `Info`, `All`),
  `AuthScope`, `AuthScopes` (`Id`, `All`), `AuthResource`, `AuthResources` (extension methods
  `Audience()` / `AcceptedScopes()` / `IncludedClaims()` / `All`).
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

1. Review PR #986 (`review` skill) and record the outcome under `## Reviews`. Then mark it ready once
   review + exact-head CI are green. Merging it publishes `Concertable.Auth.Contracts`.
2. After the publish + the generated `platform-sync` PR merges (pin moves past `0.1.0-alpha.0.1364`),
   start Phase 2 from a fresh worktree off that `origin/main`. Close this PR's worktree first
   (`worktrees.ps1 close -PullRequest 986 -PlanManaged`), then create the Phase 2 one and resume this
   ledger. Phase 2 scope + consumption table: `AUTH_IDENTITY_MODEL_PLAN.md`.

Housekeeping (any time): retire the stale `Chore-TechDebt-20260909-234925` worktree — its PR #981 is
closed (`worktrees.ps1 retire`).

## Completed work

- Phase 1 producer code + tests written; `Concertable.Auth.Contracts` and its unit suite build clean
  (0 warnings), 55/55 tests green. Uncommitted pending the first commit.

## Verification

- `dotnet build api/Concertable.Auth.Contracts/Concertable.Auth.Contracts.csproj` — 0 warnings, 0 errors
  (2026-09-10).
- `dotnet test …/Concertable.Auth.Contracts.UnitTests` — 55 passed, 0 failed (2026-09-10).

## Reviews

- `review` skill, PR #986 head `5951a90fb` (2026-09-10) — no correctness bugs; all catalog values traced
  against live `Config.cs` + the four resource-server hosts and confirmed exact. Two findings, both
  addressed on the branch:
  1. `AuthResources.TryGet` (and the sibling `TryGet`s) returned a `default` struct on a miss whose
     `ImmutableArray` members NRE when read — a trap in a published contract. **Fixed:** dropped every
     reverse-lookup that had no consumer; `InteractiveClients.Find` now returns `InteractiveClientInfo?`;
     `AuthResourceInfo` collapsed into `AuthResource` extension methods (`Audience()` / `AcceptedScopes()` /
     `IncludedClaims()`), so no struct carries `ImmutableArray` members.
  2. Redundant `using Concertable.Auth.Contracts;` in the three test files (namespace is nested). **Fixed.**
- `review` re-run at head `3bc6f5b3a` (2026-09-10) — default-struct trap confirmed fixed. Two more, both
  addressed:
  1. `Directory.Build.targets` imported only `TestConventions.targets`, not `PlatformSourcePackages.targets`
     that every sibling carve root carries — a later platform `PackageReference` on the test project would
     miss the source swap. **Fixed:** added the import.
  2. `InteractiveClients.Find` threw `ArgumentNullException` on a null `clientId`, contradicting its "returns
     null rather than throwing" doc — a null `CredentialRegisteredEvent.ClientId` would fault the handler.
     **Fixed:** `Find(string?)` guards null; test covers it.
- Head `<next>` after the fix commit — re-review owed before merge (`incremental-review`).

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
