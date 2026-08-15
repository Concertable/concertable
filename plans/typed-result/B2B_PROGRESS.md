# B2B typed-result migration progress

- Plan: `plans/typed-result/B2B_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Roadmap item: `typed-result/b2b`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2BTypedResultMigration`
- Branch: `Refactor/B2BTypedResultMigration`
- PR: #552 (ready; auto-merge disabled)
- Checkpoints 8-9 commit: `bfc8690b196821bdd735ea5d229182fd9a3baf36`
- Current code/package-main merge commit: `6c1e84101`, through platform-sync PR #575 merge
  `dee412ba8ff824a46ce16783d2f7d1fc161f2774`.
- Review/fix commit: `eb84634699fa643a072342cd196b9767a6694619`
- Review watermark: `a6762b0368bd930bfc564876cfc1f5cb8ce7e5e3`; incremental review and
  security review are clean.
- Checkpoint 10B consumer commits: `c55c99718` and `544144527`
- Implicit-conversion correction push: starting remote head `804d9b4e8`; pushed
  `219b34b1e..8b44d41ec`; local work head, remote branch, and PR #552 head matched
  `8b44d41ec8ecb9d8b7d4c648b76ad0967401024b`. Exact-head draft-PR CI is the next gate.
- Current-main reconciliation push: starting remote head `2b68197e0`; pushed
  `506addfee..21e58a2b8`; local work head, remote branch, and PR #552 head matched
  `21e58a2b865e79a1fdee8a2a9e7078dfb7474fbc`; checkpoint transport then matched
  `8db6d14f0fd1f644226bda27c2764f429a0e0c1b`. PR #552 was marked ready and exact-head CI passed.
- HTTP-terminal follow-up push: starting remote head `af8aa70b46883a5536ac8a32ae79a8c470210eef`;
  pushed `af8aa70b4..b9448ab0e`; local work head, remote branch, and PR #552 head matched
  `b9448ab0e7ec552347d347044f64c0b77a747520`; checkpoint transport then matched
  `bb29e929b87706a087f118025d09c7f3fbf3dc67`. Exact-head CI run `31803089514` failed only the
  B2B Concert integration project because Deal GET's generic OK terminal omitted the polymorphic
  `IDeal` discriminator. Current `origin/main` through `7b8764377` is merged as `2e9b16bd4`.
- Deal terminal correction: this commit restores the custom typed `ActionResult<IDeal>` success
  mapping required by the wire discriminator while retaining Reunion's typed error mapping. Focused
  Deal HTTP contracts pass 2/2, B2B architecture passes 8/8, Deal API formatting is clean, and
  `git diff --check` is clean.
- Polymorphic-terminal correction push: starting remote head
  `bb29e929b87706a087f118025d09c7f3fbf3dc67`; pushed `bb29e929b..2025e95bb`; local work head,
  remote branch, and PR #552 head matched `2025e95bbbd761e438558b99e9d0fcc62720013e`.
  Exact-head CI is the next gate.
- Messaging producer commit: `ade9728f9`
- Messaging delivery branch/PR: `Feature/MessagingOutboundCommands`, PR #536, remote head
  `7a0886e1245ef76267f0cf906518b2169ac3cfd6`
- Messaging merge commit: `5c4dc3ddf5e0a67c51d493b1c9f5a93da6dfb9b3`
- Dependency/package gate: terminal. Payment `0.1.0-alpha.0.973`, Messaging, and Reunion `0.1.0-alpha.3`
  are published and synced through platform-sync PR #547.
- Main reconciliation: `origin/main` through `429581025b471c5ed76d3b34518ff5623f364247`
  is merged in this commit. The four Artist/Venue dashboard conflicts preserve main's MTD Payment
  reporting behind the typed Option identity guards; platform `0.1.0-alpha.0.980` restores cleanly.
  Exact-head PR CI remains after the reconciliation review and push.
