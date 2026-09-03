# Code review — Plan/RepoSplit-Stage3-Hosting-rt3

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `122c98ece3d6cfa0a7c3aede61b213874cb7bc03`  `(2026-09-03)`
**Security-reviewed up to commit:** `122c98ece3d6cfa0a7c3aede61b213874cb7bc03`  `(2026-09-03)`
**Judgment:** `approved`

## Review pass — 2026-09-03 — full

**Candidate base:** `a43ca6f0d8a1c5e9995e8b6046344431cd20e0b0`
**Candidate head:** `122c98ece3d6cfa0a7c3aede61b213874cb7bc03`
**Candidate branch:** `Plan/RepoSplit-Stage3-Hosting-rt3`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:23aa78eea650421402422d9a815a21436440d35ba91bd7b0b023c74d03e8011b` `(42 paths)`
**Candidate bundle:** `reviewed in-place from the frozen range; no disposable bundle materialized`
**Candidate bundle identity:** `n/a — see Deviations`
**Work-order path:** `reviews/Plan-RepoSplit-Stage3-Hosting-rt3.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

Routed rules read for this path set: `packages`, `composition-testing`, `microservice-boundaries`,
`module-structure`, `unit-testing`, `integration-testing`, `csharp-style`, `csharp-naming`,
`e2e-scenarios`.

### Findings

- [x] **RT3-1 — LOW — packages** — `api/PlatformSourcePackages.targets:12`
  The AppHost swap condition matched `MSBuildProjectName.Contains('.AppHost')`, which also matches the
  `Concertable.AppHost.Shared` library, not just the six executable AppHosts. Inert only because that
  project references no id in the `PlatformSourcePackage` list; the moment it gained one it would be
  swapped to a `ProjectReference` in the monorepo alone, so the carve build would be the first thing to
  see the package path. Fixed by matching `EndsWith('.AppHost')`, which selects the five service AppHosts
  and the umbrella AppHost and nothing else — the same EndsWith-before-Contains discipline the test-tier
  naming gate uses.

- [x] **RT3-2 — MEDIUM — changed-behaviour test impact** — `scripts/e2e.ps1:104`
  This candidate makes every E2E stack boot from digest-pinned `ghcr.io` images and adds a `Log in to
  GHCR` step to all three CI E2E jobs, but nothing supplies that credential for a local run and no doc
  states the prerequisite. Observed on this branch: the pull fails `unauthorized`, Aspire marks `auth`
  `FailedToStart`, `b2b-web` / `search-web` / `payment-web-e2e` / all four SPAs cascade on
  `Dependency resource 'auth' failed to start`, and the fixture reports
  `Readiness check timed out for https://localhost:7088/health` — three layers from the cause. Fixed by
  `Assert-PinnedImagesPullable`, which resolves the pinned Auth reference before the stack boots and
  names the `docker login ghcr.io` remedy; wired into both the `api` and `ui` gates.

- [x] **RT3-3 — LOW — correctness** — `api/Concertable.Search/tests/E2ETests/Concertable.Search.E2ETests.Helpers.UnitTests/ContainerBackedPinningTests.cs:170`
  The `Environment` helper constructed an `EnvironmentCallbackContext` without a resource, so any
  endpoint-reference callback threw `Resource is not set` and the helper could not evaluate a real
  service resource's environment at all — it only ever worked on hand-built resources with plain-string
  env. Fixed by passing the resource, plus a case covering the substituted Payment web host carrying
  `ServiceBus__ServiceName`.

- [wontfix] **RT3-4 — LOW — reuse** — `api/Concertable.B2B/tests/Concertable.B2B.ArchitectureTests/B2BHostGraphTests.cs:200`
  `AssertImageEndpoint`, `AssertContainerRuntimeArgs` and `AssertUsesDeveloperCertificate` are declared
  verbatim in all four service architecture suites (`B2BHostGraphTests`, `CustomerArchitectureTests`,
  `PaymentArchitectureTests`, `SearchArchitectureTests`), ~45 lines each. Sharing them requires
  `Concertable.Testing.Architecture` to take `Aspire.Hosting` and `Concertable.AppHost.Shared`
  dependencies it does not currently have, which changes what a shared platform test package carries.
  Transferred to `api/Concertable.AppHost.Shared/TECH_DEBT.md` with an objective resolution condition.

### Security layer

Qualifying paths: `Concertable.Auth*`, `Concertable.Payment*`, `.github/workflows/`. No HIGH or MEDIUM
finding survived filtering.

- `RequireHttpsMetadata = !environment.IsDevelopment()` (`Concertable.B2B.Web`, `Concertable.Customer.Web`)
  was unset at the base, so the default was `true`. The change **relaxes** it in `Development` only;
  `E2E` and `Production` keep `true`, both now pinned by a composition test, and it matches the adjacent
  pre-existing `ValidateIssuer = !environment.IsDevelopment()`.
- `--user root` on the pinned Auth container and `WithHttpsDeveloperCertificate()` are run-mode dev/E2E
  only, documented as a temporary bridge sharing one removal gate (a corrected Auth image).
- The GHCR login uses `secrets.GITHUB_TOKEN` with job-scoped `packages: read`, on `merge_group`/`push`
  triggers only, with `e2e-ghcr-login.test.mjs` asserting the step exists in all three E2E jobs.
- Digest-pinning the service images is a net improvement over tag references.

### Deviations from the canonical procedure

Recorded so the next reader does not mistake this for a full-strength pass:

- No disposable candidate bundle was materialized; the pass was frozen by explicit `base..head` range and
  path-set digest and reviewed from the worktree at that head.
- Native/general and per-concern lenses ran in the parent context rather than as isolated dispatches,
  and the security layer likewise, because subagent dispatch was unavailable for this session.

### Verification state at this head

`e2e-api-tests` is **not** verified at this head. It runs only in the merge queue. The local B2B API tier
was driven to the point of proving the `0bed74b3f` fix — `auth` stays `Running` with no failure on the
pinned 7083 contract port, and `b2b-web`, `search-web`, `search-workers` and `workers` all reach healthy,
all of which were dead before it — then stopped at `payment-web-e2e` crashing on
`Configuration 'ServiceBus:ServiceName' is required`. That host reached `Running` in CI at the parent
commit, and this checkout lacks the `ServiceAuth__B2BClientSecret`, `ServiceAuth__CustomerClientSecret`
and `ServiceAuth__AuthClientSecret` values CI injects, so the local stop is unattributed and the merge
queue owns the verdict.
