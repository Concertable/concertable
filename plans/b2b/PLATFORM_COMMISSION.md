# Platform fee — making the settlement transaction earn

> **Why now:** a revenue model was resolved **2026-07-01** (% on the settlement transaction) and **nothing
> implements it**. Verified 2026-07-24 by grep across `api/` and `app/`: zero hits for `PlatformFee`,
> `ApplicationFeeAmount`, `CommissionRate`, or any fee concept. `ISettlementAmountResolver` returns the
> deal's gross and that exact number is charged and paid out — Concertable takes **£0** on every booking.
> `LAUNCH_CHECKLIST.md:125` ("Application fees configured if taking % cut") is unticked and accurate.
>
> **The fee shape changed 2026-07-24 — a flat fee per settled contract, not a percentage** (~£10–15,
> config). The research behind that reversal is §7; the short version is that **no competitor takes a % of
> ticket/door sales** and the only two things anyone charges on are a flat per-gig fee or a % of the
> *agreed fee on their own rails*. Flat-per-contract is the one shape immune to that across all four
> contract types. ("Commission" survives only in the branch/file name — the thing is a flat fee.)
>
> **Ownership decided 2026-07-24 (supersedes the earlier B2B-resolver design):** the fee is a flat,
> platform-level **config value owned by the Payment service**, applied automatically at the charge — see
> §1.2 and §3. It is **not** resolved in B2B and passed across the gRPC boundary. That reversal is what
> collapses this from a publish-gated two-PR cut-over into a single PR (§3).
>
> **Tracker:** [`LAUNCH_PLAN.md`](./LAUNCH_PLAN.md) — this plan implements the locked *Monetization
> principle* and unblocks the tracked **pricing-transparency UI** item.

## Scope

**In:** the flat-fee config (owned by Payment), its snapshot at charge time, the arithmetic, the money
movement across both Stripe flows, and disclosure of the fee pre-commitment in both manager SPAs.

**Out (deliberate, captured as follow-ups):** Concertable's own **platform-fee VAT invoice** to the payer
(a normal invoice, *not* self-billed — must not be conflated with `InvoiceEntity`), **per-tenant fee
overrides**, a later **`%-of-agreed-fee` variant** for the escrowed contract types, and the **money
value-object / minor-unit** question (raw `decimal` today; see the tech-debt note below). All noted in §6.

> **This fee rides the Kernel `Money` type — it does not add a new `decimal`/`*100` site.** The money
> investigation **concluded 2026-07-24**: adopt a hand-rolled `Money(decimal, Currency)` value type in
> Kernel that owns the pence conversion + rounding + the `gross/fee/total` arithmetic + the minor-unit gRPC
> mapping (rationale in `api/Concertable.Shared/TECH_DEBT.md`). Its rollout is
> [`../MONEY_VALUE_TYPE.md`](../MONEY_VALUE_TYPE.md), a publish-first Kernel cut-over, and **this feature is
> its Phase 3**: `Money` lands first (Kernel), Payment adopts it, *then* this fee is built on top — so
> `gross + fee` is `Money.operator+` and the snapshot is `Money.ToMinorUnits()`, never a hand-written
> `+ fee` or `(long)(x*100)`. See MONEY_VALUE_TYPE §"Sequencing decision" for why Money-first, not
> fee-on-`decimal`-first.

## 1. The decisions this plan locks

### 1.1 Fee-on-top, borne by the payer

`LAUNCH_PLAN.md` §9 already fixes the shape: *"charge the venue the artist's share **+ our fee**, then
pay the artist"*. So:

```
charge(payer) = gross + fee        payee receives gross        platform keeps fee
```

The payee's contractually-agreed number is never silently shaved. **"Payer" is contract-type-dependent**,
and `SettlementPayeeResolver` already encodes the inverse: FlatFee / DoorSplit / Versus → artist is payee,
**venue pays**; VenueHire → venue is payee, **artist pays**. The fee follows the payer — no new branching.

### 1.2 `fee = flat amount per settled contract`, owned by Payment as config

