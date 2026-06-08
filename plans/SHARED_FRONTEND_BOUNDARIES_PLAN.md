# Shared frontend boundaries — refactor plan

**Brief:** [`SHARED_FRONTEND_BOUNDARIES.md`](./SHARED_FRONTEND_BOUNDARIES.md). Investigation completed 2026-06-04.
**Principle:** only genuinely shareable code in shared trees; auth-coupled / site-specific code lives in
the owning app, composed into shared surfaces explicitly. No role checks in shared.

---

## 1. Findings (corrections + confirmations to the brief)

### Layering (verified)

```
@concertable/shared (app/shared/src)          platform-agnostic: axios instances + configure fns,
  ▲                          ▲                API modules, hooks, stores, types. Web AND mobile.
  │                          │
app/web/shared/src        app/mobile/shared/src
  web components/pages,      screens, navigation, axios wrappers
  own per-feature api/       (all four clients have 401→clear-session)
  OIDC axios wrappers
  ▲                          ▲
4 web SPAs                 2 mobile apps
(@/ aliases → web/shared)  (shared/* alias → mobile/shared)
```

### The good news (things already right)

- **`api` (base client) already means "own site's backend"**: customer app's `VITE_API_URL` →
  7090 = Customer service; manager apps' → B2B. So `web/shared` review *reads*
  (`reviewApi.getReviews/getReviewSummary` → `api`) are already correctly designed — B2B projections
  serve managers, Customer service serves customers. Only `canReview`/`createReview` use `customerApi`.
- **`app/shared`'s all-customerApi `reviewApi` is dead code** — sole consumer is a types re-export
  (`web/shared/src/features/reviews/types.ts`). Mobile consumes no reviews feature (verified).
- **`ReviewSection` itself is read-only**; the disease is `useReviews` bundling `useCanReviewQuery`,
  plus `AddReview`/`useAddReview` (customerApi) rendered inside shared `ConcertDetails`.
  `AddReview` renders in **`ConcertDetails` only** — Artist/VenueDetails render only read-only
  `ReviewSection`/`ReviewSummaryBadge`.
- **`ConcertDetailsPage` is routed by the customer app only** (managers route `MyConcertPage`).
  Artist/VenueDetailsPage are routed by customer + one manager app each, but after the reviews split
  they contain nothing customer-specific except `OpportunitySection` actions (stage 3).
- **`web/shared` ticketApi is a pure re-export** of `@concertable/shared` ticketApi (all customerApi);
  the real module is consumed by mobile ticket screens too.
- **Composition precedent exists**: `AppLayout({ links })` — apps already inject nav links as data.
  `ProfileMenu` (hardcoded role branches + customer/manager route literals inside Navbar) is the outlier.
- **Web business app** imports `shared/index.css` only — effectively zero shared coupling.

### The bad news (worse than the brief)

