# Tenant verification progress

- Plan: `plans/launch/TENANT_VERIFICATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/tenant-verification`
- Worktree: `.worktrees/Feature-launch_tenant-verification`
- Branch: `Feature/launch_tenant-verification`
- PR: [#824](https://github.com/Concertable/concertable/pull/824) — **DRAFT**, Phase 5 consumer wiring +
  admin feature + Phase 6. **Delivery-gated on PR #825** (see below).
- Split-off PR: [#825](https://github.com/Concertable/concertable/pull/825) — **DRAFT**, the publish-first
  `web-b2b` `features/verification` export (Phase 5 shared half). Merge → `publish-fe-packages.yml`
  publishes → #824's `carve-fe` can go green.
- Dependency/package gates: **#824 `Blocked by:` #825's publish** — `@concertable/web-b2b` is a published
  package; `carve-fe` restores it from the feed, so the venue/artist SPAs can't import
  `@concertable/web-b2b/features/verification` until #825 merges and republishes.
- Last reconciled: 2026-08-27, review complete (2 findings fixed); Phase 5 split for the web-b2b publish
  boundary after `carve-fe` caught it on #824.

## Current state

Phases 1–4 merged to `main`. Phase 5 + Phase 6 committed on `Feature/launch_tenant-verification` (PR #824,
draft). Review complete + approved (2 findings fixed).

**`carve-fe` on #824 failed** (`web/b2b/venue` + `web/b2b/artist`): `Cannot find module
'@concertable/web-b2b/features/verification'`. `@concertable/web-b2b` is a published package that `carve-fe`
restores from the feed — Phase 5 added the `./features/verification` export **and** consumed it from both
manager SPAs in one commit, which can't build until `web-b2b` republishes. **Split:**
- **PR #825** (new branch `Feature/launch_tv-web-b2b-verification`): the shared `features/verification` tree
  + the `package.json` export, no consumer → `carve-fe` green.
- **PR #824**: everything else — venue/artist route+nav+dashboard wiring, the self-contained admin
  `features/verification`, and all of Phase 6. Its `carve-fe` goes green once #825 is published.

Phase 6 removed the decorative venue-approval surface end to end (see plan Phase 6 + the `## Decisions`
scope-addition note). Venue migration re-scaffolded (`20260827211555`, drops one column).

## Next Steps

```
Blocked: #824's carve-fe (web/b2b/venue, web/b2b/artist) fails until @concertable/web-b2b republishes with the ./features/verification export
Blocked by: PR #825 (Feature/launch_tv-web-b2b-verification) + publish-fe-packages.yml
Unblock action: get #825 green → mark ready → merge → publish-fe-packages.yml publishes @concertable/web-b2b@<next>-alpha
Resume when: `npm view @concertable/web-b2b@alpha` (or the feed) exposes ./features/verification, i.e. publish-fe-packages.yml run on the #825 merge commit is green
```

Once #825 is published:
1. `git fetch && git rebase origin/main` in this worktree, drop the shared `features/verification/**` files
   from `43b42bfb7` + `b347f5cff` (now on `main` via #825 — take `main`'s copy on the conflict), push.
2. Re-run #824 `carve-fe` — should be green (feed now has the export).
3. Run the deferred **Phase 5 manual in-app smoke** — submit as venue + artist, approve + reject (reason)
   as admin, confirm the opportunity-publication block and the dashboard banner. Needs the local OIDC + B2B
   stack.
4. Mark #824 ready once smoke passes + exact-head CI green, then `/merge` (single-service → `main`,
   `full-e2e` tier).
5. Closeout commit on merge: tick `launch/tenant-verification` in `plans/launch/LAUNCH_ROADMAP.md` line 41 +
   the `Venue/artist verification enforced…` line in §7 Architecture; `git rm` this plan + ledger.

Review gate is **satisfied** (see `## Reviews`). The worktree MAX_PATH build limit and the web-b2b publish
rule are in `## Decisions` below.

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
- **Phase 5 — Admin SPA + tenant-facing UI** (`43b42bfb7`, review fixes `b347f5cff`) — **split for the
  web-b2b publish boundary:**
  - admin `features/verification` (pending queue, approve, reason-required reject dialog) +
    `/_admin/verification` route/nav — self-contained, stays in **#824**.
  - shared `app/web/b2b/shared/features/verification` (`VerificationBanner` — DAC7 `TaxDetailsBanner`
    shape; `VerificationPage` + `VerificationForm` — three fixed doc-type uploads, zod schema; multipart
    POST, PascalCase enum tokens) + the `./features/verification` package export → **PR #825** (publish
    first).
  - `/settings/verification` route + nav in venue + artist SPAs + `VerificationBanner` on both dashboards
    → stays in **#824**, delivery-gated on #825.
