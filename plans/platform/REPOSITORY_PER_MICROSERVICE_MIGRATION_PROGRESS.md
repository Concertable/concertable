# Repository-per-microservice migration progress

- Plan: `plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md`
- Roadmap: `plans/platform/POLYREPO_ROADMAP.md`
- Roadmap item: `platform/polyrepo-cut`
- Worktree: none active for landed work. Next work opens a fresh one — see `## Resume prompt`.
- Branch/PRs: stage 2 runs as publish round-trips — **rt1** (`IsPackable` on `AppHost.Shared`, PR #805) and
  **rt2** (the four `*.Hosting` packable + published + cross-service deps as packages, PR #809) are both
  **MERGED**. **rt3** is the small unblocked remainder: swap the 4 `AppHost.Shared.UnitTests` refs + delete
  `mirror.yml`/`mirror-parity.yml`.
- Dependency/package gates: none active. Stage 1 changed no *published contract* — it consumes only
  packages already on the feed at the pinned `0.1.0-alpha.0.1195`. But "publishes nothing" was wrong
  about the *pipeline*: every push to `main` reruns `publish-packages.yml`, which republishes all
  `Concertable.*` at a fresh MinVer version, and `platform-sync.yml` then auto-opens a self-landing
  pin-bump PR. Stage 1's merge (#798) duly triggered publish `0.1.0-alpha.0.1202` and sync PR #803
  (auto-merge armed, superseded #801); both self-managed with no action needed. Stage 2's own
  `IsPackable` publishes ride exactly this machinery.
- Last reconciled: **2026-08-27** — stage 9 precondition check (blocked, see `## Stage 9`); **stage 1 + stage-2 round-trips 1–2 merged** (PRs #798, #805, #809;
  platform at `0.1.0-alpha.0.1211`, all four `*.Hosting` + `Customer.Ticket.Contracts` on the feed).
  rt3 is the small remainder; the `*.ArchitectureTests` carve gate was **corrected out of stage 2 into
  stage 3** (it fails until AppHost image mode removes the sibling `AddProject` edges — see `## Next Steps`).

## Current state

**Approved by Tommy 2026-08-26 and in execution. Checkpoint 0 is largely delivered; stage 1 is
delivered (see `## Stage 1`).** The epic is
explicitly decoupled from the launch roadmap: `POLYREPO_ROADMAP` §6 gated the cut on the whole launch
plan shipping, and that gate is withdrawn. The monorepo taxes every launch PR (full E2E, full checkout,
full migration, blast radius over untouched services), so cutting accelerates launch rather than
delaying it.

### Verified rescope — 2026-08-26

The plan's inventory was captured at `d3c399ec8` (2026-08-02) and had drifted 2873 commits. Four
independent read-only audits re-verified every checkpoint against current `main`. Headline: the plan
was **right about the hard parts and wrong about the easy ones**.

| # | Checkpoint | Verified status |
|---|---|---|
| 0 | Baseline, permissions, inventory | **~70% done** — generator, ownership map, extraction map, coverage validator committed |
| 1 | DB ownership + owner-local migrations | **0%** — Duende grants still on `B2BDb`; all 5 AppHosts still provision foreign DBs; 6 runtime programs still call `MigrateAsync` at startup |
| 2 | Container-hosting seam | **~25%** — `*.Hosting` projects exist and are genuinely composition-only, but none is packable, no Dockerfile exists anywhere, no image mode, no boundary test |
| 3 | Producer-owned seeding | **0%** — B2B has Contracts+Simulator; Customer and Search have neither |
| 4 | Decouple full-stack E2E | **~0% — the long pole.** E2E references service `AppHost`, `Web` and every module `Infrastructure`; no TestKit exists |
| 5 | Frontend platform boundaries | **~85% done** by the POLYREPO_FULLSTACK effort — six npm tiers published, cross-tree aliases gone; only `@concertable/build-config` remains |
| 6–14 | Create repos; cut over platform, then auth → payment → search → customer → b2b | **0%** — target repos absent |
| 15–16 | Prove deploy/rollback; archive monorepo | **0%** |

**Corrections to the plan text, from evidence:**

- **Checkpoint 5's constraint is superseded.** It says keep `@concertable/b2b` and `@customer/shared`
  unpublished; both are already published tiers. Reality overtook the plan.
- **Package naming drifted:** the plan says `@concertable/web-shared`; the published tier is
  `@concertable/web`.
- **The six mirrors no longer exist — the rename sub-task is void (verified 2026-08-27).** `gh repo view`
  resolves none of `Concertable/{b2b,customer,auth,payment,search,shared}`, and `gh repo list Concertable`
  returns exactly five repos: `concertable`, `agent-standards`, `docs`, `config`, `infra`. The canonical
  names are therefore already free, and `mirror.yml`/`mirror-parity.yml` target deleted repositories —
  which is what round-trip 3 deletes.
- **`Concertable/config` and `Concertable/infra` exist** (private, unused proof-of-concepts). Retained
  untouched for production use; neither is a migration target.
- **`Concertable/system` is renamed `Concertable/fleet`.** It composes and ships the fleet; "system"
  read as core infrastructure and misdescribed it.
- **Ownership gaps the plan never assigned, now settled:** `app/web/admin` → b2b (the admin console is
  B2B-exclusive), `Concertable.Frontend.Hosting` → platform-dotnet, `app/mobile/shared` → platform-web.

**Measured split shape** (`eng/repository-split/inventory.json`): b2b 71 projects / 7 workspaces;
customer 57 / 3; platform-dotnet 45; payment 20; search 14; auth 8; fleet 2; platform-web 3 workspaces.
A payment change would load a 20-project repo instead of a 217-project one.

**Exactly one hard blocker exists in any production runtime closure:**
`Concertable.Auth.Contracts -> Concertable.Messaging.Contracts`, which becomes a `PackageReference` on
Auth extraction. All other 136 cross-repository edges live in AppHost, E2E, test or `*.Hosting`
projects, each owned by its own checkpoint.

**Delivered on this branch (pushed, no PR yet):** the re-baseline onto current `main` (was 2873
behind), the `eng/repository-split/` inventory generator with its `--check` drift gate, the complete
target ownership map, the `map.yaml` extraction map with its coverage validator (4303 paths, 0
duplicate claims), per-service CI test scoping with a 14-case classifier test, and this rescope.

**The plan document's 17 checkpoints are superseded by the nine stages** in `## Next Steps`. Its
inventory figures are a 2026-08-02 snapshot; this ledger overrides them.

## Stage 9 (archive the monorepo) — precondition check, 2026-08-27

Requested directly; **not executable, and no work was attempted beyond this read-only check.** Stage 9 is
the terminal stage and its precondition is "all services and platform extracted". Nothing is extracted.

- **No extraction target exists.** `gh repo view` resolves none of `Concertable/{auth,payment,search,customer,b2b,platform-dotnet,platform-web,fleet}`
  (nor the pre-rename `system`). `gh repo list Concertable --limit 100` returns 5 repos, none of them a target.
- **Every service is still monorepo source.** `api/` still holds `Concertable.{Auth,B2B,Customer,Payment,Search}`
  plus `DataAccess`, `Messaging`, `Shared`, `ServiceDefaults`, `AppHost`, `AppHost.Shared`, `Frontend.Hosting`;
  `app/` still holds `b2b`, `customer`, `mobile`, `shared`, `web`.
- **The monorepo still publishes and mirrors.** `.github/workflows/` still carries `publish-packages.yml`,
  `publish-fe-packages.yml`, `platform-sync.yml`, `platform-sync-alert.yml`, `mirror.yml`, `mirror-parity.yml`
  and `test.yml`; `main` moved today (`fd52181fe`, platform-sync `0.1.0-alpha.0.1219`).
- **Position on the nine stages:** stage 2 round-trip 3 is the open remainder. Stages 3–8 (AppHost image mode,
  E2E → `fleet`, the six extractions) are all outstanding, and plan checkpoint 15's hard stop — Tommy reviews
  deploy/rollback evidence from canonical repos — has not been reached.

Resume path is unchanged: round-trip 3, then stage 3. Stage 9 re-enters only after stage 8 closes.

## Verification — Payment extraction proven end to end (2026-08-26)

A real `git-filter-repo` extraction was run, not a paper exercise. `git clone --single-branch --branch
main` of the monorepo, then `--path api/Concertable.Payment/ --path-rename api/Concertable.Payment/:`.

**Result: a coherent standalone repository.** 5078 commits filtered to **802**; 410 files; the root is
already the target layout — `src/`, `tests/`, `tools/`, its own `Concertable.Payment.slnx`,
`Directory.Build.props`, `Directory.Packages.props`, `nuget.config`, `AGENTS.md`, `ARCHITECTURE.md`,
`README.md`, `TECH_DEBT.md`. No rename beyond stripping the prefix was needed.

**`dotnet build Concertable.Payment.slnx --configuration Release` against the real GitHub Packages
feed: the entire `src/` runtime compiled clean.** Every one of the 31 errors was in a single project,
`Concertable.Payment.IntegrationTests`, and every one was `Concertable.Testing` / `SqlFixture`
missing — the platform test library, referenced across the service boundary.

**Root cause, and it is small.** `EnforceServiceBoundary` is deliberately off for test projects, so
test projects reach platform test libraries by `ProjectReference` where runtime projects use
`PackageReference`. That is invisible in the monorepo and fatal on extraction.

**Measured blast radius — 45 non-E2E test-tier cross-repository `ProjectReference`s:**

| Referenced project | Count | Already published? |
|---|---:|---|
| `Concertable.Testing` | 25 | yes |
| `Concertable.Testing.Architecture` | 6 | yes |
| `Concertable.Testing.Integration` | 5 | yes |
| `Concertable.Seed.Shared` | 2 | yes |
| `Concertable.Messaging.Domain` / `.Infrastructure` | 2 | yes |
| `Concertable.Seed.Infrastructure` | 1 | yes |
| `Concertable.{Auth,B2B,Customer,Payment}.Hosting` | 4 | **no — must be made packable** |

**41 of 45 are one-line `ProjectReference` → `PackageReference` swaps against packages already on the
feed.** Only the four `*.Hosting` references need a producer step first, which is checkpoint 2's
packable-Hosting item. Affected test projects per target: b2b 11, customer 9, search 4, auth 3,
payment 3, platform-dotnet 1, fleet 1.

**Two environment findings that would have derailed the real run.** `git-filter-repo` is not installed
as a git subcommand and must be invoked as its module script. And a fresh clone plus extraction exceeds
Windows `MAX_PATH` on `Concertable.Payment.E2ETests.Helpers.UnitTests`; `core.longpaths=true` is
mandatory on any clone used for extraction (the monorepo itself already sets it, so the failure appears
only in a fresh clone).

## Stage 1 — test-tier package boundary (delivered 2026-08-26)

All **41** non-`*.Hosting` test-tier cross-repository `ProjectReference`s across **31** projects now
declare the published package instead. Cross-target edges fell **135 → 94**; test-tier edges **45 → 4**,
and the 4 remaining are the `AppHost.Shared.UnitTests → *.Hosting` refs that are stage 2's.

**The mechanism is one file, not 41 conditional pairs.** A literal one-line swap to `PackageReference`
would have forced a publish/restore round-trip on every `Concertable.Testing` change for as long as the
monorepo survives (through stage 9). Instead `api/PlatformSourcePackages.targets` holds one table
mapping each platform package to its in-repo source, and each test csproj carries a single
`<PackageReference>`:

- **In the monorepo** the targets file swaps that reference back to a `ProjectReference`, so the build
  graph and the inner loop are byte-identical to before — verified by `dotnet msbuild -getItem`.
- **In a carved repo** neither the targets file nor the platform source is present (both live above the
  service folder), so the declaration stands and restore comes off the feed. **The extracted repo needs
  no edit** — the import's own `Exists()` guard is the cut-over.
- `-p:UseLocalPlatformPackages=true` forces package mode in place, which is what
  `scripts/local-platform.ps1` already passes.
- Gated to the test tier only (the same `[\\/][Tt]ests[\\/]` test used by `EnforceServiceBoundary`), so
  no runtime project's package closure moves. Imported *after* each folder's `UseLocalCore` swap, which
  removes the same ids first — that ordering is what stops both mechanisms firing on `Messaging.*`.

This supersedes the earlier per-csproj dual-conditional pattern; its 8 instances collapsed to one line
each.

**Verified — the gate is met.** `git archive` carve of `api/Concertable.Payment` (the cheap equivalent
of the proven `git-filter-repo` extraction, and what CI already uses), built `--configuration Release`
against the real feed:

| Carve | Result |
|---|---|
| payment — closure + `UnitTests` + `IntegrationTests` + `PublishedContractFixture` | **0 errors** |
| auth — `Concertable.Auth` + `UnitTests` + `IntegrationTests.Fixtures` + `IntegrationTests` | **0 errors** |
| search — closure + `UnitTests` + `IntegrationTests.Fixtures` + `IntegrationTests` | **0 errors** |

The 31 `Concertable.Testing` / `SqlFixture` errors that blocked the extraction are gone. All seven
target packages were confirmed present on the feed at `0.1.0-alpha.0.1195` before any edit.

**It cannot silently regress.** Two new gates, both wired into `ci-complete`:

- **`split-inventory` job** — `inventory.py --check` now fails on any test-tier cross-repository
  `ProjectReference` (new `blockingTestEdges`), repo-wide, in seconds, with no build and no feed. This
  is the durable enforcement `EnforceServiceBoundary` structurally cannot provide, since it exempts the
  test tier by design.
- **The five carve jobs now build their non-E2E test tier**, not just the deployable closure. B2B and
  Customer discover it by path, so a new unit/integration/fixture project is covered automatically.

**Discovered, and it is stage 2's work list.** `*.ArchitectureTests` cannot join the carve yet: it
references the service `AppHost`, which references `*.Hosting`. Carving Payment with ArchitectureTests
in produces exactly **7 errors, all in `src/Concertable.Payment.Hosting`** — missing
`Concertable.Messaging.AzureServiceBus`, `Concertable.AppHost.Shared` (`AsbTopology`,
`SqlServerDatabaseResource`) and the sibling `Concertable.{Auth,B2B}` topologies. That is the precise
scope of stages 2–3, measured rather than estimated.

**Two further findings for later stages:**

- **`Concertable.Testing.E2E` is not published at all** (0 versions on the feed), unlike the other four
  testing libraries. Stage 4 must publish it, or the E2E harness cannot leave.
- **A carved service folder loses `api/TestConventions.targets`**, so the test-tier naming gate does not
  travel with it. Whoever extracts `platform-dotnet` owes that file a home the services can consume.

## Next Steps

### The nine stages, evidence-backed

The original 17 checkpoints assumed work that is already done. The carve CI already proves every
service restores from the package feed, the platform packages are already published, and the Payment
extraction above built its whole runtime clean. What actually remains is nine stages. **The monorepo
survives as the fallback for local development and cross-service E2E until stage 9**, so a service can
be extracted before its AppHost and E2E story is perfect.

1. ~~**Swap the 41 test-tier `ProjectReference`s to `PackageReference`.**~~ **Done — see `## Stage 1`.**
2. ~~**Make the four `*.Hosting` projects packable and publish them**, then swap the last 4 of the 45.~~
   **Packable + published — DONE (round-trips 1–2; on the feed at `0.1.0-alpha.0.1211`).** Remaining in
   stage 2: swap the last 4 `AppHost.Shared.UnitTests` refs + delete `mirror.yml`/`mirror-parity.yml`
   (round-trip 3, small/unblocked). **CORRECTION (2026-08-27):** adding `*.ArchitectureTests` to the carve
   jobs is **NOT** stage 2's gate and is **NOT** unblocked by `*.Hosting` reaching the feed — the earlier
   "once `*.Hosting` resolves from the feed" claim (from the stage-1 commit `bc1daf488`) was wrong. Verified
   empirically: the carve **with `*.ArchitectureTests`** still fails (`MSB3202` — `Payment.AppHost`
   `AddProject`s the sibling **Auth deployable**, absent in a single-service carve). That gate is owned by
   **stage 3**, below.
3. **AppHost image mode** — foreign services (and `AppHost.Shared`) composed via `AddContainer` on a pinned
   image instead of `AddProject` against sibling source; this is what converts the **44 `apphost`
   `AddProject(sibling)` edges** (measured untouched in rt2). .NET 10 SDK container publishing means no
   Dockerfile per host. It lets a service leave without breaking every other service's local development —
   **and it OWNS the `*.ArchitectureTests`-in-the-carve-jobs gate** (moved here from stage 2, verified
   2026-08-27): that gate can only pass once these `AddProject(sibling)` edges are gone.
4. **Move cross-service E2E to `fleet`** — 21 E2E cross-repository edges, and no TestKit exists yet.
   The largest single stage; it gates `fleet`, customer and b2b, but not auth or payment.
5. **Extract `payment`** — mechanism already proven end to end.
6. **Extract `auth`** — needs Duende persisted grants moved from `B2BDb` to `AuthDb` first.
7. **Extract `search`, then `customer`, then `b2b`** — b2b last, widest contract and seed fan-out.
8. **Extract `platform-dotnet`, `platform-web`, `fleet`.**
9. **Archive the monorepo.** Terminal stage; nothing to rename — the mirrors are gone (above).

Stages 1 and 2 are days.

**Sequencing correction — these stages are NOT a strict chain.** `blockingRuntimeEdges` is **1** for the
whole repo (`Auth.Contracts → Messaging.Contracts`, a one-line swap on Auth extraction) and all five
carve gates are green, so **nothing blocks extracting a service today**. Stages 3 (AppHost image mode)
and 4 (E2E → `fleet`) run in `fleet` **in parallel with** the extractions (stages 5–7), not in front of
them. Do not re-estimate archival on paper again — report the measured rate after one real extraction.

### Immediate next action

**Round-trip 3 (small, unblocked).** `*.Hosting` is packable and published (round-trips 1–2 DONE). What
remains of stage 2 is only: swap the last 4 `Concertable.AppHost.Shared.UnitTests` `ProjectReference`s → the
published `*.Hosting` (use the stage-1 `PlatformSourcePackages.targets` source-swap-back, consistent with the
rest of the test tier), and delete `mirror.yml` + `mirror-parity.yml`. This does **not** touch the carve jobs
or `test.yml`, so it is a normal PR (no forced full-e2e).

**Do NOT add `*.ArchitectureTests` to the carve jobs in round-trip 3.** Verified 2026-08-27 that gate fails
until **stage 3 (AppHost image mode)** removes the `AppHost → sibling deployable` `AddProject` edges. After
round-trip 3, the next substantial stage is **stage 3**, which owns that gate.

**Use the `git archive` carve, not a `git-filter-repo` clone, to verify.** It is the same gate at a
fraction of the cost, it needs no fresh clone (so no `core.longpaths` trap), and CI already runs it.
Reach for `git-filter-repo` only at stage 5+, when history actually has to move. **When a stage claims a
gate becomes possible after a prior stage, prove it by running that gate at the prior stage's closeout
before writing it as the next step — the mis-sequenced `*.ArchitectureTests` gate above was asserted, never
run, and cost a wasted phase.**

### Interaction with the two open PRs

Neither blocks this epic and this epic need not wait for them. #633 is B2B-only and the payment
refactor is Payment-only, so each has exactly one destination repository and can be replayed there as
an early PR rather than forced through the monorepo first. B2B is cut last, so #633 has the most
runway of any open work. Checkpoints 1, 2 and 4 churn `main` repository-wide, so both branches need
rebasing regardless of this epic's timing.

## Completed work

| Item | Evidence |
|---|---|
| Plan authored | commit `91e92c445` (docs-only; adds `plans/REPOSITORY_PER_MICROSERVICE_MIGRATION.md`, removes `POLYREPO.md` + `SPLIT_TIME_E2E_STRATEGY.md`) |
| Design-review blockers resolved | commit `3f8cb3494` (plan +55/-33, `plans/DEPLOYMENT.md` updated) |
| Checkpoint 0 — inventory generator, ownership map, extraction map + coverage validator | commits `f38825fba`, `fddd3d8ef`, `4fa96f8ed` |
| Per-service CI test scoping | commit `b0bbbdb06` |
| **Stage 1 — test-tier package boundary** | this branch; see `## Stage 1` |

Checkpoints 1–16 of the superseded numbering are not complete; the live plan is the nine stages, of
which stage 1 is done.

## Verification

**Stage 1.** Three `git archive` service carves built `--configuration Release` against the live feed —
payment, auth and search, each including its non-E2E test tier — **0 errors** each. Item-graph
equivalence in the monorepo confirmed with `dotnet msbuild -getItem:ProjectReference -getItem:PackageReference`
in both default and `UseLocalPlatformPackages=true` modes, including a runtime project to prove no
deployable closure moved. `python eng/repository-split/inventory.py --check` passes with
`blockingTestEdges: 0`. `.github/workflows/test.yml` parses (20 jobs).

**Not run locally:** the full `dotnet build api/Concertable.slnx` and the unit/integration suites — the
dev machine's C: drive is at 0 bytes free (see the event log). CI's `build` job and the scoped
unit/integration matrices cover both on the PR.

**Stage 1 CI outcome (PR #798, merged 2026-08-26).** 77/77 checks green, 0 failures, on the PR head and
again through the merge queue. `build` runs via `local-platform.ps1`, so the whole test tier compiled in
**package** mode; `carve-b2b` / `carve-customer` were green **including their newly-added test tier** —
neither of which fits on the dev machine.

## Reviews

A design review of the plan occurred (commit `3f8cb3494 docs(plan): resolve migration review
blockers` records the resolution). No review artifact for this plan exists under `reviews/`. Its
findings are recorded only as resolved via that commit; treat the specific findings as unknown beyond
"blockers resolved in the plan text."

**Stage 1 code review — work order complete/approved, four findings all fixed:**

- **F1 (high)** — the epic introduced exactly the silent breakage it exists to remove:
  `api/tests/Directory.Build.targets` shadowed `api/Directory.Build.targets` (MSBuild imports only the
  nearest), so `Concertable.AppHost.ArchitectureTests` lost `ValidateTestConventions` and its
  `ConcertableTestTier` went `Architecture` → empty. Fixed.
- **F2** — route-table gap. Fixed; an incremental pass then caught the first fix keyed on `^api/`, which
  `CarvedTreeReplay.test_no_row_names_a_path_outside_the_repo` correctly rejects — re-fixed.
- **F3** — `PACKAGES.md` contradiction. Fixed.
- **F4** — comments. Fixed.

## Decisions, discoveries, blockers, and deviations

- **Decision — nine canonical repositories:** five service repos (B2B, Customer, Payment, Search,
  Auth), two platform repos (`platform-dotnet`, `platform-web`), one `system` repo (full-stack
  AppHost, fleet manifest, IaC, deployment, black-box E2E), and `Concertable/.github`.
- **Decision — lockstep `ConcertablePlatformVersion` + platform-sync PR are retired** in favour of
  independently versioned producer trains + Renovate; breaking Contracts use expand/publish/migrate/
  contract, never a repo-wide forced bump.
- **Decision — B2B Workers ships as a container on native Azure Functions on Container Apps**, not
  Functions Consumption (Consumption cannot run the custom container). This supersedes the earlier
  deployment design on that point.
- **Discovery — deployable closures are already package-clean:** the only non-AppHost/non-test
  cross-area `ProjectReference` is `Concertable.Auth.Contracts -> Concertable.Messaging.Contracts` (a
  platform edge). All other cross-area edges live in AppHost/E2E code.
- **Discovery — Auth persisted-grant coupling:** Auth's Duende persisted grants currently live in
  `B2BDb` and must move to `AuthDb` before Auth extraction (Checkpoint 1).
- **Discovery — mirrors are stale:** the six generated mirrors' latest parity runs are red (all six
  differed from `main` on 2026-08-02); they are historical bootstrap inputs, not trusted cutover
  sources, and need a final refresh + independent history verification.
- **Discovery — target repos do not exist yet:** no `Concertable/config`, `/system`,
  `/platform-dotnet`, or `/platform-web`; the planning credential lacks `read:packages`, so package
  ACL verification is an explicit Checkpoint 0 preflight, not an assumption.
- **Decision — the test tier declares packages and swaps back to source, rather than swapping outright.**
  See `## Stage 1`. This is the one deliberate deviation from the plan's stage-1 wording.
- **Discovery — `*.ArchitectureTests` reaches the AppHost composition layer**, so it cannot carve until
  stages 2–3; the other three test tiers can and now do.
- **Discovery — `Concertable.Testing.E2E` has never been published** (0 feed versions), unlike its four
  sibling testing libraries — because it has a **dependency cycle across future repo boundaries**: it
  references *back into* `Concertable.Payment.E2ETests.{Web,Workers}`. That direction must be inverted
  before E2E can move to `fleet`; it is why this one testing library alone cannot publish. A stage 4
  prerequisite.
- **Discovery — `api/TestConventions.targets` does not survive a carve**, so an extracted service loses
  the test-tier naming gate. It **must ship from `platform-dotnet` as a `buildTransitive` targets
  package** or every extracted service loses that gate.
- **Decision — the carve/inventory scaffolding is disposable, not portable.** `carve-*`,
  `split-inventory` and `PlatformSourcePackages.targets` only *simulate* the end state; once a service is
  its own repo its ordinary CI answers the same question. They are deleted at stage 9, not migrated
  (`PlatformSourcePackages.targets` self-disables via its `Exists()` guard). Only two things need a new
  home on extraction: `TestConventions.targets` (above), and `local-platform.ps1`, which needs a
  per-repo redesign.
- ~~Blocker/gate — awaiting Tommy's review~~ — approved 2026-08-26; the branch is re-baselined onto
  current `main`.

## Event log

### 2026-08-04 — Reconstructed baseline (this ledger created)

- Action: Created this progress ledger for a legacy plan that had none, via the `resume-plan`
  reconstruction path. Baseline is explicitly reconstructed from repository evidence, not fabricated
  history.
- Evidence: `git log origin/main..HEAD` = 2 docs commits (`91e92c445`, `3f8cb3494`); merge-base with
  `origin/main` = `d3c399ec8`; `git status` clean; `gh pr list --head Plan/RepositoryPerMicroserviceMigration
  --state all` = empty; `git rev-list --count HEAD..origin/main` = 138; no `reviews/` artifact for this
  plan; plan document header states "awaiting Tommy's review."
- Outcome: Ledger records design-only status, no implementation, no PR, no verification, no active
  package/platform gate.
- Follow-up: Await Tommy's approval to begin Checkpoint 0; sync the branch with `origin/main` before
  any Checkpoint 0 implementation.

### 2026-08-26 — Stage 1 delivered

- Action: Declared all 41 non-`*.Hosting` test-tier cross-repository references as published packages,
  behind one source-swap mechanism (`api/PlatformSourcePackages.targets`); added the missing
  `PackageVersion` pins in six folders; added `blockingTestEdges` + the `split-inventory` CI job; extended
  the five carve gates to the non-E2E test tier.
- Evidence: 41 refs / 31 projects; cross-target edges 135 → 94, test-tier 45 → 4; three carves green at
  0 errors; `--check` green with `blockingTestEdges: 0`.
- Deviation: the plan said "swap to `PackageReference`". A literal swap would force a publish round-trip
  on every shared-test-library change until stage 9, so the reference is *declared* as a package and
  swapped back to source while the platform source is on disk. Same end state after the cut, no inner-loop
  regression before it.
- Environment: the dev machine's C: drive hit **0 bytes free** mid-stage, which killed the full-solution
  build. `dotnet nuget locals http-cache/temp --clear` freed only 68 MB. The largest reclaim found is
  ~25 GB of Sep-2025 ETW captures (`sc.*.etl`) in 52 GUID folders under `%LOCALAPPDATA%\Temp`, but they
  are ACL-locked to SYSTEM — all 49 deletes returned access-denied, so they need an elevated shell.
  Other candidates, unmeasured by any agent-runnable scan: `C:\tmp` (26 GB), `C:\Users` (151 GB), and
  ~174 GB not visible to a file scan at all (WSL/Docker VHDX, pagefile). `C:\ce-extract` (250 MB) is a
  leftover from the earlier `git-filter-repo` verification and is disposable.
- Follow-up: stage 2, and the two later-stage findings recorded in `## Stage 1`
  (`Concertable.Testing.E2E` unpublished; `TestConventions.targets` does not survive a carve).

### 2026-08-26 — Stage 1 merged; stage 2 begun (round-trip 1)

- Action: Confirmed PR #798 merged through the queue (merge-group run green, e2e-ui the last job).
  Closed the stage-1 worktree. Opened `Plan/RepoSplit-Stage2-AppHostShared`. Set
  `<IsPackable>true</IsPackable>` on `Concertable.AppHost.Shared` (round-trip 1's whole opt-in).
- Evidence: #798 `mergedAt` 2026-08-26T19:48:33Z; publish run `33007120356` green (all `Concertable.*`
  at `0.1.0-alpha.0.1202`); sync PR #803 auto-merge armed, #801 auto-closed as superseded.
  BUILD1 holds — AppHost.Shared's only `ProjectReference` (`Messaging.AzureServiceBus`) is already
  packable; no Reunion carrier so no `PrivateAssets`. `dotnet pack` of AppHost.Shared: 0 errors.
- Correction: the handoff's "stage 1 publishes nothing — verify" and this ledger's earlier "it
  publishes nothing and needs no platform-sync" were both wrong about the *pipeline*: every `main` push
  republishes everything and auto-opens a self-landing sync PR. Nothing broke; the sync PRs self-manage.
- Follow-up: merge round-trip 1 → let publish + sync land → round-trip 2 (swap 15 Contracts refs + the
  5 AppHost.Shared refs in `*.Hosting` to `PackageReference`, `IsPackable` on the four `*.Hosting`) →
  round-trip 3 (last 4 refs in `AppHost.Shared.UnitTests`; delete `mirror.yml` + `mirror-parity.yml`).
  Gate: add `*.ArchitectureTests` to the five carve jobs once `*.Hosting` resolves from the feed.

### 2026-08-26 — Stage 2 round-trip 2 delivered (branch `Plan/RepoSplit-Stage3-Hosting`)

- Action: swapped the **20** cross-service `*.Hosting` `ProjectReference`s (15 Contracts + 5
  `AppHost.Shared`) to `PackageReference`; made `Auth/B2B/Customer/Payment.Hosting` packable
  (`<IsPackable>true</IsPackable>`); left `Search.Hosting` unpackable (nothing consumes it cross-repo).
  Added the `Concertable.AppHost.Shared` `PackageVersion` in all five service folders (the sync bot does
  not add it) and the missing Contracts/Email pins (Payment: `B2B.Concert`/`B2B.Tenant.Contracts`; Auth:
  `Shared.Email.Application`). Regenerated `inventory.json` (`--check` green).
- **BUILD1 resolved structurally, not hacked.** `Payment.Hosting`/`Customer.Hosting` each dragged one
  non-packable Application layer in only to name one queued command in the ASB topology. Those commands —
  `ProcessStripeWebhookCommand`, `SendTicketEmailCommand` — are `IIntegrationCommand` wire contracts
  (`[MessageType]`, ASB-queued) sitting in the wrong layer; the escrow commands already live in
  `Payment.Contracts`. Moved both to their `*.Contracts` (packable), which dissolved both Hosting→Application
  `ProjectReference`s with **no cascade**. Making the Application layers packable was rejected — it would
  cascade BUILD1 through Domain/Infrastructure and re-monolith the closure.
- **`Customer.Ticket.Contracts` made packable.** It was intra-Customer only (0 feed versions), so a packable
  `Customer.Hosting` keeping a `ProjectReference` to it would trip BUILD1. It is a Contracts layer with only
  package deps → packable with no cascade. Kept `<PackageReference Include="Reunion" />` without
  `PrivateAssets` to match its six sibling module Contracts (Artist/Concert/Tenant/Venue/User/Admin), which
  all expose `Option<>` carriers the same way; the skill's `PrivateAssets` rule is honoured only by Kernel
  in this repo — a pre-existing choice, not this round-trip's to change.
- **Auth folder MinVer trap fixed (the phase-2 bug, live here).** `Auth/Directory.Build.props` +
  `Directory.Packages.props` lacked MinVer + package metadata — Auth's only published contract
  (`Auth.Contracts`) lives in a *separate* folder, so the service folder had never published. Added MinVer +
  metadata mirroring Payment, else `Auth.Hosting` would have shipped `1.0.0`.
- **Bumped `ConcertablePlatformVersion` 1202→1206** across all six folders. `local-platform.ps1 prepare`
  restores the full slnx from the feed at the *real* `ConcertablePlatformVersion` (no local override at that
  step), and `AppHost.Shared` exists on the feed only at 1206+, so 1202 would fail the pack restore. 1206 is
  the current feed head (all packages published lockstep at #805's merge), so this is verified-safe — it is
  exactly what open sync PR **#808** does, which this **supersedes**.
- **Stage-3 question answered — the 44 `apphost` edges did NOT collapse.** `crossTargetEdgeCount` fell
  94→74 (the 20 Hosting→cross-service edges), `blockingTestEdges` still 0, `blockingRuntimeEdges` still the
  lone `Auth.Contracts → Messaging.Contracts`. All 44 `apphost` edges are rooted in the `Concertable.AppHost`
  executable orchestrator referencing sibling deployables (`*.Web`, `*.Workers`, seeders) and the
  `*.Hosting`/`AppHost.Shared` libraries via `AddProject` — untouched by Hosting packability. **AppHost image
  mode (stage 3) does NOT shrink to runtime-only;** converting those 44 `AddProject` edges to `AddContainer`
  is still its whole job.
- Verified locally: all 5 `*.Hosting`, both moved-command `*.Contracts`, and the Payment + Customer `*.Web`
  closures (Infrastructure/Application/Contracts — every moved-command consumer) build `-c Release` clean
  against the feed; pin cross-check green; `inventory.py --check` green. The full-slnx `build`,
  `local-platform-pack` (the real packability/BUILD1 gate) and the five carves run on CI — the dev machine's
  C: is at 3.1 GB free and cannot fit the full pack (as in stages 1–2).
- Tier: package-topology only, no `test.yml`/workflow/wire/UI change → **`skip-e2e`**. Deviation from the
  ledger's stage-2 wording, which bundled the carve `*.ArchitectureTests` gate (and its forced full-e2e)
  into stage 2: that gate restores `*.Hosting` from the feed, which is impossible until this round-trip
  publishes them, so it is necessarily a later PR (round-trip 3). The split is forced by publish-before-consume,
  not a shortcut.
- Review (2 lenses, one finding, fixed): the package-topology lens caught that `Auth.Hosting` kept
  `Auth.Contracts` as a `ProjectReference`, but `Auth.Contracts` lives at `api/Concertable.Auth.Contracts/` —
  *outside* the Auth carve root (unlike the other three Hosting, whose kept Contracts refs are intra-carve-root).
  The handoff mislabelled it "intra-service" by ownership; physically it escapes. Swapped to
  `PackageReference` (pin already present), matching the Auth deployable's documented "never a
  ProjectReference, so Auth carves standalone" rule. `Auth.Hosting` now has zero `ProjectReference`s.
- Follow-up: merge (skip-e2e) → publish lands the 4 `*.Hosting` + `Ticket.Contracts` on the feed → sync PR
  bumps pins → round-trip 3 (last 4 `AppHost.Shared.UnitTests` refs + add
  `*.ArchitectureTests` to the five carve jobs in `test.yml`, which forces full-e2e + security review +
  delete `mirror.yml`/`mirror-parity.yml`).
- **LANDED (2026-08-27).** **PR #809 merged** (`059165407`). During review its `carve-fe (web/b2b/venue)`
  went red — not from this diff: the branch had fallen 8 behind `main` and CI ran the FE carve against the
  stale `app/`, hitting a `@concertable/shared` `Checkout` mismatch already fixed on `main` by #810. Merged
  `origin/main` in (clean); the FE carve then skipped (backend-only PR) and the merge-queue run went green.
  Publish republished everything at **`0.1.0-alpha.0.1211`** — the four `*.Hosting` and
  `Customer.Ticket.Contracts` are on the feed; **`Search.Hosting` is correctly absent** (unpackable by
  design). Platform-sync **PR #812 synced `ConcertablePlatformVersion` → `0.1.0-alpha.0.1211`** (merged);
  `main` now pins 1211 everywhere and the `AppHost.Shared` pins added here follow the variable. (Note: the
  earlier 1206 sync **#808 merged independently** at 1206 before #809 landed, so the intra-PR 1202→1206 bump
  here was a no-op against `main` by merge time; the effective jump was 1206→1211 via #812. The "supersedes
  #808" framing above was written pre-merge and did not hold — #808 landed on its own.) **Round-trip 2 is
  DONE.** Next: round-trip 3.
- **CORRECTION (2026-08-27) — supersedes this entry's `Follow-up` and `Tier` bullets above.** Those said
  round-trip 3 would "add `*.ArchitectureTests` to the five carve jobs" and framed that gate as merely
  awaiting `*.Hosting` on the feed. **Wrong** — proven by running the carve *with* `*.ArchitectureTests`
  (fails `MSB3202`: the service `AppHost` `AddProject`s a sibling **deployable**, absent in a single-service
  carve). That gate belongs to **stage 3 (AppHost image mode)**, not round-trip 3. Round-trip 3's real scope
  is ONLY: swap the 4 `AppHost.Shared.UnitTests` refs (source-swap-back) + delete `mirror.yml`/`mirror-parity.yml`
  — see the corrected `## Next Steps`. Lesson banked there: prove a "possible after stage Y" gate by running
  it at Y's closeout before writing it as the next step.

## Resume prompt

```
/open-worktree Plan/RepoSplit-Stage2-rt3
Read plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PLAN.md and
plans/platform/REPOSITORY_PER_MICROSERVICE_MIGRATION_PROGRESS.md and do what its
`### Immediate next action` says.
```

Deliberately NO scope, gates, commands, versions or worktree specifics are restated here — they live in
the ledger above and drift the moment they are duplicated. That duplication is exactly what mis-sequenced
the `*.ArchitectureTests` gate (asserted in a copied "next step", never run). **The ledger is the single
source of truth; if a prompt and the ledger disagree, the ledger wins, and any "gate X becomes possible
after stage Y" claim must be re-proven by running the gate before it is acted on.**
