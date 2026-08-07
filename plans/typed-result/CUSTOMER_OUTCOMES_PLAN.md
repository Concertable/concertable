# Customer non-Payment outcomes and lookups

> Roadmap item: **Customer non-Payment outcomes and lookups** in
> [`TYPED_RESULT_MIGRATION_ROADMAP.md`](./TYPED_RESULT_MIGRATION_ROADMAP.md).
>
> **Next steps live in @plans/typed-result/CUSTOMER_OUTCOMES_PROGRESS.md → `## Next Steps`.**

## Objective

Migrate the Customer-owned Review, Preference, User, Venue, and Artist in-process contracts to the
smallest owned functional shape that represents their real outcomes:

- operation-specific `Result<TValue, TError>` only for expected failures a caller can act on;
- `Option<T>` for ordinary absence at application and module boundaries;
- successful empty `IReadOnlyList<T>` values for collection queries;
- plain values and capability booleans where no actionable failure or absence exists.

Repository single-item lookups remain nullable. Infrastructure, cancellation, violated-invariant,
and identity-claim failures remain exceptions. Functional types stop at the HTTP/module terminal and
never enter HTTP DTOs, integration events, EF models, or other wire/persistence contracts.

## Evidence baseline

The branch was created from fresh `origin/main` and reconciled to `e419966a9` before handoff. The plan
commit is the sole branch commit above that base. The scoped production inventory on that base is:

| Area | Current in-process contract/problem | Planned contract |
|---|---|---|
| Review | `IConcertReviewService.CreateAsync` throws `NotFoundException` for no ticket; eligibility is a boolean and creation does not reject a future or already-reviewed ticket before the unique constraint | `Result<ReviewDto, CreateReviewError>` with caller-safe not-found/conflict outcomes; capability endpoints remain booleans |
| Preference | nullable `GetByUser*`, lazy `IEnumerable` collections, `OrNotFound`, ownership exception, and a unique-per-user create invariant exposed only by the database | `Option<PreferenceDto>`, materialized `IReadOnlyList<T>`, `Result<PreferenceDto, CreatePreferenceError>`, and `Result<PreferenceDto, UpdatePreferenceError>` |
| User | nullable `GetMeAsync`; `IUserModule` and repository collections return `IReadOnlyCollection<T>` | `Option<CustomerDto>` and successful empty `IReadOnlyList<T>`; `SaveLocationAsync` stays a plain success because its missing-row path is an invariant behind the `Customer` authorization policy |
| Venue | nullable repository and service detail lookup; controller maps null to 404 | nullable repository contract preserved; service returns `Option<VenueDetails>` and the controller preserves 200/404 |
| Artist | nullable repository and service detail lookup; controller maps null to 404 | nullable repository contract preserved; service returns `Option<ArtistDetails>` and the controller preserves 200/404 |

Review pagination and summaries already model successful empty results correctly. Eligibility methods
are capability queries whose complete caller decision is boolean; wrapping them in `Result` would
manufacture an outcome the caller does not need. Preference collection inputs remain `IEnumerable<T>`;
only returned query results are normalized.

There are existing Review and User unit/integration projects. Preference, Venue, and Artist have
friend-assembly names reserved but no test projects or HTTP coverage. The shared Customer integration
fixture already registers their production modules and test seeders, so each missing module can own
normal unit and HTTP integration projects without a new cross-module fixture.

## Ownership and isolation

This plan owns only:

- `api/Concertable.Customer/src/Modules/{Review,Preference,User,Venue,Artist}/**`;
- new tests owned by those five modules;
- the Customer/root solution and `scripts/integration.ps1` entries needed to discover those new test
  projects;
- this plan, its ledger, and the roadmap lifecycle tick after delivery is terminal.

