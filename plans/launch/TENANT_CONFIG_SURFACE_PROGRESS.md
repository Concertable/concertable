# Tenant configuration surface progress

- Plan: `plans/launch/TENANT_CONFIG_SURFACE_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_tenant-config-surface`
- Branch: `Feature/launch_tenant-config-surface`
- PR: not opened
- Dependency/package gates: no implementation dependency; full merge-queue E2E and post-merge platform sync required
- Last reconciled: 2026-08-05 against branch `Feature/launch_tenant-config-surface` at `origin/main`

## Current state

Both implementation phases are complete and all local gates are green. The Tenant aggregate and
Organization surface own the nullable overrides and platform-default fallback; Concert consumes
effective values for PRS, supplier VAT, supplier payment terms, and venue cancellation notice.

## Next Steps

Review the Phase 2 diff and commit the verified consumer/roadmap checkpoint. Then run the repository
code-review workflow before opening a PR; the PR must use full merge-queue E2E and the post-merge
platform-sync gate.

## Completed work

- Repository and code reconnaissance completed; the plan records the concrete shipped seams and the
  implementation design.
- Phase 1 implemented: Tenant configuration value object and EF complex mapping, appsettings options
  and field-wise resolver, Tenant contracts/module/service/API mapping, Organization UI controls,
  regenerated initial migration, seed/test plumbing, and focused tests.
- Phase 2 implemented: venue PRS is licence-aware and deducted before the artist split; supplier VAT
  uses the resolved tenant rate; supplier payment terms defer settlement; venue cancellation notice
  rejects late cancellation; all four have focused unit/integration coverage.
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

## Reviews

- Phase 2 implementation diff inspected after all gates; no local correctness or boundary finding
  remained.
- Formal repository code-review workflow remains a pre-PR delivery step.

## Decisions, discoveries, blockers, and deviations

- The root checkout was on unrelated `Fix/customer-review-authz`, so the feature uses the isolated
  sibling worktree above.
- `TaxCompliance` carries the music-licence attestation and the Organization form already round-trips
  it; the new configuration extends this path rather than creating another subsystem.
- The parallel money-value-type branch may overlap settlement/VAT seams; whichever lands second must
  rebase there.
- Payment terms are supplier-owned: artist for FlatFee/DoorSplit/Versus and venue for VenueHire,
  matching the existing settlement-payee and VAT supply direction.
- The long sibling worktree path exceeded the Windows native-loader limit for
  `Microsoft.Data.SqlClient.SNI.dll`. Running integration tests through the short
  `C:\tmp\concertable-tc` junction resolved the environment-only failure; the targeted test and full
  Tenant suite then passed.
- Two full-build attempts reached Playwright asset copies without compiler errors but exhausted C:.
  Removing every generated `bin`/`obj` beside a project recovered the missing nested outputs; the
  third exact build then passed without deleting shared Docker images or caches.
- No code, test, or environment blocker remains.

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
- Follow-up: review and commit the Phase 2 checkpoint.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_tenant-config-surface
Read @plans/launch/TENANT_CONFIG_SURFACE_PLAN.md and @plans/launch/TENANT_CONFIG_SURFACE_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
