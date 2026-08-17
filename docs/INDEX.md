# Guidance index — topic → owning doc

Every rule has **one** owning doc. Look the topic up here before writing a rule down; if it already has
an owner, add it there and link from wherever else it matters. A second copy of a rule is a bug, not
emphasis — the copies drift, and the reader can't tell which one is current.

**Two kinds of owner.** A `skill` entry names a load-on-demand skill: generic standards that name no
product, so they live outside this repo (`~/.agents/skills/` from `tomjseery/dotagents`, plus the
Concertable process skills in `Concertable/agent-standards`) and apply to every repo. Invoke it by name;
the task you are doing is the trigger. A path entry names a file here, and every one of those carries only
what a skill deliberately omits — the roster of real types, contexts, clients and tables in *this* system.
When a topic has both, the skill owns the rule and the file owns the inventory.

`.agents/hooks/docs_reachability.py` checks that docs are *reachable*. Nothing checks that they are
*non-duplicated* or *correct*. That is what this file and the rules at the bottom are for.

## Process — how work gets done

| Topic | Owner |
|---|---|
| Long-term-over-hack; questions before actions; autonomy on reversible work | [`AGENTS.md`](../AGENTS.md) |
| Branching, `<Type>/<Name>` casing, worktree identity gate, branch from `origin/main` | [`AGENTS.md`](../AGENTS.md) "Git branch" |
| Ready-for-review ≠ merge authorization | [`AGENTS.md`](../AGENTS.md) |
| Branch must be current with base before auto-merge | [`AGENTS.md`](../AGENTS.md) "Before enabling auto-merge" |
| Merge confirmation — the four terminal states, bash until-loop, never `Monitor` | [`AGENTS.md`](../AGENTS.md) "Confirming a PR merge" |
| Platform sync gate after an `api/**` merge | [`AGENTS.md`](../AGENTS.md) "Platform sync is a live gate" |
| Which E2E tier a merge runs | [`.agents/skills/merge/SKILL.md`](../.agents/skills/merge/SKILL.md) Step 4 |
| Docker health pre-flight before any E2E run | [`AGENTS.md`](../AGENTS.md) "E2E suites" |
| Which gate runs where — local vs draft-PR CI vs merge queue | [`REMOTE_VALIDATION.md`](./REMOTE_VALIDATION.md) |
| Recording and clearing tech debt | [`AGENTS.md`](../AGENTS.md) "Tech debt" |
| Plan/roadmap/ledger structure and lifecycle | [`plans/AGENTS.md`](../plans/AGENTS.md), [`plans/agents/PLAN.md`](../plans/agents/PLAN.md) |
| Review files as work orders; addressing and deleting findings | [`reviews/AGENTS.md`](../reviews/AGENTS.md) |
| Continuation, handoff and resume prompt shape | [`PROMPTS.md`](../PROMPTS.md) |
| Code comments — default to none | [`AGENTS.md`](../AGENTS.md) "Code comments" |
| Doc locality, `CLAUDE.md` siblings, reachability | [`AGENTS.md`](../AGENTS.md) "Per-area guidance" |
| Throwaway working markdown | [`AGENTS.md`](../AGENTS.md) |
| Worktree cleanup | [`AGENTS.md`](../AGENTS.md) + `scripts/worktrees.ps1` |

## Architecture — what may depend on what

| Topic | Owner |
|---|---|
| System-wide premise; monorepo vs the split-repo world | [`ARCHITECTURE.md`](../ARCHITECTURE.md) |
| **Current-state backend architecture — authoritative** | [`api/ARCHITECTURE.md`](../api/ARCHITECTURE.md) |
| Adapter vs data services; what may `WaitFor` what | [`api/ARCHITECTURE.md`](../api/ARCHITECTURE.md) |
| Standalone AppHost is canonical; the simulator pattern | [`api/ARCHITECTURE.md`](../api/ARCHITECTURE.md) |
| Producer seed libraries point downward only | [`api/ARCHITECTURE.md`](../api/ARCHITECTURE.md) |
| Cross-service contract distribution; per-folder build closures; `UseLocalCore` | [`api/ARCHITECTURE.md`](../api/ARCHITECTURE.md) |
| Protocol selection — gRPC / HTTP / Service Bus | skill `microservice-boundaries` |
| Which surface each service actually exposes | [`api/ARCHITECTURE.md`](../api/ARCHITECTURE.md) "The surface each service actually exposes" |
| Design rationale and decision history (not current state) | [`api/docs/MICROSERVICES_ARCHITECTURE.md`](../api/docs/MICROSERVICES_ARCHITECTURE.md) |
| Per-service specifics | that service's own `AGENTS.md` / `ARCHITECTURE.md` |

## Backend code (`api/`)

