# Guidance-docs restructure — progress

- Plan: `plans/docs/GUIDANCE_DOCS_RESTRUCTURE_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/guidance-restructure`
- Also delivered by this ledger: roadmap item `docs/agent-standards`, now checked off
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-guidance-docs`
- Branch: `Docs/GuidanceDocsRestructure`
- PR: #637 — **open working branch, NOT for merge** (see the standing constraint under Next Steps; it deletes 2,662 lines whose replacement is not yet correctly tiered). Label `skip-e2e`.
  commit). Updated for base currency three times on 2026-08-17: from **69 behind** and `DIRTY` (three doc
  conflicts resolved, below), from 2 behind after platform-sync #645 merged — a clean merge carrying only
  the `<ConcertablePlatformVersion>` bump `0.1.0-alpha.0.1055` → `0.1.0-alpha.0.1061` across the five
  service `Directory.Packages.props` — and from 10 behind at `2b04d57e2`.
- Shared repos **today**: `tomjseery/dotagents` — `standards/dotnet/` (20 docs) + `standards/communication/` (2); `tomjseery/react-agents` — `standards/react/` (9), **created 2026-08-18, on `main`, CI green**; `Concertable/agent-standards` — `standards/process/` (7) + the `skill_router` hook, **no stack sections yet**. `dotagents` and `agent-standards` are still on `Refactor/StandardsDomainTree` with PRs open and CI green (`dotagents#1`, `agent-standards#2`), neither merged. Target shape and the three purged wrong models: `## Topology` below.
- Dependency/package gates: no consumer migration to do, but this PR **will** trigger publish + platform sync — `publish-packages.yml` triggers on the coarse `paths: api/**`, which this branch's `api/**` markdown matches. MinVer republishes and a `chore/platform-sync-*` PR opens; non-breaking (no published type changed), so it should auto-merge green. Follow it to green anyway — whoever merges owns the sync.
- Last reconciled: 2026-08-18 — merged `origin/main` into the branch after the Tier 2 split (was 23 behind, clean merge, no conflicts). `dotagents` at `1389a2e` (pushed, CI green), `react-agents` at `b69b517` (`main`, CI green), `agent-standards` unchanged at `88cf091` — re-check currency at enqueue time

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

**The #637 gate is now SATISFIED — the install is proven, 2026-08-18.** Both plugins were installed for
real from the branch worktree as a local-path marketplace, in **both** harnesses, and every one of the 20
`dotnet-standards` routers resolved to a real doc inside the plugin's own copy (20/20 in Claude, 20/20 in
Codex). Claude copied the payload into a sha-versioned cache even from a `Directory` source, confirming
copies-not-references. Codex read `.agents/plugins/marketplace.json` natively while Claude used
`.claude-plugin/` — one repo, both formats. The machine was restored to its exact pre-test snapshot
afterwards. Measured cost: `dotnet-standards` adds **~5,242 always-on tokens** (20 descriptions), each
router ~90 on invoke.

**One caveat, stated rather than glossed:** "without junctions" could not be tested literally, because this
machine has 48 of them. What was proven junction-independently is the part that matters — the payload is
self-contained and its internal paths resolve inside the cache. The end-to-end "agent reads it on a machine
that never cloned the repo" is inferred from that, not observed.

**Tier 2 is now real.** `tomjseery/react-agents` exists, private, with the nine React docs, their routers,
the generator, the marketplace, the `react-standards` plugin and a green CI check; `dotagents` no longer
carries them, and `deploy-skills.ps1` takes all three repos as source roots. The 38 deployed routers were
re-checked afterwards and every one resolves to a real doc — `~/.agents/standards/` had in fact never been
deployed on this machine before, so that gap closed with the same run.

**What remains after that:** `agent-standards` still has no stack sections (Next Step 1), `api/agents/` is
still a destination the polyrepo cut removes (5b), the
discovery pass has not run (5c, with `dotnet/STACK.md` its first known gap), Phase 3c (markdown outside
the conventions folders), Phase 4 (rows still with >1 home, chiefly seeding across `api/AGENTS.md` and
`SEEDING_CONVENTIONS.md`), and the deferred auto-load thinning of root `AGENTS.md`.

