# Repository-per-microservice migration

**Status:** APPROVED by Tommy 2026-08-26 and in execution. Current state and next steps live in the
exclusive stream ledgers named by the `platform/polyrepo-cut` active-owner table in
`POLYREPO_ROADMAP.md`; those ledgers override any status or inventory figure in this document.

**This document's inventory is a 2026-08-02 snapshot and has drifted.** Four audits re-verified every
checkpoint against current `main` on 2026-08-26; Git history retains that rescope evidence. Known corrections:
checkpoint 5 is ~85% delivered and its "keep b2b/customer local" constraint is superseded; the
`*.Hosting` projects of checkpoint 2 already exist; the `system` repository is renamed `fleet`.

**Planning baseline:** `origin/main` at `d3c399ec8b4a4f4916b17764400ffbf73ba455a9` on 2026-08-02.
**Re-verified against `origin/main` on 2026-08-26** (2873 commits later); machine-readable inventory
and extraction map live in `eng/repository-split/`.

This plan replaces the monorepo as the source of truth with independent repositories. Worktrees and
sparse checkout are not part of the target architecture. They may be used only as disposable local
implementation mechanics.

## Decision summary

The durable target is nine canonical repositories:

1. five service repositories: B2B, Customer, Payment, Search, and Auth;
2. two platform repositories: one for shared .NET packages and one for shared frontend packages;
3. one system repository for the full-stack AppHost, fleet manifest, infrastructure, deployment, and
   black-box E2E; and
4. the conventional organization `.github` repository for reusable workflows and repository policy.

Every service owns its runtime, database migrations, development seeding, public Contracts, container
images, and canonical standalone AppHost. Runtime service code may consume another service only through
that service's published Contracts or purpose-built client SDK. Cross-repository source references are
forbidden. The system repository composes immutable container images and published test/hosting artifacts;
it does not compile service implementations.

B2B Workers remains an Azure Functions v4 isolated runtime, but its production artifact is a container and
its production host is native Azure Functions on Azure Container Apps. Functions Consumption is not part of
the target: it cannot run the same custom container used by standalone AppHosts, system E2E, and rollback.

The current lockstep `ConcertablePlatformVersion` and platform-sync PR are replaced by independently
versioned release trains plus Renovate PRs. Breaking package changes use an expand/publish/migrate/contract
sequence. There is never a repository-wide forced bump that can strand every service on a red pin.

## Non-negotiable boundaries

- B2B and Customer remain separate data services. Neither references the other's Domain, Application,
  Infrastructure, Web, migrations, DbContext, or source tree.
- Auth, Payment, and Search remain services/adapters with private runtimes. Their public packages are
  deliberately small boundary artifacts, not a route to sharing implementation.
- Each service owns one logical database. Auth's current persisted-grant use of `B2BDb` must move into
  `AuthDb` before extraction.
- Data services never wait for sibling data services. A consumer needing deterministic producer data runs
  the producer-owned seed simulator image and receives the producer's published events.
- Each service's standalone AppHost is canonical for service development. It runs that service from source
  and foreign dependencies from pinned images.
- The full-stack AppHost and E2E suite live in `Concertable/system` and run only published images at the
  compatibility versions recorded in the fleet manifest.
- A service runtime may reference only platform packages and published boundary packages. Build-time
  AppHost and test tooling packages are allowed only in AppHost/test projects and are rejected from runtime
  closures.
- No checkpoint merges without Tommy explicitly instructing the merge. Every completed checkpoint is a
  hard stop under `plans/AGENTS.md`.

## Actual current-state inventory

### Repository and build shape

- The repository contains 176 `.csproj` files, five `.slnx` files, and nine `package.json` files.
- Service solutions exist for B2B, Customer, Payment, and Search. Auth has a standalone AppHost but no
  Auth-only solution file.
- Each service already has a carve-compatible `Directory.Build.props`, `Directory.Build.targets`,
  `Directory.Packages.props`, and `nuget.config`.
- Carve CI proves each deployable closure restores from the GitHub Packages feed. AppHosts, E2E projects,
  and the optional `UseLocalCore` path are intentionally exempt from the current source-boundary check.
- All service folders currently pin the same
  `ConcertablePlatformVersion=0.1.0-alpha.0.745`.

### Compile-time dependency graph

The deployable closures are already package-clean. The only non-AppHost, non-test cross-area
`ProjectReference` found is:

```text
Concertable.Auth.Contracts -> Concertable.Messaging.Contracts
```

That is a platform dependency and becomes a `PackageReference` when Auth is extracted. All other
cross-area project references occur in composition or full-stack test code. The observed composition/test
edge counts are:

| Source area | Cross-area project-reference edges |
|---|---|
| Auth | Platform 1 |
| B2B | Auth 1; Auth.Contracts 1; Customer 1; Payment 7; Platform 19; Search 2 |
| Customer | Auth 1; Auth.Contracts 1; B2B 5; Payment 7; Platform 12; Search 2 |
| Payment | Auth 1; Auth.Contracts 1; B2B 2; Platform 5 |
| Search | Auth 1; B2B 4; Platform 8 |
| Full-stack shared tests | Auth 1; B2B 3; Customer 2; Payment 3; Platform 1; Search 3 |

Those edges are concentrated in `*.AppHost`, `*.AppHost.Extensions`, E2E fixtures, and E2E helpers. They
must become image/package edges before the source split.

The actual published-package consumption is:

