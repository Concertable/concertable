# Code review — Feature/typed-result_customer-outcomes

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `958c05c5a3f51d312b25e1570b36e06a577615de`  _(2026-08-10)_

**Security-reviewed up to commit:** `5cfdb9427f1896351c72b5e829d105b637fcd390`  _(2026-08-10)_

> Range reviewed: `d916e95cf..5cfdb9427` (43 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — MEDIUM — native/package ownership** — `api/Concertable.Customer/src/Modules/Review/Concertable.Customer.Review.Infrastructure/Concertable.Customer.Review.Infrastructure.csproj:27`
  Fixed in this commit by adding direct `Reunion.Errors` ownership; the focused Release build succeeds with 0 errors.
- [x] **NAT2 — MEDIUM — native/correctness** — `api/Concertable.Customer/src/Modules/Preference/Concertable.Customer.Preference.Infrastructure/Services/PreferenceService.cs:37`
  Fixed in this commit with an atomic repository `TryAddAsync` backed by the unique `UserId` index; duplicate-key failures discard the rejected insert and return the typed conflict.
- [x] **NAT3 — MEDIUM — native/correctness** — `api/Concertable.Customer/src/Modules/Review/Concertable.Customer.Review.Infrastructure/Services/ConcertReviewService.cs:90`
  Fixed in this commit with an atomic repository `TryAddAsync` backed by the unique `TicketId` index; duplicate-key failures discard the rejected review and return `ReviewAlreadyExists`.

## Incremental review — 2026-08-10

> Range reviewed: `5cfdb9427..958c05c5a` (6 commits).

No new findings. The three finding fixes preserve repository exception semantics, translate only
duplicate-key races to the existing typed conflicts, discard rejected tracked inserts, and cover
success, conflict, fault, and cancellation paths. The focused Preference and Review integration
wrappers pass against fresh SQL containers.
