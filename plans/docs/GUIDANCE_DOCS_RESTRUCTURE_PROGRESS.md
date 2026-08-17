# Guidance-docs restructure — progress

- Plan: `plans/docs/GUIDANCE_DOCS_RESTRUCTURE_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/guidance-restructure`
- Also delivered by this ledger: roadmap item `docs/agent-standards`, now checked off
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-guidance-docs`
- Branch: `Docs/GuidanceDocsRestructure`
- PR: #637 — ready, label `skip-e2e`, **current with `origin/main`** (0 behind as of the Phase 6 tier-3
  commit). Updated for base currency three times on 2026-08-17: from **69 behind** and `DIRTY` (three doc
  conflicts resolved, below), from 2 behind after platform-sync #645 merged — a clean merge carrying only
  the `<ConcertablePlatformVersion>` bump `0.1.0-alpha.0.1055` → `0.1.0-alpha.0.1061` across the five
  service `Directory.Packages.props` — and from 10 behind at `2b04d57e2`.
- Shared repos: `Concertable/agent-standards` (7 process skills + the `skill_router` hook, `88cf091`, pushed) and `tomjseery/dotagents` (29 generic — 20 .NET + 9 TS/React — synced to `~/.agents/skills/`), cloned at `C:\Users\TommySeery\source\repos\{agent-standards,dotagents}`
- Dependency/package gates: no consumer migration to do, but this PR **will** trigger publish + platform sync — `publish-packages.yml` triggers on the coarse `paths: api/**`, which this branch's `api/**` markdown matches. MinVer republishes and a `chore/platform-sync-*` PR opens; non-breaking (no published type changed), so it should auto-merge green. Follow it to green anyway — whoever merges owns the sync.
- Last reconciled: 2026-08-17 against `agent-standards` `88cf091` (pushed) after the incremental review's three fixes; `origin/main` matched at the tier-3 commit (0 behind) — re-check at enqueue time

**Scope changed 2026-08-17: this is no longer a docs PR.** It now carries build behaviour
(`api/TestConventions.targets` gating every test project) and a PreToolUse hook, because Phase 6 must land
with the thinning rather than after it. `skip-e2e` is still correct — no Step 4 positive trigger: no UI
flow, no HTTP/gRPC contract, no published-package shape, no auth/routing change. But PR CI now matters far
more than it did, since the new targets file participates in every project's build.

## Current state

The reduction has happened. Every generic rule now has exactly one home — a skill — and the in-repo docs hold
only this system's roster of real types, contexts, clients, tables and pins. The corpus that auto-loads on an
`api/**` prompt went from **1,429 lines to 246**, and on an `app/**` prompt from **786 to 151**; a unit-test
project no longer pulls in 80 lines and an E2E project no longer pulls in 37.

Enforcement is now complete at all three tiers, **and in both harnesses** — the incremental review found the
router had been enforcing in Claude only, so a Codex session wrote past it silently. The build fails a
misnamed or misclassified test project, the write-time router blocks the first write into a routed path
whichever harness makes it, every test project's stub opens with the unit-vs-integration decision, and
`/review` resolves the standards it owes from the same table. What remains
is Phase 6a's deployment (Tommy's to run), Phase 3c (the markdown outside the conventions folders), Phase 4
(the duplication rows that still have >1 home, chiefly seeding across `api/AGENTS.md` and
`SEEDING_CONVENTIONS.md`), and the deferred auto-load thinning of root `AGENTS.md`.

## Done

**Phase 6 tier 3 — the stub states the tier, the gate makes it unskippable, and the review reads the
same table** (`agent-standards` `262564f`, this PR)

- **The route table now answers after the fact, not only at write time.** `skill_router.py --skills-for
  <paths>` (paths as args or on stdin, `--json` for a caller) resolves changed files through the same
  matcher the hook blocks with, prints the skills those files oblige a reader to load, and reports any
  deny pattern already sitting in the tree. Path/content matching and the deny scan moved into shared
  functions, so a review can never resolve a path differently from the block that fired on it. Authored
  in `agent-standards` and re-vendored here (hash check green); its tests there went 13 → 20.
- **`review` Step 2 runs it.** The hand-maintained list of 24 skill names is gone — a list in a review
  skill is the same discretion the incident exposed, one indirection later. `incremental-review` and
  `big-review` inherit it. A `DENY PATTERN HIT` line is stated to be a confirmed finding, not a hint.
  **The table is narrower than that list was**, so four rows were added where a miss is expensive and
  silent — `*Seeder.cs` → `seeding`, `*Validator(s).cs` → `validation`, `*Module.cs` → `module-structure`,
  `*AppHost*/Program.cs` → `microservice-boundaries` (28/29/20/6 files) — and Step 2 states the table is
  the floor, not the ceiling: a skill you had to *remember* is a missing row, not a paragraph to re-add.
- **Every test project now states its tier at the point of use, and `docs_reachability.py` requires it.**
  17 test projects had no `AGENTS.md` at all — including `Concertable.B2B.ArchitectureTests`, both Auth
  suites, Payment's integration suite and five `Concertable.Shared` unit suites — which is the hole that
  made the `@`-import argument moot in the incident: there was nowhere for a pointer to live at the moment
  the tier was being chosen. All 17 pairs created; the gate errors on any `IsTestProject == true`
  directory without one (the existing sibling rule supplies the `CLAUDE.md` half, so it is stated once).
- **Every unit and integration stub now leads with the decision, not the pointer** — "a test that needs a
  host, HTTP, a container or a database belongs in `*.IntegrationTests`", and its converse. The four
  `*.IntegrationTests.Fixtures` stubs got the honest version instead: a support library declaring
  `<IsTestProject>false</IsTestProject>`, which the earlier sweep had wrongly labelled a suite.
- Verified: hook suite **91 passed** (was 88), `agent-standards` **20 passed**, `sync-generated -Check`
  and `vendor-hooks -Check` clean, `docs_reachability.py` **0 errors / 23 warnings** (all `plans/`).
- **Gap found, not closed here:** no Concertable workflow runs the Python hook tests, so the vendored-hook
  drift test and the route-table test only run when someone runs them locally. Adding a workflow would put
  `.github/workflows/**` in the diff, which the merge gate treats as security-sensitive and would re-stale
  #637's markers — so it is a follow-up, listed below rather than smuggled in.

**Phase 6b — one authored hook, two delivery routes, both harnesses wired** (`agent-standards` `df35cea`,
this PR)

Tommy's ruling when the install question surfaced: the plugin stays the single author, and each consuming
repo carries a generated, drift-checked copy plus its own wiring — because a plugin only runs where it was
installed, and enforcement absent on a fresh clone is not enforcement.

- **`agent-standards` restructured.** `.agents/hooks/skill_router.py` (+ 13 mechanism tests over a fixture
  route table, so a route added downstream can't change what the mechanism is asserted to do),
  `.agents/plugins/marketplace.json` as the canonical manifest, `plugins/agent-process/hooks/hooks.json`,
  and `.agents/sync-generated.ps1` replacing the stub script — it now emits the repo-local stubs, the
  plugin payload, the hook copy and the `.claude-plugin/marketplace.json` shim, with a `-Check` mode. The
  repo ships code now, so it got CI (pytest + staleness check) and a `.gitignore`.
- **A latent defect fixed in passing:** the plugin's skill files were stubs pointing at
  `../../../../.agents/skills/…`, and a harness installs only the plugin subtree — so the plugin would
  have installed cleanly and delivered nothing, on every machine. Payload now carries full copies.
- **Concertable keeps the router, as a generated copy.** `.agents/vendor-hooks.ps1 -Into <repo>` wrote it
  plus `.agents/hooks/vendored.json` (source, commit, sha256); `test_vendored_hooks.py` fails if the copy
  is edited in place or wired for only one harness. `.agents/README.md` now separates repo-owned hooks
  from vendored ones, and `docs/INDEX.md` gained the three missing enforcement rows (build tier gate,
  skill router, vendored-hook check).
- **The real Claude-only defect closed:** `.codex/hooks.json` now carries the router alongside its Stop
  hook, using the same `command`/`commandWindows` pair. Codex reads project hooks from
  `<repo>/.codex/hooks.json` and trusts them **by hash per hook per path** — verified from
  `~/.codex/config.toml`'s `[hooks.state.'…:stop:0:0']` entry — so the new PreToolUse entry needs a
  one-time approval in Codex, and each worktree is trusted separately from the main checkout.
- Hook suite **88 passed** (was 84); `agent-standards` **13 passed**, generator `-Check` and
  `vendor-hooks -Check` both clean.
- **Found while probing, not yet fixed (Phase 6a's problem, not this PR's):** a Codex session logs
  `failed to load skill ~/.agents/skills/sync/SKILL.md: missing YAML frontmatter` and the same for
  `worktree` — two installed skills are dead in every Codex session — and Codex warns that skill
  descriptions were shortened to fit its skills budget.

**`origin/main` merge — three doc conflicts, resolved keeping both sides' intent** (this PR)

- `api/Concertable.Payment/ARCHITECTURE.md` — kept the `keyed-strategies` skill pointer *and* main's new
  `PROVIDER_CONTRACT.md` line.
- `api/TECH_DEBT.md` — took main's sharper `extension()` resolves-when (whole-container migration,
  `XExtensions`/`XMappers` grouping, generator declarations excluded) with the citation repointed from the
  deleted `agents/CODE_CONVENTIONS.md` section to the `csharp-style` skill.
- `api/agents/CODE_CONVENTIONS.md` — kept the reduced file, but main had added a rule the cut never saw:
  **integration events version the wire identity, not the CLR type**
  (`concertable.payment.payment-operation-state-changed.v1`, verified against `[MessageType]` in code).
  No skill owns event wire versioning, so deleting it would have dropped a live rule; kept in repo and
  listed under promotion candidates. `docs/INDEX.md` updated to name the new topic.
- Main's other two edits to that file needed no repo home: the `EscrowMappers` example moving to
  `extension()` blocks, and the strengthened extension-members section — both generic, both now
  promotion candidates against `csharp-style`/`csharp-naming`.

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

- 36 skills, split by whether the rule names this product: 29 generic in `dotagents` → `~/.agents/skills/`
  (C# style/naming, comments, DI, logging, validation, the three Result skills, persistence, multitenancy,
  keyed strategies, module structure, service boundaries, proto, HTTP contracts, seeding, the three test tiers,
  and the 9 TypeScript/React ones — 29 by a later enumeration of `dotagents/.agents/skills`, which the
  original 28/8 counts undercounted); 7 Concertable-shaped process ones in the org repo.
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

Both markers have since moved forward twice more with nothing re-reviewable in between — the two
base-currency merges of `origin/main`, whose only content outside `reviews/` is the platform pin bump
already reviewed and merged on `main` as #645. Review state on that range: **clean, 0 open findings.**

**That review has now run** (`c8302694..e29cd957`, 21 commits, 101 files). Note the range: the marker
already sat at `c8302694`, not the `2b93b45b` this ledger previously named — the marker is the contract
between runs, and the two moves after `2b93b45b` were over merges already reviewed on `main`. **Three
findings, all fixed on the branch**, so the markers are stamped at the fix head rather than `e29cd957`:

- **ENF1 (HIGH)** — the router's Codex leg was inert. `.codex/hooks.json` matched `apply_patch`, but the
  router only knew Claude's PascalCase names and its `file_path` key, so every Codex write exited 0. The
  drift test could not see it: it asserts the hook's *filename* appears in both wiring files, which was
  true throughout. Fixed upstream (`agent-standards` `268796e`, payload `88cf091`) and re-vendored;
  `test_every_wired_tool_name_is_one_the_hook_acts_on` now makes the matcher and the hook's own
  vocabulary agree. **This retires Next Step 2's premise** — approving the old hook in Codex would have
  approved an inert one.
- **ENF2 (MEDIUM)** — the unit-tier source list covered 2 of the 6 host families the PackageReference
  check names, so a unit project reaching Respawn/Playwright/Testcontainers/Aspire *transitively* (the
  live shape: `Concertable.Payment.E2ETests.Helpers.UnitTests` → `…E2ETests.Helpers`) passed both gates.
  Six fingerprints added and proved end-to-end with a build probe.
- **ENF3 (LOW)** — 53 lines of one comment pasted at nine import sites, self-contradictory in
  `api/Directory.Build.targets`. Deleted.

## Next Steps

**#637 is no longer merge-ready, and not because of its own content.** Phase 6 (added to the plan
2026-08-17) must ride the same PR: thinning the in-repo corpus to skill pointers must not land before
the mechanism that makes those skills fire. A live failure proved triage is not a guarantee — an agent
created `Concertable.ServiceDefaults.Tests`, booted a `WebApplication` inside a "unit" test, used the
wrong assertion library and wrote no sibling `AGENTS.md`, with `unit-testing` and `integration-testing`
both installed, described and listed. Neither fired; the follow-up `/review` repeated the blind spot and
returned clean. Merging the thinning before the enforcement opens exactly that window.

**All three Phase 6 tiers and Phase 6b are in** — the build gate (`f99fa8c2f`:
`api/TestConventions.targets` + `BannedSymbols.UnitTests.txt`, imported from all 9
`Directory.Build.targets`, zero migration, tier resolution and both gates verified), the router (authored
in `agent-standards`, vendored here with a hash check, wired for **both** harnesses), and tier 3 above.
**What remains of Phase 6 is deployment, and it needs Tommy.**

1. **Phase 6a — skill deployment.** `agent-standards`' 7 process skills load in **no session today**
   (`installed_plugins.json` has only stripe/clangd/rust-analyzer; `known_marketplaces.json` only
   `claude-plugins-official`), so Phase 3a delivered 29 skills, not 36. `dotagents/.agents/deploy-skills.ps1`
   (`dotagents` `71de4b5`) junctions canonical → `~/.agents/skills`; `-WhatIf` is clean at 46 skills, zero
   refusals. **Not yet run — it needs Tommy, because deleting 36 directories under `~/.claude`/`~/.agents`
   trips the permission classifier:**
   `! & "$env:USERPROFILE\source\repos\dotagents\.agents\deploy-skills.ps1" -Confirm:$false`
   Phase 6b does not remove this: vendoring covers the *hook*, not the skills, so the `~/.agents` leg is
   still what makes a skill loadable at all. Two extra faults for that run to fix, found by watching a real
   Codex session: `~/.agents/skills/sync/SKILL.md` and `worktree/SKILL.md` have no YAML frontmatter, so
   Codex refuses both — meaning `/sync` and `/worktree` are dead there while looking installed. Already
   done: the two installed-only skill edits that copy-deployment had stranded are recovered into `dotagents`
   `c153697` (`prune-worktrees` +34, `worktree` +17 — a junction redeploy without checking direction first
   would have destroyed both). Installed and canonical now agree on all 36; 3 canonical skills
   (`last-conversation`, `recents`, `search`) remain uninstalled.
2. **Approve the Codex hook once** (Tommy, ~5 seconds, in a Codex session in this repo). Codex trusts a
   project hook by hash per file per path, so the `PreToolUse` entry in `.codex/hooks.json` is inert
   until approved, and the worktree is trusted separately from the main checkout. **Do this after the
   ENF1 fix, not before** — approving the previous hook would have approved one that allowed every Codex
   write. Nothing else waits on it.
3. **Then Tommy's own read of the PR**, then merge. Paused: Tommy — his sign-off is required and the clean
   automated `/review` is not it; resume on his go-ahead once the above is in.
4. **Land this PR** once the above is in. Routed to `/merge`, not `/merge-docs`: the diff carries one `.cs` file
   (`ModuleBoundaryTests.cs` — comment and `.Because(...)` strings repointed by the
   `CONVENTIONS.md` → `MODULE_STRUCTURE.md` rename), which `merge-docs` hard-refuses, so the queue's
   build gate applies. Review clean (0 open findings), local = remote = PR head, `skip-e2e` label correct
   (no positive trigger — ~110 markdown files, three Python hook files, agent wiring JSON, one `.cs`
   comment/string change; no UI flow, HTTP/gRPC contract, published shape or auth behaviour). **Branch is
   current with `origin/main` as of this entry — re-check `git rev-list --count HEAD..origin/main` at
   enqueue time and update if it has moved.**
   The gate hook is satisfied: its `review_only` whitelist covers a marker-to-head range touching
   `reviews/` alone, which is this branch's exact shape once the markers sit at the ledger commit.
   Enqueue with `gh pr merge 637 --merge --auto` once the exact-head checks are green, poll for
   `MERGED`, then `./scripts/worktrees.ps1 close -Worktree
   C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-guidance-docs -PullRequest 637
   -PlanManaged`, then follow the generated `chore/platform-sync-*` PR to green (this branch's `api/**`
   markdown matches `publish-packages.yml`'s coarse `paths:`; non-breaking, so it should auto-merge).
5. **Phase 3c — the 10,011 lines of markdown outside the conventions folders.** Most is correctly-placed domain
   knowledge and stays untouched; six items need a disposition, listed in the plan's Phase 3c table.
   `app/README.md` is still the unmodified Vite scaffold, and `notes/Concert-Rust-Analysis.md` (444) is
   referenced by nothing.
6. **Phase 4 — collapse the remaining duplication rows to one home each.** Seeding is the big one: the
   `seeding` skill now owns the rule and `SEEDING_CONVENTIONS.md` the inventory, but `api/AGENTS.md:28–47`
   still restates 20 lines of it inline. Resolve under meta-rule 7 by deciding import-or-pointer — that
   summary exists precisely *because* `SEEDING_CONVENTIONS.md` is not `@`-imported. Same for
   `api/AGENTS.md`'s "shared code is the intersection" section, which `microservice-boundaries` now states
   generically.
7. **Make the 7 process skills concrete, and execute the settled merge ruling on their side.** They were
   written generic for a shared repo; the `merging` skill must lose the confirm-loop body and keep the rule,
   with the executable `.agents/skills/merge/SKILL.md` owning the procedure. Same for `pr-preflight`.
8. **Promotion candidates for the shared skills**, all found while cutting against them — none blocking:
   - **FIXED (`dotagents` `daef94d`)** — `persistence` taught that an awaiting page projection "still
     constructs its page by hand", the practice `main` refactored away from in `OpportunityMapper`. The
     10-commit merge in this turn surfaced it: the owning home was teaching the superseded rule while the
     repo doc it took over from had been tightened. Canonical and both installed copies now agree. Related
     observation for Phase 6a: `~/.claude/skills/*` holds 10-line stubs pointing at `~/.agents/skills/*`,
     so a junction deployment must target the `.agents` leg and leave the stubs, not replace them.
   - `persistence` teaches a context-typed base (`Repository<TEntity, OrderDbContext, Guid>`), but
     Concertable's shared bases are capability-typed with no `TContext` parameter. The *rule* (module-local
     alias) is the same; the example predates the change.
   - "One repository per entity" has no skill home (see Done).
   - `e2e-scenarios` closes by pointing at "the `agent-process` standards", a name no skill has — the
     container-health rule lives in `remote-validation`.
   - **Three surfaced by the `origin/main` merge**, all against the C# skills: `csharp-style`'s
     `extension()` section is weaker than the repo's current rule — it lacks "migrate every ordinary
     member of a container you touch, so a class never mixes forms" and the signature-bound
     `[LoggerMessage]` exception; `csharp-naming`'s `XMappers` example still shows the legacy
     `public static … (this X x)` form the same corpus now bans, so the two skills contradict each
     other; and **integration-event wire versioning has no skill at all** — kept in
     `api/agents/CODE_CONVENTIONS.md` for now, and the natural home is `microservice-boundaries`
     (events) or `proto` (wire identity), neither of which mentions `MessageType` today.
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
9. **Deferred to its own PR:** auto-load thinning of root `AGENTS.md` (the 86 merge lines and 32 Docker lines
   that `/merge` and `scripts/e2e.ps1` already automate), the analyzer push-down plus
   `EnforceCodeStyleInBuild`, and **a CI job running the Python hook tests** — nothing in
   `.github/workflows/` runs them today, so every hook gate here is only as live as the last person who ran
   it locally (`agent-standards` already has this; Concertable does not). Its own PR because touching
   `.github/workflows/**` pulls the merge gate's security-marker requirement in with it.

## Also Tommy's, not blocking

`tomjseery/agent-starter-kit` (public, 7 skills) looks redundant with `tomjseery/dotagents` (private, the
same 7 plus `pull-main`, `sync-all`, `unmerged`) — the same duplication disease at repo level.
