# Customer non-Payment outcomes and lookups progress

- Plan: `plans/typed-result/CUSTOMER_OUTCOMES_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Roadmap item: `typed-result/customer-outcomes`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_customer-outcomes`
- Branch: `Feature/typed-result_customer-outcomes`
- PR: [#425](https://github.com/Concertable/concertable/pull/425) — open, non-draft, head `e60219f7d`
- Dependency/package gates: exact `0.1.0-alpha.2` is published for `Reunion`,
  `Reunion.Validation`, `Reunion.Errors`, and `Reunion.AspNetCore`. Payment `0.1.0-alpha.0.894`
  publication and platform-sync
  PR #463 are terminal. Customer Ticket replacement PR #475 and platform-sync PR #479 are terminal
  on platform `.910`; historical PR #282 is closed as superseded and the shipped Ticket slice remains
  excluded.
- Last reconciled: 2026-08-10 against `origin/main` `d916e95cf`, this current-main merge, PR #425
  head `e60219f7d`, and terminal production evidence from the Reunion integration owner

## Current state

Phases 1 through 4 and Phase 5's implementation, verification, review, and finding-fix gates are
complete. Phase 6 now owns the requested DI-validator migration before delivery. The branch owns only
Review, Preference, User, Venue, and Artist outcomes/lookups; merged replacement PR #475's Ticket,
Concert, Customer Payment, purchase, and checkout slice remains excluded. Review and
Preference use named Dunet cases with exact stable definitions, application absence terminates as
`Option<T>`, and scoped multi-item outputs are materialized `IReadOnlyList<T>` values. Existing HTTP
status and payload contracts remain unchanged.

All three review findings are fixed in separate commits: `CV1` in `3ab6604c4`, `CV2` in
`312400220`, and `CV3` in `b8e13b3ff`. The incremental reviews through `6d994b78c` are clean and the
spent review artifact is removed.

Platform-sync PR #420 shipped `0.1.0-alpha.0.853` as `372be1041b20`. That exact `origin/main` is
merged into the branch as `bf20d17ce`, leaving the branch 0 commits behind. Because the base changed
Customer package and integration-fixture inputs, the final current-main gate was rerun: Docker's
fresh-container data path is healthy; Customer Review 12/12, Preference 7/7, User 6/6, Venue 2/2,
and Artist 2/2 integration tests pass (29/29 total); Shared.Api passes 60/60; and the Release solution
build succeeds with 0 errors and 8 current-base warnings. Local E2E remains intentionally deferred to
the full merge-queue tier.

PR preflight is green: the worktree is clean, the branch is 0 behind `origin/main`, no source PR or
open platform-sync PR exists, and no package cut-over is incomplete. The feature branch did not
previously exist remotely. Push leg one published the 28-commit `origin/main..75263c0b8` work range,
and the fetched remote-tracking head exactly matches `75263c0b8`.

Push leg two published the isolated ledger transport checkpoint `e60219f7d`, and the fetched remote
branch exactly matches local HEAD. GitHub PR #425 is open and non-draft against `main` at that exact
head. Its label set is empty, so the required full merge-queue E2E tier has no skip override.

Fresh `origin/main` `1043a9178` is merged into the branch. PR #425 remains open and non-draft at
`e60219f7d`; the pre-merge local tail after that PR head changed only this ledger. The merge conflict
was confined to this plan pair; no runtime conflict occurred.

PR #425 can be prepared locally now against published Reunion `.1` because its scope excludes
Payment.Client. Preserve its existing review history and remote head; update the PR once after the
current-main conversion, verification, and incremental review are complete.

The direct Reunion conversion is implemented in the working tree across Review, Preference, User,
Venue, and Artist. All 20 compiling projects that consume Reunion APIs directly own the matching
`Reunion`, `Reunion.Errors`, or `Reunion.AspNetCore` package; the scoped source contains no legacy
Kernel functional/error or Shared.Api terminal namespace. Existing HTTP statuses, bodies, and
Review's `CreatedAtAction` location are preserved.

Docker-independent verification is green: the Customer Release solution builds with 0 errors and 3
existing warnings; the five scoped unit suites pass 67/67; Shared.Api passes 60/60; and the isolated
36-project Customer carve builds with 0 errors. The ownership/scope audit and `git diff --check` also
pass. The mandatory integration pre-flight timed out without a Docker daemon response, so no current
candidate integration suite started. The conversion remains uncommitted and PR #425 remains
unchanged until Docker recovers and the integration plus incremental-review gates pass.

On resume, `docker ps` succeeds against Docker Desktop with no running containers. The integration
gate is actionable again. Fresh `origin/main` `d66b780cd` is two documentation-only commits ahead;
those commits change only three `TECH_DEBT.md` files outside this scope, so the preserved candidate
can complete integration verification before its required current-main reconciliation.

The resumed integration gate is green. Customer Review 12/12, Preference 7/7, User 6/6, Venue 2/2,
and Artist 2/2 pass (29/29 total); the module wrappers also pass B2B User 3/3, Venue 25/25, and Artist
17/17 (45/45). Each project used a fresh Testcontainers SQL container and every container was removed.

The complete 45-path candidate was stashed, `origin/main` `d66b780cd` merged as `2180c19c0`, and the
stash restored without conflict. The branch is 0 behind current main and the exact 45 dirty paths are
present again; the merge itself changed only the three unrelated `TECH_DEBT.md` files identified in
the pre-merge audit.

The current-main local gate is fully green: the complete API Release solution builds with 0 errors
and 5 existing warnings; the scoped unit suites pass 67/67; Shared.Api passes 60/60; the isolated
36-project Customer carve builds with 0 errors; and the direct-package, carrier, nullability,
collection, terminal, excluded-path, and whitespace audits all pass.

The committed conversion at `d4a5bb502` passed a fresh full code review. Its only finding, `NAT1`,
was fixed in `7a7e07e86` by removing stale direct Kernel references from Preference.Application and
User.Application. The incremental review of that fix is clean. Fresh platform-sync main
`6f4a5cc3e` was then merged as `7a854cd4c`; its only incoming change advances the five service pins
from `.890` to `.892`, and the incremental review of that merge is clean.

The exact `.892` head is green: the Release solution builds with 0 errors and 9 existing warnings;
the five scoped unit suites plus Shared.Api pass 127/127; the five integration wrappers pass 74/74
across eight projects; and the isolated 36-project Customer carve builds with 0 errors. The spent
review work order is removed in the work-head checkpoint before the PR update.

The actual work head is committed locally as `b021ebbbe`. Its push was rejected before execution
because the environment requires direct user authorization for the remote mutation; the fetched
remote branch and PR #425 therefore remain unchanged at `e60219f7d`.

The push checkpoint is superseded by the new Phase 6 requirement: PR #425 must not be updated with
the pre-validation candidate. The scoped audit found one custom DI validator in this plan,
`IReviewValidator`; it still returns `Result<TicketSummary, CreateReviewError>` and booleans. The
Review request validator is FluentValidation and remains outside this conversion.

The Phase 6 package gate is open. Production `Reunion.Validation` `0.1.0-alpha.1` comes from merged
source `1500270`, carries a valid NuGet.org repository signature, byte-matches the verified candidate
across all nine non-signature entries, and clean-restores with exact `Reunion` and `Reunion.Errors`
`.1`. Current `origin/main` `d916e95cf` is merged in this checkpoint. The only conflict was Customer
central package management; its resolution keeps platform pin `.903`, the branch's direct Reunion and
Shouldly ownership, and one entry per package. PR #425 remains unchanged at `e60219f7d`.

The PR #470 domain-outcome audit found one branch-owned classification correction. The only
production `DomainException` in the five-module scope is `ReviewEntity.Create` rejecting stars outside
1–5, while `CreateReviewRequestValidator` duplicates that rule at HTTP and direct service callers can
still reach the throw. Phase 6 now makes the domain factory return structured validation and maps it
to an operation-owned `CreateReviewError.Invalid`. Preference, User, Venue, and Artist domain methods
have no rejecting transition; their bool/Option/nullable/list outcomes already match capability,
absence, persistence, and collection semantics. Missing User after its owning save plus repository,
provider, cancellation, and identity faults remain exceptions. Shared HTTP exception policy and
other Customer modules stay with their owners or the future global audit.

Phase 6 is implemented and locally verified in this commit. Review owns the Ticket lookup once,
maps missing/period/reviewed/star-range outcomes to its operation error, and keeps collaborator,
provider, identity, and cancellation faults exceptional. All four custom `IReviewValidator` methods
return structured `ValidationResult`; Artist/Venue capability endpoints collapse those results to
their existing booleans. `ReviewEntity.Create` returns structured star validation, with the seed
factory explicitly enforcing its valid-data invariant. The exact public HTTP contracts remain
unchanged.

The current candidate passes Review unit 40/40, all five scoped unit suites 77/77, Shared.Api 60/60,
and all five integration wrappers 74/74 across eight projects. The Release solution builds with 0
errors and 7 existing warnings, and the isolated 36-project Customer carve builds with 0 errors.
Direct-package, DI-validator, carrier, nullability, collection, terminal, excluded-path, domain-
exception, whitespace, and Docker-cleanup inventories pass. The first Review integration attempt
exposed current platform `.903` loading `Reunion.Errors` `.2` beside branch-owned `.1`; direct `.2`
ownership plus migration of the three branch-owned unions to the `.2` definition factories fixed the
runtime incompatibility, and the complete integration gate then passed. PR #425 remains unchanged at
`e60219f7d`; the freshly observed `origin/main` is nine commits ahead and is intentionally reserved
for the required post-review reconciliation.

The fresh full review and security pass cover `d916e95cf..5cfdb9427` across 43 commits and the full
net branch diff. `reviews/Feature-typed-result_customer-outcomes.md` is stamped at the exact work head.
The service/module-boundary, seeding, C# convention, changed-path coverage, and security lenses are
clean. `NAT1` is fixed in this commit by adding Review.Infrastructure's missing direct
`Reunion.Errors` ownership; its focused Release build succeeds with 0 errors and one existing warning.
`NAT2` is fixed by replacing Preference's check-then-insert with an atomic repository insert that
translates the unique-`UserId` constraint into the typed conflict; Preference unit tests pass 19/19.
`NAT3` is fixed in this commit with the equivalent atomic Review insert and unique-`TicketId`
translation; Review unit tests pass 43/43, including duplicate, fault, and cancellation paths. Docker
briefly became unresponsive before the focused integration reruns, so the integration-debug rule
correctly prevented a suite start. The single planned retry then proved a stable fresh-container data
path; Preference passes 7/7 and Review passes 12/12 through their integration wrappers, including the
existing duplicate-conflict scenarios, and the post-run Docker inventory is empty.

The incremental review of `5cfdb9427..958c05c5a` is clean. It covers all three finding-fix commits
and their verification checkpoints; no correctness, isolation, boundary, seeding, convention,
changed-path coverage, or security-sensitive-path finding was added. The review watermark now names
the exact verified work head.

Current `origin/main` `aab321bd2` is merged as `c021d26c9`, leaving the branch 0 commits behind while
PR #425 remains unchanged at `e60219f7d`. The incoming product delta is the terminal Ticket slice,
platform `.910`, and AppHost/Search hosting updates. The sole conflict was Customer central package
management; its resolution uses `.910`, retains the scoped integration projects' `Shouldly` version,
and accepts main's removal of unused `FluentResults`. The incremental native, security, architecture,
and coverage review of `958c05c5a..c021d26c9` is clean and both review watermarks name that merge
head.

The deterministic current-main gate is green on the mixed alpha.1/alpha.2 candidate: the Release solution builds with 0 errors and 8
existing warnings; the five scoped unit suites pass 80/80; Shared.Api passes 60/60; the isolated
36-project Customer package-consumer carve builds with 0 errors and its temporary directory was
removed; scoped carrier, validation, excluded Ticket/Concert/Payment, legacy namespace, and
whitespace inventories pass. The mandatory fresh-container Docker health check timed out after five
minutes and a subsequent read-only `docker ps` also timed out. Docker Desktop has one non-responding
process, so no current-main integration suite was started and the two-leg PR update remains blocked.
The new alpha.2 package/API checkpoint is independently actionable before that Docker gate: align all
four used package families, adopt only unambiguous raw-payload or exact named-case conversions in the
owned scope, and rerun every Docker-independent gate first.

## Next Steps

1. Align every existing Customer `Reunion`, `Reunion.Validation`, `Reunion.Errors`, and
   `Reunion.AspNetCore` reference to `0.1.0-alpha.2`, applying the new construction surface only where
   intent remains compile-time explicit.
2. Run every Docker-independent package, build, unit, architecture, carve, scope, and whitespace gate.
3. Then restart Docker Desktop if needed, require `scripts/docker-health.ps1` to pass once, run the five
   Release integration wrappers, confirm 74/74 and an empty Docker inventory, reconcile any new base
   advance, incrementally review it, and perform the plan-managed two-leg push.

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
- Completed Phase 4's current-convention Review/Preference error migration, Venue/Artist service
  Options and unchanged HTTP terminals, owned tests, solution/runner discovery, and full local gate
  in this commit.
- Committed the Phase 5 execution and full-review checkpoint as `23461784c`, then merged current
  `origin/main` `b66325acd` as `fc9f69407` while preserving both branches' Shared.Api guards.
- Fixed `CV1` in `3ab6604c4` and fixed `CV2` in this commit without weakening the merged Shared.Api
  definition-shape guard.
- Incrementally reviewed `de2b8c163..312400220` across 91 commits; the current-main merge is
  unchanged from `origin/main` outside its resolved Shared.Api guard conflict, and `CV3` is open.
- Fixed `CV3` in this commit by rejecting discard and named `var` catch-all arms only within the
  bounded Dunet definition switch, with focused unrelated-switch coverage.
- Incrementally reviewed `312400220..b8e13b3ff` across two commits with no new finding.
- Merged current `origin/main` `fb7255b20` as `6d994b78c`; the merge was documentation-only, its
  incremental review is clean, and the spent review artifact is removed in this checkpoint.
- Observed platform-sync PR #420 merge green as `372be1041b20`, completing the live prerequisite to
  final current-main reconciliation and PR preflight.
- Merged post-sync `origin/main` `372be1041b20` as `bf20d17ce`, then passed Customer integration
  29/29, Shared.Api 60/60, and the Release solution build with 0 errors on that exact base.
- Passed PR preflight, completed the two-leg push through `e60219f7d`, and opened non-draft PR #425
  against `main` with no E2E skip label.
- Registered the published Shared.Api terminal cutover as a blocking predecessor to PR #425; its owner
  is `Refactor/typed-result_http-terminals` at package-source commit `1d261e3ce`.
- Re-routed that terminal dependency through the merged Reunion integration owner so Shared.Api
  publishes once and PR #425 consumes one generated platform baseline.
- Merged current `origin/main` as `c7ff8c474` and implemented the direct published Reunion package,
  namespace, error-definition, and HTTP-terminal conversion for the five owned Customer modules.
- Committed the conversion as `d4a5bb502`, fixed the fresh full review's only finding (`NAT1`) in
  `7a7e07e86`, merged platform-sync main as `7a854cd4c`, and completed both incremental reviews clean.
- Audited validator contracts and added Phase 6 for Review's injected validator; Ticket and B2B
  validator migrations remain with their existing owners.
- Production `Reunion.Validation` `0.1.0-alpha.1` completed its publication gate and returned this
  ledger from the Reunion integration owner.

## Verification

- Current Reunion candidate: Customer Release solution 0 errors and 3 existing warnings; Review
  30/30, Preference 19/19, User 14/14, Venue 2/2, Artist 2/2; Shared.Api 60/60.
- Current-main final rerun: complete `api/Concertable.slnx` Release build 0 errors and 5 existing
  warnings; Review 30/30, Preference 19/19, User 14/14, Venue 2/2, Artist 2/2; Shared.Api 60/60.
- Current CI-equivalent Customer carve copied the working candidate's tracked files to a validated
  temporary directory, built all 36 non-test/non-AppHost projects from package references with
  `-p:MinVerSkip=true`, and completed with 0 errors before the directory was removed.
- Direct package ownership passed for all 20 scoped compiling projects. No legacy
  `Concertable.Kernel.Functional`, `Concertable.Kernel.Errors`, or
  `Concertable.Shared.Api.Results` import remains in the five modules; no Ticket, Concert, Payment,
  shared Kernel, event, model, or migration path is changed; `git diff --check` passes.
- Integration pre-flight: the escalated `docker ps` call timed out after 34 seconds with no daemon
  response. No current-candidate Customer integration project or SQL container started.
- Resumed integration gate through `scripts/integration.ps1` with short artifacts roots: Customer
  Review 12/12, Preference 7/7, User 6/6, Venue 2/2, Artist 2/2; matching B2B User 3/3, Venue 25/25,
  Artist 17/17. All 74 tests passed across eight projects with no failed or skipped test, and every
  Testcontainers SQL container was removed.
- Current `.892` platform-pin gate: Release solution 0 errors and 9 existing warnings; scoped unit
  67/67; Shared.Api 60/60; integration 74/74 across the same eight projects; isolated 36-project
  Customer carve 0 errors and 183 existing warnings; `git diff --check` clean.
- DI-validator/package audit: `IReviewValidator` is the only custom injected validator in the five
  owned modules and still exposes one Reunion Result plus three booleans; `Reunion.Validation` source
  is merged at `a837ecb`, its validation paths are unchanged through `1500270`, and an exact
  prerelease NuGet.org search returns no package.
- Production validation package: SHA-256
  `0947D93220F585F8AD6E8617F5268807B4C05B120DF01970D49E036302010790`; valid NuGet.org repository
  signature; nine non-signature entries byte-match the candidate; a new NuGet.org-only net10 cache
  restored exact `Reunion.Validation`, `Reunion`, and `Reunion.Errors` `0.1.0-alpha.1`, built with
  zero errors, and ran the package consumer successfully.

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
- Current convention/Phase 4 unit gate passed: Review 30/30, Preference 19/19, Venue 2/2, and Artist
  2/2 in Release with separate short artifacts roots.
- Phase 4 integration pre-flight: `docker ps` returned HTTP 500 from the Docker Desktop Linux engine;
  no Venue or Artist integration test process or SQL container started.
- Current-main reconciliation: preserved the complete dirty Phase 4 tree across 18 product/workflow
  commits through merge `63924a67a`, then across two agent-hook-only commits through merge
  `4730b4d8e`; the branch is 0 behind `origin/main` `b46d10ec8`. None of the 20 base commits changed
  `api/agents/CODE_CONVENTIONS.md` or the typed-error rules.
- Live ownership/package gates: PR #282 remains open at `26ed63b896` and owns only Ticket/Concert/
  Customer Payment paths; no platform-sync PR is open. The final branch diff contains no excluded
  Ticket, Concert, Payment, checkout/purchase, shared Kernel source, model, or migration path.
- Phase 4 integration pre-flight recovered: `docker ps` succeeded. `scripts/integration.ps1 venue
  --artifacts-path C:\tmp\CustomerVenueIntegrationArtifacts` passed B2B Venue 25/25 and Customer
  Venue 2/2; the equivalent Artist run passed B2B Artist 17/17 and Customer Artist 2/2.
- Current-pin unit gate passed Review 30/30, Preference 19/19, Venue 2/2, and Artist 2/2 in Release.
  Shared.Api passed 50/50 in its repository-local output layout, including exact error-definition and
  Dunet architecture checks. Its repository-root discovery intentionally cannot run from an external
  `--artifacts-path`, so only integration projects use the short-root workaround.
- `dotnet build api/Concertable.slnx --configuration Release` succeeded with 0 errors and 9 existing
  warnings. The CI-equivalent isolated Customer carve discovered and built all 36 non-test/non-AppHost
  projects from package references with `-p:MinVerSkip=true`, with 0 errors; its verified temporary
  directory was removed.
- Final inventories place Results/Options only at owned application/service terminals, keep
  single-item nullability in repository contracts, use successful empty `IReadOnlyList` query returns,
  construct every Dunet case directly, and find no typed-slice HTTP exception. The Customer package
  diff adds Dunet only and leaves the existing FluentResults pin unchanged; `git diff --check` passed.
- `CV2` Shared.Api Release verification passed 56/56, including focused accepted-shape,
  discard/default-arm, and unrelated-switch coverage.
- `dotnet build api/Concertable.slnx --configuration Release` succeeded for the `CV2` candidate with
  0 errors and 7 existing warnings.
- `CV3` Shared.Api Release verification passed 60/60, including bounded `var _` and named `var`
  rejection plus unrelated-switch acceptance.
- `dotnet build api/Concertable.slnx --configuration Release` succeeded for the `CV3` candidate with
  0 errors and 7 existing warnings.

## Reviews

Full code review `06071872b..de2b8c163` produced
`reviews/Feature-typed-result_customer-outcomes.md`. `CV1` is fixed in `3ab6604c4`; `CV2` is fixed in
`312400220`; `CV3` is fixed in `b8e13b3ff`. Incremental reviews
`312400220..b8e13b3ff` and `b8e13b3ff..6d994b78c` are clean. With every finding fixed, the Release
build green, and current main reviewed, the spent artifact is removed in this checkpoint.

The Reunion conversion cannot use `incremental-review` directly: the prior artifact and its mandatory
marker were intentionally removed after that clean review, and the skill forbids reconstructing a
watermark from ledger prose. A fresh full `code-review` of the committed branch is the resolved gate.

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
- This non-Payment scope has no implementation blocker: published Reunion opens direct conversion.
  Customer has no dependency on B2B/Auth implementation; delivery follows its actual package topology.
- In this long worktree, default Debug output puts the x64 SqlClient SNI DLL at a 259-character path,
  which Windows rejects with `ERROR_FILENAME_EXCED_RANGE`. Use a short supported `--artifacts-path`
  for integration runs from this worktree; the binaries and application behavior are otherwise green.
- The 2026-08-07 typed-error convention replaces payload-free static error catalogs with named Dunet
  cases and replaces generated `Match`/per-case definitions with one exhaustive root switch. Existing
  wire contracts remain stable through per-case `[ErrorCode]` attributes and exact definition tests.

## Event log

### 2026-08-10 — Customer Ticket boundary advanced to terminal replacement

- Action: Accepted the terminal Customer Ticket owner handoff without changing this plan's code
  scope.
- Evidence: replacement PR #475 merged as `2b05ed110`; historical PR #282 closed as superseded;
  publication delivered platform `.910`; generated sync PR #479 merged as `b17fb07fe`.
- Outcome: Ticket remains excluded, but the exclusion now names the shipped replacement baseline
  instead of the obsolete open PR.
- Follow-up: continue this ledger's existing `## Next Steps` without editing Ticket-owned paths.

### 2026-08-10 - Phase 6 current-main reconciliation

- Action: Fetched origin and merged current `origin/main` into the clean Customer outcomes branch
  while preserving PR #425's remote head.
- Evidence: incoming main `d916e95cf`; PR #425 remains open at `e60219f7d`; the sole conflict was
  `api/Concertable.Customer/Directory.Packages.props`, resolved with platform pin `.903`, one entry
  per existing Reunion package, and the branch's Shouldly ownership.
- Outcome: The branch is current with main and Phase 6 implementation is now the resolved action.
- Follow-up: Implement and verify the Phase 6 Review validation and domain-outcome migration.

### 2026-08-10 — PR #470 domain-outcome reconciliation

- Action: Audited the net branch scope, all production domain guards and state outcomes in Review,
  Preference, User, Venue, and Artist, duplicated service/request validation, HTTP behavior, and test
  and architecture coverage.
- Evidence: `ReviewEntity.Create` is the sole scoped production `DomainException` and its star range is
  caller-actionable; the HTTP request validator duplicates it while the service relies on the throw.
  The other scoped methods expose capabilities, absence, lists, or unconditional mutations without an
  expected rejecting guard.
- Outcome: Customer non-Payment remains active/actionable; the Review correction is inside Phase 6,
  and repository-wide Shared/invariant cleanup is explicitly deferred.
- Follow-up: execute the amended `## Next Steps` without changing Ticket/Concert/Payment-owned paths.

### 2026-08-10 — Phase 6 package gate opened

- Action: Received the terminal validation-package handoff from the Reunion integration owner and
  reconciled it with the clean Customer branch, PR #425, current main, and terminal Payment sync.
- Evidence: production `Reunion.Validation` `0.1.0-alpha.1`; merged source `1500270`; valid repository
  signature and exact payload provenance; clean NuGet.org-only net10 consumer; PR #463 merged as
  `483350124`; PR #425 remains open at `e60219f7d`; local branch is 59 commits behind current main.
- Outcome: The hard blocker is removed. Phase 6 is actionable after current-main reconciliation.
- Follow-up: Execute `## Next Steps` and preserve the existing PR head until the verified phase is
  ready for its single update.

### 2026-08-10 — DI validation follow-up planned

- Action: Audited every validator in the five owned modules, the repository's other custom DI
  validators, Reunion's validation API/source lineage, and production package availability; then
  added Phase 6 to the plan.
- Evidence: only `IReviewValidator` is custom and DI-resolved in this scope; its current methods return
  `Result<TicketSummary, CreateReviewError>`/`bool`; upstream `a837ecb` supplies
  `ValidationResult = Valid | Invalid(ValidationErrors)` in a separate package; NuGet.org has no
  `Reunion.Validation` version.
- Outcome: The previous push action is superseded. Phase 6 preserves the one Ticket lookup, existing
  typed create errors and exact 201/404/409 contracts, and public capability booleans while changing
  the internal validator contract to `ValidationResult`. Ticket/B2B validators stay with their owners.
- Follow-up: The Reunion package owner publishes and verifies `Reunion.Validation` `.1`, then returns
  this ledger for Phase 6 implementation and final delivery.

### 2026-08-10 — PR update rejected before execution

- Action: Attempted the authorized-by-plan actual-work push after recording branch, upstream, work
  head, starting remote head, commit range, PR identity, clean tree, and current-main state.
- Evidence: local work head `b021ebbbeed65fbf9c7b0f8467cfd60871e83dcb`; starting and refreshed
  remote/PR head `e60219f7dfe13f0c49c818e2ed7ab7a557f84569`; 211 commits to send; branch 0
  behind `origin/main`; the execution gate rejected the push before it ran.
- Outcome: No remote mutation occurred. The verified work remains in the clean local branch and the
  two-leg push is blocked only on direct user authorization.
- Follow-up: Resume only after Tommy explicitly authorizes pushing PR #425.

### 2026-08-10 — review clean and current platform pin verified

- Action: Completed the fresh full review, fixed `NAT1` in `7a7e07e86`, incrementally reviewed the
  fix, merged platform-sync main `6f4a5cc3e` as `7a854cd4c`, incrementally reviewed that merge, and
  reran the affected local gate on platform pin `.892`.
- Evidence: review range `d66b780cd..d4a5bb502` with `NAT1` fixed; clean incremental ranges
  `d4a5bb502..7a7e07e86` and `7a7e07e86..7a854cd4c`; Release solution 0 errors; focused tests
  127/127; integration 74/74 across eight projects; 36-project Customer carve 0 errors.
- Outcome: The committed candidate is current-main, review-clean, and fully locally verified. The
  invalid `--no-restore` artifacts-root attempt ran no test; the later ten-minute shell cap preserved
  six green projects but interrupted B2B Artist during fixture startup, and the focused Artist rerun
  completed both missing projects green. The spent review artifact is removed in this checkpoint.
- Follow-up: Perform the authorized two-leg PR #425 update, verify both remote transitions, then wait
  for the updated PR gates without enqueueing it.

### 2026-08-09 — current-main local gate green

- Action: Reran the complete API Release build, five scoped unit suites, Shared.Api architecture
  suite, isolated Customer carve, and all package/carrier/scope inventories after current-main merge.
- Evidence: Release build 0 errors and 5 existing warnings; scoped unit 67/67; Shared.Api 60/60;
  36-project carve 0 errors; direct ownership across 20 projects; carrier, nullable repository,
  materialized collection, published terminal, excluded-path, FluentResults, and diff checks clean.
- Outcome: All implementation and local verification gates are green. This commit makes the Reunion
  conversion and ledger checkpoint durable before the required fresh full review.
- Follow-up: Run the fresh full code review, fix and re-review any finding, then update PR #425 once.

### 2026-08-09 — current-main reconciliation restored the candidate

- Action: Stashed the complete 45-path candidate, merged the two documentation-only main commits,
  and restored the stash.
- Evidence: merge commit `2180c19c0`; branch 0 behind `origin/main` `d66b780cd`; restore completed
  without conflict; the same 45 dirty paths are present and the merge changed only three unrelated
  `TECH_DEBT.md` files.
- Outcome: The Reunion candidate is preserved on current main. Its Docker-independent verification
  rerun now owns progress; the green integration evidence remains valid because the base delta did
  not change any runtime, package, build, test, or fixture input.
- Follow-up: Execute the current `## Next Steps` verification set.

### 2026-08-09 — Reunion candidate integration gate green

- Action: Ran the five scoped module wrappers through `scripts/integration.ps1` with short artifacts
  roots after Docker recovered; monitored each Testcontainers startup and cleanup.
- Evidence: Customer Review 12/12, Preference 7/7, User 6/6, Venue 2/2, Artist 2/2; B2B User 3/3,
  Venue 25/25, Artist 17/17; 74/74 total across eight projects with no failed or skipped test.
- Outcome: The required five Customer integration suites are green on the preserved Reunion
  candidate. Current-main reconciliation and its Docker-independent rerun gates now own progress.
- Follow-up: Stash the exact dirty tree, merge documentation-only `origin/main` `d66b780cd`, restore
  it, and execute `## Next Steps`.

### 2026-08-09 — Docker gate reopened

- Action: Re-ran the mandatory integration pre-flight and refreshed origin/PR state without changing
  the preserved candidate.
- Evidence: `docker ps` succeeded with no running containers; PR #425 remains open and non-draft at
  `e60219f7d`; `origin/main` advanced by two commits to `d66b780cd`, changing only three unrelated
  `TECH_DEBT.md` files.
- Outcome: The external blocker is removed and the five-module integration gate can run against the
  preserved candidate before its documentation-only current-main reconciliation.
- Follow-up: Execute the five module wrappers with short artifacts roots, then continue `## Next
  Steps` if all five Customer projects pass.

### 2026-08-09 — Reunion conversion implemented; Docker blocks integration gate

- Action: Migrated the five owned Customer modules to direct published Reunion `.1` packages and
  terminals, then ran every Docker-independent verification and ownership gate.
- Evidence: local merge head `c7ff8c474`; branch 0 behind `origin/main` `1043a9178`; Customer Release
  build 0 errors; scoped unit suites 67/67; Shared.Api 60/60; isolated 36-project carve 0 errors;
  direct package ownership and excluded-path inventories clean. `docker ps` timed out after 34
  seconds without a daemon response.
- Outcome: The uncommitted candidate is build-, unit-, architecture-, carve-, and scope-green, but
  the required five integration suites, incremental review, commit, and one-time PR #425 update are
  blocked on Docker Desktop health.
- Follow-up: Start or restart Docker Desktop; resume only after `docker ps` succeeds.

### 2026-08-09 — current-main merge completed and Customer migration opened

- Action: Fetched origin, reconciled the obsolete HTTP-terminal wait against the central Reunion
  ledger, and merged `origin/main` `1043a9178` into this clean worktree.
- Evidence: pre-merge head `e7c44f5b3`; PR #425 remains open at `e60219f7d`; the only merge conflicts
  were this plan and ledger; the runtime merge was automatic.
- Outcome: The branch is on the current package baseline and the independent Customer Reunion
  conversion is actionable.
- Follow-up: Execute `## Next Steps`, verify, incrementally review, and update PR #425 once.

### 2026-08-09 — PR #425 Reunion preparation dispatched

- Action: Separated this non-Payment scope from the Payment publication delivery chain.
- Evidence: the branch excludes Ticket/Concert/Payment clients; Reunion `.1` is published.
- Outcome: local conversion, verification, and review can proceed now while preserving the PR head.
- Follow-up: execute `## Next Steps` and update PR #425 once after the local gate is green.

### 2026-08-09 — Reunion integration dependency registered

- Action: Reconciled PR #425 and its clean local ledger tail with merged Reunion planning PRs
  #443/#444, preserved the HTTP-terminal work as centralized producer input, and registered this
  ledger in the Reunion owner's downstream handoffs.
- Evidence: local head `e7c44f5b3`; PR #425 head `e60219f7d`; fresh `origin/main` `c72b058af`;
  117 behind / 31 ahead; two local-only commits change this ledger; no open platform-sync PR.
- Outcome: the 29 unique Customer commits remain authoritative and unchanged. PR #425 waits for the
  Reunion Phase 4 generated platform-sync baseline and will reconcile exactly once afterward.
- Follow-up: the Reunion owner updates this ledger and surfaces its resume prompt after Phase 4 merges.
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

### 2026-08-07 — current conventions and Phase 4 gate reconciled

- Action: Fetched and merged the two docs-only `origin/main` commits, verified the worktree identity,
  ownership diff, PR #282 boundary, and absence of an open platform-sync PR, then audited every
  branch-owned Result/Option/error implementation against the updated typed-error conventions.
- Evidence: branch 0 behind `origin/main` `529dba9dd`; clean pre-edit tree; PR #282 remains open at
  `26ed63b896`; the three Review/Preference errors use static sealed records while the convention now
  requires Dunet cases and an exhaustive definition switch; the Shared.Api guard still asserts the
  superseded construction shape.
- Outcome: The exact convention reconciliation is resolved and precedes Phase 4 implementation; no
  shared runtime API or excluded Ticket/Concert/Payment path is in scope.
- Follow-up: Execute the updated `## Next Steps`, verify the combined checkpoint, and commit locally
  without pushing.

### 2026-08-07 — convention migration and Phase 4 implemented; Docker gate blocked

- Action: Migrated all branch-owned Review/Preference errors to current named Dunet unions, updated
  the Shared.Api architecture guard, implemented Venue/Artist service Options and HTTP terminals,
  added both modules' owned unit/integration projects, and wired solution/runner discovery.
- Evidence: exact error contract tests remain pinned; repository lookups alone remain nullable; Review
  unit 30/30, Preference unit 19/19, Venue unit 2/2, and Artist unit 2/2 passed in Release;
  `docker ps` failed with an HTTP 500 response from the Docker Desktop Linux engine before tests.
- Outcome: Implementation and unit verification are complete but uncommitted. Phase 4 remains open;
  integration, Shared.Api, solution, carve, inventory, and final diff gates have not run.
- Follow-up: Restore Docker Desktop health and execute the current `## Next Steps` without changing
  or discarding the working tree.

### 2026-08-07 — Phase 4 completed on current main

- Action: Reconciled the implementation with current main and typed-error conventions, restored
  Docker health, ran the Venue/Artist integration workflow, and completed the current-pin unit,
  Shared.Api, solution, isolated Customer carve, carrier/ownership, and diff gates.
- Evidence: Review 30/30, Preference 19/19, Venue 2/2, Artist 2/2, Shared.Api 50/50; Venue integration
  27/27 and Artist integration 19/19 across B2B plus Customer; solution and 36-project carve both 0
  errors; branch 0 behind `b46d10ec8`; PR #282 boundary and no-open-platform-sync gate rechecked.
- Outcome: Phase 4 is complete in this commit. The current error records follow the latest named-case,
  exhaustive-switch, generic-factory, direct-construction, and stable-code conventions; the public
  Venue/Artist 200/404 contracts remain unchanged.
- Follow-up: Resume Phase 5's final scope audit and code-review gate; do not push without a delivery
  instruction and do not run local E2E ahead of the merge queue.

### 2026-08-07 — Phase 5 execution gate and full code review completed

- Action: Merged current main through `de2b8c163`, ran the combined five-module unit/integration,
  Shared.Api, Release solution, carve-validity, scope/inventory, and diff gates, then completed the
  architecture-aware full code review.
- Evidence: Review watermark `de2b8c16395a38f1264b2a7c3a51e6103750bec6`; range
  `06071872b..de2b8c163`; artifact `reviews/Feature-typed-result_customer-outcomes.md`; `CV1` open
  (integration assertions must use Shouldly), `CV2` open (definition-switch guard must reject
  discard/default arms). Targeted unit/architecture 117/117; Customer integration Review 12/12,
  Preference 7/7, User 6/6, Venue 2/2, Artist 2/2; matching B2B User 3/3, Venue 25/25, Artist 17/17;
  Release solution 0 errors; prior 36-project Customer carve remains valid because the merge changed
  no `api/**` path; scoped inventories and `git diff --check` clean.
- Outcome: The implementation gate is green and the review has two concrete fixable findings. While
  review ran, Payment PR #392 advanced `origin/main` to `b66325acd`; only the Shared.Api guard overlaps.
- Follow-up: Commit this checkpoint, reconcile current main, fix `CV1`/`CV2`, verify, run incremental
  review, then preflight/push/open the authorized PR.

### 2026-08-07 - review checkpoint and current-main reconciliation confirmed

- Action: Refetched origin and reconciled the ledger with the clean branch, current main, review
  artifact, PR state, and live platform-sync gate before applying the review findings.
- Evidence: review checkpoint `23461784c`; merge commit `fc9f69407`; branch head is 0 behind
  `origin/main` `b66325acd`; no source PR exists; `CV1` and `CV2` remain open; platform-sync PR
  #420 has failed `build` and `ci-complete` checks.
- Outcome: The ledger's first two actions were already complete and are now recorded. Finding fixes
  can proceed from the clean current-main tree; PR preflight remains gated on platform-sync #420.
- Follow-up: Address `CV1` and `CV2` serially, verify, incrementally review, then preflight when #420
  is green.

### 2026-08-07 - review finding CV1 fixed

- Action: Added Shouldly to the five affected Customer integration projects and converted all 33
  integration assertions added or changed by the reviewed branch, leaving pre-existing xUnit
  assertions outside the review scope unchanged.
- Evidence: `CV1` is fixed in `reviews/Feature-typed-result_customer-outcomes.md` by this commit;
  Release restore/build and integration execution passed Customer Review 12/12, Preference 7/7,
  User 6/6, Venue 2/2, and Artist 2/2; no reviewed addition still uses `Assert.*`; and
  `git diff --check` is clean.
- Outcome: `CV1` is fixed and verified in its own unpushed commit. `CV2` remains open, so the review
  artifact and plan lifecycle stay live.
- Follow-up: Fix and verify `CV2` in its own commit, then run the incremental review gate.

### 2026-08-07 - review finding CV2 fixed

- Action: Extended the Shared.Api Dunet definition guard to reject discard/default arms inside
  definition switches while retaining the merged Match, abstract, and exhaustive-switch shapes; added
  focused scanner coverage.
- Evidence: `CV2` is fixed in `reviews/Feature-typed-result_customer-outcomes.md` by this commit;
  Shared.Api Release tests passed 56/56, the Release solution built with 0 errors and 7 existing
  warnings, and `git diff --check` is clean.
- Outcome: `CV2` is fixed and verified in its own unpushed commit. Both original findings are fixed;
  the review artifact remains for the required incremental review.
- Follow-up: Run the incremental review gate over the committed `CV1` and `CV2` fixes.

### 2026-08-07 - incremental review found CV3

- Action: Reviewed `de2b8c163..312400220` through correctness, microservice isolation, module
  boundaries, seeding, C# conventions, and changed-path coverage.
- Evidence: 91 commits reviewed; the Payment/current-main paths equal `origin/main`; the Shared.Api
  merge resolution preserves both definition-shape families; the committed `CV1` conversions are
  green; and direct scanner probes show `DefinitionSwitchCatchAllArmPattern` rejects `_` but accepts
  exhaustive `var _` and `var ignored` arms.
- Outcome: `CV3` is open in `reviews/Feature-typed-result_customer-outcomes.md`; the review watermark
  is advanced to `312400220`. No other high-confidence finding survived the review.
- Follow-up: Fix `CV3` in its own commit, verify Shared.Api and the Release solution, then
  incrementally review from `312400220`.

### 2026-08-07 - review finding CV3 fixed

- Action: Extended the bounded Shared.Api definition-switch scanner to reject discard and named
  `var` catch-all arms and added focused rejection plus unrelated-switch acceptance coverage.
- Evidence: `CV3` is fixed in `reviews/Feature-typed-result_customer-outcomes.md` by this commit;
  Shared.Api Release tests passed 60/60, the Release solution built with 0 errors and 7 existing
  warnings, and `git diff --check` is clean.
- Outcome: `CV3` is fixed and verified in its own unpushed commit. The review artifact remains for
  the required clean incremental review.
- Follow-up: Run the incremental review gate from watermark `312400220`.

### 2026-08-07 - CV3 incremental review clean; current-main gate reopened

- Action: Incrementally reviewed `312400220..b8e13b3ff`, then refreshed origin and platform-sync #420.
- Evidence: two commits reviewed with no new finding; Shared.Api 60/60 and Release solution 0 errors
  remain the verified candidate gate; `origin/main` advanced to `fb7255b20`, 22 commits ahead of the
  branch; platform-sync #420 has green build/carves with remaining unit/integration checks pending.
- Outcome: All review findings are fixed and the CV3 follow-up review is clean, but the review artifact
  stays live until current main is merged and incrementally reviewed.
- Follow-up: Merge `origin/main`, rerun the affected local gate, and incrementally review from
  `b8e13b3ff`.

### 2026-08-07 - current-main review clean and review artifact spent

- Action: Merged current `origin/main` `fb7255b20` and incrementally reviewed
  `b8e13b3ff..6d994b78c`.
- Evidence: the merge changed only `plans/platform/POLYREPO_FULLSTACK_PROGRESS.md`; no API, test,
  project, package, or build input changed; the branch is 0 behind main; eight documentation commits
  were reviewed with no finding; all `CV1`-`CV3` findings are fixed and their last code gates remain
  Customer integration 29/29, Shared.Api 60/60, and Release solution 0 errors.
- Outcome: The current-main gate and all review gates are green. The spent review artifact is removed
  in this checkpoint. Platform-sync #420 remains the only prerequisite before PR preflight.
- Follow-up: Wait for #420 to finish green, refresh main, then preflight, push, and open the PR.

### 2026-08-08 - platform-sync #420 merged green

- Action: Followed platform-sync PR #420 through its green source checks and merge queue to terminal
  merged state.
- Evidence: every source check passed; the fresh #420-only merge-group API/UI E2E run completed;
  PR #420 merged at 2026-08-07T23:02:33Z as `372be1041b2050d8f62eea2435005a981ec222b4`.
  The earlier combined batch failure belonged to ejected PR #418 and was not retried by this work.
- Outcome: The platform prerequisite is terminal green. Final current-main reconciliation now owns
  progress.
- Follow-up: Fetch and merge current main, inspect the base delta, run any affected local gate, then
  preflight, push, and open the PR.

### 2026-08-10 - Phase 6 Review validation outcomes implemented and verified

- Action: Added direct `Reunion.Validation` ownership; converted every custom Review DI validator to
  structured validation; moved Ticket lookup composition into `ConcertReviewService`; made
  `ReviewEntity.Create` return star-range validation; mapped it to `CreateReviewError.Invalid`; and
  added domain, definition, short-circuit, query-count, capability, fault, and cancellation coverage.
- Evidence: This commit passes Review unit 40/40, the five scoped unit suites 77/77, Shared.Api 60/60,
  the five integration wrappers 74/74 across eight projects, the Release solution build with 0
  errors and 7 existing warnings, the 36-project isolated Customer carve with 0 errors, all scoped
  architecture/ownership inventories, `git diff --check`, and an empty post-test Docker inventory.
  The initial Review integration failure was a deterministic `.1`/`.2` `Reunion.Errors` runtime
  mismatch; direct `.2` ownership and migration of the three branch-owned union definitions fixed it.
- Outcome: Phase 6 implementation and its complete local verification contract are green in this
  unpushed checkpoint. PR #425 remains at `e60219f7d`; `origin/main` is nine commits ahead and awaits
  the required post-review reconciliation.
- Follow-up: Run fresh full `/review`, fix and incrementally review any findings, then reconcile
  current main, rerun affected gates, incrementally review the merge, and perform the verified two-leg
  PR update.

### 2026-08-10 - fresh full Phase 6 delivery review found three issues

- Action: Ran the mandatory native, security, correctness, microservice-isolation, module-boundary,
  seeding, C# convention, and changed-path coverage passes over the complete net branch diff.
- Evidence: Review range `d916e95cf..5cfdb9427` spans 43 commits; both review watermarks are
  `5cfdb9427f1896351c72b5e829d105b637fcd390`; the work order is
  `reviews/Feature-typed-result_customer-outcomes.md`. `NAT1`, `NAT2`, and `NAT3` are open medium
  findings; no architecture, seeding, test-coverage, or security finding survived the confidence bar.
- Outcome: The implementation checkpoint is review-blocked on one direct-package correction and two
  duplicate-key race translations. PR #425 remains unchanged.
- Follow-up: Fix and verify `NAT1`, `NAT2`, and `NAT3` in separate commits, then run incremental
  review from `5cfdb9427`.

### 2026-08-10 - review finding NAT1 fixed

- Action: Added direct `Reunion.Errors` package ownership to Review.Infrastructure, whose validator
  directly constructs `ValidationErrors`.
- Evidence: `NAT1` is fixed in the review work order by this commit; the focused Review.Infrastructure
  Release build succeeds with 0 errors and one existing warning; direct-reference inventory and
  `git diff --check` pass.
- Outcome: `NAT1` is fixed and verified in its own unpushed commit. `NAT2` and `NAT3` remain open.
- Follow-up: Fix Preference's duplicate-create race as `NAT2`, then Review's equivalent race as
  `NAT3`, each in its own verified commit.

### 2026-08-10 - review finding NAT2 fixed; integration environment unavailable

- Action: Replaced Preference's preflight read plus insert with repository `TryAddAsync`, using the
  existing unique `UserId` index as the atomic authority and translating duplicate-key rejection to
  `PreferenceAlreadyExists` after discarding the failed tracked insert.
- Evidence: `NAT2` is fixed in the review work order by this commit; the updated service tests cover
  atomic success, duplicate conflict, collaborator fault, and cancellation, and the Preference unit
  suite passes 19/19 with `git diff --check` clean. The mandatory Docker data-path preflight timed out,
  followed by a timed-out `docker ps`; no integration project was started.
- Outcome: `NAT2` is fixed in its own unpushed commit, with its deterministic gate green and its
  existing integration conflict scenario pending a healthy Docker engine. `NAT3` remains open.
- Follow-up: Fix `NAT3`, then require one healthy Docker preflight before running the focused
  Preference and Review integration wrappers and the incremental review.

### 2026-08-10 - review finding NAT3 fixed

- Action: Replaced Review's check-then-insert persistence with repository `TryAddAsync`, using the
  existing unique `TicketId` index as the atomic authority and mapping duplicate rejection to
  `ReviewAlreadyExists` after discarding the failed tracked review.
- Evidence: `NAT3` is fixed in the review work order by this commit; Review unit tests pass 43/43,
  including atomic success, duplicate conflict, persistence fault, and persistence cancellation;
  `git diff --check` is clean.
- Outcome: All three fresh-review findings are fixed in separate unpushed commits. The focused
  Preference and Review persistence integration gates still require a healthy Docker engine before
  incremental review.
- Follow-up: Require one healthy Docker data-path preflight, run the Preference and Review integration
  wrappers, then incrementally review from `5cfdb9427`.

### 2026-08-10 - focused persistence integration gates green

- Action: Retried Docker once after both atomic create fixes, then ran the Preference and Review
  integration wrappers sequentially under the integration-debug workflow.
- Evidence: The fresh nginx probe completed a stable host-to-container data round trip; Preference
  passes 7/7 and Review passes 12/12 in Release, each with a fresh Testcontainers SQL instance; the
  existing 409 duplicate-create scenarios pass through the new atomic repository paths; and the
  post-run Docker inventory is empty.
- Outcome: `NAT2` and `NAT3` now have green deterministic and real-database gates. All three review
  findings are fixed; incremental review is actionable.
- Follow-up: Incrementally review `5cfdb9427..HEAD`, address any new finding, then reconcile current
  main and complete the final delivery gate.

### 2026-08-10 - finding-fix incremental review clean

- Action: Incrementally reviewed the three finding fixes and their plan-managed verification
  checkpoints from the full-review watermark.
- Evidence: Range `5cfdb9427..958c05c5a` spans six commits; the native, correctness,
  microservice-isolation, module-boundary, seeding, C# convention, and changed-path coverage lenses
  found no new issue. No security-sensitive production path changed in the incremental range.
- Outcome: The review work order has no open finding and is stamped at the exact verified work head.
- Follow-up: Commit this review checkpoint, then fetch and reconcile current `origin/main` before the
  final affected-gate run.

### 2026-08-10 - current-main gate review-clean but Docker-blocked

- Action: Fetched and merged current `origin/main`, resolved Customer central package management,
  ran every Docker-independent affected gate, attempted the mandatory Docker preflight, and
  incrementally reviewed the full reconciliation range including security-sensitive incoming paths.
- Evidence: main `aab321bd2` merged as `c021d26c9`; branch 0 behind; Release solution 0 errors and 8
  existing warnings; scoped units 80/80; Shared.Api 60/60; isolated 36-project carve 0 errors;
  inventories and whitespace clean. Review range `958c05c5a..c021d26c9` spans 39 commits and adds no
  finding; both review markers are `c021d26c9de0f65f291b319e38668c40844bc984`. The five-minute
  fresh-container health check and a 60-second `docker ps` both timed out; one Docker Desktop process
  reports non-responsive; no integration suite started.
- Outcome: The exact current-main head is build-, unit-, carve-, audit-, and review-clean, but final
  integration verification is environment-blocked. PR #425 remains open and unchanged at
  `e60219f7d` and no push was attempted.
- Follow-up: Restart Docker Desktop, require one green data-path preflight, complete the five Release
  integration wrappers, then recheck base currency and perform the verified two-leg PR update.
