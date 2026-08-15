# Code review - Refactor/DataAccessRepositoryPermissionHierarchy

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed - don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `c3afdb4b2fd137cbf406dfeb7174d9c082968c4d`  _(2026-08-15)_
**Security-reviewed up to commit:** `c3afdb4b2fd137cbf406dfeb7174d9c082968c4d`  _(2026-08-15)_

> Range reviewed: `429581025..94d7664ad` (2 commits).
> Status legend: `[ ]` todo - `[~]` in progress - `[x]` done - `[wontfix]` (note why).

## Findings

- [x] **BUG1 - HIGH - correctness** - `api/Concertable.DataAccess/Concertable.DataAccess.Infrastructure/ReadDbContext.cs:7`
  The shared `ReadDbContext` now owns the generic configuration-provider/schema behavior. The redundant Customer `ReadDbContext` and B2B `PublicDbContext` intermediaries were removed, and all six concrete module read contexts derive the shared base directly.

## Incremental review - 2026-08-14

> Range reviewed: `94d7664ad..b850ea4b1` (1 commit).

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.

## Incremental review - 2026-08-14

> Range reviewed: `b850ea4b1..350ae02a1` (64 commits).

- [x] **BUG2 - MEDIUM - test coverage** - `api/Concertable.B2B/src/Modules/Artist/Concertable.B2B.Artist.Infrastructure/Repositories/ArtistOrgIdentityLookup.cs:15`
  Added focused Artist/Venue lookup tests that seed through the tenant contexts, read without tenant context, and cover both found and absent tenants.

Security review found no issues in the Auth, Payment, Contracts, and configuration paths included through the merged-main portion of the range.

## Incremental review - 2026-08-14 (BUG2 follow-up)

> Range reviewed: `350ae02a1..580426684` (2 commits).

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, security-sensitive paths, and test coverage of changed paths.

## Incremental review - 2026-08-15

> Range reviewed: `580426684..beb0bd91d` (38 commits).

- [x] **CV1 - MEDIUM - C# conventions** - `api/Concertable.B2B/src/Modules/Conversations/Concertable.B2B.Conversations.Infrastructure/Handlers/ParticipantProfileProjectionHandlers.cs:10`
  Replaced both captured primary constructors with explicit readonly fields and constructors as required by `api/agents/CODE_CONVENTIONS.md`.

- [x] **BUG3 - MEDIUM - test coverage** - `api/Concertable.B2B/src/Modules/Conversations/Tests/Concertable.B2B.Conversations.UnitTests/Services/MessageServiceTests.cs:11`
  Notification and inbox tests now assert the event-fed participant profile, and the inbox tests cover the missing-profile fallback introduced by the sender-resolution rewrite.

Security review found no issues. The additive Venue contract property preserves the existing positional wire shape, payment-event metadata remains validated by transaction type, and removing ready-event merge workflows reduces repository write authority.

## Incremental review - 2026-08-15 (CV1/BUG3 follow-up)

> Range reviewed: `beb0bd91d..5c6ab849f` (1 commit).

No issues found. The explicit handler constructors comply with the repository convention, and the focused tests now verify projected notification and inbox senders plus the missing-profile fallback. No security-sensitive behavior changed.

## Incremental review - 2026-08-15 (delivery checkpoint)

> Range reviewed: `5c6ab849f..fd2c51386` (2 commits).

No issues found. The range contains only the review record and plan-managed push checkpoint; no runtime or security-sensitive behavior changed.

## Incremental review - 2026-08-15 (current-main merge)

> Range reviewed: `fd2c51386..495fd7900` (7 commits).

No issues found. The range merges current `origin/main`; its incoming changes are limited to frontend documentation, agent guidance, and their documentation-reachability guard.

## Incremental review - 2026-08-15 (current-main checkpoint)

> Range reviewed: `495fd7900..a2c2cbd33` (1 commit).

No issues found. The checkpoint records the reviewed current-main merge and verified push; no runtime or security-sensitive behavior changed.

## Incremental review - 2026-08-15 (merge-queue fix)

> Range reviewed: `a2c2cbd33..016bd25fb` (1 commit).

No issues found. The E2E reseeding host now uses the same in-process event dispatch registration as
the B2B web host, so Artist/Venue seed events populate Conversations through the production projection
handlers. The change does not alter runtime authorization, service boundaries, or security-sensitive code.

## Incremental review - 2026-08-15 (second current-main merge)

> Range reviewed: `016bd25fb..f80bd66c5` (8 commits).

No issues found. Besides the already-reviewed E2E fix checkpoint, the range brings in current main's
plan graph and handoff-hook changes plus their tests and guidance. It does not add another runtime or
security-sensitive change to this PR.

## Incremental review - 2026-08-15 (fix checkpoint transport)

> Range reviewed: `f80bd66c5..e39b3759c` (1 commit).

No issues found. The commit records the clean review and plan graph state for the replacement remote
head; no runtime or security-sensitive behavior changed.

## Incremental review - 2026-08-15 (OIDC navigation fix)

> Range reviewed: `e39b3759c..a36851c84` (1 commit).

No issues found. The mount-persistent guards prevent React Strict Mode from initiating concurrent
PKCE redirects while preserving the existing authenticated-route behavior across Customer, Venue,
and Artist. The trace supports the failure mechanism, and all four web boundary builds pass.

## Incremental review - 2026-08-15 (third current-main merge)

> Range reviewed: `a36851c84..8992a36cd` (5 commits).

No issues found. The range merges current main's independently reviewed Tenant invitation-outbox
change and tests. It does not overlap the login redirect fix, participant projection, repository
permissions, or another security-sensitive path changed by this branch.

## Incremental review - 2026-08-15 (OIDC checkpoint transport)

> Range reviewed: `8992a36cd..dc1f55591` (1 commit).

No issues found. The commit transports the clean OIDC/current-main review and plan checkpoint; no
runtime or security-sensitive behavior changed.

## Incremental review - 2026-08-15 (published DataAccess baseline)

> Range reviewed: `dc1f55591..c3afdb4b2` (47 commits).

No issues found. Checked the producer/consumer seam resolutions and incoming current-main changes
through the native correctness, security, microservice-isolation, module-boundary, seeding, C# and
frontend convention, and changed-path test-coverage lenses. The three Customer contexts retain the
consumer migration to the published shared `ReadDbContext`; all incoming runtime changes were already
reviewed on their owning merged PRs and do not overlap the repository permission hierarchy.