- Module-facade convention follow-up: implemented and locally verified. B2B Artist, Venue, and User
  plus Customer Concert, Ticket, and User module facades now delegate their existing operations to
  application services instead of owning repository/mapping logic. The convention is documented in
  renamed `api/agents/CONVENTIONS.md` and enforced by B2B architecture tests. Current main is
  reconciled through `66d26dbfa`; the sole roadmap conflict preserved both the completed B2B Payment
  producer and completed Customer Ticket delivery records.
- Post-merge review watermark: `9380696c208224e59ab77d09d8a72d00853e852f`; incremental native,
  security, isolation, module-boundary, seeding, convention, and coverage review is clean.
- Module-facade/current-main push: starting remote head `1b7c4ada6`; pushed
  `1b7c4ada6..6ec2eea87`; local work head, remote branch, and PR #552 head matched
  `6ec2eea87b9eca250163fcf42325b2aa30f9ff05`.
- Exact-head CI run `31821126177` passed at transport head `c52542f98`, including the solution build,
  B2B/Customer/Payment carves, unit and integration matrices, and `ci-complete`.
- Platform `0.1.0-alpha.0.988` reconciliation is clean and reviewed through
  `0f331b6a37cd7ffa4a746ce5e2dd96cf636109aa`.
- Platform `0.1.0-alpha.0.988` reconciliation push: starting remote head `c52542f98`; pushed
  `c52542f98..202bbce12`; local work head, remote branch, and PR #552 head matched
  `202bbce124d30350043bb6cf19002c140b3835fb`.
- Transport head is synchronized locally, remotely, and on PR #552 at
  `f1468d83626f2e32e73bb4b76e19629ea20fa13c`. Exact-head CI run `31823604514` is terminal green,
  including the solution build, service carves, unit and integration matrices, and `ci-complete`.
- Reunion `0.1.0-alpha.7` is published. Concertable PR #569 merged as `7fb3baeaf920baa11dfe540db8c408aa316825b0`,
  its package publication is green, and platform-sync PR #575 merged platform `0.1.0-alpha.0.995` as
  `dee412ba8ff824a46ce16783d2f7d1fc161f2774`. Tommy authorized landing PR #552 on 2026-08-15.

## Current state

Checkpoints 1-9 are committed. Checkpoints 8 and 9 were verified and committed together because this
session resumed an interrupted tree in which their changes were already interleaved. The current-main
merge preserves the typed contracts through main's application-executor façade and keyed Deal
strategy factories and is fully verified. Incremental review found one ownership defect, NAT6, and
fixed it in `eb8463469`; no other new finding survived the confidence filter.

Checkpoint 8 now uses the complete Reunion alpha.2 family. The two custom DI validators return
`Reunion.Validation.ValidationResult`; B2B no longer contains `FluentResults`,
`Concertable.Kernel.Functional`, `Concertable.Shared.Api.Results`, or the old Kernel
`ValidationErrors` carrier. The shared compatibility carrier and its tests remain intact for Auth and
Customer until the downstream Shared-contraction plan owns their deletion. Direct package ownership
is green in the B2B architecture suite.

Checkpoint 9 moves caller-actionable guards into domain-owned Results:

- Artist and Venue create/update return structured profile validation, with services rejecting invalid
  direct calls before image/geocoder work.
- Tenant address/tax/legal construction returns structured validation, including null DTO and null
  registered-address handling at the direct service boundary.
- Invitation accept/revoke and door-revenue declaration return typed domain alternatives that services
  map without duplicating their state checks.
- Image/geocoder/identity output guards, invitation expiry maintenance, provisioning consistency,
  `VatBreakdown` balance, and other impossible internal faults remain exceptional.

New, never-published cases use resolver-derived codes rather than `[ErrorCode]` compatibility
overrides. Existing published cases keep their pinned codes. The negative door-revenue application
case is named `Negative`, deriving `declare.door_revenue_negative`.

