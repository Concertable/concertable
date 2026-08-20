# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready-test-debug-family`
- Branch: `Docs/docs_polyrepo-ready-test-debug-family`, from `main` at `d1422b6b5` — N1 family 3, test-debug.
- PR: this repo — **#677, draft**, based on `main`; producer `Concertable/agent-standards` — none open yet.
  Shipped so far: Phase 1 as #669 + agent-standards #5; N1 family 1 as #675 + agent-standards #6; N1
  family 2 as **#676, merged 2026-08-20 at `d1422b6b5`** + agent-standards #7 at `30734a9`.
- Dependency/package gates: none outstanding. agent-standards `main` is at `30734a9` and carries the review
  and merge families, so family 3's producer work has a current base. No open `chore/platform-sync-*` PR —
  #676 touched no `api/**` path, so it triggered no publish.
- Last reconciled: 2026-08-20, after #676 merged — merge confirmed by polling to a terminal state
  (`MERGED` at 15:28:36Z, queue empty), no sync PR opened; plugin cache still stale (its
  `standards/process/` holds the seven original docs, no `review/` and no `merge/`).

## Current state

**Phase 1 and N1 families 1 and 2 are merged in both repos.** The merge/PR family landed as #676 at
`d1422b6b5`, closing the family: `merge`, `merge-docs` and `pr-preflight` keep their names so every prose
citation still resolves, and `create-gh-pr` shipped as `open-pr` for the reason under `## Decisions`.

**N1 family 3 — test-debug — is the open slice, and this branch is it.** Nothing has been written yet; what
is done is the precondition the plan had left open, **the script-path question, now settled** (plan, N1
section): vendor `docker-health.ps1`, state the `e2e.ps1` / `integration.ps1` invocation grammar, no values
file. 1,022 lines across six skills, 131 of them carrying a repo-specific token.

**N1 families 4–6 and N2–N8 are untouched.** 1,838 of N1's original 3,285 lines remain, in four families.

**Blocked, and it is Tommy's action, not a work item:** the plugin cache on this machine still predates
agent-standards #6 — `~/.claude/plugins/cache/agent-standards/agent-process/.../standards/process/` holds
the seven original docs and neither a `review/` nor a `merge/` directory, so the routers of *both* merged
families fail to resolve. Family 1's Next Step (below, now step 2) has not had its pass condition met, and
families 2 and 3 inherit the same dependency.

## Next Steps

1. **N1 family 3 — test-debug, the open slice. Producer first, on `agent-standards`.** Six skills, 1,022
   lines: `e2e-ui-debug` (248), `integration-debug` (229), `e2e-api-debug` (217), `e2e-ui-regress` (141),
   `e2e-debug` (105), `reset-test-explorer` (82). The script-path question is settled in the plan, so the
   order is fixed:

   - **`vendor-hooks.ps1` gets a second source→target tier first** (`scripts/` → `scripts/`, same
     provenance manifest; it filters `*.py` into `.agents/hooks` today), and `docker-health.ps1` moves under
     it. Until that exists the moved docs cannot name `./scripts/docker-health.ps1` as a constant, which is
     the whole basis of the settled answer.
   - **Then the docs**, under `standards/process/testing/` on the review/merge model: one doc per tier with
     a router each, names checked against **every repo on this machine** — not just the three standards
     repos and the built-ins, which is the lesson family 2 paid for.
   - **Then the consumer half on this branch**: delete the six bodies and their `.claude/skills` stubs,
     re-point citations, and leave behind only this repo's `scripts/e2e.ps1` / `scripts/integration.ps1`
     bodies. 131 of the 1,022 lines carry a repo-specific token, so the "leaves behind only values" gate is
     real work here, not a formality.

2. **Run `/plugin marketplace update agent-standards` — one command, and it is Tommy's.** Two merged
   families (thirteen skills: the review family plus `review-lifecycle`, and the merge/PR family) resolve
   from the plugin, and the cache on this machine has not been refreshed since #6 merged. Until it is,
   `/review`, `/docs-review`, `/merge` and the rest exist under no name a harness can find — which is why
   family 2's own docs review had to be run from the moved copy of the procedure. It does not block family
   3's authoring; it blocks step 3 and every *invocation* of the moved skills. Codex needs nothing beyond
   both repos being on merged `main`.

