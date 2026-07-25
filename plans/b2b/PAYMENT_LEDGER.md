# Payment ledger — money-of-record moves out of the operational entities

> **What this is:** the plan to make a **double-entry ledger the system of record for money** in the
> Payment service, so a settlement's financial truth (what the platform earned, what a payee is owed,
> what a payer was charged) lives as **balanced ledger entries in one place** — not as `PlatformFee` /
> `Amount` columns copied onto every operational entity (`EscrowEntity`, `SettlementTransactionEntity`,
> and every future money-flow type).
>
> **Why it exists:** commissioned off the platform-fee design discussion. Full evidence + verdict:
> [`PLATFORM_FEE_STORAGE_INVESTIGATION.md`](./PLATFORM_FEE_STORAGE_INVESTIGATION.md). The one-line
> conclusion that spawned this plan: *a dedicated append-only double-entry ledger is the genuinely
> correct long-term shape, and it is precisely what dissolves the "fee column smeared across every
> entity" smell* — Stripe itself is a ledger and models fees exactly this way (`balance_transaction` +
> `fee_details[]`, never a `fee` column per product object).
>
> **Prerequisite (met at implementation start):** `PLATFORM_COMMISSION.md` Phase 1 is **merged**
> (Payment charges the flat fee; `EscrowEntity.PlatformFee` + `SettlementTransactionEntity.PlatformFee`
> snapshots exist). This plan is what those columns eventually collapse *into*.

## 0. Scope & sequencing decision — read this before writing any code

**This was prioritised ahead of `PLATFORM_COMMISSION` Phase 2 (pricing transparency) by explicit
decision.** That decision overrides the investigation's *timing* call, and the plan carries the
investigation's caveats faithfully so implementation starts clear-eyed rather than cheer-led:

1. **The investigation assessed the internal ledger as the correct north-star but _premature now_** — for
   two honest reasons that do not go away just because we start early:
   - **Stripe already _is_ an authoritative fee ledger.** Every fund movement is one
     `balance_transaction` carrying a `fee_details[]` breakdown (`application_fee`, `net = amount − fee`);
     the Connect platform cut is an `application_fee` on that unified ledger. For "what did we earn on
     booking X / in period Y", Stripe can already answer it, reconcilably, today.
   - **It is a Payment-service-wide investment** (double-entry posting, balance derivation,
     correction-entry discipline, Stripe reconciliation, history) an order of magnitude larger than the
     fee feature it tidies up.
2. **Therefore the plan is phased so early phases deliver value and de-risk the big investment** — it does
   **not** big-bang an internal general ledger. The pragmatic first increment the investigation points at
   (**reconcile/derive from Stripe's ledger** rather than immediately building our own GL as source of
   truth) is folded in as an explicit validation seam (Phase 3), not skipped.
3. **The one thing that stays true regardless:** the resolved money **snapshot never disappears** — it
   moves from a column-per-operational-entity to an **immutable ledger entry**. We are relocating the
   snapshot into a ledger, never replacing it with a recompute-on-read. (That anti-pattern is the one
   red line in the investigation.)

**If, on starting, the Stripe-reconciliation view (Phase 3) turns out to answer every real reporting/
audit need on its own, stop there and do not build Phases 4–5** — that is the investigation's actual
recommendation surfacing as a live checkpoint, not a failure. Log the decision and close the plan.

## 1. The problem, concretely

Today the money-of-record is **denormalised onto operational entities**, one fee/amount pair per type:

- `EscrowEntity` — `Money Amount` (= gross + fee, the real Stripe hold) + `Money PlatformFee`
  (`EscrowEntityConfiguration` maps both as `ComplexProperty` pairs).
- `SettlementTransactionEntity : TransactionEntity` — `long Amount` (minor units, = gross + fee) +
  `long PlatformFee` (the `Transaction` TPH hierarchy stays `long` per Money Phase 2).

Every **new** money-flow type would repeat the pattern — another entity, another `PlatformFee` column,
another place "what did the platform earn" has to be summed from. The revenue truth has no single home;
it is scattered across the operational state machines that happen to move the money.

**Target:** the operational entities keep only what they operationally need (an escrow genuinely holds
`gross + fee` in Stripe — `Amount` is a real operational fact and **stays**); the **financial truth**
(fee-as-revenue, payee-owed, payer-charged) becomes **balanced entries in a ledger**, written once per
settlement, queryable as one ledger.

## 2. The target shape (evidence-backed — see the investigation)

A minimal double-entry ledger, append-only and immutable, in the Payment service:

