# B2B commercial execution progress

- Plan: `plans/commercial/COMMERCIAL_EXECUTION_PLAN.md`
- Roadmap: `plans/commercial/COMMERCIAL_ROADMAP.md`
- Roadmap item: `commercial/execution`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/launch_booking-entry-direct-offers`
- Branch: `Docs/launch_booking-entry-direct-offers`
- PR: not opened
- Dependency/package gates: planning-only; external compatibility and contract requirements are not claimed delivered
- Last reconciled: 2026-09-09; Tommy confirmed the final structural direction and D30's deliberate coupling after the independent second opinion; clean owner HEAD ebf3989b01b04389a6e8637cab8d407ee7acb28e, branch/worktrees and absence of an owning PR rechecked

Artifact paths are relative to the B2B source root, currently api/Concertable.B2B within this Git worktree.
The plan and this sole ledger travel with the qualified B2B source during extraction.

## Current state

Deal/Template structure and composition ownership are agreed; physical and remaining entry/financial
contracts are not implementation-ready. Tommy explicitly authorised recording this final architectural
direction in the existing plan and sole ledger; the original design workstream retains ownership. No
application implementation, database migration, SDK/package change, deployment or publication is
authorised by this checkpoint.

The latest clarification records D23-D27: Request parameter naming, Checkout/CheckoutAsync, inherited
repository queries with shape specifications, bounded-context child names and the four-preset parity
cutover. Initial customer-builder/private-publication/sharing features are deferred. Universal access to
the same platform catalogue remains the D6 recommendation, subject to normal eligibility and authority.

D28 is unresolved library/integration selection, not a decision to implement idempotency ourselves.
IdempotentAPI is a candidate for HTTP replay; business/payment identity and recovery remain independently
durable. Raw Guid-header binding and commandJournal are not approved contracts.

D29 records the agreed structure in plan section 3: Deal is the concrete arrangement with required
Template and negotiated values; Template is the reusable composition, grouped as Workflows -> Steps and
Requirements, with shared commercial declarations and typed bindings. No duplicate Deal.Configuration,
mandatory TemplateRevision or generic TemplateOperation/OperationStep entity. A future custom builder
creates a private Template of the same type; amount changes do not make a preset custom.

D30 is now agreed: Deal owns the strongly structured workflow-scoped Template. This intentionally
standardises configurable behaviour and couples the data contract to supported module configuration
points, without copying runtime state machines or requiring identical module internals. The rejected
Payment/readiness/collection replacement merely hid workflow mappings; no separate composition owner or
per-module-template/meta-template structure is selected. Commercial names and physical EF/DTO shapes
remain open; Parameters is not the agreed commercial noun.

Modules own supported configuration points and their enforcement. One module-authored declaration must
feed DI registration and descriptive validation metadata; existing DI builders cannot infer semantic
compatibility. Contract tests must cover guard placement and stable checkpoint timing, not only leaf
versions. Distinct union catalogues do not isolate conflicting registrations of the same interface/key.
Template-to-step selections move into data; implementation registration stays in code. Section 7 records
the flexibility versus loading/validation cost, without claiming improved runtime throughput.

The new payment capability union remains PaymentMethodStep. The separate prepared-request union/carrier
is a proposed representation, not another settled requirement. Older null-forgiving code and remaining
replay/authority collaborator contracts still require review. B6/B7 operation schemas and policies are
also incomplete; do not delegate missing design to an implementation agent.

## Next Steps

Paused: Tommy - review the remaining implementation-contract details under the agreed D29/D30 architecture: commercial-term names and declaration/value EF/DTO mappings, supported step placement/requirements and their compatibility enforcement. Reconcile D28 and remaining entry/financial proposals, then explicitly authorise an implementation slice. The ownership/hierarchy choice is settled; no implementation or migration is authorised by recording it.

This is not a context transfer. Further accepted product policy belongs in the existing Concertable/docs
owners; this B2B plan owns implementation design. Do not create another plan or ledger.

## Completed work

- Earlier architecture checkpoint: B2B-owned successor to the superseded direct-offers plan; PR633/later
  source, docs PR11, organiser research, provider inventory, entity/dispatch diagrams and coordinated slices.
- f9c9264940b92715b9d52d9077df9d738702bc11: D19-D22, Dunet PaymentMethodStep, typed behaviour enums with
  version-1 default, shared direct-key factories, and the proposed support/retirement proof.
- 8dd7b1c2c: naming/query conventions and four-preset parity recorded; replay and template persistence reopened.
- ebf3989b0: required-Template/negotiated-Deal model, hierarchy, custom-route and acceptance/retry examples;
  rejected persistence sketches replaced and ImmutableArray snapshot guidance retained.
- This commit: final structural agreement and D30 ownership recorded; consistent module configuration
  contract, non-duplicated lifecycle ownership, descriptor authority and checkpoint-compatibility gates.
  Configuration-scalability versus runtime-cost trade-offs recorded. No application source changed.

## Verification

- This checkpoint: B2B/root plan graphs pass with zero errors/warnings; Workflow v2 validates owner,
  artifacts and the design-review pause. Local link paths, same-document anchors, fence balance,
  D1-D30 continuity, ledger budget, retained ImmutableArray guidance, settled D30 references and
  git diff --check pass.
  These are documentation checks, not C# compilation or runtime verification.
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

A focused independent read-only architecture second opinion assessed the coupling, alternatives and
change-propagation risks; its accepted outcome is recorded in plan section 3/D30. Scoped documentation
consistency self-review covers this checkpoint. No independent implementation-readiness review,
compiled/generated-code qualification or executed workflow-contract tests are claimed. Remaining naming,
mapping and entry/financial excerpts are design gates, not implementation instructions.

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
