# Investigation brief: Stripe Payment Element suddenly demanding postal code; 3DS test card hangs

## What's happening (user-visible)

In the Concertable web app, the Stripe Payment Element on the **customer ticket checkout page** has, in the last 3–4 days, started:

1. Rendering a **Postal code** field that was never there before.
2. Marking valid UK postcodes (e.g. `WS11 1DB`, `SW1A 1AA`) as `"Your postal code is invalid."`
3. When the postcode field is hidden via `fields.billingDetails.address.postalCode: "never"`, `stripe.confirmPayment()` **hangs forever** — never resolves, never errors. UI sticks on "Processing…". No 3DS iframe ever appears.
4. The same happens for the 3DS test card (`StripeCards.Requires3ds`) — confirmPayment never resolves, the 3DS challenge iframe (`iframe[src*='three-ds-2-challenge']`) never appears, the E2E test times out at 30s waiting for it.

The 3-4 day window correlates **exactly** with a frontend refactor: a single SPA was split into four per-surface SPAs (customer/venue/artist/business) on different ports — see commits between `123ee804` (Add business landing + remove legacy app/web/ scaffolding) and HEAD on branch `Refactor/ThreeSurfaceSplit-Cont`.

User insists nothing on their side changed that would affect Stripe behaviour.

## Repo / environment

- **Repo root:** `C:\Users\TommySeery\source\repos\Concertable`
- **Branch:** `Refactor/ThreeSurfaceSplit-Cont`
- **Stripe SDK (client):** `@stripe/stripe-js` `^4` (resolved `4.10.0`), `@stripe/react-stripe-js` `^3` (resolved `3.10.0`) — `app/package-lock.json`
- **Stripe SDK (server):** `Stripe.net 47.3.0`
- **Customer surface dev URL:** `https://localhost:5174`
- **Auth host:** `https://localhost:7083` (Duende IS)
- **API:** `https://localhost:7086`
- **Stripe publishable key:** `pk_test_51QqfAR...` (test mode, in `app/web/.env.development`)
- **Aspire-orchestrated**, with `stripe/stripe-cli` Docker container forwarding webhooks (see `api/Concertable.AppHost/DistributedApplicationBuilderExtensions.cs`)

## Key files

### Frontend
- `app/web/shared/src/features/concerts/components/checkout/StripePaymentForm.tsx` — mounts `<Elements>` + `<PaymentElement>` for the customer ticket checkout. Currently:
  ```tsx
  <Elements stripe={stripePromise} options={{ clientSecret, customerSessionClientSecret: customerSession, appearance }}>
    <PaymentElement
      options={{
        layout: "tabs",
        defaultValues: { billingDetails: { address: { country: "GB" } } },
      }}
      onReady={() => setPaymentReady(true)}
    />
  </Elements>
  ```
  Then on submit:
  ```tsx
  const result = isSetup
    ? await stripe.confirmSetup({ elements, redirect: "if_required" })
    : await stripe.confirmPayment({ elements, redirect: "if_required" });
  ```
  No `confirmParams.payment_method_data.billing_details` is currently passed.
- `app/web/shared/src/lib/stripe.ts` — `loadStripe(import.meta.env.VITE_STRIPE_PUBLISHABLE_KEY)`. Same publishable key as before.
- `app/web/customer/src/routes/_customer/concert/checkout.$id.tsx` — TanStack route → renders `TicketCheckoutPage` from `@/features/concerts`.

### Backend (Stripe wiring)
- `api/Modules/Payment/Concertable.Payment.Infrastructure/Services/StripeAccountClient.cs` — owns all Stripe-customer and PaymentIntent/SetupIntent creation.
  - `ProvisionCustomerAsync` (L48): just modified to set `Address = new AddressOptions { Country = "GB" }`. Previously was email-only. *Existing Stripe customers in the test account were created without country.*
  - `CreatePaymentSessionAsync` (L131) — PaymentIntent: `SetupFutureUsage = "off_session"`, `AutomaticPaymentMethods.Enabled = true, AllowRedirects = "never"`.
  - `CreateSetupSessionAsync` (L154), `CreateVerifySessionAsync` (L175), `CreateHoldSessionAsync` (L199) — same shape.
  - `CreateCustomerSessionAsync` (L224) — CustomerSession with `PaymentMethodSave = "enabled"`, `PaymentMethodRemove = "enabled"`, `PaymentMethodRedisplay = "enabled"`, `PaymentMethodAllowRedisplayFilters = ["always", "limited", "unspecified"]`.

