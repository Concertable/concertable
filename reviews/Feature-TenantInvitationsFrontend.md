# Code review — Feature/TenantInvitationsFrontend

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `767910ac15b2103d67576c30cf51077e71bb2188`  _(2026-07-22)_

> Range reviewed: `6f498200..767910ac` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **TEST1 — LOW — test coverage** — `api/Concertable.B2B/src/Modules/Tenant/Tests/Concertable.B2B.Tenant.IntegrationTests/InvitationTests.cs:52`
  D9's new persona→portal switch in `SendInvitationEmailAsync` (`InvitationService.cs:128-133`) routes
  `Venue → 5175` and `Artist → 5176`, but only the Venue branch is asserted
  (`InvitationTests.cs:71`). The whole point of D9 is persona-correct accept links, and the Artist
  branch is unexercised — a regression that broke/swapped the artist URL wouldn't be caught, even
  though the fixture already wires `Urls:ArtistFrontend = https://localhost:5176` (`ApiFixture.cs:89`)
  and the seed has `ArtistManager1` (a founding artist-tenant Owner). Fix: add an integration test
  where `ArtistManager1` invites a colleague and assert the email body contains
  `https://localhost:5176/settings/members/accept/{dto.Id}` — mirror of the existing venue test.
  (Lens F: a behaviour the diff adds whose second branch has no covering assertion; concrete fix.)
  — FIXED (added `Invite_AsArtistOwner_SendsEmailWithArtistPortalAcceptLink`, asserting the artist
  tenant's invite email carries the `https://localhost:5176/...` accept link)
