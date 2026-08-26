# Code review — Feature/launch_venue-legal-on-emails

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `7efc0e017c6e65030758789d108ca007b7743798`  _(2026-08-16)_

> Range reviewed: `520761dd..b19bcc79` (5 commits). Native (`code-reviewer`) + architecture layers.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **BUG1 — MEDIUM — correctness/error-handling** — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/ConcertDraftService.cs:69`
  The booking-confirmation send (`BookingConfirmedAsync`, N blocking `IEmailTransport.SendEmailAsync`
  calls + a hard `InvalidOperationException` on a not-found tenant) ran **unguarded after
  `SaveChangesAsync()`** (line 62), inside the Stripe-webhook-driven book step. A transient SMTP error
  or missing tenant threw out of `CreateAsync` after the booking was already committed — failing the
  transition and causing the webhook to retry against an already-confirmed booking. **Fixed** (`b19bcc79`):
  wrapped in try/catch that logs via a source-generated `BookingConfirmationEmailFailed` message; the
  email is best-effort and can no longer fail a committed booking. Durable fix (outbox `IEmailSender`,
  which `IEmailTransport`'s own doc mandates for business code) stays tracked in
  `api/Concertable.B2B/TECH_DEBT.md` under the synchronous-email item — consistent with the plan's
  Open Decision 1, so the abstraction choice itself is a documented deliberate exception, not a finding.

## Cleared without a finding

- **HTML injection** — every tenant-supplied value in the body goes through `WebUtility.HtmlEncode`; the
  subject is plain text. Safe.
- **Graceful degradation** — absent `TaxComplianceDto` / null `VatNumber` / null address `Line2` handled
  cleanly (legal name always, address+VAT only when present). Covered by 6 generator unit tests.
- **Recipient resolution** — both tenants' members resolved and mailed, all `await`ed correctly; covered
  end-to-end by the integration test (both-tenant receipt + the placeholder-legal-name degradation path).
- **Member-fanout duplication** vs `Messenger`/`ApplicationNotifier` — the idiom already repeats across
  3+ existing sites with no shared helper; an accepted convention, not a new reuse violation.

## Incremental review — 2026-08-16

> Range: `b19bcc79..a8fd98ce`. Native (`code-reviewer`) + architecture layers. The booking-confirmation
> email was **entirely rewritten** here: the hand-rolled `BookingConfirmationEmailGenerator` +
> synchronous `BookingConfirmationNotifier` became the shared MJML `IEmailRenderer` + a transactional
> outbox delivery (`BookingEntity.Confirm` raises `BookingConfirmedDomainEvent` → pre-commit
> `BookingConfirmedDomainEventHandler` → `BookingConfirmationEmailSender` stages `SendEmailCommand`s).

- [x] **BUG1 superseded** — the earlier isolation fix (try/catch around a post-commit send) is **gone**:
  the send is now staged on the booking's own transaction via the outbox, so it can neither be lost nor
  fail the committed booking. The plan's Open Decision 1 (synchronous) was reversed — its deferral
  reason ("outbox not observable in the harness") no longer holds since `InvitationService` migrated.
- [x] **NAT1 — LOW — correctness** — `Emails/BookingConfirmationEmailSender.cs` — `SendAsync` forwarded
  its `CancellationToken` only to `bus.SendAsync`; the three `ITenantModule` reads dropped it. **Fixed**
  (`a8fd98ce`): threaded `ct` through `BuildPartyAsync`/`StageToMembersAsync` to all three reads.
- [x] **COV1 — LOW — test coverage (Lens F)** — the new `BookingEntity.Confirm` → domain-event wiring
  was covered only by the Docker-gated integration test. **Fixed** (`0ca49c8b`): added
  `BookingEntityTests.Confirm_...RaisesBookingConfirmedDomainEvent` (mirrors `TenantInvitationEntityTests`).
- [x] **CV1 — LOW — convention** — the sender's private `FormatAddress` helper should be a mapper per
  CODE_CONVENTIONS ("Mappers — `XMappers` extension methods"). **Fixed** (`7efc0e01`): moved to
  `RegisteredAddressMappers.ToSingleLine()` as a C# 14 `extension()` block. Behaviour-preserving (same
  output, covered by the sender test) — mechanical move, no new findings.

## Cleared without a finding — 2026-08-16

- **Transactional atomicity** — `DomainEventDispatchInterceptor` scans `IEventRaiser` entities and runs
  the pre-commit handler inside `SaveChanges`, so the staged sends commit with the booking (established
  Concert pattern: `ConcertPosted`/`Cancelled`/`Changed`).
- **EF mapping of the new `IEventRaiser` field on `BookingEntity`** — no config needed: `DomainEvents` is
  get-only (convention-ignored) and the private `events` field has no mapped property; identical to
  `ConcertEntity`, which carries no `Ignore` config and no migration.
- **`FormatAddress` NPE** — `RegisteredAddressDto.RegisteredAddress` is `required`/non-null, guarded by
  `TryGetValue`; safe. `html.escape` on every tenant value preserves the old `HtmlEncode` safety.
- **Fan-out vs `ApplicationNotifier`** — targets a different mechanism (`messenger`/`EmailCopy`); direct
  outbox staging here is not a reuse violation.
