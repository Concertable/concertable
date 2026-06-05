# Web Apps

Two products, three tiers of sharing:

- `shared/` — code every SPA compiles (universal). Rules: [`shared/CLAUDE.md`](./shared/CLAUDE.md).
- `b2b/shared/` — code both manager apps (venue + artist) compile, customer can't resolve
  (`@b2b/*` alias exists only in their configs). Rules: [`b2b/shared/CLAUDE.md`](./b2b/shared/CLAUDE.md).
- per-app `src/` — everything only that site can do.

After making any changes to a web app or shared code, run the builds to verify before reporting
done — **all four green is the boundary gate**: each app's `tsc -b` compiles the shared trees
against its own route tree, so an app-specific leak in shared fails some other app's build.

```
npm -w @concertable/web-customer run build
npm -w @concertable/web-venue run build
npm -w @concertable/web-artist run build
npm -w @concertable/web-business run build
```

The business app uses `vite build` only (no `tsc -b`) — it's a minimal app that only uses a slice of shared and does not implement the full feature set that shared references.

If you add or rename a route file, regenerate that app's `routeTree.gen.ts` before `tsc -b` can see
it (`npm -w <app> exec -- vite build` once, or run the dev server).
