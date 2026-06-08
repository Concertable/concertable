---
name: e2e-api-debug
description: Run the Concertable API E2E suite (xUnit + Aspire DistributedApplication, no browser) and diagnose / fix failing tests. These spin up the FULL service stack (B2B/Customer Web + Payment + Auth + Search + ASB emulator + SQL + Stripe test mode), drive backends over HTTP, then poll real DB + Stripe state until the async outbox→inbox→event/gRPC/webhook chain settles. Use whenever the user wants to debug an API E2E failure, run the API E2E suite, or investigate a settlement / payment / event-propagation flow that fails at the service layer (not the browser). For Reqnroll/Playwright browser scenarios use `e2e-ui-debug`; to debug both layers together use `e2e-debug`; for in-process WebApplicationFactory module tests use `integration-debug`.
---

# e2e-api-debug

Run the Concertable **API** E2E suite and analyse failures using the `ShouldBe(HttpStatusCode)` failure bodies and the Aspire resource logs already piped into the test output. This suite is the layer **between** integration tests (in-process, mocked externals) and UI E2E (full stack + browser): it runs the **full Aspire stack with real Stripe test mode and a real ASB emulator**, but drives the backends directly over HTTP with no browser. It exists to prove the **async event-driven flows** — accept → draft → settlement payout, concert-finished → completion, ticket-purchase → ticket-created — actually complete end to end across services.

## The point of this skill: run autonomously — FIX failing tests yourself, do not ask

When the user invokes this skill they are delegating the **entire** run → diagnose → fix → verify loop to you, to run autonomously end to end. **Any failing test is something you fix in code yourself**, without stopping to ask permission, then re-run and keep going until the suite is green. Diagnose the root cause, write the code change (in the service, handler, dispatcher, fixture, or test — wherever the real bug is), and re-run to confirm green. The only time you pause is a genuine product-behaviour ambiguity you cannot resolve from the code (per the "Test vs prod code — ask first" convention). Otherwise: run, fix every failure you can, verify, report what you changed — in one pass.

## NEVER disable or bypass a step to get past its failure

The suite tests the CURRENT state of the code. If something is failing — a test, a build, a service startup, a health check — that failure is the thing to debug. "Fix" means make the failing step work, never make it stop running. Concretely banned moves:

- **Never suppress builds** (`--no-build`, `SuppressBuild`, skip-build flags) because a build hung — that swaps the failure for silently running stale binaries.
- Never inflate `Polling.UntilAsync` / `WaitFor*` timeouts to outlast a hang instead of finding what's hanging. A polling timeout is a *signal that the async chain didn't complete* — chase the chain, don't widen the window.
- Never disable, skip, or stub the failing resource / handler / check so the rest goes green.

If a step hangs with no useful output, reproduce it and observe it live (process trees, Aspire resource states, Docker containers) — do not remove the step. A bypass is only acceptable when the user explicitly asks for it after seeing the diagnosis.

## How this suite differs from the others (read first)

