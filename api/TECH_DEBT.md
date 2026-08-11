# Concertable — cross-cutting technical debt

Debt spanning multiple services, host `Program.cs` files, or repo-wide build/CI config. Debt inside the shared platform tree (`Concertable.Kernel`, `Concertable.Shared.*`, the shared test libs) belongs in [`Concertable.Shared/TECH_DEBT.md`](./Concertable.Shared/TECH_DEBT.md); service-specific debt belongs in that service's own `TECH_DEBT.md`. When an item is fixed, update both this file and [`ARCHITECTURE.md`](./ARCHITECTURE.md).

---

## MED

### Repository bases repeat CRUD, and read no-tracking is a bypassable `Query` convention

The shared `Concertable.DataAccess.Infrastructure` repository bases duplicate `GetByIdAsync` /
`GetAllAsync` / `Exists` across `ReadRepository<>` and `Repository<>` (plus concrete overrides). The
*only* real difference is tracking: read reads go through the no-tracking `Query` root, write reads
through the tracked `context.Set<T>()`. And `Query` enforces nothing — a read repo can still call
`context.Foo` directly and get a **tracked** query; nothing stops it. So it's a convention, not a
guarantee, and the duplication only exists because tracking lives on the query.

**Resolves when:** no-tracking becomes a property of the **context**, not the query. Read repositories
sit on a read-only, no-tracking context (the `PublicDbContext` shape — `SaveChanges` throws — already
exists), so `context.Foo` is no-tracking by construction and can't be bypassed, and `Query` is
deleted. With tracking off the query, read/write `GetById`/`GetAll`/`Exists` become identical, so the
bases collapse to one CRUD implementation exposed through `IReadRepository` / `IWriteRepository`
facets. The base unification is a published-package change (publish-first); giving each service's read
repos their own no-tracking read context is service-internal. Projection handlers keep a tracked
context (they fetch-then-mutate), which is why context-wide `NoTracking` on the shared module context
was rejected — the split is read-context vs write-context, not a global toggle.

---

### Environment names are raw strings and test modes leak into production branches

The backend has three overlapping environment vocabularies with no single owner:

- Framework environments use `IsDevelopment()` / `IsProduction()` in some C# paths, while
  `"Development"` is repeated throughout launch configuration.
- The custom `"Testing"` name is repeated across Auth, B2B, Customer, Search, and Payment production
  hosts plus all integration fixtures.
- The custom `"E2E"` name and the `ASPNETCORE_ENVIRONMENT` / `DOTNET_ENVIRONMENT` keys are repeated
  across Auth, `Concertable.ServiceDefaults`, shared E2E composition, and service E2E helpers.

Typos compile, ASP.NET Core and generic-host environment variables can drift apart, and environment
identity has become a hidden capability switch: production entry points know that `Testing` may omit
required configuration and that `E2E` enables test-only behaviour. Environment selection should load
configuration; explicit typed composition/options should select capabilities.

**Resolves when:**

- Establish one testing-owned environment vocabulary for Concertable's custom names and environment
  variable keys, use the framework `Environments` constants/helpers for built-in names in production
  C#, and give test harnesses one API that applies the correct environment consistently to every
  resource.
- Remove every production branch on `Testing` / `E2E`, whether expressed through `IsEnvironment(...)`
  or direct `EnvironmentName` comparison. Integration and E2E hosts supply explicit configuration and
  DI overrides from their own composition roots instead of teaching production code the semantics of
  test environments.
- Eliminate raw custom environment-name literals from C#, move `appsettings.Testing.json` /
  `appsettings.E2E.json` and other test-only configuration out of production project closures, and
  validate the allowed names. Declarative JSON values may remain strings where the format requires
  them, but their values must follow the same vocabulary and be covered by a consistency test.

### `AzureServiceBusOptions` binder defaults are `= ""` instead of `null!`

`Concertable.Messaging.AzureServiceBus/Options/AzureServiceBusOptions.cs` initialises binder-populated `string` properties to `= ""`, where the convention (`agents/CODE_CONVENTIONS.md`) requires `null!` so a missing bind surfaces instead of silently becoming empty (and it uses the banned `""` literal). Deferred, not host-only: `AzureServiceBusOptions` ships in the **published** `Concertable.Messaging` package, so flipping the defaults is a cross-service package change that must ride a Messaging publish + platform-sync, not a bare edit. (The host-side `?? ""` masks that used to sit alongside this — `Auth:Authority` / `ServiceAuth:ClientId` / the ASB `ConnectionString` across the Auth, B2B.Web, B2B.Workers, Customer.Web, Payment.Web, Payment.Workers, Search.Workers, and B2B.Seed.Simulator hosts — now fail fast at startup outside the "Testing" environment, done. `ServiceAuth:ClientSecret` is a genuine optional, now bound **null** when absent — its earlier `string.Empty` was a masking cosmetic swap. The complete fix (`TokenServiceOptions.ClientSecret` → `string?` + the token service omitting the `client_secret` form param when null, correct for a secret-less/public client) is a **published Kernel change** — tracked with the `GetId()` Kernel item above as a cut-over.)

