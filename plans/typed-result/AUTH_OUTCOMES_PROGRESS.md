# Auth expected-outcome migration progress

- Plan: `plans/typed-result/AUTH_OUTCOMES_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_auth-outcomes`
- Branch: `Feature/typed-result_auth-outcomes`
- PR: not opened
- Dependency/package gates: no implementation gate; Auth has no Payment/B2B/Customer dependency.
  Auth uses published `Reunion` `0.1.0-alpha.1` and `Reunion.Errors` `0.1.0-alpha.2`; its Razor edges
  do not need `Reunion.AspNetCore`. After the Auth `api/**` change merges, this work owns publication
  and platform-sync to terminal green.
- Last reconciled: `2026-08-10` through merged `origin/main`
  `01561c093`, including the rendered-handoff matcher fix, and platform pin `0.1.0-alpha.0.910`, plus
  the domain-ownership correction described below. The worktree's plan-handoff hook matches current main.

## Current state

The task directly matches branch `Feature/typed-result_auth-outcomes`. The branch has no PR or remote
branch, no other worktree owns overlapping Auth implementation, and the unrelated untracked review
work order remains untouched. Current main was reconciled before the domain correction gate. The new
runtime checkpoint is locally green and requires incremental review plus fresh PR preflight before
any push or PR.

At the prior green checkpoint, merge commit `1a6c6d670` integrated `origin/main` `1043a9178`, including
platform `0.1.0-alpha.0.890` and the authoritative Reunion conversion plan.

The completed Auth semantics compile directly against published `Reunion` `0.1.0-alpha.1` and
`Reunion.Errors` `0.1.0-alpha.2`. Credential authentication/password decisions and token expiry
decisions now live in the domain entities. Expected refusals remain typed; token/credential identity
mismatch remains an invariant exception. Razor pages still map carriers manually, so
`Reunion.AspNetCore` is intentionally absent. No wire, persistence model, migration, or cross-service
runtime contract changed.

Before that advance, the clean branch was zero behind fresh `origin/main` after merge commits
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

Fresh full code and security review over base `1043a917876cbed48b3c1f873cdcfcc7aadf9b80`
through head `754939891b25577a3047badef751a166e62db8cb` completed with no findings. The current review work order
is stamped at the verified code head for the merge gate. A fresh fetch left `origin/main` unchanged,
the branch zero behind / 29 ahead, with no remote branch, branch PR, or open platform-sync PR. The
read-only PR preflight is GREEN; its only delivery note is that the 29 local commits remain unpushed.

The PR #470 audit required an Auth-local correction. `CredentialEntity` now owns authentication and
password mutation decisions, while the verification/reset token entities own expiry refusal and the
successful credential transition. `AuthService` maps absent database rows and coordinates persistence
without duplicating those domain decisions. Token/credential mismatch throws `DomainException` as an
invariant defect; infrastructure, cancellation, and malformed identity state remain exceptional.

## Next Steps

Run incremental code review over the commits after the existing review watermark, including the
domain-ownership correction. Address every clear finding, refresh current-main state, and run the
read-only PR preflight. Do not push or open a PR without instruction; delivery still requires full
merge-queue API/UI E2E and publication/platform-sync ownership to terminal green.

## Completed work

- Corrected the DDD boundary: domain entities own password verification/mutation and verification/
  reset token expiry decisions; the application service only maps missing persistence state and
  coordinates saving/removal.
- Moved the domain-owned errors and password-hasher port into `Concertable.Auth.Domain`, retained
  invariant exceptions for token/credential mismatch, and added focused domain tests.
- Upgraded Auth to `Reunion.Errors` `0.1.0-alpha.2` and replaced every removed
  `ErrorDefinition.For<TError>()` call with the current direct generic factory API.

- Converted the completed Auth semantic migration from old Kernel functional/error namespaces to
  direct published `Reunion` and `Reunion.Errors` `0.1.0-alpha.1` ownership without adding the unused
  ASP.NET adapter or changing observable behavior.

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

- Domain-correction checkpoint: Auth unit tests 13/13; Auth integration tests 54/54; typed-result
  architecture tests 16/16; full Release solution build 0 errors; fresh standalone Auth carve 0
  errors; `git diff --check` and stale-API/legacy-domain-method searches passed.

