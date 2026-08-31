# Code review — Refactor/data-access-specification-query-boundary

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]` findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `f0eae9ddd408`  `(2026-08-31)`
**Judgment:** `approved`

## Review pass — 2026-08-30 — full

**Candidate base:** `1309f4a098d020d1b4e3edb6951cc18870516cc5`
**Candidate head:** `5394617a0be92bfd0f9b9866068df0fc9108bbde`
**Candidate branch:** `Refactor/data-access-specification-query-boundary`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:5cbb36c5f074077702d83269b5e466b42371e5b2ba5393b7292eecaf9ad6e6fb` `(22 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\concertable-review-5394617a0be9-bd6c50b9da054320b467339c15ee9a84`
**Candidate bundle identity:** `sha256:03ccfa2fe1bd49e65412b1e5acf3ba911f32f10722981caa7fca2ed793534190`
**Work-order path:** `reviews/Refactor-data-access-specification-query-boundary.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

### Findings

No findings.

## Review pass — 2026-08-31 — incremental

**Candidate base:** `5394617a0be92bfd0f9b9866068df0fc9108bbde`
**Candidate head:** `f0eae9ddd408`
**Candidate branch:** `Refactor/data-access-specification-query-boundary`
**Candidate scope:** `branch-authored delta vs origin/main`
**Candidate path-set:** `sha256:8f6259b620d07765` `(79 paths)`
**Work-order path:** `reviews/Refactor-data-access-specification-query-boundary.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

**Docs in scope:** the pass also covers the plan reconciliation in `f0eae9ddd` — the rejected `With...`
vocabulary section replaced with the delivered `Include`/`Select` form, and Phase 3 rewritten from
future-tense to what shipped, including what is still owed (no architecture rules, no PR/CI/review on the
adoption branch). `plan_graph.py` validates 0 errors, 0 warnings.

**Reviewer independence:** this pass was performed by the same session that authored the change, because
subagent review lenses were unavailable. It is weaker than the isolated multi-lens pass `review` normally
runs, and a genuinely independent pass over the specification contract would still be worth having.

### Findings

- [x] **1 — `Select` could not project a nullable column at all.** `Nullable<T>` satisfies neither the
  `class` nor the `struct` constraint, so `.Select(entity => entity.CancelledAt)` on a `DateTime?` column
  matched no overload and failed to compile with CS0452. Fixed by adding a third overload taking an
  already-nullable selector and passing it through unchanged; the three shapes (reference, non-nullable
  value, nullable value) now each bind exactly one overload. Regression test
  `GetByIdAsync_NullableValueProjection_ProjectsTheColumn`.

- [x] **2 — A specification instance is mutable, so sharing one is unsafe.** `Specification<TEntity>.Include`
  appends to the instance's own list, so a `static readonly` or DI-registered `SpecificationBuilder` would
  accumulate includes across every caller. Inherent to the builder shape rather than a defect in it, and no
  current code shares an instance. Disposition: **no change needed** — a named reusable projection must be an
  expression-bodied member (`=> new XSpecification().Select(...)`), never a field initializer. Revisit only
  if instance sharing appears.

### Notes

- The compile error for a non-nullable value projection (`.Select(x => x.DealId)` without the lifting
  overload) is correct but its diagnostic is poor — the compiler falls through to an unrelated candidate.
  An `[Obsolete(error: true)]` decoy cannot be added because it would differ from the reference overload
  only by constraint, which C# rejects.
- `GetAllAsync<TResult>` over a value projection yields `IEnumerable<TResult?>`. There is no missing-row
  ambiguity for a collection, so the nullability is noise there; nothing projects a value type through a
  collection today.
