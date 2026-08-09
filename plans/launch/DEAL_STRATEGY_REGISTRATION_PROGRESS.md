# Deal-type strategy registration refactor progress

- Plan: `plans/launch/DEAL_STRATEGY_REGISTRATION_PLAN.md`
- Worktree: `C:\Users\tommy\source\repos\Concertable.worktrees\Refactor\launch_deal_strategy_registration`
- Branch: `Refactor/launch_deal_strategy_registration`
- PR: not opened
- Dependency/package gates: none; this is an internal B2B refactor
- Last reconciled: 2026-08-09; Phase 4 is complete in commit `02730b0da`; post-commit fetch found
  `origin/main` at `82644721f`, with the branch 4 commits behind and 15 commits ahead

## Current state

Phases 1, 2, 3, and 4 are complete. Phase 1 is in commit `506bc35e4`; Phase 2 is in commit
`4a741fa50`; Phase 3 is in commit `0a8320289`; Phase 4 is in commit `02730b0da` after merging
`origin/main` at `c72b058afe` through merge commit `dff81e44e`.

The Concert module now registers terms, cohesive payee direction, payment projection, and settlement
calculation, workflows, workflow CLR metadata, lifecycle state machines, and workflow capabilities
vertically per `DealType`. The strategy builder validates the complete graph before mutating DI;
workflow steps are registered once, and both registries are derived from the same workflow definitions.
`IConcertWorkflowFactory` remains the named business factory and delegates through the generic strategy
factory, leaving the module-local generic factory as the only keyed-service lookup.

Phase 4's Concert unit gate, Concert integration gate, and full-solution build are terminal green.
No PR exists and no package gate applies. A post-commit fetch found current `origin/main` at
`82644721f`; the clean branch is 4 commits behind and 15 commits ahead, so Phase 5 must merge that
base drift before editing. No open platform-sync PR exists.

## Next Steps

Implement Phase 5 only — Deal-module families and convention cleanup:

1. Merge current `origin/main` at `82644721f` into the clean branch and rebuild the affected Deal and
   Concert project graphs before editing.
2. Add the Deal-local strategy factory and validated vertical builder, then migrate `DealMapper` and
   `DealUpdater` without introducing a Concert runtime reference or cross-module registry.
3. Preserve EF TPH creation/update validation and JSON-polymorphic contract shapes.
4. Delete the unused `IDealStrategy` marker and correct the stale Deal architecture claim that Payment
   still provides `IStripeValidationStrategy`.
5. Update `api/agents/CODE_PATTERNS.md` with the module-local factory/builder pattern and the established
   factory/resolver/mapper/renderer/serializer/calculator suffix distinctions.
6. Add the planned architecture gate for frozen maps, direct keyed-service use, exact family coverage,
   and the module-local factory lookup allowlist.
7. Run Deal and Concert unit/integration gates plus `dotnet build api/Concertable.slnx` with short
   artifact roots; the merge queue, not local execution, owns the required full E2E tier.
8. Review the final diff, update this ledger, check off Phase 5, and commit that verified phase.

Do not begin review or delivery in the same turn; hand back after the Phase 5 commit and ledger
checkpoint.

## Completed work

- Repository-wide investigation inventoried nine hand-written `DealType` maps plus the workflow
  builder/factory/registry surface.
- Design decisions locked: factory semantics, vertical registration, cohesive terms/party combinations,
  explicit `IDealAccessor` separation, module-local ownership, and .NET 11 union compatibility.
- Existing investigation plan revised rather than duplicated.
- Planning baseline committed as `4dc7b9faf` (`docs(plan): define deal strategy registration refactor`).
- Phase 1 implemented the scoped generic factory and atomic vertical builder, migrated terms to four
  cohesive keyed `IDealTerms` leaves, retained operation-specific facades, and pinned rendering,
  canonical serialization, fingerprints, keyed coverage, and lifetime semantics with tests.
- The factory keeps its narrow `IKeyedServiceProvider` dependency through a scoped adapter that maps
  to the current DI scope; root resolution of scoped keyed leaves remains invalid.
