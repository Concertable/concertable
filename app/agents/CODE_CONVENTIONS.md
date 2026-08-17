# Frontend Code Conventions — Concertable's own precedents

The generic TypeScript/React standard is not here. It lives in load-on-demand skills: `typescript-style`
(`interface` vs `type`, camelCase on the wire, `undefined` over `null`, discriminated unions),
`contract-naming` (domain-noun reads, `XRequest` writes, one `types.ts` per feature), `react-structure`
(the feature slice, hooks orchestrate, raw vs facade hooks, Effects), `server-state` (queries, mutations,
query keys, mutation variables), `client-state` (private stores, facade hooks), `http-layer` (`xApi`
objects, one client per backend, errors resolved once), `write-boundary` (the zod parse),
`tiered-shared-code` (slots over role checks, composed identity) and `stack-defaults`.

Sibling of [`CODE_PATTERNS.md`](./CODE_PATTERNS.md): this file is **naming and style**, that one is
**structure**. Both carry only the roster of real names in *this* repo — the part a generic skill
deliberately omits.

## The four HTTP clients, and which layer configures them

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

Base URL, auth and tenant headers attach **in the app tree** — `app/web/shared/src/lib/`, and
`app/web/b2b/shared/src/lib/b2bClient.ts` — through the shared fluent builder
`configureClient(instance, url).withAuth(getToken, onUnauthorized).withTenant(getTenantId, header)`
(`@concertable/shared/lib/client`). Each platform binds its auth flavour once: `configureWebClient` (OIDC
`userManager`) or `configureMobileClient` (token storage); b2b chains `.withTenant(…, X-Tenant-Id)`.

## The error seam is `isApiError`, and axios never leaves the client layer

- `ProblemDetails`/`ErrorMeta` types and the `resolveApiError` policy live in
  `@concertable/shared/lib/problemDetails.ts` so mobile reuses them. Only the toast *rendering* (sonner)
  stays in the web `queryClient.ts`, whose `QueryCache`/`MutationCache` `onError` is the one seam.
- The shared retry policy is `queryRetry.ts`.
- A route guard that must branch on status reads `isApiError` from `@concertable/shared/lib/apiError`
  (venue/artist `guards.ts`). Features never import `isAxiosError`; axios stays confined to the shared
  client and its interceptor — [`app/web/AGENTS.md`](../web/AGENTS.md) "HTTP errors".

## `FormData` field names are PascalCase; JSON bodies are camelCase

Multipart binds to C# by property name rather than through the JSON policy, so uploads use `"Name"`,
`"Banner"`, `"Genres[0]"` (`artistApi.ts`). Correct, and stays.

## The live `$type` unions

`PaymentAmount` is `FlatPayment | DoorSharePayment | GuaranteedDoorPayment`, keyed on `$type` with the
camelCase literals `"flat"`/`"doorShare"`/`"guaranteedDoor"` copied from the backend `[JsonDerivedType]`
discriminators. The other two: `Deal` (`FlatFeeDeal | DoorSplitDeal | VersusDeal | VenueHireDeal`,
mirroring `DealTypeNames`) in `@b2b/*`'s `features/deals/types.ts`, and the search `Header` /
`AutocompleteResult` pair keyed on `HeaderType`.

The universal `User` deliberately has **no** `$type` and no flat role — product identity is composed in its
owning tier ([`CODE_PATTERNS.md`](./CODE_PATTERNS.md)).

## Third-party envelopes that keep a suffix

`GeocodeResponse` — the raw Google Geocoding `{ status, results[] }` shape, genuinely different from the
`Coordinates` we hand back. `AxiosResponse` likewise.

## zod schemas

Per feature, in `features/<feature>/schemas/`; `SearchSchema` in `features/search/schemas/` is the
precedent, and zod is already the project's validation tool via TanStack `validateSearch`, so a form schema
adds no dependency.
