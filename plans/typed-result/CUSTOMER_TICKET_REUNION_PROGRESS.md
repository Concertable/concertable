# Customer Ticket Reunion migration progress

- Plan: `plans/typed-result/CUSTOMER_TICKET_REUNION_PLAN.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Feature\typed-result_customer-ticket-reunion`
- Branch: `Feature/typed-result_customer-ticket-reunion`
- PR: not opened; historical PR #282 remains open and untouched
- Dependency/package gates: validator implementation reopened against merged Reunion.Validation
  source; Reunion.Errors `.2`, Reunion.Validation `.1`, Payment publication, and generated platform
  sync gate delivery and final clean-feed revalidation
- Last reconciled: 2026-08-10 against `origin/main` `6f4a5cc3e`, branch merge `103da45a7`, Payment
  package source `a2497e3e8`, Reunion source `1500270`, implementation commit `acaec615b`, the active
  Reunion integration owner, and historical PR #282 head `26ed63b8`

## Current state

The replacement implements the unique PR #282 Ticket purchase, checkout, eligibility, and payment
semantics on current main without carrying forward its obsolete CFE or Shared.Api terminal design.
Historical PR #282 remains open and untouched at `26ed63b8`. Customer owns three Reunion error
unions and maps them only at the Ticket controller. Missing concerts remain `404`, invalid
purchase/checkout requests remain structured `400` responses, a rejected payment remains
`ticket.payment_rejected` / `402`, richer non-rejection Payment errors retain their producer
definitions, and a missing concert during asynchronous payment completion remains a consistency
exception.

The producer advanced after this worktree opened. The final local gate therefore uses package source
commit `a2497e3e8a4f81ab550d564d0353c0683e4e44ec` and exact artifacts:

- `Concertable.Payment.Contracts` `0.1.0-alpha.0.915`, SHA-256
  `C3E6BBF9B3FEC6BC63F57873A38D29C8ACAAA0C8C03205B74751BA09A7D2561B`;
- `Concertable.Payment.Client` `0.1.0-alpha.0.915`, SHA-256
  `C2EA7EA87E3A5341389C055CA662FB1FDD2B8A18516AEC957631BE70999B2DE5`;
- `Reunion.Errors` `0.1.0-alpha.2`, SHA-256
  `16DDA3B382D696DD2F789C1FF4EE7CA6F36A1367AE57871B432C45EDD63D3DF4`.

The `.915` manifests require Reunion `.1` and Reunion.Errors `.2`. The replacement uses the `.2`
direct error-definition factories and has restored the normal platform Payment pins and repository
NuGet sources; no local feed or disposable Payment version remains in source configuration.
Implementation commit `acaec615b` passed native, security, and every Concertable review lens with no
findings. The branch has not been pushed and no PR has been opened.

The Ticket DI validator is not yet on Reunion's validation-specific carrier. `ITicketValidator`
currently returns `bool`, `Result<bool, EligibilityError>`, and
`UnitResult<IReadOnlyList<string>>`. Reunion source commit
`a837ecb60416c44eda1c87b0c858a9be7658c74b`, contained in current merged source
`1500270cc323fe43b9eaf57dad9698b24f6dfb37`, provides the intended
`Reunion.Validation.ValidationResult = Valid | Invalid(ValidationErrors)` contract, accumulation,
and explicit Result conversions. `Reunion.Validation` `0.1.0-alpha.1` is not yet published;
NuGet.org's official flat-container index returned HTTP 404 on 2026-08-10.

## Next Steps

Pack and inspect exact `Reunion.Validation` `0.1.0-alpha.1` from merged Reunion source `1500270`,
record its immutable hash and dependencies, and expose it only through a temporary local feed. Add
direct package ownership wherever Ticket source or tests use the validation API, then change
`ITicketValidator` / `TicketValidator` so synchronous validation returns `ValidationResult` and
concert lookup returns `Result<ValidationResult, EligibilityError>`. Map invalid results into the
existing `PurchaseError.Invalid` / `CheckoutError.Invalid` contracts without changing their public
field keys, update unit and integration coverage, and run Ticket unit/integration, Shared.Api
architecture, Customer/full Release builds, standalone Customer carve, legacy-carrier scans,
`git diff --check`, and complete code/security review. Restore every temporary feed input before the
commit, then stop delivery-ready behind publication and platform sync; do not push or mutate PR #282.

## Completed work

- Established one replacement owner and preserved PR #282 as read-only historical input.
- Produced and inspected exact local Payment package artifacts with immutable source and hashes.
- Audited PR #282 and preserved its unique observable error codes, status codes, validation details,
  payment rejection behavior, asynchronous completion invariant, and covering tests.
- Migrated Ticket purchase, checkout, and eligibility to direct Reunion carriers and service-owned
  MVC terminals; removed Customer's final FluentResults references.