- Published-Reunion conversion checkpoint: Auth unit tests 4/4; Auth integration tests 54/54; typed-
  result architecture tests 16/16; full Release solution build 0 errors and 5 unrelated existing
  warnings; fresh standalone Auth carve 0 errors. The temporary carve was removed.
- Conversion searches found the intended two `Option<T>`, four `UnitResult<TError>`, and two
  completion-only `IAuthService` signatures; no old Kernel functional/error namespace, unused
  Reunion ASP.NET package, legacy carrier signature, transport/persistence carrier, local-Core mode,
  model/migration change, or out-of-scope working-tree path. `git diff --check` passed.

- Fresh `docker ps` succeeded; `./scripts/integration.ps1 auth` through `integration-debug`: 54 passed,
  0 failed across the one Auth integration project.
- `dotnet build api/Concertable.slnx --configuration Release --disable-build-servers -m:1 -nr:false
  -p:UseSharedCompilation=false`: succeeded with 0 errors and 5 unrelated existing warnings.
- Current-branch Auth unit definition contracts: 4 passed, 0 failed. Current typed-result architecture
  slice: 14 passed, 0 failed.
- At the verified checkpoint, signature, legacy-carrier, Razor/Data boundary, local-core,
  model/migration, and branch/worktree `git diff --check` gates passed; repeat them after the required
  current-main merge.
- Historical `pr-preflight`: GREEN at the prior current-main checkpoint; repeat after merging fresh
  `origin/main` because the branch is now 101 commits behind.
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

Fresh full code and security review completed on 2026-08-09 over
`1043a917876cbed48b3c1f873cdcfcc7aadf9b80..754939891b25577a3047badef751a166e62db8cb`
(29 commits). No finding survived the correctness, security, isolation, boundary, seeding, C#
convention, or changed-path coverage lenses. The current work order is stamped at the reviewed code
head for the merge gate; no finding IDs or dispositions exist.

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
- Auth has no dependency on the Payment, B2B, or Customer feature branches. Published Reunion opens
  direct service-owned conversion; delivery follows Auth's actual topology.
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
- The PR #470 audit found and now owns the Auth-local domain correction. Shared/background exception
  classification and blanket HTTP handling stay with the roadmap's future global audit after all
  service tracks are terminal.

## Event log

### 2026-08-10 - Rendered-handoff matcher fixed and landed

- Action: Reproduced the repeated false rejection, replaced opaque byte-for-byte Markdown matching
  with ordered semantic validation of the reason, collision guard, worktree, and final pointer, added
  renderer-normalization regressions, and landed docs-only PR #481 before merging it into this worktree.
- Evidence: 43/43 hook tests passed; docs/meta review found no issues; PR #481 merged as
  `01561c093`; Auth is zero behind current `origin/main`; its review work order remains unchanged.
- Outcome: Correct handoffs survive removed backticks/fences, whitespace normalization, full versus
  truncated reason text, and terminal wrapping after a path hyphen, while bare, wrong, or non-final
  pointers still fail closed.
- Follow-up: run incremental review and PR preflight as `## Next Steps`.

### 2026-08-10 - Session-root handoff launcher unblocked

- Action: Preserved only the session root checkout's unrelated in-flight
  `.agents/hooks/plan_handoff_stop.py` edit in named stash
  `root-hook-before-origin-sync-2026-08-10`, then installed the current `origin/main` hook in that
  working tree without touching its other dirty work.
- Evidence: root working-hook and `origin/main` blob hashes both
  `f76c2aca15e6baaa7f15615f955c522196c7f0ca`; direct launcher execution returned `{}`; Auth remains
  zero behind `origin/main` with only its review work order untracked.
- Outcome: The session-root launcher no longer rejects Auth's valid plan handoff, while the displaced
  root hook edit remains recoverable from the named stash.
- Follow-up: run incremental review and PR preflight as `## Next Steps`.

### 2026-08-10 - Ticket migration and platform `0.910` reconciled

- Action: Fetched/pruned origin again, preserved the untracked review work order, and merged the
  Customer Ticket Reunion delivery plus generated platform pin `0.1.0-alpha.0.910` without conflict.
- Evidence: local merge `3574c04c3`; branch zero behind `origin/main` `b17fb07fe`; Auth's checked-out
  `plan_handoff_stop.py` blob equals the `origin/main` blob; the review work order was restored
  unchanged.
- Outcome: The Auth worktree is current and retains the verified domain correction. The session root
  checkout separately has an unrelated uncommitted hook edit, which its launcher will continue to
  reject until that owner reconciles or preserves it.