| Topic | Owner |
|---|---|
| C# style — fields, ctors, `null!`, braces, optional params, `base.`, `#region`, `extension()` | skill `csharp-style` |
| C# naming — suffix table, `Projection`, `Response`/`Dto`, `XMappers`, evaluators, frozen tables | skill `csharp-naming` |
| Comments and XML doc mechanics | skill `comments` (policy: [`AGENTS.md`](../AGENTS.md) "Code comments") |
| DI registration, dependency-holders, lifetimes | skill `dependency-injection` |
| Logging — source-generated `Log.cs`, probes included | skill `logging` |
| Validator tool choice, `ValidationResult`, accumulation | skill `validation` |
| Repositories, `Schema.cs`, pagination, unit of work, write→read FKs | skill `persistence` |
| Tenancy composition, context stances, query filters, repository qualifiers | skill `multitenancy` |
| Behaviour that varies by a closed key | skill `keyed-strategies` |
| Project layering, reference graph, visibility cascade, cross-module rules, module facades | skill `module-structure` |
| Endpoint contracts — DTO vs `Response`, `Request` records, route vocabulary | skill `http-api` |
| Result and Option carriers; typed errors; transport terminals | skills `result-carriers`, `result-errors`, `result-terminals` |
| Proto naming, proto mappers, wire error mapping | skill `proto` |
| Seeding — drive the trigger, never write the row | skill `seeding` |
| Unit / integration / E2E scenario authoring | skills `unit-testing`, `integration-testing`, `e2e-scenarios` |
| **This repo's** api-wide precedents — `Concertable.DataAccess` capability hierarchy, `IGeometryProvider`, `IPagination.Map` placement, integration-event wire versioning | [`api/agents/CODE_CONVENTIONS.md`](../api/agents/CODE_CONVENTIONS.md) |
| **This repo's** structural precedents — Refit client inventory, one repository per entity | [`api/agents/CODE_PATTERNS.md`](../api/agents/CODE_PATTERNS.md) |
| **This repo's** Reunion pins and legacy-carrier migration state | [`api/agents/RESULT_PATTERN.md`](../api/agents/RESULT_PATTERN.md) |
| **This repo's** project naming, `organization` routes, `Genre` enum, no cross-module read context | [`api/agents/MODULE_STRUCTURE.md`](../api/agents/MODULE_STRUCTURE.md) |
| **This repo's** forbidden seed tables, the B2B simulator, the ticket-sales exception | [`api/agents/SEEDING_CONVENTIONS.md`](../api/agents/SEEDING_CONVENTIONS.md) |
| **This repo's** integration fixtures, shared harness members, run commands | [`api/agents/INTEGRATION_CONVENTIONS.md`](../api/agents/INTEGRATION_CONVENTIONS.md) |
| **This repo's** E2E baseline path, run script, seeded fast-forward | [`Concertable.Testing.E2E`](../api/Concertable.Shared/tests/Concertable.Testing.E2E/AGENTS.md) |
| Page objects, `data-testid` naming, step-binding shape; the Stripe 3DS/timeout traps | [`E2E_UI_CONVENTIONS.md`](../api/Concertable.Shared/tests/Concertable.Testing.E2E/E2E_UI_CONVENTIONS.md), [`E2E_CONSIDERATIONS.md`](../api/Concertable.Shared/tests/Concertable.Testing.E2E/E2E_CONSIDERATIONS.md) |
| B2B's DbContext stances, filtered entities, `DealType` families and workflow steps | [`api/Concertable.B2B/CODE_PATTERNS.md`](../api/Concertable.B2B/CODE_PATTERNS.md) |
| DTOs vs Responses at the controller boundary; migrations; shared-is-the-intersection | [`api/AGENTS.md`](../api/AGENTS.md) |

## Frontend code (`app/`)

| Topic | Owner |
|---|---|
| `interface` vs `type`, casing, `undefined` over `null`, discriminated unions | skill `typescript-style` |
| Read/write contract naming, one `types.ts` per feature | skill `contract-naming` |
| Feature slices, hooks vs components, raw vs facade hooks, Effects, table dispatch | skill `react-structure` |
| Queries, mutations, query keys, mutation variables | skill `server-state` |
| Private stores, facade hooks, derived state, imperative session | skill `client-state` |
| `xApi` objects, one client per backend, errors resolved once | skill `http-layer` |
| The zod parse between buffer and request | skill `write-boundary` |
| Slots over role checks, composed identity, tier discipline | skill `tiered-shared-code` |
| Which library to reach for | skill `stack-defaults` |
| The sharing tiers and the build gate | [`app/AGENTS.md`](../app/AGENTS.md) |
| **This repo's** four HTTP clients, `isApiError` seam, `$type` unions, `FormData` casing | [`app/agents/CODE_CONVENTIONS.md`](../app/agents/CODE_CONVENTIONS.md) |
| **This repo's** `User`/`B2bIdentity` split, tenant session, `SharedPermissions` | [`app/agents/CODE_PATTERNS.md`](../app/agents/CODE_PATTERNS.md) |
| Axios confinement and the error contract | [`app/web/AGENTS.md`](../app/web/AGENTS.md) "HTTP errors" |
| What belongs in each tier | that tier's own `AGENTS.md` |
| Browser storage inventory and consent gating | [`app/web/shared/BROWSER_STORAGE.md`](../app/web/shared/BROWSER_STORAGE.md) |

