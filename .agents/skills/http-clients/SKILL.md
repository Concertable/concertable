---
name: http-clients
description: Concertable's typed Refit client inventory — `IGoogleGeocodingApi`, the `IUserClaimsApi` transition-window exception to gRPC-for-internal-sync, `ICustomerUserClaimsApi` as a per-source marker interface rather than a second contract, and `ITokenApi` for the client-credentials token call, plus why the singleton `ClientCredentialsTokenService` captive dependency is accepted here and must never be copied onto a hot or DNS-volatile client. Use when adding an outbound HTTP client here, wiring a claims source, or reviewing an HTTP hop that should have been gRPC.
---

# Typed HTTP clients — the Refit inventory

*Which* protocol a hop uses is decided first by the `microservice-boundaries` skill: gRPC for our own internal
synchronous calls, HTTP only at the forced boundaries. Once that table has chosen HTTP, the call gets a Refit
interface — one per remote contract.

| Interface | What it is |
|---|---|
| `IGoogleGeocodingApi` | Third-party REST. External, we do not own the shape. |
| `IUserClaimsApi` | The internal `/internal/users/{sub}/claims` hop. The standing transition-window exception to "our own internal sync is gRPC" — it stays Refit until that service has a gRPC surface. |
| `ICustomerUserClaimsApi` | An empty interface deriving from `IUserClaimsApi`, carrying no members of its own. |
| `ITokenApi` | The OAuth2 `/connect/token` client-credentials POST behind `ClientCredentialsTokenService`. |

## `ICustomerUserClaimsApi` is a marker, not a contract

Refit configures a client *per interface type*, so each source service the claims hop reaches needs its own
marker to hang a base address and token handler on; `AddRemoteProfileClaimsProvider<TApi>` is generic over
exactly that. Customer is the only source registered today — a second one is a second marker, not a second
contract.

## `ITokenApi` owns the wire call, not the cache

It lives in `Concertable.Kernel.Auth`: form-encoded via `[Body(BodySerializationMethod.UrlEncoded)]`, response
pinned with `[JsonPropertyName]` (`access_token`, `expires_in`), authority set as the base address per host in
`AddClientCredentials`. The scope-keyed token *cache* lives in the service above it.

`Concertable.Kernel` carrying the `Refit.HttpClientFactory` package for this is fine: "shared is the
intersection" is about not bolting audience-specific *concepts* onto shared *types*, not about forbidding a
shared utility package.

## The `ITokenApi` captive dependency is deliberate — never copy it

`ClientCredentialsTokenService` is a **singleton** because it owns the shared cache, so its Refit client is a
captive dependency: one `HttpMessageHandler` pinned for the app's lifetime, with no factory rotation. Accepted
here because the authority is a stable internal endpoint hit infrequently. Never copy that shape onto a hot or
DNS-volatile client — those stay scoped or transient so the factory rotates handlers normally.
