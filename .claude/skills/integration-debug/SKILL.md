---
name: integration-debug
description: Run the Concertable integration test suite (xUnit + WebApplicationFactory + Testcontainers SqlFixture, across B2B / Customer / Search) and diagnose any failures using per-test server-side `ILogger` output, `ShouldBe(HttpStatusCode)` failure messages (URL + status + response body), captured mock state (notifications, emails, Stripe), and stack traces. Use whenever the user wants to debug an integration test failure, run the full integration suite, investigate a flaky module-level test, or rerun a specific module's integration tests. For UI / Playwright Reqnroll scenarios use `e2e-ui-debug` instead -- those are a separate suite.
---

# integration-debug

Run the Concertable xUnit integration test suite and analyse failures using the WebApplicationFactory-driven test output, mock state captured in the `ApiFixture`, and HTTP responses asserted in test bodies. Each microservice (B2B, Customer, Search) owns its own integration test projects (one per module).

## Input

If invoked with arguments, treat them as either:
- A **fully-qualified test name** (e.g. `Concertable.B2B.Artist.IntegrationTests.ArtistApiTests.Create_Returns_201`) -- run Step 0 then jump to Step 2 for that single test.
- A **module name** (e.g. `concert`, `artist`, `venue`) -- run Step 0 then `./integration.ps1 <module>`.

If invoked with no arguments, run Step 0 then the full suite (Step 1), then Step 2 for each failure.

## Key paths

**Wrapper script** -- `integration.ps1` at the repo root. Commands: `run | b2b | customer | search | <module> | list`. Each project writes its own `integration-tests.last.log`.

**B2B integration tests** -- `api/Concertable.B2B/src/Modules/<Module>/Tests/Concertable.B2B.<Module>.IntegrationTests/`
- Modules: `Artist`, `Concert`, `Organization`, `User`, `Venue`
- Shared fixture: `api/Concertable.B2B/tests/Concertable.B2B.IntegrationTests.Fixtures/ApiFixture.cs`
- Mocks (Stripe, notification, email, geocoding, image, bus transport) live under that Fixtures project's `Mocks/` folder
- Last run log per project: `<project>/integration-tests.last.log`

**Customer integration tests** -- `api/Concertable.Customer/src/Modules/<Module>/Tests/Concertable.Customer.<Module>.IntegrationTests/`
- Modules: `Concert`, `Review`, `Ticket`, `User`
- Shared fixture: `api/Concertable.Customer/tests/Concertable.Customer.IntegrationTests.Fixtures/`

**Search integration tests** -- `api/Concertable.Search/tests/Concertable.Search.IntegrationTests/`
- Shared fixture: `api/Concertable.Search/tests/Concertable.Search.IntegrationTests.Fixtures/`

