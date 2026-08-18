# Guidance-docs restructure — progress

- Plan: `plans/docs/GUIDANCE_DOCS_RESTRUCTURE_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/guidance-restructure`
- Also delivered by this ledger: roadmap item `docs/agent-standards`, now checked off
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-guidance-docs`
- Branch: `Docs/GuidanceDocsRestructure`
- PR: #637 — **open working branch, NOT for merge** (see the standing constraint in `## Next Steps`; it deletes 2,662 lines whose replacement is junction-only today). Label `skip-e2e`, **current with `origin/main`** (0 behind as of the Phase 6 tier-3
  commit). Updated for base currency three times on 2026-08-17: from **69 behind** and `DIRTY` (three doc
  conflicts resolved, below), from 2 behind after platform-sync #645 merged — a clean merge carrying only
  the `<ConcertablePlatformVersion>` bump `0.1.0-alpha.0.1055` → `0.1.0-alpha.0.1061` across the five
  service `Directory.Packages.props` — and from 10 behind at `2b04d57e2`.
- Shared repos **today** (target shape in `## Topology` below): `Concertable/agent-standards` — `standards/process/` (7 docs) + the `skill_router` hook, shipping the `agent-process` plugin, on `Refactor/StandardsDomainTree`, **PR #2 open, CI green, unmerged**; and `tomjseery/dotagents` — `standards/{dotnet,react,communication}/` (31 docs) + 10 self-contained utilities, shipping `dotnet-standards`/`react-standards`/`communication-standards`, on `Refactor/StandardsDomainTree`, **PR #1 open, unmerged**. `main` in both still carries the pre-inversion flat layout with no plugins at all, which is what the 48 live junctions point at and why no `marketplace add` can find anything yet. Plus `agent-utilities` (session tooling, no skills) and `agent-starter-kit` (to archive), cloned at `C:\Users\TommySeery\source\repos\{agent-standards,dotagents}`. Delivery architecture: `dotagents/ARCHITECTURE.md`
- Dependency/package gates: no consumer migration to do, but this PR **will** trigger publish + platform sync — `publish-packages.yml` triggers on the coarse `paths: api/**`, which this branch's `api/**` markdown matches. MinVer republishes and a `chore/platform-sync-*` PR opens; non-breaking (no published type changed), so it should auto-merge green. Follow it to green anyway — whoever merges owns the sync.
- Last reconciled: 2026-08-18 — fetched at the doc-truth commit `4bbb2ddb0`: **0 behind `origin/main`**, 49 ahead, local head = `origin/Docs/GuidanceDocsRestructure` = PR #637 head, `mergeStateStatus` `CLEAN`, auto-merge off, label `skip-e2e`. `agent-standards` unchanged at `88cf091` (pushed) — re-check currency at enqueue time

**Scope changed 2026-08-17: this is no longer a docs PR.** It now carries build behaviour
(`api/TestConventions.targets` gating every test project) and a PreToolUse hook, because Phase 6 must land
with the thinning rather than after it. `skip-e2e` is still correct — no Step 4 positive trigger: no UI
flow, no HTTP/gRPC contract, no published-package shape, no auth/routing change. But PR CI now matters far
more than it did, since the new targets file participates in every project's build.

## Current state

**#637 is not deliverable and that is the headline.** Everything below describes a branch that is
internally coherent and externally unsupported: the reduction happened, but what it reduced *to* is
reachable only through machine-local junctions. Read the standing constraint in `## Next Steps` before acting
on anything here — the next work is Phase 7, not merging.

The reduction has happened. Every generic rule now has exactly one home — a skill — and the in-repo docs hold
only this system's roster of real types, contexts, clients, tables and pins. The corpus that auto-loads on an
`api/**` prompt went from **1,429 lines to 246**, and on an `app/**` prompt from **786 to 151**; a unit-test
project no longer pulls in 80 lines and an E2E project no longer pulls in 37.

