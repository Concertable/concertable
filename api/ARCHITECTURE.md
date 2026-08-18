# Concertable Backend Architecture (`api/`)

**The topology itself is not in this repo.** Which services exist, which are adapters versus data
services, what each may depend on, the surface each exposes, and why standalone-is-canonical are the
`concertable-microservice-boundaries` skill. The package closure, the carve gates, `UseLocalCore` and the
publish-then-sync loop are `concertable-packages`. The seed simulator and the forbidden-table roster are
`concertable-seeding`.

That is deliberate: the `api/` node disappears when the services split into their own repos, so anything
that would have to survive the split cannot live under it. What stays here is this monorepo's own folder
layout, which is exactly the thing the split removes.

For the system-wide premise — why this is a monorepo and what the eventual split-repo world looks like —
read the root [`ARCHITECTURE.md`](../ARCHITECTURE.md).

## The one thing to read before crossing a service boundary

**Services never depend on each other's runtime code.** The only cross-service references are to each
other's `*.Contracts` projects — integration event records and DTO contracts. Anything beyond Contracts
(Domain, Application, Infrastructure, Seeding) stays private to the owning service.

This single fact determines a lot of design decisions: why `Concertable.B2B.X.Contracts` is the only
cross-service project Customer references, why `Concertable.Customer.Seed` does not know B2B-owned ids,
why we build seeding simulators instead of monolithic AppHosts, and why direct projection-table seeding is
forbidden. Forgetting it leads to designs that re-monolith the system.

Re-read `concertable-microservice-boundaries` any time a cross-service dependency feels easier than the
simulator pattern.

## Folder layout

- `api/Concertable.Auth/` — Auth service (identity, OIDC, credentials).
- `api/Concertable.B2B/` — B2B service (venues, artists, concerts, contracts, bookings).
- `api/Concertable.Customer/` — Customer service (ticket purchases, reviews, preferences, projections of
  B2B data).
- `api/Concertable.Search/` — Search service (projections + search API).
- `api/Concertable.Payment/` — Payment service (Stripe integration, payouts).
- `api/Concertable.AppHost/` — umbrella AppHost (runs everything; the only host that gates cross-service
  startup with `WaitFor`).
- `api/Concertable.AppHost.Shared/` — generic cross-service Aspire helpers shared by every AppHost
  (SQL/ServiceBus/Storage/topology/secrets — references only, never cross-service `WaitFor`).
- `api/Concertable.Frontend.Hosting/` — Aspire composition for the frontend surfaces, consumed by the
  umbrella, B2B and Customer AppHosts.
- `api/Concertable.Shared/` — cross-service infrastructure (Kernel, shared seeding infra, messaging
  contracts).

Each service folder contains its own `AppHost/`, `Web/`, `Workers/`, `Seeding/` (where applicable),
`Modules/` (per bounded context) and `Tests/`. Service-level `ARCHITECTURE.md` files describe each
service's internal structure.

**Local prereq:** building any solution that consumes the org feed needs a `GITHUB_PACKAGES_TOKEN` PAT
with `read:packages` in the environment (see root `README.md`). CI uses the repo `GITHUB_TOKEN`.

## Related docs

- Root [`ARCHITECTURE.md`](../ARCHITECTURE.md) — the system-wide premise (monorepo-of-convenience,
  split-repo future).
- Root [`AGENTS.md`](../AGENTS.md) — top-of-context rules and pointers.
- [`AGENTS.md`](./AGENTS.md) — the backend floor.
- `api/Concertable.X/ARCHITECTURE.md` — per-service architecture docs.
- [`Concertable.B2B/src/Seed/Concertable.B2B.Seed.Simulator/AGENTS.md`](./Concertable.B2B/src/Seed/Concertable.B2B.Seed.Simulator/AGENTS.md)
  — the simulator pattern in detail.
