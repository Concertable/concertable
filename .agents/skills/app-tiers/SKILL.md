---
name: app-tiers
description: Concertable's frontend surface roster and the gate that enforces its sharing tiers — six apps (four separate web SPAs with their own OIDC clients and sessions, two Expo apps) over five sharing packages from `@concertable/shared` down to `@concertable/web-b2b`, the universal route contract listing the only route literals `app/web/shared` may name, and the typecheck boundary gate where each app compiles the shared trees against its own route tree so a leak in shared fails a different app's build — plus the business app's vite-only build, the `routeTree.gen.ts` regeneration step, and why `expo export` is part of the mobile gate. Use when placing new frontend code in a tier, adding a route literal or an import to a shared tree, adding a workspace, or verifying a change that touched shared code.
---

# App tiers — six surfaces, five sharing tiers, one typecheck gate

The generic standard is the `tiered-shared-code` skill: code belongs at the widest tier every consumer can
legitimately run, and variation is injected from the owning app rather than resolved inside shared code.
This is the roster that rule applies to here, and the gate that enforces it.

## The surfaces

Six apps, two products. Four web SPAs and two Expo apps:

| Surface | Product | Notes |
|---|---|---|
| `web/customer` | customer | |
| `web/b2b/venue` | b2b | |
| `web/b2b/artist` | b2b | |
| `web/b2b/business` | b2b | minimal; consumes a slice of shared |
| `mobile/customer` | customer | Expo |
| `mobile/b2b` | b2b | one app for the whole product — no venue/artist split |

**The four web SPAs are fully separate sites**: separate OIDC clients, separate sessions, separate
backends behind the same clients (customer → the Customer service, managers → B2B). A manager signed into
the venue site is not signed in anywhere else. Nothing in a shared tree can know which site it is running
in, which is what makes the tier rule enforceable rather than aspirational.

## The sharing tiers, widest first

| Tier | Package | Compiles into |
|---|---|---|
| `app/shared` | `@concertable/shared` | **every** surface — customer + b2b, web + mobile |
| `app/customer/shared` | `@concertable/customer` | customer web + mobile |
| `app/web/shared` | `@concertable/web` | the four web SPAs |
| `app/web/b2b/shared` | `@concertable/web-b2b` | venue + artist only (`@b2b/*` is its own internal alias) |
| `app/mobile/shared` | `@concertable/mobile` | both Expo apps |
| per-app `src/` | — | one surface only |

Each tier's own `AGENTS.md` carries its inventory and what its boundary adds; none of them restate the
tier rule. `app/b2b/shared` (`@concertable/b2b`) is a seventh workspace nothing imports yet — a
cross-platform b2b tier mid-cut-over, so it is not a tier in this table until the cut-over lands.

## The universal route contract

`app/web/shared` may reference **only** the routes every one of the four SPAs registers:

`/` · `/login` · `/register` · `/auth/callback` · `/success` · `/fail` · `/stripe-refresh` ·
`/stripe-return` · `/settings` · `/settings/payment` · `/find` · `/find/{artist,venue,concert}/$id`

Any other literal — `/my`, `/profile/…`, `/concert/checkout/$id` — is injected by the owning app as a
prop. A route literal is the cheapest possible tier leak and the one that hides longest: it type-checks
perfectly in the app the author is running.

## The typecheck boundary gate

**Enforcement is the type system, not review.** Each app compiles the shared trees against its *own*
route tree and its own config, so a leak in shared fails a *different* app's build.

```
npm -w @concertable/web-customer run build
npm -w @concertable/web-venue run build
npm -w @concertable/web-artist run build
npm -w @concertable/web-business run build
npm -w @concertable/mobile-b2b exec -- tsc --noEmit -p tsconfig.json
npm -w @concertable/mobile-b2b exec -- expo export --platform android
npm -w @concertable/mobile-customer exec -- tsc --noEmit -p tsconfig.json
npm -w @concertable/mobile-customer exec -- expo export --platform android
```

All six green is the gate. Three wrinkles that are not defects:

- **`web/business` runs `vite build` only**, no `tsc -b` — it uses a slice of shared and does not
  implement the full feature set shared references.
- **A new or renamed route file needs `routeTree.gen.ts` regenerated** before `tsc -b` can see it: one
  `npm -w <app> exec -- vite build`, or the dev server.
- **`expo export` is part of the mobile gate**, not extra: it proves Metro and NativeWind actually resolve
  the shared package from its built `dist` rather than only through the TS project reference.

CI runs all of it as carved, feed-restored standalone builds (`app/scripts/carve-fe.mjs`), so a build that
only works inside the workspace fails there.

## The test for new code

*"Could every surface at this tier render this, and run every call it makes, with its own token, today?"*

If only one can, it belongs in that app's tree — even when that costs an extra slot prop on a shared
component. **Do not fix a leak with an identity check inside shared**; that is the disease, not the cure.
Move the code to its owner and inject it back through a slot.