Enforcement is now complete at all three tiers, **and in both harnesses** — the incremental review found the
router had been enforcing in Claude only, so a Codex session wrote past it silently. The build fails a
misnamed or misclassified test project, the write-time router blocks the first write into a routed path
whichever harness makes it, every test project's stub opens with the unit-vs-integration decision, and
`/review` resolves the standards it owes from the same table. Phase 6a is deployed — 46 junctions across
both roots.

**Phase 5 is now done in both shared repos** (`Concertable/agent-standards#2` — CI green;
`tomjseery/dotagents#1`). The corpus is organized by domain and the payload is out of the `SKILL.md`
files: 38 standards are now docs under `standards/<domain>/`, each routed to by an eight-line skill, with
a generated `INDEX.md` per domain and a build gate that refuses a router pointing at a missing doc, a doc
with no router, or two routers claiming one doc. **Neither PR is merged and nothing is deployed** — the
deployment junctions would point at trees that exist only on those branches.

**Phase 7 is BUILT on the same two PRs**: four plugins now carry the corpus (`agent-process` 7 docs +
the hook, `dotnet-standards` 20, `react-standards` 9, `communication-standards` 2), assigned by domain
from an authored map, and `dotagents/ARCHITECTURE.md` is the durable home for the delivery
architecture. Building it exposed a fourth blocking defect: the hook resolved skills only in the
junction roots, so it would have reported every plugin-installed skill as `NOT INSTALLED - a deployment
fault` in the very mode Phase 7 makes primary.

**The #637 gate is still NOT satisfied.** Installable was built, not proven: the plugins live on two
unmerged branches, so a `marketplace add` against either `main` finds none. What closes it is merging
both PRs and verifying one real install per harness - and that verification mutates machine config, so
it is Tommy's to run or authorize.

**What remains after that:** `api/agents/` is still a destination the polyrepo cut removes (5b), the
discovery pass has not run (5c, with `dotnet/STACK.md` its first known gap), Phase 3c (markdown outside
the conventions folders), Phase 4 (rows still with >1 home, chiefly seeding across `api/AGENTS.md` and
`SEEDING_CONVENTIONS.md`), and the deferred auto-load thinning of root `AGENTS.md`.

## Done

**Phase 7 — the corpus ships as plugins, and the architecture stops being rediscovered** (same two PRs)

- **A fourth blocking defect, found by building it.** `skill_description` searched only `~/.agents/skills`
  and `~/.claude/skills`. A plugin copies its payload into
  `<harness>/plugins/cache/<marketplace>/<plugin>/<version>/skills/`, so under plugin delivery the router
  would block a write, name the owning skill, and call that correctly-installed standard "a deployment
  fault". Now searches `CLAUDE_PLUGIN_ROOT` and its own plugin subtree first, then the junction roots, then
  every other installed plugin under both `~/.claude` and `~/.codex`. An uninstall leaves `.orphaned_at`
  behind - one was found on this machine - so orphaned caches are skipped rather than read as available.
  All three resolution tests confirmed to fail without the fix; hook tests 32 → 36.
- **Payloads assigned by domain** from an authored `.agents/plugins/payloads.json`, cross-checked both ways
  against `marketplace.json` - the "explicit skill-to-plugin map" the generator previously refused to
  proceed without. A plugin gets only its own domains, so a TypeScript project installing `react-standards`
  does not also receive the .NET corpus. A domain no plugin ships is an error. All guards negative-tested,
  and all 31 plugin routers verified to resolve to a real doc inside their own plugin.
- **Pruning is by generated-set membership**, not skill name: a doc moving between plugins otherwise leaves
  a stale copy, and a consumer installs two conflicting copies of one rule. Proven by moving a domain and
  moving it back.
- **`dotagents` gained CI** - it had none, so its generated files were only as current as the last local
  run. The check also proves the generator runs on Linux/PowerShell 7 while staying 5.1-compatible.
- **`dotagents/ARCHITECTURE.md`** carries the repo map with its explicit do-not-merge, the
  authoring → generate → install chain (a plugin copies, it does not reference), per-machine setup for both
  harnesses, and what a new project needs. Linked from both READMEs. This plan is deleted when it
  completes, so none of it may live only here.
