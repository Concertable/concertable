# Code review — Feature/payments_settlement-operation-id

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Judgment:** `changes-requested`
**Reviewed up to commit:** `78900ee40185929538ad45ac367e9f07d7f9d260`
**Security-reviewed up to commit:** `78900ee40185929538ad45ac367e9f07d7f9d260`

## Review pass — 2026-08-26 — full

**Candidate base:** `3737df205093c0f6e5d1f7e6597e3b7eb48e9e12`
**Candidate head:** `78900ee40185929538ad45ac367e9f07d7f9d260`
**Candidate branch:** `Feature/payments_settlement-operation-id`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:8274e89b95c00eb6a086300a7eaf49141c9b0625c9552712151f4d1d54f48e67` `(44 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-review-78900ee40185929538ad45ac367e9f07d7f9d260`
**Candidate bundle identity:** `sha256:871ee05d93398e284e74a9de880b5507384e16376c54482faf675d1b59d9de62`
**Work-order path:** `reviews/Feature-payments_settlement-operation-id.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

### Findings

- [x] **NAT1 — HIGH — persistence** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Services/TransactionService.cs:34`
  `LogAsync` staged webhook transactions without saving them, so successful ticket and verification transactions could be discarded. Restored the repository's durable `CreateAsync` contract and added a focused regression test.

- [x] **NAT2 — HIGH — concurrency** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/EscrowService.cs:350`
  Competing release operation IDs could both pass the in-memory Held check and issue distinct Stripe transfers. Release ownership is now claimed through one atomic conditional update before Stripe; losing requests reload the canonical reservation and fail closed. Added two-context database coverage.

- [x] **NAT3 — MEDIUM — convergence** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/ManagerPaymentService.cs:104`
  Concurrent identical settlement operations could collide on the unique operation/payment identity and throw instead of converging. Duplicate-key losers now clear failed tracked state, reload and fingerprint-check the winner, and return its provider-refreshed outcome. Added two-context database coverage.

- [x] **SEC1 — MEDIUM — secret handling** — `api/Concertable.Payment/src/Concertable.Payment.Domain/Entities/SettlementTransactionEntity.cs:46`
  Settlement replay persisted Stripe client secrets in plaintext. The entity and generated schema now persist only the safe PaymentIntent identity and state; an authorized matching replay retrieves any required secret directly from Stripe at the response boundary.
