# Card Verification at PM-Storage Points (VenueHire apply + DoorSplit/Versus accept)

**Branch:** `Feature/ApplyCardVerification` (off `master`)

## The problem

Any flow that stores a PaymentMethod to charge later off-session uses Stripe `SetupIntent` to attach the card. SetupIntent confirms only that the card details are well-formed, accepted by Stripe, and eligible for off-session usage. It does **not** confirm that the card will successfully charge later.

Two flows are affected:

| Flow | PM stored on | Charged at | Failure surfaces today |
|---|---|---|---|
| **VenueHire apply** | Artist's PM on `PrepaidApplication` | Venue accept (off-session) | At venue accept — ghost application until then |
| **DoorSplit / Versus accept** | Venue's PM on `DeferredBooking` | Concert finish (off-session, deferred) | At finish (worker-driven, days later) — concert booked then settlement fails |

**FlatFee accept is unaffected** — `confirmPayment` performs the real charge in-band, so the card is verified by the act of charging.

**Reproducible with Stripe test card `4000 0000 0000 0341`** — designed to attach successfully and fail at off-session charge.

In production this is rare (>99% of cards that pass SetupIntent succeed at off-session charge), but the failure path is real (expired between attach and charge, fraud lock placed post-attach, account closed). Today both paths are poorly handled — the failure surfaces far from where the user provided the card.

## Decision

**£1 verify-and-void at apply.** Before constructing the `PrepaidApplication`, the BE attempts a £1 PaymentIntent with `capture_method: manual`, then voids regardless of outcome. If Stripe declines, the apply fails fast with a meaningful message — no application is created. If it succeeds, the card is verified for charging *right now* and the void releases any reservation immediately.

**Trade-offs vs alternatives:**

| Option | Catches at apply | Cost | Verdict |
|---|---|---|---|
| SetupIntent only (current) | Bad number / blocked / fraud-flagged | £0, no statement entry | Status quo. Doesn't catch real-world declines well enough. |
| **£1 verify-and-void (chosen)** | Above + "Stripe won't actually charge this card right now" — covers most decline reasons | £1 + void on artist's statement; no funds tied up | Right balance. |
| Full pre-auth £250 with manual capture | Above + reserves actual funds | £250 hold per application until venue decides; expires after 7 days; UX hit | Over-engineered for marketplace where artists apply to many opps. Real estate / hotel pattern, not marketplace pattern. |

**Where it lives:** at every PM-storage point that feeds a future off-session charge. Two locations:

1. **`VenueHireConcertWorkflow.ApplyAsync`** — verify before constructing `PrepaidApplication`. Covers VenueHire apply.
2. **`DeferredConcertService.InitiateAsync`** — verify before `bookingService.CreateDeferredAsync`. Covers DoorSplit + Versus accept (both contracts route through this service).

`FlatFeeConcertWorkflow.AcceptAsync` is **not** updated — the on-session `confirmPayment` already performs a real charge, which is itself the verification.

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

### 3. Wire into `VenueHireConcertWorkflow.ApplyAsync` (artist's card at apply)

```csharp
public async Task<ApplicationEntity> ApplyAsync(int artistId, int opportunityId, string paymentMethodId)
{
    await managerPaymentModule.VerifyAndVoidAsync(currentUser.GetId(), paymentMethodId);
    return PrepaidApplication.Create(artistId, opportunityId, paymentMethodId);
}
```

If `VerifyAndVoidAsync` throws → `ApplicationService.ApplyAsync` propagates → controller returns 400 with the Stripe decline message → FE shows it. **No `PrepaidApplication` row created.**

### 4. Wire into `DeferredConcertService.InitiateAsync` (venue's card at deferred-accept)

```csharp
public async Task<IAcceptOutcome> InitiateAsync(int applicationId, Guid payerId, string paymentMethodId)
{
    var appCheck = await applicationValidator.CanAcceptAsync(applicationId);
    if (appCheck.IsFailed)
        throw new BadRequestException(appCheck.Errors);

    await managerPaymentModule.VerifyAndVoidAsync(payerId, paymentMethodId);

    var booking = await bookingService.CreateDeferredAsync(applicationId, paymentMethodId);
    // ... draft creation, return DeferredAcceptOutcome
}
```

