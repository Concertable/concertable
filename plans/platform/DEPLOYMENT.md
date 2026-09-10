# Deploying Concertable to Azure — the method + first-deploy runbook (WORKING)

**Status:** design, not yet executed. This is the concrete execution of `CONFIG_AND_DEPLOYMENT_PLAN.md`
Phase 4 and resolves the four gaps Phase 0 left open (cost, SPA hosting + per-env config, Key-Vault-vs-ACA
secrets wiring, multi-DB migration ordering). Target: a **cheap, private, reproducible test environment**
we can stand up ~a month ahead and keep testable at very low cost — not a public launch. Prod-grade knobs
are called out per section so the same IaC scales up later by changing tfvars, not architecture.

---

## The method, in one breath

1. **Aspire stays local-only** — it orchestrates emulators + services for `aspire run`. It is **not** the
   cloud deployer. (azd's Aspire auto-detection emits **Bicep**; our org standard is **Terraform**, so we
   don't use the azd/Aspire generation path.)
2. **Terraform provisions 100% of Azure infra** (hand-authored modules, remote state, per-env tfvars) —
   reproducible and org-standard.
3. **Images** are built with the .NET SDK container build (`dotnet publish -t:PublishContainer`, no
   Dockerfiles) and pushed to a registry.
4. **GitHub Actions** ties it together: build+push images → `terraform apply` → run per-DB migration jobs →
   roll Container App revisions.
5. **Config + secrets** live in **Azure App Configuration (Free tier)** + **Key Vault**, read by each app
   through its **managed identity** — nothing in the image, nothing in code.
6. **SPAs** deploy to **Azure Static Web Apps (Free tier)**.

**Why no `IConnectionStringProvider` (the Infonetica confusion):** their provider is a *runtime DI* shim
because DbUp gives the app no orchestrator-injected connection string. Here the connection string is just an
**env var** — Aspire injects it locally, the ACA container app injects it in cloud (value from Key Vault via
managed identity). Deployment and "do we need a conn-string abstraction" are unrelated; the answer is still
no (see `CONFIG_AND_DEPLOYMENT_PLAN.md` → "Recommended architecture"). The *runtime* code (`GetConnectionString`
+ `EnrichSqlServerDbContext`) is identical regardless of who provisions the DB.

> **Reconciliation with the Prompt 3 architecture note:** that section floated
> `AddAzureSqlServer().RunAsContainer()` for `aspire publish`. Under **this** (Terraform) method the AppHost
> never publishes, so that change is **unnecessary** — keep the AppHost run-mode only (`AddSqlServer`
> container + `AddAzureServiceBus().RunAsEmulator()` + `AddAzureStorage().RunAsEmulator()`). Terraform owns
> every cloud resource.

---

## Cost — the honest model (and why £5 needs a specific mode)

**Compute idles near-free** (ACA Consumption gives 180,000 vCPU-s + 360,000 GiB-s + 2M requests free per
subscription per month; scale-to-zero costs nothing when idle). **The cost drivers are Service Bus and
SQL**, and one is a hard floor:

| Resource | Cheapest viable | ~£/month (light test use) | Notes |
|---|---|---|---|
| ACA environment + 8 backend apps | Consumption, scale-to-zero + KEDA | **~£0–5** | within the free grant if nothing is pinned always-on |
| **Service Bus** | **Standard** (topics required) | **~£8 (hard floor while it exists)** | Basic can't do topics; ~28 subscriptions in use. Base charge is monthly, can't pause |
| Azure SQL (5 DBs) | Serverless, auto-pause 1h | **~£2 idle → +~£0.20/DB/awake-hour** | storage ~£0.10/GB always; compute only while awake; ~30s cold resume |
| Container registry | **GHCR (public images)** or ACR Basic | **£0** (GHCR) / ~£4 (ACR) | public source/images avoid a runtime pull credential |
| App Configuration | **Free tier** | **£0** | 1,000 req/day — fine (config read at startup + rare refresh) |
| Key Vault | Standard | **~£0–1** | pennies per operation |
| Log Analytics | daily cap | **~£0–3** | cap ingestion; ACA needs a log sink |
| Static Web Apps ×4 | **Free tier** | **£0** | 10 free SWAs/subscription |
| Domain + DNS | Cloudflare | **~£1** | `concertable.co.uk` ~£8–12/yr; CF DNS + Universal SSL + Azure managed certs all **£0** |

**So the realistic always-on floor is ~£10–15/month, ~£8 of it Service Bus Standard — £5 is not achievable
while the namespace exists.** Two operating modes get you there:

- **Mode A — leave it running (cheap-idle): ~£10–15/mo.** Everything provisioned; compute scales to zero;
  SQL auto-pauses; ASB Standard + SQL storage are the standing cost. Simple, always available (~30s cold
  starts). Use this once testing is frequent.
- **Mode B — ephemeral on-demand (recommended for the ≤£5 test phase): ~£1–3/mo at light use.** The whole
  env is Terraform, and **migrations are already nuke-and-reseed with no prod data** — so `terraform apply`
  to spin up for a session (~15–20 min incl. migrate+seed), `terraform destroy` after. You pay only for the
  hours it exists; ASB Standard's £8/mo prorates to pennies for a few hours. Data is ephemeral by design
  (re-seeded each spin-up), which matches how migrations already work. Fully reproducible, pure IaC — not
  hacky, it's the point of the IaC. **This is the best fit for "keep it testable, very low cost, lots of
  lead time."**

**Levers to push either mode toward £5:** GHCR not ACR (−£4); scale-to-zero everything incl. workers via
KEDA (compute → free grant); SQL auto-pause + test in bursts; cap Log Analytics; SWA/App Config free tiers.
The one thing you can't lever away while it exists is ASB Standard — hence Mode B.

### Public-launch cost — ~£80–150/mo (and why user count barely matters)

**Cost here tracks the always-on *baseline*, not the user count.** Requests are effectively free (ACA gives
2M/month; a few hundred users generate a tiny fraction), serverless SQL bills per second of *actual activity*,
and the SPAs are static files on a free CDN. So **ten users and a few hundred users cost about the same** —
what you pay for is renting idle capacity that's ready to respond, not throughput. Launch is the same infra as
test with the money-savers *selectively* turned off — a tfvars change, not a rearchitecture.

| Line | Test | Launch (up to ~hundreds of users) | Note |
|---|---|---|---|
| **Service Bus Standard** | ~£8 | **~£8** | the only truly fixed cost; unchanged until high throughput |
| **Azure SQL (5 serverless DBs)** | auto-pause (~£2) | **~£30–80** | light traffic keeps them mostly near min 0.5 vCore / auto-pausing off-peak; an **elastic pool** caps this ~£15–30 if warm-cost annoys |
| **ACA compute** | scale-to-zero (~£0–5) | **~£25–40** | keep `min_replicas = 1` only on the 1–2 latency-sensitive hosts (auth, customer-web); the rest stay scale-to-zero + KEDA wake |
| Log Analytics (capped) | ~£0–3 | **~£5–15** | keep the daily cap |
| Static Web Apps ×4 | £0 | **£0** | Free tier serves hundreds of users fine (static CDN) |
| Egress + KV + App Config + domain | ~£1 | **~£5–15** | modest bandwidth + pennies |

**Realistic public-launch total: ~£80–150/mo** for responsive always-on — or **~£40–70/mo** if you tolerate
occasional ~30s cold starts (keep aggressive scale-to-zero + SQL auto-pause). **User count is not the driver
until real scale.** You'd only approach £300+/mo at *thousands of concurrent* users — everything pinned at
larger replicas, SQL to a provisioned tier — and that's revenue-backed and a knob-turn away on the same
modules. The earlier ~£380 figure was a pessimistic everything-pinned top-end, not the expected bill.

---

## Scaling profile — the one real architectural decision (outbox/inbox)

6 of the 8 backend hosts run an **outbox dispatcher** (SQL→ASB polling loop) and/or an **inbox consumer**
(persistent ASB `ServiceBusProcessor`) *embedded in the host*, so they can't naively scale to zero:

| Host | Embedded messaging | Test profile (Mode A/B) | Prod profile |
|---|---|---|---|
| `auth` (Web) | outbox + public OIDC | min 0, HTTP scale (wakes to drain outbox in cooldown) | **min 1** |
| `b2b-web` | outbox + inbox | min 0, HTTP + KEDA ASB-subscription scale | **min 1** |
| `customer-web` | outbox + inbox | min 0, HTTP + KEDA ASB scale | **min 1** |
| `payment-web` | outbox + inbox + gRPC + Stripe webhook | min 0, HTTP + KEDA ASB scale | **min 1** |
| `payment-workers` | inbox only | min 0, **KEDA ASB-subscription scale (0→1 on depth)** | **min 1** |
| `search-workers` | inbox only (6 projections) | min 0, **KEDA ASB scale** | **min 1** |
| `search-web` | none (query API) | min 0, HTTP scale | min 1 |
| `b2b-workers` | Azure Functions on ACA, hourly timer | native Functions timer scaling, min 0 | same |

**How scale-to-zero still processes messages (Mode B / cheap Mode A):**
- Inbox consumers (`payment-workers`, `search-workers`, and the Web hosts) get a **KEDA `azure-servicebus`
  scale rule** → ACA wakes them 0→1 when their subscription has messages. Latency = one cold start; fine for
  a test env.
- The **outbox dispatcher** is a DB poller (nothing external to scale on). It drains during the warm window
  after any HTTP request that wrote domain changes (poll interval **must be < ACA cooldown**, default 300s).
  For writes with no following request (e.g. the hourly `b2b-workers` Function), add a **KEDA `cron` rule**
  on that owner host to wake it briefly and drain. This is a per-env *config* choice (min replicas + scale
  rules in Terraform), **not** a code change — which is what keeps it reproducible and non-hacky.

**Prod is a one-knob change:** set `min_replicas = 1` for the 6 messaging hosts in the prod tfvars (drop the
scale-to-zero dependence entirely). Same modules, different variables.

---

## Databases

- **One logical Azure SQL server, five databases**: `B2BDb, CustomerDb, AuthDb, SearchDb, PaymentDb`
  (`azurerm_mssql_server` + 5× `azurerm_mssql_database`, `sku_name = "GP_S_Gen5_1"` serverless,
  `auto_pause_delay_in_minutes = 60`, `min_capacity = 0.5`). The logical server is free; you pay per DB
  (storage always, compute while awake).
- **Auth needs two connection strings** — `AuthDb` *and* `B2BDb` (IdentityServer's operational grant store
  lives in `B2BDb`'s `idsrv` schema). Provision both env vars on the `auth` app.
- Connection strings are **secrets** → stored in Key Vault, surfaced to each app as a Key-Vault-backed ACA
  secret → env var `ConnectionStrings__<Db>`. Runtime code is unchanged (`GetConnectionString(<const>)` +
  `EnrichSqlServerDbContext<T>()`, per the Prompt 3 architecture).
- **Backups:** serverless has automated PITR backups (7-day default) — no action for test. Note that
  **Mode B `terraform destroy` deletes the DBs** (intended: re-seeded on next apply).
- Prod knob: bump `sku_name`/`min_capacity` or move to provisioned GP; disable auto-pause.

## Service Bus (Standard, mandatory)

- `azurerm_servicebus_namespace` **Standard**. Topics/subscriptions/queue provisioned explicitly. The
  topology already exists in code (`AsbTopology.cs` + per-service `*Topology.cs`): topics `event-<name>`,
  one subscription per consumer (e.g. `search-concert-changed`), and one queue
  `command-processstripewebhookcommand`. Mirror it as a Terraform `for_each` over a `locals` map (or
  generate the map from the topology to avoid drift — see Open items).
- No sessions, no broker duplicate-detection (idempotency is the in-app Inbox on `MessageId`) → **no
  Premium** needed.
- Namespace connection = secret → Key Vault → app env var `ConnectionStrings__asb` (matches the current key).

## Config + secrets

**Split: non-secret config → App Configuration; secrets → Key Vault (referenced from App Config).**

- **App Configuration (Free tier)** holds the non-secret tree per environment — the same shape as the
  `SharedDefaults/appsettings.json` seam already layered by `AddServiceDefaults`. Wire it at the composition
  root with `builder.AddAzureAppConfiguration("appconfig")` (client integration) — a provider swap, business
  code untouched. This is the cloud realization of the seam Phase 1b built.
- **Key Vault** holds secret *values* (SQL/ASB connection strings, Stripe secret+webhook, Google Maps key,
  service-auth client secrets, JWT signing material). App Config stores **Key Vault references** (a URI, not
  the secret); `.ConfigureKeyVault(...)` resolves them.
- **Managed identity** is the resolver: `azurerm_user_assigned_identity` per app (or one shared), granted
  `Key Vault Secrets User` + `App Configuration Data Reader`. On ACA, Aspire/ACA sets
  `AZURE_TOKEN_CREDENTIALS=ManagedIdentityCredential`, so `DefaultAzureCredential` "just works" — no creds in
  code. Locally, `DefaultAzureCredential` falls back to your `az login`.
- **`config` repo (Phase 3):** the non-secret App Config key-values are config-as-code (JSON),
  applied by Terraform (`azurerm_app_configuration_key`). Secrets are declared as *references* only; their
  values are set out-of-band into Key Vault (never in git).

## Migrations (per-DB, deploy-time, never runtime `Migrate()`)

- Author with `initial-migrations.ps1` (unchanged — the nuke-and-rescaffold; EF never connects at author
  time; the design-time factory throws if no connection string, no hard-coded fallback — per Prompt 3).
- **Apply as one ACA Job per database** (`azurerm_container_app_job`, `trigger_type = "Manual"`), running a
  migration bundle (`dotnet ef migrations bundle` → `efbundle`) or `--idempotent` script. Each job carries
  the same Key-Vault-backed connection string as the runtime app — one source of truth. (If our Aspire SDK
  has `AddEFMigrations`, its `PublishAsMigrationBundle().PublishAsAzureContainerAppJob()` generates exactly
  this; otherwise author the bundle + job by hand.)
- **Ordering:** the 5 DBs are independent **except Auth writes to `B2BDb`** (idsrv schema). Run the `B2BDb`
  migration job **before** the `auth` app's first revision. Otherwise no cross-DB ordering constraints.
- CI sequence per deploy: `terraform apply` → start the 5 migration jobs (B2BDb first) → wait for success →
  roll app revisions.

## SPAs → Azure Static Web Apps (Free) — has real prep work

- 4 Vite/React SPAs: `web-customer` (`app/web/customer`), `web-venue` (`app/web/b2b/venue`), `web-artist`
  (`app/web/b2b/artist`), `web-business` (`app/web/b2b/business`). Each → one `azurerm_static_web_app` (Free
  tier). ✅ **`staticwebapp.config.json` authored** in each app's `public/` (Vite copies it to `dist/`) with a
  `navigationFallback` rewrite → `/index.html` (asset paths excluded) for client-side routing.
- ✅ **Build-time localhost bake — RESOLVED.** Each `vite.config.ts` `define` is now **conditional on
  `command`**: the dev/E2E block (`command === 'serve'`) is byte-identical to before (localhost via the
  running Aspire harness — customer intentionally keeps the standalone Customer AppHost's 7093/7097), and a
  new `command === 'build'` block sources real per-env URLs from `app/web/.env.production` via `loadEnv`.
  Per-app OIDC client-id/scope stay literal `define`; per-app URL aliasing (which backend is the generic
  `VITE_API_URL`/`VITE_BASE_URL`, and b2b's payout-through-b2b `VITE_PAYMENT_API_URL`) is derived from the
  distinctly-named `*_API_URL` vars; shared per-env values (`VITE_AUTH_AUTHORITY`, search/payment, web URLs,
  publishable keys) flow natively from `.env.production`. `.env.production` now carries the **prod** (bare
  `concertable.co.uk`) hosts per the DNS scheme; non-prod (staging/dev) overrides per-env via CI `VITE_*`
  (`process.env` wins over the file). Publishable Stripe + Maps keys are left **blank in git** (CI injects).
  Verified: all 4 `vite build`s green; bundles bake the `*.concertable.co.uk` hosts with **zero localhost**.
- Auth wrinkle: each SPA's OIDC redirect URIs + the Auth `SpaClients`/CORS lists must include the deployed
  SWA hostnames — parameterize per environment.

## Images

- `dotnet publish <proj> -t:PublishContainer -p:ContainerRegistry=<registry>` for the 5 Web hosts +
  `payment-workers` + `search-workers` (plain ASP.NET Core / Worker SDK → works out of the box, no
  Dockerfiles).
- **`b2b-workers` is Azure Functions v4 isolated on Azure Container Apps.** The project is a
  single function — `ConcertFinishedFunction`, an hourly `[TimerTrigger("0 0 * * * *")]`, no HTTP/queue
  triggers. It stays a native Functions app, but is built with the Functions base image and deployed by
  immutable digest like every other runtime. Consumption/Flex Consumption cannot run custom containers;
  Functions on ACA preserves the trigger model, scale-to-zero, and the same artifact across local,
  system-test, production, and rollback environments.
- **Registry: public GHCR.** Canonical source is already public, and images become public only after
  provenance, vulnerability, layer-secret, and clean-pull checks pass. ACA and local AppHosts pull
  anonymously, so there is no long-lived GHCR credential to distribute. ACR with managed identity remains
  the fallback if images ever need to become private.

## Terraform layout

```
infra/
  modules/
    aca-environment/     # Log Analytics + ACA env + shared user-assigned identity
    aca-app/             # one container app: image, ingress, env, KV-secret refs, scale rules (min/max, KEDA)
    aca-job/             # migration job (manual trigger)
    sql/                 # logical server + 5 serverless DBs
    servicebus/          # namespace(Std) + topics/subscriptions/queue (for_each over topology map)
    appconfig/           # App Configuration (Free) + key-values
    keyvault/            # vault + secrets(by reference) + role assignments
    static-web-app/      # one SWA per SPA
  envs/
    test/  (main.tf, test.tfvars)     # min_replicas=0, scale-to-zero, serverless auto-pause
    prod/  (main.tf, prod.tfvars)     # min_replicas=1, larger SKUs
  backend.tf             # remote state: azurerm backend (a storage account + container)
```

- **State:** remote `azurerm` backend (one storage account/container) so applies are reproducible and
  team-safe. Image tags passed in as a variable (e.g. the git SHA) so `apply` rolls new revisions.
- **Secrets are never in tfvars** — Terraform declares the Key Vault secret *names/references*; values are
  set out-of-band (`az keyvault secret set`, or a bootstrap script gated on operator identity).

## CI/CD (extend `.github/workflows/`)

Add a `deploy.yml` (today CI does build/test/NuGet-publish/mirror only — **no CD, confirmed**):

1. **Auth:** GitHub OIDC → Azure (federated credential; no stored SP secret) — `azure/login` with
   `client-id`/`tenant-id`/`subscription-id`.
2. **Build+push images** (matrix over the hosts) to GHCR, including the Functions-container build for
   `b2b-workers`; record each pushed manifest digest against the git SHA.
3. **`terraform apply`** (env-selected) with the immutable image digests.
4. **Migrations:** `az containerapp job start` for the 5 DBs (B2BDb first) → poll to success.
5. **Roll revisions:** `apply` already points apps at the new digests; confirm healthy.
6. **SPAs:** build each with per-env `VITE_*` → deploy via the Static Web Apps deploy action.

For **Mode B (ephemeral)**: a manual `workflow_dispatch` `spin-up` (apply + migrate + seed) and `tear-down`
(`terraform destroy`) pair — spin up for a test session, destroy after.

## Local provisioning — easy & consistent (the other half of the ask)

The goal: one command to a full local stack, and the **same config keys** locally and in cloud (only the
*source* differs — emulators local, managed Azure in cloud).

- **Run:** `aspire run` on the monorepo AppHost (`api/Concertable.AppHost`) boots the SQL container, ASB
  emulator, Azurite, all 8 backend hosts, and the 4 SPAs. This is unchanged and stays the local story — the
  AppHost is run-mode only (no publish branching, since Terraform owns cloud).
- **Secrets — mechanism wired; consolidation is optional dev hygiene, NOT deploy prep.** `UserSecretsId` is
  present on all 6 AppHosts and user-secrets already load in Development (they're the live source of
  `Parameters:sql-password`). The local dev secrets that exist today — `dev-*-not-for-production` service-auth
  client secrets, the `UseReal*=false` mock toggles, Stripe **test** keys + a Google key in the gitignored
  `B2B.Web`/`Customer.Web`/`Auth` dev files — are **purely local-run conveniences**. Prod reads every real
  secret from Key Vault/App Config and never sees these files or values, so where a fake dev placeholder lives
  (`appsettings.Development.json` vs `secrets.json`) is deploy-irrelevant. Making user-secrets the single local
  source is a tidy-up to do *if the scattered dev-file state ever annoys someone* — it's 100% local, has no
  committable diff, and blocks nothing. (Prompt 3 / Phase 2.)
- **Consistency:** local reads the exact same `IConfiguration` keys the cloud apps read
  (`ConnectionStrings__<Db>`, `ConnectionStrings__asb`, `Stripe__*`, `BlobStorage__*`); locally Aspire
  supplies them from emulators + user-secrets, in cloud the ACA app supplies them from Key Vault/App Config.
  No `#if`/env-sniffing in business code.
- **One-command bootstrap** (document in the README): `git clone` → `aspire secret set` the handful of
  secrets (or a `bootstrap.ps1` that prompts once) → `aspire run`. New dev is running in minutes with no
  appsettings editing.

## First-deploy runbook (ordered)

1. **Prep code** (before any cloud): ✅ SPA `vite.config.ts` `define` de-hardcoded + real `.env.production` +
   `staticwebapp.config.json` (done — see "SPAs → Azure Static Web Apps"); ✅ `b2b-workers` home **decided:
   Azure Functions on Azure Container Apps** (see Images); delete + history-purge + rotate the orphaned
   `B2B.Web/appsettings.Production.json` SQL password (Phase 2). *(The old "move local dev secrets to
   user-secrets" item is **dev-only hygiene, not cloud prep** — prod reads secrets from Key Vault and never
   sees the local dev files; see "Local provisioning / Secrets".)*
2. **Bootstrap Azure:** resource group, Terraform state storage, GitHub OIDC federated credential.
3. **Author Terraform modules** (above); commit `test` env tfvars (scale-to-zero, serverless auto-pause).
4. **Seed secrets** into Key Vault out-of-band (SQL/ASB conn strings are outputs of `apply`, so set the
   app-facing secrets after the infra apply, or let Terraform wire resource connection strings → KV directly).
5. **First `terraform apply`** (test env) → infra live.
6. **Build+push images**; **run the 5 migration jobs** (B2BDb first); **seed data**.
7. **Deploy SPAs** to SWA with per-env `VITE_*`; register their hostnames in Auth OIDC redirect URIs + CORS.
8. **Smoke test** (a `verify`-style round-trip: register/login via Auth, a B2B write → event → Search/Customer
   projection → a Customer read; a Stripe test webhook). Then either leave it (Mode A) or `destroy` (Mode B).

## Open items / prep before first deploy

- ~~**SPA build config** — de-hardcode `vite.config.ts` `define`; real per-env `VITE_*`.~~ ✅ **DONE** —
  `define` is `command`-conditional (dev unchanged; `build` sources `.env.production`), `.env.production`
  filled with the prod DNS-scheme hosts, `staticwebapp.config.json` shipped in each SPA. All 4 builds green.
- **ASB topology drift** — generate the Terraform topic/subscription map from `*Topology.cs`, or accept
  hand-maintained parity. (nice-to-have)
- ~~**`b2b-workers` (Functions)** — confirm Functions hosting.~~ ✅ **DECIDED — Azure Functions on Azure
  Container Apps**, using the same immutable Functions image in local, system-test, and deployment.
- **Custom domains / OIDC — Cloudflare + `concertable.co.uk` — scheme decided + runbook authored 2026-07-17,
  see [`DOMAINS_AND_DNS.md`](DOMAINS_AND_DNS.md).** Per-surface subdomains: `customer.` / `venue.` /
  `artist.` / `business.` (SPAs on SWA), `auth.` + per-service `b2b-api.` / `customer-api.` / `search-api.` /
  `payment-api.` (backends on ACA); prod bare, non-prod nests one level (`customer.dev.`, `auth.staging.`).
  Cloudflare is authoritative DNS; app hosts **DNS-only** so Azure's per-host managed certs validate; apex +
  `www` proxied → 301 to `customer.`. `Auth:Authority` / `Cors:AllowedOrigins` / `Auth:SpaClients:*` finalized
  in the `config` tfvars to match. Each host still needs its SWA/ACA custom-domain binding +
  validation + managed cert (the CNAME targets + `asuid` tokens are apply outputs). Blocked on domain purchase
  + Cloudflare + Azure creds. *(The earlier "`api.` service ingress" was shorthand — resolved to per-service
  API subdomains; see the doc's flagged decisions.)*
- **Log Analytics cap** — set a daily cap so logging can't surprise the bill.
- **Terraform ↔ azd** — decision stands: hand-authored Terraform (org standard), `dotnet publish` for images,
  no azd. Revisit only if the hand-authored ACA definitions become burdensome.

## Cross-refs
- Workstream plan + phases: [`CONFIG_AND_DEPLOYMENT_PLAN.md`](CONFIG_AND_DEPLOYMENT_PLAN.md) (this is its Phase 4 +
  the resolution of Phase 0's four open gaps).
- Custom domains / Cloudflare DNS scheme + runbook: [`DOMAINS_AND_DNS.md`](DOMAINS_AND_DNS.md).
- Runtime/design-time/secrets architecture: `CONFIG_AND_DEPLOYMENT_PLAN.md` → "Recommended architecture".
- Region config (separate, dormant): [`CONFIG_STRATEGY.md`](CONFIG_STRATEGY.md).
