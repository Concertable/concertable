# B2B commercial execution progress

- Plan: `plans/commercial/COMMERCIAL_EXECUTION_PLAN.md`
- Roadmap: `plans/commercial/COMMERCIAL_ROADMAP.md`
- Roadmap item: `commercial/execution`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/launch_booking-entry-direct-offers`
- Branch: `Docs/launch_booking-entry-direct-offers`
- PR: not opened
- Dependency/package gates: planning-only; external compatibility and contract requirements are not claimed delivered
- Last reconciled: 2026-09-09; initial Deal/Template discussion recorded at Tommy's request, from clean owner HEAD 8dd7b1c2c1c1c0212634833257d440365e0000e2; branch/worktrees rechecked and GitHub search found no owning PR

Artifact paths are relative to the B2B source root, currently api/Concertable.B2B within this Git worktree.
The plan and this sole ledger travel with the qualified B2B source during extraction.

## Current state

Initial Deal/Template design agreed for documentation and later refinement, not implementation-ready.
Tommy explicitly authorised updating the existing plan and this sole ledger after the read-only second
opinion; the original design workstream retains ownership. No application implementation, database
migration, SDK/package change, deployment or publication is authorised by this checkpoint.

The latest clarification records D23-D27: Request parameter naming, Checkout/CheckoutAsync, inherited
repository queries with shape specifications, bounded-context child names and the four-preset parity
cutover. Initial customer-builder/private-publication/sharing features are deferred. Universal access to
the same platform catalogue remains the D6 recommendation, subject to normal eligibility and authority.

D28 is unresolved library/integration selection, not a decision to implement idempotency ourselves.
IdempotentAPI is a candidate for HTTP replay; business/payment identity and recovery remain independently
durable. Raw Guid-header binding and commandJournal are not approved contracts.

D29 now records the initial model in plan section 3: Deal is the concrete arrangement with required
Template and negotiated values; Template is the reusable composition, grouped as Workflows -> Steps and
Requirements, with shared commercial declarations and typed bindings. No duplicate Deal.Configuration,
mandatory TemplateRevision or generic TemplateOperation/OperationStep entity. A future custom builder
creates a private Template of the same type; amount changes do not make a preset custom.

Commercial declaration/value names and physical EF/DTO shapes remain open. Parameters is not the agreed
commercial noun. D30 is the next architectural question: workflow-aware composition semantically couples
Deal to executing modules; data-only contracts avoiding an assembly cycle do not by themselves settle
ownership. This checkpoint preserves that concern rather than declaring the boundary correct.

The new payment capability union remains PaymentMethodStep. The separate prepared-request union/carrier
is a proposed representation, not another settled requirement. Older null-forgiving code and remaining
replay/authority collaborator contracts still require review. B6/B7 operation schemas and policies are
also incomplete; do not delegate missing design to an implementation agent.

## Next Steps

Paused: Tommy - refine the recorded initial design, starting with D30's composition ownership and dependency graph; then settle the commercial-term names, declaration/value EF/DTO mappings and supported step placement/requirements. Reconcile D28 and remaining entry/financial proposals before implementation approval. No implementation or migration may begin without separate explicit authorisation.

This is not a context transfer. Further accepted product policy belongs in the existing Concertable/docs
owners; this B2B plan owns implementation design. Do not create another plan or ledger.

## Completed work

- Earlier architecture checkpoint: B2B-owned successor to the superseded direct-offers plan; PR633/later
  source, docs PR11, organiser research, provider inventory, entity/dispatch diagrams and coordinated slices.
- f9c9264940b92715b9d52d9077df9d738702bc11: D19-D22, Dunet PaymentMethodStep, typed behaviour enums with
  version-1 default, shared direct-key factories, and the proposed support/retirement proof.
- 8dd7b1c2c: naming/query conventions and four-preset parity recorded; replay and template persistence reopened.
- This commit: initial required-Template/negotiated-Deal model, workflow hierarchy, custom-route and
  acceptance/retry examples recorded; rejected persistence sketches replaced; coupling and remaining
  mappings explicitly open. ImmutableArray snapshot guidance retained. No application source changed.

## Verification

- This checkpoint: B2B/root plan graphs pass with zero errors/warnings; Workflow v2 validates owner,
  artifacts and the design-review pause. Local link paths, same-document anchors, fence balance,
  D1-D30 continuity, ledger budget, retained ImmutableArray guidance and git diff --check pass.
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

Scoped documentation consistency self-review completed for the initial design and its open gates. No
independent implementation-readiness review or compiled/generated-code qualification is claimed.
The initial design is recorded; remaining naming, mapping, coupling and entry/financial excerpts are
explicit design gates, not implementation instructions.

## Decisions, discoveries, blockers, and deviations

- D19-D21 remain agreed: Dunet now; runtime PaymentMethodStep with method setup/authorisation cases;
  Save/Verify share IPaymentMethodSetupStep. Homogeneous families stay strategies. The old uniform
  ICommitmentReferenceStep is not what became a union; accepted commitments retain persisted references.
- BehaviourKey<TBehaviour> uses a family enum and an omitted version of 1, never latest. Persist the
  effective version before hashing/issuance. No initial V1 namespaces, speculative V2 leaves, custom
  family factories, translation dictionaries or workflow keyed service location.
- D22 separates history from executable support. Retain executors while reachable/recoverable work needs
  their semantics; retirement deadlines, retention and replay policy still require qualification.
- D23-D30 in the plan own the latest decisions and open questions. Four presets and future builder output
  use one execution model; enum per template and a permanent legacy/custom engine split are rejected.
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
