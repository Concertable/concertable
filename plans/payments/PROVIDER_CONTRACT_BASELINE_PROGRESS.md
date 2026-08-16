# Provider contract baseline progress

- Plan: `plans/payments/PROVIDER_CONTRACT_BASELINE_PLAN.md`
- Roadmap: `plans/payments/STRIPE_RELIABILITY_ROADMAP.md`
- Roadmap item: `payments/provider-contract-baseline`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\payments_provider-contract-baseline`
- Branch: `Feature/payments_provider-contract-baseline`
- PR: #597 — https://github.com/Concertable/concertable/pull/597 — open draft; checkpoint head `3d4dd68c482b236cf28ebc8b8e48a7efab08e10a` passed exact-head CI run 31950346307; this review-ready ledger checkpoint is the transport leg
- Review readiness: **READY FOR REVIEW** — all four implementation phases and exact-head CI are green; convert [PR #597](https://github.com/Concertable/concertable/pull/597) from draft after this checkpoint transport is green, then run `/review`
- Dependency/package gates: Phases 1 through 3 are complete and Phase 4 is locally implemented; the production/live-mode Stripe account has no webhook endpoint, so future deployment is locked to `2025-01-27.acacia` and must create the endpoint at the actual Payment Web URL while installing its signing secret; compatibility is anchored to published `0.1.0-alpha.0.1009`; the local branch carries platform pin `0.1.0-alpha.0.1021`
- Last reconciled: 2026-08-16 against open draft PR #597 checkpoint head `3d4dd68c4`, exact-head CI run 31950346307, published Payment packages `0.1.0-alpha.0.1009`, and `origin/main` `668ba639c`

## Current state

Phases 1 through 3 are complete, committed, and pushed. Phase 4 is implemented locally in this commit:
the checked-in generator captured 2,073 Contracts signatures, 1,161 Client signatures, 13 message
URNs, and the `payment.proto` descriptor set from published `0.1.0-alpha.0.1009`. Candidate tests
require those public APIs, URNs, protobuf messages/enums/fields/services/RPCs, field numbers, types,
cardinality, and request/response types to remain an additive subset. A frozen consumer project
compiles against the exact published Contracts and Client packages, and architecture tests enforce
provider/consumer purity across the published assemblies and deployable Payment projects. Work head
`f210577564ea4ab78c56c2f687762b9378b6a083` cleanly incorporates `origin/main`
`668ba639c6fb59a2513ba6c70d669b6b2d01f974`, is locally verified, and was pushed through the work-head
leg with local, remote-tracking, and PR equality.

`api/Concertable.Payment/PROVIDER_CONTRACT.md` now owns
the provider-product matrix, operation/attempt identity, normalization and transition tables,
terminality, retry/revision/expiry, safe failures, Connect posture, consumer ownership, compatibility
islands, and version assumptions. `provider-contract-inventory.json` classifies 43 current entry
points across seven explicit roots into 23 complete decisions, and the Payment architecture test
fails for an unclassified provider call, consumer Payment-client call, frontend confirmation, or
client-secret parser.

A read-only Stripe MCP query enumerated the only OAuth-accessible live context as Concertable account
`acct_1QqfAGLtYbsqaOIf` and returned zero webhook endpoints. There is therefore no production endpoint
ID, status, or API version to reconcile with Stripe.net `47.3.0`. The absence is explicit evidence:
normalization fixtures and future endpoint creation target `2025-01-27.acacia`, while endpoint creation
and signing-secret installation wait for an actual standalone Payment deployment. The configured test
endpoint `we_1RCqowQ1mmqr287N9MeY0iRV` remains disabled at
`2025-01-27.acacia` and subscribes only to `payment_intent.succeeded`; it is not a complete production
template because current Payment runtime also handles `payment_intent.payment_failed`,
`setup_intent.succeeded`, and `setup_intent.setup_failed`.

PR #552 merged at `33f07c47a497586324edacdcfc10321a9d3f02ee`, and its additive Payment
contracts are present after merging current `origin/main`. Draft PR #597 remains open at remote head
`bb9482c77a8264de35aa93711b99bf4f9bb2697b`; exact-head CI run 31944017441 passed all required jobs.
Local merge commit `1067982097d914f7090b94c06d0fd0bde878ecf6` brings the branch to current
`origin/main` `7db0c9be964ea9ff4a2d64468c4462eae91d36ff` with platform pin
`0.1.0-alpha.0.1017`; that local candidate is not yet pushed. The historical
`Refactor/GroupStripeWebhookHandling` branch is superseded evidence only.

Phase 2 adds the provider-neutral operation identity, session kind, normalized state, terminal/retry
disposition, safe-failure vocabulary, client descriptor/snapshot records, and matching protobuf
messages without adding an RPC. `PaymentOperationStateChangedV1` has stable message type
`concertable.payment.payment-operation-state-changed.v1`. Reunion Result mapping remains supported on
.NET 10 through the closed Dunet `PaymentOperationError`; the planned .NET 11 native-union migration
follows the complete Stripe/provider refactor rather than interrupting it.

Phase 3 adds pure Domain specifications for pinned Stripe status normalization, same-revision
transitions, identity and freshness checks, duplicate/out-of-order observations, terminal protection,
explicit cancellation, retry/revision decisions, and provider-confirmed authorization expiry. The
specification has no EF, MassTransit, gRPC, Stripe SDK, timer, persistence, webhook, or consumer
dependency. Its exhaustive test matrix evaluates all 405 state pairs across automatic payment,
authorization, both setup kinds, and refund.

## Next Steps

After this review-ready checkpoint transport is exact-head green and PR #597 is converted from draft,
run `/review` against the complete branch. Record every high-confidence finding and disposition in
this ledger before merge becomes the next action. Do not merge in the same turn.

## Completed work

- Generated committed `0.1.0-alpha.0.1009` Contracts/Client public-API, message-URN, and protobuf
  descriptor baselines with a reproducible checked-in generator.
- Added additive compatibility tests, package/service architecture purity gates, and a frozen consumer
  fixture compiled against the exact published Contracts and Client versions.
- Added the Phase 3 pure provider transition, retry/revision, and authorization-expiry specifications.
- Added exhaustive pinned status, 405-state-pair, identity/freshness, duplicate/out-of-order,
  terminality, explicit-cancellation, safe-failure, retry, revision, and expiry coverage.
- Implemented the Phase 1 durable provider contract and linked it from Payment architecture.
- Added the deterministic seven-root inventory with 43 classified entry points and 23 reusable
  decisions, including the finite PR #581 frontend compatibility islands.
- Added Payment unit architecture coverage that enforces exact inventory parity, complete decisions,
  stable scan roots, unique keys, and self-verifying committed entries.
- Queried the configured Stripe test account and recorded endpoint
  `we_1RCqowQ1mmqr287N9MeY0iRV`, URL
  `https://concertable-app.azurewebsites.net/api/webhook`, API version `2025-01-27.acacia`,
  `livemode=false`, and `status=disabled` without changing provider configuration.
