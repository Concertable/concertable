# GDPR Subject Rights — Erasure + Data Export progress

- Plan: `plans/launch/GDPR_SUBJECT_RIGHTS_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/gdpr-subject-rights`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_gdpr-subject-rights`
- Branch: `Feature/launch_gdpr-subject-rights`
- PR: not opened
- Dependency/package gates: **Pre-merge delivery gate** — solicitor retention-policy / retain-vs-erase
  sign-off (swim-lane A, tracked in `LAUNCH_CHECKLIST.md` Phase 2). This gates **merge, not
  implementation** (see `## Decisions`). Cross-service delivery is multi-PR, producer-first, all additive —
  no published-contract shape change, so no expand/contract gate.
- Last reconciled: 2026-08-21, Phase 1 implemented (Privacy module + 4 facade extensions + unit suite green +
  full B2B solution builds 0/0); migrations scaffold + PR open outstanding.

## Current state

**Phase 1 in progress.** Step 1 (docs) is **done**: the standing `plans/launch/GDPR_SUBJECT_RIGHTS.md`
compliance doc (retain-vs-erase register + one-calendar-month DSAR SLA + `[LEGAL]`/`[DECIDE]` sign-off
checklist, mirroring `OSA_COMPLIANCE.md`) is written, and B2B `Modules/Deal/LEGAL_REQUIREMENTS.md` item 8 is
flipped ABSENT → DESIGNED, linking that doc (relative link verified to resolve). Steps 2–5 (code + tests) are
the remaining build.

The B2B patterns were mapped exhaustively (module facades, the `FinishExecutor`/`ConcertFinishedFunction`/
`ISelfBillingAgreementGate` fail-closed shape, the `LifecycleState`/`FrozenDictionary` state machine, the
`TenantScopedRepository`/alias persistence, the Admin-module scaffolding + `[Authorize(Policy="Admin")]`
authority, `initial-migrations.ps1`). The subject surface + retain-vs-erase table are in the plan; the design
orchestrates erasure/export through each module's own facade only, per `../../api/ARCHITECTURE.md`.
Erasure/export/soft-delete/retention machinery is confirmed **absent** across `api/` — a green field.

The roadmap marker was de-duplicated: `` `launch/gdpr-subject-rights` `` was carried on **two** checklist
lines (the §"Build" blocker and the §7 launch-ready gate); the §7 gate line was reworded to reference the
build-blocker (matching the webhook/tenant-verification/admin-console convention), leaving the canonical,
still-unchecked marker on the build-blocker line only. The roadmap line is **not** ticked — the feature has
not shipped.

## Next Steps

**Phase 1 is implemented; source compiles (the B2B Web project builds 0/0) and the Privacy unit suite is
green (20/20).** Remaining to close the phase, in this worktree:

1. Confirm the full `Concertable.B2B.slnx` build is 0 errors (a build over the whole solution incl. both new
   Privacy test projects was in flight at last checkpoint).
2. Run `./initial-migrations.ps1` from `api/` to scaffold `PrivacyDbContext`'s `InitialCreate` (model change —
   re-scaffold, never additive) and confirm no other context's migration id drifts.
3. Run `python .agents/hooks/plan_graph.py --root <worktree>` (0 errors expected).
4. Open a **draft PR**, commit the Phase 1 checkpoint (plan + this ledger + compliance doc included), then
   route through `/review`.
5. **Integration-suite validation is the merge queue's job** (Testcontainers/Docker-gated — not run on the
   workstation). `SubjectRightsApiTests` is written and compiles; it asserts the three plan scenarios via the
   public module facades against the canonical `SeedState`. See the RETAIN-strengthening deviation note.

## Completed work

- Feature plan + this ledger authored; worktree `Feature/launch_gdpr-subject-rights` created off
  `origin/main` (`7f59fe27b`), clean.
- Roadmap `launch/gdpr-subject-rights` marker de-duplicated to a single canonical (unchecked) build-blocker
  line so the plan graph resolves exactly one marker.
- **Step 1 (docs):** `plans/launch/GDPR_SUBJECT_RIGHTS.md` compliance doc written; `LEGAL_REQUIREMENTS.md`
  item 8 flipped ABSENT → DESIGNED (link verified).
