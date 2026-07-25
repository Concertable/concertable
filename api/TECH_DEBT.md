# Concertable — cross-cutting technical debt

Debt spanning multiple services, host `Program.cs` files, or repo-wide build/CI config. Debt inside the shared platform tree (`Concertable.Kernel`, `Concertable.Shared.*`, the shared test libs) belongs in [`Concertable.Shared/TECH_DEBT.md`](./Concertable.Shared/TECH_DEBT.md); service-specific debt belongs in that service's own `TECH_DEBT.md`. When an item is fixed, update both this file and [`ARCHITECTURE.md`](./ARCHITECTURE.md).

---

## MED

### `AzureServiceBusOptions` binder defaults are `= ""` instead of `null!`

`Concertable.Messaging.AzureServiceBus/Options/AzureServiceBusOptions.cs` initialises binder-populated `string` properties to `= ""`, where the convention (`docs/CODE_CONVENTIONS.md`) requires `null!` so a missing bind surfaces instead of silently becoming empty (and it uses the banned `""` literal). Deferred, not host-only: `AzureServiceBusOptions` ships in the **published** `Concertable.Messaging` package, so flipping the defaults is a cross-service package change that must ride a Messaging publish + platform-sync, not a bare edit. (The host-side `?? ""` masks that used to sit alongside this — `Auth:Authority` / `ServiceAuth:ClientId` / the ASB `ConnectionString` across the Auth, B2B.Web, B2B.Workers, Customer.Web, Payment.Web, Payment.Workers, Search.Workers, and B2B.Seed.Simulator hosts — now fail fast at startup outside the "Testing" environment, done. `ServiceAuth:ClientSecret` is a genuine optional, now bound **null** when absent — its earlier `string.Empty` was a masking cosmetic swap. The complete fix (`TokenServiceOptions.ClientSecret` → `string?` + the token service omitting the `client_secret` form param when null, correct for a secret-less/public client) is a **published Kernel change** — tracked with the `GetId()` Kernel item above as a cut-over.)

**Resolves when:** the `= ""` defaults become `null!` as part of a `Concertable.Messaging` package publish.

---

### Auth builds against a pinned shared-platform package while the rest of the solution builds from source

`api/Concertable.Auth/Directory.Packages.props` pins the shared platform to `ConcertablePlatformVersion` (currently `0.1.0-alpha.0.526`), so in the full `Concertable.slnx` build Auth compiles against that *published* package while B2B/Customer/Search build the same shared projects from live source. Edit shared source without re-publishing + bumping the pin and Auth silently compiles against stale code; a breaking shared-API change turns only the Auth build red with a confusing "works in source, fails as package" error. Accepted build-separation tradeoff for now (Auth.Contracts has ~0 churn and the shared platform changes infrequently), but the divergence is real the moment shared code moves without a publish.

**Resolves when:** the SERVICE_BUILD_SEPARATION hybrid inner-loop toggle lands (`ProjectReference` for local multi-service dev, `PackageReference` in CI/standalone), or the platform-version pin is automated so it can't lag a shared-source change.

### Orphaned FlatFee accept-checkout holds release only by ~7-day Stripe expiry

When a venue runs FlatFee accept-checkout (a manual-capture PI ring-fencing the venue's own funds) and the application is then withdrawn/rejected/cancelled instead of accepted, nothing cancels the hold: Payment exposes no cancel RPC (`ManagerPayment` has `FindHeldIntent` but no cancel; `IStripeHoldClient.CancelAsync` is Payment-internal), so the funds stay ring-fenced until Stripe auto-expires the intent (~7 days). Money-safe, just slow to release. This was the deliberately-skipped optional Phase 5 of the delivered application-cancel plan — it needs a Payment-first two-PR cycle across the package boundary.

**Resolves when:** `ManagerPayment` gains a `CancelHeldIntent(payer_id, application_id)` RPC (+ `IManagerPaymentClient.CancelHeldIntentAsync` and fake/mock impls, published as `Payment.Client`), and B2B best-effort releases the hold on FlatFee withdraw/reject/cancel.

---

### No local-source swap for cross-service adapter packages during a breaking migration

`Directory.Build.targets`' `UseLocalCore` swaps only the churny *core* (`Kernel`, `Messaging.*`) from package to source; cross-**service** adapter packages (`Payment.Client`/`Contracts`, `*.Tenant.Contracts`, etc.) have no equivalent swap. So mid-way through a *breaking* cross-service contract change, the full `Concertable.slnx` won't build green locally — production consumers bind the old package while the integration-test fixtures `ProjectReference` the new source. You can still build/test per-service (`Payment.slnx` green; red confined to the 4 consumer fixtures + `TicketApiTests`), so it's a comfort gap, not a blocker. Deliberately deferred (was Phase 2 of the now-deleted `plans/PLATFORM_PACKAGE_SYNC.md`): the core friction — hands-off, green pin propagation — is already solved by the `platform-sync` workflow; this only removes local red while iterating, and adds a local-vs-CI divergence (the reason the swap is inner-loop-only, never committed/CI).

**Resolves when:** a real breaking migration makes the local red painful enough to justify extending the `UseLocalCore` swap to cross-service adapter packages (local/inner-loop only — CI + the carve gates always build against packages).

### CI feed restore assumes a same-repo `GITHUB_TOKEN` — fork / Dependabot PRs can't read the org feed

`.github/workflows/test.yml` authenticates the GitHub Packages feed with `secrets.GITHUB_TOKEN` in the `build`, `carve-auth`, and merge-queue E2E jobs. A PR opened from a **fork** (or a Dependabot PR) runs with a read-only token scoped to the fork, which cannot read the `Concertable` org's private packages, so those PRs would 401 at restore regardless of the change. Not a problem for the current same-repo branch + merge-queue workflow (no fork PRs), logged in case the repo is ever opened to external contributors.

**Resolves when:** the org packages are made internal-visible to the org's repos, or fork PRs are given a `read:packages` PAT (or simply aren't accepted).

