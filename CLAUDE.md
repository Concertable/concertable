# Concertable

Concertable is a monorepo (a convenience, not the architecture) with a `.NET` microservices backend in `api/` and frontend surfaces in `app/`. The backend services own their runtime; cross-service deps are Contracts-only; standalone AppHosts are canonical. **Read [`api/ARCHITECTURE.md`](./api/ARCHITECTURE.md) before designing anything that crosses a service boundary.** Forgetting this leads to re-monolithing the system.

## Per-area guidance

- **Backend (.NET, `api/`)** — seeding, migrations, DTOs, module rules, C# conventions: [`api/CLAUDE.md`](./api/CLAUDE.md).
- **Design patterns the codebase commits to** (keyed strategy resolvers, and the anti-patterns they replace — branching on `ContractType` in agnostic code, service location, throwaway DTOs): [`api/docs/CODE_PATTERNS.md`](./api/docs/CODE_PATTERNS.md). Read it before adding any rule that varies by a closed key.
- **Web SPA (`app/web/`)** — [`app/web/CLAUDE.md`](./app/web/CLAUDE.md).
- **Customer cross-platform core (`app/customer/shared`, npm `@customer/shared`)** — consumed ONLY by the customer web + mobile apps: [`app/customer/shared/CLAUDE.md`](./app/customer/shared/CLAUDE.md).

## E2E suites — Docker health first, always

Run E2E only through `./e2e.ps1` via the matching skill (`e2e-ui-regress`, `e2e-ui-debug`,
`e2e-api-debug`) — the skill's Step 0 Docker pre-flight is mandatory, every run.

- **`docker ps` answering is NOT proof Docker is healthy.** Docker Desktop can be off, paused, or
  half-started with the engine still answering `docker ps` — even running containers — while
  host→container port forwarding is dead. The E2E signature of that state: every SQL/health
  connection is accepted then reset (`pre-login handshake` errors), services never become ready,
  and the whole suite dies at fixture startup in a few minutes with **zero scenarios executed**.
- **A suite that fails at startup is an environment problem until proven otherwise.** STOP after
  the first such run — do not rerun, do not debug application code. Verify Docker properly first:
  Docker Desktop UI shows Running, `docker run --rm hello-world` works, **and** a host→container
  port round-trip succeeds. Fix, then run once.

## Plans (`plans/*.md`)

Plans are working docs for unfinished work, **not** an archive — git history is the archive. A finished plan kept "for reference" is just rot that misleads the next reader into thinking the work is still pending.

- **When you land the commit that completes a plan's work, `git rm` the plan file in that same commit.** Completion = work committed AND its verification (build / tests / E2E) passed. Deletion belongs to that commit — never defer it to a later cleanup pass.
- A plan **superseded** by a newer plan, or describing a design that was **rejected**, is deleted the moment that's decided — don't leave a tombstone.
- A **partially-done** plan stays, but strike/check off the sections that shipped so what remains is only the outstanding work.
