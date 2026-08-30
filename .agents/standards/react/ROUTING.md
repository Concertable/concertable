# Routing

TanStack Router, file-based. The route tree is the file tree, so a route's URL is never declared in two
places and never assembled from strings at a call site.

## A route file wires a route; it does not hold the feature

A file under `routes/` exports one `Route` and the thin component that lays it out. Everything the screen
actually does — queries, mutations, forms, presentation — lives in the feature slice the route imports.

**A route file that grows past its layout is the smell.** Move the body into the feature and leave the
route naming it. `routes/` is then browsable as a URL map rather than as a second copy of the app.

```tsx
export const Route = createFileRoute("/settings/members")({
  beforeLoad: requireAuth,
  component: MembersPage,   // from the members feature
});
```

## Guards are `beforeLoad`, and a guard is a plain async function

Access control belongs on the route, not inside the component that renders after it. Put it in
`beforeLoad` and let it `throw redirect(...)`; returning normally is the only way through.

**Write the guard as an exported `requireX` function in the feature that owns the identity or membership
it checks**, so every route that needs it names the same function. A guard rendered as a wrapper component
is the anti-pattern: by the time it runs, the protected component has already mounted and its queries have
already fired.

```ts
export async function requireAuth({ location }: { location?: { pathname: string } } = {}) {
  if (!(await hasValidSession()))
    throw redirect({ to: "/login", search: { redirect: location?.pathname ?? "" } });
}
```

A redirect to a login route carries the attempted path in `search` so the return trip is not guesswork.

## The guard warms the cache; it is not a loader

**Do not give a route a `loader` that fetches what the component then reads with `useQuery`.** That is two
owners for one piece of server state — a stale loader result and a live query result, differing.

Where a guard already needs the data to make its decision, have it call `ensureQueryData` with the same
query key the component uses. One fetch, one cache entry, one owner: the query client. A route `loader`
earns its place only when nothing on the page ever re-reads that data.

## Search params are parsed, not read

**Anything arriving in the URL is untrusted input.** Declare `validateSearch` with a schema so a malformed
URL fails once at the boundary with a typed error, rather than becoming `undefined` three components deep.
Read it back through the route's own typed `useSearch` — never `URLSearchParams` or `location.search`.

The parsed search object is also the right home for filter and pagination state that should survive a
reload or be shareable as a link. State that should not appear in the URL belongs in a store instead.

## Navigation is typed

Use `<Link to="…" params={…} />` and the typed `navigate` from the router. Never build a path by string
concatenation or template literal, and never `window.location.href` for an in-app destination — both
defeat the type checking that file-based routing exists to give you, and both silently survive a rename.

A full page load is correct for exactly one case: leaving the app for a different origin.

## Layout routes own the chrome

A pathless layout route (`_app/route.tsx`) owns the shell — navigation, header, sidebar — and its children
own the content. Put the guard that covers a whole section on the layout route rather than repeating it on
every leaf; a leaf added later then inherits the protection instead of quietly missing it.
