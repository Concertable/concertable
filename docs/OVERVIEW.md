# Concertable — Overview

Concertable is a platform that connects venues, artists, and fans around live music.
Venues book artists, artists find gigs, and customers buy tickets — all in one place.

## Core loop

A venue posts an **Opportunity** — an open slot carrying a **Deal** that defines how money will move.
An artist **applies**, the venue **accepts**, and a **Concert** is created. The venue then sets ticket
price and quantity, customers buy tickets, and after the concert settlement runs against the deal.

A **Contract** is a different thing from a Deal, and the two are easy to confuse: the Deal is the money
terms, and the Contract is the frozen snapshot minted at Accept and rendered as a PDF. It lives in the
Concert module (`ContractEntity`, `IContractIssuer`) — there is no Contract module.

## The four deal types

Declared in `api/Concertable.B2B/src/Modules/Deal/`, and each resolved at settlement by its own
`ISettlementAmountResolver` in the Concert module:

| Deal | Terms | Gross to the artist |
|---|---|---|
| **FlatFee** | `Fee` | the fee |
| **DoorSplit** | `ArtistDoorPercent` | that percent of declared door revenue |
| **VenueHire** | `HireFee` | — the *artist* pays the venue |
| **Versus** | `Guarantee` + `ArtistDoorPercent` | guarantee **plus** the percent, not the greater of the two |

Every deal also carries a `PaymentMethod` — `Cash` or `Transfer` — which decides whether the money moves
through Stripe at all.

## Five services

The monorepo is a convenience; each service is independently owned. Full topology and what may depend on
what: [`api/ARCHITECTURE.md`](../api/ARCHITECTURE.md).

| Service | Owns |
|---|---|
| **B2B** | the venue/artist side — modules Artist, Venue, Concert, Deal, Conversations, Tenant, User |
| **Customer** | the fan-facing marketplace — Artist, Venue, Concert, Ticket, Review, Preference, User |
| **Search** | the event-fed search read model |
| **Payment** | Stripe Connect money movement, escrow and payouts |
| **Auth** | OIDC/OAuth via Duende |

## Frontend

Four web SPAs (customer, venue, artist, business) in `app/web/` and two Expo apps (customer, b2b) in
`app/mobile/`, sharing code through nested packages. The workspace inventory and the boundary gate:
[`app/README.md`](../app/README.md) and [`app/AGENTS.md`](../app/AGENTS.md).

## Running it

`api/Concertable.AppHost` is the Aspire host that wires all five services, their SQL databases, the
Service Bus emulator, Azure Storage, the four SPAs, the mobile dev tunnels and the Stripe CLI. Each
service also has a standalone AppHost, and those are the canonical ones. Commands and seeded test
accounts: [`README.md`](../README.md).
