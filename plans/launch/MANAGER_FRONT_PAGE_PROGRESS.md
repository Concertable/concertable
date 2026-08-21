# Manager Front Page — Session Feedback & Decisions

- Plan: `plans/launch/MANAGER_FRONT_PAGE_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/manager-front-page`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Feature-launch-dashboard-b2b-consumer`
- Branch: `Feature/launch_dashboard-b2b-consumer`
- PR: [#563](https://github.com/Concertable/concertable/pull/563) — draft implementation PR

Captured during Phase A implementation. These supersede the original plan
where they conflict. Read alongside [MANAGER_FRONT_PAGE_PLAN.md](MANAGER_FRONT_PAGE_PLAN.md).

## Next Steps

1. Merge current `origin/main`, including delivered producer PR [#685](https://github.com/Concertable/concertable/pull/685), into consumer PR [#563](https://github.com/Concertable/concertable/pull/563).
2. Rebuild the frontend packages and all four SPAs against published `@concertable/web@0.1.0-alpha.0.4658`, refresh the consumer review marker, push an exact head, and require CI green.
3. Complete the remaining Phase A.8 authenticated seeded venue/artist UX review at desktop, tablet, and mobile widths before consumer delivery.

## Reviews

- Full correctness/security review of `836a15a5..41d0189f3` is recorded in
  `reviews/Feature-launch_dashboard-b2b-consumer.md`; all seven findings are closed and both markers are stamped to
  code checkpoint `41d0189f3c0c4d5fe081eb654f91c871ab0e86b7`.
- Producer full and incremental review is recorded in `reviews/Feature-launch_dashboard-frontend-package-expand.md`;
  its only finding is closed. Consumer finding ARCH1 is closed, and the incremental correctness/security review of
  `9be56b9d..90a386b1` found no new issues across 33 commits. CI2 is closed, and the incremental review of
  `90a386b1..510bd491` found no new issues. The correctness marker is stamped to work head
  `510bd491bd67dd84216f6a5dc419aa094241d673`; no security-sensitive production code changed after the security
  marker at `90a386b1416f2179eaabef3e7b8068eef8594775`.

- Current-main correctness/security review is recorded in the same review file. SER3, COR3, CI3, FE2, CI4, and ARCH4
  are closed; both review markers are stamped to code checkpoint `7529c57616b4632b6ce2fc4a78fc0cbc8872508e`.

## Current implementation

- **Review-route producer delivery and publication are complete.** Producer PR
  [#685](https://github.com/Concertable/concertable/pull/685) was reconciled with current main, reviewed with no
  findings, and locally passed 31 shared-web tests, all five frontend package builds, and all four SPA production
  builds. Exact-head CI run
  [`32473228969`](https://github.com/Concertable/concertable/actions/runs/32473228969) passed 69 jobs plus its
  completion sentinel. Because the PR changes a published public package shape and routing behavior, merge-group run
  [`32474592295`](https://github.com/Concertable/concertable/actions/runs/32474592295) ran and passed the hard floor,
  API E2E, and UI E2E before #685 merged as `e795b22bb7b93f12c4e7f07e14f306ef6dc148c1`. Exact-merge frontend
  publication run [`32477431679`](https://github.com/Concertable/concertable/actions/runs/32477431679) then
  published and feed-verified `@concertable/web@0.1.0-alpha.0.4658`, including the route-provider exports. The PR was
  app-only, so no platform-sync PR is expected.

- **Reconciled consumer work-head push verified.** Starting remote and PR head
  `d59bc7ac9a73020babe5f870f493bb30501e9fa9` advanced to merge work head
  `4d8e3dac50ab291882e8091329952f7a349895dc`; fetch verification proved local HEAD, the remote-tracking branch, and
  draft PR #563 all equal. Exact-head CI run
  [`32472741901`](https://github.com/Concertable/concertable/actions/runs/32472741901) has started, but its standalone
  frontend carve result remains delivery-gated on producer PR #685's package publication.

- **Current-main handoff repair and opportunity materialization fix are locally verified.** The consumer is reconciled
  with `origin/main` commit `cf644420b2a5217f1f7c9a4f3af06e67933aa71b`. Main's extracted
  `AddB2BWebHost()` composition is retained with the branch-only `TenantActivityRecordedEvent` publication, and all
  six retired dashboard fixture files remain deleted. Exact-head CI run
  [`32425916458`](https://github.com/Concertable/concertable/actions/runs/32425916458) also exposed a real null
  `OpportunityEntity.Venue` invariant in the branch's newly required `OpportunityDto.VenueName` mapping. Active venue
  reads now include `Venue`, while both create paths attach the already-loaded tracked venue to new opportunities.
  The plan graph passes with 0 errors/warnings; all five frontend package tiers and all four SPA production builds
  pass. The focused Concert integration project builds, but its 11 selected tests cannot start because Docker is
  unavailable in this checkout; the broader B2B solution reaches the shared Notification infrastructure project and
  then hits a local intermediate-output copy race that does not reproduce from `main`, so exact-head CI owns both
  environment-dependent gates.

- **The exact-head consumer failure is diagnosed and its publish-first producer is green.** Draft PR #563 exact-head
  run [`32421218561`](https://github.com/Concertable/concertable/actions/runs/32421218561) deterministically failed the
  standalone Venue, Artist, and Customer web carves because the branch consumes `ReviewRouteProvider`,
  `b2bReviewBasePath`, and `customerReviewBasePath` from monorepo source while the published `@concertable/web`
  package restored by those carves does not contain them. The prerequisite camel-case producer merge's frontend
  publication run [`32418971755`](https://github.com/Concertable/concertable/actions/runs/32418971755) had also failed:
  `verify-fe-package.mjs` still constructed the now-invalid `"Rock"` genre, so no replacement frontend packages were
  published and consumer PR #600 remains feed-blocked too. Draft producer PR
  [#685](https://github.com/Concertable/concertable/pull/685), exact head
  `3851e2f02cf040af81c8f06751b7040210942b4b`, repairs the package verifier and adds the surface-configurable review
  route provider with customer-compatible defaults and focused route tests. Its exact-head run
  [`32424120414`](https://github.com/Concertable/concertable/actions/runs/32424120414) completed successfully, including
  every frontend package/standalone carve and the backend matrix. After explicit review and merge authorization, its
  exact merge must publish and verify the new alpha; then #563 can merge current main, rebuild all four SPAs, push an
  exact head, require CI green, and complete the remaining Phase A.8 authenticated seeded venue/artist UX review.

- **Current-main reconciliation and review-route normalization are locally green and committed in this commit.** The
  full 657-commit drift through `origin/main` commit `087a65cf` is reconciled without restoring removed public
  repositories, obsolete module lookups, or stale package ownership. B2B Artist and Venue expose singular nested
  review resources at `artist/{id}/review` and
  `venue/{id}/review`; each parent controller owns its resource `RouteSegment`, each review controller owns
  `RouteSegment = "review"`, public reads retain current-main throttling, and an architecture test enforces
  controller-name/route-leaf consistency. Current-tenant recent reviews use
  `organization/{artist|venue}/review/recent`. The shared web review UI receives a route builder at app composition:
  B2B uses the singular contract while Customer explicitly retains its existing plural contract. Current-main's
  camel-case enum wire contract is preserved, including the branch-only withdrawn and cancelled activity cases.
  The venue opportunity-count projection remains behaviourally safe but is logged in
  `api/Concertable.B2B/TECH_DEBT.md` for migration from `IOpportunityRepository` to
  `IOpportunityReadRepository`/`IConcertReadDbContext`; no general `Query` escape hatch was restored. Generated Artist
  and Venue migrations were re-scaffolded from the merged model. The B2B Web build succeeds with 0 errors (the one
  `UserEntity` constructor warning is present in `origin/main`); the architecture suite passes 10/10 after removing
  two unused `Reunion` references from the incoming Admin unit-test project; and Venue, Artist, Customer, and Business
  production builds all pass. Starting remote and PR head `e67c9bca2384a1c22a5e0e14d9523d3cd1a3e594` advanced through
  `e67c9bca2..66c067d31`; fetch verification proved local HEAD, the remote-tracking branch, and draft PR #563 all
  equal reviewed work head `66c067d31ffcca7f782462c43e8bac0a4758f4e0`.

- **Exact-head CI follow-up is locally verified.** PR run
  [`31953565101`](https://github.com/Concertable/concertable/actions/runs/31953565101) passed every frontend carve,
  the backend build, and every integration carve, including the formerly failing B2B Concert suite. Its only failure
  was the shared repository architecture guard: Venue and Artist had introduced duplicate module-local generic
  `ReadRepository<TEntity>` wrappers. Both review repositories now inherit the canonical shared
  `ReadRepository<TEntity, int>` directly and the wrappers are deleted. The exact architecture test passes 1/1;
  Venue and Artist infrastructure both compile with zero errors.

- **Frontend carve fix push verified.** Starting remote head `20d3fda8c8792e36bf1b28d4b301f2832a9dba9e` advanced through
  `20d3fda8c..60c765abd`. Fetch verification proved local HEAD, the remote-tracking branch, and draft PR #563 all equal
  reviewed work head `60c765abd1e96aec7d89df808cc9b30a4b01fd68`. Replacement exact-head CI is the remaining remote gate.

- **Exact-head artist carve failure fixed locally.** CI run
  [`31950865437`](https://github.com/Concertable/concertable/actions/runs/31950865437) failed
  `carve-fe (web/b2b/artist)` because the standalone SPA restored a published `@concertable/b2b` package without the
  branch-local `OpportunityMatch` export; that also left the genre callback implicitly typed as `any`. The contract is
  artist-only and now lives in the artist SPA. The analogous venue-only `OpportunityApplicationMetrics` moved to the
  venue SPA before its carve ran. Shared dashboard code retains only two-manager `OpportunitySummary`.

- **Reviewed work-head push verified.** Starting remote head `2b67da3139dfa720a16a17e6e7048b5294fbf846` advanced through
  `2b67da313..73c83fe98`. Fetch verification proved local HEAD, the remote-tracking branch, and draft PR #563 all equal
  reviewed work head `73c83fe989b9768fc8e9cab59c9e69972ca63858`. Exact-head CI is the remaining remote gate.

- **Current-main review fixes are locally green.** The common application response preserves its role-specific actions
  under the typed-Result terminal, hidden messages are excluded from previews and unread state, and the duplicated
  Conversations test package reference is removed. B2B Web builds with 0 warnings/errors; Conversations unit tests
  pass 32/32. The current shell has no Node runtime, so draft-PR CI owns the TypeScript/package builds.

- **Current-main reconciliation and opportunity dashboard boundary — locally green.** The branch is reconciled with
  `origin/main`'s typed-Result/package changes without reintroducing exceptions. Opportunity dashboard orchestration now
  lives in `OpportunityDashboardService`; tenant-scoped venue metrics remain on `IOpportunityRepository`, cross-tenant
  artist match candidates remain on `IPublicOpportunityRepository`, repository-only intermediate shapes use the
  `Projection` suffix, service outputs compose the canonical `OpportunityDto`, and API response mappers own wire-only
  summaries and HATEOAS. The repository/service output naming rule is recorded in `api/agents/CODE_CONVENTIONS.md` and
  `api/AGENTS.md`. B2B Web builds with 0 warnings/errors; rebuilt Concert, Venue, and Artist unit suites pass 216/216,
  21/21, and 19/19. This shell still has no Node runtime, so draft-PR CI owns the affected TypeScript/package builds.

- **Repository/application/API boundaries and full review corrections â€” committed locally.** Review repositories now
  return persisted review/rating read models, services map application contracts, and only API response mappers own
  recent-review HATEOAS/serialization. Tenant activity persistence has a normal repository, review projection handlers
  use the generic tenant-id lookup, dashboard conversion/date logic is outside services, `.Result` is gone, list limits
  are server-owned, in-progress concerts remain upcoming, and permanently absent contract fields were deleted instead
  of represented as null/optional placeholders. B2B Web builds with 0 warnings/errors; Venue, Artist, Concert,
  Conversations, and Tenant unit suites pass 259/259; the four changed integration projects compile with 0 warnings/
  errors. Frontend package/SPA execution is locally blocked because this shell has no Node runtime; draft-PR CI owns
  those exact builds.

- **Exact-head CI contract correction — fixed and focused tests green.** PR run
  [`31913884736`](https://github.com/Concertable/concertable/actions/runs/31913884736) exposed six deterministic stale
  Concert integration assertions: five opportunity cases deserialized the controller's public response as the
  internal application DTO, and one application case expected the contract metadata route instead of the advertised
  PDF HATEOAS action. Commit `510bd491b` corrects only those tests. All six formerly failing SQL-backed cases pass
  locally from a short drive alias (4/4 create theory rows, then 2/2 seeded-opportunity and contract-link cases). The
  full local project remained active without assertion output until the ten-minute command cap. The reviewed work
  leg was pushed from checkpoint head `47ffc4ae8`; fetch verification proved local, remote-tracking, and PR #563
  heads all equal `510bd491bd67dd84216f6a5dc419aa094241d673`. Transport checkpoint `ec98e4480` then became the exact local,
  remote-tracking, and PR head. Replacement exact-head run
  [`31915550316`](https://github.com/Concertable/concertable/actions/runs/31915550316) passed all 56 substantive
  build, carve, unit, and integration jobs plus its completion sentinel.

- **Conversations API ownership — producer and package publication complete.** Producer PR
  [#591](https://github.com/Concertable/concertable/pull/591) adds a focused, tested `messageApi.getPreviews()` export
  under `@concertable/b2b/features/conversations`. Full merge-group run
  [`31909464706`](https://github.com/Concertable/concertable/actions/runs/31909464706) passed all 58 jobs and #591
  merged as `836a15a56257a0e35ca5ef5674b39e38eb6767ac`. Exact-merge frontend package publication run
  [`31911164238`](https://github.com/Concertable/concertable/actions/runs/31911164238) then packed, published, and
  verified all frontend tiers from the feed successfully.

- **Conversations ownership cut-over — implemented and locally verified.** Venue and artist inbox hooks now consume
  `messageApi.getPreviews()` from `@concertable/b2b/features/conversations`; both dashboard API objects have deleted
  their duplicate message methods. `@concertable/b2b@0.1.0-alpha.0.3721` resolves from the feed and its manifest
  contains the Conversations API runtime and declaration files. B2B package tests pass 17/17, frontend boundaries
  pass, and both venue and artist TypeScript plus production Vite builds pass. Sequential venue and artist feed-only
  standalone carves both restored and built successfully from commit `90a386b14`. That reviewed work head was pushed
  from starting remote/PR head `ec95772683e053975792f99229a1315e4e9c993d`; fetch verification proved local,
  remote-tracking, and PR #563 heads all equal `90a386b1416f2179eaabef3e7b8068eef8594775`.

- **Consumer review corrections — verified locally.** Optional application-action UI state uses `undefined`, and
  action/navigation/cache/toast orchestration has moved from the widgets into role-specific hooks. Dashboard shared
  types moved from the needless `deals/common.ts` nesting to `dashboard/types.ts`. Current application queries now
  use the locked `Opportunity.Period.End > now` relevance rule, with a focused real HTTP/SQL integration test green;
  serializer tests prove absent venue avatars are omitted; the inbox preview test asserts the actual latest seeded
  message; and both affected TypeScript projects compile. The branch review work order records all findings closed
  and the post-cut-over incremental review clean.

- **Frontend package gate — DELIVERED.** Producer PR [#578](https://github.com/Concertable/concertable/pull/578)
  passed exact-head CI and two full-E2E merge groups after `main` advanced, then merged as
  `d9bed4daf073dfb17d64a1308a878cabf370dfc3`. Publication run
  [31896533803](https://github.com/Concertable/concertable/actions/runs/31896533803) published and feed-verified
  `@concertable/b2b@0.1.0-alpha.0.3689`. Its clean source worktree was removed through the plan-managed close guard.

- **Optional wire contracts and Conversations ownership — committed locally.** Commit `01c9fbff6` imports
  `MessagePreview` from `@concertable/b2b/features/conversations`, removes it from universal dashboard types, and
  represents absent avatar, detail, banner, and review values as optional frontend properties. Backend response DTOs
  omit absent values from JSON, with focused serialization tests proving the wire contract. Before the current-main
  merge, B2B package tests passed 16/16; affected backend suites passed Conversations 9/9, Tenant 99/99, Concert
  134/134, Artist 8/8, and Venue 9/9; B2B Web built with 0 errors. The published tarball was then inspected directly:
  its export map contains `./features/conversations`, `MessagePreview.otherPartyAvatarUrl` is optional, and the
  Concerts barrel exports `actionLinkApi`. Current `origin/main` merged in `78bf8260a`; both venue and artist
  feed-only standalone carves passed, B2B package tests/build passed 16/16, boundary test and lint passed, and B2B Web
  built with 0 warnings/errors. Work head `78bf8260a20df9ff9254bf6e4e3085ba426ad540` was pushed and fetch verification
  proved it equal across local, remote-tracking, and PR heads.

- **B2B consumer checkpoint 3 — committed locally.** Recent venue/artist review endpoints, a tenant-owned persisted
  activity feed, and every remaining dashboard API integration are implemented in work commit `e4054a7e6`. Activity
  producers publish `TenantActivityRecordedEvent` through their owning transactional outbox; Tenant projects the
  current-tenant feed with inbox idempotency. The all-module migration re-scaffold changed only Tenant, Venue, and
  Artist as intended. Both dashboard SPAs now use real APIs throughout; persona switching and all fixtures are gone,
  and advertised application/contract actions execute through the HATEOAS links. `Concertable.B2B.Web` builds with
  0 warnings/errors; Tenant tests pass 98/98, Conversations 8/8, Concert 133/133, Venue 8/8, Artist 7/7, shared package
  tests 6/6; both integration projects compile cleanly; both TypeScript and production Vite builds pass; and the
  frontend boundary checker is green. SQL-backed test execution remains assigned to draft CI because Windows rejects
  the SQL SNI path in this deep worktree with `0x800700CE` before application startup.

- **B2B consumer checkpoint 2 — committed locally.** Canonical current-tenant endpoints now provide venue/artist
  applications, upcoming concerts, venue open opportunities, artist recommendations, and conversation message
  previews. Application representations are actor-specific through `ApplicationResponse<TActions>` with separate
  venue and artist action objects; the active membership exposes its tenant type so the shared resource route selects
  the correct representation. `MessagePreview` is the database projection and `MessagePreviewDto` is the enriched
  HTTP shape; the query is explicitly scoped to the active tenant and returns one latest message per counterparty with
  member-specific unread state. The later migration to exhaustive role-and-state discriminated unions is recorded in
  `api/Concertable.B2B/TECH_DEBT.md`. Work commit `16baf7cc4` builds through `Concertable.B2B.Web`; Concert unit tests
  pass 133/133, Conversations unit tests 8/8, Tenant unit tests 96/96, and the Concert integration project compiles
  with 0 warnings/errors. Seven focused SQL-backed endpoint tests were added; their local execution reached fixture
  startup but Windows rejected `Microsoft.Data.SqlClient.SNI.dll` at this deep worktree path with `0x800700CE`, so
  exact-head draft CI owns their short-checkout execution. The next implementation slice is recent venue/artist
  reviews, followed by activity persistence and the migration re-scaffold.

- **B2B consumer checkpoint 1 — pushed to draft PR #563.** Venue and artist overview endpoints now compose the
  current profile, profile-health checklist, Stripe Connect state, and review summary. Venue ticket revenue and artist
  payout charts consume the published monthly Payment reports and fill a fixed six-calendar-month series. Venue recent
  settlements consume the published Payment report and enrich booking ids through the Concert module with concert,
  counterparty, and direction data. `Concertable.B2B.Web` builds with 0 errors against platform
  `0.1.0-alpha.0.983`; focused Venue tests pass 7/7 and Artist tests pass 6/6. Work commit
  `d82f93cb7084d9d9412b4ff9b4d19ce758269652` was pushed from a new remote branch and verified as the exact local,
  remote-tracking, and PR head; exact-head draft CI run
  [31809913260](https://github.com/Concertable/concertable/actions/runs/31809913260) is in progress. The next checkpoint is
  the independent canonical list/review/inbox surface named in `## Next Steps`.

