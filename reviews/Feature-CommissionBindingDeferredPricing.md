# Code review — Feature/CommissionBindingDeferredPricing

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `0dab856dc708af4dd5612bf1be1d52598d717244`  _(2026-08-04)_

> Range reviewed: `2ccd91567..f2e206133` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **OWN1 — HIGH — configuration ownership** — `api/Concertable.Payment/src/Concertable.Payment.Domain/Entities/CommissionBindingEntity.cs:9`
  Each payer binding copies version, currency, commission rate, and VAT rate instead of referencing one immutable configuration revision. Normalize commission configuration into its own immutable entity, make bindings store only its foreign key, load terms through that relationship, and bootstrap the configured revision once with conflict validation.

  Resolved with one current Azure {ConfigurationId, RatePercentage} value and immutable SQL
  configuration history. Percentage owns validation and half-up application while configuration,
  contracts and protobuf expose the business percentage rather than basis points. Startup inserts each
  new configured ID once and rejects reuse with a different percentage. Bindings persist only the
  CommissionConfigurationId foreign key plus their own currency, identity and Stripe context; bound
  calculations load the referenced historical percentage through that relationship. Version and
  currency were removed from percentage configuration, and VAT uses the same value object. Verified
  after merging current `origin/main` with 141 Payment unit tests, 7 Payment SQL integration tests,
  no pending Payment model changes, regenerated initial migrations, the full solution build and the
  standalone Payment carve, all with zero failures or errors.

## Incremental review — 2026-08-02

> Range reviewed: `f2e206133..e73b30bb4` (39 commits).

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C#
conventions, and test coverage of changed paths.

## Incremental review — CI follow-up — 2026-08-02

> Range reviewed: `e73b30bb4..99ef2faac`.

No issues found. The GitHub-hosted integration matrix now relies on runner teardown instead of the
Docker Hub-hosted Testcontainers resource reaper. The failed Customer User integration project
passed 6/6 locally with the exact workflow setting.

## Incremental review — 2026-08-03

> Range reviewed: `99ef2faac..357a2ca7d` plus the Payment typed-result working tree.

- [x] **CV1 — MEDIUM — C# conventions** — `api/Concertable.Payment/src/Concertable.Payment.Client/Adapters/CommissionClient.cs:9`
  Thirteen changed Payment clients/services capture collaborators through primary constructors. Replace those captures with explicit constructors and `private readonly` fields, as `api/agents/CODE_CONVENTIONS.md` requires for captured state.

- [x] **BUG1 — MEDIUM — correctness** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/EscrowService.cs:525`
  Provider-failure cleanup ignores the new typed result from `ReleaseRefund`, so a non-pending reservation would now be silently saved instead of preserving the former invariant exception. Check the transition result and throw the same invariant failure in both escrow and settlement refund paths.

- [x] **CV2 — LOW — C# conventions** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/ManagerPaymentService.cs:388`
  `ResolveStripeCustomerAsync` adds an `is { }` capture. Replace it with the repository-standard explicit null check required by `api/agents/CODE_CONVENTIONS.md`.

- [x] **TEST1 — MEDIUM — test coverage** — `api/Concertable.Payment/src/Concertable.Payment.Contracts/Errors/PaymentErrors.cs:10`
  The new Payment and transition error unions have no definition tests. Add one data-driven definition assertion per case, as `api/agents/CODE_CONVENTIONS.md` requires for every operation-owned error union.

- [x] **TEST2 — MEDIUM — test coverage** — `api/Concertable.Payment/src/Concertable.Payment.Client/Adapters/PaymentClientResults.cs:13`
  The new binary-trailer transport seam is untested. Add focused tests for successful calls, mapped typed failures, unmapped/malformed trailers, and caller cancellation so the published client contract cannot silently regress to exceptions or swallow faults.

- [x] **BUG2 — MEDIUM — correctness** — `api/Concertable.Payment/src/Concertable.Payment.Client/Adapters/PaymentClientResults.cs:38`
  A malformed binary error trailer leaks `InvalidProtocolBufferException` because the nested `throw` rethrows the parser failure rather than the caught RPC. Treat malformed detail as an unrecognized provider response and rethrow the original `RpcException`.

## Resume verification — 2026-08-03

CV1, BUG1, CV2, TEST1, TEST2, and BUG2 are fixed in the completing commit. The focused BUG1
regressions pass 2/2 and the complete Payment unit suite passes 188/188. The full solution and the
standalone Payment carve build with 0 errors, and EF reports no pending Payment model changes after
merging current `origin/main`. Docker responded successfully and
`dotnet test api/Concertable.Payment/tests/Concertable.Payment.IntegrationTests/Concertable.Payment.IntegrationTests.csproj --logger "console;verbosity=normal"`
passed all 7 tests on the same combined code state.

## Post-main reconciliation — 2026-08-04

OWN1, CV1, BUG1, CV2, TEST1, TEST2, and BUG2 remain closed after merging current `origin/main`
`37c94cd03780d940b3c827c3e2f4442a8709297e`. Main's Payment.Domain internalisation and
Application-owned payout-status boundary were preserved; branch-only commission configuration,
percentage, and transition-error types were internalised consistently. The combined Payment unit
project's duplicate generated proto enum was resolved with an explicit Infrastructure assembly alias.

Fresh verification on the reconciled state: Payment SQL integration 7/7; focused payout mapper 4/4;
Payment unit 192/192; Release solution build 0 errors; standalone Payment carve 0 errors; and no
pending `PaymentDbContext` model changes. No finding was reopened. The new merge resolution and test
alias require incremental review before any push.

## Incremental review — 2026-08-04

> Range reviewed: `99ef2faac..0dab856dc`. Substantive branch delta = the two current-main merge
> reconciliations (`4946bba27` merging `origin/main` `37c94cd0`; `b6fb56c6c` merging `origin/main`
> `f05f8832d`). All other commits in the range are `origin/main`'s own merged-in work (reviewed on
> their PRs) or branch docs. `f693c955d`'s typed-result code was already covered by CV1–BUG2 above.

No issues found. Checked correctness, microservice isolation, module boundaries, seeding, C#
conventions, and test coverage of changed paths.

- `b6fb56c6c` (today's merge) introduced **no** evil-merge code hunks — every `api/**` file
  auto-merged; the only hand-resolved conflict was `plans/AGENTS.md` (docs, took main's newer
  parallel-worktree handoff text).
- `4946bba27`'s reconciliation internalises the branch-only Payment.Domain types (`Percentage`,
  `CommissionTerms`, `CommissionConfigurationEntity`, the three transition-error unions, and the
  escrow/refund/settlement/transaction entities) to match main's `267bd9d45` Domain-surface-internal
  decision. Correct: `Payment.Domain/AssemblyInfo.cs` grants `InternalsVisibleTo` to Application,
  Infrastructure, Seed, and the test projects, so no cross-assembly access breaks, and keeping them
  `public` would reintroduce exactly the surface debt main just removed. Build, 192 unit tests, 7
  integration tests, the standalone carve, and the EF model check are all green on the reconciled tree.
- The dual generated-proto collision (Client and Infrastructure both emit `Concertable.Payment.Grpc.*`)
  is resolved test-side only, with `Aliases="global,PaymentInfrastructure"` on the unit project's
  Infrastructure reference and an `extern alias PaymentInfrastructure` in `PayoutAccountStatusMapperTests`.
  This disambiguates the enum for exact assertions without coupling or weakening either published
  package boundary — the standard C# remedy for identical CLR names from two referenced assemblies.
