# B2B typed-result migration progress

- Plan: `plans/typed-result/B2B_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2BTypedResultMigration`
- Branch: `Refactor/B2BTypedResultMigration`
- PR: not opened
- Last reconciled: 2026-08-04

## Current state

Checkpoints 1-5 are complete on the single B2B migration branch. This checkpoint is merged through
`origin/main` `9abdd1cb6`, including platform `0.1.0-alpha.0.790`, the natural error-case naming
rules, and the published Kernel derived-code factories.

B2B read services now own missing-resource failures for Deal, Artist, Venue, Concert, Application,
Opportunity, Contract, and Invoice. API controllers only map successful payloads and terminate typed
Results. `ConcertService` owns the clock-dependent action capabilities; no B2B API project depends on
`Option`, and no B2B controller injects `TimeProvider`.

The B2B errors now follow the merged representation rules. Payload-free errors and errors whose
definition consumes all construction data are sealed definition records; Venue, Artist, Tenant, and
Concert Application no longer reference Dunet. Deal retains Dunet only for its structured validation
variants, with abstract root definitions and per-case overrides. Its cases are the direct natural
domain outcomes `Invalid` and `DealNotFound`; published codes remain pinned by contract tests.
`GetVatCalculationError` is now `VatCalculationError`.

Checkpoint 6 remains blocked: the Payment client on current main and platform
`0.1.0-alpha.0.790` still publicly imports FluentResults and exposes the legacy nullable result
contracts. No bridge or local source dependency was introduced.

Container-backed integration verification remains environment-blocked. The fresh-container Docker
HTTP health check passed, and Artist reached Docker plus SQL readiness, but Testcontainers then lost
its Docker endpoint during shared fixture startup. Artist reported 17 fixture failures and Concert
reported 136 immediate failures from the same unavailable fixture; the suite was stopped before
Tenant completed. No application integration test produced a valid result and the suite was not
retried.

## Next Steps

Wait for the Payment Phase 2 implementation to merge, publish, and platform-sync green. Then fetch
and merge current `origin/main`, verify the pinned `Concertable.Payment.Client` exposes the owned
typed Result surface, and proceed with checkpoints 6 and 7 only if that package gate is open. Do not
create a FluentResults adapter, string bridge, or local source dependency. Once Docker Desktop is
stable, run `scripts/docker-health.ps1`; only after it passes, run `scripts/integration.ps1 b2b` once
and record the per-project results. Do not retry the current Docker fixture failure unchanged.

## Completed work

- Checkpoints 1-5: Deal, Tenant, Venue/Artist, User, and Payment-independent Concert owned-result
  migrations, preserved from the branch's existing commits.
- Synced current `origin/main` into the branch and resolved ConcertController and Tenant GlobalUsings.
- Renamed branch from `Refactor/ConcertWorkflowDispatchers` to `Refactor/B2BTypedResultMigration`.
- Renamed worktree to
  `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2BTypedResultMigration`.
- Moved read errors into Application and named them by aggregate (`VenueError`, `ArtistError`,
  `DealError`, `ConcertError`, `ApplicationError`, `OpportunityError`, `ContractError`, `InvoiceError`).
- Moved absent-resource conversion from controllers into application services.
- Moved owner-concert action capability calculation into `ConcertService`; removed `TimeProvider`
  from `ConcertController`.
- Updated `api/agents/CODE_CONVENTIONS.md` with the controller boundary and error naming rules.
- Added architecture guards preventing B2B API dependencies on `Option` and controller dependencies
  on `TimeProvider`.
- Merged `origin/main` through `02b1e7381`, bringing platform `0.1.0-alpha.0.785` and the final
  typed-error representation conventions into the B2B branch.
- Replaced unnecessary Venue, Artist, Tenant, and Concert Dunet errors with sealed definition
  records and removed those projects' Dunet references.
- Updated Deal's necessary validation unions to abstract root definitions with per-case overrides.
- Renamed `GetVatCalculationError` to `VatCalculationError` and replaced status-shaped singleton
  factories with domain-named error values.
