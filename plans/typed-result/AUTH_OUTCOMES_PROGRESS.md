# Auth expected-outcome migration progress

- Plan: `plans/typed-result/AUTH_OUTCOMES_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_auth-outcomes`
- Branch: `Feature/typed-result_auth-outcomes`
- PR: not opened
- Dependency/package gates: No prerequisite dependency gate. Auth consumes the shipped owned Kernel foundation through `ConcertablePlatformVersion` `0.1.0-alpha.0.847`; no platform-sync PR is open. Auth remains independent of the Payment, B2B, and Customer migrations. After this `api/**` change merges, this work owns its generated package publication/platform-sync gate to terminal green.
- Last reconciled: `2026-08-07` from fresh `origin/main` `83a3f49a194c5faff60b7912d7c9b8452679f6f0`, current branch head `e196f13e19f70285d103c5fe1d5db1066f133fb3`, terminal local verification, incremental review, and PR preflight.

## Current state

The clean branch is zero behind fresh `origin/main` after merge commits
`ecb9351608d7a5b7ac3eb06f2342041cfa7bc492` and
`e196f13e19f70285d103c5fe1d5db1066f133fb3`. The first range added Auth AppHost extension wiring and
platform package `0.847`; the second changed only mobile CI and documentation. Neither changed the
Auth outcome implementation. No PR exists and nothing has been pushed.

Commit `c5e22a05b66c151497d7737d1db992ef7d66d222` converts the four Auth errors to operation-owned Dunet
unions with natural cases, exhaustive `Definition` switches, derived codes, direct case construction,
and explicit per-case contract tests. It also aligns the Shared typed-result architecture guard with
the exhaustive-switch convention. No Auth wire, Razor state, persistence model, migration, or
cross-service runtime contract changed.

Docker is responsive and the Auth integration suite passes 54/54. Auth unit contracts pass 4/4, the
typed-result architecture slice passes 14/14, and the full Release solution build terminates with 0
errors and 5 unrelated existing warnings. Signature, legacy-carrier, local-core, model/migration, and
diff checks pass. The earlier standalone Auth carve remains valid because the later main ranges changed
only AppHost composition, package pinning, mobile CI, and documentation.

`RegisterAsync` now returns `UnitResult<RegisterError>` and the obsolete `RegisterResult` enum is
deleted. Duplicate email becomes `RegisterError.EmailAlreadyExists`; `RegisterModel` maps the owned
definition message back to the unchanged disclosed Razor failure and keeps the existing submitted
success state. `VerifyEmailAsync` now returns `UnitResult<VerifyEmailError>`; missing, expired, and
orphaned tokens all become `VerifyEmailError.InvalidOrExpiredToken`, while valid verification still
mutates the credential once and consumes the token. `SendEmailVerificationAsync` remains
completion-only and preserves its missing-credential no-op.

Direct contract and HTTP coverage now prove success/failure carriers, duplicate registration without
an additional credential or verification email, token mutation/consumption, invalid-token
no-mutation, missing-user email completion, infrastructure exception propagation, and cancellation
propagation. Phase 4 password signatures remain untouched; no EF model, migration, wire contract,
or other service runtime changed.

`ChangePasswordAsync` now returns `UnitResult<ChangePasswordError>` and collapses missing credentials
and incorrect current passwords to `CurrentPasswordIncorrect`; `ChangePasswordModel` maps the owned
safe message and preserves its existing success state. `ResetPasswordAsync` now returns
`UnitResult<ResetPasswordError>` and collapses unknown, expired, and orphaned token rows to
`InvalidOrExpiredToken`; `ResetPasswordModel` preserves its success/failure page behavior.
`SendPasswordResetAsync` remains completion-only, and known/unknown email response parity is unchanged.
Direct and HTTP coverage prove success, owned refusals, cancellation propagation, reset-request
privacy/no-op behavior, invalid-token no-mutation, and one-time token consumption. The final
`IAuthService` surface contains no command-success boolean or nullable login/logout return.

Full review through `2a6fb0069c20491c5f1da6a21ce0aa3bf6e56508` and incremental review of the
three post-watermark branch-owned commits both completed with no findings. The incremental review
also checked the package/solution merge resolutions; later merge `e196f13e1` contains only already-
merged mobile CI/docs changes. The clean review artifact was deleted under the review lifecycle rule.

