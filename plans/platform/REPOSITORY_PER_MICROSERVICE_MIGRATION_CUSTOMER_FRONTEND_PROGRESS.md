# Repository-per-microservice migration — Customer progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\customer`
- Branch: `Chore/customer-promotion-preparation`
- PR: draft [`Concertable/customer#1`](https://github.com/Concertable/customer/pull/1), exact head `5555ac82b314384685a7a003fa5bc82e18fa8298`
- Dependency/package gates: package access is granted; Actions artifact retention is blocked by the organization storage quota; final publication and delivery remain unauthorized
- Last reconciled: **2026-09-01** from exact remote/PR head equality and failed-job rerun attempt 2 of CI run `33448642947`

## Current state

State: **repository preparation active; artifact retention blocked by organization quota**. GitHub repository `Concertable/customer-next`
was renamed in place to canonical `Concertable/customer`; repository ID `1351337130`, PR #1, branches, and
history were preserved. The inactive local checkout moved from `customer-next` to `customer`, and its origin
now uses `https://github.com/Concertable/customer.git`.

The extraction proof at `e21ae9079ca2fdd3a0063a252f05499159d608ff` contains the Customer backend,
web, mobile, customer-only shared package, and standalone support closure. Draft PR #1 validates the owned
build, tests, migration snapshots, current package candidates, and Customer Web and migrations OCI image
candidates. Package-level Actions read access is granted for the exact NuGet and npm closures recorded below.
Exact-head CI run [`33448642947`](https://github.com/Concertable/customer/actions/runs/33448642947) ran at
`5555ac82b314384685a7a003fa5bc82e18fa8298`. Failed-job rerun attempt 2 restarted Backend job
`99686369441`; Frontend job `99686370507` remained green. Backend again passed every build, test, package,
image, migration, simulator-smoke, and Linux artifact-integrity step before the final retention action found
all 14 expected files and requested `customer-candidate-integrity-9f730499058ba4833bb093dd4635ee50af6fd6ca`
for 30 days. GitHub again rejected artifact creation because organization storage quota usage had not
recalculated. The Customer repository still has zero stored artifacts. No package or image was published or
pushed.

No agent following this ledger may monitor or edit RT3, Stage 4 fleet E2E, Auth, Payment, Search, or another
stream's ledger. This file is the exclusive durable record for Customer; the temporary Customer promotion
ledger was retired after its live evidence was consolidated here.

## Next Steps

Paused: GitHub Actions storage provider — do not rerun while quota state is unchanged; wait for GitHub's documented 6–12-hour recalculation after the administrator's cleanup or storage increase, without deleting caches from this stream. Then rerun only the failed job of exact-head run `33448642947` at `5555ac82b314384685a7a003fa5bc82e18fa8298` and require a nonexpired 30-day `customer-candidate-integrity-9f730499058ba4833bb093dd4635ee50af6fd6ca` artifact containing `SHA256SUMS`, seven CycloneDX SBOMs, three High/Critical vulnerability reports, and three all-severity secret reports. Only after that retained artifact is inspected and the exact gate is green may Customer advance. Do not publish or push packages or images.

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
- `5555ac8` adds deterministic integrity evidence for the exact four NuGet and three OCI candidates: one
  `SHA256SUMS`, seven CycloneDX SBOMs, and pinned vulnerability and secret scans, without publication.

## Verification

- Exact-head remote CI run `33448642947`, attempt 2: Frontend job `99686370507` passed in 2m18s. Backend job
  `99686369441` ran 7m35s and passed restore, build, tests, migration snapshots, four-package clean consumer,
  three OCI builds, real simulator smoke, empty-database migration, and Linux integrity inventory; only
  `Retain candidate integrity evidence` failed.
- The failed upload found 14 files, validated the requested artifact name and root path, requested 30-day
  retention, then returned `Artifact storage quota has been hit` with a 6–12-hour recalculation notice.
  Customer's artifact inventory remained empty, so retained-content inspection was not possible.
- Local full gate at `5555ac82b314384685a7a003fa5bc82e18fa8298` passed for exactly four NuGet and
  three OCI candidates, deterministic `SHA256SUMS`, seven CycloneDX SBOMs, three High/Critical vulnerability
  reports, and three all-severity secret reports. Results: zero High/Critical vulnerabilities and zero secrets.
- Earlier standalone proof: 51-project Release build; seven migration snapshots; `npm ci`; shared 3/3;
  web 1/1 and production build; mobile typecheck and Android export — all green.

## Reviews

- Full and incremental extraction review completed through `e21ae9079ca2fdd3a0063a252f05499159d608ff`
  with all findings resolved.
- Repository-preparation review completed through `39ca980f375b5661ed7da114297f45b909915851` with no open findings.
- Independent artifact-gate review through `2ecc33cb533a95b3baa209dcdc259c6e27e81105` has no open findings
  after reconciling the current and final Customer package rosters.
- Independent artifact-integrity review through `5555ac82b314384685a7a003fa5bc82e18fa8298` fixed
  OS-aware path containment and exact artifact-name casing, then found no remaining issues. Draft PR #1 still
  owns the cumulative delivery gate before any merge.

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
- After rerun attempt 2, Customer still stores zero Actions artifacts and one 164,423,819-byte cache. Organization cache usage
  is 12,671,970,938 bytes, concentrated in five approximately 2.5-GB NuGet caches in
  `Concertable/concertable`; this Customer stream must not delete them. Repeating a seven-minute failed-job
  rerun before provider quota recalculation would only repeat the same terminal upload failure.
- Vitest invokes Vite with `command = serve` and `mode = test`; development-only configuration must consider
  both values rather than treating every `serve` configuration load as a live dev server.
- A multi-path fold must include support files outside selected app subtrees: Customer's relocated Vite app
  uses `app/.env.production`, and Expo's unchanged `../assets/*` references require `app/assets/`.
- This ledger has no write ownership over the monorepo RT3 or fleet branches.
