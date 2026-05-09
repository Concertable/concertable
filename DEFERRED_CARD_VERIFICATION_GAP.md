# Deferred Contract Card Verification Gap

## Problem

For **DoorSplit** and **Versus** contracts (deferred payment paths), the venue manager's card is
**not verified for chargeability** at accept time. A card can pass the setup intent step and create
a booking, but then fail silently when the actual charge runs at concert settlement.

## Affected paths

| Contract | Accept flow | Card tested at accept? | Card tested at settle? |
|---|---|---|---|
| FlatFee | `UpfrontConcertService` → `escrowModule.HoldAsync` | ✅ Yes — escrow hold is a real charge attempt | N/A |
| VenueHire (accept) | `UpfrontConcertService` → `escrowModule.HoldAsync` | ✅ Yes — same | N/A |
| DoorSplit | `DeferredConcertService.InitiateAsync` | ❌ No | ✅ But too late |
| Versus | `DeferredConcertService.InitiateAsync` | ❌ No | ✅ But too late |

## What the setup intent does and does not verify

`CreateSetupSessionAsync` (`StripeAccountClient.cs:149`) creates a setup intent with `usage: "off_session"`.

This confirms:
- Card is real and not expired
- Card can be used for future off-session payments (3DS authenticated)

This does **not** confirm:
- Sufficient funds
- Card is not blocked/restricted for purchases

## Where `VerifyAndVoidAsync` is

`StripeAccountClient.VerifyAndVoidAsync` (`StripeAccountClient.cs:170`) is still fully implemented.
It creates a £1 manual-capture payment intent, confirms it on-session, and cancels it immediately
on success — testing true chargeability without moving money.

It is **not called** from `DeferredConcertService.InitiateAsync` (`DeferredConcertService.cs:29`).

## Why it was removed

`VerifyAndVoidAsync` was removed from the deferred accept path because it produced two separate
3DS dialogs on the same accept page:

1. The setup intent (PaymentElement) triggers 3DS for the card save
2. `VerifyAndVoidAsync` created a second payment intent which also triggered 3DS

Two independent Stripe PIs = two independent 3DS challenges. This was a broken UX and the
removal was the quickest fix to make E2E tests pass.

## Risk

If a manager's card passes setup intent but is declined at settlement:
- `DeferredConcertService.FinishedAsync` calls `managerPaymentModule.PayAsync` off-session
- The charge fails
- The booking is left in `AwaitingPayment` indefinitely
- There is currently no retry or recovery path

## Potential solutions

1. **Restore `VerifyAndVoidAsync` with proper 3DS handling** — `DeferredAcceptOutcome` carries a
   `PaymentResponse` only when `RequiresAction = true`; FE conditionally calls `handle3ds`.
   E2E deferred 3DS steps need a second `CompleteChallengeAsync()`. Requires changes to
   `DeferredAcceptOutcome`, `DeferredConcertService`, `ApplicationCheckoutPage`, and E2E steps.

2. **Replace setup intent + verify PI with a single manual-capture PI** — one PI handles both
   card save (`setup_future_usage: "off_session"`) and verification. Single 3DS challenge.
   Bigger FE/BE change: the accept checkout session type changes from Setup → Payment.

3. **Accept deferred-only risk** — settlement failure triggers a notification/admin alert;
   manager is contacted to update card. Simpler but requires explicit error handling in
   `DeferredConcertService.FinishedAsync`.

## Key files

- `api/Modules/Concert/Concertable.Concert.Infrastructure/Services/Workflow/DeferredConcertService.cs`
- `api/Modules/Payment/Concertable.Payment.Infrastructure/Services/StripeAccountClient.cs` (line 170)
- `api/Modules/Concert/Concertable.Concert.Application/Responses/WorkflowOutcomes.cs`
- `app/src/features/concerts/pages/ApplicationCheckoutPage.tsx`
