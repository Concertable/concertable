# The B2B ↔ Payment boundary: does a Stripe payment-method id belong in B2B?

Decision document, 2026-09-04. All Stripe quotes are verbatim from docs.stripe.com, fetched
2026-09-04, with the source page linked. Anything I concluded rather than read is marked
**Inferred**. Facts verified in the repo cite the file.

---

## Verdict

1. **No.** A `pm_` id must never be stored or relayed by B2B — not POSTed by the SPA, not
   persisted on `PrepaidApplication`/`DeferredBooking`, not carried on a bus contract. Stripe's
   own documented flow for saved cards is entirely server-authoritative: the merchant backend
   learns the pm id from Stripe (webhook → session → `setup_intent` → `payment_method`), and in
   every page checked the browser carries at most a Checkout **Session id**, never the pm id.
   Passing `payment_method` as a parameter is correct in exactly one place: **Payment calling
   Stripe** — because there the parameter belongs to Stripe's API, and Payment is the service
   that owns Stripe.
2. **The in-flight `Feature/payment-method-commitments` design is the correct end state — ship
   it.** Consumer-minted `PaymentOperationReference { operation_type, consumer_correlation }`,
   Payment privately resolving the pm from provider truth, `*ByReference` financial commands.
   It matches Stripe's documented model on every axis I checked (webhook race, idempotency,
   Connect topology, staleness recovery). The remaining judgement calls are the consumer
   migration, the legacy cull, and vocabulary — covered below.