**A single flat fee per settled contract, the same across all four contract types** — see §7 for the
research. Indicative v1 value: **~£10–15**. It lives in a bound `PlatformFeeOptions` **in the Payment
service** (platform-level config), **not** on `TenantEntity`, and **not** resolved in B2B.

**Why Payment owns it, not B2B (the 2026-07-24 reversal):** the fee is Concertable's cut of the settlement
transaction that *Payment itself processes and moves*. The earlier design put an `IPlatformFeeCalculator`
in B2B (mirroring `ISettlementAmountResolver`) and passed the resolved fee across gRPC — but its only
justification was keeping deal-context in B2B for a *future* `%-of-agreed-fee` variant (§6), and §7's whole
conclusion is that v1 is flat **precisely so it has zero deal-type dependence**. For a flat fee there is
nothing deal-specific to resolve, so Payment reading one config value is simpler and — critically — means
**nothing crosses the service boundary** (§3). If the `%-variant` is ever wanted, resolving it in B2B and
passing a fee/basis becomes an additive change then; we are not painted into a corner.

The fee is expressed in **pounds (`decimal`)** in config, consistent with the rest of the service layer.
Charged **once per contract, at the point money settles** (§2). A cancelled/never-settled booking is
never charged (§1.4).

### 1.3 The fee is snapshotted, never re-read

A later fee change must not retro-alter historic settlements. The fee charged is frozen at charge time
alongside the money. Concretely the fee is persisted on `EscrowEntity.PlatformFee` (escrow flow) and on
`SettlementTransactionEntity` (direct flow), never recomputed from live config when reading history. Payment
reads `PlatformFeeOptions` at the charge site and **writes the resolved number onto the record** — the
config is the live default, the persisted column is the snapshot.

### 1.4 A cancelled booking refunds the fee too

Refund returns the **full** charged amount including the fee — no service was delivered, so Concertable
keeps nothing. This is the behaviour `EscrowService` refund already has (`refundAmount = escrow.Amount`, a
`Money`) provided `Amount` remains the **total charged**; see §2.1.

## 2. The two money flows, and where the fee attaches

Escrow is charged at Accept, not at settlement — so the fee attaches at a different site per contract type.
Under §1.2 the fee value comes from `PlatformFeeOptions` **inside Payment** at each site; B2B is unchanged.

| Contract types | Flow | Charge site (Payment adds the configured fee) |
|---|---|---|
| VenueHire | Escrow deposit (fresh hold at Accept) | `EscrowService.DepositAsync` → hold `gross + fee` |
| FlatFee | Escrow capture (hold ring-fenced at apply, captured at Accept) | `ManagerPaymentService.CreateHoldSessionAsync` ring-fences `gross + fee`; `EscrowService.CaptureAsync` snapshots it |
| DoorSplit, Versus | Direct (`TransferData`) | `ManagerPaymentService.PayAsync` → charge `gross + fee`, transfer `gross` |

**The fee lives ONLY in `ManagerPaymentService`/`EscrowService`; the shared plumbing never hears the
word.** Those two services compute `gross + fee` (`Money.operator+`) and hand absolute amounts to a
dumb `PaymentManager`, which resolves accounts and calls Stripe. Settlement calls
`SettleAsync(chargeAmount: gross + fee, payeeAmount: gross)` — the retained cut is the difference; the
ticket path (no fee) calls `ChargeAsync(amount)` — one number, no fee concept. `StripeChargeOptions`
carries `Amount` (the charge) + a nullable `TransferAmount` (`null` ⇒ forward the whole charge), never
a `Fee`. `EscrowEntity`
stores `Amount` (a `Money`, = `gross + fee`) and `PlatformFee` (a `Money`) as EF `ComplexProperty`
pairs; `SettlementTransactionEntity` stores its `long` minor-unit `Amount` (= `(gross + fee).ToMinorUnits()`)
and a `long PlatformFee` snapshot (the `Transaction` hierarchy stays `long`, per Money Phase 2). No bare
`(long)(x*100)` and no hand-written `+ fee` anywhere — that's the whole reason Money is the prerequisite
(`../MONEY_VALUE_TYPE.md`).

### 2.1 Escrow flow — a bigger charge/hold, an unchanged transfer

