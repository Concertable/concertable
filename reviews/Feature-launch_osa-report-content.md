# Code review — Feature/launch_osa-report-content

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `c3172e835d004792b0d94e4c4f430b3129620a56`  _(2026-08-15)_
**Security-reviewed up to commit:** `c3172e835d004792b0d94e4c4f430b3129620a56`  _(2026-08-15)_

> Range reviewed: `c07c526..b06e0a8` (13 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

### Layer 2 — Concertable lenses

- [x] **CV1 — HIGH — conventions (Result pattern)** — `api/Concertable.B2B/src/Modules/Conversations/Concertable.B2B.Conversations.Application/Errors/ModerationError.cs:1`
  `ModerationError` has three cases (`MessageNotFound`, `ReportNotFound`, `AlreadyResolved`) and **no
  definition-contract test**. `api/agents/RESULT_PATTERN.md` is mandatory here: *"Every error union has an
  exact definition contract test for every case. Hard-code the expected code, message, semantic kind, and
  the preserved validation fields."* `ReportMessageError` has one (`ReportMessageErrorTests`); this union
  was missed. **Fix:** add `Tests/.../Errors/ModerationErrorTests.cs` asserting the exact `Code`,
  `Message` and `ErrorKind` for all three cases, hard-coded, never derived with the production helper.

- [x] **TEST1 — MEDIUM — test coverage** — `api/Concertable.B2B/src/Modules/Conversations/Concertable.B2B.Conversations.Infrastructure/Services/ModerationService.cs:29`
  `ModerationService` has no unit tests, and `ModerationApiTests` never exercises a not-found id, so the
  `MessageNotFound` and `ReportNotFound` branches of hide/restore/resolve have zero coverage — three of
  the operation's four expected outcomes are unasserted. **Fix:** add `ModerationServiceTests` covering
  hide/restore/resolve against an unknown id (each yielding its named error and performing no
  `SaveChangesAsync`), plus the already-resolved conflict at the service level.

- [x] **PERF1 — LOW — efficiency** — `api/Concertable.B2B/src/Modules/Conversations/Concertable.B2B.Conversations.Infrastructure/Repositories/AdminContentReportRepository.cs:11`
  `GetQueueAsync` materialises **every** content report across every tenant with `.ToListAsync()`, and
  `GET /api/Moderation/reports` returns it unpaginated. Reports are never deleted (hide-never-delete is
  the whole design), so this grows without bound. **Fix:** mirror `MessageController.GetForUser` — take
  `PageParams`, return `IPagination<ContentReportDto>` via the existing `ToPaginationAsync`.

- [x] **BUG1 — LOW — correctness** — `api/Concertable.B2B/src/Modules/Conversations/Concertable.B2B.Conversations.Domain/Entities/ContentReportEntity.cs:70`
  `content[..MaxExcerptLength]` slices by UTF-16 code unit, so a message whose 500th unit falls inside a
  surrogate pair (any emoji, and messages are free text) is truncated to a lone surrogate — a malformed
  character in the one field that exists to evidence what was reported. **Fix:** step back off a trailing
  high surrogate before slicing (`char.IsHighSurrogate(content[MaxExcerptLength - 1])`), or truncate on
  `StringInfo`/rune boundaries.

### Layer 1 — Native review (`code-reviewer`, effort high)

Eight findings returned; five cleared the confidence bar and were fixed, three were dropped with reasons.

- [x] **NAT1 — MEDIUM — error handling** — `Conversations.Infrastructure/Services/ContentReportNotifier.cs`
  A missing reporter email threw `UnauthorizedAccessException`, which `GlobalExceptionHandler` maps to
  **401**, and the SPA interceptor (`app/shared/src/lib/client.ts`) treats any 401 as session expiry and
  calls `removeUser()` — so a data gap silently signed the reporter out *after* their report had already
  committed. Now logged and skipped; the safety-inbox mail still sends.
- [x] **NAT2 — MEDIUM — atomicity** — `Conversations.Infrastructure/Services/ContentReportService.cs`
  The report row commits, then emails send inline, so a transport failure failed a request whose write
  was already durable — and the retry filed a second report. The notifier call is now wrapped: the
  failure is logged and the operation still succeeds, because the persisted record is what the duty
  turns on, not the mail.
- [x] **NAT4 — MEDIUM — correctness** — `Conversations.Domain/Entities/MessageEntity.cs`
  `Restore()` nulled `HiddenAt`/`HiddenByUserId`, so after an appeal succeeded there was **no evidence
  the content was ever hidden, by whom, or when** — the exact record hide-not-delete exists to keep, and
  `TECH_DEBT.md`'s claim that every moderation action stamps actor+timestamp held only for `Resolve`.
  Now `Restore(byUserId, at)` stamps `RestoredAt`/`RestoredByUserId` and never clears the hide; the read
  filter derives visibility from the two, so a re-hide still works.
- [x] **NAT6 — LOW — correctness** — `Conversations.Infrastructure/Services/ContentReportService.cs`
  "You cannot report your own tenant's message" existed **only** in link generation, so a crafted POST
  with an outbound message id recorded `ReporterTenantId == ReportedTenantId` and mailed the safety inbox
  naming the reporter as the offender. The server now enforces it, answering `MessageNotFound` so it
  stays consistent with the D5 privacy answer.
- [x] **NAT7 — LOW — error handling** — `app/web/shared/.../ReportMessageDialog.tsx`
  The details textarea had no cap while the server rejects >2000 chars, so a long paste became an opaque
  "please try again" loop. `maxLength` added.
- [wontfix] **NAT3 — dedupe repeat reports.** Contradicts plan **D7**, a deliberate decision: suppressing
  or collapsing repeats would need a per-message "reported by me" read on every inbox page, and an OSA
  reporting route must never be *harder* to reach. A second report is data, not an error; the queue
  groups by message. (Also below the bar at ~75.)
- [wontfix] **NAT5 — concurrency token on resolve.** Requires two simultaneous resolves; there is no
  admin SPA at all (moderation is curl/Swagger by one operator) and expected volume is near zero, so it
  is not hit in practice. Revisit if an admin UI ships.
- [wontfix] **NAT8 — exclude hidden messages from `GetByIdAsync`.** ~60 confidence and the reviewer asked
  for an explicit decision, so: **accepted deliberately.** A hidden message never appears in an inbox, so
  no link is offered; a report arriving for one is a reporter acting on what they saw before moderation,
  which is legitimate evidence rather than something to reject.

### Layer 1b — Security (diff touches Controllers + Authorization)

No HIGH or MEDIUM findings. Paths traced: IDOR on the report endpoint (tenant-filtered lookup → 404 for a
non-participant, test-pinned); privilege escalation into moderation (`[Admin]` platform axis, tenant Owner
403 / anonymous 401, test-pinned); reachability of the unfiltered writable `AdminConversationsDbContext`
(only via the two admin repositories → `ModerationService` → `[Admin]` controller); email header injection
(subjects interpolate only `CR-{int}`; user text stays in the body); injection/deserialization (EF-
parameterised, `int` route ids, no dynamic SQL); enumeration (404-not-403 is deliberate); XSS (React, no
`dangerouslySetInnerHTML`).

- [wontfix] **CONV1 — LOW — conventions** — `Conversations.Infrastructure/Services/ModerationService.cs`
  Hand-built `new Pagination<ContentReportDto>(...)` where `IPagination<T>.Select` already exists —
  **fixed**, but the underlying cause is that `Select` sits in `Concertable.DataAccess.Infrastructure`,
  unreachable from Api projects and undiscoverable from the layers that can reach it (eight-plus
  hand-rolled copies repo-wide). Moving it to `Concertable.Contracts` is a publish-first cut-over, logged
  in `api/TECH_DEBT.md`.

- [wontfix] **DUP1 — LOW — reuse/duplication** — `api/Concertable.B2B/src/Modules/Conversations/Concertable.B2B.Conversations.Api/Responses/ActionLink.cs:3`
  `ActionLink` is now declared identically in `Concert.Api` and `Conversations.Api`. It belongs once in
  `Concertable.Shared.Api` — it is a generic HATEOAS wire primitive, and the frontend has always had a
  single shared `ActionLink`. **Not fixable in this PR:** `Concertable.Shared.Api` is consumed as a
  published package pinned to `ConcertablePlatformVersion`, so a type added to it is invisible to
  consumers until published and platform-synced. Logged as a publish-first cut-over in
  `api/TECH_DEBT.md`; the plan's "hoisting it would be cross-module coupling" justification is corrected
  there.

### Fixes applied in this review

Layer-2 findings, all fixed on this branch before merge:
`ModerationErrorTests` (3 definition contracts, codes confirmed by running them), `ModerationServiceTests`
(5 cases covering both not-found branches, the already-resolved conflict, and the hide stamp), the
paginated triage queue (`IPagination<ContentReportDto>` + `PageParams`, mirroring
`MessageController.GetForUser`), and surrogate-safe excerpt truncation. Conversations unit tests: **24
passed**; `api/Concertable.slnx` builds clean.

### Checked and clean

- **Microservice isolation (Lens B):** no cross-data-service reference; the only new cross-project edge is
  `Conversations.Api → User.Api` for `[Admin]`, an `Api → Api` attribute reference inside one service that
  `VenueController` already makes.
- **Module boundaries (Lens C):** no `IConversationsModule` change (nothing outside Conversations calls
  reporting); repositories inherit the module `Repository<T>` base; impls stay `internal`; the new
  `public` entity/enums match the existing B2B entity style.
- **Seeding (Lens D):** no seeder touched.
- **Migrations:** re-scaffolded via `./initial-migrations.ps1`, not additive; every other context kept its
  id. The derived `Reference` property is correctly not mapped to a column.
- **Tenancy:** `ContentReportEntity` is filtered on the tenant-filtered context and unfiltered on the new
  `AdminConversationsDbContext`, which is registered without `VenueArtistTenantInterceptor` as required.


## Incremental review — 2026-08-15

Range `eff10041..73564c59` plus the fix below. Only the branch's own changes are in scope; the rest of
the range is merged `main` (platform syncs, the Reunion action-result fix, the B2B test-isolation
refactor, and the frontend doc-parity PR #579).

Scope: `app/shared/src/features/messaging/{schemas/reportMessageRequestSchema.ts,types.ts,index.ts}`,
`app/web/shared/src/features/messaging/components/{ReportMessageDialog,Mailbox}.tsx`, the restored User
migration, and the `TECH_DEBT`/conventions doc edits.

**Layer 1 (native) was re-run after the session limit reset** and returned 8 findings; 7 cleared the
bar and were fixed, 1 dropped. Holding the merge for it was the right call — NAT1 below is a genuine
compliance defect that the inline self-review missed.

- [x] **NAT9 — MEDIUM — correctness (dead validation path)** — `ReportMessageDialog.tsx:88`
  `maxLength={2000}` on the textarea hard-capped input at exactly the schema's limit, so
  `safeParse` could never fail the length rule: `detailsError` was always `undefined`, the inline error
  never rendered, and `!parsed.success` never disabled submit. The entire inline-error affordance
  `CODE_PATTERNS.md` "The write boundary is a zod parse" asks for was unreachable. It also made a long
  paste **truncate silently** — 3000 characters in, 1000 gone, no explanation — which is worse than the
  400 the cap was added to avoid. Introduced by stacking the earlier `maxLength` fix and the later zod
  schema without reconciling them. **Fixed:** cap removed, so the parse gates submit and reports the
  message inline; the field also now carries `aria-invalid`/`aria-describedby` pointing at it.

- [x] **NAT1 — MEDIUM — correctness (compliance)** — `ReportMessageDialog.tsx:37`
  The category buffer initialised to `categories[0]` — **"Illegal content"**, the most serious OSA
  reason — so a reporter who never touched the Select filed an illegal-content report, and the zod gate
  was cosmetic for the only required field. That would systematically inflate the gravest category in
  the triage queue and misclassify reports in the record an information request would read. **Fixed:**
  no default, `placeholder="Choose a reason"`, and the parse fails until a reason is chosen.
- [x] **NAT2 — MEDIUM — error handling** — `ReportMessageDialog.tsx:46`
  Only `details` issues were surfaced, so with NAT1 fixed "no reason chosen" became a silent dead-end:
  submit disabled, nothing rendered. **Fixed** with a muted hint under the Select rather than a
  destructive error — the form should explain why submit is unavailable without shouting at a field the
  user has not touched yet.
- [x] **NAT3 — MEDIUM — correctness** — `ReportMessageDialog.tsx:50`
  Cancel was disabled while pending, but Escape, an outside click and `DialogContent`'s own close button
  still fired `onOpenChange(false)`, which drops `reportingMessageId` and unmounts the dialog mid-POST —
  the report lands, the acknowledgement never shows, and the reporter can immediately file a duplicate.
  **Fixed:** `close` early-returns while pending, and the escape/outside/X routes are suppressed too.
- [x] **NAT4 — MEDIUM — reuse** — `ReportMessageDialog.tsx:37`
  The component owned the buffer, the parse, the buffer→request mapping and the `mutate` call — the
  "mutation wiring in a component" anti-pattern in `CODE_PATTERNS.md`. **Fixed:** `useReportMessage`
  facade following the `useInviteMember` precedent, returning `{ validate, submit, isPending, isSuccess,
  isError }`; it sits in `app/shared` rather than the web tier because nothing about it is web-only.
- [x] **NAT5 — LOW — error handling** — `useMessageQuery.ts:27`
  A failed report was reported twice — the global `MutationCache.onError` toast *and* the dialog's inline
  banner, in different words. **Fixed** with the documented `meta: { silenceErrors: true }` opt-out, so
  the inline banner is the single deliberate surface.
- [x] **NAT6 — LOW — simplification** — `useMessageQuery.ts:28`
  `messageId` is fixed for the hook's lifetime yet was threaded through the mutation variables, against
  the "bind everything constant, take only per-submit variables" litmus. **Fixed:**
  `useReportMessageMutation(messageId)` closes over it.
- [x] **NAT7 — LOW — simplification** — `ReportMessageDialog.tsx:52`
  The reset block was unreachable: `onOpenChange(false)` unmounts the dialog in the same batched render,
  so the resets were discarded updates on an unmounting component. **Fixed:** deleted; the conditional
  mount is what guarantees fresh state per message.
- [wontfix] **NAT8 — focus restoration after the popover dismisses.** ~60 confidence and the reviewer
  states Radix's fallback target is inferred rather than observed. The mount structure it checked is
  correct (the dialog is a sibling of `PopoverContent`, not nested). Revisit if a keyboard user reports
  losing focus.

### Checked and clean (this delta)

- **Frontend conventions** (`app/agents/CODE_CONVENTIONS.md`, finally loadable): absent values are
  `undefined`; the textarea stays controlled via `value={details ?? ""}`; reads carry no `Dto`/`Response`
  suffix; the write input is an `XRequest`; casing is camelCase; object shapes are `interface` and the
  category union is a `type`.
- **Frontend patterns** (`app/agents/CODE_PATTERNS.md`): the schema lives in `features/messaging/schemas/`
  and is the `z.infer` source for both types, so drift from the backend validator is a compile error;
  `parsed.data` is passed with no `!` bang; `reportMessage` joined the existing `messageApi` object rather
  than minting a second client; the raw hook carries the `Mutation` suffix; no server state touches
  `useEffect`; no identity check inside the shared tier — the Report control is gated on the server's
  action link.
- **Dialog lifecycle:** closing sets `reportingMessageId` to `undefined`, which unmounts the dialog, so
  category/details/mutation state cannot leak into the next report; `close()` resets explicitly as well.
- **Mutation wiring:** reporting changes no cached list — no message is written into the thread and the
  unread count is untouched — so the absence of an invalidation is correct, not an omission.
- **E2E selectors** still match the component: `message-report-trigger`, `report-category`,
  `report-details`, `report-submit`, `report-confirmation`.
- **Migration restore** returns the User module's three files to `main`'s exact content; the migration
  diff against `main` is Conversations-only.


### Marker moved to `12b43c821` — merge of `origin/main` only

47 commits of `main` (four platform syncs to `0.1.0-alpha.0.1009`, the camelCase-JSON-enum helper, the
DataAccess repository hierarchy expansion, and a Conversations dashboard API) merged with no conflicts.
The branch's own diff against `main` is unchanged, so no new code entered review scope. Re-verified
against the new base: full solution build, 29 Conversations unit tests, the integration test project
compiles, four web builds, boundary lint, `plan_graph` and `docs_reachability`.

Checked specifically because it could have moved this feature's wire contract: `AddApplicationJson`
(camelCase enums, `allowIntegerValues: false`) is **not** adopted by `Concertable.B2B.Web`, which still
registers the plain `JsonStringEnumConverter`. So `ReportCategory` stays PascalCase on the wire and the
`"Details"` ModelState key in the integration test remains correct.


### Marker moved to `c3172e835` — main merges and the Reunion alpha.8 jump

`main` moved ~153 commits and conflicted, which took the PR `DIRTY` and silently disarmed auto-merge.
Two conflicts, both resolved to `main`: `Directory.Packages.props` (main has since added the Reunion
sub-packages itself and moved B2B to **alpha.8**, dropping FluentResults — superseding this plan's D2
alpha.3 pin) and `app/web/TECH_DEBT.md` (both sides appended a different entry; both kept).

The branch then could not build on two Concert files consuming `RefundReasonCodes`, which existed in
`Payment.Contracts` source but not in the published package at pin `1009` — `main` mid-cut-over, not a
defect here. Resolved when sync PR #598 landed and the pin advanced to `1015`.

No branch code changed. Re-verified on the new base: full solution build, 29 Conversations unit tests,
integration project compiles, four web builds, boundary lint, both hook guards, migration audit clean.
**The Reunion alpha.3 → alpha.8 jump needed no code change** — `OrFailure`, `BindAsync`, `TryGetError`,
`Success` and `ToNoContentOrProblem` all survive the version move.
