# Venue/artist legal details on booking-confirmation emails progress

- Plan: `plans/launch/VENUE_LEGAL_ON_EMAILS_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/venue-legal-on-emails`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_venue-legal-on-emails`
- Branch: `Feature/launch_venue-legal-on-emails`
- PR: `#582 (ready)`
- Dependency/package gates: `Concertable.Shared.Email IEmailRenderer — shipped (#586), on the pin via platform-sync #601 (0.1.0-alpha.0.1017); consumed here.`
- Last reconciled: `2026-08-16 — after the outbox refactor, re-sync to origin/main, and incremental /review.`

## Current state

The both-party booking-confirmation email is delivered on the **transactional outbox** and rendered with the
**shared MJML `IEmailRenderer`** (`Concertable.Shared.Email`). `BookingEntity.Confirm` raises
`BookingConfirmedDomainEvent`; the pre-commit `BookingConfirmedDomainEventHandler` delegates to
`BookingConfirmationEmailSender`, which resolves both tenants' legal details (`ITenantModule`, returning
`Reunion.Option<T>`) + recipients (`GetMemberUserIdsAsync` → `IUserModule.GetEmailsByIdsAsync`), renders the
`BookingConfirmation.mjml` template (html-escaped; VAT/address omitted until tax-compliance setup), and stages
one `SendEmailCommand` per member of both tenants. The sends commit atomically with the booking and are retried
by the outbox. The hand-rolled generator + synchronous `BookingConfirmationNotifier` are deleted; the
`ConcertDraftService` post-commit try/catch is gone (no longer needed). No model change → no migration.

Re-synced to current `origin/main` (206 commits ahead, incl. the `Reunion.Option` migration of `ITenantModule`
and the typed-error `ConcertDraftService` refactor — both accommodated). Builds clean; Concert unit suite
214/214 green (incl. new `BookingConfirmationEmailSenderTests` + `BookingEntityTests`). Two integration tests
(flow → staged-command assertion via `GetStagedEmailsAsync`; real-renderer degradation/escaping) run in CI.
Incremental `/review` done — 2 findings (NAT1 ct-threading, COV1 entity test), both fixed. No open findings.

## Next Steps

Ready to merge on Tommy's go-ahead. CI running on `1bdff596d`; `skip-e2e` set (Step 4: B2B-internal + additive,
no positive E2E trigger — integration covers the booking-confirmed path).

1. **Merge #582** on go-ahead. The assistant's `gh pr merge` is walled by the merge-review gate, so Tommy
   enqueues via `! gh pr merge 582 --repo Concertable/concertable --merge --auto`.
2. If a check goes red, diagnose only the failing scope (`integration-debug` for the two integration tests);
   don't run E2E locally ahead of the queue.
3. On merge, follow the `chore/platform-sync-*` PR to green — routine non-breaking bump
   (`BookingConfirmedDomainEvent` is additive in `Concert.Domain`; no consumer migration). Then tick roadmap §7
   "Venue legal details on booking confirmation emails + invoices" and mark the §5 row.

## Completed work

- **Shared MJML renderer** — `IEmailRenderer` in `Concertable.Shared.Email` (#586, merged; on the pin via #601).
- **Outbox delivery** — `BookingConfirmedDomainEvent` (`Concert.Domain/Events`), raised by `BookingEntity.Confirm`;
  pre-commit `BookingConfirmedDomainEventHandler` → `BookingConfirmationEmailSender` (resolve + render + stage).
  `BookingConfirmation.mjml` (embedded) + `BookingConfirmationEmailContent` + `EmailParty`. Registered in
  `ServiceCollectionExtensions`. Deleted: `IBookingConfirmationEmailGenerator`, `BookingConfirmationEmailGenerator`,
  `IBookingConfirmationNotifier`, `BookingConfirmationNotifier`, the post-commit try/catch + its log method.
- **Tests** — `BookingConfirmationEmailSenderTests` (resolution per tax-compliance + multi-recipient staging,
  mocked modules/bus/renderer); `BookingEntityTests` (Confirm raises the event); integration
  `BookingConfirmationEmailTests` rewritten (flow → `GetStagedEmailsAsync`; real-renderer degradation/escaping).

## Reviews

`reviews/Feature-launch_venue-legal-on-emails.md` — native (`code-reviewer`) + architecture layers. Marker at
`a8fd98ce` (2026-08-16). Original pass (`520761dd..b19bcc79`): BUG1 (unguarded post-commit send) — now
**superseded** by the outbox (transactional). Incremental (`b19bcc79..a8fd98ce`): NAT1 (ct not threaded to tenant
reads) + COV1 (missing entity-event test) — both fixed. No open findings. No security layer (no
`*.Contracts`/Auth/Payment path).

## Decisions

- **Open Decision 1 — delivery: REVERSED to the outbox.** The plan deferred it (synchronous; "outbox not
  observable in the harness"); that reason is stale since `InvitationService` migrated to the outbox and its
  tests observe staged sends. The outbox is the correct long-term shape (transactional, retried, off-thread) and
  makes the isolation try/catch unnecessary. Follows Concert's `ConcertPosted`/`Cancelled`/`Changed` pattern.
- **Open Decision 2 — company number:** unchanged — mirror the invoice (legal name + registered address + VAT,
  no new field); a real company-number field is additive Tenant work, deferred.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\launch_venue-legal-on-emails
Read @plans/launch/VENUE_LEGAL_ON_EMAILS_PLAN.md and @plans/launch/VENUE_LEGAL_ON_EMAILS_PROGRESS.md and do what its `## Next Steps` says.
```