| | integration-debug | **e2e-api-debug (this)** | e2e-ui-debug |
|---|---|---|---|
| Host | in-process `WebApplicationFactory` | **full Aspire `DistributedApplication`** | full Aspire `DistributedApplication` |
| Externals | all mocked (Stripe, bus, email) | **real**: Stripe test mode, ASB emulator, SQL containers | real (same) + browser |
| Driver | `HttpClient` from WAF | **plain `HttpClient` → deployed service URLs** | Playwright browser |
| Test framework | xUnit (`FullyQualifiedName~`) | **xUnit (`FullyQualifiedName~`)** | Reqnroll (`DisplayName~`) |
| Signature failure | wrong HTTP status (sync) | **`Polling.UntilAsync` timeout (async chain didn't settle)** | element/visual timeout |
| Server logs | per-test `ITestOutputHelper` | **Aspire resource logs in test output** | Aspire resource logs in test output |

The big one: in this suite a failing test usually means **the synchronous call returned 200 but a downstream handler / outbox dispatcher / gRPC settlement / Stripe webhook never completed**, so a `Polling.UntilAsync` eventually times out. The root cause is almost never in the test — it's in a **service resource log**. Diagnosis is "which resource was supposed to react, and what did its log say?"

## Input

- **Fully-qualified test name** (e.g. `Concertable.B2B.E2ETests.Payments.ConcertDraftTests.ShouldCreateDraft_WhenDoorSplitApplicationAccepted`) — run Step 0, then jump to Step 2 for that single test.
- **A service** (`b2b` / `customer`) — run Step 0 then `./e2e.ps1 api b2b` (or `customer`).
- **No arguments** — run Step 0 then the full API suite (Step 1), then Step 2 for each failure.

## Key paths

**B2B API E2E** — `api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests/`
- Tests: `Payments/ConcertDraftTests.cs` (accept → draft → settlement payout), `Payments/ConcertFinishedTests.cs` (concert-finished → completion + door-split/versus payout)
- Fixture: `AppFixture.cs` (boots `Concertable.B2B.AppHost`, seeds via `DevDbInitializer`, exposes `B2BClient` / `Polling` / `StripePaymentIntents` / `SeedState` / `DbFixture`)
- DB helpers (raw SQL, for polling state): `DbFixture.cs`, `ApplicationDb.cs`, `BookingDb.cs`, `OpportunityDb.cs`
- Stack composition: `DistributedApplicationBuilderExtensions.cs` (`AddB2BE2E` — pins Payment/Auth/Search + stripe-cli)
- Last run log: `api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests/api-tests.last.log`

**Customer API E2E** — `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests/`
- Tests: `Payments/TicketPurchaseTests.cs` (purchase → PaymentSucceeded → ticket created)
- Fixture: `AppFixture.cs` (boots `Concertable.Customer.AppHost`; exposes `CustomerClient` / `Polling` / `SeedState` / `Catalog` / `DbFixture`)
- Last run log: `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests/api-tests.last.log`

**Shared E2E infra** (service-agnostic) — `api/Shared/Tests/Concertable.E2ETests/`
- `HealthWaiter` (`WaitForAllHealthyAsync` / `WaitForAllServingAsync` / `WaitForPayoutAccountsAsync`), `PollingService`, `AspireResourceLogger`, `TestTokenMinter`, `StripeE2EAccountResolver`.

**E2E trigger endpoints** (test-only, mapped on the B2B host) — `api/Concertable.B2B/Concertable.B2B.Web/Extensions/E2EEndpointExtensions.cs`: `POST /e2e/run-completion` (runs the concert-completion sweep), `POST /e2e/finish/{concertId}` (finishes one concert). These let the API E2E tests drive time-based flows deterministically.

**Run settings** — `api/Concertable.runsettings` (`MaxCpuCount=1`; the two E2E apps must not run concurrently — see memory `e2e_parallel_execution`). The wrapper passes this automatically.

**Scratch run logs** — capture ad-hoc `dotnet test` output under `api/Shared/Tests/Concertable.E2ETests/logs/` (git-ignored; `New-Item -ItemType Directory -Force` it first) — **never the repo root**. The canonical `api-tests.last.log` files written by `./e2e.ps1 api` stay in their project dirs.

## Step 0 — Pre-flight check

These tests need Docker (SQL containers, ASB emulator, stripe-cli):

```powershell
docker ps 2>&1
```

If this errors or the daemon is unreachable, stop and tell the user: **"Docker is not running — please start Docker Desktop before running E2E tests."** Do not proceed.

The suite also needs Stripe + Google secrets in the environment (`Stripe__SecretKey`, `GoogleApiKey`) — the same ones CI injects. If a run dies immediately with a Stripe auth error or missing-config exception, confirm those are set before debugging anything else.

Then tell the user: **"Starting API E2E suite — full Aspire stack boot, ~5–7 min. I'll report back when done."**

## Step 0b — Watch for startup hangs

Run the suite as a **background PowerShell task** (`run_in_background: true`). Note the output file path.

Do NOT just launch and wait. After launching, poll the output file every ~60 seconds for the first ~5 minutes using the **PowerShell tool** directly (NOT Monitor, NOT a background poller — inline calls):

```powershell
$lines = Get-Content "<output-file>" 2>&1
Write-Host "Lines so far: $($lines.Count)"
$lines | Select-String "Initializing|FixtureReady|Running|Waiting|Exited|fail:|error:|Passed|Failed|healthy|payout" | Select-Object -Last 20
```

Do this 3–4 times during the first 4 minutes to confirm resources reach `Running`. If after 2–3 minutes resources are still `unknown`/`Waiting` and none reach `Running`, diagnose immediately:

```powershell
docker ps -a --format "table {{.ID}}`t{{.Image}}`t{{.Status}}`t{{.Names}}" 2>&1
```

Common causes (shared with the UI suite — same AppHost):
- **ASB emulator exits with code 139** — "At least one subscription required per topic": a topic in `DistributedApplicationBuilderExtensions.cs` is declared with no subscriptions for the current service flags. Gate topic creation on the subscriber flag, not the publisher flag.
- **`WaitForPayoutAccountsAsync` never reaches 4** (B2B) — Stripe `PayoutAccount` rows are provisioned by `CredentialRegisteredHandler` in Payment reacting to `CredentialRegisteredEvent`. If the count stalls, the registration→payout event chain is broken — grep `Resources.payment-web` / `Resources.auth` logs. (Do NOT "fix" by seeding payout accounts directly — that violates `SEEDING_CONVENTIONS.md`.)
- **Workers crash with "address not configured"** — a project reference is missing from the AppHost's `AddWorkers(...)` call.
- **SQL container won't start** — port conflict / volume corruption; `docker volume prune` and retry. **OOM** — bump Docker Desktop memory.

Fix the root cause before re-running. Do not keep waiting on a stuck startup.

## Step 1 — Run the API suite

```powershell
./e2e.ps1 api run        # both services; exits non-zero on any failure
# or scope it:
./e2e.ps1 api b2b
./e2e.ps1 api customer
```

Each project writes its own `api-tests.last.log`. The wrapper prints a per-test `[PASS]`/`[FAIL]` list and a summary table. After it finishes, build a results summary to present before proceeding:

| # | Service | Test | Result |
|---|---------|------|--------|
| 1 | B2B | ShouldCreateDraftAndPayArtist_WhenFlatFeeApplicationAccepted | ✅ / ❌ |

Show totals, note which tests failed, and proceed to Step 2 for each.

## Step 2 — Re-run each failing test individually for enriched output

Re-run the failed test alone via `--filter` so the resource logs for that one flow aren't buried. **Use the PowerShell tool, not Bash** (backtick continuation is PowerShell-only):

```powershell
# B2B
dotnet test 'api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests/Concertable.B2B.E2ETests.csproj' --filter "FullyQualifiedName~ConcertDraftTests.ShouldCreateDraft_WhenDoorSplitApplicationAccepted" --settings api/Concertable.runsettings --logger "console;verbosity=normal"

# Customer
dotnet test 'api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests/Concertable.Customer.E2ETests.csproj' --filter "FullyQualifiedName~TicketPurchaseTests" --settings api/Concertable.runsettings --logger "console;verbosity=normal"
```

`FullyQualifiedName~` is a substring match. Drop the method to run a whole class. To keep the output for grepping, `Tee-Object` into the scratch logs dir (NOT the repo root):

```powershell
$logs = 'api/Shared/Tests/Concertable.E2ETests/logs'; New-Item -ItemType Directory -Force $logs | Out-Null
dotnet test '<csproj>' --filter "FullyQualifiedName~<test>" --settings api/Concertable.runsettings --logger "console;verbosity=normal" | Tee-Object -FilePath "$logs/<test-slug>.log"
```

The enriched detail (HTTP failure bodies, `Concertable.*` `ILogger` lines, and the forwarded Aspire **resource** logs) is in the `dotnet test` **console output** for this re-run — read it directly, not `api-tests.last.log`.

## Step 3 — Diagnose

Work the output in this order. **Identify the failure shape first** — it tells you where to look.

### Shape A — synchronous `ShouldBe(HttpStatusCode)` mismatch (the direct call failed)

The test's own HTTP call returned the wrong status. `response.ShouldBe(...)` (from `api/Shared/Tests/Concertable.Testing/HttpResponseAssertions.cs`) throws with the full context:

```
Expected 204 NoContent, got 400 BadRequest.
Request: POST http://localhost:7083/api/Application/3/accept
Body:
{"errors":{"PaymentMethodId":["The PaymentMethodId field is required."]}}
```

URL + status + request method + response body are always present — you should not need extra logging for a wrong-status failure. If the body alone doesn't say *why*, cross-reference the service resource log (below) for the same request.

### Shape B — `Polling.UntilAsync` timeout (the async chain didn't settle) — THE COMMON ONE

The sync call returned 200/204 but a `Polling.UntilAsync(...)` waiting on DB or Stripe state timed out (default 15–30s). The synchronous half worked; a **downstream reaction never completed**. The DB/Stripe state you polled tells you which reaction is missing — now find the resource that owns it and read its log.

**MANDATORY: grep the forwarded Aspire resource logs.** Each service's stdout/stderr is forwarded into the test output prefixed `Resources.<resource-name>`. Find the `fail:` / `error:` / `warn:` lines for the resource that should have reacted:

```powershell
# Whatever you captured in Step 2 (console output / teed scratch log):
Select-String -Path "<scratch-log>" -Pattern "Resources\.(payment-web|payment-workers|b2b-workers|search-workers|auth)\b" | Select-Object -First 80
Select-String -Path "<scratch-log>" -Pattern "fail:|error:|Exception|StatusCode=" | Select-Object -First 80
```

Map the missing state → the resource that produces it:

| Polled state that never appeared | Reaction that's broken | Where to look |
|---|---|---|
| Settlement `PaymentIntentId` (`DbFixture.Payment.GetLatestSettlementPaymentIntentId`) | Booking accepted/finished → gRPC settlement call → Stripe transfer | `Resources.payment-web` (gRPC handler exception), B2B outbox dispatcher |
| `Application`/`Booking` lifecycle state | domain event → outbox → handler advancing the FSM | B2B `Resources.b2b-workers` outbox dispatcher + handler |
| Customer `TicketEntity` after purchase | Stripe webhook → `PaymentSucceededEvent` → ticket handler | stripe-cli delivery + `Resources.customer-web`/workers handler |
| Search projection rows | `XChangedEvent` over ASB → Search projection handler | `Resources.search-workers` |

If a gRPC call returned an error (B2B Web logs `Status(StatusCode=...)`), the **real** exception is in the **callee's** resource log (`Resources.payment-web`), not the caller's — always chase it to the service that threw.

### Shape C — Stripe assertion mismatch (flow completed, value wrong)

`Assert.Equal(expectedAmount, intent.Amount)` or `DestinationId` mismatch means the settlement *ran* but computed the wrong number / destination. Look at the settlement calculation (door-split %, versus base + %, hire fee) and `StripeE2EAccountResolver.AccountIds[...]`. **Known gotcha:** `StripeE2EAccountResolver` is incomplete (only some seeded users are wired) — a missing destination may be a resolver gap, not a settlement bug (see memory `stripe_e2e_resolver_state`). Don't read resolver-miss state as "normal," but do confirm whether the manager under test is actually wired before blaming the settlement code.

### When the logs still don't pinpoint it — add tracing

If the resource logs, HTTP bodies, and DB/Stripe state still don't explain *why* a handler skipped/failed, add `ILogger` tracing to the server-side class rather than guessing. Read [`api/docs/DEBUGGING_CONVENTIONS.md`](../../../api/docs/DEBUGGING_CONVENTIONS.md) first and follow it: generic, future-useful logs (handler invoked/skipped/wrote, dispatcher lifecycle) get promoted to the project's `Log.cs` with `[LoggerMessage]` source-gen and **kept**; one-off probes stay inline and are removed once found. Then re-run the single test and read your new lines from the resource log in the console output.

## Step 4 — Fix and verify

1. Make the fix in the relevant service / handler / fixture / test.
2. Re-run the specific test (`--filter "FullyQualifiedName~<test>"`) to confirm green.
3. Re-run the affected service suite (`./e2e.ps1 api b2b` or `api customer`) to catch siblings.
4. If the change touched shared infra (Kernel, messaging, seeding, a fixture, the AppHost), run the **full** API suite (`./e2e.ps1 api run`).
5. **Then run the UI E2E regression check** (`e2e-ui-regress`, `./e2e.ps1 ui regress`). The UI suite drives the same backend through the browser; a backend event-flow fix should be confirmed there too. To debug both layers in one pass, use the `e2e-debug` skill.

## Useful filter patterns

| Goal | Filter |
|------|--------|
| Single test (substring) | `FullyQualifiedName~ShouldCreateDraft_WhenDoorSplitApplicationAccepted` |
| Whole test class | `FullyQualifiedName~ConcertFinishedTests` |
| All settlement/draft tests | `FullyQualifiedName~ConcertDraftTests` |
| Exact match | `FullyQualifiedName=Concertable.B2B.E2ETests.Payments.ConcertDraftTests.ShouldCreateDraft_WhenDoorSplitApplicationAccepted` |

## Notes

- These tests make **real Stripe test-mode calls** and use a **real ASB emulator** — they are not hermetic the way integration tests are. Flakiness is usually a too-tight `Polling` window on a genuinely-slow webhook, OR cross-suite contention (never run an API E2E and a UI E2E app at the same time — that's the `e2e_parallel_execution` failure root; the wrapper + `MaxCpuCount=1` serialize them).
- The HTTP client here is a plain `new HttpClient()` against the deployed URL — there is **no** per-test `ITestOutputHelper` server-log capture like integration tests have. Server-side detail comes from the **forwarded Aspire resource logs** in the console output instead.
- Seeding runs via `DevDbInitializer` (`IDevSeeder`, the dev/E2E path) — **not** `ITestSeeder`. If seed state is wrong, fix the dev seeders, and never seed event-sourced/read-model/payout rows directly (see `api/docs/SEEDING_CONVENTIONS.md`, memory `idevseder_not_itestseeder_for_e2e`).
- This suite has **no `E2E_BASELINE.md`** — that baseline is UI-only (Reqnroll DisplayNames). Every API E2E test is expected to pass; any failure is a regression, which is why `./e2e.ps1 api run` exits non-zero on failure.
- Integration (in-process, mocked) tests are a separate suite → `integration-debug`. Browser scenarios → `e2e-ui-debug`. Both layers at once → `e2e-debug`.
