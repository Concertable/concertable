# Tenant configuration surface progress

- Plan: `plans/launch/TENANT_CONFIG_SURFACE_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_tenant-config-surface`
- Branch: `Feature/launch_tenant-config-surface`
- PR: not opened
- Dependency/package gates: no implementation dependency; full merge-queue E2E and post-merge platform sync required
- Last reconciled: 2026-08-05 against branch `Feature/launch_tenant-config-surface` at `origin/main`

## Current state

Phase 1 is complete and verified. The Tenant aggregate now persists independently nullable
configuration overrides, resolves them against appsettings-backed platform defaults, and round-trips
them through the existing Organization API and shared venue/artist form. The regenerated Tenant
migration contains the four complex-property columns. Phase 2 consumer wiring is next.

## Next Steps

Commit the verified Phase 1 checkpoint, then implement Phase 2 of
`TENANT_CONFIG_SURFACE_PLAN.md`: consume resolved tenant configuration in PRS-adjusted revenue-share
settlement, supplier VAT decomposition, payer payment-term deferral, and venue cancellation timing.
Add focused unit/integration coverage, run the Phase 2 gates, and tick the two launch-roadmap items.

## Completed work

- Repository and code reconnaissance completed; the plan records the concrete shipped seams and the
  implementation design.
- Phase 1 implemented: Tenant configuration value object and EF complex mapping, appsettings options
  and field-wise resolver, Tenant contracts/module/service/API mapping, Organization UI controls,
  regenerated initial migration, seed/test plumbing, and focused tests.

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

## Reviews

No feature review run yet.

## Decisions, discoveries, blockers, and deviations

- The root checkout was on unrelated `Fix/customer-review-authz`, so the feature uses the isolated
  sibling worktree above.
- `TaxCompliance` carries the music-licence attestation and the Organization form already round-trips
  it; the new configuration extends this path rather than creating another subsystem.
- The parallel money-value-type branch may overlap settlement/VAT seams; whichever lands second must
  rebase there.
- The long sibling worktree path exceeded the Windows native-loader limit for
  `Microsoft.Data.SqlClient.SNI.dll`. Running integration tests through the short
  `C:\tmp\concertable-tc` junction resolved the environment-only failure; the targeted test and full
  Tenant suite then passed.
- No blocker currently prevents Phase 2.

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

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_tenant-config-surface
Read @plans/launch/TENANT_CONFIG_SURFACE_PLAN.md and @plans/launch/TENANT_CONFIG_SURFACE_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
