# Auth expected-outcome migration progress

- Plan: `plans/typed-result/AUTH_OUTCOMES_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Roadmap item: `typed-result/auth-outcomes`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Docs\typed-result_auth-outcomes_closeout`
- Branch: `Docs/typed-result_auth-outcomes_closeout`
- Source PR: [#517](https://github.com/Concertable/concertable/pull/517), merged as
  `bf723db98bd1acb573d55e3cbe50692efea0e2cf`
- Dependency/package gates: no implementation gate. NuGet.org publishes `Reunion` and
  `Reunion.AspNetCore`, `Reunion.Errors`, and `Reunion.Validation` `0.1.0-alpha.3`; Auth has
  no Payment, B2B, or Customer runtime/package dependency. This branch owns Auth's semantic migration
  and direct `Reunion`/`Reunion.Errors` alpha.3 adoption. The separate repository baseline plan owns
  repository-wide version alignment. After the Auth `api/**` PR merges, this plan
  owns publication and its generated platform-sync gate to terminal green.
- Last reconciled: `2026-08-12` after PR #517 merged through full-E2E run 31641085535. The pinned
  Stripe installer passed in both API and UI jobs; both suites passed. Publication run 31644761975
  succeeded and restored all 40 published packages at platform `0.1.0-alpha.0.958`. Generated
  platform-sync PR #531 passed its hard-floor queue run and merged as
  `9d1dd7acd17badb54a93ef9b269802e1590ceb21`. Every lifecycle gate is terminal.

## Current state

Auth implementation and the durable Stripe bootstrap correction are merged. Publication and
platform sync are terminal, and the former source worktree and feature branches are removed. This
closeout worktree now owns only the terminal plan/ledger deletion and roadmap update.

Phases 1-5 are committed and locally verified. Auth's in-process contracts use direct published
Reunion ownership: login/logout return `Option<T>`, four caller-actionable refusal paths return
operation-owned `UnitResult<TError>`, and privacy-sensitive email operations remain completion-only.
Credential authentication/password decisions and token expiry transitions live in Auth domain
entities. Token/credential identity mismatch remains an invariant exception. Razor and Duende map
the carriers without leaking them into wire or persistence shapes.

Phase 6 and the final producer reconciliation are complete. The current candidate aligns `Reunion`
and `Reunion.Errors` to `0.1.0-alpha.3`, uses unambiguous target-typed value/error conversions, exact
unit `Success` cases, target-typed `null` for `None`, and direct forwarding where domain and service
result types already match. No factory, wrapper, cast, or result reconstruction should change.
Reunion source commit `91fdc6f2e33d8f396fa463ad309cb1288bea3be5` adds flexible Option HTTP
terminals, but Auth owns no Minimal API or MVC outcome surface: its account handlers are
server-rendered Razor Pages and its other outcome edge is Duende. `Reunion.AspNetCore` therefore
remains correctly absent.

PR #524's additional carrier guidance applies to Auth's boundary choices but requires no source
change: EF lookups remain nullable inside `AuthService`, while `LoginAsync` and `LogoutAsync` expose
intentional present-or-absent application outcomes as `Option<T>`. Auth does not wrap and immediately
unwrap technical nullability, leak Option into persistence, or use a Result where absence has no
distinct safe explanation.

## Next Steps

1. Delete this terminal plan and ledger together and tick the roadmap item.
2. Run `docs-review` and land the closeout through `merge-docs`.

## Downstream handoffs

- Waiting ledger: `plans/typed-result/REUNION_SHARED_CONTRACTION_PROGRESS.md` now records the Auth
  gate satisfied. Auth has zero old Kernel functional/error carriers, Shared.Api terminals,
  third-party functional carriers, or legacy Reunion factories left in its owned scope; B2B
  preparation is the remaining blocker for that plan.

## Completed milestones

- Phase 1 (`d38312937`): added Auth's direct package closure, owned error vocabulary, unit and
  integration projects, HTTP characterization, solution registration, and integration-runner entry.
- Phase 2 (`efc15b72b`): migrated login/logout ordinary absence to `Option<T>` and preserved Razor,
  cookie, redirect, and Duende behavior.
- Phase 3 (`0e434f48d`): migrated registration and email-verification refusals to owned
  `UnitResult<TError>` contracts while preserving registration disclosure and verification behavior.
- Phase 4 (`d4ebf1c9d`): migrated password-change/reset refusals, preserved reset-request privacy, and
  removed legacy command-success booleans and nullable login/logout contracts.
- Typed-error reconciliation (`c5e22a05b`): moved the four errors to operation-owned Dunet unions with
  exhaustive definitions and aligned the Shared typed-result architecture guard.
- Published Reunion conversion (`754939891`): replaced old Kernel carrier/error namespaces with
  direct `Reunion` and `Reunion.Errors` ownership without adding the ASP.NET adapter.
- Phase 5 domain correction (`af37d2618`): moved password and token decisions into Auth domain
  entities, retained invariant exceptions, and adopted the direct alpha.2 `ErrorDefinition` API.
- Current-main reconciliation (`6c6c54484`): merged the Result-pattern documentation and platform
  `0.943` without conflict; the preserved Auth outcome work remains the only branch-owned runtime
  scope.
- Phase 6 (`1afdb4b33`): aligned Auth to Reunion alpha.2 construction, including target-typed `null`
  for Option absence, and completed the final verification gate.
- Alpha.3 producer reconciliation (`12c000d7`): aligned Auth's direct `Reunion` and
  `Reunion.Errors` package closure to published `0.1.0-alpha.3` and audited Reunion source commit
  `91fdc6f2e33d8f396fa463ad309cb1288bea3be5`. Its new flexible Option HTTP terminals do not apply to
  Auth's Razor/Duende topology, so no runtime call site or AspNetCore package was added.
- Current-main merge (`38e62584`): merged `origin/main` through `6a3d66677` without conflict. The
  inherited change is Customer-only; Auth package pins and runtime source are unchanged.
- PR readiness: clean incremental correctness/security review and GREEN read-only preflight on
  current `origin/main`; no code, package-cutover, PR, or platform-sync blocker remains.
- Delivery push: local and remote work heads verified equal at `c784db2044cf11521681e842b28a38f92946385c`;
  the pushed range is the complete 53-commit branch delta over current `origin/main`.
- Push checkpoint transport: local and remote heads verified equal at
  `4b53ac5bbbe0a08af9254d7a51d80f164f68387e`. PR creation was rejected before GitHub created a PR.
- PR discovery: PR #517 exists at that exact head. Its original hard-floor checks are green; current
  `origin/main` is 28 commits ahead, so the PR is not yet eligible for queue admission.
- Current-main update (`962969cad`): merged `origin/main` through `8ec037d7d`; Auth source remained
  unchanged, its platform pin advanced to `0.950`, and the Shared-contraction ledger now retains both
  the Auth and Customer preparation handoffs.
- Platform `0.953` update (`fd69b70f0`): merged platform-sync PR #525 without source conflict and
  advanced Auth's published platform closure from `0.950` to `0.953`.
- Updated delivery push: local work head, upstream, and PR head verified equal at
  `424f1b80950450a0f462427483ac8fe36d2d785a`; the branch was zero behind `origin/main` at push time.
- Alpha.3 delivery push: local work head, upstream, and PR head verified equal at
  `2d50164b38d6b3a601e0620dba0dd374844f053a`; the branch was zero behind `origin/main` at push time.
- Transport checkpoint push: local, upstream, and PR head verified equal at
  `e91b6ecadde05e9ecad6b3126a5b44e1ea10b57b`; the branch remained zero behind `origin/main`.
- Replacement PR checks: workflow run 31631354398 passed the clean-machine build, every service
  carve, all unit/integration jobs (including Auth), and `ci-complete` on exact head `e91b6eca`.
  PR-level API/UI E2E skipped as designed and `full-e2e` remains the sole E2E-tier label.
- Second queue admission: exact remote head `e91b6eca` entered at position 1 with
  `mergeQueueEntry.state = AWAITING_CHECKS`; `full-e2e` is the sole E2E-tier label.
- Second queue failure: merge-group run 31634022795 passed API E2E but UI E2E failed before any
  scenario when GitHub's Stripe release API returned HTTP 503. PR #517 was ejected to `OPEN/CLEAN`.
  Two independent failures across release discovery and asset delivery make the unpinned GitHub
  release bootstrap a CI defect rather than a one-off transient.
- Bootstrap correction: all three E2E jobs use one local composite action that pins Stripe CLI
  `1.45.2` and installs it from Stripe's signed official apt repository. This removes runtime
  discovery of `latest`, the GitHub release API, and GitHub release assets without adding retries.
- Bootstrap verification: both workflow YAML documents parse, all three call sites resolve to the
  local composite action, no old GitHub release reference remains, `git diff --check` passes, and
  Stripe's official apt metadata publishes exact package `1.45.2`. A clean Ubuntu container pull
  stalled in Docker Desktop and was not counted as evidence; queue execution remains authoritative.
- Platform `0.955` merge (`84c07a5d`): merged platform-sync PR #527 without conflict and advanced
  Auth's published platform closure from `0.953` to `0.955`; Reunion remains pinned to alpha.3.
- Bootstrap repair push: local, upstream, and PR work heads verified equal at
  `3615bfa36cef0867f2f34d10bf21e55cb6d51a36`; the branch was zero behind `origin/main`.
- Bootstrap transport push: local, upstream, and PR final heads verified equal at
  `319680347b952db5d0fd351303c8891e445fd3c5`; replacement workflow run 31637748572 passed build,
  frontend/backend carves, every unit/integration job, and `ci-complete` on that exact head.
- Third queue failure: merge-group run 31638703850 successfully retrieved Stripe's signing key,
  verified apt metadata, downloaded, and installed exact package `1.45.2`; the action then failed only
  because its presentation-string assertion rejected `stripe version` output. API/UI scenarios did
  not run and PR #517 was ejected. The assertion now verifies the installed dpkg version exactly and
  invokes `stripe version` separately to prove the executable starts.
- Version-validation repair push: local and upstream work heads verified equal at
  `a300cf7351a766d3791b15eab9bc4babd3c99e1a`; the branch was zero behind `origin/main`.
- Final full-E2E and merge: run 31641085535 passed the pinned Stripe installer in both jobs, API E2E,
  UI E2E, and `ci-complete`; PR #517 landed as `bf723db98bd1acb573d55e3cbe50692efea0e2cf`.
- Publication: run 31644761975 succeeded at platform version `0.1.0-alpha.0.958` and verified a fresh
  restore of the complete 40-package published closure from GitHub Packages.
- Platform sync: generated PR #531 is open at `d86e79e7f0071ab63f50d1181bf931bcee792b58`.
- Source cleanup: `scripts/worktrees.ps1 close -PlanManaged` removed the merged
  `Feature/typed-result_auth-outcomes` worktree plus its local and remote branches.
- Platform sync completion: PR #531 passed workflow run 31645544356 and merged as
  `9d1dd7acd17badb54a93ef9b269802e1590ceb21` at platform `0.1.0-alpha.0.958`.
- Reviewed PR checks: remote head `dd9e3111a4b6689cf46b9232275fccd63a349b72` passed build, all
  service carves, all unit/integration jobs, and `ci-complete`; PR-level E2E skipped as designed.
- Queue readiness: a final fetch proved that remote head zero behind `origin/main`, `OPEN/CLEAN`, and
  labelled only `full-e2e`; the served untracked review work order was removed.
- Initial queue request: GitHub left `mergeQueueEntry` null because the existing bot auto-merge
  request dated from PR creation; one explicit disable/enable re-assertion is required.
- Queue admission: the one-time re-assertion admitted exact reviewed head `dd9e3111a` with
  `mergeQueueEntry.state = QUEUED`; `full-e2e` is selected for the queue-only API/UI suites.
- Queue failure: merge-group run 31618590547 passed build, all service carves, unit/integration, and
  API E2E, then failed before UI scenarios when Stripe CLI's GitHub release-asset download returned
  curl exit 56. GitHub ejected PR #517; no retry was requested.
- UI bootstrap classification: the API E2E job in the same run downloaded and executed Stripe CLI
  `1.45.2` successfully at `16:42:55`; the UI job ran the identical installer at `16:52:38` and its
  release-asset connection died after curl's five transport attempts. No archive extraction, test
  host, browser, or scenario started. This is a formally classified external transient, with no CI
  workflow or Auth code defect evidenced and no retry/timeout padding justified.

## Verification

Final producer-reconciled candidate:

- Auth unit tests: 13 passed, 0 failed in Release against `Reunion` alpha.3.
- Auth integration tests through `integration-debug`: 54 passed, 0 failed in Release against
  `Reunion` alpha.3 using the real SQL Testcontainers fixture.
- Current-main focused recheck at `38e62584`: Auth Release build 0 warnings/0 errors, 13 unit tests
  passed, and all 54 real-SQL integration tests passed after restoring generated assets removed by
  the earlier disk cleanup.
- Platform `0.955` focused recheck: Auth Release build passed with 0 warnings/0 errors and all 13 unit
  tests passed. Integration startup could not reach Docker after the clean Ubuntu image pull stalled;
  all cases failed at the shared Testcontainers fixture with `DockerEndpointAuthConfig`, and the
  mandatory `docker-health.ps1` data-path check also timed out. No SQL or application code ran, so
  this is a formally classified local Docker environment failure; replacement CI is authoritative.
- Typed-result architecture tests: 16 passed, 0 failed on the final candidate.
- Fresh standalone Auth carve: 0 errors against published platform `0.1.0-alpha.0.953`; only existing
  analyzer warnings. The verified temporary carve was removed.
- Restored package graph resolves both `Reunion` and `Reunion.Errors` exactly
  `0.1.0-alpha.3`; Auth has no alpha.1/alpha.2 pin and no Validation or AspNetCore package.
- Signature, construction-factory, package-ownership, migration, scope, and `git diff --check` gates
  pass. The service surface remains two `Option<T>`, four `UnitResult<TError>`, and two intentional
  completion-only email operations.
- Full Release solution build: 0 errors and 2 existing generated E2E nullable-annotation warnings.
  The serialized no-restore build completed under concurrent host load; the warm authoritative rerun
  completed in 1m29s with build servers, shared compilation, and parallel builds disabled.
- Current-main full Release solution build: 0 errors and 4 existing warnings in 9m08s using
  `--no-restore`, disabled build servers/shared compilation, and a single MSBuild node. This verifies
  the merged platform `0.950` candidate at local head `8c4cd5b47`.
- Platform `0.953` full restore/Release solution build: 0 errors and the same 4 existing warnings in
  12m45s with build servers/shared compilation disabled and a single MSBuild node. This verifies
  local source head `fd69b70f0`.
- Alpha.3 full-solution build attempts: no code diagnostic was reached. The first run failed after
  14m05s with 54 disk-full write errors; after removing 6.33 GiB of this worktree's generated output,
  the clean/warm attempts again exhausted the shared drive while compiling unrelated late Customer
  integration projects. Auth's focused tests and standalone carve are green; replacement PR CI must
  provide the authoritative clean-machine solution build.
- No API/UI E2E was run locally; the merge workflow owns any selected E2E tier.

## Reviews

The review work order `reviews/Feature-typed-result_auth-outcomes.md` records a clean full
correctness/security review with no open findings. Because the previously served review work order
was removed before the first queue attempt, incremental-review correctly fell back to a fresh review
of the complete branch net diff, followed by an incremental review of the conflict-free current-main
merge. The post-ejection incremental correctness/security review of the local observation tail,
reusable Stripe installer, and conflict-free platform `0.955` pin merge found no issues. Its marker
will be stamped to the compound candidate before push.

## Decisions, discoveries, blockers, and deviations

- Auth remains credential-only and has no Payment, B2B, Customer, role, tenant, or business-profile
  dependency. The migration changes only Auth-owned runtime/tests plus its existing Shared
  architecture guard.
- Expected caller-actionable refusals are typed in process. Infrastructure, cancellation, malformed
  identity, and invariant failures remain exceptions. Token/credential mismatch is an invariant defect.
- Unknown credentials, wrong passwords, unverified credentials, unknown reset emails, and invalid or
  orphaned tokens retain their existing privacy-equivalent edge behavior.
- `Reunion.AspNetCore` remains intentionally absent because Auth's UI is Razor Pages and the pages map
  owned carriers manually. `Reunion.Validation` is unused.
- Auth's integration fixture must scope process environment overrides through host startup, and direct
  logout service tests must supply an ambient request `HttpContext` for Duende logout-context access.
- Direct EF Core and Relational test references intentionally converge Auth's published platform
  closure with its local EF build.
- No EF model or migration change is planned. No local E2E run is authorized before a queue failure.
