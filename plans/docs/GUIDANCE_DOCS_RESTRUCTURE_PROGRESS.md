# Guidance-docs restructure — progress

- Plan: `plans/docs/GUIDANCE_DOCS_RESTRUCTURE_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/guidance-restructure`
- Also delivered by this ledger: roadmap item `docs/agent-standards`, now checked off
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-guidance-docs`
- Branch: `Docs/GuidanceDocsRestructure`
- PR: #637. Reviewed at `fcec9925` with 4 of 5 findings fixed; three commits have landed since, so both markers are stale and `/incremental-review` plus `/security-review` must run before enqueue. Still needs the upstream merge that closes ENF12, then an explicit merge instruction. Label `skip-e2e`.
  commit). Updated for base currency three times on 2026-08-17: from **69 behind** and `DIRTY` (three doc
  conflicts resolved, below), from 2 behind after platform-sync #645 merged — a clean merge carrying only
  the `<ConcertablePlatformVersion>` bump `0.1.0-alpha.0.1055` → `0.1.0-alpha.0.1061` across the five
  service `Directory.Packages.props` — and from 10 behind at `2b04d57e2`.
- Shared repos **today**: `tomjseery/dotagents` — `standards/dotnet/` (20 docs) only; `tomjseery/react-agents` — `standards/react/` (9), created 2026-08-18 on `main`; `Concertable/agent-standards` — `standards/process/` (7) + `standards/dotnet/` (12) + `standards/react/` (8) + the `skill_router` hook, three plugins. All three repos CI green at `852e467` / `9cb5ea3` / `b6312f7`. `dotagents` and `agent-standards` are still on `Refactor/StandardsDomainTree` with PRs open and CI green (`dotagents#1`, `agent-standards#2`), neither merged. Target shape and the three purged wrong models: `## Topology` below.
- Dependency/package gates: no consumer migration to do, but this PR **will** trigger publish + platform sync — `publish-packages.yml` triggers on the coarse `paths: api/**`, which this branch's `api/**` markdown matches. MinVer republishes and a `chore/platform-sync-*` PR opens; non-breaking (no published type changed), so it should auto-merge green. Follow it to green anyway — whoever merges owns the sync.
- Last reconciled: 2026-08-19 — `/incremental-review` over `890c68d2..16230d76` (the hook-ownership
  commits) found five findings and four are fixed. `agent-standards` `32795d8` (#2) carries the gate
  fix and the `delivery` field, `sync-generated.ps1 -Check` green; `dotagents` `e52a11b` (#1),
  `react-agents` `1ebb42b` (`main`) unchanged. #637 at `fcec9925` + the marker commit, 0 behind
  `origin/main`, all checks green at `16230d76`. Re-check currency at enqueue time.

**Scope changed 2026-08-17: this is no longer a docs PR.** It now carries build behaviour
(`api/TestConventions.targets` gating every test project) and a PreToolUse hook, because Phase 6 must land
with the thinning rather than after it. `skip-e2e` is still correct — no Step 4 positive trigger: no UI
flow, no HTTP/gRPC contract, no published-package shape, no auth/routing change. But PR CI now matters far
more than it did, since the new targets file participates in every project's build.

## Current state

**All four tiers match the model, and the process corpus is now moved rather than copied. Nothing is
merged.** The corpus is 56 docs across three standards repos,
each doc owned by exactly one router skill, delivered as five plugins and — on this machine — as junctions.
The in-repo docs hold only this system's roster of real types, contexts, clients, tables and pins.

| Repo | Head | Contents |
|---|---|---|
| `tomjseery/dotagents` #1 | `e52a11b` | `standards/dotnet/` 20 docs · plugin `dotnet-standards` · 10 utility skills · `deploy-skills.ps1` |
| `tomjseery/react-agents` `main` | `1ebb42b` | `standards/react/` 9 · plugin `react-standards` |
| `Concertable/agent-standards` #2 | `32795d8` | `standards/process/` 7 + `dotnet/` 12 + `react/` 8 · plugins `agent-process` (owns the hook), `concertable-dotnet`, `concertable-react` |
| `Concertable/concertable` #637 | `fcec9925` | the reduction, the process cut-over, and the review's four fixes. Needs the upstream merge for ENF12, then an explicit merge instruction |

**Auto-loaded floor.** An `api/**` prompt loads `api/AGENTS.md` (77) + `api/ARCHITECTURE.md` (62) with
**zero** `@`-imports, from 1,429 lines at the start of this plan. An `app/**` prompt loads
`app/AGENTS.md` (36), from 786. A unit-test project pulls in a 6-line stub; an E2E project none. Root
`AGENTS.md` is 150 lines, from 298 — thinned by the process cut-over rather than by a follow-up PR.

**Every hook is upstream-owned (2026-08-19).** All six live in `agent-standards` and are vendored down;
this repo keeps only `skill-routes.json` and `merge-gate.json` — its data — and the tests over them. A
plugin `Stop` hook was proven to fire and to block with zero repo wiring before any of it moved.

**Enforcement is complete at three tiers and in both harnesses.** `api/TestConventions.targets` fails a
misnamed or misclassified test project; the write-time `skill_router.py` blocks the first write into a
routed path whichever harness makes it, resolving descriptions from plugin caches as well as junction
roots; `/review` resolves the standards it owes from the same `skill-routes.json`. Every route now names
both the generic skill and its Concertable counterpart.

**Deployed here:** 66 skills junctioned into both `~/.agents/skills` and `~/.claude/skills`, and 5
repo-scoped standards junctions at `~/.agents/standards/<repo>/<domain>/`. All **112** deployed routers
(56 × two roots) verified to open a byte-identical copy of their own repo's authored doc.

**The install is proven, in both harnesses (2026-08-18).** Plugins installed for real from a branch
worktree as a local-path marketplace; every `dotnet-standards` router resolved inside the plugin's own
copy, 20/20 in Claude and 20/20 in Codex. Claude copied the payload into a sha-versioned cache even from a
`Directory` source, confirming copies-not-references; Codex read `.agents/plugins/marketplace.json`
natively while Claude used `.claude-plugin/`. The machine was restored to its pre-test snapshot. Measured
cost: ~5,242 always-on tokens per 20 router descriptions, ~90 per router on invoke — so the 20
`concertable-*` routers add roughly the same again.

**Caveat, stated rather than glossed:** "works on a machine that never cloned the repo" could not be
tested literally, because this machine has the junctions. What was proven junction-independently is that
the payload is self-contained and its internal paths resolve inside the cache; the end-to-end claim is
inferred from that, not observed.

**The process corpus has one home (2026-08-19).** The six Concertable originals went 969 → 572 lines, each
keeping only this repo's labels, scripts, hooks and commands and naming the skill that owns the generic half;
all seven skill names are now referenced from in-repo markdown, where the audit measured zero. Root
`AGENTS.md` is 150 lines, from 298. The merge poll loop lives only in `.agents/skills/merge/SKILL.md` Step 4.

**The audit is closed (2026-08-19).** All 16 defects and the four P2 rows are fixed and the findings file is
deleted; the corpus no longer contradicts itself, and no rule is missing the identifier it turns on.

**Phase 4 is complete.** The Docker-health block no longer exists in more than one place: the mechanism
lives in `remote-validation`, root `AGENTS.md` keeps the invariant, and the four `e2e-*` skills keep only
the command and the stop-and-tell-the-user procedure.

**Enforcement now lives with its rule (2026-08-19).** All six hooks are vendored from
`agent-standards`; Concertable keeps only its two tables (`skill-routes.json`, `merge-gate.json`) and the
tests over them. A plugin `Stop` hook was proven to fire *and block* with zero repo wiring first — the
gate this step was held behind.

**What remains:** 5c (discovery), 3c (markdown outside the conventions folders), and ENF12 — the
vendored hooks' provenance pins name commits that live only on `agent-standards` #2's branch, so that PR
must land before #637 is enqueued.

**The move itself is now audited, not asserted (2026-08-19).** What is mechanically proven: every doc the
plan's four-tier list names exists, and the only absences are rows the plan itself marks `GAP`/`NEW` (1 in
`dotagents`, 5 in `react-agents`, 3 in `agent-standards`) plus the two 3c holdovers; `api/agents/` and
`app/agents/` are gone; doc↔router is 1:1 in all three repos (20/9/27, no doc with none or two); all **112**
deployed routers open a byte-identical copy of their own repo's doc, so no cross-repo shadowing; the six
vendored hooks hash to the commits `vendored.json` records.

**What the audit found still outstanding** — every deleted doc's backticked identifiers were checked
against the whole current corpus, and four live concepts landed nowhere:

- `IDomainEvent` (60 code files) and `IDomainEventDispatcher` (7) — the raise-in-the-domain /
  dispatch-to-integration-event mechanism is documented in no standard and no in-repo doc. `MM_NORTH_STAR.md`
  held it and was deleted as obsolete; this is one of the "still-true corollaries" that was never inlined.
- `ExcludeFromMigrations` (37 code files) — same source, same loss.
- `CallCredentials` (1) — how a gRPC call carries credentials, from `MICROSERVICE_COMMUNICATION.md`.
- Concertable's tenancy inventory is *not* lost but is Tier-4 only (`api/Concertable.B2B/CODE_PATTERNS.md`,
  its `ARCHITECTURE.md`), and there is no `concertable-multitenancy` router, so the write-time router never
  loads it; the generic `multitenancy` skill is routed on `Repository.cs$` alone.

