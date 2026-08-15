# Code review — Feature/launch_dashboard-b2b-consumer

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `ec95772683e053975792f99229a1315e4e9c993d`  _(2026-08-15)_
**Security-reviewed up to commit:** `ec95772683e053975792f99229a1315e4e9c993d`  _(2026-08-15)_

> Range reviewed: `1f4ea1f..ec957726` (12 commits).
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
- [~] **ARCH1 — Message previews are exposed from persona dashboard APIs instead of Conversations.** Producer PR
  #591 publishes `messageApi` from `@concertable/b2b/features/conversations`; after publication the consumer must
  import that API and delete both dashboard-owned message methods.
