# Code review — Feature/payment-method-commitments

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `66975737e3d40960481e4bb970445aeb0c04bc48`  `(2026-09-05)`
**Security-reviewed up to commit:** `66975737e3d40960481e4bb970445aeb0c04bc48`  `(2026-09-05)`
**Judgment:** `approved`

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
- [x] **PAY-004 — LOW — result contracts** — `api/Concertable.Payment/src/Concertable.Payment.Contracts/Errors/PaymentMethodChargeError.cs:7`
  The new published error union has no exact definition contract inventory. Add both composite cases to `PaymentErrorDefinitionTests`, hard-coding their code, message, and semantic kind.
  Resolved by adding exact code, message, and kind assertions for both composite cases. Focused error-contract tests passed (72).

## Review pass — 2026-09-03 — incremental

**Candidate base:** `233ca5c90c644a89a828e6f7c62251abf9236161`
**Candidate head:** `9c6c30c0b027eb3da59dcd56d4ef6b1e2725537a`
**Candidate branch:** `Feature/payment-method-commitments`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:66a37995863a09186dcd43286a3bb570ace9985bd75c10cbb25bd0eb2d267353` `(11 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-review-payment-5cad6e294b8e406d9a561d096adadeeb`
**Candidate bundle identity:** `sha256:f260d7d08c7ecc8a70df1a94e5f90983811230eb6fecac7a3acdc498edbfce11`
**Work-order path:** `reviews/Feature-payment-method-commitments.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

- [x] **PAY-005 — MEDIUM — domain invariants** — `api/Concertable.Payment/src/Concertable.Payment.Domain/Entities/PaymentSessionAttemptEntity.cs:137`
  The remediation validates the payment-method identifier before mutation, but `ApplyTransition` still assigns `LastAttemptedAt` and `State` before validating the required provider status and optional provider diagnostics. Invalid provider input can therefore still leave a partially transitioned in-memory entity. Normalize every supplied diagnostic before the first mutation and prove rejection leaves all state unchanged.
  Resolved by normalizing every provider diagnostic before the first assignment and asserting an oversized diagnostic leaves state, timestamps, diagnostics, and events unchanged. Focused domain tests passed (6).
- [x] **PAY-006 — HIGH — correctness** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Services/PaymentOperationResolver.cs:123`
  A transient provider retrieval or reconciliation failure is returned as `ProviderUnavailable`, but both reference-based queue handlers pass every resolver error to `RejectAsync`, making the financial operation terminal and replaying that rejection forever. Preserve the operation as pending and surface a retryable handler failure for provider-unavailable resolution; cover the persisted pending state and subsequent successful replay.
  Resolved by surfacing provider unavailability as a retryable handler exception without rejecting the durable operation. The SQL-backed replay scenario passed and proved the operation remains pending before succeeding after provider recovery.
- [x] **PAY-007 — LOW — test architecture** — `api/Concertable.Payment/tests/Concertable.Payment.UnitTests/Infrastructure/FinancialOperationHandlerTests.cs:48`
  The new reference-deposit, reference-capture, and manager-payment tests are mock-interaction orchestration tests, which the routed unit-test standard assigns to integration. Replace them with SQL-backed scenarios that use the real operation resolver and persistence boundary while faking only the external payment provider seams.
  Resolved by replacing the three orchestration unit tests with SQL-backed integration scenarios using the real resolver, repositories, unit of work, audit interceptor, and persisted reloads. Focused integration tests passed (3).

## Review pass — 2026-09-04 — incremental

**Candidate base:** `9c6c30c0b027eb3da59dcd56d4ef6b1e2725537a`
**Candidate head:** `5903062f6b0e8d3a2c4c6d3d82e1bf655223a8a4`
**Candidate branch:** `Feature/payment-method-commitments`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:3ea68765a7314ff44f8537aedc1af4ef6845ca2cdc17c0909b15a7ef918105fa` `(9 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-review-payment-b71d686fe87c44e8bbaf7e885234ad04`
**Candidate bundle identity:** `sha256:971081043bb4f18c373192737be216ccbe8db2d17ec8903f7177b0f1856353d2`
**Work-order path:** `reviews/Feature-payment-method-commitments.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

- [x] **PAY-008 — LOW — test architecture** — `api/Concertable.Payment/tests/Concertable.Payment.IntegrationTests/ReferencePaymentOperationTests.cs:69`
  The replacement scenarios use SQL but still manually construct the handler and manager service while mocking the internal `IEscrowService` and `IPaymentManager`, so they remain orchestration tests and do not satisfy PAY-007's requirement to exercise the real composition while faking only external provider seams. Drive the scenarios through a real Payment host/service-provider composition with the real internal services and repositories, replacing only Stripe and transport boundaries, then assert the persisted outcomes.
  Resolved by adding Payment's host-backed integration fixture and driving the registered handlers and manager-payment service through the production container with real internal collaborators. Only the Stripe session adapter and bus transport are replaced; focused host integration tests passed (3).

## Review pass — 2026-09-04 — incremental

**Candidate base:** `5903062f6b0e8d3a2c4c6d3d82e1bf655223a8a4`
**Candidate head:** `eef36ac547f8a61c025af2f428c45317a64223de`
**Candidate branch:** `Feature/payment-method-commitments`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:35a66b2d88601b7b11d81d2cab7475ef002ee46ba0d42b58458e501f7683b1d4` `(13 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-review-payment-9ea2edc5ebec47e9808de6b8563bd2d4`
**Candidate bundle identity:** `sha256:4b4aa47258ad1dccda1f92b8c7a61ac985cfb3a7ed1c463dfbef605e0d1f1742`
**Work-order path:** `reviews/Feature-payment-method-commitments.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No findings.

## Review pass — 2026-09-04 — incremental

**Candidate base:** `eef36ac547f8a61c025af2f428c45317a64223de`
**Candidate head:** `6510ca80cc7b27557512eac2f24f859ab1269254`
**Candidate branch:** `Feature/payment-method-commitments`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:affce3e3b644f7abf16c1f35b129e69892f934beebe227b2d28ed4a54f2829c2` `(2 paths)`
**Candidate bundle:** `local immutable Git objects`
**Candidate bundle identity:** `sha256:affce3e3b644f7abf16c1f35b129e69892f934beebe227b2d28ed4a54f2829c2`
**Work-order path:** `reviews/Feature-payment-method-commitments.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No findings.

## Review pass — 2026-09-04 — incremental

**Candidate base:** `6510ca80cc7b27557512eac2f24f859ab1269254`
**Candidate head:** `060e7619e503b4283040d2cd34b196d01c97dec4`
**Candidate branch:** `Feature/payment-method-commitments`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:b407949401013a144915dff82ed75eb4d37646d7a35784e5ffb3c6c4ce99755c` `(65 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable--worktrees-Feature-payment-method-commitments\ba80e5ef-1310-4d8a-8735-834a064fd1da\scratchpad\review-bundle`
**Candidate bundle identity:** `sha256:c9f546a44816682f4694c6a830766daecd898372359053fe423802bc9a56a9ff`
**Work-order path:** `reviews/Feature-payment-method-commitments.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

- [x] **PAY-008 — MEDIUM — result contracts** — `api/Concertable.Payment/src/Concertable.Payment.Application/Errors/PaymentRejection.cs:3`
  `PaymentRejection` and `ManagerPaymentRejection` are `readonly record struct` types used as the `TError` of
  `Result<PaymentOutcome, …>` across `IStripePaymentIntentClient.ChargeAsync`, `IPaymentManager.SettleAsync` /
  `SettleBoundCommissionAsync`, and `ManagerPaymentService.PayCoreAsync`. The `result-errors` standard requires every
  `TError` to be a closed, operation-owned `XError` union implementing `IError`, declared with Dunet and implicit
  conversions off, with `Definition` derived in one exhaustive switch. These instead carry a `PaymentRecovery` enum
  discriminant beside the error rather than a named case, and add `Declined` / `Unrecoverable` / `FromPayment` wrapper
  factories the same standard bans. Replace both with internal Dunet unions owning an `AuthenticationRequired` case and
  a composite case forwarding the nested definition, delete the enum, and match the union exhaustively where it is
  translated to `PaymentMethodChargeError`.
  Resolved by replacing both structs with internal operation-owned Dunet unions `ChargeError` and
  `ManagerChargeError`, each with an `AuthenticationRequired` case and a composite case forwarding the nested
  definition, and by deleting `PaymentRecovery` and every wrapper factory. `ChargeErrorMappers` translates between
  them and to `PaymentError` / `ManagerPaymentOperationError` through exhaustive switches with no discard arm, and
  `ManagerPaymentService` matches the union exhaustively where it produces `PaymentMethodChargeError`. Both unions
  gained exact definition-contract rows with derived codes. Payment unit tests passed (589), integration (59),
  architecture (9).

## Review pass — 2026-09-04 — incremental

**Candidate base:** `060e7619e503b4283040d2cd34b196d01c97dec4`
**Candidate head:** `448316d2a260e1507dc1c8e1ca3dba607fb5b9ec`
**Candidate branch:** `Feature/payment-method-commitments`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:5f14a6f922b4262183d5fcde9b05fa821dcdae983823ac6fe439f5546cecc020` `(20 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable--worktrees-Feature-payment-method-commitments\ba80e5ef-1310-4d8a-8735-834a064fd1da\scratchpad\review-bundle`
**Candidate bundle identity:** `sha256:c9f546a44816682f4694c6a830766daecd898372359053fe423802bc9a56a9ff`
**Work-order path:** `reviews/Feature-payment-method-commitments.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No findings. The PAY-008 remediation delta was reviewed against `result-carriers`, `result-errors`, and
`csharp-naming`: both replacement unions are closed, operation-owned, Dunet-declared with implicit conversions
off, and derive `Definition` in one exhaustive switch with no discard arm; every translation between them
matches all cases and preserves the observed error rather than reconstructing one. The remaining `rejection!`
extractions are the already-recorded Reunion non-null-accessor debt, not new.

## Review pass — 2026-09-05 — full staged

**Candidate base:** `a1244c4542df12a3db96d5119acae91338e4dde1`
**Candidate head:** `98b56896a27519b67059e4036939436ccfdc7103`
**Candidate branch:** `Feature/payment-method-commitments`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:dad14f14b7692f3c33cd9a1399015e74d07d4764938908f74cbe3ad92c7fd9e3` `(269 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\concertable-review-payment-2479592f62104d47874a03e0c5bcfb8b`
**Candidate bundle identity:** `sha256:611635e36f77d836039e36ca6eccd647addddb78c8de549672f9cbc42542d5b2`
**Work-order path:** `reviews/Feature-payment-method-commitments.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

- [x] **PAY-009 — HIGH — service boundary** — `api/Concertable.Payment/src/Concertable.Payment.Contracts/PaymentOutcome.cs:3`
  Payment's published v1 responses still expose Stripe identifiers through `PaymentOutcome.TransactionId`,
  `EscrowDeposit.ChargeId`, `Transfer.TransferId`, `Refund.RefundId`, their protobuf response fields, and the
  `PaymentSucceededEvent` / `PaymentFailedEvent` transaction identity. That
  preserves provider knowledge in every consuming product and contradicts the agnostic boundary this cut-over
  establishes. Split provider execution results from public operation outcomes, return only Payment-owned
  references or status at the public edge, and add an architecture/contract guard that rejects provider
  identifiers in the published client, contracts, and operation-response protobuf surface.
  Resolved by separating provider execution DTOs from public outcomes, publishing only opaque operation
  references, reserving the removed protobuf fields, and adding a reflection guard over both published
  assemblies. Payment architecture tests passed (13).
- [x] **PAY-010 — HIGH — correctness** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Services/EscrowService.cs:1`
  Opaque operation references are checked only for whitespace at several gRPC/client paths and are not bounded
  consistently before a money-moving provider call. An overlong operation type or client reference can therefore
  create a Stripe authorization or settlement and only then fail when Payment persists into its bounded columns,
  orphaning the provider-side mutation. Centralize reference normalization and length validation, enforce it at
  generated-client construction and every gRPC ingress before invoking application services, align persistence
  limits to the canonical contract, and prove invalid/default/overlong references fail before a provider call.
  Resolved by making `PaymentOperationReference` a validated `readonly record struct`, carrying it as one value
  through requests and repositories, applying its 100/200 limits to every persisted operation-reference pair,
  and rejecting invalid gRPC requests before the service is called. Payment unit tests passed (545) and the
  focused gRPC integration tests passed (9).
- [x] **PAY-011 — LOW — C# conventions** — `api/Concertable.Payment/src/Concertable.Payment.Client/Adapters/PaymentMappers.cs:1`
  Several extension containers changed by this candidate retain or add legacy `this` extension methods even though
  the C# standard requires a touched container to use C# 14 `extension()` blocks. Convert every ordinary extension
  member in each changed container together; source-generated logging extensions remain exempt.
  Resolved by converting every changed ordinary extension container as a unit. The changed-path scan contains no
  legacy ordinary extension declaration, and the Payment solution builds with zero warnings and errors.

## Coverage

- [x] Contracts, client, domain, and application — 100 files — `api/Concertable.Payment/src/Concertable.Payment.{Contracts,Client,Domain,Application}/**`; `api/Concertable.Payment/tools/**`
- [x] Runtime and persistence — 78 files — remaining changed paths under `api/Concertable.Payment/src/**`
- [x] Tests, docs, plans, and everything else — 91 files — all remaining changed paths

## Rules manifest

Route source: `.agents/skill-routes.json` at candidate tree `5bafa68886cdf88c837fccbd13abbd8a00267d0c`

- Contracts, client, domain, and application — skills: routed .NET naming, style, DI, domain-events, module-structure, persistence, proto, result-carriers, result-errors, and Payment boundary standards; local guidance: root and `api/Concertable.Payment/AGENTS.md`; security: yes
- Runtime and persistence — skills: routed .NET naming, style, DI, domain-events, HTTP, logging, migrations, multitenancy, persistence, result terminals, seeding, and Payment boundary standards; local guidance: root and `api/Concertable.Payment/AGENTS.md`; security: yes
- Tests, docs, plans, and everything else — skills: composition-testing, docs-and-debt, E2E, integration-testing, packages, unit-testing, and routed counterparts; local guidance: root plus changed-path `AGENTS.md` files; security: yes

## Cross-area notes

The raw provider-identifier leak also existed on the legacy webhook event path. The repair now resolves provider
objects privately from opaque references, and a succeeded or failed webhook without reference metadata is a
logged no-op rather than a provider-ID event.

## Parent finalization

**Cross-area notes status:** `complete`
**Parent summary status:** `complete`

## Review pass — 2026-09-05 — incremental

**Candidate base:** `98b56896a27519b67059e4036939436ccfdc7103`
**Candidate head:** `8fb94d14042a30bb2a28dc0838896ad1a7145c7d`
**Candidate branch:** `Feature/payment-method-commitments`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:1c0146c59b06091eaaf7a10729168ba1c81de489c0cfd1aecad9bdfc10042c18` `(101 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\concertable-review-payment-538b3be5c9294a44bf48c3e71483ccb2`
**Candidate bundle identity:** `sha256:70d55862b20706c0bec6eb4c28ef666f9f7be30836b2d3f05b7083dbaf02f258`
**Work-order path:** `reviews/Feature-payment-method-commitments.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

- [x] **PAY-012 — LOW — C# conventions** — `api/Concertable.Payment/src/Concertable.Payment.Contracts/PaymentMethodOperations.cs:9`
  The new `PaymentOperationReference` constructor assigns its public auto-properties without the `this.`
  qualification required for every constructor assignment. Qualify both assignments and retain the value
  type's validated `readonly record struct` shape.
  Resolved by qualifying both constructor assignments. The focused value-object tests passed (8).

## Review pass — 2026-09-05 — incremental

**Candidate base:** `8fb94d14042a30bb2a28dc0838896ad1a7145c7d`
**Candidate head:** `66975737e3d40960481e4bb970445aeb0c04bc48`
**Candidate branch:** `Feature/payment-method-commitments`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:5c6ee8a85ef2c591c43cdf616ea6f003e8a47f9b965972b6e7ca304d74bebe78` `(2 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\concertable-review-payment-9ea8cdfd6d80443585e92044e0dde53d`
**Candidate bundle identity:** `sha256:1c0a5579e04f938b840e74e067b5035d469c7bc1d6bb35d7e5dd1b2b5b138011`
**Work-order path:** `reviews/Feature-payment-method-commitments.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No findings. The only code change qualifies the two constructor assignments required by PAY-012; it does
not alter reference normalization, public contract shape, persistence, authorization, or provider isolation.