- **Phase 6 — Retire `VenueEntity.Approved`** (`3685c5f47`, in #824): all the venue approve/pending-approval
  API + admin SPA + `ApproveVenueError` + `PendingVenue` + the `Approved` wire field removed; the dead
  `VenuePrivileged*` chain removed as a scope addition; Venue migration re-scaffolded
  (`20260827211555_InitialCreate`, drops the one column).

## Verification

- Phase 6 backend (`3685c5f47`): `Concertable.B2B.Web` build green; Venue unit 19/19, Venue integration
  28/28 (−7 removed approve/pending tests), B2B architecture 18/18. Venue migration diff = exactly the
  dropped `Approved` column.
- Review fixes (`b347f5cff`): five `app/web` builds + `lint:boundaries` green; 28 web-b2b unit tests green.
- **#824 CI (`79fd01b20`): `carve-fe` RED** on `web/b2b/venue` + `web/b2b/artist` (the publish-boundary
  issue). All backend unit + integration matrices, `fe-boundaries`, other carves — green. Blocked pending
  the #825 split (see Next Steps).
- **#825 CI:** in progress at time of writing.
- `full-e2e` tier is the merge-queue gate — not run locally.

## Reviews

`reviews/Feature-launch_tenant-verification.md` — **status `complete`**, frozen range
`085520405..8bcbde3bf`, native layer via an independent cold `code-reviewer` context + parent synthesis.
Two findings, both **fixed on-branch**:
- **NAT1 (MEDIUM)** — `verificationApi.get` returned `undefined` on HTTP 204 (the common "never submitted"
  case) → TanStack Query v5 throws → permanent error state on the dashboard banner + `/settings/verification`.
  Fixed: returns `Verification | null`.
- **NAT2 (LOW)** — `VerificationForm` mapped per-file validation errors by catalog order, not attach order.
  Fixed: derives from `Object.keys(buffer)`.

One cross-area note (`organizationApi.get` has the same latent 204 bug, unreachable in practice) transferred
to `app/web/b2b/shared/TECH_DEBT.md` (LOW). Phase 6 backend removals + migration reviewed clean.

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
- **`@concertable/web-b2b` is a published package** and `carve-fe` restores it from the feed, not from
  workspace source — so a new `./features/*` export must publish before venue/artist can consume it. The
  plan's §3 "no published-package boundary crossed" was wrong for Phase 5; corrected there. **Rule for any
  future `web-b2b` (or `shared`/`web`) export: land the export in its own PR, let `publish-fe-packages.yml`
  publish it, then consume in a follow-up.** Precedent: commits `5246aeeb2`, `382bf817f` ("publish …
  exports"). `publish-fe-packages.yml` triggers only on push to `main`, so a combined export+consume PR's
  `carve-fe` can never go green.
- **Local `dotnet build Concertable.slnx` blocked by Windows MAX_PATH** in this worktree
  (`LongPathsEnabled=0`, worktree path depth → `obj` DLL path hits 260 chars on
  `Concertable.Shared.Notification.Infrastructure` + `Concertable.Customer.DataAccess.Infrastructure`).
  Reproduces with this change `git stash`ed; CI + normal checkout unaffected. Build changed projects
  directly, or `HKLM\SYSTEM\CurrentControlSet\Control\FileSystem\LongPathsEnabled = 1` (admin).
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
