# Shared frontend boundaries — investigation & refactor brief

**Origin:** 2026-06-04 session, fallout from big-review BUG25. Branch `refactor/Microservices`.

## The principle (user-stated, this is the brief's constitution)

Only genuinely shareable code lives in the shared frontend trees. Anything **auth-coupled or
site-specific** — OIDC identity assumptions, authenticated calls to a specific service, app-specific
route literals — lives in the **owning app**, composed into shared surfaces explicitly. Do NOT fix
leaks by hardcoding role checks (`role === "Customer"`) inside shared code — identity-aware branching
inside shared components is the disease, not the cure (a proposed `enabled: role === "Customer"` gate
on `useCanReviewQuery` was explicitly rejected by the user for this reason).

## Why this matters (what actually happened)

The three web surfaces are **fully separate sites**: each app is its own OIDC client
(`client_id: import.meta.env.VITE_OIDC_CLIENT_ID` → `venue-web` / `artist-web` / `customer-web`),
its own session. A manager logged into the venue site is simply *not logged in* anywhere else.

But shared components don't know which site they're running in:

- `ReviewSection` (shared) renders in manager apps' `VenueDetails`/`ArtistDetails`/`ConcertDetails`
  → `useCanReviewQuery` fires (gated only on `isAuthenticated`) → `customerAxios`'s request
  interceptor attaches *the current site's token* (a manager token) to a **Customer-service** call →
  401 (wrong audience).
- Because of that routine 401, the user removed the 401→`userManager.removeUser()` recovery from
  `customerAxios` in commit `44dc77c9` — otherwise *a review widget logs managers out of their own
  site* (interceptor runs inside the venue app, clears the venue session because the customer backend
  rejected the call). That removal is a band-aid over the real problem: **shared code making
  site-specific authenticated calls**.
- **NOTE9 (in `reviews/BIG-refactor-Microservices-Review.md`) is the same disease in the opposite
  direction**: `npm -w @concertable/web-customer run build` fails with 25 pre-existing TS errors,
  largely shared components hardcoding manager-app route literals that don't exist in the customer
  route tree (`"/my"`, `"/create"`, `"/applications/$applicationId/accept"` in
  `ProfileMenu.tsx` / `artists/guards.ts` / `ApplicationCard.tsx`), plus `ImageFile` vs `File`
  mismatches, `react-dom/client` types, a `PaymentMethod` ambiguous re-export. Both directions =
  unshareable code in shared.

## Evidence inventory (gathered 2026-06-04 — starting points, not exhaustive)

- `app/web/shared/src/features/reviews/api/reviewApi.ts`: GET reviews/summary → `api` (own-app
  backend; B2B serves manager apps from its **own review projections** — that's what `44dc77c9`
  built, so manager review *reads* never need Customer); `canReview`(eligibility)/`createReview` →
  `customerApi` (customer-only semantics).
- `app/shared/src/features/reviews/api/reviewApi.ts` (mobile): **all** calls, including reads, go
  through `customerApi`. Mobile business app currently consumes no reviews feature (grep clean).
- `app/shared/src/features/concerts/api/ticketApi.ts`: purchase/checkout/upcoming/history →
  `customerApi`. Customer-only functionality sitting in the shared tree.
- `useSyncUser` (NOTE8): customer app already forked its own copy hitting `customerApi /user/me` —
  evidence the split is already happening ad-hoc, without a principle.
- Axios clients in `web/shared/src/lib/`: `searchAxios`/`paymentAxios`/base `axios` have
  401→`removeUser`; `customerAxios` deliberately doesn't (`44dc77c9`).
  `app/mobile/shared/src/lib/customerAxios.ts` **does** have the 401→clear-session (added with the
  BUG24 mobile wiring) — latent manager-logout bug on mobile if the business app ever consumes a
  customer-authenticated shared feature.
- Role helpers exist (`isVenueManager`/`isArtistManager` in `auth/types.ts`; `Role` includes
  `"Customer"`; `useApply.ts` gates with `enabled: isArtistManager`) — note these are *themselves*
  instances of identity-branching inside shared; consider whether they survive the redesign.

## In-flight working-tree state (uncommitted, from the 2026-06-04 session)

- **Customer Review services** (`Concert/Artist/VenueReviewService.CanCurrentUserReviewAsync` ×3):
  changed to return `false` when `!currentUser.IsAuthenticated` instead of throwing → 401, plus a new
  integration test `GetConcertReviewEligibility_ShouldReturn200False_WhenUnauthenticated`.
  **PENDING decision, NOT yet test-run.** Rationale if kept: a question endpoint giving the truthful
  answer. Resolve as a consequence of the design, or revert.
- **`web/shared/src/lib/customerAxios.ts`**: bare `Promise.reject` (the `44dc77c9` state). Restoring
  the 401 handler is PENDING until manager apps make zero customerApi calls — then its only possible
  effect is "customer site clears its own stale session", which matches the separate-sites model.
- Everything else uncommitted belongs to the big-review fixes — tracker:
  `reviews/BIG-refactor-Microservices-Review.md` (BUG25 entry has the full saga).

## Investigation tasks

1. **Full inventory** of `app/shared/`, `app/web/shared/`, `app/mobile/shared/`: every API-call
   surface (which backend, authenticated?, which sites legitimately need it), every route literal,
   every identity assumption (`role`, `$type`, `isAuthenticated` used for branching).
2. **Map features → apps** via each app's route tree (web: customer/venue/artist/business; mobile:
   customer/business) — what does each app actually render from shared?
3. **Classify** each shared item: genuinely shared / customer-only / manager-only / mixed (needs
   splitting).
4. **Design the split**: what moves into `app/web/customer` (+ mobile equivalents), what stays
   shared, and a *consistent composition mechanism* for shared surfaces that need app-specific
   augmentation (e.g. `VenueDetails` is shared, but "add review" is customer-only — slot/children/
   prop injection; pick one pattern). No hardcoded role checks in shared.
5. **Resolve the pending decisions** (eligibility 200-false vs 401; customerAxios 401 handler;
   mobile customerAxios parity) as consequences of the design.
6. **NOTE9's 25 TS errors** fall out of the same split (route literals belong to owning apps) —
   fix them as part of it, restoring `app/web/CLAUDE.md`'s build-verification convention.

## Constraints

- Per `app/web/CLAUDE.md`: verify with all four web builds (business app is `vite build` only).
- Mobile: `tsc --noEmit` per workspace (`mobile/business` has 2 pre-existing `RootNavigator.tsx`
  errors — broken `shared/features/auth` alias — not yours unless in scope).
- Plan first: produce the plan as a file and present it for approval before implementing
  (user decides scope/stages). Expect this to be staged work, not one pass.

## Deliverable for the next session

A written plan (inventory → classification → target structure → migration stages), approved by the
user, then staged implementation.
