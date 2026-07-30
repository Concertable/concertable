# Pricing transparency for the Payment-owned platform fee

> **Active launch plan.** PR #209 (`Feature/PlatformCommission`) and PR #223
> (`Feature/PaymentLedger`) are merged. Payment charges a real, fail-closed, flat **£10** platform fee
> on every B2B settlement and records the retained fee in its ledger. What remains is to version that
> Payment-owned policy, bind the version disclosed to each application, and show the payer the charge
> before commitment across all four contract types.
>
> The rejected design created a `PlatformFeeQuote` row per application. None of that implementation is
> retained. A pricing revision creates one immutable policy row; any number of applications may bind it.

## 1. Current truth and launch decision

- `PlatformFeeOptions` is internal to Payment and is validated on startup. Payment Web and Workers both
  configure `Fee = 10`; a missing value does not fall back to zero.
- `ManagerPaymentService` and `EscrowService` charge `gross + platform fee`, transfer or release `gross`
  to the payee, and persist the actual fee on `SettlementTransactionEntity` or `EscrowEntity`.
- the Payment ledger posts the retained fee as platform revenue and reconciles each settlement to zero;
- B2B has no live pricing config and no Payment contract for loading the authoritative fee;
- manager checkout and door-takings surfaces do not yet show gross, platform fee and total together.

The launch price remains:

```text
payer charge = gross settlement + £10 platform fee
payee receipt = gross settlement
Concertable retention = £10 platform fee
```

FlatFee, DoorSplit and Versus are venue-to-artist payments. VenueHire is an artist-to-venue payment. The
fee follows the payer; no deal-type platform-fee calculator belongs in B2B.

## 2. Scope and non-goals

### In scope

- append-only immutable Payment-owned platform-fee policy versions;
- one current flat-fee policy revision, shared by every application bound while it is current;
- Payment-owned fee calculation with a model that can represent a fixed component, percentage,
  minimum and cap without another persistence redesign;
- binding the disclosed policy ID in B2B and reloading that exact policy in Payment when money moves;
- retaining the policy ID and actual calculated fee on Payment escrow/settlement records;
- carrying the policy ID in Stripe metadata;
- gross, platform-fee and total disclosure at every payer commitment point;
- exact fixed-price disclosure for FlatFee and VenueHire;
- formula disclosure at DoorSplit/Versus acceptance and an exact reviewed charge at door declaration;
- additive Payment client/protobuf expansion, B2B persistence and APIs, manager UI and tests.

### Explicit non-goals

- activating percentage/minimum/cap pricing for launch; the active revision remains flat £10;
- a policy row per tenant, payer, application, checkout or promotion;
- an expiry/refresh lifecycle for an application-bound policy;
- moving pricing configuration or fee calculation into B2B;
- customer ticket checkout or customer marketplace pricing;
- changing payer direction, Stripe Connect flows, refunds, ledger accounting or invoice VAT;
- changing the Versus formula: it is **guarantee plus door percentage**;
- removing legacy Payment RPCs in the expansion. Contraction is a later breaking package cut-over.

## 3. Payer journeys and commitment points

| Contract | Payer | Last payer-present commitment point | Disclosure |
|---|---|---|---|
| **FlatFee** | venue | **Confirm & Pay** on `VenueAcceptCheckoutPage`, before the manual-capture hold | artist gross, policy fee, exact total held/charged |
| **VenueHire** | artist | **Authorise & Apply** on `ArtistApplyCheckoutPage`, before the later off-session deposit | venue gross, policy fee, exact future total |
| **DoorSplit** | venue | formula commitment at accept checkout; exact commitment before confirming door takings | artist-share formula and fee policy at accept; exact gross, fee and total at declaration |
| **Versus** | venue | same two points as DoorSplit | guarantee-plus-percentage formula and fee policy at accept; exact gross, fee and total at declaration |

For DoorSplit and Versus, total takings are Concertable ticket sales plus declared external takings. The
exact gross uses the existing `IArtistShareCalculator` strategies:

```text
DoorSplit gross = total takings × artist percentage
Versus gross = guarantee + (total takings × artist percentage)
```

`AcceptApplicationPage` remains a review/routing page. FlatFee, DoorSplit and Versus disclose pricing on
the checkout it opens. VenueHire binds the policy when the artist applies because the venue later accepts
without the payer present.

