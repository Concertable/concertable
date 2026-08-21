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
- Last reconciled: 2026-08-21, plan authored on a fresh worktree off `origin/main` (`7f59fe27b`).

## Current state

Plan and this ledger written; no implementation yet (this was a write-the-plan task). The worktree was
created off fresh `origin/main`, clean. The subject surface was mapped exhaustively across Auth, B2B,
Customer, Payment and Search and is captured as the retain-vs-erase table in the plan; the design orchestrates
erasure/export through each service's own facade + integration events only, per
`../../api/ARCHITECTURE.md`. Erasure/export/soft-delete/retention machinery is confirmed **absent** across
`api/` — a green field.

The roadmap marker was de-duplicated: `` `launch/gdpr-subject-rights` `` was carried on **two** checklist
lines (the §"Build" blocker and the §7 launch-ready gate); the §7 gate line was reworded to reference the
build-blocker (matching the webhook/tenant-verification/admin-console convention), leaving the canonical,
still-unchecked marker on the build-blocker line only. The roadmap line is **not** ticked — the feature has
not shipped.

## Next Steps

Implement **Phase 1** of `plans/launch/GDPR_SUBJECT_RIGHTS_PLAN.md` — B2B-local erasure & export behind the
fail-closed gate — in this worktree:

1. Write the standing `plans/launch/GDPR_SUBJECT_RIGHTS.md` compliance doc (the plan's retain-vs-erase table
   + the one-calendar-month DSAR SLA + a `[LEGAL]`/`[DECIDE]` sign-off checklist, mirroring
   `OSA_COMPLIANCE.md`) and update `../../api/Concertable.B2B/src/Modules/Deal/LEGAL_REQUIREMENTS.md` item 8
   from ABSENT to the ratified design.
2. Add the `SubjectErasureRequest` aggregate + state machine and the fail-closed erasure-gate abstraction in
   B2B, gating on what B2B can see today (un-expired `SelfBillingAgreementEntity`; unsettled/`Booked`
   concerts). Load the `multitenancy`, `persistence`, `keyed-strategies`, `result-carriers`, `seeding` and
   `migrations` skills before touching entities.
3. Add erasure + export facade members to `IUserModule`, `ITenantModule`, `IConcertModule`,
   `IConversationsModule` applying the table's B2B rows — ERASE `UserEntity`; SEVER membership under the
   last-owner invariant; scrub `ParticipantProfile`; SEVER `MessageEntity.Content`; leave **every** RETAIN
   row (invoices, contracts, self-billing, `ESignature`) untouched.
4. Expose the reachable admin-gated `POST /api/…/subject-erasure` + `GET /api/…/subject-export`; assemble the
   JSON export bundle from the B2B module fragments.
5. Verification gate: unit tests (state machine, gate defer outcomes, last-owner invariant); integration
   tests (clean subject anonymised while every RETAIN row is unchanged; obligated subject defers; export
   bundle scope). Run `./initial-migrations.ps1` from `api/` and `python .agents/hooks/plan_graph.py --root <worktree>`.
6. Open a draft PR, commit the Phase 1 checkpoint (plan + ledger included), then route the PR through
   `/review` before any delivery step.

## Completed work

- Feature plan + this ledger authored; worktree `Feature/launch_gdpr-subject-rights` created off
  `origin/main` (`7f59fe27b`), clean.
- Roadmap `launch/gdpr-subject-rights` marker de-duplicated to a single canonical (unchecked) build-blocker
  line so the plan graph resolves exactly one marker.

## Verification

- `python .agents/hooks/plan_graph.py --root <worktree>`: pending this checkpoint (baseline over the tree
  before the ledger was added: 0 errors, 0 warnings).

## Reviews

None yet — no code committed.

## Decisions, discoveries, blockers, and deviations

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
