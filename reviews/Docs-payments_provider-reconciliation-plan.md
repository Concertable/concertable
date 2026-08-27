# Code review — Docs/payments_provider-reconciliation-plan

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]` findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `f1e925f31a2774e875e1b8f7883dfd8eed7d87b4`  `(2026-08-27)`
**Judgment:** `changes-requested`

## Review pass — 2026-08-27 — docs

**Candidate base:** `fe0f9dac14c73027f0c67feb35a932b685530580`
**Candidate head:** `f1e925f31a2774e875e1b8f7883dfd8eed7d87b4`
**Candidate branch:** `Docs/payments_provider-reconciliation-plan`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:0060ff2b0250342dd3c0a02fcc079abd161559dc3657601b5d4dc84d0f8669ed` `(2 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\concertable-docs-review-0e8fd0cd87c6427aa2efee39f82c70cf`
**Candidate bundle identity:** `sha256:1e970adaffd562ee4d488b70020be5d2233871bd31f33ad5f1f5c4319b513b3d`
**Work-order path:** `reviews/Docs-payments_provider-reconciliation-plan.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

### Findings

- [x] **ACC1 — MEDIUM — accuracy** — `plans/payments/PROVIDER_RECONCILIATION_PLAN.md:67`
  Route supported Refund webhook events through current-object retrieval and the refund reconciliation service, with duplicate, reordered, and stale-event coverage.

- [x] **CON1 — MEDIUM — contradiction** — `plans/payments/PROVIDER_RECONCILIATION_PLAN.md:49`
  State that any changed published `Concertable.*` contract needs its own producer plan before this implementation may consume the terminal published baseline.

- [x] **INST1 — MEDIUM — followability** — `plans/payments/PROVIDER_RECONCILIATION_PLAN.md:95`
  State that this work clears only the provider-reconciliation prerequisite; B2B remains blocked on frontend orchestration and active B2B consumer gates.

## Review pass — 2026-08-27 — incremental

**Candidate base:** `f1e925f31a2774e875e1b8f7883dfd8eed7d87b4`
**Candidate head:** `c0c9bbaf36f39b2432cad8eb3019b024c5e5308e`
**Candidate branch:** `Docs/payments_provider-reconciliation-plan`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:f0a8866794cbdee42d31620fd0727a041b61b1e94341779272398bdc89db64de` `(3 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-docs-incremental-47357a854fb44cf7b4fffcc0d9dadbd9`
**Candidate bundle identity:** `sha256:add0e59c0d0e1bdbfc5a8a08e22d278b597d970dff5d1bc2ad3f42f7aefacf2e`
**Work-order path:** `reviews/Docs-payments_provider-reconciliation-plan.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

- [x] **ACC2 — MEDIUM — accuracy** — `plans/payments/PROVIDER_RECONCILIATION_PLAN.md:81`
  Route `refund.succeeded` through current-object retrieval and include it in the deterministic Refund webhook coverage.

- [x] **CON2 — MEDIUM — contradiction** — `plans/payments/STRIPE_RELIABILITY_ROADMAP.md:Implementation DAG`
  Add the frontend-orchestration dependency edge to B2B payment workflows so the DAG matches the item dependency table.

## Review pass — 2026-08-27 — incremental

**Candidate base:** `c0c9bbaf36f39b2432cad8eb3019b024c5e5308e`
**Candidate head:** `05f57298a52cf627ec5d1a81ab1a046c4773262d`
**Candidate branch:** `Docs/payments_provider-reconciliation-plan`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:f658787043daa0f0ca0a8d33420ac04fdd32bd17aec1b706239c160743431e73` `(4 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-docs-final-5e0d9e28072841d7b3a39a7e4aafecbe`
**Candidate bundle identity:** `sha256:e2d8b32f0071e4b4076353237656f92fa344517402cf80f5444d0657bb7b731c`
**Work-order path:** `reviews/Docs-payments_provider-reconciliation-plan.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

- [x] **ACC3 — MEDIUM — accuracy** — `plans/payments/PROVIDER_RECONCILIATION_PLAN.md:81`
  Remove `refund.succeeded`; Refund webhook coverage is limited to created, updated, and failed events.

- [x] **CON3 — MEDIUM — contradiction** — `plans/payments/STRIPE_RELIABILITY_ROADMAP.md:590`
  State that B2B remains independent of Customer work but requires provider reconciliation, frontend orchestration, and its active package/consumer gates.

- [x] **INST2 — LOW — followability** — `plans/payments/PROVIDER_RECONCILIATION_PROGRESS.md:18`
  Replace the stale commit instruction with the actual final incremental review range.

- [x] **ACC4 — LOW — accuracy** — `plans/payments/PROVIDER_RECONCILIATION_PROGRESS.md:24`
  Replace the malformed Refund-event entry with the three supported provider events.

## Review pass — 2026-08-27 — incremental

**Candidate base:** `05f57298a52cf627ec5d1a81ab1a046c4773262d`
**Candidate head:** `847ae2b110e41e45328c8ea5e5c64a83f29ec8ca`
**Candidate branch:** `Docs/payments_provider-reconciliation-plan`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:f658787043daa0f0ca0a8d33420ac04fdd32bd17aec1b706239c160743431e73` `(4 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\concertable-docs-clean-final`
**Candidate bundle identity:** `sha256:40a7156b1005b7b45ba477c4123e003d763c88f3ec34c8e2e9d00e1f94fd7b2b`
**Work-order path:** `reviews/Docs-payments_provider-reconciliation-plan.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

- [~] **INST3 — LOW — followability** — plans/payments/PROVIDER_RECONCILIATION_PROGRESS.md:18$([Environment]::NewLine)  Replace the stale commit instruction with the final incremental-review range.