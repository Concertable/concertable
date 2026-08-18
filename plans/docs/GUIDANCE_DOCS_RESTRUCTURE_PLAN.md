# Guidance-docs restructure — plan

Reorganize the guidance corpus (root `AGENTS.md`, `api/agents/*`, `app/agents/*`, `docs/*`,
`api/docs/*`) so that every rule has exactly one home, the auto-loaded weight is proportionate, and the
generic half lives in a separate repo, mounted at a fixed path, so carving a service out of this
monorepo rewrites no imports.

All line references verified against `origin/main` at `dc037f477`.

Non-goal here: the extraction itself, and the analyzer push-down. Both are follow-up PRs.

## What is already solved — do not re-propose it

`origin/main` fixed the discovery problem while this analysis was being written:

- `app/AGENTS.md` and `app/mobile/AGENTS.md` now exist; root `AGENTS.md:46–62` is a real per-area index.
- Root `AGENTS.md:50` states a reachability rule — every `AGENTS.md` needs a `CLAUDE.md` sibling
  containing exactly `@AGENTS.md`, and every `*/agents/*.md` must be transitively reachable from some
  `AGENTS.md`/`CLAUDE.md`/`SKILL.md`. **Mechanically enforced** by `.agents/hooks/docs_reachability.py`,
  run as part of `docs-review`. It currently reports 0 errors.

So the corpus is now *loaded*. It is not *organized*, and nothing checks that it is.

## The gap: three properties, only one of them automated

`docs_reachability.py` (135 lines) does exactly two things — verifies the `CLAUDE.md` sibling
(`:68–83`) and BFS-reachability of `agents/*.md` (`:100–109`). What it does **not** do, verified by
reading it:

- **It never tests that a link target exists.** `resolve_reference` (`:47`) strips anchors and skips
  `http`/`mailto`; the only `is_file()` call in the file (`:72`) is the `CLAUDE.md` sibling check. A link
  to a deleted file still counts as an edge and still marks the target "reachable".
- **Its orphan check is scoped to `agents/*.md` only** (`:31–37`), one level deep. `docs/**`,
  `api/docs/**`, `plans/**`, `README*`, `PROMPTS.md` are outside its search space entirely — which is
  why it is green while five docs are unreachable.
- **Root-absolute links create bogus edges.** `/api/docs/MM_NORTH_STAR.md` resolves against drive-root,
  not repo-root. `api/agents/CONVENTIONS.md:3` and `:106` use exactly that form.

And the only review that could catch duplication is scoped out of it: `docs-review` Lens B/C are prose
lenses over a **diff**, and Step 4 explicitly instructs *"Discard: pre-existing issues on unchanged
lines"* (`.agents/skills/docs-review/SKILL.md:139–140`). A contradiction that already exists on
unchanged lines is structurally invisible to the process meant to find it.

**Reachable ≠ correct ≠ non-duplicated.** One is automated; the other two are not checked by anything.

## Ten live contradictions

Each verified against current file contents.

| # | Conflict | Resolution |
|---|---|---|
| 1 | `api/agents/DEBUGGING_CONVENTIONS.md:5` — "leave it as an inline `logger.Log*` call" vs `CODE_CONVENTIONS.md:396` "**No** inline `logger.LogInformation/…`" and `api/AGENTS.md:3` "**never**" | **Settled by evidence.** `.editorconfig:17` sets `CA1848` to error for all `[*.cs]`, exempt only under Migrations; grep finds **0** inline calls across `api/**`. The debugging doc instructs code that does not compile — and it is the doc `e2e-ui-debug:222` / `e2e-api-debug:194` tell you to read first |
| 2 | Root `AGENTS.md:227`, `:241`×2, `:244` cite `./e2e.ps1` / `./docker-health.ps1` vs `E2E_CONVENTIONS.md:29`, `:36` citing `./scripts/e2e.ps1` | **Settled.** Neither exists at root; only `scripts/` does, and `scripts/e2e.ps1` prints `./scripts/e2e.ps1` in its own usage. 4 wrong lines in the always-loaded file, plus ~32 in the four e2e skills, against 5 correct |
| 3 | Root `AGENTS.md:125–127` "exactly ONE of **three** terminal states" vs `:129` "The **four** outcomes:" with four items listed and exits 0/2/3/4 | **Settled** — the preamble wasn't updated when the DIRTY state was inserted |
| 4 | `Monitor`: root `AGENTS.md:123` (heading) + `:157` "**Never use the `Monitor` tool**" vs `merge/SKILL.md:111` "**Prefer the `Monitor` tool**" and `:229` "prefer the `Monitor` tool over busy-waiting" | **Resolved:** rule stays in root — always loaded, and it carries the rationale — and `merge` now defers to it. Moving it into the skill would have traded correctness for token savings |
| 5 | Branch currency: root `AGENTS.md:102–107` "**Update first, then enable — always**… mandatory" and `merge:100–108` "non-negotiable" vs `pr-preflight:25–27` "still mergeable (the queue rebases it)" and `:63–68` "soft… not fatal" | **Resolved: mandatory.** `pr-preflight` is now a hard gate. The cost is asymmetric — being wrong means merging code never built against current `main`, with a possibly stale platform pin |
| 6 | Notification is an adapter **service** (`api/ARCHITECTURE.md:34`, `api/AGENTS.md:11`, `MICROSERVICES_NORTH_STAR.md:22`, `:50`) vs a shared **library** (`MICROSERVICES_ARCHITECTURE.md:186–206`, decided 2026-05-19) | **Settled by code.** There is no Notification service or host — only `api/Concertable.Shared/src/Concertable.Shared.Notification`, a library. Nothing can `WaitFor` it. Both auto-loaded docs are wrong |
| 7 | `MICROSERVICES_ARCHITECTURE.md:246` lists "gRPC between services" under **Disallowed**, repeated `:461`, `:492` vs `api/ARCHITECTURE.md:35` "may call them synchronously (gRPC)", `api/AGENTS.md:24` (B2B fronts Payment over gRPC), and `Concertable.Grpc` existing | **Settled by code** — the design record is stale |
| 8 | `CODE_CONVENTIONS.md:292–297` "do **not** add Result-pattern rules here" vs `:299–328`, a Result-payload naming rule duplicating `RESULT_PATTERN.md:574–585` | **Settled** — self-contradiction two lines apart |
| 9 | `api/AGENTS.md:69–73` mandates `XDetailsResponse` "even when it is currently a field-for-field clone" vs `CODE_CONVENTIONS.md:301` / `RESULT_PATTERN.md:581` gating `Response` on the wire "genuinely differ[ing]" | **Resolved:** `RESULT_PATTERN` now points at `CODE_CONVENTIONS` for DTO naming, which points at `api/AGENTS.md` where the carve-out lives. One home, one path to the exception |
| 10 | `app/web/AGENTS.md:26` "never import `AxiosError`/`isAxiosError`" vs `app/agents/CODE_CONVENTIONS.md:206` and `CODE_PATTERNS.md:191`, which allow `isAxiosError(e) && status === 401` in a route guard | **Resolved by code, opposite to the first guess:** guards use `isApiError` from the shared seam and `isAxiosError` appears only in the shared client/interceptor, so `app/web/AGENTS.md` was right and both `app/agents` docs were stale |

