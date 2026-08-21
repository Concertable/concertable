# Code review — Feature/payments_payment-session-state

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `8fe54fc665afc7bcd0e66948c75dfdf88761c011`  _(2026-08-21)_
**Security-reviewed up to commit:** `8fe54fc665afc7bcd0e66948c75dfdf88761c011`  _(2026-08-21)_

> Range reviewed: `69df07b8..7e165607` (6 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — MEDIUM — native/correctness** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Services/PaymentSessionService.cs:139`
  Concurrent duplicate retry requests can both observe a cancellable predecessor; after the first request cancels it, the second treats the provider's already-canceled response as `ProviderUnavailable` and returns before repository reservation can replay the winner's successor. Make predecessor cancellation convergent by re-reading after cancellation failure and accepting a confirmed canceled state before reserving or replaying the successor.
- [x] **SEC1 — HIGH — security** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Services/PaymentSessionService.cs:116`
  Retry authorization accepts either participant, but a successful retry returns the payer's PaymentIntent client secret, CustomerSession secret, and Stripe customer token. Require the retry owner to equal the persisted payer owner; keep participant-wide authorization only on the secret-free status read, and test that a payee retry returns the indistinguishable unknown-operation failure without calling Stripe.
- [x] **SEC2 — MEDIUM — security/correctness** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Services/PaymentSessionService.cs:137`
  Retry cancels the current Stripe object before the persisted attempt is evaluated for retry eligibility, so a retry of a nonterminal or authorized attempt can destroy the live payment or hold and only then return `OperationConflict`. Refresh and normalize provider truth, evaluate the explicit-retry policy, and cancel only after it approves a new attempt; test that retrying an authorized or nonterminal attempt does not call cancellation.
- [x] **SEC3 — MEDIUM — security/correctness** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Services/PaymentSessionService.cs:138`
  Retry normalizes provider truth only while the persisted attempt is nonterminal. A stale persisted terminal failure can therefore cancel a provider object that has advanced to an active or unknown state. Normalize every retrieved observation before cancellation; for a protected terminal row, require known provider truth compatible with retry without rewriting history, and test persisted `Failed` plus provider `requires_capture` makes no cancellation and no successor.

## Incremental review — 2026-08-21

> Range reviewed: `7e165607..e7f2e36a` (6 commits).

No new Payment work-order findings. The range contains this review checkpoint plus the merged N3
guidance/meta-only series; native, security, docs ownership, skill-route, architecture, and plan/review
lifecycle lenses were clean. The native pass rediscovered N3's deleted-`api/AGENTS.md` workflow reference,
already owned as `ACC1` by `plans/docs/POLYREPO_READY_PROGRESS.md` on its dedicated follow-up branch, so it
is not duplicated here.

## Incremental review — 2026-08-21

> Range reviewed: `e7f2e36a..9751bd83` (4 commits).

The native correctness, reuse, efficiency, and error-handling pass was clean. The security pass found
`SEC3`: a persisted terminal failure can bypass normalization and permit cancellation against stale active
or unknown provider truth. The remaining architecture, persistence, language/framework, test-coverage,
docs ownership, and plan/review lifecycle lenses were clean.

## Incremental review — 2026-08-21

> Range reviewed: `9751bd83..6bf01d7b` (1 commit).

No new findings. The native correctness, reuse, efficiency, and error-handling pass; the security pass over
terminal-state normalization, retry eligibility, payer authorization, cancellation ordering and races,
provider identity/status handling, and secret exposure; and the architecture, persistence,
language/framework, changed-behaviour coverage, docs ownership, and plan/review lifecycle lenses were clean.

## Incremental review — 2026-08-21

> Range reviewed: `6bf01d7b..8fe54fc6` (72 commits).

No new findings. The native correctness, reuse, efficiency, and error-handling pass and the security pass
were clean. The two current-main merges imported their upstream commits unchanged and introduced no
conflict-resolution delta. The branch-local integration-test correction exercises the fail-closed retry
contract with earlier persisted failure state and current declined provider truth. The architecture,
service-boundary, persistence, language/framework, changed-behaviour coverage, docs ownership, routed-skill,
and plan/review lifecycle lenses were also clean.
