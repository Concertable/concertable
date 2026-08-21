# Docs review — Docs/docs_polyrepo-ready-n3-api-floor

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `3f511247aa772efc58980fa58f32a14fafa804d2`  _(2026-08-21)_

> Range reviewed: `69df07b8..3f511247`.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

Polyrepo node N3: deletes `api/AGENTS.md` + `api/CLAUDE.md` (the backend "floor" pointer doc), adds one
route row to `.agents/skill-routes.json` firing `microservice-boundaries` on the universal shared tier, and
repoints inbound guidance-doc links that used to target `api/AGENTS.md`. The floor rules now come from the
route table over the `dotnet` plugin skills.

## Verified clean (folded in, no finding)

- `docs_reachability.py --root <checkout>`: **0 errors**, 24 warnings — all in `plans/` (warn-only, acceptable).
- Route row: JSON parses; route count **38** (37 → 38 as the ledger claims). `Concertable\.(Kernel|Contracts)/.*\.cs$`
  matches the real universal tier (`api/Concertable.Shared/src/Concertable.Kernel/**`,
  `.../Concertable.Contracts/**`) and does **not** over-match any per-service `*.Contracts`
  (`Concertable.B2B.Deal.Contracts`, `Concertable.Auth.Contracts`, etc. — none place `Kernel`/`Contracts`
  immediately after `Concertable.`).
- Repointed targets exist and are accurate: root `AGENTS.md` backend bullet, `docs/INDEX.md` row (`migrations`,
  `dotnet:microservice-boundaries`, `dotnet:seeding` — bare `migrations` matches existing INDEX line 103),
  Deal/Payment/Search `ARCHITECTURE.md` line-12 repoints, the four service `AGENTS.md` now inheriting root only,
  and `api/ARCHITECTURE.md` dropping its backend-floor bullet. Roadmap north-star now → root `ARCHITECTURE.md`;
  its quote matches root `ARCHITECTURE.md:4-5` verbatim.
- Lens B: no surviving service `AGENTS.md` or sibling still asserts an `api/`-level floor doc must be read.

## Findings

> **Disposition (2026-08-21):** ACC2 (docs) fixed in this PR. **ACC1 (`.yml`, CI) and ACC3 (`.cs` comment,
> source) are out of this PR's meta-only scope** — fixing them here would force the docs re-home through the
> full merge queue's E2E. They are dangling references this deletion creates, fixed in the dedicated code/CI
> follow-up branch `Fix/polyrepo-ready-n3-code-floor-refs`. Delete this review file once that PR lands.

- [x] **ACC1 — HIGH — Lens A / Lens F** — `.github/workflows/claude-review.yml:34` — _→ follow-up branch `Fix/polyrepo-ready-n3-code-floor-refs`_
  The automated PR-review bot's prompt instructs "Read, and review against, the docs that ARE in the checkout:
  repo-root `AGENTS.md`, `api/AGENTS.md`, `api/ARCHITECTURE.md`, ...". This PR deletes `api/AGENTS.md`, so the
  bot is told to load a file that no longer exists — and the backend floor rules it carried (shared-is-the-intersection,
  the seeder trigger rule, migrations) silently drop out of the review harness on every future PR. **Fix:** remove
  `api/AGENTS.md` from that list; the same prompt already routes rules via `.agents/skill-routes.json`, which now
  fires `microservice-boundaries` on the shared tier.

- [x] **ACC2 — MED — Lens A — FIXED in this PR** — `api/Concertable.Search/ARCHITECTURE.md:60`
  Still cites `(api/AGENTS.md)` as the authority for "depends on no other data service's runtime", a doc this PR
  deletes. The PR repointed line 12 of this very file (to `dotnet:http-api`) but missed this one; it is a plain
  inline-code citation, not a markdown link, which is why `docs_reachability.py` did not catch it. **Fix:** repoint
  to the `microservice-boundaries` skill, matching the sibling line-12 repoint and the `Payment/ARCHITECTURE.md`
  change in this same PR.

- [x] **ACC3 — LOW — Lens A** — `api/Concertable.B2B/.../Controllers/StripeAccountController.cs:12` — _→ follow-up branch `Fix/polyrepo-ready-n3-code-floor-refs`_
  XML doc-comment reads "Rationale in `api/CLAUDE.md` ("Shared code is the intersection, never the union")". That
  file is deleted by this PR (and `api/CLAUDE.md` never held the rule — it was one line `@AGENTS.md`; the rule lived
  in `api/AGENTS.md`), so the citation now dangles. Out of the strict docs-file scope (a `.cs` comment) but a real
  reference broken by this change. **Fix:** repoint the citation to the `microservice-boundaries` skill, or drop it.