## Rules enforced by a machine, not by prose

Check this before writing a style rule — if a tool can hold it, the doc gets one line and the
diagnostic or test name, not an argument.

| Rule | Enforcer | Fails a build? |
|---|---|---|
| No inline `logger.Log*` | `CA1848` = error (`.editorconfig`) | Yes |
| Sealing where possible | `MA0053` = error (Meziantou) | Yes |
| `IgnoreQueryFilters` banned | `RS0030` = error + `api/BannedSymbols.txt` | Yes |
| Private instance fields camelCase, no underscore | `.editorconfig` naming rule | **No** — IDE only; no `EnforceCodeStyleInBuild` is set |
| File-scoped namespaces, `IDE0130` | `.editorconfig` | **No** — same reason |
| Keyed-strategy coverage and no service location | `DealStrategyArchitectureTests`, plus `RequireAll`/`RequireExactly` at composition | Yes |
| No legacy Result carriers; no Dunet in shared production | `ReunionArchitectureTests`, `TypedResultArchitectureTests` | Yes |
| One read-context contract, one generic read repository | `RepositoryArchitectureTests` | Yes |
| Service boundaries hold when carved | `EnforceServiceBoundary` + the `carve-*` CI jobs | Yes |
| Docker is really healthy before E2E | `scripts/docker-health.ps1`, gated by `scripts/e2e.ps1` | Gate |
| Docs are reachable; `CLAUDE.md` siblings exist; every test project carries a stub stating its tier | `.agents/hooks/docs_reachability.py` via `docs-review` | Gate |
| Plan handoff ends with its continuation pointer | `.agents/hooks/plan_handoff_stop.py` | Gate |
| A test project's name declares its tier; a unit test cannot boot a host, container or database | `api/TestConventions.targets` + `api/BannedSymbols.UnitTests.txt` | Yes |
| The standard that owns a path is loaded before the first write into it | `.agents/hooks/skill_router.py` over `.agents/skill-routes.json`, wired in `.claude/settings.json` and `.codex/hooks.json` | Gate |
| A vendored hook still matches upstream and is wired for both harnesses | `.agents/hooks/tests/test_vendored_hooks.py` | Gate |
| A review loads the same standards the author was required to load | `skill_router.py --skills-for` over the same table, run by [`review`](../.agents/skills/review/SKILL.md) Step 2 | Gate |

## Adding to the corpus

1. **One rule, one home.** Look it up above first. Elsewhere links; it never restates. If you find
   yourself writing "as described in X" followed by the rule itself, delete the rule and keep the link.
2. **If a machine can enforce it, say so in one line** with the diagnostic or test name, and skip the
   argument. Prose is for what a tool cannot express.
3. **Headings are imperative rule statements, not topic labels** — "Repositories inherit the module
   base", not "Repositories". The heading should be the rule.
4. **A rule is about 15 lines**: statement, anti-pattern, one example, in that order. Past ~80 lines it
   earns its own file; under ~20 lines a file should merge into its parent.
5. **Never name violation sites.** They get fixed and the citation rots — silently, because nothing
   checks it. Violations belong in the owning `TECH_DEBT.md`. State the shape, not the address.
6. **A doc is either `@`-imported or summarized — never both.** Summarizing an imported doc duplicates
   it into the same context twice; summarizing a linked one is how the two versions drift apart. Decide
   which, then commit to it.
7. **Scope a rule by what pulls it in, not by where the file sits.** A generic rule becomes a skill and
   is pulled in by the task — its `description` is the router, so it must name both the content and the
   trigger or it silently never loads. A repo-specific rule is a file imported by **only** the `AGENTS.md`
   of a consumer that actually has the thing: B2B's context and `DealType` rosters load on B2B prompts,
   not on every `api/**` prompt. A folder cannot stop a file loading; only the import edge can. A scoped
   topic that gains a second consumer stays one file and gains an import — never a copy, and never a
   promotion into the baseline just because two consumers use it.
8. **Keep the rule generic and the precedents local.** A rule that names Concertable types can't be
   reused or lifted, so state the shape generically and put the roster of real examples in the
   consumer's own doc. Generic topic files are therefore exempt from doc locality — a generic convention
   isn't *about* any node, it's a library entry addressed by import rather than by position.
9. **Links are repo-relative.** A root-absolute `/api/...` path renders broken and silently satisfies
   the reachability hook without pointing anywhere.
10. **Check the code before you write the rule down.** Several rules here taught things the codebase had
   already moved past, and every one of them read as maintained.