## Done

**Tier 2 split — `tomjseery/react-agents` exists** (`react-agents` `main` `b69b517`, CI green;
`dotagents#1` `1389a2e`, CI green)

- **The nine React docs and their routers left `dotagents`**, with the delivery machinery that makes them
  installable alone: `sync-generated.ps1`, `.agents/plugins/{marketplace,payloads}.json`, the
  `react-standards` plugin and the CI `-Check` job. The generated payload was diffed against the copy
  `dotagents` had produced — INDEX, plugin standards and plugin routers all byte-identical, so the move
  carried no content change. The routers' one authored fact, the doc path, is unchanged; only the repo they
  name moved.
- **`dotagents` is now .NET plus `communication/`**: two plugins, two domains, and a marketplace
  description that no longer claims the TypeScript half. Its README's frontend gap list moved to
  `react-agents` with the corpus it described.
- **`deploy-skills.ps1` takes three source roots and three standards roots.** Its duplicate-domain guard
  already covered the collision; nothing else in it changed. Re-running it repointed 18 skill junctions at
  `react-agents` and — unplanned but found by the same run — created the four `~/.agents/standards/`
  domain junctions **for the first time**. Until then every deployed router named a
  `~/.agents/standards/...` path that did not exist on this machine; Phase 6a had deployed the skills and
  not the trees they route to. All 38 deployed routers now resolve to a real file.
- **The generator is a third copy, and the note that said to revisit at the third repo is now due.** The
  `react-agents` copy differs from the `dotagents` one only in its header paragraph. Sharing them needs a
  published module or a submodule, because each repo's CI must self-verify without reaching a private
  sibling; `dotagents/ARCHITECTURE.md` now carries that cost explicitly rather than leaving it in this
  plan, and names a fourth repo as the trigger.

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

## Topology — four tiers, settled 2026-08-18

**Authoritative statement: `dotagents/AGENTS.md` (mirrors to `~/AGENTS.md`, so it loads every session) and
`dotagents/ARCHITECTURE.md`. The plan's "target structure — four tiers" section is the file-by-file version.
This ledger does not restate the model — it records only what is not yet true of it.**

| Tier | Repo | Status |
|---|---|---|
| Generic .NET | `tomjseery/dotagents` | 20 docs in `standards/dotnet/` — correct |
| Generic React/TS | `tomjseery/react-agents` | created 2026-08-18 — 9 docs in `standards/react/`, moved out of `dotagents` |
| Concertable | `Concertable/agent-standards` | only `standards/process/` exists; needs `dotnet/` + `react/` sections, into which the 480 lines still in `api/agents/` + `app/agents/` re-home |
| One microservice | that service's own repo | 67 backend + 8 app `AGENTS.md` in place, and each may name sibling docs (`CODE_CONVENTIONS.md`) where a module has conventions of its own |

`dot` in `dotagents` is **dotNET**, not dotfiles. `agent-standards` gets no `platform/` or `concertable/`
folder — Concertable *is* the platform and the repo boundary already carries that scope.

**Three models were recorded as settled before this one, each wrong, and each rebuilt confidently by a later
session from what its predecessor wrote down:** one merged `standards` repo holding generic and product rules
together; then `dotagents` holding both stacks; then a `platform/` folder inside the Concertable repo. All
three are purged from the plan and this ledger. The misreading of `dot` as "dotfiles" is what produced the
second.

`agent-utilities` keeps the machine tooling (`sync`, `worktree`, `recents`, `search`, `unmerged`, …), which is
neither a standard nor stack-specific. `agent-starter-kit` is archived — a strict subset of `dotagents`, two
of its skills carrying a BOM that breaks frontmatter parsing.

**A service repo carries no skills at all (Phase 7).** Not `.claude/skills/` stubs, not `.agents/skills/`,
not a vendored hook. Skills arrive as plugins installed once per machine at user scope; a repo keeps only its
`AGENTS.md` roster, its `skill-routes.json`, and the two per-harness wiring files.


## Next Steps

