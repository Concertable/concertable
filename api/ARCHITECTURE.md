# Concertable Backend Architecture (`api/`)

This is the backend (`.NET`) architecture. For the system-wide premise — why this is a
monorepo, what the eventual split-repo world looks like, and the rules that follow from
treating every service as independently owned — read the root [`ARCHITECTURE.md`](../ARCHITECTURE.md)
**first**. This file describes *how* the backend realises that premise.

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

See `api/Concertable.B2B/Seed/Concertable.B2B.Seed.Simulator/CLAUDE.md` for the simulator pattern.

### Adapter services may be depended on; data services may not depend on each other

There are two kinds of service, and they get two different rules:

- **Adapter services — `Auth`, `Payment`, `Notification`** — are shared runtime dependencies, present
  in every host. A data service may call them synchronously (gRPC) and may `WaitFor` them at startup.
  So `WaitFor(auth)` / `WaitFor(paymentWeb)` live in the shared `Concertable.AppHost.Shared` helpers
  and apply in **every** host — B2B and Customer each genuinely require Auth + Payment to run.
- **Data services — `B2B`, `Customer`, `Search`** — must NEVER depend on each other's runtime. **B2B
  and Customer require Payment + Auth, but never each other.** A data service `WaitFor`-ing another
  data service is the bug to never introduce; cross-data-service communication is `*.Contracts`
  events only. When a standalone host lacks another data service's events at seed time, a
  `*.Seed.Simulator` replays them (see below) — you never run the other data service to fix it.

The litmus test for a standalone host: it may wait on **adapter** services, but a B2B developer must
never have to stand up Customer (nor a Customer developer B2B) to run their own. `WithReference(x)`
(inject x's service-discovery URL) is always fine; `WaitFor(x)` (gate startup on x being healthy) is
for adapter dependencies only.

Note that real `Payment` only emits `PaymentSucceededEvent` for a live Stripe webhook, never for seed
data. Payment is an agnostic adapter that always runs, so it does **not** own a seed catalog or a
simulator — the seed-only state its events would have produced (B2B `TicketsSold`, Customer `Ticket`
rows) is inherently unreproducible historical data and is reflection-seeded on each consumer's own
side instead (see "Producer seed libraries" below).

### Why this is load-bearing

This is the single fact that determines a lot of design decisions:

- Why `Concertable.B2B.X.Contracts` is the **only** cross-service project Customer references.
- Why `Concertable.Customer.Seed` doesn't know B2B-owned IDs.
- Why we build seeding simulators instead of monolithic AppHosts.
- Why direct projection-table seeding is forbidden (the producing service owns its events; consumers receive them).

Forgetting it leads to designs that re-monolith the system. Re-read this section any time a cross-service dependency feels easier than the simulator pattern.

## Producer seed libraries point downward only (read before touching any `*.Seed.Contracts`)

A **data service** that other data services project from owns two seed projects, and they obey the same
dependency direction as everything else — **consumer → producer, never the reverse**:

- **`Concertable.X.Seed.Contracts`** — the producer's canonical seed data (the `XSeedSpec` records +
  their `ToEvent()` mappers). Ships as a private NuGet in the split-repo world. Anyone who needs X's
  seed data — X's own seeders, downstream consumers' projection-test seeders, X's simulator — references
  **this**. It is referenced by consumers; it references **none of them**.
- **`Concertable.X.Seed.Simulator`** — a Worker that replays `Seed.Contracts` onto the bus on startup
  then exits. Registered as an Aspire resource in the AppHosts that need X's events.

`B2B` is the live example: `Concertable.B2B.Seed.Contracts` is referenced by Customer (its downstream
consumer) and by `Concertable.B2B.Seed.Simulator`, which exists because **Customer runs without B2B's
server** and must replay B2B's events to build its projections.

Two rules people (and AIs) keep getting wrong:

