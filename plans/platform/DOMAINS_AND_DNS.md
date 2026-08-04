# Domains, subdomains & Cloudflare DNS — scheme + runbook (WORKING, author-only)

**Status:** authored 2026-07-17, **not executed** — the domain isn't purchased, Cloudflare isn't wired,
and there are no Azure creds yet. This resolves the "Custom domains / OIDC — Cloudflare" open item in
[`DEPLOYMENT.md`](DEPLOYMENT.md) and finalizes the placeholder subdomain values the config repo
(`config`) already carries. It is the bridge between two other pieces of work:

- **`config` (Phase 3)** owns the *config values* that key off these hostnames —
  `Auth:Authority`, `Cors:AllowedOrigins`, and (now) `Auth:SpaClients:*` — per environment in App Config.
  This doc is where those hostnames are decided; the tfvars are finalized to match (see "What changed").
- **`DEPLOYMENT.md` (Phase 4)** owns the *infra* that binds these hostnames to hosts — the per-app SWA/ACA
  `custom_domain` + managed-cert Terraform. This doc is the DNS + validation runbook that plugs into it.

Nothing here can be applied until the domain exists and Cloudflare + Azure are wired — it is a spec and a
runbook, authored ahead so the DNS work is a lookup-and-type job when the domain lands.

---

## The scheme (decided)

One subdomain per surface. Frontends (SPAs) on **Azure Static Web Apps**; backends (Auth + the APIs) on
**Azure Container Apps**. Environments are a **subdomain prefix**: prod is bare, non-prod nests one level.

| Surface | prod host | non-prod host (`<env>` = `dev`/`staging`) | Host platform | OIDC client | Browser-facing? |
|---|---|---|---|---|---|
| **Customer SPA** | `customer.concertable.co.uk` | `customer.<env>.concertable.co.uk` | SWA | `customer-web` | yes (frontend) |
| **Venue SPA** | `venue.concertable.co.uk` | `venue.<env>.concertable.co.uk` | SWA | `venue-web` | yes (frontend) |
| **Artist SPA** | `artist.concertable.co.uk` | `artist.<env>.concertable.co.uk` | SWA | `artist-web` | yes (frontend) |
| **Business SPA** | `business.concertable.co.uk` | `business.<env>.concertable.co.uk` | SWA | *(none — unauthenticated slice)* | yes (frontend) |
| **Auth** (IdentityServer) | `auth.concertable.co.uk` | `auth.<env>.concertable.co.uk` | ACA | — (is the authority) | yes (OIDC endpoints) |
| **B2B API** | `b2b-api.concertable.co.uk` | `b2b-api.<env>.concertable.co.uk` | ACA | — | yes (venue/artist/business call it) |
| **Customer API** | `customer-api.concertable.co.uk` | `customer-api.<env>.concertable.co.uk` | ACA | — | yes (customer SPA calls it) |
| **Search API** | `search-api.concertable.co.uk` | `search-api.<env>.concertable.co.uk` | ACA | — | yes (customer SPA calls it) |
| **Payment API** | `payment-api.concertable.co.uk` | `payment-api.<env>.concertable.co.uk` | ACA | — | yes (customer SPA + Stripe webhook) |
| **apex + www** | `concertable.co.uk`, `www.concertable.co.uk` | *(prod only)* | Cloudflare redirect | — | redirect → customer |

**Who calls what** (grounds the CORS + API-host list; from the SPA `vite.config.ts`/`.env` and Auth `Config.cs`):
- **Customer SPA** → Auth + Customer API + Search API + **Payment API directly** (customer pays direct, mints its own `owner`).
- **Venue / Artist SPA** → Auth + B2B API. Payout calls go **through B2B's own backend** (tenant-scoped Stripe proxy), *not* the Payment host — so venue/artist never hit `payment-api.` directly.
- **Business SPA** → B2B API only, **no OIDC** (`VITE_USE_MOCK_DASHBOARD=true`, minimal slice).

**Internal service-to-service traffic (gRPC, outbox→ASB) does NOT use these public subdomains.** On ACA those
calls go over the environment's **internal** DNS/ingress. The public `*-api.` hosts are the *browser-facing*
edge only. This keeps the public surface exactly the five API/Auth hosts above and nothing more.

