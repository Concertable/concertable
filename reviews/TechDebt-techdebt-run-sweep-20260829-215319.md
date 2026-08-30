# Code review — TechDebt/techdebt-run-sweep-20260829-215319

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `3d9e58686c0938d365807db037b4b6e843157534`  `(2026-08-30)`
**Security-reviewed up to commit:** `3d9e58686c0938d365807db037b4b6e843157534`  `(2026-08-30)`
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

## Review pass — 2026-08-30 — incremental

**Candidate base:** `bf1533719d9e2af52ac4f64321e3a9e39a8a97d2`
**Candidate head:** `3d9e58686c0938d365807db037b4b6e843157534`
**Candidate branch:** `TechDebt/techdebt-run-sweep-20260829-215319`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:7c900647b6fe3fe031ac0b0602aef98821c1392915c59b9506fa31ab01277f3c` `(17 paths)`
**Work-order path:** `reviews/TechDebt-techdebt-run-sweep-20260829-215319.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

### Context

The first pass's "deferred to platform-sync" call was wrong: CI's `build` job runs
`./scripts/local-platform.ps1 build api/Concertable.slnx` (project references, not the published package),
so it exercised the local, now-sealed `DataAccess` source directly and failed immediately on the four
un-migrated overrides, rather than waiting for a future platform-sync PR. This pass migrates all four —
`PreferenceRepository` (Customer) gets `AutoInclude()` on `GenrePreferences` (owned, always loaded
elsewhere in the class); `OpportunityRepository`'s `Venue` include is dropped outright at first (no caller
appeared to need it) then restored as an explicit `GetWithVenueByIdAsync` once a real caller was found (see
below); `ApplicationRepository` and `BookingRepository` get explicit named methods
(`GetWithArtistAndOpportunityByIdAsync`, `GetWithApplicationAndConcertByIdAsync`) for their one
eager-loading caller each. Also renames `BookingRepository.GetForSettlementByConcertIdAsync` (named for
its use case, not what it fetches) to `GetWithApplicationByConcertIdAsync`, and adds a `csharp-naming`
standard rule (in `dotagents`, `Docs/naming-agent-noun-suffixes`) codifying "name the real joined relation,
never a bucket noun or a use case."

**Two further sub-passes found and fixed real regressions this candidate's own build could not catch,
because none of them failed to compile:**

- Sub-pass 1 (native review + a from-scratch caller audit across the whole `api/` tree): `OpportunityService.GetByIdAsync`
  (backs `GET /api/Opportunity/{id}`) fed the now-bare `GetByIdAsync` result to a mapper needing
  `opportunity.Venue.Name` — a guaranteed `NullReferenceException`, missed by the first sweep because this
  caller injects the repository as `repository`, not `opportunityRepository`. Fixed with the restored
  `GetWithVenueByIdAsync`. Also `AcceptExecutor.VerifyTermsUnchanged` read `app.Opportunity.Period`,
  which only "worked" via EF's incidental relationship-fixup (another call in the same request loads the
  Opportunity into the same scoped `DbContext` first) — fixed by fetching the period explicitly via
  `IOpportunityRepository.GetPeriodByIdAsync`. Also caught by CI itself, independently:
  `CommissionConfigurationPersistenceTests.cs` (an *integration* test, outside the unit-test-only
  verification the first pass ran) called `CommissionBindingRepository` by its old `GetByIdAsync` name.
- Sub-pass 2 (native review over sub-pass 1's diff): `ContractIssuer.IssueAsync` read
  `application.Opportunity.Period` the same fixup-dependent way (made to "work" by its own
  `GetArtistAndVenueByIdAsync` call moments earlier) — same fix, explicit `GetPeriodByIdAsync`. Also
  flagged that the new "opportunity must exist" guards in `AcceptExecutor`/`ContractIssuer` should use this
  module's established `.OrNotFound(DisplayNames.X)` idiom rather than a raw `InvalidOperationException`;
  fixed in `ContractIssuer` (matches its own sibling lookup one line above); left as-is in `AcceptExecutor`,
  which already had a raw `InvalidOperationException` for an equivalent "booking must exist" case a few
  lines below, so it's internally consistent with that class's own pre-existing convention.

### Rules manifest

Same set as the first pass, all already loaded; no new routed skill beyond `dotnet-standards:integration-testing`
/ `dotnet:integration-testing` (routed by the `CommissionConfigurationPersistenceTests.cs` fix).

### Native/general review

Three dispatches against successive frozen diffs (`bf1533719..28b8a650d`, `28b8a650d..da1428f92`,
`da1428f92..3d9e58686`), each independently re-deriving the claimed fixes rather than trusting the prior
commit's description. Findings folded into Context above; all resolved in this candidate.

### Security review

`da1428f92..3d9e58686` again touches `api/Concertable.Payment/**` (the integration-test fix), so the
security layer re-ran over the cumulative branch diff. No findings — same reasoning as the first pass, plus:
the new `GetWithVenueByIdAsync`/`GetWithArtistAndOpportunityByIdAsync`/`GetWithApplicationAndConcertByIdAsync`
methods query through the same tenant-scoped context every sibling method in these classes already uses, so
no tenant-filter bypass is introduced.

### Verification

- `./scripts/local-platform.ps1 build` (project-reference mode, reproducing CI's `build` job) green for:
  `Concertable.B2B.Web`, `Concertable.Customer.Web`, `Concertable.Payment.Web`,
  `Concertable.B2B.Concert.Infrastructure`, `Concertable.Customer.Preference.Infrastructure`.
- `dotnet test` green: `Concertable.B2B.Concert.UnitTests` 234/234, `Concertable.Customer.Preference.UnitTests`
  19/19, `Concertable.Payment.UnitTests` 569/569, `Concertable.Payment.IntegrationTests` 49/49 (includes the
  regression this pass fixed, run against a real containerized SQL instance).
- `Concertable.B2B.Concert.IntegrationTests` and `Concertable.Customer.Preference.IntegrationTests` could not
  run locally (Windows path-length limit on this worktree's `SNI.dll` load, and an unrelated Docker Desktop
  instability) — left to CI, whose Linux runners don't hit either issue.

### Findings

None open — all findings from both native-review sub-passes were fixed within this same pass.
