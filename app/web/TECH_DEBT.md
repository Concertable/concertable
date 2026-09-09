# app/web — Technical Debt

---

## MED

### `.env.development` holds a live Google Maps key, against the convention its sibling states

`app/web/.env.development` sets a real `VITE_GOOGLE_MAPS_API_KEY`. `app/web/.env.production` beside
it blanks both publishable keys and states the convention — "injected by CI at build, blank in git on
purpose" — and the dev file never adopted it. This repository is public, so the key is exposed today,
and it is billable, which a Stripe *publishable* key in the same file is not.

It also spreads: this file is web-tier configuration every product surface loads through its vite
`envDir`, so every repository that inherits the tier inherits the key.

**Resolves when:** the key is rotated and either CI-injected and blank in git like the production one,
or demonstrably restricted to localhost referrers so it is not a usable credential.

### Venue Deny button never renders — gated on a wire key the server does not send

`app/web/b2b/venue/src/features/concerts/components/ApplicationCard.tsx:60` gates Deny on
`actions.reject`. `GET /api/application/opportunity/{id}` returns `VenueApplicationActions`
(`ApplicationResponses.cs`), whose member is `Decline`, so the serialized key is `decline` and
`reject` is never present. The href behind it is `/api/application/{id}/reject`, which is what made
the mismatch look right. The button has therefore never rendered on the venue applications list; the
venue *dashboard* uses its own type that says `decline`, which is why Decline works there.

`app/web/b2b/shared/src/features/concerts/types.ts` currently declares both `decline` and a
`@deprecated reject`, the latter existing only so this consumer keeps type-checking.

Not fixable on the branch that found it: `carve-fe` builds each app's committed source against the
`@concertable/*` tiers **as published to the feed**, and the published `web-b2b` still types
`ApplicationActions` with `reject` only — so flipping line 60 to `actions.decline` fails
`carve-fe (web/b2b/venue)` with `TS2339: Property 'decline' does not exist`. It needs the package
republished first.

**Resolves when:** `@concertable/web-b2b` has published a version whose `ApplicationActions` carries
`decline` and not `reject`, `ApplicationCard.tsx` gates Deny on `actions.decline`, the `reject` member
is deleted from `ApplicationActions`, and `carve-fe (web/b2b/venue)` is green.

---

### Web concert detail Buy Tickets below `@3xl` — fixed, narrow-viewport E2E outstanding

Fixed on `Fix/TechDebtSweep`: the single `ConcertCard` now reflows (full-width at the top below
`@3xl`, sticky sidebar at/above it) instead of being `display:none`, so `buy-tickets` is reachable at
every width and stays one unambiguous testid (Playwright strict mode stays happy). Outstanding only: a
**narrow-viewport E2E** asserting `buy-tickets` is reachable at a sub-`@3xl` width (needs Docker).

**Resolves when:** the narrow-viewport E2E scenario lands green.


### Browser-storage classification is detection-by-regex, not prevention-by-construction

First-party device storage has no single sanctioned accessor: `consent.ts` (`cookie-consent`),
`ThemeProvider` (`theme`), and the B2B tenant session (`concertable.active-tenant`) each
write `localStorage` their own way. The "new storage must be classified" guarantee is enforced by a
**regex drift-guard** (`shared/src/lib/storageManifest.test.ts`) that scans for known write patterns
(`setItem`, `document.cookie=`, `persist(`, `indexedDB.open(`) against `STORAGE_MANIFEST` — detection
after the fact, not prevention. This already missed once: the guard was blind to zustand `persist()`'s
implicit write, so `concertable.active-tenant` shipped unclassified and undetectable until code review
caught it (finding NAT1). A novel first-party write mechanism can slip past the same way.

Durable fix: a generic first-party `createClassifiedStorage({ key, api, classification,
consentCategory })` in `shared/src/lib` that is the only sanctioned way our code touches storage —
it auto-registers itself in `STORAGE_MANIFEST` and refuses to write an `analytics`/`marketing` item
until `hasConsent(category)`. `consent.ts` and `ThemeProvider` move onto it; classification becomes a
compile/construction property instead of a scanner's guess. **Caveat (why the manifest + drift-guard
stay):** third-party writers we don't control — oidc-client-ts, Stripe.js, and zustand `persist`
itself — always write on their own, so they can never route through the accessor; the manifest + guard
remain the catch-all for those. This hardens only the first-party path.

**Resolves when:** first-party storage writes go through the classified accessor and the drift-guard's
role is reduced to covering the enumerated third-party/library writers.