- **NOTE9 understated: customer, venue, AND artist builds all fail `tsc -b`** (~23–24 errors each;
  each app's build typechecks `web/shared` against its own TanStack route tree, so each fails on the
  *other* apps' literals). Business passes only because it's vite-only. Error taxonomy (union of all
  three builds):
  | Category | Errors | Fixed by |
  |---|---|---|
  | App-specific route literals in shared (ProfileMenu, ConcertCard, ApplicationCard, OpportunityCard, useNotifications, TicketsPage, artists/venues guards, SearchBar/NavbarSearch find-routes) | ~13–15 per app | Stages 2–3 |
  | `ImageFile` (RN `uri` type) vs browser `File` — `app/shared` artist/venue stores leak RN types into web (AvatarUpload, BannerUpload, ArtistDetails, VenueDetails) | 6 | Stage 1 |
  | `PaymentMethod` ambiguous re-export in `app/shared/src/index.ts` (contracts vs payments) | 1 | Stage 1 |
  | `react-dom/client` missing types (`src/main.tsx` in all three apps) | 1 | Stage 1 |
  | `StripePaymentForm` union narrowing (`setupIntent`/`paymentIntent`) | 2 | Stage 1 |
  | `OpportunitySection` → `OpportunityCard` missing required edit props (genuine prop-contract break) | 1 | Stage 3 |
- **Even "universal" search nav isn't universal**: `SearchBar` → `/find/$headerType/$id`,
  `NavbarSearch` → `/find` fail in venue/artist builds (venue registers only `/find/artist/$id`, etc.).
- **Mobile business app**: `RootNavigator.tsx` imports `shared/features/auth` which doesn't exist in
  `mobile/shared` (the 2 known TS errors); should be `@concertable/shared/features/auth`.
- **Mobile latent manager-logout bug confirmed**: all four `mobile/shared` clients (incl. customerApi)
  have identical 401→`tokenStorage.clear()` + `setUser(null)` handlers. Business app currently has no
  customerApi code path (tickets/reviews are customer-tab-only), so it's latent.
- **Mobile business exposes `PreferencesScreen`** (customer-domain) via the shared ProfileStack in
  Artist/Venue tabs.

### Identity branching inventory (web/shared)

| Site | Check | Fate |
|---|---|---|
| `components/ProfileMenu.tsx` | `role === "Customer"/"Admin"`, `isArtistManager`, `isVenueManager`, customer-view/manager-view switcher | Stage 3: app-injected menu |
| `hooks/useNavSection.ts` + `auth/hooks/useRouteRole.ts` | pathname prefix → role | Stage 3: delete (apps know what site they are) |
| `concerts/components/opportunities/OpportunityCard.tsx` | `role === "ArtistManager"`, `isVenueManager(user) && user.venueId === …` | Stage 3: presentational card + app-injected actions |
| `payments/pages/PaymentPage.tsx` | `role === "VenueManager" \|\| role === "ArtistManager"` payout section | Stage 3: payout slot from manager apps |
| `concerts/hooks/useApply.ts` | `enabled: isArtistManager` | Stage 3: move to artist app |
| `auth/guards.ts` `requireRole(role)` | parameterized — app declares its role | Keep (app passes the role) |
| `auth/types.ts` `isVenueManager/isArtistManager` | type guards | Keep only if data-ownership checks remain; review at end of stage 3 |

---

## 2. Target boundary rules

1. **Backend rule** — shared code may make authenticated calls only to backends every consuming site
   can call with its own token: own-app `api`, `searchAxios`, `paymentAxios`. Calls only one site's
   token can make (`customerApi`) live in that app (or the customer-shared core, see D2).
2. **Identity rule** — no role/identity branching in shared. Apps own identity-conditional
   composition; shared components receive variation via props/slots (the `AppLayout({ links })`
   pattern). `isAuthenticated`-gating inside *app-owned* code is fine.
3. **Route rule** — shared code may reference only the universal route contract (routes registered by
   every app that compiles it): `/`, `/login`, `/register`, `/auth/callback`, `/success`, `/fail`,
   `/stripe-refresh`, `/stripe-return`, `/settings`, `/settings/payment` (+ `/find` variants —
   resolve in stage 3). Everything else is injected by the owning app.
   *Enforcement is the type system*: each app's `tsc -b` compiles `web/shared` against its route tree
   — an app-specific literal in shared fails some app's build. This is why all four builds green is
   the standing gate (restore `app/web/CLAUDE.md` convention).
4. **Platform rule** — `@concertable/shared` must be platform-agnostic: no RN-specific types
   (`ImageFile`) or web-specific types in cross-platform surfaces.

**Composition mechanism (chosen): slot/prop injection.** Shared components expose `ReactNode` slots
or callback props for app-specific affordances; apps compose at their route/page level. Matches the
existing `AppLayout({ links })` precedent. (Rejected: app-config context provider — becomes a
capability kitchen-sink and re-centralizes identity knowledge.)

---

## 3. Classification summary

| Item (web/shared unless noted) | Class | Target |
|---|---|---|
| reviews: `getReviews`/`getReviewSummary`, `ReviewSection`, `ReviewSummaryBadge` | shared (reads via own `api`) | stay |
| reviews: `canReview`/`createReview`, `useCanReviewQuery`, `useAddReview`, `AddReview` | customer-only | → customer app |
| tickets: api + hooks + Tickets/Upcoming/History pages + `TicketCard` + checkout CTA | customer-only | → customer app / customer core (D2) |
| `ConcertDetailsPage` | customer-only (sole router) | → customer app |
| `ConcertDetails`, `ArtistDetails`, `VenueDetails`, `MyConcertPage`, Artist/VenueDetailsPage | shared | stay; gain slots |
| `lib/customerAxios.ts` (web OIDC wrapper) | customer-only | → customer app; restore 401→`removeUser` |
| `ProfileMenu` role branches + customer/manager literals | mixed | shell stays; items app-injected |
| `useNavSection`, `useRouteRole`, customer↔manager view switcher | legacy (pre-split single-site) | delete; cross-site links app-injected as plain hrefs |
| `ApplicationCard` | venue-only | → venue app |
| `OpportunityCard` role branches + nav | mixed | presentational card stays; actions injected (venue: manage/view-applications; artist: apply) |
| `useNotifications` 3 role hooks | app-specific | hooks → owning apps; SignalR glue + payload types stay |
| `artists/guards.ts`, `venues/guards.ts` (`/create` redirects) | app-specific | → artist / venue apps |
| `PaymentPage` payout section | manager-only | payment-method mgmt stays; payout slot injected by venue/artist |
| `useApply` | artist-only | → artist app |
| applicationApi (apply-half / accept-half, all via own `api`) | no auth disease | stays; optional split (LOW) |
| search, messaging, user profile basics, auth infra, UI components | shared | stay |
| `app/shared` reviews api/hooks | dead | delete (keep types where consumed) |
| `app/shared` ticketApi + ticket hooks | customer-only, cross-platform (web+mobile customer) | D2 |
| `app/shared` preferenceApi + hooks | customer-domain (currently reachable in business apps) | D2 + stage 4 |
| mobile/shared customer screens (Tickets*, TicketCheckout, CheckoutSuccess, Preferences) | customer-only | → mobile/customer (stage 4 / D3) |

---

## 4. Migration stages

Each stage is independently committable. **Gate for every stage:** four web builds
(`npm -w @concertable/web-{customer,venue,artist,business} run build`) + `tsc --noEmit` in both mobile
workspaces; error count must only shrink, with remaining errors attributable to later stages.

### Stage 1 — Mechanical build fixes (no boundary changes)

- [x] `app/shared/src/index.ts`: explicit re-export to resolve `PaymentMethod` ambiguity.
- [x] Add `@types/react-dom` (root `app/package.json` devDeps) — fixes `src/main.tsx` in 3 apps.
- [x] `StripePaymentForm.tsx`: narrow the Stripe result union (`'setupIntent' in result`).
- [x] `ImageFile` vs `File`: resolved web-side — `useImageUpload` already produces `File & { uri }`
  (object URL), which satisfies `ImageFile = { uri; name; type }` as the platform-agnostic structural
  contract; `app/shared` stores were already correct (they need `uri` for preview). Aligned the web
  component chain (`Hero`, `AvatarUpload`, `BannerUpload`) on `ImageFile` instead of browser `File`.
- Gate ✅ 2026-06-04: customer 15 / venue 16 / artist 17 errors — all route-literal +
  OpportunitySection; business green; mobile-customer 0; mobile-business only the 2 known
  RootNavigator errors.

### Stage 2 — Customer-auth ownership (reviews, tickets, customerAxios)

- [x] `web/shared` reviews: `useReviews` drops `useCanReviewQuery`; `reviewApi` keeps reads only.
- [x] Move to `app/web/customer/src/features/reviews/`: `AddReview`, `useAddReview`,
  `useCanReviewQuery`, new `customerReviewApi` (eligibility + create via customerApi).
- [x] `ConcertDetails` gains `addReviewSlot?: ReactNode` + `onBuyTickets?: () => void` —
  **user decision:** ConcertCard keeps its Buy Tickets button declared in shared (manager preview =
  see what the customer sees), `disabled={!onBuyTickets}`; only the navigation behaviour is
  app-injected. Kills the `/concert/checkout/$id` literal. Rule of thumb (in
  `app/web/shared/CLAUDE.md`, written this stage): behaviour only one site can execute → injected;
  affordance every viewer sees → declared in shared, dumb-dispatched.
- [x] Move `ConcertDetailsPage` → `app/web/customer/src/features/concerts/pages/`; it passes the
  customer slots. Manager `MyConcertPage` stays shared, passes none.
- [x] Move ticket surface → customer app: Tickets/Upcoming/History pages, `TicketCard` + `QrPopover`,
  ticket hooks imported from `@concertable/shared` directly; deleted `web/shared` ticketApi +
  useTicketsQuery re-exports. Kills `TicketsPage`'s `/profile/tickets/upcoming` literal.
- [x] Move `web/shared/src/lib/customerAxios.ts` → `app/web/customer/src/lib/`; restored the
  401→`userManager.removeUser()` handler (exact pre-44dc77c9 shape from git history).
- [x] D1 resolved: 200-false kept; Review integration tests **8/8 passed** 2026-06-04.
- Gates ✅ 2026-06-04: zero `customerApi`/`customerAxios` refs under `web/shared/src`; builds
  customer 14 / venue 13 / artist 14 errors — all Stage 3 categories (ProfileMenu, OpportunityCard/
  Section, ApplicationCard, useNotifications, guards, SearchBar/NavbarSearch); business + both mobile
  unchanged.

### Stage 3 — Route literals + identity branching out of shared (+ B2B web tier, D5)

- [x] **`app/web/b2b/shared` tier created (D5, user decision mid-stage)**: opportunities, contracts,
  applications, manager payouts are B2B concepts a customer must never see; with only `web/shared`
  as a meeting point they had defaulted into the all-sites bucket (customers could browse venue
  opportunities and open contract details). `@b2b/*` alias exists only in venue/artist tsconfig+vite
  — an import from customer code is a module-resolution error. Rules: `app/web/b2b/shared/CLAUDE.md`.
  Moved in: opportunities components/hooks/stores, `features/contracts`, application checkout hooks
  + applicationApi/opportunityApi re-exports + `acceptCheckoutFormat`, `AcceptContractSummary`,
  `MyConcertPage` (+useMyConcert/useConcertStore), payout surface (`StripeOnboardingBanner`,
  `useStripeAccount`, payout queries, new `PayoutAccountSection`).
- [x] Single-app code → owning apps: `useMyVenue`→venue, `useMyArtist`→artist, `useApply`→artist
  (role gate dropped, auth gate kept), `ApplicationCard`→venue, `requireArtist`/`requireVenue`
  guards → owning apps, notification hooks → owning apps (SignalR glue + payload types stay shared).
- [x] `ProfileMenu`: shared shell (login/register, email header, Settings + Payment/Billing, logout)
  + injected `ProfileMenuItem[]` through `AppLayout`/`Navbar`. View-switcher, `useNavSection`,
  `useRouteRole` deleted. **User decision: NO cross-site customer link from manager apps — they are
  separate sites; you don't navigate between them** (original "plain href" idea dropped).
- [x] `VenueDetails`/`ArtistDetails` dissolved (user decision): shared owns the pieces
  (`DetailsLayout`, generic `AboutSection`/`LocationSection`, `VenueHero`/`ArtistHero`,
  `VenueConcerts`/`ArtistConcerts`) plus per-entity section builders (`venueSections`/
  `artistSections` — pure functions returning named `DetailsSection`s; pages destructure and
  assemble their own order). Venue pages are per-app because the opportunities section genuinely
  differs (venue=manage/View Applications, artist=Apply, customer=**none — invisible on the
  customer site**); `ArtistDetailsPage` stays shared (identical everywhere after deleting the dead
  opportunities placeholder). `FEATURES.md` added to venue+artist apps (location-editing gap etc.).
- [x] `/find` contract universal: venue adds `find/venue/$id`+`find/concert/$id`, artist adds
  `find/artist/$id`+`find/concert/$id`; shared `ConcertDetailsPage` reintroduced with slots
  (customer passes AddReview + checkout nav; managers render it bare — buy CTA disabled).
  `NavbarSearch` now renders on every site.
- [x] Manager settings routes added (venue+artist `/settings`, `/settings/payment`), shared
  `SettingsLayout` extracted; `PaymentPage` keeps payment-method management only + `payoutSlot`
  (manager payment routes pass `PayoutAccountSection` from `@b2b`); role literals gone.
- [x] Dead identity surface deleted: `useRole` + `isVenueManager`/`isArtistManager` web re-exports
  (zero consumers anywhere, incl. mobile).
- [x] Docs: `app/web/b2b/shared/CLAUDE.md` written; `app/web/shared/CLAUDE.md` (+b2b rule, /find in
  contract) and `app/web/CLAUDE.md` (three tiers, four-green gate) updated.
- [x] Gate ✅ 2026-06-04: **all four web builds green** (NOTE9 fully resolved — first time);
  mobile-customer 0 errors, mobile-business only the 2 known RootNavigator errors (Stage 4). Greps:
  zero `role ===`/`isVenueManager`/`isArtistManager` under `web/shared/src` + `web/b2b/shared/src`
  (type-guard re-exports deleted too — no consumers); every route literal in the shared trees is in
  the universal contract (`requireRole`/`requireBusinessRole` stay parameterized).

### Stage 4 — Mobile parity (scope per D3) — ✅ 2026-06-05

- [x] Fix `mobile/business/src/navigation/RootNavigator.tsx` import →
  `@concertable/shared/features/auth` (killed the 2 known TS errors).
- [x] `@customer/shared` package created per D2 (see updated D2 for contents + naming).
- [x] web-customer repointed to `@customer/shared`; `web/shared` purged of `features/customer`
  re-exports + `TicketPurchasedPayload`/`Ticket*` type re-exports; `app/shared` lost
  customerAxiosClient, ticket surface, preferences feature, TicketPurchased payload/handler and the
  dead reviews api (reviews feature is types-only now; `Checkout`-shared `CheckoutSession`/
  `CheckoutLabels` stayed).
- [x] Customer-only mobile code → `mobile/customer/src`: the 4 ticket/checkout screens +
  CheckoutAwaiting, PreferencesScreen, HomeScreen/SearchScreen/SearchFilterSheet (they hard-reference
  customer tab routes — one consumer), CustomerTabs/HomeStack/SearchStack/TicketsStack/ProfileStack +
  customer nav param lists, `useCustomerNotifications`, `lib/customerAxios` (bootstrapped from
  customer `App.tsx`; `useAuthInit`/`useLogin` dropped the side-effect import — the business app
  never registers the customer client, killing the latent manager-logout bug).
- [x] Shared mobile contract mirrors web: `ConcertNavParamList` shrunk to the browse trio
  (`ConcertDetail`/`ArtistDetail`/`VenueDetail`); `ConcertDetails` gained `onBuyTickets?` (CTA
  declared in shared, disabled without a handler — the web ConcertCard ruling); customer registers
  `CustomerConcertDetailScreen` wrapper that injects the checkout navigation.
- [x] `PreferencesScreen` → customer app; `ProfileScreen` gained `accountItems` injection (web
  ProfileMenu pattern) — customer wrapper passes the Preferences row, business renders it bare;
  ProfileStack forked per app. Legacy role-switching `mobile/shared` RootNavigator deleted.
- [x] Manager-only mobile code → `mobile/business/src`: ArtistTabs/VenueTabs/MyArtistStack/
  MyVenueStack + their param lists, MyArtistScreen/MyVenueScreen, PlaceholderScreen, 3-screen
  ProfileStack.
- [x] mobile customerAxios 401 handler kept, customer-app-only — documented in
  `app/customer/shared/CLAUDE.md`.
- [x] **Mobile customer/b2b mirror (per D5)**: `mobile/business` → `mobile/b2b/business` as a
  move-only step (workspaces glob + lockfile, metro watchFolders/global.css, tsconfig paths/include,
  tailwind content, App.tsx css import; AppHost untouched — only the customer mobile surface is
  wired there). No `mobile/b2b/shared` tier: one manager app, manager-only code lives in it.
- Gate ✅ 2026-06-05: `tsc --noEmit` 0 errors in BOTH mobile workspaces; all four web builds green;
  greps clean (no `@customer/shared` imports under any shared tree, no customer refs left in
  `mobile/shared` or `app/shared`).

### Out of scope (noted, not done here)

- Backend preference-module duplication (B2B `Modules/Customer` vs Customer service both serve
  `/preference`) — pre-existing, separate effort.
- `useSyncUser` normalization (customer fork hits customerApi `/user/me`; shared one hits own `api`) —
  both are correct post-split; merge opportunity only.
- applicationApi apply/accept split (no auth disease; cosmetic ownership polish).

---

## 5. Decisions (resolved with user 2026-06-04)

- **D1 — Review eligibility endpoint**: **keep 200-false**. The in-flight changes to the 3 Review
  services + new integration test stay; run the Review integration tests in stage 2. Customer app
  still gates the query on its own auth state.
- **D2 — Cross-platform customer-only core**: **new workspace package** at `app/customer/shared`
  (npm name `@customer/shared` — user decision 2026-06-05: nested `customer/shared` folder like every
  other shared tier, never hyphenated; package scope mirrors the web `@b2b` precedent) — customer
  axios client + configure fn, ticketApi+hooks+types, review eligibility/create api (hooks stayed in
  web/customer — one consumer owns its auth gate), preferenceApi+hooks+types, ticket-purchased
  payload + handler hook. Consumed only by web-customer and mobile-customer.
- **D3 — Mobile scope**: **full stage 4** (screen moves included).
- **D4 — Branch placement**: stages land as commits on **`refactor/Microservices`**.
- **D5 — B2B web tier (2026-06-04)**: `app/web/b2b/shared` (`@b2b/*`, manager apps only) for code
  both manager apps share and customers must never see. Follow-up ✅ DONE 2026-06-04: relocated
  `venue/`, `artist/`, `business/` under `app/web/b2b/` as a separate move-only commit
  (workspaces globs + lockfile, tsconfig/vite alias depths, `envDir`, `index.css` `@source` paths
  — which also gained the missing `b2b/shared/src` entry — and `AddSpaSurface` tier segment in
  AppHost.Shared; all four builds re-verified green). Mirror the customer/b2b split in
  `app/mobile` as part of Stage 4.
- **D6 — No cross-site navigation (2026-06-04)**: customer and B2B sites get no links to each
  other's apps (manager profile menus carry no "customer site" href; the in-app view-switcher is
  deleted). The customer app's "For Artists & Venues" marketing-site link is unaffected.
