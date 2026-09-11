# Review — Chore/ResumeAuthPublishing

**Reviewed up to commit:** `be8d19663`
**Security-reviewed up to commit:** `be8d19663`
**Review status:** `complete`
**Judgment:** `approved`

PR #1020 — publish Auth from the monorepo again until its refactor settles.

## Pass 1

**Pass judgment:** `approved`
**Effort:** `low`
**Mode:** `new`

| Field | Value |
| --- | --- |
| Branch | `Chore/ResumeAuthPublishing` |
| Base | `origin/main` at `9370ade8a` |
| Head | `be8d19663` |
| Scope | all |
| Path count | 2 |

Frozen paths: `.github/scripts/package_ownership.py`,
`.github/workflows/tests/test_publish_packages_policy.py`.

**Security classification:** qualifying, via the merge gate's generic `^\.github/workflows/` pattern. This
is supply-chain policy code — it decides which packed artifacts the monorepo pushes to the org feed — so
the security question is whether the change lets the monorepo push something it does not own. Checks 1, 4
and 6 below answer that directly and were verified against the real inventory rather than the constants.

## Findings — none

An independent native/general layer checked six specific questions and the parent verified the two that
carry the risk:

- **The push set is correct.** `KNOWN_TARGETS` is the union of both sets, so moving `auth` between them
  leaves it unchanged. Every distinct `target` in `eng/repository-split/inventory.json` — `auth`, `b2b`,
  `customer`, `payment`, `platform-dotnet`, `search`, `system` — still resolves, so `load_ownership`
  cannot raise on a legitimate inventory. `platform-frontend` appears only on npm entries, which
  `load_ownership` never iterates.
- **The guard is dormant, not dead, and not vacuous.** With `PROMOTED_TARGETS` empty,
  `require_single_publisher` cannot fire at this commit — that is the point of it, since it exists to
  catch a future promotion that adds a target without removing it from `RETAINED_TARGETS`. The test still
  exercises it by re-executing the real module source with `PROMOTED_TARGETS = frozenset({"b2b"})`, and
  `b2b` is already retained, so the mutated module raises. Deleting the call site at
  `package_ownership.py:26` still fails the suite. The fixture literal `PROMOTED_TARGETS = frozenset()`
  appears exactly once in the source, so the substitution cannot silently stop matching.
- **The foreign-artifact check moved to a genuinely foreign target.** `platform-dotnet` is not in
  `RETAINED_TARGETS`, so `validate_platform_dependencies` still raises "not published by this repository"
  for `Concertable.DataAccess.Infrastructure`. Using an auth package for that case would now be wrong,
  since auth is retained again.
- **The batch expectations were walked, not trusted.** Against the synthetic four-project inventory,
  `retained` is `["Concertable.Auth.Contracts", "Concertable.B2B.Contracts"]` and `removed` is
  `["Concertable.DataAccess.Infrastructure", "Concertable.Testing.E2E"]`, matching the assertions.
- **No tautological assertions survive.** The suite had exactly that defect before and it was fixed; the
  fix holds. The membership check still depends on the real source rather than passing unconditionally.

Policy suite green at 14 assertions.

## Why this is a revert rather than a fix forward

Recorded because the next reader will otherwise re-derive it. `auth#6` was extracted on 09-10, before the
typed identity model landed in `c15da3f03` and a day before `#1008` removed `ApiScopeIds`/`ClientIds`/
`AuthParty` and added `InteractiveClient`. `Concertable/auth` therefore carries the pre-typed-model shape,
and building `Concertable.Auth` against the canonical `0.2.0-alpha.0.285` fails with five `CS0246`s for
exactly those types. Syncing the delta by hand was rejected: the monorepo also regenerated auth's initial
migration on 09-10 (`20260910184438_InitialCreate` against auth's `20260724223954_InitialCreate`), and the
published auth image was built against the older one, so a content sync would have carried a migration
change with it.

Auth is promoted last instead, from a quiet tree, once the in-flight corrective refactor of that same
model settles. `0.2.0-alpha.0.285` stays published and stale; it sorts above the `0.1.x` line, which is
inert because every consumer pins explicitly and nothing resolves a floating version.
