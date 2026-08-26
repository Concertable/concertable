# Review — Plan/RepoSplit-Stage3-Hosting (PR #809)

**Review status:** `complete`
**Judgment:** `approved`
**Reviewed up to commit:** `4356e872effb397c5f053ad528156d2765fb7379`
**Security-reviewed up to commit:** `4356e872effb397c5f053ad528156d2765fb7379`

Candidate: base `24d6f0693` → head `4356e872e`, branch `Plan/RepoSplit-Stage3-Hosting`, scope `all`.
Repository-per-microservice migration — Stage 2 round-trip 2: make the four service `*.Hosting`
projects packable and consume cross-service deps as packages; relocate two `IIntegrationCommand`
wire contracts from Application to Contracts; bump `ConcertablePlatformVersion` 1202→1206.

## Layers run

- **Native / general** (correctness, reuse, simplification, efficiency, error handling) — **clean, no findings.**
- **Package topology + microservice/module boundaries** lens — one finding (F1), fixed.
- Routed standards read: `packages`/`PACKAGES.md`, `module-structure`, `microservice-boundaries`,
  `csharp-style`/`csharp-naming`, `dependency-injection`, `persistence`, `integration-testing`.
- **Security layer: ran, clean.** The repo's `merge-gate.json` classifies `Concertable.Auth`,
  `Concertable.Payment`, and `.Contracts` paths as security-sensitive; this diff touches all three, so a
  security review ran over the sensitive changes. Verdict: security-neutral — the two moved records are
  wire-identical (same `[MessageType]`, same fields), the Payment webhook handlers change only `using`
  lines (no signature-verification/secret/authorization change), `Payment.Contracts` was already packable,
  and the newly-packable `Ticket.Contracts`/`*.Hosting` expose only contract types and Aspire composition
  (no secrets, no `InternalsVisibleTo`). Marker stamped.

## Findings

- [x] **F1 (medium) — `Auth.Hosting` kept an escaping `ProjectReference` to `Auth.Contracts`.**
  `api/Concertable.Auth/src/Concertable.Auth.Hosting/Concertable.Auth.Hosting.csproj` referenced
  `..\..\..\Concertable.Auth.Contracts\...`, which resolves to `api/Concertable.Auth.Contracts/` —
  **outside** the Auth carve root (`api/Concertable.Auth/`), unlike the other three Hosting projects
  whose retained Contracts refs are intra-carve-root. Making `Auth.Hosting` packable turns that escaping
  edge into a source reference that would not resolve in a standalone Auth repo. The Auth deployable
  already documents the rule (`Concertable.Auth.csproj`: "Auth.Contracts as feed packages … never
  ProjectReferences, so Auth carves out standalone"), and the CPM pin already exists.
  **Fixed in `4356e872e`** — swapped to `<PackageReference Include="Concertable.Auth.Contracts" />`;
  `Auth.Hosting` now has zero `ProjectReference`s; `Auth.Hosting` rebuilds `-c Release` clean; inventory
  regenerated (`crossAreaEdgeCount` 93→92, `--check` green).

## Verified clean

- **BUILD1** — every packable project (`Auth/B2B/Customer/Payment.Hosting`, `Customer.Ticket.Contracts`)
  has only packable ProjectReference targets (or, for Auth.Hosting, none). No feed-absent nuspec dep.
- **`IIntegrationCommand` relocation** — `ProcessStripeWebhookCommand` and `SendTicketEmailCommand` are
  byte-faithful moves (identical `[MessageType]` strings and signatures), correct per module-structure
  (bus-queued types are wire contracts → `*.Contracts`, matching sibling `IIntegrationEvent`s). All 12
  consumer `using`s resolve; no duplicate or dangling `*.Application.Commands` import; old files deleted.
- **Pins** — every added `Concertable.*` `PackageReference` has a matching `$(ConcertablePlatformVersion)`
  = `0.1.0-alpha.0.1206` pin in its folder; 1206 confirmed on the feed for AppHost.Shared, Auth.Contracts,
  B2B.Concert/Tenant.Contracts, Shared.Email.Application.
- **Reunion carrier** — `Ticket.Contracts` keeps `Reunion` without `PrivateAssets`, matching its six
  sibling packable module Contracts (all expose `Option<>` carriers the same way).
- **Local builds** — all 5 `*.Hosting`, both moved-command `*.Contracts`, and the Payment + Customer
  `*.Web` closures build `-c Release` clean against the feed. Full-slnx build, `local-platform-pack`
  (real packability/BUILD1 gate) and the five carves run on CI.
