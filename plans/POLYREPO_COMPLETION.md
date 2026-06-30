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

**Remaining cleanup (do on a branch — these are build config, not docs):**
- 8 `Directory.Build.props` (`api/Shared`, `api/Concertable.{Messaging,ServiceDefaults,DataAccess,
  Auth.Contracts,B2B,Customer,Payment}`) carry `<RepositoryUrl>`/`<PackageProjectUrl>` =
  `https://github.com/Concertable/Concertable` → lowercase to `…/concertable`. Ships in package
  metadata; redirects keep it resolving until then, so not urgent.

**OPEN DECISION — mirror repo ownership (blocks Phase 4, resolve before wiring mirrors).**
`mirror.yml` and POLYREPO.md reference mirror targets under **`thomasseery/`** (personal account:
`thomasseery/concertable-b2b`, `-customer`, `-contracts`) while the umbrella now lives under the
**`Concertable` org**. Decide which owner the mirrors live under:
- **(recommended) `Concertable/concertable-*`** — keep everything under the one org; consistent with
  the umbrella and the package feed.
- **`thomasseery/concertable-*`** — mirrors on the personal account (only if deliberate).

The mirror repos appear **not to exist yet** (POLYREPO.md "one-time setup" lists creating them as a
TODO), so `thomasseery/` is likely just stale drift. Once decided, update `mirror.yml`'s matrix +
POLYREPO.md's table + this plan's Phase 3/4 references to the chosen owner, then create the repos.
**Until then, do not change `mirror.yml` targets.**

---

# Phase 1 — Frictionless cross-repo restore (decide + wire feed auth)

**Problem.** The carve gates and `publish-packages.yml` restore using the monorepo Actions'
`GITHUB_TOKEN`. A *cloned mirror* has no such token, so today `dotnet build` in a fresh clone of a
mirror 401s (NU1301) against the private org feed. "Buildable mirror" means a stranger's clone
restores.

**Decision (pick one, record it here when chosen):**
- **(A — recommended) Make the org feed's `Concertable.*` packages public.** GitHub Packages can be
  set public per-package (or org default). A clone then restores with **no token** — true
  "clone-and-build just works". Public packages under your name are also a portfolio plus.
- **(B) Republish to nuget.org.** Maximally frictionless + public, but a second publish target to
  maintain. Only if you want the nuget.org presence specifically.
- **(C) Keep private; ship a `nuget.config` + documented `read:packages` PAT in each mirror.**
  Bulletproof but a cloner needs a token — reads worse to a reviewer. Fallback only.

**Changes.** Apply the chosen option's feed config. Whichever is chosen, ensure each service folder's
existing `nuget.config` references the resulting public/auth’d feed so the *mirror* (which is just
that folder) carries working restore config with it.

**Verification gate.**
- `dotnet build api/Concertable.slnx` green.
- Carve gates still green in CI.
- **Cross-repo proof:** in a clean directory with **no monorepo present** and (for A/B) **no token**,
  `git archive HEAD:api/Concertable.B2B | tar -x` into it, `dotnet restore` + `dotnet build` the
  deployable closure → succeeds. (This is the carve gate minus the monorepo's token — script it
  locally; it becomes the real test once mirrors exist in Phase 4.)

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

**Verification gate.**
- `dotnet build api/Concertable.slnx` green; build-time guardrail green (AppHosts are exempt from the
  package boundary, so cross-folder refs here are allowed by design).
- Each new AppHost **boots standalone** (`dotnet run`) to a healthy state with its infra.
- Carve gates still green (AppHosts aren't in the deployable closure, so this must not regress them).
- Not a behavior change to covered flows → build + boot smoke is the gate; **skip E2E**.

# Phase 3 — Shared-platform mirror

**Problem.** Mirrors of services reference shared packages, but the **shared source** (`Kernel`,
`Contracts`, `ServiceDefaults`, `AppHost.Shared`, `Messaging.*`, `Seed.Shared`, `Shared.*`) has no
repo of its own — so the separated world has no browsable/owning home for the platform.

**Changes.** Add a `thomasseery/concertable-shared` (or split finer later) entry to `mirror.yml`'s
matrix for `api/Shared/` (and `api/Concertable.AppHost.Shared` if kept separate). The monorepo's
`publish-packages.yml` still publishes these — the shared mirror is *browsable/buildable* output only
at this stage; its own publish workflow is true-polyrepo work (deferred section).

**Verification gate.**
- `mirror.yml` YAML valid (Actions lint / `workflow_dispatch` dry intent).
- No build impact (workflow-only change) — `dotnet build` unaffected.

# Phase 4 — Create repos, wire secrets, first full mirror run + clone proof

**Problem.** Pull it together: real repos must exist and the auto-mirror must produce clones that
build.

**Changes.**
1. Extend `mirror.yml` matrix to **all** mirrored targets: `concertable-auth`, `-payment`, `-search`
   (joining `-b2b`, `-customer`) + `-shared` from Phase 3.
2. Create the empty GitHub repos (no README/license), default branch `master`.
3. Ensure `MIRROR_PAT` (or fine-grained equivalent) can push to all of them; confirm the
   `MIRROR_PAT` secret is set on the monorepo.
4. Trigger the mirror run (push to `master` or `workflow_dispatch`).

**Verification gate (the real one for this whole plan):**
- Mirror workflow green for every matrix entry.
- **Clone proof:** `git clone` each service mirror into a clean checkout with **no monorepo present**,
  then `dotnet build` (and for one service, `dotnet run` its AppHost) → succeeds, restoring from the
  feed per Phase 1's chosen auth model.
- This is a broadly cross-cutting milestone → **run the UI E2E suite** (`e2e-ui-debug`, Docker
  pre-flight first) once against the monorepo to confirm nothing in the AppHost/feed reshuffling
  regressed runtime, since Phase 2 added new hosts.

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
