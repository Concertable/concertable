# Concertable — backend cross-cutting technical debt

Debt spanning multiple services or host `Program.cs` files. Debt inside the shared platform tree (`Concertable.Kernel`, `Concertable.Shared.*`, the shared test libs) belongs in [`Concertable.Shared/TECH_DEBT.md`](./Concertable.Shared/TECH_DEBT.md); service-specific debt belongs in that service's own `TECH_DEBT.md`; debt spanning `api/` and `app/`, or in root-level `.github/workflows/**`/config, belongs in the root [`TECH_DEBT.md`](../TECH_DEBT.md).

---

## MED

### Redundant `this.` qualification survives outside the PR #633 file set

`STYLE.md` now states that `this.` exists only to disambiguate a member a parameter or local shadows,
and PR #633 stripped it from the 602 `.cs` files that PR touches. The rest of `api/` still carries
**977 redundant `this.` qualifications across 85 files** — concentrated in Customer module services,
`Concertable.Shared` test libraries, Payment infrastructure, and the B2B files this PR does not open.
The rule is not expressible in `.editorconfig`: `dotnet_style_qualification_for_field` is
all-or-nothing and its `true` setting is the opposite of the convention.

The sweep was scoped deliberately rather than run repo-wide: four other worktrees are live on shared
files, and the mechanical pass needs member-scope shadow analysis (a naive strip produces
`competingChange = competingChange;` wherever a non-constructor setter takes a same-named parameter).

**Resolves when:** the remaining 977 sites are stripped with shadowed members left qualified, the
solution builds, and no `X = X;` self-assignment exists anywhere in `api/`.

---

### Injected collaborator variables drop their shape noun across the backend

`NAMING.md` requires an injected parameter and its field to keep the collaborator's shape noun
(`ISettlementService settlementService`), dropping only the domain prefix the containing type already
supplies (`repository` inside `SettlementService`). PR #633 corrected the ~50 sites its own refactor
introduced. **562 sites across `api/` still deviate**, two conventions dominating:

| Pattern | Sites | Should read |
|---|---|---|
| `XDbContext context` | 132 | `xDbContext`, or `dbContext` where the owner supplies `X` |
| `XApiFixture fixture` | 98 | `xApiFixture`, or `apiFixture` inside `XApiTests` |

The remaining ~330 are one-offs — pluralised domain nouns for a service (`IBookingService bookings`),
dropped qualifiers (`IArtistReadModelRepository artistRepository`), and abbreviations
(`IUnitOfWorkBehavior uowBehavior`). Both dominant patterns are repo-wide conventions that predate the
module carve, so correcting only a subset fragments them.

**Resolves when:** every injected field and constructor parameter in `api/` names its collaborator type
in lower camel case with the shape noun intact, and the two dominant patterns are converted in one
sweep each rather than per-PR.

---
### Production assemblies own dev/test seeding across the backend

Dev/test seeder implementations and seed-only helpers currently live in production assemblies across
the backend:

- B2B has sixteen `IDevSeeder` / `ITestSeeder` implementations in module `*.Infrastructure` projects;
  Concert Infrastructure also owns `SeededApplicationSigner`, `SeededContractFactory`, and
  `SeededSelfBillingAgreementGranter`. The module Infrastructure projects depend on
  `Concertable.B2B.Seed.Infrastructure`.
- Customer has nine dev/test seeder implementations in module `*.Infrastructure` projects, whose
  production project graph depends on `Concertable.Customer.Seed.Infrastructure`.
- Payment and Search each keep a test seeder in their production Infrastructure assembly:
  `PaymentTestSeeder` and `SearchProjectionTestSeeder`. Search Infrastructure also depends on
  `Concertable.Search.Seed.Infrastructure`.
- Auth keeps `AuthDevSeeder` in the production Auth assembly, and the published Shared Blob
  Infrastructure package keeps `BlobDevSeeder` beside its production implementation.

Moving an individual helper only hides one symptom while its caller, registration, and seed-project
dependency remain in the production closure. The correction is a backend-wide composition change that
keeps production write capabilities in their owning modules while moving seed orchestration,
implementations, helpers, and registration into seed/test-owned assemblies.

