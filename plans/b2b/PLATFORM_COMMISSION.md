# Pricing transparency for the Payment-owned platform fee

> **Active launch plan.** PR #209 (`Feature/PlatformCommission`) and PR #223
> (`Feature/PaymentLedger`) are merged. Payment currently charges a real, fail-closed, flat **£10**
> platform fee on every B2B settlement and records it in the ledger. What remains is to bind that
> Payment-owned price to the payer's pre-commitment disclosure across all four contract types.
>
> The percentage rate-card is a deferred evolution. This work discloses and honours the flat fee the
> product charges now; it does not implement percentage pricing.

## 1. Current truth and decision history

Current code is the source of truth:

- `PlatformFeeOptions` is internal to Payment and is validated on startup. Both Payment Web and
  Workers configure `Fee = 10`; a missing value does not fall back to zero.
- `ManagerPaymentService` and `EscrowService` charge `gross + platform fee`, transfer or release
  `gross` to the payee, and persist the fee on `SettlementTransactionEntity` or `EscrowEntity`.
- the Payment ledger posts the retained fee as platform revenue and reconciles each settlement to zero;
- B2B has no live pricing config and no API contract for obtaining the Payment-owned fee;
- the manager checkouts show only the deal amount or formula, and the door-takings dialog shows revenue
  inputs but not the resulting artist settlement, platform fee, or total charge.

The dated history resolves the stale tracker prose:

| Date | Evidence | Decision |
|---|---|---|
| 2026-07-01 | `ab68fdbd`, early `LAUNCH_PLAN.md` | proposed roughly 5% plus a floor before implementation existed |
| 2026-07-24 | `f541dc32`, Platform Commission work | changed launch pricing to a flat Payment-owned fee |
| 2026-07-25 | `0f9e4205`, `44ec2651` | made configuration fail closed and set the real fee to £10 |
| 2026-07-25 | `48e4f6fc` | recorded percentage pricing as a later evolution, not the launch implementation |
| 2026-07-26 | PR #223 / `d02c825c` | completed the Payment ledger first and explicitly handed back to pricing transparency |

Therefore the launch decision is:

```text
payer charge = gross settlement + £10 platform fee
payee receipt = gross settlement
Concertable retention = £10 platform fee
```

The fee follows the payer. FlatFee, DoorSplit and Versus are venue-to-artist payments; VenueHire is an
artist-to-venue payment. No deal-type pricing calculator belongs in B2B for this flat-fee launch.

## 2. Scope and non-goals

### In scope

- an authoritative, immutable Payment-owned fee quote which the eventual settlement must honour;
- gross, platform-fee and total-charge disclosure at every real payer commitment point;
- exact fixed-price disclosure for FlatFee and VenueHire;
- formula disclosure at DoorSplit/Versus acceptance and an exact reviewed charge at door declaration;
- Payment client/proto additions, B2B persistence, API DTOs and HATEOAS actions;
- venue-manager and artist-manager UI, failure handling, accessibility and tests;
- correction of the launch trackers to the shipped £10 flat-fee decision.

### Explicit non-goals

- the deferred percentage/minimum/cap rate-card, policy versioning, tenant-specific rates or promotions;
- changing the £10 amount or moving live pricing configuration into B2B;
- customer ticket checkout or any customer marketplace pricing surface;
- changing which party pays, the Stripe Connect money flows, refunds, ledger accounting or invoice VAT;
- changing the Versus formula. The implemented contract is **guarantee plus door percentage**; only the
  incorrect “whichever is greater” manager-SPA copy is corrected;
- a platform-fee VAT invoice. The existing self-billed invoice concerns the artist/venue supply, not
  Concertable's fee supply, and remains separate legal/accounting work;
- removing the old Payment client overloads in the same cut-over. That would be a later breaking package
  contraction after every consumer has migrated.

## 3. The real payer journeys and disclosure points

The existing HATEOAS actions are accurate routing signals, but “Apply/Accept” alone is not precise enough
to identify financial commitment.

