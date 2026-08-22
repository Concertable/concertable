# Code review — Feature/launch_dashboard-b2b-consumer

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `b4eb94e5005a264bb575dcb91a7d8a2a2261f32d`  _(2026-08-22)_
**Security-reviewed up to commit:** `b4eb94e5005a264bb575dcb91a7d8a2a2261f32d`  _(2026-08-22)_

> Range reviewed: `1f4ea1f..ec957726` (12 commits).
> Incremental range reviewed: `ec957726..9be56b9d` (1 commit).
> Incremental range reviewed: `9be56b9d..90a386b1` (33 commits).
> Incremental range reviewed: `90a386b1..510bd491` (2 commits).
> Full range reviewed: `836a15a5..41d0189f3` (current branch net diff against its implementation base).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **COR1 — Current application lists excluded in-progress opportunities.** Both new repository queries used
  `Opportunity.Period.Start > now`, despite the plan's locked still-relevant rule being `Period.End > now`. They now
  retain applications until the opportunity ends, with a real HTTP/SQL integration test covering both manager roles.
- [x] **FE1 — Application widgets used `null` as an absence sentinel and owned mutation orchestration.** Optional UI
  state now uses `undefined`; fake empty dialog strings are gone; navigation, action execution, cache invalidation,
  confirmation state, and toasts live in role-specific hooks so the widgets only render.
- [x] **SER1 — Optional venue avatars serialized as explicit JSON null.** `RecommendedOpportunity.VenueAvatarUrl`
  now follows the optional frontend contract and omits absent values, with serializer coverage.
- [x] **CI1 — The inbox preview integration test expected the older of two seeded messages.** The assertion now checks
  the latest counterparty message, matching the repository's contract and the seed timestamps.
- [x] **CV1 — New infrastructure services captured dependencies through primary constructors.** The two stateful
  services now use the repository's explicit private-field constructor convention.
- [x] **ARCH1 — Message previews are exposed from persona dashboard APIs instead of Conversations.** Both personas
  now consume the published `messageApi` from `@concertable/b2b/features/conversations`; the dashboard-owned message
  methods are deleted.

- [x] **CI2 — Concert integration tests asserted obsolete HTTP response contracts.** Opportunity endpoint tests now
  deserialize the public `OpportunityResponse` rather than its internal application DTO, and the contract HATEOAS
  assertion targets the canonical PDF action route.

## Incremental review — 2026-08-16

No issues found. Checked correctness, security, microservice isolation, module boundaries, seeding, C# conventions,
and test coverage of changed paths.

The incremental review of `90a386b1..510bd491` found no new issues. The range contains the prior plan/review transport
checkpoint and the focused integration-test correction; it introduces no security-sensitive production change.

## Full review â€” 2026-08-16

- [x] **ARCH2 â€” Review repositories returned API-shaped DTOs containing JSON policy and SPA links.** Venue and artist
  review repositories now return persisted review/rating read models. Services map public review contracts, while API
  mappers alone create `RecentReviewResponse` and its HATEOAS link. Permanently empty avatar members were removed.
- [x] **ARCH3 â€” `TenantActivityService` owned EF queries directly.** A normal tenant activity repository now owns
  persistence and returns entities; the service maps the activity contract and the event handler retains the single
  inbox/activity commit.
- [x] **CV2 â€” Review projection handlers bypassed the tenant-scoped repository for owner lookup.** Both handlers now
  use the existing `GetTenantIdByIdAsync` repository capability and retain their DbContext only for projection/inbox/
  outbox work.
- [x] **CV3 â€” Dashboard services mixed orchestration, conversion helpers, inline month construction, and `.Result`.**
  Conversion moved to role-local mappers, month boundaries to `StartOfMonth()`, and completed task values are awaited.
- [x] **COR2 â€” Manager upcoming-concert queries excluded concerts already in progress.** Both role queries now use
  `Period.End > now`, with an HTTP/SQL integration regression covering venue and artist managers.
- [x] **API1 â€” Dashboard clients controlled fixed presentation list sizes.** Activity and recent-review limits are now
  server-owned; the SPAs no longer send `take` query parameters.
- [x] **SER2 â€” New contracts carried permanently absent placeholder members.** Unimplemented KPI deltas, message and
  review avatars, and recommended-venue avatars were removed end to end instead of being represented by null/optional
  fields that no producer populated and no UI rendered.

No open findings remain. The full pass covered correctness, security and tenant scoping, module/repository boundaries,
serialization/HATEOAS ownership, frontend mutation/query contracts, seeding implications, and focused test coverage.

## Current-main review - 2026-08-16

- [x] **SER3 - The common application endpoint declared only the base response contract.** The role-specific
  `Actions` member could be omitted when MVC serialized `ActionResult<ApplicationResponse>`. The concrete response
  base now registers both closed role variants for JSON polymorphism while remaining deserializable as the common
  wire contract and retaining the clean typed-Result terminal.
- [x] **COR3 - Recent message previews ignored hidden-message moderation.** Preview selection and unread calculation
  now reuse the inbox's `NotHidden` predicate. A focused repository test covers a hidden latest message and verifies
  that the preceding visible message is returned with the correct read state.
