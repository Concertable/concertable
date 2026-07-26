# E2E (UI) Scenario Conventions

The scenario-authoring rules for **every** UI E2E suite (B2B, Customer) — Reqnroll + Playwright.
Identical across suites, so they live in one doc; each suite's own `CLAUDE.md` adds only its concrete
fast-forward mechanics (its `SeedState` shape). For non-browser API/service E2E and for the
service-agnostic harness-project boundary rules, see the harness project's own `CLAUDE.md`.

**A scenario tests one behaviour and starts at the nearest already-verified state.** It never
re-drives earlier stages through the browser to reach its starting line. If a happy path already
covers `create → book`, a scenario acting on a booking fast-forwards to "booked" and drives only its
own behaviour + assertion. Litmus test before writing a `When`/`And` setup step: *is this proving the
behaviour in the scenario title, or just getting me to the starting line?* If it's the latter and
another scenario already covers it, make it a fast-forward `Given`, not UI steps.

**Fast-forward via seeded state, never UI replay.** A setup `Given` reads pre-seeded data off the
suite's fixture (`fixture.App.SeedState…`) and sets the id on scenario state — no navigation, no clicks.
When the starting state you need doesn't exist yet, add the seeded state + a `Given`; don't reach it by
replaying UI steps another scenario already runs.

**The one thing you cannot seed: payment/Stripe state.** Seeding obeys production's rule (a seeder only
writes what prod writes directly), and real Payment emits only on live Stripe webhooks — so no seeder
creates a PaymentIntent/charge. A scenario whose assertion needs a real Stripe object (e.g. a refund
reversing a real charge) must run the real paying flow; it can't be pure-seed fast-forwarded. Split it:
the cheap state-transition assertion starts from seeded state, the Stripe-dependent assertion stays on
a flow that actually paid.

**Baseline discipline — `E2E_BASELINE.md`.** `./e2e.ps1 ui regress` trusts it. Two traps: (1) when a
scenario crosses the line, move it between the `passing`/`failing` blocks AND fix both `(N)` counts and
the summary table (the parser throws on a mismatch); (2) adding an assertion to an already-green
scenario can silently turn it red while the baseline still lists it as passing — the name didn't
change, but the body now fails. Re-run and reconcile; a name in `passing` is not proof the current body
passes.

**Headless by default.** Run via `./e2e.ps1 ui <cmd>` (mandatory Docker health gate); `-Headed` only
when a human is watching — it changes nothing that's asserted.
