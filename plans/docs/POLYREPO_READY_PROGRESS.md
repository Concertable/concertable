# Polyrepo-ready guidance corpus — progress

- Plan: `plans/docs/POLYREPO_READY_PLAN.md`
- Roadmap: `plans/docs/DOCS_ROADMAP.md`
- Roadmap item: `docs/polyrepo-ready`
- Worktree: `C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready-nodes`
- Branch: `Docs/docs_polyrepo-ready-nodes`
- PR: this repo — not opened yet for this slice; producer `Concertable/agent-standards`
  [PR #6](https://github.com/Concertable/agent-standards/pull/6). Phase 1 shipped as this repo's #669
  (`1d15a7920`) and agent-standards #5 (`1d44caa38`)
- Dependency/package gates: **#6 merges first.** This branch deletes the six skill bodies whose procedure
  #6 publishes; landing this one first leaves the repo with no review procedure at all. No open
  `chore/platform-sync-*` PR.
- Last reconciled: 2026-08-20 after implementing N1 family 1 in both repos, from `gh pr view`,
  `sync-generated.ps1 -Check`, and the hook runs below.

## Current state

**Phase 1 is merged in both repos. N1 family 1 — the review family — is implemented in both repos and
delivery-gated on the producer PR.**

Producer `agent-standards` #6 publishes seven docs under `standards/process/review/` with a router each;
consumer (this branch, `b1ecd3303`) deletes 941 lines — the six skill bodies, their `.claude/skills` stubs,
and `reviews/AGENTS.md` + its `CLAUDE.md` sibling — and re-points the two path citations in `docs/INDEX.md`.
Skill names are unchanged, so every prose citation of `/review`, `/docs-review` and the rest still resolves.

**Nothing stays as values.** That is the slice's main finding and it reshapes the remaining families: the
moved procedure resolves its repo-specific inputs mechanically at run time rather than reading a values
file. See `## Decisions`.

**N1 families 2–6 and N2–N8 are untouched.** The plan carries the measured inventory; 2,472 of N1's original
3,285 lines remain, in five families.

## Next Steps

1. **Land N1 family 1, in this order.** agent-standards [#6](https://github.com/Concertable/agent-standards/pull/6)
   merges first — its CI is hook tests plus `sync-generated.ps1 -Check`, both green locally. Then open this
   branch's PR against `main`, read its CI, and land it through `/merge-docs`; its docs review is recorded
   below. Afterwards Tommy runs `/plugin marketplace update agent-standards` per machine so the seven new
   routers resolve in Claude; Codex needs nothing beyond that repo being on merged `main`.

2. **Then the `^reviews/.*\.md$` route row.** It is deliberately not in this slice: a route row naming a
   skill the plugin cache has not reinstalled yet hard-blocks every write to `reviews/**` until the refresh,
   the same trap that kept `handoff` out of the table in Phase 1. The row is what restores automatic
   delivery of the review-file lifecycle, which used to come from `reviews/AGENTS.md` sitting in the
   directory. **Pass condition:** `review-lifecycle` resolves after `/plugin marketplace update
   agent-standards` on this machine, and Tommy has confirmed the same on any other machine he works from.

3. **N1 family 2 — merge/PR (634 lines): `merge`, `merge-docs`, `pr-preflight`, `create-gh-pr`.** Same
   producer-then-consumer shape. Try the family-1 answer first — resolve values mechanically, ship no values
   file. The queue and platform-sync are platform facts, not one service's.

4. **Then N1 families 3–6**, in the plan's order: test-debug (1,022 — settle the script-path question first,
   and it is the one family where a named value may be unavoidable) → git (429, plus reconciling `dotagents`'
   `commit-push`/`sync`/`pull-main` overlap) → plan-workflow (203, and it should absorb
   `resume-plan/references/plan-progress-checkpoint.md`, 138 lines cited by fifteen skills) →
   `package-cutover` (184).

5. **N2 can run in parallel**; N3–N6 after N1; N7 when roadmap §4c unblocks; N8 last as the only evidence.
   N6 still carries the one open question to put to Tommy rather than answer: `OVERVIEW.md`, `USP.md` and
   `DEEP_RESEARCH_PROMPT_GUIDE.md` are product narrative, neither platform standard nor service-specific.

## Completed work

- **N1 family 1 producer — agent-standards #6** (`bf1927e`, `f477d57`, `be116ea`). Seven docs under
  `standards/process/review/` with a router each: `FULL.md` (the two-layer branch review) ← `review`;
  `INCREMENTAL.md` ← `incremental-review`; `STAGED.md` ← `big-review`; `UNATTENDED.md` ← `big-review-all`;
  `DOCS.md` ← `docs-review`; `ADDRESSING.md` ← `address-review`; `LIFECYCLE.md` ← this repo's
  `reviews/AGENTS.md`, routed by a new skill `review-lifecycle`. README charter and both marketplace
  manifests reworded to admit the review family. The purchase-time snapshot rule folded into
  `standards/dotnet/structure/SERVICE_BOUNDARIES.md`.
- **N1 family 1 consumer — this branch** (`b1ecd3303`). The six `.agents/skills/*` bodies, their six
  `.claude/skills/*` stubs, and `reviews/AGENTS.md` + `reviews/CLAUDE.md` deleted — 941 lines. Root
  `AGENTS.md` skill list extended by the review family; `docs/INDEX.md` — the "review files as work orders"
  row split into a producing row and a consuming row naming the skills, and the review-loads-the-same-table
  gate row de-linked from the deleted directory.
- **Phase 1 producer — agent-standards PR #5.** `standards/process/PLANS.md` 78 → 248 lines, absorbing the
  method from `plans/agents/PLAN.md` and the roadmap tier from `plans/agents/ROADMAP.md`; new
  `standards/process/HANDOFF.md` (57); new `handoff` router; `plans` router description widened; README
  charter reworded to separate roster from method and to record why a fourth process repo was rejected.
- **Phase 1 consumer — this repo's #669.** `PROMPTS.md`, `plans/agents/PLAN.md` and `plans/agents/ROADMAP.md`
  deleted. `plans/AGENTS.md` rewritten as the in-repo floor (75 lines). Every citation re-pointed: root
  `AGENTS.md`, `docs/INDEX.md` (2 rows), the `^PROMPTS.md$` route row removed and the `plans` row's note
  rewritten, eight skills, `api/Concertable.Shared/TECH_DEBT.md`, and five roadmap headers.

## Verification

N1 family 1, producer (`agent-standards`):

- `.agents/sync-generated.ps1` → 23 files written on the first run; `-Check` reports **128 current
  (39 skills, 39 docs)** after every subsequent edit. The generator's one-doc-one-router invariant is what
  forced `review-lifecycle` to exist as its own skill.
- hook tests **161/161**.
- Skill names checked collision-free before adding: none of `review`, `docs-review`, `big-review`,
  `big-review-all`, `incremental-review`, `address-review`, `review-lifecycle` exists in `dotagents` (32),
  `react-agents` (14), `agent-standards` (32 before this) or Claude Code's built-ins (`/code-review`,
  `/security-review`, `/simplify`).

