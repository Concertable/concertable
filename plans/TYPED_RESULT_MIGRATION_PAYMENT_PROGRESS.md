# Payment owned-result expansion progress

- Plan: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\PaymentOwnedResultExpansion`
- Branch: `Feature/PaymentOwnedResultExpansion`
- PR: not opened; frozen donor PR #296 remains open and DIRTY at `82d0555cd`
- Dependency/package gates: This branch is the exclusive canonical implementation owner for Payment Phase 2. Phase 1 merged in PR #290 and platform-synced in PR #291; Payment currently consumes platform `0.1.0-alpha.0.792`. Removing the published FluentResults client surface is an intentional breaking package cutover: Payment must merge and publish before B2B/Customer can migrate on the generated platform-sync PR.
- Last reconciled: 2026-08-04 from local Git, current `origin/main`, and the complete local verification gate

## Current state

The donor worktree was checkpointed at `69259720d`, `origin/main` was merged as `805d98c1d`, and the
donor implementation was merged as `8e7003de0`. Both `69259720d` and donor PR #296's remote head
`82d0555cd` are ancestors of this branch. The donor worktree's later `7fb1da427` is only its own
plans-reorganization merge; the canonical branch carries the equivalent reorganization in
`757015159`. No donor implementation or dirty file is at risk of loss.

The five obsolete published client interfaces, registrations, `ToLegacy` conversions, parallel
operation errors, and superseded RPC/result helpers are removed while the five owned-result
interfaces remain. Metadata contracts are narrowed to `IReadOnlyDictionary`; adapters copy into
protobuf's mutable map at the wire boundary, and the resolved Payment tech-debt entry is deleted.

The donor behavior is now authoritative: `CreateOrBindAsync` validates the reviewed configuration and
any optional expected amounts supplied at binding time, while `CalculateBoundAsync` is a pure later
calculation from the immutable bound terms and does not accept expected values. The stale merged
transaction-time validation body and test are removed. Operation-specific errors use explicit Dunet
case constructors with abstract-root/per-case `Definition`, leaving callers shaped for the future
native-union cutover. Each public union root now has its own matching source file. The local
implementation gate is green. Re-synced to current `origin/main` (platform `0.1.0-alpha.0.795`) at
merge `4beec1c64`; tree clean, 0 behind, 51 ahead. Re-verified green on the new pin: full Release
solution build 0 errors, Payment unit 198/198. Not pushed; no PR opened.

The existing escrow tests establish the intended idempotency semantics: no escrow, an escrow that is
not held, an already-refunded escrow, and a non-refundable state are successful no-ops. An operation
that executes returns its transfer or refund. The owned contract is therefore
`Result<Option<Transfer>, EscrowReleaseError>` and
`Result<Option<Refund>, EscrowRefundError>` rather than a typed failure or a payload-free success.

## Next Steps

The branch is done, green, current with `origin/main`, and unpushed. Remaining is landing it:

1. **Review DONE** (2026-08-05, see `## Reviews`). Before the PR, apply the two fixes it found: **M2**
   — composite `FromCode` must try `CommissionError.FromCode` before `PaymentError.FromCode` and drop
   `PaymentError.FromCode`'s commission fall-through, plus a server-case→wire→client-case fidelity test;
   **M3** — `StripeFailureClassifier` classifies only 402/`card_error` + specific decline codes as typed
   and lets `resource_missing`/`invalid_request_error` rethrow. Then re-run the gate.
2. **Confirm the `CalculateBoundAsync` financial decision with Tommy (review finding H1)** —
   transaction-time revalidation was removed; expected amounts are validated only at binding
   (`CreateOrBindAsync`) and nothing is persisted on the binding, so a deferred `Capture/PayBoundCommission`
   charges from the caller-supplied gross with no in-Payment check it equals the payer-reviewed
   `FinalSettlementGrossMinor`. Plan-consistent (§8); the compensating control is B2B's §4.1 gross freeze.
   Accept that, or persist the reviewed gross/ceiling on the binding and re-assert at money-movement.
   Freezes into the published wire contract, so it is an explicit call before publish.
