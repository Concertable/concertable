# Percentage platform commission and pricing transparency

> **Active launch plan.** The temporary flat £10 platform fee is shipped, but it is not the launch
> pricing model. Replace it before launch with one Payment-owned percentage applied to the final
> deal gross calculated by B2B. The payer pays gross plus commission; the payee receives the agreed
> gross. This plan deliberately excludes fixed-fee compatibility, minimums, caps, tenant overrides
> and deal-type rates.
>
> The previously planned generic fixed/rate/minimum/cap policy model is rejected. Payment persists one
> immutable percentage configuration row per pricing revision, and each payer commitment receives a
> Payment-issued commission binding referencing that revision. Completed financial operations
> keep their actual money, Stripe and ledger facts.

## 1. Locked product and architecture decisions

### 1.1 Commercial model

- one universal platform commission rate applies to all four B2B deal types;
- the commission base is the final gross owed to the payee, not total ticket sales;
- the commission is a payer surcharge: payer total is gross plus commission and the payee receives
  the full agreed gross;
- the advertised percentage is inclusive of any VAT Concertable must account for, so a tax-status
  change does not alter a price already accepted by the payer;
- launch is GBP-only and all financial APIs and persistence use integer minor units;
- the exact launch rate remains a business configuration value to set before Phase 1 is deployed. It
  is not a reason to add multiple pricing modes.

### 1.2 Ownership boundary

B2B owns four pure, keyed final-gross calculations:

```text
FlatFee gross       = agreed artist payment
VenueHire gross     = agreed venue-hire payment
DoorSplit gross     = artist percentage × eligible takings
GuaranteePlus gross = guarantee + (artist percentage × eligible takings)
```

`GuaranteePlus` is the commercial formula currently carried by the `Versus` deal type. Payer-facing
copy must say “guarantee plus percentage”, never “whichever is greater”. Renaming the published
`DealType.Versus` wire value is not required to implement commission and would be a separate breaking
package cut-over.

Implement the four calculations as four strategies behind a closed-key resolver such as
`ISettlementGrossCalculator`, following `api/agents/CODE_PATTERNS.md`. FlatFee and VenueHire remain
separate strategies even though both currently return a fixed term: their payer, payee and deal
semantics differ. The strategies are pure domain logic. Whether the interface is consumed by the
application workflow or the calculation is moved onto an appropriate DDD value object can be decided
within Phase 2; the invariants are four deal-specific calculations, one result, and no Payment concern
inside them.

Payment owns one deal-type-agnostic calculation:

```text
commission minor = rate percentage applied to gross minor with round-half-up
payer total       = gross minor + commission minor
payee amount      = gross minor
```

Payment must not receive a caller-supplied rate and must never branch on B2B `DealType`.

### 1.3 Why the base is final deal gross

A percentage of ticket sales is only reliable when the platform owns the ticket checkout. It also
misprices revenue-share deals: a platform fee calculated against £1,000 of takings bears no consistent
relationship to a payee receiving, for example, a 20% £200 share. Commissioning the £200 settlement
charges for the payment Concertable actually facilitates.

For Concertable ticket sales, B2B uses its own authoritative sales data. External takings remain
venue-declared for launch. A false declaration changes the artist entitlement as well as Concertable's
commission, so it is a deal-settlement dispute rather than a Payment pricing problem. The UI must label
external takings as declared, require venue attestation, show the artist the calculation and provide a
dispute route. A future ticketing import may strengthen the input without changing the commission model.

## 2. Designs considered

| Design | Result |
|---|---|
| keep the flat £10 fee | rejected: temporary pricing unrelated to transaction value |
| commission total ticket/door takings | rejected: unverifiable for external sales and disproportionate to the actual settlement |
| vary rates by deal type | rejected: adds pricing policy and Payment domain knowledge without a launch requirement |
| deduct commission from the payee | rejected: changes the negotiated gross and weakens “what you agree is what you receive” |
| generic fixed/rate/minimum/cap policies | rejected: speculative product surface and persistence |
| resolve the live rate only when charging | rejected: delayed bookings could be charged on terms the payer never accepted |
| copy the current rate into B2B | rejected: B2B could become a second pricing authority or submit a favourable rate |
| copy the percentage into every binding | rejected: repeats identical configuration terms and conflates a pricing revision with a payer commitment |
| generic policy catalogue referenced directly by every application | rejected: exposes speculative pricing modes and binds pricing before the payer commitment |
| immutable percentage configuration revision referenced by a Payment-issued binding | selected: declares each rate once while separately recording who accepted it, for which obligation and Stripe context |