- Follow-up: run incremental review and PR preflight as `## Next Steps` once the session-root hook gate
  is resolved.

### 2026-08-10 - Current plan-handoff hook reconciled

- Action: Fetched/pruned origin, verified the branch-local Auth work has no PR or remote branch,
  preserved the untracked review work order, and merged the four current-main handoff-hook/prompt
  commits without conflict.
- Evidence: local merge `7b28e8bb9`; branch zero behind `origin/main` `673a41737`; direct diff confirms
  `.agents/hooks/plan_handoff_stop.py` matches `origin/main`; the review work order was restored
  unchanged.
- Outcome: The checkout now has the collision-safe active-owner handoff hook and can rely on the
  ledger's non-terminal continuation pointer again.
- Follow-up: run incremental review and PR preflight as `## Next Steps`.

### 2026-08-10 - Domain ownership and current Reunion API corrected

- Action: Reconciled current main, moved credential authentication/password decisions and token
  expiry transitions into Auth domain entities, kept token/credential mismatch exceptional, moved the
  owned error types and hasher port into the domain namespace, and upgraded `Reunion.Errors` from
  `0.1.0-alpha.1` to `0.1.0-alpha.2`.
- Evidence: no `ErrorDefinition.For<TError>()`, token `IsActive`, or `SetPasswordHash` call remains;
  Auth unit tests 13/13; Auth integration tests 54/54; typed-result architecture tests 16/16; Release
  solution build 0 errors; fresh standalone Auth carve 0 errors; diff check passed; checkpoint commit
  `refactor(auth): move expected outcomes into domain`.
- Outcome: Expected domain alternatives are typed where the decision is made, invariant defects still
  throw, and all error definitions use the current direct factory API with exhaustive Dunet switches.
- Follow-up: run incremental review and PR preflight as `## Next Steps`.

### 2026-08-10 - Explicit delivery gate and current-main drift revalidated

- Action: Resumed the recorded worktree, read the plan lifecycle sources in full, fetched origin, and
  checked branch, worktree, review-artifact, GitHub PR, and platform-sync state without publishing or
  changing runtime code.
- Evidence: local head `56ef28241d10f6c7da4cfab4d616572a79178cf3`; fresh `origin/main`
  `d916e95cfc5fbcc13a581e6d34bc211a4dfa639c`; branch 31 ahead / 123 behind; no Auth PR or remote branch;
  no open platform-sync PR; the sole dirty path is the untracked clean-review work order.
- Outcome: The explicit-delivery blocker remains in force. The old preflight result is no longer
  current because main advanced, so authorization now resumes with current-main reconciliation and a
  complete local gate before push/PR delivery.
- Follow-up: wait for Tommy's explicit Auth delivery authorization in `## Next Steps`.

### 2026-08-10 - PR #470 domain-outcome reconciliation

- Action: Audited Auth's net merge-base-to-HEAD production scope, entity/token methods, all existing
  Option/UnitResult/bool/nullable outcomes, Razor and Duende mappings, exception paths, and test and
  architecture evidence.
- Evidence: Auth contains no production `DomainException`; no service pre-check duplicates a throwing
  domain guard; capability/page booleans do not encode rejection; 4 unit and 54 integration tests plus
  the architecture slice cover the intended owned outcomes and exception propagation.
- Outcome: The plan is classification-clean and delivery-gated; no runtime work or global-audit
  implementation was added.
- Follow-up: wait for the explicit delivery authorization in `## Next Steps`.

### 2026-08-09 - Review and PR preflight completed

- Action: Ran a fresh full code/security review of the net Auth branch, refreshed origin, and executed
  the repository's read-only PR readiness checks.
- Evidence: reviewed `1043a9178..754939891` (29 commits) with no findings; review and security markers
  are stamped at `754939891`; fresh `origin/main` remains `1043a9178`; branch is zero behind / 29
  ahead; code is committed; no remote branch, branch PR, or open platform-sync PR exists.
- Outcome: The branch is GREEN to publish. The review work order is the only uncommitted markdown and
  the merge queue owns full API/UI E2E.
- Follow-up: wait for explicit delivery instruction, then push and open the plain GitHub PR without
  skip labels or trailers.

### 2026-08-09 - Published Reunion conversion completed

- Action: Audited Auth's merged package and Razor topology, migrated its completed semantic contracts
  to direct published Reunion ownership, and ran the complete local verification gate.
