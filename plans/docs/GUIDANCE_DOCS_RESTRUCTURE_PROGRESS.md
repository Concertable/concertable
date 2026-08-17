# Guidance-docs restructure — progress

- Plan: `plans/docs/GUIDANCE_DOCS_RESTRUCTURE_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/guidance-restructure`
- Also delivered by this ledger: roadmap item `docs/agent-standards`, now checked off
- Worktree: `C:\Users\TommySeery\source\repos\Concertable\.worktrees\Docs-guidance-docs`
- Branch: `Docs/GuidanceDocsRestructure`
- PR: #637 (draft)
- Shared repos: `Concertable/agent-standards` (7 process skills) and `tomjseery/dotagents` (28 generic, synced to `~/.agents/skills/`), cloned at `C:\Users\TommySeery\source\repos\{agent-standards,dotagents}`
- Dependency/package gates: none — docs and one hook only; no `api/**` code change, so no package publication or platform sync
- Last reconciled: 2026-08-17 against `origin/main` (merged in, 0 behind), plus `agent-standards` `8c42daa`

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

None recorded for Phase 3b yet. This is a docs/meta-only change with no `api/**` code in it, so its gate is
`/review` on PR #637 followed by `/merge-docs` — the no-E2E docs path.

## Next Steps

1. **Review and land this PR.** Run `/review` against #637, address findings, then `/merge-docs`. Docs-only,
   no platform-sync gate.
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
6. **Deferred to its own PR:** auto-load thinning of root `AGENTS.md` (the 86 merge lines and 32 Docker lines
   that `/merge` and `scripts/e2e.ps1` already automate), the analyzer push-down plus
   `EnforceCodeStyleInBuild`.

## Also Tommy's, not blocking

`tomjseery/agent-starter-kit` (public, 7 skills) looks redundant with `tomjseery/dotagents` (private, the
same 7 plus `pull-main`, `sync-all`, `unmerged`) — the same duplication disease at repo level.