- **Settled: the 10 utilities ship in no plugin.** They are procedures for working the machine, not
  standards a project consults, and `dotagents` stays a clone-and-junction repo for `~/AGENTS.md` and
  `~/.claude/` regardless. A fifth `personal-utilities` plugin is one map entry away if ever wanted.
- **Generator duplication assessed and consciously kept.** The two scripts are parallel but not identical,
  and `agent-standards` CI must self-verify without reaching a private repo, which rules out one shared copy
  short of a published module. Revisit only if a third repo appears.

**Phase 5 — the doc is the payload, the skill is the router** (`Concertable/agent-standards#2`, CI green;
`tomjseery/dotagents#1`. Neither merged, nothing deployed.)

- **38 skill bodies became docs in a domain tree.** `agent-standards` gained `standards/process/` (7);
  `dotagents` gained `standards/dotnet/` (20, nesting `data/`, `results/`, `structure/`, `testing/`),
  `standards/react/` (9) and `standards/communication/` (2). Every skill that owns a doc is now eight
  lines: front matter plus that doc's path. Doc names shed what the folder already says
  (`csharp-style` → `dotnet/STYLE.md`) while skill names stay globally unique, because the deployed
  namespace is flat and spans every stack.
- **The repos did NOT merge, and the plan's Phase 5 text saying they would is corrected.** Phase 7's repo
  map is the authority: `dotagents` is personal cross-project machine config, so scoping it to one product
  would break every other codebase that depends on it. The same `standards/<domain>/` convention is applied
  twice instead, with domain names globally unique so both halves land in one deployed namespace.
- **A utility is not a standard.** The 10 machine-tooling skills (`sync`, `worktree`, `recents`, …) are
  procedures the agent runs, not a corpus anyone consults, so their bodies stay in their `SKILL.md` and they
  own no doc. Both generators understand the two kinds.
- **The orphan gate is proven, not nominal.** A router naming a missing doc, a doc with no router, and two
  routers claiming one doc were each negative-tested to confirm they fail. A gate nobody proved fires is the
  same defect class as the `hooks.json` matcher that shipped inert for every Codex write.
- **Generated per-domain `INDEX.md`** replaces `dotagents/README.md`'s hand-maintained topic table — a second
  structure with nothing holding it to the first. `AGENTS.md`'s pointer now names the tree.
- **The plugin gets its own copy of the tree.** An installed plugin is only its own subtree, so plugin
  routers carry a path relative to their own `SKILL.md`. `deploy-skills.ps1` junctions each
  `standards/<domain>` into `~/.agents/standards`, refusing a domain declared by two repos exactly as it
  refuses a duplicate skill name — without that, a deployed router points at a file the session cannot open.
- **Two pre-existing `dotagents` defects fixed:** `draft-comment`'s description carried a colon-space, which
  truncates an unquoted YAML scalar — the repo's own guard existed for it but had never run since the skill
  was added; and `draft-comment` + `explaining-code` had no `.claude/skills` stub at all (41 canonical, 39
  stubs), so both were invisible to a session opened on that repo.
- Path handling deliberately avoids `[Path]::GetRelativePath` and `Resolve-Path -RelativeBasePath` (both
  PowerShell 7 only): these repos are cloned onto 5.1 machines, while CI runs the same script on Linux.

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

**The doc-truth probe that ran alongside those three is now closed too** (`4bbb2ddb0`). It carried four
open items into Next Step 5; checking each against code rather than the prose split them evenly. Two were
real — `CODE_PATTERNS.md`'s repository counts (Concert 6/6 and Conversations 2/2 against an actual 9/13
and 3/4) and its Refit inventory missing `ICustomerUserClaimsApi` — and are fixed. Two were correct as
written and needed only the extraction nobody had done: the `TenantPermission` ↔ `SharedPermissions`
mirror diffs identical at 13, and `customerClient` is where the doc says. The counts finding also killed a
precedent rather than a number — the doc cited Conversations as proof of a rule `MessageRepository`
breaks.

