# Code review — Feature/typed-result_customer-outcomes

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `d4a5bb50229afdd4269de309d933a833cfc32d8f`  _(2026-08-09)_
**Security-reviewed up to commit:** `d4a5bb50229afdd4269de309d933a833cfc32d8f`  _(2026-08-09)_

> Range reviewed: `d66b780cd..d4a5bb502` (34 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **NAT1 — LOW — native** — `api/Concertable.Customer/src/Modules/Preference/Concertable.Customer.Preference.Application/Concertable.Customer.Preference.Application.csproj:12`
  Remove the stale direct `Concertable.Kernel` references from Preference.Application and User.Application, plus Preference's unused Kernel global usings. Those references were introduced for the old Kernel Result/Option contracts, but both projects now consume Reunion and no longer compile against a Kernel API.
  Fixed by removing both direct package references and the two unused Preference global usings; both application projects build successfully.
