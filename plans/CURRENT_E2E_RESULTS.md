# Current E2E Results — 2026-05-29

Branch: `Refactor/Microservices`, head `ec3a6723` ("Drop Roles= from B2B auth attributes, swap IHealthWaiter for IHealthCheck, add Customer dev seeders").

## Summary

**9 passed, 21 failed (out of 30 total)**

| Suite | Total | Passed | Failed |
|-------|-------|--------|--------|
| B2B | 23 | 7 | 16 |
| Customer | 7 | 2 | 5 |

## Passing scenarios

- **B2B (7):** New artist manager registers · Venue manager books artist on a door split · Venue manager books artist on a flat fee · Venue manager signs in via OIDC · Artist pays hire fee upfront to book venue · New venue manager registers · Venue manager books artist on a versus deal
- **Customer (2):** New customer registers and signs in · Customer signs in via OIDC

## Failing scenarios (21)

All 21 failures cluster on Stripe payment flows. Auth (the original 403 on `/api/venue/user` and `/api/artist/user`) and fixture readiness are no longer in the failure set.

- **B2B (16):** all 3DS scenarios, all "new card" variants, all declined-card variants, across flat-fee / door-split / versus / venue-hire
- **Customer (5):** Customer 3DS authentication fails · Customer completes 3DS challenge · Customer purchases a ticket using a new card · Customer purchase is declined · Customer searches for concerts, purchases a ticket

## What landed in `ec3a6723`

### 1. B2B 403 fix — drop `Roles =` from auth attributes

The attributes introduced in `56ace89d` set both `Policy = "X"` and `Roles = "X"`. The framework's role check fires before the policy handler; the JWT after the role-agnostic auth refactor (`1089da3d`) no longer carries a static `role` claim, so every protected request 403'd without reaching the DB-backed `XManagerProfileHandler`.

Renamed `AuthorizeXAttribute` → `XAttribute` (dropping the `Authorize` prefix per C# convention) and removed `Roles =`. Policy handler — which checks for the row in `[user].[XManagerProfiles]` keyed by `sub` — is the source-of-truth role check. Updated 7 B2B controllers.

### 2. Readiness gate via `IHealthCheck`

`E2EDbInitializer` was a test-fixture-only wrapper that called `DevDbInitializer` then awaited a custom `IHealthWaiter` until `[user].[Users]` reached 71 (event-driven by `CredentialRegisteredEvent`). It deleted the abstraction and the wrapper:

- Added `UserHealthCheck : IHealthCheck` in `Concertable.B2B.User.Infrastructure.Data` — polls user count, returns `Unhealthy("users=N/71")` until the target is reached.
- Registered via `services.AddHealthChecks().AddCheck<UserHealthCheck>("users")` in `AddUserModule`. Fires in dev AND E2E with one registration.
- `Concertable.ServiceDefaults.MapDefaultEndpoints` already maps `/health` via `MapHealthChecks` in Dev/E2E, so no endpoint changes needed. The static `app.MapGet("/health", () => Ok())` line was removed (it caused an `AmbiguousMatchException` against the proper health check route).
- Deleted: `IHealthWaiter`, `DbHealthWaiter` + its `Log.cs`, `UserHealthWaiter`, `E2EDbInitializer`. Both `DevDbInitializer`s reverted to seeders-only.

The test fixture's existing `HealthWaiter.WaitForAllHealthyAsync([B2BWebUrl, ...])` at `AppFixture.cs:116` already polls `/health` — it now waits for the actual data without any custom abstraction. Architectural side-benefit: B2B Web's own `/health` honestly reports unready until users land, so Aspire / future health-aware orchestrators respect it.

### 3. Customer dev seeders replace `ProjectionSeeder`

`ProjectionSeeder` published three fake B2B events through ASB and polled SQL for projections to land — needed because `Concertable.Customer.AppHost` doesn't run B2B. Replaced with idempotent `IDevSeeder`s in each Customer module (`ArtistDevSeeder`, `VenueDevSeeder`, `ConcertDevSeeder`) that write the projection rows directly via EF. No bus, no polling, no ASB transport in the seed host. Idempotent so the umbrella `Concertable.AppHost` (where B2B does publish those events) doesn't double-write.

### 4. Observability — permanent source-gen logs in claims providers

`B2BProfileClaimsProvider` and `CustomerProfileClaimsProvider` previously had `catch { return []; }` (silent). Now they emit `B2BClaimsRequested / Received / NonSuccess / Failed` (and Customer equivalents) via `Concertable.Auth/Log.cs` source-gen. `UserClaimsController` logs the role being returned. These are permanent — useful for any future auth weirdness.

## Suggested next investigation

The 21 failures cluster on 3DS / declined-card / new-card payment flows. Most likely candidates:

- Stripe-CLI webhook routing (`stripe-cli` container forwards to `payment-web` — confirm port match in E2E)
- Payment-intent state handling (3DS requires `requires_action` → confirm → `succeeded` lifecycle)
- 3DS challenge iframe interaction in Playwright (frame switching, click timing)

Pick one failing scenario (e.g. `Venue manager completes 3DS challenge on flat fee`), re-run with `--filter "DisplayName~..."`, and inspect HTTP 4xx/5xx + `Resources.payment-web` `fail:`/`error:` lines in the test output. The screenshot under `playwright-failures/` will show the on-screen state at the moment of timeout.
