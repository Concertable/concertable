# Customer non-Payment outcomes and lookups progress

- Plan: `plans/typed-result/CUSTOMER_OUTCOMES_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_customer-outcomes`
- Branch: `Feature/typed-result_customer-outcomes`
- PR: not opened
- Dependency/package gates: owned Kernel foundation PR #290 and platform sync #291 are shipped; no Payment/B2B package dependency; platform-sync PR #373 shipped `0.1.0-alpha.0.814` green in merge commit `9169107c0`; PR #282 remains the exclusive open owner of Ticket/Concert/Customer Payment work and is not a dependency
- Last reconciled: 2026-08-06T10:52:00+01:00 from current `origin/main` `48bd0eaf5e`, local refs/worktrees, GitHub PR metadata, PR #282 ownership, green platform-sync PR #393, merge commit `9a3a947b7`, and the complete Phase 3 verification gate

## Current state

Phases 1 through 3 are complete in local commits after this checkpoint. User profile absence is
`Option<CustomerDto>` at the application boundary; repository and module multi-user lookups are
materialized `IReadOnlyList` values; the existing Preference and UserClaims consumers remain
wire-compatible; and `SaveLocationAsync` remains plain. User unit coverage passed 14/14, direct
Customer User integration passed 6/6, and the `user` integration wrapper passed both discovered
projects: B2B User 3/3 and Customer User 6/6.

Every Phase 3 gate is green: User unit 14/14, User integration 6/6 through the module wrapper,
Shared.Api 51/51, Release solution build with 0 errors, isolated Customer carve of 36 package-clean
projects with 0 errors, scoped carrier/collection/ownership inventories, and `git diff --check`.
The long worktree's default Debug native-output path reaches 259 characters and prevents Windows
from loading `Microsoft.Data.SqlClient.SNI.dll`; the supported short `--artifacts-path
C:\tmp\CustomerUserIntegrationArtifacts` route passed the isolated reproduction and full wrapper.

Preference now owns exact duplicate-create,
missing-update, and foreign-update outcomes; ordinary user-preference absence is `Option`; returned
queries and mappers are materialized `IReadOnlyList` values; and successful updates return the
tracked entity without a nullable re-read. The HTTP terminal preserves 200/204 reads, 201 create with
no body, 200 update, and typed 403/404/409 ProblemDetails.

Every Phase 2 gate is green: Preference unit 19/19, Preference integration 7/7, Shared.Api 51/51,
Release solution build with 0 errors, isolated Customer carve of 36 package-clean projects with 0
errors, scoped carrier/collection/ownership inventories, and `git diff --check`. The branch is 0
commits behind `origin/main` `48bd0eaf5e` and contains no excluded Ticket, Concert, Payment,
purchase, checkout, shared Kernel API, model, or migration edit. Local E2E was correctly not run; the
final PR requires full merge-queue E2E.

The implementation owns Review, Preference, User, Venue, and Artist only. PR #282 /
`Feature/TypedResultMigrationPhase2` owns every Ticket, Concert, Customer Payment client/mock,
purchase/checkout, and related coverage path; those paths must remain absent from this branch's diff.
The scoped modules have no production FluentResults reference, while Ticket/Concert still require the
shared `Directory.Packages.props` version entry, so that entry stays.

## Next Steps

Implement Phase 4 — Venue and Artist detail Options. Convert only the application services to Option,
preserving nullable repositories and the existing 200/404 HTTP endpoints. Add the owned Venue and
Artist unit/integration test projects, solution wiring, and integration-runner discovery described by
the plan.

Run the affected unit and integration suites through the `integration-debug` workflow, Shared.Api,
Release solution build, the CI-equivalent Customer carve, scoped carrier/collection/ownership
inventories, and `git diff --check`. Re-fetch and reconcile only if the branch is behind and the tree
can be preserved safely; re-check PR #282 ownership and platform sync. Do not run local E2E. Update
the plan and ledger, check off Phase 4, and commit the green checkpoint locally. Do not push. End with
the pointer-only resume prompt from this ledger.

## Completed work

- Audited the five scoped modules' real application/module/repository contracts, controllers,
  consumers, package references, seed-backed integration fixture, and existing tests.
- Audited the shipped Kernel `Result`/`Option` implementation, Shared.Api terminals and architecture/
  terminal tests, and the current typed-operation conventions.
- Inspected GitHub PR #282, its remote files/head, and the further local
  `Feature/TypedResultMigrationPhase2` diff/ledger to make the Ticket/Concert/Payment exclusion exact.
