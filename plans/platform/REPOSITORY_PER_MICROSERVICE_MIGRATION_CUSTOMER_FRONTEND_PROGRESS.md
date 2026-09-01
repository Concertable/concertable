# Repository-per-microservice migration — Customer progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\customer`
- Branch: `Chore/customer-promotion-preparation`
- PR: draft [`Concertable/customer#1`](https://github.com/Concertable/customer/pull/1), exact head
  `08ddbd812fd037544be47da2530098c49b278e86`
- Dependency/package gates: package access, artifact retention, TestKit, CODEOWNERS, immutable action refs,
  and repository SHA enforcement are green; final publication and delivery remain unauthorized
- Last reconciled: **2026-09-02** from reviewed Customer head
  `08ddbd812fd037544be47da2530098c49b278e86`, successful CI run `33570252885`, and Actions-permissions
  readback

## Current state

State: **repository preparation active; inert promotion preflight green**. GitHub repository `Concertable/customer-next`
was renamed in place to canonical `Concertable/customer`; repository ID `1351337130`, PR #1, branches, and
history were preserved. The inactive local checkout moved from `customer-next` to `customer`, and its origin
now uses `https://github.com/Concertable/customer.git`.

The extraction proof at `e21ae9079ca2fdd3a0063a252f05499159d608ff` contains the Customer backend,
web, mobile, customer-only shared package, and standalone support closure. Draft PR #1 validates the owned
build, tests, migration snapshots, current package candidates, and Customer Web and migrations OCI image
candidates. Package-level Actions read access is granted for the exact NuGet and npm closures recorded below.
Exact-head CI run [`33448642947`](https://github.com/Concertable/customer/actions/runs/33448642947) ran at
`5555ac82b314384685a7a003fa5bc82e18fa8298`. Failed-job rerun attempt 3 restarted Backend job
`100007086492`; Frontend job `100007089026` remained green. Backend passed every build, test, package, image,
migration, simulator-smoke, Linux artifact-integrity, and retention step. Artifact `9818452253`, named
`customer-candidate-integrity-9f730499058ba4833bb093dd4635ee50af6fd6ca`, is retained through
2026-10-01 with digest `sha256:edb44e3c5334c2b2a2e4ab1ad775270bf84198d3be834c4b5c842deadcf2989b`.
Its downloaded contents contain exactly the required 14 evidence files. No package or image was published or
pushed.

At exact head `c83169dd2a3d172d765425b12e032e704fcdc4fa`, Customer gained a machine-readable
promotion manifest for exactly four NuGet and three OCI candidates. CI validates actual NuGet metadata and
each Docker archive's embedded repository, selected SHA tag, and config-digest shape. Manual dispatch remains
read-only and requires an existing annotated v-prefixed tag that resolves to the exact selected commit; its
NuGet versions and OCI tags must match that release tag. The workflow contains no package/image publish or
push operation and has only `contents: read` and `packages: read` permissions.

At exact head `08ddbd812fd037544be47da2530098c49b278e86`, the manifest and every package and
integrity gate now cover five NuGet candidates, adding the black-box `Concertable.Customer.TestKit`.
The package exposes an injected-`HttpClient` client and Customer-owned ticket purchase/upcoming-ticket wire
models only; it has no runtime implementation, DbContext, entity, Hosting, or foreign-service reference.
Its focused contract tests, clean-consumer closure, and the complete Customer CI gate are green without any
package or image publication.

Customer PR #1 carries repository-wide bootstrap ownership for `@tomjseery` and immutable SHAs for all five
action invocations. Exact-head CI run [`33570252885`](https://github.com/Concertable/customer/actions/runs/33570252885)
is green, and the repository Actions policy reads back `sha_pinning_required: true` while preserving
`allowed_actions: all`, default read-only workflow permissions, and disabled PR approvals.

No agent following this ledger may monitor or edit RT3, Stage 4 fleet E2E, Auth, Payment, Search, or another
stream's ledger. This file is the exclusive durable record for Customer; the temporary Customer promotion
ledger was retired after its live evidence was consolidated here.

## Next Steps

No further Customer-only preparation slice is independently implementable. Keep Customer PR #1 draft and
hold final 13A–13E delivery until the owning plan's canonical platform/system baselines and preceding service
cutovers are green. Resume standalone AppHost work only when its named foreign container-hosting inputs are
available; resume rules/main-protection work only when the private-plan capability changes. Do not publish or
push candidates, create tags, change visibility, retry or bypass the protection `403`, or enter another stream.

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
- `4d1a1ef` adds the exact promotion manifest, repository metadata validator, and manual annotated-tag gate;
  `c83169d` binds every built OCI archive to its configured repository and selected SHA/release tag. The
  promotion path remains a read-only preflight with no publication command or permission.
- `0702477` adds repository-wide bootstrap `CODEOWNERS` for `@tomjseery` and pins every Customer CI action to the verified immutable commit behind its recorded `v4` channel.
- `01bc246` adds the black-box `Concertable.Customer.TestKit`, focused HTTP contract tests, and the fifth
  NuGet promotion candidate; `08ddbd8` aligns the integrity-evidence gate with eight package/image SBOMs and
  six Trivy reports.

## Verification

- Exact-head remote CI run `33448642947`, attempt 3: Frontend job `100007089026` passed in 2m18s. Backend job
  `100007086492` ran 6m55s and passed restore, build, tests, migration snapshots, four-package clean consumer,
  three OCI builds, real simulator smoke, empty-database migration, Linux integrity inventory, and evidence
  retention.
- The retained 30-day artifact has exactly `SHA256SUMS`, seven CycloneDX 1.7 SBOMs, three High/Critical
  vulnerability reports, and three all-severity secret reports. All JSON parsed; the vulnerability and secret
  reports contain zero findings; `SHA256SUMS` contains seven unique, valid SHA-256 entries covering the four
  NuGet and three OCI candidates.
- Local full gate at `5555ac82b314384685a7a003fa5bc82e18fa8298` passed for exactly four NuGet and
  three OCI candidates, deterministic `SHA256SUMS`, seven CycloneDX SBOMs, three High/Critical vulnerability
  reports, and three all-severity secret reports. Results: zero High/Critical vulnerabilities and zero secrets.
- Earlier standalone proof: 51-project Release build; seven migration snapshots; `npm ci`; shared 3/3;
  web 1/1 and production build; mobile typecheck and Android export — all green.
- Exact-head CI run `33566459131` at `070247795927ec6045b138c3225fbed99e5a2eb5`: Frontend job `100050657740` passed in 2m19s and Backend job `100050657995` passed in 7m23s, including the complete package/image/migration/simulator/integrity/retention gate.
- Exact-head CI run `33570252885` at `08ddbd812fd037544be47da2530098c49b278e86`:
  Frontend job `100062440903` passed in 2m25s and Backend job `100062440740` passed in 7m24s, including
  TestKit's focused tests, five-package packing and clean-consumer restore/build, all three OCI candidates,
  migration/simulator gates, eight SBOMs, six Trivy reports, and retained integrity evidence.
- Retained artifact `9824844924`, `customer-candidate-integrity-39a769bc914f1ffbf4854474f02cd91010fe1095`,
  expires 2026-10-01 and has digest
  `sha256:824d572595749d3141f94ff9792ca5489a0f39e96a4416c6fdf3fa6ab8e2bf10`.
- Local TestKit verification passed 3/3 focused contract tests, the complete Release solution build,
  promotion selection for five NuGet and three OCI candidates, and a temporary five-package clean-consumer
  restore/build with zero errors; no candidate was published.
- Actions policy readback: `enabled: true`, `allowed_actions: all`, `sha_pinning_required: true`; default workflow permissions remain `read` and PR approvals remain disabled.
- Local promotion validation passed against the existing four NuGet and three OCI outputs. A temporary local
  annotated `v0.1.0-alpha.0.329` tag then built all three OCI candidates and proved exact tag-to-commit,
  NuGet-version, embedded repository/tag, and config-digest validation; the tag and dedicated outputs were
  removed afterward.

## Reviews

- Full and incremental extraction review completed through `e21ae9079ca2fdd3a0063a252f05499159d608ff`
  with all findings resolved.
- Repository-preparation review completed through `39ca980f375b5661ed7da114297f45b909915851` with no open findings.
- Independent artifact-gate review through `2ecc33cb533a95b3baa209dcdc259c6e27e81105` has no open findings
  after reconciling the current and final Customer package rosters.
- Independent artifact-integrity review through `5555ac82b314384685a7a003fa5bc82e18fa8298` fixed
  OS-aware path containment and exact artifact-name casing, then found no remaining issues.
- Independent repository-policy review approved
  `c83169dd2a3d172d765425b12e032e704fcdc4fa..070247795927ec6045b138c3225fbed99e5a2eb5`
  with no findings after verifying CODEOWNERS precedence, official signed action commits, exact `v4` ref
  equality, immutable-reference closure, and read-only permissions. Draft PR #1 still owns the cumulative
  delivery gate before any merge.
- Independent TestKit review approved
  `070247795927ec6045b138c3225fbed99e5a2eb5..08ddbd812fd037544be47da2530098c49b278e86`
  after finding and correcting the stale 13-file/seven-SBOM integrity count; no findings remain. Draft PR #1
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
  `Concertable.Customer.Seed.Contracts`, `Concertable.Customer.TestKit`, and
  `Concertable.Customer.Ticket.Contracts`. The Customer-owned candidate roster is complete. Ticket Contracts
  are intentional because Hosting directly uses
  `TicketPurchasedEvent` and `SendTicketEmailCommand`.
- Exact-head CI now creates local Customer Web, `customer-migrations`, and `customer-seed-simulator` archives.
  The simulator smoke uses `docker load` and `docker run --rm` against the built archive; it is not a source-only
  substitute. Runtime `MigrateAsync` remains as a temporary fallback until the standalone AppHost invokes the
  migration resource. No package, image, canonical release, visibility change, deployment, or system-consumer
  update was authorized or performed.
- The organization quota recalculated without Customer deleting another stream's caches. Failed-job rerun
  attempt 3 created the required retained artifact, so the quota blocker is closed.
- Customer now has repository-wide bootstrap `CODEOWNERS` for `@tomjseery`; all workflow actions are pinned to verified immutable SHAs, and repository Actions requires SHA pinning. GitHub still returns the private-plan `Upgrade to GitHub Pro or make this repository public` `403` for both repository rulesets and `main` branch protection. Do not bypass or retry that delivery-time capability gate.
- The extracted `Concertable.Customer.AppHost` remains excluded from `CarveCustomer.slnx` and has ten foreign
  monorepo `ProjectReference`s. Invoking the Customer migration resource there and removing runtime
  `MigrateAsync` is not independently buildable or validatable until its foreign container-hosting inputs are
  available. Do not fake that gate or widen this stream into RT3, Stage 4, Auth, Payment, Search, or B2B.
- Vitest invokes Vite with `command = serve` and `mode = test`; development-only configuration must consider
  both values rather than treating every `serve` configuration load as a live dev server.
- A multi-path fold must include support files outside selected app subtrees: Customer's relocated Vite app
  uses `app/.env.production`, and Expo's unchanged `../assets/*` references require `app/assets/`.
- This ledger has no write ownership over the monorepo RT3 or fleet branches.
