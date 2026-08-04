# Self-billing agreement + 12-month renewal consent

> **Active launch plan.** Per-settlement self-billed invoice *generation* has shipped
> (`Feature/VatAndSelfBilledInvoicing`: immutable `InvoiceEntity`, gap-free per-supplier numbering,
> VAT-status branching, HMRC self-billing legends, PDF via `IPdfRenderer`). The invoice document already
> prints *"raised by the customer on behalf of the supplier under a self-billing agreement"* — but no such
> agreement record exists yet, so today that legend asserts a document Concertable does not hold. HMRC
> self-billing rules require the customer (Concertable, issuing on the supplier's behalf) to hold a
> self-billing **agreement the supplier has accepted, reviewed/renewed at least every 12 months**, and
> permit self-billed invoices **only while a current agreement is in force**.
>
> This plan closes that gap: a per-supplier self-billing agreement with a 12-month review/expiry and a
> renewal path, self-billed invoice issuance made **fail-closed** on a current agreement, and a
> supplier-facing grant/renew surface in the manager SPAs. It reuses the shipped agreement/e-sign
> machinery (advanced-tier self-hosted e-signature, immutable snapshot, lazy PDF render, HATEOAS gating)
> — it does not reinvent it.
>
> **Next steps live in @plans/b2b/SELF_BILLING_AGREEMENT_PROGRESS.md → `## Next Steps`.**

Ledger: [`SELF_BILLING_AGREEMENT_PROGRESS.md`](./SELF_BILLING_AGREEMENT_PROGRESS.md). Read
[`../agents/PLAN.md`](../agents/PLAN.md) before working a phase.

---

## 1. Scope

**In scope**

- A **per-supplier self-billing agreement**: the supplier (the tenant that is the settlement payee /
  supplier for that deal — direction per [`LEGAL_REQUIREMENTS.md`](../../api/Concertable.B2B/src/Modules/Deal/LEGAL_REQUIREMENTS.md)
  item 1) accepts a self-billing clause, recorded **immutably** with an accepted-at timestamp and a
  12-month review/expiry, captured with the advanced-tier self-hosted e-signature.
- A **renewal consent path**: re-acceptance produces a new immutable record and a fresh 12-month window,
  available before *and* after expiry.
- **Fail-closed issuance**: no current agreement for the supplier → **no self-billed invoice** is minted
  for that supplier's settlement. The settlement defers with a surfaced reason and self-heals when the
  supplier grants/renews consent, exactly as the existing tax-compliance gate does.
- A **supplier-facing surface** in the manager SPAs to grant and renew consent, HATEOAS-gated, reachable
  by both tenant types (artist is the supplier for FlatFee/DoorSplit/Versus; venue for VenueHire).

**Out of scope** (all shipped or deliberately deferred)

- Invoice generation, VAT calculation, per-contract VAT, gap-free numbering, HMRC legends — shipped.
- Non-UK jurisdictions (UK-only launch; the jurisdiction seam already exists).
- Any third-party or per-signature e-signature cost — the self-hosted advanced tier is reused.
- Forcing re-acceptance on a self-billing **clause** version change — the `PlatformTermsVersion` snapshot
  is captured (§4) so this is a later additive tightening, not core-scope work.

## 2. Legal shape (the requirement being encoded)

HMRC self-billing: the **customer** (the party receiving the supply and raising the invoice) holds an
agreement with the **supplier** (the party making the supply, in whose name the invoice is raised).
Concertable is a **disclosed agent** (item 0) that raises the *supplier's* invoice on the supplier's
behalf, so the agreement is **Concertable ↔ supplier**, and one current agreement per supplier tenant
authorises every self-billed invoice raised in that tenant's name.

The supplier per deal type (item 1; already encoded by `SettlementPayeeResolver`):

| Deal type | Supplier (must hold the agreement) |
|---|---|
| FlatFee / DoorSplit / Versus | **artist** |
| VenueHire | **venue** |

So for any concert, the tenant returned by `SettlementPayeeResolver.ResolveTenantId(concert)` is the
supplier, and **that** tenant must hold a current agreement before that concert's invoice may issue.
The plan adds no new deal-type branching: supplier direction is read from the existing resolver.

Required attributes of the agreement (HMRC): identity of both parties, the supplier's acceptance, an
issued/accepted date, a **not-more-than-12-month** review period, and that self-billed invoices are only
raised while it is in force.

## 3. Ownership decision — the Concert module owns the agreement

The agreement is owned by the **Concert module**, beside `InvoiceEntity` / `ContractEntity` and the
`FinishExecutor` it gates. Rationale:

- **Maximum reuse, zero seam extraction.** Every piece to mirror — the `ESignature` value object
  (advanced-tier typed name + optional drawn signature, server-owned attribution), `IPdfBlobCache`
  (lazy render-on-download), `IPdfRenderer`, the `IDocument` document class shape, `DisplayNames`, the
  `ActionLink` HATEOAS pattern, the immutable-snapshot `Create(...)` factory — already lives in Concert.
  Concert ownership consumes them in place; no value object moves across a module boundary.
- **Local enforcement.** The fail-closed gate is called from *inside* `FinishExecutor`; a Concert-owned
  agreement makes it a local service call, not a cross-module hop.
- **Sits with the artifact it makes truthful.** The invoice legend Concert already prints asserts an
  agreement; keeping the agreement in Concert keeps assertion and evidence in one module.

**Designs considered**

| Design | Result |
|---|---|
| Concert owns the agreement | **chosen** — reuses all shipped e-sign/PDF/HATEOAS machinery in place; enforcement is a local call |
| Tenant module owns it (beside `TaxCompliance`; gate via `ITenantModule.HasCurrentSelfBillingAgreementAsync`, mirroring `IsTaxComplianceCompleteAsync`) | rejected — natural single-owner/compliance analogy, but forces extracting `ESignature` + `IPdfBlobCache` to a shared seam and re-homing the PDF document, reinventing machinery the brief says to reuse |
| Per-booking agreement (one per settlement) | rejected — HMRC self-billing is a **standing** supplier relationship, not per-invoice; per-booking would re-collect consent every gig |
| Mutate one agreement row on renewal | rejected — the record must be immutable evidence; renewal is a new append-only acceptance (mirrors invoice/contract immutability) |

**Cost of the choice (accepted):** the agreement is a **single-supplier-tenant** fact, whereas every
existing Concert entity is `IVenueArtistTenantScoped` (two-party). So Concert gains, for this one
aggregate, a single-owner tenancy stance and a system (tenant-filter-free) read — both standard
composed-tenancy building blocks (see [`../../api/agents/CODE_PATTERNS.md`](../../api/agents/CODE_PATTERNS.md),
"Tenancy is composed"). Details in §5.

## 4. Domain model

`SelfBillingAgreementEntity` (Concert module, `Domain/Entities/`), following the shipped
`InvoiceEntity` / `ContractEntity` immutability shape (private setters, private EF ctor, single static
`Create(...)` factory, `[DisplayName(DisplayNames.SelfBillingAgreement)]`):

- **Tenancy:** `ITenantScoped` single-owner — `TenantId` = the **supplier** tenant. *Not*
  `IVenueArtistTenantScoped`; there is no counterparty on this record.
- **Identity snapshot (frozen at acceptance):** the supplier's legal identity — reuse `InvoiceParty`
  (legal name, VAT number, registered address) built the same way `InvoiceIssuer` builds it from
  `ITenantModule` (`GetByIdAsync` + `GetTaxComplianceAsync`). Freezing it means the signed agreement
  states who accepted, as they were then.
- **Consent:** `ESignature Supplier` (the shipped VO — required typed `SignatoryName`, optional
  `DrawnSignatureImage`, server-owned `UserId`/`AtUtc`/`Ip`/`UserAgent`).
- **Dates:** `AcceptedAtUtc`; `ExpiresAtUtc = AcceptedAtUtc + 12 months` (the HMRC ≤12-month review).
- **Terms provenance:** `string PlatformTermsVersion` snapshot (mirrors `ContractEntity`), and the
  rendered self-billing clause `string ClauseText` frozen at acceptance.
- **Artifact:** `string? PdfBlobName = $"self-billing-agreements/{tenantId}-{Guid.NewGuid():N}.pdf"`,
  assigned in the factory; bytes rendered lazily on first download via `IPdfBlobCache` (mirrors invoice
  and contract — no pre-generation).
- `CreatedAtUtc`.

**Append-only series + "in force":**

- Each acceptance (grant **or** renewal) is a **new immutable row**; nothing is mutated.
- The tenant's **current** agreement = the latest acceptance whose `ExpiresAtUtc > now`.
- **In force** ⇔ such a row exists. If the latest acceptance is expired, the supplier is not in force
  until a renewal is recorded.

`DisplayNames.SelfBillingAgreement` and `Schema.Tables.SelfBillingAgreements` are added; blob prefix is
`self-billing-agreements/`.

## 5. Persistence — two composed stances

Per the composed-tenancy pattern, the single entity is read through **two** stances (one stance per
repository class):

- **Single-owner self-service** — `SelfBillingAgreementRepository` on a `TenantScopedDbContext`-composed
  context (`ApplySingleOwner`, `TenantId == current`). Backs the supplier grant/renew, read-own-current,
  and download-own-PDF flows. A stranger reading another tenant's agreement gets a filtered **404**,
  never a probeable 403 — same stance as the invoice/contract repositories.
- **System gate read** — `ISelfBillingAgreementGate.HasCurrentAsync(supplierTenantId, nowUtc)` on a
  tenant-filter-free stance (composes the module provider with no tenancy, read-only — the
  `PublicDbContext` shape), answering a boolean by **explicit** supplier tenant id. Required because the
  fail-closed check in `FinishExecutor` runs for a supplier who is not the request tenant, and the
  hourly completion sweep runs with **no** tenant context at all. This exactly parallels how
  `IsTaxComplianceCompleteAsync` is answered by explicit id today.

Model change ⇒ re-scaffold via `./initial-migrations.ps1` from `api/` (no additive migrations).

## 6. Phases

Each phase ends green (build + affected unit/integration) and is independently shippable. The order puts
the **grant path before enforcement**, so a supplier (and the dev/E2E seeder) can hold consent *before*
settlement requires it — otherwise turning on the gate would defer every settlement with no way to
satisfy it.

### Phase 1 — Agreement domain + persistence + gate (dormant)

Build the record and both stances; nothing enforces or surfaces it yet.

- [ ] `SelfBillingAgreementEntity` (§4), `DisplayNames.SelfBillingAgreement`,
  `Schema.Tables.SelfBillingAgreements`, EF configuration (`InvoiceParty` + `ESignature` as complex
  properties, reusing `InvoicePartyConfiguration` / `ESignatureConfiguration`).
- [ ] Single-owner `SelfBillingAgreementRepository` + system-read `ISelfBillingAgreementGate` (§5).
- [ ] `ISelfBillingAgreementService` — grant/renew (build the `InvoiceParty` snapshot from
  `ITenantModule`, compose the supplier `ESignature` from the request + server ambient context via
  `ICurrentUser`/`IClientContext`, set the 12-month expiry), read-current, and PDF via `IPdfBlobCache` +
  a new `SelfBillingAgreementDocument : IDocument` (clause text, both-side identity + supplier VAT
  number, accepted-at/expiry, platform terms version, a Signatures block reusing the
  `ContractDocument.Signature(...)` render of typed name + optional drawn image + attribution line).
- [ ] Re-scaffold Concert migration (`./initial-migrations.ps1`).
- [ ] **Verification gate:** `dotnet build api/Concertable.slnx` green; Concert unit + integration
  (`integration-debug`) green, asserting: `ExpiresAtUtc == AcceptedAtUtc + 12 months`; a renewal appends
  a new row and becomes current; current-resolution picks the latest non-expired acceptance; the gate is
  true when in force, false when none/expired; immutability (no public setters, `Create` factory only);
  single-owner scoping (owner reads own, stranger 404); PDF renders lazily under
  `self-billing-agreements/` and contains the clause, the supplier VAT number, and the signature.

### Phase 2 — Supplier-facing grant/renew surface (not yet enforced)

Give suppliers (and the seeder) a way to hold consent; still nothing gates settlement.

- [ ] Endpoints (a `SelfBillingAgreementController`, or an existing compliance controller, reachable by
  **both** tenant types — not `[RequiredTenantType(TenantType.Venue)]`): `GET .../self-billing-agreement`
  (current status + `ExpiresAtUtc`), `POST .../self-billing-agreement` (grant/renew, body carries
  `ESignatureRequest`; presence of the e-signature *is* the consent — reject with 400 if absent, mirror
  Apply/Accept), `GET .../self-billing-agreement/pdf`. Single-owner scoped.
- [ ] HATEOAS: a `selfBillingAgreement` action; the POST affordance labelled **grant** when none exists
  and **renew** when the current one is absent/expired/nearing expiry (state-gated, same `ActionLink`
  pattern as the `Invoice`/`Contract` links).
- [ ] SPA (`app/web/b2b/shared/...`, compiled into both manager apps): reuse the existing
  `ESignaturePanel` / `SignatureCanvas`; add the grant/renew page + a
  `useDownloadSelfBillingAgreementMutation` mirroring `useDownloadContractMutation`; a dashboard nag when
  the agreement is missing or within the renewal window (mirror the DAC7 tax-details nag).
- [ ] Dev/E2E seeder grants an agreement for each seeded supplier tenant by **calling the grant service**
  (a production write path a supplier performs — legitimate to seed, not an event-reaction bypass; see
  [`../../api/agents/SEEDING_CONVENTIONS.md`](../../api/agents/SEEDING_CONVENTIONS.md)), so post-Phase-3
  settlements are not all deferred.
- [ ] **Verification gate:** solution + both manager SPAs build; Concert unit + integration green:
  grant 400 without consent; grant records the supplier e-signature (typed name + server attribution);
  renew before and after expiry; read-current status; **both** artist and venue tenants can grant;
  download-own PDF / stranger 404; HATEOAS grant-vs-renew affordance flips with state.

### Phase 3 — Fail-closed enforcement + roadmap tick (final)

Turn the gate on where the invoice is minted; make the invoice legend truthful.

- [ ] In `FinishExecutor`, after the existing tax-compliance gate and using the already-resolved
  `supplierTenantId`, call `ISelfBillingAgreementGate.HasCurrentAsync`. If not in force → return a new
  `SettlementOutcome.DeferredPendingSelfBillingAgreement`, mint **no** invoice, and surface the reason
  (a deferred-reason KPI/read, mirroring `AwaitingDoorRevenue` / `DeferredPendingTaxCompliance`). The
  hourly completion sweep re-attempts and self-heals once the supplier grants/renews; the per-supplier
  sequence number is only consumed when an invoice actually commits, so no number is skipped across a
  deferral.
- [ ] Supplier-facing deferred surface: the settlement/payout screen tells the supplier *why* issuance
  is blocked and links to the grant/renew affordance.
- [ ] Tick the roadmap **in this commit**: [`LAUNCH_ROADMAP.md`](./LAUNCH_ROADMAP.md) 🟡 "Self-billed VAT
  invoice engine" line → ✅ (invoice engine now complete: generation + agreement + renewal), and the §7
  "Definition of launch-ready" self-billed-invoice checklist line. The roadmap is never deleted.
- [ ] **Verification gate:** `dotnet build api/Concertable.slnx` green; Concert unit + integration green:
  settlement with no agreement → `DeferredPendingSelfBillingAgreement`, no invoice minted, reason
  surfaced; after a grant, the next sweep issues the invoice; gap-free per-supplier numbering preserved
  across the deferral; the invoice legend now always corresponds to an in-force agreement. **Final phase:
  merge-queue full E2E tier** — this changes settlement behaviour and adds a user-facing legal/compliance
  flow across both SPAs, so it is **not** `skip-e2e`-eligible.

## 7. Verification coverage (summary)

- Supplier direction is read from `SettlementPayeeResolver`; no new deal-type branch is introduced.
- The 12-month window, append-only renewal, and current-resolution are correct and immutable.
- The gate is true only while an agreement is in force; false for none/expired.
- Both tenant types can grant; consent requires the e-signature; strangers get 404, not 403.
- Settlement is fail-closed and self-healing: deferred without a current agreement, issued after grant,
  with gap-free numbering preserved.
- The self-billed-invoice legend is truthful — an in-force agreement is guaranteed whenever an invoice
  is minted.

## 8. Completion criteria

- Every supplier tenant holds an immutable, e-signed self-billing agreement with a 12-month review, and
  a renewal path exists before and after expiry.
- No self-billed invoice can be issued for a supplier without a current agreement; the deferral reason is
  surfaced and self-heals on grant.
- The supplier can grant/renew/download consent from the manager SPA, HATEOAS-gated, as either tenant
  type.
- The `LAUNCH_ROADMAP.md` self-billed-invoice line is ✅ and the launch-ready checklist line ticked.
- All affected verification gates are green; because Phase 3 touches `api/**`, the plan and its ledger
  stay open until the `chore/platform-sync-*` PR is green/merged, then both are deleted together in the
  close-out change (git history is the archive). Lifecycle per [`../agents/PLAN.md`](../agents/PLAN.md).
