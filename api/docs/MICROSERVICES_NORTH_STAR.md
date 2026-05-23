# Microservices North Star

> **The canonical vision.** What we're building toward, principle-first. Companion to [MICROSERVICES_ARCHITECTURE.md](MICROSERVICES_ARCHITECTURE.md) (the implementation detail) and [MICROSERVICE_STEPS.md](MICROSERVICE_STEPS.md) (the migration order). Read this first — the others fill in the *how* and the *when*.

## 1. The vision in one paragraph

Concertable is **two products** built on **shared infrastructure**: a **B2B SaaS** (venue ↔ artist booking workflow, contracts, settlement — like GigPig) and a **Customer marketplace** (ticket purchase for the concerts that workflow produces). These are two distinct bounded contexts that live as **independently deployable microservices**, communicating via **events on a bus**. Each has its own DB, host, release cadence, deploy pipeline, version. They could be developed by different teams in different repos; the only reason they share one repo is solo-dev ergonomics.

## 2. Why two products, not one

B2B's `Concert` and Customer's `Concert` look like the same word but are different concepts:

- **B2B Concert** — a contracted booking with a workflow state machine (Posted → Applied → Accepted → Verified → Finished → Settled), contract terms, settlement obligations, compliance snapshot
- **Customer Concert** — an event you can buy tickets for: when, where, how much, seats remaining

Each product has different write models, different legal scope (B2B = commercial; Customer = consumer rights, CMA, refund liability), different release cadence, and would scale independently under load. Putting them in one binary forces them to share fate; splitting them keeps blast radius contained and lets B2B SaaS ship without dragging Customer code (and its consumer liability) into the production binary.

## 3. Three service categories — the rule everything hangs on

| Category | Sync calls from other services OK? | Examples |
|---|---|---|
| **Adapter service** — wraps an external concern (Stripe, identity provider, email) | **Yes** — same shape as calling the external thing directly | Payment, Auth, Notification |
| **Data service** — owns canonical domain data | **No** — distributed-monolith antipattern. Project via events. | B2B, Customer |
| **Read projection service** — event-fed denormalized store, read-only from outside | **Yes** — it's a projection, not an authority | Search |

The whole architecture is this rule. **A read projection that ever owns a write becomes a data service and loses sync-call privilege.** A data service that exposes its data via sync HTTP is a distributed monolith.

## 4. The principles that keep it from rotting

1. **Events-only between data services.** No sync HTTP, no gRPC, no shared DB between B2B and Customer. Ever.
2. **Canonical owner publishes; consumers project.** Direction follows ownership, not "B2B-first." B2B publishes venue/concert events that Customer and Search project. Customer publishes ticket/review events that B2B and Search project. The bus is bidirectional.
3. **Transactional outbox + inbox idempotency.** Each publishing service has its own outbox in its own DB; each consuming service has its own inbox in its own DB. No shared outbox infrastructure. Exactly-once *effects* from at-least-once *delivery*.
4. **Per-service database.** No shared schemas, no shared connection strings, no cross-DB joins. Migrations are per service.
5. **Auth issues identity, not roles.** Tokens carry `sub` + audience only. Each service derives operational role from token audience + its own profile-table membership. Roles never leak into the Auth tier.
6. **Service-to-service auth via OAuth2 `client_credentials`.** Same Duende, same JWT plumbing as user tokens — just a different grant type. No mTLS, no API keys, no "trust the network."
7. **Shared code lives in two csprojs only.** `Concertable.Contracts` (the wire — event types, cross-service DTOs, `ICurrentUser`, static lookups like Genre) and `Concertable.Kernel` (the framework — `BaseEntity`, `Period`, `IUnitOfWork<TContext>`, `DbContextBase`, validation helpers). No shared DB, no shared service, no third package.
8. **Each microservice is internally a modular monolith.** The discipline that got us here (`IXModule` facades, per-module logical boundaries, in-process domain events, module-owned EF configs, `MODULAR_MONOLITH_RULES.md`) stays inside each service. Inside B2B, `Concert` and `Booking` and `Contract` remain separate modules with their own DbContext schemas and facades — they just happen to share a process and a DB. The pattern shrinks to the scope of each service; it doesn't go away. Future sub-extraction (e.g., Contract becoming its own service) stays a packaging change rather than a refactor.

## 5. Target service inventory

**Six logical services. Eleven deployable hosts at end-state.**

| Service | Category | Owns |
|---|---|---|
| **B2B** | Data | Venue, Artist, Concert (workflow shape), Contract, Booking, Application, Opportunity, Settlement, Organization, Messaging, manager/admin profiles |
| **Customer** | Data | Tickets, Reviews, Preferences, customer profile |
| **Search** | Read projection | `*SearchModel`s — browse, autocomplete, detail-page reads for both audiences |
| **Payment** | Adapter | Stripe Connect refs, payment/transfer/refund ledger; sole receiver of Stripe webhooks; only service in PCI scope |
| **Auth** | Adapter | OIDC issuer (Duende). Identity authority only — `sub`, email, password hash, email-verification |
| **Notification** *(deferred extraction)* | Adapter | Email delivery — both identity (verification, password reset) and domain (booking accepted, ticket purchased) |

Plus `Concertable.AppHost` (Aspire, dev only), `Concertable.Contracts` + `Concertable.Kernel` (csprojs, not deployables), Azure Service Bus (managed broker).

## 6. Mono-repo, multiple solutions

**Mono-repo for solo-dev ergonomics.** One `git clone`. Refactor a shared contract → every service rebuilds in one go. No NuGet feed maintenance. No version-coordination headache.

**Multiple `.sln` files at end-state for VS ergonomics:** `Concertable.B2B.sln`, `Concertable.Customer.sln`, one per platform service (or grouped — taste call). A `.sln` is a VS playlist, not an architectural boundary. The same shared csproj appears in every `.sln` that needs it — that's normal in .NET mono-repos.

**Poly-repo optionality is preserved by discipline, not by `.sln` layout:**

1. No `<ProjectReference>` between B2B csprojs and Customer csprojs — only Contracts + Kernel cross the line
2. No sync HTTP between data services (events only)
3. Per-service DB, per-service migrations
4. CI architecture test (NetArchTest or similar) fails the build on boundary violations

If those four rules hold, mono-repo → poly-repo is a folder move + replacing `<ProjectReference>` with `<PackageReference>` to a private NuGet feed. No code changes, no architecture changes.

## 7. What this doc deliberately does not cover

- **Entity migration map**, communication-pattern tables, event-flow diagrams, decision log → **[MICROSERVICES_ARCHITECTURE.md](MICROSERVICES_ARCHITECTURE.md)**
- **Phase ordering, what-to-do-when, exit criteria, calendar estimates** → **[MICROSERVICE_STEPS.md](MICROSERVICE_STEPS.md)**
- **Risks and open questions** → ARCHITECTURE.md §11

This doc is target-state principles. The other two docs are the path and the detail.

---

## Background framing (non-architecture)

- **Learning project.** The Nov 2026 launch in `LAUNCH_PLAN.md` is aspirational, not a deadline. Skill development (event-driven architecture, transactional outbox, sagas, OpenTelemetry, Service Bus operations) is an explicit goal alongside any deployment.
- **B2B SaaS deploys first** (current expectation). Customer marketplace follows once microservices separation is complete; without that separation, B2B can't ship without dragging Customer code into the production binary.
- **Solo developer.** Mono-repo + Aspire AppHost are the load-bearing pieces that make multi-service development tractable for one person.
