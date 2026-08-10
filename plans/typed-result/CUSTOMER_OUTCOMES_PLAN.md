# Customer non-Payment outcomes and lookups

> **Next steps live in @plans/typed-result/CUSTOMER_OUTCOMES_PROGRESS.md → `## Next Steps`.**

## Objective

Migrate the Customer-owned Review, Preference, User, Venue, and Artist in-process contracts to the
smallest Reunion-backed functional shape that represents their real outcomes:

- operation-specific `Result<TValue, TError>` only for expected failures a caller can act on;
- `ValidationResult` for validation-only contracts implemented behind dependency injection;
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
| Review | `IConcertReviewService.CreateAsync` throws `NotFoundException` for no ticket; eligibility is a boolean and creation does not reject a future or already-reviewed ticket before the unique constraint | `Result<ReviewDto, CreateReviewError>` with caller-safe not-found/conflict outcomes; the injected validator returns `ValidationResult`; capability endpoints remain booleans |
| Preference | nullable `GetByUser*`, lazy `IEnumerable` collections, `OrNotFound`, ownership exception, and a unique-per-user create invariant exposed only by the database | `Option<PreferenceDto>`, materialized `IReadOnlyList<T>`, `Result<PreferenceDto, CreatePreferenceError>`, and `Result<PreferenceDto, UpdatePreferenceError>` |
| User | nullable `GetMeAsync`; `IUserModule` and repository collections return `IReadOnlyCollection<T>` | `Option<CustomerDto>` and successful empty `IReadOnlyList<T>`; `SaveLocationAsync` stays a plain success because its missing-row path is an invariant behind the `Customer` authorization policy |
| Venue | nullable repository and service detail lookup; controller maps null to 404 | nullable repository contract preserved; service returns `Option<VenueDetails>` and the controller preserves 200/404 |
| Artist | nullable repository and service detail lookup; controller maps null to 404 | nullable repository contract preserved; service returns `Option<ArtistDetails>` and the controller preserves 200/404 |

Review pagination and summaries already model successful empty results correctly. Eligibility service
methods are capability queries whose complete caller decision is boolean, but their injected
validation dependency uses Reunion's validation-specific `Valid | Invalid(ValidationErrors)` shape
and the service reduces it to the public boolean. Preference collection inputs remain
`IEnumerable<T>`; only returned query results are normalized.

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

- shared Kernel or Shared.Api source/API changes or local carrier/terminal implementations;
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
  - ticket already reviewed → `Conflict`, code `review.already_exists`;
  - stars outside 1–5 → `Invalid(ValidationErrors)`, code `review.invalid`, preserving the `Stars`
    field and caller-safe range message.
- Change `IConcertReviewService.CreateAsync` to
  `Task<Result<ReviewDto, CreateReviewError>>`.
- Reshape every `IReviewValidator` validation method to return `Reunion.Validation.ValidationResult`.
  `ConcertReviewService` owns the single `ITicketModule.GetByUserAndConcertAsync` lookup, maps null to
  `CreateReviewError.TicketNotFound`, then calls distinct review-period and already-reviewed validator
  methods in the existing short-circuit order. Because each validation call owns one rule, the service
  maps an invalid result directly to the existing `ConcertNotReviewableYet` or `ReviewAlreadyExists`
  case without parsing human messages. Reuse that private service evaluation for create and concert
  eligibility so Ticket is still queried once per operation.
- Return `ValidationResult` from the Artist and Venue validator methods too; their services reduce
  `IsValid` to the existing capability booleans. Pin the structured field/message payloads in unit
  tests even though these eligibility endpoints intentionally expose only true/false.
- Add direct `Reunion.Validation` ownership to every compiling Review project whose source names its
  API, and add its exact version to Customer central package management. FluentValidation request
  validators, Duende protocol validators, and Microsoft `IValidateOptions<T>` are separate framework
  contracts and are not converted by this phase.
- Keep absent authentication/identity claims and repository/database races on their existing exception
  paths. Do not catch unique-index, cancellation, or provider failures into `CreateReviewError`.
