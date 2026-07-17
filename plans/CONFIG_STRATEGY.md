# Region config — keep it consolidated, don't build the machinery (WORKING)

**Status:** first application SHIPPED (tax form labels moved off the B2B org read, on
`Feature/Dac7Onboarding`). Remaining = consolidate the other latent UK hardcoding (currency, default
country). No region *machinery* is planned.

> **App-wide config/secrets/deployment** (App Configuration, Key Vault, the `config` repo,
> Azure deployment) is a *separate, bigger* workstream — see
> [`CONFIG_AND_DEPLOYMENT.md`](./CONFIG_AND_DEPLOYMENT.md). This doc stays scoped to *region* variance.

## Position: UK-only today; "scalable" = consolidated, not abstracted
There is one deployment: UK. We are **not** building region machinery — no Azure App Config, no
`config` repo, no Terraform config pipeline, no `VITE_REGION`, no region profiles. All of
that is speculative for a single region; add it the day a real second region (e.g. France) exists, not
before.

"Scalable" is satisfied by **consolidation**: each region-varying value lives in exactly one place, so a
second region becomes an *additive* change (a second file + a switch), not a scattered rewrite. That's
the whole requirement for now.

If a 2nd region ever becomes real, these earlier conclusions are worth revisiting (recorded so we don't
re-derive them, explicitly NOT now): region resolved once at startup as a deployment identity (not
per-tenant); config as a store fed by IaC, never a runtime service; native Terraform over a bespoke
Python pipeline.

## Region config vs per-tenant data — orthogonal (why the seam is clean)
Is any UK config *also* genuinely per-tenant? No, by design:
- **Per-tenant** = tenant *data* (VAT number, registered address, tax residence, TIN, Stripe account,
  bank details) or *commercial terms* (deal splits, fees, payout timing) — none region-flavoured.
- **Region** = tax labels, VAT rules/rate/threshold, currency, default country, locale/date/timezone —
  **constant across every UK tenant**.

The axes never overlap — which is exactly why welding `taxFormLabels` onto per-tenant `TenantDetails`
was wrong. Gray zone (out of scope for UK-now): a cross-border seller (EU-resident tenant under the UK
deployment) carries a non-GB VAT number the startup-fixed UK validator would reject — but that's tenant
*data* driving residence-based validation, not a per-tenant region *profile*.

## Consolidated so far
- **Tax form labels** → `app/web/b2b/shared/.../organizations/taxFormLabels.ts` (FE owns region copy;
  backend serves region *behaviour*, not strings).
- **VAT rules** → `UkTaxComplianceRules` behind `ITaxComplianceRules`, selected once at startup.

## Still scattered — the real remaining cleanup (no abstraction, just consolidate)
- **Currency** — `GBP`/`gbp`/`£`/pence(÷100)/`en-GB` across Payment Stripe clients,
  `app/shared/lib/currency.ts`, KPI/revenue widgets.
- **Default country** — `"United Kingdom"` hardcoded in `OrganizationPage.tsx`.
- Likely also locale / date-format / timezone — audit.

**Boundary rule for currency:** Payment is an agnostic adapter — currency is *passed to* Payment by the
caller (B2B/Customer), never Payment's own config (the `ICurrentPayoutOwner` precedent). The hardcoded
`"GBP"` in Payment's Stripe clients is a boundary + consolidation fix.
