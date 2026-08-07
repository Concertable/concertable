# Tenant configuration surface progress

- Plan: `plans/launch/TENANT_CONFIG_SURFACE_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_tenant-config-surface`
- Branch: `Feature/launch_tenant-config-surface`
- PR: not opened
- Dependency/package gates: no implementation dependency; full merge-queue E2E and post-merge platform sync required
- Last reconciled: 2026-08-07 against the pending merge of current `origin/main`; no PR exists

## Current state

Both implementation phases are committed and reconciled with current `origin/main`. The Tenant
aggregate and Organization surface own the nullable overrides and platform-default fallback; Concert
consumes effective values for PRS, supplier VAT, supplier payment terms, and venue cancellation
notice. The synchronized tree builds with zero errors and both affected unit suites are green. The
Concert integration rerun could not start because Docker Desktop failed the repository health check.

## Next Steps

Complete the current-main merge checkpoint, then run the repository code-review workflow and address
every clear finding. Open the PR with full merge-queue E2E and own the post-merge platform-sync gate.

## Completed work

- Repository and code reconnaissance completed; the plan records the concrete shipped seams and the
  implementation design.
- Phase 1 implemented: Tenant configuration value object and EF complex mapping, appsettings options
  and field-wise resolver, Tenant contracts/module/service/API mapping, Organization UI controls,
  regenerated initial migration, seed/test plumbing, and focused tests; committed as `d9cb98260`.
- Phase 2 implemented: venue PRS is licence-aware and deducted before the artist split; supplier VAT
  uses the resolved tenant rate; supplier payment terms defer settlement; venue cancellation notice
  rejects late cancellation; all four have focused unit/integration coverage; committed with the
  roadmap gates as `a9ca579a9`.
- Current `origin/main` reconciled in this commit; the revenue-share resolver preserves the PRS
  deduction while returning the landed `Money` value type, and its unit assertion uses the typed
  amount.
- Launch roadmap §5 row and §7 gate ticked; the separate solicitor-owned cancellation/refund matrix
  remains open.

## Verification

- Branch identity: `Feature/launch_tenant-config-surface`, based on current `origin/main`; tracked tree
  clean before plan creation.
- Open platform-sync PR #384 was checked before branching; all required checks were green.
- `dotnet build api/Concertable.slnx --no-restore`: succeeded, 0 errors (6 existing warnings).
- Tenant unit tests: 103/103 passed.
- Tenant integration tests: 56/56 passed from the short `C:\tmp\concertable-tc` worktree junction.
- Targeted fresh-context configuration round trip: 1/1 passed.
- `api/initial-migrations.ps1`: succeeded for all contexts; Tenant migration re-scaffolded as
  `20260805214707_InitialCreate` with nullable configuration columns and decimal precision.
- `npm run build:web-packages`: succeeded.
- `npm run build:customer`, `build:venue`, `build:artist`, and `build:business`: succeeded.
- Tenant unit tests after consumer wiring: 106/106 passed.
- Concert unit tests: 82/82 passed.
- Concert integration module: B2B 146/146 and Customer Concert 11/11 passed.
- Supplier payment-terms ownership correction: Concert unit 82/82 and targeted B2B integration 1/1
  passed.
- Final `dotnet build api/Concertable.slnx`: succeeded with 0 errors (5 existing warnings).
- Post-sync `dotnet build api/Concertable.slnx`: succeeded with 0 errors (9 existing warnings).
- Post-sync Tenant unit tests: 106/106 passed.
- Post-sync Concert unit tests: 82/82 passed.
- Current-main reconciliation `dotnet build api/Concertable.slnx --no-restore`: succeeded with 0
  errors and 9 existing warnings.
- Current-main reconciliation Tenant unit tests: 106/106 passed.
- Current-main reconciliation Concert unit tests: 82/82 passed.
- Current-main reconciliation Concert integration run: no tests executed because Testcontainers
  could not resolve a healthy Docker endpoint; `scripts/docker-health.ps1` confirmed Docker Desktop's
  Linux engine was unhealthy with a 500 response.
- Post-sync SQL integration rerun did not start because both `docker ps` and
  `docker desktop status` timed out against the non-responsive local Docker Desktop control plane;
  the pre-sync B2B Concert 146/146 result remains green.

## Reviews

- Phase 2 implementation diff inspected after all gates; no local correctness or boundary finding
  remained.
- Formal repository code-review workflow remains a pre-PR delivery step.

## Decisions, discoveries, blockers, and deviations

- The root checkout was on unrelated `Fix/customer-review-authz`, so the feature uses the isolated
  sibling worktree above.
- `TaxCompliance` carries the music-licence attestation and the Organization form already round-trips
  it; the new configuration extends this path rather than creating another subsystem.
- The money-value-type branch landed on `origin/main`; merging it produced no textual conflicts, and
  the post-sync full solution build plus affected unit suites remained green.
- Payment terms are supplier-owned: artist for FlatFee/DoorSplit/Versus and venue for VenueHire,
  matching the existing settlement-payee and VAT supply direction.
