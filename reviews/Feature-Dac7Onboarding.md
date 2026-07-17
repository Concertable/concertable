# Code review — Feature/Dac7Onboarding

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `b06533b846746370554d6d333acf738d1ef9e908`  _(2026-07-16)_

> Range reviewed: `32bb1c8a..b06533b8` (3 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

> **Scope note.** Reviewed the committed range `32bb1c8a..b06533b8` (a coherent, buildable unit). The
> working tree has a large **uncommitted** DAC7 → TaxCompliance/PayoutCompliance rename in flight (new
> `Tenant.Application/Tax/`, `Domain/TaxCompliance.cs`, WIP `plans/b2b/TAX_COMPLIANCE_REFACTOR.md`,
> deleted `Dac7/*` + `ComplianceDto`) — mid-refactor and not yet buildable, so out of scope here. Re-run
> `incremental-review` once that lands. Where a committed finding is already addressed by the in-flight
> rename, it's annotated below.

The branch is clean against the high-value lenses: **no** correctness bugs, **no** microservice-isolation
issues (all changes are within B2B; Concert→Tenant crosses only via the `ITenantModule` Contracts facade),
**no** module-boundary violations, **no** seeding violations (tenant + its `Compliance` are the documented
direct-insert exception, written through the same domain method production uses). C# conventions hold
(source-generated logging, explicit ctors, field naming, `is not null`, brace rules). The keyed-strategy
resolvers (`TicketPayeeResolver`/`SettlementPayeeResolver`, `Dac7Strategy`) match `CODE_PATTERNS.md`
exactly, and the fail-closed gate was verified end-to-end (the `SettlementDeferred` success reason
survives `IScoped<T>.RunAsync`, so the hourly sweep correctly suppresses the "finished" log on deferral;
ticket/settlement payee maps are exact inverses reading the right tenant/user fields).

Only two low-severity comment-convention nits:

- [x] **CV1 — LOW — C# conventions (comments)** — `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Application/Dac7/IDac7Strategy.cs:7`
  The `<summary>` cited transient plan phases: _"Consumers (the org-form validator; the **Phase-2** payout
  gate; the **Phase-3** nag) inject this…"_. Root `CLAUDE.md` (Code comments) names this exactly: a comment
  is wrong if it _"cites a transient artifact (a plan filename, 'Phase N', a ticket) that will be deleted."_
  **Resolved by the in-flight rename** — this file is deleted and its replacement `Tax/ITaxComplianceRules.cs`
  carries no phase refs. No action taken this pass (nothing to edit).

- [x] **CV2 — LOW — C# conventions (comments)** — `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Application/Validators/TenantValidators.cs:21`
  Two adjacent comments stated the same reasoning twice: the above-class comment and the in-ctor comment
  both said the VAT-number **format** check is region-scoped and lives in `TenantService` via the rules
  strategy. **Fixed** in the working tree (`TaxComplianceDtoValidator`): removed the redundant above-class
  comment, kept the in-ctor note at the exact rule it explains.
