# Messaging Module Extraction — Implementation Plan

Full 4-layer module — owns its own aggregate and DB table. User-to-user inbox with persistent
messages, read/unread lifecycle, and real-time push on delivery.

Planned after Notification module: Messaging.Infrastructure references Notification.Contracts
for `INotificationModule`.

---

## Scope

**Messaging owns:**
- `MessageEntity` (`Id`, `FromUserId`, `ToUserId`, `Content`, `SentDate`, `Read`, `MessageAction?`)
- `MessageAction` enum
- `MessageService` — `SendAsync`, `SendAndSaveAsync`, `GetForUserAsync`, `GetSummaryForUser`,
  `GetUnreadCountForUserAsync`, `MarkAsReadAsync`
- `MessageRepository`
- `MessageController` (GET summary, GET paginated, GET unread count, POST mark-read)
- `MessageDto`, `MessageSummaryDto`, mappers, requests, validators
- `MessagingDbContext` — owns the `Messages` table

**Not Messaging:**
- Real-time push — Messaging calls `INotificationModule.SendAsync(...)` from Notification.Contracts
- Email

---

## Cross-module facade

`IMessagingModule` in Contracts is minimal. Currently nothing reads messages cross-module, but the
unread-count header badge is a likely consumer — expose `Task<int> GetUnreadCountAsync(Guid userId)`
and nothing else. If no cross-module consumer materialises before extraction, ship an empty shell
and fill it when needed.

---

## Steps

### Step 1 — Scaffold projects
Create:
- `Concertable.Messaging.Contracts`
- `Concertable.Messaging.Domain`
- `Concertable.Messaging.Application`
- `Concertable.Messaging.Infrastructure`
- `Concertable.Messaging.Api`

Add to solution under `Modules/Messaging`.

### Step 2 — Move Domain
- `MessageEntity` from `Concertable.Core/Entities/` → `Messaging.Domain/`
- `MessageAction` from `Concertable.Core/Enums/` → `Messaging.Domain/`
- Update namespaces to `Concertable.Messaging.Domain`
- Add `Concertable.Core` → `Messaging.Domain` project ref so Core still compiles until it retires

### Step 3 — Move Application layer
Move to `Messaging.Application`, update namespaces to `Concertable.Messaging.Application`:
- `IMessageService` + `IMessageRepository` (from `Concertable.Application.Interfaces/`)
- `MessageDto`, `MessageSummaryDto` (from `Concertable.Application.DTOs/`)
- `MessageMappers` (from `Concertable.Application.Mappers/`)
- `MarkMessagesReadRequest` (from `Concertable.Application.Requests/`)
- `MessageValidators` (from `Concertable.Application.Validators/`)

All `internal`. Add `AssemblyInfo.cs`:
```csharp
[assembly: InternalsVisibleTo("Concertable.Messaging.Infrastructure")]
[assembly: InternalsVisibleTo("Concertable.Messaging.Api")]
[assembly: InternalsVisibleTo("Concertable.Web.IntegrationTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
```

### Step 4 — Create `MessagingDbContext`
`internal class MessagingDbContext : DbContextBase`. Owns `Messages` DbSet.

`MessageEntityConfiguration` under `Messaging.Infrastructure/Data/Configurations/`:
- `FromUserId` and `ToUserId` as plain `Guid` columns — no FK nav into Identity.Domain
- FK constraints to `Users` table are safe: Messaging scaffolds after Identity

### Step 5 — Move Infrastructure layer
Move to `Messaging.Infrastructure`, update namespaces:
- `MessageRepository` (from `Concertable.Infrastructure/Repositories/`) — repoint to `MessagingDbContext`
- `MessageService` (from `Concertable.Infrastructure/Services/`) — replace `IMessageNotificationService`
  with `INotificationModule` from Notification.Contracts:
  ```csharp
  private const string MessageReceivedEvent = "MessageReceived";
  // ...
  await notificationModule.SendAsync(toUserId.ToString(), MessageReceivedEvent, message.ToDto(fromUser));
  ```

`AddMessagingModule()` registers:
- `IMessageService` → `MessageService`
- `IMessageRepository` → `MessageRepository`
- `IMessagingModule` → `MessagingModule`
- `MessagingDbContext`

### Step 6 — Move Api
Move `MessageController` from `Concertable.Web/Controllers/` → `Messaging.Api/`.
Update namespace. Controller is `public`.

`AddMessagingApi()` in `Messaging.Api` — called by Web to wire the ApplicationPart.

### Step 7 — Seeders
`MessagingDevSeeder` + `MessagingTestSeeder`: seed messages between seeded users.
`Order` value after Identity (users must exist first).

### Step 8 — Remove Messages from `ApplicationDbContext`
- Remove `DbSet<MessageEntity> Messages` from `ApplicationDbContext`
- Remove `MessageEntityConfiguration` from `Data.Infrastructure/Data/Configurations/`
  (applied there today via `ApplyConfigurationsFromAssembly`)

### Step 9 — Re-scaffold migrations
Delete all `Migrations/` folders, re-scaffold `InitialCreate` in dependency order:

Shared → Identity → Artist → Venue → Concert → Contract → **Messaging** → AppDb

### Step 10 — Wire in Web + Workers; test suite
- Add `AddMessagingModule()` + `AddMessagingApi()` in Web and Workers
- Remove `MessageController` usings from `Concertable.Web/GlobalUsings.cs`
- Full test suite: build + integration tests

---

## Progress

- [x] Step 1 — Scaffold projects
- [x] Step 2 — Move Domain
- [ ] Step 3 — Move Application layer
- [ ] Step 4 — Create `MessagingDbContext`
- [ ] Step 5 — Move Infrastructure layer
- [ ] Step 6 — Move Api
- [ ] Step 7 — Seeders
- [ ] Step 8 — Remove Messages from `ApplicationDbContext`
- [ ] Step 9 — Re-scaffold migrations
- [ ] Step 10 — Wire in Web + Workers; test suite
