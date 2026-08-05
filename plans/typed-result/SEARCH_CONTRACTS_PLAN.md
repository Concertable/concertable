# Search contract normalization plan

> **Next steps live in @plans/typed-result/SEARCH_CONTRACTS_PROGRESS.md → `## Next Steps`.**

## Objective

Normalize Search's in-process collection operation contracts to successful, materialized
`IReadOnlyList<T>` values while preserving the service's existing search behavior, transport shapes,
projection boundaries, exception semantics, and standalone package closure. Introduce a typed
`Result` or `Option` only if implementation uncovers a real caller decision that the current audit did
not find; such a discovery changes this plan and must be recorded before code adopts the new carrier.

## Audited baseline

The audit was performed against fresh `origin/main` across Search Application interfaces and
services, `HeaderDispatcher`, API controllers, Infrastructure repositories, focused integration
coverage, the shared typed-result architecture tests, and the owned Kernel functional contracts.

- Search production source and project files contain no FluentResults or
  CSharpFunctionalExtensions usage, no owned `Result`/`Option` usage, and no nullable single-item
  application result.
- Every affected query already terminates with EF Core `ToListAsync()`. No-match behavior is therefore
  already a successful empty `List<T>` at runtime; the ambiguity is only in the declared
  `IEnumerable<T>` contracts.
- The autocomplete chain declares `Task<IEnumerable<Autocomplete>>` in the all/artist/concert/venue
  repositories, their implementations, `IAutocompleteService`, and the four service implementations.
- The header chain declares `Task<IEnumerable<T>>` in the artist/venue/concert repositories, the
  header and concert-header services, and the dispatcher. It covers amount, popular, free, and
  recommended queries.
- `SearchAsync` is not part of this normalization: it returns `IPagination<T>`, whose `Data` is already
  `IReadOnlyList<T>` and whose infrastructure mapper materializes it with `ToListAsync()`.
- Controllers already terminate through `IActionResult` and serialize the application payloads as
  JSON arrays or the existing pagination object. They have no domain-failure branch to map.
- Nullable `searchTerm`, nullable `HeaderType` for the all-autocomplete selection, and nullable search
  filters are ordinary input vocabulary. Missing keyed registrations are configuration/invariant
  defects from `GetRequiredKeyedService`, not expected domain failures.
- Existing integration coverage proves successful empty responses for unmatched autocomplete,
  unmatched paginated search, no free concerts, and unmatched recommendations. It also covers the
  corresponding populated paths and API-owned validation/authentication responses.
- `IEnumerable<Genre>` on header DTO properties and in the two EF projection expressions is not an
  operation return contract. Those members participate in projection translation and JSON wire
  shapes and remain unchanged in this slice.

## Contract target

| Boundary | Current shape | Target shape |
|---|---|---|
| Autocomplete repositories | `Task<IEnumerable<Autocomplete>>` | `Task<IReadOnlyList<Autocomplete>>` |
| Autocomplete service | `Task<IEnumerable<Autocomplete>>` | `Task<IReadOnlyList<Autocomplete>>` |
| Header amount repositories | `Task<IEnumerable<ArtistHeader/VenueHeader/ConcertHeader>>` | matching `Task<IReadOnlyList<THeader>>` |
| Header service and dispatcher amount queries | `Task<IEnumerable<IHeader>>` | `Task<IReadOnlyList<IHeader>>` |
| Concert popular/free/recommended repositories and service | `Task<IEnumerable<ConcertHeader>>` | `Task<IReadOnlyList<ConcertHeader>>` |
| Paginated search | `Task<IPagination<T>>` with read-only `Data` | unchanged |
| API actions and JSON payloads | `Task<IActionResult>` arrays/pagination | unchanged |

`IReadOnlyList<T>` covariance allows the header services to return the repository's materialized
`IReadOnlyList<ArtistHeader>`, `IReadOnlyList<VenueHeader>`, or `IReadOnlyList<ConcertHeader>` as
`IReadOnlyList<IHeader>` after awaiting it. Do not add casts, rematerialization, or wrapper allocations.

## Scope constraints

Preserve all of the following:

- Empty search and autocomplete matches are successful empty collections, never `null`, failure, or
  `Option.None`.
- Nullable query and filter inputs remain nullable ordinary input values.
- Projection mapping, projection persistence, integration-event handlers, event DTOs, and producer
  contracts remain unchanged.
- Provider, database, infrastructure, cancellation, and invariant failures stay on the exception
  path. Do not catch them into a Result.
- Search continues to consume Kernel and cross-service contracts through published package references;
  no source reference may escape the Search service folder.
- HTTP arrays, pagination JSON, polymorphic header metadata, authentication, validation, and every
  other wire contract remain transport-owned shapes.

Out of scope:

- B2B or Customer producer-contract changes.
- Projection seeding, event flow, read-model, mapper, or persistence-schema changes.
- Shared Kernel API or owned functional-type changes.
- Cross-service runtime or project references.
- `Option<T>` around collections or opportunistic operation-error/Result types.
- Renaming services, repositories, controllers, DTOs, or unrelated cleanup.

