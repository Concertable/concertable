# Code review — Refactor/mapper-naming

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `9f571bcbc5d7bc05ed5747999c00da7bc6d7c985`  `(2026-08-28)`
**Judgment:** `approved`

## Review pass — 2026-08-28 — full

**Candidate base:** `95134600526276eebecd63b2096928a9bb7b5f1e`
**Candidate head:** `9f571bcbc5d7bc05ed5747999c00da7bc6d7c985`
**Candidate branch:** `Refactor/mapper-naming`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:08d61d7d7099b08d6b78e23b02d41320dea8818cfc141fcec448d1b110c4521c` `(39 paths)`
**Candidate bundle:** `C:\Users\TommySeery\source\repos\Concertable` (live checkout at frozen head; no separate export materialized)
**Candidate bundle identity:** `sha256:08d61d7d7099b08d6b78e23b02d41320dea8818cfc141fcec448d1b110c4521c` (path-set digest; patch/tree hashes not separately computed)
**Work-order path:** `reviews/Refactor-mapper-naming.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

### Scope

Mechanical rename/reorganization, no intended behavior change:

- Nine static `XResponseMappers` extension classes (Api layer, pure Dto->Response mapping) renamed to
  `XMappers`, converted from legacy `this`-parameter extension methods to C# 14 `extension()` blocks.
- `SelfBillingAgreementResponseMapper` renamed to `SelfBillingAgreementMappers`, same conversion.
- `ApplicationResponseMapper`/`IApplicationResponseMapper` and `OpportunityResponseMapper`/
  `IOpportunityResponseMapper` (Api layer, DI-injected, depend on `IConcertWorkflowCapabilityRegistry`)
  renamed to `ApplicationMapper`/`IApplicationMapper` and `OpportunityMapper`/`IOpportunityMapper`.
- The rename in the prior bullet collided (`CS0104`) with the Application layer's own
  `IApplicationMapper`/`IOpportunityMapper` (Entity->Dto), globally `using`'d project-wide in the Api host.
  Resolved by moving those two interfaces out of the flat `Application/Interfaces/` folder into
  `Application/Mappers/`, alongside their existing implementations — matching the pre-existing
  `IPaymentAmountMapper`/`IUserMapper` precedent. Applied the same move to `IDealMapper`
  (`Concertable.B2B.Deal.Application`) and `ITransactionMapper` (`Concertable.Payment.Application`) for
  consistency.
- Consumer updates: `ApplicationController.cs`, `OpportunityController.cs`, Concert Api
  `ServiceCollectionExtensions.cs`, `DealService.cs`, the five Deal keyed-strategy mapper implementations,
  `ApplicationServiceEligibilityTests.cs`, `TransactionServiceTests.cs`.

### Verification

- `Concertable.B2B.Web`, `Concertable.Customer.Web`, `Concertable.Payment.Web` build with 0 errors against
  this exact diff.
- `Concertable.B2B.Concert.UnitTests`, `Concertable.B2B.Deal.UnitTests`, `Concertable.Payment.UnitTests`
  build with 0 errors.
- Independent review lens (`workflow-review-lens`) read every file in the diff plus the repo-wide reference
  surface and checked for: leftover `*ResponseMapper(s)` references, `extension()` conversion correctness
  (receiver types, nullability, visibility), mixed legacy/`extension()` style within one class, unused or
  missing `using`s from the interface moves, completeness of the `Interfaces/` -> `Mappers/` moves, and
  naming-convention consistency. No defects found.

### Findings

No findings. Pass approved.