- Queried every OAuth-accessible live Stripe context through the authenticated MCP connection and
  proved Concertable account `acct_1QqfAGLtYbsqaOIf` currently has zero webhook endpoints.
- Added the Phase 2 provider-neutral Contracts, Client records, protobuf messages/enums, stable
  `PaymentOperationStateChangedV1` message type, and exhaustive .NET 10 Reunion/Dunet error mapping.
- Added contract and mapper coverage for stable enum values, protobuf fields, safe error definitions,
  unknown-value rejection, optional-field mapping, and Stripe/consumer reference purity.
- Reconciled the inventory scanner with PR #552's command-based B2B capture, deposit, and refund entry
  points while retaining the existing decisions and 43-entry inventory.
- Merged current `origin/main` `2ec423f5f1583a74c2c9121eb82229ca3e46bb42` into the clean feature
  branch as `9b8c1b5d0ee681e70662ef32dfae21b23d02379e`, bringing merged PR #552 and platform pin
  `0.1.0-alpha.0.1015` into the implementation baseline.
- Committed Phase 1 as `7cd053d0719c699e77f4f8d5b4a3803367db6bf5`, pushed the two-commit
  implementation range from current `origin/main`, and opened draft PR #597 for remote validation.
- Created the clean worktree from current `origin/main` on
  `Feature/payments_provider-contract-baseline`.
- Inspected all requested repository guidance, legal/architecture constraints, Payment/Customer/B2B
  backend entry points, customer/B2B web entry points, customer mobile flow, PRs #544/#581/#552, and
  the historical webhook branch.
- Researched current primary Stripe guidance and verified the installed Stripe.net `47.3.0` source
  pins API version `2025-01-27.acacia`.
- Wrote the implementation plan and copied the source roadmap unchanged into this worktree.
- Extended the plan-graph validator and its focused test to recognize a roadmap status-table row as
  the same stable checklist marker as a CommonMark task-list row, preserving the supplied roadmap
  byte-for-byte (`e9898bda8f431d50e14ee1aed74266d043664caa`).
- Normalized docs-reachability diagnostic paths to repository-style forward slashes so its hook tests
  are portable on Windows.
- Pushed reviewed work head `0986af4a2b99203a2671cac51d41715d230cdf90` and verified the
  remote-tracking ref matches.
