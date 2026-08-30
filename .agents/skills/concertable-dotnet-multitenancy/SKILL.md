---
name: concertable-dotnet-multitenancy
description: Where Concertable's tenancy lives — only B2B of the five services has a tenant stance at all (Payment scopes by an opaque `owner` claim, Customer's data is user-scoped), with `ITenantScoped` and `ITenantContext` in `Concertable.Kernel` and `TenantInterceptor` plus the read stance in `Concertable.DataAccess` while every filtered stance (`TenantScopedDbContext`, `VenueArtistTenantScopedDbContext`, `AdminDbContext`, `TenantFilters`) stays inside `Concertable.B2B.DataAccess`, the concrete per-module stance roster in B2B's own `CODE_PATTERNS.md`, and the `IgnoreQueryFilters` ban wired through `api/BannedSymbols.txt` and `RS0030 = error`. Use when touching a tenant-filtered context or entity here, when a tenancy question arrives outside B2B, or before generalising a B2B stance into shared code.
---

# Multitenancy — where Concertable's tenancy actually lives

The generic standard is the `multitenancy` skill: visibility composed rather than subtracted, the stance
classes, one stance per query class. This file says which of Concertable's projects own that machinery, and
which services have it at all.

## Tenancy is B2B's, and only B2B's

Of the five services, **only B2B has a tenant stance.** Auth, Customer, Search and Payment have none — no
tenant-filtered context, no tenant marker on their entities. Of the 64 files referencing `ITenantContext`,
61 are B2B's; the other three are the abstraction itself, its resolver, and the shared interceptor.

So a tenancy question that arrives while working on another service is almost always the wrong question.
Payment scopes by an opaque `owner` claim read at its own HTTP boundary (`ICurrentPayoutOwner`,
fail-closed), and no other service filters by tenant at all. Do not generalise B2B's stances into shared
code to "get ahead of" a second tenanted service.

## What sits in shared code, and what does not

| Piece | Project |
|---|---|
| `ITenantScoped` — the `TenantId` marker, settable so the interceptor can stamp it | `Concertable.Kernel` |
| `ITenantContext` — the ambient tenant (`Guid? TenantId`, nullable so it fails closed) | `Concertable.Kernel.Identity` |
| `TenantInterceptor` — stamps `TenantId` at `SaveChanges` | `Concertable.DataAccess.Infrastructure` |
| `ReadDbContext` — the tenant-independent read stance, `SaveChanges` throws | `Concertable.DataAccess.Infrastructure` |
| `TenantScopedDbContext`, `VenueArtistTenantScopedDbContext`, `PrivilegedDbContext`, `IHasTenantContext`, `IVenueArtistTenantScoped`, `TenantFilters` | `Concertable.B2B.DataAccess` |

The split is deliberate and is the shared-is-the-intersection rule applied: the *marker* and the *ambient
value* are audience-agnostic enough to live in Kernel, while every filtered **stance** is B2B's and stays
behind B2B's boundary. Domain code never sets `TenantId` — the interceptor does, mirroring `IAuditable`.

## The concrete stance roster is B2B's own doc

Which base each module's context derives from, which entities are filtered and which are unfiltered by
design, and how `ApplyVenueArtist` / `ApplySingleOwner` are declared: `api/Concertable.B2B/CODE_PATTERNS.md`,
"The DbContext stances, per module" and "Which entities are filtered". It lives with the service because it
travels with the service when B2B becomes its own repo.

## The `IgnoreQueryFilters` ban is wired, not aspirational

`api/BannedSymbols.txt` bans both overloads and the root `.editorconfig` sets `RS0030 = error`, so a
per-query bypass fails the build rather than earning a review comment. The analyzer's own message names the
`multitenancy` skill and B2B's roster — keep it pointing at something that exists.
