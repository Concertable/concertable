# Production config, secrets & deployment — plan (WORKING)

**Status:** Phase 0 (deployment research) DONE 2026-07-17 — target resolved (ACA via `azd`; IaC =
**Terraform**). Phase 1a (factory consolidation + B2B CORS fix) **and Phase 1b-now (ServiceDefaults
shared-defaults seam + B2B BlobStorage intra-service collapse)** done + build-green on
`Feature/ConfigConsolidation` (uncommitted). Phase 1b-after-sync (publish-gated cross-service dedup +
`AddDefaultCors`) + Phases 2-4 outstanding. The end-to-end config + connection-string **architecture**
(runtime / design-time / secrets / prod source) is now designed — see **"Recommended architecture …
(Prompt 3 outcome)"** below; the phases implement it. Triggered by an investigation that found the app has **no production existence** — no
deployment path, no config/secrets store, secrets committed to source. This is the **app-wide**
config/secrets/deployment workstream; the *region*-scoped config seam lives separately in
[`CONFIG_STRATEGY.md`](CONFIG_STRATEGY.md). Branch when Phase 1 code starts (doc-only until then).

## Why — investigation findings (2026-07-17)
Three parallel investigations mapped the current state. Headlines (all confirmed with file paths):

### Deployment — none
- No IaC (Bicep/Terraform/ARM/azd), no Dockerfiles, no CD. CI builds/tests/publishes NuGet + mirrors
  service folders, but **deploys nothing** (no `azure/login`, no Azure credentials anywhere).
- App runs only locally: **6 Aspire AppHosts against emulators** (SQL container, ASB emulator, Azurite).
  No run-vs-publish branching to swap emulators for managed Azure.
- Independently confirmed by a `deep-research` pass (see Phase 0 outcome below).

### Config / secrets store — none
- **Zero** Azure App Configuration, **zero** Key Vault. Config = layered `appsettings.*.json` +
  user-secrets (dev) + env vars Aspire injects at runtime.
- Dev config **duplicated across ~40 files**: same Stripe keys, Google Maps key, service-auth secrets,
  CORS lists, OIDC `SpaClients` blocks, endpoint/port maps; the localhost SQL string is re-hardcoded in
  **~20 DbContext design-time factories**.
- Prod config **effectively nonexistent**: one orphaned `B2B.Web/appsettings.Production.json`; no
  staging anywhere. *(SPA `.env.production` was an empty stub — ✅ now filled with the prod DNS-scheme hosts;
  publishable keys stay CI-injected. See `DEPLOYMENT.md` "SPAs → Azure Static Web Apps".)*
- The **one** thing already centralized: infra connection strings (SQL/ASB/Blob) — but in Aspire code
  (`AppHost.Shared/DistributedApplicationBuilderExtensions.cs`), not config files.

### Committed secrets — security, fix regardless of the big plan
- Stripe **test** keys (`sk_test`/`pk_test`/`whsec`) duplicated in B2B.Web + Customer.Web
  `appsettings.Development.json`.
- Google Maps API key committed in 3 files.
- **`B2B.Web/appsettings.Production.json` commits a plaintext Azure SQL admin password (`Password11!`).**
  It's in git history → rotate + purge (or rotate + accept). The file is orphaned (nothing provisions it).

## Decision (2026-07-17): build `config`
A native, simple config-as-IaC repo — **NOT** CRIS's bespoke ~700-line pipeline. Shape:
- **Config-as-code** — appsettings-shape JSON, partitioned by **environment** (dev/staging/prod) now;
  **region** as a future top partition (dormant while UK-only). Single-sources the duplicated
  non-secret config. Same shape doubles as the local/dev fallback (one format, two providers).
- **Secrets by reference, never by value** — the repo declares secret *keys* + their **Key Vault
  references**; actual values live in Key Vault, set out-of-band. This is what gets secrets out of git.
- **Native IaC** — **Terraform** (the org standard — NOT Bicep) for the App Config store + Key Vault +
  key-values/references (`azurerm_app_configuration`, `azurerm_key_vault`, `azurerm_app_configuration_key`),
  applied in one GitHub Action.
- **Flow** — repo → CI → App Config + Key Vault → services via `IConfiguration`. In code it's a
  **provider swap at the composition root** (`AddAzureAppConfiguration` + KV refs); business code
  untouched (the typed-options seam already exists — confirmed by the runtime-wiring investigation).

## Key decision — deployment target (RESOLVED in Phase 0, 2026-07-17)
**Resolved: Azure Container Apps via `azd`** — the Aspire-native path, confirmed lowest-effort: `azd init`
auto-detects the AppHost (no `azure.yaml`, no Dockerfiles), `azd up` provisions + deploys in one step, and
the emulator→managed swap is a publish-time no-op in our code. Full grounding, gotchas, and the four gaps
it left open are in the **Phase 0 outcome** section below. Decides the delivery leg (Phase 4) and the
secrets home (Key Vault + ACA secrets; the exact azd↔Key Vault wiring is still an open gap). Did NOT block
the config/secrets consolidation work (Phases 1-3).

