# Code review — Refactor/data-access-specification-query-boundary

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]` findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `9ab7736cc45b`  `(2026-09-02)`
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

## Review pass — 2026-09-01 — incremental

**Candidate base:** `f0eae9ddd408c86072d40a171f9c040bc53a7413`
**Candidate head:** `76d498e5f5c17465d8684eb70caa520053658655`
**Candidate branch:** `Refactor/data-access-specification-query-boundary`
**Candidate scope:** `base merge only — no branch-authored change in range`
**Candidate path-set:** `sha256:ca56178ed1248a37e7b2f36c72ffee1079c0cdbaae0d1bf7a90d8bf6711e528e` `(46 paths)`
**Work-order path:** `reviews/Refactor-data-access-specification-query-boundary.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

The range is one merge of `origin/main` (46 commits) taken to clear a 46-commit base lag and the stale
`ConcertablePlatformVersion` pin (`0.1.0-alpha.0.1281` -> `0.1.0-alpha.0.1296`, sync #911). The merge was
textually clean with no conflict resolution, so the range contains no authored content and the branch's
own diff against base is byte-identical to the previously approved pass.

The interaction risk a base merge carries is semantic, not textual, so the pass checked it directly: the
46 base-changed paths and the 80 branch-authored paths intersect only at this work-order file. Nothing
that arrived from base touches the Kernel specification contracts, the DataAccess evaluator, or the
Search/B2B/Customer consumers this branch rewrites. `b5217a167` was the one base commit naming Search;
it changes `CommaDelimitedIntArrayModelBinder` and E2E page objects, not the reclassified Search
specifications.

Verification on the merged head: `local-platform.ps1 prepare` (55 packages) then
`local-platform.ps1 build api/Concertable.slnx` -> 0 errors, and the full PR matrix at `76d498e5f` is
72 pass / 5 skipping / 0 fail, including `local-platform-pack`, every carve gate, and the complete
unit, integration and architecture suites.

**Reviewer independence:** this pass was performed by the session that authored the merge. For a
base-merge-only range with no authored content and no path overlap it is proportionate, and it does not
revisit the specification contract itself, whose independence caveat from the 2026-08-31 pass stands.

### Findings

No findings.

## Review pass — 2026-09-02 — incremental

**Candidate base:** `76d498e5f5c17465d8684eb70caa520053658655`
**Candidate head:** `9ab7736cc45b7f016363d818af3311f843bad605`
**Candidate branch:** `Refactor/data-access-specification-query-boundary`
**Candidate scope:** `branch-authored delta vs the prior watermark`
**Candidate path-set:** `sha256:b33853138682b396d1afc886419d64636ce547d88f023b44e1b5701836ba10d3` `(9 paths)`
**Work-order path:** `reviews/Refactor-data-access-specification-query-boundary.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

Covers the repository read-surface rework: the runtime-detecting `ApplyOrders(ISpecification)`
overload, the collapse of the ordered/unordered overload twins, `GetPageAsync`, the
`IEnumerable` -> `IReadOnlyList` return change, and the `CancellationToken` on
`ToPaginationAsync`. Applied to all three `IReadRepository` implementations (`Repository`,
`ReadRepository`, Customer's `QueryableReadRepository`).

Verification: `local-platform.ps1 build api/Concertable.slnx` 0 errors; Kernel 260 passed,
DataAccess unit 22 passed, DataAccess integration 19 passed.

### Findings

- [x] **3 — Ordering bound by static type, so a spec held at its shape interface silently lost
  its order.** `GetAllAsync` carried an `ISpecification` and an `IOrderedSpecification` overload
  per shape. Because overload resolution is static, `ISpecification<T, TResult> spec = ...Select(...)`
  bound the non-ordering overload and the query ran unordered with no diagnostic. Fixed by
  detecting `IOrderedSpecification<T>` at runtime inside a single `ApplyOrders(ISpecification<T>)`
  evaluator overload and deleting the twins. Regression tests
  `GetAllAsync_OrderedSpecificationHeldAsShapeSpecification_StillAppliesOrders` and
  `GetAllAsync_OrderedProjectionHeldAsShapeSpecification_StillAppliesOrders` hold the spec at the
  shape interface exactly as the trap required.

- [x] **4 — `GetPageAsync` accepted a specification carrying no order.** A page taken without
  `ORDER BY` is undefined: row order is unspecified between the `Count` and the `Skip`/`Take`, so a
  row can appear on two pages or on none. Fixed by `ApplyPagedOrders`, which throws when the
  specification carries no order. This matches the precedent already set by a projected
  specification rejecting include registration rather than silently issuing a wrong query. Test
  `GetPageAsync_UnorderedSpecification_Throws`.

### Notes

- The `GetByIdAsync` class/struct/nullable triple is unchanged; collapsing it would cost
  nullability precision on value projections, which pass 1 deliberately bought.
- The read surface still takes no `IPredicateSpecification`, so a filtered read remains a bespoke
  repository method. That gap is additive rather than breaking, so it does not need this window.

**Reviewer independence:** this pass was performed by the session that authored the change.
Findings 3 and 4 were both raised and fixed within it, so the pass is not a substitute for an
independent read of the specification contract, whose caveat from 2026-08-31 still stands.
