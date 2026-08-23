# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: `Concertable.worktrees/Docs/docs_polyrepo-ready-n8-carve-evidence` — the N8 evidence worktree, off
  `origin/main` (`fb561acee`, incl. #752 N7a + #760 review sweep).
- Branch: `Docs/docs_polyrepo-ready-n8-carve-evidence` (this branch); `Docs/*` (durable guidance, meta-only).
- PRs: **Phase 1 + N1–N7a all MERGED.** N7a = [#752](https://github.com/Concertable/concertable/pull/752)
  (thin `plans/AGENTS.md`); spent-review sweep = [#760](https://github.com/Concertable/concertable/pull/760).
  Earlier: N6 #750, N5 #745 (+ agent-standards #25/#27), N4 #18/#19 + #715/#713, N3 #15 + #698 + #700, N2 #12
  + #695, N1 six families, Phase 1 #669 + #5, cross-harness prerequisite. **N8 = this branch** (evidence-only,
  no code edits).
- Standards install: last reprovision **`13fcef1c0`**; agent-standards `main` is now
  **`ab1755d`** (#20 plans-consumption-contract merged) — ahead of the install. The gap is material: it is
  what surfaced the N8 generator-drift finding below, and closing it is part of the remaining debt.
- Dependency/package gates: none open. N8 is **evidence-only** — edits `plans/docs/*` (this ledger + plan +
  roadmap); no `api/**`, so no publish / `chore/platform-sync-*`.
- Last reconciled: **2026-08-23** — reconciled the ledger to merged reality (N7a #752, sweep #760 both merged;
  Phase 1 + N1–N7a all merged) and recorded **N8**. N8 proved the carved Payment table's routing/skills and
  surfaced one real defect (generator drift) plus the expected carve-time doc deltas — details below.

## Current state

**N7a is MERGED (#752); N8 (the last real milestone) is recorded on this branch as evidence.** Every re-home
node — Phase 1 and N1 through N7a — has landed on both sides. What remains before epic close-out is the small
debt in `## Next Steps`, and N7b (relocating the plan *documents*), which stays gated on roadmap §4c/§6 (the
frontend-carve seam) — **do not attempt N7b.**

**N8 — carved Payment service, standalone readiness (evidence, not edits).** The tracked
`api/Concertable.Payment` subtree was extracted as a simulated carved repo root (410 files, `git init`), its
table generated with `gen_skill_routes.py --kind dotnet-service`, and the real `skill_router.py` run against
it. Routing and skill-resolution criteria **pass**; the doc criteria are met modulo the expected carve-time
deltas; and N8 did its job — it surfaced the one place the carve tooling is wrong today (the generator drift,
finding A). Full evidence under `## Verification — N8`.

## Next Steps

Two decisions are owed to Tommy before the remaining debt can be cleared (both surfaced this session); neither
blocks the recorded N8 evidence, which is complete.

1. **N8 finding A — generator drift (a real defect, decision owed).** The live `.agents/skill-routes.json`
   carries a `\.ArchitectureTests` route the agent-standards generator does not emit (it still emits
   `\.CompositionTests`), so `gen --kind monorepo` no longer reproduces the live table and a carve would lose
   that tier's routing. The scalable fix: update the generator CANONICAL in **agent-standards** (rename the
   CompositionTests row → `\.ArchitectureTests` and add `module-structure`, matching the live table) and
   regenerate the live consumer table so `gen --check` is clean again. **Recommendation:** fold the producer
   half into a **single consolidated agent-standards PR** together with the two `PLANS.md` follow-ups below
   (now safe — #20 has landed), then a consumer regen worktree here. Pending Tommy's go-ahead on that bundling.
2. **`auto-memory` durable home (decision owed — ask Tommy).** Criterion 1 still requires a durable home for
   this Codex-only utility before close-out; the cross-harness delivery removed its former blocker but did not
   choose a destination.
3. **Two agent-standards `PLANS.md` follow-ups (non-blocking).** (a) the "opening/naming a `plans/*.md`
   obliges reading it before acting" obligation (only workflow-scoped in `RESUME.md`/`CHECKPOINT.md` today);
   (b) promote the standing-reference **bare-stem** naming from an assumed category to a stated one-line rule.
   Bundle with finding A into the consolidated agent-standards PR (never a third concurrent `PLANS.md` PR).
4. **Close-out housekeeping (at final epic close-out, not now).** `git rm`
   `reviews/Docs-docs_polyrepo-ready-review-sweep.md` (the sweep's own spent review record). N7b and the cut
   itself stay separate/gated (roadmap §4c/§6). Endpoint: root `AGENTS.md`/`CLAUDE.md` are deleted at the
   **cut**, not in this plan.

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

**N8 is evidence-only (no code diff)** — a `## Reviews` entry is still owed on the ledger diff for the local
merge gate; the delivery route is `/docs-review` then `/merge-docs` (meta-only: `plans/docs/*`). Prior nodes'
findings were all fixed on-branch before merge and their review files swept in #760 (N3–N7a) — reconstruct
from git history if needed.

## Decisions, discoveries, blockers, and deviations (durable — still bind close-out)

- **N8 discovered generator drift (finding A), untracked by either epic.** The architecture-tests-rename epic
  hand-edited the live consumer route table (its `PLAN.md` step 4) but never propagated the change to the
  agent-standards generator CANONICAL — the exact copy-and-drift failure this epic exists to kill, reintroduced
  one row at a time. The N2 invariant ("`gen --kind monorepo` reproduces the live table exactly") is currently
  **broken**. Fix owned here (Next Steps 1); the architecture-tests-rename epic is otherwise terminal.
- **The two-destination test is the design, not "does it mention Concertable".** Confirmed again by N8: every
  Payment doc that mentions the monorepo or links outside the service is either a value or a carve-time delta,
  never a rule with no home.
- **`auto-memory`** stays in-repo; criterion 1 still requires a durable home for this Codex-only utility before
  close-out (decision owed — Next Steps 2).
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
Phase 1 + N1–N7a are all MERGED; N8 evidence is recorded (carved Payment table routes/skills pass; one real
defect found — generator drift, finding A). Do what the ledger's `## Next Steps` says: (1) once Tommy
approves, fix finding A + the two PLANS.md follow-ups in one consolidated agent-standards PR, then regen the
live consumer table; (2) give auto-memory a durable home (ask Tommy); (4) delete the spent review-sweep file
at final close-out. N7b stays gated on §4c/§6 — do not attempt. Endpoint: root AGENTS.md/CLAUDE.md are
deleted at the cut, not in this plan.
```
