# Repository-per-microservice migration — Stage 4 system E2E progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\s4`
- Branch: `Plan/RepoSplit-Stage4-System-E2E`
- PR: [#912](https://github.com/Concertable/concertable/pull/912) — draft
- Last reconciled: **2026-09-02** from branch head `4e740aeaf`, current dependency inventory, and exact-candidate local validation

## Current state

This ledger exclusively owns Stage 4: removing remaining cross-service E2E implementation composition from
service repositories and moving full-system composition into the future `Concertable/system` repository.
No agent following this ledger may execute or edit RT3, CUSTOMER-FRONTEND, AUTH-NEXT, or the old shared
migration ledger.

The future repository remains `Concertable/system`: system describes the repository's responsibility for
full-product composition and testing, but is not a C# namespace prefix. Its umbrella Aspire project is
`Concertable.AppHost`, and its code namespace is also `Concertable.AppHost`. The idiomatic top-level entry
file is `AppHost.cs`; `AppHostFactory` exposes the reusable builder seam required by architecture tests.
The split preserves artifact namespacing at `src/Concertable.AppHost/`; deployment
manifests live under `manifests/` rather than an ambiguous nested `system/` directory.

The full-product E2E projects use matching `Concertable.E2E*` assembly and root-namespace names. Explicit
`RootNamespace` values on the AppHost, E2E, source-composition, unit-test, and architecture-test projects keep
future scaffolded files on that convention. The source-composition seam is named for its responsibility:
`IComposition`, `Compositions`, and `SourceComposition`. `Compositions.Source()` is covered through the real
reflection path, so its assembly-qualified binding cannot silently drift.

The regenerated inventory contains 51 cross-target edges. Eight are temporary source-mode full-product E2E edges,
all owned by the non-packable, carve-excluded source factory: two each for B2B, Customer, Payment, and Search.
There are zero blocking E2E edges and zero blocking test edges. The one blocking runtime edge remains the
pre-existing Auth Contracts dependency on Messaging Contracts and is outside this stream.

The shared Auth, Search, Payment Web, and Payment Workers pinning helpers now accept image-backed/container
resources as well as source `ProjectResource` instances. Auth and Search retain their image identity and are
pinned in place. A production Payment image intentionally has no TestKit routes or Stripe test adapter, so
the imported Payment containers remain in the graph as explicit-start resources while the Payment-owned E2E
Web and Workers projects run with copied database/bus configuration; dependent waits are retargeted to those
E2E hosts. This preserves Payment's E2E admin reset and Stripe behavior without adding it to a production image.

## Next Steps

Push the reviewed exact-head candidate to draft PR #912 and use its secret-backed CI and merge-queue runs as
the authoritative remaining E2E gates; do not mark it ready, enqueue it, or merge it while a blocking E2E
check is red. The boundary regression that blocked RT3 is fixed:
focused image-resource tests cover Search pinning and the Payment E2E-host substitution, isolated B2B and
Customer OIDC probes pass with image-backed foreign resources, and the sole failure from the latest full API
run passed in isolation after correcting the source-project build-output seam. The latest local all-scenario
UI run passes 32/39. Its six B2B failures require the `GOOGLE_API_KEY` supplied by CI but absent from this fresh
worktree; its one Customer failure is the existing `@quarantine` genre-filter scenario, which runs in CI's
non-gating quarantine lane and is excluded from the blocking UI lane. The runner returns non-zero for
failed/empty suites and non-zero testhost exits, so neither limitation can be mistaken for a green gate.

After the full gate is restored by the owning product streams and this slice merges, select the Payment pair
as the next smallest independently shippable Stage 4 slice: replace the source factory's Payment Web and
Workers implementation references with service-owned package/image composition, reducing the eight remaining
temporary source-mode edges to six without moving service behavior or duplicating TestKit contracts.

## Completed work

- PR #882 established service-owned TestKits, E2E admin hosts, the full-product composition seam, and `carve-e2e.ps1`.
- Created the owning isolated worktree and kept all Stage 4 progress updates in this ledger.
- Moved Search Web and Workers project metadata behind the composition seam; the carve-retained Search helper no longer knows repository-relative service project paths.
- Renamed Search extension containers for their `IDistributedApplicationBuilder` receiver and recorded the broader existing naming inconsistency as tech debt.
- Reconciled the `Concertable/system` repository vocabulary without leaking `System` into .NET identifiers: `Concertable.AppHost` for the Aspire project and namespace, and `Concertable.E2E*` for full-product tests.
- Preserved namespaced split layout at `src/Concertable.AppHost/`, selected `manifests/` for deployment inputs, and recorded the rationale in the split map.
- Renamed the source-composition abstraction and collaborators to `IComposition`, `Compositions`, and `SourceComposition`.
- Gave Search and Payment's divergent pinning operations semantic names: Search replaces project metadata only for source-backed resources, while Payment substitutes its E2E projects for image-backed resources to retain TestKit/admin behavior.
- Made Auth and Search pinning resource-neutral and made image-backed Payment compositions substitute the Payment-owned E2E hosts while retaining the imported production-image resources as explicit-start graph members.
- Added focused Search container-pinning and Payment E2E-host substitution coverage, including copied environment and retargeted-wait assertions.
- Synchronized the seven participating package pin files to the current `1281` service package train required by the E2E composition build.
- Corrected `scripts/e2e.ps1` so UI/API runs fail on failed scenarios, zero discovered tests, or an aborted/non-zero testhost even when a partial TRX exists.
- Corrected the E2E carve manifest so the Search helper and its unit-test project are copied as two distinct paths.
- Regenerated `eng/repository-split/inventory.json` from the current candidate.

## Verification

- `python eng/repository-split/inventory.py --check` — passed; inventory current, zero blocking E2E/test edges.
- Search/shared E2E helper unit tests — passed 3/3 in source mode; coverage includes the image-backed Search configuration and Payment E2E-host substitution paths.
- Payment E2E helper unit tests — passed 6/6 in source mode and 6/6 in the package-only carve.
- Full-product source-composition unit tests — passed 3/3 in isolated per-project build output, including `Compositions.Source()` reflection resolution.
- AppHost architecture tests — passed 3/3 in isolated per-project build output.
- `Concertable.E2E` core project build — passed with zero warnings and zero errors.
- B2B and Customer E2E composition projects — built successfully against the renamed composition contract in isolated per-project output; only sandboxed NuGet vulnerability-data warnings were emitted.
- The final `scripts/e2e.ps1 ui run` preparation generated 55 exact-candidate packages at `0.1.0-local.1788306290376`.
- Short-path package-only `Concertable.E2E.slnx` carve build — passed against exact candidate `0.1.0-local.1788306290376` with `UseSourceComposition=false`; 12 projects built with zero errors. Carved Search helper tests passed 3/3 and Payment helper tests passed 6/6.
- `scripts/e2e.ps1 api run` — passed 10/11; the sole Customer readiness failure then passed 1/1 in isolation after restoring conventional consumer build outputs required by Aspire's `dotnet run --no-build` source launch.
- Isolated B2B OIDC and Customer OIDC UI probes — passed 1/1 each with image-backed foreign resources.
- `scripts/e2e.ps1 ui run` — B2B 26/32 and Customer 6/7, total 32/39, with process exit code 1; no gate bypass was applied. All six B2B failures reach geocoding and fail because this fresh worktree has no `GoogleApiKey`; the blocking CI job supplies `GOOGLE_API_KEY`. The Customer failure is the pre-existing `@quarantine` genre-filter scenario and is excluded from the blocking CI lane. A previously failing representative B2B member-management scenario passed 1/1 from the canonical short worktree path after removing the duplicate-React drive alias.
- `scripts/e2e.ps1 help` under Windows PowerShell — parsed and executed successfully after the exit-semantics change.

## Reviews

The canonical branch-local review record is `reviews/Plan-RepoSplit-Stage4-System-E2E.md`. The full and
remediation passes are complete and approved with no open findings. The review corrected the remaining live
`Concertable.System.AppHost` run instructions and the shared E2E helper's mixed legacy/C# 14 extension syntax;
the focused Search and Payment helper suites passed after remediation.

## Decisions, discoveries, blockers, and deviations

- `Concertable.Testing.E2E` remains service-agnostic; full-product types belong to system-repository-owned projects.
- Service TestKits expose service-owned reset/seed/client contracts; the system repository owns cross-service orchestration.
- `system` is the repository/responsibility vocabulary, not a .NET identifier. Avoiding `Concertable.System.*` prevents shadowing the BCL namespace and keeps assembly and source namespaces aligned.
- `src/Concertable.AppHost/` retains useful artifact namespacing after the split; flattening it to `apphost/` would discard that convention without benefit.
- Image-backed resources are the canonical representation of foreign services; shared pinning must not require the imported resource itself to be a `ProjectResource`.
- Auth and Search containers run in place. Payment's production containers keep their image identity in the graph but are explicit-start during E2E because the Payment-owned E2E Web/Workers projects provide the required admin and Stripe test behavior.
- The full UI run originally returned exit code zero despite seven failures. That runner defect is fixed and is not accepted as validation evidence.
- The first fresh carve build used a stale local package train and correctly failed downgrade checks. Regenerating the exact-head 55-package feed removed that mismatch.
- A nested-worktree carve then exceeded Windows' path-length limit. Repeating the identical carve beneath the repository's short ignored `artifacts` path passed.
- The remaining local UI failures are not caused by the naming/image-boundary diff. CI owns the real Google key and explicitly excludes `@quarantine` from its blocking UI lane, so the next candidate must obtain authoritative remote E2E evidence rather than changing product behavior or fabricating a local secret in this stream.
