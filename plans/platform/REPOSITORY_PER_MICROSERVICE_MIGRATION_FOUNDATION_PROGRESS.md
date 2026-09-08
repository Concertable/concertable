# Repository-per-microservice foundation progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-M1-Platform-Expand`
- Branch: `Refactor/M1-Platform-Expand`
- PR: #942; first stage of the four-branch M1 stack in PRs #942-#945
- Dependency/package gates: PR #633 and M3 PR #948 landed on `origin/main`; Platform Expand exact-head run
  `34178610726` passed all 87 jobs. Merge-group run `34179430318` then exposed a reset race after the
  cancellation/refund scenario: Payment's outbox dispatcher still owned an active row while the following
  scenario reset the Payment database, and B2B was also retrying an unregistered `BookingConfirmedEvent`.
  The owning Platform Expand repair now waits for the complete cancellation pipeline and Payment outbox
  quiescence, and registers the missing B2B loopback topology, before the four stages are restacked and
  republished. Owner Hosting Sync may then enter exact-head validation.
  AppHost Sync and Platform Contract remain gated on publishing the Owner Hosting
  Auth image, pinning its immutable digest, and qualifying all four standalone Auth client rosters. Package
  inventory and ACL checks require a credential with `read:packages`; private-repository merge-queue rulesets
  remain unavailable on the current GitHub entitlement.
- Last reconciled: 2026-09-07 — current `origin/main` commit
  `12efedd68da08d92b08990a30e76dab5546b5ed4`, which includes PR #633, B2B producer PR #949, and M3 PR #948;
  the corrective topology commits `82bf5dbbb` and `bb59d9ba3`; and the fixed M1 repository topology.

## Current state

Checkpoint 6A is terminal: `.github` PRs #1 and #2 merged, all eleven reusable workflows passed from the
public fixture, and shared policy was applied and read back. Checkpoint 6B M1 is active. Existing private
`auth`, `b2b`, `customer`, `payment`, `search`, `infra`, and `config` repositories retain their identities.
The remaining repository boundaries are `platform-dotnet`, `platform-frontend`, and `system`; no repository
creation is part of M1. Four clean M1 branches preserve the Platform Expand, Owner Hosting Sync, AppHost Sync,
and Platform Contract boundaries above landed `origin/main` commit `12efedd68`; Git owns their current
rewritten heads. Local
review remediation preserves the legacy Auth and B2B hosting contracts through the consumer-migration stage,
retires them only in Platform Contract, keeps the platform SPA surface product-neutral, and moves Auth client
associations into the B2B and Customer owners before system composition consumes their combined roster.
Platform Expand owns both merge-group repairs: it accepts Stripe's authoritative `requires_capture` state
when optional `capture_before` metadata is absent, and it makes cancellation scenarios wait for Payment
refund completion, Booking and Concert cancellation, and Payment outbox quiescence before the next database
reset. B2B now declares its staged-and-consumed `BookingConfirmedEvent` in both runtime and AppHost topology.

## Next Steps

- Commit and publish the Platform Expand merge-group repair, restack the three dependent M1 stages without
  changing their publication boundaries, and re-enter exact-head validation and the merge queue.
- Deliver Platform Expand and Owner Hosting Sync in order through their existing PRs. Follow the Auth image
  publication caused by Owner Hosting Sync to its immutable digest.
- Pin and qualify that Auth image on AppHost Sync, then deliver AppHost Sync and Platform Contract in order.

## Completed work

- Checkpoint 6A closed through `.github` PR #1 (`ab2a127cdba9bacd73411fba8cca2b6a20fc02c0`) and policy repair
  PR #2 (`a2f574a1f4fad3df5e3ec8aa0dd552d717c95728`); fixture acceptance run 33894314188 passed.
- Corrective commits `82bf5dbbb` and `bb59d9ba3` established that the seven active carve repositories retain
  their identities; M1 fixes the remaining topology as `platform-dotnet`, `platform-frontend`, and `system`.
- Extraction-map preflight reports 4,793 tracked paths, 4,793 target claims, 82 unclaimed tracked paths, and
  zero multiply-claimed paths; 6C is not ready.
- Platform Expand was rebased onto current `origin/main` `12efedd68`. The shared inventory and plan conflicts
  preserved M3's landed `app/build-config` ownership and `platform-frontend` identity while restoring M1 as
  the active foundation ledger; no runtime-code conflict occurred.
- M3 landed through PR #948 at `12efedd68`; its product-neutral `@concertable/build-config` ownership remains
  independent of M1's .NET hosting stack and no repository boundary changed.
- Platform frontend service URL propagation now resolves both HTTPS and HTTP Aspire endpoints and both hyphenated
  and normalized resource names, so the B2B mobile API tunnel is emitted correctly.
