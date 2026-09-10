# Code review — Fix/PaymentImageComposition

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `55424f06ff29a9f0e41500a4e5ffe349b2277442`  _(2026-09-10)_

> Range reviewed: `6d0fd42dc..55424f06f` (2 commits, 6 files, +67 −8).
> Status legend: `[ ]` todo · `[~]` in progress · `[x]` done · `[wontfix]` (note why).

## Findings

- [x] **Medium — the commit message and PR body assert a causal chain the evidence does not establish.**
  Both state that the stale `Concertable.Payment.Client` is why `b2b-web` fails, reasoning that "Aspire
  keys discovery by an endpoint's scheme" so `services:payment-web:https:0` is never produced. That
  premise came from `api/Concertable.AppHost.Shared/TECH_DEBT.md` and is not true for this Aspire
  version: `api/Concertable.Frontend.Hosting/FrontendResourcesExtensions.cs:83` reads
  `services__{resourceName}__{endpointName}__0`, keyed by endpoint **name**. `payment-web` declares an
  endpoint named `https`, so that key should exist and the stated mechanism cannot be the explanation.

  Both changes in this branch remain independently justified — the version pin genuinely drifted 36
  versions behind and its client lacks both the `grpc:0` fallback and the h2c insecure-credential
  handling, read directly out of the published assemblies; the duplicate endpoint genuinely breaks the
  Docker binding, proven by stripping it in the `system` harness. What is unproven is that either one
  makes `b2b-web` start. Restate the PR body to claim only what was verified, and say the composed
  outcome is settled by re-pinning `system` and running its qualification, not by this branch.

The native correctness, reuse, simplification, efficiency and error-handling pass is otherwise clean.

`ConcertablePaymentVersion` tracking `$(ConcertableDotNetPlatformVersion)` matches the `packages`
standard, whose whole premise is that the sync PR bumps every service's pin; the two package families
publish in lockstep with identical version lists, and all three consumers — `B2B.Web`, `B2B.Workers`,
`Customer.Web` — build clean at the tracked version, so there is no consumer migration owed. The
property stays inside each service folder, so the per-folder closure rule is intact, and the
`'$(ConcertablePaymentVersion)' == ''` guard preserves the external override the local inner loop uses.

Removing the duplicate endpoint changes a published package's runtime shape rather than its public API,
so it needs the ordinary publish-then-bump release and no accept-new-with-fallback step. The only
consumer of the endpoint being removed was `AddStripeCli`'s containerised branch, repointed here to the
surviving endpoint that its run-mode branch already used; `Concertable/system` binds neither by name.
Dropping the two `ResourceGraphTests` assertions is required by that removal, and the surviving `https`
and `grpc` assertions plus the `ASPNETCORE_HTTP_PORTS` check still pin one endpoint per container port.

No security-sensitive path changed, so no security pass was required and no security marker is stamped.

## Noted, not a finding

`Concertable.Customer.StartupTests.ResourceGraphTests.ProductionGraphAndStrictValidation_AreValid` fails
locally with a `TimeoutException` from `AddStripeCli`'s callback, which waits on a webhook secret that
only exists when Stripe is running. Verified identical on `origin/main` with none of this branch applied,
so it is a pre-existing local-environment limitation rather than a regression here, and CI passes it.
