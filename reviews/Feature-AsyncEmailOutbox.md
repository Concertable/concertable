# Code review — Feature/AsyncEmailOutbox

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `462f2883b10f8f4909fe23c77c2b725060865d01`  _(2026-07-20)_

> Range reviewed: `8f4be3ab..462f2883` (2 commits).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Verdict

**No blocking issues.** Phase 1 is a clean additive expand of `Concertable.Shared.Email` plus the
service-scoped queue-name change that Phase 2 depends on. Correctness, microservice isolation, and C#
conventions all check out; the one transitional item below is already tracked in the plan, so it's
recorded here only for completeness.

Checked correctness, microservice isolation, module boundaries, seeding, and C# conventions.

### Why the two riskiest-looking changes are actually safe

- **New command handlers can't break any consumer's startup.** `AddSharedEmail` only DI-registers
  `IIntegrationCommandHandler<SendEmailCommand>` / `<SendVerificationEmailCommand>`. The ASB receiver
  provisions a queue processor per `MessageTypeRegistry.RegisteredCommandTypes`, and that set is
  populated **only** by explicit `registry.HandleCommand<T>()` — never by DI handler registration. No
  service calls `HandleCommand` for the email commands yet, so the surface is inert until Phase 2.
- **`QueueNameFor` service-scoping + `PaymentTopology` dual-queue is correct expand handling.** Payment
  references `Concertable.Messaging.AzureServiceBus` as a pinned **PackageReference**, so it keeps
  computing the old unscoped `command-processstripewebhookcommand` until platform-sync re-pins it, then
  switches to `command-concertable-payment-processstripewebhookcommand`. Both queues are provisioned so
  the webhook flow works on either side of the pin flip; Payment.Web + Payment.Workers share one
  repo-wide version, so sender and receiver never disagree. Removal of the old line is tracked in the
  plan's Phase 2 ("scope Payment's queue name to only the new value once repinned").

## Findings

- [x] **CV1 — LOW — convention (defensive default)** — `api/Concertable.Messaging/Concertable.Messaging.AzureServiceBus/Extensions/ServiceCollectionExtensions.cs`
  FIXED (Tommy chose fail-fast-at-registration): `AddAzureServiceBusTransport` now applies the
  `configure` action to a probe instance and throws `InvalidOperationException` if `ServiceName` is
  empty, so a host that forgets to set it dies at composition/boot rather than silently producing a
  malformed `command--<type>` queue name. `ValidateOnStart` was not used — the lean transport package
  references only `*.Abstractions` + `Options`, not `Microsoft.Extensions.Hosting`; the eager check
  achieves the same fail-at-boot without pulling in a new dependency. Build green; 48/48 messaging unit
  tests pass.

  _Original finding: with the interpolation now embedding `ServiceName`, an unset `ServiceName` (default
  `""`) yielded a malformed `command--<type>` queue name instead of a loud failure — never hit in
  practice (all seven hosts set it), but the repo's "don't default away a failure" rule prefers a guard._
