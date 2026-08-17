# Code Patterns — Concertable's own precedents

The generic structural standard lives in the `persistence`, `multitenancy`, `keyed-strategies`,
`dependency-injection`, `module-structure`, `microservice-boundaries` and `proto` skills. Sibling of
[`CODE_CONVENTIONS.md`](./CODE_CONVENTIONS.md) (naming/style); this file is about *structure*, and
carries only what this repo adds to those skills.

B2B's own rosters — its DbContext stances, which entities are query-filtered, the `DealType` strategy
families and workflow steps — are in
[`../Concertable.B2B/CODE_PATTERNS.md`](../Concertable.B2B/CODE_PATTERNS.md), imported only by B2B.

## One repository per entity — never fold a satellite entity into another entity's repository

A repository's generic base binds it to exactly one entity. Give every entity its own repository even
when several share a module and DbContext, and even when one is queried far more than another — Concert's
six sibling entities have six repositories, Conversations' two have two.

The tell that a repository has drifted: its interface mixes queries for two or more unrelated entity
types, or it hand-writes a `GetXByIdAsync`/`AddX` pair that re-implements what the generic base already
gives the *wrong* entity bound as `TEntity`. Split it — one interface, one repository, one entity — even
if a single service then injects two repositories. That is the service's job, not a reason to merge the
persistence contracts.

## Typed HTTP clients — the current Refit inventory

*Which* protocol a hop uses is decided first by the `microservice-boundaries` skill (gRPC for our own
internal sync; HTTP only at the forced boundaries). Once that table has chosen HTTP, the call gets a
Refit interface — one per remote contract:

- **`IGoogleGeocodingApi`** — third-party REST. External, we don't own the shape.
- **`IUserClaimsApi`** — the internal `/internal/users/{sub}/claims` hop. The standing transition-window
  exception to "our own internal sync is gRPC"; it stays Refit until that service has a gRPC surface.
- **`ITokenApi`** (`Concertable.Kernel.Auth`) — the OAuth2 `/connect/token` client-credentials POST behind
  `ClientCredentialsTokenService`. Form-encoded via `[Body(BodySerializationMethod.UrlEncoded)]`, response
  pinned with `[JsonPropertyName]` (`access_token`/`expires_in`), authority set as the base address per
  host in `AddClientCredentials`. The scope-keyed token *cache* lives in the service; Refit owns only the
  wire call underneath it.

`Concertable.Kernel` carries the `Refit.HttpClientFactory` package for `ITokenApi`. That is fine: the
"shared is the intersection" rule is about not bolting audience-specific *concepts* onto shared *types*,
not about forbidding a shared utility package.

**One caveat specific to `ITokenApi`:** `ClientCredentialsTokenService` is a **singleton** (it owns the
shared cache), so its Refit client is a captive dependency — one `HttpMessageHandler` pinned for the
app's lifetime, with no factory rotation. Accepted here because the authority is a stable internal
endpoint hit infrequently. Never copy that shape onto a hot or DNS-volatile client; those stay
scoped/transient so the factory rotates handlers normally.
