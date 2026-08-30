# Concertable

Concertable connects venues, artists and fans around live music ([product overview](https://github.com/Concertable/docs/blob/main/OVERVIEW.md) — the product in one page, in the central [`Concertable/docs`](https://github.com/Concertable/docs) repo). It is a monorepo (a convenience, not the architecture) with a `.NET` microservices backend in `api/` and frontend surfaces in `app/`. The backend services own their runtime; cross-service deps are Contracts-only; standalone AppHosts are canonical. **Read the `microservice-boundaries` skill before designing anything that crosses a service boundary** (its design rationale and decision history are the `microservices-architecture` skill). Forgetting this leads to re-monolithing the system.

**No standard lives in this repo — not for code, not for process.** How work gets done is a set of load-on-demand skills installed from the standards plugins — the git, review, merge/PR, test-debug, plan and validation families, plus `docs-and-debt` and `failing-tests`; the always-on behavioral floor (scalable-fix, questions-before-actions, autonomy) is injected at every session start (the `floor` skill owns it). What stays in this repo is only what is true of *Concertable* — its real surfaces, scripts and suites, and the pointers below.

## Per-area guidance

**Topic → owner: [`docs/INDEX.md`](./docs/INDEX.md).** Look a topic up there before writing a rule down.

**The generic .NET and TypeScript/React standards are load-on-demand skills.** The task you are doing is the trigger to read the matching one — `csharp-style`, `persistence`, `multitenancy`, `keyed-strategies`, `result-carriers`, `seeding`, `unit-testing`, `react-structure`, `server-state`, `http-layer`, `tiered-shared-code`, and the rest. `.agents/skill-routes.json` maps path to skill and the write-time hook enforces it. The committed payload is a repository-local delivery copy; its shared source remains outside this repo. The in-repo docs below carry only what those skills deliberately omit: the roster of real types, contexts, clients and tables in *this* system.

- **Backend (.NET, `api/`)** — the floor and which skills apply are the `.agents/skill-routes.json` routes. The committed `.agents/skills` payload contains both Concertable's `concertable-dotnet-*` rosters and their `dotnet-standards-*` generic counterparts, so Codex never depends on a user plugin catalogue.
- **Frontend (React/TS, `app/`)** — the tier map: [`app/AGENTS.md`](./app/AGENTS.md). The committed `.agents/skills` payload contains both Concertable's `concertable-react-*` rosters and their `react-standards-*` generic counterparts. `app-tiers`, `identity`, and `permissions` remain Concertable-only skills.
- **Web SPA (`app/web/`)** — [`app/web/AGENTS.md`](./app/web/AGENTS.md).
- **Mobile apps (`app/mobile/`)** — [`app/mobile/AGENTS.md`](./app/mobile/AGENTS.md).
- **Customer cross-platform core (`app/customer/shared`, npm package `@concertable/customer`, exported as `@concertable/customer/shared/*`)** — consumed ONLY by the customer web + mobile apps: [`app/customer/shared/AGENTS.md`](./app/customer/shared/AGENTS.md).

## This repo's values and pointers

- **Plans** — this repo's layout, hooks and commands: [`plans/AGENTS.md`](./plans/AGENTS.md); the method and the shapes of the prompts that carry a plan are the `plans` and `handoff` skills.
- **Validation / E2E** — this repo's gates, commands and suites: [`docs/REMOTE_VALIDATION.md`](./docs/REMOTE_VALIDATION.md). Local E2E runs only through `./scripts/e2e.ps1` (gated on `./scripts/docker-health.ps1`) via the `e2e-*` skills, and only to diagnose a merge-queue failure; the policy is the `remote-validation` skill.
- **Worktree identity gate** — before any edit, state whether the task matches the current branch/PR or is branch-local work, verifying **service ownership**, the dirty paths and other worktrees rather than matching on a shared refactor name; if neither basis holds or anything contradicts it, STOP and ask. Branch discipline is the `git-branching` skill; the gate itself is the `open-worktree` skill.