- PR #594 passed the clean docs-review gate, carried only `.agents/**` and `plans/**`, and was
  admin-merged with `skip-e2e` as `3c8a2c5a847d0f9702884949ed57850c6e494c47`.
- Closed the merged plan-managed source worktree and recreated this Feature worktree from the merged
  current `origin/main`.

## Verification

- Phase 4 compatibility and architecture suite: 6 passed, 0 failed, 0 skipped.
- Phase 4 full Payment unit carve: 440 passed, 0 failed, 0 skipped.
- Payment UnitTests plus frozen published-contract fixture build: succeeded with 0 warnings and 0
  errors; fixture and generator both resolved `Concertable.Payment.Client` and
  `Concertable.Payment.Contracts` exactly at `0.1.0-alpha.0.1009`.
- Focused formatting for UnitTests, the frozen fixture, and the generator: passed after correcting the
  test-helper namespace to match its folder.
- Current-main reconciliation: merged `origin/main` `668ba639c6fb59a2513ba6c70d669b6b2d01f974`
  cleanly; regenerated baselines produced no diff; formatting, 0-warning build, 440-test Payment carve,
  plan graph, and whitespace gates all passed at work head `f210577564ea4ab78c56c2f687762b9378b6a083`.
- Phase 4 work push: starting remote/PR head `324dc4714565b623cba297de461b566787c6a521`;
  pushed range `324dc4714565b623cba297de461b566787c6a521..f210577564ea4ab78c56c2f687762b9378b6a083`;
  local, remote-tracking, and PR work heads all verified at `f210577564ea4ab78c56c2f687762b9378b6a083`.
- Exact-head draft-PR CI run 31950346307 passed at checkpoint head
  `3d4dd68c482b236cf28ebc8b8e48a7efab08e10a`: build, local platform package validation, all five
  standalone service carves, the complete unit matrix, and the complete integration matrix were green;
  E2E jobs were skipped under the draft-PR policy.
- Phase 3 focused provider-contract suite: 106 passed, 0 failed, 0 skipped.
- Phase 3 full Payment unit carve: 434 passed, 0 failed, 0 skipped.
- Payment UnitTests project build: succeeded with 0 warnings and 0 errors.
- Focused `dotnet format --verify-no-changes`: passed.
- Focused XPlat coverage: authorization-expiry specification 100% line/branch; retry specification
  97.72% line and 92% branch; transition specification 98.34% line and 96.38% branch. The explicit
  state-pair oracle asserts all 405 product/state combinations, so removing an allowed or forbidden
  edge changes the focused suite.
- Phase 3 work push: starting remote/PR head `bb9482c77a8264de35aa93711b99bf4f9bb2697b`;
  pushed range `bb9482c77a8264de35aa93711b99bf4f9bb2697b..2c972be16d4fafd26c90d0c2d6d88887e9c159f2`;
  local, remote-tracking, and PR work heads all verified at `2c972be16d4fafd26c90d0c2d6d88887e9c159f2`.
- `dotnet build api/Concertable.Payment/tests/Concertable.Payment.UnitTests/Concertable.Payment.UnitTests.csproj --no-restore`: succeeded with 0 warnings and 0 errors.
- `dotnet test api/Concertable.Payment/tests/Concertable.Payment.UnitTests/Concertable.Payment.UnitTests.csproj --no-build --no-restore`: 318 passed, 0 failed, 0 skipped.
- Focused inventory architecture coverage: 46 passed after correcting the detector to distinguish
  Stripe SDK services from internal services and secret-ID splits from kind-prefix checks.
- Inventory parse: schema 1, seven roots, 23 decisions, 43 entry points.
- `python .agents/hooks/docs_reachability.py --root <worktree>`: 0 errors, 0 warnings.
- `python .agents/hooks/plan_graph.py --root <worktree>`: 0 errors, 0 warnings.
- `git diff --check`: passed.
- Phase 2 focused Payment contract/mapper/error tests: 104 passed, 0 failed, 0 skipped.
- Full Payment unit suite after current-main reconciliation: 374 passed, 0 failed, 0 skipped.
- Payment Client/Contracts build: succeeded with 0 warnings and 0 errors.
- Payment Web and Workers runtime builds: each succeeded with 0 warnings and 0 errors.
- Focused `dotnet format --verify-no-changes` for Payment Contracts, Client, and UnitTests: passed.
- Phase 2 work push: `dd1fa17b82a96c33d9979fe9cb5798d5fd99b6d7..ef4b4c0848820cd0746e44b067c5c922471c985e`; local HEAD, remote-tracking ref, and PR #597 `headRefOid` matched the work head after fetch.
- Exact-head CI run 31941813626 exposed five stale inventory keys after its test merge incorporated
  newer PR #552 closeout changes; the focused local reproduction failed 6 of 374 tests, and the
  command-entry reconciliation restored all 374 tests without changing B2B runtime code.