## 4. Architecture decision: shared immutable policy versions

### 4.1 Designs considered

| Design | Problem | Decision |
|---|---|---|
| duplicate a live fee in B2B | creates split-brain pricing authority | rejected |
| call `GetCurrentFee` when charging | a revision can change after disclosure | rejected |
| create a Payment quote row per application | duplicates identical pricing data and invents quote lifecycle/ownership for a global price | rejected |
| persist one immutable Payment policy row per pricing revision and bind its ID in B2B | keeps one authority and makes historical charges reproducible without per-application Payment state | **selected** |

Payment is an adapter service, so B2B may call it synchronously through the published
`Concertable.Payment.Client` package. B2B stores an opaque policy ID, never Payment configuration.

### 4.2 Policy row and calculation

Add `PlatformFeePolicyEntity` in Payment with:

- opaque `Guid Id`, supplied by the configured revision rather than generated per request;
- `Currency`;
- non-negative `long FixedAmountMinor`;
- non-negative `int RateBasisPoints`;
- nullable non-negative `long MinimumAmountMinor`;
- nullable non-negative `long MaximumAmountMinor`;
- `DateTimeOffset CreatedAt`.

The one calculation is owned by Payment:

```text
raw fee = fixed amount + (gross minor × rate basis points / 10,000)
rounded fee = round raw fee to minor units using the documented Payment rounding rule
actual fee = apply optional minimum, then optional cap
```

Validation requires cap ≥ minimum and all policy money to use the policy currency. The launch revision
is `fixed=1000`, `rate=0`, no minimum and no cap. A later 5%-with-£10-floor-and-cap revision is another
row with `fixed=0`, `rate=500`, `minimum=1000` and the chosen cap. Existing rows never change.

The domain exposes no mutation methods and the repository exposes add/get only. Payment rejects tracked
updates or deletes of policy rows. Tests prove an existing ID cannot be redefined with different terms.

### 4.3 Selecting and bootstrapping the current revision

Replace the scalar `PlatformFee:Fee` settings with a validated current-policy definition containing the
stable policy ID and its terms. Payment Web and Workers run an idempotent bootstrap after migration:

1. if the configured ID is absent, insert that policy once;
2. if the ID exists with identical terms, continue;
3. if the ID exists with different terms, fail startup;
4. never update or delete an older row.

A pricing revision is therefore an explicit new ID plus new terms. Changing terms while reusing an ID
cannot silently rewrite history. During a rolling deployment, an old and new current revision may both
be issued briefly; either remains chargeable because charging resolves the bound ID, not the local
process's current setting.

Legacy Payment calls select the current policy internally during the expansion, preserving their current
behaviour while removing direct fee reads from `ManagerPaymentService` and `EscrowService`.

### 4.4 Public policy client

Add an additive `IPlatformFeePolicyClient` to `Concertable.Payment.Client`:

```csharp
Task<PlatformFeePolicy> GetCurrentAsync(CancellationToken ct = default);
Task<PlatformFeePolicy> GetAsync(Guid policyId, CancellationToken ct = default);
Task<PlatformFeeCalculation> CalculateAsync(
    Guid policyId,
    decimal gross,
    CancellationToken ct = default);
```

`PlatformFeePolicy` carries the immutable ID, currency, fixed minor amount, rate basis points, optional
minimum/cap and creation time. `PlatformFeeCalculation` carries the policy ID, gross, actual fee and
total in integer minor units plus currency. B2B never reimplements rounding, minimum or cap rules.

The protobuf adds a separate `PlatformFeePolicy` service with current/get/calculate RPCs. Unknown IDs,
invalid gross and currency incompatibility fail closed.

### 4.5 Policy-aware money movement and deployment safety

Add policy-aware overloads to `IManagerPaymentClient` and `IEscrowClient`, but map them to **new protobuf
RPC methods**, not optional fields on the existing methods:

- policy-aware create-hold for FlatFee;
- policy-aware capture for FlatFee;
- policy-aware deposit for VenueHire;
- policy-aware direct pay for DoorSplit/Versus.

