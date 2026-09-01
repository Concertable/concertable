# Repository-per-microservice migration — Stage 4 system E2E progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Plan-RepoSplit-Stage4-System-E2E`
- Branch: `Plan/RepoSplit-Stage4-System-E2E`
- PR: [#912](https://github.com/Concertable/concertable/pull/912) — draft
- Last reconciled: **2026-09-01** from branch head `3a104fbbf`, current dependency inventory, and exact-candidate local validation

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
obsolete umbrella-name-prefixed, `Concertable.AppHost`, or `ConcertableAppHost` identifiers in the active Stage 4 corpus.

The regenerated inventory contains 51 cross-target edges. Eight are temporary source-mode System E2E edges,
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

Push the reviewed exact-head candidate to draft PR #912 and await CI/reviewer feedback; do not mark it ready,
enqueue it, or merge it while the full UI E2E gate is red. The boundary regression that blocked RT3 is fixed:
focused image-resource tests cover Search pinning and the Payment E2E-host substitution, isolated B2B and
Customer OIDC probes pass with image-backed foreign resources, and the full API E2E suite passes 11/11.
The latest full UI run reaches product flows but passes 29/39; those product-flow failures are outside this
Stage 4 stream. The runner now returns non-zero for failed/empty suites and non-zero testhost exits, so this
state cannot be mistaken for a green gate.

After the full gate is restored by the owning product streams and this slice merges, select the Payment pair
as the next smallest independently shippable Stage 4 slice: replace the source factory's Payment Web and
Workers implementation references with service-owned package/image composition, reducing the eight remaining
temporary source-mode edges to six without moving service behavior or duplicating TestKit contracts.

## Completed work

- PR #882 established service-owned TestKits, E2E admin hosts, the System AppHost factory seam, and `carve-e2e.ps1`.
- Created the owning isolated worktree and kept all Stage 4 progress updates in this ledger.
- Moved Search Web and Workers project metadata behind `ISystemAppHostFactory`; the carve-retained Search helper no longer knows repository-relative service project paths.
- Renamed Search extension containers for their `IDistributedApplicationBuilder` receiver and recorded the broader existing naming inconsistency as tech debt.
- Renamed the full-system repository, Aspire project, namespace, composition type, architecture tests, E2E projects, profiles, surfaces, runs, and documentation to canonical System/AppHost terminology.
- Renamed the source-composition provider abstraction and collaborators to factory terminology.
- Made Auth and Search pinning resource-neutral and made image-backed Payment compositions substitute the Payment-owned E2E hosts while retaining the imported production-image resources as explicit-start graph members.
- Added focused Search container-pinning and Payment E2E-host substitution coverage, including copied environment and retargeted-wait assertions.
- Synchronized the seven participating package pin files to the current `1281` service package train required by the E2E composition build.
- Corrected `scripts/e2e.ps1` so UI/API runs fail on failed scenarios, zero discovered tests, or an aborted/non-zero testhost even when a partial TRX exists.
- Regenerated `eng/repository-split/inventory.json` from the current candidate.

## Verification

- `python eng/repository-split/inventory.py --check` — passed; inventory current, zero blocking E2E/test edges.
- Search/shared E2E helper unit tests — passed 3/3 in source mode; coverage includes the image-backed Search configuration and Payment E2E-host substitution paths.
- Payment E2E helper unit tests — passed 6/6 in source mode and 6/6 in the package-only carve.
- System source-factory unit tests — passed 2/2.
- System AppHost architecture tests — passed 3/3.
- The final `scripts/e2e.ps1 ui run` preparation generated 55 exact-candidate packages at `0.1.0-local.1788222842907`.
- Short-path package-only `Concertable.System.E2E.slnx` carve build — passed with zero errors; existing package-resolution, MessagePack advisory, and generated nullable warnings remain.
- `scripts/e2e.ps1 api run` — passed B2B 10/10 and Customer 1/1, total 11/11.
- Isolated B2B OIDC and Customer OIDC UI probes — passed 1/1 each with image-backed foreign resources.
- `scripts/e2e.ps1 ui run` — B2B 24/32 and Customer 5/7, total 29/39, with process exit code 1; gate remains red. The previously recorded booking, sign-up, and Customer purchase failures remain, with three additional card/OIDC variants failing in this run; no gate bypass was applied.
- `scripts/e2e.ps1 help` under Windows PowerShell — parsed and executed successfully after the exit-semantics change.

## Reviews

The canonical branch-local review record is `reviews/Plan-RepoSplit-Stage4-System-E2E.md`. Refresh its exact-head review
after staging the final ledger and runner changes, then push only if no unresolved Stage 4 finding remains.

## Decisions, discoveries, blockers, and deviations

- `Concertable.Testing.E2E` remains service-agnostic; System-specific types belong to System-owned projects.
- Service TestKits expose service-owned reset/seed/client contracts; System owns cross-service orchestration.
- Image-backed resources are the canonical representation of foreign services; shared pinning must not require the imported resource itself to be a `ProjectResource`.
- Auth and Search containers run in place. Payment's production containers keep their image identity in the graph but are explicit-start during E2E because the Payment-owned E2E Web/Workers projects provide the required admin and Stripe test behavior.
- The full UI run originally returned exit code zero despite seven failures. That runner defect is fixed and is not accepted as validation evidence.
- The first fresh carve build used a stale local package train and correctly failed downgrade checks. Regenerating the exact-head 55-package feed removed that mismatch.
- A nested-worktree carve then exceeded Windows' path-length limit. Repeating the identical carve beneath the repository's short ignored `artifacts` path passed.
- The remaining UI failures are not caused by the System naming/image-boundary diff. CUSTOMER-FRONTEND and B2B product-flow changes are explicitly outside this stream, so this PR records the red gate without bypassing or absorbing those fixes.
