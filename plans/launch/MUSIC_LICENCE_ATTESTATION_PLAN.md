# Music licence attestation — `holdsMusicLicence` on `Tenant.Compliance`

> Spun off `LAUNCH_ROADMAP.md` line 26 / §5 "Music licence attestation field" (Swim-lane C).
> **Next steps live in @plans/launch/MUSIC_LICENCE_ATTESTATION_PROGRESS.md → `## Next Steps`** — this
> plan holds the design and outstanding phase only, no next-action prose.

## 1. Outcome

A venue records, in the Org setup form, whether it holds the live-music licence it is legally required
to hold (PRS for Music / a venue live-music licence). The declared flag persists on
`Tenant.Compliance`, round-trips through the org read/update DTO + API, and is displayed/editable in
the B2B manager SPA. This satisfies the §7 launch checkbox "Music licence attestation captured in Org
setup form" and roadmap line 26.

**We record the attestation only.** No verification, no third-party check, no gating of payouts or
bookings on it. Holding the licence is the venue's legal responsibility and liability
(`LEGAL_REQUIREMENTS.md` item 5); we store the declared flag and nothing more.

## 2. Where it lives — extend the shipped DAC7 slice, don't invent plumbing

The compliance value object the roadmap calls `Tenant.Compliance` is the shipped `TaxCompliance` VO in
the Tenant module (`TenantEntity.TaxCompliance`), the same one the DAC7 fields (`VatNumber`,
`SellerIdentifier`, `RegisteredAddress`, `BankReference`) live on. This feature adds **one `bool`** to
that VO and threads it through the exact same layers the DAC7 fields already established — no new
compliance sub-structure, no new endpoint, no new module method.

## 3. Design — the vertical slice

One `bool HoldsMusicLicence`, threaded end to end. Files, in dependency order:

### Backend — Tenant module (`api/Concertable.B2B/src/Modules/Tenant/`)

