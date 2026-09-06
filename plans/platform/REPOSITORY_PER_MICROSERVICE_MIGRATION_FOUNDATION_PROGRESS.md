# Repository-per-microservice foundation progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-M1-Platform-Expand`
- Branch: `Refactor/M1-Platform-Expand`
- PR: not opened; first stage of the four-branch M1 stack above monorepo PR #633
- Dependency/package gates: M1 delivery is gated on PR #633 landing; package inventory and ACLs require a
  credential with `read:packages`; private-repository merge-queue rulesets are unavailable on the current
  GitHub entitlement.
- Last reconciled: 2026-09-06 — corrective topology commits `82bf5dbbb` and `bb59d9ba3`, the fixed M1
  repository topology, and PR #633 head `3f89818c7c91b5cf9d658fbe7e8460163de06d78`.

## Current state

Checkpoint 6A is terminal: `.github` PRs #1 and #2 merged, all eleven reusable workflows passed from the
public fixture, and shared policy was applied and read back. Checkpoint 6B M1 is active. Existing private
`auth`, `b2b`, `customer`, `payment`, `search`, `infra`, and `config` repositories retain their identities.
The remaining repository boundaries are `platform-dotnet`, `platform-frontend`, and `system`; no repository
creation is part of M1. Four clean M1 branches preserve the Platform Expand, Owner Hosting Sync, AppHost Sync,
and Platform Contract boundaries above PR #633 head `3f89818c7`; Git owns their current rewritten heads. Local
review remediation preserves the legacy Auth and B2B hosting contracts through the consumer-migration stage,
retires them only in Platform Contract, keeps the platform SPA surface product-neutral, and moves Auth client
associations into the B2B and Customer owners before system composition consumes their combined roster.

## Next Steps

- Keep the candidate frozen at the immutable head recorded by the local review work order. Any content change
  requires a new artifact and watermark; repair any finding on its owning M1 stage without changing the four
  publication boundaries.
- When PR #633 lands, restack the four stages onto the exact landed `origin/main` if necessary. Re-run the
  Customer and system composition suites that are currently blocked by #633, plus the package-clean gates.
- Deliver Platform Expand, Owner Hosting Sync, AppHost Sync, and Platform Contract in that order only after the
  landed-base validation and review are terminal.

## Completed work

- Checkpoint 6A closed through `.github` PR #1 (`ab2a127cdba9bacd73411fba8cca2b6a20fc02c0`) and policy repair
  PR #2 (`a2f574a1f4fad3df5e3ec8aa0dd552d717c95728`); fixture acceptance run 33894314188 passed.
- Corrective commits `82bf5dbbb` and `bb59d9ba3` established that the seven active carve repositories retain
  their identities; M1 fixes the remaining topology as `platform-dotnet`, `platform-frontend`, and `system`.
- Extraction-map preflight reports 4,766 tracked paths, 4,766 target claims, 79 unclaimed tracked paths, and
  zero multiply-claimed paths; 6C is not ready.
- The complete four-stage M1 chain was rebased without conflicts onto PR #633 head `3f89818c7` and retains its
  staged package expansion, owner migration, composition migration, and contract-removal boundaries.
- Platform frontend service URL propagation now resolves both HTTPS and HTTP Aspire endpoints and both hyphenated
  and normalized resource names, so the B2B mobile API tunnel is emitted correctly.
- Review remediation added exact Auth SPA replacement and unknown-client fail-closed coverage, retained legacy
  hosting compatibility until the final contract stage, made resolver assertions portable across Windows and
  Linux, completed the exact platform extraction table, and added owner Auth-roster assertions to the B2B,
  Customer, and system graphs.

## Verification

- Ancestry from PR #633 head `3f89818c7` through the complete M1 stack is verified after each local restack.
- Package inventory and local platform preparation pass with 57 packages. Auth Hosting, B2B Hosting, Auth
  AppHost, and B2B AppHost build successfully against the locally prepared platform packages; the compatibility
  form of Auth Hosting and B2B Hosting also builds at the AppHost Sync boundary.
- `Concertable.AppHost.Shared` passes 16/16 tests. Auth architecture passes 9/9 tests. B2B package-mode
  architecture passes 33/33 against the current Payment.Hosting producer placed at #633's pinned package slot;
  Search architecture passes 4/4 and Payment architecture passes 13/13. B2B and Customer Hosting also build
  independently against the locally prepared platform packages.
- Customer and umbrella system execution remain blocked by #633's Customer compile errors (`PaymentOutcome` is
  sealed and `CheckoutSession` is missing). Exact immutable B2B package qualification remains a delivery gate
  until #633 publishes or advances its pinned Payment.Hosting producer: the currently published pinned binary
  expects the previous `AsbTopology.WithService` contract, while the current producer passes locally at that slot.
- No local E2E suite was run; E2E remains a remote merge-queue diagnostic gate.

## Reviews

The local work order is `reviews/Refactor-M1-Platform-Contract.md`. Its immutable full pass requested changes;
all four newly reported findings are repaired on their owning stages. B2B, Search, and Payment composition
verification is complete, while the Customer and system executions retain the observable #633 resume condition
above. Because remediation restacked the candidate, the next full pass establishes the frozen review watermark
rather than treating the rewritten history as an incremental review.

## Decisions, discoveries, blockers, and deviations

- Existing service, `infra`, and `config` repository IDs and active owner ledgers override historical labels;
  they are not renamed or replaced.
- Shared packages have two repository owners: `platform-dotnet` and `platform-frontend`. The frontend owner
  contains general shared web/mobile code; web and mobile remain package tiers, not repositories.
- `system` is a separate container-composition and black-box qualification boundary.
- M1 creates no repositories and makes no further topology decision.
- The current GitHub entitlement returns 403 for private-repository ruleset, merge-queue, and branch-protection
  reads. There is no technical private-main enforcement substitute on this entitlement: targets remain private
  and non-canonical behind an administrator-operated CI/PR gate until an entitlement upgrade is verified.
