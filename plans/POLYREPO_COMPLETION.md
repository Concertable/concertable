# Polyrepo completion

The end goal is **separate per-service repos**. The hard part — making each service's deployable
closure build standalone from a package feed — is **already done** (the Service Build Separation
effort). What remains is staged deliberately so the **one-way door is taken last**:

1. **Buildable mirrors first** (Phases 1–4 below). Monorepo stays canonical; each `concertable-*`
   repo is auto-regenerated, read-only, and **clones + `dotnet build`s on its own**. Reversible,
   low-risk, delivers the entire separate-repos story.
2. **Then, only if/when the monorepo actually holds you back, cut to true polyrepo** — per service,
   at your own pace (see "Deferred: cut to true polyrepo"). Buildable-mirror state is a strict
   *prefix* of polyrepo, so this is reconfiguration (mostly *removing* the mirror sync), not a
   rewrite.

> **Why this order.** Repo-per-service's payoff is decoupling *separate teams*. Solo, that payoff is
> ~zero but the coordination tax (cross-repo PRs, shared-core publish→consume cycles, lost atomic
> changes) is full. The `UseLocalCore` hybrid exists *because* `Kernel`/`Messaging.*` co-change with
> the services constantly — and it stops working across repos (no sibling source on disk). So defer
> the irreversible cut until evidence demands it. See git history of this file's discussion if needed.

## Already done — do not re-derive (state as of this plan)

- Every service's **deployable closure** consumes shared platform + cross-service `*.Contracts` as
  `PackageReference`s from the org feed `https://nuget.pkg.github.com/Concertable`. Per-folder CPM /
  `nuget.config` / `Directory.Build.props` — **no** repo-root build config (deliberate; a carve takes
  only the service folder).
- **Carve CI gates** `carve-{auth,payment,search,b2b,customer}` in `.github/workflows/test.yml`
  `git archive` each folder and build it standalone from the feed. **Required** status checks.
- **Build-time guardrail** (`EnforceServiceBoundary` in each folder's `Directory.Build.targets`) fails
  the build on a deployable-closure `ProjectReference` escaping the folder (AppHost/Tests/UseLocalCore exempt).
- **`publish-packages.yml`** packs `IsPackable` projects (lockstep MinVer) to the org feed and a
  `verify-restore` job proves the published closure restores into a fresh consumer.
- **`mirror.yml`** subtree-splits `api/Concertable.B2B` → `thomasseery/concertable-b2b` and
  `Concertable.Customer` → `concertable-customer` on every push to `master`.
- **Slim bootable AppHost** exists for **B2B and Customer only**. Auth/Payment/Search have
  `*.AppHost.Extensions` (registration helpers) but **no standalone runnable host**.
- **Hybrid `UseLocalCore`** inner loop for the churny core (never committed `true`).

---

# Phase 0 — Naming + ownership cleanup (partly done)

**Done.** The umbrella repo was renamed `Concertable/Concertable` → **`Concertable/concertable`**
(lowercase, to match the `concertable-*` mirror convention). The org stays `Concertable`; the
package feed `https://nuget.pkg.github.com/Concertable` keys off the **org**, so it is unaffected.
Local `origin` remote updated to the lowercase URL; GitHub redirects all old URLs.

**Remaining cleanup — DONE.** The 8 `Directory.Build.props` (`api/Shared`,
`api/Concertable.{Messaging,ServiceDefaults,DataAccess,Auth.Contracts,B2B,Customer,Payment}`)
`<RepositoryUrl>`/`<PackageProjectUrl>` are now `https://github.com/Concertable/concertable`
(lowercased).

**RESOLVED — mirror repo ownership = `Concertable/concertable-*` (the org).** Keep everything under
the one org, consistent with the umbrella and the package feed. Correction to the prior note: the
`thomasseery/` mirrors were **not** stale drift — `ThomasSeery/concertable-b2b` and
`ThomasSeery/concertable-customer` actually exist (public, auto-synced, created 2026-06-01). Phase 4
therefore must **create the org repos and retire the personal ones**, not just create-from-scratch.
`mirror.yml`'s matrix + POLYREPO.md's table get repointed to `Concertable/concertable-*` in Phase 4
(not before — repointing the live workflow without the target repos existing would just fail the run).

---

# Phase 1 — Frictionless cross-repo restore (decide + wire feed auth)

**Problem.** The carve gates and `publish-packages.yml` restore using the monorepo Actions'
`GITHUB_TOKEN`. A *cloned mirror* has no such token, so today `dotnet build` in a fresh clone of a
mirror 401s (NU1301) against the private org feed. "Buildable mirror" means a stranger's clone
restores.

**RESOLVED — option C (keep the packages private; restore with a `read:packages` PAT).** nuget.org
(B) cannot host private packages, and this repo may go private, so public (A) is off the table —
publishing then can't be clawed back. C is the standard private-feed pattern (GitHub Packages /
Azure Artifacts + token in CI). "Buildable mirror" therefore means **clone + authenticate (PAT) +
build**, not "a stranger builds with nothing" — the correct trade for a possibly-private project.
The rejected options, for the record:
- ~~(A) Make the org feed's `Concertable.*` packages public.~~ Incompatible with going private.
- ~~(B) Republish to nuget.org.~~ nuget.org is public-only; same problem as A, plus a second target.