- [x] **CI3 - Current-main reconciliation duplicated the Conversations test project's `Reunion` reference.** The
  duplicate was removed; all 32 Conversations unit tests and the B2B host build pass with zero warnings or errors.
- [x] **FE2 - Persona-only opportunity contracts were exported from the published two-manager dashboard package.**
  Standalone SPA carving correctly failed because its published package did not contain the branch-local exports.
  `OpportunityMatch` now belongs to the artist SPA and `OpportunityApplicationMetrics` to the venue SPA; only the
  genuinely shared `OpportunitySummary` remains in `@concertable/b2b/features/dashboard`.
- [x] **CI4 - `TenantActivityRecordedEvent` was written to the outbox without message-registry or ASB topology
  registration.** The B2B host now registers the event for publication and both AppHosts provision its topic. The
  focused topology regression passes.
- [x] **ARCH4 - Venue and Artist introduced module-local generic `ReadRepository<TEntity>` implementations.** Both
  review repositories now inherit the canonical shared `ReadRepository<TEntity, int>` directly; the duplicate
  generic wrappers are deleted and the repository architecture guard passes.

No open findings remain. The review covered the current net branch diff `35b114d4a..7529c5761`, including correctness,
security and tenant scoping, Result terminals, serialization/HATEOAS, module/repository boundaries, frontend contracts,
seeding implications, and focused test coverage.

## Final current-main review - 2026-08-21

No issues found. The incremental review covered `7529c5761..bb0e6f3f4`, with focused inspection of every
conflict resolution: controller routes and concrete response mapping, module and host registrations, dashboard service
composition, the retired fixture deletion, opportunity navigation materialization, and the review-route provider's
customer-compatible default with explicit Venue and Artist overrides. The security pass rechecked tenant scoping,
manager permissions, and authenticated controller boundaries. Local validation passed the plan graph, all five
frontend package tiers, 31 shared-web tests, and all four SPA production builds. Environment-dependent integration
and end-to-end coverage remains assigned to exact-head CI.

## Incremental review — 2026-08-21

- [x] **NAT1 — MEDIUM — native/correctness** — `app/web/shared/src/components/Navbar.tsx:115`
  Responsive navigation hid the desktop autocomplete below `lg` without retaining a search entry point. Tablet and
  mobile now receive an accessible compact link to the universal `/find` route, while desktop keeps autocomplete.

No other findings survived the confidence filter. The incremental review covered
`bb0e6f3f4..c531e4f1a` (46 commits) through the native and security layers plus correctness, service isolation,
module boundaries, seeding, routed language/framework conventions, and changed-behaviour test coverage. Security
review found no auth, CORS, development-certificate/private-key, tenant-scope, action-exposure, secret, or supply-chain
issue. The shared package and all four consuming web builds passed after the fix.

## Incremental review — 2026-08-21 (current-main reconciliation)

- [x] **NAT2 — HIGH — native/correctness** — `api/Concertable.Auth/src/Concertable.Auth.Hosting/AppHostExtensions.cs:29`
  Current main launched the Admin SPA on port 5178 without registering its local Auth redirect, logout redirect, or
  allowed origin. Admin now receives the same host-owned local SPA client configuration as the other authenticated
  surfaces.
- [x] **NAT3 — HIGH — native/correctness** — `api/Concertable.B2B/src/Concertable.B2B.Hosting/AppHostExtensions.cs:37`
  Current main launched Admin against B2B without allowing its origin. B2B now includes the exact Admin origin in its
  local CORS configuration.
- [x] **CI5 — HIGH — build/configuration** — `app/web/admin/vite.config.ts:6`
  Admin still depended on the removed per-repository `plugin-basic-ssl` setup and failed its production build after
  reconciliation. It now uses the shared ASP.NET development-certificate helper and explicit IPv4 binding, and the
  obsolete dependency is removed from its manifest and lockfile.
- [x] **NAT4 — MEDIUM — native/test coverage** — `api/Concertable.B2B/tests/Concertable.B2B.CompositionTests/B2BCompositionTests.cs:96`
  The host had no regression pinning the SPA resource ports to their Auth and backend origins. The new composition
  test enumerates the complete B2B web roster and verifies every port, authenticated redirect/origin, and B2B CORS
  origin from the real AppHost model.

No open findings remain. The incremental review covered `c531e4f1a..27e51f65c` (58 commits) through the native and
security layers plus all mechanically routed standards. The final current-main tail was platform-sync PR #709 only;
all five service pins moved consistently to `0.1.0-alpha.0.1120`. Validation passed the B2B AppHost build, all six B2B
composition tests, the shared-web tests, and Customer, Venue, Artist, Business, and Admin production builds. Local
automated E2E remained intentionally unrun; the merge queue owns that tier.

## Incremental review — 2026-08-21 (frontend carve correction)

No issues found. The incremental review covered `27e51f65c..c2a69d062` (5 commits) through the mandatory native
and security layers plus correctness, service isolation, module boundaries, seeding, all mechanically routed
standards, and changed-behaviour test coverage. The carve keeps feed-only isolation while preserving each surface's
real `app/...` hierarchy, archives the shared Vite HTTPS helper from the same Git tree, and now includes Admin in the
CI matrix. The 7 frontend-tooling tests and every dependency boundary passed locally; the package-scoped CI token
remains responsible for the authoritative standalone restores and builds.

