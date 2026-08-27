# Frontend domain API and companion mapping refactor

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

The same migration also closes the adjacent readability and encapsulation problems found during the
inventory: closed labels use typed lookup tables, cohesive browser state uses one service object,
feature stores stay behind facade hooks or an imperative session, and client-owned absence uses
`undefined`. It does not manufacture method syntax for interface values.

## Final convention

### Naming and placement

1. Name the companion exactly after its source interface: `export interface Opportunity` plus
   `export const Opportunity`.
2. Name operations `toX`. Use the shortest unambiguous destination name: `toRequest` when the feature
   has one request shape for the concept and `toCreateRequest` and `toUpdateRequest` when both exist.
   Use `toFormValues` only when a read model genuinely initializes a differently shaped RHF form.
3. Put the companion immediately below the related interfaces in the owning feature's `types.ts`.
   Keep all companions in `types.ts` for this migration; do not create a `domain/`, `mappers/`,
   `mapping/`, or general `utils/` folder.
4. Export the companion through the existing feature barrel only when consumers already import the
   corresponding type through that barrel. Do not widen a package tier solely to share a mapper.
5. Give every method an explicit parameter and return type. Request return types are exported
   contracts from the same feature's `types.ts`, not anonymous objects declared in an API module.

### Interface and Zod relationship

Distinct object contracts remain interfaces in `types.ts`; schemas validate them but do not replace
them with schema-derived aliases. When a write contract is exactly a read-model subset, derive it with
`Pick` or `Omit` instead of duplicating its fields. Bind the schema to that contract explicitly:

```ts
export type PreferenceRequest = Omit<Preference, "id" | "userId">;

export const preferenceRequestSchema = z.object({
  radiusKm: z.number().min(1),
  genres: z.array(z.enum(GENRE_VALUES)),
}) satisfies z.ZodType<PreferenceRequest>;
```

Do not also declare `type PreferenceRequest = z.infer<typeof preferenceRequestSchema>`. Interactive
forms use React Hook Form with `zodResolver`; the schema's parsed output is the request passed to the
mutation. When raw form values differ from the request because the schema normalizes or restructures
them, use `useForm<FormValues, unknown, Request>` and derive `FormValues` with `z.input`. Do not create
`XBuffer` or `XDraft` types merely to mirror form state.

### Behaviour boundary

A companion method must be pure, synchronous, deterministic, and free of React, hooks, HTTP clients,
global stores, navigation, environment state, and side effects. It may select fields, rename fields,
construct nested domain/request shapes, normalize already-validated values, and handle a closed
discriminator exhaustively.

The source owns the operation. Do not add destination-owned `from`, TypeScript prototype augmentation,
classes used only to gain methods, declaration merging tricks, decorators, reflection, or service-style
`XMapper` objects.

### Domain-facing API shapes

Use the runtime shape that honestly owns the behaviour:

| Behaviour                               | Required spelling                                                                                                                    |
| --------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| Membership in a real runtime collection | Call the collection method directly, such as `permissions.has(permission)`. Do not wrap it in `can(permission)`.                     |
| Label for a closed union                | Index one exhaustive typed table, such as `GENRE_LABELS[genre]`. Do not keep a one-line `genreLabel(genre)` wrapper.                 |
| Cohesive stateful browser capability    | Export one lower-camel-case service object, such as `consent.read()`, `consent.write()`, `consent.has()`, and `consent.subscribe()`. |
| Pure calculation with no source object  | Keep a named pure function whose noun states the result, such as `paymentSummary(amount)`.                                           |
| Reusable source-shape conversion        | Use the same-name companion and source-owned `toX` operation defined above.                                                          |

Do not introduce a companion solely to turn an existing function into fake instance syntax. A value
can call `.toX()` only when it is a real runtime object constructed with that method; API response
interfaces are plain data, so this plan deliberately keeps `Opportunity.toRequest(opportunity)`.

### Absence semantics

