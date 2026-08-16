# Online Safety Act — report-content flow

> **Next steps live in @plans/launch/OSA_REPORT_CONTENT_PROGRESS.md → `## Next Steps`** — this plan
> holds the design and outstanding phases only, no next-action prose.

## 1. Outcome

Concertable is an in-scope user-to-user service under the Online Safety Act 2023 because it carries
artist↔venue messaging. This plan builds the **only engineering slice** of the compliance pack
([`OSA_COMPLIANCE.md`](OSA_COMPLIANCE.md) "Code slice"): the illegal-content **reporting route**, the
**internal record** of each report and the action taken on it, and the **admin moderation capability**
that makes acting on a substantiated report real rather than aspirational.

Three capabilities, one coherent slice:

1. **Report a message.** A `Report` action on inbound messages in both manager SPAs → category +
   free text → a structured report emailed to the configured safety inbox, plus the automated
   acknowledgement to the reporter that Artifact 3 requires on submit.
2. **A persisted report record.** Artifact 3 requires "an internal record of each report and action"
   for an Ofcom information request; Artifact 4's appeals process needs a decision reference. Email
   alone cannot serve either.
3. **Admin moderation.** A platform admin can **hide** a message (and **restore** it, which Artifact 4's
   appeal right requires) and **resolve** a report with an outcome, actor and timestamp — closing the
   report → triage → action → record loop.

**Tenant suspension is deliberately not in this plan.** The analysis and the gate that splits it are in
§7; it is a real gate, not a scope reduction.

## 2. What OSA duty this closes, and what it does not

Closes: Artifact 2 (reporting route, in-app half), the Artifact 3 acknowledgement + internal record,
and the Artifact 1 mitigation "ability to remove a message". Message removal *is* the illegal-content
takedown duty — the content stops being encounterable.

Does **not** close the launch-readiness item for the reporting route **in full**. Artifact 2's
always-available fallback is a published `report@`/`safety@` address on the footer legal pages, and
those page routes are solicitor-gated and do not exist ([`LAUNCH_CHECKLIST.md`](LAUNCH_CHECKLIST.md)
Phase 3 — the T&Cs cluster). The in-app route ships here; the published fallback ships with the legal
pages, so the launch item is only partially satisfied by this work and must not be recorded as green
(§9, §12).

## 3. Where it lives — module ownership and the boundaries crossed

The reported artefact is a `MessageEntity`, so **Conversations owns everything**: the report entity, the
submission endpoint, the moderation endpoints and the admin data stance. The other two modules are
touched only through seams that already exist.

| Module | What this plan uses | Why it is not a boundary violation |
|---|---|---|
| **Conversations** (`api/Concertable.B2B/src/Modules/Conversations/`) | Owns `ContentReportEntity`, `MessageEntity.HiddenAt`, both controllers, both data stances | The module that owns the data owns the operation |
| **User** | `[Admin]` (`User.Api/Authorization/AdminAttribute.cs`) applied to the moderation controller | An `Api → Api` attribute reference, exactly as `VenueController.cs:52` already does |
| **Tenant** | `ITenantContext` for the acting tenant; `IVenueArtistTenantScoped` write-guard | Already the ambient contract every B2B module consumes |

No `IConversationsModule` change: nothing outside Conversations calls into reporting. No cross-service
published contract changes, therefore **no publish-first gate** (§12).

**Authorization axis.** Moderation is gated on `[Admin]`, never on tenant RBAC
(`TenantRole`/`HasPermissionAttribute`). Tenant RBAC is scoped to *one tenant* — a venue Owner holding
`MessagesRead` in their own tenant must never be able to hide a message in someone else's thread. The
axes are orthogonal and the platform axis is the correct one. An integration test asserts a tenant Owner
gets **403** on every moderation endpoint (§6.2), so the wrong axis cannot be wired in later by accident.

**Honest assessment of the `[Admin]` seam before extending it.** It is thin: a bare `Sub` column
(`AdminProfileEntity`), no roles, no scoping, an uncached `UserDbContext` query per request
(`AdminProfileHandler`), provisioning only via registration through the `admin` client-id
(`CredentialRegisteredHandler`) or `UserTestSeeder`, applied in exactly one place today, and **no admin
SPA at all**. Two conclusions, and they differ:

- As an **authorization axis** it is correct and sufficient. It answers "is this caller a platform
  operator?", which is precisely the question these endpoints ask. Building tenant-agnostic admin RBAC
  inside an OSA compliance feature would be the wrong place to decide it.
- As an **operations surface** it is not sufficient, and this plan compensates *in its own data* rather
  than by growing the seam: every moderation action stamps the acting user id and timestamp onto the
  report record, so the audit trail Artifact 3 needs exists regardless of how thin the authz is.

The seam's real gaps (uncached per-request DB hit; no roles; no admin UI, so moderation is
Swagger/curl-driven at launch) are logged in `api/Concertable.B2B/TECH_DEBT.md` as part of this work.
They are acceptable at the pack's expected near-zero volume and are not this plan's to fix.

## 4. Design — backend

### 4.1 The report record (`Conversations.Domain`)

**`Domain/Enums/ReportCategory.cs`** — `IllegalContent`, `Harassment`, `Fraud`, `Spam`, `Other`, with
`[JsonConverter(typeof(JsonStringEnumConverter))]`. Internal, in Domain: nothing cross-module consumes
it, so it earns no place in `.Contracts` (`MODULAR_MONOLITH_RULES.md` — "a purely internal helper has no
Contracts"). `MessageSenderKind` in `Application/DTOs/MessageDtos.cs` is the precedent for an internal
string-serialized enum on the wire.

**`Domain/Enums/ReportOutcome.cs`** — `NoActionTaken`, `ContentRemoved`, `ReferredToLegal`. What the
operator decided, recorded for Ofcom and for the appeal.

**`Domain/Entities/ContentReportEntity.cs`** — `IIdEntity, IVenueArtistTenantScoped`:

| Field | Purpose |
|---|---|
| `Id` | PK; the appeal/decision reference is rendered from it (D4) |
| `MessageId` | The reported message |
| `VenueTenantId`, `ArtistTenantId` | The thread pair — satisfies `IVenueArtistTenantScoped` |
| `ReporterTenantId`, `ReportedTenantId` | Who reported whom, denormalised so the record stands alone |
| `ReportedByUserId` | The member who submitted it |
| `Category`, `Details` | The submitted reason and free text |
| `MessageExcerpt` | Content snapshot at report time, truncated to 500 chars |
| `SubmittedAt` | From `TimeProvider` |
| `Outcome`, `ResolvedAt`, `ResolvedByUserId`, `ResolutionNotes` | Null until an admin resolves it |

`Create(...)` is a static factory in the module's house style (`MessageEntity.Create`), taking the
message and the acting identity. `Resolve(outcome, resolvedByUserId, notes, at)` is the single
transition; it throws `DomainException` if already resolved.

`MessageExcerpt` exists because a report must survive the moderation it triggers: once the message is
hidden, the evidence Ofcom would ask for must still be readable from the report itself. It is a
snapshot, never a live read.

### 4.2 Hiding a message (`Conversations.Domain`)

**`Domain/Entities/MessageEntity.cs`** gains `DateTime? HiddenAt`, `Guid? HiddenByUserId`, and two
transitions: `Hide(byUserId, at)` and `Restore()`. Content is **never deleted** — Artifact 4's appeal
and any Ofcom information request both need the original, and the report's excerpt is a copy, not the
record of authority. Hiding is a visibility change.

`MessageRepository.GetByTenantIdAsync` and `GetUnreadCountByTenantIdAsync` both exclude
`HiddenAt != null`, so a hidden message leaves the inbox and stops counting as unread for every member
of both tenants in the same request.

### 4.3 Data stances (`Conversations.Infrastructure/Data`)

Per `CODE_PATTERNS.md` "Tenancy is composed, never subtracted", and mirroring `AdminVenueDbContext`:

- **`ConversationsDbContext`** (existing, tenant-filtered) gains `DbSet<ContentReportEntity>` and
  `modelBuilder.ApplyVenueArtist<ContentReportEntity>(this)`. Reports about a thread are private to that
  thread's pair; the filter is the fail-closed default for any future participant-facing read.
- **`AdminConversationsDbContext`** (new, `: AdminDbContext`) — the same anemic
  `ConversationsConfigurationProvider`, no tenancy, writable. Registered with `AuditInterceptor`,
  `TenantInterceptor` and `IDomainEventDispatchInterceptor` — **not** `VenueArtistTenantInterceptor`,
  which would try to stamp a pair onto a tenant-less admin write.

Both entities therefore have two genuine stances, which is what earns the qualifier:
`ContentReportRepository` / `AdminContentReportRepository`, and `MessageRepository` /
`AdminMessageRepository`.

### 4.4 Submission service (`Conversations.Application` + `.Infrastructure`)

**`Application/Errors/ReportMessageError.cs`** — a Dunet union in the shape `RESULT_PATTERN.md`
mandates:

```csharp
[Union(EnableImplicitConversions = false)]
internal abstract partial record ReportMessageError : IError
{
    public ErrorDefinition Definition => this switch
    {
        MessageNotFound => ErrorDefinition.NotFound<MessageNotFound>(),
        Invalid(var errors) => ErrorDefinition.Validation<Invalid>(errors)
    };

    public partial record MessageNotFound;
    public partial record Invalid(ValidationErrors Errors);
}
```

**`Application/Validators/ContentReportValidators.cs`** — returns Reunion `ValidationResult`, following
`Customer.Ticket.Infrastructure/Validators/TicketValidator.cs`: category `IsInEnum`-equivalent, details
≤ 2000 chars. Details are optional — the category may say everything, and an OSA reporting route must
never be harder to complete than it has to be.

**`IContentReportService.SubmitAsync(int messageId, ReportMessageRequest request)` →
`Task<UnitResult<ReportMessageError>>`.** The composition:

```csharp
public Task<UnitResult<ReportMessageError>> SubmitAsync(int messageId, ReportMessageRequest request) =>
    repository.GetByIdAsync(messageId)
        .OrFailure<MessageEntity, ReportMessageError>(new ReportMessageError.MessageNotFound())
        .Ensure(_ => validator.Validate(request), errors => new ReportMessageError.Invalid(errors))
        .BindAsync(message => RecordAndNotifyAsync(message, request));
```

The lookup runs on the **tenant-filtered** context, so a tenant that is not party to the thread gets
`None` → `MessageNotFound` → **404**. That is both the non-participant rejection and the right privacy
answer: a 403 would confirm the message exists (D5).

`RecordAndNotifyAsync` persists the report, then calls the notifier. It is a private multi-statement
operation returning `Task<UnitResult<...>>`, which is why it is `BindAsync` and not `MapAsync`.

**`IContentReportNotifier` / `ContentReportNotifier`** (Application interface, Infrastructure impl —
the `IConversationsNotifier`/`ConversationsNotifier` precedent) sends exactly two emails through the
existing `IEmailTransport.SendEmailAsync`, the same seam `Messenger` uses:

1. **To the safety inbox** — reporter tenant, reported tenant, message id, category, submitted-at,
   the excerpt, and the reference. Plain text; this is an internal ops email.
2. **To the reporter** (`ICurrentUser.Email`, as `ApplicationNotifier` already does) — the reference
   and confirmation that it will be reviewed. **No SLA figures in code**: the working-day targets live
   in the solicitor-validated Artifact 3 table, and a second copy in a string literal would drift the
   day that table changes.

The reported party is **never** notified through the thread, and no message is written into the
conversation. A report is not a conversation event.

### 4.5 Configuration

**`Conversations.Infrastructure/SafetySettings.cs`**, copying `Concert.Infrastructure/LegalSettings.cs`:

```csharp
public sealed class SafetySettings
{
    public const string SectionName = "Safety";
    public string ReportInboxEmail { get; set; } = null!;
}
```

Bound with `services.Configure<SafetySettings>(configuration.GetSection(SafetySettings.SectionName))`
in the Conversations composition root, injected as `IOptions<SafetySettings>`.

`api/Concertable.B2B/src/Concertable.B2B.Web/appsettings.json` gains:

```json
"Safety": { "ReportInboxEmail": "safety@concertable.invalid" }
```

The domain is not registered yet (`LAUNCH_CHECKLIST.md` Phase 0), so no real address can be set. The
placeholder uses the RFC 2606 reserved `.invalid` TLD so a mis-deployed environment provably cannot mail
a stranger. This is an inbox address, not a credential — no secret is committed, and the value is
overridden per environment when the domain exists. `appsettings.E2E.json` and
`appsettings.Integration.json` layer over `appsettings.json`, so both inherit it with no new file.

### 4.6 HTTP surface

Two controllers, because ASP.NET **ANDs** stacked authorize attributes: an `[Admin]` action inside the
class-level `[HasPermission(SharedPermissions.MessagesRead)]` on `MessageController` would demand a
tenant membership the platform admin does not have.

**`MessageController`** (existing, unchanged gating) gains:

```
POST /api/Message/{id}/report   → 204 | 400 | 404 | 401
```

returning `result.ToNoContentOrProblem()` (`Reunion.AspNetCore.Mvc`). The reference reaches the reporter
in the acknowledgement email, which Artifact 3 requires anyway — so the operation is a `UnitResult`, the
smallest truthful carrier, and no one-use response DTO is minted (D6).

**`ModerationController`** (new, `[Admin]` at class level, `[Route("api/[controller]")]`):

```
GET   /api/Moderation/reports                 → the triage queue
POST  /api/Moderation/messages/{id}/hide      → 204
POST  /api/Moderation/messages/{id}/restore   → 204
POST  /api/Moderation/reports/{id}/resolve    → 204   (outcome + notes)
```

`restore` exists because Artifact 4 gives a user the right to appeal content removal; an appeal process
with no mechanical way to reverse a wrong decision is not a process.

### 4.7 HATEOAS gating

`api/AGENTS.md` names HATEOAS as a reason a `Response` type is warranted, and `Concert.Api/Responses`
is the worked precedent (`ApplicationResponse` + `ApplicationActions` + `ActionLink`). So
`Conversations.Api/Responses/MessageResponses.cs` introduces `MessageResponse`,
`MessageActions(ActionLink? Report)` and a module-local `ActionLink`, mapped from `MessageDto` in an
Api mapper; `MessageController.GetForUser` returns `IPagination<MessageResponse>`.

The `Report` link is present **iff the message is inbound** — `Sender.Kind == MessageSenderKind.Org`,
which the service already derives from `SenderTenantId != activeTenantId`. No new marker and no
re-derivation: the question is already answered by a value on the DTO. You cannot report your own
tenant's message. Hidden messages never reach the mapper because the repository excludes them.

`ActionLink` is duplicated rather than shared with Concert's: it is a three-line internal record, and
hoisting it into a shared package to avoid the duplication would create exactly the cross-module
coupling `MODULAR_MONOLITH_RULES.md` forbids.

## 5. Design — web

`app/web/shared/src/features/messaging/components/Mailbox.tsx` is shared by both b2b manager SPAs, so
this is **one component change, not two**.

Cross-platform layer (`app/shared/src/features/messaging/`, the `@concertable/shared` package):

1. **`types.ts`** — `ReportCategory` union, `ReportMessageRequest`, `ActionLink`, and
   `actions?: { report?: ActionLink }` on `Message`.
2. **`api/messageApi.ts`** — `reportMessage(messageId, request)` posting to `/message/{id}/report`.
3. **`hooks/useMessageQuery.ts`** — `useReportMessageMutation`.

Web layer (`app/web/shared/src/features/messaging/`):

4. **`components/ReportMessageDialog.tsx`** (new) — a `Dialog` with a category `Select`, a details
   `Textarea`, submit, and a post-submit confirmation. Web-only, so it lives beside `Mailbox.tsx`
   rather than in the cross-platform package; exported from the feature `index.ts`.
5. **`components/Mailbox.tsx`** — render a small `Report` control on a message **only when
   `message.actions?.report` is present**, opening the dialog for that message.

`data-testid`s for E2E: `message-report-trigger`, `report-category`, `report-details`, `report-submit`,
`report-confirmation`.

No `isVenueManager` branching anywhere — the report route is identical for both personas.

## 6. Tests

### 6.1 Unit (`Conversations.UnitTests`)

- `ContentReportEntityTests` — `Create` snapshots the excerpt, pair, reporter and reported tenant;
  excerpt truncates at 500 chars; `Resolve` stamps outcome/actor/time; resolving twice throws.
- `MessageEntityTests` — `Hide` stamps `HiddenAt`/`HiddenByUserId`; `Restore` clears them.
- `ContentReportServiceTests` — the success path sends **exactly two** emails, to the configured safety
  inbox and to the reporter, and persists one report; an unknown/invisible message yields
  `MessageNotFound`; over-long details yield `Invalid` carrying the field key.
- `ReportMessageErrorTests` — the exact definition contract for **every** case (code, message, kind, and
  the preserved validation fields), hard-coded, never derived with the production helper. Mandatory per
  `RESULT_PATTERN.md`.
- `MessageRepositoryTests` — hidden messages are excluded from the inbox page and from the unread count.

### 6.2 Integration (`Conversations.IntegrationTests`, over `ConversationsApiFixture`)

Report endpoint:
- a participant posting a valid report → **204**, one `ContentReportEntity` persisted, and
  `MockEmailSender.Sent` contains the safety-inbox mail and the reporter's acknowledgement;
- a tenant **not party to the thread** → **404** (the non-participant rejection);
- anonymous → **401**; invalid category / over-long details → **400** with field-indexed
  `ValidationProblemDetails`;
- the inbox payload carries the `report` action link on the counterparty's message and **omits it** on
  the tenant's own outbound message.

Moderation endpoints (`fixture.SeedState.Admin`, the pattern `VenueApiTests` already uses):
- admin `hide` → 204, and the message then disappears from **both** participants' inbox and unread
  count; `restore` puts it back;
- **a tenant Owner gets 403 on every moderation endpoint** — the wrong-axis guard;
- anonymous → 401;
- `resolve` records outcome, notes, resolving user and timestamp, and a second `resolve` on the same
  report → 409/400 per the domain guard;
- `GET /api/Moderation/reports` returns reports across tenants for an admin and 403 for a tenant member.

### 6.3 UI E2E

`api/Concertable.B2B/tests/E2ETests/Concertable.B2B.E2ETests.Ui/Features/ContentReport.feature`,
`@VenueManager`, one scenario: the venue owner opens their mailbox, reports the artist's message with a
category and details, and sees the confirmation. Steps in `Steps/ContentReportSteps.cs`; the dialog
interactions extend `PageObjects/MailboxPage.cs`.

Moderation has **no UI**, so it is integration-tested only — stated here so its absence from E2E is a
recorded decision rather than a gap.

## 7. Tenant suspension — the analysis, and why it splits

Tommy's 2026-08-14 decision was to build admin moderation rather than accept the pack's "manual DB/ops
is acceptable" option. Message hide/restore/resolve is built here. **Tenant suspension is split into its
own item.** The reasoning, in full, because the split must be justified rather than asserted.

### 7.1 Enforcement — the seam is not token issuance

The kickoff named token issuance as an enforcement point. In B2B it is not available and would be the
weaker choice anyway:

- B2B tokens are **identity-only** since `Feature/RetireRoleClaim`; the `role` claim and the
  manager-profile tables were deliberately retired.
- Only Customer exposes `/internal/users/{sub}/claims`. B2B contributes **no** claims to Auth, so there
  is nothing to suspend at issuance without first building a B2B claims endpoint.
- `RemoteProfileClaimsProvider` caches claims for 5 minutes, and tokens outlive that, so a token-time
  gate lags a suspension by the token TTL.

The correct seam already exists: **`TenantContext.ResolveMembershipAsync`**
(`Tenant.Infrastructure/Services/TenantContext.cs`), which resolves the acting membership **from the DB
on every request**, precisely so that "role changes and removals take effect on the next request". A
suspension check there is one change, in one place, that closes every tenant-scoped endpoint and every
tenant-filtered query at once, with no TTL lag.

One design caveat that must not be skipped: resolving to *nothing* makes a suspended operator look
identical to a user with no membership, which reads as a bug and generates support load. The context
must carry a **distinguishable suspended state** so authorization denies while `/api/auth/me` can tell
the SPA to show "your organization is suspended". That is a slightly wider change than "return null".

### 7.2 Money — the part that cannot be hand-waved

**Held escrow.** `EscrowEntity` lives in **Payment**, keyed by `BookingId` with `FromOwnerId`/`ToOwnerId`
tenant ids, and `Held` funds sit on Concertable's Stripe balance under `OnBehalfOf`. Suspending the
**payee** may be exactly when we must not release; suspending the **payer** leaves an innocent
counterparty who has already performed. And holding indefinitely is the specific hazard
`LAUNCH_CHECKLIST.md` Phase 1 flags: *"weeks-long balance accumulation looks like a Payment Institution
operation"*. There is no safe default here — only an explicit per-booking decision.

**Pending payouts.** Settlement already has two fail-closed gates (tax compliance, self-billing
agreement) that defer and **self-heal** on the hourly `ConcertFinishedFunction` sweep once the tenant
fixes its own data. Suspension is categorically different: the tenant **cannot clear it**. Bolting it on
as a third gate would strand settlements forever with no self-healing path — a silent money-stopper
dressed as an existing pattern.

**Recommended policy, for the suspension plan to confirm rather than invent:**

1. **Suspension freezes forward capability, never in-flight money.** A suspended tenant cannot act on
   any tenant-scoped endpoint — no posting, applying, accepting, or being applied to. Existing bookings
   stay on their contracted path.
2. **Held escrow is never stranded.** Each in-flight booking of a suspended tenant must be resolved
   *explicitly* by an admin down one of the two paths that **already exist in shipped code**: release,
   or refund via the cancel path (`IEscrowOperationsClient.RefundByBookingIdAsync`, the same call the
   shipped application- and concert-cancel flows use). Suspension forces the decision; it never invents
   a money outcome and adds no money primitive.
3. **Settlement is not gated on suspension.** The sweep continues to settle already-performed
   obligations. Withholding payment is the explicit per-booking refund decision in (2), not a silent
   deferral.

The significant consequence: under this policy **Payment needs to know nothing about tenant
suspension.** B2B only stops initiating new obligations and reuses existing release/refund RPCs. So
there is **no published-contract change and no publish-first gate** — which is what makes the work
tractable at all, and is worth recording before someone reaches for a `Contracts` change.

### 7.3 The gate that actually splits it

Not effort, and not the money design — that is answered above. The gate is legal:

**Suspending a paying business customer's account is a contractual act.** `OSA_COMPLIANCE.md` Artifact 1
lists "Terms of service prohibiting illegal content and setting out enforcement" as **[LEGAL]**,
solicitor-owned, and it does not exist yet (`LAUNCH_CHECKLIST.md` Phase 3, a 2–4 week engagement not yet
started). Building and shipping a suspension button before we hold the contractual right to suspend is
legal exposure of exactly the kind this compliance pack exists to remove. This is the same class of gate
that already keeps the footer `report@` fallback out of scope, and `plans/AGENTS.md` "fewest safe merges"
splits at real gates like this one.

Secondary, and reinforcing: suspension touches the request pipeline for **every** endpoint and the money
state machine that the in-flight commission work
([`PLATFORM_COMMISSION_PLAN.md`](PLATFORM_COMMISSION_PLAN.md)) is currently rewriting. Landing it
inside an OSA feature PR would make an unrelated compliance change the carrier of a platform-wide
authorization change.

### 7.4 What ships instead, and why the OSA duty is still met

The illegal-content duty is *"act on illegal content swiftly"*. **Hiding the message removes the
content** — that is the takedown. Suspension is enforcement escalation against an account, which for a
KYC'd business counterparty is commercial and contractual as much as it is a safety measure. At the
pack's expected near-zero volume, suspension at launch is a documented manual runbook step, which is
precisely what Artifact 1's `[DECIDE]` already permits ("manual DB/ops action is acceptable"). Tommy's
override was to build the *moderation* capability; that is built.

**Tracked, not dropped:** the launch epic's compliance swim-lane gains an entry for it, recorded in the
same commit as this plan with its dependency named as the `[LEGAL]` T&Cs enforcement clause; it spins off
its own plan when that clause exists. No plan or ledger is created for it now — its first phase would be
"Tommy and the solicitor decide", which is a blocked ledger with no resolver, and `plans/AGENTS.md` is
explicit that those must not be left lying around.

**If Tommy wants suspension in this delivery anyway**, the design above is complete enough to execute;
the only genuinely missing input is the contractual right, and the answer is to say so, not to build
around it.

## 8. Decisions

- **D1 — No keyed strategy resolver for report categories.** `CODE_PATTERNS.md` demands the module-local
  keyed factory when behaviour varies by a closed key. Here **nothing varies**: every category produces
  the identical record, the identical two emails and the identical triage path. The category is *data on
  the report*, not a behaviour selector. Five registered strategies with identical bodies would be the
  "bare empty marker" smell `api/Concertable.B2B/src/Modules/Concert/AGENTS.md` warns about. The email prints the enum name directly, so
  there is no enum→label switch either. The moment a category genuinely routes to a different inbox or
  SLA, that is when the module-local factory is introduced — and the closed-key constraint means the
  registration builder will force the new member to be handled deliberately.
- **D2 — Adopt Reunion for the new operation.** `RESULT_PATTERN.md` is unambiguous: "New and changed
  contracts use Reunion directly", and B2B's exception style (`OrNotFound` → `NotFoundException`,
  FluentValidation) is "migration debt, not precedent". This is B2B's **first production Reunion usage**,
  scoped to exactly one new operation in one module; `Customer` is the worked reference and
  `B2B.Web/Program.cs:57` already registers `AddProblemDetails()` before MVC, so the terminal works. The
  arch test `TypedResultSlices_DoNotUseHttpExceptions` is **per-file**, so a Result-based file living
  next to exception-based siblings is legal — but the new files must not mix the two.
  **Risk, verified at the first checkpoint:** `api/Concertable.B2B/Directory.Packages.props` declares
  only `Reunion 0.1.0-alpha.3`; this needs `Reunion.Errors`, `Reunion.Validation`, `Reunion.AspNetCore`
  and `Dunet 1.16.2` added **at alpha.3**, keeping the service on one Reunion version. Do **not** align
  B2B on Customer's alpha.6 — B2B consumes Payment's published client packages, which are compiled
  against alpha.3, and a mixed graph would break at the union types. If alpha.3 of the missing packages
  is not on the feed, that is a genuine fork worth surfacing rather than deciding silently.
  **This decision is the one to veto if it is unwanted**: the fallback is `OrNotFound` +
  FluentValidation, consistent with the rest of B2B and a smaller diff, at the cost of adding to the
  migration debt the pattern doc names.