- Corrected exact-head CI run 31942952029 at `59b8e266d3630c9c98390827d0a2a820ec71d0d2`:
  58 of 58 jobs passed, including build, package, service carves, all unit projects, and integration
  matrices.
- Push verification: local work head, `origin/Feature/payments_provider-contract-baseline`, and PR #597
  `headRefOid` all resolved to `7cd053d0719c699e77f4f8d5b4a3803367db6bf5`.
- Worktree branch was created at and reconciled to `origin/main`
  `836a15a56257a0e35ca5ef5674b39e38eb6767ac` with zero commits behind.
- Source and copied roadmap SHA-256 matched at
  `4181DB21EEF72F29EC4C61536858FE7F5B8ED659ED991C8076C9EB4DE8B2CDB0`.
- GitHub evidence: PR #544 merged at `d6619a85667617fb29b7cbb8ce005b779b39346d`;
  PR #581 merged at `c75890243c44435d707eacf7e51377e4631bcf22`; PR #552 merged at
  `33f07c47a497586324edacdcfc10321a9d3f02ee`; platform-sync PR #601 was open with no failed check.
- Stripe MCP live-context query: `query_succeeded=true`; Concertable account
  `acct_1QqfAGLtYbsqaOIf`, `livemode=true`, returned `endpoints=[]` from
  `GET /v1/webhook_endpoints?limit=100`.
- Stripe test endpoint read: `we_1RCqowQ1mmqr287N9MeY0iRV`, disabled,
  `2025-01-27.acacia`, `livemode=false`, with only `payment_intent.succeeded` enabled.
- Current-main reconciliation: local branch merged `origin/main`
  `2ec423f5f1583a74c2c9121eb82229ca3e46bb42` cleanly as
  `9b8c1b5d0ee681e70662ef32dfae21b23d02379e`; `HEAD..origin/main` is zero.
- Post-merge `python .agents/hooks/plan_graph.py --root <worktree>`: 0 errors, 0 warnings.
- `python .agents/hooks/tests/test_plan_graph.py`: 19 tests passed.
- `python -m unittest discover -s .agents/hooks/tests -p 'test_*.py'`: 62 tests passed.
- `python .agents/hooks/plan_graph.py --root C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\payments_provider-contract-baseline`:
  0 errors, 0 warnings.
- `git diff --check`: passed for the tracked validator changes; the new plan and ledger also passed
  no-index whitespace checks. The unchanged source roadmap retains its existing final blank line.
- Push verification: local work head and
  `origin/Feature/payments_provider-contract-baseline` both resolved to
  `0986af4a2b99203a2671cac51d41715d230cdf90`.
- GitHub reports PR #594 `MERGED` at `3c8a2c5a847d0f9702884949ed57850c6e494c47`;
  the recreated worktree started at that exact `origin/main` commit with zero commits behind.

## Review status

**READY FOR REVIEW.** All implementation phases and exact-head CI run 31950346307 are green for
[PR #597](https://github.com/Concertable/concertable/pull/597). After this review-ready checkpoint
transport is green and the PR is converted from draft, `/review` is the only next action; merge is not
authorized by this continuation.

The planning docs review through PR head `ccb1dd00585b7943a401166f3f8eb3237ed6d628` found no issues across
accuracy, contradiction, document ownership, concision, dangling references, and followable
instructions. Its spent untracked review artifact was deleted after PR #594 merged. No Phase 1
implementation review exists; the branch is not merge-ready.

## Decisions, discoveries, blockers, and deviations

- Current flows stay on PaymentIntents for money movement and SetupIntents for save/verify; Checkout
  Sessions are not selected for any current flow.
- The future public model separates caller-owned `OperationId` from Payment-owned `AttemptId`.
- Existing capture/deposit/refund saga contracts remain authoritative; no universal financial-operation
  abstraction will replace them.
- Full webhook handling, reconciliation, persistence, frontend migration, and removal of the tactical
  3DS bridge remain with later work.
- The full Stripe/provider refactor remains on .NET 10 with Reunion and Dunet; migration to .NET 11
  preview/native unions follows completion of the Stripe refactor.
- The live account has no webhook endpoint, so no production endpoint version exists to infer or
  normalize against. Fixtures target `2025-01-27.acacia`; creation waits for the actual Payment Web
  deployment URL and is a delivery gate, not a Phase 2 implementation blocker.
- The configured test endpoint matches the SDK request version but is disabled and subscribes only to
  `payment_intent.succeeded`; it cannot be copied as the production event selection.
