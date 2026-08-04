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
published typed Payment client and are **blocked** until the Payment owned-result expansion (PR #296)
merges, Payment publishes, and its platform-sync PR lands green — no FluentResults adapter, string
bridge, or local source dependency may be introduced to cross that gate.

- [x] **Checkpoint 1 — Deal.** Deal module outcomes → owned Results; Deal keeps Dunet only for its
  structured validation variants (`Invalid`, `DealNotFound`) with an abstract root `Definition` and
  per-case overrides; published codes pinned by contract tests.
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
- Payload-free errors are sealed definition records; Dunet is retained only where alternatives carry
  data or need runtime case discrimination (Deal validation), with `Definition` abstract on the root
  and overridden per case. Cases use natural domain names, not `Case`-suffixed aliases.
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
published and platform-synced green (owned by PR #296). Do not bridge the package gate.
