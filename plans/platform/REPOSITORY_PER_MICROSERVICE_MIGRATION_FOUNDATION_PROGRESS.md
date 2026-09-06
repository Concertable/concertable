# Repository-per-microservice foundation progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Active packet: M4, monorepo closure repair
- Worktree: `C:\Users\tommy\source\repos\Concertable\.worktrees\Refactor-RepoSplit-M4-Closure-Repair`
- Branch: `Refactor/RepoSplit-M4-Closure-Repair`
- PR: not opened; local preparation only
- Base: exact M1 P4 candidate and PR #945 head
  `4f2681974c914a15e50c6292e724e42900d3d20b` (`Refactor/M1-Platform-Contract`)
- Dependency/package gates: the M1 package/API shape is present locally. M4 publication or delivery remains
  gated on the ordered M1 package releases and the G0 package baseline; this packet does not publish packages.
- Last reconciled: 2026-09-06 — canonical M4 packet metadata, corrective topology commits `82bf5dbbb`
  and `bb59d9ba3`, and live PR #945 head `4f2681974c914a15e50c6292e724e42900d3d20b`.

## Current state

Checkpoint 6A is terminal. M1 P4 provides the exact local platform package/API boundary required to prepare M4.
The M4 candidate replaces the final Auth.Contracts-to-Messaging cross-repository runtime source edge with the
`Concertable.Messaging.Contracts` package seam, exposes Payment through an HTTPS proxy endpoint in the B2B and
Customer standalone AppHosts, and makes inventory validation reject blocking runtime edges as well as test-tier
edges. The Auth carve gate now includes both Auth-owned source roots, so it proves Auth.Contracts restores
Messaging from the package feed rather than silently omitting the contract project.

Existing `auth`, `b2b`, `customer`, `payment`, `search`, `infra`, and `config` repositories retain their
identities. The remaining repository boundaries are `platform-dotnet`, `platform-frontend`, and `system`.
General shared frontend code covers web and mobile; web and mobile are package tiers, not repositories. M4
creates no repository and makes no topology decision.

## Next Steps

- Hand the immutable M4 candidate to the owning workflow for independent review and eventual stacking on the
  landed M1 P4 commit. Repair any finding in M4 rather than in an M1 stage.
- Keep the candidate local. Do not publish, push, or open an M4 PR until the M1 publication sequence and G0
  package baseline authorize the consumer transition.

## Completed work

- Reconciled the active ledger against the corrected repository topology and restored the canonical
  dependency-ordered packet table without importing divergent pre-correction topology text.
- Based the isolated M4 branch on exact PR #945 head `4f2681974c914a15e50c6292e724e42900d3d20b`.
- Replaced the Auth.Contracts `ProjectReference` to Messaging.Contracts with a centrally pinned package reference.
- Corrected the B2B and Customer Payment resource endpoints to terminate HTTPS at the Aspire proxy while keeping
  container target port 8080, and updated the owner host-graph assertions.
- Extended the split-inventory check to fail for blocking runtime edges and regenerated the inventory.
- Extended the Auth carve workflow to include and build the Auth.Contracts owner root.

## Verification

- `scripts/local-platform.ps1 prepare` produced exact local version `0.1.0-local.1788721241736` with 57 packages,
  including the modified Auth.Contracts package and its Messaging.Contracts dependency.
- `eng/repository-split/inventory.py --check` passes with zero blocking runtime edges and zero blocking test-tier
  edges.
- `eng/repository-split/validate_map.py` reports 4,766 tracked paths, 4,766 claims, 79 unclaimed paths, and zero
  multiply claimed paths. The 79 unclaimed paths remain the pre-existing F0 map-admission work; M4 does not cross
  that gate.
- The Auth clean carve builds both Auth-owned roots plus the runtime/unit/integration closure with zero errors.
  Its restored asset graph resolves `Concertable.Messaging.Contracts/0.1.0-local.1788721241736` as a package and
  contains no project reference.
- The B2B clean carve builds its 104-project package-only solution with zero errors, and all 13 B2B standalone
  host-graph tests pass against the same exact local M1 package set.
- The Customer clean carve builds its 54-project package-only solution with zero errors, and all 9 Customer
  standalone architecture tests pass against the same exact local M1 package set.
- Windows verification used a temporary short drive mapping because the isolated worktree plus the longest B2B
  project path is 265 characters. Fresh archive carves eliminated the path-length artifact; no source workaround
  or reduced graph was used.
- No local E2E suite was run; E2E remains a remote merge-queue diagnostic gate.

## Reviews

The base-to-tip implementation review of the immutable local candidate found no code or plan findings after the
clean-carve and standalone-composition evidence was captured. The owning workflow should perform its independent
review after handoff; any resulting finding remains M4-owned.

## Decisions, discoveries, blockers, and deviations

- The Payment container continues to listen on target port 8080. `WithHttpsEndpoint` changes the Aspire proxy
  discovery scheme to HTTPS; it does not require TLS inside the Payment container.
- Auth.Contracts owns its package pin because it is a separately mapped root in the retained Auth repository.
  Local M1 validation overrides that pin with the exact locally prepared platform version.
- Initial in-worktree B2B/Customer build attempts failed in MSBuild copy targets because the 265-character B2B
  path crossed the Windows path limit. Short-mounted clean archive carves proved the identical candidate; this
  was an execution-environment artifact, not a test assertion or repository-closure failure.
- Package publication, repository creation/import, and G0, C1, F0, or R1 gate execution are outside M4.
