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
`Concertable.Messaging.Contracts` package seam, exposes Payment through protocol-correct HTTP discovery in the
B2B and Customer standalone AppHosts, and makes inventory validation reject blocking runtime edges as well as
test-tier edges. Payment.Hosting retains the HTTP-schemed `https` compatibility endpoint on the container's
HTTP/1-capable port 8080 for REST, webhook, and mobile callers, and adds an HTTP-schemed `grpc` endpoint on the
separate HTTP/2-only port 8081. Payment.Client prefers that h2c endpoint and retains the legacy `https` discovery
fallback for TLS-backed project hosts. The Auth carve gate now includes both Auth-owned source roots, so it proves
Auth.Contracts restores Messaging from the package feed rather than silently omitting the contract project.
Cleartext Payment call credentials are default-deny: the B2B and Customer AppHosts inject the explicit opt-in only
for local run-mode resources, while publish-mode manifests omit it.
The complete candidate is independently reviewed and security-reviewed through `this commit` with an
approved judgment and no open findings.

Existing `auth`, `b2b`, `customer`, `payment`, `search`, `infra`, and `config` repositories retain their
identities. The remaining repository boundaries are `platform-dotnet`, `platform-frontend`, and `system`.
General shared frontend code covers web and mobile; web and mobile are package tiers, not repositories. M4
creates no repository and makes no topology decision.

## Next Steps

- Blocked by: the ordered M1 package releases and the G0 package baseline. M4 may exist remotely only as a
  `[skip ci]` portability checkpoint; it must not be opened as a PR or treated as a delivery candidate before
  those gates authorize the consumer transition.
- Unblock action: after M1 P4 lands, restack this reviewed range onto the exact landed P4 head, publish and verify
  Payment.Client and Payment.Hosting plus the dual-port Payment.Web image, then pin that immutable image digest.
- Resume when: the exact M1 package versions are feed-visible and G0 supplies the accepted package baseline;
  revalidate the restacked M4 head before delivering either B2B or Customer AppHost consumer.

## Completed work

- Reconciled the active ledger against the corrected repository topology and restored the canonical
  dependency-ordered packet table without importing divergent pre-correction topology text.
- Based the isolated M4 branch on exact PR #945 head `4f2681974c914a15e50c6292e724e42900d3d20b`.
- Replaced the Auth.Contracts `ProjectReference` to Messaging.Contracts with a centrally pinned package reference.
- Restored HTTP-schemed Payment discovery in the B2B and Customer hosts while retaining the `https` endpoint name
  for REST/mobile compatibility, added a separately named h2c-only gRPC endpoint, and made Payment.Hosting own the
  container endpoint and listener-environment contract.
- Made Payment.Client prefer the `grpc` discovery key, preserve the legacy `https` fallback for project hosts,
  and fail closed before registering call credentials for cleartext HTTP unless the owning composition explicitly
  opts in through `PaymentClient:AllowInsecureHttp=true`. B2B and Customer set that opt-in only in local run mode;
  their published manifests omit it.
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
- M4R1 targeted AppHost graph verification passes 4/4 B2B tests and 4/4 Customer tests against exact local
  platform version `0.1.0-local.1788721241736`; both suites assert the compatibility endpoint's HTTP scheme.
- M4R2 live Payment transport verification passes 1/1. A real Kestrel host serves HTTP/1.1 REST on one listener
  and generated Payment gRPC over a separate HTTP/2-only listener; the public Payment.Client registration prefers
  the `grpc` key over a deliberately unusable legacy endpoint and delivers the service bearer metadata.
- Local platform version `0.1.0-local.1788730449876` was freshly prepared with all 57 packages, including the
  repaired Payment.Client and Payment.Hosting. Against that feed, all 4 B2B and all 4 Customer `AppHost_` tests
  pass, covering production, Stripe publish, and mobile tunnel graphs; all 13 Payment architecture tests pass.
- M4R3 validation prepared a new exact 57-package set at `0.1.0-local.1788732761225`. Against that feed,
  Payment architecture passes 13/13, B2B architecture passes 13/13, and Customer architecture passes 9/9.
  The focused Payment transport suite passes 2/2: the explicit composition opt-in completes a real generated
  client/server bearer handshake, while the default cleartext path is rejected before client registration.
- Payment.Web builds with zero warnings and zero errors after the split-listener change.
- Evaluated SDK container metadata exposes TCP ports 8080 and 8081 and bakes the matching
  `ASPNETCORE_HTTP_PORTS` and `PaymentTransport__GrpcPort` defaults into the Payment.Web image.
- Windows verification used a temporary short drive mapping because the isolated worktree plus the longest B2B
  project path is 265 characters. Fresh archive carves eliminated the path-length artifact; no source workaround
  or reduced graph was used.
- No local E2E suite was run; E2E remains a remote merge-queue diagnostic gate.

## Reviews

[`reviews/Refactor-RepoSplit-M4-Closure-Repair.md`](../../reviews/Refactor-RepoSplit-M4-Closure-Repair.md) is
complete and approved through `this commit`; native and security review have no open findings.

## Decisions, discoveries, blockers, and deviations

- The Payment container terminates no TLS. Port 8080 remains HTTP/1-capable for REST, webhooks, mobile tunnelling,
  and the `https` compatibility endpoint name. Port 8081 is a distinct HTTP/2-only h2c listener exposed as `grpc`.
  Payment.Client enables insecure-channel call credentials only for an `http` address carrying the explicit
  `PaymentClient:AllowInsecureHttp=true` composition opt-in; otherwise registration fails closed. Aspire does not
  terminate TLS on behalf of either cleartext container target.
- M4 delivery requires a coordinated Payment publication: consumers must not receive the new `grpc`-preferring
  Payment.Client/Hosting packages until a Payment.Web image containing the 8081 listener exists, and B2B/Customer
  must pin that immutable image digest before their standalone AppHosts are delivered.
- Auth.Contracts owns its package pin because it is a separately mapped root in the retained Auth repository.
  Local M1 validation overrides that pin with the exact locally prepared platform version.
- Initial in-worktree B2B/Customer build attempts failed in MSBuild copy targets because the 265-character B2B
  path crossed the Windows path limit. Short-mounted clean archive carves proved the identical candidate; this
  was an execution-environment artifact, not a test assertion or repository-closure failure.
- Package publication, repository creation/import, and G0, C1, F0, or R1 gate execution are outside M4.
