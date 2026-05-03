# Apply-time Card Verification (VenueHire)

**Branch:** `Feature/ApplyCardVerification` (off `master`)

## The problem

VenueHire artist apply-checkout uses Stripe `SetupIntent` to attach the card. SetupIntent confirms only that the card details are well-formed, accepted by Stripe, and eligible for off-session usage. It does **not** confirm that the card will successfully charge later.

Result: an artist can complete apply-checkout with a card that will decline at venue-accept time. The application gets created, the venue spends time reviewing it, and the failure only surfaces when the venue clicks Accept and the BE off-session charge returns a decline.

**Reproducible with Stripe test card `4000 0000 0000 0341`** — designed to attach successfully and fail at off-session charge.

In production this is rare (>99% of cards that pass SetupIntent succeed at off-session charge), but the failure path is real (expired between attach and charge, fraud lock placed post-attach, account closed). Today the path is poorly handled — venue gets a 400, artist gets nothing.

## Decision

**£1 verify-and-void at apply.** Before constructing the `PrepaidApplication`, the BE attempts a £1 PaymentIntent with `capture_method: manual`, then voids regardless of outcome. If Stripe declines, the apply fails fast with a meaningful message — no application is created. If it succeeds, the card is verified for charging *right now* and the void releases any reservation immediately.

**Trade-offs vs alternatives:**

| Option | Catches at apply | Cost | Verdict |
|---|---|---|---|
| SetupIntent only (current) | Bad number / blocked / fraud-flagged | £0, no statement entry | Status quo. Doesn't catch real-world declines well enough. |
| **£1 verify-and-void (chosen)** | Above + "Stripe won't actually charge this card right now" — covers most decline reasons | £1 + void on artist's statement; no funds tied up | Right balance. |
| Full pre-auth £250 with manual capture | Above + reserves actual funds | £250 hold per application until venue decides; expires after 7 days; UX hit | Over-engineered for marketplace where artists apply to many opps. Real estate / hotel pattern, not marketplace pattern. |

**Where it lives:** inside `VenueHireConcertWorkflow.ApplyAsync` — the verify step is part of the apply workflow for prepaid contracts. Keeps the role-interface composition clean: `IPaidApply` workflows verify; `ISimpleApply` workflows don't (because there's no card at apply for them).

## Backend changes

### 1. New primitive on `IManagerPaymentModule`

```csharp
Task VerifyAndVoidAsync(
    Guid payerId,
    string paymentMethodId,
    CancellationToken ct = default);
```

Implementation in `ManagerPaymentModule`:

```csharp
public async Task VerifyAndVoidAsync(Guid payerId, string paymentMethodId, CancellationToken ct = default)
{
    var stripeCustomerId = await EnsureStripeCustomerAsync(payerId, ct);
    await stripeAccountService.VerifyAndVoidAsync(stripeCustomerId, paymentMethodId, ct);
}
```

### 2. New primitive on `IStripeAccountService`

```csharp
Task VerifyAndVoidAsync(
    string stripeCustomerId,
    string paymentMethodId,
    CancellationToken ct = default);
```

Real impl in `StripeAccountService`:

```csharp
public async Task VerifyAndVoidAsync(string stripeCustomerId, string paymentMethodId, CancellationToken ct = default)
{
    var intent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
    {
        Amount = 100, // £1.00
        Currency = "gbp",
        Customer = stripeCustomerId,
        PaymentMethod = paymentMethodId,
        Confirm = true,
        CaptureMethod = "manual",
        OffSession = true,
        Description = "Card verification (auto-voided)"
    }, cancellationToken: ct);

    // Throws StripeException on decline — caller treats that as the verification failure

    if (intent.Status == "requires_capture" || intent.Status == "succeeded")
    {
        await paymentIntentService.CancelAsync(intent.Id, cancellationToken: ct);
    }
}
```

