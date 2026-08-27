# Code review — Plan/RepoSplit-Stage2-rt3-Swap

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `183b3c6b2080adcb1662a12ef32140706e2bea1e`  `(2026-08-27)`
**Security-reviewed up to commit:** `183b3c6b2080adcb1662a12ef32140706e2bea1e`  `(2026-08-27)`
**Judgment:** `approved`

## Review pass — 2026-08-27 — full

**Candidate base:** `085520405dc79e98b4e8bfcf982ec1225a36249a`
**Candidate head:** `183b3c6b2080adcb1662a12ef32140706e2bea1e`
**Candidate branch:** `Plan/RepoSplit-Stage2-rt3-Swap`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:ba5be54e6d82f447292c433fd9bd211d5676e32873582a2a1b3e27d2c6f80c9c` `(12 paths)`
**Candidate bundle:** `C:\Users\TommySeery\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable\0049c2db-306b-4fb1-952c-c9280ed7d04d\scratchpad\review-bundle-rt3`
**Candidate bundle identity:** `sha256:725d30eec1856364ed2bdb45c723842b25174ec0d6f33ba34c6cbedb3a9d6a14`
**Work-order path:** `reviews/Plan-RepoSplit-Stage2-rt3-Swap.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

Frozen tree object: `ef12d6a1cc174f6b43c7e766c96f6b81ca52373b`. 12 paths — 4 build/project files, 2 workflow
deletions, 1 generated inventory, 5 docs.

### Findings

None. The diff is a mechanical package-boundary swap plus deletion of machinery whose subject no longer
exists, and both directions of the swap were executed rather than reasoned about.

### Rules re-checked here, not trusted from write time

Routed mechanically over the frozen paths (`skill_router.py --skills-for`): `packages`,
`dotnet:unit-testing`, `dotnet-standards:unit-testing`, `dotnet:integration-testing`,
`dotnet-standards:integration-testing`. Each was re-opened at review time.

- [x] **`packages` — "a service's own test tier is not exempt."** This is the rule the change satisfies: the
  four `*.Hosting` refs were the last test-tier cross-repository `ProjectReference`s, and `split-inventory`
  now reports `crossTargetEdgesByKind.unit-test` absent (4 → 0).
- [x] **`packages` — shared `api/`-root build infra must be `Exists()`-guarded.** The new import of
  `PlatformSourcePackages.targets` from `api/Concertable.Shared/tests/Directory.Build.targets:47` carries the
  guard, so a carve that leaves the file behind degrades to package mode instead of failing. That guard *is*
  the cut-over.
- [x] **`packages` — the nested-`Directory.Build.targets` shadowing trap ("it fails silently").** Checked
  rather than assumed. `api/Directory.Build.targets` supplies exactly one thing — a guarded import of
  `TestConventions.targets` — and the nested `api/Concertable.Shared/tests/Directory.Build.targets` that
  shadows it already re-imports that same file. No shadowed enforcement, and this change adds to the existing
  nested file rather than introducing a new shadowing layer.
- [x] **`packages` — no repo-root or `api/`-root *version* config.** The pin went into
  `api/Concertable.Shared/Directory.Packages.props:5`, that folder's own CPM file, matching the five service
  folders. Nothing versioned moved upward.
- [x] **`packages` — `UseLocalCore` never in committed config.** Untouched.
- [x] **`PlatformSourcePackages.targets`' own stated invariant — "import AFTER a folder's UseLocalCore
  swap."** `api/Concertable.Shared/` has no `Directory.Build.targets` and no `UseLocalCore` swap, so there is
  no second mechanism for the new rows to double-fire against.
- [x] **`dotnet:unit-testing` — the build-time tier gate.** Name still ends `.UnitTests`; no banned host
  package (`Mvc.Testing`, `TestHost`, `Respawn`, `Testcontainers*`, `Playwright*`, `Reqnroll*`) is added, and
  `TestConventions.targets` is still reached through the guarded import.
- [x] **Reunion pins** — not touched by this diff.

### Blast radius of the folder-wide import — checked, not assumed

Adding the import to `api/Concertable.Shared/tests/Directory.Build.targets` makes the swap apply to **every**
project in that folder, including the platform sources that *produce* the swapped packages. A project there
declaring a `PackageReference` to a swapped id would receive a `ProjectReference` to itself.

- [x] Grepped all 11 `PlatformSourcePackage` ids across `api/Concertable.Shared/tests/`: the only project
  declaring any of them is the one changed here, so the transform is inert for the other ten.
- [x] Proved it rather than resting on the grep — built all 11 test projects in the folder: **11/11 OK**.

### Gates executed

| Gate | Result |
|---|---|
| In-repo build, swap-back path | 0 warnings, 0 errors; the four `*.Hosting` projects built from source |
| Suite | 7/7 passed |
| Carve build, feed path (targets file absent) | 0 errors; the four packages restored from the feed at `0.1.0-alpha.0.1221` |
| All `Concertable.Shared/tests` projects | 11/11 build |
| `inventory.py --check` | current |
| `plan_graph.py` | 0 errors, 0 warnings |

### Security layer

Qualifies under the merge gate's generic `^\.github/workflows/` pattern (two workflow deletions), so the
layer is required and was run. **0 findings.**

- **Both deleted workflows only ever added surface.** Each embedded `secrets.MIRROR_PAT` into a remote URL
  (`https://x-access-token:${MIRROR_PAT}@github.com/...`) and force-pushed cross-repo. Removing them removes
  that credential path; nothing replaces it.
- **Dependency confusion on the four new package ids is already closed.** `api/Concertable.Shared/nuget.config`
  carries a `packageSourceMapping` pinning `Concertable.*` to the private GitHub feed and `*` to nuget.org, so
  a public package published under one of these names cannot be substituted.
- **The pin is an exact version, not a floating range**, so no silent upgrade path is introduced.
- The remaining files are MSBuild item paths, generated inventory data and documentation; no untrusted input
  reaches any of them.

**Operational note — not a finding, not merge-blocking:** the `MIRROR_PAT` repository secret now has no
consumer. It is a PAT with write access to repositories that no longer exist. Worth revoking at the GitHub
level, but out of this PR's scope and not a vulnerability it introduces.
