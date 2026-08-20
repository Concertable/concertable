# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready`
- Branch: `Docs/docs_polyrepo-ready`
- PR: this repo — [#669](https://github.com/Concertable/concertable/pull/669), based on
  `Docs/skill-routes-mapper-coverage` (retarget to `main` when [#668](https://github.com/Concertable/concertable/pull/668)
  lands); producer — `Concertable/agent-standards` [PR #5](https://github.com/Concertable/agent-standards/pull/5)
- Dependency/package gates: this branch's docs point at `standards/process/PLANS.md` and `HANDOFF.md`, which
  ship from agent-standards PR #5. **#5 merges first**, then #668 (this branch's base), then this one.
- Last reconciled: 2026-08-20, from `gh pr view` on #5/#668/#669, `gh pr checks`, and the hook gates below.

## Current state

**Phase 1 is implemented across both repos, reviewed, and delivery-gated on the producer PR.** Phases 2–4
are untouched and unblocked.

This branch is cut from `Docs/skill-routes-mapper-coverage` (PR #668), not from `origin/main`, because
that PR carries this plan *and* the route table this work edits — branching from `main` would have
conflicted on `.agents/skill-routes.json`. Its commits drop out of this PR's diff as soon as #668
merges. **`test.yml` only triggers on PRs based on `main`** (`pull_request: branches: [main]`), so #669
reports "no checks reported" until it is retargeted — retarget it the moment #668 lands, then read its CI.

## Next Steps

1. **Deliver Phase 1, in this order.** agent-standards
   [PR #5](https://github.com/Concertable/agent-standards/pull/5) merges first — the docs on this branch name
   `standards/process/PLANS.md` and `HANDOFF.md`, so landing this one first would point at a doc that has not
   shipped. Then [#668](https://github.com/Concertable/concertable/pull/668) merges (it is this branch's base
   and carries the route table this work edits), retarget
   [#669](https://github.com/Concertable/concertable/pull/669) to `main`, read the CI that then triggers for
   the first time, and land it through `/merge-docs`. Its docs review is recorded below and clean, so the
   review gate is met. Tommy needs one harness action per machine afterwards so the new router is live:
   `/plugin marketplace update agent-standards` (Claude). Codex reads the standards through junctions into
   the local `agent-standards` checkout, so it needs nothing beyond that repo being on merged `main`.

2. **Phase 2 — re-anchor `^api/`, `^app/`, `^plans/`.** Key the two area floors on what a file *is*, the
   way the four layer routes already do. Verify by replaying every tracked path through the table twice:
   once against the monorepo tree, once against a tree with the prefix stripped, and require 100% both
   times. Do it on a branch that has #668's version of the table.

3. **Phases 3 and 4** as the plan states. Phase 4 is the only one that produces evidence rather than
   edits; do not call the item done without it.

**Open question for Tommy, not blocking:** each carved service repo will need its own thin
`plans/AGENTS.md` and hook wiring naming its own script paths — the file this phase left behind is 75
lines of genuinely local content, times eight repos. Vendoring already handles the hooks. Whether that
thin file is **generated** from a template at carve time or hand-kept per repo is a real choice, and
Phase 4 is where it gets tested either way.

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
and ticked; no open findings.

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

- **The measured baseline in the plan was wrong and is corrected.** The 259 lines it recorded were
  actually 324 (`PLAN.md` 233, `PROMPTS.md` 57, `ROADMAP.md` 34), of which 32 carry any
  Concertable-specific name or command. The plan and roadmap now carry the `wc -l` figures.
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
- **Codex needs no separate setup for this change.** The canonical guidance is `.agents/**` (Codex reads it
  directly; `.claude/skills/*` are stubs), both hooks already handle Claude and Codex, and Codex reaches the
  standards through per-domain junctions from `~/.agents/standards/agent-standards/*` into the local
  `agent-standards` checkout. Only the Claude plugin cache needs the one-line refresh.

## Resume prompt

```
cd C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready
Read @plans/docs/POLYREPO_READY_PLAN.md and @plans/docs/POLYREPO_READY_PROGRESS.md and do what its `## Next Steps` says.
```
