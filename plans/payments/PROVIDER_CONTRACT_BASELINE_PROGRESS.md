# Provider contract baseline progress

- Plan: `plans/payments/PROVIDER_CONTRACT_BASELINE_PLAN.md`
- Roadmap: `plans/payments/STRIPE_RELIABILITY_ROADMAP.md`
- Roadmap item: `payments/provider-contract-baseline`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\payments_provider-contract-baseline`
- Branch: `Feature/payments_provider-contract-baseline`
- PR: #597 — https://github.com/Concertable/concertable/pull/597 — open; reviewed code head `10d07780fd1feaec34c2f7ae765d91ac8291d83e` is verified at the local, remote-tracking, and PR refs; the review/ledger checkpoint and exact-head CI are required; auto-merge is disabled and the PR is not queued
- Review readiness: **REVIEW COMPLETE — CHECKPOINT AND EXACT-HEAD CI REQUIRED** — all eight findings remain resolved; incremental implementation and security review is current through reviewed code head `10d07780fd1feaec34c2f7ae765d91ac8291d83e` with no new findings
- Dependency/package gates: Phases 1 through 4 are locally complete; the production/live-mode Stripe account has no webhook endpoint, so future deployment is locked to `2025-01-27.acacia` and must create the endpoint at the actual Payment Web URL while installing its signing secret; compatibility is anchored to published `0.1.0-alpha.0.1009`; the branch carries platform pin `0.1.0-alpha.0.1055`
- Last reconciled: 2026-08-17 against open PR #597 reviewed code head `10d07780`, published Payment packages `0.1.0-alpha.0.1009`, and current `origin/main` `9205e82d`; current main and platform pin `.1055` are incorporated through merge `c9dac0b8d` with zero commits behind

## Current state

A locally verified domain-boundary refactor now leaves raw Stripe API version, product, status, and
nullable session evidence in the Stripe normalizer. Successful normalization produces one of five
closed, non-nullable payment operation contexts and invokes a separate provider-neutral transition
evaluator. Domain identity, binding, context, freshness, projection equality, terminal protection,
and legal-transition rules live in focused receiver-owned extensions behind that canonical entry
point. Stripe request idempotency and webhook/persistence deduplication remain future adapter and
infrastructure responsibilities. Fixed mappings and transition tables use frozen collections directly,
without private collection-construction aliases. `PaymentOperationError` remains the single
Reunion-backed authority for safe failure messages. The former
`*Specification` evaluator names are removed, and the general C# convention now records this boundary.
The integration event is named `PaymentOperationStateChanged`; only its stable broker identity carries
the `.v1` version. Every ordinary extension member in the Payment mapper container touched by this PR
now uses a C# 14 `extension(Receiver)` block, and the convention requires complete-container migration
whenever legacy extension code is edited.
Protobuf operation-error kinds now translate through one private frozen table exposed as the
receiver-owned `ToErrorKind()` extension; unknown values map to no internal kind and therefore retain
the existing fail-closed contract-mismatch result.
The shared generic `RpcException` error conversion is also a private C# 14 extension member, so every
named error mapper delegates through `exception.ToError(...)`; the PR contains no remaining
source-first `To...` helper.
Provider-neutral observations retain the closed provider failure classification until one frozen
mapping translates it to a public Concertable failure code. Undefined operation states and provider
failure classifications return typed transition rejections, and state and failure validation are
separate evaluator steps.

Phases 1 through 4 and all eight review findings are complete. The reviewed code is pushed through
`10d07780fd1feaec34c2f7ae765d91ac8291d83e`. The branch resolves
NAT1 at `0686b7f52c68ab492ba7683fa5fee895096785da`, NAT2 at
`19e194c9eaefee2734718a298a127f414f75af6c`, BUG1 at
`055c6bfd868484e847f907926b5da7b6dea55ff9`, SEC1 at
`7b7561fa43b57ca004d082ebd207242f0e4499fd`, NAT3 at
`c4140cd3bb42a8a0f13beb652b7590f98691a63d`, and NAT4 at
`6cc1d59d5281a141f72f9b4f6ddd233ea46da233`; NAT5 and NAT6 are resolved at
`862e4722c484ad44ef08d5017b39395696258b3e`. For Phase 4,
the checked-in generator captured 2,073 Contracts signatures, 1,161 Client signatures, 13 message
URNs, and the `payment.proto` descriptor set from published `0.1.0-alpha.0.1009`. Candidate tests
require those public APIs, URNs, protobuf messages/enums/fields/services/RPCs, field numbers, types,
cardinality, and request/response types to remain an additive subset. A frozen consumer project
compiles against the exact published Contracts and Client packages, and architecture tests enforce
provider/consumer purity across the published assemblies and deployable Payment projects. The local
branch cleanly incorporates current `origin/main` `9205e82df4359df8ddf8dfdace07b4aa09b6d186`
through merge `c9dac0b8d` and is zero commits behind.

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
contracts are present after merging current `origin/main`. PR #597 is open and no longer draft at
reviewed code head `10d07780fd1feaec34c2f7ae765d91ac8291d83e`; the final checkpoint and
exact-head CI are required. NAT1 through NAT6, BUG1, and SEC1 are resolved. The historical
`Refactor/GroupStripeWebhookHandling` branch is superseded evidence only.

Phase 2 adds the provider-neutral operation identity, session kind, normalized state, terminal/retry
disposition, safe-failure vocabulary, client descriptor/snapshot records, and matching protobuf
messages without adding an RPC. `PaymentOperationStateChanged` has stable message type
`concertable.payment.payment-operation-state-changed.v1`. Reunion Result mapping remains supported on
.NET 10 through the closed Dunet `PaymentOperationError`; the planned .NET 11 native-union migration
follows the complete Stripe/provider refactor rather than interrupting it.

Phase 3 adds pure Domain evaluators and receiver-owned extensions for pinned Stripe status normalization, same-revision
transitions, identity and freshness checks, duplicate/out-of-order observations, terminal protection,
explicit cancellation, retry/revision decisions, and provider-confirmed authorization expiry. The
policy has no EF, MassTransit, gRPC, Stripe SDK, timer, persistence, webhook, or consumer
dependency. Its exhaustive test matrix evaluates all 405 state pairs across automatic payment,
authorization, both setup kinds, and refund.

## Next Steps

Commit and push the review/ledger checkpoint, verify local, remote-tracking, and PR head equality, and
require exact-head CI to pass at that final checkpoint-transport head. Keep PR #597 open with
auto-merge disabled; merge only after Tommy reviews and explicitly approves it.

## Completed work

- Moved the shared generic `ToError<TError>` implementation into the `RpcException` C# 14 extension
  block and audited all `To...` conversions introduced by this PR for receiver ownership.
- Resolved NAT5 and NAT6 by rejecting undefined operation states before disposition lookup and by
  retaining the closed provider failure classification through domain validation and deterministic
  frozen failure-code mapping.
- Split the former broad context validation into explicit state and failure validation extensions and
  added direct provider-neutral regressions for malformed states, malformed/incompatible failure
  classifications, and the safe declined result.
- Separated raw Stripe normalization from the provider-neutral payment transition evaluator and added
  direct tests that invoke the reusable domain policy without Stripe types.
- Replaced the nullable internal session discriminator with a closed operation context for payment,
  authorization, payment-method setup, payment-method verification, and refund; null remains legal
  only on untrusted raw Stripe evidence before normalization.
- Split observation identity, provider binding, domain-context, freshness, projection, and lifecycle
  rules into receiver-owned C# 14 extensions behind one evaluator entry point.
- Changed duplicate classification to require the complete mutable persisted projection to be
  unchanged, so same-state safe-failure and capture-deadline changes are applied.
- Added regressions for a same-state `RequiresPaymentMethod` observation changing the persisted
  failure to `Declined` and a same-state authorization observation revising `CaptureBefore`.
- Removed the async-name assumption from the provider inventory scanner, limited semantic discovery to
  Stripe API client/service receiver types, and added synchronous `RefundService.Create` coverage.
- Prevented protobuf failure messages from crossing the public client boundary by deriving every
  known-code message from the central `PaymentOperationError.Definition`; exhaustive mapper tests
  prove arbitrary wire text is ignored and unspecified or unknown codes still fail closed.
- SEC1 focused validation: Payment unit-test project build succeeded with 0 warnings and 0 errors;
  mapper, central error-definition, and operation-contract tests passed 107 of 107; focused Contracts,
  Client, and UnitTests formatting, plan graph, and whitespace checks passed.
- Added a provider-neutral internal decline classification for provider observations, mapped
  `requires_payment_method` declines to the closed safe `Declined` failure, and rejected the
  classification on every incompatible or unknown provider status.
- Replaced import-gated Stripe entry-point regexes with per-project Roslyn receiver binding and added
  focused fully-qualified, namespace-specific, separate global-using, and injected SDK coverage.
- Generated committed `0.1.0-alpha.0.1009` Contracts/Client public-API, message-URN, and protobuf
  descriptor baselines with a reproducible checked-in generator.
- Added additive compatibility tests, package/service architecture purity gates, and a frozen consumer
  fixture compiled against the exact published Contracts and Client versions.
- Added the Phase 3 pure provider transition, retry/revision, and authorization-expiry evaluators.
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
  `PaymentOperationStateChanged` message type, and exhaustive .NET 10 Reunion/Dunet error mapping.
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

- Receiver-owned error conversion at `10d07780`: Payment UnitTests built with 0 warnings and 0 errors;
  focused `PaymentClientResultsTests` passed 17 of 17; the complete Payment unit suite passed 487 of
  487; focused Client formatter verification passed; the PR-scoped declaration audit found no other
  source-first `To...` helper.
- Incremental native and security review covered `862e4722..10d07780` and found no new correctness,
  boundary, seeding, convention, security, or test-coverage issue; both review markers are current at
  `10d07780fd1feaec34c2f7ae765d91ac8291d83e`.
- Receiver-owned error conversion push: starting remote/PR head
  `421f5b91e1de6b2c3270da9786fb75b45f19abc0`; pushed range `421f5b91..10d07780`; local work head,
  remote-tracking ref, and PR #597 head all verified at
  `10d07780fd1feaec34c2f7ae765d91ac8291d83e` with auto-merge disabled.
- Exact-head PR CI run 32046327247 passed at final checkpoint
  `421f5b91e1de6b2c3270da9786fb75b45f19abc0`; solution build, local platform package validation,
  standalone service carves, unit tests, and integration tests were green, while policy-selected E2E
  jobs were skipped.
- Final checkpoint push: local `HEAD`, remote-tracking branch, and PR #597 `headRefOid` all verified at
  `421f5b91e1de6b2c3270da9786fb75b45f19abc0`; the branch was zero commits behind `origin/main`, the
  PR remained open, and auto-merge remained disabled.
- Reviewed domain-boundary correction: Payment UnitTests built with 0 warnings and 0 errors; the
  focused provider-contract suite passed 124 of 124; the complete Payment unit suite passed 487 of
  487; focused Domain and UnitTests formatter verification passed; plan graph and whitespace checks
  are required again at the checkpoint commit.
- Incremental native and security review covered `cb2d41c3..862e4722`, excluding incoming
  `origin/main` changes except merge resolution. NAT5 and NAT6 are resolved at `862e4722`; no findings
  remain open across correctness, security, microservice isolation, module boundaries, seeding, C#
  conventions, or changed-path test coverage.
- Reviewed code push: starting remote/PR head `c7d70ab9fc9ba01519f6d35925301d2c7ac1c262`;
  pushed range `c7d70ab9..862e4722`; local code head, remote-tracking ref, and PR #597 head all verified
  at `862e4722c484ad44ef08d5017b39395696258b3e` with auto-merge disabled.
- Plan-managed deterministic-mapper work push: starting remote/PR head
  `e1186d498311c396a5460d6788dd74d04441e3f9`; pushed range `e1186d49..487e1983`; local work head,
  remote-tracking ref, and PR #597 head all verified at `487e19833273c28a9875199069c080c0af9494b2`.
- Dictionary-backed error-kind mapper commit `b4f65daaf`: Payment UnitTests built with 0 warnings and
  0 errors; focused `PaymentClientResultsTests` passed 17 of 17, including protobuf kind `999`; the full
  Payment unit suite passed 479 of 479; focused formatter verification passed.
- Final current-main reconciliation at `cb2d41c3`: restored platform pin `.1052`, built the Payment
  unit-test project with 0 warnings and 0 errors, passed all 479 Payment unit tests, and remained zero
  commits behind `origin/main`.
- Incremental implementation and security review covered `45faf5d7..cb2d41c3`, excluding incoming
  `origin/main` changes except merge resolution, and recorded no new correctness, boundary, seeding,
  convention, security, or test-coverage findings.
- Plan-managed correction work push: starting remote/PR head `af5bb9c3ed6aef2bc0fc50e442eec1e9a5ed9e88`;
  pushed range `af5bb9c3..6e0482d6`; local work head, remote-tracking ref, and PR #597 head all
  verified at `6e0482d6eac215ec844d965860975635c1da9c00`.
- Requested contract/convention correction at `45faf5d7`: Payment UnitTests built with 0 warnings and
  0 errors and passed 478 of 478; focused formatting, plan graph, and whitespace checks passed.
- Incremental native and security review covered `7c1253f6..45faf5d7`, excluding incoming
  `origin/main` changes except merge resolution, and recorded no new correctness, boundary, seeding,
  convention, security, or test-coverage findings. The review watermark's invalid long-form expansion
  of `7c1253f6` was replaced by the verified current head.
- Reviewed work push `200e49f3..0402ee59`: local `HEAD`, remote-tracking branch, and PR #597
  `headRefOid` all verified at `0402ee590ae371ddb2a6f8de60f3fb76a06cff7d`.
- Final current-main/platform reconciliation at `c99443ce`: restored the published `.1039` package
  closure, built with 0 warnings and 0 errors, and passed all 478 Payment unit tests; plan graph and
  whitespace checks passed with the branch zero commits behind `origin/main`.
- Current readability refactor and current-main reconciliation: Payment UnitTests built with 0
  warnings and 0 errors and passed 478 of 478; focused formatting passed; plan graph and whitespace
  checks passed before the review wording checkpoint.
- Incremental implementation and security review covered `200e49f3..7c1253f6`; CV1 corrected an
  over-broad extension-placement convention, and no runtime, boundary, security, or test-coverage
  findings remain open.
- Exact-head PR CI run 31963564771 passed at
  `3b49ef2d0a715626abd93aea39df80657da20bfd`; solution build, local platform package validation, all
  service carves, unit tests, and integration tests were green; policy-selected E2E jobs were skipped.
- Plan-managed work push: starting remote/PR head
  `f56c80fde78c3cc99016bf65a122de250e5adcc3`; pushed range
  `f56c80fde78c3cc99016bf65a122de250e5adcc3..ce43a2283c26416ca60593aefca35a79d2159698`;
  local work head, remote-tracking ref, and PR head all verified at `ce43a2283c26416ca60593aefca35a79d2159698`.
- Current-main reconciliation build succeeded with 0 warnings and 0 errors; the focused
  `ProviderContract` filter passed 116 of 116; plan graph and whitespace checks passed.
- Incremental implementation and security review through merge commit
  `01171e1b21b8a08a273eafb3d3f99859081756e2` recorded no additional findings.
- NAT4 Payment UnitTests build succeeded with 0 warnings and 0 errors; focused transition tests passed
  42 of 42 and the full provider-contract filter passed 116 of 116.
- NAT4 focused `dotnet format --verify-no-changes` passed.
- NAT3 Payment UnitTests build succeeded with 0 warnings and 0 errors; focused
  `ProviderContractInventoryTests` passed 51 of 51.
- BUG1 Payment UnitTests build: succeeded with 0 warnings and 0 errors.
- BUG1 focused transition tests: 40 passed, 0 failed, 0 skipped.
- BUG1 full provider-contract filter: 113 passed, 0 failed, 0 skipped.
- BUG1 focused `dotnet format --verify-no-changes`: passed; workspace-load warnings only.
- NAT2 Payment UnitTests build: succeeded with 0 warnings and 0 errors.
- NAT2 focused `ProviderContractInventoryTests`: 50 passed, 0 failed, 0 skipped.
- Current-main reconciliation: fetched `origin/main` `07624709d873dd0aecc934e59bbc45f78b0c844b`,
  merged it cleanly as `d84e391bcbd0fc562e2eacea0e5913160eebba74`, and reran the plan graph with 0
  errors and 0 warnings.
- Review work push: starting remote/PR head `85d85aab1c6e3ef448c792cc9cad7c37639a8ae9`;
  pushed range `85d85aab1c6e3ef448c792cc9cad7c37639a8ae9..6a3f545e6400725b7b962bc4209cc306ab65ce19`;
  local, remote-tracking, and PR work heads all verified at
  `6a3f545e6400725b7b962bc4209cc306ab65ce19`; PR #597 remained open with auto-merge disabled and no
  merge-queue entry.
- Exact-head PR CI run 31953753845 passed at reviewed head
  `85d85aab1c6e3ef448c792cc9cad7c37639a8ae9`: 54 jobs succeeded and five policy-selected jobs were
  skipped; build, local platform packages, all service carves, unit tests, and integration tests were
  green.
- Full implementation and security review covered
  `e861f3642cea14e919d203604a4e9e7d00bcced8..85d85aab1c6e3ef448c792cc9cad7c37639a8ae9`
  (22 commits) and recorded four medium-severity open findings: two native-review findings, one
  correctness finding, and one security finding.
- Current-main review candidate: merged `origin/main` `e861f3642cea14e919d203604a4e9e7d00bcced8`
  cleanly as work head `e8976712839a528a10a0bd039cd21fab68685e2a`; `HEAD..origin/main` is zero.
- Regenerated the published `0.1.0-alpha.0.1009` contract baselines with no diff.
- Payment UnitTests and frozen published-contract fixture build: succeeded with 0 warnings and 0
  errors against platform pin `0.1.0-alpha.0.1025`.
- Focused provider-contract, inventory, contract, mapper, error, and compatibility suite: 216 passed,
  0 failed, 0 skipped.
- Current plan graph and whitespace gates: 0 errors and 0 warnings; `git diff --check` passed.
- Current-main work push: `f5c6218a51d210328201d72e2d0b4cc09f18bb3e..e8976712839a528a10a0bd039cd21fab68685e2a`;
  local, remote-tracking, and PR work heads all verified at `e8976712839a528a10a0bd039cd21fab68685e2a`.

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
- Focused XPlat coverage: authorization-expiry evaluator 100% line/branch; retry evaluator
  97.72% line and 92% branch; transition evaluator 98.34% line and 96.38% branch. The explicit
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

**REVIEW COMPLETE — CHECKPOINT AND EXACT-HEAD CI REQUIRED.** Incremental implementation and security
review is current through `10d07780fd1feaec34c2f7ae765d91ac8291d83e`. The provider-neutral
transition-policy review found NAT5 and NAT6; both are resolved at that head, and the follow-up review
found no remaining issue. The full implementation and security review for
[PR #597](https://github.com/Concertable/concertable/pull/597) covered
`e861f3642cea14e919d203604a4e9e7d00bcced8..85d85aab1c6e3ef448c792cc9cad7c37639a8ae9`
recorded NAT1, NAT2, BUG1, and SEC1. NAT1 is resolved at
`0686b7f52c68ab492ba7683fa5fee895096785da`; NAT2 is resolved at
`19e194c9eaefee2734718a298a127f414f75af6c`; BUG1 is resolved at
`055c6bfd868484e847f907926b5da7b6dea55ff9`; SEC1 is resolved at
`7b7561fa43b57ca004d082ebd207242f0e4499fd`; NAT3 is resolved at
`c4140cd3bb42a8a0f13beb652b7590f98691a63d`; NAT4 is resolved at
`6cc1d59d5281a141f72f9b4f6ddd233ea46da233`; NAT5 and NAT6 are resolved at
`862e4722c484ad44ef08d5017b39395696258b3e`. No finding remains open across the
microservice-isolation, module-boundary, seeding, convention, or security lenses. The branch is not
authorized to merge; the review/ledger checkpoint and exact-head CI are required, and auto-merge
remains disabled.

The planning docs review through PR head `ccb1dd00585b7943a401166f3f8eb3237ed6d628` found no issues across
accuracy, contradiction, document ownership, concision, dangling references, and followable
instructions. Its spent untracked review artifact was deleted after PR #594 merged.

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
