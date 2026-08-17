# Guidance index — topic → owning doc

Every rule has **one** owning doc. Look the topic up here before writing a rule down; if it already has
an owner, add it there and link from wherever else it matters. A second copy of a rule is a bug, not
emphasis — the copies drift, and the reader can't tell which one is current.

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
| Protocol selection — gRPC / HTTP / Service Bus | [`api/agents/MICROSERVICE_COMMUNICATION.md`](../api/agents/MICROSERVICE_COMMUNICATION.md) |
| Design rationale and decision history (not current state) | [`api/docs/MICROSERVICES_ARCHITECTURE.md`](../api/docs/MICROSERVICES_ARCHITECTURE.md) |
| Per-service specifics | that service's own `AGENTS.md` / `ARCHITECTURE.md` |

## Backend code (`api/`)

| Topic | Owner |
|---|---|
| C# style and naming — fields, ctors, braces, suffixes, mappers, `#region` | [`api/agents/CODE_CONVENTIONS.md`](../api/agents/CODE_CONVENTIONS.md) |
| DTO naming — `Response` is HTTP-only, `Dto` disambiguates | [`api/agents/CODE_CONVENTIONS.md`](../api/agents/CODE_CONVENTIONS.md) "DTO naming" |
| Logging — source-generated `Log.cs`, probes included | [`api/agents/CODE_CONVENTIONS.md`](../api/agents/CODE_CONVENTIONS.md) "Logging" |
| Validator tool choice — FluentValidation vs `ValidationResult` | [`api/agents/CODE_CONVENTIONS.md`](../api/agents/CODE_CONVENTIONS.md) "Validators" |
| Structural patterns — tenancy composition, keyed strategies, DI, dependency-holders, Refit, unit of work | [`api/agents/CODE_PATTERNS.md`](../api/agents/CODE_PATTERNS.md) |
| Project layering, reference graph, visibility cascade, folder layout, cross-module rules | [`api/agents/CONVENTIONS.md`](../api/agents/CONVENTIONS.md) |
| Result, Option, typed errors, validation, transport terminals | [`api/agents/RESULT_PATTERN.md`](../api/agents/RESULT_PATTERN.md) |
| Seeding — drive the trigger, never write the row | [`api/agents/SEEDING_CONVENTIONS.md`](../api/agents/SEEDING_CONVENTIONS.md) |
| DTOs vs Responses at the controller boundary; migrations; shared-is-the-intersection | [`api/AGENTS.md`](../api/AGENTS.md) |
| Unit tests | [`api/agents/UNIT_CONVENTIONS.md`](../api/agents/UNIT_CONVENTIONS.md) |
| Integration tests | [`api/agents/INTEGRATION_CONVENTIONS.md`](../api/agents/INTEGRATION_CONVENTIONS.md) |
| E2E scenario authoring | [`api/agents/E2E_CONVENTIONS.md`](../api/agents/E2E_CONVENTIONS.md) |
| Adding a logger while investigating | [`api/agents/DEBUGGING_CONVENTIONS.md`](../api/agents/DEBUGGING_CONVENTIONS.md) |

## Frontend code (`app/`)

| Topic | Owner |
|---|---|
| The sharing tiers and the build gate | [`app/AGENTS.md`](../app/AGENTS.md) |
| TS/React conventions — naming, casing, `interface` vs `type`, contract types | [`app/agents/CODE_CONVENTIONS.md`](../app/agents/CODE_CONVENTIONS.md) |
| Frontend patterns — slots over role checks, hooks orchestrate, state homes, zod boundary, table dispatch | [`app/agents/CODE_PATTERNS.md`](../app/agents/CODE_PATTERNS.md) |
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
| Docs are reachable; `CLAUDE.md` siblings exist | `.agents/hooks/docs_reachability.py` via `docs-review` | Gate |
| Plan handoff ends with its continuation pointer | `.agents/hooks/plan_handoff_stop.py` | Gate |

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
7. **Links are repo-relative.** A root-absolute `/api/...` path renders broken and silently satisfies
   the reachability hook without pointing anywhere.
8. **Check the code before you write the rule down.** Several rules here taught things the codebase had
   already moved past, and every one of them read as maintained.
