# Review — Docs/AuthCarveReconciliation

**Reviewed up to commit:** `7a71c9eaf`
**Review status:** `complete`
**Judgment:** `approved`

PR #1000 — record what landing auth's carve added past the rehearsal.

## Pass 1

**Pass judgment:** `approved`
**Effort:** `low`
**Mode:** `new`

Docs-only: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`. No path matches the generic or
repository `security_paths` inventories, so no security classification and no security marker.

## Findings — none

Every load-bearing claim is corroborated by work done independently since this branch was written, which
is the strongest available evidence for a findings record:

- **`Exists()`-guarded imports fail silently.** The record states that `TestConventions.targets` and
  `BannedSymbols*.txt` are reached through `Exists`-guarded imports that no-op when unresolved, so the
  test-tier gate and banned-API list were simply off in the target with nothing said. Confirmed twice
  since: search's re-cut findings reached the same conclusion for `customer` and `search`, and
  `customer#5` has now dropped those guards so a missing file is a hard build error. This is the defect
  class the whole migration keeps producing — a construct chosen for tolerance where the thing it
  tolerates is mandatory.
- **`merge -s ours` for landing a re-cut.** Recorded here as necessary because a rewritten extraction
  conflicts on every touched file, GitHub marks the PR `DIRTY`, refuses to build a merge ref, and
  `pull_request` CI therefore never fires — which reads as disabled Actions rather than as a conflict.
  That diagnosis matches the unexplained zero-workflow-runs symptom seen on b2b, and the technique was
  used by `auth#6` and reproduced by the payment and customer re-cuts.
- **A green `dotnet build` is not a green carve.** `.dockerignore` is read only by `docker build`, and
  `PackageReadmeFile` only by `pack`, so both defects sit off the build's path. The generalisation —
  every reference to a shared file written relative to the project that moved — is the same root as the
  `BaseOutputPath` and `BannedSymbols` findings recorded elsewhere in this document.
- **The `.editorconfig` replication claim is specific and checkable**: 246 warnings to 2 on replicating
  it alone, at 0 errors either way, and the correction that the rehearsal misattributed those 246 to
  `MSB3277` when they were analyzer noise.

The one deliberate exception it names — `PlatformSourcePackages.targets` must *not* be replicated,
because its absence is the cut-over to feed packages — matches the live state: customer left both
`PlatformSourcePackages` imports guarded and dead on purpose.

## Merge resolution recorded

`origin/main` was merged in at `7a71c9eaf` to clear a `DIRTY` state. One conflict, purely additive on
both sides: this branch's `auth` section against main's `customer` rehearsal and `search` re-cut
sections. Both kept, no content dropped, all three sections verified present afterwards.