- Reconciled the implementation from superseded Payment `.911` / Reunion.Errors `.1` to current
  Payment `.915` / Reunion.Errors `.2` before checkpointing it.
- Committed locally as `acaec615b` and completed native, security, correctness, isolation, boundary,
  seeding, convention, and changed-path test-coverage review with no findings.

## Verification

- Package manifests and SHA-256 hashes verified from the stable local feed.
- Search audit found no separate Search Reunion migration work.
- Ticket unit tests: 33 passed; Ticket integration tests: 25 passed; Shared.Api architecture tests:
  52 passed.
- Customer Release build: 0 warnings, 0 errors; full Release solution build: 0 errors and 2 existing
  generated E2E nullable warnings.
- Standalone Customer deployable-closure carve: 0 errors and 1 existing UserEntity warning.
- `git diff --check` and legacy carrier/terminal scans pass; normal Payment pins and NuGet sources are
  restored after verification.
- Code/security review range `1043a9178..acaec615b`: one implementation commit, no findings.

## Decisions, discoveries, blockers, and deviations

- The historical branch is semantic input, not the implementation base.
- Payment publication gates delivery only; exact artifacts opened and completed local implementation.
- The producer's move to Reunion.Errors `.2` removed `ErrorDefinition.For<TError>()`; reconciling to
  direct nested-case factories now avoids landing a replacement against another obsolete rehearsal.
- `ValidationResult` is the correct validator boundary: it permanently fixes the invalid payload to
  structured `ValidationErrors` and supports accumulation. Missing concert remains a separate
  `EligibilityError`, so the async lookup contract becomes
  `Result<ValidationResult, EligibilityError>` rather than misclassifying absence as validation.
- Existing `purchase` and `checkout` validation field keys are observable wire contracts and remain
  stable when the validator's internal carrier changes.

## Event log

### 2026-08-10 â€” Reunion.Validation phase added

- Action: Audited the completed Ticket validator contracts against Reunion's merged validation
  package and added a fifth implementation phase.
- Evidence: current `ITicketValidator` signatures; Reunion commits `a837ecb` / `1500270`; package
  project version `0.1.0-alpha.1`; official NuGet.org flat-container response HTTP 404; clean branch
  merged to current `origin/main` `6f4a5cc3e` as `103da45a7`.
- Outcome: The earlier implementation is not the final validator design. Local preparation is
  actionable from exact merged source even though Reunion.Validation publication still gates
  delivery.
- Follow-up: execute the new `## Next Steps` through a verified, reviewed local Phase 5 checkpoint.

### 2026-08-09 â€” implementation checkpoint reviewed delivery-ready

- Action: Committed the verified source and reviewed the exact one-commit branch delta through the
  native, security, and Concertable architecture-aware lenses.
- Evidence: implementation `acaec615b`; review range `1043a9178..acaec615b`; no findings; no local
  feed, disposable Payment pin, push, new PR, or PR #282 mutation.
- Outcome: All local phases are complete and reviewed. Only the external publication, Payment merge,
  generated sync, and clean-feed revalidation lifecycle remains.
- Follow-up: the Reunion integration owner clears the exact blocker in `## Next Steps` and updates
  this ledger when the normal-feed resume condition is true.

### 2026-08-09 â€” current producer gate reconciled and implementation verified

- Action: Audited PR #282, migrated Customer Ticket to Reunion, then reconciled from recorded Payment
  `.911` to the active producer's `.915` packages and Reunion.Errors `.2` candidate.
- Evidence: immutable hashes and source commit above; unit 33/33; integration 25/25; architecture
  52/52; Customer and full Release builds at 0 errors; standalone Customer carve at 0 errors;
  temporary restore inputs removed.
- Outcome: The replacement is locally complete against the current producer API and preserves the
  historical branch's unique wire semantics without its obsolete carrier/terminal mechanics.
- Follow-up: commit the checkpoint and complete code/security review before entering the publication
  blocker.

### 2026-08-09 — replacement worktree opened

- Action: Created the reserved branch and worktree from fresh `origin/main`.
- Evidence: Clean `Feature/typed-result_customer-ticket-reunion` at `1043a9178`, zero commits ahead or
  behind `origin/main`; PR #282 remains open at `26ed63b8`; both `.911` package hashes reverified.
- Outcome: The replacement owner is active and its exact implementation inputs remain valid.
- Follow-up: audit PR #282 and implement the replacement through the delivery-ready gate.

### 2026-08-09 — replacement workstream made parallel-ready

- Action: Separated implementation from delivery and reserved the replacement owner.
- Evidence: PR #282 inventory; Payment package provenance above.
- Outcome: Customer Ticket can be implemented now without mutating the historical PR.
- Follow-up: execute `## Next Steps` in the reserved worktree.
