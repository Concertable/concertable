# Payment owned-result expansion progress

- Plan: `plans/TYPED_RESULT_MIGRATION_PAYMENT_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-payment-owned-closeout`
- Branch: `Docs/typed-result_payment-owned-result-expansion_closeout`
- PR: #392 (`https://github.com/Concertable/concertable/pull/392`); frozen donor PR #296 remains open at `82d0555cd`
- Dependency/package gates: This branch is the exclusive canonical implementation owner for Payment Phase 2. Phase 1 merged in PR #290 and platform-synced in PR #291; Payment currently consumes platform `0.1.0-alpha.0.847`. Removing the published FluentResults client surface is an intentional breaking package cutover: Payment must merge and publish before B2B/Customer can migrate on the generated platform-sync PR.
- Downstream handoffs: B2B checkpoints 6-7 are waiting in `plans/typed-result/B2B_PROGRESS.md`
  (`Refactor/B2BTypedResultMigration`) for this branch to merge, publish, and platform-sync green.
- Last reconciled: 2026-08-08 from local Git, `origin/main`, GitHub PR state, and workflow runs

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

The donor behavior remains the baseline, but H1's contract is now decided: Payment must persist the
payer-reviewed gross as `Money` on the binding and reject money movement whose gross is unconfirmed or
different. Caller-supplied expected commission and payer-total values are rejected as redundant;
Payment calculates those amounts itself. Operation-specific errors use explicit Dunet
case constructors with abstract-root/per-case `Definition`, leaving callers shaped for the future
native-union cutover. Each public union root now has its own matching source file. The local
implementation gate is green. Re-synced to current `origin/main` (platform `0.1.0-alpha.0.795`) at
merge `4beec1c64`; tree clean, 0 behind, 51 ahead. Re-verified green on the new pin: full Release
solution build 0 errors, Payment unit 198/198. On 2026-08-05 the branch was refreshed again and merged
32 newer `origin/main` commits as `e787dd0122`, advancing Payment to platform
`0.1.0-alpha.0.798`; it is now clean, 0 behind, and 56 ahead. The M2/M3 fixes and verification gate
are complete on this new base: composite parsers preserve their commission case across gRPC, while
Stripe only returns typed rejection for HTTP 402, `card_error`, or an actual decline code and
propagates invalid-request/resource faults. The complete owner gate is green. Not pushed; no PR
opened. A fresh fetch on 2026-08-05 found that `origin/main` had advanced 65 commits to `0ed29d8f0`
and platform `0.1.0-alpha.0.814`. That mainline is now merged into this branch; the combined branch
has not been re-verified and must repeat the owner gate after H1 is decided.

The existing escrow tests establish the intended idempotency semantics: no escrow, an escrow that is
not held, an already-refunded escrow, and a non-refundable state are successful no-ops. An operation
that executes returns its transfer or refund. The owned contract is therefore
`Result<Option<Transfer>, EscrowReleaseError>` and
`Result<Option<Refund>, EscrowRefundError>` rather than a typed failure or a payload-free success.

H1 is implemented in this checkpoint. Commission bindings persist nullable `ReviewedGrossMinor`,
but every caller-facing monetary boundary added or changed in this slice uses `Money`. Binding and
review are now separate operations: `CreateOrBindAsync` fixes identity and pricing terms, then
`ConfirmReviewedGrossAsync` atomically confirms one reviewed gross and returns Payment's calculation.
Bound calculation, manager pay/hold/refund, and escrow deposit/capture/refund all reject an
unconfirmed or different gross before Stripe. Payment calculates commission and payer total itself;
the expected-value inputs and error are removed from the client, protobuf, application, and service
contracts. Remaining primitive monetary DTOs are recorded in Payment's `TECH_DEBT.md`.

The regenerated Payment initial migration contains the nullable `ReviewedGrossMinor` column and no
unrelated migration changes remain. The required all-context `initial-migrations.ps1` was attempted
three times; it regenerated Payment correctly but twice exceeded command caps and the final pass
hung in later unchanged Customer contexts. Interrupted Customer migration artifacts were restored or
removed exactly. Payment's owner gates are green. A diagnostic full integration run passed B2B
Artist 17/17, then B2B Concert failed all 144 cases at fixture startup because Windows could not load
`Microsoft.Data.SqlClient.SNI.dll` from the deep worktree path (`ERROR_FILENAME_EXCED_RANGE`); the
wrapper was stopped and its two Testcontainers resources removed. Payment integration independently
passed 7/7 against Docker and the regenerated migration.

Current `origin/main` at `3e3bcce89` is merged as `a4ae0081e`; the canonical branch is 0 behind.
The canonical full review is complete over `3e3bcce89..a4ae0081e` in
`reviews/Feature-PaymentOwnedResultExpansion.md`. BUG1, BUG2, BUG3, CI1, TEST1, and CV1 are fixed in
review-fix commit `d0fe18afe`. Its incremental review found no new issues. Committed-tree verification
is green: full Release solution build 0 errors, Payment unit 219/219, Shared API unit 52/52, Payment
SQL integration 8/8, and the nine-project standalone package carve 0 errors. Canonical PR #392 is open
at verified head `1c9380726`; donor PR #296 remains open and unchanged at `82d0555cd`. `origin/main`
advanced five commits after the verification gate, so the merge workflow must update the branch and
rebuild before enqueueing.

