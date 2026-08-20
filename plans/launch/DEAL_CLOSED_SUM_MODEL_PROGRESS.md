# Deal DTO and strategy-dispatch foundation progress

## Worktree and branch

- Plan: `plans/launch/DEAL_CLOSED_SUM_MODEL_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/deal-closed-sum-model`
- Worktree:
  `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-deal-dispatch-foundation`
- Branch: `Refactor/deal-dispatch-foundation`
- Implementation PR: not created
- Push state: intentionally unpushed
- Prototype branch: `Spike/net11-closed-dispatch`
- Prototype commit: `785cd80403eb2f3db173428854730dec961e39d9`
- Foundation implementation commit: `a7c836930652dc18653f9e8a5670019310fdef54`
- Downstream dependent: `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md` remains suspended until
  this foundation is terminal on `main`.
- Last reconciled: 2026-08-20 after the full review and clean incremental review of both fix commits

## Current state

The current PR is the smaller .NET 10 foundation. `IDeal` is deleted and the transport hierarchy is an
abstract `DealDto` record with four sealed `*DealDto` cases. Existing JSON discriminator tokens remain
unchanged. The parallel `DealEntity` hierarchy remains the persistence model, and the B2B consumers have
been migrated to DTO names.

Deal Application retains invariant `IDealStrategyFactory<TStrategy>` with `Create(DealDto)` and
`Create(DealEntity)`. `IDealMapper` and `IDealUpdater` inherit the module strategy marker; their facades
ask only that factory to select the concrete leaf.

Deal Infrastructure now implements the factory with Microsoft's built-in keyed DI. The module-local
validated builder declares mapper/updater registrations vertically by `DealType`, rejects duplicates and
lifetime conflicts, and requires complete coverage for both families during composition. Only the Deal
composition root and internal factory use keyed APIs. The updater facade validates DTO/entity `DealType`
agreement before resolving and casting a concrete updater.

The production generator projects, analyzer references, attributes, anchors, and generated registration
dependency have been removed from this branch. The complete prototype remains recoverable from
`Spike/net11-closed-dispatch` at `785cd8040`, including the incomplete same-pass attribute-source redesign.

The plan now separates three delivery stages:

1. this .NET 10 DTO/keyed-DI foundation;
2. a separate general-purpose public C# 15/.NET 11 NuGet library;
3. a later Concertable migration that preserves the module factory and family interfaces while replacing
   their implementation and registrations.

## Guarantees

The .NET 10 foundation provides:

- an invariant module-owned factory seam;
- one selection mechanism shared by mapper/updater facades;
- selection from either `DealDto` or `DealEntity`;
- composition-time validation of every mapper/updater family and `DealType` pair;
- keyed-provider isolation inside Deal Infrastructure;
- typed DTO/entity mismatch failure;
- tests aligning the known DTO/entity/enum/JSON/keyed-registration/frontend-token catalogs;
- DI validation and keyed-leaf lifetime coverage.

It does not provide:

- language-level closure of either hierarchy;
- compiler-proven exhaustive subtype switches;
- compile-time proof of every family-by-case implementation;
- generator diagnostics for missing, duplicate, inaccessible, or unconstructable leaves;
- native-union exhaustiveness;
- a public reusable dispatch abstraction.

Those compile-time guarantees belong to the separate public library and later .NET 11 migration.

## Next Steps

Bring `Refactor/deal-dispatch-foundation` up to date with current `origin/main` in this worktree, resolve any
conflicts, rerun the affected focused gates, and incrementally review the resulting branch-only changes. Run
`/pr-preflight` only after that review is clean; do not push or create the PR before the preflight is green.

## Separate public-library follow-up

After this foundation is reviewed, create a new plan and isolated branch from current `main` for the
public C# 15/.NET 11 library. Use `Spike/net11-closed-dispatch` as research evidence, then settle the
semantic API and package split before implementation:

- distinguish ordinary keyed lookup from exhaustive closed-case dispatch and closed-family resolution;
- resolve the collision with Microsoft's `IKeyedServiceProvider` and whether any generic public
  interface is needed;
- settle operation naming, nullability, and variance;
- design compiler-visible abstractions plus analyzer/generator packaging;
- prove Roslyn symbol-based family-by-case diagnostics and generated DI against real .NET 11 and
  non-Concertable fixtures.