**Resolves when:** the `= ""` defaults become `null!` as part of a `Concertable.Messaging` package publish.

---

### Auth builds against a pinned shared-platform package while the rest of the solution builds from source

`api/Concertable.Auth/Directory.Packages.props` pins the shared platform to `ConcertablePlatformVersion` (currently `0.1.0-alpha.0.526`), so in the full `Concertable.slnx` build Auth compiles against that *published* package while B2B/Customer/Search build the same shared projects from live source. Edit shared source without re-publishing + bumping the pin and Auth silently compiles against stale code; a breaking shared-API change turns only the Auth build red with a confusing "works in source, fails as package" error. Accepted build-separation tradeoff for now (Auth.Contracts has ~0 churn and the shared platform changes infrequently), but the divergence is real the moment shared code moves without a publish.

**Resolves when:** the SERVICE_BUILD_SEPARATION hybrid inner-loop toggle lands (`ProjectReference` for local multi-service dev, `PackageReference` in CI/standalone), or the platform-version pin is automated so it can't lag a shared-source change.

### Orphaned FlatFee accept-checkout holds release only by ~7-day Stripe expiry

When a venue runs FlatFee accept-checkout (a manual-capture PI ring-fencing the venue's own funds) and the application is then withdrawn/rejected/cancelled instead of accepted, nothing cancels the hold: Payment exposes no cancel anywhere (`ManagerPayment` has `FindHeldIntent` but no cancel RPC, and there is no internal hold-cancel — `IStripeHoldClient` has only `FindHeldIntent`/`Capture`), so the funds stay ring-fenced until Stripe auto-expires the intent (~7 days). Money-safe, just slow to release. This was the deliberately-skipped optional Phase 5 of the delivered application-cancel plan — it needs a Payment-first two-PR cycle across the package boundary.

**Resolves when:** `ManagerPayment` gains a `CancelHeldIntent(payer_id, application_id)` RPC (+ `IManagerPaymentClient.CancelHeldIntentAsync` and fake/mock impls, published as `Payment.Client`), and B2B best-effort releases the hold on FlatFee withdraw/reject/cancel.

---

### No local-source swap for cross-service adapter packages during a breaking migration

`Directory.Build.targets`' `UseLocalCore` swaps only the churny *core* (`Kernel`, `Messaging.*`) from package to source; cross-**service** adapter packages (`Payment.Client`/`Contracts`, `*.Tenant.Contracts`, etc.) have no equivalent swap. So mid-way through a *breaking* cross-service contract change, the full `Concertable.slnx` won't build green locally — production consumers bind the old package while the integration-test fixtures `ProjectReference` the new source. You can still build/test per-service (`Payment.slnx` green; red confined to the 4 consumer fixtures + `TicketApiTests`), so it's a comfort gap, not a blocker. Deliberately deferred (was Phase 2 of the now-deleted `plans/PLATFORM_PACKAGE_SYNC.md`): the core friction — hands-off, green pin propagation — is already solved by the `platform-sync` workflow; this only removes local red while iterating, and adds a local-vs-CI divergence (the reason the swap is inner-loop-only, never committed/CI).

**Resolves when:** a real breaking migration makes the local red painful enough to justify extending the `UseLocalCore` swap to cross-service adapter packages (local/inner-loop only — CI + the carve gates always build against packages).

### CI feed restore assumes a same-repo `GITHUB_TOKEN` — fork / Dependabot PRs can't read the org feed

`.github/workflows/test.yml` authenticates the GitHub Packages feed with `secrets.GITHUB_TOKEN` in the `build`, `carve-auth`, and merge-queue E2E jobs. A PR opened from a **fork** (or a Dependabot PR) runs with a read-only token scoped to the fork, which cannot read the `Concertable` org's private packages, so those PRs would 401 at restore regardless of the change. Not a problem for the current same-repo branch + merge-queue workflow (no fork PRs), logged in case the repo is ever opened to external contributors.

**Resolves when:** the org packages are made internal-visible to the org's repos, or fork PRs are given a `read:packages` PAT (or simply aren't accepted).

### `Cors:AllowedOrigins` / `ExternalServices` config reads are magic-string literals with no shared home

Every `Configure<XSettings>(GetSection(...))` binding now goes through a typed `SectionName` const (the
`SpaClientSettings` pattern), but two magic-string reads of a different shape remain — inline
`.Get<>()`/`.GetValue<>()`, not settings-class bindings, so `SectionName` doesn't apply directly and they're
duplicated with no shared owner:

