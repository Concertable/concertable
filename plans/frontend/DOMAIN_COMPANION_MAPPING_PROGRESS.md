# Frontend domain companion mapping progress

- Plan: `plans/frontend/DOMAIN_COMPANION_MAPPING_PLAN.md`
- Roadmap: `plans/frontend/FRONTEND_DOMAIN_MODEL_ROADMAP.md`
- Roadmap item: `frontend/domain-companion-mapping`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-frontend-domain-apis`
- Branch: `Refactor/frontend_domain-companion-mapping`
- PR: none
- Dependency/package gates: none; PRs #595, #600, and #637 are merged; the branch-time platform-sync check found no completed red check
- Last reconciled: 2026-08-22 against `origin/main` at `09c535eb8101cccf93b8652f167245732daed244`, current frontend source, open PRs, and registered worktrees

## Current state

Phases 0 and 1 are complete. The dependency gate cleared, the canonical branch owns the
implementation, and the inventory was refreshed after the frontend guidance and package-topology
changes landed.

The direct label tables, `paymentSummary`, cohesive `consent` service, canonical
`Opportunity.toRequest`, Organization buffer/request companions, raw-buffer validation, and tenant
barrel contraction are implemented and verified. Consent-owned absence is `undefined` and its stored
value is `StoredConsent`; only browser Storage signatures and JavaScript object validation retain
`null`.

The plan now also owns the audited store boundaries and codebase-wide absence normalization. The
editor-store work stays in the existing Concert/Artist/Venue phases because those phases already
change the same buffers. Auth/search encapsulation and the full `null` audit are Phase 4.

## Next Steps

Begin Phase 2 in this worktree:

1. Split route identity from the shared review request, add the web-customer `ReviewBuffer` companion,
   and map only its parsed data to `CreateReviewRequest`.
2. Move report-message normalization into its Zod schema and remove the pre-parse object mapper.
3. Replace Preference's read-as-write mutation input with a slim parsed request, preserving selected
   genres for both create and update.
4. Introduce the shared Concert buffer/request contracts and `Concert.toBuffer`, then make
   `useMyConcert` the complete editor facade and remove public store consumption.
5. Run the Phase 2 focused tests and package builds, update this ledger with exact evidence, and commit
   the coherent phase checkpoint. Do not start Phase 3 in the same turn.

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
- Added `Organization.toBuffer` and `OrganizationBuffer.toUpdateRequest`, moved the request contract
  into `types.ts`, and made the form parse its raw buffer before request construction.
- Removed unused tenant membership derivations from the public barrel and added focused Opportunity,
  Organization, schema, and consent coverage.

## Verification

- Baseline source: `origin/main` at `09c535eb8101cccf93b8652f167245732daed244`.
- `gh pr view` confirms #595 merged at 2026-08-20T21:21:17Z, #600 at
  2026-08-21T10:10:55Z, and #637 at 2026-08-19T19:29:52Z.
- Branch-time platform-sync inspection found no completed red check on the open sync PR.
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

## Reviews

Full docs review covered `9205e82df..959dbb516`. Five accuracy, contradiction, and followability
findings were fixed; incremental review found no issues in the ledger checkpoint through
`385441409`. No implementation review exists.

## Decisions, discoveries, blockers, and deviations

- `Opportunity.toRequest`, not `OpportunityRequest.from`, is the canonical spelling.
- Companions stay in feature `types.ts` for this migration. No threshold or speculative folder split
  is left for the implementer to decide.
- Zod remains the only added behaviour at form boundaries and is already installed; no new dependency
  is planned.
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
  extracted ignored dependencies. Verification restored exact locked package contents from a
  complete sibling worktree or exact npm tarballs; no manifest or lockfile changed.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-frontend-domain-apis
Read @plans/frontend/DOMAIN_COMPANION_MAPPING_PLAN.md and @plans/frontend/DOMAIN_COMPANION_MAPPING_PROGRESS.md and do what its `## Next Steps` says.
```
