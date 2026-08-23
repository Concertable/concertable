# Code review — Chore/TryInsert

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `af0998f01110a6d328e4a26398c36f58891639c0`  _(2026-08-23)_

> Range reviewed: `b7d0fcb..81775ce` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No findings.

- **Layer 1 (native — correctness, reuse, simplification, efficiency, error handling):** clean. Confirmed
  a faithful mechanical port — same duplicate-key detection (`IsDuplicateKey`/`DiscardFailedChanges`),
  same return semantics, interfaces unchanged.
- **Lens A (correctness):** clean. No control-flow, await, or transaction changes — pure delegation.
- **Lens B (service isolation):** not applicable — no path outside `Concertable.Customer`.
- **Lens C (module boundaries):** not applicable — change is internal to the Preference and Review
  modules' own `Infrastructure` projects; no cross-module reach.
- **Lens D (data seeding):** not applicable — no seeder touched.
- **Lens E (conventions — csharp-style, csharp-naming, dotnet:persistence, dependency-injection,
  multitenancy, docs-and-debt):** clean. `WriteRepository<TEntity>` alias matches the existing
  `Repository<TEntity>` module-alias pattern (primary-constructor base-forward, `internal abstract`);
  chosen over the full `Repository<TEntity, TKey>` alias because `ConcertReviewRepository` only ever
  needs the write capability (`InsertAsync`) — its reads are all custom no-tracking projections, never
  the base's `GetAll`/`GetById` shape — matching persistence's context-capability-triple rule directly.
  `context` field naming, `this.`-qualified extension calls, and the deleted `TECH_DEBT.md` entry (full
  deletion, not left as an archive) all match their owning standards. Customer has no tenant filtering to
  violate (dotnet:multitenancy: Customer's data is user-scoped, not tenant-scoped).
- **Lens F (test coverage):** clean — the new wiring (repository → shared `TryInsertAsync` → real
  `DbUpdateException` on a unique-constraint hit) is exercised by pre-existing, still-green integration
  tests that post a duplicate directly against it:
  `PreferenceApiTests.Create_ExistingPreference_Returns409` and
  `ReviewApiTests.CreateConcertReview_ShouldReturn409_WhenTicketAlreadyReviewed`. Not new tests, but the
  refactor didn't leave the new path unpinned.

## Incremental review — 2026-08-23

> Range reviewed: `81775ce..af0998f` (6 commits, currency merges only).

The delta since the last watermark is entirely two `origin/main` currency merges — no new commits on
this branch's own content. The only non-merge-commit changes are this review file's own prior commit and
files from an already-merged, already-reviewed upstream PR (#764, `Docs/docs_polyrepo-ready-n8-carve-evidence`):
`plans/docs/DOCS_ROADMAP.md`, `plans/docs/POLYREPO_READY_PLAN.md`, `plans/docs/POLYREPO_READY_PROGRESS.md`,
`reviews/Docs-docs_polyrepo-ready-n8-carve-evidence.md`. None of it touches this branch's own diff
(`api/Concertable.Customer`, `api/Concertable.DataAccess`) — no new findings.