1. **A producer's `Seed.Contracts` must never reference a consumer's.** `B2B.Seed.Contracts` references
   none of its consumers; Customer references it because Customer is *downstream* of B2B. If producer
   seed data ever needs a foreign id it doesn't own, it declares it as a literal/opaque value — it does
   **not** import a consumer's catalog to resolve it.

2. **A `*.Seed.Simulator` is only for peer data services that don't run each other's servers — never for
   an agnostic adapter.** The B2B simulator exists because Customer runs *without* B2B. **Payment is an
   adapter, not a data service**: every host runs real Payment, so nothing it would emit is ever
   "missing" for a structural reason. Payment therefore owns **no** seed catalog and **no** simulator —
   parking a catalog of ticket *purchases* in Payment would also invert the graph (purchase semantics
   live in the B2B/Customer consumers that read `PaymentSucceededEvent.Metadata`, not in agnostic
   Payment; see `plans/PAYMENT_AGNOSTIC_AUDIT.md`).

   The one thing Payment's events would otherwise have produced is seed-only payment-derived state — B2B
   `ConcertEntity.TicketsSold` and Customer `TicketEntity` rows for *past-dated* concerts. Real Payment
   never emits for seed data (only live Stripe webhooks), and you can't buy a ticket to a concert that
   already happened, so this is **inherently unreproducible historical state**. Each consumer
   reflection-seeds its own copy directly (a documented exception in `docs/SEEDING_CONVENTIONS.md`) —
   no Payment-owned simulator. (In the umbrella host, StripeCli drives real test-mode payments for
   *live* flows; seed-only historical state is still reflection-seeded.)

## Cross-service contract distribution

**This is executed, not aspirational.** Every backend service builds from its **own published
package closure**: it consumes the shared platform and cross-service contracts as private NuGet
`PackageReference`s from the org feed `https://nuget.pkg.github.com/Concertable`, **not** via
`ProjectReference`s reaching into sibling folders. Carving any service into its own tree (or repo)
produces a build that restores and compiles. The dependency types map as:

| Project type | Monorepo (today) | Split-repo |
|---|---|---|
| `Concertable.X.Contracts` (events, DTOs) | **`PackageReference`** (feed) | Private NuGet — unchanged |
| `Concertable.X.Seed.Contracts` (canonical seed data) | **`PackageReference`** (feed) | Private NuGet — unchanged |
| Shared platform + seeding infra (`Kernel`, `Messaging.*`, `Seed.Shared`, …) | **`PackageReference`** (feed) | Private NuGet — unchanged |
| `Concertable.X.Seed.Simulator` (Worker host) | `AddProject<Projects.X>()` in AppHost | Container image, `AddContainer(...)` in AppHost |

Within a service, intra-folder references stay `ProjectReference`. Two layers are **exempt** from the
package boundary and keep their cross-folder `ProjectReference`s by design: the **AppHosts**
(dev-composition — they reference sibling deployables to orchestrate the topology) and the
**full-stack E2E/integration test harnesses** (test-composition — they boot multiple services
together). A service's *deployable closure* must be package-clean; its AppHost and test harness need
not be — until the deployment effort turns those refs into `AddContainer` / a containerised E2E topology.

The split-repo step is now nearly a no-op: the csproj reference types are already what they need to
be, so a `subtree split` of `api/Concertable.X/` restores from the (org-scoped, split-surviving) feed.
What does *not* survive a split automatically: the root `publish-packages.yml` (GitHub Actions is
repo-root-only — each separated repo gets its own smaller publish workflow), and cross-repo *restore*
needs the org packages made internal or a `read:packages` PAT.

### Per-folder build closures — never repo-root config

Each service folder and `api/Shared/` carries its **own** `nuget.config`, `Directory.Packages.props`
(CPM), and `Directory.Build.props`/`.targets`. There is deliberately **no** repo-root or `api/`-root
version/build config: a carve takes only the service folder, so any config above it would be left
behind and break the standalone restore. Don't add a root `Directory.Packages.props` ("the monorepo
idiom") — it is the trap this separation exists to avoid.

