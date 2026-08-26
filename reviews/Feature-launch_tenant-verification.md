# Code review — Feature/launch_tenant-verification

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `d2821f682`  _(2026-08-26)_
**Security-reviewed up to commit:** `d7f398ffc59f42ec1cc3dc633ac6500e430bdbd9`  _(2026-08-26)_

> Range reviewed: `421acb5b6..d2821f682` (4 commits, incl. one merge-base-in with no code content).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No findings. Checked: native review (correctness, reuse, simplification, efficiency, error handling —
clean over all commits); security review (`.Contracts` touched by the first commit — tenant-id sourcing,
`IsApprovedByTenantIdAsync`'s parameterized EF query, seed-fixture containment, guard-clause logic, and
confirmation the rename commit is pure identifier rename with no behavior change — all clean); Lens A
correctness; Lens B service isolation (n/a); Lens C module boundaries; Lens D data seeding; Lens E
conventions (the naming fix — `IsApprovedAsync` → `IsApprovedByTenantIdAsync` per `csharp-naming`'s
"state the key" rule — propagated through `VerificationRepository` and `VerificationService`, rebuilt
and re-tested green); Lens F test coverage (both new gate call sites have a covering success and failure
test).

## Incremental review — 2026-08-26

CI caught a real bug the static review passed: `SeedState.Verifications` giving every tax-compliant
tenant (including `VenueManager1`) an `Approved` row by default silently invalidated most of
`VerificationApiTests.cs` (Phase 2's pre-existing submission-lifecycle suite, which needs its tenant to
start with no verification row at all). 4 of 7 tests failed observably; the other 3 passed by
coincidence (they only assert response status, not verification state). Fixed by swapping every
`VenueManager1` reference in that file for the dedicated clean-slate `UnverifiedVenueManager` fixture
this branch already introduced. Re-reviewed (native layer): confirmed all 8 sites swapped, the fixture
genuinely has no verification row, `TenantOf(owner.Id)` still resolves correctly, and the staff-forbidden
test's intent is undisturbed (only the owner changed, not the staff user). No findings.
