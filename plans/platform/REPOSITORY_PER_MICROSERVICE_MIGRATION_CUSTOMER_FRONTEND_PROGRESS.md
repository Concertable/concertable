# Repository-per-microservice migration — Customer progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\customer`
- Branch: `Chore/customer-promotion-preparation`
- PR: draft [`Concertable/customer#1`](https://github.com/Concertable/customer/pull/1), exact head `2ecc33cb533a95b3baa209dcdc259c6e27e81105`
- Dependency/package gates: the recorded NuGet and npm Actions-access closure, including `Concertable.AppHost.Shared`, is granted; final checkpoint 13 publication and delivery remain gated on explicit authorization and the plan's platform/system prerequisites
- Last reconciled: **2026-08-31** from repository ID `1351337130`, exact remote/PR head equality, CI run `33431717338`, and completed artifact-gate review

## Current state

State: **repository preparation active; package gate cleared**. GitHub repository `Concertable/customer-next`
was renamed in place to canonical `Concertable/customer`; repository ID `1351337130`, PR #1, branches, and
history were preserved. The inactive local checkout moved from `customer-next` to `customer`, and its origin
now uses `https://github.com/Concertable/customer.git`.

The extraction proof at `e21ae9079ca2fdd3a0063a252f05499159d608ff` contains the Customer backend,
web, mobile, customer-only shared package, and standalone support closure. Draft PR #1 now validates the
owned build, tests, migrations, current package candidates, and Customer Web OCI image candidate. Package-level
Actions read access is granted for the exact NuGet and npm closures recorded below, including the later
`Concertable.AppHost.Shared` dependency. Exact-head CI run
[`33431717338`](https://github.com/Concertable/customer/actions/runs/33431717338) is green at
`2ecc33cb533a95b3baa209dcdc259c6e27e81105`. It built, but did not publish, the OCI image candidate and the
current `Hosting`, `Review.Contracts`, and `Ticket.Contracts` package candidates. Actual package or image
publication remains unauthorized.

No agent following this ledger may monitor or edit RT3, Stage 4 fleet E2E, Auth, Payment, Search, or another
stream's ledger. This file is the exclusive durable record for Customer; the temporary Customer promotion
ledger was retired after its live evidence was consolidated here.

## Next Steps

Continue draft PR #1 from exact head `2ecc33cb533a95b3baa209dcdc259c6e27e81105` with one owner-local
migration slice: add an idempotent Customer migration job project/image candidate that applies the seven
Customer contexts plus the exact pinned Inbox/Outbox migration assemblies to Customer's database, include it
in the standalone carve and CI candidate build, and prove it migrates an empty database. Keep runtime
`MigrateAsync` calls until the standalone AppHost invokes the migration resource. Do not publish or push the
image; publication remains an explicit later authorization gate.

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

## Verification

- Exact-head remote CI run `33431717338`: Backend/tests/packages/image and Frontend jobs are fully green at
  `2ecc33cb533a95b3baa209dcdc259c6e27e81105`; the package and OCI outputs were candidates only.
- Backend verification covered restore/build, serialized tests, seven migration snapshots, the three-package
  isolated clean-consumer restore/build, and a non-empty Customer Web OCI archive.
- Frontend verification covered clean install, shared package tests/build, web test/build, and mobile
  typecheck/export.
- Earlier standalone proof: 51-project Release build; seven migration snapshots; `npm ci`; shared 3/3;
  web 1/1 and production build; mobile typecheck and Android export — all green.

## Reviews

- Full and incremental extraction review completed through `e21ae9079ca2fdd3a0063a252f05499159d608ff`
  with all findings resolved.
- Repository-preparation review completed through `39ca980f375b5661ed7da114297f45b909915851` with no open findings.
- Independent artifact-gate review through `2ecc33cb533a95b3baa209dcdc259c6e27e81105` has no open findings
  after reconciling the current and final Customer package rosters.

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
- Current package candidate set: `Concertable.Customer.Hosting`,
  `Concertable.Customer.Review.Contracts`, and `Concertable.Customer.Ticket.Contracts`. The final Customer
  train also requires new `Concertable.Customer.Seed.Contracts` and a black-box Customer TestKit; those two
  artifacts remain outstanding. Ticket Contracts are intentional because Hosting directly uses
  `TicketPurchasedEvent` and `SendTicketEmailCommand`.
- The exact-head OCI proof created a local Customer Web archive inside CI only. No package, image, canonical
  release, visibility change, deployment, or system-consumer update was authorized or performed.
- Vitest invokes Vite with `command = serve` and `mode = test`; development-only configuration must consider
  both values rather than treating every `serve` configuration load as a live dev server.
- A multi-path fold must include support files outside selected app subtrees: Customer's relocated Vite app
  uses `app/.env.production`, and Expo's unchanged `../assets/*` references require `app/assets/`.
- This ledger has no write ownership over the monorepo RT3 or fleet branches.
