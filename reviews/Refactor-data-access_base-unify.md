# Code review — Refactor/data-access_base-unify

> **This file is a work order, not a discussion.** Fix the open `[ ]` findings directly and report what
> changed. Tick each `[x]` as you land it.

**Reviewed up to commit:** `be808fb378080ccd2799d1f4cee7163d0d00c8a0`  _(2026-08-13)_
**Security-reviewed up to commit:** `be808fb378080ccd2799d1f4cee7163d0d00c8a0`  _(2026-08-13)_

> Range reviewed: `58e19d938..190674ea5` (1 commit — PR-B). Status legend: `[ ]` todo · `[x]` done · `[wontfix]`.

## Findings

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.

Layer 1 (native `code-reviewer`, medium effort) verified: (1) removing `AsNoTracking` from the shared `ReadRepository` changes no live behaviour — it's extended only by `Repository` (write repos over tracked contexts); Customer read repos moved to the interface-bound base in #526 and the B2B/Payment `ReadRepository<T>` aliases were dead; (2) `Repository`'s inherited reads (`context.Set<T>()`, tracked) match its prior tracked read path — consistent with `TenantScopedRepository`/`VenueArtistTenantScopedRepository` custom reads; (3) the former write-method duplication in `Repository` was forced by the rejected inheritance design, the methods are trivial, and it's logged in `TECH_DEBT.md`; (4) no caller invokes the removed write-facet `GetAllAsync` (`CollectionSyncer`/`OpportunitySyncer` use only writes), and adding `InsertAsync` breaks nothing — every module repo derives from the shared base classes (which supply it), and no hand-rolled write-facet or `IRepository` implementers or fakes exist.

Security layer (Payment path in range → gate-flagged): the only Payment change is deleting a dead `ReadRepository<T>` alias. The diff has no auth/authz, credential, data-exposure, injection, or serialization surface — it's a repository-base refactor. Security-clean.

> Environment note (not a code finding): the review agent hit a transient `C:` full-disk spike (0 bytes free) mid-run; `df` later showed ~30G free. The session has created several full worktrees — worth pruning the merged/stale ones.

## Incremental review — 2026-08-13

- [x] **NAT1 — LOW — native** — `scripts/unit.ps1:128`
  The new DataAccess unit-test group is included in `run` but omitted from `list` and from the `run` help text, so the command no longer reports the complete inventory it executes. Add a DataAccess section and include it in the help summary.

> Range reviewed: `2dfe09cc9..6914b9baf` (20 commits). NAT1 was fixed in `6914b9baf`; no open findings remain.

No additional issues found. Checked native correctness, security-sensitive workflow/package handling, microservice isolation, module boundaries, seeding, C# conventions, and test coverage of changed paths.

## Incremental review — 2026-08-13 (current-main sync)

No issues found. Reviewed `6914b9baf..f133bbefb`: the only upstream delta is the five service platform pins moving to `0.1.0-alpha.0.978`, plus the plan checkpoint carried by the merge.

## Incremental review — 2026-08-13 (carve argument forwarding)

No issues found. Reviewed `f133bbefb..be808fb37` (4 commits). The native pass verified that manual positional parsing preserves every downstream `dotnet` token while avoiding PowerShell option binding, and the focused local-platform build exercised the failing `-p:` shape successfully. The security pass found no credential, secret, injection, or untrusted-input change.
