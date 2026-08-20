# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready-git-family`
- Branch: `Docs/docs_polyrepo-ready-git-family`, from `main` at `1176a002f` - N1 family 4, git.
- PR: this repo - **open this branch's PR**; producer - **agent-standards #9, open at `06405a3`**.
  Shipped so far: Phase 1 as #669 + agent-standards #5; N1 family 1 as #675 + agent-standards #6; N1
  family 2 as #676 + agent-standards #7 at `30734a9`; N1 family 3 as **#677, merged 2026-08-20 at
  `1176a002f`** + agent-standards #8 at `2d9a8fe`.
- Dependency/package gates: none outstanding. agent-standards `main` is at `2d9a8fe` and carries the review,
  merge and test-debug families, so family 4's producer work has a current base. No open
  `chore/platform-sync-*` PR - #677 touched no `api/**` path, so it triggered no publish.
- **Plugin cache is current.** Tommy refreshed it 2026-08-20 to `2d9a8fe` via the supported CLI
  (`claude plugin marketplace update agent-standards`, then `claude plugin update` for each of the three
  plugins) rather than the interactive command. The installed cache now carries `review/`, `merge/` and
  `testing/`, and `review-lifecycle` resolves - which clears the gate that blocked the two steps below it
  for three families. **Claude only:** see `## Decisions` for what this turned up about Codex.
- Last reconciled: 2026-08-20, after #677 merged and the plugin refresh. Family 4's producer half is open as
  agent-standards #9; this branch is the consumer half.

## Current state

**Phase 1 and N1 families 1-3 are merged in both repos.** The test-debug family landed as #677 at
`1176a002f` with all six skill names kept, and `scripts/docker-health.ps1` vendored under
`vendor-hooks.ps1`'s second tier.

**N1 family 4 - git - is authored on both sides.** agent-standards **#9** at `06405a3`: six docs under
`standards/process/git/`, and the first family to *rename*, twice. `sync` -> **`sync-checkout`** and
`worktree` -> **`open-worktree`**, on the `create-gh-pr` -> `open-pr` precedent, because both names are
already taken on this machine by skills doing narrower or contradictory jobs. `commit`, `commit-all`,
`push` and `pull` kept their names. `scripts/worktrees.ps1` (396 lines) proved repo-invariant and is
vendored into the existing `scripts` tier - no `vendor-hooks.ps1` change needed, its filter is already
`*.ps1`. This branch is the consumer half: 429 lines of skill body deleted across twelve directories, the
vendored-script provenance entry, and three citation sites.

**The plugin-cache blocker is cleared.** It stood for three families; `review-lifecycle`, `merge`,
`docs-review` and the rest now resolve from the installed cache at `2d9a8fe`. That unblocks the
`^reviews/.*\.md$` route row, which has been deferred since family 1 and is now the only carried-over
work item.

**N1 families 5-6 and N2-N8 are untouched.** 387 of N1's original 3,285 lines remain, in two families -
plan-workflow (203) and `package-cutover` (184).

## Next Steps

1. **Land agent-standards #9, then this branch's PR.** The producer half must merge first, as every family
   has: the consumer deletes the skill bodies, so a consumer merged alone leaves the procedures reachable
   under no name. This branch is **meta-only** - every changed path is `.agents/**`, `.claude/**`, `docs/**`
   or `plans/**`, and `scripts/worktrees.ps1` is byte-identical to what was already there (only its
   provenance entry is new) - so it lands through `/merge-docs`. Then close this worktree with
   `./scripts/worktrees.ps1 close -Worktree <path> -PullRequest <n> -PlanManaged`.

2. **Refresh the plugin cache once more after #9 merges**, for the same reason as last time: the two renamed
   skills resolve under no name until the installed cache carries them, and the old `sync`/`worktree` names
   now resolve to the *personal* skills only. One command, and it is Tommy's.

3. **Then the `^reviews/.*\.md$` route row** (carried over from family 1, now genuinely unblocked). Its pass
   condition - `review-lifecycle` resolving on this machine - **is met**: the refreshed cache at `2d9a8fe`
   carries `standards/process/review/LIFECYCLE.md` and the router beside it. The row restores automatic
   delivery of the review-file lifecycle, which used to come from `reviews/AGENTS.md` sitting in the
   directory. Confirm on any other machine Tommy works from before treating it as closed.

