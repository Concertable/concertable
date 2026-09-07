# Guidance index — topic → owning doc

Every rule has **one** owning doc. Look the topic up here before writing a rule down; if it already has
an owner, add it there and link from wherever else it matters. A second copy of a rule is a bug, not
emphasis — the copies drift, and the reader can't tell which one is current.

**Two kinds of owner.** A `skill` entry names a load-on-demand skill: generic standards that name no
product, so they live outside this repo (`~/.agents/skills/`, fed by `tomjseery/dotagents` for .NET,
`tomjseery/react-agents` for React/TS, and `Concertable/agent-standards` for the process skills) and apply
to every repo. Invoke it by name;
the task you are doing is the trigger. A path entry names a file here, and every one of those carries only
what a skill deliberately omits — the roster of real types, contexts, clients and tables in *this* system.
When a topic has both, the skill owns the rule and the file owns the inventory.

`.agents/hooks/docs_reachability.py` checks that docs are *reachable*. Nothing checks that they are
*non-duplicated* or *correct*. That is what this file and the rules at the bottom are for.

## Product — what is being built

Product & system narrative lives in the central [`Concertable/docs`](https://github.com/Concertable/docs) repo, not here — it belongs to no single service.

| Topic | Owner |
|---|---|
| What the product is, the core loop, the four deal types | [`OVERVIEW.md`](https://github.com/Concertable/docs/blob/main/OVERVIEW.md) |
| Product thesis and competitive landscape | [`USP.md`](https://github.com/Concertable/docs/blob/main/USP.md) |
| How to run a `/deep-research` discovery | [`DEEP_RESEARCH_PROMPT_GUIDE.md`](https://github.com/Concertable/docs/blob/main/DEEP_RESEARCH_PROMPT_GUIDE.md) |

## Process — how work gets done

| Topic | Owner |
|---|---|
| Long-term-over-hack; questions before actions; autonomy on reversible work | skill `floor` (injected at SessionStart) |
| Branching — `<Type>/<Name>` casing, branching from `origin/main`, refactors that stay on their feature branch | skill `git-branching` |
| The worktree identity gate; splitting durable guidance onto a `Docs/*` branch | skills `open-worktree`, `git-branching`; the repo's identity-gate keep in [`AGENTS.md`](../AGENTS.md) |
| When to commit, when to push, fewest safe merges | skill `committing` |
| Committing procedure — slicing by workstream, or the whole tree in one commit | skills `commit`, `commit-all` |
| Pushing a head and proving the remote carries it; pulling and recovering a failed pull | skills `push`, `pull` |
| Bringing a checkout up to date — a stale `origin/HEAD`, a branch that already shipped, drift against the default | skill `sync-checkout` |
| Ready-for-review ≠ merge authorization | skill `merging` |
| Merge procedure — currency check, enqueue, the four terminal states, sync follow-through | skill `merge` (the reasoning behind each state: skill `merging`) |
| Opening a PR, and the read-only gate that clears a branch first | skills `open-pr`, `pr-preflight` |
| Landing a meta-only change without the queue's E2E gate | skill `merge-docs` |
| The merge invariants — never arm auto-merge behind `main`, a failed check is real, whoever merges owns the sync | skill `merging` |
| Which E2E tier a merge runs | skill `merge`, Step 4 |
| Platform sync gate after an `api/**` merge | skill `merging` |
| Which gate runs where, and the Docker pre-flight before any local E2E | skill `remote-validation`; this repo's commands [`REMOTE_VALIDATION.md`](./REMOTE_VALIDATION.md) |
| Running the app locally — one-time `setup-local-dev.ps1`, gitignored dev config, ServiceAuth secrets, port map, MAX_PATH gotcha | [`LOCAL_DEV.md`](./LOCAL_DEV.md) (`README.md` links it from "How to Run the Code") |
| A red suite is never just reported, and which tier's skill owns it | skill `failing-tests` |
| Driving a red suite to green — the in-process, service-E2E and browser-E2E tiers, the baseline regression check, both tiers at once | skills `integration-debug`, `e2e-api-debug`, `e2e-ui-debug`, `e2e-ui-regress`, `e2e-debug` |
| Test Explorer showing traits the build strips, or missing a project's tests | skill `reset-test-explorer` |
| Plan/roadmap/ledger structure, lifecycle and method | skill `plans`; this repo's layout, scripts and skill names [`plans/AGENTS.md`](../plans/AGENTS.md) |
| Reviewing a branch, a docs diff, a massive branch, or only the new commits | skills `review`, `docs-review`, `big-review`, `big-review-all`, `incremental-review` |
| Review files as work orders; splitting, deferring and deleting findings | skill `review-lifecycle` (the procedure that obeys it: skill `address-review`) |
| Continuation, handoff and resume prompt shape | skill `handoff` |
| One rule one home, doc locality, reachability, tech debt, throwaway markdown | skill `docs-and-debt` |
| Code comments — default to none | global agent instructions (mechanics: skill `comments`) |
| Creating, inspecting or closing one worktree | skill `open-worktree` |
| Worktree cleanup — audit/close/retire | skill `open-worktree`; plan-managed close [`plans/AGENTS.md`](../plans/AGENTS.md); `scripts/worktrees.ps1` |

## Architecture — what may depend on what

| Topic | Owner |
|---|---|
| System-wide premise; monorepo vs the split-repo world | [`ARCHITECTURE.md`](../ARCHITECTURE.md) |
| Cross-service references are Contracts-only | skill `dotnet:microservice-boundaries` |
| Protocol selection — gRPC / HTTP / Service Bus | skill `dotnet-standards:microservice-boundaries` |
| Adapter vs data services, what may `WaitFor` what, standalone-AppHost-is-canonical, the surface each service exposes | skill `dotnet-standards:microservice-boundaries` |
| Producer seed libraries point downward only; the simulator that makes standalone work | skill `dotnet-standards:seeding` |
| Design rationale and decision history (not current state) | skill `microservices-architecture` |
| Per-service specifics | that service's own `AGENTS.md` / `ARCHITECTURE.md` |

## Backend code (`api/`)

| Topic | Owner |
|---|---|
| C# style — fields, ctors, `null!`, braces, optional params, `base.`, `#region`, `extension()` | skill `csharp-style` |
| C# naming — suffix table, `Projection`, `Response`/`Dto`, `XMappers`, evaluators, frozen tables | skill `csharp-naming` |
| Comments and XML doc mechanics | skill `comments` (the default-to-none policy is global) |
| DI registration, dependency-holders, lifetimes | skill `dependency-injection` |
| Executable host inventory, strict provider validation and dynamic activation-root coverage | skill `composition-testing` |
| Logging — source-generated `Log.cs`, probes included | skill `logging` |
| Validator tool choice, `ValidationResult`, accumulation | skill `validation` |
| Repositories, `Schema.cs`, pagination, unit of work, write→read FKs | skill `dotnet-standards:persistence` |
| Tenancy composition, context stances, query filters, repository qualifiers | skill `dotnet-standards:multitenancy` |
| Domain events — raise on the entity, dispatch at the save, publish through the outbox | skill `dotnet-standards:domain-events` |
| Behaviour that varies by a closed key | skill `keyed-strategies` |
| Project layering, reference graph, visibility cascade, cross-module rules, module facades | skill `dotnet-standards:module-structure` |
| Endpoint contracts — DTO vs `Response`, `Request` records, route vocabulary | skill `dotnet-standards:http-api` |
| Result and Option carriers; typed errors; transport terminals | skills `result-carriers`, `result-errors`, `dotnet-standards:result-terminals` |
| Proto naming, proto mappers, wire error mapping | skill `proto` |
| Seeding — drive the trigger, never write the row | skill `dotnet-standards:seeding` |
| Which .NET library for which job, and what is deliberately not used | skill `dotnet-stack` |
| `XMappers` placement and naming; when a pure operation is an extension vs a named evaluator | skill `csharp-naming` |
| `extension()` block form, and migrating a legacy `this`-parameter container | skill `csharp-style` |
| Unit / integration / E2E scenario authoring | skills `dotnet-standards:unit-testing`, `dotnet-standards:integration-testing`, `e2e-scenarios` |
| **This system's** `Concertable.DataAccess` capability hierarchy, one repository per entity | skill: `dotnet:persistence` |
| **This system's** `IGeometryProvider` / WGS84 | skill: `geometry` |
| **This system's** `IPagination.Map` placement, integration-event wire versioning, the `Genre` enum | skill: `dotnet-contracts` |
| **This system's** Refit client inventory and the `ITokenApi` caveat | skill: `http-clients` |
| Contract distribution, per-folder build closures, `UseLocalCore`, the Reunion pins, never-redistribute and the carve gates | skill `packages` |
| **This system's** gRPC cancellation predicate | skill: `dotnet:result-terminals` |
| **This system's** project naming, internal controllers, no cross-module read context | skill: `dotnet:module-structure` |
| **This system's** `Tenant`-to-`organization` translation and route shapes | skill: `dotnet:http-api` |
| **This system's** service roster, adapter-vs-data, the surfaces each exposes | skill: `dotnet:microservice-boundaries` |
| **This system's** migration policy | skill: `migrations` |
| **This system's** tenanted service, which project owns each stance piece | skill: `dotnet:multitenancy` |
| **This system's** 13 pre-commit handlers and the seeding interceptor's phase swap | skill: `dotnet:domain-events` |
| **This system's** forbidden seed tables, the B2B simulator, the ticket-sales exception | skill: `dotnet:seeding` |
| **This system's** integration fixtures and shared harness members | skill: `dotnet:integration-testing` |
| **This system's** test-tier gate, the unit-tier bans, the settled assertion library | skill: `dotnet:unit-testing` |
| **This repo's** E2E baseline path, run script, seeded fast-forward | [`Concertable.Testing.E2E`](../api/Concertable.Shared/tests/Concertable.Testing.E2E/AGENTS.md) |
| Page objects, `data-testid` naming, step-binding shape; the Stripe 3DS/timeout traps | [`E2E_UI_CONVENTIONS.md`](../api/Concertable.Shared/tests/Concertable.Testing.E2E/E2E_UI_CONVENTIONS.md), [`E2E_CONSIDERATIONS.md`](../api/Concertable.Shared/tests/Concertable.Testing.E2E/E2E_CONSIDERATIONS.md) |
| B2B's DbContext stances, filtered entities, `DealType` families and workflow steps | [`api/Concertable.B2B/CODE_PATTERNS.md`](../api/Concertable.B2B/CODE_PATTERNS.md) |
| DTOs vs `Response` at the controller boundary | skills `dotnet-standards:http-api`, `dotnet:http-api` |
| Migrations; shared-is-the-intersection; the seeder trigger rule | skills `migrations`, `dotnet:microservice-boundaries`, `dotnet:seeding` |

## Frontend code (`app/`)

| Topic | Owner |
|---|---|
| `interface` vs `type`, casing, `undefined` over `null`, discriminated unions | skill `react-standards:typescript-style` |
| Read/write contract naming, one `types.ts` per feature | skill `contract-naming` |
| Feature slices, hooks vs components, raw vs facade hooks, Effects, table dispatch | skill `react-structure` |
| Queries, mutations, query keys, mutation variables | skill `server-state` |
| Private stores, facade hooks, derived state, imperative session | skill `react-standards:client-state` |
| `xApi` objects, one client per backend, errors resolved once | skill `react-standards:http-layer` |
| The zod parse between buffer and request | skill `write-boundary` |
| Slots over role checks, composed identity, tier discipline | skill `tiered-shared-code` |
| Which library to reach for | skill `stack-defaults` |
| Routing, guards, search params, loader vs query | skill `routing` |
| Tailwind, `cn`/`cva`, owned primitives, toasts, motion | skill `ui-components` |
| TanStack Table behind one `DataTable` | skill `data-tables` |
| dayjs behind one formatting module | skill `date-formatting` |
| What a Vitest suite covers and what the browser suite owns | skill `frontend-testing` |
| **This system's** surfaces, sharing tiers, route contract and typecheck gate | skill: `app-tiers` |
| **This system's** four HTTP clients and the `isApiError` seam | skill: `react:http-layer` |
| **This system's** `$type` unions and `FormData` casing | skill: `react:typescript-style` |
| **This system's** `User`/`B2bIdentity` split | skill: `identity` |
| **This system's** tenant session and active-tenant state | skill: `react:client-state` |
| **This system's** `SharedPermissions` matrix | skill: `permissions` |
| Axios confined to the client layer; where a guard may branch on status | skills `react-standards:http-layer`, `react:http-layer` |
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
| Deal-strategy coverage and no service location | `DealStrategyBuilder` / `DealUnionBuilder<TUnion>` composition validation plus `DealStrategyArchitectureTests` | Yes |
| Every executable .NET host has strict provider validation and real composition-test coverage or a reviewed exclusion | `ExecutableHostInventory` + the `architecture-tests` CI matrix | Yes |
| No legacy Result carriers; no Dunet in shared production | `ReunionArchitectureTests`, `TypedResultArchitectureTests` | Yes |
| One read-context contract, one generic read repository | `RepositoryArchitectureTests` | Yes |
| Service boundaries hold when carved | `EnforceServiceBoundary` + the `carve-*` CI jobs | Yes |
| Docker is really healthy before E2E | `scripts/docker-health.ps1` (vendored — edit it upstream, not here), gated by `scripts/e2e.ps1` | Gate |
| Docs are reachable; `CLAUDE.md` siblings exist; every test project carries a stub stating its tier | `.agents/hooks/docs_reachability.py` via `docs-review` | Gate |
| Plan handoff ends with its continuation pointer | `.agents/hooks/plan_handoff_stop_launcher.py` | Gate |
| No `gh pr merge` without a current, clean code-review | `.agents/hooks/merge_review_gate.py` over `.agents/merge-gate.json` | Gate |
| A test project's name declares its tier; a unit test cannot boot a host, container or database | `api/TestConventions.targets` + `api/BannedSymbols.UnitTests.txt` | Yes |
| The standard that owns a path is loaded before the first write into it | `.agents/hooks/skill_router.py` over `.agents/skill-routes.json`, wired in `.claude/settings.json` and `.codex/hooks.json` | Gate |
| **No source file is ungated.** Two area floors (`api/**/*.cs`, `app/**/*.{ts,tsx}`) plus four layer routes keyed on the project's layer, so a file shape nobody wrote a rule for still loads its floor | the same table — floors first, specific routes add to them rather than replace them | Gate |
| A vendored hook or script still matches upstream; a hook is wired in both harnesses or in neither as its `delivery` says, a script in neither | `.agents/hooks/tests/test_vendored_hooks.py`, run by the `hook-tests` CI job | Yes |
| A review loads the same standards the author was required to load | `skill_router.py --skills-for` over the same table, run by skill `review` Step 2 | Gate |

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
