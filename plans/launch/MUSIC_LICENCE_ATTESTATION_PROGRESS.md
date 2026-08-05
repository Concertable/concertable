# Music licence attestation progress

- Plan: `plans/launch/MUSIC_LICENCE_ATTESTATION_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable`
- Branch: `Feature/launch_music-licence-attestation`
- PR: not opened
- Dependency/package gates: none pre-merge. Post-merge: `chore/platform-sync-*` (api/** MinVer bump), expected non-breaking.
- Last reconciled: 2026-08-05 — branch created off `origin/main` (0 behind), no open red platform-sync PR.

## Current state

Plan and this ledger written; **no code changes yet.** Branch `Feature/launch_music-licence-attestation`
is at `origin/main`. The design (one `bool HoldsMusicLicence` threaded through the shipped `TaxCompliance`
DAC7 slice) is fully specified in the plan, including the exact files and every construction site a new
`required` DTO member touches.

## Next Steps

Implement Phase 1 (the whole vertical slice — one PR) per `MUSIC_LICENCE_ATTESTATION_PLAN.md` §3, in order:

1. **Domain** — `Tenant/…/Domain/ValueObjects/TaxCompliance.cs`: add `public bool HoldsMusicLicence { get; private init; }` + a `bool holdsMusicLicence` ctor param (no validation).
2. **Migration** — after the model change, run `./initial-migrations.ps1` from `api/` (re-scaffold; the Tenant `InitialCreate` gains the column). EF auto-maps the bool — no `TenantEntityConfiguration` change.
3. **Contracts DTO** — `Tenant/…/Contracts/TaxComplianceDto.cs`: add `public required bool HoldsMusicLicence { get; init; }` (not `[JsonIgnore]`).
4. **Mapper** — `Tenant/…/Application/Mappers/TenantMappers.cs`: carry the field in both `ToDto` and `ToTaxCompliance`. (Validator/request/service/module: no change — see plan.)
5. **Cross-module compile fix** — `Concert/…/Concert.UnitTests/Services/SelfBillingAgreementServiceTests.cs` (~line 41): set `HoldsMusicLicence` in the `new TaxComplianceDto { … }`.
6. **Backend tests** — add the ctor arg to every `new TaxCompliance(…)` (`TaxComplianceTests`, `TenantServiceTests`, `TenantEntityTests`) and the DTO builder in `TenantValidatorsTests`; extend `TaxComplianceRoundTripTests` (`BuildRequest` → true, replacement → false).
7. **Web** — `app/web/b2b/shared/src/features/organizations/`: `types.ts`, `schemas/updateOrganizationRequestSchema.ts`, `hooks/useOrganization.ts`, `components/OrganizationForm.tsx` (new "Music licence" checkbox section), `taxFormLabels.ts`.

Then the verification gate (plan §6): `dotnet build api/Concertable.slnx` = 0 errors; Tenant unit+integration + touched Concert unit tests green via `integration-debug`; `./initial-migrations.ps1` run; all four web builds green. Commit when green. Then open the PR, tick roadmap line 26 + §7 in the same commit as the feature, `/merge` with **full E2E** (do not skip), and own the platform-sync PR to green.

## Completed work

- 2026-08-05 — `Feature/launch_music-licence-attestation` created off `origin/main`; plan + this ledger authored. (commit pending)

## Verification

None yet.

## Reviews

None yet.

## Decisions, discoveries, blockers, and deviations

- **D1** — field goes on the existing `TaxCompliance` VO (the roadmap's `Tenant.Compliance`), not a new VO/config sub-structure; a `TaxCompliance`→`Compliance` rename is out of scope for this isolated change.
- **D2** — non-nullable `bool` (the VO is all-or-nothing, so no "unknown" third state; unchecked = a valid negative attestation, like the VAT checkbox).
- **D3** — record-only: not wired into `IsTaxComplianceCompleteAsync`, settlement, payouts, or invoices.
- **D4** — shown on the shared org form for all B2B tenants (no `isVenueManager` branching); venue-only-via-slot is the noted follow-up alternative.
- **D5** — backend + web ship in one PR (a `required` DTO field couples them); no internal publish gate — `Tenant.Contracts` is B2B-internal (Concert consumes it by project reference, not as a cross-service package).
- **Discovery** — adding a `required` member to `TaxComplianceDto` forces a compile-fix in the **Concert** module's `SelfBillingAgreementServiceTests.cs` (constructs the DTO) and `TenantValidatorsTests.cs`. `InvoiceIssuer.BuildPartyAsync` only reads named fields, so the flag never leaks onto an invoice.

## Event log

### 2026-08-05 — plan spun off roadmap line 26

- Action: read the launch roadmap (line 26 / §5 / §7), `LEGAL_REQUIREMENTS.md`, and the full shipped DAC7 slice (domain VO, EF config, DTO, mapper, validator, service, module boundary, tests, and the b2b/shared org form); created branch `Feature/launch_music-licence-attestation` off `origin/main`; wrote `MUSIC_LICENCE_ATTESTATION_PLAN.md` + this ledger.
- Evidence: `git rev-list --count HEAD..origin/main` = 0; no open `chore/platform-sync-*` PR; construction sites enumerated by grep (`new TaxCompliance(` ×4, `new TaxComplianceDto {` in Concert + Tenant validator tests).
- Outcome: design fixed; ready to implement Phase 1.
- Follow-up: implement per `## Next Steps`.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable
Read @plans/launch/MUSIC_LICENCE_ATTESTATION_PLAN.md and @plans/launch/MUSIC_LICENCE_ATTESTATION_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
