# Frontend architecture refactor — execution plan

> Working doc for the FE architecture refactor. The defined architecture is authored **and signed
> off**: [`app/web/CODE_CONVENTIONS.md`](../app/web/CODE_CONVENTIONS.md) (naming/style) +
> [`app/web/CODE_PATTERNS.md`](../app/web/CODE_PATTERNS.md) (structure). This plan tracks the remaining
> **code** refactor that brings `app/` under them. `git rm` this file in the commit that lands the last
> phase.

Branch `Feature/TenantInvitationsFrontend` — stays here (rides with the in-flight invitations FE work;
same type of change). Personal repo: plain git/gh, commit at boundaries, push only when asked.

## Status

- [x] **AUDIT** — full FE audit against tiers + anti-patterns (findings appendix below).
- [x] **AUTHOR** — `CODE_CONVENTIONS.md` revised to backend rigor; `CODE_PATTERNS.md` written (8
  structural patterns, each with a backend sibling + "anti-patterns this replaces"). Signed off.
- [ ] **REFACTOR** — phased below. **All four web builds green at every step** is the gate.

## Locked decisions

1. **Identity — compose per product.** Base `User` (`@concertable/shared`) = the intersection only
   (id, email, isEmailVerified, …universal). B2B owns a **composed identity** (base user + persona +
   `memberships`) populated by a **B2B-owned typed `/me`**. Kills the `membershipsOf` cast *and* the
   persona-enumerating union together. FE mirror of backend `ICurrentUser` vs `ICurrentTenant`.
2. **Tier cleanup — relocate now.** `deals` / `opportunities` / `applications` move from `app/shared`
   (universal) → `@b2b/*`.
3. **Canon additions in scope** (beyond the original kickoff list): concert-draft scatter +
   server-data-in-store; the deals/opps/apps tier leak; the double-toast bug; permission-matrix drift.

## Build gate (run after every step)

```
npm -w @concertable/web-customer run build
npm -w @concertable/web-venue run build
npm -w @concertable/web-artist run build
npm -w @concertable/web-business run build
```
Add/rename a route → regenerate that app's `routeTree.gen.ts` (`vite build` once) before `tsc -b`.

---

## Phases — small, reviewable, green at each step

### Phase 1 — Kill the double-toast (canon #4) — ✅ DONE
`MutationCache.onError` is now the only API-error toast. Every feature-local `onError` toast and
`try/catch`→`toast.error` removed across `MembersPage`, `OrganizationPage`, `useMyVenue`/`useMyArtist`,
and the application/booking/door-revenue confirm hooks; the confirm hooks converted from
`mutateAsync`+`try/catch` to `mutate`+`onSuccess` (so `ConfirmActionDialog.onConfirm` is `() => void`).
`AddReview`'s "select a star rating" toast is now an inline validation message.

### Phase 2 — Tenant/identity consolidation (patterns #2 + #3) — ✅ DONE
Collapsed the sprayed tenant domain into one composed-identity slice with one state home.
- **2a. B2B identity slice** ✅ — `@b2b/features/tenant`: `types` adds `B2bIdentity` (`extends User` +
  `memberships`, matching the flat `/me` wire); `api/identityApi.ts` is the typed `getMe(): Promise<B2bIdentity>`;
  `model.ts` is the **one core** — pure selectors (`forPersona`/`resolveActiveMembership`/`choicePending`)
  with a `getState`/cache entry point (`getTenantChoicePending`, `reconcileActiveTenant`) and a hook entry
  point (`useMemberships`/`useActiveMembership`/`useTenantChoicePending`/…) over the same core. Persisted
  `useActiveTenantStore` (client selection) unchanged; memberships stay in the `/me` query cache (not copied
  into the store).
- **2b. Base `User` sheds personas** ✅ — `@concertable/shared/features/auth/types.ts` is now a single
  `interface User` = the Kernel `IUser` intersection; dropped the `VenueManager`/`ArtistManager`/`Customer`/`Admin`
  subtypes, `$type`, `venueId`/`artistId`, `baseUrl`. **Resolved sub-decision: `role` STAYS on base `User`** —
  the backend intersection (`Concertable.Kernel.Identity.IUser`) carries `Role`, so it's universal, not
  persona-ish; and with the union gone `role` is no longer a second discriminant. `isVenueManager`/`isArtistManager`
  were dead (no consumers) → deleted, not moved (persona now derives from `useActiveMembership().type`).
- **2c. Populate via typed `/me`** ✅ — b2b `__root.tsx` calls `useSyncUser(identityApi.getMe)`; memberships
  are read from the typed `/me` query cache (`B2bIdentity`), never a cast off the base `User`. `guards.ensureUser`
  stays base-identity only (session/role) — memberships are now a separate, typed concern.
