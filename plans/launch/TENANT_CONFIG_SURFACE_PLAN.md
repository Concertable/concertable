# Tenant configuration surface

> Spun off `LAUNCH_ROADMAP.md` §5 "Tenant configuration surface" and the §7 launch gate
> "Tenant config surface live".
> **Next steps live in @plans/launch/TENANT_CONFIG_SURFACE_PROGRESS.md → `## Next Steps`** — this
> plan holds the design and outstanding phases only, no next-action prose.

## 1. Outcome

PRS pass-through, VAT rate, payment terms, and cancellation defaults are tenant configuration rather
than constants. Operators edit the values on the existing B2B Organization setup form; the Tenant
aggregate persists optional per-field overrides; the Tenant module resolves every absent override
against platform defaults bound through `IOptions` from B2B appsettings; and settlement, VAT, and
cancellation consume the resolved values.

The same configuration path runs under the local Aspire AppHost and in production. No Azure config
store, deployment resource, or production-only adapter is introduced. A future configuration provider
can replace the appsettings source without changing the Tenant domain model or any consumer.

## 2. Existing precedent and seams

- `TenantEntity.TaxCompliance` is the shipped legal/tax aggregate value. The Organization GET/PUT
  path already maps it through `TenantDetails`, `UpdateTenantRequest`, `TenantMappers`, and the shared
  venue/artist `OrganizationForm`; the music-licence checkbox proves this is the correct surface.
- `TenantEntity` is the legal/VAT/Stripe entity. Tenant configuration therefore belongs beside
  `TaxCompliance`, not in Deal, Concert, Payment, or a parallel configuration subsystem.
- Concert reads tenant tax facts through `ITenantModule`. The same module boundary will expose resolved
  configuration; Concert must not query Tenant persistence directly.
- `UkVatCalculator` currently owns a literal `0.20m`; `RevenueShareSettlementAmount` currently sends
  all gross door revenue into the artist-share calculator; `FinishExecutor` settles immediately when
  the concert ends; and `CancelExecutor` has no tenant timing rule. These are the constants/implicit
  defaults this feature replaces.
- `LEGAL_REQUIREMENTS.md` item 5 requires the PRS pass-through to be venue-owned, suppressed when the
  venue declares that it holds its music licence, and deducted from gross door revenue before the
  artist split. The old flat 3% platform skim must not return.

## 3. Configuration model

### 3.1 Persisted overrides

Add a `TenantConfiguration` value object in the Tenant domain with nullable scalar overrides:

- `decimal? PrsPassThroughRate`
- `decimal? VatRate`
- `int? PaymentTermsDays`
- `int? CancellationNoticeHours`

`TenantEntity.Configuration` is non-null from provisioning onward and starts as
`TenantConfiguration.Empty`. Each scalar remains nullable so fallback is independent per field: a
tenant can override VAT while continuing to inherit the platform PRS/payment/cancellation defaults.
`TenantEntity.UpdateLegalDetails` replaces legal name, tax compliance, and configuration together,
matching the existing all-at-once Organization form transition.

Map `TenantEntity.Configuration` with EF `ComplexProperty`, using nullable columns and explicit decimal
precision. This is a value with no identity or lifecycle of its own, like the existing complex value
objects (`ESignature`, `DateRange`, invoice breakdowns); it is not an owned entity or a new table.

Domain and request validation enforce:

- rates in the inclusive range `0..1`;
- payment terms in `0..365` days;
- cancellation notice in `0..8760` hours.

Null is valid and means "inherit the platform default". Zero is a real override and must not be
coalesced as absent.

### 3.2 Platform defaults and resolution

Add `TenantConfigurationDefaultsOptions`, bound and startup-validated from the
`TenantConfigurationDefaults` section in B2B `appsettings.json`:

| Setting | Launch default | Meaning |
|---|---:|---|
| `PrsPassThroughRate` | `0.042` | Current popular-music default from the legal requirement |
| `VatRate` | `0.20` | UK standard VAT rate |
| `PaymentTermsDays` | `0` | Settle when the performance has ended |
| `CancellationNoticeHours` | `0` | No advance-notice buffer; cancellation closes at the performance start |

A Tenant application resolver merges the persisted overrides with `IOptions` field by field and
returns a non-null `TenantConfigurationValues` contract. `ITenantModule.GetConfigurationAsync` is the
only cross-module read. Unknown tenants fail closed; a provisioned tenant with no overrides returns
the options defaults.

The base `appsettings.json` owns the values, so Aspire and production receive identical defaults
through normal .NET configuration precedence. Environment variables or a later production config
provider can override that section without code changes.

### 3.3 API and Organization form

Reuse the current Organization DTO/mapper/request pipeline:

- `TenantDetails.Configuration` returns the nullable persisted override DTO.
- `TenantDetails.ConfigurationDefaults` returns the current non-null platform defaults so the web
  client never duplicates configuration constants.
- `UpdateTenantRequest.Configuration` accepts the same nullable override shape and maps it to the
  domain value object.
- The shared Organization form adds one "Booking and settlement defaults" section for PRS %, VAT %,
  payment-term days, and cancellation-notice hours. Blank means "use platform default"; each control
  shows the server-supplied platform default. Percentage inputs display human percentages and map to
  decimal rates only at the request boundary.

Both manager SPAs render the existing shared form. No persona branch or second endpoint is added.

## 4. Consumers