## 3. Rate selection and binding

### 3.1 Current rate

Replace the flat-fee setting with one validated current Payment configuration:

```text
PlatformCommission
  ConfigurationId  required unique non-empty Guid
  RatePercentage   required decimal, greater than 0 and no more than 100, up to 4 decimal places
```

This is not a generic policy engine: there is no fixed component, minimum, cap, tenant override,
deal-type selector, expiry or mutable status. Local appsettings and deployed Azure App Configuration
contain only the current values. Payment validates them on startup and inserts the configuration into
its immutable SQL history if the ID is new. Reusing an existing ID with a different percentage fails
startup.

Changing the rate deploys one new ID and percentage in Azure configuration. The previous Azure
values are replaced while their immutable SQL rows remain for existing bindings. During a rolling
deployment, preview and commitment commands compare the configuration ID the payer reviewed with the
instance processing the commitment; a mismatch returns `pricing_changed` before Stripe or domain
mutation, forcing a refresh. After commitment, calculations load the binding's referenced SQL
configuration rather than the current Azure configuration.

### 3.2 Binding point

The rate becomes binding when the payer accepts the financial obligation, not when an opportunity or
unaccepted application is created:

| Deal | Payer | Binding point |
|---|---|---|
| FlatFee | venue | Confirm & Pay, when Payment creates the manual-capture hold |
| VenueHire | artist | Authorise & Apply, when the SetupIntent/future charge mandate is accepted |
| DoorSplit | venue | booking acceptance, when the SetupIntent/future charge mandate is accepted |
| Guarantee Plus (`Versus`) | venue | booking acceptance, when the SetupIntent/future charge mandate is accepted |

An unaccepted application takes the then-current rate when it reaches the binding point. A booking
with a Payment binding retains its referenced configuration through settlement even if the
current rate changes later. Reopening a checkout before commitment shows the current rate; it does not
reserve an older rate merely because a page was visited.

### 3.3 Payment commission binding

Add `CommissionBindingEntity` in Payment:

```text
Id                         Guid, generated by Payment
CommissionConfigurationId  required foreign key to an immutable Payment configuration row
Currency                   immutable payment currency
ExternalReference          immutable application/booking reference
PayerReference             immutable payer/customer reference
BoundAt                    DateTimeOffset
StripePaymentIntentId      nullable
StripeSetupIntentId        nullable
```

Require a unique operation identity appropriate to the existing payment journey so retries are
idempotent. A binding can only be consumed by a payment with the same external reference,
payer, bound currency and Stripe intent/mandate. It cannot be rebound or supplied for another
booking. Many bindings can reference one revision; the binding stores its payment currency but does
not duplicate the percentage. Payment stores each deployed configuration once in SQL so historical
bindings remain
resolvable after Azure configuration moves to the next value.

B2B persists only the opaque `CommissionBindingId` on the application/booking path that already
owns the payer commitment. It does not persist a second rate, policy terms or calculated commission.
Signed booking terms continue to own the deal formula; the Payment binding owns the platform
rate commitment.

## 4. Values persisted and derived

### 4.1 B2B

Continue to store the agreed deal terms and signed agreement snapshot. Do not duplicate them into a
pricing table.

For DoorSplit and Guarantee Plus:

- Concertable sales remain derived from authoritative ticket records;
- external takings remain the venue declaration;
- after the payer reviews the exact settlement, persist the eligible-takings inputs and
  `FinalSettlementGrossMinor` atomically;
- the completion worker reads that frozen gross rather than recalculating against mutable sales or
  declaration data.

The frozen gross is justified: it is the amount the payer reviewed, the artist entitlement and the
input to an asynchronous charge. FlatFee and VenueHire gross remain safely derivable from immutable
agreed terms and do not need another snapshot in B2B.

### 4.2 Payment

Keep the existing actual `PlatformFee` snapshots on escrow and settlement records, renamed to
commission terminology where the package cut-over permits. Each money-moving record must retain:

```text
CommissionBindingId
Currency
PayeeGrossMinor
CommissionGrossMinor
CommissionNetMinor
CommissionVatMinor
PayerTotalMinor
Stripe charge / PaymentIntent / transfer identifiers
status and timestamps
```

`PayerTotalMinor` is mathematically derivable but remains an actual transaction fact for Stripe
reconciliation. Commission net and VAT are accounting facts, not pricing-policy duplication. The
ledger continues to post from transaction snapshots and never recalculates an old rate.

