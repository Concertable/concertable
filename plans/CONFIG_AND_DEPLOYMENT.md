# Production config, secrets & deployment — plan (WORKING)

**Status:** Phase 0 (deployment research) DONE 2026-07-17 — target resolved (ACA via `azd`; IaC =
**Terraform**). Phase 1a (factory consolidation + B2B CORS fix) **and Phase 1b-now (ServiceDefaults
shared-defaults seam + B2B BlobStorage intra-service collapse)** done + build-green on
`Feature/ConfigConsolidation` (uncommitted). Phase 1b-after-sync (publish-gated cross-service dedup +
`AddDefaultCors`) + Phases 2-4 outstanding. Triggered by an investigation that found the app has **no production existence** — no
deployment path, no config/secrets store, secrets committed to source. This is the **app-wide**
config/secrets/deployment workstream; the *region*-scoped config seam lives separately in
[`CONFIG_STRATEGY.md`](./CONFIG_STRATEGY.md). Branch when Phase 1 code starts (doc-only until then).

## Why — investigation findings (2026-07-17)
Three parallel investigations mapped the current state. Headlines (all confirmed with file paths):

### Deployment — none
- No IaC (Bicep/Terraform/ARM/azd), no Dockerfiles, no CD. CI builds/tests/publishes NuGet + mirrors
  service folders, but **deploys nothing** (no `azure/login`, no Azure credentials anywhere).
- App runs only locally: **6 Aspire AppHosts against emulators** (SQL container, ASB emulator, Azurite).
  No run-vs-publish branching to swap emulators for managed Azure.
- Independently confirmed by the repo's own `DEEP_RESEARCH_PROMPTS.md` Prompt 2.

### Config / secrets store — none
- **Zero** Azure App Configuration, **zero** Key Vault. Config = layered `appsettings.*.json` +
  user-secrets (dev) + env vars Aspire injects at runtime.
- Dev config **duplicated across ~40 files**: same Stripe keys, Google Maps key, service-auth secrets,
  CORS lists, OIDC `SpaClients` blocks, endpoint/port maps; the localhost SQL string is re-hardcoded in
  **~20 DbContext design-time factories**.
- Prod config **effectively nonexistent**: one orphaned `B2B.Web/appsettings.Production.json`; no
  staging anywhere; SPA `.env.production` is a stub (empty API URL/authority, no Stripe key).
- The **one** thing already centralized: infra connection strings (SQL/ASB/Blob) — but in Aspire code
  (`AppHost.Shared/DistributedApplicationBuilderExtensions.cs`), not config files.

### Committed secrets — security, fix regardless of the big plan
- Stripe **test** keys (`sk_test`/`pk_test`/`whsec`) duplicated in B2B.Web + Customer.Web
  `appsettings.Development.json`.
- Google Maps API key committed in 3 files.
- **`B2B.Web/appsettings.Production.json` commits a plaintext Azure SQL admin password (`Password11!`).**
  It's in git history → rotate + purge (or rotate + accept). The file is orphaned (nothing provisions it).

## Decision (2026-07-17): build `concertable-config`
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
`deep-research` on `DEEP_RESEARCH_PROMPTS.md` Prompt 2: 28 sources → 126 claims → **24 confirmed by 3-0
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

## Phases

### Phase 0 — Deployment research ✅ DONE (2026-07-17)
Ran `deep-research` on `DEEP_RESEARCH_PROMPTS.md` Prompt 2. Resolved the deployment target (Container Apps
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

### Phase 3 — `concertable-config` repo + App Configuration
Create the repo (config-as-code + native IaC). Provision App Config. Wire the provider at each service's
composition root. Partition by environment; region seam dormant.

### Phase 4 — Deployment pipeline
IaC + CD to provision + deploy per Phase 0 (target host, prod EF migrations, SPA hosting).

## Verification
Per phase: `dotnet build` green + affected tests. Phases that change runtime config need a real
boot/round-trip check (the `verify` skill or a deployed smoke test), not just unit tests.

## Cross-refs
- Region config seam (stays region-scoped): [`CONFIG_STRATEGY.md`](./CONFIG_STRATEGY.md)
- Research input: `DEEP_RESEARCH_PROMPTS.md` Prompt 2
- Launch tracker: [`b2b/LAUNCH_PLAN.md`](./b2b/LAUNCH_PLAN.md) — this is a newly-surfaced launch blocker