### Two flagged judgment calls (author-only — change here is cheap, nothing is deployed)

**1. Customer app: `customer.` subdomain (chosen) vs the apex.**
The committed `Concertable.Auth/appsettings.json` and `app/web/.env.production` currently put the customer app
on the **apex** (`https://concertable.co.uk`). The prompt's scheme + the `config` tfvars say
`customer.`. **Decision: `customer.concertable.co.uk` is canonical**, and the **apex + `www` 301-redirect to it**
(so the bare domain isn't dead). This makes the scheme uniform — every surface is a subdomain — and keeps all
the per-env CORS/redirect lists symmetric. The Auth `appsettings.json` customer entry is updated to match
(see "What changed").
*Alternative (apex-canonical):* serve the customer SWA on the apex and redirect `www` → apex — the stronger
consumer-brand choice, and what the committed config assumed. Flipping to it later is a localized change:
serve customer on apex (SWA apex binding, below), swap the redirect direction, and update three things —
`Auth:SpaClients:Customer:*`, the `customer` entry in `Cors:AllowedOrigins`, and `VITE_CUSTOMER_WEB_URL`.
Because it's that contained, I took the prompt's `customer.` now rather than block; say the word to flip it.

**2. APIs: per-service subdomains (chosen) vs one `api.` gateway.**
`DEPLOYMENT.md` wrote "`api.` (service ingress on ACA)" as shorthand, but the SPAs call **four distinct API
services** by separate base URLs, and ACA gives each app exactly one ingress + its own FQDN. **Decision:
per-service API subdomains** (`b2b-api.`, `customer-api.`, `search-api.`, `payment-api.`) — ACA-idiomatic,
one custom-domain binding per app, **no gateway to build or own**.
*Alternative (single `api.`):* one `api.concertable.co.uk` with path routing (`/b2b`, `/customer`, …) behind a
gateway (Azure Front Door / APIM / a Cloudflare Worker). One origin and one CORS entry, but it's new infra to
stand up and maintain — not worth it for the test phase. Revisit if a single edge (WAF/rate-limit) is wanted.

---

## Cloudflare setup

**Registrar / nameservers.** Buy `concertable.co.uk` (any registrar, or Cloudflare Registrar at cost), then set
the domain's nameservers to the two Cloudflare assigns. Cloudflare is authoritative DNS; Azure is never
queried for DNS — it only validates ownership and serves per-host managed certs.

**Proxy mode — the one decision that matters here:**

- **App hostnames (all SWA + ACA hosts) → DNS-only (grey cloud).** Azure SWA and ACA both issue **free managed
  TLS certs**, but issuance/renewal validates the hostname *to Azure* (CNAME + a TXT token). Proxying (orange)
  cloaks the CNAME behind Cloudflare's IPs and can block managed-cert validation. DNS-only also sidesteps the
  Universal-SSL depth limit below. Start here for every app host; it's the mode that "just works" with Azure certs.
- **apex + `www` → proxied (orange).** These carry no Azure resource — they're pure redirects, which run on
  Cloudflare's edge and therefore need the record proxied. Point apex at a dummy target (`AAAA 100::` or
  `A 192.0.2.1`) and attach a **Redirect Rule** (below). `www` the same.

> **Universal SSL depth gotcha (why DNS-only for non-prod matters).** Cloudflare's free Universal SSL covers
> `concertable.co.uk` and `*.concertable.co.uk` — **one** label deep. It does **not** cover
> `auth.dev.concertable.co.uk` (two deep). If you ever proxy the non-prod app hosts you'd need Advanced
> Certificate Manager (~$10/mo) or Total TLS. Keeping the Azure-bound hosts **DNS-only** makes Cloudflare's
> edge cert irrelevant — **Azure** serves the per-host managed cert regardless of nesting depth — so this
> never bites. It's a reason the whole scheme leans DNS-only for app hosts.

**Later (optional), proxying the SPA hosts for WAF/caching/DDoS** is a deliberate follow-up, not first-deploy:
set SSL mode **Full (strict)**, put an Azure-origin cert on the SWA, and add ACM for the two-deep non-prod
hosts. Don't do it during initial cert issuance.

