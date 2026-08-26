# Payment session state progress

- Plan: `plans/payments/PAYMENT_SESSION_STATE_PLAN.md`
- Roadmap: `plans/payments/STRIPE_RELIABILITY_ROADMAP.md`
- Roadmap item: `payments/payment-session-state`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-payments_payment-session-state`
- Branch: `Feature/payments_payment-session-state`
- PR: ready #721 — https://github.com/Concertable/concertable/pull/721
- Dependency/package gates: PR #597 and platform sync #645 supplied the implementation baseline; this producer's publication and generated platform-sync remain pending
- Last reconciled: `2026-08-26` against `origin/main` `e1f4ff562fabc4cbc420cbb8952b9ab5e8c0b2b8`, branch integration head `944eba8dcb9d355f8c36329450d88b9012abff09`, executable head `e133a066ff27f6a9afd8d902493c48a539ef27c8`, Payment platform `0.1.0-alpha.0.1189`, and clean incremental review through the branch integration head

## Current state

Phases 1 through 3 are implemented. Payment owns the durable session operation/attempt aggregate, canonical
request fingerprint, race-safe reservation and revision history, provider-neutral Stripe execution and
refresh, and additive backend-only gRPC/Client operations for create/replay, payer-only retry, and
participant-scoped status reads. Public status remains secret-free and every legacy RPC remains live.

The reviewed producer head replaces the raw idempotency-key generator with the
internal `PaymentSessionIdempotencyKey` value object, carries it through `IStripeSessionClient`, and converts
it to provider text only inside the real and fake Stripe adapters. No reverse parser is added because no
string ingress exists.

PR #721 is current with `origin/main` through the conflict-free docs-only merge `944eba8dc`.
The Payment migration was re-scaffolded
from the combined model so the session operation/attempt schema and main's `DateTimeOffset` audit model both
remain present. The protobuf compatibility tests retain the session contract assertions while main's assembly
reference guards remain in the architecture tier. The latest conflict-free main merge leaves the Payment tree
byte-identical and is cleanly reviewed; exact-head CI remains before the merge queue. Consumer
work remains delivery-gated until the producer merges, its packages publish, and the causally generated
platform-sync PR is green and merged. The roadmap item remains unchecked until those gates are terminal.

## Next Steps

Commit and push the current-main review/ledger checkpoint, verify the remote and PR head exactly match, then
require exact-head CI green before re-entering `/merge`. The first merge group was built on top of adjacent
queue entry #786's synthetic head and inherited #786's independently reproduced B2B UI E2E failure; do not
change Payment for that unrelated frontend failure.

## Completed work

- Implemented and re-scaffolded the durable Payment session operation/attempt aggregate, versioned
  fingerprint, optimistic-concurrency repositories, and migration in Phases 1–2.
- Implemented provider-neutral PaymentIntent/SetupIntent create, replay, refresh, cancellation, explicit
  retry, crash-window convergence, and fail-closed provider-truth handling.
- Added the authenticated additive protobuf, Contracts, and typed Client surface with owner scoping,
  exhaustive error mapping, compatibility guards, and focused unit/integration coverage.
- Resolved review findings NAT1 and SEC1–SEC3 in commits `9801e2d0d`, `17f3fcc71`, `9751bd838`, and
  `6bf01d7b4`; subsequent current-main incremental reviews were clean through `c685747a4`.
- Opened PR #721 and reconciled it with current `main`, most recently through this commit.
- Replaced the string generator with an opaque idempotency-key value object at the Application/provider seam
  and added direct value-equality and canonical-format coverage in this commit.
- Applied the interactive review: owned construction now lives on the request/fingerprint types; provider
  shapes have precise files; on/off-session and selected payment method are caller-supplied, persisted and
  fingerprinted; known Stripe failures use typed Results; embedded confirmation failures converge through
  provider binding; and current presence no longer changes future-reuse policy.

## Verification

- `api/initial-migrations.ps1`: succeeded; only the combined Payment model required a new scaffold.
- Payment UnitTests: 525 passed, 0 failed, 0 skipped against platform `.1189`.
- Focused Payment session IntegrationTests: 23 passed, 0 failed, 0 skipped.
- Payment ArchitectureTests: 9 passed, 0 failed, 0 skipped.
- Stripe SDK-level confirmation-decline and future-usage regression tests: 2 passed, 0 failed, 0 skipped.

## Reviews

Review artifact: `reviews/Feature-payments_payment-session-state.md`. Full and incremental native/security
review is clean through `e133a066ff27f6a9afd8d902493c48a539ef27c8`; NAT1–NAT3 and SEC1–SEC3 are resolved.

## Decisions, discoveries, blockers, and deviations

- Payment remains an agnostic adapter: the session surface accepts only Payment vocabulary and opaque owner
  identifiers; no Customer or B2B workflow type crosses the boundary.
- Idempotency identity is a typed Application value until the provider adapter formats it; there is no
  speculative `TryParse` because no persisted, wire, or provider string is read back into the application.
- Secrets remain response-only and are neither persisted nor exposed by status reads.
- A changed immutable request requires a new caller-owned operation ID; only an eligible explicit retry
  creates a Payment-owned revision.
- Consumer adoption cannot land before this producer's published package version and generated platform sync.
- Merge-group run `32960960691` failed only in B2B UI E2E after GitHub placed #721 on #786's already-red
  synthetic queue head `6687f83c4`. #786 independently failed runs `32957620097` and `32960630720`;
  #721's build, carve, architecture, unit, integration, and Payment gates were green. Re-enqueue only from a
  current exact head after #786 is no longer ahead of it in the queue.
