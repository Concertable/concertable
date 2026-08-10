# B2B typed-result migration plan

Next steps live in @plans/typed-result/B2B_PROGRESS.md → `## Next Steps`.

Migrate every B2B service module from FluentResults + nullable lookups to the shared Reunion-backed
`Result` / `Option` / `UnitResult` vocabulary. Custom validation contracts resolved through dependency
injection use `Reunion.Validation.ValidationResult`; FluentValidation request validators remain
framework contracts, and non-DI domain/entity validation is outside this validation checkpoint.
One migration branch (`Refactor/B2BTypedResultMigration`), delivered in checkpoints. Repository single-item lookups
stay nullable (a persistence concern); modules and application services convert absence with the
published functional surface and expose typed Results; controllers only map successful payloads and
terminate typed Results. The Reunion integration plan owns the carrier/package substitution once;
this service branch owns only B2B semantics and consumes the integrated published baseline.

## Checkpoints

Checkpoints 1–7 are complete on the branch. Checkpoint 8 is actionable against published
`Reunion.Validation` `0.1.0-alpha.1` and the published Payment/platform baseline. No FluentResults
adapter, string bridge, committed local source, feed path, or disposable package pin may be introduced.

- [x] **Checkpoint 1 — Deal.** Deal module outcomes → owned Results; operation errors use explicit
  Dunet cases with disabled implicit conversions and one exhaustive root `Definition` switch;
  published codes, messages, kinds, and structured validation payloads pinned by contract tests.
- [x] **Checkpoint 2 — Tenant.** Invitation, membership, tenant, tax-compliance, and current-tenant
  operations; expected not-found / conflict / invalid / forbidden become operation-specific Results;
  "missing immediately after this operation saved it" stays an invariant fault; framework
  authorization and infrastructure/cancellation stay exceptional.
- [x] **Checkpoint 3 — Venue & Artist.** Create/update/ownership operations → operation-specific
  Results; current-tenant IDs/details and public single-item queries → Option; public list/search →
  empty `IReadOnlyList<T>`.
- [x] **Checkpoint 4 — User.** User module lookups → Option; multi-ID queries → read-only lists;
  remaining expected exceptions → operation-specific Results.
- [x] **Checkpoint 5 — Concert core (Payment-independent).** Apply / accept / reject / withdraw / draft
  and lifecycle-transition errors; `LifecycleStateMachine.Next` / `ILifecycleTransitioner` typed
  without catch/rethrow; dispatcher / executor / capability interfaces migrated as vertical slices;
  owner-concert action capabilities moved into `ConcertService` (no `TimeProvider` in any controller);
  keyed deal-strategy resolution preserved (no `DealType` switches, no service location).
- [x] **Checkpoint 6 — Concert payment / cancel / finish workflows.** Migrate `IConcertWorkflowModule`,
  cancellation/completion dispatchers, and every
  keyed cancel / finish / accept / payment step to owned Results; compose Payment failures with
  `MapError` (no `BadRequestException(result.Errors)` bridge); `ConcertCompletionRunner` distinguishes
  expected deferral/refusal from retryable faults; remove catch-all conversions.
- [x] **Checkpoint 7 — B2B FluentResults removal.** Remove FluentResults
  from the migrated B2B projects once their last local use is gone and every migrated signature uses
  the published Reunion-backed surface.
- [ ] **Checkpoint 8 — DI validation results.** Convert the custom DI-resolved
  `IApplicationValidator` and `IConcertValidator` validation-only contracts to
  `Reunion.Validation.ValidationResult = Valid | Invalid(ValidationErrors)`. Move resource lookup and
  operation-error mapping out of `IApplicationValidator` into the application service, reduce
  eligibility to the existing public booleans, and map invalid results into the existing operation
  errors without parsing messages or changing ProblemDetails field/message contracts. Add direct
  `Reunion.Validation` ownership to every compiling project that names its API. FluentValidation
  `AbstractValidator<T>` request validators and non-DI Deal/domain validation are explicitly excluded.

## Error and boundary rules

- Read-path errors are named by aggregate noun (`VenueError`, `ArtistError`, `DealError`,
  `ConcertError`, `ApplicationError`, `OpportunityError`, `ContractError`, `InvoiceError`); mutation
  errors keep a disambiguating verb prefix; alternate lookups name the missing key
  (`InvoiceError.ConcertNotFound(concertId)`). `VatCalculationError` drops the redundant `Get` prefix.
- Every operation-error root is a Dunet union with
  `[Union(EnableImplicitConversions = false)]`, explicit naturally named cases, and one exhaustive
  `Definition => this switch`. Call sites construct cases directly and cast explicitly to the root;
  no singleton factories, alias factories, abstract root definition, or per-case override remains.
- `[ErrorCode]` is reserved for preserving an already-published code; explicit messages remain where
  the derived default is not the existing contract. Contract tests pin every case's code, message,
  kind, and payload-bearing values.
- No B2B `*.Api` project depends on `Option`; no controller injects `TimeProvider`. Architecture guards
  enforce both.
- B2B consumes the published Kernel package only; a nullable-to-Result Kernel extension is not added
  here because it would violate the B2B-only package boundary.
- DI validators return the validation-specific Reunion carrier, not `UnitResult<ValidationErrors>` or
  operation errors. Application services own lookup absence, translate `Invalid.Errors` into their
  operation-specific cases, and preserve the current `application`, `totalTickets`, `booking`, and
  `datePosted` structured fields at the HTTP terminal.

## Verification gate — every checkpoint

- `dotnet build api/Concertable.B2B/Concertable.B2B.slnx` and `api/Concertable.slnx` (Release), 0 errors;
- affected module unit + integration tests via the `integration-debug` skill;
- B2B architecture tests;
- Checkpoint 8: validator tests pin exact Valid/Invalid structured payloads; service and HTTP tests pin
  lookup/error mapping, capability booleans, rule accumulation/order, collaborator exception
  propagation, and unchanged validation ProblemDetails; a scoped inventory proves every custom
  DI-resolved validator returns `ValidationResult` while excluding framework validators;
- final checkpoint: select the merge-queue E2E tier (full by default); do not duplicate the queue run
  locally.

## Dependency gate

The dependency gate is open. `Reunion.Validation` `0.1.0-alpha.1` is published, indexed,
repository-signature and payload-provenance verified, and clean-restored from NuGet.org with its
published Reunion dependency graph. Payment `0.1.0-alpha.0.894` and generated platform-sync PR #463
are terminal on the merged main baseline. Checkpoint 8 must use only normal configured feeds and the
published versions; temporary package inputs remain forbidden.
