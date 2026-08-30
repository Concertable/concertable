---
name: concertable-dotnet-integration-testing
description: Concertable's integration-test harness — one fixture project per service each holding its own `ApiFixture`, and the shared members every fixture composes (`SqlFixture` with Testcontainers and Respawn, `TestAuthHandler` driven by `X-Test-Sub`, the `Environments` extension members that replace raw environment literals, xunit logging, `RemoveAzureServiceBus`, `IntegrationDbInitializer`, the mock webhook simulators), plus the one `SeedState` per service that `fixture.SeedState` exposes, the module and `Process` integration suites a service carries, and the local factory-seed precedents. Use when adding an integration test or fixture here, wiring shared test setup, choosing which suite a test belongs in, or dispatching an integration event in a test.
---

# Integration tests — Concertable's fixtures and shared harness

The generic standard is the `integration-testing` skill: what makes a test an integration test, the
fixture-per-service shape, lifting anything shared into the shared library, `IScoped<T>` instead of
hand-rolled scopes, one resource-grouped `<Resource><Qualifier>ApiTests` class per public resource, the
per-endpoint contract each test owes, the owning-boundary rule, and the one canonical seed-state model.
Production seeding rules are the `seeding` skill plus [`../data/SEEDING.md`](../../standards/dotnet/data/SEEDING.md), and they bind
`ITestSeeder` too.

This file is the inventory of what exists here.

## One fixture project per service

| Fixture project | Boots |
|---|---|
| `Concertable.Auth.IntegrationTests.Fixtures` | `Concertable.Auth` (Razor Pages + Duende) |
| `Concertable.B2B.IntegrationTests.Fixtures` | `Concertable.B2B.Web` |
| `Concertable.Customer.IntegrationTests.Fixtures` | `Concertable.Customer.Web` |
| `Concertable.Search.IntegrationTests.Fixtures` | `Concertable.Search.Web` |

Each holds that service's `ApiFixture` — identically named, in its own namespace. Only genuinely
service-specific wiring stays there: Duende and Razor, Payment-in-process, Stripe fakes, per-service mocks and
seeders.

## The shared members every fixture composes

In `Concertable.Testing.Integration`, with unit-level helpers in `Concertable.Testing`:

- **`SqlFixture`** — Testcontainers SQL plus Respawn.
- **`TestAuthHandler`**, via `services.AddTestAuthentication()`. Authenticate a request with `X-Test-Sub` and
  optionally `X-Test-Email`. No token, no `role` claim.
- **`Environments.Integration` / `.E2E`** to set, `env.IsIntegration()` / `env.IsE2E()` to check — extension
  members in `Concertable.Kernel` hung onto the framework's `Environments` and `IHostEnvironment`. Never a raw
  environment literal.
- **`services.AddXunitLogging(accessor)`** — routes host logs to the current xunit output.
- **`services.RemoveAzureServiceBus()`** — drops the ASB receivers and swaps `IBusTransport` for
  `MockBusTransport`. Omit it in a service with no bus (Search).
- **`IntegrationDbInitializer`** — migrates inbox and outbox, then migrates and seeds every registered
  `ITestSeeder`. Register as `services.AddScoped<IDbInitializer, IntegrationDbInitializer>()`.
- **`MockWebhookSimulator` / `MockWebhookSimulatorFail`** — dispatch `PaymentSucceededEvent` and
  `PaymentFailedEvent` straight to the registered `IIntegrationEventHandler` implementations in a new scope,
  bypassing HTTP.

Assertions use Shouldly; on a failing status the message carries URL, status and response body.

Dispatch integration events through `IScoped<IEnumerable<IIntegrationEventHandler<TEvent>>>` — the Kernel
abstraction. The copy in `Concertable.DataAccess` is a temporary compatibility surface.

Derive expectations from `fixture.SeedState`, and `fixture.SeedNow` for the clock — never invented literals.

## How a service's integration tests split into two suites

| Suite | Owns |
|---|---|
| `Concertable.<Service>.<Module>.IntegrationTests` | that module's public API, via `<Module>ApiFixture` |
| `Concertable.<Service>.Process.IntegrationTests` | cross-module `<Journey>JourneyTests`, `ProcessApiFixture` |

The journey suite is the process integration tier's home here, and it sits beside the service's fixtures
project rather than inside a module, because it belongs to no single module.

## Where each service's `SeedState` lives, and what it reaches

Each service has exactly one `SeedState`, in its `Concertable.<Service>.Seed.Infrastructure` project beside
the seed factories, holding the **real seeded entities** (`ApplicationEntity`, `BookingEntity`,
`ConcertEntity`, …). `ApiFixture` DI-resolves it and exposes it directly as `fixture.SeedState`, which
therefore reaches every module's entities in the service, while a fixture resolves only its own module's
contexts.

## The local precedents for the seeding shapes

Factory `Seed` statics and sentinel guards are the `seeding` skill. The precedents to copy here are
`CredentialFactory.Seed` versus `CredentialFactory.Create` — the reflection stamp is
`EntityReflectionExtensions.With(...)` — and the admin user id as the sentinel that `SeedIfEmptyAsync` callers
guard on.
