# Code review — Feature/typed-result_customer-outcomes

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `d623a35014cd23632f190e557ee37668953680b9`  _(2026-08-12)_

**Security-reviewed up to commit:** `d623a35014cd23632f190e557ee37668953680b9`  _(2026-08-12)_

> Range reviewed: `d916e95cf..5cfdb9427` (43 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — MEDIUM — native/package ownership** — `api/Concertable.Customer/src/Modules/Review/Concertable.Customer.Review.Infrastructure/Concertable.Customer.Review.Infrastructure.csproj:27`
  Fixed in this commit by adding direct `Reunion.Errors` ownership; the focused Release build succeeds with 0 errors.
- [x] **NAT2 — MEDIUM — native/correctness** — `api/Concertable.Customer/src/Modules/Preference/Concertable.Customer.Preference.Infrastructure/Services/PreferenceService.cs:37`
  Fixed in this commit with an atomic repository `InsertAsync` backed by the unique `UserId` index; duplicate-key failures discard the rejected insert and return the typed conflict.
- [x] **NAT3 — MEDIUM — native/correctness** — `api/Concertable.Customer/src/Modules/Review/Concertable.Customer.Review.Infrastructure/Services/ConcertReviewService.cs:90`
  Fixed in this commit with an atomic repository `InsertAsync` backed by the unique `TicketId` index; duplicate-key failures discard the rejected review and return `ReviewAlreadyExists`.

## Incremental review — 2026-08-10

> Range reviewed: `5cfdb9427..958c05c5a` (6 commits).

No new findings. The three finding fixes preserve repository exception semantics, translate only
duplicate-key races to the existing typed conflicts, discard rejected tracked inserts, and cover
success, conflict, fault, and cancellation paths. The focused Preference and Review integration
wrappers pass against fresh SQL containers.

## Incremental review — 2026-08-10 (current-main reconciliation)

> Range reviewed: `958c05c5a..c021d26c9` (39 commits).

No issues found. Checked correctness, security-sensitive paths, microservice isolation, module
boundaries, seeding, C# conventions, and test coverage of changed paths. The only merge conflict was
Customer central package management; the resolution takes platform `.910`, preserves the branch's
required `Shouldly` ownership, and accepts main's removal of unused `FluentResults`.

## Incremental review — 2026-08-12

> Range reviewed: `c021d26c9..22fb61697` (166 commits).

- [x] **CV4 — LOW — C# convention** — `api/Concertable.Customer/src/Modules/Artist/Concertable.Customer.Artist.Infrastructure/Repositories/ArtistReadRepository.cs:14`, `api/Concertable.Customer/src/Modules/Venue/Concertable.Customer.Venue.Infrastructure/Repositories/VenueReadRepository.cs:14`, `api/Concertable.Customer/src/Modules/Concert/Concertable.Customer.Concert.Infrastructure/Repositories/ConcertReadRepository.cs:15`
  Fixed by qualifying all five inherited `Query` uses with `base.`; the affected Concert, Artist, and Venue Release unit suites pass 25/25.

No other issues found. Checked correctness, security-sensitive paths, microservice isolation, module
boundaries, AppHost composition, seeding, C# conventions, and test coverage. The Auth/hosting changes
preserve secret injection and authorization behavior; the browser-storage changes defer Stripe and
Maps loading without introducing a fail-open consent path; the Reunion alpha.2 conversions preserve
the target-typed success and error alternatives verified by the scoped unit and integration suites.

## Incremental review — 2026-08-12 (CV4 fix)

> Range reviewed: `22fb61697..d623a3501` (1 commit).

No new findings. The five `base.Query` qualifications are convention-only, preserve the no-tracking
query root, and pass the affected Concert, Artist, and Venue Release unit suites 25/25.
