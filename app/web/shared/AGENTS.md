# app/web/shared — code shared across the web SPAs

Inherits [`../../AGENTS.md`](../../AGENTS.md) (frontend conventions + patterns).

## Everything here compiles into EVERY web app. Nothing app-specific goes here. Ever.

Concertable is two products. Code shared by the two manager apps but not the customer app —
opportunities, contracts, applications, payouts — belongs in `app/web/b2b/shared`
(see [its `AGENTS.md`](../b2b/shared/AGENTS.md)), never here. This tree is for what is genuinely universal: design system,
auth/search/messaging infrastructure, and the details views every site renders.

The five SPAs (customer, venue, artist, business, admin) are fully separate sites: separate OIDC clients,
separate sessions, separate backends behind the same `api` client (customer → Customer service,
managers → B2B). A manager logged into the venue site is simply not logged in anywhere else. Code in
this tree cannot know which site it's running in — so it may only do things every site can
legitimately do:

- **Backend rule** — authenticated calls go only to backends every site can call with its own token:
  the own-site `apiClient`, `searchClient`, `paymentClient`. A call only one site's token can make
  (e.g. `customerClient` → Customer service) lives in that app, never here.
- **Identity rule** — no `role === ...` / `isVenueManager(...)` branching. Apps own
  identity-conditional composition; shared components receive variation as props/slots
  (`AppLayout({ links })`, `ConcertDetails({ addReviewSlot, onBuyTickets })`). Keep the shared UI
  intentional: a fixed affordance stays declared in shared (`ConcertCard`'s Buy Tickets button,
  disabled when no `onBuyTickets` is supplied); only the app-specific behaviour or widget is
  injected.
- **Route rule** — only the universal route contract. The literals, and the typecheck gate that
  enforces them, are the `app-tiers` skill.

The test for new code: *"could every one of the five sites render this and run every call it makes,
with its own token, today?"* If only one site can — customer tickets, review eligibility/create,
manager payout onboarding — it belongs in that app's tree, even when that costs an extra slot prop
on a shared component.

This rule has been violated before and produced real bugs: shared review widgets fired
Customer-service calls with manager tokens (routine 401s, then a band-aid that stripped the
customer 401 logout handler), and customer route literals broke the venue/artist builds for months.
Don't fix a leak with a role check inside shared — that is the disease, not the cure. Move the code
to its owner and inject it back through a slot.
