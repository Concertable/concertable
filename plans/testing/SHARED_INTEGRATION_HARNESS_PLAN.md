# Shared integration-test harness

Lift the setup every service integration `ApiFixture` re-hand-rolls into `Concertable.Testing.Integration`,
replace the magic strings with the constants that already exist, and write the convention down so future
PRs follow it by default. Composition (shared extension methods + constants), never a forced base class.

Branch: `Chore/TechDebt` (this is the `/techdebt` persistent branch). Shared libs are `ProjectReference`d
(not pinned), so shared additions + consumer migrations land atomically — no platform-sync.

## The duplication (from the cross-service survey)

- `TestAuthHandler` default-scheme registration block — verbatim in Auth, B2B, Customer, Search (×4).
- xunit logging block (`ClearProviders` + `XunitLoggerProvider(accessor)` + `SetMinimumLevel`) + the
  `XunitOutputAccessor` field + `AttachOutput`/`DetachOutput` — ×4.
- `"AzureServiceBusReceiver"` hosted-service removal + `Replace(IBusTransport, no-op)` — Auth, B2B, Customer (×3).
- Auth's `TestBusTransport` is a byte-identical copy of shared `MockBusTransport` (Auth isn't in the lib's
  `InternalsVisibleTo`, so it couldn't consume the internal type).
- Env names `"Testing"`/`"E2E"` as raw literals everywhere; no constant exists.
- DB-name / client-id / service-name literals hard-coded although `AuthConstants.Database`,
  `ClientIds.CustomerWeb`, `*Constants.ServiceName` already exist.

## Shared additions (`Concertable.Testing.Integration`)

- `TestEnvironments` — `public const string Testing`/`E2E`.
- `MockBusTransport` → `public` (so Auth drops its `TestBusTransport`).
- `IntegrationTestHostExtensions` — composition, each fixture calls what it needs:
  - `AddTestAuthentication(this IServiceCollection)` — the default-scheme + `AddScheme<…, TestAuthHandler>` block.
  - `AddXunitLogging(this IServiceCollection, XunitOutputAccessor)` — the logging block.
  - `RemoveAzureServiceBus(this IServiceCollection)` — receiver removal + swap `IBusTransport` → `MockBusTransport`.

Kept per-service (resist unification): Auth's Duende/Razor specifics + eager-config env-var injection,
B2B's Payment-in-process + Stripe fakes, per-service mocks/seeders, `TestEmailSender`'s `Failure`/`Token`
extras vs shared `MockEmailSender`.

## PRs

- **PR 1 (this branch):** shared additions above; migrate **Auth** (fold in the Testing/E2E split already
  done; drop `TestBusTransport`; use `AuthConstants`/`ClientIds`) and **Search** (opts out of the bus swap —
  proves the composition generalizes). Update `INTEGRATION_CONVENTIONS.md` (stale layout + the "common → shared"
  rule) and `Concertable.Testing.Integration/AGENTS.md` (reconcile the `TestAuthHandler` `role` doc bug, point
  at the shared setup). Delete the resolved Auth `TECH_DEBT.md` entry. *(entry already removed)*
- **PR 2:** migrate B2B + Customer onto the extensions; consider consolidating the duplicated `TestDbInitializer`
  and reconciling `MockEmailSender` with Auth's `TestEmailSender` (`Failure`/`Token`).
- Payment integration is DB-only (no host) — out of scope unless it grows an HTTP fixture.

## Progress

- [x] PR 1: shared additions (`TestEnvironments`, `IntegrationTestHostExtensions`, public `MockBusTransport`);
      Auth + Search migrated; `INTEGRATION_CONVENTIONS.md` + shared-lib `AGENTS.md` updated; Auth `TECH_DEBT`
      entry removed; `TestBusTransport` deleted. Built green (Auth + Search integration projects).
- [ ] PR 2: migrate B2B + Customer onto the extensions; consider consolidating the duplicated
      `TestDbInitializer` and reconciling `MockEmailSender` with Auth's `TestEmailSender`.