- Created the implementation-ready plan and this progress ledger in this commit.
- Completed Phase 1 Review-owned typed create outcomes with exact error, service, validator,
  exception/cancellation, and HTTP ProblemDetails coverage in this commit.
- Completed Phase 2 Preference-owned create/update Results, user-preference Options, materialized
  query lists, unchanged HTTP terminals, owned unit/integration projects, solution wiring, and
  integration-runner discovery in this commit.
- Completed Phase 3 User profile Option, materialized repository/module lists, existing-consumer
  compatibility, HTTP coverage, and owned unit coverage in this commit.

## Verification

- `git fetch origin --quiet`: refreshed origin before branch creation and again before the plan edit.
- Branch/worktree/PR audit: no pre-existing branch, worktree, or PR owned this slice.
- Platform gate at creation: PR #367 had all checks green. PR #372 later passed every check and
  closed unmerged; final reconciliation found successor PR #373 open with several integration jobs
  pending and no failed checks, so no red platform-sync gate blocked planning.
- Base reconciliation: the initially fresh `origin/main` base advanced during research; the clean
  worktree was fast-forwarded to `f04025c5e` before edits, then the sole unpushed planning commit was
  rebased onto `e419966a9` after three more commits landed. Its final parent equals `origin/main` and
  `git rev-list --count HEAD..origin/main` is 0.
- Planning verification: `git diff --check` is required immediately before the planning commit.
- No build or tests were run because this context changed documentation only and deliberately did not
  implement a migration phase.
- Phase 1 Review unit suite:
  `dotnet test api/Concertable.Customer/src/Modules/Review/Tests/Concertable.Customer.Review.UnitTests/Concertable.Customer.Review.UnitTests.csproj --configuration Release`
  passed 30/30 with 0 failed and 0 skipped.
- Phase 1 working tree: `git diff --check` passed. The changed-path inventory contains Review-owned
  production/tests plus this ledger only; no Ticket, Concert, Payment, purchase, or checkout path is
  changed.
- Integration pre-flight: the prior `docker ps` attempt produced no daemon response and timed out
  after 34 seconds. On resume, `docker ps` returned successfully with an empty container list, so the
  Review integration suite may start.
- Base sync: stashed the exact Review working tree including untracked files, merged the five fetched
  `origin/main` commits as `786bd3267`, restored the stash without conflicts, and verified the branch
  is 0 commits behind with the same Review-only dirty path inventory and a clean `git diff --check`.
- Review integration workflow: the first attempt was cancelled without a verdict after its wrapper
  was stopped during severe unrelated host contention; its child and SQL container exited. The one
  clean rerun through `scripts/integration.ps1 review` passed 11/11 with 0 failed and 0 skipped.
- Review Release unit suite rerun passed 30/30 with 0 failed and 0 skipped.
- Shared.Api Release architecture/terminal suite passed 51/51 with 0 failed and 0 skipped.
- `dotnet build api/Concertable.slnx --configuration Release` succeeded with 0 errors and 6 warnings.
- CI-equivalent Customer carve copied the candidate `api/Concertable.Customer` tree to an isolated
  `C:\tmp\CarveCustomer-*` directory, discovered and built all 36 non-test/non-AppHost projects from
  package references with `-p:MinVerSkip=true`, and succeeded with 0 errors; the verified temporary
  directory was removed afterward.
- Scoped inventories found functional carriers only in the Review Application interfaces and
  Infrastructure implementations, no carrier in HTTP DTOs/events/persistence, no explicit nullable
  or non-pagination collection return in Review, and no FluentResults, Dunet union, or HTTP exception
  in the typed-result slice.
- Final ownership inventory against current `origin/main` found only Review-owned production/tests
  plus this plan/ledger, with no Ticket, Concert, Payment, purchase, checkout, shared Kernel API,
  migration, or model path. `git diff --check` passed.
- `origin/main` advanced during verification by workflow-only PR #374. The verified tree was stashed,
  merge `4a13785e4` integrated those two commits, and the tree restored without conflicts; the branch
  is 0 commits behind `0ed29d8f0`. Product/build inputs did not change, so the green gates remain valid.
- Local E2E was not run because the plan rules assign the required full tier to the merge queue.
- Phase 2 Preference unit suite first compiled all new projects and ran 19 cases; two assertions
  expected the opposite enum sort order while production returned the correct values. After fixing
  only those expectations and tightening provider-fault coverage, the final Release run passed 19/19
  with 0 failed and 0 skipped.
- Preference integration pre-flight: `docker ps` returned successfully. The background workflow's
  diagnostic relaunch collided with the already-active per-project log and exited before tests; the
  original `scripts/integration.ps1 preference` run continued normally and passed 7/7 with 0 failed
  and 0 skipped against Testcontainers SQL.
