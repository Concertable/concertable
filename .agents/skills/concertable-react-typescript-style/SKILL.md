---
name: concertable-react-typescript-style
description: Concertable's frontend wire shapes — multipart `FormData` field names are PascalCase while JSON bodies stay camelCase because multipart binds to C# by property name, the three live `$type` discriminated unions (`PaymentAmount`, `Deal`, and the search `Header` and `AutocompleteResult` pair) whose literals are copied from the backend `[JsonDerivedType]` discriminators, and the universal `User` that deliberately carries no discriminator and no flat role. Use when typing a payload here, adding an upload, or modelling a shape the server sends in more than one form.
---

# TypeScript — Concertable's wire shapes

The generic standard is the `typescript-style` skill: `interface` versus `type`, camelCase matching the wire,
optional over nullable, discriminated unions with a `never` exhaustiveness arm. This file is the roster of
real shapes here.

## `FormData` field names are PascalCase; JSON bodies are camelCase

Multipart binds to C# by property name rather than through the JSON policy, so uploads use `"Name"`,
`"Banner"`, `"Genres[0]"` (`artistApi.ts`). Correct, and stays — it is not an inconsistency to normalise.

## The live `$type` unions

| Union | Members | Discriminator |
|---|---|---|
| `PaymentAmount` | `FlatPayment`, `DoorSharePayment`, `GuaranteedDoorPayment` | `$type` — `"flat"`, `"doorShare"`, `"guaranteedDoor"` |
| `Deal` | `FlatFeeDeal`, `DoorSplitDeal`, `VersusDeal`, `VenueHireDeal` | `$type`, mirroring `DealTypeNames` |
| `Header` / `AutocompleteResult` | the search pair | `HeaderType` |

The `$type` literals are copied from the backend `[JsonDerivedType]` discriminators key-for-key. `Deal` lives
in `@b2b/*`'s `features/deals/types.ts`.

## The universal `User` has no `$type` and no flat role

Product identity is composed in its owning tier — see [`IDENTITY.md`](../../standards/react/IDENTITY.md). Adding a discriminator
or a role field to the shared `User` is the widening that rule exists to stop.
