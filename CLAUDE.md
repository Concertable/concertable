# Concertable

Concertable is a monorepo (a convenience, not the architecture) with a `.NET` microservices backend in `api/` and frontend surfaces in `app/`. The backend services own their runtime; cross-service deps are Contracts-only; standalone AppHosts are canonical. **Read [`api/ARCHITECTURE.md`](./api/ARCHITECTURE.md) before designing anything that crosses a service boundary.** Forgetting this leads to re-monolithing the system.

## Per-area guidance

- **Backend (.NET, `api/`)** — seeding, migrations, DTOs, module rules, C# conventions: [`api/CLAUDE.md`](./api/CLAUDE.md).
- **Design patterns the codebase commits to** (keyed strategy resolvers, and the anti-patterns they replace — branching on `ContractType` in agnostic code, service location, throwaway DTOs): [`api/docs/CODE_PATTERNS.md`](./api/docs/CODE_PATTERNS.md). Read it before adding any rule that varies by a closed key.
- **Web SPA (`app/web/`)** — [`app/web/CLAUDE.md`](./app/web/CLAUDE.md).
- **Customer cross-platform core (`app/customer/shared`, npm `@customer/shared`)** — consumed ONLY by the customer web + mobile apps: [`app/customer/shared/CLAUDE.md`](./app/customer/shared/CLAUDE.md).

## Git branch naming — capitalized type prefix, always

Branches are named `<Type>/<Name>` with the type prefix **capitalized**: `Feature/`, `Refactor/`, `Bug/`, `Fix/`, etc. Never create a lowercase variant (`feature/...`). Windows' case-insensitive filesystem cannot hold two casings of the same ref, so a remote with both `feature/x` and `Feature/x` breaks `git fetch`/`git pull` for everyone ("cannot lock ref ... File exists"). Before creating a branch, match the casing of any existing branch of the same name exactly.

## E2E suites — Docker health first, always

Run E2E only through `./e2e.ps1` via the matching skill (`e2e-ui-regress`, `e2e-ui-debug`,
`e2e-api-debug`) — the skill's Step 0 Docker pre-flight is mandatory, every run.

- **`docker ps` answering is NOT proof Docker is healthy.** Docker Desktop can be off, paused, or
  half-started with the engine still answering `docker ps` — even running containers — while
  host→container forwarding of real bytes for NEW containers is dead. The E2E signature of that
  state: every SQL/health connection is accepted then reset (`pre-login handshake` errors), services
  never become ready, and the whole suite dies at fixture startup in a few minutes with **zero
  scenarios executed**.
- **`docker ps`, `docker run hello-world`, and a bare TCP connect are ALL insufficient.** hello-world
  needs no port forwarding, and the host-side `docker-proxy` completes a TCP handshake *locally* even
  when forwarding into the container is dead — so a connect "succeeds" while no data flows (exactly
  the `pre-login handshake` mode). The only valid check is a real **data** round-trip to a fresh
  container: run **`./docker-health.ps1`** (fresh container + published port + HTTP round-trip +
  stability check; exit 1 = unhealthy). `./e2e.ps1` runs it as an automatic gate before booting.
- **A suite that fails at startup is an environment problem until proven otherwise.** STOP after
  the first such run — do not rerun, do not debug application code. Verify Docker with
  `./docker-health.ps1` (and Docker Desktop showing **Running**). Fix, then run once.

## Tech debt (`TECH_DEBT.md`)

Avoid introducing tech debt wherever possible. But when a quick fix is the right call, or you notice or introduce debt the user is aware of, log a line in the `TECH_DEBT.md` nearest the area you touched (there's one per area — use the closest, not the root).

## Code comments — size is a smell signal

A comment that needs a paragraph to justify the code below it is usually telling you the code is hacky. If it is, do the proper fix — or, if a quick fix is genuinely the right call, log it in the nearest `TECH_DEBT.md` and keep the comment short. If the code is sound, it doesn't need a wall of text: state the non-obvious *why* in a line or two and let the commit message carry the full story (the incident, the root cause, the alternatives). Big inline explanations rot in place; commit messages are the archive.

## Plans (`plans/*.md`)

Plans are working docs for unfinished work, **not** an archive — git history is the archive. A finished plan kept "for reference" is just rot that misleads the next reader into thinking the work is still pending.

- **When you land the commit that completes a plan's work, `git rm` the plan file in that same commit.** Completion = work committed AND its verification (build / tests / E2E) passed. Deletion belongs to that commit — never defer it to a later cleanup pass.
- A plan **superseded** by a newer plan, or describing a design that was **rejected**, is deleted the moment that's decided — don't leave a tombstone.
- A **partially-done** plan stays, but strike/check off the sections that shipped so what remains is only the outstanding work.
