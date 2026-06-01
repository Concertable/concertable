---
name: e2e-ui-debug
description: Run the full Concertable UI E2E suite (Reqnroll + Playwright, all 30 scenarios) and diagnose / fix failing ones using enriched HTTP + Playwright logs. Use whenever the user wants to debug a UI E2E failure, run the full suite, discover newly-passing or newly-failing scenarios, or investigate a flaky Reqnroll/Playwright scenario. For a "did I break anything?" check that fail-fasts on the expected-passing set, use the `e2e-ui-regress` skill instead (it runs whatever the baseline lists as passing — a fast subset when some are excluded as failing, up to the full suite when all pass).
---

# e2e-ui-debug

Run the Concertable UI E2E test suite and analyse any failures using the enriched HTTP + Playwright logs already baked into the test fixtures. Use this for full sweeps and deep-dive debugging; use `e2e-ui-regress` for fast no-regression checks.

## The point of this skill: run autonomously — FIX failing tests yourself, do not ask

When the user invokes this skill, they are delegating the **entire** run → diagnose → fix → verify loop to you, to run autonomously end to end. **Any failing scenario is something you fix in code yourself**, without stopping to ask permission, then re-run and keep going until the suite is green. Do not report findings and wait for a go-ahead; do not treat the `E2E_BASELINE.md` "failing" list or an "out of scope for this branch" note as a reason to leave a test broken — the baseline records the *current* state, it is not an instruction to skip fixing. Diagnose the root cause, write the code change (in the app, page objects, step definitions, or test support — wherever the real bug is), and re-run to confirm green. The only time you pause for the user is a genuine product-behaviour ambiguity you cannot resolve from the code (per the "Test vs prod code — ask first" convention). Otherwise: run the suite, fix every failure you can, verify, and report what you changed — all in one pass.

## Input

If the skill is invoked with arguments, treat them as the full scenario names as they appear in the test output (e.g. `"Venue manager accepts a venue hire application on a flat fee", "Customer purchases a ticket and completes 3DS challenge"`). Run Step 0, then skip Step 1 and go straight to Step 2, running each one individually using `DisplayName~` with the full name as the filter value.

If invoked with no arguments, run Step 0 then the full suite (Step 1) to discover which scenarios fail, then proceed to Step 2 for each failure.

## Headless vs headed — default to headless

**Always run headless** unless the user explicitly asks to watch the browser. Headless is faster and does not interfere with debugging: failure screenshots (`CaptureFailureAsync`), Playwright traces, and the enriched HTTP/console logs all work identically headless.

- `./e2e.ps1 <cmd>` (Step 1, Step 4 full-suite re-runs) is headless by default — it sets `HEADLESS=true` unless you pass `-Headed`.
- Direct `dotnet test` runs (Step 2 single-scenario deep-dives) do NOT pick up that default — the fixture runs **headed** with `SlowMo` unless `HEADLESS` is set. So always prefix Step 2 commands with `$env:HEADLESS='true'; ` (shown in Step 2 below).
- If (and only if) the user asks to watch the browser, run headed: pass `-Headed` to `./e2e.ps1`, or set `$env:HEADLESS='false'` (or omit it) for direct `dotnet test`.

## Key paths

**B2B UI tests** — `api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/`
- Feature files: `api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/Features/`
- Step definitions: `api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/Steps/`
- Page objects: `api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/PageObjects/`
- Fixtures/hooks: `api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/Fixtures/` and `.../Hooks/`
- Last run log: `api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/ui-tests.last.log`

**Customer UI tests** — `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests.Ui/`
- Feature files: `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests.Ui/Features/`
- Step definitions: `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests.Ui/Steps/`
- Page objects: `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests.Ui/PageObjects/`
- Hooks: `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests.Ui/Hooks/`
- Last run log: `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests.Ui/ui-tests.last.log`

- Full suite (both): `./e2e.ps1 run` (~25–30 min)
- **Regression check** (baseline-passing only): `./e2e.ps1 regress` (~3–6 min)
- B2B only: `./e2e.ps1 b2b`
- Customer only: `./e2e.ps1 customer`
- 3DS-only: `./e2e.ps1 3ds`
- Trace viewer: `./e2e.ps1 trace`

**Baseline file** (which scenarios are expected to pass vs fail): `api/Shared/Tests/Concertable.E2ETests/E2E_BASELINE.md`.

**Scratch run logs** — if you capture `dotnet test` output to a file for later grepping (retries, deep-dives, scenario reruns), write it under `api/Shared/Tests/Concertable.E2ETests/logs/` — **never the repo root**. Create the dir first if needed: `New-Item -ItemType Directory -Force api/Shared/Tests/Concertable.E2ETests/logs | Out-Null`. That folder is git-ignored. The canonical `ui-tests.last.log` / `regress.last.log` files written by `./e2e.ps1` stay in their project dirs (above) — leave those as-is.

## Which command to use