The Reunion integration close-out and stop-hook scope correction are merged independently on
`origin/main` by PR #512 and included in the current reconciliation. Do not recreate the deleted
`REUNION_INTEGRATION_PLAN.md` or `REUNION_INTEGRATION_PROGRESS.md` files here.

Tommy authorized the durable SEC1 B2B + Payment saga/package cut-over on 2026-08-12. The hard decision
blocker is removed. Checkpoint 10A is implemented and committed on the Payment producer branch;
Checkpoint 10B is implemented, verified, and committed in this consumer worktree. B2B persists stable
acceptance/cancellation operation IDs, atomically stages Payment commands through its outbox, consumes
typed terminal/deferred outcomes idempotently, and exposes the latest operation through a typed Option
HTTP terminal. Payment owns operation replay and pending-operation recovery; no B2B reconciliation
runner or Payment runtime
reference was added. The resolved SEC1 tech-debt entry has been deleted.

The module-facade follow-up preserves the existing typed contracts and runtime behavior. Application
services own the moved query implementations and forward `Option`/boolean results directly across the
facade. Artist and Venue keep their existing `Option<int>` dashboard identity flow and explicit
`TryGetValue` branching; only the operation name now states that the identity belongs to the current
tenant. B2B and Customer consume the published Reunion `0.1.0-alpha.7` family after restoring their
service-local package graphs; no Reunion extension was copied or recreated locally.

## Next Steps

Commit and push the clean incremental-review checkpoint, require local/remote/PR head equality and
exact-head build, carve, unit, integration, architecture, formatting, and HTTP-contract CI. Enqueue
PR #552 with the merge workflow's E2E tier and own publication/platform sync through terminal green.
At that gate, update the registered downstream ledgers and dispatch their open work.

## Completed work

- Checkpoints 1-5 migrated Deal, Tenant, Venue/Artist, User, and Payment-independent Concert outcomes.
- Checkpoints 6-7 migrated payment/cancel/finish workflows, retryable completion faults, direct Reunion
  carrier/terminal ownership, and complete B2B FluentResults removal; implementation commit
  `e229afb581c829279ca821b0a85729c4c4f0f441`.
- The staged review covered `1043a9178..e229afb58`; the prior incremental review covered later commits
  through `3d50d321c`. NAT1-NAT5, SEC2, and CV1 are fixed. SEC1 is authorized and owned by Checkpoint 10.
- Checkpoints 8-9 and their alpha.2 package reconciliation, direct unit coverage, and HTTP contract
  coverage are implemented, verified, and committed as `bfc8690b1`.
- Current main was reconciled and verified in merge commit `5613a817a`.
- Incremental review covered `3d50d321c..eb8463469` (332 commits). NAT6 restored the Shared-owned
  compatibility carrier and tests in `eb8463469`; the follow-up review is clean.
- Checkpoint 10A producer implementation is committed through `6458ec0d0`. Checkpoint 10B stages
  capture/deposit/refund commands transactionally, handles seven typed Payment outcomes, persists
  operation/failure state, maps pending cancellation safely, and provides the typed operation-status
  endpoint. The B2B migration was re-scaffolded; unrelated Payment migration churn was removed.
- B2B registers the three Payment commands as outbound-only through Messaging `Sends<T>`. The
  Messaging registry resolves their wire identities without making B2B receive or handle Payment
  commands; implementation commit `ade9728f9`, consumer follow-up `544144527`.
- Exact local verification used `Concertable.Payment.Contracts` / `.Client` `0.1.0-alpha.0.949` from
  producer commit `6458ec0d0` (SHA-256 `F0330F4687B8D4E073262D99C0AC16B7BAF50387C13A85B2C75D6A199818246C`
  and `7585A321BBB16C87323806F67885C011ED838DB42BD1AADD207F352681EE8C92`) plus Reunion
  `0.1.0-local.113be42`. Reunion core, AspNetCore, and Errors SHA-256 hashes are
  `36F5C1C66BD9D63DFD180AEF69D266FDF05FB5EEDBE7573DCEB326063129A9A2`,
  `5BCE01783D79B99F60FB1F848560B04563169C9346A84CF02815E483A5E8767C`, and
  `993E8F966BEDEF06C94D8D8FDC28C89A7856BCFCB6DD21980CE64F329FD82544`.
