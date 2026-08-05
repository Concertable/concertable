# Auth expected-outcome migration progress

- Plan: `plans/typed-result/AUTH_OUTCOMES_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_auth-outcomes`
- Branch: `Feature/typed-result_auth-outcomes`
- PR: not opened
- Dependency/package gates: No prerequisite dependency gate. The owned Kernel Result/Option foundation is shipped and available through Auth's current `ConcertablePlatformVersion` (`0.1.0-alpha.0.814`). Auth is independent of the Payment, B2B, and Customer migrations. After this `api/**` change merges, this work owns its generated package publication/platform-sync gate to terminal green.
- Last reconciled: `2026-08-05T19:22:32+01:00` from the Phase 1 implementation, Auth test runs, Release solution build, standalone Auth carve, diff/signature/model searches, and branch/package state.

## Current state

Phase 1 is complete, green, and checkpointed by this commit. The branch is current with fresh
`origin/main` `0ed29d8f077fc9593467d6c858c6a0cbab688290`, uses Auth platform pin
`0.1.0-alpha.0.814`, has no PR, and has no prerequisite package or platform-sync gate.

Auth now has its direct published Kernel package dependency, operation-owned error vocabulary, unit
test project, integration fixture/project, integration runner registration, and HTTP characterization
coverage for every current Auth flow. No `IAuthService` signature, runtime caller, EF model, or
migration changed in Phase 1.

The complete `IAuthService` and caller surface has been audited. The target is two `Option<T>`
operations (`LoginAsync`, `LogoutAsync`), four operation-owned `UnitResult<TError>` refusals
(register, change password, verify email, reset password), and two intentional completion-only email
operations. The published Kernel API is sufficient; no shared-foundation prerequisite was found.

## Next Steps

Implement Phase 2 only from `plans/typed-result/AUTH_OUTCOMES_PLAN.md` in this worktree. First fetch
and reconcile the clean branch, upstream, PR, platform-sync status, and Auth package pin; update from
fresh `origin/main` before editing if behind. Change `LoginAsync` to `Task<Option<ClaimsPrincipal>>`
and `LogoutAsync` to `Task<Option<string>>`, convert nullable persistence/framework results only at
the Auth service boundary, and map `Some`/`None` at `LoginModel`, `LogoutModel`, and
`ResourceOwnerPasswordValidator` without changing Razor, cookie, redirect, or Duende protocol
behavior. Keep unknown email, wrong password, and unverified email identical at both login edges.
Extend the focused unit/characterization coverage for every `Some`/`None` mapping, run the Auth unit
and integration projects through `integration-debug`, the Release solution build, a fresh standalone
Auth carve, `git diff --check`, and Phase 2 signature/legacy-carrier searches. Update the plan and
this ledger with exact evidence, commit the green Phase 2 checkpoint locally, and stop with Phase 3
as the next handoff. Do not push, open a PR, run E2E locally, or begin Phase 3 in the same context.

## Completed work

- Completed Phase 1 in this commit: added Auth's direct published `Concertable.Kernel` dependency,
  four operation-owned error definitions, and exact definition contract tests without changing any
  existing `IAuthService` signature or caller.
- Added Auth-owned unit, integration-fixture, and integration test projects to `api/Concertable.slnx`;
  registered the Auth integration suite in `scripts/integration.ps1`.
- Added real HTTP characterization for Razor login/logout/registration/verification/password flows,
  the Duende resource-owner password grant, known/unknown privacy parity, token consumption and
  no-mutation cases, representative email infrastructure failures, and cancellation propagation.
- Brought the branch onto fresh `origin/main` and platform pin `0.1.0-alpha.0.814` before completing
  Phase 1.
- Created the requested worktree and branch from freshly fetched `origin/main`; after main advanced
  during the audit, refreshed the still-clean branch and finally rebased the unpushed planning
  checkpoint onto `e419966a97dda7b2bc0fd9354a002a9b0dad0d57`.
- Passed the branch-time no-red-platform-sync gate: PR #367 was non-red at creation and PR #372 was
  non-red after the pre-edit refresh; PR #373 was non-red at the final reconciliation.
- Audited all required planning, architecture, convention, Kernel implementation/test, Auth service,
  Razor caller, Duende caller, persistence, and repo-wide test coverage sources.
- Designed the exhaustive caller-driven contracts, coverage additions, four green implementation
  phases, and terminal review/delivery lifecycle in the companion plan.
