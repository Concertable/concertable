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
  `Concertable.Shared.{Blob,Email,Geocoding,Imaging,Notification,Pdf}.*`,
  `Concertable.Seed.{Shared,Identity}`, `Concertable.Testing(.Integration)`; **plus** cross-service
  contracts — `Auth.Contracts`, `B2B.{Artist,Concert,Venue,User,Tenant}.Contracts`,
  `B2B.Seed.Contracts`, `Customer.Review.Contracts`, `Payment.Contracts`, `Payment.Client`.
- **Stays source / build-from-source:** every service-internal `*.Domain/.Application/.Infrastructure`
  and module `*.Api`, each service's `Seed.Infrastructure`, and the **AppHosts** (the dev-composition
  layer — see note below).
- **Container (later, deployment effort):** the deployables (`Auth`, `B2B.Web`, `B2B.Workers`,
  `Customer.Web`, `Search.Web`, `Search.Workers`, `Payment.Web`, `Payment.Workers`) and the
  `B2B.Seed.Simulator`. Not this plan.

**AppHost note:** AppHost projects legitimately reference sibling deployables to orchestrate the dev
topology (e.g. `B2B.AppHost` → Auth, Payment.Web/Workers, Search.Web/Workers; `Customer.AppHost` →
`B2B.Seed.Simulator`). They are the dev-composition layer and remain monorepo-bound — they are the
**one place** cross-folder references stay allowed. A service's *deployable closure* (Web/Workers +
modules) must be package-clean; its AppHost need not be until the deployment effort turns those refs
into `AddContainer`.

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
  test seam: escrow simulation moved from `PaymentDbContext`/`EscrowEntity` to an in-memory
  `EscrowStore`/`EscrowRecord`; 6 dead Stripe-internal mocks deleted (no consumer once Payment runs
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

## Phase 1 — Stand up the packaging rails (publishes nothing consumed yet) — ✅ CODE DONE (CI run pending)

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
  intended packages). `Concertable.Kernel` is the first opted-in package — proven locally:
  `dotnet pack` → `Concertable.Kernel.0.1.0-alpha.0.526.nupkg`, no NU1507. _(commit 2)_
- ✅ **CI workflow** `.github/workflows/publish-packages.yml`: packs every `IsPackable` project,
  pushes to the feed (`GITHUB_TOKEN`, `packages: write`), then a `verify-restore` job restores
  `Concertable.Kernel` into a fresh consumer from the feed. Triggers: push to `master`
  (path-filtered) + `workflow_dispatch`.
- ⏳ **Gate — CI run (handoff):** build green ✅ + full restore green ✅ + local pack green ✅ here, but
  the publish+restore loop only runs in CI. Verify by merging to `master` (or manual dispatch once on
  the default branch). **Prereq:** GitHub Packages enabled for the `Concertable` org (the workflow's
  `GITHUB_TOKEN` has `packages: write`); local dev consuming `Concertable.*` later needs a
  `GITHUB_PACKAGES_TOKEN` PAT with `read:packages`. Zero behavior change; no E2E.
- **Note:** the *full* publishable-set marking (Auth.Contracts + the rest of the shared platform) is
  Phase 2 — Phase 1 proves the rails with one package.

## Phase 2 — Prove the mechanism on the most stable boundary (Auth + shared platform)

- Publish `Auth.Contracts` + the shared-platform packages.
- Convert **one** service (recommend `Concertable.Auth`, smallest closure) to consume the shared
  platform via `PackageReference`; flip its escaping refs.
- **Prove standalone:** carve that service's tree (the Phase-0/`git archive` repro) and confirm it now
  **restores from the feed and builds**. Add this as a CI check.
- **Gate:** standalone build of the service green; its tests green.

## Phase 3 — Payment standalone

- Publish `Payment.Contracts` + `Payment.Client`. Flip Payment's refs to packages — shared platform,
  `Auth.Contracts`, and `B2B.Tenant.Contracts` (the `TenantCreatedEvent` subscription; see the Phase 0
  finding — package it, or re-route the subscription to a Payment-owned event to drop the edge).
- Prove Payment carves-and-builds against the feed.
- **Gate:** standalone Payment build + unit tests green.

## Phase 4 — Search standalone

- Publish the B2B contracts Search reads (`B2B.{Artist,Concert,Venue}.Contracts`,
  `B2B.Seed.Contracts`). Flip Search's refs.
- Prove Search carves-and-builds against the feed.
- **Gate:** standalone Search build + unit/integration green.

## Phase 5 — B2B standalone (churny core packaged here, with hybrid inner loop)

- Publish remaining B2B contracts (`User`, `Tenant`) and `Customer.Review.Contracts` (B2B consumes it
  — the one reverse data-flow). Flip B2B's refs to packages.
- Introduce the **hybrid inner-loop** toggle for `Kernel`/`Messaging` so cross-cutting dev stays fast
  while CI/standalone use packages.
- **Gate:** standalone B2B build + unit/integration green; **run E2E** (B2B is behaviorally central
  and cross-cutting — meets the massive/risky bar).

## Phase 6 — Customer standalone

- Customer consumes the B2B contracts + `Auth.Contracts` + `Payment.Client/Contracts` as packages.
  (`Customer.AppHost → B2B.Seed.Simulator` stays a dev-host `ProjectReference` until the deployment
  effort turns it into `AddContainer` — out of scope here.)
- Prove Customer carves-and-builds against the feed.
- **Gate:** standalone Customer build + unit/integration green; **run E2E** (the other cross-cutting
  service).

## Phase 7 — Lock it in

- Add a guardrail (build target / test) that **fails the build if any service deployable project
  gains a `ProjectReference` escaping its service folder** — so separation can't silently regress.
- Update `api/ARCHITECTURE.md`: the split mapping is now executed; document the hybrid inner-loop
  convention. Delete this plan in the same commit that lands the final phase (per `plans/CLAUDE.md`).

---

## Evidence carried forward (from the deleted `REPO_SPLIT_INVESTIGATION.md`)

- **Cross-service runtime coupling is 100% `*.Contracts`** across ~160 `.csproj`. Only source leaks:
  `B2B.IntegrationTests.Fixtures → Payment.Infrastructure`, `B2B.Web/E2ETests → Payment.Seed`, and the
  `Payment → B2B.*.Contracts` backwards edges (all addressed in Phase 0).
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
