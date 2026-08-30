# Code review — Plan/RepoSplit-Stage4-E2E-TestKit

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `3c910eab93743db6eafb5bcde2c701f3093f412e`  _(2026-08-30)_

**Security-reviewed up to commit:** `5fa21cb71da77607e553738b4d6111353d00c638`  _(2026-08-30)_

> Range reviewed: `037a9ec..89f4962` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **SEC1 — MEDIUM — security** — `api/Concertable.B2B/tests/E2ETests/Concertable.B2B.E2ETests.Server/E2EAdminExtensions.cs:25`
  Admin-key validation fails open when `E2E:AdminKey` is blank: an absent request header is also an empty byte sequence, so `FixedTimeEquals` succeeds and exposes the destructive reset endpoint. The same defect exists in the Customer and Payment E2E admin modules. Reject blank keys at startup, reject missing/blank headers before comparison, require the E2E host environment before registering or mapping the endpoints, and add regression coverage for blank configuration and an absent header.

- [x] **MB1 — HIGH — module boundary** — `api/Concertable.Shared/tests/Concertable.Testing.E2E/FleetProfile.cs:5`
  `FleetSurface`, `FleetProfile`, and `IFleetProjectProvider` name B2B and Customer inside `Concertable.Testing.E2E`, violating that project's rule: “This project is SERVICE-AGNOSTIC. Nothing service-specific goes here. Ever.” Move the fleet-specific composition contracts and source-provider factory into a fleet-owned project, and keep the shared harness APIs generic over endpoint values and project metadata.

## Incremental review — 2026-08-30

Range reviewed: `89f4962..abf045a` (2 commits).

- [x] **CV1 — MEDIUM — test convention** — `api/Concertable.B2B/tests/Concertable.B2B.E2EAdmin.UnitTests/E2EAdminSecurityTests.cs:10`
  The three new `E2EAdmin.UnitTests` suites directly exercise guard clauses and internal authorization helpers, contrary to the unit-testing rule “Do not add a unit test merely to cover a guard clause” and its requirement that endpoint/host wiring use the integration tier. Replace them with service-local integration coverage that boots the E2E admin route on a test host, proves missing/blank keys fail closed over HTTP/startup, and remove the internal-only unit-test exposure.

## Incremental review — 2026-08-30 (final fix pass)

Range reviewed: `abf045a..a3a8548` (1 commit).

- [x] **NAT1 — MEDIUM — native** — `api/Concertable.Payment/tests/Concertable.Payment.E2EAdmin.UnitTests/E2EAdminSecurityTests.cs:1`
  The Payment unit-test source was left orphaned when its project moved to `E2EAdmin.IntegrationTests`, so the obsolete internal-helper tests silently stopped compiling instead of being removed as CV1 required. Delete the stale file and now-empty unit-test directory, then verify no `E2EAdmin.UnitTests` paths remain.

## Incremental review — 2026-08-30 (cleanup pass)

Range reviewed: `a3a8548..3c4255e` (1 commit).

No new findings. Native correctness and security coverage were both clean; the deletion retained equivalent Payment integration coverage and left no stale `E2EAdmin.UnitTests` paths.

## Incremental review — 2026-08-30 (main sync)

Range reviewed: `3c4255e..5fa21cb` (23 commits, including the current `origin/main` merge).

No new findings. Native correctness, repository boundary/convention, and security review were clean. The sole merge conflict was regenerated inventory; source-mode and package-only carve builds both passed against platform version `0.1.0-alpha.0.1267`.

## Incremental review — 2026-08-30 (CI verifier repair)

Range reviewed: `3bf078f..755f3a1` (1 commit).

No new findings. Native correctness, error handling, boundary, convention, and test-coverage lenses were clean. The change preserves the exact-one and version checks for projects that resolve `Concertable.DataAccess.Infrastructure` while allowing host-only integration projects to omit that package. No security-sensitive path changed, so the security layer was not required for this range.

## Incremental review — 2026-08-30 (merge-queue AppHost repair)

Range reviewed: `fe9f443..3c910ea` (1 commit).

No new findings. Native correctness, Aspire project-metadata behavior, service-boundary policy, and package-only isolation were clean. Marking the two AppHost references as non-resource compiler references prevents the fleet source provider from generating shadow `Projects.*_AppHost` markers, so startup resolves the markers from the executable AppHost assemblies instead.
