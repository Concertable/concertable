# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready-route-table-convention`
- Branch: `Docs/docs_polyrepo-ready-route-table-convention`, from `origin/main` at `1e26f8244` — N2, route-table convention.
- PR: producer **agent-standards #12 — open** (`verify` running); consumer **this repo #695 — open** (meta-only).
- Dependency/package gates: none. This diff touches no `api/**` path → no publish, no `chore/platform-sync-*`.
  **Plugin-cache refresh is pending** — the family-5 plan skills, `plan-checkpoint`, `package-cutover`
  (`dotnet:package-cutover`), and now `skill-routes` resolve under no name until the installed cache carries
  them. A **three-slice** backlog (families 5 + 6 + N2).
- Last reconciled: 2026-08-21, after landing N1 in full (family 6 = #11 + #693 MERGED) and authoring N2
  (agent-standards #12 + this branch). **N1 is complete; N2 is authored.**

## Current state

**Phase 1 and all of N1 (six families, 28 skills) are merged in both repos.** `main` is at `1e26f8244`,
agent-standards `main` at `e8fd22f` (includes family-6 producer #11). Family 6 (package-cutover) landed as
producer #11 + consumer #693; `package-cutover` homed in the **dotnet** plugin as `dotnet:package-cutover`.

**N2 — the route-table convention — is authored on both sides.**

- **Producer (agent-standards #12):** the route table is per-repo data, but the *convention* its rows follow
  was homeless after the cut deletes the monorepo root (it lived only in the table's `_comment` and
  `docs/INDEX.md`). Landed as `standards/process/SKILL_ROUTES.md` (skill **`skill-routes`**, agent-process
  plugin) + a carve-time generator `.agents/gen_skill_routes.py` + its gate test. The generator carries the
  canonical rows once, tagged by group; `--kind dotnet-service` emits a carved service's table by re-anchoring
  the one `.cs` area floor to the repo root and dropping the react rows, `--kind monorepo` reproduces the
  platform's own table. **`react-app` is refused** until the frontend carve seam (roadmap §6/§4c) is decided —
  the react rows carry `app/` mid-pattern, so generating one now would name paths that repo does not have.
- **Consumer (this branch):** the table's `_comment` shrinks from restating the model + field semantics to a
  pointer at the convention home, per one-rule-one-home. Routes unchanged (37); comment-only.

**Generator/template decision (Tommy): generator.** "Run a script, get a correct table" is the only shape the
eight carved repos genuinely cannot drift from — the table is generated once at carve time and committed, so
every clone has the conventions wired from the first commit and nobody hand-edits. A template still relies on
correct hand-editing per repo.

**`auto-memory` stays in-repo, by decision** (see `## Decisions`): a Codex-only feature toggle Codex could no
longer resolve if moved to a plugin it does not load.

## Next Steps

1. **Land N2 — producer agent-standards #12 first, then this branch's consumer PR.** The producer must merge
   first: the consumer's `_comment` points at the convention, so a consumer merged alone points at a doc not
   yet on `main`. #12 is a standards-only diff → merges when `verify` is green (that repo has no queue; it
   merges directly). This branch is **meta-only** (`.agents/**`, `plans/**`) → the `/merge-docs` admin-merge
   path (bypass the queue; the queue runs E2E on a normal enqueue even for meta-only — see `## Decisions`).
   Producer #12, consumer #695:
   ```bash
   gh -R Concertable/agent-standards pr merge 12  --merge --delete-branch
   gh -R Concertable/concertable    pr edit  695 --add-label skip-e2e
   gh -R Concertable/concertable    pr merge 695 --merge --admin
   ```
   Then `git checkout main && git pull --ff-only origin main` and close this worktree with
   `./scripts/worktrees.ps1 close -Worktree <path> -PullRequest 695 -PlanManaged`.

   **Merge authorization:** Tommy runs the merges (or approves interactively). The classifier stays the hard
   gate; no blanket `gh pr merge` permission was added.

2. **Refresh the plugin cache** — Tommy's one command, now a three-slice backlog: the family-5 plan skills,
   `plan-checkpoint`, `dotnet:package-cutover`, and `skill-routes` resolve under no name until the installed
   cache carries them.

3. **N3–N6 + N7a next** — N1/N2 no longer block them. N6 still carries the open question for Tommy:
   `OVERVIEW.md`, `USP.md`, `DEEP_RESEARCH_PROMPT_GUIDE.md` are product narrative, neither platform standard
   nor service-specific — surface, don't invent a home. **N7b** waits on roadmap §4c; **N8** last as the only
   evidence, and must include the Codex delivery gap. The **frontend carve seam** (roadmap §6/§4c) now also
   gates the generator's `react-app` kind — an input N8 needs before a carved frontend repo can be proven.

## Completed work

- **N2 producer — agent-standards #12.** `standards/process/SKILL_ROUTES.md` (the convention) routed by the
  new `skill-routes` skill; `.agents/gen_skill_routes.py` (the generator, canonical rows embedded once);
  `.agents/hooks/tests/test_gen_skill_routes.py` (11 tests). Generator current at **62 skills / 62 docs** (197
  files), hook suite **161 → 172**, plugin router rewrote to a valid relative path, description colon-space
  clean, INDEX row present, `skill-routes` collision-free across every repo on the machine.
- **N2 consumer — this branch.** `.agents/skill-routes.json` `_comment` repointed to the convention; routes
  unchanged (37), JSON valid, router resolves representative paths. Committed and pushed early (worktree-prune
  lesson). Plan + ledger updated.
- **N1 — all six families, merged both repos.** Family 1 review (#675 + agent-standards #6): seven `review/`
  docs. Family 2 merge/PR (#676 + #7): four `merge/` docs, `create-gh-pr` → `open-pr`. Family 3 test-debug
  (#677 + #8): six `testing/` docs, `docker-health.ps1` vendored. Family 4 git (#679 + #9): six `git/` docs,
  `sync` → `sync-checkout`, `worktree` → `open-worktree`, `worktrees.ps1` vendored. Family 5 plan-workflow
  (#687 + #10): `plan/RESUME.md`, `plan/CONTINUE_ROADMAP.md`, `plan/UPDATE_ROADMAP.md`, `TECHDEBT.md`, plus a
  new `plan-checkpoint` skill (`plan/CHECKPOINT.md` with the ledger template folded in). Family 6
  package-cutover (#693 + #11): `standards/dotnet/PACKAGE_CUTOVER.md` in the **dotnet** plugin.
- **Phase 1** (#669 + agent-standards #5): the plan method into `PLANS.md`; `HANDOFF.md` new.

## Verification — N2

Producer (agent-standards #12):
- **Faithfulness (the strong proof):** `build_routes("monorepo")` reproduces Concertable's live 37-row table
  **exactly** — paths, skills, `content_requires`, `deny`, and notes all match (structural diff against the
  real `.agents/skill-routes.json`). The canonical rows are the real rows, not a paraphrase.
- **Carve (`dotnet-service`):** 28 rows (4 meta + 24 dotnet), **zero `^api/`/`^app/`/`app/` leakage**, the
  `.cs` floor re-anchored to root (`^(?!.*(GlobalUsings|AssemblyInfo)\.cs$).*\.cs$`) and still excluding
  `GlobalUsings`. Layer/name/test-tier/deny rows all port and fire through the **real** `skill_router`
  matcher on a simulated carved Payment tree — every tracked path covered.
- Hook suite **172 passed** (161 + 11 new). `sync-generated.ps1 -Check` current (62 skills / 62 docs, 197
  files). Plugin router → `../../standards/process/SKILL_ROUTES.md`. `skill-routes` collision-free across
  `~/.claude`, `~/.agents`, `~/.codex`, `dotagents`, `react-agents`, `agent-starter-kit`, the work repos.

Consumer (this repo):
- `.agents/skill-routes.json` parses; 37 routes intact; `skill_router.py --skills-for` resolves
  `csharp-*`/`dotnet:persistence`/`plans`/`react-standards:typescript-style` on representative paths.
- **Meta-only holds** — changed top-level paths are `.agents`, `plans`. No `api/**`, no workflow file → the
  `/merge-docs` path, no publish/`chore/platform-sync-*`.

## Reviews

**N2 — low review surface, self-checked; no review file yet.** The producer doc states the convention that
already lived (verbatim in substance) in the table's `_comment` and `docs/INDEX.md`, now with one home; the
generator is proven by the committed gate test + the exact-reproduction faithfulness check; the consumer is a
comment-only edit. A `/docs-review` over both halves can be run from the moved copy
`standards/process/review/DOCS.md` before merge if wanted, as prior slices did while the cache is stale.

## Decisions, discoveries, blockers, and deviations

- **Generator, not template (Tommy confirmed).** The only shape the eight carved repos cannot drift from:
  generate the table once at carve time and commit it. The generator is the routing-table analog of
  `vendor-hooks.ps1` — a carve-time authoring tool at `.agents/gen_skill_routes.py`, run `--into <repo>`, not
  vendored into every consumer (a table, once committed, needs no per-clone regeneration).
- **The react rows genuinely cannot be carved yet — surfaced, not papered over.** For a .NET service only the
  one `.cs` area floor is location-keyed; every layer/name/meta row ports verbatim. The react rows are
  different: they carry `app/` *mid-pattern* (`app/.*/api/…`), not just as a prefix, so a carved frontend
  repo's table depends on whether an `app/` node survives the cut — undecided (roadmap §6/§4c). The generator
  therefore **refuses `--kind react-app`** with that reason rather than emitting wrong rows. This is a new
  input the frontend carve seam owes, folded into N8's dependencies.
- **Consumer scope kept minimal by design.** N2 requires the convention published + the generator + the gate;
  it does **not** require Concertable to regenerate its own table or wire `--check` into CI. Making the
  monorepo table a `--check`-guarded generator output is an available follow-up (it would reformat the table
  and touch CI), deliberately out of this slice. The faithfulness proof already demonstrates the generator
  against the real repo without that churn.
- **No values file — resolved at run time again.** The generator embeds the canonical rows once and
  parameterises only the floor anchor by `--kind`; there is no per-repo values file, seventh slice running.
- **A meta-only consumer must ADMIN-MERGE via `/merge-docs`, never `--auto`** — the merge queue runs E2E on a
  normal enqueue even for a meta-only diff (inside `merge_group` the path-filter has no diff base, so E2E does
  not skip). This bit #687 (a 17-min UI-E2E run that then fell out). `/merge-docs` admin-merges to bypass the
  queue, with `skip-e2e` as belt-and-braces.
- **The installed plugin cache holds stale/reduced snapshots** — a prior session resolved `docs-and-debt` from
  a reduced snapshot lacking the executable skills, so `docs-review` et al. are not invocable as skills here;
  run reviews from the moved copy on disk. The refresh (Next Steps 2) is Tommy's.
- **Durable cross-slice rules that still bind N3–N8:**
  - **Collision-check a new skill name across *every* repo on the machine**, not just the standards repos and
    the harness built-ins — the family-2/3/4 lesson (`create-gh-pr`, `sync`, `worktree` all collided).
  - **No values file — resolve per-repo values at run time** (discovery, a script's own usage, or a genuine
    constant). Reaching for a values file is evidence the discovery mechanism has not been found.
  - **Commit+push the irreversible core of a slice before the longer ledger prose** — concurrent sessions
    prune worktrees here, and a family-6 worktree was `rm`'d mid-authoring with uncommitted work in it.
  - **The Codex delivery gap is real and must be N8's concern:** every plugin-delivered standard resolves in
    Claude Code only; Codex has `agent-standards`/`dotagents`/`react-agents` registered nowhere. `auto-memory`
    is the first skill whose *value* the gap changes, not just its delivery.

## Resume prompt

```
cd C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready-route-table-convention
Read @plans/docs/POLYREPO_READY_PLAN.md and @plans/docs/POLYREPO_READY_PROGRESS.md and do what its `## Next Steps` says.
```