3. **On Tommy's explicit go:** push and open ONE canonical PR (Payment Phase 2: owned typed
   Result/Option + FluentResults cutover + commission Phase 1). Full merge-queue E2E — do **not**
   `skip-e2e` (payment/gRPC blast radius).
4. **Merge** (Tommy's call, via the queue).
5. **Own the platform-sync — the breaking part.** Payment publishes; the generated
   `chore/platform-sync-*` PR goes RED because B2B + Customer no longer compile against the removed
   FluentResults client. Migrate B2B + Customer consumers to the owned typed client **in that sync
   PR**, build `api/Concertable.slnx` to 0 errors, push, take it green (this is B2B checkpoints 6–7 +
   Customer).
6. **Close donor PR #296** only after this branch's canonical PR head is on the remote.

Merge/push/publish/platform-sync all remain gated on Tommy's explicit instruction.

## Completed work

- Phase 1: PR #290 merged as `68210e5e`; platform-sync PR #291 delivered the owned Kernel functional package.
- Shared.Api validation writer: PR #312 merged as `40b3341de`; platform-sync PR #324 delivered `0.1.0-alpha.0.772`.

## Verification

- `dotnet build api/Concertable.Payment/Concertable.Payment.slnx --configuration Release --no-restore`: 0 warnings, 0 errors.
- `dotnet test api/Concertable.Payment/tests/Concertable.Payment.UnitTests/Concertable.Payment.UnitTests.csproj --configuration Release --no-restore`: 198 passed, 0 failed, 0 skipped.
- `dotnet test api/Concertable.Payment/tests/Concertable.Payment.IntegrationTests/Concertable.Payment.IntegrationTests.csproj --configuration Release --no-restore --no-build`: 7 passed, 0 failed, 0 skipped.
- `dotnet build api/Concertable.slnx --configuration Release --no-restore`: 0 errors and 7 unrelated
  pre-existing/generated E2E warnings outside Payment.
- Payment standalone carve from committed `9cd162ce1`: 0 errors; package-only restore and all nine
  deployable-closure projects built successfully, with no file/type warning for the operation unions.
- Payment grep gate: no `FluentResults`, `ToLegacy`, obsolete published clients, parallel operation
  errors, generated union factory aliases, or stale gRPC result helpers outside `bin`/`obj`;
  `git diff --check` passed.

## Reviews

### 2026-08-05 — first adversarial review of the Phase 2 diff (`origin/main...HEAD`)

Verdict: careful, mostly-correct migration — cancellation preserved, no provider-message leak, unknown
wire codes fail loudly, rounding/VAT/cumulative-refund math sound. No defect mischarges a payer in the
normal flow today. Findings:

- **H1 (financial — DECISION NEEDED):** the cutover removed Payment's in-service cross-check that a
  deferred charge's gross was payer-reviewed. `CalculateBoundAsync` is now pure `rate × caller gross`;
  the binding persists no gross/reviewed amount, and a deferred bind happens with `gross=null`, so
  nothing is validated at bind. `CaptureBoundCommissionAsync(gross=G)` then charges from `G` with no
  check that `G` equals the payer-reviewed `FinalSettlementGrossMinor`. Plan-consistent (§8); the primary
  control now lives in B2B's §4.1 gross freeze (out of this diff). Not "charges wrong today" — it is the
  removal of defense-in-depth. **This is the `CalculateBoundAsync` sign-off:** accept it (relying on the
  B2B §4.1 freeze as the compensating control), or persist the reviewed gross/ceiling on the binding and
  re-assert it at money-movement.
- **M2 (correctness — fix before PR):** `PaymentError.FromCode` greedily claims all `payment.commission_*`
  codes into `CommissionFailure` via its `_ =>` fall-through, so composite unions' own `CommissionFailure`
  arm is dead — a server `ManagerPaymentError.CommissionFailure` round-trips to a `PaymentFailure`-shaped
  case on the client. Code/Kind survive, but `is …CommissionFailure` matching silently never fires, and
  it is untested. Fix: composite `FromCode` tries `CommissionError.FromCode` before `PaymentError.FromCode`;
  drop the commission fall-through; add a server-case→wire→client-case fidelity test.
- **M3 (fault-swallowing — fix before PR):** `StripeFailureClassifier` maps 400/404/409/422 all to a
  benign `PaymentRejected` decline. A 404 `resource_missing` / 400 `invalid_request_error` is an infra/
  logic fault that should throw (retry/dead-letter), not report a decline — violates the convention
  (infra/unknown Stripe faults stay exceptions). Fix: classify only 402/`card_error` + specific decline
  codes as typed; let `resource_missing`/`invalid_request_error` rethrow.
- **Lows:** L4 settlement refunds publish `escrow.refund_*` codes (wrong namespace); L5 "already refunded"
  returns `Some` not the no-op `None` (pre-existing); L6 `EnsureTransition` throws after the Stripe side
  succeeded (latent, guarded).

Clean: escrow `Result<Option<T>,E>` semantics, rounding/VAT/refund math, wire hygiene/cancellation, owned-error conventions.

## Decisions, discoveries, blockers, and deviations

- Release/refund absence is a benign idempotent no-op represented by `Option.None`; successful execution returns `Option.Some`.
- Owned Result/Option stay in-process. Protobuf retains an owned wire contract with explicit mapping at the gRPC boundary.
- Payload-free operation errors are sealed definition records with named static values. Dunet is
  reserved for alternatives carrying distinct data; those unions declare `Definition` abstract on
  the root and override it on each case instead of using positional `Match` lambdas.
- A not-found definition without an explicit message uses `ErrorDefinition.NotFound<T>(code)` and
  the type's required `[DisplayName]`; otherwise the message remains explicit.
- Tommy rejected the additive compatibility design on 2026-08-04. Phase 2 removes FluentResults from
  Payment completely and uses the repository's breaking publish-then-sync cutover; no in-source
  compatibility adapter or duplicate legacy method is permitted.
- Infrastructure, cancellation, authentication, rate-limit/server, and unknown Stripe faults remain exceptions; only caller-actionable decline/refusal becomes a typed error.
- API E2E belongs to the merge queue after the PR is ready; no local E2E runs ahead of it.
- `Feature/PaymentOwnedResultExpansion` is the exclusive Phase 2 implementation owner. The overlapping
  commission-branch implementation is donor evidence only; its behavior is now reconciled here.

## Event log

### 2026-08-05 — First adversarial Phase 2 review completed

- Action: Ran the first-ever adversarial code review of the whole Phase 2 diff (`origin/main...HEAD`).
- Evidence: findings recorded in `## Reviews` — H1 (deferred-charge defense-in-depth removed; the
  `CalculateBoundAsync` decision), M2 (dead `CommissionFailure` wire round-trip), M3 (Stripe
  400/404/409/422 mis-classified as declines), plus lows L4–L6. Money math, escrow Option semantics,
  wire hygiene, and conventions confirmed clean.
- Outcome: two fixes (M2, M3) required before the PR; H1 is an explicit financial sign-off for Tommy.
- Follow-up: apply M2 + M3, get the H1 decision, then push/PR per `## Next Steps`.

### 2026-08-05 — Re-synced to current main and re-verified green ahead of push

- Action: In the PaymentOwned worktree (delegated by Tommy this session), fetched and merged current
  `origin/main` (2 incoming commits = the `0.1.0-alpha.0.795` platform-sync bump), then rebuilt and
  re-tested against the new pin.
- Evidence: clean merge `4beec1c64` (only `Directory.Packages.props` bumps, no conflicts); 0 behind /
  51 ahead; `dotnet build api/Concertable.slnx --configuration Release` = 0 errors (5 pre-existing
  warnings); Payment unit 198/198.
- Outcome: branch is current, green, clean, and unpushed — ready for review then push/PR on Tommy's go.
- Follow-up: first code review, confirm the `CalculateBoundAsync` decision, then push/open the PR.

### 2026-08-04 — Synced current main and passed the standalone package gate

- Action: Committed the reconciled owner as `60fbf6b93`, merged nine current `origin/main` commits as
  `a31457d48`, repeated the local verification gate, and built the committed Payment archive as a
  standalone package-only closure. Split the public union roots into matching source files after the
  carve exposed the file/type analyzer convention.
- Evidence: Branch is 0 behind `origin/main`; Payment build is 0 warnings/0 errors; unit tests pass
  198/198; integration tests pass 7/7; full Release build has 0 errors; standalone carve has 0 errors;
  the final `9cd162ce1` carve has no operation-union file/type warnings; dependency greps and diff
  hygiene are clean.
- Outcome: The Payment owner is current, verified, package-isolated, and ready for its canonical PR.
- Follow-up: Wait for explicit instruction to push and open the canonical PR.

### 2026-08-04 — Reconciled donor behavior and restored a green Payment owner

- Action: Replaced the bad-merge implementation bodies with the verified donor behavior, adapted
  them to the canonical owned error cases, removed obsolete clients/errors/helpers, and retained the
  newer Stripe failure-path tests.
- Evidence: Payment Release build completed with 0 warnings and 0 errors; unit tests passed 198/198;
  Docker-gated integration tests passed 7/7; the full Release solution completed with 0 errors and 6
  unrelated generated E2E warnings; all Payment dependency greps and `git diff --check` passed.
- Outcome: `CalculateBoundAsync` is again a pure calculation from immutable bound terms, operation
  signatures expose honest composite errors, and the temporary Dunet cases are shaped for a direct
  native-union replacement.
- Follow-up: Commit this checkpoint, sync current `origin/main`, and repeat the final gates including
  the committed Payment carve before push/PR delivery.

### 2026-08-04 — Consolidated the frozen donor and reached the published-interface cutover gate

- Action: Checkpointed the donor's four dirty files as `69259720d`, merged current `origin/main` into
  PaymentOwned as `805d98c1d`, then merged the complete donor history with PaymentOwned winning
  overlapping hunks after Tommy's explicit approval.
- Evidence: `git merge-base --is-ancestor Feature/CommissionBindingDeferredPricing HEAD` returned
  success; the canonical branch is 0 behind and 45 ahead of `origin/main`. The topology scan found
  `Concertable.Payment.Client` package consumers in B2B and Customer and no consumers of the new
  interface names yet.
- Outcome: Donor history is preserved, but the Payment build remains red while the old published
  interfaces coexist with the new owned-result contracts. An attempted deletion of the new
  `ICommissionPricingClient` was stopped before any file deletion and explicitly rejected as wrong.
- Follow-up: Obtain explicit approval for the planned breaking removal of only the five old published
  interfaces, then complete the owner build and publish-before-sync sequence in `## Next Steps`.

### 2026-08-04 — Reconstructed Payment Phase 2 baseline

- Action: Refreshed the dedicated Payment worktree to current `origin/main`, read the plan and current conventions, inventoried Payment Result/client surfaces, and resolved escrow optionality from established tests.
- Evidence: clean `Feature/PaymentOwnedResultExpansion` at `5d06d3121`; platform pin `0.1.0-alpha.0.772`; existing release/refund tests explicitly assert successful null/no-op outcomes.
- Outcome: Phase 2 is unblocked with `Result<Option<T>, TError>` selected for release/refund, and this ledger now owns the Payment worktree's operational state.
- Follow-up: Execute the single implementation and verification action in `## Next Steps`.

### 2026-08-04 — Added the owned-error and structured gRPC foundation

- Action: Added Payment operation error unions, their stable definitions, the protobuf error detail, and the server-side gRPC mapping while preserving legacy status detail.
- Evidence: Payment unit tests passed 161/161, including every leaf definition, composite forwarding, and structured gRPC detail; the Payment Release solution build completed with zero warnings and zero errors.
- Outcome: The additive transport/error foundation is green and ready for the application, infrastructure, and client paths to consume.
- Follow-up: Continue the single implementation and verification action in `## Next Steps`.

### 2026-08-04 — Canonical ownership assigned after duplicate-work discovery

- Action: Audited every Typed Result ledger, worktree, branch, PR, unique commit, and dirty path after
  discovering Payment Phase 2 implementation on both this branch and PR #296.
- Evidence: PR #296 remains open at `82d0555cd`; its `f693c955d` changes 71 files for Payment typed
  results. This branch has two unique Phase 2 commits plus overlapping uncommitted Payment paths and no PR.
- Outcome: `Feature/PaymentOwnedResultExpansion` is the exclusive canonical Phase 2 owner. The
  commission branch is frozen donor state; no implementation is to continue there.
- Follow-up: Reconcile and salvage the donor implementation before writing further Phase 2 code.

### 2026-08-04 — Removed the rejected FluentResults compatibility path

- Action: Replaced Phase 2's additive compatibility requirement with an intentional breaking package
  cutover, removed every legacy FluentResults client method and adapter including `ToLegacy` and
  `ToLegacyNullable`, removed the Payment client package reference and service-level version pin, and
  deleted the adapter-parity tests.
- Evidence: `rg -n --glob '!**/bin/**' --glob '!**/obj/**' "ToLegacy|FluentResults" api/Concertable.Payment`
  returned no matches; `git diff --check` passed.
- Outcome: Payment exposes only owned typed Result/Option operations. B2B/Customer migration is
  intentionally deferred until the new Payment package exists on the feed.
- Follow-up: Run the owning-package verification gate and commit the green checkpoint locally.

### 2026-08-04 — Simplified operation error representation

- Action: Replaced payload-free Payment error unions with sealed definition records and moved
  `Definition` onto each case of the remaining data-bearing Dunet unions; updated every Payment call
  site and the repository conventions.
- Evidence: Dunet 1.16.2 compiled the abstract-root/per-case override shape; Payment unit tests passed
  169/169 and the Payment Release solution build completed with zero warnings and zero errors.
- Outcome: Error declarations no longer use unused positional `Match` parameters, and Dunet is used
  only where alternatives carry distinct data.
- Follow-up: Complete donor reconciliation and the remaining Phase 2 verification gates.

### 2026-08-04 — Verified the FluentResults-free Payment owner cutover

- Action: Restored and built the Payment standalone closure, ran Payment unit and integration tests,
  ran the full Release solution build, and repeated the Payment dependency grep gate.
- Evidence: Payment build completed with 0 warnings and 0 errors; unit tests passed 169/169;
  integration tests passed 6/6; the full solution completed with 0 errors and 5 unrelated E2E
  warnings; Payment contains no `FluentResults` or `ToLegacy` match outside build outputs.
- Outcome: The owning-package side of the breaking cutover is green and ready for a local checkpoint.
- Follow-up: Commit this checkpoint, update from `origin/main`, and repeat the build gates before any push.

### 2026-08-04 — Completed interface removal and exposed the commission validation overlap

- Action: Deleted the five obsolete published Payment client interfaces, retained the five
  owned-result interfaces, narrowed metadata inputs to `IReadOnlyDictionary`, removed their resolved
  tech-debt entry, and ran the Payment Release build.
- Evidence: GitHub reports donor PR #296 open/DIRTY at `82d0555cd`, which is an ancestor of this
  branch; Payment contains no `FluentResults` or `ToLegacy` match outside build outputs. The build
  failed with seven compile errors: protobuf mutable-map inputs, stale `CommissionQuote` names, and a
  `CommissionService` comparison against expected amounts absent from its merged signature.
- Outcome: Safe map-boundary, type-name, and Stripe ambiguity corrections are applied. The next build
  exposed 39 overlap errors. Their mechanical reconciliation can continue after the financial
  behavior choice is fixed because the donor protobuf deliberately reserves the expected-amount
  fields while stale service/test code still requires transaction-time validation.
- Follow-up: Obtain the explicit pricing-contract decision recorded in `## Next Steps`, implement it,
  and rerun every owner-side verification gate before checkpointing.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\PaymentOwnedResultExpansion
Read @plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md and @plans/TYPED_RESULT_MIGRATION_PAYMENT_PROGRESS.md and do what its `## Next Steps` says.
```
