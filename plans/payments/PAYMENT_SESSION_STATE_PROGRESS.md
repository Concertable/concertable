# Payment session state progress

- Plan: `plans/payments/PAYMENT_SESSION_STATE_PLAN.md`
- Roadmap: `plans/payments/STRIPE_RELIABILITY_ROADMAP.md`
- Roadmap item: `payments/payment-session-state`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-payments_payment-session-state`
- Branch: `Feature/payments_payment-session-state`
- PR: ready #721 — https://github.com/Concertable/concertable/pull/721
- Dependency/package gates: PR #597 and platform sync #645 supplied the implementation baseline; this producer's publication and generated platform-sync remain pending
- Last reconciled: `2026-08-25` against `origin/main` `24145d1e826731f4510edb1cb1f69c7a1fa3f105`, merge head `305806eba2e1aeba20a46957ca466d2b575b1cb1`, Payment platform `0.1.0-alpha.0.1186`, and clean native/security review through that merge head

## Current state

Phases 1 through 3 are implemented. Payment owns the durable session operation/attempt aggregate, canonical
request fingerprint, race-safe reservation and revision history, provider-neutral Stripe execution and
refresh, and additive backend-only gRPC/Client operations for create/replay, payer-only retry, and
participant-scoped status reads. Public status remains secret-free and every legacy RPC remains live.

The reviewed producer head replaces the raw idempotency-key generator with the
internal `PaymentSessionIdempotencyKey` value object, carries it through `IStripeSessionClient`, and converts
it to provider text only inside the real and fake Stripe adapters. No reverse parser is added because no
string ingress exists.

PR #721 is ready. Current `origin/main` is merged in this commit. The Payment migration was re-scaffolded
from the combined model so the session operation/attempt schema and main's `DateTimeOffset` audit model both
remain present. The protobuf compatibility tests retain the session contract assertions while main's assembly
reference guards remain in the architecture tier. The latest conflict-free main merge leaves the Payment tree
byte-identical and is cleanly reviewed; exact-head CI remains before the merge queue. Consumer
work remains delivery-gated until the producer merges, its packages publish, and the causally generated
platform-sync PR is green and merged. The roadmap item remains unchecked until those gates are terminal.

## Next Steps

Push the reviewed current-main candidate plus its review tail, verify the remote and PR head exactly match,
then require exact-head CI green before entering `/merge`.

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

## Verification

- `api/initial-migrations.ps1`: succeeded; only the combined Payment model required a new scaffold.
- Payment UnitTests build: succeeded with 0 warnings and 0 errors against platform `.1186`.
- Focused session idempotency/provider/protobuf contract tests: 9 passed, 0 failed, 0 skipped.
- Focused Payment contract/package architecture guards: 6 passed, 0 failed, 0 skipped.

## Reviews

Review artifact: `reviews/Feature-payments_payment-session-state.md`. Full and incremental native/security
review is clean through `305806eba2e1aeba20a46957ca466d2b575b1cb1`; NAT1 and SEC1–SEC3 are resolved.

## Decisions, discoveries, blockers, and deviations

- Payment remains an agnostic adapter: the session surface accepts only Payment vocabulary and opaque owner
  identifiers; no Customer or B2B workflow type crosses the boundary.
- Idempotency identity is a typed Application value until the provider adapter formats it; there is no
  speculative `TryParse` because no persisted, wire, or provider string is read back into the application.
- Secrets remain response-only and are neither persisted nor exposed by status reads.
- A changed immutable request requires a new caller-owned operation ID; only an eligible explicit retry
  creates a Payment-owned revision.
- Consumer adoption cannot land before this producer's published package version and generated platform sync.
