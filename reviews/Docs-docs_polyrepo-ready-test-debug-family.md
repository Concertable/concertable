# Docs review — Docs/docs_polyrepo-ready-test-debug-family

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `55de65d0850b82cf10550d207ceb5a560f9ceb8d`  _(2026-08-20)_

> Range reviewed: `d1422b6b5..55de65d08` (7 commits), plus the producer half it exists to consume —
> `Concertable/agent-standards` `30734a9..5cf3608` (2 commits, PR #8 — the second commit is this review's
> own fixes). One review across both branches
> rather than two, because three of the four findings are defects in the moved docs; the producer branch
> carries a companion file pointing here.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).
>
> Run from the moved copy of the procedure — `standards/process/review/DOCS.md` in `agent-standards`,
> merged as #6 — because this repo no longer holds a `docs-review` skill and the plugin cache on this
> machine has not been refreshed since that merge.

## Findings

- [x] **INST1 — MEDIUM — Lens F (followable instruction)** — `standards/process/testing/API_E2E.md:~180`
  Step 2's single-test command carries `--settings <the repo's run settings>` with no way to resolve the
  placeholder. The doc states two paragraphs earlier that the entrypoint passes those settings and that two
  E2E applications must never boot at once — so a reader who cannot find the file either omits the flag and
  trips the hazard the doc just warned about, or stops. **Fix:** say where to read it from — the entrypoint
  itself — so the placeholder has a mechanical resolution like every other value in the family.

- [x] **INST2 — MEDIUM — Lens B (contradiction) + F** — `standards/process/FAILING_TESTS.md:10`
  The new tier table routes "Unit or integration" to `testing/INTEGRATION.md`, whose closing note says
  "Unit tests are a different tier again, and run directly against their own projects" — so a reader with a
  red unit test is sent to a doc that tells them they are in the wrong place, and nothing tells them where
  the right one is. The pre-move `plans/AGENTS.md` had the same routing and the same gap; filing the two
  side by side is what exposed it. **Fix:** `INTEGRATION.md` states what the unit tier shares with it
  (the same runner, the same filter grammar, the same read-the-failure order) and what it does not (no
  container, no fixture), instead of deflecting.

- [x] **CON1 — MEDIUM — Lens B (contradiction)** — `standards/process/testing/REGRESSION.md:~78`
  `REGRESSION.md` says the entrypoint's isolated retry means a red regress must **not** be re-run by hand.
  `FAILING_TESTS.md`'s "Flaky-versus-real triage" mandates exactly that re-run — the failed scenario alone
  on a fresh stack — as the way to prove a blip. Both are right and neither says so: the entrypoint has
  already performed that triage. Unreconciled, the two docs read as a direct conflict, which is the defect
  this review exists to catch. **Fix:** name the relationship in both places — the retry *is* that triage,
  performed automatically, and its verdict stands.

- [x] **INST3 — LOW — Lens F (followable instruction)** — `standards/process/testing/BOTH_LAYERS.md:~60`
  Step 0 says both tiers need "the same real secrets CI injects" and later says to "confirm the secrets are
  set before debugging anything else", but never names them; only `API_E2E.md` does. A reader whose run dies
  on a payment-auth error has an instruction with no subject. **Fix:** point at the doc that names them.

## Lenses checked with no finding

- **Lens A (accuracy vs reality)** — every command, terminal line, log-file convention and file-system claim
  in the six new docs was checked against the scripts and workflows it describes, not read for
  plausibility: the entrypoint's `ui`/`api` grammar and its argument-less usage listing;
  `integration.ps1 list`; all four of the regress terminal strings, including `BASELINE DRIFT` and the
  `REGRESSED:` block; the separate retry log beside the run log; that the entrypoint sets headless unless
  `-Headed`; that it passes the run settings; that CI's browser job depends on the service job; and that
  the screenshot directory is anchored to the build output. `docs_reachability.py` reports 0 errors with a
  warning set byte-identical to the branch base's.
  `IDE_DISCOVERY.md`'s enumeration snippet was **executed** rather than reasoned about, and returns exactly
  the three projects the deleted skill hard-coded — which is what makes replacing that list with discovery
  safe rather than merely tidier.
- **Lens C (right home)** — the family adds no rule to a hub. Two candidate duplications were removed
  rather than moved: `integration-debug`'s fixture-and-mock roster (owned by the integration-testing
  standards) and its authoring conventions (same). `plans/AGENTS.md`'s by-tier list is deleted rather than
  kept beside the table that now owns it.
- **Lens D (concision of reloaded docs)** — root `AGENTS.md` gains six skill names and no prose;
  `plans/AGENTS.md` is four lines shorter; `docs/INDEX.md` gains two rows and widens two.
- **Lens E (dangling references)** — the six published docs cite no plan filename, phase number or
  PR. Three classes of citation were dropped for exactly this reason and are recorded in the ledger:
  per-repo memory ids, a conventions file that no longer exists, and both suites' scenario and module
  rosters.

## Incremental review — 2026-08-20

Re-stamped to `55de65d08`. The two commits since the original stamp changed this file and the plan ledger
only — the checkpoint tail the plan protocol requires — so there is nothing reviewable in them and no
finding to add. Recorded rather than silently re-stamped, because "nothing changed" is a judgement someone
should be able to check.
