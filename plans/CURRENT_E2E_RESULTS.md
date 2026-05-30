# Current E2E Results — 2026-05-30

Branch: `Refactor/Microservices`, head `f53f6fe8` + uncommitted B2B seeding simulator session (Phase 0–7 of `plans/B2B_SEEDING_SIMULATOR_PLAN.md`).

## Summary

**9 passed, 21 failed (out of 30 total)** — **exact match with baseline `ec3a6723`**. Zero regressions, zero new passes.

| Suite | Total | Passed | Failed | vs `ec3a6723` |
|---|---|---|---|---|
| B2B | 23 | 7 | 16 | = |
| Customer | 7 | 2 | 5 | = |
| **Total** | **30** | **9** | **21** | **=** |

## Passing scenarios

### B2B (7)

- New artist manager registers, signs in, creates their artist profile
- New venue manager registers, signs in, creates their venue
- Venue manager signs in via OIDC
- Venue manager books artist on a door split
- Venue manager books artist on a flat fee
- Venue manager books artist on a versus deal
- Artist pays hire fee upfront to book venue

### Customer (2)

- New customer registers and signs in
- Customer signs in via OIDC

## Failing scenarios (21)

All failures cluster on Stripe payment flows — 3DS, "new card" variants, declined-card variants. Same set as baseline `ec3a6723`. Not in scope for this branch.

### B2B (16)

**Door split (4):**
- Venue manager books artist on a door split with a new card
- Venue manager 3DS authentication fails on door split
- Venue manager completes 3DS challenge on door split
- Venue manager door split card registration is declined

**Flat fee (4):**
- Venue manager books artist on a flat fee with a new card
- Venue manager 3DS authentication fails on flat fee
- Venue manager completes 3DS challenge on flat fee
- Venue manager flat fee attempt is declined

**Versus (4):**
- Venue manager books artist on a versus deal with a new card
- Venue manager 3DS authentication fails on versus
- Venue manager completes 3DS challenge on versus
- Venue manager versus card registration is declined

**Venue hire (4):**
- Artist pays hire fee upfront with a new card
- Artist 3DS authentication fails on venue hire
- Artist completes 3DS challenge on venue hire
- Artist venue hire attempt is declined

### Customer (5)

- Customer 3DS authentication fails
- Customer completes 3DS challenge
- Customer purchases a ticket using a new card and views the QR code
- Customer purchase is declined
- Customer searches for concerts, purchases a ticket, and views the QR code

## Diff vs baseline `ec3a6723`

- **Regressions (baseline pass → now fail):** none
- **New passes (baseline fail → now pass):** none
- **Status changes:** none — the pass/fail sets are identical by scenario name.

## What landed in this session (vs `ec3a6723`)

The B2B Seeding Simulator refactor (Phases 0–7 of `plans/B2B_SEEDING_SIMULATOR_PLAN.md`):

- Phase 0 — reverted the bad `XDevSeeder` projection-table seeders added in `ec3a6723`.
- Phase 2 — moved `api/Seeding/` projects out by ownership: shared infra to `api/Shared/Concertable.Seeding.{Shared,Identity,Infrastructure}`, B2B-owned to `api/Concertable.B2B/Concertable.B2B.Seeding`, Customer-owned to `api/Concertable.Customer/Concertable.Customer.Seeding`. `api/Seeding/` deleted.
- Phase 3 — new `Concertable.B2B.Seeding.Fixture` with `B2BSeedFixture` holding 35 venues + 35 artists + 47 concerts as canonical `XChangedEvent` records.
- Phase 3b — B2B `SeedData` venue/artist/concert blocks now project from the fixture via new `FromSeedFixture` factories on the fakers.
- Phase 4 — new `Concertable.B2B.Seeding.Simulator` Worker host that publishes all ~117 fixture events on startup then exits.
- Phase 5 — registered the simulator in `Concertable.Customer.AppHost` only; **not** in umbrella `Concertable.AppHost` or `Concertable.B2B.AppHost`.
- Phase 6 — skipped the standalone `ConcertProjectionHealthWaiter` class (the `IHealthWaiter` abstraction was deleted in `ec3a6723`).
- Phase 7 — deleted `ProjectionSeeder.cs`, replaced with an inline `WaitForSeedProjectionAsync` in `AppFixture` that polls Customer DB until the seed concert lands via the simulator's events. `TicketPurchaseTests` switched from `SeedData.UpcomingConcertId` to `B2BSeedFixture.UpcomingConcertId`.

## Suggested next investigation

Same as `ec3a6723` — the 21 failures all cluster on Stripe payment flows. Most likely candidates:

- Stripe-CLI webhook routing (`stripe-cli` container forwards to `payment-web` — confirm port match in E2E)
- Payment-intent state handling (3DS requires `requires_action` → confirm → `succeeded` lifecycle)
- 3DS challenge iframe interaction in Playwright (frame switching, click timing)

## Notes on noise

- `asb_emulatorhealth_/health_200_check` reports `Unhealthy` continuously throughout every run, but direct `curl` to `/health` on the emulator's port-5300 mapping returns `HTTP 200 {"status":"healthy",...}`. The Aspire-side probe takes 290–430ms per attempt which smells like a TLS handshake timeout (Aspire probing `https://` against the emulator's HTTP-only listener). Tests proceed anyway via Aspire's WaitFor grace timeout. Worth filing upstream against `dotnet/aspire`; not a test-blocker.