| Consumer | Platform | Auth | B2B | Customer | Payment |
|---|---|---|---|---|---|
| Auth | Kernel, Contracts, Messaging, DataAccess, ServiceDefaults, shared capabilities, seeding | `Auth.Contracts` | - | - | - |
| B2B | same platform families | `Auth.Contracts` | owned module Contracts and `Seed.Contracts` | `Customer.Review.Contracts` | `Payment.Contracts`, `Payment.Client` |
| Customer | same platform families | `Auth.Contracts` | Artist, Concert, Venue, User, and Seed Contracts | `Customer.Review.Contracts` | `Payment.Contracts`, `Payment.Client` |
| Payment | same platform families | `Auth.Contracts` | - | - | `Payment.Contracts`, `Payment.Client` |
| Search | same platform families | Auth runtime at composition time | Artist, Concert, Venue, and Seed Contracts | rating/review events | - |

`Payment.Client` is an intentional Payment-owned transport SDK built on `Payment.Contracts`; it is not a
shared runtime. The target keeps that distinction.

Current packable ownership is:

- Auth: `Concertable.Auth.Contracts`.
- B2B: Artist, Concert, Tenant, User, and Venue Contracts plus `Concertable.B2B.Seed.Contracts`.
- Customer: `Concertable.Customer.Review.Contracts`, `Concertable.Customer.Ticket.Contracts`, and
  `Concertable.Customer.Hosting`.
- Payment: `Concertable.Payment.Contracts` and `Concertable.Payment.Client`.
- Platform: `Concertable.Contracts`, Kernel, DataAccess Application/Infrastructure, all five Messaging
  layers, ServiceDefaults, shared API/capability Application and Infrastructure packages, seed primitives,
  and Testing/Testing.Integration.

### Frontend dependency graph

- The root npm workspace contains the customer, venue, artist, and business web apps; customer and B2B
  mobile apps; `app/shared`; and `app/customer/shared`.
- `@concertable/shared` is consumed by every product surface and belongs in the frontend platform repo.
- `app/web/shared` is source-aliased into all four web SPAs and is not currently a package. It must become a
  published `@concertable/web-shared` package before extraction.
- `@customer/shared` is consumed only by Customer web/mobile and moves with Customer as a local workspace.
- `app/web/b2b/shared` is consumed only by B2B web surfaces and moves with B2B as a local workspace.

### AppHost and local-development graph

| Host | Own source runtime | Foreign runtime/project dependencies today | Databases today |
|---|---|---|---|
| Umbrella | all services and four SPAs/mobile | all source projects | B2B, Customer, Auth, Search, Payment |
| B2B | B2B Web/Workers and B2B frontends | Auth; Payment Web/Workers; Stripe CLI | B2B, Auth, Payment |
| Customer | Customer Web and customer frontends | Auth; Payment Web/Workers; B2B Seed Simulator; Stripe CLI | Customer, Auth, Payment, B2B |
| Payment | Payment Web/Workers | Auth; Stripe CLI | Payment, Auth, B2B |
| Search | Search Web/Workers | Auth; B2B Seed Simulator | Search, Auth, B2B |
| Auth | Auth | no sibling data service runtime | Auth, B2B |

The foreign `B2BDb` entries are caused by Auth's persisted-grant-store coupling, not by a legitimate data
service dependency. The umbrella host currently runs all source projects; the standalone hosts are already
the canonical architecture and must remain so.

`Concertable.AppHost.Shared` currently mixes generic resource/topology helpers with Auth-specific
composition. Generic primitives move to the .NET platform; service-specific hosting metadata moves to the
owning service.

The current `UseLocalCore` switch replaces selected `PackageReference`s with sibling
`ProjectReference`s. It cannot cross canonical repository boundaries and is removed. Its replacement is a
documented local NuGet feed workflow: pack a prerelease from the platform repo, push it to a user-local feed,
and select that version through a gitignored `Directory.Packages.local.props`. CI never imports that file.

### Databases, migrations, and seeding

There are 20 EF model snapshots:

| Owner | Context migrations |
|---|---:|
| B2B | 7 |
| Customer | 7 |
| Auth | 2 |
| Payment | 1 |
| Search | 1 |
| Platform Messaging | 2 (Inbox and Outbox) |

`api/initial-migrations.ps1` currently re-scaffolds all 20 contexts in one command. It preserves unchanged
migration IDs to avoid source/package migration collisions. Several runtime programs still call
`MigrateAsync`; the deployment design already requires deploy-time migration bundles/jobs instead.

Seed ownership is mostly aligned already:

- B2B owns its canonical seed catalog, `B2B.Seed.Contracts`, and `B2B.Seed.Simulator`.
- Customer and Search build projections from B2B seed events.
- Customer has local seed infrastructure but no producer simulator package/image for its own outbound
  review/rating events; Search's standalone topology therefore has an acknowledged gap.
- Payment owns Stripe-specific E2E seeding and local test seeding, but has no cross-service catalog.
- Projection test seeders are confined to integration tests; production-like local/full-stack composition
  must use producer events.

### CI/CD and GitHub state

The monorepo currently has these active workflow responsibilities:

- `test.yml`: change classification, build, five carve gates, unit/integration, merge-queue API/UI E2E, and
  the required `ci-complete` result.
- `publish-packages.yml`: packs every `IsPackable` project under one MinVer release train and verifies a
  clean consumer restore.
- `platform-sync.yml`: waits for package publication, discovers a bellwether package version, bumps every
  service's platform pin in one PR, and auto-merges only if the whole monorepo builds.
- `platform-sync-alert.yml`: labels and opens an issue for a broken sync.
- `mirror.yml`: manual `git subtree split` plus force-push to six read-only mirrors.
- `mirror-parity.yml`: nightly comparison of those mirrors with `main`.

The six existing public mirrors are `Concertable/{b2b,customer,auth,payment,search,shared}`. They default to
`master`. The latest mirror publication was green on 2026-07-27, but the latest three nightly parity runs
are red; on 2026-08-02 all six mirrors differed from `main`. They are therefore historical bootstrap inputs,
not trusted cutover sources. A final refresh and independent history verification are mandatory.