PR #282 / `Feature/TypedResultMigrationPhase2` exclusively owns Customer Ticket, Concert, Customer
Payment clients and mocks, purchase/checkout flows, and their coverage. This plan must not edit those
modules or transplant their local-only changes. Review may consume the existing `ITicketModule`
contract and mock it in Review-owned tests, but must adapt its nullable result at the Review boundary;
it must not change `ITicketModule`, `TicketSummary`, Ticket tests, or Concert tests.

Also out of scope:

- shared Kernel or Shared.Api API changes;
- cross-service runtime references or changes to integration-event payloads/handlers;
- persistence/model changes and migrations;
- Customer-wide FluentResults cleanup.

`FluentResults` remains versioned in `api/Concertable.Customer/Directory.Packages.props` while the
Ticket and Concert owner still consumes it. None of the five scoped production modules currently
references FluentResults, so this work adds no replacement carrier and removes no shared package
entry.

## Contract design

### Review

- Add `CreateReviewError` beside the create operation as a Dunet union with named payload-free cases,
  one exhaustive definition switch, and exact contract tests:
  - ticket absent → `NotFound`, code `review.ticket_not_found`;
  - concert not yet reviewable → `Conflict`, code `review.concert_not_reviewable_yet`;
  - ticket already reviewed → `Conflict`, code `review.already_exists`.
- Change `IConcertReviewService.CreateAsync` to
  `Task<Result<ReviewDto, CreateReviewError>>`.
- Reshape `IReviewValidator` so one Ticket lookup returns
  `Result<TicketSummary, CreateReviewError>` for the create path. Reuse that evaluation to collapse
  the concert eligibility endpoint to its existing boolean instead of duplicating policy or querying
  Ticket twice. The Review Application project may reference the existing Ticket Contracts project;
  no Ticket-owned file changes.
- Keep absent authentication/identity claims and repository/database races on their existing exception
  paths. Do not catch unique-index, cancellation, or provider failures into `CreateReviewError`.
- Terminate create through `Concertable.Shared.Api.Results` as the existing 201 response, with typed
  ProblemDetails for the expected failures. Leave review DTOs, pagination, summaries, and
  `CustomerReviewSubmittedEvent` unchanged.

### Preference

- Add a payload-free `CreatePreferenceError.PreferenceAlreadyExists` union case (`Conflict`,
  `preference.already_exists`) and `UpdatePreferenceError` union cases for a missing preference
  (`NotFound`, `preference.not_found`) and a preference owned by another user (`Forbidden`,
  `preference.not_owned`). Keep the database unique index authoritative; the normal pre-existing
  preference path becomes the typed conflict, while a provider/race failure still propagates.
- Change `GetByUserIdAsync` and `GetByUserAsync` to `Option<PreferenceDto>` by applying `ToOption()`
  after the nullable repository call.
- Change `GetAsync`, `GetUserIdsByLocationAndGenresAsync`, the extra matching-genres repository query,
  and the collection mapper to materialized `IReadOnlyList<T>` results. Keep the inherited repository
  `GetAllAsync` shape required by the published data-access base and materialize at the service edge.
- Change create/update to their operation-owned Results. Return the tracked updated entity after a
  successful save instead of performing a nullable re-read and using `!`.
- Preserve HTTP shapes: create remains 201 with no response body, update remains 200 with the DTO,
  and no current preference remains the existing 204 representation. Typed failures use the shared
  ProblemDetails terminal. `ConcertPostedEvent` and notification delivery remain exception-based wire
  processing and consume the new empty list normally.

### User

- Change `IUserService.GetMeAsync` to `Option<CustomerDto>` after the nullable repository lookup.
- Change `IUserRepository.GetByIdsAsync`, public in-process `IUserModule.GetByIdsAsync`, and
  `UserModule` to `IReadOnlyList<T>` with `[]` for no users. Preference is the only Customer consumer;
  `UserClaimsController` continues returning an empty claim array for no match.
- Keep `SaveLocationAsync` plain. A missing row after the `Customer` authorization handler has
  succeeded is a violated invariant/race, not a new caller-actionable Result case; geocoding,
  cancellation, and persistence faults continue to propagate.
