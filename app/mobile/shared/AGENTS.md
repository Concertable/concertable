# app/mobile/shared — `@concertable/mobile`, code shared across the mobile apps

Inherits [`../../AGENTS.md`](../../AGENTS.md) (frontend conventions + patterns).

## Everything here compiles into BOTH mobile apps (b2b + customer). Nothing web-only, nothing single-app, goes here.

Published as package `@concertable/mobile`, the mobile counterpart of `app/web/shared`: design
system (NativeWind theme), navigation shell, auth/search/messaging infrastructure, and the
concert/venue/artist detail views every mobile app renders. B2B-only concepts (opportunities,
contracts, applications, payouts) have no home here — the mobile b2b app has no shared-with-customer
"both manager apps" tier the way web does, because each mobile app is a single Expo app for its
whole product, not two.

- **Backend rule** — same as web: authenticated calls go only to backends every mobile app can call
  with its own token (`apiClient`, `searchClient`, `paymentClient`, built bare in `lib/*Client.ts`
  with no auth). Auth attaches per app via `configureMobileClient` (token-storage flavour of the
  shared fluent builder in [`CODE_PATTERNS.md`](../../agents/CODE_PATTERNS.md), "one axios instance per
  backend service") — never wired into the bare `lib/*Client.ts` instance.
- **Identity rule** — no product-conditional branching here (`isVenueManager`, tenant-type checks).
  Same slot/prop injection pattern as `app/web/shared` — the owning app decides, this tier renders
  what it's given.
- **Auth is token storage, not a browser OIDC flow.** `auth/tokenStorage.ts` +
  `auth/getValidAccessToken.ts` are the mobile equivalent of the web `userManager`; there is no
  redirect-based login here, so don't import or assume the web auth shape.

Enforcement is CI's carved, feed-restored build of both `mobile/b2b` and `mobile/customer`
(`app/scripts/carve-fe.mjs`) — the mobile counterpart of "all four web builds green." A leak of a
single-app concept into this tree fails the *other* mobile app's build the same way a web leak does.

The litmus: *"could both mobile apps render this and run every call it makes, with their own tokens,
today?"* If only one can, it belongs in that app's `src/`, even at the cost of a slot prop.
