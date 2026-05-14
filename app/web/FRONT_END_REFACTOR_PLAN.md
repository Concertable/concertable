# SPA Feature-Based Refactor Plan

## Current pain

`app/src/` is organized by **layer** (api/, hooks/, components/, pages/, types/, store/, schemas/). Adding or touching a feature means editing ~5 sibling folders, and sibling files in those folders are unrelated to each other. ~270 files, 16 API clients, 39 hooks — the layer split is showing its limits.

## Target shape

```
src/
├── components/          ← shared UI only (Shadcn ui/* + truly cross-feature primitives)
├── features/            ← where 90% of the app lives — one folder per feature
├── hooks/               ← only truly global hooks (useDebounce, useMediaQuery, useMountEffect)
├── lib/                 ← third-party config (axios, queryClient, oidc, signalr, stripe, maps)
├── routes/              ← TanStack file-based routes — STAYS (this IS the "thin pages" layer)
├── types/               ← only shared global types (Pagination<T>, Genre, Role, Location)
├── utils/               ← pure helpers used everywhere
├── App.tsx
├── main.tsx
└── routeTree.gen.ts
```

> **Note on `pages/` vs `routes/`:** The proposed `pages/` slot in your sketch is already filled by TanStack's `routes/` (file-based routing). Don't introduce a second `pages/`; keep `routes/` and let route files import from features. The current `pages/*` directory dissolves into features.

### Inside a feature

Every feature is self-contained:

```
features/<feature>/
├── api/                 ← axios calls (was api/<feature>Api.ts)
├── hooks/               ← query hooks + composite domain hooks
├── components/          ← feature-specific UI
├── store/               ← Zustand slices for this feature (if any)
├── schemas/             ← Zod schemas (if any)
├── types.ts             ← feature-local types
└── index.ts             ← public surface (re-exports the things consumers need)
```

Not every feature needs every subfolder — drop the empty ones.

## Proposed features

Based on inventory, 14 features map cleanly:

