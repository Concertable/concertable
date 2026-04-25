# Messaging Module Extraction — Implementation Plan

Planned after Payment extraction. Messaging is a self-contained domain: persistent
user-to-user inbox with its own aggregate, REST surface, and read/unread lifecycle.

Memory: `project_future_modules.md` documents the module boundary decisions.

---

## Scope

**Messaging owns:**
- `MessageEntity` (`Id`, `FromUserId`, `ToUserId`, `Content`, `SentDate`, `Read`, `MessageAction?`)
- `MessageService` — `SendAsync`, `SendAndSaveAsync`, `GetForUserAsync`, `GetSummaryForUser`,
  `GetUnreadCountForUserAsync`, `MarkAsReadAsync`
- `MessageRepository`
- `MessageController` (endpoints: GET summary, GET paginated, GET unread count, POST mark-read)
- `MessageDtos.cs`, `MessageRequests.cs`, `MessageMappers.cs`
- `IMessageService`, `IMessageRepository`
- `MessageAction` enum (from `Concertable.Core/Enums/`)

**Not Messaging:**
- `IMessageNotificationService` / `SignalRMessageNotificationService` — that is the **Notification**
  module's concern. Messaging calls into `INotificationModule` (or raises an integration event)
  to trigger the real-time push when a message is saved. The two are decoupled: Messaging is
  the inbox, Notification is the delivery mechanism.
- Email — `Shared.Infrastructure`.

---

## Key design decision — how Messaging triggers SignalR push

Currently `MessageService.SendAndSaveAsync()` directly injects `IMessageNotificationService`
and calls `MessageReceivedAsync`. Two options at extraction time:

**Option A (simpler, acceptable for now):** `IMessageNotificationService` stays a Contracts-
level interface in Notification.Contracts; Messaging.Infrastructure injects it to trigger the
push after save. Cross-module call via Contracts — acceptable per CLAUDE.md rule 1.

**Option B (north-star):** Messaging raises a `MessageSentEvent` integration event.
Notification handles it and pushes via SignalR. Fully decoupled.

Option A is the right call for the extraction step. Option B is a follow-up once the outbox
is in place.

---

## Stage 1 — Implementation steps

### Step 1 — Scaffold Messaging projects
- `Concertable.Messaging.Domain` — `MessageEntity`, `MessageAction` enum
- `Concertable.Messaging.Application` — interfaces, DTOs, requests, validators, mappers
- `Concertable.Messaging.Infrastructure` — EF repo, service, `AddMessagingModule()`
- `Concertable.Messaging.Api` — `MessageController`, `AddMessagingApi()`
- `Concertable.Messaging.Contracts` — `IMessagingModule` (minimal facade; cross-module
  need is low — possibly just `GetUnreadCountAsync(Guid userId)` for header badge)

### Step 2 — Move entity + enum to Messaging.Domain
Move `MessageEntity` from `Concertable.Core/Entities/` and `MessageAction` from
`Concertable.Core/Enums/`. Add `Core → Messaging.Domain` project ref.

### Step 3 — Move Application layer
Move interfaces, DTOs, requests, mappers to `Messaging.Application`. All `internal`.
`AssemblyInfo.cs` with standard `InternalsVisibleTo` grants.

### Step 4 — Create MessagingDbContext
`internal class MessagingDbContext : DbContextBase`. Owns `Messages` DbSet.
Config: `MessageEntityConfiguration` (FK to Users — plain primitives `FromUserId`,
`ToUserId` as `Guid`; no nav properties into Identity.Domain).

`MessageEntity` has FKs to `UserEntity` (from Identity.Domain). Since Messaging runs
before AppDb but after Identity, the FK into `Users` is safe. No cross-context gotcha.

### Step 5 — Move Infrastructure layer
Move `MessageService`, `MessageRepository` to `Messaging.Infrastructure`. Rewrite
`IMessageNotificationService` injection to reference `Notification.Contracts` (Option A).

### Step 6 — Move controller to Messaging.Api

### Step 7 — MessagingDevSeeder + MessagingTestSeeder
Seed a handful of messages between seeded users for dev/test fixture.

### Step 8 — Remove Message DbSets from ApplicationDbContext + rescaffold migrations

### Step 9 — Wire AddMessagingApi() in Web. Full test suite.