PR preflight is GREEN: the feature branch is valid, the working tree and code are clean, the branch is
zero behind current main and 24 commits ahead, no Auth PR exists, no platform-sync PR is open, no
published-package cut-over is in flight, and the terminal Release build is green.

## Next Steps

Push the Auth branch using the plan-aware two-leg push protocol, verifying the work head and checkpoint
head against the remote branch. Then open a plain GitHub PR for the verified branch. Require full
merge-queue API and UI E2E: add no skip label or trailer. Record the remote heads, PR number, URL, and
exact next delivery action here; do not run E2E locally.

## Completed work

- Completed the post-review verification, incremental review, and PR preflight on current main; the
  branch is locally green and ready for push/PR delivery.
- Reconciled the completed Auth migration with typed-error convention PR #407: four operation-owned
  Dunet unions now derive their definitions from exhaustive switches, callers construct natural
  cases directly, and exact tests pin every code, message, and kind. Committed as
  `c5e22a05b66c151497d7737d1db992ef7d66d222`.
- Updated the stale Shared typed-result architecture guard so repository enforcement matches the
  merged exhaustive-switch/direct-construction convention.

- Completed Phase 4 in `d4ebf1c9d33a367fb642c013daa542c7d267a6b8`: migrated password-change and password-reset refusals to their
  operation-owned `UnitResult<TError>` contracts, mapped both Razor callers, and completed the
  `IAuthService` carrier cleanup while leaving reset-email requests completion-only.
- Added direct service coverage for successful/refused change/reset operations, cancellation,
  missing-account reset requests, invalid-token non-mutation, and token consumption; the unchanged
  HTTP characterization proves page behavior and account-disclosure parity.
- Completed Phase 3 in this commit: migrated registration and email-verification refusals to their
  operation-owned `UnitResult<TError>` contracts, deleted `RegisterResult`, and mapped both carriers
  back to unchanged Razor behavior.
- Added direct success/refusal and cancellation contracts plus HTTP side-effect/no-mutation coverage;
  kept verification-email sending completion-only with its missing-credential no-op.
- Completed Phase 2 in this commit: migrated login and logout ordinary absence to `Option<T>` at the
  Auth service boundary and mapped the owned carrier back to unchanged Razor, cookie, redirect, and
  Duende protocol behavior.
- Added direct `Some`/`None` service-contract coverage for valid/missing login and logout outcomes;
  the existing HTTP characterization continues to prove every Razor and password-grant adapter edge.
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

- Fresh `docker ps` succeeded; `./scripts/integration.ps1 auth` through `integration-debug`: 54 passed,
  0 failed across the one Auth integration project.
- `dotnet build api/Concertable.slnx --configuration Release --disable-build-servers -m:1 -nr:false
  -p:UseSharedCompilation=false`: succeeded with 0 errors and 5 unrelated existing warnings.
- Current-branch Auth unit definition contracts: 4 passed, 0 failed. Current typed-result architecture
  slice: 14 passed, 0 failed.
- Current signature, legacy-carrier, Razor/Data boundary, local-core, model/migration, and branch/worktree
  `git diff --check` gates passed. Branch is zero behind `origin/main`.
- `pr-preflight`: GREEN; valid feature branch, clean code/tree, 0 behind and 24 ahead, no existing Auth
  PR, no open platform-sync PR, and no package cut-over.
- Post-review convention checkpoint `dotnet test api/Concertable.Auth/tests/Concertable.Auth.UnitTests/Concertable.Auth.UnitTests.csproj --configuration Release`: 4 passed, 0 failed, 0 skipped.
- Targeted `TypedResultArchitectureTests`: 14 passed, 0 failed, 0 skipped.
- `dotnet build api/Concertable.Auth/tests/Concertable.Auth.IntegrationTests/Concertable.Auth.IntegrationTests.csproj --configuration Release --no-restore -m:1 -nr:false -p:UseSharedCompilation=false`: succeeded with 0 warnings and 0 errors.
- Fresh standalone Auth carve restored from the published `0.842` package closure and built with 0 errors; its verified `C:\tmp\auth-carve-*` directory was removed.
- Current searches prove all four errors use Dunet unions and exhaustive definition switches, `IAuthService` retains the intended two `Option<T>`, four `UnitResult<TError>`, and two completion-only signatures, and no model/migration diff exists. `git diff --check` passes.
- Auth integration execution is pending: Docker Desktop processes exist, but `docker ps` timed out twice at the mandatory preflight, so Testcontainers was not started.
- The full Release solution build remains pending after two capped attempts did not return a terminal result under machine-wide build contention; the exact affected closure and standalone carve are green.

