# Auth identity contract shapes progress

- Plan: `plans/auth-identity-model/AUTH_IDENTITY_CONTRACT_SHAPES_PLAN.md`
- Roadmap: `plans/auth-identity-model/AUTH_IDENTITY_MODEL_ROADMAP.md`
- Roadmap item: `auth-identity-model/extension-block-syntax`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-AuthIdentityContractShapes`
- Branch: `Refactor/AuthIdentityContractShapes`
- PR: #1021 (draft) — https://github.com/Concertable/concertable/pull/1021
- Last reconciled: `2026-09-11` against `origin/main` `9e77b42b4` (post PR #1020, Auth resumed
  monorepo-publishing).

## Phase 1 — producer — implemented, not yet reviewed/pushed

`InteractiveClientInfo` → `sealed record class`, `InteractiveClients` deleted and folded in
(`Get`/`GetOrDefault`/`All`), `MobileScheme`/`IsMobile` removed, `All` → `internal`
(`InternalsVisibleTo` added to the csproj). `AuthScopes`/`AuthResources` → C# 14 `extension()` blocks.
Package's own tests rewritten to match (`InteractiveClientsTests.cs` → `InteractiveClientInfoTests.cs`,
`AuthScopesTests.cs`/`AuthResourcesTests.cs` updated to parenless property syntax).

**Verified:** `dotnet build` 0 warnings/errors; `dotnet test` on
`Concertable.Auth.Contracts.UnitTests` — 32/32 pass. `git status` confirms the diff is scoped entirely to
`api/Concertable.Auth.Contracts/` — no consumer file touched.

## Next Steps

1. Review (`review` skill) — this diff touches `.Contracts` paths, matches this repo's
   `.agents/merge-gate.json` `security_paths`, so a security pass is required too.
2. Push, open a draft PR, get exact-head CI green (build/unit tier only — no consumer here to break).
3. Merge. This republishes `Concertable.Auth.Contracts` (breaking removal of `InteractiveClients`, breaking
   rename `Find`→`GetOrDefault`, breaking extension-method→property conversion on `AuthScopes`/
   `AuthResources`).
4. Start Phase 2 (consumer migration) in a fresh worktree off the post-publish `origin/main`, per the
   Plan's Phase 2 section.

## Reviews

None yet.

## Decisions, discoveries, blockers, and deviations

- This work was blocked earlier tonight on a false premise (Auth appeared to have moved to a separate
  `Concertable/auth` repo via PR #1016) — that promotion was reverted (PR #1020,
  `Chore/ResumeAuthPublishing`) before this phase started, confirmed directly against
  `.github/scripts/package_ownership.py` (`RETAINED_TARGETS` includes `"auth"` again) before any code was
  written. See `AUTH_IDENTITY_MODEL_ROADMAP.md`'s item history for the full back-and-forth — not repeated
  here.
