# Opportunity sync infrastructure (PR — `Feature/UiE2eFlatFeeWorkflow-2`)

The original goal was to land FlatFee UI E2E. While drafting it, we discovered the SPA had no UI to *post* an opportunity, only to display existing ones. This PR builds that missing UI plus all the BE+FE machinery needed to support it. The actual E2E test for FlatFee happy path lands in a follow-up PR — this one's the prerequisite scaffolding.

## What's done

### Backend — collection sync infrastructure

- **`Concertable.Shared.Application/Diffing/`**
  - `ICollectionSyncer<TEntity, TDto, TContext>` interface
  - `CollectionSyncer<TEntity, TDto, TContext>` abstract base — async hook methods (`CreateAsync`, `UpdateAsync`, `DeleteAsync`), generic diff loop. Reusable for any aggregate that needs declarative full-set sync.
- **`OpportunityRequest`** gained optional `int? Id` — sync uses it to distinguish create vs update; ignored by single-create flow.
- **`IOpportunitySyncer`** marker interface (Concert.Application) + **`OpportunitySyncer`** impl (Concert.Infrastructure). Cross-module Contract create/update via `IContractModule`. Hard-deletes via base default (no soft-delete column on `OpportunityEntity`).
- **`IOpportunityRepository.GetActiveByVenueIdAsync(int venueId)`** — non-paginated overload returning `IEnumerable<OpportunityEntity>` (the existing paginated overload kept).
- **`IOpportunityService.UpdateAsync(int venueId, IEnumerable<OpportunityRequest>)`** — replaces the old per-id Update. Stripe validation + venue-ownership check, sync-applies inside `IUnitOfWorkBehavior`. New `IEnumerable<OpportunityDto> GetActiveByVenueIdAsync(int)` overload (non-paginated) added for FE hydration.
- **`OpportunityController`**:
  - `PUT /api/Venue/{venueId}/opportunities` — declarative full-set sync (replaces per-id PUT)
  - `GET /api/Venue/{venueId}/opportunities` — non-paginated full list (mirror)
  - The paginated `GET /api/Opportunity/active/venue/{id}` kept for now (no FE consumer; left intentionally for potential mobile/future use).
- **DI:** `IOpportunitySyncer` registered as scoped, repo + contract module injected.

### Frontend — opportunity editing UX

- **Type split (`features/concerts/types.ts`):**
  - `OpportunityDraft` — editable subset (startDate, endDate, genres, contract). What new drafts are.
  - `Opportunity extends OpportunityDraft` — adds `id`, `venueId`, `actions`. What server returns. (Inheritance reflects the actual subset relationship; no separate `OpportunityRequest` artefact.)
- **`opportunityApi`:**
  - `getPaged(venueId, params)` (paginated, kept for future use)
  - `getAll(venueId)` (non-paginated; used by venue page now)
  - `update(venueId, items)` — sends `(Opportunity | OpportunityDraft)[]`, inline-maps wire shape (`genreIds = genres.map(g => g.id)`, `id` only when present)
- **`useOpportunitiesQuery` / `useAllOpportunitiesQuery`** — React Query hooks (paginated + non-paginated), live in `hooks/useOpportunitiesQuery.ts`. `opportunitiesQueryKey(venueId)` exported for cache lookups.
- **`useOpportunitiesStore`** (NEW Zustand store, `features/concerts/store/`) — owns its own draft buffer separate from `useVenueStore`. Holds `opportunities: (Opportunity | OpportunityDraft)[]`, `isDirty`, `hydrate`/`reset`/`add`/`update`/`remove` actions.
- **`useOpportunities(venueId)` hook** (NEW, mirrors `useMyVenue` shape) — wraps the query + store + save into one hook. Returns `{ opportunities, isLoading, isError, isDirty, hydrate, reset, add, update, remove, save }`. No useEffect/useRef anywhere; the store IS the source of truth at edit time, hydration happens synchronously at `toggleEdit` (called from `useMyVenue`).
- **`useMyVenue.toggleEdit`** — reads RQ cache, calls both `storeToggleEdit(venue)` AND `hydrateOpportunities(cached)` synchronously. No effects.
- **`useMyVenue.save`** — chains `venueApi.updateVenue` → `saveOpportunities()`. Both stores reset on success.
- **`useMyVenue.resetDraft`** — cancels both venue draft + opportunities buffer.
- **`useMyVenue.isDirty`** — `venueIsDirty || opportunitiesIsDirty`.

### UI components

