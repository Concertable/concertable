# B2B `/api/venue/user` returns 403 in venue-hire E2E

Branch: `Refactor/Microservices`, head `d6ac7d19` ("Fix E2E Respawn TablesToIgnore arg order + add Schema table consts").

## What's done and verified

- Seed phase passes cleanly. `[user].[Users]` reaches 71 (1 admin + 70 manager rows written by `CredentialRegisteredHandler` from `CredentialRegisteredEvent`). `[messaging].[Inbox]` populated alongside.
- Respawn `TablesToIgnore` arg order is correct in all three E2E DbFixtures and uses `Schema.Name` / `Schema.<Table>` constants.
- Permanent observability logs added: `HandlingCredentialRegistered`, `SkippedCredentialRegistered`, `WroteUserFromCredentialRegistered` in B2B User; `EventProcessorStarted` / `CommandProcessorStarted` / `MessageReceived` in `AzureServiceBusReceiver`; `DbHealthWaiterProgress` in `DbHealthWaiter`. Use the `Log.cs` source-gen convention for any new permanent logs (per `api/docs/DEBUGGING_CONVENTIONS.md`).

## The bug

Scenario "Artist pays hire fee upfront to book venue" now passes the readiness gate but fails ~48s into the scenario at:

```
HTTP 403 GET https://localhost:7086/api/venue/user
```

`[NotificationHub] connected userId=b1000000-0000-0000-0000-000000000001` shows up around the same time — that's `SeedUsers.VenueManagerId(1)`, i.e. venue manager 1. Whether the failing HTTP call is in that same auth context is the first thing to confirm.

## What we know

- `VenueController.GetDetailsForCurrentUser` (`api/Concertable.B2B/Modules/Venue/Concertable.B2B.Venue.Api/Controllers/VenueController.cs:28`) is decorated with `[AuthorizeVenueManager]` → policy `VenueManager` (`api/Concertable.B2B/Modules/User/Concertable.B2B.User.Infrastructure/Extensions/ServiceCollectionExtensions.cs:55`).
- `VenueManagerProfileHandler.HandleRequirementAsync` (`api/Concertable.B2B/Modules/User/Concertable.B2B.User.Infrastructure/Authorization/VenueManagerProfileHandler.cs:18`) parses the `sub` claim as a Guid and calls `db.VenueManagerProfiles.AnyAsync(p => p.Sub == sub)`. It only calls `context.Succeed` if a row exists.
- `CredentialRegisteredHandler` (`api/Concertable.B2B/Modules/User/Concertable.B2B.User.Infrastructure/Events/CredentialRegisteredHandler.cs`) writes `new VenueManagerProfileEntity(user.Id)` per venue-manager event. The handler reported 70 `wrote` logs in the previous run, and `[user].[VenueManagerProfiles]` is in `TablesToIgnore`. So the row for `b1000000-…-0001` should exist at request time.
- The auth requirement should therefore pass for b1…0001. It doesn't.

## Things to check first

1. **Whose JWT is on the failing request?** The browser may have multiple tabs / users. Confirm via the request headers in the test output that the failing `/api/venue/user` call carries a token whose `sub` matches a seeded venue manager.
2. **What does `VenueManagerProfileHandler` actually see?** Add a temporary inline log at the top of `HandleRequirementAsync` showing the raw `subClaim` value, the parsed `sub` Guid, and whether the `AnyAsync` returned true. If it returns false even when the row exists, the `sub` shape may not match `VenueManagerProfileEntity.Sub` (Guid vs string, lowercased, prefixed, etc.).
3. **Check `VenueManagerProfileEntity`'s `Sub` configuration and ctor** — confirm `Sub` is being set to the same Guid the seeder uses. If the ctor takes a `user.Id` argument but assigns it to a different field, the AnyAsync won't match.
4. The relevant `IProfileClaimsProvider` impls (`AuthLocalClaimsProvider`, `B2BProfileClaimsProvider`) shape the JWT claims server-side in Auth. Verify they put a Guid in `sub` rather than e.g. a `name` or `email` claim.

## Run

Use the `/run-ui-e2e-tests` skill. Args:

```
b2b "Artist pays hire fee upfront to book venue"
```

## After this bug

Resume the microservices migration roadmap (`MICROSERVICE_STEPS_CONT.md`, Phase 8 Step 24).

## Hard rules

- No `Co-Authored-By: Claude` or `Generated with Claude Code` trailers.
- Show staged diff and wait for explicit approval before commit.
- No comments narrating fixes (`feedback_no_fix_comments`).
- PowerShell tool, not Bash, for `dotnet build` / `dotnet test`.
- `is not null` over `is { }`.
- No primary constructors for services.
- `this.field = param`, never `_field` prefix.
- Permanent observability logs go via `Log.cs` source-gen; one-off probes stay inline and get stripped once the bug is found (per `api/docs/DEBUGGING_CONVENTIONS.md`).
- `PayoutAccount` row counts are not an event-delivery proxy — see `api/Concertable.Payment/Concertable.Payment.Seeding/CLAUDE.md` before reasoning from them.