**Changes.** Mostly **already in place**: each service folder's `nuget.config` already maps
`Concertable.*` to the private feed `https://nuget.pkg.github.com/Concertable/index.json` with
`%GITHUB_PACKAGES_TOKEN%` creds, so the mirror carries working restore config with it. Remaining
work is to **document the PAT** a cloner needs (`read:packages`, exported as `GITHUB_PACKAGES_TOKEN`)
so the mirror is honestly buildable — a `nuget.config` comment and/or per-mirror README note.

**Verification gate — PASSED.**
- ✅ `dotnet build api/Concertable.slnx` green.
- Carve gates run in CI (unchanged here — this phase only touched feed *docs* + `nuget.config` comments).
- ✅ **Cross-repo proof:** carved `api/Concertable.B2B` into a clean dir with **no monorepo present**,
  built its 42-project deployable closure with only `GITHUB_PACKAGES_TOKEN` set to a `read:packages`
  PAT → 0 errors. The honest "clone + authenticate + build" is proven; the full clone proof over real
  mirror repos lands in Phase 4.

**Delivered:** PAT-scope note added to all 14 `nuget.config`; per-service mirror READMEs
(`api/Concertable.{B2B,Customer,Auth,Payment,Search}/README.md`) documenting read-only-mirror status
+ the exact standalone build command + the `read:packages` PAT.

# Phase 2 — Standalone bootable AppHost for Auth, Payment, Search

**Problem.** A mirror should not just *build* but *run* alone (the honest "this service runs
independently against its dependencies' contracts" demo, per `api/ARCHITECTURE.md` "The AppHost
problem"). B2B/Customer have a slim AppHost; Auth/Payment/Search have only `*.AppHost.Extensions`.

**Changes.** Add a slim `Concertable.{Auth,Payment,Search}.AppHost` per service that boots **only**
that service + its own infra (SQL, Azure Service Bus emulator), reusing the existing
`*.AppHost.Extensions` helpers and `Concertable.AppHost.Shared`. Adapter services (Auth/Payment) wire
their own infra; no sibling `WaitFor`. Where a data service needs another's events, register the
producer's `*.Seed.Simulator` (mirror world: as an `AddContainer` image — but for the monorepo-canonical
buildable-mirror stage, `AddProject` is still fine since the monorepo composes it).

**Delivered.** Added `Concertable.{Auth,Payment,Search}.AppHost` (csproj + Program.cs + appsettings +
launchSettings), composed via the existing `DistributedApplicationBuilderExtensions` helpers, and
registered them in `Concertable.slnx`. Notes on what the helpers dictated:
- **Auth is the universal adapter** — every host includes it (`AddPaymentWeb`/`AddSearchWeb` bake in
  `WaitFor(auth)`, matching `api/CLAUDE.md` "Auth present in every host"). So Payment and Search each
  boot Auth too; the "no sibling `WaitFor`" rule means no *data-service* sibling, which holds.
- **Search** is a data service: it boots the **B2B seed simulator** (`AddB2BSeedingSimulator`) to
  replay catalog events. Its Customer-origin rating events have no simulator yet — logged in
  `api/Concertable.Search/TECH_DEBT.md`.
- **Auth glob fix:** `Concertable.Auth.csproj` is a rooted `Sdk.Web` project, so it globbed the new
  nested AppHost sources (CS8802). Excluded the `Concertable.Auth.AppHost/` subtree from its default
  Compile/Content/None globs.

