# B2B typed-result migration plan

Next steps live in @plans/typed-result/B2B_PROGRESS.md → `## Next Steps`.

Migrate every B2B service module from FluentResults + nullable lookups to the owned
`Concertable.Kernel` `Result` / `Option` / `UnitResult` / `ValidationErrors` vocabulary. One migration
branch (`Refactor/B2BTypedResultMigration`), delivered in checkpoints. Repository single-item lookups
stay nullable (a persistence concern); modules and application services convert absence with the
published Kernel `ToOption().OrFailure(...)` and expose typed Results; controllers only map successful
payloads and terminate typed Results.

## Checkpoints

Checkpoints 1–5 are Payment-independent and shipped on the branch. Checkpoints 6–7 consume the
published typed Payment client and are **blocked** until generated platform-sync PR #420 is repaired
and lands green — no FluentResults adapter, string bridge, or local source dependency may be
introduced to cross that gate. Payment implementation PR #392 has merged and published version
`0.1.0-alpha.0.853`, but the red sync means B2B cannot consume it yet.

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
- [ ] **Checkpoint 6 — Concert payment / cancel / finish workflows.** *Blocked on the published typed
  Payment client.* Migrate `IConcertWorkflowModule`, cancellation/completion dispatchers, and every
  keyed cancel / finish / accept / payment step to owned Results; compose Payment failures with
  `MapError` (no `BadRequestException(result.Errors)` bridge); `ConcertCompletionRunner` distinguishes
  expected deferral/refusal from retryable faults; remove catch-all conversions.
- [ ] **Checkpoint 7 — B2B FluentResults removal.** *Blocked with Checkpoint 6.* Remove FluentResults
  from the migrated B2B projects once their last local use is gone, after Payment is consumed through
  the owned typed client.

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

## Verification gate — every checkpoint

- `dotnet build api/Concertable.B2B/Concertable.B2B.slnx` and `api/Concertable.slnx` (Release), 0 errors;
- affected module unit + integration tests via the `integration-debug` skill;
- B2B architecture tests;
- final checkpoint: select the merge-queue E2E tier (full by default); do not duplicate the queue run
  locally.

## Dependency gate

Checkpoints 6–7 must not begin on a red platform pin or before the Payment owned-result client is
published and platform-synced green. Payment PR #392 is merged; generated platform-sync PR #420 owns
the remaining red gate. Do not bridge the package gate.