Fake impl in `FakeStripeAccountService`: `Task.CompletedTask` (always passes — integration tests using the fake never see decline; we test decline by keying on a magic PM id like `pm_decline`).

### 3. Wire into `VenueHireConcertWorkflow.ApplyAsync`

```csharp
public async Task<ApplicationEntity> ApplyAsync(int artistId, int opportunityId, string paymentMethodId)
{
    await managerPaymentModule.VerifyAndVoidAsync(currentUser.GetId(), paymentMethodId);
    return PrepaidApplication.Create(artistId, opportunityId, paymentMethodId);
}
```

If `VerifyAndVoidAsync` throws → `ApplicationService.ApplyAsync` propagates → controller returns 400 with the Stripe decline message → FE shows it. **No `PrepaidApplication` row created.**

### 4. Translate Stripe decline → `BadRequestException` cleanly

The Stripe SDK throws `StripeException` with a `StripeError.DeclineCode`/`Message` on card decline. Want to surface a friendly message to the artist, not a raw Stripe stack trace. Either:
- Catch in `VerifyAndVoidAsync` and re-throw as `BadRequestException`, OR
- Catch in `ApplicationController.Apply` middleware-style

Put it in `VerifyAndVoidAsync` so any consumer of the primitive gets the same translated error.

## Frontend changes

**Approximately none.** The apply page already POSTs `/application/{oppId}` and surfaces the response error. With the BE change, declined-card cards will get a 400 with the Stripe message instead of a 201 + ghost application.

Cosmetic improvement worth tightening (existing copy is misleading):
- `ApplyCheckoutPage` success copy: `"Your card was authorised and your application was sent"` → `"Your card was saved and your application was sent. The venue will only charge if they accept."`

## Tests

### Integration test — new

`ApplicationVenueHireApiTests.ApplyCheckoutThenApply_ShouldFail_WhenCardWillDecline`:
- Setup: `MockStripeClient` rigged to throw a decline on the verify-and-void call (or use a magic PM id `pm_decline_at_verify`)
- Act: artist runs apply-checkout, then POSTs apply with the bad PM
- Assert: `400 BadRequest`, no `PrepaidApplication` exists in `fixture.ReadDbContext`

### Unit test — new

`VenueHireConcertWorkflowTests.ApplyAsync_ShouldThrow_WhenVerifyFails`:
- Mock `IManagerPaymentModule.VerifyAndVoidAsync` to throw
- Call `workflow.ApplyAsync(...)`
- Assert: throws, no entity returned

### Manual test

Use Stripe test card `4000 0000 0000 0341` on the apply-checkout page:
- Before this change: apply succeeds (ghost app), venue accept later 400s
- After this change: apply 400s, no application created, artist sees "Your card was declined" or similar

## Out of scope

- Full £250 pre-auth at apply (Option A from the conversation) — over-engineered for this workflow.
- Notification to artist when a venue-accept-time charge fails — separate post-merge task. Less critical now because the verify catches most failure cases at apply.
- Verify-and-void for FlatFee/DoorSplit/Versus — they're `ISimpleApply` (no PM at apply), so this entire flow doesn't apply to them. The PM is collected at venue-accept-checkout where the venue's card is verified by `confirmPayment` itself.
- 3DS during the £1 verify — `confirm: true` with `off_session: true` will NOT trigger 3DS challenges (off-session SCA exempt). If 3DS IS triggered (rare for verification), the verify itself fails — same handling as decline (apply 400s, artist re-tries with a different card).

## Acceptance

1. Apply with `4242 4242 4242 4242` (good card) → succeeds as today, application created
2. Apply with `4000 0000 0000 0341` (verify-passes-charge-fails card) → **400 at apply, no application created**
3. Apply with `4000 0000 0000 0002` (immediate decline card) → still rejected at `confirmSetup` step in browser (unchanged from today)
4. Concert integration suite still 46/46 + the new test = 47/47
5. FE tsc clean
6. Manual: see `4000...0341` fail at apply with a clean error message
