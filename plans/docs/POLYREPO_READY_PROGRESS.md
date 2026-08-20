# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready-nodes`
- Branch: `Docs/docs_polyrepo-ready-nodes`
- PR: this repo — #669 MERGED as `1d15a7920`; producer `Concertable/agent-standards` #5 MERGED as `1d44caa38`. Current slice: no PR opened yet
- Dependency/package gates: none open. Phase 1's producer shipped; platform-sync #672 (0.1.0-alpha.0.1086) was the merge's own sync and carried no consumer migration.
- Last reconciled: 2026-08-20 after the three merges, from `gh pr view`, `git ls-tree origin/main` for the node inventory, and `wc -l` for every figure in the plan.

## Current state

**Phase 1 is merged in both repos.** agent-standards #5 → `1d44caa38`; this repo #668 → `6f8a31f02` and
#669 → `1d15a7920`; both worktrees closed with `worktrees.ps1 close`; agent-standards `main` pulled so
Codex reads the merged standards through its junctions. The one remaining harness step is Tommy running
`/plugin marketplace update agent-standards` per machine so the `handoff` router resolves in Claude.

**Everything else is untouched and now expressed as nodes, not phases.** The plan carries a measured
inventory of every guidance node still sitting in the root that §6 deletes — N1 skills (3,285 lines), N2 the
36-row route table’s convention, N3 `api/AGENTS.md` (78), N4 `api/ARCHITECTURE.md` + `MICROSERVICES_ARCHITECTURE.md`
(587), N5 root `AGENTS.md` (147), N6 `docs/` (554), N7 the `plans/` tree — worked one at a time in that
order, each as its own producer→consumer slice. N8 proves one carved service and is the only evidence.

The 136 `AGENTS.md`/`CLAUDE.md` pairs under `api/**` are **not** in scope: they are per-service,
per-module and per-test-project, already at the lowest containing node, and they ride their service into its
repo. That is the destination working as intended.

## Next Steps

1. **N1, family 1 — the review skills (813 lines).** Producer PR in `Concertable/agent-standards`: move
   `review`, `docs-review`, `big-review`, `incremental-review`, `address-review`, `big-review-all` out as
   `standards/process/` docs with their routers, parameterised over the values a repo supplies (its review
   file location, its hook paths, its area globs). Consumer PR here deletes the six bodies and leaves the
   values. Land producer first, exactly as Phase 1 did. Gate: a simulated carved tree loses no rule, the
   routers resolve from a fresh install, and `docs-review` still runs end-to-end from the moved copy — this
   plan's own next review is the test case.

2. **Then N1 families 2–6, one slice each**, in the plan's order: merge/PR (634) → test-debug (1,022, needs
   the script-path parameterisation decided first) → git (429, plus reconciling `dotagents`'
   `commit-push`/`sync`/`pull-main` overlap) → plan-workflow (203) → `package-cutover` (184).

3. **N2 can run in parallel** — it touches only the route convention and a generator, not the skills.

4. **N3–N6 after N1**, then N7 when roadmap §4c unblocks, then N8 as the terminal evidence gate. N6 carries
   the one open question to put to Tommy rather than answer: `OVERVIEW.md`, `USP.md` and
   `DEEP_RESEARCH_PROMPT_GUIDE.md` are product narrative, neither platform standard nor service-specific.

## Completed work

- **Phase 1 producer — agent-standards PR #5.** `standards/process/PLANS.md` 78 → 248 lines, absorbing the
  method from `plans/agents/PLAN.md` and the roadmap tier from `plans/agents/ROADMAP.md`; new
  `standards/process/HANDOFF.md` (57) for the continuation pointer's exact shape; new `handoff` router;
  `plans` router description widened and handing the prompt shape to `handoff`; README charter reworded to
  separate roster (`dotnet`, `react`) from method (`process`) and to record why a fourth process repo was
  rejected. `sync-generated.ps1` wrote 7 files, `-Check` reports 107 current, hook tests 161/161.
