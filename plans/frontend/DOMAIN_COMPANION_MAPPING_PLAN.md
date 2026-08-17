# Frontend domain companion mapping refactor

> **Next steps live in @plans/frontend/DOMAIN_COMPANION_MAPPING_PROGRESS.md → `## Next Steps`.**

## Outcome

Replace scattered, anonymous domain transformations with a small TypeScript convention that reads
like domain behaviour without changing interface-based contracts or introducing runtime model classes.
Reusable pure conversions become source-owned operations on a same-name value companion:

```ts
export interface OpportunityDraft {
  startDate: string;
  endDate: string;
  genres: Genre[];
  deal: Deal;
}

export interface Opportunity extends OpportunityDraft {
  id: number;
  venueId: number;
  actions: OpportunityActions;
}

export interface OpportunityRequest extends OpportunityDraft {
  id?: number;
}

export const Opportunity = {
  toRequest(value: Opportunity | OpportunityDraft): OpportunityRequest {
    return {
      id: "id" in value ? value.id : undefined,
      startDate: value.startDate,
      endDate: value.endDate,
      genres: value.genres,
      deal: value.deal,
    };
  },
};
```

The call site becomes `desired.map(Opportunity.toRequest)`. Interfaces still describe the slim API
contracts; the companion exists only in the value namespace erased interfaces do not occupy.

## Final convention

### Naming and placement

1. Name the companion exactly after its source interface: `export interface Opportunity` plus
   `export const Opportunity`.
2. Name operations `toX`. Use the shortest unambiguous destination name: `toRequest` when the feature
   has one request shape for the concept; `toCreateRequest` and `toUpdateRequest` when both exist;
   `toBuffer` for a named editable buffer.
3. Put the companion immediately below the related interfaces in the owning feature's `types.ts`.
   Keep all companions in `types.ts` for this migration; do not create a `domain/`, `mappers/`,
   `mapping/`, or general `utils/` folder.
4. Export the companion through the existing feature barrel only when consumers already import the
   corresponding type through that barrel. Do not widen a package tier solely to share a mapper.
5. Give every method an explicit parameter and return type. Request return types are exported
   interfaces from the same feature's `types.ts`, not anonymous objects declared in an API module.

### Interface and Zod relationship

Object contracts remain interfaces in `types.ts`; schemas validate them but do not replace them with
schema-derived aliases. When a schema's parsed output is already the request shape, bind the schema to
the interface explicitly:

```ts
export interface PreferenceRequest {
  radiusKm: number;
  genres: Genre[];
}

export const preferenceRequestSchema: z.ZodType<PreferenceRequest> = z.object({
  radiusKm: z.number().min(1),
  genres: z.array(z.enum(GENRE_VALUES)),
});
```

Do not also declare `type PreferenceRequest = z.infer<typeof preferenceRequestSchema>`. When the live
buffer and request differ, bind the schema to the buffer interface, parse it, and pass `parsed.data`
to the buffer companion. This keeps the user-input proof and the HTTP contract distinct without
duplicating either interface.

### Behaviour boundary

A companion method must be pure, synchronous, deterministic, and free of React, hooks, HTTP clients,
global stores, navigation, environment state, and side effects. It may select fields, rename fields,
construct nested domain/request shapes, normalize already-validated values, and handle a closed
discriminator exhaustively.

The source owns the operation. Do not add destination-owned `from`, TypeScript prototype augmentation,
classes used only to gain methods, declaration merging tricks, decorators, reflection, or service-style
`XMapper` objects.

### Classification table

| Transformation | Required owner and spelling |
|---|---|
| Shapes are already identical | Delete the mapper and type the value as the destination. |
| Reusable or semantic domain/read/buffer → request conversion | Source companion in feature `types.ts`, named `Source.toRequest` or the specific `toXRequest`. |
| Raw form buffer → request | Parse the raw buffer with its feature Zod schema first; map only `parsed.data`. If parsed data already is the request shape, pass it directly. |
| Query parameters, route construction, headers, JSON/multipart encoding | Keep in `api/xApi.ts`. The complex encoders named by this plan become module-private `toX` helpers; one-use one-or-two-field bodies stay inline. |
| Raw third-party response → internal value | Keep in the third-party adapter module; do not place raw SDK/wire types on a domain companion. |
| Domain value → chart row, label, JSX props, or other view model | Keep a named pure projection beside the rendering hook/component. |
| Behaviour varies by a closed discriminator | Use one exhaustive typed registry/table in the owning feature; do not add `ts-pattern` for this refactor. |
| Single-use one-or-two-field request body | Keep the object literal inline with an explicit request type where it crosses a function boundary. |

## Dependency and branch gate