**Resolves when:** production assemblies contain no `IDevSeeder` / `ITestSeeder` implementations,
seed-only helpers, or seeder registration methods; production projects do not reference service seed
projects or `Concertable.Seed.*`; and AppHost, development, integration, and E2E composition roots add
the appropriate seed-owned assemblies without changing the production write paths each seeder exercises.

---

### Controller route-token casing is implemented only in the B2B host

`Concertable.B2B.Web` owns `KebabCaseRouteTransformer` and registers
`RouteTokenTransformerConvention` directly in `Program.cs`. Controller-token casing is an HTTP-host
convention rather than B2B domain behaviour, so leaving it local lets other backend hosts implement a
different route format or copy the same plumbing.

**Resolves when:** `Concertable.Shared.Api` exposes the transformer through one shared MVC registration
extension, the package is published, every MVC host installs that extension, and the B2B-local
transformer and inline registration are removed.

### Async application and persistence APIs do not consistently propagate cancellation

Many application-service, repository, module-facade, and infrastructure methods perform EF Core,
HTTP, blob, payment, or other asynchronous I/O without accepting a `CancellationToken`, while some
neighbouring paths already propagate one. Request cancellation therefore stops at inconsistent
boundaries, and callers cannot reliably cancel work after a client disconnect or host shutdown.

**Resolves when:** inventory every async backend interface and implementation, add
`CancellationToken ct = default` to methods that can reach I/O, thread it through all supporting
dependency and framework calls, and add architecture coverage that rejects new cancellable I/O paths
without a token. Preserve the Result convention: cancellation propagates as cancellation and is never
mapped to an expected-outcome error.

### Repository query outputs blur entities, read models, projections, and DTO contracts

Repository contracts across B2B, Customer, Search, and Payment do not follow one ownership or naming
model for query results. Depending on the module, repositories return persistence entities,
event-maintained `*ReadModel` entities, application `*Details`/`*Dto` shapes, paged DTOs, tuples, or a
public `*.Contracts` DTO. For example, Customer Concert's `IConcertReadRepository.GetDtoAsync` returns
the module contract `ConcertDto`, so its persistence adapter materializes a cross-module contract
directly, while neighbouring repositories return `ConcertDetails`, entities, or persisted read models.

The repository-output and DTO rules in the `csharp-naming` skill now define the intended naming,
ownership, and mapping boundary. Existing repositories predate that standard, however, so `Dto`,
`Details`, `Projection`, `ReadModel`, and `Entity` still communicate different things in different areas,
and dependency direction, tracking expectations, and public-contract coupling remain inconsistent.

**Resolves when:** inventory and migrate every violation in coherent service/package cut-overs, validate
the standard against representative read, write, paginated, cross-module, and performance-sensitive
queries, and add practical architecture tests or mechanical guards for the parts that can be enforced
automatically.

### Repository bases repeat CRUD, and read no-tracking is a bypassable `Query` convention

The shared `Concertable.DataAccess.Infrastructure` repository bases duplicate `GetByIdAsync` /
`GetAllAsync` / `Exists` across `ReadRepository<>` and `Repository<>` (plus concrete overrides). The
*only* real difference is tracking: read reads go through the no-tracking `Query` root, write reads
through the tracked `context.Set<T>()`. And `Query` enforces nothing — a read repo can still call
`context.Foo` directly and get a **tracked** query; nothing stops it. So it's a convention, not a
guarantee, and the duplication only exists because tracking lives on the query.

