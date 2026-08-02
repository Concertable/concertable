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

  Resolved with one current Azure {ConfigurationId, RatePercentage} value and immutable SQL
  configuration history. Percentage owns validation and half-up application while configuration,
  contracts and protobuf expose the business percentage rather than basis points. Startup inserts each
  new configured ID once and rejects reuse with a different percentage. Bindings persist only the
  CommissionConfigurationId foreign key plus their own currency, identity and Stripe context; bound
  calculations load the referenced historical percentage through that relationship. Version and
  currency were removed from percentage configuration, and VAT uses the same value object. Verified
  with 141 Payment unit tests, 7 Payment SQL integration tests, regenerated initial migrations and the
  full solution build, all with zero failures or errors. Standalone Payment carve verification follows
  the implementation commit.