There is no active deployment workflow, tracked Terraform, or tracked Dockerfile. The existing deployment
plans selected Azure Container Apps, Azure Functions Consumption for B2B Workers, Azure Static Web Apps,
Terraform, GHCR, Azure App Configuration, Key Vault, managed identity, and deploy-time migration jobs.
Consumption cannot run a custom Functions container, so this plan supersedes that part of the deployment
design with native Azure Functions on Azure Container Apps. The GitHub repository has one `Production`
environment with no protection rules or branch policy. Its active ruleset protects `main` through merge
queue and required `ci-complete`.

Repository secrets currently mix CI, E2E, mirroring, package sync, and abandoned/current Azure App Service
credentials. No secret values were read. The planning credential lacks `read:packages`, so package ACL and
repository-linkage verification is an explicit preflight gate rather than an assumption.

No `Concertable/config`, `Concertable/system`, `Concertable/platform-dotnet`, or
`Concertable/platform-web` repository currently exists. The deployment plan's earlier assumption that a
config repo had been authored is not true in GitHub. The target places fleet IaC and config-as-code in the
system repo so desired images, infrastructure, migrations, and E2E promotion stay atomic.

## Target repository topology and ownership

| Repository | Owns | Must not own | Code owner |
|---|---|---|---|
| `Concertable/b2b` | B2B backend, modules, migrations, B2B Contracts, B2B seed catalog/simulator, B2B web/mobile surfaces, standalone AppHost | Customer runtime/projections by direct source, shared platform implementations | `@Concertable/b2b-maintainers` |
| `Concertable/customer` | Customer backend, migrations, Review/Ticket/Seed Contracts, Customer simulator, customer web/mobile and `@customer/shared`, standalone AppHost | B2B runtime/source | `@Concertable/customer-maintainers` |
| `Concertable/payment` | Payment Web/Workers, Payment DB migrations, Contracts, Client, Stripe test tooling, standalone AppHost | B2B business logic or database | `@Concertable/payment-maintainers` |
| `Concertable/search` | Search Web/Workers, projections, Search DB migrations, standalone AppHost | producer write models/databases | `@Concertable/search-maintainers` |
| `Concertable/auth` | Auth runtime, `Auth.Contracts`, Auth DB and both Auth/Duende migrations, standalone AppHost | B2B DB or tenant/business persistence | `@Concertable/auth-maintainers` |
| `Concertable/platform-dotnet` | Kernel, generic Contracts, Messaging, DataAccess, ServiceDefaults, shared capabilities, seed primitives, test primitives, generic Aspire hosting primitives, `Concertable.Build` | service DTOs, service topology, service runtime | `@Concertable/platform-maintainers` |
| `Concertable/platform-web` | `@concertable/shared`, `@concertable/web-shared`, shared ESLint/TypeScript/Vite conventions | B2B- or Customer-only UI/domain code | `@Concertable/frontend-platform-maintainers` |
| `Concertable/system` | full-stack container AppHost, immutable fleet manifest, API/UI/mobile E2E, Terraform, App Configuration declarations, deployment/promotion/rollback workflows | service implementations, EF models, private domain code | `@Concertable/system-maintainers` |
| `Concertable/.github` | reusable CI workflows, hardened composite actions, shared Renovate preset, PR/repository policy templates | application/runtime libraries | `@Concertable/platform-maintainers` |

Tommy is bootstrap administrator. Teams and `CODEOWNERS` express the durable ownership boundary even while
one person fills multiple roles.

The current source and generated mirrors are public, and the canonical repositories preserve that public
visibility. Temporary `*-next` repositories remain private until their history, settings, and artifacts pass
the cutover review. `Concertable/.github` is public from creation so public repositories can call its reusable
workflows. Canonical GHCR runtime, migration, and simulator images are public and anonymously pullable after
image-layer secret scanning; this avoids a long-lived registry credential in Azure and in local AppHosts.
NuGet and npm packages retain explicit package/repository access grants because they do not share GHCR's
anonymous public-pull behavior.

### Repository layout contracts

Each service repository uses the same small top-level vocabulary:

```text
src/                 service runtime and owned public packages
app/                 owned web/mobile surfaces, when applicable
tests/unit/
tests/integration/
apphost/             canonical standalone Aspire host
eng/                 dependency manifest, local-feed helper, boundary checks
Directory.Build.props
Directory.Packages.props
nuget.config
```

`system` uses:

```text
apphost/             full fleet from containers
tests/api/
tests/ui/
tests/mobile/
testkits/            system-only generic harness code
fleet/               image digests and compatible package/testkit versions
infra/modules/
infra/environments/{test,production}/
config/{test,production}/
scripts/
```

## Contracts, hosting metadata, and shared packages

### Service-owned published trains

- Auth publishes `Concertable.Auth.Contracts`, `Concertable.Auth.Hosting`, and, only if needed by black-box
  E2E, `Concertable.Auth.TestKit`.
- B2B publishes its existing module Contracts and Seed Contracts, plus `Concertable.B2B.Hosting` and a
  black-box TestKit.
- Customer publishes Review Contracts, Ticket Contracts, new Customer Seed Contracts, Hosting, and a
  black-box TestKit. Ticket Contracts remain in the train because Customer Hosting directly uses
  `TicketPurchasedEvent` and `SendTicketEmailCommand` in its topology metadata.
- Payment publishes Contracts, Client, Hosting, and Stripe/E2E TestKit artifacts.
- Search publishes Hosting and a TestKit. It gets a Search Contracts package only when a real external
  compile-time consumer exists; no empty symmetry package is created.