- **D3 — Hide, never delete.** Artifact 4's appeal right and any Ofcom information request both need the
  original content. `HiddenAt` is a visibility change; the report's `MessageExcerpt` is a snapshot, not
  the record of authority.
- **D4 — The decision reference is rendered from the report PK (`CR-{Id}`), not stored.** Artifact 4
  needs a quotable reference an appellant can cite. Gap-free per-supplier numbering exists for invoices
  but is heavy machinery for an internal ops reference, and a `Guid` is unusable over the phone. Derived,
  not persisted — no new numbering, nothing to keep in sync.
- **D5 — A non-participant gets 404, not 403.** The lookup runs on the tenant-filtered context, so
  "not yours" and "does not exist" are the same answer. That is also the correct privacy answer: a 403
  would confirm the message exists to a tenant with no right to know.
- **D6 — `UnitResult` + 204, reference by email.** Returning the reference in the HTTP body would force
  either a bare JSON string or a one-use DTO, both of which `RESULT_PATTERN.md` rejects. Artifact 3
  already mandates an acknowledgement email on submit, so that email is the natural carrier and the
  operation stays the smallest truthful shape.
- **D7 — Repeat reports are allowed.** Suppressing the `Report` link after a member has already reported
  a message would need a per-message "reported by me" read on every inbox page, and an OSA reporting
  route should never be *harder* to reach. A second report is data, not an error; the triage queue
  groups by message.