- Phase 1 committed as `506bc35e4` (`refactor(concert): introduce deal strategy registration`).
- Phase 2 merged current `origin/main`, replaced the inverse payee maps with one cohesive
  `IDealPayeeResolver` family, migrated `PaymentAmountMapper` to the generic factory, and moved both
  families into the existing vertical registration block in commit `4a741fa50`
  (`refactor(concert): register payee and payment strategies`).
- Phase 2 added table-driven recipient coverage for all four deal types and composition tests for the
  payee and payment strategy registrations.
- Phase 3 migrated settlement calculation to four vertically registered leaves, collapsed the nested
  artist-share dispatch into DoorSplit and Versus leaves over one revenue-loading base, removed the
  duplicate Deal-entity formulae, and pinned all four gross amounts through unit and integration tests
  in commit `0a8320289` (`refactor(concert): register settlement strategies`).
- Phase 4 folded keyed workflow registration, workflow CLR metadata, lifecycle state machines, steps,
  and capability metadata into the same validated per-`DealType` builder as the migrated Concert
  strategy families. The obsolete parallel registry builder and `AddConcertWorkflows` composition path
  are gone, and the named workflow factory delegates through the generic strategy factory in commit
  `02730b0da` (`refactor(concert): converge workflow registration`).

## Verification

- Read-only repository searches confirmed no production `DealType == ...` or `switch (dealType)` business
  branches in Deal/Concert.
- Confirmed `main` was aligned with `origin/main` before creating the plan branch.
- Documentation-only planning changes require no build or test run.
- 2026-08-08: `git fetch origin --quiet` plus branch/worktree checks confirmed the dedicated worktree
  is clean at its planning baseline, 0 commits behind `origin/main`, and contains no unrelated
  branch-local paths.
- 2026-08-08: full-solution build attempts compiled the changed Concert Application, Infrastructure,
  UnitTests, API, Web, and IntegrationTests projects. The solution result remained red on unrelated
  `MSB3030` copy failures for Customer DataAccess Infrastructure, shared Notification Infrastructure,
  and B2B Conversations IntegrationTests after the interrupted first build; no green solution result
  is claimed.
- 2026-08-08: `docker ps` failed because the Docker Desktop Linux engine pipe did not exist.
  Integration-debug stopped at its mandatory pre-flight; unit and integration tests were not run.
- 2026-08-08: after Docker Desktop started, the Concert unit project ran 105 tests: 86 passed and 19
  failed. Every failure shared the missing `IKeyedServiceProvider` DI registration; no green unit result
  is claimed after the rejected diagnostic change was reverted. The integration run was stopped before
  a terminal result.
- 2026-08-09: `dotnet test` for `Concertable.B2B.Concert.UnitTests` passed 107/107 on the final Phase 1
  source.
- 2026-08-09: `./scripts/integration.ps1 concert --artifacts-path
  C:\Users\tommy\AppData\Local\Temp\Concertable\launch-deal-strategy-integration` passed both projects:
  B2B Concert 144/144 and Customer Concert 11/11.
- 2026-08-09: `dotnet build api/Concertable.slnx --artifacts-path
  C:\Users\tommy\AppData\Local\Temp\Concertable\launch-deal-strategy-build` succeeded with 0 errors
  and 7 existing E2E nullable/generated-code warnings.
- 2026-08-09: `git diff --check` passed and the final diff review found no Phase 2 paths or open Phase 1
  findings.
- 2026-08-09: after merging `origin/main` at `d57e0c2a6`, the affected Concert unit-test project
  built with 0 errors before Phase 2 edits.
- 2026-08-09: `dotnet test` for `Concertable.B2B.Concert.UnitTests` passed 121/121 on the final
  Phase 2 source.
- 2026-08-09: `./scripts/integration.ps1 concert --artifacts-path
  C:\Users\tommy\AppData\Local\Temp\Concertable\launch-deal-strategy-phase2-integration` passed
  both projects: B2B Concert 144/144 and Customer Concert 11/11.
