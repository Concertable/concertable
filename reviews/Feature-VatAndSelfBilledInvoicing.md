# Code review — Feature/VatAndSelfBilledInvoicing

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `4830f832b34b0cb647f732d07f0d5e9f3df98176`  _(2026-07-18)_

> Range reviewed: `df651abb..4830f832` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

Two low-severity C# convention deviations. No correctness, microservice-isolation, module-boundary, or
seeding issues found — the settlement/invoice design holds up (single `SaveChanges` = atomic mint,
unique index on `Invoices.BookingId` blocks double-mint, gap-free numbering via `RowVersion` + the
sweep's outer retry, payment idempotent on `bookingId`; the Phase-1 VAT-check removal is safe because
`AddFluentValidationAutoValidation()` + the new `includeInternalTypes: true` registration now enforce
the format in the write pipeline pre-action).

- [x] **CV1 — LOW — C# conventions** — `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Application/Tax/VatPolicy.cs:3`
  _Fixed: converted to an explicit ctor + `private readonly IVatCalculator calculator` field._
  `internal sealed class VatPolicy(IVatCalculator calculator) : IVatPolicy` uses a **primary
  constructor to capture state** — `calculator` is read in `Apply()`. `CODE_CONVENTIONS.md` ("No
  primary constructors for captured state"): *"Captured constructor parameters — anything read by a
  method or property — must be explicit `private readonly` fields assigned via `this.field = param`,
  never primary-constructor captures. This covers services, repositories, handlers, and validators."*
  `VatPolicy` is a captured-state service, so it needs an explicit ctor + `private readonly
  IVatCalculator calculator;`. (The plan text spelled it this way too, but `CODE_CONVENTIONS.md` is
  the authority, not the plan.) Every other new service/strategy in the branch already uses the
  explicit-ctor form — this is the one that slipped.

- [-] **CV2 — LOW (optional/judgment) — code patterns (naming)** — DEFERRED — `api/Concertable.B2B/src/Modules/Concert/Concertable.B2B.Concert.Infrastructure/Services/Settlement/FlatFeeSettlementAmount.cs:6` (and `VenueHireSettlementAmount`, `RevenueShareSettlementAmount`)
  _Deferred (author's call): rename the three strategies to `{Key}SettlementAmountResolver` for pattern parity, or keep the current noun names as-is. Decision needed from you; code untouched._
  The keyed-strategy family is built correctly (facade `SettlementAmountResolver` + `FrozenDictionary`
  + concrete strategies + `ISettlementAmountResolver`), but the **strategy classes drop the role
  suffix**: they're `FlatFeeSettlementAmount` / `VenueHireSettlementAmount` /
  `RevenueShareSettlementAmount` while the facade is `SettlementAmountResolver` and the method is
  `ResolveGrossAsync`. `CODE_PATTERNS.md` ("Keyed strategy resolver") names the three roles as
  interface `IX` / strategies `{Key}X` / facade `X` — i.e. strategies should share the facade's stem
  (`FlatFeeSettlementAmountResolver`, as `DealMapper`→`FlatFeeDealMapper`). Borderline — the current
  names read fine as nouns, so treat this as an optional consistency tidy, not a required change.
