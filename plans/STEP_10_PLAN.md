# Step 10 — Idempotent Consumers (Inbox per service)

> Companion to `MICROSERVICE_STEPS.md` Step 10. Implement step-by-step; `/clear` between
> steps and re-enter via `@STEP_10_PLAN.md`.

**Goal:** integration-event handlers become idempotent — re-delivery of the same
`MessageEnvelope.MessageId` is silently deduplicated. Proves the at-least-once lesson
before the ASB transport arrives (Step 14).

**Exit criteria (MICROSERVICE_STEPS.md Phase 2):** inbox proven on at least one event in
each direction — `ConcertChangedEvent` (B2B→Customer) and `ReviewSubmittedEvent`
(Customer→B2B). Together with Step 9 outbox, Phase 2 exit criteria are fully met.

---

## Current state

- `InMemoryBusTransport.PublishAsync<TEvent>` receives `MessageEnvelope` (with `MessageId`
  Guid) but ignores it — dispatches to every registered `IIntegrationEventHandler<TEvent>`
  unconditionally.
- No inbox table, no deduplication anywhere.
- `OutboxDispatcher` assigns `row.Id` as the `MessageEnvelope.MessageId` when draining —
  each outbox row has a stable GUID that can serve as the idempotency key.

## Design

- **D1 — Inbox filter injected into the transport.** `InMemoryBusTransport.PublishAsync` and
  `SendAsync` resolve `IInboxFilter` via `GetService` (null = no-op). If the filter returns
  false (already seen), skip all handlers. No wrapper transport, no DI gymnastics.
- **D2 — Insert-or-skip via unique constraint.** `InboxFilter.IsNewAsync(messageId)` inserts
  an `InboxMessageEntity` row; if EF catches a `DbUpdateException` from a duplicate-PK
  violation, returns false. Row presence = already processed.
- **D3 — `InboxDbContext` mirrors `OutboxDbContext`.** Owns `messaging.Inbox` table;
  migration scaffolded the same way. `Id` (Guid) is the PK and naturally unique — no
  separate UNIQUE index needed.
- **D4 — `AddInbox` extension.** Registers `InboxDbContext`, `IInboxFilter → InboxFilter`
  (scoped). Called in both `Concertable.Web` and `Concertable.Customer.Web` alongside
  `AddOutbox`.
- **D5 — Migrations.** Add `InboxDbContext` to `initial-migrations.ps1`; re-run to
  produce `messaging.Inbox` migration. Module DbContexts do NOT map `InboxMessageEntity`
  (only the reader/filter needs it, and it reads via `InboxDbContext`).

## Components

### `Concertable.Messaging.Domain`
- `InboxMessageEntity` — `Id` (Guid, PK), `MessageType` (string), `ReceivedAt` (DateTimeOffset).
  Minimal: no status field (presence = processed). Private ctor + `Create(Guid, string, DateTimeOffset)`.

### `Concertable.Messaging.Application`
- `IInboxFilter` — `Task<bool> IsNewAsync(Guid messageId, string messageType, CancellationToken ct)`.

### `Concertable.Messaging.Infrastructure`
- `InboxMessageEntityConfiguration` — maps to `messaging.Inbox`; `Id` is `ValueGeneratedNever`.
- `InboxDbContext : DbContext` — applies config; same ctor shape as `OutboxDbContext`.
- `InboxFilter : IInboxFilter` — inserts row via `InboxDbContext`; catches `DbUpdateException`
  to detect duplicates; calls `SaveChangesAsync` before returning.
- `InboxServiceCollectionExtensions.AddInbox(configureDb)` — registers `InboxDbContext` +
  `IInboxFilter → InboxFilter` (scoped).
- Modify `InMemoryBusTransport.PublishAsync` + `SendAsync` to resolve and call `IInboxFilter`.

### Tests
- `InboxFilterTests` — in-memory SQLite provider; verifies first call returns true, second
  call with same id returns false.
- `InboxIdempotencyTests.PostConcert_SecondDispatch_IsDeduplicated` (Concert.IntegrationTests)
  — posts concert, lets dispatcher drain (inbox row inserted), then directly calls
  `IBusTransport.PublishAsync` again with the same `MessageEnvelope.MessageId`; asserts
  `ConcertProjectionHandler` only ran once (check read-model row count or a counter mock).

  **Simpler alternative:** just assert the inbox row exists after the PUT (atomicity proof)
  and write a pure unit test for the filter. Skip the "called twice" integration test for now
  — the unit test is sufficient to prove deduplication logic.

## Steps (one commit each)

1. **Inbox library.** Add `InboxMessageEntity` (Domain); `IInboxFilter` (Application);
   `InboxMessageEntityConfiguration` + `InboxDbContext` + `InboxFilter` (Infrastructure/Inbox);
   `AddInbox` extension; modify `InMemoryBusTransport`; unit tests for `InboxFilter`.
2. **Wire per service.** `AddInbox(UseSqlServer)` in `Concertable.Web` and
   `Concertable.Customer.Web`. Re-run `./initial-migrations.ps1` from `api/` to scaffold
   `InboxDbContext.InitialCreate`.
3. **Verify.** Integration test asserting inbox row created on `ConcertChangedEvent` dispatch
   (atomicity of the dedup record). Update `MICROSERVICE_STEPS.md` — mark Step 10 done.

## Notes

- `InboxFilter.IsNewAsync` calls `SaveChangesAsync` internally so the row is committed before
  the handler runs — if the handler throws, the inbox row is already committed, which prevents
  re-processing on retry. This is intentional: "at least process" is safer than "maybe double-process".
- With ASB (Step 14), the inbox intercept point moves to the ASB receiver's dispatch loop
  (before calling handlers), not `InMemoryBusTransport`. The `IInboxFilter` interface + DB
  layer are reused unchanged; only the call site in the transport adapter changes.
- Step 10 completes Phase 2 exit criteria. Steps 11 (s2s auth) can follow.