- **2d. Deleted the duplication** ✅ — removed the `membershipsOf` cast, `lib/tenantChoice.ts`, and
  `hooks/useActiveMembership.ts`; single core in `model.ts`. `_venue`/`_artist` `route.tsx` use the getState
  form in `beforeLoad` and the hook form in the layout.
- **2e. `tenantPermissions.ts` stays pure** ✅ (untouched rulebook); `useHasPermission` composes it over
  the active membership in `model.ts`.
- **2f.** ✅ `b2bAxios.ts` still reads `activeTenantId` from `useActiveTenantStore` for `X-Tenant-Id` (no change needed).
- **2g.** ✅ Rewired `TenantSwitcher`/`TenantChooser` (import from `../model`); `MembersPage` gates + `useAcceptInvitation`
  consume the unchanged index surface.

### Phase 3 — Tier relocation (canon #2) — ✅ DONE
Moved the two B2B-only leaks out of `@concertable/shared`; the customer build is proven unable to
resolve them.
- **opportunities + applications** ✅ — `Opportunity`/`OpportunityDraft`/`Application`/`ApplicationStatus`
  (+ `ApplicationActions`/`OpportunityActions`), the `useOpportunities*` + `useApplicationQuery` bundles,
  `opportunityApi`/`applicationApi`, and `useOpportunitiesStore` moved into `@b2b/features/concerts` (the
  re-export shells became the real files; new `@b2b` `concerts/types.ts`). `Checkout`/`PaymentAmount`/
  `ESignatureRequest` + the `Concert` reads stay in shared (customer ticket checkout uses them). Dropped
  the leaked `Opportunity`/`Application`/`ApplicationStatus` re-exports from the `app/web/shared` concerts
  barrel. All b2b hook/api consumers already imported via `@b2b/*`, so only the type-consumers on the
  universal `@/features/concerts` barrel were repointed.
- **deals** ✅ — the `deals` feature (types, `defaultDeal`/`DEAL_TYPE_LABELS`, `dealSummary`) moved into
  `@b2b/features/deals`; the four b2b dashboard widgets repointed. Deleted `app/shared/features/deals`
  and its `package.json` export.
- **Discovered leak (beyond the audit): the shared `dashboard` carried `Deal`.**
  `OpportunitySummary`/`OpportunityWithCounts`/`OpportunityCard` (`app/shared/.../dashboard/deals/common.ts`)
  each embed a `Deal`, so shared could not shed deals while still defining them. These three are consumed
  only by venue+artist dashboards → extracted to a new `@b2b/features/dashboard` (six consumers repointed);
  the universal dashboard types (`ProfileHealth`/`ActivityItem`/`MonthlyRevenuePoint`/`Settlement`/…) stay
  in shared.
- **Boundary proven** ✅ — a throwaway customer import of `@concertable/shared/features/deals` fails
  `TS2307` (module gone) and `Opportunity`/`Application` fail `TS2305` (not exported from shared concerts);
  all four web builds green.

### Phase 4 — Hooks orchestrate; components render (+ zod at the write boundary) — ✅ DONE
- **`DeclareDoorRevenueButton`** ✅ — `useDeclareDoorRevenue(concert, rawValue)` owns the zod parse and
  the concertable/external/total derivation; the component keeps only its buffer `useState` + JSX.
- **`PayoutAccountSection`** ✅ — new `usePayoutAccount` facade owns the `window` `message` listener, the
  refetch-then-toast branching and `window.open`; the component renders status and calls `openOnboarding()`.
- **`organizations`** ✅ — `useOrganization` facade owns the buffer → request mapping, the `safeParse` and
  the save; added `schemas/updateOrganizationRequestSchema.ts` with `UpdateOrganizationRequest` as its
  `z.infer` (dropped from `types.ts`, the `updateConcertRequestSchema` precedent); `OrganizationForm`
  moved to `components/` keeping only its buffer + JSX.
- **review** ✅ — added `schemas/createReviewRequestSchema.ts`; `useAddReview` is now a facade owning the
  parse + submit (replacing the hand-rolled `stars === 0`); `concertId` stays a function argument, not a
  request field.
- **`MembersPage`** ✅ — split into `components/{MembersRoster,PendingInvitations,InviteForm,Spinner}` over
  `useMembersRoster` / `usePendingInvitations` / `useInviteMember` facades; `membersApi` split off the
  cross-resource accept path into `api/invitationApi.ts`; the hand-maintained `ALL_ROLES` replaced by
  `TENANT_ROLES` in `@b2b/features/tenant`, with `TenantRole` **derived from it** so list and type can't
  drift. **This one rides with the (still uncommitted) invitations feature — deliberately kept out of the
  `refactor(web)` commits.**
- **Moved to Phase 5, not skipped — `useMyVenue`/`useMyArtist` + create-artist/venue edit-form zod.**
  Their write path PUTs the *whole entity* as `FormData` (artist literally `JSON.stringify(artist)`), so
  parsing into a real `XRequest` changes **what the backend receives** — a request-contract change, not a
  drop-in parse. They also sit on the same `draft = {...entity}` store-copies-cache anti-pattern Phase 5
  already owns for `useConcertStore`. Doing them together is the only way either lands clean.
