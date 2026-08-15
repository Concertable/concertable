# Code review - Refactor/DataAccessRepositoryPermissionHierarchy

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed - don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `5c6ab849f477df411db357ee6e050eba7b41fdfb`  _(2026-08-15)_
**Security-reviewed up to commit:** `5c6ab849f477df411db357ee6e050eba7b41fdfb`  _(2026-08-15)_

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