> ⚠️ **IaC = Terraform, not Bicep (org standard) — reconcile with azd in Phase 4.** The Phase 0 research
> below describes azd's *Aspire auto-detection*, which generates **Bicep** — precisely the convenience the
> Terraform standard forgoes. Phase 4 must choose: hand-author Terraform (`azurerm_container_app*`) for the
> ACA infra and use azd/the .NET SDK only for image build+push, OR run azd with its Terraform provider
> (which does **not** auto-generate from the Aspire manifest the way the Bicep path does). Every "generated
> Bicep" mention in the Phase 0 outcome is azd's mechanism, superseded by this decision for provisioning.

## Phase 0 outcome — deployment research (ran 2026-07-17)
`deep-research` on the deployment-target question: 28 sources → 126 claims → **24 confirmed by 3-0
adversarial verification** against primary sources (Microsoft Learn, aspire.dev, .NET DevBlogs,
dotnet/aspire + dotnet/efcore issues), 1 refuted. All findings high-confidence.

**Deployment target — ACA via `azd`.** `azd init` auto-detects the Aspire AppHost (no hand-written
`azure.yaml`, no Dockerfiles) and lets you pick which of the ~7 apps get public HTTP ingress; `azd up`
provisions + deploys in one step. It provisions a resource group, user-assigned managed identity (per-app
since Aspire 9.2+), Basic-SKU ACR, Log Analytics workspace, the Container Apps environment + Aspire
dashboard, then builds images via the .NET SDK container build, pushes to ACR, grants AcrPull.
*(aca-deployment-azd-in-depth; aspire.dev/deployment/azure/container-apps; DevBlog how-to-deploy)*

**Emulator→managed swap needs NO run/publish branching in our code.** Wrap each Azure resource as
`AddAzure*` + `RunAsEmulator()`/`RunAsContainer()`; those calls affect ONLY local run mode and are ignored
in the publish manifest, so local dev is unchanged while generated Bicep provisions the real managed
service. Concertable's exact stack is covered: `AddAzureServiceBus().RunAsEmulator()` (Service Bus
emulator), `AddAzureStorage().RunAsEmulator()` (Azurite), `AddAzureSqlServer().RunAsContainer()` (mssql
container — a container substitution that publishes as managed Azure SQL). Contrast: plain `AddSqlServer()`
publishes as an ACA app running the SQL image, NOT managed Azure SQL. *(integrations-overview.md;
azure/local-provisioning; runasemulator API ref)*

**EF migration strategy — replace nuke-and-rescaffold; NEVER runtime `Migrate()` in prod.** Runtime
`Database.Migrate()` is officially "inappropriate for production" (needs elevated app DB perms, no
rollback, no SQL inspection, pre-EF9 concurrent-apply corruption; EF9 locking mitigates but the doc still
rejects it). Use EF's recommended path: idempotent SQL scripts (`--idempotent`) **or** migration bundles
(`dotnet ef migrations bundle` → `efbundle`, a self-contained exec needing only the .NET runtime — not
`bundle.exe`, that name was refuted) run as a **separate deploy job per service DB**, decoupled from app
startup — removes multi-instance races + the need for elevated app perms. One bundle/script per service
database (B2B, Customer, Auth, Search, Payment). *(ef/managing-schemas/migrations/applying; migration-
bundles DevBlog)*

**SPA hosting — Azure Static Web Apps** handles the Vite SPAs' client-side routing via a
`navigationFallback` rewrite to `/index.html` in `staticwebapp.config.json`.
*(azure/static-web-apps/configuration)*

**Secrets + CD — `azd pipeline config`** auto-creates the service principal, sets pipeline vars/secrets,
and supports GitHub Actions (OIDC by default) or Azure Pipelines (needs a PAT, no OIDC); the operator
needs Contributor to create the SP. *(azure-developer-cli/configure-devops-pipeline)*

**Hard gotchas to plan for:**
- **ACA ingress limits** — each app gets exactly ONE HTTP ingress; external *non-HTTP* endpoints are
  unsupported; extra endpoints publish as TCP ports, **max 5/app** (more → Azure support request). Matters
  if any service needs external TCP. *(aspire.dev container-apps; aca/ingress-overview)*
