---
name: e2e-debug
description: Run and debug BOTH Concertable E2E layers end to end — the API E2E suite (xUnit, full Aspire stack, no browser) and the UI E2E suite (Reqnroll + Playwright) — diagnosing and fixing every failure across both, lowest layer first. Use whenever the user wants a complete E2E sweep ("run all the E2E tests", "debug E2E", "are the E2E tests green", "full E2E pass"), or when a UI scenario fails and you want to confirm whether the root cause is actually in the backend event flow. Runs the API layer first (CI gates the UI job on it), so backend bugs are found and fixed before the slower browser suite runs. For just one layer, use `e2e-api-debug` or `e2e-ui-debug`; for a fast no-regression check use `e2e-ui-regress`.
---

# e2e-debug

The umbrella E2E debugging skill. Concertable has **two** full-stack E2E suites over the same backend:

1. **API E2E** (xUnit + Aspire `DistributedApplication`, no browser) — drives services over HTTP, polls real DB + Stripe state until the async event chain settles. Proves the **service-layer flows**. → mechanics in **`e2e-api-debug`**.
2. **UI E2E** (Reqnroll + Playwright, real browser) — drives the SPAs, proves the **user-facing flows** on top of that same backend. → mechanics in **`e2e-ui-debug`**.

This skill runs and fixes **both**, in the order that makes debugging cheapest: **API first, then UI.** That mirrors CI (`e2e-ui-tests` `needs: e2e-api-tests`) and reflects the dependency — the UI suite exercises the same services the API suite does, plus a browser. A broken backend event flow fails *both* suites, but it's far faster to diagnose at the API layer (resource logs + DB/Stripe polling) than through a browser timeout. So: get API green first, then UI failures are genuinely UI-layer (selectors, navigation, render, SPA wiring), not backend.

## The point of this skill: run autonomously — FIX every failure yourself across both layers

The user is delegating the entire run → diagnose → fix → verify loop for both suites. Fix each failure in code and re-run only that failing test in isolation until it is green. Pause only for a genuine product-behaviour ambiguity that the code cannot resolve.

## NEVER disable or bypass a step to get past its failure

Same hard rule as both sub-skills. "Fix" means make the failing step work, never make it stop running. **No** `--no-build`/`SuppressBuild` for a slow/hung build; **no** raising `Polling`/`WaitFor`/Playwright timeouts to make a flaky wait pass (not for a "hang", "slow webhook", or "CI load" — a blown bound is a real failure to fix (or, for a *proven* UI flake, quarantine into the non-gating lane), never a number to widen); **no** disabling a resource/handler/scenario to go green. If something hangs with no output, reproduce and observe it live (resource states, Docker, process trees) — don't remove it. Bypass only on explicit user request after they've seen the diagnosis.

## Input

