# Auth expected-outcome migration progress

- Plan: `plans/typed-result/AUTH_OUTCOMES_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Roadmap item: `typed-result/auth-outcomes`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_auth-outcomes`
- Branch: `Feature/typed-result_auth-outcomes`
- PR: not opened; no remote branch exists
- Dependency/package gates: no implementation gate. NuGet.org publishes `Reunion` and
  `Reunion.Errors` `0.1.0-alpha.2`; Auth has no Payment, B2B, or Customer runtime/package dependency.
  This branch owns Auth's semantic migration and alpha.2 source adoption. The separate alpha.2
  baseline plan owns repository-wide version alignment. After the Auth `api/**` PR merges, this plan
  owns publication and its generated platform-sync gate to terminal green.
- Last reconciled: `2026-08-12` after a clean incremental correctness/security review through
  `1afdb4b3396d2fde525a7a1da324b66cf9575f54`. Plan graph: 0 errors, 0 warnings.

## Current state

The task directly matches this branch and worktree. No other worktree owns the Auth implementation,
no Auth PR or remote branch exists, and no platform-sync PR is open. The branch was zero behind
`origin/main` at the Phase 6 checkpoint. The sole unrelated dirty path is the preserved untracked
clean-review work order `reviews/Feature-typed-result_auth-outcomes.md`.

Phases 1-5 are committed and locally verified. Auth's in-process contracts use direct published
Reunion ownership: login/logout return `Option<T>`, four caller-actionable refusal paths return
operation-owned `UnitResult<TError>`, and privacy-sensitive email operations remain completion-only.
Credential authentication/password decisions and token expiry transitions live in Auth domain
entities. Token/credential identity mismatch remains an invariant exception. Razor and Duende map
the carriers without leaking them into wire or persistence shapes.

Phase 6 is complete and fully verified. Auth aligns `Reunion` and `Reunion.Errors` to
`0.1.0-alpha.2`, uses unambiguous target-typed value/error conversions, exact `Success` cases, and
target-typed `null` for `None`. It contains no old Auth factory call and still uses neither
`Reunion.Validation` nor `Reunion.AspNetCore`.

## Next Steps

Fetch current `origin/main`, confirm the branch remains current, then run read-only PR preflight.
Record any concrete blocker; otherwise report Auth ready for PR delivery. Do not push or open a PR
without instruction; delivery still requires full merge-queue E2E plus publication and
platform-sync ownership to terminal green.

## Downstream handoffs

- Waiting ledger: `plans/typed-result/REUNION_SHARED_CONTRACTION_PROGRESS.md`.
  Gate: Auth must be delivery-ready and identify every remaining old carrier, terminal, and
  third-party dependency outside its owned scope.

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

## Verification

Final Phase 6 candidate:

- Auth unit tests: 13 passed, 0 failed on the final candidate.
- Auth integration tests through `integration-debug`: 54 passed, 0 failed on the final candidate.
- Typed-result architecture tests: 16 passed, 0 failed on the final candidate.
- Fresh standalone Auth carve: 0 errors against published platform `0.1.0-alpha.0.943`; 55 existing
  analyzer warnings. The verified temporary carve was removed.
- Restored package graph resolves both `Reunion` and `Reunion.Errors` exactly
  `0.1.0-alpha.2`; Auth has no alpha.1 pin and no Validation or AspNetCore package.
- Signature, construction-factory, package-ownership, migration, scope, and `git diff --check` gates
  pass. The service surface remains two `Option<T>`, four `UnitResult<TError>`, and two intentional
  completion-only email operations.
- Full Release solution build: 0 errors and 4 existing warnings in 17m47s using the ledger's exact
  serialized no-restore command with build servers, shared compilation, and parallel builds disabled
  after restoring platform `0.943` on an otherwise idle host.
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