N1 family 1, consumer (this repo):

- `python .agents/hooks/docs_reachability.py --root <worktree>` → **0 errors, 26 warnings**, the same 26
  pre-existing `plans/` warnings as the Phase 1 baseline. Deleting `reviews/AGENTS.md` orphaned nothing.
- `python .agents/hooks/plan_graph.py --root <worktree>` → 0 errors, 0 warnings.
- `python -m unittest discover -s .agents/hooks/tests` → **14/14**.
- No route row in `.agents/skill-routes.json` names any deleted skill (checked by parsing the table, not by
  eye), and `skill_router.py --skills-for` still resolves.
- Every `##`/`###` heading of the six deleted skills maps to a section of a moved doc. The only one not
  carried across is `review`'s `## When to use`, whose triggers are the router's `description` — which is
  what makes a skill load at all, so it is the right home rather than a lost section.

## Reviews

`/docs-review` over `133b018da..eb089c56a` → `reviews/Docs-docs_polyrepo-ready-nodes.md`. Three findings,
all fixed and ticked; no open findings. **Run from the moved copy of the procedure**
(`standards/process/review/DOCS.md` on the producer branch), because this branch deletes the in-repo
`docs-review` skill and #6 has not merged — which discharges the family's own gate, "`docs-review` still
runs end-to-end from the moved copy", on this slice's diff.

- `ACC1` — `DOCS_ROADMAP.md`'s `docs/polyrepo-ready` row still measured N1 at 28 skills / 3,285 lines after
  six of them left. The roadmap is the durable tracker, so its figures now move with the plan's.
- `HOME1` — the new `docs/INDEX.md` row named `address-review` beside `review-lifecycle` as owners of one
  rule, in a file whose premise is one owner per rule. `review-lifecycle` owns it; `address-review` is
  named as the procedure that obeys it.
- `INST1` — Next Step 2 gated the route row on "the plugin refresh has happened on every machine", which
  nothing can check. It now carries an objective pass condition.

The producer PR is reviewed in its own repo — `agent-standards` `reviews/Docs-polyrepo-ready-review-family.md`.

