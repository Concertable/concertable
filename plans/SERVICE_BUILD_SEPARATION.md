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
- **Feed:** GitHub Packages (already on GitHub).
- **Source stays in the monorepo** — separate the *build closures* in place. Moving services to
  their own repos is a later, optional, org-driven step; it is **out of scope here**.
- **Phased by boundary stability** — most-stable contract first, churny shared core last.

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

## Phase 0 — Remove the cross-service source leaks (no packaging yet)

These edges drag another service's **source** into a service's closure, so they'd poison its package
boundary. They are violations regardless of this plan.

- Land `plans/PAYMENT_AGNOSTIC_AUDIT.md`: drop the dead `Payment → B2B.{Contract,Concert,User}.Contracts`
  edges + the stale `DataAccess.Application` domain `ProjectReference`s/GlobalUsings, and the
  B2B↔Payment reverse leak.
- Remove `B2B.Web → Payment.Seed` (B2B compiling Payment's seed project).
- Replace `B2B.IntegrationTests.Fixtures → Payment.Infrastructure` (the one true backwards edge into
  another service's internals) with a Payment Client/contract-based test seam.
- **Gate:** full build green; B2B + Payment unit/integration green. No E2E (no behavior change).

## Phase 1 — Stand up the packaging rails (publishes nothing consumed yet)

- Add `nuget.config` with the GitHub Packages source + auth.
- Add `Directory.Packages.props` (central package management) so package versions are managed in one
  place across the monorepo.
- Add package metadata + a deterministic version source (CI run / `MinVer`/`Nerdbank.GitVersioning`)
  to the projects that will publish.
- Add a CI workflow that builds + **publishes changed packages** to the feed (path-filtered so a
  contract change republishes only that package).
- **Gate:** a CI run publishes a throwaway package and a consumer can `restore` it. Zero behavior
  change; no E2E.

## Phase 2 — Prove the mechanism on the most stable boundary (Auth + shared platform)

- Publish `Auth.Contracts` + the shared-platform packages.
- Convert **one** service (recommend `Concertable.Auth`, smallest closure) to consume the shared
  platform via `PackageReference`; flip its escaping refs.
- **Prove standalone:** carve that service's tree (the Phase-0/`git archive` repro) and confirm it now
  **restores from the feed and builds**. Add this as a CI check.
- **Gate:** standalone build of the service green; its tests green.

## Phase 3 — Payment standalone

- Publish `Payment.Contracts` + `Payment.Client`. Flip Payment's refs to packages (after Phase 0,
  Payment depends only on shared + `Auth.Contracts`).
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