3. **Then the `^reviews/.*\.md$` route row** (carried over from family 1, unchanged and still deferred). A
   route row naming a skill the plugin cache has not reinstalled hard-blocks every write to `reviews/**`
   until the refresh — the same trap that kept `handoff` out of the table in Phase 1. The row is what
   restores automatic delivery of the review-file lifecycle, which used to come from `reviews/AGENTS.md`
   sitting in the directory. **Pass condition:** `review-lifecycle` resolves after step 2 on this machine,
   and Tommy has confirmed the same on any other machine he works from.

4. **Then N1 families 4–6**, in the plan's order: git (429 lines, plus reconciling `dotagents`'
   `commit-push`/`sync`/`pull-main` overlap — and note `worktree` collides with a skill of that name in
   *both* `dotagents` and the work repos, so family 2's naming problem recurs there) → plan-workflow (203,
   and it should absorb `resume-plan/references/plan-progress-checkpoint.md`, 138 lines cited by fifteen
   skills, which both merged families now cite only indirectly as "the checkpoint procedure the repository's
   plan floor names") → `package-cutover` (184).

5. **N2 can run in parallel**; N3–N6 after N1; N7 when roadmap §4c unblocks; N8 last as the only evidence.
   N6 still carries the one open question to put to Tommy rather than answer: `OVERVIEW.md`, `USP.md` and
   `DEEP_RESEARCH_PROMPT_GUIDE.md` are product narrative, neither platform standard nor service-specific.

## Completed work

- **N1 family 2 — merged in both repos.** agent-standards #7 at `30734a9`; this repo's **#676 merged
  2026-08-20 at `d1422b6b5`**, confirmed by polling the queue to a terminal state rather than assuming.
  No publish run and no `chore/platform-sync-*` PR, as predicted: the diff touched no `api/**` path. The
  worktree `Docs/docs_polyrepo-ready-merge-family` is spent and closes with
  `./scripts/worktrees.ps1 close -Worktree <path> -PullRequest 676 -PlanManaged`.

- **N1 family 2 producer — agent-standards #7, merged 2026-08-20 at `30734a9`.** Four docs under
  `standards/process/merge/`, one router each: `QUEUE.md` (287) ← `merge`; `META_ONLY.md` (100) ←
  `merge-docs`; `PREFLIGHT.md` (108) ← `pr-preflight`; `OPENING.md` (95) ← `create-gh-pr`, routed by a new
  skill `open-pr`. `MERGING.md`'s delegation of the runnable poll loop re-pointed from "whatever executable
  merge command the repository owns" to `merge/QUEUE.md`. README charter and both marketplace manifests
  widened to admit the family.
- **N1 family 2 consumer — this branch.** The four `.agents/skills/*` bodies and their four
  `.claude/skills/*` stubs deleted — 674 lines. Five citation sites re-pointed, every one from a file path
  to a skill name: root `AGENTS.md` (skill roster extended by the family; the Merging run-book pointer),
  `docs/INDEX.md` (the merge-procedure and E2E-tier rows, plus new rows for PR-opening/preflight and the
  meta-only path), `docs/REMOTE_VALIDATION.md`, `plans/AGENTS.md`, and the `changes`-job comment in
  `.github/workflows/test.yml`.
- **N1 family 2 review — clean.** Four findings, all fixed before either branch was pushed; three of them
  defects in the moved docs themselves (see `## Reviews`).
- **N1 family 1 producer — agent-standards #6.** Seven docs under `standards/process/review/` with a router
  each: `FULL.md` ← `review`; `INCREMENTAL.md` ← `incremental-review`; `STAGED.md` ← `big-review`;
  `UNATTENDED.md` ← `big-review-all`; `DOCS.md` ← `docs-review`; `ADDRESSING.md` ← `address-review`;
  `LIFECYCLE.md` ← this repo's `reviews/AGENTS.md`, routed by a new skill `review-lifecycle`. The
  purchase-time snapshot rule folded into `standards/dotnet/structure/SERVICE_BOUNDARIES.md`.
- **N1 family 1 consumer — this repo's #675.** The six skill bodies, their stubs, and `reviews/AGENTS.md` +
  its `CLAUDE.md` sibling deleted — 941 lines; root `AGENTS.md` skill list extended; two `docs/INDEX.md`
  rows re-pointed.
