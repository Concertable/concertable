# Code review — Feature/typed-result_customer-outcomes

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `5cfdb9427f1896351c72b5e829d105b637fcd390`  _(2026-08-10)_

**Security-reviewed up to commit:** `5cfdb9427f1896351c72b5e829d105b637fcd390`  _(2026-08-10)_

> Range reviewed: `d916e95cf..5cfdb9427` (43 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [ ] **NAT1 — MEDIUM — native/package ownership** — `api/Concertable.Customer/src/Modules/Review/Concertable.Customer.Review.Infrastructure/Concertable.Customer.Review.Infrastructure.csproj:27`
  `ReviewValidator` directly imports and constructs `Reunion.Errors.ValidationErrors`, but its compiling project declares only `Reunion` and `Reunion.Validation`; add a direct `Reunion.Errors` reference so the standalone project does not rely on a transitive compile asset.
- [ ] **NAT2 — MEDIUM — native/correctness** — `api/Concertable.Customer/src/Modules/Preference/Concertable.Customer.Preference.Infrastructure/Services/PreferenceService.cs:37`
  The new `PreferenceAlreadyExists` result is guarded only by a read before insert, so concurrent create requests can both pass the read and the loser surfaces SQL's unique-`UserId` violation as a 500; translate the existing duplicate-key constraint at `SaveChangesAsync` into the same typed conflict and discard the failed tracked insert.
- [ ] **NAT3 — MEDIUM — native/correctness** — `api/Concertable.Customer/src/Modules/Review/Concertable.Customer.Review.Infrastructure/Services/ConcertReviewService.cs:90`
  The new `ReviewAlreadyExists` result has the same check-then-insert race: concurrent submissions for one ticket can both pass `HasReviewForTicketAsync`, after which the unique-`TicketId` loser becomes a 500; translate that duplicate-key persistence failure into `ReviewAlreadyExists` and discard the failed tracked review.
