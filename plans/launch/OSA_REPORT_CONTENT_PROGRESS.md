# Online Safety Act — report-content flow progress

- Plan: `plans/launch/OSA_REPORT_CONTENT_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/osa-report-content`
- Worktree: `/home/tommy/projects/csharp/Concertable` (the main checkout — the kickoff instruction created the branch here rather than in an isolated worktree)
- Branch: `Feature/launch_osa-report-content`
- PR: not opened
- Dependency/package gates: none blocking. No open or red `chore/platform-sync-*` PR at branch time. The
  D2 package question is **settled** — see "Completed work".
- Environment gate (local only, not a code gate): the .NET restore of `api/**` needs
  `GITHUB_PACKAGES_TOKEN`; see "Decisions, discoveries, blockers, and deviations".
- Last reconciled: 2026-08-14, after writing the Phase 1 backend slice and the whole web + UI E2E slice.

## Current state

**Phase 1 backend is written; the web slice and UI E2E feature are written and verified green.** The
backend has not yet been compiled once — the local .NET restore is blocked on a missing
`GITHUB_PACKAGES_TOKEN` (environment, not code). Phase 2 (admin moderation), the migration
re-scaffold, and the integration tests are outstanding.

Three things worth knowing, all verified against source rather than assumed:

- **Tenant suspension is split out** and is not part of this delivery. The full analysis, the recommended
  escrow/payout policy, and the legal gate that forces the split are in plan §7. A Swim-lane C row now
  tracks it in the roadmap; no plan or ledger was created for it, deliberately.
- **The `[Admin]` seam is being extended, not rebuilt.** Assessment in plan §3; its gaps get a
  `api/Concertable.B2B/TECH_DEBT.md` entry in Phase 2.
- **This is B2B's first production Reunion usage** (plan D2), scoped to the one new operation.

## Next Steps

