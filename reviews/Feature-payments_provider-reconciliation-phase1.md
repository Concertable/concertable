# Code review — Feature/payments_provider-reconciliation-phase1

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `08582baab75b9753ef5f959aa395ea663e11e698`  `(2026-08-28)`
**Judgment:** `approved`

## Review pass — 2026-08-28 — full

**Candidate base:** `95134600526276eebecd63b2096928a9bb7b5f1e`
**Candidate head:** `4cd3d1d49d995a9d60c60a41c81e7dc1ce6f91e1`
**Candidate branch:** `Feature/payments_provider-reconciliation-phase1`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:99d0fc9c2c5d09202fdc110e8039a1b42e5813073f2266bad08f79ad17eebaa4` `(11 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\concertable-review-payments-phase1`
**Candidate bundle identity:** `sha256:2992a69ea3b3702d42bc4d13bc1e6c106b28d5950a77794e751559c80df2e74d`
**Work-order path:** `reviews/Feature-payments_provider-reconciliation-phase1.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

### Findings

- [x] **PAY-REC-001 — MEDIUM — changed-behaviour test impact** — `api/Concertable.Payment/tests/Concertable.Payment.IntegrationTests/PaymentSessionServiceTests.cs:112`
  Add a provider-retrieval failure scenario that proves the eager path persists `NextReconcileAt` without changing the canonical normalized state.
  Resolved by `RefreshAsync_ProviderRetrievalUnavailable_PersistsReconciliationRequirement`; the focused integration project build passes with zero warnings.

## Review pass — 2026-08-28 — incremental

**Candidate base:** `4cd3d1d49d995a9d60c60a41c81e7dc1ce6f91e1`
**Candidate head:** `83a4f1c43c117a37d6615029efb1c429bbe53f1c`
**Candidate branch:** `Feature/payments_provider-reconciliation-phase1`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:f5bab8f00c1bfaa9f584e62ffafa9bdb08d88135e2bb8a86f9f3f2817bc46553` `(4 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\concertable-review-payments-phase1-inc`
**Candidate bundle identity:** `sha256:67c4d870134c7438537b3b6e6013d6ea4f958951414dbeed9d9de1f48fc6c7e9`
**Work-order path:** `reviews/Feature-payments_provider-reconciliation-phase1.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No findings.

## Review pass — 2026-08-28 — incremental

**Candidate base:** `83a4f1c43c117a37d6615029efb1c429bbe53f1c`
**Candidate head:** `08582baab75b9753ef5f959aa395ea663e11e698`
**Candidate branch:** `Feature/payments_provider-reconciliation-phase1`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:99901ef04c11fa3dbcd5bee71d4c1bdae3475194729acbb2b2f361be406f6532` `(3 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\concertable-review-payments-phase1-naming-c1499243d9b84d028b26c197a5d46f0d`
**Candidate bundle identity:** `sha256:94af9760c3268d5c106701f689f177887d2bdf42d5dd3c03489755065034f63f`
**Work-order path:** `reviews/Feature-payments_provider-reconciliation-phase1.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Findings

No findings.