Two in-scope doc defects the audit also turned up, neither of them 5c:

- **Six in-repo docs still cite a `CLAUDE.md` for content** — `ARCHITECTURE.md:13`, `app/shared/AGENTS.md:14`,
  `app/web/shared/AGENTS.md:9` and `:34`, `app/web/b2b/shared/AGENTS.md:22`,
  `api/Concertable.B2B/src/Modules/Deal/ARCHITECTURE.md:329`, `api/Concertable.Shared/TECH_DEBT.md:9`. Every
  `CLAUDE.md` is a one-line `@AGENTS.md` stub, so each of these points at a file without the text it quotes.
  Phase 2 claims this was fixed.
- **`app/shared/AGENTS.md` still restates ~40 lines** of the shared-tier intersection rule and the
  `User`/`B2bIdentity` composition that `react/SHARED_CODE.md` and `agent-standards/react/IDENTITY.md` now
  own — a Phase 4 dedupe that was missed because the file is not under `app/agents/`.

## Done

**The last four hooks moved to `agent-standards`** (2026-08-19; `agent-standards` `7cb3fdd`, this PR
`569591efe`)

- **The gate first: a plugin `Stop` hook was only ever assumed to work.** Proven by real install of a probe
  plugin into a directory with **zero** hook wiring: it fires; it **blocks** (the model was re-prompted and
  complied); `stop_hook_active` flips to `true` on the retry, so `plan_handoff_stop.py`'s recursion guard
  behaves identically under plugin delivery; and both `CLAUDE_PROJECT_DIR` and the payload's `cwd` give the
  consuming repo's root, which is what the hook already resolves from. Machine restored to its pre-test
  snapshot (1 marketplace, 4 plugins). Proven in headless `-p`; interactive uses the same machinery but was
  not separately observed.