### Config section names are magic-string literals, not typed constants (one lone outlier)

Every `Configure<XSettings>(configuration.GetSection("..."))` across the backend passes the section name as a
bare string literal — `"Stripe"` (`Payment.Infrastructure`), `"Legal"` (`B2B.Concert`), `"Urls"` (`Kernel`),
`"BlobStorage"` (`Shared.Blob`), `"TaxCompliance"` (`B2B.Tenant`), plus the `"Cors:AllowedOrigins"` /
`"ExternalServices"` reads in the host `Program.cs` files. The sole exception is `Concertable.Auth`'s
`SpaClientSettings.SectionName = "Auth:SpaClients"`, bound via `GetSection(SpaClientSettings.SectionName)` —
the pattern the rest should follow. A renamed section silently stops binding: the literal and the appsettings
key drift independently with no compile error.

**Resolves when:** a repo-wide sweep gives each settings class a `public const string SectionName` and every
`Configure<T>(GetSection(...))` binds through it (adopting the `SpaClientSettings` pattern). Done as one
consistency pass, not piecemeal — a lone typed section next to magic-string neighbours is worse than uniform.

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
`ITicketRepository`). But the suffix is also worn by types that own no persistence and are really
value-producers or gateways, which flattens a distinction worth seeing at the injection site:

- **`IContractPdfService` / `IInvoicePdfService`** (B2B Concert) — inject only `IPdfBlobCache`, no
  repository; they render a document from data. The codebase already has `IPdfRenderer`, and
  `CODE_PATTERNS.md` already blesses `Renderer.Render` — so these two are inconsistent with vocabulary
  that exists here today.
- **`IBlobStorageService`** (`Shared.Blob`) — wraps `BlobServiceClient` + options; a gateway/store.
- **`IImageService`** (`Shared.Imaging`) — `Upload`/`Download`/`Replace`/`Delete`, sitting directly on
  `IBlobStorageService`. Bytes in and out of a backing store, no domain logic; a store over a store.

Why it matters beyond taste: "a service calling another service" is a smell worth spotting by name, and
it only reads as a smell when *service* means orchestrator. When a pure value-producer is also called
`Service`, every such call looks equally suspicious and the signal is lost. `CODE_PATTERNS.md` already
states the rule this would follow — name the type as the agent-noun of its one method
(`Renderer.Render`, `Resolver.Resolve`, `Calculator.Calculate`).

Note the distinction is *shape*, not *staticness*: these are injected, config-bound collaborators, so
`Helper`/`Utility` (which in sibling codebases denotes a `static` class of pure functions) would be the
wrong correction — the honest names are `Factory` / `Renderer` / `Store`.

**Resolves when:** a naming pass renames the non-orchestrator `*Service` types to their agent-noun,
settling on one vocabulary — `Factory` creates values, `Renderer` produces a document, `Store` fronts a
byte/blob backing store, and `Service` is reserved for repository-backed orchestrators:
the two PDF ones → `*PdfRenderer` (alongside the existing `IPdfRenderer`);
`IBlobStorageService` → `IBlobStore`; `IImageService` → `IImageStore`. Best done as one sweep — renaming
the `Kernel` and `Shared.*` types republishes those packages and triggers a platform-sync, so batch them
rather than paying that cost once per rename.