An older Payment server ignores unknown protobuf fields, so adding `platform_fee_policy_id` to existing
requests would be unsafe: it could charge the live fee after B2B believed the policy was bound. A new RPC
is intentionally unavailable on an older server and therefore returns `UNIMPLEMENTED` before Stripe is
called. B2B maps that deployment mismatch to pricing unavailable.

Each policy-aware server path:

1. requires a non-empty policy ID;
2. reloads the immutable policy from Payment's database;
3. calculates the fee from the operation gross through the shared Payment calculator;
4. validates policy/gross currency;
5. puts `PaymentMetadataKeys.PlatformFeePolicyId` in Stripe metadata;
6. for capture, requires the supplied ID to match the ID placed on the held PaymentIntent;
7. performs the Stripe operation for `gross + actual fee`;
8. persists `PlatformFeePolicyId` and the existing actual-fee snapshot on `EscrowEntity` or
   `SettlementTransactionEntity`.

Setup/verify sessions do not move money; B2B adds the same policy metadata key through their existing
metadata dictionaries. Refund and release arithmetic continues to use the persisted actual fee snapshot,
never a policy recalculation. Ledger postings also use the snapshot.

Legacy RPCs remain during migration. A later removal is a breaking expand/publish/sync/consumer/contract
sequence and is outside this plan.

### 4.6 Package and runtime sequence

The Payment expansion must land before any B2B source references it:

1. merge Payment policy persistence, client contracts, new RPCs and server implementation while B2B
   remains on legacy calls;
2. let `publish-packages` publish the new `Concertable.Payment.Client` and
   `Concertable.Payment.Contracts`;
3. own the generated `chore/platform-sync-*` PR until it is green and merged;
4. deploy/start the expanded Payment server in any environment before enabling the B2B consumer there;
5. create the B2B consumer branch from the updated `origin/main`, which now pins the published package;
6. only then compile B2B against `IPlatformFeePolicyClient` and the policy-aware overloads.

There is no Payment source project reference and no manual per-package version bump. The distinct RPCs
make a mistaken runtime order fail closed, but they do not remove the required deployment gate.

## 5. B2B binding, API and exact review

### 5.1 Bind the policy to the application

Add nullable `Guid? PlatformFeePolicyId` to `ApplicationEntity`. It is nullable only for the package
expansion/intermediate model and existing rows; every new priced transition requires a value.

- **FlatFee:** `HoldCheckoutStep` obtains the current policy once, attaches and saves its ID before the
  hold, calculates the exact disclosure through Payment, and uses the new policy-aware hold RPC.
  Reopening loads the attached policy by ID.
- **DoorSplit/Versus:** `VerifyCheckoutStep` obtains, attaches and saves the current policy before
  returning the verification session. Reopening loads the same policy.
- **VenueHire:** no application exists when apply checkout opens. The checkout returns the current policy
  ID. `ApplyRequest` echoes it; Apply requires it still to be current, then stores it on the new
  `PrepaidApplication`. If a revision changed meanwhile, return `pricing_changed` and make the payer
  review again. This prevents replaying an arbitrary older global policy without creating per-checkout
  Payment state.

FlatFee/DoorSplit/Versus Accept does not need a caller-supplied policy ID: the server already attached the
policy when it produced checkout. Bypassing checkout leaves the application unbound and Accept fails
before booking creation or money movement.

Carry the ID through `BookingSettlement` and the fixed-price accept steps so capture, deposit and direct
pay use the policy-aware Payment methods. Missing policy is a conflict/precondition failure; there is no
fallback to Payment's current revision.

Re-scaffold the Concert EF model after adding the field.

### 5.2 Checkout pricing DTO

Keep the existing `IPaymentAmount` union for deal gross/formula and add a separate discriminated pricing
disclosure to `Checkout`:

```text
ExactPrice
  platformFeePolicyId
  grossMinor
  platformFeeMinor
  totalMinor
  currency

DeferredPrice
  platformFeePolicyId
  fixedAmountMinor
  rateBasisPoints
  minimumAmountMinor?
  maximumAmountMinor?
  currency
```

FlatFee and VenueHire return `ExactPrice`. DoorSplit and Versus return `DeferredPrice`; their existing
`DoorSharePayment` / `GuaranteedDoorPayment` supplies the gross formula. The launch policy renders as a
fixed £10 addition, while the same DTO can later render percentage/minimum/cap terms.

