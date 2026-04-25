# Notification Module Extraction — Implementation Plan

Planned after Messaging extraction. Notification is a pure event-consumer module — no owned
write tables. Its only concern is pushing real-time events to connected clients via SignalR.

Memory: `project_future_modules.md` documents the module boundary decisions.

---

## Scope

**Notification owns:**
- `NotificationHub` (currently `Concertable.Web/Hubs/NotificationHub.cs`) — SignalR hub,
  manages user connection groups (`OnConnectedAsync` adds to group by userId,
  `OnDisconnectedAsync` removes)
- `IMessageNotificationService` + `SignalRMessageNotificationService`
- `ITicketNotificationService` + `SignalRTicketNotificationService`
- `IApplicationNotificationService` + `SignalRApplicationNotificationService`
- `IConcertNotificationService` + `SignalRConcertNotificationService`

**Not Notification:**
- Email — `Shared.Infrastructure`. Email is a general-purpose transport injected directly
  by any module that needs it. Locking it in Notification would force Identity (verification
  emails), Concert (acceptance emails), and Payment (ticket PDFs) to go through
  Notification.Contracts — wrong coupling.
- User-to-user inbox — that is Messaging.
- `NotificationHub` SignalR routing config (stays in Web/Program.cs as middleware concern).

---

## North-star shape

In north-star state Notification is purely an event consumer. Each `SignalR*NotificationService`
impl gets replaced by an integration event handler that reacts to events from other modules
and pushes to connected clients:

| Event | Source module | Push method |
|---|---|---|
| `MessageSentEvent` | Messaging | `MessageReceived` |
| `TicketPurchasedEvent` | Payment | `TicketPurchased` |
| `ApplicationAcceptedEvent` | Concert | `ApplicationAccepted` |
| `ConcertPostedEvent` | Concert | `ConcertPosted` |

During extraction, the existing `IXNotificationService` interfaces can stay as-is (called
directly by Messaging/Payment/Concert via Contracts). The event-handler refactor is follow-up
work once the outbox is in place.

---

## Key decisions

**No owned DbContext.** Notification has no write tables. If a future "in-app notification
inbox" feature lands, that's a new aggregate that warrants revisiting the module boundary.

**`NotificationHub` moves to `Notification.Infrastructure`**, not `Notification.Api`. The
hub is registered via SignalR middleware plumbing; it isn't a controller. `Program.cs`
stays responsible for `app.MapHub<NotificationHub>("/hubs/notification")` (host concern),
but the hub class itself lives inside the module.

**No `Notification.Domain`** — pure query/push module with no owned writes, mirrors the
Search module pattern (skip Domain layer).

---

## Stage 1 — Implementation steps

### Step 1 — Scaffold Notification projects
- `Concertable.Notification.Contracts` — `INotificationModule` (minimal; possibly just
  the `IXNotificationService` interfaces that other modules inject)
- `Concertable.Notification.Application` — (thin; mostly the service interfaces if not
  in Contracts)
- `Concertable.Notification.Infrastructure` — `NotificationHub`, all four
  `SignalR*NotificationService` impls, `AddNotificationModule()`
- `Concertable.Notification.Api` — `AddNotificationApi()` (no controllers; just wires the
  module + registers hub)

No Domain layer.

### Step 2 — Move NotificationHub to Notification.Infrastructure
Move `Concertable.Web/Hubs/NotificationHub.cs` → `Notification.Infrastructure/Hubs/`.
`Program.cs` keeps `app.MapHub<NotificationHub>(...)` but refs the type from
`Notification.Infrastructure` (via `AddNotificationApi()` which makes the type available).

### Step 3 — Move SignalR service impls to Notification.Infrastructure
Move all four `SignalR*NotificationService` implementations. The `IXNotificationService`
interfaces move to `Notification.Contracts` so callers (Messaging, Payment, Concert) can
inject them via Contracts reference only.

### Step 4 — Wire AddNotificationApi() in Web. Full test suite.

---

## Future: event-driven refactor

Once outbox is in place:

1. Each source module raises an integration event instead of calling `IXNotificationService`.
2. Notification.Infrastructure gains an `IIntegrationEventHandler<XEvent>` per event type.
3. `IXNotificationService` interfaces + impls retire.
4. Notification has zero inbound Contracts references from other modules.