### E2E tests
- `api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/Support/StripeCardEntry.cs` — fills card via Playwright into the Stripe iframe (`iframe[src*='elements-inner-accessory-target']`). Just extended to fill postal code with `SW1A1AA` if a `[autocomplete='postal-code']` field is present.
- `api/Tests/Concertable.E2ETests/Concertable.E2ETests.Ui/Support/StripePayment.cs` — `CompleteChallengeAsync` waits for `iframe[src*='three-ds-2-challenge']` (30s) and times out.
- Failing test: `TicketPurchase.feature: "Customer completes 3DS challenge"` (line 56). Steps: customer is on concert detail → buys tickets → pays with `StripeCards.Requires3ds` → expects 3DS iframe → expects success screen.

## What's been tried and what didn't work

1. **Hide postal code client-side only** (`fields.billingDetails.address.postalCode: "never"`):
   - Result: `stripe.confirmPayment()` hangs, "Processing…" forever, no resolve/reject. Stripe docs say if you hide a field you must pass it back via `confirmParams.payment_method_data.billing_details`. We didn't, hence hang.
   - **Reverted.**

2. **`defaultValues.billingDetails.address.country = "GB"` on the PaymentElement** (currently in place):
   - Result: User reports postcode field still says invalid for real UK postcodes. `defaultValues` may only seed the UI; it doesn't force the validator's country if the customer/intent provides a different (or no) country.

3. **Set `Address.Country = "GB"` on `CustomerCreateOptions`** (just applied):
   - Only affects *new* Stripe customers. The customer being used in the failing test/manual session was created earlier with no country. Tests should pick it up via the per-scenario seeder reset; manual test won't until that customer is recreated.

4. **Test card-entry helper extended** to fill `[autocomplete='postal-code']` with `SW1A1AA` when present (`StripeCardEntry.cs:FillCardAsync`).

After these changes, the **3DS test still fails the same way**: card details get filled, button clicked, no 3DS iframe ever appears, 30s timeout.

## What I want investigated

1. **Why does `confirmPayment` never trigger 3DS for `StripeCards.Requires3ds` (`4000002500003155`)?** Is it failing validation silently *before* hitting the Stripe API? Inspect the browser console / network tab during the failing run. Look for:
   - Any `stripe.com/v1/payment_intents/.../confirm` calls
   - Whether the confirm call returns a `requires_action` status (which would then prompt 3DS)
   - Whether `elements.submit()` is rejecting with a validation error we're not handling visibly
   - Whether the `Promise` returned by `confirmPayment` is being dropped (e.g. unhandled rejection swallowed)

2. **What actually drives Stripe's postal-code-validation country?** I've assumed it's the Customer's `address.country`. Verify against current Stripe docs whether `defaultValues.billingDetails.address.country` is sufficient on its own, or whether `confirmParams.payment_method_data.billing_details.address.country` must also be set, or whether the customer's stored address is the only authoritative source. The seemingly-overlapping knobs are confusing.

3. **Did Stripe ship a Radar / Payment Element behaviour change in the 3-4 day window matching the user's report?** Check Stripe changelog (`stripe.com/changelog`, `stripe.com/blog`, `stripe.com/docs/upgrades`) for anything around address collection / SCA / Radar for that window (~2026-05-13 to 2026-05-17).

4. **Test-card-entry fills `SW1A1AA` without a space** — UK postcode validators sometimes require the space. Worth verifying that the validation accepts both, and updating the test to use `SW1A 1AA` if not.

5. **Is the registered Stripe `payment_method_domain` set for `localhost:5174`?** Pre-split the SPA was on a different port. Stripe requires domain registration for Apple Pay / Link / Google Pay. Unregistered domains can fall back to more conservative defaults for *all* payment methods, sometimes. Check Stripe Dashboard → Settings → Payment methods → Payment method domains. The user has a domain `pmc_1RCaz2IKFUeKxLxa...` config visible there.

6. **Is the existing test Stripe customer being reused or recreated per E2E scenario?** Find the seeder path for customers in `api/Tests/Concertable.E2ETests/` and `api/Seeding/`. Verify whether `ResetAsync` actually triggers a fresh `ProvisionCustomerAsync` per scenario (which would mean my Country=GB change applies), or whether customers persist across runs in the Stripe account (which would mean the country fix doesn't apply until the customer is manually deleted).

## What I'm hoping for back

A diagnosis with file:line citations and either:
- A concrete root cause for why `confirmPayment` hangs / never triggers 3DS, with the exact code change needed, **or**
- A confirmed Stripe-side change in the relevant window with the canonical Stripe-recommended workaround, **or**
- A pointer that one of the things I tried *should* have worked and the reason it didn't (e.g. caching, customer not actually recreated, etc.).

Do not just speculate — verify with the codebase and Stripe docs.