**The model is settled and written down; the trees do not match it yet.** Authoritative statement:
`dotagents/AGENTS.md` (mirrors to `~/AGENTS.md`, so it loads every session) and
`dotagents/ARCHITECTURE.md`; the file-by-file target is the plan's "target structure — four tiers" section.
Three superseded models were purged — do not reintroduce a merged standards repo, `dotagents` holding both
stacks, or a `platform/`/`concertable/` folder.

Audit findings: `reviews/Docs-GuidanceDocsRestructure-AuditFindings.md` (16 defects). Several move depending
on which tier a doc lands in, so the remaining split comes first. **Tier 2 is done** — `react-agents`
exists and holds the React corpus; what is left is the Concertable tier.

1. **Add `standards/dotnet/` and `standards/react/` to `agent-standards`, and re-home the 480 lines** still
   in `api/agents/` (`CODE_CONVENTIONS`, `CODE_PATTERNS`, `INTEGRATION_CONVENTIONS`, `MODULE_STRUCTURE`,
   `RESULT_PATTERN`, `SEEDING_CONVENTIONS`) and `app/agents/` (`CODE_CONVENTIONS`, `CODE_PATTERNS`). Target
   docs are named in the plan. `api/agents/` is then **deleted** — the polyrepo cut leaves no `api/` node to
   host it. Per-service and per-module `AGENTS.md` stay, and may name sibling docs (`CODE_CONVENTIONS.md`)
   where a module has conventions of its own.

2. **Cut the process corpus over — P0 in the findings.** `standards/process/` was copied, not moved: its
   Concertable originals sit at full length and nothing references the extracted docs (zero hits for any
   process skill name across Concertable markdown), and `MERGING.md` duplicates root `AGENTS.md`'s poll loop
   near-byte-for-byte. Slim the Concertable originals to Concertable-only procedure and point at the skills,
   exactly as the React half already does.

3. **Fix the 16 audit defects**, P0 correctness first: the `[LoggerMessage]` carve-out (the corpus currently
   bans its own canonical example), the `XMappers` examples that demonstrate a banned form, and
   `PERSISTENCE.md`'s impossible repository signature. Then the paraphrase-losses — `axios`,
   `Reqnroll`/`Playwright`, `Aspire` and its four extension methods, `Docker`/`pre-login handshake`,
   `Monitor`, the `NAMING.md` precedent column, `[Collection]`/`InitializeAsync`,
   `Environments`/`IHostEnvironment`, the raw-hook litmus members, the TanStack API names, `silenceErrors`,
   the retry cap, `grep -rniE`.

4. **Move Concertable's four remaining hooks to `agent-standards`** — `plan_handoff_stop.py` + launcher,
   `plan_graph.py`, `docs_reachability.py`, `merge-review-gate.py`. They enforce standards that already
   moved, so enforcement sits apart from its rule with nothing watching for drift. **Verify a plugin `Stop`
   hook fires with zero repo wiring first** — that was proven only for `PreToolUse`.

5. **Then the remaining phases**: 5c (the discovery pass — conventions that exist only in code, e.g. B2B's
   stance taxonomy), 3c (markdown outside the conventions folders), 4 (the last duplication rows), and the
   deferred auto-load thinning of root `AGENTS.md`.

**#637 does not merge until steps 1-2 land.** It deletes 2,662 lines whose replacement is now organized and
installable — the plugin install was proven in both harnesses on 2026-08-18 — but not yet correctly
*tiered*. `react-agents` now exists, so Tier 2 is no longer the blocker; `agent-standards` still has no
stack sections, and the 480 lines in `api/agents/` + `app/agents/` still have nowhere correct to go, so the
corpus #637 points at is still not the one the model describes.

**Tommy's, not agent work:** approve the Codex `PreToolUse` hook once in a Codex session in this worktree;
archive `agent-starter-kit`; rule on `GenreController` in a shared library
(`api/Concertable.Shared/TECH_DEBT.md:70`); decide whether React Hook Form is adopted (in no `app/`
workspace today); settle the Shouldly-for-unit-tests open call now recorded in `dotnet/testing/UNIT.md`.


## Also Tommy's, not blocking

`tomjseery/agent-starter-kit` (public, 7 skills) looks redundant with `tomjseery/dotagents` (private, the
same 7 plus `pull-main`, `sync-all`, `unmerged`) — the same duplication disease at repo level.
