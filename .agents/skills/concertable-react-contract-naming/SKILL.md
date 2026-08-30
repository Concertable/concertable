---
name: concertable-react-contract-naming
description: The only two frontend contract types in Concertable that keep a suffix — `GeocodeResponse` for the raw Google Geocoding envelope and `AxiosResponse` — both third-party wrappers genuinely different from the shape we hand back, while a suffix on one of our own types is still the thing to remove. Use when naming a client type that mirrors a server or third-party payload, or when deciding whether an existing suffix earns its place.
---

# Contract naming — the envelopes that keep a suffix here

The generic standard is the `contract-naming` skill: reads take the plain domain noun with no `Dto` or
`Response` suffix, writes take `XRequest`, and a suffix survives only where it distinguishes two real shapes.

Two here genuinely do:

- **`GeocodeResponse`** — the raw Google Geocoding `{ status, results[] }` envelope, genuinely different from
  the `Coordinates` we hand back from it.
- **`AxiosResponse`** — likewise a third-party envelope, not our contract.

Both are third-party wrappers we do not own. That is the whole exception; a suffix on one of our own shapes
is still the thing to remove.