- Phase 4 `dotnet test api/Concertable.Auth/tests/Concertable.Auth.UnitTests/Concertable.Auth.UnitTests.csproj --configuration Release`: 4 passed, 0 failed, 0 skipped.
- Phase 4 `./scripts/integration.ps1 auth` through `integration-debug`: 54 passed, 0 failed across the
  Auth integration project, including all direct service and unchanged Razor/Duende contracts.
- Phase 4 `dotnet build api/Concertable.slnx --configuration Release`: succeeded with 0 errors and 9
  existing warnings outside the Auth outcome migration.
- Fresh standalone copy of the complete current `api/Concertable.Auth` tree, excluding build outputs:
  `dotnet build src/Concertable.Auth/Concertable.Auth.csproj --configuration Release` restored from
  Auth's published `0.1.0-alpha.0.827` package closure and succeeded with 0 errors. The verified
  temporary carve under `C:\tmp` was deleted.
- Final Phase 4 searches found all eight intended `IAuthService` signatures, exactly the two
  completion-only email methods, and no `RegisterResult`, nullable login/logout return, password
  command-success boolean, functional carrier in Razor/Data shapes, active local-Core mode, runtime
  project reference, model/migration change, or non-Auth working-tree code path. `git diff --check`
  passed.
- Final origin reconciliation found the branch zero behind
  `48bd0eaf5e8079d07302ec4e07dfdc78167427d2` after a docs/meta-only merge that did not invalidate
  the Phase 4 gate, with no PR. Platform-sync PR #393 is green and open at
  remote head `6064e1f97df7a3fe386c26ea286755a0e28c9a2c`; Auth remains on the stable
  `0.1.0-alpha.0.827` pin until that sync lands.
- No API/UI E2E was run locally because the merge queue owns the required full E2E gate.
- Final reconciled `dotnet test api/Concertable.Auth/tests/Concertable.Auth.UnitTests/Concertable.Auth.UnitTests.csproj --configuration Release --no-restore`: 4 passed, 0 failed, 0 skipped.
- Final reconciled `./scripts/integration.ps1 auth` through `integration-debug`: 44 passed, 0 failed
  across the Auth integration project, including all seven new Phase 3 direct and HTTP contracts.
- Final reconciled `dotnet build api/Concertable.slnx --configuration Release`: succeeded with 0
  errors; 5 existing warnings were outside the Phase 3 Auth changes.
- Fresh standalone copy of the complete current `api/Concertable.Auth` tree, excluding build outputs:
  `dotnet build src/Concertable.Auth/Concertable.Auth.csproj --configuration Release` restored from
  Auth's published `0.1.0-alpha.0.827` package closure and succeeded with 0 errors. The verified
  temporary carve under `C:\tmp` was deleted.
- Phase 3 searches found exactly the two `UnitResult<TError>` signatures and the intentional
  completion-only verification-email signature; no `RegisterResult`, boolean verification signature,
  functional carrier in Razor/Data shapes, committed local-Core enablement, model/migration change,
  or non-Auth working-tree path survives. Final `git diff --check` passed.
- Branch `Feature/typed-result_auth-outcomes` is zero behind `origin/main`
  `3e3bcce89b7cc6c96843e2d80cb634835453a253`; Auth consumes `0.1.0-alpha.0.827`, platform-sync PR
  #388 is merged, no newer sync PR is open, and no Auth PR exists.
- No API/UI E2E was run locally because the merge queue owns the required full E2E gate.
- Final reconciled `dotnet test api/Concertable.Auth/tests/Concertable.Auth.UnitTests/Concertable.Auth.UnitTests.csproj --configuration Release`:
  4 passed, 0 failed, 0 skipped.