- **`plan_handoff_stop.py`, `plan_graph.py` and `docs_reachability.py` moved byte-identical** — all three
  already took their root from the payload or `--root`, so nothing needed changing.
- **`merge_review_gate.py` had two couplings to this repo and both are now data.** Jurisdiction was "does
  `cwd` belong to the repo this hook *file* lives in", which a plugin-delivered copy can only ever answer
  no to — inert while looking wired. It is now an opt-in `.agents/merge-gate.json` found by walking up
  from the directory the merge actually runs in, so `cd <other-repo> && merge` is judged by *that* repo's
  opt-in; `same_repository()` is deleted rather than kept as dead code. Its security-path list named
  Concertable services; the generic patterns (CI workflows, auth/credential vocabulary) stay in the hook
  and the service names moved to `security_paths`. A present-but-broken table blocks rather than failing open.
- **The launcher would have blocked every turn under plugin delivery.** Its currency check asks whether a
  checkout still matches its `origin/main`; a plugin has no repo above it, so the check could only fail. It
  now detects `CLAUDE_PLUGIN_ROOT` and runs the sibling implementation, keeping **one** `Stop` entrypoint
  for both delivery routes.
- **Two pre-existing defects surfaced, neither caused by this move.** The vendored `skill_router.py` was
  pinned at `268796e` with three upstream commits missing — including plugin-cache resolution and the
  orphaned-cache fix. Nothing was watching for it: `sync-generated -Check` validates the *generator*, not a
  consumer's copy. Refreshing it then failed `test_an_unreadable_routes_file_fails_open`, a mechanism test
  duplicated here that still asserted the fail-open behaviour upstream deliberately reversed in `78e0586`.
- **Mechanism tests moved wholesale; this repo keeps only tests over its own tables.** All five plan/docs
  test files were already fixture-based, and the merge gate's hand-rolled script is rewritten as unittest.
  The eight duplicated router mechanism tests here are deleted, with a class docstring recording why the
  duplication drifted. `agent-standards` 13 → 145 tests; Concertable 20 → 12.
- **The Codex gap is recorded, not silent.** The gate knows only Claude's `Bash` tool name, so it stays
  Claude-wired; `SHELL_TOOLS` in the hook states that vocabulary, and `SINGLE_HARNESS` in
  `test_vendored_hooks.py` is the one legal half-wiring, carrying its reason plus a companion test that
  fails once the exemption is no longer needed. The both-harnesses rule now skips hooks wired in neither,
  because `plan_graph.py` and `docs_reachability.py` are command-line checks rather than hooks.
- Verified: `agent-standards` **145 passed**, generator `-Check` clean; Concertable hook suite **12 passed**,
  `docs_reachability` **0 errors / 26 warnings** (all pre-existing `plans/` working docs), `plan_graph`
  **0 errors**, and the moved gate correctly blocks `gh pr merge 637` on the stale review marker.

**Incremental docs-review of the branch, and its findings fixed** (2026-08-19, `890c68d28`)

- Range `b525776b4..b413784cd` — the branch's own 25 commits since the last marker. **Three findings,
  all the same class and all in `docs/INDEX.md`:** five architecture rows still pointed at
  `api/ARCHITECTURE.md` for topics `1e9bd3088` moved out of it (one quoting a section that no longer
  exists there), a backend row sent "DTOs vs Responses" to `api/AGENTS.md` which contains neither word,
  and a frontend row pointed at the `app/web/AGENTS.md` "HTTP errors" section deleted the same day. All
  fixed; the file whose job is to be the anti-drift map is the one that had drifted.
- Clean on everything checked mechanically: reachability 0 errors, all 35 routed skills resolve, every
  `` `x` skill `` reference and every `INDEX` link resolves, every enforcer the INDEX names exists,
  `scripts/worktrees.ps1` really takes `audit|close|retire` + `-PlanManaged`, `merge` Step 4 really is the
  E2E tier, and the hook suites pass (94 tests + 13 subtests + 8 merge-gate cases).

**Phase 4 finished — the last duplication row** (2026-08-19)

- The Docker-health pre-flight was near-verbatim in `e2e-api-debug`, `e2e-debug`, `e2e-ui-debug` and
  `e2e-ui-regress`. Each now carries one pointer line plus what is genuinely local — the
  `./scripts/docker-health.ps1` invocation, that `./scripts/e2e.ps1` gates on it automatically, and the
  stop-and-wait-for-Docker-Desktop procedure. The mechanism (why `docker ps`, `hello-world` and a bare TCP
  connect all miss it, and the `pre-login handshake` signature) is stated once, in `remote-validation`.

**Audit closed — 16 defects and 4 duplication rows** (2026-08-19; `dotagents` `e52a11b`,
`react-agents` `1ebb42b`, `agent-standards` `c66157c`)