- Exact local Messaging verification used `Concertable.Messaging.Application` `0.1.0-alpha.0.943`
  from `ade9728f9` (SHA-256
  `040342DF1327CBBB7A1CFD64351332C626CC2E45B0DF46AE4FFEA88D1D4BD8B9`).
- The reproducible isolated closure is under
  `C:\Users\TommySeery\source\repos\Concertable\.artifacts\package-cutover\consumer-packages-ade9728f9`;
  the Messaging producer pack is under the sibling `messaging-ade9728f9-platform943` directory. All
  temporary feed and version inputs were restored before commit.

## Verification

- Full `api/Concertable.slnx` Release build against the exact local package closure: passed, 0 errors
  and two existing generated B2B UI nullable-context warnings.
- B2B solution build: passed, 0 errors and the same two existing generated warnings.
- B2B unit suites: 4/4 projects green; Concert 205/205. B2B architecture tests: 8/8.
- Standalone B2B package carve: passed in Release with 0 errors and one existing `UserEntity` warning;
  Payment runtime source was absent and deployable references remained package-only.
- Affected-project formatting: passed. The solution-wide formatter itself cannot apply generated
  document-property changes; narrowed project gates completed successfully. `git diff --check` is clean.
- Docker health passed after a clean Docker Desktop restart with a fresh-container host-to-container
  HTTP data round-trip. No local E2E was run.
- Focused changed saga and HTTP surface: 34/34 passed. Full B2B Concert integration: 155/155 passed.
- Messaging Application unit tests: 41/41 passed. Azure Service Bus unit tests: 8/8 passed.
- Current-main isolated Messaging producer branch: full API Release build passed with 0 errors;
  affected formatting and diff checks passed. Code review through `28e5797ff` is clean.
- Messaging PR #536's build, carve, unit, and integration checks are terminal and green against remote
  head `28e5797ff`; PR-level E2E jobs skipped as expected before queue admission.
- PR #536 is current with `origin/main` and labelled `full-e2e` because it changes a public published-package API.
- Tommy explicitly authorized merging PR #536 on 2026-08-13.
- The naming follow-up `2142f5d6a` uses `RegisterCommand` for wire identity,
  `RegisterCommandHandler` for receiver ownership, and `HandledCommandTypes` for receiver creation.
  Focused build passed with 0 errors; Messaging tests passed 41/41 and Azure Service Bus tests 8/8;
  formatting and diff gates passed. Incremental review is clean through `2142f5d6a`.
- PR #536's refreshed build, carve, unit, and integration matrix is terminal and green at exact remote
  head `2142f5d6a`; PR-level E2E jobs skipped as expected before queue admission.
- PR #536 was reconciled conflict-free with current `origin/main` in `7a0886e12`; the full API Release
  solution rebuild passed with 0 errors and four existing warnings. The remote head matches locally,
  is 0 commits behind main, and the reconciliation review is clean.
- PR #536's final current-main build, carve, unit, and integration matrix is terminal and green; exact
  local, remote, and PR heads match `7a0886e12`, merge state is clean, and `full-e2e` is applied.
- Auto-merge was enabled but PR #536 remained `OPEN/CLEAN` with no queue entry for six consecutive
  minutes. No merge-group run was dispatched or failed, proving the GitHub re-evaluation glitch; the
  sanctioned one-time disable/re-enable nudge is the next action.
- The one-time nudge admitted PR #536 to the merge queue at position 1. GitHub cleared the standalone
  auto-merge request as expected and retained the exact reviewed head.
- PR #536 passed its full-E2E merge group and merged as
  `5c4dc3ddf5e0a67c51d493b1c9f5a93da6dfb9b3`.