- Preserve `/api/user/me` and `/api/user/location` behavior: unauthenticated 401, unknown Customer 403
  at authorization, successful DTO responses unchanged. Functional values remain inside the host.

### Venue and Artist

- Keep `IVenueReadRepository.GetDetailsByIdAsync` and
  `IArtistReadRepository.GetDetailsByIdAsync` nullable as persistence contracts.
- Change only `IVenueService`/`IArtistService` and their implementations to
  `Option<VenueDetails>` / `Option<ArtistDetails>` using `ToOption()`.
- Match the Options in the controllers and map DTOs to the existing dedicated detail Responses only
  in the Some arm. Preserve anonymous 200 and missing 404 responses exactly; do not invent an error
  union for ordinary projection absence.
- Leave B2B projection events, handlers, replica entities, and seeding unchanged.

## Test design

Use the existing Review/User projects and add module-owned UnitTests and IntegrationTests projects for
Preference, Venue, and Artist. Add all new projects to both `api/Concertable.Customer/Concertable.Customer.slnx`
and `api/Concertable.slnx`, add the three integration projects to `scripts/integration.ps1`, and add
the missing Preference friend-assembly entries. Give each new test project the standard local
`AGENTS.md`/`CLAUDE.md` pointer to `UNIT_CONVENTIONS.md` or `INTEGRATION_CONVENTIONS.md`. Do not put
these cases in another module's test project.

Coverage required by the phases:

- exact code/message/kind tests for every Review and Preference error value;
- Review service/validator tests for missing ticket, not-yet-reviewable concert, existing review,
  success, boolean eligibility collapse, and propagation of thrown collaborator/cancellation faults;
- Review HTTP tests for the same expected status/code outcomes plus unchanged 201 and eligibility
  behavior;
- Preference unit tests for Some/None, duplicate create, missing/foreign/successful update, eager empty
  lists, and no swallowed collaborator failures; HTTP tests for 200/204 reads, 201 create, 409 duplicate,
  404 missing update, 403 foreign update, and 200 successful update;
- User service/module tests for Some/None and populated/empty lists, plus existing HTTP authorization
  and success cases;
- Venue and Artist service tests for Some/None and HTTP tests for seeded 200 and absent 404;
- the repository-wide Shared.Api typed-result architecture suite after every Result-bearing phase.

Tests use the established Customer `ApiFixture`, seed state, per-module test projects, and normal
integration-debug workflow. They do not change seed data or write replica tables outside the existing
test-seeder path.

## Verification contract for every phase

Every implementation phase ends with all of the following green against that phase's candidate tree:

1. `dotnet build api/Concertable.slnx --configuration Release` with 0 errors.
2. The affected module UnitTests project(s).
3. The affected module IntegrationTests project(s), run through the `integration-debug` skill and
   `scripts/integration.ps1`; any red run stays in that debug workflow until green.
4. `dotnet test api/Concertable.Shared/tests/Concertable.Shared.Api.UnitTests/Concertable.Shared.Api.UnitTests.csproj --configuration Release` for the repository-wide typed-result architecture checks.
5. The CI-equivalent `carve-customer` recipe from `.github/workflows/test.yml`, building every
   non-test/non-AppHost project from an isolated `api/Concertable.Customer` tree with package restore
   only and `-p:MinVerSkip=true`.
6. Scoped inventories confirming functional carriers stop before HTTP/events/persistence, repository
   nullability is the only allowed single-item nullable return, returned collections are
   `IReadOnlyList<T>`, and no scoped production file introduces FluentResults, Dunet, HTTP exceptions
   in typed-result slices, or Ticket/Concert/Payment edits.

No phase changes the EF model, so `api/initial-migrations.ps1` is not required. Local E2E is not part
of a pre-PR phase gate; the final behavior-changing, multi-module PR requires the full merge-queue E2E
tier and receives no skip label.

