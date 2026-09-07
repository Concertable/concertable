# Organiser direct offers

> **Next steps live in @plans/launch/BOOKING_ENTRY_DIRECT_OFFERS_PROGRESS.md → `## Next Steps`.**

Baseline: `origin/main` at `516f4cc25`, the merge of PR #633, read directly from that ref. Every file this
plan cites is byte-identical at `15ce7946f`, the current default-branch head. Judged against
the capability model decided in the workflow-divergence research, the entry-route section of the
configurable-deal-workflows product doc, and the standing rule those hand to
`api/Concertable.B2B/CODE_PATTERNS.md`.

## 1. The question this plan answers

Venues create opportunities and artists apply. An organiser must also be able to select an artist and
send an offer they accept, decline or counter.

Two worries have to be answered, not one. The first is a **cross matrix**: entry route × deal type, a 2×4
grid of behaviour leaves. The second, which sank the previous pass, is a **modelling** question: an
invitation is not obviously an application, and the thing offered is not obviously an opportunity.

The first worry is an illusion and §3–§4 dismantle it. The second is real, and §5 answers it with a shape
the previous pass did not consider.

## 2. Corrections to the previous pass

The previous pass's financial reading was right and is confirmed below against the real post-#633 tree.
Four things were wrong.

- **It was authored against the wrong tree.** It states baseline `516f4cc25` but was written in a
  checkout 592 commits behind, from before PR #633 split `Concert` into `Application`, `Booking`,
  `Opportunity` and `Dashboard`. Every path in it was stale. Everything below is read from `origin/main`.
- **Its §6 asserted "one aggregate, not two" from evidence that does not carry the claim.** The filtered
  unique index proves that acceptance needs *one exclusivity scope*; it does not prove one table. That
  section is now this §5, argued from the constraint that actually decides it.
