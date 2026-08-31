# Repository-per-microservice migration — Customer progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\customer`
- Branch: `Chore/customer-promotion-preparation`
- PR: draft [`Concertable/customer#1`](https://github.com/Concertable/customer/pull/1), exact head `9e23956120f5e8c0ca6599c963f758648b42c366`
- Dependency/package gates: the recorded NuGet and npm Actions-access closure is granted; final checkpoint 13 delivery remains gated on its explicit platform/system prerequisites and authorization
- Last reconciled: **2026-08-31** from repository ID `1351337130`, exact remote/PR head equality, CI run `33426383173`, and completed incremental review

## Current state

State: **repository preparation active; package gate cleared**. GitHub repository `Concertable/customer-next`
was renamed in place to canonical `Concertable/customer`; repository ID `1351337130`, PR #1, branches, and
history were preserved. The inactive local checkout moved from `customer-next` to `customer`, and its origin
now uses `https://github.com/Concertable/customer.git`.

The extraction proof at `e21ae9079ca2fdd3a0063a252f05499159d608ff` contains the Customer backend,
web, mobile, customer-only shared package, and standalone support closure. Draft PR #1 adds the repository
CI and migration-validation preparation. Package-level Actions read access is now granted for the exact
NuGet and npm closures recorded below. Exact-head CI run
[`33426383173`](https://github.com/Concertable/customer/actions/runs/33426383173) is green at
`9e23956120f5e8c0ca6599c963f758648b42c366`.

No agent following this ledger may monitor or edit RT3, Stage 4 fleet E2E, Auth, Payment, Search, or another
stream's ledger. This file is the exclusive durable record for Customer; the temporary Customer promotion
ledger was retired after its live evidence was consolidated here.

## Next Steps

Continue draft PR #1 from exact head `9e23956120f5e8c0ca6599c963f758648b42c366` with the next
checkpoint-13 Customer-owned repository-preparation slice. Prefer work that is independent of sibling
streams: repository settings/evidence, Customer-owned publication and images, Hosting/TestKit and
Review/Seed contract closure, and the Customer simulator. Defer only a gate that names the exact missing
sibling artifact or required authorization; do not wait on another stream merely because the overall
checkpoint is ordered.

## Completed work

- Customer backend, web, mobile, and `@concertable/customer` histories were folded into the private
  repository; local Customer workspaces use `file:` linkage and external
  `@concertable/{shared,web,mobile}` dependencies use the published `alpha` channel.
- `b63a311` made the extracted workspace standalone with its root manifest, lockfile, package feed, ignore
  state, production environment seam, Vite helper, route tree, and canonical `CarveCustomer.slnx`.
- `b484496` restored the production URL closure and Expo assets; `e21ae90` retired the obsolete force-push
  handoff; `39ca980` configured repository-scoped package authentication in CI.
- The repository and local checkout now use the canonical `customer` name.
- The package administrator granted `Concertable/customer` Actions read access to all 38 recorded NuGet
  packages and `@concertable/{mobile,shared,web}`.
- `9e23956` prevents Vitest's `serve`/`test` configuration load from invoking the trusted development-certificate
  requirement while preserving HTTPS for the real Vite development server.

## Verification

- Focused local regression at `9e23956`: `npm -w @concertable/web-customer test` — 1/1 passed.
- Exact-head remote CI run `33426383173`: Backend and migrations green; Frontend green.
- The preceding rerun proved the package repair directly: backend restore/build/migration validation became
  green and frontend `npm ci` completed before exposing the separate Vitest certificate defect.
- Earlier standalone proof: 51-project Release build; seven migration snapshots; `npm ci`; shared 3/3;
  web 1/1 and production build; mobile typecheck and Android export — all green.

## Reviews

- Full and incremental extraction review completed through `e21ae9079ca2fdd3a0063a252f05499159d608ff`
  with all findings resolved.
- Repository-preparation review completed through `39ca980f375b5661ed7da114297f45b909915851` with no open findings.
- Independent incremental review of `39ca980f375b5661ed7da114297f45b909915851..9e23956120f5e8c0ca6599c963f758648b42c366`
  found no issues and confirmed test mode is excluded while real development HTTPS remains enabled.

## Decisions, discoveries, blockers, and deviations

- Repository ID `1351337130` survived the canonical rename. Reuse
  `C:\Users\tommy\source\repos\customer`; do not create another clone or rewrite private `main`.
- Exact NuGet package ACL closure:
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
- Vitest invokes Vite with `command = serve` and `mode = test`; development-only configuration must consider
  both values rather than treating every `serve` configuration load as a live dev server.
- A multi-path fold must include support files outside selected app subtrees: Customer's relocated Vite app
  uses `app/.env.production`, and Expo's unchanged `../assets/*` references require `app/assets/`.
- This ledger has no write ownership over the monorepo RT3 or fleet branches.