- Evidence: runtime references `Reunion` and `Reunion.Errors` `0.1.0-alpha.1`; unit tests 4/4; Auth
  integration 54/54; typed-result architecture 16/16; Release solution build 0 errors with 5 unrelated
  existing warnings; fresh standalone Auth carve 0 errors; package, namespace, signature, boundary,
  model/migration, scope, and diff checks passed.
- Outcome: Auth is locally green on the published Reunion baseline without `Reunion.AspNetCore`,
  behavioral change, transport leakage, model change, or another service runtime dependency.
- Follow-up: commit this checkpoint, run incremental review, reconcile fresh main, and run PR preflight.

### 2026-08-09 - Current main and Reunion baseline reconciled

- Action: Fetched current origin, verified the Auth worktree identity and live delivery gates, and
  merged `origin/main` while reconciling the stale branch ledger to main's authoritative Reunion plan.
- Evidence: clean pre-merge Auth head `98599413a` was 27 ahead / 300 behind fresh main `1043a9178`;
  no Auth PR or remote branch exists, no platform-sync PR is open, and no other worktree owns Auth.
  Main pins Auth to platform `0.1.0-alpha.0.890`; published Reunion is `0.1.0-alpha.1`. The only merge
  conflicts were the plan pair and typed-result architecture guard, resolved to current main's
  Reunion state and broader supported-definition enforcement.
- Outcome: Auth is current with main and independently implementable against published Reunion. The
  completed semantic migration is preserved; direct package ownership and namespace conversion remain.
- Follow-up: execute `## Next Steps`, beginning with the Auth package/source topology conversion.

### 2026-08-09 — direct Reunion conversion dispatched

- Action: Reclassified the Payment/platform-sync wait against Auth's actual service topology.
- Evidence: Auth has no Payment, B2B, or Customer dependency; Reunion `.1` is published.
- Outcome: direct Auth conversion and verification are actionable now.
- Follow-up: execute `## Next Steps`.

### 2026-08-09 - Reunion integration dependency registered

- Action: Reconciled the clean Auth owner with merged Reunion planning PRs #443/#444 and registered
  this ledger in the Reunion owner's downstream handoffs.
- Evidence: local head `98599413a`; fresh `origin/main` `c72b058af`; 218 behind / 27 ahead; no Auth PR
  or remote branch; no open platform-sync PR.
- Outcome: the completed Auth semantics remain authoritative and unchanged. Delivery waits for the
  single Reunion Phase 4 generated platform-sync baseline, then performs one reconciliation and gate run.
- Follow-up: the Reunion owner updates this ledger and surfaces its resume prompt after Phase 4 merges.

### 2026-08-08 - Payment platform gate discharged

- Action: Recorded the Payment owner's completed package and generated consumer-sync delivery.
- Evidence: Payment PR #392 merged as `b66325ac`; platform-sync PR #420 merged as `372be1041` after
  migrating B2B/Customer consumers; publish run `31225852815` produced platform
  `0.1.0-alpha.0.857`, and sync run `31225952562` passed with no recursive follow-on PR.
- Outcome: Auth delivery is no longer blocked by repository platform state. Fresh `origin/main` is
  `372be1041`; this clean branch is 101 behind and 26 ahead.
- Follow-up: execute `## Next Steps` in this worktree, beginning with the current-main merge.

### 2026-08-07 - Auth delivery blocked by Payment platform sync

- Action: Refetched origin and GitHub before the planned push, revalidated worktree/branch ownership,
  and traced the open repository platform gate to Payment owned-result publication.
- Evidence: clean Auth branch `8c6b2c320b4b345812fa98c39fcd3ac5f54c6f15`; no branch PR or remote
  branch; fresh `origin/main` `b66325acdee7979bb3771e4c28248364b769d402`; branch 88 behind and 25
  ahead. Platform-sync PR #420 at `c2679d1ad6e0d245d0aa8b7f830083b22aee2247` is open/red: build run
  `31212334407` reports 24 missing Payment client-interface errors across B2B and Customer.
- Outcome: Push/PR preflight is blocked by a genuine red platform-sync gate. Auth is registered as a
  downstream handoff in the Payment owner ledger and will not merge the mid-cutover mainline or poll.
- Follow-up: The Payment owner must migrate PR #420 to terminal green, update this ledger, and surface
  the Auth resume prompt.

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
