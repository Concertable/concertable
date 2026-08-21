# Deal DTO and strategy-dispatch foundation progress

## Worktree and branch

- Plan: `plans/launch/DEAL_CLOSED_SUM_MODEL_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/deal-closed-sum-model`
- Worktree:
  `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-deal-dispatch-foundation`
- Branch: `Refactor/deal-dispatch-foundation`
- Implementation PR: [#678](https://github.com/Concertable/concertable/pull/678), open and ready, targeting `main`;
  queue-ejected after API E2E failure; verified remote head
  `ad1de76998d3916ba11028eb625357aad91feb11`
- Push state: reviewed work head `0a0958719cd44cdb29e8228bacaa8c464c485642` was pushed and verified
  equal locally, at `origin/Refactor/deal-dispatch-foundation`, and on PR #678; this ledger commit is
  the checkpoint-transport leg
- Prototype branch: `Spike/net11-closed-dispatch`
- Prototype commit: `785cd80403eb2f3db173428854730dec961e39d9`
- Foundation implementation commit: `a7c836930652dc18653f9e8a5670019310fdef54`
- Current-main merge commit: `a190d4552`, merging `origin/main` at
  `42f760994e15c909c9e56ffb3fde045210457bbf`
- Downstream dependent: `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md` remains suspended until
  this foundation is terminal on `main`.
- Last reconciled: 2026-08-21 after current-main sync and local Workers composition verification

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
lifetime conflicts, and requires complete coverage for both families during composition. Deal-owned keyed
registration and resolution remain confined to Deal Infrastructure. The updater facade validates DTO/entity
`DealType` agreement before resolving and casting a concrete updater.

The production generator projects, analyzer references, attributes, anchors, and generated registration
dependency have been removed from this branch. The complete prototype remains recoverable from
`Spike/net11-closed-dispatch` at `785cd8040`, including the incomplete same-pass attribute-source redesign.

The full-E2E merge group exposed a pre-existing Workers composition defect: the User module's
`CredentialRegisteredHandler` requires `IAdminModule`, but B2B Workers did not reference or register Admin
Infrastructure. Current `main` has since landed the required dependency and a stronger production-host
composition suite. Merging that state exposed duplicate Admin references and registrations from the two
concurrent fixes; this branch now reconciles them to one and removes its superseded lightweight unit test.

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

After this review checkpoint commit, publish the reviewed head through the plan-managed two-leg push,
wait for exact-head CI, then requeue PR #678 with `full-e2e` as the authoritative E2E gate.

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

- Deal unit tests: 57 passed, 0 failed after `beab16bd9`, including JSON, catalog, registration-builder,
  factory, lifetime, and mismatch coverage.
- Concert unit tests: 229 passed, 0 failed after `beab16bd9`.
- `dotnet build api/Concertable.B2B/Concertable.B2B.slnx --no-restore --nologo`: succeeded with 0 errors
  and 2 existing generated nullable-context warnings after `beab16bd9`.
- Architecture tests after `beab16bd9`: 8 passed and 1 unrelated existing package-ownership test failed. The first
  mismatch is the DataAccess unit-test project retaining an unused direct `Reunion` package; a read-only
  diagnostic found the same stale-package class in several projects outside this change. No Deal
  project is mismatched.
- No E2E suite was run locally.
- Invariant scans passed after `beab16bd9`: no production `IDeal`, old concrete C# DTO names, generator
  protocol, Deal-owned keyed-DI use outside Deal Infrastructure, or strategy switches in Deal consumers.
- `git diff --check`: passed.
- `python .agents/hooks/plan_graph.py --root
  C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-deal-dispatch-foundation`: 0 errors
  and 0 warnings.
- `/pr-preflight`: GREEN after `5436a4af3`; branch naming, clean tree, current-main drift (`0` behind,
  `8` ahead), existing-PR, platform-sync, non-packable Deal contracts, and recorded local gates all pass.
- PR #678 exact-head CI at `ad1de7699`: build, carves, all unit suites, and all integration suites passed.
- Full-E2E merge-group run [32414372870](https://github.com/Concertable/concertable/actions/runs/32414372870)
  failed in `e2e-api-tests`: the four `ConcertFinishedTests` timed out in `WorkersFixture.TriggerAsync`;
  diagnostics showed Workers crashing because `IAdminModule` was not registered. UI E2E was not run after
  the API gate failed.
- Current-main B2B production-host composition tests after Workers deduplication: 5 passed, 0 failed,
  including strict Functions-host validation and the explicit missing-`IAdminModule` regression.
- Current-main Deal unit tests after the sync and Workers reconciliation: 57 passed, 0 failed.
- `scripts/docker-health.ps1` passed a fresh-container host-to-container data round trip. The one targeted
  `ConcertFinishedTests` run then stopped at fixture startup because Aspire reported the Docker runtime
  unhealthy; no scenario executed and the environment-failed E2E was not retried.

## Reviews

- Full review `133b018d..2e34ce37`: two findings in
  `reviews/Refactor-deal-dispatch-foundation.md`.
- CV1 fixed in `91e1aa756`: the keyed factory again uses an explicit readonly dependency field.
- CV2 fixed in `bb8aa0840`: validated vertical strategy registration and composition-time family coverage
  were restored.
- Incremental review `2e34ce37..bb8aa084`: clean; reviewed watermark is `bb8aa0840`.
- Post-main-sync incremental review `bb8aa084..beab16bd` (28 commits): clean; native and security-sensitive
  passes found no issues, and both review watermarks are `beab16bd9`.
- Pre-merge incremental review `beab16bd..e4fdc642` (3 commits): clean; the range contains only plan and
  review checkpoints, with no runtime or security-sensitive changes; review watermark is `e4fdc642d`.
- Queue-fix/current-main incremental review `e4fdc642..c4a536f8`: clean after isolating the three PR-owned
  commits and checking the net branch diff against `origin/main@42f760994`; both review watermarks are
  `c4a536f8b`.

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