## Phases

### Phase 1 — Review create outcomes ✅ DONE (2026-08-05)

- Add `CreateReviewError`, reshape the create/eligibility contracts, compose the existing nullable
  Ticket module result into the Review-owned Result, and terminate it at `ConcertReviewsController`.
- Add exact error, service/validator, and HTTP coverage without editing any Ticket/Concert-owned file.
- Run the full per-phase verification contract and commit the phase with its plan/ledger checkpoint.

### Phase 2 — Preference outcomes, Options, and lists ✅ DONE (2026-08-05)

- Add Preference create/update error contracts and Results.
- Convert user-preference absence to Option, collection outputs to `IReadOnlyList<T>`, and make the
  204/201/200 HTTP terminals explicit without changing their wire payloads.
- Create Preference-owned UnitTests/IntegrationTests, wire their solution/script discovery, run the
  full per-phase verification contract, and commit the checkpoint.

### Phase 3 — User Option and module-list normalization ✅ DONE (2026-08-06)

- Convert `GetMeAsync` to Option and `GetByIdsAsync` repository/module contracts to
  `IReadOnlyList<T>`; update Preference and UserClaims consumers without changing their wire behavior.
- Extend User unit/integration coverage for Some/None, empty lists, authorization, and successful
  location/profile flows.
- Run the full per-phase verification contract and commit the checkpoint.

### Phase 4 — Venue and Artist detail Options ✅ DONE (2026-08-07)

- Reconcile the Review and Preference operation errors plus the Shared.Api architecture guard with
  the current typed-error union, definition-switch, generic-factory, and case-construction conventions.
- Convert both application services to Option while preserving nullable repositories and public
  200/404 detail responses.
- Create Venue- and Artist-owned UnitTests/IntegrationTests, wire their solution/script discovery,
  and cover Some/None plus 200/404.
- Run the full per-phase verification contract, including the affected Review and Preference unit
  suites, and commit the checkpoint.

### Phase 5 — Scope audit and delivery

- Run the five-module nullable/collection/carrier inventories and the combined Review, Preference,
  User, Venue, Artist, Shared.Api architecture, Release solution, and Customer carve gates.
- Confirm the diff contains no Ticket, Concert, Customer Payment client/mock, checkout/purchase,
  shared Kernel API, event-contract, model/migration, or FluentResults package-entry change.
- Complete `/code-review`, address every fixable finding in separate commits with incremental review,
  then push/open the PR only under the active delivery instruction.
- Before merge, update the branch to current `origin/main`, rebuild/retest affected areas, require the
  full merge-queue E2E tier, and follow the generated platform-sync PR to green/merged. A red sync is
  part of this feature's delivery and must be fixed before close-out.
- Only after the feature PR and its publication/platform-sync lifecycle are terminal: record the final
  evidence in the ledger, tick **Customer non-Payment outcomes and lookups** in
  `TYPED_RESULT_MIGRATION_ROADMAP.md`, and never delete the roadmap. Delete this plan and ledger
  together in the following close-out change under the repository lifecycle rules.

## Definition of done

- Review create and Preference create/update expose only operation-owned expected failures with exact,
  stable definitions and central HTTP terminals.
- All ordinary single-item application/module absence in the five scoped modules is `Option<T>`;
  persistence lookup contracts remain nullable.
- All scoped returned collection contracts are successful empty `IReadOnlyList<T>` values, except
  inherited published repository-base members and pagination abstractions.
- Existing HTTP DTOs/status semantics, integration events, projection handlers, EF models, exception
  propagation, and Customer's standalone package closure remain intact.
- The scoped unit/integration suites, Shared.Api architecture suite, Release solution build, and
  Customer carve are green, and full merge-queue E2E plus the generated platform sync complete.
- PR #282's exclusive Ticket/Concert/Payment slice remains untouched.
- The roadmap line is checked only after the complete delivery lifecycle ships; the roadmap itself is
  retained permanently.
