# Concertable.Testing.Integration

Shared integration-test infrastructure. This is a reusable library — treat it like one.

## What belongs here

Only add something here if it is used by **two or more microservices**. Current consumers: B2B, Customer, Search.

- `SqlFixture` — Testcontainers MsSql + Respawn reset
- `TestAuthHandler` — injects `sub` / `role` claims via request headers
- `IResettable` — marker interface for mocks that flush state between tests
- `Mocks/MockBusTransport` — no-op `IBusTransport` (suppresses real ASB)
- `Mocks/MockEmailSender` / `IMockEmailSender` — captures sent emails, exposes `Sent` list
- `Mocks/MockCustomerPaymentClient` — stub `ICustomerPaymentClient` returning fixed pi_mock values
- `Mocks/MockGeocodingService` / `MockGeocodingServiceFail` — stub geocoding
- `Mocks/MockImageService` — stub image upload/replace/delete

## What does NOT belong here

If something is only used by one microservice it goes in that service's own fixture library, not here.

| Type of thing | Where it lives |
|---|---|
| `ApiFixture` for a specific service | `Concertable.Testing.Integration.<Service>` |
| Service-specific mocks (Stripe, webhook simulators, notification) | `Concertable.Testing.Integration.<Service>/Mocks/` |
| Service-specific DB initializers / seeders | `Concertable.Testing.Integration.<Service>` |
| Test collection definitions | `Concertable.Testing.Integration.<Service>` |

## Layout of per-service fixture libraries

Each microservice has its own fixture library that **references this shared project**:

```
Tests/
  Concertable.Testing.Integration/          ← shared (this project)
  Concertable.Testing.Integration.B2B/      ← B2B-only fixture, Stripe/webhook mocks
  Concertable.Testing.Integration.Customer/ ← Customer-only fixture
  Concertable.Testing.Integration.Search/   ← Search-only fixture
```

The per-service libraries keep the `Concertable.Testing.Integration` namespace so existing global
usings in module test projects continue to work without change.
