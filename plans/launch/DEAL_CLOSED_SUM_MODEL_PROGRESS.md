# Deal representation and common-interface dispatch progress

## Worktree and branch

- Plan: `plans/launch/DEAL_CLOSED_SUM_MODEL_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/deal-closed-sum-model`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-launch_deal-dispatch-plan`
- Branch: `Docs/launch_deal-dispatch-plan`
- PR: not created
- Base: `origin/main` at `1647ec6f85911503e8f2ad3ce2bb0fdc94cd1d14`
- Dependency gates: implementation blocked on terminal delivery of lifecycle PR #633; the C# 15
  cut-over also requires the B2B .NET 11 compiler/runtime/consumer matrix
- Last reconciled: 2026-08-19

## Current state

The architecture decision is settled. The published Deal target is a C# 15 closed record hierarchy,
while genuinely heterogeneous internal operation inputs/results use native unions. Application
acceptance is fixed as exactly the paid/simple union.

When all Deal cases implement one honest interface with the same method set, parameter shapes, return
shapes, and semantics, callers use a module-specific invariant factory. A B2B-local source
generator/analyzer emits the exhaustive generic factory, one switch per input hierarchy, the long closed
generic types, closed family registrations, and build diagnostics. Consumer and composition-root source
contains no repeated Deal switch, keyed lookup, dictionary, service provider, or five-type generic.

The only proven common-interface families are Application `IDealTerms` and Deal `IDealMapper` plus
`IDealUpdater`. Lifecycle accept/confirm/complete executors and steps do not use this factory: their
inputs, results, or capabilities are genuinely heterogeneous and remain union/match operations.
Cancellation is direct, payer/payee direction is data, and the dead settlement resolver is deleted.

Net10 uses the same handwritten factory/facade surface with `IDeal`, generated catalog diagnostics,
and an explicit unknown-case fallback. C# 15 changes the parameter to closed `Deal`, removes the
fallback from generated switches, and promotes `CS8509` to an error. The generator independently
requires one case implementation for every declared family.

No production code has changed. The complete factory, registration, lifetime, casting, invariance,
exhaustiveness, fifth-case, and delivery contract is now in the plan.

## Next Steps

Blocked: The target Application, Booking, and Concert module layout is not yet delivered on `main`.
Blocked by: `plans/launch/DEAL_LIFECYCLE_OWNERSHIP_PROGRESS.md` and lifecycle PR #633.
Unblock action: The lifecycle owner must deliver PR #633 and its remaining review, CI, merge, package, platform-sync, and ledger gates, then register this plan as the follow-on owner for replacing provisional selectors with generated common-interface factories, unions, direct calls, or data according to the settled classification.
Resume when: Current `main` contains the delivered lifecycle split and the lifecycle ledger records its delivery lifecycle terminal green; then create the Phase 0 generator-proof worktree from `origin/main`.

## Completed work

- Inventoried the current and lifecycle-branch Deal contracts, entity hierarchy, JSON catalog, mapper,
  updater, Application terms, registrations, module factories, and provisional lifecycle selectors.
- Rejected repeated handwritten switches for honest same-interface families and rejected built-in keyed
  DI, `IServiceProvider`, `FrozenDictionary`, and handwritten five-type registrations as the long-term
  common-family mechanism.
- Defined the complete generated Application and Deal factory surfaces, module markers, discovery
  convention, exhaustive factory, generated registration extensions, facade use, and negative diagnostics.
- Fixed the eligible family inventory to `IDealTerms`, `IDealMapper`, and `IDealUpdater`; no current
  Booking or Concert executor/step family qualifies.
- Defined the precise guarantee split among C# compiler, generator/analyzer, DI startup, runtime domain
  validation, and architecture tests.
- Recorded that the factory is genuinely invariant, C# 15 switches are compiler-exhaustive only when
  `CS8509` is elevated, and net10 coverage is generator-enforced best effort rather than native closure.
- Recorded the unavoidable trade-offs: leaves still narrow the base Deal at runtime, and direct
  constructor injection eagerly constructs all four scoped leaves when a family factory is resolved.
- Preserved the exact paid/simple acceptance union and reclassified confirm/complete lifecycle
  operations as heterogeneous unions rather than factory families.
- Defined a generator-proof phase before any product migration and the later net10, heterogeneous,
  C# 15 package-cut-over, and enforcement phases.

## Verification

- Repository evidence inspected against the current branch and lifecycle PR #633 worktree.
- Official C# 15 evidence confirms closed-hierarchy switches are exhaustive over all reachable direct
  descendants and non-exhaustive switch expressions remain compiler warnings unless promoted.
- Source-generator feasibility is specified as an implementation hypothesis with an executable Phase 0
  compile-test gate; no uncompiled generator detail is treated as delivered fact.
- `python .agents/hooks/plan_graph.py --root
  C:\Users\TommySeery\source\repos\Concertable.worktrees\Refactor\keyed-strategy-dispatch`: 0 errors
  and 0 warnings.
- `git diff --check`: passed.
- Stale-design scans found no surviving keyed-completion, retained-keyed, mapper/updater/terms-switch,
  or `IKeyedServiceProvider` target direction in the affected plan graph.
- Complete docs review of `1647ec6f8..88c368d39` plus the finding-fix working tree passed the
  accuracy, contradiction, placement, concision, dangling-reference, and followability lenses; the
  artifact is stamped at `88c368d3935273fc6e786bc2c07c180686c99c60`.
- Incremental docs review of `88c368d39..dc9b6a744` found four issues in the corrected factory-only
  design: leaf placement was too narrow, Application diagnostics lacked a local discovery contract, the
  review state described a committed checkpoint as uncommitted, and one C# 15 example omitted the
  diagnostics annotation. Commits `4d339cf3d` and `dc9b6a744` resolve them; final confirmation is clean.

## Reviews

- `reviews/Refactor-keyed-strategy-dispatch.md` is stamped at
  `dc9b6a744c9196e088f6280b13b4ab9b45c34976`. Its original boundary findings and four incremental
  findings are resolved, final confirmation is clean, and no finding remains open.

## Decisions and discoveries

- The common-interface factory is named `Factory`, matching both the current PR vocabulary and the C#
  naming rule that a factory returns a selected component. `Resolver` is reserved for a collaborator
  that consumes inputs and returns the final domain answer.
- The existing `DealStrategyFactory<TStrategy>` is not a service-global factory. Its interface is
  internal to Deal Application and its implementation belongs to Deal Infrastructure. The reusable
  global asset in the target design is the B2B-local generator template, not a shared runtime factory.
- Application and Deal factories share generated mechanics but have disjoint compiler-visible family
  catalogs and Infrastructure registrations. Application admits only `IApplicationDealStrategy`
  families such as `IDealTerms`; Deal admits only `IDealStrategy` families such as `IDealMapper` and
  `IDealUpdater`. A cross-module family fails the factory marker constraint.
- The marker, annotated short factory interface, and family interface live in the owning module
  Application project. Concrete leaves stay in the natural layer of their family: terms and mapper
  leaves are Application-owned, while updater leaves are Infrastructure-owned. The generated long
  factory, registrations, and generation anchor live in module Infrastructure. The generator/analyzer
  lives once in B2B-local build tooling.
- Booking and Concert do not receive factories merely for symmetry. Neither currently owns a proven
  same-interface family; a module-specific factory is added only when such a family exists.
- `IApplicationDealStrategyFactory<TStrategy>` remains invariant by omitting `in` and `out`.
- `IApplicationDealStrategy` and `IDealStrategy` are module-family markers that remain on .NET 11.
  They are not per-case `IDealStrategyFor<TDeal>` interfaces; no such interface is introduced.
- Marker constraints make cross-module factory use a C# error. Generator diagnostics reject the marker
  itself, an undeclared marker subinterface, and any family without exactly one implementation per
  Deal case.
- The generator maps existing case/family names and emits registrations, preventing a composition root
  from manually swapping two valid leaf types.
- The generated switch is maintained once in the generator template and expanded once per distinct
  input hierarchy. Application needs the Deal hierarchy; Deal needs Deal and DealEntity. It is not
  repeated per operation family.
- Factory interfaces and module markers remain in Application. An Application-local contract annotation
  drives factory-use diagnostics without generating runtime output. One separate annotated anchor in
  each module Infrastructure project triggers generation into the assembly that owns the factory, DI
  registrations, and composition root. Phase 0 must prove both discovery paths through the real
  two-project `InternalsVisibleTo` topology.
- Generator coverage follows the legitimate project graph: Application validates the visible
  Deal/JSON/enum contract catalog, Deal additionally validates the entity hierarchy, and architecture
  tests retain cross-module EF and TypeScript catalog agreement.
- The long five-type factory implementation is permitted only in `.g.cs`; handwritten application and
  test code use only `IApplicationDealStrategyFactory<IDealTerms>` or the equivalent Deal factory.
- Keyed DI and `FrozenDictionary` do not provide closed-case compiler exhaustiveness or generic
  invariance. They remain valid mechanisms for other domains, but not this common-interface Deal
  backbone.
- `DealType` remains an ordinary explicitly valued enum and cannot itself be `closed`. Generated C# 15
  exhaustiveness switches on the closed `Deal`/`DealEntity` hierarchies; the enum remains boundary
  identity only.
- For four cases the generated type switch is a small chain of type tests and a direct field return,
  not a material lookup regression from `FrozenDictionary`; the design is selected for build-time
  totality and type coherence, not for an unmeasured microbenchmark claim.
- The factory interface remains the invariant module-owned selection seam and hides the generated long
  factory from facades. Removing it would either leak generated infrastructure or restore per-facade
  switches.
- Removing service location removes selected-only construction. The generated scoped factory constructs
  all four leaves on first resolution in a scope. This is accepted for the three proven local families
  and explicitly prevents using the mechanism for heterogeneous/expensive lifecycle effect graphs.
- Leaf narrowing remains a runtime cast because the common interface accepts the base Deal. Eliminating
  it would require generated per-method adapters or a different typed interface per case, neither of
  which is selected.
- Built-in DI cannot prove arbitrary constructor graphs at C# compile time. Generator coverage is a
  build error; constructor resolvability and scope safety remain `ValidateOnBuild`/`ValidateScopes`
  startup guarantees.
- Adding a fifth Deal fails the generator for every missing family case and makes the generated C# 15
  switch non-exhaustive. Once the new leaves exist, the generator expands factories and registrations
  with no caller or composition-root dispatch edit.
- Application acceptance remains exactly Dunet paid/simple on net10 and
  `internal union Accept(PaidAccept, SimpleAccept);` on C# 15. It may not be reclassified.
