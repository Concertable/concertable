# Docs review — Docs/launch_tenant-verification_plan

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> ambiguous finding: flag it in one line, take the safe path, keep going.

**Reviewed up to commit:** `a4f6fa696edbd79cde8a8cb484e1c6e7c11367b2`  _(2026-08-24)_

> Range reviewed: `1d25c3b58..a4f6fa696` (1 commit).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

No findings. Checked lenses A (accuracy vs reality), B (contradiction with sibling docs), C (right
home), D (concision — n/a, not a harness-reloaded file), E (dangling/transient references), F
(followable instructions).

Verified against the current repo: `VenueEntity.Approved`/`Approve()` read only by
`VenuePrivilegedRepository.GetPendingApprovalAsync`; `ITenantModule.IsTaxComplianceCompleteAsync` and
`ISelfBillingAgreementGate` both exist and are consumed by `FinishExecutor.FinishAsync` as fail-closed
dual gates; `SharedPermissions.TenantSettingsEdit` and `RateLimitPolicies.Upload` exist;
`IBlobStorageService`, `ContentReportNotifier`/`IEmailTransport`, `IDealPayeeResolver`,
`ConcertCompletionRunner`, `ModerationController`, `OpportunityMutationError` (Dunet union with
`VenueNotFound`/`ErrorDefinition`), `VenueController`'s approve/pending-approval surface,
`app/web/admin/src/features/venues/` and `features/moderation/ResolveReportDialog.tsx`,
`routes/_admin/route.tsx`, `TaxDetailsBanner.tsx` (the DAC7 nag), and `api/initial-migrations.ps1` all
exist as described. The `LAUNCH_ROADMAP.md` `§5`/`§9` cross-references (tenant suspension, settlement
dispute) resolve to the correct sections. Plan layout (`<NAME>_PLAN.md` + `<NAME>_PROGRESS.md` under
`plans/launch/`) matches `plans/AGENTS.md`'s convention. No dead links, no cross-doc contradictions, no
dangling Phase/line references outliving their host.