- Preference integration coverage preserves 200/204 reads, 201 create with no response body, 409
  duplicate create, 404 missing update, 403 foreign update, and 200 successful update.
- Shared.Api Release architecture/terminal suite passed 51/51 with 0 failed and 0 skipped.
- `dotnet build api/Concertable.slnx --configuration Release` succeeded with 0 errors and 6
  pre-existing warnings.
- The CI-equivalent Customer carve copied the final production candidate to isolated
  `C:\tmp\CarveCustomer-2965ff86879c4181bc753dffb36fb0a0`, discovered and built all 36
  non-test/non-AppHost projects from package references with `-p:MinVerSkip=true`, and succeeded
  with 0 errors; the temporary directory was removed afterward.
- Scoped inventories found Results/Options only in Preference's application interface and
  infrastructure service, no functional carrier in DTOs/requests/events/domain/persistence, only the
  allowed nullable repository single-item lookups and inherited `GetAllAsync` enumerable, and no
  FluentResults, Dunet, or HTTP exception in the typed slice.
- Final ownership inventory against current `origin/main` contains only Review and Preference-owned
  production/tests plus solution discovery, `scripts/integration.ps1`, and this plan/ledger. No
  Ticket, Concert, Payment, purchase, checkout, shared Kernel API, migration, or model path is
  changed; `git diff --check` passed.
- Final `git fetch origin --quiet` left the branch 0 commits behind `origin/main` `0ed29d8f0`.
- Local E2E was not run because the final behavior-changing PR requires the full merge-queue tier.
- Phase 3 User unit suite passed 14/14 with 0 failed and 0 skipped, covering profile Some/None,
  populated/empty materialized module lists, successful location/profile mapping, and the
  missing-user invariant.
- The first `scripts/integration.ps1 user` run passed the incidental B2B User project 3/3, then the
  Customer project exposed controller compile errors because MVC method groups did not convert both
  `Option.Match` arms to `ActionResult<CustomerDto>`. Explicit context-typed lambdas fixed the
  compile defect.
- The direct Customer User integration retry compiled the corrected candidate, then all six tests
  stopped before fixture startup when the default Debug native-output path reached 259 characters.
  Windows returned `0x800700CE` while loading `Microsoft.Data.SqlClient.SNI.dll`; an isolated Debug
  reproduction confirmed the same native-loader failure.
- Direct Customer User integration in Release passed 6/6. The isolated Debug reproduction passed
  1/1 with `--artifacts-path C:\tmp\CustomerUserIntegrationArtifacts`, and the full wrapper through
  the same short root passed B2B User 3/3 and Customer User 6/6.
- Shared.Api Release architecture/terminal tests passed 51/51 with 0 failed and 0 skipped.
- `dotnet build api/Concertable.slnx --configuration Release` succeeded with 0 errors and 8
  pre-existing warnings.
- The CI-equivalent Customer carve copied the final production candidate to an isolated
  `C:\tmp\CarveCustomer-*` directory, discovered and built all 36 non-test/non-AppHost projects from
  package references with `-p:MinVerSkip=true`, and succeeded with 0 errors; the verified temporary
  directory was removed afterward.
- Scoped inventories found User Option only at the application/service/HTTP terminal, only the
  preserved nullable repository single-item lookup, materialized multi-item list contracts, and the
  planned `SaveLocationAsync` missing-user invariant exception. No excluded PR #282-owned path is in
  the branch diff, and `git diff --check` passed.
- The dirty Phase 3 tree was preserved across the 65-commit base advance and a later 12-commit
  documentation-only advance. Merge commits `6aa81a8130` and `9a3a947b7` leave the branch 0 commits
  behind `origin/main` `48bd0eaf5e`; the Review conflict preserves current authorization attributes
  and the typed create terminal.
- PR #282 remains open at head `26ed63b896` and exclusively owns Ticket/Concert/Customer Payment.
  Platform-sync PR #393 is open with every build, carve, unit, and integration check green. This
  branch has no PR and has not been pushed.
- Local E2E was not run because the final behavior-changing PR requires the full merge-queue tier.

## Reviews

No implementation review has run. The completed plan is based on direct repository and PR evidence;
Phase 5 requires `/code-review` before delivery.

## Decisions, discoveries, blockers, and deviations

- The current conventions explicitly forbid manufacturing Results for uniformity. Review create and
  Preference create/update have caller-actionable expected failures; User save-location absence is an
  invariant behind authorization, and Venue/Artist absence is ordinary Option vocabulary.
- Persistence single-item nullability remains correct. Conversion occurs at the application/service
  boundary; the Review boundary adapts the existing Ticket nullable contract without changing it.