The merge workflow has now brought `origin/main` at `59bdd7a8a` into the worktree and resolved the
Payment client conflicts in favour of the branch-owned Result API. Payment has also been migrated to
the current error convention before delivery: every operation error is a named Dunet union with one
exhaustive root `Definition` switch; the old static catalogs, per-case overrides, and every `FromCode`
parser are gone. Client reverse mapping uses closed `FrozenDictionary` indexes per operation and
throws `PaymentContractMismatchException` for an unknown or changed wire contract. Stable published
codes are preserved with explicit `[ErrorCode]` attributes where natural case naming differs.

The refreshed source gate is green: Payment Release build 0 warnings/0 errors, Payment unit 222/222,
the full Release API solution build 0 errors (seven existing warnings outside Payment), and Payment
SQL integration 8/8 after the repository Docker data-round-trip preflight passed. The merge and
error-convention changes are committed locally and the incremental review through `842b9c332` found
no issues. Current `origin/main` at `b46d10ec8` is merged locally as `17c8bb2b3`; the post-merge
verification is green: full Release solution build 0 errors, Payment unit 222/222, and Payment SQL
integration 8/8 on a healthy Docker data path. The incremental review through `df76074c4` found no
issues. The verified work head `d6ab44540` is pushed: local, remote-tracking, and PR #392 heads match.
The checkpoint-transport head `bbe9a3522` is also verified across local, remote-tracking, and PR
heads. PR checks are running and the exact head carries the required `full-e2e` override; it has not
been enqueued. PR run `31206320776` failed before queue admission: the primary failure is Shared.Api
unit test `DunetUnionDefinitions_UseSupportedDefinitionShape`; the other reported unit failures are
fail-fast cancellations. The guard does not recognize the new exhaustive root `this switch` error
definition shape used by all ten Payment unions. The guard now recognizes that documented shape;
Shared.Api unit tests pass 52/52 and the full Release solution builds with 0 errors.
Incremental review through `2e120cb40` confirms CI2 fixed with no additional findings.
Replacement work head `40695a4b3` is verified across local, remote-tracking, and PR #392 heads;
checkpoint-transport head `a40761eba` is verified across all three heads. Replacement run
`31208576213` is terminal green across build, carves, unit, and integration checks; `full-e2e`
remains applied and queue-only E2E correctly skipped at PR level.
An initial `gh pr merge --auto` left the green/CLEAN PR with no `mergeQueueEntry`; its auto-merge
request still dates from 2026-08-05, confirming the documented GitHub re-evaluation glitch.
The one-time disable/re-enable nudge admitted exact remote head `a40761eba`; its queue entry is
`QUEUED` and `full-e2e` remains applied.
Merge-group run `31209734022` passed full API and UI E2E; PR #392 merged as `b66325acd`.
The four plan-only commits after source PR head `a40761eba` are transferred onto the clean docs
closeout branch at current merged `origin/main`; normalized plan and ledger content matched the
source worktree before identity changed.
The merged Payment feature worktree is removed, its local branch is deleted, and the remote branch
was already absent. The closeout recovery anchor now lives at the worktree recorded above.
Package publication run `31212157110` succeeded and published platform
`0.1.0-alpha.0.853`. Generated platform-sync PR #420 is open and red at build with 24 expected
consumer compile errors: B2B and Customer still reference the five intentionally removed legacy
Payment client interfaces.
Repair commit `d67416546` replaces all B2B/Customer legacy Payment clients with the published
owned-result interfaces and is the exact local, remote-tracking, and PR #420 head. The full Release
solution builds with 0 errors; legacy-client grep is empty; Concert 79/79, Tenant 96/96, and Ticket
18/18 unit tests pass. Fresh-container Docker health passed before each integration layer; B2B passed
5/5 projects and Customer passed 4/4 projects. Replacement CI is pending and the existing
`platform-sync-broken` label still reflects the superseded red head.

Replacement PR CI run `31220241305` passed. Merge-group run `31220911252` then passed the complete
build, unit, integration, API E2E, and UI E2E gate. Two later merge groups ran concurrently after
docs-only admin merges advanced `main`; both hit the same shared Stripe hold collision. GitHub's
subsequent isolated #420 group `31223601869` passed build, unit, integration, API E2E, and B2B UI but
its Customer UI run timed out waiting for Stripe's card iframe in `Customer completes 3DS challenge`.
GitHub nevertheless merged PR #420 as `372be1041` on 2026-08-07 at 23:02 UTC. The consumer migration
is present on `main`, the queue entry is gone, and no code remains stranded on the sync branch.

Post-merge package publication run `31225852815` passed and published platform
`0.1.0-alpha.0.857`, including the migrated `Concertable.Payment.Client`. Platform-sync run
`31225952562` also passed and correctly created no recursive follow-on sync PR. The Payment package
and consumer cutover gate is terminal; the B2B and Auth dependency ledgers can now be woken.

## Next Steps

Update the waiting B2B and Auth ledgers with the merged #420 / published `0.1.0-alpha.0.857` gate and
their exact resume actions. Close frozen donor PR #296, remove the merged sync worktree, then complete
the Payment terminal checkpoint and delete this plan/ledger together through the docs closeout flow.

## Downstream handoffs