Client-owned absence is `undefined`: optional fields, hook results, local state, context defaults,
service reads, and store state use `?: T` or `T | undefined`. Do not introduce or retain
`T | null | undefined`.

`null` remains only when the runtime or protocol makes it meaningful or mandatory:

- JSX intentionally renders nothing with `return null`;
- DOM, React ref callbacks, browser Storage, and third-party SDK signatures require `null`;
- JavaScript object guards must reject `null` because `typeof null === "object"`;
- an application branch genuinely distinguishes explicitly cleared from never supplied.

An explicitly nullable server field is not by itself sufficient. Client contracts model it as optional
unless the distinction is acted on, and consumers continue to read defensively with `??` or `!= null`
at the wire edge.

### Classification table

| Transformation                                                         | Required owner and spelling                                                                                                                      |
| ---------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| Shapes are already identical                                           | Delete the mapper and type the value as the destination.                                                                                         |
| Write shape omits only read/server fields                              | Derive the request with `Omit<Read, ...>`; use `Pick<Read, ...>` for a strict writable subset. Do not add a mapper when selection alone is sufficient. |
| Reusable or semantic domain/read → request conversion                  | Source companion in feature `types.ts`, named `Source.toRequest` or the specific `toXRequest`.                                                   |
| User-editable form → request                                           | A private feature store may own a neutral shared draft; React Hook Form owns validation, dirty state, and submission, and `zodResolver` produces the request passed directly to the mutation. Use a schema transform for restructuring. |
| Query parameters, route construction, headers, JSON/multipart encoding | Keep in `api/xApi.ts`. The complex encoders named by this plan become module-private `toX` helpers; one-use one-or-two-field bodies stay inline. |
| Raw third-party response → internal value                              | Keep in the third-party adapter module; do not place raw SDK/wire types on a domain companion.                                                   |
| Domain value → chart row, label, JSX props, or other view model        | Keep a named pure projection beside the rendering hook/component.                                                                                |
| Behaviour varies by a closed discriminator                             | Use one exhaustive typed registry/table in the owning feature; do not add `ts-pattern` for this refactor.                                        |
| Single-use one-or-two-field request body                               | Keep the object literal inline with an explicit request type where it crosses a function boundary.                                               |

## Dependency and branch gate

PRs #595, #600, and #637 are merged. The implementation branch is
`Refactor/frontend_domain-companion-mapping`, created from `origin/main` at
`09c535eb8101cccf93b8652f167245732daed244` after the branch-time platform-sync check passed.

`Refactor/OrganizationProfileRouteContraction` has committed frontend Artist/Venue changes but no
open PR. It is not an implementation base and does not block this plan. Phase 0 must classify it
again: if its content reached `main` through another branch, use the landed shape; otherwise preserve
it as unrelated work and refactor the `origin/main` implementation only.

The backend `Refactor/OpportunityMapperPagination` work is unrelated. This plan must not edit or
coordinate C# mappers.

## Complete baseline inventory and required disposition

The line descriptions below are semantic anchors; Phase 0 refreshes line numbers after the dependency
gate without changing the recorded decision unless the underlying shape changed.

