# Deal DTO and strategy-dispatch foundation progress

- Plan: `plans/launch/DEAL_CLOSED_SUM_MODEL_PLAN.md`
- Roadmap: `plans/launch/LAUNCH_ROADMAP.md`
- Roadmap item: `launch/deal-closed-sum-model`
- Worktree: former foundation worktree retired; no public-library implementation worktree
- Branch: former `Refactor/deal-dispatch-foundation`; no active follow-up branch
- PR: foundation [#678](https://github.com/Concertable/concertable/pull/678) merged;
  downstream lifecycle ledger records foundation/sync #678/#694 terminal
- Dependency/package gates: future native closure needs the supported .NET 11 compiler/target matrix;
  configurable Deal language/storage is a separate owner, not blocked on a public dispatch library
- Last reconciled: `2026-09-10` against `17e03aca1` (`origin/main`)

## Current state

The .NET 10 DTO/keyed-DI foundation is on `main` through PR #678, merge
`1e26f824472fb5329e22eaca8ecd53cab49c1e86`. The old instructions to push/requeue that PR are spent.
It introduced four sealed Deal DTO cases, preserved the four-case wire protocol and parallel entity
model, validated keyed mapper/updater coverage and hid lookup inside the owning factory.

The production generator was deliberately not shipped. The prototype remains research on
`Spike/net11-closed-dispatch` at `785cd80403eb2f3db173428854730dec961e39d9`, including its unresolved
same-pass attribute-source design. No public .NET 11 dispatch library is delivered by this foundation.

The configurable product now belongs to `plans/launch/DEAL_CONFIGURATION_PROGRESS.md`. It makes whole
deals versioned configurations of finite typed rules and capabilities. Future compile-time closure here
must target that vocabulary and honest method headers, not preserve four whole-Deal subclasses or
restore mechanical mapper/updater strategies deleted by the active layering refactor.

## Next Steps

No foundation delivery action remains. When the separate public-library work is requested, create its
own plan/branch from current `origin/main` and settle Stage 2's API/package semantics before implementation.
Use a real .NET 11 consumer and a non-Concertable fixture; do not treat the preserved prototype as an API
contract. Reconcile surviving Concertable consumers before Stage 3.

Configurable-deal implementation instead starts from
`plans/launch/DEAL_CONFIGURATION_PROGRESS.md`; it does not wait for this library or .NET 11.

## Completed work

- Preserved the generator prototype independently of production delivery.
- Replaced `IDeal` with the four `DealDto` cases while preserving discriminator compatibility.
- Validated known DTO/entity/enum/JSON/registration catalogs and typed updater mismatches.
- Delivered invariant module-owned factory selection backed by Microsoft keyed DI, without claiming
  language-level closure or compiler-proven family completeness.
- Separated the shipped .NET 10 foundation, future public library and later consumer adoption.
- Reconciled the future closure target with versioned Deal configurations rather than permanent TPT cases.

## Verification

- PR #678 is merged and its source merge is an ancestor of current `origin/main`.
- The lifecycle owner's ledger records #678/#694 as its terminal consumed foundation.
- Historical foundation evidence includes 57 Deal unit tests, JSON/catalog/registration/mismatch gates
  and the B2B build. This planning reconciliation does not rerun or claim current runtime verification.
- Old queue failures/retries and review rounds remain recoverable from Git/PR history; they are not
  active failures or continuation instructions for this owner.

## Reviews

- Foundation review history is retained with the delivered PR and ledger history.
- The changed future design receives the documentation branch's isolated review; no runtime/library
  implementation approval is inferred from it.

## Decisions and downstream handoffs

- Generic keyed lookup, exhaustive closed-case dispatch and closed-family service resolution require
  separate semantic analysis before any public API is published. Keep the API non-Deal-specific and
  prove naming, variance, nullability, diagnostics and DI generation with compiler-visible symbols.
- `plans/dotnet-11/B2B_WORKFLOW_UNIONS_PROGRESS.md` owns the compiler/runtime matrix needed for native
  closure adoption, not for basic configurable-deal validation.
- `plans/launch/DEAL_CONFIGURATION_PROGRESS.md` owns the shared representation and eventual removal of
  whole-Deal subtype identity. Preserve module-local operation ownership and genuine interface families.
- `Refactor/layering_deal-vocabulary-and-mapper-collapse` owns the active vocabulary/mapper cleanup.
  Its landed APIs, not Stage 1's historical interfaces, are the consumer inventory for future dispatch.
- The lifecycle owner already consumed this foundation. Its live operation-claims ledger is not edited
  or restarted by this planning checkpoint; no instruction to resume source PR #633 is emitted here.
