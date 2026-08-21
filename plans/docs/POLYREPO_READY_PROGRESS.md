# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready-plan-workflow-family`
- Branch: `Docs/docs_polyrepo-ready-plan-workflow-family`, from `main` at `c39077f1a` — N1 family 5, plan-workflow.
- PR: producer **agent-standards #10 — MERGED** (`main` at `9437795`); consumer this repo **#687 — open**,
  meta-only, `CLEAN`/`MERGEABLE`, all checks green. #687 needs only its admin-merge (see Next Steps 1).
- Dependency/package gates: none. No open `chore/platform-sync-*` PR — #687 touches no `api/**` path, so it
  triggers no publish. **Plugin-cache refresh is pending** (Next Steps 2) — the renamed/new skills resolve
  under no name until the installed cache carries them.
- Last reconciled: 2026-08-21, after #10 merged and a `--auto` attempt on #687 mis-routed it into the
  full-E2E queue (cancelled and corrected to the admin-merge path — see Next Steps 1 and `## Decisions`).

## Current state

**Phase 1 and N1 families 1–4 are merged in both repos.** Review (#675 + agent-standards #6), merge/PR
(#676 + #7), test-debug (#677 + #8) and git (#679 + #9) all landed; `main` is at `c39077f1a` and
agent-standards `main` at `9437795`.

**N1 family 5 — plan-workflow — is authored on both sides and open.** agent-standards **#10**: four docs
under `standards/process/` (`plan/RESUME.md` ← `resume-plan`, `plan/CONTINUE_ROADMAP.md` ←
`continue-roadmap`, `plan/UPDATE_ROADMAP.md` ← `update-roadmap`, `TECHDEBT.md` ← `techdebt`) plus a fifth,
`plan/CHECKPOINT.md`, routed by a **new** `plan-checkpoint` skill — the 138-line plan-progress checkpoint
procedure (was `resume-plan/references/plan-progress-checkpoint.md`) with the progress-ledger template
folded in as a routed section. This repo's **#687** is the consumer half: the four skill bodies + `.claude`
stubs deleted, the checkpoint/template files under `resume-plan/` gone, and `.agents/commands/techdebt.md`
+ `.claude/commands/techdebt.md` deleted (the last repository command — both command dirs are now gone).
Three citation sites re-pointed (`plans/AGENTS.md`, `package-cutover`, `.agents/README.md`).

**`auto-memory` stays in-repo, by decision** (see Next Steps and `## Decisions`): a Codex-only feature
toggle that Codex, loading no agent-standards plugin, could no longer resolve if moved.

**Remaining N1: `package-cutover` only** (family 6, 184 lines). Its checkpoint citation was already
re-pointed to the indirect form in #687, so family 6 need only move its body and fix any remaining
citations. N2–N8 untouched.

## Next Steps

1. **agent-standards #10 is MERGED; land #687 next — via ADMIN-MERGE, not `--auto`.** The producer merged
   (agent-standards `main` at `9437795`), so the consumer is safe to land. #687 is **meta-only** — every
   changed path is `.agents/**`, `.claude/**`, `plans/**`, `reviews/**` — so it lands through `/merge-docs`,
   whose Step 5 **admin-merges to bypass the merge queue**. It must NOT go through `--auto`: the queue's
   `merge_group` runs full E2E on a meta-only diff (the path-filter has no diff base inside a merge_group, so
   E2E does not skip unless the `skip-e2e` label is present), and a raw `--auto` this turn enqueued it, ran
   full E2E, hung on `e2e-ui-tests`, and dropped out of the queue (auto-merge disabled). That orphaned
   `merge_group` run was cancelled; #687 is now `OPEN`/`CLEAN`/`MERGEABLE`, no label, auto off. The correct
   commands, in order:
   ```bash
   gh -R Concertable/concertable pr edit  687 --add-label skip-e2e     # belt-and-braces for any queue fallback
   gh -R Concertable/concertable pr merge 687 --merge --admin          # bypass the queue; no --delete-branch
   ```
   Then `git checkout main && git pull --ff-only origin main` and close this worktree with
   `./scripts/worktrees.ps1 close -Worktree <path> -PullRequest 687 -PlanManaged`.

   **Blocked on authorization — the same wall families 2–4 hit.** The auto-mode classifier refused both
   `gh pr edit --add-label` and `gh pr merge --admin` this turn (it allowed `--auto` and `gh run cancel`
   only). So an agent tool here cannot land it the meta-only way. **Merge needs Tommy to run the two commands
   above, approve them interactively, or add a permission rule.**

2. **Refresh the plugin cache after #10 merges** — Tommy's one command. The new `plan-checkpoint` and the
   moved plan skills resolve under no name until the installed cache carries them, and the cache currently
   holds stale snapshots (see `## Decisions`).

3. **Then N1 family 6 — `package-cutover` (184 lines)** — the last N1 family. Move its body to
   `standards/dotnet/` or `standards/process/` (it is the published-contract cut-over mechanic; decide its
   home when authoring), keep the `plan-checkpoint` indirection #687 already installed, and re-point any
   remaining citations.

4. **Then N2** (route-table convention) can run in parallel; **N3–N6** and **N7a** after N1; **N7b** when
   roadmap §4c unblocks; **N8** last as the only evidence. N6 still carries the open question for Tommy:
   `OVERVIEW.md`, `USP.md` and `DEEP_RESEARCH_PROMPT_GUIDE.md` are product narrative, neither platform
   standard nor service-specific.

## Completed work

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

**Clean, no findings.** `reviews/Docs-docs_polyrepo-ready-plan-workflow-family.md` covers both halves —
consumer `c39077f1a..2b92ca18d` and producer `9437795..260f7f68` (agent-standards #10); the producer carries
a companion pointer. All six lenses clean plus the `CHECKPOINT.md` fence-nesting and anchor checks, verified
a second time in an independent fresh context. Run from the moved copy `standards/process/review/DOCS.md`,
since the session's active plugin snapshot lacks `docs-review`.

## Decisions, discoveries, blockers, and deviations

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
cd C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready-plan-workflow-family
Read @plans/docs/POLYREPO_READY_PLAN.md and @plans/docs/POLYREPO_READY_PROGRESS.md and do what its `## Next Steps` says.
```