| Current site                                                                                                                           | Existing transformation                                                                                                       | Required disposition                                                                                                                                                                                                                                                                                                                        |
| -------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `app/shared/src/types/common.ts` and genre renderers                                                                                   | `genreLabel(genre)` wraps one closed typed table.                                                                             | Export `GENRE_LABELS`, index it directly, and remove the wrapper.                                                                                                                                                                                                                                                                           |
| `app/shared/src/features/messaging/types.ts` and `Mailbox.tsx`                                                                         | `messageActionLabel(action)` wraps one closed typed table.                                                                    | Export `MESSAGE_ACTION_LABELS`, index it directly, and remove the wrapper.                                                                                                                                                                                                                                                                  |
| `app/web/b2b/shared/src/features/tenant/constants.ts` and member components                                                            | `tenantRoleLabel(role)` wraps one closed typed table.                                                                         | Export `TENANT_ROLE_LABELS`, index it directly, and remove the wrapper. Keep `permissions.has(permission)` unchanged.                                                                                                                                                                                                                       |
| `app/web/b2b/shared/src/features/concerts/utils/acceptCheckoutFormat.ts`                                                               | `summaryFor(amount)` is pure but its name omits the result.                                                                   | Rename it to `paymentSummary(amount)`; do not create a companion around `PaymentAmount`.                                                                                                                                                                                                                                                    |
| `app/web/shared/src/lib/consent.ts` and `ConsentProvider.tsx`                                                                          | Four free functions jointly own versioned local storage and subscriptions, and absence is exposed as `null`.                  | Export one `consent` service with `has`, `read`, `write`, and `subscribe`; call the stored value `StoredConsent`; return `undefined` for no valid decision.                                                                                                                                                                                 |
| `app/web/b2b/shared/src/features/concerts/api/opportunityApi.ts`                                                                       | Anonymous `desired.map` strips `venueId`/`actions` and conditionally carries `id`.                                            | Introduce `OpportunityRequest` and `Opportunity.toRequest` in concerts `types.ts`; use `desired.map(Opportunity.toRequest)`. This is the canonical companion example.                                                                                                                                                                       |
| `app/web/b2b/shared/src/features/organizations/components/OrganizationForm.tsx`                                                        | Local state flattens an `Organization` read into editable fields and defaults absent compliance data.                         | Use `Organization.toFormValues` for RHF defaults; let the Zod resolver normalize and transform those values into `UpdateOrganizationRequest`.                                                                                                                                                                                               |
| `app/web/b2b/shared/src/features/organizations/hooks/useOrganization.ts` and `schemas/updateOrganizationRequestSchema.ts`              | The hook constructs a nested request from raw form state and only then validates it.                                          | Make the schema transform RHF form values into the nested request and let the hook accept only `UpdateOrganizationRequest`.                                                                                                                                                                                                                 |
| `app/web/customer/src/features/reviews/hooks/useAddReview.ts` and `app/customer/shared/src/features/reviews/api/reviewApi.ts`          | The form reshapes local state; the shared request incorrectly contains route-only `concertId`, which the API strips.          | Use RHF plus `zodResolver`, change the shared API to `createReview(concertId, request)`, and remove `concertId` from `CreateReviewRequest`.                                                                                                                                                                                                    |
| `app/shared/src/features/messaging/hooks/useReportMessage.ts` and `schemas/reportMessageRequestSchema.ts`                              | An object literal trims/omits details before parsing.                                                                         | Use RHF plus `zodResolver`, put normalization in the Zod schema, and pass its `ReportMessageRequest` output directly.                                                                                                                                                                                                                        |
| `app/customer/shared/src/features/preferences/api/preferenceApi.ts`, `hooks/usePreferenceQuery.ts`, and mobile `PreferencesScreen.tsx` | Update accepts a full `Preference` read and the screen reconstructs server-owned `id`/`user`; create drops selected genres.   | Model the actual response with `userId`, derive `PreferenceRequest` by omitting `id` and `userId`, keep route `id` as a function argument, bind its schema, and use RHF for both create and update.                                                                                                                                            |
| `app/web/b2b/shared/src/features/concerts/hooks/useMyConcert.ts` and its private `store/useConcertStore.ts`                            | A full `Concert` read was copied into editor state and silently stripped to an update request by Zod.                         | Derive `UpdateConcertRequest` as the writable `Pick<Concert, ...>` and let the private `ConcertState` draft perform that selection once when editing begins; RHF/Zod owns validation and request submission. No companion mapper is required.                                                                                                |
| `app/shared/src/features/artists/api/artistApi.ts`, `hooks/useMyArtist.ts`, and private `store/useArtistStore.ts`                       | Create types lived in the API module; update accepted a full `Artist` read, then manually encoded selected fields as multipart. | Define slim create/update request interfaces and `Artist.toUpdateRequest`; keep a neutral `ArtistState` draft behind workflow hooks, use RHF plus Zod for requests, and keep module-private multipart encoders in `artistApi.ts`.                                                                                                               |
| `app/shared/src/features/venues/api/venueApi.ts`, `hooks/useMyVenue.ts`, and private `store/useVenueStore.ts`                           | Update accepted a full `Venue` read, then manually encoded selected fields as multipart.                                     | Define slim create/update request interfaces and `Venue.toUpdateRequest`; keep a neutral `VenueState` draft behind workflow hooks, use RHF plus Zod for requests, and keep module-private multipart encoders in `venueApi.ts`.                                                                                                                 |
| `app/shared/src/features/search/api/headerApi.ts`                                                                                      | `SearchFilters` is renamed and combined into HTTP query parameters.                                                           | Define a private `HeaderSearchParams` interface and module-private `toSearchParams(filters)` in `headerApi.ts`, then pass its result as Axios params. Do not put transport parameters on `SearchFilters`.                                                                                                                                   |
| `app/shared/src/features/artists/api/artistApi.ts` and `venues/api/venueApi.ts`                                                        | Domain/request fields become PascalCase multipart entries and indexed genre keys.                                             | Keep the exact wire conversion in the four module-private `toCreateFormData`/`toUpdateFormData` helpers named above. These are transport encoders, not domain companions.                                                                                                                                                                   |
| `app/web/shared/src/features/dashboard/components/MonthlyRevenueChart.tsx`                                                             | Revenue points become Recharts rows with formatted months and major currency units.                                           | Define a private `ChartRevenuePoint` interface and `toChartRevenuePoint(point)` beside the component; call `data.map(toChartRevenuePoint)`. It remains a presentation projection.                                                                                                                                                           |
| `app/shared/src/lib/googleGeocodingApi.ts`                                                                                             | Google address components and result ranking become an internal location label.                                               | Keep the private adapter functions in this module. Raw Google response types do not enter feature domain companions.                                                                                                                                                                                                                        |
| `app/web/b2b/shared/src/features/concerts/components/applications/AcceptDealSummary.tsx`                                               | Closed deal variants dispatch through an exhaustive typed record.                                                             | Preserve the registry pattern; no mapper library or `ts-pattern` migration.                                                                                                                                                                                                                                                                 |
| `app/web/b2b/shared/src/features/members/hooks/useInviteMember.ts`                                                                     | Hand-written local form state and validation duplicate RHF.                                                                   | Use RHF plus `zodResolver`; the hook accepts only `InviteMemberRequest`.                                                                                                                                                                                                                                                                     |
| `app/shared/src/features/concerts/api/concertApi.ts`, ticket/payment/application API calls, and self-billing API calls                 | One-use small bodies are posted directly.                                                                                     | Preserve inline request bodies when they are one or two fields and do not hide a read/write shape conversion.                                                                                                                                                                                                                               |

