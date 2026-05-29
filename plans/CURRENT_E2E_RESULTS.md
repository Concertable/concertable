# Current E2E Results — 2026-05-29

Branch: `Refactor/Microservices`

## Run 4 — after switching to `IHealthCheck` (matches Run 2 baseline)

**9 passed, 21 failed (out of 30 total)**

| Suite | Total | Passed | Failed |
|-------|-------|--------|--------|
| B2B | 23 | 7 | 16 |
| Customer | 7 | 2 | 5 |

Same pass/fail set as Run 2. The Run 3 regression is fully resolved.

### What changed since Run 3

The `IHealthWaiter` abstraction was thrown out entirely in favour of `IHealthCheck` (the ASP.NET Core standard):

- New `UserHealthCheck : IHealthCheck` in `Concertable.B2B.User.Infrastructure.Data` — polls `[user].[Users]` count, returns `Healthy` when ≥ 71 else `Unhealthy("users={count}/71")`.
- `AddUserModule` now does `services.AddHealthChecks().AddCheck<UserHealthCheck>("users")` instead of registering `DbHealthWaiter` + `IHealthWaiter`.
- `Concertable.ServiceDefaults.MapDefaultEndpoints()` already wires `app.MapHealthChecks("/health")` in Dev/E2E — no new endpoint mapping needed. (Run 3 added an explicit `MapHealthChecks` call that duplicated this and caused `AmbiguousMatchException` on every `/health` probe.)
- Both `DevDbInitializer`s reverted to seeders-only (no `IEnumerable<IHealthWaiter>`).
- Deleted: `IHealthWaiter.cs`, `DbHealthWaiter.cs` + its `Log.cs`, `UserHealthWaiter.cs`.

Net effect: the test fixture's existing `HealthWaiter.WaitForAllHealthyAsync([B2BWebUrl, ...])` call at `AppFixture.cs:116` now waits for the actual data — `/health` returns 503 until 71 users land, then 200. No deadlock; no Program.cs reorder; less code than Run 2 had. Registration fires in dev AND E2E automatically via `AddUserModule`.

### Still failing (21)

Same set as Run 2: all 3DS scenarios, all "new card" variants, all declined-card variants, plus the Customer 3DS / ticket-purchase / declined-purchase / search-purchase scenarios. Per Run 2's analysis these are Stripe-flow related, not auth or fixture-readiness related.

---

## Run 3 — after `IHealthWaiter` merge (regressed)

**2 passed, 28 failed (out of 30 total)**

| Suite | Total | Passed | Failed |
|-------|-------|--------|--------|
| B2B | 23 | 0 | 23 |
| Customer | 7 | 2 | 5 |

All 23 B2B scenarios failed at fixture init (1 ms each) with:

```
Unhandled exception. System.TimeoutException: Timed out waiting for 71 UserEntity rows; last observed count 1.
```

…then `System.TimeoutException : Health check timed out for https://localhost:7086/health`.

### Root cause (regression, introduced in this run's prep)

`E2EDbInitializer` was deleted and its waiter loop folded into `DevDbInitializer` via `IEnumerable<IHealthWaiter>`. Per user instruction, `UserHealthWaiter` was registered inside `AddUserModule` so it would also fire in B2B Web's own startup. That registration creates a **chicken-and-egg deadlock** in B2B Web:

- `Program.cs` blocks on `await initializer.InitializeAsync()` before `app.Run()`.
- The waiter inside `InitializeAsync` polls `[user].[Users]` waiting for 71 rows.
- Those rows arrive only when the ASB consumer (a hosted service) consumes `CredentialRegisteredEvent`.
- Hosted services don't start until `app.Run()`, which is unreachable while `InitializeAsync` blocks.

The test fixture's host has the same registration via its own `AddUserModule` call, polls the same DB, and times out for the same reason — B2B Web never finishes starting, so no events ever get consumed.

### Fix (decided, not yet applied)

Move the `services.AddScoped<IHealthWaiter, UserHealthWaiter>()` line out of `AddUserModule` and back into `AppFixture` only. `DevDbInitializer`'s consumption of `IEnumerable<IHealthWaiter>` stays — B2B Web in dev mode just resolves an empty enumerable and doesn't wait. The test fixture's host (a separate process from B2B Web) gets the waiter and works.

Wanting the waiter to also fire in B2B Web's own startup is architecturally not possible from inside `DevDbInitializer` without a post-`app.Run()` hosted service.

### Salvageable observations from this run

- The new permanent source-gen logs in `Concertable.Auth/Log.cs` now report `CustomerProfileClaimsProvider: failed subjectId=…` instead of silently catching. This will be useful for diagnosing the remaining Customer payment-flow failures once B2B is unblocked.
- File moves performed (still good even after rolling back the registration): `DbHealthWaiter` and `IHealthWaiter` now in `Concertable.DataAccess.Infrastructure` / `Concertable.DataAccess.Application`; `UserHealthWaiter` in `Concertable.B2B.User.Infrastructure`. `E2EDbInitializer` deleted.

