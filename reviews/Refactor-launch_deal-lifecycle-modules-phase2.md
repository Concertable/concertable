# Code review — Refactor/launch_deal-lifecycle-modules-phase2

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `4b1752304598997ee43c7538daf2f8251a21d41d`  _(2026-08-17)_
**Security-reviewed up to commit:** `4b1752304598997ee43c7538daf2f8251a21d41d`  _(2026-08-17)_

> Range reviewed: `92ea04166..4b1752304` (4 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CV1 — MEDIUM — keyed strategy convention** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Mappers/QueryableConcertMappers.cs:39`
  Preserve the Booking subtype's door-revenue requirement in `ConfirmedBooking` and Concert instead of reinterpreting `DealType` independently in the mapper, specification, and service.

No other issues found. Checked correctness, security of the changed Contracts surface, microservice isolation, module boundaries, seeding, C# conventions, and changed-path test coverage.
