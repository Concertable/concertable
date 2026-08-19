# Concertable — frontend (`app/`)

The React/TypeScript surfaces. **No standard lives in this repo.** The generic TS/React rules are
load-on-demand skills (`typescript-style`, `contract-naming`, `react-structure`, `server-state`,
`client-state`, `http-layer`, `write-boundary`, `tiered-shared-code`, `stack-defaults`, `routing`,
`ui-components`, `data-tables`, `date-formatting`, `frontend-testing`), and what is true
of *this* system is their same-named counterparts in the `react` plugin — `http-layer` (the four clients and the
`isApiError` seam), `typescript-style` (the live `$type` unions, multipart casing),
`identity` (`User` versus `B2bIdentity`), `permissions`,
`client-state` (the tenant session), `contract-naming`,
`write-boundary` and `react-structure`. The task you are doing is the trigger to
load the matching pair; `.agents/skill-routes.json` maps path to skill.

**The surface roster, the five sharing tiers, the universal route contract and the typecheck boundary
gate are the `app-tiers` skill.** Load it before placing code in a shared tree, adding a route literal,
or verifying a change that touched one. Each tier's own `AGENTS.md` below carries only its inventory and
what its own boundary adds:

- `app/shared` (`@concertable/shared`) — [`shared/AGENTS.md`](./shared/AGENTS.md)
- `app/customer/shared` (`@concertable/customer`) — [`customer/shared/AGENTS.md`](./customer/shared/AGENTS.md)
- `app/web` — [`web/AGENTS.md`](./web/AGENTS.md)
- `app/mobile` — [`mobile/AGENTS.md`](./mobile/AGENTS.md)
