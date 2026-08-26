# Repository-per-microservice migration progress

- Plan: `plans/REPOSITORY_PER_MICROSERVICE_MIGRATION.md`
- Worktree: `C:\Users\TommySeery\source\repos\Concertable.worktrees\Plan\RepositoryPerMicroserviceMigration`
- Branch: `Plan/RepositoryPerMicroserviceMigration`
- PR: not opened (`gh pr list --head Plan/RepositoryPerMicroserviceMigration --state all` → empty)
- Dependency/package gates: none active. No checkpoint has been implemented, so nothing is published, pinned, or platform-synced against this plan.
- Last reconciled: 2026-08-04, reconstructed from git + PR evidence (this ledger did not previously exist).

## Current state

**Approved by Tommy 2026-08-26 and in execution. Checkpoint 0 is largely delivered.** The epic is
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
- **Mirrors are a month stale** (last pushed 2026-07-27) and are bootstrap inputs only, never cutover
  sources. Being pure `subtree split` output they hold no unique content, and the packages are linked
  to `concertable` rather than to them, so they are renamed to `<name>-mirror-archive-<date>` to free
  the canonical names — not deleted, which is irreversible and buys nothing.
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

The branch holds two docs-only commits on top of merge-base `d3c399ec8` (the plan's own stated
planning baseline):

- `91e92c445 docs(plan): design repository-per-microservice migration` — authored the plan; deleted
  the superseded `plans/POLYREPO.md` and `plans/SPLIT_TIME_E2E_STRATEGY.md`; touched
  `api/Concertable.B2B/TECH_DEBT.md` and `api/Concertable.Customer/TECH_DEBT.md` (1 line each).
- `3f8cb3494 docs(plan): resolve migration review blockers` — revised the plan (+55/-33) and
  `plans/DEPLOYMENT.md` to resolve design-review blockers.

Working tree is clean. No `api/**`, `app/**`, workflow, or migration code has changed on this branch.

The plan breaks the migration into **17 checkpoints (0–16)**, each a separate PR/merge boundary, most
with an explicit human-review hard stop. **None have started.**

Staleness to be aware of: the branch is **138 commits behind `origin/main`** (docs-only branch, so
this is expected and harmless while it stays design-only). The plan's inventory numbers were captured
against baseline `d3c399ec8` and may have drifted since — treat them as a baseline snapshot, not
current truth, and re-verify at Checkpoint 0.

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

## Next Steps

### The nine stages, evidence-backed

The original 17 checkpoints assumed work that is already done. The carve CI already proves every
service restores from the package feed, the platform packages are already published, and the Payment
extraction above built its whole runtime clean. What actually remains is nine stages. **The monorepo
survives as the fallback for local development and cross-service E2E until stage 9**, so a service can
be extracted before its AppHost and E2E story is perfect.

1. **Swap the 41 test-tier `ProjectReference`s to `PackageReference`.** Mechanical; every target is
   already on the feed. Verified by re-running the Payment extraction to a fully green build.
2. **Make the four `*.Hosting` projects packable and publish them**, then swap the last 4 of the 45.
3. **AppHost image mode** — foreign services composed via `AddContainer` on a pinned image instead of
   `AddProject` against sibling source. .NET 10 SDK container publishing means no Dockerfile per host.
   This is what lets a service leave without breaking every other service's local development.
4. **Move cross-service E2E to `fleet`** — 21 E2E cross-repository edges, and no TestKit exists yet.
   The largest single stage; it gates `fleet`, customer and b2b, but not auth or payment.
5. **Extract `payment`** — mechanism already proven end to end.
6. **Extract `auth`** — needs Duende persisted grants moved from `B2BDb` to `AuthDb` first.
7. **Extract `search`, then `customer`, then `b2b`** — b2b last, widest contract and seed fan-out.
8. **Extract `platform-dotnet`, `platform-web`, `fleet`.**
9. **Archive the monorepo** and rename the six stale mirrors to `<name>-mirror-archive-<date>`.

Stages 1 and 2 are days. The mirror renames happen at stage 9, not before: the canonical names are
needed only when the real repositories claim them, and renaming earlier leaves public repositories
with odd names for months.

### Immediate next action

**Stage 1.** Swap the 41 test-tier references, then re-run the proven extraction
(`git clone --single-branch --branch main` + `core.longpaths=true`, filter to
`api/Concertable.Payment/`) and require `dotnet build Concertable.Payment.slnx` to be fully green
including `Concertable.Payment.IntegrationTests`. That green build is stage 1's gate.

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

No implementation checkpoint (0–16) is complete.

## Verification

No build/test verification applies — the branch is docs-only. No `dotnet build`, unit, integration,
or E2E run is associated with this branch, and none is required for a design document.

## Reviews

A design review of the plan occurred (commit `3f8cb3494 docs(plan): resolve migration review
blockers` records the resolution). No review artifact for this plan exists under `reviews/`. Its
findings are recorded only as resolved via that commit; treat the specific findings as unknown beyond
"blockers resolved in the plan text."

No code review applies — there is no implementation to review yet.

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
- **Blocker/gate — plan is awaiting Tommy's review;** no checkpoint is authorized until he names one.
- **Deviation from staleness:** branch is 138 commits behind `origin/main`; plan inventory reflects
  baseline `d3c399ec8` and may have drifted.

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

## Resume prompt

```
cd C:\Users\TommySeery\source\repos\Concertable.worktrees\Plan\RepositoryPerMicroserviceMigration
Read AGENTS.md, plans/AGENTS.md, plans/REPOSITORY_PER_MICROSERVICE_MIGRATION.md, and plans/REPOSITORY_PER_MICROSERVICE_MIGRATION_PROGRESS.md, then do what the ledger's `## Next Steps` says.
```
