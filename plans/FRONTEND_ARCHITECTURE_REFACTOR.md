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

### Phase 2 — Tenant/identity consolidation (patterns #2 + #3) — the headline
Collapse the sprayed tenant domain into one composed-identity slice with one state home.
- **2a. B2B identity slice.** `@b2b/features/identity` (or fold into `features/tenant`): `types`
  (`B2bIdentity` = base user fields + `memberships: Membership[]`, matching the flat `/me` wire), `api`
  (`getMe(): Promise<B2bIdentity>` — the typed `/me`), one **model/store** owning `activeTenantId`
  (persisted) + selectors (memberships-for-persona, active membership, choice-pending). Expose **one
  core** with both a `getState`-based fn (route `beforeLoad`) and hooks (render) — no dual impl.
- **2b. Base `User` sheds personas.** `@concertable/shared/features/auth/types.ts`: `User` becomes the
  intersection; drop `VenueManager`/`ArtistManager`/`Customer`/`Admin` subtypes + `venueId`/`artistId`.
  Move `isVenueManager`/`isArtistManager` into the B2B identity (persona from memberships).
  **Open sub-decision:** does `role` stay on base `User` or move to the composed identity? (Leaning:
  move — it's persona-ish; revisit against what OIDC/`/me` actually returns.) Customer/mobile compose
  their own identity if they need more than the base — watch their builds here.
- **2c. Populate via typed `/me`.** B2B reads memberships from the identity store (typed), not the
  shared `userApi.getMe` cast. Reconcile `useSyncUser` / `guards.ensureUser` with the base user +
  B2B identity hydration.
- **2d. Delete the duplication.** Remove `membershipsOf` cast + `lib/tenantChoice.ts` imperative
  predicates + `hooks/useActiveMembership.ts` reactive duplicates → single core. Rewire
  `_venue/route.tsx` + `_artist/route.tsx` (`beforeLoad` → getState form; layout → hook form).
- **2e. `tenantPermissions.ts` stays pure** (stateless rulebook); `useHasPermission` composes it over
  the identity store's active membership.
- **2f.** `b2bAxios.ts` reads `activeTenantId` from the identity store for `X-Tenant-Id`.
- **2g.** Rewire consumers: `TenantSwitcher`, `TenantChooser`, `MembersPage` gates, `useAcceptInvitation`.

### Phase 3 — Tier relocation (canon #2)
Move from `app/shared` → `@b2b/*` (customer/mobile must stop compiling them):
- `deals` (`app/shared/features/deals/types.ts` → `@b2b`; drop the b2b one-line re-export shell).
- `opportunities` + `applications` types/hooks currently in `app/shared/features/concerts/index.ts`
  (`Opportunity`, `OpportunityDraft`, `Application`, `ApplicationStatus`, `useOpportunities*`,
  `useApplicationQuery` bundle) → `@b2b`; delete the `@b2b` re-export one-liners.
- Verify the customer build now *cannot* resolve them (the gate proves the boundary).

### Phase 4 — Hooks orchestrate; components render (+ zod at the write boundary)
- `MembersPage.tsx`: extract `InviteForm`/`MembersRoster`/`PendingInvitations` orchestration into facade
  hooks (`useInviteMember`, …); component keeps buffer `useState` + JSX. Split `membersApi` off the
  cross-resource `acceptInvitation` path. Add `members/components/`.
- `organizations`: `OrganizationForm` orchestration → `useOrganization` facade; add
  `organizations/schemas/updateOrganizationRequestSchema.ts`; parse buffer → `UpdateOrganizationRequest`.
- `payments/PayoutAccountSection.tsx`: window/message + refetch orchestration → a hook.
- `DeclareDoorRevenueButton.tsx`: `safeParse` + totals → the hook.
- Remaining zod: `useMyVenue`/`useMyArtist` edit forms; create artist/review/preference/ticket inputs.

### Phase 5 — Naming + state hygiene
- **Permissions:** complete the matrix to the full backend `SharedPermissions` set (13, not 4),
  backend-sourced names; document that the server is the trust boundary (FE gate is cosmetic).
- **`useConcertStore`:** stop copying server data into the store (`draft = {...concert}`); live buffer is
  the component's `useState`. Derive `isDirty`, don't store it.
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