- **P0 correctness — the three that made following the doc produce a violation.** `dotnet/STYLE.md` banned
  legacy `this` extension members absolutely while `LOGGING.md`'s own canonical example is a
  `[LoggerMessage]` partial method in exactly that form; the carve-out is back, as is the "touch a container,
  migrate every ordinary member in it" clause, without which the rule permitted the mixed container it exists
  to ban. The `XMappers` worked example in `NAMING.md` and `structure/PROTO.md` had been rewritten into the
  banned form; both are `extension(Receiver)` blocks again. `data/PERSISTENCE.md` taught
  `Repository<TEntity, OrderDbContext, Guid>`, threading a concrete context through a base that deliberately
  has no `TContext` parameter; the capability triple that decides *which* base to inherit is restored.
- **13 paraphrase-losses restored**, each an identifier the prose had been left looking complete without:
  `axios` / `AxiosResponse<T>` / `{ responseType: "arraybuffer" }` — absent from the entire React tree, so
  `create()` was neither greppable nor callable and `STACK.md`'s no-second-HTTP-client ban named no
  incumbent; `Reqnroll` / `Playwright`; `Aspire` with `AddServiceDiscovery()`, `AddServiceDefaults()`,
  `AddStandardResilienceHandler()`, `AddGrpcClient<T>()`, `OpenTelemetry` and the scope-limiting negative
  that existed *because* it is the assumption people make; `Docker` / `docker ps` / `docker-proxy` and the
  `pre-login handshake` signature the rule promised and then withheld; `Monitor`; `NAMING.md`'s precedent
  column, the calibration anchors for a distinction the doc itself calls mechanics-not-vibes; `[Collection]`
  and `InitializeAsync()` (constructor-versus-async-hook is a real silent failure) plus `Environments` /
  `IHostEnvironment`; `.data` / `.isPending` / `.mutate` and the raw-versus-facade litmus built on them;
  `useQuery` / `useMutation` / `QueryCache` / `MutationCache` / `mutateAsync`; `silenceErrors`; the retry cap
  `2`; `grep -rniE` and the casing enumeration, in the rule whose stated point is removing the discretion.
- **The four P2 rows.** One-repository-per-entity and `InsertAsync`-vs-`AddAsync` gained the generic home
  they lacked — both were surviving only in the product doc, whose copy is now roster only;
  `FAILING_TESTS.md` regained the per-tier routing concept; and the `isAxiosError` ban went from three
  statements to one — generic `react/HTTP.md` owns the clause, `agent-standards` keeps the inventory half,
  and `app/web/AGENTS.md`'s restatement is deleted, which also settles the plan's open decision 1.
- `sync-generated.ps1` re-run in all three repos so every plugin copy follows; `-Check` exits 0 in each.

**Process corpus cut over — copied became moved** (2026-08-19)

- Six originals slimmed to Concertable-only procedure, 969 → 572 lines: root `AGENTS.md` 298 → 150,
  `plans/AGENTS.md` 168 → 71, `plans/agents/PLAN.md` 328 → 233, `plans/agents/ROADMAP.md` 51 → 34,
  `docs/REMOTE_VALIDATION.md` 57 → 27, `PROMPTS.md` 67 → 57. Each names the owning skill and keeps only real
  labels, scripts, hooks, commands and the invariants whose violation is silent and expensive.
- **Deleted outright** (the skill states it, nothing here adds to it): the commit-when-green block and
  fewest-safe-merges from `plans/AGENTS.md`, the tech-debt / code-comments / throwaway-markdown sections from
  root `AGENTS.md` (the comment *policy* is global, per the Phase 3a decision), the branch-from-`origin/main`
  and casing-hazard paragraphs, the gate-ownership table and delivery loop from `docs/REMOTE_VALIDATION.md`,
  and the rename grep gate from `plans/AGENTS.md`.
- **Phase 3c's ruling executed on both sides.** The four-terminal-state poll loop now lives only in
  `.agents/skills/merge/SKILL.md` Step 4 — which previously carried a naive `state == MERGED` loop that could
  not see `DIRTY`, a red `merge_group` run, or the never-admitted glitch, so this fixed a real gap as well as
  a duplicate. Root `AGENTS.md` keeps four one-line invariants; the generic `agent-standards`
  `standards/process/MERGING.md` keeps the rule plus the five signals a loop must read and drops the body
  (95 → 78 lines), with `sync-generated.ps1` re-run so the `agent-process` plugin copy matches.
- **Four new `skill-routes.json` rows** where a path implies a process skill: `plans/**.md` and `PROMPTS.md`
  → `plans`; `TECH_DEBT.md`, any `AGENTS.md`/`CLAUDE.md`, and `.agents/skills/*/SKILL.md` → `docs-and-debt`.
  `--skills-for` verified against one path per row. `git-branching`, `committing`, `merging`,
  `remote-validation` and `failing-tests` get no row — no path implies them.
- Cross-references repointed rather than left dangling: `pr-preflight` (three), `package-cutover` (one, which
  named a `plans/AGENTS.md` section that no longer existed), and `docs/INDEX.md`'s whole Process table.
- Gates: `docs_reachability` 0 errors / 26 warnings (all pre-existing), `plan_graph` clean, 94 hook tests
  green.
