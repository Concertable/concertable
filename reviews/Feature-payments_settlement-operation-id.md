# Code review — Feature/payments_settlement-operation-id

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Judgment:** `clean`
**Reviewed up to commit:** `ae5fd3e5fef9d05546e7d9e82bc8aad79e8c2bbd`
**Security-reviewed up to commit:** `ae5fd3e5fef9d05546e7d9e82bc8aad79e8c2bbd`

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

## Review pass — 2026-08-26 — incremental

**Candidate base:** `78900ee40185929538ad45ac367e9f07d7f9d260`
**Candidate head:** `cf3caac66ac324510f4f365bba75348b6d30e8cb`
**Candidate branch:** `Feature/payments_settlement-operation-id`
**Candidate scope:** `incremental`
**Candidate path-set:** `sha256:5f1ac0202d2acb3c4a7d7d7dba5174a3679d3d3dd5797a3efff1b993cc3a9421` `(30 paths)`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

- [x] **NAT4 — MEDIUM — typed convergence** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Repositories/EscrowRepository.cs:48`
  Reusing one release operation ID across two escrows now translates the duplicate reservation into the published `OperationConflict`. Covered at the service boundary and with two escrows in the real database.

- [x] **NAT5 — MEDIUM — replay availability** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/ManagerPaymentService.cs:211`
  A completed settlement that originally required 3DS now returns its locally persisted terminal outcome without calling Stripe. Added regression coverage proving replay succeeds without a provider call.

## Review pass — 2026-08-26 — incremental

**Candidate base:** `cf3caac66ac324510f4f365bba75348b6d30e8cb`
**Candidate head:** `ae5fd3e5fef9d05546e7d9e82bc8aad79e8c2bbd`
**Candidate branch:** `Feature/payments_settlement-operation-id`
**Candidate scope:** `incremental`
**Candidate path-set:** `sha256:72a99cb867cfaab10e653d722e6dadf2240cbeb792cc4ab3cd8e8dec8f4dbab1` `(8 paths)`
**Work-order mode:** `append`
**Pass judgment:** `clean`

### Findings

None.