Phase 1's reviews, all clean and merged: this repo's #669 → `reviews/Docs-docs_polyrepo-ready.md`, four
findings all fixed; agent-standards #5 → `reviews/Docs-polyrepo-ready-process.md`, one finding fixed and one
recorded as deferred (`PLANS.md` at 248 lines is 3× that repo's eighty-line split rule); #668 → an
`## Incremental review` section on `reviews/Docs-skill-routes-mapper-coverage.md`.

## Decisions, discoveries, blockers, and deviations

- **A moved workflow needs no values file — this is the reusable answer for N1.** The plan expected the six
  review skills to be "parameterised over the values a repo supplies (its review file location, its hook
  paths, its area globs)". None of those turned out to be a value. The review-file location is one
  convention every repo should share, so the standard states `reviews/<branch-slug>.md` outright; the hook
  paths are fixed by `vendor-hooks.ps1` and identical everywhere; and the area globs are never named at all,
  because Step 2 resolves what to read from the repo's own route table, its `AGENTS.md` tree, and whichever
  architecture doc its root `AGENTS.md` names. **Inventing a parameter that has one value in every repo is
  worse than stating the value.** Reach for a named value only where a script's path genuinely cannot be
  discovered.
- **Lenses B–E were four copies of rules that already had owners.** The review skill restated the
  service-boundary roster, the module rules, the seeding rules and the C#/frontend conventions as
  "recurring defects". Every one of those is owned by a skill the route table already pairs and returns.
  The moved lenses therefore hold **no rules of their own** — each names a class of defect and cites the
  standard that owns it. That is 25 lines of Concertable names becoming zero, and it removes four
  drift surfaces rather than relocating them.
- **Two rules had the review skill as their only home.** Moving a doc is when this surfaces, and it is the
  reason the "loses no rule" gate is worth running literally.
  - *Purchase-time snapshots* — a consumer holds by-value copies of a producer's fields rather than
    nav-chaining back into its runtime. Verified live (`TicketEntity` carries `ConcertName`, `ArtistName`,
    `VenueName`) and folded into `SERVICE_BOUNDARIES.md`, beside the rest of the cross-service read stance.
  - *The `IUnitOfWork` no-op rule* — **dropped, not moved.** It named `ApplicationDbContext`, which exists
    nowhere in the codebase; every module binds its own `UnitOfWorkBehavior`. A dead rule that read exactly
    as maintained as a live one, which is what the docs standard's "check the code before writing the rule
    down" is about.
- **`reviews/AGENTS.md` joined the family and left with it.** It was not in the node inventory — N1 is
  `.agents/skills/` — but all 64 lines are the review-file lifecycle, the same rule in every repo, and it is
  the companion the moved docs cite most. Leaving it would have stranded the family's most-referenced rule
  in the deleted root.
- **A workflow skill's invocation surface is a per-machine install, not a per-repo file.** The six names
  survive unchanged, so prose citations hold, but they now resolve from the `agent-process` plugin rather
  than from `.agents/skills/`. Bare `/review` in Claude therefore needs the plugin refresh; the plugin
  namespaces it as `agent-process:review`. The alternative — a thin router file kept in each repo — is eight
  copies of a file whose only content is a path, which is what this epic exists to end.
- **The route row for `reviews/**` is deliberately deferred, not forgotten.** `reviews/AGENTS.md` was
  delivered by *location* (a harness loads the `AGENTS.md` of a directory it works in); a route row is the
  mechanism that replaces it. But a row naming a skill the plugin cache has not reinstalled hard-blocks
  every write to that path, which is exactly why Phase 1 added no row for `handoff`. It is Next Step 2.
- **`.claude/agents/code-reviewer.md` is a real per-repo dependency and is now named as one.** Layer 1 spawns
  it, and it is a repo file rather than anything a harness supplies, so a carved repo without it silently has
  no native review layer. The generic doc names it explicitly — per the docs standard, a harness identifier
  is not a product identifier and stripping it would have deleted the requirement. Whether the plugin should
  *ship* the agent definition is a live question for N1 as a whole, not for this family.
- **`plan-progress-checkpoint.md` is the next family's real work.** 138 lines under
  `.agents/skills/resume-plan/references/`, cited by fifteen skills, and **not** in `standards/process/PLANS.md`
  — the first draft of the moved docs claimed "the plan checkpoint the `plans` standard requires", which was
  false. They now name the checkpoint procedure the repo's plan floor names, which is true today and stays
  true after N7. Family 5 should move the procedure itself.
- **`standards/process/PLANS.md` fails `docs_reachability.py` on two pre-existing errors** — the literal
  `[PR #<number>](<url>)` link shape it documents reads as a dead link. Not this slice's node, and not a live
  gate: `agent-standards` CI runs hook tests and `sync-generated.ps1 -Check`, not the reachability checker on
  itself. Worth a one-line fix whenever that doc is next opened.

## Resume prompt

```
cd C:/Users/TommySeery/source/repos/Concertable.worktrees/Docs/docs_polyrepo-ready-nodes
Read @plans/docs/POLYREPO_READY_PLAN.md and @plans/docs/POLYREPO_READY_PROGRESS.md and do what its `## Next Steps` says.
```
