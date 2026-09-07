# Organiser direct offers

> **Next steps live in @plans/launch/BOOKING_ENTRY_DIRECT_OFFERS_PROGRESS.md → `## Next Steps`.**

Baseline: `origin/main` at `516f4cc25`, the merge of PR #633. Judged against the capability model decided
in the workflow-divergence research and the standing rule it hands to
`api/Concertable.B2B/CODE_PATTERNS.md`.

## 1. The question this plan answers

Venues create opportunities and artists apply. An organiser must also be able to select an artist and
send an offer they accept, decline or counter.

The design worry is a **cross matrix**: entry route × deal type, a 2×4 grid of behaviour leaves that
doubles again with the next route or the next deal. This plan's central claim is that **the matrix is an
illusion produced by one hardcoded coincidence**, and that removing the coincidence leaves a single
derived key and no new leaves.

## 2. The coincidence, read from the code

`ApplicationCheckoutExtensions` states entry-time behaviour as a deal-type test:

```csharp
public bool RequiresApplyCheckout() => dealType == DealType.VenueHire;
public bool RequiresAcceptCheckout() => dealType != DealType.VenueHire;
```

But who actually pays, read from `ApplicationCheckoutService`:

| Deal | Payer tenant passed to Payment | Checkout endpoint | Who performs that action |
|---|---|---|---|
| FlatFee | `application.VenueTenantId` → `escrowOperationsClient.AuthorizeAsync` | accept | the venue |
| DoorSplit / Versus | `application.VenueTenantId` → `SetupPaymentMethodAsync` | accept | the venue |
| VenueHire | `artistTenantId` → `SetupPaymentMethodAsync` | apply | the artist |

`RequiresApplyCheckout()` was never about venue hire. It is **`Payer == Artist`**, hardcoded to the one
entry route in which the artist happens to move first. The deal type is not the discriminator; the payer
is, and having exactly one route made the two indistinguishable.

That is the whole cross matrix. It exists only because the route was constant.

## 3. The real constraint — the payer has to be at the keyboard

Both `AuthorizeAsync` and `SetupPaymentMethodAsync` return a `ClientSecret` that the payer's browser
confirms. The payer must be **present** when their instrument is collected. Money moves at binding — the
last consent. So there are exactly two situations:

- the payer consents **last**, so they are present at binding → authorise on-session and capture
  (`CaptureEscrow`);
- the payer consents **first**, so they are gone by binding → collect a method now, move money
  off-session at binding (`DepositEscrow`).

That single rule reproduces today's behaviour and extends to offers with no new machinery:

| Route | Deal | Payer | Payer consents | Operation |
|---|---|---|---|---|
| apply | FlatFee | venue | last (accept) | `CaptureEscrow` — today |
| apply | DoorSplit / Versus | venue | at accept | `VerifyPayment` — today |
| apply | VenueHire | artist | first (apply) | `DepositEscrow` — today |
| **offer** | FlatFee | venue | **first (offer)** | **`DepositEscrow`** |
| **offer** | DoorSplit / Versus | venue | at offer | `VerifyPayment` |
| **offer** | VenueHire | artist | **last (accept)** | **`CaptureEscrow`** |

The two genuinely new cells reuse the two operations that already exist. **No new `FinancialOperation`
member, no new confirm leaf, no new cancel leaf.**

## 4. Payment needs no change at all

```csharp
public sealed record CaptureEscrowCommand(
    Guid OperationId, PaymentOperationReference Reference,
    Guid PayerId, Guid PayeeId, long AmountMinor, Currency Currency,
    PaymentOperationReference Authorization) : IIntegrationCommand;

public sealed record DepositEscrowCommand(
    Guid OperationId, PaymentOperationReference Reference,
    Guid PayerId, Guid PayeeId, long AmountMinor, Currency Currency,
    PaymentOperationReference PaymentMethod, PaymentSession Session) : IIntegrationCommand;
```

Payer and payee are plain tenant ids. Payment does not know a venue from an artist, so a flat fee funded
by the venue off-session and a hire fee captured from the artist on-session are commands it already
serves. This is why the offer route costs nothing across the service boundary.

## 5. The matrix collapses into one derived key

