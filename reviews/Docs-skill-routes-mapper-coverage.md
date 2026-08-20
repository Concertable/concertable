# Docs review — Docs/skill-routes-mapper-coverage

Work order for the route-coverage change. Findings are `- [ ]` until fixed, `- [x]` when addressed.

**Reviewed up to commit:** `0853f10db70e54a4aabd61c8924b5077d39613c1`  _(2026-08-20)_

> Range: `origin/main...HEAD`. Scope: `.agents/skill-routes.json`, `docs/INDEX.md`, and the deletion of
> a spent review. All paths are meta, so this is a `docs-review`, not a `review` — but the route table
> is behaviour, so the surviving change gets real lenses rather than the close-out exemption.

## Findings

- [x] **DOC1 — HIGH — Lens A (silent under-match)** — `.agents/skill-routes.json`
  The `Mappers\.cs$` route added in the previous commit matched the plural spelling only, so the 29
  `XMapper.cs` files under `api/**/Mappers/` — the Api-layer response mappers — matched no route and
  loaded no rule at the moment of the write. The failure mode is the dangerous one: nothing fires, so
  nothing complains. Widened to `(Mappers?\.cs|/Mappers/[^/]*\.cs)$`; 95 of 95 mapper files now gate.

- [x] **DOC2 — HIGH — Lens A (the strategy, not the instance)** — `.agents/skill-routes.json`
  DOC1 is not a one-off, it is what suffix routing does. A table of name-shaped rules only ever covers
  the names someone thought to enumerate, and every miss is invisible. Measured before the fix: **1,436
  of 3,127** tracked source files (46%) matched no route at all, including every `*Service.cs` (116),
  `*Error.cs` (64), `*Entity.cs` (58), `*Handler.cs` (44) and `*Client.cs` (40).
  Fixed structurally rather than by enumerating twelve more suffixes: two **area floors**
  (`api/**/*.cs`, `app/**/*.{ts,tsx}`) and four **layer routes** keyed on the layer the project name
  already declares. `matching_routes` yields every match, so specific rows add to the floor rather than
  replacing it. Coverage **56% → 100%** of real source files, measured by replaying all 2,940 paths.

- [x] **DOC3 — MEDIUM — Lens A/C (the file's own contract is now wrong)** — `.agents/skill-routes.json` `_comment`
  The header still read "Path -> owning skill … a new concern is a new row here", which is first-match
  thinking. With floors, a path deliberately matches several rows and the floor always applies — so the
  next person adding a route would have written it assuming their row was the only one to fire. The
  comment now states that every matching row fires, why the table is layered, and that a layer/area row
  is preferred over a name-shaped one. It also documents `note`, which the hook supports and the
  contract never mentioned.

- [x] **DOC4 — MEDIUM — Lens A (a roadmap that overstates completion)** — `plans/docs/DOCS_ROADMAP.md`
  Four of five items read `[x]`, which says the epic is nearly shipped. It is not: the restructure split
  the corpus by portability but applied it as though this repo survives, and
  `plans/platform/POLYREPO_ROADMAP.md` records the ruling that it does not. Measured: ~259 lines of
  generic plan process (`plans/agents/PLAN.md` 96% generic, `PROMPTS.md` 98%, `plans/agents/ROADMAP.md`
  100%) sit in a repo with no future, while six sibling process docs already moved; three route rows are
  anchored on `^api/`/`^app/`/`^plans/`, none of which exist in a service repo; and the hub docs open by
  describing a monorepo. Added `docs/polyrepo-ready` with the measurements and the reason copying into
  eight repos is the wrong answer. Distinct from `POLYREPO_ROADMAP` 4c, which is about where a plan
  *document* lives, not how one is written.

## Verified, no finding

- **Lens A, skill resolution** — all **53** distinct skills named in the table resolve to an installed
  plugin skill or a deployed junction; none dangle. Bare vs qualified is right per the table's own rule:
  `csharp-style`, `csharp-naming`, `result-carriers`, `result-errors` and `dependency-injection` are
  single-home so they stay bare; `typescript-style` ships in both `react` and `react-standards`, so both
  are named.
- **Lens A, over-match** — the four layer routes match **0** files in any `*Tests` project
  (`.Api.UnitTests/` does not contain `.Api/`), so a test file gets its testing route and the floor, not
  a production-layer rule it should ignore.
- **Lens B, contradiction** — the four docs that describe the router (`AGENTS.md`, `api/AGENTS.md`,
  `app/AGENTS.md`, `.agents/README.md`) all say "maps path to skill and the write-time hook enforces
  it", which stays true and is not narrowed by the floors.
- **Lens A, reachability** — `docs_reachability.py` 0 errors, router tests 6/6, JSON valid.

## Known and accepted, not a finding

`GlobalUsings.cs` and `AssemblyInfo.cs` are excluded from the two area floors but still match their
**layer** route, so ~163 near-empty files would load 2–3 skills on a write. Left deliberately: these are
written once per project, and adding the same negative lookahead to four layer patterns would cost more
in table readability than it saves in noise. Recorded here rather than silently shipped.

## Incremental review — 2026-08-20

Range `08ff41bf3..60acb8f6b` (2 commits): the re-stamp of this file, and `60acb8f6b` spinning off
`docs/polyrepo-ready` as `plans/docs/POLYREPO_READY_PLAN.md` (+100) and its ledger (+61). Gates in this
worktree: `plan_graph.py` 0 errors/0 warnings, `docs_reachability.py` 0 errors.

- [wontfix] **DOC5 — LOW — Lens A (imprecise label)** — `plans/docs/POLYREPO_READY_PLAN.md:20`,
  `POLYREPO_READY_PROGRESS.md` baseline table
  The baseline column is headed `Lines` but the figures are **non-blank** lines: 183/50/26/53 are exactly
  `grep -cv '^[[:space:]]*$'` on `plans/agents/PLAN.md`, `PROMPTS.md`, `plans/agents/ROADMAP.md` and
  `plans/AGENTS.md`, whose `wc -l` totals are 233/57/34/71. The measurement is internally consistent —
  every file counted the same way — so the 259 figure is sound for what it measures; only the label is
  ambiguous. It did mislead downstream: the follow-up branch read the totals as a correction and recorded
  that the baseline had been "wrong".
  **Not fixed here** — [#669](https://github.com/Concertable/concertable/pull/669) replaces this whole
  table with `wc -l` figures within minutes of this PR landing, and editing the same lines on this branch
  buys a merge conflict for no reader benefit. Fixed instead where it survives: #669 now states the
  baseline counted non-blank lines rather than calling it an error.

- [wontfix] **DOC6 — LOW — Lens A (stale claim)** — `POLYREPO_READY_PROGRESS.md` `## Next Steps`
  "Watch for: the Stop hook and `plan_graph.py` both read the handoff pointer's shape, and `PROMPTS.md`
  is currently its only definition." Neither hook reads `PROMPTS.md`; `plan_handoff_stop.py` hard-codes
  the pointer's shape. A planning-time caution that turned out not to exist.
  **Not fixed here** — #669 deletes this `## Next Steps` entirely and records the disproof as a
  discovery. Same reasoning as DOC5.

Nothing else in the range needs a change: the ledger's headers satisfy the graph, the plan cites only
`POLYREPO_ROADMAP` (a sibling epic's roadmap, not its own — the coupling rule bars citing its own), and
the re-stamp is honest about what it covered.

Re-stamped to `0853f10db` after merging `origin/main` (9 commits: the #663 platform sync and the
#667 review sweep). That merge authored nothing on this branch — the branch's diff against `main`
is the same seven meta paths — so the review above still covers everything this PR changes.
