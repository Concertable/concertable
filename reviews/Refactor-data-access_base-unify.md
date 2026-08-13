# Code review — Refactor/data-access_base-unify

> **This file is a work order, not a discussion.** Fix the open `[ ]` findings directly and report what
> changed. Tick each `[x]` as you land it.

**Reviewed up to commit:** `190674ea520feca593ab9327b051325c87202054`  _(2026-08-12)_
**Security-reviewed up to commit:** `190674ea520feca593ab9327b051325c87202054`  _(2026-08-12)_

> Range reviewed: `58e19d938..190674ea5` (1 commit — PR-B). Status legend: `[ ]` todo · `[x]` done · `[wontfix]`.

## Findings

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.

Layer 1 (native `code-reviewer`, medium effort) verified: (1) removing `AsNoTracking` from the shared `ReadRepository` changes no live behaviour — it's extended only by `Repository` (write repos over tracked contexts); Customer read repos moved to the interface-bound base in #526 and the B2B/Payment `ReadRepository<T>` aliases were dead; (2) `Repository`'s inherited reads (`context.Set<T>()`, tracked) match its prior tracked read path — consistent with `TenantScopedRepository`/`VenueArtistTenantScopedRepository` custom reads; (3) the former write-method duplication in `Repository` was forced by the rejected inheritance design, the methods are trivial, and it's logged in `TECH_DEBT.md`; (4) no caller invokes the removed write-facet `GetAllAsync` (`CollectionSyncer`/`OpportunitySyncer` use only writes), and adding `InsertAsync` breaks nothing — every module repo derives from the shared base classes (which supply it), and no hand-rolled write-facet or `IRepository` implementers or fakes exist.

Security layer (Payment path in range → gate-flagged): the only Payment change is deleting a dead `ReadRepository<T>` alias. The diff has no auth/authz, credential, data-exposure, injection, or serialization surface — it's a repository-base refactor. Security-clean.

> Environment note (not a code finding): the review agent hit a transient `C:` full-disk spike (0 bytes free) mid-run; `df` later showed ~30G free. The session has created several full worktrees — worth pruning the merged/stale ones.
