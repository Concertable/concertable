# Repository-per-microservice migration — Stage 4 system E2E progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Plan-RepoSplit-Stage4-Fleet-E2E`
- Branch: `Plan/RepoSplit-Stage4-Fleet-E2E`
- PR: [#896](https://github.com/Concertable/concertable/pull/896) — draft
- Last reconciled: **2026-09-01** from current branch head `fef64122e`, current dependency inventory, and exact-head local validation

## Current state

This ledger exclusively owns Stage 4: removing remaining cross-service E2E implementation composition from
service repositories and moving full-system composition into the future `Concertable/system` repository.
No agent following this ledger may execute or edit RT3, CUSTOMER-FRONTEND, AUTH-NEXT, or the old shared
migration ledger.

The umbrella Aspire project is now canonically `Concertable.System.AppHost`, with namespace
`Concertable.System.AppHost` and entry composition type `AppHost` in `AppHost.cs`. The System E2E projects
retain canonical `Concertable.System.*` assembly names while their C# namespaces use
`Concertable.SystemTesting.*`; this avoids shadowing the .NET `System` namespace in generated Reqnroll code.

The source-composition seam is named for what it does: `ISystemAppHostFactory`,
`SystemAppHostFactories`, and `SourceSystemAppHostFactory`. There are no remaining provider-prefixed,
Fleet-prefixed, `Concertable.AppHost`, or `ConcertableAppHost` identifiers in the active Stage 4 corpus.

The regenerated inventory contains 51 cross-target edges. Eight are temporary source-mode System E2E edges,
all owned by the non-packable, carve-excluded source factory: two each for B2B, Customer, Payment, and Search.
There are zero blocking E2E edges and zero blocking test edges. The one blocking runtime edge remains the
pre-existing Auth Contracts dependency on Messaging Contracts and is outside this stream.

The shared Auth, Search, Payment Web, and Payment Workers pinning helpers now accept image-backed/container
resources as well as source `ProjectResource` instances. Project metadata is replaced only when the resource
is source-backed; image/container annotations remain intact. Payment still retains its E2E TestKit admin
resource and test-admin environment behavior.

## Next Steps

Push the reviewed exact-head candidate to draft PR #896 and await CI/reviewer feedback; do not mark it ready,
enqueue it, or merge it while the full UI E2E gate is red. The boundary regression that blocked RT3 is fixed:
isolated B2B and Customer OIDC probes pass with image-backed foreign resources, and the full API E2E suite
passes 11/11. The full UI run reaches product flows but currently passes 32/39; the seven failures reproduce
in unchanged B2B/Customer product flows and are outside this Stage 4 stream. The runner now returns non-zero
for failures or zero discovered tests, so this state cannot be mistaken for a green gate.

After the full gate is restored by the owning product streams and this slice merges, select the Payment pair
as the next smallest independently shippable Stage 4 slice: replace the source factory's Payment Web and
Workers implementation references with service-owned package/image composition, reducing the eight remaining
temporary source-mode edges to six without moving service behavior or duplicating TestKit contracts.

## Completed work

- PR #882 established service-owned TestKits, E2E admin hosts, the System AppHost factory seam, and `carve-e2e.ps1`.
- Created the owning isolated worktree and kept all Stage 4 progress updates in this ledger.
- Moved Search Web and Workers project metadata behind `ISystemAppHostFactory`; the carve-retained Search helper no longer knows repository-relative service project paths.
- Renamed Search extension containers for their `IDistributedApplicationBuilder` receiver and recorded the broader existing naming inconsistency as tech debt.
- Renamed the full-system repository, Aspire project, namespace, composition type, architecture tests, E2E projects, profiles, surfaces, runs, and documentation from Fleet/legacy AppHost terminology to System/AppHost terminology.
- Renamed the source-composition provider abstraction and collaborators to factory terminology.
- Made Auth, Search, Payment Web, and Payment Workers E2E pinning resource-neutral while preserving Payment TestKit/admin behavior.
- Added Search container-backed pinning coverage and retained the Payment helper model coverage.
- Synchronized the seven participating package pin files to the current `1281` service package train required by the E2E composition build.
- Corrected `scripts/e2e.ps1` so UI/API runs fail on failed scenarios or zero discovered tests.
- Regenerated `eng/repository-split/inventory.json` from the current candidate.

## Verification

- `python eng/repository-split/inventory.py --check` — passed; inventory current, zero blocking E2E/test edges.
- Search E2E helper unit tests — passed 2/2 in source mode and 2/2 in the package-only carve.
- Payment E2E helper unit tests — passed 6/6 in source mode and 6/6 in the package-only carve.
- System source-factory unit tests — passed 2/2.
- System AppHost architecture tests — passed 3/3.
- The final `scripts/e2e.ps1 ui run` preparation generated 55 exact-head packages at `0.1.0-local.1788217534474`.
- Short-path package-only `Concertable.System.E2E.slnx` carve build — passed with zero errors; existing package-resolution, MessagePack advisory, and generated nullable warnings remain.
- `scripts/e2e.ps1 api run` — passed B2B 10/10 and Customer 1/1, total 11/11.
- Isolated B2B OIDC and Customer OIDC UI probes — passed 1/1 each with image-backed foreign resources.
- `scripts/e2e.ps1 ui run` — B2B 26/32 and Customer 6/7, total 32/39, with process exit code 1; gate remains red. Four opportunity flows stop before Payment because the unchanged venue UI remains in `Editing`; two sign-up flows time out after successful submission while waiting for SPA-root navigation; the Customer purchase journey is also an unchanged product-flow failure.
- `scripts/e2e.ps1 help` under Windows PowerShell — parsed and executed successfully after the exit-semantics change.

## Reviews

The canonical review record is `reviews/Plan-RepoSplit-Stage4-System-E2E.md`. Refresh its exact-head review
after staging the final ledger and runner changes, then push only if no unresolved Stage 4 finding remains.

## Decisions, discoveries, blockers, and deviations

- `Concertable.Testing.E2E` remains service-agnostic; System-specific types belong to System-owned projects.
- Service TestKits expose service-owned reset/seed/client contracts; System owns cross-service orchestration.
- Image-backed resources are the canonical representation of foreign services; shared pinning must not require `ProjectResource`.
- Resource replacement is metadata-only and applies only to source projects; container resources keep their image identity.
- The full UI run originally returned exit code zero despite seven failures. That runner defect is fixed and is not accepted as validation evidence.
- The first fresh carve build used a stale local package train and correctly failed downgrade checks. Regenerating the exact-head 55-package feed removed that mismatch.
- A nested-worktree carve then exceeded Windows' path-length limit. Repeating the identical carve beneath the repository's short ignored `artifacts` path passed.
- The seven remaining UI failures are not caused by the System naming/resource-neutral pinning diff. CUSTOMER-FRONTEND and B2B product-flow changes are explicitly outside this stream, so this PR records the red gate without bypassing or absorbing those fixes.