- **B2B typed-result migration:** `plans/typed-result/B2B_PROGRESS.md` in
  `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2BTypedResultMigration` is waiting
  for this canonical Payment branch to merge, publish `Concertable.Payment.Client`, and complete its
  generated platform-sync PR green. When that gate opens, the Payment delivery session must update the
  B2B ledger's current state, `## Next Steps`, and event log, then surface its exact resume prompt.
  The B2B worktree must not poll this dependency or rely on Tommy remembering to revisit it.
- **Auth expected-outcome delivery:** `plans/typed-result/AUTH_OUTCOMES_PROGRESS.md` in
  `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_auth-outcomes` is
  locally verified and review-clean but blocked from pushing and opening its PR while platform-sync
  PR #420 is red. When #420 merges green, update the Auth ledger's current state, `## Next Steps`, and
  event log, then surface its exact resume prompt. The Auth worktree must not poll this dependency.

## Completed work

- Phase 1: PR #290 merged as `68210e5e`; platform-sync PR #291 delivered the owned Kernel functional package.
- Shared.Api validation writer: PR #312 merged as `40b3341de`; platform-sync PR #324 delivered `0.1.0-alpha.0.772`.

## Verification

- `dotnet build api/Concertable.Payment/Concertable.Payment.slnx --configuration Release --no-restore`: 0 warnings, 0 errors.
- `dotnet test api/Concertable.Payment/tests/Concertable.Payment.UnitTests/Concertable.Payment.UnitTests.csproj --configuration Release --no-restore`: 201 passed, 0 failed, 0 skipped.
- `dotnet test api/Concertable.Payment/tests/Concertable.Payment.IntegrationTests/Concertable.Payment.IntegrationTests.csproj --configuration Release --no-restore --no-build`: 7 passed, 0 failed, 0 skipped.
- `dotnet build api/Concertable.slnx --configuration Release --no-restore`: 0 errors and 9 unrelated
  pre-existing/generated E2E warnings outside Payment.
- Payment standalone carve with M2/M3 on platform `0.1.0-alpha.0.798`: package-only restore
  and all nine deployable-closure projects built with 0 errors; existing analyzer warnings remain.
- Payment standalone carve from committed `9cd162ce1`: 0 errors; package-only restore and all nine
  deployable-closure projects built successfully, with no file/type warning for the operation unions.
- Payment grep gate: no `FluentResults`, `ToLegacy`, obsolete published clients, parallel operation
  errors, generated union factory aliases, or stale gRPC result helpers outside `bin`/`obj`;
  `git diff --check` passed.
- H1 working-tree gate on platform `0.1.0-alpha.0.814`: Payment build 0 warnings/0 errors; Payment unit
  209/209; Payment integration 7/7; `git diff --check` clean; no expected commission/payer-total
  parameters remain. B2B Artist integration passed 17/17 before the diagnostic full suite hit the
  unrelated B2B Concert long-path SQL native-DLL startup failure described in `## Current state`.
- Committed review-fix gate at `d0fe18afe`: full Release solution build 0 errors (6 generated E2E
  nullable warnings); Payment unit 219/219; Shared API unit 52/52; Payment integration 8/8; standalone
  package-only carve built all nine deployable-closure projects with 0 errors (existing analyzer
  warnings remain).
- Current-main/error-convention gate on platform `0.1.0-alpha.0.847`: Payment Release build 0
  warnings/0 errors; Payment unit 222/222; full Release API solution build 0 errors with seven
  existing warnings outside Payment; `git diff --check` clean; no `FromCode`,
  static error catalog, or per-case `Definition` override remains in Payment; the repository Docker
  data-round-trip preflight passed and Payment SQL integration passed 8/8.
- Final current-main gate after `b46d10ec8`: full Release API solution build 0 errors with seven
  existing warnings outside Payment; Payment unit 222/222; repository Docker data-round-trip
  preflight healthy; Payment SQL integration 8/8.
- Architecture-guard CI fix: Shared.Api unit tests 52/52; full Release API solution build 0 errors
  with seven existing warnings outside the changed test.

## Reviews

### 2026-08-07 - PR-check fix incremental review (`df76074c4..2e120cb40`)

Artifact: `reviews/Feature-PaymentOwnedResultExpansion.md`. CI2 is fixed in `2e120cb40`; no additional
finding met the confidence threshold across all six repository lenses. The watermark is
`2e120cb40`.

- **CI2 (fixed in this commit):** the typed-result architecture guard recognized only legacy
  generated-`Match` and abstract/per-case definitions, so it rejected the documented exhaustive root
  `this switch` shape used by all ten Payment unions. The guard now recognizes that exact shape and
  its 52-test project is green.

### 2026-08-07 - final current-main incremental review (`842b9c332..df76074c4`)

Artifact: `reviews/Feature-PaymentOwnedResultExpansion.md`. The six-commit range contains plan/review
checkpoints plus the already-landed plan-handoff session-scoping fix and its focused tests. No new
finding met the confidence threshold across all six repository lenses. The watermark is
`df76074c4`.

### 2026-08-07 - incremental delivery review (`ee8dbdd57..842b9c332`)

Artifact: `reviews/Feature-PaymentOwnedResultExpansion.md`. The 96-commit range includes the
current-main merges and the branch-owned Payment error-convention/conflict-resolution work. No new
finding met the confidence threshold across correctness, microservice isolation, module boundaries,
seeding, C# conventions, or changed-path test coverage. The watermark is `842b9c332`.

### 2026-08-05 - canonical full review after H1 (`3e3bcce89..a4ae0081e`)