- **User wants to verify a code change hasn't broken anything → `./e2e.ps1 regress`.** It parses `E2E_BASELINE.md`, runs only the scenarios listed under the `passing` fenced blocks, and exits 1 if any of them fails or if any baseline name no longer matches a real test. Much faster than the full suite, and the only signal needed to confirm "no regression."
- **User wants to discover newly-passing or newly-failing scenarios, or you've just landed a real test fix → `./e2e.ps1 run`.** This runs all 30 scenarios.
- **After `./e2e.ps1 run` reveals a status change** (a scenario crossed the line), prompt the user to update `E2E_BASELINE.md`: move the scenario between the `passing` and `failing` fenced blocks and bump the `(N)` count in the heading. Both regress and PR review depend on this file being current.

## Step 0 — Pre-flight check

Before running anything, check Docker is up:

```powershell
docker ps 2>&1
```

If this errors or shows the daemon is not reachable, stop and tell the user: **"Docker is not running — please start Docker Desktop before running E2E tests."** Do not proceed.

Then tell the user: **"Starting full E2E suite — this takes ~7 minutes. I'll report back when done."**

## Step 0b — Watch for startup hangs

Run the test as a **background PowerShell task** (run_in_background: true on the PowerShell tool). Note the output file path from the task result.

Do NOT just launch and wait. After launching, poll the output file every ~60 seconds for the first 5 minutes using the **PowerShell tool** directly (NOT Monitor, NOT a background poller — just call it inline):

```powershell
# Replace <output-file> with the path from the background task result
$lines = Get-Content "<output-file>" 2>&1
Write-Host "Lines so far: $($lines.Count)"
$lines | Select-String "AppFixture|Running|Waiting|Exited|fail:|error:|Passed|Failed|healthy" | Select-Object -Last 20
```

Call this every ~60 seconds with a plain PowerShell tool call. Do this 3–4 times during the first 4 minutes of startup to confirm resources reach `Running`.

If after 2–3 minutes resources are still `unknown`/`Waiting` and none reach `Running`, diagnose immediately. Look for `Exited` resources — get their log lines from the output file:

```powershell
$lines = Get-Content "<output-file>" 2>&1
for ($i = 0; $i -lt $lines.Count - 1; $i++) {
    if ($lines[$i] -match "Resources\.<service-name>\[0\]") { $lines[$i+1] }
}
```

Also check Docker containers:
```powershell
docker ps -a --format "table {{.ID}}`t{{.Image}}`t{{.Status}}`t{{.Names}}" 2>&1
```

Common causes:
- **ASB emulator exits with code 139** — "At least one subscription required per topic" means a topic in `DistributedApplicationBuilderExtensions.cs` is declared but has no subscriptions for the current service flags. Fix: gate the topic creation on the subscriber flag, not the publisher flag.
- **Workers crashing with "address not configured"** — a project reference (e.g. `paymentWeb`) is missing from the AppHost's `AddWorkers(...)` call.
- **SQL container won't start** — port conflict or volume corruption; run `docker volume prune` and retry.
- **OOM** — container ran out of memory; increase Docker Desktop memory limit.

Fix the root cause before re-running the test. Do not keep waiting on a stuck startup.

## Step 1 — Run the full suite

```powershell
./e2e.ps1 run
```

Run headless (default). After it finishes, parse both `ui-tests.last.log` files to extract pass/fail counts and build a results summary table to present to the user before proceeding:

| # | Suite | Scenario | Result |
|---|-------|----------|--------|
| 1 | B2B | Scenario name here | ✅ Passed / ❌ Failed |
| 2 | Customer | Scenario name here | ✅ Passed / ❌ Failed |

Show totals: **X passed, Y failed** (or "All passed" / "All failed"). Then note which scenarios failed and proceed to Step 2 for each.

## Step 2 — Re-run each failing test individually for enriched output

First identify which project the failing scenario belongs to — B2B scenarios come from features like `ArtistSignUp`, `FlatFeeWorkflow`, `VenueHireWorkflow`, `DoorSplitWorkflow`, `VersusWorkflow`, `VenueSignUp`; Customer scenarios come from `CustomerSignUp`, `TicketPurchase`, `Login`.

For each failed scenario, run it alone using `--filter` so the verbose logs from the HTTP logger and Playwright page-error hooks aren't buried:

```powershell
# B2B scenario — run via PowerShell tool (not Bash; backtick continuation is PowerShell-only)
# $env:HEADLESS='true' keeps the direct dotnet test run headless (the fixture is headed by default).
$env:HEADLESS='true'; dotnet test 'api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/Concertable.B2B.E2ETests.Ui.csproj' --filter "DisplayName~<scenario name substring>" --logger "console;verbosity=normal"