The configuration referenced by the binding explains the historical price commitment; the
settlement/escrow row explains what was actually charged and transferred. Both are required.

## 5. Currency, rounding and tax

- GBP is the only accepted commission currency at launch; reject mismatches before Stripe calls.
- Represent rates as validated decimal Percentage value objects. Apply the percentage to integer
  minor-unit gross and round once using round-half-up to the nearest minor unit.
- Each B2B strategy produces one final gross in minor units. Payment commissions that combined value
  once; it does not separately round a guarantee and revenue-share commission.
- The displayed rate is VAT-inclusive. When Concertable is not VAT registered,
  `CommissionNetMinor == CommissionGrossMinor` and VAT is zero.
- When VAT applies, decompose the already-calculated commission gross using the tax rate effective for
  the platform-fee supply. Persist that tax rate and the exact net/VAT split on the transaction and
  post VAT to a liability ledger account rather than platform revenue.
- Confirm the platform-fee tax point, invoice requirements and VAT registration transition with the
  accountant before production activation. This validation may change decomposition timing, but must
  not silently change the payer's accepted commission gross.
- The payee's FlatFee/VenueHire/DoorSplit/Guarantee Plus invoice and VAT treatment remain separate
  from Concertable's own commission supply to the payer.

## 6. Refunds, failures, release and disputes

### 6.1 Refund rule

Refund commission in the same proportion as payee gross:

```text
cumulative commission refund
  = round-half-up(original commission × cumulative gross refund / original gross)

this refund's commission
  = cumulative commission refund - commission already refunded
```

A full gross refund therefore returns the full commission. A partial gross refund returns the
proportional commission without cumulative rounding drift. Reverse VAT proportionally from the stored
commission tax snapshot.

Replace the single-refund assumption with one immutable Payment refund row per Stripe refund:

```text
PaymentRefundEntity
  Id
  SettlementOrEscrowId
  StripeRefundId
  GrossRefundedMinor
  CommissionRefundedMinor
  CommissionVatReversedMinor
  PayerTotalRefundedMinor
  Status
  CreatedAt / CompletedAt
```

Payment accepts the gross amount to reverse, derives the commission from stored transaction facts,
and enforces cumulative limits. B2B never submits a commission refund.

### 6.2 Journey behaviour

- a failed or abandoned payment creates no revenue posting and does not consume a different rate on
  retry after the payer has bound a commission calculation;
- an escrow refund before release refunds the payer total according to the rule above;
- escrow release transfers the stored payee gross and recognizes the stored commission according to
  the existing ledger timing; it never recalculates the rate;
- a refund after transfer reverses/recoveries the relevant connected-account amount using the Stripe
  flow's required transfer reversal;
- a dispute is recorded against the original charged total. Payment posts dispute and recovery facts
  from Stripe webhooks, uses the stored gross/commission allocation and does not ask B2B to reconstruct
  pricing;
- fees or losses Stripe does not return are separate platform costs, not silently deducted from the
  payee's agreed gross.

## 7. Stripe model

Keep the existing Connect money flows:

- destination-style direct payments charge payer total and transfer payee gross;
- escrow charges payer total to the platform flow and later transfers payee gross;
- Concertable retains the difference and records it in the Payment ledger.

Do not migrate solely to Stripe `application_fee_amount` in this work. Stripe application fees provide
useful per-charge reporting and optional proportional fee refunds for destination charges, but
Concertable also uses separate charge/transfer escrow semantics where refunds and transfer reversals
must be coordinated explicitly. A mixed application-fee/non-application-fee accounting model would
not remove the need for Concertable's persisted transaction and ledger facts.

Stripe guidance also makes the platform responsible for disputes on these indirect charge models.
The ledger and webhook work above is therefore required regardless of whether Stripe labels the
retained amount an application fee.

Primary references:

- Stripe destination charges and application fees:
  https://docs.stripe.com/connect/destination-charges
- Stripe separate charges and transfers:
  https://docs.stripe.com/connect/separate-charges-and-transfers?locale=en-GB
- Stripe refunds:
  https://docs.stripe.com/api/refunds/create
- Stripe Connect disputes:
  https://docs.stripe.com/connect/disputes?locale=en-GB
- HMRC commission as the agent's own supply:
  https://www.gov.uk/hmrc-internal-manuals/vat-valuation/vatval11700

## 8. Payment APIs and authority enforcement

Additive Payment client/protobuf capabilities must cover:

