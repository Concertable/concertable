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