**Shared test infra** (used by every service's fixture) -- `api/Concertable.Shared/tests/Concertable.Testing.Integration/`
- `SqlFixture` -- Testcontainers MsSql + Respawn
- `TestAuthHandler` -- header-driven auth
- `MockBusTransport`, `MockEmailSender`, `MockGeocodingService`, `MockImageService`
- `IResettable` -- mocks that flush between tests

## Step 0 -- Pre-flight check

Integration tests use Testcontainers for SQL Server, so Docker must be running:

```powershell
docker ps 2>&1
```

If this errors or the daemon is unreachable, stop and tell the user: **"Docker is not running -- please start Docker Desktop before running integration tests."** Do not proceed.

Then tell the user: **"Starting full integration suite -- this takes a few minutes (10 csproj's, each spins up a Testcontainers SQL container). I'll report back when done."**

## Step 0b -- Watch for startup hangs

Run as a **background PowerShell task** (`run_in_background: true` on the PowerShell tool). Note the output file path from the task result.

Do NOT just launch and wait. After launching, poll the output file every ~60 seconds for the first few minutes using the **PowerShell tool** directly (NOT Monitor, NOT a background poller -- inline calls):

```powershell
$lines = Get-Content "<output-file>" 2>&1
Write-Host "Lines so far: $($lines.Count)"
$lines | Select-String "=== |Passed!|Failed!|error|fail:|Test Run|MsSql|Container started|timed out" | Select-Object -Last 20
```

Common startup issues:
- **Testcontainers can't reach Docker** -- "Docker daemon is not running" or unix socket errors. Fix Docker Desktop, retry.
- **Port already in use** -- another test run's MsSql container didn't clean up. Run `docker ps -a` then `docker rm -f $(docker ps -aq --filter "ancestor=mcr.microsoft.com/mssql/server")`.
- **Image pull on first run** -- the MsSql image is ~1.5 GB; first run can take several minutes before any test actually executes. Be patient on a fresh machine.
- **OOM** -- if Docker Desktop is low on memory, containers exit before tests run. Bump the memory limit and retry.

Do not keep waiting on a stuck startup. Diagnose, fix, re-run.

## Step 1 -- Run the suite

Pick the narrowest scope the user asked for:

```powershell
# Everything
./integration.ps1 run

# One service
./integration.ps1 b2b
./integration.ps1 customer
./integration.ps1 search

# One module (matches any service that has it -- e.g. 'concert' hits both B2B and Customer)
./integration.ps1 concert
```

The script runs each csproj in turn and writes a per-project `integration-tests.last.log`. After it finishes, parse each log to extract pass/fail counts and build a summary table to present before proceeding:

| # | Service | Module | Passed | Failed | Skipped | Result |
|---|---------|--------|--------|--------|---------|--------|
| 1 | B2B | Artist | 24 | 0 | 0 | OK |
| 2 | B2B | Concert | 31 | 2 | 0 | FAIL |
| ... | | | | | | |

Show totals: **X passed, Y failed across N projects**. Then note which test cases failed and proceed to Step 2.

## Step 2 -- Re-run each failing test individually for enriched output

Once you know which fully-qualified test names failed (the `Failed!` lines in the per-project log include the FQN), re-run each one alone via `--filter` so the assertion message and any captured mock state aren't buried under thousands of other tests:

```powershell
dotnet test 'api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.IntegrationTests/Concertable.B2B.Concert.IntegrationTests.csproj' --filter "FullyQualifiedName~ApplicationFlatFeeApiTests.Accept_Returns_400_When_Already_Accepted" --logger "console;verbosity=detailed"
```

`FullyQualifiedName~` does substring match, which is the friendliest for nested classes / theory rows. Use `=` for exact match. For an entire test class, drop the method name: `FullyQualifiedName~ApplicationFlatFeeApiTests`.

**Always use the PowerShell tool, not Bash, for `dotnet test`** -- per project convention, backtick continuation is PowerShell-only and Bash mangles quoted filters.

## Step 3 -- Diagnose from logs

For a failing test, xUnit renders three things together in the failure block:

1. **Assertion / exception message** -- at the top. For HTTP status mismatches this is the `ShouldBe(...)` failure (see "HTTP failure format" below). For other assertions it's the standard xUnit message.
2. **`Output:` / `Standard Output Messages` block** -- every `ILogger.Log*` call the API made during this test. Piped via `XunitLoggerProvider` in `Concertable.Testing.Integration.Logging`. Passing tests hide this block; failing tests render it.
3. **Stack trace** -- only useful if the test itself threw (rather than asserted).

Read in that order.

### HTTP failure format

Every HTTP status assertion in integration tests goes through `response.ShouldBe(HttpStatusCode.X)` (see `HttpResponseAssertions.cs`). The standardised failure message looks like:

```
Expected 201 Created, got 400 BadRequest.
Request: POST http://localhost/api/Application/3/accept
Body:
{"errors":{"PaymentMethodId":["The PaymentMethodId field is required."]}}
```

URL, status, request method, and response body always present -- you should not need to add `Console.WriteLine` or extra logging to debug a wrong-status failure. If the body is a `ProblemDetails` / validation envelope it'll be right there.

### Cross-referencing with the server log block

When the assertion says "got 400" but the body alone doesn't tell you *why*, scan the server log block for the matching request. Look for:

- `[warn]` / `[fail]` lines from `Concertable.*` namespaces -- application code logging a guard / validation failure
- `Microsoft.AspNetCore.Mvc.Infrastructure` lines around the same request -- model state failures, action filter rejections
- EF Core `[warn]` lines -- concurrency conflicts, missing entities

### Missing side-effect assertions

When a test asserts something *happened* (`Assert.Single(fixture.EmailSender.Sent)`) and the mock list is empty, the production code didn't fire the event at all. Look at:

1. **The relevant mock list itself.** Which mock owns this side-effect?
   - `fixture.EmailSender.Sent` -- captured emails (shared `MockEmailSender`)
   - `fixture.NotificationService.DraftCreated` / `.Other` -- captured notifications (B2B `MockNotificationClient`; `DraftCreated` is special-cased, everything else lands in `Other`)
   - `fixture.NotificationClient.*` -- Customer-side equivalent
   - `fixture.StripeApiClient.LastPaymentIntentId` -- last Stripe intent created (note: only the most-recent is remembered)
2. **The server log block.** If the handler that's supposed to fire the side-effect logged nothing, it wasn't invoked. Trace backwards: is the event being raised? Handler registered? Bus transport being flushed?

### If the test threw

Stack trace is in the failure block. Filter to `Concertable.*` frames -- application code path. The frame above the throw is where the bug lives.

### Database state

Tests share a single Testcontainers SQL Server per `ApiFixture`; Respawn resets between tests. If seed data is missing, either the seeder isn't registered (check `services.AddXTestSeeder()` in the fixture) or Respawn ran after seeding.

To inspect mid-test:

```csharp
using var scope = fixture.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<XDbContext>();
var rows = await db.Set<XEntity>().ToListAsync();
```

### Cross-context FK gotcha

If you see an FK violation referencing a table from a different module/context, check that the principal table's migration runs before the dependent's. Per project convention the cross-context FK is stripped from the dependent's migration -- look for a missing `[ForeignKey]` strip. See memory `cross_context_fk`.

## Step 4 -- Fix and verify

After identifying the cause:
1. Make the fix (application code, fixture setup, or test).
2. Re-run the specific test with `--filter "FullyQualifiedName=<FQN>"` to confirm green.
3. Re-run the whole module's integration project (`./integration.ps1 <module>`) to catch regressions in sibling tests.
4. If the change is broader (Kernel, shared infra, fixture, mock), re-run the full suite (`./integration.ps1 run`).
5. **Run the UI E2E regression check** (`./e2e.ps1 ui regress`, ~3-6 min). Integration tests live in-process with mocked external services; the regression check exercises the full Aspire stack with real bus + Stripe CLI + browser. A fix that's green at the integration layer can still break the UI E2E baseline (mock vs real bus behaviour, controller / service / SignalR interactions, request shape changes). Required before considering any non-trivial change done. Skill to invoke: `e2e-ui-regress`.

## Useful filter patterns

| Goal | Filter |
|------|--------|
| Single test (exact) | `FullyQualifiedName=Concertable.B2B.Artist.IntegrationTests.ArtistApiTests.Create_Returns_201` |
| Single test (substring) | `FullyQualifiedName~Create_Returns_201` |
| Whole test class | `FullyQualifiedName~ArtistApiTests` |
| All integration tests in a project | `Category=Integration` (every integration project has `[assembly: AssemblyTrait("Category", "Integration")]`) |
| Skip a flaky test temporarily | `FullyQualifiedName!~FlakyTestName` |

## Conventions that affect how failures read

- **All HTTP status checks go through `response.ShouldBe(HttpStatusCode.X)`** -- defined in `api/Concertable.Shared/tests/Concertable.Testing/HttpResponseAssertions.cs`. Throws `XunitException` with `URL + status + body` in the message. The codebase has no `EnsureSuccessStatusCode` / `Assert.Equal(HttpStatusCode.*)` / `Assert.True(IsSuccessStatusCode)` in tests -- if you see any during debugging, treat it as a regression and switch to `ShouldBe`.
- **All HTTP calls return `HttpResponseMessage`** (no typed `GetAsync<T>` helper). The canonical pattern for "fetch a JSON resource" is three lines:
  ```csharp
  var response = await client.GetAsync(url);
  await response.ShouldBe(HttpStatusCode.OK);
  var application = await response.Content.ReadAsync<ApplicationResponse>();
  ```
  `Content.ReadAsync<T>` is defined in `HttpClientExtensions.cs`; it's deserialization only, with no implicit status check.
- **Response variable naming:** sole response in a test = `var response`. Multiple responses in one test = qualify each (`acceptResponse`, `applicationResponse`, `concertResponse`). Same rule applies inside polling lambdas.
- **Client variable naming:** sole client in a test = `var client` (regardless of role). Multiple clients in one test = qualify the specialised one (`adminClient`, `artistClient`) and leave the default-shape one as `client`. Never inline `fixture.CreateClient()` mid-test -- always extract in Arrange.

## Notes

- **Server-side `ILogger` output is captured per-test.** Every test class constructor takes `(ApiFixture fixture, ITestOutputHelper output)` and calls `fixture.AttachOutput(output)`; the host's `ILoggingBuilder` is wired (in each `ApiFixture.ConfigureTestServices`) to `Concertable.Testing.Integration.Logging.XunitLoggerProvider`, which routes log lines into xUnit's per-test buffer. Minimum level is `Information`. xUnit only renders the block on failure, so passing tests stay quiet. If you add a new integration test class, copy the two-arg constructor pattern -- forgetting it just means that test sees no server logs on failure (everything else still works).
- Tests use a real SQL Server (Testcontainers), not in-memory -- side-effects, FKs, and triggers behave realistically.
- All external integrations are mocked (Stripe, ASB bus, email, geocoding, image upload, notification). Tests do **not** make real network calls.
- Each project boots its own `WebApplicationFactory<Program>` -- expensive (~5-10s per project for warm-up). Running everything sequentially is intentional; parallel collection-level execution per project is fine but cross-project parallelism would race on the Testcontainers daemon.
- B2B Modules: Artist, Concert, Organization, User, Venue
- Customer Modules: Concert, Review, Ticket, User
- Search has one combined integration test project (no per-module split).
- UI / Playwright Reqnroll suites are separate -- this skill does NOT cover them. Use `e2e-ui-debug` / `e2e-ui-regress` for those.
- Workers / unit tests are separate -- run those with `dotnet test` directly against the relevant `*.UnitTests.csproj`.