```text
PreviewCommission(gross, currency)
  -> commissionConfigurationId, ratePercentage, gross, commission, payerTotal

CreateOrBindCommission(
  externalReference, payerReference, currency,
  reviewedCommissionConfigurationId, Stripe intent context)
  -> bindingId, referenced configuration and exact amounts when gross is known

CalculateBoundCommission(bindingId, gross, currency)
  -> referenced configuration, gross, commission, payerTotal
```

The binding-aware hold, capture, deposit and direct-pay methods accept a binding ID and gross,
not a rate. Payment:

1. loads the binding;
2. loads its immutable commission configuration;
3. verifies payer, external reference, currency and Stripe intent context;
4. calculates from the referenced rate;
5. performs the Stripe action;
6. persists actual facts and posts the ledger atomically/idempotently.

`CreateOrBind` is the sole commitment boundary that accepts and validates payer-reviewed exact gross,
commission and total. Later bound calculation and money-movement calls accept the binding ID and gross, never
caller-supplied commission or payer-total values; Payment derives both from the immutable binding.

Unknown, missing, mismatched or stale pricing fails before money movement. New protobuf methods must be
distinct from legacy ones: an older Payment server must return `UNIMPLEMENTED`, not ignore a new field
and execute the £10 path.

## 9. Checkout disclosure

| Surface | Required disclosure |
|---|---|
| FlatFee venue checkout | exact artist gross, commission percentage and amount, VAT wording, exact total |
| VenueHire artist apply checkout | exact venue gross, commission percentage and amount, VAT wording, exact future total |
| DoorSplit venue accept checkout | artist share formula, eligible-takings definition, commission percentage applied to final artist gross, worked example, warning that exact total follows after the event |
| Guarantee Plus venue accept checkout | guarantee-plus-share formula, eligible-takings definition, commission percentage applied to the combined final artist gross, worked example, deferred-total warning |
| DoorSplit/Guarantee Plus final review | Concertable sales, venue-declared external takings, artist calculation, exact frozen gross, commission and payer total, followed by explicit confirmation |

The artist must also be able to see venue-declared takings and the resulting payee gross. Editing
takings invalidates the exact review. No financial action is enabled while pricing is unavailable or
stale, and no error path displays an invented zero.

Browser DTOs use integer minor units and currency. JavaScript formats authoritative values but never
calculates commission.

## 10. Package, migration and deployment sequence

This is an expand → publish → sync → consume → contract cut-over. Do not attempt to compile B2B against
unpublished Payment package source.

### Phase 1 — Payment percentage expansion

- [x] Add one validated current percentage configuration, immutable SQL configuration history,
  calculation and binding persistence by configuration ID only.
- [x] Add additive preview/bind/bound-calculation contracts and distinct binding-aware
  money-movement RPCs.
- [x] Add transaction tax facts, multi-refund persistence and proportional refund logic.
- [x] Keep the existing £10 RPCs only as the temporary expansion seam; do not model £10 as a supported
  commission mode in new code.
- [x] Re-scaffold Payment migrations.
- [x] Build `api/Concertable.slnx`; run Payment unit and integration tests.
- [x] Commit with `Skip-E2E: true`.
- [x] **Hard stop:** merge, let packages publish, own the generated platform-sync PR to green/merged,
  and deploy the expanded Payment runtime before starting the consumer phase.

### Phase 1b — Deferred binding consumption seam

- [x] Confine payer-reviewed exact amount validation to `CreateOrBind`, the payer commitment boundary.
- [x] Remove caller-supplied commission and payer total from every later bound calculation and money-movement
  API; Payment calculates them internally from the immutable binding and caller-owned gross.
- [x] Resolve review finding OWN1: keep one current {ConfigurationId, RatePercentage} in Azure
  configuration, insert each deployed percentage once into immutable Payment SQL history, and persist
  only its foreign key on bindings. Currency belongs to the binding, not the percentage configuration.
- [x] Verify OWN1 on current `origin/main`: Payment unit tests (141 passed),
  Payment integration tests (7 passed), no pending Payment model changes,
  `dotnet build api/Concertable.slnx` (0 errors), and the standalone Payment carve (0 errors).
- [ ] **Hard stop:** merge, publish, own platform sync to green and deploy the updated Payment runtime
  before Phase 2 consumes the corrected binding-owned surface.

### Phase 2 — B2B gross ownership and percentage cut-over

Start from updated `origin/main` after Phase 1b's platform sync.

