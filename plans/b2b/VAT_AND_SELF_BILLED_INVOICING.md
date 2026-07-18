# VAT calculation + self-billed invoicing

> **Branch:** `Feature/VatAndSelfBilledInvoicing`
> **Closes:** [`LAUNCH_PLAN.md`](./LAUNCH_PLAN.md) blockers — per-contract VAT calculation (line 26) + self-billed
> VAT invoice engine (line 23); §5 rows; §7 "VAT calculated per contract type + self-billed invoice generated
> per settlement".
> **Spec:** [`LEGAL_REQUIREMENTS.md`](../../api/Concertable.B2B/src/Modules/Deal/LEGAL_REQUIREMENTS.md) items **1, 3, 4**
> (item 0 = VAT posture; item 3 VAT capture already **shipped** with DAC7).
>
> **START HERE AFTER CLEAR → Phase 1** (Tenant Tax module: VAT computation + facade + validation-message cleanup).
> The VAT computation design below is **locked** (decided over a long design discussion). The only unresolved
> product calls are the two in "Open items" — neither blocks writing Phase 1 code.

## What this builds

Three things, all **inside the B2B service** (Tenant + Concert modules; no service boundary crossed):

1. **VAT computation** — given a settlement gross + the supplier's VAT-registration status, produce net/VAT/rate.
   Branches on **supply direction** (which party is supplier per contract type) and the supplier's **VAT-registered
   status** (`TenantEntity.TaxCompliance.VatNumber`). Lives in the **Tenant module's Tax area** (all tax logic
   co-located); Concert consumes it via `ITenantModule`.
2. **A small tax-area cleanup** — the user-facing VAT-number validation message currently hangs off the domain
   `ITaxComplianceRules` interface; move it to the validation layer where it belongs.
3. **Self-billed VAT invoice per settlement** — an immutable invoice per settled booking, gap-free per-supplier
   sequential numbering, HMRC self-billing legends, PDF reusing the existing contract-PDF plumbing.

## VAT posture (item 0 — read first)

Concertable is a **disclosed agent**, not a principal. For the venue↔artist leg we self-bill on the **supplier's**
behalf and the invoice shows **the supplier's** VAT (charged iff that supplier is VAT-registered). Concertable's
*own* VAT (on its platform fee) is **out of scope** — see "Out of scope".

Not everyone is VAT-registered: UK registration is only mandatory above ~£90k turnover, so most grassroots artists
are **not** registered and legally charge **no VAT**; venues (bar turnover) usually are. Hence the branch on
registration is the core requirement, not an edge case.

The four contract types and their supply direction (LEGAL_REQUIREMENTS item 1):

| Contract | Supplier (issues invoice) | Customer | VATable iff… |
|---|---|---|---|
| FlatFee | **artist** | venue | artist VAT-registered |
| DoorSplit | **artist** | venue | artist VAT-registered |
| Versus | **artist** | venue | artist VAT-registered |
| VenueHire | **venue** ← *flip* | artist | venue VAT-registered |

**We do not invent a new per-type map.** Direction is already encoded in two shipped resolvers
(`api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Application/Resolvers/`):

- **Supplier** = `SettlementPayeeResolver.ResolveTenantId(concert)` — the party who *receives* settlement supplies the service.
- **Customer** = `TicketPayeeResolver.ResolveTenantId(concert)` — the exact inverse.

## ⚠️ Design decision — amounts are VAT-**inclusive**; we decompose, never inflate — needs accountant sign-off

The agreed figure (`FlatFeeDeal.Fee`, `VenueHireDeal.HireFee`, the DoorSplit/Versus artist share) is the **gross
(VAT-inclusive)** consideration. We decompose it — `net = round(gross/(1+rate), 2)`, `vat = gross − net` (so
`net + vat == gross` exactly). Unregistered supplier ⇒ `vat = 0`, `net = gross`.

**Why inclusive, not add-20%-on-top:** LEGAL_REQUIREMENTS item 1 warns "a blanket 'add 20%' rule is wrong." Adding
VAT on top would change money that already moved through Stripe Connect and re-open settled amounts; decomposing an
inclusive figure leaves the money path **untouched** — the invoice is a faithful record of a supply that already
settled. Item 0 already requires **accountant sign-off** before launch. If the intended semantics are VAT-*exclusive*,
the decomposition and the Phase-2 mint change — flag before Phase 1.

Rounding: **2dp, `MidpointRounding.AwayFromZero`** on `net`, `vat` as the remainder.

