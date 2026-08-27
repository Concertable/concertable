# Tenant verification progress

- Plan: `plans/launch/TENANT_VERIFICATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/tenant-verification`
- Worktree: `.worktrees/Feature-launch_tenant-verification`
- Branch: `Feature/launch_tenant-verification`
- PR: not opened — Phase 5 + Phase 6 land together in a fresh PR off current `main`
- Dependency/package gates: none — single-service (`Concertable.B2B`), no published-contract boundary
  crossed
- Last reconciled: 2026-08-27, Phase 6 code complete + committed; review + manual smoke + merge outstanding

## Current state

Phases 1–4 merged to `main`. **Phase 5 (frontend, `3c77f8115`) and Phase 6 (retire `VenueEntity.Approved`,
this commit) are committed on `Feature/launch_tenant-verification`, not yet PR'd.**

Phase 6 removed the decorative venue-approval surface end to end: `VenueEntity.Approved`/`Approve()`, the
`[Admin]` approve + `pending-approval` endpoints and their service/repo methods, `ApproveVenueError`,
`PendingVenue`, the `Approved` wire field on venue details, the now-dead
`IVenuePrivilegedRepository`/`VenuePrivilegedRepository`/`VenuePrivilegedDbContext` chain, and
`app/web/admin/src/features/venues/` + its route/nav. Venue migration re-scaffolded (drops one column).

**Two items outstanding before closeout:**
1. Phase 5 manual in-app smoke (submit/approve/reject, publication block, banner) — needs local OIDC + B2B
   stack.
2. Review of the Phase 5 + 6 slice (no `## Reviews` entry yet), then PR → merge, then tick the roadmap
   (line 41 + §7 Architecture line) and delete this plan + ledger in the closeout commit.

## Next Steps

1. Open a review of the full `Feature/launch_tenant-verification` delta (Phase 5 `3c77f8115` + Phase 6) —
   write `reviews/Feature-launch_tenant-verification.md`; address findings; record the `## Reviews` gate.
2. Run the deferred Phase 5 manual in-app smoke.
3. `/open-pr` then `/merge` (single-service, merges straight to `main`, `full-e2e` tier — new observable
   HTTP behaviour: removed endpoints + verification UI).
4. Closeout commit: tick `launch/tenant-verification` in `plans/launch/LAUNCH_ROADMAP.md` line 41 + the
   `Venue/artist verification enforced…` line in §7 Architecture; `git rm` this plan + ledger.

Note: `dotnet build Concertable.slnx` fails **locally in this worktree** on two long-named projects
(`Concertable.Shared.Notification.Infrastructure`, `Concertable.Customer.DataAccess.Infrastructure`) — a
pre-existing Windows MAX_PATH limit (`LongPathsEnabled=0`, worktree path depth pushes the `obj` DLL path to
260 chars), **not** caused by this change (reproduces with the change `git stash`ed) and not present in CI
or the normal checkout. Build changed projects individually, or enable long paths
(`HKLM\SYSTEM\CurrentControlSet\Control\FileSystem\LongPathsEnabled = 1`, admin).

## Completed work

