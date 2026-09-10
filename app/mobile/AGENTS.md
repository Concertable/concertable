# Mobile Apps

Inherits [`../AGENTS.md`](../AGENTS.md) — which names the frontend standards skills every tier below
obeys. This file covers only the mobile sharing tier and the
build gate; don't restate the conventions here.

Two products, two tiers of sharing:

- `shared/` (`@concertable/mobile`) — code both mobile apps (b2b + customer) compile. Rules:
  [`shared/AGENTS.md`](./shared/AGENTS.md).
- per-app `src/` (`b2b/`, `customer/`) — everything only that app can do. Unlike the web tier, each
  mobile app is a single Expo app for its whole product (no venue/artist split), so there is no
  nested "both-manager-apps" tier here.

After making any changes to a mobile app or `shared/`, verify before reporting done — **both mobile
builds green is the boundary gate**, mirroring the web builds green gate: each app's `tsc --noEmit`
typechecks the shared trees against its own tree, and `expo export` proves Metro/NativeWind actually
resolve `@concertable/mobile` from its built `dist`, not just from the TS project reference.

```
npm -w @concertable/mobile-b2b exec -- tsc --noEmit -p tsconfig.json
npm -w @concertable/mobile-b2b exec -- expo export --platform android
npm -w @concertable/mobile-customer exec -- tsc --noEmit -p tsconfig.json
npm -w @concertable/mobile-customer exec -- expo export --platform android
```

CI runs both as carved, feed-restored standalone builds (`app/scripts/carve-fe.mjs`), the mobile
counterpart of the web carve matrix.
