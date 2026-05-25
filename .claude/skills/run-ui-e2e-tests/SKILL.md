---
name: run-ui-e2e-tests
description: Run the Concertable UI E2E tests (Reqnroll + Playwright), surface failures with enriched logs, then diagnose and fix failing scenarios. Use this skill whenever the user asks to run UI/E2E tests, check E2E failures, or debug Playwright/Reqnroll scenarios.
---

# run-ui-e2e-tests

Run the Concertable UI E2E test suite and analyse any failures using the enriched HTTP + Playwright logs already baked into the test fixtures.

## Key paths

- Script: `./e2e.ps1 run` (wraps `api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/run-ui-tests.ps1`)
- 3DS-only script: `./e2e.ps1 3ds` (wraps `api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/Run-3dsTests.ps1`)
- Trace viewer: `./e2e.ps1 trace`
- Last run log: `api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/ui-tests.last.log`
- Feature files: `api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/Features/`
- Step definitions: `api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/Steps/`
- Page objects: `api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/PageObjects/`
- Fixtures/hooks: `api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/Fixtures/` and `Hooks/`

## Step 1 — Run the full suite

```powershell
./e2e.ps1 run
```

Run headless (default). The script prints a colour-coded results table and a grouped failure summary. Note which scenarios failed.

## Step 2 — Re-run each failing test individually for enriched output

For each failed scenario, run it alone using `--filter` so the verbose logs from the HTTP logger and Playwright page-error hooks aren't buried:

```powershell
dotnet test 'api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/Concertable.E2ETests.Ui.csproj' `
    --filter "DisplayName~<scenario name substring>" `
    --logger "console;verbosity=normal"
```

The test fixtures emit:
- **HTTP request/response logs** — every API call made during the scenario with status codes and bodies
- **Browser console errors** — JavaScript errors visible in the Playwright page
- **On-screen error text** — Playwright assertions capture visible error messages

Read `Standard Output Messages` in the test output for this enriched detail — it is far more informative than the stack trace alone.

## Step 3 — Diagnose from logs and screenshots

Work through the enriched output in this order:

1. **HTTP 4xx/5xx calls** — which API endpoint failed and with what response body?
2. **Browser console errors** — unhandled promise rejections, network errors, JS exceptions
3. **Visible page errors** — what error text was visible on screen when the assertion failed?
4. **Stack trace** — only after the above; filter to `Concertable.*` frames

### Failure screenshots

On every scenario failure, `CaptureFailureAsync` saves a full-page screenshot to:

```
api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/bin/Debug/net10.0/playwright-failures/
```

The log line `Failure screenshot: playwright-failures/<name>-<timestamp>.png` in the test output gives you the exact filename. Read the image with the `Read` tool — it renders inline so you can see exactly what was on screen when the assertion timed out. Use this when the HTTP and console logs are not enough to understand the visual state (e.g. a page crash error boundary, a disabled button, a missing element).

The logs are usually sufficient to identify the root cause without needing to add extra instrumentation.

## Step 4 — Fix and verify

After identifying the cause:
1. Make the fix in the relevant service/page object/step definition.
2. Re-run the specific scenario to confirm it goes green.
3. Re-run the full suite to confirm no regressions.

## Useful filter patterns

| Scenario | Filter |
|----------|--------|
| Single scenario by name | `DisplayName~"books artist on a flat fee"` |
| All 3DS scenarios | `DisplayName~3DS` |
| 3DS success only | `DisplayName~"completes 3DS challenge"` |
| All flat-fee scenarios | `DisplayName~"flat fee"` |
| All ticket purchase scenarios | `DisplayName~"purchase"` |

## When to use `./e2e.ps1 3ds` instead

The `3ds` command is a convenience wrapper for 3DS-specific scenarios with TRX output. Use it when working specifically on 3DS authentication flows — it produces a `.trx` file in `TestResults/` that can be parsed for structured failure data.

## Notes

- Tests run headless by default. Pass `-Headed` to `run-ui-tests.ps1` to watch the browser.
- The `ui-tests.last.log` file always contains the full output from the most recent run — useful for re-reading without re-running.
- Integration (non-UI) E2E tests live in a sibling project; this skill covers the Reqnroll+Playwright suite only.