- Publish run 31693533673 succeeded, including fresh-feed restore verification, and published platform
  `0.1.0-alpha.0.966`. Platform-sync run 31693704239 succeeded and opened PR #538.
- PR #538 was superseded and closed after another API merge published `0.1.0-alpha.0.967`; cumulative
  sync PR #539 contains the Messaging publication. Its build, carve, unit, integration, and
  `ci-complete` checks are terminal and green.
- PR #539 was then superseded by cumulative sync PR #541 after API PR #529 merged and published
  `0.1.0-alpha.0.968`. PR #541 remains the active gate and includes the Messaging release.
- Cumulative platform-sync PR #541 passed its required matrix and merged green as
  `1c88858f93f648f1719fa9e4d273749b8932b364`. The Messaging prerequisite is terminal on normal feeds.
- Payment PR #544 passed its corrected full-E2E merge group and merged as `d6619a856`. Publish run
  `31722209038` released platform `0.1.0-alpha.0.973`; platform-sync PR #547 aligned B2B to Reunion
  `0.1.0-alpha.3`, passed its full build/carve/unit/integration matrix, and merged as `7bd956499`.
- Current-main/package reconciliation committed as `dc651f49f`. The deployable B2B Web runtime closure
  builds in Release against normal-feed Payment `0.1.0-alpha.0.973` and Reunion `0.1.0-alpha.3` with
  0 errors and 0 warnings. The full B2B solution attempt hit the contended host's 20-minute timeout
  without diagnostic output; exact-head draft-PR CI owns that complete matrix.
- Payment was reconciled with that current main in `15de28fb8` and now consumes the released Reunion
  alpha.3 family containing `113be42`. Payment reached the queue after its build, carve, unit,
  integration, ownership, plan-graph, and review gates passed. Its queue regression fix is locally
  green for the Payment AppHost build, 58 focused service/adapter tests, the command-topology test,
  formatting, and diff checks.
- Package-only B2B Web Release build against the isolated exact Messaging, Payment, and Reunion
  artifact closure: passed, 0 errors and one existing `UserEntity` warning.
- `dotnet build api/Concertable.B2B/Concertable.B2B.slnx --configuration Release --no-restore -m:1
  -nr:false`: passed, 0 errors and 4 existing warnings (two Concert integration nullable warnings and
  two generated Reqnroll nullable warnings).
- Affected unit tests: Artist 11/11, Venue 12/12, Tenant 124/124, Concert 151/151.
- B2B architecture tests: 8/8.
- B2B Artist integration: 17/17.
- B2B Venue integration: 25/25.
- B2B Tenant integration: 58/58.
- Earlier baseline B2B Concert integration: 153/153; Customer Concert integration: 11/11.
- Full `api/Concertable.slnx` Release build: passed, 0 errors and 2 existing generated E2E
  nullable-context warnings.
- Scoped changed-file formatting and immediate `--verify-no-changes`: passed.
- Post-merge full `api/Concertable.slnx` Release build: passed, 0 errors and 3 existing warnings after
  restoring all 182 current-main projects.
- Post-merge B2B Release build: passed, 0 errors and 5 existing warnings.
- Post-merge unit tests: Artist 11/11, Venue 12/12, Tenant 124/124, Deal 53/53, Concert 201/201;
  B2B architecture 8/8.
- Post-merge B2B integrations: Artist 17/17, Concert 153/153, Tenant 58/58, User 4/4, Venue 25/25.
- Post-merge plan graph: 0 errors and 0 warnings.
- Post-review B2B Release build: passed, 0 errors and 2 generated Reqnroll nullable-context warnings.
- Post-review Kernel unit tests: 241/241; B2B architecture tests: 8/8.
- Exact assets audit: Concert Application resolves Reunion, Reunion.Validation, and Reunion.Errors
  alpha.2; Concert API additionally resolves Reunion.AspNetCore alpha.2.