- Merged the natural error-name and derived-code changes through `origin/main` `9abdd1cb6`, including
  the published Kernel `0.1.0-alpha.0.790` platform sync.
- Replaced Deal's `ValidationCase` / `NotFoundCase` names and alias factories with direct `Invalid`
  and `DealNotFound` cases using derived definitions while preserving the published codes.

## Verification

- `dotnet build api/Concertable.B2B/Concertable.B2B.slnx --configuration Release --no-restore`:
  succeeded, 0 errors (2 generated UI-test nullable warnings).
- `dotnet build api/Concertable.slnx --configuration Release --no-restore`: succeeded, 0 errors
  against platform `0.1.0-alpha.0.790` (5 pre-existing warnings outside this correction).
- B2B architecture tests: 6 passed, 0 failed.
- Deal unit tests: 21 passed, 0 failed.
- Tenant unit tests: 115 passed, 0 failed.
- Concert unit tests: 75 passed, 0 failed.
- Conversations unit tests: 6 passed, 0 failed.
- B2B API source audit: zero `.OrFailure(` calls and zero `TimeProvider` dependencies in `*.Api`.
- Payment gate: blocked; platform `0.1.0-alpha.0.790` still exposes FluentResults from
  `Concertable.Payment.Client`, including nullable release/refund success payloads.
- Docker health: fresh-container host-to-container HTTP data round-trip passed.
- B2B integration suite: environment-blocked during shared fixture startup after Artist SQL
  readiness. Artist reported 17 fixture failures and Concert 136 fixture failures; no application
  result is valid, the runner was stopped before Tenant completed, and no retry was attempted.
- Final reconciliation: merged with `origin/main` `9abdd1cb6`; typed-error conventions are applied
  and all non-container verification is green.

## Decisions, discoveries, blockers, and deviations

- Read-path errors use aggregate nouns. Mutation errors retain verb prefixes where they disambiguate
  the operation. Alternate lookup factories name the missing key, for example
  `InvoiceError.ConcertNotFound(concertId)`.
- Repository nullability remains a persistence concern. Application services compose the published
  Kernel `ToOption().OrFailure(...)` API and expose typed Results.
- A proposed direct nullable-to-Result Kernel extension was not retained: B2B consumes the published
  Kernel package, so adding and consuming it here would violate the B2B-only package boundary.
- Payload-free singleton errors use domain names rather than HTTP status names; their
  `ErrorDefinition.Kind` remains the centralized transport-policy source.
- `GetVatCalculationError` became `VatCalculationError`; the redundant `Get` prefix is reserved out
  of default read errors while mutation errors keep their disambiguating verb.
- Necessary Dunet unions expose natural cases directly. Deal call sites construct `Invalid` and
  `DealNotFound`; they do not route through alias factories or `Case`-suffixed types.
- Integration tests must be rerun once Docker remains stable through Testcontainers startup; the
  fixture failure is not application evidence.

## Event log

### 2026-08-04 - main sync and B2B controller-boundary correction

- Action: merged current `origin/main`, renamed the B2B branch/worktree, resolved conflicts, and
  corrected read-result ownership across the affected B2B modules.
- Evidence: full solution Release build, architecture tests, affected unit suites, source audits,
  and Payment client inspection recorded above.
- Outcome: locally verified code checkpoint; integration pending; Payment-dependent checkpoint 6
  remains blocked.
- Follow-up: perform `## Next Steps` when Docker and the Payment package prerequisite allow it.

### 2026-08-04 - post-checkpoint mainline advance discovered

- Action: reconciled branch state after local commit `ed800758a`.
- Evidence: `git rev-list --count HEAD..origin/main` returned 11; the range includes
  `eb87a6225 docs(api): codify typed error union conventions` and
  `52ad35432 docs(api): simplify typed error representation`.
- Outcome: no additional merge was started in this turn; the convention-sync detour is the first
  item in `## Next Steps`.
