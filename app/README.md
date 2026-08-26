# app — Concertable's frontend

React + TypeScript in one npm workspace: Vite for the four web SPAs, Expo for the two mobile apps. Two
products — a customer marketplace and a b2b manager platform — sharing code through nested tiers.

**Working here? Read [`AGENTS.md`](./AGENTS.md)** — it holds the tier map, the standards skills each tier
obeys, and the boundary gate. This file is only the workspace inventory and the commands.

## The 12 workspaces

| Workspace | Package | Compiles into |
|---|---|---|
| `shared` | `@concertable/shared` | every surface — customer and b2b, web and mobile |
| `customer/shared` | `@concertable/customer` | customer web + mobile |
| `b2b/shared` | `@concertable/b2b` | b2b web + mobile |
| `web/shared` | `@concertable/web` | all four web SPAs |
| `web/b2b/shared` | `@concertable/web-b2b` | the venue, artist and business SPAs |
| `mobile/shared` | `@concertable/mobile` | both Expo apps |
| `web/customer` | `@concertable/web-customer` | — the customer SPA |
| `web/b2b/venue` | `@concertable/web-venue` | — the venue SPA |
| `web/b2b/artist` | `@concertable/web-artist` | — the artist SPA |
| `web/b2b/business` | `@concertable/web-business` | — the business SPA |
| `mobile/customer` | `@concertable/mobile-customer` | — the customer app |
| `mobile/b2b` | `@concertable/mobile-b2b` | — the b2b app |

The shared packages publish to GitHub Packages; the six app workspaces are private.

## Commands

Run from `app/`:

```
npm run dev:customer          # or dev:venue / dev:artist / dev:business
npm run dev:mobile:customer   # or dev:mobile:b2b
npm run build:packages        # every shared package, in dependency order
npm run build:customer        # or build:venue / build:artist / build:business
npm run lint:boundaries       # dependency-cruiser over all 12
```

Every app typechecks the shared trees against its own tree, so a tier leak fails a *different* app's
build — all four web builds and both mobile builds green is the boundary gate. `lint:boundaries` catches
what a typecheck cannot, through two `error`-severity rules in `.dependency-cruiser.cjs`:
`not-to-foreign-workspace` (no reaching into a sibling workspace's files) and
`cross-platform-b2b-has-no-platform-dependencies` (`b2b/shared` may not touch `web/`, `mobile/`, or any
platform-only library). It runs in the `fe-boundaries` CI job as well as by hand.

Per-app detail, including the `routeTree.gen.ts` regeneration step:
[`web/AGENTS.md`](./web/AGENTS.md), [`mobile/AGENTS.md`](./mobile/AGENTS.md).