**Verification gate — PASSED.**
- ✅ `dotnet build api/Concertable.slnx` green; build-time guardrail green (AppHosts exempt — name
  contains `.AppHost`).
- ✅ Each new AppHost **boots standalone** (`dotnet run`) to healthy, Docker pre-flight green first:
  Auth `/health`→200 + OIDC discovery live; Payment = Auth + Payment.Web + Payment.Workers all
  `/health`→200; Search = Auth + Search.Web + Search.Workers all `/health`→200. Infra (SQL + ASB
  emulator) up and stable each time; no startup exceptions.
- ✅ Carve gates unaffected: `carve-auth` re-proven locally with the AppHost folder present (service
  csproj excludes it); `carve-payment`/`carve-search` use explicit project lists that omit AppHosts.
- Not a behavior change to covered flows → build + boot smoke was the gate; **E2E skipped**.

# Phase 3 — Shared-platform mirror

**Problem.** Mirrors of services reference shared packages, but the **shared source** (`Kernel`,
`Contracts`, `ServiceDefaults`, `AppHost.Shared`, `Messaging.*`, `Seed.Shared`, `Shared.*`) has no
repo of its own — so the separated world has no browsable/owning home for the platform.

**Changes.** Add a `Concertable/concertable-shared` (or split finer later) entry to `mirror.yml`'s
matrix for `api/Shared/` (and `api/Concertable.AppHost.Shared` if kept separate). The monorepo's
`publish-packages.yml` still publishes these — the shared mirror is *browsable/buildable* output only
at this stage; its own publish workflow is true-polyrepo work (deferred section).

**Delivered.** Added an `api/Shared → Concertable/concertable-shared` entry to `mirror.yml`'s matrix.
Scoped to `api/Shared` (Kernel, Contracts, Shared.*, Seed.*) only; the other shared infra libs
(`Messaging`, `ServiceDefaults`, `DataAccess`, `AppHost.Shared`, `Auth.Contracts`) are separate
top-level folders and a per-prefix `subtree split` can't bundle them — **finer splits deferred**.
The entry's target repo is created in Phase 4; until then it only runs on merge to `master`.

**Verification gate — PASSED.**
- ✅ `mirror.yml` YAML valid (parsed; 3 matrix entries resolve).
- ✅ No build impact (workflow-only change).

# Phase 4 — Create repos, wire secrets, first full mirror run + clone proof

**Problem.** Pull it together: real repos must exist and the auto-mirror must produce clones that
build.

**Decisions (resolved with Tommy):** mirrors are **public**, owner **Concertable** org, and the stale
personal `ThomasSeery/concertable-{b2b,customer}` get **deleted**. The monorepo itself is public, so
public mirrors are public→public (no exposure).

**Root cause (two bugs, found in sequence):** the mirror push failed for **two** independent reasons,
the first masking the second:
1. `MIRROR_PAT` secret didn't exist → push fell back to `GITHUB_TOKEN`, 403 as `github-actions[bot]`.
2. Even with `MIRROR_PAT` set, the push *still* 403'd as `github-actions[bot]` — `actions/checkout`
   persists the job token as `http.https://github.com/.extraheader`, and that Authorization header
   overrode the `x-access-token:MIRROR_PAT` creds in the push URL. Fixed with `persist-credentials:
   false` on the checkout step (the subtree split is local; the push carries its own PAT creds).

