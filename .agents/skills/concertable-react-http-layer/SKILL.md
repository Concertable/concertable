---
name: concertable-react-http-layer
description: Concertable's four frontend HTTP clients — `apiClient`, `paymentClient`, `searchClient` and `customerClient`, created bare in the shared lib with only `searchClient` carrying the comma param serializer, base URL and auth and tenant headers attached in the app tree through the `configureClient` fluent builder, and the single error seam where `resolveApiError` and `ProblemDetails` live in shared while only toast rendering stays in the web query client, with `isApiError` the one status branch a route guard may use. Use when adding an api module here, wiring headers onto a client, or seeing a feature import `isAxiosError`.
---

# HTTP — Concertable's four clients and the error seam

The generic standard is the `http-layer` skill: one `xApi` per resource, one client instance per backend
created bare and enhanced in the app tree, errors resolved once at the query client. This file is which
clients exist here and where each layer configures them.

## The four clients

One instance per backend the site calls, created **bare** in `app/shared/src/lib/*Client.ts`
(`@concertable/customer` for `customerClient`):

| Instance | Backend |
|---|---|
| `apiClient` | our own site API |
| `paymentClient` | Payment |
| `searchClient` | Search |
| `customerClient` | Customer |

Only `searchClient` carries the `qs` comma param serializer — it pairs with Search's
`CommaDelimitedGenreArrayModelBinder`. The other three send no array query params.

## Base URL, auth and tenant headers attach in the app tree

In `app/web/shared/src/lib/` and `app/web/b2b/shared/src/lib/b2bClient.ts`, through the shared fluent builder
from `@concertable/shared/lib/client`:

```ts
configureClient(instance, url).withAuth(getToken, onUnauthorized).withTenant(getTenantId, header)
```

Each platform binds its auth flavour once — `configureWebClient` for the OIDC `userManager`,
`configureMobileClient` for token storage. B2B chains `.withTenant(…, X-Tenant-Id)`.

## The error seam is `isApiError`, and axios never leaves the client layer

- `ProblemDetails` and `ErrorMeta` types and the `resolveApiError` policy live in
  `@concertable/shared/lib/problemDetails.ts` so mobile reuses them. Only the toast *rendering* (sonner) stays
  in the web `queryClient.ts`, whose `QueryCache`/`MutationCache` `onError` is the one seam.
- The shared retry policy is `queryRetry.ts`.
- A route guard that must branch on status reads `isApiError` from `@concertable/shared/lib/apiError` — the
  venue and artist `guards.ts` are the only callers.
