# Online Safety Act — report-content flow progress

- Plan: `plans/launch/OSA_REPORT_CONTENT_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/osa-report-content`
- Worktree: `/home/tommy/projects/csharp/Concertable` (the main checkout — the kickoff instruction created the branch here rather than in an isolated worktree)
- Branch: `Feature/launch_osa-report-content`
- PR: not opened
- Dependency/package gates: none blocking. No open or red `chore/platform-sync-*` PR at branch time. One
  package question to settle at the first checkpoint — see "Open question" below.
- Last reconciled: 2026-08-14, from `origin/main` at branch creation plus a direct read of the
  Conversations, Tenant, User and Payment source named in the plan.

## Current state

Plan and ledger written; **no code written**. The branch was created from current `origin/main` and
carries only these two documents plus the roadmap edits.

The design is fully resolved and needs no further input to start Phase 1. Three things worth knowing
before touching code, all verified against source rather than assumed:

- **Tenant suspension is split out** and is not part of this delivery. The full analysis, the recommended
  escrow/payout policy, and the legal gate that forces the split are in plan §7. A Swim-lane C row now
  tracks it in the roadmap; no plan or ledger was created for it, deliberately.
- **The `[Admin]` seam is being extended, not rebuilt.** Assessment in plan §3; its gaps get a
  `api/Concertable.B2B/TECH_DEBT.md` entry in Phase 2.
- **This is B2B's first production Reunion usage** (plan D2), scoped to the one new operation.

## Next Steps

Implement **Phase 1** of `plans/launch/OSA_REPORT_CONTENT_PLAN.md` (§10) on this branch, in the order the
phase lists.

1. **First, settle the D2 package question before writing any Reunion code.** Add
   `Reunion.Errors`, `Reunion.Validation`, `Reunion.AspNetCore` (all at `0.1.0-alpha.3`, matching the
   `Reunion` version already declared) and `Dunet 1.16.2` to
   `api/Concertable.B2B/Directory.Packages.props`, then restore the Conversations projects. If alpha.3 of
   any of those is not on the feed, **stop and surface it** — do not silently bump B2B's Reunion graph to
   Customer's alpha.6, because B2B consumes Payment client packages compiled against alpha.3 (plan D2).
2. Build the backend slice in plan §10 order: `ReportCategory` → `ContentReportEntity` →
   `ConversationsDbContext` + EF configuration → `ContentReportRepository` → `ReportMessageError` +
   validators → `IContentReportService` + `ContentReportNotifier` → `SafetySettings` +
   `appsettings.json` → composition root → `MessageController` report endpoint + the `MessageResponses`
   HATEOAS mapping.
3. Run `./initial-migrations.ps1` from `api/` (the model changed — re-scaffold, never additive).
4. Write the Phase 1 unit and integration tests (plan §6.1 and the report half of §6.2), including the
   mandatory `ReportMessageError` definition-contract test.
5. Build the web slice (plan §5) and the UI E2E feature/steps/page-object additions (plan §6.3). Do not
   run E2E locally — the merge queue owns it.
6. Verification gate as written in plan §10, then commit and push, opening a **draft** PR at that
   checkpoint. Update this ledger in the same commit.

Do **not** touch the roadmap lines yet. They move only in the same commit as the shipping work, and
the plan deliberately does not name them (a plan never cites the roadmap), so they are recorded here:

- `plans/launch/LAUNCH_ROADMAP.md` **§5**, the "Online Safety Act report-content flow" Swim-lane C row —
  mark it `✅` with Month → `done`, annotated that the in-app route shipped and the published-email
  fallback remains with the legal pages.
- `plans/launch/LAUNCH_ROADMAP.md` **§7**, "Online Safety Act report-content button + email destination
  live `launch/osa-report-content`" — **leave it un-ticked**, annotated with what is live (report button,
  structured safety-inbox email, persisted record, admin moderation) and what is outstanding (the
  published `report@` address on the footer legal pages, solicitor-gated). Ticking it would claim a
  compliance state we do not have.

## Completed work

- Plan and ledger written from current `origin/main`; branch `Feature/launch_osa-report-content` created
  from `origin/main`.
- `launch/osa-report-content` key added to the roadmap §7 checklist line, and a Swim-lane C §5 row added
  for the split-out tenant-suspension work.

## Verification

`python .agents/hooks/plan_graph.py --root /home/tommy/projects/csharp/Concertable` — 0 errors.

No build or test verification applies yet; no code exists on this branch.

## Reviews

None yet.

## Decisions, discoveries, blockers, and deviations

Durable findings from the design pass that cost real source-reading and would be expensive to
re-derive. The full reasoning is in the plan; these are the facts a fresh agent must not re-litigate.

- **Token issuance is the wrong suspension seam in B2B.** B2B tokens are identity-only since
  `Feature/RetireRoleClaim`; only Customer exposes `/internal/users/{sub}/claims`, so B2B contributes no
  claims to Auth, and `RemoteProfileClaimsProvider` caches for 5 minutes anyway. The correct seam is
  `TenantContext.ResolveMembershipAsync`, which already re-reads membership from the DB every request.
  Plan §7.1.
- **Suspension needs no Payment contract change** under the recommended policy (freeze forward
  capability; resolve each in-flight booking explicitly through the existing release/refund paths; do
  **not** add a third fail-closed settlement gate, because unlike tax-compliance and self-billing it
  cannot self-heal). No publish-first gate. Plan §7.2.
- **The split gate for suspension is legal, not technical:** the `[LEGAL]` T&Cs illegal-content
  enforcement clause does not exist yet. Plan §7.3.
- **Report categories drive no varying behaviour**, so the keyed strategy resolver is deliberately *not*
  used — five identical strategies would be the marker-interface smell. Plan D1.
- **A non-participant report attempt returns 404, not 403** — the tenant-filtered context makes "not
  yours" and "does not exist" the same lookup, which is also the right privacy answer. Plan D5.
- **`ActionLink` is duplicated in Conversations.Api** rather than shared with Concert's; hoisting a
  three-line internal record into a shared package would be the cross-module coupling the modular
  monolith rules forbid. Plan §4.7.
- **Two controllers are required**, not one: ASP.NET ANDs stacked authorize attributes, so an `[Admin]`
  action inside `MessageController`'s class-level `[HasPermission(MessagesRead)]` would demand a tenant
  membership the platform admin does not have. Plan §4.6.
- **Open question, first checkpoint only:** whether `Reunion.Errors` / `Reunion.Validation` /
  `Reunion.AspNetCore` exist at `0.1.0-alpha.3` on the feed. Handling is step 1 of `## Next Steps`. If
  Tommy prefers not to make this PR B2B's first Reunion adoption, the named fallback is `OrNotFound` +
  FluentValidation (plan D2) — a smaller diff that adds to the migration debt `RESULT_PATTERN.md` names.

## Resume prompt

```
cd /home/tommy/projects/csharp/Concertable
Read @plans/launch/OSA_REPORT_CONTENT_PLAN.md and @plans/launch/OSA_REPORT_CONTENT_PROGRESS.md and do what its `## Next Steps` says.
```