Artifact: `reviews/Feature-PaymentOwnedResultExpansion.md`. The missing branch-owned incremental
watermark required a full review to establish the canonical artifact. Six findings met the confidence
threshold and are fixed in the review-fix commit:

- **BUG1 (fixed):** bound deposit, capture, and settlement retries now revalidate identity, intent,
  and reviewed gross before returning an existing operation; regressions cover all three.
- **BUG2 (fixed):** an already-refunded booking returns the no-op `Option.None`.
- **BUG3 (fixed):** internal settlement refunds own `SettlementRefundError` and
  `settlement.refund_*` definitions instead of escrow codes.
- **CI1 (fixed):** the typed-result architecture guard matches the current explicit-case convention
  and no longer allowlists the completed client adapter.
- **TEST1 (fixed):** real SQL coverage proves a different second reviewed gross cannot overwrite the
  first.
- **CV1 (fixed):** the new `is { }` capture is replaced with an explicit null check.

Review-fix verification: typed-result architecture tests 16/16; Payment unit tests 219/219; Payment
integration tests 8/8; `git diff --check` clean.

Incremental review of `a4ae0081e..d0fe18afe` found no additional issues across correctness,
microservice isolation, module boundaries, seeding, C# conventions, or changed-path coverage.

### 2026-08-05 — first adversarial review of the Phase 2 diff (`origin/main...HEAD`)

Verdict: careful, mostly-correct migration — cancellation preserved, no provider-message leak, unknown
wire codes fail loudly, rounding/VAT/cumulative-refund math sound. No defect mischarges a payer in the
normal flow today. Findings:

- **H1 (financial — FIXED in this commit):** the cutover removed Payment's in-service cross-check that a
  deferred charge's gross was payer-reviewed. `CalculateBoundAsync` is now pure `rate × caller gross`;
  the binding persists no gross/reviewed amount, and a deferred bind happens with `gross=null`, so
  nothing is validated at bind. `CaptureBoundCommissionAsync(gross=G)` then charges from `G` with no
  check that `G` equals the payer-reviewed `FinalSettlementGrossMinor`. Plan-consistent (§8); the primary
  control now lives in B2B's §4.1 gross freeze (out of this diff). Not "charges wrong today" — it is the
  removal of defense-in-depth. **This is the `CalculateBoundAsync` sign-off:** accept it (relying on the
  B2B §4.1 freeze as the compensating control), or persist the reviewed gross/ceiling on the binding and
  re-assert it at money-movement. Tommy selected exact reviewed-`Money` persistence and rejected
  caller-supplied expected calculation fields. Implemented by atomically persisting the reviewed gross
  on the binding and rejecting unconfirmed or different bound money movement before Stripe.
- **M2 (correctness — FIXED in this commit):** `PaymentError.FromCode` greedily claimed all `payment.commission_*`
  codes into `CommissionFailure` via its `_ =>` fall-through, so composite unions' own `CommissionFailure`
  arm was dead — a server `ManagerPaymentError.CommissionFailure` round-tripped to a `PaymentFailure`-shaped
  case on the client. Code/Kind survive, but `is …CommissionFailure` matching silently never fires, and
  it was untested. Composite `FromCode` now tries `CommissionError.FromCode` before
  `PaymentError.FromCode`, the commission fall-through is removed, and a server-case→wire→client-case
  test proves `ManagerPaymentError.CommissionFailure` survives.
- **M3 (fault-swallowing — FIXED in this commit):** `StripeFailureClassifier` mapped 400/404/409/422 all to a
  benign `PaymentRejected` decline. A 404 `resource_missing` / 400 `invalid_request_error` is an infra/
  logic fault that should throw (retry/dead-letter), not report a decline — violates the convention
  (infra/unknown Stripe faults stay exceptions). Classification is now limited to HTTP 402,
  `card_error`, or a non-empty Stripe decline code; `resource_missing` and `invalid_request_error`
  propagation are covered directly.
- **Lows:** L4 settlement refunds publish `escrow.refund_*` codes (wrong namespace); L5 "already refunded"
  returns `Some` not the no-op `None` (pre-existing); L6 `EnsureTransition` throws after the Stripe side
  succeeded (latent, guarded).

Clean: escrow `Result<Option<T>,E>` semantics, rounding/VAT/refund math, wire hygiene/cancellation, owned-error conventions.

## Decisions, discoveries, blockers, and deviations

- H1 uses exact `Money`, not a ceiling: Payment atomically confirms the payer-reviewed gross on the
  binding, calculates commission and payer total itself, and rejects unconfirmed or different money
  movement before Stripe. Caller-supplied expected commission/payer-total parameters are removed.
- Release/refund absence is a benign idempotent no-op represented by `Option.None`; successful execution returns `Option.Some`.
- Owned Result/Option stay in-process. Protobuf retains an owned wire contract with explicit mapping at the gRPC boundary.
- Every operation error, including payload-free errors, is a named Dunet union with one exhaustive
  `Definition` switch on the root. Stable published codes use `[ErrorCode]` where natural case naming
  would derive a different code; client reverse maps are closed per operation and never parse through
  `FromCode` chains.
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

### 2026-08-08 - Platform sync and post-merge publication completed

- Action: Followed generated PR #420 through replacement CI and merge-queue runs, diagnosed the
  concurrent Stripe-fixture collision, and confirmed GitHub's final merge plus post-merge workflows.