- **DELIVERED (2026-08-14).** The final incremental review found no new issues after the fixed settlement-completion-time
  defect. Exact-head PR CI run [31796969708](https://github.com/Concertable/concertable/actions/runs/31796969708)
  passed at `03dd59a561ab3a439d586f98d5757bc8966cb3ed`. Producer PR
  [#557](https://github.com/Concertable/concertable/pull/557) then passed its full API + UI E2E merge-group run
  [31797907306](https://github.com/Concertable/concertable/actions/runs/31797907306) and merged as
  `19f044ae37b1d600e01a59a0af7801c80e02202c`. Package publication run
  [31800448691](https://github.com/Concertable/concertable/actions/runs/31800448691) published platform
  `0.1.0-alpha.0.983`; generated platform-sync PR
  [#562](https://github.com/Concertable/concertable/pull/562) passed its build/unit/integration gate and merged as
  `7b876437779851765e99377b84799bf1991370bc`. The producer delivery chain is fully closed and its source worktree was
  removed through the plan-managed close guard.
- Producer implementation commit `0d37bfa7a` remains the published implementation baseline on draft PR
  [#557](https://github.com/Concertable/concertable/pull/557). Full review through `bc56de2d8` found BUG1: settlement
  reports used creation time instead of completion time. Fix commit `0eb0babfb` persists immutable `CompletedAt`, uses
  it for completed-settlement totals/months/recency, and adds the boundary coverage. Incremental correctness/security
  review of `bc56de2d8..0eb0babfb` found no new issues. Current `origin/main` merged conflict-free as `931dde050`; the
  effective PR source diff is unchanged beyond the reviewed fix. The reviewed current-main work head `9d9ffff66` was
  pushed from starting remote/PR head `bc56de2d8`; fetch verification proved local, remote-tracking, and PR heads all
  equal `9d9ffff66433a50f2e616029faa3ce3e0c8d0eb5`; the ledger transport commit then advanced all three to
  `93847e86a57ee4dd9016b88281104db53a399ca0`. Exact-head CI run
  [31789070465](https://github.com/Concertable/concertable/actions/runs/31789070465) exposed that B2B's concrete
  integration-test client had not implemented the three additive reporting methods. The compatibility fix supplies
  deterministic empty report results, and the exact CI-equivalent local-platform Release build of
  `Concertable.B2B.IntegrationTests.Fixtures` passes with 0 warnings/errors.
- Compatibility fix commit `8b7ba4e80` and its full incremental correctness/security review introduce no new findings.
  Reviewed work head `5904b8c567fab16207b604320a1f333d363643cd` was pushed from starting remote/PR head
  `93847e86a57ee4dd9016b88281104db53a399ca0`; a fetch then proved local, remote-tracking, and PR heads all equal the
  reviewed work head. Transport checkpoint `816a88b09e5f8fbb15ba9611bc8ee9539d72dbde` then became the exact local,
  remote-tracking, and PR head; exact-head CI run
  [31792858654](https://github.com/Concertable/concertable/actions/runs/31792858654) passed the full build, unit, and
  integration matrix. All review findings were closed, so the spent review work order was deleted in closeout commit
  `b52f0e28afa75d1f0f71b48773e2d0377b025881`; transport commit `1b0b46792842fb63916f7a299a7cc55de4d62ad3`
  became the exact local, remote-tracking, and PR head, and exact-head CI run
  [31793924515](https://github.com/Concertable/concertable/actions/runs/31793924515) passed. The merge-authorized
  re-review restored the work order, preserved the fixed BUG1 evidence, and found no new issues through `1b0b46792`;
  only plan/review checkpoints followed the last reviewed code commit. Reviewed work head
  `36dcdeb2c94d9d6e0a1d750b221c86983329a3c2` was pushed from starting remote/PR head
  `1b0b46792842fb63916f7a299a7cc55de4d62ad3`; fetch verification proved local, remote-tracking, and PR heads all
  equal the reviewed work head.
- Payment now owns agnostic reporting contracts for monthly ticket revenue, monthly settlement payouts, and recent
  settlements. Each aggregate materialises once in `TransactionRepository`; B2B will enrich opaque booking and owner
  identifiers after the published-client gate.
- The gRPC surface and `IManagerPaymentReportingClient` expose `Money`-based report records without venue, artist,
  concert, or dashboard concepts leaking into Payment.
- Local verification after current-main merge `931dde050`: plan graph passed with 0 errors/warnings; Payment Web build
  succeeded with 0 warnings/errors against platform `0.1.0-alpha.0.980`; focused domain/service tests passed 24/24;
  SQL `TransactionRepositoryAggregateTests` passed 5/5 against a real Testcontainers SQL Server. The earlier full
  `api/initial-migrations.ps1` re-scaffold also passed for the BUG1 model change.

**Item 3 — DELIVERED (2026-08-13).** Producer PR [#545](https://github.com/Concertable/concertable/pull/545) merged
(`3004fb52d`); consumer PR [#554](https://github.com/Concertable/concertable/pull/554) merged (`2dfe09cc9`) wired both
published `IManagerPaymentReportingClient` reporting RPCs into `VenueDashboardService` / `ArtistDashboardService`,
replacing the `MtdRevenueCents` / `MtdPayoutsCents` zero stubs (verified: no stub or TODO remains). Window is UTC
month-to-date, payee the fail-closed `ITenantContext.GetTenantId()`, exact month-start returns zero without a
degenerate `DateRange`, `Money.ToMinorUnits()` fills the `long` cents. Platform-sync PR #556 **merged** (2026-08-13,
21:15) — platform now `0.1.0-alpha.0.978`. Item 3's delivery chain is fully closed.

**Update (2026-08-13):** PR [#50](https://github.com/Concertable/concertable/pull/50) **merged** (2026-05-19)
— Phase A + B.9–B.11 are on `main`. The repo has since **carved** into `Concertable.B2B` /
`Concertable.Customer` / `Concertable.Payment` services; dashboard FE now lives under
`app/web/b2b/{venue,artist,shared}/`. Reconciled outstanding work:

1. ✅ **Migration re-scaffold (was item 3) — DONE.** The carve re-ran `api/initial-migrations.ps1`; the
   B2B `Concerts` table already carries the owned `Period_Start`/`Period_End` columns. No drift; nothing
   to run.
2. ✅ **`AcceptedAwaitingCheckout` KPI (was item 2, artist slice) — DONE**, PR
   [#414](https://github.com/Concertable/concertable/pull/414) on branch
   `Feature/launch_dashboard-accepted-checkout`. Added `IConcertWorkflowCapabilityRegistry.DealTypesWith<T>()`,
   a third `Accepted` + checkout-capable + upcoming applications query in `ConcertDashboardRepository`, the
   `ArtistDashboardCounts.AcceptedAwaitingCheckout` field + projection, and wired `ArtistDashboardService`.
   **2026-08-13:** layering refactor (`ec5e7b7bb`) moved `IConcertWorkflowCapabilityRegistry` out of the
   repo into `ConcertDashboardService`, passing the resolved checkout-capable `DealType` set down as a
   filter param (repo stays pure data-access). Branch was **330 commits behind `main`** — synced
   (`b711b9365`, one conflict in `ConcertWorkflowCapabilityRegistry.cs`: kept `DealTypesWith` on
   origin/main's `workflowTypes` rename), B2B build 0 errors, Concert unit tests 133/133, pushed.
   **2026-08-13 delivery:** incremental code + security review found no issues. PR #414 merged as
   `306f072af2683e25ddaf29c36688feaa0253a189` after its full API + UI E2E merge-group run passed.
   Package publication succeeded; cumulative platform-sync PR #541 superseded #539, updated the platform to
   `0.1.0-alpha.0.968`, passed build/unit/integration checks, and merged as
   `1c88858f93f648f1719fa9e4d273749b8932b364`.
3. ✅ **MTD revenue/payouts (was item 2, money slices) — DELIVERED.** See "Item 3 — DELIVERED" above. Producer #545
   + consumer #554 both merged. Ticket revenue sums `TicketTransaction.Amount`; artist payouts sum
   `SettlementTransaction.PayeeGrossMinor` (excludes payer-side commission). Two additive `ManagerPayment` RPCs on a
   new `IManagerPaymentReportingClient` (protobuf `Timestamp` + `Money`) kept `IManagerPaymentOperationsClient`
   source-compatible with B2B's concrete test client; B2B consumes the published `Concertable.Payment.Client` package,
   never producer source. Platform-sync #556 merged — platform on `0.1.0-alpha.0.978`; the chain is fully closed.
4. ✅ **B.11 pickup endpoints + Phase C FE cutover — IMPLEMENTED (draft PR #563).** Every overview, chart,
   canonical list, review, inbox, and activity endpoint is implemented and locally verified. Both SPAs use the real
   API, fixtures and persona controls are deleted, and HATEOAS actions are functional. Exact-head CI remains.
5. ⏳ **Phase A.8 — UX freeze (was item 1).** Review the live seeded venue and artist dashboards at desktop,
   tablet, and mobile widths. Exercise application actions and contract download. Needs the running authenticated B2B
   stack and Tommy's visual feedback.

## Naming & terminology

- **`Header` → `Overview`** everywhere. The dashboard top strip is named
  `Overview` (DTO: `VenueDashboardOverview` / `ArtistDashboardOverview`,
  endpoint: `/overview`, hook: `useVenueOverview` / `useArtistOverview`).
  Reason: `Header` collides with the existing search `Header` polymorphic
  type (`Artist | Venue | Concert`) at
  `app/shared/src/features/search/types.ts`.
- **`Concert` not `gig`** anywhere. Artist KPI label is "Upcoming concerts",
  endpoint is `/upcoming-concerts`, hook is `useArtistUpcomingConcerts`,
  hero is `ArtistNextConcertHero`. Backend entity is `ConcertEntity` so the
  FE matches.

## Architecture

- **Persona-specific dashboard code lives in the SPA, not in shared.**
  The plan originally placed `VenueDashboardPage` in
  `app/web/shared/src/features/venues/pages/`. **Wrong.** Dashboard widgets
  + page + hooks + fixtures + `dashboardApi.ts` + `types.ts` are 100% tied
  to the manager persona — they belong in `app/web/{venue,artist}/src/features/dashboard/`.
  Shared keeps only agnostic UI primitives and cross-cutting types.
- **Per-SPA `dashboardApi.ts` following the `venueApi.ts` pattern.** One
  flat const object per SPA, default-export. No interface, no mock vs real
  dispatcher, no separate mock file. The mock IS the dashboardApi — when
  real endpoints land, swap each method body from
  `return venueFixtures[selectPersona()].xxx;` to
  `const { data } = await api.get(...); return data;`. Export shape stays.
  Reason: React-idiomatic is object literal + structural typing, not
  interface+impl (that's a .NET/Angular DI-container reflex that doesn't
  transfer to React).
- **No `take` / `monthsBack` parameters on FE hooks.** Server decides
  response size. Hook signatures are nullary (`useVenueInbox()` not
  `useVenueInbox(5)`). The `/charts/ticket-revenue?monthsBack=6` and
  `?take=N` query strings in the original plan should be dropped — the
  server is authoritative on window/limit.
- **One hook per file in a `hooks/` folder.** Matches the existing
  `app/shared/src/features/venues/hooks/` convention. Each section gets
  its own `useVenueXxx.ts` file plus a barrel `index.ts`.

## Data model

- **`ProfileHealth` is a single items list with `done: boolean` per item.**
  Not split `items[]` + `done[]`. BE returns the full checklist;
  FE renders rows, ticking `done: true` ones. Shape:
  ```ts
  interface ProfileHealthItem { id; label; href; done: boolean }
  interface ProfileHealth { completeness: number; items: ProfileHealthItem[] }
  ```

## Code quality rules from review

- **`formatCurrency` lives in `@concertable/shared/lib`** (single source).
  Signature: `formatCurrency(cents, { currency?, compact?, fractionDigits? })`.
  Don't duplicate locally in widgets.
- **`<ChartTooltip>` is our wrapper around recharts' `<Tooltip>`.** All
  the recharts type-juggling lives inside `ChartTooltip.tsx`. Charts compose
  `<ChartTooltip currency="GBP" />`, never inline a content callback.
- **No `as string` casts that lie.** Use `String(x)` to coerce honestly
  (e.g. `key={String(p.dataKey)}` not `key={p.dataKey as string}`).
- **No overdefensive `?? ""` on values that are statically known.**
  Recharts payloads come from our `dataKey`s — they're deterministic.

## Mock-tier infrastructure (deleted in Phase C)

Phase C kept each `dashboardApi.ts` export shape and replaced its fixture bodies with real API calls. It deleted:

- `app/web/{venue,artist}/src/features/dashboard/fixtures/` — fake data per persona
- `app/shared/src/features/dashboard/persona.ts` — `FixturePersona`, `selectPersona`, `NOW` anchor + date helpers (`daysAhead`, `daysAgo`, `hoursAgo`, `monthsAgoIso`)
- `app/web/shared/src/features/dashboard/components/PersonaSwitcher.tsx` — dev-only floating control

Fixtures pin a deterministic dayjs anchor: `NOW = dayjs("2026-05-18T12:00:00Z")`. No live `Date.now()` calls.

## Artist diverges from venue layout

Artist isn't a mirror of venue:

- **No `ApplicationsToReview`** — artists don't review applications.
- **No `OpenOpportunities`** — artists don't post them.
- **No `Settlements`** — artists receive money, don't pay out.
- **`ApplicationsPipeline`** — artist's applications grouped by status
  (Pending / Awaiting payment / Confirmed / Rejected). Replaces venue's
  "applications to review".
- **`NextConcertHero`** — the most imminent concert promoted to a wide
  hero card with countdown ("In 4 days") + venue + ticket-sold progress.
  Sits where venue's upcoming-concerts strip sits.
- **`RecommendedOpportunities`** — full-width strip, more prominent than
  venue's open-opportunities widget. The artist's outbound funnel.

## Surviving file structure

```
app/shared/src/features/dashboard/
  contracts/common.ts          # shared DTOs (Activity, Application, Settlement, etc.)
  persona.ts                   # mock-tier — FixturePersona, selectPersona, NOW, date helpers
  polling.ts                   # DASHBOARD_POLLING tier constants (real product)
  index.ts                     # barrel: contracts/common + persona + polling

app/web/shared/src/features/dashboard/
  components/                  # agnostic UI primitives
    DashboardCard.tsx
    KpiTile.tsx
    MonthlyRevenueChart.tsx
    ChartTooltip.tsx
    ActivityFeed.tsx
    SectionGrid.tsx
    StripeConnectBanner.tsx
    ProfileHealthCard.tsx
    PersonaSwitcher.tsx        # dev-only
    WidgetState.tsx            # WidgetLoading / WidgetError / WidgetEmpty
    index.ts
  index.ts

app/web/venue/src/features/dashboard/
  VenueDashboardPage.tsx
  Venue*.tsx                   # 11 widget files
  dashboardApi.ts              # methods returning Promise<X>, currently fixture-backed
  types.ts                     # VenueDashboardOverview, VenueDashboardKpis
  hooks/
    useVenueOverview.ts        # one file per hook
    useVenueKpis.ts
    useVenueApplicationsToReview.ts
    useVenueInbox.ts
    useVenueUpcomingConcerts.ts
    useVenueTicketRevenue.ts
    useVenueOpenOpportunities.ts
    useVenueActivity.ts
    useVenueSettlements.ts
    index.ts
  fixtures/
    empty.ts mid.ts thriving.ts
    types.ts                   # VenueDashboardFixture
    index.ts                   # venueFixtures = { empty, mid, thriving }
  index.ts                     # exports { VenueDashboardPage }

app/web/artist/src/features/dashboard/  # same shape, artist-specific
```

## Open Phase A todo

✅ 1–10 all done. Phase A committed on `Feature/ManagerFrontPage` (commit `5fb54e96`) 2026-05-18.

11. **Phase A.8 — UX freeze.** Run the authenticated seeded B2B stack, inspect `/_venue/` and `/_artist/` as the
    corresponding managers, verify responsive collapse at desktop, tablet, and mobile widths, and exercise the
    advertised application actions and contract download. Do not bypass guards or restore fixture personas.

## Session-2 deltas (2026-05-18, committed)

- **Applications widgets** redesigned: flat 3-column table (counterparty / status / actions) on **shadcn DataTable + `@tanstack/react-table`** (installed via `npm -w @concertable/web-* install @tanstack/react-table`). Status grouping replaced with column. No FE sort — BE will `ORDER BY` when the endpoint lands.
- **HATEOAS per-role `ApplicationActions`** — each SPA owns its own `applicationActions.ts` with `ApplicationActionName` union, `ApplicationActions` mapped type, and `APPLICATION_ACTION_LABELS` record. Mirrors `Concert.Api/Mappers/ApplicationResponseMapper.cs`.
- **`Application` type per SPA** (`app/web/{role}/src/features/dashboard/types.ts`) — nests shared `OpportunitySummary` + (venue) `ArtistSummary`. Drops `href` (actions are the only way to act), drops flat opportunity fields and `canAccept/canDecline/canCheckout` booleans.
- **`OpportunitySummary` + `OpportunityCard`** now carry structured `Contract`. New `contractSummary(contract)` helper at `app/shared/src/features/contracts/format.ts` registry-formats it (`flatFee` → "£N", `doorSplit` → "N% door", etc.). `ContractSummaryLabel.tsx` imports it.
- **`ActionLink`** primitive lives in `app/shared/src/types/common.ts` next to `Pagination<T>`. Removed duplicate from `features/concerts/types.ts`.
- **Reviews widgets** show recent excerpt list + aggregate header (was single-number tile). New shared `RecentReviewsList` primitive, new `useVenueRecentReviews` / `useArtistRecentReviews` hooks.
- **Page wrapper** dropped `max-w-7xl` for full-bleed. `DashboardCard` is `h-full` so cards in paired-row sections stretch to the row's tallest height.
- **Dashboard controller scope refined** — only owns aggregations (`overview`, `kpis`, `activity`). Plain list endpoints (`applications`, `inbox`, `upcoming-concerts`, `settlements`, `recommended-opportunities`) hit canonical resource controllers filtered to "me". Updates the round-trip plan in PLAN.md.

## Session-3 deltas (2026-05-18, committed `094fd4d4` → `23c8fc4c`)

### B.9 — `ConcertEntity.Period` (commit `094fd4d4`)

- `ConcertEntity` drops `DateTime StartDate` / `EndDate`, owns `DateRange Period` (mirrors `OpportunityEntity`).
- `OwnsOne(e => e.Period, p => { p.Property(x => x.Start).HasColumnName("StartDate"); p.Property(x => x.End).HasColumnName("EndDate"); })` keeps DB column names identical.
- `ConcertSearchModel` (Search module's read projection over the same `Concerts` table with `ExcludeFromMigrations`) is untouched — same columns, no edits needed.
- Existing `QueryableConcertMappers` already project from `c.Booking.Application.Opportunity.Period.Start/End` via the nav chain — no DTO mapper changes.
- **Migration re-scaffold (`./initial-migrations.ps1`) deferred** until end of Phase B code work. Column names unchanged → no schema drift while deferred.

### B.10 — Specification pattern locked at dual-method shape (commits `e2193f46` + `23c8fc4c`)

```csharp
public interface IUpcomingSpecification<TEntity> where TEntity : class, IHasDateRange
{
    IQueryable<TEntity> Apply(IQueryable<TEntity> query);

    IQueryable<TParent> ApplyExpression<TParent>(
        IQueryable<TParent> query,
        Expression<Func<TParent, TEntity>> navigation);
}
```

Both overloads return `IQueryable` — Expression never escapes the spec impl. Internally, both call `private Expression<Func<TEntity, bool>> BuildPredicate()` (one source of truth for the rule + one `TimeProvider` read).

- **`ApplyExpression` uses `Concertable.Shared.Infrastructure.Expressions.ExpressionExtensions.Substitute`** — the existing extension (built on `ParameterReplacer`) that rewrites a predicate's parameter onto a navigation expression's body. Don't introduce a new `Lift` extension — `Substitute` is more general (returns `TResult`, not just `bool`) and already exists.
- `IDateRangeSpecification<T>` is symmetric: `Apply(query, range)` + `ApplyExpression<TParent>(query, nav, range)`.
- DI registered as open-generics in `AddSharedInfrastructure`.
- Single consumer of `ApplyExpression` today: `ConcertDashboardRepository`'s Application filter (`a => a.Opportunity`).

### B.11 — KPI endpoint shape (commits `d4f9a3a6` + `a91c7271` + `23c8fc4c`)

- **One SQL round trip per persona**, anchored on `VenueReadModels` / `ArtistReadModels`, projecting three (venue) / two (artist) scalar subqueries through new `QueryableVenueDashboardMappers.ToVenueCounts` / `QueryableArtistDashboardMappers.ToArtistCounts`. Matches the `ConcertHeaderRepository.SearchAsync` / `QueryableConcertHeaderMappers.ToHeaderDtos` precedent — single composed `IQueryable`, single materialisation.
- **`IConcertDashboardRepository`** is a dedicated read-shape repo (separate from `ConcertRepository` / `OpportunityRepository` / `ApplicationRepository`) — mirrors `ConcertHeaderRepository` precedent in Search. Per-aggregate count methods on the existing repos were tried then reverted; the dedicated repo is the right home for dashboard-shaped reads.
- **Cross-module orchestration lives in `IVenueDashboardService` / `IArtistDashboardService`**, not in the controller. Resolves "me" via `IXService.GetIdForCurrentTenantAsync`, calls `IConcertModule`, assembles wire DTO. `Task.WhenAll` of one task today; Payment slots into the second position when it lands.
- **Controllers are one-line delegates.** Return `NoContent` (204) when the service returns null DTO — read-model projection hasn't populated yet for that venue/artist. Honest about "you exist by auth, the data just isn't here yet" vs a real 404.
- **`Venue.Api.csproj` and `Artist.Api.csproj` no longer reference `Concert.Contracts`** — controllers don't touch Concert types anymore.

### Period semantics — locked (codified in PLAN.md → "Period semantics")

- **"Upcoming" (concerts)**: `Period.End > now` (includes in-progress). Via `IUpcomingSpecification<ConcertEntity>`.
- **"Open" (opportunities)**: `Period.Start >= now` (excludes in-progress). Inlined in `ConcertDashboardRepository`, NOT via spec — different rule than "upcoming" by design (open = still accepting apps, not "still happening").
- **"Still relevant" (applications)**: parent `Opportunity.Period.End > now`. Via `IUpcomingSpecification<OpportunityEntity>.ApplyExpression(a => a.Opportunity)`.

### Wire-shape stubs at merge time (TODOs in code)

The KPI DTO matches the FE wire shape verbatim, with three fields hard-stubbed at 0 because their dependencies aren't built yet. Each has a TODO at the literal pointing at the missing dependency:

- `MtdRevenueCents: 0` → `IManagerPaymentModule.GetVenueTicketRevenueMtdAsync` (not built)
- `MtdPayoutsCents: 0` → `IManagerPaymentModule.GetArtistPayoutsMtdAsync` (not built)
- `AcceptedAwaitingCheckout: 0` → `IConcertWorkflowCapabilityRegistry` / `IAcceptsCheckout` workflow lookup (lift `ApplicationResponseMapper.cs` per-application logic to an aggregate count)

### Phase A.8 still pending

A.8 UX freeze (browser eyeball + responsive pass) was not done this session. Independent of Phase B. Pick up whenever — see Phase A todo above.

## Things NOT to redo

- Don't put dashboardApi back in shared.
- Don't reintroduce `take` / `monthsBack` params on hooks.
- Don't rename `Overview` back to `Header`.
- Don't inline ChartTooltip content callbacks in chart components — use `<ChartTooltip>`.
- Don't duplicate `formatCurrency`.
- Don't add `mock` to the api filename or export — it's just `dashboardApi`.
- Don't sort applications on the FE — BE orders by date on the endpoint.
- Don't put `ApplicationListItem` back in shared — each SPA owns its own `Application` type with role-specific `actions` + counterparty (artist | venue).
- Don't reintroduce `contractLabel: string` — `OpportunitySummary.contract: Contract` is the canonical shape; format with `contractSummary()`.
- Don't put `href` back on `Application` — the row IS the view; act via `actions`.
- Don't duplicate `ActionLink` — single source at `shared/types/common.ts`.
- **Don't expose `Expression<Func<T, bool>>` on the spec interface.** Two methods (`Apply` + `ApplyExpression<TParent>`), both IQueryable-shaped. Expression stays inside the impl. The `Substitute` extension on the navigation does the lift.
- **Don't add per-aggregate dashboard count methods to `IConcertRepository` / `IOpportunityRepository` / `IApplicationRepository`.** Dashboard counts live in `IConcertDashboardRepository` as one composed projection — one SQL round trip. Tried per-aggregate, reverted.
- **Don't make `ConcertModule` inline EF queries for dashboard reads.** `feedback_no_ef_in_facade` — facade impls delegate to repos. `IConcertDashboardRepository` is the right home.
- **Don't put cross-module orchestration in the controller.** Controllers are thin delegates to `IXDashboardService`. The service owns `Task.WhenAll` of facade calls and assembles the wire DTO. Payment / future facades slot into the service without changing the controller.
- **Don't change `Apply` on the spec to return `Expression<Func<T, bool>>` or expose a `Predicate` property.** Both shapes (`Apply` direct + `ApplyExpression<TParent>` via nav) are IQueryable-in/out by design — keeps the abstraction honest about what consumers receive.
- **Don't add `IHasDateRangeExpression` or static-Expression members on entities for nav-lift convenience.** That's an anti-pattern — pretends `ApplicationEntity` is a range entity (it isn't), and pulls `System.Linq.Expressions` into Domain. The asymmetry is handled at the spec call site via `ApplyExpression(query, nav)`.
- **Don't return `NotFound` from the dashboard KPI endpoint** when the read-model row is missing. The user owns the venue/artist by authorization (`GetIdForCurrentTenantAsync` would have thrown 403 otherwise) — the projection just hasn't populated yet. Use `NoContent` (204).
- **Don't rename `VenueDashboardCountsDto` to drop the `Dto` suffix.** Keep `Dto` on cross-module DTOs in `Concert.Contracts` (the user explicitly preferred this over the CLAUDE.md "drop suffix" guidance — Concert.Contracts has multiple `XxxDto` records and dropping for one creates inconsistency).