- 2026-08-09: `dotnet build api/Concertable.slnx --artifacts-path
  C:\Users\tommy\AppData\Local\Temp\Concertable\launch-deal-strategy-phase2-solution` succeeded
  with 0 errors and 9 existing nullable/generated-code warnings.
- 2026-08-09: `git diff --check` passed, removed Phase 2 type names have no remaining `api/`
  references, and the final diff review found no open Phase 2 findings.
- 2026-08-09: the post-merge affected Concert and Deal unit-test project builds both succeeded with
  0 errors before Phase 3 edits.
- 2026-08-09: `dotnet test` for `Concertable.B2B.Concert.UnitTests` passed 117/117 and
  `Concertable.B2B.Deal.UnitTests` passed 9/9 on the final Phase 3 source.
- 2026-08-09: `./scripts/integration.ps1 concert --artifacts-path
  C:\Users\tommy\AppData\Local\Temp\Concertable\launch-deal-strategy-phase3-integration-diagnostic
  --blame-hang --blame-hang-timeout 5m --blame-hang-dump-type none` passed both projects: B2B Concert
  144/144 and Customer Concert 11/11.
- 2026-08-09: `dotnet build api/Concertable.slnx --artifacts-path
  C:\Users\tommy\AppData\Local\Temp\Concertable\launch-deal-strategy-phase3-solution` succeeded with
  0 errors and 9 existing nullable/generated-code warnings.
- 2026-08-09: `git diff --check` passed; removed settlement calculator/type names and
  `CalculateArtistShare` have no remaining `api/` references; the final Phase 3 diff review found no
  open findings.
- 2026-08-09: after merging `origin/main` at `c72b058afe` through `dff81e44e`, both affected Concert
  unit and integration project graphs built with 0 errors before Phase 4 edits.
- 2026-08-09: `dotnet test` for `Concertable.B2B.Concert.UnitTests` passed 132/132 on the final Phase 4
  source.
- 2026-08-09: `./scripts/integration.ps1 concert --artifacts-path
  C:\Users\tommy\AppData\Local\Temp\Concertable\launch-deal-strategy-phase4-integration-final`
  passed both projects on the final source: B2B Concert 144/144 and Customer Concert 11/11.
- 2026-08-09: `dotnet build api/Concertable.slnx --artifacts-path
  C:\Users\tommy\AppData\Local\Temp\Concertable\launch-deal-strategy-phase4-solution-final`
  succeeded with 0 errors and 9 existing nullable/generated-code warnings.
- 2026-08-09: `git diff --check` passed; the removed workflow composition API has no remaining
  `api/` references; direct keyed lookup remains confined to the generic strategy factory and its
  focused tests; the final Phase 4 diff review found no open findings.

## Reviews

- Planning document self-reviewed for module ownership, suffix semantics, DI lifetime safety, exact
  coverage validation, accessor separation, union compatibility, and phase verification gates; no open
  planning findings remain.
- Implementation code review pending after the code phases complete.
- Phase 2 implementation self-review covered payee direction, response-shape preservation, keyed
  coverage, facade lifetimes, consumer migration, test construction, and documentation accuracy; no
  open findings remain.
- Phase 3 implementation self-review covered exact keyed coverage, scoped repository dependencies,
  shared revenue loading, formula ownership, independent amount assertions, removed-symbol coverage,
  and documentation accuracy; no open findings remain.
- Phase 4 implementation self-review covered atomic DI mutation, exact workflow/state-machine coverage,
  workflow lifetime and named-factory semantics, immutable capability metadata, preserved capability
  behavior, removed parallel registration paths, test-fixture scope validation, and documentation
  accuracy; no open findings remain.

## Decisions, discoveries, blockers, and deviations

- The Concert factory sits in Concert.Application/Infrastructure because it returns Concert-owned
  strategies. Deal gets a separate module-local equivalent for mapper/updater strategies.
- `IDealAccessor` remains in Concert and is not injected into the factory; callers pass `DealType`
  explicitly after resolving the deal.
- C# 15 union types are available in .NET 11 preview, but the syntax remains preview. Union migration is
  a compatible later change to workflow internals, not a prerequisite for this plan.
- The old plan's recommendation to expose a generic strategy map to consumers was superseded by named
  facades backed by a module-local factory.