- **D8 — One PR for both phases.** `plans/AGENTS.md` "fewest safe merges": there is no merge,
  publication, sync or deployment gate between the reporting route and moderation, and both are `api/**`
  changes that would otherwise queue behind each other's platform sync. A reporting route with no way to
  act on a report is also half a duty. Two commits, one PR.

## 9. Out of scope

- **All `[LEGAL]` artifacts** — illegal-content risk assessment, children's-access assessment, the T&Cs
  illegal-content clause, the retention policy. Solicitor-owned, documentation not code.
- **The published `report@`/`safety@` footer fallback** — depends on the Privacy/T&Cs page routes, which
  are solicitor-gated and do not exist. **This is why the launch-readiness item for the reporting route
  will not be fully green from this work** (§2).
- **Tenant suspension** — split per §7, tracked as its own launch-epic entry.
- **Customer/marketplace OSA scope** — materially more in-scope and deferred with the marketplace
  (`plans/marketplace/MARKETPLACE_PLAN.md`).
- **An admin SPA**, admin RBAC/roles, and caching the `[Admin]` per-request DB hit — logged as tech debt,
  not built here.

## 10. Phase 1 — reporting route, record and acknowledgement

Backend: `ReportCategory` → `ContentReportEntity` → `ConversationsDbContext` + configuration →
`ContentReportRepository` → `ReportMessageError` + validators → `IContentReportService` +
`ContentReportNotifier` → `SafetySettings` + `appsettings.json` → composition root →
`MessageController` report endpoint + `MessageResponses` HATEOAS mapping. Then
`./initial-migrations.ps1` from `api/` (the model changed — re-scaffold, never additive). Then unit +
integration tests (§6.1, §6.2 report half). Then web (§5) and the E2E feature (§6.3).

