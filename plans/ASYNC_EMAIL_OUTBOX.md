# Async email via the transactional outbox

## Context

`Concertable.Shared.Email` sent SMTP **synchronously inline** (`EmailSender`), so every send blocked its
caller and a transient failure lost the email. Worst case: Customer's ticket receipt was swallowed by a
`try/catch` *after* payment + tickets committed, so a buyer could silently never get their tickets.

Fix: model "send this email" as an `IIntegrationCommand` delivered through the existing transactional
outbox (`OutboxBus` → `OutboxDispatcher` → ASB receiver, with retry + dead-lettering). Callers return
immediately; the send commits atomically with the business change and is retried, not lost.

**This is a boundary-blocked, multi-merge refactor** (`plans/CLAUDE.md` → "Boundary-blocked refactors").
Auth/B2B/Customer compile against the **published** `Concertable.Shared.Email` package pinned by
`ConcertablePlatformVersion`, not the sibling source — so the new public types must reach the feed
before consumers can adopt them. Hence phase 1 (expand the package) and phase 2 (migrate consumers)
cannot land in one PR.

## Phase 1 — expand the shared package ✅ DONE (PR #162, branch `Feature/AsyncEmailOutbox`)

Additive, no runtime behaviour change. Shipped:

- `IEmailTransport` (actual SMTP/fake send) split from business-facing `IEmailSender`;
  `EmailSender`/`FakeEmailSender` → `SmtpEmailTransport`/`FakeEmailTransport`.
- `SendEmailCommand` / `SendVerificationEmailCommand` (`IIntegrationCommand` + `[MessageType]`) and their
  handlers, delivering via `IEmailTransport`.
- `AddSharedEmail` registers the handlers and **transitionally keeps `IEmailSender` pointing at the
  synchronous transport** (both transports implement `IEmailSender` too), so unmigrated callers are
  unchanged. `OutboxEmailSender` is intentionally **not** in this PR — it lands in phase 2 with its
  registration + call sites.
- `Concertable.Messaging.AzureServiceBus` `QueueNameFor` now **service-scopes** the queue
  (`command-<ServiceName>-<type>`), because a global command queue let Auth + B2B compete on
  `SendEmailCommand`. `PaymentTopology` declares old **and** new webhook queue names to bridge the
  version bump.

Gate: `dotnet build api/Concertable.slnx` = 0 errors; Messaging unit tests 48/48. No migrations (every
context already maps `OutboxMessageEntity`).

## Phase 2 — migrate consumers 🔴 OUTSTANDING (blocked until PR #162 merges + its platform-sync PR is green)

The new package version must be on the feed and `ConcertablePlatformVersion` bumped first. Then, in one
branch:

- **`AddSharedEmail`**: flip `IEmailSender` → `OutboxEmailSender` (the producer that stages
  `bus.SendAsync(...)`); stop registering the transports as `IEmailSender`. Re-add `OutboxEmailSender`.
- **Auth** (`AuthService.SendEmailVerificationAsync`, `SendPasswordResetAsync`): inject
  `IDbContextAccessor`, set `contextAccessor.Context = context`, enqueue before the existing
  `SaveChangesAsync` (WebhookService pattern). Register both commands in `Program.cs`
  (`reg.HandleCommand<…>()`).
- **Customer tickets** (`TicketService.CompleteAsync`): new `SendTicketEmailCommand(email, ticketIds)` +
  handler (renders PDFs via `ITicketPdfService`, sends via `IEmailTransport`); stage it in the ticket
  save; **delete the `try/catch` swallow**. Register `HandleCommand<SendTicketEmailCommand>` in
  Customer.Web. Remove `ITicketEmailSender`/`TicketEmailSender` + the `TicketEmailFailed` log method.
- **B2B Messenger**: switch to `IEmailTransport` (stays synchronous — interim). Log tech debt: its real
  transactional anchor needs the concert-lifecycle transition to raise a domain event first. Register
  the mock as `IEmailTransport` in the B2B integration fixture (+ `IMockEmailSender : IEmailTransport`).
- **ASB topology**: `AddAuthTopology` (new, in `Concertable.AppHost.Shared`) + email command queues in
  `B2BTopology`/`CustomerTopology`; wire `.AddAuthTopology()` into all AppHosts that run Auth; scope
  Payment's queue name to only the new value once repinned.

All of phase-2's code was written and verified against the local source, then reverted out of PR #162 —
it re-applies cleanly against the new package version. `git log`/this session is the reference.

Gate: build + the Auth / Customer.Ticket / B2B.Concert integration suites (the existing
`EmailSender.Sent` assertions must stay green). E2E: registration/verification + ticket-purchase now
settle async — let the merge queue run it; watch the verification poll windows.

## Delete this plan when phase 2 lands and the codebase is back in sync (not before — `plans/CLAUDE.md` §8).