1. **`Domain/ValueObjects/TaxCompliance.cs`** — add `public bool HoldsMusicLicence { get; private init; }`
   and a `bool holdsMusicLicence` constructor parameter, assigned straight through. **No validation** —
   every bool is a valid attestation. Give it an XML `<summary>` in the house style ("A self-declared
   attestation that the tenant holds the live-music licence it is required to hold; recorded, never
   verified — the tenant's liability.").
2. **`Infrastructure/Data/Configurations/TenantEntityConfiguration.cs`** — **no change required.** EF
   auto-maps the new property of the owned `TaxCompliance` type as a non-null `bit` column; there is no
   max-length/required config to add for a value-type bool. (The column materialises on re-scaffold.)
3. **Migration** — run `./initial-migrations.ps1` from `api/` to re-scaffold every module's
   `InitialCreate` (the Tenant one gains the `HoldsMusicLicence` column). Never an additive migration
   (`api/AGENTS.md`).
4. **`Contracts/TaxComplianceDto.cs`** — add `public required bool HoldsMusicLicence { get; init; }`. It
   is always present when the DTO exists (unlike optional `VatNumber`), so it is `required` and **not**
   `[JsonIgnore]`.
5. **`Application/Mappers/TenantMappers.cs`** — carry the field both ways: `HoldsMusicLicence = …` in
   `ToDto(this TaxCompliance)`, and pass `dto.HoldsMusicLicence` into the constructor in
   `ToTaxCompliance(this TaxComplianceDto)`.
6. **`Application/Validators/TenantValidators.cs`** — **no change.** A bool has no format to validate;
   `required` + System.Text.Json required-member enforcement already reject a request that omits it (a
   400 at deserialisation, same as the other required fields).
7. **`Application/Requests/UpdateTenantRequest.cs`**, **`Contracts/ITenantModule.cs`**,
   **`Infrastructure/Services/TenantService.cs`** — **no change.** The request wraps `TaxComplianceDto`;
   the flag rides the existing `GetTaxComplianceAsync`/org-read DTO across the module boundary but
   nothing consumes it there. `IsTaxComplianceCompleteAsync` is untouched — completeness stays "the VO
   is present", the bool does not gate it.

### Backend — cross-module compile fix (Concert)

8. **`Concert/Tests/Concertable.B2B.Concert.UnitTests/Services/SelfBillingAgreementServiceTests.cs`** —
   the `new TaxComplianceDto { … }` (~line 41) must set `HoldsMusicLicence` now that it is `required`.
   Mechanical; no behaviour change — `InvoiceIssuer.BuildPartyAsync` reads only named invoice fields, so
   the flag never appears on an invoice.

### Backend — tests (Tenant module)

9. **Unit** — add the new constructor argument to every `new TaxCompliance(…)` site
   (`TaxComplianceTests.cs`, `TenantServiceTests.cs`, `TenantEntityTests.cs`) and the
   `new TaxComplianceDto { … }` builder in `TenantValidatorsTests.cs`. In `TaxComplianceTests`, assert
   the flag stores through the constructor and add a true/false `[Theory]`.
10. **Integration — `TaxComplianceRoundTripTests.cs`** — `BuildRequest()` sets `HoldsMusicLicence = true`;
    the replacement in `Update_ReplacesExistingTaxCompliance` sets `false` (proves both values
    round-trip through a fresh context); add the argument to the direct-VO `expected` in
    `Update_RoundTrips…`. The existing whole-DTO equality assertions already cover the value; an explicit
    `Assert.True/False(read.TaxCompliance!.HoldsMusicLicence)` is optional colour.

### Web — B2B shared org form (`app/web/b2b/shared/src/features/organizations/`)

11. **`types.ts`** — add `holdsMusicLicence: boolean;` to the `TaxCompliance` interface (required — always
    present when `taxCompliance` is).
12. **`schemas/updateOrganizationRequestSchema.ts`** — add `holdsMusicLicence: z.boolean()` to
    `taxComplianceSchema`.
13. **`hooks/useOrganization.ts`** — add `holdsMusicLicence: boolean` to `OrganizationBuffer`; map
    `holdsMusicLicence: buffer.holdsMusicLicence` into the `taxCompliance` object in `save`.
14. **`components/OrganizationForm.tsx`** — `initialBuffer`: `holdsMusicLicence: tax?.holdsMusicLicence ?? false`;
    add a `Separator`-delimited "Music licence" section with a `Checkbox` bound to
    `buffer.holdsMusicLicence` (mirror the existing VAT-registered checkbox), a `Label`, and a
    `text-muted-foreground text-xs` hint that it is a self-attestation and the tenant's responsibility.
15. **`taxFormLabels.ts`** — add `musicLicenceLabel` + `musicLicenceHint` (region display copy is owned by
    the frontend, per the file's existing convention).

## 4. Decisions

- **D1 — on the existing `TaxCompliance` VO, not a new VO or config sub-structure.** Per the handoff and
  the DAC7 pattern. There is a mild naming tension (a *music* licence on a type named `TaxCompliance`),
  but renaming `TaxCompliance` → `Compliance` is a wide grep-gated rename across backend + web + tests,
  out of scope for this isolated ~0.5-day change. Noted, deliberately deferred.
- **D2 — non-nullable `bool`, not `bool?`.** `TaxCompliance` is all-or-nothing: absent until org setup,
  fully populated after. There is no "unknown" third state to model — before setup the whole VO is null;
  after setup the checkbox carries a definite true/false the operator submitted. This matches the VO's
  "presence IS completeness" invariant and the existing VAT-checkbox UX (unchecked = a valid negative
  declaration).
- **D3 — record-only, no gate.** Not wired into `IsTaxComplianceCompleteAsync`, settlement, payouts, or
  invoices. The venue's liability; we neither verify nor gate on it.
- **D4 — shown on the shared org form for all B2B tenants.** No `isVenueManager` branching (forbidden in
  `app/web/b2b/shared`), consistent with VAT/seller-id already being shared across both manager apps.
  Worded for the live events the org hosts. *Alternative if product wants it hidden from artists:*
  venue-only via slot injection from the venue app — a follow-up, not this isolated change.
- **D5 — backend + web in one PR.** A `required` DTO field means the SPA must send it or every org PUT
  400s, so the two halves are not independently shippable. No internal merge/publish gate splits the
  slice: everything is inside the B2B service, and `Tenant.Contracts` is a B2B-internal contract Concert
  consumes by project reference — not a cross-service published package.

## 5. Out of scope

Licence verification; any payout/booking gate; the PRS rate + per-tenant pass-through and the tenant
configuration surface (separate unbuilt roadmap items — `LEGAL_REQUIREMENTS.md` item 5); folding into the
commission or DAC7 work; renaming `TaxCompliance`.

## 6. Phase 1 — the vertical slice (one PR)

Implement steps 1–15 in order (backend model → re-scaffold migration → DTO/mapper → cross-module test fix
→ backend tests → web). One coherent PR; a backend commit then a web commit is fine for reviewability.

**Verification gate:**
- Tenant module build and focused unit tests green locally, together with the touched Concert unit
  tests.
- `./initial-migrations.ps1` run from `api/` (the model changed).
- Push the coherent checkpoint. Exact-head PR CI owns the full solution, standalone carves, all four
  web builds, and complete unit/integration matrices. A remote red integration job enters the
  `integration-debug` skill at its narrowest failing scope.
- **Merge-queue E2E tier: full E2E, do not skip.** The change touches a module/package boundary
  (`Tenant.Contracts`), shared web code, and a user-facing org-setup flow — it fails the `skip-e2e`
  criteria. Let the merge queue run E2E; do not duplicate it locally.

## 7. Delivery & close-out

- Open the PR with plain `gh pr create` (personal repo — no `AB#`, no assignee).
- **Tick roadmap line 26 (🟡 → ✅) and the §7 "Music licence attestation captured in Org setup form"
  checkbox in the same commit as the feature** (`ROADMAP.md`: update in the same commit as the work, not
  a deferred pass). Do not delete the roadmap.
- Merge via `/merge` (full E2E tier).
- **Own the post-merge `chore/platform-sync-*` PR to green.** This is an `api/**` change, so MinVer
  republishes and platform-sync bumps every service's pin; expected non-breaking (no cross-service
  published contract changed) → should auto-merge. A red sync is this plan's to fix.
- **Close out only after platform-sync is green.** Because there is a post-merge package gate, the plan
  does not close in the feature commit. After the sync lands, `git rm` this plan and its `_PROGRESS.md`
  as a doc-only close-out riding the next change — never its own PR (`PLAN.md` Lifecycle 5 / doc-only
  close-out).
