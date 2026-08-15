# Code review — Feature/launch_venue-legal-on-emails

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `b19bcc792d6a9b1889643a7e075552f40ab09c72`  _(2026-08-15)_

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