- Added the plan and this initialized ledger in this commit. No migration code was implemented.

## Verification

- `dotnet test api/Concertable.Auth/tests/Concertable.Auth.UnitTests/Concertable.Auth.UnitTests.csproj --configuration Release`:
  4 passed, 0 failed, 0 skipped, with no warnings.
- `./scripts/integration.ps1 auth` through `integration-debug`: 31 passed, 0 failed across the Auth
  integration project; the valid Duende logout-context test also passed alone before the full rerun.
- `dotnet build api/Concertable.slnx --configuration Release`: succeeded with 0 errors; 9 existing
  warnings were outside the Phase 1 Auth changes.
- Fresh standalone copy of the complete current `api/Concertable.Auth` tree, excluding build outputs:
  `dotnet build src/Concertable.Auth/Concertable.Auth.csproj --configuration Release` restored from
  Auth's package closure and succeeded with 0 errors. The verified temporary carve was deleted.
- Phase searches: all eight legacy `IAuthService` signatures and callers remain unchanged; no runtime
  `Option`/`UnitResult` carrier was introduced early; no `UseLocalCore=true` setting exists; no Auth
  migration/model path changed; the direct Kernel package and all three test projects are registered.
- `git diff --check` passed before staging; the staged form is rechecked immediately before commit.
- Branch/worktree gate: `Feature/typed-result_auth-outcomes`, clean before editing, with its sole local
  planning checkpoint based on `origin/main` `e419966a97dda7b2bc0fd9354a002a9b0dad0d57` and zero behind.
- Platform-sync gate: `gh pr checks` reported no failed checks for open sync PR #373 after the final
  fetch.
- Surface inventory: repo-wide `rg` confirmed `IAuthService` has exactly seven Razor callers plus
  `ResourceOwnerPasswordValidator`, and no downstream runtime consumer.
- Coverage inventory: no `api/Concertable.Auth/tests/` project exists. Existing API E2E uses successful
  `/connect/token` minting; existing passing UI E2E covers successful Customer/B2B login and Customer/
  Venue/Artist registration -> fake-email verification -> login only.
- Planning checkpoint checks passed: all eight `IAuthService` methods are classified; the plan has zero roadmap
  references; the ledger pointer is near the plan top; every template/current-state section is
  populated; only the requested two files are changed; and neither file has trailing whitespace.
- No build or test run was required for the earlier plan-only checkpoint.

## Reviews

No formal code review has run because the plan schedules full branch review after Phase 4. Phase 1
received a local implementation/test-foundation audit before this checkpoint; the final lifecycle must
still run full code review over the complete branch and incremental review after later code commits.

## Decisions, discoveries, blockers, and deviations

- Origin: the permanent typed-result epic item is **Auth expected-outcome migration** in
  `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`. Tick that item only after the entire feature
  lifecycle, including merge and platform sync, ships; never delete the permanent tracker.
- No dependency blocker exists. Auth must not wait for or consume work from the Payment, B2B, or
  Customer migration branches.
- Auth's top-level program reads required E2E host settings before `WithWebHostBuilder` app
  configuration is visible. The fixture therefore scopes process environment overrides to host
  startup and restores every prior value immediately after `factory.Services` materializes.
- Direct EF Core and Relational references in the unit/fixture projects are required to converge the
  published platform closure's EF 10.0.3 assemblies with Auth's EF 10.0.7 build; removing them produced
  MSBuild assembly-conflict warnings.
- Valid redirect logout characterization creates a real Duende `LogoutMessage` through its
  `IMessageStore`; an unauthenticated `/connect/endsession` request produces no usable logout context.
- Kernel's shipped factories and observers are sufficient: `Option<T>`, `UnitResult<TError>`,
  `TryGetValue`, `TryGetError`, `Match`, `ValueOr`, `ToOption`, and explicit success/failure factories.
- Login deliberately collapses unknown email, wrong password, and unverified email to `None` and the
  same Razor/Duende response.
- Password-reset email remains completion-only so known and unknown accounts have the same observable
  page outcome. Verification-email missing-user behavior also remains a silent completion because no
  caller has a safe actionable branch.
- Duplicate registration remains intentionally disclosed exactly as it is today.
- Existing E2E provides successful cross-surface compatibility coverage but leaves all failure,
  logout, password, and disclosure branches uncovered. Auth-owned unit/integration tests close those
  gaps; this branch does not modify another service's runtime or make its test tree Auth's owner.