## State boundary inventory and required disposition

| Current boundary                                                   | Leak                                                                                                                                             | Required disposition                                                                                                                                                                                                                                                                  |
| ------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `app/b2b/shared/src/features/tenant/index.ts`                      | Pure membership derivation internals and `TenantResolution` are exported although production consumers use only `useTenant` and `tenantSession`. | Remove the derivation exports from the barrel. Keep the functions module-private to the feature and directly unit tested.                                                                                                                                                             |
| Universal and web search barrels plus web/mobile search components | `useSearchFiltersStore` is public and components assemble transitions from `setFilters`; web `SearchBar` also calls `getState()`.                | Add the universal `useSearchFilters` facade returning `filters`, `replaceFilters`, and `updateFilters`. The web facade composes it with router synchronization and adds `applyFilters`. Remove store exports and all component/store imports.                                         |
| Web auth/user components and guards                                | TanStack Query user data is copied by `useSyncUser` into `useAuthStore`, creating two reactive owners.                                           | Add `useMeQuery(getMe)`; web components read it and guards use the same query options through `queryClient`. Delete the web store mirror and `useSyncUser`.                                                                                                                           |
| Mobile auth hooks, screens, SignalR, and client configuration      | The universal `useAuthStore` is imported as a public raw store across mobile.                                                                    | Move mobile identity client state into `app/mobile/shared/src/auth/`; expose `useCurrentUser` to React consumers and one internal `mobileAuthSession` to login, logout, initialization, SignalR, and client configuration. Use `undefined` for no user.                               |
| Artist, Venue, and Concert editor pages/components                 | Public stores exposed form fields and transitions directly; several pages combined those stores with existing workflow hooks.                    | Keep neutral editor state in private feature stores consumed only by workflow facade hooks. Let RHF own dirty/valid/error state and parsed request submission; add create workflows and pass image callbacks into Hero components.                                                                                                          |
| `useOpportunitiesStore` and tenant stores                          | Stores are already consumed only by their facade hook/session boundary.                                                                          | Preserve this shape and keep the stores private.                                                                                                                                                                                                                                      |

