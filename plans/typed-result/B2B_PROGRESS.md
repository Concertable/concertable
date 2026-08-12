# B2B typed-result migration progress

- Plan: `plans/typed-result/B2B_PLAN.md`
- Roadmap: `plans/typed-result/TYPED_RESULT_MIGRATION_ROADMAP.md`
- Roadmap item: `typed-result/b2b`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-B2BTypedResultMigration`
- Branch: `Refactor/B2BTypedResultMigration`
- PR: not opened
- Local head: `ef24a9e39832fe06596877809da6bc668797a9be`
- Review watermark: `3d50d321c62fc7b9bc302aa9b2cbb93d77aa28b0`
- Dependency/package gate: open; `Reunion`, `Reunion.Validation`, `Reunion.Errors`, and
  `Reunion.AspNetCore` `0.1.0-alpha.2` resolve from normal configured feeds
- Main reconciliation: fetched `origin/main` at `b94028d3fe39ce2495bc9555ca13a5e6992272ee`;
  the dirty worktree is 211 commits behind and must be committed before merging main

## Current state

Checkpoints 1-7 are committed. Checkpoints 8 and 9 are implemented and verified together in the
current dirty worktree because this session resumed an interrupted tree in which their changes were
already interleaved. Do not split, stash, or discard that work. There are 84 changed/untracked paths.

Checkpoint 8 now uses the complete Reunion alpha.2 family. The two custom DI validators return
`Reunion.Validation.ValidationResult`; B2B no longer contains `FluentResults`,
`Concertable.Kernel.Functional`, or `Concertable.Shared.Api.Results`; the obsolete shared Kernel
`ValidationErrors` carrier and its tests are deleted. Direct package ownership is green in the B2B
architecture suite.

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

The Reunion integration close-out and stop-hook scope correction are already merged independently on
`origin/main` by PR #512. The later main merge must absorb those changes; do not recreate the deleted
`REUNION_INTEGRATION_PLAN.md` or `REUNION_INTEGRATION_PROGRESS.md` files here.

SEC1 is a delivery decision, not an implementation blocker. Its durable B2B + Payment saga/package
cut-over remains recorded in `api/Concertable.B2B/src/Modules/Concert/TECH_DEBT.md` and must be decided
before delivery.

## Next Steps

1. Commit the verified interleaved Checkpoint 8-9 tree as one coherent checkpoint. Do not push.
2. Merge current `origin/main` into the now-clean branch, preserving this ledger's authoritative B2B
   state while absorbing PR #512. Run the local plan graph, repeat affected build/test gates, and
   resolve any reconciliation fallout.
3. Use the `incremental-review` skill from watermark `3d50d321c`, address every new finding serially,
   and update this ledger. Do not push, open a PR, or merge B2B until separately instructed.

## Completed work

- Checkpoints 1-5 migrated Deal, Tenant, Venue/Artist, User, and Payment-independent Concert outcomes.
- Checkpoints 6-7 migrated payment/cancel/finish workflows, retryable completion faults, direct Reunion
  carrier/terminal ownership, and complete B2B FluentResults removal; implementation commit
  `e229afb581c829279ca821b0a85729c4c4f0f441`.
- The staged review covered `1043a9178..e229afb58`; the prior incremental review covered later commits
  through `3d50d321c`. NAT1-NAT5, SEC2, and CV1 are fixed. SEC1 is deferred to the owning tech-debt
  entry as a pre-delivery decision.
- Checkpoints 8-9 and their alpha.2 package reconciliation, direct unit coverage, and HTTP contract
  coverage are implemented and verified in the dirty tree.

## Verification

- `dotnet build api/Concertable.B2B/Concertable.B2B.slnx --configuration Release --no-restore -m:1
  -nr:false`: passed, 0 errors and 4 existing warnings (two Concert integration nullable warnings and
  two generated Reqnroll nullable warnings).
- Affected unit tests: Artist 11/11, Venue 12/12, Tenant 124/124, Concert 151/151.
- B2B architecture tests: 8/8.
- B2B Artist integration: 17/17.
- B2B Venue integration: 25/25.
- B2B Tenant integration: 58/58.
- B2B Concert integration: 153/153; Customer Concert integration: 11/11.
- Full `api/Concertable.slnx` Release build: passed, 0 errors and 2 existing generated E2E
  nullable-context warnings.
- Scoped changed-file formatting and immediate `--verify-no-changes`: passed.
- Exact assets audit: Concert Application resolves Reunion, Reunion.Validation, and Reunion.Errors
  alpha.2; Concert API additionally resolves Reunion.AspNetCore alpha.2.
- Source/config audit: no B2B legacy carriers, alpha.1 pins, caller-actionable `DomainException`
  guards, new `[ErrorCode]` compatibility attributes, or added code comments; `git diff --check` clean.
- Docker health passed with a fresh-container host-to-container HTTP data round-trip before the
  successful integration sequence.

## Decisions and deviations

- The inherited dirty tree interleaved Checkpoints 8 and 9 before Checkpoint 8 was committed. Preserve
  it and use one verified combined commit instead of risking a semantic split through partial staging.
- Customer Docker and SEC1 are later verification/delivery concerns and did not block alpha.2 or
  Checkpoint 9 implementation.
- Do not kill unrelated .NET processes owned by parallel Auth, Customer, Shared, or platform work.
- The origin/main plan graph reports 13 errors only in stale non-B2B ledgers on this 211-commit-behind
  branch. B2B itself satisfies the graph contract; the required post-commit main merge owns resolving
  the stale snapshot instead of editing unrelated plan state here.

## Event log

### 2026-08-12 - Checkpoints 8-9 verified

- Scoped formatting and `--verify-no-changes` passed over all changed C# files.
- Docker health passed; Artist 17/17, Venue 25/25, Tenant 58/58, B2B Concert 153/153, and Customer
  Concert 11/11 integrations are green.
- The full Release solution build passed with 0 errors; affected unit and architecture suites passed
  at Artist 11/11, Venue 12/12, Tenant 124/124, Concert 151/151, and architecture 8/8.
- Static source/package audits and `git diff --check` passed. Checkpoint 9 is complete; the combined
  Checkpoints 8-9 tree is ready for its required coherent commit.

### 2026-08-12 - Host-capacity verification gate reopened

- No Concertable build or test command was active; the machine had one unrelated long-lived `dotnet`
  process.
- `origin/main` remained `b94028d3fe39ce2495bc9555ca13a5e6992272ee`; the branch is still 211
  commits behind and 46 ahead, so the dirty checkpoint must be verified and committed before merging.
- Removed the temporary host-capacity blocker and resumed the pending verification sequence.

### 2026-08-11 - Checkpoints 8-9 implementation reconciled; verification paused for host capacity

- Upgraded all B2B Reunion-family pins to alpha.2 and removed unnecessary direct `Reunion` references
  from Artist/Venue unit-test projects.
- Completed structured domain outcomes and direct service mappings for Artist, Venue, Tenant,
  invitations, and door revenue; added direct unit and HTTP contract coverage.
- Corrected null tax-compliance mapping, new-code derivation, and redundant async continuations found
  during reconciliation.
- Passed the B2B build, affected unit tests, architecture tests, Artist integration, package/source
  audits, Docker health, and whitespace audit.
- Paused the remaining environment-sensitive gates after Testcontainers startup failed under unrelated
  concurrent .NET load; no application failure was observed.
