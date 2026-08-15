# app/shared — Technical Debt

---

## LOW

### The four `lib/*Client.ts` factories are near-verbatim copies

`apiClient`, `paymentClient`, `searchClient`, and `customerClient` (`app/shared/src/lib/*Client.ts`,
`@concertable/customer` for `customerClient`) are each a hand-written `axios.create()` call, near-
identical apart from base URL and the `searchClient`-only `qs` comma serializer. Each app tree then
repeats the same per-backend interceptor wiring (`configureClient(...).withAuth(...).withTenant(...)`)
once per client.

Durable fix: a `createApiClient(name)` factory here that the four call sites reduce to, plus a shared
`attachAuth(client)` helper so app-tree wiring stops repeating the same two-layer shape per backend.
Same two-layer split as today (bare core client / app-tree auth) — this only collapses the duplication
within each layer, not the layers themselves.

**Resolves when:** the four `lib/*Client.ts` files are one `createApiClient(name)` factory, and the
per-app interceptor call sites share one `attachAuth` helper.