Plus a looser-predecessor pair that isn't opposing but is exploitable: `CODE_CONVENTIONS.md:192–194`
("Only add a comment when the WHY is non-obvious" — no ≤2-line cap, no disqualifiers) sits directly
above `:196–200`, which correctly delegates to root. An agent can cite `:192` to justify a comment root
policy bans.

## Duplication with drift

The full tables are in the analysis; these are the ones that matter.

| Rule | Copies | State |
|---|---|---|
| Never seed handler-written rows | `api/AGENTS.md:26–45`, `SEEDING_CONVENTIONS.md:3–11` + `:45–57`, `api/ARCHITECTURE.md:63`, `INTEGRATION_CONVENTIONS.md:71`, `E2E_CONVENTIONS.md:17` | **Drifting.** `api/AGENTS.md:28` says "Not the summary below — the full file. Every time." then gives a 17-line summary. `SEEDING_CONVENTIONS.md:54–55` has invitation rows/memberships that `api/AGENTS.md` lacks |
| Migrations | `api/AGENTS.md:47–56`, `api/agents/CONVENTIONS.md:139–142` | **Drift.** "every **module's**" vs "every **context's**"; the CONVENTIONS copy drops the load-bearing "never a cost to weigh" clause and ends with a pathless "See CLAUDE.md" |
| DTO / `Response` naming | `api/AGENTS.md:58–87`, `CODE_CONVENTIONS.md:299–328`, `RESULT_PATTERN.md:574–585` | 3 copies, 2 drift — see contradictions 8 and 9 |
| Payment owns no seed catalog | `api/AGENTS.md:14`, `api/ARCHITECTURE.md:49–53` + `:88–103`, `SEEDING_CONVENTIONS.md:36` + `:71–81` | Agree, 4 copies |
| Docker health pre-flight | root `AGENTS.md:213–244` + 4 e2e skills | Agree, **5 near-verbatim copies**, all with the wrong path. `integration-debug:58` states a *weaker* floor with no data-round-trip gate |
| Plan handoff pointer | `PROMPTS.md:3–13`, `plans/AGENTS.md:88–104`, `plans/agents/PLAN.md:287–309`, root `AGENTS.md:277–283`, + `plan_handoff_stop.py` | 4 prose copies + a hook. `plans/AGENTS.md:88–100` paraphrases `PROMPTS.md:3–13` *and* links it |
| Branch prefix | root `AGENTS.md:85` allows `Feature/`,`Refactor/`,`Bug/`,`Fix/` vs `plans/AGENTS.md:124` hardcodes `Feature/<Name>`; `commit/SKILL.md:43` adds "unless the user explicitly says otherwise" to root's absolute rule | Narrowing drift both ways |
| App-side near-verbatim double-writes | `app/agents/CODE_CONVENTIONS.md:194`/`CODE_PATTERNS.md:177` (error seam), `:360`/`:303` (zod boundary), `:217`/`:264` (`xApi`), `:237`/`:274` (axios), `:285`/`:216` (hook tiers) | 5 sections written twice — **and both files are `@`-imported**, so each rule enters context twice |

The app-side axios copy is not merely redundant, it is wrong: `app/agents/CODE_PATTERNS.md:276` cites
`web/shared/lib/axios.tsx` and `b2b/shared/lib/b2bAxios.ts`; neither exists. The real files are
`app/shared/src/lib/apiClient.ts`, `paymentClient.ts`, `searchClient.ts` and
`app/web/b2b/shared/src/lib/b2bClient.ts` — exactly as `CODE_CONVENTIONS.md:249` says. Both versions
load every prompt.

## Auto-loaded weight

`@`-import is the only auto-load mechanism.

| Scope | Lines |
|---|---|
| Always (repo root) | **302** — `CLAUDE.md` 1 + `AGENTS.md` 300 |
| On touching any `api/**` file | **+1,429** — `api/CLAUDE.md` 1 + `api/AGENTS.md` 97 + `CODE_CONVENTIONS.md` 418 + `RESULT_PATTERN.md` 620 + `CODE_PATTERNS.md` 293 |
| On touching `app/**` | **+786** — `app/AGENTS.md` 34 + `CODE_CONVENTIONS.md` 389 + `CODE_PATTERNS.md` 363 |
| Peak, inside an integration-test project | **~1,881** |
| Plain-linked pool, loaded only on explicit read | **560** — `api/ARCHITECTURE.md` 219, `CONVENTIONS.md` 142, `SEEDING_CONVENTIONS.md` 113, `MICROSERVICE_COMMUNICATION.md` 81, `DEBUGGING_CONVENTIONS.md` 5 |

Two things fall out of this table:

- `RESULT_PATTERN.md` alone (620) is more than double the always-loaded root file, and every `api/**`
  prompt pays for it — including its package pins, gRPC transport rules and CI-carve requirements.
- **`SEEDING_CONVENTIONS.md` is not auto-loaded — which is precisely why `api/AGENTS.md:26–45`
  re-summarizes it inline.** The duplication is a symptom of the load mechanism, not carelessness. The
  fix is to choose: import it, or cut the summary to a pointer. Not both.

The app side shows the same trade made the other way: orphan-ness was fixed by `@`-importing 752 lines
that contain 5 duplicated sections. Reachability was bought with weight rather than with dedupe.

## Prose that re-argues what a machine already enforces

| Rule | Prose | Enforcer |
|---|---|---|
| `CODE_PATTERNS.md:7` tenancy composition | 61 lines | `RS0030` = error (`.editorconfig:20`) + `api/BannedSymbols.txt`; the doc admits it at `:10–12`. The other 55 lines are the unenforced stance taxonomy |
| `CODE_CONVENTIONS.md:396` logging | 8 lines, stated a 3rd time at `api/AGENTS.md:3` | `CA1848` = error, build-enforced, 0 violations |
| `CODE_PATTERNS.md:90` keyed strategies | 90 lines | `DealStrategyArchitectureTests.cs` (4 tests) + composition-time `RequireAll`/`RequireExactly` |
| `RESULT_PATTERN.md:605` "Do not introduce" + `:81` boundary rules | 36 lines | `ReunionArchitectureTests.cs`, `TypedResultArchitectureTests.cs` (8 tests) |
| `CODE_CONVENTIONS.md:116` read-context singularity | 15 lines | `RepositoryArchitectureTests.cs` (2 tests) |
| Root `AGENTS.md:213` Docker health | **32 always-loaded lines** | `scripts/docker-health.ps1`, run as an automatic gate by `scripts/e2e.ps1` — `:241` says so itself |
| Root `AGENTS.md:102` + `:123` merge | **86 always-loaded lines, 2 bash blocks** | The `/merge` skill; `:107` and `:202` both say "`/merge` does it for you" |

Two asymmetries worth fixing while here:

- **`MA0053` sealing and file-scoped namespaces are enforced at error severity but documented in no
  prose file** — only in `.editorconfig` comments. Enforcement without a written rule.
- **Several `severity = error` style rules are IDE-only.** There is no `EnforceCodeStyleInBuild`
  property anywhere under `api/**`, so the private-field naming rule (`.editorconfig:31–33`),
  `IDE0130` (`:5`) and file-scoped namespaces (`:4`) do **not** fail a build. The 27 lines of prose at
  `CODE_CONVENTIONS.md:5–31` are therefore doing real work, and the `error` severity oversells itself.
  Setting `EnforceCodeStyleInBuild` would let that prose shrink to a line.

## Dangling and misdirected references

| Source | Target | Status |
|---|---|---|
| root `AGENTS.md:227`, `:241`×2, `:244` | `./e2e.ps1`, `./docker-health.ps1` | Absent at root |
| root `AGENTS.md:275` | `plans/AGENTS.md` "Companion progress ledger" | Section is in `plans/agents/PLAN.md:17` |
| `api/ARCHITECTURE.md:95`, `:219` | `plans/PAYMENT_AGNOSTIC_AUDIT.md` | Absent |
| `api/ARCHITECTURE.md:191` | `plans/PLATFORM_PACKAGE_SYNC.md` | Absent |
| `api/agents/CONVENTIONS.md:55`, `:60` | `feedback_module_impl_visibility_cascade.md`, `feedback_module_facade_surface.md` | Absent repo-wide |
| `.agents/skills/review/SKILL.md:128`, `:151` | `api/agents/MODULAR_MONOLITH_RULES.md` | **Renamed** in `7908c97ed` — rename collateral, breaks a skill lens |
| `api/agents/CONVENTIONS.md:142` | "See CLAUDE.md" | Pathless; that file is one `@AGENTS.md` line |
| `CODE_PATTERNS.md:260` | "the rule in `api/CLAUDE.md`" | Section is `api/AGENTS.md:16` |
| `CODE_CONVENTIONS.md:196` heading | "policy in root `CLAUDE.md`" | Heading and its own link (`:198` → `../../AGENTS.md`) disagree |
| `E2E_CONVENTIONS.md:29` | `E2E_BASELINE.md` | Bare name; only resolvable at `api/Concertable.Shared/tests/Concertable.Testing.E2E/` |
| `app/agents/CODE_PATTERNS.md:276` + 4 more | `axios.tsx`, `b2bAxios.ts`, `tenantChoice.ts`, a missing `schemas/` folder, the auth persona union | All absent or already fixed |
| `api/agents/CONVENTIONS.md:3`, `:106` | `/api/docs/MM_NORTH_STAR.md` | Root-absolute — renders broken, and fools the reachability hook |

None of these are caught by anything today, because the hook does not check target existence.

## The naming collision

`MODULAR_MONOLITH_RULES.md` → `api/agents/CONVENTIONS.md` (in `7908c97ed`) put two files named
`CONVENTIONS.md` and `CODE_CONVENTIONS.md` side by side, and dropped the only word that told them
apart. `api/AGENTS.md:97` rendered as a bare `See [CONVENTIONS.md](./agents/CONVENTIONS.md)` under
`## Module rules` — the filename gives no hint it is layer/project topology, and "CONVENTIONS" reads as
the *superset* of "CODE_CONVENTIONS", which is backwards.

The underlying three-way split is actually clean and worth keeping: **where code lives** (`CONVENTIONS.md`
— projects, layers, visibility, module boundaries) / **how it's named** (`CODE_CONVENTIONS.md`) /
**what structures to use** (`CODE_PATTERNS.md`). Only the name is wrong. `MODULE_STRUCTURE.md` fixes
both the collision and the stale "Applies to modules in the monolith" framing at `:6`/`:91`, which
contradicts `api/ARCHITECTURE.md:8` "LOCK THIS IN".

## The two obsolete architecture docs

Authority is already settled, inside the doc being demoted —
`api/docs/MICROSERVICES_ARCHITECTURE.md:509`: *"`api/ARCHITECTURE.md` is the canonical current-state
doc; this doc records the design and its decision history."*

- **`api/docs/MICROSERVICES_NORTH_STAR.md`** (83) claims at `:3` to be "**The canonical vision… Read
  this first**", is reachable only from a checked-off task line in a plan, and is contradicted four
  ways: 6 services / 11 hosts vs 5 / 9 (`:41`); "shared code lives in two csprojs only, no third
  package" (`:36`) vs the reversal in §4.8 and 11 shared projects on disk; "no NuGet feed maintenance"
  (`:56`) vs `api/ARCHITECTURE.md:105` "executed, not aspirational"; and the Notification error.