- Evidence: PR #420 merged as `372be1041`; isolated merge-group run `31223601869` passed every gate
  except Customer UI scenario `Customer completes 3DS challenge`, which timed out waiting for Stripe's
  card iframe. Publish run `31225852815` and platform-sync run `31225952562` both passed; platform
  `0.1.0-alpha.0.857` restored successfully from the feed and no recursive sync PR remains open.
- Outcome: Payment's breaking package cutover and B2B/Customer consumer migration are landed on
  `main`; the B2B and Auth dependency gates are open.
- Follow-up: Update both waiting ledgers, close donor PR #296, clean the sync worktree, and finish the
  docs-only Payment closeout.

### 2026-08-07 - Repaired and pushed platform-sync PR #420

- Action: Migrated B2B and Customer production consumers, unit tests, integration fixtures, and
  mocks to the five published owned-result Payment interfaces; committed and pushed `d67416546`.
- Evidence: full Release solution build 0 errors; legacy-client grep zero; affected unit tests
  79/79, 96/96, and 18/18; Docker health green before B2B 5/5 and Customer 4/4 integration projects;
  local, remote-tracking, and PR heads all equal `d67416546`.
- Outcome: The breaking consumer migration is locally green and replacement CI now owns the live
  platform-sync gate.
- Follow-up: Monitor exact PR #420 head `d67416546` through green and merged.

### 2026-08-07 - Payment package published and opened the breaking sync gate

- Action: Followed the source merge's package publication and generated platform-sync workflow to
  terminal results, then inspected PR #420's failed build.
- Evidence: `Publish packages` run `31212157110` succeeded with version
  `0.1.0-alpha.0.853`; platform-sync workflow `31212305398` succeeded; PR #420 build job
  `92977870685` failed with 24 missing-type errors across the five removed legacy clients.
- Outcome: The new Payment client is available on the feed and the publish-first consumer migration
  is now legal in the generated sync PR.
- Follow-up: Migrate B2B and Customer consumers in the isolated PR #420 worktree and drive it green.

### 2026-08-07 - Removed the merged Payment source checkout

- Action: Removed the merged `Feature/PaymentOwnedResultExpansion` worktree and deleted its local
  branch after transferring all post-source-head plan state to this closeout branch.
- Evidence: Git no longer registers the feature worktree; its exact long-path residue is absent; the
  local branch is deleted; the remote branch was already absent.
- Outcome: No merged feature checkout or branch remains, and this docs worktree is the sole recovery
  anchor for the live package and downstream delivery gates.
- Follow-up: Follow the package publication caused by merge commit `b66325acd`.

### 2026-08-07 - Transferred recovery state to the docs closeout worktree

- Action: Created `Docs/typed-result_payment-owned-result-expansion_closeout` from merged
  `origin/main` at `b66325acd`, cherry-picked the four plan-only commits after source PR head
  `a40761eba`, and moved this plan/ledger identity to the short closeout path.
- Evidence: `origin/main..HEAD` contains only the active plan and ledger; normalized transferred plan
  and ledger content matched the source worktree before the identity update.
- Outcome: The docs closeout worktree is the recovery anchor for publication, platform sync,
  downstream handoff, donor closure, and terminal plan deletion.
- Follow-up: Remove the merged source worktree/branch and continue `## Next Steps` here.

### 2026-08-07 - PR #392 merged with full E2E green

- Action: Monitored merge-group run `31209734022` and PR #392 to a terminal result without retrying
  or toggling queue state.
- Evidence: API E2E passed; B2B and Customer UI E2E passed; no merge-group failure occurred; PR #392
  merged exact source head `a40761eba` as merge commit `b66325acd`.
- Outcome: The canonical Payment source PR is terminal green. Recovery ownership must now move to
  the docs closeout worktree before publication and platform-sync monitoring.
- Follow-up: Execute the closeout-transfer action in `## Next Steps`.

### 2026-08-07 - PR #392 entered the merge queue

- Action: Performed the single documented auto-merge disable/re-enable nudge on exact remote head
  `a40761eba` after the green-but-unadmitted state.
- Evidence: GraphQL reports `mergeQueueEntry.state = QUEUED`; PR #392 remains open at
  `a40761eba` with `[full-e2e]`.
- Outcome: The queue now owns delivery and will run the full API/UI E2E gate.
- Follow-up: Monitor the queue to the terminal result in `## Next Steps`.

### 2026-08-07 - Green PR was not admitted to the merge queue

- Action: Enabled merge-queue auto-merge on exact green head `a40761eba` and queried its GraphQL
  queue entry.
- Evidence: PR #392 remains open/CLEAN with `[full-e2e]`, but `mergeQueueEntry` is null and the
  auto-merge request still carries its stale 2026-08-05 timestamp.
- Outcome: This is the documented GitHub re-evaluation glitch, not a CI failure. One explicit
  disable/re-enable nudge is required before queue monitoring.
- Follow-up: Execute the one-time queue-admission nudge in `## Next Steps`.

### 2026-08-07 - Replacement PR checks passed green

- Action: Followed replacement run `31208576213` to a terminal result on exact PR head
  `a40761eba`.
- Evidence: Build, every backend/frontend carve, every unit project, and every integration project
  passed; PR-level API/UI E2E skipped as designed; PR is open/CLEAN with `[full-e2e]`.
- Outcome: PR #392 is ready for merge-queue admission on its unchanged verified remote head.
- Follow-up: Execute the queue-admission action in `## Next Steps`.