`*.Hosting` packages are composition-time only. They depend on generic platform hosting primitives and
Contracts, expose Aspire container/resource/topology registration, and require the caller to supply an
immutable image reference. They contain no runtime implementation and are rejected from production runtime
closures by the boundary check.

`*.TestKit` packages are test-only clients and fixtures for public/test-admin endpoints. They never expose
DbContexts, entities, repositories, or service internals. Full-stack tests poll observable APIs/events. If
an otherwise unobservable assertion is essential, the service owns an E2E-only admin endpoint enabled only
in the E2E environment and the TestKit owns its client.

### Platform admission rule

A package belongs in a platform repo only when it is domain-neutral, has at least two legitimate product
consumers, and can version without importing a service concept. Otherwise it stays in the service owner.
This prevents `shared` from becoming a distributed monolith.

The .NET platform packages use one repository release train and one consumer property,
`ConcertableDotNetPlatformVersion`. Service-owned package trains use distinct properties such as
`ConcertableB2BContractsVersion` and `ConcertablePaymentVersion`. Third-party versions remain centrally
managed inside each repository, not across the organization.

The frontend platform publishes independent npm packages with Changesets. B2B and Customer internal
workspace packages are not published merely because the repositories split.

## Publication, versioning, and dependency automation

- NuGet and npm packages remain immutable in GitHub Packages. Existing package IDs are retained.
- Each producer tags its own releases and uses repository-local MinVer for .NET. All packages deliberately
  released together by one producer share that producer version. Frontend packages use Changesets and
  package-specific SemVer.
- Checkpoint 0 records every published version for every retained package ID and the highest precedence
  version in each future producer train. Each filtered repository receives a deterministic bootstrap
  tag/version above that train's recorded high-water mark. CI evaluates MinVer/Changesets before publication
  and rejects a version that already exists or is not greater than the recorded baseline.
- Every main-branch package/image build produces a unique prerelease/version. A version is never overwritten.
- Service images are published to GHCR with commit-SHA and SemVer tags; deployment and AppHosts pin image
  digests, never mutable tags. Canonical images are public only after provenance, vulnerability, and secret
  scans pass; publishing still requires the owning repository's `GITHUB_TOKEN`.
- Every package workflow produces provenance, SBOM, and a clean-consumer restore/build test before publish.
- Renovate is installed across the organization. The shared preset groups packages by producer train,
  updates NuGet/npm/GitHub Actions and OCI digests, and uses custom managers for `fleet/*.yaml`.
- Patch/minor additive updates may auto-merge only after the consumer's full required CI. Major updates and
  Contract removals always require owner review. No dependency bot may merge a PR with red or missing checks.
- GitHub Packages access is granted explicitly to every consuming repository. A dedicated least-privilege
  dependency-bot credential supplies `read:packages` only if the selected Renovate hosting mode cannot use
  repository/package permissions directly. `PLATFORM_SYNC_TOKEN` and `MIRROR_PAT` are retired after cutover.

### Replacement for platform-sync

The new flow is producer pull, not central push:

```text
producer main -> publish immutable package/image -> Renovate opens consumer PRs
              -> each consumer builds/tests independently -> system updates compatible fleet digest
```

Breaking Contracts use this mandatory sequence:

1. **Expand:** add the replacement while retaining the old shape and behavior; publish a minor version.
2. **Migrate:** Renovate/manual PRs move every known consumer. Each consumer release remains compatible with
   both the old and expanded producer during the window.
3. **Prove:** the system compatibility matrix and E2E run the new producer and all migrated consumers.
4. **Contract:** remove the obsolete shape in a producer major version only after dependency inventory proves
   no supported consumer remains.
5. **Adopt:** consumers take the major version independently; old package/image versions remain available for
   rollback.

There is no equivalent of the current one-PR global platform bump. A failed update blocks only that
consumer and remains visible on the Renovate dependency dashboard.

## Standalone AppHosts and local development

Each service AppHost:

- references its own service projects and local frontends from source;
- consumes foreign `*.Hosting` packages and pinned foreign OCI images;
- provisions only its own database plus local SQL/ASB/Azurite resources needed by the topology;
- never mounts or locates sibling source directories;
- runs a producer simulator image when it needs foreign deterministic seed events; and
- exposes one `./dev.ps1` entry point plus a secret bootstrap that writes only user-secrets.

Expected foreign image composition after the Auth DB correction:

| Standalone host | Foreign images |
|---|---|
| Auth | none |
| Payment | Auth, Stripe CLI |
| Search | Auth, B2B Seed Simulator, including B2B-owned rating events |
| Customer | Auth, Payment Web/Workers, B2B Seed Simulator, Stripe CLI |
| B2B | Auth, Payment Web/Workers, Customer Seed Simulator where review data is required, Stripe CLI |

Canonical CI smoke-tests each standalone host with fresh infrastructure. A developer changing platform and
service code together publishes a unique local prerelease to a user-local feed and opts the service into
that version with a gitignored override. Cross-repo ProjectReferences, sibling-path discovery, worktrees,
and sparse checkout are not supported development architecture.

## Full-stack orchestration and E2E

`Concertable/system` is the sole owner of tests that require more than one service runtime. The present B2B,
Customer, Payment, Search, and shared E2E projects move there; unit and WebApplicationFactory/Testcontainers
integration tests stay with their service.

Before extraction, E2E must lose all service implementation ProjectReferences. The system suite may consume:

- public Contracts and service TestKit packages;
- HTTP/gRPC/OIDC and broker behavior;
- Playwright/Appium drivers;
- Stripe test-mode APIs; and
- generic SQL readiness/reset infrastructure that does not compile service EF models.