| Contract | Payer | Current journey | Last payer-present commitment point | Required disclosure |
|---|---|---|---|---|
| **FlatFee** | venue | artist applies normally → venue selects Accept → `ApplicationActions.Checkout` routes to `VenueAcceptCheckoutPage` → Stripe manual-capture hold → `/accept` captures into escrow | immediately before **Confirm & Pay** confirms the hold; acceptance/capture follows from the same screen | artist gross fee, £10 platform fee, exact total held/charged |
| **VenueHire** | artist | `OpportunityActions.Checkout` routes Apply to `ArtistApplyCheckoutPage` → SetupIntent saves the card → application is submitted → venue later accepts and Payment deposits off-session into escrow | immediately before **Authorise & Apply**; this is the artist's last chance to approve the later charge | venue gross hire fee, £10 platform fee, exact future total charge |
| **DoorSplit** | venue | artist applies normally → venue accepts through `VenueAcceptCheckoutPage` → SetupIntent verifies/saves a card → after the concert the venue opens `MyConcertPage` and declares external takings → completion worker charges off-session | formula commitment before **Confirm** in accept checkout; exact monetary commitment before confirming door takings | at accept: `artist % × total takings`, £10, and formula total; at declaration: exact artist gross, £10, exact total |
| **Versus** | venue | same deferred flow as DoorSplit | same two points | at accept: `guarantee + artist % × total takings`, £10, and formula total; at declaration: exact artist gross, £10, exact total |

For DoorSplit and Versus, “total takings” means Concertable ticket sales plus the venue's declared external
takings. The exact disclosure must use the same `IArtistShareCalculator` strategies as settlement:

```text
DoorSplit gross = total takings × artist percentage
Versus gross = guarantee + (total takings × artist percentage)
```

`AcceptApplicationPage` remains the review/routing page. For FlatFee, DoorSplit and Versus it must not
pretend its **Continue** button is the charge commitment; the price is rendered on the checkout it opens.
VenueHire acceptance is performed later by the venue without a payer present, so it uses the quote already
approved and stored when the artist applied.

## 4. Architecture decision: Payment-owned immutable fee quotes

### Designs considered

| Design | Service carve | Price integrity | Decision |
|---|---|---|---|
| bind `PlatformFee` independently in B2B | no runtime call, but duplicates live pricing authority across services | split-brain config can disclose one price and charge another | rejected |
| B2B calls a live `GetCurrentFee` RPC | Payment remains authoritative | time-of-check/time-of-charge drift remains, especially for VenueHire and post-concert settlement | rejected |
| manager SPAs call Payment directly | exposes the adapter service and its auth/contract to browsers; bypasses B2B HATEOAS | still needs B2B binding at settlement | rejected |
| Payment creates an immutable quote and every quoted settlement supplies its ID | respects the carve: B2B may synchronously depend on the Payment adapter through its published client package | the fee shown is the fee charged, even after config changes or service restarts | **selected** |

Payment remains the sole owner of `PlatformFeeOptions`. B2B stores only an opaque quote ID and the
application/settlement facts it already owns. It never stores or evaluates a second live rate card.

### 4.1 Payment model and public client contract

Add a Payment-owned `PlatformFeeQuoteEntity` with:

- opaque `Guid Id`;
- `Guid PayerTenantId`;
- immutable Kernel `Money PlatformFee`;
- `DateTimeOffset CreatedAt`.

Quotes are valid for the application lifetime and do not silently expire or refresh. A config change
affects newly created quotes only. The quote table survives restarts and delayed settlement; unused quotes
are harmless audit records.

Add an additive `IPlatformFeeQuoteClient` to the published `Concertable.Payment.Client` package:

```csharp
Task<PlatformFeeQuote> CreateAsync(Guid payerTenantId, CancellationToken ct = default);
Task<PlatformFeeQuote> GetAsync(Guid quoteId, Guid payerTenantId, CancellationToken ct = default);
```

`PlatformFeeQuote` contains the ID, `Money PlatformFee` and creation time. The proto adds a
`PlatformFeeQuoteService`, create/get messages, and uses the existing minor-unit money representation.
Register the client in `AddPaymentClient` and map the gRPC service in Payment Web.

`CreateAsync` snapshots the validated `PlatformFeeOptions` value once. `GetAsync` fails for an unknown
quote or a payer mismatch; it never substitutes the current fee.