- Follow-up: sync and reconcile in the next prompt.

### 2026-08-04 - typed-error convention reconciliation

- Action: merged `origin/main` `02b1e7381`, resolved the plan conflict, reconciled B2B error
  representations and names with the merged conventions, and removed unnecessary Dunet references.
- Evidence: B2B carve and full solution Release builds succeeded with 0 errors; architecture 6/6,
  Deal 21/21, Tenant 115/115, Concert 75/75, and Conversations 6/6 passed.
- Outcome: the B2B convention correction is complete and locally verified; Payment-dependent
  checkpoint 6 remains blocked on the published typed Payment client.
- Follow-up: wait for the Payment publish/platform-sync gate, and rerun B2B integration only after
  Docker remains stable through the health and fixture startup gates.

### 2026-08-04 - B2B integration environment failure

- Action: ran the mandatory Docker data-round-trip health check, started
  `scripts/integration.ps1 b2b`, inspected the per-project logs, and stopped the runner after the
  shared Testcontainers fixture lost its Docker endpoint.
- Evidence: the Docker health check passed; Artist reached SQL readiness, then all 17 Artist tests
  and all 136 Concert tests reported the same `DockerEndpointAuthConfig` fixture failure.
- Outcome: no application integration result was produced; the run is environment-blocked and was
  not retried.
- Follow-up: stabilize Docker Desktop, rerun the health check, then run the B2B suite once.

### 2026-08-04 - natural case names and derived-code publication synced

- Action: merged the natural-name convention, the Kernel derived-code implementation, and its
  `0.1.0-alpha.0.790` platform sync; then reconciled the Deal unions to the published surface.
- Evidence: the full Release solution build passed with 0 errors; architecture 6/6, Deal 21/21,
  Tenant 115/115, Concert 75/75, and Conversations 6/6 passed; controller and stale-case audits were
  empty.
- Outcome: checkpoints 1-5 are current with `origin/main` and the latest typed-result conventions.
  Checkpoint 6 remains blocked because the published Payment client still exposes FluentResults.
- Follow-up: wait for Payment Phase 2 publication and platform sync; do not bridge the package gate.

### 2026-08-04 - reconciled into the typed-result epic folder (ROADMAP → PLAN → PROGRESS)

- Action: brought this worktree's legacy flat B2B plan/ledger into the `plans/typed-result/` epic
  folder per the plans convention. Created `plans/typed-result/B2B_PLAN.md` (Full PLAN tier, spun off
  the roadmap's B2B phases as checkpoints 1-7), `git mv`d this ledger from
  `plans/TYPED_RESULT_MIGRATION_PROGRESS.md` to `plans/typed-result/B2B_PROGRESS.md`, and repointed the
  dangling `- Plan:` header and resume prompt (both had targeted the pre-rename
  `plans/TYPED_RESULT_MIGRATION.md`, since promoted to the roadmap). Added the B2B plan/ledger to the
  roadmap's pointer block. This is the "repoint/relocate on its own sync" step the plans-convention
  overhaul (§6) deferred to each in-flight typed-result worktree.
- Evidence: `git status` shows `R plans/TYPED_RESULT_MIGRATION_PROGRESS.md -> plans/typed-result/B2B_PROGRESS.md`;
  repo grep leaves no B2B reference to the old paths — surviving `TYPED_RESULT_MIGRATION.md` hits belong
  to the DERIVED_CODES/ERROR_CASE_NAMES ledgers (owned by other worktrees' syncs) and the overhaul
  plan's own rename table.
- Outcome: docs-only structural reconcile; no code, migration state, or checkpoint status changed —
  checkpoints 1-5 shipped, 6-7 blocked on the Payment package gate.
- Follow-up: none for the reconcile; the substantive next action is unchanged in `## Next Steps`.

## Resume prompt

```text
cd C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2BTypedResultMigration
Read @plans/typed-result/B2B_PLAN.md and @plans/typed-result/B2B_PROGRESS.md and do what its `## Next Steps` says.
```