- The long sibling worktree path exceeded the Windows native-loader limit for
  `Microsoft.Data.SqlClient.SNI.dll`. Running integration tests through the short
  `C:\tmp\concertable-tc` junction resolved the environment-only failure; the targeted test and full
  Tenant suite then passed.
- Two full-build attempts reached Playwright asset copies without compiler errors but exhausted C:.
  Removing every generated `bin`/`obj` beside a project recovered the missing nested outputs; the
  third exact build then passed without deleting shared Docker images or caches.
- No code or test blocker remains. A non-responsive local Docker Desktop control plane blocks only a
  repeat of the already-green SQL integration suite; full E2E remains assigned to the merge queue.
- The current-main merge changed `ISettlementAmountResolver` from `decimal` to `Money`; the single
  revenue-share conflict was resolved by retaining the venue PRS deduction and wrapping the computed
  artist share in `Money.Gbp`.

## Event log

### 2026-08-05 — plan created

- Action: fetched origin, passed the platform-sync branch gate, created the requested feature branch
  from `origin/main`, traced the existing implementation, and wrote the plan plus ledger.
- Evidence: worktree/branch metadata; `LAUNCH_ROADMAP.md` §5 and §7; Tenant, Concert, and shared-web
  source paths named in the plan.
- Outcome: the configuration mechanism and consumer design are explicit and implementation-ready.
- Follow-up: implement and verify Phase 1.

### 2026-08-05 — Phase 1 implemented and verified

- Action: added the persisted Tenant configuration mechanism, platform-default resolution, existing
  Organization API/form plumbing, focused unit/integration tests, and a regenerated Tenant migration.
- Evidence: Tenant unit 103/103; targeted integration 1/1; full Tenant integration 56/56; full API
  build 0 errors; shared web packages plus venue, artist, and business production builds green.
- Outcome: Phase 1 gate is green and ready for its checkpoint commit.
- Follow-up: commit Phase 1, then implement Phase 2 consumers.

### 2026-08-05 — Phase 2 implemented and affected suites verified

- Action: wired resolved tenant configuration into PRS settlement, supplier VAT, supplier payment
  terms, and venue cancellation; added unit and real-SQL integration coverage; ticked the roadmap row
  and launch gate.
- Evidence: Tenant unit 106/106; Concert unit 82/82; B2B Concert integration 146/146; Customer Concert
  integration 11/11; post-correction targeted payment-terms integration 1/1.
- Outcome: behavior and affected suites are green; full merge-queue E2E remains required.
- Follow-up: review and commit Phase 2.

### 2026-08-05 — final build gate green

- Action: reran the full solution build after clearing only feature-worktree generated artifacts.
- Evidence: `dotnet build api/Concertable.slnx` succeeded with 0 errors and 5 existing warnings.
- Outcome: every local Phase 2 verification gate is green.
- Follow-up: Phase 2 committed as `a9ca579a9`; run the formal branch review before PR delivery.

### 2026-08-06 — synchronized with current main

- Action: merged current `origin/main`, including PR #390's money-value refactor, then rebuilt the
  full solution and reran the affected Tenant and Concert unit suites.
- Evidence: merge completed without conflicts; `dotnet build api/Concertable.slnx` succeeded with
  0 errors and 9 existing warnings; Tenant unit 106/106 and Concert unit 82/82 passed.
- Outcome: the tenant-configuration settlement/VAT seam compiles and behaves correctly against the
  landed payment contract. The SQL integration rerun did not start because the local Docker Desktop
  control plane timed out; the earlier B2B Concert integration result is 146/146 green.
- Follow-up: run the formal branch review before PR delivery; full E2E remains required in the merge
  queue.

### 2026-08-07 — resume reconciliation found stale base

- Action: fetched origin and reconciled the ledger against the feature worktree, branch, working tree,
  other worktrees, and GitHub PR state before starting the formal review.
- Evidence: `HEAD` is `696adad2b`; the working tree is clean; no PR exists; `git rev-list --count
  HEAD..origin/main` returned 59.
- Outcome: the implementation remains committed on the correct isolated feature branch, but the
  recorded current-base claim is stale and review is blocked until the branch is synchronized.
- Follow-up: merge current `origin/main`, revalidate the affected gates, then run the formal review.

### 2026-08-07 — synchronized with current main and revalidated

- Action: merged current `origin/main`, resolved the revenue-share settlement conflict at the landed
  `Money` seam, updated the focused unit assertion, and reran the affected local gates.
- Evidence: full solution build succeeded with 0 errors and 9 existing warnings; Tenant unit 106/106
  and Concert unit 82/82 passed. Both Concert integration projects failed before executing tests
  because Testcontainers could not resolve Docker; `scripts/docker-health.ps1` independently failed
  against the Docker Desktop Linux engine with HTTP 500.
- Outcome: the synchronized implementation compiles and its affected unit behavior is green; the SQL
  integration rerun remains environment-blocked, with the previously green B2B Concert 146/146 and
  Customer Concert 11/11 results still the last executed integration evidence.
- Follow-up: complete the merge commit, run the formal branch review, and fix every confirmed finding.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_tenant-config-surface
Read @plans/launch/TENANT_CONFIG_SURFACE_PLAN.md and @plans/launch/TENANT_CONFIG_SURFACE_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
