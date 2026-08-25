# Code review — Feature/payments_payment-session-state

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `96b8a3280e156eb6b521b2b7a5eab9dd3abf80ed`  _(2026-08-25)_
**Security-reviewed up to commit:** `96b8a3280e156eb6b521b2b7a5eab9dd3abf80ed`  _(2026-08-25)_

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

## Incremental review — 2026-08-23

> Range reviewed: `8fe54fc6..0ad5a36e` (250 commits).

No new findings. The native correctness, reuse, efficiency, and error-handling pass and the security pass
were clean. The six branch-unique commits contain plan/review checkpoints and two current-main merges. Merge
`61e13b0c6` retains the feature's new published request and Client-interface cases while adopting main's
shared assembly-reference assertion; no production Payment file conflicted. Merge `0ad5a36ed` has no
conflict-resolution delta, and the remaining upstream commits were imported unchanged. The service-boundary,
module-boundary, persistence, language/framework, changed-behaviour coverage, docs ownership, routed-skill,
and plan/review lifecycle lenses were also clean.

## Incremental review — 2026-08-23 (second current-main reconciliation)

> Range reviewed: `0ad5a36e..6632bd3f` (21 commits).

No new findings. The native and security passes were clean. The four branch-unique commits contain only
review/plan checkpoints and conflict-free merge `6632bd3f6`; its remerge diff is empty. Payment's sole
upstream delta advances `ConcertablePlatformVersion` from `0.1.0-alpha.0.1158` to `.1161`, with no runtime,
authorization, secret-handling, provider-operation, persistence, contract, or test-behaviour change. The
service-boundary, module-boundary, package, language/framework, changed-behaviour coverage, docs ownership,
routed-skill, and plan/review lifecycle lenses were also clean.

## Incremental review — 2026-08-23 (docs-only currency tail)

> Range reviewed: `6632bd3f..c685747a` (9 commits).

No new findings. Native and security passes were clean. The four branch-unique commits are review/plan
checkpoints plus conflict-free merge `c685747a4`; its remerge diff is empty. The five imported commits belong
to docs-only PR #764 and touch no Payment or runtime path. The service-boundary, module-boundary, package,
language/framework, changed-behaviour coverage, docs ownership, routed-skill, and plan/review lifecycle lenses
were also clean.

## Incremental review — 2026-08-25

> Range reviewed: `c685747a..9367612c` (49 commits).

No new findings. The native correctness, reuse, simplification, efficiency, and error-handling pass and the
required Payment security pass were clean. The branch-local change preserves the canonical provider
idempotency key while carrying its identity as an opaque value through the application/provider seam; text
conversion remains inside the real and fake Stripe adapters. Imported main changes and conflict-free merge
`843c82cd2` introduce no branch-specific resolution delta. The service-boundary, module-boundary, package,
language/framework, changed-behaviour coverage, docs ownership, routed-skill, and plan/review lifecycle lenses
were also clean.

## Incremental review — 2026-08-25 (current-main reconciliation)

> Range reviewed: `9367612c..96b8a328` (36 commits).

No new findings. Native and required Payment security passes were clean. The merge-resolution delta
re-scaffolds the combined Payment model, preserving every session operation/attempt table, constraint, index,
rowversion and relationship while adopting main's `DateTimeOffset` audit columns. The protobuf contract tests
retain every session enum, field-number/type and service-method assertion; assembly-reference guards moved to
the architecture tier without reducing Contracts or Client assembly coverage. Imported main commits introduce
no branch-specific regression. The service-boundary, module-boundary, persistence, package,
language/framework, changed-behaviour coverage, docs ownership, routed-skill, and plan/review lifecycle lenses
were also clean.
