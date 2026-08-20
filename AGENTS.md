# Concertable

Concertable connects venues, artists and fans around live music ([`docs/OVERVIEW.md`](./docs/OVERVIEW.md) — the product in one page). It is a monorepo (a convenience, not the architecture) with a `.NET` microservices backend in `api/` and frontend surfaces in `app/`. The backend services own their runtime; cross-service deps are Contracts-only; standalone AppHosts are canonical. **Read [`api/ARCHITECTURE.md`](./api/ARCHITECTURE.md) before designing anything that crosses a service boundary.** Forgetting this leads to re-monolithing the system.

**No standard lives in this repo — not for code, not for process.** How work gets done is a set of
load-on-demand skills: `git-branching`, `committing`, `merging`, `remote-validation`, `plans`,
`failing-tests`, `docs-and-debt`. What stays here is only what is true of *Concertable* — its real
labels, workflows, scripts and commands, plus the few invariants whose violation is silent and
expensive enough not to wait for a skill invocation.

## Always take the scalable, long-term approach — never the hacky quick fix

**When two solutions present themselves, take the one that is correct for the long term, even when it is harder, larger, or slower to land.** Never reach for the quick hack, the shim, the special-case, the timeout/retry bumped to ride out a flake, or the "just make it work for now" — a workaround that unblocks today becomes the landmine someone trips on later (this very tech-debt backlog is full of exactly those). The proper, scalable fix is the default and the expectation, not a nice-to-have to weigh against effort.

- **Multiple PRs, cross-package cut-overs, publish-first migrations, extra scaffolding — all fine.** Scope is never a reason to pick the worse design. If the right fix needs three PRs or crosses a package boundary, do it in three PRs; say so in one line and proceed. Splitting the *delivery* of the correct solution is encouraged; substituting a *worse* solution to fit one PR is not.
- **A shortcut is only acceptable when it is genuinely, provably the right call** (e.g. deferring live tax-ID verification that overlaps Stripe) — and then it is *logged* in the owning `TECH_DEBT.md` with the reasoning, never left silent.
- **If effort/complexity is pushing you toward the lesser option, surface that as a trade-off for Tommy to decide — do not quietly downgrade the solution.** The bias is always toward the durable, maintainable, architecturally-honest answer.

## Questions come before actions

When Tommy asks a question, answer it directly before taking any action. Discussion of possible work,
numbered options, prompts, branches, or plans is not authorization to execute it. If one message both
asks a question and explicitly requests an action, answer the question first, then perform only the
explicitly requested action.

## Autonomy — act on reversible work, don't ask

Decide and act on reversible work (doc/plan edits, isolated commits, retrying a transient failure), then report — no check-ins. Research: run end-to-end, update the relevant docs, commit in isolation. Pause only when an action is irreversible or contradicts what you find (e.g. unrelated work already staged) — flag it in one line and take the safe path, don't ask permission.

**Never gate a reversible local (working-tree) change behind a "should I?" — just make it.** Editing / writing / refactoring a file, or running a plan's code steps, is the default action, never a question and never a "just report / do nothing" menu. When to commit and when to push is the `committing` skill.

**Completed docs/meta-only work is the exception to the push gate:** once reviewed, commit, push, and
merge it through `/merge-docs` without waiting for another instruction, keeping agent-loaded guidance current via the no-E2E docs path.

**If requested work depends on a PR that does not exist, create it and do the work; never hand back the same blocked prompt.**

**A terminal delivery instruction authorizes the required delivery chain.** When Tommy says to merge,
ship, finish a cut-over, sort it out, or otherwise complete plan-managed work, that authorization
includes every required producer and consumer PR, package publication, generated platform-sync PR,
and merge in that delivery chain. Do not stop to request the same authorization again unless Tommy
explicitly limits it to a named PR or stage.

## Per-area guidance

**Every rule has one owning doc or skill — [`docs/INDEX.md`](./docs/INDEX.md) maps topic → owner, lists what a machine already enforces, and carries the rules for adding to the corpus. Look a topic up there before writing a rule down; elsewhere links, never restates.**

**Doc locality — a guidance/architecture doc lives at the lowest node that fully contains its concern:** single-service → that service's own folder (thin, inheriting root + `api/` upward, never restating — e.g. [`api/Concertable.Payment/AGENTS.md`](./api/Concertable.Payment/AGENTS.md)); cross-service or orchestration → root. Create one only where genuine service-specific content exists.

