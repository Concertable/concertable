# Review — Chore/StopPublishingAuth

**Reviewed up to commit:** `2dc34b53b6a7a1a230375a5b51f994e9fedec8f2`
**Security-reviewed up to commit:** `2dc34b53b6a7a1a230375a5b51f994e9fedec8f2`
**Review status:** complete
**Judgment:** approved

PR #1016 — stop the monorepo publishing Auth's package ids.

## Pass 1 — frozen at `35184b1c6`

**Pass judgment:** changes requested (all findings since remediated in pass 2)
**Effort:** low
**Mode:** new

| Field | Value |
| --- | --- |
| Branch | `Chore/StopPublishingAuth` |
| Base | `71157873dde762232ac4bbde679f49d44595d812` |
| Head | `35184b1c6e04438d929a6ec838ff24d22e6178d5` |
| Scope | all |
| Path count | 2 |
| Path set SHA-256 | `14330cc9225d908d054fcfa470fcdf11528fca6a24eaf58088938f94fb61c8cd` |
| Bundle SHA-256 | `1efb5b68e218b3554df1ce843d4ccbd7dd88ee41ea04dd01af1d121058557322` |

Frozen paths: `.github/scripts/package_ownership.py`,
`.github/workflows/tests/test_publish_packages_policy.py`.

**Routed skills:** none — `skill_router.py --skills-for` over both frozen paths returns
`no routed paths among the 2 given - no skill is owed`. That `.github/scripts/**` carries no route is a
route-table observation, not a defect in this candidate.

**Security classification:** qualifying, via the merge gate's generic `^\.github/workflows/` pattern. The
repository `security_paths` match neither frozen path.

**Layers run:** native/general; lenses `publication-policy-correctness`, `changed-behaviour-test-impact`,
`security`.

### Production code — no findings

Three independent layers agreed, and the parent verified each claim:

- The withheld set is exactly `Concertable.Auth.Contracts` and `Concertable.Auth.Hosting`. Enumerated from
  the frozen `eng/repository-split/inventory.json`: ten `target: auth` entries, of which only those two are
  `packable`. No id the monorepo must still publish is starved.
- `list retained` and `filter_batch` both resolve through the same `packages_for(ownership, "retained")`
  call, so the push set and the unlink set cannot disagree.
- Workflow order is `filter` (`publish-packages.yml:53-57`) before `validate-platform` (`:59-67`), and that
  order is itself pinned by `test_publish_packages_policy.py:142-146`. Withheld artifacts are unlinked from
  disk before validation globs the directory, so `validate_platform_dependencies`' foreign-artifact raise
  cannot fire on a legitimate run — defensive redundancy, not dead-code-by-mistake.
- B2B and Customer consume `Concertable.Auth.Contracts` as a `PackageReference` pinned through a dedicated
  `$(ConcertableAuthVersion)`, already decoupled from their own lockstep versions before this diff. Restore
  resolves by id and version regardless of publisher, so those restores keep working.

### Findings — all in this candidate's own new test code

- [x] **F1 (medium) — the intersection assertion could not fail.**
  `test_publish_packages_policy.py:323-326` read `RETAINED_TARGETS` and `PROMOTED_TARGETS` directly.
  Deleting the import-time guard at `package_ownership.py:20-23` outright left the suite green, because the
  shipped constants never intersect. Verified by grep: `PROMOTED_TARGETS` appeared exactly once in the whole
  test file, at that assertion. Fix: guard extracted to `require_single_publisher()`, still invoked at
  import; the test re-executes the real module source with an intersecting `PROMOTED_TARGETS` and asserts
  the module refuses to load, plus asserts the substitution matched so a reformat fails loudly.
- [x] **F2 (medium) — the withholding side effect was unasserted.**
  `filter_batch` unlinks withheld artifacts (`package_ownership.py:94`) but only the returned lists were
  checked. A regression computing `removed` correctly while no longer deleting would stay green and leave
  artifacts on disk for a later glob to push — the two-publisher collision this branch exists to prevent.
  Fix: the withheld batch is asserted absent from `package_dir`.
- [x] **F3 (low) — the foreign-artifact raise was unreachable in the suite.**
  By the time `validate_platform_dependencies` ran, `filter_batch` had unlinked the promoted artifact, so
  the new raise at `package_ownership.py:108-109` was never exercised. Correct in production (see above);
  the gap was coverage only. Fix: exercised against a directory holding only the promoted artifact.
- [x] **F4 (low) — the loadability assertion read constants.**
  `test_publish_packages_policy.py:319-322` asserted over `KNOWN_TARGETS`. Fix: asserts the observable
  result, that `load_ownership` maps `Concertable.Auth.Contracts` to `auth`.

## Pass 2 — incremental, frozen at `2dc34b53b`

**Pass judgment:** approved
**Mode:** append
**Range:** `35184b1c6..2dc34b53b` — same two paths, no widening.

All four findings resolved. Remediation verified by **mutation**, not by the suite passing, since the defect
under repair was assertions that could not fail:

| Mutation | Result |
| --- | --- |
| delete the `require_single_publisher(...)` call site | caught |
| replace `discovered[...].unlink()` with `pass` | caught |
| drop the foreign-artifact `raise` | caught |
| re-add `auth` to `RETAINED_TARGETS` | caught (exit 1 from the import-time guard) |

The last one exits non-zero with `ValueError: Target published from two repositories: ['auth']` rather than
printing `FAIL`, so a `FAIL`-string check alone is not sufficient evidence that this suite caught a
regression — check the exit code.

Suite green at 12 assertions in the promoted-target case; full file passes.

## Operational condition carried out of this review

After this merges, `ConcertableAuthVersion` **cannot be bumped in the monorepo** until the auth repository
publishes that version, because the monorepo no longer pushes those ids. Today's pin
(`0.1.0-alpha.0.1383`) is already on the feed, so every current consumer restores unchanged; the constraint
binds only on the next bump. That is the `10C` gate, and it is the reason this PR is ordered before
Auth's canonical publish rather than after it.

Not verifiable from this candidate, and deliberately not asserted: that the auth repository's own pipeline
publishes both ids. The frozen tree shows `map.yaml:22-32` moving both projects' source to `Concertable/auth`
and `inventory.py:33-34` agreeing, which is corroborating but not proof.
