# Big review — Refactor/launch_deal-lifecycle-modules-phase2

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Plan anchored to commit:** `c50469d483f697890dc9b4f3d2b3013ee1b8c1c9`  _(2026-08-23)_
**Security-reviewed up to commit:** `c50469d483f697890dc9b4f3d2b3013ee1b8c1c9`  _(2026-08-23)_
Net diff reviewed: `fb561acee..c50469d48`. Move-only files skipped.
Status legend: `[ ]` not yet reviewed · `[x]` reviewed (date) · `[~]` in progress (incomplete — re-review).

## Coverage

- [x] Lifecycle contracts and domain foundation — 56 files — reviewed 2026-08-23 — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Contracts/` `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Domain/` `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Contracts/` `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Domain/` `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Contracts/` `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Domain/` `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Domain/` `api/Concertable.B2B/src/Modules/Artist/Concertable.B2B.Artist.Contracts/` `api/Concertable.B2B/src/Modules/Venue/Concertable.B2B.Venue.Contracts/` `api/Concertable.B2B/src/Modules/User/Concertable.B2B.User.Domain/` `api/Concertable.B2B/src/Modules/Deal/Concertable.B2B.Deal.Api/` `api/Concertable.Shared/tests/Concertable.Testing/`
- [ ] Application and Opportunity implementations — 140 files — `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Application/` `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Infrastructure/` `api/Concertable.B2B/src/Modules/Application/Concertable.B2B.Application.Api/` `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Application/` `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Infrastructure/` `api/Concertable.B2B/src/Modules/Opportunity/Concertable.B2B.Opportunity.Api/`
- [ ] Booking and supporting module implementations — 86 files — `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Application/` `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Infrastructure/` `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Api/` `api/Concertable.B2B/src/Modules/Artist/Concertable.B2B.Artist.Application/` `api/Concertable.B2B/src/Modules/Artist/Concertable.B2B.Artist.Infrastructure/` `api/Concertable.B2B/src/Modules/Artist/Concertable.B2B.Artist.Api/` `api/Concertable.B2B/src/Modules/Venue/Concertable.B2B.Venue.Application/` `api/Concertable.B2B/src/Modules/Venue/Concertable.B2B.Venue.Infrastructure/` `api/Concertable.B2B/src/Modules/Venue/Concertable.B2B.Venue.Api/` `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure/` `api/Concertable.B2B/src/Modules/User/Concertable.B2B.User.Infrastructure/` `api/Concertable.B2B/src/Modules/Admin/Concertable.B2B.Admin.Infrastructure/` `api/Concertable.B2B/src/Seed/` `api/Concertable.B2B/src/Concertable.B2B.Web/` `api/Concertable.B2B/src/Concertable.B2B.Workers/`
- [ ] Concert application and API — 111 files — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Application/` `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Api/`
- [ ] Concert infrastructure — 103 files — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/`
- [ ] Module-owned tests — 152 files — `api/Concertable.B2B/src/Modules/Application/Tests/` `api/Concertable.B2B/src/Modules/Booking/Tests/` `api/Concertable.B2B/src/Modules/Opportunity/Tests/` `api/Concertable.B2B/src/Modules/Concert/Tests/` `api/Concertable.B2B/src/Modules/Artist/Tests/` `api/Concertable.B2B/src/Modules/Venue/Tests/` `api/Concertable.B2B/src/Modules/Deal/Tests/` `api/Concertable.B2B/src/Modules/Tenant/Tests/` `api/Concertable.B2B/src/Modules/User/Tests/` `api/Concertable.B2B/src/Modules/Admin/Tests/`
- [ ] Host tests, topology, migrations, and plans — 48 files — `api/Concertable.B2B/tests/` `api/Concertable.B2B/Concertable.B2B.slnx` `api/Concertable.B2B/Directory.Packages.props` `api/Concertable.slnx` `api/initial-migrations.ps1` `api/Concertable.Payment/provider-contract-inventory.json` `api/Concertable.Customer/TECH_DEBT.md` `plans/` `reviews/Refactor-launch_deal-lifecycle-modules-phase2.md`

## Cross-area notes

## Findings

## Lifecycle contracts and domain foundation — reviewed 2026-08-23

- [ ] **NAT1 — HIGH — correctness** — `api/Concertable.B2B/src/Modules/Booking/Concertable.B2B.Booking.Domain/Entities/BookingEntity.cs:121`
  A refund rejection leaves the booking in `CancellationFailed`, but `BeginCancellation` rejects that state, so `BookingService.CancelAsync` retries crash instead of issuing another refund; allow `CancellationFailed`, assign a fresh `CancellationOperationId` for the retry, and cover the rejected-refund retry path.
- [ ] **NAT2 — HIGH — correctness** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Domain/Entities/ConcertEntity.cs:156`
  A Concert cancellation retry reuses the rejected operation ID, causing Payment's terminal-operation replay to return the same rejection forever; assign a fresh operation ID when beginning cancellation from `CancellationFailed` and cover the rejected-refund retry path.

The parallel module-local state-machine slice was not present at the anchored commit. Recheck both findings
against that incoming delta before fixing or closing them.

No security issues were found in this area.
