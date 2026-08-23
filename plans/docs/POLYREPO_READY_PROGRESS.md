# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: `Concertable.worktrees/Docs/polyrepo-ready-consumer-sync` — the consumer-sync worktree, off
  `origin/main` (`75b564bc9`, incl. #764 N8 evidence). N8 evidence worktree closed (merged #764).
- Branch: `Docs/polyrepo-ready-consumer-sync` (this branch); `Docs/*` (durable guidance, meta-only).
- PRs: **Phase 1 + N1–N7a all MERGED; N8 evidence MERGED ([#764](https://github.com/Concertable/concertable/pull/764)).**
  Producer follow-ups MERGED as **agent-standards [#34](https://github.com/Concertable/agent-standards/pull/34)**
  (generator CANONICAL → `\.ArchitectureTests`; two `PLANS.md` follow-ups; `auto-memory` home). **This branch**
  = the consumer sync (regen the route table + delete the in-repo `auto-memory` copies). Earlier: N7a #752,
  sweep #760, N6 #750, N5 #745 (+ agent-standards #25/#27), N4 #18/#19 + #715/#713, N3 #15 + #698 + #700, N2 #12
  + #695, N1 six families, Phase 1 #669 + #5.
- Standards install: reprovisioned to **`1aefd60`** (#34 merged) — both harnesses at 5/5 plugins; the fixed
  generator and the `auto-memory` skill are installed. The earlier `13fcef1c0`-vs-`ab1755d` gap is closed.
- Dependency/package gates: none open. Meta-only — edits `plans/docs/*` + `.agents/skill-routes.json` + the
  `auto-memory` skill deletion; no `api/**`, so no publish / `chore/platform-sync-*`.
- Last reconciled: **2026-08-23** — N8 evidence merged (#764); both session decisions taken and delivered
  (agent-standards #34 merged, machine reprovisioned); this branch regenerates the route table (restoring
  `gen --check`) and removes the in-repo `auto-memory` copies now the plugin ships it.

## Current state

**Every node is landed: Phase 1 + N1–N7a re-homed, N8 evidence recorded (#764), and both session decisions
delivered.** Finding A (generator drift) is fixed at the source — agent-standards #34 repointed the generator
CANONICAL to `\.ArchitectureTests` + module-structure — and `auto-memory` now lives in the agent-process
plugin. The machine is reprovisioned to `1aefd60`. **This branch closes the consumer loop:** the route table
is regenerated from the fixed generator (so `gen --check` is clean again — the N2 invariant restored), and the
in-repo `auto-memory` copies are deleted now the plugin ships it.

Only close-out housekeeping and the separately-gated work remain (see `## Next Steps`). N7b (relocating the
plan *documents*) stays gated on roadmap §4c/§6 (the frontend-carve seam) — **do not attempt N7b.**

## Next Steps

All producer and consumer work for the epic body is done; nothing is blocked. What is left is close-out and
separately-gated work only.

1. **Close-out housekeeping (final epic close-out).** `git rm`
   `reviews/Docs-docs_polyrepo-ready-review-sweep.md` and `reviews/Docs-docs_polyrepo-ready-n8-carve-evidence.md`
   (spent review records of merged PRs), then delete this plan + ledger together per the `plans` lifecycle,
   landed as a docs-only close-out. Do this only once N7b and the cut are also resolved, or when Tommy calls
   the epic body done.
2. **N7b (gated, do not attempt).** Relocating the plan *documents* waits on roadmap §4c/§6 (the frontend-carve
   seam), which also gates the generator's `react-app` kind.
3. **Endpoint reminder.** Root `AGENTS.md`/`CLAUDE.md` are deleted at the **cut** (separate `POLYREPO_ROADMAP`),
   not in this plan — this plan makes root hold zero shared canon.

## Verification — N8

Carved Payment tree = tracked `api/Concertable.Payment` subtree extracted as the repo root (`git archive
HEAD:api/Concertable.Payment | tar -x`), 410 files, `git init`. Table generated with the agent-standards
`gen_skill_routes.py --kind dotnet-service`; routing run with the live repo's real
`.agents/hooks/skill_router.py` from inside the carved tree.

**Routes fire (pass).**
- Generated table: **30 rows** (= 39 monorepo − 9 react rows; the `^api/` area floor loses its prefix; **no
  row names a path outside the service** — no `^api/`, `^app/`, or `app/` mid-pattern).
- `skill_router.py --skills-for` resolves the expected skill(s) for a representative path per family:
  `.Api`+`Controller.cs` → `http-api`/`result-terminals`; `.Application`+`Repository.cs` →
  `result-carriers`/`module-structure` + `persistence`/`multitenancy`; `.Domain`(+`/Events/`) →
  `domain-events`/`result-errors`; `.Infrastructure` + `DbContext`/`Seeder`/`Migrations` →
  `persistence`/`dependency-injection` + `multitenancy`/`seeding`/`migrations`; `AppHost/Program.cs` →
  `microservice-boundaries`; test `.csproj` (via the `IsTestProject` content gate) →
  `unit-testing`+`integration-testing`; `Directory.Build.props` → `packages`; `AGENTS.md`/`TECH_DEBT.md` →
  `docs-and-debt`.
- **Carve-safe negative confirmed:** the service's own `Concertable.Payment.Contracts/*.cs` triggers **only**
  the floor, never the universal `Concertable\.(Kernel|Contracts)/` shared-tier row — in a carved repo those
  are consumed as published packages, so the row correctly fires on nothing.
- Coverage sweep over all **396** routable carved files: **0** non-trivial `.cs` unrouted (the floor catches
  every one), **0** deny-pattern violations. Equivalence vs the live monorepo table (same files under the
  `api/Concertable.Payment/` prefix): identical skill set **except** the ArchitectureTests drift (finding A).

**Skills resolve (pass).** `skill_router.py --verify-install claude` in the carved tree resolves **all 37**
routed skills; `provision-agent-standards.ps1 -VerifyOnly -Repository <carved>` exercises the same check
against the installed plugins.

**Findings.**
- **A — generator drift (real defect; the finding N8 exists to catch).** The live
  `.agents/skill-routes.json` carries `\.ArchitectureTests[^/]*/.*\.cs$` →
  `[composition-testing, dotnet-standards:module-structure, dotnet:module-structure]`, hand-added by the
  architecture-tests-rename epic (its `PLAN.md` step 4, repointing the tier from Composition→Architecture and
  adding module-structure). The agent-standards generator CANONICAL — in **both** the installed `13fcef1c0`
  and current `ab1755d` — still emits `\.CompositionTests[^/]*/.*\.cs$` → `[composition-testing]` only. So
  `gen --kind monorepo` no longer reproduces the live table (the N2 invariant is broken), and a carved
  Payment repo would route `PaymentArchitectureTests.cs` to only the floor, **losing composition-testing +
  module-structure**. Untracked by either epic. Fix owned here — see `## Next Steps` 1.
- **B–D — carve-time doc deltas (expected, not defects now).** `README.md:11,29` describe development "in the
  monorepo"; Payment `AGENTS.md:3` links `../../AGENTS.md` (the root/api floor); the E2E-helpers
  `tests/E2ETests/Concertable.Payment.E2ETests.Helpers/AGENTS.md:3` links
  `../../../../Concertable.Shared/tests/Concertable.Testing.E2E/AGENTS.md`. All three are **correct for a
  service still living in the monorepo** and resolve at the cut, when each service takes its own floor —
  consistent with the endpoint (root files deleted at the cut, not in this plan). They are the bounded
  doc-delta a carve applies, and are recorded here so the cut inherits a known list rather than rediscovering
  it.

**Verdict.** Routing and skill-resolution criteria **pass** for the carved Payment table; the "no doc links
outside / no doc asserts a monorepo" criteria are met **modulo the expected carve-time deltas B–D**; one real
defect (A) surfaced and is owned for a fix. N8 evidence is complete; no code was edited (evidence-only, as
scoped).

## Completed work (compact — full detail is in git history / spent ledgers)

- **N8 evidence + generator-drift fix + `auto-memory` home.** N8 recorded and MERGED (#764). Both session
  decisions delivered: agent-standards **#34** (MERGED) repointed the generator CANONICAL to `\.ArchitectureTests`
  + module-structure (matching the live table), added the two `PLANS.md` follow-ups, and homed `auto-memory`
  in the agent-process plugin (router+doc). Machine reprovisioned `13fcef1c0` → `1aefd60`. **Consumer sync
  (this branch):** regenerated `.agents/skill-routes.json` from the fixed generator — `gen --check` clean,
  route SET provably unchanged (cosmetic reorder + `_comment` only) — and `git rm`'d the in-repo
  `auto-memory` copies (`.agents/skills`, `.claude/skills`) now the plugin ships it.
- **N7a — `plans/AGENTS.md` thinned 68 → 31 lines. MERGED (#752).** Consumer-only: an independent rule-by-rule
  mapping confirmed every platform rule was already homed (Phase 1 + N1), so the file thinned to this repo's
  values (layout, hook/script paths, suite names) + pointers to the owning skills.
- **Spent-review sweep. MERGED (#760).** All merged-PR `Docs-docs_polyrepo-ready-*` review files removed;
  `reviews/Docs-docs_polyrepo-ready-review-sweep.md` (the sweep's own record) intentionally left for final
  close-out.
- **N6 — `docs/` product narrative → private `Concertable/docs`. MERGED (#750).** `INDEX.md` kept as this-repo
  index; `REMOTE_VALIDATION.md` kept as a per-repo doc.
- **N5 — root `AGENTS.md` thinned 149 → 23 lines. MERGED (#745).** Rules re-homed to the plugins; producer
  agent-standards #25 (`FLOOR.md` + `session_floor.py` SessionStart hook) + #27 (router fails CLOSED).
- **N4 — `api/ARCHITECTURE.md` + `api/docs/MICROSERVICES_ARCHITECTURE.md` re-home. Terminal.** Producer #18/#19,
  consumer #715/#713; sync cascade to `.1128` merged non-breaking.
- **N3 — `api/AGENTS.md` re-home. Terminal.** Producer #15, consumer #698 + #700; shared-is-the-intersection
  rule homed in `SERVICE_BOUNDARIES.md`, route table 37 → 38.
- **N2 — route-table convention.** Producer #12: `SKILL_ROUTES.md` + carve-time generator `gen_skill_routes.py`;
  consumer #695.
- **N1 — six families / 28 skills.** review, merge/PR, test-debug, git, plan-workflow, package-cutover
  (`auto-memory` deferred). **Cross-harness prerequisite** delivered (both harnesses resolve all routed skills
  independently).
- **Phase 1** (#669 + #5): plan method into `PLANS.md`; `HANDOFF.md` new.

## Reviews

- **N8 evidence (#764)** — docs-reviewed clean, merged; review record
  `reviews/Docs-docs_polyrepo-ready-n8-carve-evidence.md` (spent — delete at final close-out).
- **agent-standards #34** — independently reviewed clean (one INFO, NAT1: `gen --check` still stale on cosmetic
  divergences, owned by the consumer regen on this branch); recorded in agent-standards
  `reviews/Fix-polyrepo-carve-followups.md`; MERGED.
- **This branch (consumer sync)** — meta-only (`plans/docs/*` + `.agents/skill-routes.json` regen + the
  `auto-memory` skill deletion); delivery route `/docs-review` then `/merge-docs`.

## Decisions, discoveries, blockers, and deviations (durable — still bind close-out)

- **N8 discovered generator drift (finding A) — FIXED (agent-standards #34) + consumer regen (this branch).**
  The architecture-tests-rename epic hand-edited the live consumer route table (its `PLAN.md` step 4) but never
  propagated the change to the agent-standards generator CANONICAL — the exact copy-and-drift failure this epic
  exists to kill. #34 repointed the CANONICAL; this branch regenerated the live table, so the N2 invariant
  ("`gen --kind monorepo` reproduces the live table exactly") is **restored** (`gen --check` clean). Route SET
  was unchanged throughout — the live table already routed correctly; only `gen --check` was broken.
- **The two-destination test is the design, not "does it mention Concertable".** Confirmed again by N8: every
  Payment doc that mentions the monorepo or links outside the service is either a value or a carve-time delta,
  never a rule with no home.
- **`auto-memory` homed** in the agent-process plugin (agent-standards #34) as a router+doc, and its in-repo
  copies deleted here — criterion 1 satisfied for this Codex-only utility.
- **Durable cross-slice rules that still bind close-out:**
  - **Collision-check a new skill name across *every* repo on the machine** (the family-2/3/4 lesson).
  - **No values file — resolve per-repo values at run time** (the generator parameterises only the floor
    anchor by `--kind`; it does not read a per-repo variable).
  - **A meta-only consumer ADMIN-MERGES via `/merge-docs`, never `--auto`** — the queue runs E2E even on a meta
    diff inside `merge_group`.
  - **Commit+push the irreversible core of a slice before the longer ledger prose** — concurrent sessions prune
    worktrees here.
  - **Checkpoint the ledger on the delivery branch** before the worktree close, so `worktrees.ps1 close
    -PlanManaged` anchors normally.
  - **Cross-harness completeness is a per-slice gate** — run the provisioner's repository verification for
    Claude and Codex whenever route ownership changes.

## Resume prompt

```
cd C:/Users/TommySeery/source/repos/Concertable
Read @plans/AGENTS.md, @plans/docs/POLYREPO_READY_PLAN.md, and @plans/docs/POLYREPO_READY_PROGRESS.md.
The epic body is DONE: Phase 1 + N1–N7a re-homed; N8 evidence merged (#764); the generator drift is fixed
(agent-standards #34) and the live route table regenerated so `gen --check` is clean; auto-memory is homed in
the agent-process plugin and its in-repo copies deleted; machine reprovisioned to 1aefd60. Only close-out and
gated work remain (ledger `## Next Steps`): at final close-out, git rm the spent review-sweep +
n8-carve-evidence review files and delete this plan+ledger; N7b stays gated on §4c/§6 (do not attempt); root
AGENTS.md/CLAUDE.md are deleted at the cut, not in this plan.
```