### Hybrid inner loop for the churny core (`UseLocalCore`)

The churny shared core (`Concertable.Kernel`, `Concertable.Messaging.*`) is consumed as packages by
default — required for the standalone carve + CI, which have no sibling source on disk. But because
B2B↔Customer co-change with the core constantly, packaging it would force a publish→consume cycle even
in local dev. So B2B and Customer support a **hybrid inner loop**: pass `-p:UseLocalCore=true` (or set
`CONCERTABLE_LOCAL_CORE=1`) to swap the core packages for in-repo `ProjectReference`s for fast
cross-cutting local dev against uncommitted core changes — no publish/restore round-trip. The swap is
implemented centrally in each folder's `Directory.Build.targets` (`ChurnyCorePackage` = id→path source
of truth). **Never set `UseLocalCore=true` in committed config** — it breaks the carve.

### How separation is enforced

- **Build-time guardrail (fast-fail, local + CI).** Each service folder's `Directory.Build.targets`
  fails the build if any deployable-closure project gains a `ProjectReference` escaping the service
  folder. AppHost/Tests projects and `UseLocalCore=true` builds are exempt (`EnforceServiceBoundary`).
- **Carve CI gates.** `carve-{auth,payment,search,b2b,customer}` jobs in `.github/workflows/test.yml`
  `git archive` each service folder, restore from the feed, and build the closure standalone — so an
  escaping reference (or a missing package on the feed) fails CI as a project-not-found.

**Local prereq:** building any solution that consumes the feed (every backend AppHost does, via Auth)
needs a `GITHUB_PACKAGES_TOKEN` PAT with `read:packages` in the environment (see root `README.md`). CI
uses the repo `GITHUB_TOKEN`.

C# code changes are minimal across the split. The ownership-based folder layout (`api/Concertable.X/`
for service-owned projects, `api/Shared/` for cross-service infra) previews the split.

## Folder layout

- `api/Concertable.Auth/` — Auth service (identity, OIDC, credentials).
- `api/Concertable.B2B/` — B2B service (venues, artists, concerts, contracts, bookings).
- `api/Concertable.Customer/` — Customer service (ticket purchases, reviews, preferences, projections of B2B data).
- `api/Concertable.Search/` — Search service (projections + search API).
- `api/Concertable.Payment/` — Payment service (Stripe integration, payouts).
- `api/Concertable.AppHost/` — Umbrella AppHost (runs everything; the only host that gates cross-service startup with `WaitFor`).
- `api/Concertable.AppHost.Shared/` — Aspire resource registration helpers shared by every AppHost (topology/references only — never cross-service `WaitFor`).
- `api/Shared/` — Cross-service infrastructure (Kernel, shared seeding infra, messaging contracts, etc.).
- `api/docs/` — Conventions, rules, design docs.

Each service folder contains its own `AppHost/`, `Web/`, `Workers/`, `Seeding/` (where applicable), `Modules/` (per-bounded-context), and `Tests/` (E2E + integration). Service-level `ARCHITECTURE.md` files describe each service's internal structure.

## Related docs

- Root [`ARCHITECTURE.md`](../ARCHITECTURE.md) — the system-wide, app-global premise (monorepo-of-convenience, split-repo future).
- Root `CLAUDE.md` — top-of-context rules and pointers.
- `api/docs/SEEDING_CONVENTIONS.md` — seeding rules (never seed event-driven data, etc.).
- `api/docs/MODULAR_MONOLITH_RULES.md` — module boundary rules within a service.
- `api/Concertable.X/ARCHITECTURE.md` — per-service architecture docs.
- `api/Concertable.B2B/Seed/Concertable.B2B.Seed.Simulator/CLAUDE.md` — the simulator pattern in detail.
- `plans/PAYMENT_AGNOSTIC_AUDIT.md` — why Payment depends on no consumer.
