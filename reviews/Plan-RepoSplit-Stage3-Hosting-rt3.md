# Code review — Plan/RepoSplit-Stage3-Hosting-rt3

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `8a9af31ff`  `(2026-09-03)`
**Security-reviewed up to commit:** `8a9af31ff`  `(2026-09-03)`
**Judgment:** `changes-requested`

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

## Review pass — 2026-09-03 — incremental

**Candidate base:** `122c98ece3d6cfa0a7c3aede61b213874cb7bc03`
**Candidate head:** `76b20c877db47293524a0a9164a5f6a31b3c8294`
**Candidate branch:** `Plan/RepoSplit-Stage3-Hosting-rt3`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:6e5b60697c77f3d62461a9f80a43fa0e101fc9792f4afa0219b1cd09ba7bc676` `(8 paths)`
**Candidate bundle:** `reviewed in-place from the frozen range; no disposable bundle materialized`
**Candidate bundle identity:** `n/a — see Deviations in the full pass`
**Work-order path:** `reviews/Plan-RepoSplit-Stage3-Hosting-rt3.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

Delta driven by merge-queue run 33799140674, which executed all ten B2B API E2E tests for the
first time on this branch and failed all ten on the Auth endpoint. Evidence: `TestTokenMinter`
dialling `https://localhost:7083/connect/token` returned `AuthenticationException: Cannot determine
the frame size or a corrupted frame was received`, the B2B workers host's service-token call failed
identically, and `docker image inspect` of the pinned digest shows `ASPNETCORE_HTTP_PORTS=8080`
with no HTTPS port and `ExposedPorts: {"8080/tcp"}`.

### Findings

- [x] **RT3-5 — HIGH — correctness** — `api/Concertable.Auth/src/Concertable.Auth.Hosting/AppHostExtensions.cs:31`
  `WithHttpsDeveloperCertificate()` supplied the pinned Auth image a certificate but nothing opened a
  TLS socket, while all four AppHosts declare that container port as their `https` endpoint — the
  endpoint asserted a protocol the container never spoke, so every https consumer met a plaintext
  listener. Fixed by setting `ASPNETCORE_URLS` to the container's own https URL, which outranks
  `ASPNETCORE_HTTP_PORTS`, guarded to run mode so a published manifest never inherits the developer
  certificate. The repeated `8080` literal is now one `AuthConstants.ContainerPort` shared by the URL
  and the four endpoint declarations. Verified: all four AppHost architecture suites pass (9/5/10/4/2),
  including each publish-graph test.

- [x] **RT3-6 — LOW — test-coverage** — `api/Concertable.Search/tests/E2ETests/Concertable.Search.E2ETests.Helpers.UnitTests/ContainerBackedPinningTests.cs:112`
  `PinHttpsEndpoint` changed from replacing an endpoint to mutating one, so the create path every
  substituted E2E project still uses had no guard against drifting from the previous declarative form.
  Pinned by asserting name, scheme, port, target port, proxy mode and external flag against a
  declaratively built peer. This also rules the RT3 helper change out as the cause of RT3-5.

### Security layer

Re-run over the delta. `Concertable.Auth*` and `Concertable.Payment*` paths changed, so the marker is
re-stamped at this head. No HIGH or MEDIUM finding. `ASPNETCORE_URLS` is a trusted orchestration value,
the developer certificate stays run-mode-only by the new `IsRunMode` guard — which makes the existing
"publish mode is unaffected" comment true rather than aspirational — and no secret handling changed.

### Verification state at this head

`e2e-api-tests` remains unverified at this head; it runs only in the merge queue, and local runs cannot
reach it in this checkout (missing `ServiceAuth__B2BClientSecret`, `ServiceAuth__CustomerClientSecret`,
`ServiceAuth__AuthClientSecret`). What run 33799140674 did establish, and this head keeps: the fixture
reaches ready, auth and payment-web both publish on their contract ports, and all ten tests execute.

## Review pass — 2026-09-03 — incremental

**Candidate base:** `76b20c877db47293524a0a9164a5f6a31b3c8294`
**Candidate head:** `8a9af31ff`
**Candidate branch:** `Plan/RepoSplit-Stage3-Hosting-rt3`
**Candidate scope:** `all`
**Candidate bundle:** `reviewed in-place from the frozen range; no disposable bundle materialized`
**Candidate bundle identity:** `n/a — see Deviations in the full pass`
**Work-order path:** `reviews/Plan-RepoSplit-Stage3-Hosting-rt3.md`
**Work-order mode:** `append`
**Pass judgment:** `changes-requested`

### Findings

- [ ] **RT3-7 — HIGH — correctness** — `api/Concertable.Auth/src/Concertable.Auth.Hosting/AppHostExtensions.cs:24`
  **This branch cannot pass `e2e-api-tests` and must not be merged until this is decided.**
  `WithHttpsDeveloperCertificate()` does not supply a usable certificate to the pinned Auth image,
  so the https-Auth bridge the RT3 E2E story rests on has no working implementation. Both reachable
  states are red, proven by two queue runs:
  - Without `ASPNETCORE_URLS` (current state, run 33799140674): the image binds HTTP only, so the port
    every AppHost declares as `https` serves plaintext. All ten tests execute and all ten fail —
    `TestTokenMinter` and the B2B workers' service-token call both get
    `AuthenticationException: Cannot determine the frame size or a corrupted frame was received`.
  - With `ASPNETCORE_URLS=https://+:8080` (run 33803938996, reverted in `8a9af31ff`): Kestrel fails to
    bind — `Unable to configure HTTPS endpoint. No server certificate was specified, and the default
    developer certificate could not be found or is out of date` — auth exits and every dependent
    cascades. Strictly worse, hence the revert.

  Three candidate resolutions, each a real trade-off for the owner to pick:
  1. Serve Auth over HTTP in E2E (what `36299abfc` did before `dd95b011a` reversed it): declare the
     endpoint `http`, make `Endpoints.Auth` an `http://` URL, and treat `E2E` as dev-like for
     `RequireHttpsMetadata`. Unblocks now; costs the https fidelity `dd95b011a` was reaching for.
  2. Mount a real certificate: export the developer certificate to a file, bind-mount it into the
     container, and set `Kestrel__Certificates__Default__Path`/`__Password`. Faithful; the workflow's
     existing "Provision HTTPS dev certificate" step is the hook, and local dev needs the same.
  3. Wait for a corrected Auth image that serves HTTPS — already the documented removal gate this
     bridge shares with `--user root`.

### Verification state at this head

Red. `e2e-api-tests` fails at this head for the reason recorded above. `AuthConstants.ContainerPort`
and its four AppHost call sites are kept from the reverted commit and are verified by the architecture
suites (B2B 20/20, Auth 2/2).

