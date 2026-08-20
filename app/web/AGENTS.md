# Web Apps

Inherits [`../AGENTS.md`](../AGENTS.md) — which names the frontend standards skills every tier below
obeys. The tier roster, the route contract and the build gate are the `app-tiers` skill; this file covers
only what the web tree adds.

Two products, three tiers of sharing:

- `shared/` (`@concertable/web`) — code every SPA compiles (universal). Rules:
  [`shared/AGENTS.md`](./shared/AGENTS.md).
- `b2b/shared/` (`@concertable/web-b2b`) — code both manager apps (venue + artist) compile; the customer
  app does not depend on it. Rules: [`b2b/shared/AGENTS.md`](./b2b/shared/AGENTS.md).
- per-app `src/` — everything only that site can do.

After making any changes to a web app or shared code, run the four web builds before reporting done —
all four green is the boundary gate, and `app-tiers` carries the commands, the business app's vite-only
build and the `routeTree.gen.ts` regeneration step.
