# Kernel derived error codes progress

- Plan: `plans/TYPED_RESULT_MIGRATION.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Refactor\DerivedErrorDefinitions`
- Branch: `Refactor/DerivedErrorDefinitions`
- PR: not opened yet
- Dependency/package gates: publishes `Concertable.Kernel`, so this phase owns its generated
  `chore/platform-sync-*` PR through green. Phase 2 (PR #296) consumes the new factories only after
  that sync lands; PR #296 is not touched here.
- Last reconciled: 2026-08-04 after fast-forwarding the worktree to `origin/main` `9dfb5e63d`

## Current state

Phase 1B is implemented and locally green. Kernel gains `ErrorCodeAttribute`, the cached internal
`ErrorCodeResolver`, and one `<TCase>` factory per `ErrorKind`. Every pre-existing explicit factory,
including `NotFound<T>(string code)`, is unchanged.

The uncommitted work this ledger inherited (generic factories, `ErrorCodeAttribute`, cached derivation,
initial tests, none of it ever compiled) is preserved in behaviour but was restructured twice on
Tommy's direct feedback — see Decisions. Nothing is left uncommitted beyond the working tree described
under Completed work.

## Next Steps

Review the complete `origin/main..HEAD` diff, resolve every finding, then commit, push, and open the
Kernel PR with the `skip-e2e` label (Kernel-only, additive, fully unit-covered). Merge it, then follow
the generated `chore/platform-sync-*` PR to green/merged and record the published Kernel version here.
Do not modify PR #296.

## Completed work

- Fast-forwarded the worktree from `c45b33740` to `origin/main` `9dfb5e63d`, picking up merged docs PRs
  #340 and #341 without disturbing the two dirty files.
- `api/Concertable.Shared/src/Concertable.Kernel/Errors/Error.cs` — added `ErrorCodeAttribute` and the
  `Invalid<TCase>`, `NotFound<TCase>()`, `Conflict<TCase>`, `Unauthenticated<TCase>`,
  `Forbidden<TCase>`, `PaymentRequired<TCase>`, and `Validation<TCase>` factories.
- `api/Concertable.Shared/src/Concertable.Kernel/Errors/ErrorCodeResolver.cs` — new internal cached
  resolver: `Of<TCase>()`/`Of(Type)` over one `ConcurrentDictionary<Type, string>`.
- `api/Concertable.Shared/src/Concertable.Kernel/AssemblyInfo.cs` — new, `InternalsVisibleTo`
  `Concertable.Kernel.UnitTests` so the resolver is tested directly instead of by reflection.
- `api/Concertable.Shared/tests/Concertable.Kernel.UnitTests/` — `ErrorCaseFixtures.cs` (shared union
  and case fixtures), `ErrorCodeResolverTests.cs` (derivation table, `[ErrorCode]` override and
  non-inheritance, caching, six underivable shapes), and `ErrorDefinitionTests.cs` extended with the
  per-kind case factories; all pre-existing tests kept.
- `api/agents/CODE_CONVENTIONS.md` and `plans/TYPED_RESULT_MIGRATION.md` — codes are derived, messages
  stay explicit apart from the `[DisplayName]`-backed not-found one, `[ErrorCode]` pins published
  codes, the union example uses the new factories, and the per-case `Definition` overrides are recorded
  as collapsing into one exhaustive native-union `switch` at that cutover.
- `plans/TYPED_RESULT_MIGRATION.md` — added Phase 1B and made Phase 2's dependency include this
  Kernel publication.
- Deleted `plans/TYPED_RESULT_MIGRATION_CONVENTIONS_PROGRESS.md`: both PRs it tracked (#335, #340) are
  merged and docs-only, so its lifecycle is terminal and Lifecycle 5 forbids keeping it.

## Verification

- `dotnet test api/Concertable.Shared/tests/Concertable.Kernel.UnitTests` — **239 passed, 0 failed**
  (2026-08-04, after the `ErrorCodeResolver` rename).
- `dotnet build api/Concertable.Shared/tests/Concertable.Kernel.UnitTests/...csproj` — 0 warnings,
  0 errors.
- `dotnet build api/Concertable.slnx --configuration Release` — see the event log entry for its result.
- Derivation is asserted against hard-coded expected codes, never recomputed with the production
  helper: `payment.invalid_request`, `payment.payer_not_found`, `payment.declined`,
  `payment.not_found`, `commission.binding_not_found`, `commission.rate_not_found`,
  `escrow.refund_not_found` (from both `EscrowNotFound` and `RefundNotFound`),
  `escrow.refund_currency_mismatch`, `gateway.http_2_unavailable`, `gateway.ach_mandate_not_found`,
  and the pinned `escrow.refund_not_allowed`.

## Reviews

Pending: the complete-diff review named in `## Next Steps` has not run yet.

## Decisions, discoveries, blockers, and deviations

- **No nullable metadata record.** The inherited design cached
  `ErrorCaseMetadata(string Code, string? NotFoundDisplayName)`, carrying a not-found-only field on
  every case. Tommy rejected it outright. Codes and messages are separate concerns; only the code is
  resolved and cached.
- **No CLR-name message fallback.** An intermediate version humanized a case name into the not-found
  message (`BindingNotFound` → "Binding not found."). That contradicts the merged convention "the CLR
  type name is never a fallback", so `NotFound<TCase>()` uses `DisplayNameResolver.Of<TCase>()` and
  throws when the case has no `[DisplayName]`. This deleted the humanizer and its acronym handling.
- **Named for what it resolves.** `ErrorCaseResolver.CodeOf` was renamed `ErrorCodeResolver.Of` to
  match `ErrorCodeAttribute` and the existing `DisplayNameResolver.Of<T>()`.
- The resolver stays `internal`; `InternalsVisibleTo` is the repo convention and keeps the published
  Kernel surface to the factories and the attribute.
- Word splitting validates as well as splits: a name that does not reassemble from its words
  (`Legacy_NotFound`) is rejected rather than silently producing a code.
- `[ErrorCode]` and `[DisplayName]` are read with `inherit: false` because Dunet cases inherit their
  union root — a root-level attribute must not leak into every case's published code.
- Two cases of one union may legitimately derive the same code (`EscrowNotFound` and `RefundNotFound`
  both give `escrow.refund_not_found`). Kernel does not police that; the per-case contract test is
  where a collision surfaces.
- The payload-free `XError(ErrorDefinition)` record form has no per-alternative type, so derivation
  does not apply to it and its codes stay explicit. Recorded in both docs.

## Event log

### 2026-08-04 — synced the worktree and inherited the uncommitted design

- Action: read the required AGENTS/plan docs, fetched, and fast-forwarded `Refactor/DerivedErrorDefinitions`
  from `c45b33740` to `origin/main` `9dfb5e63d` with the two dirty files intact.
- Evidence: `git rev-list --count HEAD..origin/main` returned `0` afterwards; `git status --short`
  still listed only `Error.cs` and `ErrorDefinitionTests.cs`.
- Outcome: discovered that merged PR #340 had already replaced the centralized `Definition.Match`
  convention with abstract-root plus per-case overrides, so the docs work reduced to the derived-code
  rules rather than that representation change.
- Follow-up: implement and verify the Kernel derivation.

### 2026-08-04 — implemented, restructured twice, and verified locally

- Action: split the derivation into `ErrorCodeResolver`, dropped the nullable metadata record and the
  CLR-name message fallback on Tommy's feedback, renamed the resolver to match the attribute, and
  rewrote the tests against shared fixtures.
- Evidence: 239/239 Kernel unit tests pass; the test project builds with 0 warnings.
- Outcome: Kernel derives codes only, messages stay explicit or `[DisplayName]`-backed, and the
  published surface is the seven `<TCase>` factories plus `ErrorCodeAttribute`.
- Follow-up: Release solution build, complete-diff review, then PR.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Refactor\DerivedErrorDefinitions
Read @plans/TYPED_RESULT_MIGRATION.md and @plans/TYPED_RESULT_MIGRATION_DERIVED_CODES_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
