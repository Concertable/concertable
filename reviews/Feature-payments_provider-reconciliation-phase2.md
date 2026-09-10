# Code review — Feature/payments_provider-reconciliation-phase2

> **This file is a work order, not a discussion.** Fix the open `[ ]` findings directly and report
> what changed; tick each `[x]` as it lands.

**Review status:** `complete`
**Reviewed up to commit:** `363c84c8280e170ff5f8eadedafbc92c42676a30`  `(2026-08-29)`
**Judgment:** `approved`

## Review pass — 2026-08-28 — webhook slice

**Candidate base:** `caa13a0a05aa3d101b884f93eca05aaa5d7ad37a`
**Candidate head:** `f23b1708cd29bb90d543f90f377f2fd36163b54e`
**Candidate branch:** `Feature/payments_provider-reconciliation-phase2`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:97e8bd6770d806d88d6a1f36be3315670669dee66f9652a0226b82ac12a264aa` `(22 paths)`
**Work-order path:** `reviews/Feature-payments_provider-reconciliation-phase2.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

Layers: native/general, correctness+concurrency, conventions, changed-behaviour test impact.

### Findings

- [x] **PAY-REC2-001 — LOW — correctness** — `PaymentSessionAttemptEntity.cs` — the published
  `PaymentOperationStateChanged.ExpiresAt` is always `null` because `PaymentSessionAttemptEntity.ExpiresAt`
  is never populated. **Disposition:** tracked, not fixed here — `ExpiresAt` is a pre-existing field on the
  already-published `PaymentOperationStateChanged` contract, and PaymentIntent/SetupIntent session
  operations have no session-level expiry to source it from (the capture deadline is carried separately as
  `CaptureBefore`). Populating or removing it is a published-`Concertable.*`-contract change owed to a
  dedicated producer plan; emitting `null` is compatible.
- [x] **PAY-REC2-002 — LOW — test impact** — the webhook tests only exercised PaymentIntent. Resolved by
  `Webhook_SetupIntent_PublishesStateChangeOnce`.
- [x] **PAY-REC2-003 — LOW — test impact** — no webhook-specific provider-unavailable-deferral test.
  **Disposition:** covered — the webhook path shares the reconciliation service with the eager path, whose
  `RefreshAsync_ProviderRetrievalUnavailable_PersistsReconciliationRequirement` already proves the deferral
  (`NextReconcileAt` set, no publish); a webhook-specific duplicate covers no distinct code.

## Review pass — 2026-08-29 — state-machine + DDD reshape

**Candidate base:** `f23b1708cd29bb90d543f90f377f2fd36163b54e`
**Candidate head:** `ebd98bf39fa5c0f35c7aaa653175dd22b91c9584`
**Candidate branch:** `Feature/payments_provider-reconciliation-phase2`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:1561a1cae923e01d2387cd5d4ae3fc97da48487eda91c34c47f66a4e536782ec` `(68 paths)`
**Work-order path:** `reviews/Feature-payments_provider-reconciliation-phase2.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

Layers: native/general, correctness+behaviour-preservation. Verified: the intent/refund edge tables are
edge-for-edge equivalent to the deleted adjacency maps (plus self-edges; terminals self-only);
terminal-protection keeps the exact `TerminalStateProtected` reason `RetryAsync` depends on; freshness,
capture-deadline, and normalization rejections are preserved; publish-once holds via the observable-change
gate + self-edges with no double-publish on a concurrency loser; DI wiring is clean and nothing references a
deleted type.

### Findings

- [x] **PAY-REC2-004 — LOW — correctness** — `PaymentSessionStateMachine.Evaluate` checked the
  capture-deadline invariant before transition legality, so an illegal edge into `Authorized` reported
  `CaptureDeadlineRequired` instead of `IllegalTransition`. Fixed: legality is checked first.
- [x] **PAY-REC2-005 — LOW — reuse/dead-code** — `PaymentProviderOperationContextExtensions`
  (`SupportsState` / `HasSameProviderProductAs`) was orphaned after the evaluators were removed. Deleted.
- [x] **PAY-REC2-006 — LOW — reuse/dead-code** — `PaymentRefundStateMachine` was unreferenced Phase-3
  scaffolding. Deleted; Phase 3 will add the refund machine with its consumer.
- [x] **PAY-REC2-007 — decision — correctness** — the "an automatic-capture `Payment` can never be
  `Authorized`" guard had been dropped in favour of relying on Stripe's capture mode. Restored as a one-line
  invariant in `Evaluate` (`InvalidProviderObjectForSessionKind`), keeping the money-path defence-in-depth.