The system AppHost reads `fleet/local.yaml`, pulls images by digest, provisions the five databases and shared
emulators, applies migrations, seeds through owners/simulators, and starts the four SPAs or their preview
images. `fleet/test.yaml` and `fleet/production.yaml` are desired-state manifests for deployment. A dependency
PR is green only when the full-stack AppHost becomes healthy and affected E2E passes.

## CI/CD, environments, secrets, and deployment

### Shared workflow model

`Concertable/.github` owns reusable workflows pinned by commit SHA:

- .NET restore/build/unit/integration and architecture-boundary validation;
- npm install/lint/typecheck/unit/build;
- NuGet/npm publication with clean-consumer verification;
- OCI build, vulnerability scan, SBOM/provenance, sign, and push;
- standalone AppHost boot smoke; and
- Terraform plan/apply policy.

Each repository owns a thin calling workflow and its final `ci-complete` aggregation job. Required rulesets,
merge queue, dependency review, secret scanning, and branch naming are applied uniformly. Reusable workflow
updates arrive through Renovate rather than floating tags.

### Service CI and release

Every service PR runs the owned solution build, unit tests, affected integration tests, frontend checks when
present, package boundary checks, package pack/restore tests, and a standalone AppHost smoke test. On `main`,
the service publishes changed boundary packages, migration artifact/image, and runtime images. It does not
deploy itself to a shared environment. B2B publishes its Workers project as an Azure Functions container;
the same digest is used by standalone/system composition and Azure Functions on Container Apps.

### System CI and deployment

The system repo runs the full Docker health preflight, composes the fleet, and executes API then UI/mobile
E2E according to the existing risk-tier policy. A merged fleet-manifest PR is the only source of a deployable
multi-service release.

Terraform remains the cloud owner, consistent with `CONFIG_AND_DEPLOYMENT.md` and `DEPLOYMENT.md`:

1. GitHub OIDC authenticates to Azure; no long-lived Azure client secret is stored.
2. Terraform applies the ACA environment, five databases, Service Bus topology, storage, Key Vault, App
   Configuration, Static Web Apps, and the native Azure Functions-on-Container-Apps B2B Workers resource.
3. The workflow runs the owning service's migration job against only that service database and waits.
4. The workflow rolls container revisions to the manifest digests and verifies health.
5. SPAs deploy with environment-specific build-time public configuration.
6. A smoke journey proves Auth, a B2B write, event propagation into Customer/Search, and Payment/Stripe.

GitHub environments become `test` and `production`. `production` requires Tommy/production-owner review,
allows only protected release refs, prevents self-review where supported, and serializes deployments.
`test` may deploy automatically from a green system manifest. Azure permissions are separate per environment.

Secrets are redistributed by least privilege:

- service repositories receive only package credentials and their own integration-test secrets;
- Stripe/Google/full-fleet service-auth test secrets live only in the system E2E environment unless an owned
  service test genuinely requires one;
- canonical GHCR images are anonymous-read, so Azure and local AppHosts hold no GHCR pull credential;
- deployment uses OIDC plus managed identity, App Configuration, and Key Vault references;
- stale App Service publish profiles, `PLATFORM_SYNC_TOKEN`, and `MIRROR_PAT` are removed only after their
  rollback windows close; and
- all secret values are rotated when moved. Values are never copied through issues, plans, logs, or commits.

## Database migrations and seeding target

- Auth first moves Duende persisted grants from `B2BDb` into `AuthDb`; no runtime or migration job receives a
  foreign database connection string afterward.
- Each service repo owns its EF migrations and an idempotent migration bundle/job image. Platform Messaging
  owns Inbox/Outbox migrations; a service migration runner applies the exact platform migration assembly
  version pinned by that service to its own database.
- Runtime startup stops calling `Database.MigrateAsync`. Local AppHosts and system/deployment orchestration
  run migration resources before service readiness.
- The monorepo `initial-migrations.ps1` is split into one owner script per repository. Each script preserves
  the current unchanged-ID behavior and has a CI model-drift check.
- Production schema changes use expand/migrate/contract. Destructive schema contraction never shares a
  deployment with the code that first stops using the old schema.
- B2B and Customer own canonical seed catalogs and simulator images for outbound events. Consumers never
  seed another service's projections directly outside narrowly scoped integration projection tests.
- System seeding starts owner-local seed jobs, then producer simulators, then waits for consumer projections.
  Seed catalog/package/image versions are recorded in the fleet manifest.
- Payment and Auth keep service-local development/test seeders. They publish a simulator only if another
  service gains a real event dependency; symmetry alone is not a reason.

## Shared build conventions

`Concertable.Build` is a platform NuGet package containing MSBuild props/targets, analyzers, banned-symbol
rules, architecture tests, package metadata defaults, deterministic/source-link settings, and the guard that
rejects cross-service implementation references. It does not centrally pin third-party packages.

`@concertable/build-config` supplies frontend TypeScript, ESLint, Vitest, and Vite defaults. The `.github`
repo supplies CI behavior. Each consumer pins both and Renovate updates them. Minimal local
`Directory.Build.props`/`Directory.Packages.props` and package-manager lockfiles stay in every repo so a clone
is independently reproducible.

## Git history preservation

Filtering rewrites commit IDs, so the archived monorepo remains the authoritative map from original SHAs.
The extraction is reproducible and audited:

1. Tag the final monorepo source state `monorepo-cutover/<date>` and create a signed bundle backup.
2. Commit `eng/repository-split/map.yaml` describing every included/excluded path and target rename.
3. Run pinned `git-filter-repo` in clean disposable clones. Preserve authors, author/committer dates, messages,
   merge topology where representable, and tags relevant to the selected paths.
4. Emit and retain filter-repo commit maps, path maps, object counts, earliest/latest commit checks, and
   sampled blame comparisons.
