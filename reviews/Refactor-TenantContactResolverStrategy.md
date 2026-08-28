# Review — Refactor/TenantContactResolverStrategy

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `f125ee9687982e1b7ce4e66b31baca4c299d86a2`  _(2026-08-28)_

Net diff reviewed: `1c0a260..f125ee9` — 28 files, 10 commits.
Status legend: `[ ]` not yet reviewed · `[x]` reviewed (date) · `[~]` in progress (incomplete — re-review).

## Summary

Review complete — all 4 areas `[x]`. **2 findings**, neither merge-blocking.

- **By severity:** MEDIUM ×1 (COV1) · LOW ×1 (RT1).
- **By lens:** changed-behaviour test impact ×1 (COV1) · route-table defect ×1 (RT1).

No correctness, service-isolation, module-boundary or convention findings. The keyed spine mirrors the
established `DealType` precedent name-for-name (marker + generic factory + unkeyed facade sharing the leaves'
interface), the leaves are correctly `Scoped` rather than Deal's `Singleton` because they hold scoped module
facades, and `RequireAll<ITenantContactResolver>()` makes a third `TenantType` a composition failure rather
than a runtime throw. `Option.None<TenantContact>()` in `ReviewAsync` is correct rather than a
`result-carriers` violation: the conditional has no target type and `TenantContact` is a value type, so no
`null` conversion to `Option<T>` exists.

## Coverage

- [x] Shared keyed-strategy mechanism — 3 files — reviewed 2026-08-28 — `api/Concertable.B2B/src/Concertable.B2B.KeyedStrategies` `api/Concertable.B2B/tests/Concertable.B2B.KeyedStrategies.UnitTests`
- [x] Tenant keyed spine and contact resolution — 9 files — reviewed 2026-08-28 — `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Application` `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure`
- [x] Deal migration onto the shared builder — 2 files — reviewed 2026-08-28 — `api/Concertable.B2B/src/Modules/Deal/Concertable.B2B.Deal.Infrastructure`
- [x] Tests, package pins, solution and split inventory — 8 files — reviewed 2026-08-28 — `api/Concertable.B2B/src/Modules/Tenant/Tests` `api/Concertable.B2B/Directory.Packages.props` `api/Concertable.Customer/Directory.Packages.props` `eng/repository-split/inventory.json`

## Findings

### [ ] COV1 (MEDIUM) — the rerouted "no contact, don't notify" decision has no test

`IVerificationNotifier` previously took `string? contactEmail` and `VerificationNotifier.SendAsync` owned the
decision behind a null check — log `VerificationContactEmailMissing`, return without sending. This branch
moves that decision up into `VerificationService.ReviewAsync` and narrows the notifier to a non-null `string`.
Observable behaviour is unchanged (no email, same log, same tenant id), but nothing pins the new placement.

Every Approve/Reject test asserts an email **was** sent
(`Assert.Contains(fixture.EmailSender.Sent, e => e.To == venue.Email)`), so the absent branch is never taken.
`GetPending_ShouldReturn200_WhenTwoPendingRowsShareTenantType` does exercise a contactless tenant
(`VenueManagerNoVenue`, which owns no venue) but asserts only that the row is present — never that `Contact`
is absent. So the contactless shape runs and is unasserted.

**Fix:** in `VerificationAdminApiTests`, assert `Assert.Null(row.Contact)` for the `VenueManagerNoVenue` row in
that test, and add an Approve case for a tenant owning no profile that asserts the verification still
transitions to Approved while `fixture.EmailSender.Sent` gains no entry.

### [ ] RT1 (LOW) — `skill-routes.json` does not route this code to `keyed-strategies`

Running the frozen tree's router over all 28 changed paths returns 16 skills; `keyed-strategies` is not among
them, despite this branch adding the shared `KeyedStrategyBuilder<TKey>`, a `TenantType`-keyed strategy family,
and a second consumer of the pattern. A reviewer following the route table therefore never loads the standard
that actually governs this code, and the write-time hook does not enforce it either.

**Fix:** add a route mapping the keyed-strategy surfaces — `api/Concertable.B2B/src/Concertable.B2B.KeyedStrategies/**`
and `**/Strategies/**` under a module's Application/Infrastructure — to `dotnet-standards:keyed-strategies`.

## Notes (no action)

- `PendingVerificationDto` changes its wire shape from two nullable scalars (`name`, `email`) to one optional
  group (`contact`), omitted rather than null. Verified no consumer: no reference to the pending-verification
  endpoint or its fields exists anywhere under `app/`, consistent with the standing debt entry recording that
  no admin moderation UI exists yet. Only `VerificationAdminApiTests` reads it, and it was updated.
- `Reunion` moves `0.1.0-alpha.8` → `0.1.0-alpha.14` in B2B and Customer. Auth and Payment stay on `alpha.3`;
  they do not consume B2B contracts and already coexisted with B2B's `alpha.8`, so the mixed graph is
  pre-existing and unchanged by this branch rather than introduced by it.
