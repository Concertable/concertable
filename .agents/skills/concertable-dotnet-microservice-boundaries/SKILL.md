---
name: concertable-dotnet-microservice-boundaries
description: Concertable's service roster and what each may depend on — the five services and which are adapters (Auth, Payment) versus data services (B2B, Customer, Search), why a data service may never `WaitFor` another data service, standalone-is-canonical and the seed simulator that makes it work, the surface each service actually exposes with Payment the only live gRPC host today, and why Payment owns no seed catalog. Use when designing anything that crosses a service boundary here, adding a startup dependency or health wait, or when a cross-service dependency starts to feel easier than the simulator.
---

# Service boundaries — Concertable's service roster

The generic standard is the `microservice-boundaries` skill: the two kinds of service, the protocol decision
table, and the traps of serving gRPC and HTTP from one host. This file is which services exist here, which
kind each one is, and what surface each actually exposes.

## The five services

Each is independently developed, runs as its own Aspire AppHost in dev
(`api/Concertable.X/Concertable.X.AppHost/`), and ships as its own deployable.

| Service | Kind | Owns |
|---|---|---|
| `Concertable.Auth` | Adapter | identity, OIDC, credentials |
| `Concertable.Payment` | Adapter | Stripe integration, payouts |
| `Concertable.B2B` | Data | venues, artists, concerts, contracts, bookings |
| `Concertable.Customer` | Data | ticket purchases, reviews, preferences, projections of B2B data |
| `Concertable.Search` | Data | projections and the search API |

**Services never depend on each other's runtime code.** The only cross-service references are to each other's
`*.Contracts` projects — integration event records and DTO contracts. Anything beyond Contracts (Domain,
Application, Infrastructure, Seeding) stays private to the owning service.

## Adapter services may be depended on; data services may not depend on each other

- **Adapter services are shared runtime dependencies present in every host.** A data service may call them
  synchronously over gRPC and may `WaitFor` them at startup, so `WaitFor(auth)` and `WaitFor(paymentWeb)` live
  in the shared `Concertable.AppHost.Shared` helpers and apply in every host. B2B and Customer each genuinely
  require Auth and Payment to run.
- **Data services must never depend on each other's runtime.** B2B and Customer require Payment and Auth, but
  never each other. Cross-data-service communication is `*.Contracts` events only. A data service
  `WaitFor`-ing another data service is the bug to never introduce.

The litmus test for a standalone host: it may wait on **adapter** services, but a B2B developer must never
have to stand up Customer, nor a Customer developer B2B, to run their own. `WithReference(x)` — inject x's
service-discovery URL — is always fine; `WaitFor(x)` — gate startup on x being healthy — is for adapter
dependencies only.

## Shared code is the intersection, never the union

The generic rule is the `microservice-boundaries` skill; this is where it bites here. `Concertable.Kernel`
and `Concertable.Contracts` (and every `Concertable.Shared.*` library) are consumed by **every** service, so a
member only ever populated or meaningful for one audience is dead weight for the others and does not belong on
a shared type — not a property, method, enum case, or claim accessor. A shared *container* like `ICurrentUser`
legitimately living in `Kernel` does not license adding audience-specific *members* to it; the package a shared
utility carries is a separate question (`Concertable.Kernel` referencing `Refit.HttpClientFactory` for the
token client is fine — the rule is about concepts on types, not about a utility dependency).

When a shared adapter needs an audience-specific value, either the **caller** resolves it and passes it in, or
it lives in a **separate abstraction only the services with the concept depend on** — e.g. `ITenantContext`,
declared beside `ICurrentUser` so the shared save-interceptor can read it, but implemented and depended on by
B2B alone. This is the same split `MULTITENANCY.md` applies to the tenant marker versus B2B's filtered stances.

**The anti-pattern to never reintroduce — a tenant/owner key on the shared `ICurrentUser`.** An `Owner` member
and a `GetOwnerId()` extension once lived in `Kernel` for exactly this and were removed. As resolved: Payment
reads the opaque `owner` claim at its own HTTP boundary (`Concertable.Payment.Api.Identity.ICurrentPayoutOwner`,
fail-closed); Customer mints `owner` and calls those endpoints directly; B2B no longer mints `owner` — its
`Tenant.Api/StripeAccountController` fronts Payment's payout operations over gRPC, passing the active tenant id
explicitly from its request-scoped `ITenantContext`, never the shared identity type.