- Final reconciled `./scripts/integration.ps1 auth` through `integration-debug`: 37 passed, 0 failed
  across the Auth integration project. The first full run exposed two fixture-only logout tests that
  lacked Duende's required ambient `HttpContext`; after the fixture was corrected, both failed tests
  passed individually and two subsequent full Auth runs passed 37/37.
- Final reconciled `dotnet build api/Concertable.slnx --configuration Release`: succeeded with
  0 errors; a subsequent quiet incremental confirmation reported 0 warnings and 0 errors.
- Final fresh standalone copy of the complete current `api/Concertable.Auth` tree, excluding build
  outputs: `dotnet build src/Concertable.Auth/Concertable.Auth.csproj --configuration Release`
  restored from Auth's published package closure and succeeded with 0 errors. The verified temporary
  carve under `C:\tmp` was deleted.
- Phase 2 searches found exactly the two `Option<T>` `IAuthService` signatures and their three runtime
  adapters; no nullable login/logout signature, returned-principal null check, redirect `??` fallback,
  local-Core setting, model/migration change, or transport/persistence carrier survives.
- Final `git diff --check` passed before staging. Branch `Feature/typed-result_auth-outcomes` is zero
  behind `origin/main` `355f658b9d556dd07e3fa612fb1b04bcdb63a59d` after four clean upstream
  reconciliations; both incoming changes were outside Auth runtime.
- Platform-sync PR #381 (`chore/platform-sync-0.1.0-alpha.0.819`) passed every check and merged as
  `355f658b9d556dd07e3fa612fb1b04bcdb63a59d`; Auth now consumes the published `0.1.0-alpha.0.819` pin.
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

Full code review completed on 2026-08-06 over
`48bd0eaf5e8079d07302ec4e07dfdc78167427d2..2a6fb0069c20491c5f1da6a21ce0aa3bf6e56508`
(17 commits). The stamped artifact `reviews/Feature-typed-result_auth-outcomes.md` recorded no findings
and was deleted under the clean-review lifecycle rule. No finding IDs or dispositions exist. Any later
code commit requires incremental review from watermark
`2a6fb0069c20491c5f1da6a21ce0aa3bf6e56508`.

Incremental code review completed on 2026-08-07 from that watermark over the branch-owned commits
`1bd00115c`, `c5e22a05b`, and `28a586a75`, plus the package/solution merge resolutions, through
`ecb9351608d7a5b7ac3eb06f2342041cfa7bc492`. No findings survived the correctness, isolation,
boundary, seeding, C# convention, or changed-path test lenses. The clean artifact was deleted; no
finding IDs or dispositions exist. Later merge `e196f13e1` contains only already-merged mobile CI/docs.

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
- Direct `AuthService.LogoutAsync` contract tests must supply an ambient request `HttpContext` because
  Duende's `DefaultIdentityServerInteractionService` derives logout data through the current request.
  The fixture scopes and restores that context around generic direct-service calls.
- Login deliberately collapses unknown email, wrong password, and unverified email to `None` and the
  same Razor/Duende response.
- Registration deliberately discloses only `EmailAlreadyExists`; verification deliberately collapses
  missing, expired, and orphaned tokens to `InvalidOrExpiredToken`. Cancellation and infrastructure
  faults remain exceptions.
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

### 2026-08-07 - Verification, incremental review, and PR preflight completed

- Action: Reconciled two fresh main ranges, restored Docker-backed verification, completed the
  terminal Release solution build and mechanical gates, reviewed every post-watermark branch-owned
  change, and ran the read-only PR preflight.
- Evidence: current branch `e196f13e19f70285d103c5fe1d5db1066f133fb3` is zero behind
  `origin/main` `83a3f49a194c5faff60b7912d7c9b8452679f6f0`; Auth integration 54/54; Auth unit 4/4;
  typed-result architecture 14/14; Release solution build 0 errors; carrier/model/diff searches
  green; incremental review through `ecb9351608d7a5b7ac3eb06f2342041cfa7bc492` found no issues;
  preflight found no PR or open platform-sync gate.
- Outcome: Local implementation, verification, and review are terminal and the branch is GREEN to
  publish. Full API/UI E2E remains owned by the merge queue.
- Follow-up: Push with verified plan checkpoints, open the plain GitHub PR without skip labels, then
  continue normal queue delivery.