### 4.1 PRS pass-through in revenue-share settlement

The venue tenant owns the PRS choice for every contract type. Tenant resolves its effective PRS rate
to zero when `TaxCompliance.HoldsMusicLicence` is true; otherwise it returns the tenant override or
platform default. This keeps the legal attestation and configuration policy in one module.

`RevenueShareSettlementAmount` reads the concert's venue tenant, obtains the effective PRS rate via
`ITenantModule`, rounds the PRS deduction to currency precision using away-from-zero rounding, removes
it from total gross door revenue, and only then invokes `IArtistShareCalculator`. FlatFee and VenueHire
remain unchanged because they do not settle from door revenue. Payment's platform fee is untouched.

### 4.2 VAT

VAT registration remains the presence of the supplier's `TaxCompliance.VatNumber`. When registered,
`TenantService.GetVatCalculationAsync` supplies the tenant's resolved VAT rate to the calculator; when
unregistered, `VatPolicy` still returns no VAT. `UkVatCalculator` performs the same VAT-inclusive
decomposition and rounding but receives the rate instead of owning `0.20m`.

The supplier tenant is still chosen by the existing keyed settlement-payee resolver, so VenueHire's
reversed supply direction remains correct.

### 4.3 Payment terms

The settlement supplier/payee is the existing `ISettlementPayeeResolver` result: artist for FlatFee,
DoorSplit, and Versus; venue for VenueHire. `FinishExecutor` reads that tenant's effective
`PaymentTermsDays` and defers the transition until `concert.Period.End + days`. The hourly completion
sweep naturally retries the still-booked concert. Add an explicit deferred outcome and structured log
path so payment-term deferral is distinguishable from tax/self-billing gates.

The invoice tax point remains the concert end. With the launch default of zero, existing settlement
timing is unchanged unless a tenant opts into a delay.

### 4.4 Cancellation default

The venue tenant owns the booking's default cancellation notice. `CancelExecutor` reads the effective
`CancellationNoticeHours` and rejects cancellation after
`concert.Period.Start - notice`. A zero-hour default preserves the existing full-refund behaviour up
to the performance start while preventing a booked concert being cancelled after it has started.

This is one tenant-wide timing default only. It does not decide who may cancel, vary by contract type,
calculate non-refundable deposits, or introduce partial refunds; those belong to the separate,
solicitor-dependent cancellation/refund policy matrix.

## 5. Explicit constraints and out of scope

- **Platform fee:** Payment-owned platform configuration. No tenant field, B2B calculation, or UI
  control is added for it.
- **Cancellation policy matrix:** still blocked on solicitor input. This feature supplies the mechanism
  and one timing default, not per-party/per-contract refund percentages, deposits, or policy text.
- **Production config/deployment:** no Azure resource or dependency on the config/deployment roadmap
  gate. Standard `IOptions` configuration only.
- **Parallel subsystem:** none. The existing Organization form, Tenant aggregate, DTOs, mapper,
  repository, and `ITenantModule` boundary are extended.
- **Historical snapshot policy:** this launch item supplies tenant defaults consumed at execution
  time. Freezing configurable terms into booking agreements is part of the solicitor-owned policy
  matrix; do not invent that matrix here.
- **Money value-type refactor:** `Refactor/launch_money-value-type` may change
  `ISettlementAmountResolver` and VAT arithmetic. Whichever branch lands second rebases and adapts at
  those seams without weakening this design.

## 6. [x] Phase 1 — Tenant mechanism and Organization surface

Implement the Tenant value object, options binding/validation, resolver, DTO/request/mapper plumbing,
EF complex mapping, Tenant unit/integration coverage, seed/test builders, and the shared Organization
form fields. Re-scaffold all initial migrations.

**Verification gate:**

- `dotnet build api/Concertable.slnx` → 0 errors.
- Tenant unit and integration tests green through the `integration-debug` workflow.
- `./initial-migrations.ps1` run from `api/`.
- All four web builds green (`web-customer`, `web-venue`, `web-artist`, `web-business`).

## 7. [x] Phase 2 — Settlement, VAT, and cancellation consumers

Wire the effective configuration into PRS-adjusted revenue-share settlement, supplier VAT
decomposition, payer payment-term deferral, and venue cancellation timing. Add focused Tenant and
Concert unit tests plus affected Concert integration coverage. Tick the §5 row and §7 gate in
`LAUNCH_ROADMAP.md` in the completing feature commit; do not delete the roadmap.

**Verification gate:**

- `dotnet build api/Concertable.slnx` → 0 errors.
- Affected Tenant and Concert unit/integration tests green through `integration-debug`.
- All four web builds green.
- **Merge-queue E2E tier: full E2E, do not skip.** This changes shared manager UI and user-facing
  Organization setup plus settlement/cancellation runtime flows. Let the merge queue run it; do not
  duplicate it locally.

## 8. Delivery and close-out

- Review the committed branch through the repository code-review workflow and address all clear
  findings before opening the PR.
- Open a plain GitHub PR (personal repo; no Azure DevOps item, `AB#`, or assignee).
- Merge through `/merge` with the full E2E tier.
- Own the post-merge `chore/platform-sync-*` PR to green because this changes `api/**`.
- Keep this plan and its ledger until review, PR, merge, package publication, and platform sync are
  terminal. Record the final gate, then delete both together in the following doc-only close-out
  change; never delete `LAUNCH_ROADMAP.md`.
