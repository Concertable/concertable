# Code review — Feature/CommissionBindingDeferredPricing

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `f2e206133397868baafc9f362f0e451b8f322178`  _(2026-08-01)_

> Range reviewed: `2ccd91567..f2e206133` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **OWN1 — HIGH — configuration ownership** — `api/Concertable.Payment/src/Concertable.Payment.Domain/Entities/CommissionBindingEntity.cs:9`
  Each payer binding copies version, currency, commission rate, and VAT rate instead of referencing one immutable configuration revision. Normalize commission configuration into its own immutable entity, make bindings store only its foreign key, load terms through that relationship, and bootstrap the configured revision once with conflict validation.

  Resolved against the authoritative plan by keeping validated append-only revisions in the immutable
  in-memory Payment catalog. Bindings persist only `CommissionConfigurationId` plus binding identity and
  Stripe context; bound calculations resolve the referenced revision from the catalog. The obsolete SQL
  configuration entity, repository, initializer, table and relationship were removed. Verified with 140
  Payment unit tests, 7 Payment integration tests, an exact EF model/snapshot check, the full solution
  build and standalone Payment carve, all with zero failures/errors or pending model changes. The same
  gate passed again after merging current `origin/main` into the branch.
