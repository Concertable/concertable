# Auth expected-outcome migration progress

- Plan: `plans/typed-result/AUTH_OUTCOMES_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Roadmap item: `typed-result/auth-outcomes`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_auth-outcomes`
- Branch: `Feature/typed-result_auth-outcomes`
- PR: [#517](https://github.com/Concertable/concertable/pull/517), open at remote head
  `424f1b80950450a0f462427483ac8fe36d2d785a`
- Dependency/package gates: no implementation gate. NuGet.org publishes `Reunion` and
  `Reunion.Errors` `0.1.0-alpha.2`; Auth has no Payment, B2B, or Customer runtime/package dependency.
  This branch owns Auth's semantic migration and alpha.2 source adoption. The separate alpha.2
  baseline plan owns repository-wide version alignment. After the Auth `api/**` PR merges, this plan
  owns publication and its generated platform-sync gate to terminal green.
- Last reconciled: `2026-08-12` after pushing the green, current-main work head
  `424f1b80950450a0f462427483ac8fe36d2d785a` to PR #517. A fetch verified the local work head,
  upstream, and PR `headRefOid` are identical; the branch is zero behind `origin/main` at
  `5bf622fecd600868b4ec437daf6c6ad0389029a6`.

## Current state

The task directly matches this branch and worktree. No other worktree owns the Auth implementation.
PR #517 is open at verified work head `424f1b80950450a0f462427483ac8fe36d2d785a`, current with
`origin/main` through `5bf622fec`. The updated candidate includes merge commits `962969cad` and
`fd69b70f0`; its full Release restore/build is green against platform `0.953`. The clean-review work
order is preserved in a local stash. No Auth implementation edit was required by PR #524's nullable/
Option guidance. Replacement PR checks are now being dispatched for the updated head.

Phases 1-5 are committed and locally verified. Auth's in-process contracts use direct published
Reunion ownership: login/logout return `Option<T>`, four caller-actionable refusal paths return
operation-owned `UnitResult<TError>`, and privacy-sensitive email operations remain completion-only.
Credential authentication/password decisions and token expiry transitions live in Auth domain
entities. Token/credential identity mismatch remains an invariant exception. Razor and Duende map
the carriers without leaking them into wire or persistence shapes.

Phase 6 and the final producer reconciliation are complete. Auth aligns `Reunion` and
`Reunion.Errors` to `0.1.0-alpha.2`, uses unambiguous target-typed value/error conversions, exact
unit `Success` cases, target-typed `null` for `None`, and direct forwarding where domain and service
result types already match. No factory, wrapper, cast, or result reconstruction should change.
Producer commit `113be42` adds flexible Option HTTP terminals, but Auth owns no Minimal API outcome
surface: its account handlers are server-rendered Razor Pages and its other outcome edge is Duende.
`Reunion.AspNetCore` therefore remains correctly absent.

PR #524's additional carrier guidance applies to Auth's boundary choices but requires no source
change: EF lookups remain nullable inside `AuthService`, while `LoginAsync` and `LogoutAsync` expose
intentional present-or-absent application outcomes as `Option<T>`. Auth does not wrap and immediately
unwrap technical nullability, leak Option into persistence, or use a Result where absence has no
distinct safe explanation.

## Next Steps

Transport this push checkpoint, restore the clean review work order from its local stash, and run the
required incremental correctness/security review over the new branch-owned range. Then wait for green
PR checks, apply `full-e2e` because Auth behavior is observable end to end, enqueue the exact reviewed
head, and own merge, publication, platform sync, and docs closeout to terminal completion.

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
- Producer reconciliation (`this commit`): audited Auth against Reunion `113be42`; the new flexible
  Option HTTP terminals do not apply to Auth's Razor/Duende topology, and existing construction plus
  direct result forwarding is already canonical. No implementation or focused-test edit was needed.
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

## Verification

Final producer-reconciled candidate:

- Auth unit tests: 13 passed, 0 failed in Release on the unchanged implementation.
- Auth integration tests through `integration-debug`: the existing final-candidate result remains
  54 passed, 0 failed. A fresh rerun did not start because Docker did not answer the mandatory
  `docker ps` preflight; no application or test failure occurred.
- Typed-result architecture tests: 16 passed, 0 failed on the final candidate.
- Fresh standalone Auth carve: 0 errors against published platform `0.1.0-alpha.0.943`; 55 existing
  analyzer warnings. The verified temporary carve was removed.
- Restored package graph resolves both `Reunion` and `Reunion.Errors` exactly
  `0.1.0-alpha.2`; Auth has no alpha.1 pin and no Validation or AspNetCore package.
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
- No API/UI E2E was run locally; the merge workflow owns any selected E2E tier.

## Reviews

The untracked work order `reviews/Feature-typed-result_auth-outcomes.md` records clean full and
incremental correctness/security reviews with no open findings. The latest range is
`e50d9bbe..1afdb4b3` (341 commits including inherited `main` merges); the branch-owned runtime delta
reviewed in depth is `af37d2618` and `1afdb4b3`. Both review markers are stamped through
`1afdb4b3396d2fde525a7a1da324b66cf9575f54`.

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