- **Discrepancy left in place, not silently resolved:** Phase 3a recorded that "questions come before
  actions" and "act on reversible work" *stay global*, but neither is in `dotagents/AGENTS.md` — they are
  only in Concertable's root `AGENTS.md`. Cutting them would have deleted live rules with no other home, so
  they stayed. Moving them into the global file is a one-line change Tommy owns.

**Tier 2 split — `tomjseery/react-agents` exists** (`react-agents` `main` `b69b517`, CI green;
`dotagents#1` `1389a2e`, CI green)

- **The nine React docs and their routers left `dotagents`**, with the delivery machinery that makes them
  installable alone: `sync-generated.ps1`, `.agents/plugins/{marketplace,payloads}.json`, the
  `react-standards` plugin and the CI `-Check` job. The generated payload was diffed against the copy
  `dotagents` had produced — INDEX, plugin standards and plugin routers all byte-identical, so the move
  carried no content change. The routers' one authored fact, the doc path, is unchanged; only the repo they
  name moved.
- **`dotagents` is now .NET only**: one plugin, one domain, and a marketplace description that no longer
  claims the TypeScript half. Its README's frontend gap list moved to `react-agents` with the corpus it
  described. `standards/communication/` went too — see below.
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

**`standards/communication/` removed from `dotagents`** (`dotagents#1` `a7e9b40`)

- `REVIEW_COMMENTS.md` and `EXPLAINING_CODE.md`, the `draft-comment` and `explaining-code` routers, and the
  `communication-standards` plugin are gone, along with their marketplace and payload entries. They are not
  dotagents' to hold; the "one deliberate exception to this repo being stack-scoped" the plan recorded for
  them is deleted rather than re-argued. `dotagents` is now one plugin, one domain, 20 docs.
- The five junctions the removal orphaned (`draft-comment` and `explaining-code` under both
  `~/.agents/skills` and `~/.claude/skills`, plus `~/.agents/standards/communication`) were deleted as
  reparse points only. `deploy-skills.ps1` leaves orphans in place by design, so it does not do this — 46
  skills and 3 standards domains deployed, nothing dangling.
- **Consequence to be aware of: `~/.claude/skills` no longer carries `draft-comment` or
  `explaining-code`.** Tommy's global `CLAUDE.md` invokes both by name. Nothing in this repo, in
  `implement`, `ship`, `verify`, `worktree`. Their text is recoverable from `dotagents` history at
  `1389a2e^`.

**Step 1 — `agent-standards` gained its stack sections and the 480 lines came home**
(`agent-standards#2` `b6312f7`; `dotagents` `852e467`; `react-agents` `9cb5ea3`; #637)

- **20 docs, 20 routers, 2 plugins.** `standards/dotnet/` (12 docs across `data/`, `results/`,
  `structure/`, `testing/`) and `standards/react/` (8), mirroring the generic tiers path for path so a
  local doc and its counterpart pair up. `api/agents/` and `app/agents/` are deleted. `api/ARCHITECTURE.md`
  went 242 → 62 lines, keeping only the folder layout the polyrepo cut removes anyway.
- **Blast radius all repointed:** 20 integration-test `AGENTS.md` stubs that `@`-imported
  `INTEGRATION_CONVENTIONS.md`, `api/AGENTS.md`'s three imports and its duplicate copy of the
  forbidden-table list, `app/AGENTS.md`'s two, root `AGENTS.md`, `docs/INDEX.md`, the `review` and
  `e2e-api-debug` skills, three service `ARCHITECTURE.md` files, four `TECH_DEBT.md` files, two tier
  preambles. `skill-routes.json` now pairs every generic route with its local counterpart and gained rows
  for build config, migrations and the tenant feature.
- **Three defects found by executing it, not by reading it:**
  1. The hook was copied into **every** plugin. Fine with one; with three, installing them all registers
     the same `PreToolUse` matcher three times and the router fires three times per write. `payloads.json`
     now names exactly one hook owner, cross-checked against `marketplace.json`.
  2. Pruning never covered `plugins/<p>/hooks`, so the stale copies survived the fix that stopped
     generating them — the same class of bug pruning-by-generated-set-membership was added to prevent.
  3. **`deploy-skills.ps1` junctioned per domain onto one flat `~/.agents/standards/<domain>`.** Correct
     while each domain lived in one repo; wrong the moment a generic domain and its counterpart share a
     name on purpose. Eight paths collide, and it failed **silently**: `concertable-persistence` opened
     dotagents' generic `PERSISTENCE.md` — right path, wrong repo, no error. Standards now deploy at
     `~/.agents/standards/<repo>/<domain>`; skills stay flat because discovery does not recurse, standards
     do not because they are only opened by path. Plugin delivery was never affected — each plugin carries
     its own subtree.
- **Verified rather than assumed:** both new generator guards negative-tested, 37 hook tests pass, 87
  generated files current, 27 plugin routers resolve inside their own plugin, and all **112** deployed
  routers (56 × two harness roots) open a byte-identical copy of their own repo's authored doc.
- **Cost, stated:** 20 more always-on router descriptions, roughly the ~5,242 tokens measured for
  dotagents' 20 again. Forced by the design — the generator refuses a doc with no router.

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
  `standards/react/` (9) and `standards/communication/` (2, since removed). Every skill that owns a doc is now eight
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

**The hook-ownership commits are reviewed** (`890c68d2..16230d76`, 3 commits, 20 files). Verified first
rather than assumed: all six vendored copies hash byte-identical to the commits `vendored.json` records,
every mechanism test deleted here exists upstream where CI runs it, and `CLAUDE_PLUGIN_ROOT` is the
variable both harnesses substitute, so the launcher's plugin guard is not Claude-only. **Five findings;
four fixed on the branch at `fcec9925`, ENF12 open because only a merge can close it.**

- **ENF8 (HIGH)** — the merge gate resolves a PR's own head so a worktree PR merges correctly from any
  checkout, then its security layer diffed the literal `HEAD`. Shown live: PR #637 judged from the main
  checkout classified **0 files**, found nothing sensitive, and never asked for the security marker, where
  its own worktree classifies 16 paths. The layer failed open on exactly the flow the gate supports.
- **ENF9 (MEDIUM)** — security-marker currency was `marker == head` with only a `reviews/`-only exemption,
  while `review` Step 1d re-stamps it only for a sensitive range. No legal exit, so the marker was being
  hand-moved; it now asks whether a security-sensitive path changed since the marker.
- **ENF10 (MEDIUM)** — the wiring test's new "wired nowhere is fine" escape legalized a harness-fired hook
  losing both wirings. `vendor-hooks.ps1` now records each hook's `delivery`, derived from its own
  `hooks.json`, and the test asserts both directions; unwiring `skill_router.py` from both files fails.
- **ENF11 (MEDIUM)** — no workflow here ran the vendoring guard at all, so six generated copies were
  protected by memory. `test.yml` gains `hook-tests` and `ci-complete` needs it.
- **ENF12 (LOW, open)** — every `commit` in `vendored.json` resolves only to `agent-standards`
  `Refactor/StandardsDomainTree`. Squash-merging #2 deletes that branch and the provenance with it. Close
  it by landing #2 (merge commit, or re-vendor against `main` afterwards and commit the re-pinned
  manifest).