4. **Then N1 families 5-6**, in the plan's order: plan-workflow (203 lines - `resume-plan`,
   `continue-roadmap`, `update-roadmap`, `techdebt`, `auto-memory`) -> `package-cutover` (184). Family 5
   should absorb `resume-plan/references/plan-progress-checkpoint.md`, 138 lines cited by fifteen skills,
   which all four merged families now cite only indirectly as "the checkpoint procedure the repository's
   plan floor names". Both remaining families still cite it by relative path, so that absorption is what
   makes the indirection universal rather than partial.

5. **N2 can run in parallel**; N3-N6 after N1; N7 when roadmap section 4c unblocks; N8 last as the only
   evidence. N6 still carries the one open question to put to Tommy rather than answer: `OVERVIEW.md`,
   `USP.md` and `DEEP_RESEARCH_PROMPT_GUIDE.md` are product narrative, neither platform standard nor
   service-specific.

## Completed work

- **N1 family 4 producer - agent-standards #9, open at `06405a3`.** Six docs under
  `standards/process/git/`, one router each: `COMMIT.md` <- `commit`; `COMMIT_ALL.md` <- `commit-all`;
  `PUSH.md` <- `push`; `PULL.md` <- `pull`; `SYNC.md` <- **`sync-checkout`** (was `sync`); `WORKTREE.md` <-
  **`open-worktree`** (was `worktree`). Phase 1 had already moved this family's *principles* -
  `COMMITTING.md` and `BRANCHING.md` - so these are the procedures underneath them, the same split
  `MERGING.md` has from `merge/QUEUE.md`. `scripts/worktrees.ps1` vendored into the existing `scripts`
  tier. `sync-generated.ps1 -Check`: 176 files current, 55 skills, 55 docs.
