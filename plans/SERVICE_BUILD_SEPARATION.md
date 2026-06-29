# Plan: Service build-separation (each backend service builds from its own package closure)

**Goal.** Make every backend service compile and test from **its own dependency closure** —
consuming shared-platform and cross-service code as **published private NuGet packages**, not via
`ProjectReference`s that reach into other folders. When this lands, carving any service into its own
tree (or repo) produces a build that *restores and compiles*, instead of the folder-copy that fails
today.

**Why now (the trigger).** The "microservice" boundaries are real at runtime (verified: no data
service references another's `.Domain`/`.Application`/`.Infrastructure`; cross-service comms are
`*.Contracts`/events only; separate per-service databases). But the **build** is monolithic: every
service pulls Kernel/Messaging/contracts via `..\..\..\` `ProjectReference`, and there are **zero
`PackageReference`s to any `Concertable.*` package** anywhere in `api/`. Proven: `git subtree split`
of `api/Concertable.B2B` + `dotnet build` ⇒ `Build FAILED, 9× MSB3202 project-not-found`. This is the
documented-but-never-executed half of the split mapping in `api/ARCHITECTURE.md`
(`ProjectReference` today → private NuGet "later"). Independent deployment and any repo split both
sit on top of this; it goes first.

**Decisions locked** (with the user):
- **Feed:** GitHub Packages (already on GitHub) — `https://nuget.pkg.github.com/Concertable/index.json`.
- **Source stays in the monorepo** — separate the *build closures* in place. Moving services to
  their own repos is a later, optional, org-driven step; it is **out of scope here**. (But the
  endgame *is* separate repos — every decision below is shaped so a future split is a no-op.)
- **Phased by boundary stability** — most-stable contract first, churny shared core last.
- **Versioning = MinVer** (git tag + commit height). Lockstep across all packages while this is one
  repo; becomes natural independent per-repo versioning the moment a service splits out. Chosen over
  CI-build-number (encodes no semver intent) and Nerdbank.GitVersioning (its per-path versioning only
  earns its config cost *inside* a monorepo — pointless once repos are separate).
