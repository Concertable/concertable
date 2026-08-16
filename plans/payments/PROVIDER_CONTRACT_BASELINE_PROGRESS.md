# Provider contract baseline progress

- Plan: `plans/payments/PROVIDER_CONTRACT_BASELINE_PLAN.md`
- Roadmap: `plans/payments/STRIPE_RELIABILITY_ROADMAP.md`
- Roadmap item: `payments/provider-contract-baseline`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\payments_provider-contract-baseline`
- Branch: `Feature/payments_provider-contract-baseline`
- PR: #597 — https://github.com/Concertable/concertable/pull/597 — draft at verified local/remote/PR head `59b8e266d3630c9c98390827d0a2a820ec71d0d2`; branch is current with `origin/main` `35b114d4a` through merge commit `133b9386d`; planning PR #594 merged as `3c8a2c5a847d0f9702884949ed57850c6e494c47`
- Review readiness: **NOT READY FOR REVIEW** — Phases 3 and 4 remain; PR #597 stays draft until the final implementation candidate is locally verified and exact-head CI is green
- Dependency/package gates: Phases 1 and 2 are complete; the production/live-mode Stripe account has no webhook endpoint, so future deployment is locked to `2025-01-27.acacia` and must create the endpoint at the actual Payment Web URL while installing its signing secret; PR #552 merged as `33f07c47a497586324edacdcfc10321a9d3f02ee`; compatibility remains anchored to published `0.1.0-alpha.0.1009`; platform-sync PR #601 merged and the current platform pin is `0.1.0-alpha.0.1017`
- Last reconciled: 2026-08-16 against `origin/main` `35b114d4a`, merged PRs #552/#601, draft PR #597, the source roadmap, current repository entry points, and live/test Stripe API evidence

## Current state

Phases 1 and 2 are complete, committed, and pushed. `api/Concertable.Payment/PROVIDER_CONTRACT.md` now owns
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
contracts are present after merging current `origin/main`. Local HEAD, the remote branch, and draft
PR #597 all resolve to `59b8e266d3630c9c98390827d0a2a820ec71d0d2`; exact-head CI run
31942952029 passed all 58 jobs. The branch is current with `origin/main` `35b114d4a` through merge
commit `133b9386d`; the historical `Refactor/GroupStripeWebhookHandling` branch is superseded evidence
only.

Phase 2 adds the provider-neutral operation identity, session kind, normalized state, terminal/retry
disposition, safe-failure vocabulary, client descriptor/snapshot records, and matching protobuf
messages without adding an RPC. `PaymentOperationStateChangedV1` has stable message type
`concertable.payment.payment-operation-state-changed.v1`. Reunion Result mapping remains supported on
.NET 10 through the closed Dunet `PaymentOperationError`; the planned .NET 11 native-union migration
follows the complete Stripe/provider refactor rather than interrupting it.

## Next Steps

Implement and verify Phase 3 only. Add the pure transition specification that normalizes the complete
Stripe.net `47.3.0` PaymentIntent, SetupIntent, and Refund status vocabulary at the
`2025-01-27.acacia` baseline; encode every allowed and rejected state edge, duplicate/stale
observation, terminal protection, retry/revision rule, and capture expiry; prove the rules
exhaustively without wiring runtime webhooks, persistence, reconciliation, or consumers. Update this
ledger and the plan checklist, commit and push the coherent checkpoint, and let draft-PR CI validate
the exact remote head. Do not start Phase 4 in the same turn.

## Completed work

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

**NOT READY FOR REVIEW.** [Draft PR #597](https://github.com/Concertable/concertable/pull/597) still
requires Phases 3 and 4. Once the final implementation candidate and exact-head CI are green,
`## Next Steps` must route through `/review`; that review-ready checkpoint still emits the standard
plan continuation pointer because the plan lifecycle is not terminal.

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