Raw `Query`/`Mutation` hooks are not automatically leaks: they remain the public server-state API
when a component needs the query result itself. Add a workflow facade only when it composes server
state with client transitions, validation, navigation, or derivation.

## Phases

### Phase 0 — refresh ownership and freeze the migration matrix ✅

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

### Phase 1 — establish the convention and canonical B2B conversions ✅

1. Finish the direct label tables, `paymentSummary`, and cohesive `consent` service already started
   in this worktree. Keep the browser-required `null` checks and use `undefined` for owned absence.
2. Implement `OpportunityRequest` and `Opportunity.toRequest`; replace the API's anonymous map.
3. Refactor Organization initialization to `Organization.toFormValues`; use RHF and a schema transform
   that outputs `UpdateOrganizationRequest` directly.
4. Remove the unused tenant membership derivations from the public barrel without moving or duplicating
   their pure implementation.
5. Add focused tests for Opportunity's conditional `id`/field omission and Organization's absent
   compliance defaults, VAT branch, optional address line, trimming, and nested request shape.

Gate: the domain-facing APIs match the classification table, the canonical call site reads
`desired.map(Opportunity.toRequest)`, Organization maps only validated data, and the B2B package tests
plus web-shared consent tests and affected package builds are green.

### Phase 2 — correct shared customer and form write boundaries ✅

1. Split route identity from the review request and move the form to RHF plus `zodResolver`.
2. Move report-message normalization into its schema and move the form to RHF.
3. Replace Preference's read-as-write API with the slim shared request and correct both create and
   update mobile submissions, including selected genres.
4. Replace the full-Concert edit copy with a neutral `ConcertState` draft behind `useMyConcert`;
   derive `UpdateConcertRequest` with `Pick` and initialize RHF from the exact draft selected by the store.
5. Keep `useConcertStore` private, remove every hand-written `XBuffer` form abstraction, and make
   affected facade hooks submit parsed request contracts only.
6. Test semantic normalization, route/body separation, create/update parity, form initialization,
   and absence of server-owned fields in request bodies.

Gate: customer/shared and universal shared tests/builds are green; route ids are API arguments; no
edited mutation accepts a read model.

### Phase 3 — correct multipart write contracts without moving transport encoding ✅

1. Reconcile the landed Artist/Venue APIs after the route-contraction branch check.
2. Move create/update request contracts out of API modules and into each feature `types.ts`.
3. Add `Artist.toUpdateRequest` and `Venue.toUpdateRequest`; use RHF and aligned Zod schemas so
   mutations receive parsed slim requests while neutral `ArtistState` and `VenueState` drafts remain
   private behind workflow hooks.
4. Keep PascalCase multipart keys, optional image handling, and indexed genre fields inside the API
   modules.
5. Make the Artist and Venue workflow hooks coordinate their private editor stores with RHF; add create
   workflows and pass image callbacks into Hero components without exposing raw stores to components.
