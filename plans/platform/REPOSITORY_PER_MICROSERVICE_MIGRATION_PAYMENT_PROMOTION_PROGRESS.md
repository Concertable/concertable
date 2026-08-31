# Repository-per-microservice migration — Payment promotion progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\payment-next`
- Branch: `Chore/payment-promotion-preparation`
- PR: draft [`Concertable/payment#1`](https://github.com/Concertable/payment/pull/1), exact head `38633969d85d62f04521f7423e0a93b7ce2e51e8`
- Dependency/package gates: the exact private package closure now restores; remote validation is blocked only by the organization-level GitHub Actions artifact-storage quota; final checkpoint 11 delivery is ordered and requires explicit authorization
- Last reconciled: **2026-08-31** from Payment commits, reviews, PR, and Actions run `33427604910`

## Current state

Private `Concertable/payment` exists with its extraction proof and draft PR #1. This reserved stream owns
only checkpoint-11 Payment repository preparation: Web, Workers, Contracts, Client, migrations, Stripe
tooling, images, Hosting/TestKit, AppHost, CI, publication setup, and repository evidence. It must not edit
RT3, Stage 4, Auth-next, Customer-next, Search-next, or shared execution ledgers.

Before this slice the target had no `.github` workflows. Web, Workers, UnitTests, IntegrationTests, Contracts,
Client, and Hosting are package-clean; the whole solution is not. AppHost/ArchitectureTests retain foreign
Auth/B2B/AppHost.Shared source and database composition, and E2E Helpers retain foreign test source. The
repository-owned metadata now identifies the standalone Payment repository.

The first repository-owned CI/metadata slice and the causal standalone provider-inventory fix are committed
and independently reviewed. Package ACL is fully unblocked. Exact-head CI restores, builds Web and Workers,
runs all unit tests, packs the Payment-owned packages, and passes the Docker-backed integration suite. The only
failure is `actions/upload-artifact`: the organization artifact-storage quota is exhausted after package creation.

State: **implementation complete; validation blocked on organization artifact storage; delivery-gated**. This dedicated ledger is
the atomic ownership claim for the exact checkout and branch above. Agents not explicitly dispatched to this
ledger treat the stream as owned and must not create a checkout or branch. No canonical rename, visibility change,
canonical publication, production deployment, or monorepo source removal is authorized.

## Next Steps

Blocked: Payment PR #1 exact-head CI cannot upload the already-created package candidates because the organization GitHub Actions artifact-storage quota is exhausted.
Blocked by: a GitHub organization billing/actions administrator or the normal 6–12-hour storage-usage recalculation after quota is freed.
Unblock action: free or increase GitHub Actions artifact storage without weakening the Payment workflow.
Resume when: rerun Actions run `33427604910` on unchanged exact head `38633969d85d62f04521f7423e0a93b7ce2e51e8`; require restore, Web/Workers builds, all unit tests, package creation and upload, Docker-backed integration, and `ci-complete` to pass before delivery review.

## Completed work

- Payment extraction mechanism was proven and pushed to private `Concertable/payment-next`.
- Repository metadata/guidance and first CI are committed at `157cbb7891c85595b34f77f1551a19d3a481fc1d`; the
  workflow Release-builds Web/Workers, runs UnitTests and IntegrationTests, and packs Contracts, Client, and
  Payment.Hosting without publishing them.
- GitHub Packages Actions read access is proven for the full private closure, including
  `Concertable.Payment.Contracts` and `Concertable.Payment.Client`.
- `ProviderContractInventoryTests` now resolves both monorepo and standalone Payment layouts, maps standalone
  `src/` paths to the canonical inventory namespace, requires the local Payment surface, scans only physically
  available repository roots, and retains completeness validation for every cross-repository decision. The fix is
  committed at `38633969d85d62f04521f7423e0a93b7ce2e51e8`.

## Verification

- Local `dotnet build src/Concertable.Payment.Web/Concertable.Payment.Web.csproj --configuration Release`:
  zero errors, with 523 pre-existing analyzer warnings.
- Local focused `dotnet test tests/Concertable.Payment.UnitTests/Concertable.Payment.UnitTests.csproj
  --configuration Release --no-restore --filter FullyQualifiedName~ProviderContractInventoryTests`: 38 passed,
  zero failed (run twice after the final assertion).
- PR CI run [`33415887059`, attempt 3](https://github.com/Concertable/payment/actions/runs/33415887059/attempts/3):
  private restore, builds, and Docker-backed integration passed; 9 provider-inventory test cases failed from the
  monorepo-only root locator, providing the causal defect reproduced by the focused fix.
- Exact-head PR CI run [`33427604910`](https://github.com/Concertable/payment/actions/runs/33427604910): restore,
  Web/Workers builds, all unit tests, package creation, and Docker-backed integration passed. Package candidate
  upload failed only with `Failed to CreateArtifact: Artifact storage quota has been hit`; `ci-complete` therefore
  correctly failed.

## Reviews

Independent focused review of `e4da6e23f79bed9105e4a82f828c0608feee68a5..157cbb7891c85595b34f77f1551a19d3a481fc1d`
completed clean with no actionable findings.

Independent focused review of `157cbb7891c85595b34f77f1551a19d3a481fc1d..38633969d85d62f04521f7423e0a93b7ce2e51e8`
completed clean with no actionable findings.

## Decisions, discoveries, blockers, and deviations

- Payment is an adapter service and owns the only live internal gRPC surface plus its Stripe HTTP webhook.
- Delivery ordering does not prevent repository-local preparation against exact current artifacts.
- The package ACL is proven through the repository `GITHUB_TOKEN`; no personal credential was transferred and no
  repository secret workaround was introduced.
- The proven least-privilege closure is: `Concertable.AppHost.Shared`, `Concertable.Auth.Contracts`,
  `Concertable.B2B.Concert.Contracts`, `Concertable.B2B.Tenant.Contracts`, `Concertable.Contracts`,
  `Concertable.DataAccess.Application`, `Concertable.DataAccess.Infrastructure`, `Concertable.Grpc`,
  `Concertable.Kernel`, all five `Concertable.Messaging.*` packages, `Concertable.Seed.Identity`,
  `Concertable.Seed.Shared`, `Concertable.ServiceDefaults`, `Concertable.Shared.Api`, the Email/Geocoding/
  Imaging shared Application packages, `Concertable.Testing`, and `Concertable.Testing.Integration`.