- **`OpportunityCard.tsx`** — single component, `<Editable view={ReadView} edit={EditView} />`. Customer/public side doesn't wrap in `EditableProvider`, so always renders read-only there. Edit mode renders form fields + delete button.
- **`OpportunityList.tsx`** — edit-mode list. Reads `useOpportunities(venueId)`, renders `OpportunityCard`s with `onUpdate`/`onRemove` callbacks, `+ Add opportunity` button. Has its own loading skeleton via `OpportunityListSkeleton`.
- **`OpportunitySection.tsx`** — top-level switcher. Uses `<Editable view={read-only paginated list} edit={<OpportunityList />} />`. Both branches read from the same React Query cache, so toggling Edit doesn't re-fetch (no skeleton flash).

### Form primitives

- **`features/contracts/components/ContractFields.tsx`** — switch-based dispatch on `contract.$type` to per-variant `FlatFeeFields` / `DoorSplitFields` / `VersusFields` / `VenueHireFields`. Switch over registry: keeps TypeScript narrowing intact for both `contract` (input) AND `onChange` (output) — registry would erase the variance via cast. Read-only `ContractDetails` keeps the registry pattern (one-way data flow, registry cast benign).
- **`features/contracts/defaults.ts`** — `defaultContract(type, paymentMethod)` factory + `CONTRACT_TYPE_LABELS`.
- **`components/datetime/`** (NEW):
  - `useSingleDayDateRange` hook — cross-midnight inference (endTime ≤ startTime → next day)
  - `useMultiDayDateRange` hook — explicit start/end, no inference
  - `SingleDayPicker.tsx` / `MultiDayPicker.tsx` — pure JSX, each consumes its hook
  - `DateRangeField.tsx` — toggle (Checkbox: "Single day"). Tick is authoritative once user touches it. Initial state defaults to single-day if loaded dates span ≤ 24h, multi-day otherwise.
- **`components/ui/NumberInput.tsx`** — wraps shadcn `Input` with `type="number"` and `onWheel={(e) => e.target.blur()}`. Fixes the classic browser quirk where mouse-wheel scrolling decrements `<input type="number" step="0.01">` by 0.01 per tick. Used by all 5 contract number fields.

### Misc

- **`venueApi.updateVenue`** — fixed shape mismatch. BE `UpdateVenueRequest` expects flat form fields (`Name`, `About`, `Latitude`, `Longitude`, `Approved`, `Banner.Url`/`Banner.File`, `Avatar`); FE was sending JSON-stringified `venue` blob. Pre-existing bug, surfaced by this PR's testing.

## What's NOT in this PR (deferred)

- **The actual FlatFee UI E2E test.** The whole reason this scaffolding exists. Lands next PR.
- **Soft-delete on `OpportunityEntity`.** Currently hard-delete. If a sync removes an opportunity that has applications referencing it, FK cascade will fire (or fail — hasn't been stress-tested). Deferred until "VM cancels opportunity with pending apps" UX is designed.
- **Authorization tightening on the BE sync.** Items in the payload with an `Id` that doesn't belong to the venue silently get treated as "create new." Should reject with 403. Tracked but deferred.
- **Pruning the paginated FE path.** `useOpportunitiesQuery` + `opportunityApi.getPaged` + `GET /api/Opportunity/active/venue/{id}` are now unreferenced from the venue page (own + public both use non-paginated). Kept intentionally per user instruction in case mobile/external integrations want them later.

## Outstanding decisions for the next PR

- **Opportunity-sync-on-cancel semantics.** Current behaviour: `resetDraft` clears the Zustand `useOpportunitiesStore` buffer. The React Query cache (`useAllOpportunitiesQuery`) keeps whatever was last fetched. If user cancels after editing, edits are wiped from the store but the cache is untouched. Fine because the next edit-mode entry re-hydrates from cache. If the cache is stale, refetch is triggered by React Query's normal staleTime/refetch logic. Acceptable but worth confirming when E2E exercises it.
- **Where to put hydration if `useMyVenue` shrinks.** Right now `useMyVenue.toggleEdit` does the cache→store hydration. Clean enough. If/when there's a non-`useMyVenue` consumer of `useOpportunities`, that consumer would need its own hydration trigger. Defer.

## Test status

- BE: builds clean, 0 errors. No new BE tests for the sync — coverage gap, should add unit tests for `OpportunitySyncer` + integration tests for `PUT /api/Venue/{id}/opportunities`.
- FE: `tsc --noEmit` passes. Vite production build passes. No new FE tests.
- E2E: existing Login.feature still green. FlatFeeWorkflow.feature exists as an empty Gherkin shell awaiting the next PR's bindings.

## Branch / PR

Working branch: `Feature/UiE2eFlatFeeWorkflow-2`. Target: `master`. Follow-on branch will be `Feature/UiE2eFlatFeeWorkflow-3` (or similar) for the actual E2E test.
