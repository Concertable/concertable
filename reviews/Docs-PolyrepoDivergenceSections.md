# Code review — Docs/PolyrepoDivergenceSections

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `67d51e3fe6d99d4ae3466981ec7e3e7a6a654c17`  `(2026-09-10)`
**Judgment:** `approved`

## Review pass — 2026-09-10 — docs

**Candidate base:** `aa64924852582e62c2601ed853903562fc0886e8`
**Candidate head:** `67d51e3fe6d99d4ae3466981ec7e3e7a6a654c17`
**Candidate branch:** `Docs/PolyrepoDivergenceSections`
**Candidate scope:** `all`
**Candidate path-set:** `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md` `(1 path)`
**Work-order path:** `reviews/Docs-PolyrepoDivergenceSections.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

Scope guard: one surviving path (`git diff --diff-filter=ACMRT`) — the migration plan — so this is an
ordinary docs review, not an exempt pure close-out. Nothing runtime, package, migration or
CI-test-selection is in the candidate. Lenses run as the strong parent; subordinate dispatch is disabled
by session policy.

`python .agents/hooks/docs_reachability.py --root .` reports 11 errors and 28 warnings on this head and
the identical 11 and 28 on the candidate base, so the change introduces no reachability regression. None
of them names this file.

### Findings

- [x] **ACC1 — MEDIUM — accuracy** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
  The rehearsal claimed the 12 replayed patches produced "two conflicts". The `git am` run produced one,
  in `Directory.Packages.props` on `chore(packages): prepare Auth release train`; the other four conflicts
  belonged to the skipped catch-up import. Corrected to "a single conflict" in `67d51e3`.

- [x] **ACC2 — MEDIUM — accuracy** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
  The Contracts-reference count was given as "nine references across four files ... five `Dockerfile`
  `COPY` lines". The applied fix diff is nine changed lines across five files, of which eight across four
  are Contracts-location references — `.slnx` 1, `ProjectReference` 2, `Dockerfile` 4, verify script 1;
  the ninth line is the unrelated `ArchitectureTests` to `StartupTests` rename. Corrected in `67d51e3`.

- [x] **CON1 — MEDIUM — contradiction** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
  Checkpoints 7C and 8C instructed "update package links and Actions access", which the rewritten
  package-ownership section shows is impossible: bindings cannot be re-pointed and no Actions-access panel
  exists for a NuGet or npm package. Both now name the `CONCERTABLE_PACKAGES_TOKEN` swap that is the real
  remaining work, with 8C noting the changesets version PR keeps the repository token. Fixed in `67d51e3`.

- [x] **ACC3 — LOW — accuracy** — `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
  "the rename cost nothing" overstated `git am --3way`'s rename detection, which prevented a duplicate
  Contracts tree but left every in-file reference to the old location broken. Narrowed to "no duplicate
  tree appeared" in `67d51e3`, with the reference breakage carried by the bullet that follows.

### Verified, no finding

Every cited artifact was checked against the repository rather than accepted from the source handoff:
`fee52b4cf` is "refactor(tests): split the startup tier out of the architecture suites" and does rename
Auth's `ArchitectureTests` to `StartupTests`; `9c20128` and `198ca1e` are the boundary and first
repository-only commit in `Concertable/auth`; the 6635-to-815 commit reduction, the 13 non-merge patches
from 18 commits, and the green Release build at `0.1.0-alpha.0.1370` are this session's own run;
`ApiScopeIds`, `AuthConstants.ContainerPort` and `WithSpaClients` are present in the extraction and absent
from the target, read from the files; `platform-dotnet/.github/workflows/publish.yml`,
`platform-frontend/.github/workflows/release.yml` and `Concertable/.github`'s `nuget-publish.yml` all
exist; `Concertable.Shared.Geocoding.Application` is a real id in `eng/repository-split/inventory.json`.
