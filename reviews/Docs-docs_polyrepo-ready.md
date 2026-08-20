# Docs review — Docs/docs_polyrepo-ready

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `c2d2b01589e18612e18a579c9bb1e53168dfa88b`  _(2026-08-20)_

> Range reviewed: `60acb8f6b..c2d2b0158` (2 commits).
> All four findings fixed on this branch; no open findings.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **HOME1 — MEDIUM — right home / duplication** — `plans/AGENTS.md:46-54`
  The new `## The repo's plan skills` section restates the resolution semantics the in-repo skills
  already own: `/resume-plan`'s ledger/plan/worktree resolution and the `- Plan:` grouping are
  `.agents/skills/resume-plan/SKILL.md:13-27` (which states them *more* precisely — it also covers "the
  invocation also named a worktree" and "restore that exact branch", which this copy drops), and
  `/continue-roadmap [@plans/<X>_ROADMAP.md] [preferred item]` is `continue-roadmap/SKILL.md:16-17`. The
  paragraph came over verbatim from the deleted `plans/agents/PLAN.md:205-208`, so the duplication moved
  rather than resolved — now into a harness-reloaded area floor whose own opening line says it "carries
  only what is true of *this* repo: its folder layout, its hooks, its scripts, and its skill names".
  Fix: keep the skill *names and roles* (one line — `/resume-plan` resumes a ledger, `/continue-roadmap`
  creates the next roadmap item's plan, reviews route `/review` · `/big-review` · `/incremental-review`,
  docs `/docs-review` + `/merge-docs`) and delete the restated resolution rules.
  **Fixed** — the section is three lines naming the skills by role and handing resolution back to each
  skill; `plans/AGENTS.md` is 75 lines.

- [x] **ACC1 — MEDIUM — accuracy / self-contradiction** — `plans/docs/POLYREPO_READY_PROGRESS.md:6`
  The header still reads `PR: this repo — pending`, but PR
  [#669](https://github.com/Concertable/concertable/pull/669) is open on this branch and the ledger's own
  `## Current state` names it twice ("PR #669 reports no checks until it is retargeted"). The header is
  the ledger's operational truth and `plan_graph.py` does not check it. Fix: record `#669` with its base
  (`Docs/skill-routes-mapper-coverage`, retarget to `main` when #668 lands) in the header.
  **Fixed** — header now records #669, its base, and the retarget condition.

- [x] **HOME2 — LOW — right home** — `plans/AGENTS.md:21-30`
  `plan_handoff_stop.py` is now described under the heading `## The plan graph is machine-checked`, which
  is about `plan_graph.py`. The handoff hook is the other machine gate in this repo and a reader scanning
  headings for it won't find it there. Fix: widen the heading to cover both hooks (e.g. `## The plan
  hooks are machine gates`).

- [x] **INST1 — MEDIUM — followable instruction / accuracy** — `plans/docs/POLYREPO_READY_PROGRESS.md`
  The ledger's own section names had drifted from the mandatory progress template
  (`.agents/skills/resume-plan/assets/progress-template.md`): `## Review state` for `## Reviews`,
  `## Completed milestones` for `## Completed work`, `## Decisions and discoveries` for
  `## Decisions, discoveries, blockers, and deviations`, `## Next Steps` last instead of second, and no
  `- Dependency/package gates:` / `- Last reconciled:` header or `## Resume prompt`. This is not cosmetic:
  `plan_graph.py:78-99` reads `## Reviews` literally, so a review recorded under any other heading is
  invisible to the pre-merge review gate — the hook failed the ledger the moment `## Next Steps` named
  `/merge-docs` with a clean review already recorded above it. Fix: restructure to the template's headers,
  names and order. **Fixed** — restructured; `plan_graph.py` 0 errors.

## Verified clean

- **Every moved-content citation resolves.** `standards/process/PLANS.md` really has the sections now
  cited by name — "Never leave the codebase out of sync" (`pr-preflight`) and "Breaking
  published-contract changes" (`package-cutover`, `api/Concertable.Shared/TECH_DEBT.md`) — and
  `HANDOFF.md` carries every rule `plans/AGENTS.md` dropped: the two-line pointer, the foreign-owner
  clause, the blocker/`Paused:` exceptions, the `/worktree create` opener. Nothing was deleted without a
  new home; `PLANS.md`/`HANDOFF.md` carry no Concertable-specific path, which is the point of the phase.
- **No dangling reference and no dead anchor.** The only surviving mentions of `PROMPTS.md` /
  `plans/agents/*` are historical records in ledgers, the roadmap's struck line, and spent review files.
  No doc anywhere links a heading anchor into `plans/AGENTS.md`, so renaming its sections broke nothing.
- **Measured claims check out exactly**: deletions 233/57/34 = 324, `plans/AGENTS.md` 71 → 79,
  `PLANS.md` 78 → 248, `HANDOFF.md` 57, root `AGENTS.md` 147.
- **The CI claim is true**: `.github/workflows/test.yml` triggers `pull_request: branches: [main]` only,
  and `gh pr checks 669` reports "no checks reported" — the ledger's retarget note is correct.
- Gates: `plan_graph.py` 0 errors/0 warnings · `docs_reachability.py` 0 errors, 26 pre-existing `plans/`
  warnings · hook tests 14/14 · `api/initial-migrations.ps1` and the progress-template/checkpoint paths
  all exist.