### 2026-08-07 - Typed-error convention reconciliation implemented

- Action: Merged fresh `origin/main`, reread the current backend/test conventions, audited every Auth
  branch change, migrated the four superseded static error catalogs to operation-owned Dunet unions,
  updated their callers/contracts, and aligned the stale Shared architecture guard with convention
  PR #407.
- Evidence: branch merge `d8cceed2a1874f74103121375d403e1a576c7ec4` is zero behind
  `origin/main` `529dba9dde0776e058a168d4ce137e482194a9ed`; platform-sync PR #393 is
  merged green and no sync PR is open; Auth unit tests 4/4; typed-result architecture tests 14/14;
  affected Auth integration closure build 0 warnings/0 errors; fresh Auth carve 0 errors; carrier,
  model/migration, and diff checks passed; reconciliation commit
  `c5e22a05b66c151497d7737d1db992ef7d66d222`.
- Outcome: The Auth migration now follows the merged natural-case, derived-definition,
  exhaustive-switch convention without changing externally observable Auth behavior. Docker-backed
  integration and the full Release solution build remain non-terminal environment gates.
- Follow-up: Restore responsive Docker, complete the remaining verification, run incremental review,
  then run PR preflight; do not push, open a PR, or run E2E locally.

### 2026-08-06 - Full branch code review completed cleanly

- Action: Ran the repository `code-review` workflow over the complete Auth expected-outcome migration,
  loading the root, backend, Auth, architecture, module, seeding, C#, unit, integration, plan, and
  review lifecycle rules before checking every changed production, test, package, solution, script,
  and plan path through all six review lenses.
- Evidence: reviewed range
  `48bd0eaf5e8079d07302ec4e07dfdc78167427d2..2a6fb0069c20491c5f1da6a21ce0aa3bf6e56508`
  (17 commits, 35 files); stamped `reviews/Feature-typed-result_auth-outcomes.md` at the reviewed head;
  `git diff --check` passed; fresh `origin/main` remains `48bd0eaf5e8079d07302ec4e07dfdc78167427d2`;
  no branch PR exists; platform-sync PR #393 remains open at
  `6064e1f97df7a3fe386c26ea286755a0e28c9a2c` with every check passing.
- Outcome: No findings survived the high-confidence filter across correctness, microservice isolation,
  module boundaries, seeding, C# conventions, or changed-path test coverage. The clean review artifact
  was removed because it had no remaining work-order purpose; no address-review handoff is required.
- Follow-up: Reconcile fresh base/package gates and run the read-only PR preflight; do not push, open a
  PR, or run E2E in that context.

### 2026-08-06 - Post-Phase 4 docs-only main transition reconciled

- Action: Refetched origin after the Phase 4 commit exposed a new 12-commit base advance, verified the
  incoming range, merged it cleanly, and reread the changed plan/checkpoint guidance before handoff.
- Evidence: the incoming range changed only agent/plan documentation and closed Search plan artifacts;
  it contained no Auth, solution, or integration-runner path. Merge commit
  `d5ac70605eac829afac7d71c547520dc950b3fe9` is zero behind
  `origin/main` `48bd0eaf5e8079d07302ec4e07dfdc78167427d2`; the tree was clean before
  this ledger-only checkpoint.
- Outcome: The Phase 4 evidence at `d4ebf1c9d33a367fb642c013daa542c7d267a6b8` remains valid, and
  full branch review will start from the current base and updated plan lifecycle rules.
- Follow-up: Run full code review over `origin/main..HEAD` only; do not push, open a PR, run E2E
  locally, or begin delivery in that context.

### 2026-08-06 - Phase 4 password Result migration and exhaustive cleanup completed

- Action: Migrated change-password and reset-password outcomes to operation-owned
  `UnitResult<TError>` contracts, mapped both Razor callers, retained completion-only password-reset
  email semantics, and expanded direct/HTTP contracts for every Phase 4 outcome and privacy rule.
- Evidence: Auth unit tests 4/4; Auth integration tests 54/54; Release solution build 0 errors; fresh
  standalone Auth carve 0 errors; diff, signature, legacy-carrier, boundary, local-Core,
  project-reference, and model/migration searches passed. The branch is zero behind fresh
  `origin/main` `6586122b82f1cca835db2537656ec96f40e9aaa7`; platform-sync PR #393 is green at
  `6064e1f97df7a3fe386c26ea286755a0e28c9a2c` and Auth remains on stable pin `0.827`.
