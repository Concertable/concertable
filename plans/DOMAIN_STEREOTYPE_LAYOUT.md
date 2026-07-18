# Refactor: domain stereotype layout (`Entities/` · `ValueObjects/` · `Enums/` · `ReadModels/`)

## What & why

Make every domain project organise its types **by stereotype**, uniformly, so you always know where a
type lives from its kind alone: entities in `Entities/`, value objects in `ValueObjects/`, enums in
`Enums/`, domain events in `Events/`, event-synced replicas in `ReadModels/`. Today it's inconsistent —
half the modules fold `Entities/`, half dump entities flat at the domain root; value objects are
scattered (Kernel flat, Tenant flat, Concert *inside* `Entities/`); enums are half-foldered, half-loose;
read models go by three different names.

This is the mainstream modern .NET convention (folder-by-stereotype within a bounded context, e.g. the
jasontaylordev Clean Architecture template). The stricter DDD-by-aggregate layout was considered and
**rejected** for this repo — it already splits one module per bounded context and already uses
`Entities/`, so stereotype folders are the consistent, least-surprising fit.

**Hard constraint: this is a pure move + namespace change. ZERO behaviour change.** No type renames, no
signature changes, no model changes — files move folder, their file-scoped namespace changes to match,
and every consumer's `using` is updated. Value objects stay `sealed record`s (records already give value
equality — no `ValueObject` base class; that idiom predates records).

## Prerequisite & branch

- **Runs AFTER the VAT / self-billed-invoicing PR (`Feature/VatAndSelfBilledInvoicing`) merges.** Do not
  bolt 100+ file moves onto that feature's diff — most of this code is already in `master`, so per the
  branch rules it belongs on its own branch.
- Branch: `Refactor/DomainStereotypeLayout` (match existing casing conventions; capitalised type prefix).

## Decided conventions (the rules to apply)

1. **Every domain project uses stereotype folders:** `Entities/`, `ValueObjects/`, `Enums/`, `Events/`,
   `ReadModels/`. Folder ⇒ namespace (folder-based namespaces are already the norm here), so moving a
   file changes its file-scoped namespace and every consumer needs its `using` updated.
2. **Value objects** = the `sealed record`s with no identity: `Address`, `DateRange` (Kernel),
   `RegisteredAddress`, `TaxCompliance` (Tenant), `ESignature`, `InvoiceParty`, `VatBreakdown` (Concert).
   Records stay as-is; no base class.
3. **Enums → `Enums/`, including in `*.Contracts`.** The tie-breaker: Contracts projects **already** fold
   `Events/`, so leaving enums loose there recreates the exact inconsistency this refactor removes. One
   project can't be half-foldered. So `Deal.Contracts/Enums/DealType.cs`, etc. — **uniformly**, even a
   module with a single enum (`Conversations.Contracts/Enums/MessageAction.cs`).
4. **`ReadModels/` is the single name** for event-synced replicas / projections. Rename Search's `Models/`
   + `Projections/` into it, and lift the flat B2B `*Projection` / `*Review` files into it.
5. **Uniformity even for single-member modules** — a module with only `UserEntity` still gets `Entities/`.
   Same payoff as the `Enums/` rule: one predictable answer everywhere.  ⚠️ **This is the one convention
   call to sanity-check before starting** — it's the "don't blanket-force, but do stay uniform" line.
6. **Cohesive concept-clusters are exempt.** A folder that groups a *concept* (not a stereotype) stays
   intact — don't shred it to satisfy a stereotype folder: `Concert.Domain/Lifecycle/` (state machine +
   its `Trigger`/`LifecycleState` enums live together), `Kernel/Identity/` (`Role`),
   `Kernel/Services/Geometry/` (`GeometryProviderType`), `Customer.User.Domain/Factories/`.
7. **Application-layer enums are out of scope.** `Enums/` foldering applies to Domain + Contracts only.
   Enums that live with their application concern (`Concert.Application/DTOs/ApplicationStatus`,
   `.../Workflow/Executors/SettlementOutcome`, `Search.Application/Params/Sort`, `HeaderType`) stay put —
   they're cohesively placed next to what uses them, same principle as rule 6.
8. **Events are already correct — no change.** Domain events in `Domain/Events/`, integration events in
   `Contracts/Events/`, uniform across every module. Leave them.
