# Code review — Refactor/PaymentPayoutOwnerEvent

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `38ad3d1fbeed7249c0dca1342195b3cd7a4c5710`  _(2026-07-25)_

> Range reviewed: `73a5fd8a..38ad3d1f` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **BUG1 — MEDIUM — correctness** — `api/Concertable.B2B/src/Modules/Tenant/Concertable.B2B.Tenant.Infrastructure/Events/TenantCreatedDomainEventHandler.cs:26`
  `TenantCreatedEvent` is still published (production handler + `Concertable.B2B.Seed.Simulator`) but Payment's subscription to it was just deleted in this same PR (`TenantCreatedHandler.cs` removed, `SubscribeTo<TenantCreatedEvent>()` dropped from `Concertable.Payment.Workers/Program.cs`), and nothing else in the codebase subscribes to it. Every tenant creation now writes a second, permanently-unconsumed outbox row/ASB publish. If `PayoutOwnerRegisteredEvent` is meant as a full replacement (per the PR's own stated intent — the deleted `Concertable.Payment/TECH_DEBT.md` entry and the updated docstrings both describe `PayoutOwnerRegisteredEvent` as the operative trigger, no longer mentioning `TenantCreatedEvent`), drop the now-dead `TenantCreatedEvent` publish calls (`TenantCreatedDomainEventHandler.cs:26`, `SeedEventPublisher.cs:34-35`, and the `Publishes<TenantCreatedEvent>()` registrations in `Concertable.B2B.Web/Program.cs:145` and `Concertable.B2B.Seed.Simulator/Program.cs:28`) and the now-unused `TenantCreatedEvent` contract itself. If a future consumer is genuinely planned, say so in a `TECH_DEBT.md` line instead of leaving it silently orphaned. - FIXED (dropped the dead `TenantCreatedEvent` publish calls + registrations and deleted the contract; confirmed no remaining subscribers)

- [ ] **TEST1 — LOW — test coverage** — `api/Concertable.Payment/src/Concertable.Payment.Infrastructure/Handlers/PayoutOwnerRegisteredHandler.cs`
  `PayoutOwnerRegisteredHandler` (new class, new event, new wiring) has no unit test covering it — no `PayoutOwnerRegisteredHandlerTests.cs` exists, and nothing in `Concertable.Payment.Infrastructure.Tests` asserts it calls `ProvisionCustomerAsync`/`ProvisionConnectAccountAsync` with `OwnerId`/`Email`. Add a test pinning that behaviour (the old `TenantCreatedHandler` was equally untested, but this PR is the point the handler and its event contract changed — the new wiring should be pinned, not carried forward unverified).

