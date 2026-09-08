# B2B commercial execution progress

- Plan: `plans/commercial/COMMERCIAL_EXECUTION_PLAN.md`
- Roadmap: `plans/commercial/COMMERCIAL_ROADMAP.md`
- Roadmap item: `commercial/execution`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/launch_booking-entry-direct-offers`
- Branch: `Docs/launch_booking-entry-direct-offers`
- PR: not opened
- Dependency/package gates: planning-only; external compatibility requirements are specified in the plan, not claimed delivered
- Last reconciled: 2026-09-08 against Git/worktree identities, monorepo main 3826320d, docs main 99ad353b and the local organiser research at 5ac4048

Metadata paths are relative to the B2B source root, currently api/Concertable.B2B within the recorded
Git worktree. This plan/ledger moves with B2B when the extraction owner qualifies its source cutover.

## Current state

The architecture and coordinated PR slices are a discussion-refined review draft. No application implementation,
database migration or deployment is authorised or performed. Recommendations and unresolved policies
are explicitly separated from established decisions in the plan.

The latest update makes operation composition, typed definition bindings, concrete factory matching and
the existing-code migration explicit. It incorporates the five research arrangements as qualification
fixtures without turning every researched capability or unresolved policy into approved initial scope.

Tommy requested B2B ownership. The prior direct-offers plan and ledger are replaced, not retained as
parallel instructions. The old launch item now routes to this B2B owner. Historical text remains in Git.

The main checkout and other active task worktrees were not modified. The older all-services PostgreSQL
draft remains with its separate owner; this plan owns only B2B scope and the required consumption gates.

## Next Steps

Paused: Tommy - review the refined design in sections 3-4 and the research scope in section 9; resolve the required section 14 policies and continue section 12's contract-design gate before B4-B7 implementation. Application implementation or migration requires a separate explicit authorisation.

On feedback, update this plan and the existing product-decision owners in Concertable/docs as appropriate.
Do not infer approval of every recommendation from permission to write/read the draft.

## Completed work

- Investigated PR633 and later source, current docs PR11, provider coupling and existing plan owners.
- Replaced the superseded direct-offers draft with the B2B-owned architecture, concrete entity model,
  typed dispatch explanation, three diagrams, B2B migration inventory and proposed delivery slices.
- Removed the disproven synthetic-entry design and the unsupported blanket enum-widening prerequisite.
- Retained the four lifecycle sections with constrained mix-and-match inside operations; separated
  definitions, step families, capability interfaces, implementations and durable execution instances.
- Added DealEntity/DealDto and typed binding sketches, concrete same-case/multiple-implementation factory
  registration and matching, Application acceptance flow, operation ownership and current-code migration maps.
- Incorporated research R1-R5, compatibility counterexamples and slice-specific verification; removed
  the unsupported schedule-selection acceptance example from the dispatch diagram.

## Verification

- Source authority refreshed through 3826320d; extracted B2B main remains older than PR633. Docs main
  includes PR11; organiser research 5ac4048 is local/unpublished evidence, not approved product policy.
- This update: B2B and root plan graphs pass with zero errors/warnings. Workflow v2 repository provider
  validates the B2B artifact paths, Git worktree/branch identity and explicit design-review pause.
- All 25 local file/anchor links and three SVG XML documents validate. The edited dispatch diagram was
  rendered and visually checked; the other two diagrams are unchanged from their prior visual checks.
- Scoped diff whitespace check passes. Only the existing plan, ledger and dispatch SVG are changed.
- The prior broad B2B docs-reachability check found four baseline errors at 85ed6353:
  missing CLAUDE siblings in the two Dashboard test directories; missing AGENTS in E2EAdmin integration
  and KeyedStrategies unit test directories. These are not introduced by the plan; the owning test-guidance
  work/source reconciliation must clear them before docs publication. Do not edit another task's checkout.
- No application build, database test, migration or E2E run was performed for this planning-only change.

## Reviews

This update received a consistency/self-review and diagram inspection. No independent implementation-
readiness review or C# compilation is claimed. This remains a design draft for Tommy's review.

## Decisions, discoveries, blockers, and deviations

- Keyed unions are retained for genuine input/result divergence; matching belongs to the owning workflow.
  A named calculation resolver owns pure calculation dispatch. Data-only templates do not require new
  enum members or union arms; multiple implementations already fit one interface case.
- Apply/Accept are operations, not a requirement for two union parents. Current Application uses
  IApplyStep and ICommitmentReferenceStep, not IAcceptStep. Commitment reference resolution remains a
  strategy unless its contract genuinely diverges; a reference is not proof of payment readiness.
- Configuration contains the actual selected commercial design; a template supplies reusable defaults.
  Version meanings, template provenance, issued proposals and accepted snapshots are separate concerns.
- Stored compatibility needs structural/semantic and live checks as well as typed code. Final admitted
  schemas, signatures, bindings, output consumers and recovery contracts remain a named design gate;
  do not delegate those decisions to an implementation agent or treat snippets as compiled evidence.
- Source ownership must be refreshed again before delivery; a B2B README or extracted checkout alone
  does not establish that its source includes the merged architecture.
- The main checkout owns unrelated work. Do not switch/reset/stash/edit it to continue this plan.
- Existing data retention is unverified. Preserve data until the named environments and required
  treatment are established; InitialCreate regeneration does not authorise database recreation.
- Finance: external ticket revenue is evidence, not funding; collection, available funds, recipient
  transfer and bank payout remain distinct.
- Shared DataAccess/Messaging compatibility, commission, tenant configuration, lifecycle seals and
  operation recovery keep their existing owners. Qualify artifacts before consumer delivery.
- The old PostgreSQL owner must reconcile its B2B/residual all-service scope after design agreement.
  Do not copy its ledger here or implement its all-service sequence.