ENF8/ENF9 are authored in `agent-standards` `32795d8` with six new tests; the one that catches a return to
the literal `HEAD` was mutation-checked. Upstream 151 tests pass, this repo's 14 pass, `docs_reachability`
and `plan_graph` 0 errors.

## Topology — four tiers, settled 2026-08-18

**Authoritative statement: `dotagents/AGENTS.md` (mirrors to `~/AGENTS.md`, so it loads every session) and
`dotagents/ARCHITECTURE.md`. The plan's "target structure — four tiers" section is the file-by-file version.
This ledger does not restate the model — it records only what is not yet true of it. As of 2026-08-19 the
file trees match it; what is left is the process corpus still living in two places (Next Step 1).**

| Tier | Repo | Status |
|---|---|---|
| Generic .NET | `tomjseery/dotagents` | 20 docs in `standards/dotnet/` — correct |
| Generic React/TS | `tomjseery/react-agents` | created 2026-08-18 — 9 docs in `standards/react/`, moved out of `dotagents` |
| Concertable | `Concertable/agent-standards` | all three sections built 2026-08-19 — `process/` 7, `dotnet/` 12, `react/` 8. `api/agents/` and `app/agents/` are deleted |
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

**The four tiers match the model and the process corpus now has one home.** Authoritative statement:
`dotagents/AGENTS.md` (mirrors to `~/AGENTS.md`, so it loads every session) and
`dotagents/ARCHITECTURE.md`; the file-by-file target is the plan's "target structure — four tiers" section.
Three superseded models were purged — do not reintroduce a merged standards repo, `dotagents` holding both
stacks, or a `platform/`/`concertable/` folder.