- **The five-nullables objection is refuted, not answered — and it exposed a real defect.** The previous
  pass modelled the second route **by party** (add a venue signature beside the artist's), which is what
  produced five nullable fields. The entity's existing shape is already **by role**; it is merely *named*
  after the party, because only one party could ever hold that role. Modelled by role the count goes the
  other way: the recommended shape ends with **two fewer** `null!` fields than today. §5.2.
- **Its §8 ruled out an `OpportunityWorkflow` by citing the standing rule alone.** That is the right
  conclusion from the wrong argument, and it silently contradicted the product doc, which explicitly
  permits creating the private opportunity and the initial proposal together. The real reason is the
  module boundary. §8.

## 3. The coincidence, confirmed against `origin/main`

`ApplicationCheckoutExtensions` states entry-time behaviour as a deal-type test:

```csharp
public bool RequiresApplyCheckout() => dealType == DealType.VenueHire;
public bool RequiresAcceptCheckout() => dealType != DealType.VenueHire;
```

Who actually pays is settled two layers down, in the Booking confirm steps, and it is not a deal-type
fact at all:

```csharp
// FlatFeeConfirmStep                        // VenueHireConfirmStep
new CaptureEscrowCommand(                    new DepositEscrowCommand(
    booking.OperationId,                         booking.OperationId,
    PaymentOperationReferences.Escrow(...),      PaymentOperationReferences.Escrow(...),
    flatFee.VenueTenantId,   // payer            venueHire.ArtistTenantId,  // payer
    flatFee.ArtistTenantId,  // payee            venueHire.VenueTenantId,   // payee
    ..., flatFee.Commitment);                    ..., PaymentSession.OffSession);
```

`RequiresApplyCheckout()` was never about venue hire. It is **`Payer == Artist`**, hardcoded to the one
entry route in which the artist happens to move first. `PaymentSession.OffSession`, sitting in the venue
hire arm and nowhere else, is the mechanical proof.

**The payer axis already exists in code.** `IDealPayeeResolver` has exactly two leaves —
`VenuePaysArtistDealPayeeResolver` and `ArtistPaysVenueDealPayeeResolver` — registered across four
`DealType` keys. So does `ICommitmentReferenceStep`, whose three leaves (`EscrowHold`,
`MethodVerification`, `MethodSetup`) are `FinancialOperation` under other names. So do `IConfirmStep`
(`FlatFee`/`VenueHire`/`Verified`) and the `ICompleteStep`/`ICancelStep` pair (escrowed versus deferred).
Every one of these families is already keyed on an axis and merely *registered* per deal type.

## 4. The real constraint — the payer has to be at the keyboard

Both `AuthorizeAsync` and `SetupPaymentMethodAsync` return a `ClientSecret` the payer's browser confirms,
so the payer must be **present** when their instrument is collected. Money moves at binding, which is the
last consent. Two situations exist:

- payer consents **last** → present at binding → authorise on-session, capture (`CaptureEscrow`);
- payer consents **first** → gone by binding → collect a method now, move off-session (`DepositEscrow`).

| Route | Deal | Payer | Payer consents | Operation |
|---|---|---|---|---|
| apply | FlatFee | venue | last (accept) | `CaptureEscrow` — today |
| apply | DoorSplit / Versus | venue | at accept | `VerifyPayment` — today |
| apply | VenueHire | artist | first (apply) | `DepositEscrow` — today |
| **offer** | FlatFee | venue | **first (offer)** | **`DepositEscrow`** |
| **offer** | DoorSplit / Versus | venue | at offer | `VerifyPayment` |
| **offer** | VenueHire | artist | **last (accept)** | **`CaptureEscrow`** |

The two new cells reuse operations that already exist. **No new `FinancialOperation` member, no new
confirm leaf, no new cancel leaf.** The payer never changes with the route — only the *timing* does.

**Payment needs no change.** `CaptureEscrowCommand` and `DepositEscrowCommand` take plain `Guid PayerId`
and `Guid PayeeId`. Payment cannot tell a venue from an artist, so both new cells are commands it already
serves. No producer PR, no platform sync.

**One real change falls out of this.** `FlatFeeConfirmStep` and `VenueHireConfirmStep` hardcode which
tenant id is the payer. Re-keyed on `FinancialOperation`, each leaf must take payer and payee from the
arm's declared `Payer` instead of from a literal. Three leaves stay three leaves.

## 5. The modelling question, answered

### 5.1 Does Opportunity's fusion survive?

Opportunity today carries `VenueId`, `Period`, `DealId` — the **slot** — and `Genres` plus `State.Open` —
the **listing**. `Genres` exists only to match an open call (`ApplyAsync` rejects a genre mismatch;
`GetRecommendedAsync` ranks on it), and `WhereActive` filters `State == Open`. For an offer to a named
artist both listing concerns are meaningless.

The fusion nevertheless **survives**, because the product doc already settles it: *"Opportunity creation
remains organiser-led in both routes. Public visibility, permitted entry routes and targeted access are
separate choices."* Visibility is an **attribute of the slot**, not a second entity. Splitting Slot from
Listing would touch every `opportunityId` foreign key, `IOpportunitySyncer`, the Search projection and the
Customer service — none of which this feature needs.

So Opportunity gains **one non-null enum**, defaulted for every existing row:

```csharp
public enum OpportunityAdmission { OpenToApplications, ByInvitationOnly }
```

`WhereActive` adds `o.Admission == OpenToApplications`; `ApplyAsync` rejects `ByInvitationOnly`. Two call
sites, zero nullables, and it is an axis rather than a switch.

### 5.2 The nullable objection, refuted

Today the Application persists **one** signature. The venue's is never stored on the Application at all —
`ApplicationWorkflow.AcceptCoreAsync` builds it at accept time and puts it straight into the
`ContractSnapshot`, which is persisted on the **Contract**. So the true invariant is:

> The Application persists exactly one signature: **the author of the standing proposal**. The
> responder's is captured at acceptance and persisted on the Contract beside it.

That is a **role** invariant wearing a party's name. `ArtistESignature` is called that only because the
artist is always the author under one entry route — the identical error as `RequiresApplyCheckout()`.

Add a venue signature beside the artist's and you are modelling by party, and you get the five nullables
that sank the previous pass. Model by role and the count falls instead.

### 5.3 The four shapes, scored

| Shape | New nullables | Exclusivity | Duplicate guard | Counter history | Verdict |
|---|---|---|---|---|---|
| (a) variant of `ApplicationEntity`, keyed on initiator | 0 by role / 5 by party | unchanged ✓ | unchanged ✓ | **nowhere** | under-specified |
| (b) its own entity, converging at Booking | pushed onto `BookingEntity` | **lost** | **lost** | own table ✓ | **reject** |
| (c) exclusivity moved onto the slot | 1 on Opportunity | ✓ conceptually best | still needs a home | orthogonal | **reject, on boundaries** |
| **(d) Application as thread + Proposal versions** | **−2** | unchanged ✓ | unchanged ✓ | ✓ | **recommended** |

**(a)** is right about the aggregate and has nowhere to put a counteroffer. The product doc requires
*"counteroffers supported by versioned proposals"* and *"completed actions remain attributable to their
original version"*. A single row can only overwrite. (a) is not wrong; it is (d) missing its second table.

**(b)** fails on the two indexes it silently discards:

```csharp
builder.HasIndex(a => new { a.OpportunityId, a.ArtistId }).IsUnique();
builder.HasIndex(a => a.OpportunityId).IsUnique()
       .HasFilter($"[State] = {(int)ApplicationState.Accepted}");
```

The prompt is right that the second proves one *scope*, not one *table*. But the **first** is the one that
bites daily — at most one live proposal per (slot, artist), whoever started it — and an organiser offering
to an artist who has already applied is exactly the collision it exists to catch. Split the table and both
become application-level locks. (b) also forces `BookingEntity` to accept two parents, so the nullables
reappear downstream, and it copies the whole accept/reject/withdraw/notify flow, which the standing rule's
acceptance test 3 forbids: *an endpoint split is for an act, never a copy of the shared action.*

**(c)** is the conceptually cleanest home for exclusivity — `Opportunity.AcceptedProposalId` is a column
that can physically hold one value, and it would survive the Postgres migration without porting a
bracket-syntax filtered predicate. It is rejected for a boundary reason, not a conceptual one:
`ApplicationDbContext` and `OpportunityDbContext` are **separate contexts**, `IOpportunityModule` is
read-only, and Opportunity learns about acceptance **asynchronously** through
`ApplicationAcceptedIntegrationEventHandler` calling `MarkFilled()`. Making Opportunity the exclusivity
holder means a synchronous cross-module write, which `module-structure` forbids. The portability win it
offers belongs to `POSTGRES_MIGRATION_PLAN.md`, which has to port that predicate anyway.

### 5.4 The recommended model — (d)

`ApplicationEntity` becomes the **negotiation thread** over one slot with one artist. `ProposalEntity`
rows are its **append-only versions**.

```csharp
// ApplicationEntity - loses ArtistESignature and TermsFingerprint
Id, Version, OpportunityId, ArtistId, VenueTenantId, ArtistTenantId,
State, DealType, AcceptanceOperationId, VerifyPayment
Proposals : IReadOnlyList<ProposalEntity>          // ordered, append-only

// ProposalEntity - every field non-null
Id, ApplicationId, Ordinal,
ProposedBy       : ProposalSide,      // Artist | Organiser
Signature        : ContractSignature, // the proposer signs their own terms
TermsFingerprint : string,
DealId           : int,               // the terms this version proposes
CreatedAtUtc     : DateTime
```

The two `null!` fields on `ApplicationEntity` move down and become non-null, because a proposal cannot
exist unsigned. **Net −2 nullable fields against today.**

What this buys, none of it bolted on:

- **No new lifecycle state.** The previous pass added `Offered` beside `Applied`. Whose turn it is is
  `Proposals[^1].ProposedBy`, so the state machine keeps its four transitions and gains one self-loop,
  `Applied --Counter--> Applied`. Adding `Offered` would encode the initiator into the state and then
  multiply with every counter.
- **Both indexes untouched**, because the filtered index still reads `State = Accepted` on the thread.
- **"An offer alone creates no counterparty consent"** is structurally true: one signature exists on the
  thread until acceptance. It cannot be violated by forgetting a check.
- **Per-version attribution** for free — the Contract records the `ProposalId` it was cut from.
- **The stale-consent guard improves.** Today `AcceptCoreAsync` compares `application.TermsFingerprint`
  against the *opportunity's current* deal. It becomes: recompute against `Proposals[^1].DealId`. Same
  guard, and a counter's own terms stop being invalidated by an unrelated edit to the opportunity's deal.
- **A counter may not change `DealType`** — asserted against the thread's `DealType`. Changing the
  arrangement is a decline plus a new offer.

Cost: one table, one EF configuration, one re-scaffolded initial migration.

## 6. Where each divergence lives

| The new thing | Home | Key |
|---|---|---|
| `Payer`, `FundsTiming`, `SettlementBasis`, `DealProfile` | abstract member on the terms arm | compiler-forced per arm |
| `FinancialOperation` | derived from the profile plus one boolean | `(FundsTiming, payerConsentsLast)` |
| entry-time payment-method check | `IApplyStep`, re-keyed | `FinancialOperation` |
| commitment reference | `ICommitmentReferenceStep`, re-keyed | `FinancialOperation` |
| payer/payee at binding | `IConfirmStep` leaf reads the arm's `Payer` | `FinancialOperation` |
| countered figures | a new `Deal` through `IDealModule`; `Proposal.DealId` | data arm, legitimately `DealType`-keyed |
| offer, counter, decline | their own endpoints recording a fact | acts, not arms |

`payerConsentsLast` is `profile.Payer` compared against the thread's first proposer — known when the
thread is created and frozen with it. Entry route enters as **one boolean into a derivation**, never as a
second registration dimension. A fifth deal type declares its axes and adds no leaf; a third entry route
adds no leaf either.

**No keyed union is introduced, and the reason is not the previous pass's.** It argued that a counter
supplies data rather than behaviour. The real reason is that Apply and Offer are different **acts** —
different authority, different actor, different inputs, different preconditions — not two arms of one
shared action. The union exists for the case where one action's arms need different client-supplied
parameters; installments at Accept is still that case, and it is still hypothetical. `KeyedUnionBuilder`
therefore keeps its zero production consumers, deliberately.

**Generics are not the answer either, and the product doc says why:** *"`IApply<Artist>` versus
`IApply<Venue>` alone does not describe the operation or accommodate a promoter performing the same offer
action."* The discriminator is the capability axis, never the profile type.

`FinancialOperation` is currently `internal` to `Concertable.B2B.Booking.Domain.Financial`. Keying
Application families on it requires promoting it, with `Payer`, `FundsTiming` and `SettlementBasis`, to
`Concertable.B2B.Deal.Contracts.Enums` beside `DealType`. Every consumer of that project takes it by
`ProjectReference` and nothing outside `api/Concertable.B2B/` references it, so **this is not a published
contract change and needs no producer PR.**

## 7. The dispatch conditionals to remove

A conditional deciding *what kind of thing this is* belongs in a keyed strategy, an abstract member, or a
union match. A conditional deciding whether a guard passes is fine. The tell is a `default`/`_` arm a new
deal type or tenant type would fall into.

| Site | What it really keys on | Fix |
|---|---|---|
| `ApplicationCheckoutExtensions` (both) | `Payer` plus who is entering | delete; derive from the profile |
| `ApplicationCheckoutService:61` `is not VenueHireDealDto` | `FinancialOperation.DepositEscrow` | keyed leaf |
| `ApplicationCheckoutService:110` `is FlatFeeDealDto` | `FinancialOperation.CaptureEscrow` | keyed leaf |
| `ApplicationCheckoutService:131` `is not (DoorSplit or Versus)` | `FinancialOperation.VerifyPayment` | keyed leaf |
| `ApplicationCheckoutService:153` `ToPaymentAmount` throwing `_` | the arm's own figures | abstract member on `DealTerms` |
| `ApplicationController:87` `switch (membership.Type)` → `Forbid()` | responder versus proposer | role-derived response mapper (PR #871) |
| `ApplicationEntity.NotifyCounterparty` recipient ternary | who acted | counterparty of the actor |
| `BookingRepository:71` `!= DealType.VenueHire` | `Payer == Venue` | payer-based predicate |

Three more the prompt did not list, found by sweeping `origin/main`:

| Site | Why it qualifies |
|---|---|
| `ApplicationTermsFingerprint:14` `deal switch` with `_ => throw` | pure computation over the arm's figures; belongs on `DealTerms` beside `Render()`. A fifth deal type throws at runtime. |
| `ApplicationMappers` (Api) `:28`, `:44` | the two live consumers of `RequiresAcceptCheckout()`; they decide action links and must become role-derived |
| `OpportunityMapper:80` `RequiresApplyCheckout()` | the third consumer, in a different module |

`SeedState:541` (`is VenueHireDealEntity`) is seed-fixture selection, not production dispatch; left alone
and recorded here so the next sweep does not re-flag it.

`ApplicationCounterpartyNotifiedDomainEventHandler.Copy` has a throwing `_` over `ApplicationNotification`
and its copy is written from the artist-applies perspective. It gains `Offered`, `Countered` and
`Declined` members and the copy becomes role-phrased.

## 8. Where the offer is created, and no `OpportunityWorkflow`

The product doc permits *"a direct-offer action may create its private opportunity and initial proposal
together"* and says an `OpportunityWorkflow` is warranted only *"when creation needs to coordinate the
opportunity, initial proposal and related actions"*.

It is not warranted, for a boundary reason. Opportunity and Application are separate modules with separate
`DbContext`s; `IOpportunityModule` exposes reads only. A server-side composition would either make
Application a writer into Opportunity or need a cross-module transaction — both forbidden. So:

**The composition is the client's.** One click is two calls: `POST /api/opportunity` with
`Admission = ByInvitationOnly`, then `POST /api/application/opportunity/{id}/offer`. Application only ever
*reads* Opportunity, the same edge `Booking → Application.Contracts` already uses.

**The one place this misses the product doc's first-slice bar, stated plainly.** The doc asks for
*"retries that cannot duplicate an opportunity or offer"*. The **offer** is protected by a hard constraint
— `(OpportunityId, ArtistId)` unique. The **opportunity** is not: a retried first call can leave an
orphan. An orphan `ByInvitationOnly` opportunity appears in no discovery query and carries no proposals,
so it is invisible rather than harmful. Closing it properly is a client-supplied idempotency key on
opportunity creation; that is named here and deliberately not built in this PR.

## 9. Endpoints, authority and visibility

| Endpoint | Actor | Permission |
|---|---|---|
| `POST /api/application/opportunity/{id}/offer` | organiser | new `VenuePermissions.OffersSend` |
| `POST /api/application/{id}/counter` | whoever owes the answer | existing pair |
| `POST /api/application/{id}/decline` | whoever owes the answer | existing pair |
| `POST /api/application/{id}/accept` | whoever owes the answer | existing pair |
| `POST /api/application/{id}/withdraw` | the author of the standing proposal | existing pair |

Accept, decline and withdraw become symmetric and derive authority from the row's own
`VenueTenantId`/`ArtistTenantId` pair against `Proposals[^1].ProposedBy` — never from `TenantType`. That
is what retires the `switch (membership.Type)` in `GetById`.

`ApplicationsDecide` stays the venue's accept/reject permission. Sending an offer is a distinct act and
gets `OffersSend`, so an organisation can grant one without the other.

Targeted visibility is `OpportunityAdmission` (§5.1). A `ByInvitationOnly` opportunity is absent from
artist discovery and rejects a direct apply; the invited artist reaches it through the Application the
offer created, not through the opportunity.

## 10. The handoff into Booking

`Booking` is already route-agnostic. Its entry point is one pre-commit handler:

```csharp
ApplicationAcceptedDomainEvent(AcceptedApplication) → bookingWorkflow.ConfirmAsync(...)
```

`AcceptedApplication` wraps `ApplicationAcceptanceSnapshot(OperationId, ApplicationSnapshot,
ContractSnapshot)`, and `ContractSnapshot` already carries **both** `ArtistSignature` and
`VenueSignature`. Nothing in Booking asks who moved first.

**Minimum handoff:** build the same `AcceptedApplication`, taking the proposer's signature from
`Proposals[^1]` and the responder's from the accept request. Add `ProposalId` to
`ApplicationAcceptanceSnapshot` for attribution. The only Booking-side change is the payer/payee
generalisation in the confirm steps from §4 — no new event, no new handler, no new state.

## 11. Phases

All five land as **one PR**; they are sequenced so each commit builds and its suites pass.

**Phase 1 — declare the axes.** `Payer`, `FundsTiming`, `SettlementBasis`, `DealProfile`, abstract
`DealTerms.Profile`; promote `FinancialOperation` into `Concertable.B2B.Deal.Contracts.Enums`;
`ContractEntity.ExpectedFinancialOperation` derived from the profile instead of hand-written per arm. Move
the fingerprint numbers onto `DealTerms` beside `Render()`.
*Gate:* Deal, Booking and Application unit suites; the commitment-token assertion in
`ContractFactory<TTerms>` still passes.

**Phase 2 — re-key off `DealType`.** `IApplyStep` and `ICommitmentReferenceStep` on `FinancialOperation`;
confirm-step payer/payee read from the arm's `Payer`; delete `ApplicationCheckoutExtensions` and its three
consumers; retire the three `is XDealDto` tests and `ToPaymentAmount`'s throwing arm; make
`BookingRepository:71` payer-based. Behaviour identical, one route still.
*Gate:* Application and Booking unit and integration suites, unchanged assertions.

**Phase 3 — the thread and its versions.** `ProposalEntity`, `ProposalSide`, the `Counter` self-loop,
`Application.Proposals`; move `ArtistESignature`/`TermsFingerprint` down; `OpportunityAdmission` and the
discovery filter; the fingerprint guard against `Proposals[^1].DealId`; `ProposalId` on the acceptance
snapshot. Re-scaffold initial migrations via `./initial-migrations.ps1`.
*Gate:* Application and Opportunity integration suites; a concurrent-acceptance test.

**Phase 4 — API.** Offer, counter and decline endpoints; `OffersSend`; role-derived action links and the
role-derived `GetById` mapper; the new notification kinds and role-phrased copy.
*Consumption contract:* `ApplicationResponse<TActions>` keeps its shape; `TActions` becomes
proposer/responder-derived rather than venue/artist-derived, and every action link is present exactly when
the caller may take it — the frontend adds no conditional of its own.

**Phase 5 — frontend.** Artist offers surface with accept/decline/counter; venue send-offer and
counter-review; shared types and action labels. No `dealType` conditional exists in `app/` today and none
is added.

## 12. Acceptance checks

- An organiser offers a **flat fee**; the artist accepts; the venue's funds move off-session through
  `DepositEscrow` and the booking confirms — no Payment change, no new confirm leaf.
- An organiser offers **venue hire**; the artist accepts on-session through `CaptureEscrow`.
- Both existing apply-route journeys are unchanged in behaviour.
- No consumer contains a `DealType` switch, an `is XDealDto` test or a `== DealType.X` comparison.
- Adding a fifth deal type requires a profile line and no behaviour leaf — asserted by a test.
- An offer alone creates no responder signature, no booking, no commitment, no charge.
- Artist declines; organiser withdraws; both reach the right terminal state and notify the right side.
- Artist counters, organiser accepts: the contract renders the countered figures, both signatures name the
  same fingerprint, and the Contract records the `ProposalId` it was cut from. A counter changing
  `DealType` is rejected. A stale consent blocks acceptance.
- A `ByInvitationOnly` opportunity is absent from artist discovery and rejects a direct apply.
- An organiser offers to an artist who has already applied: the unique index rejects it and the API
  reports the existing thread rather than a server error.
- Two acceptances race on one opportunity: exactly one booking, loser reported not rerun.
- `ApplicationEntity` declares no `null!` member.

## 13. Out of scope

Customer-configurable deals, workflow builders, capability registries, versioned configuration. Promoter
tenants, multiple business profiles, rooms, shared shows. Installment schedules and the `Accept` union —
that tier stays empty until product asks. Obligations. Splitting Opportunity into Slot and Listing. Moving
exclusivity onto the slot (§5.3c) and the idempotency key on opportunity creation (§8), both named and
both deliberately deferred. The organiser here is the opportunity's owning venue tenant; the domain says
"organiser" so later work changes a resolver, not every call site.
