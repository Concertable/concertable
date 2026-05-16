# Concertable

Concertable is a contract-driven live music booking platform. Venue managers create **Opportunities** — each paired with a **Contract** that defines the commercial terms (e.g. *Flat Fee*, *Venue Hire*, *Door Split*, *Versus*). Artist managers browse and apply to those opportunities; once a venue accepts an application the system resolves the appropriate **workflow** automatically based on the contract type — who pays, when, and how much. All financial movement (artist payouts, venue charges) is handled through Stripe Connect. Customers discover and purchase tickets to the resulting concerts entirely within the app. There is no external communication: negotiation, contracting, payment, and ticketing all happen inside the platform.

**Actors:** Venue Manager · Artist Manager · Customer  
**Contract types drive distinct workflows** — Venue Hire (artist pays venue), Flat Fee (venue pays artist fixed sum), Door Split (revenue share), Versus (higher of two terms).