# Customer scenario
$env:HEADLESS='true'; dotnet test 'api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests.Ui/Concertable.Customer.E2ETests.Ui.csproj' --filter "DisplayName~<scenario name substring>" --logger "console;verbosity=normal"
```

If you want to keep the output to grep later, tee it into the scratch logs dir (NOT the repo root):

```powershell
$logs = 'api/Shared/Tests/Concertable.E2ETests/logs'; New-Item -ItemType Directory -Force $logs | Out-Null
$env:HEADLESS='true'; dotnet test '<csproj>' --filter "DisplayName~<scenario>" --logger "console;verbosity=normal" | Tee-Object -FilePath "$logs/<scenario-slug>.log"
```

The test fixtures emit:
- **HTTP request/response logs** — every API call made during the scenario with status codes and bodies
- **Browser console errors** — JavaScript errors visible in the Playwright page
- **On-screen error text** — Playwright assertions capture visible error messages

Read `Standard Output Messages` in the test output for this enriched detail — it is far more informative than the stack trace alone.

## Step 3 — Diagnose from logs and screenshots

**MANDATORY FIRST CHECK — HTTP 4xx/5xx errors.** Before looking at screenshots, stack traces, or browser console logs, grep the test output for HTTP errors. These are almost always the root cause. Do not skip this step.

```powershell
# B2B log
Select-String -Path "api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/ui-tests.last.log" -Pattern "HTTP [45][0-9][0-9]" | Select-Object -First 50

# Customer log
Select-String -Path "api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests.Ui/ui-tests.last.log" -Pattern "HTTP [45][0-9][0-9]" | Select-Object -First 50

# Browser console warnings and on-screen errors (adjust path to the relevant log)
Select-String -Path "api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/ui-tests.last.log" -Pattern "\[console warn\]|On-screen error" -CaseSensitive:$false | Select-Object -First 50
```

**Important**: for the individual scenario re-run (Step 2), the enriched output is in the `dotnet test` console output directly — NOT in `ui-tests.last.log` (that file is only written by the `./e2e.ps1` wrapper). Search the console output captured during Step 2 instead.

Work through the enriched output in this order:

1. **HTTP 4xx/5xx calls** — which endpoint failed and what's in the response body?
2. **gRPC errors** — if B2B Web returns HTTP 500 with `Status(StatusCode=...)`, the actual exception is in the **Aspire service logs**, not the browser. Grep the test output for `Resources.payment-web` (or whichever service hosts the gRPC handler) `fail:`/`error:` lines to find the real stack trace and exception message.
3. **Browser console errors and warnings** — `[console warn]` and `[console error]` lines from the Playwright page console
4. **Visible page errors** — `On-screen error` lines from `CaptureFailureAsync`
5. **Stack trace** — only after the above; filter to `Concertable.*` frames

### Failure screenshots

On every scenario failure, `CaptureFailureAsync` saves a full-page screenshot to:

```
api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/bin/Debug/net10.0/playwright-failures/
api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests.Ui/bin/Debug/net10.0/playwright-failures/
```

The path is anchored to `AppContext.BaseDirectory` so it always lands in the build output folder regardless of the test runner's working directory. The log line `Failure screenshot: <full-path>/<name>-<timestamp>.png` in the test output gives you the exact filename. Read the image with the `Read` tool — it renders inline so you can see exactly what was on screen when the assertion timed out. Use this **after** confirming there are no HTTP/gRPC errors — screenshots show visual state (e.g. a disabled button, a missing element) but the HTTP/gRPC logs identify the root cause.

### When the logs don't pinpoint the cause — add tracing

If the HTTP/gRPC errors, console output, and screenshots still don't explain *why* (e.g. an endpoint 404s because a row is missing and you can't tell whether a projection handler ran, skipped, or failed), add `ILogger` tracing to the relevant server-side class rather than guessing. Read [`api/docs/DEBUGGING_CONVENTIONS.md`](../../../api/docs/DEBUGGING_CONVENTIONS.md) first and follow it: generic, future-useful logs (handler invoked/skipped/wrote, processor lifecycle) get promoted to the project's `Log.cs` with `[LoggerMessage]` source-gen and **kept permanently**; one-off probes stay inline and are removed once the bug is found. Then re-run the single scenario (Step 2) and read your new log lines from the Aspire service output.

## Step 4 — Fix and verify

After identifying the cause:
1. Make the fix in the relevant service/page object/step definition.
2. Re-run the specific scenario to confirm it goes green.
3. Re-run the full suite to confirm no regressions.

## Useful filter patterns

| Scenario | Suite | Filter |
|----------|-------|--------|
| Single scenario by name | B2B | `DisplayName~"books artist on a flat fee"` |
| All 3DS scenarios | B2B | `DisplayName~3DS` |
| All flat-fee scenarios | B2B | `DisplayName~"flat fee"` |
| All venue hire scenarios | B2B | `DisplayName~"venue hire"` |
| All ticket purchase scenarios | Customer | `DisplayName~"purchase"` |
| Customer sign-up | Customer | `DisplayName~"CustomerSignUp"` |

## Notes

- Tests run headless by default (`HEADLESS=true`). Pass `-Headed` to `./e2e.ps1` to watch the browser.
- Each suite writes its own `ui-tests.last.log` — useful for re-reading without re-running.
- B2B features: `ArtistSignUp`, `DoorSplitWorkflow`, `FlatFeeWorkflow`, `Login`, `VenueHireWorkflow`, `VenueSignUp`, `VersusWorkflow`
- Customer features: `CustomerSignUp`, `Login`, `TicketPurchase`
- Integration (non-UI) E2E tests live in sibling projects; this skill covers the Reqnroll+Playwright suites only.
