# Production config, secrets & deployment — plan (WORKING)

**Status:** design starting (2026-07-17). Triggered by an investigation that found the app has **no
production existence** — no deployment path, no config/secrets store, secrets committed to source.
This is the **app-wide** config/secrets/deployment workstream; the *region*-scoped config seam lives
separately in [`CONFIG_STRATEGY.md`](./CONFIG_STRATEGY.md). Branch when Phase 1 code starts (doc-only
until then).

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
- **Native IaC** — Bicep or Terraform for the App Config store + Key Vault + key-values/references;
  applied in one GitHub Action (`az appconfig kv import` or `azurerm_app_configuration_key`).
- **Flow** — repo → CI → App Config + Key Vault → services via `IConfiguration`. In code it's a
  **provider swap at the composition root** (`AddAzureAppConfiguration` + KV refs); business code
  untouched (the typed-options seam already exists — confirmed by the runtime-wiring investigation).

## Key open decision — deployment target (drives the delivery leg)
*How* config reaches a running service depends on *where* it runs. The Aspire-native path is **Azure
Container Apps via `azd`** (generates Bicep, provisions Container Apps + Key Vault, wires config/secrets
in almost for free). This decides App Config provider vs Container Apps secrets vs both. **Resolve in
Phase 0.** Does NOT block the config-as-code consolidation or secrets-into-Key-Vault work.

## Phases

### Phase 0 — Deployment research (DO FIRST, in a fresh context — token-heavy)
Run the `deep-research` skill on: deploying a .NET Aspire app to Azure with production config + secrets.
Input already written: `DEEP_RESEARCH_PROMPTS.md` **Prompt 2**. Output: recommended deployment target
(Container Apps + azd?), prod EF-migration strategy, how the Vite SPAs are hosted (Static Web Apps vs
containers), and where config/secrets live (App Config + Key Vault via azd). **→ resolves the open
decision above and grounds every later phase.**

### Phase 1 — Config consolidation (no new infra; valuable on any target)
Single-source the duplicated non-secret config (CORS, `Auth.Authority`/`SpaClients`, endpoint maps,
`ExternalServices` toggles, blob container names) and the ~20 hardcoded DbContext-factory connection
strings. Appsettings-shape config consumed locally as the dev fallback.

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