## Incremental review — 2026-08-21 (latest current-main reconciliation)

- [x] **NAT5 — MEDIUM — native/correctness** — `.github/workflows/platform-sync.yml:153`
  `mergeStateStatus == CLEAN` proves that a superseded sync PR is mergeable, but not that auto-merge is armed. A
  transient failure across all auto-merge attempts could therefore leave a clean, idle PR that every later sync
  preserved indefinitely. Protect only a clean PR with a non-null `autoMergeRequest`, close clean-but-idle PRs, and
  pin both states with a repository workflow-policy test. The classifier now owns that decision, its four-state table
  runs in the required CI aggregate, and the operational workflow installs the same Node runtime used by the test.

No other findings survived the confidence filter. The incremental review covered `c2a69d062..652fd3aac` (17 commits)
through the mandatory native and security layers plus correctness, service isolation, module boundaries, seeding, all
mechanically routed standards, and changed-behaviour test coverage. The NAT5 correction received a second native and
security pass with no findings. Local validation passed 46/46 Conversations unit tests, the B2B AppHost build with 0
errors, 4/4 platform-sync policy cases, workflow YAML parsing, and the plan graph at 0 errors and 0 warnings.

## Incremental review — 2026-08-21 (CI compiler correction)

No issues found. The incremental review covered `652fd3aac..4c28ab7f7` (3 commits) through the mandatory native layer,
all mechanically routed standards, and the six repository lenses. The range contains review/push checkpoints plus the
two test arrangements corrected from the removed `FlatFeeDeal` type to the current `FlatFeeDealDto` contract. The
Concert unit suite passed 233/233. No security-sensitive path changed, so the existing security watermark remains the
applicable security review boundary.

## Incremental review — 2026-08-21 (final current-main reconciliation)

- [x] **COR4 — LOW — correctness/plan state** — `plans/launch/MANAGER_FRONT_PAGE_PROGRESS.md:15`
  The reconciliation checkpoint still claimed the local head equalled the last pushed remote and PR heads after local
  merge/checkpoint commits had been created. It now records `3ebc4722f` as the last pushed state and distinguishes the
  subsequent local reconciliation tail, preventing a resumed delivery from treating unpushed work as remote.

No other findings survived the confidence filter. The incremental review covered `4c28ab7f7..c6cc262b8` (13 commits)
through the mandatory native and security layers, every mechanically routed current-main standard, and correctness,
service isolation, module boundaries, seeding, language/framework conventions, and changed-behaviour test coverage.
The incoming six base commits only relocate architecture guidance and advance all five package pins consistently to
`0.1.0-alpha.0.1124`. The correction tail received a second native and security pass with no findings. Validation
passed the B2B AppHost build with 0 errors, Conversations 46/46, Concert 233/233, and the plan graph at 0 errors and 0
warnings. No local E2E was run; the merge queue owns that tier.

## Incremental review — 2026-08-22 (second current-main reconciliation)

No findings survived the confidence filter. The incremental review covered
`c6cc262b8dddcec7108d987d04d4940c891d38e4..98b526b57d1c5d9ea6f609e290f982094bff2d8b`
(22 commits) through the mandatory native and security layers, every mechanically routed current-main standard, and
correctness, service isolation, module boundaries, seeding, language/framework conventions, and changed-behaviour
test coverage. The incoming 16 commits split Admin persistence capabilities by entity and repoint stale split-repo
build/workflow metadata; the two Admin repositories deliberately share the same scoped `AdminDbContext`, preserving
atomic invitation/profile saves. No dashboard file conflicted or was overwritten. Validation passed the B2B AppHost
build with 0 errors, Admin 32/32, Conversations 46/46, Concert 233/233, and the plan graph at 0 errors and 0 warnings.
No local E2E was run; the merge queue owns that tier.

## Incremental review — 2026-08-22 (final platform-sync reconciliation)

- [x] **COR5 — LOW — correctness/plan state** — `plans/launch/MANAGER_FRONT_PAGE_PROGRESS.md:10`
  The ledger still named `7f107d98b` as its last reconciliation and described review transport as the only local tail
  after `origin/main` advanced again. It now records `1c63e4f6b` via merge `6b48d0e5f`, the transported `fcaa35f5d`
  checkpoint and green CI run, and the pin-only merge as the reviewed local tail.

No other findings survived the confidence filter. The incremental review covered
`98b526b57d1c5d9ea6f609e290f982094bff2d8b..b4eb94e5005a264bb575dcb91a7d8a2a2261f32d`
(4 commits) through the mandatory native and security layers, the mechanically routed package/plan/review standards,
and all six repository lenses. All five services move consistently to the already-green
`0.1.0-alpha.0.1128` platform pin. The correction tail received a second native and security pass with no findings.
The B2B AppHost built with 0 errors and the plan graph passed with 0 errors and 0 warnings. No local E2E was run; the
merge queue owns that tier.