- Change `ReviewEntity.Create` to return the typed validation outcome it owns. The request validator
  may retain wire-level range validation, but `ConcertReviewService` must map the domain result and
  must remain correct for direct/internal callers without an equivalent pre-check that exists only to
  avoid the current throwing guard. Other scoped entity mutations are unconditional; missing User
  immediately after its owning save remains an invariant fault.
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
- Review service/validator tests for exact Valid/Invalid structured payloads, missing ticket,
  not-yet-reviewable concert, existing review, success, one Ticket lookup, boolean eligibility
  collapse, rule-order short-circuiting, and propagation of thrown collaborator/cancellation faults;
- Review domain/service tests for out-of-range stars at `ReviewEntity.Create`, the operation-owned
  `review.invalid` definition, direct-service mapping without the HTTP validator, and no duplicate
  application range guard or caught invariant exception;
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
7. A scoped DI-validator inventory proving `IReviewValidator` exposes only `ValidationResult`-bearing
   validation methods, with FluentValidation/framework validators explicitly excluded.
8. A scoped production `DomainException` inventory proving Review's caller-actionable star rejection
   is typed at `ReviewEntity.Create`, no equivalent application pre-check shields a throwing domain
   guard, and invariant/infrastructure/cancellation faults still propagate and do not become public
   4xx contracts.

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

### Phase 5 — Scope audit and direct Reunion conversion ✅ LOCAL COMPLETE (2026-08-10)

- Reconcile with current main now and migrate only Customer-owned imports and HTTP-edge terminals to
  directly owned published Reunion packages. Audit the resulting package topology separately from the
  Payment delivery chain; this scope excludes Payment.Client and may be independently deliverable.
- Run the five-module nullable/collection/carrier inventories and the combined Review, Preference,
  User, Venue, Artist, Shared.Api architecture, Release solution, and Customer carve gates.
- Confirm the diff contains no Ticket, Concert, Customer Payment client/mock, checkout/purchase,
  shared Kernel API, event-contract, model/migration, or FluentResults package-entry change.
- Complete `/code-review`, address every fixable finding in separate commits with incremental review,
  and retain the verified local work until the validation follow-up below is complete.

### Phase 6 — Review DI validation results and delivery

- Publish and production-verify exact `Reunion.Validation` `0.1.0-alpha.1` from merged upstream
  validation source `a837ecb` (unchanged through `1500270`) before any Concertable commit depends on
  it. Require NuGet.org indexing, repository-signature verification, and a clean net10 package restore
  resolving its `Reunion`/`Reunion.Errors` dependency graph.
- Implement the Review contract design above: keep Ticket lookup/typed domain-error mapping in the
  service, make every custom DI validator method return `ValidationResult`, preserve the public
  booleans and exact create 201/400/404/409 ProblemDetails contracts, make `ReviewEntity.Create` own
  the typed star-range alternative, and make no Ticket-owned edit.
- Add direct package ownership, validator/service unit coverage, the scoped DI-validator inventory,
  and rerun Review integration plus the complete per-phase verification contract.
- Commit and review the phase, address every fixable finding with incremental review, then update PR
  #425 through the plan-managed two-leg push only after the branch is current with main.
- Before merge, update the branch to current `origin/main`, rebuild/retest affected areas, require the
  full merge-queue E2E tier, and follow the generated platform-sync PR to green/merged. A red sync is
  part of this feature's delivery and must be fixed before close-out.
- Only after the feature PR and its publication/platform-sync lifecycle are terminal: record the final
  evidence in the ledger, then delete this plan and ledger together in the following close-out change
  under the repository lifecycle rules.

## Definition of done

- Review create and Preference create/update expose only operation-owned expected failures with exact,
  stable definitions and the published Reunion-backed central HTTP terminals.
- Review star-range rejection originates as a typed domain-factory result and is mapped by the
  application service; request validation is not the only protection against the throwing guard.
- Every custom Review validator resolved through DI returns Reunion `ValidationResult`; application
  services map it to operation-owned Results or capability booleans without leaking it onto the wire.
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