All browser-facing money is integer minor units plus ISO currency. Do not encode unknown gross/total as
zero and do not calculate the platform fee in JavaScript.

### 5.3 Exact DoorSplit/Versus review

Add `ConcertActions.QuoteDoorRevenue` beside `DeclareDoorRevenue`, available under the same venue-owner,
ended, revenue-share, `Booked`, not-yet-declared conditions:

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
  platformFeePolicyId
```

Refactor the declaration guard/context load so quote and declare share tenant, deal, end-time and lifecycle
rules. Resolve gross with `IArtistShareCalculator`, then call Payment `CalculateAsync` with the
application-bound policy. Do not branch on `DealType` in the controller/service.

The final declaration echoes `PlatformFeePolicyId`, `ExpectedGrossMinor` and
`ExpectedPlatformFeeMinor`. The server requires the policy ID to match the application, recomputes gross
and fee, and returns `409 pricing_changed` if either expected value is stale. On success it persists the
external takings and reviewed gross atomically. `RevenueShareSettlementAmount` reads that persisted gross;
the completion worker passes the same policy ID and gross to Payment, which persists its own actual fee
snapshot when charging.

### 5.4 Error mapping

- Payment unavailable or policy RPC unimplemented: `503 pricing_unavailable`;
- missing application policy: `409 pricing_policy_required`;
- unknown policy or submitted/bound mismatch: `409 pricing_policy_mismatch`;
- VenueHire policy no longer current or stale door review: `409 pricing_changed`;
- invalid door input remains `400`;
- a worker policy failure follows the existing settlement-failure path and moves no money.

## 6. Manager-SPA design

Keep B2B pricing types/components in `app/web/b2b/shared`; do not widen customer/shared with manager
settlement concepts.

| Surface | Change |
|---|---|
| venue `VenueAcceptCheckoutPage`, **FlatFee** | “Artist gross fee”, “Concertable platform fee”, “Total charged”; disable **Confirm & Pay** until exact pricing is loaded |
| artist `ArtistApplyCheckoutPage`, **VenueHire** | “Venue gross hire fee”, “Concertable platform fee”, “Total charged if accepted”; submit the rendered policy ID |
| venue `VenueAcceptCheckoutPage`, **DoorSplit** | artist settlement formula, policy formula, and combined charge formula before **Confirm** |
| venue `VenueAcceptCheckoutPage`, **Versus** | guarantee-plus-percentage settlement formula, policy formula and combined charge formula; remove “whichever is greater” copy |
| venue `DeclareDoorRevenueButton` | input → **Review charge** → exact review → **Confirm takings & charge** |

Payee-only surfaces do not add a payer charge breakdown.

Loading and failure rules:

- keep checkout loading until both session and pricing are valid;
- never mount/enable Stripe submission without a disclosure;
- show a focused `role="alert"` with **Retry price** for `pricing_unavailable`;
- preserve signature/form state on retry;
- attached application policies never silently refresh;
- VenueHire `pricing_changed` retains the form but requires a fresh disclosure;
- editing door takings invalidates the review;
- generic failures never show £0 or leave a financial action enabled.

Render breakdowns as semantic `<dl>` elements, announce loading/refreshed totals/errors, preserve keyboard
focus, and use the shared `formatCurrency` for minor-unit amounts.

## 7. Tests and verification

### Payment

- bootstrap inserts one configured revision, is idempotent, and rejects same-ID/different-terms;
- policy rows cannot be updated or deleted;
- current/get return the immutable terms;
- calculation covers fixed-only plus percentage/minimum/cap order, rounding and validation;
- launch policy calculates £10 for different gross amounts;
- policy-aware hold/capture/deposit/pay reload by ID and never read the current revision for the fee;
- capture rejects a policy that differs from hold metadata;
- unknown/missing policy and currency mismatch perform no Stripe call;
- escrow/settlement policy ID, actual fee, charged total, payee amount and ledger posting agree;
- refund/release use the stored snapshot;
- client/protobuf mappings preserve IDs, minor units, basis points, optional bounds and currency;
- legacy methods remain functional during expansion.

### B2B Concert

- FlatFee and deferred checkouts attach once and reuse the application policy;
- VenueHire Apply binds only the policy just confirmed as current;
- priced transitions reject a missing policy before persistence/money movement;
- capture, deposit and payout pass the bound ID;
- exact DTOs use Payment calculations; deferred DTOs preserve policy terms;
- quote/declare share authorization and lifecycle gates;
- DoorSplit and Versus compute their existing gross formulas correctly;
- stale gross, fee or policy returns 409 without mutation;
- successful declaration persists reviewed gross;
- Payment unavailable/unimplemented maps to 503 and leaves domain state unchanged.

### Frontend/E2E examples

- FlatFee: £500 gross + £10 = £510 before venue confirmation;
- VenueHire: £300 gross + £10 = £310 before artist authorisation;
- DoorSplit: 70% formula + £10 at accept; £300 takings → £210 + £10 = £220 at review;
- Versus: £100 + 70%, never “whichever is greater”; £20 takings → £114 + £10 = £124;
- failure/stale-review paths keep financial actions unavailable.

Local behaviour-phase gates:

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

Run `api/initial-migrations.ps1` in each model-changing phase. Phase 1 has no consumer behaviour and uses
`Skip-E2E: true`. Phases 2 and 3 change payer-visible payment behaviour and do not use that trailer.
Per `plans/AGENTS.md`, the merge queue is the E2E gate; run local E2E only to diagnose a queue failure
through the matching E2E debug skill.

## 8. Independently shippable phases

### Phase 1 — Payment immutable policy expansion

1. Add policy entity/configuration/repository, immutable-write guard, current-policy options/bootstrap and
   the shared calculator; migrate legacy fee reads to the current policy.
2. Add policy current/get/calculate gRPC plus `IPlatformFeePolicyClient`.
3. Add distinct policy-aware hold/capture/deposit/pay RPCs and client overloads.
4. Persist policy IDs beside actual fee snapshots and add Stripe metadata/hold-match validation.
5. Re-scaffold Payment migrations; build and run Payment unit/integration tests.
6. Commit with `Skip-E2E: true`.
7. **Hard stop:** merge/publish, follow platform-sync to green, and ensure the expanded Payment runtime is
   available before any B2B consumer work.

### Phase 2 — Fixed-price binding and disclosure

Start on a fresh consumer branch from updated `origin/main`.

1. Add `ApplicationEntity.PlatformFeePolicyId`, exact/deferred pricing DTOs and Concert migration.
2. Bind FlatFee hold/capture and VenueHire setup/apply/deposit.
3. Implement FlatFee/VenueHire breakdowns and fail-closed UI behaviour.
4. Add Payment/B2B unit/integration coverage and FlatFee/VenueHire Reqnroll assertions.
5. Run the local gates; leave E2E to the merge queue.
6. **Hard stop:** commit the independently usable fixed-price slice and hand off Phase 3.

### Phase 3 — Deferred-price review and close-out

1. Bind DoorSplit/Versus verify checkout and direct payout.
2. Add door-price review, shared declaration guards, stale-review checks and persisted reviewed gross.
3. Implement deferred policy formula disclosure and exact two-step door review.
4. Correct Versus copy.
5. Add DoorSplit/Versus unit, integration and Reqnroll coverage; run local gates.
6. Update `LAUNCH_PLAN.md` and `LAUNCH_CHECKLIST.md` to mark pricing transparency complete.
7. Delete this plan in the final verified implementation commit.
8. **Hard stop:** final PR handoff; do not begin marketplace or activate a new pricing revision.

## 9. Completion criteria

- every B2B payer sees gross/formula, the bound policy fee/formula and total before commitment;
- one Payment policy row exists per pricing revision, never per application;
- B2B persists the policy ID it disclosed and Payment reloads that exact immutable row when charging;
- escrow/settlement and Stripe metadata retain the policy ID, while accounting uses the actual fee snapshot;
- config revisions cannot alter an application already bound to an older policy;
- unavailable, missing or stale pricing cannot fall through to a financial action;
- the flat launch policy and percentage/minimum/cap model use the same Payment calculation path;
- affected builds/tests and merge-queue E2E are green;
- launch trackers describe immutable policy versions rather than per-application quotes;
- this plan is deleted with the final verified phase.