No Stripe application-fee primitive is needed. Hold/charge `gross + fee`, then transfer only `gross`; the
remainder stays on the platform balance.

- `EscrowEntity` gains a **`PlatformFee`** `Money` (mapped as a second EF `ComplexProperty` alongside
  `Amount`), with `Amount` keeping its meaning as the **total charged** (`gross + fee`). `Create` takes
  `gross` + `platformFee` and sets `Amount = gross + platformFee` (`Money.operator+`) internally.
- `Release` transfers `Amount - PlatformFee` (`Money.operator-`).
- `Refund` continues to refund `Amount` in full (§1.4) — unchanged, correct by construction.

**Capture/hold-session note:** the FlatFee capture flow ring-fences `gross + fee` at *apply* time
(`CreateHoldSessionAsync`) and captures it at *accept* (`CaptureAsync` snapshots `PlatformFee`). Both read
the same flat config value; because the fee is a platform-level constant that changes rarely and only
forward, the apply→accept window is not a material snapshot risk (and the entity snapshot at capture is the
system of record). Logged as risk C3.

### 2.2 Direct flow — charge more, transfer the share

`ManagerPaymentService.PayAsync` charges `gross + fee` and transfers `gross` to the connected account via
`TransferData.Amount` on the PaymentIntent, and snapshots `PlatformFee` on `SettlementTransactionEntity`.
`ApplicationFeeAmount` is the alternative primitive; we use `TransferData.Amount` so both flows express the
same idea — *the platform keeps the difference*.

## 3. No published-package boundary is crossed — this is ONE PR

Because the fee is owned inside Payment (§1.2) and never travels from B2B, **no client interface, proto
message, or gRPC contract changes.** `IEscrowClient` / `IManagerPaymentClient` (in the published
`Concertable.Payment.Client` package) keep their exact signatures. B2B calls them unchanged.

This is the whole payoff of the ownership reversal: the earlier design changed `DepositAsync` / `CaptureAsync`
/ `PayAsync` signatures, which was a breaking cross-service change forcing an expand→publish→sync→contract
cut-over across **two** PRs with a platform-sync gate between them. Owning the fee in Payment deletes all of
that — **one PR, no publish gate, no consumer migration.**

The only cross-service touch is **disclosure** (Phase 2): B2B needs the fee value to show it pre-commitment.
B2B reads the same `PlatformFeeOptions` config section (a platform-level constant bound independently), or a
cheap read off the existing Payment client — decided in Phase 2, and far cheaper than the publish gate.

## 4. Phases

Both phases are independently shippable and end green. Gate for every phase:
`dotnet build api/Concertable.slnx` (0 errors) + affected unit + integration tests via `integration-debug`.
Phase 1 changes the model → ends with `./initial-migrations.ps1` from `api/`.

### Phase 1 — Payment charges the flat fee *(the phase where money changes)* ✅ DONE (local gate green; unmerged)

> Landed on `Feature/PlatformCommission`: build 0 errors, `Payment` unit 39/39, `B2B.Concert` integration
> 129/129, re-scaffold committed. Default fee **0** → behaviour-identical; E2E (no `[skip-e2e]`) runs in the
> merge queue. Phase 2 below is unstarted.

- `PlatformFeeOptions` (flat `decimal`, default **0**) bound from config in Payment, wrapped as
  `Money(value, Currency.Gbp)` at read time.
- `EscrowEntity.PlatformFee` (minor units) + `Create` takes it. **Re-scaffold.** `SettlementTransactionEntity`
  gains its `PlatformFee` snapshot column too (direct flow).
- `IPaymentManager` splits into `ChargeAsync` (one amount — ticket pass-through) and
  `SettleAsync(chargeAmount, payeeAmount)` (settlement), sharing one private account-resolution helper.
  `ChargeRequest`/`HoldRequest` are **deleted**; `StripeChargeOptions` swaps `Fee` for a nullable
  `TransferAmount`, `StripeHoldOptions` drops `Fee` (its `Amount` is the full hold total). Stripe intent
  client charges `Amount`, sets `TransferData.Amount = (TransferAmount ?? Amount).ToMinorUnits()`; fakes
  mirror it. The fee never appears below the two services (see §2).