```text
Blocked: local `dotnet restore` of `api/**` fails 401 against the private GitHub Packages feed, so the Phase 1 verification gate (Conversations build + focused unit tests + `./initial-migrations.ps1`) cannot run.
Blocked by: Tommy — the `gh` token has scopes `gist`, `read:org`, `repo`; GitHub Packages requires `read:packages`, and adding it needs interactive browser consent no agent can give.
Unblock action: Tommy runs `gh auth refresh -s read:packages` in this session, after which the session exports `GITHUB_PACKAGES_TOKEN=$(gh auth token)`.
Resume when: `GITHUB_PACKAGES_TOKEN=$(gh auth token) dotnet restore api/Concertable.B2B/src/Modules/Conversations/Concertable.B2B.Conversations.Api/Concertable.B2B.Conversations.Api.csproj` completes with no NU1301.
```

Once the gate opens, in order:

1. **Compile the Conversations module and fix what the first build finds.** This is B2B's first Reunion
   usage, so treat the composition in `ContentReportService.SubmitAsync` as the likely fix site: the
   `.OrFailure(...).Ensure(...).BindAsync(...)` chain assumes alpha.3 exposes the validation-aware
   `Ensure` task overload and a `Result<T,E> → UnitResult<E>` `BindAsync`. If either is absent, keep the
   operation's shape and adjust the combinators — do **not** fall back to exceptions.
2. **Run the focused Conversations unit tests**, in particular `ReportMessageErrorTests`. Its expected
   codes (`report.message_not_found`, `report.message_invalid`) are *derived predictions* from the
   `RESULT_PATTERN.md` naming rule, not observed output — correct the test to whatever Reunion actually
   derives, then keep it hard-coded.
3. Run `./initial-migrations.ps1` from `api/` (the model changed — re-scaffold, never additive).
4. Write the Phase 1 integration tests (plan §6.2, report half) over `ConversationsApiFixture`.
5. Implement **Phase 2** (plan §11): `MessageEntity.Hide`/`Restore`, `AdminConversationsDbContext`,
   the admin repositories, `IModerationService`, `ModerationController` (`[Admin]`), the
   `MessageRepository` hidden-message exclusion, the moderation tests (including the
   **tenant-Owner-gets-403** guard), and the `api/Concertable.B2B/TECH_DEBT.md` entry for the `[Admin]`
   seam gaps. Re-run `./initial-migrations.ps1` after the `MessageEntity` change.
6. Verification gate as written in plan §10/§11, then push, opening a **draft** PR. Update this ledger
   in the same commit.

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
- **D2 settled, no fork.** `Reunion.Errors`, `Reunion.Validation` and `Reunion.AspNetCore` all publish
  `0.1.0-alpha.3` on nuget.org (`Reunion.AspNetCore` skips alpha.4 but has alpha.3), and `Dunet 1.16.2`
  is present. All four are declared in `api/Concertable.B2B/Directory.Packages.props` at those versions,
  so B2B stays on one Reunion version and is not bumped to Customer's alpha.6.
- **Phase 1 backend written** (unverified — see the environment gate): `ReportCategory`/`ReportOutcome`,
  `ContentReportEntity` (with `Resolve`), the EF configuration + `ConversationsDbContext` stance, a
  module `Repository<T>` base + `ContentReportRepository`, `IMessageRepository.GetByIdAsync`,
  `ReportMessageError`, `ContentReportValidators`, `ContentReportService`, `ContentReportNotifier`,
  `SafetySettings` + the `Safety` section in `appsettings.json`, the composition-root registrations, and
  the `MessageController` report endpoint with the `MessageResponse`/`MessageActions` HATEOAS mapping.
- **Phase 1 unit tests written** (unrun): `ContentReportEntityTests`, `ReportMessageErrorTests`,
  `ContentReportServiceTests`.
- **Web slice complete and green** (plan §5): `ReportCategory`/`ReportMessageRequest`/`MessageActions`
  on the cross-platform `Message` type, `messageApi.reportMessage`, `useReportMessageMutation`,
  `ReportMessageDialog.tsx`, and the data-link-gated `Report` control in `Mailbox.tsx`.
- **UI E2E feature written** (plan §6.3): `ContentReport.feature`, `ContentReportSteps.cs`, and the
  report interactions on `MailboxPage`. Not run locally — the merge queue owns E2E.

## Verification

- `python .agents/hooks/plan_graph.py --root /home/tommy/projects/csharp/Concertable` — 0 errors.
- **Web, all green:** `npm run build:web-packages`, then all four app builds
  (`web-customer`, `web-venue`, `web-artist`, `web-business`) and `npm run lint:boundaries`
  (no dependency violations across all five cruised graphs).
- **Backend: not verified.** The Conversations module has never been compiled — local `dotnet restore`
  of `api/**` fails 401 against the private feed. Treat every backend file on this branch as unbuilt.

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
- **Resolved — the D2 package question is not a fork.** All three Reunion sub-packages publish
  `0.1.0-alpha.3`; B2B stays on one Reunion version. The `OrNotFound` + FluentValidation fallback was
  not needed and was not taken.
- **Local .NET restore needs `GITHUB_PACKAGES_TOKEN`, and the current `gh` token cannot supply it.**
  `gh auth status` reports scopes `gist`, `read:org`, `repo` — GitHub Packages requires `read:packages`.
  Fix: `gh auth refresh -s read:packages`, then `export GITHUB_PACKAGES_TOKEN=$(gh auth token)`. This is
  an environment gate on *local verification only*; CI supplies its own token.
- **The ASP.NET Core targeting pack was missing from this machine and is now installed.** Arch splits
  `Microsoft.AspNetCore.App.Ref` out of `dotnet-sdk` into `aspnet-targeting-pack`, so every project with
  a `Microsoft.AspNetCore.App` framework reference failed restore with `NETSDK1226` regardless of the
  feed. Installed from the Arch archive at `10.0.10.sdk110-1` to match the installed SDK exactly (the
  repo mirrors had already moved on, so a plain `pacman -S` 404'd and a `-Sy` would have forced a
  partial upgrade). Not a repo change — recorded so the next session does not re-diagnose it.
- **`ActionLink` already existed in the web shared package** (`app/shared/src/types/common.ts`) with an
  identical shape, so the messaging feature imports it instead of declaring its own; re-exporting a
  second one broke the `@concertable/shared` build. This is the frontend counterpart of plan §4.7 and
  differs from it deliberately: on the backend the duplication is correct (module isolation), on the
  frontend the type is already shared.
- **`ReportMessageErrorTests` expectations are predictions, not observations.** The codes
  `report.message_not_found` and `report.message_invalid` were derived by hand from the
  `RESULT_PATTERN.md` derivation rule and have never been executed. First run may legitimately correct
  them.

## Resume prompt

```
cd /home/tommy/projects/csharp/Concertable
Read @plans/launch/OSA_REPORT_CONTENT_PLAN.md and @plans/launch/OSA_REPORT_CONTENT_PROGRESS.md and do what its `## Next Steps` says.
```