The public library is not implemented or published by the current PR.

## Downstream handoffs

- `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md` remains blocked until this foundation is terminal
  on `main`; then update that ledger with the delivered foundation commit before resuming its preserved
  implementation slice.

## Completed work

- Inspected the full dirty tree, complete diff, generator prototype, generator tests, Deal contracts,
  Application abstractions, Infrastructure registration, current tests, pre-generator keyed
  implementation, and every requested symbol reference.
- Preserved the complete research state on `Spike/net11-closed-dispatch` at `785cd8040` without pushing.
- Deleted `IDeal` and introduced `DealDto`, `FlatFeeDealDto`, `DoorSplitDealDto`, `VersusDealDto`, and
  `VenueHireDealDto`.
- Migrated Deal and Concert B2B consumers while retaining singular `Concertable.B2B.Deal.*` identities.
- Preserved JSON `$type` tokens and added four-case serialization round trips.
- Retained the parallel `DealEntity` hierarchy.
- Preserved invariant `IDealStrategyFactory<TStrategy>` and both selector overloads.
- Replaced generator dispatch with Microsoft keyed singleton registrations and one internal scoped
  factory in Deal Infrastructure.
- Restored the module-local validated strategy builder and vertical mapper/updater registration in
  `bb8aa0840`, with complete coverage required during composition.
- Retained mapper/updater leaves and typed mismatch validation.
- Added registration completeness coverage for the Cartesian product of current `DealType` values and
  both strategy families.
- Added catalog agreement across DTO cases, entity cases, enum members, JSON attributes, keyed
  registrations, and existing frontend discriminator tokens.
- Removed generator tools, solution entries, Roslyn package pin, analyzer references, annotations, and
  production generation anchors from the feature branch.
- Rewrote the governing plan around the current .NET 10 PR, separate public library, and later
  Concertable .NET 11 migration.

## Verification

- Deal unit tests: 57 passed, 0 failed at `bb8aa0840`, including JSON, catalog, registration-builder,
  factory, lifetime, and mismatch coverage.
- Concert unit tests: 229 passed, 0 failed.
- `dotnet build api/Concertable.B2B/Concertable.B2B.slnx --no-restore --nologo`: succeeded with 0 errors
  and 2 existing generated nullable-context warnings.
- Architecture tests: 8 passed and 1 unrelated existing package-ownership test failed. The first
  mismatch is the DataAccess unit-test project retaining an unused direct `Reunion` package; a read-only
  diagnostic found the same stale-package class in several projects outside this change. No Deal
  project is mismatched.
- No E2E suite was run locally.
- Invariant scans passed: no production `IDeal`, old concrete C# DTO names, generator protocol,
  keyed-DI use outside Deal Infrastructure, or strategy switches in Deal consumers.
- `git diff --check`: passed.
- `python .agents/hooks/plan_graph.py --root
  C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-deal-dispatch-foundation`: 0 errors
  and 0 warnings.

## Reviews

- Full review `133b018d..2e34ce37`: two findings in
  `reviews/Refactor-deal-dispatch-foundation.md`.
- CV1 fixed in `91e1aa756`: the keyed factory again uses an explicit readonly dependency field.
- CV2 fixed in `bb8aa0840`: validated vertical strategy registration and composition-time family coverage
  were restored.
- Incremental review `2e34ce37..bb8aa084`: clean; reviewed watermark is `bb8aa0840`.

## Decisions and discoveries

- The stable public-to-the-module seam is `IDealStrategyFactory<TStrategy>`, not a generic keyed-provider
  abstraction and not a second dispatcher API.
- The generic factory remains invariant by omitting variance modifiers.
- Stateless mapper/updater leaves retain the pre-generator singleton lifetime; facades and the factory
  are scoped.
- Built-in keyed DI is explicitly temporary and does not claim compile-time completeness.
- Catalog and registration tests protect the known .NET 10 case set without calling the hierarchy
  closed.
- The prototype's generated contract attribute is not semantically available during the same generator
  pass. That incomplete attempt is preserved on the spike rather than repaired for B2B.
- The eventual analyzer may use a compiler-host-compatible TFM while requiring C# 15/.NET 11 semantics
  from consuming projects.
- Heterogeneous results and handlers are called variants, not operations.
