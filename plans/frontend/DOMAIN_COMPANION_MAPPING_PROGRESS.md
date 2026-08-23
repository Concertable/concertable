# Frontend domain companion mapping progress

- Plan: `plans/frontend/DOMAIN_COMPANION_MAPPING_PLAN.md`
- Roadmap: `plans/frontend/FRONTEND_DOMAIN_MODEL_ROADMAP.md`
- Roadmap item: `frontend/domain-companion-mapping`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-frontend-domain-apis`
- Branch: `Refactor/frontend_domain-companion-mapping`
- PR: none
- Dependency/package gates: none; PRs #595, #600, and #637 are merged; no open platform-sync PR exists
- Last reconciled: 2026-08-23 against `origin/main` at `fb561acee4aac4dafeb7d57f87f28cf2af35b9a7`, current frontend source, open PRs, and registered worktrees

## Current state

Phases 0 through 3 are complete. Phase 4 implementation is complete and its source, unit, boundary,
typecheck, and web-production-build gates are green. The dependency gate cleared, the canonical
branch owns the implementation, and the inventory and plan now reflect React Hook Form as the
interactive form-state boundary.

The direct label tables, `paymentSummary`, cohesive `consent` service, canonical
`Opportunity.toRequest`, RHF/Zod form boundaries, slim request contracts, and tenant barrel
contraction are implemented and verified. Consent-owned absence is `undefined` and its stored value
is `StoredConsent`; only browser Storage signatures and JavaScript object validation retain `null`.

The Concert, Artist, and Venue editor stores are deleted. Artist and Venue now expose slim request
contracts and same-name read-to-update companions; their create and edit workflows own RHF/Zod state,
while exact PascalCase multipart encoding remains private to each API module. Web identity now has
one TanStack Query owner; mobile identity and universal search state are private behind their
hook/session facades. The production `null` audit is complete and every retained case is one of the
plan's explicit runtime-boundary categories.

The two Android Expo export checks remain blocked by the worktree's incomplete ignored
`react-native-svg` installation. Both mobile TypeScript checks pass, and Metro reaches that missing
package only after resolving the changed source, so this is a local dependency-install blocker rather
than a tracked-source failure.

The refreshed mainline contains an unconsumed `@concertable/b2b` package with parallel Artist/Venue
multipart contracts and encoder tests. No current surface imports that package; the completed Phase 3
corrected the active `@concertable/shared` APIs and every current web/mobile consumer without taking
ownership of the separate package cut-over.

## Next Steps

Finish the Phase 4 gate before beginning Phase 5:

1. Restore the worktree's ignored npm dependencies from the lockfile with a complete install; do not
   change tracked manifests solely to repair the local installation.
2. Re-run the B2B and Customer Android production Expo exports.
3. If both exports pass, begin Phase 5 in a fresh turn and close the complete inventory. Do not redo
   the completed Phase 4 source migration.

## Completed work

- Researched TypeScript/React mapping approaches and rejected a runtime library for this codebase.
- Selected the interface-plus-same-name-companion pattern with source-owned `toX` operations.
- Classified the current frontend transformations and specified an exact disposition for every
  retained or migrated site.
- Created the roadmap item, implementation plan, and operational ledger in `d09f09f23`.
- Resolved all five docs-review findings in `18fec1752` and `959dbb516`.
- Confirmed dependency PRs #595, #600, and #637 merged, created the implementation worktree from
  current `origin/main`, and renamed its branch to the plan's reserved canonical name.
- Refreshed the transformation, state-boundary, and absence inventories against the landed frontend
  topology. The audit found 219 TypeScript `null` occurrences before classification.
- Completed Phase 1 in this commit: replaced the genre, tenant-role, and message-action label wrappers
  with exhaustive direct lookup tables and renamed `summaryFor` to `paymentSummary`.
- Replaced the consent function family with `consent.has/read/write/subscribe`, renamed the persisted
  shape to `StoredConsent`, and changed owned absence from `null` to `undefined`.
- Added `OpportunityRequest` and the canonical `Opportunity.toRequest` companion and replaced the
  anonymous API projection with `desired.map(Opportunity.toRequest)`.
- Added `Organization.toFormValues`, moved the request contract into `types.ts`, and made RHF/Zod
  transform flat form values directly into the nested request.
- Removed unused tenant membership derivations from the public barrel and added focused Opportunity,
  Organization, schema, and consent coverage.
- Split review route identity from `CreateReviewRequest`; Review and report-message now use RHF with
  Zod normalization and their hooks accept request contracts only.
- Replaced Preference's read-as-write update with one slim `PreferenceRequest`; RHF now preserves the
  selected genres for both create and update.
- Added `Concert.toUpdateRequest`, moved the request interface into `types.ts`, moved editor state into
  RHF-owned `useMyConcert`, and deleted `useConcertStore` and all of its exports/imports.
- Removed the remaining hand-written form-state abstractions by converting Invite and Organization to
  RHF; production `Buffer` references now describe only binary `ArrayBuffer` values.
- Deleted the three resolved RHF technical-debt entries and removed the stale Concert-store citation
  from the remaining auth debt.
- Completed Phase 3 in this commit: introduced slim Artist/Venue create and update request contracts,
  `Artist.toUpdateRequest`, and `Venue.toUpdateRequest` while keeping multipart encoders private.
- Added aligned create/update Zod schemas and RHF-owned shared workflow hooks, including app-local
  create wrappers for navigation and venue opportunity composition through the existing facade seam.
- Removed the Artist and Venue Zustand editor stores, their web re-exports, and every component/store
  import; Hero components now receive image transitions explicitly.
- Added separate companion, schema, and exact PascalCase multipart encoder coverage for Artist and
  Venue create/update paths, including conditional image omission.
- Completed Phase 4: web identity components and guards now share `meQueryOptions`/`useMeQuery`, and
  the duplicate universal/web auth stores plus both `useSyncUser` layers are deleted.
- Moved mobile identity into the mobile package behind `useCurrentUser` and `mobileAuthSession`, then
  migrated initialization, login/logout, SignalR, client configuration, navigation, and screens.
- Encapsulated universal search state behind `useSearchFilters`, composed router synchronization in
  the web facade, removed the web duplicate store, and added focused replace/update coverage.
- Normalized owned optional state, contracts, API results, and error details to `undefined`; deleted
  the resolved web auth technical-debt entry.

## Verification

- Reconciled source: `origin/main` at `1452b5b8b0ccd01523d2493283b8497070a60c02`.
- Reconciled Phase 3 source after merging `origin/main` at
  `2d6f7e3a9d1256b549b3fc91a4acbb483d4c01f1`; the merge was conflict-free.
- `gh pr view` confirms #595 merged at 2026-08-20T21:21:17Z, #600 at
  2026-08-21T10:10:55Z, and #637 at 2026-08-19T19:29:52Z.
- Branch-time platform-sync inspection found no completed red check on the open sync PR.
- Refreshed branch-time platform-sync inspection found no open sync PR.
- Registered worktrees checked. `Refactor/OrganizationProfileRouteContraction` has committed
  Artist/Venue changes without an open PR and is explicitly not the implementation base.
- `python .agents/hooks/plan_graph.py --root <plan-worktree>`: 0 errors, 0 warnings.
- Local Markdown link check: every relative link in `plans/frontend/` resolves.
- `git diff --check`: clean for the Phase 1 checkpoint.
- Reviewed work head `385441409ab5c88c4361413003785375c8a858a5` pushed and verified equal to
  `origin/Docs/frontend_domain-companion-plan`; PR #644 opened from that exact head with `skip-e2e`.
- `@concertable/shared`: 1 test file, 7 tests passed; build passed.
- `@concertable/b2b`: 5 test files, 15 tests passed; build passed.
- `@concertable/web`: 5 test files, 31 tests passed; build passed, including its full source
  typecheck.
- `@concertable/web-b2b`: 11 test files, 25 tests passed; build passed.
- `@concertable/mobile` and `@concertable/customer` package builds passed.
- Customer, Venue, Artist, Business, and Admin production builds passed. Existing Vite config and
  chunk-size warnings remain warnings only.
- Legacy-name search found zero production occurrences of `ConsentRecord`, the four former consent
  functions, the three label helpers, and `summaryFor`.
- Canonical call-site search found `desired.map(Opportunity.toRequest)` in `opportunityApi.ts`.
- Pre-edit audits captured every production import of `useAuthStore`, `useSearchFiltersStore`,
  `useArtistStore`, `useVenueStore`, and `useConcertStore`; only Opportunities and Tenant already
  satisfy the private-store boundary.
- Phase 2 focused tests passed: 24 files and 69 tests across `@concertable/shared`,
  `@concertable/customer`, `@concertable/web`, `@concertable/web-customer`, and
  `@concertable/web-b2b`.
- Phase 2 package builds passed for `@concertable/shared`, `@concertable/customer`,
  `@concertable/web`, `@concertable/web-b2b`, and `@concertable/mobile`.
- Mobile Customer and the Customer, Venue, Artist, and Business web application typechecks passed.
  The web application checks were run serially because concurrent TypeScript processes can race on
  TanStack Router's generated route types.
- Customer, Venue, Artist, and Business production Vite builds passed after the final RHF changes.
  Existing Vite configuration and chunk-size warnings remain warnings only.
- All seven frontend dependency-cruiser boundary scopes and all five boundary-script carve tests
  passed.
- The production custom-buffer search now finds only three genuine binary `ArrayBuffer` uses.
- Phase 2 final `git diff --check`, plan-graph validation, and documentation-reachability validation
  passed with zero errors or warnings.
- Refreshed `python .agents/hooks/plan_graph.py --root <plan-worktree>`: 0 errors, 0 warnings.
- Merged the later `origin/main` advance at
  `fb561acee4aac4dafeb7d57f87f28cf2af35b9a7`; its documentation close-out and backend package rename
  were conflict-free and did not overlap the Phase 3 frontend surfaces.
- Phase 3 `@concertable/shared`: 9 test files and 21 tests passed; the focused Artist/Venue subset was
  6 files and 12 tests. `@concertable/web`: 5 files and 31 tests passed.
- Phase 3 package builds passed for `@concertable/shared`, `@concertable/web`, and
  `@concertable/mobile`; B2B Artist, B2B Venue, B2B mobile, and Customer mobile typechecks passed.
- Customer, Venue, Artist, Business, and Admin production Vite builds passed. Existing Vite
  configuration and chunk-size warnings remain warnings only.
- B2B and Customer Android production Expo exports passed. The worktree's interrupted ignored install
  required restoring exact locked package contents from the complete sibling install before Metro
  could run; no tracked dependency state was changed by that repair.
- All frontend dependency-cruiser scopes passed with no violations, and all seven boundary-script
  carve tests passed.
- Phase 3 production searches found no `useArtistStore` or `useVenueStore` occurrences and no added
  code comments; final multipart API consumers use the workflow contracts or read-only guard methods.
- Phase 4 focused Vitest runs passed: `@concertable/shared` 10 files/23 tests,
  `@concertable/web` 5 files/31 tests, and `@concertable/web-b2b` 12 files/25 tests.
- Phase 4 package/source typechecks passed for shared, web-shared, web-B2B, mobile-shared, both mobile
  apps, and all five web apps.
- Customer, Admin, Business, Artist, and Venue production Vite builds passed. The existing Node
  version and chunk-size notices remained warnings only.
- All seven frontend dependency-cruiser scopes and all seven boundary-script carve tests passed.
- Production searches found no `useAuthStore` or `useSyncUser`, no search-store import outside its
  private facade and focused test, no production search `getState()` call, and no
  `T | null | undefined` type.
- `git diff --check` passed for the Phase 4 source changes.
- B2B and Customer Android Expo exports progressed through the changed application source but could
  not resolve the incomplete ignored `react-native-svg` package in this worktree. No tracked source or
  lockfile dependency version was changed to work around the damaged local install.

## Reviews

Full docs review covered `9205e82df..959dbb516`. Five accuracy, contradiction, and followability
findings were fixed; incremental review found no issues in the ledger checkpoint through
`385441409`. No implementation review exists.

## Decisions, discoveries, blockers, and deviations

- `Opportunity.toRequest`, not `OpportunityRequest.from`, is the canonical spelling.
- Companions stay in feature `types.ts` for this migration. No threshold or speculative folder split
  is left for the implementer to decide.
- React Hook Form owns interactive form state and `zodResolver` owns parsing. Both libraries already
  existed in the monorepo; affected workspaces now declare their direct dependencies explicitly.
- The inventory includes implicit mappings where a read type is reused as a write body, even if no
  function is currently named mapper.
- Backend mappers are excluded.
- Direct typed label-table indexing replaces one-line label wrappers.
- `permissions.has(permission)` remains unchanged; a `can(permission)` alias would be less explicit.
- `consent` is a justified stateful service; `StoredConsent` names its persisted value.
- Owned absence uses `undefined`. Browser/DOM/React/SDK-required `null`, JSX no-render, JavaScript
  object guards, and genuinely distinct explicit-empty states remain.
- Raw `Query`/`Mutation` hooks remain valid public server-state APIs. Facades are required when a
  consumer would otherwise assemble store transitions, validation, derivation, or navigation.
- The worktree's first `npm ci` was interrupted by slow Windows file scanning and left partially
  extracted ignored dependencies. Verification restored exact locked package contents from npm's
  complete staging directories or a complete sibling worktree. Phase 2 intentionally changes
  workspace manifests and lockfile workspace entries for direct RHF/Zod/Vitest dependencies.
- The landed `app/b2b/shared` Artist/Venue API copies are not consumed by any surface and belong to
  the separate cross-platform B2B package cut-over; Phase 3 does not redirect consumers to them.
- The retained production `null` cases are JSX no-render branches; React, DOM, and native ref values
  and callbacks; browser Storage results; Stripe and Expo SDK signatures; JavaScript object guards;
  and defensive `!= null` checks at optional wire/UI boundaries. None represents client-owned
  optional state, and no production type combines `null` with `undefined`.
- Phase 4's only remaining gate is environmental: the interrupted ignored install left
  `react-native-svg` without a complete package payload. Repeated local package extraction was stopped
  rather than risk more filesystem damage; a clean lockfile install is the required recovery.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-frontend-domain-apis
Read @plans/frontend/DOMAIN_COMPANION_MAPPING_PLAN.md and @plans/frontend/DOMAIN_COMPANION_MAPPING_PROGRESS.md and do what its `## Next Steps` says.
```
