# Repository-per-microservice migration — Customer promotion progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\customer-next`
- Branch: `Chore/customer-promotion-preparation`
- PR: [customer-next draft PR #1](https://github.com/Concertable/customer-next/pull/1), exact reviewed head `39ca980f375b5661ed7da114297f45b909915851`
- Dependency/package gates: package-level Actions read access is blocked; final checkpoint 13 delivery remains gated on canonical platform/system baselines, preceding service cutovers, and explicit authorization
- Last reconciled: **2026-08-31** from repository ID `1351337130`, PR #1, Actions run `33418244492`, and local validation at `39ca980f375b5661ed7da114297f45b909915851`

## Current state

State: **remote-validation blocked, delivery-gated**. The existing private checkout owns the Customer stream and its
repository-local preparation branch contains the reviewed CI slice. Draft PR #1 is not code-red: Actions
run [33418244492](https://github.com/Concertable/customer-next/actions/runs/33418244492) reaches only private
package restore, where NuGet and npm return HTTP 403; backend and frontend build/test steps never start.

Local validation is green at the exact reviewed head: the 51-project Release build, all seven migration
snapshot checks, `npm ci`, shared tests (3/3), web tests (1/1) and build, and mobile typecheck plus Android
export passed. Standalone AppHost completion remains a later slice gated on sibling Hosting packages and
published image artifacts; this checkpoint does not claim AppHost validation.

No rename, visibility change, canonical publication, live migration, production deployment, or monorepo
source removal is authorized. Publication/images, Hosting/TestKit, Review/Seed Contracts, the Customer
simulator, and repository-settings evidence remain later repository-local slices.

## Next Steps

Blocked: [customer-next draft PR #1](https://github.com/Concertable/customer-next/pull/1) at exact reviewed head `39ca980f375b5661ed7da114297f45b909915851` cannot restore its private NuGet and npm closures because GitHub Packages returns HTTP 403.
Blocked by: GitHub Packages administrator.
Unblock action: grant the `customer-next` repository's GitHub Actions identity read access to every package in the exact NuGet and npm closures recorded below.
Resume when: rerun PR #1 at exact head `39ca980f375b5661ed7da114297f45b909915851` and drive both CI jobs green; then continue the next repository-local preparation slice without claiming the later-gated AppHost.

## Completed work

- The reviewed private extraction proof remains at `e21ae9079ca2fdd3a0063a252f05499159d608ff`; see `REPOSITORY_PER_MICROSERVICE_MIGRATION_CUSTOMER_FRONTEND_PROGRESS.md`.
- Customer-owned CI and seven-context migration validation are committed and independently reviewed at `39ca980f375b5661ed7da114297f45b909915851` in draft PR #1.

## Verification

- Local, exact head: 51-project Release build; seven migration snapshots; `npm ci`; shared 3/3; web 1/1 and build; mobile typecheck and Android export — all green.
- Remote, exact head: Actions run `33418244492` fails only at NuGet restore and npm clean install with HTTP 403; subsequent build, test, mobile, and migration steps are skipped.

## Reviews

Full and incremental review are complete with no findings through `39ca980f375b5661ed7da114297f45b909915851`; the canonical Customer work order records matching code and security watermarks.

## Decisions, discoveries, blockers, and deviations

- Repository ID: `1351337130` (`Concertable/customer-next`). Reuse the existing private checkout; do not create a duplicate clone or rewrite private `main`.
- Exact NuGet package ACL closure:
  - `Concertable.Auth.Contracts`
  - `Concertable.B2B.Artist.Contracts`, `Concertable.B2B.Concert.Contracts`, `Concertable.B2B.Seed.Contracts`, `Concertable.B2B.Tenant.Contracts`, `Concertable.B2B.User.Contracts`, `Concertable.B2B.Venue.Contracts`
  - `Concertable.Contracts`
  - `Concertable.DataAccess.Application`, `Concertable.DataAccess.Infrastructure`
  - `Concertable.Grpc`
  - `Concertable.Kernel`
  - `Concertable.Messaging.Application`, `Concertable.Messaging.AzureServiceBus`, `Concertable.Messaging.Contracts`, `Concertable.Messaging.Domain`, `Concertable.Messaging.Infrastructure`
  - `Concertable.Payment.Client`, `Concertable.Payment.Contracts`
  - `Concertable.Seed.Identity`, `Concertable.Seed.Shared`
  - `Concertable.ServiceDefaults`
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
- The AppHost gate is separate from the package-restore blocker: it requires sibling Hosting packages and image artifacts that do not yet exist on the canonical published baseline.