### 4.2 Bind the quote to money movement

Add quote-aware overloads to `IManagerPaymentClient` and `IEscrowClient`, retaining the existing overloads
during the package expansion:

- `CreateHoldSessionAsync(..., Guid platformFeeQuoteId, ...)` for FlatFee;
- `CaptureAsync(..., Guid platformFeeQuoteId, ...)` for FlatFee;
- `DepositAsync(..., Guid platformFeeQuoteId, ...)` for VenueHire;
- `PayAsync(..., Guid platformFeeQuoteId, ...)` for DoorSplit/Versus.

Add the corresponding optional proto fields so the wire change is backward compatible. The quote-aware
server paths must:

1. load the quote inside Payment;
2. require its payer and currency to match the operation;
3. use the quoted fee rather than re-reading `PlatformFeeOptions`;
4. persist `PlatformFeeQuoteId` beside the existing fee snapshot on `EscrowEntity` or
   `SettlementTransactionEntity`;
5. place the quote ID in Stripe metadata for audit correlation;
6. fail before a Stripe hold, charge, capture or transfer on an absent, unknown or mismatched quote.

Refund and release arithmetic remains unchanged: cancellation refunds the full charged amount, release
transfers gross, and the ledger posts the snapshotted fee. Tests must prove the fee quote and the ledger's
platform-revenue posting agree.

### 4.3 Published-package sequence

This is an additive package expansion, not a breaking identity/signature cut-over: old interfaces and proto
fields stay usable while the new overloads are introduced. It still has a strict availability sequence:

1. merge the Payment contract/client/server expansion while B2B remains on the old API;
2. let `publish-packages` publish the new `Concertable.Payment.Client`;
3. follow the generated `chore/platform-sync-*` PR to green and merged;
4. update the consumer branch from `origin/main` so B2B compiles against the published version;
5. only then commit B2B source that calls `IPlatformFeeQuoteClient` or quote-aware payment overloads.

No Payment source project reference and no manually bumped individual package version is allowed. If a
future cleanup removes the legacy overloads, that separate breaking contraction must use the full
expand → publish → sync → consumer → contract sequence.

## 5. B2B domain, API and HATEOAS design

### 5.1 Application binds the disclosed quote

Add nullable `Guid? PlatformFeeQuoteId` to `ApplicationEntity`, with domain methods that attach a quote
once and require it at a priced transition. It is nullable only for already-existing rows and the
intermediate deployment; every newly committed priced flow must finish with a quote.

- **FlatFee:** `HoldCheckoutStep` creates and attaches the quote before creating the hold. Reopening the
  checkout reuses the attached quote.
- **VenueHire:** no application exists when apply checkout opens. `SetupCheckoutStep` creates a quote and
  returns its ID; `ApplyRequest` echoes that ID; Apply validates it against the artist tenant and stores it
  on the new prepaid application before saving the payment method.
- **DoorSplit/Versus:** `VerifyCheckoutStep` creates and attaches the quote before returning the card
  verification session. Reopening reuses it.

Add optional `PlatformFeeQuoteId` to `ApplyRequest` and `AcceptRequest`. Workflow validation makes it
required only where the payer used a checkout: VenueHire Apply and FlatFee/DoorSplit/Versus Accept. The
submitted ID must equal the ID rendered on that checkout. VenueHire's later direct Accept reads the quote
from the application.

Carry the quote ID through `BookingSettlement` so `CaptureEscrowAcceptStep`,
`DepositEscrowAcceptStep` and `PayoutFinishStep` call the quote-aware Payment overloads. A missing quote
is a conflict/precondition failure before booking creation or money movement, never a fallback to the
current fee.

Re-scaffold the Concert EF model after adding the application field. Do not hand-edit generated
migrations.

### 5.2 Checkout DTO

Keep the existing `IPaymentAmount` discriminated union for the deal amount/formula and add a separate
pricing disclosure to `Checkout`:

```text
KnownPrice
  platformFeeQuoteId
  grossMinor
  platformFeeMinor
  totalMinor
  currency

DeferredPrice
  platformFeeQuoteId
  platformFeeMinor
  currency
```