9. **Out of scope:** `Concertable.Auth` (`Data/Entities` + `Data/Events` — a different, IdentityServer-
   style architecture that's internally consistent). `Concertable.Messaging` plumbing is **optional**
   (shared infra) — listed at the end; include for full uniformity or skip.

## The work, by area

### Kernel — Shared Kernel (highest ripple — see Risk)
| Move | From | To (new namespace) |
|---|---|---|
| `Address`, `DateRange` (+ shape interfaces `IAddress`, `IHasDateRange`) | `Concertable.Kernel` (flat) | `Concertable.Kernel.ValueObjects` |
| `ErrorType` | already `Kernel/Enums/` ✓ | — |
| `Role`, `GeometryProviderType` | `Identity/`, `Services/Geometry/` | stay (cohesive) |

`Concertable.Kernel.Address` / `DateRange` are owned/complex types on entities in **Venue, Artist, User,
Concert, Ticket across B2B / Customer / Search** and referenced widely in code → every consumer `using`
updates; migrations re-scaffold.

### Shared Contracts
| Move | From | To |
|---|---|---|
| `Genre` | `Concertable.Contracts` (flat) | `Concertable.Contracts/Enums/` |

### B2B — Domain projects
| Module | Move |
|---|---|
| **Concert** | `ESignature`, `InvoiceParty`, `VatBreakdown`: `Entities/` → **`ValueObjects/`**. Entities stay in `Entities/`; `Events/`, `Lifecycle/`, `ReadModels/` already correct. |
| **Deal** | `Entities/` already correct. (Deal enums live in `Deal.Contracts` — see below.) |
| **Artist** | `ArtistEntity` → `Entities/`; `ArtistRatingProjection`, `ArtistReview` → `ReadModels/`. |
| **Venue** | `VenueEntity`, `VenueImageEntity` → `Entities/`; `VenueRatingProjection`, `VenueReview` → `ReadModels/`. |
| **Tenant** | `TenantEntity`, `TenantMembershipEntity` → `Entities/`; `RegisteredAddress`, `TaxCompliance` → `ValueObjects/`. |
| **User** | `UserEntity` → `Entities/` (single-entity module, rule 5). |
| **Conversations** | `MessageEntity` → `Entities/`. |

### B2B — Contracts projects (enums → `Enums/`)
`Deal.Contracts`: `DealType`, `PaymentMethod`. `Tenant.Contracts`: `TenantType`, `TenantRole`.
`Conversations.Contracts`: `MessageAction`.

### Customer — Domain projects
| Module | Move |
|---|---|
| **Preference** | `PreferenceEntity`, `GenrePreferenceEntity` → `Entities/`. |
| **User** | `UserEntity` → `Entities/`. (`Factories/UserFactory` stays — cohesive.) |
| Artist / Concert / Review / Ticket / Venue | `Entities/` (+ `Events/`, `ReadModels/`) already correct. |

### Search
`Models/` (`*ReadModel`, `*ReadModelGenre`) **+** `Projections/` (`*RatingProjection`) → consolidate into
a single **`ReadModels/`**.

### Payment
Entities (`EscrowEntity`, `PayoutAccountEntity`, `SettlementTransactionEntity`, `StripeEventEntity`,
`TicketTransactionEntity`, `TransactionEntity`, `VerifyTransactionEntity`) → `Entities/`. Enums
(`PayoutAccountStatus`, `TransactionStatus`, `TransactionType`) → `Enums/`. `Payment.Contracts`:
`EscrowStatus` → `Enums/`. `Payment.Client`: `PayoutAccountStatus` → `Enums/`.

### Messaging (optional — shared infra plumbing)
`InboxMessageEntity`, `OutboxMessageEntity` → `Entities/`; `MessageKind`, `OutboxStatus` → `Enums/`.

## Phasing (each phase independently shippable + green)

Domain-project namespaces are **service-internal** (cross-service contact is Contracts-only), so each
service's own Domain tidy affects only that service — clean single-PR phases. The **cross-package** moves
(Kernel, `*.Contracts`, shared `Contracts`) are the ones with reach — do those first so consumer `using`s
are touched once, not twice.

- **Phase 1 — Shared foundations.** Kernel VOs → `ValueObjects/`; shared `Contracts/Genre` → `Enums/`.
  ⚠️ **RESOLVED as package-refs → a THREE-merge cut-over (see Risk below). Originally scoped as two;
  execution found a third cross-service package — `Concertable.B2B.Seed.Contracts` — that also exposes
  the moved types, adding one more publish hop.**
  - **Merge 1 (shared expand) — DONE. Landed as PR #126 (admin-merge, squash `df7ed06b`).** Moved the 5
    files + updated the 10 source-built (ProjectReference) shared-area consumers. Only red was the 2
    boundary-exempt harnesses (structural, unfixable pre-publish). Merged → `Kernel`/`Contracts`@0.590
    republished with the new namespaces.
  - **Merge 2 (platform-sync + consumer migration) — SOURCE DONE, on `chore/platform-sync-0.1.0-alpha.0.590`
    (PR #127).** Bot bumped `ConcertablePlatformVersion` → 0.590; migrated **every** package consumer's
    usings — 56 projects' `GlobalUsings` gained `Kernel.ValueObjects`/`Contracts.Enums`, ~30 file-local
    stragglers (Contracts events, Seed catalogs/specs/factories, the 2 harnesses, unit tests) fixed
    replace-vs-add. **B2B builds green.** Grep gate clean in hand-written source (only migration snapshots
    remain — the allowlist).
    - ⚠️ **Customer + Search are EXPECTED-RED here (22 `CS7069`).** Their deployable services depend
      (via seed infra) on the **`Concertable.B2B.Seed.Contracts` package**, which — published at Merge 1
      against the old platform — still exposes `Concertable.Contracts.Genre` / `Concertable.Kernel.DateRange`.
      Not a source bug (the boundary guardrail forbids source-refs; it's not in the `UseLocalCore` swap
      list). Clears only when `B2B.Seed.Contracts` republishes — i.e. when Merge 2 itself lands. Same
      expand/contract shape as Merge 1's harnesses, one package deeper.
    - Publish still works: the red seed-infra projects are non-packable and no packable project depends
      on them, so `dotnet pack` republishes `B2B.Seed.Contracts`@new fine (verified). Admin-merge → publish.
    - **Re-scaffold deferred to Merge 3** — Customer/Search can't build (so can't re-scaffold) until their
      `B2B.Seed.Contracts` pin advances; do the whole re-scaffold in one pass when the tree is green.
  - **Merge 3 (final platform-sync) — NOT done.** Next `platform-sync` bump pulls in `B2B.Seed.Contracts`@new
    → Customer/Search compile clean. Then **re-scaffold all modules** (`./initial-migrations.ps1`) and the
    **old-namespace grep gate reaches zero**. Plan stays open until Merge 3 lands and the tree is in sync.
- **Phase 2 — B2B.** All B2B module Domain reorgs + B2B `*.Contracts` enum moves. Re-scaffold. B2B build +
  unit + integration green.
- **Phase 3 — Customer.** Customer Domain reorgs + any Customer `*.Contracts` enums. Re-scaffold. Green.
- **Phase 4 — Search + Payment.** Search `ReadModels/` consolidation; Payment Domain/Contracts/Client
  moves. Re-scaffold. Green.
- **Phase 5 (optional) — Messaging.** As above.

## Risk — shared-package namespace changes may be a breaking package cut-over

`plans/CLAUDE.md` "Boundary-blocked refactors": B2B/Customer compile against **published** `Concertable.*`
packages, not source. A namespace move has **no back-compat shim** (unlike an additive method) — a type is
in exactly one namespace, and consumers can't add the new `using` until the republished package carries the
type in its new namespace. So if Kernel and the `*.Contracts` projects are consumed by **package** ref,
Phase 1 (and each `*.Contracts` enum move consumed cross-service) is a **republish-then-consumers**
cut-over across two merges — not one atomic PR.

**Verify first at execution time:** are Kernel / `*.Contracts` referenced by *project* or *published
package*? Project refs ⇒ atomic single-PR moves are fine. Package refs ⇒ sequence the shared-package
moves expand/contract (republish first), or keep the plan open across the back-to-back merges until the
tree is in sync (`plans/CLAUDE.md` "Never leave the codebase out of sync").

**RESOLVED (execution, 2026-07-18): PACKAGE-referenced.** Every service (B2B/Customer/Search/Payment)
consumes `Concertable.Kernel` + `Concertable.Contracts` as feed `PackageReference`s pinned to
`ConcertablePlatformVersion` (each service folder's own `Directory.Packages.props`); only the shared area
(`Concertable.Shared/*`, `DataAccess`, `Messaging`) uses `ProjectReference`. Confirmed by
`api/ARCHITECTURE.md` "Cross-service contract distribution" + "the publish→sync loop". So Phase 1 is the
two-merge cut-over above, **not** one atomic PR. Proof it can't be atomic: with the move applied to
shared source only, `Concertable.slnx` builds green **except** the 2 harnesses that ProjectReference
shared source *and* reference package-built module assemblies — `CS7069: type 'Genre'/'DateRange' claims
it is defined in 'Concertable.Contracts'/'Concertable.Kernel' but could not be found`. That skew clears
only when the package republishes with the new namespace (Merge 2). `UseLocalCore` does **not** rescue
it — `Concertable.Contracts` isn't in the churny-core swap list, so services still bind it as a package.

## Verification gate (every phase)

- `dotnet build api/Concertable.slnx` green (0 errors).
- Affected modules' unit + integration tests via the `integration-debug` skill (a red run → drive that
  skill to green, don't report red).
- Re-scaffold, don't hand-edit: `./initial-migrations.ps1` from `api/` (folder moves change entity/owned-
  type CLR namespaces → the model snapshots reference them → nuke & re-scaffold every module's
  `InitialCreate`). No model change, so it's a free mechanical re-scaffold.
- **Grep gate (definition of done per phase):** for each moved type, `grep -rniE` the **old** namespace
  over the repo returns zero (allowlist only for the generated migration snapshots between the move and
  the re-scaffold). Don't decide by hand which occurrences count.
- **E2E:** pure move/namespace refactor = zero behaviour change, so per-phase gate is build + unit +
  integration. But the sweep is broad and cross-cutting, so run the **UI E2E regress once on the final
  phase** as a safety net (via `e2e-ui-debug` / `e2e-ui-regress`, Docker-health-gated).

## Done when

Every domain project (and in-scope Contracts project) exposes types under the stereotype folders above;
Events unchanged; cohesive clusters intact; every old namespace greps to zero; full build + affected
tests green; final-phase E2E green. Then `git rm` this plan in the commit that lands the last phase.
