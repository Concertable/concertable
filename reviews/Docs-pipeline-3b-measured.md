# Code review — Docs/pipeline-3b-measured

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `PENDING`  `(2026-09-09)`
**Judgment:** `approved`

## Review pass — 2026-09-09 — docs

**Candidate base:** `3095cb655fe863f3fbbc07c3425fa86688c94dfa`
**Candidate head:** `74231c326a05041321508e2fbfe39e0e806c915f`
**Candidate branch:** `Docs/pipeline-3b-measured`
**Candidate scope:** `all`
**Candidate path-set:** `plans/platform/PIPELINE_REDESIGN_PLAN.md, reviews/Perf-MergeQueueLatency.md` `(2 paths)`
**Work-order path:** `reviews/Docs-pipeline-3b-measured.md`
**Work-order mode:** `new`
**Pass judgment:** `changes-requested`

Scope guard: one surviving path (`git diff --diff-filter=ACMRT`) — the plan — so this is an ordinary docs
review, not an exempt pure close-out. Nothing runtime, package, migration or CI-test-selection is in the
candidate. Lenses run as the strong parent; subordinate dispatch is disabled by session policy.

### Findings

- [x] **ACC1 — LOW — accuracy** — `plans/platform/PIPELINE_REDESIGN_PLAN.md:407`
  Claimed all four E2E rows started "in the same second at 18:19:21". Run `34387954665`'s job records
  show two rows at `18:19:21Z` and two at `18:19:22Z`. Fixed to "starting together at 18:19:21-22",
  which is what the evidence supports and still makes the point that the `build` edge is gone.

- [x] **ACC2 — LOW — accuracy** — `plans/platform/PIPELINE_REDESIGN_PLAN.md:411`
  Claimed `local-platform-pack` plus the B2B UI row "accounts for the whole 17m37". They sum to 17m12;
  the remaining 25s is the `changes` job and queue overhead, so the stated figure did not add up against
  the run it cites. Fixed to "is 17m12 of the 17m37".

Verified and clean: the 35m55s baseline matches N11's own run `34367029757`; 18:16:03Z→18:33:40Z is
17m37s and a 51.0% reduction; every per-job duration cited (pack 3m01, build 3m04, API B2B 6m07 /
Customer 5m03, UI Customer 8m47 / B2B 14m11) matches the run's job records; PR #973 merged as
`3095cb655`. No surviving document references the deleted `reviews/Perf-MergeQueueLatency.md`. The
amended gate wording records honestly that a `full-e2e`-labelled workflow PR, not an `api/` diff, proved
the restructure, rather than silently restating the original condition as met.