- **Per-service build closures — NEVER repo-root config.** Each service folder + the shared-platform
  folder carries its **own** `Directory.Packages.props` (CPM, `ManagePackageVersionsCentrally`),
  `Directory.Build.props`, and `nuget.config`, so the folder is self-contained and carve-ready. **Do
  not add a repo-root `Directory.Packages.props` (the "monorepo idiom").** Why this is the trap to
  never re-fall-into: every phase's gate carves a service with `git subtree split
  --prefix=api/Concertable.X` and builds it standalone, and a split takes *only that folder* — so any
  `api/`-root config (a root CPM file, today's `api/Directory.Build.props`) is left behind and the
  carve fails to restore. This is already why `mirror.yml`'s split produces non-building repos.

**Out of scope (explicitly):** the deployment pipeline (containers/registry/host — there is none
today; that's the *next* effort after this), the frontend, and any repo move.

## Branch & process

- Branch off **`master`** as `Feature/ServiceBuildSeparation` (this plan is unrelated to the
  in-flight `Refactor/UnifyReadMappingPattern`; do not base it on that branch).
- No model changes in any phase ⇒ **no `initial-migrations.ps1`**.
- Per-phase gate: `dotnet build api/Concertable.slnx` green + the affected service's unit/integration
  tests (via `integration-debug`). E2E only on the massive/risky phases (B2B, Customer) per
  `plans/CLAUDE.md` — not by reflex.

## The honest caveat this plan is shaped around

The shared core is the **busiest code in the repo** — `Concertable.Kernel` 37 commits and
`Concertable.Messaging` 35 in the last 3 months (≈ all-time; the structure is only ~3 months old),
and **47% of last month's commits touched ≥2 services**. The instant that core becomes a package,
every change to it is a publish-then-consume cycle — *even in local dev*. So:
- Package the **stable** boundaries first to prove the rails cheaply (`Auth.Contracts` = **0 commits**,
  then Payment, whose contract barely moves).
- For the churny core, use a **hybrid inner loop**: `ProjectReference` for local multi-service dev,
  `PackageReference` resolved in CI / standalone builds (e.g. an MSBuild prop toggled by an env var /
  build flag). This keeps the cross-cutting inner loop fast while still proving standalone builds.

## Packaging classification (what becomes what)

- **Private NuGet (published):** shared platform — `Concertable.Kernel`, `Concertable.Contracts`,
  `Concertable.Messaging.*`, `Concertable.DataAccess.*`, `Concertable.ServiceDefaults`,
  `Concertable.Shared.Api`, `Concertable.Shared.{Blob,Email,Geocoding,Imaging,Notification,Pdf}.*`,
  `Concertable.Seed.{Shared,Identity}`, `Concertable.Testing(.Integration)`; **plus** cross-service
  contracts — `Auth.Contracts`, `B2B.{Artist,Concert,Venue,User,Tenant}.Contracts`,
  `B2B.Seed.Contracts`, `Customer.Review.Contracts`, `Payment.Contracts`, `Payment.Client`.
  _(`Shared.Api` was added to this set in Phase 3 — it's shared Web/API infra, **not** a service-internal
  `*.Api`, and every service's Api/Web layer references it; Phase 2a had wrongly parked it as non-published
  only because Auth happened not to reference it.)_
- **Stays source / build-from-source:** every service-internal `*.Domain/.Application/.Infrastructure`
  and module `*.Api`, each service's `Seed.Infrastructure`, and the **AppHosts** (the dev-composition
  layer — see note below).
- **Container (later, deployment effort):** the deployables (`Auth`, `B2B.Web`, `B2B.Workers`,
  `Customer.Web`, `Search.Web`, `Search.Workers`, `Payment.Web`, `Payment.Workers`) and the
  `B2B.Seed.Simulator`. Not this plan.

**Composition-layer note (AppHosts + the full-stack E2E harness).** Two layers legitimately cross
folder boundaries because their whole job is to compose the entire topology, and both stay
monorepo-bound:
- **AppHosts** (dev-composition) reference sibling deployables to orchestrate the dev topology (e.g.
  `B2B.AppHost` → Auth, Payment.Web/Workers, Search.Web/Workers; `Customer.AppHost` →
  `B2B.Seed.Simulator`).
- **The full-stack E2E test harness** (test-composition) boots every service together and drives the
  real cross-service flow, so B2B's E2E projects reference `Payment.E2ETests.Helpers`,
  `Search.E2ETests.Helpers`, and `Payment.Seed` (the seeded Stripe test-mode payout IDs the payment
  assertions read). Those are owned by the other services by design.

A service's *deployable closure* (Web/Workers + modules) must be package-clean; its AppHost and its
E2E harness need not be — until the deployment effort turns those refs into `AddContainer` / a
containerised E2E topology.

---

## Phase 0 — Remove the cross-service source leaks (no packaging yet) — ✅ DONE

These edges drag another service's **source** into a service's closure, so they'd poison its package
boundary. They are violations regardless of this plan.

- ✅ ~~Land `PAYMENT_AGNOSTIC_AUDIT.md`~~ — **already landed** by the `Feature/payment-proxy` merge.
  On `master` the dead `Payment → B2B.{Contract,Concert,User}.Contracts` edges, the dead
  `IStripeValidation*` graph, the `ConcertPayee` projection + `payee_id` re-route, the stale
  `DataAccess.Application` domain refs/GlobalUsings, and the B2B↔Payment reverse leak are all gone.
  (That plan file is deleted in this commit — its work is fully shipped.)
- ✅ Removed `B2B.Web → Payment.Seed` — it was only an orphaned E2E `StripeE2EAccountResolver`
  registration that nothing in B2B's runtime resolved.
- ✅ Replaced `B2B.IntegrationTests.Fixtures → Payment.Infrastructure` with a Payment Client/contract
  test seam: escrow verification moved from reading real `PaymentDbContext`/`EscrowEntity` rows to
  asserting the calls B2B makes against a **recording `IEscrowClient` mock** (`MockEscrowClient.Holds`
  records `(payer, payee, amount, bookingId)`) — testing B2B's behaviour at the client boundary, not
  Payment's persistence; the real-row check (right payee in `payment.Escrows`) is owned by B2B E2E
  (`ConcertDraftTests`). 6 dead Stripe-internal mocks deleted (no consumer once Payment runs
  out-of-process); `MockStripeApiClient` → plain helper; `UseFailingPayment` re-routed to a failing
  `IEscrowClient`; csproj now references `Payment.Client` + `Payment.Contracts` (+ `Stripe.net`).
- ✅ **Gate passed:** full build green (0 errors); Payment + B2B unit (149) and B2B integration (129)
  green. No E2E (no behavior change).

> **Finding carried forward — a *new* Payment→B2B edge postdates the audit.** The payment-proxy
> refactor added a live compile edge `Payment.Infrastructure → B2B.Tenant.Contracts` (the
> `TenantCreatedEvent` payout-provisioning subscription in `TenantCreatedHandler`). It is a
> `*.Contracts` reference, **not** a source leak, so it is correctly out of Phase 0's scope — but it
> means the Phase 3 note "Payment depends only on shared + `Auth.Contracts`" no longer holds. Resolve
> it when packaging Payment (Phase 3): either consume `B2B.Tenant.Contracts` as a package, or re-route
> the subscription through a Payment-owned/generic event (the audit's pattern E).

## Phase 1 — Stand up the packaging rails (publishes nothing consumed yet) — ✅ DONE

- ✅ **Per-folder `nuget.config`** in all 12 folders: `<clear/>` + nuget.org + the GitHub Packages
  feed, with **package source mapping** (`Concertable.*` → github only, `*` → nuget.org) as a
  dependency-confusion guard. Auth via `%GITHUB_PACKAGES_TOKEN%`. Self-contained (carve-safe).
- ✅ **Per-folder `Directory.Packages.props`** (CPM) in all 12 folders; stripped inline `Version=`
  from 164 csproj (versions centralized per folder, derived from prior values; intra-folder conflicts
  reconciled to the higher pin). **Per-folder `Directory.Build.props`** (`NuGetAudit`/`NoWarn` +
  Meziantou via `GlobalPackageReference`); root `api/Directory.Build.props` deleted. **No repo-root
  version/build config** (Decisions locked — a root file breaks the carve gate). _(commit 1)_
- ✅ **MinVer** (`GlobalPackageReference`, `MinVerMinimumMajorMinor=0.1`, tag prefix `v`) + shared
  package metadata in `Shared/Directory.Build.props`, with publishing **opt-in via
  `<IsPackable>true</IsPackable>`** (default `false`, so a solution-wide `dotnet pack` yields only
  intended packages). `Concertable.Kernel` was the first opted-in package; **`Concertable.Contracts`
  is opted in alongside it** because Kernel `ProjectReference`s it — without that, Kernel's package
  would declare a feed-absent `Concertable.Contracts` dependency and `verify-restore` fails NU1101
  (big-review BUILD1). Both pack at the same lockstep MinVer version — proven locally: `dotnet pack`
  → `Concertable.Kernel` + `Concertable.Contracts` at `0.1.0-alpha.0.529`, no NU1507.
  _(commit 2; Contracts opt-in added in the BUILD1 fix)_
- ✅ **CI workflow** `.github/workflows/publish-packages.yml`: packs every `IsPackable` project,
  pushes to the feed (`GITHUB_TOKEN`, `packages: write`), then a `verify-restore` job restores
  `Concertable.Kernel` into a fresh consumer from the feed. Triggers: push to `master`
  (path-filtered) + `workflow_dispatch`.
- ✅ **Gate — CI run (passed):** the publish workflow ran on the PR #58 merge to `master` (run
  `28170887820`, 1m21s) — **both jobs green**: `publish` packed + pushed `Concertable.Kernel` +
  `Concertable.Contracts` (`0.1.0-alpha.0.533`, lockstep) to the org feed, and `verify-restore`
  restored `Concertable.Kernel` (+ its `Concertable.Contracts` dependency) into a fresh consumer
  from the feed — **NU1101 did not occur**, the rails work. GitHub Packages is enabled for the
  `Concertable` org. (Local dev consuming `Concertable.*` later needs a `GITHUB_PACKAGES_TOKEN` PAT
  with `read:packages`.) Zero behavior change; no E2E.
- **Note:** the *full* publishable-set marking (Auth.Contracts + the rest of the shared platform) is
  Phase 2 — Phase 1 proves the rails with just `Kernel` (+ its leaf dependency `Contracts`, which the
  BUILD1 fix pulled forward; the rest of the shared platform is still Phase 2).

> **Publishing model & repo-split notes (worked out during Phase 1 build-out).**
> - **What publishes:** only `IsPackable=true` *library* projects — the shared platform + the thin
>   `*.Contracts` / `Payment.Client` packages. **Never** the deployable apps (`*.Web`/`*.Workers`) or a
>   service's `Domain`/`Application`/`Infrastructure`/`Api` internals — those *consume*, never publish.
> - **Cadence:** continuous, not one-shot. Every merge to `master` touching a publishable folder
>   re-packs at a new MinVer version (commit-height bumps; a `v*` tag pins a real version).
> - **Where:** the **org-scoped** GitHub Packages registry `nuget.pkg.github.com/Concertable` (not a
>   repo) — shared by every repo in the org, so it survives the eventual split unchanged.
> - **What does NOT survive the split automatically:** (1) `publish-packages.yml` is repo-root-only (a
>   GitHub Actions requirement), so a `subtree split` leaves it behind — each separated repo gets its
>   own smaller publish workflow (platform repo publishes the platform; a service repo publishes only
>   its own contracts; consume-only repos need none). (2) Cross-repo *restore* needs the org packages
>   made **internal** (or a `read:packages` PAT — the `GITHUB_PACKAGES_TOKEN` placeholder already in
>   each `nuget.config`), because a repo's `GITHUB_TOKEN` only reads its own packages.

## Phase 2 — Prove the mechanism on the most stable boundary (Auth + shared platform) — ✅ DONE

**Sequencing — publish *before* you can consume (this is two sub-steps, not one).** Phase 1 published
only `Kernel` + `Contracts`. Auth cannot `PackageReference` the shared platform until those packages
exist on the feed, so:

- **2a — publish the rest of the shared platform. — ✅ DONE.** Flipped `<IsPackable>true</IsPackable>`
  on the 24 remaining shared-platform libs — `Auth.Contracts`;
  `Messaging.{Contracts,Domain,Application,Infrastructure,AzureServiceBus}`;
  `DataAccess.{Application,Infrastructure}`; `ServiceDefaults`;
  `Shared.{Blob,Email,Geocoding,Imaging}.{Application,Infrastructure}`;
  `Shared.Notification.Infrastructure`; `Shared.Pdf.{Application,Infrastructure}`; `Seed.{Shared,Identity}`;
  `Testing(.Integration)` — joining the Phase-1 `Kernel`+`Contracts` for **26 packages total**. The four
  folders that *started* publishing (`Concertable.Auth.Contracts`, `Concertable.Messaging`,
  `Concertable.DataAccess`, `Concertable.ServiceDefaults`) gained MinVer + package metadata in their **own**
  `Directory.Build.props` / `Directory.Packages.props` (mirroring `Shared/`; per-folder, carve-safe — no
  repo-root config). **Wider BUILD1 trap closed and proven:** every packable project's `ProjectReference`s
  all land inside the published set (the two non-published `Shared/` libs — `Shared.Api`, `Seed.Infrastructure`
  — are referenced by nobody in the set), confirmed by `dotnet pack api/Concertable.slnx` → exactly the 26
  packages at lockstep `0.1.0-alpha.0.527`, then auditing every `.nuspec`: **no package declares a feed-absent
  `Concertable.*` dependency**. `verify-restore` in `publish-packages.yml` was strengthened from restoring
  just `Kernel` to restoring the **whole 26-package closure** into a fresh consumer, so a future BUILD1
  regression surfaces as NU1101 in CI. **Gate passed:** `dotnet build api/Concertable.slnx` green (0 errors);
  shared-platform unit tests green (Kernel 14, Messaging 40, Messaging.AzureServiceBus 8); zero behaviour
  change ⇒ no E2E. **✅ Shipped to the feed:** merged via PR #59 (merge commit `ab2c6473`);
  `publish-packages.yml` ran green — all 26 packages published to the org feed at lockstep
  **`0.1.0-alpha.0.526`**, and the strengthened verify-restore restored the full closure from the *live* feed
  (so the wider BUILD1 trap is proven against real GitHub Packages, not just locally). _(The post-merge `Test`
  red-X on `ab2c6473` is an unrelated Docker Hub image-pull timeout at Testcontainers fixture startup
  — `registry-1.docker.io ... context deadline exceeded`; the identical tree passed the merge-queue `Test`,
  so it is an infra flake, **not** a 2a regression. The `Mirror` red-X is the known pre-existing failure this
  whole effort fixes.)_ **2b can now proceed.**
- **2b — flip Auth to consume them. — ✅ DONE.** Swapped all **13** of `Concertable.Auth`'s
  `ProjectReference`s (every one escaped `api/Concertable.Auth/`) for `PackageReference`s —
  `Auth.Contracts`, `Seed.{Shared,Identity}`, `DataAccess.{Application,Infrastructure}`,
  `Messaging.{AzureServiceBus,Infrastructure}`, `ServiceDefaults`,
  `Shared.{Blob,Email,Geocoding,Imaging,Pdf}.Infrastructure` — pinned in Auth's **own**
  `Directory.Packages.props` to the live lockstep feed version **`0.1.0-alpha.0.526`** (re-checked the
  feed before pinning). Only the 13 *direct* refs need a `PackageVersion`; transitive `Concertable.*`
  resolve to the same version via the packages' own dependency metadata (no transitive pinning needed).
  Even the in-monorepo Auth build now consumes packages — fine because 2a is published.
- **✅ Carve proven standalone.** `git archive HEAD:api/Concertable.Auth` (the Phase-0 carve repro,
  tracked files only) → restore-from-feed → `dotnet build` is **green (0 errors)**, built **outside the
  repo tree** so no monorepo config can leak in (verified: no `Directory.Build.props`/
  `Directory.Packages.props`/`nuget.config` at repo-root or `api/` — Auth's own three are
  self-contained). The carve takes only `api/Concertable.Auth/`; its sibling `Concertable.Auth.Contracts`
  **and** the whole shared platform resolved as packages from the feed — the Phase-0 `9× MSB3202
  project-not-found` is gone. (Used `git archive`, not `git subtree split`: the split rewrites the
  folder's whole ~1300-commit history and is far too slow for a gate; archive extracts the identical
  tracked tree at HEAD instantly.) _(Aside: the carved tree emits more `MA0004` style warnings than the
  in-repo build because the repo-root `.editorconfig` isn't inside the Auth folder — cosmetic, 0 errors,
  no `TreatWarningsAsErrors`; editorconfig distribution is a repo-split concern, not a build-closure one.)_
- **✅ CI check added.** New `carve-auth` job in `.github/workflows/test.yml` runs the same
  `git archive` carve and restores from the feed with the repo `GITHUB_TOKEN` (same technique as
  `publish-packages.yml`'s `verify-restore`); a re-introduced escaping `ProjectReference` now fails CI
  there. The `build`, `carve-auth`, **and both merge-queue E2E jobs** (`e2e-api-tests`, `e2e-ui-tests`)
  carry `GITHUB_PACKAGES_TOKEN: ${{ secrets.GITHUB_TOKEN }}` + `packages: read` — the E2E jobs need it
  because their `dotnet test`/`build` restores the AppHosts, which `ProjectReference` Auth's now
  feed-only packages. (`carve-auth` itself is **not** yet a required check in the merge-queue ruleset,
  so a re-introduced escaping ref fails it without blocking merge — wire it into the ruleset in a later
  hardening pass.)
- **✅ Gate passed:** `dotnet build api/Concertable.slnx` green (0 errors) + standalone carve build
  green. Auth has **no** unit/integration test project (single deployable csproj, behaviour
  E2E-covered) — no Auth tests to run; zero behaviour change ⇒ no E2E. Done on branch
  `Feature/ServiceBuildSeparationPhase2b` (one branch → one PR → one merge). **This completes Phase 2.**
  Phases 3–7 remain, so this plan stays.
- **Local prereq (now repo-wide, not Auth-only):** a `GITHUB_PACKAGES_TOKEN` PAT with `read:packages`
  in the env. Because the root/B2B/Customer AppHosts `ProjectReference` Auth, **every** backend dev who
  builds any of those solutions needs the PAT now — not just devs touching Auth. Documented in the root
  `README.md` prerequisites; CI uses the repo `GITHUB_TOKEN`.

## Phase 3 — Payment standalone

**Decision on the `Payment.Infrastructure → B2B.Tenant.Contracts` edge (the Phase 0 finding): option (a) —
publish `B2B.Tenant.Contracts` and consume it.** Confirmed with the user. Payment's only compile dependency
on it is `TenantCreatedEvent` (`TenantCreatedHandler` + its DI registration + the Workers `.SubscribeTo<>`);
the event is consumed by **nobody but Payment**. Chosen over (b) re-routing to a Payment-owned/generic event
because: (b) is a runtime re-architecture of the payout-provisioning flow (out of this plan's "separate build
closures in place" scope); it touches **B2B's** publish path + Seed.Simulator + seeders, which belong to the
deferred **Phase 5** ("churny core last"); and it changes a wire contract on the E2E-covered payout/settlement
chain (so it'd need an E2E run), whereas (a) is zero-behaviour-change (build + unit gate). Phase 5 already
publishes `B2B.Tenant.Contracts`, so (a) just pulls one `<IsPackable>` flip forward — zero wasted work, and it
**doesn't foreclose** a later deliberate pattern-E re-route (logged in `api/Concertable.Payment/TECH_DEBT.md`).

**Like Phase 2, this is two sub-steps — publish *before* you can consume.** A second escaping ref surfaced
that the plan hadn't anticipated: `Payment.Api → Concertable.Shared.Api`, and `Shared.Api` wasn't published
(see the classification note above). So Payment needs **two** packages live on the feed that weren't —
`Shared.Api` and `B2B.Tenant.Contracts` — plus its own `Payment.Contracts`/`Payment.Client`. They only publish
on merge to `master`, so consume (3b) waits for publish (3a) to be live.

- **3a — publish the packages Payment will consume. — ✅ DONE & SHIPPED.** Merged via **PR #61** (merge
  `af5e0b8c`); the post-merge `Publish packages` run is **green** (both `publish` and `verify-restore`), so all
  4 packages are **live on the feed at `0.1.0-alpha.0.529`**. Flipped `<IsPackable>true</IsPackable>` on
  `Concertable.Shared.Api` (inherits MinVer + metadata from `api/Shared/`), `Concertable.Payment.Contracts`,
  `Concertable.Payment.Client`, and `Concertable.B2B.Tenant.Contracts`. The Payment and B2B folders gained
  MinVer + package metadata in their **own** `Directory.Build.props` (mirroring `Shared/`; per-folder,
  carve-safe) + the MinVer `GlobalPackageReference` in their `Directory.Packages.props`. **BUILD1 proven clean:**
  `dotnet pack` → exactly **30** packages, every `.nuspec` audited (no feed-absent `Concertable.*` dependency);
  the live `verify-restore` re-proves the full closure on every publish.
  - **Two CI gaps were fixed in PR #61 (durable, now on `master`):** (1) the `publish` job is credentialed with
    `GITHUB_PACKAGES_TOKEN` — Phase 2b made Auth a package *consumer*, so `dotnet pack` of the whole solution
    restores Auth from the feed and had been **401-failing every master publish since #60** until this fix;
    (2) `verify-restore` now **generates** its package list from the `<IsPackable>true</IsPackable>` projects
    (PackageId == project-file name; empty-match guarded) instead of a hand-maintained list, so it can't drift.
    Both proven green by the #61 post-merge publish run.
- **3b — flip Payment to consume them. — ✅ DONE.** Swapped every `ProjectReference` in Payment's deployable
  closure that escaped `api/Concertable.Payment/` for a `PackageReference` across **8 csproj** (Web, Workers, Api,
  Application, Infrastructure, Domain, Contracts, Client) — **13 distinct `Concertable.*` packages**
  (Auth.Contracts, B2B.Tenant.Contracts, Contracts, Kernel, DataAccess.{Application,Infrastructure},
  Messaging.{Contracts,Infrastructure,AzureServiceBus}, ServiceDefaults, Shared.Api, Seed.{Shared,Identity}),
  pinned lockstep in Payment's **own** `Directory.Packages.props` via a single `$(ConcertablePlatformVersion)` =
  **`0.1.0-alpha.0.529`** (re-verified present on the feed for all 13 ids before pinning). Intra-folder refs
  (Domain/Application/Contracts/Client/Infrastructure/Api/Seed) stay `ProjectReference`s; AppHost.Extensions and
  the E2ETests.Helpers harness keep their cross-folder refs (composition / E2E-harness layers, exempt).
  - **✅ Carve proven standalone.** `git archive HEAD:api/Concertable.Payment` → restore-from-feed → `dotnet build`
    of the deployable closure (Web + Workers + Client), built **outside the repo tree** (the carve carries its own
    `nuget.config` / `Directory.{Build,Packages}.props`, and no repo- or `api`-root config sits above it) — **green
    (0 errors)**. The Phase-0 `9× MSB3202 project-not-found` is gone; the whole shared platform + cross-service
    contracts resolved as packages from the feed. Built Web/Workers/Client, **not** the `.slnx` (it also carries the
    exempt AppHost.Extensions + E2ETests.Helpers, which reference cross-folder projects absent from the carve).
  - **✅ `carve-payment` CI job added** in `.github/workflows/test.yml`, mirroring `carve-auth` (same `git archive`
    technique, `needs: build`, feed credential via the repo `GITHUB_TOKEN`). It builds a generated closure-only
    solution of every package-clean Payment project — one feed restore, each project built directly (so a future
    ref-removal can't orphan one from the gate) — with `MinVerSkip` since the carved tree has no `.git`.
  - **Ruleset wiring deferred to Phase 7.** `carve-payment` is **not yet a required check** in ruleset `17393335`
    — and neither is `carve-auth` (Phase 2b never wired it either). So today a re-introduced escaping ref fails the
    job without blocking merge. Wiring both gates — plus the future carve-* gates — into the ruleset's required
    checks is one repo-admin step (the agent's PATCH is auto-blocked), to run *after* each job exists on `master`
    so a concurrent merge-queue entry isn't blocked on a check its branch can't report:
    `gh api -X PATCH repos/Concertable/Concertable/rulesets/17393335 --input rules.json`. Tracked in Phase 7.
  - **✅ Gate passed:** `dotnet build api/Concertable.slnx` green (0 errors); standalone carve green; Payment unit
    tests green (**25 passed**). Zero behaviour change ⇒ no E2E. **This completes Phase 3** (3a + 3b); Phases 4–7
    remain, so this plan stays.

## Phase 4 — Search standalone

**Like Phases 2–3, two sub-steps — publish *before* you can consume.** Search's deployable closure reads
four B2B contracts that weren't on the feed — `B2B.{Artist,Venue,Concert}.Contracts` and the producer
seed library `B2B.Seed.Contracts` (`B2B.Tenant.Contracts` was already published in Phase 3a). They only
publish on merge to `master`, so consume (4b) waits for publish (4a) to be live.

- **4a — publish the B2B contracts Search consumes. — ✅ DONE & SHIPPED.** Merged via **PR #63** (merge
  `0ebed2f8`); the post-merge `Publish packages` run is **green** (both `publish` and `verify-restore`), so all
  4 packages are **live on the feed at `0.1.0-alpha.0.531`** (lockstep with `B2B.Tenant.Contracts` + the shared
  platform). Flipped `<IsPackable>true</IsPackable>` + added a `<Description>` on the four B2B contracts —
  `Concertable.B2B.{Artist,Venue,Concert}.Contracts` and `Concertable.B2B.Seed.Contracts` — joining the
  Phase-3a `B2B.Tenant.Contracts`. **No folder config needed:** the B2B folder already gained MinVer +
  package metadata + the MinVer `GlobalPackageReference` in Phase 3a. **BUILD1 proven clean:**
  `dotnet pack api/Concertable.slnx` → exactly **34** packages (30 + 4), every `.nuspec` audited —
  Artist/Venue/Concert.Contracts depend only on Contracts/Kernel/Messaging.Contracts, and Seed.Contracts on the
  three module contracts + Seed.Identity, all inside the published set; the full 34-package audit showed **no**
  feed-absent `Concertable.*` dependency, re-proven live by `verify-restore` (auto-generates its list from
  `<IsPackable>true</IsPackable>` projects).
- **4b — flip Search to consume them. — ✅ DONE.** Swapped every `ProjectReference` in Search's deployable
  closure that escaped `api/Concertable.Search/` for a `PackageReference` across **7 csproj** (Domain,
  Application, Infrastructure, Api, Web, Workers, Seed.Infrastructure) — **14 distinct `Concertable.*` packages**
  (B2B.{Artist,Venue,Concert,Seed}.Contracts, Kernel, DataAccess.Infrastructure,
  Messaging.{Contracts,Domain,Infrastructure,AzureServiceBus}, ServiceDefaults, Shared.Api, Seed.{Shared,Identity}),
  pinned lockstep in Search's **own** `Directory.Packages.props` via a single `$(ConcertablePlatformVersion)` =
  **`0.1.0-alpha.0.531`** (re-verified present on the feed for all 14 ids before pinning). Search **publishes
  nothing**, so its folder needs **no** MinVer/metadata — only the consume-side pin block. Intra-folder refs
  stay `ProjectReference`s; AppHost.Extensions + the IntegrationTests/E2ETests.Helpers harness keep their
  cross-folder refs (composition / test-harness layers, exempt). _(Application's escaping `Kernel` ref was easy
  to miss — there were 7 csproj to flip, not 6.)_
  - **✅ Carve proven standalone.** `git archive <tree>:api/Concertable.Search` → restore-from-feed →
    `dotnet build` of a closure-only solution of the 7 package-clean projects, built **outside** the repo tree
    (the carve carries its own `nuget.config` / `Directory.{Build,Packages}.props`, and no repo/`api`-root config
    sits above it) — **green (0 errors)**. The Phase-0 `MSB3202 project-not-found` is gone; the shared platform +
    B2B contracts resolved as packages from the feed. Built the closure solution, **not** the `.slnx` (it also
    carries the exempt AppHost.Extensions + test harness, which reference cross-folder projects absent from the carve).
  - **✅ `carve-search` CI job added** in `.github/workflows/test.yml`, mirroring `carve-payment` (same `git
    archive` technique, `needs: build`, feed credential via the repo `GITHUB_TOKEN`). No `MinVerSkip` — Search's
    folder has no MinVer. **Ruleset wiring stays deferred to Phase 7** (`carve-search` joins `carve-auth`/`carve-payment`
    as a non-required job until then).
  - **✅ Gate passed:** `dotnet build api/Concertable.slnx` green (0 errors); standalone carve green; Search unit
    (**14**) + integration (**27**) green. Zero behaviour change ⇒ no E2E. **This completes Phase 4**; Phases 5–7
    remain, so this plan stays.
  - **Local gotcha (recurs in Phases 5–6):** after a new lockstep version publishes, a repo-root
    `dotnet build api/Concertable.slnx` restores the *solution-root* config (no feed source above `api/`), so a
    just-published version that isn't cached yet fails `NU1101` for the newly-pinned packages. Fix: `dotnet restore`
    one project in the flipped folder first (its per-folder `nuget.config` reaches the feed and caches the version),
    then the slnx build resolves from cache. CI is unaffected (the `build`/`publish` jobs prove feed restore on a
    fresh cache).

## Phase 5 — B2B standalone (churny core packaged here, with hybrid inner loop)

**Like Phases 2–4, two sub-steps — publish *before* you can consume.** B2B's deployable closure reads two
contracts that aren't on the feed: its own last cross-service contract `B2B.User.Contracts` (`Tenant` shipped
in Phase 3a; `Artist`/`Venue`/`Concert` + `Seed.Contracts` in Phase 4a) and `Customer.Review.Contracts` (the
one reverse data-flow — B2B consumes Customer's review event). They only publish on merge to `master`, so
consume (5b) waits for publish (5a) to be live. `Contract.Contracts`/`Conversations.Contracts` are
B2B-internal (cross-module, not cross-service) → they ride along in B2B's carve, never published.

- **5a — publish `B2B.User.Contracts` + `Customer.Review.Contracts`. — ✅ DONE & SHIPPED.** Merged via
  **PR #65** (merge `ea94f148`); the post-merge `Publish packages` run (`28396344649`) is **green** (both
  `publish` and `verify-restore`), so all **36** packages are **live on the feed at `0.1.0-alpha.0.533`**
  (lockstep). Flipped `<IsPackable>true</IsPackable>` + added a `<Description>` on both. `B2B.User.Contracts`
  inherits MinVer + metadata from the B2B folder (added in Phase 3a); the **Customer folder started publishing
  for the first time**, so it gained MinVer + package metadata in its **own** `Directory.Build.props` (mirroring
  `Shared/`/B2B) + the MinVer `GlobalPackageReference` in its `Directory.Packages.props` (per-folder,
  carve-safe — no repo-root config). **BUILD1 proven clean:** `dotnet pack api/Concertable.slnx` → exactly
  **36** packages (34 + 2), every `.nuspec` audited — `User.Contracts` depends only on
  Kernel/Messaging.Contracts/`B2B.Tenant.Contracts`, and `Review.Contracts` on Contracts/Messaging.Contracts,
  all inside the published set; the full 36-package audit showed **no** feed-absent `Concertable.*` dependency,
  re-proven live by `verify-restore`. **Gate:** `dotnet build api/Concertable.slnx` green (0 errors); zero
  behaviour change ⇒ no tests/E2E. **5b can now proceed.**
- **5a.2 — publish the overlooked `Concertable.Seed.Infrastructure`. — ✅ DONE & SHIPPED (PR #66).** A
  *third* missing package surfaced when 5b's build actually tried to restore: **`B2B.Web` consumes the shared
  `Concertable.Seed.Infrastructure`** (its `DevDbInitializer` wiring), but Phase 2a left it non-`IsPackable`
  ("referenced by nobody in the published set"). That BUILD1 audit only walks *published packages'* closures,
  so a `Shared/` lib consumed by a **deployable** (not another package) slipped through — exactly what the
  carve gate exists to catch. The plan's 5b list above also overlooked it (named only `Seed.{Shared,Identity}`).
  Flipped `<IsPackable>true</IsPackable>` + `<Description>` (inherits MinVer/metadata from `api/Shared/`);
  BUILD1-clean (nuspec deps all already published). Merged via **PR #66** (admin-merged through the merge queue);
  the post-merge publish run (`28405696828`) is green, re-stamping the whole platform lockstep at
  **`0.1.0-alpha.0.535`** (Seed.Infrastructure now live there).
- **5b — flip B2B to consume them, and stand up the hybrid inner loop. — ✅ DONE (PR #67).**
  - **Pinned** to **`0.1.0-alpha.0.535`** (the 5a.2 version) via a single `$(ConcertablePlatformVersion)` in
    B2B's own `Directory.Packages.props` — **27** distinct `Concertable.*` ids (the plan's list above + the
    discovered `Seed.Infrastructure`), all re-verified live on the feed before pinning.
  - **Flipped 41 deployable-closure csproj** (modules Api/Application/Domain/Infrastructure/Contracts,
    DataAccess.{Application,Infrastructure}, Web, Workers, Seed.{Contracts,Infrastructure,Simulator}) from
    escaping `ProjectReference` → `PackageReference`. Intra-B2B refs + AppHost(.Extensions) + the
    IntegrationTests/E2E harness stayed `ProjectReference` (composition / test-harness exempt).
  - **Hybrid inner loop** for the churny core (Kernel, Messaging.*): default = packages; `-p:UseLocalCore=true`
    or `CONCERTABLE_LOCAL_CORE=1` swaps them to in-repo `ProjectReference`s for fast cross-cutting dev.
    Implemented **centrally** in `api/Concertable.B2B/Directory.Build.targets` (`ChurnyCorePackage` = id→path
    source of truth; `Update`-tags each referenced core package, transforms to a `ProjectReference`, removes the
    package) anchored on `$(ConcertableCoreRoot)` from `Directory.Build.props` — so per-csproj diffs stay
    package-only and the toggle lives in one place. **The plan's suggested per-csproj conditional approach is
    NOT viable** — custom metadata in item `Condition`s hits `MSB4191`; the central `Update`/transform pattern
    is what works. Swap proven in both modes via `dotnet msbuild -getItem`.
  - **Carve-breaker found & fixed (real bug, any OS):** 8 closure projects (+ the E2E harness) referenced B2B's
    *own* `Concertable.B2B.Seed.Infrastructure` via a **round-trip path** `..\..\..\..\Concertable.B2B\Seed\...`
    (up to `api/`, back down into `Concertable.B2B/`). Resolves in the monorepo but **escapes the git-archive
    carve**, whose root *is* the B2B folder (no `Concertable.B2B/` segment). _(The plan's note above that these
    are "not escaping" was true for the **service** boundary but wrong for the **carve**.)_ Normalized to direct
    in-folder relative paths (`..\Seed\...`); same target project, zero behaviour change.
  - **`carve-b2b` CI job added** in `test.yml` (mirrors `carve-search`; discovers the 41-project closure from the
    `standalone carve` marker so it can't drift/orphan; `MinVerSkip` in the `.git`-less carve). **✅ green on the
    ubuntu CI runner.** _(A local **Windows** `dotnet build` of the carve hits a spurious `MSB3030` "dll not
    found" on the two all-`PackageReference` leaf projects — `DataAccess.Application`, `Conversations.Contracts`;
    it does **not** reproduce on the Linux CI runner, where `carve-search`'s identical all-package leaf
    `Search.Domain` also builds fine. Treat it as a local-SDK quirk, verify the carve on CI.)_
  - **✅ Gate passed on PR #67 CI:** `build` green, **`carve-b2b` green (ubuntu)**, all B2B `unit-tests` +
    `integration-tests` green. **E2E** is `skipped` on the PR and runs as the **merge-queue gate**
    (`e2e-api-tests` + `e2e-ui-tests`) when #67 is enqueued — so E2E executes before merge (this change is
    packaging-only / zero behaviour change, but B2B meets the massive/risky bar, so the queue runs it).
  - **Local gotcha (as Phase 4):** after the pin, a repo-root `dotnet build api/Concertable.slnx` can `NU1101`
    on the just-pinned packages until cached — `dotnet restore` one B2B-folder project first, then build.
  - **This completes Phase 5.** Phases 6–7 remain, so this plan stays.

## Phase 6 — Customer standalone — ✅ DONE

**One sub-step, not two — everything Customer consumes was already on the feed.** Unlike Phases 2–5,
no 6a publish step was needed: every package Customer's deployable closure reads (shared platform from
2a/3a/5a.2, `Auth.Contracts` from 2a, the B2B `{Artist,Venue,Concert,Tenant,User}.Contracts` +
`B2B.Seed.Contracts` from 3a/4a/5a, `Payment.Client`/`Payment.Contracts` from 3a) is live at the
current lockstep `0.1.0-alpha.0.536`. Customer's own `Customer.Review.Contracts` already publishes
(Phase 5a) and is consumed in-folder, so it isn't pinned as a package.

- **Pinned** to **`0.1.0-alpha.0.536`** via a single `$(ConcertablePlatformVersion)` in Customer's own
  `Directory.Packages.props` — **29** distinct `Concertable.*` ids, all re-verified live on the feed
  before pinning.
- **Flipped 28 deployable-closure csproj** (Web, modules' Api/Application/Domain/Infrastructure/Contracts,
  Seed.Infrastructure) from escaping `ProjectReference` → `PackageReference`. Intra-Customer refs +
  AppHost(.Extensions) + the IntegrationTests/E2ETests harness stayed `ProjectReference` (composition /
  test-harness exempt). _(Only `Preference.Api` and `User.Api` among the module Api projects had an
  escaping ref — `Shared.Api`; the other Api projects reach only their own siblings.)_
- **Hybrid inner loop** mirrored from B2B (Customer is the top co-change partner): default = packages;
  `-p:UseLocalCore=true` / `CONCERTABLE_LOCAL_CORE=1` swaps the churny core (Kernel, Messaging.*) to
  in-repo `ProjectReference`s. Implemented in `api/Concertable.Customer/Directory.Build.targets`
  (`ChurnyCorePackage` id→path) anchored on `$(ConcertableCoreRoot)` from `Directory.Build.props`.
- **Carve-breaker found & fixed (real bug, any OS):** Web + 7 module Infrastructure projects referenced
  Customer's *own* `Seed.Infrastructure` via a round-trip path (`..\..\Concertable.Customer\Seed\...` —
  up to `api/`, back into `Concertable.Customer/`) that resolves in the monorepo but escapes the
  git-archive carve, whose root *is* the Customer folder. Normalized to direct in-folder relative paths;
  same target, zero behaviour change. (Identical class of bug to Phase 5b's B2B fix.)
  - **✅ Carve proven standalone.** `git archive HEAD:api/Concertable.Customer` → restore-from-feed →
    `dotnet build` of a find-discovered closure-only solution (34 projects), built **outside** the repo
    tree (carve carries its own `nuget.config` / `Directory.{Build,Packages,Build.targets}.props`, no
    repo/`api`-root config above it) — **green (0 errors)**. The Phase-0 `MSB3202 project-not-found` is
    gone; the shared platform + cross-service contracts resolved as packages from the feed. _(Unlike B2B,
    the Windows local carve build did **not** hit the spurious all-package-leaf `MSB3030`.)_
  - **✅ `carve-customer` CI job added** in `.github/workflows/test.yml`, mirroring `carve-b2b` (same
    `git archive` technique, `find`-discovered closure excluding Tests/AppHost, `MinVerSkip` since the
    carve has no `.git` and Customer's folder carries MinVer for Review.Contracts). **Ruleset wiring
    stays deferred to Phase 7** (`carve-customer` joins the other carve jobs as non-required until then).
- **✅ Gate:** `dotnet build api/Concertable.slnx` green (0 errors); standalone carve green; Customer
  **unit** tests green (Concert 30, Review 16, Ticket 19, User 8). Docker was down locally, so Customer
  **integration** + **E2E** are verified on **CI** — `integration-tests` (Customer Concert/Review/Ticket/User)
  runs per-PR, and **E2E** runs as the **merge-queue gate** (`e2e-api-tests` + `e2e-ui-tests`, which run
  Customer's API + UI E2E); Customer is the other cross-cutting service, so the queue runs E2E before merge.
  **This completes Phase 6.** Phase 7 remains, so this plan stays.

## Phase 7 — Lock it in

- Add a guardrail (build target / test) that **fails the build if any service deployable project
  gains a `ProjectReference` escaping its service folder** — so separation can't silently regress.
- **Wire every carve gate** (`carve-auth`, `carve-payment`, and the carve jobs added in Phases 4–6) into
  ruleset `17393335` as **required checks**, so an escaping `ProjectReference` actually blocks merge instead
  of just failing a non-required job. Repo-admin step; run it after each job is on `master`.
- Update `api/ARCHITECTURE.md`: the split mapping is now executed; document the hybrid inner-loop
  convention. Delete this plan in the same commit that lands the final phase (per `plans/CLAUDE.md`).

---

## Evidence carried forward (from the deleted `REPO_SPLIT_INVESTIGATION.md`)

- **Cross-service runtime coupling is 100% `*.Contracts`** across ~160 `.csproj`. Only source leaks:
  `B2B.IntegrationTests.Fixtures → Payment.Infrastructure` and `B2B.Web → Payment.Seed` (both addressed
  in Phase 0), plus the `Payment → B2B.*.Contracts` backwards edges (addressed by the payment-proxy
  merge). The B2B **E2E** projects' `→ Payment.Seed` ref is deliberately *not* cut — it's part of the
  full-stack E2E harness exception (see the composition-layer note above), not a leak to remove.
- **Shared surface consumed by all 5 services:** Kernel, Messaging.*, ServiceDefaults,
  DataAccess.Infrastructure, Shared.Api, Seed.{Shared,Identity} → these gate every standalone build.
- **Package-path churn (3mo ≈ all-time):** Kernel 37, Messaging 35, `@concertable/shared` 23 (FE, not
  in scope), B2B contracts ~40 combined, Payment.Client 12, Customer.Review.Contracts 8,
  Concertable.Contracts 8, Payment.Contracts 4, **Auth.Contracts 0**.
- **Cross-cutting:** 13% (3mo) → 47% (last month) of commits touch ≥2 data services; `B2B`+`Customer`
  is the top co-change pair every window — why the churny core is packaged last with a hybrid loop,
  and why a *repo* split (vs. this build-separation) should wait.
- **Build proof:** `git archive HEAD:api/Concertable.B2B` → `dotnet build` ⇒ 9× MSB3202
  project-not-found for `Shared/`, `Auth`, `Payment.*`, `Search.*` — the carve is a folder copy today.
- **Deployment:** none exists (no Dockerfiles/registry/IaC/deploy CI). Separate effort, after this.
