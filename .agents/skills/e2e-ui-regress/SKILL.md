---
name: e2e-ui-regress
description: Run the browser regression check — only the scenarios the baseline lists as passing, fail-fast — and report one verdict. Covers the baseline's three parseable properties and why a violation is a format error rather than a test failure, the isolated single retry of just the failures that separates a loaded-host blip from a real regression, the four terminal lines the entrypoint must print and how to wait for one with a capped echoing loop rather than a detached watcher, the opposite fixes for a renamed versus a deleted scenario when a name drifts, and the standing ban on moving a regressed scenario into the failing block. Use when the user wants to check for a regression, verify nothing is broken, confirm it is still green, or run the regress — and prefer e2e-ui-debug when they want the full sweep, newly-passing scenarios, or a specific failure diagnosed.
domain: process
---

# The regression check against the passing baseline

A confidence check that a change has not broken any browser scenario that was already passing. It runs the
scenarios the baseline lists as passing, skips the ones it lists as failing, and fail-fasts. Its duration
scales with the passing set: a fast subset while some scenarios are excluded, the whole suite once everything
passes.

Use it when the user asks to check for a regression, verify nothing is broken, confirm it is still green, or
"run the regress". Use [`e2e-ui-debug`](../e2e-ui-debug/SKILL.md) instead when they want the full sweep, want to discover
scenarios that *newly* pass, or want a specific failure diagnosed — that doc owns per-scenario re-runs and
the whole diagnostic flow.

## The baseline is the contract

One file lists, per suite, the scenarios expected to pass and the scenarios tracked as failing. The
entrypoint's regress command parses it and trusts it; the file's own editing rules sit at its top. Three
properties make it parseable, and a violation of any of them is a format error rather than a test failure:

- Each block is introduced by a heading carrying its **count**, and that count must equal the number of lines
  in the block.
- Scenario names are **plain text**, one per line — no bullets, no quotes. They must match the runner's
  display names exactly.
- The machine-read region begins at a **sentinel comment**, so prose above it can change freely.

**This procedure only ever verifies the baseline. It never edits it** — see the last section.

## Step 0 — pre-flight

A pass, failure, baseline drift, format error, environment problem, or status report is not by itself a
ledger checkpoint. If it creates a genuine blocker or the context must end with state that cannot be
reconstructed safely, apply the material checkpoint procedure the repository's plan floor names.

`docker ps` answering is **not** proof Docker is healthy; the reasoning and the `pre-login handshake`
signature are in [`remote-validation`](../remote-validation/SKILL.md). Run the real gate:

```powershell
./scripts/docker-health.ps1   # fresh container + published port + real HTTP round-trip + stability; exit 1 = unhealthy
```

It is vendored from `Concertable/agent-standards`, so that path is the same in every repo. The entrypoint
runs it automatically and refuses to boot on failure. If it reports unhealthy, **stop** — tell the user Docker
is half-started or down and to wait for Docker Desktop to show **Running**, then retry. It is an environment
failure: do not re-run and do not debug application code.

Then tell the user it is starting, with a duration scaled to the passing-baseline size. The entrypoint prints
how many scenarios the baseline requires early in its output; relay that count once you see it.

## Step 1 — run it in the background

```powershell
./scripts/e2e.ps1 ui regress
```

Run it as a background shell task and capture the output file path. The entrypoint:

1. parses the passing blocks out of the baseline;
2. builds an exact-name filter from those names;
3. **preflights the filter with a list-tests run**, so a baseline name that no longer matches a real display
   name fails fast as drift rather than as a missing test;
4. runs the filtered scenarios;
5. **retries only the failures, once, together on a freshly booted stack** separate from the full run's. The
   browser tier is genuinely flaky on a loaded Docker host — the bus emulator drops connections under the
   full run's sustained load and trips different bus-heavy scenarios each time — so a scenario that passes on
   the isolated retry is an environment blip, and one that fails **both** times is a real regression. The
   retry writes its own log beside the run's;
6. asserts, after the retry, that nothing in the passing set failed;
7. exits 0, or 1 on any real regression or baseline drift.

That retry **is** the flaky-versus-real triage [`failing-tests`](../failing-tests/SKILL.md) mandates — the
failed scenario alone on a fresh stack — performed automatically, so its verdict stands and there is nothing
left for a manual re-run to establish. A red regress is therefore a real regression: fix it, never re-run it
to see whether it goes away.

## Step 2 — wait for the terminal line

The entrypoint prints exactly one terminal line, and every repo's must:

```
REGRESS PASSED -- every baseline-passing scenario still passes.
REGRESS FAILED -- at least one baseline-passing scenario regressed.
BASELINE DRIFT: ...
BASELINE FORMAT ERROR: ...
```

Wait for it with the harness's recurring monitor or listener when available, bound to the exact process and
output file and capped at the expected run length. Confirm the terminal line with one direct file read when
the monitor wakes. If no monitor primitive exists, use a **capped background until-loop that echoes what it
sees on every poll and never swallows a poll error**:

```bash
until grep -aE 'REGRESS PASSED|REGRESS FAILED|BASELINE DRIFT|BASELINE FORMAT ERROR|Unhandled exception' <output-file>; do sleep 10; done
grep -aE 'REGRESS PASSED|REGRESS FAILED|REGRESSED:|OK:|BASELINE DRIFT|BASELINE FORMAT ERROR|Total tests:|Passed:|Failed:' <output-file> | tail -20
```

Cap the wait at the expected run length for the current passing set, and re-arm only if the run is
demonstrably still going.

## Step 3a — on pass

Report concisely: the number of baseline-passing scenarios that still pass, broken down per suite. Done. No
follow-up unless the user asks for the full sweep.

## Step 3b — on failure, name the regressed scenarios

The entrypoint prints the failing-scenario block: names that were in the passing baseline and failed both the
run and the isolated retry. Present them, identify each one's suite, and go straight into
[`e2e-ui-debug`](../e2e-ui-debug/SKILL.md) — its per-scenario re-run and diagnosis flow is the fix path, and driving it to green
is the job, not an option to offer.

**Never move a regressed scenario from passing to failing.** The baseline states the *expected* state; moving
it masks the regression instead of fixing it.

## Step 3c — on baseline drift

Drift means a name in the baseline no longer matches any display name the runner discovers, and the
entrypoint prints which ones are missing. Two causes, and they need opposite fixes:

1. **The scenario was renamed** in its feature file — find the new name and update the baseline entry.
2. **The scenario was deleted** — remove the entry, decrement its heading's count, and update any summary
   table in the file.

Show the user both possibilities and ask which applies before editing the baseline. Then re-run.

## Step 3d — on a format error

The parser found a structural problem — a heading count that disagrees with its block, a missing sentinel, a
bulleted or quoted scenario line. The message names exactly what is wrong. Read it to the user and fix the
file against the editing rules at its top. Then re-run.

## Updating the baseline belongs to the full sweep, not here

When a full [`e2e-ui-debug`](../e2e-ui-debug/SKILL.md) run shows a scenario has crossed the line — newly passing, or newly
failing for a reason that is not a regression of this change — the baseline is updated there: move the
scenario between the blocks, fix both counts, update the summary table and the reconciliation line. This
procedure verifies the baseline and never writes to it.

## Cost

Duration scales with the passing set — a small one runs in minutes, a complete one covers every scenario.
Either way, do not skip it because the change "looks safe". When the passing baseline *is* the whole suite,
regress and the full sweep cover the same scenarios and differ only in intent: regress fail-fasts against the
expected set and catches drift, the sweep discovers movement in both directions.
