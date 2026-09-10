# Code review — Fix/PaymentImageComposition

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed — don't re-present them as options or ask which to do.
> Tick each `[x]` as you land it. Pause only for a genuinely irreversible/ambiguous finding: flag it
> in one line, take the safe path, keep going.

**Reviewed up to commit:** `8c9f77132` _(merge of `origin/main`)_  _(2026-09-10)_

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

  **Closed 2026-09-10** by the incremental pass at the end of this file: the mechanism is established
  from the Aspire source and the causal chain does hold, via the client's `grpc:0` preference rather
  than via the key the PR body named.

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

## Incremental review — 2026-09-10

> Range reviewed: `203792dae..48210df77` (1 commit).

No findings. Records the corrected discovery-key premise against the entry that carries it, keeps that
entry open because its symptom is confirmed, and marks both of its proposed remedies as reasoned from
the wrong mechanism. Documentation only; no runtime path changed.

## Incremental review — 2026-09-10 (merge to updated base)

> Range reviewed: `48210df77..8d23aea16` (1 merge commit, 2 files).

No findings. Carries the branch onto `aa6492485`, which had advanced through `#989` and the
`0.1.0-alpha.0.1370` platform sync. Both conflicts are the same two lines in `B2B` and `Customer`
`Directory.Packages.props`: the base's `1366 -> 1370` bump is taken, and this branch's single tracking
`ConcertablePaymentVersion` replaces the base's two-line literal pin. Nothing else in the merge touches
this branch's files.

The earlier claim that CI passes
`Concertable.Customer.StartupTests.ResourceGraphTests.ProductionGraphAndStrictValidation_AreValid` was
recorded before any CI run on this branch existed. Run `34486226594` on `0f96513dc` failed that test on
the `AssertImageEndpoint(..., "http", ...)` call, which `55424f06f` then deleted; no run ever fired for
`55424f06f..dadc14d04`, so the fix has never been exercised remotely. This merge push is the first head
that carries both halves, and its run is the gate.

## Incremental review — 2026-09-10 (discovery-key mechanism established)

> Range reviewed: `84795c778..HEAD` (1 commit, 1 file).

No findings. This closes the Medium finding above by supplying the mechanism that finding said was
missing, read out of the Aspire source rather than inferred from Concertable's own code.

`ResourceBuilderExtensions` in Aspire 13.3.2 builds each service-discovery variable as
`services__{resource}__{endpoint.IsHttpSchemeNamedEndpoint ? endpoint.Scheme : endpointName}__{index}`,
and `EndpointReference.IsHttpSchemeNamedEndpoint` is true for exactly the endpoint names `http` and
`https`. An endpoint named `https` is thus keyed by its **scheme**, which `WithHttpEndpoint` sets to
`http`. `services:payment-web:https:0` is therefore never produced by any composition. The finding's
counter-evidence, `Concertable.Frontend.Hosting`, hand-writes name-keyed variables and never reaches
`WithReference`, so it was never evidence about this path.

The version pin is consequently load-bearing after all, for a reason neither the original PR body nor
the finding stated: from `0.1.0-alpha.0.1364` `AddPaymentClient` reads `services:payment-web:grpc:0`
before `:https:0`, and `grpc` is not an http-scheme name, so Aspire does produce that key. The image
that failed carried the `0.1.0-alpha.0.1330` client, provable from the error text alone — it names one
key, where every version since names two. The endpoint removal remains independently justified on the
port-binding evidence already recorded.

Both statements in the branch that reasoned from the wrong premise are corrected: the debt entry here,
and the PR body.
