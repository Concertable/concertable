# Frontend domain companion mapping progress

- Plan: `plans/frontend/DOMAIN_COMPANION_MAPPING_PLAN.md`
- Roadmap: `plans/frontend/FRONTEND_DOMAIN_MODEL_ROADMAP.md`
- Roadmap item: `frontend/domain-companion-mapping`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-frontend-domain-apis`
- Branch: `Refactor/frontend_domain-companion-mapping`
- PR: none
- Dependency/package gates: none; PRs #595, #600, and #637 are merged; the branch-time platform-sync check found no completed red check
- Last reconciled: 2026-08-22 against `origin/main` at `1452b5b8b0ccd01523d2493283b8497070a60c02`, current frontend source, open PRs, and registered worktrees

## Current state

Phases 0 through 2 are complete. The dependency gate cleared, the canonical branch owns the
implementation, and the inventory and plan now reflect React Hook Form as the interactive form-state
boundary.

The direct label tables, `paymentSummary`, cohesive `consent` service, canonical
`Opportunity.toRequest`, RHF/Zod form boundaries, slim request contracts, and tenant barrel
contraction are implemented and verified. Consent-owned absence is `undefined` and its stored value
is `StoredConsent`; only browser Storage signatures and JavaScript object validation retain `null`.

The Concert editor store is deleted. Artist and Venue store removal remains in Phase 3 alongside
their multipart request cut-over. Auth/search encapsulation and the full `null` audit remain Phase 4.

## Next Steps

Begin Phase 3 in this worktree:

1. Reconcile the current Artist and Venue APIs, hooks, forms, stores, schemas, and multipart tests
   against the Phase 3 inventory before editing.
2. Move slim create/update request interfaces into each feature's `types.ts`; add
   `Artist.toUpdateRequest` and `Venue.toUpdateRequest` for read-model initialization.
3. Move Artist and Venue interactive state to RHF plus `zodResolver`, add create workflow hooks where
   needed, and delete both editor stores and every public/component store import.
4. Keep PascalCase multipart conversion module-private in the API files and test request projection
   separately from exact multipart encoding.
5. Run focused tests, universal package builds, and both mobile gates; update this ledger and commit
   the Phase 3 checkpoint. Do not start Phase 4 in the same turn.

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

## Verification

- Reconciled source: `origin/main` at `1452b5b8b0ccd01523d2493283b8497070a60c02`.
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

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable\.worktrees\Refactor-frontend-domain-apis
Read @plans/frontend/DOMAIN_COMPANION_MAPPING_PLAN.md and @plans/frontend/DOMAIN_COMPANION_MAPPING_PROGRESS.md and do what its `## Next Steps` says.
```