Create the implementation worktree only after PRs #595, #600, and #637 are merged or closed. They
currently overlap the target concert/messaging types and the frontend guidance documents. At that
point fetch `origin/main`, confirm no open red platform-sync PR, and create
`Refactor/frontend_domain-companion-mapping` from current `origin/main`.

`Refactor/OrganizationProfileRouteContraction` has committed frontend Artist/Venue changes but no
open PR. It is not an implementation base and does not block this plan. Phase 0 must classify it
again: if its content reached `main` through another branch, use the landed shape; otherwise preserve
it as unrelated work and refactor the `origin/main` implementation only.

The backend `Refactor/OpportunityMapperPagination` work is unrelated. This plan must not edit or
coordinate C# mappers.

## Complete baseline inventory and required disposition

The line descriptions below are semantic anchors; Phase 0 refreshes line numbers after the dependency
gate without changing the recorded decision unless the underlying shape changed.

| Current site | Existing transformation | Required disposition |
|---|---|---|
| `app/web/b2b/shared/src/features/concerts/api/opportunityApi.ts` | Anonymous `desired.map` strips `venueId`/`actions` and conditionally carries `id`. | Introduce `OpportunityRequest` and `Opportunity.toRequest` in concerts `types.ts`; use `desired.map(Opportunity.toRequest)`. This is the canonical companion example. |
| `app/web/b2b/shared/src/features/organizations/components/OrganizationForm.tsx` | `initialBuffer` flattens an `Organization` read into editable fields and defaults absent compliance data. | Move `OrganizationBuffer` to `app/web/b2b/shared/src/features/organizations/types.ts`; add `Organization.toBuffer`; initialize component state with it. |
| `app/web/b2b/shared/src/features/organizations/hooks/useOrganization.ts` and `schemas/updateOrganizationRequestSchema.ts` | The hook constructs a nested request from the raw buffer and only then validates it. | Validate the raw `OrganizationBuffer` first, then call `OrganizationBuffer.toUpdateRequest(parsed.data)`. Put `UpdateOrganizationRequest` in `types.ts`; the API imports it from there. |
| `app/web/customer/src/features/reviews/hooks/useAddReview.ts` and `app/customer/shared/src/features/reviews/api/reviewApi.ts` | The form reshapes its buffer; the shared request incorrectly contains route-only `concertId`, which the API strips. | Create `app/web/customer/src/features/reviews/types.ts` with `ReviewBuffer.toCreateRequest` and call it after buffer parsing. Change the shared API to `createReview(concertId, request)` and remove `concertId` from `CreateReviewRequest` in `app/customer/shared/src/features/reviews/types.ts`. |
| `app/shared/src/features/messaging/hooks/useReportMessage.ts` and `schemas/reportMessageRequestSchema.ts` | An object literal trims/omits details before parsing. | Put normalization in the Zod schema and parse `buffer` directly. The parsed shape already is `ReportMessageRequest`; add no companion. |
| `app/customer/shared/src/features/preferences/api/preferenceApi.ts`, `hooks/usePreferenceQuery.ts`, and mobile `PreferencesScreen.tsx` | Update accepts a full `Preference` read and the screen reconstructs server-owned `id`/`user`; create drops selected genres. | Define one slim `PreferenceRequest` in `app/customer/shared/src/features/preferences/types.ts`; keep route `id` as a function argument; add the request schema under that feature's `schemas/`; validate the two-field editable buffer and pass parsed data directly. Add no companion because the parsed buffer and request are identical. |
| `app/shared/src/features/concerts/hooks/useMyConcert.ts` and `app/shared/src/features/concerts/store/useConcertStore.ts` | A full `Concert` read is copied as editable state and silently stripped to an update request by Zod. | Define `ConcertBuffer`, `UpdateConcertRequest`, and `Concert.toBuffer` in `app/shared/src/features/concerts/types.ts`; store only the buffer; bind the existing schema to the request interface and pass parsed data directly. Do not add a second request mapper. |
| `app/shared/src/features/artists/api/artistApi.ts`, `hooks/useMyArtist.ts`, and `store/useArtistStore.ts` | Create types live in the API module; update accepts a full `Artist` read, then manually encodes selected fields as multipart. | Define slim create/update request interfaces, `ArtistBuffer`, and `Artist.toBuffer` in `app/shared/src/features/artists/types.ts`; parse before mutation. Add module-private `toCreateFormData` and `toUpdateFormData` helpers in `artistApi.ts`; the API accepts only the slim requests. |
| `app/shared/src/features/venues/api/venueApi.ts`, `hooks/useMyVenue.ts`, and `store/useVenueStore.ts` | Update accepts a full `Venue` read, then manually encodes selected fields as multipart. | Define slim create/update request interfaces, `VenueBuffer`, and `Venue.toBuffer` in `app/shared/src/features/venues/types.ts`; parse before mutation. Add module-private `toCreateFormData` and `toUpdateFormData` helpers in `venueApi.ts`; the API accepts only the slim requests. |
| `app/shared/src/features/search/api/headerApi.ts` | `SearchFilters` is renamed and combined into HTTP query parameters. | Define a private `HeaderSearchParams` interface and module-private `toSearchParams(filters)` in `headerApi.ts`, then pass its result as Axios params. Do not put transport parameters on `SearchFilters`. |
| `app/shared/src/features/artists/api/artistApi.ts` and `venues/api/venueApi.ts` | Domain/request fields become PascalCase multipart entries and indexed genre keys. | Keep the exact wire conversion in the four module-private `toCreateFormData`/`toUpdateFormData` helpers named above. These are transport encoders, not domain companions. |
| `app/web/shared/src/features/dashboard/components/MonthlyRevenueChart.tsx` | Revenue points become Recharts rows with formatted months and major currency units. | Define a private `ChartRevenuePoint` interface and `toChartRevenuePoint(point)` beside the component; call `data.map(toChartRevenuePoint)`. It remains a presentation projection. |
| `app/shared/src/lib/googleGeocodingApi.ts` | Google address components and result ranking become an internal location label. | Keep the private adapter functions in this module. Raw Google response types do not enter feature domain companions. |
| `app/web/b2b/shared/src/features/concerts/components/applications/AcceptDealSummary.tsx` | Closed deal variants dispatch through an exhaustive typed record. | Preserve the registry pattern; no mapper library or `ts-pattern` migration. |
| `app/web/b2b/shared/src/features/members/hooks/useInviteMember.ts` | Buffer and request are the same shape after Zod parsing. | Preserve direct `safeParse(buffer)` and `mutate(parsed.data)`; add no companion. |
| `app/shared/src/features/concerts/api/concertApi.ts`, ticket/payment/application API calls, and self-billing API calls | One-use small bodies are posted directly. | Preserve inline request bodies when they are one or two fields and do not hide a read/write shape conversion. |