3. **Driving the later charge off a ledger row was architecturally wrong**, and the branch
   already fixes it: `VerifyTransactionEntity` records history; the operational home for a
   provider object is the session-operation state (`PaymentSessionAttemptEntity` /
   `BindProviderObject` + the operation's payment-method column, now written from provider truth).
4. Three Stripe-correctness gaps stand regardless of the refactor:
   `PaymentMethodAllowRedisplayFilters = ["always", "limited", "unspecified"]` surfaces cards the
   customer never consented to see again; the variable-amount MIT **consent terms** (amount
   determination, timing, written record) are a documented obligation with no recorded artifact;
   and the legacy `"{identity}:{action}"` Stripe idempotency keys error if a retry ever changes
   params. All three are small, independently shippable fixes.

---

## 0. The ground moved — this judges the worktree, not the brief's snapshot

The brief's PART 2 describes `main`. `Feature/payment-method-commitments`
(`.worktrees/Feature-payment-method-commitments`, producer PR
[#933](https://github.com/Concertable/concertable/pull/933), reviewed and approved through
`6510ca80`) already delivers the producer side:

- `PaymentOperationReference { operation_type, consumer_correlation }` in `payment.proto`, with
  `SetupPaymentMethod` / `ValidatePaymentMethod` RPCs and `CaptureEscrowByReferenceCommand` /
  `DepositEscrowByReferenceCommand` on the bus; `ManagerPayUsingPaymentMethodRequest.payment_method`
  is now a reference, not a string.
- `PaymentOperationResolver` reconciles the referenced provider object on demand (PAY-001), keeps
  transient provider failures non-terminal (PAY-006), and the setup webhook/attempt state records
  the resolved pm privately.
- Per-attempt Stripe idempotency keys: `PaymentSessionIdempotencyKey(operationId, attemptId, revision)`.
- The raw-identifier surface (B2B's pm columns, `ApplyRequest`/`AcceptRequest.PaymentMethodId`, the
  pm id enriched into `PaymentSucceededEvent` metadata by `SetupIntentWebhookHandler`) remains
  **only** for package-compatible migration and is scheduled to die.

So questions 1, 2 and 4 of the brief are no longer "should we" but "was the call right" — and it
was. What follows substantiates that with Stripe's documentation and settles what's still open.

---

## 1. The pm id as a parameter

### What Stripe documents

The setup-mode guide ([payments/checkout/save-and-reuse](https://docs.stripe.com/payments/checkout/save-and-reuse))
labels every retrieval step **[Server-side]**:

> "**Asynchronously**: Handle `checkout.session.completed` webhooks, which contain a Session
> object. **Synchronously**: Obtain the Session ID from the `success_url` when a user redirects
> back to your site."
>
> "After you have retrieved the Session object, get the value of the `setup_intent` key, which is
> the ID for the SetupIntent created during the Checkout Session."
>
> "Using the `setup_intent` ID, retrieve the SetupIntent object. The returned object contains a
> `payment_method` ID that you can attach to a customer in the next step."

The only thing that transits the browser is the **session id**, inside a Stripe-constructed
redirect URL. An instruction for the client to relay the pm id to the merchant's backend was
**not found** in [payments/checkout/save-and-reuse](https://docs.stripe.com/payments/checkout/save-and-reuse)
(hosted variant) or [payments/save-and-reuse](https://docs.stripe.com/payments/save-and-reuse)
(elements variant) — a scoped absence claim over those pages, not all Stripe docs. In the elements
variant the client only ever receives a `client_secret` and confirms.

Stripe's off-session charge API does take the pm id as a parameter
([payments/save-and-reuse](https://docs.stripe.com/payments/save-and-reuse)):

> "use the ID of the `Customer` … and the `PaymentMethod` ID to create a `PaymentIntent` with the
> amount and currency of the payment … Set off_session to true … Set confirm to true … Set
> payment_method to the `PaymentMethod`'s ID."

### The distinction that settles it

The `payment_method` parameter exists on **Stripe's** API, for **Stripe's** direct caller — the
merchant-of-record's own payment layer, which in this system is `Concertable.Payment` and nothing
else. "Payment passes `payment_method` to `PaymentIntents.Create`" is the API working as designed.
"B2B persists `pm_…` on a booking entity and replays it weeks later over the bus" is a business
service holding another service's provider state, and it is wrong four separate ways:

- **Unverifiable.** B2B cannot validate a pm id — by its own architecture it never talks to
  Stripe. The SPA can POST any string: another customer's method, a method whose SetupIntent
  failed asynchronously, a detached method. Payment can validate; B2B can only trust.
- **Consent-blind.** The pm id carries no `allow_redisplay`, no mandate context, no record of what
  the customer agreed to (§5.4). Only the service that saw the SetupIntent complete knows that.
- **It races the webhook.** The client-relay path and the webhook path are two sources of truth
  for the same fact. Stripe's answer to that race is one idempotent server-side function (§5.2),
  not a second channel.
- **Provider lock-in on published contracts.** `DepositEscrowCommand.PaymentMethodId` makes a raw
  Stripe identifier part of B2B↔Payment's published vocabulary — swap or add a provider and the
  bus contract breaks.

**Inferred** (Stripe has no opinion on your service decomposition): the correct consumer-side
currency is a durable reference the consumer mints and Payment resolves — exactly
`PaymentOperationReference`. If a future product feature lets a user pick among several saved
cards, the pick-list and the selection are **Payment's** surface (Stripe's documented selection
step is "list the payment methods associated with your customer" — server-side); B2B may proxy an
opaque selection token within one request, but persists nothing.

---

## 2. Options for VenueHire's later off-session charge

Scored against the brief's axes. ✔ passes, ✖ fails, △ partial.

| | A. Status quo (SPA→B2B pm relay) | B. Payment-owned commitment by reference *(delivered)* | C. Charge the owner's "default" card | D. Payment-minted handle returned to B2B |
|---|---|---|---|---|
| Fifth deal type | ✖ per-deal pm plumbing | ✔ zero new leaves (see below) | ✔ but decorative setup | ✔ |
| Webhook race | ✖ two sources of truth | ✔ resolver reconciles provider truth | ✔ | ✔ |
| Connect topology | ✖ B2B can't know clone rules | ✔ Payment owns it | ✔ | ✔ |
| `allow_redisplay` consent | ✖ invisible to B2B | ✔ recorded where the setup completed | ✖ consent scope wrong | ✔ |
| Multiple saved cards | ✖ client picks, server trusts | ✔ the reference binds *the* method set up for this commitment | ✖ ambiguous | ✔ |
| Staleness / 402 | ✖ B2B stores what it can't refresh | ✔ typed rejection → re-setup flow | △ | ✔ |
| Extra cost | — | — | — | ✖ a second identity + a B2B persistence obligation |

**A — reject.** Every failure above, plus §1.

**B — adopt (it is the delivered design).** B2B derives the reference deterministically from what
it already owns — the operation kind from its closed enum, the correlation from the business
context (`opportunity:{id}`-shaped, opaque to Payment). The SPA passes **nothing** payment-shaped:
`ApplyRequest.PaymentMethodId` doesn't get replaced by a client-supplied reference, it gets
deleted — B2B mints the reference server-side from route + identity. Payment resolves the
committed method from its own operation state, reconciling Stripe directly when the webhook
hasn't landed (which is Stripe's own prescription for unordered events, §5.2).

**C — reject as the primary mechanism.** Stripe has no automatic default for one-off
PaymentIntents: `invoice_settings.default_payment_method` is defined as the "default payment
method for subscriptions and invoices"
([api/customers/object](https://docs.stripe.com/api/customers/object)), and the cards overview
says outright "To consider default payment methods in other scenarios, use custom code"
([payments/cards/overview](https://docs.stripe.com/payments/cards/overview)). Worse, the consent
recorded at setup is scoped to the agreement shown at setup (§5.4) — charging "whatever card is
newest" for a specific hire is exactly the reuse-beyond-consented-scope Stripe warns about. At
most this is an explicit, Payment-owned *fallback policy*, and that's a product decision.

**D — reject the return leg; it collapses into B.** B2B already mints the identity (the
correlation, and the UUIDv7 `operation_id`). A Payment-minted handle flowing back is a second
name for the same fact plus a persistence obligation in B2B that B doesn't have.

### Homes, unions, and the fifth deal type

B lives in **Home 1 + Home 2**: each contract arm declares `ExpectedFinancialOperation`
(compiler-forced), and the behavioural families (`IConfirm`, `ICancel`, `IComplete`) re-key by
`FinancialOperation`. **The re-keying insight is right — endorse it.** The capability partition
is more stable than the deal partition: `IConfirm` 4→3 leaves, `ICancel` 4→2, `IComplete` 4→2,
and a fifth `DealType` adds **zero** strategy leaves — it fails to compile until its arm declares
which operation it expects, then rides the existing operation-keyed families. That is the
acceptance test answered at its strongest: *it doesn't compile until someone classifies it
deliberately*. `IContractFactory` stays keyed by `DealType` — per-arm economics genuinely vary
per deal.

**No union survives this.** The old temptation — `Apply.Standard` vs `Apply.Prepaid(string
paymentMethodId)` — existed only because the prepaid arm carried a client-relayed `string?`. With
the relay gone, apply's call shape is identical across arms (`Apply(opportunityId, eSignature)`);
what varies is that the VenueHire strategy first validates its commitment
(`ValidatePaymentMethod`) — a collaborator difference with the same call shape, which is the
definition of a keyed strategy, not a union. The plan's own invariant already states this
("no union represents identical call shapes").

---

## 3. The leaks, one by one

| Leak | Disposition | Replacement vocabulary |
|---|---|---|
| `TransactionTypes.ApplicationApply` | **Delete.** Consumed by nothing; the webhook it named was being dropped on the floor. | none — the generic subsystem routes by operation binding, not metadata type |
| `TransactionTypes.ApplicationAccept` | **Retire with the bespoke path.** | `PaymentSessionKind.PaymentMethodVerification` + the consumer's reference |
| `PaymentMetadataKeys.{ApplicationId, OpportunityId, VenueManagerId}` | **Replace with agnostic keys.** | `contextId` (the column already got it right), `payerOwnerId`; the consumer encodes *what kind* of context inside its own correlation string |
| `FindHeldIntentAsync(payerId, applicationId)` | **Delete** — B2B round-tripping a provider object id is §1's disease in read form. | `CaptureEscrowByReferenceCommand` *(delivered)* |
| `DepositEscrowCommand.PaymentMethodId` | **Delete after cut-over.** | `DepositEscrowByReferenceCommand` *(delivered)* |
| `SetupIntentWebhookHandler` enriching `PaymentSucceededEvent` metadata with the raw pm id (`Webhook/SetupIntentWebhookHandler.cs:40`) | **Delete with the legacy path** — a `pm_` id on the integration bus is the same leak with a different transport. | consumers react to reference-scoped events |

On the brief's specific question — should `ApplicationApply`/`ApplicationAccept` become
**B2B-owned opaque strings**? Split the two jobs the `type` string was doing:

- **Routing to behaviour** inside Payment is Payment's closed vocabulary — the keyed handler /
  `PaymentSessionKind` set. A consumer cannot mint these: every value needs a handler, so the set
  is closed by Payment's composition root (and the factory throwing on an unregistered type is
  the runtime symptom of pretending otherwise).
- **Naming the consumer's context** is the consumer's opaque vocabulary — `operation_type` +
  `consumer_correlation` on the reference, which Payment stores and compares but never parses.
  This is exactly the split the delivered plan records ("B2B will own any closed operation enum
  and map it at its Payment adapter boundary").

So: yes for the *context* half (B2B-owned, opaque to Payment), no for the *behaviour* half
(Payment-owned, closed). The current constants conflated the two, which is why they read as leaks.

---

## 4. The two subsystems

**Adopt the generic one — the branch already did, and the judgement stands on merits.** The
bespoke `CreateSetupSession`/`CreateHoldSession`/`CreateVerifySession` path plus transaction-log
rows has no idempotent identity, no state machine, no reconciliation hook, and no home for the
resolved payment method; the generic `PaymentSessionOperations` subsystem has all four, and they
correspond one-to-one to obligations Stripe documents (§5.1–5.2).

**Driving a later charge off `VerifyTransactionEntity` is architecturally wrong — plainly.** A
ledger/audit row is an immutable record of what happened; it is not an operational read model. It
stores an *intent* id, not a method id; it can't represent "setup completed but not yet charged",
"charge pending retry", or "consent recorded"; and reading it to decide a future money movement
makes the audit trail load-bearing for the thing it's supposed to audit.
`PaymentSessionAttemptEntity.ProviderObjectId` — bound immutably via `BindProviderObject`, with
the payment-method identifier written from provider truth (webhook or reconciliation, per
PAY-001/PAY-002) — is the correct home. Keep `VerifyTransactionEntity` as ledger only.

One nuance the reviews already caught and I'll underline: the operation's method column must only
ever be populated from **provider truth** (webhook or a server-side retrieve). The pre-branch
schema had a `PaymentMethodId` column "only ever set from the inbound request" — that column was
a lie waiting to be believed.

---

## 5. Stripe correctness checklist for the target design

### 5.1 Idempotency (at-least-once bus + outbox, 24h key pruning)

> "You can remove keys from the system automatically after they're at least 24 hours old. We
> generate a new request if a key is reused after the original is pruned." —
> [api/idempotent_requests](https://docs.stripe.com/api/idempotent_requests)
>
> "The idempotency layer compares incoming parameters to those of the original request and errors
> if they're not the same to prevent accidental misuse." — same page
>
> "Stripe's idempotency works by saving the resulting status code and body of the first request
> made for any given idempotency key, regardless of whether it succeeds or fails. Subsequent
> requests with the same key return the same result, including `500` errors." — same page

Consequences, and where the branch stands:

- Stripe keys are a **short-window network-retry guard, never the durable dedupe**. The durable
  layer is Payment's own operation row keyed by the consumer's UUIDv7 `operation_id` — a
  redelivery three days later replays the recorded outcome from the row, not from Stripe. ✔ delivered.
- The key must be a **pure function of the attempt identity**, never of payload fields.
  `PaymentSessionIdempotencyKey(operationId, attemptId, revision)` is the right shape ✔. The
  legacy `new RequestOptions { IdempotencyKey = $"{identity}:{action}" }`
  (`Services/StripeRequestOptions.cs:49`) is not: no attempt/revision component, so a legitimate
  retry with any changed param errors, and a retry after 24h silently re-executes. **Migrate the
  legacy call sites onto the keyed shape during the cull.**
- A replayed **500** must be treated as retryable via a new revision, never terminal — consistent
  with the PAY-006 resolution (provider-unavailable stays pending).

### 5.2 The webhook race

> "Stripe doesn't guarantee the delivery of events in the order that they're generated." …
> "Track [event IDs] to identify duplicate deliveries instead. You can also use the API to
> retrieve any missing objects." — [webhooks](https://docs.stripe.com/webhooks)
>
> "Your `fulfill_checkout` function must: 1. Correctly handle being called multiple times with the
> same Checkout Session ID. … 3. Retrieve the Checkout Session from the API … 4. Check the
> payment_status property …" — [checkout/fulfillment](https://docs.stripe.com/checkout/fulfillment)
>
> "Checkout waits up to 10 seconds for your server to respond to the webhook event delivery
> before redirecting your customer." — same page

The documented pattern is *two racing triggers converging on one idempotent, re-retrieving
function* — and `PaymentOperationResolver` reconciling current provider truth on demand is
precisely that function generalized. B2B's post-checkout call (`ValidatePaymentMethod`) and the
webhook both land on the same operation state; whichever arrives first wins and the other is a
no-op. ✔ delivered (PAY-001 was exactly the finding that the pre-fix code trusted webhook order).
Ack-fast + event-id dedupe on the webhook endpoint is the standing obligation for the receiving
side.

### 5.3 Connect topology

The delivered flows create PaymentIntents **on the platform** with
`TransferData.Destination`/`OnBehalfOf` (`Services/StripePaymentIntentClient.cs:51,103`,
`StripeSessionClient.cs:167-168`) — destination charges. For that topology:

> "To create a destination charge, define both the customer and the price on the platform
> account. … The customer must exist within the platform account. When using destination charges,
> the platform is the *merchant of record*." —
> [connect/subscriptions](https://docs.stripe.com/connect/subscriptions)
>
> "Cloning saved payment methods is only relevant when creating direct charges on connected
> accounts. It's not necessary when making charges on your platform account." —
> [connect/cloning-customers-across-accounts](https://docs.stripe.com/connect/cloning-customers-across-accounts)

So: **a PaymentMethod saved against the platform Customer is charged directly on a destination
charge — no cloning, nothing to do.** Cloning
([connect/direct-charges-multiple-accounts](https://docs.stripe.com/connect/direct-charges-multiple-accounts))
only enters if Concertable ever switches to direct charges on connected accounts — and cloned
methods are card/us_bank_account-only, unsynced, and consumed per charge unless re-attached.

`on_behalf_of` does **not** move the charge or restrict card reuse; it changes settlement
country/currency, fee schedule, statement descriptor, merchant of record — and therefore which
account's *payment-method configuration* applies:

> "For charges where the connected account is the MoR, including *direct charges* and *indirect
> charges* that have `on_behalf_of` set, the payment method must be enabled on the connected
> account." — [connect/manage-payment-methods](https://docs.stripe.com/connect/manage-payment-methods)

**Standing guard for the future:** for mandate-based methods, "If a mandate is authorized for a
PaymentIntent or SetupIntent on_behalf_of a connected account, you can't use that mandate with a
different connected account"
([payments/payment-methods/payment-method-connect-support](https://docs.stripe.com/payments/payment-methods/payment-method-connect-support)),
and SEPA mandates can't be cloned at all. Cards are exempt today, but if a non-card method is
ever enabled, "one saved method reused across hires with different venues" breaks for those
methods. The commitment model already scopes a commitment per context, which is the safe shape —
keep it that way; don't ever generalize to "the owner's saved method" (Option C) partly for this
reason.

### 5.4 `allow_redisplay` and variable-amount MIT consent

> "This field indicates whether this payment method can be shown again to its customer in a
> checkout flow." — [api/payment_methods/object](https://docs.stripe.com/api/payment_methods/object)
>
> "If the customer leaves the checkbox unselected, the `allow_redisplay` value is set to
> `limited`. This means you can't use the payment method for future purchases—it's limited to the
> current subscription you're setting up." —
> [payments/save-customer-payment-methods](https://docs.stripe.com/payments/save-customer-payment-methods)
>
> "By default, only saved payment methods with 'allow_redisplay: 'always' are shown in Checkout."
> — [api/checkout/sessions/create](https://docs.stripe.com/api/checkout/sessions/create)

**Inferred** (and flagged as such by the research pass): `limited` is not an API-level charging
gate — the Setup Intents guide even distinguishes "payment methods saved only for offline usages"
via this field — but it *records the absence of reuse consent*, which makes charging a `limited`
method for a new purpose a compliance problem even where the API allows it.

**Concrete defect:** `StripeAccountClient.cs:268` sets
`PaymentMethodAllowRedisplayFilters = ["always", "limited", "unspecified"]`, overriding Stripe's
default to surface cards the customer never consented to see again. **Fix: `["always"]`** (or
drop the parameter — the default is already correct), unless a deliberate, recorded consent story
covers the others.

For the off-session variable-amount charge itself
([payments/save-and-reuse](https://docs.stripe.com/payments/save-and-reuse), Compliance):

> "To charge a customer when they're offline, make sure your terms include the following: The
> customer's agreement to your initiating a payment or a series of payments on their behalf for
> specified transactions. The anticipated timing and frequency of payments … How you determine
> the payment amount. Your cancellation policy … Make sure you keep a record of your customer's
> written agreement to these terms."

VenueHire's charge is variable (settled after acceptance) — so the setup flow must present terms
covering *how the amount is determined*, and the **written agreement must be recorded**.
**Inferred:** Checkout's built-in reuse-agreement text does not satisfy this on its own (no
checked page claims it does). Recommendation: persist the consent evidence (terms version,
timestamp) on the Payment operation row at setup — it is payment consent, it belongs with the
commitment, and it is what makes Stripe mark later charges as SCA-exempt MITs
("Merchant-initiated transactions require an agreement between you and your customer" —
[payments/setup-intents](https://docs.stripe.com/payments/setup-intents)).

`usage=off_session` on the SetupIntent is the API default and the correct value (front-loads SCA
so later off-session charges qualify for exemptions); the code already sets it explicitly
(`StripeAccountClient.cs:170,191`). ✔

### 5.5 Staleness and the 402

> "When a payment attempt fails, the request also fails with a 402 HTTP status code and the
> status of the PaymentIntent is requires_payment_method … You must notify your customer to
> return to your application to complete the payment." —
> [payments/save-and-reuse](https://docs.stripe.com/payments/save-and-reuse)
>
> "If the payment failed due to an authentication_required decline code, use the declined
> PaymentIntent's client secret with confirmPayment to allow the customer to authenticate the
> payment. If the payment failed for other reasons … send your customer to a payment page to
> enter a new payment method. You can reuse the existing PaymentIntent." — same page

Two distinct recoveries, so Payment must surface a **typed** rejection distinguishing them —
`authentication_required` (recoverable in place: bring the payer on-session against the *same*
PaymentIntent) vs everything else (new method: a fresh setup commitment). B2B's reaction is then
a keyed strategy on `FinancialOperation`, never a deal-type branch. Check the delivered
`PaymentMethodChargeError` union carries this distinction; if it collapses both into one case,
that's the gap to close before the consumer migration bakes the contract in.

Staleness: the card account updater refreshes card details in place — the events "include the
card's new expiration date and last four digits, so you can update your own records"
([payments/cards/overview](https://docs.stripe.com/payments/cards/overview)). **Inferred** (no
page states it as a single sentence, but the event shape — `payment_method.automatically_updated`
whose `data.object` is the existing payment_method — implies it): the `pm_` id itself is stable
across reissue/expiry updates, so a commitment held for weeks stays valid; the residual risk is
non-participating issuers, which lands in the 402 path above. No proactive action needed beyond
that recovery path.

### 5.6 Events to subscribe

Currently handled (worktree, `Services/Webhook/`): `setup_intent.succeeded`,
`setup_intent.setup_failed`, `payment_intent.succeeded`, `payment_intent.payment_failed`.

- **Keep** the four. Setup-mode's documented primary trigger is `checkout.session.completed`
  ([payments/checkout/save-and-reuse](https://docs.stripe.com/payments/checkout/save-and-reuse)),
  but `setup_intent.succeeded` carries the pm directly (no session→setup_intent hop) and is a
  legitimate catalog event ([api/events/types](https://docs.stripe.com/api/events/types)) — an
  acceptable deviation *because* the resolver also reconciles by API retrieve, so a missed/late
  event self-heals.
- **Add `checkout.session.expired`** — the documented signal to release anything reserved against
  a session ([managing-limited-inventory](https://docs.stripe.com/payments/checkout/managing-limited-inventory)).
  If `PaymentOperationRetryAndExpiryEvaluator` + `NextReconcileAt` already time these out, the
  event is an optimization, not a gap — verify which.
- **Optional hygiene:** `payment_method.automatically_updated` / `payment_method.detached` (card
  metadata display + early staleness signal); `payment_intent.processing` only matters if a
  delayed payment method is ever enabled — card-only today, skip.

---

## 6. Naming

- **`PaymentSessionSpecification` → `PaymentSessionDefinition`.** It is validated construction
  input, not a Specification-pattern predicate — and the repo has just given "Specification" a
  precise, load-bearing meaning (the DataAccess specification/query-boundary work), so the
  collision now actively misleads. Stripe's .NET SDK precedent is `SessionCreateOptions`
  (industry precedent, not re-verified this pass), but `*Options` collides with `IOptions`
  config semantics in .NET; `Definition` is the honest noun.
- **`CreateOrReplay` → `Create`.** Idempotent create is still *create*: Stripe's own idempotent
  POST keeps the verb and moves idempotency to the key ("A client generates an idempotency key…"
  — [api/idempotent_requests](https://docs.stripe.com/api/idempotent_requests)); Google's AIP-155
  likewise keeps the standard `Create` method and adds `request_id` (industry precedent, not
  fetched). Replay-on-duplicate is a documented *property* of the method, not part of its name.
  Note: a gRPC method rename changes the wire path — batch it with the breaking cull (§7).
- **`ConsumerCorrelation` → `ClientReference`.** Stripe's exact precedent, on the exact analogous
  seam (a consumer handing its own reconciliation key to the payment layer):
  > "`client_reference_id` (string, nullable) — A unique string to reference the Checkout
  > Session. This can be a customer ID, a cart ID, or similar, and can be used to reconcile the
  > Session with your internal systems." —
  > [api/checkout/sessions/object](https://docs.stripe.com/api/checkout/sessions/object)
  It also clears the collision with the messaging-trace `CorrelationId`.
- **Payment's `ApplicationId` property → `ContextId`.** The column
  (`TransactionEntityConfiguration` → `"ContextId"`) already got it right; rename the property to
  match, and make `contextId` the metadata key (§3).

---

## 7. End state and migration order

**End state:** the SPA never sees or sends a pm id; B2B holds only its own reference parts
(operation kind from its closed enum + business correlation) and reacts to typed outcomes keyed
by `FinancialOperation`; Payment holds every provider identifier, every consent artifact, and
every retry/reconcile decision; the bus and gRPC contracts speak references and typed errors only.

1. **Deliver the final producer surface on PR #933** (`Feature/payment-method-commitments`, open):
   keep the completed consent + key hardening, replace every consumer correlation with
   `(OperationType, ClientReference)`, remove the raw-identifier and bespoke session surfaces,
   remove consumer-role vocabulary, and land the §6 renames. Re-scaffold Payment, re-record the
   deliberately breaking compatibility baselines, review the full candidate, then publish the
   Payment Contracts + Client packages. Owner decision, 2026-09-04: one breaking release carries
   all producer changes because neither consumer has adopted the intermediate reference surface.
2. **Unblock PR #633** (`Refactor/launch_deal-lifecycle-modules-phase2`): the B2B
   lifecycle-ownership refactor consumes the new Payment surface, so it goes ready after the
   packages publish — advance its Payment pins, revalidate, merge.
3. **B2B + SPA consumer migration** (the plan's step 4): advance the package pin; adopt
   `SetupPaymentMethod`/`ValidatePaymentMethod` + `*ByReference` commands; delete
   `PrepaidApplication.PaymentMethodId`, `DeferredBooking.PaymentMethodId`,
   `BookingSettlement.PaymentMethodId`, `ApplyRequest`/`AcceptRequest.PaymentMethodId`,
   `FindHeldIntentAsync` usage; re-scaffold B2B initial migrations; SPA stops POSTing pm ids.
   One coordinated delivery chain (B2B + frontend) after the package publish.
4. **Re-key `IConfirm`/`ICancel`/`IComplete` by `FinancialOperation`** (§2). B2B-internal,
   independently shippable; naturally rides with or immediately after 3 since it touches the same
   call sites.
Step 1 is in flight; step 2 follows its package publish; step 4 is B2B-internal and independently
shippable; step 3 is the coordinated B2B consumer cut-over. Customer migrates independently after
the same package publish. There is no second producer cull release.