| Feature | Owns | Notes |
|---|---|---|
| `auth` | login/register/callback pages, `useAuthStore`, `useRole`, `useRouteRole`, `useSyncUser`, `userApi` | OIDC config stays in `lib/` (third-party) |
| `artists` | `artistApi`, artist hooks, store, types, ArtistDetails, MyArtistPage, CreateArtistPage, ArtistDashboard | |
| `venues` | `venueApi`, venue hooks, store, types, VenueDetails, MyVenuePage, CreateVenuePage, VenueDashboard, VenueLocation | |
| `concerts` | `concertApi`, concert hooks, store, types, ConcertDetails, ConcertPage (artist + venue) | |
| `opportunities` | `opportunityApi`, OpportunityCard, OpportunitySection, ContractDetails, ContractSummaryLabel | Venue posts gigs, artists browse |
| `applications` | `applicationApi`, useApply, ApplicationCard, AcceptContractSummary, ApplicationsPage, AcceptApplicationPage | Artist applies → venue accepts |
| `tickets` | `ticketApi`, useTicketsQuery, TicketCard, QrPopover, TicketsPage, UpcomingTicketsPage, TicketHistoryPage | |
| `checkout` | `checkout/*` (13 components), `useTicketCheckout`, ConcertCheckoutPage, ApplicationCheckoutPage, checkout types | Shared by ticket + application flows |
| `search` | `headerApi`, `autocompleteApi`, `genreApi`, search hooks/store/schema, SearchBar, SearchResults, FindPage, AutocompleteDropdown, FilterSlider, DateRangePicker, searchSerializer, **headers/** (all carousels + cards) | Headers are search-result UI — they belong here |
| `messaging` | `messageApi`, useMessageQuery, useMailbox, Mailbox component, message types | |
| `notifications` | `useNotifications`, notification types | SignalR hookup; signalr lib stays in `lib/` |
| `reviews` | `reviewApi`, review hooks, ReviewSection, AddReview, ReviewSummaryBadge | |
| `preferences` | `preferenceApi`, usePreferenceQuery, PreferencesPage, CreatePreferencePage | |
| `payments` | `stripeAccountApi`, useStripeAccount, StripeOnboardingBanner, AddPaymentMethodModal, NewCardSection, StripeRefreshPage, StripeReturnPage, SuccessPage, FailPage, acceptCheckoutFormat | Stripe.js init stays in `lib/` |
| `profile` | ProfilePage, LocationPage, PaymentPage, SettingsPage, LocationPicker, GoogleMap, AvatarUpload, BannerUpload, useImageUpload | "Things logged-in users edit about themselves" |

## What stays at top level

### `components/` — shared UI only
- `ui/*` — entire Shadcn library (button, dialog, sheet, etc.) — **keep flat, don't move**
- `AppLayout`, `Navbar`, `NavbarSearch`, `ProfileMenu`, `Footer`, `Breadcrumbs`, `ConfigBar`, `Hero` — global layout chrome
- `ThemeToggle`, `IconButton`, `Select` — tiny generic primitives
- `editable/*` — used by 3+ features (artists/venues/concerts inline edit) → shared
- `skeletons/*` — used everywhere → shared
- `ScrollspyNav` — generic
- `NavbarHeightContext` (currently in `context/`) → moves here as it's layout infrastructure

### `hooks/` — only truly global
Keep: `useDebounce`, `use-mobile`, `useMountEffect`, `useMountLayoutEffect`, `usePagination`, `useImageUrl`, `useBannerTextColor`, `useNavSection`

Everything else moves into a feature.

### `lib/` — third-party config (largely unchanged)
`axios.tsx`, `oidcConfig.ts`, `signalr.ts`, `stripe.ts`, `queryRetry.ts`, `guards.ts`, `utils.ts`

### `types/` — shared global types only
Keep: `common.ts` (Pagination, Genre), `location.ts`, `auth.ts` (Role enum), `ui.ts`, `paymentTiming.ts`

Everything else moves into the owning feature's `types.ts`.

### `utils/`
Currently only has `serializers/searchSerializer.ts` — that moves into `features/search/`. After the move, `utils/` is empty; delete it or keep as a parking spot.

### `routes/` and `routeTree.gen.ts`
**Untouched.** TanStack file-based routing requires the directory layout. Route files just become thinner — they import page components from features instead of `pages/`.

### `providers/` and `context/`
- `ThemeProvider` → `components/` (or stay where it is — small enough)
- `DetailsFormProvider`, `EditableProvider` → `components/editable/` (only used there)
- `NavbarHeightContext` → `components/` (layout infra)
- After this, `context/` and `providers/` are gone.

### `pages/`
**Deleted.** Each page moves into its owning feature's `components/` (or `pages/` subfolder if you want the distinction inside a feature). Routes import from feature.

### `assets/`
Stays — Vite convention.

### `schemas/`
Currently only `searchSchema.ts` — moves to `features/search/`. Folder gone.

### `store/`
All 5 stores move into their features. Folder gone.

### `api/`
All 16 clients move into their features. Folder gone.

## Cross-feature dependencies (worth flagging)

These are real and unavoidable — the goal is to keep them one-way and explicit through `index.ts` re-exports:

- `concerts` → `artists`, `venues` (a concert references both)
- `opportunities` → `venues`, `concerts`, `contracts` (gigs are at a venue, for a concert, with contract terms)
- `applications` → `opportunities`, `artists`, `checkout` (artists apply to opportunities, accept triggers checkout)
- `tickets` → `concerts`, `checkout`
- `checkout` → `payments` (stripe), `tickets`, `applications` (consumers)
- `search` → `artists`, `venues`, `concerts` (results aggregate all three) — but only **types**, not full feature surface
- `reviews` → `artists`, `venues`, `concerts` (subjects of reviews)
- `notifications` → `concerts`, `tickets`, `applications` (event payload types)

Rule: import from `features/x` (the index), never `features/x/internal/file`. Use `index.ts` to gatekeep what's public per feature.

**Contract types** (`contract.ts`) are a small shared shape — either give them their own micro-feature `features/contracts/` (just types + ContractDetails component), or park them in `features/opportunities/` since that's the primary consumer. **Recommendation: own folder**, since applications also uses them.

## Migration order

To minimize broken builds, do features in dependency order (leaves first):

1. **Setup** — create `features/` folder, add path alias `@/features/*` in `tsconfig.app.json` and `vite.config.ts`
2. **Leaf features (no inbound deps from other features):**
   - `messaging`, `reviews`, `preferences`, `notifications`, `profile`, `payments`
3. **Mid-tier:**
   - `artists`, `venues`, `contracts`
4. **Composers:**
   - `concerts` (depends on artists/venues)
   - `opportunities` (depends on venues/concerts/contracts)
   - `tickets` (depends on concerts)
   - `checkout` (depends on payments)
   - `applications` (depends on opportunities/checkout)
5. **Cross-cutting:**
   - `search` (depends on artists/venues/concerts) + absorbs `headers/`
   - `auth` (touches everything; do last)
6. **Cleanup:**
   - Delete empty `api/`, `pages/`, `store/`, `schemas/`, `context/`, `providers/`, `utils/`
   - Trim `components/`, `hooks/`, `types/` to the shared-only set above
   - Update barrel exports

Each feature is one PR-sized chunk. Build + manually click through that feature's pages between moves.

## Open questions before we start

1. **`features/<x>/index.ts` discipline** — strict (every cross-feature import goes through index, enforce with ESLint `no-restricted-imports`) or loose (index for convenience, free imports allowed)? Stricter is better long-term but adds friction.
2. **`features/<x>/pages/` subfolder** — do you want pages segregated inside the feature, or flat alongside other components? (I'd suggest flat — page is just a component.)
3. **`contracts` feature** — own folder or fold into `opportunities`?
4. **Customer/Artist/Venue role split** — three role-flavored variants of the same feature exist (e.g. customer-side `find` vs artist-side `FindVenuePage` vs venue-side `FindArtistPage`). Group by feature (one `search` feature, three pages inside) or by role at the top? **Recommendation: by feature** — the API and hooks are shared; only the page wrapper differs.
5. **Aliases** — add `@/features/auth` etc. as named aliases, or rely on `@/features/auth` via the existing `@/*` alias? Named is more verbose to set up but makes refactors easier.

## What this buys us

- Touching a feature touches one folder, not 5–7
- New devs find feature code by name, not by guessing layer
- Deleting a feature is `rm -rf features/x` + clean up route imports
- `index.ts` boundaries make it harder for features to grow tentacles into each other's internals
- `components/` and `hooks/` shrink to genuinely shared things, easier to navigate

---

## Backend cross-check

The API is a modular monolith with 11 modules: **Artist, Authorization, Concert, Contract, Customer, Messaging, Notification, Payment, Search, User, Venue**.

### Where frontend features map 1:1

| SPA feature | Backend module | Notes |
|---|---|---|
| `artists` | `Artist` | ✅ |
| `venues` | `Venue` | ✅ |
| `concerts` | `Concert` | ✅ |
| `contracts` | `Contract` | ✅ — confirms own-folder recommendation; backend also makes it a top-level module |
| `payments` | `Payment` | ✅ |
| `messaging` | `Messaging` | ✅ |
| `notifications` | `Notification` | ✅ |
| `search` | `Search` | ✅ |
| `preferences` | `Customer` | Backend's Customer module owns Preferences (per memory). Consider renaming SPA's `preferences` → `customer`, or keep `preferences` since that's the only customer-shaped surface in the UI |

### Where backend differs — implications for the plan

**1. Backend has Customer + User as separate modules** — frontend currently mashes them together as "profile". Worth splitting:
- `features/user` — "me" data (`getMe`, `updateLocation`, `userApi`) — mirrors backend `User` module
- `features/customer` — preferences, customer-specific shapes (renamed from `preferences`)
- `features/profile` collapses; ProfilePage / SettingsPage / LocationPage move into `user`

**2. Backend has Authorization as its own module** — separate from User/Customer. Frontend mirror:
- `features/auth` — login/register/callback, OIDC, role guards, `useRole`, `useAuthStore` (matches backend `Authorization`)
- `features/user` — "me" profile data (matches backend `User`)
- This is a **clean split worth making** — auth concerns (token, roles, login UI) live independently from user-data concerns (location, settings, payment methods).

**3. Backend keeps `opportunities`, `applications`, `tickets` INSIDE the Concert module** — they're concert lifecycle stages on the backend (`IConcertModule`, `IConcertLifecycleModule`, etc.). **Decision (revised): mirror the backend — fold them into `features/concerts/`.** Reasons:
- The cross-feature deps I flagged (opportunities→concerts, applications→opportunities, tickets→concerts) all evaporate when they're siblings inside one feature
- These aren't independent domains, they're stages of a concert's lifecycle. Splitting them creates artificial boundaries
- Backend already proved the cohesion model works — frontend gains the same benefits (one mental model, one folder for "everything concert-flow")
- `checkout/` similarly folds in — it only orchestrates ticket purchase + application acceptance, both concert lifecycle stages

Structure inside `features/concerts/`:
```
features/concerts/
├── api/                ← concertApi, opportunityApi, applicationApi, ticketApi
├── components/
│   ├── (concert-level: ConcertDetails, ConcertCard, ConcertPage)
│   ├── opportunities/  ← OpportunityCard, OpportunitySection
│   ├── applications/   ← ApplicationCard, AcceptContractSummary, ApplicationsPage
│   ├── tickets/        ← TicketCard, QrPopover, TicketsPage, UpcomingTicketsPage
│   └── checkout/       ← all 13 checkout components, ConcertCheckoutPage, ApplicationCheckoutPage
├── hooks/              ← useConcertQuery, useOpportunityQuery, useApplicationQuery, useTicketsQuery, useApply, useTicketCheckout, useMyConcert
├── store/              ← useConcertStore
├── types.ts            ← concert, opportunity, application, ticket, checkoutSession, ticketCheckout, acceptCheckout
└── index.ts
```

What stays separate:
- **`features/contracts/`** — backend keeps Contract as its own module; frontend mirrors. Contracts are referenced by concerts but are a distinct domain object.
- **`features/payments/`** — Stripe integration (account onboarding, payment methods, success/fail pages). Checkout calls into it, but Payment is its own backend module too.

**4. No `Reviews` backend module** — reviews are split across Artist + Venue + Search backend modules (per memory: "Search.Infrastructure refs Artist.Infrastructure + Venue.Infrastructure for rating configs"). On the frontend, reviews are cohesive UI (ReviewSection, AddReview, ReviewSummaryBadge) — keep as a single `features/reviews/` even though backend doesn't. Don't over-mirror.

**5. No `Profile` or `Checkout` backend module** — these are pure UI concerns:
- `features/checkout` is a UI orchestration feature on top of `features/payments` (which mirrors backend `Payment`). Keep it.
- `features/profile` collapses into `features/user` per point 1.

### Revised feature list (post-cross-check) — 12 features

```
auth          ← Authorization (backend)
user          ← User (backend) — replaces 'profile'
customer      ← Customer (backend) — replaces 'preferences'
artists       ← Artist
venues        ← Venue
concerts      ← Concert — absorbs opportunities, applications, tickets, checkout (lifecycle stages)
contracts     ← Contract
payments      ← Payment
messaging     ← Messaging
notifications ← Notification
reviews       ← UI-only (backend splits across Artist + Venue + Search)
search        ← Search
```

**Verdict:** the original 16-feature plan was ~75% right; 12 features is the better mirror. Three changes from the first draft:
- Fold `opportunities`, `applications`, `tickets`, `checkout` into `concerts/` (mirror Concert module — they're lifecycle stages, not peers)
- Split `profile` → `user` (mirror backend User module)
- Rename `preferences` → `customer` (mirror backend Customer module)

Everything else stays.

### Migration order (revised)

1. **Setup** — `features/` folder, path aliases
2. **Leaves:** `messaging`, `reviews`, `customer`, `notifications`, `user`, `payments`
3. **Mid-tier:** `artists`, `venues`, `contracts`
4. **Composer:** `concerts` (big one — absorbs 4 former features; do it as multiple sub-PRs if needed)
5. **Cross-cutting:** `search` (absorbs `headers/`), `auth`
6. **Cleanup:** delete empty top-level folders, trim shared `components/`/`hooks/`/`types/`
