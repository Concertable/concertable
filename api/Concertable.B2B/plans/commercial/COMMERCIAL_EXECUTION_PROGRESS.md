# B2B commercial execution progress

- Plan: `plans/commercial/COMMERCIAL_EXECUTION_PLAN.md`
- Roadmap: `plans/commercial/COMMERCIAL_ROADMAP.md`
- Roadmap item: `commercial/execution`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/launch_booking-entry-direct-offers`
- Branch: `Docs/launch_booking-entry-direct-offers`
- PR: not opened
- Dependency/package gates: planning-only; external compatibility and contract requirements are not claimed delivered
- Last reconciled: 2026-09-09; Tommy requested a durable design checkpoint and an independent PostgreSQL architecture prompt; clean owner HEAD 654d242fd0583ea13694ae41b73ffdbb0427d960, branch/worktrees and absence of an open owning PR rechecked

Artifact paths are relative to the B2B source root, currently api/Concertable.B2B within this Git worktree.
The plan and this sole ledger travel with the qualified B2B source during extraction.

## Current state

The concrete logical configuration design is now consolidated in plan section 3; PostgreSQL physical
storage remains unresolved. Tommy authorised saving this design and preparing a read-only independent
architecture prompt. The original workstream retains the plan and sole ledger; the next reviewer returns
findings, not edits or implementation. No application changes, migration, SDK/package change, deployment
or publication is authorised by this checkpoint.

The latest clarification records D23-D27: Request parameter naming, Checkout/CheckoutAsync, inherited
repository queries with shape specifications, bounded-context child names and the four-preset parity
cutover. Initial customer-builder/private-publication/sharing features are deferred. Universal access to
the same platform catalogue remains the D6 recommendation, subject to normal eligibility and authority.

D28 is unresolved library/integration selection, not a decision to implement idempotency ourselves.
IdempotentAPI is a candidate for HTTP replay; business/payment identity and recovery remain independently
durable. Raw Guid-header binding and commandJournal are not approved contracts.

D29 now records required Deal.Template and typed DealTerm values; Template selects reusable
WorkflowConfigurations and shared Term declarations; workflows contain typed StepConfigurations.
Section 3 contains cardinalities, concrete relationship members and worked negotiation/acceptance cases.
Money/Percentage declaration and value shapes are distinct; step inputs use named typed references
(Fee/Guarantee -> MoneyTerm, Share -> PercentageTerm), not a generic binding bag or whole-deal subtype.
No duplicate Deal.Configuration, mandatory revision aggregate or catch-all custom row. The future builder
creates a private Template of the same type; amount changes alone do not make a preset custom.

D30 is now agreed: Deal owns the strongly structured workflow-scoped Template. This intentionally
standardises configurable behaviour and couples the data contract to supported module configuration
points, without copying runtime state machines or requiring identical module internals. The rejected
Payment/readiness/collection replacement merely hid workflow mappings; no separate composition owner or
per-module-template/meta-template structure is selected. Terms, WorkflowConfiguration, StepConfiguration
and Version are the current names; sketches omit Entity/Definition suffixes where redundant. This does
not authorise renaming existing code. Complete DTOs, family shapes and physical mappings remain open.

Modules own supported configuration points, mandatory prerequisites and their enforcement. The earlier
generic StepInputBinding, WorkflowPoint and template-authored RequirementEntity graph are superseded.
One module declaration must feed DI registration and compatibility metadata; existing builders lack the
complete configuration/input contract. Same interface/key collisions are not isolated by union catalogue.
Typed preparation and contract tests must cover inputs, guard timing and versions, not only DI resolution.

Section 7 owns the unresolved PostgreSQL review: the blanket TPT recommendation was withdrawn; later
TPC-interim/polymorphic-JSONB-target suggestions are unqualified candidates, not approved mappings.
Tommy wants a concluded best target now, with a supported interim and exact exit only if a real provider
gap requires it. PostgreSQL is already a prerequisite for this refactor. Do not claim JSONB/inheritance
support automatically preserves entity navigations or incoming abstract-step FKs, or invent benchmarks.

The new payment capability union remains PaymentMethodStep. The separate prepared-request union/carrier
is a proposed representation, not another settled requirement. Older null-forgiving code and remaining
replay/authority collaborator contracts still require review. B6/B7 operation schemas and policies are
also incomplete; do not delegate missing design to an implementation agent.

## Next Steps

Paused: Tommy - obtain the requested independent read-only PostgreSQL architecture conclusion against plan sections 3 and 7, then approve the target physical model and any genuinely necessary interim/cutover. Resume design reconciliation when that recommendation and the approval decisions are returned; implementation still needs separate explicit authority.

The outgoing prompt commissions a second opinion, not transfer of plan/ledger ownership. The reviewer
must read repository instructions, establish current B2B source ownership during extraction, inspect
PR633/later entities/DTOs/mappings/consumers/shared factories and current first-party EF/Npgsql evidence,
then return one concrete model with reasons, disadvantages, constraints, query shapes and worked cases.
Choose the best architecture; do not inherit the previous assistant's changing storage recommendations.
No edits, implementation, new ledger, branch/worktree changes or new market research. Preserve the four
preset semantics, distinct entry paths, typed behaviour keys/default 1 and the single shared engine.
Further product decisions stay with the existing Concertable/docs owners.

## Completed work

- Earlier architecture checkpoint: B2B-owned successor to the superseded direct-offers plan; PR633/later
  source, docs PR11, organiser research, provider inventory, entity/dispatch diagrams and coordinated slices.
- f9c9264940b92715b9d52d9077df9d738702bc11: D19-D22, Dunet PaymentMethodStep, typed behaviour enums with
  version-1 default, shared direct-key factories, and the proposed support/retirement proof.
- 8dd7b1c2c: naming/query conventions and four-preset parity recorded; replay and template persistence reopened.
- ebf3989b0: required-Template/negotiated-Deal model, hierarchy, custom-route and acceptance/retry examples;
  rejected persistence sketches replaced and ImmutableArray snapshot guidance retained.
- 654d242fd: final structural agreement and D30 ownership recorded; consistent module configuration
  contract, non-duplicated lifecycle ownership, descriptor authority and checkpoint-compatibility gates.
  Configuration-scalability versus runtime-cost trade-offs recorded. No application source changed.
- This commit: typed terms/step members, reusable workflow relationships, module-owned guards and shared
  definition immutability consolidated; contradictory generic sketches superseded. Independent target-
  first PostgreSQL review commissioned without promoting TPT/TPC/JSONB proposals to settled decisions.

## Verification

- This checkpoint: B2B/root plan graphs pass with zero errors/warnings; Workflow v2 validates the owner,
  artifacts and review pause. Local links/anchors, fences, D1-D30 continuity, ledger budget, retained
  typed/ImmutableArray members, superseded-sketch checks and git diff --check pass. Documentation checks
  do not constitute compilation, provider mapping or performance evidence.
- Rechecked at owner HEAD 654d242fd: git diff against PR633 over api/Concertable.B2B/src is empty.
  This validates the named local baseline, not current remote B2B authority; refresh the latter for review.
- Source evidence retained from this discussion: monorepo 64b3dccec2 includes PR633 and later changes;
  PR951 concerns Payment reconciliation and the comparison from 2df4105989 changes package pins only.
  Extracted B2B main fded052c was older than PR633. Refresh source/owners before delivery.
- Previously inspected product baseline: docs main 99ad353b includes PR11; organiser research 5ac4048 is
  local/unpublished evidence, not approved policy. No newer remote-content qualification is claimed here.
- Current naming/query correction follows BookingWorkflow's inherited GetByIdAsync with
  BookingSpecification.CreateWithContract(); source-parity checks preserve additive Versus and the
  actual Application checkout / Booking confirmation responsibilities.
- Earlier broad B2B reachability check reported four baseline guidance errors (Dashboard CLAUDE siblings,
  E2EAdmin and KeyedStrategies test AGENTS). Reconcile with their owner before publication; do not edit
  another task's worktree. No application build, migration or E2E run was performed.

## Reviews

Earlier independent read-only opinions assessed coupling and typed configuration; their accepted logical
outcomes are in section 3/D30. This checkpoint records the later discussion, not a completed independent
PostgreSQL architecture review. Scoped documentation consistency self-review covers this checkpoint. No compiled
EF model, generated migration, benchmark, workflow-contract test or implementation-readiness proof is
claimed. Remaining mapping and entry/financial excerpts are design gates, not implementation instructions.

## Decisions, discoveries, blockers, and deviations

- D19-D21 remain agreed: Dunet now; runtime PaymentMethodStep with method setup/authorisation cases;
  Save/Verify share IPaymentMethodSetupStep. Homogeneous families stay strategies. The old uniform
  ICommitmentReferenceStep is not what became a union; accepted commitments retain persisted references.
- BehaviourKey<TBehaviour> uses a family enum and an omitted version of 1, never latest. Persist the
  effective version before hashing/issuance. No initial V1 namespaces, speculative V2 leaves, custom
  family factories, translation dictionaries or workflow keyed service location.
- D22 separates history from executable support. Retain executors while reachable/recoverable work needs
  their semantics; retirement deadlines, retention and replay policy still require qualification.
- D23-D30 in the plan own the latest decisions and remaining qualification. Four presets and future builder
  output use one execution model; enum per template and a permanent legacy/custom engine split are rejected.
- Keep both collection choices: HashSet/IReadOnlySet with reference equality for EF relationships;
  ImmutableArray of immutable values for DTO/proposal/accepted snapshots with qualified serialization.
  Neither protects database rows, and hashing must not rely on unordered collection iteration.
- Shared published workflows, steps and declarations must be sealed too; editing one template cannot
  mutate another's accepted behaviour. New composition gets new identities; retire rather than delete
  referenced definitions. Draft retargeting/concurrency and final type/FK enforcement remain open.
- EF TPH/TPC and typed JSONB support already exist; the open complex-type-inheritance feature is not a
  promise of a release or of navigations inside JSON. Section 7 has the dated first-party evidence.
- Entry routes remain distinct and converge in Booking. Keep the proposed separate acceptance record;
  questions about its purpose did not withdraw it. D17-D18's other policies still need approval.
- Existing data retention is unverified. Preserve data until exact environments/cutover are qualified;
  InitialCreate regeneration does not authorise database recreation or replay of completed payments.
- External ticket revenue is evidence, not funding. Collection, available funds, transfer and bank payout
  remain distinct; Payment owns provider execution/ledger facts.
- Shared DataAccess/Messaging, commission, tenant configuration, lifecycle seals and operation recovery
  retain their existing owners. The older all-service PostgreSQL plan must reconcile B2B versus residual
  scope after agreement; never copy its ledger or implement all-service migration from this assignment.
- Specification guidance was found stale in installed generic standards. Upstream generic/roster owners
  are dotagents/agent-standards with unrelated dirty work; no cache or other-worktree edits are authorised.
- The main checkout owns unrelated work and was not modified, switched, reset or stashed.