The research's `Expected()` derivation gains one input:

```csharp
public FinancialOperation Expected(this DealProfile profile, bool payerConsentsLast) =>
    (profile.Funds, payerConsentsLast) switch
    {
        (FundsTiming.Deferred, _)   => FinancialOperation.VerifyPayment,
        (FundsTiming.Escrowed, true)  => FinancialOperation.CaptureEscrow,
        (FundsTiming.Escrowed, false) => FinancialOperation.DepositEscrow,
    };
```

`payerConsentsLast` is `profile.Payer` compared against the entry route's second mover — a property of
the Application row, known the moment it is created and frozen with it.

Entry route therefore enters as **one boolean into a derivation**, never as a second registration
dimension. `IApply`, `IMintCommitment`, `IConfirm`, `ICancel` and `IComplete` stay keyed on
`FinancialOperation` with their existing leaf counts. A fifth deal type declares its axes and adds no
leaf; a third entry route adds no leaf either. That is standing-rule acceptance test 2 satisfied by
construction, and it is the answer to the cross-matrix worry.

`RequiresApplyCheckout()` / `RequiresAcceptCheckout()` are deleted. The question they answered becomes
"is this party the payer, and is this their consent?" — which the row already knows.

## 6. The modelling question — OPEN, NOT DECIDED

> **Contested.** This section previously read as a decision ("one aggregate, not two"). It is not one.
> The author lost confidence in it after an objection this section never answered: a variant-of-Application
> shape turned two non-null fields into five nullables. Do not build against this section. It is being
> re-derived; until that lands, treat the entry-route model as an open question with at least three live
> candidates — (a) variant of `ApplicationEntity` keyed on initiator, (b) its own entity converging at
> `Booking`, (c) slot/exclusivity extracted into a first-class thing both routes point at.

The argument this section *did* make, preserved because its evidence is real even though its conclusion
does not follow from it:

```csharp
builder.HasIndex(application => application.OpportunityId)
    .IsUnique()
    .HasFilter($"[State] = {(int)ApplicationState.Accepted}");
```

That filtered unique index is the only thing making an exclusive slot safe under concurrent acceptance.
Two tables cannot share a unique index, so splitting the aggregate turns "the artist accepts our offer"
racing "we accept a rival application" from a database decision into an application-level lock. It also
duplicates the shared flow the standing rule requires to stay in one stage workflow — an endpoint split
is for an *act*, never a copy of the shared action.

The index is real. **The inference is not.** It proves that whatever accepts into a `Booking` needs *one
exclusivity scope*; it does not prove *one table*. Shape (c) satisfies the same constraint with the scope
living on the slot rather than on the proposal. That gap is the open question.

## 7. The lifecycle, unchanged

```text
                          ┌── artist applies ───►  Application[Applied]  ── organiser accepts ──┐
Opportunity (organiser) ──┤                              ▲        │                             ├──► Booking ──► Concert
                          └── organiser offers ─►  Application[Offered] ─── artist accepts ─────┘
                                                         └── counter ──┘
```

Opportunity → Application → Booking → Concert survives exactly. The opportunity is organiser-created in
both routes, which is already true today; an offer is not a reversed arrow, it is the same arrow with the
first mover swapped. `Applied` and `Offered` are one fact — a proposal stands and one named party owes an
answer — differing only in whose turn it is. `Counter` swaps the turn and carries new terms.

Accept, Reject and Withdraw become symmetric: the party whose turn it is may accept or reject; the party
that authored the standing proposal may withdraw it. Authority derives from the row's own
`VenueTenantId`/`ArtistTenantId` pair and the state, never from `TenantType`.

## 8. No `OpportunityWorkflow`, and no backward write

The standing rule's **Not a home** list settles it: *"anything spanning stages: no process entity,
workflow-as-data, cross-module machine or Deal-owned orchestration."* Opportunity does not build
Bookings and must not build Applications; each stage builds itself from the previous stage's facts.

The offer endpoint therefore takes an **existing `opportunityId`**. The organiser creates the slot with
the existing `POST /api/opportunity` and offers against it, so Application only ever *reads* Opportunity —
the same edge `Booking → Application.Contracts` already uses. One click in the UI is two calls from the
client, not a module reaching backwards.

