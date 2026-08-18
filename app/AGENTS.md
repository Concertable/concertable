# Concertable — frontend (`app/`)

The React/TypeScript surfaces. **No standard lives in this repo.** The generic TS/React rules are
load-on-demand skills (`typescript-style`, `contract-naming`, `react-structure`, `server-state`,
`client-state`, `http-layer`, `write-boundary`, `tiered-shared-code`, `stack-defaults`), and what is true
of *this* system is their `concertable-` counterparts — `concertable-http-layer` (the four clients and the
`isApiError` seam), `concertable-typescript-style` (the live `$type` unions, multipart casing),
`concertable-identity` (`User` versus `B2bIdentity`), `concertable-permissions`,
`concertable-client-state` (the tenant session), `concertable-contract-naming`,
`concertable-write-boundary` and `concertable-react-structure`. The task you are doing is the trigger to
load the matching pair; `.agents/skill-routes.json` maps path to skill.

What stays here is the tier map — which is structural, not a convention, and is what every tier doc below
inherits.

## Five sharing tiers, widest first

Each tier doc covers only what its own boundary adds; none of them restate the standards.

| Tier | Compiles into | Rules |
|---|---|---|
| `app/shared` (`@concertable/shared`) | **every** surface — customer + b2b, web + mobile | [`shared/AGENTS.md`](./shared/AGENTS.md) |
| `app/customer/shared` (`@concertable/customer`) | customer web + mobile only | [`customer/shared/AGENTS.md`](./customer/shared/AGENTS.md) |
| `app/web` | the four web SPAs | [`web/AGENTS.md`](./web/AGENTS.md) |
| `app/mobile` | the two mobile apps | [`mobile/AGENTS.md`](./mobile/AGENTS.md) |
| per-app `src/` | one site only | — |

The tier rule is the same at every level: code belongs at the **widest tier every consumer can
legitimately run**, and variation is injected from the owning app through props/slots — never resolved
inside shared code with an identity check.

## The build gate

All four web builds green, and both mobile builds green, is the boundary gate — each app's typecheck
compiles the shared trees against its own tree, so a leak in shared fails a *different* app's build.
Web commands: [`web/AGENTS.md`](./web/AGENTS.md). Mobile commands: [`mobile/AGENTS.md`](./mobile/AGENTS.md).