## Phases

### Phase 0 — refresh ownership and freeze the migration matrix

1. Wait for the three dependency PRs to reach a terminal state, fetch/prune, and create the reserved
   implementation branch from current `origin/main`.
2. Confirm there is no open red `chore/platform-sync-*` PR and no newer plan or PR owns these same
   frontend transformations.
3. Re-run the inventory against current `origin/main`, including request interfaces, API method input
   types, Zod `safeParse` object literals, `FormData` encoders, and non-render collection mappings.
4. Update only the progress ledger when a path or line moved. Change this plan only if the underlying
   contract or ownership decision changed.
5. Record the exact affected npm workspaces and existing focused test commands before editing.

Gate: the ledger contains a current path-by-path matrix, dependency PR outcomes, active-worktree
ownership evidence, and the exact verification commands.

### Phase 1 — establish the convention and canonical B2B conversions

1. Add the finalized companion convention and classification table to
   `app/agents/CODE_PATTERNS.md`, and reconcile `app/agents/CODE_CONVENTIONS.md` so object requests stay
   interfaces in `types.ts` while Zod schemas bind through `z.ZodType<XRequest>`. Integrate with the
   post-#637 structure rather than restoring old prose.
2. Implement `OpportunityRequest` and `Opportunity.toRequest`; replace the API's anonymous map.
3. Refactor Organization initialization and submit flow to `Organization.toBuffer`, raw-buffer Zod
   parsing, and `OrganizationBuffer.toUpdateRequest`.
4. Add focused tests for Opportunity's conditional `id`/field omission and Organization's absent
   compliance defaults, VAT branch, optional address line, trimming, and nested request shape.

Gate: the convention is documented, the canonical call site reads
`desired.map(Opportunity.toRequest)`, Organization maps only validated data, and the B2B package tests
and build are green.

### Phase 2 — correct shared customer and form write boundaries

1. Split route identity from the review request and implement the parsed `ReviewBuffer` conversion.
2. Remove the messaging pre-parse object mapper by moving normalization into its schema.
3. Replace Preference's read-as-write API with the slim shared request and correct both create and
   update mobile submissions, including selected genres.
4. Replace the full-Concert edit copy with `ConcertBuffer`, `Concert.toBuffer`, and direct parsed
   request mutation.
5. Test semantic normalization, route/body separation, create/update parity, buffer initialization,
   and absence of server-owned fields in request bodies.

Gate: customer/shared and universal shared tests/builds are green; route ids are API arguments; no
edited mutation accepts a read model.

