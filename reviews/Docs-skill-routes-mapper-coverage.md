# Docs review — Docs/skill-routes-mapper-coverage

Work order for the route-coverage change. Findings are `- [ ]` until fixed, `- [x]` when addressed.

**Reviewed up to commit:** `6f46455e91481210ae82164aa3fa17625ebc6285`  _(2026-08-19)_

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
