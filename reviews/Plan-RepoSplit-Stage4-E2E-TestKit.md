# Code review — Plan/RepoSplit-Stage4-E2E-TestKit

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `abf045ad259f61774ab07fdc404c18b0ad35239f`  _(2026-08-30)_

**Security-reviewed up to commit:** `abf045ad259f61774ab07fdc404c18b0ad35239f`  _(2026-08-30)_

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
