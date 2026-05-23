# Concertable — Overview

Concertable is a platform that connects venues, artists, and fans around live music.
Venues book artists, artists find gigs, and customers buy tickets — all in one place.

## Core loop

A venue posts an **Opportunity** (an open slot tied to a **Contract** that defines how money will move), an artist **applies**, the venue **accepts**, and a **Concert** is automatically created. The venue then sets ticket price/quantity and customers buy tickets; after the concert, settlement runs against the chosen contract.

## Settlement contracts

Four contract types drive who pays whom and when (see `api/Modules/Contract/Concertable.Contract.Domain/Entities/`):

- **FlatFee** — venue pays the artist a fixed fee.
- **DoorSplit** — artist takes a % of ticket revenue.
- **VenueHire** — artist pays the venue a hire fee (artist-pays direction).
- **Versus** — guaranteed minimum to the artist + a % of ticket revenue (max of the two, or guarantee + split, depending on type — see `CalculateArtistShare`).

Each contract also carries a `PaymentMethod` (e.g. upfront vs settled-after-concert) that decides *when* money moves through Stripe.

## Two sites

Concertable runs as two separate sites:

- **B2B** (`api/Concertable.B2B/`) — the venue/artist side: opportunities, applications, contracts and post-concert settlement.
- **Customer marketplace** (`api/Concertable.Customer/`) — the fan-facing side: browsing concerts and buying tickets.

## Architecture

- **Backend** — .NET services split across `api/Concertable.B2B/` and `api/Concertable.Customer/`, each a modular host with modules under its own `Modules/` folder (Artist, Venue, Concert, Contract, Payment, Notification, Search, Identity/User, etc.). Cross-module calls go through `IXModule` facades in `Module.Contracts`; rules in [`MODULAR_MONOLITH_RULES.md`](./MODULAR_MONOLITH_RULES.md).
- **Frontend** — React SPA in `app/web/` split by audience (customer / venue / artist / business / shared); Expo mobile in `app/mobile/` (three apps + shared).
- **Payments** — Stripe Connect; Payment module is stateless money-movement, Concert orchestrates ticket purchase and settlement.
- **Infra** — Aspire AppHost (`api/Concertable.AppHost`) wires SQL Server, the API, the Workers host, and dev tunnels for mobile.

See `README.md` for run instructions and seeded test accounts.
