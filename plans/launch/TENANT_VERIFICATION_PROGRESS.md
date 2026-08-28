# Tenant verification progress

- Plan: `plans/launch/TENANT_VERIFICATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/tenant-verification`
- Worktree: `.worktrees/Feature-launch_tenant-verification`
- Branch: `Feature/launch_tenant-verification`
- PR: [#824](https://github.com/Concertable/concertable/pull/824) — **ENQUEUED** (`--merge --auto`,
  `full-e2e` label), head `92a60f13e`. Phase 5 consumer wiring + admin feature + Phase 6. All gates green;
  in the merge queue (~29 min ETA at last check).
- Split-off PR: [#825](https://github.com/Concertable/concertable/pull/825) — **MERGED** `04667a53a`,
  published `@concertable/web-b2b@0.1.0-alpha.0.5385` with the `./features/verification` export.
- Related PR: [#827](https://github.com/Concertable/concertable/pull/827) — **MERGED** `99bcccce4`,
  `setup-local-dev.ps1` + `docs/LOCAL_DEV.md`.
- Dependency/package gates: cleared.
- Last reconciled: 2026-08-28, #824 enqueued after review + security review + smoke.

## Current state

Phases 1–4 merged. **#824 (Phase 5 consumers + admin feature + Phase 6) is in the merge queue.** Phase 5's
shared `web-b2b` half went via #825 (published), because `@concertable/web-b2b` is a published package whose
`carve-fe` restores from the feed — a new export must publish before consumers can build. #824 carries the
venue/artist route/nav wiring, the self-contained admin `features/verification`, and all of Phase 6.

Gates: review ✅ + security review ✅ (0 findings) + `carve-fe` ✅ + all unit/integration ✅. Manual smoke
partially done (see `## Verification`); the submit/approve/reject UI flow is covered by merged integration
tests + `full-e2e` in the queue.

## Next Steps

1. `full-e2e` runs in the merge queue → #824 merges to `main`.
2. **Closeout** (fresh `Docs/tv_closeout` worktree, `merge-docs`): tick `launch/tenant-verification` in
   `plans/launch/LAUNCH_ROADMAP.md` line 41 + the `Venue/artist verification enforced…` line in §7
   Architecture; `git rm` `TENANT_VERIFICATION_PLAN.md` + this ledger + `reviews/Feature-launch_tenant-verification.md`.
   Also add the seed follow-up (below) to `api/Concertable.B2B/TECH_DEBT.md`.
3. If `full-e2e` ejects #824: `failing-tests` on the failing scenario, fix, push, re-enqueue.

Follow-up (not blocking): `AuthDevSeeder` seeds credentials only for `SeedUsers.Managers` + admin +
customers, so `SeedState.UnverifiedVenueManager` (`tenant-verification-gate@test.com`) has **no OIDC
credential** — the verification submit/admin-review flow can't be manually smoked in dev. Add its
credential to `AuthDevSeeder`.

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

- Phase 6 backend: Venue unit 19/19, Venue integration 28/28 (−7 removed), B2B architecture 18/18;
  `Concertable.B2B.Web` + `Concertable.B2B.AppHost` build 0-error. Migration diff = exactly the dropped
  `Approved` column.
- Review fixes: five `app/web` builds + `lint:boundaries` + 28 web-b2b unit tests green.
- **#824 full CI green** on head `92a60f13e` — all backend unit/integration matrices, `carve-fe` (all
  surfaces, against the published `web-b2b`), `fe-boundaries`. `full-e2e` runs in the merge queue.
- **Manual smoke (2026-08-28, live `B2B.AppHost` from this worktree):** real OIDC venue login works (proves
  `appsettings.Development.json`); venue dashboard renders with **no** verification banner (correct — a
  seeded tenant is `Approved`); `/settings/verification` route + Settings-nav item render, showing the
  approved-state card ("Your organisation is verified. Company registration — 27 Aug 2026") and no upload
  form (correct); the `GET /api/organization/verification` round-trip renders (the NAT1 non-204 path);
  admin SPA `requireAdmin` guard correctly denies a non-admin. **Not visually reached:** the submit form +
  populated admin queue + approve/reject buttons — no OIDC-loginable seed user is in the unverified state
  (see the Next Steps follow-up); covered by `VerificationAdminApiTests` / `submitVerificationRequestSchema.test.ts`
  / `usePendingVerifications.test.ts` (all merged) + `full-e2e`.

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
- **Windows MAX_PATH** only bites a *clean-obj standalone* build of the two longest-named projects
  (`Concertable.Shared.Notification.Infrastructure`, `Concertable.Customer.DataAccess.Infrastructure`) in
  this deep worktree (`LongPathsEnabled=0`). In-graph builds are fine: `Concertable.B2B.Web` and
  `Concertable.B2B.AppHost` build 0-error here, so the local smoke runs from this worktree. Full
  `Concertable.slnx` from clean still needs `HKLM\...\LongPathsEnabled = 1` (admin) or the main checkout.
- **Local dev config is now set up** (PR #827): `setup-local-dev.ps1` ran — `ServiceAuth:*ClientSecret`
  user-secrets on all 3 AppHosts (machine-wide), `appsettings.Development.json` created in this worktree +
  the main checkout. `docs/LOCAL_DEV.md` documents it. Was previously undocumented tribal knowledge.
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