- **Phase 1 producer — agent-standards #5.** `standards/process/PLANS.md` 78 → 248 lines, absorbing the
  method from `plans/agents/PLAN.md` and the roadmap tier from `plans/agents/ROADMAP.md`; new
  `standards/process/HANDOFF.md` (57); new `handoff` router; `plans` router description widened; README
  charter reworded.
- **Phase 1 consumer — this repo's #669.** `PROMPTS.md`, `plans/agents/PLAN.md` and
  `plans/agents/ROADMAP.md` deleted. `plans/AGENTS.md` rewritten as the in-repo floor (75 lines). Every
  citation re-pointed across root `AGENTS.md`, `docs/INDEX.md`, the route table, eight skills,
  `api/Concertable.Shared/TECH_DEBT.md`, and five roadmap headers.

## Verification

N1 family 2, producer (`agent-standards`):

- `.agents/sync-generated.ps1` → 15 files written, 1 more after the review fixes; `-Check` reports **140
  current (43 skills, 43 docs)**.
- Hook tests **161/161**.
- Every relative link in the four new docs and in the edited `MERGING.md` resolves — checked by walking the
  tree, zero broken. `PLANS.md` really carries the "Never leave the codebase out of sync" heading
  `PREFLIGHT.md` cites.
- Names checked collision-free across `dotagents` (32), `react-agents` (14), `agent-standards` (39 before
  this), this repo (22) and Claude Code's built-ins. Three were free; `create-gh-pr` was not.

N1 family 2, delivery gate (2026-08-20):

- agent-standards #7: `verify` pass, `CLEAN`, 0 behind `origin/main`. That repo has no merge queue — its
  rulesets API answers 403 on a private free plan — so it merges directly rather than through `--auto`.
- #676: every non-`merge_group` check **pass** — build, `ci-complete`, all seven carves, `fe-boundaries`,
  `hook-tests`, the full integration matrix; `CLEAN`; 0 behind `origin/main`. Verified at `fd9231be` and
  re-verified in full at `3f70e103` after the first checkpoint pushed. The only non-pass rows are the three
  `merge_group`-gated E2E suites reporting `skipping`, which is expected on the PR itself. Every checkpoint
  commit re-runs the PR gate, so the green set belongs to whichever head the ledger's last push produced.
- **E2E tier: `skip-e2e` applied to #676, no `full-e2e`.** No positive trigger — the diff is markdown plus
  one comment line in `.github/workflows/test.yml`: no UI flow, no HTTP or cross-service contract, no
  published package shape, no auth or routing behaviour. The hard floor still gates it.

N1 family 2, consumer (this repo):

- `python .agents/hooks/docs_reachability.py --root <worktree>` → **0 errors, 26 warnings**, against 27 on
  this branch's own base: zero added, one removed. The removed one was this ledger's own — its header
  carried a markdown link whose target was the literal placeholder `<url>`, which the checker reads as a
  dead link, and rewriting the header dropped it. Writing that shape *about* itself re-triggers the warning,
  so describe it rather than quote it. (Family 1's ledger recorded 26 as its baseline; `main` then gained one warning and this slice clears a
  different one. Diffing against the branch base rather than a remembered number is what makes the claim
  checkable at all — the numbers alone would have looked unchanged while hiding both movements.)
- `python .agents/hooks/plan_graph.py --root <worktree>` → 0 errors, 0 warnings.
- `python -m unittest discover -s .agents/hooks/tests` → **14/14**.
- No route row names a deleted skill, and no row names `create-gh-pr` — checked by parsing
  `.agents/skill-routes.json` and intersecting the skill names it yields with the deleted set, not by eye.
  36 rows, 53 skills named, unchanged by this slice.
- `skill_router.py --skills-for` still resolves on every path this branch touches.
- Every `##`/`###` heading of the four deleted skills maps to a section of a moved doc. `merge`'s
  `### Transition checkpoints` was promoted to a top-level section, since it governed every step rather
  than sitting inside `## Steps`. The only content deliberately **not** carried is `create-gh-pr`'s
  contrast with the work repos' PR skill — see `## Decisions`.
- Repo-wide grep: no surviving reference to a deleted skill's path or to the name `create-gh-pr`.

## Reviews

