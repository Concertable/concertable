# Code review — Refactor/RepoSplit-M4-Closure-Repair

> **This file is a work order, not a discussion.** If you're handed this file, fix the open `[ ]`
> findings directly and report what changed. Tick each `[x]` as you land it. Pause only for a genuinely
> irreversible or ambiguous finding: record its durable disposition, take the safe path, and keep going.

**Review status:** `complete`
**Reviewed up to commit:** `967277325e28392bf034d39dc18f744fc7d59805`  `(2026-09-08)`
**Security-reviewed up to commit:** `967277325e28392bf034d39dc18f744fc7d59805`  `(2026-09-08)`
**Judgment:** `approved`

## Review pass — 2026-09-08 — full

**Candidate base:** `c108226e9` (Platform Contract PR #945 head)
**Candidate head:** `4030eed8d31151f2ef49a17a4f214323f034745d`
**Candidate branch:** `Refactor/RepoSplit-M4-Closure-Repair`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:7eb3c7d474174abbf905e3f31e017631a30f6b3ac74d95f43e3a81020213ac35` `(18 paths)`
**Work-order path:** `reviews/Refactor-RepoSplit-M4-Closure-Repair.md`
**Work-order mode:** `new`
**Pass judgment:** `approved`

This is a **new frozen watermark, not an extension**. The prior artifact on this branch approved an obsolete
base: the branch carried 19 commits, of which 15 were stale rebased copies of the P2/P3/P4 work, confirmed
patch-identical by `git range-diff`. The candidate was rebuilt as the branch's four real commits cherry-picked
onto Platform Contract's head, which produced one plan-document conflict instead of the seven code conflicts a
merge produced.

Lenses applied: the cross-repository closure seam, Payment transport correctness, fail-closed credential
handling, and package-pin consistency.

### Findings

- [x] **M4-1 — MEDIUM — package closure** — `api/Concertable.Auth.Contracts/Directory.Packages.props:5`
  The closure-seam commit introduces `api/Concertable.Auth.Contracts` as an eighth folder carrying a
  `ConcertablePlatformVersion` pin, at `0.1.0-alpha.0.1329` — three versions behind main's
  `0.1.0-alpha.0.1332`. Landing it as written would put two versions of `Concertable.Messaging.Contracts`
  in one dependency graph until the next generated sync corrected it. Fixed by `6d317d348`, which aligns the
  pin with main; `Concertable.Auth.Contracts` rebuilds clean at 1332.

### Verified clean

- **The closure seam is the actual point of this stage and it is complete.**
  `Concertable.Auth.Contracts.csproj` replaces its `ProjectReference` to
  `Concertable.Messaging.Contracts` with a `PackageReference` — the last cross-repository runtime source edge
  in the graph. After this, no deployable project reaches outside its service folder by source.
- **The new pin needs no workflow change.** I checked `bump-platform-version.sh` rather than assuming: it
  discovers pin files with `grep -rlE '<ConcertablePlatformVersion>…' "$root/api" --include=Directory.Packages.props`,
  not a fixed list, so this eighth folder is picked up automatically by every future sync. This is
  deliberately unlike `ConcertablePaymentVersion`, which the sync genuinely does not track and which
  therefore drifts by hand.
- **Payment transport now selects protocol per endpoint rather than globally.** `ConfigurePaymentTransport`
  sets `HttpProtocols.Http2` on the gRPC port and `Http1AndHttp2` elsewhere, replacing a blanket
  `Http1AndHttp2` on all endpoints. The gRPC port comes from `PaymentTransport:GrpcPort`, which
  `AppHostExtensions` emits from `PaymentConstants.GrpcPort`.
- **The duplicated `8081` default is not a maintenance hazard.** It appears in `PaymentConstants.GrpcPort`
  (the AppHost's declaration) and as `?? 8081` in `Payment.Web`, which deliberately does not reference
  `Payment.Hosting` — a runtime service depending on its own AppHost composition package would invert the
  boundary. The env var is the contract and the literal is the standalone fallback. `PaymentTransportTests`
  allocates ports dynamically and asserts the mechanism honours the config, so it pins the behaviour rather
  than a port; the `8081` in its insecure-URL test is incidental to the URL string.
- **Credential handling is fail-closed.** The insecure-HTTP path is opt-in through an explicit
  `PaymentClient__AllowInsecureHttp` variable, and `AddPaymentClient` throws `InvalidOperationException` for
  an `http://` gRPC address otherwise, with a test asserting the throw.
- **Coverage is real, not incidental.** `PaymentTransportTests` is 134 new lines exercising the transport
  end to end over dynamically allocated ports.

### E2E tier

`skip-e2e-ui`, decided on the final head rather than inherited from this branch's obsolete history.

A positive trigger is present: the Payment gRPC transport change is a cross-service wire concern, because
B2B and Customer reach Payment over gRPC. So end-to-end coverage is **not** skipped — API E2E is retained,
and it is the suite that actually exercises that path, driving B2B checkout through to Payment over gRPC
against live main. `PaymentTransportTests` covers the mechanism in the integration tier over dynamically
allocated ports, and `carve-auth` now proves the closure seam builds package-clean.

The browser suites are excluded because nothing in the candidate changes a user-facing flow: they would
re-drive the same B2B-to-Payment path with a UI layer on top of it, adding ~30 minutes and no coverage of
what changed. A transport regression fails API E2E first and faster.

### Validation

- `Concertable.Payment.Web`, `Concertable.Payment.IntegrationTests` and `Concertable.Auth.Contracts` all
  build clean, the last one rebuilt after the pin alignment.
- Exact-head PR CI and the merge queue, including API E2E against live main, are the authoritative gates.
  Delivered as PR #959; the rebuilt branch was force-pushed by Tommy, since the rebuild rewrote its history.

### Extended to the landed M1 stack

After the initial pass the candidate was brought onto the merged M1 stack: `0c7116ecc` merges
`origin/main` at Platform Contract's merge `2b5e8aad0` plus the 1335 pin sync, cleanly with no conflicts, and
`4030eed8d` re-aligned the `Concertable.Auth.Contracts` pin to 1335, and after sync PR #958 landed 1338 the candidate merged main once more and settled that pin at 1338 because sync PR #957 could not reach a file
that exists only on this branch. Nothing else changed and `Concertable.Auth.Contracts` rebuilds clean.

The pin was deliberately not chased while publishes were still in flight, because each M1 merge produced a
new version. With the whole M1 stack landed and sync PR #958 merged, main is settled at 1338 and all nine pin
files now agree. From here `bump-platform-version.sh` reaches this folder by grep like the rest.

### CI gate extension is correct for the seam

`carve-auth` now archives `api/Concertable.Auth.Contracts` into the carved tree and adds it to `CarveAuth.slnx`,
so the package-clean gate proves `Auth.Contracts` restores and builds from the feed alongside the Auth
runtime. That is the right CI change to accompany the closure seam: `Auth.Contracts` has just joined Auth's
deployable closure, and without this the carve job would not cover it. The `split-inventory` comment is
widened to say the gate covers runtime as well as test-tier cross-repository edges.

### Security pass

`.github/workflows/` is a security-sensitive path, so this candidate requires a security marker. Its workflow
diff carries no security-relevant change: no `permissions`, `secrets.*`, `GITHUB_TOKEN`, `pull_request_target`
or registry-login edits — only a comment and the two `carve-auth` lines above. The one credential-adjacent
change in the candidate moves in the fail-closed direction: an insecure `http://` gRPC address now throws
unless `PaymentClient__AllowInsecureHttp` is set explicitly, where previously it was accepted.

## Review pass — 2026-09-08 — incremental

**Candidate base:** `5f1c672c490da68cb41751f3b16f1ca2803b0da8`
**Candidate head:** `8090cc46f1cbdba29923c02008104c7ad81b3707`
**Candidate branch:** `Refactor/RepoSplit-M4-Closure-Repair`
**Candidate scope:** `all`
**Candidate path-set:** `sha256:42268d705c85275ed9461f5f5285324a519ad0a406ed387c6b20a4a65f09e56b` `(11 paths)`
**Candidate bundle identity:** `sha256:7a7974aa902b86ba62c181db3f3e72123bedc6cc12555d5276e0d40cc11e763a`
**Work-order path:** `reviews/Refactor-RepoSplit-M4-Closure-Repair.md`
**Work-order mode:** `append`
**Pass judgment:** `approved`

Covers the repair of the Payment gRPC regression that ejected PR #959 from the merge queue in merge-group
run `34256886321`, plus the two tech-debt entries it produced. Lenses applied: the E2E service-discovery
substitution seam, Kestrel protocol selection, test-tier placement, and duplication in the touched files.

### Findings

No open findings. One duplication found in this delta was fixed within it and is recorded below.

### Root cause, confirmed rather than assumed

The four failing tests all died on `POST .../checkout` with a Polly timeout inside
`Http2Connection.Http2Stream.WaitForDataAsync`. The chain was verified end to end, not inferred:

- **The `grpc` endpoint reaches consumers.** Resolving the real `Concertable_B2B_AppHost` graph shows both
  `b2b-web` and `workers` receiving `services__payment-web__grpc__0` — emitted by the `WithReference` the
  AppHost already had, because `AddPaymentWeb`'s container overload now declares a third endpoint. The same
  resolution also shows the graph emits **no** `services__payment-web__https__0` at all: the container's
  `https`-*named* endpoint carries an `http` scheme, so it lands as `http__0`. The pinning code's key list was
  therefore already out of step with the container's endpoint roster.
- **The advertised host never starts.** `SubstituteE2EProject` puts the container on `WithExplicitStart()` and
  runs the Payment-owned project beside it on the single pinned `ASPNETCORE_URLS`.
- **The client prefers exactly the broken key.** `AddPaymentClient` reads `services:payment-web:grpc:0` first
  and falls back to `https:0` — a preference introduced by `d168522f2` in this same branch. Before it, the
  client read `https:0` only, which the harness did pin, which is why the path was green until M4.
- **A DCP proxy with no backend accepts and hangs**, which is why this presented as a 30-second timeout in
  `WaitForDataAsync` rather than a connection refusal.

### The fix is placed where the asymmetry is

`PinPaymentDiscovery` repoints **every** `services__payment-web__*` key already present in the consumer's
environment, deriving the list from what the reference actually emitted rather than restating a key list that
had already drifted once. B2B web, B2B workers and Customer web all route through it; Customer has no other
Payment consumer in its AppHost, which was checked rather than assumed.

`ContainerBackedPinningTests` locks the invariant: it composes the real container overload, references it from
a consumer, and asserts the `grpc` key is present **and** that every advertised key resolves to the pinned
host. Reverting the helper to a single-key pin fails it.

### The `?? 8081` default is removed, and the earlier pass's reasoning on it is superseded

The prior pass recorded the duplicated `8081` as "not a maintenance hazard — the env var is the contract and
the literal is the standalone fallback". The fallback claim does not hold: `Concertable.Payment.Web.csproj`
sets `PaymentTransport__GrpcPort` as a `ContainerEnvironmentVariable` on the image itself, so a standalone
container already carries the value and the literal defended nothing. What it did do was make **any**
project-hosted endpoint that happened to bind 8081 HTTP/2-only, silently breaking REST. `grpcPort` is now a
genuine `int?` and an unset value forces no endpoint to HTTP/2.

`ConfigurePaymentTransport_NoConfiguredGrpcPort_LeavesTheContainerGrpcPortHttp1Capable` was confirmed to fail
against the old `?? 8081` before being kept.

### Kestrel does not serve h2c by prior knowledge on a cleartext `Http1AndHttp2` endpoint

Established empirically while writing the above: a gRPC call to such an endpoint is rejected with
`HTTP_1_1_REQUIRED`. This is why the container topology needs a genuinely separate HTTP/2-only listener on
8081, and why a project-hosted Payment carries gRPC on its **TLS** endpoint via ALPN instead. That distinction
is now stated in `ARCHITECTURE.md` and asserted by
`AddPaymentWeb_ProjectOverload_AdvertisesNoDedicatedGrpcTransport`, which converts the project overload's
previously accidental gap into a stated contract.

### Duplication found and fixed in this delta

The new pinning test needed the environment callbacks awaited, which briefly left
`ContainerBackedPinningTests` with two helpers differing only in that. `8090cc46f` folds the four existing
callers onto the awaited one and deletes the synchronous copy, so an async environment callback cannot go
silently unresolved in any of them. Suite green at 9/9 after the fold.

### Validation

- `Concertable.Payment.ArchitectureTests` 14/14, `Concertable.Search.E2ETests.Helpers.UnitTests` 9/9,
  `PaymentTransportTests` 3/3.
- `Concertable.B2B.E2ETests` and `Concertable.Customer.E2ETests` both build clean.
- `Concertable.B2B.ArchitectureTests` is 34/35 locally: `AppHost_ProductionGraphAndStrictValidation_AreValid`
  fails with a bare `TimeoutException` **only on a machine that has `Stripe:SecretKey` in user secrets**,
  because `AddStripeCli` then registers a `WithEnvironment` callback that blocks 60s waiting for the CLI's
  webhook secret. CI has no such secret and the job passed on run `34247389726`. Not caused by this delta;
  logged in `api/Concertable.Payment/TECH_DEBT.md`.
- The merge queue's `e2e-api-tests` remains the authoritative gate for the regression itself.

### Security pass

No security-relevant change in this delta. No `.github/workflows/` path is touched, no permission, secret or
token handling moves, and the fail-closed `PaymentClient:AllowInsecureHttp` guard is untouched — the E2E keys
this pass repoints all resolve to the existing `https://` pinned endpoint, so the guard is not newly bypassed.

### Tech debt recorded

- `api/TECH_DEBT.md` — architecture-test classes that restate their own project name across Payment, Customer,
  Search, Auth and the umbrella AppHost, to be resolved by splitting per subject as B2B already is.
- `api/Concertable.Payment/TECH_DEBT.md` — `AddStripeCli`'s blocking environment callback making a host graph
  unresolvable without a live Stripe CLI.

### Extended over the main merge

`967277325` merges `origin/main`, which had moved the per-service host-graph classes out of
`*.ArchitectureTests` into a new `*.StartupTests` tier. Four conflicts, all reviewed:

- **B2B / Customer `ResourceGraphTests`** — M4's Payment transport assertions carried onto main's class,
  keeping main's required `scheme` parameter and adding `targetPort` so the 8081 grpc listener is assertable.
  `PublishGraphWithStripeCli_IsValid` becomes `async Task` because M4's fail-closed `AllowInsecureHttp` check
  awaits the resolved environment.
- **Search `ResourceGraphTests`** — rename detection paired the deleted `PaymentArchitectureTests.cs` with
  this file and silently pulled the project-overload transport test into it, which does not compile there.
  Search is restored byte-identical to `origin/main`, and the test is re-homed as
  `ProjectHostedPaymentWeb_AdvertisesNoDedicatedGrpcTransport` in `Concertable.Payment.StartupTests`.
- **`eng/repository-split/inventory.json`** — generated, so regenerated with `inventory.py` rather than
  hand-resolved; `--check` is clean.

The `api/TECH_DEBT.md` naming entry was rewritten rather than left as landed: the startup-tier split already
resolved most of what it described, and only `PaymentContractReferenceTests`,
`PaymentPublishedPackageReferenceTests` and `ReunionArchitectureTests` still repeat their project.

Post-merge: `Concertable.Payment.StartupTests` 7/7, `Concertable.Search.StartupTests` 6/6,
`Concertable.B2B.StartupTests` 12/13 and `Concertable.Customer.StartupTests` 7/8 — each single failure being
the `AddStripeCli` local-secret block, proven by re-running B2B with `Stripe__SecretKey` cleared, which passes
4/4. No security-relevant change in the merge resolution.