- **Phase 1 consumer — this branch.** `PROMPTS.md`, `plans/agents/PLAN.md` and `plans/agents/ROADMAP.md`
  deleted (`plans/agents/` is gone). `plans/AGENTS.md` rewritten as the in-repo floor (71 → 75 lines):
  layout, ledger template + checkpoint paths, `plan_graph.py`/`plan_handoff_stop.py`,
  `worktrees.ps1 close/retire`, this repo's plan and review skill names, the debug tiers,
  `initial-migrations.ps1`, the merge-queue E2E tier, and the carve's instance of the breaking-contract
  rule. Every citation re-pointed: root `AGENTS.md`, `docs/INDEX.md` (2 rows), the `^PROMPTS.md$` route row
  removed and the `plans` row's note rewritten, eight skills (`continue-roadmap`, `docs-review` ×4,
  `merge`, `merge-docs`, `package-cutover`, `pr-preflight`, `resume-plan` ×2, `update-roadmap` ×2,
  `plan-progress-checkpoint` ×2), `api/Concertable.Shared/TECH_DEBT.md`, and the five roadmap headers that
  linked the deleted roadmap playbook.
- **Phase 1 review — clean.** `/docs-review`, four findings, all fixed on this branch (see `## Reviews`).

## Verification

- `python .agents/hooks/plan_graph.py --root <worktree>` → 0 errors, 0 warnings.
- `python .agents/hooks/docs_reachability.py --root <worktree>` → 0 errors, 26 warnings, all pre-existing
  `plans/` ones.
- `python -m unittest discover -s .agents/hooks/tests` → 14/14.
- The `plans` route still fires: `skill_router.py --skills-for` on a `plans/**/*.md` path resolves to
  `plans` (and `docs-and-debt` for `plans/AGENTS.md`).
- Grep sweep: no guidance doc mentions a moved file. The only survivors are historical records in
  `plans/launch/MUSIC_LICENCE_ATTESTATION_PROGRESS.md`, `reviews/Fix-WorktreeLifecycleAutomation.md` and
  `reviews/Docs-skill-routes-mapper-coverage.md`, deliberately left as the record of what was decided.
- Every section name cited from another repo's doc resolves: `PLANS.md` really carries "Never leave the
  codebase out of sync" and "Breaking published-contract changes", and `HANDOFF.md` carries every handoff
  rule `plans/AGENTS.md` dropped.

## Reviews

`/docs-review` over `60acb8f6b..c2d2b0158` → `reviews/Docs-docs_polyrepo-ready.md`. Four findings, all fixed
and ticked; no open findings. Marker re-stamped to head after the base merge.

The other two PRs in this chain are now reviewed too, which they were not when this ledger last claimed
the chain was ready: agent-standards #5 → `reviews/Docs-polyrepo-ready-process.md` (one finding fixed —
`PLANS.md` hid the literal `## Reviews` heading its own merge gate reads, which is what caused `INST1`
here; one recorded as deferred — `PLANS.md` at 248 lines is 3× that repo's eighty-line split rule, which
Phase 4 settles), and #668 → an `## Incremental review` section on
`reviews/Docs-skill-routes-mapper-coverage.md` covering the two commits past its old watermark.

- `HOME1` — the new `## The repo's plan skills` section restated the `/resume-plan` and `/continue-roadmap`
  resolution rules their own `SKILL.md` files own (the duplication had come over verbatim from the deleted
  `plans/agents/PLAN.md`). Collapsed to three lines naming the skills by role; `plans/AGENTS.md` 79 → 75.
- `ACC1` — the header still said `PR: pending` while #669 was open and the body named it twice. The header
  now records #669, its base, and the retarget condition.
- `HOME2` — the handoff Stop hook was described under a heading about the plan graph. Heading widened to
  `## The plan hooks are machine gates`.
- `INST1` — this ledger's own section names had drifted from the mandatory progress template, and
  `plan_graph.py`'s review gate reads `## Reviews` literally, so a recorded review was invisible to it.
  Restructured to the template's headers, section names, and order.

## Decisions, discoveries, blockers, and deviations

- **The baseline counted non-blank lines, not `wc -l`.** Its 183/50/26 are exactly the non-blank counts
  of `PLAN.md`/`PROMPTS.md`/`ROADMAP.md`, whose totals are 233/57/34 — one consistent method, not an
  error, so 259 was sound for what it measured. The plan and roadmap now carry `wc -l` (324 total, 32 of
  them naming anything Concertable-specific) and say which method each figure is.
