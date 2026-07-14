# Code review — Refactor/DealRename

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `c93299cbf63753eeda45b63e0776c0cc1cec6e4a`  _(2026-07-14)_

> Range reviewed: `8b08d308..c93299cb` (9 commits, 356 files, +3648/-3978).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, and C# conventions.

## What this branch is

A purely-mechanical **dual rename** across 6 phases, disambiguating two concepts that were both called
"Contract":

1. **deal-terms family `Contract` → `Deal`** — the economic arrangement (`IContract`→`IDeal`,
   `ContractType`→`DealType`, `DoorSplitContract`→`DoorSplitDeal`, the whole `Modules/Contract` →
   `Modules/Deal`, and the FE `contracts` feature → `deals`).
2. **real signed contract `BookingAgreement` → `Contract`** — the binding artifact
   (`BookingAgreementEntity`→`ContractEntity`, `IBookingAgreementBuilder`→`IContractIssuer`,
   `BookingAgreementPdfService`→`ContractPdfService`, in the Concert module).
3. `ContractStateMachine` → `LifecycleStateMachine`.

## What was verified (all clean)

- **Correctness / cross-wiring** — the highest risk in a dual rename is a find/replace sending the
  wrong "Contract" the wrong way. Confirmed faithful everywhere: mappers (`OpportunityMapper`,
  `PaymentAmountMapper`, `ApplicationResponseMapper`), DI bindings (`DealAccessor` correctly bound to
  both `IDealAccessor` + `IDealResolver`; workflows keyed on `DealType`), and the Accept/Checkout
  orchestration keeps "resolve the *deal* for terms" and "issue the *contract*" on the correct sides.
  No smuggled logic/operator/constant/query changes.
- **Microservice isolation** (`api/ARCHITECTURE.md`) — rename is confined to B2B + `app/`;
  `Concertable.B2B.Deal.Contracts` references only `Concertable.Contracts` + `Kernel` packages (no
  consumer/cross-service escape). Customer/Search runtime untouched.
- **Module boundaries** (`MODULAR_MONOLITH_RULES.md`) — Concert reads deal-terms through the
  `IDealModule` facade + `Deal.Contracts`, never the Deal module's internals.
- **Seeding** (`SEEDING_CONVENTIONS.md`) — `SeedState`/`DealFactory` are 1:1 renames; seed amounts,
  entity IDs and index arithmetic preserved exactly. No new direct writes of reaction-owned data.
- **Migrations** — re-scaffolded, not additive: exactly one `InitialCreate` per context (Concert
  `20260714145531`, Deal `20260714010829`).
- **Persisted / wire formats stable** — `DealType` enum *member* names (`FlatFee`/`DoorSplit`/…) and
  the `IDeal` polymorphic `$type` discriminators are unchanged, so the terms fingerprint
  (`{deal.DealType}|…`) and existing e-signatures stay valid; FE `$type` literals match. The
  `/agreement`→`/contract` route rename and the `agreements/`→`contracts/` blob prefix are intentional
  (the point of the `BookingAgreement→Contract` rename) and their only client (the SPA) was updated in
  lockstep.

## Noted, not flagged

- `api/Concertable.B2B/src/Modules/Deal/ARCHITECTURE.md` still narrates the module as "the Contract
  module" and references a rejected `IContractLoader` design + old `ConcertStage` mechanics. This is a
  **documented exception**, not an oversight: the doc opens with an explicit "Staleness warning (read
  first)" marking the narrative as indicative-only pending a rewrite. Left as recorded debt.
  _(Resolved in the incremental range — see below.)_

## Incremental review — 2026-07-14

> Re-ran the full branch review at HEAD `c93299cb`. Range since the prior watermark
> (`ec5ff1cc..c93299cb`) is 3 commits: two doc-only (`c93299cb` Deal ARCHITECTURE rewrite,
> `8128525a` unit.ps1 help text) and one functional (`46188c82` CI path fix). Re-verified the whole
> branch through all five lenses; the earlier "all clean" holds. Two updates:

**Resolved since last review:** the stale `ARCHITECTURE.md` narrative (prior "Noted, not flagged") was
rewritten in `c93299cb`.

**Verified functional change:** `46188c82` repoints the unit-test matrix in `.github/workflows/test.yml:332`
(and `unit.ps1:13`) from the old `Modules/Contract/…/Concertable.B2B.Contract.UnitTests.csproj` to
`Modules/Deal/Tests/Concertable.B2B.Deal.UnitTests/Concertable.B2B.Deal.UnitTests.csproj` — the new
path exists (verified) and no `Modules/Contract/` path remains, so CI runs the renamed project.

**Wire consistency (re-confirmed BE↔FE, the one non-compiler-caught risk of a wire rename):** every
Phase-6 rename lands on both sides — JSON keys `deal` / action-link `contract` (BE `OpportunitySummaryResponse.Deal`,
`ApplicationActions.Contract` / `ConcertActions.Contract` ↔ FE `opportunity.deal`, `actions.contract`),
routes `{id}/contract[/pdf]` (BE `ApplicationController`/`ConcertController` ↔ FE `concertApi.ts`,
`useDownloadContract.ts`), and `data-testid`s (`opportunity-deal-type`, `download-contract`). The
`$type` discriminator values and enum member names are unchanged by design. No BE-emits-X / FE-expects-Y
mismatch. (`IDeal.DealType` serialises to `dealType`, used server-side only — the FE keys off `$type`, so
the field is simply unread on the wire, not a mismatch.)

## Findings (incremental)

- [x] **MB1 — LOW — module-boundary** — `api/Concertable.B2B/src/Modules/Deal/Concertable.B2B.Deal.Application/AssemblyInfo.cs:5-6`
  Dead cross-module internals grant + false comment: `[assembly: InternalsVisibleTo("Concertable.B2B.Concert.Infrastructure")]`
  with `// Ride-along (§3.3): Concert.Infrastructure applies DealEntityConfiguration on ConcertDbContext.`
  The ride-along no longer exists — `ConcertDbContext` has no Deal `DbSet`s and applies no Deal config;
  Deal owns its own `DealDbContext` + `DealConfigurationProvider` (`Deal.Infrastructure/Data/`). And
  `DealEntityConfiguration` lives in `Deal.Infrastructure` (internal), not `Deal.Application`, so this
  grant on `Deal.Application` could never expose it anyway. Net: `Deal.Application`'s internals are open
  to another module with no live reader — a latent boundary hole per `MODULAR_MONOLITH_RULES.md`
  (cross-module access only via `Contracts`/facades). **Pre-existing** — identical at merge-base (the
  rename only swapped `Contract`→`Deal` in the comment), so it's carried-forward debt the rename made
  provably stale, not newly introduced. Fix: delete both lines. (The `§` shows as mojibake — minor
  pre-existing encoding artifact in the same comment.)