**Done (committed / executed) — Phase 4 COMPLETE:**
- ✅ `mirror.yml` matrix repointed to all six `Concertable/concertable-*` targets; POLYREPO.md table updated.
- ✅ The six **public** org repos created and now **populated** (empty=false, pushed 2026-06-30).
- ✅ **Org matrix landed on `master`** — `Feature/PolyrepoCompletion` merged (PR #70).
- ✅ **Mirror-checkout gitlink bug fixed** (`Fix/MirrorWorktreeGitlinks`, PR #72): three
  `.claude/worktrees/agent-*` gitlinks (no `.gitmodules`) made every run log
  `fatal: No url found for submodule path …`. Untracked them + gitignored `.claude/worktrees/`.
- ✅ **`MIRROR_PAT` secret set** on `Concertable/concertable` (Tommy, from his `gh` token).
- ✅ **`persist-credentials: false` fix** (same branch/PR #72) — the actual unblock for the cross-repo push.
- ✅ **Mirror run GREEN for all six** (run 28459700244 on the fix branch): `b2b, customer, auth, payment,
  search, shared` all `success`; all six repos `empty=false`.
- ✅ **Clone proof (structure)** — cloned `concertable-b2b` standalone (no monorepo present): full service
  tree + its own `Concertable.B2B.slnx`, `nuget.config`, `README.md`, `Directory.Build.props/targets`,
  `Directory.Packages.props`. A real, self-contained repo. The `dotnet build`-from-feed leg was proven
  green in Phase 1 (local B2B carve, 42-proj closure, 0 errors); repeating it against the real mirror
  only needs a `read:packages` PAT exported as `GITHUB_PACKAGES_TOKEN` (Phase 1 option C) — the `gh`
  token lacks that scope, so this last confirmation is optional/when convenient.

**Remaining — small, gated on Tommy:**
1. **Merge PR #72** so the `persist-credentials: false` + gitlink fixes land on `master`. ⚠️ Until then,
   the *auto*-mirror-on-master push event re-runs the **old** broken `mirror.yml` and 403s again (it
   won't lose the seeded content — a failed run pushes nothing — but auto-sync stays broken). The branch
   ref run already proved the fix; merging makes it the steady state. (Self-merge of the agent's own PR
   was blocked by the auto-mode classifier — Tommy merges.)
2. **Delete the personal mirrors:** `gh auth refresh -h github.com -s delete_repo` then
   `gh repo delete ThomasSeery/concertable-b2b --yes` / `…-customer --yes` (session token lacks
   `delete_repo`). Both empty — pure cleanup.

**UI E2E: judgment-skip.** Mirror/CI-config + `.gitignore` only; no runtime behavior on any covered
flow. Build + the six carve gates + all unit/integration are green on PR #72. Doesn't meet the
massive/risky bar.

**On completion of Phase 4, buildable mirrors are done.** Update `plans/POLYREPO.md` (its "Deferred:
make mirrors clone-and-build" section is now realised — trim it to a pointer or fold its live bits
in), and `git rm` this plan **only when** the deferred section below is also abandoned or completed.
If true polyrepo is still wanted, keep this file with Phases 1–4 struck and the section below live.

---

# Deferred: cut to true polyrepo (per-service, one-way door)

Do **not** start until the monorepo demonstrably holds you back. Buildable-mirror state already gives
separate, building repos; this only changes **where you commit**. Migrate **service-by-service**;
split the services that co-change most with the core **last**.

**Prerequisite — stand up `concertable-shared` as a real publishing repo first.** Give the shared
mirror its own `publish-packages.yml` (per-repo MinVer → independent versioning, which the monorepo
workflow comment notes comes "for free"). Until shared publishes independently, no consumer can leave,
because leaving means consuming shared from the feed without the monorepo republishing it.

**Per service, to promote a mirror from generated → canonical:**
1. **Remove it from `mirror.yml`'s matrix.** ⚠️ Footgun: if left in, the next `master` push
   force-pushes over your new commits. This is the single irreversible-feeling step — do it first.
2. **Promote the mirror to canonical** — its last mirrored history *is* the starting point; begin
   committing directly in the repo.
3. **Per-repo CI** — copy the relevant slice of `test.yml` (its carve job *becomes* the repo's normal
   build) into the repo; add a per-repo `publish-packages.yml` for any packages it owns (its
   `*.Contracts`, `*.Seed.Contracts`).
4. **Cross-repo restore auth** — already solved by Phase 1's feed model; confirm the repo's CI token
   can read the feed.

**One-time, shared across all services:**
- **Umbrella `Concertable.AppHost`** can no longer reference service *projects* it no longer contains
  → compose **container images** (`AddContainer`) instead, or retire it in favour of per-service
  hosts. (`api/ARCHITECTURE.md` "The AppHost problem".)
- **Full-stack E2E / integration harnesses** boot multiple services together and are *exempt* from the
  package boundary (they reference siblings) → they don't survive a split. Move to a **containerised
  E2E topology** (pull each service's published image) and decide their home repo.
- **`*.Seed.Simulator`s** ship as **container images** referenced via `AddContainer` in dependent
  hosts (vs. `AddProject` today).
- **`UseLocalCore`** stays useful right up until a service leaves; once a service is its own repo,
  cross-core changes for it go through the shared package release flow.

**Suggested order:** `concertable-shared` (publishing) → adapter services (`Auth`, `Payment`, `Search`
— they don't consume sibling data-service contracts heavily) → finally the churny data services
(`B2B`, `Customer`) once the shared release loop is comfortable.