### 2026-08-07 - Pushed the verified PR-check fix

- Action: Compound-pushed the verified, review-clean recovery range to PR #392 after confirming it
  was open and unlocked.
- Evidence: The starting PR head was `bbe9a3522`; local `HEAD`,
  `origin/Feature/PaymentOwnedResultExpansion`, and PR `headRefOid` all matched replacement work head
  `40695a4b3` after fetch.
- Outcome: Replacement PR checks are running on the exact fixed head; `full-e2e` remains applied.
- Follow-up: Wait for the replacement checks and execute `## Next Steps` when terminal green.

### 2026-08-07 - Incremental review cleared the PR-check fix

- Action: Reviewed `df76074c4..2e120cb40`, including the architecture-guard fix and its recovery
  checkpoints, through all six repository lenses.
- Evidence: CI2 is fixed in `2e120cb40`; `reviews/Feature-PaymentOwnedResultExpansion.md` is stamped
  through that commit and records no additional findings.
- Outcome: The replacement branch head is verified and review-clean for its compound push.
- Follow-up: Execute the replacement push action in `## Next Steps`.

### 2026-08-07 - Fixed the typed-result architecture guard

- Action: Added the documented exhaustive root `Definition => this switch` shape to the supported
  Dunet definition forms in `TypedResultArchitectureTests`.
- Evidence: Shared.Api unit tests pass 52/52; the full Release API solution builds with 0 errors and
  seven existing warnings outside the changed test.
- Outcome: The deterministic PR-head failure is fixed locally and the branch is ready for incremental
  review before its replacement compound push.
- Follow-up: Execute the review-and-push action in `## Next Steps`.

### 2026-08-07 - PR checks exposed a stale typed-result architecture guard

- Action: Followed PR #392's exact pushed head through its first CI run and reproduced the one
  primary failed unit project locally.
- Evidence: Run `31206320776`; `Concertable.Shared.Api.UnitTests` failed
  `DunetUnionDefinitions_UseSupportedDefinitionShape` 51 passed / 1 failed. Ten Payment unions use
  the documented exhaustive root `Definition => this switch` shape, while the guard accepts only
  legacy generated-`Match` and abstract/per-case definitions. Other red unit jobs were cancelled by
  fail-fast; Payment unit passed in CI.
- Outcome: PR #392 was not enqueued. The failure is deterministic and owned by this branch's error
  convention migration, so the architecture guard must be corrected before a replacement push.
- Follow-up: Execute the fix-and-verification action in `## Next Steps`.

### 2026-08-07 - Selected full merge-queue E2E for PR #392

- Action: Verified two branch commits carry `Skip-E2E: true` trailers and applied the authoritative
  `full-e2e` label to PR #392.
- Evidence: PR head `bbe9a3522` has labels `[full-e2e]`, remains open, and its PR checks are running.
- Outcome: The breaking Payment package cutover cannot inherit a historical E2E opt-out; both queue
  E2E suites will run after admission.
- Follow-up: Wait for the exact PR head's checks and execute `## Next Steps` when terminal green.

### 2026-08-07 - Pushed the verified compound Payment head

- Action: Pushed the current, verified, review-clean branch head to PR #392.
- Evidence: The starting remote/PR head was `bcac5261d`; 101 commits were pushed through work head
  `d6ab44540`; local `HEAD`, `origin/Feature/PaymentOwnedResultExpansion`, and PR `headRefOid` all
  matched `d6ab44540` after fetch.
- Outcome: The canonical PR now carries the current Payment implementation and its complete local
  delivery checkpoints. PR checks are running on the exact verified head.
- Follow-up: Execute the check-and-queue action in `## Next Steps`.

### 2026-08-07 - Final current-main incremental review found no issues

- Action: Reviewed `842b9c332..df76074c4`, including the last current-main merge and verification
  checkpoints, through all six repository lenses.
- Evidence: `reviews/Feature-PaymentOwnedResultExpansion.md` records no new findings and is stamped
  through `df76074c4`; `git diff --check` is clean.
- Outcome: The current, fully verified branch is review-clean and ready for its two-leg verified
  push to PR #392.
- Follow-up: Execute the PR delivery action in `## Next Steps`.

### 2026-08-07 - Passed the final current-main verification gate

- Action: Rebuilt the full Release API solution after merging `b46d10ec8`, reran Payment unit tests,
  passed the repository Docker data-round-trip preflight, and reran Payment SQL integration.
- Evidence: Full solution build 0 errors with seven existing warnings outside Payment; Payment unit
  222/222; Payment SQL integration 8/8.
- Outcome: The current local branch is fully verified; only the incremental review remains before
  the authorized compound push.
- Follow-up: Execute the review-and-delivery action in `## Next Steps`.

### 2026-08-07 - Merged the final current-main update before delivery

- Action: Refreshed `origin`, found PR #392's local branch two commits behind, and merged current
  `origin/main` at `b46d10ec8` as `17c8bb2b3` without conflicts.
- Evidence: The branch is now 0 behind / 76 ahead of `origin/main`; the incoming changes only update
  the plan-handoff hook and its tests.
- Outcome: The branch is current, but the mandatory post-merge build, affected tests, and incremental
  review must pass before the compound push.
- Follow-up: Execute the verification-and-review action in `## Next Steps`.

### 2026-08-07 - Incremental delivery review found no issues