- **Phase 1 — Domain** (PR #772, merged `5222bce51`): `TenantVerificationEntity` +
  `VerificationDocumentEntity`, transitions via `Concertable.Kernel.StateMachine`.
- **Phase 2 — Tenant-facing submission API** (PR #784, merged `1867f0a72`): `IVerificationService` /
  `VerificationController` (`api/organization/verification`), evidence upload via `IBlobStorageService`,
  content-type + magic-byte + size validation.
- **Phase 3 — Cross-module gate + enforcement** (PR #792, merged `564649a26`):
  `ITenantModule.IsVerifiedAsync` (fail-closed); enforced at `OpportunityService.CreateAsync` /
  `CreateMultipleAsync` (`OpportunityMutationError.VenueNotVerified`) and `FinishExecutor.FinishAsync`
  (`SettlementOutcome.DeferredPendingVerification`). Seed: `SeedState.Verifications` +
  `SeedState.UnverifiedTenant` / `UnverifiedVenueManager` (+ venue `9001`). Sync PR #794 merged.
- **Phase 4 — Admin review + cross-module contact + notification** (PR #799, merged `c99c7795c`):
  `[Admin]` `GET api/verification/pending` / `POST {tenantId}/approve` / `reject` on the existing
  `VerificationController`; `IVenueModule`/`IArtistModule.GetContactByTenantIdAsync` +
  `TenantContact` readonly record struct per module Contracts; `IVerificationNotifier` (direct call).
- **Phase 5 — Admin SPA + tenant-facing UI** (`3c77f8115`): admin `features/verification` (pending queue,
  approve, reason-required reject dialog) + `/_admin/verification` route/nav; `app/web/b2b/shared`
  `features/verification` — `VerificationBanner` (DAC7 `TaxDetailsBanner` shape, on both manager
  dashboards), `VerificationPage` + `VerificationForm` (three fixed doc-type uploads, zod schema),
  multipart POST with PascalCase enum tokens; `/settings/verification` route + nav in venue + artist SPAs;
  `./features/verification` package export.
- **Phase 6 — Retire `VenueEntity.Approved`** (this commit): see plan Phase 6 checklist. All the venue
  approve/pending-approval API + admin SPA + `ApproveVenueError` + `PendingVenue` + the `Approved` wire
  field removed; the dead `VenuePrivileged*` chain removed as a scope addition; Venue migration
  re-scaffolded (`20260827211555_InitialCreate`, drops the one column).

## Verification

- Phase 6 (2026-08-27, this commit): `Concertable.B2B.Web` build green; Venue unit tests 19/19,
  Venue integration 28/28 (−7 removed approve/pending tests), B2B architecture 18/18 — all green.
  Five `app/web` builds green + `lint:boundaries` green. Venue migration diff = exactly the dropped
  `Approved` column.
- `dotnet test Concertable.B2B.E2ETests` / merge-queue `full-e2e` tier is the merge gate — not run
  locally. Local full-`slnx` build blocked by a pre-existing MAX_PATH env issue (see Next Steps).
- Backend suites last full green at Phase 4 (`c99c7795c`).

## Reviews

No review recorded yet for the Phase 5 + Phase 6 slice. Phase 4's review file was deleted on merge (all
findings resolved). Needs a fresh `reviews/Feature-launch_tenant-verification.md` before merge — this is
the next step.

## Decisions, discoveries, blockers, and deviations

- Verification stays modeled on `Tenant` (`TenantVerificationEntity`), not duplicated onto `Venue` /
  `Artist` — plan §1.1, do not re-litigate.
- Only two enforcement points: opportunity publication and settlement. Artist Apply is not gated — §1.4.
- **Phase 6 scope addition:** the entire `IVenuePrivilegedRepository` / `VenuePrivilegedRepository` /
  `VenuePrivilegedDbContext` + DI was removed, not just the two approval methods — venue approval was its
  only consumer, so leaving a zero-consumer unfiltered writable `DbContext` was the worse option. Doc
  pointers repointed to `ConversationsPrivilegedDbContext` (still live for moderation).
- **`Approved` was also removed from the public `DetailsResponse` / `VenueDetails`** — it was dead data on
  the wire (no FE reads it; grep confirmed). The `DetailsResponse` *type* stays (frozen public marketplace
  contract per `http-api`), only the field is gone.
- **Local `dotnet build Concertable.slnx` blocked by Windows MAX_PATH** in this worktree — see Next Steps
  for the exact cause and fix. Not caused by this change; CI unaffected. Build changed projects directly.
- **Phase 5 manual in-app smoke still outstanding** — deferred (needs local OIDC + B2B stack). Run before
  closeout.
- **Seeding stayed consistent:** `VenueFactory` no longer calls `venue.Approve()` (the method is gone);
  verification seeding (`SeedState.Verifications`, every seeded tenant gets an `Approved` verification row)
  is untouched. Any new seeded venue/artist fixture still needs an explicit verified/unverified decision.
- `Concertable.B2B.E2ETests/AppFixture.cs`'s standalone seed host hand-duplicates a subset of
  `AddB2BWebHost` registrations — MED tech-debt in `api/Concertable.B2B/TECH_DEBT.md`. It builds green
  with Phase 6's removals (E2E project builds).
- Per `unit-testing`, admin-service orchestration defaults to the integration tier — no new mocked unit
  tests added for Phase 6; the Venue integration suite covers the removed surface's absence.
- Cross-repo standards gap from Phase 2's review (`tomjseery/dotagents` PR #12) still open — unrelated.

## Resume prompt

```
/open-worktree Feature/launch_tenant-verification
Read @plans/launch/TENANT_VERIFICATION_PLAN.md and @plans/launch/TENANT_VERIFICATION_PROGRESS.md and do what its `## Next Steps` says.
```