`KnownPrice` is returned for FlatFee and VenueHire. `DeferredPrice` is returned for DoorSplit and Versus;
their existing `DoorSharePayment` / `GuaranteedDoorPayment` supplies the formula. Do not encode an unknown
gross or total as zero or nullable “success”.

All browser-facing money is integer minor units plus an ISO currency code. B2B maps the Payment `Money`
quote and its own gross through Kernel `Money`; there are no new `decimal * 100`, JavaScript float
addition or hard-coded pound-string sites.

`ApplicationActions.Checkout` and `OpportunityActions.Checkout` keep their current meanings and routes.
The price payload is returned by those POST checkout actions.

### 5.3 Exact DoorSplit/Versus review

Add a HATEOAS action `ConcertActions.QuoteDoorRevenue` beside `DeclareDoorRevenue`, available only under
the same venue-owner, ended, revenue-share, `Booked`, not-yet-declared conditions. Add:

```text
POST /api/Concert/{id}/door-revenue/quote
body: { doorRevenue }
response:
  concertableSalesMinor
  externalTakingsMinor
  totalTakingsMinor
  grossMinor
  platformFeeMinor
  totalMinor
  currency
  platformFeeQuoteId
```

The quote endpoint is read-only. Refactor the existing declaration guard/context load so quote and declare
cannot drift on tenant, deal type, end time or lifecycle rules. Resolve the gross with the existing
`IArtistShareCalculator`; do not add controller branching on `DealType`.

The final declaration request echoes `PlatformFeeQuoteId` and `ExpectedGrossMinor`. The server recomputes
from the same external input and current frozen deal/ticket facts and returns `409 Conflict` if the expected
gross is stale. On success it persists the external takings and the reviewed gross atomically.
`RevenueShareSettlementAmount` then reads that persisted gross, so the completion worker and invoice issuer
cannot later recompute a different charge. A second declaration remains unavailable and fails closed.

### 5.4 Error mapping

- Payment unavailable while creating/loading a quote: B2B returns `503 Service Unavailable` with a stable
  problem code such as `pricing_unavailable`.
- quote absent from a required application transition: `409 Conflict` / `pricing_quote_required`;
- unknown quote, payer mismatch or submitted ID mismatch: `409 Conflict` /
  `pricing_quote_mismatch`;
- stale DoorSplit/Versus gross: `409 Conflict` / `pricing_changed`, requiring a new review;
- invalid door input remains `400 Bad Request`;
- a quote failure during the completion worker follows the existing settlement-failure path and moves no
  money. It must not retry with current configuration.

## 6. Manager-SPA design

Keep B2B pricing types and components in `app/web/b2b/shared`; do not widen customer/shared types with
manager settlement concepts. `CheckoutSession` and genuinely universal primitives may remain in
`app/web/shared`. Move the currently B2B-only checkout amount/types out of the universal tier as needed
rather than adding more leakage.

### 6.1 Exact surfaces and copy

| App and surface | Change |
|---|---|
| venue manager — `VenueAcceptCheckoutPage` for **FlatFee** | `OrderSummaryCard` shows “Artist gross fee”, “Concertable platform fee”, and “Total charged”; **Confirm & Pay** stays disabled until the quote is loaded and bound |
| artist manager — `ArtistApplyCheckoutPage` for **VenueHire** | show “Venue gross hire fee”, “Concertable platform fee”, and “Total charged if accepted” before **Authorise & Apply**; submit the rendered quote ID with the application |
| venue manager — `VenueAcceptCheckoutPage` for **DoorSplit** | show “Artist settlement: N% of total takings”, “Concertable platform fee: £10.00”, and “Total charged after the concert: artist settlement + £10.00” before **Confirm** |
| venue manager — `VenueAcceptCheckoutPage` for **Versus** | show “Artist settlement: £X guarantee + N% of total takings”, the platform fee, and the formula total; correct `AcceptDealSummary` to the implemented plus formula |
| venue manager — `DeclareDoorRevenueButton` on owned `MyConcertPage` | replace one-step **Record takings** with input → **Review charge** → exact review → **Confirm takings & charge**; show sales-source totals followed by artist gross, platform fee and total charge |

