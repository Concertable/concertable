# Concertable-owned Result and Option migration progress

- Plan: `plans/TYPED_RESULT_MIGRATION.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2BTypedResultMigration`
- Branch: `Refactor/B2BTypedResultMigration`
- PR: not opened
- Last reconciled: 2026-08-04

## Current state

Checkpoints 1-5 are complete on the single B2B migration branch. This checkpoint merges current
`origin/main`, resolves its Concert/Tenant conflicts, corrects the read-result boundary exposed by
the merge, and renames the worktree/branch so its B2B ownership is explicit.

B2B read services now own missing-resource failures for Deal, Artist, Venue, Concert, Application,
Opportunity, Contract, and Invoice. API controllers only map successful payloads and terminate typed
Results. `ConcertService` owns the clock-dependent action capabilities; no B2B API project depends on
`Option`, and no B2B controller injects `TimeProvider`.

Checkpoint 6 remains blocked: the Payment client on current main still imports FluentResults, so the
typed Payment package required by the plan has not published and platform-synced. No bridge or local
source dependency was introduced.

Container-backed integration verification remains pending. Docker was initially reachable, then
became unresponsive during the first Artist Testcontainers startup; no integration test completed
and the stuck run was stopped without retrying.

## Next Steps

When Docker Desktop is healthy, run `scripts/integration.ps1 b2b` once and record the per-project
results. Then re-check the Payment Phase 2 publication/platform-sync gate. If the published Payment
client exposes owned typed Results, proceed with checkpoint 6 and checkpoint 7. Otherwise leave
checkpoint 6 blocked and do not create a FluentResults adapter or string bridge.

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

## Verification

- `dotnet build api/Concertable.slnx --configuration Release --no-restore`: succeeded, 0 errors
  (5 pre-existing warnings outside this change).
- B2B architecture tests: 6 passed, 0 failed.
- Deal unit tests: 21 passed, 0 failed.
- Tenant unit tests: 115 passed, 0 failed.
- Concert unit tests: 75 passed, 0 failed.
- Conversations unit tests: 6 passed, 0 failed.
- B2B API source audit: zero `.OrFailure(` calls and zero `TimeProvider` dependencies in `*.Api`.
- Payment gate: blocked; `Concertable.Payment.Client` on current main still uses FluentResults.
- B2B integration suite: environment-blocked before a result because Docker became unresponsive.

## Decisions, discoveries, blockers, and deviations

- Read-path errors use aggregate nouns. Mutation errors retain verb prefixes where they disambiguate
  the operation. Alternate lookup factories name the missing key, for example
  `InvoiceError.ConcertNotFound(concertId)`.
- Repository nullability remains a persistence concern. Application services compose the published
  Kernel `ToOption().OrFailure(...)` API and expose typed Results.
- A proposed direct nullable-to-Result Kernel extension was not retained: B2B consumes the published
  Kernel package, so adding and consuming it here would violate the B2B-only package boundary.
- Integration tests must be rerun once Docker is healthy; the startup failure is not application
  evidence.

## Event log

### 2026-08-04 - main sync and B2B controller-boundary correction

- Action: merged current `origin/main`, renamed the B2B branch/worktree, resolved conflicts, and
  corrected read-result ownership across the affected B2B modules.
- Evidence: full solution Release build, architecture tests, affected unit suites, source audits,
  and Payment client inspection recorded above.
- Outcome: locally verified code checkpoint; integration pending; Payment-dependent checkpoint 6
  remains blocked.
- Follow-up: perform `## Next Steps` when Docker and the Payment package prerequisite allow it.

## Resume prompt

```text
cd C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2BTypedResultMigration
Read @plans/TYPED_RESULT_MIGRATION.md and @plans/TYPED_RESULT_MIGRATION_PROGRESS.md and do what its `## Next Steps` says.
```
