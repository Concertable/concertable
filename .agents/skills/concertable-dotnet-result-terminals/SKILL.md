---
name: concertable-dotnet-result-terminals
description: Concertable's own Result terminal pieces — the reusable gRPC cancellation predicate and operation-detail extraction live in `Concertable.Grpc`, while the concrete error-case map and the contract-mismatch exception stay in the owning client, because they are that client's knowledge of its own remote contract rather than shared infrastructure. Use when writing a gRPC client terminal here, or deciding whether a mapping belongs in shared code or in the calling client.
---

# Result terminals — what Concertable owns at its own edges

The generic standard is the `result-terminals` skill: the HTTP, gRPC, worker and exception terminals and what
an exception becomes. This file is what is specific to this system.

## The reusable gRPC cancellation predicate lives in `Concertable.Grpc`

Along with operation-detail extraction. Both are shared because every gRPC client needs them identically.

**The concrete error-case map and the contract-mismatch exception stay in the owning client.** They are that
client's knowledge of its own remote contract, not shared infrastructure — see the `proto` and
`result-terminals` skills for the generic shape.

## Package ownership is in `PACKAGES.md`

Apply [`../PACKAGES.md`](../../standards/dotnet/PACKAGES.md) to Reunion terminal dependencies; it owns their package pins,
consumer references, private build references and verification.
