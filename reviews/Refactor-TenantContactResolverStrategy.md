# Review — Refactor/TenantContactResolverStrategy

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `31851a883`  _(2026-08-28)_

## Incremental pass — f15616cdf..31851a883

Prior watermark `f15616cdf` through `31851a883` spans two `main` merges (129 changed paths), but nearly all
of that is already-reviewed trunk history pulled in by the merges (the tenant-verification closeout,
payments reconciliation, and platform-sync PRs each carry their own review file). The genuine incremental
delta — this branch's own new work, found by intersecting `git diff origin/main...HEAD` against
`git diff f15616cdf..HEAD` — is three files: `ArtistReadRepository.cs`, `VenueReadRepository.cs`, and
`VerificationAdminApiTests.cs`.

**1 finding, fixed on this pass.**

### [x] BOM1 (LOW) — an unintended byte-order mark landed on two edited files

The fix to `VenueReadRepository.cs`/`ArtistReadRepository.cs` was applied via a script using `utf-8-sig`,
which writes a BOM unconditionally regardless of whether the source file had one. Neither file did on
`main`. Pure noise in the diff, no functional effect.

**Addressed 2026-08-28:** stripped in `31851a883`; solution rebuilt to 0 errors afterward.

### No other findings

**The repository fix itself (`ArtistReadRepository.cs`/`VenueReadRepository.cs`).** Casting the projected
struct to `TenantContact?` inside `Select`, before `FirstOrDefaultAsync`, is correct and matches documented
EF Core practice for exactly this gap — `FirstOrDefaultAsync<T>` on a non-nullable value-type projection
returns `default(T)` for zero rows, which the method's `Task<TenantContact?>` signature then silently
promotes to `HasValue = true` via the implicit struct-to-`Nullable<T>` conversion. Verified in an isolated
LINQ-to-Objects harness before and after: `HasValue=False` for an empty source with the cast, `True` without
it. Both leaves of `IVerificationNotifier`'s notify-vs-log branch are otherwise unaffected — the bug was one
layer below the code this PR added, in pre-existing repository code this PR never wrote, only newly exercised
because COV1's own finding was that the absent-contact path had no test.

**The test rewrite (`VerificationAdminApiTests.cs`).** `GetPending_ShouldReturn200_WhenTwoPendingRowsShareTenantType`
reverted to its original assertions — its documented purpose (the `///` comment above it) is pinning
sequential-await concurrency between two pending rows, not contact absence; the earlier COV1 remediation had
bolted an unrelated assertion onto it, which is what this pass removes. The no-notify test now uses
`ArtistManagerNoArtist` without creating an artist, provably contactless via the same fixture the adjacent
`GetPending_ShouldReturn200_WithArtistContactEnrichment` test proves absent by explicitly creating one before
asserting on it — no inference about seed data, verified within the same file.

**EF Core translation risk of the cast, called out rather than silently assumed.** `.Select(x => (T?)new
T(...))` before `FirstOrDefaultAsync` cannot be proven against the real SQL Server provider from this
environment — Testcontainers hit an unrelated Windows path-length DLL-load failure blocking every local
integration-test attempt. The pattern is well-established EF Core practice for precisely this
struct-projection gap, and the untouched enrichment tests already prove the underlying `Where`/`Select`
shape translates and executes correctly for the has-a-row case; only the zero-row path is new. Confidence is
high, not certain — the remote CI run is the actual proof, and its result stands as the record of that.

Watermark advanced past the frozen head `f125ee9` to cover the two remediation commits for COV1 and
RT1, which change only test assertions and the route table — no production behaviour.

Net diff reviewed: `1c0a260..f125ee9` — 28 files, 10 commits.
Status legend: `[ ]` not yet reviewed · `[x]` reviewed (date) · `[~]` in progress (incomplete — re-review).

## Summary

Review complete — all 4 areas `[x]`. **2 findings**, both addressed on this branch.

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

### [x] COV1 (MEDIUM) — the rerouted "no contact, don't notify" decision has no test

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

**Addressed 2026-08-28:** the first attempt used `VenueManagerNoVenue` to assert absence in
`GetPending_ShouldReturn200_WhenTwoPendingRowsShareTenantType` and in a new Approve test; CI showed that
tenant's contact resolving to a value, contradicting both a static read of the venue seed catalog and the
independent `VenueApiTests.GetDetails_ShouldReturn204_WhenNoVenueExists` test, and it could not be
reproduced locally (an unrelated Windows path-length failure blocked Testcontainers in this environment).
Rather than push a second unverified assumption about the same fixture, the concurrency test's assertions
were reverted to their original form (its purpose is sequential-await concurrency, not contact absence), and
`Approve_ShouldReturn204_AndSendNothing_WhenTenantOwnsNoProfile` now uses `ArtistManagerNoArtist` without
creating an artist — the same fixture the adjacent, already-passing
`GetPending_ShouldReturn200_WithArtistContactEnrichment` test proves is contactless by explicitly creating
one before asserting on it. It pins the rerouted decision: Approved status, sent-mail count unchanged.

### [x] RT1 (LOW) — `skill-routes.json` does not route this code to `keyed-strategies`

Running the frozen tree's router over all 28 changed paths returns 16 skills; `keyed-strategies` is not among
them, despite this branch adding the shared `KeyedStrategyBuilder<TKey>`, a `TenantType`-keyed strategy family,
and a second consumer of the pattern. A reviewer following the route table therefore never loads the standard
that actually governs this code, and the write-time hook does not enforce it either.

**Fix:** add a route mapping the keyed-strategy surfaces — `api/Concertable.B2B/src/Concertable.B2B.KeyedStrategies/**`
and `**/Strategies/**` under a module's Application/Infrastructure — to `dotnet-standards:keyed-strategies`.

**Addressed 2026-08-28:** route added to `.agents/skill-routes.json`; the router now returns
`dotnet-standards:keyed-strategies` for 4 files on this branch, where it previously returned none.

## Notes (no action)

- `PendingVerificationDto` changes its wire shape from two nullable scalars (`name`, `email`) to one optional
  group (`contact`), omitted rather than null. Verified no consumer: no reference to the pending-verification
  endpoint or its fields exists anywhere under `app/`, consistent with the standing debt entry recording that
  no admin moderation UI exists yet. Only `VerificationAdminApiTests` reads it, and it was updated.
- `Reunion` moves `0.1.0-alpha.8` → `0.1.0-alpha.14` in B2B and Customer. Auth and Payment stay on `alpha.3`;
  they do not consume B2B contracts and already coexisted with B2B's `alpha.8`, so the mixed graph is
  pre-existing and unchanged by this branch rather than introduced by it.
