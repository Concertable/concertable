# Deal-type strategy registration refactor progress

- Plan: `plans/launch/DEAL_STRATEGY_REGISTRATION_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-launch_deal_strategy_registration`
- Branch: `Refactor/launch_deal_strategy_registration`
- PR: [#451](https://github.com/Concertable/concertable/pull/451) — open; verified remote head `bc05263e7`
- Dependency/package gates: no pre-merge package dependency; generated platform-sync observation is
  required after the `api/**` source PR merges
- Last reconciled: 2026-08-11; all five implementation phases are complete; incremental review of
  `36375ffdf..bc05263e7` found no issues; PR #451 is green at `bc05263e7` but the branch is 221
  commits behind the last fetched `origin/main`; pre-existing platform-sync PR #488 has now merged
  green as `130211aa9`, so the branch can fetch and reconcile the final current platform pin

## Current state

All five implementation phases are complete. Phase 1 is in commit `506bc35e4`; Phase 2 is in commit
`4a741fa50`; Phase 3 is in commit `0a8320289`; Phase 4 is in commit `02730b0da` after merging
`origin/main` at `c72b058afe` through merge commit `dff81e44e`.

The Concert module now registers terms, cohesive payee direction, payment projection, and settlement
calculation, workflows, workflow CLR metadata, lifecycle state machines, and workflow capabilities
vertically per `DealType`. The strategy builder validates the complete graph before mutating DI;
workflow steps are registered once, and both registries are derived from the same workflow definitions.
`IConcertWorkflowFactory` remains the named business factory and delegates through the generic strategy
factory, leaving the module-local generic factory as the only keyed-service lookup.

The Deal module now owns an equivalent scoped generic strategy factory and validated vertical builder
for `IDealMapper` and `IDealUpdater`; both named facades delegate through the factory, all eight leaves
are keyed singleton registrations, and exact coverage is required before DI mutates. The unused
`IDealStrategy` marker and stale Payment strategy claim are gone. A self-verifying architecture gate
confines frozen deal-type maps to workflow registries, keyed-provider access to module composition and
factories, and keyed lookup to the two module-local factory implementations.

Phase 5's Deal/Concert unit gates, Concert integration gate, and full-solution build are terminal green.
The implementation is committed as `4d4f44e0a`. The complete `code-review` range
`43fe1caf4aa2c8e808c79821b3cf6d66c7b48574..fb34f37b17387dd398a3d1a8d6e3e31dfb0a2719`
is recorded in `reviews/Refactor-launch_deal_strategy_registration.md`; native, security, correctness,
microservice, module-boundary, seeding, convention, strategy-registration, architecture-gate, and
changed-path test-coverage lenses found no issues. The branch contains the clean review checkpoint
`75001d1d1`. During final reconciliation, `origin/main` advanced to
`dc0da9360` through docs/plans-only commits plus the recursive-handoff hook fix. Merge commit
`eda6dbaa6` brought that base current without conflicts. The reviewed API diff's SHA-256 remained
`F8C1A1E3A5FA8CD5329DDDFA7361B39F12B695EC8BCAC3E0CBBA57FD71A43582`, proving the merge did not
change the reviewed implementation. PR #451 is now open against `main`. Its `headRefOid` and
`origin/Refactor/launch_deal_strategy_registration` were both verified at the pushed work head
`b578b74e70fe3ea33821d0a1e3ae345fd690aab6`; no implementation commit is unpushed. The incremental
review of `fb34f37b1..36375ffdf` found no issues, and `b578b74e7` contains only that review artifact and
its plan checkpoint.

The current remote PR head is `bc05263e7bd1015f81fb51ada31e636c5ed7c874`. Incremental review of
`36375ffdf..bc05263e7` found no issues: the two commits only carry the prior review artifact and its
plan-ledger transport checkpoint. Refreshed `origin/main` is 221 commits ahead of the branch, so the
clean reviewed head must now merge current main, rebuild the full API solution, and receive a final
incremental review before its replacement head is pushed.

## Next Steps

Land PR #451 through the repository merge workflow only:

1. Merge current `origin/main` into the clean branch, rebuild `api/Concertable.slnx` to 0 errors, and
   incrementally review the reconciliation commit.
2. This broad booking/payment/settlement refactor requires full API + UI merge-queue E2E; remove any
   skip label or trailer and use the full tier.
3. Follow the PR to a terminal merge result and then own the generated `chore/platform-sync-*` PR to
   green/merged because the source change touches `api/**`.
4. On source merge, transfer recovery state to the required clean close-out worktree and complete the
   plan lifecycle there.

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
- Phase 5 added the Deal-local scoped generic strategy factory and atomic validated builder, migrated
  `DealMapper` and `DealUpdater` to exact vertical registrations, removed the unused marker and stale
  Payment claim, documented the pattern and suffix semantics, and added a self-verifying architecture
  gate in this checkpoint.
- Complete implementation `code-review` covered `43fe1caf4..fb34f37b1` (21 commits) and wrote
  `reviews/Refactor-launch_deal_strategy_registration.md`; no finding survived the confidence filter.
- Pushed reviewed work head `14e97cabf` and opened GitHub PR #451 against `main`.
- Incremental review covered `fb34f37b1..36375ffdf` (23 commits) after base reconciliation and PR
  checkpointing; no finding survived the confidence filter.
- Incremental review covered `36375ffdf..bc05263e7` (2 documentation-only checkpoint commits); no
  finding survived the native or repository-specific confidence filters.

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
- 2026-08-09: after merging `origin/main` at `b5af92fdc` through `514d5a0e5`, the affected Deal and
  Concert unit-test project graphs built with 0 errors before Phase 5 edits.
- 2026-08-09: `dotnet test` for `Concertable.B2B.Deal.UnitTests` passed 41/41 and
  `Concertable.B2B.Concert.UnitTests` passed 132/132 on the final Phase 5 source.
- 2026-08-09: `./scripts/integration.ps1 concert --artifacts-path
  C:\Users\tommy\AppData\Local\Temp\Concertable\launch-deal-strategy-phase5-integration-final`
  passed both projects: B2B Concert 144/144 and Customer Concert 11/11.
- 2026-08-09: `dotnet build api/Concertable.slnx --artifacts-path
  C:\Users\tommy\AppData\Local\Temp\Concertable\launch-deal-strategy-phase5-solution` succeeded with
  0 errors and 9 existing nullable/generated-code warnings.
- 2026-08-09: `git diff --check` passed; `IDealStrategy` and `IStripeValidationStrategy` have no
  remaining `api/` references; the only Deal/Concert keyed lookups are the two module-local factories;
  the final Phase 5 diff review found no open findings.
- 2026-08-09: complete branch review confirmed Phase 5 commit `4d4f44e0a` intact, a clean starting
  worktree, `origin/main` at merge base `43fe1caf4`, and the branch 0 behind. Mechanical scans found no
  added deal-type branch or inline logger, confined keyed lookup/frozen maps to their explicit
  allowlists, and found only same-service unit-test project references. No tests were rerun because the
  implementation state had not changed since its terminal green gates.
- 2026-08-09: final post-review reconciliation found `origin/main` at `98f5cc637`, 16 commits ahead of
  the branch. Those commits change only `plans/dotnet-11/**` and `plans/typed-result/B2B_PROGRESS.md`;
  their path set has no overlap with `43fe1caf4..fb34f37b1`. No PR exists.
- 2026-08-09: merged current `origin/main` at `dc0da9360` through `eda6dbaa6` with no conflicts and
  verified the branch 0 behind. The reviewed API diff SHA-256 was identical before and after the merge;
  the newly merged hook regression suite passed 23/23, including the exact recursive dependency-ledger
  handoff case. No runtime rebuild was required because the reviewed implementation patch did not change.
- 2026-08-09: plan-managed work-head push verified local HEAD,
  `origin/Refactor/launch_deal_strategy_registration`, and PR #451 `headRefOid` all equal
  `14e97cabfb215c7b430d13e133cad8296d02eb31`. The pushed range was new branch
  `dc0da9360..14e97cabf`; PR #451 is open against `main`.
- 2026-08-09: incremental code review covered `fb34f37b1..36375ffdf` (23 commits) and appended its
  result to `reviews/Refactor-launch_deal_strategy_registration.md`. No new finding survived the
  confidence filter; the review watermark is now `36375ffdffe23c0a69e59958a1afc4588ff86e13`.
- 2026-08-09: pushed review-checkpoint commit `b578b74e70fe3ea33821d0a1e3ae345fd690aab6`
  and verified local HEAD, `origin/Refactor/launch_deal_strategy_registration`, and PR #451
  `headRefOid` all equal that work head before this transport checkpoint.
- 2026-08-11: incremental code review covered `36375ffdf..bc05263e7` (2 commits) and appended its
  result to `reviews/Refactor-launch_deal_strategy_registration.md`. No finding survived the native
  or repository-specific confidence filters; the reviewed range changes only the review artifact and
  plan ledger. PR-level build, carve, unit, and integration checks were terminal green at remote head
  `bc05263e7`; merge-queue E2E remained correctly skipped at PR level.
- 2026-08-11: pre-existing platform-sync PR #488 passed build, carve, unit, and integration checks and
  merged through its normal queued path as `130211aa90ae031a31e8b827e2567c3667fbc2b8`.

## Reviews

- Planning document self-reviewed for module ownership, suffix semantics, DI lifetime safety, exact
  coverage validation, accessor separation, union compatibility, and phase verification gates; no open
  planning findings remain.
- Complete implementation `code-review` reviewed `43fe1caf4..fb34f37b1` (21 commits) and wrote
  `reviews/Refactor-launch_deal_strategy_registration.md`, stamped with current native and security
  watermarks. No `NAT`, `SEC`, `BUG`, `MS`, `MB`, `SEED`, `CV`, or missing-test finding remained open;
  every lens disposition is no issue found.
- Incremental review of `36375ffdf..bc05263e7` found no native, correctness, boundary, seeding,
  convention, or changed-path coverage issue; the range is documentation-only and opened no finding ID.
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
- Phase 5 implementation self-review covered module ownership, atomic DI mutation, exact mapper/updater
  coverage, facade/factory lifetimes, keyed-lookup confinement, TPH and wire-shape preservation,
  self-verifying architecture allowlists, removed-symbol coverage, and documentation accuracy; no open
  findings remain.

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
- Deal mirrors Concert with its own Application factory contract and Infrastructure factory/builder;
  there is no runtime reference between the modules and no strategy type in Deal.Contracts.
- The architecture source scanner anchors itself with `CallerFilePath` because short SDK artifact roots
  move both the test process base directory and current directory outside the repository.

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

### 2026-08-09 — Phase 5 base reconciled

- Action: Fetched `origin`, verified the dedicated worktree was clean and correctly owned by the plan
  branch, confirmed no PR or platform-sync blocker existed, checked the other active Deal-adjacent
  worktree for path overlap, merged current `origin/main`, and rebuilt the affected project graphs.
- Evidence: merge commit `514d5a0e5`; `origin/main` at `b5af92fdc`; branch 0 commits behind before
  edits; Deal and Concert affected builds completed with 0 errors.
- Outcome: Phase 5 started against the current base without overlapping unrelated in-flight work.
- Follow-up: Implement and verify Deal-module strategy registration and convention cleanup only.

### 2026-08-09 — Phase 5 architecture gate rooted to source

- Action: Ran the Deal unit gate, traced all failures to the architecture scanner resolving from the
  short artifact output, anchored repository discovery to the compile-time source path, and reran the
  complete project.
- Evidence: initial result 23/41 with 18 identical repository-root failures; final result 41/41.
- Outcome: The architecture guard remains portable when SDK outputs live outside the repository; no
  production behavior changed in response to the harness failure.
- Follow-up: Complete the Concert unit, integration, solution-build, and final-review gates.

### 2026-08-09 — Phase 5 verified and checkpointed

- Action: Added the Deal-local factory and validated vertical builder, migrated mapper/updater
  selection, removed the unused marker and stale Payment claim, documented the committed pattern,
  added architecture enforcement, and completed the final local review.
- Evidence: Deal unit 41/41; Concert unit 132/132; B2B Concert integration 144/144; Customer Concert
  integration 11/11; full `api/Concertable.slnx` build 0 errors; removed symbols absent from `api/`;
  `git diff --check` clean.
- Outcome: All five implementation phases are complete in this checkpoint with no open self-review
  findings; full merge-queue E2E remains the delivery gate, not a duplicate local run.
- Follow-up: Run the implementation code review only.

### 2026-08-09 — post-commit base drift checkpointed

- Action: Verified the Phase 5 commit and clean worktree, then fetched `origin` to reconcile the
  review identity before handoff.
- Evidence: Phase 5 commit `4d4f44e0a`; `origin/main` at `43fe1caf4`; branch 3 commits behind and
  18 commits ahead; no PR. The three new base commits change only repository and plan guidance.
- Outcome: Phase 5 remains complete and verified against its merged base; code review must first
  merge the docs/meta-only base drift and reload the changed guidance.
- Follow-up: Reconcile current `origin/main`, then run the implementation code review only.

### 2026-08-09 — handoff guidance synchronized

- Action: Responded to the plan-handoff gate by running the repository sync workflow, merging current
  `origin/main`, and reloading the newly merged root, prompt, plan, resume, and checkpoint guidance.
- Evidence: merge commit `5411948e2`; `origin/main` at `43fe1caf4`; branch 0 commits behind and
  20 commits ahead; clean worktree before this ledger checkpoint; no PR. The merged base changes only
  docs, hooks, skills, and their tests.
- Outcome: The checkout now uses the same plan-handoff protocol as current main, and the implementation
  review is actionable without a runtime rebuild.
- Follow-up: Run the implementation code review only.

### 2026-08-09 — complete implementation code review checkpointed

- Action: Ran the mandatory native review first, the security review triggered by the deleted
  `Deal.Contracts` marker, and the repository-specific correctness, microservice-isolation,
  module-boundary, seeding, C# convention, strategy-registration, architecture-gate, and changed-path
  coverage lenses over the complete branch diff.
- Evidence: range `43fe1caf4..fb34f37b1` (21 commits); artifact
  `reviews/Refactor-launch_deal_strategy_registration.md`; reviewed and security-reviewed watermarks
  both `fb34f37b17387dd398a3d1a8d6e3e31dfb0a2719`; no findings; branch 0 commits behind
  `origin/main`; no unrelated starting dirt.
- Outcome: The implementation review gate is terminal green. No fixes are required, and PR creation is
  now the single resolved delivery action.
- Follow-up: Push the reviewed branch and open its full-E2E implementation PR only.

### 2026-08-09 — post-review base drift checkpointed

- Action: Reconciled the local review checkpoint against refreshed remote-tracking and GitHub PR state
  before handoff.
- Evidence: review checkpoint `75001d1d1`; `origin/main` at `98f5cc637`; branch 16 commits behind and
  22 commits ahead before this ledger checkpoint; no PR; clean worktree before this ledger update. The
  new base commits are docs/plans-only and have no changed-path overlap with the reviewed branch range.
- Outcome: The implementation review remains terminal green, but current-main reconciliation is now a
  prerequisite of PR creation.
- Follow-up: Merge current `origin/main`, verify the reviewed implementation diff is preserved, then
  push and open the full-E2E implementation PR only.

### 2026-08-09 — post-review base reconciled

- Action: Merged current `origin/main`, including PR #450's recursive-handoff hook fix, into the clean
  feature worktree before PR creation.
- Evidence: base `dc0da9360`; merge commit `eda6dbaa6`; branch 0 commits behind and 24 ahead before this
  ledger checkpoint; no conflicts; reviewed API diff SHA-256 unchanged at
  `F8C1A1E3A5FA8CD5329DDDFA7361B39F12B695EC8BCAC3E0CBBA57FD71A43582`; hook tests 23/23.
- Outcome: The branch is current, the reviewed implementation is unchanged, and the fixed handoff hook
  now scopes claims to actual mutation targets.
- Follow-up: Push the reviewed branch and open its full-E2E implementation PR only.

### 2026-08-09 — reviewed implementation PR opened

- Action: Passed PR preflight, pushed the reviewed work head, verified the remote-tracking ref, opened
  GitHub PR #451, and verified its source head.
- Evidence: preflight GREEN; no existing PR or platform-sync gate; pushed range
  `dc0da9360..14e97cabf`; local, remote-tracking, and PR `headRefOid` all
  `14e97cabfb215c7b430d13e133cad8296d02eb31`; PR
  https://github.com/Concertable/concertable/pull/451 is OPEN against `main`.
- Outcome: The work-head leg and PR creation are verified; this checkpoint is the transport leg for the
  durable PR identity and landing action.
- Follow-up: Land PR #451 through `merge` with full API + UI E2E and own its generated platform sync.

### 2026-08-09 — incremental review reconciled to PR head

- Action: Reviewed every commit after the original implementation watermark through the verified PR
  head, including the main reconciliation and durable PR checkpoint.
- Evidence: incremental range `fb34f37b1..36375ffdf` (23 commits); artifact
  `reviews/Refactor-launch_deal_strategy_registration.md`; no finding IDs were opened, fixed, deferred,
  or superseded; the merged hook regression suite remained 23/23 green.
- Outcome: The reviewed feature implementation is unchanged and the review watermark now matches PR
  #451 head `36375ffdffe23c0a69e59958a1afc4588ff86e13`.
- Follow-up: Transport this review checkpoint, wait for terminal PR checks, then enqueue the verified
  remote head with full API + UI E2E.

### 2026-08-09 — incremental review checkpoint pushed

- Action: Pushed the incremental review artifact and its plan checkpoint, fetched the feature ref, and
  compared all delivery heads.
- Evidence: pushed range `36375ffdf..b578b74e7`; local HEAD, remote-tracking ref, and PR #451
  `headRefOid` all `b578b74e70fe3ea33821d0a1e3ae345fd690aab6`.
- Outcome: The reviewed remote head is fully transported and ready for PR-level checks.
- Follow-up: Transport this ledger checkpoint, then observe terminal checks for PR head `b578b74e7`.

### 2026-08-11 — final checkpoint tail incrementally reviewed

- Action: Re-ran the native and Concertable architecture-aware review over every commit after the
  recorded review watermark and inspected PR #451's terminal checks at its verified remote head.
- Evidence: range `36375ffdf..bc05263e7` (2 commits); artifact
  `reviews/Refactor-launch_deal_strategy_registration.md`; no finding IDs opened; PR build, carve,
  unit, and integration checks all passed at `bc05263e7`.
- Outcome: The pre-reconciliation branch state is review-clean. Current `origin/main` is 221 commits
  ahead, so queue admission remains gated on base reconciliation, rebuild, and final incremental review.
- Follow-up: Merge current `origin/main`, rebuild the API solution, and incrementally review the new head.

### 2026-08-11 — existing platform-sync gate cleared

- Action: Inspected the repository's only open platform-sync PR, verified every required check green,
  confirmed it was already queued, and followed it to merge before reconciling PR #451's base.
- Evidence: platform-sync PR #488 for `0.1.0-alpha.0.917`; merge commit
  `130211aa90ae031a31e8b827e2567c3667fbc2b8`; no failed check.
- Outcome: No red or pending platform-sync gate remains ahead of PR #451's base reconciliation.
- Follow-up: Fetch and merge current `origin/main`, then rebuild and incrementally review the result.

## Resume prompt

```
cd C:\Users\tommy\source\repos\Concertable.worktrees\Refactor\launch_deal_strategy_registration
Read @plans/launch/DEAL_STRATEGY_REGISTRATION_PLAN.md and @plans/launch/DEAL_STRATEGY_REGISTRATION_PROGRESS.md and do what its `## Next Steps` says.
```