## Topology — settled 2026-08-18

Phase 3a split the corpus on **portability**, which is why React sat beside .NET and machine utilities in
one repo. Settled shape, by domain instead:

| Repo | Holds |
|---|---|
| `standards` (rename of `Concertable/agent-standards`) | `dotnet/`, `react/`, `process/`, `concertable/`, later `infra/` — docs tree + flat routing skills |
| `agent-utilities` | machine tooling (`sync`, `worktree`, `recents`, `search`, `unmerged`, …) |
| service repos | that service's own rosters |
| `dotagents` | **stays its own repo — corrected 2026-08-18.** It is Tommy's *personal machine config*: its README mirrors `%USERPROFILE%` (`~/AGENTS.md`, `~/.agents/`, `~/.claude/`), synced across machines, and its general engineering standards apply to **every** codebase he owns, not to Concertable. Folding it into a Concertable org repo would scope personal, cross-project standards to one product. An earlier revision of this table said it collapses into `standards/dotnet/` + `standards/react/`; that was wrong. |
| `agent-starter-kit` | archived — strict subset of `dotagents`, two skills BOM-broken |

**`api/agents/` is deleted, not thinned.** POLYREPO_ROADMAP §6 settled the same day as a true one-way cut,
so there is no `api/` node to host it. Each rule re-homes by one test: names no product → `standards/dotnet`
or `standards/react`; names a platform type every service uses → `standards/concertable`; names one
service's type → that service's repo.

**Refined 2026-08-18 — a service repo carries no skills at all (Phase 7).** The table above still had
each repo holding *something*. It should hold no skills: not `.claude/skills/` stubs, not
`.agents/skills/`, not a vendored hook. Skills become four plugins out of `standards` (`agent-process`,
`dotnet-standards`, `react-standards`, `concertable-delivery`), installed once per machine at user scope;
a repo keeps only its `AGENTS.md` roster, its `skill-routes.json`, and the two per-harness wiring files.
Phase 6b had rejected this because a repo-declared plugin does not auto-install — correct, and re-verified
today — but it weighed that ritual against one repo. `--scope user` is machine-wide, so the cost is one
command **per machine**, not per repo, and does not grow with the service count. Vendoring is retired in
favour of a hook that fails loudly when the plugin is missing, which buys the same no-silent-unenforcement
property without a code copy in every repo. Two gating unknowns are recorded in the plan's Phase 7: Codex's
install semantics, and whether a plugin-shipped `hooks.json` fires without repo wiring.

Also added: **Phase 5c, the discovery pass.** The earlier phases only relocate rules that already exist as
prose. Conventions that live solely in code — B2B's stance taxonomy (`TenantScopedDbContext` tenant-scoped
read/write, `ReadDbContext` unfiltered reads, `AdminDbContext` everything, plus the filtered-entity list) is
the example Tommy had to state verbally — are not found by relocation. Each domain node is only done once its
inventory has been checked against code.

## Next Steps

**Next work is to LAND and PROVE the delivery, not to write more of it.** Phases 5 and 7 are both built on
`Concertable/agent-standards#2` and `tomjseery/dotagents#1`: the corpus is organized by domain and ships as
four plugins with a durable architecture doc. What is missing is not code - it is that a plugin
`marketplace add` against either repo's `main` still finds nothing, because both PRs are open. Until an
install is proven, the 2,662 lines Phase 3b removed from this repo are still held only by 48 junctions on
one machine, and #637 must not merge.

Three steps, in order — **prove the install BEFORE merging, not after.** An earlier revision of this
ledger had these reversed, which would have merged an unproven payload into two `main` branches. Neither
harness forces that: `claude plugin marketplace add` takes "a URL, path, or GitHub repo" and
`codex plugin marketplace add` takes "a local path, `owner/repo[@ref]`, HTTPS Git URL" plus a `--ref`
flag. So the branch, or the working tree itself, is directly installable.

