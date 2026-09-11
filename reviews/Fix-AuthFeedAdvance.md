# Review — Fix/AuthFeedAdvance

**Reviewed up to commit:** `e9c0096e4`
**Security-reviewed up to commit:** `e9c0096e4`
**Review status:** `complete`
**Judgment:** `approved`

PR #1022 — stop the monorepo packing Auth so the publisher advances again.

## Pass 1

**Pass judgment:** `approved`
**Effort:** `low`
**Mode:** `new`

| Field | Value |
| --- | --- |
| Branch | `Fix/AuthFeedAdvance` |
| Base | `origin/main` at `9e77b42b4` |
| Head | `e9c0096e4` |
| Path count | 2 |

Frozen paths: `.github/scripts/package_ownership.py`,
`.github/workflows/tests/test_publish_packages_policy.py`.

**Security classification:** qualifying, via the merge gate's generic `^\.github/workflows/` pattern.
This is supply-chain policy code. The change narrows what the monorepo pushes — auth leaves the batch —
so it cannot cause an artifact to be published that was not published before. No new id, no wider scope.

## Findings — none

The defect and the fix were both established against evidence rather than reasoning:

- **The failure is real and reproduced from the run log**, not inferred:
  `Concertable.Auth.Contracts@0.1.0-alpha.0.1396 does not advance its feed history`, run 34645007960,
  step `Require a new lockstep package version`.
- **The mechanism is confirmed in the source.** `validate_batch` requires a single version across the
  whole batch (`package_publication_policy.py:78`) and requires each id to advance the feed's maximum
  published version (`:93`), comparing against `max(published)` with no train awareness. `0.1.x` cannot
  advance past the `0.2.0-alpha.0.285` that `Concertable/auth` published, and because the first offender
  raises, the whole step fails — so the monorepo was publishing nothing for any service, not just auth.
- **Two alternatives were tested and rejected on evidence, not preference.** Raising only Auth's floor
  fails `:78` (two versions in one batch) — this was written and reverted before pushing. Raising the
  whole repository to `0.2` works but would place every id above `0.2.0-alpha.0.1396`, which sits above
  every carve repository's own height (auth 285, search 311, payment 366), locking all four out of
  publishing without version tags. Deleting the two `0.2.0-alpha.0.285` versions was preferred and is
  unavailable: the token lacks `delete:packages`, and GitHub reports the versions in use.
- **The policy suite passes at 14 assertions** with auth promoted.

## The test fixture defect this also fixes

The two-publisher fixture rewrote one *spelling* of the `PROMOTED_TARGETS` declaration — first
`frozenset({"auth"})`, then `frozenset()` — and broke on each promotion. That is the same inert-gate shape
this suite has already been corrected for once: a guard that silently stops guarding when the thing it
watches changes. It now rewrites the whole declaration line via `re.subn` and asserts exactly one
substitution occurred, so a promotion cannot quietly void it. `b2b` is retained in every state, so the
rewritten module always conflicts and the guard is always exercised.

## Accepted consequence, recorded deliberately

Auth contract changes made in the monorepo no longer reach consumers: nothing publishes those ids from
here, and `Concertable/auth` is one extraction behind (its tree predates the typed identity model). The
in-flight Auth refactor stays in the monorepo and is carried over in the end-of-migration sweep, when
every service moves at once.

This is the second reversal on auth's promotion in one evening. The root cause is not the mechanism,
which works — it is that auth was promoted while its contracts were still being changed. Promotion is
now batched for all five services precisely so that window cannot exist again.
