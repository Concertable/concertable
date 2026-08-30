---
name: concertable-dotnet-domain-events
description: Concertable's domain-event roster — the `Concertable.Kernel` contracts (`IDomainEvent`, `IEventRaiser` plus the composable `EventRaiser`, `IDomainEventHandler<T>`, the `IPreCommitDomainEventHandler<T>` phase marker, `DomainEventDispatcher`), all 13 handlers being pre-commit and each doing nothing but publish one integration event through `OutboxBus`, where the event, handler and closed-interface registration each live per module, and the two interceptors — with `SeedingDomainEventDispatchInterceptor` deliberately running pre-commit handlers after the save while a seeding scope is active. Use when adding or reviewing a domain event or handler here, when a seeded entity's integration event arrives after its row, or before adding the first post-commit handler.
---

# Domain events — Concertable's own pieces

The generic standard is the `domain-events` skill: raise on the entity, dispatch at the save, translate to
the integration event in a pre-commit handler. This file is the roster of the types that shape plays out on
here, and the one place Concertable's behaviour differs from the generic description.

## The Kernel contracts

All in `Concertable.Kernel`:

| Type | Shape |
|---|---|
| `IDomainEvent` | empty marker |
| `IEventRaiser` | `IReadOnlyList<IDomainEvent> DomainEvents` + `ClearDomainEvents()` |
| `EventRaiser` | the composable collection an entity holds as `private readonly EventRaiser events = new()` |
| `IDomainEventHandler<TEvent>` | `HandleAsync(TEvent, CancellationToken)` |
| `IPreCommitDomainEventHandler<TEvent>` | marker deriving from the above; the interceptor uses it to pick the phase |
| `IDomainEventDispatcher` / `DomainEventDispatcher` | `DispatchPreCommitAsync` + `DispatchAsync`, resolving handlers by the event's runtime type |

## Every handler in the system is pre-commit

13 domain events and 13 handlers, and **all 13 are `IPreCommitDomainEventHandler`** — there is no
post-commit handler in Concertable today. Each one does exactly one thing: `bus.PublishAsync(...)` an
integration event. `IBus` resolves to `OutboxBus`, which stages an `OutboxMessageEntity` row on the caller's
ambient context, so the publish commits with the business transaction; `OutboxDispatcher` forwards it after.

A post-commit handler is legal and the dispatcher supports it — but adding the first one means you are
claiming the side effect must not be transactional. Say why in the commit message.

## Where each piece lives

- The event: `<Module>.Domain/Events/XDomainEvent.cs`, a `sealed record` holding the entity, raised from
  intent-named methods on that entity.
- The handler: `<Module>.Infrastructure/Events/XDomainEventHandler.cs`, `internal sealed`.
- The registration: that module's `Infrastructure/Extensions/ServiceCollectionExtensions.cs`, against the
  closed interface — `services.AddScoped<IDomainEventHandler<XDomainEvent>, XDomainEventHandler>()`.

Auth has no modules, so its pair sits in `Concertable.Auth/Data/Events/`.

## Two interceptors, and the seeding one reorders the phases

`DomainEventDispatchInterceptor` (`Concertable.DataAccess.Infrastructure.Data`) is the normal one, and it
also swaps `IDbContextAccessor.Context` to the saving context for the duration of pre-commit dispatch, so a
handler's outbox write lands on the right context.

`SeedingDomainEventDispatchInterceptor` (`Concertable.Seed.Infrastructure`) replaces it while seeding, and
when `SeedingScope.IsActive` it runs the **pre-commit** handlers in `SavedChangesAsync` instead — after the
rows exist. So during seeding a domain event's outbox row is written after the entity's own insert commits,
not in the same transaction: expect a seeded entity's integration event to appear after its row, not with it.
The reason for the reordering is recorded nowhere in the code; treat the behaviour as the contract and do not
"tidy" the phases back together.