**Every `AGENTS.md` gets a `CLAUDE.md` sibling containing exactly `@AGENTS.md`, and every guidance doc must be reachable — by plain link or `@`-import, followed transitively — from some `AGENTS.md`/`CLAUDE.md`/`SKILL.md`.** A doc that fails this is loaded nowhere, which is exactly how 754 lines of frontend law went unread until a shipped feature violated it. Mechanically checked by `.agents/hooks/docs_reachability.py`, run as part of `docs-review` — which also fails a guidance doc that links a file that doesn't exist, or uses a root-absolute `/api/...` path (dead links in `plans/`/`reviews/` only warn).

**The generic .NET and TypeScript/React standards are load-on-demand skills, not files in this repo.** The task you are doing is the trigger to read the matching one — `csharp-style`, `persistence`, `multitenancy`, `keyed-strategies`, `result-carriers`, `seeding`, `unit-testing`, `react-structure`, `server-state`, `http-layer`, `tiered-shared-code`, and the rest. `.agents/skill-routes.json` maps path to skill and the write-time hook enforces it. The in-repo docs below carry only what those skills deliberately omit: the roster of real types, contexts, clients and tables in *this* system.

- **Backend (.NET, `api/`)** — the floor and which skills apply: [`api/AGENTS.md`](./api/AGENTS.md). This system's own rosters come from the `dotnet` plugin in `Concertable/agent-standards`, which pairs name-for-name with `dotnet-standards` from `tomjseery/dotagents` — `persistence` (the `Concertable.DataAccess` capability hierarchy), `seeding` (the forbidden-table inventory), `packages` (Reunion pins, the carve gates), `http-clients` (the Refit inventory), `microservice-boundaries` (the service roster). Same skill name on both sides; the plugin says which one you mean.
- **Frontend (React/TS, `app/`)** — the tier map: [`app/AGENTS.md`](./app/AGENTS.md). This system's own rosters come from the `react` plugin in `Concertable/agent-standards`, pairing name-for-name with `react-standards` from `tomjseery/react-agents` — `http-layer` (the four clients, the `isApiError` seam), `typescript-style` (the live `$type` unions), plus `app-tiers` (the surfaces and the typecheck gate), `identity` (`User` vs `B2bIdentity`) and `permissions` (`SharedPermissions`), which have no generic counterpart. Same rule as above: same skill name on both sides, and the plugin says which one you mean.
- **Web SPA (`app/web/`)** — [`app/web/AGENTS.md`](./app/web/AGENTS.md).
- **Mobile apps (`app/mobile/`)** — [`app/mobile/AGENTS.md`](./app/mobile/AGENTS.md).
- **Customer cross-platform core (`app/customer/shared`, npm package `@concertable/customer`, exported as `@concertable/customer/shared/*`)** — consumed ONLY by the customer web + mobile apps: [`app/customer/shared/AGENTS.md`](./app/customer/shared/AGENTS.md).

## Git branch — branch first, then prove the worktree is the right one

**Before starting any work, create a relevant branch for it if you're not already on one** — never commit to `main` or an unrelated branch. Naming, casing, where to branch from, and when a refactor stays on its feature branch are the `git-branching` skill.

**Worktree identity gate — before any edit.** State whether the task matches the current branch/PR directly or is branch-local work because it changes code not yet in `main`; verify service ownership, the dirty paths, and other worktrees rather than matching on a shared refactor name. If neither basis holds or anything contradicts it, **STOP and ask**.

**Durable guidance leaves a feature branch immediately.** When feature work changes an `AGENTS.md`, an
`agents/*.md` playbook, a skill, or another cross-cutting instruction, split that change onto a `Docs/*`
branch from `origin/main`, review it, and land it through `/merge-docs`. Working markdown — `plans/*.md`,
any `TECH_DEBT.md`, scratch notes — rides its owning branch.

## Ready for review is not merge authorization

Changing a PR from draft to ready only changes its review state. No workflow may enable auto-merge or
merge a normal PR in response to `opened`, `reopened`, `synchronize`, or `ready_for_review`. Only an
explicit `/merge` / `/merge-docs` instruction may start the delivery workflow. Repository-owned
generated PRs such as `chore/platform-sync-*` may enable auto-merge for themselves as part of the
already-authorized producer delivery chain; that scoped automation must never apply to ordinary PRs.

## Merging — `/merge` owns the procedure; these are the invariants

The run-book is [`.agents/skills/merge/SKILL.md`](./.agents/skills/merge/SKILL.md) — currency check, E2E
tier selection, enqueue, the four-state confirm loop, and the platform-sync follow-through. The generic
rule and the reasoning behind each state are the `merging` skill. Four things are expensive and silent
enough to state here:

- **Never enable auto-merge on a branch behind `main`.** Update, rebuild to 0 errors, push, *then* arm
  `--auto`. GitHub otherwise holds the PR `BLOCKED`/`BEHIND` so it silently never merges, or merges code
  never built against current `main`. Do it in the branch's **own** checkout — a session sitting in the
  main checkout is how the staleness goes unnoticed. An `api/**` branch that's behind also carries a stale
  `<ConcertablePlatformVersion>` pin, which merging `origin/main` fixes.
- **A failed check is a real failure.** Surface it and debug it; never retry it, never toggle auto-merge
  to shake it loose.
- **Never use the `Monitor` tool** to wait for a merge — its detached poller silently missed merges here
  (it timed out instead of firing). Confirm with a capped Bash `run_in_background` until-loop that echoes
  its state every poll and never swallows poll errors.
- **Whoever merges owns the platform-sync PR** the merge triggers — below.

## Platform sync is a live gate — a package merge isn't done until its sync PR is green

Any merge that touches `api/**` makes `publish-packages` republish and `platform-sync` open a
`chore/platform-sync-*` PR that bumps every service's `<ConcertablePlatformVersion>` to the new
version (MinVer bumps it on every merge). **Non-breaking → the sync PR auto-merges green in minutes.
Breaking** — a published type's shape/namespace moved and a consumer no longer compiles against the
new pin — **→ the sync PR goes RED, and until it's fixed every service is stranded on a broken
platform pin.**

- **Whoever merges owns the sync.** Follow the `chore/platform-sync-*` PR to green/merged — or, if it's
  red, migrate the failing consumer(s) **in that PR** (legal now: the new version is on the feed), build
  `api/Concertable.slnx` to 0 errors, and push. `/merge` step 6 automates this; do it by hand if you
  merged another way. **Never leave a red sync PR behind.**
- **Before branching for new feature work, confirm no open red sync PR** — don't build on a mid-break
  platform. This is a **branch-time** check (the cheap checkpoint), *not* a per-prompt one:
  ```bash
  sp=$(gh pr list --state open --json number,headRefName --jq '.[] | select(.headRefName|startswith("chore/platform-sync-")) | .number' | head -1)
  [ -n "$sp" ] && gh pr checks "$sp" | awk -F'\t' '$2=="fail"'   # any output → clear it before starting new work
  ```
- **Automated backstop (no action needed):** `.github/workflows/platform-sync-alert.yml` opens a
  tracking Issue + labels the PR `platform-sync-broken` the moment a sync goes red (and closes the
  Issue when it greens), so a broken sync can't rot unnoticed even when the merge bypassed `/merge`.

## Validation is remote-first, and E2E runs only through its script

Concertable is developed across concurrent worktrees. Which gate belongs to the workstation and which to
CI is the `remote-validation` skill; this repo's own commands, suites and exceptions are
[`docs/REMOTE_VALIDATION.md`](./docs/REMOTE_VALIDATION.md). The merge skill's Step 4 is the single source
of truth for the merge-queue E2E tier — apply it mechanically, and never run E2E locally to be safe.

Run E2E only through `./scripts/e2e.ps1` via the matching skill (`e2e-ui-regress`, `e2e-ui-debug`,
`e2e-api-debug`), which gates on `./scripts/docker-health.ps1` — a real data round-trip to a fresh
container, because `docker ps` answering proves nothing. **A suite that dies at fixture startup with zero
scenarios executed is an environment failure:** STOP, verify Docker, then run once. Do not rerun and do
not debug application code.

## Plans (`plans/*.md`) and the prompts that carry them

The lifecycle and the method — roadmap → plan → ledger, phases and their gates, what is deleted when, the
two dependency graphs, cross-plan blockers — is the `plans` skill; the shape of every continuation, resume,
handoff or implementation prompt is the `handoff` skill. This repo's layout, hooks and commands are
[`plans/AGENTS.md`](./plans/AGENTS.md), and **opening a `plans/*.md` to work from obliges you to read it in
the same breath**; the plan's own prose is not a substitute for it.

## Worktree cleanup is repository automation, not an AI audit

Use `./scripts/worktrees.ps1 audit` for a read-only inventory. It fetches once, queries PRs once, and
classifies registered worktrees with Git evidence; it never deletes. Use `close` only with the exact
merged PR, adding `-PlanManaged` for plan work. Use `retire` only for a superseded no-PR branch after
the retirement decision is committed on `main`. The commands refuse dirty, detached, mismatched,
post-PR, case-colliding, persistent, and missing-ledger states. Routine cleanup needs no AI or schedule.