- **New `Concertable.B2B.Privacy` module** (Domain/Application/Infrastructure/Api + Unit/Integration tests):
  `SubjectErasureRequestEntity` aggregate + `ErasureState`/`ErasureTrigger`/`ErasureStateMachine` (FrozenDictionary,
  fail-closed) + `ErasureTransitionError`; `ISubjectErasureService` (owns the aggregate/repo/UoW), `IErasureGate`,
  `ISubjectExporter`; `PrivacyDbContext : DbContextBase` (unscoped) + config/factory/provider/schema + repo alias;
  migrate-only Dev/Test seeders; `SubjectRightsController` `[Authorize(Policy="Admin")]` at `POST /api/subject-erasure/{id}`
  + `GET /api/subject-export/{id}`. Wired into `.slnx`, `B2BWebHostExtensions` (AddPrivacyApi + AddPrivacyDevSeeder),
  base `ApiFixture` (AddPrivacyTestSeeder), and `initial-migrations.ps1`.
- **Per-module facade additions** (delegating, zero cross-module queries): User `EraseAsync`/`ExportAsync` +
  domain `Anonymise` + `UserExport`; Tenant `SeverMembershipsAsync` (wound-down detection)/`PurgePendingInvitationsAsync`
  via a focused `TenantErasureService` + repo finders; Conversations `SeverAuthoredMessagesAsync`
  (`MessageEntity.SeverAuthor`)/`ScrubParticipantProfilesAsync`/`ExportAsync` via `ConversationsErasureService` +
  a privileged participant-profile repo + `MessageExport`; Concert `HasLiveObligationsAsync` (`ConcertObligationGate`
  over the unfiltered read stance) + `ExportAsync` (`ConcertRecordsExporter` + `ConcertExportMappers`) +
  `ConcertRecordsExport`/`InvoiceExport`/`ContractExport`/`SelfBillingAgreementExport`; `IConcertReadDbContext`
  gained `Applications`/`Invoices`/`Contracts`.
- **Tests:** Privacy unit suite 20/20 green (state machine edges + fail-closed, entity, `ErasureTransitionError`
  definition contract, gate, erasure-service orchestration incl. defer-vs-complete and email-before-erase ordering).
  `SubjectRightsApiTests` integration written (clean→completed+anonymised, obligated→deferred+intact, export scope,
  admin-gate reachability) — validates in the merge queue.

## Verification

- `dotnet build Concertable.B2B/Concertable.B2B.slnx`: **0 errors, 4 warnings** (whole solution incl. Privacy
  module + both new test projects + fixtures).
- Privacy unit suite (`dotnet test …Privacy.UnitTests`): **20/20 passed**.
- `./initial-migrations.ps1`: run in progress at this checkpoint (scaffolds `PrivacyDbContext.InitialCreate`).
- `python .agents/hooks/plan_graph.py --root <worktree>`: pending this checkpoint.
- Integration suite: Testcontainers-gated — not run on the workstation; validates in the merge queue.

## Reviews

None yet — Phase 1 PR not yet opened; route through `/review` once opened.

## Decisions, discoveries, blockers, and deviations

- **A new `Concertable.B2B.Privacy` vertical-slice module owns the erasure orchestration** (the scalable
  answer over bolting GDPR domain logic onto Admin). It holds the `SubjectErasureRequest` aggregate + state
  machine, the fail-closed erasure gate, the export assembler, and the admin-gated controller. Layers:
  `Privacy.Domain` (aggregate + `ErasureState`/`ErasureTrigger` + `ErasureStateMachine`), `Privacy.Application`
  (service/repo/gate/eraser interfaces, `RequestErasureError`, `SubjectExportBundle`, the `ErasureOutcome`
  success-side enum), `Privacy.Infrastructure` (`PrivacyDbContext : DbContextBase` — unscoped, admin-operated,
  subject-`Guid`-keyed; repo alias on `Guid`; gate/eraser/service impls; DI), `Privacy.Api` (controller). No
  `Privacy.Contracts` yet (no cross-module consumer until Phase 2/5 — YAGNI per module-structure).
- **All data mutation is delegated to the four owning modules via new facade members** (zero cross-module
  queries): `IUserModule` ERASE `UserEntity` (new domain `Anonymise()` nulls Address/Location/Avatar +
  tombstones Email) + export fragment; `ITenantModule` SEVER memberships under the last-owner invariant +
  PURGE pending invitations + sole-trader tax handling + export; `IConversationsModule` scrub
  `ParticipantProfile` (re-project to pseudonym) + SEVER `MessageEntity` author link + export;
  `IConcertModule` `HasLiveObligationsAsync(tenantIds)` for the gate (Booked/AwaitingSettlement applications +
  current `SelfBillingAgreementEntity`) + RETAIN-only export fragment (invoices/contracts, untouched).