---

## Azure custom-domain binding — the mechanics per platform

The exact CNAME targets and TXT tokens come from the **deployment infra apply** (`DEPLOYMENT.md` Phase 4),
because you can't know a SWA's `*.azurestaticapps.net` hostname or an ACA app's verification ID until the
resource exists. This section is the shape; fill the values from the apply outputs.

**Static Web Apps (customer / venue / artist / business):**
1. Terraform binds `azurerm_static_web_app_custom_domain` for `<surface>.<env?>.concertable.co.uk`.
2. DNS: a **CNAME** `<surface>` (or `<surface>.<env>`) → the SWA default hostname `xxxxx.azurestaticapps.net`.
3. SWA validates ownership **via that CNAME** (subdomains) and auto-issues + auto-renews a managed cert. No TXT needed for a subdomain.
4. Client-side routing: each SWA ships `staticwebapp.config.json` with `navigationFallback` → `/index.html` (already tracked in `DEPLOYMENT.md`'s SPA prep).

**Container Apps (auth / b2b-api / customer-api / search-api / payment-api):**
1. Terraform binds the ACA custom domain + a **managed certificate** (free) on the app's ingress.
2. DNS: a **CNAME** `<surface>` (or `<surface>.<env>`) → the app's default FQDN `app.<env-hash>.<region>.azurecontainerapps.io` (or an **A** record → the ACA environment's static inbound IP).
3. Ownership: a **TXT** `asuid.<surface>` = the app's custom-domain verification ID (from the apply).
4. ACA issues + renews the managed cert once validation passes.

