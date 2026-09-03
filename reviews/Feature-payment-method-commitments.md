# Code review — Feature/payment-method-commitments

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `233ca5c90c644a89a828e6f7c62251abf9236161`  `(2026-09-03)`
**Security-reviewed up to commit:** `233ca5c90c644a89a828e6f7c62251abf9236161`  `(2026-09-03)`
**Judgment:** `changes-requested`

## Review pass — 2026-09-03 — full

**Candidate base:** `a43ca6f0d8a1c5e9995e8b6046344431cd20e0b0`
**Candidate head:** `233ca5c90c644a89a828e6f7c62251abf9236161`
**Candidate branch:** `Feature/payment-method-commitments`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:d278a4cd834c97d17f5445c875e06f9ca746e3cf3b56966fd5fabf7b408a3f1d` `(56 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-review-payment-2f9ce46f4c1e411a887e9000af431eeb`
**Candidate bundle identity:** `sha256:7ac17a7be6e7cd0ca74fe65a6985588b2bacd595ced148bdf701435a3d74c828`
**Work-order path:** `reviews/Feature-payment-method-commitments.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

### Findings

- [x] **PAY-001 — HIGH — correctness** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Services/PaymentOperationResolver.cs:32`
  Reference resolution trusts only the last persisted webhook state. A provider-completed setup or authorization is rejected until its webhook wins the race; the message handlers then persist that rejection as terminal and replay it forever. Reconcile the referenced provider object before classifying a non-ready operation, preserve its concrete failure code, and cover validation plus both reference-based financial paths without a manual refresh.
  Resolved by reconciling current provider truth inside `PaymentOperationResolver`, retaining terminal failure codes, and covering setup validation, authorization resolution, reference deposit/capture, and reference manager payment. Focused integration tests passed (20); the unit suite passed (573).
- [x] **PAY-002 — MEDIUM — domain invariants** — `api/Concertable.Payment/src/Concertable.Payment.Domain/Entities/PaymentSessionAttemptEntity.cs:124`
  `ApplyTransition` mutates the attempt to `Succeeded` before validating the required payment-method identifier. A thrown validation exception therefore leaves an invalid succeeded in-memory entity that a later save can persist. Validate and normalize the identifier before any mutation, including the mapped length limit, and assert that rejection leaves the attempt unchanged.
  Resolved by validating and normalizing the immutable payment-method identifier before mutating observed attempt state. Focused domain tests passed (5), including the unchanged-state assertion.
- [x] **PAY-003 — LOW — C# conventions** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Grpc/ManagerPaymentRequestMappers.cs:65`
  The edited extension container adds another legacy `this` extension even though the C# standard requires migrating the whole touched container to C# 14 `extension()` blocks. Convert every ordinary extension in this mapper together.
  Resolved by converting every extension member in the mapper to receiver blocks. The Payment infrastructure project builds with zero warnings and errors.
- [ ] **PAY-004 — LOW — result contracts** — `api/Concertable.Payment/src/Concertable.Payment.Contracts/Errors/PaymentMethodChargeError.cs:7`
  The new published error union has no exact definition contract inventory. Add both composite cases to `PaymentErrorDefinitionTests`, hard-coding their code, message, and semantic kind.
