# Concertable — frontend (`app/`)

The React/TypeScript surfaces. TS/React code conventions: @./agents/CODE_CONVENTIONS.md (notably:
absent values are `undefined`, never `null`, unless "deliberately set to empty" is a distinct acted-on
state; reads are named for the domain noun with no `Dto`/`Response` suffix; writes are `XRequest`).
Design patterns the frontend commits to (structure — slots over role checks, hooks orchestrate while
components render, one `xApi` per resource, the zod write boundary, table dispatch on a closed key):
@./agents/CODE_PATTERNS.md — use the pattern, don't invent a local variant.

## Five sharing tiers, widest first

Everything below inherits the two documents above. Each tier doc covers only what its own boundary
adds; none of them restate the conventions.

| Tier | Compiles into | Rules |
|---|---|---|
| `app/shared` (`@concertable/shared`) | **every** surface — customer + b2b, web + mobile | [`shared/AGENTS.md`](./shared/AGENTS.md) |
| `app/customer/shared` (`@concertable/customer`) | customer web + mobile only | [`customer/shared/AGENTS.md`](./customer/shared/AGENTS.md) |
| `app/web` | the five web SPAs | [`web/AGENTS.md`](./web/AGENTS.md) |
| `app/mobile` | the two mobile apps | [`mobile/AGENTS.md`](./mobile/AGENTS.md) |
| per-app `src/` | one site only | — |

The tier rule is the same at every level: code belongs at the **widest tier every consumer can
legitimately run**, and variation is injected from the owning app through props/slots — never resolved
inside shared code with an identity check.

## The build gate

All five web builds green, and both mobile builds green, is the boundary gate — each app's typecheck
compiles the shared trees against its own tree, so a leak in shared fails a *different* app's build.
Web commands: [`web/AGENTS.md`](./web/AGENTS.md). Mobile commands: [`mobile/AGENTS.md`](./mobile/AGENTS.md).