- **N1 family 4 consumer - this branch.** The six `.agents/skills/*` bodies and their six `.claude/skills/*`
  stubs deleted - 429 lines. Only **three** citation sites, the fewest of any family, because the hub docs
  cite the Phase-1 *principle* skills (`committing`, `git-branching`) and `scripts/worktrees.ps1` directly,
  not the executables: `docs/INDEX.md` (three rows added, plus a worktree-creation row above the existing
  cleanup row), `.agents/README.md` (its starter-kit note said "`sync` covers the useful default branch
  update flow" - that sentence *was* the collision, and now states the split), and
  `.agents/hooks/vendored.json`. The generic provenance tests needed no change: they iterate the manifest
  rather than hard-coding a roster.

- **N1 family 3 producer — agent-standards #8, merged 2026-08-20 at `2d9a8fe`.** Six docs under
  `standards/process/testing/`, one router each and every name unchanged: `UI_E2E.md` ← `e2e-ui-debug`;
  `API_E2E.md` ← `e2e-api-debug`; `REGRESSION.md` ← `e2e-ui-regress`; `BOTH_LAYERS.md` ← `e2e-debug`;
  `INTEGRATION.md` ← `integration-debug`; `IDE_DISCOVERY.md` ← `reset-test-explorer`. `vendor-hooks.ps1`
  restructured around a tier list and given its second source→target tier; `scripts/docker-health.ps1`
  added as its first member. Three sibling docs re-pointed because this slice falsified what they said —
  see `## Decisions`. README charter, both marketplace manifests and the plugin manifest widened.
- **N1 family 3 consumer — this branch.** The six `.agents/skills/*` bodies and their six `.claude/skills/*`
  stubs deleted — 1,082 lines. Five citation sites touched, and **none of them a rename**: root `AGENTS.md`
  (skill roster extended by the family), `plans/AGENTS.md` (its own by-tier list deleted, now that
  `failing-tests` owns the table), `docs/INDEX.md` (the red-suite row split into three and two
  machine-enforced rows widened), and `.agents/hooks/vendored.json` + its test suite for the new script
  tier. `.agents/skills/package-cutover/SKILL.md`'s `integration-debug` citation needed no change at all.

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

N1 family 3, producer (`agent-standards` #8):

- `.agents/sync-generated.ps1` → 24 files written; `-Check` reports **158 current (49 skills, 49 docs)**.
- Hook tests **161/161**.
- Every relative link in the six new docs and the four edited ones resolves — 150 links walked across the
  whole `standards/` tree plus the README, one broken and it is pre-existing (`PLANS.md`'s ledger-header
  template contains the literal placeholder `<url>`, which any link checker reads as dead).
- **Names checked collision-free across every repo on this machine, not just the standards set** — the
  lesson family 2 paid for. `dotagents` (32 skills), `react-agents` (14), `agent-standards` (43 before
  (~90 deployed), `~/.codex` (~1,300 installed) and Claude Code's built-ins. All six names free; the only
  hits were this repo's own copies across 58 worktree checkouts.
- The second vendoring tier proved itself before it was trusted: `vendor-hooks.ps1 -Check` against this
  repo reported **only `vendored.json`** stale, never `docker-health.ps1`. A byte-identical body is the
  evidence that the script is genuinely repo-invariant rather than merely believed to be.

N1 family 3, consumer (this repo):

- `python .agents/hooks/docs_reachability.py --root <worktree>` → **0 errors, 26 warnings**, and the warning
  *set* is byte-identical to the branch base's — measured by running the checker in a throwaway detached
  worktree at `d1422b6b5` and diffing the two sorted lists, not by comparing counts. Nothing added, nothing
  removed.
- `python .agents/hooks/plan_graph.py --root <worktree>` → 0 errors, 0 warnings.
- `python -m unittest discover -s .agents/hooks/tests` → **19/19** (14 before; five new cover the script
  tier — hash, upstream path, provenance, absence of a `delivery` field, and wired in no harness).
- No route row names a deleted skill — checked by parsing `.agents/skill-routes.json` and intersecting, not
  by eye. 36 rows, 53 skills named, unchanged by this slice.
- `skill_router.py --skills-for` over the staged diff resolves `docs-and-debt` and `plans` on every path
  this branch touches.
- Repo-wide grep: no surviving reference to any deleted skill's **path**. Every surviving reference is by
  skill *name*, and all six names still exist.
- Every `##`/`###` heading of the six deleted skills maps to a section of a moved doc, with four
  deliberate exceptions recorded under `## Decisions`.
- **Meta-only holds:** every changed path is `.agents/**`, `.claude/**`, `docs/**`, `plans/**` or root
  `AGENTS.md`. `scripts/docker-health.ps1` is untouched on disk.

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

**Family 3 — four findings, all fixed and ticked before either branch went up for review; no open
findings.** One review file across both branches, on the consumer:
`reviews/Docs-docs_polyrepo-ready-test-debug-family.md`, covering `d1422b6b5..207b28d44` here and
`30734a9..5cf3608` in agent-standards; that repo carries a companion pointing at it. **Run from the moved
copy of the procedure** (`standards/process/review/DOCS.md`, merged as #6) for the second slice running,
since this repo holds no `docs-review` skill and the plugin cache is still stale.

- `INST1` — `API_E2E.md` Step 2 carried `--settings <the repo's run settings>`, a placeholder with no
  resolution route, two paragraphs after warning that omitting those settings is how two E2E applications
  boot at once. It now says to grep the entrypoint for it.
- `INST2` — the new tier table in `FAILING_TESTS.md` routes a **unit** failure to `INTEGRATION.md`, whose
  closing note deflected it ("unit tests are a different tier again"). The pre-move `plans/AGENTS.md`
  routed identically and carried the identical gap; **filing the six tiers in one directory is what exposed
  it**, which is the second time this epic has found a defect that six separate skills had hidden. The note
  now says what carries over and what does not.
- `CON1` — `REGRESSION.md` banned re-running a red regress by hand while `FAILING_TESTS.md` mandates
  exactly that re-run as flaky triage. Both correct, neither saying so: the entrypoint has already
  performed it. Named in both docs.
- `INST3` — `BOTH_LAYERS.md` said to confirm the secrets are set without naming them.

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

- **The plan's own measurement was wrong, and the family that checked it is the one that found out.**
  The N1 table said of this family: *"Zero lines name this repo."* Five of the six held to that - `commit`,
  `commit-all`, `push` and `pull` are generic to the point of saying so in their own descriptions. But
  `worktree` named Concertable **in its first sentence**, cited `scripts/worktrees.ps1`, encoded the
  platform-sync gate, and named `resume-plan` and `prune-worktrees` as siblings. The line-count was right
  and the portability claim was not. **A family's measured size is not evidence about its content**; the
  section-6 test has to be applied per document, and re-applied at the moment of moving rather than trusted
  from the plan's table.
- **Two renames, and the second one had already broken something before it was decided.** A skill's name is
  machine-scoped, not corpus-scoped - the merge family's finding with `create-gh-pr`. `sync` collides with
  a user-global skill of the same name doing a *narrower* job; `worktree` collides three ways at once -
  user-global, `dotagents`, and the work repos' shared skills. **This was not theoretical.** Standing up
  this very worktree, the user-global `worktree` fired instead of the repository's own, and its script
  junctioned **sixteen** untracked skill directories into the fresh checkout - every one a stale local copy
  of a skill that now ships from the plugin (`review`, `merge`, `docs-review`, the four `e2e-*`, ...), each
  shadowing the current one under the same name. They were unlinked before any authoring. `sync-checkout`
  and `open-worktree` resolve the names, and `git/WORKTREE.md` now states the junction hazard as a rule
  rather than leaving it as folklore for the next agent to rediscover.
- **The two `worktree` skills also disagreed about where a worktree goes**, and both layouts are live on
  disk - `.worktrees/<flattened>` inside the repo, and a `<repo>.worktrees/<nested>` sibling. That looked
  like a defect to resolve until `worktrees.ps1` turned out to inventory **both** (its one layout-shaped
  line accepts either root). So there is nothing to standardise: the script already tolerates the split,
  and `WORKTREE.md` says so instead of picking a winner the automation does not require.
- **`worktrees.ps1` ports whole, which was not obvious from its size.** 396 lines with exactly one
  layout-shaped line: the default branch is resolved from `origin/HEAD` with `'main'` only as a last-resort
  fallback, the plan-managed ledger check keys on the platform-wide `plans/*_PROGRESS.md` convention, and
  `gh` resolves `{owner}/{repo}` itself. It sorts by **body**, not path - the test-debug family's rule - and
  lands in the `scripts` tier with no `vendor-hooks.ps1` change at all, since that tier's filter is already
  `*.ps1`. **Fifth family, still no values file.**
- **The ledger's claim that "Codex needs nothing beyond both repos being on merged `main`" is false, and the
  plugin refresh is what exposed it.** Claude and Codex keep entirely separate plugin registries. Codex's
  (`~/.codex/config.toml`) has `openai-bundled`, `openai-primary-runtime` and `claude-plugins-official` -
  and **not** `agent-standards`, `dotagents` or `react-agents`; its `~/.codex/plugins/marketplaces/` and
  `~/.codex/skills/` are both empty. So `claude plugin marketplace update` had no Codex-side counterpart to
  refresh: there is no stale copy, there is no copy. **Every plugin-delivered standard resolves in Claude
  Code only.** In Codex, the root `AGENTS.md` routes by name to skills that do not exist there - it sees
  only the in-repo `.agents/skills/` remainder plus the `AGENTS.md` corpus. Codex supports the same
  mechanism (`codex plugin marketplace add`, `codex plugin add`, `codex plugin marketplace upgrade`), so the
  fix is three marketplace registrations, but it is **Tommy's call and not part of this family** - it is a
  machine-setup gap, not a corpus one. It does mean the polyrepo-ready work is currently delivering to one
  of the two harnesses this repo claims to support, which N8's evidence gate should not be allowed to miss.

- **Every node moved so far has surfaced a rule whose only home was the thing being moved. This family
  surfaced four, and three of them were *false statements* the move created rather than merely homeless
  rules.** `FAILING_TESTS.md` said each tier's debug skill is named "in a repository's own guidance" — it
  no longer is, so it now carries the tier table itself. `REMOTE_VALIDATION.md` ended its container
  pre-flight section with "script it, and make the suite runner gate on that script"; the script now exists
  at a constant path in every repo, so it names it. And `MERGING.md` plus `merge/QUEUE.md` both justified an
  executable merge procedure by its need to resolve "the repo's own slug, check names **and debug routes**"
  — debug routes became platform-wide constants in this very commit, so the third item had to go from both.
  A slice that publishes a family invalidates the sentences that described it as per-repo; **grep the
  standards repo for claims about the thing being moved, not just for links to it.**
- **Two moved rules contradicted their new siblings, and the contradiction pre-dated the move.**
  `e2e-ui-regress` said to arm a detached watcher for its terminal line; `e2e-ui-debug` and `e2e-api-debug`
  both said explicitly *not* to use one, and the merge family had already settled on a capped background
  loop that echoes every poll. Filing all six in one directory is what made the disagreement visible at
  all — inside six separate skills it had survived indefinitely. `REGRESSION.md` uses the capped echoing
  loop. Second: `integration-debug` carried the fixture-and-mock roster that
  `dotnet:integration-testing` already owns, so `INTEGRATION.md` keeps only *how to read a failure* and
  points at that inventory for *what the harness contains*.
- **Four headings were deliberately not carried, and one whole section was refused.** `e2e-ui-regress`'s
  "When to use" / "When NOT to use" fold into the opening two paragraphs (a doc opens by saying what it is
  for; a skill needed the headings because its front matter could not). `integration-debug`'s "If the test
  threw" is item 3 of the read-order list rather than its own section. Refused outright:
  `integration-debug`'s **"Conventions that affect how failures read"** — the status-assertion failure
  format moved into Step 3 because it genuinely governs reading a failure, but the response/client variable
  naming and the three-line fetch shape are *authoring* rules the integration-testing standards already
  own. A debug procedure restating them is the duplication this epic exists to end.
- **Three citation classes were dropped rather than moved, all for the same reason: the target does not
  exist in the destination.** Per-repo memory ids (`e2e_parallel_execution`, `stripe_e2e_resolver_state`,
  `idevseder_not_itestseeder_for_e2e`, `cross_context_fk`) — memories are project-scoped files, so an id
  cited from a machine-wide plugin points at nothing on a machine that never had this repo; the *facts*
  they carried are stated in prose instead. A citation to `SEEDING_CONVENTIONS.md`, which **already did not
  exist in this repo** — a dead link the move surfaced, and its rule is owned by the two seeding skills the
  same bullet names. And both suites' scenario, feature and module rosters, which the docs standard's "a
  count would be wrong by the next one anyone writes" rule forbids; the docs derive them from
  `--list-tests` and `integration.ps1 list` instead.
- **`docker ps` versus the real gate is a deliberate split, not an inconsistency, and it is now written
  down.** The three E2E docs mandate `./scripts/docker-health.ps1`; `INTEGRATION.md` keeps the cheap
  `docker ps`. The gate's own header scopes it to the E2E suites, and the failure it catches is a *booted
  stack's* published ports dying. `INTEGRATION.md` states the one case that promotes it — the cheap check
  passes but every database connection is accepted then reset — so the next reader does not "fix" the
  asymmetry by copying the heavier gate into a tier that does not need it.
- **The vendoring tier's manifest is keyed by tier, and only hooks carry `delivery`.** A script is invoked
  from a command line or by another script and is never wired to a harness event, so a `delivery` field
  with one possible value is an invitation to answer it wrongly. The consumer's test suite asserts the
  absence of the field, that the copy lands at the same path it holds upstream (which is the entire basis
  for a doc naming `./scripts/docker-health.ps1` as a constant), and that it is wired in no harness.
- **`e2e.ps1` and `integration.ps1` did not move, and their discovery mechanism already existed.** Both
  scripts print a usage listing when called with no recognised argument, and `integration.ps1 list` prints
  every integration project. That is the same shape the merge family found in `gh repo view` / `gh pr
  checks`: the value is *resolvable at run time*, so no doc names a project path and no repo writes a
  values file. **A fifth family, a fifth time with no values file** — the pattern is now strong enough that
  reaching for one should be treated as evidence the discovery mechanism has not been found yet.

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
cd C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready-git-family
Read @plans/docs/POLYREPO_READY_PLAN.md and @plans/docs/POLYREPO_READY_PROGRESS.md and do what its `## Next Steps` says.
```
