# Repository-per-microservice foundation progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-M1-Platform-Expand`
- Branch: `Refactor/M1-Platform-Expand`
- PR: #942; first stage of the four-branch M1 stack in PRs #942-#945
- Dependency/package gates: PR #633 landed on `origin/main`; Platform Expand and Owner Hosting Sync may now
  enter exact-head validation. AppHost Sync and Platform Contract remain gated on publishing the Owner Hosting
  Auth image, pinning its immutable digest, and qualifying all four standalone Auth client rosters. Package
  inventory and ACL checks require a credential with `read:packages`; private-repository merge-queue rulesets
  remain unavailable on the current GitHub entitlement.
- Last reconciled: 2026-09-07 — PR #633 landing commit `516f4cc25936289744babef3f98b1a297035fbb6`,
  the corrective topology commits `82bf5dbbb` and `bb59d9ba3`, and the fixed M1 repository topology.

## Current state

Checkpoint 6A is terminal: `.github` PRs #1 and #2 merged, all eleven reusable workflows passed from the
public fixture, and shared policy was applied and read back. Checkpoint 6B M1 is active. Existing private
`auth`, `b2b`, `customer`, `payment`, `search`, `infra`, and `config` repositories retain their identities.
The remaining repository boundaries are `platform-dotnet`, `platform-frontend`, and `system`; no repository
creation is part of M1. Four clean M1 branches preserve the Platform Expand, Owner Hosting Sync, AppHost Sync,
and Platform Contract boundaries above landed `origin/main` commit `516f4cc25`; Git owns their current
rewritten heads. Local
review remediation preserves the legacy Auth and B2B hosting contracts through the consumer-migration stage,
retires them only in Platform Contract, keeps the platform SPA surface product-neutral, and moves Auth client
associations into the B2B and Customer owners before system composition consumes their combined roster.

## Next Steps

- Complete landed-base package-clean and composition validation, then establish a current-head review
  watermark for the restacked candidate. Repair any finding on its owning M1 stage without changing the four
  publication boundaries.
- Deliver Platform Expand and Owner Hosting Sync in order through their existing PRs. Follow the Auth image
  publication caused by Owner Hosting Sync to its immutable digest.
- Pin and qualify that Auth image on AppHost Sync, then deliver AppHost Sync and Platform Contract in order.

## Completed work

- Checkpoint 6A closed through `.github` PR #1 (`ab2a127cdba9bacd73411fba8cca2b6a20fc02c0`) and policy repair
  PR #2 (`a2f574a1f4fad3df5e3ec8aa0dd552d717c95728`); fixture acceptance run 33894314188 passed.
- Corrective commits `82bf5dbbb` and `bb59d9ba3` established that the seven active carve repositories retain
  their identities; M1 fixes the remaining topology as `platform-dotnet`, `platform-frontend`, and `system`.
- Extraction-map preflight reports 4,769 tracked paths, 4,769 target claims, 79 unclaimed tracked paths, and
  zero multiply-claimed paths; 6C is not ready.
- The complete four-stage M1 chain was rebased without conflicts onto PR #633 landing commit `516f4cc25`.
  Range-diff verification preserves every staged package expansion, owner migration, composition migration,
  and contract-removal patch and boundary.
- Platform frontend service URL propagation now resolves both HTTPS and HTTP Aspire endpoints and both hyphenated
  and normalized resource names, so the B2B mobile API tunnel is emitted correctly.
- Review remediation added exact Auth SPA replacement and unknown-client fail-closed coverage, retained legacy
  hosting compatibility until the final contract stage, made resolver assertions portable across Windows and
  Linux, completed the exact platform extraction table, added owner Auth-roster assertions to the B2B,
  Customer, and system graphs, and added deterministic coverage that exercises every owner frontend path through
  the production B2B and Customer hosting extensions in both extracted-only and monorepo-preferred layouts.

## Verification

- Ancestry from landed `origin/main` commit `516f4cc25` through the complete M1 stack is verified after the
  final local restack; landed-base package and composition revalidation is in progress.
- Package inventory and local platform preparation pass with 57 packages. Auth Hosting, B2B Hosting, Auth
  AppHost, and B2B AppHost build successfully against the locally prepared platform packages; the compatibility
  form of Auth Hosting and B2B Hosting also builds at the AppHost Sync boundary.
- `Concertable.AppHost.Shared` passes 16/16 tests. Auth architecture passes 9/9 tests. B2B package-mode
  architecture passes 35/35 against the current Payment.Hosting producer placed at #633's pinned package slot;
  Search architecture passes 4/4 and Payment architecture passes 13/13. B2B and Customer Hosting also build
  independently against the locally prepared platform packages. Customer's current Hosting and architecture-test
  assemblies compile in isolation and the two extracted/monorepo frontend-layout cases pass 2/2.
- The former #633 Customer compile blocker and Payment.Hosting package slot are now eligible for exact landed-base
  revalidation; their previous blocked result is not carried forward as current evidence.
- No local E2E suite was run; E2E remains a remote merge-queue diagnostic gate.

## Reviews

The local work order is `reviews/Refactor-M1-Platform-Contract.md`. Its last immutable full pass requested one
delivery-gated change: publish and pin the Owner Hosting Auth image before AppHost Sync. All other findings are
repaired on their owning stages. The landed-base candidate requires a new frozen review watermark after current
package and composition validation completes.

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
