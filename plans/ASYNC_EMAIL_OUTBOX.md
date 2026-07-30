# Async email via the transactional outbox

## Context

Email delivery must be staged in the producer's transactional outbox so callers return without
waiting for SMTP and delivery retries instead of being lost. The shared email infrastructure is a
published package; Auth, B2B, and Customer consume the feed version rather than sibling source.

PR #162 published the phase-1 commands, handlers, and `IEmailTransport` split while keeping
`IEmailSender` synchronous for compatibility.

PR #172 originally tried to publish `OutboxEmailSender`, flip `AddSharedEmail`, and migrate Auth in
one merge. Merge-queue CI #950 exposed the invalid package boundary: Auth still ran the phase-1
package, so its synchronous fake verification callback executed before the new token was saved.
The callback could not find the token, no verification command was staged, and signup remained
unverified.

## Phase 2A — publish the opt-in and compatible consumer migrations

- Keep `AddSharedEmail` mapping `IEmailSender` to the synchronous transport.
- Publish `OutboxEmailSender` behind the additive `UseOutboxEmailSender` registration.
- Keep the Customer ticket command, B2B transport migrations, topology, and outbox unit-of-work
  changes that compile and run against the phase-1 package.
- Keep Auth synchronous until the opt-in exists on the feed.

Gate:

- `dotnet build api/Concertable.slnx` with zero errors.
- Affected integration tests green.
- Artist and venue signup UI scenarios green in merge-queue-equivalent local verification.

## Phase 2B — migrate Auth after publish and platform sync

Blocked until phase 2A merges, its packages publish, and the platform-sync PR is green.

- Call `UseOutboxEmailSender` in Auth after `AddSharedEmail`.
- Restore Auth's outbox unit-of-work around verification and password-reset staging.
- Add an explicit UI E2E step that polls Auth token minting until background verification completes,
  then performs the existing single UI sign-in.
- Run Auth integration coverage and the full UI E2E suite.

Delete this plan in the verified phase-2B completion commit.