6. Test the pure read-to-request conversions and API-level multipart encoding separately. Do not test browser
   rendering to prove pure mapping.

Gate: Artist/Venue APIs accept no read type, multipart wire behaviour is unchanged, neutral editor
stores are private, and universal shared tests/build plus both mobile builds are green.

### Phase 4 — encapsulate shared client state and normalize absence ✅

1. Replace the web user Zustand mirror with `useMeQuery(getMe)` and query-client-backed guards; delete
   `useSyncUser` and every web `useAuthStore` import.
2. Move mobile identity state behind `useCurrentUser` and `mobileAuthSession`; remove the universal
   auth-store export and migrate all mobile consumers.
3. Put search filter state behind the universal and web `useSearchFilters` facades specified in the
   state inventory; remove every store import, export, and direct `getState()` call.
4. Audit every TypeScript `null` under `app/`. Convert client-owned state, contexts, hook results,
   and optional contract fields to `undefined`; retain only the explicit categories in the absence
   section. Record the retained non-obvious cases in the ledger rather than adding code comments.
5. Delete the resolved `useSyncUser`/`useAuthStore` technical-debt entry in the same commit.

Gate: store-import searches return only private facade/session implementations and focused store tests;
no production type contains `T | null | undefined`; the ledger classifies every retained production
`null`; auth, search, web-shared, and both mobile tests/builds are green.

### Phase 5 — close the inventory and deliver

1. ~~Revisit every baseline row and confirm its final owner.~~
2. ~~Run the mapper, augmentation, read-as-write, buffer, state-boundary, and owned-absence invariant
   searches and resolve or classify every hit.~~
3. ~~Run the complete package, boundary, web, mobile typecheck, and Android export matrix.~~
4. Incrementally review every commit after `539c1a520`, then push the coherent candidate and open its
   PR so CI validates the exact reviewed head.
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
npm -w @concertable/web test
npm -w @concertable/customer run build
npm run lint:boundaries
npm run test:boundaries
npm -w @concertable/web-customer run build
npm -w @concertable/web-venue run build
npm -w @concertable/web-artist run build
npm -w @concertable/web-business run build
npm -w @concertable/web-admin run build
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
- Zod tests prove invalid form values are rejected and normalized request output is what mutations receive.

## Explicit exclusions

- Backend C# mappers, DTO projection strategy, and PR #617.
- Runtime mapping libraries, decorator metadata, reflection, code generation, Effect, Remeda,
  AutoMapper TypeScript, `class-transformer`, `ts-pattern`, and the TC39 pipeline proposal.
- Prototype extension methods or conversion of interface contracts into classes.
- A repository-wide rewrite of ordinary rendering `.map` calls.
- Moving transport encoders, third-party adapters, or presentation projections into domain companions.
- Wrapping raw query hooks in facade hooks when no workflow, derivation, navigation, validation, or
  client-state transition is being composed.

## Definition of done

- The interface-plus-companion and domain-facing API classification is applied at every inventoried site.
- `desired.map(Opportunity.toRequest)` is the canonical Opportunity call site.
- Every inventoried domain conversion has the exact owner recorded above; no item is left as a generic
  future cleanup.
- Slim request contracts live in feature `types.ts`, route ids remain API arguments, and edited APIs
  do not accept read models as writes.
- Neutral editor state is private behind workflow facades; React Hook Form owns validation, dirty
  state, errors, and submission, and `zodResolver` produces mutation requests. No hand-written
  `XBuffer`/`XDraft` request-mirror abstraction remains.
- No new mapping dependency, global mapper folder, class model, prototype augmentation, or mapper
  service exists.
- Feature stores are private behind facade hooks or one imperative session, and client-owned absence
  uses `undefined`; retained `null` occurrences match the plan's explicit categories.
- Focused unit/API tests, package builds, boundary checks, all five web builds, and both mobile builds
  are green on the reviewed head.
- The implementation PR is reviewed and merged through the repository's remote-first delivery flow.
