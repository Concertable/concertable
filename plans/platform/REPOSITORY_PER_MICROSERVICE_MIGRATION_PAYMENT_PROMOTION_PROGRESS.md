# Repository-per-microservice migration — Payment promotion progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\payment-next`
- Branch: `main` at `2925ee6d69fa8a0cf490021bbf546f59d08f0641`
- Delivery: [`Concertable/payment#1`](https://github.com/Concertable/payment/pull/1), [`#2`](https://github.com/Concertable/payment/pull/2), and [`#3`](https://github.com/Concertable/payment/pull/3) are merged
- Dependency/package gates: repository CI has package access; AppHost and E2E-helper standalone closure remains blocked on published Hosting/TestKit packages and pinned service images; final checkpoint 11 canonical delivery still requires explicit authorization
- Last reconciled: **2026-09-01** from merged Payment commits, reviews, and Actions runs through `33448718062`

## Current state

Private `Concertable/payment` exists with its extraction proof and three merged preparation slices. This
reserved stream owns only checkpoint-11 Payment repository preparation: Web, Workers, Contracts, Client,
migrations, Stripe tooling, images, Hosting/TestKit, AppHost, CI, publication setup, and repository evidence.
It must not edit RT3, Stage 4, Auth, Customer, Search, B2B, or shared execution ledgers.

The repository-local solution now restores and builds on both Windows and Linux. Repository CI builds that
solution, runs all unit tests, runs Web/Workers architecture and composition tests, packs the three
Payment-owned package candidates without publishing them, and runs the Docker-backed integration suite.
The lean `.claude/settings.json` plugin opt-in is the only repository bootstrap for agent standards; redundant
vendored hooks and copied helper scripts were removed.

AppHost and the two E2E helper projects remain outside the default solution and repository CI because they
still hold cross-repository Auth/B2B/AppHost.Shared/TestKit source references. AppHost carries the required
explicit temporary `CompositionValidationExclusion`; the monorepo composition suite remains its substitute
until published Hosting/TestKit packages and pinned service images make the graph repository-local.

State: **repository-local preparation advanced through merged PR #3; dependency-blocked at the AppHost/E2E
package-image seam; canonical delivery-gated**. No canonical rename, package/image publication, production
deployment, repository visibility change, or monorepo source removal is authorized.

## Next Steps

Blocked: Payment AppHost and `Concertable.Payment.E2ETests.Helpers*` cannot restore or validate standalone while they retain Auth, B2B, AppHost.Shared, and `Concertable.Testing.E2E` source references.
Blocked by: the earlier shared/Auth/B2B promotion checkpoints that must provide versioned Hosting/TestKit packages and pinned service images.
Unblock action: make the approved package/image artifacts available, then replace only Payment's remaining cross-repository source references with their pinned package/image seams.
Resume when: restore and build AppHost and E2E helpers standalone, return them to the default solution and repository CI, remove the temporary AppHost `CompositionValidationExclusion`, run the complete Payment validation matrix, and continue checkpoint-11 delivery only with its required authorization.

## Completed work

- Payment extraction was proven and promoted to private `Concertable/payment`.
- PR #1 merged the repository-owned CI and metadata preparation at merge commit
  `3683990262f792cc56b510238b8807102c0debc1`; exact-head run [`33444052725`](https://github.com/Concertable/payment/actions/runs/33444052725)
  passed after package access was granted and package candidates stopped being uploaded into the exhausted
  organization Actions artifact quota.
- PR #2 merged at `a5202705468753318ff0136b6a06cd18c0fe1a8d`. It made ArchitectureTests repository-local,
  added Web/Workers composition validation to CI, recorded the temporary AppHost validation exclusion, fixed
  canonical repository URLs, and removed redundant copied agent-standard hooks/scripts. Exact-head run
  [`33446812588`, attempt 2](https://github.com/Concertable/payment/actions/runs/33446812588/attempts/2) passed.
- PR #3 merged at `2925ee6d69fa8a0cf490021bbf546f59d08f0641`. It removed only the blocked AppHost/E2E-helper
  projects from the default solution and made CI restore/build that exact repository-local solution. Exact-head
  run [`33448718062`](https://github.com/Concertable/payment/actions/runs/33448718062) passed.

## Verification

- Local `dotnet restore Concertable.Payment.slnx`: success with no skipped projects.
- Local Release `dotnet build Concertable.Payment.slnx --no-restore`: zero errors.
- Local UnitTests: 543 passed, zero failed.
- Local ArchitectureTests: 8 passed, zero failed.
- PR #2 exact-head CI: solution closures, 543 unit tests, 8 architecture/composition tests, three package
  candidates, Docker-backed integration, and aggregate gate passed.
- PR #3 exact-head CI: repository-local solution restore/build, unit tests, architecture/composition tests,
  package candidates, Docker-backed integration, and aggregate gate passed.

## Reviews

- PR #1 preparation and its provider-inventory repair were independently reviewed clean with no actionable findings.
- PR #2 exact head `61001089941a7f2565fc5bc2291a8012832e2fea` was reviewed clean before remote validation and merge.
- PR #3 exact range `a5202705468753318ff0136b6a06cd18c0fe1a8d..2dc8f7fd47235af934da32300ea8c1c0fce42ea2` completed the canonical review workflow with no findings and an approved judgment before merge.

## Decisions, discoveries, blockers, and deviations

- Payment remains an agnostic adapter service and owns the live internal gRPC surface plus Stripe HTTP webhook.
- Repository `GITHUB_TOKEN` package access is proven, including `Concertable.Testing.Architecture`; no personal
  credential was transferred and no repository-secret workaround was introduced.
- Package candidates are built and validated but intentionally neither uploaded as workflow artifacts nor
  published canonically during preparation.
- Architecture coverage is now repository-local. AppHost production-graph validation remains explicitly and
  temporarily excluded rather than silently omitted.
- Default-solution completeness means the repository-local service/test closure: E2E Web, Workers, and Stripe
  adapter projects remain included; only AppHost and the two externally coupled E2E helper projects are deferred.