## 9. What keys what

| The new thing | Home | This plan |
|---|---|---|
| Terms figures, including countered terms | data arm; `IDealMapper` / `IDealUpdater`, legitimately `DealType`-keyed | counter creates a new deal through `IDealModule`; `ProposedDealId` on the Application |
| Payer-instrument behaviour | capability-keyed strategy on `FinancialOperation` | `IApply` / `IMintCommitment` re-keyed off `DealType` |
| Sending, declining, countering an offer | its own endpoint recording a fact (`DeclareDoorRevenue` precedent) | three endpoints |
| Client-supplied arm-specific *behaviour* | keyed union + tagged request | **none needed** — counter supplies data, not behaviour |

No union is introduced. Countered terms are figures of the deal, so they belong on the data arm, whose
families the standing rule keeps on `DealType`.

## 10. Consent and terms

Each side's signature is recorded only when that side acts, stamped with the terms fingerprint it signed.
Accept requires both sides' fingerprints to equal the effective terms' fingerprint now — one invariant
replacing today's `TermsFingerprint` guard and covering counteroffers with the same rule. An offer alone
creates no counterparty consent, no booking and no charge.

Effective terms are `ProposedDealId ?? the opportunity's deal`. A counter may not change `DealType`: the
financial arrangement is the opportunity's and the negotiation is over its figures. Changing arrangement
is a decline plus a new offer.

## 11. Phases

**Phase 1 — declare the profile.** `Payer`, `FundsTiming`, `SettlementBasis`, `DealProfile`, abstract
`DealTerms.Profile`; `ContractEntity.ExpectedFinancialOperation` computed from the profile instead of
hand-written per arm. *Gate:* Deal and Booking unit suites; the existing commitment-token assertion in
`ContractFactory<TTerms>` still passes.

**Phase 2 — re-key Application off `DealType`.** `IApply` and `IMintCommitment` on `FinancialOperation`;
delete `ApplicationCheckoutExtensions` and the three DTO type-tests in `ApplicationCheckoutService`.
Behaviour identical, one route still. *Gate:* Application unit and integration suites unchanged.

**Phase 3 — the second entry direction.** `Offered` state, `Counter` trigger, `Initiator`, per-side
signatures with fingerprints, `ProposedDealId`, opportunity admission and discovery filtering,
`payerConsentsLast` on the row and the extended `Expected()`. Re-scaffold initial migrations. *Gate:*
Application and Opportunity suites.

**Phase 4 — API.** Offer, counter and decline endpoints; the organiser permission; both action shapes.

**Phase 5 — frontend.** Artist offers surface with accept/decline/counter; venue send-offer and
counter-review; shared types and action labels.

## 12. Acceptance checks

- An organiser offers a **flat fee**; the artist accepts; the venue's funds move off-session through
  `DepositEscrow` and the booking confirms — with no Payment change and no new confirm leaf.
- An organiser offers **venue hire**; the artist accepts on-session through `CaptureEscrow`.
- Both existing apply-route journeys are byte-for-byte unchanged in behaviour.
- No consumer contains a `DealType` switch, `is XDealDto` test or `== DealType.X` comparison.
- Adding a fifth deal type requires a profile line and no behaviour leaf — asserted by a test.
- An offer alone creates no artist signature, no booking, no commitment, no charge.
- Artist declines; organiser withdraws; both reach the right terminal state and notify the right side.
- Artist counters, organiser accepts: the contract renders the countered figures and both signatures name
  the same fingerprint. A counter changing `DealType` is rejected. A stale consent blocks acceptance.
- A targeted opportunity is absent from artist discovery and rejects a direct apply.
- Two acceptances race on one opportunity: exactly one booking, loser reported not rerun.

## 13. Out of scope

Customer-configurable deals, workflow builders, capability registries, versioned configuration.
Promoter tenants, multiple business profiles, rooms, shared shows. Installment schedules and the `Accept`
union — that tier stays empty until product asks. Obligations. The organiser here is the opportunity's
owning venue tenant; the domain says "organiser" so later work changes a resolver, not every call site.