- Review remediation added exact Auth SPA replacement and unknown-client fail-closed coverage, retained legacy
  hosting compatibility until the final contract stage, made resolver assertions portable across Windows and
  Linux, completed the exact platform extraction table, added owner Auth-roster assertions to the B2B,
  Customer, and system graphs, and added deterministic coverage that exercises every owner frontend path through
  the production B2B and Customer hosting extensions in both extracted-only and monorepo-preferred layouts.

## Verification

- Ancestry from landed `origin/main` commit `12efedd68` through Platform Expand is verified; the three
  dependent stages are being restacked in order before combined-base CI is treated as current evidence.
- Pre-M3 exact-head PR run `34166392329` passed all 81 executed jobs, including package preparation, generated
  inventory, solution/image builds, five service carves, architecture, unit, and integration matrices.
- Package inventory and local platform preparation pass with 58 packages. Auth Hosting, B2B Hosting, Auth
  AppHost, and B2B AppHost build successfully against the locally prepared platform packages; the compatibility
  form of Auth Hosting and B2B Hosting also builds at the AppHost Sync boundary.
- `Concertable.AppHost.Shared` passes 16/16 tests. Auth architecture passes 9/9 tests. B2B package-mode
  architecture passes 35/35 against the current Payment.Hosting producer placed at #633's pinned package slot;
  Search architecture passes 4/4 and Payment architecture passes 13/13. B2B and Customer Hosting also build
  independently against the locally prepared platform packages. Customer's current Hosting and architecture-test
  assemblies compile in isolation and the two extracted/monorepo frontend-layout cases pass 2/2.
- The post-repair package-clean gates pass `Concertable.AppHost.Shared` 18/18,
  `Concertable.Payment.E2ETests.Server` 7/7, and B2B architecture 32/32. The B2B UI project is rebuilt
  package-clean before publication; the exact browser scenario remains a remote gate because this
  workstation's Docker endpoint is unavailable.
- The former #633 Customer compile blocker and Payment.Hosting package slot are now eligible for exact landed-base
  revalidation; their previous blocked result is not carried forward as current evidence.
- The targeted local 3DS UI E2E suite passes 8/8. This includes the formerly failing venue-manager flat-fee
  successful-challenge scenario. Its deterministic cause was a missing `ApplicationAcceptedEvent` B2B
  subscription in `B2BTopology`; the repair provisions the exact
  `concertable-b2b-application-accepted` subscription and locks it down with composition coverage. The
  scenario URL wait is 30 seconds so a genuine failure terminates sooner. Remote merge-group E2E remains the
  authoritative delivery gate.
- Platform Expand merge-group commit `f0ad78ad1` proved the B2B UI suite green at 32/32, including every 3DS
  path. Customer UI passed 6/7; the sole failure trace showed the card-number input remained exactly empty
  because the generic successful-test-card step selected the saved-card path even though the isolated Stripe
  fixture creates fresh customers without attached cards. Platform Expand now routes that step through the
  explicit successful new-card path; the same merge-group artifact already proves that path completes Stripe
  confirmation, ticket creation, ticket listing, and QR display. A local post-fix run authorised all three
  card payments but its downstream confirmation assertions encountered workstation Service Bus and SQL health
  degradation, so the exact-head merge-group Customer run remains the acceptance evidence for this repair.
- The subsequent exact merge-group run `34174839388` at `be47d6beec91744955930e5fd75b61c5770e6281`
  passed 31/32 B2B UI scenarios. Its sole failure completed Stripe's 3DS challenge and returned 204 from B2B
  acceptance, then Payment rejected the `requires_capture` observation and exhausted all three deliveries of
  `concertable.payment.capture-escrow.v1` as `PaymentProviderUnavailableException`; no capture event or concert
  draft could follow. The owning repair accepts an authorized observation without optional `capture_before`,
  normalizes Stripe.NET's missing-timestamp Unix-epoch sentinel back to absence, and retains fail-closed expiry
  evaluation when no provider deadline exists. Payment unit tests pass 552/552, including the exact
  `RequiresAction` to `Authorized` regression and adapter normalization. The focused resolver integration test
  compiles locally; its execution awaits CI because this workstation's Docker endpoint is unavailable.
- Exact-head run `34178610726` at `2373f68545919353a364aa2f8e75bc89c483a073` passed all 87 jobs, including
  browser E2E. Its merge-group successor `34179430318` passed 31/32 B2B UI scenarios; the final scenario did
  not start because its `BeforeScenario` Payment reset received HTTP 500. Diagnostics identify SQL deadlock
  victim 1205 in Respawn while Payment's outbox dispatcher was completing the preceding refund, plus repeated
  B2B `BookingConfirmedEvent` registry failures. The repair establishes a causal boundary by verifying the
  Stripe refund, both B2B terminal states, and zero active Payment outbox rows before returning from the
  cancellation step, and adds the missing B2B publish/subscribe registration with composition coverage.

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