**Verification gate:**
- `Conversations` module builds clean, plus whatever the `MessageDto` → `MessageResponse` change touches.
- Focused Conversations unit tests green, including the `ReportMessageError` definition-contract test.
- `./initial-migrations.ps1` run from `api/`.
- `app/web` typecheck/build for the two b2b SPAs.
- Commit and push to a draft PR. Exact-head PR CI owns the full solution build, standalone carves, and
  the complete unit/integration matrices — do not reproduce those locally. A red remote job enters
  `integration-debug` at its narrowest failing scope.

## 11. Phase 2 — admin moderation

`ReportOutcome` → `MessageEntity.Hide`/`Restore` → `AdminConversationsDbContext` + registration →
`AdminMessageRepository` + `AdminContentReportRepository` → `IModerationService` →
`ModerationController` (`[Admin]`) → `MessageRepository` hidden-message exclusion. Re-run
`./initial-migrations.ps1` (`MessageEntity` gained columns). Then the moderation tests (§6.2).

Also in this phase: the `api/Concertable.B2B/TECH_DEBT.md` entry for the `[Admin]` seam gaps (§3).

**Verification gate:** as Phase 1, plus the moderation integration tests — in particular the
**tenant-Owner-gets-403** assertion, which is the guard against the wrong authorization axis.

