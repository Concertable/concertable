---
name: concertable-dotnet-http-api
description: Concertable translates `Tenant` to `organization` exactly once, in the Api layer — organization vocabulary in routes and HTTP models, `Tenant` and `TenantId` everywhere below, and never an `OrganizationId` alias under the HTTP boundary; plus routes as `[controller]` token templates rather than hand-lowercased literals, `X-Tenant-Id` as the only active-tenant selector, singleton sub-resources for a tenant's zero-or-one artist or venue, and the public details endpoints that keep a frozen `Response`. Use when adding or reviewing an endpoint here, naming a route, or shaping a public marketplace response.
---

# HTTP API — `Tenant` internally, `organization` on the wire

The generic standard is the `http-api` skill: DTOs out, `Request` records in, identity from the route. This
file is the one vocabulary translation this system makes, and the routing shapes that follow from it.

## Translate once, in the Api layer

`Tenant` is the domain and persistence term; `organization` is the product and API term. Organization
vocabulary appears in routes and HTTP models where the surface represents the active tenant, while services,
repositories, entities and columns keep `Tenant`/`TenantId`.

**Never introduce an `OrganizationId` alias below the HTTP boundary.**

## Routes are token templates, not hand-lowercased literals

```csharp
[Route("api/[controller]")]
[HttpGet("/api/organization/[controller]")]   // the active-tenant surface
```

`[controller]` is lowercased and kebab-cased by `RouteTokenTransformerConvention` plus
`KebabCaseRouteTransformer`, registered in `Concertable.B2B.Web/Program.cs` — **only** in the B2B host, which
that repo's `TECH_DEBT.md` records.

## The active tenant comes from a header, never a route or query parameter

`X-Tenant-Id` selects it. Never duplicate that selector in a route or query string.

A tenant's zero-or-one Artist or Venue is a singleton sub-resource resolving to `api/organization/artist` or
`api/organization/venue` — not a human-user resource, and not an invented multi-profile collection. Canonical
resources stay addressable by their own ids at `api/artist/{artistId}` and `api/venue/{venueId}`.

## The details endpoints keep a frozen `Response`

The generic skill's default is that a controller returns the DTO verbatim. The bounded exception here: the
public `Venue`/`Artist`/`Concert` **details** endpoints in B2B and Customer always expose a dedicated
`XDetailsResponse`, even when it is currently a field-for-field clone of the DTO. These are the anonymous
marketplace surface, and the `Response` is the frozen wire contract that lets the internal read DTO change —
server-only fields, projection shape — without breaking public clients.

This covers the details read endpoints only, not every DTO.
