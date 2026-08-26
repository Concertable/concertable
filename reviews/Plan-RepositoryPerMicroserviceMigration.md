# Review — Plan/RepositoryPerMicroserviceMigration

- Review status: **complete**
- Judgment: **approve**
- PR: [#798](https://github.com/Concertable/concertable/pull/798)
- Branch: `Plan/RepositoryPerMicroserviceMigration`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Plan\RepositoryPerMicroserviceMigration`
- Effort: high
- Reviewed up to commit: `<pending — restamped at the remediation head below>`

## Candidate

- Base: `3737df205093c0f6e5d1f7e6597e3b7eb48e9e12`
- Head at freeze: `bc1daf488fa72c26095b522f21c18746609f23d3`
- Scope: `all` (56 files; 16 commits — checkpoint 0 and stage 1, no prior review artifact existed)
- Mode: new

## Rules loaded

Routed from the frozen paths via `.agents/hooks/skill_router.py`: `packages`,
`dotnet:unit-testing`, `dotnet:integration-testing`, `dotnet-standards:unit-testing`,
`dotnet-standards:integration-testing`. Plus root `AGENTS.md` and `plans/AGENTS.md`.

Routing gap found and fixed — see F2.

## Findings

### F1 — `api/tests/Directory.Build.targets` shadowed the test-tier gate — **high** — FIXED

`api/tests/Directory.Build.targets:1` (new file)

MSBuild imports only the **nearest** `Directory.Build.targets` walking up from a project. Adding one at
`api/tests/` shadowed `api/Directory.Build.targets`, whose sole job is importing
`api/TestConventions.targets`. `Concertable.AppHost.ArchitectureTests` therefore silently lost the test-tier
gate — no `ValidateTestConventions`, no `BannedSymbols.UnitTests.txt` wiring for anything added under
`api/tests/` later.

Proved by evaluation rather than inference:

| State | `ConcertableTestTier` |
|---|---|
| without the new file (pre-change) | `Architecture` |
| with the new file, as first committed | `` (empty) |
| after the fix | `Architecture` |

**Fix applied:** the new file re-imports `../TestConventions.targets` alongside
`../PlatformSourcePackages.targets`, matching what the five service `Directory.Build.targets` already do, with
a comment naming the shadowing hazard.

### F2 — `api/`-root shared build infra was unrouted — **medium** — FIXED

`.agents/skill-routes.json`

The `packages` route matches only `Directory\.(Build|Packages)\.(props|targets)$`, so the new
`api/PlatformSourcePackages.targets` — and the pre-existing `api/TestConventions.targets` — routed to no
skill. An agent editing either would not be told to read `packages` first.

**Fix applied** at the source, not in the emitted file: a `^api/[A-Za-z.]+\.targets$` route added to
`agent-standards/.agents/gen_skill_routes.py`, then regenerated into this repo (+7 lines, 40 rows). The
emitted `skill-routes.json` states it is generator output and must not be hand-edited; an initial hand-edit
was reverted for that reason.

### F3 — `PACKAGES.md` forbade the placement this change uses — **medium** — FIXED

`agent-standards/standards/dotnet/PACKAGES.md`

The standard said there is "deliberately **no** repo-root or `api/`-root version **or build** config." That was
already inaccurate (`api/Directory.Build.targets`, `api/TestConventions.targets`, `api/BannedSymbols.txt` all
live at `api/` root) and this change adds a fourth. Left as written, the next reader would flag
`PlatformSourcePackages.targets` as a violation of a rule the repo does not actually keep.

**Fix applied** on `agent-standards` PR #60: the prohibition is narrowed to *version* config, and the
`Exists()`-guard condition that makes api-root build infra safe is stated, along with the nested-shadowing
trap from F1. Also corrected there: the doc's claim that integration harnesses are package-exempt.

### F4 — over-narrated comments — **low** — FIXED

`api/PlatformSourcePackages.targets`, `eng/repository-split/inventory.py`

Both carried rationale that belongs in the commit message (why the test tier is the tier that needs a swap,
what was invisible in the monorepo) rather than an invariant a reader needs at that line. Trimmed to the two
things a caller can actually break — import ordering versus the `UseLocalCore` swap, and "a carve must not
copy this file" — with the rationale left to `packages` and the commit.

## Checked and clean

- **The swap mechanism.** Item-graph equivalence confirmed with `dotnet msbuild -getItem` in default and
  `UseLocalPlatformPackages=true` modes for a fixture, an integration test, a module integration test, the
  fleet architecture test, and a **runtime** project — the last proving no deployable closure moved.
- **Runtime tier untouched.** `Payment.Infrastructure` keeps all eight platform packages in both modes; the
  swap is gated on the same `[\\/][Tt]ests[\\/]` test `EnforceServiceBoundary` uses.
- **No double-swap.** The `UseLocalCore` block removes the same ids first and is imported earlier, so
  `Messaging.Domain`/`.Infrastructure` cannot be swapped twice. Only B2B has both mechanisms and the
  `Messaging.*` overlap.
- **Unit tier inherits nothing banned.** `Concertable.Testing` depends only on `PdfPig`, `xunit`,
  `xunit.assert` — none in `BannedSymbols.UnitTests.txt`. `Testing.Integration` (which does carry
  Respawn/Testcontainers) reaches only fixtures and integration projects, never a `.UnitTests` project.
- **CPM completeness.** All seven ids have a `PackageVersion` in every folder that references them; the six
  folders resolve `$(ConcertablePlatformVersion)`, and `api/tests` gained the pin it lacked.
- **Pin automation still covers the new pin site.** `bump-platform-version.sh` discovers pins by grep over
  `api/**/Directory.Packages.props`, so `api/tests` is picked up with no workflow change.
- **Feed reality.** All seven packages confirmed present at the pinned `0.1.0-alpha.0.1195` before any edit.
- **`--check` semantics.** Excludes `*.Hosting` targets (stage 2) and E2E (stage 4), so it asserts exactly
  what stage 1 owns: `blockingTestEdges: 0`. `composition-test` is included forward-looking; no such edge
  exists today.
- **CI wiring.** `test.yml` parses (20 jobs); `split-inventory` is in `ci-complete.needs`; it gates on
  `run_code`, and `eng/` is not in the `INERT` set, so a generator change re-runs it. Carve jobs keep their
  feed credential, and B2B/Customer discover the new test projects by path rather than a hand list.
- **Stale comments.** The five carve-job comments claiming the test harness is excluded were rewritten; no
  `*/Tests/*` exclusion remains.

## Not verified here (owned by CI)

`dotnet build api/Concertable.slnx` and the unit/integration suites did not run locally — the workstation's
C: drive was exhausted. The PR's `build` job and scoped test matrices own that evidence. Note the `build` job
runs through `local-platform.ps1`, i.e. with `UseLocalPlatformPackages=true`, so it exercises the whole test
tier in **package** mode against freshly packed platform libraries — stronger coverage than before this change.
