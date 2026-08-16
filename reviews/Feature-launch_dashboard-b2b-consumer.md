# Code review — Feature/launch_dashboard-b2b-consumer

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `7529c57616b4632b6ce2fc4a78fc0cbc8872508e`  _(2026-08-16)_
**Security-reviewed up to commit:** `7529c57616b4632b6ce2fc4a78fc0cbc8872508e`  _(2026-08-16)_

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