**Repo heads at handoff:** `dotagents` `82e5d87` (#1), `react-agents` `1ebb42b` (`main`),
`agent-standards` `0d773bd` (#2), Concertable #637 at the head this ledger commit creates, all pushed
2026-08-19. Nothing is merged.

**Deployed layout on this machine:** `~/.agents/standards/<repo>/<domain>/` — repo-scoped, because a
generic domain and its Concertable counterpart share a relative path on purpose. Skills stay flat in
`~/.agents/skills` and `~/.claude/skills` (**69** of them, after `domain-events`,
`concertable-domain-events` and `concertable-multitenancy`). `deploy-skills.ps1` in `dotagents` owns both.

Audit findings: **all closed**; `reviews/Docs-GuidanceDocsRestructure-AuditFindings.md` is deleted.

**The three `agent-standards` `GAP` rows are resolved.** `testing/UNIT.md` is written +
`concertable-unit-testing`: the tier gate in `api/TestConventions.targets` is real Concertable content,
and writing it down settled the Shouldly open call — the build already fails a unit project that
references it, so it was never actually open. `results/ERRORS.md` and `testing/E2E.md` are **deleted from
the plan rather than filled**, each by the corpus's own rules: a roster of the 75 error unions rots on the
next feature and the per-module definition-contract tests enforce their shape already, and Concertable's
E2E conventions live with the harness they drive in `Concertable.Testing.E2E`, which `docs/INDEX.md`
already names as the owner.

`react/APP_TIERS.md` and `react/BROWSER_STORAGE.md` are still named in the plan's Tier 3 block but their
sources sit outside the moved lines, so they stay with the rest of 3c.

### Done on 2026-08-19, after the handoff above

**5c's four undocumented live concepts are closed**, each verified against the code before it was written:

- **Domain events** — generic `dotagents/standards/dotnet/structure/DOMAIN_EVENTS.md` + `domain-events`,
  and Concertable's roster in `agent-standards/standards/dotnet/structure/DOMAIN_EVENTS.md` +
  `concertable-domain-events`. Measured, not assumed: 13 domain events, 13 handlers, **all 13
  pre-commit**, none doing anything but one `bus.PublishAsync`. `SeedingDomainEventDispatchInterceptor`
  runs pre-commit handlers in `SavedChangesAsync` while a seeding scope is active — the doc records the
  behaviour and says outright that no reason for it is written anywhere, rather than inventing one.
- **`ExcludeFromMigrations`** — the rule in generic `data/PERSISTENCE.md`; the roster (outbox and inbox
  from `DbContextBase`, plus Concert borrowing the two rating projections) in `agent-standards`'.
- **`CallCredentials`** — the rule in generic `structure/PROTO.md`; `AddPaymentClient`'s five stubs and
  their shared `payment:write` scope in `agent-standards`' `structure/SERVICE_BOUNDARIES.md`.
- **`concertable-multitenancy`** — the routing hole, not a missing doc. B2B's stance roster stays in
  `api/Concertable.B2B/CODE_PATTERNS.md` (it travels with the service); the new
  `agent-standards/standards/dotnet/data/MULTITENANCY.md` carries what is platform truth instead — that
  of the five services **only B2B has a tenant stance**, and which project owns each piece — and points
  at B2B's doc for the per-module table. Routes added: `/Events/`, `DbContext.cs`,
  `Configurations/*Configuration.cs`, and `concertable-multitenancy` onto `Repository.cs`, which
  previously carried the generic `multitenancy` skill alone.

**The two in-scope doc defects are fixed** — six `CLAUDE.md`-for-content citations repointed at the file
that owns the words, and `app/shared/AGENTS.md` reduced from restating `react/SHARED_CODE.md` and
`react/IDENTITY.md` to its own inventory.

**Five of 3c's rows are taken** — the ones whose disposition the plan had already settled, and every one
was carrying a wrong claim rather than merely sitting in the wrong place:

- **`docs/OVERVIEW.md`** — the best product primer in the repo, orphaned, so nothing had checked it. It
  called the money terms a **Contract** (which is now the signed PDF, so the old wording actively
  misleads), said Versus pays "the max of the two, or guarantee + split depending on type" when
  `VersusSettlementAmount` is unambiguously guarantee **plus** the percent, described `PaymentMethod` as
  upfront-vs-settled when it is `Cash`/`Transfer`, told the reader to go read `CalculateArtistShare`
  which does not exist, listed four B2B modules that are services or a library now while missing Deal,
  Conversations and Tenant, and gave mobile three apps. Rewritten against the code; linked from root
  `AGENTS.md`.
- **`docs/USP.md`, `docs/DEEP_RESEARCH_PROMPT_GUIDE.md`** — linked from `docs/INDEX.md`; no longer orphans.
- **`app/README.md`** — was still the untouched Vite scaffold. Now the real inventory: 12 workspaces, six
  published, and the two `error`-severity dependency-cruiser rules — including
  `cross-platform-b2b-has-no-platform-dependencies`, which no doc mentioned.
- **`api/docs/VS_TEST_EXPLORER_TROUBLESHOOTING.md`** — deleted. Not a duplicate of the
  `reset-test-explorer` skill but a *contradicting* one: it told you to CLI-rebuild the project, the one
  thing the skill says never to do (VS only registers a container it built itself). Its two unique facts
  moved into the skill. Both files had cited `api/Tests/`, which does not exist. `api/docs/` now holds
  only `MICROSERVICES_ARCHITECTURE.md`, which four service `ARCHITECTURE.md` files link on purpose.
- **`README.md`** — still said Auth was the first service on feed packages and the rest built from
  source. Phase 2 recorded this row as done; it was not.

**Found while doing 3c, and left alone deliberately:** `app/b2b/shared` (`@concertable/b2b`, 25 files
including a second `features/tenant`) is a **sixth** sharing tier that `app/AGENTS.md`'s tier table does
not list, and nothing imports it yet. It is mid-cut-over under `plans/platform/B2B_PACKAGE_TOPOLOGY_PLAN.md`
with its own live worktree, so the tier map is that plan's to settle — `app/README.md` states the
workspace inventory as fact without asserting a tier story. `notes/Concert-Rust-Analysis.md` (444 lines)
and the `api/docs/`-as-a-tree collapse both stay open: the plan leaves each a delete-or-keep decision, and
neither is loaded anywhere, so neither costs anything while it waits.

**Four more citations of the deleted `api/agents/*`, all missed because they are not markdown.** The
audit swept backticked identifiers in docs; these are machine-read payloads:

- `api/BannedSymbols.txt` (×2) — the RS0030 message shown at the moment a developer reaches for
  `IgnoreQueryFilters` pointed at `api/agents/CODE_PATTERNS.md`.
- `.editorconfig` (×2) — the CA1848 and private-field comments.
- `.github/workflows/claude-review.yml` — worse than a dead link: it called the system a modular
  monolith, cited `api/agents/MODULAR_MONOLITH_RULES.md`, and restated five rules inline, one of which
  ("module services save their own context, never `IUnitOfWork`") is the **opposite** of the current
  persistence standard, so the automated PR reviewer was flagging conforming code. It now reads
  `skill-routes.json` and loads the pairs for the changed paths.

**`ICurrentTenant` does not exist.** `api/AGENTS.md` (auto-loaded), `agent-standards`'
`react/IDENTITY.md` and the `concertable-identity` description all named it; the real pair is
`ICurrentUser` + `ITenantContext`, both `Concertable.Kernel.Identity`. The claim it illustrated —
referenced by B2B, not Customer — is exactly true of `ITenantContext` (61 B2B files, 0 Customer).

**Three upstream fixes the review pass produced**, because enforcement moves with the rule it enforces:
`agent-standards` `0d773bd` removed `common_git_dir()`, defined and called from nowhere while its comment
described a hazard the live code handles elsewhere; `9795896` corrected two overclaims in the new docs
(an `ITenantContext` count stated as "all B2B's" when 61 of 64 are, and an unverified claim about
Customer's scoping); `dotagents` `82e5d87` took two real Concertable identifiers out of a generic
standard's examples.

**A merge-gate defect found live and fixed upstream** (`agent-standards` `2231bd0`, +4 tests,
mutation-checked). `gh pr merge <n> --repo <other>/<repo>` never read the flag, so `gh pr view <n>`
resolved `<n>` against the local checkout: merging `agent-standards` #2 from a Concertable session
blocked on `reviews/TS-ReviewRefactor.md`, a merged and unrelated Concertable PR. Both directions are
wrong and the second is dangerous — a sibling merge **passes** whenever the local PR of the same number
carries a clean review. Same class as ENF8. The flag now decides jurisdiction the way the target
directory already did.

1. **What is left of the two phases.** 5c: `dotnet/STACK.md` and the five `react` `NEW` rows
   (`ROUTING.md`, `UI.md`, `TABLES.md`, `DATES.md`, `TESTING.md`) — all in the two generic repos, none in
   Concertable. 3c:
   `app/web/shared/BROWSER_STORAGE.md` → `react/BROWSER_STORAGE.md`, `react/APP_TIERS.md`, and the two
   rows above that need a decision rather than work (`notes/Concert-Rust-Analysis.md`, collapsing
   `api/docs/`). The tier-map row for `app/b2b/shared` belongs to `B2B_PACKAGE_TOPOLOGY_PLAN`, not here.

2. **#637 is re-reviewed and both markers are current; it waits on ENF12 and then on Tommy.**
   `/incremental-review` ran over `6d80032e..9799ac91` — native layer plus the security layer, which the
   `.github/workflows/**` change obliged. **Security: nothing at the bar. Native: five findings, all
   fixed on the branch**, plus four fixed upstream. The one open `- [ ]` in the review file is still
   ENF12, which only the upstream merge closes. It is **not** a docs-only PR — it carries
   `api/TestConventions.targets`, the CI job and the hooks — so the docs-only auto-merge path does not
   apply. Merging will trigger publish + platform sync (`paths: api/**` matches this branch's `api/**`
   markdown); follow the `chore/platform-sync-*` PR to green.

3. **Landing `agent-standards` #2 and `dotagents` #1 needs Tommy** — both `CLEAN` with `verify` green,
   and the work is done, but this session could not merge them: the Bash gate that fires is the *stale*
   copy on `main` (which #637 replaces), and the GitHub API path was refused by the sandbox classifier.
   #2 is a prerequisite, not tidiness — Concertable's six hooks are vendored from it and their
   provenance pins resolve only on its branch (ENF12).

   ```bash
   gh pr merge 2 --repo Concertable/agent-standards --merge   # merge commit: keeps the vendored SHAs reachable
   gh pr merge 1 --repo tomjseery/dotagents --merge
   ```

   Then re-run `pwsh .agents/vendor-hooks.ps1 -Into <worktree>` from a checkout on `main`, commit the
   re-pinned `vendored.json` (it also picks up the `--repo` jurisdiction fix), tick ENF12, and re-stamp
   the markers.


**Tommy's, not agent work:**

- **`draft-comment` and `explaining-code` are deployed nowhere.** Their docs were removed from `dotagents`
  on 2026-08-19 as not that repo's to hold (`dotagents` `a7e9b40`; text recoverable at `1389a2e^`). His
  (`attach-pr-screenshots`, `create-devops-item`, `create-gh-pr`, `implement`, `ship`, `verify`,
  wired up", not "does not exist". **Needs Tommy to say which repo owns them.**
- Approve the Codex `PreToolUse` hook once in a Codex session in this worktree.
- Archive `agent-starter-kit`.
- Rule on `GenreController` in a shared library (`api/Concertable.Shared/TECH_DEBT.md:70`).
- Decide whether React Hook Form is adopted (in no `app/` workspace today).

**Operational note for whoever picks this up:** the Bash tool truncates a long command, and a heredoc that
gets cut mid-body fails as `unexpected EOF while looking for matching quote` rather than as a length error.
Two doc-writing batches died that way. Write the file with the Write tool, or put a generator in a `.py`
file and run that — do not push long content through a heredoc.


## Also Tommy's, not blocking

`tomjseery/agent-starter-kit` (public, 7 skills) looks redundant with `tomjseery/dotagents` (private, the
same 7 plus `pull-main`, `sync-all`, `unmerged`) — the same duplication disease at repo level.
