# B2B commercial execution progress

- Plan: `plans/commercial/COMMERCIAL_EXECUTION_PLAN.md`
- Roadmap: `plans/commercial/COMMERCIAL_ROADMAP.md`
- Roadmap item: `commercial/execution`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/launch_booking-entry-direct-offers`
- Branch: `Docs/launch_booking-entry-direct-offers`
- PR: not opened
- Dependency/package gates: planning-only; external compatibility requirements are specified in the plan, not claimed delivered
- Last reconciled: 2026-09-08 against Git/worktree identities, docs main 99ad353b and monorepo main f72431d7

Metadata paths are relative to the B2B source root, currently api/Concertable.B2B within the recorded
Git worktree. This plan/ledger moves with B2B when the extraction owner qualifies its source cutover.

## Current state

The architecture and coordinated PR slices are a proposed review draft. No application implementation,
database migration or deployment is authorised or performed. Recommendations and unresolved policies
are explicitly separated from established decisions in the plan.

Tommy requested B2B ownership. The prior direct-offers plan and ledger are replaced, not retained as
parallel instructions. The old launch item now routes to this B2B owner. Historical text remains in Git.

The main checkout and other active task worktrees were not modified. The older all-services PostgreSQL
draft remains with its separate owner; this plan owns only B2B scope and the required consumption gates.

## Next Steps

Paused: Tommy - review the B2B architecture draft and resolve the policy/data questions in section 14; resume design refinement from his feedback. Application implementation or migration requires a separate explicit authorisation.

On feedback, update this plan and the existing product-decision owners in Concertable/docs as appropriate.
Do not infer approval of every recommendation from permission to write/read the draft.

## Completed work

- Investigated PR633 and later source, current docs PR11, provider coupling and existing plan owners.
- Replaced the superseded direct-offers draft with the B2B-owned architecture, concrete entity model,
  typed dispatch explanation, three diagrams, B2B migration inventory and proposed delivery slices.
- Removed the disproven synthetic-entry design and the unsupported blanket enum-widening prerequisite.

## Verification

- Source authority refreshed through f72431d7; extracted B2B main remains older than PR633.
- B2B and monorepo plan graphs: zero errors/warnings. Workflow v2 repository provider validates the
  B2B artifact paths, Git worktree/branch identity and review pause.
- All 21 local file/anchor links and three static SVGs validate; all diagrams were rendered and inspected.
- Scoped diff whitespace check passes. No application/source/migration files are changed.
- Broader B2B docs reachability retains four baseline errors, verified against unchanged HEAD 85ed6353:
  missing CLAUDE siblings in the two Dashboard test directories; missing AGENTS in E2EAdmin integration
  and KeyedStrategies unit test directories. These are not introduced by the plan; the owning test-guidance
  work/source reconciliation must clear them before docs publication. Do not edit another task's checkout.
- No application build, database test, migration or E2E run was performed for this planning-only change.

## Reviews

No independent implementation-readiness review recorded. This is a design draft for Tommy's review.

## Decisions, discoveries, blockers, and deviations

- Keyed unions are retained for genuine input/result divergence; matching belongs to the owning workflow.
  Data-only templates do not require new enum members or union arms.
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
