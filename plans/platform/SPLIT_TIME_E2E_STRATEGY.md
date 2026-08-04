# Cross-service E2E: behave-as-separate strategy

The repos may not actually split for a while. **That does not matter — the system must behave as if
each service (`Auth`, `B2B`, `Customer`, `Search`, `Payment`) already lives in its own repo, now.**
This doc says what that discipline requires today versus what is genuinely inert until a second repo
exists.

## The problem (today)

`Concertable.B2B.E2ETests/AppFixture.cs` and `Concertable.Customer.E2ETests/AppFixture.cs` boot their
service's Aspire AppHost via `DistributedApplicationTestingBuilder.CreateAsync<Projects.Concertable_X_AppHost>()`,
which composes **real** Payment + Auth + Search through `Projects.Concertable_*` **source** references.

That is full-fleet E2E living *inside one service's repo*. A B2B developer in a standalone B2B repo
could not run it — it needs the rest of the world's source. That violates behave-as-separate, and it
won't compile once the services split.

## The line: discipline now, plumbing at split

The test for "do it now" is **not** "have we split yet." It's: *does this enforce the as-if-separate
discipline, or is it deployment plumbing with no function until a second repo exists?*

### Do now — structure / ownership / coupling (this is the whole point)

A service's repo must contain only tests that need only *its own* runtime. So:

- **Move full-fleet E2E out of the service test folders** (`B2B/Tests/E2ETests`,
  `Customer/Tests/E2ETests`) into a **system-level E2E project** that boots the umbrella
  `Concertable.AppHost`. At split time that project lifts wholesale into its own system/deployment repo.
- **Each service keeps integration tests only**, with adapter services faked behind their contracts —
  Payment via the existing `MockManagerPaymentClient` / `MockEscrowClient` / `MockCustomerPaymentClient`
  against `Payment.Contracts`. No other service's source or runtime required.

This needs no second repo. It is the same "folder layout previews the split" principle applied to tests.

### Inert until a second repo actually exists (building it now is dead weight)

- A CI pipeline publishing container images to a registry — nothing consumes them yet.
- `Payment.Contracts` (etc.) `ProjectReference` → `PackageReference` — there is one solution.
- `AddProject<Projects.Concertable_Payment_Web>()` → `AddContainer("payment", "<registry>/payment:<v>")`
  for the system-E2E fleet — the source is right here; swap it the day the registry image exists.

### Grey area — consumer-driven contract tests

Post-split, a consumer binds to a *published* contract version, so the compiler no longer catches a
breaking change; contract tests (e.g. Pact) do. In the monorepo the project reference means **the
compiler already is that test**. So contract tests are the one behave-as-separate safety net currently
provided for free — worth adding when you want off the compiler crutch, low marginal value before then.

## Target model (two tiers)

- **Tier 1 — per service repo, every PR:** unit + integration (adapter services faked behind contracts)
  + (eventually) contract tests. Fast, hermetic, no other service's runtime. What a lone developer runs.
- **Tier 2 — full-fleet system E2E, rare / pre-release, centralised:** the real fleet, run against real
  services. In the monorepo today: the system-E2E project booting the umbrella. Post-split: a system
  pipeline composing each service from its published **container image**. Real B2B ↔ real Payment ↔
  real Auth ↔ real Stripe — never stubbed.

## Already in place (reuse, don't rebuild)

- `Payment.Contracts` — the cross-service contract surface; becomes the published package post-split.
- `Mock*Client` fakes in each service's integration fixtures — the Tier-1 fake layer.
- `*.Seed.Simulator` + `*.Seed.Contracts` — already image-ready (`api/ARCHITECTURE.md` split table).
- The reference-vs-wait seam in `Concertable.AppHost.Shared` (adapter-service `WaitFor`s live there).

## Steps

**Now (discipline):**
1. Create a system-level E2E project (boots the umbrella `Concertable.AppHost`); move the B2B and
   Customer full-fleet E2E suites into it; share the existing `api/Shared/Tests/Concertable.E2ETests`
   infra.
2. Reduce each service's own test footprint to integration tests with the `Mock*Client` fakes; ensure
   coverage that the relocated E2E used to provide at the service level is met by integration tests.

**At split (plumbing):**
3. Each service repo: CI builds + publishes a versioned container image (GHCR).
4. `*.Contracts` publish as versioned NuGet; consumers switch `ProjectReference` → `PackageReference`;
   add consumer-driven contract tests.
5. System-E2E pipeline composes services from images (`AddProject` → `AddContainer`); the system-E2E
   project moves into its own repo.

## Related

- `api/ARCHITECTURE.md` — adapter-vs-data services; the split-repo dependency table.
- `api/Concertable.B2B/TECH_DEBT.md` / `api/Concertable.Customer/TECH_DEBT.md` — the tracked debt items.
