# Code review — TechDebt/techdebt-run-sweep-20260829-215319

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `bf1533719d9e2af52ac4f64321e3a9e39a8a97d2`  `(2026-08-30)`
**Security-reviewed up to commit:** `bf1533719d9e2af52ac4f64321e3a9e39a8a97d2`  `(2026-08-30)`
**Judgment:** `approved`

## Review pass — 2026-08-30 — full

**Candidate base:** `c4451509fbfe2757955518a7f0a183af409d8aca`
**Candidate head:** `bf1533719d9e2af52ac4f64321e3a9e39a8a97d2`
**Candidate branch:** `TechDebt/techdebt-run-sweep-20260829-215319`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:20a6c543aa517b97ca89af62a0af1982a1e2c2e802ba310fbeffc9ebd6e6beba` `(9 paths)`
**Candidate bundle:** `C:\Users\TOMMYS~1\AppData\Local\Temp\claude\C--Users-TommySeery-source-repos-Concertable\7fa0ad02-f09d-44bc-a3e5-d3bd2e2925fb\scratchpad\review-bundle-techdebt-run-sweep-20260829-215319`
**Candidate bundle identity:** `sha256:283bb074492f89faa1982ab83aac69e03e2d474f4a129831532c9a1f5dfecb15`
**Work-order path:** `reviews/TechDebt-techdebt-run-sweep-20260829-215319.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

### Context

Resolves the `IReadRepository.GetByIdAsync should not be virtual` entry in
`api/Concertable.DataAccess/TECH_DEBT.md`. Seals `GetByIdAsync` on the shared repository bases
(`Concertable.DataAccess.Infrastructure/Repository.cs`) and migrates the two overrides the entry names:
`ConcertReadRepository`'s `Genres` eager-load becomes `AutoInclude()` on `ConcertEntityConfiguration`
(owned relation); `CommissionBindingRepository`'s `CommissionConfiguration` eager-load becomes an explicit
`GetWithConfigurationByIdAsync` method, consumed by the two `CommissionService` call sites that need it.
Test mocks updated to match.

`Concertable.DataAccess.Infrastructure` is consumed by every other service as a published NuGet package,
not a project reference (per the `packages` skill), so this seal has no effect on other consumers until the
package republishes and `platform-sync` bumps their pin. A full search during implementation found four
more repos with their own `GetByIdAsync` override not named in the original tech-debt entry —
`PreferenceRepository` (Customer), `ApplicationRepository`, `OpportunityRepository`, `BookingRepository`
(B2B Concert) — deliberately left unmigrated here since sealing the base is inert for them today; they will
go compile-red in the platform-sync PR once this publishes and get migrated there per the `packages`/
`merging` skills. Flagged in the PR description (#861) for the next handler.

### Rules manifest

Routed and read: `csharp-style`, `csharp-naming`, `docs-and-debt`, `dotnet-standards:unit-testing`,
`dotnet:unit-testing`, `dotnet-standards:dependency-injection`, `dotnet-standards:module-structure`,
`dotnet-standards:multitenancy`, `dotnet-standards:persistence`, `dotnet-standards:result-carriers`,
`dotnet:module-structure`, `dotnet:multitenancy`, `dotnet:persistence`. No violations found against any of
them — the diff removes a re-declared base method rather than adding one (matches `persistence`'s
"never re-declare GetById, not even for a CancellationToken overload"), and the new
`GetWithConfigurationByIdAsync` name follows `csharp-naming`'s "name a repository method for the query, by
what key" rule.

### Native/general review

Dispatched to `code-reviewer` over the frozen `base..head` diff (correctness, reuse, simplification,
efficiency, error handling). No findings. Verified: the `AutoInclude()` migration is behaviorally
equivalent to the removed `Include(Genres)` override for every query path; the Payment call-site split
(`GetByIdAsync` for `FindBoundPaymentIntentAsync`, `GetWithConfigurationByIdAsync` for
`ConfirmReviewedGrossAsync`/`CalculateBoundAsync`) is correct; test mocks match; the four other
pre-existing overriding repos are unaffected today because they consume the shared base as a
`PackageReference`, consistent with the stated intent.

### Security review

Diff touches `api/Concertable.Payment/**`, which this repo's merge-gate config (`security_paths`) treats
as security-sensitive, so the host security review ran over the same frozen diff. No findings — pure
data-access reshape (repository method rename/seal, one `AutoInclude()`), no user input, no
auth/authz, no crypto, no new data exposure (`CommissionConfigurationEntity` carries only `Id`/`Rate`/
`CreatedAt`, no PII/secrets, and was already reachable through the prior override).

### Verification

- `dotnet build` clean: `Concertable.DataAccess.Infrastructure`, `Concertable.Payment.Web`,
  `Concertable.Customer.Web`, `Concertable.B2B.Concert.Infrastructure`.
- `dotnet test`: `Concertable.Payment.UnitTests` 569/569 green; `Concertable.Customer.Concert.UnitTests`
  25/25 green.

### Findings

None.