**Merge-queue E2E tier: full E2E, do not skip.** The change touches shared web code, a user-facing
messaging flow, the request-authorization surface, and the data model — it fails every `skip-e2e`
criterion. Let the merge queue run it; do **not** duplicate it locally ahead of the merge.

## 12. Delivery and close-out

- One PR covering both phases (D8), opened with plain `gh pr create` — personal repo, no `AB#`, no
  assignee. Open it draft at the first coherent checkpoint and push later checkpoints without prompting.
- **Report completion upward in the same commit as the shipping work**, never a deferred pass. The
  compliance swim-lane entry for this code slice is complete on ship. The **launch-readiness item for
  the reporting route stays open**, annotated with exactly what is live (in-app report button,
  structured safety-inbox email, persisted record, admin moderation) and exactly what is outstanding
  (the published `report@` address on the footer legal pages, solicitor-gated). Recording it as green
  would claim a compliance state we do not have. The exact lines to edit are in
  `OSA_REPORT_CONTENT_PROGRESS.md`.
- Merge via `/merge` (full E2E tier).
- **Own the post-merge `chore/platform-sync-*` PR to green.** This is an `api/**` change, so
  `publish-packages` republishes and `platform-sync` bumps every service's `<ConcertablePlatformVersion>`.
  Expected non-breaking — no cross-service published contract changes (§3) — so it should auto-merge. A
  red sync is this plan's to fix, in that PR.
- **Close out only after platform sync is green.** Record the terminal evidence in the ledger, then
  `git rm` this plan and its `_PROGRESS.md` together in a doc-only close-out riding the next change,
  landed through `/merge-docs` (`plans/agents/PLAN.md` Lifecycle 5). The source PR never deletes its own
  recovery artifacts.