1. **Prove one install per harness, off the branch.** Add the local worktree (or `owner/repo@ref`) as a
   marketplace, install a standards plugin, and confirm a routed skill loads *and* its doc opens from
   inside the plugin copy — that last part is the whole point of the payload rewrite, and the only thing
   a green generator cannot tell you. Then remove the marketplace and plugin again, as the earlier spike
   did. This mutates machine config, so it is Tommy's to run or to authorize.
2. **Merge both PRs** once step 1 passes. `agent-standards#2` and `dotagents#1`, both CI green.
3. **Then run `pwsh dotagents/.agents/deploy-skills.ps1`** for the personal half (`~/AGENTS.md`,
   `~/.claude/`, the 10 utilities, and the standards-domain junctions). It must come after the merge: the
   junctions would otherwise point at `standards/` trees that exist only on the branches, so a
   `git checkout main` would dangle every domain.

Only once step 1 passes is the #637 standing constraint below actually satisfied — it asks for proof, and
a merge is not proof.

**Standing constraint - #637 does not merge, and no session may propose merging it.** Verified
2026-08-18: `~/.claude/skills` is 48/48 junctions (41 -> `dotagents`, 7 -> `agent-standards`) and
`installed_plugins.json` carries no standards plugin. The branch guts `RESULT_PATTERN.md`, both
`CODE_CONVENTIONS.md`, both `CODE_PATTERNS.md` and deletes `UNIT_CONVENTIONS.md`, `E2E_CONVENTIONS.md`,
`MICROSERVICE_COMMUNICATION.md`, `DEBUGGING_CONVENTIONS.md`, `CONVENTIONS.md`. Move a clone or open the
repo anywhere else and those rules are gone. **Phases 5 and 7 have made it organized and built its
installer; it merges only once an install is actually PROVEN on a clean machine without junctions** - built
is not proven, and both plugin repos still have the payload sitting on an unmerged branch. Earlier revisions of this ledger said
"enqueue-ready, review clean, paused on Tommy's read"; both were true and neither was the point - a
clean review speaks to the diff's own quality, never to whether what it deletes has a home. Phase 6
riding this PR fixed the *enforcement* ordering, never the *delivery* ordering. Do not reinstate a merge
instruction until that condition actually holds.

1. **The one piece of Phase 7 still outstanding is `concertable-delivery`**, and it is blocked on 5b:
   its `standards/concertable/` domain does not exist yet. Adding it is one `payloads.json` entry plus the
   domain, because the mechanism is built and guarded. Nothing else in Phase 7 remains to design.

   Two things Phase 5 hands it: the plugin payload generation for `standards/` already exists in
   `agent-standards/.agents/sync-generated.ps1` (tree copied into the plugin, router paths rewritten
   relative to their own `SKILL.md`), so `dotnet-standards` / `react-standards` follow that shape rather
   than inventing one. And the two generators are now near-identical PowerShell in two repos - the
   duplication this plan otherwise forbids. Sharing them needs a package or submodule, so it is Phase 7's
   to solve, not something to fix by copying a third time.

2. **Phase 5b / 5c / 3c / 4**, in that order now the tree exists. 5b: `api/agents/` is deleted, not
   thinned - the polyrepo cut leaves no `api/` node to host it. 5c: the discovery pass, since relocation
   only moves rules that already exist as prose (B2B's stance taxonomy - `TenantScopedDbContext`,
   `ReadDbContext`, `AdminDbContext` and the filtered-entity list - lives only in code). 3c: the six
   markdown items outside the conventions folders (`app/README.md` is still the unmodified Vite
   scaffold; `notes/Concert-Rust-Analysis.md` is referenced by nothing). 4: collapse the remaining
   duplication rows - chiefly `api/AGENTS.md:28-47` still restating 20 lines of seeding inline.

