# Customer payment-reference migration plan

## Outcome

Customer never stores or forwards a provider payment-method identifier. Ticket purchase runs an
on-session `Payment`-kind session identified by a Customer-minted `PaymentOperationReference`; the
buyer completes with a client secret, and any future saved-card selection is Payment's surface.

## Delivery

1. Advance Customer's Payment package pins to the published Contracts and Client packages from the
   payment-operation-ownership producer. Verification gate: Customer solution builds against the
   published pin with zero warnings.
2. Replace the ticket-purchase flow: delete `TicketPurchaseParams.PaymentMethodId` and its
   validator rule; mint the reference at Customer's Payment adapter (operation type from a closed
   Customer-owned enum; consumer correlation from the stable purchase identity — buyer user id +
   concert id — with the server-minted UUIDv7 operation id as the idempotency key); create the
   session through the durable operation surface (`kind = Payment`, `session = OnSession`,
   destination funds routing, amount from ticket pricing) and return the client secret. Ticket
   issuance keeps riding the existing success events. Consumption contract: the purchase endpoint
   returns the client secret and operation reference to the SPA; no payment-method field anywhere
   on the wire. Verification gate: Customer unit + integration suites green.
3. Frontend: drop `paymentMethodId` from the `@concertable/customer` shared ticket types (its own
   publish-first npm bump for the web and mobile consumers), and move the customer web + mobile
   checkout to confirm with the client secret via Stripe.js / the mobile SDK. Verification gate:
   customer app typecheck gates green.
4. Rename gate: `grep -rniE "paymentMethodId"` over `api/Concertable.Customer` and the customer
   frontends returns zero, with any deliberate survivor allowlisted explicitly.

## Invariants

- A provider payment-method identifier exists only in Payment persistence and provider adapter calls.
- Customer's reference parts are Customer-owned opaque strings; Payment never parses them.
- An on-session single charge collects no mandate: `MandateTermsVersion` is not supplied for
  `Payment`-kind sessions. A future saved-card feature is a setup-kind session plus recorded
  consent, and is out of scope here.

## Package cut-over

Consumer-only change: it waits for the published Payment packages and cannot merge before the
Customer platform pin advances. The `@concertable/customer` npm bump is its own publish-first step
ahead of the web and mobile consumers.