5. Push every result to a temporary `*-next` repository first. No existing default branch is force-pushed.
6. Verify clean clone/build/test/package restore, history counts, LFS/submodule absence, secret scan, and the
   exact cutover tree against the source mapping.
7. At the approved cutover, rename the old generated mirror to
   `<name>-mirror-archive-<date>` and rename `<name>-next` to the canonical name. This keeps the old repository
   and repo ID recoverable without destructive rewriting.

Path ownership for extraction is:

| Target | Included source paths |
|---|---|
| B2B | `api/Concertable.B2B` excluding full-stack E2E; `app/web/b2b`; `app/mobile/b2b` |
| Customer | `api/Concertable.Customer` excluding full-stack E2E; `app/web/customer`; `app/mobile/customer`; `app/customer/shared` |
| Payment | `api/Concertable.Payment` excluding full-stack E2E helpers |
| Search | `api/Concertable.Search` excluding full-stack E2E helpers |
| Auth | `api/Concertable.Auth`; `api/Concertable.Auth.Contracts` |
| platform-dotnet | `api/Concertable.Shared`; `api/Concertable.Messaging`; `api/Concertable.DataAccess`; `api/Concertable.ServiceDefaults`; generic portions of `api/Concertable.AppHost.Shared` |
| platform-web | `app/shared`; packageized `app/web/shared`; frontend build configuration |
| system | umbrella AppHost; all current full-stack E2E/helper paths; E2E/docker scripts; fleet IaC/config/deployment history |
| `.github` | reusable portions of current workflows and policy files, with monorepo-specific jobs excluded |

Files needed by more than one target may legitimately have history in more than one filtered repository, but
only one target owns the live file after cutover.

## Compatibility, rollback, and archive policy

### Compatibility windows

- An additive Contract version remains supported until every known consumer is released on it and the system
  matrix is green. Removal then requires a separate major release.
- During a service cutover, both the last monorepo image and first canonical-repo image must run against the
  same consumer Contract and database schema.
- The system fleet retains at least the last known-green and candidate image digests and package matrix.
- Database expansion precedes code rollout; contraction follows only after rollback to the old image is no
  longer required.

### Rollback levels

1. Before a target repository's first canonical commit, rollback is a repository rename swap plus re-enabling
   the mirror. No source reconciliation is needed.
2. After canonical commits but before monorepo source removal, cherry-pick/replay those commits back into the
   frozen monorepo path, verify, then explicitly re-enable mirroring. Never overwrite canonical work with the
   old force-push workflow.
3. After source removal, operational rollback is a system-manifest PR to the last known-green image digests
   and compatible migration/schema state. Repository topology is not automatically reversed.
4. Break-glass source rollback restores the signed monorepo bundle/tag into a new recovery repository; it
   never unarchives and force-pushes the historical monorepo silently.

### Old repository retirement

The monorepo remains canonical and writable through preparation. During service cutovers, migrated paths are
frozen by CI/CODEOWNERS and then removed after the system consumes the canonical image. After all services are
gone, the monorepo receives a final README/topology map, disables package/mirror/deploy workflows, rotates and
removes obsolete credentials, marks packages as retained historical artifacts, and becomes read-only and
archived. Generated mirror archives remain private/read-only through the rollback window, then are archived;
they are not deleted.

## Exact independently mergeable checkpoints

Every numbered checkpoint is a separate PR/merge boundary. Complete its verification, commit it, open or
update the PR, and stop. Cross-repository letters within a checkpoint are also ordered merge boundaries;
never merge a later letter before the earlier one is green. Tommy must explicitly instruct every merge.

**Checkpoint numbering is final delivery order, not permission to idle.** Preparation for checkpoints
10–14 runs in parallel whenever the target repository and exact producer artifacts exist. Each `*-next`
owner may independently land repository-local CI, build/test entry points, package and image publication
setup, migrations, Hosting/TestKit, seed contracts/simulators, documentation, and repository-settings
evidence. Record implementation dependencies separately from delivery gates and keep the result
`implementable, delivery-gated` until its published-baseline revalidation is possible.

The irreversible cutover letters remain ordered: do not freeze or remove monorepo source, rename or change
repository/package/image visibility, publish a canonical release, change system consumption, migrate live
data, or deploy production before the preceding checkpoint gates and explicit authorization are satisfied.
RT3 and Stage 4 are verification/cutover dependencies; they do not block repository-local preparation in a
private extraction proof.

### 0. Baseline, permissions, and reproducible inventory (`concertable`)

- Commit a machine-readable project/package/workspace/AppHost/E2E/migration/seed graph and a drift-checking
  generator.
- Record package IDs, every published version and per-producer high-water mark, visibility, linked
  repositories, and Actions access. Record GHCR names, visibility, source linkage, Actions access, and every
  intended pull consumer. Record existing repo metadata, rulesets, environments, and secret names without
  reading secret values.
- Commit the version-baseline manifest and prove the planned initial MinVer/Changesets version for every
  future producer is unique and greater than every retained package ID's recorded high-water mark.
- Repair or intentionally refresh the currently stale mirrors; record the final parity SHA.
- Commit the pinned filter-repo version and source-to-target map; perform local extraction dry runs only.
- Verification: graph regeneration has no diff; all five service carve builds, package clean-consumer restore,
  version-collision validation, public GHCR anonymous-pull probe, workflow/schema validation, and mirror
  parity are green. E2E is skipped because behavior is unchanged.
- **Hard stop:** review the inventory, package ACL report, and extraction reports before creating target repos.

### 1. Enforce database ownership and owner-local migration commands (`concertable`)

- Move Duende persisted grants from `B2BDb` to `AuthDb`; remove Auth's B2B connection and all foreign B2B DB
  resources from Auth/Payment/Search/Customer hosts where they existed only for Auth.