**apex + www (Cloudflare only, no Azure resource):**
- Proxied dummy record at apex + a **Redirect Rule**: `concertable.co.uk/*` and `www.concertable.co.uk/*` → `https://customer.concertable.co.uk/$1` (301, preserve path/query).
- (If apex-canonical is chosen instead — flagged call #1 — the apex gets a real SWA binding via CNAME-flattening + a TXT validation token, and `www` redirects to apex.)

### DNS records — prod, once the app hosts exist

All app-host records **DNS-only**; apex/www **proxied**. `⟶` targets are apply outputs.

| Name | Type | Target | Proxy |
|---|---|---|---|
| `customer` | CNAME | `⟶ customer SWA .azurestaticapps.net` | DNS-only |
| `venue` | CNAME | `⟶ venue SWA .azurestaticapps.net` | DNS-only |
| `artist` | CNAME | `⟶ artist SWA .azurestaticapps.net` | DNS-only |
| `business` | CNAME | `⟶ business SWA .azurestaticapps.net` | DNS-only |
| `auth` | CNAME | `⟶ auth ACA FQDN` | DNS-only |
| `asuid.auth` | TXT | `⟶ auth ACA verification id` | — |
| `b2b-api` | CNAME | `⟶ b2b-web ACA FQDN` | DNS-only |
| `asuid.b2b-api` | TXT | `⟶ b2b-web verification id` | — |
| `customer-api` | CNAME | `⟶ customer-web ACA FQDN` | DNS-only |
| `asuid.customer-api` | TXT | `⟶ customer-web verification id` | — |
| `search-api` | CNAME | `⟶ search-web ACA FQDN` | DNS-only |
| `asuid.search-api` | TXT | `⟶ search-web verification id` | — |
| `payment-api` | CNAME | `⟶ payment-web ACA FQDN` | DNS-only |
| `asuid.payment-api` | TXT | `⟶ payment-web verification id` | — |
| `@` (apex) | AAAA | `100::` (dummy) | proxied → Redirect Rule to `customer` |
| `www` | CNAME | `concertable.co.uk` | proxied → Redirect Rule to `customer` |

**dev / staging** repeat the app-host rows with the env label inserted (`customer.dev`, `auth.staging`, …).
No apex/www for non-prod. Same DNS-only / managed-cert story.

---

## How the hostnames flow into config

Two config surfaces key off these hostnames. Both are **per-environment**, so both live in App Config
(`config`), selected by label — *not* baked into an image:

1. **`Auth:Authority`** — the OIDC issuer the SPAs and APIs trust. `= https://auth.<env?>.concertable.co.uk`.
2. **`Cors:AllowedOrigins`** — the SPA origins the API hosts (`b2b-api`/`customer-api`/`search-api`/`payment-api`)
   allow. `= the four SPA origins` (business/venue/artist/customer). Read by B2B/Customer/Search/Payment Web
   via `Configuration.GetSection("Cors:AllowedOrigins")`.
3. **`Auth:SpaClients:*`** — the OIDC **redirect URIs / post-logout / per-client CORS** for the three web
   clients (Customer/Venue/Artist). These are as env-specific as `Auth:Authority`, so they belong in App
   Config next to it — **finalized into the tfvars in this pass** (previously they only existed in Auth's
   `appsettings.json`, which can't express staging vs prod). Auth binds them into `SpaClientSettings`
   (`Auth:SpaClients`), and `Config.WebClients` turns them into IdentityServer clients.

**Build-time SPA config (the `DEPLOYMENT.md` "SPA build config" blocker — ✅ now fixed):** the SPAs
bake `VITE_AUTH_AUTHORITY` + the per-service `VITE_*_API_URL` + `VITE_*_WEB_URL` at build. `app/web/.env.production`
now carries these at the **prod** hosts above — `VITE_CUSTOMER_WEB_URL=https://customer.concertable.co.uk`
(was apex — corrected to the `customer.` decision in flagged call #1) and the four `*-api.` service URLs. The
`vite.config.ts` `define` blocks are `command`-conditional (dev localhost unchanged; `command === 'build'`
sources these). Non-prod (staging/dev) hosts override per-env via CI `VITE_*` (`process.env` wins over the
file). This doc supplied the values; the SPA-build blocker owned the wiring, and both are done.

---

## What changed in this pass (author-only, uncommitted)

- **`config/environments/{dev,staging,prod}.tfvars`** — added the `Auth:SpaClients:*` redirect-URI
  block (Customer/Venue/Artist) per env, matching the scheme. `Auth:Authority` + `Cors:AllowedOrigins` were
  already present and correct for `customer.` — verified, unchanged.
- **`api/Concertable.Auth/.../appsettings.json`** — customer SpaClient `concertable.co.uk` → `customer.concertable.co.uk`
  (RedirectUri/PostLogout/CORS), so the tracked base fallback matches the scheme. (In cloud App Config
  overrides this per env; locally `appsettings.Development.json` overrides it to localhost — so this is the
  no-App-Config fallback only, now scheme-consistent.)
- **`plans/platform/DEPLOYMENT.md`** — the "Custom domains / OIDC" open item now records the resolved scheme + points here.
- **`plans/platform/CONFIG_AND_DEPLOYMENT_PLAN.md`** — Phase 3 "next up" line updated to point here.

## Blocked on / not done (in order)

1. **Buy `concertable.co.uk`** and move nameservers to Cloudflare.
2. **Azure creds** (subscription + OIDC federation) — shared blocker with `config` apply and the
   `DEPLOYMENT.md` infra. Until then no SWA/ACA exists, so no CNAME targets or `asuid` tokens.
3. **`terraform apply` the deployment infra** (`DEPLOYMENT.md` Phase 4) → get the SWA/ACA default hostnames +
   verification IDs → fill the DNS record targets above.
4. **Create the DNS records** (this runbook) → wait for Azure managed-cert issuance per host.
5. **Apply `config`** so Auth/APIs read `Auth:Authority` / `Cors` / `Auth:SpaClients` from App Config.
6. **Smoke test** the OIDC round-trip on real hosts (register/login via `auth.`, an SPA→API call with CORS).
   OIDC/CORS/DNS can't be verified until steps 1–5 exist — that's the whole "blocked" premise; the deploy
   smoke test in `DEPLOYMENT.md` is the verification gate.

## Cross-refs
- Deploy method + first-deploy runbook: [`DEPLOYMENT.md`](DEPLOYMENT.md)
- Config/secrets architecture + phases: [`CONFIG_AND_DEPLOYMENT_PLAN.md`](CONFIG_AND_DEPLOYMENT_PLAN.md) (Phase 3)
- Config-as-code repo: `config` (sibling repo) — `environments/*.tfvars`, README