- Action: Reviewed `ee8dbdd57..842b9c332`, including the current-main merges and the branch-owned
  Payment error-convention and conflict-resolution changes, through all six repository lenses.
- Evidence: `reviews/Feature-PaymentOwnedResultExpansion.md` records no new findings and is stamped
  through `842b9c332`; `git diff --check` is clean.
- Outcome: The verified local branch is review-clean and ready for the compound push and merge-queue
  delivery workflow.
- Follow-up: Execute the PR #392 delivery action in `## Next Steps`.

### 2026-08-07 - Cleared the Payment SQL integration gate

- Action: Reconciled the canonical worktree and PR state, passed the repository Docker
  data-round-trip health check, and ran the committed Payment SQL integration project.
- Evidence: Local `HEAD` `0f614bbbe` is clean and 0 behind `origin/main` at `59bdd7a8a`; PR #392
  remains open at the older remote head `bcac5261d`; Payment SQL integration passed 8/8.
- Outcome: Every local build, unit, grep, and SQL integration gate is green. The local merge and
  error-convention commits are ready for the required incremental review before compound push.
- Follow-up: Execute the incremental review and delivery action in `## Next Steps`.

### 2026-08-07 - Restored the required companion plan

- Action: Added `plans/TYPED_RESULT_MIGRATION_PAYMENT_PLAN.md`, changed this ledger's `Plan` metadata
  from the epic roadmap to that companion, and refreshed the roadmap's Payment delivery state.
- Evidence: The handoff hook's 10 unit tests pass, and `expected_pointer` resolves this ledger to the
  companion plan and the exact worktree continuation command without error.
- Outcome: The active Payment lifecycle now satisfies the repository's ROADMAP -> PLAN -> PROGRESS
  contract and can be handed off safely.
- Follow-up: Execute the Docker integration and delivery action in `## Next Steps`.

### 2026-08-07 - Updated Payment errors to the current convention

- Action: Merged current `origin/main` into the canonical worktree, resolved Payment client conflicts
  for the branch-owned API, replaced the obsolete static/per-case/`FromCode` error design with named
  Dunet unions and closed operation-specific gRPC reverse maps, and repeated the source build gates.
- Evidence: Payment Release build 0 warnings/0 errors; Payment unit 222/222; full Release API solution
  build 0 errors with seven existing warnings outside Payment; error-convention
  and conflict-marker greps clean. Docker's API timed out during the integration preflight.
- Outcome: The merge and convention migration are locally checkpointed with source and unit/build
  verification green on platform `0.1.0-alpha.0.847`; Payment SQL integration is the sole local
  blocker before review, push, and merge-queue delivery.
- Follow-up: Restore Docker health and execute the exact integration-and-delivery action in
  `## Next Steps`.

### 2026-08-05 - Opened canonical Payment PR #392

- Action: Pushed the previously-unpublished canonical branch and opened GitHub PR #392 against
  `main` with the verified owned-result cutover and its review/plan checkpoints.
- Evidence: the remote branch did not exist before the push; local HEAD, remote head, and PR head
  all matched `1c9380726333643be812d11a4db256141ccd3b5d` after creation. PR URL:
  `https://github.com/Concertable/concertable/pull/392`.
- Outcome: the one canonical Payment PR is open. Five newer `main` commits arrived after local
  verification; the merge workflow owns the mandatory update and rebuild before enqueueing.
- Follow-up: run `merge` for PR #392 with `full-e2e`, then own publication, the generated breaking
  platform-sync migration, the B2B ledger handoff, and donor PR #296 closure.

### 2026-08-05 - Passed the committed-tree delivery gate

- Action: Committed the six review fixes as `d0fe18afe`, incrementally reviewed that commit, and
  repeated the complete owner and standalone-carve verification on the committed tree.
- Evidence: full Release solution build 0 errors; Payment unit 219/219; Shared API unit 52/52;
  Payment integration 8/8; all nine carved deployable-closure projects built from packages with
  0 errors. The incremental review found no new issues. PR preflight found and corrected the Payment
  architecture page's stale FluentResults sentence and pre-migration adapter names.
- Outcome: the canonical Payment branch is review-clean and locally ready for its full-E2E PR.
- Follow-up: push the verified branch, open the one canonical PR, and checkpoint its number and
  exact remote head.

### 2026-08-05 - Completed canonical H1 review and fixes

- Action: Established the missing branch-owned full review artifact, reviewed
  `3e3bcce89..a4ae0081e` through all six lenses, and fixed all six confirmed findings.
- Evidence: `reviews/Feature-PaymentOwnedResultExpansion.md`; BUG1, BUG2, BUG3, CI1, TEST1, and CV1
  are all closed. Typed-result architecture tests pass 16/16, Payment unit tests 219/219, Payment SQL
  integration tests 8/8, and `git diff --check` is clean.
- Outcome: no review finding remains open. The next gate is committed-tree owner verification and
  standalone carve before canonical PR delivery.
- Follow-up: commit this review-fix checkpoint, repeat the complete committed-tree owner/carve gate,
  then push and open the full-E2E PR.

### 2026-08-05 - Reconciled H1 checkpoint with advancing main

- Action: Refreshed `origin`, the canonical branch, worktree inventory, and GitHub PR state before
  starting the requested review.
- Evidence: clean `Feature/PaymentOwnedResultExpansion` at `951472cae`, 61 ahead / 52 behind
  `origin/main` at `3e3bcce89`; no canonical PR; donor PR #296 still open at `82d0555cd`.