- Split migration scaffolding into platform and per-service commands while retaining a root delegator during
  compatibility.
- Add service migration bundle/job projects and remove runtime `MigrateAsync` only after AppHosts invoke the
  migration resources.
- Verification: umbrella and all standalone AppHosts build/boot; migration model-drift tests; affected unit
  and integration suites; full API then UI E2E because auth persistence and startup ordering changed.
- **Hard stop:** Auth must start with only `AuthDb`, and all five owned DBs must migrate from empty.

### 2. Publish the container-hosting seam (`concertable`)

- Split generic `AppHost.Shared` code into package-clean `Concertable.Hosting` and service-owned
  `*.Hosting` projects.
- Add source-vs-image switches to standalone hosts; own service stays `AddProject`, foreign services use
  `AddContainer` with explicit image digests.
- Publish all service runtime, worker, SPA-preview where needed, migration, and simulator images from the
  monorepo as a temporary bridge.
- Add a boundary test proving runtime closures cannot reference Hosting/TestKit or foreign source.
- Verification: pack/restore Hosting packages; build all service solutions; boot every standalone host once
  in image mode; affected integration tests. Run API E2E against image mode; UI E2E only if endpoints or SPA
  hosting differ.
- **Hard stop:** no standalone AppHost has a foreign `ProjectReference`.

### 3. Complete producer-owned seeding (`concertable`)

- Add Customer Seed Contracts/catalog/simulator for Customer-origin review/rating events required by Search
  or B2B.
- Make B2B/Customer/Search standalone image-mode hosts provision projections only through producer simulator
  events. Keep direct projection seeding integration-test-only.
- Publish simulator images and add seed parity/idempotency tests.
- Verification: producer unit tests, B2B/Customer/Search integration tests, standalone seed smoke, and full
  API E2E for event convergence.
- **Hard stop:** every foreign projection needed by a canonical standalone host has an owner simulator.

### 4. Remove service implementation references from full-stack E2E (`concertable`)

- Introduce minimal service TestKits/test-admin seams and migrate E2E polling/reset/setup away from service
  DbContexts, entities, AppHost extensions, and runtimes.
- Make the umbrella AppHost's image mode the E2E default while preserving source mode temporarily for local
  diagnosis.
- Verification: all TestKit pack/restore tests, service integration suites, full API E2E, full UI E2E, and
  the mobile scenario where its harness changed.
- **Hard stop:** a generated E2E-only carve builds with no service source folders present.

### 5. Publish frontend platform boundaries (`concertable`)

- Convert `app/web/shared` to `@concertable/web-shared`; make `@concertable/shared` independently packable;
  add `@concertable/build-config`.
- Keep `@b2b/*` and `@customer/shared` local to their owning product workspaces.
- Add npm clean-consumer tests and a local prerelease feed workflow.
- Verification: lint/typecheck/unit/build for platform packages and all four SPAs plus both mobile apps.
  Behavior-preserving package extraction does not require E2E.
- **Hard stop:** product frontends contain no source alias escaping their future repository.

### 6. Organization repository and workflow foundation (`.github` then target `*-next` repos)

- 6A: create public `Concertable/.github`, reusable workflows, Renovate preset, ruleset/environment templates,
  teams, and bootstrap CODEOWNERS. Verify a disposable public fixture consumes every reusable workflow.
- 6B: create private `platform-dotnet-next`, `platform-web-next`, `system-next`, and five service `*-next`
  repositories. Apply least-privilege Actions/package settings and `main` merge-queue rulesets.
- 6C: push filtered histories and reports; run secret scans and clean-clone builds. Do not rename an existing
  repository or make a target canonical.
- Each approved canonical rename also changes the verified `*-next` repository to public and makes its
  scanned GHCR images public. Visibility promotion never happens during preparation or implicitly on publish.
- Verification: history audit, package-auth probe, workflow fixture, all target clean-clone builds.
- **Hard stop:** Tommy reviews every history/ACL/repository-settings report before any canonical name swap.

### 7. Cut over the .NET platform publisher

- 7A (`platform-dotnet-next`): land platform source, `Concertable.Build`, CI, and package publication under a
  new repository release train; apply the checkpoint-0 bootstrap baseline, publish the first non-conflicting
  version, and prove clean restore.
- 7B (`concertable`): replace the global pin with `ConcertableDotNetPlatformVersion`, consume the new release
  in all five service closures, and stop the monorepo publishing those package IDs.
- 7C (GitHub): rename the stale `shared` mirror to `shared-mirror-archive-<date>` and
  `platform-dotnet-next` to `platform-dotnet`; update package links/Actions access.
- Verification: platform unit/integration tests, pack/restore; all five service builds and integration suites;
  umbrella build. E2E is skipped unless runtime package behavior changed.
- **Hard stop:** only `platform-dotnet` can publish platform package IDs.

### 8. Cut over the frontend platform publisher

- 8A (`platform-web-next`): land history, CI, Changesets publication, and publish initial package versions.
- 8B (`concertable`): switch every product workspace to registry packages, generate stable per-repo-ready
  lockfiles, and remove monorepo publication for those IDs.
- 8C (GitHub): rename `platform-web-next` to `platform-web` and update package links.
- Verification: clean npm installs, package tests, all four SPA builds, both mobile builds/tests.
- **Hard stop:** product builds succeed with the platform source directories absent.

### 9. Make the system repository canonical

- 9A (`system-next`): land filtered full-stack AppHost/E2E history, container-only composition,
  `fleet/local.yaml`, Docker health gate, and black-box API/UI/mobile tests.
- 9B (`system-next`): land the existing Terraform/config/deployment design under system ownership, validate
  plans for test/production, create protected GitHub environments, and perform a test-environment plan or
  ephemeral deployment when Azure credentials/resources are available.