Covers DoorSplit + Versus. If verify throws → no `DeferredBooking` row, no concert draft. Venue sees a 400 with the decline message at accept-checkout time.

### 5. Translate Stripe decline → `BadRequestException` cleanly

The Stripe SDK throws `StripeException` with a `StripeError.DeclineCode`/`Message` on card decline. Want to surface a friendly message to the artist, not a raw Stripe stack trace. Either:
- Catch in `VerifyAndVoidAsync` and re-throw as `BadRequestException`, OR
- Catch in `ApplicationController.Apply` middleware-style

Put it in `VerifyAndVoidAsync` so any consumer of the primitive gets the same translated error.

## Frontend changes

**Approximately none.** Both flows already POST and surface response errors. With the BE change, declined cards get a 400 with the Stripe message instead of a 201/200 + ghost row.

Cosmetic improvements worth tightening (existing copy is misleading):
- `ApplyCheckoutPage` success copy: `"Your card was authorised and your application was sent"` → `"Your card was saved and your application was sent. The venue will only charge if they accept."`
- `ApplicationCheckoutPage` deferred copy ("Card saved" awaiting step) — already accurate, no change.

## Tests

### Integration tests — new (3)

1. `ApplicationVenueHireApiTests.Apply_ShouldFail_WhenCardWillDecline` — VenueHire apply path, asserts 400 + no `PrepaidApplication` row.
2. `ApplicationDoorSplitApiTests.Accept_ShouldFail_WhenCardWillDecline` — DoorSplit accept path, asserts 400 + no `DeferredBooking` + no concert draft.
3. `ApplicationVersusApiTests.Accept_ShouldFail_WhenCardWillDecline` — Versus accept path, same shape as DoorSplit.

Setup: `MockStripeClient` keyed on a magic PM id (`pm_decline_at_verify`) — `FakeStripeAccountService.VerifyAndVoidAsync` throws when called with that PM.

### Unit tests — new (2)

1. `VenueHireConcertWorkflowTests.ApplyAsync_ShouldThrow_WhenVerifyFails` — mock `IManagerPaymentModule.VerifyAndVoidAsync` to throw, assert no entity returned.
2. `DeferredConcertServiceTests.InitiateAsync_ShouldThrow_WhenVerifyFails` — same shape, mock the same primitive.

### Manual test

Use Stripe test card `4000 0000 0000 0341` on:
- VenueHire apply-checkout — before: apply succeeds (ghost app), venue accept later 400s. After: apply 400s, no application created.
- DoorSplit/Versus accept-checkout — before: accept succeeds (concert created!), settlement at finish 400s. After: accept 400s, no concert draft, venue tries another card.

## Out of scope

- Full £-amount pre-auth with manual capture (Option A from the conversation) — over-engineered for this workflow; verify-and-void gets >95% of the value at no UX cost.
- Notification to artist when a venue-accept-time charge fails — separate post-merge task. Less critical now because the verify catches most failure cases at the storage point.
- Verify on FlatFee accept — already verified by the on-session `confirmPayment`. Adding a £1 hold there would just be a redundant Stripe call.
- Verify on FlatFee/DoorSplit/Versus apply — they're `ISimpleApply` (no PM at apply), nothing to verify.
- 3DS during the £1 verify — `confirm: true` with `off_session: true` will NOT trigger 3DS challenges (off-session SCA exempt). If 3DS IS triggered (rare for verification), the verify itself fails — same handling as decline (apply/accept 400s, user re-tries with a different card).

## Acceptance

VenueHire apply:
1. Apply with `4242 4242 4242 4242` (good card) → succeeds as today, application created
2. Apply with `4000 0000 0000 0341` (verify-passes-charge-fails card) → **400 at apply, no application created**
3. Apply with `4000 0000 0000 0002` (immediate decline card) → still rejected at `confirmSetup` step in browser (unchanged from today)

DoorSplit/Versus accept:
4. Accept with `4242...` → succeeds, booking + concert draft created
5. Accept with `4000...0341` → **400 at accept, no booking, no concert draft**
6. Accept with `4000...0002` → still rejected at `confirmSetup` step in browser (unchanged)

Suite:
7. Concert integration suite 46 → 49 with the 3 new tests
8. Concert unit suite 14 → 16 with the 2 new tests
9. FE tsc clean
10. Manual: `4000...0341` fails at the storage step with a clean error message in both flows