- Active prerequisite: Docker Desktop must be running before the Concert integration test gate.
- Phase 1 intentionally leaves `DealTermsRenderer` and `DealTermsSerializer` as operation-specific
  facades; only the four per-type leaves combine rendering and canonical serialization through
  `IDealTerms`.
- The builder accumulates registrations and validates them before mutating `IServiceCollection`, so
  invalid duplicate, coverage, or conflicting-lifetime composition cannot leave partial keyed
  registrations behind.
- Every registered strategy family must explicitly declare its supported `DealType` coverage; the
  builder rejects a family with no declaration before it mutates the service collection.
- This deep Windows worktree pushes several normal `obj`/native-test paths to 260-269 characters.
  Plain build/test commands can therefore fail with `MSB3030` or `0x800700CE` in untouched projects;
  the supported SDK `--artifacts-path` option with the short roots recorded under Verification avoids
  the path limit without changing source or weakening a test.
- Two non-terminal integration attempts exhibited host-load/reset flakes: one SQL command timeout and
  one cluster of date-conflict responses in `ContractApiTests`. The exact timeout test and the whole
  contract class passed in isolation; after shutting down stale SDK build servers, the clean canonical
  module run passed 155/155. No product or fixture change was justified by the non-reproducing failures.
- The planned `IDealPartyResolver` name conflicted with the Concert module's standing vocabulary,
  which reserves `Party` for the invoice snapshot value object. Phase 2 uses
  `IDealPayeeResolver` instead; the cohesive methods and directional semantics are unchanged.
- Because keyed leaves and their unkeyed facade intentionally share the same service interface,
  lifetime tests must select the unkeyed descriptor explicitly rather than count keyed descriptors as
  duplicate facade registrations.
- Settlement leaves follow their real dependencies: stateless FlatFee and VenueHire are singleton;
  repository-backed DoorSplit and Versus are scoped, as is the factory-capturing unkeyed resolver.
- DoorSplit and Versus settlement formulas now have one runtime home in their Concert settlement
  leaves. Deal entities retain validation and mutation invariants but no longer provide a second
  calculation oracle.
- The first Phase 3 Concert integration attempt stopped making progress after 37 passing tests and was
  terminated after eight hours. An immediate full-module diagnostic rerun crossed the same boundary
  and passed 155/155 with a five-minute blame-hang detector; this was a non-reproducing host/test-runner
  hang, so no product, retry, or timeout change was justified.
- Workflow definitions now accumulate transitions and step types without touching `IServiceCollection`;
  the enclosing strategy builder validates exact coverage and lifetimes before emitting keyed workflows,
  deduplicated scoped steps, frozen workflow metadata, and state-machine registries.
- Targeted strategy unit fixtures intentionally retain scope validation but no longer request whole-graph
  `ValidateOnBuild`: the production registration now includes workflows whose unrelated runtime
  dependencies are absent from those partial fixtures. The composition tests prove exact declarations,
  and the real integration host validates the complete startup graph.

## Event log

### 2026-08-08 — design reconciled and plan rewritten

- Action: Audited the current Deal/Concert dispatch families, resolved naming and ownership decisions,
  verified official .NET 11/C# 15 union documentation, and rewrote the existing plan.
- Evidence: `plans/launch/DEAL_STRATEGY_REGISTRATION_PLAN.md`; repository searches recorded above.
- Outcome: The implementation has one decided architecture and five independently green phases.
- Follow-up: Implement Phase 1 only.

### 2026-08-08 — planning baseline committed

- Action: Committed the reviewed plan, progress ledger, and launch-roadmap ownership entry.
- Evidence: commit `4dc7b9faf` (`docs(plan): define deal strategy registration refactor`).
- Outcome: The implementation branch has a durable, resumable planning baseline.
- Follow-up: Implement Phase 1 only.

### 2026-08-08 — dedicated implementation worktree assigned

- Action: Assigned the refactor branch to
  `C:\Users\tommy\source\repos\Concertable.worktrees\Refactor\launch_deal_strategy_registration` and
  returned the primary checkout to `main`.
