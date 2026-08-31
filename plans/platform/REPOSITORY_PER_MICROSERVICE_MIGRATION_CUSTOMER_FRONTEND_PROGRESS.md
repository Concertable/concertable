# Repository-per-microservice migration — Customer progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\customer`
- Branch: `Chore/customer-promotion-preparation`
- PR: draft [`Concertable/customer#1`](https://github.com/Concertable/customer/pull/1), exact head `1b6e49f7460ac61795ffe3767591af7f0bfdbdd0`
- Dependency/package gates: the recorded NuGet and npm Actions-access closure, including `Concertable.AppHost.Shared`, is granted; final checkpoint 13 publication and delivery remain gated on explicit authorization and the plan's platform/system prerequisites
- Last reconciled: **2026-08-31** from repository ID `1351337130`, exact remote/PR head equality, CI run `33442759487`, and the completed Customer seed-contract/simulator slice

## Current state

State: **repository preparation active; package gate cleared**. GitHub repository `Concertable/customer-next`
was renamed in place to canonical `Concertable/customer`; repository ID `1351337130`, PR #1, branches, and
history were preserved. The inactive local checkout moved from `customer-next` to `customer`, and its origin
now uses `https://github.com/Concertable/customer.git`.

The extraction proof at `e21ae9079ca2fdd3a0063a252f05499159d608ff` contains the Customer backend,
web, mobile, customer-only shared package, and standalone support closure. Draft PR #1 validates the owned
build, tests, migration snapshots, current package candidates, and Customer Web and migrations OCI image
candidates. Package-level Actions read access is granted for the exact NuGet and npm closures recorded below.
Exact-head CI run [`33442759487`](https://github.com/Concertable/customer/actions/runs/33442759487) is fully
green at `1b6e49f7460ac61795ffe3767591af7f0bfdbdd0`: Frontend completed in 2m17s and Backend in 6m00s. The
backend job builds and tests the carve, validates migration snapshots, restores a clean consumer from all four
Customer package candidates, builds Web/Migrations/Seed Simulator OCI candidates, loads and runs the real
simulator image, and proves the migration job against an empty database. No package or image was published.

No agent following this ledger may monitor or edit RT3, Stage 4 fleet E2E, Auth, Payment, Search, or another
stream's ledger. This file is the exclusive durable record for Customer; the temporary Customer promotion
ledger was retired after its live evidence was consolidated here.

## Next Steps

Continue draft PR #1 from exact head `1b6e49f7460ac61795ffe3767591af7f0bfdbdd0` with one Customer-only
artifact-integrity slice. For the four NuGet and three OCI candidates, generate one deterministic manifest of
artifact names and SHA-256 hashes, create SBOM evidence, and scan each OCI archive for vulnerabilities and
embedded secrets using pinned tool/action versions. Retain the manifest, SBOMs, and scan reports as CI run
artifacts with an explicit retention period, and fail CI on a missing candidate, hash mismatch, embedded
secret, or High/Critical vulnerability. Do not add an ignore or allowlist mechanism in this slice. Do not grant
write/package/attestation permissions and do not publish or push any candidate. This slice must not edit
standalone AppHost, TestKit/fleet E2E, or another service's source.

## Completed work

- Customer backend, web, mobile, and `@concertable/customer` histories were folded into the private
  repository; local Customer workspaces use `file:` linkage and external
  `@concertable/{shared,web,mobile}` dependencies use the published `alpha` channel.
- `b63a311` made the extracted workspace standalone with its root manifest, lockfile, package feed, ignore
  state, production environment seam, Vite helper, route tree, and canonical `CarveCustomer.slnx`.
- `b484496` restored the production URL closure and Expo assets; `e21ae90` retired the obsolete force-push
  handoff; `39ca980` configured repository-scoped package authentication in CI.
- The repository and local checkout now use the canonical `customer` name.
- The package administrator granted `Concertable/customer` Actions read access to all 39 recorded NuGet
  packages, including `Concertable.AppHost.Shared`, and `@concertable/{mobile,shared,web}`.
- `9e23956` prevents Vitest's `serve`/`test` configuration load from invoking the trusted development-certificate
  requirement while preserving HTTPS for the real Vite development server.
- `2ecc33c` adds serialized backend tests, validates the current three-package candidate set from an isolated
  consumer, and builds a Customer Web OCI archive candidate without publishing packages or images.
- `97aec2b` adds the dedicated `customer-migrations` job/image candidate, isolates migration-only service
  registration from runtime startup, and keeps the runtime fallback until AppHost orchestration invokes the
  migration resource.
- `1b6e49f` adds the downward-only `Concertable.Customer.Seed.Contracts` package and deterministic Customer
  seed simulator, drives Customer seed state and the simulator from one review spec, and adds parity,
  idempotency, clean-consumer, and real OCI load/run gates without publishing artifacts.

## Verification

- Exact-head remote CI run `33442759487`: Frontend 2m17s and Backend 6m00s, fully green at
  `1b6e49f7460ac61795ffe3767591af7f0bfdbdd0`; all four package and three OCI outputs were candidates only.
- Backend verification covered the zero-warning build, full tests, seven migration snapshots, four-package
  isolated clean-consumer restore/build, real Seed Simulator OCI load/run, and the Development-mode
  empty-database migration job. The simulator container exited successfully.
- Frontend verification covered clean install, shared package tests/build, web test/build, and mobile
  typecheck/export.
- Earlier standalone proof: 51-project Release build; seven migration snapshots; `npm ci`; shared 3/3;
  web 1/1 and production build; mobile typecheck and Android export — all green.
- Local validation at `1b6e49f` covered a zero-warning carve build, all prior full tests plus focused seed
  tests 4/4, the four-package clean consumer, and a real local simulator OCI load/run with exit code 0.

## Reviews

- Full and incremental extraction review completed through `e21ae9079ca2fdd3a0063a252f05499159d608ff`
  with all findings resolved.
- Repository-preparation review completed through `39ca980f375b5661ed7da114297f45b909915851` with no open findings.
- Independent artifact-gate review through `2ecc33cb533a95b3baa209dcdc259c6e27e81105` has no open findings
  after reconciling the current and final Customer package rosters.
- Independent re-review through `1b6e49f7460ac61795ffe3767591af7f0bfdbdd0` found no issues. Draft PR #1
  still owns the cumulative delivery gate before any merge.

## Decisions, discoveries, blockers, and deviations

- Repository ID `1351337130` survived the canonical rename. Reuse
  `C:\Users\tommy\source\repos\customer`; do not create another clone or rewrite private `main`.
- Exact NuGet package ACL closure:
  - `Concertable.AppHost.Shared`
  - `Concertable.Auth.Contracts`
  - `Concertable.B2B.Artist.Contracts`, `Concertable.B2B.Concert.Contracts`, `Concertable.B2B.Seed.Contracts`, `Concertable.B2B.Tenant.Contracts`, `Concertable.B2B.User.Contracts`, `Concertable.B2B.Venue.Contracts`
  - `Concertable.Contracts`
  - `Concertable.DataAccess.Application`, `Concertable.DataAccess.Infrastructure`
  - `Concertable.Grpc`, `Concertable.Kernel`
  - `Concertable.Messaging.Application`, `Concertable.Messaging.AzureServiceBus`, `Concertable.Messaging.Contracts`, `Concertable.Messaging.Domain`, `Concertable.Messaging.Infrastructure`
  - `Concertable.Payment.Client`, `Concertable.Payment.Contracts`
  - `Concertable.Seed.Identity`, `Concertable.Seed.Shared`, `Concertable.ServiceDefaults`
  - `Concertable.Shared.Api`
  - `Concertable.Shared.Blob.Application`, `Concertable.Shared.Blob.Infrastructure`
  - `Concertable.Shared.Email.Application`, `Concertable.Shared.Email.Infrastructure`
  - `Concertable.Shared.Geocoding.Application`, `Concertable.Shared.Geocoding.Infrastructure`
  - `Concertable.Shared.Imaging.Application`, `Concertable.Shared.Imaging.Infrastructure`
  - `Concertable.Shared.Notification.Infrastructure`
  - `Concertable.Shared.Pdf.Application`, `Concertable.Shared.Pdf.Infrastructure`
  - `Concertable.Shared.QrCode.Application`, `Concertable.Shared.QrCode.Infrastructure`
  - `Concertable.Testing`, `Concertable.Testing.Integration`
- Exact npm package ACL closure: `@concertable/mobile`, `@concertable/shared`, `@concertable/web`.
- Current package candidate set: `Concertable.Customer.Hosting`, `Concertable.Customer.Review.Contracts`,
  `Concertable.Customer.Ticket.Contracts`, and `Concertable.Customer.Seed.Contracts`. The final Customer
  train still requires a black-box Customer TestKit, which remains outstanding. Ticket Contracts are
  intentional because Hosting directly uses
  `TicketPurchasedEvent` and `SendTicketEmailCommand`.
- Exact-head CI now creates local Customer Web, `customer-migrations`, and `customer-seed-simulator` archives.
  The simulator smoke uses `docker load` and `docker run --rm` against the built archive; it is not a source-only
  substitute. Runtime `MigrateAsync` remains as a temporary fallback until the standalone AppHost invokes the
  migration resource. No package, image, canonical release, visibility change, deployment, or system-consumer
  update was authorized or performed.
- Vitest invokes Vite with `command = serve` and `mode = test`; development-only configuration must consider
  both values rather than treating every `serve` configuration load as a live dev server.
- A multi-path fold must include support files outside selected app subtrees: Customer's relocated Vite app
  uses `app/.env.production`, and Expo's unchanged `../assets/*` references require `app/assets/`.
- This ledger has no write ownership over the monorepo RT3 or fleet branches.
