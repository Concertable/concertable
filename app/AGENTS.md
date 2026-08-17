# Concertable — frontend (`app/`)

The React/TypeScript surfaces. **The generic TS/React standard is not in this repo** — it is a set of
load-on-demand skills (`typescript-style`, `contract-naming`, `react-structure`, `server-state`,
`client-state`, `http-layer`, `write-boundary`, `tiered-shared-code`, `stack-defaults`), and the task you
are doing is the trigger to read the matching one. Use the pattern, don't invent a local variant.

What lives here is the roster of real names those skills deliberately omit — the four HTTP clients, the
error seam, the `$type` unions: @./agents/CODE_CONVENTIONS.md; identity composition, the tenant session,
permissions: @./agents/CODE_PATTERNS.md.

## Five sharing tiers, widest first

Everything below inherits the two documents above. Each tier doc covers only what its own boundary
adds; none of them restate the conventions.

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