1. Establish the four keyed pure gross strategies and exhaustive formula/rounding tests.
2. Persist only `CommissionBindingId`; add the frozen final-gross snapshot for deferred deals.
3. Bind the rate at each payer commitment point and route all four payment journeys through the new
   Payment methods.
4. Add exact and deferred pricing DTOs, final takings review/attestation and fail-closed error mapping.
5. Implement payer and artist disclosures in the manager SPAs.
6. Re-scaffold the Concert model.
7. Build the solution and all manager SPAs; run affected B2B and Payment unit/integration tests. This
   phase changes payment behaviour, so the merge queue remains the E2E gate.
8. Update this plan and launch trackers in the implementation commit.
9. **Hard stop:** merge and own publish/platform-sync before removing legacy Payment APIs.

### Phase 3 — Remove the temporary £10 model

Start after Phase 2 and its platform sync are green.

1. Prove no consumer calls the legacy fixed-fee RPCs or reads the old flat-fee configuration.
2. Remove the legacy RPCs, client methods, options and fixed-fee-only tests.
3. Rename remaining internal `PlatformFee` identifiers to percentage-commission language where that
   can be completed as one safe package cut-over; never discard actual accounting snapshots.
4. Re-scaffold affected Payment models if required.
5. Build and run affected Payment/B2B unit and integration tests.
6. Merge, publish and own the final platform-sync PR to green. Fix every consumer in that sync before
   considering the cut-over complete.
7. Mark pricing transparency complete in `LAUNCH_PLAN.md` and `LAUNCH_CHECKLIST.md`.
8. Delete this plan in the final verified implementation commit.
9. **Hard stop:** hand off the completed feature for review; do not begin marketplace work.

## 11. Verification coverage

### B2B

- one strategy resolves for each and only each supported deal type;
- FlatFee and VenueHire return their immutable agreed gross;
- DoorSplit uses artist percentage × eligible takings;
- Guarantee Plus uses guarantee + artist percentage × eligible takings;
- external takings attestation and exact review are required before deferred settlement;
- changing takings invalidates a prior review;
- the frozen reviewed gross is the value sent by the completion worker;
- no B2B request or database field can set or override a commission rate.

### Payment

- configuration rejects zero, out-of-range or over-precision percentages;
- commission APIs reject non-GBP currency before Stripe calls;
- the current Azure configuration is validated and inserted into immutable SQL history at startup;
- reusing an ID with a different percentage fails startup, and referenced SQL revisions remain
  available after Azure configuration changes;
- calculation applies the decimal Percentage value object to integer minor units with the documented half-up rule;
- binding creation is idempotent and references the authoritative current configuration;
- any number of bindings can reference one immutable configuration without copying its terms into SQL;
- a binding cannot be reused for another payer, reference, currency or Stripe intent;
- a later current-rate change does not affect an existing binding;
- preview/configuration-ID races fail before Stripe calls;
- all four money paths charge gross plus commission and transfer/release gross;
- transaction, Stripe metadata and ledger values reconcile;
- VAT decomposition posts tax separately without changing commission gross;
- multiple partial refunds preserve cumulative totals and a full refund returns the remaining exact
  gross, commission and VAT;
- failed payments, releases, post-transfer refunds and disputes use stored facts rather than
  recalculation;
- legacy £10 methods are absent after Phase 3.

### UI/integration examples

At a representative 5% test rate:

```text
FlatFee:       £500 gross -> £25 commission -> £525 payer total
VenueHire:     £400 gross -> £20 commission -> £420 payer total
DoorSplit:     70% × £1,000 = £700 gross -> £35 commission -> £735 total
GuaranteePlus: £100 + (70% × £1,000) = £800 gross -> £40 commission -> £840 total
```

Tests use a representative rate; they do not set the production launch rate.

## 12. Completion criteria

- the temporary £10 model and generic multi-mode policy proposal are absent;
- all four B2B deal types calculate one final payee gross through deal-owned pure logic;
- Payment applies one authoritative universal percentage to that gross;
- each rate revision exists once as an immutable Payment configuration and bindings reference it;
- every delayed commitment retains its historically bound configuration;
- unaccepted applications receive the current rate while accepted bookings retain their bound rate;
- payer, payee and deferred-review surfaces disclose the required formula or exact amounts;
- transaction, refund, VAT, Stripe and ledger facts are durable and reconcilable;
- no speculative pricing feature or duplicated B2B rate data remains;
- all package publish/platform-sync gates and affected verification gates are green;
- launch trackers describe the percentage model;
- this plan is deleted with the final verified implementation phase.