Artist-manager screens for FlatFee/DoorSplit/Versus and venue-manager screens for VenueHire do not add a
charge breakdown because those users are payees, not payers. Their existing deal/contract information
remains unchanged.

### 6.2 Loading, failure and stale-price behaviour

- keep the existing checkout skeleton while both checkout session and pricing quote load;
- do not mount/enable the Stripe submit path without a valid price disclosure;
- render an actionable, focused `role="alert"` with **Retry price** when `pricing_unavailable` occurs;
- preserve the form/signature when retrying;
- a rendered immutable quote is valid for that application lifetime. Refreshing an attached
  FlatFee/DoorSplit/Versus checkout returns the same quote; the fee never changes silently in-place;
- VenueHire submits exactly the quote visible when the artist authorised. A config change after Apply
  cannot alter the later off-session charge;
- editing door takings after review invalidates the review and returns to **Review charge**;
- disable final door confirmation during quote/declaration requests. On `pricing_changed`, retain the
  entered value, announce that the price changed, and require review again;
- generic network or API failures never expose an enabled financial action and never display £0 as a fee.

### 6.3 Accessibility and formatting

- render each breakdown as a semantic `<dl>` with `dt`/`dd`; headings identify whether the total is due
  now or after acceptance/concert;
- announce quote loading, refreshed totals and failures through a polite live region; errors also receive
  focus;
- do not use colour alone for status or fee emphasis;
- use the shared `formatCurrency(minor, { currency, fractionDigits: 2 })` for every displayed amount;
- retain meaningful button labels and visible focus; the door review remains keyboard operable and returns
  focus to the changed/error heading when invalidated;
- add stable test IDs to gross, platform-fee and total rows, but keep accessible names as the primary
  Reqnroll selectors where practical.

## 7. Tests and verification

### Payment unit/integration coverage

- quote creation snapshots the configured £10 as GBP `Money`;
- `GetAsync` returns the immutable value after the configured option changes;
- unknown quote and payer mismatch fail;
- hold, capture, deposit and direct pay use the quote rather than live options;
- charged total, payee transfer/release, persisted quote ID, persisted fee and ledger posting agree;
- no Stripe operation occurs for a missing/mismatched quote;
- full cancellation refund still includes the fee;
- gRPC/client mappings preserve quote ID, minor units and currency.

### B2B Concert unit/integration coverage

- `HoldCheckoutStep`, `SetupCheckoutStep` and `VerifyCheckoutStep` return the correct known/deferred
  disclosure and bind/reuse quotes as designed;
- VenueHire Apply and all priced Accept paths reject absent or mismatched quote IDs before persistence;
- capture, deposit and `PayoutFinishStep` pass the application quote ID;
- application/opportunity checkout HATEOAS remains contract-type correct;
- `QuoteDoorRevenue` and `DeclareDoorRevenue` share authorization/lifecycle gates;
- DoorSplit uses percentage of combined Concertable plus external takings;
- Versus uses guarantee **plus** percentage;
- stale expected gross returns 409 and does not mutate; successful declaration persists the reviewed gross;
- Payment unavailability maps to 503 and leaves applications/concerts unchanged.

Extend the four existing contract integration classes and `ConcertDoorRevenueApiTests`; update the
integration fixture's Payment mocks with `IPlatformFeeQuoteClient` and quote-aware settlement calls.

### Frontend and E2E coverage

There is no frontend unit-test runner in these workspaces; do not introduce one solely for this feature.
The TypeScript/build gates cover component/type integration and the existing Reqnroll UI suite covers the
rendered payer journeys:

- FlatFee: £500.00 gross + £10.00 fee = £510.00 before venue confirmation;
- VenueHire: £300.00 gross + £10.00 fee = £310.00 before artist authorisation;
- DoorSplit: acceptance shows the 70% formula plus £10; the current £300.00 takings example reviews
  £210.00 gross + £10.00 = £220.00 before declaration;
- Versus: acceptance says £100 + 70%, never “whichever is greater”; the current £20.00 takings example
  reviews £114.00 gross + £10.00 = £124.00;
- quote failure/stale-review coverage proves the financial button remains unavailable until a valid price
  is reviewed.

Local gates for behaviour phases:

