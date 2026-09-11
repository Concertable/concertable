# Code review — Docs/PolyrepoExtractionRehearsals

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `7303236622918dc2ca0932cc7374a5362701c414`  `(2026-09-10)`
**Judgment:** `approved`

## Review pass — 2026-09-10 — docs

**Candidate base:** `754f62e57b1f53843f161b33eec31f7816cbee5b`
**Candidate head:** `7303236622918dc2ca0932cc7374a5362701c414`
**Candidate branch:** `Docs/PolyrepoExtractionRehearsals`
**Candidate scope:** `all`
**Candidate path-set:** `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` `(1 path)`
**Work-order path:** `reviews/Docs-PolyrepoExtractionRehearsals.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

Scope guard: one surviving path, the migration plan. Nothing runtime, package, migration or
CI-test-selection. `docs_reachability.py` reports 11 errors and 28 warnings on this head and the
identical counts on the base, so no regression; none names this file.

This branch is itself partly a correction to #993, which landed a result measured on `auth` as though
it generalised. Both corrections below were caught before the commit rather than after.

### Findings

- [x] **ACC1 — MEDIUM — accuracy** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
  Drafted that fetching the target's objects turns `payment`'s patch 7 into "a clean three-file
  merge". It does not: the fetch buys a real three-way merge, which then still conflicts modify/delete
  on `PaymentArchitectureTests.cs`. Two of the three files auto-merge. Reworded before commit.

- [x] **ACC2 — MEDIUM — accuracy** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
  Drafted that `auth`, `payment` and `search` "all conflict modify/delete" on the ArchitectureTests
  file. `auth` does not — the commit carrying that file is its skipped catch-up import, so the
  subset relationship is settled there by inspection, not by a conflict. Reworded before commit.

### Verified, no finding

Counted or read directly, not carried over from the previous session's summary: six of nine targets
declare `exclude` and so need two passes, with `auth`, `platform-frontend` and `org-github` the only
single-pass ones (enumerated from `map.yaml`); `payment` reconciles 8 patches with 1 conflict and
restores plus builds Release with 0 errors; `search` stops at patch 9 of 23 and exactly five later
patches (`0010`, `0014`, `0015`, `0019`, `0021`) touch the same AppHost/Hosting seam; the first
repo-only commit touches zero `src/` or `tests/` files in `payment`, `search` and `customer` and four
in `auth`; `search`'s target `AppHostExtensions.cs` declares six overloads including an
`AddSearchMigrations` pair against the extraction's four, and uses the `extension()` block the
extraction's copy does not; `payment`'s standalone-solution commit drops `E2ETests.Server` alongside
the two Helpers projects it should drop.
