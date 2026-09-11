# Auth identity model plan

## Outcome

`Concertable.Auth.Contracts` stops exposing flat string-constant classes (`ClientIds`, `ApiScopeIds`) and
exposes a typed model instead:

- `AuthParty` — Customer | Venue | Artist | Admin. The axis the registration handlers classify on.
- `InteractiveClient` enum + `InteractiveClientInfo` (`readonly record struct`, scalars only) +
  `InteractiveClients` frozen catalog — the browser SPAs, native apps and the E2E harness client.
  `Find(clientId) → InteractiveClientInfo?` resolves a wire id off `CredentialRegisteredEvent` (null on an
  unknown id); `client.Info()` gives the row; `All` for registration.
- `AuthScope` enum + `AuthScopes` catalog — `scope.Id()` / `All`. One home for the five scope strings.
- `AuthResource` enum + `AuthResources` extension catalog — `resource.Audience()` /
  `resource.AcceptedScopes()` / `resource.IncludedClaims()` / `All`. One home for `concertable.payment.api`
  (today duplicated Auth ↔ Payment.Web), whose audience is not one of its scopes.

Service-to-service clients (`concertable-b2b/-customer/-auth`) do **not** cross the wire and their secrets
are Auth's — a `ServiceClient` enum + catalog lives in `Concertable.Auth`, not in the published contract.

Wire strings never change (live tokens, seeded rows, external harness): the enum member names may differ
from the wire string (`CustomerBrowser` → `"customer-web"`); the catalog is the only bridge.
`CredentialRegisteredEvent.ClientId` stays `string` on the wire.

## Package topology

`Concertable.Auth.Contracts` is a published, feed-pinned package consumed by Auth, B2B, Customer, Payment,
Search and the external `Concertable.Testing.E2E` harness. Removing `ClientIds` / `ApiScopeIds` is a
breaking change (`dotnet:package-cutover`: breaking — public type removed). It cannot land in one PR:
consumers only see the new model once the package republishes and `platform-sync` bumps their pins.

`TreatWarningsAsErrors` is set nowhere in the repo, so the `[Obsolete]` classes produce **warnings, not
errors** — the post-publish `platform-sync` PR goes green on its own. Phase 2 is therefore driven
deliberately, not forced by a red gate.

## Design decisions

- **Enum + descriptor, not `readonly struct` + `TryParse` over the raw string.** The client-id set is
  closed and small, and three handlers across two services classify on it — an exhaustive `switch` on
  `AuthParty` means adding a client breaks every site that must be updated. A bare string wrapper gives no
  exhaustiveness and still needs the descriptor table.
- **`Browser` / `Mobile`, not `Web`.** `Concertable.X.Web` is the API host; `CustomerWeb` read as "the web
  API host". The OIDC client is the human-facing app, so the member names say `Browser` / `Mobile`.