- **N/A (no write boundary to parse):** preference — `CreatePreferencePage` is still a
  `<div>Create Preference</div>` stub; ticket checkout — its only input is the bounded `QuantitySelector`,
  no free-typed field.

### Phase 5 — Naming + state hygiene
- **Permissions:** complete the matrix to the full backend `SharedPermissions` set (13, not 4),
  backend-sourced names; document that the server is the trust boundary (FE gate is cosmetic).
- **`useConcertStore`:** stop copying server data into the store (`draft = {...concert}`); live buffer is
  the component's `useState`. Derive `isDirty`, don't store it. **Same for `useVenueStore` /
  `useArtistStore` — and land their edit-form zod in this same change** (carried over from Phase 4):
  `venueApi.updateVenue` / `artistApi.updateArtist` currently PUT the whole entity as `FormData`, so they
  need real `UpdateVenueRequest` / `UpdateArtistRequest` shapes + schemas before a parse means anything.
  Same for `CreateArtistPage` / `CreateVenuePage` (`draft!.name` bangs + `banner! as unknown as File`).
- Raw hook suffixes (`useVenueKpis` → `useVenueKpisQuery`, …); leave facades.
- Per-feature query-key factories (incremental, per feature touched).
- Read/request naming: `PaymentResponse` → shared `PaymentOutcome` (dedupe `TicketPurchaseResponse`);
  `CreateArtist` → `CreateArtistRequest`; move inline `XRequest`s (`ESignatureRequest`,
  `UpdateConcertRequest`, `TicketPurchaseRequest`, `CreateReviewRequest`) into feature `types.ts`.
- Lift `ProblemDetails` + `handleError` policy into `@concertable/shared` (mobile reuse).

### Phase 6 — Tech debt: axios factory
`createApiClient(name)` + shared `attachAuth(client)` to remove the 4-copy client + interceptor
duplication (same two-layer shape). Log the closure in the nearest `TECH_DEBT.md`.

---

## Audit appendix — file-level findings (reference for the phases)

**Tenant domain today (Phase 2 target):** `app/shared/features/auth/types.ts` (`User` union + persona
fields + `role`/`$type` dual discriminant, guards on `role`) · `app/shared/features/auth/store/useAuthStore.ts`
· `web/shared/features/user/hooks/useSyncUser.ts` + `web/shared/features/auth/guards.ts` (fill the store
from `/auth/me`) · `@b2b/features/tenant/{types,constants,store/useActiveTenantStore,lib/tenantChoice,
hooks/useActiveMembership,tenantPermissions,components/*,index}` · `@b2b/lib/b2bAxios.ts` (reads
`activeTenantId` → `X-Tenant-Id`) · `b2b/{venue,artist}/routes/_{venue,artist}/route.tsx` (call both the
imperative *and* hook forms of `tenantChoicePending`) · `@b2b/features/members/hooks/useAcceptInvitation.ts`.
The bridge: `membershipsOf` (`lib/tenantChoice.ts:9`) is the single cast feeding both chains.

**Duplicated derivations (drift):** `samePersona`/`isTenantChoicePending` (`tenantChoice.ts`, imperative)
vs `useSamePersonaMemberships`/`useTenantChoicePending` (`useActiveMembership.ts`, reactive) — same core
`tenantChoicePending`, two callers.

**Tier leaks in `app/shared` (Phase 3):** `features/deals/types.ts` (Deal union + PaymentMethod);
`features/concerts/index.ts` (Opportunity/Application/OpportunityDraft + `useOpportunities*` +
`useApplicationQuery` bundle); `PaymentResponse` re-export.

**Central-error violations (Phase 1):** listed under Phase 1.

**Orchestration-in-component (Phase 4):** `MembersPage` (roster/invitations/invite-form),
`OrganizationForm`, `PayoutAccountSection`, `DeclareDoorRevenueButton`.

**State misuse (Phase 5):** `app/shared/features/concerts/store/useConcertStore.ts` (`draft` copies cache
data; `isDirty` hand-maintained).

**Permissions (Phase 5):** `@b2b/features/tenant/tenantPermissions.ts` `byRole` models 4 of 13 backend
`SharedPermissions.ByRole` entries. `TenantRole`/`TenantPermission` names already match backend.

**Folder anatomy:** `members` no `components/`; `organizations` no `schemas/`; `tenant` no `api/`
(memberships via cast); no query-key factories anywhere.

**Axios dup (Phase 6):** `app/shared/lib/{axios,paymentAxios,searchAxios}Client.ts` (3 near-verbatim) +
per-app interceptor wiring (`web/shared/lib/{axios,paymentAxios,searchAxios}`, `@b2b/lib/b2bAxios`).
