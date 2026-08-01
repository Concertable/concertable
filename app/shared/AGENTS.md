# app/shared — `@concertable/shared`, the universal cross-platform core

## This is the WIDEST tier. Everything here compiles into EVERY surface — customer AND b2b, web AND mobile. It is the intersection, never the union.

Concertable is two products (customer marketplace + b2b manager platform) across two platforms
(web + mobile). This package is the one meeting point all of them import: the `User`/auth types, the
shared axios clients, cross-cutting hooks. Because a Customer bundle, an Artist/Venue manager bundle,
and the mobile apps all compile this tree, code here may only model what **every** one of them
legitimately has.

**Never put a product- or persona-specific concept on a shared type.** Same rule as the backend
(`api/CLAUDE.md` — "shared is the intersection, never the union"): a member that is only ever
populated or meaningful for one product/persona — and is dead weight for the others — does not belong
here.

- **B2B-only** (tenant / membership / role / opportunity / contract / payout concepts) → live in
  `@b2b/*` (`app/web/b2b/shared`), consumed only by venue + artist. **Do not** add `TenantType`,
  `TenantRole`, `Membership`, or a `memberships` field to the `User` type here — a customer/mobile user
  has none. They live in `@b2b/features/tenant`, and B2B composes them onto the base user as
  `B2bIdentity` (`User` + `memberships`), populated by a B2B-owned typed `/me` — never a field or cast
  on the shared `User`.
- **Customer-only** (tickets, reviews, buyer concepts) → live in the customer app / `@customer/shared`.

The litmus: *could every surface — customer AND manager, web AND mobile — legitimately carry this
member and populate it today?* If only one product or persona can, it belongs in that tree, even at
the cost of a per-app cast or slot.

## `User` is the flat intersection — compose personas on top, never widen it

`auth/types.ts` `User` is a single identity interface
(`id` / `email` / location / `isEmailVerified`) — **not** a persona union. The old
`VenueManager | ArtistManager | Customer | Admin` subtypes (with `venueId`/`artistId`/`$type`) and the
`isVenueManager`/`isArtistManager` guards are gone; a product persona is composed in its owning tier
(B2B's `B2bIdentity` = `User` + `memberships`). The flat `role` field is gone. Don't reintroduce
persona subtypes or product-specific fields on `User`.
