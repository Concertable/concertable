# Microservice Communication

> **Companion to** [MICROSERVICES_ARCHITECTURE.md](MICROSERVICES_ARCHITECTURE.md) and [MICROSERVICE_STEPS.md](MICROSERVICE_STEPS.md).
> Those cover *what services exist* and *what order they extract*. This one covers *how they talk*.
>
> **Rule:** the protocol is chosen by the **consumer**, not by preference. If you can name who calls an endpoint, the protocol is already decided.

---

## The decision table

| Consumer | Protocol | Why |
|---|---|---|
| Your service → your service (sync) | **gRPC** | Both sides ours, contract-first `.proto`, HTTP/2 multiplexing, smaller payloads, codegen on both ends |
| Browser / mobile / SPA (`Customer.Web`, B2B SPA) | **HTTP/JSON** | Browsers cannot open a raw gRPC connection — hard limitation, not a choice |
| Third party calling in (Stripe webhooks) | **HTTP/JSON** | Stripe dictates the protocol; it sends HTTP POSTs and always will |
| OIDC / OAuth token endpoints (Auth / Duende) | **HTTP/JSON** | OAuth is HTTP by specification |
| Event-driven / fire-and-forget | **Azure Service Bus** | Already the seam — see [MICROSERVICE_STEPS.md](MICROSERVICE_STEPS.md) Steps 8–10 |

**Default is gRPC.** HTTP appears only at the three forced boundaries above — every one is a case where something *outside our control* (a browser, Stripe, the OAuth spec) dictates the wire format.

---

## Sync communication — gRPC

For every internal service-to-service hop: B2B → Payment, Customer → Search, Customer → Payment, anything mine-to-mine behind the edge.

- One `.proto` per service is the single source of truth. Codegen the client and server from it — no hand-written contract on either side, no drift.
- Service contract surface mirrors the in-process facade: a service's gRPC surface exposes the same command/query operations its `IXModule` facade does today.
- gRPC clients are registered with `AddGrpcClient<T>()` + Aspire service discovery (see below) — logical service name, no hard-coded URLs.
- Service-to-service auth: `client_credentials` bearer token on the gRPC call metadata. `ITokenService` / `ClientCredentialsTokenService` already exist in `Concertable.Kernel` (Step 11) — inject the same token into the gRPC `CallCredentials`.

**Do not** use gRPC-Web to reach a service from a browser. Frontends go through that service's HTTP edge.

## HTTP surfaces

Where HTTP is required, the host serves it alongside gRPC in the same Kestrel app.

- **New HTTP endpoints** — Minimal APIs. Webhooks (Stripe), and any net-new edge endpoint.
- **Existing public APIs** — Controllers stay. Do not rewrite working edge endpoints for protocol consistency; they and new Minimal APIs converge over time, not urgently.
- **Consuming HTTP we don't own** — Refit typed client. This is Refit's job: external/third-party REST APIs.
- **Consuming our own HTTP internally** — don't. If both sides are ours, the call should be gRPC. Refit against our own Minimal API means maintaining two contract surfaces for one service. The only exception is a transition window before a service has its gRPC surface.

## Async communication

Unchanged from [MICROSERVICE_STEPS.md](MICROSERVICE_STEPS.md). Azure Service Bus via the `IBus` / `IBusTransport` seam handles event-driven and fire-and-forget flows (outbox/inbox, Steps 8–10). gRPC is for *synchronous request/response only* — if the caller doesn't need an answer right now, it's a message, not a gRPC call.

---

## Running gRPC + HTTP in one service

Some services need both — **Payment** is the clear case: gRPC for B2B/Customer sync calls, plus an HTTP webhook endpoint for Stripe. This is fine; Kestrel maps gRPC services and Minimal API endpoints in the same app. Watch for:

- **HTTP/2** — gRPC requires it. Over TLS this negotiates automatically (ALPN). Confirm the **production ingress speaks HTTP/2 end-to-end**, or gRPC calls won't reach the service.
- **Load balancing** — gRPC multiplexes over one long-lived connection; a naive L4 balancer pins all traffic to one backend. A gRPC-aware L7 proxy is needed for real per-call balancing.
- **Two cross-cutting surfaces** — auth, error mapping, and logging are configured once per protocol. `AddServiceDefaults()` (below) covers most of it for both.

---

## What Aspire gives us

Aspire removes the *plumbing*, not the protocol decision:

- **Service discovery** — logical names resolve from injected config. `AddServiceDiscovery()` makes both `HttpClient` and gRPC channels target `http://payment` etc. — no environment-specific URLs.
- **`AddServiceDefaults()`** — applies OpenTelemetry, health checks, and `AddStandardResilienceHandler()` uniformly to gRPC and HTTP clients alike.
- **Typed clients** — Refit clients and `AddGrpcClient` both point at the logical service name; Aspire wires the rest.

Aspire does **not** generate `.proto`, share contracts, version anything, or handle auth — those stay ours.

---

## Per-service summary

| Service | Internal surface | Edge / external surface |
|---|---|---|
| **B2B** | gRPC | HTTP (existing public SPA APIs — controllers) |
| **Customer** | gRPC | HTTP (`Customer.Web` SPA) |
| **Search** | gRPC (internal queries) | HTTP (customer-facing search UI) |
| **Payment** | gRPC (B2B/Customer sync calls) | HTTP (Stripe webhook — controller) |
| **Auth** | — | HTTP (OIDC/OAuth — Duende, spec-mandated) |
| **Notification** | gRPC *if* called synchronously; otherwise ASB only | — |