- `EscrowService.ReleaseAsync` transfers `Amount - PlatformFee`. Refund untouched (§2.1).
- Apply the configured fee at the three sites in §2 (`DepositAsync`, `CreateHoldSessionAsync` + `CaptureAsync`,
  `PayAsync`), reading `IOptions<PlatformFeeOptions>`. **Customer flow is not touched** (not a settlement).
- Tests: charge/hold `gross + fee`, payee receives exactly `gross`, cancellation refunds the full charged
  amount, across **all four** contract types; a config fee of 0 is behaviour-identical to today.

**Default fee is 0**, so with no config set the behaviour is unchanged — but a configured fee flips
user-facing money movement on the payment + settlement flows, clearing the massive/risky bar in
[`plans/AGENTS.md`](../AGENTS.md). **Gate:** build + `Payment` unit + `B2B` integration, **plus the E2E
suites** (let the merge queue run them; don't duplicate locally). **No `[skip-e2e]`.**

### Phase 2 — Pricing transparency

The fee disclosed **before** the committing action: at Apply/Accept for the escrow types and on the
door-takings entry screen for revenue-share. Surface the breakdown (`gross`, `fee`, `total`) on the existing
DTOs and render it in both manager SPAs. B2B obtains the fee per §3 (shared config read).

Closes the `LAUNCH_PLAN.md` §5 *pricing transparency UI* row and the launch-ready checklist item.

**Gate:** build + affected integration + the web workspace builds; UI E2E via the merge queue.

## 5. Risks

| # | Risk | Mitigation |
|---|---|---|
| C2 | Fee double-counted (added at Accept *and* at settlement) for an escrow type | Escrow types charge **once**, at Accept (deposit or capture); `PayAsync` is only reached by DoorSplit/Versus. Fee applied per §2 table, one site per type. Integration tests assert the total charged per contract type. |
| C3 | Fee change retro-alters historic settlements / apply→accept drift | Snapshot at charge time on the entity/transaction (§1.3); history never recomputes from live config. Flat forward-only config makes the apply→accept window immaterial (§2.1). |
| C4 | Platform-fee VAT not accounted for (§6) | Out of scope by decision, but a **real HMRC obligation** once VAT-registered — tracked below. |

*(The earlier C1 — "Phase 2 merged before the Phase 1 platform-sync pin lands" — is **gone**: there is no
publish gate now, §3.)*

## 6. Follow-ups this plan deliberately does not build

- **Platform-fee VAT invoice** — the fee is a supply *by Concertable to the payer*, needing its own VAT
  treatment and its own invoice/numbering. It is **not** the self-billed `InvoiceEntity` and must not reuse
  it. Required before taking real money at launch.
- **`%-of-agreed-fee` variant for the escrowed types** — the one thing flat-per-contract gives up is upside
  on large fixed-fee deals. If ever wanted, resolve it in B2B (which owns deal context) and pass the
  resolved fee/basis to Payment — an additive change to the boundary at that point, *not* a rework of this
  design. Deliberately **not** v1 (§7).
- **Per-tenant fee overrides** — additive later; Payment config keyed by owner/tenant, or a resolver.
- **Money value type / minor-unit discipline** — the raw-`decimal` + scattered `(long)(x*100)` question.
  Investigation **closed 2026-07-24**: **adopt a hand-rolled Kernel `Money(decimal, Currency)` value type**
  (carries currency; owns the pence conversion + one rounding rule + the `gross/fee/total` arithmetic + the
  minor-unit gRPC mapping); revisit a library only if multi-region firms up. A publish-first Kernel cut-over
  — a sequencing task, not a blocker. Decision + resolves-when recorded in
  `api/Concertable.Shared/TECH_DEBT.md`. Not entangled with this feature (the flat fee is 2 dp,
  conversion-safe on `decimal`).
- **Fee terms in the T&Cs** — Swim-lane A; the solicitor needs the locked shape from §1.

## 7. Why a flat fee per contract — the pricing research (2026-07-24)

The shape moved twice in one day: **% of settlement → capped % → flat fee per contract.** Each step was
forced by a question, and the endpoint is the shape the whole market already uses.

### 7.1 The trigger: "what happens when they sell on DICE?"

The challenge: *we take a % of their ticket-sale earnings — but ticketing is external (DICE, Skiddle, …), so
how do we even see the number?* Premise correction: we never cut ticket sales. We cut the **settlement
transaction** — the venue→artist payment routed through our Stripe Connect — which exists regardless of who
sold the tickets. But the challenge exposed a real flaw: for DoorSplit/Versus, a **% is a slice of a
self-declared, externally-generated number we cannot verify.** That's the wrong thing to base revenue on.

### 7.2 Can we just get the sales number? (No.)

- **No normalized/standard ticketing API exists** — each is bespoke and requires the account owner to grant
  access.
- **DICE** — partner API exists but is gated behind a commercial partnership (MIO console), effectively
  closed to a small venue. ([DICE partner API](https://partners-endpoint.dice.fm/graphql/docs/index.html))
- **Eventbrite** — clean public OAuth; the one genuinely integratable case. ([Eventbrite API](https://www.eventbrite.com/platform/api))
- **Skiddle** — beta, non-commercial-only. ([Skiddle API](https://www.skiddle.com/api/))
- **Fatsoma** — no public sales API found.

**A guaranteed sales feed is impossible.** Third-party integration can only be an opt-in, post-launch trust
*enhancer*, never the *basis* of the model. The fee must not depend on a verified sales number at all.

### 7.3 The decisive finding: nobody charges a % of sales — the market avoids it on purpose

([platform comparison](https://gigxchange.app/blog/uk-live-music-booking-platforms-compared-2026),
[GigPig pricing](https://www.gigpig.uk/venues/pricing), [GigXchange](https://gigxchange.app/))

| Platform | Fee | Charged **on** |
|---|---|---|
| **GigPig** | £10/gig, or £150–£250/mo | Flat per booking / subscription |
| **GigXchange** | 0% or 5% (opt-in escrow) | The **agreed booking fee** through their Stripe |
| **Encore** | 20% | The **agreed performance fee** |
| **Alive Network** | ~20% | The **agreed performance fee** |
| **Last Minute Musicians** | Subscription | Nothing per booking |

Verbatim conclusion: *"No platform takes a percentage of ticket/door sales revenue. All commission
structures are calculated against the agreed performance/booking fee, not venue door revenue or ticket
sales."* Only two things anyone charges on: a **flat fee per gig/contract**, or a **% of the agreed fee that
flows through their own rails**. Both share one rule: **you only take a cut of money that moves through your
own payment system.**

### 7.4 Why flat-per-contract, not capped-%

- Our types split into "amount we control" (FlatFee/VenueHire, escrowed) and "amount we don't"
  (DoorSplit/Versus, the door). A **flat fee works identically on all four**, with no per-type branching and
  zero exposure to an unverifiable number anywhere. **This is also why Payment can own it as one config
  value (§1.2) — there is nothing deal-specific to resolve.**
- With a ~£15 cap, capped-% and flat **converge** above ~£300 anyway.
- Matches **GigPig's proven £10/gig** head-on, with a better story: we actually *settle the split and issue
  the contract*.
- Bills only on **completed bookings**, so it sells with zero traction (subscription stays rejected).

**What flat gives up** — upside on large fixed-fee deals; recoverable additively later via the
`%-of-agreed-fee` variant (§6).

### 7.5 What flat does *not* solve — and why that's acceptable

- **Declaration honesty.** The venue still self-reports the door take on DoorSplit/Versus. Flat-per-contract
  makes this a pure **venue↔artist** matter — it no longer touches our revenue. We're the settlement rail
  and the paper trail, not the auditor.
- **Disintermediation.** Two parties can always agree off-platform. The only defence is being worth using —
  escrow **plus** self-billed VAT invoicing, DAC7 compliance, the e-signed agreement, automated settlement.

> **Revisit at v1.1.** The flat amount is config (§1.2), so tuning it — or adding the `%-of-agreed-fee`
> variant (§6) — is cheap once a real booking-value distribution exists.
