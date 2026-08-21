# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready-package-cutover-family`
- Branch: `Docs/docs_polyrepo-ready-package-cutover-family`, from `main` at `fbf011ac1` — N1 family 6, package-cutover.
- PR: producer **agent-standards #11 — open** (`verify` running); consumer this repo — branch pushed
  (deletion committed), plan+ledger edits to commit, then open its PR. Family 5 landed: producer #10 + consumer
  **#687 MERGED** (`main` at `fbf011ac1`).
- Dependency/package gates: none. This diff touches no `api/**` path → no publish, no `chore/platform-sync-*`.
  **Plugin-cache refresh is pending** (Next Steps 2) — the family-5 plan skills, the new `plan-checkpoint`, and
  now `package-cutover` resolve under no name until the installed cache carries them.
- Last reconciled: 2026-08-21, after landing family 5 (#10 + #687) and authoring family 6 (agent-standards
  #11 + this branch). **N1 is complete — all six families moved.**

## Current state

**Phase 1 and N1 families 1–5 are merged in both repos.** Review (#675 + agent-standards #6), merge/PR
(#676 + #7), test-debug (#677 + #8), git (#679 + #9) and plan-workflow (#687 + #10) all landed; `main` is
at `fbf011ac1` and agent-standards `main` at `b283d44`.

**N1 family 6 — package-cutover — is authored on both sides.** agent-standards **#11**:
`standards/dotnet/PACKAGE_CUTOVER.md` routed by the `package-cutover` skill, homed in the **dotnet** plugin
(not agent-process) — it is a .NET/NuGet/EF mechanic (CS7069, EF re-scaffold, `dotnet build`) irrelevant to
React repos, and the runnable counterpart to what `dotnet:packages` already owns. Cross-references
re-pointed to docs that exist in agent-standards: `PACKAGES.md` + `process/PLANS.md` (the why),
`data/MIGRATIONS.md` (re-scaffold), `process/plan/CHECKPOINT.md` (the plan-progress checkpoint) — replacing
`api/ARCHITECTURE.md` and the `plans` skill, neither reachable from a standards doc. This branch is the
consumer half: `.agents/skills/package-cutover/` + its `.claude` stub deleted. **No rename, no route row**
(invoked by name only), and root `AGENTS.md`/`docs/INDEX.md` never cited it — the only citations
(`docs/REMOTE_VALIDATION.md`, `PIPELINE_DEBT.md`) reference it by name, which survives the move untouched.

**`auto-memory` stays in-repo, by decision** (see `## Decisions`): a Codex-only feature toggle that Codex,
loading no agent-standards plugin, could no longer resolve if moved.

**N1 is complete — all 28 workflow skills moved across six families.** What remains: N2–N8.

## Next Steps

1. **Land family 6 — producer agent-standards #11 first, then this branch's consumer PR.** The producer
   must merge first, as every family: the consumer deletes the skill body, so a consumer merged alone leaves
   the procedure reachable under no name. #11 is a standards-only diff → merges when `verify` is green (that
   repo has no queue; it merges directly). This branch is **meta-only** (`.agents/**`, `.claude/**`,
   `plans/**`) → the `/merge-docs` admin-merge path, same as #687. Open this branch's PR, then:
   ```bash
   gh -R Concertable/agent-standards pr merge 11  --merge --delete-branch
   gh -R Concertable/concertable    pr edit  693 --add-label skip-e2e
   gh -R Concertable/concertable    pr merge 693 --merge --admin
   ```
   Then `git checkout main && git pull --ff-only origin main` and close this worktree with
   `./scripts/worktrees.ps1 close -Worktree <path> -PullRequest 693 -PlanManaged`.

   **Merge authorization:** Tommy runs the merges (or approves interactively). This session decided **not**
   to add a blanket `gh pr merge` permission rule — the classifier stays the hard gate; Tommy approves each
   merge. The agent cannot edit its own permission allowlist (classifier blocks self-escalation), so a rule,
   if ever wanted, is Tommy's to add via `/permissions`.

2. **Refresh the plugin cache once family 6 merges** — Tommy's one command. `package-cutover` (now
   `dotnet:package-cutover`), the family-5 plan skills, and `plan-checkpoint` resolve under no name until the
   installed cache carries them. This is now a **two-family** backlog (families 5 + 6).

3. **N2 next (route-table convention), then N3–N6 + N7a** — N1 no longer blocks them. N6 still carries the
   open question for Tommy: `OVERVIEW.md`, `USP.md`, `DEEP_RESEARCH_PROMPT_GUIDE.md` are product narrative,
   neither platform standard nor service-specific — surface, don't invent a home. **N7b** waits on roadmap
   §4c; **N8** last as the only evidence, and must include the Codex delivery gap.

## Completed work

- **N1 family 6 producer — agent-standards #11.** `standards/dotnet/PACKAGE_CUTOVER.md` (the mechanic) +
  `.agents/skills/package-cutover/SKILL.md` (the router); the dotnet plugin ships it automatically because
  `payloads.json` maps the `dotnet` domain to it, so no manifest/README change was needed (README does not
  enumerate per-domain docs). Generator current (**61 skills / 61 docs**, 194 files), hook tests **161/161**,
  plugin router rewrote to a valid relative path, all four cross-links resolve, `package-cutover`
  collision-free across every repo on the machine.
- **N1 family 6 consumer — this branch.** `.agents/skills/package-cutover/` + its `.claude/skills/` stub
  deleted (committed and pushed early — see `## Decisions` on the worktree-deletion incident). No route row
  (invoked by name only), no rename, no hub cited it — the surviving references
  (`docs/REMOTE_VALIDATION.md`, `PIPELINE_DEBT.md`) are by name and still resolve. Plan + ledger updated to
  mark N1 complete.
- **N1 family 5 producer — agent-standards #10.** Five docs, one router each; `plan-checkpoint` is the only
  new skill name (collision-checked free across every repo on the machine). The progress-ledger template
  folded into `CHECKPOINT.md` inside a `~~~markdown` fence, because the generator routes every `standards/`
  doc and a bare asset/reference has no home. Per-repo values not restated: driving roadmap, architecture
  doc and hook/script paths resolve from the plan floor and the vendored `plan_graph.py`/`worktrees.ps1`
  constants; scalable-fix and delete-the-entry defer to `docs-and-debt`; worktree creation and PR opening
  defer to `open-worktree`/`open-pr`. Generator current (60/60), 161 hook tests pass, README charter widened.
- **N1 family 5 consumer — this branch, #687.** Four skill bodies + four `.claude` stubs + the two
  `resume-plan/` support files + the techdebt command (both dirs) deleted. Three citation sites: `plans/AGENTS.md`
  now names the `plan-checkpoint` standard for the ledger template and checkpoint; `package-cutover` defers
  to "the checkpoint the plan floor names"; `.agents/README.md` drops the now subject-less "Repository
  commands" section and its intro mention of command instructions. `auto-memory` left intact.
- **Phase 1 and N1 families 1–4 — merged in both repos.** Phase 1 (#669 + agent-standards #5): the plan
  method into `PLANS.md`, `HANDOFF.md`. Family 1 review (#675 + #6): seven `review/` docs. Family 2 merge/PR
  (#676 + #7): four `merge/` docs, `create-gh-pr` → `open-pr`. Family 3 test-debug (#677 + #8): six
  `testing/` docs, `docker-health.ps1` vendored. Family 4 git (#679 + #9): six `git/` docs, `sync` →
  `sync-checkout` and `worktree` → `open-worktree`, `worktrees.ps1` vendored.

## Verification

Family 6, producer (agent-standards #11):
- `.agents/sync-generated.ps1 -Check` → **current, 61 skills / 61 docs** (194 files); 4 written on the
  authoring run (`.claude` stub, plugin doc copy, plugin router, `dotnet/INDEX.md`).
- Hook tests **161/161**. Plugin router rewrote to `../../standards/dotnet/PACKAGE_CUTOVER.md`. Description
  carries no colon-space (generator would reject).
- All four cross-links from the new doc resolve: `PACKAGES.md`, `../process/PLANS.md`, `data/MIGRATIONS.md`,
  `../process/plan/CHECKPOINT.md`. INDEX row present with the H1 title and `package-cutover` skill.
- `package-cutover` collision-free across `~/.claude`, `~/.agents`, `~/.codex`, `dotagents`, `react-agents`,
  `agent-starter-kit`, the work repos — nowhere else on the machine.

Family 6, consumer (this repo):
- `docs_reachability.py --root <worktree>` → **0 errors, 25 warnings** — the same set as the family-5
  baseline (all pre-existing `plans/` dangles); nothing added or removed by this diff.
- `plan_graph.py --root <worktree>` → 0 errors, 0 warnings. Hook tests **19/19**.
- No route row names `package-cutover` (there never was one); no surviving reference to the deleted skill
  **path** — the two by-name references resolve unchanged.
- **Meta-only holds** — changed top-level paths are `.agents`, `.claude`, `plans`. No `api/**`, no workflow
  file → the `/merge-docs` path, no publish/`chore/platform-sync-*`.

Family 5, producer (agent-standards #10):
- `.agents/sync-generated.ps1 -Check` → **current, 60 skills / 60 docs** (191 files). 16 files written on the
  authoring run (5 `.claude` stubs, 5 docs + 5 routers into `plugins/agent-process/`, INDEX).
- Hook tests **161/161**. Plugin router rewrite produced a valid relative path
  (`../../standards/process/plan/RESUME.md`). No skill description carries a colon-space.
- Names checked collision-free across every repo on the machine — `~/.claude`, `~/.agents`, `~/.codex`,
  `dotagents`, `react-agents`, `agent-standards`, `agent-starter-kit`, the work repos — not just the
  standards set. All five free (`resume-plan`, `continue-roadmap`, `update-roadmap`, `techdebt` were only
  this repo's own copies; `plan-checkpoint` is new).
- Cited constants exist in the producer: `scripts/worktrees.ps1`, `.agents/hooks/plan_graph.py`. New docs'
  only `.md` links (`RESUME.md` → `CHECKPOINT.md`) resolve; the `<url>` reachability error is pre-existing
  in `PLANS.md`, not this diff.

Family 5, consumer (this repo, #687):
- `docs_reachability.py` → **0 errors, 25 warnings**, and the warning **set is byte-identical** to the
  branch base (measured by running the checker on the main checkout at `c39077f1a` and diffing sorted
  lists — nothing added or removed).
- `plan_graph.py` → 0 errors, 0 warnings. Hook tests **19/19**.
- No route row names a deleted skill; no surviving reference to any deleted skill **path** or to the
  techdebt command; `skill_router.py --skills-for` resolves `docs-and-debt`/`plans` on every touched path.
- **Meta-only holds** — changed top-level paths are only `.agents`, `.claude`, `plans`. No `api/**`, no
  workflow file → the `/merge-docs` path, and no publish/`chore/platform-sync-*`.

## Reviews

**Family 6 — low review surface, self-checked; no review file yet.** The consumer is a pure deletion; the
producer doc is the vetted skill body verbatim with only four cross-references re-pointed (all verified to
resolve) and the E2E-tier pointer restated as "the `merge` skill's Step 4" (matches root `AGENTS.md`). A
`/docs-review` over both halves can still be run from the moved copy `standards/process/review/DOCS.md` if
wanted before merge, as prior families did while the cache is stale.

**Family 5 — clean, no findings.** `reviews/Docs-docs_polyrepo-ready-plan-workflow-family.md` covers both halves —
consumer `c39077f1a..2b92ca18d` and producer `9437795..260f7f68` (agent-standards #10); the producer carries
a companion pointer. All six lenses clean plus the `CHECKPOINT.md` fence-nesting and anchor checks, verified
a second time in an independent fresh context. Run from the moved copy `standards/process/review/DOCS.md`,
since the session's active plugin snapshot lacks `docs-review`.

## Decisions, discoveries, blockers, and deviations

- **package-cutover went to the `dotnet` plugin, not `agent-process`.** The plan left the home open
  (`standards/dotnet/` or `standards/process/`). It is decisively .NET/NuGet/EF — CS7069, EF re-scaffold,
  `dotnet build`, `PackageReference` — and would be noise in a React service repo, which `agent-process`
  (installed everywhere) would give it. `dotnet:packages` already owns the *why* ("a published contract
  change is a two-step release"); package-cutover is its runnable execution procedure, so they are siblings
  under the dotnet plugin. Domain→plugin is by `payloads.json` (`dotnet` → dotnet plugin), so a doc under
  `standards/dotnet/` ships correctly with no manifest edit.
- **A background cleanup process deleted the family-6 worktree mid-authoring — commit early.** After the
  producer was pushed and the consumer files deleted (but not yet committed), another Claude Code process
  ran a startup worktree cleanup and `rm`'d the freshly-created worktree, taking the uncommitted consumer
  deletions and plan/ledger edits with it (the branch existed only locally, so it went too). The producer
  (already pushed as #11) was untouched. Recovery: recreate the worktree from `origin/main`, redo the
  deletion, **commit and push it immediately** so the branch is durable on origin, then redo the doc edits.
  Lesson for the remaining nodes: in a repo where concurrent sessions prune worktrees, commit+push the
  irreversible core of a slice before doing the longer ledger prose, not after.
- **First family with a values question that genuinely dissolves rather than ports.** The doc names
  `api/Concertable.slnx`, `./initial-migrations.ps1` and the `Concertable.*` package families. Per the
  calibration set by `PACKAGES.md`/`MIGRATIONS.md`, agent-standards **is** Concertable's roster and names
  these freely; the re-scaffold defers to `data/MIGRATIONS.md` (which owns `./initial-migrations.ps1`) rather
  than restating it. No values file — sixth family, sixth time.
- **A meta-only consumer must ADMIN-MERGE, never `--auto` — the queue runs E2E on it.** This turn a raw
  `gh pr merge 687 --merge --auto` enqueued #687 into the normal queue; inside the `merge_group` the
  path-filter has no diff base, so E2E did **not** skip — `e2e-api-tests` passed, `e2e-ui-tests` ran for
  ~17 min and the entry fell out (auto-merge disabled) before merging. `/merge-docs` (`merge/META_ONLY.md`
  Step 5) exists precisely to avoid this: it admin-merges to **bypass the queue**, with `skip-e2e` as a
  belt-and-braces label for any fallback. Prior meta-only families landed this way; the mistake was mine in
  reaching for `--auto`. The orphaned run was cancelled; no other repair needed.
- **`auto-memory` does not move, by Tommy's decision.** It is a Codex-only feature toggle
  (`.Codex/settings.local.json`), and family 4 established that Codex loads **no** agent-standards plugin —
  so moving it to the plugin would make it useless in Claude Code (no such feature) and unresolvable in
  Codex (no plugin), strictly worse than the in-repo `.agents/skills/` copy Codex reads today. Left in-repo
  and deferred with the Codex machine-setup decision; the plan's acceptance criterion ("nothing agent-based
  left in root") is not yet met for this one skill, deliberately.
- **The generator routes every `standards/` doc, so a reference or asset cannot be a bare file.** The
  checkpoint procedure and the progress template were a `references/` doc and an `assets/` file under the
  `resume-plan` skill. In the router/doc model each doc needs exactly one owning skill, so the checkpoint
  became `plan/CHECKPOINT.md` routed by a new `plan-checkpoint` skill, and the template folded into it as a
  section. Any future move of a reference/asset (N7's `progress-template` is already handled here) faces the
  same rule — fold it into a routed doc or give it a skill.
- **This move created no false sibling statements — the first family that didn't.** Every prior family
  surfaced a rule whose only home was the thing being moved, usually a now-false sentence. This one is clean
  because the four merged families already cite the checkpoint **indirectly** ("the checkpoint procedure the
  repository's plan floor names"), so landing the actual standard **fulfils** that indirection rather than
  falsifying anything. The consumer's `plans/AGENTS.md` is now the doc that names it (`plan-checkpoint`),
  closing the loop.
- **The "Repository commands" mechanism died with its only instance.** `techdebt` was the sole
  `.agents/commands/` entry; once it ships from the plugin, the repo has no commands and both command dirs
  are gone, so `.agents/README.md`'s "Repository commands" section described nothing and was removed. The
  router/doc model supersedes the canonical-command-plus-thin-wrapper mechanism; it has no polyrepo home.
- **The installed plugin cache holds stale snapshots, and the session resolved against a reduced one.**
  `installed_plugins.json` pins `agent-process` to `2d9a8fedf0e7` (the full family-3 snapshot with
  `review`/`merge`/`testing`/`docs-review`), but two reduced snapshots (`88cf091a3c2b`, `bbb5cd69c7e9`)
  carrying only the seven principle skills also exist, and this session loaded `docs-and-debt` from
  `bbb5cd69c7e9` — so `docs-review` and the other executables are **not invocable as skills here**. Ran the
  review from the moved copy on disk, the family-3 fallback. The refresh (Next Steps 2) is Tommy's.
- **Durable cross-family rules that still bind family 6 and N2–N8:**
  - **Collision-check a new skill name across *every* repo on the machine**, not just the three standards
    repos and the harness built-ins — the family-2/3/4 lesson (`create-gh-pr`, `sync`, `worktree` all
    collided).
  - **No values file — resolve per-repo values at run time** (discovery via the plan floor / `gh` / a
    script's own usage listing, or state a genuine constant). Five families, five times; reaching for a
    values file is evidence the discovery mechanism has not been found.
  - **The Codex delivery gap is real and must be N8's concern:** every plugin-delivered standard resolves in
    Claude Code only; Codex has `agent-standards`/`dotagents`/`react-agents` registered nowhere. `auto-memory`
    is the first skill whose *value* the gap changes, not just its delivery.

## Resume prompt

```
cd C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready-package-cutover-family
Read @plans/docs/POLYREPO_READY_PLAN.md and @plans/docs/POLYREPO_READY_PROGRESS.md and do what its `## Next Steps` says.
```
