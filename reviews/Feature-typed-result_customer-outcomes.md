# Code review — Feature/typed-result_customer-outcomes

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `312400220bff550abc7abd4442abc1523a8ab22d`  _(2026-08-07)_

> Range reviewed: `06071872b..de2b8c163` (15 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **CV1 — LOW — C# conventions** — `api/Concertable.Customer/src/Modules/Preference/Tests/Concertable.Customer.Preference.IntegrationTests/PreferenceApiTests.cs:44`
  New and changed Customer integration assertions use xUnit `Assert.*`; `api/agents/INTEGRATION_CONVENTIONS.md` requires integration assertions to use Shouldly. Convert the added assertions across the Review, Preference, User, Venue, and Artist integration tests to Shouldly.

- [x] **CV2 — MEDIUM — C# conventions** — `api/Concertable.Shared/tests/Concertable.Shared.Api.UnitTests/TypedResultArchitectureTests.cs:200`
  `DunetUnionDefinitions_UseExhaustiveSwitch` only checks for `Definition => this switch`, so a discard/default arm passes even though `api/agents/CODE_CONVENTIONS.md` forbids it because it defeats closed-case exhaustiveness. Extend the architecture guard to reject discard/default arms in Dunet `IError` definition switches.

## Incremental review - 2026-08-07

> Range reviewed: `de2b8c163..312400220` (91 commits).

- [x] **CV3 - MEDIUM - C# conventions** - `api/Concertable.Shared/tests/Concertable.Shared.Api.UnitTests/TypedResultArchitectureTests.cs:388`
  `DefinitionSwitchCatchAllArmPattern` rejects literal `_` but accepts valid exhaustive `var _ =>`
  and `var ignored =>` arms, so a Dunet definition switch can still bypass the closed-case
  exhaustiveness rule in `api/agents/CODE_CONVENTIONS.md`. Reject catch-all `var` patterns within
  the bounded definition switch and cover both discard and named forms.
