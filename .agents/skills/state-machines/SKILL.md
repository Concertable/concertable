---
name: state-machines
description: Concertable's Result-based immutable state-machine convention — the generic transition algorithm in Concertable.Kernel, contextual workflow ownership in consuming modules, frozen snapshot and duplicate-edge invariants, operation-owned mutation and errors, direct Reunion ownership, and the behavioral and package verification owed by an implementation. Use when adding or changing a lifecycle state machine, transition table, state or trigger type, or transition failure handling.
---

# State machines — shared transition algorithm, contextual workflow ownership

`Concertable.Kernel` owns one reusable deterministic transition algorithm. A consuming module owns the
workflow: its state and trigger types, configured transition table, contextual names, aggregate mutation and
operation error. Do not publish a configured business workflow from Kernel or move this abstraction into
Reunion.

The shared contract is fixed:

```csharp
public interface IStateMachine<TState, TTrigger>
    where TState : notnull
    where TTrigger : notnull
{
    Result<TState, TransitionError<TState, TTrigger>> Transition(
        TState current,
        TTrigger trigger);
}

public sealed record TransitionError<TState, TTrigger>(
    TState Current,
    TTrigger Trigger);
```

`StateMachine<TState, TTrigger>` takes an enumerable of `(Current, Trigger, Next)` edges and copies it into a
`FrozenDictionary<(TState State, TTrigger Trigger), TState>` during construction. That snapshot has four
observable guarantees:

- a defined edge returns its actual next state;
- an undefined edge returns `TransitionError` with the attempted current state and trigger;
- a duplicate `(Current, Trigger)` edge is invalid construction and throws `ArgumentException`;
- later mutation of the source collection cannot alter the machine, and concurrent reads are safe.

The machine stores no current state. It has no mutable configuration API, guards, callbacks, dependency
injection registration, persistence, retries, event publication or lifecycle side effects. If a transition
needs any of those, the consuming operation owns that work around the pure lookup.

## Consumer ownership

Keep the configured table in the module that owns the lifecycle, and use that module's vocabulary for its
state, trigger and collaborator names. Give the **state and trigger types** contextual domain names —
`TenantVerificationStatus`/`TenantVerificationTrigger`, not the generic `State`/`Trigger` — because those
two are the shared algorithm's own type-parameter vocabulary, not domain names to reuse. That constrains
the state and trigger *type* names, not every identifier sharing a word with the interface: a
`private static readonly IStateMachine<...> StateMachine = ...` field is exactly right when that is
literally what the field holds.

An operation passes the aggregate's current state to `Transition`. On success it applies the returned next
state and only then performs the operation's remaining effects. On failure it leaves the aggregate unchanged
and composes `TransitionError<TState, TTrigger>` into the operation-owned closed error union. Do not make the
generic machine own an operation error or mutate an aggregate itself.

Because the public contract contains a Reunion carrier, apply the dependency ownership rules in
[`PACKAGES.md`](PACKAGES.md).

## Verification

Kernel's behavioral tests prove the four construction and lookup guarantees above, including concurrent
reads. A consuming module tests its complete configured edge table, its rejected edges, successful aggregate
mutation and the no-mutation failure path. Apply the package verification in [`PACKAGES.md`](PACKAGES.md)
alongside those behavioral tests.
