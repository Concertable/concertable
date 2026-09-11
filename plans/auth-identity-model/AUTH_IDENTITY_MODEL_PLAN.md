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
consumers only see the new model once the package republishes and every consumer's pin moves past that
version.

**No automated sync bot exists any more, and `ConcertableDotNetPlatformVersion` cannot simply be bumped.**
Commit `7adedd3a0` ("Cut over platform package publishing", merged 2026-09-11 ~03:27, ~7h before Phase 1
merged) deleted `platform-sync.yml` / `platform-sync-alert.yml` and split what this repo publishes into
**retained** (service-owned: `auth`/`b2b`/`customer`/`payment`/`search` — still published from this repo,
MinVer-height-versioned as before) and **platform-dotnet** (`Kernel`, `Messaging.*`, `DataAccess.*`,
`ServiceDefaults`, `Shared.*`, …) — the latter now published from an **external** platform-dotnet repo on
its own independent train (`0.2.0-alpha.0.x` on the feed today, versus retained packages' `0.1.0-alpha.0.x`).
`ConcertableDotNetPlatformVersion` still pins *both* groups from one variable. Per
`plans/platform/PLATFORM_RELEASE_TRAINS_PROGRESS.md` (written by the same cutover): advancing it to the new
`0.2.x` platform train breaks restore for every retained service package still on `0.1.x`, so it is
**deliberately frozen at its last resolvable value (`0.1.0-alpha.0.1370`)** until retained packages get
their own train property — exactly the `ConcertablePaymentVersion` pattern that already exists for Payment
(`api/Concertable.Shared/TECH_DEBT.md`: Payment's height isn't monotonic with the platform's either).

**Consequence for this plan:** `Concertable.Auth.Contracts` publishes at `0.1.0-alpha.0.1383`, but no
consumer can see it through `ConcertableDotNetPlatformVersion` — that pin must stay put. Phase 2 needs its
own `ConcertableAuthVersion` property (mirroring `ConcertablePaymentVersion`) in every consumer's
`Directory.Packages.props`, with `PackageVersion Include="Concertable.Auth.Contracts"` retargeted to it.
Pin refresh going forward is **Renovate**, weekly (`renovate.json`, `automerge: false`) for the shared
platform train; `ConcertableAuthVersion` is this repo's own to bump per-need, same as
`ConcertablePaymentVersion` today.

`TreatWarningsAsErrors` is set nowhere in the repo, so the `[Obsolete]` classes produce **warnings, not
errors** — nothing forces Phase 2's timing.

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
- **Extension members must be C#14 `extension()` blocks with property members, not legacy `this`-parameter
  methods.** `InteractiveClients`/`AuthScopes`/`AuthResources`/`ServiceClients` as landed on this branch all
  use the legacy form (`scope.Id()`, `resource.Audience()`, `client.Info()`) — missed by both Phase 1 review
  rounds. `csharp-style` requires `extension()` blocks for new extension members, migrating every ordinary
  member in a touched container. **Not yet applied on this branch** — do it before merge; mechanical,
  already verified safe on a scratch branch (`extension(AuthScope scope) { public string Id => ...; }` etc.,
  50/50 `Concertable.Auth.Contracts.UnitTests` green). Every `.Id()`/`.Info()`/`.Audience()`/
  `.AcceptedScopes()`/`.IncludedClaims()` call site in this plan's consumption contract below becomes
  parenless (`.Id`, `.Info`, …) once applied.
- **Considered and declined: splitting `InteractiveClient` into separate `Browser`/`Mobile` record types**
  (one nullable `MobileScheme` field currently distinguishes the two shapes). Real trade-off, not a clear
  win: the current enum-keyed shape is what gives the compiler-checked exhaustiveness the "Enum +
  descriptor" decision above deliberately wants across the three classifying handlers; a record-per-shape
  split would need either two separate catalogs (losing one `Find(clientId)` entry point across both) or
  a discriminated union. **Correction 2026-09-11, twice:** an earlier version of this line claimed "this
  repo deliberately avoids `Dunet` in shared production" — false; Dunet is used extensively elsewhere here
  (B2B Deal lifecycle, `TYPED_RESULT_MIGRATION_ROADMAP.md`, error unions), and is this repo's correct,
  current-best tool for a genuine discriminated union — a deliberate bridge until C#'s native `union`
  keyword ships, not something to avoid. A first fix wrongly reframed the reasoning around the *published
  package* boundary instead; that's not the test either. The actual, simple reason `InteractiveClient` isn't
  a Dunet union: **it isn't a union at all.** Every case (`CustomerBrowser`, `VenueMobile`, …) carries the
  identical shape — one wire id, one optional mobile scheme — so it's a flat, homogeneous enum-keyed data
  table, correctly modeled as enum + `FrozenDictionary` catalog. Dunet is for *heterogeneous* cases (each
  carrying genuinely different fields/behavior); reach for it only when the shape actually calls for it,
  published package or not.

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
  values, `Find` miss → null, no duplicate wire ids, `IsMobile`. (`IsB2b` / `AuthParty` were removed — see
  "RESOLVED 2026-09-11" under Open questions below; this row is stale w.r.t. that removal, kept only as a
  historical record of what Phase 1 originally shipped.)

**Consumption contract** (what Phase 2 consumers will call — fixed now, not deferred). **Superseded
2026-09-11 for the three handler rows below** by the `AuthParty` resolution under Open questions — kept here
struck-through as the historical record of what this table originally specified; the current shape is in that
section, not here:

| Consumer | Call |
|---|---|
| Auth `Config.cs` `ApiScopes` | `AuthScopes.All.Select(s => new ApiScope(s.Id(), <display>))` |
| Auth `Config.cs` `ApiResources` | `AuthResources.All.Select(r => new ApiResource(r.Audience(), <display>) { Scopes = { r.AcceptedScopes().Select(s => s.Id())... }, UserClaims = { r.IncludedClaims()... } })` |
| Auth `Config.cs` clients | iterate `InteractiveClients.All`; `info.IsMobile` picks the mobile shape, `info.MobileScheme` the redirect scheme, `info.Client is InteractiveClient.E2ETest` the ROPC shape |
| ~~`TenantProvisioningHandler`~~ | ~~`if (InteractiveClients.Find(e.ClientId) is not { Party: AuthParty.Venue or AuthParty.Artist } c) return;` then `c.Party is AuthParty.Venue ? TenantType.Venue : TenantType.Artist`~~ — now `InteractiveClients.Find(e.ClientId) is not { } c \|\| c.Client.ManagerTenantType is not { } type` via B2B-local `ManagerClients` |
| ~~`CredentialRegisteredHandler`~~ | ~~`if (InteractiveClients.Find(e.ClientId) is not { IsB2b: true }) return;`~~ — now `!c.Client.IsManagerClient` via the same B2B-local `ManagerClients` |
| ~~`UserCreationHandler`~~ | ~~`if (InteractiveClients.Find(e.ClientId) is not { Party: AuthParty.Customer }) return;`~~ — now `c.Client is InteractiveClient.CustomerBrowser or InteractiveClient.CustomerMobile`, inline, Customer-local |
| B2B/Customer/Search web hosts | `options.Audience = AuthResource.B2B.Audience()` (+ `Concertable.Auth.Contracts` package ref) |
| Payment.Web | `ValidAudiences = [AuthResource.Payment.Audience()]`; `RequireClaim("scope", AuthScope.PaymentWrite.Id())` |
| `Payment.Client` | `GetTokenAsync(AuthScope.PaymentWrite.Id())` (+ `Concertable.Auth.Contracts` ref, `PrivateAssets="all"` — published package, internal use) |
| `TestTokenMinter` | `client_id` = `InteractiveClient.E2ETest.Info().Id`; scope = `string.Join(' ', new[]{ B2BApi, CustomerApi, SearchApi }.Select(s => s.Id()))` |

**Verification gate:** `Concertable.Auth.Contracts` + its unit tests build and pass, 0 warnings.

### Phase 2 — consumers + contract removal (after Phase 1 publishes)

Fresh worktree off the post-publish `origin/main`. Adds `ConcertableAuthVersion` (see Package
topology) to every consumer's `Directory.Packages.props` — Auth, B2B, Customer, Payment, Search, Shared —
pinned to `0.1.0-alpha.0.1383`, and retargets each `PackageVersion Include="Concertable.Auth.Contracts"`
onto it. `ConcertableDotNetPlatformVersion` is untouched.

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

This PR republishes `Auth.Contracts` (removal) — a non-breaking publish, since nothing outside this repo
still references the deleted classes once it merges; no further sync PR to follow (see Package topology).

## Open questions — surfaced 2026-09-11, this branch is code-complete but not yet reconciled with them

Not addressed by either Phase 1 review round.

### RESOLVED 2026-09-11: business/party classification does not belong in Auth.Contracts at all

Decision (repository owner, same evening the question was surfaced): stronger than the "move `Party` into
`Concertable.Contracts`" option originally floated below — **no shared party/classification type at all**,
in *any* package. `AuthParty` is deleted outright from `Concertable.Auth.Contracts`; `InteractiveClientInfo`
carries only identity facts (`Client`, `Id`, `MobileScheme`). Each of the three consuming handlers now
resolves the *typed* `InteractiveClient` via Auth's `InteractiveClients.Find` (identity resolution — still
legitimately Auth's job, since `CredentialRegisteredEvent.ClientId` is a raw wire string), then classifies
what that client means **locally**, in its own service:

- `TenantProvisioningHandler` / `CredentialRegisteredHandler` (B2B) read a new
  `Concertable.B2B.Infrastructure.Authorization.ManagerClients` — one `FrozenDictionary<InteractiveClient,
  TenantType?>`, C# 14 `extension()` block members (`client.IsManagerClient`, `client.ManagerTenantType`),
  matching the `InteractiveClients`/`AuthScopes`/`AuthResources`/`ServiceClients` catalog idiom already used
  4× in this codebase. One table, not two — the first attempt split it into two parallel per-handler maps,
  which is the exact "parallel hand-written maps... lets one family drift" anti-pattern the `csharp-naming`
  skill calls out; consolidated before landing.
- `UserCreationHandler` (Customer) matches `client.Client is InteractiveClient.CustomerBrowser or
  InteractiveClient.CustomerMobile` inline — single call site, no table needed (`keyed-strategies`'s
  "one exhaustive match... at one call site is not the anti-pattern" carve-out).

Rationale volunteered unprompted, not merely accepted: B2B and Customer are two standalone apps with
unrelated authorization needs: there is no product reason to centralize authorization in a shared package,
and the previous design (Auth-owned `AuthParty`) was reintroducing exactly the centralization this repo's
microservice boundaries exist to prevent. This is stronger than, and supersedes, the "move to
`Concertable.Contracts`" option originally recorded here.

**RESOLVED 2026-09-11 (repository owner):** B2B has no dedicated Authorization module. The pre-existing
`PermissionAuthorizationHandler` / `PermissionRequirement` / `IMembershipContext` (request-time policy
checks) live inside `Concertable.B2B.Tenant.Infrastructure`/`.Contracts` purely because Tenant happens to own
the membership data they read — which means *every* B2B module with a protected endpoint depends on
Tenant.Contracts for an orthogonal concern, the wrong dependency direction. The new `ManagerClients`
(registration-time role assignment — a different lifecycle moment, same underlying concern: authorization)
stays at its interim placement in `Concertable.B2B.Infrastructure/Authorization/` — already the right *local*
shape, just not the full module move. **Decision: leave the interim placement; the module inversion is
tracked as its own roadmap item, not folded into Phase 2.** Rationale: it rewires every B2B module's policy
registration — a materially bigger, separately-reviewable change than this plan's typed-identity-contract
scope — and Phase 2 is otherwise verified and ready; sequencing the rewire as its own plan is correct
splitting of delivery, not a downgrade of the fix. Logged in
[`api/Concertable.B2B/TECH_DEBT.md`](../../api/Concertable.B2B/TECH_DEBT.md) ("Authorization code is homed by
accident-of-ownership, not by a real module") with the design sketch (a genuine `Concertable.B2B.Authorization`
module: `.Contracts` — `PermissionRequirement`, a membership/role-fact facade interface Tenant *implements*;
`.Infrastructure` — `PermissionAuthorizationHandler`, `ManagerClients`; no `.Domain`/`.Api`, no persisted
aggregate of its own) and an objective resolution condition. Roadmap item
`b2b-authorization/authorization-module` in
[`plans/b2b-authorization/B2B_AUTHORIZATION_ROADMAP.md`](../b2b-authorization/B2B_AUTHORIZATION_ROADMAP.md) —
the next plan to work once Phase 2 merges, not indefinitely deferred. Phase 2 can now proceed to close-out.

### Does `TestTokenMinter` belong inside the service-agnostic `Concertable.Testing.E2E` harness?

That project's own `AGENTS.md`: "SERVICE-AGNOSTIC. Nothing service-specific goes here. Ever.", with one
stated exception — pins for adapter services (Auth, Payment), because they sit in every host by
architecture. `TestTokenMinter` predates this plan and goes further than a pin: it POSTs directly to Auth's
`/connect/token`, and this branch has it resolving `InteractiveClient`/`AuthScope` directly, carrying Auth's
own identity vocabulary into the shared harness rather than opaque strings a reviewer could mistake for
generic OAuth plumbing.

**Blocks:** nothing — `TestTokenMinter` can stay on the typed model regardless of where it ends up living;
this is about *where the file sits*, not whether it compiles.

## Out of scope

- Renaming `CredentialRegisteredEvent.ClientId` or making it typed on the wire — it is a versioned message
  contract; the typed value is derived at the handler boundary.
- Frontend client-id constants (`app/`) — separate tier, separate package.