### Phase 3 — correct multipart write contracts without moving transport encoding

1. Reconcile the landed Artist/Venue APIs after the route-contraction branch check.
2. Move create/update request contracts out of API modules and into each feature `types.ts`.
3. Introduce writable buffers plus `Artist.toBuffer` and `Venue.toBuffer`; add or align Zod schemas so
   mutations receive parsed slim requests.
4. Keep PascalCase multipart keys, optional image handling, and indexed genre fields inside the API
   modules.
5. Test the pure buffer conversions and API-level multipart encoding separately. Do not test browser
   rendering to prove pure mapping.

Gate: Artist/Venue APIs accept no read type, multipart wire behaviour is unchanged, universal shared
tests/build and both mobile builds are green.

### Phase 4 — close the inventory and deliver

1. Revisit every baseline row. Confirm each is either a companion, an identity/direct request, an
   API-local transport encoder, a third-party adapter, a presentation projection, or an exhaustive
   registry exactly as specified.
2. Search for new global mapper folders, `XMapper` objects, destination-owned `.from`, prototype
   augmentation, read types used as mutation bodies, and unparsed editable buffers. Resolve every hit
   or record why it belongs to a named excluded category in the ledger.
3. Run focused package tests/builds while editing, then the frontend boundary checks and all six
   surface builds required by `app/AGENTS.md`.
4. Push the coherent implementation checkpoint so draft-PR CI validates the exact remote head; review
   the full diff before merge.
5. Select the merge-queue E2E tier mechanically from the merge policy. This refactor does not by
   itself justify a local E2E run.

Gate: the complete inventory is closed, the review has no open findings, exact-head CI is green, and
the source PR reaches terminal merge state.

## Verification matrix

Use the scripts present on the refreshed branch; Phase 0 records any renamed workspace. The expected
minimum is:

```text
npm -w @concertable/b2b test
npm -w @concertable/b2b run build
npm -w @concertable/shared test
npm -w @concertable/shared run build
npm -w @concertable/customer run build
npm run lint:boundaries
npm run test:boundaries
npm -w @concertable/web-customer run build
npm -w @concertable/web-venue run build
npm -w @concertable/web-artist run build
npm -w @concertable/web-business run build
npm -w @concertable/mobile-b2b exec -- tsc --noEmit -p tsconfig.json
npm -w @concertable/mobile-b2b exec -- expo export --platform android
npm -w @concertable/mobile-customer exec -- tsc --noEmit -p tsconfig.json
npm -w @concertable/mobile-customer exec -- expo export --platform android
git diff --check
```

Run commands from `app/` except `git diff --check`. If a package exposes no test script after the
refresh, test its companion behaviour in the nearest owning workspace that already runs Vitest; do
not introduce a second runner.

## Testing rules

- Test conversions that branch, normalize, omit fields, restructure nesting, or distinguish read and
  write shapes.
- Do not add a unit test for a direct identity pass-through or a one-field object literal.
- Pure companion tests live beside the owning feature as `types.test.ts`.
- API tests assert transport facts such as route/body separation and multipart field names; companion
  tests do not mock HTTP.
- Zod tests prove raw invalid buffers are rejected and normalized parsed values are what companions
  receive.

## Explicit exclusions

- Backend C# mappers, DTO projection strategy, and PR #617.
- Runtime mapping libraries, decorator metadata, reflection, code generation, Effect, Remeda,
  AutoMapper TypeScript, `class-transformer`, `ts-pattern`, and the TC39 pipeline proposal.
- Prototype extension methods or conversion of interface contracts into classes.
- A repository-wide rewrite of ordinary rendering `.map` calls.
- Moving transport encoders, third-party adapters, or presentation projections into domain companions.
- Refactoring unrelated state, toast, hook, or query architecture unless required to make one of the
  named write boundaries parse and submit a slim request.

## Definition of done

- The interface-plus-companion pattern and exclusions are present in current frontend guidance.
- `desired.map(Opportunity.toRequest)` is the canonical Opportunity call site.
- Every inventoried domain conversion has the exact owner recorded above; no item is left as a generic
  future cleanup.
- Slim request interfaces live in feature `types.ts`, route ids remain API arguments, and edited APIs
  do not accept read models as writes.
- Raw editable buffers are parsed before request construction; parsed identity shapes are passed
  directly instead of wrapped in ceremonial companions.
- No new runtime dependency, global mapper folder, class model, prototype augmentation, or mapper
  service exists.
- Focused unit/API tests, package builds, boundary checks, all four web builds, and both mobile builds
  are green on the reviewed head.
- The implementation PR is reviewed and merged through the repository's remote-first delivery flow.