- **`api/docs/MM_NORTH_STAR.md`** (423) describes the pre-microservices monolith — in-process bus
  (`:335`), `SharedDbContext.Genres` (deleted per §4.6), `Modules/Identity` (doesn't exist), four
  missing plan citations. **It is one hop from an auto-loaded file**: `api/CLAUDE.md` → `api/AGENTS.md:97`
  → `api/agents/CONVENTIONS.md:3` and `:106`, which present it as the authority for *why*. Parts of its
  corollary text have been retrofitted to current reality, which makes it read as maintained.

## The organizing axes

Four axes are currently collapsed into a vague pair of filenames. Separate them and "where does this
rule go?" becomes a lookup.

| Axis | Values | Encoded by |
|---|---|---|
| **Scope** | universal · api-wide · one service · one project type | **which `AGENTS.md` `@`-imports the topic file** |
| **Genericity** | generic (belongs in the shared repo) · Concertable-specific | **repo** (`conventions/` submodule vs the consumer's own `agents/`) |
| **Kind** | style · naming · structure · testing · process | **filename — one topic per file** |
| **Audience** | human-consultable convention · agent-process instruction | `conventions/` vs `docs/process/` |

**Scope governs bloat, and a folder cannot express it — only an import edge can.** A topic file under
`api/conventions/` still loads for every service that touches `api/**`. The fix is granular topic files
plus per-consumer composition: the file exists once, generic, and only the `AGENTS.md` of a consumer
that actually needs it imports it.

The repo already does this in 42 files — every test project's `AGENTS.md` is a two-line stub that
`@`-imports exactly one of `UNIT_CONVENTIONS.md` / `INTEGRATION_CONVENTIONS.md`. Generalizing that
precedent to every topic is the model.

Measured scope violations in the currently always-loaded set:

| Topic | Lines | Consumers that can use it |
|---|---|---|
| `CODE_PATTERNS.md` "Tenancy is composed, never subtracted" | 62 | B2B only (`TenantScopedDbContext`, `VenueArtistTenantScopedDbContext`, `AdminDbContext`) |
| `CODE_PATTERNS.md` "Module-local keyed strategy factory" | 90 | B2B only (`IDealStrategyFactory`, `DealType`) |
| `RESULT_PATTERN.md` "gRPC boundaries" | 34 | Payment only |
| `CODE_CONVENTIONS.md` proto naming + proto mapper example | ~12 | Payment only — exactly one `.proto` exists in the repo |

~200 lines that four of the five services can never act on, on every `api/**` prompt. Genuinely api-wide
by contrast: `Log.cs` (7 services), `Schema.cs` (7), `IPagination` (6), `IGeometryProvider` (5),
`IUnitOfWorkBehavior` (3), `ValidationResult` (3).

Two distinct defects fall out, with different fixes — don't conflate them:

- **A generic rule carrying local examples** (`base.` qualification illustrated with `CurrentTenant`,
  `XMappers` illustrated with `EscrowDeposit`). The rule is portable; genericize the example.
- **A genuinely single-service topic** (tenancy composition, keyed `DealType` strategies, proto). The
  whole topic is scoped; it becomes its own file, imported only by that service.

**Corollary — generic topic files are exempt from doc locality.** Root `AGENTS.md` "Doc locality" places
a doc at the lowest node containing its concern; that governs *Concertable-specific* guidance. A generic
convention is not *about* any node, it is a library entry addressed by import rather than by position.
Locality still governs the thin local files that name precedents.

## The shared repo: `.agents/`-canonical skills

The monorepo is temporary, so the generic half must not live inside it — otherwise every future
carve-out is an import rewrite.

**The mechanism is not Nx** — Nx is a JS/TS task graph and build cache and distributes nothing to other
repos. Nor is it a Claude Code plugin marketplace *by itself*: take the idea from
not its Claude-only layout. **`.agents/skills/<topic>/SKILL.md` is canonical**, the same
agent-agnostic convention this repo and `dotagents` already use, and the Claude-side paths are generated
stubs:

```text
.agents/skills/<topic>/SKILL.md       the real instructions — one topic per skill
.agents/sync-claude-skill-stubs.ps1   regenerates every stub from them
.claude/skills/<topic>/SKILL.md       generated stub (Claude Code, repo-local)
plugins/<plugin>/skills/<topic>/      generated stub (Claude Code, installed plugin)
.claude-plugin/marketplace.json       the marketplace manifest
```

The skill inventory is in "Target structure" below — it is the live one, so it is not restated here.

Two consumption paths off one source, so nothing is Claude-only: a Claude Code repo installs the plugin
(`/plugin marketplace add <owner>/agent-standards`), while Codex and anything else reads `.agents/skills/`
directly — junctioned, cloned, or synced to `~/.agents/skills/`. **No submodule, no `submodules: true` in
CI, and no import path a carve-out would rewrite.**

**The `description` front-matter is the load-bearing part.** It is the router that decides whether the
well: *"…Use when writing, reviewing, or restructuring backend tests, adding tests for a new endpoint…"*.
A vague description means the skill silently never loads.

### The one thing a skill cannot do — and the tier that follows

A skill is load-on-demand, so it applies **only if it gets invoked**. That is the whole token win and
also the whole risk, and it decides where each rule belongs:

| Tier | Mechanism | Cost | Guarantee |
|---|---|---|---|
| Cross-project always-on | global `~/.claude/CLAUDE.md` (already exists) | every prompt, every repo | unconditional |
| Repo hard floor | that repo's `AGENTS.md` | every prompt in the repo | unconditional |
| Reference standard | plugin skill from `agent-standards` | ~one listing line until invoked | only when invoked |

**Sort by the cost of missing the rule, not by topic.** A rule whose violation is expensive and silent
stays in `AGENTS.md`: never seed handler-written rows, never `WaitFor` another data service, shared code
is the intersection, comments default to none. A rule you consult *while already doing the work* becomes
a skill, because the task itself is the trigger: proto mappers, result-pattern detail, testing shape,
tenancy composition, keyed strategies.

Convenient consequence: the hard floor is mostly Concertable-specific anyway (seed entity inventory,
service topology), so it was never a candidate for the shared repo. The tiers fall out cleanly.

### Per-service composition

Each service keeps a thin `CODE_CONVENTIONS.md` / `CODE_PATTERNS.md` that names the skills relevant to
it and carries its own precedents — the roster of real types the generic skill deliberately omits.

**Mechanical trap:** nested `AGENTS.md` files *compose* — the parent and the child both load. So a
service file must carry **only its extras**, never a copy of the api-wide baseline list. Five services
each restating the baseline is five copies that drift, which is the defect this whole plan exists to
remove.

## Target structure

A **skill is a convention doc with a trigger** — the same markdown, plus a `description` front-matter
that decides when it loads. So nothing is rewritten into a different genre; the doc is the payload and the
skill routes to it.

**Superseded 2026-08-18 — the shape below is what Phase 3a shipped, not the target.** Phase 3a's split was
made on *portability* (generic vs Concertable), which is why React ended up beside .NET and machine
utilities in one repo. The target is a single `standards` repo with domain trees (`dotnet/`, `react/`,
`process/`, `concertable/`, later `infra/`) and flat routing skills — see Phase 5. Both repos keep
`.agents/skills/` canonical and generate the Claude-side stubs from it, so nothing is Claude-only and
Codex reads the same files.

- **`tomjseery/dotagents` → `~/.agents/skills/`** — the 28 generic ones. They name no project, so they are
  personal and every repo on the machine has them with no per-repo install.
- **`Concertable/agent-standards`** — the 7 process ones, which are Concertable-shaped: the merge queue and
  its platform-sync PR, the draft-PR/queue tiering, ROADMAP→PLAN→PROGRESS and the worktree scripts. In the
  org so carve-out service repos inherit them.

```text
dotagents  .agents/skills/     -> ~/.agents/skills/    (personal, generic, 28)
  csharp-style/  csharp-naming/  comments/  dependency-injection/  logging/
  validation/  persistence/  result-carriers/  result-errors/  result-terminals/
  http-api/  module-structure/  microservice-boundaries/  seeding/
  unit-testing/  integration-testing/  e2e-scenarios/
  proto/            loads only when the task touches a .proto
  multitenancy/     loads only when the task touches tenant scoping
  keyed-strategies/ loads only when behaviour varies by a closed key
  typescript-style/  contract-naming/  react-structure/  server-state/  client-state/
  http-layer/  write-boundary/  tiered-shared-code/

agent-standards  .agents/skills/                       (Concertable process, 7)
  git-branching/  committing/  remote-validation/  merging/  plans/
  docs-and-debt/  failing-tests/
```

**A description is the router, and a bare colon-space breaks it.** An unquoted YAML scalar truncates at
`: `, so a description containing one leaves the skill with nothing to route on and it never loads — with
no visible error. Two migrated skills shipped that way; the stub generator now fails instead of emitting an
unroutable stub.

Two departures from the earlier sketch, both from applying the tier table above rather than the topic list:

- **`git-hygiene`/`merge-confirmation`/`branch-currency` became `git-branching`/`committing`/`merging`.**
  Branch currency is a pre-step of enabling auto-merge, not a topic of its own, so it belongs inside
  `merging` with the confirm loop it gates. Committing is a genuinely separate trigger from branching.
- **No `reviews` skill.** There is no generic review standard to migrate: `.agents/skills/review/SKILL.md`
  is lenses pointed at this repo's own docs, which is local by construction.

And two topics deliberately **not** migrated, because a load-on-demand skill is the wrong tier for them:
"questions come before actions" and "act on reversible work" are always-applicable behavioural rules whose
violation is silent — the task would have to summon the skill, and by then the miss has happened. They stay
in the global `~/.claude/CLAUDE.md`, as does the comment *policy*; only the C# mechanics of comments and
XML doc became a skill.

**Staying in Concertable:**

```text
AGENTS.md                       the repo hard floor + docs/INDEX.md pointer
docs/INDEX.md                   topic -> owner, incl. which skill owns which topic
api/AGENTS.md                   api-wide floor: seeding trigger rule, service topology,
                                shared-is-the-intersection, migrations
api/ARCHITECTURE.md             authoritative current state
api/Concertable.<Service>/
  CODE_CONVENTIONS.md           thin: this service's precedents + relevant skills
  CODE_PATTERNS.md              thin: same
  AGENTS.md / ARCHITECTURE.md   unchanged
app/  same shape
```

The per-service files carry the roster the generic skill deliberately omits — B2B's context roster and
filtered-entity list, Payment's Refit client roster and money conventions, the `DealType` families.

**Mechanical trap:** nested `AGENTS.md` files *compose* — parent and child both load. A service file
carries **only its extras**, never a copy of the api-wide floor, or five services drift five ways.

## The meta-rules

Landed in `docs/INDEX.md` (for this repo) and the `agent-standards` README (for the shared half). These
generalize what the repo already states in two places — `.agents/README.md` ("duplicated skill bodies
drift") and `RESULT_PATTERN.md:4` ("sole source of truth").

1. **One rule, one home.** Everywhere else links; never restates. A second copy is a bug.
2. **No file straddles the shared repo and the consumer repo.** A shared skill contains no Concertable
   identifier; concrete precedents live in the consumer's own `agents/` doc.
3. **If a machine enforces it, the doc gets one line and the diagnostic/test name.** Before writing a
   style rule, check whether `.editorconfig` or an architecture test can hold it.
4. **Headings are imperative rule statements, not topic labels.**
5. **≤15 lines per rule** — statement, anti-pattern, one example. Past ~80 lines it earns its own file;
   under ~20 lines a file merges into its parent.
6. **Never name violation sites in a rule doc.** They rot. Violations go in the owning `TECH_DEBT.md`.
7. **A doc is either `@`-imported or summarized — never both.** If a rule matters enough to restate
   inline, import it; if it doesn't, link it and cut the summary.
8. **Links are repo-relative.** No root-absolute `/api/...` paths.

## Phases

### Phase 1 — index and meta-rules, nothing moves — DONE
`docs/INDEX.md` mapping topic → owning file; the two `conventions/README.md` files. Reviewable against
the current tree.

### Phase 2 — factual fixes (separately committed) — DONE
Contradictions 1, 2, 3, 6, 7, 8 (evidence-settled) and 9; all 12 dangling/misdirected references
including `review/SKILL.md:128`, `:151`; delete `CODE_CONVENTIONS.md:192–194`; the five stale app-side
citations; the `./scripts/` path fixes across root `AGENTS.md` and the four e2e skills; the
`integration-debug` Docker floor.

### Phase 3a — move the corpus out to `.agents`-canonical skills — DONE
36 skills, split by whether the rule names this product: 29 generic to `dotagents` (`~/.agents/skills/`),
7 Concertable process ones to `Concertable/agent-standards`. Examples genericized off any single project's
domain; `proto`'s mapper and payload-naming sections collapsed to pointers rather than restating
`csharp-naming`.

### Phase 3b — reduce the in-repo half to the hard floor — DONE
`api/agents/*` 1,986 → 349 lines and `app/agents/*` 752 → 118, with every surviving file naming the skills
that own its generic half and carrying only this repo's roster. `UNIT_CONVENTIONS.md`,
`DEBUGGING_CONVENTIONS.md`, `E2E_CONVENTIONS.md` and `MICROSERVICE_COMMUNICATION.md` deleted, their local
remnants folded into `Concertable.Testing.E2E/AGENTS.md` and `api/ARCHITECTURE.md`;
`CONVENTIONS.md` → `MODULE_STRUCTURE.md` with the monolith framing fixed. B2B's scoped topics (context
stances, filtered entities, `DealType` families, workflow steps) moved to
`api/Concertable.B2B/CODE_PATTERNS.md`, imported only by B2B — so four of the five services stop paying for
them. 17 unit-test stubs and 6 E2E stubs now name their skill instead of importing a file.

One rule was **kept in repo rather than cut**: "one repository per entity", which landed on `main` mid-phase
and no skill owns yet. Cutting it would have deleted a live rule; it is a promotion candidate for
`persistence`.

### Phase 3c — the markdown outside the two conventions folders
10,011 lines of markdown live outside `plans/`, `reviews/`, `api/agents/` and `app/agents/`. Most of it is
domain knowledge already sitting in the right place — per-service `ARCHITECTURE.md`, `LEGAL_REQUIREMENTS.md`,
the `TECH_DEBT.md` set, `E2E_BASELINE.md`, `BROWSER_STORAGE.md` — and is not touched. What needs a decision:

| File | Lines | Disposition |
|---|---|---|
| `app/README.md` | 73 | Still the unmodified Vite scaffold, describing ESLint config the repo doesn't use. Replace with the tier map. |
| `notes/Concert-Rust-Analysis.md` | 444 | Orphan analysis, referenced by nothing. Delete per the throwaway-markdown rule, or give it a home. |
| `api/docs/MICROSERVICES_ARCHITECTURE.md` | 525 | Self-declared subordinate to `api/ARCHITECTURE.md`, stale-dated. Keep as dated history or fold and delete. |
| `api/docs/VS_TEST_EXPLORER_TROUBLESHOOTING.md` | 51 | Duplicate of the `reset-test-explorer` skill. One owner, one pointer. |
| `docs/USP.md`, `docs/DEEP_RESEARCH_PROMPT_GUIDE.md`, `docs/OVERVIEW.md` | 319 | All orphaned. `OVERVIEW.md` is the clearest "what is Concertable" anywhere and nothing links it — link it from root. |
| `api/docs/` as a tree | — | After 3b it holds only the two files above. Collapse it; two parallel doc trees in one service tree with no stated distinction is the original defect. |

**Settled (was: "settle before 3b edits root `AGENTS.md`").** The merge-confirmation loop exists in root
`AGENTS.md`, in `.agents/skills/merge/SKILL.md` which *automates* it, and in the `merging` standards skill.
The ruling: **the executable in-repo skill owns the procedure** — it knows this repo's real labels, workflows
and commands — while root `AGENTS.md` keeps only the invariants whose violation is expensive and silent (never
enable auto-merge on a stale branch; a failed check is a real failure, never retried or toggled; never
`Monitor`; whoever merges owns the platform-sync PR), one line each pointing at the skill. The `merging`
standards skill stays as the *generic* standard for carve-out repos that will not have this repo's `merge`
skill, and must be reduced to the rule without the loop body. Same shape for `pr-preflight` and `review`.

3b did **not** need this ruling after all: it never edited root `AGENTS.md`'s merge or Docker blocks, because
the plan already defers auto-load thinning to a follow-up PR. Executing the ruling belongs to that PR (root
side) and to Next Step 4 (standards-skill side).

### Phase 4 — dedupe to one home
Collapse each duplication row. Biggest win: seeding from 5 locations to the `seeding` skill (the principle)
plus one in-repo seed inventory (the forbidden-table list), with `api/AGENTS.md:26–45` becoming a pointer —
resolved under meta-rule 7 by deciding import-or-pointer, not both. Same treatment for the 5 app-side
double-writes, which currently load twice. Runs after 3b: dedupeing into files 3b then restructures would
edit the same lines twice.

### Phase 5 — one `standards` repo, domain trees, skills as routers

**Decided 2026-08-17, topology settled 2026-08-18.** Phase 3a landed 36 skills as flat sibling
directories. That is fine for the router and bad for a human: a listing of `write-boundary`,
`contract-naming`, `tiered-shared-code` answers "what exists?" only if you already know the answer.
Tommy's actual question — *"did I document this, and where is it?"* — has no cheap way to be asked, and
this plan's own execution proves the cost: three gaps were found by accident while cutting an unrelated
file, never by inspection.

So invert the two: **the doc is the payload and the skill is the router**, and organize the payload as a
tree by domain.

```text
standards/                          renamed from Concertable/agent-standards
  dotnet/
    style/      CSHARP_STYLE.md  CSHARP_NAMING.md  COMMENTS.md
    wiring/     DEPENDENCY_INJECTION.md  LOGGING.md  VALIDATION.md
    data/       PERSISTENCE.md  MULTITENANCY.md  SEEDING.md
    results/    CARRIERS.md  ERRORS.md  TERMINALS.md
    structure/  MODULE_STRUCTURE.md  MICROSERVICE_BOUNDARIES.md  HTTP_API.md
                PROTO.md  KEYED_STRATEGIES.md
    testing/    UNIT.md  INTEGRATION.md  E2E.md
  react/        TYPESCRIPT_STYLE.md  REACT_STRUCTURE.md  SERVER_STATE.md
                CLIENT_STATE.md  HTTP_LAYER.md  CONTRACT_NAMING.md
                WRITE_BOUNDARY.md  TIERED_SHARED_CODE.md  STACK_DEFAULTS.md
  process/      GIT_BRANCHING.md  COMMITTING.md  MERGING.md  PLANS.md
                DOCS_AND_DEBT.md  FAILING_TESTS.md  REMOTE_VALIDATION.md
  infra/        (later)
  concertable/  the platform roster every service repo inherits once api/ is gone:
                DATA_ACCESS.md  CONTRACTS.md  HTTP_CLIENTS.md  SEEDING_INVENTORY.md
  .agents/skills/<name>/SKILL.md    thin router -> its doc. Flat, one level.
```

**`dotagents` and a separate `react-agents` are not the shape.** They collapse into `standards/dotnet/`
and `standards/react/`, so there is one repo to look in and one place a new domain (`infra/`) is added.
`agent-utilities` keeps the machine tooling (`sync`, `worktree`, `recents`, `search`, `unmerged`,
`prune-worktrees`, …), which is neither a standard nor stack-specific. `agent-starter-kit` is a strict
subset of `dotagents` — two of its eight skills differ only by carrying a BOM that breaks frontmatter
parsing — so it is archived, not migrated.

**Skills stay flat** because discovery is `<root>/skills/*/SKILL.md` and does not recurse; only the
content moves into the tree. The deploy script flattens the tree into `~/.agents/skills/<name>`, so the
source layout is free and names must stay globally unique. Three consequences, all wanted: the tree is
browsable and diffable, a gap is visible as a missing node, and the same doc gains two delivery modes —
`@`-imported by a repo that wants it always-on, routed by the skill everywhere else. Content inside a
`SKILL.md` can only ever be delivered one way.

- **Do not name every leaf `CONVENTIONS.md`.** Twenty identically-named files defeat tabs, grep and
  review. The topic goes in the filename. This repo already paid twice: `CODE_CONVENTIONS.md` vs
  `CODE_PATTERNS.md` was the original "where does this rule go?" failure, and Phase 3b had to rename
  `CONVENTIONS.md` → `MODULE_STRUCTURE.md` to clear a collision.
- **Mandatory machine check — the tree must not grow orphans.** Every doc has exactly one routing skill
  and every skill points at a doc that exists. Two structures that can drift is precisely how 754 lines
  of frontend law ended up with zero inbound links. Both halves already exist: `docs_reachability.py`
  and the stub generator that refuses to emit an unroutable stub.
- **Generated index per domain.** The tree answers "where is it"; an index generated from the tree
  answers "did I document this" without opening anything.
- **Both generators must recurse.** `deploy-skills.ps1:53` and `sync-claude-skill-stubs.ps1` walk one
  level today, which is what made a flat source look mandatory.

### Phase 5b — `api/agents/` is deleted, not thinned

**The monorepo is going** (POLYREPO_ROADMAP §6, settled 2026-08-18: a true one-way cut, not read-only
mirrors). There is no `api/` node in a polyrepo, so nothing can live at `api/agents/` — the folder and
`api/AGENTS.md` are destinations with no future, and every rule in them must be re-homed. One test per
tier, applied per rule:

| The rule… | Home |
|---|---|
| names no product | `standards/dotnet/` or `standards/react/` |
| names a platform type every Concertable service uses | `standards/concertable/` |
| names one service's type | that service's own repo |

Worked example, the one Tommy raised: *"a repository binds to a capability, not a context type"* is
generic → `standards/dotnet/data/PERSISTENCE.md`. The `Concertable.DataAccess` capability hierarchy
(`IReadDbContext` → `IReadRepository` → `ReadRepository`) is platform-wide →
`standards/concertable/DATA_ACCESS.md`. B2B's concrete stances — `TenantScopedDbContext` for tenant-scoped
read/write, `VenueArtistTenantScopedDbContext`, `ReadDbContext` for unfiltered reads, `AdminDbContext` for
everything, and which entities carry a query filter — are B2B's → the B2B repo.

### Phase 5c — the discovery pass: write down what was never written down

**New scope, 2026-08-18.** Phases 3–5 move and organize rules that already exist as prose. They do not
find the conventions that live only in the code and in Tommy's head — and the B2B stance taxonomy above
is one he had to state verbally because no doc holds it. Organizing an incomplete corpus just produces a
tidy incomplete corpus.

So each domain node gets a pass that mines the code for its real conventions and writes the missing ones,
rather than only relocating text. The signals that a convention exists but is undocumented:

- a shape repeated across every module with no doc naming it (the stance taxonomy, `Schema.cs` constants)
- an `.editorconfig`/analyzer rule with no prose counterpart, or prose with no analyzer where one is possible
- a rule stated for one service that is really platform-wide, or the reverse
- a rule whose only statement is a code comment, a test name, or a `.Because(...)` string

This phase has no fixed size and is not a blocker for the others; it runs per domain node as that node is
created, so a node is only "done" when its inventory has been checked against code rather than against
the old docs. Every claim written must be verified against the code at the time of writing — four of the
seven findings in this PR's own review were docs asserting something the code did not do.

### Phase 6 — make consultation non-optional, because triage is not a guarantee — DONE (all three tiers; deployment is Phase 6a)

**Added 2026-08-17, after a live failure.** The plan already names the risk — a skill "applies **only if
it gets invoked**", and the tier table's guarantee column says exactly that. Its answer was *triage*: put
the expensive-and-silent rules in `AGENTS.md` and let the rest be skills. **That answer is now disproven.**
An agent added a test to `Concertable.ServiceDefaults` and got four things wrong at once — created
`Concertable.ServiceDefaults.Tests` (neither `*.UnitTests` nor `*.IntegrationTests`), booted a
`WebApplication` over HTTP inside what it called a unit test, used `Assert.*` where the integration tier
uses Shouldly, and wrote no sibling `AGENTS.md`. `unit-testing` and `integration-testing` were both
installed, both described, both listed. Neither was invoked, and the follow-up `/review` repeated the
identical blind spot and returned clean.

So the failure was **discretion, not reachability**, and no amount of better pointing fixes it. Three
tiers, in this order:

1. **Build-time is the guarantee.** Every service already has a `Directory.Build.props` that wires the
   shared `api/BannedSymbols.txt` into `AdditionalFiles` for `BannedApiAnalyzers` — the seam already
   trusted at error severity for `RS0030`. A shared targets file, imported the same one-line way, adds:
   a **tier-naming gate** (`IsTestProject == true` and no `UnitTests`/`IntegrationTests`/`E2ETests`/
   `ArchitectureTests` segment → error), a **misclassification gate** (a `*.UnitTests` project
   referencing `Mvc.Testing`, `TestHost`, `Testcontainers.*`, `Respawn`, `Playwright`, `Reqnroll` →
   error), and a per-tier `BannedSymbols.UnitTests.txt` catching `WebApplicationFactory<T>`/`TestServer`/
   `WebApplication.CreateBuilder` in source, which a package check misses on a transitive reference.
   `<IsTestProject>` is already explicit and correct in 51 csprojs (`false` in the 12 support libs), so
   there is nothing to infer. **Measured: zero current violations of all three, so this lands at error
   severity with no migration.** Must execute as a `<Target>`, not props logic — `Directory.Build.props`
   is imported before the csproj body, so `$(IsTestProject)` is not yet set there.
2. **A PreToolUse skill router for fast feedback and context injection** — `.agents/hooks/skill_router.py`
   over `Write|Edit|MultiEdit`, driven by a checked-in path→skill table (`*.UnitTests/**/*.cs` →
   `unit-testing`; `*Repository.cs` → `persistence` + `multitenancy`; `*.proto` → `proto`;
   `app/**/api/*.ts` → `http-layer` + `contract-naming`; a test csproj → both testing skills, because
   that is the classification moment). It injects the owning doc on first touch and blocks a fingerprint
   hit. **This is the piece that generalizes to anything: a new concern is a new row, not another
   paragraph nobody reads.** Same table should drive `/review`, which is how that blind spot closes
   mechanically instead of by remembering.
   **Its coverage is genuinely leaky and that is why it is tier 2, not tier 1:** matchers are per-tool, so
   `dotnet new`, a Bash heredoc or an MCP file write never reach a `Write` matcher, and it binds only a
   harness wired to it. Injection also is not compliance. Only the decidable set can be guaranteed.
3. **The stub leads with the decision, not the pointer.** `Conventions: the unit-testing skill` names a
   standard; what was needed was the unasked question, at the point of use — *"Unit-only: a test that
   needs a host, HTTP or a database belongs in `<Service>.IntegrationTests`."* And
   `docs_reachability.py` should require every `IsTestProject` directory to carry the `AGENTS.md` +
   `CLAUDE.md` pair, which removes "there was no stub at all" as a reachable state — the hole that made
   the `@`-import argument moot, since the incident folder had no `AGENTS.md` for an import to live in.

**Deliberately not mechanized: test method names.** Measured 1,062 unit-test methods; **256 (24%) do not
use the 3-part `Method_Scenario_ExpectedBehaviour` form** (`Map_ProjectsEveryItem`,
`CancelledSourceTasks_RemainCancelled`), most of them reasonable. A gate needs a 256-site migration first,
and the skill is overstating a rule the repo follows ~76% of the time. It belongs in the injected
standard, not in a blocker. Gating everything is how a gate gets switched off.

**Ordering consequence — this phase gates Phase 3b's delivery.** Thinning the in-repo corpus to
skill pointers must not merge before the mechanism that makes those skills fire, or the window between
them is exactly the incident above. Phase 6 rides the same PR.

### Phase 6b — `.agents`-first plugins ship the standard AND its enforcement, to both tools — DONE

**Decided 2026-08-17, and it supersedes how Phase 6's router is currently wired.** Two harnesses are in
daily use here, Codex at least as much as Claude, so anything Claude-only is the wrong primary. Verified
empirically rather than assumed:

- **`.agents/plugins/marketplace.json` is Codex's native plugin manifest path.** `codex plugin list`
  shows OpenAI's own bundled marketplaces resolving through exactly that path
  (`…/openai-primary-runtime/.agents/plugins/marketplace.json`). Codex's plugin loader also knows
  `.claude-plugin/marketplace.json` and `.cursor-plugin/marketplace.json` as alternates.
- **A plugin ships `skills/` *and* `hooks/hooks.json`** — both appear in Codex's plugin loader next to
  `plugin.json#hooks[]`. So one package can carry a standard and the hook that enforces it.
- **Codex supports `pre_tool_use` / `post_tool_use`**; both strings are in the Codex binary. The
  write-time router is therefore not a Claude-only mechanism, which was the open question.
- Repo-level wiring (`.claude/settings.json`, `.codex/hooks.json`) stays irreducibly per-harness —
  neither tool will read hook wiring out of `.agents/`.

**Then three further findings changed the shape, and the first one reverses the conclusion above.**

- **A plugin only runs where it was installed, so "delete the per-repo wiring" would have traded a
  code copy for an install ritual.** Installation is per-machine state (`~/.claude/plugins/installed_plugins.json`,
  `~/.codex/config.toml`), and Phase 6a already measured that this machine has installed the standards
  plugin *nowhere*. Enforcement that is absent on a fresh clone is not enforcement, so the wiring stays
  in the repo and the hook is **vendored** into it.
- **A harness installs a plugin by copying only the plugin subtree** (`~/.claude/plugins/cache/<marketplace>/<plugin>/<version>`
  holds `.claude-plugin`, `skills`, `agents` and nothing above them). The plugin's skill files were stubs
  pointing at `../../../../.agents/skills/…`, which exists only in the source repo — so the plugin would
  have installed cleanly and delivered nothing on every machine. The payload now carries generated full
  copies.
- **Codex reads project hooks from `<repo>/.codex/hooks.json` and trusts them by hash.** Confirmed live:
  `~/.codex/config.toml` carries `[hooks.state.'…\Concertable\.codex\hooks.json:stop:0:0']` with a
  `trusted_hash`, and its hook config is the same PascalCase-event/`matcher`/`command` shape Claude uses,
  plus `commandWindows`, `statusMessage`, and `${CLAUDE_PLUGIN_ROOT}` substitution. Its `pre_tool_use`
  payload keys and exit-2 block contract match Claude's exactly, so **one hook file serves both**. Two
  consequences: a new or edited hook needs a one-time trust approval in Codex, and each worktree is
  trusted separately from the main checkout.

**Target shape.** `.agents/` in the standards repo authors everything exactly once — skills, the hook,
and `.agents/plugins/marketplace.json` as the manifest Codex reads. Two generators emit the rest and
refuse to emit anything unroutable: `sync-generated.ps1` writes the repo-local stubs, the plugin payload
(full copies) and the `.claude-plugin/marketplace.json` shim; `vendor-hooks.ps1 -Into <repo>` copies the
hook into a consuming repo and records source, commit and hash in that repo's `.agents/hooks/vendored.json`.
The consuming repo owns only its own data — `skill-routes.json` — and its two wiring files, and its test
suite fails if a vendored copy was edited in place or is wired for one harness only.

So the plugin is the distribution channel for a repo that wants to install one, and vendoring is what
makes the mechanism fire on a clone where nothing is installed. Both are fed by the same authored file.

**Consequence for work already committed:** the router (`45c3cd304`) keeps its place in the repo, but as
a generated copy rather than a fork, and gains the `.codex/hooks.json` wiring it never had — the actual
defect behind "it is Claude-only". The build gate (`f99fa8c2f`) is unaffected — MSBuild is
harness-agnostic by construction, which is exactly why it is tier 1.

`extraKnownMarketplaces` + `enabledPlugins` in `.claude/settings.json` plus a per-person
`claude plugin install`. That works there because it is a Claude-only shop. Here it would strand Codex,
so it is at most optional sugar on the Claude side.


It is the same model — "engineering standards, distributed to every repo as load-on-demand skills" — and
it settles two things this plan was still arguing:

- **Mechanical enforcement belongs outside the skills.** Its editing rule: *"Mechanical rules don't belong
  here. Linters and hooks can fail a build; prose can't."* Phase 6 is the missing half of that model, not
  a departure from it.
- **Phase 5's sibling-doc pattern is proven, not speculative.** `backend-testing/fixtures.md` (174 lines)
  and `observability/kql-cookbook.md` (61) sit beside their `SKILL.md`, under the rule *"Long code goes in
  a sibling file, so the skill body stays cheap to load."* Phase 5 generalizes it to a topic-named tree
  with an orphan check and a generated index.

Also worth copying: it treats the commit SHA as the version (every merge ships), and its troubleshooting
table names our own Phase 6a failure exactly — *"No skills, `plugin list` empty → Step 2 skipped. Settings
never install a plugin."*

### Phase 6a — the shared skills are deployed by copy, and the copies had drifted

`~/.agents/skills/` is a **plain directory of copies** — not a junction, not a clone — so nothing links
an installed skill to its canonical source and nothing detects divergence. Verified state on 2026-08-17:

- **`agent-standards` is installed nowhere.** Not in `~/.agents/skills/`, not in `~/.claude/skills/`;
  `installed_plugins.json` holds only `stripe`, `clangd-lsp`, `rust-analyzer-lsp`, and
  `known_marketplaces.json` only `claude-plugins-official` — despite the repo carrying a valid
  `.claude-plugin/marketplace.json` declaring the `agent-process` plugin. So Phase 3a's "36 skills"
  is **29 in reality**, and the 7 process skills are unreachable from every session, by every agent.
- **Two installed skills were ahead of canonical and were nearly lost.** `prune-worktrees` (+34 lines,
  the sweep of orphaned leftover folders `git worktree list` is structurally blind to) and `worktree`
  (+17, the Windows "stand in the main checkout before deleting, then verify" footgun) existed **only**
  as loose installed files, edited 2026-08-10 against a repo still at its 2026-08-08 initial commit.
  Recovered into `dotagents` `c153697`; installed and canonical now agree on all 36. **A junction-based
  redeploy done without checking direction first would have destroyed both.**
- **3 canonical skills are not installed** — `last-conversation`, `recents`, `search`.

Fix the mechanism, not the symptom: **per-skill directory junctions** from `~/.agents/skills/<name>` and
`~/.claude/skills/<name>` to the owning repo, so a `git pull` is the deployment and drift is structurally
impossible. Per-skill rather than one junction at `skills/` because discovery is `<root>/skills/*/SKILL.md`
and does not recurse, and because two source repos must land in one namespace. It is also the idiom
already used for the work-repo skills. One stated trade-off: junctions make the installed set depend on
the repo staying put, and a deploy script must therefore also report orphaned and missing links.

### Deferred to follow-up PRs
Auto-load thinning (`api/AGENTS.md:3`'s three imports; the 86 merge lines and 32 Docker lines that
`/merge` and `scripts/e2e.ps1` already automate); the analyzer push-down plus
`EnforceCodeStyleInBuild`. The `portable/`/`local/` folder axis this plan originally specified is
superseded: skills cannot nest, and the axis is now carried by the `standards/<domain>/` tree plus repo
boundary (Phase 5).

The `docs_reachability.py` extension moved forward into Phase 2 rather than being deferred: without a
machine check, the nine dangling references fixed there simply accumulate again. It now errors on a
guidance doc that links a non-existent file or uses a root-absolute path, and warns for `plans/` and
`reviews/`, which are working docs that get deleted. It skips fenced blocks — a shell regex like
`[/\](bin|obj)[/\]` matches the markdown link pattern and is not a link.

## Open decisions

All Phase 1–2 rulings are settled (see the contradictions table). The `CONVENTIONS.md` →
`MODULE_STRUCTURE.md` rename **has happened** in Phase 3b, along with the fix to its stale "modules in the
monolith" framing; the three-copy merge-procedure question is settled above.

Codex parity, previously listed here as a gate on Phase 3b, was never a real one: the existing `.agents/`
convention already solves it. Canonical skills are plain markdown under `.agents/skills/`, agent-agnostic
by construction, and `dotagents` syncs them to `~/.agents/skills/`, which a Codex session reads. Claude Code
gets generated stubs. Phase 3b cuts nothing that either tool then lacks.

Still open:

1. **Auto-load budget** — Phase 5 would drop `api/AGENTS.md:3`'s three `@`-imports (1,331 lines) and
   the always-loaded merge/Docker blocks that `/merge` and `scripts/e2e.ps1` already automate. Is
   dropping `RESULT_PATTERN.md` from every-prompt load acceptable given it is the most-violated set?