- **`.AsExisting()` broken in Aspire 13.0.0** — recreated infra instead of reusing a pre-provisioned ACA
  environment (dotnet/aspire #12977, later fixed). Verify our Aspire version before reusing shared infra.
- **Azurite binding-leak (dotnet/aspire #7330, open at research time)** — emulator bindings can leak into
  prod Bicep if a custom binding expression references an emulator-only property (e.g. `bindings.blob.host`).
  Relevant — we use the Storage emulator.

**Gaps this research did NOT answer — carry into later phases (may warrant a targeted follow-up run):**
1. **Cost + effort** — monthly ballpark (~7 ACA + Azure SQL + Service Bus + SWA) and first-deploy effort;
   no figure surfaced.
2. **SPA hosting** — SWA-vs-containerised trade-off, and per-env config injection (API base URLs, auth
   redirect URIs, CORS) at build/deploy time for each of the 4 SPAs.
3. **Secrets wiring** — Key Vault vs ACA secrets pattern under azd for Stripe/JWT/conn-strings; does
   azd/Aspire auto-provision Key Vault + bind via managed identity? rotation?
4. **Multi-DB migration ordering** — cross-service migration-job sequencing vs app rollout for
   zero-downtime; any inter-service schema dependencies.

**Time-sensitivity:** Aspire moves fast — docs migrated learn.microsoft.com → aspire.dev; API churn
(`AddQueue`→`AddServiceBusQueue`, `AddAzureRedis`→`AddAzureManagedRedis`); a newer `aspire deploy` CLI now
co-exists with the azd flow. Re-check against the installed Aspire version before Phase 4.

## Recommended architecture — runtime + design-time + secrets + prod source (Prompt 3 outcome, 2026-07-17)

A `deep-research` pass, run 2026-07-17 (codebase map + official Aspire/EF/Azure docs,
2025-26). Prompt 3's stated landing spot was `CONFIG_STRATEGY.md`, but that doc is now *region*-scoped
and delegates app-wide config here (matching where Prompt 2 landed) — so the architecture lives here.
This section is the **design** the phases below implement; it refines their "how", it doesn't add phases.

**The one-line answer: the Aspire AppHost already *is* the `IConnectionStringProvider`.** Do **not** build
a custom connection-string abstraction. `WithReference(db)` → `ConnectionStrings__<Db>` → the service reads
`ConnectionStrings:<Db>` from `IConfiguration`; that is a single, environment-driven source already. A
custom `IConnectionStringProvider<T>` would only re-wrap `IConfiguration` and duplicate the resiliency /
no orchestrator injecting the string; Aspire does — so **its runtime provider idea does not transfer; the
idiomatic Aspire move is to delete the abstraction, not port it.** (aspire.dev EF SqlServer integration.)

### Runtime (Q1, Q2)

**Q2 — confirmed: no connection-string provider is needed.** Multi-tenancy is shared-DB-per-service with
scoped `ITenantContext` + EF global query filters (`TenantFilters`, read through the DbContext instance,
*switching* is a DB-per-tenant pattern and **does not apply**. A custom provider only earns its keep for
genuinely dynamic resolution (DB-per-tenant, runtime vault fetch, read-replica routing) — none of which
Concertable does.

**Keep `GetConnectionString(<Name>)`; add Aspire enrichment via `EnrichSqlServerDbContext`, NOT
`AddSqlServerDbContext`.** The pooled `AddSqlServerDbContext<T>("<Name>")` one-liner is the textbook Aspire
call, but it registers via `AddDbContextPool`, which **requires a `DbContextOptions`-only constructor**.
Every Concertable context takes extra ctor args (a `*ConfigurationProvider`, plus `ITenantContext` on
B2B Concert/Artist/Venue) and varies its model per instance — fundamentally non-poolable. So the idiomatic
fit is the *enrich* path: keep the existing `AddDbContext` and layer Aspire's retries + `CanConnect` health
check + OpenTelemetry on top.

```csharp
// AddXModule — now takes the builder so it can enrich (was IConfiguration).
public static IHostApplicationBuilder AddTenantModule(this IHostApplicationBuilder builder)
{
    builder.Services.AddDbContext<TenantDbContext>((sp, o) =>
        o.UseSqlServer(builder.Configuration.GetConnectionString(B2BDb.Name))   // constant, not "B2BDb"
         .AddInterceptors(sp.GetRequiredService<AuditInterceptor>(),
                          sp.GetRequiredService<IDomainEventDispatchInterceptor>()));
    builder.EnrichSqlServerDbContext<TenantDbContext>();   // retries + health check + OTel, no pooling
    return builder;
}
```

`<Name>` must equal the AppHost `AddDatabase(<Name>)` name. Tunable per-context under
`Aspire:Microsoft:EntityFrameworkCore:SqlServer[:{ContextName}]` (`DisableRetry`, `DisableHealthChecks`,
`DisableTracing`, `CommandTimeout`). Enrichment is a valuable-for-prod add-on and is *independent* of the
source-of-truth story — adopt it whenever the ~19 module signatures are touched; nothing else here depends
on it.

**Consistency fix (part of the source-of-truth):** every connection name must be a constant whose value
matches the AppHost's `Databases.*`. B2B already has `B2BDb.Name`; introduce the sibling per-service
constant (`CustomerDb.Name`, `SearchDb.Name`, `PaymentDb.Name`, `AuthDb.Name`) and replace the string
literals (`"CustomerDb"` in all 7 Customer modules, and the Search/Payment/Auth literals). The constant
can't cross the carve, so its *value* is matched in two places (AppHost `Databases.X` + the service
constant) — that's carve-correct, and the value is a non-secret name, not a credential.

### Design-time (Q3, Q6)

**`IDesignTimeDbContextFactory<T>` is the canonical, doc-endorsed idiom here** — the documented trigger is
exactly "the ctor takes parameters not registered in design-time DI" (our `*ConfigurationProvider` /
`ITenantContext`). Host-based resolution, `--startup-project`, and a dedicated migrations host don't remove
that need. Migration **bundles/scripts are deploy-time (apply), not design-time (author)** — see prod below.

Replace the 4 hard-coded `DesignTimeConnectionString` helpers (Phase 1a's per-closure consolidation) with a
per-closure **base factory** that (a) resolves from config by the name constant, (b) **throws — no
hard-coded fallback**, (c) collapses each of the 18 factories to ~4 lines. Per-closure (not one shared
package) keeps the carve and matches the granularity Phase 1a already chose; `Microsoft.EntityFrameworkCore.Design`
is `PrivateAssets` design-time-only, so none of this enters any runtime graph.

```csharp
// One per closure (B2B.DataAccess.Infrastructure, Customer.Seed.Infrastructure, Messaging, Auth) —
// where DesignTimeConnectionString already lives. Replaces that helper.
public abstract class B2BDesignTimeDbContextFactory<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    public TContext CreateDbContext(string[] args)
    {
        var cs = DesignTimeConfiguration.Build().GetConnectionString(B2BDb.Name)     // constant, no literal
            ?? throw new InvalidOperationException(
                $"Design-time connection string '{B2BDb.Name}' not set " +
                $"(ConnectionStrings__{B2BDb.Name} via env or user-secrets).");       // surface, don't default away
        var options = new DbContextOptionsBuilder<TContext>().UseSqlServer(cs, ConfigureSqlServer).Options;
        return Create(options);
    }
    protected abstract TContext Create(DbContextOptions<TContext> options);
    protected virtual void ConfigureSqlServer(SqlServerDbContextOptionsBuilder sql) { }
}

// DesignTimeConfiguration.Build() (shared once per closure): env + user-secrets + optional appsettings.
// No connection string is embedded in code — the value comes from config, or it throws.
internal static class DesignTimeConfiguration
{
    public static IConfiguration Build() => new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"}.json", optional: true)
        .AddUserSecrets(typeof(DesignTimeConfiguration).Assembly, optional: true)
        .AddEnvironmentVariables()                                                     // picks up ConnectionStrings__<Db>
        .Build();
}
```

Each concrete factory is then trivial, and the tenant stub becomes a **single** `DesignTimeTenantContext`
in the B2B closure (was duplicated as a nested class in 3 factories):

```csharp
public sealed class ConcertDbContextFactory : B2BDesignTimeDbContextFactory<ConcertDbContext>
{
    protected override ConcertDbContext Create(DbContextOptions<ConcertDbContext> o) =>
        new(o, new ConcertConfigurationProvider(), DesignTimeTenantContext.Instance);
    protected override void ConfigureSqlServer(SqlServerDbContextOptionsBuilder sql) => sql.UseNetTopologySuite();
}
```

**Where the design-time value comes from — the honest split (this is the crux of "make the env-var step
disappear"):**
- **Authoring** (`initial-migrations.ps1` re-scaffold, `migrations add`): EF **never opens the connection**
  — it only needs a *parseable* string to instantiate the provider and build the model. So there is no
  secret and no live DB involved. The one central non-secret dev value lives in **one** place read by
  `DesignTimeConfiguration` (a design-time `appsettings.json`, or the dev's user-secrets, or a single
  `ConnectionStrings__<Db>` export at the top of the script) — **not** re-hard-coded in 4 C# files, and the
  factory **throws** if it's absent rather than silently targeting `localhost…Password11!`.
- **Applying** (real schema change, prod + local): done by Aspire, **no design-time factory, no env-var
  dance** — a per-DB migration bundle/job that carries the *same* `WithReference(db)` the runtime service
  uses, resolving the identical connection string / Key-Vault secret. This is where the manual env-var step
  genuinely disappears (see prod below).

### Secrets + prod config source (Q4, Q5)

**Local secrets — Aspire parameters in the AppHost's user-secrets, replacing gitignored
`appsettings.Development.json`.** The AppHosts already have `UserSecretsId` and the `AddSecrets(...)` →
`WithEnvironment` pattern. Complete it: move Stripe/Google keys out of each service's Development.json into
AppHost user-secrets as `AddParameter("…", secret: true)` / `AddConnectionString(...)`, flow them via
`WithEnvironment`, and rotate the exposed keys. Values live in AppHost `secrets.json`
(`Parameters:*` / `ConnectionStrings:*`), set via `aspire secret set` or the dashboard "save to user
secrets" — one out-of-repo, per-dev store. (Aspire docs: *"Always use `AddParameter` to pass secrets …
never include passwords or connection strings in source code."*) Raw `dotnet user-secrets` is the storage
underneath; the *abstraction* is the Aspire parameter, so the same declaration works local and prod.

**Prod config — Azure App Configuration (tree, per-env) + Key Vault (secrets by reference), one AppHost
with run-vs-publish branching.** Services never change — they only read `IConfiguration`; the AppHost
decides the source:

```csharp
// SQL: the current gap — plain AddSqlServer publishes as an ACA app running the SQL image, NOT managed
// Azure SQL. Switch to the container-substitution form so run = local container, publish = managed Azure SQL.
var sql = builder.AddAzureSqlServer("sql").RunAsContainer();      // was AddSqlServer("sql")
var b2bDb = sql.AddDatabase(Databases.B2B);                        // ConnectionStrings__B2BDb unchanged

// Centralized config + secrets, provisioned on publish, emulated/existing locally.
var appConfig = builder.AddAzureAppConfiguration("appconfig").RunAsEmulator();
var keyVault  = builder.AddAzureKeyVault("secrets");
var stripe    = builder.AddParameter("stripe-secret", secret: true);
var stripeRef = keyVault.AddSecret("stripe-secret", stripe);      // Aspire 9.4 IAzureKeyVaultSecretReference

builder.AddPaymentWeb<Projects.Concertable_Payment_Web>(...)
       .WithReference(appConfig).WithReference(keyVault)
       .WithEnvironment("Stripe__SecretKey", stripeRef);           // KV-backed ACA secret, never in the image
```

On ACA the environment gets a user-assigned managed identity (`AZURE_TOKEN_CREDENTIALS=ManagedIdentityCredential`),
so `DefaultAzureCredential` resolves App Config + Key Vault with no code change; non-secrets flow as env
vars, secrets as Key-Vault-backed container-app secrets resolved at revision start. App Configuration
stores Key Vault *references* (a URI, not the secret); `.ConfigureKeyVault(...)` resolves them. This is the
provider-swap-at-the-composition-root the Decision section already anticipated. (Reconcile provisioning
with the Terraform-not-Bicep standard per the Phase 4 ⚠️ note — the *code shape* above is provider-agnostic;
only who emits the IaC differs.)

**Migrations share the one source of truth.** Prefer Aspire's first-class `AddEFMigrations` (package
`Aspire.Hosting.EntityFrameworkCore`; verify it's in our SDK version) — `RunDatabaseUpdateOnStart()` locally,
`PublishAsMigrationBundle(publishContainer: true).PublishAsAzureContainerAppJob()` for a per-DB run-once ACA
job on publish. Because the job carries the same `WithReference(db)` (and can carry `WithReference(keyVault)`),
it resolves the identical connection string as runtime — the deploy-time realization of Phase 0's
"bundles/idempotent scripts as a separate per-DB job, never runtime `Migrate()`". The older dedicated
migration-worker `BackgroundService` gated by `WaitForCompletion` is the fallback if `AddEFMigrations` isn't
available in-version.

### Where each value lives, per environment

| Value | Local (`aspire run`) | Prod (`aspire publish` → ACA) | Design-time author | Design-time apply |
|---|---|---|---|---|
| Connection strings | AppHost `AddDatabase` → `ConnectionStrings__<Db>` (SQL container) | `AddAzureSqlServer` managed Azure SQL → same env var | one central non-secret value (config/user-secrets); factory throws if absent | Aspire migration job's `WithReference(db)` — same as runtime |
| App secrets (Stripe/Google) | AppHost user-secrets `Parameters:*` → `WithEnvironment` | `AddParameter(secret:true)` → Key Vault secret → KV-backed ACA secret | n/a (never opens external services) | n/a |
| Non-secret config (BlobStorage, CORS via extension) | `SharedDefaults/appsettings.json` seam (lowest precedence) → App Config later | Azure App Configuration | reads config if present; not required | n/a |
| Nothing in source code | — | — | — | — |

### Migration path from today (maps onto the phases below)

1. **Runtime constants + enrich (Phase 1 / after-sync):** add per-service name constants, replace the
   literals; when touching module registration, switch `AddXModule(IConfiguration)` → `IHostApplicationBuilder`
   and add `EnrichSqlServerDbContext<T>()`. Source-of-truth unchanged; zero behavior change to resolution.
2. **Design-time base factory (extends Phase 1a) ✅ DONE (2026-07-17):** replaced the 4
   `DesignTimeConnectionString` helpers with the per-closure base factory + `DesignTimeConfiguration`
   (B2B + Customer); deleted the hard-coded `Password11!` fallback — factories now throw if
   `ConnectionStrings__<Db>` is absent; collapsed the 14 B2B/Customer factories to ~4 lines; single
   `DesignTimeTenantContext`; Auth + Messaging kept their divergent construction, source swapped only.
   `./initial-migrations.ps1` now exports **credential-free** design-time strings (scaffolding never
   connects). Still outstanding: Payment + Search have no design-time factory (they scaffold via their
   Web host as `--startup-project`); converting them is optional.
3. **Secrets → user-secrets/Key Vault (Phase 2):** move Development.json secrets to AppHost parameters;
   delete + history-purge + rotate the orphaned `B2B.Web/appsettings.Production.json` SQL password.
4. **App Config + run-vs-publish (Phase 3) and migration jobs (Phase 4):** `AddAzureAppConfiguration` /
   `AddAzureKeyVault`, switch SQL to `AddAzureSqlServer().RunAsContainer()`, `AddEFMigrations` per DB.

### Rating against the "as clean as an injected `IConnectionStringProvider`" bar

- **Runtime — BEATS the bar.** The cleanest result is *no custom abstraction at all*: a two-liner
  (`AddDbContext(… GetConnectionString(Const) …)` + `EnrichSqlServerDbContext<T>()`) keyed by a constant,
  giving resiliency/health/telemetry for free and nothing to own. The only gap vs the textbook
  `AddSqlServerDbContext("Name")` one-liner is the explicit `GetConnectionString` + `Enrich` line — forced
  by non-poolable ctors, a property of the model, not a design miss.
- **Design-time — MEETS the bar, with one flagged residual.** A 4-line factory + shared base, constants
  only, throws on missing, zero secrets. **What can't reach the bar:** `dotnet ef` runs with Aspire *not
  running*, so authoring still needs a connection string *present in config* — the live orchestrator can't be
  the source at author time. This is a fundamental EF/design-time limitation, not fixable by any abstraction.
  Mitigated to near-nothing: authoring never connects (any parseable value works, from one central non-secret
  place), and applying — the part that needs a *real* string — is fully Aspire-sourced with no env var.
- **Secrets + prod — MEETS the bar.** Aspire parameters + App Config/Key Vault + run-vs-publish give one
  env-driven source, zero secrets in code, identical service code across environments; managed identity
  resolves everything on ACA.

**Sources (key):** aspire.dev EF SqlServer integration + overview (`EnrichSqlServerDbContext`, pooling
constraint); aspire.dev EF migrations (`AddEFMigrations`, bundles/jobs); aspire.dev external-parameters +
Azure overview (run-vs-publish, `AddParameter(secret:true)`, `AsExisting`); aspire.dev what's-new 9.4
(`IAzureKeyVaultSecretReference`); learn.microsoft.com EF `dbcontext-creation` (design-time factory triggers,
"primary smell is a hard-coded connection string") + `migrations/applying` (never runtime `Migrate()`);
learn.microsoft.com App Configuration Key Vault references + Aspire quickstart.

## Phases

### Phase 0 — Deployment research ✅ DONE (2026-07-17)
Ran `deep-research` on the deployment-target question (Prompt 2). Resolved the deployment target (Container Apps
+ azd), the prod EF-migration strategy (bundles / idempotent scripts as a separate deploy job, never
runtime `Migrate()`), SPA hosting (Static Web Apps), and the secrets home (Key Vault + ACA secrets via
azd). Findings, citations, gotchas, and the four gaps it left open are in the **Phase 0 outcome** section
above.

### Phase 1 — Config consolidation (no new infra; valuable on any target)

**Recon (2026-07-17, 4 parallel investigations) refined the original framing in three ways:**
1. **Much of the "duplication" is not flatten-able.** Genuinely identical across services (single-source
   candidates): the 4-origin dev CORS list (5 files), `BlobStorage:ContainerName="images"` (5 files),
   the Auth localhost `SpaClients` block (2 files), the Payment `UseRealStripe` Web/Workers pairs, and
   the SPA port constants (5174–5177). Legitimately **service-specific — do NOT flatten**: the
   `Endpoints` maps (B2B's 708x stack vs Customer's 709x stack per microservice isolation), the
   per-service `ExternalServices` matrices, `Urls:Frontend`, and connection strings.
2. **The carve makes cross-service config-sharing publish-gated.** There is no carve-surviving way to
   share a config *file* by path — only the package boundary survives an independent service carve
   (confirmed: zero `<Link>` usage; cross-folder includes exist only in the exempt E2E harnesses). The
   chosen home is an appsettings-shaped JSON embedded in **`Concertable.ServiceDefaults`** (precedent:
   `Concertable.Shared.Blob.Infrastructure` embeds PNGs as `<EmbeddedResource>`), loaded via
   `AddJsonStream` at **lowest precedence** in `AddServiceDefaults()` (so appsettings/env-vars still
   override — safe "dev fallback" semantics, and the exact provider seam Phase 3 swaps App Config into).
   But `ServiceDefaults` is a version-pinned package (not churny-core-swappable), so **a service can't
   consume the seam in the same PR that adds it** — it needs the repo's normal publish→platform-sync
   round-trip first. This splits Phase 1 into 1a (in-PR) and 1b (publish-gated follow-up).

**Phase 1a — in-PR, no publish gate (THIS PR, `Feature/ConfigConsolidation`):**
- ✅ **18 DbContext design-time factory connection strings** consolidated into **per-closure** helpers
  (`DesignTimeConnectionString`): B2B → `Concertable.B2B.DataAccess.Infrastructure`; Customer →
  `Concertable.Customer.Seed.Infrastructure` (Customer has no DataAccess layer — see debt note);
  Messaging + Auth → same project as their factories. Per-closure (not one global helper) is the
  carve-correct granularity: each independently-deployable unit owns its own design-time DB fallback,
  and a shared helper would be publish-gated anyway. Only 2 literal templates remain (b2b ×3 closures,
  customer ×1) instead of 18 copies. Auth's 2 factories also gained the `ConnectionStrings__B2BDb`
  env-var override the other 16 already had (design-time-only improvement).
- ✅ **B2B CORS dev-bug fix** — `B2B.Web/appsettings.Development.json` allowed `[5174,5175,5176]`
  (customer/venue/artist) but B2B serves the *business-side* SPAs; aligned to `[5175,5176,5177]`
  (venue/artist/business), matching B2B's own E2E list. Dev-only, not exercised by the E2E suite.
  ⚠️ **Local-only — does NOT ship:** `appsettings.Development.json` is gitignored (see note below), so this
  edit lives only in the working tree. To fix dev CORS for every dev it would have to move to a tracked
  file (base) or the after-sync `AddDefaultCors` extension. Flagged for a Phase-1b/2 decision.

**Phase 1b — the shared appsettings source. Two sub-steps, split by the publish gate:**

*1b-now (landed in the SAME PR as 1a — additive / in-closure, no publish gate):*
- ✅ **ServiceDefaults seam (additive):** `Concertable.ServiceDefaults/SharedDefaults/appsettings.json`
  embedded (`<EmbeddedResource>`) + loaded lowest-precedence in `AddServiceDefaults()` via a new
  `AddSharedDefaults()`. Env-layered loader: builds a sub-config from the embedded base +
  `appsettings.{EnvironmentName}.json` **if that resource exists** (absent today → skipped, present after
  after-sync → loaded), then inserts it **once at index 0 of `Configuration.Sources`** so
  appsettings/env-vars/user-secrets all override it (dev-fallback semantics; the exact provider seam
  Phase 3 swaps App Config into). **Footgun captured in code:** it chains a *pre-built* sub-config, not
  direct `JsonStreamConfigurationSource` inserts — `ConfigurationManager` rebuilds every provider on each
  `Sources` mutation, re-reading a one-shot manifest stream to EOF and silently dropping it. Base missing
  = throw (packaging bug); env file missing = skip (genuinely optional). Dormant for all consumers (every
  service references ServiceDefaults by **`<PackageReference>`**, confirmed — so this must publish +
  platform-sync before 1b-after-sync can consume it).
- ✅ **Content decision — only the true intersection goes in the universal package.** ServiceDefaults is
  consumed by *every* service, so shared defaults must be the intersection, not the union (`api/CLAUDE.md`).
  Shared base holds **only `BlobStorage:ContainerName="images"`** (the sole genuinely-universal, env-agnostic
  inventory value). Deliberately **excluded** (would contaminate services that shouldn't carry them):
  **CORS 4-origin list** → routed through the opt-in `AddDefaultCors` extension (after-sync), *not* a blanket
  config value — the 4-list includes the customer origin `5174` that B2B/Auth must never allow;
  **`UseRealStripe`** (intra-Payment Web≡Workers), **`Stripe:SkipWebhookVerification`** (B2B/Customer subset,
  per-service env nuance), **Auth `SpaClients`** (intra-Auth Dev≡E2E, Auth-specific) — each an intra-service
  collapse or subset extension elsewhere, never the universal defaults.
- ✅ **Intra-service redundancy (no gate):** `BlobStorage:ContainerName="images"` in `B2B.Web` base +
  Development + E2E → kept in base (inherited by the env files), dropped from Development + E2E. Base value
  == removed value, so zero behavior change.

*1b-after-sync (only after 1a+seam merges and platform-sync bumps the `ServiceDefaults` pin — a service
can't consume the seam until then):*
- **Cross-service value removal:** drop the *tracked* copies the shared defaults now supply. For
  `BlobStorage:ContainerName` that's `B2B.Web` **base** (the sole remaining tracked copy — Customer's is in
  gitignored Development, so moot; B2B E2E already dropped in 1b-now); after removal B2B falls back to the
  shared base. If more universal values are seeded into the shared base later, their tracked per-service
  copies drop here too.
- **CORS via extension (its own additive-publish step):** add an `AddDefaultCors(origins…)` extension to
  ServiceDefaults carrying the localhost 4-list, publish it (adding a method is additive/safe), then after
  the *next* sync replace the verbatim `AddCors` block in the 4 `Program.cs` (B2B/Customer/Search/Payment).
  CORS origins live in this extension (opt-in, per-service scoping), **not** in the universal shared JSON —
  which is why they were not seeded in 1b-now.

> ⚠️ **Tracked vs local config — `appsettings.Development.json` is gitignored (`.gitignore:389`).** Only
> `appsettings.json` (base), `.E2E.json`, `.Production.json`, `.Testing.json` are tracked; every
> `appsettings.Development.json` is local-only (it holds each dev's own Stripe/Google keys). **Consequences
> the inventory below did not account for:** (1) any Development.json row is moot for the repo — the real
> tracked dedup targets are base + E2E (+ Production for Phase 2); (2) the 1a B2B CORS fix and the 1b B2B
> BlobStorage-Development removal are local-only, don't ship; (3) **Phase 2 recheck** — the "committed"
> Stripe/Google secrets the plan cites in Development.json are **not in the current tree** (gitignored);
> they'd only be in *git history*, so Phase 2's job there is history-purge + rotate, not a working-tree delete.

**1b target inventory (exact single-source candidates — from recon; ⚠️ read the tracked-vs-local note above
before treating any `Development` row as a committable change):**
- **CORS 4-origin list** `["https://localhost:5174","...5175","...5176","...5177"]` — byte-identical in 5
  files: `Customer.Web` Development + E2E, `Payment.Web` base, `Search.Web` Development + E2E. (B2B is
  service-specific — keep its `[5175,5176,5177]` override; do NOT fold it into the shared 4-list.)
  *(Tracked copies: Customer/Search E2E + Payment base; the Development copies are local-only.)*
- **`BlobStorage:ContainerName="images"`** — `B2B.Web` base/Dev/E2E (intra-service, 1b-now) + `Customer.Web`
  Development (cross-service, after-sync). (`AzureBlobStorage` key in `B2B.Web` Production = Phase-2 deletion.)
- **Auth localhost `SpaClients` block** — byte-identical in `Auth` Development + E2E.
- **Payment `UseRealStripe`** — `Payment.Web` base ≡ `Payment.Workers` base (`false`); `Payment.Web` E2E ≡
  `Payment.Workers` E2E (`true`).
- **`Stripe:SkipWebhookVerification: true`** — dev toggle repeated in `B2B.Web` Dev, `Customer.Web` Dev + E2E.

**Flagged inconsistencies (decisions, not silently "fixed"):**
- `UseFakeExternalServices` in `B2B.Web/appsettings.Testing.json` is **dead config** — nothing reads
  it; the real toggles are `ExternalServices:UseReal*`, and Testing gets fakes via the absent-default.
  Slated to be cut at release (per Tommy). Left as-is; remove when the fake-services plumbing goes.
- `BlobStorage` vs `AzureBlobStorage` container key: the `AzureBlobStorage` variant lives **only** in
  the orphaned `B2B.Web/appsettings.Production.json` that **Phase 2 deletes** — resolves itself, no
  Phase 1 action.
- Auth's design-time factories point at the **`concertable-b2b`** database (not an `AuthDb`) — preserved
  as existing behavior; flag for review (is Auth sharing the B2B database intentional?).

### Phase 2 — Secrets out of source → Key Vault
Rotate + remove committed secrets (Stripe keys, Google key, the SQL password); delete the orphaned
Production file. Stand up Key Vault (per Phase 0 target), move secrets there, wire Key Vault references.

### Phase 3 — `config` repo + App Configuration ✅ AUTHORED (2026-07-17); apply deferred (no creds)
The **`config`** repo (sibling to `Concertable/`, bare-name-under-the-Concertable-org) is built:
config-as-code + hand-authored **Terraform** (App Configuration store + Key Vault + non-secret key-values +
secret *references* + role assignments + azurerm remote-state backend) + a GitHub Action + a seed-secrets
script. `terraform fmt`/`validate` green; **apply blocked on an Azure subscription + creds**. Partition by
environment; region seam dormant. The single consumer seam is the `config_label` variable (the App Config
label a consumer selects on) — data, not structure. See the `config` repo README for the platform-agnostic
design rationale.

> ✅ **Provider swap RESTORED (2026-07-17, working tree).** `AddAzureAppConfiguration` is back in
> `Concertable.ServiceDefaults/Extensions.cs`, called from `AddServiceDefaults` right after
> `AddSharedDefaults` — recovered verbatim from the lost pre-merge commit `d669a292` (it was authored
> before PR #119 but never landed — same loss as `DEPLOYMENT.md`). Reads the endpoint from
> `ConnectionStrings:appconfig`; **no-op when absent** (local `aspire run`, tests, and E2E all keep the
> embedded SharedDefaults + user-secrets as the source); when present it connects with
> `DefaultAzureCredential`, selects unlabeled defaults then this environment's overrides, and resolves Key
> Vault references via managed identity. Packages added back to ServiceDefaults:
> `Microsoft.Extensions.Configuration.AzureAppConfiguration` 8.5.0 + `Azure.Identity` 1.16.0. Additive +
> dormant, full-solution build green (0 errors). Publish-gated like the 1b seam — service consumers pick it
> up after the next ServiceDefaults publish + platform-sync.

**Next — custom domains:** Cloudflare + `concertable.co.uk` subdomains, which finalize the per-env
`Auth:Authority` / `Cors:AllowedOrigins` / `Auth:SpaClients:*` values in `config`. Scheme
decided + DNS runbook authored in [`DOMAINS_AND_DNS.md`](DOMAINS_AND_DNS.md); apply blocked on domain
purchase + Cloudflare + Azure creds.

### Phase 4 — Deployment pipeline
IaC + CD to provision + deploy per Phase 0 (target host, prod EF migrations, SPA hosting). **Designed in
detail in [`DEPLOYMENT.md`](DEPLOYMENT.md)** (2026-07-17) — the concrete method (Terraform + `dotnet
publish` images + GitHub Actions; Aspire local-only), resource topology, scaling profile (outbox/inbox →
which hosts can scale to zero), migrations-as-ACA-Jobs, SWA for the SPAs, the easy/consistent local story,
and a first-deploy runbook. **Resolves Phase 0's four open gaps:** cost (honest ~£10–15/mo floor, ~£8 of it
Service Bus Standard; a ≤£5 *ephemeral* mode via `terraform apply`/`destroy` since migrations are
nuke-and-reseed with no prod data), SPA hosting + per-env config (the `vite.config.ts` localhost-`define`
blocker), Key-Vault-vs-ACA secrets (App Config Free + KV references via managed identity), and multi-DB
migration ordering (independent except **B2BDb before Auth** — Auth's idsrv store lives in B2BDb).

## Verification
Per phase: `dotnet build` green + affected tests. Phases that change runtime config need a real
boot/round-trip check (the `verify` skill or a deployed smoke test), not just unit tests.

## Cross-refs
- Region config seam (stays region-scoped): [`CONFIG_STRATEGY.md`](CONFIG_STRATEGY.md)
- Research input: `deep-research` Prompt 2 (run 2026-07-17, see Phase 0 outcome above)
- Launch tracker: [`b2b/LAUNCH_ROADMAP.md`](../launch/LAUNCH_ROADMAP.md) — this is a newly-surfaced launch blocker
