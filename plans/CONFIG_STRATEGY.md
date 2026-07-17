# Region & deployment configuration — strategy (WORKING / in discussion)

**Status:** first application SHIPPED (tax form labels moved off the B2B org read); the rest is design in
progress. Remaining work = the `concertable-config` / App Config graduation + the open decisions below.

## What triggered this
B2B `/organizations` welds region-constant tax **display copy** (`taxFormLabels`) onto the per-tenant
`TenantDetails`, re-serialized every GET/PUT. It's presentation, divided by tenant, that actually varies by
region. Generalised: deployment/region config keeps creeping onto per-tenant reads and into scattered
literals. We want ONE systematic way to handle deployment/region config.

## Scope: UK-only today; region-scalable by construction, not by deployment
There is **one** deployment today — UK. No other region is provisioned, deployed, or in-flight, and we
are **not** building multi-region machinery now. What we build now is the *seam*: region resolved once at
startup as a deployment identity, so a future in-country deployment (France, etc.) becomes a
provisioning/config act, not a code change. "Deployment-per-region" is how we'd scale — France is
illustrative, not being stood up. The bar for the NOW work is "the UK seam is clean and stampable
later", not "multi-region works".

## Status of each piece — NOT all agreed

Nothing here is locked until Tommy says so. One item was agreed this round (native IaC, not Python); the rest
is either something Tommy originated or something proposed-but-not-ratified. Flagged accordingly.

### Tommy originated / confirmed
- **Deployment-per-region.** Tommy reasoned to it (France = its own in-country deployment), so region is a
  provisioning fact resolved once at startup, not per-request/per-tenant state. → keep the startup-fixed
  `ITaxComplianceRules` selection; do NOT re-add the per-tenant `Jurisdiction` column (B4 deletion stands);
  build-time `VITE_REGION` on the FE is fine.
- **✅ SHIPPED — Backend stops shipping display copy; FE owns region copy** (Tommy's day-1 diagnosis). Removed
  `ITaxComplianceRules.GetFieldLabels()`, the `TaxFormLabels` DTO, and `taxFormLabels` from
  `TenantDetails`/`Organization`; region *behaviour* (`IsValidVatNumber`, `DescribeVatNumberRequirement`) stays
  server-side; `taxCompliance` stays flat; UK copy is inlined in
  `app/web/b2b/shared/.../organizations/taxFormLabels.ts` (no i18n framework, no `VITE_REGION` yet — that
  arrives with the config graduation).
- **Config gets its own boundary** — Tommy proposed `concertable-config`.
- **Native IaC — Terraform, not Python, not Bicep** *(agreed this round)*. Terraform (provider-agnostic — one
  IaC language across all infra, no Azure-only Bicep lock-in) declares the App Config store
  (`azurerm_app_configuration`) + its key-values (`azurerm_app_configuration_key`), and/or `az appconfig kv
  import` of checked-in JSON. Config files in appsettings-JSON shape so they double as the local/dev fallback.
  No homegrown generator/schema. CRIS's ~700-line Python build/diff/deploy pipeline is explicitly rejected (its
  *principle* — Git → App Config, region as top partition, PR diff, snapshot rollback — is fine; its mechanics
  are not).

### Proposed — NOT yet ratified (Tommy's call)
- **Config is a store fed by IaC, never a runtime *service*.** Argued: a `Concertable.Config` service called at
  boot = universal `WaitFor(config)` = the coupling anti-pattern `api/ARCHITECTURE.md` forbids. Tommy leaned
  toward IaC / "recreate from code", which fits — but hasn't ruled out a service outright.
- **In-code seam: typed, purpose-named accessors; never inject `IConfiguration`/`IOptions` into business code;
  `IOptions`→`IOptionsMonitor` for refresh.** This is what makes appsettings→App Config a provider swap with no
  consumer churn. Fits the "no generic AppConfigService" constraint, but the mechanics aren't ratified.
- **Region config never crosses the BE/FE wire.** Derived from the two Tommy-confirmed points above.

## Latent UK config to formalise (raw material for the region profile)
- **Currency** — `GBP`/`gbp`/`£`/pence(÷100)/`en-GB` scattered across Payment Stripe clients,
  `app/shared/lib/currency.ts`, KPI/revenue widgets. The obvious 2nd region-config member: proves the pattern
  isn't tax-specific and deletes real hardcoding.
- **Default country** — `"United Kingdom"` hardcoded in `OrganizationPage.tsx`.
- **Tax** — already modelled (`UkTaxComplianceOptions`).
- Likely also locale / date-format / timezone — audit.

## Boundary rule
**Payment is an agnostic adapter** — currency must be *passed to* Payment by the caller (B2B/Customer) from the
caller's region config, never become Payment's own config (the `ICurrentPayoutOwner` precedent). The
hardcoded `"GBP"` in Payment's Stripe clients is a boundary + config cleanup.

## Region config vs per-tenant data — orthogonal (why the seam is clean)
Is any UK config *also* genuinely per-tenant? By design, no:
- **Per-tenant** = tenant *data* (VAT number, registered address, tax residence, TIN, Stripe account,
  bank details) or *commercial terms* (deal splits, fees, payout timing) — none region-flavoured.
- **Region** = tax labels, VAT format/validation rules, VAT rate/threshold, currency, default country,
  locale/date/timezone, DAC7 reporting authority — **constant across every UK tenant**.

The two axes never overlap, which is exactly why welding `taxFormLabels` onto per-tenant `TenantDetails`
was wrong: nothing region-specific varies per tenant, so nothing region-specific belongs on a per-tenant
read. **Gray zone, out of scope for UK-now:** a cross-border seller (EU-resident tenant under the UK
deployment) carries a non-GB VAT number the startup-fixed UK `IsValidVatNumber` would reject — but that
is tenant *data* driving residence-based validation, not a per-tenant region *profile*. Flag it, don't
design for it yet.

## Open decisions (NOT yet made)
- **Store topology:** one App Config per region shared by all services (per-service key-namespacing; region
  reference data lives once) vs one per service per region. Lean: **shared-per-region** — App Config is shared
  infra like SQL/ASB; the code boundary stays in each service's binding.
- **Config repo scope/shape:** one small `concertable-config` repo partitioned **region × environment**. Do
  NOT port CRIS's tenant/reverse-proxy surface. Env naming: dev/staging/prod — **never `ci`**, and don't mash
  region+env into one slug.
- **Naming:** first-class `Region` as the deployment identity (currency/locale/tax hang off it) vs keep
  `Jurisdiction` (tax-specific). Lean: **`Region`** umbrella.
- **First-cut scope:** tax-labels only, or tax-labels + currency.
- **Sequencing:** the App Config repo is a LATER graduation; the in-code seam + tax-label move (+ maybe
  currency) is the NOW work and does NOT depend on App Config existing (appsettings-backed today).

## Reconciliation
`plans/b2b/TAX_COMPLIANCE_REFACTOR.md` was fully shipped (Parts A, B1, B2, B4; B2's `TaxComplete` bool was
superseded by presence-is-completeness; B3/B5 labels landed with this strategy's first application) and is
**removed**. The earlier "group `taxCompliance` + labels into a nested `tax` container" idea is superseded —
pulling labels off leaves one tax field, nothing to group.