- Source/config audit: no B2B legacy carriers, alpha.1 pins, caller-actionable `DomainException`
  guards, new `[ErrorCode]` compatibility attributes, or added code comments; `git diff --check` clean.
- Docker health passed with a fresh-container host-to-container HTTP data round-trip before the
  successful integration sequence.
- Current-main focused financial-operation entity/workflow unit slice: 5/5.
- Final B2B Web/architecture closure build after direct Reunion conversion cleanup: passed, 0 errors
  and 0 warnings.
- Final focused unit and contract slices: Concert 72/72 and Tenant 26/26.
- Shared typed-result architecture tests: 24/24, including the no-HTTP-exception boundary rule.
- B2B architecture tests: 8/8. Deployable B2B Web/module runtime has no Payment runtime namespace or
  project reference; canonical AppHost/E2E orchestration remains outside that runtime boundary.
- Scoped changed-file formatting: passed. The full-solution formatter reports only pre-existing
  whitespace/style debt outside this checkpoint.
- Final affected unit suites: Artist 11/11, Concert 211/211, Deal 53/53, Tenant 128/128, User 1/1,
  and Venue 12/12. Focused Concert result/contract tests passed 72/72; focused Tenant tests passed
  26/26.
- The 11-test `ApplicationApiTests` HTTP contract slice compiles after updating its dashboard helper
  for the published `Option<ArtistDashboardCounts>` contract. Local execution could not start because
  Docker/Testcontainers was unavailable; every failure occurred during fixture construction before
  application code ran. Draft-PR integration CI owns the healthy-runner execution.
- Final changed-file formatting verification and `git diff --check`: passed.
- Final result-boundary commits: `fef0d2007`; this commit corrects the follow-up Option audit.
- B2B result/Option construction audit: target-typed absence branches use `return null;`, present
  reference payloads return directly, and success values, typed errors, and forwarded typed results
  use Reunion's implicit conversions wherever C# supplies a target type. `ToOption()` remains only
  for nullable value types and composition; explicit Result factories remain only where generic
  `Bind`/`BindAsync` inference has no result target, and named `Success<T>` remains for interface-typed
  payloads that C# forbids user-defined conversion from. The B2B architecture closure builds with
  0 warnings and 0 errors; rebuilt affected suites pass Artist 11/11, Concert 211/211, Tenant 128/128,
  User 1/1, and Venue 12/12; architecture passes 8/8.
- Current-main reconciliation restores platform `0.1.0-alpha.0.980`, retains MTD Payment reporting,
  and short-circuits missing Artist/Venue identities as implicit `None` before Concert or Payment
  calls. B2B architecture/Web closure builds with 0 warnings and 0 errors; Artist passes 16/16,
  Venue 17/17, architecture 8/8, and scoped formatting passes.
- Module-facade follow-up builds: B2B Web passed with 0 warnings/errors; Customer Web passed with 0
  errors and the existing sealed `UserEntity` constructor warning.
- Module-facade focused units: B2B Artist 16/16, Venue 17/17; Customer User 15/15, Concert 25/25,
  Ticket 37/37. B2B architecture passed 9/9, including direct Reunion package ownership and the new
  no-persistence-or-mapper facade dependency guard.
- Focused B2B User integration passed 4/4 against the restored platform `0.1.0-alpha.0.983` graph.
  The first fixture attempt proved stale local assets (`Payment.Client` 0.973 versus pin 0.983); the
  restored run reached Docker and passed all application assertions.
- Changed-project formatting and `git diff --check` pass. Customer solution-level formatting still
  hits the known Roslyn generated-document property limitation, so all affected Customer projects
  were verified individually. Rename/source greps are zero and the plan graph passes 0 errors,
  0 warnings.