- **`LedgerAccountEntity`** — the chart of accounts. Minimal viable chart for a Connect platform:
  - `PlatformRevenue` (a single platform account — fee income credits here)
  - `StripeClearing` (a single platform account — mirrors funds held/moving on the Stripe balance)
  - per-owner **payable** accounts (`Payable:{ownerId}`) — what a connected payee is owed / was paid
  - per-owner **receivable/clearing** accounts (`Receivable:{ownerId}`) — what a payer owes / paid
  - (accounts are created on demand as a reaction to the first posting that needs them — **never
    directly seeded**; see §5 seeding note.)
- **`LedgerTransactionEntity`** — one per financial event (a settlement, a release, a refund). Immutable
  once posted; a correction is a **new** opposing transaction, never an edit (Modern Treasury discipline).
  Carries the external correlation (`BookingId`, the Stripe `PaymentIntentId`/`ChargeId`).
- **`LedgerEntryEntity`** — the append-only legs. Each entry: `AccountId`, signed `long` minor-unit
  `Amount` (+ `Currency`), `Direction` (Debit/Credit), FK to its `LedgerTransactionEntity`. **Invariant:
  the entries of one transaction sum to zero** — enforced in the domain factory (balance-or-throw), not
  by convention.

Example posting — a **direct settlement** of `gross` with `fee` (payer pays `gross + fee`):

```
Dr  Receivable:{payer}     (gross + fee)     // payer charged the total
  Cr  Payable:{payee}        gross            // payee owed their share
  Cr  PlatformRevenue        fee              // platform keeps the fee
```

Release/refund/escrow-capture each get their analogous balanced posting (§4).

## 3. Operational-vs-ledger split — what stays, what moves

| Fact | Where it lives at the end |
|---|---|
| Escrow **hold total** (`gross + fee` actually held in Stripe) | **Stays** `EscrowEntity.Amount` — a real operational Stripe fact, not revenue. |
| Escrow status / Stripe ids / `ChargeId` / `TransferId` | **Stays** on `EscrowEntity` — operational state machine. |
| **Platform fee as revenue** | **Moves** → `Cr PlatformRevenue` ledger entry. `EscrowEntity.PlatformFee` / `SettlementTransactionEntity.PlatformFee` columns are **removed** at Phase 5. |
| **Payee owed / paid** | **Moves** → `Payable:{payee}` entries. |
| **Payer charged** | **Moves** → `Receivable:{payer}` entries. |
| "What did the platform earn (ever / in period)" | Ledger query (sum `PlatformRevenue`), one place — not a scan of every entity's `PlatformFee`. |

## 4. Phases (each independently shippable, each ends green)

Verification gate for **every** phase (per [`plans/CLAUDE.md`](../CLAUDE.md)): `dotnet build
api/Concertable.slnx` (0 errors) + Payment unit + affected integration via `integration-debug`. Phases
that change the model end with `./initial-migrations.ps1` from `api/` (re-scaffold, never additive).
**No published-package boundary is crossed** — the ledger is entirely internal to Payment; `IEscrowClient`
/ `IManagerPaymentClient` signatures are untouched, so this is a sequence of ordinary single-PRs, no
platform-sync break (same payoff as `PLATFORM_COMMISSION` §3).

### Phase 1 — Ledger schema + posting engine *(foundational, zero behaviour change)* — ✅ DONE
- ✅ New entities `LedgerAccountEntity` / `LedgerTransactionEntity` / `LedgerEntryEntity` in
  `Concertable.Payment.Domain`; EF configs + a `Ledger`-prefixed table set (extended `Schema.Tables`);
  registered on `PaymentDbContext`. Amounts are `long` minor units + `Currency`. Entries store a
  **signed** minor-unit `Amount` (Debit `+`, Credit `−`) alongside `Direction`; the balance invariant is
  `Sum(Amount) == 0`.
- ✅ Domain posting factory `LedgerTransactionEntity.Post(...)` — validates ≥2 legs, single currency,
  positive magnitudes, **balance-or-throw** by construction; `Entries` exposed as a defensive
  `AsReadOnly()` (no post-construction mutation). Façade `ILedger`/`LedgerPostingService` posts one
  balanced transaction and **resolves/creates accounts on demand** (per-call cache + a **non-filtered**
  unique index on `(Type, OwnerId, Currency)` so the null-owner platform accounts fail closed on races).
- ✅ **Nothing calls it yet** → zero behaviour change, nothing seeded.
- ✅ **Gate met:** `dotnet build api/Concertable.slnx` 0 errors + 54 Payment unit tests green
  (balance-or-throw, immutability, account-on-demand). Model re-scaffolded via `./initial-migrations.ps1`.
  **`[skip-e2e]`** on the PR (nothing runtime exercises it).

