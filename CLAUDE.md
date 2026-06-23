# Concertable

Concertable is a monorepo (a convenience, not the architecture) with a `.NET` microservices backend in `api/` and frontend surfaces in `app/`. The backend services own their runtime; cross-service deps are Contracts-only; standalone AppHosts are canonical. **Read [`api/ARCHITECTURE.md`](./api/ARCHITECTURE.md) before designing anything that crosses a service boundary.** Forgetting this leads to re-monolithing the system.

## Autonomy — act on reversible work, don't ask

Decide and act on reversible work (doc/plan edits, isolated commits, retrying a transient failure), then report — no check-ins. Research: run end-to-end, update the relevant docs, commit in isolation. Pause only when an action is irreversible or contradicts what you find (e.g. unrelated work already staged) — flag it in one line and take the safe path, don't ask permission.

## Per-area guidance

- **Backend (.NET, `api/`)** — seeding, migrations, DTOs, module rules, C# conventions: [`api/CLAUDE.md`](./api/CLAUDE.md).
- **Design patterns the codebase commits to** (keyed strategy resolvers, and the anti-patterns they replace — branching on `ContractType` in agnostic code, service location, throwaway DTOs): [`api/docs/CODE_PATTERNS.md`](./api/docs/CODE_PATTERNS.md). Read it before adding any rule that varies by a closed key.
- **Web SPA (`app/web/`)** — [`app/web/CLAUDE.md`](./app/web/CLAUDE.md).
- **Customer cross-platform core (`app/customer/shared`, npm `@customer/shared`)** — consumed ONLY by the customer web + mobile apps: [`app/customer/shared/CLAUDE.md`](./app/customer/shared/CLAUDE.md).

## Git branch — branch first, capitalized type prefix, always

**Before starting any work, create a relevant branch for it if you're not already on one** — never commit to `master` or an unrelated branch.

**Don't branch to refactor code from the feature you're already on.** If the code only lives on the current feature branch (not yet in `master`), the refactor is part of that feature — stay on the branch and commit there. A new `Refactor/<Name>` branch is only for code **already merged to `master`**. Branching off an in-flight feature fragments it across two PRs and orphans the original.

Branches are named `<Type>/<Name>` with the type prefix **capitalized**: `Feature/`, `Refactor/`, `Bug/`, `Fix/`, etc. Never create a lowercase variant (`feature/...`). Windows' case-insensitive filesystem cannot hold two casings of the same ref, so a remote with both `feature/x` and `Feature/x` breaks `git fetch`/`git pull` for everyone ("cannot lock ref ... File exists"). Before creating a branch, match the casing of any existing branch of the same name exactly.

**Docs and plans are exempt from branch hygiene.** Non-code markdown — `plans/*.md`, any `TECH_DEBT.md`, scratch notes — is non-breaking and never affects a build or another PR, so just commit it on whatever branch you're already on. Don't branch for it, don't split it into its own commit, and don't worry if `git add -A` sweeps a stray plan/doc into a feature commit — bundling doc-only changes is fine, not worth a force-push to tidy up.

## E2E suites — Docker health first, always

This section is **how** to run E2E safely. **Whether** to run it for a given change is a judgment
call — reserved for massive or behaviorally-risky changes, skipped for stage-1/zero-behavior-change
work — governed by [`plans/CLAUDE.md`](./plans/CLAUDE.md). Don't run the full suites by reflex.

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

## Code comments — default to none; the commit message is the archive

Default to **zero** comments. The diff shows *what* changed; the commit message is where *why* lives (the incident, the root cause, the alternatives). A code comment is the exception, not the habit — **≤2 lines**, and only for a *why* a reader needs *at this line* and can't get from well-named identifiers. Anything longer belongs in the commit message, not the file; big inline explanations rot in place.

A comment is **wrong**, not merely long, if it:
- **restates reasoning already in a `CLAUDE.md`/`docs` file** — link it in a phrase, or omit it (two copies drift the day one changes);
- **cites a transient artifact** (a plan filename, "Phase N", a ticket) that will be deleted — the reference is engineered to dangle;
- **narrates the *what*** — well-named code already does that.

The one comment that always earns its place: a **single-line footgun/invariant warning** at the exact site a future edit would break something ("don't put X here — it would Y").

And if a comment needs a paragraph to justify the code below it, that's usually the *code* telling you it's hacky — do the proper fix, or if a quick fix is genuinely right, log it in the nearest `TECH_DEBT.md` and keep the comment short.

## Plans (`plans/*.md`)

Plans are working docs for unfinished work, **not** an archive — git history is the archive. A finished plan kept "for reference" is just rot that misleads the next reader into thinking the work is still pending. Full workflow (phases, verification gates, when to run E2E): [`plans/CLAUDE.md`](./plans/CLAUDE.md).

- **When you land the commit that completes a plan's work, `git rm` the plan file in that same commit.** Completion = work committed AND its verification passed (build + the affected unit/integration tests always; E2E only when the change is massive/risky per `plans/CLAUDE.md`). Deletion belongs to that commit — never defer it to a later cleanup pass.
- A plan **superseded** by a newer plan, or describing a design that was **rejected**, is deleted the moment that's decided — don't leave a tombstone.
- A **partially-done** plan stays, but strike/check off the sections that shipped (in the same commit as the work) so what remains is only the outstanding work.

## Throwaway working markdown — in the repo, then deleted

Ad-hoc markdown — investigation prompts, scratch analysis, handoff notes for another tool/agent — goes **in the repo**, never in a temp/scratchpad directory. The scratchpad is invisible to the user and to other tools operating on the repo, so a doc written there is effectively lost. Put it where it can be seen and used.

These are working docs, not an archive (same rule as plans): **delete the file once it's served its purpose** — the handoff happened, the question was answered, the analysis landed in code/commit. Don't let throwaway markdown accumulate as rot.