## Phase 1 — Normalize Search operation collection contracts

Change the complete declared return chain from `IEnumerable<T>` to `IReadOnlyList<T>`:

1. Update the autocomplete repository interfaces and implementations, then
   `IAutocompleteService` and all four service implementations.
2. Update `IArtistHeaderRepository`, `IVenueHeaderRepository`, `IConcertHeaderRepository`, their
   implementations, `IHeaderService`, `IConcertHeaderService`, the three header services,
   `IHeaderDispatcher`, and `HeaderDispatcher`.
3. Keep each query's existing `ToListAsync()` terminal and ordering/take/filter behavior. Do not
   introduce a second materialization solely to satisfy the new type.
4. Leave controller action types, `IPagination<T>`, header DTO genre properties, projection selectors,
   nullable inputs, and factory exception behavior untouched.

Verification gate:

- `dotnet build api/Concertable.slnx --configuration Release` completes with 0 errors.
- `dotnet test api/Concertable.Search/tests/Concertable.Search.UnitTests/Concertable.Search.UnitTests.csproj --configuration Release` passes.
- Run the Search integration project through the `integration-debug` workflow; the full project must
  pass, including the existing empty-list and populated-result API cases.
- Re-run the production inventory and confirm no operation method in Application interfaces/services,
  `HeaderDispatcher`, or Infrastructure repositories returns `IEnumerable<T>`.
- Update this plan and the progress ledger, then commit the green phase before stopping.

## Phase 2 — Add Search-owned contract enforcement and final local gates

Add `Architecture/ContractArchitectureTests.cs` to `Concertable.Search.UnitTests`:

1. Reflect over declared operation methods in Search Application interfaces/services,
   `HeaderDispatcher`, and Infrastructure repositories. Unwrap `Task<T>` and require a collection
   payload to be declared as `IReadOnlyList<T>`; allow `IPagination<T>` as its already-read-only
   paginated contract.
2. Cover the guard itself with representative allowed and rejected return-type cases so a broken
   detector cannot silently green.
3. Keep the enforcement Search-owned. Do not add a temporary repository-wide allowlist to
   `Concertable.Shared.Api.UnitTests.TypedResultArchitectureTests`; the existing shared suite governs
   the owned functional foundation and terminals, while unfinished service migrations make a global
   collection rule premature.
4. Do not duplicate integration tests whose empty and populated behavior is already explicit. Add an
   API case only if implementation changes behavior or reveals a previously uncovered endpoint branch.

Verification gate:

- Run the new architecture test directly, then the full Search unit test project in Release.
- Run `dotnet build api/Concertable.slnx --configuration Release` to 0 errors.
- Run the Search integration project through the `integration-debug` workflow to green.
- After the final implementation commit, reproduce the committed Search carve from `HEAD`: extract
  `git archive HEAD:api/Concertable.Search` into a new temporary directory, create `CarveSearch.slnx`,
  add the Web, Workers, Api, Application, Infrastructure, Domain, and Seed.Infrastructure projects,
  and run `dotnet build CarveSearch.slnx --configuration Release` from the carved directory.
- Confirm the committed Search production inventory contains no third-party Result carrier, no owned
  functional carrier introduced without a documented caller-actionable failure, and no operation
  `IEnumerable<T>` return in the normalized namespaces.
- Update this plan and the progress ledger, then commit the green phase before review.

## Delivery and lifecycle gates

1. Run `/code-review` over the complete implementation range and resolve every open finding. Later
   code commits require `/incremental-review`.
2. Reconcile the branch with current `origin/main` while clean, rebuild/retest affected scope if it
   moved, push, and open the GitHub PR.
3. Let the merge workflow select the merge-queue E2E tier from the final diff. If the diff remains the
   isolated in-process Search signature normalization and Search-owned enforcement described here, it
   should satisfy the strict `skip-e2e` criteria; any runtime/wire expansion or broader refactor uses
   full merge-queue E2E. Never run duplicate local E2E before the PR.
4. Keep this plan and ledger until the PR, checks, merge, and any generated platform-sync PR are
   terminal and green. Record those outcomes before close-out.
5. In the shipping change, mark the parent Search item complete. Never delete the epic tracker.
6. Delete this plan and its progress ledger together only after the complete feature lifecycle is
   terminal, following the plan close-out rules.

## Definition of done

- Every Search operation collection contract in the audited repository/application/service/dispatcher
  chain returns `IReadOnlyList<T>` and every no-match path returns an empty list.
- `IPagination<T>`, DTO/projection/event boundaries, nullable inputs, controller/wire shapes, and
  exception semantics are unchanged.
- No Result, Option, operation error, third-party functional carrier, shared Kernel change, or
  cross-service runtime/source dependency was introduced without a newly evidenced requirement and an
  explicit plan revision.
- Search-owned architecture enforcement prevents `IEnumerable<T>` operation returns from returning.
- Release solution build, Search unit tests, Search integration tests, the committed standalone carve,
  review, PR checks, merge queue, and any platform-sync gate are green and recorded in the ledger.