- **The "Watch for" trap in the old Next Steps did not exist as written.** Neither `plan_graph.py` nor
  `plan_handoff_stop.py` reads `PROMPTS.md`; the Stop hook hard-codes the pointer's shape (`plan_handoff_stop.py:339-347`)
  and both hooks are vendored *from* agent-standards, so the enforcement already sits with the doc's new
  home. Deleting `PROMPTS.md` broke no gate. What did need re-pointing was prose, not code.
- **PLANS.md at 248 lines is past agent-standards' own "earns its own file past about eighty lines"
  guideline.** It is one topic and the plan directed one file, so it landed as one. Splitting the ledger
  format or the blocker schema into their own nodes is a live option for Phase 4 to settle.
- **`handoff` was verified collision-free** against all skill names in `dotagents` (32), `react-agents`
  (14), `agent-standards` (31) and this repo (28) before it was added.
- **No new route row was added for `handoff`.** Phase 1's "add a `handoff` router" is the SKILL.md in
  agent-standards. A route row naming a skill the machine has not reinstalled yet would block writes to
  `plans/**` until the plugin cache refreshed, and the `^plans/` row plus the widened `plans` description
  already carry the trigger.
- **There are only two destinations, and "in-repo floor" is not one of them.** `POLYREPO_ROADMAP` §6
  (2026-08-18) already ruled it: everything re-homes to `standards/` (platform-wide) or to the owning
  service's repo, because the root is deleted. The plan now states that rule before its phases, and two
  earlier calls were wrong against it: Phase 5's first draft kept ~1,700 lines as "genuinely local", and
  Phase 1's own note called the 75-line `plans/AGENTS.md` "genuinely per-repo". Both are platform-wide —
  every repo runs a plan graph, merges, debugs by tier — with only the *values* differing. The test is
  **common across services**, never *does it name Concertable*.
- **All 28 skills (2,900 lines) are platform-wide; none is single-service.** That is the finding, not a
  classification detail: review family 813 lines (25 naming this repo), merge/PR family 634, test-debug
  family 1,022, git family 429 (zero), plan-workflow + misc 203, `package-cutover` 184. What a carved repo
  keeps is values — its `scripts/e2e.ps1`, its suite names, its hook and migration paths — named in a thin
  `AGENTS.md` on the `Concertable.Payment` model. This answers the ledger's old "generated or hand-kept?"
  question: neither, the content leaves.
- **Phase 2 was also written as though the root survives, and is rewritten.** "Re-anchor the three
  monorepo-shaped rows so the table works in both shapes" keeps one root table alive in two worlds. The
  mechanism already splits correctly: `agent-standards` vendors `skill_router.py` (provenance-hashed, via
  `vendor-hooks.ps1`) and ships **no** `skill-routes.json`, so the hook is platform-wide procedure and the
  table is per-repo data. The three area-floor rows are therefore values that die with the root — nothing to
  re-anchor. What is actually missing is the **convention** those 37 rows follow (area floor + layer route,
  every matching row fires, location-keyed rows can't port), which today lives only in the table's own notes
  and `docs/INDEX.md` — both in the deleted root. Phase 2 now publishes that convention plus a
  template/generator from `agent-standards`, and its gate generates a carved repo's table rather than
  replaying the monorepo's.
- **Phase 3 was rewriting a doomed file.** "Re-premise root `AGENTS.md` so the monorepo reads as packaging,
  not premise" spends effort on a hub that dies with the root. Re-homing its rules by the same test is the
  work; the wording is the cosmetic tier.
- **Codex needs no separate setup for this change.** The canonical guidance is `.agents/**` (Codex reads it
  directly; `.claude/skills/*` are stubs), both hooks already handle Claude and Codex, and Codex reaches the
  standards through per-domain junctions from `~/.agents/standards/agent-standards/*` into the local
  `agent-standards` checkout. Only the Claude plugin cache needs the one-line refresh.

## Resume prompt

```
cd C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready
Read @plans/docs/POLYREPO_READY_PLAN.md and @plans/docs/POLYREPO_READY_PROGRESS.md and do what its `## Next Steps` says.
```
