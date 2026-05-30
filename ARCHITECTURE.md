# Concertable Architecture

## Microservice premise (LOCK THIS IN)

Concertable is a multi-microservice system. Each service —

- `Concertable.Auth`
- `Concertable.B2B`
- `Concertable.Customer`
- `Concertable.Search`
- `Concertable.Payment`

— is independently developed, runs as its own Aspire AppHost in dev (`api/Concertable.X/Concertable.X.AppHost/`), and ships as its own deployable in prod.

**Services NEVER depend on each other's runtime code.** The only cross-service references are to each other's `*.Contracts` projects (integration event records + DTO contracts). In a split-repo world those ship as private NuGet packages. Anything beyond Contracts — Domain, Application, Infrastructure, Seeding — stays private to the owning service.

### Standalone is canonical

`Concertable.Customer.AppHost` running alone is the canonical Customer dev experience. The umbrella `Concertable.AppHost` is for "I want everything wired up at once," not for "Customer requires B2B to function."

If standalone Customer is broken because B2B isn't running, the fix is **never** "add B2B to Customer.AppHost." That defeats the entire point of microservice isolation. The fix is for the upstream service (B2B) to ship a "seeding simulator" — a Worker that publishes its integration events without needing its full runtime — and Customer.AppHost references that simulator as an Aspire resource.

See `api/Concertable.B2B/Concertable.B2B.Seeding.Simulator/CLAUDE.md` for the simulator pattern.

### Why this is load-bearing

This is the single fact that determines a lot of design decisions:

- Why `Concertable.B2B.X.Contracts` is the **only** cross-service project Customer references.
- Why `Concertable.Customer.Seeding` doesn't know B2B-owned IDs.
- Why we build seeding simulators instead of monolithic AppHosts.
- Why direct projection-table seeding is forbidden (the producing service owns its events; consumers receive them).

Forgetting it leads to designs that re-monolith the system. Re-read this section any time a cross-service dependency feels easier than the simulator pattern.

## Cross-service contract distribution

In the split-repo future, the dependency types map as:

| Project type | Monorepo (today) | Split-repo |
|---|---|---|
| `Concertable.X.Contracts` (events, DTOs) | `ProjectReference` | Private NuGet (`PackageReference`) |
| `Concertable.X.Seeding.Fixture` (canonical wire data) | `ProjectReference` | Private NuGet |
| `Concertable.X.Seeding.Simulator` (Worker host) | `AddProject<Projects.X>()` in AppHost | Container image, `AddContainer(...)` in AppHost |
| Shared seeding infra (`Concertable.Seeding.Shared` etc.) | `ProjectReference` from `api/Shared/` | Private NuGet |

C# code changes are minimal across the split — only csproj reference types and a single AppHost line per resource. The ownership-based folder layout (`api/Concertable.X/` for service-owned projects, `api/Shared/` for cross-service infra) previews the split.

## Folder layout

- `api/Concertable.Auth/` — Auth service (identity, OIDC, credentials).
- `api/Concertable.B2B/` — B2B service (venues, artists, concerts, contracts, bookings).
- `api/Concertable.Customer/` — Customer service (ticket purchases, reviews, preferences, projections of B2B data).
- `api/Concertable.Search/` — Search service (projections + search API).
- `api/Concertable.Payment/` — Payment service (Stripe integration, payouts).
- `api/Concertable.AppHost/` — Umbrella AppHost (runs everything).
- `api/Concertable.AppHost.Shared/` — Aspire resource registration helpers shared by every AppHost.
- `api/Shared/` — Cross-service infrastructure (Kernel, shared seeding infra, messaging contracts, etc.).
- `api/docs/` — Conventions, rules, design docs.

Each service folder contains its own `AppHost/`, `Web/`, `Workers/`, `Seeding/` (where applicable), `Modules/` (per-bounded-context), and `Tests/` (E2E + integration). Service-level `ARCHITECTURE.md` files describe each service's internal structure.

## Related docs

- Root `CLAUDE.md` — top-of-context rules and pointers.
- `api/docs/SEEDING_CONVENTIONS.md` — seeding rules (never seed event-driven data, etc.).
- `api/docs/MODULAR_MONOLITH_RULES.md` — module boundary rules within a service.
- `api/Concertable.X/ARCHITECTURE.md` — per-service architecture docs.
- `api/Concertable.B2B/Concertable.B2B.Seeding.Simulator/CLAUDE.md` — the simulator pattern in detail.
