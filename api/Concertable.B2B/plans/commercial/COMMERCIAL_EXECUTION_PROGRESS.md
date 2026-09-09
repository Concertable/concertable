# B2B commercial execution progress

- Plan: `plans/commercial/COMMERCIAL_EXECUTION_PLAN.md`
- Roadmap: `plans/commercial/COMMERCIAL_ROADMAP.md`
- Roadmap item: `commercial/execution`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/launch_booking-entry-direct-offers`
- Branch: `Docs/launch_booking-entry-direct-offers`
- PR: not opened
- Dependency/package gates: planning-only; external compatibility requirements are specified in the plan, not claimed delivered
- Last reconciled: 2026-09-09 against Git/worktree identities, monorepo main 5b367c5a, extracted B2B main fded052c, docs main 99ad353b and the local organiser research at 5ac4048

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

The latest agreed corrections are recorded in D19-D21: Dunet-authored PaymentMethodStep with
PaymentMethod/PaymentAuthorisation cases, typed `BehaviourKey<TBehaviour>` enum selection and an omitted
version default of 1, and shared generic strategy/union factories only. Save/Verify share
IPaymentMethodSetupStep; amount authorisation uses IPaymentAuthorisationStep. This real preparation union
does not convert today's uniform reference resolver into a union. AcceptStep remains a future name only
if acceptance earns distinct callable contracts. No runtime code, package pin or SDK changed.

Sections 4/16 now use the composite key directly, replacing the rejected per-family factory and
persisted-key-to-runtime-enum adapter. Shared builders gain explicit supported-key coverage while retaining
existing enum-consumer, overlap and lifetime checks. Omission always means 1, never latest; storage pins the
effective version before issuance/hashing. Initial leaves need no V1 namespace or repeated version argument.
Incompatible future semantics may require concurrent leaves; no speculative V2 implementation is approved.
D22 records the accepted support direction and proposed retirement proof: outstanding work, not historical
rows alone, determines executor retention. Specific deadlines, retained-data and replay policies remain open.

Tommy requested B2B ownership. The prior direct-offers plan and ledger are replaced, not retained as
parallel instructions. The old launch item now routes to this B2B owner. Historical text remains in Git.

The main checkout and other active task worktrees were not modified. The older all-services PostgreSQL
draft remains with its separate owner; this plan owns only B2B scope and the required consumption gates.

## Next Steps

Paused: Tommy - review the reconciled sections 4/16 against agreed D19-D21, then the remaining D17-D18 entry contracts and D22 support-policy details. Before overall approval, reconcile remaining collaborator/entity names and older null-forgiving snippets, resolve admitted-slice policies and complete B6/B7's operation contracts. Application implementation or migration requires separate explicit authorisation.

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
- Aligned all sketched sum-type declarations with Dunet, retained explicit interface-valued case construction
  and added exact semantics selection/retirement requirements to B4/B5 and the entry verification matrix.
- Replaced bespoke family factories/string-key translation with shared direct-key factories, typed enums
  and the stable version-1 default; reconciled configuration, registration, workflow, decision and slice
  examples plus the dispatch diagram. Distinguished the new payment-preparation union from the old reference
  strategy, and initial unversioned class names from genuinely incompatible future V1/V2 leaves.

## Verification

- Source authority refreshed through 5b367c5a; relevant lifecycle/keyed-builder files inspected at 3826320d
  remain unchanged. Current B2B pins Dunet 1.16.2 and already uses it in ConfirmedBookingTerms. Extracted
  B2B main remains older than PR633. Docs main
  includes PR11; organiser research 5ac4048 is local/unpublished evidence, not approved product policy.
- This commit: B2B and root plan graphs pass with zero errors/warnings. Workflow v2 validates the B2B
  artifact owner, worktree/branch and design-review pause. Superseded runtime union/factory names and
  string-key/translation-enum examples are absent from the commercial plan tree; historical source
  ICommitmentReferenceStep references are deliberately retained.
- This commit: local Markdown links/anchors, three SVG XML documents, union declaration consistency and
  C# excerpt delimiters validate. These checks are not compilation or generated-code qualification.
  Scoped diff whitespace passes. Only the existing plan, ledger and dispatch diagram labels changed.
  Diagram geometry is unchanged; this checkpoint's XML/text checks are not a new rendered visual review.
- The prior broad B2B docs-reachability check found four baseline errors at 85ed6353:
  missing CLAUDE siblings in the two Dashboard test directories; missing AGENTS in E2EAdmin integration
  and KeyedStrategies unit test directories. These are not introduced by the plan; the owning test-guidance
  work/source reconciliation must clear them before docs publication. Do not edit another task's checkout.
- No application build, database test, migration or E2E run was performed for this planning-only change.

## Reviews

The clarification received a scoped consistency self-review against the actual reference strategy,
enum-constrained shared builders, existing factory algorithm and Dunet contract. The prior checkpoint
qualified the Dunet authoring guidance against first-party documentation. Composite-key/default-version
execution, coexistence, recovery and executor removal remain specified design proofs, not implemented
or executed application tests. No independent implementation-readiness review or C# compilation is claimed.
This remains a design draft for Tommy.

## Decisions, discoveries, blockers, and deviations

- Keyed unions are retained for genuine input/result divergence; matching belongs to the owning workflow.
  A named calculation resolver owns pure calculation dispatch. Data-only templates do not require new
  enum members or union arms; multiple implementations already fit one interface case.
- Apply/Accept are operations, not a requirement for two union parents. Current Application uses
  IApplyStep and ICommitmentReferenceStep, not IAcceptStep. The target stores commitment references and
  removes the redundant acceptance resolver after conversion; starting a commitment is the real union
  example. A reference alone is not proof of payment readiness.
- Dunet authoring and explicit PaymentMethodStep naming are agreed; native unions remain a future qualified
  representation/toolchain change, not an SDK upgrade authorised now. Distinct implementations sharing
  one contract do not require separate union cases. Keep the separate proposed acceptance record;
  questions about its purpose did not withdraw that design choice.
- D20-D21 agree typed enum behaviour keys, the optional version default of 1 and shared generic factories.
  The old per-family adapter is superseded. Extend the shared builder/catalogue for direct composite keys
  with explicit supported sets, preserving existing guarantees rather than widening constraints blindly.
  A different closed generic dependency is not a separately implemented factory. No workflow keyed service
  location, automatic latest fallback or enum per template. D22 keeps historical reading separate from
  executable support; unresolved reachable old work can genuinely require continued maintenance.
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