## The VAT computation design (LOCKED)

Three layers, cleanly separated. All in the **Tenant module** (it owns tax); Concert never sees a rate, a policy, or
a calculator — it asks the facade.

**Result — the three figures the invoice records** (`Concertable.B2B.Tenant.Contracts`):
```csharp
public sealed record VatCalculation(decimal Net, decimal Vat, decimal Rate)
{
    public static VatCalculation None(decimal gross) => new(gross, 0m, 0m);   // supplier not VAT-registered
}
```

**Layer 1 — pure region arithmetic** (`Tenant.Application.Tax`, internal). The interface exists because the region
swaps the impl at startup (real polymorphism, not mocking). Rate is a **constant** — it *is* the UK impl (it's always
20%; a rate change is a deliberate code edit that also needs tax-point transition logic, never a config flip):
```csharp
internal interface IVatCalculator
{
    decimal Rate { get; }
    decimal Calculate(decimal gross);            // VAT-inclusive gross -> the VAT portion. pure. no policy.
}

internal sealed class UkVatCalculator : IVatCalculator
{
    public decimal Rate => 0.20m;                // UK standard rate
    public decimal Calculate(decimal gross)
        => gross - Math.Round(gross / (1 + Rate), 2, MidpointRounding.AwayFromZero);
}
```

**Layer 2 — the registration policy** (`Tenant.Application.Tax`, internal). The "does VAT apply" decision lives here,
**once**; generic (holds the region calculator). This is the only place the registered→calculate/else-none rule exists:
```csharp
internal interface IVatPolicy
{
    VatCalculation Apply(decimal gross, string? supplierVatNumber);
}

internal sealed class VatPolicy(IVatCalculator calculator) : IVatPolicy
{
    public VatCalculation Apply(decimal gross, string? supplierVatNumber)
    {
        if (string.IsNullOrWhiteSpace(supplierVatNumber)) return VatCalculation.None(gross);
        var vat = calculator.Calculate(gross);
        return new VatCalculation(gross - vat, vat, calculator.Rate);
    }
}
```

