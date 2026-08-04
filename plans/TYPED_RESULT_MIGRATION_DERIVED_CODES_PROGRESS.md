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

Push the branch and open the Kernel PR with the `skip-e2e` label (Kernel-only, additive, fully
unit-covered). Merge it, then follow the generated `chore/platform-sync-*` PR to green/merged and
record the published Kernel version here. Do not modify PR #296.

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
- Left `plans/TYPED_RESULT_MIGRATION_CONVENTIONS_PROGRESS.md` in place. It was briefly deleted here as
  a terminal ledger (its PRs #335 and #340 are merged and docs-only), then restored on discovering that
  open PR #343 is still editing it — it is the rolling docs-convention ledger, not terminal, and
  deleting it would have made #343 a modify/delete conflict. Its close-out belongs to whoever lands the
  last docs PR in that family.

## Verification

- `dotnet test api/Concertable.Shared/tests/Concertable.Kernel.UnitTests` — **240 passed, 0 failed**
  (2026-08-04, after the review fixes; 239 before the added `RefundEscrowNotFound` row).
- `dotnet test api/Concertable.Shared/tests/Concertable.Shared.Api.UnitTests` — **51 passed, 0 failed**;
  the `TError : IError` terminal and ProblemDetails mapping are unaffected by the new factories.
- `dotnet build api/Concertable.slnx --configuration Release` — **0 errors**, 7 warnings, all
  pre-existing (`CS0628` on both `UserEntity` types, `CS8604` in `AppFixture`, `CS8632` in generated
  Reqnroll temp files) and none in a touched file.
- Derivation is asserted against hard-coded expected codes, never recomputed with the production
  helper: `payment.invalid_request`, `payment.payer_not_found`, `payment.declined`,
  `payment.not_found`, `commission.binding_not_found`, `commission.rate_not_found`,
  `escrow.refund_not_found` (from both `EscrowNotFound` and `RefundNotFound`),
  `escrow.refund_currency_mismatch`, `gateway.http_2_unavailable`, `gateway.ach_mandate_not_found`,
  and the pinned `escrow.refund_not_allowed`.

## Reviews

`/code-review` over the complete branch diff `9dfb5e63d..c0b5802b2` (1 commit), artifact
`reviews/Refactor-DerivedErrorDefinitions.md`. Range taken from `origin/main` because local `main` was
4 commits stale, which would otherwise have re-reviewed merged PRs #340 and #341. Two LOW findings,
both fixed in the follow-up commit and ticked in the artifact; no findings remain open.

- CV1 (test hygiene): the underivable-shape fixtures were at namespace scope, one named exactly
  `Error`, shadowing that name for every file in the test assembly; `PaymentError` also held the
  malformed `Legacy_NotFound`. Both moved into an `UnderivableShapes` container.
- TEST1 (coverage): the repeated-context loop only ever saw one repeated word, so its second iteration
  was unasserted. Added `EscrowRefundError.RefundEscrowNotFound` → `escrow.refund_not_found`.

Lenses B (microservice isolation), C (module boundaries), and D (seeding) had nothing to judge: the
diff is Kernel plus docs, with no service, seeder, facade, or cross-service reference touched.

## Decisions, discoveries, blockers, and deviations

- **Open PR #343 `Docs/NaturalErrorCaseNames` overlaps this PR's docs.** It edits
  `api/agents/CODE_CONVENTIONS.md` and `plans/TYPED_RESULT_MIGRATION.md` in the same error-convention
  region and prefers natural case names without a `Case` suffix. That direction is compatible with the
  resolver — stripping an optional `Case` suffix is a tolerance, not a requirement — but whichever PR
  merges second needs a rebase on those two files. Not resolved here; #343 is not touched.
- **The local branch ref was lowercase `refactor/DerivedErrorDefinitions`** while the remote uses
  `Refactor/`. Nothing had been pushed, so it was renamed and published through the explicit refspec
  `HEAD:refs/heads/Refactor/DerivedErrorDefinitions`; a lowercase push would have created the dual-casing
  ref that breaks `git fetch` for everyone. The on-disk `.git/refs/heads/refactor/` directory still
  holds four other branches in that casing, which is why `git branch --list` still prints lowercase.

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

### 2026-08-04 — verified, reviewed, and findings resolved

- Action: ran the Release solution build and the Shared.Api suite, committed `c0b5802b2`, ran
  `/code-review` over `9dfb5e63d..c0b5802b2`, then fixed both findings.
- Evidence: Release build 0 errors; Kernel 240/240; Shared.Api 51/51;
  `reviews/Refactor-DerivedErrorDefinitions.md` with both findings ticked.
- Outcome: no review finding is open and the branch is one commit ahead of `origin/main` plus the
  fix commit.
- Follow-up: push, open the `skip-e2e` PR, merge, then carry Kernel publication and platform sync.

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Refactor\DerivedErrorDefinitions
Read @plans/TYPED_RESULT_MIGRATION.md and @plans/TYPED_RESULT_MIGRATION_DERIVED_CODES_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
