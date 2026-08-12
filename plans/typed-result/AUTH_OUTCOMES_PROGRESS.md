# Auth expected-outcome migration progress

- Plan: `plans/typed-result/AUTH_OUTCOMES_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Roadmap item: `typed-result/auth-outcomes`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_auth-outcomes`
- Branch: `Feature/typed-result_auth-outcomes`
- PR: [#517](https://github.com/Concertable/concertable/pull/517), open at remote head
  `dd9e3111a4b6689cf46b9232275fccd63a349b72`
- Dependency/package gates: no implementation gate. NuGet.org publishes `Reunion` and
  `Reunion`, `Reunion.AspNetCore`, `Reunion.Errors`, and `Reunion.Validation` `0.1.0-alpha.3`; Auth has
  no Payment, B2B, or Customer runtime/package dependency. This branch owns Auth's semantic migration
  and direct `Reunion`/`Reunion.Errors` alpha.3 adoption. The separate repository baseline plan owns
  repository-wide version alignment. After the Auth `api/**` PR merges, this plan
  owns publication and its generated platform-sync gate to terminal green.
- Last reconciled: `2026-08-12` after formally classifying merge-group run
  [31618590547](https://github.com/Concertable/concertable/actions/runs/31618590547) as an external
  bootstrap transport failure and verifying the Auth alpha.3 candidate locally. The PR remains open
  on unchanged remote head `dd9e3111a4b6689cf46b9232275fccd63a349b72`; the local source update is
  not yet committed, reviewed, or pushed.

## Current state

The task directly matches this branch and worktree. No other worktree owns the Auth implementation.
PR #517 is open at verified remote head `dd9e3111a4b6689cf46b9232275fccd63a349b72`. Six local commits
after that head change only this active progress ledger. The current working candidate additionally
updates Auth's direct `Reunion` and `Reunion.Errors` pins from alpha.2 to alpha.3 and updates this plan
pair; it has not been committed, reviewed, or pushed. The first full-E2E merge group was ejected by an
external bootstrap-download failure, not an Auth build, carve, unit, integration, API E2E, or scenario
failure.

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

1. Commit the verified Auth alpha.3 package update and this plan checkpoint.
2. Run incremental review from the existing `dd9e3111` watermark; resolve every open finding.
3. Push the exact reviewed candidate through the plan push protocol and require replacement PR checks
   to pass on that exact head. The clean-machine `build` job is the authoritative full-solution gate
   because both local full-build attempts exhausted the shared drive while compiling unrelated late
   solution projects.
4. Keep `full-e2e`, enqueue PR #517 again, and follow it through merge, publication, generated
   platform sync, and plan close-out.

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
- Alpha.3 producer reconciliation (`this commit`): aligned Auth's direct `Reunion` and
  `Reunion.Errors` package closure to published `0.1.0-alpha.3` and audited Reunion source commit
  `91fdc6f2e33d8f396fa463ad309cb1288bea3be5`. Its new flexible Option HTTP terminals do not apply to
  Auth's Razor/Duende topology, so no runtime call site or AspNetCore package was added.
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

The review work order `reviews/Feature-typed-result_auth-outcomes.md` records clean full and
incremental correctness/security reviews with no open findings. The latest exact range is
`ac7b2341..dd9e3111` (43 commits including inherited, already-merged `main` work); branch-unique
non-merge commits change only the Auth plan pair and delivery ledger, while Auth runtime source is
unchanged. Both review markers are stamped through PR head
`dd9e3111a4b6689cf46b9232275fccd63a349b72`. The alpha.3 package update remains to be incrementally
reviewed after its commit.

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