### Phase 2 — Post to the ledger from every money flow *(expand: write alongside)* — ✅ DONE
- ✅ `LedgerPostings` (Infrastructure) builds the balanced posting per flow; `ILedger` injected into
  `ManagerPaymentService` / `EscrowService` posts it after each money-confirmed operational save. The
  fee leg is **omitted when the fee is 0** (default fee is £0 → a zero-magnitude leg would trip the
  domain's positive-magnitude guard). `StripeClearing` carries the escrow dwell so each transaction
  balances on its own and the lifecycle nets to zero:
  - ✅ **Direct** — `PayAsync` (§2 posting: `Dr Receivable / Cr Payable + Cr PlatformRevenue`).
  - ✅ **Escrow deposit / capture** — `DepositAsync` / `CaptureAsync`: `Dr Receivable / Cr StripeClearing`
    (payer's total captured into clearing).
  - ✅ **Escrow release** — `ReleaseAsync`: `Dr StripeClearing / Cr Payable + Cr PlatformRevenue` (fee
    recognised as the transfer settles the payee).
  - ✅ **Escrow refund** — `RefundAsync`: opposing transaction, branched on `escrow.TransferId` (funds in
    clearing vs already transferred), never an edit (§1.4 full refund incl. fee).
- ✅ **Additive only** — operational columns untouched, real money movement unchanged, **no model
  change** (nothing reads the ledger yet). Nothing seeded.
- ✅ **Gate met:** `dotnet build api/Concertable.slnx` 0 errors + 62 Payment unit tests green — pure
  per-flow balance+reconcile (`LedgerPostingsTests`, incl. fee==0 collapse) plus service-site wiring
  (`EscrowServiceTests` / `ManagerPaymentServiceTests` capture the posted `LedgerPosting` and assert it
  balances and reconciles to charge/share/fee). Merge queue runs E2E (behaviour-adjacent, **no token**).

**Deferred to Phase 3 (the async-completion + read/reconcile increment), not gaps in Phase 2:**
- **Async-completion posting.** Direct `PayAsync` and escrow `DepositAsync` post only on their
  synchronous `!RequiresAction` branch — the 3DS/webhook-confirmed completion (`SettlementTransactionHandler`
  → `TransactionService.CompleteAsync`; `EscrowConfirmedHandler`) does **not** yet post. Exactly-once
  across the sync post and the idempotent webhook completion needs a single "on-transition-to-complete"
  post site; folded into Phase 3 (which owns the completion/reconcile seam). Capture/Release/Refund have
  no async branch and always post.
- **E2E-level reconciliation.** Payment has no in-process integration harness (real flows run only in the
  Aspire API E2E). Reconciliation is asserted at the unit tier now; the DB-side assertion (helpers already
  added to `PaymentDb`: `GetLedgerTransactionCountAsync` / `GetLedgerSignedSumAsync` /
  `GetLedgerPlatformRevenueAsync`) lands in Phase 3 once async-completion posting closes the timing gap.
- **Ticket pass-through.** Deferred: a ticket has no `BookingId`, and the Phase-1 ledger correlation is a
  non-nullable `int BookingId`. Posting tickets cleanly needs the correlation generalised (nullable
  booking + `PaymentIntentId` as the real key) — a Phase-1 schema tweak, kept out of this no-model-change
  phase.

### Phase 3 — Read the ledger + reconcile against Stripe *(make it trustworthy)*
- **First close the write gap from Phase 2:** post at the async settlement-completion sites (settlement
  webhook completion; `EscrowConfirmedHandler`) so 3DS/webhook-confirmed flows are on the ledger too —
  exactly-once via a single on-transition-to-complete post site. Then turn on the E2E DB reconciliation
  assertions (helpers already in `PaymentDb`).
- Point "platform earned" / "owed to X" reads at the ledger.
- **Reconciliation seam:** validate the internal ledger against Stripe's `balance_transaction` /
  `application_fee` (the authoritative external ledger) — a test + a runtime check that internal
  `PlatformRevenue` matches Stripe's collected application fees for a period. This is the investigation's
  "reconcile-from-Stripe-first" increment, landed as the correctness gate on our own ledger.
- **CHECKPOINT (from §0.3):** if this reconciliation view answers every real reporting/audit need,
  **stop here** — do not build Phases 4–5; the columns can stay. Record the decision, close the plan.
- **Gate:** build + integration + reconciliation tests.

### Phase 4 — Operational entities reference the ledger *(link)*
- `EscrowEntity` / `SettlementTransactionEntity` gain a FK/correlation to their `LedgerTransactionEntity`
  (or the ledger transaction carries the correlation keys — decide at implementation; avoid a bidirectional
  cycle). No columns removed yet.
- **Gate:** build + unit + integration.

### Phase 5 — Contract: remove the smeared columns *(the payoff — behaviour-affecting model change)*
- Remove `PlatformFee` from `SettlementTransactionEntity` and `EscrowEntity` (revenue truth now lives in
  the ledger). **Keep `EscrowEntity.Amount`** (real Stripe hold). Update `EscrowService.ReleaseAsync`'s
  transfer math to derive the payee amount from the ledger/`Amount` rather than the removed column.
- Re-scaffold: `./initial-migrations.ps1`.
- **Gate:** build + Payment unit + integration + **E2E via the merge queue** (model + money-read change
  on covered flows — no `[skip-e2e]`).

## 5. Conventions this plan must obey (don't rediscover them)

- **Seeding** ([`api/docs/SEEDING_CONVENTIONS.md`](../../api/docs/SEEDING_CONVENTIONS.md)) — ledger
  entries/accounts are **written only as a reaction to a settlement**, so they are **never directly
  seeded**. If seed/E2E data needs ledger rows, drive the settlement path that posts them; do not
  `AddRange` entries in a seeder. (Same rule that governs `PayoutAccount`.)
- **Money** ([`../MONEY_VALUE_TYPE.md`](../MONEY_VALUE_TYPE.md)) — ledger amounts are `long` minor units +
  `Currency` at rest (matching the `Transaction` hierarchy); arithmetic goes through the Kernel `Money`
  type (`ToMinorUnits()`, `operator +/-`), never a hand-rolled `(long)(x*100)` or bare `+ fee`.
- **Keyed strategy resolver** ([`api/docs/CODE_PATTERNS.md`](../../api/docs/CODE_PATTERNS.md)) — if the
  per-flow posting shape varies by a closed key (contract type / transaction type), resolve it with a
  keyed resolver façade, **not** a `switch` inside `EscrowService` / `ManagerPaymentService`.
- **C# conventions** ([`api/docs/CODE_CONVENTIONS.md`](../../api/docs/CODE_CONVENTIONS.md)) — `this.`
  fields (no `_`), source-generated `Log.cs` (no inline `logger.LogX`), `Schema.Tables` constants for
  table names, no design-narration comments.
- **Microservice boundary** ([`api/ARCHITECTURE.md`](../../api/ARCHITECTURE.md)) — the ledger is Payment's
  own money model; it is **not** a shared/Kernel concept and **not** exposed across a service boundary.
  Nothing in `Concertable.Payment.Client` / `.Contracts` changes shape.

## 6. Relationship to the other open threads

- **`PLATFORM_COMMISSION.md` §6 hybrid rate-card** (config *inputs*: per-tenant overrides, `%-variant`)
  is the **orthogonal, config-side** evolution — versioned immutable pricing policy referenced by id. It
  is independent of this ledger (outputs) and can land before, after, or never relative to it. Don't
  entangle the two: the rate-card answers *"which rule produced this fee?"*; the ledger answers *"where
  does the resulting money live?"*.
- **`PLATFORM_COMMISSION.md` Phase 2 (pricing transparency)** is deferred behind this plan by the §0
  decision — resume it after this plan reaches its checkpoint/close.

## 7. Risks

| # | Risk | Mitigation |
|---|---|---|
| L1 | Double-entry invariant violated (unbalanced posting persists) | Balance-or-throw in the domain posting factory (Phase 1) — by construction, unit-tested; never a runtime "sum then hope". |
| L2 | Ledger diverges from Stripe's actual money movement | Phase 3 reconciliation against `balance_transaction` / `application_fee` is the gate; a divergence fails the check, doesn't rot silently. |
| L3 | Over-building past need (the investigation's premature-warning realised) | §0.3 hard checkpoint after Phase 3 — stop if Stripe-reconciliation suffices; Phases 4–5 are opt-in, not automatic. |
| L4 | Removing `PlatformFee` columns (Phase 5) breaks a read path | Expand→link→contract ordering: the ledger is authoritative and reconciled (Ph 3) and linked (Ph 4) **before** any column is removed; re-scaffold + E2E gate the contract phase. |
| L5 | A correction is done by editing a posted transaction | Immutable-by-construction: no update path on `LedgerTransactionEntity`/`Entry`; corrections are new opposing transactions (enforced in the repository/domain). |

## 8. Definition of done

The plan is complete when **either**: (a) the §0.3 checkpoint fired at Phase 3 and Stripe-reconciliation
was judged sufficient (Phases 4–5 dropped by decision), **or** (b) Phase 5 landed — `PlatformFee` no
longer exists as a column on any operational entity, the platform's revenue is one ledger query, and the
E2E suite is green. In both cases: `git rm` this plan in the completing commit, and tick the
`PLATFORM_COMMISSION.md` §6 / `LAUNCH_PLAN.md` lines the ledger touched.
