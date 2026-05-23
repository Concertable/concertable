# Modular Monolith Rules

Concrete conventions. The *what goes where* doc. Companion to [MM_NORTH_STAR.md](MM_NORTH_STAR.md)
(the *why*).

Applies to **modules** in the monolith (`Concert`, `Customer`, `Payment`, ...) AND **shared
libraries** that aren't tied to a specific module's data (`Concertable.Messaging`,
`Concertable.Authorization`, `Concertable.Notification`, ...). Same layering rules. The only
difference is that a shared library is consumed by multiple modules/services, whereas a module is
the unit of audience-facing functionality.

---

## Project layering

Every module / shared library follows the same Clean-Architecture-ish layering. Pick the layers you
need — not every csproj has all five.

| Layer | Purpose | Visibility |
|---|---|---|
| `X.Contracts` | Cross-boundary surface — interfaces, events, marker types, value types other consumers see. The public API. | `public` |
| `X.Domain` | Entities, value objects, domain events. Pure types, no infrastructure deps. | `internal` (default) |
| `X.Application` | Service/repo interfaces, validators, internal DTOs, mappers. | `internal` |
| `X.Infrastructure` | EF configs, DbContext, concrete service/repo impls, integration event handlers, DI registration. | `internal` |
| `X.Api` | Controllers, HTTP-specific extensions. **Modules only** — shared libs don't expose HTTP. | `public` (controllers); `internal` (everything else) |

### Reference graph (allowed dependencies)

```
Contracts       → Kernel (and other Contracts when sharing base types)
Domain          → Contracts, Kernel
Application     → Domain, Contracts, Kernel
Infrastructure  → Application, Domain, Contracts, Kernel, framework deps (EF, Hosting, etc.)
Api             → Application, Contracts, Kernel, ASP.NET
```

Arrows only point inward (toward Contracts/Kernel). **No outward refs** — `Domain` doesn't ref
`Infrastructure`; `Contracts` doesn't ref `Application`.

### When you need each layer

- **Contracts** — when the thing has a cross-boundary surface (cross-module facade, cross-service
  events, public types). A purely internal helper has no Contracts.
- **Domain** — when the thing owns entities, value objects, or domain events. A pure utility lib
  (`Kernel`) has no Domain.
- **Application** — when there are service/repo abstractions distinct from their infra impls.
- **Infrastructure** — when there are concrete impls behind the Application abstractions, or when
  the thing owns EF mappings.
- **Api** — modules that expose HTTP endpoints. Shared libs never have an Api project.

### Visibility cascade

- `*.Contracts` types are `public` — they're the cross-boundary contract.
- `*.Domain` entities default `internal`. Promote to `public` *only* when another module legitimately
  needs the type (e.g. cross-module read projection target). See `feedback_module_impl_visibility_cascade.md`.
- `*.Application` interfaces (`IXService`, `IXRepository`) stay `internal`. Use
  `InternalsVisibleTo("X.Infrastructure")` + `InternalsVisibleTo("X.Api")` to keep siblings linked.
- `*.Infrastructure` impls stay `internal`.
- `*.Api` controllers are `public` (reverted from internal 2026-04-25 — see
  `feedback_module_facade_surface.md`).
- Tests reach `internal` types via `InternalsVisibleTo("X.UnitTests")` /
  `InternalsVisibleTo("X.IntegrationTests")` on the owning csproj's `AssemblyInfo.cs`.

---

## Folder layout

Modules always include the owning service segment (`B2B`, `Customer`, `Search`, etc.) in the
project name. Shared libs under `api/Shared/` are unprefixed because they are genuinely
cross-service.

```
api/Concertable.<Service>/Modules/<ModuleName>/
  Concertable.<Service>.<Module>.Contracts/
  Concertable.<Service>.<Module>.Domain/
  Concertable.<Service>.<Module>.Application/
  Concertable.<Service>.<Module>.Infrastructure/
  Concertable.<Service>.<Module>.Api/
  Tests/
    Concertable.<Service>.<Module>.UnitTests/
    Concertable.<Service>.<Module>.IntegrationTests/
```

Examples: `Concertable.B2B.Concert.Domain`, `Concertable.Customer.Review.Application`.

Shared libs sit at `api/Shared/Concertable.<Name>/` and follow the same per-layer csproj split
(`Concertable.<Name>.Contracts`, etc.) when they need more than one layer.

---

## Cross-module rules (the §1 of MM_NORTH_STAR)

- Zero cross-module runtime queries. Every module reads only from its own `DbContext`.
- Cross-module communication only via `IXModule` facades in Contracts (commands, narrow queries)
  or integration events (fan-out).
- Cross-module FKs are plain primitives (`int ArtistId`, `Guid UserId`) — never nav properties
  across boundaries.
- Shared reference data (Genres, etc.) FKs into `SharedDbContext`. See MM_NORTH_STAR §6.

Full set of corollaries + rationale in [MM_NORTH_STAR.md](MM_NORTH_STAR.md).

---

## Migrations

Don't add additive migrations. When any model changes, run `./initial-migrations.ps1` from `api/`
to nuke and re-scaffold every context's `InitialCreate`. See CLAUDE.md.
