# Music licence attestation progress

- Plan: `plans/launch/MUSIC_LICENCE_ATTESTATION_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/music-licence-attestation`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_music-licence-attestation`
- Branch: `Feature/launch_music-licence-attestation`
- PR: [#363](https://github.com/Concertable/concertable/pull/363) — open, full E2E tier
- Dependency/package gates: none pre-merge. Post-merge: `chore/platform-sync-*` (api/** MinVer bump), expected non-breaking.
- Last reconciled: 2026-08-05 — worktree created off the branch, synced to `origin/main` (0 behind); Phase 1 implemented and the full local gate is green (see Verification).

## Current state

**Phase 1 complete and committed; local gate green.** The `bool HoldsMusicLicence` is threaded end-to-end
through the shipped `TaxCompliance` DAC7 slice: domain VO, migration (re-scaffolded), contracts DTO, mapper,
the Concert cross-module compile fix, the Seed `TenantFactory`, all backend tests, and the b2b/shared org
form (new "Music licence" checkbox). Roadmap line 26 + §7 ticked. Remaining: open the PR, `/review`,
`/merge` with **full E2E**, and own the post-merge `chore/platform-sync-*` PR to green.

## Next Steps

Phase 1 is implemented, verified (full local gate green) and committed. Delivery gates remain:

1. **Open the PR** — plain `gh pr create` (personal repo; no `AB#`, no assignee).
2. **`/review`** — full code review; the change crosses the `Tenant.Contracts` boundary and touches a user-facing org-setup flow, so expect the security lens. Address every finding.
3. **`/merge` with full E2E** — do **not** skip. The change crosses the `Tenant.Contracts` boundary, touches shared web code, and a user-facing org-setup flow (plan §6).
4. **Own the post-merge `chore/platform-sync-*` PR to green** — api/** MinVer bump; expected non-breaking (no cross-service published contract changed).
5. **Close out** — after platform-sync is green, `git rm` this plan + `_PROGRESS.md` as a doc-only close-out riding the next change (plan §7).

## Completed work

- 2026-08-05 — `Feature/launch_music-licence-attestation` created off `origin/main`; plan + this ledger authored.
- 2026-08-05 — Phase 1 vertical slice implemented and committed: domain VO, re-scaffolded migration, contracts DTO, mapper, Concert cross-module fix, Seed `TenantFactory`, backend tests, b2b/shared org form; roadmap line 26 + §7 ticked.

## Verification

Full local gate green (2026-08-05):
- `dotnet build api/Concertable.slnx` → **0 errors** (caught + fixed one missed construction site: the Seed `TenantFactory` target-typed `new(...)`, which the plain grep didn't surface).
- Tenant unit **96/96**, Concert unit **79/79**.
- Tenant integration **56/56** (`TaxComplianceRoundTripTests` proves the new field round-trips `true`→`false` through a fresh EF context).
- `./initial-migrations.ps1` re-scaffolded: only `TenantDbContext` regenerated; new `20260805105823_InitialCreate` carries the `TaxCompliance_HoldsMusicLicence` `bit` column (nullable at DB level because the owned VO is optional, consistent with the DAC7 columns).
- All four web builds green (`web-customer`, `web-venue`, `web-artist`, `web-business`).

**Worktree footgun (MAX_PATH):** the integration suite fails at startup in this deep worktree path — `Microsoft.Data.SqlClient.SNI.dll` won't native-load (`0x800700CE`, filename too long) even with `LongPathsEnabled=1`. Not a code/Docker fault (the main checkout's shorter path is fine). Workaround for a local run: `subst X: "<worktree root>"` and run tests via `X:\…`. Web builds also need the `@concertable/shared` library built first (`npm -w @concertable/shared run build`) after a fresh `npm ci` — its `exports` resolve to `dist/`.

## Reviews

None yet.

## Decisions, discoveries, blockers, and deviations

- **D1** — field goes on the existing `TaxCompliance` VO (the roadmap's `Tenant.Compliance`), not a new VO/config sub-structure; a `TaxCompliance`→`Compliance` rename is out of scope for this isolated change.
- **D2** — non-nullable `bool` (the VO is all-or-nothing, so no "unknown" third state; unchecked = a valid negative attestation, like the VAT checkbox).
- **D3** — record-only: not wired into `IsTaxComplianceCompleteAsync`, settlement, payouts, or invoices.
- **D4** — shown on the shared org form for all B2B tenants (no `isVenueManager` branching); venue-only-via-slot is the noted follow-up alternative.
- **D5** — backend + web ship in one PR (a `required` DTO field couples them); no internal publish gate — `Tenant.Contracts` is B2B-internal (Concert consumes it by project reference, not as a cross-service package).
- **Discovery** — adding a `required` member to `TaxComplianceDto` forces a compile-fix in the **Concert** module's `SelfBillingAgreementServiceTests.cs` (constructs the DTO) and `TenantValidatorsTests.cs`. `InvoiceIssuer.BuildPartyAsync` only reads named fields, so the flag never leaks onto an invoice.

## Event log

### 2026-08-05 — plan spun off roadmap line 26

- Action: read the launch roadmap (line 26 / §5 / §7), `LEGAL_REQUIREMENTS.md`, and the full shipped DAC7 slice (domain VO, EF config, DTO, mapper, validator, service, module boundary, tests, and the b2b/shared org form); created branch `Feature/launch_music-licence-attestation` off `origin/main`; wrote `MUSIC_LICENCE_ATTESTATION_PLAN.md` + this ledger.
- Evidence: `git rev-list --count HEAD..origin/main` = 0; no open `chore/platform-sync-*` PR; construction sites enumerated by grep (`new TaxCompliance(` ×4, `new TaxComplianceDto {` in Concert + Tenant validator tests).
- Outcome: design fixed; ready to implement Phase 1.
- Follow-up: implement per `## Next Steps`.

### 2026-08-05 — Phase 1 implemented, verified, committed

- Action: stood up the worktree, synced to `origin/main`, implemented the full vertical slice (steps 1–15), re-scaffolded migrations, and ran the whole local gate.
- Evidence: build 0 errors; Tenant unit 96/96, Concert unit 79/79, Tenant integration 56/56; migration `20260805105823_InitialCreate` gains `TaxCompliance_HoldsMusicLicence`; 4/4 web builds green. Discovery: the Seed `TenantFactory` target-typed `new(...)` was a construction site the grep missed — the build caught it. Integration suite needed a `subst` short-path workaround (MAX_PATH on the native SQL client DLL); web builds needed `@concertable/shared` built first.
- Outcome: Phase 1 done; roadmap line 26 + §7 ticked in the same commit.
- Follow-up: open PR → `/merge` full E2E → own platform-sync → doc-only close-out.

### 2026-08-05 — handoff correction; branch freed for a worktree

- Action: `Docs/RoadmapBlockerTraversal` (PR #356) merged to `main` mid-session and the main checkout moved to `main`, freeing this branch; brought the feature branch current with `origin/main` (merge of #356 etc.); fixed the resume prompt to the `/worktree create` form (implementation runs in an isolated worktree, not the main checkout). The general `PROMPTS.md` + `plans/agents/PLAN.md` convention fix for that opener was split out to its own docs PR #358, not bundled here.
- Evidence: `git rev-list --count HEAD..origin/main` = 0; docs PR #358 (https://github.com/Concertable/concertable/pull/358).
- Outcome: `/worktree create Feature/launch_music-licence-attestation` will stand up the worktree carrying the plan commit.
- Follow-up: land #358 first, then hand off implementation.

## Resume prompt

```
/worktree create Feature/launch_music-licence-attestation
Read @plans/launch/MUSIC_LICENCE_ATTESTATION_PLAN.md and @plans/launch/MUSIC_LICENCE_ATTESTATION_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