- Evidence: ledger worktree/branch identity and `git worktree list`.
- Outcome: Plan implementation runs in an isolated worktree rather than occupying the main checkout.
- Follow-up: Implement Phase 1 only in the dedicated worktree.

### 2026-08-08 — Phase 1 implemented; verification blocked at Docker pre-flight

- Action: Added the Concert-local generic strategy factory and vertical builder, migrated the terms
  family to four cohesive keyed leaves, converted all factory-capturing facades to scoped, and added
  pinned rendering/serialization/fingerprint plus composition/lifetime tests.
- Evidence: uncommitted Concert Application, Infrastructure, and UnitTests paths in this worktree;
  solution build output compiled every changed Concert project before unrelated generated-output copy
  failures; `docker ps` could not connect to Docker Desktop.
- Outcome: Runtime and test implementation is ready for verification, but Phase 1 is not complete and
  must not be committed until the solution, unit, and integration gates are green.
- Follow-up: Start Docker Desktop, complete the exact verification steps above, fix any failures, and
  commit Phase 1 without beginning Phase 2.

### 2026-08-08 — implementation rebased onto current main

- Action: Fetched `origin`, verified the worktree/branch identity and dirty paths, preserved the
  recorded Phase 1 work, merged current `origin/main`, and restored the work without conflicts.
- Evidence: merge commit `c984c96a9`; branch is 0 commits behind `origin/main` at `b4cb18e88`; no PR;
  dirty paths remain limited to the recorded Concert Phase 1 implementation and this ledger.
- Outcome: The Phase 1 gate will run against current main while preserving the implementation state.
- Follow-up: Complete the Docker, unit, integration, and full-solution build gates.

### 2026-08-08 — Phase 1 verification stopped on keyed-provider composition

- Action: Started Docker Desktop, ran the Concert unit project, isolated the common DI construction
  failure, and stopped before completing integration verification.
- Evidence: 105 Concert unit tests executed: 86 passed and 19 failed because
  `IKeyedServiceProvider` was not registered for constructor injection. Replacing the dependency with
  `IServiceProvider` made the representative and full unit runs green, proving the cause, but that
  design change was rejected and reverted; the factory still depends on `IKeyedServiceProvider`.
- Outcome: Phase 1 remains uncommitted and unverified. No integration or full-solution green result is
  claimed.
- Follow-up: Register the scoped keyed-provider abstraction correctly, then resume the Phase 1 gates.

### 2026-08-08 — branch refreshed before Phase 1 verification resumed

- Action: Fetched `origin`, preserved the recorded dirty Phase 1 tree, merged current `origin/main`,
  and restored the work without conflicts.
- Evidence: merge commit `3824aa4fd`; branch is 0 commits behind `origin/main` at `0fa7f6460`; no PR;
  dirty paths remain limited to the recorded Concert Phase 1 implementation and this ledger.
- Outcome: Phase 1 verification resumes against current main without changing its scope.
- Follow-up: Add the scoped keyed-provider registration and scope-isolation test, then complete the
  unit, integration, and solution-build gates.

### 2026-08-09 — Phase 1 verified and checkpointed

- Action: Added the scoped keyed-provider adapter and scope-isolation test, strengthened the builder
  to require explicit coverage declarations, completed the unit/integration/build gates, and reviewed
  the final Phase 1 diff.
- Evidence: Concert unit tests 107/107; B2B Concert integration tests 144/144; Customer Concert
  integration tests 11/11; full `api/Concertable.slnx` build 0 errors; `git diff --check` clean.
- Outcome: Phase 1 is complete in this checkpoint; the terms pilot proves the reusable strategy
  registration model without changing any Phase 2 family.
- Follow-up: Implement and verify Phase 2 only.

### 2026-08-09 — post-commit base drift checkpointed

- Action: Verified the Phase 1 commit and clean worktree, then fetched `origin` to reconcile the
  delivery identity before handoff.
- Evidence: commit `506bc35e4`; clean status; `origin/main` at `d57e0c2a6`; branch 48 commits behind
  and 6 commits ahead; no PR.