- **No arguments and no failures already reported by CI or the merge queue** — run one full two-layer discovery sweep. After fixing failures, verify only those tests individually.
- **`api` or `ui`** — run only that layer (just defer to that sub-skill; this skill adds nothing over running it directly, but it's a convenient entry point).
- **A specific test / scenario name** — identify its layer (xUnit `FullyQualifiedName` → API; Reqnroll DisplayName → UI), then follow that sub-skill's single-test path.

## Step 0 — Pre-flight (shared)

Before any report or stop, including an environment or layer failure, if this workflow is
plan-managed, read and apply
[the shared plan-progress checkpoint](../resume-plan/references/plan-progress-checkpoint.md).

Both suites need Docker (SQL containers, ASB emulator, stripe-cli) and the Stripe/Google secrets (`Stripe__SecretKey`, `GoogleApiKey`).

**`docker ps` answering is NOT proof Docker is healthy** — why the cheap checks miss a half-started engine, and the `pre-login handshake` signature it produces, are in the `remote-validation` skill. Run the real gate:

```powershell
./scripts/docker-health.ps1   # fresh container + published port + real HTTP round-trip + stability check; exit 1 = unhealthy
```

`./scripts/e2e.ps1 api|ui ...` runs this gate automatically and refuses to boot on failure — but run it yourself first so you catch a bad engine before anything else. If it reports unhealthy, **STOP**: tell the user Docker is half-started/down and to wait for Docker Desktop to show **Running**, then retry. Do **not** rerun the suite or debug application code for this — it is an environment failure. If a run dies instantly with a Stripe-auth / missing-config error, confirm the secrets are set before debugging anything else.

Tell the user the exact targeted test(s) being run. Use the full two-layer cost estimate only when the user explicitly requested a discovery sweep and no failures are already known.

The two E2E apps must **never** run concurrently (the `e2e_parallel_execution` failure root: Vite starvation + dual stripe-cli on one Stripe account). Always run the two layers **sequentially** — which is exactly what doing API-then-UI gives you. Don't kick both off at once.

## Step 1 — API E2E layer (do this FIRST)

Run and fix the API suite to green using the full **`e2e-api-debug`** flow:

```powershell
./scripts/e2e.ps1 api run        # both services; exits non-zero on any failure
```

- Watch startup for hangs (Step 0b in `e2e-api-debug` — ASB emulator exit 139, payout-account stall, etc.).
- For each failure, re-run the single test with `--filter "FullyQualifiedName~<test>"` and diagnose by failure shape: synchronous `ShouldBe` body, `Polling.UntilAsync` timeout → **forwarded Aspire resource logs** (`Resources.payment-web` etc.), or Stripe value mismatch. Full mechanics: **`e2e-api-debug`** Steps 2–3.
- Fix the root cause (service / handler / dispatcher / fixture), then re-run only the failing test until green.

**Do not start the UI layer until the API layer is green.** A backend flow that's red here will also fail the corresponding UI scenario, and you'd be debugging it the slow way. Getting API green first means any remaining UI failure is real UI-layer work.

If the user scoped to `ui` only, skip this step.

## Step 2 — UI E2E layer (after API is green)

Run and fix the UI suite to green using the full **`e2e-ui-debug`** flow:

```powershell
./scripts/e2e.ps1 ui run         # all 30 Reqnroll scenarios, headless
```

- Watch startup (same Aspire AppHost, same startup-hang playbook).
- For each failed scenario, re-run it alone with `--filter "DisplayName~<scenario>"`, and diagnose **HTTP 4xx/5xx first**, then gRPC (callee resource log), then browser console / on-screen errors, then the failure screenshot. Full mechanics: **`e2e-ui-debug`** Steps 2–3.
- Fix the real bug (service, page object, step def, or test support), then re-run only the failing scenario until green.
- If a scenario **crossed the line** (newly passes or newly fails vs `api/Concertable.Shared/tests/Concertable.Testing.E2E/E2E_BASELINE.md`), prompt the user to update the baseline (move it between `passing`/`failing`, bump the `(N)` counts, update the summary table). The UI baseline is the only baseline — the API suite has none.

If the user scoped to `api` only, skip this step.

## Step 3 — Final verdict

Report per layer, concisely:

```
API E2E:  X/X passed   (fixed: <one-line per fix>)
UI  E2E:  Y/Y passed   (fixed: <one-line per fix>)
```

Once every originally failing test is green in isolation, return to the PR merge workflow without another local suite run; the merge queue verifies both layers once. If anything remains red because it needs a product decision, state exactly what is needed.

## Why API-first matters (keep this discipline)

- **Cheaper signal.** An API failure points at a resource log + DB/Stripe state in seconds; the same broken flow in the UI suite is a 60s+ browser timeout with a screenshot of a spinner.
- **No double-debugging.** Fix the event chain once at the API layer and the UI scenario that depended on it usually goes green for free.
- **Honest UI failures.** Once API is green, a remaining UI red is genuinely the browser layer — selectors, `GotoSpaAsync` navigation, render, SPA port/env wiring — not a backend red wearing a browser costume.

## Relationship to the other skills

| Want | Skill |
|---|---|
| Both layers, full sweep + fix | **`e2e-debug`** (this) |
| Service-layer flows only (xUnit + Aspire, no browser) | `e2e-api-debug` |
| Browser scenarios only (Reqnroll + Playwright) | `e2e-ui-debug` |
| Fast "did I break anything?" on the UI passing-baseline | `e2e-ui-regress` |
| In-process module tests (WAF, mocked externals) | `integration-debug` |