- Full API and UI E2E is required in the merge queue. It must not be duplicated locally before the PR.
- No model change or migration is planned. A discovered model or shared-Kernel need requires a plan
  amendment/separate additive shared item before implementation proceeds.

## Event log

### 2026-08-05 - Phase 1 Auth test foundation completed

- Action: Added Auth's direct Kernel package dependency, four owned error definitions, exact unit
  contracts, Auth integration infrastructure, and HTTP characterization across every existing Auth
  flow; registered all projects in the solution and integration runner.
- Evidence: Auth unit tests 4/4; Auth integration tests 31/31; Release solution build 0 errors;
  fresh standalone Auth carve build 0 errors; diff, signature, runtime-carrier, package, local-core,
  and migration/model searches passed. No API/UI E2E was run locally because the merge queue owns it.
- Outcome: Phase 1 is complete and green without changing `IAuthService`, runtime callers, models, or
  migrations. The characterization suite now locks the current Razor/Duende behavior for migration.
- Follow-up: Stop at this local checkpoint. Resume with Phase 2 login/logout `Option<T>` migration
  only; do not push or begin Phase 3 in that context.

### 2026-08-05 - Phase 1 partial state and upstream transition reconciled

- Action: Resumed the plan in its recorded worktree, fetched `origin`, inspected the branch, worktree,
  dirty paths, package pins, PR inventory, and open platform-sync inventory before editing.
- Evidence: branch `Feature/typed-result_auth-outcomes` at `3eb2c2d12b39b49535f91c9a67dfd481dc5f5929`;
  fresh `origin/main` `0ed29d8f077fc9593467d6c858c6a0cbab688290`; ahead `1`, behind `7`; no branch PR;
  no open platform-sync PR; `origin/main` Auth pin `0.1.0-alpha.0.814`; dirty paths are confined to
  the Phase 1 Auth test foundation, package/solution registration, and integration runner.
- Outcome: The branch/worktree identity is valid and the partial Phase 1 work is in scope, but it must
  be preserved while the branch is updated before it is built or completed. The work was stashed,
  `origin/main` merged as `185876e9219d8854dae8e09ceef791b351361855`, and the stash restored without
  conflict; the branch is now zero behind at platform pin `0.1.0-alpha.0.814`.
- Follow-up: Audit the restored implementation against Phase 1, complete the missing coverage, and
  drive the full phase gate green.

### 2026-08-05 - Worktree and dependency gate established

- Action: Fetched `origin`, checked open platform-sync PRs, and created the sibling worktree on
  `Feature/typed-result_auth-outcomes` from `origin/main`; refreshed again before the first edit when
  main advanced.
- Evidence: initial base `03aa7e35dadd1c4b03b5a7a06577f49092f8af73`; pre-edit refreshed base
  `f04025c5ef4d7fe68d7ecef6cea4786470e138a1`; final rebased base
  `e419966a97dda7b2bc0fd9354a002a9b0dad0d57`; `git rev-list --count HEAD..origin/main` returned `0`;
  platform-sync PRs #367, #372, and #373 had no failed checks at their respective gates.
- Outcome: The requested branch is current, isolated, and not blocked by a red platform sync.
- Follow-up: Implement only after this planning checkpoint is committed.

### 2026-08-05 - Caller and coverage planning completed

- Action: Read the mandated sources in full and audited Auth service methods, every Razor/Duende
  caller, persistence behavior, test projects, API token minting, and passing UI E2E login/sign-up
  scenarios.
- Evidence: `plans/typed-result/AUTH_OUTCOMES_PLAN.md` and this ledger in this commit; repo-wide file,
  symbol, route, feature, project, and package-reference inventories; planning checks classified all
  eight service methods, found zero roadmap references in the plan, verified every ledger section and
  the near-top pointer, limited the working tree to the two requested files, and found no trailing
  whitespace.
- Outcome: The migration has an exhaustive target contract table, explicit privacy/boundary rules,
  a real coverage-gap inventory, four independently green phases, and a full delivery lifecycle.
- Follow-up: Execute Phase 1 exactly as described in `## Next Steps`.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_auth-outcomes
Read @plans/typed-result/AUTH_OUTCOMES_PLAN.md and @plans/typed-result/AUTH_OUTCOMES_PROGRESS.md and do what its `## Next Steps` says.
```
