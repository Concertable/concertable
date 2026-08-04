# Code review — Refactor/DerivedErrorDefinitions

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `4e3d8e3a185db87e6df7e80ea6f0e0963bec2602`  _(2026-08-04)_

> Range reviewed: `9dfb5e63d..4e3d8e3a1` (2 commits, the second being this review's fixes). Local `main` is 4 commits stale, so the range is
> taken from `origin/main`, not `git merge-base main HEAD`.
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CV1 — LOW — Lens E (test hygiene)** — `api/Concertable.Shared/tests/Concertable.Kernel.UnitTests/ErrorCaseFixtures.cs:71`
  The deliberately-invalid union fixtures sit at namespace scope, one of them named exactly `Error`, so
  every other file in `Concertable.Kernel.UnitTests` now resolves a bare `Error` to a test fixture.
  `PaymentError` also carries the malformed `Legacy_NotFound` beside its realistic cases, which is the
  one fixture readers will copy. Nest the invalid-shape unions in a container type; only
  `UnnestedNotFound` must stay top-level, since being top-level is what it tests.

- [x] **TEST1 — LOW — Lens F** — `api/Concertable.Shared/tests/Concertable.Kernel.UnitTests/ErrorCodeResolverTests.cs:54`
  `WithoutRepeatedContext` drops repeated leading case words in a loop, and both docs state the plural
  ("leading case words that repeat the union are dropped"), but every table row repeats at most one
  word, so the loop's second iteration is unasserted. Add a `EscrowRefundError.RefundEscrowNotFound`
  fixture and a row pinning it to `escrow.refund_not_found`.

Both fixed in `4e3d8e3a1`: the five underivable unions moved into an `UnderivableShapes` container
(`UnnestedNotFound` deliberately stays top-level), `Legacy_NotFound` moved out of `PaymentError` into
`UnderivableShapes.UnsplittableError`, and `EscrowRefundError.RefundEscrowNotFound` added with its
table row. Kernel tests 240/240.

Checked correctness, microservice isolation, module boundaries, seeding, C# conventions, and test
coverage of changed paths. Lenses B, C, and D have nothing to judge: the diff is Kernel + docs only,
with no service, seeder, module facade, or cross-service reference touched.
