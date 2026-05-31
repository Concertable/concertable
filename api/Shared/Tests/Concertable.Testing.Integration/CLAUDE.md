# Concertable.Testing.Integration

Shared integration-test infrastructure. This is a reusable library — treat it like one.

## What belongs here

Only add something here if it is used by **two or more microservices**. Current consumers: B2B, Customer, Search.

- `SqlFixture` — Testcontainers MsSql + Respawn reset
- `TestAuthHandler` — injects `sub` / `role` claims via request headers
- `IResettable` — marker interface for mocks that flush state between tests
- `Mocks/MockBusTransport` — no-op `IBusTransport` (suppresses real ASB)
- `Mocks/MockEmailSender` / `IMockEmailSender` — captures sent emails, exposes `Sent` list
- `Mocks/MockGeocodingService` / `MockGeocodingServiceFail` — stub geocoding
- `Mocks/MockImageService` — stub image upload/replace/delete

## What does NOT belong here

If something is only used by one microservice it goes in that service's own fixture library, not here.

| Type of thing | Where it lives |
|---|---|
| `ApiFixture` for a specific service | that service's `Concertable.<Service>.IntegrationTests.Fixtures` |
| Service-specific mocks (Stripe, webhook simulators, notification, payment client) | that service's `...IntegrationTests.Fixtures/Mocks/` |
| Service-specific DB initializers / seeders | that service's `...IntegrationTests.Fixtures` |
| Test collection definitions | that service's `...IntegrationTests.Fixtures` |

## Layout of per-service fixture libraries

This shared project lives at `api/Shared/Tests/Concertable.Testing.Integration/`. Each microservice
owns its own fixture library, located under that service, which **references this shared project**:

```
api/Shared/Tests/Concertable.Testing.Integration/            ← shared (this project)
api/Concertable.B2B/Tests/Concertable.B2B.IntegrationTests.Fixtures/             ← B2B-only fixture, Stripe/webhook mocks
api/Concertable.Customer/Tests/Concertable.Customer.IntegrationTests.Fixtures/   ← Customer-only fixture (owns MockCustomerPaymentClient)
api/Concertable.Search/Tests/Concertable.Search.IntegrationTests.Fixtures/       ← Search-only fixture
```

Each service-owned fixture uses its own root namespace (`Concertable.<Service>.IntegrationTests.Fixtures`)
and references this shared project for `SqlFixture` / `TestAuthHandler` / `IResettable` / shared mocks.
