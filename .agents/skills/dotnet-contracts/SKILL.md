---
name: dotnet-contracts
description: Concertable's shared contract shapes — `IPagination<T>.Map` lives in `Concertable.Contracts` beside the interface so every layer including `*.Api` can reach it, an integration event carries its version in the stable `MessageType` wire identity rather than in a suffixed CLR type name, and the shared reference vocabulary is the `Genre` enum. Use when placing a shared contract type, versioning an integration event, or reaching for a lookup table where an enum belongs.
---

# Contracts — where shared shapes live, and how events carry a version

## `IPagination<T>.Map` lives in `Concertable.Contracts`

Beside `IPagination<T>` itself, so every layer can reach it — including `*.Api`, which deliberately does not
reference the data-access package.

## Version the wire identity of an integration event, never its C# type

Keep the CLR event name free of transport-version suffixes. The version belongs in the stable `MessageType`
wire identity:

```text
PaymentOperationStateChanged  ->  concertable.payment.payment-operation-state-changed.v1
```

Never `PaymentOperationStateChangedV1`. Application code talks in domain event names; serializers and brokers
own wire-version selection.

## The shared reference vocabulary is `Genre`

The `module-structure` skill's "shared reference vocabulary is an enum, not a table" rule resolves here to
`Genre`, in `Concertable.Contracts`.