- Review creation currently omits the eligibility checks already used by its capability query. The
  migration makes not-yet-reviewable and already-reviewed states explicit before persistence while
  leaving provider/unique-index races as exceptions.
- Preference's unique UserId index is the evidence for an expected duplicate-create conflict. The
  database remains authoritative; provider/race exceptions are not caught into the Result.
- Missing Preference, Venue, and Artist test projects are added under their own modules rather than
  hiding their coverage in Review/User. The existing Customer integration fixture and seeders already
  support them.
- No model change or migration is needed. The final PR is multi-module and behavior-changing, so the
  merge queue must run full E2E.
- Preference create checks the normal existing-row path before persistence while leaving the unique
  index authoritative for races; add/save provider failures still propagate.
- Preference update returns the tracked entity after save. The former nullable re-read and null
  suppression were removed instead of translating a post-save invariant violation into an outcome.
- The create controller uses the shared Result HTTP terminal with a 201 success arm that deliberately
  drops the service payload, preserving the existing empty response body.
- PR #282 is open at remote head `26ed63b8` and owns the excluded Ticket/Concert/Payment slice. Its
  local branch contains substantial unpushed owned-result work; none of it may be copied or modified
  here.
- There are no blocking package dependencies. A platform-sync PR that is pending but not red does not
  block local planning; any red sync discovered before implementation must be resolved first.
- In this long worktree, default Debug output puts the x64 SqlClient SNI DLL at a 259-character path,
  which Windows rejects with `ERROR_FILENAME_EXCED_RANGE`. Use a short supported `--artifacts-path`
  for integration runs from this worktree; the binaries and application behavior are otherwise green.

## Event log

### 2026-08-05 — ownership and platform gate established

- Action: Fetched origin and audited worktrees, matching branches, open PRs, and the open platform-sync gate before creating the requested worktree.
- Evidence: no matching owner existed; PR #367's checks were all green; PR #282 was the distinct Ticket owner.
- Outcome: Created `Feature/typed-result_customer-outcomes` in the sibling worktree from fresh `origin/main`.
- Follow-up: Inspect the full planning sources and live code.

### 2026-08-05 — repository and PR evidence audited

- Action: Read the required planning/architecture/convention documents and owned functional implementation/tests; inspected all five modules end to end, their frontend/HTTP consumers, project references, test fixture, CI carve, and PR #282's remote and local branch state.
- Evidence: source inventory and call-site searches on `origin/main`; GitHub PR #282 metadata; local `Feature/TypedResultMigrationPhase2` diff and ledger.
- Outcome: Resolved the exact Result, Option, list, wire-boundary, test-project, and ownership design recorded in the plan.
- Follow-up: Reconcile base/gates and make the planning checkpoint durable.

### 2026-08-05 — current-main reconciliation and plan checkpoint

- Action: Re-fetched origin after research, fast-forwarded the still-clean worktree over nine new base commits, created the plan/ledger, then rebased that sole unpushed commit over three further main commits that landed before handoff.
- Evidence: the final plan commit's parent is `origin/main` `e419966a9`; behind count is 0; platform-sync PR #372 closed after all checks passed and successor PR #373 had no failed check; this commit contains only `CUSTOMER_OUTCOMES_PLAN.md` and `CUSTOMER_OUTCOMES_PROGRESS.md`.
- Outcome: Planning is complete on the current base and Phase 1 is implementation-ready.
- Follow-up: Execute `## Next Steps`; do not push the planning commit.

### 2026-08-05 — Phase 1 implemented; verification paused at Docker pre-flight

- Action: Reconciled the branch, PR #282 ownership, and green platform-sync PR #373; implemented
  Review create Results, shared eligibility evaluation, Shared.Api termination, and unit/integration
  coverage without touching excluded module paths.
- Evidence: Review unit tests passed 30/30; `git diff --check` passed; the working-tree path inventory
  is Review-only. The required `docker ps` integration pre-flight timed out after 34 seconds without
  a daemon response.
- Outcome: Phase 1 is implemented but remains uncommitted and incomplete because its integration and
  broader verification gates have not run.
- Follow-up: Start Docker Desktop, resume the Review `integration-debug` run, complete every Phase 1
  gate, update the plan/ledger, and commit the green checkpoint without pushing.

### 2026-08-05 — platform sync landed and Docker recovered

- Action: Refetched `origin`, reconciled PR #282 and platform-sync PR #373, inspected the five new
  base commits, and reran the mandatory Docker pre-flight with host access.
