# Rename: free "Contract" from the pricing model; put it on the real contract

> **STATUS — Phase 6 implemented; E2E smoke pending before final `git rm`.** Phases 1–5 done &
> committed (`5bdcefad`). Phase 6 (the WIRE sync) is now done in code: JSON keys (`contract`→`deal`,
> `contractType`→`dealType` incl. the `IDeal.DealType` property), routes (`/agreement`→`/contract`),
> HATEOAS link, PDF filename/ref/blob-path/`DisplayName`, `data-testid`s (+ E2E C# page objects), UI
> copy, the type-aware lowercase-identifier scrub, and the Concert migration (`DealType` column). The
> `ContractBuilder` was subsequently renamed `ContractIssuer` (issues the contract) — kept.
> **Gate passed:** `grep -rniE "agreement|contracttype|bookingagreement"` = 0 repo-wide (no allowlist
> needed). **Verified:** solution build (0 errors) · Concert integration 105/105 · all 4 web builds.
> **Remaining:** `e2e-ui-debug` smoke (Phase 6 touches covered flows), then `git rm` this plan.

**Decision (settled):** the word *contract* is currently on the one thing that is **not** a contract
(the money-terms bag on the Opportunity), while the thing that genuinely **is** a contract is called
`BookingAgreement`. Fix the inversion:

1. The deal-terms family (`IContract` / `*Contract` records / `ContractType` / `ContractEntity` +
   the whole `Concertable.B2B.Contract` module) → **`Deal`**. These are the four **deal structures**
   (flat fee, door split, versus, venue hire) — the economic arrangement a booking agent negotiates,
   not a contract. `DealType` is the strategy key that selects the settlement/workflow strategy.
2. `BookingAgreementEntity` (parties + both e-signatures + snapshotted terms + PDF, formed at Accept)
   → **`Contract`**. It is exactly a contract; "agreement" was only ever a synonym-hedge forced by
   the name being spent elsewhere. Freed, `Contract` is the precise word for the binding artifact.
3. The leak: `ContractStateMachine` drives the **application lifecycle** (`LifecycleState`), not a
   contract → **`LifecycleStateMachine`**.

Full reasoning: see the design discussion in git history / the commit that lands this plan. The
short version — a thing that exists before a counterparty exists, that one side edits unilaterally,
and that records no assent, is the *opposite* of a contract; the thing with parties, mutual
signatures, immutable agreed terms, a formation moment and a PDF instrument *is* one.

## Why this is safe to do as an ordinary refactor (not expand/contract)

- **B2B-internal.** Nothing outside B2B references `Concertable.B2B.Contract.*` — Payment does **not**
  reference the B2B contract types or `ContractType` (verified). So this is **not** a published-package
  break; no expand/contract across merges (contrast `plans/PDF_RENDERER_RENAME.md`). One branch, phased.
- **The Rust extraction does not delete this.** `plans/RUST_CONTRACT_MICROSERVICE.md` §7.1/§7.8: the
  engine is **stateless** — B2B keeps the deal entities, tables, `DbContext`, and the persisted
  `DealType` column. Only the Concert-module *workflow engine* (executors/factory/validator) is
  replaced at cutover. So this rename is **durable**, not churn about to be deleted.
- **Wire keys DO change — in Phase 6.** Phases 1–5 kept the wire (`contract`/`contractType` JSON keys,
  `/agreement` routes) as a *staging step*, but the end state must be consistent, so Phase 6 renames
  the wire on BE **and** SPA together (single deploy). **Only** the `$type` discriminator *values*
  (`"flatFee"`/`"doorSplit"`/`"versus"`/`"venueHire"`) and enum member names (`FlatFee`, …) stay — they
  already name the deal structures. DB table/discriminator names change via nuke-and-rescaffold
  (`./initial-migrations.ps1`, dev/E2E only, no prod data) per `api/CLAUDE.md`.

## Branch / timing

Most of the deal-terms surface is **already merged to master**, so this is a `Refactor/DealRename`
branch off master — **not** part of `Feature/BookingAgreement`. Run it **after `Feature/BookingAgreement`
merges**, so Phase 2 (renaming the agreement surface) operates on merged code rather than racing the
feature. (This plan file is doc-only — commit it on the current branch per `plans/CLAUDE.md`.)

## Ordering constraint (must hold)

Phase 1 **frees** the `Contract*` names; Phase 2 **reuses** them for the agreement. Do Phase 1 before
Phase 2 or the two `ContractEntity` types collide. Phases 3–4 are independent.

---

## Naming map

### The deal-terms family → `Deal` (was wrongly "contract")

| Now | Rename to |
|---|---|
| module `Concertable.B2B.Contract` (all 5 projects + `.slnx`) | `Concertable.B2B.Deal` |
| `IContract` | `IDeal` |
| `FlatFeeContract` / `DoorSplitContract` / `VersusContract` / `VenueHireContract` (records) | `FlatFeeDeal` / `DoorSplitDeal` / `VersusDeal` / `VenueHireDeal` |
| `ContractType` enum | `DealType` |
| `ContractTypeNames` (const strings — **values unchanged**) | `DealTypeNames` |
| `ContractEntity` (TPH root) + `*ContractEntity` subtypes | `DealEntity` + `*DealEntity` |
| `IContractModule` / `ContractModule` | `IDealModule` / `DealModule` |
| `IContractService` / `ContractService` | `IDealService` / `DealService` |
| `IContractRepository` / `ContractRepository` | `IDealRepository` / `DealRepository` |
| `ContractDbContext` (+ factory, config provider, EF configs) | `DealDbContext` |
| `IContractMapper` / `ContractMapper` / `*ContractMapper` | `IDealMapper` / `DealMapper` / `*DealMapper` |
| `IContractUpdater` / `ContractUpdater` / `*ContractUpdater` | `IDealUpdater` / `DealUpdater` / `*DealUpdater` |
| `ContractController` | `DealController` |
| `IContractStrategy` | `IDealStrategy` |
| `IContractResolver` / `IContractAccessor` / `ContractAccessor` (Concert side) | `IDealResolver` / `IDealAccessor` / `DealAccessor` |
| `IContractFingerprintComponent` | `IDealFingerprintComponent` |
| `OpportunityEntity.ContractId` (+ `GetContractIdByIdAsync` on repos) | `DealId` (+ `GetDealIdByIdAsync`) |
| `ContractFactory` (Seed), `ContractDevSeeder` / `ContractTestSeeder` | `DealFactory`, `DealDevSeeder` / `DealTestSeeder` |
| `ContractStateMachineTests` **(this is the leak, see below — not deal terms)** | (Phase 3) |

Judgment calls (decide when you hit them, default in parens):
- `AgreementTermsRenderer` takes an `IDeal` and produces the terms prose → **`DealTermsRenderer`**
  (renders a deal's terms). `TermsFingerprintCalculator` stays — it fingerprints *terms*, correct.
- `TicketPayeeResolver` / `ArtistShareCalculator` / `*Calculator` already name what they do — leave them;
  only their `ContractType`/`IContract` parameters get the type rename.

### The real contract: `BookingAgreement` → `Contract` (Phase 2)

| Now | Rename to |
|---|---|
| `BookingAgreementEntity` (Concert.Domain) | `ContractEntity` |
| `IBookingAgreementService` / `BookingAgreementService` | `IContractService` / `ContractService` |
| `IBookingAgreementBuilder` / `BookingAgreementBuilder` | `IContractBuilder` / `ContractBuilder` |
| `IBookingAgreementRepository` / `BookingAgreementRepository` | `IContractRepository` / `ContractRepository` |
| `IBookingAgreementPdfService` / `BookingAgreementPdfService` | `IContractPdfService` / `ContractPdfService` |
| `BookingAgreementDocument` (QuestPDF) | `ContractDocument` |
| `BookingAgreementDtos` / `BookingAgreementDto` / `AgreementPdf` | `ContractDtos` / `ContractDto` / `ContractPdf` |
| `BookingAgreementEntityConfiguration` (+ table `BookingAgreements`) | `ContractEntityConfiguration` (table `Contracts`) |
| `BookingAgreementApiTests` | `ContractApiTests` |

Wire-touching bits in this phase (single app + SPA, one deploy — safe to change together, but each is
optional; keep if you'd rather not touch the SPA):
- HTTP route(s) exposing the agreement (`ConcertController` / `ApplicationController`) and the SPA hook
  `app/web/b2b/shared/src/features/concerts/hooks/useDownloadAgreement.ts` — decide keep-or-rename the
  `booking-agreement` path.
- The download filename `booking-agreement-BA-{id}.pdf` in `BookingAgreementService`.

### The leak → `LifecycleStateMachine` (Phase 3)

| Now | Rename to |
|---|---|
| `ContractStateMachine` (Concert.Domain.Lifecycle) | `LifecycleStateMachine` |
| `ContractStateMachineTests` | `LifecycleStateMachineTests` |

---

## Phases

Each phase is one commit (or a tight few), builds green, and ends on a passing gate.

### ✅ Phase 1 — deal-terms family → `Deal` (DONE — branch `Refactor/DealRename`)
~~Rename per the first table across: the whole `Concertable.B2B.Contract` module (5 projects + folders +
namespaces + `Concertable.slnx` / `Concertable.B2B.slnx` entries), the Concert-module consumers
(`DealAccessor`, resolvers, workflows, executors, renderers/fingerprint, mappers, `OpportunityEntity.DealId`),
Seed factories/seeders, and all tests. **Keep** every JSON/enum string value and the JSON
`[JsonDerivedType]` discriminators unchanged.~~
- Done. Wire key `contractType` deliberately **kept**: the enum *type* is `DealType`, but the
  property `IDeal.ContractType` retains its name so the SPA wire contract is untouched (SPA-side
  rename deferred to Phase 4). Tables `Deals`/`FlatFeeDeals`/…, schema `deal`; `Opportunities.DealId`.
- Migrations re-scaffolded; only the two changed contexts (Deal, B2B Concert) committed — the
  timestamp-only churn on unaffected modules was reverted to keep the diff honest.
- **Gate met:** `dotnet build api/Concertable.slnx` green · Deal 10/10 + Concert 57/57 unit green ·
  Concert integration green.

### ✅ Phase 2 — the agreement → `Contract` (DONE — depends on Phase 1)
~~Now that `Contract*` is free, rename the second table. Re-scaffold migrations (table `BookingAgreements`
→ `Contracts`).~~
- Done: `BookingAgreementEntity`→`ContractEntity`, service/builder/pdf-service/repository/document/
  DTO/config/mappers/tests → `Contract*`, `AgreementPdf`→`ContractPdf`. Concert migration
  re-scaffolded (table `Contracts`).
- **SPA/wire left untouched** (the plan's optional bits): HTTP routes `{id}/agreement[/pdf]`, the
  HATEOAS `Agreement` link + `ApplicationDto.AgreementId`, the download filename, and the SPA hook
  all keep their names — backend types renamed only, no JSON-key/route change, no SPA builds needed.
- Also fixed a Phase-1 straggler: `AgreementTermsRendererTests` → `DealTermsRendererTests` (it tests
  the now-`DealTermsRenderer`).
- **Gate:** solution build green · Concert unit 57/57 · Concert integration green.

### ✅ Phase 3 — the leak → `LifecycleStateMachine` (DONE)
~~Rename `ContractStateMachine` (+ tests). Pure Concert-domain rename, no model change.~~
Done: `ContractStateMachine`→`LifecycleStateMachine` (+ `*Tests`) across Concert.Domain.Lifecycle
and its workflow-registry/builder consumers. `ConcertStateMachine*` (a different type) untouched.
- **Gate:** build green · Concert unit tests green.

### ✅ Phase 4 — docs + Rust-plan re-alignment (mostly DONE; doc-only)
- ✅ Moved the module doc folder to `Modules/Deal/`; **names** in `ARCHITECTURE.md` +
  `LEGAL_REQUIREMENTS.md` updated, title → "Deal Architecture", and a **staleness banner** added at the
  top (the §2+ workflow narrative still describes the pre-executor / `ConcertStage` design — a full
  narrative rewrite is pre-existing staleness, *not* created by this rename).
- ✅ Updated references in `api/docs/CODE_PATTERNS.md`, `api/docs/MICROSERVICES_ARCHITECTURE.md`,
  `api/Concertable.B2B/ARCHITECTURE.md`, `api/Concertable.B2B/TECH_DEBT.md`.
- ✅ Re-aligned the Rust plan → `plans/RUST_DEAL_MICROSERVICE.md` (`ContractEngine`→`DealEngine`,
  proto `concertable.contract.v1`→`concertable.deal.v1`, `message Contract`→`Deal`, scope
  `contract:settle`→`deal:settle`, crate `concertable-contract`→`concertable-deal`, folder
  `deal-engine`). Discriminator string values kept.
### ✅ Phase 5 — SPA TypeScript *types* → `Deal` (DONE, commit `5bdcefad`)
`Contract`/`*Contract`/`ContractBase` → `Deal` family; `contractSummary`/`defaultContract`/
`CONTRACT_TYPE_LABELS` → `deal*`/`DEAL_TYPE_LABELS`; store/hook/prop actions → `*Deal*`; feature folders
`features/contracts/` → `features/deals/` (+ components, `app/shared` exports subpath). Wire kept (staging
step). Gate: all four web builds pass.

### ⬜ Phase 6 — the WIRE + full-stack sync (OUTSTANDING — the plan is NOT done without this)
Phases 1–5 renamed the *types* but deliberately kept the *wire* as a staging step. That leaves the
codebase **out of sync** — `DealType` serialising to `contractType`, the renamed `Contract` served at
`/agreement`. Per `plans/CLAUDE.md` ("never leave the codebase out of sync" + the grep-gate), the rename
is **not done** until the wire matches. Do it as ONE coordinated BE+SPA change (single deploy, not a
published-package boundary):
- **JSON keys:** opportunity's deal object `contract` → `deal`; enum key `contractType` → `dealType`
  (rename the kept `IDeal.ContractType` **property** → `DealType` — the C# "Color Color" same-name
  property/type pattern compiles — plus the matching `Opportunity*`/`Application*` DTO/Request/Response
  fields, on BE **and** SPA).
- **HTTP routes:** `{id}/agreement` → `{id}/contract`, `{id}/agreement/pdf` → `{id}/contract/pdf`
  (`ApplicationController`/`ConcertController`) + the SPA fetch calls + rename hook
  `useDownloadAgreement` → `useDownloadContract`.
- **HATEOAS link:** `ApplicationActions.Agreement` / `ConcertActions.Agreement` / wire `agreement` →
  `Contract` / `contract`; `ApplicationDto.AgreementId` → `ContractId`.
- **PDF / storage:** filename `booking-agreement-BA-{id}.pdf` → `contract-{id}.pdf`; PDF reference `BA-`
  → `C-`; blob path `agreements/` → `contracts/` (+ its integration-test assertion);
  `ContractEntity.DisplayName` `"Booking agreement"` → `"Contract"`.
- **`data-testid`s:** deal-terms ones `contract-*` → `deal-*` (incl. `opportunity-contract-type` →
  `opportunity-deal-type`); signed-doc download `download-agreement` → `download-contract`. Sync the E2E
  C# page objects (`MyVenuePage`/`MyConcertPage`) to match the FE.
- **UI copy:** deal-editing screens ("Contract type/Terms/Details", "View Contract") → "Deal …"; the
  signed-document download button "Booking agreement" → "Contract"; BE validator "sign the booking
  agreement" → "sign the contract".
- **Lowercase identifiers + comments (type-aware):** deal-typed locals/fields
  (`contractAccessor`:IDealAccessor, `contractModule`:IDealModule, `contract`:IDeal, `contractType`,
  `contractId`) → `deal*`; ex-agreement locals/comments (`agreement*`) → `contract*`. Gotchas hit last
  time: the `ContractBuilder` var collision (deal var vs the `contract` entity var — name them `deal`
  and `contract`), and the `Log.cs` `[LoggerMessage]` template/param must stay matched (`{ContractId}`
  ↔ `contractId`, which is the *contract* id there, not a deal id).
- **Migrations:** re-scaffold (Concert column `ContractType` → `DealType`) via `./initial-migrations.ps1`
  from `api/`, then revert the timestamp-only churn on unaffected modules (only Concert changes).

**Allowlist — kept on purpose (NOT residual):** the `$type` discriminator *values*
`"flatFee"/"doorSplit"/"versus"/"venueHire"` + enum member names (`FlatFee`…); the `.Contracts`
integration-event packages of every module; the word "Contract" where it now legitimately means the
signed contract (ex-BookingAgreement).

### Definition of done — the grep gate (run on a COHERENT tree, not mid-scramble)
Not done until, over the **whole repo** (both `api/` and `app/`, code + identifiers of every case +
comments + string literals + `data-testid`s + routes + docs):
- `grep -rniE "agreement"` → **0** (the ex-agreement is `Contract` now).
- `grep -rniE "contracttype"` → **0** (it's `DealType`).
- `grep -rniE "bookingagreement"` → **0**.
- no deal-typed identifier still named `contract*` (spot-check `contractAccessor`/`contractModule`/
  `contractId`/`contractType`).
…all outside the allowlist above. No "cosmetic tier". `git rm` this plan only when that gate passes.

**Verification gate:** solution build green · Concert integration green · all four web builds ·
`e2e-ui-debug` smoke (Phase 6 changes a covered user-facing flow, so E2E is now warranted).

## Not in scope
- No behaviour, workflow, settlement-math, or lifecycle changes — names only.
- The `$type` discriminator string values / enum member names — already name the deal structures
  correctly, so they stay (see the Phase 6 allowlist). Everything *else* on the wire is renamed in Phase 6.
- The Rust *implementation* — only its plan's naming is re-aligned (`plans/RUST_DEAL_MICROSERVICE.md`).
