# B2B commercial execution progress

- Plan: `plans/commercial/COMMERCIAL_EXECUTION_PLAN.md`
- Roadmap: `plans/commercial/COMMERCIAL_ROADMAP.md`
- Roadmap item: `commercial/execution`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/launch_booking-entry-direct-offers`
- Branch: `Docs/launch_booking-entry-direct-offers`
- PR: not opened
- Dependency/package gates: planning-only; external compatibility requirements are specified in the plan, not claimed delivered
- Last reconciled: 2026-09-08 against Git/worktree identities, monorepo main ef8d505f, docs main 99ad353b and the local organiser research at 5ac4048

Metadata paths are relative to the B2B source root, currently api/Concertable.B2B within the recorded
Git worktree. This plan/ledger moves with B2B when the extraction owner qualifies its source cutover.

## Current state

The architecture and coordinated PR slices are a discussion-refined review draft. No application implementation,
database migration or deployment is authorised or performed. Recommendations and unresolved policies
are explicitly separated from established decisions in the plan.

Section 16 provides the code-level approval walkthrough: concrete Apply/Send/Accept and
commitment-start methods, entity/request/read shapes, separate invitation API/service/workflow, collaborator
contracts and atomic Booking convergence. It does not declare the whole programme implementation-ready;
B6/B7 financial-operation definitions still need equally concrete review before enabling those capabilities.

The latest clarification fixes the proposed step/factory names and distinguishes reference lookup from
executable steps in section 4. AcceptStep is the future union name if acceptance earns distinct contracts;
section 16.9 now consistently uses CommitmentStep / ICommitmentStepFactory. No runtime code changed.

Tommy requested B2B ownership. The prior direct-offers plan and ledger are replaced, not retained as
parallel instructions. The old launch item now routes to this B2B owner. Historical text remains in Git.

The main checkout and other active task worktrees were not modified. The older all-services PostgreSQL
draft remains with its separate owner; this plan owns only B2B scope and the required consumption gates.

## Next Steps

Paused: Tommy - review sections 3-4 and 16's proposed structure and D17-D18. Before approval, reconcile the remaining collaborator/entity names and null-forgiving snippets with the discussion; resolve policies needed by each admitted slice and complete B6/B7's operation contracts before their implementation. Application implementation or migration requires separate explicit authorisation.

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
- Specified entry-local drafts, issued proposal/consent storage, shared accepted Booking input, explicit
  Apply/Send/both Accept methods, payer-only commitment start and the full route/use-case inventory.
- Replaced the tentative acceptance reference-strategy sketch with recorded commitment references;
  demonstrated the genuine method-setup/amount-authorisation union and two implementations in one case.
- Identified the existing normal-return transaction semantics; required failed-Result rollback, fresh-scope
  recovery, post-flush ETags and one Booking creation path. Added acceptance/counter/commitment race proofs.

## Verification

- Source authority refreshed through ef8d505f; entry/Booking/Deal runtime files inspected at 3826320d are
  unchanged by that refresh. Extracted B2B main remains older than PR633. Docs main
  includes PR11; organiser research 5ac4048 is local/unpublished evidence, not approved product policy.
- This commit: B2B and root plan graphs pass with zero errors/warnings. Workflow v2 validates the B2B
  artifact owner, worktree/branch and design-review pause. The commercial plan tree has no remaining
  superseded commitment union/factory names; source-history reference-step names are deliberately retained.
- At d3faa0f0e, all 27 local links/anchors, three SVG XML documents and delimiters for 18 C# excerpts validate;
  delimiter checks are not compilation. Scoped diff whitespace passes. Only the existing plan and
  ledger changed; diagrams are unchanged from their prior rendered/visual checks.
- The prior broad B2B docs-reachability check found four baseline errors at 85ed6353:
  missing CLAUDE siblings in the two Dashboard test directories; missing AGENTS in E2EAdmin integration
  and KeyedStrategies unit test directories. These are not introduced by the plan; the owning test-guidance
  work/source reconciliation must clear them before docs publication. Do not edit another task's checkout.
- No application build, database test, migration or E2E run was performed for this planning-only change.

## Reviews

The naming clarification received a scoped consistency self-review against the actual reference strategy,
keyed builder and current first-party C# union documentation. No independent implementation-readiness
review, application test or C# compilation is claimed. This remains a design draft for Tommy.

## Decisions, discoveries, blockers, and deviations

- Keyed unions are retained for genuine input/result divergence; matching belongs to the owning workflow.
  A named calculation resolver owns pure calculation dispatch. Data-only templates do not require new
  enum members or union arms; multiple implementations already fit one interface case.
- Apply/Accept are operations, not a requirement for two union parents. Current Application uses
  IApplyStep and ICommitmentReferenceStep, not IAcceptStep. The target stores commitment references and
  removes the redundant acceptance resolver after conversion; starting a commitment is the real union
  example. A reference alone is not proof of payment readiness.
- Future acceptance union naming and native-interface matching are specified in section 4; distinct
  implementations sharing one contract do not require separate cases. Native union adoption is not an
  SDK upgrade authorised by this clarification. Keep the separate proposed acceptance record; questions
  about its purpose did not withdraw that design choice.
- Configuration contains the actual selected commercial design; a template supplies reusable defaults.
  Version meanings, template provenance, issued proposals and accepted snapshots are separate concerns.
- Stored compatibility needs structural/semantic and live checks as well as typed code. Section 16's
  entry contracts await approval; richer B6/B7 schemas and policies remain a named design gate. Do not
  delegate missing commercial decisions to an implementation agent or treat snippets as compiled evidence.
- D17-D18 are recommendations, not agreed product policy: private drafts, two initial legal principals
  with representation, separate invitation edge/workflow, and result-aware atomic acceptance.
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