- **The gate orchestrates:** Privacy → `ITenantModule.GetMembershipsAsync(userId)` → tenantIds →
  `IConcertModule.HasLiveObligationsAsync(tenantIds)`; `false`→proceed, live→`Deferred` (never throws),
  mirroring `FinishExecutor`'s success-side `Deferred…` outcome. Payment obligations join the gate in Phase 4.
- **Admin gate reused, not reinvented:** the controller is `[Authorize(Policy="Admin")]` against the existing
  data-driven `AdminProfiles` policy (owned by the Admin module, present in the Web host + integration
  fixtures); Privacy does not define a second admin concept and does not reference `Admin.Api`.
- **Integration tests use public-facade read-back; the "RETAIN byte-for-byte" assertion is partial.** The
  canonical `SeedState` seeds **no** invoice/contract/self-billing rows (those are created at Accept/Finish,
  never seeded), so there is no financial RETAIN row to assert survives a *completed* erasure. The suite
  proves RETAIN the reachable way — the obligated-subject test asserts a deferred erasure leaves **every** row
  intact — but the stronger "clean subject anonymised *while a financial row it owns survives*" case needs a
  seeded/hand-built `SelfBillingAgreementEntity` (expired, so it is not itself a live obligation). Deferred as
  a Phase-1 integration follow-up rather than shipped as a blind, unrunnable seed (the Testcontainers suite is
  not runnable on this workstation; it validates in the merge queue).
- **Pre-merge delivery gate (does NOT block implementation).** The solicitor sign-off is a delivery gate,
  not a hard blocker — the design is against the *known* HMRC six-year financial retention, so implementation
  proceeds now and every retain-vs-erase call is recorded for the solicitor to confirm. Recorded in the
  four-field form for the delivery gate (the owner is external / swim-lane A, so there is no sibling
  `_PROGRESS.md` to register a reciprocal handoff against, and this is deliberately **not** placed in
  `## Next Steps`):
  - Blocked: merge of any PR that ships the erasure/export capability to production.
  - Blocked by: solicitor retention-policy / retain-vs-erase sign-off — swim-lane A, tracked in
    `LAUNCH_CHECKLIST.md` Phase 2 ("Data retention schedule documented", "DSAR process documented", `[LEGAL]`).
  - Unblock action: solicitor ratifies the retain-vs-erase table + DSAR SLA in the standing
    `plans/launch/GDPR_SUBJECT_RIGHTS.md` compliance doc (Phase 1 deliverable).
  - Resume when: the compliance doc's `[LEGAL]` sign-off checklist is confirmed by the solicitor.
- **Subject surface corrections from the code map** (do not re-derive): messaging is the **B2B Conversations
  module**, not a separate service; there is **no `BookingAgreementEntity`** — the signed artifact is
  `ContractEntity` and the aggregate is `BookingEntity`; Customer `ReviewEntity` is keyed by **`Email`** (no
  `UserId`), so its erasure matches on email; the transport is a **custom transactional-outbox bus**
  (`Concertable.Messaging`), not MassTransit; the fan-out to mirror is `CredentialRegisteredEvent`; the
  fail-closed pattern to mirror is `FinishExecutor` + the hourly `ConcertFinishedFunction` sweep.
- **`launch/admin-console` is a soft (UX) dependency, not an implementation blocker.** The capability + a
  reachable admin-gated route are testable without the admin SPA (as admin-provisioning was); the polished
  operator UI is the admin console's tenant when it lands.
- **Cross-service delivery is additive.** New erasure event, new facade members, new Payment gRPC method — no
  published `Concertable.*` contract shape changes, so multi-PR delivery keeps the codebase in sync at every
  phase boundary and needs no expand/contract cycle. If one is discovered, it becomes its own plan.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_gdpr-subject-rights
Read @plans/launch/GDPR_SUBJECT_RIGHTS_PLAN.md and @plans/launch/GDPR_SUBJECT_RIGHTS_PROGRESS.md and do what its `## Next Steps` says.
```
