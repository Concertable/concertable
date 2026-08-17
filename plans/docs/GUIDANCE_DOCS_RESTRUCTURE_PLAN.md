# Guidance-docs restructure — plan

Reorganize the guidance corpus (root `AGENTS.md`, `api/agents/*`, `app/agents/*`, `docs/*`,
`api/docs/*`) so that every rule has exactly one home, the auto-loaded weight is proportionate, and the
portable half sits in its own folder — ready to be extracted to a shared .NET conventions repo by
`git mv` rather than by rewriting.

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
apart. `api/AGENTS.md:97` renders as a bare `See [CONVENTIONS.md](./agents/CONVENTIONS.md)` under
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

## Target structure

```text
AGENTS.md                          entry: the floor + the index. No rule bodies that live elsewhere.
docs/
  INDEX.md                         topic -> owning file. The dedupe device.
api/
  AGENTS.md                        pointers + api-wide floor only
  ARCHITECTURE.md                  authoritative current state
  conventions/                     from api/agents/
    README.md                      index + the meta-rules below
    portable/
      CSHARP_STYLE.md              braces, empty blocks, this., null!, primary ctors, extension members
      CSHARP_NAMING.md             suffix table, mappers, Projection, optional params, pure operations
      COMMENTS_AND_XMLDOC.md
      DEPENDENCY_INJECTION.md      DI + dependency-holders
      LOGGING.md                   Log.cs, absorbing DEBUGGING_CONVENTIONS
      VALIDATION.md                FluentValidation vs ValidationResult
      RESULT_CARRIERS.md           RESULT_PATTERN :28-302
      RESULT_ERRORS.md             :303-481
      RESULT_TERMINALS.md          :482-621
      TESTING_UNIT.md
      TESTING_INTEGRATION.md
      TESTING_E2E.md
      MODULE_STRUCTURE.md          from CONVENTIONS.md - layers, visibility, folder layout
      MICROSERVICE_BOUNDARIES.md
      MICROSERVICE_COMMUNICATION.md
      CONTRACT_DISTRIBUTION.md
      PERSISTENCE.md               repository bases, Schema.cs, unit of work, pagination
      TENANCY_COMPOSITION.md       compose-don't-subtract, stance naming
      SEEDING.md                   the principle: drive the trigger, never the row
      KEYED_STRATEGIES.md          the pattern, key-agnostic
    local/
      SEED_INVENTORY.md            the Concertable forbidden-table list
      TENANCY.md                   context roster, which entities are filtered
      DEAL_STRATEGIES.md           DealType families, workflow steps
      HTTP_CONTRACTS.md            Dto/Request/Response + the XDetailsResponse carve-out
      REFIT_CLIENTS.md             client roster + ITokenApi caveat
      ORGANIZATION_BOUNDARY.md     tenant internally, organization at HTTP
      GEOMETRY.md
      PACKAGES.md                  Reunion pins, UseLocalCore, carve enforcement
app/
  AGENTS.md                        keep as-is (already the correct mount point)
  conventions/
    README.md
    portable/                      TYPESCRIPT_STYLE, CONTRACT_NAMING, REACT_STRUCTURE,
                                   SERVER_STATE, CLIENT_STATE, HTTP_LAYER, WRITE_BOUNDARY,
                                   TIERED_SHARED_CODE
    local/                         APP_TIERS, IDENTITY, CLIENTS, PERMISSIONS, BROWSER_STORAGE
```

Per-service `AGENTS.md` / `ARCHITECTURE.md` / `TECH_DEBT.md` files stay put — doc locality already works
at that level. `api/docs/` keeps `MICROSERVICES_ARCHITECTURE.md` as dated history.

## The meta-rules

Into each `conventions/README.md`. These generalize what the repo already states in two places —
`.agents/README.md` ("duplicated skill bodies drift") and `RESULT_PATTERN.md:4` ("sole source of truth").

1. **One rule, one home.** Everywhere else links; never restates. A second copy is a bug.
2. **No file straddles `portable/` and `local/`.** A portable file contains no Concertable identifier;
   concrete precedents live in the local sibling.
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

### Phase 3 — split and move
`git mv` into the target tree; split `RESULT_PATTERN.md`, both `CODE_CONVENTIONS.md`, both
`CODE_PATTERNS.md`, and `CONVENTIONS.md` → `MODULE_STRUCTURE.md`. Text carried verbatim except where
Phase 4 dedupes it, so the diff reads as a move.

### Phase 4 — dedupe to one home
Collapse each duplication row. Biggest win: seeding from 5 locations to
`portable/SEEDING.md` + `local/SEED_INVENTORY.md`, with `api/AGENTS.md:26–45` becoming a pointer —
resolved under meta-rule 7 by deciding import-or-pointer, not both. Same treatment for the 5 app-side
double-writes, which currently load twice.

### Deferred to follow-up PRs
Auto-load thinning (`api/AGENTS.md:3`'s three imports; the 86 merge lines and 32 Docker lines that
`/merge` and `scripts/e2e.ps1` already automate); the analyzer push-down plus
`EnforceCodeStyleInBuild`; extraction of `portable/`.

The `docs_reachability.py` extension moved forward into Phase 2 rather than being deferred: without a
machine check, the nine dangling references fixed there simply accumulate again. It now errors on a
guidance doc that links a non-existent file or uses a root-absolute path, and warns for `plans/` and
`reviews/`, which are working docs that get deleted. It skips fenced blocks — a shell regex like
`[/\](bin|obj)[/\]` matches the markdown link pattern and is not a link.

## Open decisions

All Phase 1–2 rulings are settled (see the contradictions table). Still open, and needed before Phase 3:

1. **`api/agents/CONVENTIONS.md` rename** — `MODULE_STRUCTURE.md` (recommended: fixes both the
   collision with `CODE_CONVENTIONS.md` and the stale "monolith" framing at `:6`/`:91`) or keep it?
2. **Auto-load budget** — Phase 5 would drop `api/AGENTS.md:3`'s three `@`-imports (1,331 lines) and
   the always-loaded merge/Docker blocks that `/merge` and `scripts/e2e.ps1` already automate. Is
   dropping `RESULT_PATTERN.md` from every-prompt load acceptable given it is the most-violated set?
