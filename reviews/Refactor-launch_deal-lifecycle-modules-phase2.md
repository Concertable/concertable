# Code review — Refactor/launch_deal-lifecycle-modules-phase2

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `3a83e68d3557753fa5f241d8dc51da7a8000995a`  _(2026-08-17)_
**Security-reviewed up to commit:** `3a83e68d3557753fa5f241d8dc51da7a8000995a`  _(2026-08-17)_

> Current-base range reviewed: `2cfbce326..3a83e68d3` (13 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CV1 — MEDIUM — keyed strategy convention** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Mappers/QueryableConcertMappers.cs:39`
  Preserve the Booking subtype's door-revenue requirement in `ConfirmedBooking` and Concert instead of reinterpreting `DealType` independently in the mapper, specification, and service.
- [x] **CV2 — LOW — repository hygiene** — `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Contracts/Concertable.B2B.Opportunity.Contracts.csproj:10`
  Remove the extra trailing blank lines from the two scaffolded Opportunity project files.
- [x] **BUG1 — HIGH — missed E2E consumer** — `api/Concertable.B2B/tests/E2ETests/Concertable.B2B.E2ETests/Payments/ConcertFinishedTests.cs:50`
  Replace the remaining Booking-to-Concert navigation references in both B2B E2E projects with the explicit seed-state lookup.

No other issues found. Checked correctness, security of the changed Contracts surface, current-main integration, microservice isolation, module boundaries, seeding, C# conventions, and changed-path test coverage.
