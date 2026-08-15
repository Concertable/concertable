# Code review — Feature/launch_dashboard-b2b-consumer

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `510bd491bd67dd84216f6a5dc419aa094241d673`  _(2026-08-16)_
**Security-reviewed up to commit:** `90a386b1416f2179eaabef3e7b8068eef8594775`  _(2026-08-16)_

> Range reviewed: `1f4ea1f..ec957726` (12 commits).
> Incremental range reviewed: `ec957726..9be56b9d` (1 commit).
> Incremental range reviewed: `9be56b9d..90a386b1` (33 commits).
> Incremental range reviewed: `90a386b1..510bd491` (2 commits).
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
