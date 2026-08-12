# B2B typed-result migration plan

Next steps live in these workstream ledgers:

- @plans/typed-result/B2B_PAYMENT_SAGA_PRODUCER_PROGRESS.md → `## Next Steps`
- @plans/typed-result/B2B_PROGRESS.md → `## Next Steps`

Migrate every B2B service module from FluentResults + nullable lookups to the shared Reunion-backed
`Result` / `Option` / `UnitResult` vocabulary. Custom validation contracts resolved through dependency
injection use `Reunion.Validation.ValidationResult`; FluentValidation request validators remain
framework contracts, and non-DI domain/entity validation is outside this validation checkpoint.
One migration branch (`Refactor/B2BTypedResultMigration`), delivered in checkpoints. Repository single-item lookups
stay nullable (a persistence concern); modules and application services convert absence with the
published functional surface and expose typed Results; controllers only map successful payloads and
terminate typed Results. The published Reunion alpha.2 baseline owns the carrier/package surface;
this service branch owns only B2B semantics and consumes that baseline.

## Checkpoints

Checkpoints 1–9 are implemented, committed, reconciled with current main, and incrementally reviewed.
Tommy authorized the durable SEC1 B2B + Payment saga/package cut-over on 2026-08-12. Checkpoint 10 is
split into independently deliverable producer and consumer workstreams because B2B compiles against
published Payment packages rather than Payment source. No FluentResults adapter, string bridge,
committed local source, feed path, or disposable package pin may be introduced.

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
- [x] **Checkpoint 8 — DI validation results.** Convert the custom DI-resolved
  `IApplicationValidator` and `IConcertValidator` validation-only contracts to
  `Reunion.Validation.ValidationResult = Valid | Invalid(ValidationErrors)`. Move resource lookup and
  operation-error mapping out of `IApplicationValidator` into the application service, reduce
  eligibility to the existing public booleans, and map invalid results into the existing operation
  errors without parsing messages or changing ProblemDetails field/message contracts. Add direct
  `Reunion.Validation` ownership to every compiling project that names its API. FluentValidation
  `AbstractValidator<T>` request validators and non-DI Deal/domain validation are explicitly excluded.
- [x] **Checkpoint 9 — domain-owned expected alternatives.** Reconcile the production domain guards
  that are already inside this branch's B2B semantic scope. `TenantInvitationEntity.Accept` and
  `Revoke` return operation-owned typed failures for the pending/expired alternatives and the tenant
  service maps them without duplicating the same checks. `ConcertEntity.DeclareDoorRevenue` owns the
  non-negative-revenue alternative and maps it into a stable `DeclareDoorRevenueError` case. Artist
  and Venue create/update plus Tenant legal/tax/address construction return structured validation for
  caller-supplied fields; services map those results instead of relying on request validators to avoid
  domain throws. Preserve exceptions for malformed geocoder/image/identity-provider output,
  invitation expiry after the pending query, `VatBreakdown` imbalance, and other impossible internal
  construction or consistency faults. Do not catch those invariant faults in Result combinators.
- [ ] **Checkpoint 10A — Payment saga contract and idempotent producer.** Add Payment-owned financial
  operation command and outcome contracts for capture, deposit, and refund. Payment handles commands
  through its own runtime, keys operation replay by B2B operation ID and booking, and publishes the
  same terminal outcome after retries without moving money twice. Expected caller-actionable
  refusals become explicit contract outcomes; infrastructure/cancellation faults remain exceptional.
  Consume the exact Reunion package artifact from producer commit `113be42` and use its implicit
  conversions and projected `ToOkOr` terminals without recreating its extensions in Concertable.
- [ ] **Checkpoint 10B — B2B durable lifecycle saga.** Persist acceptance/cancellation intent and its
  financial-operation state before money moves, stage the Payment command in the same transaction via
  the B2B outbox, and complete or fail the lifecycle only from Payment-owned outcome events. Reconcile
  pending operations in the B2B worker with the same operation ID. Cancellation requested before or
  after capture/deposit must converge to `Cancelled`; a deferred refund remains pending and retryable.
  Expose operation status through a typed HTTP contract and Reunion terminals without weakening any
  endpoint union to `IResult`.

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
- Use target-typed raw payload conversions only where success/error intent is unambiguous. Use exact
  named cases when payload types overlap or branch intent matters, and direct static factories when
  inference would obscure the owned error contract. Do not add conversion helpers or casts that hide
  ambiguity.
- Expected alternatives are owned by the domain method or factory that enforces them. HTTP request
  validation may reject the same malformed wire input, but application services remain correct when
  called directly and do not repeat an equivalent guard merely to avoid a `DomainException`.
- The repository-wide audit retains the B2B invariant inventory: Artist/Venue collaborator and
  identity-output guards; `TenantInvitationEntity.Expire` and provisioning-handler consistency;
  `VatBreakdown` arithmetic balance; and unconditional state mutations that expose no expected
  rejection today. Those faults must not become public 4xx contracts through blanket exception
  handling.

## Verification gate — every checkpoint

- `dotnet build api/Concertable.B2B/Concertable.B2B.slnx` and `api/Concertable.slnx` (Release), 0 errors;
- affected module unit + integration tests via the `integration-debug` skill;
- B2B architecture tests;
- Checkpoint 8: validator tests pin exact Valid/Invalid structured payloads; service and HTTP tests pin
  lookup/error mapping, capability booleans, rule accumulation/order, collaborator exception
  propagation, and unchanged validation ProblemDetails; a scoped inventory proves every custom
  DI-resolved validator returns `ValidationResult` while excluding framework validators;
- Checkpoint 9: domain tests pin each typed rejection at its owning method/factory; direct service
  tests prove mappings without HTTP validators; source/architecture inventories prove the named
  caller-actionable guards no longer throw `DomainException`, equivalent service pre-checks are gone,
  and the deferred invariant inventory still propagates exceptionally; HTTP tests pin unchanged
  stable codes, messages, structured fields, and ProblemDetails while invariant exceptions remain
  500-class faults;
- final checkpoint: select the merge-queue E2E tier mechanically via merge Step 4; do not duplicate
  the queue run locally.

## Dependency gate

The dependency gate is open. `Reunion`, `Reunion.Validation`, and `Reunion.Errors`
`0.1.0-alpha.2` are published, indexed, repository-signature and payload-provenance verified, and
clean-restored from NuGet.org with their published dependency graph. Checkpoint 8 uses only normal
configured feeds and published versions; temporary package inputs remain forbidden. Shared contraction
is downstream cleanup and does not block this branch's local alpha.2 implementation or verification.

## Checkpoint 10 package topology

- Producer layer: `Concertable.Payment.Contracts` owns the additive command/outcome wire contracts;
  `Concertable.Payment.Client` republishes against the same Payment package release but does not
  re-expose the saga types in a changed public surface.
- Consumer layer: B2B consumes `Concertable.Payment.Contracts` and `Concertable.Payment.Client` only
  as published packages. Customer consumes the same packages but needs no source migration because the
  saga surface is additive.
- Delivery DAG: Payment producer branch → Payment package publication → generated platform sync →
  B2B published-package revalidation and delivery. The exact local producer artifact may make B2B
  delivery-ready, but only the published package and generated sync can make it merge-ready.
- Implementation DAG: Payment producer and B2B consumer may be prepared independently. Temporary
  package versions and feeds are never committed; each ledger records the producer commit, package
  version, SHA-256 hashes, and reproducible artifact location used for local verification.
