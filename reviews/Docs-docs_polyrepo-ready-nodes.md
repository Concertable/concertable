# Docs review — Docs/docs_polyrepo-ready-nodes

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `2053862d1274a8b690dba60827f2c50b1c1f4955`  _(2026-08-20)_

> Range reviewed: `133b018da..2053862d1` (4 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

Run from the moved copy of the procedure — `standards/process/review/DOCS.md` on `agent-standards`
`Docs/polyrepo-ready-review-family` — because this branch deletes the in-repo `docs-review` skill and its
producer PR has not merged yet. That is the N1 family-1 gate ("`docs-review` still runs end-to-end from the
moved copy") discharged on this slice's own diff.

## Findings

- [x] **ACC1 — MEDIUM — accuracy vs reality** — `plans/docs/DOCS_ROADMAP.md:40`
  The `docs/polyrepo-ready` row still says "N1 the 28 skills (3,285 lines)" after this slice took six of
  them out. `POLYREPO_READY_PLAN.md` now records "22 skills left of 28, 2,472 of 3,285 lines" — a reader on
  the roadmap would believe none of N1 has landed. The roadmap is the durable tracker, so its measurement
  has to move with the plan's.

- [x] **HOME1 — MEDIUM — right home** — `docs/INDEX.md:43`
  The row "Review files as work orders; addressing and deleting findings | skills `address-review`,
  `review-lifecycle`" names a consumer beside the owner. `docs/INDEX.md` opens with "Every rule has **one**
  owning doc"; `review-lifecycle` owns the work-order framing and the delete-when-spent rule, and
  `address-review` is the procedure that obeys it. Listing both makes the index answer "who owns this rule?"
  with two names.

- [x] **INST1 — LOW — followable instruction** — `plans/docs/POLYREPO_READY_PROGRESS.md:43`
  Next Step 2 gates the `^reviews/.*\.md$` route row on "once the plugin refresh has happened on every
  machine" — a condition with no way to check it, which is exactly the gate-with-no-pass-condition defect.
  The step is right and the trap it avoids is real; it needs an objective test instead, e.g. the row lands
  only after `review-lifecycle` resolves from a fresh install on this machine and Tommy confirms the other.

No findings for cross-doc contradiction, harness-reloaded concision, or dangling references. Checked: no
surviving reference to `reviews/AGENTS.md` outside spent ledgers and review files (left deliberately, as the
Phase 1 review established); `TECH_DEBT.md:40`'s citation of `docs-review`'s in-scope path list still
matches the moved `DOCS.md`; every prose citation of `/review`, `/docs-review`, `/big-review` and
`/incremental-review` in `AGENTS.md`, `plans/AGENTS.md`, `merge` and `merge-docs` still resolves, because the
skill names did not change.