- `GetSection("Cors:AllowedOrigins").Get<string[]>()` — copy-pasted identically across all four host
  `Program.cs` files (B2B, Customer, Search, Payment.Web).
- `GetSection("ExternalServices").GetValue<bool>("UseReal…")` — read in three separate packages
  (`Payment.Infrastructure` `UseRealStripe`, `Shared.Email.Infrastructure` `UseRealEmail`,
  `Shared.Blob.Infrastructure` `UseRealBlob`), each reading only its own sub-key.

A renamed section/key silently stops binding, with no compile error and no single place to change.

**Resolves when:** CORS wiring is extracted to one shared `AddDefaultCors(configuration)` extension over a typed
`CorsSettings.SectionName`, and the `ExternalServices` flags bind through a shared typed options type (home
referenced by all three packages) instead of per-package literals — so neither section name lives as a
duplicated literal.

### Timestamps are `DateTime` (UTC-by-naming-convention), not `DateTimeOffset`

Every timestamp across the backend is stored as `DateTime` with a `…Utc` suffix — sourced from
`TimeProvider.GetUtcNow().UtcDateTime`, mapped to SQL `datetime2` (`ContractEntity.CreatedAtUtc`,
`ConcertEntity.Period`, `InvoiceEntity.TaxPointUtc`/`CreatedAtUtc`, and so on across every module). The
UTC-ness is a *naming* convention, not carried by the type: nothing stops a caller assigning a `Kind=Local`
or `Kind=Unspecified` value, and the offset the instant was recorded at is lost. `DateTimeOffset` (SQL
`datetimeoffset`) would make "this is an absolute instant" type-enforced rather than suffix-promised. New
entities (e.g. the Phase-2 invoice) match the existing `DateTime` convention deliberately — switching one
entity in isolation just makes it the odd column type.

**Resolves when:** a repo-wide sweep moves entity/DTO timestamps to `DateTimeOffset` in one consistency
pass (entities, EF configs → `datetimeoffset`, DTOs, and the `TimeProvider.GetUtcNow()` call sites that
currently `.UtcDateTime` them away). One coordinated migration-touching change, not piecemeal — a lone
`DateTimeOffset` next to `DateTime` neighbours is worse than uniform.

### `Service` is used as a catch-all suffix, hiding which collaborators are orchestrators

Most `IXService` types are genuine services — they orchestrate domain logic over a repository
(`IVenueService`, `IConcertService`, `IInvitationService`, and `ITicketPdfService`, which does inject
`ITicketRepository`). But the suffix is also worn by two shared types that own no persistence and are
really byte/blob gateways, which flattens a distinction worth seeing at the injection site:

- **`IBlobStorageService`** (`Shared.Blob`) — wraps `BlobServiceClient` + options; a gateway/store.
- **`IImageService`** (`Shared.Imaging`) — `Upload`/`Download`/`Replace`/`Delete`, sitting directly on
  `IBlobStorageService`. Bytes in and out of a backing store, no domain logic; a store over a store.

The module-internal half of this is **done**: the B2B Concert `IContractPdfService` / `IInvoicePdfService`
— pure `IPdfBlobCache`-backed document renderers with no repository — are renamed to
`IContractPdfRenderer` / `IInvoicePdfRenderer`, alongside the existing `IPdfRenderer`. Only the two
shared store types remain, and they're boundary-blocked (published packages).

Why it matters beyond taste: "a service calling another service" is a smell worth spotting by name, and
it only reads as a smell when *service* means orchestrator. When a pure value-producer is also called
`Service`, every such call looks equally suspicious and the signal is lost. `CODE_PATTERNS.md` already
states the rule this would follow — name the type as the agent-noun of its one method
(`Renderer.Render`, `Resolver.Resolve`, `Calculator.Calculate`).

Note the distinction is *shape*, not *staticness*: these are injected, config-bound collaborators, so
`Helper`/`Utility` (which in sibling codebases denotes a `static` class of pure functions) would be the
wrong correction — the honest name here is `Store`.

**Resolves when:** the two shared byte/blob gateways are renamed to their agent-noun as a publish-first
package cut-over — `IBlobStorageService` → `IBlobStore` (`Shared.Blob`), `IImageService` → `IImageStore`
(`Shared.Imaging`) — reserving `Service` for repository-backed orchestrators. Both ship in published
packages consumed cross-service (Auth/B2B/Customer call `AddSharedBlob` / imaging), so a rename reds
`platform-sync` and can't be atomic: rename in the package, publish, migrate consumers in the sync PR.
Do the pair in one sweep so the store vocabulary doesn't land half-applied.

