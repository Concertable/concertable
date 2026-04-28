# Guest Checkout Refactor

## Goal

Allow unauthenticated users to purchase concert tickets via `ConcertCheckoutPage` without forcing account creation. The current behaviour 401s on `POST /api/ticket/purchase` because `TicketController` is decorated `[Authorize(Roles = "Customer")]`.

## Architecture decision: Path A — "ghost customer"

Guest provides email + name at checkout. BE finds-or-creates a `Customer` Identity record for that email (no password, marked as guest). All downstream code (`TicketService`, `CustomerPaymentModule`, ticket FK to `userId`, email delivery, ticket history) stays untouched — the guest has a real `Guid` like any other Customer.

Future story: when the guest later registers with the same email, their existing tickets are already linked.

Rejected alternatives:
- **Require login.** Would defeat the purpose; consumer ticket friction hurts the artist↔venue side of the platform.
- **True anonymous tickets** (nullable `UserId`, separate guest payment module). Forces every downstream consumer (queries, webhooks, refunds, history) to handle a parallel anonymous world forever.

## Steps

### 1. Identity / Customer module

- [ ] Add `IsGuest` flag to `CustomerEntity` (TPH subtype). Default `false`. Migration via `./initial-migrations.ps1` from `api/`.
- [ ] Add to `ICustomerModule`:
  ```csharp
  Task<Customer> FindOrCreateGuestAsync(string email, string name, CancellationToken ct = default);
  ```
- [ ] Impl behaviour:
  - If user exists with email → return existing (regardless of `IsGuest`). User is paying with their own email; not a hijack risk.
  - If no user → create with `IsGuest = true`, random unset password, given name, given email.
- [ ] Decide: should re-purchasing as a guest with the same email reuse the prior ghost record? Yes — that's what "find" does. The same email always maps to the same Customer.

### 2. Payment module — Stripe Customer for guests

`CustomerPaymentModule.PayAsync` currently throws if `payerAccount.StripeCustomerId is null`. For a brand-new guest, we either:

- **(Preferred) Provision Stripe Customer at guest creation time.** Inside `FindOrCreateGuestAsync` (or a follow-up call), invoke `IStripeAccountService.ProvisionCustomerAsync(userId, email)`. Then `CustomerPaymentModule.PayAsync` works unchanged — the guest has a Stripe Customer like any other.
- Alt: relax `StripeCustomerId` requirement in `CustomerPaymentModule` for ephemeral guest charges. Adds branching. Not recommended.

Pick the first option. Means guests get a real Stripe Customer record. Future tickets/refunds against that Customer work normally.

### 3. Concert module — ticket controller + service

- [ ] `TicketController.Purchase` — change to action-level auth:
  ```csharp
  // Remove or relax class-level [Authorize(Roles = "Customer")].
  // Add per-action:
  [Authorize(Roles = "Customer")]                          // Get* + can-purchase
  [HttpGet("upcoming/user")] public ... GetUserUpcoming()

  [AllowAnonymous]
  [HttpPost("purchase")] public ... Purchase(...)
  ```

- [ ] `TicketPurchaseParams` (in `Concertable.Payment.Domain`) — add fields:
  ```csharp
  public string? BuyerEmail { get; set; }
  public string? BuyerName { get; set; }
  ```

- [ ] `TicketPurchaseParamsValidator` — `BuyerEmail`/`BuyerName` required when `currentUser.Id is null`. Inject `ICurrentUser` into validator OR validate in service layer instead (cleaner).

- [ ] `TicketService.PurchaseAsync` — branch at top:
  ```csharp
  Guid payerId;
  string buyerEmail;

  if (currentUser.Id is Guid userId)
  {
      if (currentUser.GetRole() != Role.Customer)
          throw new ForbiddenException("Only Customers can buy tickets");
      payerId = userId;
      buyerEmail = currentUser.Email!;
  }
  else
  {
      if (string.IsNullOrWhiteSpace(purchaseParams.BuyerEmail) ||
          string.IsNullOrWhiteSpace(purchaseParams.BuyerName))
          throw new BadRequestException("Email and name required for guest checkout");

      var guest = await customerModule.FindOrCreateGuestAsync(
          purchaseParams.BuyerEmail, purchaseParams.BuyerName);
      payerId = guest.Id;
      buyerEmail = guest.Email!;
  }
  ```
  Then proceed with existing payment + return path, replacing `currentUser.GetId()` → `payerId` and `currentUser.Email` → `buyerEmail`.

- [ ] Inject `ICustomerModule` into `TicketService`.

### 4. Frontend

- [ ] Create `app/src/components/checkout/GuestContactForm.tsx`:
  - Two fields: name + email.
  - Lifts state to parent via `onChange({ email, name })`.
  - Render only when `!isAuthenticated`.

- [ ] `ImmediatePaymentSection` (or wrap at page level) — render `<GuestContactForm />` above `<NewCardSection />` for guests.

- [ ] `useTicketCheckout` (and its `purchase` mutation) — accept `buyerEmail` + `buyerName` in the request body. Required for guests.

- [ ] `ConcertCheckoutPage` — wire guest contact state alongside `paymentMethodId`. Submit button disabled until: `paymentMethodId` set AND (logged in OR guest contact filled).

- [ ] Optional: add a "Continue as guest" / "Log in" toggle at the top of the payment step for clarity. Not required — guest form just appears for unauth users.

### 5. Email delivery

- `SendTicketsToEmailAsync(buyerEmail, ticketIds)` already takes an explicit email. Just pass the resolved `buyerEmail` (from auth or guest path).

### 6. Tests

- [ ] Integration test: `POST /api/ticket/purchase` with no auth, body includes `buyerEmail` + `buyerName` + `paymentMethodId` (fake one) + `concertId`. Assert:
  - 200 OK with TicketPaymentResponse
  - Customer record created with email, `IsGuest = true`
  - Ticket entity created with `UserId = guest.Id`
  - Email delivered to provided address
- [ ] Integration test: same email purchases twice — second purchase reuses the existing guest record (no duplicate Customer).
- [ ] Integration test: guest email matches an existing real (non-guest) account → purchase succeeds, ticket attached to existing user, no `IsGuest` flag flip.

## Open questions

- **Email collision UX:** if a guest enters an email that matches a real account, do we silently attach to that account or warn them? Path A spec says silent attach (it's their own email). Could add gentle messaging: "Already have an account? [Log in] to view past tickets."
- **"Claim your tickets" flow:** when guest later registers, do they get an email saying "we found tickets under this address"? Probably yes — small follow-up. Not blocking guest checkout itself.
- **Refunds for guest tickets:** today refunds presumably go through user-authenticated flow. Guests can't log in to request. Could rely on email-based refund link, or block refunds entirely for guests, or require account upgrade. Decide later.
- **Stripe Customer cleanup:** if a guest never returns, their Stripe Customer sits unused. Stripe doesn't charge for idle Customers — fine.

## Out of scope (for this refactor)

- Magic-link "claim your account" emails for guests.
- Guest-to-real-user upgrade flow (currently happens implicitly when they register with same email — fine).
- Application checkout (`ApplicationCheckoutPage`) guest support — managers already require auth; that flow stays.

## Estimated scope

~150 lines across:
- 1 entity flag + migration
- 1 new `ICustomerModule` method + impl
- 1 controller attribute change
- 2 fields on `TicketPurchaseParams`
- ~10-line branch in `TicketService.PurchaseAsync`
- 1 small FE form component
- A handful of FE state/wiring updates
- 3 integration tests
