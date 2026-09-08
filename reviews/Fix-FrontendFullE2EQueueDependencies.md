# Code review — Fix/FrontendFullE2EQueueDependencies

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `in-progress`
**Judgment:** `pending`

## Review pass — 2026-09-08 — full

**Candidate base:** `ed5c0fce602fc6a2e9aaa65cfe74970c51dc7c90`
**Candidate head:** `77825a5887dce6aedc2e481749b7cb36b064549b`
**Candidate branch:** `Fix/FrontendFullE2EQueueDependencies`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:9c03b2b73219eaee1d83b0481a503fb670593ba79e663dda725c3fa63fe2fb9d` `(3 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable--worktrees-Fix-FrontendFullE2EQueueDependencies\ee2eb30f-9c26-4e7a-a72f-86f9ac88c7ec\scratchpad\bundle-952`
**Candidate bundle identity:** `sha256:d44ae74d0a7d77f902514580cb9204e119d8b4650a45189fcdc4fa802b15f984`
**Work-order path:** `reviews/Fix-FrontendFullE2EQueueDependencies.md`
**Work-order mode:** `new`
**Pass judgment:** `pending`

### Findings

- [ ] **F1 — MEDIUM — correctness** — `.github/workflows/tests/test_service_scope.py:141`
  `queue_e2e_dependency_cases` intersects the queue-E2E dependency closure with the hand-maintained
  `MATRIX_GUARDS` dict, so the new guard only covers the three jobs someone remembered to list there.
  Add a fourth empty-matrix-guarded job and wire it into `e2e-api-tests`/`e2e-ui-tests`'s closure and the
  gate re-breaks silently — exactly the failure mode this candidate exists to prevent. Derive the
  empty-matrix-guarded set from the frozen workflow instead (every job whose `if` contains `!= '[]'`)
  and intersect the closure against that.

- [ ] **F2 — LOW — correctness** — `.github/workflows/test.yml:910`
  The new `needs` comment claims GitHub "skips a downstream job before evaluating its own `if` when a
  dependency is skipped". The `if` is evaluated; what skips the job is the implicit `success()` over
  `needs`, which a skipped dependency does not satisfy — which is also why `always()`/`!cancelled()`
  would override it. State the invariant and the accurate reason so a reader does not conclude the `if`
  is unreachable.