**Resolves when:** no-tracking becomes a property of the **context**, not the query. Read repositories
sit on a read-only, no-tracking context (the shared `ReadDbContext` shape — `SaveChanges` throws — already
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
  resource. (Owner + cut-over DONE: `"Testing"` → `"Integration"` rename; `Concertable.Kernel` owns C# 14
  extension members — `Environments.Integration`/`.E2E` and `env.IsIntegration()`/`.IsE2E()` (mirroring
  `IsDevelopment()`); post-publish, the 24 production `Integration` checks + Auth's 2 `E2E` checks now call the
  helpers, the fixtures resolve `Environments.Integration` from Kernel, and the transitional
  `Concertable.Testing.Integration` copy is deleted. **One literal remains:** `Concertable.ServiceDefaults` sits
  *below* Kernel and can't reference its vocabulary without a layering inversion, so its single
  `IsEnvironment("E2E")` stays a string — closing it needs the vocabulary to live in the lowest shared project,
  a separate design call.)
- Remove every production branch on `Testing` / `E2E`, whether expressed through `IsEnvironment(...)`
  or direct `EnvironmentName` comparison. Integration and E2E hosts supply explicit configuration and
  DI overrides from their own composition roots instead of teaching production code the semantics of
  test environments.
- Eliminate raw custom environment-name literals from C#, move `appsettings.Testing.json` /
  `appsettings.E2E.json` and other test-only configuration out of production project closures, and
  validate the allowed names. Declarative JSON values may remain strings where the format requires
  them, but their values must follow the same vocabulary and be covered by a consistency test.

### Existing extension containers still use legacy `this` parameters

Many existing extension containers still use the pre-C# 14 form — `public static T M(this X x, …)` in `XExtensions`
static classes. C# 14 (net10) added `extension()` blocks: the unified "extension members" form that also expresses
extension properties, indexers, and static members, and groups members by receiver. Both compile to identical IL,
so this is modernization/consistency debt, not a behavioural gap. The env-vocabulary work set the example —
`Concertable.Kernel.EnvironmentsExtensions` / `HostEnvironmentExtensions` use `extension(Environments)` /
`extension(IHostEnvironment env)` blocks (giving `Environments.Integration` + `env.IsIntegration()`).

**Resolves when:** ordinary `this`-parameter extension methods migrate to `extension()` blocks, with
receiver-owned members grouped in `XExtensions` and related mapping receivers grouped in `XMappers`.
Every touched container migrates completely; new extension members use `extension()` from the start
(see the `csharp-style` skill). Signature-bound generator/framework declarations are excluded.

### Extension-container names do not consistently identify their receiver

Backend extension containers use mixed naming: receiver-aligned names such as
`DistributedApplicationBuilderExtensions` and `ServiceCollectionExtensions` coexist with concern-aligned
names such as `AppHostExtensions`, `HostExtensions`, and `E2EAdminExtensions`, even when those types extend
the same framework builders or service collection. A reader therefore cannot reliably infer the extended
type from the container or filename, and equivalent extensions are harder to discover together.

**Resolves when:** inventory every backend extension container, rename receiver-owned containers and files
to `<Receiver>Extensions` (using the shortest unambiguous receiver name), keep mapping families in
`<Target>Mappers`, and add a practical architecture or source check for new public/internal extension
containers whose name does not match their receiver. Concern names remain on the methods that describe the
operation being added; declaration-contract exceptions remain excluded.

### `AzureServiceBusOptions` binder defaults are `= ""` instead of `null!`

`Concertable.Messaging.AzureServiceBus/Options/AzureServiceBusOptions.cs` initialises binder-populated `string` properties to `= ""`, where the convention (`csharp-style` skill) requires `null!` so a missing bind surfaces instead of silently becoming empty (and it uses the banned `""` literal). Deferred, not host-only: `AzureServiceBusOptions` ships in the **published** `Concertable.Messaging` package, so flipping the defaults is a cross-service package change that must ride a Messaging publish + platform-sync, not a bare edit. (The host-side `?? ""` masks that used to sit alongside this — `Auth:Authority` / `ServiceAuth:ClientId` / the ASB `ConnectionString` across the Auth, B2B.Web, B2B.Workers, Customer.Web, Payment.Web, Payment.Workers, Search.Workers, and B2B.Seed.Simulator hosts — now fail fast at startup outside the "Testing" environment, done. `ServiceAuth:ClientSecret` is a genuine optional, now bound **null** when absent — its earlier `string.Empty` was a masking cosmetic swap. The complete fix (`TokenServiceOptions.ClientSecret` → `string?` + the token service omitting the `client_secret` form param when null, correct for a secret-less/public client) is a **published Kernel change** — tracked with the `GetId()` Kernel item above as a cut-over.)

**Resolves when:** the `= ""` defaults become `null!` as part of a `Concertable.Messaging` package publish.

**Done (PR1, `Chore/TechDebt`) — pending publish:** both `ConnectionString` and `ServiceName` defaults are now `null!` — required, assigned-before-use. The connection string is no longer eagerly bound-and-thrown at registration; it is validated on resolution of the Service Bus client (see the `AddAzureServiceBusTransport` eager-probe item in [`Concertable.Messaging/TECH_DEBT.md`](./Concertable.Messaging/TECH_DEBT.md), the same package change). Delete this entry once that change publishes.

---

### Auth builds against a pinned shared-platform package while the rest of the solution builds from source

`api/Concertable.Auth/Directory.Packages.props` pins the shared platform to `ConcertablePlatformVersion` (currently `0.1.0-alpha.0.526`), so in the full `Concertable.slnx` build Auth compiles against that *published* package while B2B/Customer/Search build the same shared projects from live source. Edit shared source without re-publishing + bumping the pin and Auth silently compiles against stale code; a breaking shared-API change turns only the Auth build red with a confusing "works in source, fails as package" error. Accepted build-separation tradeoff for now (Auth.Contracts has ~0 churn and the shared platform changes infrequently), but the divergence is real the moment shared code moves without a publish.

**Resolves when:** the SERVICE_BUILD_SEPARATION hybrid inner-loop toggle lands (`ProjectReference` for local multi-service dev, `PackageReference` in CI/standalone), or the platform-version pin is automated so it can't lag a shared-source change.

### Per-project `obj`/`bin` output risks Windows `MAX_PATH` as module nesting deepens

A project's `obj`/`bin` folders sit inside its own source directory and repeat the full project name a second time beneath it, so a nested module's build output can exceed Windows' 260-character path limit — e.g. `Concertable.B2B.Dashboard.Opportunity.Application/obj/Debug/net10.0/Concertable.B2B.Dashboard.Opportunity.Application.dll` is 272 characters. On a Windows machine without NTFS long-path support enabled, this intermittently fails MSBuild's `Copy` task with `MSB3030: could not copy ... because it was not found` even though the file compiled and exists — the referencing project simply can't see it. Enabling `LongPathsEnabled` in the registry is an immediate per-machine mitigation, but it is not enforced anywhere, so a fresh clone or a locked-down machine hits this again. First surfaced building `Concertable.B2B.Dashboard.Opportunity.Api` on `Refactor/launch_deal-lifecycle-modules-phase2`.

**Resolves when:** each service adopts the .NET SDK's `UseArtifactsOutput`, centralizing `obj`/`bin` to one short `artifacts/` tree at the service root instead of inside every project folder — landed per-service at the point that service is extracted into its own repo during the repo-split migration, rather than as a big-bang change across the still-shared monorepo.

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


---

### `ActionLink` is declared once per Api module instead of once in `Concertable.Shared.Api`

`internal sealed record ActionLink(string Href, string Method)` now exists twice, byte-identical:
`Concertable.B2B.Concert.Api/Responses/ActionLink.cs` and
`Concertable.B2B.Conversations.Api/Responses/ActionLink.cs`. It is a generic HATEOAS wire primitive —
not a module concept — and every Api module that grows an action link will copy it a third time.

The OSA report-content plan justified the second copy on the grounds that hoisting it would create the
cross-module coupling the `module-structure` skill forbids. **That reasoning was wrong:** those rules
forbid one module reaching into another module's types, and explicitly cover shared libraries as a
legitimate home for cross-cutting layer concerns. `Concertable.Shared.Api` is exactly that home — the
Api-layer shared library both modules already consume — and the frontend has had a single shared
`ActionLink` in `app/shared/src/types/common.ts` all along, so the backend duplication is also
asymmetric with the wire contract it mirrors.

It could not be fixed in the PR that introduced the second copy, because `Concertable.Shared.Api` is
consumed as a **published package pinned to `ConcertablePlatformVersion`** — a type added to its source
is invisible to consumers until it is published and `platform-sync` bumps the pin. So it is a
publish-first cut-over, not an edit.

**Resolves when:** `public sealed record ActionLink(string Href, string Method)` lives in
`Concertable.Shared.Api`, is published, and both module-local copies are deleted in the follow-up PR
once the pin carries it. Any new Api module uses the shared one rather than minting a third.

### Rate limiting is in-process only — no distributed store for horizontally-scaled correctness

`AddDefaultRateLimiting` (`Concertable.ServiceDefaults`) registers the built-in `AddRateLimiter`, whose
partitioned limiters live in each process's memory. Under horizontal scale every replica counts
independently, so a policy nominally set to N/min actually permits up to N×(replica count)/min — the
per-user/per-IP ceiling loosens in proportion to the fleet. This is acceptable at launch (single-instance
per service) and is a deliberate launch scope cut: an in-process limiter delivers the abuse floor now
without standing up shared infrastructure.

**Resolves when:** the limiter is backed by a shared store (e.g. Redis) so counts are fleet-global, or a
gateway/edge layer enforces the coarse per-IP ceiling ahead of the app while the app keeps the
identity-aware policies. Revisit before any service runs more than one replica with rate limiting as a
relied-upon control.

### Many read endpoints are anonymous-by-omission across B2B and Payment

Auditing the three endpoints named by the rate-limiting sweep (now fixed — Payment `GET /api/Transaction`
carries `[Authorize]`; the B2B blob upload/delete endpoints were dead code and were removed; `GET
api/blob/download` is deliberately `[AllowAnonymous]` because it serves the public marketplace images every
surface renders) surfaced that the problem is far wider. Roughly thirty controller actions in B2B and Payment
carry neither a class- nor method-level `AuthorizeAttribute` (`[Authorize]`, `[HasPermission]`, `[Admin]`) nor
an explicit `[AllowAnonymous]`, so they are reachable unauthenticated. Some are legitimately public
(artist/venue/concert details, reviews), but several expose private business or financial data and almost
certainly should not be — e.g. `GET api/application/{id}/contract(/pdf)`, `GET api/concert/{id}/invoice(/pdf)`,
`GET api/application/{id}/financial-operation`, the `.../ownership` checks, and `GET api/deal/{id}`.
Public-vs-private is a per-endpoint call.

The mutating side is now guarded: `ControllerBoundaryTests.Mutating_endpoints_declare_authorization_explicitly`
fails the build if any POST/PUT/PATCH/DELETE action in B2B is neither authorized nor explicitly
`[AllowAnonymous]`. That guard is B2B-only — it scans `Concertable.B2B.*` assemblies — so Payment and the
other services have no equivalent, and no read-side guard exists anywhere (a read guard needs each public
read tagged `[AllowAnonymous]` first).

**Resolves when:** every anonymous-by-omission read is classified — private reads gain the correct
`[Authorize]`/`[HasPermission]` (scoped to the caller's own tenant/resource), genuinely public reads gain an
explicit `[AllowAnonymous]` — with tests proving an anonymous request is rejected on the private ones; then a
read-side guard *and* the mutating guard both cover every service (via one shared reflection helper in
`Concertable.Testing.Architecture`, mirroring the consolidated assembly-reference guard, rather than a
per-service copy), so no endpoint in any service is reachable anonymously by omission again.

### Public images and private PDFs share one blob container behind an anonymous read endpoint

`BlobStorageService` uses a single container (`BlobStorage:ContainerName`, `"images"`) with no per-type
separation, and B2B's `GET api/blob/download` reads from it `[AllowAnonymous]` to serve the public marketplace
images. But `PdfBlobCache` writes private contract/invoice/self-billing PDFs (`contracts/…`, `invoices/…`) into
that same container. This is not exploitable today — PDF blob names embed a 122-bit `Guid`, the `download/{blobName}`
route is a single non-catch-all segment and ASP.NET Core rejects encoded `/`, so a namespaced private blob
cannot be addressed, and `Download` now also rejects any `blobName` containing a path separator. But the
separation rests on name-secrecy plus routing shape, not on isolation: a route change to catch-all, an
encoded-slash config change, or a leaked PDF name (they are persisted and served by the authenticated PDF
endpoints) would each re-open it.

**Resolves when:** public images and private documents live in separate containers (or non-overlapping,
access-differentiated prefixes), so an anonymous read endpoint is scoped to the public store by construction and
can never resolve to a private document regardless of route shape or name exposure.