`/docs-review` over `2ce95368..29a82099` → `reviews/Docs-docs_polyrepo-ready-merge-family.md`, whose header
carries the reviewed range and watermark. Four findings, all fixed and ticked; no open findings. The
producer branch carries a companion `reviews/Docs-polyrepo-ready-merge-family.md` pointing at it rather than
splitting one review across two files, because three of the four findings were defects in the moved docs.

**Run from the moved copy of the procedure** (`standards/process/review/DOCS.md`, merged as #6), because
this repo no longer holds a `docs-review` skill and the plugin cache has not been refreshed — which
discharges family 1's own gate, "`docs-review` still runs end-to-end from the moved copy", a second time on
a real diff.

- `HOME1` — `merge/QUEUE.md` step 5 restated the worktree-close command's full refusal list, which root
  `AGENTS.md` and `docs/INDEX.md` already own in the consuming repo. Collapsed to "run the command and trust
  the refusal".
- `INST1` — `merge/QUEUE.md` step 0 said to hand the user a command "prefixed so it executes in their
  session" without naming the prefix. Over-generalising had deleted the actionable half of the rule;
  `!` restored and attributed to Claude Code.
- `ACC1` — the sync-branch prefix appeared as a `<placeholder>` in one step of `merge/QUEUE.md` and as the
  literal `chore/platform-sync-` in another. Both literal now.
- `ACC2` — the rewritten root `AGENTS.md` Merging pointer added a second copy of the E2E-tier ownership
  sentence the validation section already owns — a duplicate introduced by the commit whose purpose is
  de-duplication. Removed.

Family 1's reviews, clean and merged: this repo's #675 → `reviews/Docs-docs_polyrepo-ready-nodes.md`;
agent-standards #6 → its `reviews/Docs-polyrepo-ready-review-family.md`. Phase 1's: #669 →
`reviews/Docs-docs_polyrepo-ready.md`; agent-standards #5 → `reviews/Docs-polyrepo-ready-process.md`, one
finding deferred (`PLANS.md` at 248 lines is 3× that repo's eighty-line split rule); #668 → an
`## Incremental review` section on `reviews/Docs-skill-routes-mapper-coverage.md`.

## Decisions, discoveries, blockers, and deviations

- **A skill name can collide with a repo this corpus never sees, and one did.** `create-gh-pr` already
  exists on this machine as the work repos' issue-tracker-linked PR skill — a *different and contradictory*
  procedure (work item, board transition, assignee, attribution stripped) reached through per-repo
  junctions. A plugin installs **per machine**, so publishing `create-gh-pr` from `agent-standards` would
  offer a "GitHub only, no work item, no assignee" procedure inside repos where the opposite is mandatory.
  Claude Code namespaces plugin skills, so the two would not collide *by name*; they would collide by
  meaning, in the same session, which is worse than a name clash because nothing surfaces it. Renamed to
  **`open-pr`**; its three in-repo citations all sat in skills this slice deletes, so the rename cost
  nothing. **The generalisable lesson: family 1's collision check swept the three standards repos and the
  harness built-ins. That set is incomplete — the real namespace is every repo on the machine.**
  say what the *work* skill does that this one must not. That contrast is a machine-level fact about two
  skill sets coexisting, already owned by Tommy's global instructions ("personal repos use plain
  `gh pr create`; the Azure-DevOps skills are work-only"). Carrying it into a machine-wide plugin doc would
  have put another organisation's tooling into a Concertable standard — and it is exactly what made the name
  unsafe. `OPENING.md` states the platform-neutral form instead: use the forge's own CLI, and a
  differently-scoped PR procedure elsewhere on the machine is not this one.
- **`MERGING.md` was already pointing at something this epic deletes.** It said "the runnable loop belongs
  to whatever executable merge command the repository owns, because only that command knows the repo's own
  slug, check names and debug routes." Once the executable command is itself platform-wide, that sentence
  describes a home with no future — the §6 failure mode, found *inside* the standards repo rather than the
  monorepo. The loop now lives in `merge/QUEUE.md`, and the "only that command knows" premise is answered
  the family-1 way: it resolves the slug from `gh repo view`, the check set from `gh pr checks`, and the
  queue config from the rulesets API, so nothing needs to be known in advance. **Every node moved so far has
  surfaced at least one rule whose only home was the thing being moved; that is now the expected yield of a
  slice, not a surprise.**
- **One value in this family genuinely does not port, and it was monorepo-shaped.** "Does this merge
  publish?" was written as "does the diff touch `api/**`" — a path that does not exist in a carved service
  repo, where the repo *is* the service. `QUEUE.md` re-anchors it on what the repo's own publish workflow
  filters on. This is the plan's "rows keyed on architecture port; rows keyed on location don't" showing up
  in prose rather than in the route table.
- **No values file again — three families, three times.** The label names (`skip-e2e`, `skip-e2e-ui`,
  `full-e2e`), the aggregate required check, and `scripts/worktrees.ps1` are identical in every repo, so
  they are stated rather than parameterised. The queue's ruleset id was **dropped** rather than moved:
  nothing in the procedure uses it, so it was colour that would rot. The vendored review-gate hook path
  (`.agents/hooks/merge_review_gate.py`) is named, being fixed by `vendor-hooks.ps1` everywhere.
- **The E2E-tier rule survived the move intact, and it is the substantive content of the family.** Four
  positive triggers, "wiring is not an independent trigger", the hard floor, and why a git trailer silently
  loses to a mandated attribution trailer (git parses only the last paragraph, and a blank line between them
  makes two). The two PR numbers that evidenced the trailer bug were dropped — a PR number in another repo
  is precisely the citation the docs standard says will rot — while the mechanism they proved was kept.
- **This slice had to break its own meta-only gate, and that is the correct outcome.** The consumer branch
  edits one comment in `.github/workflows/test.yml`, so it fails the meta-only path list and lands through
  `/merge`. The gate is path-based on purpose: the alternative readings were to leave a dead pointer on
  `main`, or to split a one-line comment onto its own PR. Recorded here because the next family that
  re-points a workflow comment will hit it too.
- **The plugin refresh is now a two-family backlog, and nothing in the repo can check it.** Family 1's
  ledger made the refresh a per-machine follow-up; it has not happened, so thirteen merged skills currently
  resolve under no name. This is not a work item to schedule — it is one command, and until it runs, every
  subsequent family compounds the same gap. Hence its promotion to Next Step 1.
- **The vendored review gate has a path bug, and it fails closed — so it looks like a refusal.**
  `merge_review_gate.py`'s `merge_target_dir` takes the literal argument of the last `cd` before the merge
  command and hands it to `subprocess(cwd=…)`. From Git Bash that argument is a POSIX path (`/c/Users/…`),
  which Windows Python cannot use as a working directory, so `git rev-parse --show-toplevel` raises
  `WinError 267` and the gate blocks with "cannot resolve git state" — on a branch whose review is clean.
  A Windows-form `cd C:/Users/…` resolves fine, so the fix is one normalization step in the hook, in
  `agent-standards`, on its own slice. **Not amended onto #7:** that branch is reviewed and green, and a
  hook change is neither this family's content nor covered by its review.
- **Test-debug's scripts sort three ways, not two — and the plan had miscounted them.** The six skills name
  `scripts/e2e.ps1` 35 times, `scripts/integration.ps1` 8 (the `integration-debug` entrypoint, which the
  plan had not counted at all) and `scripts/docker-health.ps1` 4. The settled answer lives in the plan; the
  discovery that decided it belongs here: `docker-health.ps1` contains no repo path, no suite name and no
  `api/` anywhere, while `e2e.ps1` and `integration.ps1` are hardcoded lists of this monorepo's suite
  projects across four services. **Portability is a property of a script's body, not of its path**, which is
  why both "vendor the scripts" and "parameterise the paths" were the wrong question — the answer splits the
  three. Also measured, because the family gate depends on it: 131 of these 1,022 lines carry a
  repo-specific token, against a family-2 diff that carried none. "Leaves behind only values" has real work
  to do here.
- **`worktree` will recur as family 4's naming problem.** It exists in `dotagents` *and* in the work repos,
  which is two collisions rather than one, and unlike `create-gh-pr` it has real in-repo citations. Flagged
  now so family 4 budgets for it rather than discovering it mid-slice.

## Resume prompt

```
cd C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready-merge-family
Read @plans/docs/POLYREPO_READY_PLAN.md and @plans/docs/POLYREPO_READY_PROGRESS.md and do what its `## Next Steps` says.
```