- Current-main `0.1.0-alpha.0.985` revalidation: B2B Web and Customer Web build with 0 errors; focused
  units pass B2B Artist 16/16 and Venue 17/17 plus Customer User 15/15, Concert 25/25, and Ticket
  37/37; B2B architecture passes 9/9; B2B User integration passes 4/4 on the shared integration
  harness. Plan graph and `git diff --check` pass with 0 errors and 0 warnings. The only build warning
  is the existing protected constructor on each service's sealed `UserEntity`.
- Platform `0.1.0-alpha.0.988` revalidation: B2B Web and Customer Web build with 0 errors; the same
  focused unit slices pass 16/16, 17/17, 15/15, 25/25, and 37/37; B2B architecture passes 9/9; B2B
  User integration passes 4/4. Plan graph and `git diff --check` remain clean.
- Current-main merge commit `6c1e84101` preserves the B2B saga transport fixture and current-main
  Customer Review `ToCreatedAtActionOrProblem` behavior. The complete B2B Reunion family is alpha.7.
  Every eligible backend controller projection now uses its terminal overload; the repository-wide
  `Map(...).ToOk/Created*` inventory is zero. B2B retains exactly five custom `ToActionResult` calls:
  three typed file responses, one bodyless unit-result Created response, and Deal's polymorphic
  formatter. Alpha.7 required the file lambdas to return explicit `ActionResult<FileDownload>` values
  and Customer Ticket's projection lambda to name `ValidationResult` to disambiguate overloads.
- Direct Release builds pass with 0 warnings/errors for B2B Artist, Venue, and Concert APIs plus
  Customer Ticket API. B2B architecture passes 9/9; plan graph and `git diff --check` are clean.
  The B2B Web closure and project formatters timed out during local workspace loading without compiler
  or formatting diagnostics. Docker is unavailable, so current-alpha.7 HTTP integration and the wider
  closure/carve/test/format matrix remain exact-head PR CI gates.

## Decisions and deviations

- The inherited dirty tree interleaved Checkpoints 8 and 9 before Checkpoint 8 was committed. Preserve
  it and use one verified combined commit instead of risking a semantic split through partial staging.
- Customer Docker did not block alpha.2 or Checkpoint 9 implementation.
- Shared compatibility contraction is owned downstream. B2B removes its own usage but preserves the
  old Kernel carrier and tests until Auth and Customer delivery gates are terminal.
- Do not kill unrelated .NET processes owned by parallel Auth, Customer, Shared, or platform work.
- Checkpoint 10 is limited to SEC1's accept/withdraw/cancel capture, deposit, and refund paths. Concert
  finish/settlement remains outside this finding.
- B2B does not need a second recovery runner. Its durable outbox redelivers commands; Payment owns the
  persisted operation journal, resumes pending work with the same operation ID, and replays terminal
  outcomes. B2B's inbox makes outcome consumption idempotent.
- Sending a published command is distinct from handling it. `Sends<T>` adds outbound type resolution
  without adding the command to `RegisteredCommandTypes`, so Azure Service Bus does not create a
  Payment command receiver in B2B.
- The pre-merge plan graph's 13 unrelated stale-ledger errors belonged to the old branch snapshot;
  the current-main graph is the authoritative post-reconciliation gate.

## Downstream handoffs

- Owning ledger: `plans/typed-result/B2B_PAYMENT_SAGA_PRODUCER_PROGRESS.md`.
  Gate complete: Payment `0.1.0-alpha.0.973` is published and platform-sync PR #547 is terminal green.

- Waiting ledger: `plans/typed-result/REUNION_SHARED_CONTRACTION_PROGRESS.md`.
  Gate: B2B must be delivery-ready and identify every remaining old carrier, terminal, and third-party
  dependency outside its owned scope.
- Waiting ledger: `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PROGRESS.md`.
  Worktree: not created; reserved branch `Refactor/dotnet-11_b2b-workflow-unions`.
  Gate: the B2B typed-result source PR and every resulting publication/platform-sync gate must be
  terminal and green. At that gate, update the dependent ledger on current main and surface its
  implementation pointer.
