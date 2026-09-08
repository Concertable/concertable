# Code review — Fix/FrontendFullE2EQueueDependencies

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `a7491192e18f2b92b0f7df0eda1ad980eec67793`  `(2026-09-08)`
**Security-reviewed up to commit:** `a7491192e18f2b92b0f7df0eda1ad980eec67793`  `(2026-09-08)`
**Judgment:** `approved`

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

- [x] **F1 — MEDIUM — correctness** — `.github/workflows/tests/test_service_scope.py:141`
  `queue_e2e_dependency_cases` intersects the queue-E2E dependency closure with the hand-maintained
  `MATRIX_GUARDS` dict, so the new guard only covers the three jobs someone remembered to list there.
  Add a fourth empty-matrix-guarded job and wire it into `e2e-api-tests`/`e2e-ui-tests`'s closure and the
  gate re-breaks silently — exactly the failure mode this candidate exists to prevent. Derive the
  empty-matrix-guarded set from the frozen workflow instead (every job whose `if` contains `!= '[]'`)
  and intersect the closure against that.

- [x] **F2 — LOW — correctness** — `.github/workflows/test.yml:910`
  The new `needs` comment claims GitHub "skips a downstream job before evaluating its own `if` when a
  dependency is skipped". The `if` is evaluated; what skips the job is the implicit `success()` over
  `needs`, which a skipped dependency does not satisfy — which is also why `always()`/`!cancelled()`
  would override it. State the invariant and the accurate reason so a reader does not conclude the `if`
  is unreachable.

### Remediation

Both findings are resolved by `a7491192e`.

- **F1** — `queue_e2e_dependency_cases` now intersects each queue-E2E dependency closure with
  `empty_matrix_guarded_jobs(spec)`, derived from every job whose `if` carries the `!= '[]'` idiom, instead of
  with the hand-maintained `MATRIX_GUARDS` dict. A fourth guarded job wired into the closure is therefore
  caught without anyone remembering to list it. `MATRIX_GUARDS` is retained for its original assertion that
  each named job carries that exact idiom, which is what keeps the derived set from silently going empty — the
  interlock is stated in the code comment.
- **F2** — the `needs` comment now states that a skipped dependency fails the implicit `success()` over
  `needs`, rather than pre-empting the job's own `if`, and names `ci-complete` as the gate that independently
  waits for `architecture-tests`.

### Security pass

`.github/workflows/` is a security-sensitive path, so this candidate requires a security marker. The diff
carries no security-relevant change: `test.yml` changes ten lines, none of which touch `permissions`,
`secrets.*`, `GITHUB_TOKEN`, `pull_request_target`, registry login or any credential — only one job's `needs`
list and a comment. `e2e-ghcr-login.test.mjs` gains CRLF normalisation when reading the workflow, which fixes
a Windows-checkout false negative in a test and reaches no runtime. `test_service_scope.py` is test-only.
No privilege-elevation, secret-exposure or untrusted-input path is introduced.

### E2E tier

`skip-e2e-ui`. The candidate contains no product runtime code, no wire contract and no browser flow, so the
full browser suite has no positive trigger. It does rewire `needs` on the queue E2E jobs, and that failure
mode is self-concealing — a broken E2E gate hides its own absence — so API E2E is retained to prove live that
an E2E job still fires after the rewiring. The fix's own correctness is proven statically by
`workflow-tests`, which computes the dependency closure from the frozen workflow; a full browser run would
not prove it better, because this candidate's own diff is not frontend-only and so produces no empty backend
matrix to exercise.
