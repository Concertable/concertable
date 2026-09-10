# Frontend domain companion mapping progress

- Plan: `plans/frontend/DOMAIN_COMPANION_MAPPING_PLAN.md`
- Roadmap: `plans/frontend/FRONTEND_DOMAIN_MODEL_ROADMAP.md`
- Roadmap item: `frontend/domain-companion-mapping`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-frontend-domain-apis`
- Branch: `Refactor/frontend_domain-companion-mapping`
- PR: #783 — https://github.com/Concertable/concertable/pull/783
- Dependency/package gates: producer PR #786 and verifier repairs #804/#810 merged; publication run 33021346371 successfully published and feed-verified all six frontend tiers.
- Last reconciled: 2026-08-27 against `origin/main` at `9eea10bbc6ad95e9967e1fe91ab8679b7f574c8b`; merge commit `fe331f351` passes the final focused package and boundary matrix.

## Current state

Phases 0 through 4 and the Phase 5 inventory are complete. Neutral `VenueState`, `ArtistState`, and
`ConcertState` Zustand drafts remain behind their workflow facades. Request contracts now derive from
their read models with `Omit` or `Pick` where the fields are association-wise identical; genuinely
different write shapes remain explicit contracts.

Package-facing feature types now use explicit `/types` entry points, with hook- and schema-owned types routed to their exact modules. Runtime imports remain on feature barrels, and the AST boundary gate enforces both directions. The redundant Concert companion mapper is removed and the private store selects its exact RHF draft.
Artist and Venue companions remain because their read image URLs are incompatible with binary
`ImageFile` write fields. The complete frontend matrix passes for this refinement. Both incremental
reviews are clean after the RHF reset and checkpoint wording fixes. The producer export maps and verifier repairs are merged into this branch; publication run 33021346371 successfully published and feed-verified all six frontend tiers.

## Next Steps

Drive PR #783 exact-head CI and the merge queue to green, then confirm the post-merge frontend package publication.

## Completed work

- Researched TypeScript/React mapping approaches and rejected a runtime library for this codebase.
- Selected the interface-plus-same-name-companion pattern with source-owned `toX` operations.
- Classified the current frontend transformations and specified an exact disposition for every
  retained or migrated site.
- Created the roadmap item, implementation plan, and operational ledger in `d09f09f23`.
- Resolved all five docs-review findings in `18fec1752` and `959dbb516`.
- Cleared the dependency gate and completed Phases 1 through 4 in `e9a8fe7c9`, `a5ba13de2`,
  `70bfc8ac3`, and `cce96a5e7`.
- Closed the remaining inventory and review fixes through `539c1a520`.
- Merged `origin/main` at `ac7ff7f17` without conflicts.
- Opened draft PR #783 and pushed the prior reviewed candidate through `7965f2bbb`.
- Restored private neutral Zustand editor state for Artist, Venue, and Concert while retaining the
  RHF/Zod request boundary and the slim multipart/API contracts.
- Derived aligned Preference, Review, Concert, member-role, and admin-invitation write contracts from
  their read models; removed duplicate B2B Artist/Venue declarations and the redundant Concert mapper.
- Routed 80 package-facing type declarations through `/types` or their exact owner and added the bidirectional AST entry-point gate in `fcccf983a`.
- Merged producer export maps and current `origin/main` in `280a7cbde`, preserving the final consumer contracts and intentional store/type deletions while making no-argument shared identity reads cache-only observers.

## Verification

- GitHub reports PRs #595, #600, and #637 merged; platform-sync PR #780 is red and implementation
  draft PR #783 exists. #780's build fails with four `CS0738` errors because Payment's `EscrowEntity` and
  `TransactionEntity` expose `DateTime` audit properties while `IAuditable` now requires
  `DateTimeOffset`.
- Current baseline: `origin/main` at `9eea10bbc6ad95e9967e1fe91ab8679b7f574c8b`, merged cleanly in `fe331f351`.
- Tests passed: `@concertable/b2b` 5 files / 15 tests; `@concertable/shared` 10 / 23;
  `@concertable/web` 5 / 31; `@concertable/customer` 3 / 3 through its build preflight.
- B2B, shared, customer, all five web SPA, and both mobile TypeScript builds passed.
- Dependency-cruiser reported no violations; all 7 carve/boundary tests passed.
- B2B and Customer Android exports passed with 3,691 and 4,283 modules respectively.
- The editor-state correction passed shared 23/23 and web-B2B 25/25 tests; both package builds;
  dependency-cruiser; all 7 boundary tests; all five web builds; both mobile TypeScript checks; and
  B2B/Customer Android exports with 3,695/4,287 modules.
- The request-contract refinement passed shared 22/22, B2B 15/15, customer 3/3, web 31/31, and
  web-B2B 25/25 existing tests; all affected package builds; dependency-cruiser; all 7 boundary tests;
  all five web builds; both mobile TypeScript checks; and B2B/Customer Android exports with
  3,695/4,286 modules.
- Phase 5 mapper/buffer/store/absence searches passed with only the intended binary `ArrayBuffer`
  sites and private store facade/test sites allowlisted.
- The `/types` checkpoint passed all five package builds and 73 existing package tests, four available web app builds, the venue rebuild, B2B mobile typecheck, all 13 dependency-cruiser workspaces, and all 8 carve/boundary tests. Customer web/mobile await #786's export-map merge; their only remaining errors are unresolved new customer `/types` subpaths.
- `git diff --check` and `python .agents/hooks/plan_graph.py --root <worktree>` passed.
- Merge commit `280a7cbde` passes all five web package builds and 65 existing package tests, all five SPA builds, both mobile TypeScript checks, all 13 dependency-cruiser workspaces, and all 8 carve/boundary regressions.
- Publication run 33021346371 successfully packed, verified, published, and feed-verified all six frontend tiers after verifier repairs #804 and #810.
- Final merged head `fe331f351` passes all six package builds, 65 existing package tests, all 13 dependency-cruiser workspaces, and all 8 carve/boundary regressions.

## Reviews

Full implementation review covered `70af43a..4b69e66` in
`reviews/Refactor-frontend_domain-companion-mapping.md`. CV1 and CV2 were fixed in `539c1a520`; NAT1 through
NAT3 corrected delivery-checkpoint wording. The incremental editor-state review is clean with no open
findings. The request-contract review found NAT4 and NAT5; both were fixed in the follow-up commit, and
the incremental fix review is clean.

## Decisions, discoveries, blockers, and deviations

- `Opportunity.toRequest`, not `OpportunityRequest.from`, is the canonical spelling.
- Companions stay in feature `types.ts` for this migration. No threshold or speculative folder split
  is left for the implementer to decide.
- Zod remains the only added behaviour at form boundaries and is already installed; no new dependency
  is planned.
- The inventory includes implicit mappings where a read type is reused as a write body, even if no
  function is currently named mapper.
- Backend mappers are excluded.
- Transport encoders remain API-private; third-party adapters and presentation projections remain
  boundary-local rather than becoming domain companions.
- Commit `93b8e0648` established Zustand as the cross-component editor owner; the later store deletion
  was an unintended plan reversal and has been corrected.
- `VenueState`, `ArtistState`, and `ConcertState` are neutral client state. Create/update request types
  remain submission outputs and are not reused as store drafts.
- RHF/Zod produces the request directly; stores are updated through the same facade callbacks instead
  of CRIS-style store-to-form and form-to-store synchronization effects.
- Use `Omit<Read, ...>` when a request only excludes identity/server-owned fields and `Pick<Read, ...>`
  for a strict writable subset. Keep a separate interface when write semantics or field types differ.
- `TicketPurchaseRequest` remains independent from `TicketCheckout`; the checkout response only
  incidentally echoes purchase fields and is not the request's domain owner.
- No new frontend tests are added until the repository adopts a test standard; existing tests and
  build gates still run for verification.
- Customer and B2B roots own their `getMe` functions; shared navigation/profile consumers observe the common React Query key without supplying a default endpoint or restoring a second identity store.
- Local E2E is not part of this refactor's pre-PR gate.
