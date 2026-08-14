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
- Env renamed `"Testing"` → `"Integration"` across all five services' hosts (24 `IsEnvironment` sites +
  B2B's appsettings); fixtures use `Environments.Integration`. Production checks stay literals (a shared
  owner needs the pinned platform package) — the remaining bit of the `api/TECH_DEBT.md` env-vocabulary item.
- DB-name / client-id / service-name literals hard-coded although `AuthConstants.Database`,
  `ClientIds.CustomerWeb`, `*Constants.ServiceName` already exist.

## Shared additions (`Concertable.Testing.Integration`)

- C# 14 extension members (`extension()` blocks) on the framework types — `EnvironmentsExtensions`
  (`Environments.Integration`/`.E2E`) + `HostEnvironmentExtensions` (`env.IsIntegration()`/`.IsE2E()`), reading
  like `Environments.Development` / `env.IsDevelopment()`. The env was renamed `"Testing"` → `"Integration"`
  repo-wide. The **owner** now lives in `Concertable.Kernel`; a transitional `EnvironmentsExtensions` copy stays in
  `Concertable.Testing.Integration` until the Kernel version publishes and consumers switch (below).
- `MockBusTransport` → `public` (so Auth drops its `TestBusTransport`).
- `IntegrationTestHostExtensions` — composition, each fixture calls what it needs:
  - `AddTestAuthentication(this IServiceCollection)` — the default-scheme + `AddScheme<…, TestAuthHandler>` block.
  - `AddXunitLogging(this IServiceCollection, XunitOutputAccessor)` — the logging block.
  - `RemoveAzureServiceBus(this IServiceCollection)` — receiver removal + swap `IBusTransport` → `MockBusTransport`.

Kept per-service (resist unification): Auth's Duende/Razor specifics + eager-config env-var injection,
B2B's Payment-in-process + Stripe fakes, per-service mocks/seeders, `TestEmailSender`'s `Failure`/`Token`
extras vs shared `MockEmailSender`.

## PRs

- **PR 1 (this branch):** shared additions above; migrate **Auth** (drop `TestBusTransport`; use `ClientIds`)
  and **Search** (opts out of the bus swap — proves the composition generalizes). Auth integration runs purely
  under the Integration environment: the E2E-only password-grant tests are **removed**, not quarantined in an E2E fixture (their
  credential logic is covered by the `LoginService_*` tests; the ROPC token endpoint by the E2E token-mint) —
  so there is no `virtual` env seam and no E2E in the integration project. Update `INTEGRATION_CONVENTIONS.md`
  (stale layout + the "common → shared" rule) and `Concertable.Testing.Integration/AGENTS.md` (reconcile the
  `TestAuthHandler` `role` doc bug). Delete the resolved Auth `TECH_DEBT.md` entry. *(entry already removed)*
- **Kernel env cutover (publish-first):** the `EnvironmentsExtensions` / `HostEnvironmentExtensions` extension
  members are added to `Concertable.Kernel` on PR 1's branch (producer). Once that Kernel version publishes + pins
  bump, swap the 24 production checks to `env.IsIntegration()`, point the fixtures' `UseEnvironment` at
  `Environments.Integration`, and delete the `Concertable.Testing.Integration` copy — no env literals left. Gated
  on the publish, so it's a follow-up PR.
- **PR 2:** migrate B2B + Customer onto the extensions; consider consolidating the duplicated `TestDbInitializer`
  and reconciling `MockEmailSender` with Auth's `TestEmailSender` (`Failure`/`Token`).
- Payment integration is DB-only (no host) — out of scope unless it grows an HTTP fixture.

## Progress

- [x] PR 1: shared additions (`IntegrationTestHostExtensions`, public `MockBusTransport`); Auth + Search
      migrated onto them; Auth E2E-token tests + fixture removed (no `virtual`); env renamed `"Testing"` →
      `"Integration"` across all five services; `EnvironmentsExtensions`/`HostEnvironmentExtensions` extension
      members added to `Concertable.Kernel` (producer), fixtures on the transitional `.Testing` copy; docs updated;
      Auth `TECH_DEBT` entry removed; `TestBusTransport` deleted.
- [ ] Kernel cutover (after PR 1's Kernel publishes): swap the 24 production checks to `env.IsIntegration()`;
      point fixtures' `UseEnvironment` at `Environments.Integration`; delete the `.Testing` copy → no env literals.
- [ ] PR 2: migrate B2B + Customer onto the extensions; consider consolidating the duplicated
      `TestDbInitializer` and reconciling `MockEmailSender` with Auth's `TestEmailSender`.