- Outcome: Phase 1 remains complete and verified against the session-start base, but Phase 2 must
  first merge the newly advanced `origin/main` and rebuild affected projects.
- Follow-up: Reconcile the clean branch with current `origin/main`, then implement Phase 2 only.

### 2026-08-09 — Phase 2 base reconciled

- Action: Fetched `origin`, verified the dedicated worktree was clean and the platform-sync gate was
  clear, merged current `origin/main`, and rebuilt the affected Concert project graph.
- Evidence: merge commit `cadd3c7da`; `origin/main` at `d57e0c2a6`; affected build 0 errors.
- Outcome: Phase 2 started against the current base with no unrelated dirty paths.
- Follow-up: Implement and verify payee direction and payment projection only.

### 2026-08-09 — Phase 2 unit assertion corrected

- Action: Ran the Concert unit gate, diagnosed the two failures, and narrowed the lifetime assertion
  to the unkeyed facade descriptor before rerunning the full project.
- Evidence: initial result 119/121; both failures were the same test counting four keyed leaves plus
  the unkeyed facade. Final result 121/121.
- Outcome: The production registrations were correct; the composition test now distinguishes keyed
  strategies from the unkeyed facade.
- Follow-up: Complete the Concert integration and full-solution build gates.

### 2026-08-09 — Phase 2 verified and checkpointed

- Action: Replaced the inverse ticket/settlement payee maps with one cohesive strategy family,
  migrated payment projection to the generic factory, registered both families vertically, updated
  affected consumers and docs, and completed the Phase 2 verification and diff review.
- Evidence: Concert unit 121/121; B2B Concert integration 144/144; Customer Concert integration 11/11;
  full `api/Concertable.slnx` build 0 errors; removed type names absent from `api/`;
  `git diff --check` clean.
- Outcome: Phase 2 is complete in commit `4a741fa50` with no open findings and no Phase 3 source changes.
- Follow-up: Implement Phase 3 only.

### 2026-08-09 — post-commit base drift checkpointed

- Action: Verified the Phase 2 commit and clean worktree, then reconciled the delivery identity against
  the latest observed remote-tracking base.
- Evidence: Phase 2 commit `4a741fa50`; `origin/main` at `51c8d15b5`; branch 6 commits behind and
  9 commits ahead; no PR.
- Outcome: Phase 2 remains complete and verified against its merged base; Phase 3 must first merge the
  newly advanced `origin/main`.
- Follow-up: Reconcile the clean branch with current `origin/main`, then implement Phase 3 only.

### 2026-08-09 — Phase 3 base reconciled

- Action: Fetched `origin`, verified the dedicated worktree was clean and correctly owned by the
  plan branch, confirmed the open platform-sync PR was pending rather than failed, and merged current
  `origin/main` before Phase 3 edits.
- Evidence: merge commit `e929b46f15`; `origin/main` at `2eb8bc4764`; branch 0 commits behind; no PR;
  no unrelated dirty paths before this ledger checkpoint.
- Outcome: Phase 3 starts against the current base with the requested worktree and service ownership
  confirmed.
- Follow-up: Rebuild the affected Deal/Concert graph, then implement and verify settlement calculation
  only.

### 2026-08-09 — Phase 3 integration host hang diagnosed

- Action: Terminated the exact orphaned test process tree and containers from the stalled canonical
  integration run, then reran the complete Concert module with a five-minute blame-hang detector.
- Evidence: the original run stopped after 37 passing tests with no test failure; the diagnostic rerun
  passed B2B Concert 144/144 and Customer Concert 11/11 without producing a hang sequence.
- Outcome: The stall did not reproduce and is classified as a host/test-runner hang; no source or
  timeout workaround was added.
- Follow-up: Complete the remaining Phase 3 gates and final diff review.

### 2026-08-09 — Phase 3 verified and checkpointed

- Action: Migrated settlement resolution to the generic strategy factory, gave DoorSplit and Versus
  settlement leaves direct formula ownership over shared revenue loading, removed the duplicate
  calculator/entity formulae, strengthened exact-value coverage, and completed the final review.