- Outcome: branch identity and exclusive Payment ownership still match, but current main must be
  merged before further work. The previous review has no canonical branch review markdown/watermark,
  so a full review must establish one instead of guessing an incremental start SHA.
- Follow-up: merge current main, run the full review and address clear findings, then repeat the
  committed Payment owner and carve gates before push/PR delivery.

### 2026-08-05 — Implemented reviewed-Money enforcement

- Action: Removed caller-supplied expected calculation values, converted changed Payment monetary
  boundaries to `Money`, added atomic reviewed-gross confirmation, enforced it on every bound money
  movement, regenerated Payment's initial migration, and added invariant/concurrency coverage.
- Evidence: Payment build 0 warnings/0 errors; unit tests 209/209; Payment integration tests 7/7;
  `git diff --check` clean; Payment migration contains nullable `ReviewedGrossMinor`. The diagnostic
  full integration suite passed B2B Artist 17/17 before B2B Concert's 144 tests failed at fixture
  startup with Windows `ERROR_FILENAME_EXCED_RANGE` loading `Microsoft.Data.SqlClient.SNI.dll`.
- Outcome: H1 is fixed in this implementation checkpoint. No expected commission/payer-total input
  survives; Payment owns calculation and refuses unconfirmed or mismatched gross before Stripe.
- Follow-up: Run incremental review, fix findings, sync current main, repeat the committed owner/carve
  gates, then push and open the canonical full-E2E PR.

### 2026-08-05 — Selected Money-based H1 defense-in-depth

- Action: Tommy selected Payment-owned reviewed-gross enforcement and required `Money` at monetary
  boundaries; caller-supplied expected calculation fields were rejected as redundant.
- Evidence: Current branch is clean at merge `059b4a6f6`, 0 behind `origin/main`, on platform
  `0.1.0-alpha.0.814`; existing Payment client/application commission and bound-payment contracts
  still expose primitive decimal or minor-unit/currency pairs.
- Outcome: H1 is unblocked for implementation. Payment will confirm one exact reviewed `Money` value
  per binding and reject unconfirmed or mismatched money movement before Stripe.
- Follow-up: Implement the `## Next Steps` contract, log the remaining primitive-money debt, and run
  the complete owner verification gate.

### 2026-08-05 — Registered the blocked B2B downstream handoff

- Action: Added B2B checkpoints 6-7 to this dependency-owning ledger and reconciled the canonical
  Payment branch with current `origin/main` at `0ed29d8f0`.
- Evidence: `plans/typed-result/B2B_PROGRESS.md` names the Payment merge, publication, and green
  platform-sync as its single gate; the typed-result roadmap maps that dependency to this worktree.
- Outcome: the Payment delivery session now owns waking B2B when the package gate opens; B2B no longer
  depends on a remembered prompt or repeated polling.
- Follow-up: after Payment's publish/platform-sync gate is green, update the B2B ledger and surface its
  resume prompt before closing the Payment plan lifecycle.

### 2026-08-05 — Reconciled the H1 decision gate with current main

- Action: Refreshed `origin` and GitHub before resuming the Payment owner.
- Evidence: Canonical branch is clean at `581477754`, has no PR, and is 56 ahead / 65 behind
  `origin/main` at `0ed29d8f0`; Payment's current-main platform pin is `0.1.0-alpha.0.814`. Frozen
  donor PR #296 remains open at the unchanged `82d0555cd` head.
- Outcome: H1 remains the single hard stop. The previously verified implementation is now stale
  against main and must be synced and fully re-verified after the financial-contract decision.
- Follow-up: Obtain the H1 choice recorded in `## Next Steps`, then merge current main and repeat the
  complete owner gate before review and delivery.

### 2026-08-05 — Fixed the review findings and restored the complete green owner gate

- Action: Fixed M2's composite error precedence and M3's over-broad Stripe classification, added
  server-to-client commission-case fidelity plus decline/invalid-request/resource-missing regressions,
  and ran every Payment owner verification gate.
- Evidence: Payment build 0 warnings/0 errors; unit tests 201/201; integration tests 7/7; full Release
  solution build 0 errors with 9 unrelated warnings; standalone package-only Payment carve 0 errors;
  obsolete-surface/dependency/generated-API greps and `git diff --check` clean.
- Outcome: M2 and M3 are resolved on current platform `0.1.0-alpha.0.798`; H1 is the only open review
  item and requires Tommy's explicit financial-contract decision before delivery.
- Follow-up: Obtain the H1 decision recorded in `## Next Steps`.

### 2026-08-05 — Reconciled the resumed owner with current main

- Action: Refreshed GitHub and `origin`, confirmed the canonical branch still has no PR and donor PR
  #296 remains open/DIRTY at `82d0555cd`, then merged 32 current `origin/main` commits.
- Evidence: Clean merge `e787dd0122`; Payment platform pin `0.1.0-alpha.0.798`; branch is 0 behind and
  55 ahead of `origin/main`.
- Outcome: Worktree identity and ownership still match the ledger; M2/M3 can be implemented against
  the current platform without fragmenting the in-flight Payment phase.
- Follow-up: Apply M2 and M3 and rerun the owner-side verification gate before requesting the H1
  financial decision.

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
cd C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-payment-owned-closeout
Read @plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md and @plans/TYPED_RESULT_MIGRATION_PAYMENT_PROGRESS.md and do what its `## Next Steps` says.
```