```powershell
dotnet build api/Concertable.slnx
dotnet test api/Concertable.Payment/tests/Concertable.Payment.UnitTests/Concertable.Payment.UnitTests.csproj
dotnet test api/Concertable.Payment/tests/Concertable.Payment.IntegrationTests/Concertable.Payment.IntegrationTests.csproj
dotnet test api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.UnitTests/Concertable.B2B.Concert.UnitTests.csproj
dotnet test api/Concertable.B2B/src/Modules/Concert/Tests/Concertable.B2B.Concert.IntegrationTests/Concertable.B2B.Concert.IntegrationTests.csproj
cd app
npm run build:customer
npm run build:venue
npm run build:artist
npm run build:business
```

Re-scaffold and verify the affected Payment/Concert migration models in the phase that changes each model.

Pricing transparency changes payer-visible payment behaviour, so Phases 2 and 3 do **not** use a skip-E2E
trailer. Per `plans/AGENTS.md`, do not duplicate the full API/UI E2E run locally before a PR: the merge
queue is the E2E gate. If its API or UI E2E fails, reproduce once through the matching E2E debug skill,
fix the cause, and let the queue rerun it.

## 8. Independently shippable phases

### Phase 1 — Payment-owned immutable fee quote capability

1. Add `PlatformFeeQuoteEntity`, repository/configuration, migration and model re-scaffold.
2. Add quote application service, gRPC service/messages, `IPlatformFeeQuoteClient`, client implementation,
   DI registration and routing.
3. Add quote-aware ManagerPayment/Escrow overloads and proto fields while retaining legacy overloads.
4. Persist/audit quote IDs on escrow and settlement transactions and cover the money/ledger invariants.
5. Build the solution and run Payment unit/integration tests.
6. Commit with `Skip-E2E: true` because no consumer or user behaviour changes yet.
7. **Hard stop:** merge/publish this package expansion and follow platform-sync to green before Phase 2
   references the new client.

This phase is green and deployable with existing B2B consumers untouched.

### Phase 2 — Fixed-price disclosure and quote binding

Start only after the new Payment package pin is on `origin/main`.

1. Add `ApplicationEntity.PlatformFeeQuoteId`, request/checkout DTOs, known-price mapping and migration.
2. Bind FlatFee hold/capture and VenueHire setup/apply/deposit to the displayed quote.
3. Implement the venue FlatFee and artist VenueHire breakdowns, fail-closed loading/retry and currency/a11y
   component work in the B2B frontend layer.
4. Add Payment/B2B unit and integration coverage plus FlatFee/VenueHire Reqnroll assertions.
5. Run all local gates in §7; leave E2E to the merge queue.
6. **Hard stop:** commit the independently usable fixed-price transparency slice and hand off Phase 3.

### Phase 3 — Deferred-price disclosure, exact review and close-out

1. Bind DoorSplit/Versus accept checkout and later direct payout to the immutable quote.
2. Add the door-price quote endpoint/action, shared declaration guards, stale-gross check and persisted
   reviewed gross.
3. Implement formula disclosure and the two-step exact charge review on the venue manager SPA.
4. Correct Versus copy to guarantee plus percentage.
5. Add DoorSplit/Versus unit, integration and Reqnroll assertions; run all local gates and leave full E2E
   to the merge queue.
6. Update `LAUNCH_PLAN.md` and `LAUNCH_CHECKLIST.md` to mark B2B pricing transparency complete.
7. Delete this completed plan in the same verified implementation commit.
8. **Hard stop:** commit and provide the final PR handoff; do not begin percentage pricing or customer
   marketplace work.

## 9. Completion criteria

- every B2B payer sees gross, £10 platform fee and exact/formula total before the real commitment;
- the quote shown is provably the fee Payment uses for the later hold, escrow or direct charge;
- config changes cannot alter an already disclosed application fee;
- exact deferred gross is reviewed and persisted before off-session settlement;
- no unavailable/missing/stale price can fall through to a financial action;
- all four contract tests, all four frontend builds and merge-queue E2E are green;
- launch trackers describe flat £10 launch pricing and percentage/customer pricing only as deferred work;
- this plan is deleted with the final verified phase.