- Outcome: Commit `d4ebf1c9d33a367fb642c013daa542c7d267a6b8` completes all four local
  implementation phases without changing Razor disclosure or
  success behavior, reset-email privacy/no-op behavior, EF models, migrations, wire contracts, or
  another service's runtime. Every caller-actionable Auth refusal and ordinary absence now uses the
  planned smallest owned in-process carrier.
- Follow-up: Stop at this local checkpoint. Resume with full code review over `origin/main..HEAD` only;
  do not push, open a PR, run E2E locally, or begin delivery in that context.

### 2026-08-06 - Phase 4 resume and upstream gate reconciled

- Action: Fetched origin, verified the requested clean worktree/branch and absent branch PR, inspected
  the open platform sync and its dedicated worktree, and merged fresh `origin/main` before editing.
- Evidence: pre-merge branch was 11 behind and 13 ahead; incoming main had no Auth change since the
  merge base. Merge commit `f122ccb34fff25b7a296e77af8dc5eb26f9905ef` is zero behind
  `origin/main` `6586122b82f1cca835db2537656ec96f40e9aaa7`; Auth remains pinned to
  `0.1.0-alpha.0.827`; no Auth PR exists. Platform-sync PR #393 is red, while its isolated worktree
  contains only the Payment/Money consumer migration and no Auth path.
- Outcome: Phase 4 can proceed on current main without consuming the broken `0.830` pin or overlapping
  the sync worktree. The sync remains a mandatory recheck before the final Phase 4 verification gate.
- Follow-up: Implement and verify Phase 4 only, then commit the green checkpoint and hand off to full
  branch review.

### 2026-08-05 - Phase 3 registration/email-verification Result migration completed

- Action: Migrated registration and email verification to operation-owned `UnitResult<TError>`
  contracts, deleted `RegisterResult`, mapped both Razor callers, expanded direct/HTTP contracts, and
  reconciled the branch through the `0.827` platform sync before the final gate.
- Evidence: Auth unit tests 4/4; final Auth integration tests 44/44; Release solution build 0 errors;
  fresh standalone Auth carve 0 errors; diff, signature, legacy-carrier, boundary, local-Core, and
  model/migration searches passed. Branch is zero behind `origin/main`
  `3e3bcce89b7cc6c96843e2d80cb634835453a253`; platform-sync PR #388 is merged and Auth's `0.827`
  package-pin gate is green.
- Outcome: Phase 3 is complete without changing Razor disclosure/success behavior, email no-op
  semantics, EF models, migrations, wire contracts, or another service's runtime. Expected
  registration and verification refusals are explicit at Auth's in-process service boundary.
- Follow-up: Stop at this local checkpoint. Resume with Phase 4 password change/reset migration and
  exhaustive cleanup only; do not push or begin review in that context.

### 2026-08-05 - Phase 2 login/logout Option migration completed

- Action: Migrated `LoginAsync` and `LogoutAsync` to `Option<T>`, mapped the carrier at both Razor
  pages and the Duende resource-owner validator, added direct contract coverage, and reconciled four times
  as `origin/main` advanced during the phase.
- Evidence: Auth unit tests 4/4; final Auth integration tests 37/37 after both initially failing
  fixture-only logout tests passed individually; final Release solution build 0 errors; fresh
  standalone Auth carve 0 errors; diff, signature, legacy-null/fallback, local-Core, and model/migration
  searches passed. Branch is zero behind `origin/main` `355f658b9d556dd07e3fa612fb1b04bcdb63a59d`;
  platform-sync PR #381 is merged and Auth's `0.819` package-pin gate is green.
- Outcome: Phase 2 is complete without changing Razor/cookie/redirect/OAuth behavior, EF models,
  migrations, wire contracts, or another service's runtime. Ordinary login/logout absence is now
  explicit at Auth's in-process service boundary.
- Follow-up: Stop at this local checkpoint. Resume with Phase 3 registration and email-verification
  `UnitResult<TError>` migration only; do not push or begin Phase 4 in that context.

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
