# Code review — Feature/payments_payment-session-state

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `e7f2e36a8415752bf3aea04630f568f53b417179`  _(2026-08-21)_
**Security-reviewed up to commit:** `e7f2e36a8415752bf3aea04630f568f53b417179`  _(2026-08-21)_

> Range reviewed: `69df07b8..7e165607` (6 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [ ] **NAT1 — MEDIUM — native/correctness** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Services/PaymentSessionService.cs:139`
  Concurrent duplicate retry requests can both observe a cancellable predecessor; after the first request cancels it, the second treats the provider's already-canceled response as `ProviderUnavailable` and returns before repository reservation can replay the winner's successor. Make predecessor cancellation convergent by re-reading after cancellation failure and accepting a confirmed canceled state before reserving or replaying the successor.
- [ ] **SEC1 — HIGH — security** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Services/PaymentSessionService.cs:116`
  Retry authorization accepts either participant, but a successful retry returns the payer's PaymentIntent client secret, CustomerSession secret, and Stripe customer token. Require the retry owner to equal the persisted payer owner; keep participant-wide authorization only on the secret-free status read, and test that a payee retry returns the indistinguishable unknown-operation failure without calling Stripe.
- [ ] **SEC2 — MEDIUM — security/correctness** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Services/PaymentSessionService.cs:137`
  Retry cancels the current Stripe object before the persisted attempt is evaluated for retry eligibility, so a retry of a nonterminal or authorized attempt can destroy the live payment or hold and only then return `OperationConflict`. Refresh and normalize provider truth, evaluate the explicit-retry policy, and cancel only after it approves a new attempt; test that retrying an authorized or nonterminal attempt does not call cancellation.

## Incremental review — 2026-08-21

> Range reviewed: `7e165607..e7f2e36a` (6 commits).

No new Payment work-order findings. The range contains this review checkpoint plus the merged N3
guidance/meta-only series; native, security, docs ownership, skill-route, architecture, and plan/review
lifecycle lenses were clean. The native pass rediscovered N3's deleted-`api/AGENTS.md` workflow reference,
already owned as `ACC1` by `plans/docs/POLYREPO_READY_PROGRESS.md` on its dedicated follow-up branch, so it
is not duplicated here.
