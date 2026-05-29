# B2B doesn't receive CredentialRegisteredEvent from Auth — event-flow bug

Branch: `Refactor/Microservices`, head `56ace89d` ("Seed data redesign + readiness gate for event-driven data").

## What's done and verified

- Seed phase passes cleanly. `ArtistReadModel` tracking conflict fixed via scoped `SeedData` in AppFixture host.
- `AuthDevSeeder` is a proper `IDevSeeder`, run via `AuthDbInitializer`.
- `IHealthWaiter` pattern: `E2EDbInitializer` (in B2B test project) wraps `Concertable.B2B.Web.DevDbInitializer` and awaits `B2BUserHealthWaiter` before returning. The waiter polls `UserDbContext.Users` via DI-injected `DbHealthWaiter`, expects `SeedUsers.TotalCount` (= 1 admin + 35 artist + 35 venue managers = 71 rows).

## The bug

B2B venue-hire E2E ("Artist pays hire fee upfront to book venue") fails at the readiness waiter: timeout after 3 minutes because `[user].[Users]` in B2BDb only ever has 1 row (the admin, written directly by B2B's `UserDevSeeder`). The 70 manager rows should appear as B2B's `CredentialRegisteredHandler` (`api/Concertable.B2B/Modules/User/Concertable.B2B.User.Infrastructure/Events/CredentialRegisteredHandler.cs:9`) writes them in response to events published by Auth.

Hard facts from the failing run:

- **Auth IS publishing `CredentialRegisteredEvent`**: Payment side received some events (5 `INSERT INTO [payment].[PayoutAccounts]` visible in logs). So Auth → ASB → Payment leg works.
- **B2B side**: 0 `INSERT INTO [user].[Users]` event-driven, 0 `INSERT INTO [messaging].[Inbox]` (idempotency table) — B2B's handler never runs.
- **Payment only got 5/71 events.** Suspicious — expect ~71 if every credential creates a `PayoutAccount`.
- `AzureServiceBusReceiver.cs:43` opens `client.CreateProcessor(topic, options.ServiceName)` where `ServiceName` is `"concertable-b2b"` / `"concertable-payment"`. The Aspire topology (`api/Concertable.B2B/Concertable.B2B.AppHost.Extensions/B2BTopology.cs` and `PaymentTopology.cs`) creates subscriptions via `AsbTopology.Subscribe(resourceName, subscriptionName, consumerGroup)` → `topicBuilder.AddServiceBusSubscription(resourceName, subscriptionName)`. Per Aspire xml docs, the 3rd arg is the actual ASB subscription name — so subscriptions ARE named `"concertable-b2b"` / `"concertable-payment"`. Receiver looks for those. **They match.** (Earlier I thought there was a naming mismatch — there isn't.)

## Things to investigate

1. **Why only 5/71 events reach Payment.** Could be Auth's `OutboxDispatcher` silently dropping events — `Concertable.Messaging.AzureServiceBus/AzureServiceBusTransport.cs:36` catches `ServiceBusFailureReason.MessagingEntityNotFound` silently.
2. **Whether B2B's `AzureServiceBusReceiver` hosted service actually starts** and opens its processors successfully. Auth/B2B logs are at Warning level in the test fixture, so EF SQL and ASB processor errors are filtered out. Bumping log filter for `"Concertable.Messaging"` to Information would surface this.
3. **Whether the ASB emulator container is set up before Auth's outbox dispatcher first publishes** (timing race).

## Suggested first move

Bump test fixture logging to capture `Concertable.Messaging.AzureServiceBus.AzureServiceBusReceiver` at Information level — `api/Concertable.B2B/Tests/E2ETests/Concertable.B2B.E2ETests/AppFixture.cs:68`, add:

```csharp
.AddFilter("Concertable.Messaging", LogLevel.Information)
```

Then run the scenario via the skill.

## Run

Use the `/run-ui-e2e-tests` skill (it surfaces failures with enriched logs and has built-in diagnostic helpers). Args:

```
b2b "Artist pays hire fee upfront to book venue"
```

## After this bug

Resume the microservices migration roadmap (`MICROSERVICE_STEPS_CONT.md`, Phase 8 Step 24: separate SignalR from email in `Modules/Notification/`). See `MEMORY.md` for the full phase tracker.

## Hard rules

- No `Co-Authored-By: Claude` or `Generated with Claude Code` trailers.
- Show staged diff and wait for explicit approval before commit.
- No comments narrating fixes (`feedback_no_fix_comments`).
- PowerShell tool, not Bash, for `dotnet build`/`test`.
- `is not null` over `is { }`.
- No primary constructors for services.
- `this.field = param`, never `_field` prefix.
