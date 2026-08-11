# Browser-storage inventory (engineering)

Every item Concertable's four web SPAs store on a visitor's device, with the lawful basis
each is stored under. This is the engineering record a solicitor turns into the public
cookie/storage policy — it is not the policy itself, and not legal advice.

**Source of truth:** [`src/lib/storageManifest.ts`](./src/lib/storageManifest.ts). This
document mirrors it. A drift-guard test (`src/lib/storageManifest.test.ts`) scans
`app/web` for `localStorage` / `sessionStorage` / cookie / IndexedDB writes — including
zustand `persist()` stores — and fails the build when one is not classified in the manifest,
so new storage cannot ship unclassified and this inventory cannot silently fall out of date.

## Classification (post-DUAA UK PECR, from `plans/launch/CONSENT_RESEARCH.md`)

PECR reg. 6 requires consent before storing or reading anything on the device, unless it is
**strictly necessary** for a service the user explicitly requested. Working classes:

- **Necessary** — essential to a requested service (auth/session, payment fraud, the consent
  record itself). No consent; still disclosed.
- **Functional** — remembers a user preference, no PII, set on a user action. No consent under
  DUAA's customisation exemption; still disclosed.
- **Analytics / Marketing** — non-essential; must be **consent-gated before load** and default
  off. None loads today; the banner + gating primitive are the infrastructure for when it does.

## Stored items

| Key | Storage | Owner | Purpose | Duration | Class | SPAs |
|---|---|---|---|---|---|---|
| `cookie-consent` | localStorage | first-party | Records the cookie-consent decision so the banner shows once | Persistent until cleared | Necessary | customer, venue, artist, business |
| `theme` | localStorage | first-party | Light/dark UI preference | Persistent until cleared | Functional | customer, venue, artist |
| `concertable.active-tenant` | localStorage | first-party | Remembers the manager's selected active tenant across sessions | Persistent until cleared | Functional | venue, artist |
| `oidc.user:*`, `oidc.*` state | localStorage | oidc-client-ts (store configured by us) | Auth tokens + sign-in / silent-renew state | Session / token lifetime | Necessary | customer, venue, artist |
| `__stripe_mid` | cookie | Stripe | Fraud prevention (Radar) machine id during payment | ~1 year | Necessary | customer, venue, artist |
| `__stripe_sid` | cookie | Stripe | Fraud prevention (Radar) session id during payment | ~30 minutes | Necessary | customer, venue, artist |
| `m` | cookie (`m.stripe.com`, httpOnly) | Stripe | Fraud-prevention identifier | ~1–2 years | Necessary | customer, venue, artist |

**Stripe cookies are now checkout-scoped.** Stripe.js used to load at app boot (setting
`__stripe_mid`/`__stripe_sid`/`m` on anonymous landing, weakening the strictly-necessary
basis). It now loads lazily via `getStripe()` only when a checkout/payment component mounts —
so its cookies fire only for a payment the user started, which is where the exemption holds.

**OIDC keys** are written internally by `oidc-client-ts` via `WebStorageStateStore` in
`src/features/auth/config/oidcConfig.ts`; only appear after sign-in.

## Third-party contact that sets no storage

- **Google Maps JS API** — the runtime audit found it sets **no** cookie/localStorage/
  sessionStorage/IndexedDB. It used to be contacted at app boot via a root `APIProvider`; it now
  loads on use only, behind a scoped `MapsProvider` around the find/search and map components
  (`src/providers/MapsProvider.tsx`). There is no Maps storage item to consent-gate; the concern
  was the boot-time third-party contact, now removed.

## Not present

- **No analytics or marketing technology loads in any SPA** (no GA/GTM/Meta/etc.). The
  Analytics/Marketing consent categories are infrastructure ahead of roadmapped tech, not dead
  UI: when such tech is added it registers with the consent gate (`src/lib/consentGate.ts`,
  `registerConsentGatedScript`) so it loads only after opt-in.
- **IndexedDB and sessionStorage are unused.**

## Consent mechanism

- Banner + preferences dialog (`src/components/CookieConsentBanner.tsx`,
  `CookiePreferencesDialog.tsx`) with **Analytics** and **Marketing** categories, both **off by
  default**, **Reject-all at equal prominence** to Accept-all, and an always-reachable "Manage
  cookies" re-open. The decision is stored as a timestamped record (`src/lib/consent.ts`).
- `registerConsentGated` / `registerConsentGatedScript` (`src/lib/consentGate.ts`) load a
  resource only while its category is granted and remove it on withdrawal — the primitive that
  makes the toggles actually gate. Consent-Mode-v2 deny-by-default defaults are added alongside
  the first Google tag when analytics lands.

## Legal-gated tail (Month-4)

Everything above is engineering-complete. Two items wait on legal/product input, not on more
engineering:

1. **Cookie/storage policy copy** — the solicitor drafts it from this inventory; wiring the
   finished wording into the `/cookies` page is the only remaining step (that page route is the
   separate Month-4 launch item).
2. **Whether Google Maps must be consent-gated on non-search pages** — a solicitor/product call.
   Maps already loads on-use only; if it must additionally sit behind a `functional` consent
   category off its core-search pages, that wraps the existing `MapsProvider` mount points with
   `registerConsentGated` — no re-architecture.
