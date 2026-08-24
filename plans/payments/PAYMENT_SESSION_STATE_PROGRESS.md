# Payment session state progress

- Plan: `plans/payments/PAYMENT_SESSION_STATE_PLAN.md`
- Roadmap: `plans/payments/STRIPE_RELIABILITY_ROADMAP.md`
- Roadmap item: `payments/payment-session-state`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Feature-payments_payment-session-state`
- Branch: `Feature/payments_payment-session-state`
- PR: draft #721 — https://github.com/Concertable/concertable/pull/721
- Dependency/package gates: PR #597 and platform sync #645 supplied the implementation baseline; this producer's publication and generated platform-sync remain pending
- Last reconciled: `2026-08-24` against `origin/main` `08ffa92e7a54a0c47391da914d22cd8a24319b38`, merge head `843c82cd25548010579931376ebdeb7cca8eedc1`, Payment platform `0.1.0-alpha.0.1171`, and review watermark `c685747a421be9919cd189f5991d2634f620abdd`

## Current state

Phases 1 through 3 are implemented. Payment owns the durable session operation/attempt aggregate, canonical
request fingerprint, race-safe reservation and revision history, provider-neutral Stripe execution and
refresh, and additive backend-only gRPC/Client operations for create/replay, payer-only retry, and
participant-scoped status reads. Public status remains secret-free and every legacy RPC remains live.

Current `origin/main` is merged without conflict. Its only Payment delta was the platform pin from `.1161`
to `.1171`; the branch is 0 commits behind. This commit replaces the raw idempotency-key generator with the
internal `PaymentSessionIdempotencyKey` value object, carries it through `IStripeSessionClient`, and converts
it to provider text only inside the real and fake Stripe adapters. No reverse parser is added because no
string ingress exists.

PR #721 is draft while this new executable candidate receives incremental review and exact-head CI. Consumer
work remains delivery-gated until the producer merges, its packages publish, and the causally generated
platform-sync PR is green and merged. The roadmap item remains unchecked until those gates are terminal.

## Next Steps

Run `/incremental-review` from `c685747a421be9919cd189f5991d2634f620abdd` through this commit, including
the required Payment security layer, and resolve every finding. Do not mark PR #721 ready or enqueue it until
that review is clean and exact-head draft CI is green.

## Completed work

- Implemented and re-scaffolded the durable Payment session operation/attempt aggregate, versioned
  fingerprint, optimistic-concurrency repositories, and migration in Phases 1–2.
- Implemented provider-neutral PaymentIntent/SetupIntent create, replay, refresh, cancellation, explicit
  retry, crash-window convergence, and fail-closed provider-truth handling.
- Added the authenticated additive protobuf, Contracts, and typed Client surface with owner scoping,
  exhaustive error mapping, compatibility guards, and focused unit/integration coverage.
- Resolved review findings NAT1 and SEC1–SEC3 in commits `9801e2d0d`, `17f3fcc71`, `9751bd838`, and
  `6bf01d7b4`; subsequent current-main incremental reviews were clean through `c685747a4`.
- Opened PR #721 and repeatedly reconciled it with current `main`; the latest merge is
  `843c82cd25548010579931376ebdeb7cca8eedc1`.
- Replaced the string generator with an opaque idempotency-key value object at the Application/provider seam
  and added direct value-equality and canonical-format coverage in this commit.

## Verification

- `dotnet build tests\Concertable.Payment.UnitTests\Concertable.Payment.UnitTests.csproj --no-restore
  --disable-build-servers`: succeeded with 0 warnings and 0 errors against platform `.1171`.
- Focused `PaymentSessionIdempotencyKeyTests|PaymentSessionProviderExecutionTests`: 4 passed, 0 failed,
  0 skipped.
- `dotnet build tests\Concertable.Payment.IntegrationTests\Concertable.Payment.IntegrationTests.csproj
  --no-restore --disable-build-servers`: succeeded with 0 warnings and 0 errors.
- The previous reviewed PR head `f3a549eb065d1c4432e00265b52e45bb64e67dd2` completed all 70 CI checks
  without failure; that evidence is superseded for executable validation by this candidate.

## Reviews

Review artifact: `reviews/Feature-payments_payment-session-state.md`. Full and incremental native/security
review is clean through `c685747a421be9919cd189f5991d2634f620abdd`; NAT1 and SEC1–SEC3 are resolved.
The range from that watermark through this commit is pending incremental review.

## Decisions, discoveries, blockers, and deviations

- Payment remains an agnostic adapter: the session surface accepts only Payment vocabulary and opaque owner
  identifiers; no Customer or B2B workflow type crosses the boundary.
- Idempotency identity is a typed Application value until the provider adapter formats it; there is no
  speculative `TryParse` because no persisted, wire, or provider string is read back into the application.
- Secrets remain response-only and are neither persisted nor exposed by status reads.
- A changed immutable request requires a new caller-owned operation ID; only an eligible explicit retry
  creates a Payment-owned revision.
- Consumer adoption cannot land before this producer's published package version and generated platform sync.