## Standalone is canonical

`Concertable.Customer.AppHost` running alone is the canonical Customer dev experience. The umbrella
`Concertable.AppHost` is for "I want everything wired up at once", not for "Customer requires B2B to
function".

If standalone Customer is broken because B2B is not running, the fix is **never** to add B2B to
`Customer.AppHost`. That defeats the isolation. The fix is for the upstream service to ship a seeding
simulator — a Worker that publishes its integration events without needing its full runtime — which
`Customer.AppHost` references as an Aspire resource. The seeding half of that is
[`../data/SEEDING.md`](../../standards/dotnet/data/SEEDING.md).

## A consumer holds purchase-time snapshots, never a navigation chain back

Customer entities carry by-value copies of the B2B fields they need at the moment they are written —
`TicketEntity` holds `ConcertName`, `ArtistName`, `VenueName` and `Price` alongside the ids. They must never
reach back into B2B through a navigation chain to resolve those fields at read time, which would make
Customer's own reads depend on B2B's runtime and its current state rather than what was agreed. The same
shape appears inside B2B for `ContractEntity`, frozen at Accept.

## Why this is load-bearing

This single fact determines a lot of design decisions: why `Concertable.B2B.X.Contracts` is the only
cross-service project Customer references, why `Concertable.Customer.Seed` does not know B2B-owned ids, why
we build seeding simulators instead of monolithic AppHosts, and why direct projection-table seeding is
forbidden. Forgetting it leads to designs that re-monolith the system. Re-read this any time a cross-service
dependency feels easier than the simulator pattern.

## The surface each service actually exposes

**Payment is currently the only gRPC surface:** `Concertable.Payment.Client/Protos/payment.proto` is the only
`.proto`, and `AddGrpc`/`MapGrpcService` appear only in `Concertable.Payment.Web`. Every other internal
surface below is the target a service gets when it first needs a synchronous internal caller — never
something to assume is already there.

| Service | Internal surface | Edge / external surface |
|---|---|---|
| B2B | gRPC *(target — none today)* | HTTP — the public SPA APIs |
| Customer | gRPC *(target — none today)* | HTTP — the `Customer.Web` SPA |
| Search | gRPC for internal queries *(target — none today)* | HTTP — customer-facing search UI |
| Payment | gRPC — B2B/Customer sync calls, **live** | HTTP — the Stripe webhook |
| Auth | — | HTTP — OIDC/OAuth via Duende, spec-mandated |

`Concertable.Shared.Notification` is a **library, not a service**: it has no host, and nothing can `WaitFor`
it.

Payment is the standing both-protocols host — gRPC for B2B and Customer plus the Stripe HTTP webhook in the
same Kestrel app. `ITokenService`/`ClientCredentialsTokenService` in `Concertable.Kernel` mints the
`client_credentials` bearer token that goes on gRPC call metadata.

`AddPaymentClient` in `Concertable.Payment.Client` is the only place that wiring exists: it registers the
five generated stubs — `ManagerPayment`, `CustomerPayment`, `Escrow`, `PayoutAccount`, `CommissionPricing` —
against `services:payment-web:https:0`, each with its own `AddCallCredentials` callback resolving
`ITokenService` and asking for the same **`payment:write`** scope. One scope covers all five today, so a new
stub added to `payment.proto` needs the same callback and nothing more; splitting the scope means changing
five registrations, which is the moment to factor the callback out rather than paste a sixth.

## Payment emits nothing for seed data

Real Payment emits `PaymentSucceededEvent` only for a live Stripe webhook. It is an agnostic adapter that
always runs, so nothing it would emit is ever missing for a structural reason — which is why it owns no seed
catalog and no simulator. The seed-only state its events would have produced is handled per consumer; see
[`../data/SEEDING.md`](../../standards/dotnet/data/SEEDING.md).
