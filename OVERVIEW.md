# Concertable — Overview

Concertable is a platform that connects venues, artists, and fans around live music.
Venues book artists, artists find gigs, and customers buy tickets — all in one place.

## Core loop

A venue posts an **Opportunity** (an open slot tied to a **Contract** that defines how money will move), an artist **applies**, the venue **accepts**, and a **Concert** is automatically created. The venue then sets ticket price/quantity and customers buy tickets; after the concert, settlement runs against the chosen contract. a venue posts an **Opportunity** (an open slot tied to a **Contract** that defines how money will move), an artist **applies**, the venue **accepts**, and a **Concert** is automatically created. The venue then sets ticket price/quantity and customers buy tickets; after the concert, settlement runs against the chosen contract.

## Settlement contracts

Four contract types drive who pays whom and when (see `api/Modules/Contract/Concertable.Contract.Domain/Entities/`):

- **FlatFee** — venue pays the artist a fixed fee.
- **DoorSplit** — artist takes a % of ticket revenue.
- **VenueHire** — artist pays the venue a hire fee (artist-pays direction).
- **Versus** — guaranteed minimum to the artist + a % of ticket revenue (max of the two, or guarantee + split, depending on type — see `CalculateArtistShare`).

Each contract also carries a `PaymentMethod` (e.g. upfront vs settled-after-concert) that decides *when* money moves through Stripe.

## Architecture

- **Backend** — .NET modular monolith under `api/Modules/` (Artist, Venue, Concert, Contract, Payment, Notification, Messaging, Search, Customer, Identity/User, Authorization). Cross-module calls go through `IXModule` facades in `Module.Contracts`; rules in [`MODULAR_MONOLITH_RULES.md`](./MODULAR_MONOLITH_RULES.md).
- **Frontend** — React SPA in `app/web/` split by audience (customer / venue / artist / business / shared); Expo mobile in `app/mobile/` (three apps + shared).
- **Payments** — Stripe Connect; Payment module is stateless money-movement, Concert orchestrates ticket purchase and settlement.
- **Infra** — Aspire AppHost (`api/Concertable.AppHost`) wires SQL Server, the API, the Workers host, and dev tunnels for mobile.

See `README.md` for run instructions and seeded test accounts.
