# Notification Module Extraction — Implementation Plan

Pure delivery channel: no domain, no owned tables. Takes real-time push concerns out of
`Concertable.Web` and gives the rest of the system a single injection point for
sending events to connected clients via SignalR.

---

## Scope

**Notification owns:**
- `NotificationHub` — SignalR hub managing user connection groups
- `INotificationModule` (Contracts) — single generic send; callers don't know about SignalR
- `SignalRNotificationModule` (Infrastructure) — wraps `IHubContext<NotificationHub>`

**Not Notification:**
- Email — `Shared.Infrastructure`; injected directly by any module that sends email
- Messaging inbox — that is the Messaging module
- Hub endpoint mapping (`app.MapHub<NotificationHub>(...)`) — stays in `Web/Program.cs` as a
  host/middleware concern; the hub *class* moves to Notification.Infrastructure

---

## Layer layout

Two layers only — mirrors the Search module pattern of skipping unused layers:

```
Modules/Notification/
  Concertable.Notification.Contracts/      ← INotificationModule
  Concertable.Notification.Infrastructure/ ← NotificationHub, SignalRNotificationModule, AddNotificationModule()
```

No Domain — no owned aggregates.
No Application — no business logic between interface and implementation.

---

## Key decisions

**Generic `SendAsync`, not per-event interfaces.** The four existing `IXNotificationService`
interfaces (`IMessageNotificationService`, `IConcertNotificationService`,
`IApplicationNotificationService`, `ITicketNotificationService`) each reduce to a single
`hubContext.Clients.Group(userId).SendAsync(eventName, payload)` call. They add a layer of
indirection with no benefit and grow the Contracts surface for every new event type.
Replace all four with:

```csharp
public interface INotificationModule
{
    Task SendAsync(string userId, string eventName, object payload);
}
```

Event name strings stay with the caller — each module knows its own event names and defines
them as local constants. Notification.Contracts has no knowledge of other modules' events.

**`ClaimsPrincipalExtensions.GetId()` moves to `Identity.Contracts`.** Currently in
`Concertable.Web/Extentions/` (note typo in namespace). The extension reads an identity
claim — `Identity.Contracts` is the right home. Notification.Infrastructure references
`Identity.Contracts` for this. All other callers of `GetId()` update their using directive.

**`IHubContext<T>` is thread-safe — register `SignalRNotificationModule` as singleton.**

---

## Steps

### Step 1 — Scaffold projects
Create:
- `api/Modules/Notification/Concertable.Notification.Contracts/`
- `api/Modules/Notification/Concertable.Notification.Infrastructure/`

Add both to solution under `Modules/Notification` solution folder.

Project refs:
- `Notification.Contracts` — no outward refs
- `Notification.Infrastructure` → `Notification.Contracts`, `Identity.Contracts`,
  `Microsoft.AspNetCore.SignalR` (framework ref)

### Step 2 — Move `ClaimsPrincipalExtensions.GetId()` to `Identity.Contracts`
Check whether an equivalent already exists in `Identity.Contracts`. If not, move
`Concertable.Web/Extentions/ClaimsPrincipalExtensions.cs` there and update its namespace.
Update all callers (`using Concertable.Web.Extentions` → `using Concertable.Identity.Contracts`).

### Step 3 — Define `INotificationModule` in Notification.Contracts
```csharp
public interface INotificationModule
{
    Task SendAsync(string userId, string eventName, object payload);
}
```

### Step 4 — Move `NotificationHub` to `Notification.Infrastructure/Hubs/`
Move `Concertable.Web/Hubs/NotificationHub.cs`. Update namespace to
`Concertable.Notification.Infrastructure.Hubs`. Hub stays `[Authorize]` and still reads
`Context.User?.GetId()` — now from `Identity.Contracts`.

### Step 5 — Implement `SignalRNotificationModule`
```csharp
internal class SignalRNotificationModule : INotificationModule
{
    private readonly IHubContext<NotificationHub> hubContext;

    public SignalRNotificationModule(IHubContext<NotificationHub> hubContext)
        => this.hubContext = hubContext;

    public Task SendAsync(string userId, string eventName, object payload)
        => hubContext.Clients.Group(userId).SendAsync(eventName, payload);
}
```

### Step 6 — `AddNotificationModule()` extension
```csharp
public static IServiceCollection AddNotificationModule(this IServiceCollection services)
{
    services.AddSignalR();
    services.AddSingleton<INotificationModule, SignalRNotificationModule>();
    return services;
}
```

### Step 7 — Replace per-module notification interfaces
Delete:
- `IMessageNotificationService` + `SignalRMessageNotificationService`
- `IConcertNotificationService` + `SignalRConcertNotificationService` (internal in Concert.Application)
- `IApplicationNotificationService` + `SignalRApplicationNotificationService`
- `ITicketNotificationService` + `SignalRTicketNotificationService`

For each consuming service, replace the injected `IXNotificationService` with `INotificationModule`
and replace the named-method call with `SendAsync`. Example for MessageService:
```csharp
// before
await notificationService.MessageReceivedAsync(toUserId.ToString(), message.ToDto(fromUser));

// after
await notificationModule.SendAsync(toUserId.ToString(), "MessageReceived", message.ToDto(fromUser));
```

Consuming modules that need `INotificationModule`:
- `Messaging.Infrastructure` → `MessageService`
- `Concert.Infrastructure` → `ConcertService`, `AcceptanceDispatcher` (or wherever ApplicationAccepted fires)
- `Payment.Infrastructure` (or `Concert.Infrastructure`) → ticket purchase notification

Each consuming module's Infrastructure `.csproj` adds a reference to `Notification.Contracts`.

Remove `IMessageNotificationService`, `IConcertNotificationService`, `IApplicationNotificationService`,
`ITicketNotificationService` from `Concertable.Application.Interfaces/`.

### Step 8 — Update Web
- Delete `Concertable.Web/Hubs/` folder
- Delete `Concertable.Web/Services/SignalR*NotificationService.cs` (four files)
- Remove `services.AddSignalR()` from Web DI (now in `AddNotificationModule`)
- Add reference to `Concertable.Notification.Infrastructure`
- Call `builder.Services.AddNotificationModule()`
- Keep `app.MapHub<NotificationHub>("/hubs/notification")` — add
  `using Concertable.Notification.Infrastructure.Hubs`

### Step 9 — Test suite
No migration (no DB). Build + full test suite.

---

## Future — event-driven refactor

Once an outbox is in place, each module raises an integration event instead of calling
`INotificationModule` directly. Notification.Infrastructure gains an
`IIntegrationEventHandler<XEvent>` per event type and becomes a pure consumer. Direct
`INotificationModule` injection across modules retires entirely.

| Event              | Source module | Push event name      |
|--------------------|---------------|----------------------|
| `MessageSentEvent` | Messaging     | `"MessageReceived"`  |
| `TicketPurchasedEvent` | Payment   | `"TicketPurchased"`  |
| `ApplicationAcceptedEvent` | Concert | `"ApplicationAccepted"` |
| `ConcertPostedEvent` | Concert     | `"ConcertPosted"`    |

---

## Progress

- [ ] Step 1 — Scaffold projects
- [ ] Step 2 — Move `ClaimsPrincipalExtensions.GetId()` to `Identity.Contracts`
- [ ] Step 3 — Define `INotificationModule`
- [ ] Step 4 — Move `NotificationHub`
- [ ] Step 5 — Implement `SignalRNotificationModule`
- [ ] Step 6 — `AddNotificationModule()`
- [ ] Step 7 — Replace per-module notification interfaces
- [ ] Step 8 — Update Web
- [ ] Step 9 — Test suite