- Evidence: PR #373 merged green as `9169107c0` at version `0.1.0-alpha.0.814`; PR #282 remains open
  at `26ed63b8`; this branch has no PR; `docker ps` returned successfully; `origin/main` is five
  commits ahead and changes only the platform pins plus merge-skill documentation relative to the
  branch base.
- Outcome: The package gate and Docker prerequisite are green. The uncommitted Review tree remains
  intact and must be restored over current `origin/main` before verification.
- Follow-up: Integrate `origin/main`, then run the Review integration-debug workflow and every
  remaining Phase 1 gate.

### 2026-08-05 — current-main integration completed

- Action: Stashed the exact tracked and untracked Review tree, merged the five fetched `origin/main`
  commits into the clean branch, and restored the stash.
- Evidence: merge commit `786bd3267`; behind count 0; no stash conflicts; the restored dirty-path
  inventory is Review-only and `git diff --check` is clean.
- Outcome: Phase 1 is preserved on the current base and ready for integration verification.
- Follow-up: Run the Review integration-debug workflow and every remaining Phase 1 gate.

### 2026-08-05 — Phase 1 Review outcomes completed

- Action: Ran the Review integration-debug workflow, reran Review unit coverage, completed the
  Shared.Api, Release solution, isolated Customer carve, scoped carrier/ownership, and diff gates,
  then reconciled the workflow-only base advance that landed during verification.
- Evidence: Review integration 11/11; Review unit 30/30; Shared.Api 51/51; solution and 36-project
  Customer carve both 0 errors; scoped inventories clean; `git diff --check` clean; branch 0 behind
  `origin/main` `0ed29d8f0`; PR #282-owned paths absent.
- Outcome: Phase 1 is complete in this commit with no local E2E duplication and no push.
- Follow-up: Implement Phase 2 Preference outcomes, Options, lists, tests, and its full local gate.

### 2026-08-05 — Phase 2 Preference outcomes completed

- Action: Implemented Preference create/update Results, user-preference Options, materialized query
  lists, Shared.Api HTTP termination, owned unit/integration projects, and solution/script discovery;
  then ran the complete Phase 2 local gate through the integration-debug workflow.
- Evidence: Preference unit 19/19; Preference integration 7/7; Shared.Api 51/51; Release solution and
  36-project Customer carve both 0 errors; scoped carrier/collection/ownership inventories and
  `git diff --check` clean; branch 0 behind `origin/main` `0ed29d8f0`; PR #282-owned paths absent.
- Outcome: Phase 2 is complete in this commit with provider/cancellation faults still exception-based,
  unchanged 201/200/204 success payloads, no local E2E duplication, and no push.
- Follow-up: Implement Phase 3 User Option and module-list normalization with its full local gate.

### 2026-08-05 — Phase 3 implemented; verification paused at Docker

- Action: Implemented User profile Option and eager multi-user lists, added service/module unit
  coverage, strengthened existing profile/location HTTP assertions, and entered the
  `integration-debug` workflow.
- Evidence: User unit tests passed 14/14; the incidental B2B User integration project passed 3/3;
  the Customer project compile defect in the MVC Option terminal was diagnosed and fixed; the
  corrected project compiled before all six cases stopped at fixture startup with the same
  Testcontainers Docker-endpoint error; `docker ps` then timed out.
- Outcome: Phase 3 code is implemented and unit-green but remains uncommitted and incomplete. No
  Customer integration scenario executed, and the broader build/carve/inventory gates have not run.
- Follow-up: Start Docker Desktop, resume the exact integration and remaining Phase 3 gates from
  `## Next Steps`, then check off and commit only when every gate is green.

### 2026-08-06 — Phase 3 User outcomes completed

- Action: Reconciled the branch with current main, diagnosed the Windows native-loader failure in
  the long Debug output path, reran the User integration wrapper through a short artifacts root, and
  completed every remaining Phase 3 gate.
- Evidence: User unit 14/14; direct Customer User integration 6/6; wrapper B2B User 3/3 and Customer
  User 6/6; Shared.Api 51/51; Release solution and 36-project Customer carve both 0 errors; scoped
  inventories and `git diff --check` clean; branch 0 behind `origin/main` `48bd0eaf5e`; PR #282-owned
  paths absent; platform-sync PR #393 green.
- Outcome: Phase 3 is complete in this commit with no local E2E duplication and no push.
- Follow-up: Implement Phase 4 Venue and Artist Options, owned tests, and its full local gate.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_customer-outcomes
Read @plans/typed-result/CUSTOMER_OUTCOMES_PLAN.md and @plans/typed-result/CUSTOMER_OUTCOMES_PROGRESS.md and do what its `## Next Steps` says.
```
