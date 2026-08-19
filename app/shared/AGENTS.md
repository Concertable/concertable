# app/shared — `@concertable/shared`, the widest tier

Inherits [`../AGENTS.md`](../AGENTS.md) — the tier map and the standards skills. The tier rule itself
belongs to the `tiered-shared-code` skill and `User`-versus-`B2bIdentity` to `concertable-identity`;
neither is restated here. This file is only this package's inventory and the homes of the concepts it
excludes.

Everything here compiles into **every** surface — customer and b2b, web and mobile — so it holds the
one meeting point all of them import: the `User`/auth types (`auth/types.ts`), the shared axios
clients, and cross-cutting hooks.

Where the excluded concepts live instead:

| Concept | Home |
|---|---|
| tenant type, membership, role, opportunity, contract, payout | `@b2b/*` (`app/web/b2b/shared`), consumed only by venue + artist — with `B2bIdentity` composed in `@b2b/features/tenant` |
| tickets, reviews, buyer concepts | the customer app / `@concertable/customer` |
