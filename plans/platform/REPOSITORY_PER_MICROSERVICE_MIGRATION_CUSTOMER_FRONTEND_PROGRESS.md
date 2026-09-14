# Repository-per-microservice migration — Customer progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: `C:\Users\tommy\source\repos\customer`
- Branch: `Chore/customer-promotion-preparation`
- PR: [`Concertable/customer#1`](https://github.com/Concertable/customer/pull/1) merged normally from exact
  source head `79cb07d6dab684a75cba60012374ac76c41c4b0c` as `6eb1226958732f29cc5fecb866461faf594e0e67`
- Dependency/package gates, including the restored service-local ArchitectureTests suite, are green.
- Last reconciled: **2026-09-04** from Customer PR #3 head
  `d94cbb9a4f21ee9c81d6069f682a08e765257452`, merge commit
  `5f34731785f786ad9cf6864ddae59fef2fac6337`, exact-head CI run `33879657287`, and post-merge CI
  run `33880491018`.

## Current state

State: **repository preparation and ArchitectureTests restoration merged; inert promotion preflight green**. GitHub repository `Concertable/customer-next`
was renamed in place to canonical `Concertable/customer`; repository ID `1351337130`, PR #1, branches, and
history were preserved. The inactive local checkout moved from `customer-next` to `customer`, and its origin
now uses `https://github.com/Concertable/customer.git`.

The extraction proof at `e21ae9079ca2fdd3a0063a252f05499159d608ff` contains the Customer backend,
web, mobile, customer-only shared package, and standalone support closure. Customer PR #1 validates the owned
build, tests, migration snapshots, package candidates, and Customer Web, migrations, and seed-simulator OCI
candidates. Package-level Actions read access now includes `Concertable.Testing.Architecture`. The Customer
workflow did not publish or push a package or image.

Reviewed source head `79cb07d6dab684a75cba60012374ac76c41c4b0c` contains the five-package/three-image
read-only promotion preflight and Customer-only TestKit. `Db.Name` owns service-local connection lookups;
the AppHost-facing `CustomerConstants.Database` remains the composition alias, and all seven modules retain
their `Schema` constants. `Concertable.Customer.slnx` is canonical, carve/`UseLocalCore` residue is removed,
the packaged TestKit README and compile-use consumer gate are green, checkout credentials are disabled after
fetch, and standalone guidance now reflects the extracted .NET 10 repository.

Customer PR #1 merged normally as `6eb1226958732f29cc5fecb866461faf594e0e67`. It carries repository-wide
bootstrap ownership for `@tomjseery` and immutable SHAs for all five action invocations. Exact post-merge CI
run [`33646229860`](https://github.com/Concertable/customer/actions/runs/33646229860) is green. The repository
Actions policy reads back `sha_pinning_required: true`; default workflow permissions remain `read` and
`can_approve_pull_request_reviews` remains `false`.

Merging required Concertable ledger-only PR #922 as `ac74fdf9a0687a436872a7c1c4da622126e7885b`
automatically ran [packages workflow `33644847202`](https://github.com/Concertable/concertable/actions/runs/33644847202)
and [images workflow `33644847172`](https://github.com/Concertable/concertable/actions/runs/33644847172).
Both succeeded, including their package-push and image-push steps, contrary to this stream's no-publication
constraint. Tommy explicitly authorized the remaining Customer closeout delivery on 2026-09-04 after this
consequence was disclosed; this ledger records the violation rather than treating it as a Customer publication.

No agent following this ledger may monitor or edit RT3, Stage 4 fleet E2E, Auth, Payment, Search, or another
stream's ledger. This file is the exclusive durable record for Customer; the temporary Customer promotion
ledger was retired after its live evidence was consolidated here.

## Next Steps

No further Customer-only ledger step is independently implementable. Customer Actions now has read access to
`Concertable.Testing.Architecture`; Customer PR #3 restored the service-local suite to
`Concertable.Customer.slnx`, passed exact-head CI, merged, and passed post-merge CI. The preserved
AppHost composition suite remains outside that solution until the AppHost's ten foreign monorepo references
are available as published hosting artifacts; do not suppress the assertion or reintroduce source fallback.

## Completed work

- Customer backend, web, mobile, and `@concertable/customer` histories were folded into the private
  repository; local Customer workspaces use `file:` linkage and external
  `@concertable/{shared,web,mobile}` dependencies use the published `alpha` channel.
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
- `a12ab45` centralizes the Customer database connection name behind service-local `Db.Name` across runtime,
  design-time, and integration-fixture registration while preserving the AppHost composition alias.
- `6271c23` makes `Concertable.Customer.slnx` canonical, removes carve/`UseLocalCore` residue, packages the
  TestKit README, compiles package usage in the consumer preflight, hardens checkout credential handling,
  and corrects standalone guidance. `79cb07d` retains the previously validated ArchitectureTests boundary
  after its package-specific ACL failure.
- Customer PR #1 merged normally from reviewed head `79cb07d` as `6eb1226`; post-merge CI is green and the
  repository Actions settings retain SHA pinning, read-only defaults, and disabled PR approvals.
- Customer PR #3 granted Customer Actions package read access to `Concertable.Testing.Architecture` and
  restored the service-local `Concertable.Customer.ArchitectureTests` project to the canonical solution.
  It retained the existing AppHost strict-composition assertion separately in
  `Concertable.Customer.AppHost.ArchitectureTests`, outside the known foreign-hosting closure.

## Verification

- Post-merge CI run [`33646229860`](https://github.com/Concertable/customer/actions/runs/33646229860) at
  `6eb1226958732f29cc5fecb866461faf594e0e67`: Backend job `100301555908` and Frontend job
  `100301556327` passed, including the canonical solution build/tests, seven migration snapshots,
  five-package pack and compile-use consumer closure, three OCI candidates, migration/simulator, integrity,
  and retention gates.
- Retained artifact `9849406524`, `customer-candidate-integrity-4e4aa1c5354da0f1e0af912ba7c5c24865e38aea`,
  expires 2026-10-02 and has digest
  `sha256:06107990940fca0005c5c5eff40eef7aa9c39c627ecfde48bc8ac4629a1ff8e9`.
- Run [`33635705299`](https://github.com/Concertable/customer/actions/runs/33635705299) proved the otherwise
  green canonical solution cannot restore `Concertable.Customer.ArchitectureTests` because Customer lacks
  package-specific access to `Concertable.Testing.Architecture` (`403`). `79cb07d` restored the project's
  prior exclusion from the canonical solution; it did not delete or weaken the architecture tests.
- Customer PR #3 exact-head run
  [`33879657287`](https://github.com/Concertable/customer/actions/runs/33879657287) passed Backend job
  `101044918729` and Frontend job `101044918492` at `d94cbb9a4f21ee9c81d6069f682a08e765257452`.
  It merged as `5f34731785f786ad9cf6864ddae59fef2fac6337`; post-merge run
  [`33880491018`](https://github.com/Concertable/customer/actions/runs/33880491018) passed Backend job
  `101047668124` and Frontend job `101047668099`.
- Actions policy remains `enabled: true`, `allowed_actions: all`, `sha_pinning_required: true`; default
  workflow permissions remain `read` and `can_approve_pull_request_reviews: false`. The workflow remains read-only and
  both checkout invocations set `persist-credentials: false`.
- Concertable PR #922 merged as `ac74fdf9a0687a436872a7c1c4da622126e7885b`; its automatic packages run
  `33644847202` succeeded through `Push to GitHub Packages`, and its automatic images run `33644847172`
  succeeded through nine `Build and push the image` jobs. This publication was not an authorized Customer
  action and is the explicit merge gate for this closeout PR.

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
- Independent database/schema convention review approved
  `08ddbd812fd037544be47da2530098c49b278e86..a12ab4574858743ddc30432cc0bedf567a8303c2`
  with no findings after verifying connection-name closure, the intentional composition alias, all seven
  module `Schema` owners, and untouched generated migrations. Draft PR #1 still owns the cumulative delivery
  gate before any merge.

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
  migration resource. Customer's read-only workflow performed no publication, canonical release, visibility
  change, deployment, or system-consumer update. Separately, merging required Concertable ledger-only PR #922
  automatically pushed packages and images through the monorepo's main-push workflows; that violated the
  user's no-publication constraint and must be explicitly accepted or fixed outside this Customer stream before
  this closeout ledger PR may merge.
- The organization quota recalculated without Customer deleting another stream's caches. Failed-job rerun
  attempt 3 created the required retained artifact, so the quota blocker is closed.
- Customer now has repository-wide bootstrap `CODEOWNERS` for `@tomjseery`; all workflow actions are pinned to verified immutable SHAs, and repository Actions requires SHA pinning. GitHub still returns the private-plan `Upgrade to GitHub Pro or make this repository public` `403` for both repository rulesets and `main` branch protection. Do not bypass or retry that delivery-time capability gate.
- `Concertable.Customer.ArchitectureTests` is restored to `Concertable.Customer.slnx` after the package
  owner granted `Concertable/customer` Actions read access to `Concertable.Testing.Architecture`. Its
  AppHost assertion remains separately retained until the foreign hosting closure is published; do not replace
  either test with source fallback or suppress it.
- The extracted `Concertable.Customer.AppHost` remains excluded from `Concertable.Customer.slnx` and has ten foreign
  monorepo `ProjectReference`s. Invoking the Customer migration resource there and removing runtime
  `MigrateAsync` is not independently buildable or validatable until its foreign container-hosting inputs are
  available. Do not fake that gate or widen this stream into RT3, Stage 4, Auth, Payment, Search, or B2B.
- Vitest invokes Vite with `command = serve` and `mode = test`; development-only configuration must consider
  both values rather than treating every `serve` configuration load as a live dev server.
- A multi-path fold must include support files outside selected app subtrees: Customer's relocated Vite app
  uses `app/.env.production`, and Expo's unchanged `../assets/*` references require `app/assets/`.
- This ledger has no write ownership over the monorepo RT3 or fleet branches.
