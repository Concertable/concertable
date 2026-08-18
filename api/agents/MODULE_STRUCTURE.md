# Module Structure — Concertable's own naming and boundary decisions

The generic layering standard is the `module-structure` skill: the Contracts/Domain/Application/
Infrastructure/Api split, the inward-only reference graph, which layers a component actually needs, the
visibility cascade and `InternalsVisibleTo`, the cross-module rules, and what a module facade may do. It
applies to every **module** (a unit of audience-facing functionality) and every **shared library** here.

Each service is its own deployable — see [`../ARCHITECTURE.md`](../ARCHITECTURE.md). A service is a
modular monolith *internally*, which is what these rules govern; they are not a description of one
monolith spanning services.

This file carries only what this repo adds.

## Project names carry the owning service; shared libraries don't

```text
api/Concertable.<Service>/src/Modules/<Module>/
  Concertable.<Service>.<Module>.{Contracts,Domain,Application,Infrastructure,Api}/
  Tests/Concertable.<Service>.<Module>.{UnitTests,IntegrationTests}/
```

Examples: `Concertable.B2B.Concert.Domain`, `Concertable.Customer.Review.Application`. Genuinely
cross-service shared libraries sit at `api/Concertable.Shared/Concertable.<Name>/` **unprefixed**.

Internal controllers are discovered by `InternalControllerFeatureProvider`
(`Concertable.Shared.Api/Controllers/`), wired by `ControllerBuilderExtensions`. The three deliberately
`public` controllers are `BlobController`, `FallbackController` and `GenreController`.

## What the cross-module rules resolved to here

The cross-module `ReadDbContext` that existed during extraction has been **deleted**. The shared reference
vocabulary the skill's enum rule refers to is `Genre`, in `Concertable.Contracts`.

## `Tenant` internally, `organization` at the HTTP boundary

`Tenant` is the domain and persistence term; `organization` is the product/API term. Translate once, in the
Api layer: organization vocabulary in routes and HTTP models where the surface represents the active
tenant, while services, repositories, entities and columns keep `Tenant`/`TenantId`. Never introduce an
`OrganizationId` alias below the HTTP boundary.

Routes are token templates, not hand-lowercased literals: `[Route("api/[controller]")]`, and
`[HttpGet("/api/organization/[controller]")]` for the active-tenant surface. `[controller]` is lowercased
and kebab-cased by `RouteTokenTransformerConvention` + `KebabCaseRouteTransformer`, registered in
`Concertable.B2B.Web/Program.cs` — **only** in the B2B host, which `TECH_DEBT.md` records.

`X-Tenant-Id` selects the active tenant; never duplicate that selector in a route or query string. A
tenant's zero-or-one Artist or Venue is a singleton sub-resource resolving to `api/organization/artist`
or `api/organization/venue` — not a human-user resource and not an invented multi-profile collection.
Canonical resources stay addressable by their own ids at `api/artist/{artistId}` and `api/venue/{venueId}`.

## Migrations

Never additive — [`../AGENTS.md`](../AGENTS.md) "Migrations" owns the rule.
