---
name: e2e-ui-regress
description: Run the Concertable UI E2E regression check -- only the scenarios listed under `passing` in `api/Tests/Concertable.E2ETests/E2E_BASELINE.md`. ~3-6 min, fail-fast on any regression. Use whenever the user wants to verify a code change hasn't broken anything ("regress", "check for regression", "verify no regression", "did I break anything", "is it still green", "run the regress"). Use the heavier `e2e-ui-debug` skill instead when the user wants the full 30-scenario suite or wants to discover newly-passing scenarios.
---

# e2e-ui-regress

Fast confidence check that a code change hasn't regressed any baseline-passing UI E2E scenario. Runs ONLY the scenarios under `### B2B passing` and `### Customer passing` in `api/Tests/Concertable.E2ETests/E2E_BASELINE.md`. Does not re-run failing scenarios (they're tracked but expected to stay broken until separately fixed).

## When to use this skill

- User asks to "check regression", "regress the tests", "verify no regression", "is it still green", "make sure I haven't broken anything"
- User has just made a code change and wants fast confidence
- Pre-commit / pre-PR check on a feature branch

## When NOT to use this skill

- User wants the full 30-scenario sweep -> use the **`e2e-ui-debug`** skill (`./e2e.ps1 run`, ~25-30 min)
- User wants to discover scenarios that NEWLY pass (because of a real fix) -> also `e2e-ui-debug`
- User wants to debug a specific failing scenario -> also `e2e-ui-debug` (it has Step 2 for per-scenario re-runs with enriched logs)

## Key paths

- Baseline file: `api/Tests/Concertable.E2ETests/E2E_BASELINE.md`
- Script: `./e2e.ps1 regress` (PowerShell)
- B2B run log: `api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests.Ui/regress.last.log`
- Customer run log: `api/Concertable.Customer/Tests/E2ETests/Concertable.Customer.E2ETests.Ui/regress.last.log`

## Step 0 -- Pre-flight

Check Docker is running:

```powershell
docker ps 2>&1
```

If the daemon isn't reachable, stop and tell the user: **"Docker is not running -- please start Docker Desktop before running E2E."**

Then tell the user: **"Starting regression check -- ~3-6 minutes. Will report verdict."**

## Step 1 -- Run regress in background

Run `./e2e.ps1 regress` as a **background PowerShell task** (`run_in_background: true`). Capture the output file path.

The script:
1. Parses the `### B2B passing (N)` and `### Customer passing (N)` fenced text blocks in `E2E_BASELINE.md`
2. Builds a `dotnet test --filter "DisplayName=X|DisplayName=Y|..."` from those exact names
3. Preflights via `dotnet test --list-tests --filter ...` to confirm every baseline name resolves to a real Reqnroll DisplayName -- fails fast on baseline drift
4. Runs the filtered scenarios
5. Asserts `Passed == expected count, Failed == 0`
6. Exits 0 on success, 1 on any regression or baseline drift

## Step 2 -- Monitor with the Monitor tool

Watch for the final verdict. The script always prints exactly one of these terminal lines:

```
REGRESS PASSED -- every baseline-passing scenario still passes.
REGRESS FAILED -- at least one baseline-passing scenario regressed.
BASELINE DRIFT: ...
BASELINE FORMAT ERROR: ...
```

Arm a Monitor that grep-watches the background task output file for any of those, e.g.:

```bash
until grep -aE 'REGRESS PASSED|REGRESS FAILED|REGRESSED:|Baseline drift|BASELINE FORMAT ERROR|Unhandled exception' <output-file> > /dev/null 2>&1; do sleep 10; done
grep -aE 'REGRESS PASSED|REGRESS FAILED|REGRESSED:|OK:|Baseline drift|BASELINE FORMAT ERROR|Total tests:|Passed:|Failed:' <output-file> | tail -20
```

Timeout 15 minutes (`timeout_ms: 900000`). Re-arm if it times out and the test is still genuinely running.

## Step 3a -- On PASS

Report concisely:

> Regress passed: all N baseline-passing scenarios still pass.
>   - B2B: X/X passed
>   - Customer: Y/Y passed

Done. No follow-up unless the user asks for the full suite.

## Step 3b -- On FAIL -- name the regressed scenarios

Pull the `Failing scenarios:` block the script prints. It's a list of scenario names that were in the `passing` baseline but failed this run. Present them:

> Regress failed: N scenarios that previously passed now fail:
>   - <scenario name 1>
>   - <scenario name 2>

For each, identify the suite (B2B / Customer) and offer to:
- Re-run that one scenario with verbose logs via `dotnet test ... --filter "DisplayName=<name>" --logger "console;verbosity=detailed"` (the `e2e-ui-debug` skill has the Step 2 / Step 3 / Step 4 diagnostic flow for this)
- OR investigate the most recent code change that touched related files (`git log --since="1 hour ago" --stat`)

Do NOT update `E2E_BASELINE.md` to move regressed scenarios from `passing` to `failing` -- that masks the regression. The baseline reflects the **expected** state; if a scenario regressed, the fix is to fix the code, not the baseline.

## Step 3c -- On BASELINE DRIFT

The script prints:

```
BASELINE DRIFT: dotnet test --list-tests discovered X of Y expected scenarios.
Missing (renamed in .feature, or typo'd in baseline?):
  - <scenario name>
```

This means a scenario name in `E2E_BASELINE.md` no longer matches any Reqnroll DisplayName. Two causes:

1. **Scenario was renamed in the `.feature` file.** Find the new name in `api/Concertable.<suite>/Tests/E2ETests/Concertable.<suite>.E2ETests.Ui/Features/*.feature`, update the baseline entry to match. Then re-run regress.
2. **Scenario was deleted.** Remove the entry from the baseline `passing` or `failing` block, decrement the `(N)` count in the heading, update the summary table. Then re-run.

Show the user both possibilities and ask which applies before editing the baseline.

## Step 3d -- On BASELINE FORMAT ERROR

The parser found a structural issue in the baseline file (e.g. heading count doesn't match line count in fenced block, missing sentinel, bullet in scenario list). The error message names exactly what's wrong. Read it to the user and fix the file to match the format rules at the top of `E2E_BASELINE.md`.

## Updating the baseline (the OTHER skill's job)

If after `./e2e.ps1 run` (full suite, run by the `e2e-ui-debug` skill) the user has a scenario that newly passes or newly fails, they manually edit `E2E_BASELINE.md`:

- Move the scenario between the relevant `passing` / `failing` fenced blocks
- Update the `(N)` counts in both affected headings
- Update the summary table
- Update the "Last reconciled" line

This skill (`e2e-ui-regress`) only **verifies** the baseline, never modifies it.

## Cost

Each regress run takes ~3-6 minutes wall-clock (faster on a warm Aspire cache, slower on cold start). The check is cheap enough to run before every commit on a feature branch; do not skip it just because the code change "looks safe."