- Evidence: Concert unit 117/117; Deal unit 9/9; B2B Concert integration 144/144; Customer Concert
  integration 11/11; full `api/Concertable.slnx` build 0 errors; removed symbols absent from `api/`;
  `git diff --check` clean.
- Outcome: Phase 3 is complete in commit `0a8320289` with no open findings and no Phase 4 source
  changes.
- Follow-up: Implement Phase 4 only.

### 2026-08-09 — post-commit base drift checkpointed

- Action: Verified the Phase 3 commit and clean worktree, then fetched `origin` to reconcile delivery
  identity before handoff.
- Evidence: Phase 3 commit `0a8320289`; `origin/main` at `c72b058afe`; branch 13 commits behind and
  12 commits ahead; no PR and no open platform-sync PR.
- Outcome: Phase 3 remains complete and verified against its merged base; Phase 4 must first merge the
  newly advanced `origin/main` and rebuild affected projects.
- Follow-up: Reconcile the clean branch with current `origin/main`, then implement Phase 4 only.

### 2026-08-09 — Phase 4 base reconciled

- Action: Fetched `origin`, verified the dedicated worktree was clean and correctly owned by the plan
  branch, confirmed no PR or open platform-sync PR existed, merged current `origin/main`, and rebuilt
  the affected Concert unit and integration project graphs.
- Evidence: merge commit `dff81e44e`; `origin/main` at `c72b058afe`; both affected builds completed
  with 0 errors; no unrelated dirty paths existed before Phase 4 edits.
- Outcome: Phase 4 started against the current base with the requested worktree and Concert ownership
  confirmed.
- Follow-up: Implement and verify workflow composition convergence only.

### 2026-08-09 — Phase 4 partial-DI unit fixtures corrected

- Action: Ran the Concert unit gate, traced the failures to partial strategy fixtures asking
  `ValidateOnBuild` to construct the newly included workflow graph without its runtime dependencies,
  retained scoped-provider validation in those fixtures, and reran the full project.
- Evidence: the initial run failed only during service-provider construction; the final exact-tree run
  passed 132/132. Production integration startup subsequently resolved the complete workflow graph.
- Outcome: Targeted unit fixtures validate the scope behavior they own, while composition tests and the
  real host retain full workflow registration coverage.
- Follow-up: Complete integration, solution-build, and final review gates.

### 2026-08-09 — Phase 4 verified and checkpointed

- Action: Converged workflow keyed registration, CLR metadata, state machines, capabilities, and steps
  into the validated vertical strategy builder; routed the named workflow factory through the generic
  factory; removed the parallel registry builder; added exact-coverage/capability tests; updated the
  workflow architecture; and completed the final review.
- Evidence: Concert unit 132/132; B2B Concert integration 144/144; Customer Concert integration 11/11;
  full `api/Concertable.slnx` build 0 errors; removed workflow composition names absent from `api/`;
  direct keyed lookup confined to the generic factory and focused tests; `git diff --check` clean.
- Outcome: Phase 4 is complete in commit `02730b0da` with no open findings and no Phase 5 source
  changes.
- Follow-up: Implement Phase 5 only.

### 2026-08-09 — post-commit base drift checkpointed

- Action: Verified the Phase 4 commit and clean worktree, then fetched `origin` to reconcile delivery
  identity before handoff.
- Evidence: Phase 4 commit `02730b0da`; `origin/main` at `82644721f`; branch 4 commits behind and
  15 commits ahead; no PR and no open platform-sync PR.
- Outcome: Phase 4 remains complete and verified against its merged base; Phase 5 must first merge the
  newly advanced `origin/main` and rebuild the affected Deal/Concert projects.
- Follow-up: Reconcile the clean branch with current `origin/main`, then implement Phase 5 only.

## Resume prompt

```
cd C:\Users\tommy\source\repos\Concertable.worktrees\Refactor\launch_deal_strategy_registration
Read @plans/launch/DEAL_STRATEGY_REGISTRATION_PLAN.md and @plans/launch/DEAL_STRATEGY_REGISTRATION_PROGRESS.md and do what its `## Next Steps` says.
```