- **`InteractiveClient` / `ServiceClient`** — the Duende split (interactive vs machine-to-machine).
- **`Find` returns a nullable `InteractiveClientInfo?`** rather than a `TryGet(out)` that yields a
  `default` struct on a miss: the id arrives on an integration event from Auth; version skew (a client this
  consumer's build predates) must be a graceful skip, and a nullable makes "only read it when found"
  structural. Reverse lookups on `AuthScope` / `AuthResource` had no consumer and are omitted (avoids the
  same default-value trap on a published contract); add a nullable `Find` if one ever needs it.
- **`InteractiveClientInfo.Party` is `AuthParty?`** — null only for `E2ETest`, which serves no party.
- **`AuthResources` is extension methods on the enum, not an info struct** — a struct with
  `ImmutableArray` members has a `default(T)` NRE trap and fragile equality; the collections come from the
  catalog keyed by the enum instead.

## Phases

### Phase 1 — producer: typed model in `Concertable.Auth.Contracts` (additive)

Branch `Refactor/AuthIdentityModel`. Additive only — nothing outside `Concertable.Auth.Contracts.csproj`
changes, because consumers bind the published package.

- Add `AuthParty`, `InteractiveClient`, `InteractiveClientInfo`, `InteractiveClients`, `AuthScope`,
  `AuthScopes`, `AuthResource`, `AuthResources`.
- `[Obsolete]` `ClientIds` and `ApiScopeIds` pointing at the replacements.
- Add `Concertable.Auth.Contracts.UnitTests` at `api/Concertable.Auth.Contracts/tests/` — co-located so the
  `ProjectReference` (`..\..\Concertable.Auth.Contracts.csproj`) stays inside the package folder and
  resolves identically in the `carve-auth` tree (which mounts the package at `src/Concertable.Auth.Contracts/`).
  The flat package csproj gets `<Compile Remove="tests/**/*.cs" />`; the folder gains a
  `Directory.Build.targets` importing `TestConventions.targets` and test package versions in its
  `Directory.Packages.props`. Registered in `Concertable.slnx` and added to the `carve-auth` project list in
  `.github/workflows/test.yml`. Covers: catalog completeness (every enum member has one row), wire-id
  values, `Find` miss → null, no duplicate wire ids, `IsB2b` / `IsMobile`, the `AuthParty` classification.

**Consumption contract** (what Phase 2 consumers will call — fixed now, not deferred):

| Consumer | Call |
|---|---|
| Auth `Config.cs` `ApiScopes` | `AuthScopes.All.Select(s => new ApiScope(s.Id(), <display>))` |
| Auth `Config.cs` `ApiResources` | `AuthResources.All.Select(r => new ApiResource(r.Audience(), <display>) { Scopes = { r.AcceptedScopes().Select(s => s.Id())... }, UserClaims = { r.IncludedClaims()... } })` |
| Auth `Config.cs` clients | iterate `InteractiveClients.All`; `info.IsMobile` picks the mobile shape, `info.MobileScheme` the redirect scheme, `info.Client is InteractiveClient.E2ETest` the ROPC shape |
| `TenantProvisioningHandler` | `if (InteractiveClients.Find(e.ClientId) is not { Party: AuthParty.Venue or AuthParty.Artist } c) return;` then `c.Party is AuthParty.Venue ? TenantType.Venue : TenantType.Artist` |
| `CredentialRegisteredHandler` | `if (InteractiveClients.Find(e.ClientId) is not { IsB2b: true }) return;` |
| `UserCreationHandler` | `if (InteractiveClients.Find(e.ClientId) is not { Party: AuthParty.Customer }) return;` |
| B2B/Customer/Search web hosts | `options.Audience = AuthResource.B2B.Audience()` (+ `Concertable.Auth.Contracts` package ref) |
| Payment.Web | `ValidAudiences = [AuthResource.Payment.Audience()]`; `RequireClaim("scope", AuthScope.PaymentWrite.Id())` |
| `Payment.Client` | `GetTokenAsync(AuthScope.PaymentWrite.Id())` (+ `Concertable.Auth.Contracts` ref, `PrivateAssets="all"` — published package, internal use) |
| `TestTokenMinter` | `client_id` = `InteractiveClient.E2ETest.Info().Id`; scope = `string.Join(' ', new[]{ B2BApi, CustomerApi, SearchApi }.Select(s => s.Id()))` |

**Verification gate:** `Concertable.Auth.Contracts` + its unit tests build and pass, 0 warnings.

### Phase 2 — consumers + contract removal (after Phase 1 publishes + sync)

Fresh worktree off the post-publish `origin/main` (pin already bumped by the `platform-sync` PR, or bump it
in this PR).

- Migrate every consumer per the table above.
- Add `ServiceClient` enum + catalog to `Concertable.Auth`; `AuthHostExtensions` iterates it (id + secret
  config key + granted `AuthScope`).
- Add `Concertable.Auth.Contracts` package refs: `Search.Web` (+ `Concertable.Search/Directory.Packages.props`),
  `Payment.Client`, `Concertable.Testing.E2E` (+ `Concertable.Shared/Directory.Packages.props`),
  B2B.Web / Customer.Web / Payment.Web.
- Delete `ClientIds` and `ApiScopeIds`; delete the resolved `api/Concertable.Auth/TECH_DEBT.md` entry.
- Update `api/Concertable.Auth/AGENTS.md` "Duende config is in code" note.

**Verification gate:** full affected build; Auth + B2B Tenant/User + Customer User integration/unit suites;
`grep -rniE "ClientIds|ApiScopeIds"` → zero. Wire-string grep allowlist: the literals inside
`InteractiveClients.cs` / `AuthScopes.cs` / `AuthResources.cs`, and `CredentialRegisteredEvent.ClientId`
(deliberate `string` wire field).

This PR republishes `Auth.Contracts` (removal); the following `platform-sync` is non-breaking.

## Out of scope

- Renaming `CredentialRegisteredEvent.ClientId` or making it typed on the wire — it is a versioned message
  contract; the typed value is derived at the handler boundary.
- Frontend client-id constants (`app/`) — separate tier, separate package.
