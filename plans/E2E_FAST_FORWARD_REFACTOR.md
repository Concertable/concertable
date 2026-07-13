# E2E scenarios — fast-forward to the stage under test, stop re-driving

> Cross-suite test refactor (B2B + Customer UI). Not boundary-blocked (test projects, not published
> packages) — but it's a distinct concern from `Feature/BookingAgreement`, so do it on a
> `Refactor/E2EFastForward` branch off master after that feature lands. The convention this enforces
> is already written in the E2E `CLAUDE.md`s (shared + per-suite); this plan applies it to the
> existing scenarios.

## The principle (already in `Concertable.Shared/tests/Concertable.E2ETests/CLAUDE.md`)

Within a **single contract type, with all other variables held constant**, a stage proven by one
scenario is not re-proven by the next. Once the flat-fee happy path proves `post → apply → accept →
concert`, every other flat-fee scenario **starts at the stage it actually exercises** (via a
fast-forward `Given` backed by `SeedState`) instead of re-driving creation through the browser.

## Current state — mostly compliant; two offenders

Audit of `Concertable.B2B.E2ETests.Ui/Features` (2026-07-12): the payment **variants** already do this
right — `books … with a new card`, `… declined`, `… 3DS`, `… 3DS-failing` for flat fee / door split /
versus / venue hire all open with `Given a … opportunity has been applied to` (seeded applied state,
no UI replay). **Keep those.** The four per-contract `books artist on …` happy paths legitimately drive
the full flow once — that IS the end-to-end creation test per contract. **Keep those too.**

The waste is narrow:

1. **`Venue manager cancels a flat fee booking and the escrow is refunded`** re-drives `post → apply →
   accept + pay → draft concert` through the UI before cancelling — four setup steps another scenario
   already proves. (Master-resident — belongs on this separate branch.)
2. ~~**The agreement signature scenario re-asserts booking creation.**~~ ✅ **DONE on `Feature/BookingAgreement`
   (2026-07-13).** Rather than keep two near-identical scenarios each firing a real Stripe charge, the
   signature assertion was **merged into** `Venue manager books artist on a flat fee with a new card`
   (one charge now proves both draft-concert creation AND both PDF signatures). The standalone
   `The flat fee booking agreement is signed by both parties` scenario was removed. This supersedes the
   earlier "defer to this branch" decision — see Phase 2 below.

## Constraint that shapes the fix (don't fight it)

- **A seeded "booked + concert" state DOES exist** — `SeedState` carries `Booked`/`Complete` app
  handles and `Concerts`, and `ConcertDevSeeder` direct-inserts them. But that state is **reflection-
  stamped** (`ApplicationFactory` forces `State` via `.With(...)` and `.Accept(booking)` only sets the
  nav — the real state machine never runs), and critically it has **no real Stripe PaymentIntent**.
- **A refund needs a real Stripe PaymentIntent to reverse** — can't be seeded (real Payment emits only
  on live webhooks). So the refund assertion must run a real accept + pay.

Net: a pure cancel-*transition* test (assert `Cancelled`, no refund) could fast-forward to a seeded
booked concert via a new `Given`. But the scenario we're fixing asserts **refund**, which needs a real
charge — so it fast-forwards from the seeded **applied** state (dropping `post` + `apply`) and still
runs the real `accept + pay → cancel`. That removes the redundant half without faking a charge.

## Phases

### Phase 1 — Trim the flat-fee cancel scenario
- Rewrite it to open with `Given a flat fee opportunity has been applied to` (existing seeded `Given`),
  then only `accept + pay → cancel → assert cancelled + refunded`. Drops the redundant post + apply UI
  steps; keeps the real charge the refund needs.
- **Gate:** the scenario passes headed once, then via `./e2e.ps1 ui b2b`. Reconcile `E2E_BASELINE.md`
  if timing/name changes.

### ~~Phase 2 — Trim the agreement signature scenario~~ ✅ DONE on `Feature/BookingAgreement` (2026-07-13)
- **Shipped there, not here.** The earlier "defer to the refactor branch" decision was reversed: since
  the signature scenario was *created* on `Feature/BookingAgreement` and isn't in master, the root
  `CLAUDE.md` branch rule says fix it on that branch. Done.
- **What shipped:** instead of merely dropping the redundant `And a draft concert is created` line, the
  signature assertion was **merged into** the existing `Venue manager books artist on a flat fee with a
  new card` scenario (they shared `Given applied → pay-new-card` and differed only in the tail). One
  real Stripe charge now proves both draft-concert creation AND both PDF signatures; the standalone
  `The flat fee booking agreement is signed by both parties` scenario is gone. The `downloads the
  booking agreement` step gained a `[Then]` binding (it now runs in a `Then`/`And` position).
- **Key constraint that still held:** the accept (`pay with new card`) had to stay — the seeded state
  carries no `BookingAgreement` (built only by `BookingAgreementBuilder` in the accept transaction), so
  the agreement must be created by the real accept before it can be downloaded.
- **Migration side-fix (same work):** the E2E stack couldn't boot — `messaging` (a published package)
  had its Outbox/Inbox migration IDs re-timestamped in source, diverging from the package the `b2b-web`
  container runs, so the seeding host hit `already an object named 'Outbox'`. Realigned the messaging
  migrations back to the published IDs (`20260622101059`/`20260622101129`); EF now reports "database
  already up to date." Concert's re-scaffold stays (it's a project reference, not a package).

### Phase 3 — Apply the same audit to the Customer UI suite
- Check `Concertable.Customer.E2ETests.Ui/Features` for scenarios re-driving browse/search/open to
  reach a concert/ticket state another scenario already proves; fast-forward via the Customer suite's
  seeded `Given`s.
- **Gate:** `./e2e.ps1 ui customer` green; baseline reconciled.

## Not in scope
- The four per-contract `books artist on …` happy paths — the canonical creation tests; they stay
  full-flow.
- The payment variants — already fast-forwarded correctly.
- Renaming the shared harness project — separate plan (`plans/E2E_HARNESS_RENAME.md`).

## Gate (every phase)
`dotnet build api/Concertable.slnx` green · the touched UI suite green via `./e2e.ps1 ui <suite>`
(Docker pre-flight mandatory) · `E2E_BASELINE.md` reconciled in the same commit as any status change.
`git rm` this plan in the commit that completes the last phase.
