# Online Safety Act — report-content flow progress

- Plan: `plans/launch/OSA_REPORT_CONTENT_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/osa-report-content`
- Worktree: `/home/tommy/projects/csharp/Concertable` (the main checkout — the kickoff instruction created the branch here rather than in an isolated worktree)
- Branch: `Feature/launch_osa-report-content`
- PR: [#572](https://github.com/Concertable/concertable/pull/572) (draft), branch merged up to `origin/main` at `c07c52678` and rebuilt green
- Dependency/package gates: none blocking. No open or red `chore/platform-sync-*` PR at branch time. The
  D2 package question is **settled** — see "Completed work".
- Last reconciled: 2026-08-15, after both phases were implemented and verified locally.

## Current state

**Both phases are implemented and locally verified.** Phase 1 (reporting route, record,
acknowledgement) and Phase 2 (admin moderation) are complete on this branch, the full
`api/Concertable.slnx` builds clean, Conversations unit tests are green, and all four web SPA builds
plus the frontend boundary lint are green. Migrations are re-scaffolded. What remains is delivery:
open the draft PR, let CI run the integration + E2E matrices, merge, and own the platform sync.

Three things worth knowing, all verified against source rather than assumed:

## Next Steps

1. **Watch draft PR #572's CI.** It owns the full solution build, the standalone service carves, and the
   complete unit/integration matrices against the exact remote head. Do not reproduce those locally.
2. **The integration tests have never executed** — Docker is not running on this machine and the
   integration matrix is PR CI's gate. Expect the first CI run to be where
   `ContentReportApiTests` and `ModerationApiTests` are proven. If one goes red, enter
   `integration-debug` at its narrowest failing scope rather than re-running the suite.
3. **Merge via `/merge` at the full-E2E tier.** Plan §11 is explicit: this change touches shared web
   code, a user-facing messaging flow, the request-authorization surface and the data model, so it
   fails every `skip-e2e` criterion. Let the merge queue run E2E; do not run it locally first.
4. **Own the post-merge `chore/platform-sync-*` PR to green.** This is an `api/**` change, so
   `publish-packages` republishes and `platform-sync` bumps every service's pin. Expected
   non-breaking — no cross-service published contract changed — so it should auto-merge. A red sync is
   this plan's to fix, in that PR.
5. **Close out only after platform sync is green:** record the terminal evidence here, then `git rm`
   this ledger and `OSA_REPORT_CONTENT_PLAN.md` together in a doc-only close-out landed through
   `/merge-docs`. The source PR never deletes its own recovery artifacts.

The roadmap lines have already been moved in the same commit as the shipping work, as required:

- `LAUNCH_ROADMAP.md` **§5** Swim-lane C row — marked `✅`, Month → `done`, annotated that the in-app
  route shipped and the published-email fallback remains with the legal pages.
- `LAUNCH_ROADMAP.md` **§7** checklist line — deliberately **left un-ticked**, annotated with what is
  live and what is outstanding. Ticking it would claim a compliance state we do not have, because
  Artifact 2's always-available published `report@` address depends on the solicitor-gated legal pages.

## Completed work

- Plan and ledger written from current `origin/main`; branch `Feature/launch_osa-report-content` created
  from `origin/main`.
- `launch/osa-report-content` key added to the roadmap §7 checklist line, and a Swim-lane C §5 row added
  for the split-out tenant-suspension work.
- **D2 settled, no fork.** `Reunion.Errors`, `Reunion.Validation` and `Reunion.AspNetCore` all publish
  `0.1.0-alpha.3` on nuget.org (`Reunion.AspNetCore` skips alpha.4 but has alpha.3), and `Dunet 1.16.2`
  is present. All four are declared in `api/Concertable.B2B/Directory.Packages.props` at those versions,
  so B2B stays on one Reunion version and is not bumped to Customer's alpha.6.
- **Phase 1 backend complete and building:** `ReportCategory`/`ReportOutcome`,
  `ContentReportEntity` (with `Resolve`), the EF configuration + `ConversationsDbContext` stance, a
  module `Repository<T>` base + `ContentReportRepository`, `IMessageRepository.GetByIdAsync`,
  `ReportMessageError`, `ContentReportValidators`, `ContentReportService`, `ContentReportNotifier`,
  `SafetySettings` + the `Safety` section in `appsettings.json`, the composition-root registrations, and
  the `MessageController` report endpoint with the `MessageResponse`/`MessageActions` HATEOAS mapping.
- **Phase 2 (admin moderation) complete and building:** `MessageEntity.Hide`/`Restore` +
  `HiddenAt`/`HiddenByUserId`, the hidden-message exclusion in both `MessageRepository` reads,
  `AdminConversationsDbContext` + registration, `AdminMessageRepository` /
  `AdminContentReportRepository`, `ModerationError`, `ResolveReportRequest`, `ContentReportDto` +
  mapper, `ModerationService`, and `ModerationController` under `[Admin]`. The `CR-{Id}` reference moved
  onto the entity as a derived member once both the email and the triage queue rendered it.
- **`api/Concertable.B2B/TECH_DEBT.md` entry added** for the `[Admin]` seam gaps (no admin SPA, no admin
  roles, uncached per-request DB hit).
- **Unit tests written and green (18):** `ContentReportEntityTests`, `ReportMessageErrorTests`,
  `ContentReportServiceTests`, the `MessageEntity` hide/restore tests, and the `MessageRepository`
  hidden-exclusion test.
- **Integration tests written, not yet executed:** `ContentReportApiTests` (204 + both emails,
  non-participant 404, anonymous 401, field-indexed 400, the inbound-only report link) and
  `ModerationApiTests` (hide removes from both inboxes and the unread count, restore reinstates,
  **tenant Owner 403 on every moderation endpoint**, anonymous 401, resolve records the outcome and a
  second resolve conflicts, cross-tenant triage queue).
- **Web slice complete and green** (plan §5): `ReportCategory`/`ReportMessageRequest`/`MessageActions`
  on the cross-platform `Message` type, `messageApi.reportMessage`, `useReportMessageMutation`,
  `ReportMessageDialog.tsx`, and the data-link-gated `Report` control in `Mailbox.tsx`.
- **UI E2E feature written** (plan §6.3): `ContentReport.feature`, `ContentReportSteps.cs`, and the
  report interactions on `MailboxPage`. Not run locally — the merge queue owns E2E.

## Verification

All local, on this branch, after both phases:

- `python .agents/hooks/plan_graph.py --root /home/tommy/projects/csharp/Concertable` — 0 errors.
- `dotnet build api/Concertable.slnx` — **succeeded**, 5 warnings (all pre-existing).
- Conversations unit tests — **18 passed, 0 failed**.
- `Concertable.B2B.ArchitectureTests` — 4 passed (module boundaries hold with the new
  `Conversations.Api → User.Api` attribute reference).
- `Concertable.Shared.Api.UnitTests` — 62 passed (includes the typed-result architecture rules; the new
  Result-based files do not mix carriers).
- `./initial-migrations.ps1` from `api/` — every other context reported *unchanged, kept existing
  migration id*; only `ConversationsDbContext` re-stamped, carrying `ContentReports` and the
  `HiddenAt`/`HiddenByUserId` columns.
- Web — `build:web-packages`, all four SPA builds (`web-customer`, `web-venue`, `web-artist`,
  `web-business`), and `npm run lint:boundaries` (no violations across all five cruised graphs).

**Not run locally, by policy or environment:** the integration matrix (Docker is not up here; PR CI owns
it) and all E2E (the merge queue owns it).

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
- **Local .NET restore needs `GITHUB_PACKAGES_TOKEN` with the `read:packages` scope — now resolved.**
  The `gh` token originally carried only `gist`, `read:org`, `repo`. Fixed by
  `gh auth refresh -h github.com -s read:packages` (device-code flow; it needs a real TTY, so it stalls
  if run non-interactively) and then `GITHUB_PACKAGES_TOKEN=$(gh auth token)` on each dotnet command.
  CI supplies its own token; this was local-only.
- **Reunion alpha.3's API differs from what `RESULT_PATTERN.md` documents** (the doc describes a later
  version). Three concrete differences, all found at first compile and all recorded because the next
  Reunion user in B2B will hit them: (1) `Ensure` is **predicate-only** — there is no validation-aware
  overload mapping `ValidationErrors` into an owned error case, so the validator is applied through the
  `TryGetErrors` guard the pattern doc sanctions for a standalone check; (2) `ErrorKind` has **no
  `Validation` member** — the validation definition is a distinct `ValidationError : ErrorDefinition`
  carrying the errors, with `Kind == Invalid`; (3) `ValidationErrors` exposes a keyed
  `IReadOnlyDictionary<string, IReadOnlyList<string>>`, not an enumerable of pairs.
  `BindAsync(Task<Result<T,E>>, Func<T, Task<UnitResult<E>>>)` **does** exist, so the operation kept its
  intended shape.
- **The error-code derivation predictions were right.** `report.message_not_found` and
  `report.message_invalid` are what Reunion actually derives from
  `ReportMessageError.MessageNotFound`/`.Invalid`, so no `[ErrorCode]` attribute was needed and the
  contract test stands as written.
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
- **The Conversations module had no `Repositories/Repository.cs` base**; one was added binding the shared
  base to `ConversationsDbContext`, per the repository convention, rather than hand-rolling CRUD on the
  new report repositories. `IMessageRepository` gained a single `GetByIdAsync`; converting it wholesale
  to the base was left alone as out-of-scope existing code.
- **PowerShell and `dotnet-ef` were missing on this machine** and are now installed as .NET global tools
  (`dotnet tool install --global PowerShell` / `dotnet-ef`), which needs no root and no AUR build.
  `./initial-migrations.ps1` also needs the **whole** solution restored first, not just the changed
  module, because it scaffolds every service.

## Resume prompt

```
cd /home/tommy/projects/csharp/Concertable
Read @plans/launch/OSA_REPORT_CONTENT_PLAN.md and @plans/launch/OSA_REPORT_CONTENT_PROGRESS.md and do what its `## Next Steps` says.
```