3. **Cut the E2E doc footprint as one pass** (Tommy: "use this as an opportunity to cut all of this
   bloat"). None of it is stale, but almost none of it is a *convention*: `E2E_CONSIDERATIONS.md` (37)
   deletes with its four sections redistributed ("do not add timeouts" is already owned verbatim by
   `failing-tests`; the 16-line Stripe-card section names its own unfixed root cause, so it is a
   `TECH_DEBT.md` entry; "`checkout-awaiting` timing out" is a debug symptom for the `e2e-ui-debug`
   table). `E2E_UI_CONVENTIONS.md` (26) drops to ~5 - page-object naming/shape, `data-testid`
   kebab-case, step bindings that make no Playwright calls, and API-not-UI setup are generic
   Reqnroll+Playwright rules that belong in `e2e-scenarios`, leaving only the roster (`WorkflowState`,
   the Stripe-iframe selector exception, the `AcceptApplicationPage` examples). The four
   `.agents/skills/e2e-*` runbooks (711) each restate the Docker-health rule twice; `remote-validation`
   owns it - replace with a pointer.

4. **Make the 7 process skills concrete.** They were written generic for a shared repo; `merging` must
   lose the confirm-loop *body* and keep the rule, with `.agents/skills/merge/SKILL.md` owning the
   procedure. Same for `pr-preflight`.

5. **Standards fixes found while cutting** - none blocking, and they now land in the `standards/<domain>/` doc rather than the `SKILL.md`: `persistence` teaches a context-typed base
   (`Repository<TEntity, OrderDbContext, Guid>`) but Concertable's shared bases are capability-typed
   with no `TContext`; "one repository per entity" has no skill home; `e2e-scenarios` closes by pointing
   at "the `agent-process` standards", a name no skill has; `csharp-style`'s `extension()` section lacks
   the "migrate every ordinary member of a container you touch" rule and the `[LoggerMessage]`
   exception; `csharp-naming`'s `XMappers` example still shows the `public static ... (this X x)` form
   the same corpus now bans; and **integration-event wire versioning has no skill at all** (kept in
   `api/agents/CODE_CONVENTIONS.md`; natural home is `microservice-boundaries` or `proto`, neither of
   which mentions `MessageType`).

6. **Tommy's, not agent work.** **Prove one plugin install per harness** once both PRs land (step 2 of the
   opening sequence) — it mutates machine config, and it is the evidence the #637 constraint waits on.
   Approve the Codex `PreToolUse` hook once in a Codex session in this
   worktree (inert until approved, and safe now that ENF1 made it actually fire). Archive
   `agent-starter-kit`. The `Concertable/agent-standards` -> `standards` rename: two of its five hard
   references die with vendoring, and there are **zero installed consumers today**, so it is cheapest
   now - but it is a coordinated change across repos. Rule on `GenreController` in a shared library
   (`api/Concertable.Shared/TECH_DEBT.md:70`). Whether React Hook Form is adopted (in no `app/`
   workspace today).

7. **Deferred to its own PR:** auto-load thinning of root `AGENTS.md` (the 86 merge lines and 32 Docker
   lines `/merge` and `scripts/e2e.ps1` already automate), the analyzer push-down plus
   `EnforceCodeStyleInBuild`, and **a CI job running the Python hook tests** - nothing in
   `.github/workflows/` runs them today, so every hook gate here is only as live as the last person who
   ran it locally (`agent-standards` has this; Concertable does not). Its own PR because touching
   `.github/workflows/**` pulls the merge gate's security-marker requirement in with it.

**Also fixed this session, outside the plan's phases:** Concertable's merge gate policed *other* repos -
`gh` ran in the hook's own directory, so `cd <other-repo> && merge 1` resolved #1 against Concertable and
blocked an `agent-standards` merge citing `TS/AuthRefactor`. Fixed on this branch (`2d553e9d4`, 15 -> 17
cases). **It only goes live when #637 lands, so until then do not run cross-repo merges from a
Concertable session.** Concertable's vendored `skill_router.py` is also now behind upstream
(`4177899e` vs `89ed23e4`) - harmless, since its hash check verifies self-consistency rather than
currency, and Phase 7 retires vendoring anyway.

## Also Tommy's, not blocking

`tomjseery/agent-starter-kit` (public, 7 skills) looks redundant with `tomjseery/dotagents` (private, the
same 7 plus `pull-main`, `sync-all`, `unmerged`) — the same duplication disease at repo level.