**Facade — the only surface Concert sees** (`ITenantModule`, `Tenant.Contracts`). Two additions:
```csharp
// the supplier/customer tax-compliance record for the invoice snapshot
Task<TaxComplianceDto?> GetTaxComplianceAsync(Guid tenantId, CancellationToken ct = default);
// the computed VAT figures — reads the tenant's registration internally, applies the policy
Task<VatCalculation> GetVatCalculationAsync(Guid tenantId, decimal gross, CancellationToken ct = default);
```
**AS BUILT — reuse, don't duplicate.** A `TaxComplianceDto` already existed (`Tenant.Application.DTOs`, the org-setup
shape). We **did not** add a second one: the existing record was **promoted to `Tenant.Contracts`** (its shape unchanged —
`VatNumber` / `SellerIdentifier` / `RegisteredAddressDto` / `BankReference`) and is reused by the facade, the org read/write,
and Phase 2's snapshot. `GetTaxComplianceAsync` returns `tenant.TaxCompliance?.ToDto()`. It carries **no `LegalName`** —
Phase 2 snapshots that from `ITenantModule.GetByIdAsync → TenantDto.LegalName`. (SellerIdentifier riding along is a bonus:
Phase 2's invoice numbering needs it.) No new `AddressDto` — `RegisteredAddressDto` moved to Contracts alongside.
`TenantService.GetVatCalculationAsync` reads the tenant and applies the policy — **don't default-away a missing
tenant/compliance** (the settlement tax-gate guarantees compliance is present by invoice time; a null VatNumber is the
*only* legitimate absence = unregistered):
```csharp
var tenant = await repository.GetByIdAsync(tenantId, ct) ?? throw new NotFoundException(...);
var compliance = tenant.TaxCompliance ?? throw new InvalidOperationException("settlement gate should guarantee this");
return vatPolicy.Apply(gross, compliance.VatNumber);   // VatNumber null => unregistered => None
```
DI (`Tenant.Infrastructure` `ServiceCollectionExtensions`): `AddSingleton<IVatCalculator, UkVatCalculator>()`,
`AddSingleton<IVatPolicy, VatPolicy>()`.

**Naming rationale (so it isn't re-litigated):** the *new* types are VAT-specific → `Vat*`. The *existing*
`ITaxComplianceRules` / `UkTaxComplianceRules` / `UkTaxComplianceOptions` also carry **DAC7 reporting** fields
(`ReportingAuthority`, `IsoCountryCode`, `ReportableFromMinorUnits`) — broader than VAT — so they honestly **keep**
`TaxCompliance` (NOT renamed to `UkVat*`, which would drop the DAC7 truth). Generic-vs-concrete is honoured by
`IVatCalculator` (generic, region-swappable) vs `UkVatCalculator` (concrete, honest 20%).

## The validation-message cleanup (LOCKED — validator, not frontend)

Today `ITaxComplianceRules.DescribeVatNumberRequirement()` returns a full UI sentence ("VAT number must be 9 or 12
digits…") — presentation copy hanging off a domain rules interface. Fix:
- `ITaxComplianceRules` → **pure** `bool IsValidVatNumber(string)`; delete `DescribeVatNumberRequirement()`.
- The reference data (`VatLabel`, `VatNumberFormatHint`) stays in `UkTaxComplianceOptions` (legit backend data).
- The sentence moves to the FluentValidation `TenantValidators` (Application/Validators), composed from that data:
  ```csharp
  RuleFor(x => x.TaxCompliance.VatNumber)
      .Must(v => string.IsNullOrWhiteSpace(v) || rules.IsValidVatNumber(v))
      .WithMessage(_ => $"{opts.VatLabel} must be {opts.VatNumberFormatHint}.");
  ```
- Verify the validator runs in the write pipeline, then **remove** the now-duplicate check+throw in
  `TenantService.UpdateAsync` (was `if (!IsValid) throw new BadRequestException(DescribeVatNumberRequirement())`).
- Ideal end-state (out of scope): backend returns a structured failure code and the **frontend** owns the copy (the
  doc comment on `ITaxComplianceRules` already says "display copy is owned by the frontend"). Bigger; defer.

## Key seams (verified in code — don't re-derive)

- **PDF:** `IPdfService.Render(IDocument) → byte[]` (`Concertable.Shared.Pdf`, reuse unchanged). Mirror
  `ContractDocument : IDocument` (`Concert.Infrastructure/Pdf/ContractDocument.cs`). **Naming note:** code uses
  **`Contract`/`contracts/`**, not the aspirational `BookingAgreement`/`agreements/`.
- **Blob:** `IBlobStorageService` (`Concertable.Shared.Blob`) — `UploadAsync`(overwrite)/`DownloadAsync`/`ExistsAsync`.
- **PDF lifecycle to mirror:** `ContractPdfService` — `GetOrCreateAsync` (lazy render-on-download) +
  `GenerateForBookingAsync` (background) with a **static `SemaphoreSlim`** (QuestPDF `GeneratePdf` not thread-safe).
  Background render via `IBackgroundTaskRunner.RunAsync<T>` — not an outbox. `PdfBlobName` pre-minted in the issuing
  DB transaction; render only uploads, never writes the DB.
- **Entity to mirror:** `ContractEntity` — immutable private-set snapshot, `IVenueArtistTenantScoped`
  (`VenueTenantId`+`ArtistTenantId`), `PdfBlobName` pre-assigned in `Create`. Issued by `ContractIssuer.IssueAsync`.
- **Download pattern:** `ApplicationController` `GET /api/Application/{id}/contract[/pdf]` — **no `[HasPermission]`**;
  two-party tenant-scoped repo (`VenueArtistTenantScopedRepository`) returns 404 to a stranger (not a probe-able 403).
  HATEOAS link in `ApplicationResponseMapper`/`ConcertResponseMappers`, gated on the id.
- **Tenant VAT source:** `TenantEntity.TaxCompliance` (owned VO) = `VatNumber?` (null ⇒ not registered — presence
  *is* the status), `SellerIdentifier`, `RegisteredAddress` (Line1/Line2?/City/Postcode/Country), `BankReference`.
- **Cross-module read:** `ITenantModule` (`Tenant.Contracts`) today exposes only `IsTaxComplianceCompleteAsync → bool`.
  `FinishExecutor` already depends on it and calls it by payee TenantId — the exact seam Phase 1 extends.
- **Settlement trigger:** hourly timer → `ConcertCompletionRunner` → `FinishExecutor` (fail-closed tax-gate, can
  defer) → `workflow.Finish`. `FinishExecutor` is the **single common seam across all four types**.
- **Settlement gross today:** FlatFee `deal.Fee` / VenueHire `deal.HireFee` (escrow); DoorSplit/Versus
  `artistShareCalculator.Calculate(deal, totalRevenue)` in `PayoutFinishStep`, `totalRevenue = TicketsSold*Price +
  DoorRevenue` (`ConcertRepository.GetTotalRevenueByConcertIdAsync`).
- **Money type:** raw `decimal` (GBP); minor-units `(long)(x*100)` only at the Payment boundary. **No `Money` VO, no
  rounding rule** — this plan defines the VAT rounding rule above.
- **Numbering:** **nothing exists** (contract "reference" is just DB identity `C-{Id}`). Gap-free per-supplier
  invoice numbering is a new capability (Phase 2).
- **Platform fee:** **does not exist** — no commission/`ApplicationFeeAmount` anywhere; 100% of the deal flows
  party-to-party. (Confirms platform-fee VAT is separate, fee-blocked work — out of scope.)

---

## Phase 1 — Tenant Tax module: VAT computation + facade + message cleanup (items 1 & 3) — ✅ SHIPPED

Zero money-path behaviour change. Unblocks Phase 2. What landed:
1. **VAT computation** (`Tenant.Application.Tax`): `IVatCalculator`/`UkVatCalculator` + `IVatPolicy`/`VatPolicy`;
   `VatCalculation` in `Tenant.Contracts`; DI in `Tenant.Infrastructure`.
2. **Facade:** `GetTaxComplianceAsync` + `GetVatCalculationAsync` on `ITenantModule`/`TenantService`. **Reused the existing
   `TaxComplianceDto`** (promoted to Contracts) instead of adding a second one — see "AS BUILT" above.
3. **Validation cleanup:** `ITaxComplianceRules` → pure `IsValidVatNumber`; message moved to `UpdateTenantRequestValidator`;
   duplicate `TenantService.UpdateAsync` throw removed. **Latent bug found + fixed:** the internal Tenant validators were
   never registered (`AddValidatorsFromAssemblyContaining` lacked `includeInternalTypes: true`), so the VAT-format rule
   wasn't enforced *at all* — added the flag (mirrors Concert), proven by `Update_InvalidVatNumberFormat_ReturnsBadRequest`.

**Gate met:** `dotnet build api/Concertable.slnx` green; 94 Tenant unit tests + the Tenant integration suite green.
`TenantServiceTests` covers calculator/policy/facade/completeness; `TenantValidatorsTests` the message. No E2E.

## Phase 2 — Concert: invoice entity + gap-free numbering + mint at settlement (item 4, part 1)

**Tasks**
1. **Single source of truth for the gross.** `ISettlementAmountResolver` (Concert, keyed by `DealType`):
   `Task<decimal> ResolveGrossAsync(ConcertEntity concert, CancellationToken ct)` — FlatFee→`deal.Fee`,
   VenueHire→`deal.HireFee`, DoorSplit/Versus→`ArtistShareCalculator` over `GetTotalRevenueByConcertIdAsync`.
   **Refactor `PayoutFinishStep` to consume it** so charged and invoiced amounts can't diverge (existing DoorSplit/
   Versus settlement tests are the safety net — amounts must stay identical).
2. **`InvoiceEntity`** (Concert.Domain, `IVenueArtistTenantScoped`, immutable private-set snapshot mirroring
   `ContractEntity`): booking id; `SupplierTenantId`/`CustomerTenantId`; both parties' snapshotted `LegalName`,
   `VatNumber?`, `RegisteredAddress`; `Net`/`Vat`/`Gross`/`VatRate`; `SequenceNumber` + formatted `InvoiceNumber`;
   `TaxPointUtc` (= performance/finish date); `DealType`; `PdfBlobName` pre-minted `invoices/{bookingId}-{guid:N}.pdf`;
   `CreatedAtUtc`. EF owned-type config mirroring `ContractEntityConfiguration`; `./initial-migrations.ps1`.
3. **Gap-free per-supplier numbering** (new capability). `InvoiceSequenceEntity(Guid TenantId PK, long NextNumber)`.
   Allocate **inside the mint transaction** via an atomic increment (`UPDATE … SET NextNumber = NextNumber + 1`
   read-back, or locked-read + rowversion retry); lazily insert the row on a supplier's first invoice. Gap-free holds
   because allocation + insert share one transaction. Format e.g. `INV-{supplier SellerIdentifier}-{n:D6}`.
4. **`IInvoiceIssuer.IssueAsync`** — called from `FinishExecutor` **after** the successful Finish transition (after
   the tax-gate passes; deferred settlements mint nothing):
   - `gross = ISettlementAmountResolver.ResolveGrossAsync(concert)`
   - `supplierTenantId = SettlementPayeeResolver.ResolveTenantId(concert)`; customer = `TicketPayeeResolver`
   - `supplierTax = GetTaxComplianceAsync(supplierTenantId)`, `customerTax = GetTaxComplianceAsync(customerTenantId)` — for the snapshot (VAT/seller id/address/bank ref); each party's **`LegalName` from `GetByIdAsync` → `TenantDto.LegalName`** (the tax DTO carries no legal name)
   - `vat = GetVatCalculationAsync(supplierTenantId, gross)` — the figures (Net/Vat/Rate); invoice `Gross = gross`
   - allocate the sequence number; persist `InvoiceEntity`, all in the finish transaction.
   - Tax point = supply/performance date (not payment-settlement date — sound for the async DoorSplit/Versus pay path).
5. **Read surface:** `InvoiceDto` + service read + `GET /api/Concert/{id}/invoice` (DTO), two-party scoped.

**Gate:** build green; Concert integration tests via `integration-debug`: invoice minted on settlement for **all four
types** with correct direction/amounts/VAT (registered + unregistered supplier); numbering gap-free, per-supplier,
safe under concurrent settlements for the same supplier; nothing minted when the tax gate defers.
`./initial-migrations.ps1`.

## Phase 3 — Concert: invoice PDF + download + HATEOAS (item 4, part 2) — final phase

**Tasks**
1. **`InvoiceDocument : IDocument`** (Concert.Infrastructure/Pdf) mirroring `ContractDocument`: header (invoice number,
   tax point); both parties' legal name + address + VAT number; line item(s); net / VAT-rate / VAT / gross; and the
   **HMRC self-billing legends** ("The customer shall issue the invoice" / "SELF-BILLING" + both VAT numbers).
2. **`IInvoicePdfService`** mirroring `ContractPdfService`: `GetOrCreateAsync` (lazy) + `GenerateForBookingAsync`
   (background), same `IBlobStorageService`, same static render-lock, `invoices/` prefix; background render via
   `IBackgroundTaskRunner.RunAsync<IInvoicePdfService>`; lazy fallback covers a blob outage.
3. **Download endpoint** `GET /api/Concert/{id}/invoice/pdf` (and/or `/api/Application/{id}/invoice/pdf`) — **no
   `[HasPermission]`**, two-party scoped → 404 to strangers; returns `File(pdf, "application/pdf", name)`.
4. **HATEOAS `invoice` link** in `ConcertResponseMappers` (and `ApplicationResponseMapper`), gated on the invoice id.

**Gate:** build green; integration tests: download returns a PDF to **both** parties, 404 to a stranger; lazy render
regenerates a missing blob; HATEOAS link present only when the invoice exists. User-facing flow — run UI E2E via
`e2e-ui-debug` **if** the invoice surfaces in an E2E-covered SPA flow; else integration coverage is the gate (per
[`../CLAUDE.md`](../CLAUDE.md) "When to run the E2E suites").

**On the commit that completes Phase 3:** tick `LAUNCH_PLAN.md` line 26 (VAT calculation ✅), line 23 (self-billed
invoice — see caveat), §5 rows 136–137, §7 line 185; `git rm` this plan file (same commit).

---

## Out of scope (don't scope-creep)

- **Platform-fee VAT (Concertable's own output VAT).** A separate invoice (Concertable → each party), blocked on the
  platform fee itself, which **does not exist in code**. Separate LAUNCH_PLAN work (pricing UI / tenant-config). This
  plan is the **party-to-party supply** VAT only.
- **Self-billing *agreement* + annual renewal.** HMRC self-billing needs a signed agreement per supplier, renewed
  every 12 months. LAUNCH_PLAN line 23 bundles it in; the build list scopes it out. Mostly a **terms clause + a
  per-supplier consent record with a 12-month expiry** — so do **not** fully close line 23 on Phase 3; mark it
  "invoices shipped; self-billing agreement + renewal outstanding."
- **Frontend-owned validation copy + structured error contract** — the ideal end-state for the message cleanup; the
  validator move is the in-scope fix.
- **PRS pass-through** (item 5) and the **tenant configuration surface** — separate blockers.

## Open items (product calls — neither blocks writing Phase 1)

1. **VAT-inclusive vs -exclusive** — accountant sign-off (item 0). Everything downstream assumes inclusive/decompose.
2. **Self-billing agreement** — consent record + annual renewal in scope for launch, or fast-follow? (Affects whether
   line 23 fully closes.)