- 9C (GitHub): rename `system-next` to `system`; enable Renovate image/package PRs.
- 9D (`concertable`): remove the umbrella AppHost/full-stack E2E ownership and retain only a pointer during
  the remaining service cutovers.
- Verification: clean-clone system build; full container API then UI E2E; mobile affected test; Terraform
  fmt/validate/plan; deployment smoke if an environment is available.
- **Hard stop:** `system` is green using monorepo-produced images before the first service source cut.

### 10. Promote Auth

- 10A (`concertable`): refresh Auth's extraction at the approved SHA, freeze Auth source, and remove Auth from
  mirror automation. Do not delete source yet.
- 10B (`auth-next`): rebase the verified extraction on that SHA; land CI, Auth-owned publication/images,
  standalone AppHost, migrations, Hosting/TestKit, rules, and main branch.
- 10C (GitHub): rename `auth` to `auth-mirror-archive-<date>` and `auth-next` to `auth`; transfer package/image
  permissions; publish a canonical Auth release.
- 10D (`system`): Renovate/manual PR updates Auth packages and image digest; run full affected E2E.
- 10E (`concertable`): consume canonical Auth Contracts where still needed, stop duplicate Auth publication,
  then remove frozen Auth source.
- Verification: Auth build/unit/integration/AppHost/migrations; every remaining service build; system Auth and
  login flows, API/UI E2E.
- **Hard stop:** one canonical Auth repo, one Auth DB, one Auth publisher.

### 11. Promote Payment

Repeat 10A-10E for Payment. Its target owns Web, Workers, Contracts, Client, migrations, Stripe tooling,
images, and AppHost. Update B2B/Customer/system independently through published Payment artifacts.

- Verification: Payment build/unit/integration/AppHost/migrations; B2B and Customer builds/integration;
  system Stripe webhook and payment API/UI flows.
- **Hard stop:** no remaining source or image is published for Payment by the monorepo.

### 12. Promote Search

Repeat 10A-10E for Search. Its standalone host consumes Auth plus B2B simulator images and published
Contracts; no producer source or database. Search's rating inputs are B2B-owned events, so the B2B simulator
must replay them; do not add a direct Customer simulator dependency unless a separately approved contract
change makes Customer the producer Search actually consumes.

- Verification: Search build/unit/integration/AppHost/migrations and seed convergence; system search
  projection API/UI flows.
- **Hard stop:** Search rebuilds all projections from producer events in a clean standalone run.

### 13. Promote Customer

Repeat 10A-10E for Customer, including customer web/mobile, `@customer/shared`, Review/Ticket/Seed Contracts,
simulator, and all Customer migrations. B2B remains compatible with the published Customer contract train.

Customer preparation starts from the reviewed private `customer` extraction proof and runs in parallel
with RT3, Stage 4, and the earlier service promotions. Final 13A–13E delivery remains gated on the canonical
platform/system baselines and the preceding service cutovers; those gates do not prevent Customer-owned CI,
publication setup, migration/simulator closure, or standalone verification from being completed beforehand.

- Verification: Customer backend/frontend/mobile build and unit/integration; standalone AppHost; migration
  and simulator tests; system customer purchase/review API/UI/mobile flows.
- **Hard stop:** B2B and Search consume only published Customer artifacts/images.

### 14. Promote B2B

Repeat 10A-10E for B2B last because it has the widest Contract/seed fan-out. Include all manager web apps,
B2B mobile, B2B shared workspace, module Contracts, migrations, and simulator.

- Verification: full B2B backend/frontend/mobile build and unit/integration; standalone AppHost; all B2B
  migrations and simulator parity; full system API and UI E2E plus affected mobile tests.
- **Hard stop:** the system fleet contains no monorepo-built service image.

### 15. Prove deployment and rollback from canonical repositories (`system`)

- Deploy an ephemeral test environment from canonical image digests, run all migration jobs in owner order,
  seed through owner jobs/simulators, run smoke plus full E2E, then exercise rollback to the prior fleet
  manifest and destroy if using ephemeral mode.
- Verify production environment protection, OIDC, Key Vault/App Configuration access, migration logs, image
  provenance, and rollback runbook without exposing secrets.
- **Hard stop:** Tommy reviews the deployment/rollback evidence before monorepo archival.

### 16. Archive the monorepo and generated mirrors (`concertable` and GitHub settings)

- Remove obsolete workflows and credentials; disable Actions that can publish/mirror/deploy; add the final
  topology/history/package map and cutover tag/bundle reference.
- Confirm no package, image, workflow, AppHost, E2E, environment, or deployment still depends on the
  monorepo.
- Archive `Concertable/concertable` and the six dated mirror archives. Retain package/image versions and
  signed source bundles.
- Verification: clean clones/builds of all nine canonical repos; Renovate dry-run/dashboard; full system E2E;
  test deployment smoke; GitHub audit of rulesets/environments/secrets/package ACLs.
- **Hard stop:** archival is the terminal checkpoint. The plan is deleted in the commit that records the
  completed, verified migration in its final owning repository.

## Execution rules

- Work one checkpoint only per session unless Tommy explicitly names a later checkpoint and says to execute
  it now.
- At each boundary, pull current defaults, prove no red dependency-sync/migration state, implement, run the
  listed build and affected tests, commit, and stop with the next exact resume prompt.
- Package producer changes publish before consumer changes. Image producer releases precede system-manifest
  updates. Schema expansion precedes runtime adoption.
- A red build/test/package/deployment gate stops the checkpoint. Do not paper over it with a path reference,
  mutable image tag, copied source, disabled check, or global version pin.
- Never merge, auto-merge, rename a canonical repository, archive a repository, rotate/delete a credential,
  or deploy production without the explicit instruction applicable to that action.