---

## Run 2 — after auth attribute + IBusTransport fixes

**9 passed, 21 failed (out of 30 total)**

| Suite | Total | Passed | Failed |
|-------|-------|--------|--------|
| B2B | 23 | 7 | 16 |
| Customer | 7 | 2 | 5 |

### Passing scenarios

- B2B: New artist manager registers · Venue manager books artist on a door split · Venue manager books artist on a flat fee · Venue manager signs in via OIDC · Artist pays hire fee upfront to book venue · New venue manager registers · Venue manager books artist on a versus deal
- Customer: New customer registers and signs in · Customer signs in via OIDC

### Still failing

- B2B (16): all 3DS scenarios, "new card" variants, declined-card variants across flat-fee/door-split/versus/venue-hire
- Customer (5): 3DS scenarios, ticket purchase scenarios, declined-card

The remaining failures look Stripe/payment-related rather than auth-related — happy-path bookings pass, 3DS/decline/new-card paths fail across all workflows.

## Run 1 baseline — for reference

1 passed, 29 failed. Customer all hard-failed in 1ms on `IBusTransport` DI; B2B 22/23 failed on 403 from `/api/artist/user` and `/api/venue/user`.

## Fixes applied between Run 1 and Run 2

### 1. B2B AuthorizeAttribute had `Roles = "..."` blocking before the DB-backed policy

In commit `56ace89d` ("Seed data redesign") three new attributes were introduced:

- `AuthorizeVenueManagerAttribute` — `Policy = "VenueManager"` AND `Roles = "VenueManager"`
- `AuthorizeArtistManagerAttribute` — `Policy = "ArtistManager"` AND `Roles = "ArtistManager"`
- `AuthorizeAdminAttribute` — `Policy = "Admin"` AND `Roles = "Admin"`

The `Roles` check fires before the policy handler. The JWT carried no `role` claim (the role-agnostic auth design from `1089da3d` removed it from the static token shape and put it on a dynamic-lookup path via `B2BProfileClaimsProvider`, which was returning empty), so every protected request 403'd without reaching the DB-backed `XManagerProfileHandler`.

**Fix:** dropped `Roles = "..."`; the policy handler (which checks for a `XManagerProfile` row in `[user].[XManagerProfiles]` keyed by `sub`) is the source-of-truth role check. Took the opportunity to rename the attributes to drop the `Authorize` prefix per C# convention (`[VenueManager]`, `[ArtistManager]`, `[Admin]`).

Files touched:
- Renamed `AuthorizeXxx.cs` → `Xxx.cs` in `api/Concertable.B2B/Modules/User/Concertable.B2B.User.Api/Authorization/`
- Updated 7 controllers in B2B (Venue, Artist, Concert, Application, Opportunity, VenueDashboard, ArtistDashboard)

### 2. Customer seed host was missing `IBusTransport` registration

In `56ace89d` the `AddAzureServiceBusTransport(...)` block was deleted from `Concertable.Customer.E2ETests.AppFixture.cs`, but `ProjectionSeeder.SeedAsync()` still calls `scope.ServiceProvider.GetRequiredService<IBusTransport>()` to publish seed `VenueChangedEvent`/`ArtistChangedEvent`/`ConcertChangedEvent`.

**Fix (interim):** restored the `AddAzureServiceBusTransport(...)` block. This unblocks Customer suite but is being replaced in the next change (see "ProjectionSeeder hackiness" below).

## Outstanding — ProjectionSeeder refactor in progress

`ProjectionSeeder` publishes 3 fake B2B events through ASB and polls SQL for the projections to land. It exists because `Concertable.Customer.AppHost` doesn't run B2B, so there's nothing to publish those events naturally.

Refactor in progress: replace with idempotent `IDevSeeder`s in each Customer module (Venue, Artist, Concert) that write the projection rows directly via EF. No bus, no polling, no ASB transport in the seed host. Same source-of-truth pattern as `AddCustomerPreferenceDevSeeder`. Idempotent so the umbrella `Concertable.AppHost` (where B2B does publish those events) doesn't double-write.

## Suggested follow-ups for the remaining 21 failures

The 16 B2B + 5 Customer failures all cluster on 3DS/declined-card/new-card payment flows. Most likely candidates: Stripe-CLI webhook routing, payment intent state handling, or 3DS challenge frame interactions. Worth triaging one failing scenario in detail (re-run with `--filter "DisplayName~3DS"` and inspect HTTP 4xx/5xx + Aspire payment-web logs).
