# Code review â€” Refactor/B2BMembershipFrontendApi

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed â€” don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `c6891897f2df53883223bfea011fff66e1a0ec96`  _(2026-08-02)_

> Range reviewed: `ba45166..c689189` (12 commits).
> Status legend: `[ ]` todo Â· `[~]` in progress Â· `[x]` done Â· `[wontfix]` (note why).

## Findings

- [x] **[P1] Refresh the identity cache before selecting an accepted invitation's tenant**
  - `app/web/b2b/shared/src/features/members/hooks/useAcceptInvitation.ts:14`
  - The acceptance response creates a membership that is not yet present in the cached `/auth/me` identity, but `selectTenant` immediately invalidates the router and queries after persisting that new tenant ID. A concurrent route resolution can therefore reconcile the newly accepted ID away against the old membership list, while `tenantSession.tenantIdForRequest` deliberately suppresses the header until the cache recognizes it. Update the authoritative Query cache (or refetch it) before selecting and navigating, and cover that ordering in the invitation test.
