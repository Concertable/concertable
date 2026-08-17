# Guidance-docs restructure — progress

- Plan: `plans/docs/GUIDANCE_DOCS_RESTRUCTURE_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/guidance-restructure`
- Also delivered by this ledger: roadmap item `docs/agent-standards`, now checked off
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-guidance-docs`
- Branch: `Docs/GuidanceDocsRestructure`
- PR: #637 — ready, head `aa5759d5`, label `skip-e2e`, all 54 PR checks pass (6 `skipping`: both E2E, `carve-fe`, `fe-boundaries`, `review`). Awaiting enqueue.
- Shared repos: `Concertable/agent-standards` (7 process skills) and `tomjseery/dotagents` (28 generic, synced to `~/.agents/skills/`), cloned at `C:\Users\TommySeery\source\repos\{agent-standards,dotagents}`
- Dependency/package gates: no consumer migration to do, but this PR **will** trigger publish + platform sync — `publish-packages.yml` triggers on the coarse `paths: api/**`, which this branch's `api/**` markdown matches. MinVer republishes and a `chore/platform-sync-*` PR opens; non-breaking (no published type changed), so it should auto-merge green. Follow it to green anyway — whoever merges owns the sync.
- Last reconciled: 2026-08-17 against `origin/main` (0 behind at review time), plus `agent-standards` `8c42daa`

## Current state

The reduction has happened. Every generic rule now has exactly one home — a skill — and the in-repo docs hold
only this system's roster of real types, contexts, clients, tables and pins. The corpus that auto-loads on an
`api/**` prompt went from **1,429 lines to 246**, and on an `app/**` prompt from **786 to 151**; a unit-test
project no longer pulls in 80 lines and an E2E project no longer pulls in 37.

What remains is Phase 3c (the markdown outside the conventions folders), Phase 4 (the duplication rows that
still have >1 home, chiefly seeding across `api/AGENTS.md` and `SEEDING_CONVENTIONS.md`), and the deferred
auto-load thinning of root `AGENTS.md`.

## Done

**Phase 3b — the in-repo corpus reduced to its roster** (this PR)

- `api/agents/*` **1,986 → 349** lines, `app/agents/*` **752 → 118**. Each surviving file opens by naming the
  skills that own its generic half, then carries only what those skills deliberately omit.
- **Four files deleted, their local remnants rehomed.** `UNIT_CONVENTIONS.md` (fully owned by `unit-testing`,
  which also settles its open Shouldly question with "one assertion library per tier");
  `DEBUGGING_CONVENTIONS.md` (owned by `logging`, and its separate existence was what let it contradict
  `CA1848`); `E2E_CONVENTIONS.md` → the local eight lines now sit in
  `Concertable.Shared/tests/Concertable.Testing.E2E/AGENTS.md`, beside the baseline file they name;
  `MICROSERVICE_COMMUNICATION.md` → the per-service surface table folded into `api/ARCHITECTURE.md`.
- **`CONVENTIONS.md` → `MODULE_STRUCTURE.md`**, killing the collision with `CODE_CONVENTIONS.md`, and its
  "modules in the monolith" framing replaced with "each service is a modular monolith *internally*".
- **B2B's scoped topics left the api-wide floor.** Context stances, the filtered/unfiltered entity list, the
  `DealType` strategy families and the workflow steps now live in `api/Concertable.B2B/CODE_PATTERNS.md`,
  imported only by `api/Concertable.B2B/AGENTS.md` — so Customer, Search, Payment and Auth stop paying ~150
  lines they can never act on. B2B's active-tenant *naming* rules joined the active-tenant section already in
  its `AGENTS.md`, which is the same concern.
- **23 stubs repointed:** 17 unit-test and 6 E2E project `AGENTS.md` files now name their skill in one line
  instead of `@`-importing a file. The 12 integration stubs still import `INTEGRATION_CONVENTIONS.md`, which
  survives as the fixture/harness inventory.
- **`docs/INDEX.md` re-pointed** at the owning skill per topic, with a stated two-kinds-of-owner rule (a skill
  owns the rule, an in-repo file owns the inventory) and rule 7 rewritten around what *pulls a rule in* rather
  than the old 42-stub example that no longer exists.
- **~30 inbound references corrected** across service `ARCHITECTURE.md`s, four `TECH_DEBT.md`s, `docs/OVERVIEW.md`,
  `api/docs/MICROSERVICES_ARCHITECTURE.md`, `app/mobile/shared/AGENTS.md`, and the `review`, `e2e-ui-debug` and
  `e2e-api-debug` skills — each now pointing at whichever skill or file actually holds the rule.
- **Two live defects found by checking code before writing the rule down**, both inherited from the old docs:
  the B2B identity module is `@b2b/*`'s `features/tenant`, not the `features/identity` both docs claimed, and
  `B2bIdentity` *extends* `User` rather than wrapping it in the `{ user, memberships }` shape the old code
  sample showed. Also fixed: `e2e-ui-debug` and `e2e-api-debug` still told you to leave a one-off probe as an
  inline `logger.Log*` call, which `CA1848 = error` rejects at build — the same contradiction Phase 2 fixed in
  the doc but not in the two skills that read it first.
- **One rule deliberately kept in repo rather than cut:** "one repository per entity", which landed on `main`
  mid-phase and no skill owns. Deleting it would have removed a live rule on the strength of a skill that does
  not cover it.
- Verified: `docs_reachability.py` reports **0 errors** (21 warnings, all pre-existing `plans/` working docs);
  hook suite green at 72. Every type name written into the new B2B and app rosters was checked against code.

**Phase 3a — the corpus moved out as `.agents`-canonical skills** (`agent-standards` `ffe5721`,
`dotagents` `00e02f9`)

- 35 skills, split by whether the rule names this product: 28 generic in `dotagents` → `~/.agents/skills/`
  (C# style/naming, comments, DI, logging, validation, the three Result skills, persistence, multitenancy,
  keyed strategies, module structure, service boundaries, proto, HTTP contracts, seeding, the three test tiers,
  and the 8 TypeScript/React ones); 7 Concertable-shaped process ones in the org repo.
- **`.agents/skills/` is canonical in both**, with `.claude/skills/` and `plugins/*/skills/` as generated
  stubs from one generator — nothing is Claude-only, and Codex reads the same files.
- The stub generator mirrors each canonical `description` and fails rather than emit an unroutable stub. That
  guard caught a live defect: a bare colon-space truncates an unquoted YAML scalar, so `module-structure` and
  `typescript-style` had no description to route on and would never have loaded, with nothing visibly wrong.
- Two topics deliberately not migrated — "questions come before actions" and "act on reversible work" are
  always-applicable rules whose violation is silent, so a load-on-demand skill is the wrong tier. They stay
  global, as does the comment *policy*; only the C# mechanics became a skill.

**Phase 2 — correctness** (`e5df43bd4`)

- Ten contradictions between loaded docs reconciled, six settled by code or config rather than opinion.
- Root-relative `./e2e.ps1` / `./docker-health.ps1` corrected in root `AGENTS.md` (4) and the four e2e skills (39).
- `Notification` no longer documented as an adapter service a data service may `WaitFor` — only
  `Concertable.Shared.Notification`, a library, exists.
- Deleted `MM_NORTH_STAR.md` (423) and `MICROSERVICES_NORTH_STAR.md` (83); twelve dangling references fixed;
  five rotted citations stripped from `app/agents/CODE_PATTERNS.md`.

**Phase 1 — index, meta-rules, and the machine check**

- `docs/INDEX.md`; `docs_reachability.py` extended to error on a guidance doc linking a non-existent file or
  using a root-absolute path. Six tests added; suite green at 72.

## Reviews

`/review` run against #637 over `9205e82d..2b93b45b` → `reviews/Docs-GuidanceDocsRestructure.md`, security
layer included (the range touches `api/Concertable.Payment/**`, which the merge gate treats as sensitive;
those 12 lines are pointer rewiring, nothing to report). **10 findings, all fixed on the branch.** Four of
them were the corpus asserting something the code does not do, found by checking the code rather than the
prose:

- The rename to `MODULE_STRUCTURE.md` left five citations of the deleted `CONVENTIONS.md` — including a
  NetArchTest `.Because(...)` string, so a failing boundary test pointed the developer at a missing file.
- `api/ARCHITECTURE.md`'s new surface table gave B2B, Customer and Search a gRPC internal surface. Payment
  is the only one: one `.proto`, `AddGrpc`/`MapGrpcService` only in `Payment.Web`. The table came from the
  deleted `MICROSERVICE_COMMUNICATION.md`, where it was target-state — folding it into the doc `INDEX.md`
  calls "current-state, authoritative" turned a plan into a claim. Now marked target-vs-live per row.
- `INTEGRATION_CONVENTIONS.md` told tests to read `fixture.Catalog`, which no integration fixture exposes
  (`fixture.SeedState` / `fixture.SeedNow` are real; `Catalog` is on Customer's *E2E* `AppFixture`).
- `app/agents/CODE_CONVENTIONS.md` rostered a `$type` union named `Contract`, which does not exist in
  `app/` at all — the real second union is `Deal`, and a third is the search `Header` pair.

Also fixed: B2B's stance table put every concrete `DbContext` in `B2B.DataAccess.Infrastructure` (only the
bases are there); `INTEGRATION_CONVENTIONS.md` kept two `seeding`-skill rules under a
"Concertable-specific" heading; `api/AGENTS.md`'s inlined seed list still omitted invitation rows (the
drift the plan's table recorded); `E2E_UI_CONVENTIONS.md` + `E2E_CONSIDERATIONS.md` had zero inbound links
repo-wide and the hook can't see them (its orphan walk covers `*/agents/*.md` only) — now linked from the
harness `AGENTS.md` and `docs/INDEX.md`; `docs/INDEX.md` gave one topic two owners; `e2e-api-debug`
cited `api/docs/SEEDING_CONVENTIONS.md`.

Re-verified after the fixes: hooks **72 passed**, `docs_reachability.py` **0 errors / 21 warnings**.

## Next Steps

Paused: Tommy — run `! gh pr merge 637 --merge --auto` in his own session to enqueue #637.

0. **Re-review before that merge command works.** This checkpoint commits `plans/docs/*`, so the review
   watermark is genuinely stale now (it was only `reviews/`-only-diff stale before, which the gate
   whitelists). Run `/docs-review` over the branch, re-stamp `reviews/Docs-GuidanceDocsRestructure.md`,
   push, then the enqueue command above. The diff to review is plan/ledger prose only.

1. **Land this PR.** Routed to `/merge`, not `/merge-docs`: the diff carries one `.cs` file
   (`ModuleBoundaryTests.cs` — comment and `.Because(...)` strings repointed by the
   `CONVENTIONS.md` → `MODULE_STRUCTURE.md` rename), which `merge-docs` hard-refuses, so the queue's
   build gate applies. Review clean (0 open findings), branch 0 behind `origin/main`, local = remote =
   PR head, all 54 checks green, `skip-e2e` label correct (no positive trigger).
   **Enqueue is human-gated by a stale local gate hook, not by anything about this PR.**
   `merge-review-gate.py` runs from `CLAUDE_PROJECT_DIR` (the main checkout at
   `C:\Users\TommySeery\source\repos\Concertable`), whose `main` is **678 commits behind
   `origin/main`** and therefore still the 180-line copy that predates its own `review_only` fix. That
   old copy demands the review marker equal `HEAD`, which stamping can never satisfy — the 281-line
   version already on `origin/main` whitelists a marker-to-head range that touches `reviews/` alone,
   exactly this branch's case (`3893ea642..aa5759d5` = the review file only). Not fixed here: the main
   checkout has unrelated staged work (`.agents/hooks/plan_*.py`), so syncing it is Tommy's call.
   After the enqueue lands: poll for `MERGED`, then `./scripts/worktrees.ps1 close -Worktree
   C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-guidance-docs -PullRequest 637
   -PlanManaged`, then follow the generated `chore/platform-sync-*` PR to green (this branch's `api/**`
   markdown matches `publish-packages.yml`'s coarse `paths:`; non-breaking, so it should auto-merge).
2. **Phase 3c — the 10,011 lines of markdown outside the conventions folders.** Most is correctly-placed domain
   knowledge and stays untouched; six items need a disposition, listed in the plan's Phase 3c table.
   `app/README.md` is still the unmodified Vite scaffold, and `notes/Concert-Rust-Analysis.md` (444) is
   referenced by nothing.
3. **Phase 4 — collapse the remaining duplication rows to one home each.** Seeding is the big one: the
   `seeding` skill now owns the rule and `SEEDING_CONVENTIONS.md` the inventory, but `api/AGENTS.md:28–47`
   still restates 20 lines of it inline. Resolve under meta-rule 7 by deciding import-or-pointer — that
   summary exists precisely *because* `SEEDING_CONVENTIONS.md` is not `@`-imported. Same for
   `api/AGENTS.md`'s "shared code is the intersection" section, which `microservice-boundaries` now states
   generically.
4. **Make the 7 process skills concrete, and execute the settled merge ruling on their side.** They were
   written generic for a shared repo; the `merging` skill must lose the confirm-loop body and keep the rule,
   with the executable `.agents/skills/merge/SKILL.md` owning the procedure. Same for `pr-preflight`.
5. **Promotion candidates for the shared skills**, all found while cutting against them — none blocking:
   - `persistence` teaches a context-typed base (`Repository<TEntity, OrderDbContext, Guid>`), but
     Concertable's shared bases are capability-typed with no `TContext` parameter. The *rule* (module-local
     alias) is the same; the example predates the change.
   - "One repository per entity" has no skill home (see Done).
   - `e2e-scenarios` closes by pointing at "the `agent-process` standards", a name no skill has — the
     container-health rule lives in `remote-validation`.
   - **`E2E_UI_CONVENTIONS.md` is generic content still sitting in the repo** (Tommy flagged it). Page-object
     naming/shape, `data-testid` kebab-case and no-type-prefix, step bindings delegating with no Playwright
     calls, and API-not-UI setup for steps not under test are Reqnroll+Playwright rules with nothing
     Concertable in them — `e2e-scenarios` covers scenario authoring and stops short of all four. It survived
     Phase 3b unexamined because it was an *orphan*: the review fixed its zero inbound links by wiring it up,
     which is not the same as asking whether it belonged in the repo at all. Promote those four sections to
     `dotagents` (extend `e2e-scenarios` or add a sibling), leaving behind only the roster: `WorkflowState`,
     the Stripe-iframe selector exception, and the `AcceptApplicationPage`/`opportunity-add` examples.
   - **Cut the whole E2E doc footprint next, as one pass** (Tommy: "use this as an opportunity to cut all
     of this bloat"). None of it is stale — every identifier in `E2E_CONSIDERATIONS.md`
     (`CompleteChallengeIfRequiredAsync`, `4000002500003155`, `Requires3ds`, `checkout-awaiting`,
     `WaitUntilSavedAsync`) is still live in code — but almost none of it is a *convention*:
     - `E2E_CONSIDERATIONS.md` (37) → **delete**, redistributing all four sections. "Do not add timeouts"
       is already owned verbatim by `failing-tests` in `agent-standards`. "Tests must pass in isolation"
       has no skill home — promote one line to `e2e-scenarios`. The 16-line Stripe-card section names its
       own unfixed root cause ("provision a fresh Stripe test customer per run… until that is done,
       `CompleteChallengeIfRequiredAsync` is the pragmatic stopgap") — that is a `TECH_DEBT.md` entry, and
       the file already sits next to one. "`checkout-awaiting` timing out" is a debug symptom → the
       `e2e-ui-debug` symptom table.
     - `E2E_UI_CONVENTIONS.md` (26) → ~5, per the promotion above.
     - The four `.agents/skills/e2e-*` runbooks (711) each restate the Docker-health rule twice; that rule
       is owned by `remote-validation`. Replace with a pointer.
     Not on this branch: #637 is enqueue-ready and any further push re-stales its review; the cut also
     rewrites `Concertable.Testing.E2E/AGENTS.md`, which #637 already edits. Do it in the continuation
     worktree once #637 lands.
6. **Deferred to its own PR:** auto-load thinning of root `AGENTS.md` (the 86 merge lines and 32 Docker lines
   that `/merge` and `scripts/e2e.ps1` already automate), the analyzer push-down plus
   `EnforceCodeStyleInBuild`.

## Also Tommy's, not blocking

`tomjseery/agent-starter-kit` (public, 7 skills) looks redundant with `tomjseery/dotagents` (private, the
same 7 plus `pull-main`, `sync-all`, `unmerged`) — the same duplication disease at repo level.
