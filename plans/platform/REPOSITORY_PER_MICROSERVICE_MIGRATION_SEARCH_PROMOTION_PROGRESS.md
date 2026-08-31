# Repository-per-microservice migration — Search promotion progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\search-next`
- Branch: `Chore/search-promotion-preparation`
- PR: draft [`Concertable/search-next#1`](https://github.com/Concertable/search-next/pull/1)
- Dependency/package gates: the first slice is locally green at `c7e6f766256ddaa0c6eb3bd4514a65bddbd96b1d`, but exact-head CI is package-access-gated; final checkpoint 12 delivery is ordered and requires explicit authorization
- Last reconciled: **2026-08-31** from local verification and Search PR #1 run `33416246792`

## Current state

Private `Concertable/search-next` is checked out once at the reserved path. This stream owns only
checkpoint-12 Search repository preparation: Search runtime, migrations, Hosting/TestKit, standalone AppHost,
CI, package/image publication setup, seed convergence gates, and repository evidence. It must not edit RT3,
Stage 4, Auth-next, Customer-next, Payment-next, or shared execution ledgers.

The first candidate adds repository-owned `ci-complete`, minimal .NET ignores, and accurate writable-staging
and B2B-owned rating-event documentation. Web, Workers, UnitTests, and IntegrationTests pass locally. Draft
PR #1 exact-head CI reaches the package feed but receives `403 Forbidden`: `search-next` has not been granted
GitHub Actions access to its private package closure. The whole solution remains deliberately unclaimed:
AppHost retains foreign Auth/B2B/AppHost.Shared source and foreign database resources, ArchitectureTests
consume that AppHost, and the inherited E2E Helper retains foreign source. Search.Hosting exists but is
non-packable; no TestKit or migration job/bundle exists.

State: **first slice implemented and reviewed; package-access-gated in remote validation**. This ledger is
the atomic ownership claim for the exact checkout and branch above. Agents not explicitly dispatched to this
ledger treat the stream as owned and must not create a checkout or branch. Search must consume published
Contracts and producer simulator artifacts rather than another data service's runtime source. No canonical
rename, visibility change, canonical publication, production deployment, or monorepo source removal is authorized.

## Next Steps

Blocked: Search PR #1 exact-head `ci-complete` cannot restore the private NuGet closure because the
repo-scoped `GITHUB_TOKEN` receives `403 Forbidden`.
Blocked by: Tommy or a GitHub package administrator with authority over the packages below.
Unblock action: grant repository `Concertable/search-next` (repository id `1351099165`) GitHub Actions read
access to the exact private NuGet closure below, then re-run Search PR #1 `ci-complete`.
Resume when: the repo-scoped `GITHUB_TOKEN` restores successfully and exact-head `ci-complete` is green.

Required package access: `Concertable.B2B.Artist.Contracts`, `Concertable.B2B.Concert.Contracts`,
`Concertable.B2B.Seed.Contracts`, `Concertable.B2B.Venue.Contracts`, `Concertable.Contracts`,
`Concertable.DataAccess.Application`, `Concertable.DataAccess.Infrastructure`, `Concertable.Kernel`,
`Concertable.Messaging.Application`, `Concertable.Messaging.AzureServiceBus`,
`Concertable.Messaging.Contracts`, `Concertable.Messaging.Domain`, `Concertable.Messaging.Infrastructure`,
`Concertable.Seed.Identity`, `Concertable.Seed.Shared`, `Concertable.ServiceDefaults`,
`Concertable.Shared.Api`, `Concertable.Shared.Email.Application`,
`Concertable.Shared.Geocoding.Application`, `Concertable.Shared.Imaging.Application`,
`Concertable.Testing`, and `Concertable.Testing.Integration`.

## Completed work

- Search extraction proof was built and pushed to private `Concertable/search-next`.
- First repository CI/metadata slice committed as `c7e6f766256ddaa0c6eb3bd4514a65bddbd96b1d` and opened as draft Search PR #1.

## Verification

- Local at `c7e6f766256ddaa0c6eb3bd4514a65bddbd96b1d`: Release Web and Workers builds succeeded; UnitTests passed
  21/21; Docker-backed IntegrationTests passed 27/27; `git diff --check` passed.
- Remote exact-head run `33416246792`: workflow parsed and started, then failed in Build Search Web before
  compilation because private NuGet restores returned `403 Forbidden`.

## Reviews

Full and security review completed through `c7e6f766256ddaa0c6eb3bd4514a65bddbd96b1d`; approved with no findings.
Canonical local work order: `C:\Users\tommy\source\repos\search-next\reviews\Chore-search-promotion-preparation.md`.

## Decisions, discoveries, blockers, and deviations

- Search is a data service: its current projection inputs, including rating updates, are B2B-owned events; it consumes B2B simulator artifacts, never B2B/Customer runtime source or databases.
- Delivery ordering does not prevent repository-local preparation against exact current artifacts.
- Do not copy the personal `read:packages` PAT into repository Actions secrets. The available package token
  has only `read:packages` and cannot administer package ACLs; use repository-scoped package Actions access.
