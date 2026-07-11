# Rename: free "Contract" from the pricing model; put it on the real contract

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
- **Wire + DB string values do NOT change.** The JSON polymorphic discriminator strings
  (`"flatFee"`/`"doorSplit"`/`"versus"`/`"venueHire"`) and enum member names (`FlatFee`, …) stay
  identical — only the C# *type* names change. The SPA wire contract is untouched. TPH discriminator
  and table names change, but migrations are nuke-and-rescaffold (`./initial-migrations.ps1`,
  dev/E2E only, no prod data) per `api/CLAUDE.md`.

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
- `PayeeResolver` / `ArtistShareCalculator` / `*Calculator` already name what they do — leave them;
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

### Phase 1 — deal-terms family → `Deal`
Rename per the first table across: the whole `Concertable.B2B.Contract` module (5 projects + folders +
namespaces + `Concertable.slnx` / `Concertable.B2B.slnx` entries), the Concert-module consumers
(`DealAccessor`, resolvers, workflows, executors, renderers/fingerprint, mappers, `OpportunityEntity.DealId`),
Seed factories/seeders, and all tests. **Keep** every JSON/enum string value and the JSON
`[JsonDerivedType]` discriminators unchanged.
- Re-scaffold migrations (`./initial-migrations.ps1` from `api/`) — table/discriminator names change.
- **Gate:** `dotnet build api/Concertable.slnx` green · Concert + Deal (was Contract) unit + integration
  green via `integration-debug`.

### Phase 2 — the agreement → `Contract` (depends on Phase 1)
Now that `Contract*` is free, rename the second table. Re-scaffold migrations (table `BookingAgreements`
→ `Contracts`). If renaming the route/hook/filename, update the SPA in the same commit.
- **Gate:** solution build green · Concert integration green (`integration-debug`) · four web builds if
  the SPA was touched.

### Phase 3 — the leak → `LifecycleStateMachine`
Rename `ContractStateMachine` (+ tests). Pure Concert-domain rename, no model change.
- **Gate:** build green · Concert unit tests green.

### Phase 4 — docs + Rust-plan re-alignment (doc-only; can ride Phase 1–3 commits)
- Rewrite `api/Concertable.B2B/src/Modules/Contract/ARCHITECTURE.md` → the Deal module (also currently
  **stale** vs code: it references `IContractLoader`, `ConcertStage`, `Steps/` — the code now has
  `IDealAccessor`, `LifecycleState`, executors/`LifecycleTransitioner`). Move/rename the folder + the
  module's `LEGAL_REQUIREMENTS.md`.
- Update references in `api/docs/CODE_PATTERNS.md`, `api/docs/MICROSERVICES_ARCHITECTURE.md`,
  `api/Concertable.B2B/ARCHITECTURE.md`, `api/Concertable.B2B/TECH_DEBT.md`.
- **Re-align `plans/RUST_CONTRACT_MICROSERVICE.md`** (nothing built yet — doc-only): the future engine
  computes *deal* settlement, so `ContractEngine`→`DealEngine`, proto `concertable.contract.v1`→
  `concertable.deal.v1` / `message Contract`→`Deal` / `service ContractEngine`→`DealEngine`, auth scope
  `contract:settle`→`deal:settle`, crate `concertable-contract`→`concertable-deal`, folder
  `api/Concertable.Contract/contract-engine`→`api/Concertable.Deal/deal-engine`. Its `DealType`
  discriminator over the wire keeps the same string values.
- Optionally rename the SPA's own TS `ContractType`→`DealType` in
  `app/shared/src/features/concerts/**` (low-risk, independent).

### Final gate
Behaviour change is **zero** (pure rename), so build + unit + integration is the bar for Phases 1–3.
Run `e2e-ui-debug` **once** at the end as a smoke check because the rename touches the accept → agreement
→ PDF flow that E2E covers (per `plans/CLAUDE.md` "when to run E2E").

`git rm` this plan in the commit that lands the last phase.

## Not in scope
- No behaviour, workflow, settlement-math, or lifecycle changes — names only.
- No change to JSON wire strings, `[JsonDerivedType]` discriminators, or enum member names.
- The Rust *implementation* — only its plan's naming is re-aligned here.
