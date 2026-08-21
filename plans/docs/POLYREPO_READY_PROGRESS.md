# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Fix/CrossHarnessStandardsDelivery`
- Branch: `Docs/docs_polyrepo-ready-cross-harness-delivery`, from `origin/main` at `1e26f8244`, layered on
  N2 consumer branch `Docs/docs_polyrepo-ready-route-table-convention` at `a3bdd42e8`.
- PRs: N2 producer **agent-standards #12 MERGED** (`d175b8b`), N2 consumer **this repo #695 MERGED**
  (`2650f5c`); cross-harness producers **agent-standards #13 MERGED** (`e685095`), **dotagents #3 MERGED**
  (`e5ca9cc`), **react-agents #1 MERGED** (`c9a973d`); consumer **this repo #696 — open**.
- Dependency/package gates: none. This diff touches no `api/**` path → no publish, no `chore/platform-sync-*`.
  The current machine is provisioned from the merged GitHub marketplaces and verified for both harnesses.
- Last reconciled: 2026-08-21, after merging N2 (#12 + #695), all three cross-harness producers, and
  refreshing both harnesses from those merged revisions. **Only consumer #696 remains in this delivery.**

## Current state

**Phase 1 and all of N1 (six families, 28 skills) are merged in both repos.** `main` is at `1e26f8244`,
agent-standards `main` at `e8fd22f` (includes family-6 producer #11). Family 6 (package-cutover) landed as
producer #11 + consumer #693; `package-cutover` homed in the **dotnet** plugin as `dotnet:package-cutover`.

**N2 — the route-table convention — is merged on both sides.**

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

**Cross-harness standards producers are merged and provisioned from GitHub.** Claude Code and Codex each have
all five standards plugins enabled at user scope. Both independently resolve all 54 unique skills named by
Concertable's live route table. The router now resolves only against the active harness and keeps every write
blocked when an owning skill is absent; a stale cache in the other harness can no longer create a false pass.

**`auto-memory` stays in-repo temporarily.** Its old blocker is gone because Codex now loads agent-standards;
criterion 1 still requires a durable home for this Codex-only utility before close-out.

## Next Steps

1. **Land consumer #696 through the normal queue.** Its net diff includes executable
   `scripts/provision-agent-standards.ps1`, so it is not meta-only. No end-to-end positive trigger applies;
   use `skip-e2e`, retain the hard-floor build/hooks, and close this plan-managed worktree after merge.

2. **Start new Claude and Codex sessions.** The installed catalogues now point at the merged producer
   revisions; existing sessions retain the snapshot they started with.

3. **N3–N6 + N7a next** — N1/N2 no longer block them. N6 still carries the open question for Tommy:
   `OVERVIEW.md`, `USP.md`, `DEEP_RESEARCH_PROMPT_GUIDE.md` are product narrative, neither platform standard
   nor service-specific — surface, don't invent a home. **N7b** waits on roadmap §4c; **N8** last as the only
   carved-repo evidence; it repeats the already-delivered Claude/Codex checks against the carved table. The
   **frontend carve seam** (roadmap §6/§4c) now also
   gates the generator's `react-app` kind — an input N8 needs before a carved frontend repo can be proven.

## Completed work

- **Cross-harness prerequisite — producer PRs merged and machine reprovisioned.** agent-standards #13 adds
  Codex manifests for `agent-process`, `dotnet`, and `react`, a one-command Claude/Codex provisioner, and the
  active-harness fail-closed router with an all-route installation verifier. dotagents #3 and react-agents #1
  add their Codex marketplace/plugin schemas. This consumer vendors the router, provisioner, and provenance.
  All five plugins are installed from the merged GitHub marketplaces and enabled in both harnesses on this
  machine; all 54 routed skills resolve independently in each harness.
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

## Verification — cross-harness delivery

- Provisioner full run and `-VerifyOnly -Repository <worktree>`: Claude **5/5** plugins enabled, Codex
  **5/5** enabled; each harness resolves all **54/54** unique skills named by the live table.
- Fresh Codex CLI session loaded both `dotnet-standards:unit-testing` and `dotnet:unit-testing`, followed
  both routers to their shipped `UNIT.md` payloads, and returned the expected smoke marker.
- Claude's installed component inventory exposes `unit-testing` in both `dotnet-standards` (22 skills) and
  `dotnet` (16 skills). The Claude plugin list shows all five standards plugins enabled at user scope.
- Producer combined hook/generator suite **177/177**; consumer vendored-hook suite **19/19**. Plan graph **0 errors / 0
  warnings**; docs reachability **0 errors / 24 pre-existing warnings**.
- All three generated corpora are current: agent-standards **62 skills / 62 docs** (196 checked), dotagents **77 files**,
  react-agents **43 files**. Claude validates all marketplaces/plugins; Codex validates and installs all
  five `.codex-plugin` manifests.

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
- **The stale/reduced plugin-cache gap is repaired on this machine.** Both harnesses now have the complete
  five-plugin set at user scope, and the route verifier resolves all 54 live skills independently in each.
  New sessions are required to load the refreshed catalog; N2's new `skill-routes` skill arrives after #12.
- **Durable cross-slice rules that still bind N3–N8:**
  - **Collision-check a new skill name across *every* repo on the machine**, not just the standards repos and
    the harness built-ins — the family-2/3/4 lesson (`create-gh-pr`, `sync`, `worktree` all collided).
  - **No values file — resolve per-repo values at run time** (discovery, a script's own usage, or a genuine
    constant). Reaching for a values file is evidence the discovery mechanism has not been found.
  - **Commit+push the irreversible core of a slice before the longer ledger prose** — concurrent sessions
    prune worktrees here, and a family-6 worktree was `rm`'d mid-authoring with uncommitted work in it.
  - **Cross-harness completeness is a per-slice gate now, not deferred to N8.** Run the provisioner's
    repository verification for Claude and Codex whenever route ownership changes. N8 repeats it against the
    carved service rather than implementing delivery for the first time.

## Resume prompt

```
cd C:/Users/TommySeery/source/repos/Concertable.worktrees/Fix/CrossHarnessStandardsDelivery
Read @plans/docs/POLYREPO_READY_PLAN.md and @plans/docs/POLYREPO_READY_PROGRESS.md and do what its `## Next Steps` says.
```
